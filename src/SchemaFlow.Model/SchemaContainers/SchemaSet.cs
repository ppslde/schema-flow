using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.Types;

namespace SchemaFlow.Model.SchemaContainers;

/// <summary>
/// Container for a set of XSD documents and quick-lookups of global components.
/// </summary>
public class SchemaSet
{
    /// <summary>
    /// All schema documents that are part of this set.
    /// </summary>
    public List<SchemaDocument> Documents { get; } = new();

    // Schnelle Lookups (Key = QName.ToKey())
    /// <summary>Global element declarations by qualified name key.</summary>
    public Dictionary<string, ElementDecl> GlobalElements { get; } = new();

    /// <summary>Global complex type definitions by qualified name key.</summary>
    public Dictionary<string, ComplexType> GlobalComplexTypes { get; } = new();

    /// <summary>Global simple type definitions by qualified name key.</summary>
    public Dictionary<string, SimpleType> GlobalSimpleTypes { get; } = new();

    /// <summary>Global attribute declarations by qualified name key.</summary>
    public Dictionary<string, AttributeDecl> GlobalAttributes { get; } = new();

    /// <summary>Global model group declarations by qualified name key.</summary>
    public Dictionary<string, CompositorDecl> GlobalGroups { get; } = new();

    /// <summary>Global attribute group declarations by qualified name key.</summary>
    public Dictionary<string, AttributeGroupDecl> GlobalAttributeGroups { get; } = new();

    /// <summary>
    /// Diagnostics and informational messages collected by loaders/validators.
    /// </summary>
    public List<string> Diagnostics { get; } = new(); // einfache Meldungen
}