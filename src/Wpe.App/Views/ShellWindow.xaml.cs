using System.Windows;
using System.Windows.Media;

namespace Wpe.App.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
        StateChanged += (_, _) => UpdateMaximizeVisual();
    }

    private void OnMinimize(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    /// <summary>最大化时补偿 WindowChrome 溢出边距，并切换图标为“还原”。</summary>
    private void UpdateMaximizeVisual()
    {
        if (WindowState == WindowState.Maximized)
        {
            RootBorder.Margin = new Thickness(7);
            RootBorder.BorderThickness = new Thickness(0);
            MaxIcon.Data = (Geometry)FindResource("Icon.Restore");
            BtnMax.ToolTip = "还原";
        }
        else
        {
            RootBorder.Margin = new Thickness(0);
            RootBorder.BorderThickness = new Thickness(1);
            MaxIcon.Data = (Geometry)FindResource("Icon.Maximize");
            BtnMax.ToolTip = "最大化";
        }
    }
}
