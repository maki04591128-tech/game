using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;

namespace RougelikeGame.Gui;

/// <summary>
/// タイトル画面ウィンドウ
/// </summary>
public partial class TitleWindow : Window
{
    private readonly Button[] _menuButtons;
    private int _selectedIndex = 0;

    /// <summary>タイトル画面の選択結果</summary>
    public TitleAction SelectedAction { get; private set; } = TitleAction.Quit;

    /// <summary>選択された難易度</summary>
    public DifficultyLevel SelectedDifficulty { get; private set; } = DifficultyLevel.Normal;

    /// <summary>選択された種族</summary>
    public Race SelectedRace { get; private set; } = Race.Human;

    /// <summary>選択された職業</summary>
    public RougelikeGame.Core.CharacterClass SelectedClass { get; private set; } = RougelikeGame.Core.CharacterClass.Fighter;

    /// <summary>選択された素性</summary>
    public RougelikeGame.Core.Background SelectedBackground { get; private set; } = RougelikeGame.Core.Background.Adventurer;

    /// <summary>プレイヤー名</summary>
    public string PlayerName { get; private set; } = "冒険者";

    /// <summary>選択されたセーブスロット番号</summary>
    public int SelectedSaveSlot { get; private set; }

    public TitleWindow()
    {
        InitializeComponent();

        _menuButtons = [NewGameButton, ContinueButton, SettingsButton, QuitButton];

        // コンティニューボタンの有効/無効をセーブデータ存在で制御
        ContinueButton.IsEnabled = SaveManager.AnySaveExists();

        UpdateMenuHighlight();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        // 難易度選択
        var difficultyWindow = new DifficultySelectWindow();
        difficultyWindow.Owner = this;
        if (difficultyWindow.ShowDialog() != true || !difficultyWindow.Confirmed)
            return;

        SelectedDifficulty = difficultyWindow.SelectedDifficulty;

        // キャラクター作成
        var charWindow = new CharacterCreationWindow();
        charWindow.Owner = this;
        if (charWindow.ShowDialog() != true || !charWindow.Confirmed)
            return;

        SelectedRace = charWindow.SelectedRace;
        SelectedClass = charWindow.SelectedClass;
        SelectedBackground = charWindow.SelectedBackground;
        PlayerName = charWindow.PlayerName;

        SelectedAction = TitleAction.NewGame;
        DialogResult = true;
        Close();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        if (!SaveManager.AnySaveExists()) return;

        // セーブデータ選択画面を表示
        var saveSelectWindow = new SaveDataSelectWindow();
        saveSelectWindow.Owner = this;
        if (saveSelectWindow.ShowDialog() != true || !saveSelectWindow.Confirmed)
            return;

        SelectedSaveSlot = saveSelectWindow.SelectedSlot;
        SelectedAction = TitleAction.Continue;
        DialogResult = true;
        Close();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void QuitButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedAction = TitleAction.Quit;
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // デバッグマップ: 左Ctrl + 右Ctrl + D
        if (e.Key == Key.D &&
            (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            SelectedAction = TitleAction.DebugMap;
            DialogResult = true;
            Close();
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.Up:
            case Key.W:
                MoveCursor(-1);
                e.Handled = true;
                break;
            case Key.Down:
            case Key.S:
                MoveCursor(1);
                e.Handled = true;
                break;
            case Key.Enter:
            case Key.Space:
                ActivateSelectedButton();
                e.Handled = true;
                break;
            case Key.Escape:
                SelectedAction = TitleAction.Quit;
                DialogResult = false;
                Close();
                e.Handled = true;
                break;
        }
    }

    private void MoveCursor(int direction)
    {
        int newIndex = _selectedIndex;
        do
        {
            newIndex = (newIndex + direction + _menuButtons.Length) % _menuButtons.Length;
        } while (!_menuButtons[newIndex].IsEnabled && newIndex != _selectedIndex);

        _selectedIndex = newIndex;
        UpdateMenuHighlight();
    }

    private void UpdateMenuHighlight()
    {
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            if (i == _selectedIndex)
            {
                _menuButtons[i].Focus();
            }
        }
    }

    private void ActivateSelectedButton()
    {
        var button = _menuButtons[_selectedIndex];
        if (!button.IsEnabled) return;

        button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
    }
}

/// <summary>
/// タイトル画面で選択された操作
/// </summary>
public enum TitleAction
{
    NewGame,
    Continue,
    Settings,
    Quit,
    DebugMap
}
