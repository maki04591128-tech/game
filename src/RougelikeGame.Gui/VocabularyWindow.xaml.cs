using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core.Entities;
using RougelikeGame.Data.MagicLanguage;

namespace RougelikeGame.Gui;

/// <summary>
/// 習得済みルーン語一覧ウィンドウ - カテゴリ別フィルタ、理解度表示
/// </summary>
public partial class VocabularyWindow : Window
{
    private readonly Player _player;
    private readonly List<LearnedWordViewModel> _displayedWords = new();

    public VocabularyWindow(Player player)
    {
        InitializeComponent();
        _player = player;

        InitializeCategories();
        RefreshWordList();
    }

    private void InitializeCategories()
    {
        var categories = new List<CategoryItem>
        {
            new("全て", null),
            new("効果語", RuneWordCategory.Effect),
            new("対象語", RuneWordCategory.Target),
            new("属性語", RuneWordCategory.Element),
            new("修飾語", RuneWordCategory.Modifier),
            new("範囲語", RuneWordCategory.Range),
            new("時間語", RuneWordCategory.Duration),
            new("条件語", RuneWordCategory.Condition)
        };

        CategoryCombo.ItemsSource = categories;
        CategoryCombo.DisplayMemberPath = "DisplayName";
        CategoryCombo.SelectedIndex = 0;
    }

    private void RefreshWordList()
    {
        _displayedWords.Clear();

        var selectedCategory = (CategoryCombo.SelectedItem as CategoryItem)?.Category;

        foreach (var (wordId, mastery) in _player.LearnedWords.OrderBy(w => w.Key))
        {
            var word = RuneWordDatabase.GetById(wordId);
            if (word == null) continue;

            if (selectedCategory.HasValue && word.Category != selectedCategory.Value)
                continue;

            _displayedWords.Add(new LearnedWordViewModel(word, mastery));
        }

        WordList.ItemsSource = null;
        WordList.ItemsSource = _displayedWords;
        WordList.DisplayMemberPath = "DisplayText";

        WordCountText.Text = $"({_displayedWords.Count} / {_player.LearnedWords.Count} 語)";

        // 詳細パネルをクリア
        ClearDetail();
    }

    private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshWordList();
    }

    private void WordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WordList.SelectedItem is LearnedWordViewModel vm)
        {
            ShowDetail(vm);
        }
        else
        {
            ClearDetail();
        }
    }

    private void ShowDetail(LearnedWordViewModel vm)
    {
        DetailName.Text = $"{vm.Word.OldNorse}  —  {vm.Word.Meaning}";
        DetailPronunciation.Text = $"発音: {vm.Word.Pronunciation}";
        DetailCategory.Text = $"分類: {GetCategoryDisplayName(vm.Word.Category)}　|　MP: {vm.Word.BaseMpCost}　|　ターン: {vm.Word.BaseTurnCost}";
        DetailMastery.Text = $"理解度: {vm.Mastery}%";
        MasteryBar.Value = Math.Min(vm.Mastery, 100);
        DetailDifficulty.Text = $"難度: {"★".PadRight(vm.Word.Difficulty, '★')}({vm.Word.Difficulty})";
    }

    private void ClearDetail()
    {
        DetailName.Text = "ルーン語を選択してください";
        DetailPronunciation.Text = "";
        DetailCategory.Text = "";
        DetailMastery.Text = "";
        MasteryBar.Value = 0;
        DetailDifficulty.Text = "";
    }

    private static string GetCategoryDisplayName(RuneWordCategory category) => category switch
    {
        RuneWordCategory.Effect => "効果語",
        RuneWordCategory.Target => "対象語",
        RuneWordCategory.Element => "属性語",
        RuneWordCategory.Modifier => "修飾語",
        RuneWordCategory.Range => "範囲語",
        RuneWordCategory.Duration => "時間語",
        RuneWordCategory.Condition => "条件語",
        _ => "不明"
    };

    private static string GetMasteryLabel(int mastery) => mastery switch
    {
        >= 100 => "極",
        >= 80 => "熟",
        >= 60 => "習",
        >= 40 => "学",
        >= 20 => "知",
        _ => "初"
    };

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.R)
        {
            Close();
            e.Handled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>カテゴリ選択用ViewModel</summary>
    private record CategoryItem(string DisplayName, RuneWordCategory? Category);

    /// <summary>習得済みルーン語のViewModel</summary>
    private class LearnedWordViewModel
    {
        public RuneWord Word { get; }
        public int Mastery { get; }
        public string DisplayText { get; }

        public LearnedWordViewModel(RuneWord word, int mastery)
        {
            Word = word;
            Mastery = mastery;
            string masteryLabel = GetMasteryLabel(mastery);
            DisplayText = $"[{masteryLabel}] {word.OldNorse,-14} {word.Meaning,-8} ({word.Pronunciation})  理解度:{mastery}%";
        }
    }
}
