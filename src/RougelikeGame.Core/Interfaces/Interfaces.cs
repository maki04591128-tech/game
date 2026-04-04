using RougelikeGame.Core.Map;

namespace RougelikeGame.Core.Interfaces;

/// <summary>
/// 全てのエンティティの基底インターフェース
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}

/// <summary>
/// ターン行動可能なアクター
/// </summary>
public interface ITurnActor
{
    /// <summary>
    /// 行動優先度（敏捷性など）
    /// </summary>
    int ActionPriority { get; }

    /// <summary>
    /// 次の行動を決定する
    /// </summary>
    TurnAction DecideAction(IGameState state);

    /// <summary>
    /// 行動を実行する
    /// </summary>
    void ExecuteAction(TurnAction action, IGameState state);
}

/// <summary>
/// ダメージを受けることができるエンティティ
/// </summary>
public interface IDamageable
{
    int CurrentHp { get; }
    int MaxHp { get; }
    bool IsAlive { get; }

    void TakeDamage(Damage damage);
    void Heal(int amount);

    event EventHandler<DamageEventArgs>? OnDamaged;
    event EventHandler<DeathEventArgs>? OnDeath;
}

/// <summary>
/// インベントリを持つエンティティ
/// </summary>
public interface IInventoryHolder
{
    IInventory Inventory { get; }
    bool CanPickUp(IItem item);
    bool PickUp(IItem item);
    void Drop(IItem item);
}

/// <summary>
/// インベントリ
/// </summary>
public interface IInventory
{
    int MaxSlots { get; }
    int UsedSlots { get; }
    IReadOnlyList<IItem> Items { get; }

    bool Add(IItem item);
    bool Remove(IItem item);
    bool Contains(IItem item);
}

/// <summary>
/// アイテム
/// </summary>
public interface IItem
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    int StackSize { get; }
    int CurrentStack { get; }
    bool IsStackable { get; }
}

/// <summary>
/// ゲーム状態（依存性逆転のための抽象）
/// </summary>
public interface IGameState
{
    IPlayer Player { get; }
    IMap CurrentMap { get; }
    ICombatSystem CombatSystem { get; }
    IRandomProvider Random { get; }
    CombatState CombatState { get; }
    long CurrentTurn { get; }

    float GetMovementModifier(IEntity entity);
}

/// <summary>
/// プレイヤー
/// </summary>
public interface IPlayer : ITurnActor, IDamageable, IInventoryHolder
{
    string Name { get; }
    int Level { get; }
    int Sanity { get; }
    int Hunger { get; }
}

/// <summary>
/// マップ
/// </summary>
public interface IMap
{
    int Width { get; }
    int Height { get; }

    bool InBounds(Position position);
    bool IsWalkable(Position position);
    bool BlocksSight(Position position);
    bool CanMoveTo(Position position);
    bool HasLineOfSight(Position from, Position to);
    float GetEnvironmentModifier(Position position, TurnActionType actionType);
    Tile GetTile(Position position);
    void SetTile(Position position, Tile tile);
}

/// <summary>
/// 戦闘システム
/// </summary>
public interface ICombatSystem
{
    CombatResult ExecuteAttack(IDamageable attacker, IDamageable target, AttackType attackType);
    bool CheckHit(IDamageable attacker, IDamageable target, AttackType attackType);
    Damage CalculateDamage(IDamageable attacker, IDamageable target, AttackType attackType, bool isCritical);
}

/// <summary>
/// 乱数プロバイダー
/// </summary>
public interface IRandomProvider
{
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
}

/// <summary>
/// セーブマネージャー
/// </summary>
public interface ISaveManager
{
    Task SaveGameAsync(IGameState state, string slotName);
    Task<IGameState?> LoadGameAsync(string slotName);
    Task DeleteSaveAsync(string slotName);
    Task<IReadOnlyList<SaveInfo>> GetSaveInfosAsync();
}

/// <summary>
/// 死に戻りマネージャー
/// </summary>
public interface IRebirthManager
{
    Task<IGameState> ExecuteRebirthAsync(IPlayer player, DeathCause cause);
    int CalculateSanityLoss(DeathCause cause);
}

/// <summary>
/// マップジェネレーター
/// </summary>
public interface IMapGenerator
{
    IMap Generate(DungeonGenerationParameters parameters);
}

/// <summary>
/// ターン管理
/// </summary>
public interface ITurnManager
{
    bool IsPlayerTurn { get; }
    ITurnActor? CurrentActor { get; }
    long CurrentTurn { get; }
    CombatState CombatState { get; set; }

    void ProcessTurn(IGameState state);
    void RegisterActor(ITurnActor actor);
    void UnregisterActor(ITurnActor actor);

    TimeSpan GetGameTime();
    int GetGameDay();
    int GetGameHour();
}

/// <summary>
/// レンダラー
/// </summary>
public interface IRenderer
{
    void Render(IGameState state);
    void RenderMessage(string message);
    void Clear();
}

/// <summary>
/// 入力ハンドラー
/// </summary>
public interface IInputHandler
{
    Task<GameCommand> GetInputAsync(CancellationToken cancellationToken);
}
