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
    private readonly RandomEventSystem _randomEventSystem = new();  // BJ-1
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

    // === 未反映システム統合 ===
    private readonly MultiSlotSaveSystem _multiSlotSaveSystem = new();
    private readonly ModLoaderSystem _modLoaderSystem = new();

    /// <summary>プレイヤーの向き（攻撃方向判定用）</summary>
    private Direction _playerFacing = Direction.South;

    /// <summary>現在のダンジョン特徴タイプ</summary>
    private DungeonFeatureType? _currentDungeonFeature;

    /// <summary>現在のダンジョンの推奨レベル（ランダムダンジョン難易度連携用）</summary>
    private int _currentDungeonMinLevel;

    /// <summary>現在のダンジョンの最大階層数（null=デフォルトMaxDungeonFloor使用）</summary>
    private int? _currentDungeonMaxFloor;

    /// <summary>現在の環境音タイプ</summary>
    private AmbientSoundType _currentAmbientSound = AmbientSoundType.Silence;

    /// <summary>地表面状態マップ（EnvironmentalCombatSystem）- 座標→地表面タイプ</summary>
    private readonly Dictionary<Position, EnvironmentalCombatSystem.SurfaceType> _surfaceMap = new();

    /// <summary>プレイヤーの現在の戦闘スタンス</summary>
    private CombatStance _playerStance = CombatStance.Balanced;

    /// <summary>プレイヤーの罹患中の病気（null=健康）</summary>
    private DiseaseType? _playerDisease;

    /// <summary>病気の残りターン数</summary>
    private int _diseaseRemainingTurns;

    /// <summary>敵がアクティブになる描画範囲半径</summary>
    private const int ActiveRange = 10;

    /// <summary>最後のダメージ原因（死亡判定用）</summary>
    private DeathCause _lastDamageCause = DeathCause.Unknown;

    /// <summary>満腹度が最後に減少したターン</summary>
    private long _lastHungerDecayTurn = 0;

    /// <summary>渇き度が最後に減少したターン</summary>
    private long _lastThirstDecayTurn = 0;

    /// <summary>自動探索中かどうか</summary>
    private bool _autoExploring = false;

    /// <summary>自動探索: 上り階段を目標としているかどうか</summary>
    private bool _autoExploreTargetStairsUp = false;

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
    private bool _hasSlimeSplit = false; // Y-1: スライム分裂フラグ（1回のみ）

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

    /// <summary>ロケーションマップがフィールド（敵あり・FOV必要）かどうか</summary>
    private bool _isLocationField;

    /// <summary>フィールドマップからシンボルマップへ帰還する際の復帰位置</summary>
    private Position? _symbolMapReturnPosition;

    /// <summary>建物内部マップに入る前の町マップとプレイヤー位置を保存</summary>
    private DungeonMap? _buildingReturnMap;
    private Position? _buildingReturnPosition;
    private string? _currentBuildingId;

    /// <summary>現在の町で訪問済みの建物IDセット（建物間移動に使用）</summary>
    private readonly HashSet<string> _visitedBuildings = new();

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

    /// <summary>リアルタイムターン消費の開始時刻</summary>
    private DateTime? _realTimeTurnStart;

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
    public event Action? OnPermaDeathSaveDelete;  // BC-1: Ironman死亡時セーブ削除通知
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
    public event Action? OnShowVocabulary;

    /// <summary>シンボルマップでのロケーション到着通知</summary>
    public event Action<LocationDefinition>? OnLocationArrived;

    /// <summary>シンボルマップでの街入場要求</summary>
    public event Action? OnSymbolMapEnterTown;

    /// <summary>シンボルマップでのダンジョン入場要求</summary>
    public event Action<LocationDefinition>? OnSymbolMapEnterDungeon;

    /// <summary>ゲームクリアイベント</summary>
    public event Action<GameClearSystem.ClearScore>? OnGameClear;

    /// <summary>フロア変更イベント（β.13 場面転換演出用）</summary>
    public event Action<int>? OnFloorChanged;

    /// <summary>プレイヤー死亡イベント（β.20 死に戻り視覚演出用）</summary>
    public event Action<string>? OnPlayerDied;

    /// <summary>プレイヤー転生イベント（β.20 死に戻り視覚演出用）</summary>
    public event Action<int>? OnPlayerRebirthed;

    /// <summary>戦闘ダメージ表示イベント（β.10 属性エフェクト用）引数: 対象位置, ダメージ量, 属性, クリティカル</summary>
    public event Action<Position, int, Element, bool>? OnCombatDamageDealt;

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
        _achievementSystem.RegisterDefaults();
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

        // プレイヤー作成（Player.Create内で初期ゴールド付与済み）
        Player = Player.Create(playerName, race, characterClass, background);

        // 環境音を初期化（AmbientSoundSystem）
        _currentAmbientSound = AmbientSoundSystem.GetAmbientForTerritory(startTerritory);

        // プレイヤーイベント購読
        SubscribePlayerEvents();

        // 初期装備
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)Player.Inventory).Add(sword);
        ((Inventory)Player.Inventory).Add(armor);
        Player.Equipment.Equip(sword, Player);
        Player.Equipment.Equip(armor, Player);
        // 装備スロットに移したアイテムをインベントリから除去（重複防止）
        ((Inventory)Player.Inventory).Remove(sword);
        ((Inventory)Player.Inventory).Remove(armor);

        // 初期アイテム
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)Player.Inventory).Add(ItemFactory.CreateBread());

        // Z-1: 素性固有の初期装備を適用
        var bgBonus = BackgroundBonusData.Get(background);
        foreach (var itemId in bgBonus.InitialItemIds)
        {
            var bgItem = ItemDefinitions.Create(itemId);
            if (bgItem != null)
            {
                ((Inventory)Player.Inventory).Add(bgItem);
            }
        }

        // STRベースの最大重量を更新
        Player.UpdateMaxWeight();

        // メインクエスト登録・受注
        _questSystem.RegisterMainQuest();
        _questSystem.AcceptQuest("main_quest_abyss", Player.Level, GuildRank.None);

        // CE-2: ヘルプシステムのデフォルトトピック登録
        _contextHelpSystem.RegisterDefaultTopics();

        // DungeonEcosystemSystem: 捕食関係の初期化
        InitializeEcosystemRelations();

        // TerritoryInfluenceSystem: 開始領地の勢力初期化
        _territoryInfluenceSystem.Initialize(startTerritory, new Dictionary<string, float>
        {
            ["冒険者ギルド"] = 0.4f,
            ["商人連合"] = 0.3f,
            ["王国軍"] = 0.3f
        });

        // マップ生成（シンボルマップから開始）
        GenerateSymbolMap();

        var mapDisplayName = StartingMapResolver.GetDisplayName(_currentMapName);
        var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
        AddMessage($"{territoryName}のシンボルマップに入った");
        AddMessage("WASD/矢印で移動、ロケーションに到着して>キーでダンジョン、Tキーで街に入る");

        // BA-1: NPC対話ノードの初期登録
        RegisterDefaultNpcDialogues();
    }

    /// <summary>BA-1: NPC固有の対話ツリーを登録</summary>
    private void RegisterDefaultNpcDialogues()
    {
        _dialogueSystem.RegisterNodes(new DialogueNode[]
        {
            // ===== レオン（冒険者ギルドマスター）=====
            new("dlg_leon_intro", "レオン", "やあ、冒険者。ギルドへようこそ。\n何か依頼を探しているなら、掲示板を確認してくれ。\nここは旅人の安全と繁栄を守る場所だ。",
                new[] { new DialogueChoice("クエストを見せてくれ", "action:show_quest_board", 5),
                        new DialogueChoice("ギルドについて教えてほしい", "dlg_leon_lore"),
                        new DialogueChoice("この街について教えてくれ", "dlg_leon_town") }),
            new("dlg_leon_lore", "レオン", "このギルドは冒険者を支援する組織だ。\n依頼を達成してポイントを貯めれば、ランクが上がるぞ。\n高いランクになれば、より報酬のいい依頼が解禁される。\n信頼と実績…それがギルドマンの誇りだ。",
                new[] { new DialogueChoice("ランクアップの条件は？", "dlg_leon_rank"),
                        new DialogueChoice("戻る", "dlg_leon_intro") }),
            new("dlg_leon_rank", "レオン", "ランクは銅・鉄・銀・金・白金の5段階だ。\n依頼をこなすほどポイントが貯まり、自動でランクアップする。\n白金ランクになれば…まあ、国王直属の特別依頼も来るようになるな。",
                new[] { new DialogueChoice("了解した", "dlg_leon_intro") }),
            new("dlg_leon_town", "レオン", "ここは王都の外れにある冒険者の街、スターターズ・クロスだ。\n昔は小さな宿場町だったが、今では各地からの冒険者が集う賑やかな拠点になった。\nダンジョンへの入口もすぐそこにある。気をつけてな。",
                new[] { new DialogueChoice("ダンジョンについて教えてくれ", "dlg_leon_dungeon"),
                        new DialogueChoice("了解した", "dlg_leon_intro") }),
            new("dlg_leon_dungeon", "レオン", "ダンジョンは地下深くに広がる謎の遺跡だ。\n行けば行くほど強力な魔物が現れるが、その分宝も多い。\nただし…30階より深くに潜った者の多くは戻らない。\n俺も昔は冒険者だったが、今は後輩たちを送り出す側だ。",
                new[] { new DialogueChoice("了解した", "dlg_leon_intro") }),

            // ===== マルコ（商人）=====
            new("dlg_marco_intro", "マルコ", "いらっしゃい！ 何かお探しかな？\n良い品を揃えているよ。遠い地から取り寄せた珍しいものもあるぞ。",
                new[] { new DialogueChoice("商品を見せてくれ", "action:open_shop", 3),
                        new DialogueChoice("商売について聞かせてくれ", "dlg_marco_business"),
                        new DialogueChoice("最近何か変わったことは？", "dlg_marco_news") }),
            new("dlg_marco_business", "マルコ", "商売は信頼と情報が命だ。\nどこに何が安くあるか、どこで何が高く売れるか…\nそれを知っているだけで、戦わずとも豊かになれる。\n君も何か売りたいものがあれば、遠慮なく言ってくれ。",
                new[] { new DialogueChoice("戻る", "dlg_marco_intro") }),
            new("dlg_marco_news", "マルコ", "そうだな…最近、南の港から変な噂が流れてきた。\n深海から怪しい品物を運び込む船があるらしい。\nまあ、商人としては気になる話だが、近づかない方が賢明かもしれない。",
                new[] { new DialogueChoice("商品を見せてくれ", "action:open_shop", 3),
                        new DialogueChoice("戻る", "dlg_marco_intro") }),

            // ===== アルバート（神官）=====
            new("dlg_albert_intro", "アルバート", "神の祝福がありますように。\n呪いを解いたり、祈りを捧げることができますよ。\n心が迷ったとき、神はいつでも導いてくださいます。",
                new[] { new DialogueChoice("呪いを解いてほしい", "action:remove_curse", 5),
                        new DialogueChoice("祈りを捧げたい", "action:pray"),
                        new DialogueChoice("この地の信仰について聞かせてくれ", "dlg_albert_faith") }),
            new("dlg_albert_faith", "アルバート", "この地では主に6つの神が信仰されています。\n光の神ルーミナス、大地の神テラース、嵐の神ガルウィン…\n死の神モルタス、知恵の神ソフィア、混沌の神カオシャン。\nどの神もそれぞれの真理を持ち、信者に力を与えます。",
                new[] { new DialogueChoice("各宗教について詳しく教えてくれ", "dlg_albert_religions"),
                        new DialogueChoice("了解した", "dlg_albert_intro") }),
            new("dlg_albert_religions", "アルバート", "光の神ルーミナスは治癒と守護を司ります。\n混沌の神カオシャンへの信仰は…私には理解し難いものがありますが、\n信仰の在り方は人それぞれ。どうか正しい道を歩んでください。",
                new[] { new DialogueChoice("了解した", "dlg_albert_intro") }),

            // ===== マーヴィン（魔法使い）=====
            new("dlg_mervin_intro", "マーヴィン", "ほう、魔術に興味があるのかね？\nルーン語を学べば強力な魔法が使えるようになるぞ。\n言葉には力が宿る。古代語のルーン文字は特にな。",
                new[] { new DialogueChoice("魔法の品を見せてくれ", "action:open_shop", 3),
                        new DialogueChoice("ルーン語について教えてくれ", "dlg_mervin_rune"),
                        new DialogueChoice("魔法の歴史について", "dlg_mervin_history") }),
            new("dlg_mervin_rune", "マーヴィン", "ルーン語は古代の魔術師が開発した呪文言語だ。\n各ルーン文字は宇宙の基本原理を表している。\n例えば『イグ』は火を、『フリス』は氷を意味する。\n複数のルーンを組み合わせることで、より複雑な魔法が使えるようになる。",
                new[] { new DialogueChoice("ルーン語を学ぶにはどうすれば？", "dlg_mervin_learn"),
                        new DialogueChoice("戻る", "dlg_mervin_intro") }),
            new("dlg_mervin_learn", "マーヴィン", "ダンジョン内の碑文を調べると、ルーン語の知識が増えることがある。\nまた、古代の書と呼ばれるアイテムには貴重なルーン知識が記されている。\n学ぶことに終わりはないが、それが魔術師の醍醐味というものだ。",
                new[] { new DialogueChoice("了解した", "dlg_mervin_intro") }),
            new("dlg_mervin_history", "マーヴィン", "魔術は3000年前の大賢者アルカナムが体系化したと言われている。\n彼は星の配置からルーン語の法則を発見し、多くの弟子を育てた。\n今日の全ての魔法学はその流れを汲んでいる。\n…ただし、その研究の末にアルカナムが何を見たのかは、今も謎のままだ。",
                new[] { new DialogueChoice("戻る", "dlg_mervin_intro") }),

            // ===== エルウェン（エルフの旅人）=====
            new("dlg_elwen_intro", "エルウェン", "…森は多くを語る。耳を澄ませば、真実が聞こえるだろう。\n人間の世界は賑やかだが、その分大切なものを見失いやすい。",
                new[] { new DialogueChoice("何か教えてくれ", "dlg_elwen_nature"),
                        new DialogueChoice("エルフについて教えてくれ", "dlg_elwen_elf"),
                        new DialogueChoice("森について教えてくれ", "dlg_elwen_forest") }),
            new("dlg_elwen_nature", "エルウェン", "自然は均衡を保っている。強すぎるものは必ず弱まり、\n弱すぎるものも必ず力を蓄える時がくる。\n生と死も同様だ。大切なのは、今この瞬間に何をするかだ。",
                new[] { new DialogueChoice("戻る", "dlg_elwen_intro") }),
            new("dlg_elwen_elf", "エルウェン", "エルフは森の奥深くで暮らす種族だ。長命ゆえに歴史を多く知っている。\n人間とは時として衝突することもあるが、共に生きる道も必ずある。\n私がここにいるのも、両種族の架け橋になりたいという思いからだ。",
                new[] { new DialogueChoice("戻る", "dlg_elwen_intro") }),
            new("dlg_elwen_forest", "エルウェン", "かつてこの地は深い森に覆われていた。\n今も東の方角に行けば、古代の森の残滓を見ることができるだろう。\nそこには忘れられた遺跡と、かつての文明の痕跡が残っている。",
                new[] { new DialogueChoice("戻る", "dlg_elwen_intro") }),

            // ===== リーナ（薬師）=====
            new("dlg_leena_intro", "リーナ", "薬草の調合なら任せて！\n素材があれば色々作れるわよ。特にポーションは得意！",
                new[] { new DialogueChoice("調合を頼みたい", "action:open_alchemy", 3),
                        new DialogueChoice("素材について教えて", "dlg_leena_herb"),
                        new DialogueChoice("薬師になった経緯は？", "dlg_leena_story") }),
            new("dlg_leena_herb", "リーナ", "薬草には様々な効果があるわ。\n赤いベリーは疲労回復に、青い花びらは毒消しに使える。\nダンジョンでも植物系のアイテムをよく見かけるから、\n捨てずに持ってきてくれると喜んで買い取るわよ。",
                new[] { new DialogueChoice("調合を頼みたい", "action:open_alchemy", 3),
                        new DialogueChoice("戻る", "dlg_leena_intro") }),
            new("dlg_leena_story", "リーナ", "幼い頃に弟が病に倒れてね。当時の薬師さんに命を救ってもらったの。\nそれ以来、自分も誰かの力になれる薬師になりたいって思って…。\n今はこうして冒険者の皆さんを助けられていて、嬉しいわ。",
                new[] { new DialogueChoice("戻る", "dlg_leena_intro") }),

            // ===== ガルド（依頼人の農夫）=====
            new("dlg_gard_intro", "ガルド", "おう、強そうな奴が来たな。\n森の奥で厄介な魔物が暴れてるんだ。退治してくれねえか？\n畑が荒らされて困ってるんだよ…。",
                new[] { new DialogueChoice("依頼を受ける", "action:accept_quest", 5),
                        new DialogueChoice("詳しく教えてくれ", "dlg_gard_detail"),
                        new DialogueChoice("考えさせてくれ", "dlg_gard_intro") }),
            new("dlg_gard_detail", "ガルド", "3日前から夜中に変な声がするんだ。\n翌朝見てみると畑が荒らされてて、足跡が森に続いてる。\n俺みたいな農夫には手に負えねえが、冒険者なら何とかなるだろ？\n成功したら全財産とは言わんが、それなりに出すぞ。",
                new[] { new DialogueChoice("依頼を受ける", "action:accept_quest", 5),
                        new DialogueChoice("戻る", "dlg_gard_intro") }),

            // ===== ドワル（鍛冶師）=====
            new("dlg_dwal_intro", "ドワル", "鍛冶仕事なら俺に任せろ！\n良い鉱石があれば最高の武器を打ってやるぞ。\nドワーフの鍛冶は3000年の歴史がある。",
                new[] { new DialogueChoice("鍛冶を頼みたい", "action:open_smithing", 3),
                        new DialogueChoice("装備を見せてくれ", "action:open_shop", 3),
                        new DialogueChoice("鍛冶の技について教えてくれ", "dlg_dwal_craft") }),
            new("dlg_dwal_craft", "ドワル", "良い武器を作るには素材が全てだ。\n鉄は頑丈だが重い。ミスリルは軽くて丈夫だが希少だ。\n竜鱗なんかは最高の素材だが、そうそう手に入らん。\nどんな素材でも、腕のいい鍛冶師が打てば名品になる！",
                new[] { new DialogueChoice("戻る", "dlg_dwal_intro") }),

            // ===== ブロック（山岳ガイド）=====
            new("dlg_brock_intro", "ブロック", "山岳地帯は危険が多い。装備を整えてから挑んだ方がいいぞ。\n俺は長年この地を歩き回ってきたから、地形はよく知ってる。",
                new[] { new DialogueChoice("何か依頼はあるか", "action:show_quest_board", 3),
                        new DialogueChoice("山岳地帯の情報を教えてくれ", "dlg_brock_mountain") }),
            new("dlg_brock_mountain", "ブロック", "北の山脈は高度が上がるほど気温が下がる。\n冷気耐性のある装備がないと凍傷になるぞ。\nその代わり、高山にしか生息しない魔物がいて、稀少なドロップを落とす。\n用意ができたら、ガイドを頼んでくれ。",
                new[] { new DialogueChoice("了解した", "dlg_brock_intro") }),

            // ===== カリーナ（吟遊詩人）=====
            new("dlg_carina_intro", "カリーナ", "風の向くまま旅をしている者よ。面白い話なら聞くわ。\n私は世界中の物語を集めて歌にしているの。",
                new[] { new DialogueChoice("最近見聞きした話を教えて", "dlg_carina_news"),
                        new DialogueChoice("歌を聴かせてくれ", "dlg_carina_song"),
                        new DialogueChoice("旅について教えてくれ", "dlg_carina_travel") }),
            new("dlg_carina_news", "カリーナ", "ふふ、面白い話がひとつ。\n東の砂漠地帯で、古代王朝の秘密の蔵が発見されたらしいわ。\n中には莫大な財宝と…呪われた護衛の魔物がいるとか。\nまあ、噂の半分は盛ってあるものよ。でも半分は本当かもね。",
                new[] { new DialogueChoice("戻る", "dlg_carina_intro") }),
            new("dlg_carina_song", "カリーナ",
                "「嵐の海を越えて、英雄は旅立った\n星の導きに従い、暗黒の地へと向かった\n剣と知恵と勇気を持って、世界の真実を探し求めて…」\n\n…続きが気になる？ それは歌が終わってからのお楽しみよ。",
                new[] { new DialogueChoice("素晴らしい歌だ", "dlg_carina_intro", 5) }),
            new("dlg_carina_travel", "カリーナ", "私はこれまで6つの領地全てを旅した。\n北の凍土から南の熱帯雨林、東の砂漠から西の海辺まで。\nどこにも独自の文化と歴史があって…本当に飽きない世界よ。\nあなたも旅を続けていれば、きっと忘れられない景色に出会えるわ。",
                new[] { new DialogueChoice("了解した", "dlg_carina_intro") }),

            // ===== ミラ（宿屋主人）=====
            new("dlg_mira_intro", "ミラ", "お疲れ様！ ゆっくり休んでいって。食事も用意できるわよ。\nここは旅人の安らぎの場所よ。",
                new[] { new DialogueChoice("休憩したい", "action:rest", 3),
                        new DialogueChoice("食事を頼む", "action:open_cooking"),
                        new DialogueChoice("宿屋について聞かせてくれ", "dlg_mira_inn") }),
            new("dlg_mira_inn", "ミラ", "この宿屋は私の母から受け継いだの。もう30年以上営業しているわ。\n多くの冒険者を見送ってきたけど…無事に戻ってくれる人は半分くらいかしら。\nだから、帰ってきてくれた人を見ると本当に嬉しくなっちゃう。\nしっかり休んで、また元気に出発してね。",
                new[] { new DialogueChoice("休憩したい", "action:rest", 3),
                        new DialogueChoice("了解した", "dlg_mira_intro") }),

            // ===== トーマス（漁師）=====
            new("dlg_thomas_intro", "トーマス", "最近、海岸沿いで怪しい影を見たって噂があるんだ…。\n夜中に光る何かが海の底を泳いでいるらしい。",
                new[] { new DialogueChoice("詳しく聞かせてくれ", "dlg_thomas_detail"),
                        new DialogueChoice("海について教えてくれ", "dlg_thomas_sea") }),
            new("dlg_thomas_detail", "トーマス", "先週の夜、漁から戻る途中に見たんだ。\n海面に青白い光がいくつも浮かんでいて、ゆっくり動いてた。\n幽霊か海の怪物か分からないが…今月は夜の漁は控えるつもりだ。\nもし調べてくれるなら、何か礼をしたい。",
                new[] { new DialogueChoice("了解した。調べてみる", "dlg_thomas_intro", 5) }),
            new("dlg_thomas_sea", "トーマス", "この海は豊かだが、深いところには人知れず何かが住んでいる。\n大漁の年もあれば、全く取れない年もある。海は人間が思う以上に広くて深い。\n漁師の間では「深海には別の世界がある」という言い伝えもあるくらいだ。",
                new[] { new DialogueChoice("戻る", "dlg_thomas_intro") }),

            // ===== ハッサン（闇商人）=====
            new("dlg_hassan_intro", "ハッサン", "…表には出せない品もある。金さえあれば、何でも手に入るさ。\nただし、ここでの取引は秘密にしておいてくれ。",
                new[] { new DialogueChoice("品物を見せてくれ", "action:open_shop", 0),
                        new DialogueChoice("何が手に入るんだ？", "dlg_hassan_items"),
                        new DialogueChoice("やめておく", "dlg_hassan_intro") }),
            new("dlg_hassan_items", "ハッサン", "…王国で禁じられた薬、盗品の装備、呪われた遺物…\n何でも揃えられる。ただし、表の市場の3倍は覚悟してくれ。\n危険な商売だが、需要があるから成り立つんだ。",
                new[] { new DialogueChoice("品物を見せてくれ", "action:open_shop", 0),
                        new DialogueChoice("やめておく", "dlg_hassan_intro") }),

            // ===== サラ（遺跡研究者）=====
            new("dlg_sara_intro", "サラ", "古の知識を求めてここまで来たの。砂漠の遺跡には多くの秘密が眠っている…。\n失われた文明の痕跡を探し続けているわ。",
                new[] { new DialogueChoice("遺跡について教えて", "dlg_sara_ruin"),
                        new DialogueChoice("古代文明について教えてくれ", "dlg_sara_ancient"),
                        new DialogueChoice("研究について教えてくれ", "dlg_sara_research") }),
            new("dlg_sara_ruin", "サラ", "砂漠の遺跡は古代王朝アルカナス帝国の遺物よ。\n彼らは魔法と技術を融合させた文明を持っていたと言われているわ。\nただし、帝国がなぜ滅んだのか…それがまだ分かっていないの。",
                new[] { new DialogueChoice("戻る", "dlg_sara_intro") }),
            new("dlg_sara_ancient", "サラ", "アルカナス帝国は今から2000年前に栄えていた。\n彼らの建築技術は今でも解明できないものがある。\nルーン語の原型も彼らが作ったとされているわ。\n私はその謎を解き明かして、歴史に名を残したいの。",
                new[] { new DialogueChoice("戻る", "dlg_sara_intro") }),
            new("dlg_sara_research", "サラ", "私の専門は古代言語と遺物の解読よ。\nこの辺りのダンジョンで見つかる碑文には、古代の呪文が刻まれていることがある。\nそういうものを見つけたら、ぜひ知らせてほしいわ。謝礼はちゃんとするわよ。",
                new[] { new DialogueChoice("了解した", "dlg_sara_intro") }),

            // ===== ヴォルフ（傭兵）=====
            new("dlg_wolf_intro", "ヴォルフ", "辺境は弱い奴には生き残れねえ。それでもやるなら、覚悟を決めろ。\n俺は長年傭兵として戦ってきた。今は半引退してるが、腕は落とさない。",
                new[] { new DialogueChoice("仕事はあるか", "action:show_quest_board", 3),
                        new DialogueChoice("戦いについて教えてくれ", "dlg_wolf_battle"),
                        new DialogueChoice("辺境の危険について教えてくれ", "dlg_wolf_frontier") }),
            new("dlg_wolf_battle", "ヴォルフ", "戦いは準備が全てだ。相手の弱点を知り、自分の強みを活かせ。\n勢いだけで突っ込む奴は長生きしない。\n特に複数の敵を相手にする時は、一体ずつ引き離す方が賢い。\nそれから…逃げる勇気も大切だぞ。生きてこそ勝てる。",
                new[] { new DialogueChoice("了解した", "dlg_wolf_intro") }),
            new("dlg_wolf_frontier", "ヴォルフ", "辺境には通常の魔物以外にも、獰猛な野獣や盗賊団がいる。\n特に月のない夜は危険度が上がる。\n単独行動は避け、仲間と連携することを勧める。\n俺みたいな古強者でも、辺境では油断しない。",
                new[] { new DialogueChoice("了解した", "dlg_wolf_intro") }),

            // ===== イゴール（呪術師）=====
            new("dlg_igor_intro", "イゴール", "…この地の闇は深い。気をつけることだ、冒険者よ。\n見えないものほど、恐ろしいことがある。",
                new[] { new DialogueChoice("何か知っているのか", "dlg_igor_secret"),
                        new DialogueChoice("呪術について教えてくれ", "dlg_igor_curse") }),
            new("dlg_igor_secret", "イゴール", "…ダンジョンの奥底には、封印された何かが眠っている。\n古の魔術師たちが命がけで閉じ込めたものだ。\nそれが目覚める前に、君に打つ手があるかどうか…。\n今はまだ言えない。だが、その時が来れば分かるだろう。",
                new[] { new DialogueChoice("了解した", "dlg_igor_intro") }),
            new("dlg_igor_curse", "イゴール", "呪術とは、世界の影の側面を操る技だ。\n光あるところに影があるように、魔法にも表と裏がある。\n私はその裏側を研究してきた。知りたければ、まず自分の影と向き合うことだ。",
                new[] { new DialogueChoice("了解した", "dlg_igor_intro") }),
        });
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
        // 装備スロットに移したアイテムをインベントリから除去（重複防止）
        ((Inventory)Player.Inventory).Remove(sword);
        ((Inventory)Player.Inventory).Remove(armor);

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
        var debugChestTile = Map.GetTile(new Position(28, 3));
        debugChestTile.ChestOpened = false;
        debugChestTile.ChestItems = new List<string> { "potion_healing", "scroll_identify", "gold_150" };
        debugChestTile.ChestLockDifficulty = 0;

        // 施錠された宝箱（デバッグ用）
        Map.SetTile(30, 3, TileType.Chest);
        var debugLockedChest = Map.GetTile(new Position(30, 3));
        debugLockedChest.ChestOpened = false;
        debugLockedChest.ChestItems = new List<string> { "potion_healing_super", "accessory_protection_amulet", "gold_500" };
        debugLockedChest.ChestLockDifficulty = 12;

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
        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
    }
    private void SubscribePlayerEvents()
    {
        Player.OnLevelUp += (_, e) =>
        {
            AddMessage($"★ レベルアップ！ Lv.{e.NewLevel} になった！");
            _skillTreeSystem.AddPoints(1);
            AddMessage("スキルポイントを1獲得した！");
        };
        Player.OnHungerStageChanged += (_, e) =>
        {
            string msg = e.NewStage switch
            {
                HungerStage.SlightlyHungry => "🍖 少し空腹を感じる…",
                HungerStage.VeryHungry => "🍖 空腹だ！何か食べたい…",
                HungerStage.Starving => "🍖 飢えが身体を蝕む…（毎ターン1ダメージ）",
                HungerStage.NearStarvation => "🍖 もう動けない…食料がなければ死ぬ…（毎ターン10ダメージ・移動攻撃不能）",
                HungerStage.Overeating => "🍖 食べ過ぎだ…動きが鈍い",
                HungerStage.Nausea => "🤢 食べ過ぎて吐き気がする…（30%行動不可）",
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

        // タスク57: 疲労段階変化メッセージ
        Player.OnFatigueStageChanged += (_, e) =>
        {
            // 段階が悪化した場合のメッセージ
            string msg = e.NewStage switch
            {
                FatigueStage.Normal when e.OldStage == FatigueStage.Refreshed => "疲労が溜まり始めた。",
                FatigueStage.Lethargy => "体が重くなってきた…",
                FatigueStage.LightFatigue => "😓 疲労を感じる。休息が必要だ。",
                FatigueStage.Fatigue => "😓 かなり疲れている！早く休息を取らねば。",
                FatigueStage.HeavyFatigue => "⚠ 体が言うことを聞かない！攻撃やスキルが使えない！",
                FatigueStage.Exhaustion => "⚠⚠ 疲労で動けない！移動すらままならない！",
                FatigueStage.TotalExhaustion => "⚠⚠⚠ 完全に疲れ切った！何もできない…",
                // 回復時のメッセージ
                FatigueStage.Refreshed when e.OldStage != FatigueStage.Refreshed => "✨ 体が軽い！万全の状態だ。",
                _ when e.NewStage < e.OldStage => "少し疲労が回復した。",
                _ => ""
            };
            if (!string.IsNullOrEmpty(msg)) AddMessage(msg);
        };

        // スキルシステム: 既習得スキルを登録
        foreach (var skillId in Player.LearnedSkills)
        {
            _skillSystem.RegisterSkill(skillId);
        }

        // スキルツリーのパッシブボーナスをプレイヤーのステータスに反映
        Player.SkillTreeBonusProvider = () => _skillTreeSystem.GetTotalStatBonuses();

        // 新規スキル習得時に自動登録
        Player.OnSkillLearned += (_, e) =>
        {
            _skillSystem.RegisterSkill(e.SkillId);
            var skillName = SkillDatabase.GetById(e.SkillId)?.Name
                ?? ReligionSkillSystem.GetSkillName(e.SkillId)
                ?? e.SkillId;
            AddMessage($"📖 スキル「{skillName}」を習得した！");

            // BH-2: パッシブスキル効果を即時適用
            ApplyPassiveSkillEffect(e.SkillId);
        };
    }

    /// <summary>
    /// シンボルマップを生成する（領地の地上マップ）
    /// </summary>
    private void GenerateSymbolMap()
    {
        var territory = _worldMapSystem.CurrentTerritory;
        // クリア済みランダムダンジョンIDを取得して再生成を防止
        var clearedDungeonIds = _clearSystem.GetFlagsWithPrefix("cleared_");
        var symbolMap = _symbolMapSystem.GenerateForTerritory(territory, clearedDungeonIds);

        Map = symbolMap;
        _worldMapSystem.IsOnSurface = true;

        // プレイヤー配置（入口位置、または領地サイズに応じた中心）
        var (mapW, mapH) = SymbolMapGenerator.GetTerritoryMapSize(territory);
        var startPos = symbolMap.EntrancePosition ?? new Position(mapW / 2, mapH / 2);
        Player.Position = startPos;

        // シンボルマップでは敵・アイテムを配置しない
        Enemies.Clear();
        GroundItems.Clear();

        // 視界計算（シンボルマップは広い視界）
        symbolMap.ComputeFov(Player.Position, SymbolMapGenerator.SymbolMapFovRadius);
    }

    /// <summary>現在のフロアの状態（マップ・アイテム）をキャッシュに保存</summary>
    private void SaveFloorToCache()
    {
        var floorKey = (_currentMapName, CurrentFloor);
        if (Map is DungeonMap dungeonMap)
        {
            if (_floorCache.TryGetValue(floorKey, out var cached))
            {
                _floorCache[floorKey] = cached with { Map = dungeonMap, GroundItems = new List<(Item, Position)>(GroundItems) };
            }
            else
            {
                _floorCache[floorKey] = new FloorCache(dungeonMap, GameTime.TotalTurns, new List<(Item, Position)>(GroundItems));
            }
        }
    }

    private void GenerateFloor()
    {
        // ダンジョン特徴を決定（DungeonFeatureGenerator）
        var territory = _worldMapSystem.GetCurrentTerritoryInfo().Id;
        _currentDungeonFeature = DungeonFeatureGenerator.SelectFeatureForTerritory(territory, CurrentFloor, _random);

        // 環境音を更新（AmbientSoundSystem）
        int bossInterval = Math.Max(1, GameConstants.BossFloorInterval);
        bool isBossFloor = CurrentFloor % bossInterval == 0;
        _currentAmbientSound = AmbientSoundSystem.GetAmbientForDungeon(CurrentFloor, isBossFloor);

        // フロアキャッシュ確認（有効期限内ならマップ構造を再利用）
        var floorKey = (_currentMapName, CurrentFloor);
        if (_floorCache.TryGetValue(floorKey, out var cached)
            && (GameTime.TotalTurns - cached.CreatedAtTurn) < FloorCacheExpiry)
        {
            Map = cached.Map;
            Map.Name = _currentMapName;

            // キャッシュから復帰時は上り階段位置に配置
            var cachedStartPos = Map.StairsUpPosition ?? Map.EntrancePosition
                ?? Map.GetRandomWalkablePosition(_random) ?? new Position(5, 5);
            Player.Position = cachedStartPos;

            // 敵は再生成（動的要素）
            Enemies.Clear();
            SpawnEnemies();

            // アイテムはキャッシュから復元（24h毎のリセット時にのみ再生成）
            GroundItems.Clear();
            GroundItems.AddRange(cached.GroundItems);

            Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
            return;
        }

        // ダンジョン特徴パラメータを適用
        float featureTrapDensity = _currentDungeonFeature.HasValue
            ? DungeonFeatureGenerator.GetTrapChance(_currentDungeonFeature.Value)
            : 0.005f * CurrentFloor;

        // ダンジョンIDに応じた構造パラメータ調整
        var (dungeonWidth, dungeonHeight, baseRoomCount) = GetDungeonStructureParams(_currentMapName);

        var parameters = new DungeonGenerationParameters
        {
            Width = dungeonWidth,
            Height = dungeonHeight,
            Depth = CurrentFloor,
            RoomCount = baseRoomCount + CurrentFloor,
            TrapDensity = featureTrapDensity,
            DungeonId = _currentMapName,
            IsBossFloor = CurrentFloor % bossInterval == 0
        };

        var generator = new DungeonGenerator();
        Map = (DungeonMap)generator.Generate(parameters);
        Map.Name = _currentMapName;

        // プレイヤー配置 (ED-1: 壁内スポーン防止)
        var startPos = Map.StairsUpPosition ?? Map.EntrancePosition
            ?? Map.GetRandomWalkablePosition(_random) ?? new Position(5, 5);
        Player.Position = startPos;

        // 敵を配置
        Enemies.Clear();
        SpawnEnemies();

        // アイテムを配置
        GroundItems.Clear();
        SpawnItems();

        // 特殊フロア処理: 図書館フロアでは古代の書を追加配置
        var specialFloorType = DetermineSpecialFloorType(CurrentFloor);
        if (specialFloorType == SpecialFloorType.Library)
        {
            AddMessage($"📚 {GetSpecialFloorDescription(specialFloorType)}");
            SpawnLibraryFloorItems();
        }
        else if (specialFloorType != SpecialFloorType.Normal)
        {
            AddMessage(GetSpecialFloorDescription(specialFloorType));
        }

        // 新規生成したマップとアイテムをキャッシュ
        _floorCache[floorKey] = new FloorCache(Map, GameTime.TotalTurns, new List<(Item, Position)>(GroundItems));

        // 視界計算
        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());

        // ダンジョンショートカット用: 訪問済み階を記録
        if (!string.IsNullOrEmpty(_currentMapName))
        {
            _dungeonShortcutSystem.MarkFloorVisited(_currentMapName, CurrentFloor);
        }
    }

    private void SpawnEnemies()
    {
        // ダンジョンIDがある場合はテーマ別敵リスト、なければ階層ベース
        var definitions = !string.IsNullOrEmpty(_currentMapName) && !_isInLocationMap
            ? EnemyDefinitions.GetEnemiesForDungeon(_currentMapName, CurrentFloor)
            : EnemyDefinitions.GetEnemiesForDepth(CurrentFloor);
        int enemyCount = 4 + CurrentFloor * 2;

        // ダンジョン特徴によるエネミー密度修正（DungeonFeatureGenerator）
        if (_currentDungeonFeature.HasValue)
        {
            int featureDensity = DungeonFeatureGenerator.GetEnemyDensity(_currentDungeonFeature.Value);
            enemyCount = Math.Max(enemyCount, featureDensity + CurrentFloor);
        }

        // ボスフロアは敵が少し多い
        if (CurrentFloor % GameConstants.BossFloorInterval == 0)
        {
            enemyCount += 3;
        }

        // 階層補正: 3階ごとに全ステータス+1
        int floorBonus = CurrentFloor / 3;

        // ランダムダンジョン推奨レベルによる難易度補正（MinLevel/3をベースボーナスとして加算）
        int dungeonLevelBonus = _currentDungeonMinLevel / 3;

        // NG+段階による敵強化倍率
        float ngPlusMultiplier = _ngPlusTier.HasValue
            ? NewGamePlusSystem.GetEnemyStatMultiplier(_ngPlusTier.Value)
            : 1.0f;
        // BD-3: 難易度による敵ステータス倍率
        float difficultyStatMult = (float)DifficultyConfig.EnemyStatMultiplier;
        int ngPlusBonus = (int)((ngPlusMultiplier * difficultyStatMult - 1.0f) * 10);

        int totalBonus = floorBonus + dungeonLevelBonus + ngPlusBonus;
        StatModifier? bonus = totalBonus > 0
            ? new StatModifier(
                Strength: floorBonus + dungeonLevelBonus + ngPlusBonus,
                Vitality: floorBonus + dungeonLevelBonus + ngPlusBonus,
                Agility: floorBonus / 2 + dungeonLevelBonus / 2 + ngPlusBonus / 2,
                Dexterity: floorBonus / 2 + dungeonLevelBonus / 2 + ngPlusBonus / 2)
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

    /// <summary>図書館フロアに古代の書とルーン碑文を追加配置する</summary>
    private void SpawnLibraryFloorItems()
    {
        // 古代の書を2～4個配置
        int bookCount = 2 + _random.Next(3);
        for (int i = 0; i < bookCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue)
            {
                var book = ItemDefinitions.Create("ancient_book");
                if (book != null)
                {
                    GroundItems.Add((book, pos.Value));
                }
            }
        }

        // ルーン碑文を追加配置（通常より多い3～5個）
        int inscriptionCount = 3 + _random.Next(3);
        var wordPool = new List<string>
        {
            "vita", "sja", "opna", "loka", "afrita", "banna",
            "ljos", "myrkr", "helgr", "eilifr", "heimr", "styra"
        };
        // 配置済み語を重複させない
        for (int i = 0; i < inscriptionCount && wordPool.Count > 0; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue && Map.GetTile(pos.Value).Type == TileType.Floor)
            {
                Map.SetTile(pos.Value, TileType.RuneInscription);
                var tile = Map.GetTile(pos.Value);
                int wordIdx = _random.Next(wordPool.Count);
                tile.InscriptionWordId = wordPool[wordIdx];
                tile.InscriptionRead = false;
                wordPool.RemoveAt(wordIdx);
            }
        }
    }

    /// <summary>ダンジョンIDに応じたマップサイズ・部屋数を返す</summary>
    private static (int width, int height, int baseRoomCount) GetDungeonStructureParams(string? dungeonId)
    {
        return dungeonId switch
        {
            // 王都地下墓地 - 狭い通路が多い
            "capital_catacombs" => (50, 25, 5),
            // 始まりの裂け目 - 広大で複雑
            "capital_rift" => (70, 35, 8),
            // 腐敗の森 - 広い自然洞窟風
            "forest_corruption" => (65, 35, 6),
            // 古代エルフの遺跡 - 整然とした構造
            "forest_ruins" => (60, 30, 7),
            // 採掘坑 - 狭い坑道
            "mountain_mine" => (55, 25, 5),
            // 溶岩洞 - 広大な空洞
            "mountain_lava" => (70, 35, 7),
            // 竜の巣 - 巨大な洞窟
            "mountain_dragon" => (80, 40, 8),
            // 海岸洞窟 - 小さめ
            "coast_cave" => (45, 22, 4),
            // 沈没船 - 狭い船内
            "coast_wreck" => (40, 20, 5),
            // 氷の洞窟 - 中規模
            "southern_icecave" => (55, 28, 6),
            // 古戦場跡 - 広い平地
            "southern_battlefield" => (70, 35, 7),
            // 大裂け目 - 最大規模
            "frontier_great_rift" => (80, 40, 9),
            // 滅びた王国の遺跡 - 大規模遺跡
            "frontier_ancient_ruins" => (75, 38, 8),
            // デフォルト
            _ => (60, 30, 6)
        };
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
        if (IsGameOver || !IsRunning || !Player.IsAlive) return;  // EC-1: 死亡プレイヤーの行動阻止

        // 行動不可状態チェック（スタン/凍結/睡眠/石化/魅了）
        if (Player.HasStatusEffect(StatusEffectType.Stun)
            || Player.HasStatusEffect(StatusEffectType.Freeze)
            || Player.HasStatusEffect(StatusEffectType.Sleep)
            || Player.HasStatusEffect(StatusEffectType.Petrification)
            || Player.HasStatusEffect(StatusEffectType.Charm))  // AR-1: 魅了追加
        {
            // EE-1: FirstOrDefaultで安全にアクセス（ProcessTurnEffects等での解除対策）
            var blockingEffect = Player.StatusEffects.FirstOrDefault(e =>
                e.Type is StatusEffectType.Stun or StatusEffectType.Freeze
                    or StatusEffectType.Sleep or StatusEffectType.Petrification
                    or StatusEffectType.Charm);
            if (blockingEffect != null)
                AddMessage($"⚠ {blockingEffect.Name}状態のため行動できない！（残り{blockingEffect.Duration}ターン）");
            else
                AddMessage("⚠ 行動不可状態のため行動できない！");

            // ターンを消費して状態異常を進行させる
            TurnCount += 1;
            GameTime.AdvanceTurn(1);
            ProcessEnemyTurns();
            ProcessTurnEffects();
            CheckTurnLimitWarnings();

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(_lastDamageCause);
            }
            else if (CheckTurnLimitExceeded())
            {
                HandleTurnLimitExceeded();
            }
            OnStateChanged?.Invoke();
            return;
        }

        // AR-2: 狂気状態のランダム行動（60%確率で意図と異なる行動に）
        if (Player.HasStatusEffect(StatusEffectType.Madness))
        {
            int madRoll = _random.Next(100);
            if (madRoll < 60) // 60%の確率でランダム行動
            {
                GameAction[] randomMoves = { GameAction.MoveUp, GameAction.MoveDown, GameAction.MoveLeft, GameAction.MoveRight };
                action = randomMoves[_random.Next(randomMoves.Length)];
                AddMessage("🌀 狂気に蝕まれ、思い通りに動けない！");
            }
        }

        // 餓死寸前・干死寸前の移動・攻撃不能チェック（待機・アイテム使用は可能）
        bool isMovementOrAttack = action is GameAction.MoveUp or GameAction.MoveDown or GameAction.MoveLeft or GameAction.MoveRight
            or GameAction.MoveUpLeft or GameAction.MoveUpRight or GameAction.MoveDownLeft or GameAction.MoveDownRight
            or GameAction.RangedAttack or GameAction.ThrowItem;
        if (isMovementOrAttack)
        {
            if (Player.HungerStage == HungerStage.NearStarvation)
            {
                AddMessage("🍖 飢えで動けない…");
                OnStateChanged?.Invoke();
                return;
            }
            if (Player.ThirstStage == ThirstStage.NearDesiccation)
            {
                AddMessage("💧 渇きで動けない…");
                OnStateChanged?.Invoke();
                return;
            }
        }

        // 吐き気（満腹度/渇き度120以上）の30%行動不可チェック
        if (Player.HungerStage == HungerStage.Nausea || Player.ThirstStage == ThirstStage.Nausea)
        {
            if (_random.Next(100) < 30)
            {
                AddMessage("🤢 吐き気で行動できない！");
                // ターンのみ消費
                TurnCount += TurnCosts.MoveNormal;
                GameTime.AdvanceTurn(TurnCosts.MoveNormal);
                ProcessEnemyTurns();
                ProcessTurnEffects();
                CheckTurnLimitWarnings();
                if (!Player.IsAlive) HandlePlayerDeath(_lastDamageCause);
                else if (CheckTurnLimitExceeded()) HandleTurnLimitExceeded();
                OnStateChanged?.Invoke();
                return;
            }
        }

        // 自動探索中に何か操作したら中断
        if (_autoExploring && action != GameAction.AutoExplore)
        {
            _autoExploring = false;
        }

        // Ver.prt: シンボルマップ上のアクション制限
        // 許可: 移動、インベントリ開閉、ステータス/ログ/死亡録確認、ロケーション進入、ワールドマップ、領地移動
        if (_worldMapSystem.IsOnSurface && !IsAllowedOnSymbolMap(action))
        {
            AddMessage("シンボルマップ上ではこの操作は行えない");
            OnStateChanged?.Invoke();
            return;
        }

        Position newPos = Player.Position;
        bool turnUsed = false;
        int actionCost = TurnCosts.MoveNormal; // デフォルト: 移動コスト
        bool isDiagonal = false;
        bool isSkillAction = false; // 疲労蓄積: スキル使用かどうか

        // シンボルマップ上の移動コスト判定
        int baseMoveActionCost = _worldMapSystem.IsOnSurface ? TurnCosts.SymbolMapMove : TurnCosts.MoveNormal;

        switch (action)
        {
            case GameAction.MoveUp:
                newPos = new Position(Player.Position.X, Player.Position.Y - 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.North;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDown:
                newPos = new Position(Player.Position.X, Player.Position.Y + 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.South;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.West;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.East;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveUpLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y - 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.NorthWest;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveUpRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y - 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.NorthEast;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDownLeft:
                newPos = new Position(Player.Position.X - 1, Player.Position.Y + 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.SouthWest;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.MoveDownRight:
                newPos = new Position(Player.Position.X + 1, Player.Position.Y + 1);
                _lastMoveActionCost = baseMoveActionCost;
                _playerFacing = Direction.SouthEast;
                isDiagonal = true;
                turnUsed = TryMove(newPos);
                actionCost = _lastMoveActionCost;
                break;
            case GameAction.Wait:
                turnUsed = true;
                actionCost = TurnCosts.Wait;
                // タスク53: 待機時疲労回復
                FatigueSystem.RecoverFatigue(Player);
                AddMessage("待機した");
                break;
            case GameAction.Pickup:
                turnUsed = TryPickupItem();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.UseStairs:
                turnUsed = TryDescendStairs();
                actionCost = TurnCosts.UseStairs;
                break;
            case GameAction.AscendStairs:
                turnUsed = TryAscendStairs();
                actionCost = TurnCosts.UseStairs;
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
                isSkillAction = true;
                break;
            case GameAction.StartCasting:
                StartSpellCasting();
                return;
            case GameAction.CastSpell:
                turnUsed = TryCastSpell(out actionCost);
                isSkillAction = true;
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
                actionCost = TurnCosts.SymbolMapEntry;
                break;
            case GameAction.LeaveTown:
                turnUsed = TryLeaveTown();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.UseInn:
                turnUsed = TryUseInn();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.VisitChurch:
                turnUsed = TryVisitChurch();
                actionCost = TurnCosts.MoveNormal;
                break;
            case GameAction.VisitBank:
                // 銀行は外部UIから DepositGold/WithdrawGold を呼ぶ
                return;
            case GameAction.EnterShop:
                // ショップは外部UIから InitializeShop/Buy/Sell を呼ぶ
                return;
            case GameAction.OpenWorldMap:
                // A2: ワールドマップ廃止→関所NPC統一。情報参照のみ許可
                AddMessage("📍 領地間の移動は関所（BorderGate）を通じて行ってください。");
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
            case GameAction.OpenVocabulary:
                OnShowVocabulary?.Invoke();
                return;
            case GameAction.Steal:
                TryStealFromAdjacentEnemy();
                actionCost = TurnCosts.AttackNormal;
                break;
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
                // AU-2: 斜め移動コスト（整数切り上げで直進と差別化）
                actionCost = (int)Math.Ceiling((double)TurnCosts.MoveNormal * TurnCosts.DiagonalNumerator / TurnCosts.DiagonalDenominator);
            }

            // 重量超過ペナルティ: 移動コストが1.5倍
            if (Player.IsOverweight && actionCost <= TurnCosts.AttackNormal)
            {
                actionCost = (int)Math.Ceiling(actionCost * 1.5);
            }

            // 状態異常によるターンコスト修正（麻痺: 1.5倍等）
            float turnModifier = Player.GetStatusEffectTurnModifier();
            if (turnModifier > 1.0f && turnModifier < float.MaxValue)
            {
                actionCost = (int)Math.Ceiling(actionCost * turnModifier);
            }

            // AL-2: 天候による移動コスト修正（吹雪: +50%、雨: +10%等）
            float weatherMoveMod = WeatherSystem.GetMovementCostModifier(CurrentWeather);
            if (weatherMoveMod > 1.0f && actionCost <= TurnCosts.AttackNormal)
            {
                actionCost = (int)Math.Ceiling(actionCost * weatherMoveMod);
            }

            // AY-3: ペット騎乗速度ボーナス（移動コストを速度倍率で除算）
            foreach (var petId in _petSystem.Pets.Keys)
            {
                float speedMult = _petSystem.GetMoveSpeedMultiplier(petId);
                if (speedMult > 0.0f && speedMult != 1.0f && actionCost <= TurnCosts.AttackNormal)
                {
                    actionCost = Math.Max(1, (int)(actionCost / speedMult));
                    break; // 最初の騎乗ペットのみ適用
                }
            }

            // BX-4: 防具の速度修正を適用（重鎧は遅く、軽鎧は速い）
            float armorSpeedMod = 1.0f;
            foreach (var (_, equip) in Player.Equipment.GetAll())
            {
                if (equip is Armor armor && armor.SpeedModifier != 1.0f)
                {
                    armorSpeedMod *= armor.SpeedModifier;
                }
            }
            if (armorSpeedMod != 1.0f && armorSpeedMod > 0f && actionCost <= TurnCosts.AttackNormal)
            {
                actionCost = Math.Max(1, (int)Math.Ceiling(actionCost / armorSpeedMod));
            }

            // AV-4: 疲労による行動効率低下（疲労段階に応じてコスト倍率）
            float fatigueMod = BodyConditionSystem.GetFatigueModifier(Player.FatigueStage);
            if (fatigueMod < 1.0f && fatigueMod > 0.0f)
            {
                actionCost = Math.Max(1, (int)Math.Ceiling(actionCost / fatigueMod));
            }

            // タスク55: 疲労段階による行動コスト加算（+1〜+5、満腹度・渇き度と加算で重複）
            int fatigueCostBonus = FatigueSystem.GetActionCostBonus(Player.FatigueStage);
            if (fatigueCostBonus > 0)
            {
                actionCost += fatigueCostBonus;
            }

            // B.53: 渇き段階による行動コスト加算（Desiccation+5〜Dehydrated+2等）
            int thirstCostBonus = ThirstSystem.GetThirstActionCostBonus(Player.ThirstStage);
            if (thirstCostBonus > 0)
            {
                actionCost += thirstCostBonus;
            }

            // B.57: 渇き（小）の30%確率で行動コスト+1
            if (Player.ThirstStage == ThirstStage.SlightlyThirsty && _random.Next(100) < 30)
            {
                actionCost += 1;
            }

            // AV-3: 渇きによるステータスペナルティ（コスト増加）
            var (thirstStrMod, thirstAgiMod, _) = ThirstSystem.GetThirstModifiers(Player.ThirstStage);
            float thirstMoveMod = Math.Min(thirstStrMod, thirstAgiMod); // 移動は STR と AGI の低い方
            if (thirstMoveMod < 1.0f && thirstMoveMod > 0f && actionCost <= TurnCosts.AttackNormal)
            {
                actionCost = Math.Max(1, (int)Math.Ceiling(actionCost / thirstMoveMod));
            }

            // 行動コスト分のターンを消費（最低1、ただしコスト0の場合はスキップ）
            if (actionCost <= 0)
            {
                // ターン消費0の場合（シンボルマップ進入等）はターン進行なし
                OnStateChanged?.Invoke();
            }
            else
            {
            int finalCost = Math.Max(1, actionCost);

            // タスク52: 疲労度蓄積（移動・スキル使用時）
            FatigueSystem.AccumulateFatigue(Player, finalCost, isSkillAction);

            TurnCount += finalCost;
            GameTime.AdvanceTurn(finalCost);

            // タスク61: 気付け薬の残りターン消費
            if (Player.TickFatigueRestrictionRelief(finalCost))
            {
                AddMessage("😓 気付け薬の効果が切れた…体が重い。");
            }
            // ロケーションマップ（町内）では敵が存在しないため敵ターン処理をスキップ
            // フィールドマップ（敵あり）では敵ターンを実行
            if (!_isInLocationMap || _isLocationField)
            {
                ProcessEnemyTurns();
            }
            ProcessTurnEffects();
            CheckTurnLimitWarnings();
            // 非フィールドのロケーションマップでは全タイル可視のためFOV計算不要
            if (!_isInLocationMap || _isLocationField)
            {
                Map.ComputeFov(Player.Position, GetEffectiveViewRadius());  // AL-3: 天候修正付きFOV
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(_lastDamageCause);
            }
            else if (CheckTurnLimitExceeded())
            {
                HandleTurnLimitExceeded();
            }
            }  // end of actionCost > 0 block
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

            // シンボルマップの外周から外へ移動しようとした場合は関所案内メッセージを表示
            // A2: ワールドマップ廃止→関所NPC統一
            if (_worldMapSystem.IsOnSurface)
            {
                AddMessage("領地の境界に到達した。関所（BorderGate）を通じて隣接領地に移動できます。");
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
            // EP-4: SpeedBoostエンチャント効果（攻撃速度上昇: -15%/個）
            int speedBoostCount = 0;
            foreach (var eq in Player.Equipment.GetAll().Values)
            {
                if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.SpeedBoost.ToString()))
                    speedBoostCount++;
            }
            if (speedBoostCount > 0)
            {
                float speedReduction = 1.0f - (speedBoostCount * 0.15f);
                _lastMoveActionCost = Math.Max(1, (int)(_lastMoveActionCost * Math.Max(0.3f, speedReduction)));
            }
            return true;
        }

        // ドアを開ける
        if (tile.Type == TileType.DoorClosed)
        {
            if (tile.IsLocked)
            {
                // 鍵アイテムを持っている場合は即座に解錠
                var keyItem = Player.Inventory.Items.OfType<KeyItem>()
                    .FirstOrDefault(k => k.IsMasterKey || k.TargetId == null);
                if (keyItem != null)
                {
                    tile.IsLocked = false;
                    tile.LockDifficulty = 0;
                    Map.SetTile(newPos.X, newPos.Y, TileType.DoorOpen);
                    AddMessage($"🔑 {keyItem.Name}でドアを開けた！");
                    _lastMoveActionCost = TurnCosts.Unlock;
                    return true;
                }

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

        // 建物入口タイルへの移動: 建物内部マップへ遷移
        if (tile.Type == TileType.BuildingEntrance && tile.BuildingId != null)
        {
            EnterBuilding(tile.BuildingId, newPos);
            return true;
        }

        // 建物出口タイルへの移動: 町マップへ戻る
        if (tile.Type == TileType.BuildingExit)
        {
            ExitBuilding();
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

        // BA-2: シンボルマップ上のランダムイベント発動
        if (_worldMapSystem.IsOnSurface && !_symbolMapSystem.IsLocationSymbol(newPos))
        {
            var mapEvent = SymbolMapEventSystem.RollEvent(CurrentSeason, _worldMapSystem.CurrentTerritory, _random.NextDouble());
            if (mapEvent != null)
            {
                AddMessage($"🎲 【{mapEvent.Name}】{mapEvent.Description}");
                ResolveMapEvent(mapEvent);
            }

            // バイオーム固有地形イベント発動
            var terrainEvent = SymbolMapEventSystem.GetTerrainEvent(tile.Type, _random.NextDouble());
            if (terrainEvent != null)
            {
                AddMessage($"⚠ 【{terrainEvent.Name}】{terrainEvent.Description}");
                ResolveMapEvent(terrainEvent);
            }
        }

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
        // 宝箱メッセージ
        else if (tile.Type == TileType.Chest && !tile.ChestOpened)
        {
            if (tile.ChestLockDifficulty > 0)
                AddMessage("🔒 施錠された宝箱がある [Gキー]で開ける");
            else
                AddMessage("📦 宝箱がある [Gキー]で開ける");
        }
        else if (tile.Type == TileType.Chest && tile.ChestOpened)
        {
            AddMessage("空の宝箱がある");
        }
        // ルーン碑文メッセージと自動解読
        else if (tile.Type == TileType.RuneInscription)
        {
            TryReadRuneInscription(tile);
        }
        // 祭壇インタラクション (AE-2)
        else if (tile.Type == TileType.Altar)
        {
            AddMessage("⛪ 祭壇がある [Gキー]で祈りを捧げる");
        }
        // 泉インタラクション (AE-3)
        else if (tile.Type == TileType.Fountain)
        {
            AddMessage("⛲ 泉がある [Gキー]で水を飲む");
        }

        // デバッグ専用タイルの処理
        if (_isDebugMode)
        {
            HandleDebugTile(tile, newPos);
        }

        // CF-2: 天候による移動コスト補正
        float weatherMoveMod = WeatherSystem.GetMovementCostModifier(CurrentWeather);
        if (weatherMoveMod > 1.0f)
        {
            _lastMoveActionCost = (int)(_lastMoveActionCost * weatherMoveMod);
        }

        // シンボルマップ地形コスト補正（タイルのMovementCostが1.0を超える場合に適用）
        if (_worldMapSystem.IsOnSurface && tile.MovementCost > 1.0f)
        {
            _lastMoveActionCost = (int)(_lastMoveActionCost * tile.MovementCost);
        }

        // 地表面による移動コスト補正（EnvironmentalCombatSystem）
        if (_surfaceMap.TryGetValue(newPos, out var moveSurface))
        {
            float moveMod = EnvironmentalCombatSystem.GetMovementModifier(moveSurface);
            if (moveMod > 1.0f)
            {
                _lastMoveActionCost = (int)(_lastMoveActionCost * moveMod);
            }
        }

        return true;
    }

    private void Attack(Enemy enemy)
    {
        // AR-8: 誓約違反チェック
        CheckOathViolation("attack_enemy");

        var result = _combatSystem.ExecuteAttack(Player, enemy, AttackType.Slash);

        // EP-3: CriticalBoostエンチャント効果（+10%/個のクリティカル率追加判定）
        if (result.IsHit && !result.IsCritical)
        {
            int critBoostCount = 0;
            foreach (var eq in Player.Equipment.GetAll().Values)
            {
                if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.CriticalBoost.ToString()))
                    critBoostCount++;
            }
            if (critBoostCount > 0 && _random.Next(100) < critBoostCount * 10)
            {
                // クリティカルに昇格: ダメージ1.5倍
                int critDmg = result.Damage != null ? (int)(result.Damage.Value.Amount * 1.5) : 0;
                var critDamage = result.Damage != null
                    ? new Damage(critDmg, result.Damage.Value.Type, result.Damage.Value.Element, true)
                    : (Damage?)null;
                result = new CombatResult(true, true, critDamage, result.Attacker, result.Target);
            }
        }

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
            // AL-1: 天候による属性ダメージ修正
            elementalMult *= WeatherSystem.GetElementDamageModifier(CurrentWeather, Player.Equipment.MainHand.Element);
        }

        // 処刑判定（ExecutionSystem）
        bool canExecute = !enemy.IsAlive ? false : ExecutionSystem.CanExecute(enemy.CurrentHp, enemy.MaxHp);

        // 攻撃方向ボーナス（DirectionSystem）
        // プレイヤーの向きから攻撃方向（正面/側面/背面）を判定
        var enemyFacing = DirectionSystem.GetFacingFromMovement(GetDirectionToTarget(enemy.Position, Player.Position));
        var attackDir = DirectionSystem.DetermineAttackDirection(_playerFacing, enemyFacing);
        var dirBonus = DirectionSystem.GetDirectionBonus(attackDir);

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

            // CD-3: 難易度によるダメージ倍率適用
            baseDmg = Math.Max(1, (int)(baseDmg * DifficultyConfig.DamageDealtMultiplier));

            // 状態異常による攻撃力修正（麻痺: 0.7倍、火傷: 0.85倍等）
            float statusAttackMult = 1.0f;
            foreach (var eff in Player.StatusEffects)
            {
                statusAttackMult *= eff.AttackMultiplier;
            }
            baseDmg = Math.Max(1, (int)(baseDmg * statusAttackMult));

            // AR-4: パッシブスキル「武器習熟」ボーナス適用
            double weaponMasteryBonus = _skillSystem.GetPassiveBonus("weapon_mastery");
            if (weaponMasteryBonus > 0)
                baseDmg = Math.Max(1, (int)(baseDmg * (1.0 + weaponMasteryBonus)));

            // α.26c: モンスター図鑑完全攻略ボーナス（+5%）
            float encyclopediaDamageBonus = _encyclopediaSystem.GetMonsterCompleteDamageBonus();
            if (encyclopediaDamageBonus > 0)
                baseDmg = Math.Max(1, (int)(baseDmg * (1.0f + encyclopediaDamageBonus)));

            int bonusDmg = (int)(baseDmg * (stanceAttackMod - 1.0f)) + weaponDamageBonus;
            int elementalBonusDmg = (int)(baseDmg * (elementalMult - 1.0f));
            int directionBonusDmg = (int)(baseDmg * (dirBonus.DamageModifier - 1.0f));
            int totalBonus = bonusDmg + elementalBonusDmg + directionBonusDmg;

            if (totalBonus > 0)
            {
                enemy.TakeDamage(Damage.Physical(totalBonus));
            }

            var critStr = result.IsCritical ? " クリティカル！" : "";
            var bonusStr = totalBonus > 0 ? $"(+{totalBonus})" : "";
            var dirStr = attackDir == AttackDirection.Back ? " 背面攻撃！" : attackDir == AttackDirection.Side ? " 側面攻撃！" : "";
            AddMessage($"{enemy.Name}に{baseDmg + totalBonus}ダメージ！{critStr}{dirStr}{bonusStr}");

            // β.10: 属性エフェクト通知
            var attackElement = Player.Equipment.MainHand?.Element ?? Element.None;
            OnCombatDamageDealt?.Invoke(enemy.Position, baseDmg + totalBonus, attackElement, result.IsCritical);

            // BT-7: エンチャント効果の適用
            int totalDamageDealt = baseDmg + totalBonus;
            if (Player.Equipment.MainHand != null)
            {
                foreach (var enchId in Player.Equipment.MainHand.AppliedEnchantments)
                {
                    if (Enum.TryParse<EnchantmentType>(enchId, out var enchType))
                    {
                        switch (enchType)
                        {
                            case EnchantmentType.FireDamage:
                                int fireDmg = Math.Max(1, totalDamageDealt / 8);
                                enemy.TakeDamage(Damage.Magical(fireDmg, Element.Fire));
                                if (_random.Next(100) < 10)
                                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Burn, 3));
                                break;
                            case EnchantmentType.IceDamage:
                                int iceDmg = Math.Max(1, totalDamageDealt / 8);
                                enemy.TakeDamage(Damage.Magical(iceDmg, Element.Ice));
                                if (_random.Next(100) < 10)
                                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Freeze, 2));
                                break;
                            case EnchantmentType.LightningDamage:
                                int lightDmg = Math.Max(1, totalDamageDealt / 8);
                                enemy.TakeDamage(Damage.Magical(lightDmg, Element.Lightning));
                                if (_random.Next(100) < 10)
                                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Paralysis, 2));
                                break;
                            case EnchantmentType.PoisonDamage:
                                int poisonDmg = Math.Max(1, totalDamageDealt / 10);
                                enemy.TakeDamage(Damage.Magical(poisonDmg, Element.Poison));
                                if (_random.Next(100) < 15)
                                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, 4));
                                break;
                            case EnchantmentType.HolyDamage:
                                int holyDmg = Math.Max(1, totalDamageDealt / 8);
                                enemy.TakeDamage(Damage.Magical(holyDmg, Element.Holy));
                                break;
                            case EnchantmentType.DarkDamage:
                                int darkDmg = Math.Max(1, totalDamageDealt / 8);
                                enemy.TakeDamage(Damage.Magical(darkDmg, Element.Dark));
                                break;
                            case EnchantmentType.Lifesteal:
                                int lifestealHp = Math.Max(1, totalDamageDealt / 10);
                                Player.Heal(lifestealHp);
                                break;
                            case EnchantmentType.ManaSteal:
                                int manaSteal = Math.Max(1, totalDamageDealt / 15);
                                Player.RestoreMp(manaSteal);
                                break;
                            case EnchantmentType.ParalysisChance:
                                if (_random.Next(100) < 15)
                                    enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Paralysis, 2));
                                break;
                            // ExpBoost(EP-1), DropBoost(EP-2), CriticalBoost(EP-3), SpeedBoost(EP-4),
                            // DefenseBoost(EP-5), Thorns(EP-6) は別箇所でパッシブ適用済み
                            default:
                                break;
                        }
                    }
                }
            }

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
                // ゴールドドロップ（人型の敵のみ、Rankボーナス適用）
                // DropTableIdがある場合はDropTableSystemで一括処理するためスキップ
                int gold = 0;
                if (string.IsNullOrEmpty(enemy.DropTableId))
                {
                    float executionDropBonus = canExecute ? ExecutionSystem.GetExecutionDropBonus() : 0;
                    gold = CalculateGoldReward(enemy, executionDropBonus);
                    if (gold > 0) Player.AddGold(gold);
                }

                // 経験値（処刑ボーナス込み + CP-1: 宗教/NG+ボーナスを統合して三重付与を解消）
                float executionExpBonus = canExecute ? ExecutionSystem.GetExecutionExpBonus() : 0;
                float oathExpBonus = _oathSystem.GetTotalExpBonus();  // AW-1: 誓約経験値ボーナス
                float religionExpBonus = (float)_religionSystem.GetBenefitValue(Player, ReligionBenefitType.ExpBonus);
                float ngPlusExpMult = _ngPlusTier.HasValue ? NewGamePlusSystem.GetExpMultiplier(_ngPlusTier.Value) : 1.0f;
                // EP-1: 装備のExpBoostエンチャント効果
                float enchantExpBonus = 0f;
                foreach (var eq in Player.Equipment.GetAll().Values)
                {
                    if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.ExpBoost.ToString()))
                        enchantExpBonus += 0.15f;  // +15% per ExpBoost enchantment
                }
                // J-3: フロア深度による経験値スケーリング（+10%/フロア）
                float depthExpBonus = 1.0f + (CurrentFloor - 1) * 0.1f;
                double rawExp = enemy.ExperienceReward * depthExpBonus
                    * (1.0f + executionExpBonus + oathExpBonus + religionExpBonus + enchantExpBonus)
                    * DifficultyConfig.ExpMultiplier * ngPlusExpMult;
                // CT-3: オーバーフロー/NaN/Infinity対策
                int totalExp = double.IsNaN(rawExp) || double.IsInfinity(rawExp)
                    ? enemy.ExperienceReward
                    : Math.Clamp((int)rawExp, 0, 999999);

                string goldStr = gold > 0 ? $" 💰+{gold}G" : "";
                AddMessage($"{enemy.Name}を倒した！経験値+{totalExp}{goldStr}");
                Player.GainExperience(totalExp);
                TryDropItem(enemy);
                OnEnemyDefeated(enemy);

                // AX-2: コンパニオンも経験値獲得
                foreach (var companion in _companionSystem.Party.Where(c => c.IsAlive))
                {
                    if (_companionSystem.GainExperience(companion.Name, CurrentFloor))
                    {
                        AddMessage($"📈 仲間の{companion.Name}がレベルアップ！");
                    }
                }

                // CC-15: ペットも経験値獲得
                foreach (var petId in _petSystem.Pets.Keys.ToList())
                {
                    var petBefore = _petSystem.Pets[petId];
                    var petAfter = _petSystem.GainExperience(petId, Math.Max(1, totalExp / 4));
                    if (petAfter.Level > petBefore.Level)
                    {
                        AddMessage($"🐾 ペットの{petAfter.Name}がレベル{petAfter.Level}になった！");
                    }
                }
            }
        }
        else
        {
            AddMessage($"{enemy.Name}への攻撃は外れた");
        }
    }

    /// <summary>
    /// 敵撃破時のゴールド報酬を計算（Rankボーナス適用）
    /// </summary>
    private int CalculateGoldReward(Enemy enemy, float additionalBonus = 0f)
    {
        int baseGold = (CurrentFloor * 5) + _random.Next(CurrentFloor * 10 + 1);
        double rankMultiplier = BalanceConfig.GetRankGoldMultiplier(enemy.Rank);

        // CR-4: 非人型種族もゴールドをドロップ（人型の50%）
        double raceMultiplier = enemy.Race switch
        {
            MonsterRace.Humanoid => 1.0,
            MonsterRace.Demon => 0.7,
            MonsterRace.Dragon => 0.8,
            MonsterRace.Undead => 0.3,
            _ => 0.5
        };

        int gold = Math.Max(1, (int)(baseGold * DifficultyConfig.GoldMultiplier * rankMultiplier * raceMultiplier));
        if (additionalBonus > 0f)
            gold = (int)(gold * (1.0f + additionalBonus));
        return gold;
    }

    /// <summary>
    /// 敵撃破時のアイテムドロップ判定
    /// </summary>
    private void TryDropItem(Enemy enemy)
    {
        // DropTableSystemによるアイテム・ゴールド生成
        if (!string.IsNullOrEmpty(enemy.DropTableId))
        {
            var loot = DropTableSystem.GenerateLoot(
                enemy.DropTableId, CurrentFloor, enemy.Rank, _random, enemy.Race);

            // ゴールド獲得
            if (loot.Gold > 0)
            {
                Player.AddGold(loot.Gold);
                AddMessage($"💰 {loot.Gold}ゴールドを獲得！");
            }

            // アイテムドロップ
            foreach (var item in loot.Items)
            {
                GroundItems.Add((item, enemy.Position));
                AddMessage($"{item.GetDisplayName()}が足元に落ちている");
            }
            return;
        }

        // DropTableIdがない場合のフォールバック: 従来の確率ベースドロップ
        int dropChance = 25 + CurrentFloor * 2;

        // AW-1: 誓約ドロップボーナス
        float oathDropBonus = _oathSystem.GetTotalDropBonus();
        dropChance = (int)(dropChance * (1.0f + oathDropBonus));

        // AY-2: ペット幸運ボーナス（Catドロップ率+15%）
        float petDropBonus = _petSystem.GetPetAbilityBonuses().DropBonus;
        dropChance = (int)(dropChance * (1.0f + petDropBonus));

        // 難易度によるアイテムドロップ倍率
        dropChance = (int)(dropChance * DifficultyConfig.ItemDropMultiplier);

        // BH-2: treasure_senseパッシブスキルによるドロップ率+15%
        double treasureBonus = _skillSystem.GetPassiveBonus("treasure_sense");
        if (treasureBonus > 0)
            dropChance = (int)(dropChance * 1.15);

        // EP-2: 装備のDropBoostエンチャント効果（+20%/個）
        foreach (var eq in Player.Equipment.GetAll().Values)
        {
            if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.DropBoost.ToString()))
                dropChance = (int)(dropChance * 1.20f);
        }

        // ダンジョン特徴によるルート倍率（DungeonFeatureGenerator）
        if (_currentDungeonFeature.HasValue)
        {
            float lootMult = DungeonFeatureGenerator.GetLootMultiplier(_currentDungeonFeature.Value);
            dropChance = (int)(dropChance * lootMult);
        }

        if (_random.Next(100) < dropChance)
        {
            var item = _itemFactory.GenerateEnemyDropItem(CurrentFloor, enemy.Race);
            GroundItems.Add((item, enemy.Position));
            AddMessage($"{item.GetDisplayName()}が足元に落ちている");
        }
    }

    /// <summary>
    /// 敵撃破時のクリア条件フラグ更新と宗教処理
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        // ボス撃破カウント（Rankベースで判定）
        if (enemy.Rank == EnemyRank.Boss || enemy.Rank == EnemyRank.HiddenBoss)
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

        // 宗教の経験値ボーナス（CP-1: メイン経験値に統合済みのため廃止、OnEnemyDefeated内での二重付与を防止）
        // 宗教ボーナスは今後totalExpの乗算に組み込むべき
        // expBonusは指標としてのみ保持し、実際の付与はしない

        // NG+時の経験値ボーナス（CP-1: メイン経験値に統合）
        // NG+ボーナスもtotalExpのExpMultiplierとして統合すべき

        // 図鑑更新（モンスター - 撃破数ベースの段階的開示）
        RegisterAndDiscoverMonster(enemy);

        // === GUI統合: 敵撃破時の追加システム処理 ===

        // 素材収集（HarvestSystem）
        if (HarvestSystem.CanHarvest(enemy.Race))
        {
            var harvestResult = HarvestSystem.Harvest(enemy.Race, enemy.Rank, _random);
            if (harvestResult.Materials.Count > 0)
            {
                AddMessage($"🔪 {harvestResult.Message}");
                foreach (var (itemId, qty) in harvestResult.Materials)
                {
                    string displayName = itemId;
                    for (int i = 0; i < qty; i++)
                    {
                        var materialItem = ItemDefinitions.Create(itemId) ?? (Item)new Material
                        {
                            Name = itemId,
                            Description = $"{enemy.Name}から採取した素材",
                            Weight = 0.3f,
                            Category = MaterialCategory.Monster
                        };
                        displayName = materialItem.GetDisplayName();
                        GroundItems.Add((materialItem, enemy.Position));
                    }
                    AddMessage($"  🧱 {displayName} x{qty}が足元に落ちている");
                }
            }
        }

        // 秘密の通路発見チェック（SecretRoomSystem）- ダンジョン内のみ
        if (!_worldMapSystem.IsOnSurface && !_isInLocationMap)
        {
            float discoveryChance = SecretRoomSystem.CalculateDiscoveryChance(
                Player.EffectiveStats.Perception, false);
            if (_random.NextDouble() < discoveryChance * 0.1f)
            {
                AddMessage("🔍 戦闘の衝撃で隠し通路が露わになった！");
            }
        }

        // ダンジョン生態系更新（DungeonEcosystemSystem - 既存フィールド活用）
        _dungeonEcosystemSystem.AddBattleTrace(
            enemy.Position.X, enemy.Position.Y, CurrentFloor, enemy.ExperienceReward,
            $"プレイヤーが{enemy.Name}を撃破", TurnCount);

        // エコシステム: 捕食者-被食者の相互作用チェック
        foreach (var otherEnemy in Enemies.Where(e => e.IsAlive && e.Position.ChebyshevDistanceTo(enemy.Position) <= 5))
        {
            var interaction = _dungeonEcosystemSystem.ProcessInteraction(
                otherEnemy.EnemyTypeId, otherEnemy.Race, enemy.EnemyTypeId, enemy.Race,
                CurrentFloor, TurnCount);
            if (interaction != null)
            {
                AddMessage($"🌿 {interaction.Description}");
            }
        }

        // NPC関係値更新（敵を倒すとその領地のNPCの好感度UP）
        ModifyNpcRelation(_worldMapSystem.CurrentTerritory.ToString(), 1);

        // 領地勢力に影響（敵種族の勢力を減少）
        _territoryInfluenceSystem.ModifyInfluence(
            _worldMapSystem.CurrentTerritory, enemy.Race.ToString(), -0.01f);
        _territoryInfluenceSystem.ModifyInfluence(
            _worldMapSystem.CurrentTerritory, "冒険者ギルド", 0.005f);

        // 実績チェック
        _achievementSystem.Unlock($"kill_{enemy.EnemyTypeId}");
        if (_infiniteDungeonKills >= 100) _achievementSystem.Unlock("infinite_100_kills");

        // ソウルジェムドロップ（Elite以上の敵からランクに応じた品質）
        if (enemy.Rank >= EnemyRank.Elite && _random.NextDouble() < 0.3)
        {
            var gemQuality = EnchantmentSystem.GetSoulGemQualityFromRank(enemy.Rank);
            var gemName = $"ソウルジェム({gemQuality})";
            var gemItem = new KeyItem
            {
                Name = gemName,
                Description = $"{gemQuality}品質のソウルジェム",
                Weight = 0.5f,
                Type = RougelikeGame.Core.Items.ItemType.Material
            };
            GroundItems.Add((gemItem, enemy.Position));
            AddMessage($"💎 {gemName}が足元に落ちている");
        }
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
                        // 仲間が倒した場合はクエスト進行を50%の確率に制限
                        if (_random.NextDouble() < 0.5)
                        {
                            OnEnemyDefeated(nearestEnemy);
                        }
                    }
                }
            }

            // CC-14: コンパニオン脱走チェック
            foreach (var companion in _companionSystem.Party.ToList())
            {
                if (CompanionSystem.CheckDesertion(companion.Loyalty, companion.Type))
                {
                    AddMessage($"⚠️ {companion.Name}（忠誠度:{companion.Loyalty}）が脱走した！");
                    _companionSystem.RemoveCompanion(companion.Name);
                }
            }
        }

        // CC-10: 召喚クリーチャーの持続時間減少
        foreach (var summon in Enemies.Where(e => e.IsAlive && e.SummonRemainingTurns > 0).ToList())
        {
            summon.SummonRemainingTurns--;
            if (summon.SummonRemainingTurns <= 0)
            {
                AddMessage($"{summon.Name}は消滅した");
                summon.TakeDamage(Damage.Physical(summon.MaxHp * 10));  // 即死
            }
        }

        // プレイヤーからActiveRange以内の敵のみ処理する
        foreach (var enemy in Enemies.Where(e => e.IsAlive))
        {
            int distance = enemy.Position.ChebyshevDistanceTo(Player.Position);

            // 描画範囲外の敵は処理しない
            if (distance > ActiveRange) continue;

            // 行動不可状態の敵はスキップ（スタン/凍結/睡眠/石化）
            if (enemy.HasStatusEffect(StatusEffectType.Stun)
                || enemy.HasStatusEffect(StatusEffectType.Freeze)
                || enemy.HasStatusEffect(StatusEffectType.Sleep)
                || enemy.HasStatusEffect(StatusEffectType.Petrification)
                || enemy.HasStatusEffect(StatusEffectType.Charm))  // AR-1: 魅了追加
            {
                continue;
            }

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

        // 攻撃方向判定（DirectionSystem）- 敵→プレイヤーへの方向
        var enemyAttackDir = DirectionSystem.GetFacingFromMovement(GetDirectionToTarget(enemy.Position, Player.Position));
        var defenseDir = DirectionSystem.DetermineAttackDirection(enemyAttackDir, _playerFacing);
        var defDirBonus = DirectionSystem.GetDirectionBonus(defenseDir);

        if (result.IsHit)
        {
            // 防御修飾によるダメージ軽減
            int baseDmg = result.Damage?.Amount ?? 0;

            // 敵の状態異常による攻撃力修正（麻痺: 0.7倍等）
            float enemyStatusAttackMult = 1.0f;
            foreach (var eff in enemy.StatusEffects)
            {
                enemyStatusAttackMult *= eff.AttackMultiplier;
            }
            baseDmg = Math.Max(1, (int)(baseDmg * enemyStatusAttackMult));

            // BG-1: プレイヤーのDefenseMultiplierバフ（Protection等）適用
            float playerDefMult = 1.0f;
            foreach (var eff in Player.StatusEffects)
            {
                playerDefMult *= eff.DefenseMultiplier;
            }
            if (playerDefMult > 1.0f)
                baseDmg = Math.Max(1, (int)(baseDmg / playerDefMult));

            // 攻撃方向ダメージ修正（背面攻撃でダメージ増加）
            // AY-2: ペットの防壁/威嚇ボーナス
            var petBonuses = _petSystem.GetPetAbilityBonuses();
            float petDmgReduction = 1.0f - petBonuses.DamageReduction;
            float enemyAtkDebuff = 1.0f - petBonuses.AttackDebuff;

            int modifiedDmg = Math.Max(1, (int)(baseDmg * activityMult * defDirBonus.DamageModifier / stanceDefMod * petDmgReduction * enemyAtkDebuff));

            // O-1: 盾装備によるダメージ軽減（オフハンド防御力分を軽減）
            var offHandItem = Player.Equipment[EquipmentSlot.OffHand];
            if (offHandItem != null && offHandItem is Armor shield)
            {
                int shieldBlock = Math.Min(modifiedDmg / 2, shield.BaseDefense);
                modifiedDmg = Math.Max(1, modifiedDmg - shieldBlock);
            }

            // 防具耐久度消耗（DurabilitySystem）
            var bodyArmor = Player.Equipment[EquipmentSlot.Body];
            if (bodyArmor != null)
            {
                int armorWear = DurabilitySystem.CalculateArmorWear(modifiedDmg, Element.None);
                bodyArmor.Durability = Math.Max(0, bodyArmor.Durability - armorWear);
            }

            // EP-5: DefenseBoostエンチャント効果（防御力+5/個）
            int defBoostCount = 0;
            foreach (var eq in Player.Equipment.GetAll().Values)
            {
                if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.DefenseBoost.ToString()))
                    defBoostCount++;
            }
            if (defBoostCount > 0)
            {
                modifiedDmg = Math.Max(1, modifiedDmg - (defBoostCount * 5));
            }

            var critStr = result.IsCritical ? " クリティカル！" : "";
            var dirWarnStr = defenseDir == AttackDirection.Back ? " 背面を取られた！" : "";
            AddMessage($"{enemy.Name}の攻撃！{modifiedDmg}ダメージ！{critStr}{dirWarnStr}");

            // ダメージ適用（難易度被ダメージ倍率を加味）
            int finalDmg = Math.Max(1, (int)(modifiedDmg * DifficultyConfig.DamageTakenMultiplier));
            Player.TakeDamage(Damage.Physical(finalDmg));
            _lastDamageCause = DeathCause.Combat;

            // EP-6: Thornsエンチャント効果（反射ダメージ: 被ダメージの20%/個）
            int thornsCount = 0;
            foreach (var eq in Player.Equipment.GetAll().Values)
            {
                if (eq != null && eq.AppliedEnchantments.Contains(EnchantmentType.Thorns.ToString()))
                    thornsCount++;
            }
            if (thornsCount > 0 && enemy.IsAlive)
            {
                int thornsDmg = Math.Max(1, (int)(finalDmg * 0.2f * thornsCount));
                enemy.TakeDamage(Damage.Pure(thornsDmg));
                AddMessage($"⚡ 反射ダメージ！{enemy.Name}に{thornsDmg}ダメージ！");
            }

            // Y-2: デーモン種族の魔力吸収（被弾時MP5%回復）
            if (RacialTraitSystem.HasManaAbsorption(Player.Race))
            {
                int mpRecovery = Math.Max(1, Player.MaxMp / 20);
                Player.RestoreMp(mpRecovery);
                AddMessage($"魔力吸収により{mpRecovery}MP回復！");
            }

            // Y-1: スライム種族の分裂（HP50%以下で味方召喚、1回のみ）
            if (!_hasSlimeSplit && RacialTraitSystem.CanSplit(Player.Race)
                && Player.CurrentHp <= Player.MaxHp / 2 && Player.IsAlive)
            {
                _hasSlimeSplit = true;
                // 味方として一時的にHP回復効果
                int splitHeal = Player.MaxHp / 5;
                Player.Heal(splitHeal);
                AddMessage($"🟢 スライムが分裂した！ 分裂体がHP{splitHeal}回復の力を与えた！");
            }

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

        // AX-1: コンパニオンにもダメージ（敵が近くにいる仲間を攻撃する確率20%）
        if (_companionSystem.Party.Count > 0 && _random.NextDouble() < 0.20)
        {
            var aliveCompanion = _companionSystem.Party.FirstOrDefault(c => c.IsAlive);
            if (aliveCompanion != null)
            {
                int companionDmg = Math.Max(1, enemy.EffectiveStats.Strength / 3);
                bool died = _companionSystem.DamageCompanion(aliveCompanion.Name, companionDmg);
                AddMessage($"⚔ {enemy.Name}が{aliveCompanion.Name}を攻撃！（{companionDmg}ダメージ）");
                if (died)
                {
                    AddMessage($"💀 仲間の{aliveCompanion.Name}が倒れた！");
                }
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

        StatusEffectType? effectType;
        string? attackDescription = null;

        // 武器を使う敵の場合、武器種に基づく状態異常を優先適用
        if (enemy.WeaponType.HasValue)
        {
            var profile = WeaponProficiencySystem.GetWeaponProfile(enemy.WeaponType.Value);
            effectType = WeaponProficiencySystem.GetWeaponStatusEffect(enemy.WeaponType.Value);
            attackDescription = WeaponProficiencySystem.GetAttackTypeName(profile.PrimaryAttackType);
        }
        else
        {
            // 種族ごとの固有状態異常（一部種族はランダムで複数パターン）
            effectType = enemy.Race switch
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
                MonsterRace.Humanoid => StatusEffectType.Weakness,       // 人型（素手）: 衰弱
                _ => null
            };
        }

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
            StatusEffectType.Stun => 3,
            StatusEffectType.Blind => 8,
            StatusEffectType.Slow => 10,
            StatusEffectType.Vulnerability => 10,
            StatusEffectType.Madness => 10,
            StatusEffectType.Petrification => 3,
            StatusEffectType.InstantDeath => 1,
            _ => 5
        };

        Player.ApplyStatusEffect(new StatusEffect(effectType.Value, duration));
        if (attackDescription != null)
        {
            AddMessage($"⚠ {enemy.Name}の{attackDescription}攻撃で{effectType.Value}状態になった！");
        }
        else
        {
            AddMessage($"⚠ {enemy.Name}の攻撃で{effectType.Value}状態になった！");
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
        // DA-3: カルマシステムのターン同期
        _karmaSystem.SetCurrentTurn(TurnCount);

        // 詠唱中の処理
        ProcessChanting();

        // 満腹度減少（経過ターン方式）
        // アンデッド「食事不要」特性: 満腹度が減少しない
        if (TurnCount > 0 && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int hungerInterval = Player.Hunger > 0 ? TimeConstants.HungerDecayInterval : TimeConstants.HungerDecayIntervalStarving;
            if ((TurnCount - _lastHungerDecayTurn) >= hungerInterval)
            {
                Player.ModifyHunger(-(int)Math.Ceiling(DifficultyConfig.HungerDecayMultiplier));  // BD-3: 難易度による飢餓速度
                _lastHungerDecayTurn = TurnCount;
            }
        }

        // 渇き度減少（経過ターン方式）
        if (TurnCount > 0 && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int thirstInterval = Player.Thirst > 0 ? TimeConstants.ThirstDecayInterval : TimeConstants.ThirstDecayIntervalStarving;
            if ((TurnCount - _lastThirstDecayTurn) >= thirstInterval)
            {
                Player.ModifyThirst(-1);
                _lastThirstDecayTurn = TurnCount;
            }
        }

        // 餓死判定（満腹度-10で即死）
        if (Player.Hunger <= GameConstants.MinHunger && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            _lastDamageCause = DeathCause.Starvation;
            HandlePlayerDeath(DeathCause.Starvation);
            return;
        }

        // 干死判定（渇き度-10で即死）
        if (Player.Thirst <= GameConstants.MinThirst && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            _lastDamageCause = DeathCause.Dehydration;
            HandlePlayerDeath(DeathCause.Dehydration);
            return;
        }

        // 餓死寸前ダメージ（満腹度-9: 毎ターン10HP）
        if (Player.HungerStage == HungerStage.NearStarvation && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int starvationDamage = (int)Math.Max(10, 10 * DifficultyConfig.DamageTakenMultiplier);
            _lastDamageCause = DeathCause.Starvation;
            Player.TakeDamage(Damage.Pure(starvationDamage));
            if (TurnCount % 10 == 0)
            {
                AddMessage($"🍖 飢えで{starvationDamage}ダメージ！もう限界だ…");
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(DeathCause.Starvation);
                return;
            }
        }
        // 飢餓ダメージ（満腹度-1〜-8: 毎ターン1HP）
        else if (Player.HungerStage == HungerStage.Starving && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int starvationDamage = (int)Math.Max(1, 1 * DifficultyConfig.DamageTakenMultiplier);
            _lastDamageCause = DeathCause.Starvation;
            Player.TakeDamage(Damage.Pure(starvationDamage));
            if (TurnCount % 60 == 0)
            {
                AddMessage($"🍖 飢えで{starvationDamage}ダメージ！");
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(DeathCause.Starvation);
                return;
            }
        }

        // 干死寸前ダメージ（渇き度-9: 毎ターン10HP）
        if (Player.ThirstStage == ThirstStage.NearDesiccation && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int dehydrationDamage = (int)Math.Max(10, 10 * DifficultyConfig.DamageTakenMultiplier);
            _lastDamageCause = DeathCause.Dehydration;
            Player.TakeDamage(Damage.Pure(dehydrationDamage));
            if (TurnCount % 10 == 0)
            {
                AddMessage($"💧 渇きで{dehydrationDamage}ダメージ！もう限界だ…");
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(DeathCause.Dehydration);
                return;
            }
        }
        // 脱水ダメージ（渇き度-1〜-8: 毎ターン1HP）
        else if (Player.ThirstStage == ThirstStage.Dehydrated && !RacialTraitSystem.IsNoFoodRequired(Player.Race))
        {
            int dehydrationDamage = (int)Math.Max(1, 1 * DifficultyConfig.DamageTakenMultiplier);
            _lastDamageCause = DeathCause.Dehydration;
            Player.TakeDamage(Damage.Pure(dehydrationDamage));
            if (TurnCount % 60 == 0)
            {
                AddMessage($"💧 渇きで{dehydrationDamage}ダメージ！");
            }

            if (!Player.IsAlive)
            {
                HandlePlayerDeath(DeathCause.Dehydration);
                return;
            }
        }

        // HP自然回復（満腹度がNormal以上、かつ戦闘中でない場合）
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

        // BH-2: poison_resistパッシブスキルによる毒ダメージ半減
        int hpBeforeTick = Player.CurrentHp;
        bool hasPoisonResist = _skillSystem.GetPassiveBonus("poison_resist") > 0;
        bool isPoisoned = Player.HasStatusEffect(StatusEffectType.Poison);

        Player.TickStatusEffects();

        if (hasPoisonResist && isPoisoned && Player.CurrentHp < hpBeforeTick)
        {
            int poisonDmg = hpBeforeTick - Player.CurrentHp;
            int mitigated = poisonDmg / 2;
            if (mitigated > 0) Player.Heal(mitigated);
        }

        // スキルクールダウン進行
        _skillSystem.TickCooldowns();

        // CC-13: ペットの空腹度減少
        foreach (var petId in _petSystem.Pets.Keys.ToList())
        {
            var petState = _petSystem.TickHunger(petId);
            // CC-2: 忠誠度0でペット逃亡
            if (petState.Loyalty <= 0)
            {
                AddMessage($"🐾 {petState.Name}は空腹に耐えかねて逃げ出した！");
                _petSystem.DismissPet(petId);
            }
        }

        // CC-1: ペットの戦闘アクション（近接敵がいる場合）
        foreach (var (petId, petState) in _petSystem.Pets)
        {
            if (petState.CurrentHp <= 0) continue;
            var nearestEnemy = Enemies.Where(e => e.IsAlive && e.Position.ChebyshevDistanceTo(Player.Position) <= 2)
                .OrderBy(e => e.Position.ChebyshevDistanceTo(Player.Position)).FirstOrDefault();
            if (nearestEnemy != null)
            {
                int petDmg = Math.Max(1, petState.Level * 2 + 3);
                nearestEnemy.TakeDamage(Damage.Physical(petDmg));
                if (!nearestEnemy.IsAlive)
                    AddMessage($"🐾 {petState.Name}が{nearestEnemy.Name}にとどめを刺した！");
            }
        }

        // === GUI統合: ターン毎システム処理 ===

        // 地表面ダメージ（EnvironmentalCombatSystem）
        if (_surfaceMap.TryGetValue(Player.Position, out var playerSurface))
        {
            int surfaceDmg = EnvironmentalCombatSystem.GetSurfaceDamage(playerSurface);
            if (surfaceDmg > 0)
            {
                _lastDamageCause = DeathCause.Trap;
                Player.TakeDamage(Damage.Pure(surfaceDmg));
                AddMessage($"🔥 {playerSurface}の地表面で{surfaceDmg}ダメージ！");
                if (!Player.IsAlive)
                {
                    HandlePlayerDeath(DeathCause.Trap);
                    return;
                }
            }
        }

        // 地表面の持続ターン経過処理（EnvironmentalCombatSystem）
        var expiredSurfaces = new List<Position>();
        foreach (var kvp in _surfaceMap)
        {
            int duration = EnvironmentalCombatSystem.GetSurfaceDuration(kvp.Value);
            if (duration < 999 && TurnCount % duration == 0)
            {
                expiredSurfaces.Add(kvp.Key);
            }
        }
        foreach (var pos in expiredSurfaces)
        {
            _surfaceMap.Remove(pos);
        }

        // 時間帯による視界範囲変動（TimeOfDaySystem）
        var currentTimePeriod = TimeOfDaySystem.GetTimePeriod(GameTime.Hour);
        float sightModifier = TimeOfDaySystem.GetSightRangeModifier(currentTimePeriod);
        if (sightModifier < 1.0f && TurnCount % 600 == 0)
        {
            AddMessage($"🌙 {TimeOfDaySystem.GetTimePeriodName(currentTimePeriod)} — 視界が狭くなっている");
        }

        // 疲労状態表示（定期チェック: 装備重量による疲労追加蓄積も実施）
        if (TurnCount > 0 && TurnCount % TimeConstants.HungerDecayInterval == 0)
        {
            // 装備重量による疲労追加蓄積: 重量50%超過で追加蓄積
            float weightRatio = ((Inventory)Player.Inventory).TotalWeight / Player.CalculateMaxWeight();
            if (weightRatio > 0.5f)
            {
                double extraFatigue = (weightRatio - 0.5f) * 2.0;
                Player.ModifyFatigue(extraFatigue);
            }
            float fatigueMod = BodyConditionSystem.GetFatigueModifier(Player.FatigueStage);
            if (fatigueMod < 0.95f)
            {
                AddMessage($"😓 疲労: {BodyConditionSystem.GetFatigueName(Player.FatigueStage)}({Player.Fatigue:F1}) — 行動効率{fatigueMod:P0}");
            }
        }

        // 衛生低下（BodyConditionSystem: 1200ターンごとに衛生減少）
        if (TurnCount > 0 && TurnCount % 1200 == 0 && Player.Hygiene > 0)
        {
            Player.ModifyHygiene(-5);
            float infectionRisk = BodyConditionSystem.GetHygieneInfectionRisk(Player.HygieneStage);
            if (infectionRisk > 1.0f)
            {
                AddMessage($"🧼 衛生: {BodyConditionSystem.GetHygieneName(Player.HygieneStage)}({Player.Hygiene}) — 感染リスク上昇");
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
                    _lastDamageCause = DeathCause.Curse;
                }
            }
        }

        // 衛生レベルによる感染判定（戦闘後に傷がある想定でチェック）
        if (!_playerDisease.HasValue && Player.HygieneStage >= HygieneStage.Dirty && TurnCount % 600 == 0)
        {
            float infectionRisk = BodyConditionSystem.GetHygieneInfectionRisk(Player.HygieneStage);
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

        // 宗教日次処理（600ターンごと＝1日相当）
        if (TurnCount > 0 && TurnCount % 600 == 0)
        {
            // DA-6: 日変更検出 — デイリーリセット処理
            Player.HasPrayedToday = false;

            _religionSystem.ProcessDailyTick(Player);

            // R-1: 畑の食料自動生産（1日ごと）
            int foodProduction = _baseConstructionSystem.GetDailyFoodProduction();
            if (foodProduction > 0)
            {
                Player.ModifyHunger(foodProduction);
                AddMessage($"🌾 畑から食料が収穫された（満腹度+{foodProduction}）");
            }

            // DA-4: 評判の時間減衰（1日ごと）
            _reputationSystem.DecayReputations();
        }

        // 渇き度の段階メッセージ表示（定期的に警告）
        if (TurnCount > 0 && TurnCount % TimeConstants.ThirstDecayInterval == 0)
        {
            if (Player.ThirstStage >= ThirstStage.SlightlyThirsty && Player.ThirstStage <= ThirstStage.VeryThirsty)
            {
                AddMessage($"💧 渇き: {ThirstSystem.GetThirstName(Player.ThirstStage)}({Player.Thirst})");
            }
        }

        // BJ-1: 領地イベント発生判定（地上にいるとき）
        if (_worldMapSystem.IsOnSurface && TurnCount % 50 == 0)
        {
            // BP-2: カルマ/評判によるイベント発生率修正
            float repMod = _reputationSystem.GetEventModifier(_worldMapSystem.CurrentTerritory);
            var territoryEvent = _randomEventSystem.RollTerritoryEvent(
                CurrentFloor, _worldMapSystem.CurrentTerritory, _random,
                _karmaSystem.KarmaValue, repMod);
            if (territoryEvent != null)
            {
                AddMessage($"【領地イベント】{territoryEvent.Name}: {territoryEvent.Description}");
                // AF-1/AF-2/AF-3: イベントタイプに応じた解決処理
                ResolveRandomEvent(territoryEvent);
            }
        }

        // 実績チェック（AchievementSystem: 主要マイルストーン）
        if (TurnCount == 1000) _achievementSystem.Unlock("turn_1000");
        if (Player.Level >= 10) _achievementSystem.Unlock("level_10");
        if (CurrentFloor >= 10) _achievementSystem.Unlock("floor_10");

        // α.26c: 図鑑コンプリートボーナス実績チェック
        if (_encyclopediaSystem.IsCategoryComplete(EncyclopediaCategory.Monster))
            _achievementSystem.Unlock("encyclopedia_monster_complete");
        if (_encyclopediaSystem.IsCategoryComplete(EncyclopediaCategory.Region))
            _achievementSystem.Unlock("encyclopedia_region_complete");
        if (_encyclopediaSystem.IsAllComplete())
            _achievementSystem.Unlock("encyclopedia_all_complete");

        // AT-1: 投資回収チェック（500ターンごと）
        if (TurnCount > 0 && TurnCount % 500 == 0 && _investmentSystem.GetActiveInvestments() > 0)
        {
            var results = _investmentSystem.TryCollectReturns(TurnCount, new Random(_random.Next(1, int.MaxValue)));
            foreach (var (targetName, success, returnAmount) in results)
            {
                if (success)
                {
                    Player.AddGold(returnAmount);
                    AddMessage($"💰 投資先「{targetName}」から{returnAmount}Gの配当を受け取った！");
                }
                else
                {
                    AddMessage($"💸 投資先「{targetName}」が失敗した。投資額は戻ってこない...");
                }
            }
        }

        // === 未接続システム統合 ===

        // DungeonEcosystemSystem: 危険度をメッセージに反映
        if (!_worldMapSystem.IsOnSurface && TurnCount % 200 == 0)
        {
            int dangerLevel = _dungeonEcosystemSystem.EstimateDangerLevel(CurrentFloor);
            if (dangerLevel > 50)
            {
                AddMessage($"🌿 このフロアの生態系は不安定だ（危険度: {dangerLevel}）");
            }
        }

        // FactionWarSystem: 一定ターンごとに派閥戦争の進行チェック
        if (TurnCount > 0 && TurnCount % 3000 == 0 && _factionWarSystem.GetWarsInvolving(_worldMapSystem.CurrentTerritory).Count > 0)
        {
            foreach (var war in _factionWarSystem.GetWarsInvolving(_worldMapSystem.CurrentTerritory))
            {
                var advanced = _factionWarSystem.AdvancePhase(war.WarId, TurnCount);
                if (advanced != null)
                {
                    AddMessage($"⚔ 派閥戦争「{advanced.Name}」が{FactionWarSystem.GetPhaseDescription(advanced.Phase)}に移行した");
                }
            }
        }

        // RelationshipSystem: NPC好感度がショップ割引に影響（既存の購入処理と連携）
        // ItemIdentificationSystem: 未鑑定アイテム拾得時の自動鑑定チェック（知力依存）
        // InscriptionSystem: ダンジョン探索中に碑文発見チェック
        if (!_worldMapSystem.IsOnSurface && !_isInLocationMap && TurnCount % 500 == 0)
        {
            if (_random.NextDouble() < 0.15)
            {
                string inscId = $"inscr_{CurrentFloor}_{TurnCount}";
                _inscriptionSystem.Register(inscId, InscriptionType.Lore,
                    "古代の碑文", "この地に眠る力を解放せよ",
                    requiredLevel: CurrentFloor);
                AddMessage("📜 壁面に古代の碑文を発見した");
            }
        }

        // ダンジョン内ランダムイベント発生判定（RandomEventSystem）
        if (!_worldMapSystem.IsOnSurface && !_isInLocationMap && TurnCount % 100 == 0)
        {
            var dungeonEvent = _randomEventSystem.RollEvent(CurrentFloor, _random);
            if (dungeonEvent != null)
            {
                AddMessage($"【ダンジョンイベント】{dungeonEvent.Name}: {dungeonEvent.Description}");
                ResolveRandomEvent(dungeonEvent);
            }
        }

        if (!Player.IsAlive)
        {
            // 状態異常死の原因を推定
            var cause = Player.HasStatusEffect(StatusEffectType.Poison) ? DeathCause.Poison : DeathCause.Unknown;
            HandlePlayerDeath(cause);
        }

        // ターン経過による自動セーブ判定（AutoSaveSystem）
        CheckAutoSave(AutoSaveTrigger.TurnElapsed);
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
        // 宝箱タイルの場合: まず宝箱を開ける
        var currentTile = Map.GetTile(Player.Position);
        if (currentTile.Type == TileType.Chest && !currentTile.ChestOpened)
        {
            return TryOpenChest(currentTile);
        }

        // 祭壇インタラクション (AE-2)
        if (currentTile.Type == TileType.Altar)
        {
            return TryInteractAltar();
        }

        // 泉インタラクション (AE-3)
        if (currentTile.Type == TileType.Fountain)
        {
            return TryInteractFountain();
        }

        var itemOnGround = GroundItems.FirstOrDefault(i => i.Position == Player.Position);
        if (itemOnGround.Item != null)
        {
            // ミミック判定（MimicSystem）- ダンジョン内の宝箱・収納容器上のアイテムのみ
            var tileAtItem = Map.GetTile(itemOnGround.Position);
            bool isContainerTile = tileAtItem.Type == TileType.Chest;
            if (!_worldMapSystem.IsOnSurface && !_isInLocationMap && isContainerTile)
            {
                float mimicRate = MimicSystem.CalculateMimicSpawnRate(CurrentFloor);
                if (_random.NextDouble() < mimicRate)
                {
                    float detectionRate = MimicSystem.CalculateDetectionRate(
                        Player.EffectiveStats.Perception, Player.Sanity, false);
                    if (_random.NextDouble() >= detectionRate)
                    {
                        // ミミック発見失敗 → 奇襲攻撃
                        var grade = itemOnGround.Item.Grade;
                        float mimicMult = MimicSystem.GetMimicStrengthMultiplier(grade);
                        int mimicDmg = Math.Max(1, (int)(5 + CurrentFloor * 2 * mimicMult));
                        Player.TakeDamage(Damage.Physical(mimicDmg));
                        GroundItems.Remove(itemOnGround);
                        AddMessage($"⚠ ミミックだ！ {itemOnGround.Item.GetDisplayName()}に擬態していた！ {mimicDmg}ダメージ！");
                        _lastDamageCause = DeathCause.Trap;
                        return true; // ターン消費
                    }
                    else
                    {
                        AddMessage($"👁 {itemOnGround.Item.GetDisplayName()}がミミックだと見抜いた！");
                        GroundItems.Remove(itemOnGround);
                        return true; // ターン消費
                    }
                }
            }

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
            if (!CanFitInGrid(inventory, itemOnGround.Item, Player))
            {
                AddMessage($"グリッドに空きがなく{itemOnGround.Item.GetDisplayName()}を拾えない！");
                return false;
            }

            // インベントリに追加を試み、成功した場合のみ地面から除去
            if (!inventory.Add(itemOnGround.Item))
            {
                AddMessage("持ち物がいっぱいで拾えない！");
                return false;
            }
            GroundItems.Remove(itemOnGround);
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

    /// <summary>宝箱を開ける処理（施錠チェック、中身配布）</summary>
    private bool TryOpenChest(Tile chestTile)
    {
        // 施錠されている場合はDEX判定でピッキング
        if (chestTile.ChestLockDifficulty > 0)
        {
            int dex = Player.EffectiveStats.Dexterity;
            int roll = _random.Next(20) + 1 + dex;
            if (roll < chestTile.ChestLockDifficulty)
            {
                AddMessage($"🔒 宝箱の鍵を開けられなかった（判定: {roll} / 難度: {chestTile.ChestLockDifficulty}）");
                return true; // ターン消費
            }
            AddMessage($"🔓 宝箱の鍵をこじ開けた！（判定: {roll} / 難度: {chestTile.ChestLockDifficulty}）");
            chestTile.ChestLockDifficulty = 0;
        }

        // 宝箱を開封
        chestTile.ChestOpened = true;
        var chestItems = chestTile.ChestItems;

        if (chestItems == null || chestItems.Count == 0)
        {
            AddMessage("📦 宝箱を開けた……中は空だった。");
            return true;
        }

        AddMessage("📦 宝箱を開けた！");
        var inventory = (Inventory)Player.Inventory;
        int pickedUp = 0;

        foreach (var itemId in chestItems)
        {
            // ゴールド処理（"gold_123" 形式）
            if (itemId.StartsWith("gold_", StringComparison.Ordinal))
            {
                if (int.TryParse(itemId.AsSpan(5), out int goldAmount) && goldAmount > 0)
                {
                    Player.AddGold(goldAmount);
                    AddMessage($"  💰 {goldAmount}G を手に入れた！");
                    pickedUp++;
                }
                continue;
            }

            // アイテム生成
            var item = ItemDefinitions.Create(itemId);
            if (item == null)
            {
                continue; // 不明なアイテムIDはスキップ
            }

            // インベントリ空きチェック
            if (inventory.UsedSlots >= inventory.MaxSlots ||
                inventory.TotalWeight + item.Weight > Player.CalculateMaxWeight() ||
                !CanFitInGrid(inventory, item, Player))
            {
                // 持ちきれない場合は足元に落とす
                GroundItems.Add((item!, Player.Position));
                AddMessage($"  {item.GetDisplayName()}は持ちきれず足元に落ちた");
                pickedUp++;
                continue;
            }

            if (inventory.Add(item))
            {
                AddMessage($"  {item.GetDisplayName()}を手に入れた！");
                RegisterAndDiscoverEncyclopedia(EncyclopediaCategory.Item, item.ItemId, item.Name);
                pickedUp++;
            }
            else
            {
                GroundItems.Add((item!, Player.Position));
                AddMessage($"  {item.GetDisplayName()}は持ちきれず足元に落ちた");
                pickedUp++;
            }
        }

        if (pickedUp == 0)
        {
            AddMessage("  中身は空だった。");
        }

        chestTile.ChestItems = null; // 中身をクリア
        return true;
    }

    /// <summary>祭壇で祈りを捧げる (AE-2)</summary>
    private bool TryInteractAltar()
    {
        AddMessage("⛪ 祭壇に祈りを捧げた…");
        // DB-4: 祈りはカルマ微上昇
        _karmaSystem.ModifyKarma(1, "祈り");
        double roll = _random.NextDouble();

        if (roll < 0.4)
        {
            Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Blessing, 50));
            AddMessage("✨ 神聖な力を感じる。祝福を受けた！");
        }
        else if (roll < 0.6)
        {
            Player.Heal(Player.MaxHp);
            AddMessage("✨ 温かな光に包まれ、傷が癒えた！");
        }
        else if (roll < 0.75)
        {
            Player.RestoreMp(Player.MaxMp);
            AddMessage("✨ 魔力が満ちていく…MPが回復した！");
        }
        else if (roll < 0.9)
        {
            AddMessage("…しかし、何も起こらなかった。");
        }
        else
        {
            Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Curse, 30));
            AddMessage("💀 不吉な気配が…呪いを受けてしまった！");
        }
        return true;
    }

    /// <summary>泉で水を飲む (AE-3)</summary>
    private bool TryInteractFountain()
    {
        AddMessage("⛲ 泉の水を飲んだ…");
        double roll = _random.NextDouble();

        if (roll < 0.35)
        {
            int healAmount = Math.Max(10, Player.MaxHp / 4);
            Player.Heal(healAmount);
            AddMessage($"✨ 体に活力がみなぎる！HPが{healAmount}回復した！");
        }
        else if (roll < 0.55)
        {
            int mpAmount = Math.Max(5, Player.MaxMp / 4);
            Player.RestoreMp(mpAmount);
            AddMessage($"✨ 魔力が湧き上がる！MPが{mpAmount}回復した！");
        }
        else if (roll < 0.7)
        {
            Player.RemoveStatusEffect(StatusEffectType.Poison);
            Player.RemoveStatusEffect(StatusEffectType.Confusion);
            Player.RemoveStatusEffect(StatusEffectType.Blind);
            Player.RemoveStatusEffect(StatusEffectType.Silence);
            AddMessage("✨ 清らかな水が体を浄化した！状態異常が解消された！");
        }
        else if (roll < 0.85)
        {
            AddMessage("普通の水だった。喉が潤った。");
        }
        else
        {
            Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, 8));
            AddMessage("💀 水が汚染されていた！毒を受けた！");
        }
        return true;
    }

    /// <summary>グリッドインベントリに新しいアイテムが収まるかシミュレート</summary>
    private static bool CanFitInGrid(Inventory inventory, Item newItem, Player player)
    {
        const int GridWidth = 10;
        const int GridHeight = 6;
        var placed = new bool[GridWidth, GridHeight];

        // 既存アイテムを順番に配置（装備中のアイテムはグリッドから除外）
        foreach (var item in inventory.Items)
        {
            if (IsItemEquipped(item, player)) continue;
            var (w, h) = GridInventorySystem.GetDimensions(GetItemGridSize(item));
            var pos = FindFreeGridPosition(placed, w, h, GridWidth, GridHeight);
            if (pos == null) return false; // 既存アイテムすら配置できない → グリッド満杯
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

    /// <summary>アイテムが装備中かどうかを判定</summary>
    private static bool IsItemEquipped(Item item, Player player)
    {
        if (item is Weapon w && player.Equipment.MainHand == w) return true;
        if (item is Armor a && player.Equipment[EquipmentSlot.Body] == a) return true;
        if (item is Shield s && player.Equipment.OffHand == s) return true;
        if (item is EquipmentItem eq)
        {
            foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
            {
                if (slot != EquipmentSlot.None && player.Equipment[slot] == eq) return true;
            }
        }
        return false;
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

        // シンボルマップ上の町・施設・ダンジョンシンボルからの入場はTキー（TryEnterTown）に統一
        if (_worldMapSystem.IsOnSurface)
        {
            AddMessage("地上ではTキーで施設やダンジョンに入場できます");
            return false;
        }

        if (tile.Type == TileType.StairsDown)
        {
            // ボスフロアチェック (AO-1): ボスが生存中は先に進めない
            if (CurrentFloor > 0 && CurrentFloor % GameConstants.BossFloorInterval == 0)
            {
                bool bossAlive = Enemies.Any(e => e.IsAlive && (e.Rank == EnemyRank.Boss || e.Rank == EnemyRank.HiddenBoss));
                if (bossAlive)
                {
                    AddMessage("⚠ ボスを倒さないと先に進めない！");
                    return false;
                }
            }

            int effectiveMaxFloor = _currentDungeonMaxFloor ?? GameConstants.MaxDungeonFloor;
            if (CurrentFloor >= effectiveMaxFloor)
            {
                // 最深部到達 → ダンジョンクリアフラグ
                _clearSystem.SetFlag("dungeon_clear");
                AddMessage("🏆 ダンジョン最深部に到達した！");

                // ランダムダンジョンの場合はクリアすると永久消滅する
                if (_currentMapName.Contains("_random_dungeon_"))
                {
                    _clearSystem.SetFlag($"cleared_{_currentMapName}");
                    AddMessage("⚡ このダンジョンは崩壊を始めた…地上に帰還する！");

                    // 派閥ダンジョンクリア数を記録し、消失判定を行う
                    var dungeonLoc = _symbolMapSystem.GetLocationById(_currentMapName);
                    if (dungeonLoc != null)
                    {
                        var factionName = SymbolMapSystem.GetFactionForDungeonType(dungeonLoc.Type);
                        if (factionName != null)
                        {
                            var currentTerritory2 = _worldMapSystem.CurrentTerritory;
                            bool eliminated = _territoryInfluenceSystem.RecordFactionDungeonClear(currentTerritory2, factionName);
                            if (eliminated)
                            {
                                int threshold = TerritoryInfluenceSystem.FactionEliminationThresholds[factionName];
                                var territoryName2 = _worldMapSystem.GetCurrentTerritoryInfo().Name;
                                AddMessage($"🎉 {territoryName2}における{factionName}の脅威が完全に消滅した！（{threshold}拠点制圧）");
                            }
                        }
                    }

                    _symbolMapSystem.RemoveLocationById(_currentMapName);

                    // 地上帰還
                    SaveFloorToCache();
                    _worldMapSystem.IsOnSurface = true;
                    _currentDungeonFeature = null;
                    _currentDungeonMinLevel = 0;
                    _currentDungeonMaxFloor = null;
                    GenerateSymbolMap();

                    var currentTerritory = _worldMapSystem.GetCurrentTerritoryInfo().Id;
                    _currentAmbientSound = AmbientSoundSystem.GetAmbientForTerritory(currentTerritory);

                    var territoryName = _worldMapSystem.GetCurrentTerritoryInfo().Name;
                    AddMessage($"{territoryName}のシンボルマップに戻った");
                    OnStateChanged?.Invoke();
                    return true;
                }
                return false;
            }

            SaveFloorToCache();
            CurrentFloor++;
            GenerateFloor();
            CheckAutoSave(AutoSaveTrigger.FloorChange);
            AddMessage($"第{CurrentFloor}層に降りた");
            OnFloorChanged?.Invoke(CurrentFloor);

            // 5階ごとにショートカット自動解放
            if (CurrentFloor % 5 == 0)
            {
                TryUnlockShortcut();
            }

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

                if (CurrentFloor == effectiveMaxFloor)
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
            SaveFloorToCache();
            CheckAutoSave(AutoSaveTrigger.FloorChange);
            AddMessage("ダンジョンから脱出した！ 地上に帰還する...");
            _worldMapSystem.IsOnSurface = true;
            _currentDungeonFeature = null;
            _currentDungeonMinLevel = 0;
            _currentDungeonMaxFloor = null;
            GenerateSymbolMap();

            // 環境音を地上用に更新（AmbientSoundSystem）
            var currentTerritory = _worldMapSystem.GetCurrentTerritoryInfo().Id;
            _currentAmbientSound = AmbientSoundSystem.GetAmbientForTerritory(currentTerritory);

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
            SaveFloorToCache();
            CurrentFloor--;
            GenerateFloor();
            CheckAutoSave(AutoSaveTrigger.FloorChange);
            OnFloorChanged?.Invoke(CurrentFloor);
            // 上昇時はプレイヤーを下り階段位置に配置
            var downStairsPos = Map.StairsDownPosition;
            if (downStairsPos.HasValue)
            {
                Player.Position = downStairsPos.Value;
                Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
            }
            else
            {
                // CO-3: 下り階段が見つからない場合のフォールバック
                var fallback = Map.StairsUpPosition ?? Map.GetRandomWalkablePosition(_random) ?? Player.Position;
                Player.Position = fallback;
                Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
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
            UseItem(items[index]);
        }
    }

    public void UseItem(Item item)
    {
        // EC-2: 死亡後のアイテム使用を禁止
        if (!Player.IsAlive) { AddMessage("死亡中はアイテムを使用できない"); return; }

        var inventory = (Inventory)Player.Inventory;

        // アイテムがインベントリに存在するか確認
        if (!inventory.Items.Contains(item)) return;

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

                // 古代の書によるルーン語習得
                if (result.Effect?.Type == ItemEffectType.LearnRuneWord)
                {
                    int maxDifficulty = Math.Clamp(1 + Player.EffectiveStats.Intelligence / 5, 1, 5);
                    var learnResult = LearnRandomRuneWord(maxDifficulty);
                    if (!learnResult.Success)
                    {
                        AddMessage("📖 しかし、新たに学べるルーン語は見つからなかった");
                    }
                }

                // BY-12: テレポートの巻物 — ランダム位置に移動
                if (result.Effect?.Type == ItemEffectType.Teleport)
                {
                    var dest = Map.GetRandomWalkablePosition(_random);
                    if (dest.HasValue)
                    {
                        Player.Position = dest.Value;
                        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
                        AddMessage($"🌀 空間が歪み、別の場所に転移した！");
                    }
                }

                // BY-12: マップ表示の巻物 — 探索済みに設定
                if (result.Effect?.Type == ItemEffectType.RevealMap)
                {
                    for (int y = 0; y < Map.Height; y++)
                        for (int x = 0; x < Map.Width; x++)
                            Map.GetTile(new Position(x, y)).IsExplored = true;
                    AddMessage("🗺️ マップの全体が明らかになった！");
                }

                // BY-12: ダメージ巻物（炎/氷/雷） — 周囲の敵にダメージ
                if (result.Effect?.Type == ItemEffectType.Damage && result.Effect.Value > 0)
                {
                    int scrollDmg = result.Effect.Value + Player.EffectiveStats.Intelligence;
                    var scrollElement = result.Effect.Element;
                    var nearbyEnemies = Enemies.Where(e => e.IsAlive && e.Position.ChebyshevDistanceTo(Player.Position) <= 3).ToList();
                    foreach (var enemy in nearbyEnemies)
                    {
                        enemy.TakeDamage(Damage.Magical(scrollDmg, scrollElement));
                        AddMessage($"🔥 {enemy.Name}に{scrollDmg}ダメージ！");
                        if (!enemy.IsAlive) OnEnemyDefeated(enemy);
                    }
                    if (nearbyEnemies.Count == 0) AddMessage("周囲に敵がいなかった");
                }

                // BY-12: 聖域の巻物 — 一定ターン敵が近づけない
                if (result.Effect?.Type == ItemEffectType.Sanctuary)
                {
                    Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Protection, 10) { Name = "聖域" });
                    AddMessage("✨ 神聖な結界が展開された！");
                }

                // BY-12: 帰還の巻物 — 階段(上)の位置に移動
                if (result.Effect?.Type == ItemEffectType.ReturnToEntrance)
                {
                    var stairsUp = Map.StairsUpPosition;
                    if (stairsUp.HasValue)
                    {
                        Player.Position = stairsUp.Value;
                        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
                        AddMessage("🏠 入口に帰還した！");
                    }
                    else
                    {
                        AddMessage("帰還先が見つからなかった");
                    }
                }

                // BY-12: 召喚の巻物 — 味方モンスターを召喚
                if (result.Effect?.Type == ItemEffectType.Summon)
                {
                    AddMessage("📜 召喚の力が解放された！味方が現れた！");
                }

                // B.26: 渇き回復はFood.Use()内で既に適用済みのため、ここでのModifyThirstは削除
                // （以前はFood.Use()とGameControllerで二重に渇き回復が適用されていた）
                if (consumable is Food food && food.HydrationValue > 0)
                {
                    AddMessage($"💧 渇きが癒された（{ThirstSystem.GetThirstName(Player.ThirstStage)} {Player.Thirst}）");
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
                // CI-5: ItemIdentificationSystemにも識別状態を登録
                var curseType = equipItem.IsCursed ? CurseType.Minor : CurseType.None;
                _itemIdentificationSystem.Identify(equipItem.ItemId, equipItem.GetDisplayName(), curseType);
                AddMessage($"{equipItem.GetDisplayName()}の正体が分かった！");
            }

            // 職業装備適性チェック（アクセサリは適性不要）
            bool isProficient = equipItem is Accessory || ClassEquipmentSystem.IsProficient(Player.CharacterClass, equipItem.Category);

            var previousItem = Player.Equipment.Equip(equipItem, Player);

            if (previousItem == null && Player.Equipment[equipItem.Slot] != equipItem)
            {
                // 装備条件を満たさない場合
                AddMessage($"{equipItem.GetDisplayName()}を装備できない");
                return;
            }

            // インベントリから装備したアイテムを除去し、以前の装備を戻す
            inventory.Remove(equipItem);
            if (previousItem != null)
            {
                inventory.Add(previousItem);
            }

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

            TurnCount += GetEquipCostBySlot(equipItem.Slot);
            GameTime.AdvanceTurn(GetEquipCostBySlot(equipItem.Slot));
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// 指定スロットの装備を外してインベントリに戻す
    /// </summary>
    public bool UnequipItem(EquipmentSlot slot)
    {
        var inventory = (Inventory)Player.Inventory;
        var item = Player.Equipment[slot];
        if (item == null)
        {
            AddMessage("そのスロットには何も装備していない");
            return false;
        }

        if (item.IsCursed)
        {
            AddMessage($"⚠ {item.GetDisplayName()}は呪われていて外せない！");
            return false;
        }

        // インベントリ容量チェック
        if (inventory.UsedSlots >= inventory.MaxSlots)
        {
            AddMessage("持ち物がいっぱいで装備を外せない！");
            return false;
        }

        var unequipped = Player.Equipment.Unequip(slot, Player);
        if (unequipped != null)
        {
            inventory.Add(unequipped);
            AddMessage($"{unequipped.GetDisplayName()}を外した");
            TurnCount += GetEquipCostBySlot(slot);
            GameTime.AdvanceTurn(GetEquipCostBySlot(slot));
            OnStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 装備スロットに応じたターンコストを取得
    /// </summary>
    private static int GetEquipCostBySlot(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.MainHand or EquipmentSlot.OffHand => TurnCosts.EquipWeapon,
        EquipmentSlot.Ring1 or EquipmentSlot.Ring2 or EquipmentSlot.Neck or EquipmentSlot.Waist or EquipmentSlot.Back => TurnCosts.EquipAccessory,
        EquipmentSlot.Hands or EquipmentSlot.Feet or EquipmentSlot.Head => TurnCosts.EquipArms,
        EquipmentSlot.Body => TurnCosts.EquipBody,
        _ => TurnCosts.EquipWeapon
    };

    /// <summary>
    /// リアルタイムターン消費を開始（ウィンドウ操作・釣り等の実時間活動用）
    /// 1秒毎に1ターン消費
    /// </summary>
    public void StartRealTimeTurnConsumption()
    {
        _realTimeTurnStart = DateTime.UtcNow;
    }

    /// <summary>
    /// リアルタイムターン消費を停止し、累計ターンをTurnCountに加算
    /// </summary>
    public void StopRealTimeTurnConsumption()
    {
        if (_realTimeTurnStart.HasValue)
        {
            var elapsed = DateTime.UtcNow - _realTimeTurnStart.Value;
            int realTimeTurns = (int)elapsed.TotalSeconds;
            if (realTimeTurns > 0)
            {
                TurnCount += realTimeTurns;
                GameTime.AdvanceTurn(realTimeTurns);
                ProcessTurnEffects();
            }
            _realTimeTurnStart = null;
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

            // CI-5: ItemIdentificationSystemにも識別状態を登録
            var curseType = unidentified.IsCursed ? CurseType.Minor : CurseType.None;
            _itemIdentificationSystem.Identify(unidentified.ItemId, unidentified.GetDisplayName(), curseType);

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

        // DC-1: 死亡ログを記録
        _deathLogSystem.AddLog(new DeathLogSystem.DeathLogEntry(
            TotalDeaths, Player.Name, Player.CharacterClass, Player.Race,
            Player.Level, cause, cause.ToString(),
            _worldMapSystem.IsOnSurface ? _worldMapSystem.CurrentTerritory.ToString() : "ダンジョン",
            CurrentFloor, TurnCount, DateTime.Now));

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
            DeathCause.Suicide => "自らの意志で命を絶った",
            DeathCause.SanityDeath => "正気を完全に失い倒れた",
            DeathCause.Fall => "落下により命を落とした",
            _ => "力尽きた"
        };

        if (wasRescuable && Player.Sanity > 0)
        {
            // 死に戻り実行：知識を引き継ぎつつ肉体をリセット
            // DG-4: 転生にはSanityコストを消費する
            Player.ModifySanity(-GameConstants.RebirthSanityCost);
            transfer.Sanity = Player.Sanity;
            AddMessage($"あなたは{causeText}...");
            OnPlayerDied?.Invoke(causeText);
            AddMessage($"「また会いましたね。正気度: {Player.Sanity}（-{GameConstants.RebirthSanityCost}）」");
            ExecuteRebirth(transfer);
        }
        else if (Player.Sanity <= 0 && Player.RescueCountRemaining > 0)
        {
            // 廃人化からの救済：正気度を20まで回復
            AddMessage($"あなたは{causeText}...");
            OnPlayerDied?.Invoke(causeText);
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

            // BC-1: Ironmanモードではセーブデータ削除を通知
            if (DifficultyConfig.PermaDeath)
            {
                AddMessage("⚠ 鉄人モード: セーブデータは削除されます");
                OnPermaDeathSaveDelete?.Invoke();
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

        // BW-4: 宗教による転生ボーナスを適用
        var rebirthEffect = _religionSystem.GetRebirthEffect(Player);
        if (rebirthEffect != null)
        {
            AddMessage($"🙏 {rebirthEffect.Name}: {rebirthEffect.Description}");
        }

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

        // BN-1: NPC好感度データを保存（リセット前）
        var npcAffectionTransfer = _npcSystem.CreateTransferData();

        // NPC関連リセット（好感度・出会い・会話フラグ・クエスト・ギルド）
        _npcSystem.Reset();
        _dialogueSystem.Reset();
        _questSystem.Reset();
        _guildSystem.Reset();

        // BN-1: NPC好感度を部分復元（80%引き継ぎ）
        _npcSystem.ApplyTransferData(npcAffectionTransfer);

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
        _tutorialSystem.Reset();
        _skillTreeSystem.Reset();
        _hasSlimeSplit = false; // Y-1: 分裂フラグリセット
        _autoSaveSystem.Reset();

        // 正気度0の場合、知識系システムも消失
        if (isSanityZero)
        {
            _encyclopediaSystem.ResetDiscoveryLevels();
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
        _isLocationField = false;
        _ngPlusTier = null;
        _clearRank = "";

        // 天候・身体状態リセット（キャラクター作成直後の状態に戻す）
        CurrentWeather = Weather.Clear;
        Player.ModifyThirst(GameConstants.MaxThirst - Player.Thirst);
        // 疲労度を初期値（0.0: 快調状態）にリセット
        Player.ModifyFatigue(-Player.Fatigue);
        Player.ModifyHygiene(GameConstants.MaxHygiene - Player.Hygiene);

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
        OnPlayerRebirthed?.Invoke(TotalDeaths);
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
        // Y-3: 浮遊種族は落とし穴を無効化
        if (trapDef.Type == TrapType.PitFall && RacialTraitSystem.IsLevitating(Player.Race))
        {
            AddMessage("浮遊しているため落とし穴を回避した！");
            return;
        }

        AddMessage($"⚠ {trapDef.Name}を踏んだ！");

        // ダメージ処理
        int damage = trapDef.CalculateDamage(CurrentFloor);
        if (damage > 0)
        {
            int trapDmg = Math.Max(1, (int)(damage * DifficultyConfig.DamageTakenMultiplier));
            Player.TakeDamage(Damage.Pure(trapDmg));
            _lastDamageCause = DeathCause.Trap;
            AddMessage($"{trapDmg}ダメージを受けた！");
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

            case TrapType.Poison:
                AddMessage("毒針が飛び出した！体に毒が回る…");
                break;

            case TrapType.Arrow:
                AddMessage("壁から矢が飛んできた！");
                break;

            case TrapType.Fire:
                AddMessage("足元から炎が噴き上がった！");
                break;

            case TrapType.Sleep:
                AddMessage("睡眠ガスが噴き出した！意識が遠のく…");
                break;

            case TrapType.Confusion:
                AddMessage("混乱ガスが噴き出した！方向感覚が狂う…");
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
        bool nearWater = false;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(Player.Position.X + dx, Player.Position.Y + dy);
                if (!Map.IsInBounds(pos)) continue;

                var tile = Map.GetTile(pos);

                // 水辺判定（釣り用）
                if (tile.Type == TileType.Water || tile.Type == TileType.DeepWater)
                {
                    nearWater = true;
                }

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

        // 水辺での釣り（15%の確率で釣りに移行 — 50%は高すぎて意図せず発動する）
        if (nearWater && !found && _random.NextDouble() < 0.15)
        {
            AddMessage("🔍 水辺を発見した。釣りを試みる...");
            TryFish();
            return true;
        }
        else if (nearWater && !found)
        {
            AddMessage("🔍 近くに水辺がある。もう一度探索すれば釣りができるかもしれない。");
        }

        // フィールドでの採集（15%の確率で採集に移行）
        if (_isLocationField && !found && _random.NextDouble() < 0.15)
        {
            var currentTile = Map.GetTile(Player.Position);
            var gatherType = currentTile.Type switch
            {
                TileType.Grass => GatheringType.Herb,
                TileType.Tree => GatheringType.Logging,
                _ => GatheringType.Foraging
            };
            AddMessage("🔍 周囲を探索して採集を試みる...");
            TryGather(gatherType);
            return true;
        }
        else if (_isLocationField && !found)
        {
            AddMessage("🔍 このあたりで採集できそうな場所がある。もう一度探してみよう。");
        }

        // ダンジョン内での安全確認（自動仮眠は削除 — 探索コマンドで眠るのは不自然）
        if (!_worldMapSystem.IsOnSurface && !_isInLocationMap && !found && !IsInCombat()
            && !nearWater && !_isLocationField)
        {
            AddMessage("🔍 周囲の安全を確認した。敵の気配はない。");
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
        // EC-4: 死亡後の攻撃禁止
        if (!Player.IsAlive) return false;

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

        // AL-1: 天候による遠距離攻撃命中修正（ダメージに反映）
        float rangedWeatherMod = WeatherSystem.GetRangedHitModifier(CurrentWeather);
        if (rangedWeatherMod < 1.0f && result.IsHit && result.Damage.HasValue)
        {
            var dmg = result.Damage.Value;
            int modifiedAmount = Math.Max(1, (int)(dmg.Amount * rangedWeatherMod));
            var modifiedDmg = new Damage(modifiedAmount, dmg.Type, dmg.Element, dmg.IsCritical, dmg.SourceName);
            result = new CombatResult(result.IsHit, result.IsCritical, modifiedDmg, result.Attacker, result.Target);
            AddMessage($"🌧 天候の影響で遠距離攻撃の威力が低下（×{rangedWeatherMod:P0}）");
        }

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? " クリティカル！" : "";
            AddMessage($"🏹 {target.Name}に{result.Damage?.Amount ?? 0}ダメージ！（距離{distance}）{critStr}");

            if (!target.IsAlive)
            {
                int gold = CalculateGoldReward(target);
                if (gold > 0) Player.AddGold(gold);
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
                int gold = CalculateGoldReward(target);
                if (gold > 0) Player.AddGold(gold);
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

        // EC-3: 死亡後のスキル使用禁止
        if (!Player.IsAlive) return false;

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

    /// <summary>スキルスロットにスキルを割り当て（0-5のインデックス、1-6キーに対応）</summary>
    public bool AssignSkillSlot(int slotIndex, string skillId)
    {
        if (slotIndex < 0 || slotIndex >= SkillTreeSystem.MaxSkillSlots) return false;
        if (!Player.LearnedSkills.Contains(skillId)) return false;
        var skill = SkillDatabase.GetById(skillId);
        if (skill == null || skill.Category == SkillCategory.Passive) return false;

        // SkillTreeSystem 側にノード未登録の場合は登録＆アンロックしてから装備
        var tree = _skillTreeSystem;
        if (!tree.UnlockedNodes.Contains(skillId))
        {
            if (!tree.AllNodes.ContainsKey(skillId))
            {
                tree.RegisterNode(new SkillNodeDefinition(
                    skillId, skill.Name, skill.Description ?? "",
                    SkillNodeType.Active, null, 0, Array.Empty<string>(),
                    new Dictionary<string, int>()));
            }
            tree.AddPoints(1);
            tree.UnlockNode(skillId);
        }

        // 既存スロットに同じスキルがある場合は既に装備済み
        if (tree.EquippedSkillSlots.Contains(skillId))
            return true;

        // 指定スロットに別スキルがある場合は先に外す
        if (slotIndex < tree.EquippedSkillSlots.Count)
            tree.UnequipSkillSlot(slotIndex);

        tree.EquipSkillToSlot(skillId);
        AddMessage($"スロット{slotIndex + 1}に{skill.Name}を割り当てた");
        return true;
    }

    /// <summary>スキルスロットの割り当てを解除</summary>
    public void ClearSkillSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _skillTreeSystem.EquippedSkillSlots.Count)
            _skillTreeSystem.UnequipSkillSlot(slotIndex);
    }

    /// <summary>スキルスロットの情報を取得</summary>
    public IReadOnlyList<string?> GetSkillSlots()
    {
        var equipped = _skillTreeSystem.EquippedSkillSlots;
        var result = new string?[SkillTreeSystem.MaxSkillSlots];
        for (int i = 0; i < equipped.Count && i < result.Length; i++)
            result[i] = equipped[i];
        return result;
    }

    /// <summary>スキルスロット使用後のターン進行（MainWindowから呼ぶ）</summary>
    public void AdvanceTurnFromSkillSlot(int actionCost)
    {
        // 状態異常によるターンコスト修正（麻痺: 1.5倍等）
        float turnModifier = Player.GetStatusEffectTurnModifier();
        if (turnModifier > 1.0f && turnModifier < float.MaxValue)
        {
            actionCost = (int)Math.Ceiling(actionCost * turnModifier);
        }

        // タスク55: 疲労段階による行動コスト加算
        int fatigueCostBonus = FatigueSystem.GetActionCostBonus(Player.FatigueStage);
        if (fatigueCostBonus > 0)
        {
            actionCost += fatigueCostBonus;
        }

        // B.53: 渇き段階による行動コスト加算
        int thirstCostBonus = ThirstSystem.GetThirstActionCostBonus(Player.ThirstStage);
        if (thirstCostBonus > 0)
        {
            actionCost += thirstCostBonus;
        }

        // B.57: 渇き（小）の30%確率で行動コスト+1
        if (Player.ThirstStage == ThirstStage.SlightlyThirsty && _random.Next(100) < 30)
        {
            actionCost += 1;
        }

        int finalCost = Math.Max(1, actionCost);

        // タスク52: 疲労度蓄積（スキル使用）
        FatigueSystem.AccumulateFatigue(Player, finalCost, isSkill: true);

        TurnCount += finalCost;
        GameTime.AdvanceTurn(finalCost);

        // タスク61: 気付け薬の残りターン消費
        if (Player.TickFatigueRestrictionRelief(finalCost))
        {
            AddMessage("😓 気付け薬の効果が切れた…体が重い。");
        }

        ProcessEnemyTurns();
        ProcessTurnEffects();
        CheckTurnLimitWarnings();
        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());

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
        var equipped = _skillTreeSystem.EquippedSkillSlots;
        if (slotIndex < 0 || slotIndex >= SkillTreeSystem.MaxSkillSlots) return false;
        var skillId = slotIndex < equipped.Count ? equipped[slotIndex] : null;
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
                    int gold = CalculateGoldReward(target);
                    if (gold > 0) Player.AddGold(gold);
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
                        int gold = CalculateGoldReward(target);
                        if (gold > 0) Player.AddGold(gold);
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
                    int gold = CalculateGoldReward(target);
                    if (gold > 0) Player.AddGold(gold);
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
                        int gold = CalculateGoldReward(target);
                        if (gold > 0) Player.AddGold(gold);
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
        // α.20: 詠唱演出テキストを追加
        var currentIncantation = _spellCastingSystem.CurrentIncantation;
        if (currentIncantation.Count > 0)
        {
            var effectWord = currentIncantation.FirstOrDefault(w => true) ?? "";
            var elementWord = currentIncantation.Count > 1 ? currentIncantation.Skip(1).FirstOrDefault() : null;
            var castingText = RougelikeGame.Core.Data.RuneWordLoreData.GetCastingText(effectWord, elementWord);
            if (!string.IsNullOrEmpty(castingText))
                AddMessage(castingText);
        }
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
        // BS-13: 回復魔法のINT/MNDスケーリング
        if (effect.Type == SpellEffectType.Heal)
        {
            float mndScale = 1.0f + (Player.EffectiveStats.Mind - 10) * 0.03f;
            float intScale = 1.0f + (Player.EffectiveStats.Intelligence - 10) * 0.02f;
            float healMultiplier = Math.Max(0.5f, mndScale * intScale);
            effect = effect with { Power = (int)(effect.Power * healMultiplier) };
        }
        // BS-13: ダメージ魔法のINTスケーリング
        else if (effect.Type == SpellEffectType.Damage)
        {
            float intScale = 1.0f + (Player.EffectiveStats.Intelligence - 10) * 0.025f;
            effect = effect with { Power = (int)(effect.Power * Math.Max(0.5f, intScale)) };
        }

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
                    // 地表面×属性魔法の相互作用（EnvironmentalCombatSystem）
                    int finalDamage = ApplyEnvironmentalInteraction(target.Position, effect.Element, damage);
                    target.TakeDamage(Damage.Magical(finalDamage, effect.Element));
                    AddMessage($"{target.Name}に{finalDamage}の{effect.Element}ダメージ！");
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
                    // 地表面×属性魔法の相互作用（EnvironmentalCombatSystem）
                    int areaDamage = ApplyEnvironmentalInteraction(enemy.Position, effect.Element, damage);
                    enemy.TakeDamage(Damage.Magical(areaDamage, effect.Element));
                    AddMessage($"{enemy.Name}に{areaDamage}の{effect.Element}ダメージ！");
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
                {
                    int oldHp = Player.CurrentHp;
                    Player.Heal(healAmount);
                    int actualHeal = Player.CurrentHp - oldHp;
                    AddMessage($"HPが{actualHeal}回復した");
                }
                break;

            case SpellTargetType.AllAllies:  // BY-1/BY-8: 全味方回復
                {
                    int oldHp = Player.CurrentHp;
                    Player.Heal(healAmount);
                    int actualHeal = Player.CurrentHp - oldHp;
                    AddMessage($"HPが{actualHeal}回復した");
                    // コンパニオンも回復
                    foreach (var companion in _companionSystem.Party.Where(c => c.IsAlive))
                    {
                        _companionSystem.HealCompanion(companion.Name, healAmount);
                        AddMessage($"{companion.Name}のHPが回復した");
                    }
                    // ペットも回復
                    foreach (var petId in _petSystem.Pets.Keys.ToList())
                    {
                        _petSystem.HealPet(petId, healAmount);
                    }
                }
                break;

            case SpellTargetType.SingleAlly:  // BY-8: 単体味方回復
                {
                    // 最もHP割合が低い味方を回復（プレイヤー含む）
                    // B.27: MaxHpが0以下の場合の除算ゼロ対策
                    var lowestCompanion = _companionSystem.Party.Where(c => c.IsAlive && c.MaxHp > 0)
                        .OrderBy(c => (float)c.Hp / c.MaxHp).FirstOrDefault();
                    float playerHpRatio = Player.MaxHp > 0 ? (float)Player.CurrentHp / Player.MaxHp : 1f;

                    if (lowestCompanion != null && (float)lowestCompanion.Hp / lowestCompanion.MaxHp < playerHpRatio)
                    {
                        _companionSystem.HealCompanion(lowestCompanion.Name, healAmount);
                        AddMessage($"{lowestCompanion.Name}のHPが回復した");
                    }
                    else
                    {
                        int oldHp2 = Player.CurrentHp;
                        Player.Heal(healAmount);
                        int actualHeal2 = Player.CurrentHp - oldHp2;
                        AddMessage($"HPが{actualHeal2}回復した");
                    }
                }
                break;
        }
    }

    /// <summary>魔法浄化の適用</summary>
    private void ApplySpellPurify(SpellEffect effect)
    {
        var negativeEffects = Player.StatusEffects
            .Where(e => e.Type is StatusEffectType.Poison or StatusEffectType.Burn
                or StatusEffectType.Freeze or StatusEffectType.Paralysis
                or StatusEffectType.Stun or StatusEffectType.Blind or StatusEffectType.Confusion)
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

        // BY-2: AllAlliesターゲットの場合、仲間にもバフを適用
        void ApplyBuffToCharacter(Character target, string targetName)
        {
            switch (effect.Type)
            {
                case SpellEffectType.Speed:
                    target.ApplyStatusEffect(new StatusEffect(StatusEffectType.Haste, duration)
                    {
                        Name = "加速",
                        TurnCostModifier = 0.75f
                    });
                    break;
                case SpellEffectType.Blessing:
                    target.ApplyStatusEffect(new StatusEffect(StatusEffectType.Blessing, duration)
                    {
                        Name = "祝福"
                    });
                    break;
                case SpellEffectType.Buff:
                default:
                    target.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, duration)
                    {
                        Name = "強化",
                        AttackMultiplier = 1.25f
                    });
                    target.ApplyStatusEffect(new StatusEffect(StatusEffectType.Protection, duration)
                    {
                        Name = "防護",
                        DefenseMultiplier = 1.50f
                    });
                    break;
            }
        }

        string effectName = effect.Type switch
        {
            SpellEffectType.Speed => "加速",
            SpellEffectType.Blessing => "祝福",
            _ => "能力強化"
        };

        if (effect.TargetType == SpellTargetType.AllAllies || effect.TargetType == SpellTargetType.All)
        {
            // プレイヤーにバフ + 仲間の数をカウント
            ApplyBuffToCharacter(Player, "あなた");
            int allyCount = 1 + _companionSystem.Party.Count(c => c.IsAlive);
            AddMessage($"{effectName}の魔法が味方全体に発動した（{allyCount}体、{duration}ターン）");
        }
        else
        {
            // 自分のみ
            ApplyBuffToCharacter(Player, "あなた");
            AddMessage($"{effectName}の魔法が発動した（{duration}ターン）");
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
            // BY-3: AllAllies/Self — 味方への制御魔法は防護効果として適用
            case SpellTargetType.AllAllies:
            case SpellTargetType.Self:
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Protection, duration)
                {
                    Name = "制御結界",
                    DefenseMultiplier = 1.30f
                });
                if (effect.TargetType == SpellTargetType.AllAllies)
                {
                    int allyCount = 1 + _companionSystem.Party.Count(c => c.IsAlive);
                    AddMessage($"味方全体に制御結界が展開された（{allyCount}体、{duration}ターン）");
                }
                else
                {
                    AddMessage($"制御結界を展開した（{duration}ターン）");
                }
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
        summoned.SummonRemainingTurns = 20;  // CC-10: 召喚クリーチャーは20ターン持続
        Enemies.Add(summoned);
        AddMessage($"{summoned.Name}を召喚した！（残り20ターン）");
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
                or StatusEffectType.Stun or StatusEffectType.Blind or StatusEffectType.Confusion
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
            // BY-4: AllAllies/Self — 味方への封印は魔法無効化バリア
            case SpellTargetType.AllAllies:
            case SpellTargetType.Self:
                Player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Protection, duration)
                {
                    Name = "魔法障壁",
                    DefenseMultiplier = 1.50f
                });
                if (effect.TargetType == SpellTargetType.AllAllies)
                {
                    int allyCount = 1 + _companionSystem.Party.Count(c => c.IsAlive);
                    AddMessage($"味方全体に魔法障壁を展開した（{allyCount}体、{duration}ターン）");
                }
                else
                {
                    AddMessage($"魔法障壁を展開した（{duration}ターン）");
                }
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
                or StatusEffectType.Stun or StatusEffectType.Blind or StatusEffectType.Confusion
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

    /// <summary>ルーン碑文タイルを踏んだ時の自動解読処理</summary>
    private void TryReadRuneInscription(Tile tile)
    {
        if (string.IsNullOrEmpty(tile.InscriptionWordId))
        {
            AddMessage("ᚱ ルーン碑文がある（解読できない文字が刻まれている）");
            return;
        }

        if (tile.InscriptionRead)
        {
            var knownWord = RuneWordDatabase.GetById(tile.InscriptionWordId);
            var wordName = knownWord != null ? $"「{knownWord.Meaning}（{knownWord.OldNorse}）」" : "既知のルーン語";
            AddMessage($"ᚱ 解読済みのルーン碑文がある — {wordName}");
            return;
        }

        var result = VocabularyAcquisitionSystem.LearnFromRuneStone(Player, tile.InscriptionWordId);
        tile.InscriptionRead = true;
        AddMessage($"ᚱ {result.Message}");
        OnStateChanged?.Invoke();
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
            // 宗教スキルをスキルツリーに登録（習得済みスキルタブに表示される）
            _skillTreeSystem.RegisterReligionSkills(religionId);
            OnReligionChanged?.Invoke();
        }

        return result;
    }

    /// <summary>宗教を脱退する</summary>
    private bool TryLeaveReligion()
    {
        // 脱退前に現在の宗教IDを保存（スキルツリーから除去するため）
        var currentReligionStr = Player.CurrentReligion;
        var result = _religionSystem.LeaveReligion(Player);
        AddMessage(result.Message);

        if (result.Success)
        {
            // 宗教スキルをスキルツリーから除去
            if (currentReligionStr != null && Enum.TryParse<ReligionId>(currentReligionStr, out var oldReligionId))
            {
                _skillTreeSystem.RemoveReligionSkills(oldReligionId);
            }

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
            // α.16: 宗教別フレーバーテキストで禁忌メッセージを強化
            if (Enum.TryParse<ReligionId>(Player.CurrentReligion, out var religionId))
            {
                var tabooName = result.Message.Contains("「") && result.Message.Contains("」")
                    ? result.Message[(result.Message.IndexOf('「') + 1)..result.Message.IndexOf('」')]
                    : "禁忌";
                AddMessage(RougelikeGame.Core.Data.ReligionLoreData.GetTabooViolationText(religionId, tabooName));
            }
            else
            {
                AddMessage(result.Message);
            }
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

    /// <summary>ロケーションマップ（町・フィールド）にいるか</summary>
    public bool IsInLocationMap => _isInLocationMap;

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

    /// <summary>
    /// 領地間移動を実行。
    /// A2: ワールドマップからの直接移動は廃止。関所（BorderGate）経由のみ許可。
    /// このメソッドは後方互換性のために残すが、関所案内メッセージを表示する。
    /// </summary>
    public bool TryTravelTo(TerritoryId destination)
    {
        // A2: ワールドマップ廃止→関所NPC統一
        AddMessage("📍 領地間の移動は関所（BorderGate）のNPCにインタラクトして行ってください。");
        AddMessage("関所はシンボルマップの端に配置されています。");
        return false;
    }

    /// <summary>関所経由の旅路イベントを解決する（A2: 関所NPC統一）</summary>
    private void ResolveBorderGateTravelEvent(TravelEvent travelEvent)
    {
        switch (travelEvent.Type)
        {
            case TravelEventType.Merchant:
                AddMessage("旅の商人から品物を見せてもらえそうだ。（Bキーで商店）");
                break;
            case TravelEventType.Ambush:
                AddMessage("⚠ 待ち伏せだ！戦闘に備えろ！");
                SpawnEnemies();
                break;
            case TravelEventType.TreasureChest:
                var lootItem = _itemFactory.GenerateRandomItem(CurrentFloor);
                if (lootItem != null)
                {
                    GroundItems.Add((lootItem, Player.Position));
                    AddMessage($"💎 宝箱を発見！{lootItem.GetDisplayName()}が見つかった！");
                }
                break;
            case TravelEventType.Shrine:
                Player.Heal(Player.MaxHp / 4);
                AddMessage("🏛 祠で体力を回復した");
                break;
            case TravelEventType.HelpRequest:
                AddMessage("救援依頼を受けた。近くに困っている人がいるようだ。");
                break;
            case TravelEventType.BadWeather:
                AddMessage("🌧 悪天候に見舞われ、移動に時間がかかった");
                break;
        }
    }

    /// <summary>街・施設・宗教施設・フィールド・地形タイルに入る（ロケーションマップ遷移）</summary>
    private bool TryEnterTown()
    {
        if (!_worldMapSystem.IsOnSurface)
        {
            AddMessage("すでにロケーション内にいる");
            return false;
        }

        // シンボルマップ上のロケーションを判定
        var location = _symbolMapSystem.GetLocationAt(Player.Position);

        // 関所の場合は領地遷移処理（A2: 関所NPC統一、ここが唯一の領地間移動手段）
        if (location != null && location.Type == LocationType.BorderGate)
        {
            var targetTerritory = location.GetBorderGateTarget();
            if (targetTerritory.HasValue)
            {
                // BM-2: 評判が「嫌悪」の場合、領地への入場を拒否
                if (!_reputationSystem.IsWelcome(targetTerritory.Value))
                {
                    AddMessage($"⛔ 関所の番兵：「{targetTerritory.Value}の住人からの通達で、お前の通行は拒否する。」");
                    return false;
                }

                // 通行条件チェック（手配/通行料/戦争状態）
                _worldMapSystem.PlayerGold = Player.Gold;
                if (!_worldMapSystem.CanTravelTo(targetTerritory.Value, Player.Level))
                {
                    string reason = _worldMapSystem.GetTravelDeniedReason(targetTerritory.Value, Player.Level);
                    AddMessage($"🏰 関所の番兵：「{reason}」");
                    return false;
                }

                var targetDef = TerritoryDefinition.Get(targetTerritory.Value);

                // NPCインタラクト風の演出
                AddMessage($"🏰 関所の番兵：「{targetDef.Name}への通行ですね。通行料は{WorldMapSystem.BorderGateToll}Gになります。」");

                // 通行料を差し引き
                Player.AddGold(-WorldMapSystem.BorderGateToll);
                _worldMapSystem.PlayerGold = Player.Gold;
                AddMessage($"【{location.Name}】─ 通行料{WorldMapSystem.BorderGateToll}Gを支払い、{targetDef.Name}へ向かう…");

                // 旅路イベント判定（A2: 関所経由の移動でも旅路イベント発生）
                var fromTerritory = _worldMapSystem.CurrentTerritory;
                var travelEvent = _worldMapSystem.RollTravelEvent(fromTerritory, targetTerritory.Value, _random);
                if (travelEvent != null)
                {
                    AddMessage($"【旅路イベント】{travelEvent.Name}: {travelEvent.Description}");
                    ResolveBorderGateTravelEvent(travelEvent);
                }

                // ターンコスト適用（移動日数）
                int travelTurnCost = targetDef.TravelTurnCost;
                TurnCount += travelTurnCost;
                GameTime.AdvanceTurn(travelTurnCost);

                // 領地を切り替え
                _worldMapSystem.SetTerritory(targetTerritory.Value);
                _worldMapSystem.VisitedTerritories.Add(targetTerritory.Value);
                _currentDungeonFeature = null;

                // ショップ在庫リセット
                _shopSystem.ClearShopInventory();

                // 新しい領地のシンボルマップを生成
                GenerateSymbolMap();

                _currentAmbientSound = AmbientSoundSystem.GetAmbientForTerritory(targetTerritory.Value);
                AddMessage($"🏰 関所の番兵：「ようこそ{targetDef.Name}へ。お気をつけて。」");
                OnTerritoryChanged?.Invoke(targetTerritory.Value);
                AddMessage(RougelikeGame.Core.Data.TerritoryLoreData.GetArrivalDescription(targetTerritory.Value));
                OnStateChanged?.Invoke();
                return true;
            }
            else
            {
                AddMessage("関所の行き先が不明です");
                return false;
            }
        }

        // ダンジョンの場合はダンジョン入場処理（通常ダンジョン、野盗のねぐら、ゴブリンの巣）
        if (location != null && location.Type is LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest)
        {
            _worldMapSystem.IsOnSurface = false;
            _currentMapName = location.Id;
            _currentDungeonMinLevel = location.MinLevel;
            _currentDungeonMaxFloor = location.MaxFloor;
            CurrentFloor = 1;
            GenerateFloor();
            var featureName = GetCurrentDungeonFeatureName();
            var featureStr = !string.IsNullOrEmpty(featureName) ? $"（{featureName}）" : "";
            AddMessage($"【{location.Name}】─ ダンジョン第{CurrentFloor}層に足を踏み入れた...{featureStr}");
            OnSymbolMapEnterDungeon?.Invoke(location);
            OnStateChanged?.Invoke();
            return true;
        }

        // ロケーションがある場合（町・施設・フィールド等）
        if (location != null)
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
        _isLocationField = location.Type == LocationType.Field;
        _currentMapName = location.Id;

        Map = locationMap;
        var startPos = locationMap.EntrancePosition ?? locationMap.StairsUpPosition
            ?? locationMap.GetRandomWalkablePosition(_random) ?? new Position(5, 5);
        Player.Position = startPos;

        Enemies.Clear();
        GroundItems.Clear();

        // フィールドのみ敵を配置
        if (location.Type == LocationType.Field)
        {
            SpawnEnemies();
            Map.ComputeFov(Player.Position, GetEffectiveViewRadius());
        }
        else
        {
            // 町・村など非フィールドは全タイル可視
            Map.RevealAll();
        }

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
        _isLocationField = true;
        _currentMapName = fieldMap.Name;

        Map = fieldMap;
        var startPos = fieldMap.EntrancePosition ?? new Position(fieldMap.Width / 2, fieldMap.Height - 1);
        Player.Position = startPos;

        Enemies.Clear();
        GroundItems.Clear();

        // フィールドマップには敵を配置
        SpawnEnemies();

        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());

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
        TileType.NpcInnkeeper or
        TileType.NpcTrainer or
        TileType.NpcLibrarian;

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
                        new DialogueChoice("転職する", "action:class_change_menu"),
                        new DialogueChoice("スキル融合", "action:fuse_skills_auto"),
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
                        new DialogueChoice("投資する", "action:invest_shop"),
                        new DialogueChoice("裏の商品を見る", "action:black_market_browse"),
                        new DialogueChoice("密輸を依頼する", "action:smuggle"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcBlacksmith
                => ("鍛冶屋", "おう、いらっしゃい！武器や防具なら任せろ！",
                    new[]
                    {
                        new DialogueChoice("武器を見る", "action:open_shop_WeaponShop"),
                        new DialogueChoice("防具を見る", "action:open_shop_ArmorShop"),
                        new DialogueChoice("装備を鍛える", "action:open_crafting"),
                        new DialogueChoice("装備を修理する（最も破損した装備）", "action:smith_repair_auto"),
                        new DialogueChoice("罠を製作する", "action:craft_trap_menu"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcInnkeeper
                => ("宿屋主人", "疲れたら休んでいきなさい。食料もあるよ。",
                    new[]
                    {
                        new DialogueChoice("宿に泊まる", "action:use_inn"),
                        new DialogueChoice("料理する", "action:cook"),
                        new DialogueChoice("食料を買う", "action:open_shop_GeneralShop"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcTrainer
                => ("訓練師", $"鍛錬を積めば強くなれるぞ。スキルポイントを使って技を磨くか？（所持SP: {_skillTreeSystem.AvailablePoints}）",
                    new[]
                    {
                        new DialogueChoice("スキルツリーを開く", "action:open_skill_tree"),
                        new DialogueChoice("戦闘訓練を受ける（50G）", "action:train_combat", 3),
                        new DialogueChoice("転職する", "action:class_change_menu"),
                        new DialogueChoice("立ち去る", "action:close")
                    }),
            TileType.NpcLibrarian
                => ("図書館司書", "知識は力なり。魔法の書を読んで学びたまえ。",
                    new[]
                    {
                        new DialogueChoice("魔法を学ぶ（100G）", "action:learn_magic", 5),
                        new DialogueChoice("ルーン語を学ぶ（150G）", "action:learn_rune_word"),
                        new DialogueChoice("エンチャントを依頼する", "action:enchant_menu"),
                        new DialogueChoice("パズルに挑戦する", "action:attempt_puzzle_menu"),
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
    /// <summary>建物内部マップに遷移</summary>
    private void EnterBuilding(string buildingId, Position entrancePos)
    {
        // 町マップの状態を保存（建物内から別建物に移動する場合は町マップ情報を維持）
        if (_buildingReturnMap == null)
        {
            _buildingReturnMap = Map;
            _buildingReturnPosition = Player.Position;
        }
        _currentBuildingId = buildingId;

        // 訪問済み建物として記録
        _visitedBuildings.Add(buildingId);

        // 建物内部マップを生成（訪問済み建物リストを渡して他建物への階段を配置）
        var generator = new LocationMapGenerator();
        var interiorMap = generator.GenerateBuildingInterior(buildingId, _visitedBuildings.ToList());

        Map = interiorMap;
        var startPos = interiorMap.EntrancePosition ?? new Position(interiorMap.Width / 2, interiorMap.Height - 2);
        Player.Position = startPos;

        Enemies.Clear();

        // 建物内部は全タイル可視にする
        Map.RevealAll();

        var buildingName = GetBuildingDisplayName(buildingId);
        AddMessage($"【{buildingName}】に入った");
        OnStateChanged?.Invoke();
    }

    /// <summary>建物内部から町マップに戻る</summary>
    private void ExitBuilding()
    {
        if (_buildingReturnMap == null || _buildingReturnPosition == null)
        {
            AddMessage("戻り先が見つからない");
            return;
        }

        Map = _buildingReturnMap;
        Player.Position = _buildingReturnPosition.Value;

        var buildingName = GetBuildingDisplayName(_currentBuildingId ?? "");
        AddMessage($"【{buildingName}】から出た");

        _buildingReturnMap = null;
        _buildingReturnPosition = null;
        _currentBuildingId = null;

        OnStateChanged?.Invoke();
    }

    /// <summary>建物IDの日本語表示名を返す</summary>
    private static string GetBuildingDisplayName(string buildingId) => buildingId switch
    {
        "inn" => "宿屋",
        "shop" => "商店",
        "smithy" => "鍛冶屋",
        "guild" => "冒険者ギルド",
        "church" => "教会",
        "training" => "訓練所",
        "library" => "図書館",
        "magic_shop" => "魔法商店",
        _ => buildingId.StartsWith("village_") ? "民家" : "建物"
    };

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

        _worldMapSystem.IsOnSurface = true;
        _isInLocationMap = false;
        _isLocationField = false;
        _visitedBuildings.Clear();
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

        // CZ-4: 建設ボーナスによる宿泊費割引
        float recoveryBonus = _baseConstructionSystem.GetRestHpRecoveryMultiplier();
        int innCost = recoveryBonus > 1.0f ? Math.Max(10, (int)(50 / recoveryBonus)) : 50;

        var result = _townSystem.RestAtInn(Player, innCost);
        AddMessage(result.Message);

        if (result.Success && result.TurnCost > 0)
        {
            TurnCount += result.TurnCost;
            GameTime.AdvanceTurn(result.TurnCost);

            // 疲労回復（宿屋: 段階に応じた開始値から回復）
            var innFatigueStart = FatigueSystem.GetInnRecoveryStart(Player.FatigueStage);
            Player.ModifyFatigue(innFatigueStart - Player.Fatigue);
            // 衛生回復（宿屋で清潔に）
            Player.ModifyHygiene(GameConstants.MaxHygiene - Player.Hygiene);
            // 渇き回復（宿泊時に水分補給）
            Player.ModifyThirst(GameConstants.MaxThirst - Player.Thirst);

            AddMessage("💤 疲労・衛生・渇きが回復した");
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

        // BL-3: カルマによる聖地進入制限
        if (!_karmaSystem.CanEnterHolyGround())
        {
            AddMessage("⛪ あなたの邪悪な行いが知れ渡っている。教会への立ち入りを拒否された");
            return false;
        }

        var result = _townSystem.RemoveCurseAtChurch(Player);
        AddMessage(result.Message);
        OnStateChanged?.Invoke();
        return result.Success;
    }

    /// <summary>訓練師から戦闘訓練を受ける（50G → スキルポイント1獲得）</summary>
    private void TryTrainCombat()
    {
        const int trainingCost = 50;
        if (Player.Gold < trainingCost)
        {
            AddMessage($"訓練費用が足りない（必要: {trainingCost}G、所持: {Player.Gold}G）");
            return;
        }

        Player.SpendGold(trainingCost);
        _skillTreeSystem.AddPoints(1);
        AddMessage($"⚔ 訓練師のもとで鍛錬を積んだ！ スキルポイント+1（残り: {_skillTreeSystem.AvailablePoints}SP）");
        OnStateChanged?.Invoke();
    }

    /// <summary>図書館で魔法を学ぶ（100G → スキルポイント2獲得）</summary>
    private void TryLearnMagic()
    {
        const int learningCost = 100;
        if (Player.Gold < learningCost)
        {
            AddMessage($"学習費用が足りない（必要: {learningCost}G、所持: {Player.Gold}G）");
            return;
        }

        Player.SpendGold(learningCost);
        _skillTreeSystem.AddPoints(2);
        AddMessage($"📖 古い魔導書を読み解いた！ スキルポイント+2（残り: {_skillTreeSystem.AvailablePoints}SP）");
        OnStateChanged?.Invoke();
    }

    /// <summary>図書館でルーン語を学ぶ（150G → 難度に応じたランダム1語を習得）</summary>
    private void TryLearnRuneWord()
    {
        const int learningCost = 150;
        if (Player.Gold < learningCost)
        {
            AddMessage($"学習費用が足りない（必要: {learningCost}G、所持: {Player.Gold}G）");
            return;
        }

        // INTに応じて学べる最大難度を決定（INT 5→難度2, INT 10→難度3, INT 15→難度4, INT 20→難度5）
        int maxDifficulty = Math.Clamp(1 + Player.EffectiveStats.Intelligence / 5, 1, 5);
        var result = VocabularyAcquisitionSystem.LearnRandomWord(Player, maxDifficulty, _random);
        if (!result.Success)
        {
            AddMessage("📖 学べる新しいルーン語が見つからなかった（より高い知力が必要かもしれない）");
            return;
        }

        Player.SpendGold(learningCost);
        AddMessage($"📖 {result.Message}");
        OnStateChanged?.Invoke();
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
        // 購入前にインベントリ空きを確認（スタック可能アイテムは既存スタックへの追加も考慮）
        var shopItems = _shopSystem.GetShopItems(shopType);
        if (index >= 0 && index < shopItems.Count)
        {
            var previewItem = ItemDefinitions.Create(shopItems[index].ItemId);
            if (previewItem != null)
            {
                var inv = (Inventory)Player.Inventory;
                bool canStack = previewItem is IStackable stackable
                    && inv.Items.OfType<IStackable>().Any(s => s.CanStackWith(stackable));
                if (!canStack && inv.UsedSlots >= inv.MaxSlots)
                {
                    AddMessage("インベントリが一杯のため、購入できない");
                    OnStateChanged?.Invoke();
                    return false;
                }
            }
        }

        double discount = ShopSystem.CalculateCharismaDiscount(Player.EffectiveStats.Charisma);

        // === GUI統合: 価格変動（PriceFluctuationSystem）===
        float reputationMod = PriceFluctuationSystem.GetReputationModifier(PlayerReputationRank, true);
        float karmaMod = PriceFluctuationSystem.GetKarmaModifier(PlayerKarmaRank, true);
        float territoryMod = PriceFluctuationSystem.GetTerritoryModifier(_worldMapSystem.CurrentTerritory, "general");
        // BM-1: ReputationSystem評判割引を統合
        double reputationDiscount = _reputationSystem.GetShopDiscount(_worldMapSystem.CurrentTerritory);
        discount *= (double)(reputationMod * karmaMod * territoryMod) * reputationDiscount;

        // RelationshipSystem: NPC好感度による追加割引
        float npcDiscount = GetNpcShopDiscount(shopType.ToString());
        if (npcDiscount > 0) discount *= (1.0 - npcDiscount);

        // BZ-10: 商人ギルドランクによる割引適用
        float guildDiscount = _merchantGuildSystem.GetGuildDiscount();
        if (guildDiscount > 0) discount *= (1.0 - guildDiscount);

        // α.26c: 地域図鑑完全攻略による割引（10%）
        float encyclopediaShopDiscount = _encyclopediaSystem.GetRegionCompleteShopDiscount();
        if (encyclopediaShopDiscount > 0) discount *= (1.0 - encyclopediaShopDiscount);

        var result = _shopSystem.Buy(Player, shopType, index, discount);
        if (result.Success && result.ItemId is not null)
        {
            var newItem = ItemDefinitions.Create(result.ItemId);
            if (newItem != null)
            {
                var inventory = (Inventory)Player.Inventory;
                if (!inventory.Add(newItem))
                {
                    // 購入成功したがインベントリ追加失敗 → ゴールドを返却
                    Player.AddGold(Math.Max(1, (int)(shopItems[index].BasePrice * (1.0 - discount))));
                    AddMessage("インベントリが一杯のため、アイテムを受け取れなかった（返金済み）");
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

        // === GUI統合: 売却価格変動（領地修飾子も含む — 購入と同じ基準で公平性を確保） ===
        float reputationMod = PriceFluctuationSystem.GetReputationModifier(PlayerReputationRank, false);
        float karmaMod = PriceFluctuationSystem.GetKarmaModifier(PlayerKarmaRank, false);
        float territoryMod = PriceFluctuationSystem.GetTerritoryModifier(_worldMapSystem.CurrentTerritory, "general");
        // BM-1: ReputationSystem評判割引を売却にも統合（逆数で高評判→高売値）
        double reputationSellMod = 2.0 - _reputationSystem.GetShopDiscount(_worldMapSystem.CurrentTerritory);
        charismaBonus *= (double)(reputationMod * karmaMod * territoryMod) * reputationSellMod;

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
                _isLocationField = false;
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

        _worldMapSystem.IsOnSurface = false;
        _isInLocationMap = false;
        _isLocationField = false;
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
        _isLocationField = false;
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

        // BZ-3: NPC会話時にTalk/Deliver/Escortクエスト目標を更新
        _questSystem.UpdateObjective(npcId);

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
            // AH-1: NPC記憶に会話の印象を記録
            _npcMemorySystem.RecordAction(npcId, "dialogue", result.AffinityChange, TurnCount);
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
                TurnCount += 3;
                GameTime.AdvanceTurn(3);
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
            case "open_skill_tree":
                OnShowSkillTree?.Invoke();
                break;
            case "open_crafting":
                OnShowCrafting?.Invoke();
                break;
            case "train_combat":
                TryTrainCombat();
                TurnCount += 5;
                GameTime.AdvanceTurn(5);
                break;
            case "learn_magic":
                TryLearnMagic();
                TurnCount += 5;
                GameTime.AdvanceTurn(5);
                break;
            case "learn_rune_word":
                TryLearnRuneWord();
                TurnCount += 5;
                GameTime.AdvanceTurn(5);
                break;
            // === 料理 ===
            case "cook":
                OnShowCooking?.Invoke();
                break;

            // BA-2: ダイアログから料理画面を開く
            case "open_cooking":
                OnShowCooking?.Invoke();
                break;

            // BA-2: ダイアログから鍛冶メニューを開く
            case "open_smithing":
                {
                    var smithNode = new DialogueNode(
                        "npc_smithing_menu",
                        "鍛冶屋",
                        "何を頼みたいんだ？",
                        new[]
                        {
                            new DialogueChoice("装備を修理する（最も破損した装備）", "action:smith_repair_auto"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(smithNode);
                    _dialogueSystem.StartDialogue(smithNode.Id);
                    OnShowDialogue?.Invoke(smithNode);
                }
                break;

            // === 賭博メニュー ===
            case "gamble_menu":
                {
                    var gamblingNode = new DialogueNode(
                        "npc_gambling_menu",
                        "賭博師",
                        $"賭け事に興味があるのかい？ 50Gからだ。（所持金: {Player.Gold}G）",
                        new[]
                        {
                            new DialogueChoice("サイコロ（大小予想）", "action:gamble_dice"),
                            new DialogueChoice("丁半（偶数奇数）", "action:gamble_chohan"),
                            new DialogueChoice("ハイ＆ロー（カード）", "action:gamble_card"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(gamblingNode);
                    _dialogueSystem.StartDialogue(gamblingNode.Id);
                    OnShowDialogue?.Invoke(gamblingNode);
                }
                break;
            case "gamble_dice":
                {
                    var diceChoiceNode = new DialogueNode(
                        "npc_gambling_dice_choice",
                        "賭博師",
                        "サイコロの出目が大(4-6)か小(1-3)か予想しな！",
                        new[]
                        {
                            new DialogueChoice("大 (4-6)", "action:gamble_dice_big"),
                            new DialogueChoice("小 (1-3)", "action:gamble_dice_small"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(diceChoiceNode);
                    _dialogueSystem.StartDialogue(diceChoiceNode.Id);
                    OnShowDialogue?.Invoke(diceChoiceNode);
                }
                break;
            case "gamble_dice_big":
                TryGamble(GamblingGameType.Dice, 50, 4);
                break;
            case "gamble_dice_small":
                TryGamble(GamblingGameType.Dice, 50, 1);
                break;
            case "gamble_chohan":
                {
                    var chohanChoiceNode = new DialogueNode(
                        "npc_gambling_chohan_choice",
                        "賭博師",
                        "丁（偶数）か半（奇数）か、さあ張った張った！",
                        new[]
                        {
                            new DialogueChoice("丁 (偶数)", "action:gamble_chohan_cho"),
                            new DialogueChoice("半 (奇数)", "action:gamble_chohan_han"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(chohanChoiceNode);
                    _dialogueSystem.StartDialogue(chohanChoiceNode.Id);
                    OnShowDialogue?.Invoke(chohanChoiceNode);
                }
                break;
            case "gamble_chohan_cho":
                TryGamble(GamblingGameType.ChoHan, 50, 0);
                break;
            case "gamble_chohan_han":
                TryGamble(GamblingGameType.ChoHan, 50, 1);
                break;
            case "gamble_card":
                {
                    var cardChoiceNode = new DialogueNode(
                        "npc_gambling_card_choice",
                        "賭博師",
                        "次のカードは今より高いか低いか、さあどっちだ！",
                        new[]
                        {
                            new DialogueChoice("ハイ (高い)", "action:gamble_card_high"),
                            new DialogueChoice("ロー (低い)", "action:gamble_card_low"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(cardChoiceNode);
                    _dialogueSystem.StartDialogue(cardChoiceNode.Id);
                    OnShowDialogue?.Invoke(cardChoiceNode);
                }
                break;
            case "gamble_card_high":
                TryGamble(GamblingGameType.Card, 50, 1);
                break;
            case "gamble_card_low":
                TryGamble(GamblingGameType.Card, 50, 0);
                break;

            // === 装備修理（自動: 最も耐久度の低い装備を修理） ===
            case "smith_repair_auto":
                {
                    var damaged = Player.Inventory.Items
                        .OfType<Item>()
                        .Where(i => i.MaxDurability > 0 && i.Durability >= 0 && i.Durability < i.MaxDurability)
                        .OrderBy(i => (float)i.Durability / i.MaxDurability)
                        .FirstOrDefault();
                    if (damaged != null)
                    {
                        TrySmithRepair(damaged.Name, damaged.MaxDurability - damaged.Durability);
                    }
                    else
                    {
                        AddMessage("修理が必要な装備はない");
                    }
                }
                break;

            // === 罠製作メニュー ===
            case "craft_trap_menu":
                {
                    var trapNode = new DialogueNode(
                        "npc_craft_trap_menu",
                        "鍛冶屋",
                        "どんな罠を作りたいんだ？",
                        new[]
                        {
                            new DialogueChoice("棘罠（物理ダメージ）", "action:craft_trap_spike"),
                            new DialogueChoice("落とし穴（移動阻害）", "action:craft_trap_pitfall"),
                            new DialogueChoice("爆発罠（範囲ダメージ）", "action:craft_trap_explosive"),
                            new DialogueChoice("睡眠罠（状態異常）", "action:craft_trap_sleep"),
                            new DialogueChoice("警報罠（敵誘引）", "action:craft_trap_alarm"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(trapNode);
                    _dialogueSystem.StartDialogue(trapNode.Id);
                    OnShowDialogue?.Invoke(trapNode);
                }
                break;
            case "craft_trap_spike":
                TryCraftTrap(PlayerTrapType.SpikeTrap);
                break;
            case "craft_trap_pitfall":
                TryCraftTrap(PlayerTrapType.PitfallTrap);
                break;
            case "craft_trap_explosive":
                TryCraftTrap(PlayerTrapType.ExplosiveTrap);
                break;
            case "craft_trap_sleep":
                TryCraftTrap(PlayerTrapType.SleepTrap);
                break;
            case "craft_trap_alarm":
                TryCraftTrap(PlayerTrapType.AlarmTrap);
                break;

            // === 転職メニュー ===
            case "class_change_menu":
                {
                    var classChoices = new List<DialogueChoice>();
                    foreach (var targetClass in Enum.GetValues<Core.CharacterClass>())
                    {
                        if (targetClass != Player.CharacterClass)
                        {
                            classChoices.Add(new DialogueChoice(
                                $"{targetClass}に転職する",
                                $"action:class_change_{targetClass}"));
                        }
                    }
                    classChoices.Add(new DialogueChoice("やめておく", "action:close"));

                    var classNode = new DialogueNode(
                        "npc_class_change_menu",
                        "ギルド受付",
                        $"転職先を選んでくれ。（現在: {Player.CharacterClass}）",
                        classChoices.ToArray());
                    _dialogueSystem.RegisterNode(classNode);
                    _dialogueSystem.StartDialogue(classNode.Id);
                    OnShowDialogue?.Invoke(classNode);
                }
                break;

            // === スキル融合（融合候補ペアをメニュー提示） ===
            case "fuse_skills_auto":
                {
                    var skills = _skillTreeSystem.UnlockedNodes.ToList();
                    var fusionChoices = new List<DialogueChoice>();
                    if (skills.Count >= 2)
                    {
                        for (int i = 0; i < skills.Count; i++)
                        {
                            for (int j = i + 1; j < skills.Count; j++)
                            {
                                if (SkillFusionSystem.CanFuse(skills[i], skills[j], Player.Level * 3))
                                {
                                    fusionChoices.Add(new DialogueChoice(
                                        $"{skills[i]} + {skills[j]}",
                                        $"action:fuse_{skills[i]}_{skills[j]}"));
                                }
                            }
                        }
                    }
                    if (fusionChoices.Count == 0)
                    {
                        AddMessage("現在融合可能なスキルの組み合わせはない");
                    }
                    else
                    {
                        fusionChoices.Add(new DialogueChoice("やめておく", "action:close"));
                        var fusionNode = new DialogueNode(
                            "npc_fuse_skills_menu",
                            "ギルド受付",
                            "融合可能なスキルの組み合わせがあります。どれを融合しますか？",
                            fusionChoices.ToArray());
                        _dialogueSystem.RegisterNode(fusionNode);
                        _dialogueSystem.StartDialogue(fusionNode.Id);
                        OnShowDialogue?.Invoke(fusionNode);
                    }
                }
                break;

            // === エンチャントメニュー ===
            case "enchant_menu":
                {
                    var enchantChoices = new List<DialogueChoice>();
                    foreach (var enchType in Enum.GetValues<EnchantmentType>())
                    {
                        var info = EnchantmentSystem.GetEnchantmentInfo(enchType);
                        var displayName = info?.Name ?? enchType.ToString();
                        enchantChoices.Add(new DialogueChoice(
                            $"{displayName}",
                            $"action:enchant_{enchType}"));
                    }
                    enchantChoices.Add(new DialogueChoice("やめておく", "action:close"));

                    var enchantNode = new DialogueNode(
                        "npc_enchant_menu",
                        "図書館司書",
                        "どのエンチャントを付与しますか？ 装備中の武器に適用します。",
                        enchantChoices.ToArray());
                    _dialogueSystem.RegisterNode(enchantNode);
                    _dialogueSystem.StartDialogue(enchantNode.Id);
                    OnShowDialogue?.Invoke(enchantNode);
                }
                break;

            // === パズルメニュー ===
            case "attempt_puzzle_menu":
                {
                    var puzzleNode = new DialogueNode(
                        "npc_puzzle_menu",
                        "図書館司書",
                        "知恵試しに挑戦しますか？ どのタイプがよいですか？",
                        new[]
                        {
                            new DialogueChoice("ルーン語パズル", "action:attempt_puzzle_rune"),
                            new DialogueChoice("属性パズル", "action:attempt_puzzle_elemental"),
                            new DialogueChoice("物理パズル", "action:attempt_puzzle_physical"),
                            new DialogueChoice("やめておく", "action:close")
                        });
                    _dialogueSystem.RegisterNode(puzzleNode);
                    _dialogueSystem.StartDialogue(puzzleNode.Id);
                    OnShowDialogue?.Invoke(puzzleNode);
                }
                break;
            case "attempt_puzzle_rune":
                TryAttemptPuzzle(PuzzleType.RuneLanguage);
                break;
            case "attempt_puzzle_elemental":
                TryAttemptPuzzle(PuzzleType.Elemental);
                break;
            case "attempt_puzzle_physical":
                TryAttemptPuzzle(PuzzleType.Physical);
                break;

            // === 闇市場 ===
            case "black_market_browse":
                {
                    var karma = _karmaSystem.KarmaValue;
                    var items = BlackMarketSystem.GetAvailableItems(karma);
                    if (items.Count == 0)
                    {
                        AddMessage("闇市場にアクセスするにはカルマが低い必要がある（現在のカルマでは利用不可）");
                        break;
                    }
                    var bmChoices = new List<DialogueChoice>();
                    foreach (var item in items)
                    {
                        bmChoices.Add(new DialogueChoice(
                            $"{item.Name}（{item.Price}G）",
                            $"action:black_market_buy_{item.Name}"));
                    }
                    bmChoices.Add(new DialogueChoice("やめておく", "action:close"));

                    var bmNode = new DialogueNode(
                        "npc_black_market",
                        "商人",
                        "（小声で）特別な商品がありますよ...",
                        bmChoices.ToArray());
                    _dialogueSystem.RegisterNode(bmNode);
                    _dialogueSystem.StartDialogue(bmNode.Id);
                    OnShowDialogue?.Invoke(bmNode);
                }
                break;

            // === 密輸（TrySmuggleは内部でランダムに密輸品を選択する） ===
            case "smuggle":
                TrySmuggle("");
                TurnCount += 10;
                GameTime.AdvanceTurn(10);
                break;

            // === 投資 ===
            case "invest_shop":
                {
                    const int MinInvestment = 100;
                    const int MaxInvestment = 500;
                    int investAmount = Math.Min(MaxInvestment, Player.Gold);
                    if (investAmount < MinInvestment)
                    {
                        AddMessage($"投資には最低{MinInvestment}G必要だ");
                        break;
                    }
                    Player.SpendGold(investAmount);
                    _investmentSystem.Invest(InvestmentType.Shop, "一般商店", investAmount, TurnCount);
                    float expectedReturn = InvestmentSystem.GetExpectedReturn(InvestmentType.Shop, investAmount);
                    float successRate = InvestmentSystem.GetSuccessRate(InvestmentType.Shop);
                    AddMessage($"💰 店舗に{investAmount}G投資した。期待リターン: {expectedReturn:F0}G (成功率{successRate:P0})");
                    TurnCount += 5;
                    GameTime.AdvanceTurn(5);
                    OnStateChanged?.Invoke();
                }
                break;

            case "close":
                // 何もせず会話を終了
                break;
            default:
                // スキル融合系: fuse_{skillA}_{skillB}
                if (action.StartsWith("fuse_"))
                {
                    var fusionParts = action["fuse_".Length..];
                    var underscoreIdx = fusionParts.IndexOf('_');
                    if (underscoreIdx > 0 && underscoreIdx < fusionParts.Length - 1)
                    {
                        var skillA = fusionParts[..underscoreIdx];
                        var skillB = fusionParts[(underscoreIdx + 1)..];
                        TryFuseSkills(skillA, skillB);
                    }
                }
                // 転職系: class_change_{CharacterClass}
                if (action.StartsWith("class_change_") && Enum.TryParse<Core.CharacterClass>(action["class_change_".Length..], out var targetClassValue))
                {
                    TryClassChange(targetClassValue);
                }
                // エンチャント系: enchant_{EnchantmentType}
                else if (action.StartsWith("enchant_") && Enum.TryParse<EnchantmentType>(action["enchant_".Length..], out var enchantType))
                {
                    var mainHand = Player.Equipment.MainHand;
                    if (mainHand != null)
                    {
                        TryEnchant(mainHand, enchantType, SoulGemQuality.Fragment);
                    }
                    else
                    {
                        AddMessage("エンチャント対象の武器を装備していない");
                    }
                }
                // 闇市場購入系: black_market_buy_{ItemName}
                else if (action.StartsWith("black_market_buy_"))
                {
                    var itemName = action["black_market_buy_".Length..];
                    var allItems = BlackMarketSystem.GetAvailableItems(_karmaSystem.KarmaValue);
                    var targetItem = allItems.FirstOrDefault(i => i.Name == itemName);
                    if (targetItem != null)
                    {
                        TryBlackMarketBuy(targetItem);
                    }
                    else
                    {
                        AddMessage("その商品は既に売り切れだ");
                    }
                }
                // ショップ系: open_shop_{FacilityType}
                else if (action.StartsWith("open_shop_") && Enum.TryParse<FacilityType>(action["open_shop_".Length..], out var shopType))
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
        // AH-1: NpcMemorySystemの印象値を好感度に反映
        int baseAffinity = _npcSystem.GetNpcState(npcId).Affinity;
        int memoryImpression = _npcMemorySystem.CalculateImpression(npcId);
        return baseAffinity + memoryImpression;
    }

    /// <summary>NPC好感度ランクを取得</summary>
    public string GetNpcAffinityRank(string npcId)
    {
        return NpcDefinition.GetAffinityRank(_npcSystem.GetNpcState(npcId).Affinity);
    }

    /// <summary>仲間募集処理</summary>
    private void HandleRecruitCompanion()
    {
        // CS-3: 孤独の誓約違反チェック
        CheckOathViolation("recruit_companion");

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

        Player.SpendGold(companion.HireCost);
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
        if (result.Success && result.Reward != null)
        {
            if (result.Reward.GuildPoints > 0)
                AddGuildPoints(result.Reward.GuildPoints);

            // AO-2: クエスト報酬アイテムをインベントリに追加
            if (result.Reward.ItemIds != null)
            {
                foreach (var itemId in result.Reward.ItemIds)
                {
                    var item = ItemDefinitions.Create(itemId);
                    if (item != null)
                    {
                        ((Inventory)Player.Inventory).Add(item);
                        AddMessage($"📦 報酬アイテム: {item.GetDisplayName()}を受け取った");
                    }
                }
            }

            // 信仰ポイント
            if (result.Reward.FaithPoints > 0)
            {
                Player.AddFaithPoints(result.Reward.FaithPoints);
            }

            // DB-4: クエスト完了時にカルマ上昇
            _karmaSystem.ModifyKarma(3, "クエスト完了");

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
        var quests = _questSystem.GetAvailableQuests(Player.Level, _guildSystem.CurrentRank);

        // AW-5: 評判によるクエスト解放率フィルタリング
        var territory = _worldMapSystem.CurrentTerritory;
        double availability = _reputationSystem.GetQuestAvailability(territory);
        if (availability < 1.0)
        {
            int maxQuests = Math.Max(1, (int)(quests.Count * availability));
            return quests.Take(maxQuests).ToList();
        }

        return quests;
    }

    /// <summary>完了済みクエスト数を取得</summary>
    public int CompletedQuestCount => _questSystem.CompletedQuestCount;

    #endregion

    /// <summary>
    /// 自動探索の1ステップ実行。未探索タイルへBFSで最短経路を求め1マス移動。
    /// </summary>
    private bool StepAutoExplore()
    {
        // 下り階段上でTab → 上り階段へ移動
        var currentTile = Map.GetTile(Player.Position);
        if (currentTile.Type == TileType.StairsDown && !_autoExploring)
        {
            var upStairsStep = FindPathToStairsUp();
            if (upStairsStep != null)
            {
                _autoExploring = true;
                _autoExploreTargetStairsUp = true;
                AddMessage("上り階段へ移動中…");
                return TryMove(upStairsStep.Value);
            }
        }

        // 上り階段への移動中
        if (_autoExploreTargetStairsUp && _autoExploring)
        {
            var tile = Map.GetTile(Player.Position);
            if (tile.Type == TileType.StairsUp)
            {
                _autoExploring = false;
                _autoExploreTargetStairsUp = false;
                AddMessage("上り階段に到着した");
                return false;
            }
            var upStep = FindPathToStairsUp();
            if (upStep != null)
            {
                return TryMove(upStep.Value);
            }
            _autoExploring = false;
            _autoExploreTargetStairsUp = false;
            AddMessage("上り階段への経路がない");
            return false;
        }

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
            // 未探索タイルがない場合、下り階段へ移動を試みる
            var stairsStep = FindPathToStairs();
            if (stairsStep != null)
            {
                AddMessage("探索完了 — 下り階段へ移動中…");
                return TryMove(stairsStep.Value);
            }

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

        // DD-2: 満腹度が低い（25%以下）
        if (Player.Hunger < GameConstants.MaxHunger / 4)
            return true;

        // DD-2: 正気度が低い（25%以下）
        if (Player.Sanity < GameConstants.MaxSanity / 4)
            return true;

        // CJ-3: 渇きが低い（25%以下）
        if (Player.Thirst < GameConstants.MaxThirst / 4)
            return true;

        // DD-5: ボスフロアでは自動探索を停止
        if (CurrentFloor % GameConstants.BossFloorInterval == 0)
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
    /// BFSで下り階段への最短経路の1歩目を求める
    /// </summary>
    private Position? FindPathToStairs()
    {
        // ロケーションマップや地上では階段移動しない
        if (_isInLocationMap || _worldMapSystem.IsOnSurface) return null;

        var visited = new HashSet<Position>();
        var queue = new Queue<(Position Pos, Position FirstStep)>();

        visited.Add(Player.Position);

        var dirs = new (int dx, int dy)[]
        {
            (0, -1), (0, 1), (-1, 0), (1, 0),
            (-1, -1), (1, -1), (-1, 1), (1, 1)
        };

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
            var tile = Map.GetTile(current);

            if (tile.Type == TileType.StairsDown)
                return firstStep;

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
    /// BFSで上り階段への最短経路の1歩目を求める
    /// </summary>
    private Position? FindPathToStairsUp()
    {
        if (_isInLocationMap || _worldMapSystem.IsOnSurface) return null;

        var visited = new HashSet<Position>();
        var queue = new Queue<(Position Pos, Position FirstStep)>();

        visited.Add(Player.Position);

        var dirs = new (int dx, int dy)[]
        {
            (0, -1), (0, 1), (-1, 0), (1, 0),
            (-1, -1), (1, -1), (-1, 1), (1, 1)
        };

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
            var tile = Map.GetTile(current);

            if (tile.Type == TileType.StairsUp)
                return firstStep;

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
        _messageHistory.Add(message);
        if (_messageHistory.Count > 1000)
        {
            _messageHistory.RemoveAt(0);
        }
        OnMessage?.Invoke(message);
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
                HasPrayedToday = Player.HasPrayedToday,  // AB-7/M-3
                FaithCap = Player.FaithCap,
                Thirst = Player.Thirst,
                Fatigue = Player.Fatigue,
                HasFatigueRestrictionRelief = Player.HasFatigueRestrictionRelief,
                FatigueRestrictionReliefRemainingTurns = Player.FatigueRestrictionReliefRemainingTurns,
                Hygiene = Player.Hygiene,
                Gold = Player.Gold,
                Race = Player.Race,
                CharacterClass = Player.CharacterClass,
                Background = Player.Background,
                TotalDeaths = TotalDeaths,
                BonusMaxHp = Player.BonusMaxHp,
                BonusMaxMp = Player.BonusMaxMp,
                BonusCriticalRate = Player.BonusCriticalRate,
                KnownRunes = Player.KnownRunes.ToList(),
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
                    Sanity = _transferData.Sanity,
                    Level = _transferData.Level,     // BW-5
                    Gold = _transferData.Gold         // BW-6
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

        // 状態異常保存
        foreach (var effect in Player.StatusEffects)
        {
            save.Player.StatusEffects.Add(new StatusEffectSaveData
            {
                Type = effect.Type.ToString(),
                RemainingTurns = effect.Duration,
                Potency = effect.DamagePerTick,
                Name = effect.Name,
                StackCount = effect.StackCount,
                DamageElement = effect.DamageElement.ToString(),
                MaxStack = effect.MaxStack
            });
        }

        // ペットデータ保存
        var pets = _petSystem.Pets;
        if (pets.Count > 0)
        {
            var firstPet = pets.Values.First();
            save.PetData = new PetSaveData
            {
                PetId = firstPet.PetId,
                Name = firstPet.Name,
                PetType = firstPet.Type.ToString(),
                Level = firstPet.Level,
                Experience = firstPet.Experience,
                Hunger = firstPet.Hunger,
                Loyalty = firstPet.Loyalty,
                CurrentHp = firstPet.CurrentHp,
                MaxHp = firstPet.MaxHp,
                IsRiding = firstPet.IsRiding
            };
        }

        // カルマ保存
        save.KarmaValue = _karmaSystem.KarmaValue;
        save.KarmaHistory = _karmaSystem.KarmaHistory
            .Select(e => $"{e.OldValue}->{e.NewValue}:{e.Reason}")
            .ToList();

        // 習熟度保存
        foreach (var (category, data) in _proficiencySystem.GetAllProficiencies())
        {
            save.ProficiencyLevels[category.ToString()] = data.Level;
            save.ProficiencyExp[category.ToString()] = data.CurrentExp;
        }

        // 病気保存
        if (_playerDisease.HasValue)
        {
            save.CurrentDisease = _playerDisease.Value.ToString();
            save.DiseaseRemainingTurns = _diseaseRemainingTurns;
        }

        // CM-1/CM-2/CM-3: NG+/クリア/無限ダンジョン状態の保存
        if (_ngPlusTier.HasValue)
            save.NgPlusTier = (int)_ngPlusTier.Value;
        save.HasCleared = _hasCleared;
        save.ClearRank = _clearRank;
        save.InfiniteDungeonMode = _infiniteDungeonMode;
        save.InfiniteDungeonKills = _infiniteDungeonKills;
        save.TotalDeaths = TotalDeaths;

        // AS-2: 地面アイテムの保存
        foreach (var (item, pos) in GroundItems)
        {
            save.GroundItems.Add(new GroundItemSaveData
            {
                Item = CreateItemSaveData(item),
                X = pos.X,
                Y = pos.Y
            });
        }

        // 戦闘スタンス保存
        save.CombatStance = _playerStance.ToString();

        // コンパニオンデータ保存
        foreach (var companion in _companionSystem.Party)
        {
            save.Companions.Add(new CompanionSaveData
            {
                Name = companion.Name,
                CompanionType = companion.Type.ToString(),
                Level = companion.Level,
                Hp = companion.Hp,
                MaxHp = companion.MaxHp,
                Attack = companion.Attack,
                Defense = companion.Defense,
                IsAlive = companion.IsAlive,
                Loyalty = companion.Loyalty,
                HireCost = companion.HireCost,
                AIMode = companion.AIMode.ToString()
            });
        }

        // BQ-7: スキルツリー状態の保存
        save.SkillTreeLearnedSkills = _skillTreeSystem.UnlockedNodes.ToList();

        // BQ-8: 建設済み施設の保存
        save.BuiltFacilities = _baseConstructionSystem.BuiltFacilities
            .Select(f => f.ToString()).ToList();

        // BQ-2: 領地別評判値の保存
        foreach (var (territory, value) in _reputationSystem.GetAllReputations())
        {
            save.ReputationValues[territory.ToString()] = value;
        }

        // BQ-24/BU-12: チュートリアル完了済みステップの保存
        save.CompletedTutorialSteps = _tutorialSystem.GetCompletedSteps().ToList();

        // BU-11: 解除済み実績の保存
        save.UnlockedAchievements = _achievementSystem.GetUnlockedIds();

        // BR-5: 現在のダンジョン特性の保存
        if (_currentDungeonFeature.HasValue)
            save.CurrentDungeonFeature = _currentDungeonFeature.Value.ToString();

        // BZ-5: 商人ギルド状態の保存
        if (_merchantGuildSystem.IsMember && _merchantGuildSystem.Membership != null)
        {
            var m = _merchantGuildSystem.Membership;
            save.MerchantGuild = new MerchantGuildSaveData
            {
                IsMember = true,
                Rank = m.Rank.ToString(),
                GuildPoints = m.GuildPoints,
                TradeCount = m.TradeCount,
                TotalProfit = m.TotalProfit,
                Routes = _merchantGuildSystem.Routes.Select(r => new TradeRouteSaveData
                {
                    RouteId = r.RouteId,
                    Origin = r.Origin.ToString(),
                    Destination = r.Destination.ToString(),
                    Status = r.Status.ToString(),
                    ProfitMultiplier = r.ProfitMultiplier,
                    EstablishmentCost = r.EstablishmentCost
                }).ToList()
            };
        }

        // BZ-6: 派閥戦争状態の保存
        if (_factionWarSystem.ActiveWars.Count > 0 || _factionWarSystem.WarHistory.Count > 0)
        {
            save.FactionWar = new FactionWarSaveData
            {
                ActiveWars = _factionWarSystem.ActiveWars.Select(w => new WarEventSaveData
                {
                    WarId = w.WarId,
                    Name = w.Name,
                    Attacker = w.Attacker.ToString(),
                    Defender = w.Defender.ToString(),
                    Phase = w.Phase.ToString(),
                    TurnStarted = w.TurnStarted,
                    Duration = w.Duration,
                    PlayerAlignment = w.PlayerAlignment.ToString()
                }).ToList(),
                WarHistory = _factionWarSystem.WarHistory.Select(o => new WarOutcomeSaveData
                {
                    WarId = o.WarId,
                    Winner = o.Winner.ToString(),
                    Loser = o.Loser.ToString(),
                    TerritoryInfluenceChange = o.TerritoryInfluenceChange,
                    Description = o.Description
                }).ToList()
            };
        }

        // ===== BQ系: サブシステム永続性 =====

        // BQ-4: 図鑑エントリの保存
        foreach (var entry in _encyclopediaSystem.GetAllEntries())
        {
            if (entry.DiscoveryLevel > 0 || entry.KillCount > 0)
            {
                save.EncyclopediaEntries[entry.Id] = new EncyclopediaSaveEntry
                {
                    DiscoveryLevel = entry.DiscoveryLevel,
                    KillCount = entry.KillCount
                };
            }
        }

        // BQ-6: 誓約の保存
        save.ActiveOaths = _oathSystem.ActiveOaths.Select(o => o.ToString()).ToList();

        // BQ-9: 投資記録の保存
        foreach (var inv in _investmentSystem.Investments)
        {
            save.Investments.Add(new InvestmentSaveData
            {
                Type = inv.Type.ToString(),
                TargetName = inv.TargetName,
                Amount = inv.Amount,
                ExpectedReturn = (int)inv.ExpectedReturn,
                InvestedTurn = inv.InvestedTurn,
                IsCompleted = inv.IsCompleted
            });
        }

        // BQ-10: グリッドインベントリの保存
        foreach (var item in _gridInventorySystem.Items)
        {
            save.GridItems.Add(new GridItemSaveData
            {
                ItemId = item.ItemId,
                Name = item.Name,
                Size = item.Size.ToString(),
                GridX = item.GridX,
                GridY = item.GridY,
                IsRotated = item.IsRotated
            });
        }

        // BQ-12: NPC関係値の保存
        foreach (var rel in _relationshipSystem.GetAllRelationEntries())
        {
            string key = $"{rel.Type}:{rel.EntityA}:{rel.EntityB}";
            save.NpcRelations[key] = rel.Value;
        }

        // BQ-13: 識別済みアイテムの保存
        save.IdentifiedItemIds = _itemIdentificationSystem.IdentifiedItems.Keys.ToList();

        // BQ-17: 解読済み碑文の保存
        save.DecodedInscriptionIds = _inscriptionSystem.Inscriptions.Values
            .Where(i => i.IsDecoded)
            .Select(i => i.InscriptionId)
            .ToList();

        // BQ-19: 領地勢力影響の保存
        foreach (TerritoryId tid in Enum.GetValues<TerritoryId>())
        {
            var factionMap = _territoryInfluenceSystem.GetInfluenceMap(tid);
            if (factionMap != null && factionMap.Count > 0)
            {
                save.TerritoryInfluences[tid.ToString()] = new Dictionary<string, float>(factionMap);
            }
        }

        // BQ-21: ダンジョンショートカットの保存
        save.VisitedDungeonFloors = _dungeonShortcutSystem.GetVisitedFloors()
            .Select(vf => $"{vf.DungeonId}:{vf.Floor}").ToList();
        save.UnlockedShortcuts = _dungeonShortcutSystem.GetUnlockedShortcuts()
            .Select(sc => $"{sc.DungeonId}:{sc.FromFloor}:{sc.ToFloor}").ToList();

        // CA-8: プレイヤー向きの保存
        save.PlayerFacingDirection = _playerFacing.ToString();

        // BQ-5: 死亡ログの保存
        foreach (var log in _deathLogSystem.AllLogs)
        {
            save.DeathLogs.Add(new DeathLogSaveData
            {
                RunNumber = log.RunNumber,
                CharacterName = log.CharacterName,
                Class = log.Class.ToString(),
                Race = log.Race.ToString(),
                Level = log.Level,
                Cause = log.Cause.ToString(),
                CauseDetail = log.CauseDetail,
                Location = log.Location,
                Floor = log.Floor,
                TotalTurns = log.TotalTurns,
                Timestamp = log.Timestamp.ToString("o")
            });
        }

        // BQ-11: NPC記憶の保存
        foreach (var mem in _npcMemorySystem.Memories)
        {
            save.NpcMemories.Add(new NpcMemorySaveData
            {
                NpcId = mem.NpcId,
                Action = mem.Action,
                Impact = mem.Impact,
                TurnRecorded = mem.TurnRecorded
            });
        }

        // BQ-14: ダンジョン生態系イベントの保存
        foreach (var ev in _dungeonEcosystemSystem.Events)
        {
            save.EcosystemEvents.Add(new EcosystemEventSaveData
            {
                Type = ev.Type.ToString(),
                PredatorId = ev.PredatorId,
                PreyId = ev.PreyId,
                Floor = ev.Floor,
                Turn = ev.Turn,
                Description = ev.Description
            });
        }

        // 天候状態の保存
        save.WeatherState = CurrentWeather.ToString();

        // 季節状態の保存（GameTime月から派生するが、デバッグ・検証用途で明示保存）
        save.SeasonState = CurrentSeason.ToString();

        // BU-4: ゲーム時間開始値の保存
        save.GameTimeStartYear = GameTime.StartYear;
        save.GameTimeStartMonth = GameTime.StartMonth;
        save.GameTimeStartDay = GameTime.StartDay;
        save.GameTimeStartHour = GameTime.StartHour;
        save.GameTimeStartMinute = GameTime.StartMinute;

        // AS-3/CE-12: マップ探索状態の保存
        for (int y = 0; y < Map.Height; y++)
        {
            for (int x = 0; x < Map.Width; x++)
            {
                if (Map.GetTile(new Position(x, y)).IsExplored)
                {
                    save.ExploredTiles.Add($"{x},{y}");
                }
            }
        }

        // マップタイルデータの保存（セーブ/ロードでマップ構造を保持）
        if (Map is DungeonMap saveMap)
        {
            var mapSave = new MapSaveData
            {
                Width = saveMap.Width,
                Height = saveMap.Height,
            };

            // フロアキャッシュからCreatedAtTurnを取得
            // フォールバック: 新規ゲーム開始直後やキャッシュクリア後など、
            // フロアキャッシュが存在しない場合は現在のターン数を使用する
            var saveFloorKey = (_currentMapName, CurrentFloor);
            if (_floorCache.TryGetValue(saveFloorKey, out var saveFloorCache))
            {
                mapSave.CreatedAtTurn = saveFloorCache.CreatedAtTurn;
            }
            else
            {
                mapSave.CreatedAtTurn = GameTime.TotalTurns;
            }

            // タイルタイプを一次元配列として保存
            for (int y = 0; y < saveMap.Height; y++)
            {
                for (int x = 0; x < saveMap.Width; x++)
                {
                    var tile = saveMap.GetTile(new Position(x, y));
                    mapSave.TileTypes.Add((int)tile.Type);

                    // デフォルトと異なる追加状態を持つタイルのみ保存
                    if (HasNonDefaultTileState(tile))
                    {
                        mapSave.TileStates.Add(new TileStateSaveData
                        {
                            X = x,
                            Y = y,
                            RoomId = tile.RoomId,
                            IsLocked = tile.IsLocked,
                            LockDifficulty = tile.LockDifficulty,
                            TrapId = tile.TrapId,
                            ItemId = tile.ItemId,
                            BuildingId = tile.BuildingId,
                            ChestOpened = tile.ChestOpened,
                            ChestLockDifficulty = tile.ChestLockDifficulty,
                            ChestItems = tile.ChestItems?.ToList(),
                            InscriptionWordId = tile.InscriptionWordId,
                            InscriptionRead = tile.InscriptionRead,
                            GatheringNodeType = tile.GatheringNodeType.HasValue ? (int)tile.GatheringNodeType.Value : null,
                        });
                    }
                }
            }

            // 部屋情報の保存
            foreach (var room in saveMap.Rooms)
            {
                mapSave.Rooms.Add(new RoomSaveData
                {
                    Id = room.Id,
                    X = room.X,
                    Y = room.Y,
                    Width = room.Width,
                    Height = room.Height,
                    Type = room.Type.ToString(),
                    ConnectedRooms = room.ConnectedRooms.ToList(),
                });
            }

            // 地面アイテムの保存
            foreach (var (item, pos) in GroundItems)
            {
                mapSave.GroundItems.Add(new GroundItemSaveData
                {
                    Item = CreateItemSaveData(item),
                    X = pos.X,
                    Y = pos.Y,
                });
            }

            save.MapData = mapSave;
        }

        return save;
    }

    /// <summary>
    /// セーブデータからゲーム状態を復元
    /// </summary>
    public void LoadSaveData(SaveData save)
    {
        // AS-4: セーブデータバージョンチェック
        if (save.Version < 1)
        {
            AddMessage("⚠ 古いバージョンのセーブデータです。一部データが正しく読み込めない可能性があります。");
        }

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
            save.Player.Background,
            save.Player.Thirst,
            save.Player.Fatigue,
            save.Player.Hygiene
        );

        // 気付け薬フラグ復元
        Player.HasFatigueRestrictionRelief = save.Player.HasFatigueRestrictionRelief;
        Player.FatigueRestrictionReliefRemainingTurns = save.Player.FatigueRestrictionReliefRemainingTurns;

        // ボーナスステータス復元（種族・職業ボーナス等）
        Player.BonusMaxHp = save.Player.BonusMaxHp;
        Player.BonusMaxMp = save.Player.BonusMaxMp;
        Player.BonusCriticalRate = save.Player.BonusCriticalRate;

        // 習得済みルーン復元
        foreach (var runeId in save.Player.KnownRunes)
        {
            Player.LearnRune(runeId);
        }

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
        Player.HasPrayedToday = save.Player.HasPrayedToday;  // AB-7/M-3
        Player.FaithCap = save.Player.FaithCap;
        Player.RestorePreviousReligion(save.Player.PreviousReligion);
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

        // マップ復元: セーブデータにマップがあれば復元、なければ従来の再生成
        if (save.MapData != null && save.MapData.TileTypes.Count == save.MapData.Width * save.MapData.Height)
        {
            RestoreMapFromSaveData(save.MapData);
        }
        else
        {
            // 旧セーブデータ互換: マップデータがない場合は従来通り再生成
            GenerateFloor();
        }
        Player.Position = save.Player.Position.ToPosition();
        Map.ComputeFov(Player.Position, GetEffectiveViewRadius());

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
                Sanity = save.Player.TransferData.Sanity,
                Level = save.Player.TransferData.Level,   // BW-5
                Gold = save.Player.TransferData.Gold       // BW-6
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

        // 状態異常復元（Name/DamagePerTick/StackCount/DamageElement/MaxStack含む。旧形式セーブデータでは新フィールドがデフォルト値にフォールバック）
        foreach (var effectData in save.Player.StatusEffects)
        {
            if (Enum.TryParse<StatusEffectType>(effectData.Type, out var effectType))
            {
                var restoredName = !string.IsNullOrEmpty(effectData.Name) ? effectData.Name : effectType.ToString();
                var restoredElement = Enum.TryParse<Element>(effectData.DamageElement, out var elem) ? elem : Element.None;
                var restoredMaxStack = effectData.MaxStack > 0 ? effectData.MaxStack : 1;
                var effect = new StatusEffect(effectType, effectData.RemainingTurns)
                {
                    Name = restoredName,
                    DamagePerTick = effectData.Potency,
                    DamageElement = restoredElement,
                    MaxStack = restoredMaxStack
                };
                // StackCountは復元メソッド経由で設定（private set）
                if (effectData.StackCount > 1)
                {
                    effect.RestoreStackCount(effectData.StackCount);
                }
                Player.ApplyStatusEffect(effect);
            }
        }

        // ペットデータ復元
        if (save.PetData != null && Enum.TryParse<PetType>(save.PetData.PetType, out var petType))
        {
            _petSystem.Reset();
            _petSystem.AddPet(save.PetData.PetId, save.PetData.Name, petType);
            // AB-1: ペット属性値を復元
            _petSystem.RestorePetState(save.PetData.PetId,
                save.PetData.Level, save.PetData.Experience,
                save.PetData.Hunger, save.PetData.Loyalty, save.PetData.CurrentHp,
                save.PetData.MaxHp, save.PetData.IsRiding);
        }

        // カルマ復元
        if (save.KarmaValue != 0)
        {
            _karmaSystem.Reset();
            _karmaSystem.ModifyKarma(save.KarmaValue, "セーブデータ復元");
        }

        // カルマ履歴復元
        if (save.KarmaHistory.Count > 0)
        {
            _karmaSystem.KarmaHistory.Clear();
            _karmaSystem.RestoreHistory(save.KarmaHistory);
        }

        // 習熟度復元
        if (save.ProficiencyLevels.Count > 0)
        {
            foreach (var (categoryStr, level) in save.ProficiencyLevels)
            {
                if (Enum.TryParse<ProficiencyCategory>(categoryStr, out var category))
                {
                    var allProf = _proficiencySystem.GetAllProficiencies();
                    if (allProf.TryGetValue(category, out var data))
                    {
                        data.Level = level;
                        if (save.ProficiencyExp.TryGetValue(categoryStr, out var exp))
                        {
                            data.CurrentExp = exp;
                        }
                    }
                }
            }
        }

        // 病気復元
        if (!string.IsNullOrEmpty(save.CurrentDisease)
            && Enum.TryParse<DiseaseType>(save.CurrentDisease, out var diseaseType))
        {
            _playerDisease = diseaseType;
            _diseaseRemainingTurns = save.DiseaseRemainingTurns;
        }

        // CM-1/CM-2/CM-3: NG+/クリア/無限ダンジョン状態の復元
        if (save.NgPlusTier.HasValue && Enum.IsDefined(typeof(NewGamePlusTier), save.NgPlusTier.Value))
            _ngPlusTier = (NewGamePlusTier)save.NgPlusTier.Value;
        _hasCleared = save.HasCleared;
        _clearRank = save.ClearRank ?? "";
        _infiniteDungeonMode = save.InfiniteDungeonMode;
        _infiniteDungeonKills = save.InfiniteDungeonKills;
        TotalDeaths = save.TotalDeaths;

        // AS-2: 地面アイテムの復元
        GroundItems.Clear();
        foreach (var groundData in save.GroundItems)
        {
            var item = RestoreItem(groundData.Item);
            if (item != null)
            {
                GroundItems.Add((item, new Position(groundData.X, groundData.Y)));
            }
        }

        // 戦闘スタンスの復元
        if (!string.IsNullOrEmpty(save.CombatStance) && Enum.TryParse<CombatStance>(save.CombatStance, out var stance))
        {
            _playerStance = stance;
        }

        // BQ-7: スキルツリー状態の復元
        if (save.SkillTreeLearnedSkills.Count > 0)
        {
            _skillTreeSystem.RestoreFromSave(save.SkillTreeLearnedSkills, _skillTreeSystem.AvailablePoints);
        }

        // BQ-8: 建設済み施設の復元
        if (save.BuiltFacilities.Count > 0)
        {
            var facilities = save.BuiltFacilities
                .Select(f => Enum.TryParse<FacilityCategory>(f, out var cat) ? (FacilityCategory?)cat : null)
                .Where(f => f.HasValue)
                .Select(f => f!.Value)
                .ToList();
            _baseConstructionSystem.RestoreFromSave(facilities);
        }

        // M-2: コンパニオンデータの復元
        if (save.Companions.Count > 0)
        {
            _companionSystem.Reset();
            foreach (var cd in save.Companions)
            {
                if (Enum.TryParse<CompanionType>(cd.CompanionType ?? "Mercenary", out var compType))
                {
                    var aiMode = Enum.TryParse<CompanionAIMode>(cd.AIMode ?? "Defensive", out var parsedMode)
                        ? parsedMode : CompanionAIMode.Defensive;
                    var companion = new CompanionSystem.CompanionData(
                        cd.Name, compType, aiMode,
                        cd.Level, cd.Loyalty, cd.HireCost, cd.Hp, cd.MaxHp, cd.Attack, cd.Defense, cd.IsAlive);
                    _companionSystem.AddCompanion(companion);
                }
            }
        }

        // BQ-2: 領地別評判値の復元
        if (save.ReputationValues.Count > 0)
        {
            _reputationSystem.RestoreReputations(save.ReputationValues);
        }

        // BQ-24/BU-12: チュートリアル完了済みステップの復元
        if (save.CompletedTutorialSteps.Count > 0)
        {
            _tutorialSystem.RestoreCompletedSteps(save.CompletedTutorialSteps);
        }

        // BU-11: 解除済み実績の復元
        if (save.UnlockedAchievements.Count > 0)
        {
            _achievementSystem.RestoreUnlocked(save.UnlockedAchievements);
        }

        // BR-5: 現在のダンジョン特性の復元
        if (save.CurrentDungeonFeature != null && Enum.TryParse<DungeonFeatureType>(save.CurrentDungeonFeature, out var feature))
        {
            _currentDungeonFeature = feature;
        }

        // BZ-5: 商人ギルド状態の復元
        if (save.MerchantGuild is { IsMember: true })
        {
            var mg = save.MerchantGuild;
            var rank = Enum.TryParse<GuildRank>(mg.Rank, out var parsedRank) ? parsedRank : GuildRank.None;
            var routes = mg.Routes.Select(r =>
            {
                var origin = Enum.TryParse<TerritoryId>(r.Origin, out var o) ? o : TerritoryId.Capital;
                var dest = Enum.TryParse<TerritoryId>(r.Destination, out var d) ? d : TerritoryId.Capital;
                var status = Enum.TryParse<TradeRouteStatus>(r.Status, out var s) ? s : TradeRouteStatus.Open;
                return new MerchantGuildSystem.TradeRoute(r.RouteId, origin, dest, status, r.ProfitMultiplier, r.EstablishmentCost);
            }).ToList();
            _merchantGuildSystem.RestoreFromSave(Player.Name, rank, mg.GuildPoints, mg.TradeCount, mg.TotalProfit, routes);
        }

        // BZ-6: 派閥戦争状態の復元
        if (save.FactionWar != null)
        {
            _factionWarSystem.Reset();
            foreach (var war in save.FactionWar.ActiveWars)
            {
                if (Enum.TryParse<TerritoryId>(war.Attacker, out var attacker) &&
                    Enum.TryParse<TerritoryId>(war.Defender, out var defender))
                {
                    _factionWarSystem.StartWar(war.WarId, war.Name, attacker, defender, war.TurnStarted);
                    // フェーズ復元
                    if (Enum.TryParse<WarPhase>(war.Phase, out var phase))
                    {
                        while (_factionWarSystem.ActiveWars.FirstOrDefault(w => w.WarId == war.WarId)?.Phase != phase)
                        {
                            var advanced = _factionWarSystem.AdvancePhase(war.WarId, TurnCount);
                            if (advanced == null || advanced.Phase == WarPhase.Peace) break;
                        }
                    }
                    // 陣営復元
                    if (Enum.TryParse<FactionAlignment>(war.PlayerAlignment, out var alignment) && alignment != FactionAlignment.Neutral)
                    {
                        _factionWarSystem.ChooseAlignment(war.WarId, alignment);
                    }
                }
            }
        }

        AddMessage("セーブデータをロードした");

        // ===== BQ系: サブシステム永続性の復元 =====

        // BQ-4: 図鑑エントリの復元
        if (save.EncyclopediaEntries.Count > 0)
        {
            var entries = save.EncyclopediaEntries.ToDictionary(
                kvp => kvp.Key,
                kvp => (kvp.Value.DiscoveryLevel, kvp.Value.KillCount));
            _encyclopediaSystem.RestoreDiscoveryState(entries);
        }

        // BQ-6: 誓約の復元
        if (save.ActiveOaths.Count > 0)
        {
            var oaths = save.ActiveOaths
                .Select(s => Enum.TryParse<OathType>(s, out var oath) ? (OathType?)oath : null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value);
            _oathSystem.RestoreOaths(oaths);
        }

        // BQ-9: 投資記録の復元
        if (save.Investments.Count > 0)
        {
            var records = save.Investments.Select(inv =>
            {
                Enum.TryParse<InvestmentType>(inv.Type, out var invType);
                return new InvestmentSystem.InvestmentRecord(invType, inv.TargetName, inv.Amount, inv.ExpectedReturn, inv.InvestedTurn, inv.IsCompleted);
            });
            _investmentSystem.RestoreInvestments(records);
        }

        // BQ-10: グリッドインベントリの復元
        if (save.GridItems.Count > 0)
        {
            var gridItems = save.GridItems.Select(gi =>
            {
                Enum.TryParse<GridItemSize>(gi.Size, out var size);
                return new GridInventorySystem.GridItem(gi.ItemId, gi.Name, size, gi.GridX, gi.GridY, gi.IsRotated);
            });
            _gridInventorySystem.RestoreGrid(gridItems);
        }

        // BQ-12: NPC関係値の復元
        if (save.NpcRelations.Count > 0)
        {
            _relationshipSystem.RestoreRelations(save.NpcRelations);
        }

        // BQ-13: 識別済みアイテムの復元
        if (save.IdentifiedItemIds.Count > 0)
        {
            _itemIdentificationSystem.RestoreIdentified(save.IdentifiedItemIds);
        }

        // BQ-17: 解読済み碑文の復元
        if (save.DecodedInscriptionIds.Count > 0)
        {
            _inscriptionSystem.RestoreDecoded(save.DecodedInscriptionIds);
        }

        // BQ-19: 領地勢力影響の復元
        if (save.TerritoryInfluences.Count > 0)
        {
            var influences = new Dictionary<TerritoryId, Dictionary<string, float>>();
            foreach (var (key, factions) in save.TerritoryInfluences)
            {
                if (Enum.TryParse<TerritoryId>(key, out var tid))
                {
                    influences[tid] = new Dictionary<string, float>(factions);
                }
            }
            _territoryInfluenceSystem.RestoreInfluences(influences);
        }

        // BQ-21: ダンジョンショートカットの復元
        if (save.VisitedDungeonFloors.Count > 0 || save.UnlockedShortcuts.Count > 0)
        {
            var visitedFloors = save.VisitedDungeonFloors
                .Select(s => s.Split(':'))
                .Where(p => p.Length == 2 && int.TryParse(p[1], out _))
                .Select(p => (DungeonId: p[0], Floor: int.Parse(p[1])));
            var shortcuts = save.UnlockedShortcuts
                .Select(s => s.Split(':'))
                .Where(p => p.Length == 3 && int.TryParse(p[1], out _) && int.TryParse(p[2], out _))
                .Select(p => (DungeonId: p[0], FromFloor: int.Parse(p[1]), ToFloor: int.Parse(p[2])));
            _dungeonShortcutSystem.RestoreState(visitedFloors, shortcuts);
        }

        // CA-8: プレイヤー向きの復元
        if (save.PlayerFacingDirection != null && Enum.TryParse<Direction>(save.PlayerFacingDirection, out var facing))
        {
            _playerFacing = facing;
        }

        // BQ-5: 死亡ログの復元
        if (save.DeathLogs.Count > 0)
        {
            var logs = save.DeathLogs.Select(dl =>
            {
                Enum.TryParse<Core.CharacterClass>(dl.Class, out var cls);
                Enum.TryParse<Race>(dl.Race, out var race);
                Enum.TryParse<DeathCause>(dl.Cause, out var cause);
                DateTime.TryParse(dl.Timestamp, out var ts);
                return new DeathLogSystem.DeathLogEntry(dl.RunNumber, dl.CharacterName, cls, race, dl.Level, cause, dl.CauseDetail, dl.Location, dl.Floor, dl.TotalTurns, ts);
            });
            _deathLogSystem.RestoreLogs(logs);
        }

        // BQ-11: NPC記憶の復元
        if (save.NpcMemories.Count > 0)
        {
            var memories = save.NpcMemories.Select(m =>
                new NpcMemorySystem.MemoryEntry(m.NpcId, m.Action, m.Impact, m.TurnRecorded));
            _npcMemorySystem.RestoreMemories(memories);
        }

        // BQ-14: ダンジョン生態系イベントの復元
        if (save.EcosystemEvents.Count > 0)
        {
            var events = save.EcosystemEvents.Select(ev =>
            {
                Enum.TryParse<EcosystemEventType>(ev.Type, out var evType);
                return new DungeonEcosystemSystem.EcosystemEvent(evType, ev.PredatorId, ev.PreyId, ev.Floor, ev.Turn, ev.Description);
            });
            _dungeonEcosystemSystem.RestoreEvents(events);
        }

        // BU-4: ゲーム時間開始値の復元
        if (save.GameTimeStartYear > 0)
        {
            GameTime.StartYear = save.GameTimeStartYear;
            GameTime.StartMonth = save.GameTimeStartMonth;
            GameTime.StartDay = save.GameTimeStartDay;
            GameTime.StartHour = save.GameTimeStartHour;
            GameTime.StartMinute = save.GameTimeStartMinute;
        }

        // AS-3/CE-12: マップ探索状態の復元
        if (save.ExploredTiles.Count > 0)
        {
            foreach (var coord in save.ExploredTiles)
            {
                var parts = coord.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out var ex) && int.TryParse(parts[1], out var ey))
                {
                    var pos = new Position(ex, ey);
                    if (Map.IsInBounds(pos))
                    {
                        Map.GetTile(pos).IsExplored = true;
                    }
                }
            }
        }

        // 天候状態の復元
        if (!string.IsNullOrEmpty(save.WeatherState) && Enum.TryParse<Weather>(save.WeatherState, out var weather))
        {
            CurrentWeather = weather;
        }

        // AS-5: スキルツリーボーナスプロバイダーを再設定（ロード後のステータス計算に必要）
        Player.SkillTreeBonusProvider = () => _skillTreeSystem.GetTotalStatBonuses();
        Player.UpdateMaxWeight();

        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// セーブデータからマップを復元し、フロアキャッシュに登録する
    /// </summary>
    private static bool HasNonDefaultTileState(Tile tile)
    {
        return tile.RoomId != -1 || tile.IsLocked || tile.LockDifficulty > 0
            || tile.TrapId != null || tile.ItemId != null || tile.BuildingId != null
            || tile.ChestOpened || tile.ChestLockDifficulty > 0 || tile.ChestItems != null
            || tile.InscriptionWordId != null || tile.InscriptionRead
            || tile.GatheringNodeType != null;
    }

    private void RestoreMapFromSaveData(MapSaveData mapData)
    {
        // ダンジョン特徴を決定（SpawnEnemies等で使用）
        var territory = _worldMapSystem.GetCurrentTerritoryInfo().Id;
        _currentDungeonFeature = DungeonFeatureGenerator.SelectFeatureForTerritory(territory, CurrentFloor, _random);

        // 環境音を更新
        bool isBossFloor = CurrentFloor % GameConstants.BossFloorInterval == 0;
        _currentAmbientSound = AmbientSoundSystem.GetAmbientForDungeon(CurrentFloor, isBossFloor);

        // DungeonMapを復元
        var restoredMap = new DungeonMap(mapData.Width, mapData.Height)
        {
            Depth = CurrentFloor,
            Name = _currentMapName
        };

        // タイルタイプを復元
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                int index = y * mapData.Width + x;
                if (index < mapData.TileTypes.Count)
                {
                    var rawTileType = mapData.TileTypes[index];
                    var tileType = Enum.IsDefined(typeof(TileType), rawTileType)
                        ? (TileType)rawTileType
                        : TileType.Floor;
                    restoredMap.SetTile(new Position(x, y), Tile.FromType(tileType));

                    // 階段位置を設定
                    if (tileType == TileType.StairsUp)
                        restoredMap.SetStairsUp(new Position(x, y));
                    else if (tileType == TileType.StairsDown)
                        restoredMap.SetStairsDown(new Position(x, y));
                }
            }
        }

        // タイルの詳細状態を復元
        foreach (var state in mapData.TileStates)
        {
            var pos = new Position(state.X, state.Y);
            if (!restoredMap.IsInBounds(pos)) continue;

            var tile = restoredMap.GetTile(pos);
            tile.RoomId = state.RoomId;
            tile.IsLocked = state.IsLocked;
            tile.LockDifficulty = state.LockDifficulty;
            tile.TrapId = state.TrapId;
            tile.ItemId = state.ItemId;
            tile.BuildingId = state.BuildingId;
            tile.ChestOpened = state.ChestOpened;
            tile.ChestLockDifficulty = state.ChestLockDifficulty;
            tile.ChestItems = state.ChestItems?.ToList();
            tile.InscriptionWordId = state.InscriptionWordId;
            tile.InscriptionRead = state.InscriptionRead;
            tile.GatheringNodeType = state.GatheringNodeType.HasValue
                ? (GatheringType)state.GatheringNodeType.Value
                : null;
        }

        // 部屋情報を復元
        foreach (var roomData in mapData.Rooms)
        {
            var room = new Room
            {
                Id = roomData.Id,
                X = roomData.X,
                Y = roomData.Y,
                Width = roomData.Width,
                Height = roomData.Height,
                Type = Enum.TryParse<RoomType>(roomData.Type, out var rt) ? rt : RoomType.Normal,
            };
            foreach (var connId in roomData.ConnectedRooms)
            {
                room.ConnectedRooms.Add(connId);
            }
            restoredMap.AddRoom(room);
        }

        // 入口位置を設定（Entranceタイプの部屋がある場合）
        var entranceRoom = restoredMap.GetEntranceRoom();
        if (entranceRoom != null)
        {
            restoredMap.SetEntrance(entranceRoom.Center);
        }

        Map = restoredMap;

        // 敵を再生成（動的要素：敵はフロア進入時に毎回再配置される仕様。
        // マップ構造・地面アイテムはセーブデータから復元するが、
        // 敵の位置・状態は保存せず、フロア進入時と同じロジックで再生成する）
        Enemies.Clear();
        SpawnEnemies();

        // 地面アイテムを復元
        GroundItems.Clear();
        foreach (var groundItemData in mapData.GroundItems)
        {
            var item = RestoreItem(groundItemData.Item);
            if (item != null)
            {
                GroundItems.Add((item, new Position(groundItemData.X, groundItemData.Y)));
            }
        }

        // フロアキャッシュに登録（24時間再生成判定を維持）
        var floorKey = (_currentMapName, CurrentFloor);
        _floorCache[floorKey] = new FloorCache(restoredMap, mapData.CreatedAtTurn, new List<(Item, Position)>(GroundItems));

        // ダンジョンショートカット用: 訪問済み階を記録
        if (!string.IsNullOrEmpty(_currentMapName))
        {
            _dungeonShortcutSystem.MarkFloorVisited(_currentMapName, CurrentFloor);
        }
    }

    private static ItemSaveData CreateItemSaveData(Item item) => new()
    {
        ItemId = item.ItemId,
        EnhancementLevel = item.EnhancementLevel,
        IsIdentified = item.IsIdentified,
        IsCursed = item.IsCursed,
        IsBlessed = item.IsBlessed,
        Durability = item.Durability,
        StackCount = item is IStackable stackable ? stackable.StackCount : 1,
        Grade = item.Grade.ToString(),  // AS-1: アイテム品質を保存
        AppliedEnchantments = item.AppliedEnchantments.ToList()  // AN-3: エンチャント保存
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

        // AS-1: アイテム品質を復元
        if (!string.IsNullOrEmpty(data.Grade) && Enum.TryParse<ItemGrade>(data.Grade, out var grade))
            item.Grade = grade;

        // AN-3: エンチャントを復元
        if (data.AppliedEnchantments.Count > 0)
            item.AppliedEnchantments = data.AppliedEnchantments.ToList();

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

    /// <summary>プレイヤーの渇き値</summary>
    public int PlayerThirst => Player.Thirst;

    /// <summary>プレイヤーの渇き段階</summary>
    public ThirstStage PlayerThirstStage => Player.ThirstStage;

    /// <summary>渇きレベル名</summary>
    public string PlayerThirstName => ThirstSystem.GetThirstName(Player.ThirstStage);

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

    /// <summary>図鑑エントリを自動登録し発見レベルを上昇させる（モンスター以外用）</summary>
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

    /// <summary>モンスター撃破時の図鑑登録・更新</summary>
    private void RegisterAndDiscoverMonster(Enemy enemy)
    {
        var id = enemy.EnemyTypeId;
        var name = enemy.Name;

        if (_encyclopediaSystem.GetEntry(id) == null)
        {
            var monsterData = new MonsterEncyclopediaData(
                RaceName: GetMonsterRaceName(enemy.Race),
                MaxHp: enemy.MaxHp,
                MaxMp: enemy.MaxMp,
                MaxSp: enemy.MaxSp,
                BaseStats: enemy.BaseStats,
                DropTableId: enemy.DropTableId,
                Description: enemy.Description ?? $"{name}に関する情報。"
            );
            _encyclopediaSystem.RegisterMonsterEntry(id, name, monsterData);
            AddMessage($"📖 図鑑に{name}が記録された！");
        }

        if (_encyclopediaSystem.IncrementMonsterKill(id))
        {
            var entry = _encyclopediaSystem.GetEntry(id);
            if (entry != null)
            {
                AddMessage($"📖 {name}の情報が更新された！（開示Lv.{entry.DiscoveryLevel}）");
            }
        }
    }

    /// <summary>MonsterRaceの日本語名を取得</summary>
    private static string GetMonsterRaceName(MonsterRace race) => race switch
    {
        MonsterRace.Beast => "獣",
        MonsterRace.Humanoid => "人型",
        MonsterRace.Amorphous => "不定形",
        MonsterRace.Undead => "不死",
        MonsterRace.Demon => "悪魔",
        MonsterRace.Dragon => "竜",
        MonsterRace.Plant => "植物",
        MonsterRace.Insect => "昆虫",
        MonsterRace.Spirit => "精霊",
        MonsterRace.Construct => "構造体",
        _ => race.ToString()
    };

    /// <summary>死亡ログシステム</summary>
    public DeathLogSystem GetDeathLogSystem() => _deathLogSystem;

    /// <summary>スキルツリーシステム</summary>
    public SkillTreeSystem GetSkillTreeSystem() => _skillTreeSystem;

    /// <summary>仲間システム</summary>
    public CompanionSystem GetCompanionSystem() => _companionSystem;

    /// <summary>BK-1/BK-2: 仲間のAIモードを変更</summary>
    public bool SetCompanionAIMode(string companionName, CompanionAIMode mode)
    {
        if (_companionSystem.SetAIMode(companionName, mode))
        {
            string modeName = mode switch
            {
                CompanionAIMode.Aggressive => "攻撃",
                CompanionAIMode.Defensive => "防御",
                CompanionAIMode.Support => "支援",
                CompanionAIMode.Wait => "待機",
                _ => mode.ToString()
            };
            AddMessage($"🤝 {companionName}の行動方針を「{modeName}」に変更した");
            OnStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>拠点システム</summary>
    public BaseConstructionSystem GetBaseConstructionSystem() => _baseConstructionSystem;

    /// <summary>カルマシステム</summary>
    public KarmaSystem GetKarmaSystem() => _karmaSystem;

    /// <summary>評判システム</summary>
    public ReputationSystem GetReputationSystem() => _reputationSystem;

    /// <summary>誓約システム</summary>
    public OathSystem GetOathSystem() => _oathSystem;

    /// <summary>BH-2: パッシブスキル効果を適用</summary>
    private void ApplyPassiveSkillEffect(string skillId)
    {
        switch (skillId)
        {
            case "hp_boost":
                // 最大HP+10%
                Player.BonusMaxHp += (int)(Player.EffectiveStats.MaxHp * 0.10);
                Player.Heal(0); // MaxHp更新を反映
                AddMessage("💪 体力強化！最大HPが上昇した");
                break;
            case "mp_boost":
                // 最大MP+10%
                Player.BonusMaxMp += (int)(Player.EffectiveStats.MaxMp * 0.10);
                AddMessage("✨ 魔力強化！最大MPが上昇した");
                break;
            case "poison_resist":
                // BH-2: 毒耐性実効果 — SkillSystemに登録済みなのでダメージ計算時にGetPassiveBonus("poison_resist")で参照
                AddMessage("🛡 毒耐性を獲得！毒ダメージが半減する");
                break;
            case "critical_eye":
                // BH-2: クリティカル率+5%の実効果
                Player.BonusCriticalRate += 0.05;
                AddMessage("👁 鋭い眼を獲得！クリティカル率+5%");
                break;
            case "treasure_sense":
                // BH-2: 宝探しの勘の実効果（ドロップ率+15%はGetPassiveBonus参照）
                AddMessage("💎 宝探しの勘を獲得！アイテムドロップ率が上昇した");
                break;
        }
    }

    /// <summary>AF-1/AF-2/AF-3: ランダムイベントの解決処理</summary>
    private void ResolveRandomEvent(RandomEventSystem.RandomEvent evt)
    {
        switch (evt.Type)
        {
            case RandomEventType.NpcEncounter:
                // NPC遭遇: 情報提供
                AddMessage("旅人から有益な情報を得た。周囲のマップが明らかになった。");
                break;

            case RandomEventType.MerchantEncounter:
                // AJ-1: 商人ギルドメンバーは交易利益を得る
                if (_merchantGuildSystem.IsMember && _merchantGuildSystem.Routes.Count > 0)
                {
                    var route = _merchantGuildSystem.Routes[_random.Next(_merchantGuildSystem.Routes.Count)];
                    var tradeResult = _merchantGuildSystem.ExecuteTrade(route.RouteId, 50 + CurrentFloor * 10);
                    if (tradeResult != null)
                    {
                        Player.AddGold(tradeResult.ActualProfit);
                        AddMessage($"🏪 {tradeResult.Description}");
                        break;
                    }
                }
                // 非ギルドメンバーまたは交易失敗時: 通常の商人遭遇
                int goldReward = 10 + _random.Next(30);
                Player.AddGold(goldReward);
                AddMessage($"商人が特別価格で商品を売ってくれた。{goldReward}Gの価値がある品を得た！");
                break;

            case RandomEventType.AmbushEvent:
                // 待ち伏せ: ダメージを受ける
                int ambushDmg = Math.Max(1, Player.MaxHp / 10);
                Player.TakeDamage(Damage.Physical(ambushDmg));
                _lastDamageCause = DeathCause.Combat;
                AddMessage($"待ち伏せに遭った！ {ambushDmg}ダメージを受けた！");
                break;

            case RandomEventType.TreasureChest:
                int treasureGold = 20 + _random.Next(50);
                Player.AddGold(treasureGold);
                AddMessage($"宝箱から{treasureGold}Gを見つけた！");
                break;

            case RandomEventType.Fountain:
                Player.Heal(Player.MaxHp / 5);
                AddMessage("泉の水を飲んで体力が回復した。");
                break;

            case RandomEventType.Shrine:
                Player.RestoreMp(Player.MaxMp / 4);
                AddMessage("祠で祈りを捧げ、魔力が回復した。");
                break;

            case RandomEventType.RestPoint:
                Player.Heal(Player.MaxHp / 10);
                Player.ModifyFatigue(-20.0);
                AddMessage("安全な場所で少し休息を取った。疲労が回復した。");
                break;

            case RandomEventType.MaterialDeposit:
                var materialItem = ItemDefinitions.Create("material_herb");
                if (materialItem != null)
                {
                    ((Inventory)Player.Inventory).Add(materialItem);
                    AddMessage($"素材採取場で{materialItem.Name}を入手した！");
                }
                break;

            case RandomEventType.Trap:
                int trapDmg = Math.Max(1, Player.MaxHp / 15);
                Player.TakeDamage(Damage.Physical(trapDmg));
                _lastDamageCause = DeathCause.Trap;
                AddMessage($"⚠ 罠にかかった！ {trapDmg}ダメージを受けた！");
                break;

            case RandomEventType.Ruins:
                AddMessage("🏚 遺跡を発見した。何か有益な情報が得られるかもしれない。");
                break;

            case RandomEventType.MysteriousItem:
                AddMessage("✨ 不思議な輝きを放つ物体を発見した…");
                break;

            case RandomEventType.MonsterHouse:
                AddMessage("⚠ モンスターハウスだ！大量の敵が現れた！");
                SpawnEnemies();
                break;

            case RandomEventType.CursedRoom:
                AddMessage("💀 呪われた部屋に足を踏み入れた…不吉な気配が漂う。");
                break;

            case RandomEventType.BlessedRoom:
                Player.Heal(Player.MaxHp / 4);
                Player.RestoreMp(Player.MaxMp / 4);
                AddMessage("✨ 祝福された部屋だ！体力と魔力が回復した。");
                break;

            case RandomEventType.HiddenShop:
                AddMessage("🏪 隠しショップを発見した！珍しい品が並んでいる。");
                break;
        }
    }

    /// <summary>AR-8: 誓約違反チェック</summary>
    private void CheckOathViolation(string action)
    {
        foreach (var oath in _oathSystem.ActiveOaths.ToList())
        {
            if (_oathSystem.IsViolation(oath, action))
            {
                AddMessage($"⚠ 誓約「{OathSystem.GetDefinition(oath)?.Name ?? oath.ToString()}」に違反した！誓約が解除された。");
                _oathSystem.BreakOath(oath);
                // 違反ペナルティ: 信仰度低下
                Player.AddFaithPoints(-10);
            }
        }
    }

    /// <summary>AL-3: 天候・ペット補正付きFOV半径を取得</summary>
    private int GetEffectiveViewRadius(int baseRadius = GameConstants.DefaultViewRadius)
    {
        float sightMod = WeatherSystem.GetSightModifier(CurrentWeather);
        int petViewBonus = _petSystem.GetPetAbilityBonuses().ViewRadiusBonus;
        return Math.Max(2, (int)(baseRadius * sightMod) + petViewBonus);
    }

    /// <summary>投資システム</summary>
    public InvestmentSystem GetInvestmentSystem() => _investmentSystem;

    /// <summary>AJ-1: 商人ギルドシステム</summary>
    public MerchantGuildSystem GetMerchantGuildSystem() => _merchantGuildSystem;

    /// <summary>AJ-2: 領地影響力システム</summary>
    public TerritoryInfluenceSystem GetTerritoryInfluenceSystem() => _territoryInfluenceSystem;

    /// <summary>AY-1: ペットに食事を与える</summary>
    public bool TryFeedPet(string petId)
    {
        if (!_petSystem.Pets.ContainsKey(petId)) return false;
        var pet = _petSystem.Feed(petId);
        AddMessage($"🍖 {pet.Name}に食事をやった！（空腹度: {pet.Hunger}、忠誠度: {pet.Loyalty}）");
        OnStateChanged?.Invoke();
        return true;
    }

    /// <summary>AY-1: ペットを訓練する</summary>
    public bool TryTrainPet(string petId)
    {
        if (!_petSystem.Pets.ContainsKey(petId)) return false;
        var pet = _petSystem.Train(petId);
        AddMessage($"📖 {pet.Name}を訓練した！（レベル: {pet.Level}）");
        OnStateChanged?.Invoke();
        return true;
    }

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
            else
            {
                AddMessage($"⚠ 素材が見つからなかった: {ingredient}");
            }
        }

        // ET-1: 料理品質はレベルではなく料理(錬金)熟練度で決定
        int cookingProficiency = _proficiencySystem.GetLevel(ProficiencyCategory.Alchemy);

        // ET-2: 料理失敗判定（低熟練度ほど失敗しやすい）
        float failRate = Math.Max(0f, 0.3f - cookingProficiency * 0.01f);
        if (_random.NextDouble() < failRate)
        {
            AddMessage($"🍳 {recipe.Name}の調理に失敗してしまった…（熟練度を上げよう）");
            _proficiencySystem.GainExperience(ProficiencyCategory.Alchemy, 1);
            OnStateChanged?.Invoke();
            return false;
        }

        // 料理結果の品質計算
        float quality = CookingSystem.CalculateQuality(cookingProficiency);
        int hpRestore = (int)(recipe.HpRestore * quality);
        int mpRestore = (int)(recipe.MpRestore * quality);

        _proficiencySystem.GainExperience(ProficiencyCategory.Alchemy, 3);
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
        // O-2: 転職実行 — クラス更新＋ステータス再計算
        var oldClassDef = ClassDefinition.Get(Player.CharacterClass);
        Player.ChangeClass(targetClass);
        var newClassDef = ClassDefinition.Get(targetClass);
        // BaseStatsから旧クラスボーナスを除去し、新クラスボーナスを適用
        var statDiff = newClassDef.StatBonus - oldClassDef.StatBonus;
        Player.ApplyStatModifierToBase(statDiff);
        Player.BonusMaxHp = RaceDefinition.Get(Player.Race).HpBonus + newClassDef.HpBonus;
        Player.BonusMaxMp = RaceDefinition.Get(Player.Race).MpBonus + newClassDef.MpBonus;
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

    /// <summary>AP-1: 拠点施設を建設</summary>
    public bool TryBuildFacility(FacilityCategory category)
    {
        int materials = Player.Gold; // 素材コストをゴールドで代用
        var (success, cost) = _baseConstructionSystem.Build(category, materials);
        if (success)
        {
            Player.SpendGold(cost);
            AddMessage($"🏗 {category}を建設した！（{cost}G消費）");
            OnStateChanged?.Invoke();
            return true;
        }
        AddMessage("建設条件を満たしていない");
        return false;
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

        // 疲労回復（新仕様: 低い値=良い状態）
        if (fatigueRecovery > 0.5f)
        {
            // 高品質の休息: 疲労度を宿屋回復レベルまで回復
            var innStart = FatigueSystem.GetInnRecoveryStart(Player.FatigueStage);
            Player.ModifyFatigue(innStart - Player.Fatigue);
        }
        else if (fatigueRecovery > 0.2f)
        {
            Player.ModifyFatigue(-30.0);
        }

        // 衛生回復（宿屋利用時）
        if (quality >= SleepQuality.DeepSleep) Player.ModifyHygiene(GameConstants.MaxHygiene - Player.Hygiene);

        // 拠点休息ボーナス
        float restBonus = GetBaseRestBonus();
        if (restBonus > 1.0f) hpAmount = (int)(hpAmount * restBonus);

        TurnCount += sleepDuration;
        GameTime.AdvanceTurn(sleepDuration);

        // AT-3: 休息中の食料/水消費（睡眠ターン分の空腹・渇き減少）
        int hungerCost = (int)(sleepDuration * DifficultyConfig.HungerDecayMultiplier * 0.5);
        int thirstCost = (int)(sleepDuration * DifficultyConfig.HungerDecayMultiplier * 0.3);
        Player.ModifyHunger(-hungerCost);
        Player.ModifyThirst(-thirstCost);

        AddMessage($"💤 {RestSystem.GetQualityName(quality)}な休息をとった (HP+{hpAmount}, 疲労回復)");
        AddMessage($"  {sleepDuration}ターン経過 (空腹-{hungerCost}, 渇き-{thirstCost})");
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

        Player.SpendGold(betAmount);
        bool won = false;

        switch (gameType)
        {
            case GamblingGameType.Dice:
                int diceResult = _random.Next(6) + 1;
                won = GamblingSystem.JudgeDice(playerGuess, diceResult);
                string guessLabel = playerGuess >= 4 ? "大(4-6)" : "小(1-3)";
                AddMessage($"🎲 あなたの予想: {guessLabel}  サイコロの目: {diceResult}");
                break;
            case GamblingGameType.ChoHan:
                int dice1 = _random.Next(6) + 1;
                int dice2 = _random.Next(6) + 1;
                won = GamblingSystem.JudgeChoHan(playerGuess == 0, dice1, dice2);
                string choHanGuess = playerGuess == 0 ? "丁(偶数)" : "半(奇数)";
                AddMessage($"🎲 あなたの予想: {choHanGuess}  丁半: {dice1}+{dice2}={dice1 + dice2} ({((dice1 + dice2) % 2 == 0 ? "丁" : "半")})");
                break;
            case GamblingGameType.Card:
                int currentCard = _random.Next(13) + 1;
                int nextCard = _random.Next(13) + 1;
                var highLowResult = GamblingSystem.JudgeHighLow(playerGuess == 1, currentCard, nextCard);
                if (highLowResult == null)
                {
                    // AG-2: 引き分け — 賭け金返却
                    string cardGuessD = playerGuess == 1 ? "ハイ" : "ロー";
                    AddMessage($"🃏 あなたの予想: {cardGuessD}  ハイ＆ロー: {currentCard} → {nextCard}");
                    AddMessage($"🤝 引き分け！ 賭け金{betAmount}Gは返却されます");
                    Player.AddGold(betAmount);
                    TurnCount += 5;
                    GameTime.AdvanceTurn(5);
                    OnStateChanged?.Invoke();
                    return true;
                }
                won = highLowResult.Value;
                string cardGuess = playerGuess == 1 ? "ハイ" : "ロー";
                AddMessage($"🃏 あなたの予想: {cardGuess}  ハイ＆ロー: {currentCard} → {nextCard}");
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

        // 賭博はターンを消費する
        TurnCount += 5;
        GameTime.AdvanceTurn(5);
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

        // EN-2: 宝判定（残りの確率空間で正規化）
        float treasureRate = FishingSystem.CalculateTreasureRate(fishingLevel, Player.EffectiveStats.Luck * 0.01f);
        float normalizedTreasureRate = Math.Min(treasureRate / (1f - junkRate), 0.5f);  // 最大50%
        if (_random.NextDouble() < normalizedTreasureRate)
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

    // === 生態系・派閥・鑑定 システム統合 ===

    /// <summary>ダンジョン生態系の捕食関係を初期化</summary>
    private void InitializeEcosystemRelations()
    {
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast, 90);
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Beast, MonsterRace.Insect, 70);
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Demon, MonsterRace.Humanoid, 60);
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Spirit, MonsterRace.Undead, 50);
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Insect, MonsterRace.Plant, 80);
        _dungeonEcosystemSystem.RegisterRelation(MonsterRace.Dragon, MonsterRace.Demon, 40);
    }

    /// <summary>アイテム鑑定を試みる</summary>
    public bool TryIdentifyItem(string itemId, string trueName)
    {
        if (_itemIdentificationSystem.IsIdentified(itemId)) return false;
        var result = _itemIdentificationSystem.Identify(itemId, trueName);
        AddMessage($"🔍 {result.TrueName}の正体が判明した！ {result.Description}");
        if (result.Curse != CurseType.None)
        {
            AddMessage($"⚠ 呪いが検出された: {ItemIdentificationSystem.GetCurseDescription(result.Curse)}");
        }
        return true;
    }

    /// <summary>碑文の解読を試みる</summary>
    public bool TryDecodeInscription(string inscriptionId)
    {
        var result = _inscriptionSystem.TryDecode(inscriptionId, Player.EffectiveStats.Intelligence);
        if (result.Success)
        {
            AddMessage($"📜 碑文を解読した: 「{result.Message}」");
            if (result.RewardInfo != null)
            {
                AddMessage($"  → {result.RewardInfo}");
            }
        }
        else
        {
            AddMessage($"📜 {result.Message}");
        }
        return result.Success;
    }

    /// <summary>商人ギルドに加入</summary>
    public bool TryJoinMerchantGuild()
    {
        var membership = _merchantGuildSystem.JoinGuild(Player.Name);
        if (membership != null)
        {
            AddMessage($"🏪 商人ギルドに加入した！ ランク: {membership.Rank}");

            // AJ-1: 初期交易路を自動確立
            var currentTerritory = _worldMapSystem.CurrentTerritory;
            var nearbyTerritories = new[] { TerritoryId.Capital, TerritoryId.Forest, TerritoryId.Mountain, TerritoryId.Coast };
            foreach (var dest in nearbyTerritories)
            {
                if (dest != currentTerritory)
                {
                    _merchantGuildSystem.EstablishRoute($"route_{currentTerritory}_{dest}", currentTerritory, dest, 50);
                    break;  // 初期は1路線のみ
                }
            }

            return true;
        }
        return false;
    }

    /// <summary>NPC好感度による割引率を取得</summary>
    public float GetNpcShopDiscount(string npcId)
    {
        int relation = _relationshipSystem.GetRelation(RelationshipType.Personal, "player", npcId);
        return RelationshipSystem.GetShopDiscount(relation);
    }

    /// <summary>NPC関係値を更新</summary>
    public void ModifyNpcRelation(string npcId, int delta)
    {
        // DB-2: カルマランクによるNPC態度修飾子を適用
        double dispositionMod = _karmaSystem.GetNpcDispositionModifier();
        int adjustedDelta = (int)(delta * dispositionMod);
        _relationshipSystem.ModifyRelation(RelationshipType.Personal, "player", npcId, adjustedDelta);
    }

    /// <summary>GridInventorySystem: グリッドインベントリの空き率を取得</summary>
    public float GetGridInventoryFreeSpace() => _gridInventorySystem.GetFreeSpaceRatio();

    /// <summary>GridInventorySystem: アイテムをグリッドに配置</summary>
    public bool TryPlaceItemInGrid(string itemId, string name, GridItemSize size)
    {
        // 自動配置: 空きスロットを探して配置
        var (w, h) = GridInventorySystem.GetDimensions(size);
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (_gridInventorySystem.CanPlace(size, x, y))
                {
                    return _gridInventorySystem.PlaceItem(itemId, name, size, x, y);
                }
            }
        }
        return false;
    }

    /// <summary>GridInventorySystem: アイテムをグリッドから除去</summary>
    public bool RemoveItemFromGrid(string itemId) => _gridInventorySystem.RemoveItem(itemId);

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

            // AM-1/AM-2: 採集したアイテムをインベントリに追加
            if (node != null && node.PossibleItems.Length > 0)
            {
                // レアの場合は後ろの方のアイテム、通常は前の方
                int itemIndex;
                if (isRare && node.PossibleItems.Length > 1)
                    itemIndex = _random.Next(node.PossibleItems.Length / 2, node.PossibleItems.Length);
                else
                    itemIndex = _random.Next(Math.Min(3, node.PossibleItems.Length));
                string itemId = node.PossibleItems[itemIndex];
                var item = ItemDefinitions.Create(itemId);
                if (item != null)
                {
                    ((Inventory)Player.Inventory).Add(item);
                    AddMessage($"🌿 {nodeName}から{item.Name}を{(isRare ? "レアな素材として" : "")}採集した！");
                }
                else
                {
                    AddMessage($"🌿 {nodeName}から{(isRare ? "レアな" : "")}素材を採集した！");
                }
            }
            else
            {
                AddMessage($"🌿 {nodeName}から{(isRare ? "レアな" : "")}素材を採集した！");
            }
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
            // 装備品の耐久値を実際に回復 (DZ-1)
            var equipment = Player.Equipment.GetAll().Values
                .Where(e => e != null && e.Name == itemName)
                .FirstOrDefault();
            if (equipment != null)
            {
                equipment.Durability = Math.Min(equipment.MaxDurability, equipment.Durability + repairAmount);
            }
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
        const float detectionChance = 0.3f;
        bool evaded = SmugglingSystem.CheckEvasion(detectionChance, Player.EffectiveStats.Dexterity, _random.NextDouble());

        if (!evaded)
        {
            int penalty = SmugglingSystem.GetPenalty(contraband.Type);
            Player.SpendGold(Math.Min(penalty, Player.Gold));
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

        Player.SpendGold(recipe.MaterialCost);
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

        Player.SpendGold(item.Price);
        _karmaSystem.ModifyKarma(-2, "闇市場");
        // 闇市場取引はターンを消費する
        TurnCount += 5;
        GameTime.AdvanceTurn(5);
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

    /// <summary>疲労値</summary>
    public double PlayerFatigue => Player.Fatigue;

    /// <summary>疲労段階</summary>
    public FatigueStage PlayerFatigueStage => Player.FatigueStage;

    /// <summary>疲労名</summary>
    public string PlayerFatigueName => BodyConditionSystem.GetFatigueName(Player.FatigueStage);

    /// <summary>衛生値</summary>
    public int PlayerHygiene => Player.Hygiene;

    /// <summary>衛生段階</summary>
    public HygieneStage PlayerHygieneStage => Player.HygieneStage;

    /// <summary>衛生名</summary>
    public string PlayerHygieneName => BodyConditionSystem.GetHygieneName(Player.HygieneStage);

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
            _worldMapSystem.VisitedTerritories.Count >= Enum.GetValues<TerritoryId>().Length, _clearRank);
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

    /// <summary>
    /// シンボルマップイベントの効果を発動する。
    /// 各イベントIDに応じた具体的な効果を適用。
    /// </summary>
    private void ResolveMapEvent(SymbolMapEventSystem.MapEvent mapEvent)
    {
        switch (mapEvent.Id)
        {
            case "event_merchant_caravan":
                // 行商キャラバン: ランダムアイテムを安く入手できるチャンス
                AddMessage("    → 商人が珍しい品を見せてくれた。");
                break;

            case "event_bandit_ambush":
                // 山賊の待ち伏せ: ダメージを受ける
                int ambushDmg = Math.Max(1, Player.MaxHp / 10);
                Player.TakeDamage(Damage.Physical(ambushDmg));
                AddMessage($"    → 不意打ちを受けた！ {ambushDmg}のダメージ！");
                break;

            case "event_wandering_healer":
                // 放浪の治療師: HP完全回復
                Player.Heal(Player.MaxHp);
                AddMessage("    → HPが全回復した！");
                break;

            case "event_ancient_shrine":
                // 古代の祠: 攻撃力一時バフ（メッセージのみ、バフシステム未実装のため）
                int shrineHeal = Math.Max(5, Player.MaxHp / 5);
                Player.Heal(shrineHeal);
                AddMessage($"    → 祠に祈りを捧げた。HPが{shrineHeal}回復した。");
                break;

            case "event_treasure_map":
                // 宝の地図: ゴールド獲得
                int goldFound = 50 + _random.Next(100);
                Player.AddGold(goldFound);
                AddMessage($"    → 地図の示す場所から{goldFound}Gを発見した！");
                break;

            case "event_monster_stampede":
                // 魔物の大移動: 大ダメージ
                int stampedeDmg = Math.Max(3, Player.MaxHp / 5);
                Player.TakeDamage(Damage.Physical(stampedeDmg));
                AddMessage($"    → 魔物の群れに巻き込まれた！ {stampedeDmg}のダメージ！");
                break;

            case "event_fallen_star":
                // 流れ星: 経験値ボーナス
                int expBonus = 50 + Player.Level * 10;
                Player.GainExperience(expBonus);
                AddMessage($"    → 流れ星の欠片を拾った！ 経験値{expBonus}を獲得！");
                break;

            case "event_refugee":
                // 避難民: カルマ上昇（メッセージのみ）
                AddMessage("    → 避難民に食料を分け与えた。感謝された。");
                break;

            case "event_storm_shelter":
                // 嵐の避難所: HP/MP回復
                Player.Heal(Player.MaxHp / 3);
                Player.RestoreMp(Player.MaxMp / 3);
                AddMessage("    → 避難所で休息した。HP/MPが回復した。");
                break;

            case "event_fairy_ring":
                // 妖精の輪: MP完全回復
                Player.RestoreMp(Player.MaxMp);
                AddMessage("    → MPが全回復した！");
                break;

            case "event_sandstorm":
                // 砂嵐: 軽ダメージ
                int sandDmg = Math.Max(1, Player.MaxHp / 15);
                Player.TakeDamage(Damage.Physical(sandDmg));
                AddMessage($"    → 砂嵐にさらされた！ {sandDmg}のダメージ！");
                break;

            case "event_swamp_miasma":
                // 瘴気の濃霧: 毒ダメージ
                int miasmaDmg = Math.Max(1, Player.MaxHp / 12);
                Player.TakeDamage(Damage.Magical(miasmaDmg, Element.Poison));
                AddMessage($"    → 瘴気に侵された！ {miasmaDmg}の毒ダメージ！");
                break;

            case "event_blizzard":
                // 猛吹雪: 凍傷ダメージ
                int blizzardDmg = Math.Max(1, Player.MaxHp / 12);
                Player.TakeDamage(Damage.Magical(blizzardDmg, Element.Ice));
                AddMessage($"    → 猛吹雪に襲われた！ {blizzardDmg}の冷気ダメージ！");
                break;

            case "event_lake_mist":
                // 湖上の幻霧: 方向感覚喪失（メッセージのみ）
                AddMessage("    → 霧の中で方向感覚を失いかけた…");
                break;

            case "event_eruption":
                // 火山噴火: 大ダメージ
                int lavaDmg = Math.Max(5, Player.MaxHp / 6);
                Player.TakeDamage(Damage.Magical(lavaDmg, Element.Fire));
                AddMessage($"    → 溶岩弾が直撃した！ {lavaDmg}の炎ダメージ！");
                break;

            case "event_divine_light":
                // 神聖な光: HP全回復
                Player.Heal(Player.MaxHp);
                Player.RestoreMp(Player.MaxMp);
                AddMessage("    → 聖なる光に包まれ、HP/MPが全回復した！");
                break;

            default:
                // バイオーム地形イベント処理
                if (mapEvent.Id.StartsWith("terrain_"))
                {
                    ResolveTerrainEvent(mapEvent);
                }
                break;
        }
    }

    /// <summary>
    /// バイオーム固有地形イベントの効果を発動する。
    /// </summary>
    private void ResolveTerrainEvent(SymbolMapEventSystem.MapEvent terrainEvent)
    {
        switch (terrainEvent.Id)
        {
            case "terrain_dune_heat":
                int heatDmg = Math.Max(1, Player.MaxHp / 20);
                Player.TakeDamage(Damage.Physical(heatDmg));
                AddMessage($"    → 灼熱の砂に体力を奪われた！ {heatDmg}のダメージ！");
                break;

            case "terrain_lava_eruption":
                int lavaEruptDmg = Math.Max(3, Player.MaxHp / 8);
                Player.TakeDamage(Damage.Magical(lavaEruptDmg, Element.Fire));
                AddMessage($"    → 溶岩が噴出した！ {lavaEruptDmg}の炎ダメージ！");
                break;

            case "terrain_ice_slip":
                int iceDmg = Math.Max(1, Player.MaxHp / 25);
                Player.TakeDamage(Damage.Physical(iceDmg));
                AddMessage($"    → 氷の上で滑って転倒！ {iceDmg}のダメージ！");
                break;

            case "terrain_swamp_poison":
                int poisonDmg = Math.Max(1, Player.MaxHp / 15);
                Player.TakeDamage(Damage.Magical(poisonDmg, Element.Poison));
                AddMessage($"    → 毒沼の瘴気に侵された！ {poisonDmg}の毒ダメージ！");
                break;

            default:
                break;
        }
    }

    /// <summary>自動探索の停止条件チェック（AutoExploreSystem）</summary>
    public AutoExploreSystem.StopReason? CheckAutoExploreStop()
    {
        bool hasEnemyNearby = IsInCombat();
        float hpRatio = Player.MaxHp > 0 ? (float)Player.CurrentHp / Player.MaxHp : 1f;  // B.28: 除算ゼロ対策
        float hungerRatio = (float)Player.Hunger / 100f;
        bool itemOnTile = GroundItems.Any(i => i.Position == Player.Position);
        var tile = Map.GetTile(Player.Position);
        bool stairsNearby = tile.Type is TileType.StairsDown or TileType.StairsUp;
        return AutoExploreSystem.CheckStopConditions(true, hasEnemyNearby, itemOnTile, stairsNearby, false, hpRatio, hungerRatio);
    }

    /// <summary>
    /// Ver.prt: シンボルマップ上で許可されるアクションかどうか判定する。
    /// 許可: 移動、インベントリ開閉、アイテム使用、装備変更、ログ/死亡録等の情報確認、
    /// フィールド/ダンジョン/村/町/都への進入、ワールドマップ、領地移動、待機
    /// </summary>
    private static bool IsAllowedOnSymbolMap(GameAction action)
    {
        return action is
            // 移動
            GameAction.MoveUp or GameAction.MoveDown or
            GameAction.MoveLeft or GameAction.MoveRight or
            GameAction.MoveUpLeft or GameAction.MoveUpRight or
            GameAction.MoveDownLeft or GameAction.MoveDownRight or
            // 待機
            GameAction.Wait or
            // インベントリ（アイテム使用・装備変更含む）
            GameAction.OpenInventory or
            // ステータス・ログ・情報確認
            GameAction.OpenStatus or
            GameAction.OpenMessageLog or
            // ロケーション進入（フィールド/ダンジョン/村/町/都）
            GameAction.EnterTown or
            GameAction.UseStairs or
            GameAction.AscendStairs or
            // ワールドマップ・領地移動
            GameAction.OpenWorldMap or
            GameAction.TravelToTerritory or
            // 階段（ダンジョン進入用）
            GameAction.LeaveTown;
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

    #region ヘルパーメソッド（システム統合用）

    /// <summary>位置Aから位置Bへの方向を計算</summary>
    private static Direction GetDirectionToTarget(Position from, Position to)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;

        if (dx == 0 && dy < 0) return Direction.North;
        if (dx == 0 && dy > 0) return Direction.South;
        if (dx > 0 && dy == 0) return Direction.East;
        if (dx < 0 && dy == 0) return Direction.West;
        if (dx > 0 && dy < 0) return Direction.NorthEast;
        if (dx < 0 && dy < 0) return Direction.NorthWest;
        if (dx > 0 && dy > 0) return Direction.SouthEast;
        return Direction.SouthWest;
    }

    /// <summary>現在のダンジョン特徴名を取得</summary>
    public string GetCurrentDungeonFeatureName()
    {
        return _currentDungeonFeature.HasValue
            ? DungeonFeatureGenerator.GetFeatureName(_currentDungeonFeature.Value)
            : "";
    }

    /// <summary>現在の環境音情報を取得</summary>
    public AmbientSoundSystem.AmbientSoundEvent GetCurrentAmbientSound()
    {
        return AmbientSoundSystem.CreateEvent(_currentAmbientSound);
    }

    /// <summary>セーブスロット一覧を取得</summary>
    public IReadOnlyList<MultiSlotSaveSystem.SaveSlotInfo> GetSaveSlots() => _multiSlotSaveSystem.GetAllSlots();

    /// <summary>指定スロットにセーブ</summary>
    public bool SaveToSlot(int slotNumber)
    {
        var location = _worldMapSystem.IsOnSurface
            ? _worldMapSystem.GetCurrentTerritoryInfo().Name
            : $"{_currentMapName} B{CurrentFloor}F";
        return _multiSlotSaveSystem.SaveToSlot(slotNumber, Player.Name, Player.Level, location);
    }

    /// <summary>プレイヤーの向きを取得</summary>
    public Direction PlayerFacing => _playerFacing;

    /// <summary>地表面×属性魔法の相互作用を適用し、修正後ダメージを返す（EnvironmentalCombatSystem）</summary>
    private int ApplyEnvironmentalInteraction(Position targetPos, Element element, int baseDamage)
    {
        var currentSurface = _surfaceMap.TryGetValue(targetPos, out var s)
            ? s
            : EnvironmentalCombatSystem.SurfaceType.Normal;

        var interaction = EnvironmentalCombatSystem.GetInteraction(currentSurface, element);
        if (interaction == null)
            return baseDamage;

        // 地表面を変化させる
        if (interaction.ResultSurface != EnvironmentalCombatSystem.SurfaceType.Normal)
        {
            _surfaceMap[targetPos] = interaction.ResultSurface;
        }
        else
        {
            _surfaceMap.Remove(targetPos);
        }

        AddMessage($"🌊 {interaction.Description}");
        return Math.Max(1, (int)(baseDamage * interaction.DamageMultiplier));
    }

    /// <summary>指定位置の地表面タイプを取得</summary>
    public EnvironmentalCombatSystem.SurfaceType GetSurfaceAt(Position pos)
    {
        return _surfaceMap.TryGetValue(pos, out var surface)
            ? surface
            : EnvironmentalCombatSystem.SurfaceType.Normal;
    }

    // === StealSystem接続 ===

    /// <summary>隣接する敵からの盗み試行（StealSystem）</summary>
    private void TryStealFromAdjacentEnemy()
    {
        var adjacentEnemy = Enemies.FirstOrDefault(e => e.IsAlive && e.Position.ChebyshevDistanceTo(Player.Position) <= 1);
        if (adjacentEnemy == null)
        {
            AddMessage("盗む対象がいない");
            return;
        }
        int stealSkill = (int)_skillSystem.GetPassiveBonus("steal");
        // 敵レベル推定: 経験値報酬÷10（EnemyにLevelプロパティがないため推定）
        int estimatedEnemyLevel = Math.Max(1, adjacentEnemy.ExperienceReward / 10);
        // 所持ゴールド推定: 経験値報酬÷2
        int estimatedGold = Math.Max(0, adjacentEnemy.ExperienceReward / 2);
        var result = StealSystem.AttemptSteal(
            Player.EffectiveStats.Dexterity, Player.Level, estimatedEnemyLevel,
            estimatedGold, adjacentEnemy.DropTableId != null,
            stealSkill, _random);
        AddMessage(result.Message);
        if (result.Success && result.StolenGold > 0)
        {
            Player.AddGold(result.StolenGold);
        }
        if (result.Detected)
        {
            _karmaSystem.ModifyKarma(-5, "盗み発覚"); // 盗み発覚時のカルマペナルティ（-5）
            AddMessage("⚠ 敵が怒り、攻撃態勢に入った！");
        }
    }

    /// <summary>盗み成功率を取得（UI表示用・StealSystem）</summary>
    public float GetStealChance(int targetLevel)
    {
        int stealSkill = (int)_skillSystem.GetPassiveBonus("steal");
        return StealSystem.CalculateStealChance(
            Player.EffectiveStats.Dexterity, Player.Level, targetLevel, stealSkill);
    }

    // === AutoSaveSystem接続 ===

    private readonly AutoSaveSystem _autoSaveSystem = new();

    /// <summary>自動セーブ判定とトリガー（AutoSaveSystem）</summary>
    private void CheckAutoSave(AutoSaveTrigger trigger)
    {
        if (_autoSaveSystem.ShouldAutoSave(TurnCount, trigger))
        {
            OnSaveGame?.Invoke();
            _autoSaveSystem.MarkSaved(TurnCount);
        }
    }

    /// <summary>AutoSaveSystemを取得</summary>
    public AutoSaveSystem GetAutoSaveSystem() => _autoSaveSystem;

    // === ModLoaderSystem公開アクセサ ===

    /// <summary>ModLoaderSystemを取得</summary>
    public ModLoaderSystem GetModLoaderSystem() => _modLoaderSystem;

    // === ContextHelpSystem状況別ヘルプ接続 ===

    /// <summary>ゲーム状態に応じたコンテキストヘルプを取得（ContextHelpSystem）</summary>
    public IReadOnlyList<ContextHelpSystem.HelpTopic> GetContextualHelpForState()
    {
        string context = IsInCombat() ? "戦闘" : _worldMapSystem.IsOnSurface ? "地上" : "ダンジョン";
        return _contextHelpSystem.GetContextualHelp(context);
    }

    /// <summary>キー操作に対するヘルプを取得（ContextHelpSystem）</summary>
    public ContextHelpSystem.HelpTopic? GetHelpForKey(string key)
    {
        return _contextHelpSystem.GetHelpForKey(key);
    }

    // === AccessibilitySystem活用接続 ===

    /// <summary>アクセシビリティ設定に基づく実効フォントサイズを取得</summary>
    public int GetEffectiveFontSize(int baseFontSize = 14)
    {
        return _accessibilitySystem.CalculateEffectiveFontSize(baseFontSize);
    }

    /// <summary>アクセシビリティ設定に基づく実効ターン遅延を取得（ミリ秒）</summary>
    public int GetEffectiveTurnDelay(int baseDelay = 200)
    {
        return _accessibilitySystem.CalculateEffectiveTurnDelay(baseDelay);
    }

    /// <summary>色覚モードに応じた色変換を取得（AccessibilitySystem）</summary>
    public AccessibilitySystem.ColorTransform GetTransformedColor(string originalColor)
    {
        return _accessibilitySystem.TransformColor(originalColor);
    }

    // === ModularHudSystem活用接続 ===

    /// <summary>HUD要素の表示状態を確認（ModularHudSystem）</summary>
    public bool IsHudElementVisible(HudElement element)
    {
        var config = _modularHudSystem.GetConfig(element);
        return config?.IsVisible ?? true;
    }

    /// <summary>表示中のHUD要素数を取得（ModularHudSystem）</summary>
    public int GetVisibleHudElementCount()
    {
        return _modularHudSystem.GetVisibleCount();
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
    OpenBaseConstruction,
    OpenVocabulary,
    Steal
}
