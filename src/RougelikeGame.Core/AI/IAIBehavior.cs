using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.AI;

/// <summary>
/// AIビヘイビアのインターフェース
/// </summary>
public interface IAIBehavior
{
    /// <summary>
    /// ビヘイビア名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 行動を決定
    /// </summary>
    TurnAction DecideAction(Enemy enemy, IGameState state);

    /// <summary>
    /// このビヘイビアが現在の状況で有効か
    /// </summary>
    bool IsApplicable(Enemy enemy, IGameState state);

    /// <summary>
    /// 優先度（高いほど優先）
    /// </summary>
    int Priority { get; }
}

/// <summary>
/// 複合ビヘイビア（複数のビヘイビアを組み合わせる）
/// </summary>
public class CompositeBehavior : IAIBehavior
{
    private readonly List<IAIBehavior> _behaviors = new();

    public string Name => "Composite";
    public int Priority => 0;

    public void AddBehavior(IAIBehavior behavior)
    {
        _behaviors.Add(behavior);
        _behaviors.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public void RemoveBehavior(IAIBehavior behavior)
    {
        _behaviors.Remove(behavior);
    }

    public TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        foreach (var behavior in _behaviors)
        {
            if (behavior.IsApplicable(enemy, state))
            {
                return behavior.DecideAction(enemy, state);
            }
        }

        return TurnAction.Wait;
    }

    public bool IsApplicable(Enemy enemy, IGameState state) => true;
}

/// <summary>
/// AIビヘイビアの基底クラス
/// </summary>
public abstract class AIBehaviorBase : IAIBehavior
{
    public abstract string Name { get; }
    public virtual int Priority => 0;

    public abstract TurnAction DecideAction(Enemy enemy, IGameState state);
    public abstract bool IsApplicable(Enemy enemy, IGameState state);

    /// <summary>
    /// 目標位置への移動アクションを生成
    /// </summary>
    protected TurnAction MoveTowards(Enemy enemy, Position target, IGameState state)
    {
        // IM-1: CurrentMapがnullの場合は待機
        if (state.CurrentMap == null)
            return TurnAction.Wait;

        var direction = enemy.Position.GetDirectionTo(target);
        var nextPos = enemy.Position.Move(direction);

        if (state.CurrentMap.CanMoveTo(nextPos))
        {
            return TurnAction.Move(direction);
        }

        // 迂回を試みる
        foreach (var neighbor in enemy.Position.GetNeighbors(includeDiagonals: true))
        {
            if (state.CurrentMap.CanMoveTo(neighbor) &&
                neighbor.ChebyshevDistanceTo(target) < enemy.Position.ChebyshevDistanceTo(target))
            {
                var altDirection = enemy.Position.GetDirectionTo(neighbor);
                return TurnAction.Move(altDirection);
            }
        }

        return TurnAction.Wait;
    }

    /// <summary>
    /// 脅威から離れる移動アクションを生成
    /// </summary>
    protected TurnAction MoveAwayFrom(Enemy enemy, Position threat, IGameState state)
    {
        // IM-1: CurrentMapがnullの場合は待機
        if (state.CurrentMap == null)
            return TurnAction.Wait;

        var awayDirection = threat.GetDirectionTo(enemy.Position);
        var nextPos = enemy.Position.Move(awayDirection);

        if (state.CurrentMap.CanMoveTo(nextPos))
        {
            return TurnAction.Move(awayDirection);
        }

        // 逃げられない場合はサイドに移動を試みる
        var sideDir = awayDirection.RotateClockwise();
        var sidePos = enemy.Position.Move(sideDir);
        if (state.CurrentMap.CanMoveTo(sidePos))
        {
            return TurnAction.Move(sideDir);
        }

        return TurnAction.Wait;
    }

    /// <summary>
    /// ランダムな方向に移動
    /// </summary>
    protected TurnAction MoveRandom(Enemy enemy, IGameState state, IRandomProvider random)
    {
        var directions = new[] {
            Direction.North, Direction.South, Direction.East, Direction.West,
            Direction.NorthEast, Direction.NorthWest, Direction.SouthEast, Direction.SouthWest
        };

        // シャッフル
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }

        foreach (var dir in directions)
        {
            var nextPos = enemy.Position.Move(dir);
            if (state.CurrentMap.CanMoveTo(nextPos))
            {
                return TurnAction.Move(dir);
            }
        }

        return TurnAction.Wait;
    }
}

/// <summary>
/// 敵の種別タイプ
/// </summary>
public enum EnemyType
{
    /// <summary>
    /// 通常の敵（バランス型）
    /// </summary>
    Normal,

    /// <summary>
    /// 攻撃的な敵（積極的に追跡）
    /// </summary>
    Aggressive,

    /// <summary>
    /// 防御的な敵（縄張りを守る）
    /// </summary>
    Defensive,

    /// <summary>
    /// 臆病な敵（早めに逃走）
    /// </summary>
    Coward,

    /// <summary>
    /// 待ち伏せ型（動かずに待機）
    /// </summary>
    Ambusher,

    /// <summary>
    /// 群れ型（仲間と連携）
    /// </summary>
    Pack,

    /// <summary>
    /// ボス（特殊行動パターン）
    /// </summary>
    Boss,

    /// <summary>
    /// AZ-2: 召喚者型（仲間を召喚）
    /// </summary>
    Summoner
}

/// <summary>
/// 敵のランク
/// </summary>
public enum EnemyRank
{
    /// <summary>
    /// 一般
    /// </summary>
    Common,

    /// <summary>
    /// エリート
    /// </summary>
    Elite,

    /// <summary>
    /// レア
    /// </summary>
    Rare,

    /// <summary>
    /// ボス
    /// </summary>
    Boss,

    /// <summary>
    /// 隠しボス
    /// </summary>
    HiddenBoss
}
