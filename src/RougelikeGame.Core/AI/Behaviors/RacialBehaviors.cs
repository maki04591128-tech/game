using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.AI.Behaviors;

/// <summary>
/// 群れ狩りビヘイビア（Beast用） - 仲間と連携して攻撃
/// </summary>
public class PackHuntingBehavior : AIBehaviorBase
{
    public override string Name => "PackHunting";
    public override int Priority => 55;

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Beast
            && enemy.CurrentAIState == AIState.Combat
            && enemy.Target != null;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.Target == null) return TurnAction.Wait;

        int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);

        // 隣接時は攻撃
        if (distance == 1)
        {
            return TurnAction.Attack(enemy.Target);
        }

        // 接近
        return MoveTowards(enemy, enemy.Target.Position, state);
    }
}

/// <summary>
/// 不死行動ビヘイビア（Undead用） - 追跡を諦めない、逃走しない
/// </summary>
public class UndeadBehavior : AIBehaviorBase
{
    public override string Name => "Undead";
    public override int Priority => 105;  // FleeBehavior(100)より高い

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Undead
            && enemy.ShouldFlee();  // 通常なら逃走する状況
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        // アンデッドは逃走せず戦い続ける
        if (enemy.Target != null)
        {
            int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);
            if (distance == 1)
            {
                return TurnAction.Attack(enemy.Target);
            }
            return MoveTowards(enemy, enemy.Target.Position, state);
        }

        return TurnAction.Wait;
    }
}

/// <summary>
/// 不定形行動ビヘイビア（Amorphous用） - ランダム移動パターン
/// </summary>
public class AmorphousBehavior : AIBehaviorBase
{
    public override string Name => "Amorphous";
    public override int Priority => 15;

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Amorphous
            && enemy.CurrentAIState == AIState.Idle;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        // 不定形はランダムに這い回る
        if (state.Random.NextDouble() < 0.4)
        {
            return MoveRandom(enemy, state, state.Random);
        }

        return TurnAction.Wait;
    }
}

/// <summary>
/// 構造体行動ビヘイビア（Construct用） - 縄張り防衛特化
/// </summary>
public class ConstructBehavior : AIBehaviorBase
{
    private readonly int _guardRadius;

    public override string Name => "Construct";
    public override int Priority => 45;

    public ConstructBehavior(int guardRadius = 5)
    {
        _guardRadius = guardRadius;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Construct
            && enemy.Target != null;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.Target == null) return TurnAction.Wait;

        int distanceFromHome = enemy.Position.ChebyshevDistanceTo(enemy.HomePosition);
        int distanceToTarget = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);

        // 縄張り外なら帰還
        if (distanceFromHome > _guardRadius)
        {
            return MoveTowards(enemy, enemy.HomePosition, state);
        }

        // 隣接時は攻撃
        if (distanceToTarget == 1)
        {
            return TurnAction.Attack(enemy.Target);
        }

        // 縄張り内ならターゲットに接近
        if (distanceToTarget <= _guardRadius + 2)
        {
            return MoveTowards(enemy, enemy.Target.Position, state);
        }

        return TurnAction.Wait;
    }
}

/// <summary>
/// 竜行動ビヘイビア（Dragon用） - 遠距離攻撃優先
/// </summary>
public class DragonBehavior : AIBehaviorBase
{
    private readonly int _breathRange;

    public override string Name => "Dragon";
    public override int Priority => 75;

    public DragonBehavior(int breathRange = 3)
    {
        _breathRange = breathRange;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Dragon
            && enemy.CurrentAIState == AIState.Combat
            && enemy.Target != null;
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        if (enemy.Target == null) return TurnAction.Wait;

        int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);

        // ブレス射程内なら遠距離攻撃（射撃として処理）
        if (distance <= _breathRange && distance > 1)
        {
            return TurnAction.Attack(enemy.Target);
        }

        // 隣接時は近接攻撃
        if (distance == 1)
        {
            return TurnAction.Attack(enemy.Target);
        }

        // 接近
        return MoveTowards(enemy, enemy.Target.Position, state);
    }
}

/// <summary>
/// 精霊行動ビヘイビア（Spirit用） - 一定確率でテレポート移動
/// </summary>
public class SpiritBehavior : AIBehaviorBase
{
    private readonly float _teleportChance;

    public override string Name => "Spirit";
    public override int Priority => 35;

    public SpiritBehavior(float teleportChance = 0.2f)
    {
        _teleportChance = teleportChance;
    }

    public override bool IsApplicable(Enemy enemy, IGameState state)
    {
        return enemy.Race == MonsterRace.Spirit
            && (enemy.CurrentAIState == AIState.Combat || enemy.CurrentAIState == AIState.Alert);
    }

    public override TurnAction DecideAction(Enemy enemy, IGameState state)
    {
        // 一定確率でランダムな場所に瞬間移動
        if (state.Random.NextDouble() < _teleportChance)
        {
            return MoveRandom(enemy, state, state.Random);
        }

        // 通常の行動（ターゲットに接近 or 攻撃）
        if (enemy.Target != null)
        {
            int distance = enemy.Position.ChebyshevDistanceTo(enemy.Target.Position);
            if (distance == 1)
            {
                return TurnAction.Attack(enemy.Target);
            }
            return MoveTowards(enemy, enemy.Target.Position, state);
        }

        return TurnAction.Wait;
    }
}
