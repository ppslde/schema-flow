using SchemaFlow.Model.ContentModels;

namespace SchemaFlow.Model.GlobalDefinitions;

/// <summary>
/// Global (named) model group declaration.
/// </summary>
public record class CompositorDecl
{
    /// <summary>Qualified name of the group.</summary>
    public QualifiedName Name { get; set; } = new();

    /// <summary>The actual group definition.</summary>
    public Compositor Compositor { get; set; } = new();
}