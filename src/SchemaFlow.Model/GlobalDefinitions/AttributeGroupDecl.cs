using SchemaFlow.Model.Attributes;
using SchemaFlow.Model.ContentModels;

namespace SchemaFlow.Model.GlobalDefinitions;

/// <summary>
/// Global attribute group declaration, containing attribute uses and an optional wildcard.
/// </summary>
public record class AttributeGroupDecl
{
    /// <summary>Qualified name of the attribute group.</summary>
    public QualifiedName Name { get; set; } = new();

    /// <summary>Attributes included in the group (direct members; nested groups are to be expanded by loaders).</summary>
    public List<Attributes.Attribute> Attributes { get; } = new();

    /// <summary>Optional wildcard for attributes (xsd:anyAttribute).</summary>
    public Wildcard? AnyAttribute { get; set; }
}