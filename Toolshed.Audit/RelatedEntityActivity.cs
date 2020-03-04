using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    /// <summary>
    /// Another entity related to the audited entity. For example, the parent when a child is updated.
    /// </summary>
    public class RelatedEntityActivity
    {
        public string EntityType { get; set; }
        public string EntityId { get; set; }

        public string GetPartitionKey()
        {
            return $"{EntityType}_{EntityId}".ToLowerInvariant();
        }

        public override string ToString()
        {
            return GetPartitionKey();
        }
    }
}
