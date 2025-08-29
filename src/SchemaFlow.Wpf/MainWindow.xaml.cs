using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using SchemaFlow.Wpf.ViewModels;
using SchemaFlow.ViewModel.Plugins;

namespace SchemaFlow.Wpf;

public partial class MainWindow : Window
{
    private readonly List<IDetailTemplateProvider> _providers = new();
    private AppSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        _settings = SettingsService.Load();
        LoadTemplateProviders();
        TemplateCombo.SelectionChanged += TemplateCombo_SelectionChanged;
        ApplySelectedProvider();
    }

    private void LoadTemplateProviders()
    {
        // Discover providers from local Plugins folder (*.dll)
        var baseDir = AppContext.BaseDirectory;
        var pluginsDir = Path.Combine(baseDir, "Plugins");
        if (Directory.Exists(pluginsDir))
        {
            foreach (var dll in Directory.EnumerateFiles(pluginsDir, "*.dll"))
            {
                TryLoadProvidersFromAssemblyPath(dll);
            }
        }

        TemplateCombo.ItemsSource = _providers;

        if (!string.IsNullOrEmpty(_settings.SelectedTemplateProvider))
        {
            var idx = _providers.FindIndex(p => string.Equals(p.Name, _settings.SelectedTemplateProvider, System.StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                TemplateCombo.SelectedIndex = idx;
        }
        if (TemplateCombo.SelectedIndex < 0 && TemplateCombo.Items.Count > 0)
            TemplateCombo.SelectedIndex = 0;
    }

    private void TryLoadProvidersFromAssemblyPath(string assemblyPath)
    {
        try
        {
            var asm = Assembly.LoadFrom(assemblyPath);
            AddProvidersFromAssembly(asm);
        }
        catch { }
    }

    private void AddProvidersFromAssembly(Assembly asm)
    {
        foreach (var t in asm.GetTypes())
        {
            if (typeof(IDetailTemplateProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            {
                if (Activator.CreateInstance(t) is IDetailTemplateProvider p)
                    _providers.Add(p);
            }
        }
    }

    private void TemplateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySelectedProvider();
        var provider = TemplateCombo.SelectedItem as IDetailTemplateProvider;
        _settings.SelectedTemplateProvider = provider?.Name;
        SettingsService.Save(_settings);
    }

    private void ApplySelectedProvider()
    {
        var provider = TemplateCombo.SelectedItem as IDetailTemplateProvider
                       ?? _providers.FirstOrDefault();
        if (provider == null) return;

        for (int i = Resources.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            var src = Resources.MergedDictionaries[i].Source?.ToString() ?? string.Empty;
            if (src.Contains("Resources/DetailTemplates.xaml", System.StringComparison.OrdinalIgnoreCase))
                Resources.MergedDictionaries.RemoveAt(i);
        }

        var dict = provider.Load();
        Resources.MergedDictionaries.Add(dict);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "XSD files (*.xsd)|*.xsd|All files (*.*)|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog(this) == true)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.LoadSchemas(dlg.FileNames);
            }
        }
    }

    private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.SelectedNode = e.NewValue as SchemaFlow.ViewModel.TreeNode;
        }
    }
}
