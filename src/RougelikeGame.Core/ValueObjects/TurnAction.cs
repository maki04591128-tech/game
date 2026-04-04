using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core;

/// <summary>
/// キャラクターの1回の行動を表す
/// </summary>
public readonly struct TurnAction
{
    public TurnActionType Type { get; init; }
    public int BaseTurnCost { get; init; }
    public Direction Direction { get; init; }
    public IDamageable? Target { get; init; }
    public string? SkillId { get; init; }
    public string? Incantation { get; init; }
    public string? ItemId { get; init; }

    /// <summary>
    /// 最終ターン消費を計算
    /// </summary>
    public int CalculateFinalCost(CombatState combatState, float equipmentModifier = 1.0f, float statusModifier = 1.0f, float environmentModifier = 1.0f)
    {
        float modifier = 1.0f;

        // 状況修正（戦闘中・隠密中の移動）
        if (Type == TurnActionType.Move)
        {
            modifier *= GetMovementMultiplier(combatState);

            // 斜め移動補正
            if (Direction.IsDiagonal())
            {
                modifier *= (float)TurnCosts.DiagonalNumerator / TurnCosts.DiagonalDenominator;
            }
        }

        // 各種修正適用
        modifier *= equipmentModifier;
        modifier *= statusModifier;
        modifier *= environmentModifier;

        return Math.Max(1, (int)Math.Ceiling(BaseTurnCost * modifier));
    }

    private static float GetMovementMultiplier(CombatState state)
    {
        if (state.HasFlag(CombatState.Combat)) return TurnCosts.MoveCombat;
        if (state.HasFlag(CombatState.Stealth)) return TurnCosts.MoveStealth;
        if (state.HasFlag(CombatState.Pursuit)) return TurnCosts.MovePursuit;
        if (state.HasFlag(CombatState.Alert)) return TurnCosts.MoveAlert;
        return TurnCosts.MoveNormal;
    }

    // ファクトリメソッド

    /// <summary>
    /// 移動アクション
    /// </summary>
    public static TurnAction Move(Direction direction) => new()
    {
        Type = TurnActionType.Move,
        Direction = direction,
        BaseTurnCost = TurnCosts.MoveNormal
    };

    /// <summary>
    /// 通常攻撃
    /// </summary>
    public static TurnAction Attack(IDamageable target) => new()
    {
        Type = TurnActionType.Attack,
        Target = target,
        BaseTurnCost = TurnCosts.AttackNormal
    };

    /// <summary>
    /// スキル使用
    /// </summary>
    public static TurnAction UseSkill(string skillId, IDamageable? target, int turnCost) => new()
    {
        Type = TurnActionType.UseSkill,
        SkillId = skillId,
        Target = target,
        BaseTurnCost = Math.Clamp(turnCost, 1, 50)  // IO-3: ターンコスト範囲チェック
    };

    /// <summary>
    /// 魔法詠唱
    /// </summary>
    public static TurnAction CastSpell(string incantation, int turnCost) => new()
    {
        Type = TurnActionType.CastSpell,
        Incantation = incantation,
        BaseTurnCost = Math.Clamp(turnCost, TurnCosts.SpellMinimum, TurnCosts.SpellMaximum)
    };

    /// <summary>
    /// アイテム使用
    /// </summary>
    public static TurnAction UseItem(string itemId, int turnCost) => new()
    {
        Type = TurnActionType.UseItem,
        ItemId = itemId,
        BaseTurnCost = turnCost
    };

    /// <summary>
    /// 待機
    /// </summary>
    public static TurnAction Wait => new()
    {
        Type = TurnActionType.Wait,
        BaseTurnCost = TurnCosts.Wait
    };

    /// <summary>
    /// 休息
    /// </summary>
    public static TurnAction Rest => new()
    {
        Type = TurnActionType.Rest,
        BaseTurnCost = TurnCosts.Rest
    };

    /// <summary>
    /// 周囲を調べる
    /// </summary>
    public static TurnAction Search => new()
    {
        Type = TurnActionType.Search,
        BaseTurnCost = TurnCosts.Search
    };

    /// <summary>
    /// インタラクト
    /// </summary>
    public static TurnAction Interact => new()
    {
        Type = TurnActionType.Interact,
        BaseTurnCost = TurnCosts.OpenDoor
    };

    /// <summary>
    /// 入力待ち（プレイヤー専用）
    /// </summary>
    public static TurnAction WaitForInput => new()
    {
        Type = TurnActionType.Wait,
        BaseTurnCost = 0
    };
}

/// <summary>
/// セーブ情報
/// </summary>
public record SaveInfo(
    string SlotName,
    DateTime SaveTime,
    string PlayerName,
    int PlayerLevel,
    int PlayTime,
    int DeathCount
);

// DungeonGenerationParameters is defined in Map/Tile.cs
