using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;

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
        /// Add required services to dependency injection
        /// </summary>
        /// <param name="services"></param>
        public static void AddToolshedAuditing(this IServiceCollection services, string azureStorageConnectionString)
        {
            ServiceManager.InitConnectionString(azureStorageConnectionString);

            services.AddScoped<AuditRepository>();
            services.AddScoped<AuditEnqueuer>();
        }
    }
}
