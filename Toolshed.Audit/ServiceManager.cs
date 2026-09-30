using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace Toolshed.Audit;

public static class ServiceManager
{
    public const string DefaultQueueName = "auditor-pending-items";

    static IReadOnlyDictionary<string, string?> _queueTablePrefixes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultQueueName] = null
    };

    public static string StorageName { get; private set; } = null!;
    public static string ConnectionString { get; private set; } = null!;

    /// <summary>
    /// The registered queue names and their table prefixes. The default queue has a NULL prefix
    /// </summary>
    public static IReadOnlyDictionary<string, string?> QueueTablePrefixes => _queueTablePrefixes;

    /// <summary>
    /// The distinct table prefixes of all registered queues (NULL is no prefix)
    /// </summary>
    public static IEnumerable<string?> TablePrefixes => _queueTablePrefixes.Values.Distinct(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// For tables that use a DATE BASED partition, this is the time zone that the UTC date is CONVERTED TO before saving. This means that QUERYING the partition must provide this date or use
    /// the helper method
    /// </summary>
    public static string? PartitionTimeZone { get; set; }

    public static bool IsEnabled { get; set; }
    public static bool IsLoginsEnabled { get; set; }
    public static bool IsPermissionsEnabled { get; set; }

    /// <summary>
    /// Initializes the service. When no queues are configured, the default queue (<see cref="DefaultQueueName"/>) with no table prefix is used.
    /// </summary>
    public static void Init(string connectionString, Action<AuditOptions>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var options = new AuditOptions();
        configure?.Invoke(options);
        if (options.Queues.Count == 0)
        {
            options.UseDefaultQueue();
        }

        ConnectionString = connectionString;
        _queueTablePrefixes = new Dictionary<string, string?>(options.Queues, StringComparer.OrdinalIgnoreCase);
        IsEnabled = options.IsEnabled;
        IsLoginsEnabled = options.IsLoginsEnabled;
        IsPermissionsEnabled = options.IsPermissionsEnabled;
        PartitionTimeZone = options.PartitionTimeZone;
    }

    public static bool IsQueueRegistered(string? queueName)
    {
        return !string.IsNullOrWhiteSpace(queueName) && _queueTablePrefixes.ContainsKey(queueName.Trim());
    }

    /// <summary>
    /// Gets the registered, normalized queue name. Throws when the queue is not registered.
    /// </summary>
    public static string GetQueueName(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        if (!IsQueueRegistered(queueName))
        {
            throw new InvalidOperationException($"The queue '{queueName}' is not registered. Register it when adding auditing.");
        }
        return queueName.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the table prefix for the queue name. Throws when the queue is not registered.
    /// </summary>
    public static string? GetTablePrefix(string queueName)
    {
        return _queueTablePrefixes[GetQueueName(queueName)];
    }

    /// <summary>
    /// The key used to register keyed services for the queue
    /// </summary>
    public static string GetServiceKey(string queueName)
    {
        return AuditOptions.NormalizeQueueName(queueName);
    }

    public static TableClient GetTableClient(string tableName)
    {
        return new TableClient(ConnectionString, tableName);
    }

    static IEnumerable<string> GetAllTables()
    {
        return TablePrefixes.SelectMany(TableAssist.GetTables).Distinct();
    }

    public async static Task CreateTablesIfNotExistsAsync()
    {
        await Task.WhenAll(GetAllTables().Select(table => GetTableClient(table).CreateIfNotExistsAsync()));
    }
    public static void CreateTablesIfNotExists()
    {
        foreach (var tableName in GetAllTables())
        {
            GetTableClient(tableName).CreateIfNotExists();
        }
    }

    public async static Task CreateQueuesIfNotExistsAsync()
    {
        foreach (var queue in _queueTablePrefixes.Keys)
        {
            await new QueueClient(ConnectionString, queue).CreateIfNotExistsAsync();
        }
    }
    public static void CreateQueuesIfNotExists()
    {
        foreach (var queue in _queueTablePrefixes.Keys)
        {
            new QueueClient(ConnectionString, queue).CreateIfNotExists();
        }
    }
}
