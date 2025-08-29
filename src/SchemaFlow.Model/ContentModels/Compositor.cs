namespace SchemaFlow.Model.ContentModels;

/// <summary>
/// Inline (anonymous) model group such as sequence/choice/all, containing child particles.
/// </summary>
public record class Compositor : Term
{
    /// <summary>Compositor: sequence, choice, or all.</summary>
    public Kind Type { get; set; } = Kind.Sequence;

    /// <summary>Child particles of the group.</summary>
    public List<Particle> Particles { get; } = new();

    public enum Kind { Sequence, Choice, All }
}