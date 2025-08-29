namespace SchemaFlow.Model.ContentModels;

/// <summary>
/// Reference to a named model group.
/// </summary>
public record class GroupRef : Term
{
    /// <summary>Qualified name of the referenced group.</summary>
    public QualifiedName GroupName { get; set; } = new();
}