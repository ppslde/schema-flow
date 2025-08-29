using System.Globalization;
using System.Windows.Data;
using SchemaFlow.Model;

namespace SchemaFlow.Templates.Default.Converters;

public class OccursToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Occurs o ? o.ToDisplay() : (value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
