using System.Windows;
using SchemaFlow.ViewModel.Plugins;

namespace SchemaFlow.Templates.Default;

public class DefaultDetailTemplateProvider : IDetailTemplateProvider
{
    public string Name => "Default";

    public ResourceDictionary Load()
    {
        // Load an empty root dictionary from a stable pack URI so MainWindow cleanup can still detect/remove it
        var root = new ResourceDictionary
        {
            Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/DetailTemplates.xaml", System.UriKind.Relative)
        };

        // Ensure we control the merged set
        root.MergedDictionaries.Clear();

        // Converters first, then templates
        root.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/Templates.SchemaDocument.xaml", System.UriKind.Relative) });
        root.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/Templates.ElementDecl.xaml", System.UriKind.Relative) });
        root.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/Templates.Types.xaml", System.UriKind.Relative) });
        root.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/Templates.ContentModels.xaml", System.UriKind.Relative) });

        return root;
    }
}
