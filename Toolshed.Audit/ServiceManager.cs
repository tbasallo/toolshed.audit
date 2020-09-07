using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos.Table;

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

        public static string QueueName { get; private set; }
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
            StorageConnectionType = StorageConnectionType.Key;
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

        static CloudTableClient _cloudTableClient;
        internal static CloudTableClient GetTableClient()
        {
            if (_cloudTableClient == null)
            {
                CloudStorageAccount storageAccount;
                if (StorageConnectionType == StorageConnectionType.Key)
                {
                    storageAccount = new CloudStorageAccount(new StorageCredentials(StorageName, ConnectionKey), true);
                }
                else if (StorageConnectionType == StorageConnectionType.ConnectionString)
                {
                    storageAccount = CloudStorageAccount.Parse(ConnectionKey);
                }
                else
                {
                    throw new NotImplementedException("Unknown Unimplemented Connection to Storage");
                }

                //makes sense here...how small?
                // ServicePointManager.FindServicePoint(storageAccount.TableEndpoint).UseNagleAlgorithm = true;
                _cloudTableClient = storageAccount.CreateCloudTableClient();
            }

            return _cloudTableClient;
        }

    }
}
