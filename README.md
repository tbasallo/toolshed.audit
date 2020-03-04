# toolshed.audit
A simple auditing framework that uses Azure queues and tables


## Tables & Table Entities & 

- *AuditActivities* The main audit activity table. Contains whatever is audited base on the entity and entity ID. While this could audit user's activity, using the same logic, user activity is tracked separately. However, changes to a user entity would be reflected here. PK: {entity type}_{entity id} (eg., company_323), RK: Ticks
- *AuditActivityHistories* A daily record of any entity with activity. Allows for easy lookups for purging or at seeing records accessed on a given date. PK: YYYYMMDD, RK: Entity audited's PK|RK. The table contains the PK and RK for easy lookup. The RK for this entity is the concatenation of the entity's keys.
- *AuditPermissions* Contains all permission failures. Allows for a quick look at permission failures recently. These failures would also appear in a user's login or activity feed. PK: YYYYMM, RK: Ticks
- *AuditUsers* A user's activity. References the audit activity table via the EntityPartitionKey and EntityRowKey for quick reference. Every audited item automatically adds an entry at the user level.
- *AuditUserLogins* Contains a user's logins and permission exceptions. Sllows for a quick look at all logins or failures for a user. Allows focus on this since a user might have a lot of activity. PK: Userid, RK: Ticks