using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>运行日志：实时抽取原生 LogQueue 到 LogList，展示 Socket / 代理 两类日志（1:1 复用内核）。</summary>
    public sealed class LogViewModel : PageViewModel
    {
        public override string Title => "运行日志";
        public override string Description => "实时展示内核运行日志，包含封包 / 注入模块与 SOCKS 代理模块两个通道。";

        private readonly DispatcherTimer _timer;

        public ICollectionView SocketLogs { get; }
        public ICollectionView ProxyLogs { get; }

        public RelayCommand ClearSocketCommand { get; }
        public RelayCommand ClearProxyCommand { get; }

        public LogViewModel()
        {
            SocketLogs = CollectionViewSource.GetDefaultView(Socket_Cache.LogList.lstSocketLog);
            ProxyLogs = CollectionViewSource.GetDefaultView(Socket_Cache.LogList.lstProxyLog);

            ClearSocketCommand = new RelayCommand(_ => Socket_Cache.LogList.ResetLogList(Socket_Cache.System.LogType.Socket));
            ClearProxyCommand = new RelayCommand(_ => Socket_Cache.LogList.ResetLogList(Socket_Cache.System.LogType.Proxy));

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (s, e) => Drain();
            _timer.Start();
        }

        private void Drain()
        {
            try
            {
                if (!Socket_Cache.LogQueue.qSocket_Log.IsEmpty)
                    Socket_Cache.LogList.LogToList(Socket_Cache.System.LogType.Socket);

                if (!Socket_Cache.LogQueue.qProxy_Log.IsEmpty)
                    Socket_Cache.LogList.LogToList(Socket_Cache.System.LogType.Proxy);
            }
            catch
            {
                // 日志抽取失败不影响主流程
            }
        }
    }
}
