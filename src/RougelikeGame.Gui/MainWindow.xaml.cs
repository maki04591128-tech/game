using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;
using RougelikeGame.Engine.Magic;
using RougelikeGame.Gui.Audio;

namespace RougelikeGame.Gui;

/// <summary>
/// ゲーム終了時の理由
/// </summary>
public enum GameExitReason
{
    /// <summary>アプリケーション終了</summary>
    Quit,
    /// <summary>タイトル画面に戻る</summary>
    ReturnToTitle,
    /// <summary>NG+を開始する</summary>
    StartNewGamePlus
}

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
    private readonly NewGamePlusTier? _ngPlusTier;

    /// <summary>ゲーム終了時の理由</summary>
    public GameExitReason ExitReason { get; private set; } = GameExitReason.Quit;

    /// <summary>NG+開始時の段階（ExitReason=StartNewGamePlusの場合に使用）</summary>
    public NewGamePlusTier? NgPlusTier { get; private set; }
    private readonly List<string> _messageHistory = new();
    private const int MaxMessages = 50;
    private bool _minimapVisible = true;
    private DispatcherTimer? _autoExploreTimer;
    private readonly HashSet<Key> _heldMovementKeys = new();
    private Dictionary<int, (int GridX, int GridY)> _inventoryGridPositions = new();
    private bool _inventorySorted;
    private KeyBindingSettings _keyBindings = KeyBindingSettings.Load();

    public MainWindow() : this(GameSettings.CreateDefault(), new SilentAudioManager(), false, false)
    {
    }

    public MainWindow(GameSettings settings, IAudioManager audioManager, bool loadSave, bool debugMap = false,
        string playerName = "冒険者", Race playerRace = Race.Human,
        RougelikeGame.Core.CharacterClass playerClass = RougelikeGame.Core.CharacterClass.Fighter,
        RougelikeGame.Core.Background playerBackground = RougelikeGame.Core.Background.Adventurer,
        DifficultyLevel difficulty = DifficultyLevel.Normal,
        int saveSlot = 0,
        NewGamePlusTier? ngPlusTier = null)
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
        _ngPlusTier = ngPlusTier;

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
        _gameController.OnOpenShop += OnOpenShop;
        _gameController.OnQuestUpdated += OnQuestUpdated;
        _gameController.OnGuildRankUp += OnGuildRankUp;
        _gameController.OnCraftingResult += OnCraftingResult;
        _gameController.OnEnhancementResult += OnEnhancementResult;
        _gameController.OnEnchantmentResult += OnEnchantmentResult;
        _gameController.OnShowCrafting += OnShowCrafting;
        _gameController.OnShowTutorial += OnShowTutorial;
        _gameController.OnReligionChanged += OnReligionChanged;
        _gameController.OnTerritoryChanged += OnTerritoryChanged;

        // ゲームクリアイベント購読
        _gameController.OnGameClear += OnGameClear;

        // シンボルマップイベント購読
        _gameController.OnLocationArrived += OnLocationArrived;
        _gameController.OnSymbolMapEnterTown += OnSymbolMapEnterTown;
        _gameController.OnSymbolMapEnterDungeon += OnSymbolMapEnterDungeon;

        // ウィンドウ再アクティブ時にキー状態をリセット（ShowDialog後の斜め移動バグ防止）
        Activated += (_, _) => _heldMovementKeys.Clear();

        // 新システム画面イベント購読
        _gameController.OnShowEncyclopedia += ShowEncyclopediaDialog;
        _gameController.OnShowDeathLog += ShowDeathLogDialog;
        _gameController.OnShowSkillTree += ShowSkillTreeDialog;
        _gameController.OnShowCompanion += ShowCompanionDialog;
        _gameController.OnShowCooking += ShowCookingDialog;
        _gameController.OnShowBaseConstruction += ShowBaseConstructionDialog;
        _gameController.OnShowRecruitCompanion += ShowRecruitCompanionDialog;
        _gameController.OnShowQuestBoard += ShowQuestBoardDialog;
        _gameController.OnShowVocabulary += ShowVocabularyDialog;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // デバッグマップモードの場合は小さい固定マップで初期化
        if (_debugMap)
            _gameController.InitializeDebug();
        else if (_ngPlusTier.HasValue)
            _gameController.InitializeNewGamePlus(_playerName, _playerRace, _playerClass, _playerBackground, _difficulty, _ngPlusTier.Value);
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

        // Ver.β 視覚エフェクト: プレイヤーイベント購読
        _gameController.Player.OnDamaged += OnPlayerDamaged;
        _gameController.Player.OnSanityStageChanged += OnPlayerSanityStageChangedEffect;
        _gameController.Player.OnStatusEffectApplied += OnPlayerStatusEffectApplied;

        // Ver.β 視覚エフェクト: ゲームコントローラーイベント購読（β.13/β.20）
        _gameController.OnFloorChanged += OnFloorChangedEffect;
        _gameController.OnPlayerDied += OnPlayerDiedEffect;
        _gameController.OnPlayerRebirthed += OnPlayerRebirthedEffect;

        UpdateDisplay();
        Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        GameAction? action = null;

        // ESCキーでポーズ画面を開く
        if (e.Key == Key.Escape)
        {
            ShowPauseDialog();
            e.Handled = true;
            return;
        }

        // 移動キーバインド取得
        var moveUpKey = _keyBindings.Bindings.GetValueOrDefault(KeyBindAction.MoveUp)?.Key ?? Key.W;
        var moveDownKey = _keyBindings.Bindings.GetValueOrDefault(KeyBindAction.MoveDown)?.Key ?? Key.S;
        var moveLeftKey = _keyBindings.Bindings.GetValueOrDefault(KeyBindAction.MoveLeft)?.Key ?? Key.A;
        var moveRightKey = _keyBindings.Bindings.GetValueOrDefault(KeyBindAction.MoveRight)?.Key ?? Key.D;

        // 移動キーの押下状態を追跡
        if (e.Key == moveUpKey || e.Key == moveDownKey || e.Key == moveLeftKey || e.Key == moveRightKey
            || e.Key is Key.Up or Key.Down or Key.Left or Key.Right)
        {
            _heldMovementKeys.Add(e.Key);
        }

        // WASD斜め移動判定（同時押し：自前追跡で判定）
        bool isUp = e.Key == moveUpKey || e.Key == Key.Up;
        bool isDown = e.Key == moveDownKey || e.Key == Key.Down;
        bool isLeft = e.Key == moveLeftKey || e.Key == Key.Left;
        bool isRight = e.Key == moveRightKey || e.Key == Key.Right;

        bool holdLeft = _heldMovementKeys.Contains(moveLeftKey) || _heldMovementKeys.Contains(Key.Left);
        bool holdRight = _heldMovementKeys.Contains(moveRightKey) || _heldMovementKeys.Contains(Key.Right);
        bool holdUp = _heldMovementKeys.Contains(moveUpKey) || _heldMovementKeys.Contains(Key.Up);
        bool holdDown = _heldMovementKeys.Contains(moveDownKey) || _heldMovementKeys.Contains(Key.Down);

        if (isUp)
        {
            if (holdLeft) action = GameAction.MoveUpLeft;
            else if (holdRight) action = GameAction.MoveUpRight;
            else action = GameAction.MoveUp;
        }
        else if (isDown)
        {
            if (holdLeft) action = GameAction.MoveDownLeft;
            else if (holdRight) action = GameAction.MoveDownRight;
            else action = GameAction.MoveDown;
        }
        else if (isLeft)
        {
            if (holdUp) action = GameAction.MoveUpLeft;
            else if (holdDown) action = GameAction.MoveDownLeft;
            else action = GameAction.MoveLeft;
        }
        else if (isRight)
        {
            if (holdUp) action = GameAction.MoveUpRight;
            else if (holdDown) action = GameAction.MoveDownRight;
            else action = GameAction.MoveRight;
        }
        else
        {
            // キーバインド辞書からアクションを検索
            var modifiers = Keyboard.Modifiers;
            var bindAction = _keyBindings.FindAction(e.Key, modifiers);

            if (bindAction != null)
            {
                action = bindAction.Value switch
                {
                    KeyBindAction.Wait => GameAction.Wait,
                    KeyBindAction.Pickup => GameAction.Pickup,
                    KeyBindAction.UseStairs => GameAction.UseStairs,
                    KeyBindAction.AscendStairs => GameAction.AscendStairs,
                    KeyBindAction.AutoExplore => GameAction.AutoExplore,
                    KeyBindAction.Search => GameAction.Search,
                    KeyBindAction.CloseDoor => GameAction.CloseDoor,
                    KeyBindAction.RangedAttack => GameAction.RangedAttack,
                    KeyBindAction.ThrowItem => _gameController.IsOnSurface ? GameAction.EnterTown : GameAction.ThrowItem,
                    KeyBindAction.EnterTown => _gameController.IsOnSurface ? GameAction.EnterTown : (GameAction?)null,
                    KeyBindAction.StartCasting => GameAction.StartCasting,
                    KeyBindAction.Pray => GameAction.Pray,
                    KeyBindAction.OpenInventory => GameAction.OpenInventory,
                    KeyBindAction.OpenStatus => GameAction.OpenStatus,
                    KeyBindAction.OpenMessageLog => GameAction.OpenMessageLog,
                    KeyBindAction.OpenSkillTree => GameAction.OpenSkillTree,
                    KeyBindAction.OpenWorldMap => GameAction.OpenWorldMap,
                    KeyBindAction.OpenEncyclopedia => GameAction.OpenEncyclopedia,
                    KeyBindAction.OpenCompanion => GameAction.OpenCompanion,
                    KeyBindAction.OpenDeathLog => GameAction.OpenDeathLog,
                    KeyBindAction.OpenVocabulary => GameAction.OpenVocabulary,
                    KeyBindAction.Save => GameAction.Save,
                    KeyBindAction.Load => GameAction.Load,
                    _ => null
                };

                // スキルスロット処理
                if (bindAction.Value >= KeyBindAction.SkillSlot1 && bindAction.Value <= KeyBindAction.SkillSlot6)
                {
                    int slotIndex = bindAction.Value - KeyBindAction.SkillSlot1;
                    int slotCost;
                    bool slotUsed = _gameController.TryUseSkillSlot(slotIndex, out slotCost);
                    if (slotUsed)
                    {
                        _gameController.AdvanceTurnFromSkillSlot(slotCost);
                    }
                    UpdateDisplay();
                    e.Handled = true;
                    return;
                }

                // UI専用アクション（GameActionを経由しない直接処理）
                if (bindAction.Value == KeyBindAction.OpenQuestLog)
                {
                    ShowQuestLogDialog();
                    e.Handled = true;
                    return;
                }
                if (bindAction.Value == KeyBindAction.OpenReligion)
                {
                    ShowReligionDialog();
                    e.Handled = true;
                    return;
                }
                if (bindAction.Value == KeyBindAction.ToggleMinimap)
                {
                    _minimapVisible = !_minimapVisible;
                    MinimapBorder.Visibility = _minimapVisible ? Visibility.Visible : Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                if (bindAction.Value == KeyBindAction.CycleCombatStance)
                {
                    _gameController.CycleCombatStance();
                    UpdateDisplay();
                    e.Handled = true;
                    return;
                }
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

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        _heldMovementKeys.Remove(e.Key);
    }

    private void UpdateDisplay()
    {
        // ステータス更新
        if (_gameController.IsInLocationMap || _gameController.IsOnSurface)
        {
            FloorText.Text = "";
        }
        else
        {
            FloorText.Text = $"第{_gameController.CurrentFloor}層";
        }
        DateText.Text = _gameController.GameTime.ToFullString();
        TimePeriodText.Text = _gameController.GameTime.TimePeriod;

        // 領地名・地上/ダンジョン表示
        var territory = TerritoryDefinition.Get(_gameController.CurrentTerritory);
        TerritoryText.Text = territory.Name;
        if (_gameController.IsOnSurface || _gameController.IsInLocationMap)
        {
            SurfaceStatusText.Text = _gameController.IsInLocationMap ? "【町】" : "【地上】";
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
            RougelikeGame.Core.HungerStage.SlightlyHungry => System.Windows.Media.Brushes.Orange,
            RougelikeGame.Core.HungerStage.VeryHungry => System.Windows.Media.Brushes.OrangeRed,
            RougelikeGame.Core.HungerStage.Overeating => System.Windows.Media.Brushes.Gold,
            RougelikeGame.Core.HungerStage.Nausea => System.Windows.Media.Brushes.Gold,
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
        ThirstText.Text = $"{_gameController.PlayerThirst}({_gameController.PlayerThirstName})";
        ThirstText.Foreground = _gameController.PlayerThirstStage switch
        {
            ThirstStage.Full => System.Windows.Media.Brushes.DeepSkyBlue,
            ThirstStage.Normal => System.Windows.Media.Brushes.DeepSkyBlue,
            ThirstStage.SlightlyThirsty => System.Windows.Media.Brushes.Yellow,
            ThirstStage.VeryThirsty => System.Windows.Media.Brushes.Orange,
            ThirstStage.Dehydrated => System.Windows.Media.Brushes.Red,
            ThirstStage.Overdrinking => System.Windows.Media.Brushes.Gold,
            ThirstStage.Nausea => System.Windows.Media.Brushes.Gold,
            _ => System.Windows.Media.Brushes.DarkRed
        };

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

        // === GUI統合: 新システムステータス表示 ===

        // スタンス表示
        StanceText.Text = _gameController.PlayerStanceName;
        StanceText.Foreground = _gameController.PlayerStance switch
        {
            CombatStance.Aggressive => System.Windows.Media.Brushes.OrangeRed,
            CombatStance.Defensive => System.Windows.Media.Brushes.DeepSkyBlue,
            _ => System.Windows.Media.Brushes.LightGreen
        };

        // 疲労表示
        FatigueText.Text = $"{_gameController.PlayerFatigue:F1} / 100.0 ({_gameController.PlayerFatigueName})";
        FatigueText.Foreground = _gameController.PlayerFatigueStage switch
        {
            FatigueStage.Refreshed => System.Windows.Media.Brushes.LimeGreen,
            FatigueStage.Normal => System.Windows.Media.Brushes.White,
            FatigueStage.Lethargy => System.Windows.Media.Brushes.Yellow,
            FatigueStage.LightFatigue => System.Windows.Media.Brushes.Orange,
            FatigueStage.Fatigue => System.Windows.Media.Brushes.Red,
            FatigueStage.HeavyFatigue => System.Windows.Media.Brushes.DarkRed,
            FatigueStage.Exhaustion => System.Windows.Media.Brushes.DarkRed,
            FatigueStage.TotalExhaustion => System.Windows.Media.Brushes.DarkRed,
            _ => System.Windows.Media.Brushes.White
        };

        // 衛生表示
        HygieneText.Text = $"{_gameController.PlayerHygiene}({_gameController.PlayerHygieneName})";
        HygieneText.Foreground = _gameController.PlayerHygieneStage switch
        {
            HygieneStage.Clean => System.Windows.Media.Brushes.LightBlue,
            HygieneStage.Normal => System.Windows.Media.Brushes.LightGreen,
            HygieneStage.Dirty => System.Windows.Media.Brushes.Yellow,
            HygieneStage.Filthy => System.Windows.Media.Brushes.Orange,
            _ => System.Windows.Media.Brushes.Red
        };

        // 病気表示
        DiseaseText.Text = _gameController.PlayerDiseaseName != null
            ? $"🤒{_gameController.PlayerDiseaseName}"
            : "健康";
        DiseaseText.Foreground = _gameController.PlayerDiseaseName != null
            ? System.Windows.Media.Brushes.OrangeRed
            : System.Windows.Media.Brushes.LimeGreen;

        // スキルスロット表示（PoE風アイコン）
        var slots = _gameController.GetSkillSlots();
        SkillSlotIconPanel.Children.Clear();
        for (int i = 0; i < SkillTreeSystem.MaxSkillSlots; i++)
        {
            var skillId = i < slots.Count ? slots[i] : null;
            var displayName = skillId != null
                ? (SkillDatabase.GetById(skillId)?.Name
                   ?? _gameController.GetSkillTreeSystem().AllNodes.GetValueOrDefault(skillId)?.Name
                   ?? ReligionSkillSystem.GetSkillName(skillId)
                   ?? skillId)
                : null;
            var iconChar = displayName != null && displayName.Length > 0
                ? displayName.Substring(0, 1) : "-";

            var iconBorder = new Border
            {
                Width = 32,
                Height = 32,
                Background = skillId != null
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1a, 0x3a, 0x2a))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x14, 0x14, 0x20)),
                BorderBrush = skillId != null
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4e, 0xcc, 0xa3))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0f, 0x34, 0x60)),
                BorderThickness = new Thickness(skillId != null ? 2 : 1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(2, 0, 2, 0),
                ToolTip = displayName != null ? $"[{i + 1}] {displayName}" : $"[{i + 1}] 空"
            };

            var grid = new Grid();
            // スロット番号（左上小さく）
            var numText = new TextBlock
            {
                Text = $"{i + 1}",
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 8,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(2, 0, 0, 0)
            };
            grid.Children.Add(numText);

            // スキル頭文字（中央）
            var charText = new TextBlock
            {
                Text = iconChar,
                Foreground = skillId != null
                    ? System.Windows.Media.Brushes.White
                    : System.Windows.Media.Brushes.DarkGray,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(charText);

            iconBorder.Child = grid;
            SkillSlotIconPanel.Children.Add(iconBorder);
        }

        // マップ描画
        _renderer.Render(
            _gameController.Map,
            _gameController.Player,
            _gameController.Enemies,
            _gameController.GroundItems
        );

        // ミニマップ描画（町・フィールド内ではスキップして軽量化）
        if (_minimapVisible && !_gameController.IsInLocationMap)
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
            result = $"あなたは第{_gameController.CurrentFloor}層で力尽きた...\n\n到達階層: 第{_gameController.CurrentFloor}層\n{_gameController.GameTime.ToFullString()}\n死亡回数: {_gameController.TotalDeaths}回";
        }
        else if (_gameController.IsGameOver && _gameController.Player.IsAlive)
        {
            result = $"時間切れ — 世界の崩壊に巻き込まれた...\n\n到達階層: 第{_gameController.CurrentFloor}層\n{_gameController.GameTime.ToFullString()}\n死亡回数: {_gameController.TotalDeaths}回";
        }
        else
        {
            result = $"冒険終了\n\n到達階層: 第{_gameController.CurrentFloor}層\n{_gameController.GameTime.ToFullString()}";
        }

        // β.21: 専用ゲームオーバー画面を表示（MessageBox→GameOverWindow）
        var gameOverWindow = new GameOverWindow(result);
        gameOverWindow.Owner = this;
        gameOverWindow.ShowDialog();

        if (gameOverWindow.ReturnToTitle)
        {
            ExitReason = GameExitReason.ReturnToTitle;
        }
        else
        {
            ExitReason = GameExitReason.Quit;
        }
        Close();
    }

    private void OnGameClear(GameClearSystem.ClearScore score)
    {
        _audioManager.StopBgm();

        bool unlocksNgPlus = GameClearSystem.UnlocksNewGamePlus(score.Rank);

        var sb = new StringBuilder();
        sb.AppendLine("🏆 ゲームクリア！！ 🏆");
        sb.AppendLine();
        sb.AppendLine($"スコア: {score.TotalScore}");
        sb.AppendLine($"ランク: {score.Rank}");
        sb.AppendLine($"ターンボーナス: +{score.TurnBonus}");
        sb.AppendLine($"死亡ペナルティ: -{score.DeathPenalty}");
        sb.AppendLine($"レベルボーナス: +{score.LevelBonus}");
        sb.AppendLine($"フロアボーナス: +{score.FloorBonus}");
        sb.AppendLine();
        sb.AppendLine($"プレイヤー: {_gameController.Player.Name} ({_gameController.Player.Race}/{_gameController.Player.CharacterClass})");
        sb.AppendLine($"レベル: {_gameController.Player.Level} | 死亡回数: {_gameController.TotalDeaths}");
        sb.AppendLine($"日時: {_gameController.GameTime.ToFullString()}");

        if (unlocksNgPlus)
        {
            sb.AppendLine();
            sb.AppendLine("⚔ NG+（周回プレイ）が解放されました！");

            var ngChoice = MessageBox.Show(
                $"{sb}\n\nNG+（周回プレイ）を開始しますか？\n（「いいえ」を選ぶとタイトル画面に戻ります）",
                "ゲームクリア",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (ngChoice == MessageBoxResult.Yes)
            {
                // 現在のNG+段階を1段階進める
                var currentTier = _gameController.CurrentNgPlusTier;
                NgPlusTier = currentTier.HasValue
                    ? (NewGamePlusTier)Math.Min((int)currentTier.Value + 1, (int)NewGamePlusTier.Plus5)
                    : NewGamePlusTier.Plus1;
                ExitReason = GameExitReason.StartNewGamePlus;
                Close();
                return;
            }
        }
        else
        {
            MessageBox.Show(sb.ToString(), "ゲームクリア", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        ExitReason = GameExitReason.ReturnToTitle;
        Close();
    }

    private void OnLocationArrived(LocationDefinition location)
    {
        // ロケーション到着時のBGM切替等（将来拡張用）
    }

    private void OnSymbolMapEnterTown()
    {
        _audioManager.StopBgm();
        _audioManager.PlayBgm(BgmIds.Town);
    }

    private void OnSymbolMapEnterDungeon(LocationDefinition location)
    {
        _audioManager.StopBgm();
        _audioManager.PlayBgm(BgmIds.DungeonNormal);
    }

    private void ShowInventoryDialog(List<Item> items)
    {
        StopAutoExploreTimer();

        Func<List<Item>> getItems = () =>
        {
            var inv = (RougelikeGame.Core.Entities.Inventory)_gameController.Player.Inventory;
            return inv.Items.ToList();
        };

        Action<Item> onDropItem = (Item item) =>
        {
            var inv = (RougelikeGame.Core.Entities.Inventory)_gameController.Player.Inventory;
            if (inv.Items.Contains(item))
            {
                inv.Remove(item);
                _gameController.GroundItems.Add((item, _gameController.Player.Position));
                _gameController.AddMessage($"{item.GetDisplayName()}を地面に置いた");
            }
        };

        var dialog = new InventoryWindow(items, _gameController.Player,
            onUseItem: (Item item) => _gameController.UseItem(item),
            onDropItem: onDropItem,
            getItems: getItems,
            savedPositions: _inventoryGridPositions,
            onUnequipItem: (slot) => _gameController.UnequipItem(slot),
            isSorted: _inventorySorted);
        dialog.Owner = this;
        dialog.ShowDialog();
        _inventoryGridPositions = dialog.GetSavedPositions();
        _inventorySorted = dialog.IsSorted;

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
            if (SaveManager.Save(saveData))
                AddMessage("💾 ゲームをセーブした");
            else
                AddMessage("⚠ セーブに失敗しました（ディスク容量や権限を確認してください）");
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

    private void ShowVocabularyDialog()
    {
        StopAutoExploreTimer();
        var dialog = new VocabularyWindow(_gameController.Player);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

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
            if (dialog.TravelDestination.HasValue)
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

    private void OnOpenShop(FacilityType shopType)
    {
        StopAutoExploreTimer();

        var shopWindow = new ShopWindow(_gameController, shopType);
        shopWindow.Owner = this;
        shopWindow.ShowDialog();
        Focus();
    }

    private void OnShowCrafting()
    {
        StopAutoExploreTimer();

        var craftingWindow = new CraftingWindow(_gameController);
        craftingWindow.Owner = this;
        craftingWindow.ShowDialog();
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

    private void ShowPauseDialog()
    {
        StopAutoExploreTimer();
        var pauseWindow = new PauseWindow(_keyBindings) { Owner = this };
        if (pauseWindow.ShowDialog() == true)
        {
            // キーバインドが更新された場合
            if (pauseWindow.UpdatedKeyBindings != null)
            {
                _keyBindings = pauseWindow.UpdatedKeyBindings;
                _keyBindings.Save();
            }

            switch (pauseWindow.Result)
            {
                case PauseResult.Save:
                    _gameController.ProcessInput(GameAction.Save);
                    break;
                case PauseResult.Load:
                    _gameController.ProcessInput(GameAction.Load);
                    break;
                case PauseResult.ReturnToTitle:
                    ExitReason = GameExitReason.ReturnToTitle;
                    Close();
                    return;
                case PauseResult.Resume:
                default:
                    break;
            }
        }

        // CD-7: 設定変更後にオーディオボリュームを反映
        var settings = GameSettings.Load();
        _audioManager.ApplyVolumeSettings(settings.MasterVolume, settings.BgmVolume, settings.SeVolume);

        UpdateDisplay();
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
        var dialog = new EncyclopediaWindow(_gameController) { Owner = this };
        dialog.ShowDialog();
        Focus();
    }

    private void ShowDeathLogDialog()
    {
        StopAutoExploreTimer();
        var dialog = new DeathLogWindow(_gameController) { Owner = this };
        dialog.ShowDialog();
        Focus();
    }

    private void ShowSkillTreeDialog()
    {
        StopAutoExploreTimer();
        var dialog = new SkillTreeWindow(_gameController) { Owner = this };
        dialog.ShowDialog();
        Focus();
    }

    private void ShowCompanionDialog()
    {
        StopAutoExploreTimer();
        var dialog = new CompanionWindow(_gameController) { Owner = this };
        dialog.ShowDialog();
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

    private void ShowRecruitCompanionDialog(List<CompanionSystem.CompanionData> candidates)
    {
        StopAutoExploreTimer();
        var dialog = new RecruitCompanionWindow(_gameController, candidates);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    private void ShowQuestBoardDialog()
    {
        StopAutoExploreTimer();
        var dialog = new QuestBoardWindow(_gameController);
        dialog.Owner = this;
        dialog.ShowDialog();
        Focus();
    }

    #endregion

    #region Ver.β 視覚エフェクト（β.9/β.11/β.12/β.18）

    // ============================================================
    // β.13: 場面転換演出 — 階層移動時のフェードアウト→フェードイン
    // β.20: 死に戻り視覚演出 — 死亡暗転→テキスト表示→フェードイン
    // ============================================================
    private void OnFloorChangedEffect(int newFloor)
    {
        Dispatcher.Invoke(() => TriggerFadeTransition(null));
    }

    private void OnPlayerDiedEffect(string causeText)
    {
        Dispatcher.Invoke(() => TriggerFadeTransition($"— {causeText} —"));
    }

    private void OnPlayerRebirthedEffect(int totalDeaths)
    {
        Dispatcher.Invoke(() => TriggerFadeIn());
    }

    private void TriggerFadeTransition(string? deathMessage)
    {
        // フェードアウト（黒画面に暗転）
        FadeOverlay.Opacity = 0;
        var fadeOut = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400));

        if (deathMessage != null)
        {
            // 死亡演出: 暗転後にメッセージを表示
            fadeOut.Completed += (_, _) =>
            {
                var msgBlock = new System.Windows.Controls.TextBlock
                {
                    Text = deathMessage,
                    Foreground = new SolidColorBrush(Colors.DarkRed),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                };
                FadeOverlay.Child = msgBlock;

                // 2秒後にフェードイン
                var holdTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };
                holdTimer.Tick += (_, _) =>
                {
                    holdTimer.Stop();
                    FadeOverlay.Child = null;
                    TriggerFadeIn();
                };
                holdTimer.Start();
            };
        }
        else
        {
            // 通常フロア移動: すぐにフェードイン
            fadeOut.Completed += (_, _) => TriggerFadeIn();
        }

        FadeOverlay.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void TriggerFadeIn()
    {
        var fadeIn = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fadeIn.Completed += (_, _) => FadeOverlay.Child = null;
        FadeOverlay.BeginAnimation(OpacityProperty, fadeIn);
    }

    // ============================================================
    // β.18: 正気度演出 — 正気度ステージに応じた画面オーバーレイ
    // ============================================================
    private void OnPlayerSanityStageChangedEffect(object? sender, SanityStageEventArgs e)
    {
        Dispatcher.Invoke(() => UpdateSanityOverlay(e.NewStage));
    }

    private void UpdateSanityOverlay(SanityStage stage)
    {
        // 正気度ステージに応じたオーバーレイ色と不透明度
        var (color, opacity) = stage switch
        {
            SanityStage.Normal   => (Color.FromArgb(0, 0, 0, 0), 0.0),
            SanityStage.Uneasy   => (Color.FromArgb(30, 100, 0, 200), 0.15),
            SanityStage.Anxious  => (Color.FromArgb(50, 120, 0, 180), 0.25),
            SanityStage.Unstable => (Color.FromArgb(70, 150, 0, 100), 0.35),
            SanityStage.Madness  => (Color.FromArgb(90, 200, 0, 50),  0.45),
            _                    => (Color.FromArgb(120, 220, 0, 0),  0.55),  // Broken
        };

        SanityOverlay.Background = new SolidColorBrush(color);

        var fadeAnim = new DoubleAnimation(SanityOverlay.Opacity, opacity, TimeSpan.FromSeconds(1.0));
        SanityOverlay.BeginAnimation(OpacityProperty, fadeAnim);
    }

    // ============================================================
    // β.11: 被弾フラッシュ — 大ダメージ時に画面が赤くフラッシュ
    // β.9:  フローティングダメージ数値 — 被ダメ時アニメーション
    // ============================================================
    private void OnPlayerDamaged(object? sender, DamageEventArgs e)
    {
        int dmg = e.FinalDamage;
        if (dmg <= 0) return;

        Dispatcher.Invoke(() =>
        {
            // β.11: 大ダメージ（10以上）で赤フラッシュ
            if (dmg >= 10)
                TriggerFlashOverlay();

            ShowFloatingDamage(dmg, e.OriginalDamage.Element);
        });
    }

    private void TriggerFlashOverlay()
    {
        FlashOverlay.BeginAnimation(OpacityProperty, null);
        FlashOverlay.Opacity = 0.45;
        var anim = new DoubleAnimation(0.45, 0.0, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
        };
        FlashOverlay.BeginAnimation(OpacityProperty, anim);
    }

    private void ShowFloatingDamage(int damage, RougelikeGame.Core.Element element)
    {
        if (EffectCanvas.ActualWidth <= 0) return;

        // ダメージ数値の色（属性別）
        var color = element switch
        {
            RougelikeGame.Core.Element.Fire      => Colors.OrangeRed,
            RougelikeGame.Core.Element.Ice       => Colors.DeepSkyBlue,
            RougelikeGame.Core.Element.Lightning => Colors.Yellow,
            RougelikeGame.Core.Element.Dark      => Colors.MediumPurple,
            RougelikeGame.Core.Element.Light     => Colors.Gold,
            RougelikeGame.Core.Element.Poison    => Colors.LimeGreen,
            _                                    => Colors.White,
        };

        // プレイヤーキャラ '@' の画面中央付近にランダムオフセットで表示
        var rng = new Random();
        double cx = EffectCanvas.ActualWidth / 2.0;
        double cy = EffectCanvas.ActualHeight / 2.0;
        double startX = cx + rng.NextDouble() * 60 - 30;
        double startY = cy - 20;

        var tb = new System.Windows.Controls.TextBlock
        {
            Text = $"-{damage}",
            Foreground = new SolidColorBrush(color),
            FontSize = damage >= 20 ? 18 : 14,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false
        };

        Canvas.SetLeft(tb, startX);
        Canvas.SetTop(tb, startY);
        EffectCanvas.Children.Add(tb);

        // 上方向へ移動しながらフェードアウト（600ms）
        var moveAnim = new DoubleAnimation(startY, startY - 45, TimeSpan.FromMilliseconds(600))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        moveAnim.Completed += (_, _) => EffectCanvas.Children.Remove(tb);
        tb.BeginAnimation(Canvas.TopProperty, moveAnim);

        var fadeAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(600));
        tb.BeginAnimation(OpacityProperty, fadeAnim);
    }

    // ============================================================
    // β.12: 状態異常視覚エフェクト — 状態異常テキストをオーバーレイ表示
    // ============================================================
    private static readonly Dictionary<StatusEffectType, (string Text, Color Color)> _statusEffectDisplay = new()
    {
        { StatusEffectType.Poison,      ("☠ 毒",   Colors.LimeGreen) },
        { StatusEffectType.Burn,        ("🔥 燃焼", Colors.OrangeRed) },
        { StatusEffectType.Paralysis,   ("⚡ 麻痺", Colors.Yellow) },
        { StatusEffectType.Sleep,       ("💤 睡眠", Colors.SkyBlue) },
        { StatusEffectType.Silence,     ("🔇 沈黙", Colors.Plum) },
        { StatusEffectType.Confusion,   ("❓ 混乱", Colors.Orange) },
        { StatusEffectType.Curse,       ("💀 呪い", Colors.MediumPurple) },
        { StatusEffectType.Weakness,    ("↓ 弱体化", Colors.Gray) },
        { StatusEffectType.Strength,    ("↑ 強化",  Colors.Gold) },
        { StatusEffectType.Blessing,    ("✨ 祝福",  Colors.Cyan) },
        { StatusEffectType.Haste,       ("⚡ 加速",  Colors.LightCyan) },
        { StatusEffectType.Slow,        ("🐢 減速",  Colors.RosyBrown) },
        { StatusEffectType.Invisibility,("👻 透明化", Colors.AliceBlue) },
        { StatusEffectType.Bleeding,    ("🩸 出血",  Colors.Crimson) },
        { StatusEffectType.Freeze,      ("❄ 凍結",  Colors.DeepSkyBlue) },
        { StatusEffectType.Stun,        ("💫 気絶",  Colors.Khaki) },
        { StatusEffectType.Fear,        ("😱 恐怖",  Colors.DarkOrange) },
        { StatusEffectType.Charm,       ("💕 魅了",  Colors.HotPink) },
    };

    private void OnPlayerStatusEffectApplied(object? sender, StatusEffectEventArgs e)
    {
        if (_statusEffectDisplay.TryGetValue(e.Effect.Type, out var display))
            ShowStatusEffectNotification(display.Text, display.Color);
    }

    /// <summary>状態異常付与時にエフェクトCanvasにアイコンを表示する</summary>
    public void ShowStatusEffectNotification(string effectName, Color color)
    {
        Dispatcher.Invoke(() =>
        {
            if (EffectCanvas.ActualWidth <= 0) return;

            double cx = EffectCanvas.ActualWidth / 2.0 - 30;
            double cy = EffectCanvas.ActualHeight * 0.35;

            var tb = new System.Windows.Controls.TextBlock
            {
                Text = effectName,
                Foreground = new SolidColorBrush(color),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(tb, cx);
            Canvas.SetTop(tb, cy);
            EffectCanvas.Children.Add(tb);

            var fadeIn  = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(200));
            var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(500));
            fadeOut.BeginTime = TimeSpan.FromMilliseconds(900);
            fadeOut.Completed += (_, _) => EffectCanvas.Children.Remove(tb);

            tb.BeginAnimation(OpacityProperty, fadeIn);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(900) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                tb.BeginAnimation(OpacityProperty, fadeOut);
            };
            timer.Start();
        });
    }

    #endregion
}
