using SchemaFlow.Model.Types;

namespace SchemaFlow.Model.GlobalDefinitions;

/// <summary>
/// Global attribute declaration (simple-typed).
/// </summary>
public record class AttributeDecl
{
    /// <summary>Qualified name of the attribute.</summary>
    public QualifiedName Name { get; set; } = new();

    /// <summary>Default value (if present).</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Fixed value (if present).</summary>
    public string? FixedValue { get; set; }

    /// <summary>Optional human-readable documentation (annotation).</summary>
    public string? Documentation { get; set; }

    // Nur SimpleType erlaubt (QName oder anonym)
    /// <summary>Simple type by name.</summary>
    public QualifiedName? TypeName { get; set; }

    /// <summary>Anonymous simple type definition embedded in the attribute declaration.</summary>
    public SimpleType? AnonymousType { get; set; }
}
