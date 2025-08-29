using SchemaFlow.Model.Facets;

namespace SchemaFlow.Model.Types;

/// <summary>
/// Simple type definition. Supports restriction facets, list item type, and unions.
/// </summary>
public record class SimpleType : TypeDefinition
{
    /// <summary>Construction kind of this simple type. Default is Restriction.</summary>
    public Kind Type { get; set; } = Kind.Restriction;

    // Für Restriction
    /// <summary>Base simple type for restriction (e.g., xsd:string).</summary>
    public QualifiedName? BaseType { get; set; }    // z. B. xsd:string

    /// <summary>Applied facets for restriction.</summary>
    public List<Facet> Facets { get; } = new();

    // Für List
    /// <summary>Item type QName for a list simple type.</summary>
    public QualifiedName? ListItemType { get; set; }     // QName des Item-Typs (oder)

    /// <summary>Anonymous item type for a list simple type.</summary>
    public SimpleType? AnonymousListItemType { get; set; } // anonymer Item-Typ

    // Für Union
    /// <summary>Member type QNames for a union simple type.</summary>
    public List<QualifiedName> UnionMemberTypes { get; } = new();         // QNames

    /// <summary>Anonymous member types for a union simple type.</summary>
    public List<SimpleType> AnonymousUnionMembers { get; } = new(); // anonyme Member

    public enum Kind { BuiltIn, Restriction, List, Union }
}
