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
    public async Task Delete(string partitionkeyStartsWith, DateTimeOffset maxDateToDelete)
    {

        var tc = ServiceManager.GetTableClient(TableAssist.AuditActivities());
        var history = ServiceManager.GetTableClient(TableAssist.AuditActivityHistories());
        var userTable = ServiceManager.GetTableClient(TableAssist.AuditUsers());
        var deletionsTable = ServiceManager.GetTableClient(TableAssist.AuditDeletions());

        string filter = $"PartitionKey ge '{partitionkeyStartsWith}'";// and PartitionKey le 'o'";
        var allData = tc.Query<AuditActivity>(filter: filter).ToList();

        var users = new ConcurrentBag<Tuple<string, string, string>>();


        // Parallelize audit activity deletions
        Parallel.ForEach(allData, item =>
        {
            if (item.PartitionKey.StartsWith(partitionkeyStartsWith) && item.On < maxDateToDelete)
            {
                //delete the activity
                tc.DeleteEntity(item.PartitionKey, item.RowKey);
                //delete the history
                history.DeleteEntity(item.On.ToString("yyyyMMdd"), $"{item.PartitionKey}_{item.RowKey}");
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
                    await tc.DeleteEntityAsync(match.PartitionKey, match.RowKey);
                }
            }
        }));
    }
}
