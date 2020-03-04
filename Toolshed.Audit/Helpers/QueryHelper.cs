using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public static class QueryHelper
    {
        public static string GetPartitionKey(string entityType, object entityId)
        {
            return string.Format("{0}_{1}", entityType, entityId).ToLowerInvariant();
        }
    }
}
