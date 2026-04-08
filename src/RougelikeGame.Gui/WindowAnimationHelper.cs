using System.Windows;
using System.Windows.Media.Animation;

namespace RougelikeGame.Gui;

/// <summary>
/// β.16: ウィンドウアニメーション共通ヘルパー
/// </summary>
public static class WindowAnimationHelper
{
    /// <summary>
    /// ウィンドウのフェードイン演出を開始する
    /// </summary>
    public static void FadeIn(Window window, double durationMs = 200)
    {
        window.Opacity = 0;
        var anim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        window.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    /// <summary>
    /// ウィンドウの軽いスライドイン演出（上から少し下がってくる）
    /// </summary>
    public static void SlideInFromTop(Window window, double durationMs = 180)
    {
        var transform = new System.Windows.Media.TranslateTransform(0, -12);
        window.RenderTransform = transform;
        window.Opacity = 0;

        var slideAnim = new DoubleAnimation(-12, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var fadeAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(durationMs));

        transform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideAnim);
        window.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }
}
