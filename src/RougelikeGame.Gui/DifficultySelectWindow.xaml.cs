using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;

namespace RougelikeGame.Gui;

/// <summary>
/// 難易度選択ウィンドウ
/// </summary>
public partial class DifficultySelectWindow : Window
{
    /// <summary>選択された難易度</summary>
    public DifficultyLevel SelectedDifficulty { get; private set; } = DifficultyLevel.Normal;

    /// <summary>確定されたか</summary>
    public bool Confirmed { get; private set; }

    public DifficultySelectWindow()
    {
        InitializeComponent();
        LoadDifficulties();
    }

    private void LoadDifficulties()
    {
        var items = new List<DifficultyItem>
        {
            new(DifficultyLevel.Easy, "\u2B50", "Easy",
                "初心者向け。ダメージ軽減、経験値増加、ターン制限緩和。",
                "敵ダメージ: 0.7倍 | 経験値: 1.5倍 | ターン制限: 1.5倍"),
            new(DifficultyLevel.Normal, "\u2694\uFE0F", "Normal",
                "標準的なバランス。推奨難易度。",
                "敵ダメージ: 1.0倍 | 経験値: 1.0倍 | ターン制限: 1.0倍"),
            new(DifficultyLevel.Hard, "\uD83D\uDD25", "Hard",
                "上級者向け。敵が強化され、リソースが厳しい。",
                "敵ダメージ: 1.3倍 | 経験値: 0.8倍 | ターン制限: 0.8倍"),
            new(DifficultyLevel.Nightmare, "\uD83D\uDC80", "Nightmare",
                "極めて困難。一つのミスが命取り。",
                "敵ダメージ: 1.6倍 | 経験値: 0.6倍 | ターン制限: 0.6倍"),
            new(DifficultyLevel.Ironman, "\u26D3\uFE0F", "Ironman",
                "最高難易度。死亡時セーブデータ削除。やり直し不可。",
                "敵ダメージ: 1.2倍 | 経験値: 1.0倍 | 死亡時セーブ削除")
        };

        DifficultyList.ItemsSource = items;
        DifficultyList.SelectedIndex = 1; // Normal
    }

    private void DifficultyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DifficultyList.SelectedItem is DifficultyItem item)
        {
            DetailText.Text = item.Detail;
        }
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (DifficultyList.SelectedItem is DifficultyItem item)
        {
            SelectedDifficulty = item.Level;
            Confirmed = true;
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                DialogResult = false;
                Close();
                e.Handled = true;
                break;
            case Key.Enter:
                ConfirmButton_Click(sender, e);
                e.Handled = true;
                break;
        }
    }

    public record DifficultyItem(
        DifficultyLevel Level, string Icon, string Name, string Description, string Detail);
}
