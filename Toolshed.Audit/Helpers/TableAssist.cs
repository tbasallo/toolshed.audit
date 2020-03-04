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


        public static string[] GetTables()
        {
            return new[] { AuditActivities(), AuditActivityHistories(), AuditUsers(), AuditDeletions(), AuditUserLogins(), AuditLogins(), AuditPermissions() };
        }

        public static string AuditActivities()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditActivityTableName);
        }
        public static string AuditActivityHistories()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditActivityHistoryTableName);
        }
        public static string AuditDeletions()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditDeletionTableName);
        }
        public static string AuditUsers()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditUserTableName);
        }
        public static string AuditUserLogins()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditUserLoginsTableName);
        }
        public static string AuditLogins()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditLoginsTableName);
        }
        public static string AuditPermissions()
        {
            return string.Format("{0}{1}", AuditSettings.TablePrefix, AuditPermissionsTableName);
        }
    }
}
