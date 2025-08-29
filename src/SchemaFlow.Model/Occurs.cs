namespace SchemaFlow.Model;

/// <summary>
/// Occurrence constraints (minOccurs/maxOccurs) for a particle. Max = null means "unbounded".
/// </summary>
public readonly record struct Occurs(int Min, int? Max)
{
    public static Occurs ExactlyOne => new(1, 1);
    public static Occurs Optional => new(0, 1);
    public static Occurs ZeroOrMore => new(0, null);
    public static Occurs OneOrMore => new(1, null);

    public string ToDisplay() => $"[{Min}..{(Max is null ? "∞" : Max.Value)}]";
}