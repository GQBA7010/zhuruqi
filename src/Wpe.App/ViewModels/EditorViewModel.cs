using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>封包编辑器（主程序侧 / 代理模式）：实时列表 + 十六进制详情 + 工具栏。</summary>
    public sealed class EditorViewModel : PageViewModel
    {
        public override string Title => "封包编辑器";
        public override string Description => "实时拦截、查看与修改 WinSock 封包，消息队列缓存显示。";

        /// <summary>直接复用原生内核的接收列表，保证与注入模式 1:1 同源。</summary>
        public ICollectionView Packets { get; }

        private readonly Action<string> _navigate;

        public RelayCommand CleanUpCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand FilterCommand { get; }
        public RelayCommand SendCommand { get; }
        public RelayCommand FindCommand { get; }
        public RelayCommand ClearSearchCommand { get; }

        public EditorViewModel(Action<string> navigate = null)
        {
            _navigate = navigate;
            Packets = CollectionViewSource.GetDefaultView(Socket_Cache.SocketList.lstRecPacket);

            if (Socket_Cache.SocketList.lstRecPacket is INotifyCollectionChanged incc)
                incc.CollectionChanged += OnPacketsChanged;

            CleanUpCommand = new RelayCommand(_ => CleanUp());
            SaveCommand = new RelayCommand(_ => Save());
            FilterCommand = new RelayCommand(_ => _navigate?.Invoke("filter"));
            SendCommand = new RelayCommand(_ => _navigate?.Invoke("send"));
            FindCommand = new RelayCommand(_ => ApplySearch());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch(), _ => _searchActive);

            RefreshStats();
        }

        // ==== 查找（复用原生 FindOptions，保证 1:1） ====

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _searchIsHex;
        public bool SearchIsHex
        {
            get => _searchIsHex;
            set => SetProperty(ref _searchIsHex, value);
        }

        private bool _searchActive;

        private byte[] _searchBytes;

        private void ApplySearch()
        {
            try
            {
                if (string.IsNullOrEmpty(_searchText))
                {
                    ClearSearch();
                    return;
                }

                _searchBytes = _searchIsHex
                    ? Socket_Operation.StringToBytes(Socket_Cache.SocketPacket.EncodingFormat.Hex, _searchText)
                    : Socket_Operation.StringToBytes(Socket_Cache.SocketPacket.EncodingFormat.UTF8, _searchText);

                Socket_Cache.SocketList.DoSearch = true;

                _searchActive = true;
                Packets.Filter = MatchPacket;
                RefreshStats();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private bool MatchPacket(object item)
        {
            if (_searchBytes == null || _searchBytes.Length == 0) return true;
            if (!(item is Socket_PacketInfo spi) || spi.PacketBuffer == null) return false;
            return IndexOfBytes(spi.PacketBuffer, _searchBytes) >= 0;
        }

        private static int IndexOfBytes(byte[] haystack, byte[] needle)
        {
            if (needle.Length == 0 || haystack.Length < needle.Length) return -1;
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                int j = 0;
                for (; j < needle.Length; j++)
                    if (haystack[i + j] != needle[j]) break;
                if (j == needle.Length) return i;
            }
            return -1;
        }

        private void ClearSearch()
        {
            _searchActive = false;
            _searchBytes = null;
            SearchText = string.Empty;
            Socket_Cache.SocketList.DoSearch = false;
            Packets.Filter = null;
            RefreshStats();
        }

        private Socket_PacketInfo _selectedPacket;
        public Socket_PacketInfo SelectedPacket
        {
            get => _selectedPacket;
            set
            {
                if (SetProperty(ref _selectedPacket, value))
                {
                    Socket_Cache.SocketList.spiSelect = value;
                    OnPropertyChanged(nameof(HexDump));
                    OnPropertyChanged(nameof(HasSelection));
                }
            }
        }

        public bool HasSelection => _selectedPacket != null;

        public bool AutoRoll
        {
            get => Socket_Cache.SocketList.AutoRoll;
            set
            {
                if (Socket_Cache.SocketList.AutoRoll != value)
                {
                    Socket_Cache.SocketList.AutoRoll = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoClear
        {
            get => Socket_Cache.SocketList.AutoClear;
            set
            {
                if (Socket_Cache.SocketList.AutoClear != value)
                {
                    Socket_Cache.SocketList.AutoClear = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _packetCount;
        public int PacketCount
        {
            get => _packetCount;
            private set => SetProperty(ref _packetCount, value);
        }

        /// <summary>选中封包的十六进制 + ASCII 转储（offset | hex | ascii）。</summary>
        public string HexDump
        {
            get
            {
                var spi = _selectedPacket;
                if (spi?.PacketBuffer == null || spi.PacketBuffer.Length == 0)
                    return string.Empty;
                return BuildHexDump(spi.PacketBuffer);
            }
        }

        private static string BuildHexDump(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 4);
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append(i.ToString("X8")).Append("  ");

                int lineLen = Math.Min(16, data.Length - i);
                for (int j = 0; j < 16; j++)
                {
                    if (j < lineLen)
                        sb.Append(data[i + j].ToString("X2")).Append(' ');
                    else
                        sb.Append("   ");
                    if (j == 7) sb.Append(' ');
                }

                sb.Append(' ');
                for (int j = 0; j < lineLen; j++)
                {
                    byte b = data[i + j];
                    sb.Append(b >= 0x20 && b < 0x7F ? (char)b : '.');
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        private void OnPacketsChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshStats();

        private void RefreshStats() => PacketCount = Socket_Cache.SocketList.lstRecPacket.Count;

        private void CleanUp()
        {
            try
            {
                Socket_Cache.SocketQueue.ResetSocketQueue();
                Socket_Cache.SocketList.lstRecPacket.Clear();
                Socket_Cache.SocketList.spiSelect = null;
                SelectedPacket = null;
                Socket_Cache.SocketPacket.TotalPackets = 0;
                Socket_Cache.SocketPacket.Total_SendBytes = 0;
                Socket_Cache.SocketPacket.Total_RecvBytes = 0;
                RefreshStats();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Save()
        {
            try
            {
                Socket_Cache.SocketList.SaveSocketList_Dialog();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
