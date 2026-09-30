using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public static class TableAssist
    {
        const string AuditActivityTableName = "AuditActivities";
        const string AuditActivityHistoryTableName = "AuditActivityHistories";
        const string AuditDeletionTableName = "AuditDeletions";
        const string AuditUserTableName = "AuditUsers";
        const string AuditUserLoginsTableName = "AuditUserLogins";
        const string AuditLoginsTableName = "AuditLogins";
        const string AuditPermissionsTableName = "AuditPermissions";


        /// <summary>
        /// Returns all the table names used by the Toolshed.Audit library, with the optional prefix applied to each table name at startup.
        /// This is used by the application directly writing and reading from tables
        /// </summary>
        /// <returns></returns>
        public static string[] GetTables()
        {
            return GetTables(ServiceManager.TablePrefix);
        }

        /// <summary>
        /// Returns all the table names used by the Toolshed.Audit library, with the specified prefix applied to each table name at startup.
        /// This is used by the indirectly when you need to set the prefix, like auditmanager reading from the queue
        /// </summary>
        /// <returns></returns>
        public static string[] GetTables(string? prefix)
        {
            return new[] { AuditActivities(prefix), AuditActivityHistories(prefix), AuditUsers(prefix), AuditDeletions(prefix), AuditUserLogins(prefix), AuditLogins(prefix), AuditPermissions(prefix) };
        }

        public static string AuditActivities() => AuditActivities(ServiceManager.TablePrefix);
        public static string AuditActivities(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditActivityTableName);
        }
        public static string AuditActivityHistories() => AuditActivityHistories(ServiceManager.TablePrefix);
        public static string AuditActivityHistories(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditActivityHistoryTableName);
        }
        public static string AuditDeletions() => AuditDeletions(ServiceManager.TablePrefix);
        public static string AuditDeletions(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditDeletionTableName);
        }
        public static string AuditUsers() => AuditUsers(ServiceManager.TablePrefix);
        public static string AuditUsers(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditUserTableName);
        }
        public static string AuditUserLogins() => AuditUserLogins(ServiceManager.TablePrefix);
        public static string AuditUserLogins(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditUserLoginsTableName);
        }
        public static string AuditLogins() => AuditLogins(ServiceManager.TablePrefix);
        public static string AuditLogins(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditLoginsTableName);
        }
        public static string AuditPermissions() => AuditPermissions(ServiceManager.TablePrefix);
        public static string AuditPermissions(string? prefix)
        {
            return string.Format("{0}{1}", prefix, AuditPermissionsTableName);
        }
    }
}
