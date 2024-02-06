using Avalonia.Styling;

namespace Avalonia.Controls.ToolBar.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ThemeVariants.SelectedItem = Application.Current!.RequestedThemeVariant;
        ThemeVariants.SelectionChanged += (_, _) =>
        {
            if (ThemeVariants.SelectedItem is ThemeVariant themeVariant)
            {
                Application.Current.RequestedThemeVariant = themeVariant;
            }
        };
    }
}