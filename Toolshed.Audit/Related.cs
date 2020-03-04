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
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Description { get; set; }
    }
}
