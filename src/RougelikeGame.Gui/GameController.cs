using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;
using RougelikeGame.Data.MagicLanguage;
using RougelikeGame.Engine;
using RougelikeGame.Engine.Combat;
using RougelikeGame.Engine.Magic;

namespace RougelikeGame.Gui;

/// <summary>
/// ゲームロジックを管理するコントローラー
/// </summary>
public class GameController
{
    private readonly IRandomProvider _random;
    private readonly CombatSystem _combatSystem;
    private readonly EnemyFactory _enemyFactory;
    private readonly ItemFactory _itemFactory;
    private readonly BackgroundClearSystem _clearSystem = new();
    private readonly SkillSystem _skillSystem = new();
    private readonly SpellCastingSystem _spellCastingSystem = new();
    private readonly ReligionSystem _religionSystem = new();
    private readonly WorldMapSystem _worldMapSystem = new();
    private readonly TownSystem _townSystem = new();
    private readonly ShopSystem _shopSystem = new();
    private readonly NpcSystem _npcSystem = new();
    private readonly DialogueSystem _dialogueSystem = new();
    private readonly QuestSystem _questSystem = new();
    private readonly GuildSystem _guildSystem = new();
    private readonly CraftingSystem _craftingSystem = new();
    private readonly TutorialSystem _tutorialSystem = new();

    // === Ver.prt.0.2-0.4 新システム ===
    private readonly KarmaSystem _karmaSystem = new();
    private readonly ReputationSystem _reputationSystem = new();
    private readonly CompanionSystem _companionSystem = new();
    private readonly EncyclopediaSystem _encyclopediaSystem = new();
    private readonly DeathLogSystem _deathLogSystem = new();
    private readonly OathSystem _oathSystem = new();
    private readonly SkillTreeSystem _skillTreeSystem = new();
    private readonly BaseConstructionSystem _baseConstructionSystem = new();
    private readonly InvestmentSystem _investmentSystem = new();
    private readonly GridInventorySystem _gridInventorySystem = new();
    private readonly SymbolMapSystem _symbolMapSystem = new();

    // === Ver.prt.0.5-0.6 新システム ===
    private readonly NpcMemorySystem _npcMemorySystem = new();
    private readonly RelationshipSystem _relationshipSystem = new();
    private readonly ItemIdentificationSystem _itemIdentificationSystem = new();
    private readonly DungeonEcosystemSystem _dungeonEcosystemSystem = new();
    private readonly PetSystem _petSystem = new();
    private readonly MerchantGuildSystem _merchantGuildSystem = new();
    private readonly InscriptionSystem _inscriptionSystem = new();
    private readonly FactionWarSystem _factionWarSystem = new();
    private readonly TerritoryInfluenceSystem _territoryInfluenceSystem = new();

    // === GUI統合: 全システムGUI動作確認対応 ===
    private readonly ProficiencySystem _proficiencySystem = new();
    private readonly DungeonShortcutSystem _dungeonShortcutSystem = new();
    private readonly SmithingSystem _smithingSystem = new();
    private readonly AchievementSystem _achievementSystem = new();

    /// <summary>プレイヤーの現在の戦闘スタンス</summary>
    private CombatStance _playerStance = CombatStance.Balanced;

    /// <summary>プレイヤーの疲労レベル</summary>
    private FatigueLevel _playerFatigue = FatigueLevel.Fresh;

    /// <summary>プレイヤーの衛生レベル</summary>
    private HygieneLevel _playerHygiene = HygieneLevel.Clean;

    /// <summary>プレイヤーの罹患中の病気（null=健康）</summary>
    private DiseaseType? _playerDisease;

    /// <summary>病気の残りターン数</summary>
    private int _diseaseRemainingTurns;

    /// <summary>敵がアクティブになる描画範囲半径</summary>
    private const int ActiveRange = 10;

    /// <summary>最後のダメージ原因（死亡判定用）</summary>
    private DeathCause _lastDamageCause = DeathCause.Unknown;

    /// <summary>自動探索中かどうか</summary>
    private bool _autoExploring = false;

    /// <summary>ターン制限延長フラグ（素性別フラグ達成で有効化）</summary>
    private bool _turnLimitExtended = false;

    /// <summary>ターン制限撤廃フラグ（素性別フラグ達成で有効化）</summary>
    private bool _turnLimitRemoved = false;

    /// <summary>前回のターン制限警告段階（重複メッセージ防止）</summary>
    private int _lastTurnLimitWarningStage = 0;

    /// <summary>最後のTryMoveで実行された行動のコスト（攻撃/ドア開け等の区別用）</summary>
    private int _lastMoveActionCost = TurnCosts.MoveNormal;

    /// <summary>デバッグモードかどうか</summary>
    private bool _isDebugMode = false;

    /// <summary>デバッグ: 現在の敵定義インデックス（敵種類切替用）</summary>
    private int _debugEnemyIndex = 0;

    /// <summary>デバッグ: 敵AIアクティブ状態（デバッグモードでは初期OFF）</summary>
    private bool _debugAIActive = false;

    /// <summary>ゲーム難易度</summary>
    private DifficultyLevel _difficulty = DifficultyLevel.Normal;

    /// <summary>現在のマップ名（種族・素性に応じた開始マップ名等）</summary>
    private string _currentMapName = "capital_guild";

    /// <summary>ロケーションマップ（非ダンジョン）内にいるか</summary>
    private bool _isInLocationMap;

    /// <summary>フィールドマップからシンボルマップへ帰還する際の復帰位置</summary>
    private Position? _symbolMapReturnPosition;

    /// <summary>引き継ぎデータ（死に戻り用）</summary>
    private TransferData? _transferData;

    /// <summary>NG+段階（非NG+時はnull）</summary>
    private NewGamePlusTier? _ngPlusTier;

    /// <summary>ゲームクリア済みフラグ</summary>
    private bool _hasCleared = false;

    /// <summary>最終クリアランク</summary>
    private string _clearRank = "";

    /// <summary>無限ダンジョンモード</summary>
    private bool _infiniteDungeonMode = false;

    /// <summary>無限ダンジョン撃破数</summary>
    private int _infiniteDungeonKills = 0;

    /// <summary>スキルスロット（1-6キー割当、最大6スロット）</summary>
    private readonly string?[] _skillSlots = new string?[6];

    /// <summary>ダンジョンフロアキャッシュ（ダンジョン名+階層をキーとして各ダンジョンで分離）</summary>
    private readonly Dictionary<(string DungeonName, int Floor), FloorCache> _floorCache = new();

    /// <summary>フロアキャッシュの有効期間（ゲーム内24時間 = 24*60*60ターン）</summary>
    private const int FloorCacheExpiry = 24 * 60 * 60;

    /// <summary>詠唱中の残りターン数（0=詠唱中でない）</summary>
    private int _chantRemainingTurns;

    /// <summary>詠唱中の魔法効果（詠唱完了時に適用）</summary>
    private SpellCastResult? _pendingSpellResult;

    /// <summary>累計死亡回数</summary>
    public int TotalDeaths { get; private set; } = 0;

    public Player Player { get; private set; } = null!;
    public DungeonMap Map { get; private set; } = null!;
    public List<Enemy> Enemies { get; } = new();
    public List<(Item Item, Position Position)> GroundItems { get; } = new();
    public int CurrentFloor { get; private set; } = 1;
    public int TurnCount { get; private set; } = 0;
    public GameTime GameTime { get; } = new();
    public bool IsGameOver { get; private set; } = false;
    public bool IsRunning { get; private set; } = true;
    public bool IsCasting => _spellCastingSystem.IsCasting;
    public bool IsAutoExploring => _autoExploring;

    /// <summary>現在のマップ名</summary>
    public string CurrentMapName => _currentMapName;

    /// <summary>現在の難易度</summary>
    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Normal;

    /// <summary>現在の難易度設定</summary>
    public DifficultySettings DifficultyConfig => DifficultySettings.Get(Difficulty);

    /// <summary>ターン制限が延長されているか</summary>
    public bool IsTurnLimitExtended => _turnLimitExtended;

    /// <summary>ターン制限が撤廃されているか</summary>
    public bool IsTurnLimitRemoved => _turnLimitRemoved;

    /// <summary>現在のターン制限上限（難易度倍率適用済み）</summary>
    public long CurrentTurnLimit =>
        _turnLimitRemoved ? long.MaxValue :
        _turnLimitExtended
            ? (long)(TimeConstants.TurnLimitWithExtension * DifficultyConfig.TurnLimitMultiplier)
            : (long)(TimeConstants.TurnLimitYear * DifficultyConfig.TurnLimitMultiplier);

    /// <summary>残りターン数（撤廃済みの場合はlong.MaxValue）</summary>
    public long RemainingTurns =>
        _turnLimitRemoved ? long.MaxValue :
        Math.Max(0, CurrentTurnLimit - GameTime.TotalTurns);

    /// <summary>残り日数（表示用）</summary>
    public int RemainingDays =>
        _turnLimitRemoved ? int.MaxValue :
        (int)(RemainingTurns / TimeConstants.TurnsPerDay);

    public event Action<string>? OnMessage;
    public event Action? OnStateChanged;
    public event Action? OnGameOver;
    public event Action<List<Item>>? OnShowInventory;
    public event Action? OnShowStatus;
    public event Action<List<string>>? OnShowMessageLog;
    public event Action? OnSaveGame;
    public event Action? OnLoadGame;
    public event Action? OnCastingStarted;
    public event Action? OnCastingEnded;
    public event Action<SpellPreview>? OnSpellPreviewUpdated;
    public event Action? OnShowWorldMap;
    public event Action<TerritoryId>? OnTerritoryChanged;
    public event Action<DialogueNode>? OnShowDialogue;
    public event Action<FacilityType>? OnOpenShop;
    public event Action<string>? OnQuestUpdated;
    public event Action<GuildRank>? OnGuildRankUp;
    public event Action? OnShowCrafting;
    public event Action<CraftingResult>? OnCraftingResult;
    public event Action<EnhancementResult>? OnEnhancementResult;
    public event Action<EnchantmentResult>? OnEnchantmentResult;
    public event Action<TutorialStep>? OnShowTutorial;

    // === 新画面表示イベント ===
    public event Action? OnShowEncyclopedia;
    public event Action? OnShowDeathLog;
    public event Action? OnShowSkillTree;
    public event Action? OnShowCompanion;
    public event Action? OnShowCooking;
    public event Action? OnShowBaseConstruction;
    public event Action<List<CompanionSystem.CompanionData>>? OnShowRecruitCompanion;
    public event Action? OnShowQuestBoard;

    /// <summary>シンボルマップでのロケーション到着通知</summary>
    public event Action<LocationDefinition>? OnLocationArrived;

    /// <summary>シンボルマップでの街入場要求</summary>
    public event Action? OnSymbolMapEnterTown;

    /// <summary>シンボルマップでのダンジョン入場要求</summary>
    public event Action<LocationDefinition>? OnSymbolMapEnterDungeon;

    /// <summary>ゲームクリアイベント</summary>
    public event Action<GameClearSystem.ClearScore>? OnGameClear;

    /// <summary>ゲームクリア済みか</summary>
    public bool HasCleared => _hasCleared;

    /// <summary>NG+段階</summary>
    public NewGamePlusTier? CurrentNgPlusTier => _ngPlusTier;

    /// <summary>無限ダンジョンモードか</summary>
    public bool IsInfiniteDungeonMode => _infiniteDungeonMode;

    /// <summary>メッセージ履歴</summary>
    private readonly List<string> _messageHistory = new();

    /// <summary>メッセージ履歴の読み取り専用アクセス</summary>
    public IReadOnlyList<string> MessageHistory => _messageHistory;

    public GameController()
    {
        _random = new RandomProvider();
        _combatSystem = new CombatSystem(_random);
        _enemyFactory = new EnemyFactory();
        _itemFactory = new ItemFactory();
    }

    public void Initialize()
    {
        Initialize("冒険者", Race.Human, Core.CharacterClass.Fighter, Core.Background.Adventurer, DifficultyLevel.Normal);
    }

    public void Initialize(string playerName, Race race, Core.CharacterClass characterClass, Core.Background background, DifficultyLevel difficulty)
    {
        // 難易度設定
        _difficulty = difficulty;

        // マップ名を種族・素性から決定
        _currentMapName = StartingMapResolver.Resolve(race, background);

        // 開始領地を設定
        var startTerritory = StartingMapResolver.GetStartingTerritory(_currentMapName);
        _worldMapSystem.SetTerritory(startTerritory);

        // プレイヤー作成
        Player = Player.Create(playerName, race, characterClass, background);

        // プレイヤーイベント購読
        SubscribePlayerEvents();

        // 初期装備
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)Player.Inventory).Add(sword);
        ((Inventory)Player.Inventory).Add(armor);
        Player.Equipment.Equip(sword, Player);
        Player.Equipment.Equip(armor, Player);

        // 初期アイテム
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateBread());

        // STRベースの最大重量を更新
        Player.UpdateMaxWeight();

        // メインクエスト登録・受注
        _questSystem.RegisterMainQuest();
        _questSystem.AcceptQuest("main_quest_abyss", Player.Level, GuildRank.None);

        // マップ生成（シンボルマップから開始）
        GenerateSymbolMap();

        var mapDisplayName = StartingMapResolver.GetDisplayName(_currentMapName);
        var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
        AddMessage($"{territoryName}のシンボルマップに入った");
        AddMessage("WASD/矢印で移動、ロケーションに到着して>キーでダンジョン、Tキーで街に入る");
    }

    /// <summary>
    /// デバッグ用初期化：広いテストアリーナに各種インタラクティブ要素を配置
    /// GUIオートテストおよび手動テストの両方を目的とする
    /// </summary>
    public void InitializeDebug()
    {
        _isDebugMode = true;

        // プレイヤー作成
        Player = Player.Create("テスト冒険者", new Stats(14, 12, 12, 10, 10, 10, 10, 10, 10));
        SubscribePlayerEvents();

        // 初期装備
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)Player.Inventory).Add(sword);
        ((Inventory)Player.Inventory).Add(armor);
        Player.Equipment.Equip(sword, Player);
        Player.Equipment.Equip(armor, Player);

        // 初期アイテム
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateBread());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateShortBow());

        Player.UpdateMaxWeight();

        // デバッグ用固定マップ生成
        GenerateDebugFloor();

        AddMessage("【デバッグモード】テストマップに入った！");
        AddMessage("WASD/矢印で移動、敵に体当たりで攻撃");
        AddMessage("特殊マス: [E]敵切替 [A]AI切替 [D]日数+1 [N]NPC対話");
    }

    /// <summary>
    /// デバッグ用テストアリーナを手動構築（32x24）
    /// 大部屋に敵・アイテム・特殊タイル・ドア・罠・階段等を配置
    /// </summary>
    private void GenerateDebugFloor()
    {
        const int mapW = 32;
        const int mapH = 24;

        Map = new DungeonMap(mapW, mapH) { Depth = 1 };

        // --- メインルーム（2,2）〜（29,21）の大部屋 ---
        var mainRoom = new Room { Id = 0, X = 2, Y = 2, Width = 28, Height = 20, Type = RoomType.Normal };
        Map.AddRoom(mainRoom);

        // 床を敷く
        for (int x = mainRoom.X; x < mainRoom.X + mainRoom.Width; x++)
        {
            for (int y = mainRoom.Y; y < mainRoom.Y + mainRoom.Height; y++)
            {
                Map.SetTile(x, y, TileType.Floor);
            }
        }

        // --- 外周は壁（DungeonMapコンストラクタで初期化済みなので追加不要） ---

        // --- プレイヤー配置（部屋中央付近） ---
        var startPos = new Position(16, 12);
        Player.Position = startPos;

        // --- 階段 ---
        Map.SetStairsUp(new Position(3, 3));
        Map.SetStairsDown(new Position(28, 20));

        // --- ドア（通常＋施錠） ---
        // 通常ドア: 部屋内に仕切り壁を作りドアを設置
        Map.SetTile(10, 2, TileType.DoorClosed);  // 上壁にドア

        // 施錠ドア
        Map.SetTile(20, 2, TileType.DoorClosed);
        var lockedDoorTile = Map.GetTile(new Position(20, 2));
        lockedDoorTile.IsLocked = true;
        lockedDoorTile.LockDifficulty = 10;

        // --- 罠（隠れ＋発見済み） ---
        Map.SetTile(6, 10, TileType.TrapHidden);
        Map.GetTile(new Position(6, 10)).TrapId = "spike";
        Map.SetTile(8, 10, TileType.TrapVisible);
        Map.GetTile(new Position(8, 10)).TrapId = "spike";

        // --- 祭壇・泉 ---
        Map.SetTile(3, 20, TileType.Altar);
        Map.SetTile(5, 20, TileType.Fountain);

        // --- 水・柱（地形要素） ---
        Map.SetTile(15, 6, TileType.Water);
        Map.SetTile(16, 6, TileType.Water);
        Map.SetTile(17, 6, TileType.Water);
        Map.SetTile(15, 18, TileType.Pillar);
        Map.SetTile(17, 18, TileType.Pillar);

        // --- 宝箱 ---
        Map.SetTile(28, 3, TileType.Chest);

        // === デバッグ専用タイル ===
        // 敵種類切替マス（赤 E）
        Map.SetTile(5, 5, TileType.DebugEnemySpawn);
        // AI切替マス（青 A）
        Map.SetTile(5, 7, TileType.DebugAIToggle);
        // 日数進行マス（黄 D）
        Map.SetTile(5, 9, TileType.DebugDayAdvance);
        // NPC対話マス（緑 N）
        Map.SetTile(5, 12, TileType.DebugNpc);

        // === 敵配置（複数種類） ===
        Enemies.Clear();
        var allEnemies = new[]
        {
            EnemyDefinitions.Slime,
            EnemyDefinitions.Goblin,
            EnemyDefinitions.Skeleton,
            EnemyDefinitions.Orc,
            EnemyDefinitions.GiantSpider,
        };
        var enemyPositions = new[]
        {
            new Position(26, 3),
            new Position(28, 3),
            new Position(26, 5),
            new Position(28, 5),
            new Position(27, 7),
        };
        for (int i = 0; i < allEnemies.Length; i++)
        {
            var enemy = _enemyFactory.CreateEnemy(allEnemies[i], enemyPositions[i], null);
            Enemies.Add(enemy);
        }

        // === 地面アイテム配置（種類豊富） ===
        GroundItems.Clear();
        // ポーション類
        GroundItems.Add((ItemFactory.CreateHealingPotion(), new Position(10, 14)));
        GroundItems.Add((ItemFactory.CreateMinorManaPotion(), new Position(11, 14)));
        GroundItems.Add((ItemFactory.CreateAntidote(), new Position(12, 14)));
        // 食料
        GroundItems.Add((ItemFactory.CreateBread(), new Position(10, 16)));
        GroundItems.Add((ItemFactory.CreateRation(), new Position(11, 16)));
        GroundItems.Add((ItemFactory.CreateCookedMeat(), new Position(12, 16)));
        // 巻物
        GroundItems.Add((ItemFactory.CreateScrollOfIdentify(), new Position(10, 18)));
        GroundItems.Add((ItemFactory.CreateScrollOfTeleport(), new Position(11, 18)));
        GroundItems.Add((ItemFactory.CreateScrollOfFireball(), new Position(12, 18)));
        // 装備品
        GroundItems.Add((ItemFactory.CreateDagger(), new Position(14, 14)));
        GroundItems.Add((ItemFactory.CreateWoodenShield(), new Position(15, 14)));
        GroundItems.Add((ItemFactory.CreateChainmail(), new Position(16, 14)));
        // ゴールド用アイテム
        GroundItems.Add((ItemFactory.CreateIronRing(), new Position(14, 16)));

        // 視界計算
        Map.ComputeFov(Player.Position, 8);
    }
    private void SubscribePlayerEvents()
    {
        Player.OnLevelUp += (_, e) => AddMessage($"★ レベルアップ！ Lv.{e.NewLevel} になった！");
        Player.OnHungerStageChanged += (_, e) =>
        {
            string msg = e.NewStage switch
            {
                HungerStage.Hungry => "⚠ お腹が空いてきた...",
                HungerStage.Starving => "⚠ 空腹で力が入らない！",
                HungerStage.Famished => "⚠ 餓死寸前！何か食べないと危険！",
                HungerStage.Normal when e.OldStage > HungerStage.Normal => "お腹が満たされた",
                HungerStage.Full => "お腹いっぱいだ",
                _ => ""
            };
            if (!string.IsNullOrEmpty(msg)) AddMessage(msg);
        };
        Player.OnSanityStageChanged += (_, e) =>
        {
            string msg = e.NewStage switch
            {
                SanityStage.Uneasy => "⚠ 不安感が増してきた...",
                SanityStage.Anxious => "⚠ 精神が不安定になってきた...",
                SanityStage.Unstable => "⚠ 正気を保つのが難しい！",
                SanityStage.Madness => "⚠ 狂気に蝕まれている！",
                SanityStage.Broken => "⚠ 精神が崩壊した...",
                _ => ""
            };
            if (!string.IsNullOrEmpty(msg)) AddMessage(msg);
        };
        Player.OnPlayerDeath += (_, e) =>
        {
            if (e.WillBeRescued)
            {
                AddMessage($"意識が遠のく... しかし誰かが助けてくれた（正気度-{e.SanityLoss}）");
            }
        };

        // スキルシステム: 既習得スキルを登録
        foreach (var skillId in Player.LearnedSkills)
        {
            _skillSystem.RegisterSkill(skillId);
        }

        // 新規スキル習得時に自動登録
        Player.OnSkillLearned += (_, e) =>
        {
            _skillSystem.RegisterSkill(e.SkillId);
            AddMessage($"📖 スキル「{SkillDatabase.GetById(e.SkillId)?.Name ?? e.SkillId}」を習得した！");
        };
    }

    /// <summary>
    /// シンボルマップを生成する（領地の地上マップ）
    /// </summary>
    private void GenerateSymbolMap()
    {
        var territory = _worldMapSystem.CurrentTerritory;
        var symbolMap = _symbolMapSystem.GenerateForTerritory(territory);

        Map = symbolMap;
        _worldMapSystem.IsOnSurface = true;

        // プレイヤー配置（入口位置）
        var startPos = symbolMap.EntrancePosition ?? new Position(
            SymbolMapGenerator.MapWidth / 2, SymbolMapGenerator.MapHeight / 2);
        Player.Position = startPos;

        // シンボルマップでは敵・アイテムを配置しない
        Enemies.Clear();
        GroundItems.Clear();

        // 視界計算（シンボルマップは広い視界）
        symbolMap.ComputeFov(Player.Position, 12);
    }

    /// <summary>現在のフロアのアイテム状態をキャッシュに保存</summary>
    private void SaveFloorItemsToCache()
    {
        var floorKey = (_currentMapName, CurrentFloor);
        if (_floorCache.TryGetValue(floorKey, out var cached))
        {
            _floorCache[floorKey] = cached with { GroundItems = new List<(Item, Position)>(GroundItems) };
        }
    }

    private void GenerateFloor()
    {
        // フロアキャッシュ確認（有効期限内ならマップ構造を再利用）
        var floorKey = (_currentMapName, CurrentFloor);
        if (_floorCache.TryGetValue(floorKey, out var cached)
            && (GameTime.TotalTurns - cached.CreatedAtTurn) < FloorCacheExpiry)
        {
            Map = cached.Map;
            Map.Name = _currentMapName;

            // キャッシュから復帰時は上り階段位置に配置
            var cachedStartPos = Map.StairsUpPosition ?? Map.EntrancePosition ?? new Position(5, 5);
            Player.Position = cachedStartPos;

            // 敵は再生成（動的要素）
            Enemies.Clear();
            SpawnEnemies();

            // アイテムはキャッシュから復元（24h毎のリセット時にのみ再生成）
            GroundItems.Clear();
            GroundItems.AddRange(cached.GroundItems);

            Map.ComputeFov(Player.Position, 8);
            return;
        }

        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = CurrentFloor,
            RoomCount = 6 + CurrentFloor,
            TrapDensity = 0.005f * CurrentFloor
        };

        var generator = new DungeonGenerator();
        Map = (DungeonMap)generator.Generate(parameters);
        Map.Name = _currentMapName;

        // プレイヤー配置
        var startPos = Map.StairsUpPosition ?? Map.EntrancePosition ?? new Position(5, 5);
        Player.Position = startPos;

        // 敵を配置
        Enemies.Clear();
        SpawnEnemies();

        // アイテムを配置
        GroundItems.Clear();
        SpawnItems();

        // 新規生成したマップとアイテムをキャッシュ
        _floorCache[floorKey] = new FloorCache(Map, GameTime.TotalTurns, new List<(Item, Position)>(GroundItems));

        // 視界計算
        Map.ComputeFov(Player.Position, 8);
    }

    private void SpawnEnemies()
    {
        var definitions = EnemyDefinitions.GetEnemiesForDepth(CurrentFloor);
        int enemyCount = 4 + CurrentFloor * 2;

        // ボスフロアは敵が少し多い
        if (CurrentFloor % GameConstants.BossFloorInterval == 0)
        {
            enemyCount += 3;
        }

        // 階層補正: 3階ごとに全ステータス+1
        int floorBonus = CurrentFloor / 3;

        // NG+段階による敵強化倍率
        float ngPlusMultiplier = _ngPlusTier.HasValue
            ? NewGamePlusSystem.GetEnemyStatMultiplier(_ngPlusTier.Value)
            : 1.0f;
        int ngPlusBonus = (int)((ngPlusMultiplier - 1.0f) * 10);

        StatModifier? bonus = (floorBonus > 0 || ngPlusBonus > 0)
            ? new StatModifier(
                Strength: floorBonus + ngPlusBonus,
                Vitality: floorBonus + ngPlusBonus,
                Agility: floorBonus / 2 + ngPlusBonus / 2,
                Dexterity: floorBonus / 2 + ngPlusBonus / 2)
            : null;

        // フロアボスを配置（5階ごと）
        if (CurrentFloor % GameConstants.BossFloorInterval == 0)
        {
            var bossDef = EnemyDefinitions.GetFloorBoss(CurrentFloor);
            if (bossDef != null)
            {
                var bossRoom = Map.GetBossRoom();
                var bossPos = bossRoom != null
                    ? new Position(bossRoom.X + bossRoom.Width / 2, bossRoom.Y + bossRoom.Height / 2)
                    : GetRandomFloorPosition();

                if (bossPos.HasValue)
                {
                    var boss = _enemyFactory.CreateEnemy(bossDef, bossPos.Value, bonus);
                    Enemies.Add(boss);
                }
            }
        }

        // ダンジョン最深部（MaxDungeonFloor）にダンジョン固有ボスを必ず配置
        if (CurrentFloor == GameConstants.MaxDungeonFloor
            && !_isInLocationMap
            && !string.IsNullOrEmpty(_currentMapName))
        {
            // 5の倍数でない場合のみ追加（5の倍数なら上のロジックで配置済み）
            if (CurrentFloor % GameConstants.BossFloorInterval != 0 || EnemyDefinitions.GetFloorBoss(CurrentFloor) == null)
            {
                var dungeonBoss = EnemyDefinitions.GetDungeonBoss(_currentMapName);
                var bossRoom = Map.GetBossRoom();
                var bossPos = bossRoom != null
                    ? new Position(bossRoom.X + bossRoom.Width / 2, bossRoom.Y + bossRoom.Height / 2)
                    : GetRandomFloorPosition();

                if (bossPos.HasValue)
                {
                    var boss = _enemyFactory.CreateEnemy(dungeonBoss, bossPos.Value, bonus);
                    Enemies.Add(boss);
                }
            }
        }

        for (int i = 0; i < enemyCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue && pos.Value.DistanceTo(Player.Position) > 6)
            {
                var def = definitions[_random.Next(definitions.Count)];
                var enemy = _enemyFactory.CreateEnemy(def, pos.Value, bonus);
                Enemies.Add(enemy);
            }
        }
    }

    private void SpawnItems()
    {
        int itemCount = 3 + _random.Next(4);
        for (int i = 0; i < itemCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue)
            {
                var item = _itemFactory.GenerateDungeonFloorItem(CurrentFloor);
                GroundItems.Add((item, pos.Value));
            }
        }
    }

    private Position? GetRandomFloorPosition()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = _random.Next(Map.Width);
            int y = _random.Next(Map.Height);
            var pos = new Position(x, y);
            var tile = Map.GetTile(pos);
            if (!tile.BlocksMovement && !IsOccupied(pos))
            {
                return pos;
            }
        }
        return null;
    }

    private bool IsOccupied(Position pos)
    {
        if (Player.Position == pos) return true;
        return Enemies.Any(e => e.Position == pos && e.IsAlive);
    }

    public void ProcessInput(GameAction action)
    {
        if (IsGameOver || !IsRunning) return;

        // 自動探索中に何か操作したら中断
        if (_autoExploring && action != GameAction.AutoExplore)
        {
            _autoExploring = false;
        }

        Position newPos = Player.Position;
        bool turnUsed = false;
        int actionCost = TurnCosts.MoveNormal; // デフォルト: 移動コスト
        bool isDiagonal = false;

        switch (action)
        {
            case GameAction.MoveUp:
                newPos = new Position(Player.Position.X, Player.Position.Y - 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDown:
                newPos = new Position(Player.Position.X, Player.Position.Y + 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveUpLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y - 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveUpRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y - 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDownLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y + 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDownRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y + 1);
                _lastMoveActionCost = TurnCosts.MoveNormal;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.Wait:
                turnUsed = true;
                actionCost = TurnCosts.Wait;
                AddMessage("待機した");
                break;
            case GameAction.Pickup:
                turnUsed = TryPickupItem();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.UseStairs:
                turnUsed = TryDescendStairs();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.AscendStairs:
                turnUsed = TryAscendStairs();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.OpenInventory:
                ShowInventory();
                return;
            case GameAction.OpenStatus:
                OnShowStatus?.Invoke();
                return;
            case GameAction.OpenMessageLog:
                OnShowMessageLog?.Invoke(_messageHistory.ToList());
                return;
            case GameAction.AutoExplore:
                turnUsed = StepAutoExplore();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.Search:
                turnUsed = TrySearch();
                actionCost = TurnCosts.Search;
                break;
            case GameAction.CloseDoor:
                turnUsed = TryCloseDoor();
                actionCost = TurnCosts.OpenDoor;
                break;
            case GameAction.RangedAttack:
                turnUsed = TryRangedAttack();
                actionCost = TurnCosts.AttackBow;
                break;
            case GameAction.ThrowItem:
                turnUsed = TryThrowItem();
                actionCost = TurnCosts.AttackThrow;
                break;
            case GameAction.UseSkill:
                turnUsed = TryUseFirstReadySkill(out actionCost);
                break;
            case GameAction.StartCasting:
                StartSpellCasting();
                return;
            case GameAction.CastSpell:
                turnUsed = TryCastSpell(out actionCost);
                break;
            case GameAction.CancelCasting:
                CancelSpellCasting();
                return;
            case GameAction.Pray:
                turnUsed = TryPray();
                actionCost = TurnCosts.Pray;
                break;
            case GameAction.JoinReligion:
                // 入信は外部UIから宗教IDを指定して呼ぶ
                return;
            case GameAction.LeaveReligion:
                turnUsed = TryLeaveReligion();
                actionCost = TurnCosts.ReligionAction;
                break;
            case GameAction.TravelToTerritory:
                // 移動先は外部UIから TryTravelTo(TerritoryId) を呼ぶ
                return;
            case GameAction.EnterTown:
                turnUsed = TryEnterTown();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.LeaveTown:
                turnUsed = TryLeaveTown();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.UseInn:
                turnUsed = TryUseInn();
                break;
            case GameAction.VisitChurch:
                turnUsed = TryVisitChurch();
                break;
            case GameAction.VisitBank:
                // 銀行は外部UIから DepositGold/WithdrawGold を呼ぶ
                return;
            case GameAction.EnterShop:
                // ショップは外部UIから InitializeShop/Buy/Sell を呼ぶ
                return;
            case GameAction.OpenWorldMap:
                OnShowWorldMap?.Invoke();
                return;
            case GameAction.TalkToNpc:
                // NPCは外部UIから TryTalkToNpc(npcId) を呼ぶ
                return;
            case GameAction.AcceptQuest:
                // クエスト受注は外部UIから TryAcceptQuest(questId) を呼ぶ
                return;
            case GameAction.TurnInQuest:
                // クエスト納品は外部UIから TryTurnInQuest(questId) を呼ぶ
                return;
            case GameAction.RegisterGuild:
                turnUsed = TryRegisterGuild();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.ViewQuestLog:
                // クエストログ表示はUI側で GetActiveQuests() を参照
                return;
            case GameAction.AdvanceDialogue:
                turnUsed = TryAdvanceDialogue();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.OpenCrafting:
                OnShowCrafting?.Invoke();
                return;
            case GameAction.CraftItem:
                // 合成は外部UIから TryCraftItem(recipeId) を呼ぶ
                return;
            case GameAction.EnhanceEquipment:
                // 強化は外部UIから TryEnhanceEquipment(equipment) を呼ぶ
                return;
            case GameAction.EnchantWeapon:
                // 付与は外部UIから TryEnchantWeapon(weapon, element) を呼ぶ
                return;
            case GameAction.OpenEncyclopedia:
                OnShowEncyclopedia?.Invoke();
                return;
            case GameAction.OpenDeathLog:
                OnShowDeathLog?.Invoke();
                return;
            case GameAction.OpenSkillTree:
                OnShowSkillTree?.Invoke();
                return;
            case GameAction.OpenCompanion:
                OnShowCompanion?.Invoke();
                return;
            case GameAction.OpenCooking:
                OnShowCooking?.Invoke();
                return;
            case GameAction.OpenBaseConstruction:
                OnShowBaseConstruction?.Invoke();
                return;
            case GameAction.Save:
                OnSaveGame?.Invoke();
                return;
            case GameAction.Load:
                OnLoadGame?.Invoke();
                return;
            case GameAction.Quit:
                IsRunning = false;
                OnGameOver?.Invoke();
                return;
        }

        if (turnUsed)
        {
            // 斜め移動補正（攻撃・ドア以外の純粋な移動時のみ）
            if (isDiagonal && actionCost == TurnCosts.MoveNormal)
            {
                actionCost = TurnCosts.MoveNormal * TurnCosts.DiagonalNumerator / TurnCosts.DiagonalDenominator;
            }

            // 重量超過ペナルティ: 移動コストが1.5倍
            if (Player.IsOverweight && actionCost <= TurnCosts.AttackNormal)
            {
                actionCost = (int)Math.Ceiling(actionCost * 1.5);
            }

            // 行動コスト分のターンを消費（最低1）
            int finalCost = Math.Max(1, actionCost);
            TurnCount += finalCost;
            GameTime.AdvanceTurn(finalCost);
            ProcessEnemyTurns();
            ProcessTurnEffects();
            CheckTurnLimitWarnings();
            Map.ComputeFov(Player.Position, 8);

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(_lastDamageCause);
            }
            else if (CheckTurnLimitExceeded())
            {
                HandleTurnLimitExceeded();
            }
        }

        OnStateChanged?.Invoke();
    }

    private bool TryMove(Position newPos)
    {
        if (newPos.X < 0 || newPos.X >= Map.Width || newPos.Y < 0 || newPos.Y >= Map.Height)
        {
            // ロケーションマップ（町内・フィールド）で外周部から外へ移動しようとした場合は出る
            if (_isInLocationMap)
            {
                return TryLeaveTown();
            }

            // シンボルマップの外周から外へ移動しようとした場合はワールドマップを表示
            if (_worldMapSystem.IsOnSurface)
            {
                AddMessage("領地の境界に到達した。ワールドマップを開く…");
                OnShowWorldMap?.Invoke();
                return false;
            }

            return false;
        }

        var tile = Map.GetTile(newPos);

        // 敵がいる場合は攻撃
        var enemy = Enemies.FirstOrDefault(e => e.Position == newPos && e.IsAlive);
        if (enemy != null)
        {
            Attack(enemy);
            _lastMoveActionCost = TurnCosts.AttackNormal;
            return true;
        }

        // ドアを開ける
        if (tile.Type == TileType.DoorClosed)
        {
            if (tile.IsLocked)
            {
                // 施錠されている場合、DEX判定でピッキング
                int dex = Player.EffectiveStats.Dexterity;
                int difficulty = tile.LockDifficulty;
                int roll = _random.Next(20) + 1 + dex;

                if (roll >= difficulty)
                {
                    tile.IsLocked = false;
                    tile.LockDifficulty = 0;
                    Map.SetTile(newPos.X, newPos.Y, TileType.DoorOpen);
                    AddMessage("🔓 鍵をこじ開けてドアを開けた！");
                    _lastMoveActionCost = TurnCosts.Unlock;
                }
                else
                {
                    AddMessage($"🔒 ドアは施錠されている（解錠に失敗した）");
                    _lastMoveActionCost = TurnCosts.OpenDoor;
                }
                return true;
            }

            Map.SetTile(newPos.X, newPos.Y, TileType.DoorOpen);
            AddMessage("ドアを開けた");
            _lastMoveActionCost = TurnCosts.OpenDoor;
            return true;
        }

        // NPCタイルへの移動: 隣接してNPC方向へ移動キーを押した場合はインタラクション
        if (IsNpcTile(tile.Type))
        {
            HandleNpcTile(tile);
            return true;
        }

        // 移動可能チェック
        if (tile.BlocksMovement)
        {
            return false;
        }

        // 罠チェック（隠れている罠を踏んだ場合）
        if (tile.Type == TileType.TrapHidden)
        {
            var trapType = ParseTrapType(tile.TrapId);
            var trapDef = TrapDefinition.Get(trapType);

            // 浮遊特性: 落とし穴無効
            if (trapDef.Type == TrapType.PitFall && RacialTraitSystem.IsLevitating(Player.Race))
            {
                Map.SetTile(newPos.X, newPos.Y, TileType.TrapVisible);
                AddMessage($"浮遊しているため{trapDef.Name}を回避した！");
            }
            // PER判定による事前発見（野生の勘ボーナス加算）
            else
            {
                int perceptionBonus = (int)(RacialTraitSystem.GetTraitValue(Player.Race, RacialTraitType.WildIntuition) * 10);
                if (trapDef.CanDetect(Player.EffectiveStats.Perception + perceptionBonus, _random))
                {
                    Map.SetTile(newPos.X, newPos.Y, TileType.TrapVisible);
                    AddMessage($"⚠ {trapDef.Name}を発見した！注意して回避した。");
                    // 発見時は移動はするが罠は発動しない
                }
                else
                {
                    // 罠発動
                    Map.SetTile(newPos.X, newPos.Y, TileType.TrapVisible);
                    TriggerTrap(trapDef, newPos);
                }
            }
        }
        // 発見済みの罠の上を移動する場合は発動しない

        // 移動実行
        Player.Position = newPos;

        // シンボルマップ上のロケーション到着処理
        if (_worldMapSystem.IsOnSurface && _symbolMapSystem.IsLocationSymbol(newPos))
        {
            var location = _symbolMapSystem.GetLocationAt(newPos);
            if (location != null)
            {
                var msg = _symbolMapSystem.GetLocationArrivalMessage(newPos);
                if (!string.IsNullOrEmpty(msg))
                {
                    AddMessage(msg);
                }
                OnLocationArrived?.Invoke(location);
            }
        }
        // シンボルマップ上の地形タイル到着処理（ロケーション未配置）
        else if (_worldMapSystem.IsOnSurface && SymbolMapSystem.IsEnterableTerrainTile(tile.Type))
        {
            var terrainName = SymbolMapSystem.GetTerrainName(tile.Type);
            AddMessage($"【{terrainName}】に到着した。（Tキーでフィールドに入る）");
        }

        // 階段メッセージ
        if (tile.Type == TileType.StairsDown)
        {
            AddMessage("下り階段がある [Shift+>]キーで降りる");
        }
        else if (tile.Type == TileType.StairsUp)
        {
            AddMessage("上り階段がある [Shift+<]キーで上がる");
        }

        // デバッグ専用タイルの処理
        if (_isDebugMode)
        {
            HandleDebugTile(tile, newPos);
        }

        return true;
    }

    private void Attack(Enemy enemy)
    {
        var result = _combatSystem.ExecuteAttack(Player, enemy, AttackType.Slash);

        // === GUI統合: 戦闘システム修飾 ===
        // スタンス修飾（CombatStanceSystem）
        float stanceAttackMod = CombatStanceSystem.GetAttackModifier(_playerStance);

        // 武器熟練度ボーナス（WeaponProficiencySystem）
        int weaponDamageBonus = 0;
        if (Player.Equipment.MainHand != null)
        {
            var weaponType = Player.Equipment.MainHand.WeaponType;
            weaponDamageBonus = WeaponProficiencySystem.GetScalingBonus(weaponType, Player.EffectiveStats);

            // 熟練度経験値獲得（ProficiencySystem）
            var profCategory = ProficiencySystem.GetWeaponProficiencyCategory(weaponType);
            _proficiencySystem.GainExperience(profCategory, 1);
        }

        // モンスター種族特性（MonsterRaceSystem）
        var raceTraits = MonsterRaceSystem.GetTraits(enemy.Race);

        // 属性相性（ElementalAffinitySystem）
        float elementalMult = 1.0f;
        if (Player.Equipment.MainHand?.Element != null && Player.Equipment.MainHand.Element != Element.None)
        {
            elementalMult = ElementalAffinitySystem.GetDamageMultiplier(
                ElementalAffinitySystem.GetResistanceLevel(enemy.Race, Player.Equipment.MainHand.Element));
        }

        // 処刑判定（ExecutionSystem）
        bool canExecute = !enemy.IsAlive ? false : ExecutionSystem.CanExecute(enemy.CurrentHp, enemy.MaxHp);

        // 武器耐久度消耗（DurabilitySystem）
        if (Player.Equipment.MainHand != null)
        {
            int wear = DurabilitySystem.CalculateWeaponWear(result.IsCritical);
            Player.Equipment.MainHand.Durability = Math.Max(0, Player.Equipment.MainHand.Durability - wear);
        }

        if (result.IsHit)
        {
            // 追加ダメージ計算
            int baseDmg = result.Damage?.Amount ?? 0;
            int bonusDmg = (int)(baseDmg * (stanceAttackMod - 1.0f)) + weaponDamageBonus;
            int elementalBonusDmg = (int)(baseDmg * (elementalMult - 1.0f));
            int totalBonus = bonusDmg + elementalBonusDmg;

            if (totalBonus > 0)
            {
                enemy.TakeDamage(Damage.Physical(totalBonus));
            }

            var critStr = result.IsCritical ? " クリティカル！" : "";
            var bonusStr = totalBonus > 0 ? $"(+{totalBonus})" : "";
            AddMessage($"{enemy.Name}に{baseDmg + totalBonus}ダメージ！{critStr}{bonusStr}");

            // 処刑チャンス
            if (canExecute && enemy.IsAlive)
            {
                enemy.TakeDamage(Damage.Pure(enemy.CurrentHp));
                int karmaChange = ExecutionSystem.GetExecutionKarmaPenalty(enemy.Race);
                _karmaSystem.ModifyKarma(karmaChange, "処刑");
                AddMessage($"⚔ 処刑！ {enemy.Name}にとどめを刺した！ (カルマ{karmaChange:+#;-#;0})");
            }

            if (!enemy.IsAlive)
            {
                // ゴールドドロップ（人型の敵のみ）
                int gold = 0;
                if (enemy.Race == MonsterRace.Humanoid)
                {
                    int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                    gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));

                    // 処刑ボーナス
                    float executionDropBonus = canExecute ? ExecutionSystem.GetExecutionDropBonus() : 0;
                    gold = (int)(gold * (1.0f + executionDropBonus));

                    Player.AddGold(gold);
                }

                // 経験値（処刑ボーナス込み）
                float executionExpBonus = canExecute ? ExecutionSystem.GetExecutionExpBonus() : 0;
                int totalExp = (int)(enemy.ExperienceReward * (1.0f + executionExpBonus));

                string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                AddMessage($"{enemy.Name}を倒した！経験値+{totalExp}{goldStr}");
                Player.GainExperience(totalExp);
                TryDropItem(enemy);
                OnEnemyDefeated(enemy);
            }
        }
        else
        {
            AddMessage($"{enemy.Name}への攻撃は外れた");
        }
    }

    /// <summary>
    /// 敵撃破時のアイテムドロップ判定
    /// </summary>
    private void TryDropItem(Enemy enemy)
    {
        // ドロップ率: 基本25%、階層が深いほどやや上昇
        int dropChance = 25 + CurrentFloor * 2;
        if (_random.Next(100) < dropChance)
        {
            var item = _itemFactory.GenerateEnemyDropItem(CurrentFloor, enemy.Race);
            GroundItems.Add((item, enemy.Position));
            AddMessage($"{enemy.Name}が{item.GetDisplayName()}を落とした！");
        }
    }

    /// <summary>
    /// 敵撃破時のクリア条件フラグ更新と宗教処理
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        // ボス撃破カウント（高経験値＝ボス級とみなす）
        if (enemy.ExperienceReward >= 80)
        {
            _clearSystem.IncrementFlag("boss_kills");

            // ボス撃破時: その場に脱出口を生成
            if (!_worldMapSystem.IsOnSurface && !_isInLocationMap)
            {
                Map.SetTile(enemy.Position, TileType.StairsUp);
                AddMessage("⚡ ボスを倒した！ 脱出口が出現した！ [Shift+<]キーで脱出できる");
            }
        }

        // クエスト自動進行（敵撃破）
        var questMessages = _questSystem.UpdateKillObjective(enemy.EnemyTypeId);
        foreach (var msg in questMessages)
        {
            AddMessage(msg);
            OnQuestUpdated?.Invoke(msg);
        }

        // 30階ボス撃破→ゲームクリア判定
        if (GameClearSystem.IsFinalBossDefeated(CurrentFloor, enemy.EnemyTypeId))
        {
            HandleGameClear();
        }

        // 無限ダンジョンモード時の撃破カウント
        if (_infiniteDungeonMode)
        {
            _infiniteDungeonKills++;
        }

        // 宗教の経験値ボーナス
        double expBonus = _religionSystem.GetBenefitValue(Player, ReligionBenefitType.ExpBonus);
        if (expBonus > 0)
        {
            int bonusExp = (int)(enemy.ExperienceReward * expBonus);
            if (bonusExp > 0)
            {
                Player.GainExperience(bonusExp);
            }
        }

        // NG+時の経験値ボーナス
        if (_ngPlusTier.HasValue)
        {
            float expMultiplier = NewGamePlusSystem.GetExpMultiplier(_ngPlusTier.Value);
            if (expMultiplier > 1.0f)
            {
                int ngBonusExp = (int)(enemy.ExperienceReward * (expMultiplier - 1.0f));
                if (ngBonusExp > 0)
                {
                    Player.GainExperience(ngBonusExp);
                    AddMessage($"⚔ NG+ボーナス経験値: +{ngBonusExp}");
                }
            }
        }

        // 図鑑更新（モンスター）
        RegisterAndDiscoverEncyclopedia(EncyclopediaCategory.Monster, enemy.EnemyTypeId, enemy.Name);

        // === GUI統合: 敵撃破時の追加システム処理 ===

        // 素材収集（HarvestSystem）
        if (HarvestSystem.CanHarvest(enemy.Race))
        {
            var harvestResult = HarvestSystem.Harvest(enemy.Race, EnemyRank.Common, _random);
            if (harvestResult.Materials.Count > 0)
            {
                AddMessage($"🔪 {harvestResult.Message}");
                foreach (var (itemId, qty) in harvestResult.Materials)
                {
                    AddMessage($"  素材獲得: {itemId} x{qty}");
                }
            }
        }

        // 秘密の通路発見チェック（SecretRoomSystem）
        float discoveryChance = SecretRoomSystem.CalculateDiscoveryChance(
            Player.EffectiveStats.Perception, false);
        if (_random.NextDouble() < discoveryChance * 0.1f)
        {
            AddMessage("🔍 戦闘の衝撃で隠し通路が露わになった！");
        }

        // ダンジョン生態系更新（DungeonEcosystemSystem - 既存フィールド活用）
        _dungeonEcosystemSystem.AddBattleTrace(
            enemy.Position.X, enemy.Position.Y, CurrentFloor, enemy.ExperienceReward,
            $"プレイヤーが{enemy.Name}を撃破", TurnCount);

        // 実績チェック
        _achievementSystem.Unlock($"kill_{enemy.EnemyTypeId}");
        if (_infiniteDungeonKills >= 100) _achievementSystem.Unlock("infinite_100_kills");
    }

    /// <summary>ゲームクリア処理</summary>
    private void HandleGameClear()
    {
        _hasCleared = true;
        _clearSystem.SetFlag("game_clear");

        var score = GameClearSystem.CalculateScore(
            TurnCount, TotalDeaths, Player.Level, CurrentFloor);
        _clearRank = score.Rank;

        var clearText = GameClearSystem.GetClearText(Player.Background);
        var clearMsg = GameClearSystem.GetClearMessage(
            Player.Background.ToString(), score.Rank);

        AddMessage("🏆 ═══════════════════════════════════════");
        AddMessage("🏆 ゲームクリア！！");
        AddMessage(clearText);
        AddMessage(clearMsg);
        AddMessage($"📊 スコア: {score.TotalScore} | ターン: {TurnCount} | 死亡: {TotalDeaths}回");
        AddMessage("🏆 ═══════════════════════════════════════");

        if (GameClearSystem.UnlocksNewGamePlus(score.Rank))
        {
            AddMessage("⚔ NG+（周回プレイ）が解放された！");
        }

        // 無限ダンジョン解放
        AddMessage(InfiniteDungeonSystem.GetUnlockMessage());

        OnGameClear?.Invoke(score);
    }

    private void ProcessEnemyTurns()
    {
        // デバッグモードでAI非活性化中は敵のターンをスキップ
        if (_isDebugMode && !_debugAIActive) return;

        // 仲間のターン処理
        if (_companionSystem.Party.Count > 0)
        {
            var nearestEnemy = Enemies.Where(e => e.IsAlive)
                .OrderBy(e => e.Position.ChebyshevDistanceTo(Player.Position))
                .FirstOrDefault();
            bool hasNearby = nearestEnemy != null &&
                             nearestEnemy.Position.ChebyshevDistanceTo(Player.Position) <= ActiveRange;
            int distance = nearestEnemy != null
                ? nearestEnemy.Position.ChebyshevDistanceTo(Player.Position)
                : 99;

            var companionResults = _companionSystem.ProcessCompanionTurns(
                hasNearby, nearestEnemy?.Name, distance);
            foreach (var result in companionResults)
            {
                if (result.DamageDealt > 0 && nearestEnemy != null)
                {
                    nearestEnemy.TakeDamage(Damage.Physical(result.DamageDealt));
                    AddMessage($"[仲間] {result.CompanionName}: {result.ActionDescription} ({result.DamageDealt}ダメージ)");

                    if (!nearestEnemy.IsAlive)
                    {
                        AddMessage($"{nearestEnemy.Name}を倒した！（仲間の功績）");
                        Player.GainExperience(nearestEnemy.ExperienceReward / 2);
                        OnEnemyDefeated(nearestEnemy);
                    }
                }
            }
        }

        // プレイヤーからActiveRange以内の敵のみ処理する
        foreach (var enemy in Enemies.Where(e => e.IsAlive))
        {
            int distance = enemy.Position.ChebyshevDistanceTo(Player.Position);

            // 描画範囲外の敵は処理しない
            if (distance > ActiveRange) continue;

            if (distance <= enemy.SightRange)
            {
                if (distance == 1)
                {
                    EnemyAttack(enemy);
                }
                else
                {
                    MoveEnemyTowardsPlayer(enemy);
                }
            }
            else
            {
                RandomMoveEnemy(enemy);
            }
        }
    }

    private void EnemyAttack(Enemy enemy)
    {
        // === GUI統合: 時間帯による敵の活性度（TimeOfDaySystem）===
        var currentTime = TimeOfDaySystem.GetTimePeriod(GameTime.Hour);
        float activityMult = TimeOfDaySystem.GetActivityMultiplier(
            TimeOfDaySystem.GetActivityPattern(enemy.Race), currentTime);

        var result = _combatSystem.ExecuteAttack(enemy, Player, AttackType.Slash);

        // プレイヤーのスタンス防御修飾（CombatStanceSystem）
        float stanceDefMod = CombatStanceSystem.GetDefenseModifier(_playerStance);

        if (result.IsHit)
        {
            // 防御修飾によるダメージ軽減
            int baseDmg = result.Damage?.Amount ?? 0;
            int modifiedDmg = Math.Max(1, (int)(baseDmg * activityMult / stanceDefMod));

            // 防具耐久度消耗（DurabilitySystem）
            var bodyArmor = Player.Equipment[EquipmentSlot.Body];
            if (bodyArmor != null)
            {
                int armorWear = DurabilitySystem.CalculateArmorWear(modifiedDmg, Element.None);
                bodyArmor.Durability = Math.Max(0, bodyArmor.Durability - armorWear);
            }

            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"{enemy.Name}の攻撃！{modifiedDmg}ダメージ！{critStr}");
            _lastDamageCause = DeathCause.Combat;

            // === 敵種族に基づく状態異常付与 ===
            TryApplyEnemyStatusEffect(enemy);
        }
        else
        {
            // スタンスによる回避ボーナス表示
            float evasionMod = CombatStanceSystem.GetEvasionModifier(_playerStance);
            if (evasionMod > 1.0f)
            {
                AddMessage($"{enemy.Name}の攻撃を華麗に回避した");
            }
            else
            {
                AddMessage($"{enemy.Name}の攻撃は外れた");
            }
        }
    }

    /// <summary>
    /// 敵種族に基づく状態異常付与（攻撃命中時に確率で発動）
    /// </summary>
    private void TryApplyEnemyStatusEffect(Enemy enemy)
    {
        // 基礎確率15%（種族特性による状態異常付与）
        if (_random.NextDouble() >= 0.15) return;

        // 種族ごとの状態異常（一部種族はランダムで複数パターン）
        StatusEffectType? effectType = enemy.Race switch
        {
            MonsterRace.Insect => StatusEffectType.Poison,          // 昆虫: 毒攻撃
            MonsterRace.Undead => StatusEffectType.Weakness,         // 不死: 衰弱
            MonsterRace.Demon => _random.NextDouble() < 0.9
                ? StatusEffectType.Curse                             // 悪魔: 呪い（90%）
                : StatusEffectType.InstantDeath,                     // 悪魔: 即死（10%）
            MonsterRace.Dragon => _random.NextDouble() < 0.5
                ? StatusEffectType.Burn                              // 竜: 火傷（50%）
                : StatusEffectType.Freeze,                           // 竜: 凍結（50%）
            MonsterRace.Plant => StatusEffectType.Paralysis,         // 植物: 麻痺（毒胞子）
            MonsterRace.Spirit => _random.NextDouble() < 0.7
                ? StatusEffectType.Blind                             // 精霊: 盲目（70%）
                : StatusEffectType.Madness,                          // 精霊: 狂気（30%）
            MonsterRace.Amorphous => StatusEffectType.Slow,          // 不定形: 減速（粘液）
            MonsterRace.Beast => StatusEffectType.Bleeding,          // 獣: 出血（爪傷）
            MonsterRace.Construct => _random.NextDouble() < 0.7
                ? StatusEffectType.Vulnerability                     // 構造体: 脆弱化（70%）
                : StatusEffectType.Petrification,                    // 構造体: 石化（30%）
            _ => null
        };

        if (effectType == null) return;

        // 毒無効種族チェック
        if (effectType == StatusEffectType.Poison && RacialTraitSystem.IsPoisonImmune(Player.Race))
        {
            AddMessage("毒無効の体質により毒を受け付けなかった！");
            return;
        }

        // 即死は耐性判定を追加（LUK依存で回避可能）
        if (effectType == StatusEffectType.InstantDeath)
        {
            if (_random.NextDouble() < Player.EffectiveStats.Luck * 0.05)
            {
                AddMessage("⚡ 即死攻撃を運良く回避した！");
                return;
            }
        }

        int duration = effectType.Value switch
        {
            StatusEffectType.Poison => 10,
            StatusEffectType.Bleeding => 8,
            StatusEffectType.Burn => 5,
            StatusEffectType.Freeze => 3,
            StatusEffectType.Curse => int.MaxValue,
            StatusEffectType.Weakness => 15,
            StatusEffectType.Paralysis => 5,
            StatusEffectType.Blind => 8,
            StatusEffectType.Slow => 10,
            StatusEffectType.Vulnerability => 10,
            StatusEffectType.Madness => 10,
            StatusEffectType.Petrification => 3,
            StatusEffectType.InstantDeath => 1,
            _ => 5
        };

        Player.ApplyStatusEffect(new StatusEffect(effectType.Value, duration));
        AddMessage($"⚠ {enemy.Name}の攻撃で{effectType.Value}状態になった！");
    }

    private void MoveEnemyTowardsPlayer(Enemy enemy)
    {
        int dx = Math.Sign(Player.Position.X - enemy.Position.X);
        int dy = Math.Sign(Player.Position.Y - enemy.Position.Y);

        var candidates = new List<Position>();
        if (dx != 0) candidates.Add(new Position(enemy.Position.X + dx, enemy.Position.Y));
        if (dy != 0) candidates.Add(new Position(enemy.Position.X, enemy.Position.Y + dy));

        foreach (var newPos in candidates)
        {
            if (CanEnemyMoveTo(newPos))
            {
                enemy.Position = newPos;
                return;
            }
        }
    }

    private void RandomMoveEnemy(Enemy enemy)
    {
        if (_random.Next(100) < 30)
        {
            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            var (dx, dy) = directions[_random.Next(4)];
            var newPos = new Position(enemy.Position.X + dx, enemy.Position.Y + dy);

            if (CanEnemyMoveTo(newPos))
            {
                enemy.Position = newPos;
            }
        }
    }

    private bool CanEnemyMoveTo(Position pos)
    {
        if (pos.X < 0 || pos.X >= Map.Width || pos.Y < 0 || pos.Y >= Map.Height)
            return false;

        var tile = Map.GetTile(pos);
        if (tile.BlocksMovement) return false;
        if (Player.Position == pos) return false;
        if (Enemies.Any(e => e.Position == pos && e.IsAlive)) return false;

        return true;
    }

    /// <summary>
    /// デバッグ専用タイルの効果を処理
    /// </summary>
    private void HandleDebugTile(Tile tile, Position pos)
    {
        switch (tile.Type)
        {
            case TileType.DebugEnemySpawn:
                HandleDebugEnemySpawn();
                break;
            case TileType.DebugAIToggle:
                _debugAIActive = !_debugAIActive;
                AddMessage($"🔧 敵AI: {(_debugAIActive ? "活性化" : "非活性化")}");
                break;
            case TileType.DebugDayAdvance:
                GameTime.AdvanceTurn(TimeConstants.TurnsPerDay);
                TurnCount += TimeConstants.TurnsPerDay;
                AddMessage($"🔧 1日経過（{GameTime.Day}日目 {GameTime.Hour:D2}:{GameTime.Minute:D2}）");
                break;
            case TileType.DebugNpc:
                AddMessage("🔧 NPC「冒険者よ、テストを頑張れ！」");
                AddMessage($"   現在 Lv.{Player.Level} HP:{Player.CurrentHp}/{Player.MaxHp} 満腹度:{Player.Hunger}");
                AddMessage($"   所持金:{Player.Gold}G 重量:{((Inventory)Player.Inventory).TotalWeight:F1}/{Player.CalculateMaxWeight():F1}kg");
                break;
        }
    }

    /// <summary>
    /// デバッグ: 全敵を次の種類に切り替えて再配置
    /// </summary>
    private void HandleDebugEnemySpawn()
    {
        var allDefs = new[]
        {
            EnemyDefinitions.Slime,
            EnemyDefinitions.Goblin,
            EnemyDefinitions.Skeleton,
            EnemyDefinitions.Orc,
            EnemyDefinitions.GiantSpider,
            EnemyDefinitions.DarkElf,
            EnemyDefinitions.Troll,
            EnemyDefinitions.Draugr,
        };

        _debugEnemyIndex = (_debugEnemyIndex + 1) % allDefs.Length;
        var def = allDefs[_debugEnemyIndex];

        // 既存の敵をクリアし、新しい種類で再配置
        Enemies.Clear();
        var spawnPositions = new[]
        {
            new Position(26, 3),
            new Position(28, 3),
            new Position(26, 5),
            new Position(28, 5),
            new Position(27, 7),
        };
        foreach (var spawnPos in spawnPositions)
        {
            var enemy = _enemyFactory.CreateEnemy(def, spawnPos, null);
            Enemies.Add(enemy);
        }

        AddMessage($"🔧 敵を切替: {def.Name} ×{spawnPositions.Length}体 配置");
    }

    private void ProcessTurnEffects()
    {
        // 詠唱中の処理
        ProcessChanting();

        // 満腹度減少（HungerDecayInterval ターンごとに1減少）
        // アンデッド「食事不要」特性: 満腹度が減少しない
        if (TurnCount > 0 && TurnCount % TimeConstants.HungerDecayInterval == 0)
        {
            if (!RacialTraitSystem.IsNoFoodRequired(Player.Race))
            {
                Player.ModifyHunger(-1);
            }
        }

        // 飢餓ダメージ（満腹度0の場合、毎ターンダメージ）
        // 食事不要種族は飢餓ダメージも無し
        if (Player.Hunger <= 0 && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int starvationDamage = Math.Max(1, Player.MaxHp / 50);
            Player.TakeDamage(Damage.Pure(starvationDamage));
            if (TurnCount % 60 == 0) // メッセージは60ターンに1回だけ
            {
                AddMessage($"空腹で体力が奪われている！（{starvationDamage}ダメージ）");
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(DeathCause.Starvation);
                return;
            }
        }

        // HP自然回復（満腹度がHungry以上、かつ戦闘中でない場合）
        if (Player.HungerStage <= HungerStage.Normal && !IsInCombat())
        {
            if (TurnCount % 120 == 0) // 120ターン（2分）ごとにHP回復
            {
                Player.Heal(1);
            }
        }

        // SP自然回復（毎ターン少量回復）
        if (TurnCount % 30 == 0) // 30ターン（30秒）ごと
        {
            Player.RestoreSp(1);
        }

        // 状態異常のティック処理
        // 毒無効種族は毒状態異常を自動除去
        if (RacialTraitSystem.IsPoisonImmune(Player.Race))
        {
            if (Player.HasStatusEffect(StatusEffectType.Poison))
            {
                Player.RemoveStatusEffect(StatusEffectType.Poison);
                AddMessage("毒無効の体質により毒が浄化された");
            }
        }

        Player.TickStatusEffects();

        // スキルクールダウン進行
        _skillSystem.TickCooldowns();

        // === GUI統合: ターン毎システム処理 ===

        // 時間帯による視界範囲変動（TimeOfDaySystem）
        var currentTimePeriod = TimeOfDaySystem.GetTimePeriod(GameTime.Hour);
        float sightModifier = TimeOfDaySystem.GetSightRangeModifier(currentTimePeriod);
        if (sightModifier < 1.0f && TurnCount % 600 == 0)
        {
            AddMessage($"🌙 {TimeOfDaySystem.GetTimePeriodName(currentTimePeriod)} — 視界が狭くなっている");
        }

        // 疲労蓄積（BodyConditionSystem: 300ターンごとに疲労上昇）
        if (TurnCount > 0 && TurnCount % 300 == 0)
        {
            if (_playerFatigue < FatigueLevel.Exhausted)
            {
                _playerFatigue++;
                float fatigueMod = BodyConditionSystem.GetFatigueModifier(_playerFatigue);
                if (fatigueMod < 0.9f)
                {
                    AddMessage($"😓 疲労: {BodyConditionSystem.GetFatigueName(_playerFatigue)} — 行動効率{fatigueMod:P0}");
                }
            }
        }

        // 衛生低下（BodyConditionSystem: 1200ターンごとに衛生低下）
        if (TurnCount > 0 && TurnCount % 1200 == 0 && _playerHygiene < HygieneLevel.Filthy)
        {
            _playerHygiene++;
            float infectionRisk = BodyConditionSystem.GetHygieneInfectionRisk(_playerHygiene);
            if (infectionRisk > 0.05f)
            {
                AddMessage($"🧼 衛生: {BodyConditionSystem.GetHygieneName(_playerHygiene)} — 感染リスク上昇");
            }
        }

        // 病気進行（DiseaseSystem）
        if (_playerDisease.HasValue)
        {
            _diseaseRemainingTurns--;
            if (_diseaseRemainingTurns <= 0)
            {
                AddMessage($"💊 {DiseaseSystem.GetDisease(_playerDisease.Value)?.Name ?? "病気"}が治った！");
                _playerDisease = null;
            }
            else if (DiseaseSystem.CheckNaturalRecovery(_playerDisease.Value, _diseaseRemainingTurns,
                Player.EffectiveStats.Vitality))
            {
                AddMessage($"💪 免疫力により{DiseaseSystem.GetDisease(_playerDisease.Value)?.Name ?? "病気"}が回復した！");
                _playerDisease = null;
                _diseaseRemainingTurns = 0;
            }
            else if (TurnCount % 120 == 0)
            {
                var disease = DiseaseSystem.GetDisease(_playerDisease.Value);
                if (disease != null)
                {
                    Player.TakeDamage(Damage.Pure(1));
                    _lastDamageCause = DeathCause.Unknown;
                }
            }
        }

        // 衛生レベルによる感染判定（戦闘後に傷がある想定でチェック）
        if (!_playerDisease.HasValue && _playerHygiene >= HygieneLevel.Dirty && TurnCount % 600 == 0)
        {
            float infectionRisk = BodyConditionSystem.GetHygieneInfectionRisk(_playerHygiene);
            if (_random.NextDouble() < infectionRisk)
            {
                var diseases = DiseaseSystem.GetAllDiseases();
                if (diseases.Count > 0)
                {
                    var diseaseList = diseases.Values.ToList();
                    var disease = diseaseList[_random.Next(diseaseList.Count)];
                    _playerDisease = disease.Type;
                    _diseaseRemainingTurns = disease.DefaultDuration;
                    AddMessage($"🤒 {disease.Name}に感染した！ ({disease.Description})");
                }
            }
        }

        // 装備耐久度チェック（DurabilitySystem: 戦闘中に警告）
        if (TurnCount % 200 == 0 && Player.Equipment.MainHand != null)
        {
            var weapon = Player.Equipment.MainHand;
            var stage = DurabilitySystem.GetStage(weapon.Durability, weapon.MaxDurability);
            if (stage >= DurabilityStage.Worn)
            {
                float perf = DurabilitySystem.GetPerformanceMultiplier(stage);
                AddMessage($"⚠ 武器「{weapon.Name}」の耐久度低下 — 性能{perf:P0}");
            }
        }

        // NPC行動スケジュール更新（NpcRoutineSystem）
        if (TurnCount % 600 == 0 && _isInLocationMap)
        {
            var availableNpcs = NpcRoutineSystem.GetNpcsAtLocation(_currentMapName, currentTimePeriod);
            if (availableNpcs.Count > 0 && TurnCount % 1200 == 0)
            {
                AddMessage($"📋 現在の時間帯: {TimeOfDaySystem.GetTimePeriodName(currentTimePeriod)}");
            }
        }

        // 未使用熟練度の減衰（ProficiencySystem: 600ターンごと）
        if (TurnCount > 0 && TurnCount % 600 == 0)
        {
            _proficiencySystem.DecayUnusedProficiencies(new HashSet<ProficiencyCategory>());
        }

        // 天候変化（WeatherSystemは既存だが、季節に応じた変動を追加）
        if (TurnCount % 300 == 0)
        {
            var newWeather = WeatherSystem.DetermineWeather(CurrentSeason, _random.NextDouble());
            if (newWeather != CurrentWeather)
            {
                CurrentWeather = newWeather;
                AddMessage($"🌤 天候が変化: {WeatherSystem.GetWeatherName(CurrentWeather)}");
            }
        }

        // 渇き進行（ThirstSystem: 180ターンごとに渇き上昇）
        if (TurnCount > 0 && TurnCount % 180 == 0)
        {
            if (PlayerThirstLevel < ThirstLevel.Dehydrated)
            {
                PlayerThirstLevel++;
                if (PlayerThirstLevel >= ThirstLevel.Thirsty)
                {
                    AddMessage($"💧 渇き: {ThirstSystem.GetThirstName(PlayerThirstLevel)}");
                }
            }
            if (PlayerThirstLevel == ThirstLevel.Dehydrated && TurnCount % 60 == 0)
            {
                int thirstDamage = Math.Max(1, Player.MaxHp / 80);
                Player.TakeDamage(Damage.Pure(thirstDamage));
                _lastDamageCause = DeathCause.Unknown;
            }
        }

        // 実績チェック（AchievementSystem: 主要マイルストーン）
        if (TurnCount == 1000) _achievementSystem.Unlock("turn_1000");
        if (Player.Level >= 10) _achievementSystem.Unlock("level_10");
        if (CurrentFloor >= 10) _achievementSystem.Unlock("floor_10");

        if (!Player.IsAlive)
        {
            // 状態異常死の原因を推定
            var cause = Player.HasStatusEffect(StatusEffectType.Poison) ? DeathCause.Poison : DeathCause.Unknown;
            HandlePlayerDeath(cause);
        }
    }

    /// <summary>
    /// 近くに敵がいるか（戦闘中判定）
    /// </summary>
    private bool IsInCombat()
    {
        return Enemies.Any(e => e.IsAlive && e.Position.ChebyshevDistanceTo(Player.Position) <= 2);
    }

    private bool TryPickupItem()
    {
        var itemOnGround = GroundItems.FirstOrDefault(i => i.Position == Player.Position);
        if (itemOnGround.Item != null)
        {
            // 重量チェック
            var inventory = (Inventory)Player.Inventory;
            float itemWeight = itemOnGround.Item.Weight;
            if (itemOnGround.Item is IStackable stackable)
                itemWeight *= stackable.StackCount;

            if (inventory.TotalWeight + itemWeight > Player.CalculateMaxWeight())
            {
                AddMessage($"重すぎて{itemOnGround.Item.GetDisplayName()}を持てない！（{inventory.TotalWeight:F1}/{Player.CalculateMaxWeight():F1}kg）");
                return false;
            }

            // スロットチェック
            if (inventory.UsedSlots >= inventory.MaxSlots)
            {
                AddMessage("持ち物がいっぱいで拾えない！");
                return false;
            }

            // グリッド容量チェック
            if (!CanFitInGrid(inventory, itemOnGround.Item))
            {
                AddMessage($"グリッドに空きがなく{itemOnGround.Item.GetDisplayName()}を拾えない！");
                return false;
            }

            GroundItems.Remove(itemOnGround);
            inventory.Add(itemOnGround.Item);
            AddMessage($"{itemOnGround.Item.GetDisplayName()}を拾った（{inventory.TotalWeight:F1}/{Player.CalculateMaxWeight():F1}kg）");

            // 図鑑更新（アイテム）
            RegisterAndDiscoverEncyclopedia(EncyclopediaCategory.Item, itemOnGround.Item.ItemId, itemOnGround.Item.Name);

            return true;
        }
        else
        {
            AddMessage("ここには何もない");
            return false;
        }
    }

    /// <summary>グリッドインベントリに新しいアイテムが収まるかシミュレート</summary>
    private static bool CanFitInGrid(Inventory inventory, Item newItem)
    {
        const int GridWidth = 10;
        const int GridHeight = 6;
        var placed = new bool[GridWidth, GridHeight];

        // 既存アイテムを順番に配置
        foreach (var item in inventory.Items)
        {
            var (w, h) = GridInventorySystem.GetDimensions(GetItemGridSize(item));
            var pos = FindFreeGridPosition(placed, w, h, GridWidth, GridHeight);
            if (pos == null) continue;
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    placed[pos.Value.X + dx, pos.Value.Y + dy] = true;
        }

        // 新アイテムが配置可能か判定
        var (nw, nh) = GridInventorySystem.GetDimensions(GetItemGridSize(newItem));
        return FindFreeGridPosition(placed, nw, nh, GridWidth, GridHeight) != null;
    }

    /// <summary>グリッドの空き位置を検索</summary>
    private static (int X, int Y)? FindFreeGridPosition(bool[,] placed, int w, int h, int gridWidth, int gridHeight)
    {
        for (int y = 0; y <= gridHeight - h; y++)
            for (int x = 0; x <= gridWidth - w; x++)
            {
                bool fits = true;
                for (int dx = 0; dx < w && fits; dx++)
                    for (int dy = 0; dy < h && fits; dy++)
                        if (placed[x + dx, y + dy])
                            fits = false;
                if (fits) return (x, y);
            }
        return null;
    }

    /// <summary>アイテムのグリッドサイズを決定</summary>
    internal static GridItemSize GetItemGridSize(Item item)
    {
        return item switch
        {
            Weapon { IsTwoHanded: true } => GridItemSize.Size2x3,
            Weapon => GridItemSize.Size1x2,
            Shield => GridItemSize.Size2x2,
            Armor => GridItemSize.Size2x3,
            EquipmentItem => GridItemSize.Size1x2,
            _ => GridItemSize.Size1x1
        };
    }

    private bool TryDescendStairs()
    {
        var tile = Map.GetTile(Player.Position);

        // シンボルマップ上のダンジョン入口から入場
        if (_worldMapSystem.IsOnSurface && tile.Type == TileType.SymbolDungeon)
        {
            var location = _symbolMapSystem.GetLocationAt(Player.Position);
            if (location != null)
            {
                _worldMapSystem.IsOnSurface = false;
                _currentMapName = location.Id;
                CurrentFloor = 1;
                GenerateFloor();
                AddMessage($"【{location.Name}】─ ダンジョン第{CurrentFloor}層に足を踏み入れた...");
                OnSymbolMapEnterDungeon?.Invoke(location);
                OnStateChanged?.Invoke();
                return true;
            }
        }

        // シンボルマップ上の町・施設シンボルから入場
        if (_worldMapSystem.IsOnSurface && _symbolMapSystem.IsTownEntrance(Player.Position))
        {
            return TryEnterTown();
        }

        if (tile.Type == TileType.StairsDown)
        {
            if (CurrentFloor >= GameConstants.MaxDungeonFloor)
            {
                // 最深部到達 → ダンジョンクリアフラグ
                _clearSystem.SetFlag("dungeon_clear");
                AddMessage("🏆 ダンジョン最深部に到達した！");
                return false;
            }

            SaveFloorItemsToCache();
            CurrentFloor++;
            GenerateFloor();
            AddMessage($"第{CurrentFloor}層に降りた");

            // クエスト自動進行（フロア到達）
            var floorMessages = _questSystem.UpdateExploreObjective(CurrentFloor);
            foreach (var msg in floorMessages)
            {
                AddMessage(msg);
            }

            // ボスフロア通知
            if (CurrentFloor % GameConstants.BossFloorInterval == 0)
            {
                var bossDef = EnemyDefinitions.GetFloorBoss(CurrentFloor);
                var bossName = bossDef?.Name ?? "ボス";
                AddMessage($"⚠ 強大な気配を感じる...{bossName}がいるフロアだ！");

                if (CurrentFloor == GameConstants.MaxDungeonFloor)
                {
                    AddMessage("⚠ ここがダンジョン最深部！ 最終ボスが待ち受けている！");
                }
            }

            return true;
        }
        else
        {
            AddMessage("ここに階段はない");
            return false;
        }
    }

    private bool TryAscendStairs()
    {
        var tile = Map.GetTile(Player.Position);
        if (tile.Type != TileType.StairsUp)
        {
            AddMessage("ここに上り階段はない");
            return false;
        }

        // ロケーションマップ（町・施設）の場合は町脱出処理に委譲
        if (_isInLocationMap)
        {
            return TryLeaveTown();
        }

        if (CurrentFloor <= 1)
        {
            // 1層目の上り階段 → シンボルマップに帰還
            AddMessage("ダンジョンから脱出した！ 地上に帰還する...");
            _worldMapSystem.IsOnSurface = true;
            GenerateSymbolMap();

            // ダンジョン入口位置にプレイヤーを配置
            var dungeonPos = _symbolMapSystem.FindLocationPosition(_currentMapName);
            if (dungeonPos.HasValue)
            {
                Player.Position = dungeonPos.Value;
                Map.ComputeFov(Player.Position, 12);
            }

            var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
            AddMessage($"{territoryName}のシンボルマップに戻った");
            OnStateChanged?.Invoke();
            return true;
        }
        else
        {
            // 上の階へ移動
            SaveFloorItemsToCache();
            CurrentFloor--;
            GenerateFloor();
            // 上昇時はプレイヤーを下り階段位置に配置
            var downStairsPos = Map.StairsDownPosition;
            if (downStairsPos.HasValue)
            {
                Player.Position = downStairsPos.Value;
                Map.ComputeFov(Player.Position, 8);
            }
            AddMessage($"第{CurrentFloor}層に上がった");
            return true;
        }
    }

    private void ShowInventory()
    {
        var inventory = (Inventory)Player.Inventory;
        var items = inventory.Items.ToList();
        OnShowInventory?.Invoke(items);
    }

    public void UseItem(int index)
    {
        var inventory = (Inventory)Player.Inventory;
        var items = inventory.Items.ToList();

        if (index >= 0 && index < items.Count)
        {
            var item = items[index];

            if (item is ConsumableItem consumable)
            {
                var result = inventory.UseItem(consumable, Player);
                if (result != null)
                {
                    AddMessage(result.Message);

                    // 識別の巻物の実処理
                    if (result.Effect?.Type == ItemEffectType.Identify)
                    {
                        HandleIdentifyEffect(inventory);
                    }

                    // 消耗品の種類に応じた行動コスト
                    int itemCost = consumable is Food ? TurnCosts.Eat : TurnCosts.UsePotion;
                    TurnCount += itemCost;
                    GameTime.AdvanceTurn(itemCost);
                    ProcessEnemyTurns();
                    OnStateChanged?.Invoke();
                }
            }
            else if (item is EquipmentItem equipItem)
            {
                // スライム等の装備制限チェック
                if (RacialTraitSystem.HasEquipmentRestriction(Player.Race) && equipItem is not (Weapon { WeaponType: WeaponType.Fist }))
                {
                    AddMessage("この種族では装備できない");
                    return;
                }

                // 未鑑定の装備を装着すると自動的に鑑定される
                if (!equipItem.IsIdentified)
                {
                    equipItem.IsIdentified = true;
                    AddMessage($"{equipItem.GetDisplayName()}の正体が分かった！");
                }

                // 職業装備適性チェック
                bool isProficient = ClassEquipmentSystem.IsProficient(Player.CharacterClass, equipItem.Category);

                Player.Equipment.Equip(equipItem, Player);

                if (isProficient)
                {
                    AddMessage($"{equipItem.GetDisplayName()}を装備した");
                }
                else
                {
                    AddMessage($"{equipItem.GetDisplayName()}を装備した（非習熟：攻撃力低下）");
                }

                // 呪われた装備の警告
                if (equipItem.IsCursed)
                {
                    AddMessage("⚠ 呪いの力を感じる...外せない！");
                }

                TurnCount += TurnCosts.EquipChange;
                GameTime.AdvanceTurn(TurnCosts.EquipChange);
                OnStateChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// 識別効果を処理（インベントリ内最初の未鑑定アイテムを鑑定）
    /// </summary>
    private void HandleIdentifyEffect(Inventory inventory)
    {
        var unidentified = inventory.Items.FirstOrDefault(i => !i.IsIdentified);
        if (unidentified != null)
        {
            unidentified.IsIdentified = true;
            AddMessage($"✨ {unidentified.GetDisplayName()}を識別した！");

            if (unidentified.IsCursed)
            {
                AddMessage("⚠ このアイテムは呪われている！");
            }
            else if (unidentified.IsBlessed)
            {
                AddMessage("✦ このアイテムは祝福されている！");
            }
        }
        else
        {
            AddMessage("識別するアイテムがない");
        }
    }

    #region 合成・強化・付与システム

    /// <summary>合成システムを取得</summary>
    public CraftingSystem GetCraftingSystem() => _craftingSystem;

    /// <summary>利用可能なレシピを取得</summary>
    public IReadOnlyList<CraftingRecipe> GetAvailableRecipes()
    {
        return _craftingSystem.GetAvailableRecipes(Player.Level);
    }

    /// <summary>アイテムを合成する</summary>
    public bool TryCraftItem(string recipeId)
    {
        var craftingInventory = new CraftingInventory();
        // プレイヤーのインベントリから素材情報を構築
        foreach (var item in ((Inventory)Player.Inventory).Items)
        {
            craftingInventory.AddItem(item.ItemId);
        }

        var result = _craftingSystem.Craft(recipeId, Player, craftingInventory);
        AddMessage(result.Message);
        OnCraftingResult?.Invoke(result);

        if (result.Success && result.ResultItem != null)
        {
            ((Inventory)Player.Inventory).Add(result.ResultItem);
            AddMessage($"{result.ResultItem.Name}をインベントリに追加した");
        }

        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>装備を強化する</summary>
    public bool TryEnhanceEquipment(EquipmentItem equipment, int cost = 100)
    {
        var result = _craftingSystem.EnhanceEquipment(equipment, Player, _random, cost);
        AddMessage(result.Message);
        OnEnhancementResult?.Invoke(result);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>武器に属性を付与する</summary>
    public bool TryEnchantWeapon(Weapon weapon, Element element, int cost = 200)
    {
        var result = _craftingSystem.EnchantWeapon(weapon, element, Player, _random, cost);
        AddMessage(result.Message);
        OnEnchantmentResult?.Invoke(result);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    #endregion

    #region Tutorial

    /// <summary>チュートリアルシステムを取得</summary>
    public TutorialSystem GetTutorialSystem() => _tutorialSystem;

    /// <summary>チュートリアルトリガーを発火</summary>
    public void TriggerTutorial(TutorialTrigger trigger)
    {
        var step = _tutorialSystem.OnTrigger(trigger);
        if (step != null)
        {
            AddMessage($"【ヒント】{step.Title}: {step.Message}");
            OnShowTutorial?.Invoke(step);
        }
    }

    /// <summary>チュートリアルの有効/無効切替</summary>
    public void SetTutorialEnabled(bool enabled)
    {
        _tutorialSystem.IsEnabled = enabled;
    }

    #endregion

    private void HandlePlayerDeath(DeathCause cause)
    {
        _autoExploring = false;
        TotalDeaths++;

        // 引き継ぎデータを生成（死亡前の知識を保存）
        var transfer = Player.CreateTransferData();
        transfer.TotalDeaths = TotalDeaths;
        transfer.Sanity = Player.Sanity;

        bool wasRescuable = Player.CanBeRescued;
        Player.HandleDeath(cause);

        // 正気度を更新（HandleDeath後）
        transfer.Sanity = Player.Sanity;
        transfer.RescueCountRemaining = Player.RescueCountRemaining;

        string causeText = cause switch
        {
            DeathCause.Combat => "戦闘で力尽きた",
            DeathCause.Boss => "ボスとの戦闘で散った",
            DeathCause.Starvation => "飢えにより力尽きた",
            DeathCause.Trap => "罠によって命を落とした",
            DeathCause.Poison => "毒に蝕まれ力尽きた",
            DeathCause.TimeLimit => "時間切れで力尽きた",
            DeathCause.Curse => "呪いにより命を落とした",
            _ => "力尽きた"
        };

        if (wasRescuable && Player.Sanity > 0)
        {
            // 死に戻り実行：知識を引き継ぎつつ肉体をリセット
            AddMessage($"あなたは{causeText}...");
            AddMessage($"「また会いましたね。正気度: {Player.Sanity}」");
            ExecuteRebirth(transfer);
        }
        else if (Player.Sanity <= 0 && Player.RescueCountRemaining > 0)
        {
            // 廃人化からの救済：正気度を20まで回復
            AddMessage($"あなたは{causeText}...");
            AddMessage("精神が崩壊した...しかし誰かが引き戻してくれた。");
            Player.ModifySanity(GameConstants.SanityRecoveryOnRescue);
            transfer.Sanity = Player.Sanity;

            // 正気度0での死に戻りは知識を失う
            transfer.LearnedWords.Clear();
            transfer.LearnedSkills.Clear();
            AddMessage($"☢ 知識が失われた...（残り救済回数: {Player.RescueCountRemaining}）");
            ExecuteRebirth(transfer);
        }
        else
        {
            // 真のゲームオーバー
            IsGameOver = true;
            AddMessage($"あなたは{causeText}...");
            if (Player.Sanity <= 0)
            {
                AddMessage("正気度が尽き、救済の余地もない。あなたの旅はここで終わりを告げる...");
            }
            else
            {
                AddMessage("もう戻ることはできない...");
            }
            OnGameOver?.Invoke();
        }
    }

    /// <summary>
    /// 死に戻り（リバース）を実行する。
    /// 死に戻りはキャラクター作成直後への時間巻き戻しであるため、
    /// プレイヤーの肉体・世界の状態を全てリセットし、内面知識のみ引き継ぐ。
    /// Player.Create()を使って同じ名前・種族・職業・素性のPlayerオブジェクトを
    /// プログラム的に再生成する（キャラクター作成画面への遷移は発生しない）。
    /// </summary>
    private void ExecuteRebirth(TransferData transfer)
    {
        _transferData = transfer;
        var race = Player.Race;
        var charClass = Player.CharacterClass;
        var background = Player.Background;
        bool isSanityZero = transfer.Sanity <= 0 || (transfer.LearnedWords.Count == 0 && transfer.LearnedSkills.Count == 0 && TotalDeaths > 0);

        // プレイヤーを再作成（肉体リセット）— 同一キャラの時間巻き戻し。画面遷移なし。
        Player = Player.Create(Player.Name, race, charClass, background);

        // 引き継ぎデータを適用（知識系）
        Player.ApplyTransferData(transfer);

        // イベント再購読
        SubscribePlayerEvents();

        // 初期装備を支給（Initialize時と同一の内容）
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)Player.Inventory).Add(sword);
        ((Inventory)Player.Inventory).Add(armor);
        Player.Equipment.Equip(sword, Player);
        Player.Equipment.Equip(armor, Player);
        // 初期アイテム支給（HP薬2個 + パン）
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateBread());

        // STRベースの最大重量を更新
        Player.UpdateMaxWeight();

        // === 世界状態の全リセット（キャラクター作成直後への時間巻き戻し） ===

        // NPC関連リセット（好感度・出会い・会話フラグ・クエスト・ギルド）
        _npcSystem.Reset();
        _dialogueSystem.Reset();
        _questSystem.Reset();
        _guildSystem.Reset();

        // メインクエストを再登録・受注
        _questSystem.RegisterMainQuest();
        _questSystem.AcceptQuest("main_quest_abyss", Player.Level, GuildRank.None);

        // 社会的状態リセット（カルマ・評判・誓約・投資）
        _karmaSystem.Reset();
        _reputationSystem.Reset();
        _oathSystem.Reset();
        _investmentSystem.Reset();

        // 仲間・拠点リセット
        _companionSystem.Reset();
        _baseConstructionSystem.Reset();

        // グリッドインベントリリセット
        _gridInventorySystem.Reset();

        // NPC記憶・噂リセット（NpcSystemとは別管理）
        _npcMemorySystem.Reset();

        // 関係値・勢力リセット
        _relationshipSystem.Reset();
        _territoryInfluenceSystem.Reset();

        // 鑑定・刻印・ペット・商人ギルド・派閥戦・生態系リセット
        _itemIdentificationSystem.Reset();
        _inscriptionSystem.Reset();
        _petSystem.Reset();
        _merchantGuildSystem.Reset();
        _factionWarSystem.Reset();
        _dungeonEcosystemSystem.Reset();

        // 正気度0の場合、知識系システムも消失
        if (isSanityZero)
        {
            _encyclopediaSystem.ResetDiscoveryLevels();
            _skillTreeSystem.Reset();
        }

        // マップ・領地リセット（開始地点に帰還）
        _currentMapName = StartingMapResolver.Resolve(race, background);
        var startTerritory = StartingMapResolver.GetStartingTerritory(_currentMapName);
        _worldMapSystem.Reset(startTerritory);
        _symbolMapSystem.Clear();

        // ゲーム進行フラグリセット
        CurrentFloor = 1;
        TurnCount = 0;
        GameTime.SetTotalTurns(0);
        _turnLimitExtended = false;
        _turnLimitRemoved = false;
        _lastTurnLimitWarningStage = 0;
        _hasCleared = false;
        _infiniteDungeonMode = false;
        _infiniteDungeonKills = 0;
        _isInLocationMap = false;
        _ngPlusTier = null;
        _clearRank = "";

        // 天候・渇きリセット（キャラクター作成直後の状態に戻す）
        CurrentWeather = Weather.Clear;
        PlayerThirstLevel = ThirstLevel.Hydrated;

        // 敵・アイテム・マップリセット
        Enemies.Clear();
        GroundItems.Clear();

        // シンボルマップから再開（Initialize時と同じ初期スポーン場所）
        GenerateSymbolMap();

        var startLocationName = StartingMapResolver.GetDisplayName(_currentMapName);
        AddMessage($"\n━━ 死に戻り ({TotalDeaths}回目) ━━");
        AddMessage($"時は巻き戻り、{startLocationName}に戻った。正気度: {Player.Sanity}");
        if (isSanityZero)
        {
            AddMessage("知識も技も全て失われた...白紙の状態で再び歩み出す。");
        }
        else
        {
            AddMessage("肉体と世界は初期化されたが、心に刻まれた知識は残っている...");
        }

        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// ターン制限延長フラグを有効化する
    /// </summary>
    public void ExtendTurnLimit()
    {
        if (!_turnLimitExtended && !_turnLimitRemoved)
        {
            _turnLimitExtended = true;
            AddMessage("★ 時間制限が延長された！ 猶予が半年分追加された。");
        }
    }

    /// <summary>
    /// 難易度を設定する（ゲーム開始時に呼ぶ）
    /// </summary>
    public void SetDifficulty(DifficultyLevel level)
    {
        Difficulty = level;
        var config = DifficultyConfig;
        AddMessage($"難易度: {config.DisplayName} が設定された");
    }

    /// <summary>
    /// ターン制限撤廃フラグを有効化する
    /// </summary>
    public void RemoveTurnLimit()
    {
        if (!_turnLimitRemoved)
        {
            _turnLimitRemoved = true;
            AddMessage("★★ 時間制限が撤廃された！ もう時間に追われることはない。");
        }
    }

    /// <summary>
    /// ターン制限を超過しているか判定
    /// </summary>
    private bool CheckTurnLimitExceeded()
    {
        if (_turnLimitRemoved) return false;
        return GameTime.TotalTurns >= CurrentTurnLimit;
    }

    /// <summary>
    /// ターン制限超過時の処理（ゲームオーバー）
    /// </summary>
    private void HandleTurnLimitExceeded()
    {
        _autoExploring = false;
        IsGameOver = true;
        AddMessage("⚠ 時間切れ ─ 猶予された時間は尽きた...");
        AddMessage("世界を救うことはできなかった。");
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// ターン制限の警告メッセージをチェック・表示
    /// </summary>
    private void CheckTurnLimitWarnings()
    {
        if (_turnLimitRemoved) return;

        long totalTurns = GameTime.TotalTurns;
        long limit = CurrentTurnLimit;
        long remaining = limit - totalTurns;

        if (remaining <= 0) return;

        int daysRemaining = (int)(remaining / TimeConstants.TurnsPerDay);

        // 段階的警告（重複表示防止）
        if (daysRemaining <= 7 && _lastTurnLimitWarningStage < 4)
        {
            _lastTurnLimitWarningStage = 4;
            AddMessage($"⚠⚠ 残り{daysRemaining}日！ 時間がほとんど残されていない！");
        }
        else if (daysRemaining <= 30 && _lastTurnLimitWarningStage < 3)
        {
            _lastTurnLimitWarningStage = 3;
            AddMessage($"⚠ 残り{daysRemaining}日... 急がなければ！");
        }
        else if (daysRemaining <= 90 && _lastTurnLimitWarningStage < 2)
        {
            _lastTurnLimitWarningStage = 2;
            AddMessage($"残り{daysRemaining}日。時間は有限だ...");
        }
        else if (daysRemaining <= 180 && _lastTurnLimitWarningStage < 1)
        {
            _lastTurnLimitWarningStage = 1;
            AddMessage($"残り約{daysRemaining}日。半年を切った。");
        }
    }

    #region トラップシステム

    /// <summary>
    /// 罠を発動させる
    /// </summary>
    private void TriggerTrap(TrapDefinition trapDef, Position pos)
    {
        AddMessage($"⚠ {trapDef.Name}を踏んだ！");

        // ダメージ処理
        int damage = trapDef.CalculateDamage(CurrentFloor);
        if (damage > 0)
        {
            Player.TakeDamage(Damage.Pure(damage));
            _lastDamageCause = DeathCause.Trap;
            AddMessage($"{damage}ダメージを受けた！");
        }

        // 状態異常処理
        if (trapDef.StatusEffect.HasValue)
        {
            // 毒無効種族は毒状態異常を無効化
            if (trapDef.StatusEffect.Value == StatusEffectType.Poison && RacialTraitSystem.IsPoisonImmune(Player.Race))
            {
                AddMessage("毒無効の体質により毒を受け付けなかった！");
            }
            else
            {
                Player.ApplyStatusEffect(new StatusEffect(trapDef.StatusEffect.Value, trapDef.StatusDuration));
                AddMessage($"{trapDef.StatusEffect.Value}状態になった！");
            }
        }

        // 特殊効果
        switch (trapDef.Type)
        {
            case TrapType.Teleport:
                var teleportPos = Map.GetRandomWalkablePosition(_random);
                if (teleportPos.HasValue)
                {
                    Player.Position = teleportPos.Value;
                    Map.ComputeFov(Player.Position, GameConstants.DefaultViewRadius);
                    AddMessage("見知らぬ場所に飛ばされた！");
                }
                break;

            case TrapType.Alarm:
                // 周囲の敵を覚醒させる（視界範囲内の敵をプレイヤーに向かわせる）
                int alertCount = 0;
                foreach (var enemy in Enemies.Where(e => e.IsAlive))
                {
                    int dist = enemy.Position.ChebyshevDistanceTo(pos);
                    if (dist <= 15)
                    {
                        alertCount++;
                    }
                }
                if (alertCount > 0)
                {
                    AddMessage($"警報が鳴り響いた！ {alertCount}体の敵に気づかれた！");
                }
                else
                {
                    AddMessage("警報が鳴り響いたが、周囲に敵はいなかった。");
                }
                break;

            case TrapType.PitFall:
                AddMessage("穴に落ちた！");
                break;
        }

        // プレイヤー死亡チェック
        if (!Player.IsAlive)
        {
            HandlePlayerDeath(DeathCause.Trap);
        }
    }

    /// <summary>
    /// TrapId文字列からTrapTypeを解析
    /// </summary>
    private static TrapType ParseTrapType(string? trapId)
    {
        if (string.IsNullOrEmpty(trapId))
            return TrapType.Arrow; // デフォルト

        if (Enum.TryParse<TrapType>(trapId, out var result))
            return result;

        return TrapType.Arrow;
    }

    #endregion

    #region ドア・隠し通路システム

    /// <summary>
    /// 周囲を探索して隠し通路や隠れた罠を発見する
    /// </summary>
    private bool TrySearch()
    {
        int per = Player.EffectiveStats.Perception;
        bool found = false;

        // 周囲8マスを探索
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(pos)) continue;

                var tile = Map.GetTile(pos);

                // 隠し通路の発見（PER判定: d20 + PER >= 15）
                if (tile.Type == TileType.SecretDoor)
                {
                    int roll = _random.Next(20) + 1 + per;
                    if (roll >= 15)
                    {
                        Map.SetTile(pos, TileType.DoorClosed);
                        AddMessage("🔍 隠し通路を発見した！");
                        found = true;
                    }
                }

                // 隠れた罠の発見（PER判定）
                if (tile.Type == TileType.TrapHidden)
                {
                    var trapType = ParseTrapType(tile.TrapId);
                    var trapDef = TrapDefinition.Get(trapType);
                    if (trapDef.CanDetect(per, _random))
                    {
                        Map.SetTile(pos.X, pos.Y, TileType.TrapVisible);
                        AddMessage($"🔍 {trapDef.Name}を発見した！");
                        found = true;
                    }
                }
            }
        }

        if (!found)
        {
            AddMessage("周囲を調べたが何も見つからなかった。");
        }

        return true; // 探索は常にターン消費
    }

    /// <summary>
    /// 隣接する開いたドアを閉じる
    /// </summary>
    private bool TryCloseDoor()
    {
        // 隣接8マスから開いたドアを探す
        var openDoors = new List<Position>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(pos)) continue;

                if (Map.GetTileType(pos) == TileType.DoorOpen)
                {
                    // ドアの上に敵やプレイヤーがいないことを確認
                    bool occupied = Enemies.Any(e => e.Position == pos && e.IsAlive);
                    if (!occupied)
                    {
                        openDoors.Add(pos);
                    }
                }
            }
        }

        if (openDoors.Count == 0)
        {
            AddMessage("近くに閉じられるドアがない。");
            return false;
        }

        // 最初に見つかった開いたドアを閉じる
        var doorPos = openDoors[0];
        Map.SetTile(doorPos, TileType.DoorClosed);
        AddMessage("ドアを閉めた。");
        return true;
    }

    #endregion

    #region 射撃・投擲システム

    /// <summary>
    /// 射程内で最も近い敵に遠距離攻撃を行う
    /// </summary>
    private bool TryRangedAttack()
    {
        var weapon = Player.Equipment.MainHand;
        if (weapon == null || weapon.Range <= 1)
        {
            AddMessage("遠距離攻撃可能な武器を装備していない。");
            return false;
        }

        // 射程内の敵を距離順に検索
        var target = FindNearestEnemyInRange(weapon.Range);
        if (target == null)
        {
            AddMessage("射程内に敵がいない。");
            return false;
        }

        // 射線チェック
        if (!Map.HasLineOfSight(Player.Position, target.Position))
        {
            AddMessage($"{target.Name}への射線が通っていない。");
            return false;
        }

        // 遠距離攻撃実行
        var result = _combatSystem.ExecuteAttack(Player, target, AttackType.Ranged);
        int distance = GetDistance(Player.Position, target.Position);

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"🏹 {target.Name}に{result.Damage?.Amount ?? 0}ダメージ！（距離{distance}）{critStr}");

            if (!target.IsAlive)
            {
                int gold = 0;
                if (target.Race == MonsterRace.Humanoid)
                {
                    int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                    gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));
                    Player.AddGold(gold);
                }
                string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                Player.GainExperience(target.ExperienceReward);
                TryDropItem(target);
                OnEnemyDefeated(target);
            }
        }
        else
        {
            AddMessage($"🏹 {target.Name}への射撃は外れた（距離{distance}）");
        }

        return true;
    }

    /// <summary>
    /// インベントリから最初の投擲可能アイテムを投げる
    /// </summary>
    private bool TryThrowItem()
    {
        // 投擲可能アイテムを検索（投擲武器 > その他武器 > 一般アイテム）
        var inventory = (Core.Entities.Inventory)Player.Inventory;
        var throwable = inventory.Items
            .OfType<Core.Items.Weapon>()
            .FirstOrDefault(w => w.WeaponType == Core.Items.WeaponType.Thrown);

        if (throwable == null)
        {
            AddMessage("投擲できるアイテムがない。");
            return false;
        }

        int throwRange = Math.Max(3, Player.EffectiveStats.Strength / 3);

        // 射程内の敵を検索
        var target = FindNearestEnemyInRange(throwRange);
        if (target == null)
        {
            AddMessage("投擲範囲内に敵がいない。");
            return false;
        }

        // 射線チェック
        if (!Map.HasLineOfSight(Player.Position, target.Position))
        {
            AddMessage($"{target.Name}への射線が通っていない。");
            return false;
        }

        // 投擲攻撃実行
        var result = _combatSystem.ExecuteAttack(Player, target, AttackType.Ranged);
        int distance = GetDistance(Player.Position, target.Position);

        // 投擲アイテムを消費
        inventory.Remove(throwable);

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"🗡 {throwable.Name}を{target.Name}に投げつけた！{result.Damage?.Amount ?? 0}ダメージ！{critStr}");

            if (!target.IsAlive)
            {
                int gold = 0;
                if (target.Race == MonsterRace.Humanoid)
                {
                    int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                    gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));
                    Player.AddGold(gold);
                }
                string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                Player.GainExperience(target.ExperienceReward);
                TryDropItem(target);
                OnEnemyDefeated(target);
            }
        }
        else
        {
            AddMessage($"🗡 {throwable.Name}を投げたが{target.Name}に当たらなかった");
        }

        // 投擲アイテムが地面に落ちる（当たっても外れても）
        GroundItems.Add((throwable, target.Position));

        return true;
    }

    /// <summary>
    /// 射程内で最も近い生存敵を検索
    /// </summary>
    private Enemy? FindNearestEnemyInRange(int range)
    {
        return Enemies
            .Where(e => e.IsAlive && GetDistance(Player.Position, e.Position) <= range)
            .OrderBy(e => GetDistance(Player.Position, e.Position))
            .FirstOrDefault();
    }

    /// <summary>
    /// 2点間のチェビシェフ距離（8方向移動基準）
    /// </summary>
    private static int GetDistance(Position a, Position b)
    {
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }

    /// <summary>
    /// 最初の使用可能な非パッシブスキルを使用する
    /// </summary>
    private bool TryUseFirstReadySkill(out int actionCost)
    {
        actionCost = TurnCosts.MoveNormal;

        // 使用可能なアクティブスキルを検索
        var usableSkill = Player.LearnedSkills
            .Select(id => SkillDatabase.GetById(id))
            .Where(s => s != null && s.Category != SkillCategory.Passive)
            .FirstOrDefault(s => _skillSystem.CanUse(s!.Id, Player.CurrentMp, Player.CurrentSp));

        if (usableSkill == null)
        {
            AddMessage("使用可能なスキルがない");
            return false;
        }

        var result = _skillSystem.Use(usableSkill.Id, Player.CurrentMp, Player.CurrentSp);
        if (!result.Success)
        {
            AddMessage(result.Message);
            return false;
        }

        // MP/SP消費
        if (result.MpCost > 0) Player.ConsumeMp(result.MpCost);
        if (result.SpCost > 0) Player.ConsumeSp(result.SpCost);

        // スキル効果を適用
        ApplySkillEffect(usableSkill);

        actionCost = Math.Max(1, result.TurnCost);
        return true;
    }

    /// <summary>スキルスロットにスキルを割り当て（0-4のインデックス、1-5キーに対応）</summary>
    public bool AssignSkillSlot(int slotIndex, string skillId)
    {
        if (slotIndex < 0 || slotIndex >= _skillSlots.Length) return false;
        if (!Player.LearnedSkills.Contains(skillId)) return false;
        var skill = SkillDatabase.GetById(skillId);
        if (skill == null || skill.Category == SkillCategory.Passive) return false;
        _skillSlots[slotIndex] = skillId;
        AddMessage($"スロット{slotIndex + 1}に{skill.Name}を割り当てた");
        return true;
    }

    /// <summary>スキルスロットの割り当てを解除</summary>
    public void ClearSkillSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _skillSlots.Length)
            _skillSlots[slotIndex] = null;
    }

    /// <summary>スキルスロットの情報を取得</summary>
    public IReadOnlyList<string?> GetSkillSlots() => _skillSlots;

    /// <summary>スキルスロット使用後のターン進行（MainWindowから呼ぶ）</summary>
    public void AdvanceTurnFromSkillSlot(int actionCost)
    {
        int finalCost = Math.Max(1, actionCost);
        TurnCount += finalCost;
        GameTime.AdvanceTurn(finalCost);
        ProcessEnemyTurns();
        ProcessTurnEffects();
        CheckTurnLimitWarnings();
        Map.ComputeFov(Player.Position, 8);

        if (!Player.IsAlive)
        {
            HandlePlayerDeath(_lastDamageCause);
        }
        else if (CheckTurnLimitExceeded())
        {
            HandleTurnLimitExceeded();
        }

        OnStateChanged?.Invoke();
    }

    /// <summary>スキルスロットのスキルを実行（1-6キー用）</summary>
    public bool TryUseSkillSlot(int slotIndex, out int actionCost)
    {
        actionCost = TurnCosts.MoveNormal;
        if (slotIndex < 0 || slotIndex >= _skillSlots.Length) return false;
        var skillId = _skillSlots[slotIndex];
        if (skillId == null)
        {
            AddMessage($"スロット{slotIndex + 1}にスキルが割り当てられていない");
            return false;
        }

        var skill = SkillDatabase.GetById(skillId);
        if (skill == null) return false;

        var result = _skillSystem.Use(skillId, Player.CurrentMp, Player.CurrentSp);
        if (!result.Success)
        {
            AddMessage(result.Message);
            return false;
        }

        if (result.MpCost > 0) Player.ConsumeMp(result.MpCost);
        if (result.SpCost > 0) Player.ConsumeSp(result.SpCost);
        ApplySkillEffect(skill);
        actionCost = Math.Max(1, result.TurnCost);
        return true;
    }

    /// <summary>現在の職業のスキルツリーを取得</summary>
    public IReadOnlyList<SkillTreeNode> GetCurrentClassSkillTree()
    {
        return SkillDatabase.GetSkillTree(Player.CharacterClass);
    }

    /// <summary>
    /// スキル効果をゲーム内に適用する
    /// </summary>
    private void ApplySkillEffect(SkillDefinition skill)
    {
        switch (skill.Category)
        {
            case SkillCategory.Combat:
                ApplyCombatSkill(skill);
                break;
            case SkillCategory.Magic:
                ApplyMagicSkill(skill);
                break;
            case SkillCategory.Support:
                ApplySupportSkill(skill);
                break;
            case SkillCategory.Exploration:
                ApplyExplorationSkill(skill);
                break;
            case SkillCategory.Crafting:
                AddMessage($"⚒ {skill.Name}を使った");
                break;
        }
    }

    private void ApplyCombatSkill(SkillDefinition skill)
    {
        switch (skill.Target)
        {
            case SkillTarget.SingleEnemy:
            {
                var target = FindNearestEnemyInRange(ActiveRange);
                if (target == null)
                {
                    AddMessage("近くに敵がいない");
                    return;
                }
                int damage = (int)(Player.EffectiveStats.Strength * skill.BasePower);
                target.TakeDamage(Damage.Physical(damage));
                AddMessage($"⚔ {skill.Name}！ {target.Name}に{damage}ダメージ！");
                if (!target.IsAlive)
                {
                    int gold = 0;
                    if (target.Race == MonsterRace.Humanoid)
                    {
                        gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                        Player.AddGold(gold);
                    }
                    string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                    AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                    Player.GainExperience(target.ExperienceReward);
                    TryDropItem(target);
                    OnEnemyDefeated(target);
                }
                break;
            }
            case SkillTarget.AllEnemies:
            {
                var targets = Enemies.Where(e => e.IsAlive && GetDistance(Player.Position, e.Position) <= ActiveRange).ToList();
                if (targets.Count == 0)
                {
                    AddMessage("近くに敵がいない");
                    return;
                }
                AddMessage($"⚔ {skill.Name}！");
                foreach (var target in targets)
                {
                    int damage = (int)(Player.EffectiveStats.Strength * skill.BasePower);
                    target.TakeDamage(Damage.Physical(damage));
                    AddMessage($"  {target.Name}に{damage}ダメージ！");
                    if (!target.IsAlive)
                    {
                        int gold = 0;
                        if (target.Race == MonsterRace.Humanoid)
                        {
                            gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                            Player.AddGold(gold);
                        }
                        string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                        AddMessage($"  {target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                        Player.GainExperience(target.ExperienceReward);
                        TryDropItem(target);
                        OnEnemyDefeated(target);
                    }
                }
                break;
            }
            case SkillTarget.Self:
            {
                // 自己バフ系戦闘スキル（盾防御等）
                AddMessage($"⚔ {skill.Name}を使った！");
                break;
            }
        }
    }

    private void ApplyMagicSkill(SkillDefinition skill)
    {
        switch (skill.Target)
        {
            case SkillTarget.Self:
            {
                // 回復系（heal等）
                if (skill.Id == "heal")
                {
                    int healAmount = (int)(Player.EffectiveStats.Mind * skill.BasePower * 2);
                    Player.Heal(healAmount);
                    AddMessage($"✨ {skill.Name}！ HPが{healAmount}回復した");
                }
                else
                {
                    AddMessage($"✨ {skill.Name}を使った！");
                }
                break;
            }
            case SkillTarget.SingleEnemy:
            {
                var target = FindNearestEnemyInRange(ActiveRange);
                if (target == null)
                {
                    AddMessage("近くに敵がいない");
                    return;
                }
                int damage = (int)(Player.EffectiveStats.Intelligence * skill.BasePower);
                target.TakeDamage(Damage.Magical(damage, skill.Element));
                AddMessage($"✨ {skill.Name}！ {target.Name}に{damage}ダメージ！");
                // life_drain の場合、HP吸収
                if (skill.Id == "life_drain")
                {
                    int healAmount = damage / 2;
                    Player.Heal(healAmount);
                    AddMessage($"  HP+{healAmount}吸収した");
                }
                if (!target.IsAlive)
                {
                    int gold = 0;
                    if (target.Race == MonsterRace.Humanoid)
                    {
                        gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                        Player.AddGold(gold);
                    }
                    string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                    AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                    Player.GainExperience(target.ExperienceReward);
                    TryDropItem(target);
                    OnEnemyDefeated(target);
                }
                break;
            }
            case SkillTarget.Area:
            {
                var targets = Enemies.Where(e => e.IsAlive && GetDistance(Player.Position, e.Position) <= ActiveRange).ToList();
                AddMessage($"✨ {skill.Name}！");
                foreach (var target in targets)
                {
                    int damage = (int)(Player.EffectiveStats.Intelligence * skill.BasePower);
                    target.TakeDamage(Damage.Magical(damage, skill.Element));
                    AddMessage($"  {target.Name}に{damage}ダメージ！");
                    if (!target.IsAlive)
                    {
                        int gold = 0;
                        if (target.Race == MonsterRace.Humanoid)
                        {
                            gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                            Player.AddGold(gold);
                        }
                        string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                        AddMessage($"  {target.Name}を倒した！経験値+{target.ExperienceReward}{goldStr}");
                        Player.GainExperience(target.ExperienceReward);
                        TryDropItem(target);
                        OnEnemyDefeated(target);
                    }
                }
                break;
            }
        }
    }

    private void ApplySupportSkill(SkillDefinition skill)
    {
        switch (skill.Id)
        {
            case "purify":
                var effects = Player.StatusEffects.ToList();
                var negative = effects.FirstOrDefault(e =>
                    e.Type == StatusEffectType.Poison ||
                    e.Type == StatusEffectType.Blind ||
                    e.Type == StatusEffectType.Curse);
                if (negative != null)
                {
                    Player.RemoveStatusEffect(negative.Type);
                    AddMessage($"🌟 {skill.Name}！ {negative.Type}が治った！");
                }
                else
                {
                    AddMessage($"🌟 {skill.Name}！ 解除する状態異常がなかった");
                }
                break;
            case "sneak":
                AddMessage($"🌟 {skill.Name}！ 気配を消した");
                break;
            default:
                AddMessage($"🌟 {skill.Name}を使った！");
                break;
        }
    }

    private void ApplyExplorationSkill(SkillDefinition skill)
    {
        switch (skill.Id)
        {
            case "disarm_trap":
            {
                var tile = Map.GetTile(Player.Position);
                if (tile.Type == TileType.TrapVisible)
                {
                    Map.SetTile(Player.Position.X, Player.Position.Y, TileType.Floor);
                    AddMessage($"🔧 {skill.Name}！ 罠を解除した");
                }
                else
                {
                    AddMessage("ここに解除できる罠がない");
                }
                break;
            }
            case "tracking":
            {
                var nearbyEnemies = Enemies.Where(e => e.IsAlive && GetDistance(Player.Position, e.Position) <= 20).ToList();
                if (nearbyEnemies.Count > 0)
                {
                    AddMessage($"🔍 {skill.Name}！ 周囲に{nearbyEnemies.Count}体の敵を感知した");
                }
                else
                {
                    AddMessage($"🔍 {skill.Name}！ 周囲に敵の気配はない");
                }
                break;
            }
            default:
                AddMessage($"🔍 {skill.Name}を使った");
                break;
        }
    }

    #endregion

    #region Spell Casting (魔法言語詠唱)

    /// <summary>詠唱モード開始</summary>
    private void StartSpellCasting()
    {
        _spellCastingSystem.CancelCasting();
        OnCastingStarted?.Invoke();
        AddMessage("詠唱を開始した…");
    }

    /// <summary>詠唱キャンセル</summary>
    private void CancelSpellCasting()
    {
        _spellCastingSystem.CancelCasting();
        OnCastingEnded?.Invoke();
        AddMessage("詠唱を中断した");
    }

    /// <summary>詠唱文にルーン語を追加</summary>
    public bool AddRuneWord(string wordId)
    {
        bool added = _spellCastingSystem.AddWord(wordId, Player);
        if (added)
        {
            var word = RuneWordDatabase.GetById(wordId);
            AddMessage($"ルーン語「{word?.Meaning ?? wordId}」を詠唱に追加");
            var preview = _spellCastingSystem.GetPreview(Player);
            OnSpellPreviewUpdated?.Invoke(preview);
        }
        else
        {
            AddMessage("そのルーン語は追加できない");
        }
        return added;
    }

    /// <summary>最後のルーン語を削除</summary>
    public bool RemoveLastRuneWord()
    {
        bool removed = _spellCastingSystem.RemoveLastWord();
        if (removed)
        {
            var preview = _spellCastingSystem.GetPreview(Player);
            OnSpellPreviewUpdated?.Invoke(preview);
        }
        return removed;
    }

    /// <summary>現在の詠唱プレビューを取得</summary>
    public SpellPreview GetSpellPreview() => _spellCastingSystem.GetPreview(Player);

    /// <summary>現在の詠唱文のルーン語ID一覧を取得</summary>
    public IReadOnlyList<string> GetCurrentIncantation() => _spellCastingSystem.CurrentIncantation.AsReadOnly();

    /// <summary>記録済み呪文一覧を取得</summary>
    public IReadOnlyList<SavedSpellRecipe> GetSavedSpells() => _spellCastingSystem.SavedSpells;

    /// <summary>現在の詠唱文を呪文として記録する</summary>
    public SavedSpellRecipe? SaveCurrentSpell(string name)
    {
        var recipe = _spellCastingSystem.SaveCurrentSpell(name, Player);
        if (recipe != null)
            AddMessage($"呪文「{name}」を記録しました");
        else
            AddMessage("呪文の記録に失敗しました");
        return recipe;
    }

    /// <summary>記録済み呪文を削除する</summary>
    public bool RemoveSavedSpell(int index)
    {
        bool removed = _spellCastingSystem.RemoveSavedSpell(index);
        if (removed)
            AddMessage("記録済み呪文を削除しました");
        return removed;
    }

    /// <summary>記録済み呪文を詠唱文にロードする</summary>
    public bool LoadSavedSpell(int index)
    {
        bool loaded = _spellCastingSystem.LoadSavedSpell(index, Player);
        if (loaded)
        {
            var preview = _spellCastingSystem.GetPreview(Player);
            OnSpellPreviewUpdated?.Invoke(preview);
            AddMessage("記録済み呪文を読み込みました");
        }
        else
        {
            AddMessage("呪文の読み込みに失敗しました");
        }
        return loaded;
    }

    /// <summary>詠唱実行</summary>
    private bool TryCastSpell(out int actionCost)
    {
        actionCost = TurnCosts.SpellMinimum;

        if (!_spellCastingSystem.IsCasting)
        {
            AddMessage("詠唱文が構築されていない");
            return false;
        }

        var result = _spellCastingSystem.Cast(Player, _random);
        OnCastingEnded?.Invoke();

        if (!result.IsSuccess)
        {
            AddMessage(result.Message);
            if (result.IsBackfire && result.BackfireDamage > 0)
            {
                Player.TakeDamage(Damage.Magical(result.BackfireDamage, Element.None));
                AddMessage($"暴発ダメージ: {result.BackfireDamage}");
                if (!Player.IsAlive)
                {
                    HandlePlayerDeath(DeathCause.Curse);
                }
            }
            actionCost = result.TurnCost > 0 ? result.TurnCost : TurnCosts.SpellMinimum;
            return result.TurnCost > 0;
        }

        // 詠唱ターンが2以上の場合、詠唱状態に入る（毎ターンカウントダウン）
        if (result.TurnCost > 1)
        {
            _chantRemainingTurns = result.TurnCost - 1; // 今のターン分を差し引く
            _pendingSpellResult = result;
            AddMessage($"詠唱を開始した… (残り{_chantRemainingTurns}ターン)");
            actionCost = TurnCosts.SpellMinimum;
            return true;
        }

        // 即時発動（TurnCost <= 1）
        AddMessage(result.Message);
        actionCost = Math.Max(1, result.TurnCost);

        var effect = SpellEffectResolver.Resolve(result);
        if (!effect.IsNone)
        {
            ApplySpellEffect(effect);
        }

        return true;
    }

    /// <summary>詠唱中の毎ターン処理</summary>
    private void ProcessChanting()
    {
        if (_chantRemainingTurns <= 0 || _pendingSpellResult == null)
            return;

        _chantRemainingTurns--;

        if (_chantRemainingTurns > 0)
        {
            AddMessage($"…詠唱中… (残り{_chantRemainingTurns}ターン)");
            return;
        }

        // 詠唱完了 — 効果を発動
        var result = _pendingSpellResult;
        _pendingSpellResult = null;

        AddMessage(result.Message);

        var effect = SpellEffectResolver.Resolve(result);
        if (!effect.IsNone)
        {
            ApplySpellEffect(effect);
        }

        AddMessage("詠唱が完了した！");
    }

    /// <summary>詠唱中かどうか（移動や他行動の制限に使用可能）</summary>
    public bool IsChanting => _chantRemainingTurns > 0;

    /// <summary>魔法効果をゲームに適用</summary>
    private void ApplySpellEffect(SpellEffect effect)
    {
        // 魔法属性による禁忌チェック
        if (effect.Element == Element.Dark || effect.Element == Element.Curse)
        {
            CheckTabooViolation(ReligionTabooType.UseDarkMagic);
        }
        if (effect.Element == Element.Light || effect.Element == Element.Holy)
        {
            CheckTabooViolation(ReligionTabooType.UseLightMagic);
        }

        switch (effect.Type)
        {
            case SpellEffectType.Damage:
                ApplySpellDamage(effect);
                break;
            case SpellEffectType.Heal:
                ApplySpellHeal(effect);
                break;
            case SpellEffectType.Purify:
                ApplySpellPurify(effect);
                break;
            case SpellEffectType.Buff:
            case SpellEffectType.Speed:
            case SpellEffectType.Blessing:
                ApplySpellBuff(effect);
                break;
            case SpellEffectType.Stealth:
                ApplySpellStealth(effect);
                break;
            case SpellEffectType.Control:
                ApplySpellControl(effect);
                break;
            case SpellEffectType.Detect:
                ApplySpellDetect(effect);
                break;
            case SpellEffectType.Unlock:
                ApplySpellUnlock(effect);
                break;
            case SpellEffectType.Teleport:
                ApplySpellTeleport(effect);
                break;
            case SpellEffectType.Summon:
                ApplySpellSummon(effect);
                break;
            case SpellEffectType.Copy:
                ApplySpellCopy(effect);
                break;
            case SpellEffectType.Reverse:
                ApplySpellReverse(effect);
                break;
            case SpellEffectType.Seal:
                ApplySpellSeal(effect);
                break;
            case SpellEffectType.Resurrect:
                ApplySpellResurrect(effect);
                break;
        }
    }

    /// <summary>魔法ダメージの適用</summary>
    private void ApplySpellDamage(SpellEffect effect)
    {
        int basePower = effect.Power;
        int intBonus = Player.EffectiveStats.Intelligence;
        int damage = Math.Max(1, basePower + intBonus);

        switch (effect.TargetType)
        {
            case SpellTargetType.SingleEnemy:
            case SpellTargetType.Forward:
                var target = GetNearestEnemy();
                if (target != null)
                {
                    target.TakeDamage(Damage.Magical(damage, effect.Element));
                    AddMessage($"{target.Name}に{damage}の{effect.Element}ダメージ！");
                    CheckEnemyDeath(target);
                }
                else
                {
                    AddMessage("対象が見つからない");
                }
                break;
            case SpellTargetType.AllEnemies:
            case SpellTargetType.All:
                var targets = GetEnemiesInRange(effect.Range);
                foreach (var enemy in targets)
                {
                    enemy.TakeDamage(Damage.Magical(damage, effect.Element));
                    AddMessage($"{enemy.Name}に{damage}の{effect.Element}ダメージ！");
                    CheckEnemyDeath(enemy);
                }
                if (targets.Count == 0)
                    AddMessage("範囲内に敵がいない");
                break;
            default:
                AddMessage("魔法が虚空に消えた");
                break;
        }
    }

    /// <summary>魔法回復の適用</summary>
    private void ApplySpellHeal(SpellEffect effect)
    {
        int mindBonus = Player.EffectiveStats.Mind;
        int healAmount = Math.Max(1, effect.Power + mindBonus);

        switch (effect.TargetType)
        {
            case SpellTargetType.Self:
            default:
                int oldHp = Player.CurrentHp;
                Player.Heal(healAmount);
                int actualHeal = Player.CurrentHp - oldHp;
                AddMessage($"HPが{actualHeal}回復した");
                break;
        }
    }

    /// <summary>魔法浄化の適用</summary>
    private void ApplySpellPurify(SpellEffect effect)
    {
        var negativeEffects = Player.StatusEffects
            .Where(e => e.Type is StatusEffectType.Poison or StatusEffectType.Burn
                or StatusEffectType.Freeze or StatusEffectType.Paralysis
                or StatusEffectType.Blind or StatusEffectType.Confusion)
            .ToList();

        if (negativeEffects.Count > 0)
        {
            var toRemove = negativeEffects.First();
            Player.RemoveStatusEffect(toRemove.Type);
            AddMessage($"{toRemove.Type}が浄化された");
        }
        else
        {
            AddMessage("浄化すべき状態異常がない");
        }
    }

    /// <summary>魔法強化の適用</summary>
    private void ApplySpellBuff(SpellEffect effect)
    {
        int duration = Math.Max(effect.Duration, 10);
        switch (effect.Type)
        {
            case SpellEffectType.Speed:
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Haste, duration)
                {
                    Name = "加速",
                    TurnCostModifier = 0.75f
                });
                AddMessage($"加速の魔法が発動した（{duration}ターン）");
                break;
            case SpellEffectType.Blessing:
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Blessing, duration)
                {
                    Name = "祝福",
                    AllStatsMultiplier = 1.10f
                });
                AddMessage($"祝福の魔法が発動した（{duration}ターン）");
                break;
            case SpellEffectType.Buff:
            default:
                // Buff魔法は攻撃力・防御力を同時に強化
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, duration)
                {
                    Name = "強化",
                    AttackMultiplier = 1.25f
                });
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Protection, duration)
                {
                    Name = "防護",
                    DefenseMultiplier = 1.50f
                });
                AddMessage($"能力強化の魔法が発動した（{duration}ターン）");
                break;
        }
    }

    /// <summary>魔法制御の適用</summary>
    private void ApplySpellControl(SpellEffect effect)
    {
        int duration = Math.Max(effect.Duration, 5);
        switch (effect.TargetType)
        {
            case SpellTargetType.SingleEnemy:
            case SpellTargetType.Forward:
                var target = GetNearestEnemy();
                if (target != null)
                {
                    // 制御魔法は魅了・減速・恐怖のいずれかを付与
                    var controlEffect = _random.Next(3) switch
                    {
                        0 => new StatusEffect(StatusEffectType.Charm, duration) { Name = "魅了" },
                        1 => new StatusEffect(StatusEffectType.Slow, duration) { Name = "減速", TurnCostModifier = 1.50f },
                        _ => new StatusEffect(StatusEffectType.Fear, duration) { Name = "恐怖" }
                    };
                    target.ApplyStatusEffect(controlEffect);
                    AddMessage($"{target.Name}に{controlEffect.Name}が効いた（{duration}ターン）");
                }
                else
                {
                    AddMessage("対象が見つからない");
                }
                break;
            case SpellTargetType.AllEnemies:
                var targets = GetEnemiesInRange(effect.Range);
                foreach (var e in targets)
                {
                    e.ApplyStatusEffect(new StatusEffect(StatusEffectType.Slow, duration)
                    {
                        Name = "減速",
                        TurnCostModifier = 1.50f
                    });
                }
                AddMessage($"{targets.Count}体の敵に減速が効いた（{duration}ターン）");
                break;
            default:
                AddMessage("制御の魔法が発動した");
                break;
        }
    }

    /// <summary>魔法探知の適用</summary>
    private void ApplySpellDetect(SpellEffect effect)
    {
        int detected = 0;
        int range = Math.Max(effect.Range, 5);
        for (int dy = -range; dy <= range; dy++)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                var pos = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(pos)) continue;
                var tile = Map.GetTile(pos);
                if (tile.Type == TileType.TrapHidden)
                {
                    Map.SetTile(pos, TileType.TrapVisible);
                    detected++;
                }
                if (!tile.IsExplored)
                {
                    tile.IsExplored = true;
                }
            }
        }
        int enemyCount = GetEnemiesInRange(range).Count;
        AddMessage($"探知の魔法が発動した（罠{detected}個発見、敵{enemyCount}体感知）");
    }

    /// <summary>魔法解錠の適用</summary>
    private void ApplySpellUnlock(SpellEffect effect)
    {
        // プレイヤー周囲のロックされたドアを解錠
        int unlocked = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(pos)) continue;
                var tile = Map.GetTile(pos);
                if (tile.IsLocked)
                {
                    tile.IsLocked = false;
                    Map.SetTile(pos, TileType.DoorOpen);
                    unlocked++;
                }
            }
        }
        AddMessage(unlocked > 0 ? $"{unlocked}個の錠前が開いた" : "解錠対象が見つからない");
    }

    /// <summary>魔法テレポートの適用</summary>
    private void ApplySpellTeleport(SpellEffect effect)
    {
        var teleportPos = Map.GetRandomWalkablePosition(_random);
        if (teleportPos.HasValue)
        {
            Player.Position = teleportPos.Value;
            Map.ComputeFov(Player.Position, GameConstants.DefaultViewRadius);
            AddMessage("空間が歪み、別の場所に転送された");
        }
        else
        {
            AddMessage("転送先が見つからなかった");
        }
    }

    /// <summary>魔法ステルスの適用</summary>
    private void ApplySpellStealth(SpellEffect effect)
    {
        int duration = Math.Max(effect.Duration, 5);
        Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Invisibility, duration));
        AddMessage($"姿を隠した（{duration}ターン）");
    }

    /// <summary>魔法召喚の適用</summary>
    private void ApplySpellSummon(SpellEffect effect)
    {
        // プレイヤー周囲の空きマスを探す
        Position? summonPos = null;
        for (int dy = -2; dy <= 2 && summonPos == null; dy++)
        {
            for (int dx = -2; dx <= 2 && summonPos == null; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var candidate = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(candidate) || !Map.IsWalkable(candidate)) continue;
                if (Enemies.Any(e => e.IsAlive && e.Position == candidate)) continue;
                if (Player.Position == candidate) continue;
                summonPos = candidate;
            }
        }

        if (summonPos == null)
        {
            AddMessage("召喚する空間がない");
            return;
        }

        var definitions = EnemyDefinitions.GetEnemiesForDepth(CurrentFloor);
        if (definitions.Count == 0)
        {
            AddMessage("召喚できる存在がいない");
            return;
        }

        var def = definitions[_random.Next(definitions.Count)];
        var summoned = _enemyFactory.CreateEnemy(def, summonPos.Value, null);
        summoned.Faction = Faction.Friendly;
        summoned.Name = $"召喚{def.Name}";
        Enemies.Add(summoned);
        AddMessage($"{summoned.Name}を召喚した！");
    }

    /// <summary>魔法複写の適用</summary>
    private void ApplySpellCopy(SpellEffect effect)
    {
        var concreteInventory = (Inventory)Player.Inventory;
        var lastItem = concreteInventory.Items.LastOrDefault();
        if (lastItem == null)
        {
            AddMessage("複写する対象がない");
            return;
        }

        var copy = ItemDefinitions.Create(lastItem.ItemId);
        if (copy == null)
        {
            AddMessage("この品は複写できない");
            return;
        }

        concreteInventory.Add(copy);
        AddMessage($"{copy.Name}を複写した！");
    }

    /// <summary>魔法反転の適用</summary>
    private void ApplySpellReverse(SpellEffect effect)
    {
        var negativeEffects = Player.StatusEffects
            .Where(e => e.Type is StatusEffectType.Poison or StatusEffectType.Burn
                or StatusEffectType.Freeze or StatusEffectType.Paralysis
                or StatusEffectType.Blind or StatusEffectType.Confusion
                or StatusEffectType.Slow or StatusEffectType.Weakness
                or StatusEffectType.Vulnerability or StatusEffectType.Silence
                or StatusEffectType.Curse)
            .ToList();

        if (negativeEffects.Count == 0)
        {
            AddMessage("反転すべき状態異常がない");
            return;
        }

        foreach (var neg in negativeEffects)
        {
            Player.RemoveStatusEffect(neg.Type);
        }
        // デバフ反転のボーナスとして一時的な強化を付与
        Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Regeneration, Math.Max(effect.Duration, 5)));
        AddMessage($"{negativeEffects.Count}個の状態異常を反転し、再生効果を得た！");
    }

    /// <summary>魔法封印の適用</summary>
    private void ApplySpellSeal(SpellEffect effect)
    {
        int duration = Math.Max(effect.Duration, 5);
        switch (effect.TargetType)
        {
            case SpellTargetType.SingleEnemy:
            case SpellTargetType.Forward:
                var target = GetNearestEnemy();
                if (target != null)
                {
                    target.ApplyStatusEffect(new StatusEffect(StatusEffectType.Silence, duration));
                    AddMessage($"{target.Name}の能力を封印した（{duration}ターン）");
                }
                else
                {
                    AddMessage("封印の対象が見つからない");
                }
                break;
            case SpellTargetType.AllEnemies:
            case SpellTargetType.All:
                var targets = GetEnemiesInRange(effect.Range);
                foreach (var enemy in targets)
                {
                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Silence, duration));
                }
                AddMessage(targets.Count > 0 ? $"{targets.Count}体の敵を封印した" : "範囲内に敵がいない");
                break;
            default:
                AddMessage("封印の魔法が虚空に消えた");
                break;
        }
    }

    /// <summary>魔法蘇生の適用</summary>
    private void ApplySpellResurrect(SpellEffect effect)
    {
        int healAmount = Math.Max(effect.Power, 1) + Player.EffectiveStats.Mind;
        int oldHp = Player.CurrentHp;
        Player.Heal(Player.MaxHp);
        int actualHeal = Player.CurrentHp - oldHp;

        // 負の状態異常も全て除去
        var negativeEffects = Player.StatusEffects
            .Where(e => e.Type is StatusEffectType.Poison or StatusEffectType.Burn
                or StatusEffectType.Freeze or StatusEffectType.Paralysis
                or StatusEffectType.Blind or StatusEffectType.Confusion
                or StatusEffectType.Curse)
            .ToList();
        foreach (var neg in negativeEffects)
        {
            Player.RemoveStatusEffect(neg.Type);
        }

        AddMessage($"蘇生の光に包まれ、HPが{actualHeal}回復し状態異常が浄化された！");
    }

    /// <summary>最も近い生存敵を取得</summary>
    private Enemy? GetNearestEnemy()
    {
        return Enemies
            .Where(e => e.IsAlive)
            .OrderBy(e => Math.Abs(e.Position.X - Player.Position.X) + Math.Abs(e.Position.Y - Player.Position.Y))
            .FirstOrDefault();
    }

    /// <summary>指定範囲内の生存敵リストを取得</summary>
    private List<Enemy> GetEnemiesInRange(int range)
    {
        return Enemies
            .Where(e => e.IsAlive &&
                Math.Abs(e.Position.X - Player.Position.X) <= range &&
                Math.Abs(e.Position.Y - Player.Position.Y) <= range)
            .ToList();
    }

    /// <summary>敵の死亡判定と処理</summary>
    private void CheckEnemyDeath(Enemy enemy)
    {
        if (!enemy.IsAlive)
        {
            AddMessage($"{enemy.Name}を倒した！");
            Player.GainExperience(enemy.ExperienceReward);
            OnEnemyDefeated(enemy);
        }
    }

    /// <summary>ルーン碑文から語彙を学習</summary>
    public VocabularyLearnResult LearnRuneWord(string wordId)
    {
        var result = VocabularyAcquisitionSystem.LearnFromRuneStone(Player, wordId);
        AddMessage(result.Message);
        return result;
    }

    /// <summary>古代の書から複数の語彙を学習</summary>
    public List<VocabularyLearnResult> LearnFromBook(string[] wordIds)
    {
        var results = VocabularyAcquisitionSystem.LearnFromAncientBook(Player, wordIds);
        foreach (var result in results)
        {
            AddMessage(result.Message);
        }
        return results;
    }

    /// <summary>ランダムな語彙を学習（難易度上限指定）</summary>
    public VocabularyLearnResult LearnRandomRuneWord(int maxDifficulty)
    {
        var result = VocabularyAcquisitionSystem.LearnRandomWord(Player, maxDifficulty, _random);
        AddMessage(result.Message);
        return result;
    }

    #endregion

    #region Religion System (宗教システム)

    /// <summary>宗教に入信する</summary>
    public ReligionActionResult TryJoinReligion(ReligionId religionId)
    {
        var result = _religionSystem.JoinReligion(Player, religionId);
        AddMessage(result.Message);

        if (result.Success)
        {
            OnReligionChanged?.Invoke();
        }

        return result;
    }

    /// <summary>宗教を脱退する</summary>
    private bool TryLeaveReligion()
    {
        var result = _religionSystem.LeaveReligion(Player);
        AddMessage(result.Message);

        if (result.Success)
        {
            // 背教状態を付与
            Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Apostasy, 100)
            {
                Name = "背教",
                AllStatsMultiplier = 0.90f  // 全ステータス-10%
            });
            OnReligionChanged?.Invoke();
        }

        return result.Success;
    }

    /// <summary>祈りを捧げる</summary>
    private bool TryPray()
    {
        var result = _religionSystem.Pray(Player);
        AddMessage(result.Message);
        return result.Success;
    }

    /// <summary>禁忌チェック（戦闘・魔法使用・行動時に呼ぶ）</summary>
    private void CheckTabooViolation(ReligionTabooType tabooType)
    {
        if (Player.CurrentReligion == null) return;

        var result = _religionSystem.ViolateTaboo(Player, tabooType);
        if (result.Success)
        {
            AddMessage(result.Message);
            OnReligionChanged?.Invoke();
        }
    }

    /// <summary>宗教の恩恵値を取得</summary>
    public double GetReligionBenefitValue(ReligionBenefitType type)
    {
        return _religionSystem.GetBenefitValue(Player, type);
    }

    /// <summary>宗教の恩恵が有効かチェック</summary>
    public bool HasReligionBenefit(ReligionBenefitType type)
    {
        return _religionSystem.HasBenefit(Player, type);
    }

    /// <summary>宗教ステータス情報を取得</summary>
    public ReligionStatusInfo GetReligionStatus()
    {
        return _religionSystem.GetStatus(Player);
    }

    /// <summary>敵対宗教の信者を倒した時の処理</summary>
    private void OnDefeatHostileFollower(ReligionId defeatedReligion)
    {
        var result = _religionSystem.OnDefeatHostileFollower(Player, defeatedReligion);
        if (result.Success)
        {
            AddMessage(result.Message);
        }
    }

    /// <summary>宗教の日次処理（新しい日のターン開始時に呼ぶ）</summary>
    private void ProcessReligionDailyTick()
    {
        _religionSystem.ProcessDailyTick(Player);
    }

    /// <summary>死に戻り時の信仰度引き継ぎ計算</summary>
    public int CalculateDeathTransferFaith()
    {
        return _religionSystem.CalculateDeathTransferFaith(Player);
    }

    /// <summary>死に戻り時の宗教特殊効果を取得</summary>
    public RebirthEffect? GetRebirthEffect()
    {
        return _religionSystem.GetRebirthEffect(Player);
    }

    /// <summary>宗教変更イベント</summary>
    public event Action? OnReligionChanged;

    #endregion

    #region World Map System (世界マップシステム)

    /// <summary>現在の領地</summary>
    public TerritoryId CurrentTerritory => _worldMapSystem.CurrentTerritory;

    /// <summary>訪問済み領地</summary>
    public IReadOnlyCollection<TerritoryId> VisitedTerritories => _worldMapSystem.VisitedTerritories;

    /// <summary>地上にいるか</summary>
    public bool IsOnSurface => _worldMapSystem.IsOnSurface;

    /// <summary>現在の領地情報を取得</summary>
    public TerritoryDefinition GetCurrentTerritoryInfo() => _worldMapSystem.GetCurrentTerritoryInfo();

    /// <summary>隣接領地一覧を取得</summary>
    public IReadOnlyList<TerritoryDefinition> GetAdjacentTerritories() => _worldMapSystem.GetAdjacentTerritories();

    /// <summary>現在の領地のロケーション一覧を取得</summary>
    public IReadOnlyList<LocationDefinition> GetCurrentLocations() => _worldMapSystem.GetCurrentLocations();

    /// <summary>現在の領地のダンジョン一覧を取得</summary>
    public IReadOnlyList<LocationDefinition> GetCurrentDungeons() => _worldMapSystem.GetCurrentDungeons();

    /// <summary>利用可能な施設一覧を取得</summary>
    public IReadOnlyList<FacilityDefinition> GetAvailableFacilities() =>
        _townSystem.GetAvailableFacilities(_worldMapSystem.CurrentTerritory);

    /// <summary>シンボルマップ上のロケーション配置情報を取得</summary>
    public IReadOnlyDictionary<Position, LocationDefinition> GetSymbolMapLocations() =>
        _symbolMapSystem.GetAllLocationPositions();

    /// <summary>プレイヤー位置のロケーション情報を取得</summary>
    public LocationDefinition? GetLocationAtPlayerPosition() =>
        _symbolMapSystem.GetLocationAt(Player.Position);

    /// <summary>領地間移動を実行</summary>
    public bool TryTravelTo(TerritoryId destination)
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("ダンジョン内から直接移動できない。まず地上に戻ること");
            return false;
        }

        var result = _worldMapSystem.TravelTo(destination, Player.Level);
        AddMessage(result.Message);

        if (result.Success)
        {
            // ターンコスト適用
            TurnCount += result.TurnCost;
            GameTime.AdvanceTurn(result.TurnCost);

            // 移動イベント判定
            var travelEvent = _worldMapSystem.RollTravelEvent(
                _worldMapSystem.CurrentTerritory, destination, _random);
            if (travelEvent != null)
            {
                AddMessage($"【旅路イベント】{travelEvent.Name}: {travelEvent.Description}");
            }

            // ショップ在庫リセット
            _shopSystem.ClearShopInventory();

            // 新しい領地のシンボルマップを生成
            GenerateSymbolMap();

            OnTerritoryChanged?.Invoke(destination);
            OnStateChanged?.Invoke();
        }

        return result.Success;
    }

    /// <summary>街・施設・宗教施設・フィールド・地形タイルに入る（ロケーションマップ遷移）</summary>
    private bool TryEnterTown()
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでにロケーション内にいる");
            return false;
        }

        // シンボルマップ上のロケーション（Dungeon以外）を判定
        var location = _symbolMapSystem.GetLocationAt(Player.Position);

        // ロケーションがある場合（既存の町・施設・フィールド等）
        if (location != null && location.Type != LocationType.Dungeon)
        {
            return EnterLocationMap(location);
        }

        // ロケーション未配置タイルでも地形タイルならフィールドマップに遷移
        if (_symbolMapSystem.CanEnterField(Player.Position))
        {
            return EnterTerrainFieldMap();
        }

        AddMessage("ここには入れるロケーションがない");
        return false;
    }

    /// <summary>ロケーション定義に基づくマップに遷移</summary>
    private bool EnterLocationMap(LocationDefinition location)
    {
        var generator = new LocationMapGenerator();
        var locationMap = generator.GenerateForLocation(location);

        _symbolMapReturnPosition = Player.Position;
        _worldMapSystem.IsOnSurface = false;
        _isInLocationMap = true;
        _currentMapName = location.Id;

        Map = locationMap;
        var startPos = locationMap.EntrancePosition ?? locationMap.StairsUpPosition ?? new Position(5, 5);
        Player.Position = startPos;

        Enemies.Clear();
        GroundItems.Clear();

        // フィールドのみ敵を配置
        if (location.Type == LocationType.Field)
        {
            SpawnEnemies();
        }

        Map.ComputeFov(Player.Position, 8);

        AddMessage($"【{location.Name}】に入った");
        OnSymbolMapEnterTown?.Invoke();
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>シンボルマップの地形タイルに応じたフィールドマップに遷移（Elona風）</summary>
    private bool EnterTerrainFieldMap()
    {
        var tile = Map.GetTile(Player.Position);
        var terrainName = SymbolMapSystem.GetTerrainName(tile.Type);

        var generator = new LocationMapGenerator();
        var fieldMap = generator.GenerateTerrainFieldMap(tile.Type, Player.Position);

        _symbolMapReturnPosition = Player.Position;
        _worldMapSystem.IsOnSurface = false;
        _isInLocationMap = true;
        _currentMapName = fieldMap.Name;

        Map = fieldMap;
        var startPos = fieldMap.EntrancePosition ?? new Position(fieldMap.Width / 2, fieldMap.Height - 1);
        Player.Position = startPos;

        Enemies.Clear();
        GroundItems.Clear();

        // フィールドマップには敵を配置
        SpawnEnemies();

        Map.ComputeFov(Player.Position, 8);

        AddMessage($"【{terrainName}】に足を踏み入れた（Tキーで脱出）");
        OnSymbolMapEnterTown?.Invoke();
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>タイルがNPCタイルかどうか判定</summary>
    private static bool IsNpcTile(TileType type) => type is
        TileType.NpcGuildReceptionist or
        TileType.NpcPriest or
        TileType.NpcShopkeeper or
        TileType.NpcBlacksmith or
        TileType.NpcInnkeeper;

    /// <summary>町内NPCタイルに隣接して話しかけた際のインタラクション</summary>
    private void HandleNpcTile(Tile tile)
    {
        var (speakerName, text, choices) = tile.Type switch
        {
            TileType.NpcGuildReceptionist when !IsGuildRegistered
                => ("ギルド受付", "冒険者ギルドへようこそ！ 登録しますか？",
                    new[]
                    {
                        new DialogueChoice("ギルドに登録する", "action:register_guild", 5),
                        new DialogueChoice("やめておく", "action:close")
                    }),
            TileType.NpcGuildReceptionist
                => ("ギルド受付", "お帰りなさい、冒険者さん。何かお手伝いしましょうか？",
                    new[]
                    {
                        new DialogueChoice("クエストを確認する", "action:view_quests"),
                        new DialogueChoice("仲間を募集する", "action:recruit_companion"),
                        new DialogueChoice("話を聞く", "action:close")
                    }),
            TileType.NpcPriest when Player.CurrentReligion == null
                => ("神父", "信仰の道へようこそ。どの神に仕えますか？",
                    new[]
                    {
                        new DialogueChoice("光の神殿に入信する", "action:join_religion_LightTemple", 3),
                        new DialogueChoice("闇の教団に入信する", "action:join_religion_DarkCult", 3),
                        new DialogueChoice("自然信仰に入信する", "action:join_religion_NatureWorship", 3),
                        new DialogueChoice("死の信仰に入信する", "action:join_religion_DeathFaith", 3),
                    }),
            TileType.NpcPriest
                => ("神父", $"ようこそ、信徒よ。今日はどうされましたか？",
                    new[]
                    {
                        new DialogueChoice("祈りを捧げる", "action:pray", 1),
                        new DialogueChoice("信仰情報を見る", "action:view_religion"),
                        new DialogueChoice("何もしない", "action:close")
                    }),
            TileType.NpcShopkeeper
                => ("商人", "いらっしゃい！何をお探しで？",
                    new[]
                    {
                        new DialogueChoice("商品を見る", "action:open_shop_GeneralShop"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcBlacksmith
                => ("鍛冶屋", "おう、いらっしゃい！武器や防具なら任せろ！",
                    new[]
                    {
                        new DialogueChoice("武器を見る", "action:open_shop_WeaponShop"),
                        new DialogueChoice("防具を見る", "action:open_shop_ArmorShop"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcInnkeeper
                => ("宿屋主人", "疲れたら休んでいきなさい。食料もあるよ。",
                    new[]
                    {
                        new DialogueChoice("宿に泊まる", "action:use_inn"),
                        new DialogueChoice("食料を買う", "action:open_shop_GeneralShop"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            _ => ("", "", Array.Empty<DialogueChoice>())
        };

        if (!string.IsNullOrEmpty(speakerName))
        {
            var dialogueNode = new DialogueNode(
                $"npc_{tile.Type}",
                speakerName,
                text,
                choices);
            _dialogueSystem.RegisterNode(dialogueNode);
            _dialogueSystem.StartDialogue(dialogueNode.Id);
            OnShowDialogue?.Invoke(dialogueNode);
        }
    }

    /// <summary>街を出る（ロケーションマップからシンボルマップへ帰還）</summary>
    private bool TryLeaveTown()
    {
        if (_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでに地上にいる");
            return false;
        }

        if (!_isInLocationMap)
        {
            AddMessage("ダンジョンから出るには<キーを使用");
            return false;
        }

        // ロケーションマップからシンボルマップに帰還
        _worldMapSystem.IsOnSurface = true;
        _isInLocationMap = false;
        GenerateSymbolMap();

        // 帰還位置を決定（保存された復帰位置 > ロケーションID検索 > デフォルト）
        Position? returnPos = _symbolMapReturnPosition
            ?? _symbolMapSystem.FindLocationPosition(_currentMapName);

        if (returnPos.HasValue)
        {
            Player.Position = returnPos.Value;
            Map.ComputeFov(Player.Position, 12);
        }

        _symbolMapReturnPosition = null;

        var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
        AddMessage($"{territoryName}のシンボルマップに戻った");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>宿屋を利用</summary>
    private bool TryUseInn()
    {
        if (!_townSystem.IsFacilityAvailable(_worldMapSystem.CurrentTerritory, FacilityType.Inn))
        {
            AddMessage("この領地には宿屋がない");
            return false;
        }

        var result = _townSystem.RestAtInn(Player);
        AddMessage(result.Message);

        if (result.Success && result.TurnCost > 0)
        {
            TurnCount += result.TurnCost;
            GameTime.AdvanceTurn(result.TurnCost);
        }

        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>教会を利用</summary>
    private bool TryVisitChurch()
    {
        if (!_townSystem.IsFacilityAvailable(_worldMapSystem.CurrentTerritory, FacilityType.Church))
        {
            AddMessage("この領地には教会がない");
            return false;
        }

        var result = _townSystem.RemoveCurseAtChurch(Player);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>銀行に預け入れ</summary>
    public bool TryDepositGold(int amount)
    {
        if (!_townSystem.IsFacilityAvailable(_worldMapSystem.CurrentTerritory, FacilityType.Bank))
        {
            AddMessage("この領地には銀行がない");
            return false;
        }

        var result = _townSystem.DepositGold(Player, amount);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>銀行から引き出し</summary>
    public bool TryWithdrawGold(int amount)
    {
        if (!_townSystem.IsFacilityAvailable(_worldMapSystem.CurrentTerritory, FacilityType.Bank))
        {
            AddMessage("この領地には銀行がない");
            return false;
        }

        var result = _townSystem.WithdrawGold(Player, amount);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>銀行残高を取得</summary>
    public int GetBankBalance() => _townSystem.BankBalance;

    /// <summary>ショップを初期化して在庫を取得</summary>
    public IReadOnlyList<ShopSystem.ShopItem> InitializeAndGetShopItems(FacilityType shopType)
    {
        _shopSystem.InitializeShop(shopType, _worldMapSystem.CurrentTerritory, Player.Level);
        return _shopSystem.GetShopItems(shopType);
    }

    /// <summary>ショップでアイテム購入</summary>
    public bool TryBuyItem(FacilityType shopType, int index)
    {
        double discount = ShopSystem.CalculateCharismaDiscount(Player.EffectiveStats.Charisma);

        // === GUI統合: 価格変動（PriceFluctuationSystem）===
        float reputationMod = PriceFluctuationSystem.GetReputationModifier(PlayerReputationRank, true);
        float karmaMod = PriceFluctuationSystem.GetKarmaModifier(PlayerKarmaRank, true);
        float territoryMod = PriceFluctuationSystem.GetTerritoryModifier(_worldMapSystem.CurrentTerritory, "general");
        discount *= (double)(reputationMod * karmaMod * territoryMod);

        var result = _shopSystem.Buy(Player, shopType, index, discount);
        if (result.Success && result.ItemId is not null)
        {
            var newItem = ItemDefinitions.Create(result.ItemId);
            if (newItem != null)
            {
                var inventory = (Inventory)Player.Inventory;
                if (!inventory.Add(newItem))
                {
                    AddMessage("インベントリが一杯のため、アイテムを受け取れなかった");
                }
            }
        }
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>ショップでアイテム売却</summary>
    public bool TrySellItem(string itemName, int baseValue)
    {
        double charismaBonus = ShopSystem.CalculateCharismaDiscount(Player.EffectiveStats.Charisma);

        // === GUI統合: 売却価格変動 ===
        float reputationMod = PriceFluctuationSystem.GetReputationModifier(PlayerReputationRank, false);
        float karmaMod = PriceFluctuationSystem.GetKarmaModifier(PlayerKarmaRank, false);
        charismaBonus *= (double)(reputationMod * karmaMod);

        var result = _shopSystem.Sell(Player, itemName, baseValue, charismaBonus);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>ダンジョン/ロケーションに入る（シンボルマップ→地下/ロケーション内部）</summary>
    public bool TryEnterDungeon()
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでにダンジョン/ロケーション内にいる");
            return false;
        }

        // シンボルマップ上のダンジョン入口にいるか確認
        if (_symbolMapSystem.IsDungeonEntrance(Player.Position))
        {
            var location = _symbolMapSystem.GetLocationAt(Player.Position);
            if (location != null)
            {
                _worldMapSystem.IsOnSurface = false;
                _isInLocationMap = false;
                _currentMapName = location.Id;
                CurrentFloor = 1;
                GenerateFloor();
                AddMessage($"【{location.Name}】─ ダンジョン第{CurrentFloor}層に足を踏み入れた...");
                OnSymbolMapEnterDungeon?.Invoke(location);
                OnStateChanged?.Invoke();
                return true;
            }
        }

        // ダンジョン以外のロケーション（町・施設・宗教施設・フィールド）の場合
        var loc = _symbolMapSystem.GetLocationAt(Player.Position);
        if (loc != null && loc.Type != LocationType.Dungeon)
        {
            return TryEnterTown();
        }

        // ロケーション外では汎用ダンジョン入場
        _worldMapSystem.IsOnSurface = false;
        _isInLocationMap = false;
        CurrentFloor = 1;
        GenerateFloor();
        AddMessage("ダンジョンに足を踏み入れた...");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>ダンジョン/ロケーションから脱出（地下→シンボルマップ）</summary>
    public bool TryExitDungeon()
    {
        if (_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでに地上にいる");
            return false;
        }

        _worldMapSystem.IsOnSurface = true;
        _isInLocationMap = false;
        GenerateSymbolMap();

        // ロケーション位置にプレイヤーを配置
        var locationPos = _symbolMapSystem.FindLocationPosition(_currentMapName);
        if (locationPos.HasValue)
        {
            Player.Position = locationPos.Value;
            Map.ComputeFov(Player.Position, 12);
        }

        var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
        AddMessage($"{territoryName}のシンボルマップに戻った");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>特殊フロアタイプの判定</summary>
    public SpecialFloorType DetermineSpecialFloorType(int depth)
    {
        return SpecialFloorSystem.DetermineFloorType(depth, _random);
    }

    /// <summary>特殊フロアの説明を取得</summary>
    public string GetSpecialFloorDescription(SpecialFloorType type)
    {
        return SpecialFloorSystem.GetFloorDescription(type);
    }

    #endregion

    #region NPC・クエスト・ギルドシステム

    /// <summary>現在の領地にいるNPC一覧を取得</summary>
    public IReadOnlyList<NpcDefinition> GetNpcsInCurrentTerritory()
    {
        return NpcDefinition.GetByTerritory(_worldMapSystem.CurrentTerritory);
    }

    /// <summary>NPCに話しかける</summary>
    public bool TryTalkToNpc(string npcId)
    {
        var npcDef = NpcDefinition.GetById(npcId);
        if (npcDef == null)
        {
            AddMessage("そのNPCは見つからない");
            return false;
        }

        if (npcDef.Location != _worldMapSystem.CurrentTerritory)
        {
            AddMessage($"{npcDef.Name}はこの領地にいない");
            return false;
        }

        // 初対面処理
        var state = _npcSystem.GetNpcState(npcId);
        if (!state.HasMet)
        {
            _npcSystem.MeetNpc(npcId);
            AddMessage($"{npcDef.Name}と初めて出会った");
        }

        // 最初の会話を開始
        if (npcDef.DialogueIds.Length > 0)
        {
            var node = _dialogueSystem.StartDialogue(npcDef.DialogueIds[0]);
            if (node != null)
            {
                OnShowDialogue?.Invoke(node);
                return true;
            }
        }

        AddMessage($"{npcDef.Name}「こんにちは、冒険者さん」");
        return true;
    }

    /// <summary>会話を進める</summary>
    public bool TryAdvanceDialogue()
    {
        if (!_dialogueSystem.IsInDialogue) return false;

        var next = _dialogueSystem.Advance();
        if (next != null)
        {
            OnShowDialogue?.Invoke(next);
        }
        else
        {
            AddMessage("会話が終了した");
        }
        return true;
    }

    /// <summary>会話の選択肢を選ぶ</summary>
    public bool TrySelectDialogueChoice(int choiceIndex, string npcId)
    {
        // アクション用NextNodeIdを事前に取得（SelectChoice後はCurrentNodeが変わるため）
        var currentNode = _dialogueSystem.CurrentNode;
        string? actionNodeId = null;
        if (currentNode?.HasChoices == true && currentNode.Choices != null
            && choiceIndex >= 0 && choiceIndex < currentNode.Choices.Length)
        {
            actionNodeId = currentNode.Choices[choiceIndex].NextNodeId;
        }

        // "action:"プレフィックスの場合はNPCアクションをディスパッチ
        if (actionNodeId != null && actionNodeId.StartsWith("action:"))
        {
            // 好感度変更を先に適用
            var choice = currentNode!.Choices![choiceIndex];
            if (choice.AffinityChange != 0)
            {
                _npcSystem.ModifyAffinity(npcId, choice.AffinityChange);
            }

            _dialogueSystem.EndDialogue();
            DispatchNpcAction(actionNodeId["action:".Length..]);
            return true;
        }

        var result = _dialogueSystem.SelectChoice(choiceIndex);
        if (result == null) return false;

        if (result.AffinityChange != 0)
        {
            _npcSystem.ModifyAffinity(npcId, result.AffinityChange);
        }

        if (result.NextNode != null)
        {
            OnShowDialogue?.Invoke(result.NextNode);
        }
        else
        {
            AddMessage("会話が終了した");
        }
        return result.Success;
    }

    /// <summary>NPC会話選択肢からのアクションディスパッチ</summary>
    private void DispatchNpcAction(string action)
    {
        switch (action)
        {
            case "register_guild":
                TryRegisterGuild();
                break;
            case "view_quests":
                OnShowQuestBoard?.Invoke();
                break;
            case "recruit_companion":
                HandleRecruitCompanion();
                break;
            case "pray":
                TryPray();
                break;
            case "view_religion":
                AddMessage("信仰情報を確認した");
                OnReligionChanged?.Invoke();
                break;
            case "use_inn":
                TryUseInn();
                break;
            case "close":
                // 何もせず会話を終了
                break;
            default:
                // ショップ系: open_shop_{FacilityType}
                if (action.StartsWith("open_shop_") && Enum.TryParse<FacilityType>(action["open_shop_".Length..], out var shopType))
                {
                    OnOpenShop?.Invoke(shopType);
                }
                // 入信系: join_religion_{ReligionId}
                else if (action.StartsWith("join_religion_") && Enum.TryParse<ReligionId>(action["join_religion_".Length..], out var religionId))
                {
                    TryJoinReligion(religionId);
                }
                else
                {
                    AddMessage($"不明なアクション: {action}");
                }
                break;
        }
    }

    /// <summary>会話を終了する</summary>
    public void EndDialogue()
    {
        _dialogueSystem.EndDialogue();
    }

    /// <summary>会話フラグを設定</summary>
    public void SetDialogueFlag(string flag)
    {
        _dialogueSystem.SetFlag(flag);
    }

    /// <summary>NPC好感度を取得</summary>
    public int GetNpcAffinity(string npcId)
    {
        return _npcSystem.GetNpcState(npcId).Affinity;
    }

    /// <summary>NPC好感度ランクを取得</summary>
    public string GetNpcAffinityRank(string npcId)
    {
        return NpcDefinition.GetAffinityRank(_npcSystem.GetNpcState(npcId).Affinity);
    }

    /// <summary>仲間募集処理</summary>
    private void HandleRecruitCompanion()
    {
        if (_companionSystem.Party.Count >= CompanionSystem.MaxPartySize)
        {
            AddMessage("パーティが満員だ！（最大4人）");
            return;
        }

        var candidates = GenerateCompanionCandidates();
        if (candidates.Count == 0)
        {
            AddMessage("現在募集できる仲間はいない");
            return;
        }

        OnShowRecruitCompanion?.Invoke(candidates);
    }

    /// <summary>ギルドランクとプレイヤーレベルに基づいて仲間候補を生成</summary>
    private List<CompanionSystem.CompanionData> GenerateCompanionCandidates()
    {
        var candidates = new List<CompanionSystem.CompanionData>();
        var rank = _guildSystem.CurrentRank;
        int playerLevel = Player.Level;

        // ランクに応じた候補数と最大レベル
        int maxCandidates = rank switch
        {
            >= GuildRank.Gold => 4,
            >= GuildRank.Silver => 3,
            _ => 2
        };
        int maxLevel = Math.Min(playerLevel + 2, (int)rank * 5 + 5);

        string[] mercenaryNames = { "ガレス", "ブリン", "セルディン", "ヴァルク", "オルガ", "ザイン", "レナス", "ドルク" };
        string[] allyNames = { "リーナ", "エルミア", "カイト", "ソフィア", "ハルト", "ミーア", "ジーク", "ユリア" };
        string[] petNames = { "ポチ", "タマ", "クロ", "シロ", "モモ", "レオ", "リュウ", "ハヤテ" };

        var existingNames = _companionSystem.Party.Select(c => c.Name).ToHashSet();

        for (int i = 0; i < maxCandidates; i++)
        {
            var type = (CompanionType)(i % 3);
            int level = Math.Max(1, _random.Next(Math.Max(1, playerLevel - 3), maxLevel + 1));
            int hireCost = CompanionSystem.CalculateHireCost(type, level);

            var namePool = type switch
            {
                CompanionType.Mercenary => mercenaryNames,
                CompanionType.Ally => allyNames,
                _ => petNames
            };

            string name = namePool[_random.Next(0, namePool.Length)];
            if (existingNames.Contains(name)) continue;
            existingNames.Add(name);

            candidates.Add(new CompanionSystem.CompanionData(
                Name: name,
                Type: type,
                AIMode: CompanionAIMode.Aggressive,
                Level: level,
                Loyalty: 50,
                HireCost: hireCost,
                Hp: 80 + level * 20,
                MaxHp: 80 + level * 20,
                Attack: 5 + level * 3,
                Defense: 3 + level * 2
            ));
        }

        return candidates;
    }

    /// <summary>仲間を雇用する</summary>
    public bool TryHireCompanion(CompanionSystem.CompanionData companion)
    {
        if (Player.Gold < companion.HireCost)
        {
            AddMessage($"お金が足りない！（必要: {companion.HireCost}G）");
            return false;
        }

        if (!_companionSystem.AddCompanion(companion))
        {
            AddMessage("仲間を追加できなかった");
            return false;
        }

        Player.AddGold(-companion.HireCost);
        AddMessage($"{companion.Name}が仲間になった！（{companion.HireCost}G）");
        return true;
    }

    /// <summary>ギルドに登録</summary>
    public bool TryRegisterGuild()
    {
        var result = _guildSystem.Register();
        AddMessage(result.Message);
        if (result.Success)
        {
            // 全クエストをクエストシステムに登録
            _questSystem.RegisterQuests(QuestDatabase.AllQuests);
        }
        return result.Success;
    }

    /// <summary>ギルドポイント加算</summary>
    public void AddGuildPoints(int points)
    {
        if (!_guildSystem.IsRegistered) return;
        var oldRank = _guildSystem.CurrentRank;
        var result = _guildSystem.AddPoints(points);
        AddMessage(result.Message);
        if (_guildSystem.CurrentRank > oldRank)
        {
            OnGuildRankUp?.Invoke(_guildSystem.CurrentRank);
        }
    }

    /// <summary>ギルドランクを取得</summary>
    public GuildRank GetGuildRank() => _guildSystem.CurrentRank;

    /// <summary>ギルドポイントを取得</summary>
    public int GetGuildPoints() => _guildSystem.GuildPoints;

    /// <summary>次のランクに必要なポイントを取得</summary>
    public int GetPointsForNextRank() => _guildSystem.GetPointsForNextRank();

    /// <summary>ギルドに登録済みか</summary>
    public bool IsGuildRegistered => _guildSystem.IsRegistered;

    /// <summary>クエストを受注</summary>
    public bool TryAcceptQuest(string questId)
    {
        var result = _questSystem.AcceptQuest(questId, Player.Level, _guildSystem.CurrentRank);
        AddMessage(result.Message);
        if (result.Success)
        {
            OnQuestUpdated?.Invoke(questId);
        }
        return result.Success;
    }

    /// <summary>クエスト報酬を受け取る</summary>
    public bool TryTurnInQuest(string questId)
    {
        var result = _questSystem.TurnInQuest(questId, Player);
        AddMessage(result.Message);
        if (result.Success && result.Reward?.GuildPoints > 0)
        {
            AddGuildPoints(result.Reward.GuildPoints);
        }
        if (result.Success)
        {
            OnQuestUpdated?.Invoke(questId);
        }
        return result.Success;
    }

    /// <summary>クエスト目標を更新（敵撃破・アイテム取得時に呼ぶ）</summary>
    public void UpdateQuestObjective(string targetId, int count = 1)
    {
        _questSystem.UpdateObjective(targetId, count);
    }

    /// <summary>アクティブクエスト一覧を取得</summary>
    public IReadOnlyList<(QuestDefinition Quest, QuestSystem.QuestProgress Progress)> GetActiveQuests()
    {
        return _questSystem.GetActiveQuests();
    }

    /// <summary>受注可能なクエスト一覧を取得</summary>
    public IReadOnlyList<QuestDefinition> GetAvailableQuests()
    {
        return _questSystem.GetAvailableQuests(Player.Level, _guildSystem.CurrentRank);
    }

    /// <summary>完了済みクエスト数を取得</summary>
    public int CompletedQuestCount => _questSystem.CompletedQuestCount;

    #endregion

    /// <summary>
    /// 自動探索の1ステップ実行。未探索タイルへBFSで最短経路を求め1マス移動。
    /// </summary>
    private bool StepAutoExplore()
    {
        // 停止条件チェック
        if (ShouldStopAutoExplore())
        {
            _autoExploring = false;
            return false;
        }

        _autoExploring = true;

        // BFSで最寄りの未探索隣接タイルを探す
        var nextStep = FindNextExploreStep();
        if (nextStep == null)
        {
            _autoExploring = false;
            AddMessage("探索する場所がない");
            return false;
        }

        // 次の移動先が閉じたドアなら開ける
        var nextTile = Map.GetTile(nextStep.Value);
        if (nextTile.Type == TileType.DoorClosed)
        {
            if (nextTile.IsLocked)
            {
                // 施錠ドアは自動探索では開けない（停止）
                _autoExploring = false;
                AddMessage("施錠されたドアがある");
                return false;
            }
            Map.SetTile(nextStep.Value.X, nextStep.Value.Y, TileType.DoorOpen);
            AddMessage("ドアを開けた");
            _lastMoveActionCost = TurnCosts.OpenDoor;
            TurnCount += _lastMoveActionCost;
            GameTime.AdvanceTurn(_lastMoveActionCost);
            ProcessEnemyTurns();
            OnStateChanged?.Invoke();
            return true;
        }

        // 1マス移動
        return TryMove(nextStep.Value);
    }

    /// <summary>
    /// 自動探索を停止すべきか判定
    /// </summary>
    private bool ShouldStopAutoExplore()
    {
        // 視界内に敵がいる
        if (Enemies.Any(e => e.IsAlive && Map.GetTile(e.Position).IsVisible))
            return true;

        // 足元にアイテムがある
        if (GroundItems.Any(i => i.Position == Player.Position))
            return true;

        // 階段の上にいる
        var currentTile = Map.GetTile(Player.Position);
        if (currentTile.Type == TileType.StairsDown || currentTile.Type == TileType.StairsUp)
            return true;

        // HPが半分以下
        if (Player.CurrentHp < Player.MaxHp / 2)
            return true;

        return false;
    }

    /// <summary>
    /// BFSで未探索エリアへの次の1歩を求める
    /// </summary>
    private Position? FindNextExploreStep()
    {
        var visited = new HashSet<Position>();
        var queue = new Queue<(Position Pos, Position FirstStep)>();

        visited.Add(Player.Position);

        // 4方向 + 斜め4方向
        var dirs = new (int dx, int dy)[]
        {
            (0, -1), (0, 1), (-1, 0), (1, 0),
            (-1, -1), (1, -1), (-1, 1), (1, 1)
        };

        // 初期近傍を追加
        foreach (var (dx, dy) in dirs)
        {
            var neighbor = new Position(Player.Position.X + dx, Player.Position.Y + dy);
            if (Map.IsInBounds(neighbor))
            {
                var nTile = Map.GetTile(neighbor);
                bool passable = !nTile.BlocksMovement || (nTile.Type == TileType.DoorClosed && !nTile.IsLocked);
                if (passable && !IsOccupied(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, neighbor));
                }
            }
        }

        while (queue.Count > 0)
        {
            var (current, firstStep) = queue.Dequeue();

            // このタイルの隣に未探索タイルがあれば、ここが目的地
            foreach (var (dx, dy) in dirs)
            {
                var adj = new Position(current.X + dx, current.Y + dy);
                if (Map.IsInBounds(adj) && !Map.GetTile(adj).IsExplored)
                {
                    return firstStep;
                }
            }

            // 探索続行
            foreach (var (dx, dy) in dirs)
            {
                var next = new Position(current.X + dx, current.Y + dy);
                if (!Map.IsInBounds(next)) continue;
                if (visited.Contains(next)) continue;
                var nxTile = Map.GetTile(next);
                bool passable = !nxTile.BlocksMovement || (nxTile.Type == TileType.DoorClosed && !nxTile.IsLocked);
                if (!passable) continue;

                visited.Add(next);
                queue.Enqueue((next, firstStep));
            }
        }

        return null;
    }

    /// <summary>
    /// 自動探索の継続実行（MainWindow側から呼ばれる）
    /// </summary>
    public bool ContinueAutoExplore()
    {
        if (!_autoExploring || IsGameOver || !IsRunning) return false;
        return StepAutoExplore();
    }

    public void AddMessage(string message)
    {
        var formattedMessage = $"[{TurnCount}] {message}";
        _messageHistory.Add(formattedMessage);
        OnMessage?.Invoke(formattedMessage);
    }

    #region Save/Load

    /// <summary>
    /// 現在のゲーム状態からセーブデータを作成
    /// </summary>
    public SaveData CreateSaveData()
    {
        var save = new SaveData
        {
            SavedAt = DateTime.Now,
            CurrentFloor = CurrentFloor,
            TurnCount = TurnCount,
            GameTime = new GameTimeSaveData { TotalTurns = GameTime.TotalTurns },
            MessageHistory = new List<string>(_messageHistory),
            TurnLimitExtended = _turnLimitExtended,
            TurnLimitRemoved = _turnLimitRemoved,
            Difficulty = Difficulty,
            CurrentMapName = _currentMapName,
            Player = new PlayerSaveData
            {
                Name = Player.Name,
                Level = Player.Level,
                Experience = Player.Experience,
                BaseStats = StatsSaveData.FromStats(Player.BaseStats),
                CurrentHp = Player.CurrentHp,
                CurrentMp = Player.CurrentMp,
                CurrentSp = Player.CurrentSp,
                Sanity = Player.Sanity,
                Hunger = Player.Hunger,
                RescueCountRemaining = Player.RescueCountRemaining,
                Position = PositionSaveData.FromPosition(Player.Position),
                LearnedWords = new Dictionary<string, int>(Player.LearnedWords),
                LearnedSkills = Player.LearnedSkills.ToList(),
                CurrentReligion = Player.CurrentReligion,
                FaithPoints = Player.FaithPoints,
                PreviousReligion = Player.PreviousReligion,
                PreviousReligions = Player.PreviousReligions.ToList(),
                HasApostasyCurse = Player.HasApostasyCurse,
                ApostasyCurseRemainingDays = Player.ApostasyCurseRemainingDays,
                DaysSinceLastPrayer = Player.DaysSinceLastPrayer,
                FaithCap = Player.FaithCap,
                Gold = Player.Gold,
                Race = Player.Race,
                CharacterClass = Player.CharacterClass,
                Background = Player.Background,
                TotalDeaths = TotalDeaths,
                TransferData = _transferData != null ? new TransferDataSaveData
                {
                    LearnedWords = new Dictionary<string, int>(_transferData.LearnedWords),
                    LearnedSkills = _transferData.LearnedSkills.ToList(),
                    Religion = _transferData.Religion,
                    FaithPoints = _transferData.FaithPoints,
                    PreviousReligion = _transferData.PreviousReligion,
                    PreviousReligions = _transferData.PreviousReligions.ToList(),
                    TotalDeaths = _transferData.TotalDeaths,
                    RescueCountRemaining = _transferData.RescueCountRemaining,
                    Sanity = _transferData.Sanity
                } : null
            }
        };

        // インベントリ保存
        var inventory = (Inventory)Player.Inventory;
        foreach (var item in inventory.Items)
        {
            save.Player.InventoryItems.Add(CreateItemSaveData(item));
        }

        // 装備保存
        foreach (var (slot, equip) in Player.Equipment.GetAll())
        {
            if (equip != null)
            {
                save.Player.EquippedItems[slot.ToString()] = CreateItemSaveData(equip);
            }
        }

        // クリア条件フラグ保存
        save.ClearFlags = _clearSystem.CreateSaveData();

        // スキルクールダウン保存
        save.SkillCooldowns = _skillSystem.GetCooldownState();

        // ワールドマップ保存
        save.CurrentTerritory = _worldMapSystem.CurrentTerritory.ToString();
        save.VisitedTerritories = _worldMapSystem.VisitedTerritories.Select(t => t.ToString()).ToList();
        save.IsOnSurface = _worldMapSystem.IsOnSurface;
        save.BankBalance = _townSystem.BankBalance;

        // NPC状態保存
        foreach (var (id, state) in _npcSystem.GetAllStates())
        {
            save.NpcStates[id] = new NpcStateSaveData
            {
                Affinity = state.Affinity,
                HasMet = state.HasMet,
                CompletedDialogues = state.CompletedDialogues.ToList()
            };
        }

        // クエスト状態保存
        save.ActiveQuests = _questSystem.CreateActiveQuestsSaveData();
        save.CompletedQuests = _questSystem.CompletedQuestIds.ToList();

        // ギルド状態保存
        save.GuildRank = _guildSystem.CurrentRank.ToString();
        save.GuildPoints = _guildSystem.GuildPoints;

        // 会話フラグ保存
        save.DialogueFlags = _dialogueSystem.GetAllFlags().ToList();

        return save;
    }

    /// <summary>
    /// セーブデータからゲーム状態を復元
    /// </summary>
    public void LoadSaveData(SaveData save)
    {
        // プレイヤー復元
        Player = Player.Create(save.Player.Name, save.Player.BaseStats.ToStats());
        Player.RestoreFromSave(
            save.Player.Level,
            save.Player.Experience,
            save.Player.Sanity,
            save.Player.Hunger,
            save.Player.CurrentHp,
            save.Player.CurrentMp,
            save.Player.CurrentSp,
            save.Player.RescueCountRemaining,
            save.Player.Race,
            save.Player.CharacterClass,
            save.Player.Background
        );
        Player.Position = save.Player.Position.ToPosition();

        // 魔法言語復元
        foreach (var (wordId, mastery) in save.Player.LearnedWords)
        {
            Player.LearnedWords[wordId] = mastery;
        }

        // スキル復元
        foreach (var skillId in save.Player.LearnedSkills)
        {
            Player.LearnedSkills.Add(skillId);
        }

        // 宗教復元
        if (!string.IsNullOrEmpty(save.Player.CurrentReligion))
        {
            Player.JoinReligion(save.Player.CurrentReligion);
            Player.AddFaithPoints(save.Player.FaithPoints);
        }

        // 宗教追加プロパティ復元
        Player.HasApostasyCurse = save.Player.HasApostasyCurse;
        Player.ApostasyCurseRemainingDays = save.Player.ApostasyCurseRemainingDays;
        Player.DaysSinceLastPrayer = save.Player.DaysSinceLastPrayer;
        Player.FaithCap = save.Player.FaithCap;
        foreach (var prevReligion in save.Player.PreviousReligions)
        {
            Player.PreviousReligions.Add(prevReligion);
        }

        // インベントリ復元
        var inventory = (Inventory)Player.Inventory;
        foreach (var itemData in save.Player.InventoryItems)
        {
            var item = RestoreItem(itemData);
            if (item != null) inventory.Add(item);
        }

        // 装備復元
        foreach (var (slotName, itemData) in save.Player.EquippedItems)
        {
            if (Enum.TryParse<RougelikeGame.Core.Items.EquipmentSlot>(slotName, out var slot))
            {
                var item = RestoreItem(itemData) as EquipmentItem;
                if (item != null)
                {
                    Player.Equipment.Equip(item, Player);
                }
            }
        }

        // STRベースの最大重量を更新
        Player.UpdateMaxWeight();

        // ゲーム状態復元
        CurrentFloor = save.CurrentFloor;
        TurnCount = save.TurnCount;
        GameTime.SetTotalTurns(save.GameTime.TotalTurns);

        // マップ名復元
        _currentMapName = save.CurrentMapName ?? "capital_guild";

        // マップ再生成（セーブには含まない）
        GenerateFloor();
        Player.Position = save.Player.Position.ToPosition();
        Map.ComputeFov(Player.Position, 8);

        // メッセージ履歴復元
        _messageHistory.Clear();
        _messageHistory.AddRange(save.MessageHistory);

        // イベント再購読
        SubscribePlayerEvents();

        IsGameOver = false;
        IsRunning = true;
        _autoExploring = false;
        _turnLimitExtended = save.TurnLimitExtended;
        _turnLimitRemoved = save.TurnLimitRemoved;
        _lastTurnLimitWarningStage = 0;
        Difficulty = save.Difficulty;
        Player.SetGold(save.Player.Gold);
        TotalDeaths = save.Player.TotalDeaths;

        // 引き継ぎデータ復元
        if (save.Player.TransferData != null)
        {
            _transferData = new TransferData
            {
                LearnedWords = new Dictionary<string, int>(save.Player.TransferData.LearnedWords),
                LearnedSkills = new HashSet<string>(save.Player.TransferData.LearnedSkills),
                Religion = save.Player.TransferData.Religion,
                FaithPoints = save.Player.TransferData.FaithPoints,
                PreviousReligion = save.Player.TransferData.PreviousReligion,
                PreviousReligions = new HashSet<string>(save.Player.TransferData.PreviousReligions),
                TotalDeaths = save.Player.TransferData.TotalDeaths,
                RescueCountRemaining = save.Player.TransferData.RescueCountRemaining,
                Sanity = save.Player.TransferData.Sanity
            };
        }

        // クリア条件フラグ復元
        if (save.ClearFlags != null)
        {
            _clearSystem.RestoreFromSave(save.ClearFlags);
        }

        // スキルクールダウン復元
        if (save.SkillCooldowns.Count > 0)
        {
            _skillSystem.RestoreCooldownState(save.SkillCooldowns);
        }

        // ワールドマップ復元
        if (Enum.TryParse<TerritoryId>(save.CurrentTerritory, out var territory))
        {
            var visited = new HashSet<TerritoryId>();
            foreach (var t in save.VisitedTerritories)
            {
                if (Enum.TryParse<TerritoryId>(t, out var v))
                    visited.Add(v);
            }
            _worldMapSystem.SetTerritory(territory, visited);
        }
        _worldMapSystem.IsOnSurface = save.IsOnSurface;
        _townSystem.SetBankBalance(save.BankBalance);

        // NPC状態復元
        if (save.NpcStates.Count > 0)
        {
            _npcSystem.RestoreStates(save.NpcStates);
        }

        // クエスト状態復元
        _questSystem.RegisterQuests(QuestDatabase.AllQuests);
        if (save.ActiveQuests.Count > 0 || save.CompletedQuests.Count > 0)
        {
            _questSystem.RestoreState(save.ActiveQuests, save.CompletedQuests);
        }

        // ギルド状態復元
        if (Enum.TryParse<GuildRank>(save.GuildRank, out var guildRank))
        {
            _guildSystem.RestoreState(guildRank, save.GuildPoints);
        }

        // 会話フラグ復元
        if (save.DialogueFlags.Count > 0)
        {
            _dialogueSystem.RestoreFlags(save.DialogueFlags);
        }

        AddMessage("セーブデータをロードした");
        OnStateChanged?.Invoke();
    }

    private static ItemSaveData CreateItemSaveData(Item item) => new()
    {
        ItemId = item.ItemId,
        EnhancementLevel = item.EnhancementLevel,
        IsIdentified = item.IsIdentified,
        IsCursed = item.IsCursed,
        IsBlessed = item.IsBlessed,
        Durability = item.Durability,
        StackCount = item is IStackable stackable ? stackable.StackCount : 1
    };

    private static Item? RestoreItem(ItemSaveData data)
    {
        var item = ItemDefinitions.Create(data.ItemId);
        if (item == null) return null;

        item.EnhancementLevel = data.EnhancementLevel;
        item.IsIdentified = data.IsIdentified;
        item.IsCursed = data.IsCursed;
        item.IsBlessed = data.IsBlessed;
        item.Durability = data.Durability;

        if (item is IStackable stackable)
        {
            stackable.StackCount = data.StackCount;
        }

        return item;
    }

    #endregion

    #region 新システムアクセス

    /// <summary>現在の季節</summary>
    public Season CurrentSeason => SeasonSystem.GetSeason(GameTime.Month);

    /// <summary>季節名を取得</summary>
    public string CurrentSeasonName => SeasonSystem.GetSeasonName(CurrentSeason);

    /// <summary>現在の天候</summary>
    public Weather CurrentWeather { get; private set; } = Weather.Clear;

    /// <summary>天候名を取得</summary>
    public string CurrentWeatherName => WeatherSystem.GetWeatherName(CurrentWeather);

    /// <summary>プレイヤーの渇きレベル（将来Player拡張時に実プロパティに接続）</summary>
    public ThirstLevel PlayerThirstLevel { get; private set; } = ThirstLevel.Hydrated;

    /// <summary>渇きレベル名</summary>
    public string PlayerThirstName => ThirstSystem.GetThirstName(PlayerThirstLevel);

    /// <summary>カルマ値</summary>
    public int PlayerKarma => _karmaSystem.KarmaValue;

    /// <summary>カルマランク</summary>
    public KarmaRank PlayerKarmaRank => _karmaSystem.CurrentRank;

    /// <summary>評判ランク（現在の領地）</summary>
    public ReputationRank PlayerReputationRank => _reputationSystem.GetReputationRank(_worldMapSystem.CurrentTerritory);

    /// <summary>仲間パーティ</summary>
    public IReadOnlyList<CompanionSystem.CompanionData> CompanionParty => _companionSystem.Party;

    /// <summary>仲間数</summary>
    public int CompanionCount => _companionSystem.Party.Count;

    /// <summary>図鑑システム</summary>
    public EncyclopediaSystem GetEncyclopediaSystem() => _encyclopediaSystem;

    /// <summary>図鑑エントリを自動登録し発見レベルを上昇させる</summary>
    private void RegisterAndDiscoverEncyclopedia(EncyclopediaCategory category, string id, string name)
    {
        if (_encyclopediaSystem.GetEntry(id) == null)
        {
            _encyclopediaSystem.RegisterEntry(category, id, name, 3, new Dictionary<int, string>
            {
                { 1, $"{name}を発見した。" },
                { 2, $"{name}についてより深く知った。" },
                { 3, $"{name}の全てを理解した。" }
            });
        }
        if (_encyclopediaSystem.IncrementDiscovery(id))
        {
            var entry = _encyclopediaSystem.GetEntry(id);
            if (entry != null && entry.DiscoveryLevel == 1)
            {
                AddMessage($"📖 図鑑に{name}が記録された！");
            }
        }
    }

    /// <summary>死亡ログシステム</summary>
    public DeathLogSystem GetDeathLogSystem() => _deathLogSystem;

    /// <summary>スキルツリーシステム</summary>
    public SkillTreeSystem GetSkillTreeSystem() => _skillTreeSystem;

    /// <summary>仲間システム</summary>
    public CompanionSystem GetCompanionSystem() => _companionSystem;

    /// <summary>拠点システム</summary>
    public BaseConstructionSystem GetBaseConstructionSystem() => _baseConstructionSystem;

    /// <summary>カルマシステム</summary>
    public KarmaSystem GetKarmaSystem() => _karmaSystem;

    /// <summary>評判システム</summary>
    public ReputationSystem GetReputationSystem() => _reputationSystem;

    /// <summary>誓約システム</summary>
    public OathSystem GetOathSystem() => _oathSystem;

    /// <summary>投資システム</summary>
    public InvestmentSystem GetInvestmentSystem() => _investmentSystem;

    /// <summary>グリッドインベントリシステム</summary>
    public GridInventorySystem GetGridInventorySystem() => _gridInventorySystem;

    #endregion

    #region Ver.prt.0.5 ゲーム完走フロー

    /// <summary>NG+でゲームを開始する</summary>
    public void InitializeNewGamePlus(string playerName, Race race, Core.CharacterClass characterClass,
        Background background, DifficultyLevel difficulty, NewGamePlusTier tier)
    {
        _ngPlusTier = tier;
        Initialize(playerName, race, characterClass, background, difficulty);

        var config = NewGamePlusSystem.GetConfig(tier);
        if (config != null)
        {
            AddMessage(NewGamePlusSystem.GetStartMessage(tier));
            var carryOverItems = NewGamePlusSystem.GetCarryOverItems(tier);
            AddMessage($"⚔ 引き継ぎ項目: {string.Join(", ", carryOverItems)}");
        }
    }

    /// <summary>無限ダンジョンモードを開始する</summary>
    public void StartInfiniteDungeon(string playerName, Race race, Core.CharacterClass characterClass,
        Background background, DifficultyLevel difficulty)
    {
        if (!_hasCleared) return;
        _infiniteDungeonMode = true;
        _infiniteDungeonKills = 0;
        Initialize(playerName, race, characterClass, background, difficulty);
        AddMessage("♾ 無限ダンジョンに挑戦！ どこまで潜れるか試してみよう");
    }

    /// <summary>料理を実行する</summary>
    public bool TryCook(string recipeName)
    {
        var recipe = CookingSystem.FindRecipe(recipeName);
        if (recipe == null)
        {
            AddMessage("そのレシピは存在しない");
            return false;
        }

        // 素材チェック（簡易版: インベントリにアイテム名で検索）
        foreach (var ingredient in recipe.Ingredients)
        {
            if (!Player.Inventory.Items.Any(i => i.Name.Contains(ingredient)))
            {
                AddMessage($"素材が足りない: {ingredient}");
                return false;
            }
        }

        // 素材消費（各素材1つ消費）
        foreach (var ingredient in recipe.Ingredients)
        {
            var item = Player.Inventory.Items.FirstOrDefault(i => i.Name.Contains(ingredient));
            if (item is Item concreteItem)
            {
                ((Inventory)Player.Inventory).Remove(concreteItem);
            }
        }

        // 料理結果の品質計算
        float quality = CookingSystem.CalculateQuality(Player.Level * 2);
        int hpRestore = (int)(recipe.HpRestore * quality);
        int mpRestore = (int)(recipe.MpRestore * quality);

        AddMessage($"🍳 {recipe.Name}を作った！ (HP+{hpRestore}, MP+{mpRestore})");
        Player.Heal(hpRestore);

        if (!string.IsNullOrEmpty(recipe.SpecialEffect))
        {
            AddMessage($"✨ 特殊効果: {recipe.SpecialEffect}");
        }

        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>スキル合成を実行する</summary>
    public bool TryFuseSkills(string skillA, string skillB)
    {
        int proficiency = Player.Level * 3;

        if (!SkillFusionSystem.CanFuse(skillA, skillB, proficiency))
        {
            int required = SkillFusionSystem.GetRequiredProficiency(skillA, skillB);
            if (required < 0)
            {
                AddMessage("その組み合わせでは合成できない");
            }
            else
            {
                AddMessage($"熟練度が足りない（必要: {required}, 現在: {proficiency}）");
            }
            return false;
        }

        var result = SkillFusionSystem.ExecuteFusion(skillA, skillB, proficiency);
        if (result != null)
        {
            AddMessage($"⚡ スキル合成成功！ {skillA} + {skillB} → {result}");
            OnStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>転職を実行する</summary>
    public bool TryClassChange(Core.CharacterClass targetClass)
    {
        var completedQuests = new HashSet<string>(_questSystem.CompletedQuestIds);
        if (!MultiClassSystem.CanClassChange(Player.CharacterClass, targetClass, Player.Level, completedQuests))
        {
            var req = MultiClassSystem.GetRequirement(Player.CharacterClass, targetClass);
            if (req == null)
            {
                AddMessage("その転職先は存在しない");
            }
            else
            {
                AddMessage($"転職条件を満たしていない（Lv.{req.RequiredLevel}以上 + クエスト「{req.QuestFlag}」完了が必要）");
            }
            return false;
        }

        AddMessage($"🔄 {Player.CharacterClass} → {targetClass} に転職した！");
        AddMessage($"サブクラス経験値倍率: {MultiClassSystem.GetSubclassExpRate():P0}");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>拠点施設の効果を休息時に適用</summary>
    public float GetBaseRestBonus()
    {
        return _baseConstructionSystem.GetRestHpRecoveryMultiplier();
    }

    /// <summary>拠点施設の製作ボーナスを取得</summary>
    public float GetBaseCraftingBonus()
    {
        return _baseConstructionSystem.GetCraftingSuccessBonus();
    }

    /// <summary>ゲームオーバー選択肢を処理</summary>
    public void ProcessGameOverChoice(GameOverSystem.GameOverChoice choice)
    {
        var result = GameOverSystem.ProcessChoice(choice, Player.Sanity);
        AddMessage(result.Message);

        if (result.ShouldReturnToTitle)
        {
            IsRunning = false;
            OnGameOver?.Invoke();
        }
        else if (result.ShouldQuitGame)
        {
            IsRunning = false;
            OnGameOver?.Invoke();
        }
    }

    #endregion

    #region GUI統合: 全システムGUI動作確認対応 — アクティビティ＆新システムアクセス

    // === スタンス切替 ===

    /// <summary>現在の戦闘スタンス</summary>
    public CombatStance PlayerStance => _playerStance;

    /// <summary>スタンス名</summary>
    public string PlayerStanceName => CombatStanceSystem.GetStanceName(_playerStance);

    /// <summary>スタンスを切り替える</summary>
    public void CycleCombatStance()
    {
        _playerStance = _playerStance switch
        {
            CombatStance.Balanced => CombatStance.Aggressive,
            CombatStance.Aggressive => CombatStance.Defensive,
            CombatStance.Defensive => CombatStance.Balanced,
            _ => CombatStance.Balanced
        };
        AddMessage($"⚔ スタンス変更: {CombatStanceSystem.GetStanceName(_playerStance)} — {CombatStanceSystem.GetStanceDescription(_playerStance)}");
        OnStateChanged?.Invoke();
    }

    // === 休憩 (RestSystem) ===

    /// <summary>野営を試みる</summary>
    public bool TryCamp(SleepQuality quality)
    {
        bool isIndoor = _isInLocationMap;
        bool hasEnemyNearby = IsInCombat();

        if (!RestSystem.CanCamp(isIndoor, hasEnemyNearby, CurrentFloor))
        {
            AddMessage("ここでは休めない（敵が近くにいるか、危険な場所だ）");
            return false;
        }

        var (hpRecovery, mpRecovery, fatigueRecovery, sanityRecovery) = RestSystem.GetRecoveryRates(quality);
        int sleepDuration = RestSystem.GetSleepDuration(quality);

        // 襲撃チェック
        float ambushChance = RestSystem.CalculateAmbushChance(CurrentFloor, false, _companionSystem.Party.Count > 0);
        if (_random.NextDouble() < ambushChance)
        {
            AddMessage("⚠ 休息中に敵に襲撃された！");
            SpawnEnemies();
            OnStateChanged?.Invoke();
            return false;
        }

        // 回復適用
        int hpAmount = (int)(Player.MaxHp * hpRecovery);
        Player.Heal(hpAmount);

        // 疲労回復
        if (fatigueRecovery > 0.5f) _playerFatigue = FatigueLevel.Fresh;
        else if (fatigueRecovery > 0.2f && _playerFatigue > FatigueLevel.Mild) _playerFatigue--;

        // 衛生回復（宿屋利用時）
        if (quality >= SleepQuality.DeepSleep) _playerHygiene = HygieneLevel.Clean;

        // 拠点休息ボーナス
        float restBonus = GetBaseRestBonus();
        if (restBonus > 1.0f) hpAmount = (int)(hpAmount * restBonus);

        TurnCount += sleepDuration;
        GameTime.AdvanceTurn(sleepDuration);

        AddMessage($"💤 {RestSystem.GetQualityName(quality)}な休息をとった (HP+{hpAmount}, 疲労回復)");
        AddMessage($"  {sleepDuration}ターン経過");
        OnStateChanged?.Invoke();
        return true;
    }

    // === ギャンブル (GamblingSystem) ===

    /// <summary>ギャンブルを行う</summary>
    public bool TryGamble(GamblingGameType gameType, int betAmount, int playerGuess)
    {
        int minBet = GamblingSystem.GetMinimumBet(gameType);
        if (Player.Gold < betAmount || betAmount < minBet)
        {
            AddMessage($"賭け金が足りない（最低{minBet}G必要）");
            return false;
        }

        Player.AddGold(-betAmount);
        bool won = false;

        switch (gameType)
        {
            case GamblingGameType.Dice:
                int diceResult = _random.Next(6) + 1;
                won = GamblingSystem.JudgeDice(playerGuess, diceResult);
                AddMessage($"🎲 サイコロの目: {diceResult}");
                break;
            case GamblingGameType.ChoHan:
                int dice1 = _random.Next(6) + 1;
                int dice2 = _random.Next(6) + 1;
                won = GamblingSystem.JudgeChoHan(playerGuess == 0, dice1, dice2);
                AddMessage($"🎲 丁半: {dice1}+{dice2}={dice1 + dice2} ({((dice1 + dice2) % 2 == 0 ? "丁" : "半")})");
                break;
            case GamblingGameType.Card:
                int currentCard = _random.Next(13) + 1;
                int nextCard = _random.Next(13) + 1;
                won = GamblingSystem.JudgeHighLow(playerGuess == 1, currentCard, nextCard);
                AddMessage($"🃏 ハイ＆ロー: {currentCard} → {nextCard}");
                break;
        }

        if (won)
        {
            float payout = GamblingSystem.GetPayoutMultiplier(gameType);
            float luckBonus = GamblingSystem.GetLuckBonus(Player.EffectiveStats.Luck);
            int winAmount = (int)(betAmount * payout * (1.0f + luckBonus));
            Player.AddGold(winAmount);
            AddMessage($"🎉 勝利！ {winAmount}G獲得！");
        }
        else
        {
            AddMessage($"😞 残念... {betAmount}G失った");
        }

        OnStateChanged?.Invoke();
        return true;
    }

    // === 釣り (FishingSystem) ===

    /// <summary>釣りを行う</summary>
    public bool TryFish()
    {
        var currentTime = TimeOfDaySystem.GetTimePeriod(GameTime.Hour);
        int fishingLevel = _proficiencySystem.GetLevel(ProficiencyCategory.Exploration);
        var availableFish = FishingSystem.GetAvailableFish(CurrentSeason, currentTime, fishingLevel);

        if (availableFish.Count == 0)
        {
            AddMessage("この時間帯・季節では魚が釣れないようだ");
            return false;
        }

        // ジャンク判定
        float junkRate = FishingSystem.CalculateJunkRate(fishingLevel);
        if (_random.NextDouble() < junkRate)
        {
            AddMessage("🎣 ゴミを釣り上げた...");
            _proficiencySystem.GainExperience(ProficiencyCategory.Exploration, 1);
            TurnCount += 30;
            GameTime.AdvanceTurn(30);
            OnStateChanged?.Invoke();
            return true;
        }

        // 宝判定
        float treasureRate = FishingSystem.CalculateTreasureRate(fishingLevel, Player.EffectiveStats.Luck * 0.01f);
        if (_random.NextDouble() < treasureRate)
        {
            AddMessage("🎣 ✨ 宝箱を釣り上げた！");
            _proficiencySystem.GainExperience(ProficiencyCategory.Exploration, 5);
            TurnCount += 30;
            GameTime.AdvanceTurn(30);
            OnStateChanged?.Invoke();
            return true;
        }

        // 魚釣り
        var fish = availableFish[_random.Next(availableFish.Count)];
        float catchRate = FishingSystem.CalculateCatchRate(fish.Rarity, fishingLevel, Player.EffectiveStats.Luck * 0.01f);
        if (_random.NextDouble() < catchRate)
        {
            AddMessage($"🎣 {fish.Name}を釣り上げた！ (レア度{fish.Rarity})");
            _proficiencySystem.GainExperience(ProficiencyCategory.Exploration, fish.Rarity);
        }
        else
        {
            AddMessage("🎣 魚に逃げられた...");
            _proficiencySystem.GainExperience(ProficiencyCategory.Exploration, 1);
        }

        TurnCount += 30;
        GameTime.AdvanceTurn(30);
        OnStateChanged?.Invoke();
        return true;
    }

    // === 採集 (GatheringSystem) ===

    /// <summary>採集を行う</summary>
    public bool TryGather(GatheringType gatheringType)
    {
        int profLevel = _proficiencySystem.GetLevel(ProficiencyCategory.Mining);

        if (!GatheringSystem.CanGather(gatheringType, profLevel))
        {
            AddMessage("このタイプの採集にはもっと経験が必要だ");
            return false;
        }

        float successRate = GatheringSystem.CalculateSuccessRate(gatheringType, profLevel, CurrentSeason);
        int duration = GatheringSystem.CalculateGatheringDuration(gatheringType, profLevel);

        if (_random.NextDouble() < successRate)
        {
            float rareChance = GatheringSystem.CalculateRareItemChance(profLevel, Player.EffectiveStats.Luck * 0.01f);
            bool isRare = _random.NextDouble() < rareChance;
            var node = GatheringSystem.GetNode(gatheringType);
            string nodeName = node?.Name ?? gatheringType.ToString();
            AddMessage($"🌿 {nodeName}から{(isRare ? "レアな" : "")}素材を採集した！");
            _proficiencySystem.GainExperience(ProficiencyCategory.Mining, isRare ? 5 : 2);
        }
        else
        {
            AddMessage("🌿 採集に失敗した...");
            _proficiencySystem.GainExperience(ProficiencyCategory.Mining, 1);
        }

        TurnCount += duration;
        GameTime.AdvanceTurn(duration);
        OnStateChanged?.Invoke();
        return true;
    }

    // === 鍛冶 (SmithingSystem) ===

    /// <summary>武器強化</summary>
    public bool TrySmithEnhance(string itemName, int currentEnhance)
    {
        var result = _smithingSystem.Enhance(Player, itemName, currentEnhance);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>装備修理</summary>
    public bool TrySmithRepair(string itemName, int durabilityLost)
    {
        var result = _smithingSystem.Repair(Player, itemName, durabilityLost);
        AddMessage(result.Message);

        // 自己修理判定（DurabilitySystem）
        int smithingLevel = _proficiencySystem.GetLevel(ProficiencyCategory.Smithing);
        if (DurabilitySystem.CanSelfRepair(smithingLevel))
        {
            int repairAmount = DurabilitySystem.CalculateSelfRepairAmount(smithingLevel);
            AddMessage($"🔧 鍛冶スキルにより追加修理: +{repairAmount}");
        }

        OnStateChanged?.Invoke();
        return result.Success;
    }

    // === エンチャント (EnchantmentSystem) ===

    /// <summary>装備にエンチャント</summary>
    public bool TryEnchant(EquipmentItem item, EnchantmentType enchantType, SoulGemQuality gem)
    {
        if (!EnchantmentSystem.CanEnchant(item, enchantType, gem))
        {
            AddMessage("このエンチャントはこの装備には適用できない");
            return false;
        }

        var result = EnchantmentSystem.Enchant(item, enchantType, gem, _random);
        AddMessage(result.Message);
        if (result.Success)
        {
            AddMessage($"✨ {EnchantmentSystem.GetEnchantmentInfo(enchantType)?.Name ?? enchantType.ToString()}のエンチャント成功！");
        }
        OnStateChanged?.Invoke();
        return result.Success;
    }

    // === ショートカット (DungeonShortcutSystem) ===

    /// <summary>ダンジョンショートカット解放</summary>
    public bool TryUnlockShortcut()
    {
        string dungeonId = _currentMapName;
        int floor = CurrentFloor;
        int targetFloor = Math.Max(1, floor - 5);

        if (_dungeonShortcutSystem.IsUnlocked(dungeonId, floor, targetFloor))
        {
            AddMessage("すでにショートカットが解放されている");
            return false;
        }

        if (_dungeonShortcutSystem.UnlockShortcut(dungeonId, targetFloor, floor))
        {
            AddMessage($"🚪 ショートカット解放！ {targetFloor}階 ↔ {floor}階");
            OnStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>ショートカット一覧を取得</summary>
    public IReadOnlyList<(int FromFloor, int ToFloor)> GetDungeonShortcuts()
    {
        return _dungeonShortcutSystem.GetShortcuts(_currentMapName);
    }

    // === 密輸 (SmugglingSystem) ===

    /// <summary>密輸を試みる</summary>
    public bool TrySmuggle(string itemId)
    {
        var contrabands = SmugglingSystem.GetAllContrabands();
        if (contrabands.Count == 0)
        {
            AddMessage("密輸品がない");
            return false;
        }

        var contraband = contrabands[_random.Next(contrabands.Count)];
        float detectionChance = 0.3f;
        bool evaded = SmugglingSystem.CheckEvasion(detectionChance, Player.EffectiveStats.Dexterity, _random.NextDouble());

        if (!evaded)
        {
            int penalty = SmugglingSystem.GetPenalty(contraband.Type);
            Player.AddGold(-Math.Min(penalty, Player.Gold));
            _karmaSystem.ModifyKarma(-5, "密輸");
            AddMessage($"🚨 密輸が発覚！ 罰金{penalty}G、カルマ低下");
        }
        else
        {
            int profit = SmugglingSystem.CalculateProfit(contraband.Type);
            Player.AddGold(profit);
            AddMessage($"🤫 密輸成功！ {profit}G獲得");
        }

        OnStateChanged?.Invoke();
        return true;
    }

    // === 罠作成 (TrapCraftingSystem) ===

    /// <summary>罠を作成して設置する</summary>
    public bool TryCraftTrap(PlayerTrapType trapType)
    {
        var recipe = TrapCraftingSystem.GetRecipe(trapType);
        if (recipe == null)
        {
            AddMessage("その罠は作成できない");
            return false;
        }

        int smithingLevel = _proficiencySystem.GetLevel(ProficiencyCategory.Smithing);
        if (!TrapCraftingSystem.CanCraft(trapType, Player.Gold, smithingLevel))
        {
            AddMessage($"素材または鍛冶レベルが足りない（必要Lv: {recipe.RequiredSmithing}）");
            return false;
        }

        Player.AddGold(-recipe.MaterialCost);
        float efficiency = TrapCraftingSystem.CalculateEfficiency(trapType, smithingLevel);
        Map.SetTile(Player.Position.X, Player.Position.Y, TileType.TrapHidden);
        _proficiencySystem.GainExperience(ProficiencyCategory.Smithing, 3);
        AddMessage($"🪤 {recipe.Name}を設置した！ (効率{efficiency:P0})");
        OnStateChanged?.Invoke();
        return true;
    }

    // === 闇市場 (BlackMarketSystem) ===

    /// <summary>闇市場のアイテムを取得</summary>
    public IReadOnlyList<BlackMarketSystem.BlackMarketItem> GetBlackMarketItems()
    {
        return BlackMarketSystem.GetAvailableItems(_karmaSystem.KarmaValue);
    }

    /// <summary>闇市場にアクセス可能か</summary>
    public bool CanAccessBlackMarket() => BlackMarketSystem.CanAccess(_karmaSystem.KarmaValue);

    /// <summary>闇市場で購入</summary>
    public bool TryBlackMarketBuy(BlackMarketSystem.BlackMarketItem item)
    {
        if (Player.Gold < item.Price)
        {
            AddMessage("ゴールドが足りない");
            return false;
        }

        Player.AddGold(-item.Price);
        _karmaSystem.ModifyKarma(-2, "闇市場");
        AddMessage($"🏴 闇市場: {item.Name}を{item.Price}Gで購入（カルマ低下）");
        OnStateChanged?.Invoke();
        return true;
    }

    // === パズル (EnvironmentalPuzzleSystem) ===

    /// <summary>パズルに挑戦</summary>
    public bool TryAttemptPuzzle(PuzzleType puzzleType)
    {
        int intelligence = Player.EffectiveStats.Intelligence;
        if (!EnvironmentalPuzzleSystem.CanAttempt(puzzleType, intelligence, Player.Level))
        {
            AddMessage($"この{EnvironmentalPuzzleSystem.GetTypeName(puzzleType)}は今の知力では解けそうにない");
            return false;
        }

        var puzzles = EnvironmentalPuzzleSystem.GetByType(puzzleType);
        if (puzzles.Count == 0) return false;
        var puzzle = puzzles[_random.Next(puzzles.Count)];

        float successRate = EnvironmentalPuzzleSystem.CalculateSuccessRate(puzzle.Difficulty, intelligence);
        if (_random.NextDouble() < successRate)
        {
            AddMessage($"🧩 {puzzle.Name}を解いた！ {puzzle.RewardDescription}");
            Player.GainExperience(puzzle.Difficulty * 5);
        }
        else
        {
            AddMessage($"🧩 {puzzle.Name}を解けなかった...");
        }

        TurnCount += 10;
        GameTime.AdvanceTurn(10);
        OnStateChanged?.Invoke();
        return true;
    }

    // === 新システムプロパティアクセス ===

    /// <summary>疲労レベル</summary>
    public FatigueLevel PlayerFatigueLevel => _playerFatigue;

    /// <summary>疲労名</summary>
    public string PlayerFatigueName => BodyConditionSystem.GetFatigueName(_playerFatigue);

    /// <summary>衛生レベル</summary>
    public HygieneLevel PlayerHygieneLevel => _playerHygiene;

    /// <summary>衛生名</summary>
    public string PlayerHygieneName => BodyConditionSystem.GetHygieneName(_playerHygiene);

    /// <summary>罹患中の病気</summary>
    public DiseaseType? PlayerDisease => _playerDisease;

    /// <summary>病気名（罹患中のみ）</summary>
    public string? PlayerDiseaseName => _playerDisease.HasValue
        ? DiseaseSystem.GetDisease(_playerDisease.Value)?.Name : null;

    /// <summary>現在の時間帯</summary>
    public TimePeriod CurrentTimePeriod => TimeOfDaySystem.GetTimePeriod(GameTime.Hour);

    /// <summary>時間帯名</summary>
    public string CurrentTimePeriodName => TimeOfDaySystem.GetTimePeriodName(CurrentTimePeriod);

    /// <summary>熟練度システム</summary>
    public ProficiencySystem GetProficiencySystem() => _proficiencySystem;

    /// <summary>鍛冶システム</summary>
    public SmithingSystem GetSmithingSystem() => _smithingSystem;

    /// <summary>ショートカットシステム</summary>
    public DungeonShortcutSystem GetDungeonShortcutSystem() => _dungeonShortcutSystem;

    /// <summary>実績システム</summary>
    public AchievementSystem GetAchievementSystem() => _achievementSystem;

    /// <summary>アイテム等級情報を取得（ItemGradeSystem）</summary>
    public static string GetItemGradePrefix(ItemGrade grade) => ItemGradeSystem.GetGradeDisplayPrefix(grade);

    /// <summary>秘密の部屋検索チャンスを取得（SecretRoomSystem）</summary>
    public float GetSecretRoomSearchChance()
    {
        return SecretRoomSystem.CalculateSearchChance(Player.EffectiveStats.Perception,
            _proficiencySystem.GetLevel(ProficiencyCategory.Exploration));
    }

    /// <summary>マルチエンディング判定（MultiEndingSystem）</summary>
    public MultiEndingSystem.EndingResult DetermineEnding()
    {
        return MultiEndingSystem.DetermineEnding(
            _hasCleared, TotalDeaths, _karmaSystem.KarmaValue,
            false, _clearRank);
    }

    /// <summary>エンディングタイプ名取得</summary>
    public static string GetEndingTypeName(EndingType ending) => MultiEndingSystem.GetEndingTypeName(ending);

    /// <summary>レベルアップ時の成長ボーナスを取得（GrowthSystem）</summary>
    public StatModifier GetLevelUpBonus()
    {
        return GrowthSystem.CalculateLevelUpBonus(Player.Race, Player.CharacterClass, Player.Level);
    }

    /// <summary>アクセシビリティ設定（AccessibilitySystem）</summary>
    private readonly AccessibilitySystem _accessibilitySystem = new();

    /// <summary>アクセシビリティ設定を取得</summary>
    public AccessibilitySystem GetAccessibilitySystem() => _accessibilitySystem;

    /// <summary>コンテキストヘルプシステム（ContextHelpSystem）</summary>
    private readonly ContextHelpSystem _contextHelpSystem = new();

    /// <summary>コンテキストヘルプを取得</summary>
    public ContextHelpSystem GetContextHelpSystem() => _contextHelpSystem;

    /// <summary>宗教スキルを取得（ReligionSkillSystem）</summary>
    public IReadOnlyList<ReligionSkillBonus> GetReligionSkills()
    {
        var religion = Player.CurrentReligion;
        if (religion == null) return Array.Empty<ReligionSkillBonus>();
        if (Enum.TryParse<ReligionId>(religion, out var religionId))
        {
            return ReligionSkillSystem.GetGrantedSkillBonuses(religionId, FaithRank.Devout);
        }
        return Array.Empty<ReligionSkillBonus>();
    }

    /// <summary>モンスター種族特性を取得（MonsterRaceSystem）</summary>
    public static MonsterRaceTraits GetMonsterTraits(MonsterRace race)
    {
        return MonsterRaceSystem.GetTraits(race);
    }

    /// <summary>ダンジョン派閥の敵対関係確認（DungeonFactionSystem）</summary>
    public static bool AreFactionHostile(MonsterRace race1, MonsterRace race2)
    {
        return DungeonFactionSystem.AreHostile(race1, race2);
    }

    /// <summary>NPC行動スケジュール確認（NpcRoutineSystem）</summary>
    public bool IsNpcAvailableNow(string npcType)
    {
        return NpcRoutineSystem.IsNpcAvailable(npcType, CurrentTimePeriod);
    }

    /// <summary>拡張ステータス効果を取得（ExtendedStatusEffectSystem）</summary>
    public static IReadOnlyList<ExtendedStatusEffect> GetActiveBuffs()
    {
        return ExtendedStatusEffectSystem.GetBuffs();
    }

    /// <summary>ModularHudSystemの要素情報取得</summary>
    private readonly ModularHudSystem _modularHudSystem = new();

    /// <summary>ModularHudSystem取得</summary>
    public ModularHudSystem GetModularHudSystem() => _modularHudSystem;

    /// <summary>RenderOptimizationSystemの視界最適化計算</summary>
    public static (int MinX, int MinY, int MaxX, int MaxY) CalculateRenderViewport(
        int playerX, int playerY, int viewportWidth, int viewportHeight)
    {
        return RenderOptimizationSystem.CalculateViewport(playerX, playerY, viewportWidth, viewportHeight);
    }

    /// <summary>テンプレートマップ一覧取得（TemplateMapSystem）</summary>
    public static IReadOnlyList<TemplateMapSystem.TemplateDefinition> GetAvailableMapTemplates()
    {
        return TemplateMapSystem.GetAllTemplates();
    }

    /// <summary>シンボルマップイベント情報取得（SymbolMapEventSystem）</summary>
    public IReadOnlyList<SymbolMapEventSystem.MapEvent> GetMapEvents()
    {
        return SymbolMapEventSystem.GetAvailableEvents(CurrentSeason, _worldMapSystem.CurrentTerritory);
    }

    /// <summary>自動探索の停止条件チェック（AutoExploreSystem）</summary>
    public AutoExploreSystem.StopReason? CheckAutoExploreStop()
    {
        bool hasEnemyNearby = IsInCombat();
        float hpRatio = (float)Player.CurrentHp / Player.MaxHp;
        float hungerRatio = (float)Player.Hunger / 100f;
        bool itemOnTile = GroundItems.Any(i => i.Position == Player.Position);
        var tile = Map.GetTile(Player.Position);
        bool stairsNearby = tile.Type is TileType.StairsDown or TileType.StairsUp;
        return AutoExploreSystem.CheckStopConditions(true, hasEnemyNearby, itemOnTile, stairsNearby, false, hpRatio, hungerRatio);
    }

    /// <summary>フラグ条件評価（FlagConditionSystem）</summary>
    public bool EvaluateFlag(string conditionText)
    {
        var condition = FlagConditionSystem.ParseCondition(conditionText);
        if (condition == null) return false;
        var context = new Dictionary<string, string>
        {
            { "level", Player.Level.ToString() },
            { "floor", CurrentFloor.ToString() },
            { "karma", _karmaSystem.KarmaValue.ToString() },
            { "gold", Player.Gold.ToString() }
        };
        return FlagConditionSystem.EvaluateCondition(condition, context);
    }

    /// <summary>ステータスフラグ評価（StatFlagSystem）</summary>
    public IReadOnlyList<StatFlagResult> GetActiveStatFlags()
    {
        var stats = new Dictionary<string, int>
        {
            { "str", Player.EffectiveStats.Strength },
            { "dex", Player.EffectiveStats.Dexterity },
            { "int", Player.EffectiveStats.Intelligence },
            { "vit", Player.EffectiveStats.Vitality },
            { "per", Player.EffectiveStats.Perception },
            { "cha", Player.EffectiveStats.Charisma },
            { "luk", Player.EffectiveStats.Luck }
        };
        return StatFlagSystem.EvaluateAll(stats);
    }

    #endregion
}

/// <summary>
/// ダンジョンフロアキャッシュ（マップ構造と生成時刻を保持）
/// </summary>
public record FloorCache(DungeonMap Map, int CreatedAtTurn, List<(Item Item, Position Position)> GroundItems);

public enum GameAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    MoveUpLeft,
    MoveUpRight,
    MoveDownLeft,
    MoveDownRight,
    Wait,
    Pickup,
    UseStairs,
    AscendStairs,
    OpenInventory,
    OpenStatus,
    OpenMessageLog,
    AutoExplore,
    Search,
    CloseDoor,
    RangedAttack,
    ThrowItem,
    UseSkill,
    StartCasting,
    CastSpell,
    CancelCasting,
    Pray,
    JoinReligion,
    LeaveReligion,
    TravelToTerritory,
    EnterTown,
    LeaveTown,
    UseInn,
    VisitChurch,
    VisitBank,
    EnterShop,
    OpenWorldMap,
    TalkToNpc,
    AcceptQuest,
    TurnInQuest,
    RegisterGuild,
    ViewQuestLog,
    AdvanceDialogue,
    OpenCrafting,
    CraftItem,
    EnhanceEquipment,
    EnchantWeapon,
    Save,
    Load,
    Quit,

    // Ver.prt.0.4 新画面アクション
    OpenEncyclopedia,
    OpenDeathLog,
    OpenSkillTree,
    OpenCompanion,
    OpenCooking,
    OpenBaseConstruction
}
