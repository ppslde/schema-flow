using System;
using System.Globalization;
using System.Windows.Data;
using SchemaFlow.Model;
using System.Windows;

namespace SchemaFlow.Templates.Default.Converters;

public class OccursToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Occurs o)
            return o.ToDisplay();
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
