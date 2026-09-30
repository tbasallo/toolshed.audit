using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace Toolshed.Audit;

public static class ServiceManager
{

    public static string StorageName { get; private set; } = null!;
    public static string ConnectionString { get; private set; } = null!;
    public static string? TablePrefix { get; set; } = null!;

    static readonly ConcurrentDictionary<string, string?> _queueTablePrefixes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The table prefixes registered per queue name
    /// </summary>
    public static IReadOnlyDictionary<string, string?> QueueTablePrefixes => _queueTablePrefixes;

    /// <summary>
    /// Associates a table prefix with a queue name. Items processed from this queue are written to tables using this prefix.
    /// </summary>
    public static void SetQueueTablePrefix(string queueName, string? tablePrefix)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        _queueTablePrefixes[queueName] = tablePrefix;
    }

    /// <summary>
    /// Gets the table prefix for the queue name. Falls back to <see cref="TablePrefix"/> when the queue is null or not registered.
    /// </summary>
    public static string? GetTablePrefix(string? queueName)
    {
        if (!string.IsNullOrWhiteSpace(queueName) && _queueTablePrefixes.TryGetValue(queueName!, out var prefix))
        {
            return prefix;
        }
        return TablePrefix;
    }

    /// <summary>
    /// For tables that use a DATE BASED partition, this is the time zone that the UTC date is CONVERTED TO before saving. This means that QUERYING the partition must provide this date or use
    /// the helper method
    /// </summary>
    public static string? PartitionTimeZone { get; set; }

    public static TableClient GetTableClient(string tableName)
    {
        return new TableClient(ConnectionString, tableName);
    }

    public const string DefaultQueueName = "auditor-pending-items";

    public static string QueueName { get; private set; } = DefaultQueueName;
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

    public static void InitConnectionString(string connectionString, string? queueName = null, string? tablePrefix = null, bool isEnabled = true)
    {
        ConnectionString = connectionString;
        QueueName = queueName.ToValue(DefaultQueueName);
        TablePrefix = tablePrefix;
        IsEnabled = isEnabled;
        IsLoginsEnabled = isEnabled;
        IsPermissionsEnabled = isEnabled;
    }

    /// <summary>
    /// Initializes the service with a table prefix per queue name. 
    /// If isDefaultTableAndQueueEnabled is FALSE, he first queue becomes the default <see cref="QueueName"/> and its prefix the default <see cref="TablePrefix"/>.
    /// </summary>
    /// <param name="isDefaultTableAndQueueEnabled">Indicates whether the first queue and its prefix should be set as the default. 
    /// <param name="queueTablePrefixes">A dictionary of queue names and their associated table prefixes. The first queue in the dictionary will be used as the default if isDefaultTableAndQueueEnabled is FALSE.</param>
    /// If TRUE, the default values will be used supporting environments that did not set a queue and table prefix.
    /// If FALSE, the first queue and its prefix will be set as the default.</param>
    public static void InitConnectionString(string connectionString, IDictionary<string, string?> queueTablePrefixes, bool isDefaultTableAndQueueEnabled = true, bool isEnabled = true)
    {
        if (queueTablePrefixes is null || queueTablePrefixes.Count == 0)
        {
            throw new ArgumentException("At least one queue name must be provided", nameof(queueTablePrefixes));
        }

        ConnectionString = connectionString;
        IsEnabled = isEnabled;
        IsLoginsEnabled = isEnabled;
        IsPermissionsEnabled = isEnabled;

        _queueTablePrefixes.Clear();
        foreach (var item in queueTablePrefixes)
        {
            SetQueueTablePrefix(item.Key, item.Value);
        }

        if(!isDefaultTableAndQueueEnabled)
        {
            var first = queueTablePrefixes.First();
            QueueName = first.Key;
            TablePrefix = first.Value;
        }
    }






    static IEnumerable<string> GetAllTables()
    {
        var prefixes = _queueTablePrefixes.Values.Append(TablePrefix).Distinct();
        return prefixes.SelectMany(TableAssist.GetTables).Distinct();
    }

    public async static Task CreateTablesIfNotExistsAsync()
    {
        var tableCreationTasks = GetAllTables()
            .ToList()
            .Select(table => GetTableClient(table)
            .CreateIfNotExistsAsync());
        await Task.WhenAll(tableCreationTasks);
    }
    public static void CreateTablesIfNotExists()
    {
        foreach (var tableName in GetAllTables())
        {
            GetTableClient(tableName).CreateIfNotExists();
        }
    }

    static IEnumerable<string> GetAllQueues(string? queueName = null)
    {
        return _queueTablePrefixes.Keys
            .Append(QueueName)
            .Append(queueName)
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .Select(q => q!)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    public async static Task CreateQueuesIfNotExistsAsync(string? queueName = null)
    {
        foreach (var queue in GetAllQueues(queueName))
        {
            await new QueueClient(ConnectionString, queue).CreateIfNotExistsAsync();
        }
    }
    public static void CreateQueuesIfNotExists(string? queueName = null)
    {
        foreach (var queue in GetAllQueues(queueName))
        {
            new QueueClient(ConnectionString, queue).CreateIfNotExists();
        }
    }
}
