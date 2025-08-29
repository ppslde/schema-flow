using System.Windows;

namespace SchemaFlow.ViewModel.Plugins;

public interface IDetailTemplateProvider
{
    string Name { get; }
    ResourceDictionary Load();
}
