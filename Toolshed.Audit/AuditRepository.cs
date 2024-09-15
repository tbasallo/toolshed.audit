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
    public AuditRepository() : base(ServiceManager.ConnectionString)
    {

    }

    public static async Task<AuditDeletion?> GetDeleteActivity(string partitionKey, string rowKey)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditDeletions()).GetEntityWhenExistsAsync<AuditDeletion>(partitionKey, rowKey);;
    }




    public static async Task<Page<AuditActivity>> GetAuditActivity(string entityType, object entityId, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivities()).GetEntitiesAsync<AuditActivity>(QueryHelper.GetPartitionKey(entityType, entityId), pageSize, continuationToken);
    }
    public static async Task<Page<AuditActivity>> GetAuditActivity(string partitionKey, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivities()).GetEntitiesAsync<AuditActivity>(partitionKey, pageSize, continuationToken);
    }
    public static async Task<Page<AuditActivityHistory>> GetAuditActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditActivities()).GetEntitiesAsync<AuditActivityHistory>(date.ToString("yyyyMMdd"), pageSize, continuationToken);
    }
    public static async Task<Page<AuditDeletion>> GetDeleteActivity(int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditDeletions()).GetEntitiesAsync<AuditDeletion>(pageSize, continuationToken);
    }

    /// <summary>
    /// Returns a all of a users activities, including login and permission activities
    /// </summary>
    public static async Task<Page<AuditUserActivity>> GetUserActivity(string userId, int pageSize = 1000, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditUsers()).GetEntitiesAsync<AuditUserActivity>(userId, pageSize, continuationToken);
    }


    /// <summary>
    /// Returns a users login and permission activities, nothing else
    /// </summary>
    public static async Task<Page<AuditUserActivity>> GetUserLoginActivity(string userId, int pageSize = 100, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditUserLogins()).GetEntitiesAsync<AuditUserActivity>(userId, pageSize, continuationToken);
    }

    /// <summary>
    /// Returns all permission exceptions for the specified date
    /// </summary>
    public static async Task<Page<AuditPermissionActivity>> GetLoginActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditLogins()).GetEntitiesAsync<AuditPermissionActivity>(date.ToString("yyyyMM"), pageSize, continuationToken);
    }

    /// <summary>
    /// Returns all permission exceptions for the specified date
    /// </summary>
    public static async Task<Page<AuditPermissionActivity>> GetPermissionExceptionActivity(DateTime date, int pageSize = 500, string? continuationToken = null)
    {
        return await ServiceManager.GetTableClient(TableAssist.AuditPermissions()).GetEntitiesAsync<AuditPermissionActivity>(date.ToString("yyyyMM"), pageSize, continuationToken);
    }
}
