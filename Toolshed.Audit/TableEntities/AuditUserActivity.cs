using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Audit
{
    public class AuditUserActivity : TableEntity, IRowIncrementable
    {
        public AuditUserActivity() { }
        public AuditUserActivity(DateTimeOffset date, string userId, string name)
        {
            PartitionKey = userId;
            this.SetRowKeyToReverseTicks();
            UserId = PartitionKey;
            Name = name ?? userId;
            On = date;
        }

        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset On { get; set; }

        public string EntityPartitionKey { get; set; }
        public string EntityRowKey { get; set; }
        public string AuditType { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Extra info related to the type of audit. For instance, for logins this would include the provider, for permission exceptions this could be the resource being accessed
        /// </summary>
        public string ExtraInfo { get; set; }        

        public override string ToString()
        {
            return PartitionKey;
        }
    }
}
