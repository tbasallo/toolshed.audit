namespace Toolshed.Audit;

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

    /// <summary>
    /// The type of the entity
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// The ID of this entity , which should be unique within the EntityType.
    /// </summary>
    public string EntityId { get; set; } = null!;

    /// <summary>
    /// The description that will be used inserting this record. If not provided, the parent activity's description will be used.
    /// </summary>
    public string? Description { get; set; }
}
