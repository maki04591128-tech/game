using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.AI.Behaviors;

/// <summary>
/// 待機ビヘイビア - その場で待機または周囲を見回す
/// </summary>
public class IdleBehavior : AIBehaviorBase
{
    public override string Name => "Idle";
    public override int Priority => 0;

    private readonly float _wanderChance;

    public IdleBehavior(float wanderChance = 0.1f)
    {
        _wanderChance = wanderChance;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        // 一定確率でランダム移動
        if (state.Random.NextDouble() < _wanderChance)
        {
            return MoveRandom(enemy, state, state.Random);
        }

        return TurnAction.Wait;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.CurrentAIState == AIState.Idle;
    }
}

/// <summary>
/// 巡回ビヘイビア - 設定されたルートを巡回
/// </summary>
public class PatrolBehavior : AIBehaviorBase
{
    public override string Name => "Patrol";
    public override int Priority => 10;

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.PatrolRoute.Count == 0)
        {
            return TurnAction.Wait;
        }

        var targetPos = enemy.PatrolRoute[enemy.PatrolIndex];

        // 目標地点に到達
        if (enemy.Position == targetPos)
        {
            enemy.PatrolIndex = (enemy.PatrolIndex + 1) % enemy.PatrolRoute.Count;

            // 到達時に少し待機
            if (state.Random.NextDouble() < 0.3)
            {
                return TurnAction.Wait;
            }

            targetPos = enemy.PatrolRoute[enemy.PatrolIndex];
        }

        return MoveTowards(enemy, targetPos, state);
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.CurrentAIState == AIState.Patrol && enemy.PatrolRoute.Count > 0;
    }
}

/// <summary>
/// 追跡ビヘイビア - ターゲットを追いかける
/// </summary>
public class ChaseBehavior : AIBehaviorBase
{
    public override string Name => "Chase";
    public override int Priority => 50;

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.Target == null) return TurnAction.Wait;

        int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);

        // 隣接している → 攻撃
        if (distance == 1)
        {
            return TurnAction.Attack(enemy.Target);
        }

        // 接近
        return MoveTowards(enemy, enemy.Target.Position, state);
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.CurrentAIState == AIState.Combat && enemy.Target != null;
    }
}

/// <summary>
/// 警戒ビヘイビア - 最後の既知位置を調査
/// </summary>
public class AlertBehavior : AIBehaviorBase
{
    public override string Name => "Alert";
    public override int Priority => 30;

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (!enemy.LastKnownTargetPosition.HasValue)
        {
            return TurnAction.Search;
        }

        var targetPos = enemy.LastKnownTargetPosition.Value;

        // 最後の既知位置に到達
        if (enemy.Position == targetPos)
        {
            enemy.LastKnownTargetPosition = null;
            return TurnAction.Search;
        }

        return MoveTowards(enemy, targetPos, state);
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.CurrentAIState == AIState.Alert;
    }
}

/// <summary>
/// 逃走ビヘイビア - 脅威から逃げる
/// </summary>
public class FleeBehavior : AIBehaviorBase
{
    public override string Name => "Flee";
    public override int Priority => 100;  // 最優先

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.Target == null)
        {
            // ターゲットがいなければホームへ戻る
            if (enemy.Position != enemy.HomePosition)
            {
                return MoveTowards(enemy, enemy.HomePosition, state);
            }
            return TurnAction.Wait;
        }

        // ターゲットから離れる
        var action = MoveAwayFrom(enemy, enemy.Target.Position, state);

        // 逃げられない場合は戦う
        if (action.Type == TurnActionType.Wait)
        {
            int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);
            if (distance == 1)
            {
                return TurnAction.Attack(enemy.Target);
            }
        }

        return action;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.CurrentAIState == AIState.Flee;
    }
}

/// <summary>
/// 攻撃的ビヘイビア - より積極的に追跡・攻撃
/// </summary>
public class AggressiveBehavior : AIBehaviorBase
{
    public override string Name => "Aggressive";
    public override int Priority => 60;

    private readonly int _aggroRange;

    public AggressiveBehavior(int aggroRange = 12)
    {
        _aggroRange = aggroRange;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null || !player.IsAlive) return TurnAction.Wait;

        int distance = enemy.Position.ChebyshevDistanceTo(player.Position);

        // 攻撃範囲内
        if (distance == 1)
        {
            return TurnAction.Attack(player);
        }

        // アグロ範囲内なら追跡
        if (distance <= _aggroRange && state.CurrentMap.HasLineOfSight(enemy.Position, player.Position))
        {
            enemy.Target = player;
            return MoveTowards(enemy, player.Position, state);
        }

        return TurnAction.Wait;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null) return false;

        int distance = enemy.Position.ChebyshevDistanceTo(player.Position);
        return distance <= _aggroRange;
    }
}

/// <summary>
/// 防御的ビヘイビア - 縄張りを守る
/// </summary>
public class DefensiveBehavior : AIBehaviorBase
{
    public override string Name => "Defensive";
    public override int Priority => 40;

    private readonly int _territoryRadius;

    public DefensiveBehavior(int territoryRadius = 5)
    {
        _territoryRadius = territoryRadius;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null || !player.IsAlive) return TurnAction.Wait;

        int distanceToPlayer = enemy.Position.ChebyshevDistanceTo(player.Position);
        int distanceFromHome = enemy.Position.ChebyshevDistanceTo(enemy.HomePosition);

        // プレイヤーが縄張り内に侵入
        int playerDistanceFromHome = player.Position.ChebyshevDistanceTo(enemy.HomePosition);
        if (playerDistanceFromHome <= _territoryRadius)
        {
            // 攻撃範囲内
            if (distanceToPlayer == 1)
            {
                return TurnAction.Attack(player);
            }

            // 追跡（ただし縄張りからあまり離れない）
            if (distanceFromHome < _territoryRadius + 2)
            {
                return MoveTowards(enemy, player.Position, state);
            }
        }

        // ホームに戻る
        if (distanceFromHome > 0)
        {
            return MoveTowards(enemy, enemy.HomePosition, state);
        }

        return TurnAction.Wait;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null) return false;

        int playerDistanceFromHome = player.Position.ChebyshevDistanceTo(enemy.HomePosition);
        return playerDistanceFromHome <= _territoryRadius + 3;
    }
}

/// <summary>
/// 待ち伏せビヘイビア - 動かずに待機し、近づいたら攻撃
/// </summary>
public class AmbushBehavior : AIBehaviorBase
{
    public override string Name => "Ambush";
    public override int Priority => 70;

    private readonly int _strikeRange;

    public AmbushBehavior(int strikeRange = 3)
    {
        _strikeRange = strikeRange;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null || !player.IsAlive) return TurnAction.Wait;

        int distance = enemy.Position.ChebyshevDistanceTo(player.Position);

        // 攻撃範囲内なら襲いかかる
        if (distance == 1)
        {
            return TurnAction.Attack(player);
        }

        if (distance <= _strikeRange && state.CurrentMap.HasLineOfSight(enemy.Position, player.Position))
        {
            enemy.Target = player;
            return MoveTowards(enemy, player.Position, state);
        }

        // それ以外は待機
        return TurnAction.Wait;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        var player = state.Player as Character;
        if (player == null) return false;

        int distance = enemy.Position.ChebyshevDistanceTo(player.Position);
        return distance <= _strikeRange;
    }
}
