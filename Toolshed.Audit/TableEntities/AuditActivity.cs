using System;
using System.Collections.Generic;


namespace Toolshed.Audit;

public class AuditActivity : Toolshed.AzureStorage.BaseTableEntity, IRowIncrementable
{
    private static readonly System.Text.Json.JsonSerializerOptions CachedJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuditActivity() { }
    public AuditActivity(string entityType, object entityId)
    {
        EntityType = entityType.ToLowerInvariant();
        EntityId = (entityId.ToString() ?? "").ToLowerInvariant();
        On = DateTime.UtcNow;
        PartitionKey = $"{EntityType}_{EntityId}";
        this.SetRowKeyToReverseTicks();
    }
    public AuditActivity(string entityType, object entityId, string auditType)
    {
        EntityType = entityType.ToLowerInvariant();
        EntityId = (entityId.ToString() ?? "").ToLowerInvariant();

        PartitionKey = $"{EntityType}_{EntityId}";
        this.SetRowKeyToReverseTicks();
        AuditType = auditType.ToString() ?? "Unknown";
    }

    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string AuditType { get; set; } = "";
    public DateTimeOffset On { get; set; }
    public string ById { get; set; } = "";
    public string ByName { get; set; } = "";
    public string? Description { get; set; }
    public string? Entity { get; set; }
    public string? Related { get; set; }
    public string? Changes { get; set; }

    public override string ToString()
    {
        return PartitionKey;
    }

    public List<PropertyComparison> GetChanges()
    {
        if (!string.IsNullOrWhiteSpace(Changes))
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<PropertyComparison>>(Changes, CachedJsonSerializerOptions) ?? [];
        }

        return [];
    }

    public List<RelatedEntity> GetRelated()
    {
        if (!string.IsNullOrWhiteSpace(Related))
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<RelatedEntity>>(Related, CachedJsonSerializerOptions) ?? [];
        }

        return [];
    }
}
