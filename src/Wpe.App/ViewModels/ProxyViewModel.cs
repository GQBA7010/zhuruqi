using System;
using System.Reflection;
using System.Windows.Threading;
using Wpe.App.Mvvm;
using Wpe.App.Services;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>SOCKS 代理模式：监听配置 + 启停 + 实时连接/流量统计（复用原生 Socket_Cache.SocketProxy 内核）。</summary>
    public sealed class ProxyViewModel : PageViewModel
    {
        public override string Title => "SOCKS 代理";
        public override string Description => "以 SOCKS5 代理方式拦截封包，支持账号认证与外部代理链，无需注入即可捕获流量。";

        private readonly ProxyServerService _server = new ProxyServerService();
        private readonly DispatcherTimer _timer;

        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }

        public ProxyViewModel()
        {
            StartCommand = new RelayCommand(_ => Start(), _ => !IsListening);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsListening);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _timer.Tick += (s, e) => RefreshStats();
            _timer.Start();
        }

        public ushort ProxyPort
        {
            get => Socket_Cache.SocketProxy.ProxyPort;
            set { if (Socket_Cache.SocketProxy.ProxyPort != value) { Socket_Cache.SocketProxy.ProxyPort = value; OnPropertyChanged(); } }
        }

        public bool EnableAuth
        {
            get => Socket_Cache.SocketProxy.Enable_Auth;
            set { if (Socket_Cache.SocketProxy.Enable_Auth != value) { Socket_Cache.SocketProxy.Enable_Auth = value; OnPropertyChanged(); } }
        }

        public bool NoRecord
        {
            get => Socket_Cache.SocketProxy.NoRecord;
            set { if (Socket_Cache.SocketProxy.NoRecord != value) { Socket_Cache.SocketProxy.NoRecord = value; OnPropertyChanged(); } }
        }

        public bool SpeedMode
        {
            get => Socket_Cache.SocketProxy.SpeedMode;
            set { if (Socket_Cache.SocketProxy.SpeedMode != value) { Socket_Cache.SocketProxy.SpeedMode = value; OnPropertyChanged(); } }
        }

        public bool EnableExternalProxy
        {
            get => Socket_Cache.SocketProxy.Enable_ExternalProxy;
            set { if (Socket_Cache.SocketProxy.Enable_ExternalProxy != value) { Socket_Cache.SocketProxy.Enable_ExternalProxy = value; OnPropertyChanged(); } }
        }

        public string ExternalProxyIP
        {
            get => Socket_Cache.SocketProxy.ExternalProxy_IP;
            set { if (Socket_Cache.SocketProxy.ExternalProxy_IP != value) { Socket_Cache.SocketProxy.ExternalProxy_IP = value; OnPropertyChanged(); } }
        }

        public ushort ExternalProxyPort
        {
            get => Socket_Cache.SocketProxy.ExternalProxy_Port;
            set { if (Socket_Cache.SocketProxy.ExternalProxy_Port != value) { Socket_Cache.SocketProxy.ExternalProxy_Port = value; OnPropertyChanged(); } }
        }

        public bool IsListening => Socket_Cache.SocketProxy.IsListening;

        public string StatusText => IsListening
            ? string.Format("正在监听 0.0.0.0:{0}", ProxyPort)
            : "未启动";

        public ulong TotalConnections => Socket_Cache.SocketProxy.ProxyTotal_CNT;
        public ulong TcpConnections => Socket_Cache.SocketProxy.ProxyTCP_CNT;
        public ulong UdpConnections => Socket_Cache.SocketProxy.ProxyUDP_CNT;
        public int UplinkSpeed => Socket_Cache.SocketProxy.ProxySpeed_Uplink;
        public int DownlinkSpeed => Socket_Cache.SocketProxy.ProxySpeed_Downlink;

        private void Start()
        {
            try
            {
                _server.Start();
                RaiseRunStateChanged();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Stop()
        {
            try
            {
                _server.Stop();
                RaiseRunStateChanged();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void RaiseRunStateChanged()
        {
            OnPropertyChanged(nameof(IsListening));
            OnPropertyChanged(nameof(StatusText));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(TotalConnections));
            OnPropertyChanged(nameof(TcpConnections));
            OnPropertyChanged(nameof(UdpConnections));
            OnPropertyChanged(nameof(UplinkSpeed));
            OnPropertyChanged(nameof(DownlinkSpeed));
        }
    }
}
