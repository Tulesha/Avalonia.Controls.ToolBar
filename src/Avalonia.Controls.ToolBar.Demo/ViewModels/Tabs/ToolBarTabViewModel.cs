using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls.ToolBar.Demo.Helpers;
using Avalonia.Layout;
using ReactiveUI;

namespace Avalonia.Controls.ToolBar.Demo.ViewModels.Tabs;

public class ToolBarTabViewModel : ViewModelBase
{
    private string _pressedElementName = string.Empty;

    public ToolBarTabViewModel()
    {
        #region ToolBarViewModel1 Init

        ToolBarViewModel1 = new ToolBarViewModel
        {
            Items = new ObservableCollection<object>(ControlHelper.CreateDefaultItems()),
            Orientation = Orientation.Horizontal
        };

        AddNewButtonCommand = ReactiveCommand.Create(() =>
        {
            ToolBarViewModel1.Items.Add(ControlHelper.CreateButton(ToolBarViewModel1.Items.Count));
        });

        AddNewToggleCommand = ReactiveCommand.Create(() =>
        {
            ToolBarViewModel1.Items.Add(ControlHelper.CreateToggleButton(ToolBarViewModel1.Items.Count));
        });

        AddNewMenuCommand = ReactiveCommand.Create(() =>
        {
            ToolBarViewModel1.Items.Add(ControlHelper.CreateMenu(ToolBarViewModel1.Items.Count));
        });

        RemoveItemCommand =
            ReactiveCommand.Create(() => { ToolBarViewModel1.Items.RemoveAt(ToolBarViewModel1.Items.Count - 1); },
                this.WhenAnyValue(x => x.ToolBarViewModel1.Items, items => items.Count != 0));

        #endregion

        #region ToolBarViewModel2 Init

        ToolBarViewModel2 = new ToolBarViewModel
        {
            Items = new ObservableCollection<object>(ControlHelper.CreateDefaultItemViewModels()),
            Orientation = Orientation.Horizontal
        };

        AddNewViewModelItemCommand = ReactiveCommand.Create(() =>
        {
            ToolBarViewModel2.Items.Add(ControlHelper.CreateCustomItemViewModel(ToolBarViewModel2.Items.Count));
        });

        RemoveViewModelItemCommand = ReactiveCommand.Create(
            () => { ToolBarViewModel2.Items.RemoveAt(ToolBarViewModel2.Items.Count - 1); },
            this.WhenAnyValue(x => x.ToolBarViewModel2.Items, items => items.Count != 0));

        #endregion

        PressedElementCommand = ReactiveCommand.Create<string>(s => { PressedElementName = $"Pressed Element: {s}"; });
    }

    #region ToolBarViewModel1

    private bool _isVertical1;
    private bool _isEnabled1;

    public bool IsVertical1
    {
        get => _isVertical1 = ToolBarViewModel1.Orientation == Orientation.Vertical;
        set
        {
            _isVertical1 = value;
            ToolBarViewModel1.Orientation = _isVertical1 ? Orientation.Vertical : Orientation.Horizontal;
            this.RaisePropertyChanged();
        }
    }

    public bool IsEnabled1
    {
        get => _isEnabled1 = ToolBarViewModel1.IsEnabled;
        set
        {
            _isEnabled1 = value;
            ToolBarViewModel1.IsEnabled = _isEnabled1;
            this.RaisePropertyChanged();
        }
    }

    public ReactiveCommand<string, Unit> PressedElementCommand { get; }
    public ICommand AddNewButtonCommand { get; }
    public ICommand AddNewToggleCommand { get; }
    public ICommand AddNewMenuCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ToolBarViewModel ToolBarViewModel1 { get; }

    #endregion

    #region ToolBarViewModel2

    private bool _isVertical2;
    private bool _isEnabled2;

    public bool IsVertical2
    {
        get => _isVertical2 = ToolBarViewModel2.Orientation == Orientation.Vertical;
        set
        {
            _isVertical2 = value;
            ToolBarViewModel2.Orientation = _isVertical2 ? Orientation.Vertical : Orientation.Horizontal;
            this.RaisePropertyChanged();
        }
    }

    public bool IsEnabled2
    {
        get => _isEnabled2 = ToolBarViewModel2.IsEnabled;
        set
        {
            _isEnabled2 = value;
            ToolBarViewModel2.IsEnabled = _isEnabled2;
            this.RaisePropertyChanged();
        }
    }

    public ICommand AddNewViewModelItemCommand { get; }
    public ICommand RemoveViewModelItemCommand { get; }
    public ToolBarViewModel ToolBarViewModel2 { get; }

    #endregion

    public string PressedElementName
    {
        get => _pressedElementName;
        set => this.RaiseAndSetIfChanged(ref _pressedElementName, value);
    }
}