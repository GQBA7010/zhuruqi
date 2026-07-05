using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wpe.App.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
        StateChanged += (_, _) => UpdateMaximizeVisual();
        SourceInitialized += (_, _) => TryEnableWin11RoundedCorners();
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

    #region Windows 11 DWM 圆角

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

    /// <summary>在 Windows 11 上启用系统级窗口圆角；旧系统静默忽略，不影响拖拽/缩放/最大化。</summary>
    private void TryEnableWin11RoundedCorners()
    {
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
        catch
        {
            // 非 Win11 环境（无该属性）忽略即可。
        }
    }

    #endregion
}
