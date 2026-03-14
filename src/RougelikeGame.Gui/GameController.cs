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

    public Player Player { get; private set; } = null!;
    public DungeonMap Map { get; private set; } = null!;
    public List<Enemy> Enemies { get; } = new();
    public List<(Item Item, Position Position)> GroundItems { get; } = new();
    public int CurrentFloor { get; private set; } = 1;
    public int TurnCount { get; private set; } = 0;
    public GameTime GameTime { get; } = new();
    public bool IsGameOver { get; private set; } = false;
    public bool IsRunning { get; private set; } = true;
    public bool IsAutoExploring => _autoExploring;

    /// <summary>ターン制限が延長されているか</summary>
    public bool IsTurnLimitExtended => _turnLimitExtended;

    /// <summary>ターン制限が撤廃されているか</summary>
    public bool IsTurnLimitRemoved => _turnLimitRemoved;

    /// <summary>現在のターン制限上限</summary>
    public long CurrentTurnLimit =>
        _turnLimitRemoved ? long.MaxValue :
        _turnLimitExtended ? TimeConstants.TurnLimitWithExtension :
        TimeConstants.TurnLimitYear;

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
        // プレイヤー作成
        Player = Player.Create("冒険者", new Stats(14, 12, 12, 10, 10, 10, 10, 10, 10));

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

        // マップ生成
        GenerateFloor();

        AddMessage($"ダンジョン第{CurrentFloor}層に入った！");
        AddMessage("WASD/矢印で移動、敵に体当たりで攻撃");
    }

    /// <summary>
    /// プレイヤーイベントを購読する（Initialize / LoadSaveData で共用）
    /// </summary>
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

        for (int i = 0; i < enemyCount; i++)
        {
            var pos = GetRandomFloorPosition();
            if (pos.HasValue && pos.Value.DistanceTo(Player.Position) > 6)
            {
                var def = definitions[_random.Next(definitions.Count)];
                var enemy = _enemyFactory.CreateEnemy(def, pos.Value);
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

        // 罠チェック
        if (tile.Type == TileType.TrapHidden)
        {
            Map.SetTile(newPos.X, newPos.Y, TileType.TrapVisible);
            int trapDamage = _random.Next(5, 15);
            Player.TakeDamage(Damage.Pure(trapDamage));
            _lastDamageCause = DeathCause.Trap;
            AddMessage($"罠を踏んだ！{trapDamage}ダメージ！");
        }

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
                AddMessage($"{enemy.Name}を倒した！経験値+{enemy.ExperienceReward}");
                Player.GainExperience(enemy.ExperienceReward);
                TryDropItem(enemy);
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

    private void ProcessEnemyTurns()
    {
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

    private void ProcessTurnEffects()
    {
        // 満腹度減少（HungerDecayInterval ターンごとに1減少）
        if (TurnCount > 0 && TurnCount % TimeConstants.HungerDecayInterval == 0)
        {
            Player.ModifyHunger(-1);
        }

        // 飢餓ダメージ（満腹度0の場合、毎ターンダメージ）
        if (Player.Hunger <= 0)
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
        Player.TickStatusEffects();

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
            GroundItems.Remove(itemOnGround);
            ((Inventory)Player.Inventory).Add(itemOnGround.Item);
            AddMessage($"{itemOnGround.Item.GetDisplayName()}を拾った");
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
            CurrentFloor++;
            GenerateFloor();
            AddMessage($"第{CurrentFloor}層に降りた");
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
                Player.Equipment.Equip(equipItem, Player);
                AddMessage($"{item.GetDisplayName()}を装備した");
                TurnCount += TurnCosts.EquipChange;
                GameTime.AdvanceTurn(TurnCosts.EquipChange);
                OnStateChanged?.Invoke();
            }
        }
    }

    private void HandlePlayerDeath(DeathCause cause)
    {
        _autoExploring = false;
        bool wasRescuable = Player.CanBeRescued;
        Player.HandleDeath(cause);

        if (wasRescuable)
        {
            // 救出処理：HP回復してリスポーン
            Player.Heal(Player.MaxHp / 2);
            AddMessage($"意識が戻った...（残り救出回数: {Player.RescueCountRemaining}）");

            // 上り階段付近にリスポーン
            var respawnPos = Map.StairsUpPosition ?? Map.EntrancePosition ?? Player.Position;
            Player.Position = respawnPos;
            Map.ComputeFov(Player.Position, 8);
        }
        else
        {
            IsGameOver = true;
            string causeText = cause switch
            {
                DeathCause.Combat => "戦闘で力尽きた",
                DeathCause.Starvation => "飢えにより力尽きた",
                DeathCause.Trap => "罠によって命を落とした",
                DeathCause.Poison => "毒に蝕まれ力尽きた",
                _ => "力尽きた"
            };
            AddMessage($"あなたは{causeText}...");
            OnGameOver?.Invoke();
        }
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
                FaithPoints = Player.FaithPoints
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
            save.Player.RescueCountRemaining
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

        // ゲーム状態復元
        CurrentFloor = save.CurrentFloor;
        TurnCount = save.TurnCount;
        GameTime.SetTotalTurns(save.GameTime.TotalTurns);

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
    Save,
    Load,
    Quit
}
