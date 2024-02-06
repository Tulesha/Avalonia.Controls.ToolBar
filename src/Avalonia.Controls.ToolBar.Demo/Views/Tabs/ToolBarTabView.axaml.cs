using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ToolBar.Demo.ViewModels.Tabs;
using Avalonia.Markup.Xaml;

namespace Avalonia.Controls.ToolBar.Demo.Views.Tabs;

public partial class ToolBarTabView : UserControl
{
    public ToolBarTabView()
    {
        InitializeComponent();

        DataContext = new ToolBarTabViewModel();
    }
}