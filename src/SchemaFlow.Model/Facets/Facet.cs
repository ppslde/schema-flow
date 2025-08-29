namespace SchemaFlow.Model.Facets;

/// <summary>
/// Base class for simple type restriction facets. <see cref="Value"/> holds the lexical value.
/// </summary>
public abstract record class Facet
{
    /// <summary>Lexical value of the facet.</summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Whether the facet is fixed (cannot be overridden downstream) where applicable (e.g., whiteSpace).
    /// </summary>
    public bool IsFixed { get; set; } // für length/whiteSpace etc.
}