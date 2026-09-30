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
    /// <summary>
    /// Writes to the queue. Throws when the queue is not registered.
    /// </summary>
    public AuditEnqueuer(string queueName)
    {
        QueueName = ServiceManager.GetQueueName(queueName);
        AuditQueue = new QueueClient(ServiceManager.ConnectionString, QueueName);
    }

    public string QueueName { get; }
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
                ById = ValueOrDefault(userId, userName),
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

            await Send(a);
        }
    }
    public async Task Enqueue(AuditActivity auditActivity)
    {
        if (ServiceManager.IsEnabled)
        {
            await Send(auditActivity);
        }
    }


    public async Task EnqueueHeartbeat(object userId, string userName)
    {
        if (ServiceManager.IsLoginsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(ValueOrDefault(userId, userName), userName)
            {
                AuditType = AuditActivityType.Heartbeat,
                ById = ValueOrDefault(userId, userName),
                ByName = userName
            };
            await Send(a);
        }
    }
    public async Task EnqueueLogin(object userId, string userName, string provider = "forms", bool isSuccess = true)
    {
        if (ServiceManager.IsLoginsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(ValueOrDefault(userId, userName), userName)
            {
                AuditType = AuditActivityType.Login,
                ById = ValueOrDefault(userId, userName),
                ByName = userName,
                Description = provider,
                Entity = isSuccess.ToString()
            };
            await Send(a);
        }
    }
    public async Task EnqueuePermissionException(object userId, string userName, string resource)
    {
        if (ServiceManager.IsPermissionsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(ValueOrDefault(userId, "Unknown"), userName)
            {
                AuditType = AuditActivityType.Permission,
                ById = ValueOrDefault(userId, "Unknown"),
                ByName = userName,
                Description = resource
            };
            await Send(a);
        }
    }
    public async Task EnqueuePermissionException(object userId, string userName, string entityType, object entityId, string resource)
    {
        if (ServiceManager.IsPermissionsEnabled)
        {
            //1 item is built and queued, the queue will handle the details
            var a = new AuditActivity(ValueOrDefault(userId, userName), userName)
            {
                AuditType = AuditActivityType.Permission,
                ById = ValueOrDefault(userId, userName),
                ByName = userName,
                EntityType = entityType,
                EntityId = ValueOrDefault(entityId, entityType),
                Description = resource
            };
            await Send(a);
        }
    }

    static string ValueOrDefault(object? value, string fallback)
    {
        if(value is null)
        {
            return fallback;
        }

        var text = value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? fallback : text;
    }

    /// <summary>
    /// Stamps the queue name on the activity so the processor can verify it is writing to the correct tables, then sends it
    /// </summary>
    async Task Send(AuditActivity auditActivity)
    {
        auditActivity.QueueName = QueueName;
        await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(auditActivity).ToBase64());
    }
}
