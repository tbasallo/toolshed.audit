using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public enum AuditActivityType
    {
        /// <summary>
        /// Indicates the specified entity was created or the specified user created an entity
        /// </summary>
        Create,
        /// <summary>
        /// Indicates a change/update was performed on the specified entity or by the user
        /// </summary>
        Update,
        /// <summary>
        /// Indicates a delete action was taken on the specified entity or by the user
        /// </summary>
        Delete,
        /// <summary>
        /// Info event; typically related to a related entity or to provide additional information on an entity
        /// </summary>
        Access,
        /// <summary>
        /// Indicates that the specified entity was accessed
        /// </summary>
        Info,
        /// <summary>
        /// Indicates a login event
        /// </summary>
        Login,
        /// <summary>
        /// Indicates an exception or failed permission test
        /// </summary>
        Permission,
        /// <summary>
        /// Indicates a user is still connected, typically used to show that a user is still active even if audit activity hasn't occurred
        /// </summary>
        Heartbeat
    }
}
