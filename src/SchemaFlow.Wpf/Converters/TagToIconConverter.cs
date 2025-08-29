using System.Globalization;
using System.Windows.Data;
using SchemaFlow.Model.Attributes;
using SchemaFlow.Model.ContentModels;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.SchemaContainers;
using SchemaFlow.Model.Types;

namespace SchemaFlow.Wpf.Converters;

public class TagToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Return a small glyph/emoji as an icon hint. Keep it simple to avoid extra dependencies.
        return value switch
        {
            SchemaDocument => "📄",
            ElementDecl => "🔹'",
            Element => "🔹",
            ComplexType => "🧱",
            SimpleType => "🔤",
            Compositor => "🔗",
            GroupRef => "🔗ref",
            Wildcard => "⭐",
            Model.Attributes.Attribute => "@",
            AttributeDecl => "🏷️",
            Particle => "▫️",
            _ => ""
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
