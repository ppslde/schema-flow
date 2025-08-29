namespace SchemaFlow.Model.ContentModels;

/// <summary>
/// Wildcard term (xsd:any or xsd:anyAttribute) with simplified namespace constraint and processing rules.
/// </summary>
public record class Wildcard : Term
{
    // Sehr einfache Darstellung der Namespace-Constraints (z. B. "##any", "##other", "ns1 ns2")
    /// <summary>
    /// Namespace constraint expression ("##any", "##other", or space-separated list of namespaces).
    /// </summary>
    public string NamespaceConstraint { get; set; } = "##any";

    /// <summary>Process contents behavior: strict, lax, or skip.</summary>
    public ProcessingType ContentProcesing { get; set; } = ProcessingType.Strict;

    public enum ProcessingType { Strict, Lax, Skip }
}