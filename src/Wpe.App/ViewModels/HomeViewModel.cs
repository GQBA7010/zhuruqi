using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels;

/// <summary>概览 / 启动模式页：模式入口 + 远程管理 + 语言 + 备份，对齐原 SystemMode_Form。</summary>
public sealed class HomeViewModel : PageViewModel
{
    private readonly Action<string> _navigate;
    private readonly string _remoteIp;

    public override string Title => "概览 / 启动模式";
    public override string Description => "选择进程注入或 SOCKS 代理模式，配置远程管理、界面语言与数据备份。";

    public string KernelVersion => string.Format(
        MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_5), Socket_Operation.AssemblyVersion);

    public HomeViewModel(Action<string> navigate)
    {
        _navigate = navigate;
        _remoteIp = ResolveLocalIp();

        _isRemote = Socket_Cache.System.IsRemote;
        _remoteUserName = Socket_Cache.System.Remote_UserName ?? string.Empty;
        _remotePassWord = Socket_Cache.System.Remote_PassWord ?? string.Empty;
        _remotePort = Socket_Cache.System.Remote_Port;

        GoInjectCommand = new RelayCommand(() => _navigate("inject"));
        GoProxyCommand = new RelayCommand(() => _navigate("proxy"));
        SaveRemoteCommand = new RelayCommand(SaveRemote);
        ImportBackupCommand = new RelayCommand(ImportBackup);
        OpenWebsiteCommand = new RelayCommand(OpenWebsite);
        SetChineseCommand = new RelayCommand(() => SetLanguage("zh-CN"));
        SetEnglishCommand = new RelayCommand(() => SetLanguage("en-US"));
    }

    public RelayCommand GoInjectCommand { get; }
    public RelayCommand GoProxyCommand { get; }
    public RelayCommand SaveRemoteCommand { get; }
    public RelayCommand ImportBackupCommand { get; }
    public RelayCommand OpenWebsiteCommand { get; }
    public RelayCommand SetChineseCommand { get; }
    public RelayCommand SetEnglishCommand { get; }

    private bool _isRemote;
    public bool IsRemote
    {
        get => _isRemote;
        set { if (SetProperty(ref _isRemote, value)) OnPropertyChanged(nameof(RemoteUrl)); }
    }

    private string _remoteUserName;
    public string RemoteUserName
    {
        get => _remoteUserName;
        set => SetProperty(ref _remoteUserName, value);
    }

    private string _remotePassWord;
    public string RemotePassWord
    {
        get => _remotePassWord;
        set => SetProperty(ref _remotePassWord, value);
    }

    private ushort _remotePort;
    public ushort RemotePort
    {
        get => _remotePort;
        set { if (SetProperty(ref _remotePort, value)) OnPropertyChanged(nameof(RemoteUrl)); }
    }

    public string RemoteUrl => string.IsNullOrEmpty(_remoteIp)
        ? string.Empty
        : $"http://{_remoteIp}:{RemotePort}";

    public string CurrentLanguage => Socket_Cache.System.DefaultLanguage;

    private void SaveRemote()
    {
        if (IsRemote && (string.IsNullOrWhiteSpace(RemoteUserName) || string.IsNullOrWhiteSpace(RemotePassWord)))
        {
            IsRemote = false;
            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_180));
            return;
        }

        Socket_Cache.System.IsRemote = IsRemote;
        Socket_Cache.System.Remote_UserName = RemoteUserName.Trim();
        Socket_Cache.System.Remote_PassWord = RemotePassWord.Trim();
        Socket_Cache.System.Remote_Port = RemotePort;
        Socket_Cache.System.Remote_URL = RemoteUrl;
        Socket_Cache.System.SaveSystemConfig_ToDB();
    }

    private void ImportBackup()
    {
        Socket_Cache.System.ImportSystemBackUp_Dialog();
        _isRemote = Socket_Cache.System.IsRemote;
        _remoteUserName = Socket_Cache.System.Remote_UserName ?? string.Empty;
        _remotePassWord = Socket_Cache.System.Remote_PassWord ?? string.Empty;
        _remotePort = Socket_Cache.System.Remote_Port;
        OnPropertyChanged(nameof(IsRemote));
        OnPropertyChanged(nameof(RemoteUserName));
        OnPropertyChanged(nameof(RemotePassWord));
        OnPropertyChanged(nameof(RemotePort));
        OnPropertyChanged(nameof(RemoteUrl));
    }

    private void OpenWebsite()
    {
        try { Process.Start(Socket_Cache.System.WPE64_URL); }
        catch (Exception ex) { Socket_Operation.DoLog(nameof(OpenWebsite), ex.Message); }
    }

    private void SetLanguage(string language)
    {
        if (string.Equals(Socket_Cache.System.DefaultLanguage, language, StringComparison.Ordinal))
            return;

        Socket_Cache.System.DefaultLanguage = language;
        Socket_Cache.System.SaveSystemConfig_ToDB();
        Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_12));
        RestartApp();
    }

    private static void RestartApp()
    {
        try
        {
            string exe = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (!string.IsNullOrEmpty(exe))
                Process.Start(exe);
        }
        catch (Exception ex)
        {
            Socket_Operation.DoLog(nameof(RestartApp), ex.Message);
        }
        Application.Current?.Shutdown();
    }

    private static string ResolveLocalIp()
    {
        try
        {
            IPAddress[] ips = Socket_Operation.GetLocalIPAddress();
            return ips.Length > 0 ? ips[0].ToString() : "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}
