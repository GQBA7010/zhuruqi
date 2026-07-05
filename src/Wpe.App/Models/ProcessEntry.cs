using System.Windows.Media;

namespace Wpe.App.Models;

/// <summary>目标进程条目（对应原 ProcessList DataTable 的一行）。</summary>
public sealed class ProcessEntry
{
    public ProcessEntry(int pid, string name, string path, ImageSource? icon)
    {
        Pid = pid;
        Name = name;
        Path = path;
        Icon = icon;
    }

    public int Pid { get; }
    public string Name { get; }
    public string Path { get; }
    public ImageSource? Icon { get; }

    /// <summary>列表显示：名称 [PID]。</summary>
    public string Display => Pid > -1 ? $"{Name} [{Pid}]" : Name;
}
