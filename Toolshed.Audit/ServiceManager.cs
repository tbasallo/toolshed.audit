using Azure.Data.Tables;
using Azure.Storage.Queues;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Toolshed.Audit;

public static class ServiceManager
{

    public static string StorageName { get; private set; } = null!;
    public static string ConnectionString { get; private set; } = null!;
    public static string? TablePrefix { get; set; } = null!;

    /// <summary>
    /// For tables that use a DATE BASED partition, this is the time zone that the UTC date is CONVERTED TO before saving. This means that QUERYING the partition must provide this date or use
    /// the helper method
    /// </summary>
    public static string? PartitionTimeZone { get; set; }

    public static void SetQueueName(string queueName)
    {
        QueueName = queueName;
    }
    public static TableClient GetTableClient(string tableName)
    {
        return new TableClient(ConnectionString, tableName);
    }

    public static string QueueName { get; private set; } = "auditor-pending-items";
    public static bool IsEnabled { get; set; }
    public static bool IsLoginsEnabled { get; set; }
    public static bool IsPermissionsEnabled { get; set; }

    public static void InitConnectionString(string connectionString, bool isEnabled)
    {
        ConnectionString = connectionString;
        IsEnabled = isEnabled;
        IsLoginsEnabled = isEnabled;
        IsPermissionsEnabled = isEnabled;
    }

    public static void InitConnectionString(string connectionString, string? tablePrefix = null, bool isEnabled = true)
    {
        ConnectionString = connectionString;
        TablePrefix = tablePrefix;
        IsEnabled = isEnabled;
        IsLoginsEnabled = isEnabled;
        IsPermissionsEnabled = isEnabled;
    }

    public async static Task CreateTablesIfNotExistsAsync()
    {
        var tableCreationTasks = TableAssist.GetTables()
            .ToList()
            .Select(table => GetTableClient(table)
            .CreateIfNotExistsAsync());
        await Task.WhenAll(tableCreationTasks);
    }
    public static void CreateTablesIfNotExists()
    {
        foreach (var tableName in TableAssist.GetTables())
        {
            GetTableClient(tableName).CreateIfNotExists();
        }
    }

    public async static Task CreateQueuesIfNotExistsAsync(string? queueName = null)
    {
        var auditQueue = new QueueClient(ConnectionString, queueName ?? QueueName);
        await auditQueue.CreateAsync();
    }
    public static void CreateQueuesIfNotExists(string? queueName = null)
    {
        var auditQueue = new QueueClient(ConnectionString, queueName ?? QueueName);
        auditQueue.CreateIfNotExists();
    }
}
