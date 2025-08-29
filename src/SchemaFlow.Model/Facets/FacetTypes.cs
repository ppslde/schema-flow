namespace SchemaFlow.Model.Facets;

/// <summary>xsd:length</summary>
public record class LengthFacet : Facet { }

/// <summary>xsd:minLength</summary>
public record class MinLengthFacet : Facet { }

/// <summary>xsd:maxLength</summary>
public record class MaxLengthFacet : Facet { }

/// <summary>xsd:pattern</summary>
public record class PatternFacet : Facet { }

/// <summary>xsd:enumeration</summary>
public record class EnumerationFacet : Facet { }

/// <summary>xsd:whiteSpace</summary>
public record class WhiteSpaceFacet : Facet { }

/// <summary>xsd:maxInclusive</summary>
public record class MaxInclusiveFacet : Facet { }

/// <summary>xsd:maxExclusive</summary>
public record class MaxExclusiveFacet : Facet { }

/// <summary>xsd:minInclusive</summary>
public record class MinInclusiveFacet : Facet { }

/// <summary>xsd:minExclusive</summary>
public record class MinExclusiveFacet : Facet { }

/// <summary>xsd:totalDigits</summary>
public record class TotalDigitsFacet : Facet { }

/// <summary>xsd:fractionDigits</summary>
public record class FractionDigitsFacet : Facet { }