using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SchemaFlow.Core;
using SchemaFlow.Model; // QualifiedName, ToKey
using SchemaFlow.Model.SchemaContainers;
using System.Linq;
using SchemaFlow.ViewModel;

namespace SchemaFlow.Wpf.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TreeNode> RootNodes { get; } = new();
    public ObservableCollection<string> Diagnostics { get; } = new();

    private TreeNode? _selectedNode;
    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (!ReferenceEquals(_selectedNode, value))
            {
                _selectedNode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedObject));
            }
        }
    }

    public object? SelectedObject => SelectedNode?.Tag;

    public void LoadSchemas(IEnumerable<string> paths)
    {
        var loader = new XsdSimpleDomainLoader();
        var model = loader.Load(paths);

        Diagnostics.Clear();
        foreach (var d in model.Diagnostics)
        {
            Diagnostics.Add(d);
        }

        RootNodes.Clear();
        foreach (var n in SchemaTreeBuilder.Build(model))
        {
            RootNodes.Add(n);
        }

        // reset selection
        SelectedNode = RootNodes.FirstOrDefault();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
