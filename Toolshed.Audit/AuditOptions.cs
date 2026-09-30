using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Toolshed.Audit;

/// <summary>
/// Configures the queues and their table prefixes used by auditing.
/// When no queue is configured, the default queue (<see cref="ServiceManager.DefaultQueueName"/>) with no table prefix is used.
/// </summary>
public partial class AuditOptions
{
    //the longest table name, used to ensure the prefix + table name is a valid Azure table name (max 63)
    const int MaxTableNameLength = 63;
    static readonly int LongestTableNameLength = TableAssist.GetTables(null).Max(x => x.Length);

    readonly Dictionary<string, string?> _queues = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The configured queues (lower case) and their table prefixes. The default queue has a NULL prefix
    /// </summary>
    public IReadOnlyDictionary<string, string?> Queues => _queues;

    public bool IsEnabled { get; set; } = true;
    public bool IsLoginsEnabled { get; set; } = true;
    public bool IsPermissionsEnabled { get; set; } = true;

    /// <summary>
    /// For tables that use a DATE BASED partition, this is the time zone that the UTC date is CONVERTED TO before saving
    /// </summary>
    public string? PartitionTimeZone { get; set; }

    /// <summary>
    /// Adds the default queue (<see cref="ServiceManager.DefaultQueueName"/>) which writes to tables without a prefix.
    /// Only needed when also adding other queues; when no queues are added, the default queue is used automatically.
    /// </summary>
    public AuditOptions UseDefaultQueue()
    {
        if (_queues.ContainsKey(ServiceManager.DefaultQueueName))
        {
            throw new ArgumentException($"The default queue '{ServiceManager.DefaultQueueName}' has already been added.");
        }
        _queues[ServiceManager.DefaultQueueName] = null;
        return this;
    }

    /// <summary>
    /// Adds a queue whose items are written to tables using the table prefix
    /// </summary>
    public AuditOptions AddQueue(string queueName, string tablePrefix)
    {
        var name = NormalizeQueueName(queueName);
        if (name == ServiceManager.DefaultQueueName)
        {
            throw new ArgumentException($"Use UseDefaultQueue() to add the default queue '{ServiceManager.DefaultQueueName}'.", nameof(queueName));
        }
        if (_queues.ContainsKey(name))
        {
            throw new ArgumentException($"The queue '{name}' has already been added.", nameof(queueName));
        }

        ValidateTablePrefix(tablePrefix);
        _queues[name] = tablePrefix;
        return this;
    }

    /// <summary>
    /// Normalizes (lower case) and validates the queue name against the Azure queue naming rules
    /// </summary>
    public static string NormalizeQueueName(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var name = queueName.Trim().ToLowerInvariant();
        if (!QueueNameRegex().IsMatch(name) || name.Contains("--"))
        {
            throw new ArgumentException($"The queue name '{queueName}' is invalid. Queue names must be 3-63 characters, contain only letters, numbers and single hyphens, and start and end with a letter or number.", nameof(queueName));
        }
        return name;
    }

    static void ValidateTablePrefix(string tablePrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tablePrefix);

        if (!TablePrefixRegex().IsMatch(tablePrefix))
        {
            throw new ArgumentException($"The table prefix '{tablePrefix}' is invalid. Table prefixes must start with a letter and contain only letters and numbers.", nameof(tablePrefix));
        }
        if (tablePrefix.Length + LongestTableNameLength > MaxTableNameLength)
        {
            throw new ArgumentException($"The table prefix '{tablePrefix}' is too long. It can be at most {MaxTableNameLength - LongestTableNameLength} characters.", nameof(tablePrefix));
        }
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$")]
    private static partial Regex QueueNameRegex();

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]*$")]
    private static partial Regex TablePrefixRegex();
}
