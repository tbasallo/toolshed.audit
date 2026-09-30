using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Toolshed.Audit
{
    public static class Extensions
    {

        /// <summary>
        /// Set the entity's ROWKEY to reverse ticks for better reverse ordering when pulling out of storage
        /// </summary>
        /// <param name="entity"></param>
        public static void SetRowKeyToReverseTicks(this Toolshed.AzureStorage.BaseTableEntity entity)
        {
            entity.RowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Increment the entity's ROWKEY, which should be a reverse tick, by one to deal with potential matches when quickly reporting on the same partition key
        /// </summary>
        public static void IncrementRowKey(this IRowIncrementable entity)
        {
            entity.RowKey = (Convert.ToInt64(entity.RowKey) + 1).ToString();
        }

        /// <summary>
        /// Initializes auditing and registers a keyed <see cref="AuditEnqueuer"/> and <see cref="AuditRepository"/> per queue.
        /// When no queues are configured, the default queue (<see cref="ServiceManager.DefaultQueueName"/>) with no table prefix is used.
        /// Inject with [FromKeyedServices("queue-name")] using the lower case queue name (see <see cref="ServiceManager.GetServiceKey"/>).
        /// </summary>
        public static IServiceCollection AddToolshedAuditing(this IServiceCollection services, string azureStorageConnectionString, Action<AuditOptions>? configure = null)
        {
            ServiceManager.Init(azureStorageConnectionString, configure);

            foreach (var queueName in ServiceManager.QueueTablePrefixes.Keys)
            {
                var key = ServiceManager.GetServiceKey(queueName);
                services.TryAddKeyedScoped(key, (_, _) => new AuditEnqueuer(queueName));
                services.TryAddKeyedScoped(key, (_, _) => new AuditRepository(queueName));
            }
            services.TryAddScoped<AuditJanitor>();

            return services;
        }
    }
}
