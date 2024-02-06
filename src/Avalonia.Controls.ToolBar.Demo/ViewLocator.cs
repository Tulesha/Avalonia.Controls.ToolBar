using Avalonia.Controls.Templates;
using Avalonia.Controls.ToolBar.Demo.ViewModels;

namespace Avalonia.Controls.ToolBar.Demo;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");
        if (name != null)
        {
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}