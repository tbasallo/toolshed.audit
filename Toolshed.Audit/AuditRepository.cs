using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Microsoft.Azure.Cosmos.Table.Queryable;
using System;

namespace Toolshed.Audit
{
    /// <summary>
    /// Repository for querying against the audit tables. No modifications occur here, only querying
    /// </summary>
    public class AuditRepository
    {
        public AuditRepository()
        {

        }

        public async Task<List<AuditDeletion>> GetDeleteActivity(int pageCount = 1, int pageSize = 500)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditDeletions());
            var query = new TableQuery<AuditDeletion>();

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
        public async Task<AuditDeletion> GetDeleteActivity(string partitionkey, string rowKey)
        {
            var o = TableOperation.Retrieve<AuditDeletion>(partitionkey, rowKey);
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditDeletions());
            var d = await t.ExecuteAsync(o);
            return d.Result as AuditDeletion;
        }

        public async Task<List<AuditActivityHistory>> GetAuditActivity(int pageCount = 1, int pageSize = 500)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories());
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
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories());
            var query = new TableQuery<AuditActivityHistory>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMMdd")));

            if(pageCount > 1)
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
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivities());
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
        public async Task<List<AuditUserActivity>> GetUserActivity(string userId, int pageCount = 1, int pageSize = 500)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers());
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
        public async Task<List<AuditUserActivity>> GetAllUsersActivity(int pageCount = 1, int pageSize = 500)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers());
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
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUserLogins());
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
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditLogins());
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
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditPermissions());
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
    }
}
