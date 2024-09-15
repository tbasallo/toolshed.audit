using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    /// <summary>
    /// Object that allows for easier reference to other audited items that may have been related
    /// </summary>
    public class RelatedEntity
    {
        public RelatedEntity() { }
        public RelatedEntity(string entityType, object entityId)
        {
            EntityType = entityType;
            EntityId = entityId.ToString() ?? entityType;
        }
        public string EntityType { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string? Description { get; set; }
    }
}
