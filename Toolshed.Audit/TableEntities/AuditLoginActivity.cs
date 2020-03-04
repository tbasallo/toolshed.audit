using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Audit
{
    public class AuditLoginActivity : TableEntity, IRowIncrementable
    {
        public AuditLoginActivity() { }
        public AuditLoginActivity(DateTimeOffset date, string userId, string userName)
        {
            PartitionKey = date.ToString("yyyyMM");
            this.SetRowKeyToReverseTicks();
            UserId = userId;
            Name = userName ?? userId;

            On = date;
        }

        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset On { get; set; }

        /// <summary>
        /// The resource that was accessed. Used by resource tracking and permission exceptions
        /// </summary>
        public string ExtraInfo { get; set; }

        public override string ToString()
        {
            return PartitionKey + " " + UserId;
        }
    }
}
