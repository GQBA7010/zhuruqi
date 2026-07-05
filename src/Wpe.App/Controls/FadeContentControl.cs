using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpe.App.Controls;

/// <summary>
/// 内容切换时执行淡入 + 轻微上移的丝滑过渡（商业级质感）。
/// </summary>
public sealed class FadeContentControl : ContentControl
{
    private static readonly Duration Dur = new(TimeSpan.FromMilliseconds(240));

    public FadeContentControl()
    {
        RenderTransformOrigin = new Point(0.5, 0.5);
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        BeginTransition();
    }

    private void BeginTransition()
    {
        var translate = new TranslateTransform(0, 14);
        RenderTransform = translate;
        Opacity = 0;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, Dur) { EasingFunction = ease });
        translate.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(14, 0, Dur) { EasingFunction = ease });
    }
}
