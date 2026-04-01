using System.Windows;
using System.Windows.Input;

namespace RougelikeGame.Gui;

/// <summary>
/// ポーズ画面の選択結果
/// </summary>
public enum PauseResult
{
    Resume,
    Save,
    Load,
    Settings,
    KeyBindings,
    ReturnToTitle
}

/// <summary>
/// ポーズ画面ウィンドウ
/// </summary>
public partial class PauseWindow : Window
{
    /// <summary>選択されたアクション</summary>
    public PauseResult Result { get; private set; } = PauseResult.Resume;

    /// <summary>キーバインド変更結果（KeyBindingsが選択され保存された場合）</summary>
    public KeyBindingSettings? UpdatedKeyBindings { get; private set; }

    private readonly KeyBindingSettings _currentKeyBindings;

    public PauseWindow(KeyBindingSettings currentKeyBindings)
    {
        InitializeComponent();
        _currentKeyBindings = currentKeyBindings;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Result = PauseResult.Resume;
            DialogResult = true;
            Close();
            e.Handled = true;
        }
    }

    private void ResumeButton_Click(object sender, RoutedEventArgs e)
    {
        Result = PauseResult.Resume;
        DialogResult = true;
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Result = PauseResult.Save;
        DialogResult = true;
        Close();
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        Result = PauseResult.Load;
        DialogResult = true;
        Close();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }

    private void KeyBindButton_Click(object sender, RoutedEventArgs e)
    {
        var keyBindWindow = new KeyBindingWindow(_currentKeyBindings) { Owner = this };
        if (keyBindWindow.ShowDialog() == true && keyBindWindow.ResultBindings != null)
        {
            UpdatedKeyBindings = keyBindWindow.ResultBindings;
        }
    }

    private void TitleButton_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "タイトル画面に戻りますか？\nセーブされていない進行状況は失われます。",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            Result = PauseResult.ReturnToTitle;
            DialogResult = true;
            Close();
        }
    }
}
