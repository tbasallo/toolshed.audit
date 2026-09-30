using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

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
        /// Sets the connection string and optional queue name and tablePrefix and then the scoped services into DI.
        /// If you want to set init values via the ServiceManager, do so after calling this method.
        /// </summary>
        /// <param name="services"></param>
        public static void AddToolshedAuditing(this IServiceCollection services, string azureStorageConnectionString, string? queueName = null, string? tablePrefix = null)
        {
            ServiceManager.InitConnectionString(azureStorageConnectionString, queueName: queueName, tablePrefix: tablePrefix);

            services.AddScoped<AuditRepository>();
            services.AddScoped<AuditEnqueuer>();

        }

        /// <summary>
        /// Sets the connection string and a dictionary with queue names and their associated table prefixes. and then the scoped services into DI.
        /// Additionally, the default queue and table will be created
        /// </summary>
        /// <param name="services"></param>
        public static void AddToolshedAuditing(this IServiceCollection services, string azureStorageConnectionString, IDictionary<string, string?> queueTablePrefixes)
        {
            ServiceManager.InitConnectionString(azureStorageConnectionString, queueTablePrefixes, isDefaultTableAndQueueEnabled: true);

            services.AddScoped<AuditRepository>();
            services.AddScoped<AuditEnqueuer>();

        }
    }
}
