using System;

namespace Toolshed.Audit
{
    public class AuditUserHistoryActivity : Toolshed.AzureStorage.BaseTableEntity, IRowIncrementable
    {
        public AuditUserHistoryActivity() { }
        public AuditUserHistoryActivity(DateTimeOffset date, string userId, string name)
        {
            PartitionKey = "USER";
            RowKey = userId;
            UserId = userId;
            Name = name ?? userId;
            On = date;
        }

        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset On { get; set; }

        public override string ToString()
        {
            return PartitionKey + "_" + RowKey;
        }
    }
}
