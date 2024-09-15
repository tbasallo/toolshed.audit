using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public static class AuditActivityType
    {
        /// <summary>
        /// Indicates the specified entity was created or the specified user created an entity
        /// </summary>
        public const string Create = "Create";
        /// <summary>
        /// Indicates a change/update was performed on the specified entity or by the user
        /// </summary>
        public const string Update = "Update";
        /// <summary>
        /// Indicates a delete action was taken on the specified entity or by the user
        /// </summary>
        public const string Delete = "Delete";
        /// <summary>
        /// Info event; typically related to a related entity or to provide additional information on an entity
        /// </summary>
        public const string Access = "Access";
        /// <summary>
        /// Indicates that the specified entity was accessed
        /// </summary>
        public const string Info = "Info";
        /// <summary>
        /// Indicates a login event
        /// </summary>
        public const string Login = "Login";
        /// <summary>
        /// Indicates an exception or failed permission test
        /// </summary>
        public const string Permission = "Permission";
        /// <summary>
        /// Indicates a user is still connected, typically used to show that a user is still active even if audit activity hasn't occurred
        /// </summary>
        public const string Heartbeat = "Heartbeat";
    }
}
