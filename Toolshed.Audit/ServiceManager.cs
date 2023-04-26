using Azure.Storage.Queues;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Toolshed.Audit
{
    public static class ServiceManager
    {
        internal static StorageConnectionType StorageConnectionType { get; private set; }

        public static string StorageName { get; private set; }
        public static string ConnectionKey { get; private set; }
        public static string TablePrefix { get; set; }

        /// <summary>
        /// For tables that use a DATE BASED partition, this is the time zone that the UTC date is CONVERTED TO before saving. This means that QUERYING the partition must provide this date or use
        /// the helper method
        /// </summary>
        public static string PartitionTimeZone { get; set; }

        public static void SetQueueName(string queueName)
        {
            QueueName = queueName;
        }

        public static string QueueName { get; private set; } = "auditor-pending-items";
        public static bool IsEnabled { get; set; }
        public static bool IsLoginsEnabled { get; set; }
        public static bool IsPermissionsEnabled { get; set; }

        public static void InitStorageKey(string storageName, string storageKey, bool isEnabled)
        {
            StorageName = storageName;
            ConnectionKey = storageKey;
            StorageConnectionType = StorageConnectionType.Key;
            IsEnabled = isEnabled;
            IsLoginsEnabled = isEnabled;
            IsPermissionsEnabled = isEnabled;
        }
        public static void InitConnectionString(string connectionString, bool isEnabled)
        {
            ConnectionKey = connectionString;
            StorageConnectionType = StorageConnectionType.Key;
            IsEnabled = isEnabled;
            IsLoginsEnabled = isEnabled;
            IsPermissionsEnabled = isEnabled;
        }


        public static void InitStorageKey(string storageName, string storageKey, string tablePrefix = null, bool isEnabled = true)
        {
            StorageName = storageName;
            ConnectionKey = storageKey;
            StorageConnectionType = StorageConnectionType.Key;
            TablePrefix = tablePrefix;
            IsEnabled = isEnabled;
            IsLoginsEnabled = isEnabled;
            IsPermissionsEnabled = isEnabled;
        }
        public static void InitConnectionString(string connectionString, string tablePrefix = null, bool isEnabled = true)
        {
            ConnectionKey = connectionString;
            StorageConnectionType = StorageConnectionType.ConnectionString;
            TablePrefix = tablePrefix;
            IsEnabled = isEnabled;
            IsLoginsEnabled = isEnabled;
            IsPermissionsEnabled = isEnabled;
        }

        public async static Task CreateTablesIfNotExistsAsync()
        {
            var tableCreationTasks = TableAssist.GetTables()
                .ToList()
                .Select(table => GetTableClient().GetTableReference(TableAssist.AuditActivities())
                .CreateIfNotExistsAsync());
            await Task.WhenAll(tableCreationTasks);
        }
        public static void CreateTablesIfNotExists()
        {
            foreach (var tableName in TableAssist.GetTables())
            {
                GetTableClient().GetTableReference(tableName).CreateIfNotExists();
            }
        }

        public async static Task CreateQueuesIfNotExistsAsync(string queueName = null)
        {
            if (StorageConnectionType == StorageConnectionType.Key)
            {
                var auditQueue = new QueueClient(new Uri($"{StorageName}.queue.core.windows.net/{queueName ?? QueueName}"), new Azure.Storage.StorageSharedKeyCredential(StorageName, ConnectionKey));
                await auditQueue.CreateIfNotExistsAsync();
            }
            else
            {
                var auditQueue = new QueueClient(ConnectionKey, queueName ?? QueueName);
                await auditQueue.CreateAsync();
            }
        }
        public static void CreateQueuesIfNotExists(string queueName = null)
        {
            if (StorageConnectionType == StorageConnectionType.Key)
            {
                var auditQueue = new QueueClient(new Uri($"{StorageName}.queue.core.windows.net/{queueName ?? QueueName}"), new Azure.Storage.StorageSharedKeyCredential(StorageName, ConnectionKey));
                auditQueue.CreateIfNotExists();
            }
            else
            {
                var auditQueue = new QueueClient(ConnectionKey, queueName ?? QueueName);
                auditQueue.CreateIfNotExists();
            }
        }
    }
}
