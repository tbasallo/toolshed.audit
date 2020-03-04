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


        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, string auditDescription, string userId, string userName)
        {
            if (AuditSettings.IsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(entityType, entityId)
                {
                    AuditType = type.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = auditDescription
                };

                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task Enqueue(string entityType, object entityId, AuditActivityType type, string auditDescription, string userId, string userName, List<PropertyComparison> changes)
        {
            if (AuditSettings.IsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(entityType, entityId)
                {
                    AuditType = type.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = auditDescription,
                    Changes = System.Text.Json.JsonSerializer.Serialize(changes)
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, string auditDescription, string userId, string userName, T entity)
        {
            if (AuditSettings.IsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(entityType, entityId)
                {
                    AuditType = type.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = auditDescription,
                    Entity = System.Text.Json.JsonSerializer.Serialize(entity)
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task Enqueue<T>(string entityType, object entityId, AuditActivityType type, string auditDescription, string userId, string userName, List<PropertyComparison> changes, T entity)
        {
            if (AuditSettings.IsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(entityType, entityId)
                {
                    AuditType = type.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = auditDescription,
                    Changes = System.Text.Json.JsonSerializer.Serialize(changes),
                    Entity = System.Text.Json.JsonSerializer.Serialize(entity)
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }

        public async Task EnqueueHeartbeat(string userId, string userName)
        {
            if (AuditSettings.IsLoginsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId, userName)
                {
                    AuditType = AuditActivityType.Heartbeat.ToString(),
                    ById = userId,
                    ByName = userName
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueueLogin(string userId, string userName, string provider)
        {
            if (AuditSettings.IsLoginsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId, userName)
                {
                    AuditType = AuditActivityType.Login.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = provider
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueuePermissionException(string userId, string userName, string resource)
        {
            if (AuditSettings.IsPermissionsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId, userName)
                {
                    AuditType = AuditActivityType.Permission.ToString(),
                    ById = userId,
                    ByName = userName,
                    Description = resource
                };
                await AuditQueue.SendMessageAsync(System.Text.Json.JsonSerializer.Serialize(a).ToBase64());
            }
        }
        public async Task EnqueuePermissionException(string userId, string userName, string entityType, object entityId, string resource)
        {
            if (AuditSettings.IsPermissionsEnabled)
            {
                //1 item is built and queued, the queue will handle the details
                var a = new AuditActivity(userId, userName)
                {
                    AuditType = AuditActivityType.Permission.ToString(),
                    ById = userId,
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