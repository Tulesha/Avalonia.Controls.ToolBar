using Avalonia.Layout;
using ReactiveUI;

namespace Avalonia.Controls.ToolBar.Demo.ViewModels;

public class ToolBarTrayViewModel : ViewModelBase
{
    private bool _isEnabled = true;
    private bool _isLocked;
    private Orientation _orientation = Orientation.Horizontal;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => this.RaiseAndSetIfChanged(ref _isLocked, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set => this.RaiseAndSetIfChanged(ref _orientation, value);
    }
}