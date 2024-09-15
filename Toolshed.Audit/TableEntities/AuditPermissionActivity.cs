using System;

namespace Toolshed.Audit
{
    public class AuditPermissionActivity : Toolshed.AzureStorage.BaseTableEntity, IRowIncrementable
    {
        public AuditPermissionActivity() { }
        public AuditPermissionActivity(DateTimeOffset date, string userId, string userName, string entityType, object entityId)
        {
            PartitionKey = date.ToString("yyyyMM");
            this.SetRowKeyToReverseTicks();
            UserId = userId;
            Name = userName ?? userId;

            EntityType = entityType;
            EntityId = entityId.ToString() ?? entityType;

            On = date;
        }

        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTimeOffset On { get; set; }

        public string EntityType { get; set; } = "";
        public string EntityId { get; set; } = "";

        /// <summary>
        /// The resource that was accessed. Used by resource tracking and permission exceptions
        /// </summary>
        public string? ExtraInfo { get; set; }

        public override string ToString()
        {
            return PartitionKey + " " + UserId;
        }
    }
}
