using System.Windows;
using System.Windows.Threading;

namespace Wpe.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "WPE x64", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
