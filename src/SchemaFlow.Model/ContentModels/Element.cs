using SchemaFlow.Model.Types;

namespace SchemaFlow.Model.ContentModels;

/// <summary>
/// Element term used within particles. Represents either a reference to a global element or a local element declaration.
/// </summary>
public record class Element : Term
{
    /// <summary>Reference to a global element declaration.</summary>
    public QualifiedName? Ref { get; set; }                 // globales Element

    /// <summary>Local element name (when declaring a local element).</summary>
    public string? Name { get; set; }               // lokal

    /// <summary>Type by name for a local element declaration.</summary>
    public QualifiedName? TypeName { get; set; }            // lokales Element: Typ per QName

    /// <summary>Anonymous type for a local element declaration.</summary>
    public TypeDefinition? InlineType { get; set; } // oder anonymer Typ

    /// <summary>Whether the local element is nillable.</summary>
    public bool Nillable { get; set; }

    /// <summary>Whether the local element is abstract.</summary>
    public bool Abstract { get; set; }

    /// <summary>Default value for a local element.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Fixed value for a local element.</summary>
    public string? FixedValue { get; set; }
}
