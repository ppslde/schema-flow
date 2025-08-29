using System.Collections.ObjectModel;

namespace SchemaFlow.ViewModel;

public class TreeNode
{
    public string Title { get; set; }
    public ObservableCollection<TreeNode> Children { get; } = new();
    public object? Tag { get; set; }

    public TreeNode(string title, object? tag = null)
    {
        Title = title;
        Tag = tag;
    }
}
