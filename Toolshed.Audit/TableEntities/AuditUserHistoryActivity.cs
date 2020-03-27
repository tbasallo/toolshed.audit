using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Audit
{
    public class AuditUserHistoryActivity : TableEntity, IRowIncrementable
    {
        public AuditUserHistoryActivity() { }
        public AuditUserHistoryActivity(DateTimeOffset date, string userId, string name)
        {
            PartitionKey = "USER";
            RowKey = userId;
            UserId = PartitionKey;
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
