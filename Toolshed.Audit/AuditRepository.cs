using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolshed.AzureStorage;

namespace Toolshed.Audit;

/// <summary>
/// Repository for querying against the audit tables. No modifications occur here, only querying
/// </summary>
public class AuditRepository : AzureStorageBaseService
{
    /// <summary>
    /// Queries the tables of the default queue
    /// </summary>
    public AuditRepository() : this(null)
    {
    }

    /// <summary>
    /// Queries the tables using the table prefix registered for the queue name. Falls back to the default prefix when the queue is null or not registered.
    /// </summary>
    public AuditRepository(string? queueName) : base(ServiceManager.ConnectionString)
    {
        QueueName = string.IsNullOrWhiteSpace(queueName) ? ServiceManager.QueueName : queueName;
        TablePrefix = ServiceManager.GetTablePrefix(queueName);
    }

    public string QueueName { get; }
    public string? TablePrefix { get; }

    public async Task<AuditDeletion?> GetDeleteActivity(string partitionKey, string rowKey)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditDeletions(TablePrefix)).GetEntityWhenExistsAsync<AuditDeletion>(partitionKey, rowKey);
    }




    public async Task<Page<AuditActivity>> GetAuditActivity(string entityType, object entityId, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivities(TablePrefix)).GetEntitiesAsync<AuditActivity>(QueryHelper.GetPartitionKey(entityType, entityId), pageSize, continuationToken);
    }
    public async Task<Page<AuditActivity>> GetAuditActivity(string partitionKey, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivities(TablePrefix)).GetEntitiesAsync<AuditActivity>(partitionKey, pageSize, continuationToken);
    }
    public async Task<Page<AuditActivityHistory>> GetAuditActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivityHistories(TablePrefix)).GetEntitiesAsync<AuditActivityHistory>(date.ToString("yyyyMMdd"), pageSize, continuationToken);
    }
    public async Task<Page<AuditDeletion>> GetDeleteActivity(int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditDeletions(TablePrefix)).GetEntitiesAsync<AuditDeletion>(pageSize, continuationToken);
    }

    /// <summary>
    /// Returns a all of a users activities, including login and permission activities
    /// </summary>
    public async Task<Page<AuditUserActivity>> GetUserActivity(string userId, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditUsers(TablePrefix)).GetEntitiesAsync<AuditUserActivity>(userId, pageSize, continuationToken);
    }


    /// <summary>
    /// Returns a users login and permission activities, nothing else
    /// </summary>
    public async Task<Page<AuditUserActivity>> GetUserLoginActivity(string userId, int pageSize = 100, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditUserLogins(TablePrefix)).GetEntitiesAsync<AuditUserActivity>(userId, pageSize, continuationToken);
    }

    /// <summary>
    /// Returns all login activities for the specified month
    /// </summary>
    public async Task<Page<AuditLoginActivity>> GetLoginActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditLogins(TablePrefix)).GetEntitiesAsync<AuditLoginActivity>(date.ToString("yyyyMM"), pageSize, continuationToken);
    }

    /// <summary>
    /// Returns all permission exceptions for the specified date
    /// </summary>
    public async Task<Page<AuditPermissionActivity>> GetPermissionExceptionActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditPermissions(TablePrefix)).GetEntitiesAsync<AuditPermissionActivity>(date.ToString("yyyyMM"), pageSize, continuationToken);
    }
}
