using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using WPELibrary.Lib;

namespace Wpe.App.Services
{
    /// <summary>
    /// SOCKS 代理监听/接入循环（1:1 移植自原 WinsockPacketEditor.SocketProxy_Form），
    /// 协议握手与转发仍完全复用 Socket_Cache.SocketProxy 原生内核。
    /// </summary>
    public sealed class ProxyServerService
    {
        private Socket _server;

        public bool IsListening => Socket_Cache.SocketProxy.IsListening;

        public bool Start()
        {
            try
            {
                Socket_Cache.SocketProxy.IsListening = true;

                InitProxyStart();

                if (_server == null)
                    InitializeServerSocket();

                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name,
                    MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_142));
                return true;
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                Socket_Cache.SocketProxy.IsListening = false;

                if (_server != null)
                {
                    try { _server.Close(); }
                    catch (Exception ex) { Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message); }
                    finally { _server = null; }
                }

                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name,
                    MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_143));
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitProxyStart()
        {
            try
            {
                Socket_Cache.SocketProxy.ProxyTCP_IP = IPAddress.Any;
                Socket_Cache.SocketProxy.ProxyUDP_IP = IPAddress.Loopback;

                Socket_Cache.SocketProxy.ProxyTotal_CNT = 0;
                Socket_Cache.SocketProxy.ProxyTCP_CNT = 0;
                Socket_Cache.SocketProxy.ProxyUDP_CNT = 0;
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitializeServerSocket()
        {
            try
            {
                _server?.Close();
                _server?.Dispose();

                var ep = new IPEndPoint(IPAddress.Any, Socket_Cache.SocketProxy.ProxyPort);
                _server = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true,
                    LingerState = new LingerOption(false, 0),
                    ExclusiveAddressUse = false,
                };

                _server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _server.Bind(ep);
                _server.Listen(backlog: 1000);

                AcceptClients();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void AcceptClients()
        {
            try
            {
                if (Socket_Cache.SocketProxy.IsListening && _server != null)
                {
                    var acceptArgs = new SocketAsyncEventArgs();
                    acceptArgs.Completed += AcceptCompleted;

                    if (!_server.AcceptAsync(acceptArgs))
                        AcceptCompleted(null, acceptArgs);
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket 已关闭，正常退出
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
                Task.Delay(5000).ContinueWith(_ => AcceptClients());
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success && Socket_Cache.SocketProxy.IsListening && e.AcceptSocket != null)
                {
                    Socket_Cache.SocketProxy.HandleClient(e.AcceptSocket);
                    e.AcceptSocket = null;

                    if (Socket_Cache.SocketProxy.IsListening)
                    {
                        if (!_server.AcceptAsync(e))
                            AcceptCompleted(null, e);
                    }
                    else
                    {
                        e.Dispose();
                    }
                }
                else
                {
                    e.Dispose();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
                e.Dispose();

                if (Socket_Cache.SocketProxy.IsListening)
                    Task.Delay(1000).ContinueWith(_ => AcceptClients());
            }
        }
    }
}
