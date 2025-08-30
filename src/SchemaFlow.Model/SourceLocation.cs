namespace SchemaFlow.Model;

/// <summary>
/// Position eines Artefakts im Quell-XSD. Ermöglicht schnelle Navigation/Anzeige.
/// </summary>
public sealed record SourceLocation
{
    /// <summary>
    /// URI/Dateipfad des XSD-Dokuments (wie übergeben/aufgelöst). Optional.
    /// </summary>
    public string? DocumentUri { get; init; }

    /// <summary>Startzeile (1-basiert).</summary>
    public int Line { get; init; }

    /// <summary>Startspalte (1-basiert).</summary>
    public int Column { get; init; }

    /// <summary>Optionale Endzeile (1-basiert), falls bekannt.</summary>
    public int? EndLine { get; init; }

    /// <summary>Optionale Endspalte (1-basiert), falls bekannt.</summary>
    public int? EndColumn { get; init; }
}
