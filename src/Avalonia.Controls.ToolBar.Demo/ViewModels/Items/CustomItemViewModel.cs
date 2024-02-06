using ReactiveUI;

namespace Avalonia.Controls.ToolBar.Demo.ViewModels.Items;

public class CustomItemViewModel : ViewModelBase
{
    private string _text;
    private string _buttonContent;

    public CustomItemViewModel(int number)
    {
        _text = $"{number}+{number} =";
        _buttonContent = (number + number).ToString();
    }

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public string ButtonContent
    {
        get => _buttonContent;
        set => this.RaiseAndSetIfChanged(ref _buttonContent, value);
    }
}