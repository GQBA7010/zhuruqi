using System;
using System.Diagnostics;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels;

/// <summary>关于页：版本、内核信息与官网链接。</summary>
public sealed class AboutViewModel : PageViewModel
{
    public override string Title => "关于 WPE x64";
    public override string Description => "Winsock Packet Editor · 商业生产级重构版（C# + WPF / .NET Framework 4.8）。";

    public string Version => Socket_Operation.AssemblyVersion;
    public string WebSite => Socket_Cache.System.WPE64_URL;

    public AboutViewModel()
    {
        OpenWebsiteCommand = new RelayCommand(() => OpenUrl(Socket_Cache.System.WPE64_URL));
    }

    public RelayCommand OpenWebsiteCommand { get; }

    private static void OpenUrl(string url)
    {
        try { Process.Start(url); }
        catch (Exception ex) { Socket_Operation.DoLog(nameof(OpenUrl), ex.Message); }
    }
}
