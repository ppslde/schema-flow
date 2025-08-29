using System.Windows;
using SchemaFlow.ViewModel.Plugins;

namespace SchemaFlow.Templates.Default;

public class DefaultDetailTemplateProvider : IDetailTemplateProvider
{
    public string Name => "Default";

    public ResourceDictionary Load()
    {
        return new ResourceDictionary
        {
            Source = new System.Uri("/SchemaFlow.Templates.Default;component/Resources/DetailTemplates.xaml", System.UriKind.Relative)
        };
    }
}
