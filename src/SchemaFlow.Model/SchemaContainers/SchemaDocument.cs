using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.Types;

namespace SchemaFlow.Model.SchemaContainers;

/// <summary>
/// Represents one physical XSD document, including includes/imports and top-level declarations.
/// </summary>
public class SchemaDocument
{
    /// <summary>Target namespace of the schema (xsd:schema/@targetNamespace).</summary>
    public string? TargetNamespace { get; set; }

    /// <summary>Schema version (xsd:schema/@version).</summary>
    public string? Version { get; set; }

    // Includes/Imports als „rohe“ Info – Auflösung macht dein Loader
    /// <summary>List of xs:include locations (as given).</summary>
    public List<string> Includes { get; } = new();

    /// <summary>List of xs:import entries: optional namespace and a location.</summary>
    public List<(string? Namespace, string Location)> Imports { get; } = new();

    // Top-Level-Deklarationen (optional zusätzlich zu den Dictionaries gepflegt)
    /// <summary>Top-level element declarations in this document.</summary>
    public List<ElementDecl> Elements { get; } = new();

    /// <summary>Top-level complex type definitions in this document.</summary>
    public List<ComplexType> ComplexTypes { get; } = new();

    /// <summary>Top-level simple type definitions in this document.</summary>
    public List<SimpleType> SimpleTypes { get; } = new();

    /// <summary>Top-level attribute declarations in this document.</summary>
    public List<AttributeDecl> Attributes { get; } = new();

    /// <summary>Top-level model group declarations in this document.</summary>
    public List<CompositorDecl> Groups { get; } = new();

    /// <summary>Top-level attribute group declarations in this document.</summary>
    public List<AttributeGroupDecl> AttributeGroups { get; } = new();
    public required Dictionary<string, string> Namespaces { get; set; }
}