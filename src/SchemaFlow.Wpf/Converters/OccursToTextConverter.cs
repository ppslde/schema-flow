using System;
using System.Globalization;
using System.Windows.Data;
using SchemaFlow.Model; // Occurs

namespace SchemaFlow.Wpf.Converters;

public class OccursToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Occurs o)
        {
            return o.ToDisplay();
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
