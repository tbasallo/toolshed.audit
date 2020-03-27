﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Audit
{
    public class AuditActivityHistory : TableEntity
    {
        public AuditActivityHistory() { }
        public AuditActivityHistory(DateTimeOffset activityOn, string userId, string userName, string auditRecordPartitionKey, string auditRecordRowKey)
        {
            PartitionKey = activityOn.ToString("yyyyMMdd");
            RowKey = $"{auditRecordPartitionKey}_{auditRecordRowKey}";

            EntityPartitionKey = auditRecordPartitionKey;
            EntityRowKey = auditRecordRowKey;
            IsUser = userId.IsEqualTo(auditRecordPartitionKey);

            UserId = userId;
            UserName = userName;
        }

        public bool IsUser { get; set; }
        public DateTimeOffset On { get; set; }

        public string EntityType { get; set; }
        public string EntityId { get; set; }

        public string EntityPartitionKey { get; set; }
        public string EntityRowKey { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public override string ToString()
        {
            return PartitionKey + " - " + RowKey;
        }
    }
}