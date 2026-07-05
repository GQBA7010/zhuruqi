using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>端口映射：本地映射（MapLocal）与远程映射（MapRemote），复用原生 ProxyMapping 列表。</summary>
    public sealed class MapViewModel : PageViewModel
    {
        public override string Title => "端口映射";
        public override string Description => "本地映射将请求指向本地文件，远程映射将请求转发到另一目标地址，代理运行时实时生效。";

        public ObservableCollection<Proxy_MapLocal> MapLocals { get; } = new ObservableCollection<Proxy_MapLocal>();
        public ObservableCollection<Proxy_MapRemote> MapRemotes { get; } = new ObservableCollection<Proxy_MapRemote>();

        public MapProtocolOption[] Protocols { get; } =
        {
            new MapProtocolOption(Socket_Cache.SocketProxy.MapProtocol.Http, "HTTP"),
            new MapProtocolOption(Socket_Cache.SocketProxy.MapProtocol.Https, "HTTPS"),
        };

        public RelayCommand AddLocalCommand { get; }
        public RelayCommand DeleteLocalCommand { get; }
        public RelayCommand AddRemoteCommand { get; }
        public RelayCommand DeleteRemoteCommand { get; }

        public MapViewModel()
        {
            AddLocalCommand = new RelayCommand(_ => AddLocal());
            DeleteLocalCommand = new RelayCommand(o => DeleteLocal(o as Proxy_MapLocal), o => o is Proxy_MapLocal);
            AddRemoteCommand = new RelayCommand(_ => AddRemote());
            DeleteRemoteCommand = new RelayCommand(o => DeleteRemote(o as Proxy_MapRemote), o => o is Proxy_MapRemote);

            ReloadLocal();
            ReloadRemote();
        }

        public bool EnableMapLocal
        {
            get => Socket_Cache.ProxyMapping.Enable_MapLocal;
            set { if (Socket_Cache.ProxyMapping.Enable_MapLocal != value) { Socket_Cache.ProxyMapping.Enable_MapLocal = value; OnPropertyChanged(); } }
        }

        public bool EnableMapRemote
        {
            get => Socket_Cache.ProxyMapping.Enable_MapRemote;
            set { if (Socket_Cache.ProxyMapping.Enable_MapRemote != value) { Socket_Cache.ProxyMapping.Enable_MapRemote = value; OnPropertyChanged(); } }
        }

        private void ReloadLocal()
        {
            MapLocals.Clear();
            foreach (var m in Socket_Cache.ProxyMapping.lstMapLocal)
                MapLocals.Add(m);
        }

        private void ReloadRemote()
        {
            MapRemotes.Clear();
            foreach (var m in Socket_Cache.ProxyMapping.lstMapRemote)
                MapRemotes.Add(m);
        }

        private void AddLocal()
        {
            try
            {
                Socket_Cache.ProxyMapping.AddMapLocal(
                    true, Socket_Cache.SocketProxy.MapProtocol.Http, "example.com", 80, "/", string.Empty);
                ReloadLocal();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DeleteLocal(Proxy_MapLocal pml)
        {
            try
            {
                if (pml != null)
                {
                    Socket_Cache.ProxyMapping.DelMapLocal(pml);
                    ReloadLocal();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AddRemote()
        {
            try
            {
                Socket_Cache.ProxyMapping.AddMapRemote(
                    true,
                    Socket_Cache.SocketProxy.MapProtocol.Http, "example.com", 80, "/",
                    Socket_Cache.SocketProxy.MapProtocol.Http, "127.0.0.1", 8080, "/");
                ReloadRemote();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DeleteRemote(Proxy_MapRemote pmr)
        {
            try
            {
                if (pmr != null)
                {
                    Socket_Cache.ProxyMapping.DelMapRemote(pmr);
                    ReloadRemote();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public sealed class MapProtocolOption
        {
            public MapProtocolOption(Socket_Cache.SocketProxy.MapProtocol value, string name)
            {
                Value = value;
                Name = name;
            }

            public Socket_Cache.SocketProxy.MapProtocol Value { get; }
            public string Name { get; }
        }
    }
}
