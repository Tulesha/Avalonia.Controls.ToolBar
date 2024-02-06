using Avalonia.Controls.ToolBar.Demo.Helpers;
using Avalonia.Controls.ToolBar.Demo.ViewModels.Tabs;
using Avalonia.Interactivity;
using DynamicData;

namespace Avalonia.Controls.ToolBar.Demo.Views.Tabs;

public partial class ToolBarTrayTabView : UserControl
{
    public ToolBarTrayTabView()
    {
        InitializeComponent();

        DataContext = new ToolBarTrayTabViewModel();

        ToolBarTray1.ToolBars.AddRange(new[]
        {
            ControlHelper.CreateToolBar(1, 0, 0),
            ControlHelper.CreateToolBar(2, 0, 1),
            ControlHelper.CreateToolBar(3, 1, 0)
        });

        ToolBarTray2.ToolBars.AddRange(new[]
        {
            ControlHelper.CreateToolBar(1, 0, 0, true),
            ControlHelper.CreateToolBar(2, 0, 1, true),
            ControlHelper.CreateToolBar(3, 1, 0, true)
        });
    }

    private void AddToolBar1_OnClick(object? sender, RoutedEventArgs e)
    {
        ToolBarTray1.ToolBars.Add(ControlHelper.CreateToolBar(ToolBarTray1.ToolBars.Count,
            ToolBarTray1.ToolBars.Count != 0 ? ToolBarTray1.ToolBars.Last().Band + 1 : 0, 0));
    }

    private void RemoveToolBar1_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ToolBarTray1.ToolBars.RemoveAt(ToolBarTray1.ToolBars.Count - 1);
        }
        catch
        {
            // ignored
        }
    }

    private void AddToolBar2_OnClick(object? sender, RoutedEventArgs e)
    {
        ToolBarTray2.ToolBars.Add(ControlHelper.CreateToolBar(ToolBarTray2.ToolBars.Count,
            ToolBarTray2.ToolBars.Count != 0 ? ToolBarTray2.ToolBars.Last().Band + 1 : 0, 0, true));
    }

    private void RemoveToolBar2_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ToolBarTray2.ToolBars.RemoveAt(ToolBarTray2.ToolBars.Count - 1);
        }
        catch
        {
            // ignored
        }
    }
}