using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core;

/// <summary>
/// ダメージ情報を表す値オブジェクト
/// </summary>
public readonly record struct Damage(
    int Amount,
    DamageType Type,
    Element Element,
    bool IsCritical,
    string? SourceName = null
)
{
    /// <summary>
    /// 物理ダメージを作成
    /// </summary>
    public static Damage Physical(int amount, bool isCritical = false) =>
        new(amount, DamageType.Physical, Element.None, isCritical);

    /// <summary>
    /// 魔法ダメージを作成
    /// </summary>
    public static Damage Magical(int amount, Element element, bool isCritical = false) =>
        new(amount, DamageType.Magical, element, isCritical);

    /// <summary>
    /// 純粋ダメージを作成（防御無視）
    /// </summary>
    public static Damage Pure(int amount) =>
        new(amount, DamageType.Pure, Element.None, false);

    /// <summary>
    /// 回復を作成
    /// </summary>
    public static Damage Healing(int amount) =>
        new(amount, DamageType.Healing, Element.None, false);

    /// <summary>
    /// 倍率を適用
    /// </summary>
    public Damage WithMultiplier(float multiplier) =>
        this with { Amount = Math.Max(1, (int)(Amount * multiplier)) };

    /// <summary>
    /// 属性を変更
    /// </summary>
    public Damage WithElement(Element element) =>
        this with { Element = element };

    /// <summary>
    /// ソース名を設定
    /// </summary>
    public Damage WithSource(string sourceName) =>
        this with { SourceName = sourceName };

    /// <summary>
    /// 最終ダメージを計算（防御・耐性考慮）
    /// </summary>
    public int CalculateFinal(int defense, float resistance = 0f)
    {
        if (Type == DamageType.Pure)
            return Math.Max(GameConstants.MinimumDamage, Amount);

        if (Type == DamageType.Healing)
            return Amount;

        int afterDefense = Math.Max(GameConstants.MinimumDamage, Amount - defense);
        int afterResistance = (int)(afterDefense * (1f - Math.Clamp(resistance, -1f, 0.9f)));

        return Math.Max(GameConstants.MinimumDamage, afterResistance);
    }
}

/// <summary>
/// 戦闘結果
/// </summary>
public readonly record struct CombatResult(
    bool IsHit,
    bool IsCritical,
    Damage? Damage,
    IDamageable? Attacker,
    IDamageable? Target
)
{
    public static CombatResult Miss(IDamageable attacker, IDamageable target) =>
        new(false, false, null, attacker, target);

    public static CombatResult Hit(IDamageable attacker, IDamageable target, Damage damage) =>
        new(true, damage.IsCritical, damage, attacker, target);
}

/// <summary>
/// ダメージイベント引数
/// </summary>
public class DamageEventArgs : EventArgs
{
    public Damage OriginalDamage { get; }
    public int FinalDamage { get; }
    public int RemainingHp { get; }

    public DamageEventArgs(Damage originalDamage, int finalDamage, int remainingHp = 0)
    {
        OriginalDamage = originalDamage;
        FinalDamage = finalDamage;
        RemainingHp = remainingHp;
    }
}

/// <summary>
/// 死亡イベント引数
/// </summary>
public class DeathEventArgs : EventArgs
{
    public Damage? KillingBlow { get; }
    public DeathCause Cause { get; }

    public DeathEventArgs(Damage? killingBlow, DeathCause cause = DeathCause.Combat)
    {
        KillingBlow = killingBlow;
        Cause = cause;
    }
}

/// <summary>
/// 回復イベント引数
/// </summary>
public class HealEventArgs : EventArgs
{
    public int Amount { get; }
    public int NewHp { get; }

    public HealEventArgs(int amount, int newHp)
    {
        Amount = amount;
        NewHp = newHp;
    }
}
