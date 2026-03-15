using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core;

/// <summary>
/// 罠の種類
/// </summary>
public enum TrapType
{
    /// <summary>毒の針 - 毒状態異常</summary>
    Poison,

    /// <summary>落とし穴 - ダメージ + 下階層移動</summary>
    PitFall,

    /// <summary>テレポート - ランダム位置に転送</summary>
    Teleport,

    /// <summary>矢の罠 - 物理ダメージ</summary>
    Arrow,

    /// <summary>警報 - 周囲の敵を覚醒</summary>
    Alarm,

    /// <summary>火炎 - 炎ダメージ</summary>
    Fire,

    /// <summary>睡眠ガス - 睡眠状態異常</summary>
    Sleep,

    /// <summary>混乱ガス - 混乱状態異常</summary>
    Confusion
}

/// <summary>
/// 罠の定義データ
/// </summary>
public class TrapDefinition
{
    /// <summary>罠の種類</summary>
    public TrapType Type { get; }

    /// <summary>表示名</summary>
    public string Name { get; }

    /// <summary>基本ダメージ（0ならダメージなし）</summary>
    public int BaseDamage { get; }

    /// <summary>付与する状態異常（なしならnull）</summary>
    public StatusEffectType? StatusEffect { get; }

    /// <summary>状態異常の持続ターン</summary>
    public int StatusDuration { get; }

    /// <summary>発見難易度（高いほど見つけにくい、PER判定の閾値）</summary>
    public int DetectionDifficulty { get; }

    /// <summary>解除難易度（高いほど解除しにくい、DEX判定の閾値）</summary>
    public int DisarmDifficulty { get; }

    /// <summary>階層ごとのダメージ増加量</summary>
    public int DamagePerFloor { get; }

    private TrapDefinition(
        TrapType type, string name, int baseDamage,
        StatusEffectType? statusEffect, int statusDuration,
        int detectionDifficulty, int disarmDifficulty, int damagePerFloor)
    {
        Type = type;
        Name = name;
        BaseDamage = baseDamage;
        StatusEffect = statusEffect;
        StatusDuration = statusDuration;
        DetectionDifficulty = detectionDifficulty;
        DisarmDifficulty = disarmDifficulty;
        DamagePerFloor = damagePerFloor;
    }

    /// <summary>
    /// 罠の種類から定義を取得
    /// </summary>
    public static TrapDefinition Get(TrapType type) => type switch
    {
        TrapType.Poison => PoisonTrap,
        TrapType.PitFall => PitFallTrap,
        TrapType.Teleport => TeleportTrap,
        TrapType.Arrow => ArrowTrap,
        TrapType.Alarm => AlarmTrap,
        TrapType.Fire => FireTrap,
        TrapType.Sleep => SleepTrap,
        TrapType.Confusion => ConfusionTrap,
        _ => ArrowTrap
    };

    /// <summary>実際のダメージを計算（階層補正込み）</summary>
    public int CalculateDamage(int floor) => BaseDamage + DamagePerFloor * (floor - 1);

    /// <summary>PER値でこの罠を発見できるか判定</summary>
    public bool CanDetect(int perception, IRandomProvider random)
    {
        // PER + ランダム(0-9) >= 発見難易度 で発見
        return perception + random.Next(10) >= DetectionDifficulty;
    }

    /// <summary>DEX値でこの罠を解除できるか判定</summary>
    public bool CanDisarm(int dexterity, IRandomProvider random)
    {
        // DEX + ランダム(0-9) >= 解除難易度 で解除
        return dexterity + random.Next(10) >= DisarmDifficulty;
    }

    public static readonly TrapDefinition PoisonTrap = new(
        TrapType.Poison, "毒針の罠", 3,
        StatusEffectType.Poison, 10,
        detectionDifficulty: 12, disarmDifficulty: 10, damagePerFloor: 1);

    public static readonly TrapDefinition PitFallTrap = new(
        TrapType.PitFall, "落とし穴", 10,
        null, 0,
        detectionDifficulty: 10, disarmDifficulty: 15, damagePerFloor: 3);

    public static readonly TrapDefinition TeleportTrap = new(
        TrapType.Teleport, "テレポートの罠", 0,
        null, 0,
        detectionDifficulty: 14, disarmDifficulty: 14, damagePerFloor: 0);

    public static readonly TrapDefinition ArrowTrap = new(
        TrapType.Arrow, "矢の罠", 8,
        null, 0,
        detectionDifficulty: 10, disarmDifficulty: 8, damagePerFloor: 2);

    public static readonly TrapDefinition AlarmTrap = new(
        TrapType.Alarm, "警報の罠", 0,
        null, 0,
        detectionDifficulty: 8, disarmDifficulty: 6, damagePerFloor: 0);

    public static readonly TrapDefinition FireTrap = new(
        TrapType.Fire, "火炎の罠", 12,
        StatusEffectType.Burn, 5,
        detectionDifficulty: 12, disarmDifficulty: 12, damagePerFloor: 3);

    public static readonly TrapDefinition SleepTrap = new(
        TrapType.Sleep, "睡眠ガスの罠", 0,
        StatusEffectType.Sleep, 8,
        detectionDifficulty: 14, disarmDifficulty: 10, damagePerFloor: 0);

    public static readonly TrapDefinition ConfusionTrap = new(
        TrapType.Confusion, "混乱ガスの罠", 0,
        StatusEffectType.Confusion, 6,
        detectionDifficulty: 12, disarmDifficulty: 10, damagePerFloor: 0);

    /// <summary>全罠定義の配列</summary>
    public static readonly TrapDefinition[] AllTraps =
    [
        PoisonTrap, PitFallTrap, TeleportTrap, ArrowTrap,
        AlarmTrap, FireTrap, SleepTrap, ConfusionTrap
    ];
}
