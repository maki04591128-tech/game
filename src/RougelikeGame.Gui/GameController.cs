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

    /// <summary>引き継ぎデータ（死に戻り用）</summary>
    private TransferData? _transferData;

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
    public event Action<string>? OnQuestUpdated;
    public event Action<GuildRank>? OnGuildRankUp;
    public event Action? OnShowCrafting;
    public event Action<CraftingResult>? OnCraftingResult;
    public event Action<EnhancementResult>? OnEnhancementResult;
    public event Action<EnchantmentResult>? OnEnchantmentResult;
    public event Action<TutorialStep>? OnShowTutorial;

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

        // マップ生成
        GenerateFloor();

        var mapDisplayName = StartingMapResolver.GetDisplayName(_currentMapName);
        AddMessage($"{mapDisplayName} ─ ダンジョン第{CurrentFloor}層に入った！");
        AddMessage("WASD/矢印で移動、敵に体当たりで攻撃");
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

    private void GenerateFloor()
    {
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
        StatModifier? bonus = floorBonus > 0
            ? new StatModifier(
                Strength: floorBonus,
                Vitality: floorBonus,
                Agility: floorBonus / 2,
                Dexterity: floorBonus / 2)
            : null;

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
                var item = _itemFactory.GenerateRandomItem(CurrentFloor);
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

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"{enemy.Name}に{result.Damage?.Amount ?? 0}ダメージ！{critStr}");

            if (!enemy.IsAlive)
            {
                // ゴールドドロップ（階層 × 5〜15 × 難易度倍率）
                int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                int gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));
                Player.AddGold(gold);

                AddMessage($"{enemy.Name}を倒した！経験値+{enemy.ExperienceReward} 💰+{gold}G");
                Player.GainExperience(enemy.ExperienceReward);
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
            var item = _itemFactory.GenerateRandomItem(CurrentFloor);
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
    }

    private void ProcessEnemyTurns()
    {
        // デバッグモードでAI非活性化中は敵のターンをスキップ
        if (_isDebugMode && !_debugAIActive) return;

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
        var result = _combatSystem.ExecuteAttack(enemy, Player, AttackType.Slash);

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"{enemy.Name}の攻撃！{result.Damage?.Amount ?? 0}ダメージ！{critStr}");
            _lastDamageCause = DeathCause.Combat;
        }
        else
        {
            AddMessage($"{enemy.Name}の攻撃は外れた");
        }
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

            GroundItems.Remove(itemOnGround);
            inventory.Add(itemOnGround.Item);
            AddMessage($"{itemOnGround.Item.GetDisplayName()}を拾った（{inventory.TotalWeight:F1}/{Player.CalculateMaxWeight():F1}kg）");
            return true;
        }
        else
        {
            AddMessage("ここには何もない");
            return false;
        }
    }

    private bool TryDescendStairs()
    {
        var tile = Map.GetTile(Player.Position);
        if (tile.Type == TileType.StairsDown)
        {
            if (CurrentFloor >= GameConstants.MaxDungeonFloor)
            {
                // 最深部到達 → ダンジョンクリアフラグ
                _clearSystem.SetFlag("dungeon_clear");
                AddMessage("🏆 ダンジョン最深部に到達した！");
                return false;
            }

            CurrentFloor++;
            GenerateFloor();
            AddMessage($"第{CurrentFloor}層に降りた");

            // ボスフロア通知
            if (CurrentFloor % GameConstants.BossFloorInterval == 0)
            {
                AddMessage("⚠ 強大な気配を感じる...ボスフロアだ！");
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

        if (CurrentFloor <= 1)
        {
            // 1層目の上り階段 → 地上帰還
            AddMessage("ダンジョンから脱出した！ 地上に帰還する...");
            IsRunning = false;
            OnGameOver?.Invoke();
            return true;
        }
        else
        {
            // 上の階へ移動
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
    /// 死に戻り（リバース）を実行する
    /// </summary>
    private void ExecuteRebirth(TransferData transfer)
    {
        _transferData = transfer;
        var race = Player.Race;
        var charClass = Player.CharacterClass;
        var background = Player.Background;

        // プレイヤーを再作成（肉体リセット）
        Player = Player.Create(Player.Name, race, charClass, background);

        // 引き継ぎデータを適用（知識系）
        Player.ApplyTransferData(transfer);

        // イベント再購読
        SubscribePlayerEvents();

        // 初期装備を支給
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)Player.Inventory).Add(sword);
        ((Inventory)Player.Inventory).Add(armor);
        Player.Equipment.Equip(sword, Player);
        Player.Equipment.Equip(armor, Player);
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateBread());

        // STRベースの最大重量を更新
        Player.UpdateMaxWeight();

        // フロア1から再開
        CurrentFloor = 1;
        TurnCount = 0;
        GameTime.SetTotalTurns(0);
        _turnLimitExtended = false;
        _turnLimitRemoved = false;
        _lastTurnLimitWarningStage = 0;
        Enemies.Clear();
        GroundItems.Clear();
        GenerateFloor();

        AddMessage($"\n━━ 死に戻り ({TotalDeaths}回目) ━━");
        AddMessage($"ダンジョン第1層に戻った。正気度: {Player.Sanity}");
        AddMessage($"経験と装備は失われたが、知識は残っている...");

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
                int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                int gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));
                Player.AddGold(gold);
                AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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
                int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
                int gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier));
                Player.AddGold(gold);
                AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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
                    int gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                    Player.AddGold(gold);
                    AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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
                        int gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                        Player.AddGold(gold);
                        AddMessage($"  {target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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
                    int gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                    Player.AddGold(gold);
                    AddMessage($"{target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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
                        int gold = Math.Max(1, (int)(CurrentFloor * 10 * DifficultyConfig.GoldMultiplier));
                        Player.AddGold(gold);
                        AddMessage($"  {target.Name}を倒した！経験値+{target.ExperienceReward} 💰+{gold}G");
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

        // 成功時: 効果を解決して適用
        AddMessage(result.Message);
        actionCost = result.TurnCost;

        var effect = SpellEffectResolver.Resolve(result);
        if (!effect.IsNone)
        {
            ApplySpellEffect(effect);
        }

        return true;
    }

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
                AddMessage("姿を隠した");
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
                AddMessage("空間が歪み、別の場所に転送された");
                break;
            case SpellEffectType.Summon:
                AddMessage("召喚の魔法が発動した…（未実装）");
                break;
            case SpellEffectType.Copy:
                AddMessage("複写の魔法が発動した…（未実装）");
                break;
            case SpellEffectType.Reverse:
                AddMessage("反転の魔法が発動した…（未実装）");
                break;
            case SpellEffectType.Seal:
                AddMessage("封印の魔法が発動した…（未実装）");
                break;
            case SpellEffectType.Resurrect:
                AddMessage("蘇生の魔法が発動した…（未実装）");
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
        string buffType = effect.Type switch
        {
            SpellEffectType.Buff => "能力強化",
            SpellEffectType.Speed => "加速",
            SpellEffectType.Blessing => "祝福",
            _ => "強化"
        };
        AddMessage($"{buffType}の魔法が発動した（{effect.Duration}ターン）");
    }

    /// <summary>魔法制御の適用</summary>
    private void ApplySpellControl(SpellEffect effect)
    {
        switch (effect.TargetType)
        {
            case SpellTargetType.SingleEnemy:
            case SpellTargetType.Forward:
                var target = GetNearestEnemy();
                if (target != null)
                {
                    AddMessage($"{target.Name}に制御魔法が効いた");
                }
                else
                {
                    AddMessage("対象が見つからない");
                }
                break;
            case SpellTargetType.AllEnemies:
                var targets = GetEnemiesInRange(effect.Range);
                AddMessage($"{targets.Count}体の敵に制御魔法が効いた");
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

            OnTerritoryChanged?.Invoke(destination);
            OnStateChanged?.Invoke();
        }

        return result.Success;
    }

    /// <summary>街に入る</summary>
    private bool TryEnterTown()
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでにダンジョン内にいる");
            return false;
        }

        AddMessage($"{_worldMapSystem.GetCurrentTerritoryInfo().Name}の街に入った");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>街を出る</summary>
    private bool TryLeaveTown()
    {
        AddMessage("街を出た");
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
        var result = _shopSystem.Buy(Player, shopType, index, discount);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>ショップでアイテム売却</summary>
    public bool TrySellItem(string itemName, int baseValue)
    {
        double charismaBonus = ShopSystem.CalculateCharismaDiscount(Player.EffectiveStats.Charisma);
        var result = _shopSystem.Sell(Player, itemName, baseValue, charismaBonus);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>ダンジョンに入る（地上→地下）</summary>
    public bool TryEnterDungeon()
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでにダンジョン内にいる");
            return false;
        }

        _worldMapSystem.IsOnSurface = false;
        CurrentFloor = 1;
        GenerateFloor();
        AddMessage("ダンジョンに足を踏み入れた...");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>ダンジョンから脱出（地下→地上）</summary>
    public bool TryExitDungeon()
    {
        if (_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでに地上にいる");
            return false;
        }

        _worldMapSystem.IsOnSurface = true;
        AddMessage($"{_worldMapSystem.GetCurrentTerritoryInfo().Name}の地上に戻った");
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
            if (Map.IsInBounds(neighbor) && !Map.GetTile(neighbor).BlocksMovement && !IsOccupied(neighbor))
            {
                visited.Add(neighbor);
                queue.Enqueue((neighbor, neighbor));
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
                if (Map.GetTile(next).BlocksMovement) continue;

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

    private void AddMessage(string message)
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
}

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
    Quit
}
