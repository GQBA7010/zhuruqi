using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyHook;
using Wpe.App.Models;
using WPELibrary.Lib;

namespace Wpe.App.Services;

/// <summary>进程枚举与 EasyHook 注入，复用原生内核 Socket_Operation / RemoteHooking。</summary>
public sealed class InjectionService
{
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(System.IntPtr hObject);

    /// <summary>枚举全部进程（复用 Socket_Operation.GetProcess 的图标/名称/路径）。</summary>
    public async Task<IReadOnlyList<ProcessEntry>> GetProcessesAsync()
    {
        DataTable table = await Socket_Operation.GetProcess();
        var list = new List<ProcessEntry>(table.Rows.Count);

        foreach (DataRow row in table.Rows)
        {
            int pid = row["PID"] is int i ? i : -1;
            string name = row["PName"]?.ToString() ?? string.Empty;
            string path = row["PPath"]?.ToString() ?? string.Empty;
            ImageSource? icon = row["ICO"] is Image img ? ToImageSource(img) : null;
            list.Add(new ProcessEntry(pid, name, path, icon));
        }

        return list;
    }

    /// <summary>把 GDI+ 图像转换为 WPF 可绑定的 ImageSource。</summary>
    private static ImageSource? ToImageSource(Image image)
    {
        using var bmp = new Bitmap(image);
        System.IntPtr hBitmap = bmp.GetHbitmap();
        try
        {
            ImageSource src = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, System.IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            src.Freeze();
            return src;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    /// <summary>注入目标进程；返回目标平台位数（32/64）。逻辑与原 Injector_Form 保持一致。</summary>
    public int Inject(int pid, string path, string name)
    {
        const string channelName = "WPE64";
        string dir = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        string injectionLibrary = System.IO.Path.Combine(dir, Socket_Cache.System.WPE64_DLL);

        int targetPid = pid;
        if (pid > -1)
        {
            RemoteHooking.Inject(pid, injectionLibrary, injectionLibrary, channelName);
        }
        else
        {
            RemoteHooking.CreateAndInject(path, string.Empty, 0, injectionLibrary, injectionLibrary, out targetPid, channelName);
        }

        Socket_Cache.System.LastInjection = name;
        return Socket_Operation.IsWin64Process(targetPid) ? 64 : 32;
    }
}
