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

    public ShellViewModel()
    {
        PrimaryItems = new ObservableCollection<NavigationItem>
        {
            new("home",    "概览",       "\uE80F", () => new PlaceholderPageViewModel("概览 / 启动模式", "选择进程注入或 SOCKS 代理模式，查看运行状态。", "P2")),
            new("editor",  "封包编辑器", "\uE7C3", () => new PlaceholderPageViewModel("封包编辑器", "实时拦截、查看与修改 WinSock 封包，消息队列缓存显示。", "P2")),
            new("inject",  "进程注入",   "\uE945", () => new InjectViewModel()),
            new("proxy",   "SOCKS 代理", "\uE968", () => new PlaceholderPageViewModel("SOCKS 代理", "代理模式拦截封包，支持多协议、SSL、代理链。", "P4")),
            new("filter",  "滤镜",       "\uE71C", () => new PlaceholderPageViewModel("高级滤镜", "自定义拦截规则，可修改封包长度与次数。", "P3")),
            new("robot",   "机器人",     "\uE99A", () => new PlaceholderPageViewModel("自动化机器人", "满足触发条件时执行预定义指令集。", "P3")),
            new("send",    "发送",       "\uE724", () => new PlaceholderPageViewModel("封包发送", "批量发送封包，自定义顺序与循环次数。", "P3")),
            new("map",     "端口映射",   "\uE704", () => new PlaceholderPageViewModel("端口映射", "本地/远程端口映射（MapLocal / MapRemote）。", "P4")),
            new("account", "代理账号",   "\uE77B", () => new PlaceholderPageViewModel("代理账号", "账号、时长、设备、授权与登录链接管理。", "P4")),
        };

        FooterItems = new ObservableCollection<NavigationItem>
        {
            new("log",      "运行日志", "\uE7C4", () => new PlaceholderPageViewModel("运行日志", "实时记录并支持导出运行日志。", "P3")),
            new("settings", "设置",     "\uE713", () => new PlaceholderPageViewModel("设置", "系统配置、远程管理、多语言与备份恢复。", "P2")),
            new("about",    "关于",     "\uE946", () => new PlaceholderPageViewModel("关于 WPE x64", "Winsock Packet Editor · 商业生产级重构版。", "P5")),
        };

        SelectedItem = PrimaryItems[0];
    }
}
