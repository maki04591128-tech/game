using System.Windows;
using System.Windows.Media.Animation;

namespace RougelikeGame.Gui;

/// <summary>
/// β.21: ゲームオーバー専用画面
/// </summary>
public partial class GameOverWindow : Window
{
    /// <summary>タイトルに戻る場合 true、ゲーム終了の場合 false</summary>
    public bool ReturnToTitle { get; private set; } = true;

    public GameOverWindow(string resultText)
    {
        InitializeComponent();
        ResultText.Text = resultText;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // タイトルフェードイン → 結果テキストフェードイン → ボタンフェードイン
        var titleFade = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(800));

        var resultFade = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(600))
        {
            BeginTime = TimeSpan.FromMilliseconds(900)
        };

        var buttonFade = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400))
        {
            BeginTime = TimeSpan.FromMilliseconds(1700)
        };

        TitleText.BeginAnimation(OpacityProperty, titleFade);
        ResultText.BeginAnimation(OpacityProperty, resultFade);
        ButtonPanel.BeginAnimation(OpacityProperty, buttonFade);
    }

    private void ReturnTitle_Click(object sender, RoutedEventArgs e)
    {
        ReturnToTitle = true;
        Close();
    }

    private void Quit_Click(object sender, RoutedEventArgs e)
    {
        ReturnToTitle = false;
        Close();
    }
}
