using System.Xml;
using System.Xml.Schema;
using SchemaFlow.Model;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.SchemaContainers;

namespace SchemaFlow.Core;

public sealed class XsdSimpleDomainLoader
{
    public SchemaSet Load(IEnumerable<string> schemaPathsOrUris)
    {
        // 1) XmlSchemaSet vorbereiten und XSDs laden
        var diags = new List<string>();
        var xmlSet = BuildXmlSchemaSet(schemaPathsOrUris, diags);

        // 2) Schema-Dokumente und Model-Gerüst anlegen
        var model = new SchemaSet();
        model.Diagnostics.AddRange(diags);

        var schemas = xmlSet.Schemas().Cast<XmlSchema>().ToList();
        var docMap = CreateDocuments(schemas, model);

        // 3) Globale Gruppen/Attributgruppen registrieren
        RegisterGlobalGroups(schemas, model, docMap);

        // 4) Typen/Attribute/Elemente und Attributgruppen-Inhalte füllen
        var agSourceMap = BuildAttributeGroupSourceMap(schemas);
        PopulateTypesAttributesAndElements(schemas, model, docMap, agSourceMap);

        return model;
    }

    // ---------- Steps ----------

    private static XmlSchemaSet BuildXmlSchemaSet(IEnumerable<string> schemaPathsOrUris, List<string> diags)
    {
        var set = new XmlSchemaSet
        {
            XmlResolver = new XmlUrlResolver()
        };

        set.ValidationEventHandler += (s, e) =>
        {
            diags.Add($"{e.Severity}: {e.Message}");
        };

        foreach (var path in schemaPathsOrUris)
        {
            var uri = TryMakeUri(path);
            set.Add(null, uri?.ToString() ?? path);
        }

        set.Compile();
        return set;
    }

    private static Dictionary<XmlSchema, SchemaDocument> CreateDocuments(List<XmlSchema> schemas, SchemaSet model)
    {
        var docMap = new Dictionary<XmlSchema, SchemaDocument>();

        foreach (var xs in schemas)
        {
            var doc = new SchemaDocument
            {
                TargetNamespace = xs.TargetNamespace,
                Version = xs.Version,
                Namespaces = xs.Namespaces?.ToArray().ToDictionary(ns => ns.Name, ns => ns.Namespace) ?? []
            };

            // Includes/Imports sammeln
            foreach (XmlSchemaObject inc in xs.Includes)
            {
                switch (inc)
                {
                    case XmlSchemaInclude i when !string.IsNullOrWhiteSpace(i.SchemaLocation):
                        doc.Includes.Add(i.SchemaLocation!);
                        break;
                    case XmlSchemaImport im when !string.IsNullOrWhiteSpace(im.SchemaLocation):
                        doc.Imports.Add((im.Namespace, im.SchemaLocation!));
                        break;
                    case XmlSchemaRedefine r when !string.IsNullOrWhiteSpace(r.SchemaLocation):
                        doc.Includes.Add(r.SchemaLocation!);
                        break;
                }
            }

            model.Documents.Add(doc);
            docMap[xs] = doc;
        }

        return docMap;
    }

    private static void RegisterGlobalGroups(List<XmlSchema> schemas, SchemaSet model, Dictionary<XmlSchema, SchemaDocument> docMap)
    {
        foreach (var xs in schemas)
        {
            var doc = docMap[xs];

            foreach (XmlSchemaObject item in xs.Items)
            {
                switch (item)
                {
                    case XmlSchemaGroup g when !g.QualifiedName.IsEmpty:
                        {
                            var decl = new CompositorDecl
                            {
                                Name = g.QualifiedName.ToQName(),
                                Compositor = g.Particle.MapCompositorBase()
                            };
                            doc.Groups.Add(decl);
                            model.GlobalGroups[decl.Name.ToKey()] = decl;
                            break;
                        }
                    case XmlSchemaAttributeGroup ag when !ag.QualifiedName.IsEmpty:
                        {
                            var decl = new AttributeGroupDecl
                            {
                                Name = ag.QualifiedName.ToQName(),
                                AnyAttribute = ag.AnyAttribute is null ? null : ag.AnyAttribute.MapWildcard()
                            };
                            doc.AttributeGroups.Add(decl);
                            model.GlobalAttributeGroups[decl.Name.ToKey()] = decl;
                            break;
                        }
                }
            }
        }
    }

    private static void PopulateTypesAttributesAndElements(
        List<XmlSchema> schemas,
        SchemaSet model,
        Dictionary<XmlSchema, SchemaDocument> docMap,
        Dictionary<string, XmlSchemaAttributeGroup> agSourceMap)
    {
        foreach (var xs in schemas)
        {
            var doc = docMap[xs];

            // Typen & globale Attribute
            foreach (XmlSchemaObject item in xs.Items)
            {
                switch (item)
                {
                    case XmlSchemaSimpleType st:
                        {
                            var mapped = st.MapSimpleType();
                            mapped.Name = st.QualifiedName.IsEmpty ? null : st.QualifiedName.ToQName();
                            doc.SimpleTypes.Add(mapped);
                            if (mapped.IsGlobal)
                            {
                                model.GlobalSimpleTypes[mapped.Name!.ToKey()] = mapped;
                            }

                            break;
                        }
                    case XmlSchemaComplexType ct:
                        {
                            var mapped = ct.MapComplexType(model);
                            mapped.Name = ct.QualifiedName.IsEmpty ? null : ct.QualifiedName.ToQName();
                            doc.ComplexTypes.Add(mapped);
                            if (mapped.IsGlobal)
                            {
                                model.GlobalComplexTypes[mapped.Name!.ToKey()] = mapped;
                            }

                            break;
                        }
                    case XmlSchemaAttribute ga when !ga.QualifiedName.IsEmpty:
                        {
                            var decl = ga.MapAttributeDecl();
                            decl.Name = ga.QualifiedName.ToQName();
                            doc.Attributes.Add(decl);
                            model.GlobalAttributes[decl.Name.ToKey()] = decl;
                            break;
                        }
                }
            }

            // Attributgruppen-Inhalte expandieren (reihenfolgenunabhängig durch agSourceMap)
            foreach (XmlSchemaAttributeGroup ag in xs.Items.OfType<XmlSchemaAttributeGroup>())
            {
                var key = ag.QualifiedName.ToQName().ToKey();
                if (!model.GlobalAttributeGroups.TryGetValue(key, out var decl))
                {
                    continue;
                }

                decl.Attributes.Clear();

                foreach (XmlSchemaObject a in ag.Attributes)
                {
                    switch (a)
                    {
                        case XmlSchemaAttribute attr:
                            decl.Attributes.Add(attr.MapAttributeUse(model));
                            break;
                        case XmlSchemaAttributeGroupRef agr:
                            ExpandAttributeGroupRef(agr.RefName.ToQName(), model, decl.Attributes, new HashSet<string>(), agSourceMap);
                            break;
                    }
                }

                if (ag.AnyAttribute is not null)
                {
                    decl.AnyAttribute = ag.AnyAttribute.MapWildcard();
                }
            }

            // Globale Elemente
            foreach (var el in xs.Items.OfType<XmlSchemaElement>())
            {
                if (el.QualifiedName.IsEmpty)
                {
                    continue;
                }

                var decl = el.MapGlobalElement(model);
                doc.Elements.Add(decl);
                model.GlobalElements[decl.Name.ToKey()] = decl;
            }
        }
    }

    // ---------- Mapping Helpers ----------

    private static Dictionary<string, XmlSchemaAttributeGroup> BuildAttributeGroupSourceMap(IEnumerable<XmlSchema> schemas)
    {
        var map = new Dictionary<string, XmlSchemaAttributeGroup>();
        foreach (var xs in schemas)
        {
            foreach (var ag in xs.Items.OfType<XmlSchemaAttributeGroup>())
            {
                if (ag.QualifiedName.IsEmpty)
                {
                    continue;
                }

                var key = ag.QualifiedName.ToQName().ToKey();
                map[key] = ag; // last one wins, consistent with XSD redefinition/override rules
            }
        }
        return map;
    }

    // Kompatibilitäts-Overload für bestehenden Aufrufcode
    internal static void ExpandAttributeGroupRef(
        QualifiedName groupName,
        SchemaSet model,
        List<Model.Attributes.Attribute> target,
        HashSet<string> visited)
        => ExpandAttributeGroupRef(groupName, model, target, visited, new Dictionary<string, XmlSchemaAttributeGroup>());

    internal static void ExpandAttributeGroupRef(
        QualifiedName groupName,
        SchemaSet model,
        List<Model.Attributes.Attribute> target,
        HashSet<string> visited,
        Dictionary<string, XmlSchemaAttributeGroup> sourceMap)
    {
        var key = groupName.ToKey();
        if (!visited.Add(key))
        {
            return; // Zyklen verhindern
        }

        if (sourceMap.TryGetValue(key, out var ag))
        {
            foreach (XmlSchemaObject a in ag.Attributes)
            {
                switch (a)
                {
                    case XmlSchemaAttribute attr:
                        target.Add(attr.MapAttributeUse(model));
                        break;
                    case XmlSchemaAttributeGroupRef agr:
                        ExpandAttributeGroupRef(agr.RefName.ToQName(), model, target, visited, sourceMap);
                        break;
                }
            }

            return;
        }

        // Fallback: bereits aufgebaute Gruppe aus dem Modell übernehmen
        if (model.GlobalAttributeGroups.TryGetValue(key, out var groupDecl))
        {
            foreach (var au in groupDecl.Attributes)
            {
                target.Add(au);
            }
        }
    }

    private static Uri? TryMakeUri(string input)
    {
        return Uri.TryCreate(input, UriKind.Absolute, out var abs) ? abs : File.Exists(input) ? new Uri(Path.GetFullPath(input)) : null;
    }
}
