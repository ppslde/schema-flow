namespace SchemaFlow.Model.ContentModels;

/// <summary>
/// Particle combines a term with min/max occurrence constraints.
/// </summary>
public record class Particle
{
    /// <summary>The underlying term (element, group, wildcard, or inline model group).</summary>
    public Term Term { get; set; } = new Element(); // default

    /// <summary>Occurrence constraints for this particle.</summary>
    public Occurs Occurs { get; set; } = Occurs.ExactlyOne;
}