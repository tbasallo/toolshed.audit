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

        public async Task<AuditDeletion> GetDeleteActivity(string partitionkey, string rowKey)
        {
            var o = TableOperation.Retrieve(partitionkey, rowKey);
            return (await AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditDeletions()).ExecuteAsync(o)).Result as AuditDeletion;
        }
        public async Task<List<AuditActivityHistory>> GetAuditActivity(DateTime date, int pageCount = 1, int pageSize = 500)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories());
            var query = new TableQuery<AuditActivityHistory>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMMdd")));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }
        public async Task<List<AuditActivity>> GetAuditActivity(string entityType, object entityId, int pageCount = 1, int pageSize = 250)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivities());
            var query = new TableQuery<AuditActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, QueryHelper.GetPartitionKey(entityType, entityId)));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }

        /// <summary>
        /// Returns a all of a users activities, including login and permission activities
        /// </summary>
        public async Task<List<AuditUserActivity>> GetUserActivity(string userId, int pageCount = 1, int pageSize = 250)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }

        /// <summary>
        /// Returns a users login and permission activities, nothing else
        /// </summary>
        public async Task<List<AuditUserActivity>> GetUserLoginActivity(string userId, int pageCount = 1, int pageSize = 250)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUserLogins());
            var query = new TableQuery<AuditUserActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }

        /// <summary>
        /// Returns all permission exceptions for the specified date
        /// </summary>
        public async Task<List<AuditPermissionActivity>> GetLoginActivity(DateTime date, int pageCount = 1, int pageSize = 250)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditLogins());
            var query = new TableQuery<AuditPermissionActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMM")));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }

        /// <summary>
        /// Returns all permission exceptions for the specified date
        /// </summary>
        public async Task<List<AuditPermissionActivity>> GetPermissionExceptionActivity(DateTime date, int pageCount = 1, int pageSize = 250)
        {
            var t = AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditPermissions());
            var query = new TableQuery<AuditPermissionActivity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMM")));
            var segment = await t.ExecuteQuerySegmentedAsync(query.Skip((pageCount - 1) * pageSize).Take(pageSize).AsTableQuery(), null);

            return segment.Results;
        }
    }
}
