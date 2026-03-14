using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Data.MagicLanguage;
using RougelikeGame.Engine;
using RougelikeGame.Engine.Combat;
using RougelikeGame.Engine.TurnSystem;
using System.Runtime.InteropServices;

namespace RougelikeGame.App;

partial class Program
{
    // Windows API for console code page
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleOutputCP(uint wCodePageID);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCP(uint wCodePageID);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    static void Main(string[] args)
    {
        // コンソールのエンコーディング設定（日本語対応）
        ConfigureConsoleEncoding();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              ローグライクゲーム デモ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  1. インタラクティブモード（実際にプレイ）");
        Console.WriteLine("  2. システムデモ（各システムの動作確認）");
        Console.WriteLine("  Q. 終了");
        Console.WriteLine();
        Console.Write("選択してください: ");

        var key = Console.ReadKey();
        Console.WriteLine();

        switch (key.Key)
        {
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                RunInteractiveGame();
                break;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                RunSystemDemo();
                break;
            case ConsoleKey.Q:
                return;
            default:
                Console.WriteLine("無効な選択です");
                break;
        }
    }

    static void RunInteractiveGame()
    {
        var random = new RandomProvider();
        var combatSystem = new CombatSystem(random);
        var demo = new InteractiveGameDemo(random, combatSystem);
        demo.Run();
    }

    static void RunSystemDemo()
    {
        Console.WriteLine("\n=== ローグライクゲーム ===\n");

        // DI Container Setup
        var services = new ServiceCollection();
        ConfigureServices(services);

        using var provider = services.BuildServiceProvider();

        // Game Initialization
        var game = new Game(provider);
        game.Initialize();

        // デモの実行
        RunDemo(game);

        Console.WriteLine("\n何かキーを押して終了...");
        Console.ReadKey();
    }

    static void ConfigureConsoleEncoding()
    {
        // .NETのエンコーディング設定
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Windowsでの追加設定
        if (OperatingSystem.IsWindows())
        {
            try
            {
                // コードページをUTF-8(65001)に設定
                SetConsoleOutputCP(65001);
                SetConsoleCP(65001);

                // 仮想ターミナル処理を有効化（Windows 10以降）
                var handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (handle != IntPtr.Zero && GetConsoleMode(handle, out uint mode))
                {
                    SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
                }
            }
            catch
            {
                // 失敗しても続行
            }
        }
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IRandomProvider, RandomProvider>();
        services.AddSingleton<ICombatSystem, CombatSystem>();
        services.AddSingleton<ITurnManager, TurnManager>();
        services.AddSingleton<SpellParser>();
    }

    static void RunDemo(Game game)
    {
        Console.WriteLine("【ターン消費システム デモ】\n");

        // 各行動のターン消費を表示
        Console.WriteLine("=== 基本ターン消費 ===");
        Console.WriteLine($"通常移動（1マス）: {TurnCosts.MoveNormal}ターン = {TurnCosts.MoveNormal}秒");
        Console.WriteLine($"戦闘中移動（1マス）: {TurnCosts.MoveCombat}ターン = {TurnCosts.MoveCombat}秒");
        Console.WriteLine($"通常攻撃: {TurnCosts.AttackNormal}ターン = {TurnCosts.AttackNormal}秒");
        Console.WriteLine($"待機: {TurnCosts.Wait}ターン = {TurnCosts.Wait}秒");
        Console.WriteLine($"休息: {TurnCosts.Rest}ターン = {TurnCosts.Rest}秒");

        Console.WriteLine("\n=== 魔法言語（ルーン語）デモ ===\n");

        var spellParser = new SpellParser();
        var masteryLevels = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "fjandi", 60 },
            { "mikill", 40 },
            { "vidr", 30 },
            { "graeda", 100 },
            { "sjalfr", 90 }
        };

        // 簡単な魔法
        TestSpell(spellParser, masteryLevels, "brenna", "基本の炎魔法");
        TestSpell(spellParser, masteryLevels, "brenna fjandi", "敵を燃やす");
        TestSpell(spellParser, masteryLevels, "mikill brenna fjandi", "激しく敵を燃やす");
        TestSpell(spellParser, masteryLevels, "vidr mikill brenna fjandi", "広範囲に激しく敵を燃やす");
        TestSpell(spellParser, masteryLevels, "graeda sjalfr", "自己回復");

        Console.WriteLine("\n=== プレイヤーステータス ===\n");
        var player = Player.Create("冒険者", Stats.Default);
        PrintPlayerStatus(player);

        // ルーン語を習得
        Console.WriteLine("\n【ルーン語習得】");
        player.LearnWord("brenna");
        player.LearnWord("fjandi");
        player.LearnWord("graeda");
        Console.WriteLine($"習得済みルーン語: {string.Join(", ", player.LearnedWords.Keys)}");

        // 敵AIデモ
        RunEnemyDemo();

        // マップ生成デモ
        RunMapGenerationDemo();

        // アイテムシステムデモ
        RunItemSystemDemo();

        // 戦闘システムデモ
        RunCombatSystemDemo();
    }

    static void RunMapGenerationDemo()
    {
        Console.WriteLine("\n=== ダンジョンマップ生成システム デモ ===\n");

        // 小さめのマップを生成（表示用）
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 25,
            Depth = 1,
            RoomCount = 6,
            TrapDensity = 0.01f
        };

        var generator = new DungeonGenerator(12345); // 固定シードで再現性確保
        var map = (DungeonMap)generator.Generate(parameters);

        Console.WriteLine($"マップサイズ: {map.Width}x{map.Height}");
        Console.WriteLine($"階層: {map.Depth}");
        Console.WriteLine($"部屋数: {map.Rooms.Count}");
        Console.WriteLine($"床タイル数: {map.CountFloorTiles()}");

        // 部屋情報を表示
        Console.WriteLine("\n【生成された部屋】");
        foreach (var room in map.Rooms)
        {
            Console.WriteLine($"  部屋{room.Id}: {room.Type} ({room.X},{room.Y}) {room.Width}x{room.Height}");
        }

        // 階段位置
        if (map.StairsUpPosition.HasValue)
            Console.WriteLine($"\n上り階段: ({map.StairsUpPosition.Value.X},{map.StairsUpPosition.Value.Y})");
        if (map.StairsDownPosition.HasValue)
            Console.WriteLine($"下り階段: ({map.StairsDownPosition.Value.X},{map.StairsDownPosition.Value.Y})");

        // マップをASCII表示
        Console.WriteLine("\n【ダンジョンマップ】");
        Console.WriteLine("凡例: # 壁  . 床  + ドア  < 上階段  > 下階段  ~ 水  ^ 罠");
        Console.WriteLine();

        // プレイヤー位置（入口）
        var playerPos = map.EntrancePosition ?? map.StairsUpPosition;

        // 視界を計算（プレイヤー位置から）
        if (playerPos.HasValue)
        {
            map.ComputeFov(playerPos.Value, 8);
        }

        // マップ表示
        Console.WriteLine(map.ToDebugString(playerPos));

        // タイルタイプの説明
        Console.WriteLine("\n【タイルタイプ一覧】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        var tileTypes = new[]
        {
            (TileType.Floor, "床", "通常の歩行可能な床"),
            (TileType.Wall, "壁", "通行不可・視線遮断"),
            (TileType.DoorClosed, "ドア", "開閉可能な扉"),
            (TileType.StairsUp, "上り階段", "上の階層へ移動"),
            (TileType.StairsDown, "下り階段", "下の階層へ移動"),
            (TileType.Water, "水", "移動コスト2倍"),
            (TileType.TrapHidden, "隠し罠", "踏むまで見えない"),
            (TileType.SecretDoor, "隠し扉", "発見するまで壁に見える")
        };

        foreach (var (type, name, desc) in tileTypes)
        {
            var tile = Tile.FromType(type);
            Console.WriteLine($"  {tile.DisplayChar} {name,-10} - {desc}");
        }

        // 複数のマップを生成して統計を取る
        Console.WriteLine("\n【マップ生成統計（10回生成）】");
        int totalRooms = 0;
        int totalFloors = 0;

        for (int i = 0; i < 10; i++)
        {
            var testGen = new DungeonGenerator();
            var testMap = (DungeonMap)testGen.Generate(parameters);
            totalRooms += testMap.Rooms.Count;
            totalFloors += testMap.CountFloorTiles();
        }

        Console.WriteLine($"  平均部屋数: {totalRooms / 10.0:F1}");
        Console.WriteLine($"  平均床タイル: {totalFloors / 10.0:F1}");
    }

    static void RunItemSystemDemo()
    {
        Console.WriteLine("\n=== アイテムシステム デモ ===\n");

        // 定義済み武器
        Console.WriteLine("【定義済み武器】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        var weapons = new[]
        {
            ItemFactory.CreateRustySword(),
            ItemFactory.CreateIronSword(),
            ItemFactory.CreateSteelSword(),
            ItemFactory.CreateDagger(),
            ItemFactory.CreateBattleAxe(),
            ItemFactory.CreateWoodenStaff(),
            ItemFactory.CreateShortBow()
        };

        foreach (var weapon in weapons)
        {
            Console.WriteLine($"  {weapon.GetDisplayName(),-16} " +
                            $"攻撃力:{weapon.DamageRange.Min}-{weapon.DamageRange.Max} " +
                            $"射程:{weapon.Range} " +
                            $"速度:{weapon.AttackSpeed:F1} " +
                            $"両手:{(weapon.IsTwoHanded ? "○" : "×")} " +
                            $"レア度:{weapon.Rarity}");
        }

        // 定義済み防具
        Console.WriteLine("\n【定義済み防具】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        var armorList = new Armor[]
        {
            ItemFactory.CreateLeatherArmor(),
            ItemFactory.CreateChainmail(),
            ItemFactory.CreatePlateArmor(),
            ItemFactory.CreateWizardRobe()
        };

        foreach (var armorItem in armorList)
        {
            Console.WriteLine($"  {armorItem.GetDisplayName(),-16} " +
                            $"防御:{armorItem.BaseDefense} " +
                            $"魔防:{armorItem.MagicDefense} " +
                            $"タイプ:{armorItem.ArmorType} " +
                            $"レア度:{armorItem.Rarity}");
        }

        // 定義済み盾
        Console.WriteLine("\n【定義済み盾】");
        var shields = new[]
        {
            ItemFactory.CreateWoodenShield(),
            ItemFactory.CreateIronShield()
        };

        foreach (var shield in shields)
        {
            Console.WriteLine($"  {shield.GetDisplayName(),-16} " +
                            $"防御:{shield.BaseDefense} " +
                            $"ブロック率:{shield.BlockChance:P0} " +
                            $"軽減:{shield.BlockReduction:P0}");
        }

        // 消費アイテム
        Console.WriteLine("\n【消費アイテム】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        var consumables = new ConsumableItem[]
        {
            ItemFactory.CreateMinorHealingPotion(),
            ItemFactory.CreateHealingPotion(),
            ItemFactory.CreateMinorManaPotion(),
            ItemFactory.CreateAntidote(),
            ItemFactory.CreateBread(),
            ItemFactory.CreateRation(),
            ItemFactory.CreateScrollOfIdentify(),
            ItemFactory.CreateScrollOfMagicMapping()
        };

        foreach (var item in consumables)
        {
            string effectInfo = item switch
            {
                Potion p => $"効果値:{p.EffectValue}",
                Food f => $"栄養:{f.NutritionValue}",
                Scroll s => $"対象:{s.TargetType}",
                _ => ""
            };
            Console.WriteLine($"  {item.GetDisplayName(),-20} {effectInfo,-15} 価格:{item.BasePrice}G");
        }

        // インベントリデモ
        Console.WriteLine("\n【インベントリ管理デモ】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var player = Player.Create("アイテムテスター", Stats.Default);
        var inventory = (Inventory)player.Inventory;

        Console.WriteLine($"初期状態: {inventory.UsedSlots}/{inventory.MaxSlots}スロット使用中");

        // アイテム追加
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        var potion = ItemFactory.CreateMinorHealingPotion();
        var bread = ItemFactory.CreateBread();

        inventory.Add(sword);
        inventory.Add(armor);
        inventory.Add(potion);
        inventory.Add(bread);

        Console.WriteLine($"\nアイテム追加後: {inventory.UsedSlots}スロット使用中");
        Console.WriteLine("所持アイテム:");
        foreach (var item in inventory.Items)
        {
            Console.WriteLine($"  - {item.GetDisplayName()} ({item.Type})");
        }

        // 装備
        Console.WriteLine("\n【装備デモ】");
        player.Equipment.Equip(sword, player);
        player.Equipment.Equip(armor, player);

        Console.WriteLine($"メイン武器: {player.Equipment.MainHand?.GetDisplayName() ?? "なし"}");
        Console.WriteLine($"防具: {player.Equipment[RougelikeGame.Core.Items.EquipmentSlot.Body]?.GetDisplayName() ?? "なし"}");

        var statMods = player.Equipment.GetStatModifiers().ToList();
        Console.WriteLine($"装備によるステータス修正: {statMods.Count}件");

        // ポーション使用
        Console.WriteLine("\n【アイテム使用デモ】");
        player.TakeDamage(Damage.Pure(30));
        Console.WriteLine($"ダメージ後HP: {player.CurrentHp}/{player.MaxHp}");

        var useResult = inventory.UseItem(potion, player);
        Console.WriteLine($"ポーション使用: {useResult?.Message}");
        Console.WriteLine($"回復後HP: {player.CurrentHp}/{player.MaxHp}");

        // ランダム生成
        Console.WriteLine("\n【ランダムアイテム生成】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var factory = new ItemFactory(42);
        Console.WriteLine("階層1の生成アイテム（5個）:");
        for (int i = 0; i < 5; i++)
        {
            var item = factory.GenerateRandomItem(1);
            Console.WriteLine($"  {item.GetDisplayName()} [{item.Rarity}]");
        }

        factory = new ItemFactory(42);
        Console.WriteLine("\n階層10の生成アイテム（5個）:");
        for (int i = 0; i < 5; i++)
        {
            var item = factory.GenerateRandomItem(10);
            Console.WriteLine($"  {item.GetDisplayName()} [{item.Rarity}]");
        }

        // レアリティ統計
        Console.WriteLine("\n【レアリティ分布（階層10、100回生成）】");
        factory = new ItemFactory();
        var rarityCounts = new Dictionary<ItemRarity, int>();
        for (int i = 0; i < 100; i++)
        {
            var item = factory.GenerateRandomItem(10);
            rarityCounts.TryGetValue(item.Rarity, out int count);
            rarityCounts[item.Rarity] = count + 1;
        }

        foreach (var (rarity, count) in rarityCounts.OrderBy(kv => kv.Key))
        {
            var bar = new string('█', count / 2);
            Console.WriteLine($"  {rarity,-12}: {count,3}% {bar}");
        }
    }

    static void RunEnemyDemo()
    {
        Console.WriteLine("\n=== 敵キャラクター・AIシステム デモ ===\n");

        var factory = new EnemyFactory();

        // 各種敵を生成
        Console.WriteLine("【定義済み敵キャラクター】");
        var enemies = new[]
        {
            factory.CreateSlime(new Position(10, 10)),
            factory.CreateGoblin(new Position(15, 10)),
            factory.CreateSkeleton(new Position(20, 10)),
            factory.CreateOrc(new Position(25, 10)),
            factory.CreateGiantSpider(new Position(10, 15)),
            factory.CreateDarkElf(new Position(15, 15)),
            factory.CreateTroll(new Position(20, 15)),
            factory.CreateDraugr(new Position(25, 15))
        };

        foreach (var enemy in enemies)
        {
            PrintEnemyInfo(enemy);
        }

        // 敵タイプと行動パターン
        Console.WriteLine("\n【敵タイプと行動パターン】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine($"{"タイプ",-12} {"説明",-40}");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine($"{"Normal",-12} {"バランス型。通常の追跡・戦闘・逃走行動",-40}");
        Console.WriteLine($"{"Aggressive",-12} {"攻撃的。広い範囲で敵を探し積極的に追跡",-40}");
        Console.WriteLine($"{"Defensive",-12} {"防御的。縄張りを守り、離れると戻る",-40}");
        Console.WriteLine($"{"Coward",-12} {"臆病。早めに逃走、HPが減ると逃げる",-40}");
        Console.WriteLine($"{"Ambusher",-12} {"待ち伏せ型。動かず待機、近づくと襲撃",-40}");
        Console.WriteLine($"{"Pack",-12} {"群れ型。仲間と連携して行動",-40}");
        Console.WriteLine($"{"Boss",-12} {"ボス。特殊な行動パターン",-40}");

        // AI状態遷移
        Console.WriteLine("\n【AI状態遷移】");
        Console.WriteLine("Idle（待機）→ Patrol（巡回）→ Alert（警戒）→ Combat（戦闘）");
        Console.WriteLine("         ↓                          ↓");
        Console.WriteLine("                              Flee（逃走）");

        // 階層別敵出現
        Console.WriteLine("\n【階層別敵出現テーブル】");
        var depths = new[] { 1, 5, 10, 15, 25 };
        foreach (var depth in depths)
        {
            var depthEnemies = EnemyDefinitions.GetEnemiesForDepth(depth);
            var names = string.Join(", ", depthEnemies.Select(e => e.Name));
            Console.WriteLine($"  階層 {depth,2}: {names}");
        }
    }

    static void PrintEnemyInfo(Enemy enemy)
    {
        var def = GetDefinitionFor(enemy.EnemyTypeId);
        Console.WriteLine($"  [{enemy.Name}] HP:{enemy.CurrentHp}/{enemy.MaxHp} " +
                         $"EXP:{enemy.ExperienceReward} " +
                         $"視界:{enemy.SightRange} " +
                         $"タイプ:{def?.EnemyType} " +
                         $"ランク:{def?.Rank}");
    }

    static EnemyDefinition? GetDefinitionFor(string typeId) => typeId switch
    {
        "slime" => EnemyDefinitions.Slime,
        "goblin" => EnemyDefinitions.Goblin,
        "skeleton" => EnemyDefinitions.Skeleton,
        "orc" => EnemyDefinitions.Orc,
        "giant_spider" => EnemyDefinitions.GiantSpider,
        "dark_elf" => EnemyDefinitions.DarkElf,
        "troll" => EnemyDefinitions.Troll,
        "draugr" => EnemyDefinitions.Draugr,
        _ => null
    };

    static void TestSpell(SpellParser parser, Dictionary<string, int> mastery, string incantation, string description)
    {
        Console.WriteLine($"【{description}】");
        Console.WriteLine($"詠唱: {incantation}");

        var result = parser.Parse(incantation, mastery);

        if (result.IsSuccess)
        {
            var formatted = parser.FormatIncantation(result.Words);
            Console.WriteLine($"ガルドル: {formatted}");
            Console.WriteLine($"効果: {result.GetDescription()}");
            Console.WriteLine($"消費MP: {result.MpCost}");
            Console.WriteLine($"詠唱ターン: {result.TurnCost}ターン（{result.TurnCost}秒）");
            Console.WriteLine($"威力倍率: {result.PowerMultiplier:F1}倍");
            Console.WriteLine($"成功率: {result.SuccessRate:P0}");
        }
        else
        {
            Console.WriteLine($"エラー: {result.ErrorMessage}");
        }

        Console.WriteLine();
    }

    static void PrintPlayerStatus(Player player)
    {
        Console.WriteLine($"名前: {player.Name}");
        Console.WriteLine($"レベル: {player.Level}");
        Console.WriteLine($"HP: {player.CurrentHp}/{player.MaxHp}");
        Console.WriteLine($"MP: {player.CurrentMp}/{player.MaxMp}");
        Console.WriteLine($"SP: {player.CurrentSp}/{player.MaxSp}");
        Console.WriteLine($"正気度: {player.Sanity}/{GameConstants.MaxSanity} ({player.SanityStage})");
        Console.WriteLine($"満腹度: {player.Hunger}/{GameConstants.MaxHunger} ({player.HungerStage})");
        Console.WriteLine($"救済残り: {player.RescueCountRemaining}回");
        Console.WriteLine($"ステータス: {player.EffectiveStats}");
    }
}

/// <summary>
/// ゲームメインクラス
/// </summary>
public class Game
{
    private readonly IServiceProvider _services;
    private GameState? _gameState;
    private TurnManager? _turnManager;

    public Game(IServiceProvider services)
    {
        _services = services;
    }

    public void Initialize()
    {
        var random = _services.GetRequiredService<IRandomProvider>();
        var combatSystem = _services.GetRequiredService<ICombatSystem>();
        _turnManager = (TurnManager)_services.GetRequiredService<ITurnManager>();

        // プレイヤー作成
        var player = Player.Create("冒険者", Stats.Default);
        player.Position = new Position(5, 5);

        // マップ作成
        var map = new BasicMap(GameConstants.DefaultMapWidth, GameConstants.DefaultMapHeight);

        // ゲーム状態作成
        _gameState = new GameState(player, map, combatSystem, random);

        // ターンマネージャーに登録
        _turnManager.RegisterActor(player);

        Console.WriteLine("ゲーム初期化完了");
    }
}

// 戦闘システムデモ
partial class Program
{
    static void RunCombatSystemDemo()
    {
        Console.WriteLine("\n=== 戦闘システム デモ ===\n");

        var random = new RandomProvider();
        var combatSystem = new CombatSystem(random);
        var damageCalc = combatSystem.DamageCalculator;
        var statusSystem = combatSystem.StatusEffectSystem;
        var resourceSystem = new ResourceSystem();

        // 属性相性
        ShowElementAffinityDemo();

        // ダメージ計算
        ShowDamageCalculationDemo(damageCalc);

        // 状態異常
        ShowStatusEffectDemo(statusSystem);

        // リソース管理
        ShowResourceSystemDemo(resourceSystem);

        // インタラクティブ戦闘
        RunInteractiveCombatDemo(combatSystem);
    }

    static void ShowElementAffinityDemo()
    {
        Console.WriteLine("【属性相性表】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("攻撃 → 対象  : 倍率  (相性)");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var testCases = new (Element atk, Element def, string desc)[]
        {
            (Element.Fire, Element.Ice, "火 → 氷"),
            (Element.Fire, Element.Water, "火 → 水"),
            (Element.Water, Element.Fire, "水 → 火"),
            (Element.Ice, Element.Fire, "氷 → 火"),
            (Element.Lightning, Element.Water, "雷 → 水"),
            (Element.Lightning, Element.Earth, "雷 → 地"),
            (Element.Light, Element.Dark, "光 → 闇"),
            (Element.Dark, Element.Light, "闇 → 光"),
            (Element.Fire, Element.Fire, "火 → 火（同属性）"),
            (Element.None, Element.Fire, "無 → 火")
        };

        foreach (var (atk, def, desc) in testCases)
        {
            var multiplier = ElementSystem.GetAffinityMultiplier(atk, def);
            var affinity = ElementSystem.GetAffinityType(atk, def);
            var affinityStr = affinity switch
            {
                ElementAffinity.Advantage => "有利 ▲",
                ElementAffinity.Disadvantage => "不利 ▽",
                ElementAffinity.Nullify => "無効 ×",
                _ => "通常 ○"
            };
            Console.WriteLine($"  {desc,-20}: {multiplier:F1}倍 ({affinityStr})");
        }
    }

    static void ShowDamageCalculationDemo(DamageCalculator calc)
    {
        Console.WriteLine("\n【ダメージ計算デモ】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var physParam = new PhysicalDamageParams
        {
            WeaponAttack = 25,
            Strength = 20,
            ArmorDefense = 15,
            Vitality = 12,
            SkillMultiplier = 1.0f,
            AttackElement = Element.None,
            TargetElement = Element.None,
            CriticalRate = 0.10
        };

        Console.WriteLine("【物理攻撃】");
        Console.WriteLine($"  攻撃側: 武器攻撃力25 + STR20");
        Console.WriteLine($"  防御側: 防具防御力15 + VIT12");

        for (int i = 0; i < 5; i++)
        {
            var result = calc.CalculatePhysicalDamage(physParam);
            var critStr = result.IsCritical ? " [クリティカル!]" : "";
            Console.WriteLine($"  試行{i + 1}: {result.FinalDamage}ダメージ{critStr}");
        }

        Console.WriteLine("\n【属性相性によるダメージ変化】");
        Console.WriteLine($"  基礎（無属性→無属性）:");
        var normalResult = calc.CalculatePhysicalDamage(physParam);
        Console.WriteLine($"    ダメージ: {normalResult.FinalDamage}");

        Console.WriteLine($"  有利（火→氷）:");
        var fireVsIce = physParam with { AttackElement = Element.Fire, TargetElement = Element.Ice };
        var advantageResult = calc.CalculatePhysicalDamage(fireVsIce);
        Console.WriteLine($"    ダメージ: {advantageResult.FinalDamage} ({advantageResult.ElementAffinity})");

        Console.WriteLine($"  不利（火→水）:");
        var fireVsWater = physParam with { AttackElement = Element.Fire, TargetElement = Element.Water };
        var disadvantageResult = calc.CalculatePhysicalDamage(fireVsWater);
        Console.WriteLine($"    ダメージ: {disadvantageResult.FinalDamage} ({disadvantageResult.ElementAffinity})");

        // HP状態とペナルティ
        Console.WriteLine("\n【HP状態とペナルティ】");
        var hpLevels = new[] { 100, 75, 50, 25, 10, 0 };
        foreach (var hp in hpLevels)
        {
            var penalty = calc.GetHpStatePenalty(hp, 100);
            var atkPen = penalty.AttackPenalty > 0 ? $"攻撃力-{penalty.AttackPenalty:P0}" : "なし";
            var spdPen = penalty.SpeedPenalty > 0 ? $"移動速度-{penalty.SpeedPenalty:P0}" : "なし";
            Console.WriteLine($"  HP {hp,3}%: {penalty.State,-10} → 攻撃ペナルティ:{atkPen}, 速度ペナルティ:{spdPen}");
        }
    }

    static void ShowStatusEffectDemo(StatusEffectSystem system)
    {
        Console.WriteLine("\n【状態異常デモ】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        Console.WriteLine("【デバフ一覧】");
        var maxHp = 100;
        var debuffs = new[]
        {
            system.CreatePoison(maxHp),
            system.CreateDeadlyPoison(maxHp),
            system.CreateBleeding(),
            system.CreateBurn(),
            system.CreateFreeze(),
            system.CreateParalysis(),
            system.CreateBlind(),
            system.CreateSilence(),
            system.CreateConfusion(),
            system.CreateFear(),
            system.CreateSleep(),
            system.CreateCurse(),
            system.CreateWeakness()
        };

        foreach (var effect in debuffs)
        {
            var details = new List<string>();
            if (effect.DamagePerTick > 0) details.Add($"毎ターン{effect.DamagePerTick}ダメージ");
            if (effect.AttackMultiplier != 1.0f) details.Add($"攻撃力×{effect.AttackMultiplier:F2}");
            if (effect.DefenseMultiplier != 1.0f) details.Add($"防御力×{effect.DefenseMultiplier:F2}");
            if (effect.HitRateModifier != 0) details.Add($"命中率{effect.HitRateModifier:+0.0%;-0.0%}");
            if (effect.AllStatsMultiplier != 1.0f) details.Add($"全ステ×{effect.AllStatsMultiplier:F2}");
            if (effect.TurnCostModifier == float.MaxValue) details.Add("行動不能");

            var durationStr = effect.Duration == int.MaxValue ? "永続" : $"{effect.Duration}ターン";
            var detailStr = details.Count > 0 ? string.Join(", ", details) : "特殊効果";
            Console.WriteLine($"  {effect.Name,-12} [{durationStr,-10}] - {detailStr}");
        }

        Console.WriteLine("\n【バフ一覧】");
        var buffs = new[]
        {
            system.CreateHaste(),
            system.CreateStrengthBuff(),
            system.CreateProtection(),
            system.CreateRegeneration()
        };

        foreach (var effect in buffs)
        {
            var details = new List<string>();
            if (effect.DamagePerTick < 0) details.Add($"毎ターン{-effect.DamagePerTick}回復");
            if (effect.AttackMultiplier > 1.0f) details.Add($"攻撃力×{effect.AttackMultiplier:F2}");
            if (effect.DefenseMultiplier > 1.0f) details.Add($"防御力×{effect.DefenseMultiplier:F2}");
            if (effect.TurnCostModifier < 1.0f) details.Add($"行動速度×{1 / effect.TurnCostModifier:F2}");

            var detailStr = details.Count > 0 ? string.Join(", ", details) : "特殊効果";
            Console.WriteLine($"  {effect.Name,-12} [{effect.Duration}ターン] - {detailStr}");
        }

        // 混乱シミュレーション
        Console.WriteLine("\n【混乱時行動シミュレーション（10回）】");
        var confusionCounts = new Dictionary<ConfusedAction, int>();
        for (int i = 0; i < 10; i++)
        {
            var action = system.GetConfusedAction();
            confusionCounts.TryGetValue(action, out int count);
            confusionCounts[action] = count + 1;
        }

        foreach (var (action, count) in confusionCounts.OrderByDescending(kv => kv.Value))
        {
            var actionName = action switch
            {
                ConfusedAction.AttackAlly => "味方を攻撃",
                ConfusedAction.AttackSelf => "自分を攻撃",
                ConfusedAction.MoveRandom => "ランダム移動",
                ConfusedAction.DoNothing => "何もしない",
                ConfusedAction.ActNormally => "正常行動",
                _ => "不明"
            };
            Console.WriteLine($"  {actionName}: {count}回");
        }
    }

    static void ShowResourceSystemDemo(ResourceSystem system)
    {
        Console.WriteLine("\n【リソース管理デモ】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        // クラス別HP/MP成長
        Console.WriteLine("【クラス別HP/MP成長（レベル10時）】");
        var classes = new[] {
            CharacterClass.Warrior, CharacterClass.Berserker, CharacterClass.Monk,
            CharacterClass.Cleric, CharacterClass.Mage, CharacterClass.Necromancer
        };

        foreach (var charClass in classes)
        {
            var hpParam = new HpCalculationParams { Vitality = 15, Level = 10, RaceBonus = 0, CharacterClass = charClass };
            var mpParam = new MpCalculationParams { Mind = 15, Intelligence = 15, Level = 10, RaceBonus = 0, CharacterClass = charClass };

            var hp = system.CalculateMaxHp(hpParam);
            var mp = system.CalculateMaxMp(mpParam);
            Console.WriteLine($"  {charClass,-12}: HP {hp,4}  MP {mp,4}");
        }

        // スタミナ消費
        Console.WriteLine("\n【スタミナ消費（最大100）】");
        var actions = new[]
        {
            (StaminaAction.NormalMove, "通常移動"),
            (StaminaAction.Dash, "ダッシュ/マス"),
            (StaminaAction.NormalAttack, "通常攻撃"),
            (StaminaAction.PhysicalSkillLight, "軽スキル"),
            (StaminaAction.PhysicalSkillMedium, "中スキル"),
            (StaminaAction.PhysicalSkillHeavy, "重スキル"),
            (StaminaAction.Defend, "防御"),
            (StaminaAction.Dodge, "回避")
        };

        foreach (var (action, name) in actions)
        {
            var normalCost = system.GetStaminaCost(action, false);
            var heavyCost = system.GetStaminaCost(action, true);
            Console.WriteLine($"  {name,-14}: {normalCost,2} SP (重装時: {heavyCost,2} SP)");
        }

        // 満腹度状態
        Console.WriteLine("\n【満腹度状態と効果】");
        var hungerLevels = new[] { 90, 60, 30, 15, 5, 0 };
        foreach (var hunger in hungerLevels)
        {
            var state = system.GetHungerState(hunger);
            var effect = system.GetHungerEffect(hunger);
            var recoveryMod = effect.StaminaRecoveryModifier == 0 ? "回復停止" : $"回復×{effect.StaminaRecoveryModifier:F1}";
            var damageInfo = effect.DamagePerTurn > 0 ? (effect.DamagePerTurn >= 999 ? " [死亡]" : $" [毎ターン{effect.DamagePerTurn}ダメージ]") : "";
            Console.WriteLine($"  満腹度{hunger,3}: {state,-12} → {recoveryMod}{damageInfo}");
        }

        // 経験値テーブル
        Console.WriteLine("\n【経験値テーブル（必要経験値）】");
        var levels = new[] { 1, 2, 5, 10, 20, 50 };
        foreach (var level in levels)
        {
            var required = system.CalculateRequiredExp(level);
            var total = system.CalculateTotalRequiredExp(level);
            Console.WriteLine($"  Lv{level,2} → Lv{level + 1}: {required,10:N0} (累計: {total,12:N0})");
        }
    }

    static void RunInteractiveCombatDemo(CombatSystem combatSystem)
    {
        Console.WriteLine("\n【インタラクティブ戦闘デモ】");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("プレイヤー vs ゴブリン のシミュレーション戦闘\n");

        // キャラクター作成
        var player = Player.Create("勇者", new Stats(18, 15, 12, 14, 10, 10, 10, 10, 12));
        var factory = new EnemyFactory();
        var enemy = factory.CreateGoblin(new Position(1, 0));

        // 初期状態表示
        Console.WriteLine("【初期状態】");
        PrintCombatStatus(player, enemy);

        // 戦闘ループ
        int turn = 1;
        while (player.IsAlive && enemy.IsAlive && turn <= 20)
        {
            Console.WriteLine($"\n--- ターン {turn} ---");

            // プレイヤー攻撃
            if (player.IsAlive && enemy.IsAlive)
            {
                var playerResult = combatSystem.ExecuteAttack(player, enemy, AttackType.Slash);
                if (playerResult.IsHit)
                {
                    var critStr = playerResult.IsCritical ? " [クリティカル!]" : "";
                    Console.WriteLine($"勇者の攻撃! → {playerResult.Damage?.Amount ?? 0}ダメージ{critStr}");
                }
                else
                {
                    Console.WriteLine("勇者の攻撃! → ミス");
                }
            }

            // 敵攻撃
            if (player.IsAlive && enemy.IsAlive)
            {
                var enemyResult = combatSystem.ExecuteAttack(enemy, player, AttackType.Slash);
                if (enemyResult.IsHit)
                {
                    var critStr = enemyResult.IsCritical ? " [クリティカル!]" : "";
                    Console.WriteLine($"ゴブリンの攻撃! → {enemyResult.Damage?.Amount ?? 0}ダメージ{critStr}");
                }
                else
                {
                    Console.WriteLine("ゴブリンの攻撃! → ミス");
                }
            }

            PrintCombatStatus(player, enemy);
            turn++;
        }

        // 結果
        Console.WriteLine("\n【戦闘結果】");
        if (player.IsAlive && !enemy.IsAlive)
        {
            Console.WriteLine("★ 勇者の勝利! ★");
        }
        else if (!player.IsAlive && enemy.IsAlive)
        {
            Console.WriteLine("☆ ゴブリンの勝利... ☆");
        }
        else if (!player.IsAlive && !enemy.IsAlive)
        {
            Console.WriteLine("相打ち...");
        }
        else
        {
            Console.WriteLine("時間切れ（20ターン経過）");
        }
    }

    static void PrintCombatStatus(Player player, Enemy enemy)
    {
        var resourceSystem = new ResourceSystem();
        var playerHpState = resourceSystem.GetHpState(player.CurrentHp, player.MaxHp);
        var enemyHpState = resourceSystem.GetHpState(enemy.CurrentHp, enemy.MaxHp);

        Console.WriteLine($"  勇者: HP {player.CurrentHp,3}/{player.MaxHp} ({playerHpState})");
        Console.WriteLine($"  ゴブリン: HP {enemy.CurrentHp,3}/{enemy.MaxHp} ({enemyHpState})");
    }
}
