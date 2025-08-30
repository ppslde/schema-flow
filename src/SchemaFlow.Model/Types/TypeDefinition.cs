namespace SchemaFlow.Model.Types;

/// <summary>
/// Base class for simple and complex type definitions. May be global (named) or anonymous (embedded).
/// </summary>
public abstract record class TypeDefinition
{
    /// <summary>
    /// Qualified name of the type. Null means the type is anonymous (local).
    /// </summary>
    public QualifiedName? Name { get; set; } // null = anonym

    /// <summary>
    /// True if this type has a global name (is a top-level definition).
    /// </summary>
    public bool IsGlobal => Name != null;

    /// <summary>
    /// Optional human-readable documentation (annotation/appinfo).
    /// </summary>
    public string? Documentation { get; set; }

    /// <summary>
    /// Optional source location in the original XSD for quick navigation.
    /// </summary>
    public SourceLocation? Source { get; set; }
}