using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RougelikeGame.Gui;

/// <summary>
/// キーバインド設定ウィンドウ
/// </summary>
public partial class KeyBindingWindow : Window
{
    private KeyBindingSettings _keyBindings;
    private KeyBindAction? _capturingAction;
    private readonly Dictionary<KeyBindAction, Button> _keyButtons = new();

    /// <summary>編集後のキーバインド設定（保存時にセット）</summary>
    public KeyBindingSettings? ResultBindings { get; private set; }

    public KeyBindingWindow(KeyBindingSettings currentBindings)
    {
        InitializeComponent();
        _keyBindings = currentBindings.Clone();
        BuildBindingUI();
    }

    private void BuildBindingUI()
    {
        BindingListPanel.Children.Clear();
        _keyButtons.Clear();

        var groups = new (string Header, KeyBindAction[] Actions)[]
        {
            ("移動", new[] {
                KeyBindAction.MoveUp, KeyBindAction.MoveDown,
                KeyBindAction.MoveLeft, KeyBindAction.MoveRight
            }),
            ("基本操作", new[] {
                KeyBindAction.Wait, KeyBindAction.Pickup,
                KeyBindAction.UseStairs, KeyBindAction.AscendStairs,
                KeyBindAction.AutoExplore, KeyBindAction.Search,
                KeyBindAction.CloseDoor, KeyBindAction.RangedAttack,
                KeyBindAction.ThrowItem, KeyBindAction.StartCasting,
                KeyBindAction.Pray, KeyBindAction.EnterTown
            }),
            ("画面", new[] {
                KeyBindAction.OpenInventory, KeyBindAction.OpenStatus,
                KeyBindAction.OpenMessageLog, KeyBindAction.OpenSkillTree,
                KeyBindAction.OpenWorldMap, KeyBindAction.OpenEncyclopedia,
                KeyBindAction.OpenCompanion, KeyBindAction.OpenDeathLog,
                KeyBindAction.OpenQuestLog, KeyBindAction.OpenReligion
            }),
            ("システム", new[] {
                KeyBindAction.Save, KeyBindAction.Load,
                KeyBindAction.Quit, KeyBindAction.ToggleMinimap,
                KeyBindAction.CycleCombatStance
            }),
            ("スキルスロット", new[] {
                KeyBindAction.SkillSlot1, KeyBindAction.SkillSlot2,
                KeyBindAction.SkillSlot3, KeyBindAction.SkillSlot4,
                KeyBindAction.SkillSlot5, KeyBindAction.SkillSlot6
            }),
        };

        foreach (var (header, actions) in groups)
        {
            var headerText = new TextBlock
            {
                Text = header,
                Foreground = new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60)),
                FontFamily = new FontFamily("Yu Gothic UI"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 6)
            };
            BindingListPanel.Children.Add(headerText);

            foreach (var action in actions)
            {
                var row = CreateBindingRow(action);
                BindingListPanel.Children.Add(row);
            }
        }
    }

    private Grid CreateBindingRow(KeyBindAction action)
    {
        var grid = new Grid { Margin = new Thickness(8, 2, 8, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var label = new TextBlock
        {
            Text = KeyBindingSettings.GetActionDisplayName(action),
            Foreground = new SolidColorBrush(Color.FromRgb(0xc0, 0xc0, 0xd0)),
            FontFamily = new FontFamily("Yu Gothic UI"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        var binding = _keyBindings.Bindings.GetValueOrDefault(action);
        var button = new Button
        {
            Content = binding?.ToString() ?? "未設定",
            Background = new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3e)),
            Foreground = new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xff)),
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            MinWidth = 100,
            Padding = new Thickness(8, 4, 8, 4),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x60)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Tag = action
        };
        button.Click += KeyButton_Click;
        Grid.SetColumn(button, 1);
        grid.Children.Add(button);

        _keyButtons[action] = button;
        return grid;
    }

    private void KeyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not KeyBindAction action)
            return;

        _capturingAction = action;
        CaptureOverlay.Visibility = Visibility.Visible;
        CaptureActionText.Text = $"変更中: {KeyBindingSettings.GetActionDisplayName(action)}";
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (_capturingAction == null)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
            }
            return;
        }

        // キャプチャ中
        if (e.Key == Key.Escape)
        {
            // キャプチャキャンセル
            _capturingAction = null;
            CaptureOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
            return;
        }

        // 修飾キー単体は無視
        if (e.Key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt or Key.System)
        {
            e.Handled = true;
            return;
        }

        var action = _capturingAction.Value;
        var newKey = e.Key;
        var modifiers = Keyboard.Modifiers;

        // 重複チェック（同じキーが他のアクションに割り当てられていないか）
        foreach (var kvp in _keyBindings.Bindings)
        {
            if (kvp.Key != action && kvp.Value.Key == newKey && kvp.Value.Modifiers == modifiers)
            {
                var conflictName = KeyBindingSettings.GetActionDisplayName(kvp.Key);
                var result = MessageBox.Show(
                    $"このキーは「{conflictName}」に既に割り当てられています。\n入れ替えますか？",
                    "キー重複",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // 既存のバインドから削除
                    var oldBinding = _keyBindings.Bindings.GetValueOrDefault(action);
                    if (oldBinding != null)
                    {
                        _keyBindings.Bindings[kvp.Key] = new KeyBinding(oldBinding.Key, oldBinding.Modifiers);
                    }
                    else
                    {
                        _keyBindings.Bindings.Remove(kvp.Key);
                    }
                    // 衝突先のボタン表示も更新
                    if (_keyButtons.TryGetValue(kvp.Key, out var conflictBtn))
                    {
                        var swapped = _keyBindings.Bindings.GetValueOrDefault(kvp.Key);
                        conflictBtn.Content = swapped?.ToString() ?? "未設定";
                    }
                }
                else
                {
                    _capturingAction = null;
                    CaptureOverlay.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                break;
            }
        }

        // バインド更新
        _keyBindings.Bindings[action] = new KeyBinding(newKey, modifiers);

        // ボタン表示更新
        if (_keyButtons.TryGetValue(action, out var btn))
        {
            btn.Content = _keyBindings.Bindings[action].ToString();
        }

        _capturingAction = null;
        CaptureOverlay.Visibility = Visibility.Collapsed;
        e.Handled = true;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ResultBindings = _keyBindings;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _keyBindings = KeyBindingSettings.CreateDefault();
        BuildBindingUI();
    }
}
