using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Queues;
using Azure.Core;
using Microsoft.Azure.Cosmos.Table;
using System.Net;
using System.Threading.Tasks;

namespace Toolshed.Audit
{
    /// <summary>
    /// Manages the processing of queued items into the respective tables
    /// </summary>
    public class AuditManager
    {
        public async Task AddActivity(AuditActivity auditActivity)
        {
            switch (auditActivity.GetActivityType())
            {
                case AuditActivityType.Login:
                case AuditActivityType.Heartbeat:
                    await ProcessLogin(auditActivity);
                    break;
                case AuditActivityType.Permission:
                    await ProcessPermissionIssue(auditActivity);
                    break;
                case AuditActivityType.Create:
                case AuditActivityType.Update:
                case AuditActivityType.Delete:
                case AuditActivityType.Info:
                case AuditActivityType.Access:
                default:
                    await ProcessActivity(auditActivity);
                    break;
            }
        }

        async Task ProcessActivity(AuditActivity auditActivity)
        {
            var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
            {
                AuditType = auditActivity.AuditType,
                EntityPartitionKey = auditActivity.PartitionKey,
                EntityRowKey = auditActivity.RowKey,
                On = auditActivity.On,
                EntityType = auditActivity.EntityType,
                EntityId = auditActivity.EntityId
            };
            var activityHistory = new AuditActivityHistory(auditActivity.On, auditActivity.ById, auditActivity.ByName, auditActivity.PartitionKey, auditActivity.RowKey)
            {
                EntityType = auditActivity.EntityType,
                EntityId = auditActivity.EntityId
            };

            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivities()), TableOperation.Insert(auditActivity));
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers()), TableOperation.Insert(activityUser));
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditActivityHistories()), TableOperation.InsertOrReplace(activityHistory));

            if (auditActivity.GetActivityType() == AuditActivityType.Delete && auditActivity.Entity != null)
            {
                var deletion = new AuditDeletion(auditActivity.EntityType, auditActivity.EntityId)
                {
                    Entity = auditActivity.Entity
                };
                await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditDeletions()), TableOperation.Insert(deletion));
            };
        }
        async Task ProcessLogin(AuditActivity auditActivity)
        {
            var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
            {
                AuditType = auditActivity.AuditType,
                ExtraInfo = auditActivity.Description
            };

            if (auditActivity.Entity != null && bool.TryParse(auditActivity.Entity, out bool isSuccess))
            {
                activityUser.IsSuccessful = isSuccess;
            }

            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers()), TableOperation.InsertOrReplace(activityUser));
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUserLogins()), TableOperation.InsertOrReplace(activityUser));

            var activityLogin = new AuditLoginActivity(auditActivity.On.DateTime, auditActivity.ById, auditActivity.ByName)
            {
                ExtraInfo = auditActivity.Description
            };
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditLogins()), TableOperation.InsertOrReplace(activityLogin));
        }
        async Task ProcessPermissionIssue(AuditActivity auditActivity)
        {
            var activityUser = new AuditUserActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName)
            {
                AuditType = auditActivity.AuditType,
                ExtraInfo = auditActivity.Description
            };
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUsers()), TableOperation.InsertOrReplace(activityUser));
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditUserLogins()), TableOperation.InsertOrReplace(activityUser));

            var activityPermission = new AuditPermissionActivity(auditActivity.On, auditActivity.ById, auditActivity.ByName, auditActivity.EntityType, auditActivity.EntityId)
            {
                ExtraInfo = auditActivity.Description
            };
            await ExecuteAsync(AuditSettings.GetTableClient().GetTableReference(TableAssist.AuditPermissions()), TableOperation.InsertOrReplace(activityPermission));
        }

        async Task ExecuteAsync(CloudTable cloudTable, TableOperation tableOperation)
        {
            var userSaved = false;
            var userSaveAttempts = 0;
            while (!userSaved)
            {
                if (userSaveAttempts > 0 && tableOperation.Entity is IRowIncrementable)
                {
                    ((IRowIncrementable)tableOperation.Entity).IncrementRowKey();
                }

                try
                {
                    await cloudTable.ExecuteAsync(tableOperation);
                    userSaved = true;
                }
                catch (StorageException x)
                {
                    userSaveAttempts++;
                    if (x.RequestInformation.HttpStatusCode == 409)
                    {
                        //already exists, need a new one, keep going
                        continue;
                    }

                    //we've tried this too many times!!
                    if (userSaveAttempts > 10)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
