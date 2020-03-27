using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace Toolshed.Audit
{
    /// <summary>
    /// Used to queue up audit items for processing asynchronously
    /// </summary>
    public class AuditEnqueuer
    {
        public AuditEnqueuer() : this(null)
        {

        }
        public AuditEnqueuer(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName) && string.IsNullOrWhiteSpace(AuditSettings.QueueName))
            {
                throw new ArgumentNullException(nameof(queueName), "The queue name must be set in the settings or in the constructor for AuditManager");
            }

            if (AuditSettings.StorageConnectionType == StorageConnectionType.Key)
            {
                AuditQueue = new QueueClient(new Uri($"https://{AuditSettings.StorageName}.queue.core.windows.net/{queueName ?? AuditSettings.QueueName}"), new Azure.Storage.StorageSharedKeyCredential(AuditSettings.StorageName, AuditSettings.ConnectionKey));
            }
            else
            {
                AuditQueue = new QueueClient(AuditSettings.ConnectionKey, queueName ?? AuditSettings.QueueName);
            }
        }

        private QueueClient AuditQueue { get; }

        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, null, null, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, null, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, List<RelatedEntity> related)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, null, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, List<PropertyComparison> changes)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, changes, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, List<RelatedEntity> related, List<PropertyComparison> changes)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, changes, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, List<RelatedEntity> related)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, related, null, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, List<PropertyComparison> changes)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, null, changes, default(object));
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, object userId, string userName, List<RelatedEntity> related, List<PropertyComparison> changes)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, related, changes, default(object));
        }


        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, object userId, string userName, T entity)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, null, null, entity);
        }
        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, T entity)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, null, null, entity);

        }
        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, object userId, string userName, List<RelatedEntity> related, T entity)
        {
            await Enqueue(entityType, entityId, type, userId, userName, null, related, null, entity);
        }
        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, List<RelatedEntity> related, T entity)
        {
            await Enqueue(entityType, entityId, type, userId, userName, auditDescription, related, null, entity);
        }


        //FOR NOW - THERE's NO OPTION WITH changes AND entity (as a shortcut) because this is a very uncommon case - if you're sending the entity, it's either new or deleted, but it wouldn't have changes
        //if you really want that last combo - use the full method call


        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, object userId, string userName, string auditDescription, List<RelatedEntity> related, List<PropertyComparison> changes, T entity)
        {
            if (AuditSettings.IsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(entityType, entityId)
                {
                    AuditType = type.ToString(),
                    ById = userId.ToString(),
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
            if (AuditSettings.IsLoginsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId.ToString(), userName)
                {
                    AuditType = AuditActivityType.Heartbeat.ToString(),
                    ById = userId.ToString(),
                    ByName = userName
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueueLogin(object userId, string userName, string provider = "forms", bool isSuccess = true)
        {
            if (AuditSettings.IsLoginsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId.ToString(), userName)
                {
                    AuditType = AuditActivityType.Login.ToString(),
                    ById = userId.ToString(),
                    ByName = userName,
                    Description = provider,
                    Entity = isSuccess.ToString()
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueuePermissionException(object userId, string userName, string resource)
        {
            if (AuditSettings.IsPermissionsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId.ToString(), userName)
                {
                    AuditType = AuditActivityType.Permission.ToString(),
                    ById = userId.ToString(),
                    ByName = userName,
                    Description = resource
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueuePermissionException(object userId, string userName, string entityType, object entityId, string resource)
        {
            if (AuditSettings.IsPermissionsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId.ToString(), userName)
                {
                    AuditType = AuditActivityType.Permission.ToString(),
                    ById = userId.ToString(),
                    ByName = userName,
                    EntityType = entityType,
                    EntityId = entityId.ToString(),
                    Description = resource
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
    }
}