using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using Wpe.App.Models;
using Wpe.App.Mvvm;
using Wpe.App.Services;
using WPELibrary.Lib;

namespace Wpe.App.ViewModels;

/// <summary>进程注入页：选择目标进程 → EasyHook 注入。对齐原 Injector_Form + ProcessList_Form。</summary>
public sealed class InjectViewModel : PageViewModel
{
    private readonly InjectionService _service = new();

    public override string Title => "进程注入";
    public override string Description => "选择目标进程并注入 Winsock 钩子，支持 32/64 位程序与模拟器。";

    public ObservableCollection<ProcessEntry> Processes { get; } = new();
    public ICollectionView ProcessesView { get; }
    public ObservableCollection<string> Log { get; } = new();

    public InjectViewModel()
    {
        ProcessesView = CollectionViewSource.GetDefaultView(Processes);
        ProcessesView.Filter = FilterProcess;

        RefreshCommand = new RelayCommand(async () => await LoadProcessesAsync(), () => !IsBusy);
        PickFileCommand = new RelayCommand(PickFile, () => !IsBusy);
        InjectCommand = new RelayCommand(Inject, () => !IsBusy && SelectedProcess is not null);

        AppendLog(string.Format(
            MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_5), Socket_Operation.AssemblyVersion));

        _ = InitLastInjectionAsync();
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand PickFileCommand { get; }
    public RelayCommand InjectCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    private ProcessEntry? _selectedProcess;
    public ProcessEntry? SelectedProcess
    {
        get => _selectedProcess;
        set => SetProperty(ref _selectedProcess, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ProcessesView.Refresh();
        }
    }

    private bool FilterProcess(object item)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return item is ProcessEntry p &&
               p.Name.StartsWith(SearchText.Trim(), System.StringComparison.OrdinalIgnoreCase);
    }

    private async System.Threading.Tasks.Task LoadProcessesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = await _service.GetProcessesAsync();
            Processes.Clear();
            foreach (var p in list) Processes.Add(p);
            ProcessesView.Refresh();
        }
        catch (System.Exception ex)
        {
            AppendLog(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async System.Threading.Tasks.Task InitLastInjectionAsync()
    {
        await LoadProcessesAsync();
        string last = Socket_Cache.System.LastInjection;
        if (!string.IsNullOrEmpty(last))
        {
            SelectedProcess = Processes.FirstOrDefault(
                p => string.Equals(p.Name, last, System.StringComparison.OrdinalIgnoreCase));
        }
    }

    private void PickFile()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_14),
            Multiselect = false,
            Filter = MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_15),
        };

        if (dlg.ShowDialog() == true)
        {
            string path = dlg.FileName;
            var entry = new ProcessEntry(-1, System.IO.Path.GetFileName(path), path, null);
            Processes.Insert(0, entry);
            SelectedProcess = entry;
        }
    }

    private void Inject()
    {
        var target = SelectedProcess;
        if (target is null)
        {
            AppendLog(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_6));
            return;
        }

        try
        {
            AppendLog(System.DateTime.Now.ToString("G"));
            AppendLog(string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_7), target.Name));

            int plat = _service.Inject(target.Pid, target.Path, target.Name);

            AppendLog(string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_8), plat));
            AppendLog(string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_9), target.Name, target.Pid));
            AppendLog(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_10));

            Socket_Cache.System.SaveSystemConfig_LastInjection_ToDB();
        }
        catch (System.Exception ex)
        {
            AppendLog(string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_11), ex.Message));
            AppendLog(string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_102), Socket_Cache.System.WPE64_URL));
        }
    }

    private void AppendLog(string line) => Log.Add(line);
}
