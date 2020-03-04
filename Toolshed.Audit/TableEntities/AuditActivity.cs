using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Audit
{
    public class AuditActivity : TableEntity, IRowIncrementable
    {
        public AuditActivity() { }
        public AuditActivity(string entityType, object entityId)
        {
            EntityType = entityType.ToLowerInvariant();
            EntityId = entityId.ToString().ToLowerInvariant();
            On = DateTime.UtcNow;
            PartitionKey = $"{EntityType}_{EntityId}";
            this.SetRowKeyToReverseTicks();
        }
        public AuditActivity(string entityType, object entityId, AuditActivityType type)
        {
            EntityType = entityType.ToLowerInvariant();
            EntityId = entityId.ToString().ToLowerInvariant();

            PartitionKey = $"{EntityType}_{EntityId}";
            this.SetRowKeyToReverseTicks();
            AuditType = type.ToString();
        }

        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string AuditType { get; set; }
        public DateTimeOffset On { get; set; }
        public string ById { get; set; }
        public string ByName { get; set; }
        public string Description { get; set; }
        public string Entity { get; set; }
        public string Related { get; set; }
        public string Changes { get; set; }

        public override string ToString()
        {
            return PartitionKey;
        }

        public AuditActivityType GetActivityType()
        {
            return (AuditActivityType)Enum.Parse(typeof(AuditActivityType), AuditType);
        }

        public List<PropertyComparison> GetChanges()
        {
            if (!string.IsNullOrWhiteSpace(Changes))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<PropertyComparison>>(Changes, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new List<PropertyComparison>();
        }

        public List<RelatedEntityActivity> GetRelated()
        {
            if (!string.IsNullOrWhiteSpace(Related))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<RelatedEntityActivity>>(Related, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new List<RelatedEntityActivity>();
        }

    }
}
