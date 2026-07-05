using Wpe.App.Mvvm;

namespace Wpe.App.ViewModels;

/// <summary>功能页面基类。各阶段将由真实实现替换占位内容。</summary>
public abstract class PageViewModel : ObservableObject
{
    public abstract string Title { get; }
    public virtual string Description => string.Empty;
}
