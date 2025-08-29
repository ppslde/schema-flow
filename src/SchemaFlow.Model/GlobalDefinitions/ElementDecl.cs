using SchemaFlow.Model.Types;

namespace SchemaFlow.Model.GlobalDefinitions;

/// <summary>
/// Global element declaration.
/// </summary>
public record class ElementDecl
{
    /// <summary>Qualified name of the element.</summary>
    public QualifiedName Name { get; set; } = new();

    /// <summary>Whether the element is abstract.</summary>
    public bool Abstract { get; set; }

    /// <summary>Whether the element is nillable.</summary>
    public bool Nillable { get; set; }

    /// <summary>Default value (if present).</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Fixed value (if present).</summary>
    public string? FixedValue { get; set; }

    /// <summary>Optional human-readable documentation (annotation).</summary>
    public string? Documentation { get; set; }

    // Typ entweder per QName (globaler Typ) oder anonym
    /// <summary>Type by name (global simple/complex type).</summary>
    public QualifiedName? TypeName { get; set; }

    /// <summary>Anonymous type definition embedded in the element declaration.</summary>
    public TypeDefinition? AnonymousType { get; set; }

    // Optional: Substitution Group
    /// <summary>Optional substitution group head.</summary>
    public QualifiedName? SubstitutionGroupHead { get; set; }
}