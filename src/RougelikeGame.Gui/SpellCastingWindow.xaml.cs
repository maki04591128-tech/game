using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Data.MagicLanguage;

namespace RougelikeGame.Gui;

/// <summary>
/// 魔法詠唱ウィンドウ
/// </summary>
public partial class SpellCastingWindow : Window
{
    private readonly GameController _controller;
    private readonly List<RuneWordViewModel> _availableWords = new();
    private readonly List<RuneWordViewModel> _incantationWords = new();
    private bool _castRequested = false;

    public bool CastRequested => _castRequested;

    public SpellCastingWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;

        InitializeCategories();
        UpdateCurrentMp();
        UpdatePreview();
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
            new("時間語", RuneWordCategory.Duration)
        };

        CategoryCombo.ItemsSource = categories;
        CategoryCombo.DisplayMemberPath = "DisplayName";
        CategoryCombo.SelectedIndex = 0;
    }

    private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryCombo.SelectedItem is not CategoryItem selected) return;

        _availableWords.Clear();

        var words = selected.Category.HasValue
            ? RuneWordDatabase.GetByCategory(selected.Category.Value)
            : RuneWordDatabase.GetAll();

        // プレイヤーが習得済みのルーン語のみ表示
        foreach (var word in words.OrderBy(w => w.Difficulty).ThenBy(w => w.Category))
        {
            if (_controller.Player.LearnedWords.ContainsKey(word.Id))
            {
                _availableWords.Add(new RuneWordViewModel(word));
            }
        }

        // 習得済みが0件の場合は全ルーン語を表示（デバッグ・チュートリアル用）
        if (_availableWords.Count == 0)
        {
            foreach (var word in words.OrderBy(w => w.Difficulty).ThenBy(w => w.Category))
            {
                _availableWords.Add(new RuneWordViewModel(word));
            }
        }

        RuneWordList.ItemsSource = null;
        RuneWordList.ItemsSource = _availableWords;
    }

    private void RuneWordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 選択変更時の処理（必要に応じて詳細表示）
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddSelectedWord();
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        RemoveLastWord();
    }

    private void CastButton_Click(object sender, RoutedEventArgs e)
    {
        ExecuteCast();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
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
                break;
            case Key.Enter:
                AddSelectedWord();
                break;
            case Key.Delete:
            case Key.Back:
                RemoveLastWord();
                break;
            case Key.C:
                ExecuteCast();
                break;
        }
        e.Handled = true;
    }

    private void AddSelectedWord()
    {
        if (RuneWordList.SelectedItem is not RuneWordViewModel selected) return;

        bool added = _controller.AddRuneWord(selected.Word.Id);
        if (added)
        {
            _incantationWords.Add(selected);
            RefreshIncantationList();
            UpdatePreview();
        }
    }

    private void RemoveLastWord()
    {
        if (_incantationWords.Count == 0) return;

        bool removed = _controller.RemoveLastRuneWord();
        if (removed)
        {
            _incantationWords.RemoveAt(_incantationWords.Count - 1);
            RefreshIncantationList();
            UpdatePreview();
        }
    }

    private void ExecuteCast()
    {
        if (_incantationWords.Count == 0) return;

        var preview = _controller.GetSpellPreview();
        if (!preview.IsValid)
        {
            MessageBox.Show("詠唱文が不完全です。効果語と対象語が必要です。", "詠唱エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _castRequested = true;
        DialogResult = true;
        Close();
    }

    private void RefreshIncantationList()
    {
        IncantationList.ItemsSource = null;
        IncantationList.ItemsSource = _incantationWords;
    }

    private void UpdatePreview()
    {
        var preview = _controller.GetSpellPreview();

        PreviewMpCost.Text = $"MP消費: {preview.MpCost}";
        PreviewTurnCost.Text = $"詠唱ターン: {preview.TurnCost}";
        PreviewSuccessRate.Text = $"成功率: {preview.SuccessRate:P0}";
        PreviewDescription.Text = preview.Description;
        PreviewIncantation.Text = preview.FormattedIncantation;

        ValidityText.Text = preview.IsValid ? "✓ 詠唱可能" : "✗ 詠唱不可（効果語と対象語が必要）";
        ValidityText.Foreground = preview.IsValid
            ? System.Windows.Media.Brushes.LimeGreen
            : System.Windows.Media.Brushes.OrangeRed;

        CurrentMpText.Text = $"{_controller.Player.CurrentMp}/{_controller.Player.MaxMp}";
    }

    private void UpdateCurrentMp()
    {
        CurrentMpText.Text = $"{_controller.Player.CurrentMp}/{_controller.Player.MaxMp}";
    }

    private record CategoryItem(string DisplayName, RuneWordCategory? Category);

    public class RuneWordViewModel
    {
        public RuneWord Word { get; }
        public string Pronunciation => Word.Pronunciation;
        public string Meaning => Word.Meaning;
        public string CategoryDisplay => Word.Category switch
        {
            RuneWordCategory.Effect => "[効果]",
            RuneWordCategory.Target => "[対象]",
            RuneWordCategory.Element => "[属性]",
            RuneWordCategory.Modifier => "[修飾]",
            RuneWordCategory.Range => "[範囲]",
            RuneWordCategory.Duration => "[時間]",
            _ => ""
        };

        public RuneWordViewModel(RuneWord word) => Word = word;
    }
}
