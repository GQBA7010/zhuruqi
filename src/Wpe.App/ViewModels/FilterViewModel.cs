using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>高级滤镜：拦截规则列表 + 规则编辑（复用原生 lstFilter，保证 1:1）。</summary>
    public sealed class FilterViewModel : PageViewModel
    {
        public override string Title => "高级滤镜";
        public override string Description => "自定义拦截规则，可指定包头/套接字/长度/端口，并对匹配封包执行替换、拦截、修改等动作。";

        public ICollectionView Filters { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand EnableAllCommand { get; }
        public RelayCommand DisableAllCommand { get; }

        public IReadOnlyList<ActionOption> ActionOptions { get; }
        public IReadOnlyList<ModeOption> ModeOptions { get; }

        public FilterViewModel()
        {
            Filters = CollectionViewSource.GetDefaultView(Socket_Cache.FilterList.lstFilter);

            ActionOptions = new List<ActionOption>
            {
                new ActionOption(Socket_Cache.Filter.FilterAction.Replace),
                new ActionOption(Socket_Cache.Filter.FilterAction.Intercept),
                new ActionOption(Socket_Cache.Filter.FilterAction.Change),
                new ActionOption(Socket_Cache.Filter.FilterAction.NoModify_Display),
                new ActionOption(Socket_Cache.Filter.FilterAction.NoModify_NoDisplay),
            };
            ModeOptions = new List<ModeOption>
            {
                new ModeOption(Socket_Cache.Filter.FilterMode.Normal, "普通"),
                new ModeOption(Socket_Cache.Filter.FilterMode.Advanced, "高级"),
            };

            AddCommand = new RelayCommand(_ => { Socket_Cache.Filter.AddFilter_New(); Filters.Refresh(); });
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => _selected != null);
            ClearCommand = new RelayCommand(_ => Socket_Cache.FilterList.CleanUpFilterList_Dialog());
            LoadCommand = new RelayCommand(_ => Socket_Cache.FilterList.LoadFilterList_Dialog());
            SaveCommand = new RelayCommand(_ => Save());
            EnableAllCommand = new RelayCommand(_ => SetAllEnable(true));
            DisableAllCommand = new RelayCommand(_ => SetAllEnable(false));
        }

        private Socket_FilterInfo _selected;
        public Socket_FilterInfo Selected
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

        // ==== 规则编辑字段（直写原生模型） ====

        public string FName
        {
            get => _selected?.FName ?? string.Empty;
            set { if (_selected != null) { _selected.FName = value; OnPropertyChanged(); Filters.Refresh(); } }
        }

        public bool IsEnable
        {
            get => _selected?.IsEnable ?? false;
            set { if (_selected != null) { _selected.IsEnable = value; OnPropertyChanged(); Filters.Refresh(); } }
        }

        public Socket_Cache.Filter.FilterMode FMode
        {
            get => _selected?.FMode ?? Socket_Cache.Filter.FilterMode.Normal;
            set { if (_selected != null) { _selected.FMode = value; OnPropertyChanged(); } }
        }

        public Socket_Cache.Filter.FilterAction FAction
        {
            get => _selected?.FAction ?? Socket_Cache.Filter.FilterAction.Replace;
            set { if (_selected != null) { _selected.FAction = value; OnPropertyChanged(); } }
        }

        public string FSearch
        {
            get => _selected?.FSearch ?? string.Empty;
            set { if (_selected != null) { _selected.FSearch = value; OnPropertyChanged(); } }
        }

        public string FModify
        {
            get => _selected?.FModify ?? string.Empty;
            set { if (_selected != null) { _selected.FModify = value; OnPropertyChanged(); } }
        }

        // ==== 作用函数（FFunction 为结构体，需整体读写） ====
        public bool FnSend { get => _selected?.FFunction.Send ?? false; set => SetFn(f => f.Send = value); }
        public bool FnSendTo { get => _selected?.FFunction.SendTo ?? false; set => SetFn(f => f.SendTo = value); }
        public bool FnRecv { get => _selected?.FFunction.Recv ?? false; set => SetFn(f => f.Recv = value); }
        public bool FnRecvFrom { get => _selected?.FFunction.RecvFrom ?? false; set => SetFn(f => f.RecvFrom = value); }
        public bool FnWSASend { get => _selected?.FFunction.WSASend ?? false; set => SetFn(f => f.WSASend = value); }
        public bool FnWSASendTo { get => _selected?.FFunction.WSASendTo ?? false; set => SetFn(f => f.WSASendTo = value); }
        public bool FnWSARecv { get => _selected?.FFunction.WSARecv ?? false; set => SetFn(f => f.WSARecv = value); }
        public bool FnWSARecvFrom { get => _selected?.FFunction.WSARecvFrom ?? false; set => SetFn(f => f.WSARecvFrom = value); }

        private void SetFn(Action<RefFn> mutate, [System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (_selected == null) return;
            var f = _selected.FFunction;
            var box = new RefFn(f);
            mutate(box);
            _selected.FFunction = box.Value;
            OnPropertyChanged(name);
        }

        private sealed class RefFn
        {
            public Socket_Cache.Filter.FilterFunction Value;
            public RefFn(Socket_Cache.Filter.FilterFunction v) { Value = v; }
            public bool Send { get => Value.Send; set => Value.Send = value; }
            public bool SendTo { get => Value.SendTo; set => Value.SendTo = value; }
            public bool Recv { get => Value.Recv; set => Value.Recv = value; }
            public bool RecvFrom { get => Value.RecvFrom; set => Value.RecvFrom = value; }
            public bool WSASend { get => Value.WSASend; set => Value.WSASend = value; }
            public bool WSASendTo { get => Value.WSASendTo; set => Value.WSASendTo = value; }
            public bool WSARecv { get => Value.WSARecv; set => Value.WSARecv = value; }
            public bool WSARecvFrom { get => Value.WSARecvFrom; set => Value.WSARecvFrom = value; }
        }

        private void RaiseEditorChanged()
        {
            foreach (var p in new[]
            {
                nameof(FName), nameof(IsEnable), nameof(FMode), nameof(FAction), nameof(FSearch), nameof(FModify),
                nameof(FnSend), nameof(FnSendTo), nameof(FnRecv), nameof(FnRecvFrom),
                nameof(FnWSASend), nameof(FnWSASendTo), nameof(FnWSARecv), nameof(FnWSARecvFrom),
            })
                OnPropertyChanged(p);
        }

        private void DeleteSelected()
        {
            try
            {
                if (_selected != null)
                {
                    Socket_Cache.FilterList.lstFilter.Remove(_selected);
                    Selected = null;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SetAllEnable(bool enable)
        {
            foreach (var f in Socket_Cache.FilterList.lstFilter)
                f.IsEnable = enable;
            Filters.Refresh();
        }

        private void Save()
        {
            try
            {
                if (Socket_Cache.FilterList.lstFilter.Count > 0)
                {
                    var list = new List<Socket_FilterInfo>(Socket_Cache.FilterList.lstFilter);
                    Socket_Cache.FilterList.SaveFilterList_Dialog(string.Empty, list);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public sealed class ActionOption
        {
            public ActionOption(Socket_Cache.Filter.FilterAction value)
            {
                Value = value;
                Name = Socket_Cache.Filter.GetName_ByFilterAction(value);
            }
            public Socket_Cache.Filter.FilterAction Value { get; }
            public string Name { get; }
        }

        public sealed class ModeOption
        {
            public ModeOption(Socket_Cache.Filter.FilterMode value, string name) { Value = value; Name = name; }
            public Socket_Cache.Filter.FilterMode Value { get; }
            public string Name { get; }
        }
    }
}
