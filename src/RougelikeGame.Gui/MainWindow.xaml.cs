using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;
using RougelikeGame.Engine.Magic;
using RougelikeGame.Gui.Audio;

namespace RougelikeGame.Gui;

/// <summary>
/// メインゲームウィンドウ
/// </summary>
public partial class MainWindow : Window
{
    private readonly GameController _gameController;
    private readonly GameRenderer _renderer;
    private readonly GameSettings _settings;
    private readonly IAudioManager _audioManager;
    private readonly bool _loadSave;
    private readonly bool _debugMap;
    private readonly string _playerName;
    private readonly Race _playerRace;
    private readonly RougelikeGame.Core.CharacterClass _playerClass;
    private readonly RougelikeGame.Core.Background _playerBackground;
    private readonly DifficultyLevel _difficulty;
    private readonly int _saveSlot;
    private readonly List<string> _messageHistory = new();
    private const int MaxMessages = 50;
    private bool _minimapVisible = true;
    private DispatcherTimer? _autoExploreTimer;

    public MainWindow() : this(GameSettings.CreateDefault(), new SilentAudioManager(), false, false)
    {
    }

    public MainWindow(GameSettings settings, IAudioManager audioManager, bool loadSave, bool debugMap = false,
        string playerName = "冒険者", Race playerRace = Race.Human,
        RougelikeGame.Core.CharacterClass playerClass = RougelikeGame.Core.CharacterClass.Fighter,
        RougelikeGame.Core.Background playerBackground = RougelikeGame.Core.Background.Adventurer,
        DifficultyLevel difficulty = DifficultyLevel.Normal,
        int saveSlot = 0)
    {
        InitializeComponent();

        _settings = settings;
        _audioManager = audioManager;
        _loadSave = loadSave;
        _debugMap = debugMap;
        _playerName = playerName;
        _playerRace = playerRace;
        _playerClass = playerClass;
        _playerBackground = playerBackground;
        _difficulty = difficulty;
        _saveSlot = saveSlot;

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

        // 追加イベント購読
        _gameController.OnCastingStarted += OnCastingStarted;
        _gameController.OnCastingEnded += OnCastingEnded;
        _gameController.OnSpellPreviewUpdated += OnSpellPreviewUpdated;
        _gameController.OnShowWorldMap += ShowWorldMapDialog;
        _gameController.OnShowDialogue += OnShowDialogue;
        _gameController.OnQuestUpdated += OnQuestUpdated;
        _gameController.OnGuildRankUp += OnGuildRankUp;
        _gameController.OnShowCrafting += ShowCraftingDialog;
        _gameController.OnCraftingResult += OnCraftingResult;
        _gameController.OnEnhancementResult += OnEnhancementResult;
        _gameController.OnEnchantmentResult += OnEnchantmentResult;
        _gameController.OnShowTutorial += OnShowTutorial;
        _gameController.OnReligionChanged += OnReligionChanged;
        _gameController.OnTerritoryChanged += OnTerritoryChanged;

        // 新システム画面イベント購読
        _gameController.OnShowEncyclopedia += ShowEncyclopediaDialog;
        _gameController.OnShowDeathLog += ShowDeathLogDialog;
        _gameController.OnShowSkillTree += ShowSkillTreeDialog;
        _gameController.OnShowCompanion += ShowCompanionDialog;
        _gameController.OnShowCooking += ShowCookingDialog;
        _gameController.OnShowBaseConstruction += ShowBaseConstructionDialog;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // デバッグマップモードの場合は小さい固定マップで初期化
        if (_debugMap)
            _gameController.InitializeDebug();
        else
            _gameController.Initialize(_playerName, _playerRace, _playerClass, _playerBackground, _difficulty);

        // セーブデータからのコンティニュー
        if (_loadSave)
        {
            try
            {
                var saveData = SaveManager.Load(_saveSlot);
                if (saveData != null)
                {
                    _gameController.LoadSaveData(saveData);
                    AddMessage("💾 セーブデータをロードした");
                }
            }
            catch (Exception ex)
            {
                AddMessage($"⚠ ロードに失敗: {ex.Message}");
            }
        }

        // ダンジョン探索BGM開始
        _audioManager.PlayBgm(BgmIds.DungeonNormal);

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
                Key.F => GameAction.Search,
                Key.X => GameAction.CloseDoor,
                Key.R => GameAction.RangedAttack,
                Key.T => GameAction.ThrowItem,
                Key.V => GameAction.StartCasting,
                Key.P => GameAction.Pray,
                Key.E => GameAction.UseSkill,
                Key.J => GameAction.OpenWorldMap,
                Key.H => GameAction.OpenCrafting,
                Key.N => GameAction.RegisterGuild,
                Key.Y => GameAction.OpenEncyclopedia,
                Key.U => GameAction.OpenCompanion,
                Key.Z => GameAction.OpenDeathLog,
                _ => null
            };

            // クエストログ（直接ダイアログ表示）
            if (e.Key == Key.K)
            {
                ShowQuestLogDialog();
                e.Handled = true;
                return;
            }

            // 宗教画面（直接ダイアログ表示）
            if (e.Key == Key.O)
            {
                ShowReligionDialog();
                e.Handled = true;
                return;
            }

            // 街入場（直接ダイアログ表示）
            if (e.Key == Key.B)
            {
                ShowTownDialog();
                e.Handled = true;
                return;
            }

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

        // 領地名・地上/ダンジョン表示
        var territory = TerritoryDefinition.Get(_gameController.CurrentTerritory);
        TerritoryText.Text = territory.Name;
        if (_gameController.IsOnSurface)
        {
            SurfaceStatusText.Text = "【地上】";
            SurfaceStatusText.Foreground = System.Windows.Media.Brushes.LimeGreen;
        }
        else
        {
            SurfaceStatusText.Text = $"【B{_gameController.CurrentFloor}F】";
            SurfaceStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xff, 0x6b, 0x6b));
        }

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

        // 所持金
        GoldText.Text = $"{_gameController.Player.Gold:N0}G";

        // 重量
        var inventory = (RougelikeGame.Core.Entities.Inventory)_gameController.Player.Inventory;
        float currentWeight = inventory.TotalWeight;
        float maxWeight = _gameController.Player.CalculateMaxWeight();
        WeightText.Text = $"{currentWeight:F1}/{maxWeight:F1}kg";
        WeightText.Foreground = currentWeight > maxWeight
            ? System.Windows.Media.Brushes.Red
            : currentWeight > maxWeight * 0.8f
                ? System.Windows.Media.Brushes.Orange
                : System.Windows.Media.Brushes.LightBlue;

        // === 新システム表示更新 ===

        // 季節表示
        SeasonText.Text = SeasonSystem.GetSeasonName(_gameController.CurrentSeason);
        SeasonText.Foreground = _gameController.CurrentSeason switch
        {
            Season.Spring => System.Windows.Media.Brushes.LimeGreen,
            Season.Summer => System.Windows.Media.Brushes.Orange,
            Season.Autumn => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xDA, 0xA5, 0x20)),
            Season.Winter => System.Windows.Media.Brushes.LightBlue,
            _ => System.Windows.Media.Brushes.White
        };

        // 天候表示
        WeatherText.Text = _gameController.CurrentWeatherName;
        WeatherText.Foreground = _gameController.CurrentWeather switch
        {
            Weather.Clear => System.Windows.Media.Brushes.LightBlue,
            Weather.Rain => System.Windows.Media.Brushes.DodgerBlue,
            Weather.Snow => System.Windows.Media.Brushes.White,
            Weather.Storm => System.Windows.Media.Brushes.DarkOrange,
            Weather.Fog => System.Windows.Media.Brushes.Gray,
            _ => System.Windows.Media.Brushes.LightBlue
        };

        // 渇き表示
        ThirstText.Text = ThirstSystem.GetThirstName(ThirstLevel.Hydrated);
        ThirstText.Foreground = System.Windows.Media.Brushes.DeepSkyBlue;

        // カルマ表示
        KarmaText.Text = KarmaSystem.GetKarmaRankName(_gameController.PlayerKarmaRank);
        KarmaText.Foreground = _gameController.PlayerKarma switch
        {
            > 50 => System.Windows.Media.Brushes.Gold,
            > 0 => System.Windows.Media.Brushes.LightGreen,
            0 => System.Windows.Media.Brushes.Silver,
            > -50 => System.Windows.Media.Brushes.Orange,
            _ => System.Windows.Media.Brushes.Red
        };

        // 仲間数表示
        CompanionCountText.Text = $"{_gameController.CompanionCount}";
        CompanionCountText.Foreground = _gameController.CompanionCount > 0
            ? System.Windows.Media.Brushes.Gold
            : System.Windows.Media.Brushes.Gray;

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
        _audioManager.StopBgm();
        _audioManager.PlayBgm(BgmIds.GameOver, loop: false);

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

    #region 追加イベントハンドラ

    private void OnCastingStarted()
    {
        StopAutoExploreTimer();
        var dialog = new SpellCastingWindow(_gameController);
        dialog.Owner = this;

        if (dialog.ShowDialog() == true && dialog.CastRequested)
        {
            _gameController.ProcessInput(GameAction.CastSpell);
        }
        else
        {
            _gameController.ProcessInput(GameAction.CancelCasting);
        }

        Focus();
    }

    private void OnCastingEnded()
    {
        // 詠唱終了通知（UIの状態リセット等があれば追加）
    }

    private void OnSpellPreviewUpdated(SpellPreview preview)
    {
        // プレビュー更新（SpellCastingWindow内で処理するため基本的に空）
    }

    private void ShowWorldMapDialog()
    {
        StopAutoExploreTimer();
        var dialog = new WorldMapWindow(_gameController);
        dialog.Owner = this;

        if (dialog.ShowDialog() == true)
        {
            if (dialog.EnterTownRequested)
            {
                ShowTownDialog();
            }
            else if (dialog.TravelDestination.HasValue)
            {
                _gameController.TryTravelTo(dialog.TravelDestination.Value);
            }
        }

        Focus();
    }

    private void OnShowDialogue(DialogueNode node)
    {
        StopAutoExploreTimer();

        // NPCのIDを取得（現在の領地のNPCから話者名で検索）
        string npcId = "";
        var npcs = _gameController.GetNpcsInCurrentTerritory();
        var matchNpc = npcs.FirstOrDefault(n => n.Name == node.SpeakerName);
        if (matchNpc != null) npcId = matchNpc.Id;

        var dialog = new DialogueWindow(_gameController, node, npcId);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void OnQuestUpdated(string questId)
    {
        AddMessage($"📋 クエスト更新: {questId}");
    }

    private void OnGuildRankUp(GuildRank newRank)
    {
        string rankName = newRank switch
        {
            GuildRank.Copper => "銅",
            GuildRank.Iron => "鉄",
            GuildRank.Silver => "銀",
            GuildRank.Gold => "金",
            GuildRank.Platinum => "白金",
            GuildRank.Mythril => "ミスリル",
            GuildRank.Adamantine => "アダマンタイト",
            _ => newRank.ToString()
        };
        AddMessage($"🎉 ギルドランクが{rankName}に昇格した！");
    }

    private void ShowCraftingDialog()
    {
        StopAutoExploreTimer();
        var dialog = new CraftingWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void OnCraftingResult(CraftingResult result)
    {
        if (result.Success && result.ResultItem != null)
        {
            AddMessage($"🔨 合成成功: {result.ResultItem.Name}");
        }
    }

    private void OnEnhancementResult(EnhancementResult result)
    {
        if (result.Success)
        {
            AddMessage($"⚔ 強化成功: +{result.NewLevel}");
        }
    }

    private void OnEnchantmentResult(EnchantmentResult result)
    {
        if (result.Success)
        {
            AddMessage($"✨ 付与成功: {result.Element}属性");
        }
    }

    private void OnShowTutorial(TutorialStep step)
    {
        AddMessage($"💡 【ヒント】{step.Title}");
        MessageBox.Show($"{step.Title}\n\n{step.Message}", "ヒント",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnReligionChanged()
    {
        AddMessage("⛪ 信仰状態が変更された");
    }

    private void OnTerritoryChanged(TerritoryId newTerritory)
    {
        var territory = TerritoryDefinition.Get(newTerritory);
        AddMessage($"🗺️ {territory.Name}に到着した");
        _audioManager.PlaySe("territory_change");
    }

    private void ShowTownDialog()
    {
        if (!_gameController.IsOnSurface)
        {
            AddMessage("※ ダンジョン内から街に入ることはできません");
            return;
        }

        var territory = TerritoryDefinition.Get(_gameController.CurrentTerritory);
        if (territory.AvailableFacilities.Length == 0)
        {
            AddMessage("※ この領地には街の施設がありません");
            return;
        }

        StopAutoExploreTimer();
        var dialog = new TownWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();

        // TownWindowからの店開きリクエスト処理
        if (dialog.OpenShopRequest.HasValue)
        {
            var shopDialog = new ShopWindow(_gameController, dialog.OpenShopRequest.Value);
            shopDialog.Owner = this;
            shopDialog.ShowDialog();
        }

        // ダンジョン入場リクエスト処理
        if (dialog.EnterDungeonRequested)
        {
            _gameController.ProcessInput(GameAction.UseStairs);
        }

        Focus();
    }

    private void ShowQuestLogDialog()
    {
        StopAutoExploreTimer();
        var dialog = new QuestLogWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void ShowReligionDialog()
    {
        StopAutoExploreTimer();
        var dialog = new ReligionWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    #endregion

    #region 新システム画面

    private void ShowEncyclopediaDialog()
    {
        StopAutoExploreTimer();
        var enc = _gameController.GetEncyclopediaSystem();
        var sb = new StringBuilder();
        sb.AppendLine("【図鑑】");
        sb.AppendLine($"総エントリ数: {enc.TotalEntries}");
        foreach (var cat in Enum.GetValues<EncyclopediaCategory>())
        {
            var entries = enc.GetByCategory(cat);
            if (entries.Count > 0)
            {
                float rate = enc.GetDiscoveryRate(cat);
                sb.AppendLine($"  {cat}: {entries.Count}件 (発見率: {rate:P0})");
            }
        }
        MessageBox.Show(sb.ToString(), "図鑑", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    private void ShowDeathLogDialog()
    {
        StopAutoExploreTimer();
        var log = _gameController.GetDeathLogSystem();
        var sb = new StringBuilder();
        sb.AppendLine("【死亡記録】");
        sb.AppendLine($"総死亡回数: {log.TotalDeaths}");
        if (log.TotalDeaths > 0)
        {
            sb.AppendLine($"最高到達レベル: {log.GetHighestLevel()}");
            sb.AppendLine($"最深到達階層: {log.GetDeepestFloor()}");
            sb.AppendLine($"平均生存ターン: {log.GetAverageSurvivalTurns():F0}");
            var common = log.GetMostCommonCause();
            if (common.HasValue)
                sb.AppendLine($"最多死因: {common.Value}");
        }
        foreach (var entry in log.AllLogs.TakeLast(5))
        {
            sb.AppendLine($"  #{entry.RunNumber} {entry.CharacterName}(Lv{entry.Level}) - {entry.CauseDetail} @{entry.Location} B{entry.Floor}F");
        }
        MessageBox.Show(sb.ToString(), "死亡記録", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    private void ShowSkillTreeDialog()
    {
        StopAutoExploreTimer();
        var tree = _gameController.GetSkillTreeSystem();
        var sb = new StringBuilder();
        sb.AppendLine("【スキルツリー】");
        sb.AppendLine($"利用可能スキルポイント: {tree.AvailablePoints}");
        sb.AppendLine($"解放済みノード数: {tree.UnlockedCount}");
        foreach (var nodeId in tree.UnlockedNodes)
        {
            if (tree.AllNodes.TryGetValue(nodeId, out var node))
            {
                sb.AppendLine($"  ✓ {node.Name} ({node.NodeType})");
            }
        }
        MessageBox.Show(sb.ToString(), "スキルツリー", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    private void ShowCompanionDialog()
    {
        StopAutoExploreTimer();
        var comp = _gameController.GetCompanionSystem();
        var sb = new StringBuilder();
        sb.AppendLine("【仲間一覧】");
        sb.AppendLine($"パーティ: {comp.Party.Count}/{CompanionSystem.MaxPartySize}");
        foreach (var c in comp.Party)
        {
            sb.AppendLine($"  {c.Name} (Lv{c.Level}) - {CompanionSystem.GetTypeName(c.Type)} / AI:{c.AIMode} / 忠誠:{c.Loyalty}");
        }
        if (comp.Party.Count == 0)
            sb.AppendLine("  仲間はまだいません");
        MessageBox.Show(sb.ToString(), "仲間管理", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    private void ShowCookingDialog()
    {
        StopAutoExploreTimer();
        var sb = new StringBuilder();
        sb.AppendLine("【料理】");
        sb.AppendLine("利用可能な調理法:");
        foreach (var method in Enum.GetValues<CookingMethod>())
        {
            sb.AppendLine($"  {CookingSystem.GetMethodName(method)} (所要ターン: {CookingSystem.GetCookingTime(method)})");
        }
        MessageBox.Show(sb.ToString(), "料理", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    private void ShowBaseConstructionDialog()
    {
        StopAutoExploreTimer();
        var baseSys = _gameController.GetBaseConstructionSystem();
        var sb = new StringBuilder();
        sb.AppendLine("【拠点管理】");
        sb.AppendLine($"建設済み施設数: {baseSys.BuiltFacilities.Count}");
        sb.AppendLine("建設済み施設:");
        foreach (var category in baseSys.BuiltFacilities)
        {
            var def = BaseConstructionSystem.GetDefinition(category);
            if (def != null)
                sb.AppendLine($"  {def.Name}");
        }
        if (baseSys.BuiltFacilities.Count == 0)
            sb.AppendLine("  まだ施設がありません");
        MessageBox.Show(sb.ToString(), "拠点管理", MessageBoxButton.OK, MessageBoxImage.Information);
        Focus();
    }

    #endregion
}
