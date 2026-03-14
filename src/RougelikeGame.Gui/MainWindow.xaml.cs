using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Gui;

/// <summary>
/// メインゲームウィンドウ
/// </summary>
public partial class MainWindow : Window
{
    private readonly GameController _gameController;
    private readonly GameRenderer _renderer;
    private readonly List<string> _messageHistory = new();
    private const int MaxMessages = 50;
    private bool _minimapVisible = true;
    private DispatcherTimer? _autoExploreTimer;

    public MainWindow()
    {
        InitializeComponent();

        _gameController = new GameController();
        _renderer = new GameRenderer(GameCanvas);

        // イベント購読
        _gameController.OnMessage += AddMessage;
        _gameController.OnStateChanged += UpdateDisplay;
        _gameController.OnGameOver += OnGameOver;
        _gameController.OnShowInventory += ShowInventoryDialog;
        _gameController.OnShowStatus += ShowStatusDialog;
        _gameController.OnShowMessageLog += ShowMessageLogDialog;
        _gameController.OnSaveGame += HandleSaveGame;
        _gameController.OnLoadGame += HandleLoadGame;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _gameController.Initialize();
        UpdateDisplay();
        Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        GameAction? action = null;

        // WASD斜め移動判定（同時押し）
        if (e.Key == Key.W || e.Key == Key.Up)
        {
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left))
                action = GameAction.MoveUpLeft;
            else if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right))
                action = GameAction.MoveUpRight;
            else
                action = GameAction.MoveUp;
        }
        else if (e.Key == Key.S || e.Key == Key.Down)
        {
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left))
                action = GameAction.MoveDownLeft;
            else if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right))
                action = GameAction.MoveDownRight;
            else
                action = GameAction.MoveDown;
        }
        else if (e.Key == Key.A || e.Key == Key.Left)
        {
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
                action = GameAction.MoveUpLeft;
            else if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
                action = GameAction.MoveDownLeft;
            else
                action = GameAction.MoveLeft;
        }
        else if (e.Key == Key.D || e.Key == Key.Right)
        {
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up))
                action = GameAction.MoveUpRight;
            else if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down))
                action = GameAction.MoveDownRight;
            else
                action = GameAction.MoveRight;
        }
        else
        {
            action = e.Key switch
            {
                Key.Space => GameAction.Wait,
                Key.G => GameAction.Pickup,
                Key.I => GameAction.OpenInventory,
                Key.C => GameAction.OpenStatus,
                Key.L => GameAction.OpenMessageLog,
                Key.Tab => GameAction.AutoExplore,
                Key.F5 => GameAction.Save,
                Key.F9 => GameAction.Load,
                Key.Q => GameAction.Quit,
                Key.OemPeriod when Keyboard.Modifiers == ModifierKeys.Shift => GameAction.UseStairs,
                Key.OemComma when Keyboard.Modifiers == ModifierKeys.Shift => GameAction.AscendStairs,
                _ => null
            };

            // ミニマップ切り替え（GameActionを経由しない直接処理）
            if (e.Key == Key.M)
            {
                _minimapVisible = !_minimapVisible;
                MinimapBorder.Visibility = _minimapVisible ? Visibility.Visible : Visibility.Collapsed;
                e.Handled = true;
                return;
            }
        }

        if (action.HasValue)
        {
            _gameController.ProcessInput(action.Value);
            e.Handled = true;

            // 自動探索タイマーの管理
            if (_gameController.IsAutoExploring)
            {
                StartAutoExploreTimer();
            }
            else
            {
                StopAutoExploreTimer();
            }
        }
    }

    private void UpdateDisplay()
    {
        // ステータス更新
        FloorText.Text = $"第{_gameController.CurrentFloor}層";
        DateText.Text = _gameController.GameTime.ToFullString();
        TimePeriodText.Text = _gameController.GameTime.TimePeriod;

        // レベル・経験値
        LevelText.Text = $"{_gameController.Player.Level}";
        ExpText.Text = $"{_gameController.Player.Experience}/{_gameController.Player.ExperienceToNextLevel}";

        // HP
        HpText.Text = $"{_gameController.Player.CurrentHp}/{_gameController.Player.MaxHp}";
        double hpRatio = (double)_gameController.Player.CurrentHp / _gameController.Player.MaxHp;
        HpText.Foreground = hpRatio switch
        {
            > 0.6 => System.Windows.Media.Brushes.LimeGreen,
            > 0.3 => System.Windows.Media.Brushes.Yellow,
            _ => System.Windows.Media.Brushes.Red
        };

        // MP/SP
        MpText.Text = $"{_gameController.Player.CurrentMp}/{_gameController.Player.MaxMp}";
        SpText.Text = $"{_gameController.Player.CurrentSp}/{_gameController.Player.MaxSp}";

        // 満腹度（色変え）
        HungerText.Text = $"{_gameController.Player.Hunger}";
        HungerText.Foreground = _gameController.Player.HungerStage switch
        {
            RougelikeGame.Core.HungerStage.Full => System.Windows.Media.Brushes.LimeGreen,
            RougelikeGame.Core.HungerStage.Normal => System.Windows.Media.Brushes.Yellow,
            RougelikeGame.Core.HungerStage.Hungry => System.Windows.Media.Brushes.Orange,
            RougelikeGame.Core.HungerStage.Starving => System.Windows.Media.Brushes.OrangeRed,
            _ => System.Windows.Media.Brushes.Red
        };

        // 正気度（色変え）
        SanityText.Text = $"{_gameController.Player.Sanity}";
        SanityText.Foreground = _gameController.Player.SanityStage switch
        {
            RougelikeGame.Core.SanityStage.Normal => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xC0, 0xA0, 0xFF)),
            RougelikeGame.Core.SanityStage.Uneasy => System.Windows.Media.Brushes.Yellow,
            RougelikeGame.Core.SanityStage.Anxious => System.Windows.Media.Brushes.Orange,
            RougelikeGame.Core.SanityStage.Unstable => System.Windows.Media.Brushes.OrangeRed,
            RougelikeGame.Core.SanityStage.Madness => System.Windows.Media.Brushes.Red,
            _ => System.Windows.Media.Brushes.DarkRed
        };

        // ターン制限残り日数
        if (_gameController.IsTurnLimitRemoved)
        {
            TurnLimitText.Text = "制限なし";
            TurnLimitText.Foreground = System.Windows.Media.Brushes.Gray;
        }
        else
        {
            int remainingDays = _gameController.RemainingDays;
            TurnLimitText.Text = $"残り{remainingDays}日";
            TurnLimitText.Foreground = remainingDays switch
            {
                > 180 => System.Windows.Media.Brushes.LimeGreen,
                > 90 => System.Windows.Media.Brushes.Yellow,
                > 30 => System.Windows.Media.Brushes.Orange,
                _ => System.Windows.Media.Brushes.Red
            };
        }

        // マップ描画
        _renderer.Render(
            _gameController.Map,
            _gameController.Player,
            _gameController.Enemies,
            _gameController.GroundItems
        );

        // ミニマップ描画
        if (_minimapVisible)
        {
            _renderer.RenderMinimap(
                MinimapCanvas,
                _gameController.Map,
                _gameController.Player,
                _gameController.Enemies
            );
        }
    }

    private void AddMessage(string message)
    {
        _messageHistory.Add(message);
        if (_messageHistory.Count > MaxMessages)
        {
            _messageHistory.RemoveAt(0);
        }

        MessageLog.Text = string.Join("\n", _messageHistory.TakeLast(5));
        MessageScroller.ScrollToEnd();
    }

    private void OnGameOver()
    {
        string result;
        if (_gameController.IsGameOver && !_gameController.Player.IsAlive)
        {
            result = $"ゲームオーバー\n\nあなたは第{_gameController.CurrentFloor}層で力尽きた...\n{_gameController.GameTime.ToFullString()}";
        }
        else if (_gameController.IsGameOver && _gameController.Player.IsAlive)
        {
            // ターン制限超過などによるゲームオーバー（プレイヤーは生存中）
            result = $"ゲームオーバー\n\n時間切れ — 世界の崩壊に巻き込まれた...\n到達階層: 第{_gameController.CurrentFloor}層\n{_gameController.GameTime.ToFullString()}";
        }
        else
        {
            result = $"冒険終了\n\n到達階層: 第{_gameController.CurrentFloor}層\n{_gameController.GameTime.ToFullString()}";
        }

        MessageBox.Show(result, "ローグライクゲーム", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void ShowInventoryDialog(List<Item> items)
    {
        StopAutoExploreTimer();
        var dialog = new InventoryWindow(items, _gameController.Player);
        dialog.Owner = this;

        if (dialog.ShowDialog() == true && dialog.SelectedIndex >= 0)
        {
            _gameController.UseItem(dialog.SelectedIndex);
        }

        Focus();
    }

    private void ShowStatusDialog()
    {
        StopAutoExploreTimer();
        var dialog = new StatusWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void ShowMessageLogDialog(List<string> messages)
    {
        StopAutoExploreTimer();
        var dialog = new MessageLogWindow(messages);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void StartAutoExploreTimer()
    {
        if (_autoExploreTimer != null) return;

        _autoExploreTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80)
        };
        _autoExploreTimer.Tick += AutoExploreTimer_Tick;
        _autoExploreTimer.Start();
    }

    private void StopAutoExploreTimer()
    {
        if (_autoExploreTimer != null)
        {
            _autoExploreTimer.Stop();
            _autoExploreTimer.Tick -= AutoExploreTimer_Tick;
            _autoExploreTimer = null;
        }
    }

    private void AutoExploreTimer_Tick(object? sender, EventArgs e)
    {
        if (!_gameController.IsAutoExploring)
        {
            StopAutoExploreTimer();
            return;
        }

        _gameController.ProcessInput(GameAction.AutoExplore);

        if (!_gameController.IsAutoExploring)
        {
            StopAutoExploreTimer();
        }
    }

    private void HandleSaveGame()
    {
        try
        {
            var saveData = _gameController.CreateSaveData();
            SaveManager.Save(saveData);
            AddMessage("💾 ゲームをセーブした");
        }
        catch (Exception ex)
        {
            AddMessage($"⚠ セーブに失敗: {ex.Message}");
        }
    }

    private void HandleLoadGame()
    {
        try
        {
            var saveData = SaveManager.Load();
            if (saveData == null)
            {
                AddMessage("⚠ セーブデータが見つからない");
                return;
            }

            _gameController.LoadSaveData(saveData);
            _messageHistory.Clear();
            AddMessage("💾 セーブデータをロードした");
        }
        catch (Exception ex)
        {
            AddMessage($"⚠ ロードに失敗: {ex.Message}");
        }
    }
}
