namespace SchemaFlow.Model;

/// <summary>
/// Qualified name consisting of Namespace URI, local name, and optional prefix.
/// This is used to reference globally declared schema components (types, elements, attributes, groups).
/// </summary>
public sealed record QualifiedName
{
    /// <summary>
    /// Namespace URI of the name. May be null for no namespace.
    /// </summary>
    public string? NamespaceUri { get; set; }

    /// <summary>
    /// Local name (NCName) part of the qualified name.
    /// </summary>
    public string LocalName { get; set; } = "";

    /// <summary>
    /// Optional lexical prefix. Not used for identity/equality; primarily for display.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Returns "prefix:local" if a prefix is set; otherwise "{namespaceUri}local".
    /// Intended for diagnostics, not round-tripping.
    /// </summary>
    public override string ToString()
        => string.IsNullOrEmpty(Prefix) ? $"{{{NamespaceUri}}}{LocalName}" : $"{Prefix}:{LocalName}";

    /// <summary>
    /// Returns a stable string key for dictionary lookups: "namespaceUri|local".
    /// </summary>
    public string ToKey() => $"{NamespaceUri}|{LocalName}";
}