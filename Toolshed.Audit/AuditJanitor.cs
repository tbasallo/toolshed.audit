using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolshed.Audit;


//cleans up the messes
internal class AuditJanitor
{
    public async Task Delete(string partitionkeyStartsWith, DateTimeOffset maxDateToDelete, string? queueName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(partitionkeyStartsWith);

        var prefix = ServiceManager.GetTablePrefix(queueName);
        var tc = ServiceManager.GetTableClient(TableAssist.AuditActivities(prefix));
        var history = ServiceManager.GetTableClient(TableAssist.AuditActivityHistories(prefix));
        var userTable = ServiceManager.GetTableClient(TableAssist.AuditUsers(prefix));

        var upperBound = partitionkeyStartsWith[..^1] + (char)(partitionkeyStartsWith[^1] + 1);
        string filter = $"PartitionKey ge '{Escape(partitionkeyStartsWith)}' and PartitionKey lt '{Escape(upperBound)}'";

        var users = new ConcurrentBag<Tuple<string, string, string>>();

        await Parallel.ForEachAsync(tc.QueryAsync<AuditActivity>(filter: filter), new ParallelOptions { MaxDegreeOfParallelism = 16 }, async (item, ct) =>
        {
            if (item.PartitionKey.StartsWith(partitionkeyStartsWith, StringComparison.Ordinal) && item.On < maxDateToDelete)
            {
                //delete the activity
                await tc.DeleteEntityAsync(item.PartitionKey, item.RowKey, cancellationToken: ct);
                //delete the history - partition uses the same time zone conversion as when it was written
                await history.DeleteEntityAsync(TimeZoneHelper.GetDate(item.On).ToString("yyyyMMdd"), $"{item.PartitionKey}_{item.RowKey}", cancellationToken: ct);
                //add the user info to delete later
                //we can't do it now because we don't have the rowkey and we would have to load all the data for the user.
                //In theory we could use the ticks to determine the rowkey, but....
                //we need to try it and see if it works
                //TODO find rowkey using ticks
                users.Add(new Tuple<string, string, string>(item.ById, item.PartitionKey, item.RowKey));
            }
        });

        // USER DELETIONS (parallelize per user group)
        await Task.WhenAll(users.GroupBy(x => x.Item1).Select(async user =>
        {
            var userItems = await userTable.GetEntitiesAsync<Toolshed.Audit.AuditUserActivity>(user.Key);
            foreach (var item in user)
            {
                var match = userItems.FirstOrDefault(x => x.EntityPartitionKey == item.Item2 && x.EntityRowKey == item.Item3);
                if (match is not null)
                {
                    await userTable.DeleteEntityAsync(match.PartitionKey, match.RowKey);
                }
            }
        }));
    }

    static string Escape(string value) => value.Replace("'", "''");
}
