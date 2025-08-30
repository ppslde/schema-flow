using System.Xml;
using System.Xml.Schema;
using SchemaFlow.Model;
using SchemaFlow.Model.ContentModels;
using SchemaFlow.Model.Facets;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.SchemaContainers;
using SchemaFlow.Model.Types;
using Attribute = SchemaFlow.Model.Attributes.Attribute;

namespace SchemaFlow.Core;

/// <summary>
/// Provides extension methods for mapping System.Xml.Schema XSD objects to SchemaFlow model representations.
/// </summary>
public static class XsdMappingExtensions
{
    /// <summary>
    /// Creates a SourceLocation from an XmlSchemaObject if line info is available.
    /// </summary>
    private static SourceLocation? ToSourceLocation(this XmlSchemaObject o)
        => o.LineNumber > 0 && o.LinePosition > 0
            ? new SourceLocation { DocumentUri = o.SourceUri, Line = o.LineNumber, Column = o.LinePosition }
            : null;

    /// <summary>
    /// Extracts human-readable documentation text from an annotated schema object (xsd:annotation/xsd:documentation).
    /// Multiple documentation blocks are joined with double newlines.
    /// </summary>
    private static string? GetDocumentation(this XmlSchemaAnnotated annotated)
    {
        if (annotated.Annotation is null)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var item in annotated.Annotation.Items)
        {
            if (item is XmlSchemaDocumentation doc)
            {
                // Prefer markup text; if none, use the source text value
                if (doc.Markup is { Length: > 0 })
                {
                    foreach (var node in doc.Markup)
                    {
                        switch (node)
                        {
                            case XmlText t:
                                if (!string.IsNullOrWhiteSpace(t.Value))
                                {
                                    parts.Add(t.Value.Trim());
                                }

                                break;
                            case XmlCDataSection cdata:
                                if (!string.IsNullOrWhiteSpace(cdata.Value))
                                {
                                    parts.Add(cdata.Value.Trim());
                                }

                                break;
                            case XmlElement el:
                                if (!string.IsNullOrWhiteSpace(el.InnerText))
                                {
                                    parts.Add(el.InnerText.Trim());
                                }

                                break;
                            case XmlNode n:
                                if (!string.IsNullOrWhiteSpace(n.InnerText))
                                {
                                    parts.Add(n.InnerText.Trim());
                                }

                                break;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(doc.Source))
                {
                    parts.Add(doc.Source.Trim());
                }
            }
        }
        var text = string.Join("\n\n", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    /// <summary>
    /// Converts an <see cref="XmlQualifiedName"/> to a <see cref="QualifiedName"/>.
    /// </summary>
    /// <param name="qn">The XML qualified name.</param>
    /// <returns>A <see cref="QualifiedName"/> with namespace URI and local name.</returns>
    public static QualifiedName ToQName(this XmlQualifiedName qn)
        => new()
        { NamespaceUri = string.IsNullOrEmpty(qn.Namespace) ? null : qn.Namespace, LocalName = qn.Name };

    /// <summary>
    /// Maps an <see cref="XmlSchemaParticle"/> to an <see cref="Occurs"/> structure.
    /// </summary>
    /// <param name="p">The schema particle.</param>
    /// <returns>An <see cref="Occurs"/> instance representing min/max occurrence constraints.</returns>
    public static Occurs ToOccurs(this XmlSchemaParticle p)
    {
        var min = (int)p.MinOccurs;
        int? max = p is { MaxOccursString: { } s } && s.Equals("unbounded", StringComparison.OrdinalIgnoreCase) ? null : (int?)(int)p.MaxOccurs;
        return new Occurs { Min = min, Max = max };
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaAny"/> wildcard to a <see cref="Wildcard"/> model.
    /// </summary>
    /// <param name="any">The schema wildcard (xsd:any).</param>
    /// <returns>A <see cref="Wildcard"/> instance.</returns>
    public static Wildcard MapWildcard(this XmlSchemaAny any)
        => new()
        {
            NamespaceConstraint = string.IsNullOrWhiteSpace(any.Namespace) ? "##any" : any.Namespace,
            ContentProcesing = any.ProcessContents switch
            {
                XmlSchemaContentProcessing.Lax => Wildcard.ProcessingType.Lax,
                XmlSchemaContentProcessing.Skip => Wildcard.ProcessingType.Skip,
                _ => Wildcard.ProcessingType.Strict
            }
        };

    /// <summary>
    /// Maps an <see cref="XmlSchemaAnyAttribute"/> wildcard to a <see cref="Wildcard"/> model.
    /// </summary>
    /// <param name="anyAttr">The schema wildcard (xsd:anyAttribute).</param>
    /// <returns>A <see cref="Wildcard"/> instance.</returns>
    public static Wildcard MapWildcard(this XmlSchemaAnyAttribute anyAttr)
        => new()
        {
            NamespaceConstraint = string.IsNullOrWhiteSpace(anyAttr.Namespace) ? "##any" : anyAttr.Namespace,
            ContentProcesing = anyAttr.ProcessContents switch
            {
                XmlSchemaContentProcessing.Lax => Wildcard.ProcessingType.Lax,
                XmlSchemaContentProcessing.Skip => Wildcard.ProcessingType.Skip,
                _ => Wildcard.ProcessingType.Strict
            }
        };

    /// <summary>
    /// Maps a model group particle (sequence, choice, all) to a <see cref="Compositor"/>.
    /// </summary>
    /// <param name="particle">The schema particle.</param>
    /// <returns>A <see cref="Compositor"/> representing the group, or a default sequence group if not recognized.</returns>
    public static Compositor MapCompositorBase(this XmlSchemaParticle? particle)
    {
        if (particle is XmlSchemaSequence seq)
        {
            var mg = new Compositor { Type = Compositor.Kind.Sequence };
            foreach (XmlSchemaParticle child in seq.Items.OfType<XmlSchemaParticle>())
            {
                mg.Particles.Add(child.MapParticle());
            }

            return mg;
        }
        if (particle is XmlSchemaChoice choice)
        {
            var mg = new Compositor { Type = Compositor.Kind.Choice };
            foreach (XmlSchemaParticle child in choice.Items.OfType<XmlSchemaParticle>())
            {
                mg.Particles.Add(child.MapParticle());
            }

            return mg;
        }
        if (particle is XmlSchemaAll all)
        {
            var mg = new Compositor { Type = Compositor.Kind.All };
            foreach (XmlSchemaElement el in all.Items.OfType<XmlSchemaElement>())
            {
                mg.Particles.Add(el.MapParticle());
            }

            return mg;
        }
        return new Compositor { Type = Compositor.Kind.Sequence };
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaParticle"/> to a <see cref="Particle"/> model.
    /// </summary>
    /// <param name="p">The schema particle.</param>
    /// <returns>A <see cref="Particle"/> instance with the mapped term and occurrence constraints.</returns>
    public static Particle MapParticle(this XmlSchemaParticle p)
    {
        var result = new Particle { Occurs = p.ToOccurs() };
        switch (p)
        {
            case XmlSchemaElement el:
                result.Term = el.MapElementTerm();
                break;
            case XmlSchemaAny any:
                result.Term = any.MapWildcard();
                break;
            case XmlSchemaGroupRef gr:
                result.Term = new GroupRef { GroupName = gr.RefName.ToQName() };
                break;
            case XmlSchemaSequence or XmlSchemaChoice or XmlSchemaAll:
                result.Term = p.MapCompositorBase();
                break;
            default:
                result.Term = new Compositor { Type = Compositor.Kind.Sequence };
                break;
        }
        return result;
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaElement"/> to an <see cref="Element"/> for use in particles.
    /// </summary>
    /// <param name="el">The schema element.</param>
    /// <returns>An <see cref="Element"/> representing the element reference or local declaration.</returns>
    public static Element MapElementTerm(this XmlSchemaElement el)
    {
        if (!el.RefName.IsEmpty)
        {
            return new Element { Ref = el.RefName.ToQName(), Nillable = el.IsNillable, Abstract = el.IsAbstract };
        }
        var term = new Element
        {
            Name = el.Name,
            Nillable = el.IsNillable,
            Abstract = el.IsAbstract,
            DefaultValue = el.DefaultValue,
            FixedValue = el.FixedValue
        };
        if (!el.SchemaTypeName.IsEmpty)
        {
            term.TypeName = el.SchemaTypeName.ToQName();
        }
        else if (el.SchemaType is not null)
        {
            term.InlineType = el.SchemaType switch
            {
                XmlSchemaComplexType ct => ct.MapComplexType(null),
                XmlSchemaSimpleType st => st.MapSimpleType(),
                _ => null
            };
        }
        return term;
    }

    /// <summary>
    /// Maps a global <see cref="XmlSchemaElement"/> to an <see cref="ElementDecl"/>.
    /// </summary>
    /// <param name="el">The global schema element.</param>
    /// <param name="model">The schema set for type resolution.</param>
    /// <returns>An <see cref="ElementDecl"/> representing the global element declaration.</returns>
    public static ElementDecl MapGlobalElement(this XmlSchemaElement el, SchemaSet model)
    {
        var decl = new ElementDecl
        {
            Name = el.QualifiedName.ToQName(),
            Abstract = el.IsAbstract,
            Nillable = el.IsNillable,
            DefaultValue = el.DefaultValue,
            FixedValue = el.FixedValue,
            Documentation = el.GetDocumentation(),
            Source = el.ToSourceLocation()
        };
        if (!el.SchemaTypeName.IsEmpty)
        {
            decl.TypeName = el.SchemaTypeName.ToQName();
        }
        else if (el.SchemaType is not null)
        {
            decl.AnonymousType = el.SchemaType switch
            {
                XmlSchemaComplexType ct => ct.MapComplexType(model),
                XmlSchemaSimpleType st => st.MapSimpleType(),
                _ => null
            };
        }
        if (!el.SubstitutionGroup.IsEmpty)
        {
            decl.SubstitutionGroupHead = el.SubstitutionGroup.ToQName();
        }
        return decl;
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaAttribute"/> to an <see cref="AttributeDecl"/>.
    /// </summary>
    /// <param name="a">The schema attribute.</param>
    /// <returns>An <see cref="AttributeDecl"/> representing the attribute declaration.</returns>
    public static AttributeDecl MapAttributeDecl(this XmlSchemaAttribute a)
    {
        var decl = new AttributeDecl
        {
            Name = a.QualifiedName.IsEmpty ? new QualifiedName { LocalName = a.Name } : a.QualifiedName.ToQName(),
            DefaultValue = a.DefaultValue,
            FixedValue = a.FixedValue,
            Documentation = a.GetDocumentation()
        };
        if (!a.SchemaTypeName.IsEmpty)
        {
            decl.TypeName = a.SchemaTypeName.ToQName();
        }
        else if (a.SchemaType is XmlSchemaSimpleType st)
        {
            decl.AnonymousType = st.MapSimpleType();
        }

        return decl;
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaAttribute"/> to an <see cref="Attribute"/> for use in complex types.
    /// </summary>
    /// <param name="a">The schema attribute.</param>
    /// <param name="model">The schema set for type resolution.</param>
    /// <returns>An <see cref="Attribute"/> representing the attribute use.</returns>
    public static Attribute MapAttributeUse(this XmlSchemaAttribute a, SchemaSet model)
    {
        var use = new Attribute
        {
            Use = a.Use switch
            {
                XmlSchemaUse.Required => Attribute.Kind.Required,
                XmlSchemaUse.Prohibited => Attribute.Kind.Prohibited,
                _ => Attribute.Kind.Optional
            }
        };
        if (!a.RefName.IsEmpty)
        {
            use.Ref = a.RefName.ToQName();
        }
        else
        {
            use.LocalAttribute = a.MapAttributeDecl();
        }
        return use;
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaComplexType"/> to a <see cref="ComplexType"/> model.
    /// </summary>
    /// <param name="ct">The schema complex type.</param>
    /// <param name="modelIfAvailable">The schema set for type resolution, if available.</param>
    /// <returns>A <see cref="ComplexType"/> representing the complex type definition.</returns>
    public static ComplexType MapComplexType(this XmlSchemaComplexType ct, SchemaSet? modelIfAvailable)
    {
        var t = new ComplexType
        {
            Abstract = ct.IsAbstract,
            Mixed = ct.IsMixed,
            Documentation = ct.GetDocumentation(),
            Source = ct.ToSourceLocation()
        };
        if (ct.ContentModel is XmlSchemaComplexContent cc)
        {
            switch (cc.Content)
            {
                case XmlSchemaComplexContentExtension ext:
                    t.DerivationMethod = "extension";
                    if (!ext.BaseTypeName.IsEmpty)
                    {
                        t.BaseType = ext.BaseTypeName.ToQName();
                    }

                    if (ext.Particle is not null)
                    {
                        t.Content = ext.Particle.MapParticle();
                    }

                    ext.Attributes.MapAttributes(ext.AnyAttribute, t, modelIfAvailable);
                    break;
                case XmlSchemaComplexContentRestriction res:
                    t.DerivationMethod = "restriction";
                    if (!res.BaseTypeName.IsEmpty)
                    {
                        t.BaseType = res.BaseTypeName.ToQName();
                    }

                    if (res.Particle is not null)
                    {
                        t.Content = res.Particle.MapParticle();
                    }

                    res.Attributes.MapAttributes(res.AnyAttribute, t, modelIfAvailable);
                    break;
            }
        }
        else if (ct.ContentModel is XmlSchemaSimpleContent sc)
        {
            switch (sc.Content)
            {
                case XmlSchemaSimpleContentExtension ext:
                    t.DerivationMethod = "extension";
                    if (!ext.BaseTypeName.IsEmpty)
                    {
                        t.BaseType = ext.BaseTypeName.ToQName();
                    }

                    ext.Attributes.MapAttributes(ext.AnyAttribute, t, modelIfAvailable);
                    break;
                case XmlSchemaSimpleContentRestriction res:
                    t.DerivationMethod = "restriction";
                    if (!res.BaseTypeName.IsEmpty)
                    {
                        t.BaseType = res.BaseTypeName.ToQName();
                    }

                    res.Attributes.MapAttributes(res.AnyAttribute, t, modelIfAvailable);
                    break;
            }
        }
        else
        {
            if (ct.ContentType != XmlSchemaContentType.Empty && ct.ContentTypeParticle is XmlSchemaParticle p)
            {
                t.Content = p.MapParticle();
            }
            ct.Attributes.MapAttributes(ct.AnyAttribute, t, modelIfAvailable);
        }
        return t;
    }

    /// <summary>
    /// Maps a collection of <see cref="XmlSchemaAttribute"/> and <see cref="XmlSchemaAttributeGroupRef"/> to the attributes of a <see cref="ComplexType"/>.
    /// </summary>
    /// <param name="attributes">The collection of schema attributes and attribute group references.</param>
    /// <param name="anyAttr">An optional wildcard attribute (xsd:anyAttribute).</param>
    /// <param name="t">The target <see cref="ComplexType"/> to populate.</param>
    /// <param name="modelIfAvailable">The schema set for attribute group expansion, if available.</param>
    public static void MapAttributes(this XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute? anyAttr, ComplexType t, SchemaSet? modelIfAvailable)
    {
        foreach (var a in attributes)
        {
            switch (a)
            {
                case XmlSchemaAttribute attr:
                    t.Attributes.Add(attr.MapAttributeUse(modelIfAvailable!));
                    break;
                case XmlSchemaAttributeGroupRef agr when modelIfAvailable is not null:
                    XsdSimpleDomainLoader.ExpandAttributeGroupRef(agr.RefName.ToQName(), modelIfAvailable, t.Attributes, new HashSet<string>());
                    break;
            }
        }
        if (anyAttr is not null)
        {
            t.AnyAttribute = anyAttr.MapWildcard();
        }
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaSimpleType"/> to a <see cref="SimpleType"/> model.
    /// </summary>
    /// <param name="st">The schema simple type.</param>
    /// <returns>A <see cref="SimpleType"/> representing the simple type definition.</returns>
    public static SimpleType MapSimpleType(this XmlSchemaSimpleType st)
    {
        if (st.Content is XmlSchemaSimpleTypeRestriction r)
        {
            var t = new SimpleType
            {
                Type = SimpleType.Kind.Restriction,
                BaseType = r.BaseTypeName.IsEmpty ? null : r.BaseTypeName.ToQName(),
                Documentation = st.GetDocumentation(),
                Source = st.ToSourceLocation()
            };
            foreach (XmlSchemaFacet f in r.Facets)
            {
                t.Facets.Add(f.MapFacet());
            }

            return t;
        }

        if (st.Content is XmlSchemaSimpleTypeList l)
        {
            var t = new SimpleType
            {
                Type = SimpleType.Kind.List,
                ListItemType = l.ItemTypeName.IsEmpty ? null : l.ItemTypeName.ToQName(),
                Documentation = st.GetDocumentation(),
                Source = st.ToSourceLocation()
            };
            if (l.ItemType is XmlSchemaSimpleType anon)
            {
                t.AnonymousListItemType = anon.MapSimpleType();
            }

            return t;
        }

        if (st.Content is XmlSchemaSimpleTypeUnion u)
        {
            var t = new SimpleType
            {
                Type = SimpleType.Kind.Union,
                Documentation = st.GetDocumentation(),
                Source = st.ToSourceLocation()
            };
            foreach (var qn in u.MemberTypes.Where(q => !q.IsEmpty))
            {
                t.UnionMemberTypes.Add(qn.ToQName());
            }
            foreach (XmlSchemaSimpleType anon in u.BaseTypes.OfType<XmlSchemaSimpleType>())
            {
                t.AnonymousUnionMembers.Add(anon.MapSimpleType());
            }
            return t;
        }
        return new SimpleType
        {
            Type = SimpleType.Kind.BuiltIn,
            Documentation = st.GetDocumentation(),
            Source = st.ToSourceLocation()
        };
    }

    /// <summary>
    /// Maps an <see cref="XmlSchemaFacet"/> to a <see cref="Facet"/> model.
    /// </summary>
    /// <param name="f">The schema facet.</param>
    /// <returns>A <see cref="Facet"/> representing the restriction facet.</returns>
    public static Facet MapFacet(this XmlSchemaFacet f)
    {
        var value = f.Value ?? "";
        return f switch
        {
            XmlSchemaLengthFacet => new LengthFacet { Value = value, IsFixed = f.IsFixed },
            XmlSchemaMinLengthFacet => new MinLengthFacet { Value = value, IsFixed = f.IsFixed },
            XmlSchemaMaxLengthFacet => new MaxLengthFacet { Value = value, IsFixed = f.IsFixed },
            XmlSchemaPatternFacet => new PatternFacet { Value = value },
            XmlSchemaEnumerationFacet => new EnumerationFacet { Value = value },
            XmlSchemaWhiteSpaceFacet => new WhiteSpaceFacet { Value = value, IsFixed = f.IsFixed },
            XmlSchemaMaxInclusiveFacet => new MaxInclusiveFacet { Value = value },
            XmlSchemaMaxExclusiveFacet => new MaxExclusiveFacet { Value = value },
            XmlSchemaMinInclusiveFacet => new MinInclusiveFacet { Value = value },
            XmlSchemaMinExclusiveFacet => new MinExclusiveFacet { Value = value },
            XmlSchemaTotalDigitsFacet => new TotalDigitsFacet { Value = value },
            XmlSchemaFractionDigitsFacet => new FractionDigitsFacet { Value = value },
            _ => new PatternFacet { Value = value }
        };
    }
}
