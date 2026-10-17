# toolshed.audit
A simple auditing framework that uses Azure queues and tables

## Getting Started

### 1. Register auditing

The simplest setup uses the default queue (`auditor-pending-items`) and tables without a prefix:

```csharp
builder.Services.AddToolshedAuditing(connectionString);
```

Optional settings:

```csharp
builder.Services.AddToolshedAuditing(connectionString, o =>
{
    o.IsEnabled = true;
    o.IsLoginsEnabled = true;
    o.IsPermissionsEnabled = true;
    o.PartitionTimeZone = "Eastern Standard Time";
});
```

Create the queues and tables once at startup (or during deployment):

```csharp
await ServiceManager.CreateQueuesIfNotExistsAsync();
await ServiceManager.CreateTablesIfNotExistsAsync();
```

### 2. Inject and use (no keyed services needed)

`AuditEnqueuer` and `AuditRepository` are registered as regular (non-keyed) scoped services that use the **default queue**:

```csharp
public class CompanyService(AuditEnqueuer auditEnqueuer, AuditRepository auditRepository)
{
    public async Task Update(Company company, string userId, string userName)
    {
        await auditEnqueuer.Enqueue("company", company.Id, AuditActivityType.Update, userId, userName);
    }

    public Task<Page<AuditActivity>> History(int id) => auditRepository.GetAuditActivity("company", id);
}
```

Outside of DI, the parameterless constructors also use the default queue: `new AuditEnqueuer()` / `new AuditRepository()`.

The default queue is:
- `auditor-pending-items` when no queues are configured, or when `UseDefaultQueue()` is called
- otherwise, the **first** queue added with `AddQueue`

### 3. Multiple queues / table prefixes (optional)

Use separate queues when multiple applications share a storage account. Each queue writes to tables using its own prefix (e.g., `AAuditActivities`).

```csharp
builder.Services.AddToolshedAuditing(connectionString, o => o
    .UseDefaultQueue()          // optional - keeps the default queue with no table prefix
    .AddQueue("app1", "A")
    .AddQueue("app2", "B"));
```

Non-keyed services still work (they use the default queue). To target a specific queue, inject a keyed service using the lower case queue name:

```csharp
public class App2Service([FromKeyedServices("app2")] AuditEnqueuer auditEnqueuer,
                         [FromKeyedServices("app2")] AuditRepository auditRepository)
{
}
```

Or create one directly: `new AuditEnqueuer("app2")`. A queue that was not registered throws an `InvalidOperationException`.

Rules:
- Queue names: 3-63 characters, lower case letters, numbers and single hyphens, starting and ending with a letter or number
- Table prefixes: start with a letter, letters and numbers only

### 4. Process the queue

In the queue-triggered processor (e.g., an Azure Function), pass the queue name the message came from:

```csharp
await AuditQueueProcessor.AddActivity(auditActivity, ServiceManager.DefaultQueueName);
```

### 5. Cleanup

`AuditJanitor` is registered as a scoped service and cleans the tables of every registered prefix:

```csharp
await auditJanitor.Delete("company_", DateTimeOffset.UtcNow.AddYears(-2));
```


## Tables & Table Entities & 

- *AuditActivities* The main audit activity table. Contains whatever is audited base on the entity and entity ID. While this could audit user's activity, using the same logic, user activity is tracked separately. However, changes to a user entity would be reflected here. PK: {entity type}_{entity id} (eg., company_323), RK: Ticks
- *AuditActivityHistories* A daily record of any entity with activity. Allows for easy lookups for purging or at seeing records accessed on a given date. PK: YYYYMMDD, RK: Entity audited's PK|RK. The table contains the PK and RK for easy lookup. The RK for this entity is the concatenation of the entity's keys.
- *AuditPermissions* Contains all permission failures. Allows for a quick look at permission failures recently. These failures would also appear in a user's login or activity feed. PK: YYYYMM, RK: Ticks
- *AuditUsers* A user's activity. References the audit activity table via the EntityPartitionKey and EntityRowKey for quick reference. Every audited item automatically adds an entry at the user level.
- *AuditUserLogins* Contains a user's logins and permission exceptions. Sllows for a quick look at all logins or failures for a user. Allows focus on this since a user might have a lot of activity. PK: Userid, RK: Ticks