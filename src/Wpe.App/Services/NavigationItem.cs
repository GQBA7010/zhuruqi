using Wpe.App.Mvvm;

namespace Wpe.App.Services;

/// <summary>侧边导航条目，对应一个功能页面。</summary>
public sealed class NavigationItem : ObservableObject
{
    public string Key { get; }
    public string Title { get; }
    public string Glyph { get; }
    private readonly Func<object> _factory;
    private object? _cached;

    public NavigationItem(string key, string title, string glyph, Func<object> factory)
    {
        Key = key;
        Title = title;
        Glyph = glyph;
        _factory = factory;
    }

    /// <summary>页面内容（懒加载并缓存，切换更丝滑）。</summary>
    public object Content => _cached ??= _factory();
}
