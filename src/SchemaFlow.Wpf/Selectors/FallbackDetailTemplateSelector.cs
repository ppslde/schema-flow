using System.Windows;
using System.Windows.Controls;

namespace SchemaFlow.Wpf.Selectors;

/// <summary>
/// Uses WPF's default template resolution and falls back to a provided template when none is found.
/// </summary>
public sealed class FallbackDetailTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FallbackTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        // Let WPF resolve a template first (via implicit DataTemplates in merged dictionaries)
        var element = container as FrameworkElement;
        if (element is not null && item is not null)
        {
            var t = element.TryFindResource(new DataTemplateKey(item.GetType())) as DataTemplate;
            if (t is not null)
            {
                return t;
            }
        }

        return FallbackTemplate ?? base.SelectTemplate(item, container);
    }
}
