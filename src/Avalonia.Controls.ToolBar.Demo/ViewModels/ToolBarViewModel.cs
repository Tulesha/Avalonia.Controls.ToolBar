using System.Collections.ObjectModel;
using Avalonia.Layout;
using ReactiveUI;

namespace Avalonia.Controls.ToolBar.Demo.ViewModels;

public class ToolBarViewModel : ViewModelBase
{
    private int _band;
    private int _bandIndex;
    private string? _header;
    private bool _isEnabled = true;
    private Orientation _orientation = Orientation.Horizontal;
    private ObservableCollection<object> _items = new();

    public int Band
    {
        get => _band;
        set => this.RaiseAndSetIfChanged(ref _band, value);
    }

    public int BandIndex
    {
        get => _bandIndex;
        set => this.RaiseAndSetIfChanged(ref _bandIndex, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public string? Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set => this.RaiseAndSetIfChanged(ref _orientation, value);
    }

    public ObservableCollection<object> Items
    {
        get => _items;
        set => this.RaiseAndSetIfChanged(ref _items, value);
    }
}