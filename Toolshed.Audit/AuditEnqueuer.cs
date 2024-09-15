using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace Toolshed.Audit;

/// <summary>
/// Used to queue up audit items for processing asynchronously
/// </summary>
public class AuditEnqueuer
{
    public AuditEnqueuer()
    {
        if (string.IsNullOrWhiteSpace(ServiceManager.QueueName))
        {
            throw new ArgumentNullException(nameof(ServiceManager.QueueName), "The queue name must be set in the settings or in the constructor for AuditManager");
        }

        AuditQueue = new QueueClient(ServiceManager.ConnectionString, ServiceManager.QueueName);

    }
    public AuditEnqueuer(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName) && string.IsNullOrWhiteSpace(ServiceManager.QueueName))
        {
            throw new ArgumentNullException(nameof(queueName), "The queue name must be set in the settings or in the constructor for AuditManager");
        }

        AuditQueue = new QueueClient(ServiceManager.ConnectionString, queueName ?? ServiceManager.QueueName);
    }

    private QueueClient AuditQueue { get; }

    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, null, null, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, string auditDescription)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, null, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, string auditDescription, List<RelatedEntity> related)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, null, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, string auditDescription, List<PropertyComparison> changes)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, changes, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, string auditDescription, List<RelatedEntity> related, List<PropertyComparison> changes)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, changes, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, List<RelatedEntity> related)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, related, null, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, List<PropertyComparison> changes)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, null, changes, default(object));
    }
    public async Task Enqueue(string entityType, object entityId, string type, object userId, string userName, List<RelatedEntity> related, List<PropertyComparison> changes)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, related, changes, default(object));
    }


    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, T entity)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, null, null, entity);
    }
    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, string auditDescription, T entity)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, null, entity);

    }
    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, List<RelatedEntity> related, T entity)
    {
        await Enqueue(entityType, entityId, type, userId, userName, null, related, null, entity);
    }
    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, string auditDescription, List<RelatedEntity> related, T entity)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, null, entity);
    }
    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, string auditDescription, List<PropertyComparison> changes, T entity)
    {
        await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, changes, entity);
    }


    //FOR NOW - THERE's NO OPTION WITH changes AND entity (as a shortcut) because this is a very uncommon case - if you're sending the entity, it's either new or deleted, but it wouldn't have changes
    //if you really want that last combo - use the full method call


    public async Task Enqueue<T>(string entityType, object entityId, string type, object userId, string userName, string? auditDescription, List<RelatedEntity>? related, List<PropertyComparison>? changes, T entity)
    {
        if (ServiceManager.IsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(entityType, entityId)
            {
                AuditType = type.ToString(),
                ById = userId.ToString() ?? userName,
                ByName = userName,
                Description = auditDescription
            };

            if(changes != null && changes.Count > 0)
            {
                a.Changes = System.Text.Json.JsonSerializer.Serialize(changes);
            }
            if (entity != null)
            {
                a.Entity = System.Text.Json.JsonSerializer.Serialize(entity);
            }
            if (related != null && related.Count > 0)
            {
                a.Related = System.Text.Json.JsonSerializer.Serialize(related);
            }

            await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
        }
    }


    public async Task EnqueueHeartbeat(object userId, string userName)
    {
        if (ServiceManager.IsLoginsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(userId.ToString() ?? userName, userName)
            {
                AuditType = AuditActivityType.Heartbeat,
                ById = userId.ToString() ?? userName,
                ByName = userName
            };
            await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
        }
    }
    public async Task EnqueueLogin(object userId, string userName, string provider = "forms", bool isSuccess = true)
    {
        if (ServiceManager.IsLoginsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(userId.ToString()?? userName, userName)
            {
                AuditType = AuditActivityType.Login,
                ById = userId.ToString() ?? userName,
                ByName = userName,
                Description = provider,
                Entity = isSuccess.ToString()
            };
            await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
        }
    }
    public async Task EnqueuePermissionException(object userId, string userName, string resource)
    {
        if (ServiceManager.IsPermissionsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(userId.ToString() ?? "Unknown", userName)
            {
                AuditType = AuditActivityType.Permission,
                ById = userId.ToString() ?? "Unknown",
                ByName = userName,
                Description = resource
            };
            await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
        }
    }
    public async Task EnqueuePermissionException(object userId, string userName, string entityType, object entityId, string resource)
    {
        if (ServiceManager.IsPermissionsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(userId.ToString() ?? userName, userName)
            {
                AuditType = AuditActivityType.Permission,
                ById = userId.ToString() ?? userName,
                ByName = userName,
                EntityType = entityType,
                EntityId = entityId.ToString() ?? entityType,
                Description = resource
            };
            await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
        }
    }
}