using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using Wpe.App.Mvvm;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels
{
    /// <summary>高级滤镜：拦截规则列表 + 规则编辑（与原版 Socket_FilterForm 输入项一一对齐，直写原生模型，保证 1:1）。</summary>
    public sealed class FilterViewModel : PageViewModel
    {
        public override string Title => "高级滤镜";
        public override string Description => "自定义拦截规则：可指定包头/套接字/长度/端口，按位置精确匹配查找与修改字节，支持递进、发送/机器人联动，与原版滤镜完全一致。";

        public ICollectionView Filters { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand EnableAllCommand { get; }
        public RelayCommand DisableAllCommand { get; }
        public RelayCommand AddByteCommand { get; }
        public RelayCommand RemoveByteCommand { get; }

        public IReadOnlyList<ActionOption> ActionOptions { get; }
        public IReadOnlyList<ModeOption> ModeOptions { get; }
        public IReadOnlyList<StartFromOption> StartFromOptions { get; }
        public IReadOnlyList<ExecuteTypeOption> ExecuteTypeOptions { get; }

        public ObservableCollection<ByteEntry> ByteEntries { get; } = new ObservableCollection<ByteEntry>();

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
            StartFromOptions = new List<StartFromOption>
            {
                new StartFromOption(Socket_Cache.Filter.FilterStartFrom.Head, "从包头"),
                new StartFromOption(Socket_Cache.Filter.FilterStartFrom.Position, "从指定位置"),
            };
            ExecuteTypeOptions = new List<ExecuteTypeOption>
            {
                new ExecuteTypeOption(Socket_Cache.Filter.FilterExecuteType.Send, "发送封包"),
                new ExecuteTypeOption(Socket_Cache.Filter.FilterExecuteType.Robot, "运行机器人"),
            };

            AddCommand = new RelayCommand(_ => { Socket_Cache.Filter.AddFilter_New(); Filters.Refresh(); });
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => _selected != null);
            ClearCommand = new RelayCommand(_ => Socket_Cache.FilterList.CleanUpFilterList_Dialog());
            LoadCommand = new RelayCommand(_ => Socket_Cache.FilterList.LoadFilterList_Dialog());
            SaveCommand = new RelayCommand(_ => Save());
            EnableAllCommand = new RelayCommand(_ => SetAllEnable(true));
            DisableAllCommand = new RelayCommand(_ => SetAllEnable(false));
            AddByteCommand = new RelayCommand(_ => AddByteEntry(), _ => _selected != null);
            RemoveByteCommand = new RelayCommand(p => RemoveByteEntry(p as ByteEntry), p => p is ByteEntry);
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
                    LoadEditorFromModel();
                    RaiseEditorChanged();
                    RefreshExecuteTargets();
                }
            }
        }

        public bool HasSelection => _selected != null;

        // ==== 基本字段 ====

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
            set
            {
                if (_selected != null) { _selected.FMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsAdvanced)); }
            }
        }

        public bool IsAdvanced => FMode == Socket_Cache.Filter.FilterMode.Advanced;

        public Socket_Cache.Filter.FilterAction FAction
        {
            get => _selected?.FAction ?? Socket_Cache.Filter.FilterAction.Replace;
            set { if (_selected != null) { _selected.FAction = value; OnPropertyChanged(); } }
        }

        public Socket_Cache.Filter.FilterStartFrom FStartFrom
        {
            get => _selected?.FStartFrom ?? Socket_Cache.Filter.FilterStartFrom.Head;
            set { if (_selected != null) { _selected.FStartFrom = value; OnPropertyChanged(); } }
        }

        // ==== 指定条件 ====

        public bool AppointHeader
        {
            get => _selected?.AppointHeader ?? false;
            set { if (_selected != null) { _selected.AppointHeader = value; OnPropertyChanged(); } }
        }

        public string HeaderContent
        {
            get => _selected?.HeaderContent ?? string.Empty;
            set { if (_selected != null) { _selected.HeaderContent = value; OnPropertyChanged(); } }
        }

        public bool AppointSocket
        {
            get => _selected?.AppointSocket ?? false;
            set { if (_selected != null) { _selected.AppointSocket = value; OnPropertyChanged(); } }
        }

        public decimal SocketContent
        {
            get => _selected?.SocketContent ?? 0;
            set { if (_selected != null) { _selected.SocketContent = value; OnPropertyChanged(); } }
        }

        public bool AppointLength
        {
            get => _selected?.AppointLength ?? false;
            set { if (_selected != null) { _selected.AppointLength = value; OnPropertyChanged(); } }
        }

        private int _lengthFrom;
        public int LengthFrom
        {
            get => _lengthFrom;
            set { if (SetProperty(ref _lengthFrom, value)) WriteLengthContent(); }
        }

        private int _lengthTo;
        public int LengthTo
        {
            get => _lengthTo;
            set { if (SetProperty(ref _lengthTo, value)) WriteLengthContent(); }
        }

        private void WriteLengthContent()
        {
            if (_selected != null)
                _selected.LengthContent = _lengthFrom.ToString(CultureInfo.InvariantCulture) + "-" + _lengthTo.ToString(CultureInfo.InvariantCulture);
        }

        public bool AppointPort
        {
            get => _selected?.AppointPort ?? false;
            set { if (_selected != null) { _selected.AppointPort = value; OnPropertyChanged(); } }
        }

        public decimal PortContent
        {
            get => _selected?.PortContent ?? 0;
            set { if (_selected != null) { _selected.PortContent = value; OnPropertyChanged(); } }
        }

        // ==== 执行（发送/机器人） ====

        public bool IsExecute
        {
            get => _selected?.IsExecute ?? false;
            set { if (_selected != null) { _selected.IsExecute = value; OnPropertyChanged(); } }
        }

        public Socket_Cache.Filter.FilterExecuteType FEType
        {
            get => _selected?.FEType ?? Socket_Cache.Filter.FilterExecuteType.Send;
            set
            {
                if (_selected != null)
                {
                    _selected.FEType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsExecuteSend));
                    OnPropertyChanged(nameof(IsExecuteRobot));
                    RefreshExecuteTargets();
                }
            }
        }

        public bool IsExecuteSend => FEType == Socket_Cache.Filter.FilterExecuteType.Send;
        public bool IsExecuteRobot => FEType == Socket_Cache.Filter.FilterExecuteType.Robot;

        public ObservableCollection<SendOption> SendOptions { get; } = new ObservableCollection<SendOption>();
        public ObservableCollection<RobotOption> RobotOptions { get; } = new ObservableCollection<RobotOption>();

        public Guid SID
        {
            get => _selected?.SID ?? Guid.Empty;
            set { if (_selected != null) { _selected.SID = value; OnPropertyChanged(); } }
        }

        public Guid RID
        {
            get => _selected?.RID ?? Guid.Empty;
            set { if (_selected != null) { _selected.RID = value; OnPropertyChanged(); } }
        }

        private void RefreshExecuteTargets()
        {
            SendOptions.Clear();
            foreach (var s in Socket_Cache.SendList.lstSend)
                SendOptions.Add(new SendOption(s.SID, s.SName));
            RobotOptions.Clear();
            foreach (var r in Socket_Cache.RobotList.lstRobot)
                RobotOptions.Add(new RobotOption(r.RID, r.RName));
            OnPropertyChanged(nameof(SID));
            OnPropertyChanged(nameof(RID));
        }

        // ==== 递进 ====

        public bool IsProgressionContinuous
        {
            get => _selected?.IsProgressionContinuous ?? false;
            set { if (_selected != null) { _selected.IsProgressionContinuous = value; OnPropertyChanged(); } }
        }

        public decimal ProgressionStep
        {
            get => _selected?.ProgressionStep ?? 1;
            set { if (_selected != null) { _selected.ProgressionStep = value; OnPropertyChanged(); } }
        }

        public bool IsProgressionCarry
        {
            get => _selected?.IsProgressionCarry ?? false;
            set { if (_selected != null) { _selected.IsProgressionCarry = value; OnPropertyChanged(); } }
        }

        public decimal ProgressionCarryNumber
        {
            get => _selected?.ProgressionCarryNumber ?? 1;
            set { if (_selected != null) { _selected.ProgressionCarryNumber = value; OnPropertyChanged(); } }
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

        // ==== 字节匹配表（按位置查找/修改/递进，序列化为原版 FSearch/FModify/ProgressionPosition 格式） ====

        private void AddByteEntry()
        {
            int nextIndex = ByteEntries.Count == 0 ? 0 : ByteEntries.Max(b => b.Index) + 1;
            var entry = new ByteEntry(nextIndex, string.Empty, string.Empty, false, WriteByteEntriesToModel);
            ByteEntries.Add(entry);
            WriteByteEntriesToModel();
        }

        private void RemoveByteEntry(ByteEntry entry)
        {
            if (entry != null && ByteEntries.Remove(entry))
                WriteByteEntriesToModel();
        }

        private void LoadEditorFromModel()
        {
            foreach (var e in ByteEntries) e.Detach();
            ByteEntries.Clear();

            if (_selected == null)
            {
                _lengthFrom = 0; _lengthTo = 0;
                OnPropertyChanged(nameof(LengthFrom));
                OnPropertyChanged(nameof(LengthTo));
                return;
            }

            ParseLengthContent(_selected.LengthContent);

            var search = ParsePairs(_selected.FSearch);
            var modify = ParsePairs(_selected.FModify);
            var prog = ParseProgression(_selected.ProgressionPosition);

            var indices = new SortedSet<int>();
            foreach (var k in search.Keys) indices.Add(k);
            foreach (var k in modify.Keys) indices.Add(k);
            foreach (var k in prog) indices.Add(k);

            foreach (var idx in indices)
            {
                search.TryGetValue(idx, out string s);
                modify.TryGetValue(idx, out string m);
                var entry = new ByteEntry(idx, s ?? string.Empty, m ?? string.Empty, prog.Contains(idx), WriteByteEntriesToModel);
                ByteEntries.Add(entry);
            }
        }

        private void ParseLengthContent(string content)
        {
            _lengthFrom = 0; _lengthTo = 0;
            if (!string.IsNullOrEmpty(content))
            {
                if (content.Contains("-"))
                {
                    var parts = content.Split('-');
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[0], out _lengthFrom);
                        int.TryParse(parts[1], out _lengthTo);
                    }
                }
                else if (int.TryParse(content, out int v))
                {
                    _lengthFrom = v; _lengthTo = v;
                }
            }
            OnPropertyChanged(nameof(LengthFrom));
            OnPropertyChanged(nameof(LengthTo));
        }

        private static Dictionary<int, string> ParsePairs(string raw)
        {
            var dict = new Dictionary<int, string>();
            if (string.IsNullOrEmpty(raw)) return dict;
            foreach (var part in raw.Split(','))
            {
                if (string.IsNullOrEmpty(part) || part.IndexOf('|') <= 0) continue;
                var pair = part.Split('|');
                if (pair.Length != 2) continue;
                if (int.TryParse(pair[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int idx))
                    dict[idx] = pair[1].Trim().ToUpperInvariant();
            }
            return dict;
        }

        private static HashSet<int> ParseProgression(string raw)
        {
            var set = new HashSet<int>();
            if (string.IsNullOrEmpty(raw)) return set;
            foreach (var part in raw.Split(','))
            {
                if (!string.IsNullOrEmpty(part) && int.TryParse(part, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int idx))
                    set.Add(idx);
            }
            return set;
        }

        private void WriteByteEntriesToModel()
        {
            if (_selected == null) return;

            var search = new StringBuilder();
            var modify = new StringBuilder();
            var prog = new StringBuilder();

            foreach (var e in ByteEntries.OrderBy(x => x.Index))
            {
                string s = NormalizeHex(e.Search);
                string m = NormalizeHex(e.Modify);
                if (s.Length == 2) search.Append(e.Index).Append('|').Append(s).Append(',');
                if (m.Length == 2) modify.Append(e.Index).Append('|').Append(m).Append(',');
                if (e.Progression) prog.Append(e.Index).Append(',');
            }

            _selected.FSearch = search.ToString().TrimEnd(',');
            _selected.FModify = modify.ToString().TrimEnd(',');
            _selected.ProgressionPosition = prog.ToString().TrimEnd(',');
        }

        private static string NormalizeHex(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Trim().ToUpperInvariant();
            return s.Length == 1 ? "0" + s : s;
        }

        private void RaiseEditorChanged()
        {
            foreach (var p in new[]
            {
                nameof(FName), nameof(IsEnable), nameof(FMode), nameof(IsAdvanced), nameof(FAction), nameof(FStartFrom),
                nameof(AppointHeader), nameof(HeaderContent), nameof(AppointSocket), nameof(SocketContent),
                nameof(AppointLength), nameof(AppointPort), nameof(PortContent),
                nameof(IsExecute), nameof(FEType), nameof(IsExecuteSend), nameof(IsExecuteRobot), nameof(SID), nameof(RID),
                nameof(IsProgressionContinuous), nameof(ProgressionStep), nameof(IsProgressionCarry), nameof(ProgressionCarryNumber),
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

        public sealed class ByteEntry : ObservableObject
        {
            private readonly Action _onChanged;

            public ByteEntry(int index, string search, string modify, bool progression, Action onChanged)
            {
                _index = index;
                _search = search;
                _modify = modify;
                _progression = progression;
                _onChanged = onChanged;
            }

            public void Detach() { }

            private int _index;
            public int Index
            {
                get => _index;
                set { if (SetProperty(ref _index, value)) _onChanged?.Invoke(); }
            }

            private string _search;
            public string Search
            {
                get => _search;
                set { if (SetProperty(ref _search, value)) _onChanged?.Invoke(); }
            }

            private string _modify;
            public string Modify
            {
                get => _modify;
                set { if (SetProperty(ref _modify, value)) _onChanged?.Invoke(); }
            }

            private bool _progression;
            public bool Progression
            {
                get => _progression;
                set { if (SetProperty(ref _progression, value)) _onChanged?.Invoke(); }
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

        public sealed class StartFromOption
        {
            public StartFromOption(Socket_Cache.Filter.FilterStartFrom value, string name) { Value = value; Name = name; }
            public Socket_Cache.Filter.FilterStartFrom Value { get; }
            public string Name { get; }
        }

        public sealed class ExecuteTypeOption
        {
            public ExecuteTypeOption(Socket_Cache.Filter.FilterExecuteType value, string name) { Value = value; Name = name; }
            public Socket_Cache.Filter.FilterExecuteType Value { get; }
            public string Name { get; }
        }

        public sealed class SendOption
        {
            public SendOption(Guid id, string name) { Id = id; Name = name; }
            public Guid Id { get; }
            public string Name { get; }
        }

        public sealed class RobotOption
        {
            public RobotOption(Guid id, string name) { Id = id; Name = name; }
            public Guid Id { get; }
            public string Name { get; }
        }
    }
}
