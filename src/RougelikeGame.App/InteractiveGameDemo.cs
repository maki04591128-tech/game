using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Engine;
using RougelikeGame.Engine.Combat;

namespace RougelikeGame.App;

/// <summary>
/// インタラクティブなゲームデモ
/// 実際にキー操作でゲームをプレイできる
/// </summary>
public class InteractiveGameDemo
{
    private readonly IRandomProvider _random;
    private readonly CombatSystem _combatSystem;
    private readonly ResourceSystem _resourceSystem;
    private readonly EnemyFactory _enemyFactory;
    private readonly ItemFactory _itemFactory;

    private Player _player = null!;
    private DungeonMap _map = null!;
    private List<Enemy> _enemies = new();
    private List<(Item Item, Position Position)> _groundItems = new();
    private int _currentFloor = 1;
    private int _turnCount = 0;
    private bool _isRunning = true;
    private string _lastMessage = "";
    private List<string> _messageLog = new();

    public InteractiveGameDemo(IRandomProvider random, CombatSystem combatSystem)
    {
        _random = random;
        _combatSystem = combatSystem;
        _resourceSystem = new ResourceSystem();
        _enemyFactory = new EnemyFactory();
        _itemFactory = new ItemFactory();
    }

    public void Run()
    {
        Initialize();
        GameLoop();
        ShowGameOver();
    }

    private void Initialize()
    {
        // プレイヤー作成
        _player = Player.Create("冒険者", new Stats(14, 12, 12, 10, 10, 10, 10, 10, 10));

        // 初期装備
        var sword = ItemFactory.CreateIronSword();
        var armor = ItemFactory.CreateLeatherArmor();
        ((Inventory)_player.Inventory).Add(sword);
        ((Inventory)_player.Inventory).Add(armor);
        _player.Equipment.Equip(sword, _player);
        _player.Equipment.Equip(armor, _player);

        // 初期アイテム
        ((Inventory)_player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)_player.Inventory).Add(ItemFactory.CreateHealingPotion());
        ((Inventory)_player.Inventory).Add(ItemFactory.CreateBread());

        // マップ生成
        GenerateFloor();

        AddMessage($"ダンジョン第{_currentFloor}層に入った！");
        AddMessage("操作: WASD/矢印で移動, I:インベントリ, G:拾う, >:階段, Q:終了");
    }

    private void GenerateFloor()
    {
        var parameters = new DungeonGenerationParameters
        {
            Width = 50,
            Height = 20,
            Depth = _currentFloor,
            RoomCount = 5 + _currentFloor,
            TrapDensity = 0.005f * _currentFloor
        };

        var generator = new DungeonGenerator();
        _map = (DungeonMap)generator.Generate(parameters);

        // プレイヤー配置
        var startPos = _map.StairsUpPosition ?? _map.EntrancePosition ?? new Position(5, 5);
        _player.Position = startPos;

        // 敵を配置
        _enemies.Clear();
        SpawnEnemies();

        // アイテムを配置
        _groundItems.Clear();
        SpawnItems();

        // 視界計算
        _map.ComputeFov(_player.Position, 8);
    }

    private void SpawnEnemies()
    {
        var definitions = EnemyDefinitions.GetEnemiesForDepth(_currentFloor);
        int enemyCount = 3 + _currentFloor;

        for (int i = 0; i < enemyCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue && pos.Value.DistanceTo(_player.Position) > 5)
            {
                var def = definitions[_random.Next(definitions.Count)];
                var enemy = _enemyFactory.CreateEnemy(def, pos.Value);
                _enemies.Add(enemy);
            }
        }
    }

    private void SpawnItems()
    {
        int itemCount = 2 + _random.Next(3);
        for (int i = 0; i < itemCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue)
            {
                var item = _itemFactory.GenerateRandomItem(_currentFloor);
                _groundItems.Add((item, pos.Value));
            }
        }
    }

    private Position? GetRandomFloorPosition()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = _random.Next(_map.Width);
            int y = _random.Next(_map.Height);
            var pos = new Position(x, y);
            var tile = _map.GetTile(pos);
            if (!tile.BlocksMovement && !IsOccupied(pos))
            {
                return pos;
            }
        }
        return null;
    }

    private bool IsOccupied(Position pos)
    {
        if (_player.Position == pos) return true;
        return _enemies.Any(e => e.Position == pos && e.IsAlive);
    }

    private void GameLoop()
    {
        while (_isRunning && _player.IsAlive)
        {
            Render();
            ProcessInput();

            if (_player.IsAlive)
            {
                ProcessEnemyTurns();
                ProcessTurnEffects();
            }
        }
    }

    private void Render()
    {
        Console.Clear();

        // ステータスバー
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        Console.WriteLine($" 第{_currentFloor}層 | ターン:{_turnCount} | " +
                         $"HP:{_player.CurrentHp}/{_player.MaxHp} " +
                         $"MP:{_player.CurrentMp}/{_player.MaxMp} " +
                         $"SP:{_player.CurrentSp}/{_player.MaxSp} | " +
                         $"満腹度:{_player.Hunger}");
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");

        // マップ描画
        _map.ComputeFov(_player.Position, 8);
        RenderMap();

        // メッセージログ
        Console.WriteLine($"───────────────────────────────────────────────────────────────");
        var recentMessages = _messageLog.TakeLast(3);
        foreach (var msg in recentMessages)
        {
            Console.WriteLine($" {msg}");
        }
        Console.WriteLine($"───────────────────────────────────────────────────────────────");

        // 操作ヘルプ
        Console.WriteLine(" [W/↑]上 [S/↓]下 [A/←]左 [D/→]右 | [I]持物 [G]拾う [>]階段 [Q]終了");
    }

    private void RenderMap()
    {
        for (int y = 0; y < _map.Height; y++)
        {
            for (int x = 0; x < _map.Width; x++)
            {
                var pos = new Position(x, y);
                var tile = _map.GetTile(pos);

                char display;
                ConsoleColor color = ConsoleColor.Gray;

                // プレイヤー
                if (_player.Position == pos)
                {
                    display = '@';
                    color = ConsoleColor.Yellow;
                }
                // 敵
                else if (_enemies.FirstOrDefault(e => e.Position == pos && e.IsAlive) is Enemy enemy)
                {
                    if (tile.IsVisible)
                    {
                        display = enemy.Name[0];
                        color = ConsoleColor.Red;
                    }
                    else if (tile.IsExplored)
                    {
                        display = tile.DisplayChar;
                        color = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        display = ' ';
                    }
                }
                // 地面のアイテム
                else if (tile.IsVisible && _groundItems.Any(i => i.Position == pos))
                {
                    display = '!';
                    color = ConsoleColor.Cyan;
                }
                // 通常タイル
                else if (tile.IsVisible)
                {
                    display = tile.DisplayChar;
                    color = tile.Type switch
                    {
                        TileType.StairsDown => ConsoleColor.Green,
                        TileType.StairsUp => ConsoleColor.Blue,
                        TileType.DoorClosed or TileType.DoorOpen => ConsoleColor.DarkYellow,
                        TileType.Water => ConsoleColor.Cyan,
                        TileType.TrapVisible => ConsoleColor.Magenta,
                        _ => ConsoleColor.Gray
                    };
                }
                else if (tile.IsExplored)
                {
                    display = tile.DisplayChar;
                    color = ConsoleColor.DarkGray;
                }
                else
                {
                    display = ' ';
                }

                Console.ForegroundColor = color;
                Console.Write(display);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    private void ProcessInput()
    {
        var key = Console.ReadKey(true);
        Position newPos = _player.Position;

        switch (key.Key)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                newPos = new Position(_player.Position.X, _player.Position.Y - 1);
                break;
            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                newPos = new Position(_player.Position.X, _player.Position.Y + 1);
                break;
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                newPos = new Position(_player.Position.X - 1, _player.Position.Y);
                break;
            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                newPos = new Position(_player.Position.X + 1, _player.Position.Y);
                break;
            case ConsoleKey.I:
                ShowInventory();
                return; // ターン消費なし
            case ConsoleKey.G:
                TryPickupItem();
                break;
            case ConsoleKey.OemPeriod when key.Modifiers == ConsoleModifiers.Shift:
            case ConsoleKey.D0 when key.Modifiers == ConsoleModifiers.Shift:
                TryDescendStairs();
                break;
            case ConsoleKey.Q:
                _isRunning = false;
                return;
            default:
                return; // ターン消費なし
        }

        // 移動処理
        if (newPos != _player.Position)
        {
            TryMove(newPos);
        }

        _turnCount++;
    }

    private void TryMove(Position newPos)
    {
        // マップ範囲チェック
        if (newPos.X < 0 || newPos.X >= _map.Width || newPos.Y < 0 || newPos.Y >= _map.Height)
        {
            AddMessage("そこには行けない");
            return;
        }

        var tile = _map.GetTile(newPos);

        // 敵がいる場合は攻撃
        var enemy = _enemies.FirstOrDefault(e => e.Position == newPos && e.IsAlive);
        if (enemy != null)
        {
            Attack(enemy);
            return;
        }

        // ドアを開ける
        if (tile.Type == TileType.DoorClosed)
        {
            _map.SetTile(newPos.X, newPos.Y, TileType.DoorOpen);
            AddMessage("ドアを開けた");
            return;
        }

        // 移動可能チェック
        if (tile.BlocksMovement)
        {
            AddMessage("そこには進めない");
            return;
        }

        // 罠チェック
        if (tile.Type == TileType.TrapHidden)
        {
            _map.SetTile(newPos.X, newPos.Y, TileType.TrapVisible);
            int trapDamage = _random.Next(5, 15);
            _player.TakeDamage(Damage.Pure(trapDamage));
            AddMessage($"罠を踏んだ！{trapDamage}ダメージ！");
        }

        // 移動実行
        _player.Position = newPos;

        // 階段メッセージ
        if (tile.Type == TileType.StairsDown)
        {
            AddMessage("下り階段がある。[>]キーで降りる");
        }
    }

    private void Attack(Enemy enemy)
    {
        var result = _combatSystem.ExecuteAttack(_player, enemy, AttackType.Slash);

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? "クリティカル！" : "";
            AddMessage($"{enemy.Name}に{result.Damage?.Amount ?? 0}ダメージ！{critStr}");

            if (!enemy.IsAlive)
            {
                AddMessage($"{enemy.Name}を倒した！経験値+{enemy.ExperienceReward}");
                _player.GainExperience(enemy.ExperienceReward);
            }
        }
        else
        {
            AddMessage($"{enemy.Name}への攻撃は外れた");
        }
    }

    private void ProcessEnemyTurns()
    {
        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            // プレイヤーとの距離
            int distance = enemy.Position.DistanceTo(_player.Position);

            // 視界内なら追跡
            if (distance <= enemy.SightRange)
            {
                if (distance == 1)
                {
                    // 隣接していれば攻撃
                    EnemyAttack(enemy);
                }
                else
                {
                    // 追跡
                    MoveEnemyTowardsPlayer(enemy);
                }
            }
            else
            {
                // ランダム移動
                RandomMoveEnemy(enemy);
            }
        }
    }

    private void EnemyAttack(Enemy enemy)
    {
        var result = _combatSystem.ExecuteAttack(enemy, _player, AttackType.Slash);

        if (result.IsHit)
        {
            var critStr = result.IsCritical ? "クリティカル！" : "";
            AddMessage($"{enemy.Name}の攻撃！{result.Damage?.Amount ?? 0}ダメージ！{critStr}");
        }
        else
        {
            AddMessage($"{enemy.Name}の攻撃は外れた");
        }
    }

    private void MoveEnemyTowardsPlayer(Enemy enemy)
    {
        int dx = Math.Sign(_player.Position.X - enemy.Position.X);
        int dy = Math.Sign(_player.Position.Y - enemy.Position.Y);

        // 優先的にX方向かY方向に移動
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
        if (_random.Next(100) < 30) // 30%の確率で移動
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
        if (pos.X < 0 || pos.X >= _map.Width || pos.Y < 0 || pos.Y >= _map.Height)
            return false;

        var tile = _map.GetTile(pos);
        if (tile.BlocksMovement) return false;
        if (_player.Position == pos) return false;
        if (_enemies.Any(e => e.Position == pos && e.IsAlive)) return false;

        return true;
    }

    private void ProcessTurnEffects()
    {
        // 満腹度減少（10ターンごと）
        if (_turnCount % 10 == 0 && _player.Hunger > 0)
        {
            // Player.Hungerはget-onlyなので直接減らせない
            // 代わりにメッセージで警告
            if (_player.Hunger <= 30)
            {
                AddMessage("お腹が減ってきた...");
            }
        }

        // SP回復
        if (_turnCount % 5 == 0)
        {
            // SP回復ロジック（Playerクラスの実装による）
        }
    }

    private void ShowInventory()
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine(" 【所持品】");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");

        var inventory = (Inventory)_player.Inventory;
        var items = inventory.Items.ToList();

        if (items.Count == 0)
        {
            Console.WriteLine(" 何も持っていない");
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var equipped = "";
                if (item is Weapon w && _player.Equipment.MainHand == w)
                    equipped = " [装備中]";
                else if (item is Armor a && _player.Equipment[Core.Items.EquipmentSlot.Body] == a)
                    equipped = " [装備中]";

                Console.WriteLine($" {i + 1}. {item.GetDisplayName()}{equipped}");
            }
        }

        Console.WriteLine("───────────────────────────────────────────────────────────────");
        Console.WriteLine(" [数字]使用/装備 [0]戻る");

        var key = Console.ReadKey(true);
        if (char.IsDigit(key.KeyChar))
        {
            int index = key.KeyChar - '1';
            if (index >= 0 && index < items.Count)
            {
                UseOrEquipItem(items[index]);
            }
        }
    }

    private void UseOrEquipItem(Item item)
    {
        var inventory = (Inventory)_player.Inventory;

        if (item is ConsumableItem consumable)
        {
            var result = inventory.UseItem(consumable, _player);
            if (result != null)
            {
                AddMessage(result.Message);
                _turnCount++;
            }
        }
        else if (item is EquipmentItem equipItem)
        {
            _player.Equipment.Equip(equipItem, _player);
            AddMessage($"{item.GetDisplayName()}を装備した");
        }
    }

    private void TryPickupItem()
    {
        var itemOnGround = _groundItems.FirstOrDefault(i => i.Position == _player.Position);
        if (itemOnGround.Item != null)
        {
            _groundItems.Remove(itemOnGround);
            ((Inventory)_player.Inventory).Add(itemOnGround.Item);
            AddMessage($"{itemOnGround.Item.GetDisplayName()}を拾った");
        }
        else
        {
            AddMessage("ここには何もない");
        }
    }

    private void TryDescendStairs()
    {
        var tile = _map.GetTile(_player.Position);
        if (tile.Type == TileType.StairsDown)
        {
            _currentFloor++;
            GenerateFloor();
            AddMessage($"第{_currentFloor}層に降りた");
        }
        else
        {
            AddMessage("ここに階段はない");
        }
    }

    private void AddMessage(string message)
    {
        _lastMessage = message;
        _messageLog.Add($"[{_turnCount}] {message}");
        if (_messageLog.Count > 100)
        {
            _messageLog.RemoveAt(0);
        }
    }

    private void ShowGameOver()
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");

        if (_player.IsAlive)
        {
            Console.WriteLine(" 【冒険終了】");
            Console.WriteLine($" 到達階層: 第{_currentFloor}層");
            Console.WriteLine($" 経過ターン: {_turnCount}");
            Console.WriteLine($" 最終HP: {_player.CurrentHp}/{_player.MaxHp}");
        }
        else
        {
            Console.WriteLine(" 【ゲームオーバー】");
            Console.WriteLine($" あなたは第{_currentFloor}層で力尽きた...");
            Console.WriteLine($" 経過ターン: {_turnCount}");
        }

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("\n何かキーを押して終了...");
        Console.ReadKey();
    }
}
