using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Data.MagicLanguage;
using RougelikeGame.Engine.Magic;

namespace RougelikeGame.Gui;

/// <summary>
/// 魔法詠唱ウィンドウ - 詠唱作成 + 記録済み呪文タブ
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
        RefreshSavedSpellList();
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
            case Key.V:
            case Key.Escape:
                DialogResult = false;
                Close();
                break;
            case Key.Enter:
                if (MainTabControl.SelectedIndex == 1)
                    LoadSelectedSavedSpell();
                else
                    AddSelectedWord();
                break;
            case Key.Delete:
            case Key.Back:
                if (MainTabControl.SelectedIndex == 1)
                    DeleteSelectedSavedSpell();
                else
                    RemoveLastWord();
                break;
            case Key.C:
                ExecuteCast();
                break;
            case Key.S:
                SaveCurrentSpell();
                break;
            case Key.L:
                LoadAndCastSelectedSavedSpell();
                break;
        }
        e.Handled = true;
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl && MainTabControl.SelectedIndex == 1)
        {
            RefreshSavedSpellList();
        }
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

    #region Saved Spells

    private void SaveSpellButton_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentSpell();
    }

    private void SaveCurrentSpell()
    {
        if (_incantationWords.Count == 0)
        {
            MessageBox.Show("詠唱文が空です。", "記録エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var preview = _controller.GetSpellPreview();
        if (!preview.IsValid)
        {
            MessageBox.Show("詠唱文が不完全です。効果語と対象語が必要です。", "記録エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 名前入力ダイアログ（シンプルなInputBox代替）
        string defaultName = preview.FormattedIncantation;
        string? name = ShowInputDialog("呪文の名前を入力してください", "呪文記録", defaultName);
        if (string.IsNullOrWhiteSpace(name)) return;

        var result = _controller.SaveCurrentSpell(name);
        if (result != null)
        {
            MessageBox.Show($"呪文「{name}」を記録しました。\n記録済み呪文タブから即時呼び出しできます。",
                "記録完了", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshSavedSpellList();
        }
        else
        {
            MessageBox.Show("呪文の記録に失敗しました。記録上限に達しているか、詠唱文が無効です。",
                "記録エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadSpellButton_Click(object sender, RoutedEventArgs e)
    {
        LoadSelectedSavedSpell();
    }

    private void LoadSelectedSavedSpell()
    {
        if (SavedSpellList.SelectedItem is not SavedSpellViewModel selected) return;

        int index = _controller.GetSavedSpells().ToList()
            .FindIndex(s => s.Name == selected.Name && s.FormattedIncantation == selected.Incantation);
        if (index < 0) return;

        // 現在の詠唱文をクリア
        while (_incantationWords.Count > 0)
            RemoveLastWord();

        bool loaded = _controller.LoadSavedSpell(index);
        if (loaded)
        {
            // UIの詠唱文リストを再構築
            RebuildIncantationFromController();
            RefreshIncantationList();
            UpdatePreview();

            // 詠唱作成タブに切り替え
            MainTabControl.SelectedIndex = 0;
        }
    }

    private void LoadAndCastButton_Click(object sender, RoutedEventArgs e)
    {
        LoadAndCastSelectedSavedSpell();
    }

    private void LoadAndCastSelectedSavedSpell()
    {
        if (SavedSpellList.SelectedItem is not SavedSpellViewModel selected) return;

        int index = _controller.GetSavedSpells().ToList()
            .FindIndex(s => s.Name == selected.Name && s.FormattedIncantation == selected.Incantation);
        if (index < 0) return;

        // 現在の詠唱文をクリア
        while (_incantationWords.Count > 0)
            RemoveLastWord();

        bool loaded = _controller.LoadSavedSpell(index);
        if (loaded)
        {
            RebuildIncantationFromController();
            RefreshIncantationList();
            UpdatePreview();
            ExecuteCast();
        }
    }

    private void DeleteSpellButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedSavedSpell();
    }

    private void DeleteSelectedSavedSpell()
    {
        if (SavedSpellList.SelectedItem is not SavedSpellViewModel selected) return;

        var result = MessageBox.Show($"呪文「{selected.Name}」を削除しますか？",
            "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        int index = _controller.GetSavedSpells().ToList()
            .FindIndex(s => s.Name == selected.Name && s.FormattedIncantation == selected.Incantation);
        if (index >= 0)
        {
            _controller.RemoveSavedSpell(index);
            RefreshSavedSpellList();
            ClearSavedSpellDetail();
        }
    }

    private void SavedSpellList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SavedSpellList.SelectedItem is SavedSpellViewModel selected)
        {
            SavedSpellNameText.Text = selected.Name;
            SavedSpellIncantText.Text = selected.Incantation;
            SavedSpellDescText.Text = selected.Description;
            SavedSpellMpText.Text = $"MP消費: {selected.MpCost}";

            // ルーン語一覧を取得
            var savedSpells = _controller.GetSavedSpells();
            int index = savedSpells.ToList()
                .FindIndex(s => s.Name == selected.Name && s.FormattedIncantation == selected.Incantation);
            if (index >= 0 && index < savedSpells.Count)
            {
                var recipe = savedSpells[index];
                var wordNames = recipe.WordIds
                    .Select(id => RuneWordDatabase.GetById(id))
                    .Where(w => w != null)
                    .Select(w => $"{w!.Pronunciation}({w.Meaning})")
                    .ToList();
                SavedSpellWordsText.Text = $"構成語: {string.Join(" → ", wordNames)}";
            }
        }
    }

    private void RefreshSavedSpellList()
    {
        if (SavedSpellList == null) return;

        var savedSpells = _controller.GetSavedSpells();
        var viewModels = savedSpells.Select(s => new SavedSpellViewModel
        {
            Name = s.Name,
            Description = s.Description,
            MpCost = s.MpCost,
            MpCostText = $"MP:{s.MpCost}",
            Incantation = s.FormattedIncantation
        }).ToList();

        SavedSpellList.ItemsSource = null;
        SavedSpellList.ItemsSource = viewModels;
    }

    private void ClearSavedSpellDetail()
    {
        SavedSpellNameText.Text = "呪文を選択";
        SavedSpellIncantText.Text = "";
        SavedSpellDescText.Text = "";
        SavedSpellMpText.Text = "";
        SavedSpellWordsText.Text = "";
    }

    private void RebuildIncantationFromController()
    {
        _incantationWords.Clear();
        var wordIds = _controller.GetCurrentIncantation();
        foreach (var wordId in wordIds)
        {
            var word = RuneWordDatabase.GetById(wordId);
            if (word != null)
            {
                _incantationWords.Add(new RuneWordViewModel(word));
            }
        }
    }

    private static string? ShowInputDialog(string prompt, string title, string defaultValue)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x1a, 0x1a, 0x2e))
        };

        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock
        {
            Text = prompt,
            Foreground = System.Windows.Media.Brushes.White,
            Margin = new Thickness(0, 0, 0, 8)
        };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var textBox = new TextBox
        {
            Text = defaultValue,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x16, 0x21, 0x3e)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(5, 3, 5, 3),
            Margin = new Thickness(0, 0, 0, 10)
        };
        textBox.SelectAll();
        Grid.SetRow(textBox, 1);
        grid.Children.Add(textBox);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var okBtn = new Button
        {
            Content = "OK",
            Width = 80,
            Margin = new Thickness(0, 0, 5, 0),
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x0f, 0x34, 0x60)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(5, 3, 5, 3)
        };
        okBtn.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        var cancelBtn = new Button
        {
            Content = "キャンセル",
            Width = 80,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x0f, 0x34, 0x60)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(5, 3, 5, 3)
        };
        cancelBtn.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };
        buttonPanel.Children.Add(okBtn);
        buttonPanel.Children.Add(cancelBtn);
        Grid.SetRow(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;

        if (dialog.ShowDialog() == true)
            return textBox.Text;
        return null;
    }

    #endregion

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

    public class SavedSpellViewModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int MpCost { get; set; }
        public string MpCostText { get; set; } = "";
        public string Incantation { get; set; } = "";
    }
}
