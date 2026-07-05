using System.Collections.ObjectModel;
using Wpe.App.Mvvm;
using Wpe.App.Services;

namespace Wpe.App.ViewModels;

/// <summary>主窗口 ViewModel：侧边导航 + 内容区。</summary>
public sealed class ShellViewModel : ObservableObject
{
    public ObservableCollection<NavigationItem> PrimaryItems { get; }
    public ObservableCollection<NavigationItem> FooterItems { get; }

    private NavigationItem? _selectedItem;
    public NavigationItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value is not null)
                CurrentContent = value.Content;
        }
    }

    private object? _currentContent;
    public object? CurrentContent
    {
        get => _currentContent;
        private set => SetProperty(ref _currentContent, value);
    }

    /// <summary>按导航键切换页面（供概览页的模式快捷入口使用）。</summary>
    public void NavigateTo(string key)
    {
        foreach (var item in PrimaryItems)
            if (item.Key == key) { SelectedItem = item; return; }
        foreach (var item in FooterItems)
            if (item.Key == key) { SelectedItem = item; return; }
    }

    public ShellViewModel()
    {
        PrimaryItems = new ObservableCollection<NavigationItem>
        {
            new("home",    "概览",       "\uE80F", () => new HomeViewModel(NavigateTo)),
            new("editor",  "封包编辑器", "\uE7C3", () => new EditorViewModel(NavigateTo)),
            new("inject",  "进程注入",   "\uE945", () => new InjectViewModel()),
            new("proxy",   "SOCKS 代理", "\uE968", () => new ProxyViewModel()),
            new("filter",  "滤镜",       "\uE71C", () => new FilterViewModel()),
            new("robot",   "机器人",     "\uE99A", () => new RobotViewModel()),
            new("send",    "发送",       "\uE724", () => new SendViewModel()),
            new("map",     "端口映射",   "\uE704", () => new MapViewModel()),
            new("account", "代理账号",   "\uE77B", () => new AccountViewModel()),
        };

        FooterItems = new ObservableCollection<NavigationItem>
        {
            new("log",      "运行日志", "\uE7C4", () => new PlaceholderPageViewModel("运行日志", "实时记录并支持导出运行日志。", "P3")),
            new("settings", "设置",     "\uE713", () => new PlaceholderPageViewModel("设置", "系统配置、远程管理、多语言与备份恢复。", "P2")),
            new("about",    "关于",     "\uE946", () => new AboutViewModel()),
        };

        SelectedItem = PrimaryItems[0];
    }
}
