namespace Wpe.App.ViewModels;

/// <summary>阶段性占位页：骨架期用于呈现完整可导航的商业级外壳。</summary>
public sealed class PlaceholderPageViewModel : PageViewModel
{
    public PlaceholderPageViewModel(string title, string description, string phase)
    {
        Title = title;
        Description = description;
        Phase = phase;
    }

    public override string Title { get; }
    public override string Description { get; }

    /// <summary>规划阶段标记（P1~P5）。</summary>
    public string Phase { get; }
}
