using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolshed.Audit
{
    /// <summary>
    /// Repository for querying against the audit tables. No modifications occur here, only querying
    /// </summary>
    public class AuditRepository : AzureStorageBaseService
    {
        public AuditRepository() : base(ServiceManager.ConnectionKey)
        {

        }


        public Task<TableQuerySegment<T>> GetSegmentedData<T>(CloudTable table, string partitionKey, int pageSize = 500, TableContinuationToken tableContinuationToken = null) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return table.ExecuteQuerySegmentedAsync<T>(query.Take(pageSize), tableContinuationToken);
        }
        public Task<TableQuerySegment<T>> GetSegmentedData<T>(CloudTable table, int pageSize = 500, TableContinuationToken tableContinuationToken = null) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();
            return table.ExecuteQuerySegmentedAsync<T>(query.Take(pageSize), tableContinuationToken);
        }


        public Task<TableQuerySegment<AuditDeletion>> GetDeleteActivity(int pageSize = 500, TableContinuationToken tableContinuationToken = null)
        {
            return GetSegmentedData<AuditDeletion>(ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditDeletions()), pageSize, tableContinuationToken);
        }
        public async Task<AuditDeletion> GetDeleteActivity(string partitionkey, string rowKey)
        {
            var o = TableOperation.Retrieve<AuditDeletion>(partitionkey, rowKey);
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditDeletions());
            var d = await t.ExecuteAsync(o);
            return d.Result as AuditDeletion;
        }

        public async Task<List<AuditActivityHistory>> GetAuditActivity(int pageCount = 1, int pageSize = 500)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories());
            var query = new TableQuery<AuditActivityHistory>();

            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }
        public async Task<List<AuditActivityHistory>> GetAuditActivity(DateTime date, int pageCount = 1, int pageSize = 500)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories());
            var query = new TableQuery<AuditActivityHistory>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMMdd")));

            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }
        public async Task<List<AuditActivity>> GetAuditActivity(string entityType, object entityId, int pageCount = 1, int pageSize = 1000)
        {
            return await GetAuditActivity(QueryHelper.GetPartitionKey(entityType, entityId), pageCount, pageSize);
        }
        public async Task<List<AuditActivity>> GetAuditActivity(string partitionkey, int pageCount = 1, int pageSize = 1000)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditActivities());
            var query = new TableQuery<AuditActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionkey));
            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }

        /// <summary>
        /// Returns a all of a users activities, including login and permission activities
        /// </summary>
        public Task<TableQuerySegment<AuditUserActivity>> GetUserActivity(string userId, int pageSize = 500, TableContinuationToken tableContinuationToken = null)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditUsers());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            return t.ExecuteQuerySegmentedAsync(query.Take(pageSize), tableContinuationToken);
        }
        public async Task<List<AuditUserActivity>> GetUserActivity(string userId)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditUsers());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

            var segment = await t.ExecuteQuerySegmentedAsync(query, null);

            var model = new List<AuditUserActivity>();

            if (segment.Results != null)
            {
                model.AddRange(segment.Results.ToList());
            }

            while (segment.ContinuationToken != null)
            {
                segment = await t.ExecuteQuerySegmentedAsync(query, segment.ContinuationToken);
                model.AddRange(segment.Results.ToList());
            }

            return model;
        }
        public async Task<List<AuditUserActivity>> GetAllUsersActivity(int pageCount = 1, int pageSize = 500)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditUsers());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "USER"));
            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }


        /// <summary>
        /// Returns a users login and permission activities, nothing else
        /// </summary>
        public async Task<List<AuditUserActivity>> GetUserLoginActivity(string userId, int pageCount = 1, int pageSize = 500)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditUserLogins());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }

        /// <summary>
        /// Returns all permission exceptions for the specified date
        /// </summary>
        public async Task<List<AuditPermissionActivity>> GetLoginActivity(DateTime date, int pageCount = 1, int pageSize = 500)
        {
            var t = ServiceManager.GetTableClient().GetTableReference(TableAssist.AuditLogins());
            var query = new TableQuery<AuditPermissionActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMM")));
            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }

        /// <summary>
        /// Returns all permission exceptions for the specified date
        /// </summary>
        public async Task<List<AuditPermissionActivity>> GetPermissionExceptionActivity(DateTime date, int pageCount = 1, int pageSize = 500)
        {
            var t = GetTableClient(TableAssist.AuditPermissions());
            var query = t.QueryAsync<AuditPermissionActivity>(x => x.PartitionKey == date.ToString("yyyyMM"));


            var p = t.QueryAsync<AuditPermissionActivity>(x => x.PartitionKey == "", maxPerPage: pageSize);
            await foreach (var secret in p.(pageSize))
            {
                Console.WriteLine($"TakeAsync: {secret.Name}");
            }

            //https://stackoverflow.com/questions/68772240/how-to-filter-the-query-result-for-pagination-in-tableclient-queryasync-azure




            //var query = new TableQuery<AuditPermissionActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMM")));
            if (pageCount > 1)
            {
                query = query.Take(pageSize).Skip((pageCount - 1) * pageSize).AsTableQuery();
            }
            else
            {
                query = query.Take(pageSize);
            }

            return (await t.ExecuteQuerySegmentedAsync(query, null)).Results;
        }
    }
}
