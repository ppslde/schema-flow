using SchemaFlow.Model.ContentModels;

namespace SchemaFlow.Model.Types;

/// <summary>
/// Complex type definition, with optional derivation and content model (particles/attributes).
/// </summary>
public record class ComplexType : TypeDefinition
{
    /// <summary>Whether the type is abstract (cannot be used directly).</summary>
    public bool Abstract { get; set; }

    /// <summary>Whether the type allows mixed content (character data interleaved with elements).</summary>
    public bool Mixed { get; set; }

    // Vereinfachung: Basis-Typ als QName (wenn Derivation), Inhalt als Particle (optional)
    /// <summary>
    /// Base type QName if derived; null if no explicit derivation is modeled.
    /// </summary>
    public QualifiedName? BaseType { get; set; } // null = keiner

    /// <summary>
    /// Derivation method: "extension", "restriction", or null. Kept as string for simplicity.
    /// </summary>
    public string? DerivationMethod { get; set; } // "extension" | "restriction" | null

    /// <summary>
    /// Content particle (sequence/choice/all/element/wildcard/group). Null means empty content.
    /// </summary>
    public Particle? Content { get; set; } // sequence/choice/all/...; null = empty content

    /// <summary>
    /// Attribute uses (local attributes, references, and attribute groups expanded by the loader).
    /// </summary>
    public List<Attributes.Attribute> Attributes { get; } = new();

    /// <summary>Optional wildcard for attributes (xsd:anyAttribute).</summary>
    public Wildcard? AnyAttribute { get; set; }
}
