using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>代理账号体系：账号列表 + 账号编辑（时长/链接数/设备数限制），复用原生 ProxyAccount 内核。</summary>
    public sealed class AccountViewModel : PageViewModel
    {
        public override string Title => "代理账号";
        public override string Description => "管理 SOCKS5 认证账号：启停、有效期、最大链接数与设备数限制，在线状态实时展示。";

        public ICollectionView Accounts { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public AccountViewModel()
        {
            Accounts = CollectionViewSource.GetDefaultView(Socket_Cache.ProxyAccount.lstProxyAccount);

            AddCommand = new RelayCommand(_ => Add());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => _selected != null);
        }

        private Proxy_AccountInfo _selected;
        public Proxy_AccountInfo Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    RaiseEditorChanged();
                }
            }
        }

        public bool HasSelection => _selected != null;

        public bool EnableAccount
        {
            get => Socket_Cache.SocketProxy.Enable_Auth;
            set { if (Socket_Cache.SocketProxy.Enable_Auth != value) { Socket_Cache.SocketProxy.Enable_Auth = value; OnPropertyChanged(); } }
        }

        public string UserName
        {
            get => _selected?.UserName ?? string.Empty;
            set { if (_selected != null) { _selected.UserName = value; OnPropertyChanged(); Accounts.Refresh(); } }
        }

        public string PassWord
        {
            get => _selected?.PassWord ?? string.Empty;
            set { if (_selected != null) { _selected.PassWord = value; OnPropertyChanged(); } }
        }

        public bool IsEnable
        {
            get => _selected?.IsEnable ?? false;
            set { if (_selected != null) { _selected.IsEnable = value; OnPropertyChanged(); Accounts.Refresh(); } }
        }

        public bool IsLimitLinks
        {
            get => _selected?.IsLimitLinks ?? false;
            set { if (_selected != null) { _selected.IsLimitLinks = value; OnPropertyChanged(); } }
        }

        public int LimitLinks
        {
            get => _selected?.LimitLinks ?? 0;
            set { if (_selected != null) { _selected.LimitLinks = value; OnPropertyChanged(); } }
        }

        public bool IsLimitDevices
        {
            get => _selected?.IsLimitDevices ?? false;
            set { if (_selected != null) { _selected.IsLimitDevices = value; OnPropertyChanged(); } }
        }

        public int LimitDevices
        {
            get => _selected?.LimitDevices ?? 0;
            set { if (_selected != null) { _selected.LimitDevices = value; OnPropertyChanged(); } }
        }

        public bool IsExpiry
        {
            get => _selected?.IsExpiry ?? false;
            set { if (_selected != null) { _selected.IsExpiry = value; OnPropertyChanged(); } }
        }

        public DateTime ExpiryTime
        {
            get => _selected?.ExpiryTime ?? DateTime.Now;
            set { if (_selected != null) { _selected.ExpiryTime = value; OnPropertyChanged(); } }
        }

        private void RaiseEditorChanged()
        {
            foreach (var p in new[]
            {
                nameof(UserName), nameof(PassWord), nameof(IsEnable),
                nameof(IsLimitLinks), nameof(LimitLinks),
                nameof(IsLimitDevices), nameof(LimitDevices),
                nameof(IsExpiry), nameof(ExpiryTime),
            })
                OnPropertyChanged(p);
        }

        private void Add()
        {
            try
            {
                int n = Socket_Cache.ProxyAccount.lstProxyAccount.Count + 1;
                DateTime now = DateTime.Now;

                Socket_Cache.ProxyAccount.AddProxyAccount(
                    Guid.NewGuid(),
                    true,
                    "user" + n,
                    "pass" + n,
                    now,
                    string.Empty,
                    string.Empty,
                    false, 0,
                    false, 0,
                    false, now.AddMonths(1),
                    now);

                Accounts.Refresh();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void Delete()
        {
            try
            {
                if (_selected != null)
                {
                    Socket_Cache.ProxyAccount.DeleteProxyAccount_ByAccountID(_selected.AID);
                    Selected = null;
                    Accounts.Refresh();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
