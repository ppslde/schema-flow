using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using SchemaFlow.Core;
using SchemaFlow.Model;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.SchemaContainers;
using SchemaFlow.Model.Types;

namespace SchemaFlow.Wpf.Converters;

public sealed class RawXsdConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        // Strongly-typed cases
        if (value is ElementDecl ed && RawXsdProvider.TryGetRawFragment(ed, out var xmlEd))
        {
            return xmlEd;
        }

        if (value is TypeDefinition td && RawXsdProvider.TryGetRawFragment(td, out var xmlTd))
        {
            return xmlTd;
        }

        if (value is SchemaDocument doc && !string.IsNullOrWhiteSpace(doc.DocumentUri) &&
            RawXsdProvider.TryGetDocumentText(doc.DocumentUri, out var docText))
        {
            return docText;
        }

        // Reflection fallback for objects exposing a SourceLocation property
        var srcProp = value.GetType().GetProperty("Source", BindingFlags.Public | BindingFlags.Instance);
        if (srcProp?.GetValue(value) is SourceLocation src && RawXsdProvider.TryGetRawFragment(src, out var xml))
        {
            return xml;
        }

        return "No raw XSD available for this item (no Source/DocumentUri).";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
