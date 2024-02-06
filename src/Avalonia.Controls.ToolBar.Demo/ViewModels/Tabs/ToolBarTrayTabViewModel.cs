using Avalonia.Layout;
using ReactiveUI;

namespace Avalonia.Controls.ToolBar.Demo.ViewModels.Tabs;

public class ToolBarTrayTabViewModel : ViewModelBase
{
    public ToolBarTrayTabViewModel()
    {
        ToolBarTrayViewModel1 = new ToolBarTrayViewModel();
        ToolBarTrayViewModel2 = new ToolBarTrayViewModel();
    }

    #region ToolBarTrayViewModel1

    private bool _isVertical1;
    private bool _isEnabled1;
    private bool _isLocked1;

    public bool IsVertical1
    {
        get => _isVertical1 = ToolBarTrayViewModel1.Orientation == Orientation.Vertical;
        set
        {
            _isVertical1 = value;
            ToolBarTrayViewModel1.Orientation = _isVertical1 ? Orientation.Vertical : Orientation.Horizontal;
            this.RaisePropertyChanged();
        }
    }

    public bool IsEnabled1
    {
        get => _isEnabled1 = ToolBarTrayViewModel1.IsEnabled;
        set
        {
            _isEnabled1 = value;
            ToolBarTrayViewModel1.IsEnabled = _isEnabled1;
            this.RaisePropertyChanged();
        }
    }

    public bool IsLocked1
    {
        get => _isLocked1 = ToolBarTrayViewModel1.IsLocked;
        set
        {
            _isLocked1 = value;
            ToolBarTrayViewModel1.IsLocked = _isLocked1;
            this.RaisePropertyChanged();
        }
    }

    public ToolBarTrayViewModel ToolBarTrayViewModel1 { get; }

    #endregion

    #region ToolBarTrayViewModel2

    private bool _isVertical2;
    private bool _isEnabled2;
    private bool _isLocked2;

    public bool IsVertical2
    {
        get => _isVertical2 = ToolBarTrayViewModel2.Orientation == Orientation.Vertical;
        set
        {
            _isVertical2 = value;
            ToolBarTrayViewModel2.Orientation = _isVertical2 ? Orientation.Vertical : Orientation.Horizontal;
            this.RaisePropertyChanged();
        }
    }

    public bool IsEnabled2
    {
        get => _isEnabled2 = ToolBarTrayViewModel2.IsEnabled;
        set
        {
            _isEnabled2 = value;
            ToolBarTrayViewModel2.IsEnabled = _isEnabled2;
            this.RaisePropertyChanged();
        }
    }

    public bool IsLocked2
    {
        get => _isLocked2 = ToolBarTrayViewModel2.IsLocked;
        set
        {
            _isLocked2 = value;
            ToolBarTrayViewModel2.IsLocked = _isLocked2;
            this.RaisePropertyChanged();
        }
    }

    public ToolBarTrayViewModel ToolBarTrayViewModel2 { get; }

    #endregion
}