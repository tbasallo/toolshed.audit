using System.Threading.Tasks;

namespace Toolshed.Audit;

/// <summary>
/// Manages the processing of queued items into the respective tables
/// </summary>
public class AuditManager
{
    public static async Task AddActivity(AuditActivity auditActivity)
    {
        switch (auditActivity.GetActivityType())
        {
            case AuditActivityType.Login:
            case AuditActivityType.Heartbeat:
                await ProcessLogin(auditActivity);
                break;
            case AuditActivityType.Permission:
                await ProcessPermissionIssue(auditActivity);
                break;
            case AuditActivityType.Create:
            case AuditActivityType.Update:
            case AuditActivityType.Delete:
            case AuditActivityType.Info:
            case AuditActivityType.Access:
            default:
                await ProcessActivity(auditActivity);
                break;
        }
    }

    static async Task ProcessActivity(AuditActivity auditActivity)
    {
        var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
        {
            AuditType = auditActivity.AuditType,
            EntityPartitionKey = auditActivity.PartitionKey,
            EntityRowKey = auditActivity.RowKey,
            EntityType = auditActivity.EntityType,
            EntityId = auditActivity.EntityId
        };
        var auditUserHistoryActivity = new AuditUserHistoryActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName);
        var activityHistory = new AuditActivityHistory(TimeZoneHelper.GetDate(auditActivity.On), auditActivity.ById, auditActivity.ByName, auditActivity.PartitionKey, auditActivity.RowKey)
        {
            EntityType = auditActivity.EntityType,
            EntityId = auditActivity.EntityId,
            On = auditActivity.On,
            AuditType = auditActivity.AuditType
        };

        await ServiceManager.GetTableClient(TableAssist.AuditActivities()).UpsertEntityAsync(auditActivity);
        await ServiceManager.GetTableClient(TableAssist.AuditUsers()).UpsertEntityAsync(activityUser);
        await ServiceManager.GetTableClient(TableAssist.AuditUsers()).UpsertEntityAsync(auditUserHistoryActivity);
        await ServiceManager.GetTableClient(TableAssist.AuditActivityHistories()).UpsertEntityAsync(activityHistory);

        if (auditActivity.GetActivityType() == AuditActivityType.Delete)
        {
            var deletion = new AuditDeletion(auditActivity.EntityType, auditActivity.EntityId)
            {
                Entity = auditActivity.Entity,
                ById = auditActivity.ById,
                ByName = auditActivity.ByName,
                On = auditActivity.On
            };
            await ServiceManager.GetTableClient(TableAssist.AuditDeletions()).UpsertEntityAsync(deletion);                
        };
    }

    static async Task ProcessLogin(AuditActivity auditActivity)
    {
        var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
        {
            AuditType = auditActivity.AuditType,
            ExtraInfo = auditActivity.Description
        };

        if (auditActivity.Entity != null && bool.TryParse(auditActivity.Entity, out bool isSuccess))
        {
            activityUser.IsSuccessful = isSuccess;
        }

        await ServiceManager.GetTableClient(TableAssist.AuditUsers()).UpsertEntityAsync(activityUser);
        await ServiceManager.GetTableClient(TableAssist.AuditUserLogins()).UpsertEntityAsync(activityUser);            
        var activityLogin = new AuditLoginActivity(auditActivity.On.DateTime, auditActivity.ById, auditActivity.ByName)
        {
            ExtraInfo = auditActivity.Description
        };
        await ServiceManager.GetTableClient(TableAssist.AuditPermissions()).UpsertEntityAsync(activityLogin);
    }

    static async Task ProcessPermissionIssue(AuditActivity auditActivity)
    {
        var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
        {
            AuditType = auditActivity.AuditType,
            ExtraInfo = auditActivity.Description
        };

        await ServiceManager.GetTableClient(TableAssist.AuditUsers()).UpsertEntityAsync(activityUser);
        await ServiceManager.GetTableClient(TableAssist.AuditUserLogins()).UpsertEntityAsync(activityUser);
        
        var activityPermission = new AuditPermissionActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName, auditActivity.EntityType, auditActivity.EntityId)
        {
            ExtraInfo = auditActivity.Description
        };
        await ServiceManager.GetTableClient(TableAssist.AuditPermissions()).UpsertEntityAsync(activityPermission);            
    }

}
