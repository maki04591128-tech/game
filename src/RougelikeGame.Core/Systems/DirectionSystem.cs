namespace RougelikeGame.Core.Systems;

/// <summary>
/// 方向攻撃の補正値
/// </summary>
public record DirectionBonus(float HitRateModifier, float DamageModifier);

/// <summary>
/// 高低差の補正値
/// </summary>
public record ElevationBonus(float DamageModifier, float HitRateModifier);

/// <summary>
/// 方向・向きシステム - 攻撃方向/高低差による補正を管理
/// </summary>
public static class DirectionSystem
{
    /// <summary>
    /// 攻撃方向に応じた命中率・ダメージ補正を取得
    /// </summary>
    public static DirectionBonus GetDirectionBonus(AttackDirection direction) => direction switch
    {
        AttackDirection.Front => new DirectionBonus(0f, 0f),
        AttackDirection.Side => new DirectionBonus(0.15f, 0.10f),
        AttackDirection.Back => new DirectionBonus(0.30f, 0.25f),
        _ => new DirectionBonus(0f, 0f)
    };

    /// <summary>
    /// 攻撃者と対象のFacingから攻撃方向を判定
    /// </summary>
    public static AttackDirection DetermineAttackDirection(Direction attackerFacing, Direction targetFacing)
    {
        // 対象の背面から攻撃（対象と同じ方向を向いている＝背後から）
        if (attackerFacing == targetFacing)
            return AttackDirection.Back;

        // 対象の正面から攻撃（対向している）
        if (IsOpposite(attackerFacing, targetFacing))
            return AttackDirection.Front;

        // それ以外は側面
        return AttackDirection.Side;
    }

    /// <summary>
    /// 高低差の補正を取得
    /// </summary>
    public static ElevationBonus GetElevationBonus(int attackerElevation, int targetElevation)
    {
        int diff = attackerElevation - targetElevation;
        return diff switch
        {
            > 0 => new ElevationBonus(0.15f, 0.10f),   // 高所→低所
            < 0 => new ElevationBonus(-0.15f, -0.10f),  // DT-2: 低所→高所（対称ペナルティに修正）
            _ => new ElevationBonus(0f, 0f)             // 同高度
        };
    }

    /// <summary>
    /// 移動方向からFacingを更新
    /// </summary>
    public static Direction GetFacingFromMovement(Direction moveDirection) => moveDirection;

    /// <summary>
    /// 2方向が対向しているか判定
    /// </summary>
    public static bool IsOpposite(Direction a, Direction b) => (a, b) switch
    {
        (Direction.North, Direction.South) => true,
        (Direction.South, Direction.North) => true,
        (Direction.East, Direction.West) => true,
        (Direction.West, Direction.East) => true,
        (Direction.NorthEast, Direction.SouthWest) => true,
        (Direction.SouthWest, Direction.NorthEast) => true,
        (Direction.NorthWest, Direction.SouthEast) => true,
        (Direction.SouthEast, Direction.NorthWest) => true,
        _ => false
    };

    /// <summary>
    /// 2方向が隣接(90度)かどうか判定
    /// </summary>
    public static bool IsAdjacent(Direction a, Direction b)
    {
        return !IsOpposite(a, b) && a != b;
    }
}
