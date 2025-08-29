using SchemaFlow.Model.GlobalDefinitions;

namespace SchemaFlow.Model.Attributes;

/// <summary>
/// Use of an attribute within a complex type: either a reference to a global attribute or a local attribute declaration.
/// </summary>
public record class Attribute
{
    /// <summary>Use kind (optional, required, prohibited).</summary>
    public Kind Use { get; set; } = Kind.Optional;

    // Entweder Referenz auf globales Attribut ODER lokale Deklaration
    /// <summary>Reference to a global attribute declaration (preferred if available).</summary>
    public QualifiedName? Ref { get; set; }              // bevorzugt, wenn global vorhanden

    /// <summary>Local attribute declaration, used when not referencing a global attribute.</summary>
    public AttributeDecl? LocalAttribute { get; set; } // wenn lokal definiert

    public enum Kind { Optional, Required, Prohibited }
}
