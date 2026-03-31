using RougelikeGame.Core.AI;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Entities;

/// <summary>
/// 敵キャラクターの基底クラス
/// </summary>
public class Enemy : Character
{
    #region Enemy Properties
    /// <summary>
    /// 敵の種別ID（データ参照用）
    /// </summary>
    public string EnemyTypeId { get; init; } = string.Empty;

    /// <summary>
    /// モンスター種族
    /// </summary>
    public MonsterRace Race { get; init; } = MonsterRace.Humanoid;

    /// <summary>
    /// 使用する武器種（武器を使う種族用、nullの場合は種族固有攻撃）
    /// </summary>
    public WeaponType? WeaponType { get; init; }

    /// <summary>
    /// 経験値報酬
    /// </summary>
    public int ExperienceReward { get; init; }

    /// <summary>
    /// ドロップテーブルID
    /// </summary>
    public string? DropTableId { get; init; }

    /// <summary>
    /// 敵ランク（Common/Elite/Rare/Boss/HiddenBoss）
    /// </summary>
    public EnemyRank Rank { get; init; } = EnemyRank.Common;

    /// <summary>
    /// 視界範囲
    /// </summary>
    public int SightRange { get; init; } = 8;

    /// <summary>
    /// 聴覚範囲
    /// </summary>
    public int HearingRange { get; init; } = 5;

    /// <summary>
    /// 追跡を諦める距離
    /// </summary>
    public int GiveUpDistance { get; init; } = 15;

    /// <summary>
    /// 逃走を開始するHP割合
    /// </summary>
    public float FleeThreshold { get; init; } = 0.2f;

    /// <summary>
    /// 巡回ルート（パトロール用）
    /// </summary>
    public List<Position> PatrolRoute { get; init; } = new();

    /// <summary>
    /// 現在の巡回インデックス
    /// </summary>
    public int PatrolIndex { get; set; }
    #endregion

    #region AI State
    /// <summary>
    /// 現在のAI状態
    /// </summary>
    public AIState CurrentAIState { get; set; } = AIState.Idle;

    /// <summary>
    /// ターゲット（追跡対象）
    /// </summary>
    public Character? Target { get; set; }

    /// <summary>
    /// ターゲットの最後の既知位置
    /// </summary>
    public Position? LastKnownTargetPosition { get; set; }

    /// <summary>
    /// 警戒状態の残りターン
    /// </summary>
    public int AlertTurnsRemaining { get; set; }

    /// <summary>
    /// ホームポジション（生成位置）
    /// </summary>
    public Position HomePosition { get; set; }

    /// <summary>
    /// AIビヘイビア
    /// </summary>
    public IAIBehavior? Behavior { get; set; }
    #endregion

    #region Perception
    /// <summary>
    /// プレイヤーを視認できるか
    /// </summary>
    public bool CanSeePlayer(IGameState state)
    {
        var player = state.Player as Character;
        if (player == null || !player.IsAlive) return false;

        int distance = Position.ChebyshevDistanceTo(player.Position);
        if (distance > SightRange) return false;

        return state.CurrentMap.HasLineOfSight(Position, player.Position);
    }

    /// <summary>
    /// プレイヤーの音を聞けるか（戦闘中の場合）
    /// </summary>
    public bool CanHearPlayer(IGameState state)
    {
        var player = state.Player as Character;
        if (player == null || !player.IsAlive) return false;

        // 戦闘状態でない場合は聞こえない
        if (!state.CombatState.HasFlag(CombatState.Combat)) return false;

        int distance = Position.ChebyshevDistanceTo(player.Position);
        return distance <= HearingRange;
    }

    /// <summary>
    /// ターゲットを検知・更新
    /// </summary>
    public void UpdateTargetAwareness(IGameState state)
    {
        var player = state.Player as Character;
        if (player == null) return;

        if (CanSeePlayer(state))
        {
            Target = player;
            LastKnownTargetPosition = player.Position;
            AlertTurnsRemaining = 30; // 30ターン警戒維持
        }
        else if (CanHearPlayer(state))
        {
            LastKnownTargetPosition = player.Position;
            AlertTurnsRemaining = 15; // 15ターン警戒
        }
        else if (AlertTurnsRemaining > 0)
        {
            AlertTurnsRemaining--;
            if (AlertTurnsRemaining == 0)
            {
                Target = null;
                LastKnownTargetPosition = null;
            }
        }
    }

    /// <summary>
    /// 逃走すべきか判定
    /// </summary>
    public bool ShouldFlee()
    {
        float hpRatio = (float)CurrentHp / MaxHp;
        return hpRatio <= FleeThreshold;
    }

    /// <summary>
    /// 追跡を諦めるべきか判定
    /// </summary>
    public bool ShouldGiveUpChase()
    {
        if (Target == null) return true;

        int distanceFromHome = Position.ChebyshevDistanceTo(HomePosition);
        return distanceFromHome > GiveUpDistance;
    }
    #endregion

    #region AI State Transitions
    /// <summary>
    /// AI状態を更新
    /// </summary>
    public void UpdateAIState(IGameState state)
    {
        UpdateTargetAwareness(state);

        var previousState = CurrentAIState;

        // 状態遷移ロジック
        CurrentAIState = DetermineNextState(state);

        if (previousState != CurrentAIState)
        {
            OnAIStateChanged?.Invoke(this, new AIStateChangedEventArgs(previousState, CurrentAIState));
        }
    }

    private AIState DetermineNextState(IGameState state)
    {
        // 逃走判定
        if (ShouldFlee() && Target != null)
        {
            return AIState.Flee;
        }

        // ターゲットが視認可能 → 戦闘
        if (Target != null && CanSeePlayer(state))
        {
            return AIState.Combat;
        }

        // 最後の既知位置がある → 警戒・追跡
        if (LastKnownTargetPosition.HasValue)
        {
            if (ShouldGiveUpChase())
            {
                LastKnownTargetPosition = null;
                Target = null;
                return AIState.Patrol;
            }
            return AIState.Alert;
        }

        // 巡回ルートがある → パトロール
        if (PatrolRoute.Count > 0)
        {
            return AIState.Patrol;
        }

        return AIState.Idle;
    }
    #endregion

    #region Turn System
    public override TurnAction DecideAction(IGameState state)
    {
        UpdateAIState(state);

        if (Behavior != null)
        {
            return Behavior.DecideAction(this, state);
        }

        // デフォルトビヘイビア
        return CurrentAIState switch
        {
            AIState.Combat => DecideCombatAction(state),
            AIState.Alert => DecideAlertAction(state),
            AIState.Flee => DecideFleeAction(state),
            AIState.Patrol => DecidePatrolAction(state),
            _ => TurnAction.Wait
        };
    }

    private TurnAction DecideCombatAction(IGameState state)
    {
        if (Target == null) return TurnAction.Wait;

        int distance = Position.ChebyshevDistanceTo(Target.Position);

        // 隣接している → 攻撃
        if (distance == 1)
        {
            return TurnAction.Attack(Target);
        }

        // 接近
        return MoveTowards(Target.Position, state);
    }

    private TurnAction DecideAlertAction(IGameState state)
    {
        if (!LastKnownTargetPosition.HasValue) return TurnAction.Wait;

        // 最後の既知位置に到達した
        if (Position == LastKnownTargetPosition.Value)
        {
            LastKnownTargetPosition = null;
            return TurnAction.Search;
        }

        // 最後の既知位置へ移動
        return MoveTowards(LastKnownTargetPosition.Value, state);
    }

    private TurnAction DecideFleeAction(IGameState state)
    {
        if (Target == null) return TurnAction.Wait;

        // ターゲットから離れる方向へ移動
        return MoveAwayFrom(Target.Position, state);
    }

    private TurnAction DecidePatrolAction(IGameState state)
    {
        if (PatrolRoute.Count == 0)
        {
            return TurnAction.Wait;
        }

        var targetPos = PatrolRoute[PatrolIndex];

        // 目標地点に到達
        if (Position == targetPos)
        {
            PatrolIndex = (PatrolIndex + 1) % PatrolRoute.Count;
            targetPos = PatrolRoute[PatrolIndex];
        }

        return MoveTowards(targetPos, state);
    }

    private TurnAction MoveTowards(Position target, IGameState state)
    {
        var direction = Position.GetDirectionTo(target);
        var nextPos = Position.Move(direction);

        if (state.CurrentMap.CanMoveTo(nextPos))
        {
            return TurnAction.Move(direction);
        }

        // 直接移動できない場合は迂回を試みる
        foreach (var neighbor in Position.GetNeighbors(includeDiagonals: true))
        {
            if (state.CurrentMap.CanMoveTo(neighbor) &&
                neighbor.ChebyshevDistanceTo(target) < Position.ChebyshevDistanceTo(target))
            {
                var altDirection = Position.GetDirectionTo(neighbor);
                return TurnAction.Move(altDirection);
            }
        }

        return TurnAction.Wait;
    }

    private TurnAction MoveAwayFrom(Position threat, IGameState state)
    {
        var awayDirection = threat.GetDirectionTo(Position);
        var nextPos = Position.Move(awayDirection);

        if (state.CurrentMap.CanMoveTo(nextPos))
        {
            return TurnAction.Move(awayDirection);
        }

        // 逃げられない場合は戦う
        int distance = Position.ChebyshevDistanceTo(threat);
        if (distance == 1 && Target != null)
        {
            return TurnAction.Attack(Target);
        }

        return TurnAction.Wait;
    }

    public override void ExecuteAction(TurnAction action, IGameState state)
    {
        switch (action.Type)
        {
            case TurnActionType.Move:
                Position = Position.Move(action.Direction);
                FacingDirection = action.Direction;
                break;

            case TurnActionType.Attack:
                if (action.Target != null)
                {
                    state.CombatSystem.ExecuteAttack(this, action.Target, AttackType.Slash);
                }
                break;

            case TurnActionType.Search:
                // 周囲を見回す（特に何もしない）
                break;
        }

        TickStatusEffects();
    }
    #endregion

    #region Factory Methods
    /// <summary>
    /// 基本的な敵を作成
    /// </summary>
    public static Enemy Create(string name, string typeId, Stats stats, int expReward)
    {
        var enemy = new Enemy
        {
            Name = name,
            EnemyTypeId = typeId,
            BaseStats = stats,
            ExperienceReward = expReward,
            Faction = Faction.Enemy
        };

        enemy.InitializeResources();
        enemy.HomePosition = enemy.Position;

        return enemy;
    }
    #endregion

    #region Events
    public event EventHandler<AIStateChangedEventArgs>? OnAIStateChanged;
    #endregion
}

/// <summary>
/// AI状態変更イベント引数
/// </summary>
public class AIStateChangedEventArgs : EventArgs
{
    public AIState PreviousState { get; }
    public AIState NewState { get; }

    public AIStateChangedEventArgs(AIState previousState, AIState newState)
    {
        PreviousState = previousState;
        NewState = newState;
    }
}
