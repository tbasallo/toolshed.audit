using System;

namespace Toolshed.Audit
{
    /// <summary>
    /// The actual item deleted. The details for this deletion are available in the object's log
    /// </summary>
    public class AuditDeletion : Toolshed.AzureStorage.BaseTableEntity, IRowIncrementable
    {
        public AuditDeletion() { }
        public AuditDeletion(string entityType, object entityId)
        {
            EntityType = entityType.ToLowerInvariant();
            EntityId = entityId.ToString().ToLowerInvariant();

            PartitionKey = $"{EntityType}_{EntityId}";
            this.SetRowKeyToReverseTicks();
        }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Entity { get; set; }
        public string ByName { get; set; }
        public string ById { get; set; }
        public DateTimeOffset On { get; set; }

        public override string ToString()
        {
            return PartitionKey + " (deleted)";
        }
    }
}
