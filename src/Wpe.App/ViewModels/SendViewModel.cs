using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>封包发送：发送任务列表 + 任务编辑 + 封包集合（复用原生 lstSend，保证 1:1）。</summary>
    public sealed class SendViewModel : PageViewModel
    {
        public override string Title => "封包发送";
        public override string Description => "构建发送任务，自定义循环次数与间隔，可对目标套接字批量重放封包。";

        public ICollectionView SendTasks { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SendCommand { get; }

        public SendViewModel()
        {
            SendTasks = CollectionViewSource.GetDefaultView(Socket_Cache.SendList.lstSend);

            AddCommand = new RelayCommand(_ => { Socket_Cache.Send.AddSend_New(); SendTasks.Refresh(); });
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => _selected != null);
            LoadCommand = new RelayCommand(_ => Socket_Cache.SendList.LoadSendList_Dialog());
            SaveCommand = new RelayCommand(_ => Save());
            SendCommand = new RelayCommand(_ => DoSend(), _ => _selected != null && _selected.SCollection.Count > 0);
        }

        private Socket_SendInfo _selected;
        public Socket_SendInfo Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    Packets = _selected != null
                        ? CollectionViewSource.GetDefaultView(_selected.SCollection)
                        : null;
                    OnPropertyChanged(nameof(Packets));
                    RaiseEditorChanged();
                }
            }
        }

        public bool HasSelection => _selected != null;

        public ICollectionView Packets { get; private set; }

        public string SName
        {
            get => _selected?.SName ?? string.Empty;
            set { if (_selected != null) { _selected.SName = value; OnPropertyChanged(); SendTasks.Refresh(); } }
        }

        public bool IsEnable
        {
            get => _selected?.IsEnable ?? false;
            set { if (_selected != null) { _selected.IsEnable = value; OnPropertyChanged(); SendTasks.Refresh(); } }
        }

        public bool SSystemSocket
        {
            get => _selected?.SSystemSocket ?? false;
            set { if (_selected != null) { _selected.SSystemSocket = value; OnPropertyChanged(); } }
        }

        public int SLoopCNT
        {
            get => _selected?.SLoopCNT ?? 1;
            set { if (_selected != null) { _selected.SLoopCNT = value; OnPropertyChanged(); } }
        }

        public int SLoopINT
        {
            get => _selected?.SLoopINT ?? 1000;
            set { if (_selected != null) { _selected.SLoopINT = value; OnPropertyChanged(); } }
        }

        public string SNotes
        {
            get => _selected?.SNotes ?? string.Empty;
            set { if (_selected != null) { _selected.SNotes = value; OnPropertyChanged(); } }
        }

        private void RaiseEditorChanged()
        {
            foreach (var p in new[]
            {
                nameof(SName), nameof(IsEnable), nameof(SSystemSocket),
                nameof(SLoopCNT), nameof(SLoopINT), nameof(SNotes),
            })
                OnPropertyChanged(p);
        }

        private void DeleteSelected()
        {
            try
            {
                if (_selected != null)
                {
                    Socket_Cache.SendList.lstSend.Remove(_selected);
                    Selected = null;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DoSend()
        {
            try
            {
                if (_selected != null)
                {
                    Guid sid = _selected.SID;
                    _ = Task.Run(() => Socket_Cache.Send.DoSendAsync(sid));
                }
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
                if (Socket_Cache.SendList.lstSend.Count > 0)
                {
                    var list = new List<Socket_SendInfo>(Socket_Cache.SendList.lstSend);
                    Socket_Cache.SendList.SaveSendList_Dialog(string.Empty, list);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
