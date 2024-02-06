using System.Collections.ObjectModel;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.ToolBar.Demo.ViewModels;
using Avalonia.Controls.ToolBar.Demo.ViewModels.Items;

namespace Avalonia.Controls.ToolBar.Demo.Helpers;

public static class ControlHelper
{
    public static IEnumerable<Control> CreateDefaultItems()
    {
        return new Control[]
        {
            CreateButton(1),
            CreateButton(2),
            CreateToggleButton(3),
            CreateToggleButton(4),
            CreateMenu(5),
            CreateMenu(6)
        };
    }

    public static Button CreateButton(int number) => new Button { Content = $"Button {number}" };

    public static ToggleButton CreateToggleButton(int number) => new()
    {
        Content = $"Toggle {number}"
    };

    public static Menu CreateMenu(int number) => new()
    {
        ItemsSource = new[]
        {
            new MenuItem
            {
                Header = $"Menu {number}",
                ItemsSource = new[]
                {
                    CreateMenuItem(1),
                    CreateMenuItem(2),
                    CreateMenuItem(3)
                }
            }
        }
    };

    private static MenuItem CreateMenuItem(int number) => new() { Header = $"Sub Item{number}" };

    public static IEnumerable<CustomItemViewModel> CreateDefaultItemViewModels()
    {
        return new[]
        {
            CreateCustomItemViewModel(1),
            CreateCustomItemViewModel(2),
            CreateCustomItemViewModel(3),
            CreateCustomItemViewModel(4)
        };
    }

    public static CustomItemViewModel CreateCustomItemViewModel(int number) => new(number);

    public static Controls.ToolBar CreateToolBar(int number, int band, int bandIndex, bool viewModeled = false)
    {
        return new Controls.ToolBar
        {
            DataContext = new ToolBarViewModel
            {
                Header = $"ToolBar {number}",
                Items = new ObservableCollection<object>(viewModeled
                    ? CreateDefaultItemViewModels()
                    : CreateDefaultItems()),
                Band = band,
                BandIndex = bandIndex
            }
        };
    }
}