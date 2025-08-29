using SchemaFlow.Model; // QualifiedName, ToKey
using SchemaFlow.Model.ContentModels;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.SchemaContainers;
using SchemaFlow.Model.Types;
using SchemaFlow.ViewModel;

namespace SchemaFlow.Wpf.ViewModels;

public static class SchemaTreeBuilder
{
    public static IEnumerable<TreeNode> Build(SchemaSet model)
    {
        var docsRoot = new TreeNode("Documents");
        foreach (var d in model.Documents)
        {
            var title = string.IsNullOrEmpty(d.TargetNamespace) ? "(no namespace)" : d.TargetNamespace!;
            var docNode = new TreeNode(title, d);

            var elementsRoot = new TreeNode("Elements");
            foreach (var e in d.Elements.OrderBy(e => e.Name.ToString()))
            {
                elementsRoot.Children.Add(BuildElementDeclNode(e, model,
                    visitedTypeKeys: new HashSet<string>(),
                    visitedAnon: new HashSet<object>(ReferenceEqualityComparer.Instance)));
            }
            docNode.Children.Add(elementsRoot);

            var complexTypesRoot = new TreeNode("ComplexTypes");
            foreach (var t in d.ComplexTypes.OrderBy(t => t.Name?.ToString()))
            {
                complexTypesRoot.Children.Add(BuildComplexTypeNode(t, model,
                    new HashSet<string>(), new HashSet<object>(ReferenceEqualityComparer.Instance)));
            }
            docNode.Children.Add(complexTypesRoot);

            var simpleTypesRoot = new TreeNode("SimpleTypes");
            foreach (var t in d.SimpleTypes.OrderBy(t => t.Name?.ToString()))
            {
                simpleTypesRoot.Children.Add(BuildSimpleTypeNode(t, model,
                    new HashSet<string>(), new HashSet<object>(ReferenceEqualityComparer.Instance)));
            }
            docNode.Children.Add(simpleTypesRoot);

            docsRoot.Children.Add(docNode);
        }

        var globalsRoot = new TreeNode("Globals");
        globalsRoot.Children.Add(MakeGroup("Elements", model.GlobalElements.Values.OrderBy(e => e.Name.ToString()).Select(e => (e.Name.ToString(), (object)e))));
        globalsRoot.Children.Add(MakeGroup("ComplexTypes", model.GlobalComplexTypes.Values.OrderBy(t => t.Name!.ToString()).Select(t => (t.Name!.ToString(), (object)t))));
        globalsRoot.Children.Add(MakeGroup("SimpleTypes", model.GlobalSimpleTypes.Values.OrderBy(t => t.Name!.ToString()).Select(t => (t.Name!.ToString(), (object)t))));
        globalsRoot.Children.Add(MakeGroup("Attributes", model.GlobalAttributes.Values.OrderBy(a => a.Name.ToString()).Select(a => (a.Name.ToString(), (object)a))));
        globalsRoot.Children.Add(MakeGroup("Groups", model.GlobalGroups.Values.OrderBy(g => g.Name.ToString()).Select(g => (g.Name.ToString(), (object)g))));
        globalsRoot.Children.Add(MakeGroup("AttributeGroups", model.GlobalAttributeGroups.Values.OrderBy(g => g.Name.ToString()).Select(g => (g.Name.ToString(), (object)g))));

        return new[] { docsRoot, globalsRoot };
    }

    private static TreeNode MakeGroup(string title, IEnumerable<(string title, object tag)> items)
    {
        var n = new TreeNode(title);
        foreach (var (t, tag) in items)
        {
            n.Children.Add(new TreeNode(t, tag));
        }

        return n;
    }

    private static TreeNode BuildElementDeclNode(ElementDecl decl, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var node = new TreeNode(decl.Name.ToString(), decl);
        if (decl.TypeName is not null)
        {
            node.Children.Add(BuildTypeRefNode(decl.TypeName, model, visitedTypeKeys, visitedAnon));
        }
        else if (decl.AnonymousType is not null)
        {
            node.Children.Add(BuildTypeDefinitionNode(decl.AnonymousType, model, visitedTypeKeys, visitedAnon));
        }

        return node;
    }

    private static TreeNode BuildTypeRefNode(QualifiedName typeName, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var key = typeName.ToKey();
        return model.GlobalComplexTypes.TryGetValue(key, out var ct)
            ? BuildComplexTypeNode(ct, model, visitedTypeKeys, visitedAnon)
            : model.GlobalSimpleTypes.TryGetValue(key, out var st)
            ? BuildSimpleTypeNode(st, model, visitedTypeKeys, visitedAnon)
            : new TreeNode($"{typeName} (unresolved)", typeName);
    }

    private static TreeNode BuildTypeDefinitionNode(TypeDefinition def, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        return !visitedAnon.Add(def)
            ? new TreeNode("(anonymous type - visited)", def)
            : def switch
            {
                ComplexType ct => BuildComplexTypeNode(ct, model, visitedTypeKeys, visitedAnon),
                SimpleType st => BuildSimpleTypeNode(st, model, visitedTypeKeys, visitedAnon),
                _ => new TreeNode("(unknown type)", def)
            };
    }

    private static TreeNode BuildComplexTypeNode(ComplexType ct, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var title = ct.IsGlobal ? ct.Name!.ToString() : "(anonymous complexType)";
        var node = new TreeNode(title, ct);

        if (ct.BaseType is not null)
        {
            var baseNode = BuildTypeRefNode(ct.BaseType, model, visitedTypeKeys, visitedAnon);
            baseNode.Title = $"base: {baseNode.Title}";
            node.Children.Add(baseNode);
        }

        if (ct.Content is not null)
        {
            node.Children.Add(BuildParticleNode(ct.Content, model, visitedTypeKeys, visitedAnon));
        }

        if (ct.Attributes.Count > 0 || ct.AnyAttribute != null)
        {
            var attrs = new TreeNode("Attributes");
            foreach (var a in ct.Attributes)
            {
                var attrTitle = a.Ref is not null ? $"@{a.Ref}" : (a.LocalAttribute is not null ? $"@{a.LocalAttribute.Name}" : "@attr");
                attrs.Children.Add(new TreeNode(attrTitle, a));
            }
            if (ct.AnyAttribute is not null)
            {
                attrs.Children.Add(new TreeNode("@anyAttribute", ct.AnyAttribute));
            }

            node.Children.Add(attrs);
        }

        return node;
    }

    private static TreeNode BuildSimpleTypeNode(
        SimpleType st,
        SchemaSet model,
        HashSet<string> visitedTypeKeys,
        HashSet<object> visitedAnon)
    {
        var title = st.IsGlobal ? st.Name!.ToString() : "(anonymous simpleType)";
        var node = new TreeNode(title, st);

        if (st.Type == SimpleType.Kind.List)
        {
            var listNode = new TreeNode("list");
            if (st.ListItemType is not null)
            {
                listNode.Children.Add(BuildTypeRefNode(st.ListItemType, model, visitedTypeKeys, visitedAnon));
            }
            if (st.AnonymousListItemType is not null)
            {
                listNode.Children.Add(BuildTypeDefinitionNode(st.AnonymousListItemType, model, visitedTypeKeys, visitedAnon));
            }

            node.Children.Add(listNode);
        }
        else if (st.Type == SimpleType.Kind.Union)
        {
            var u = new TreeNode("union");
            foreach (var qn in st.UnionMemberTypes)
            {
                u.Children.Add(BuildTypeRefNode(qn, model, visitedTypeKeys, visitedAnon));
            }
            foreach (var anon in st.AnonymousUnionMembers)
            {
                u.Children.Add(BuildTypeDefinitionNode(anon, model, visitedTypeKeys, visitedAnon));
            }

            node.Children.Add(u);
        }
        else if (st.Type == SimpleType.Kind.Restriction && st.BaseType is not null)
        {
            node.Children.Add(BuildTypeRefNode(st.BaseType, model, visitedTypeKeys, visitedAnon));
        }

        return node;
    }

    private static TreeNode BuildParticleNode(Particle p, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var occursText = p.Occurs.ToDisplay();
        var term = p.Term;
        return term switch
        {
            Compositor mg => BuildCompositorNode(mg, occursText, model, visitedTypeKeys, visitedAnon),
            Element el => BuildElementTermNode(el, occursText, model, visitedTypeKeys, visitedAnon),
            GroupRef gr => BuildGroupRefNode(gr, occursText, model, visitedTypeKeys, visitedAnon),
            Wildcard wc => new TreeNode($"{occursText} any {wc.NamespaceConstraint} ({wc.ContentProcesing})", wc),
            _ => new TreeNode($"{occursText} (unknown term)", term)
        };
    }

    private static TreeNode BuildCompositorNode(Compositor mg, string occurs, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var node = new TreeNode($"{mg.Type} {occurs}", mg);
        foreach (var child in mg.Particles)
        {
            node.Children.Add(BuildParticleNode(child, model, visitedTypeKeys, visitedAnon));
        }

        return node;
    }

    private static TreeNode BuildGroupRefNode(GroupRef gr, string occurs, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        var node = new TreeNode($"{gr.GroupName} {occurs}", gr);
        var key = gr.GroupName.ToKey();

        if (model.GlobalGroups.TryGetValue(key, out var decl))
        {
            node.Children.Add(BuildCompositorNode(decl.Compositor, "", model, visitedTypeKeys, visitedAnon));
        }
        else
        {
            node.Children.Add(new TreeNode("(unresolved group)", gr));
        }

        return node;
    }

    private static TreeNode BuildElementTermNode(Element et, string occurs, SchemaSet model, HashSet<string> visitedTypeKeys, HashSet<object> visitedAnon)
    {
        if (et.Ref is not null)
        {
            var refTitle = $"{et.Ref} {occurs}";
            var refNode = new TreeNode(refTitle, et);
            var key = et.Ref.ToKey();

            if (model.GlobalElements.TryGetValue(key, out var target))
            {
                refNode.Children.Add(BuildElementDeclNode(target, model, visitedTypeKeys, visitedAnon));
            }
            else
            {
                refNode.Children.Add(new TreeNode("(unresolved element)", et));
            }

            return refNode;
        }
        else
        {
            var localTitle = $"{et.Name} {occurs}";
            var node = new TreeNode(localTitle, et);
            if (et.TypeName is not null)
            {
                node.Children.Add(BuildTypeRefNode(et.TypeName, model, visitedTypeKeys, visitedAnon));
            }
            else if (et.InlineType is not null)
            {
                node.Children.Add(BuildTypeDefinitionNode(et.InlineType, model, visitedTypeKeys, visitedAnon));
            }

            return node;
        }
    }
}
