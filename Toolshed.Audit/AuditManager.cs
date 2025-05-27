using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toolshed.Audit;

/// <summary>
/// Manages the processing of queued items into the respective tables
/// </summary>
public class AuditManager
{
    public static async Task AddActivity(AuditActivity auditActivity)
    {
        if (auditActivity.AuditType == "Login" || auditActivity.AuditType == "Heartbeat")
        {
            await ProcessLogin(auditActivity);
        }
        else if (auditActivity.AuditType == "Permission")
        {
            await ProcessPermissionIssue(auditActivity);
        }
        else
        {
            await ProcessActivity(auditActivity);
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

        var related = auditActivity.GetRelated();
        if (related.Count > 0)
        {
            foreach (var relatedActivity in related)
            {
                var relatedAuditActivity = new AuditActivity(relatedActivity.EntityType, relatedActivity.EntityId, auditActivity.AuditType)
                {
                    ById = auditActivity.ById,
                    ByName = auditActivity.ByName,
                    On = auditActivity.On,
                    Description = relatedActivity.Description ?? auditActivity.Description,
                    Related = (new List<RelatedEntity> { new(auditActivity.EntityType, auditActivity.EntityId) }).ToJson(), // this is the main entity related to the activity, so we can link back to it
                    //Entity = auditActivity.Entity, -- this entry is meant ot be informational for related events, not the main entity                    
                    //Changes = auditActivity.Changes,-- this doesn't apply here, since it's the child
                    //Related = auditActivity.Related - this would be dumb
                };
                await ServiceManager.GetTableClient(TableAssist.AuditActivities()).UpsertEntityAsync(relatedAuditActivity);
            }
        }

        if (auditActivity.AuditType == AuditActivityType.Delete)
        {
            var deletion = new AuditDeletion(auditActivity.EntityType, auditActivity.EntityId)
            {
                Entity = auditActivity.Entity,
                ById = auditActivity.ById,
                ByName = auditActivity.ByName,
                On = auditActivity.On
            };
            await ServiceManager.GetTableClient(TableAssist.AuditDeletions()).UpsertEntityAsync(deletion);
        }        
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
