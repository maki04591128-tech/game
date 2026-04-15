namespace RougelikeGame.Engine.Combat;

using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;

/// <summary>
/// ダメージ計算システム
/// 設計書「2. ダメージ計算式」に基づく実装
/// </summary>
public class DamageCalculator
{
    private readonly IRandomProvider _random;

    public DamageCalculator(IRandomProvider random)
    {
        _random = random;
    }

    #region 物理ダメージ計算

    /// <summary>
    /// 物理ダメージを計算
    /// 基礎ダメージ = (攻撃力 × スキル倍率) - (防御力 × 0.65)
    /// 最終ダメージ = 基礎ダメージ × 乱数(0.9~1.1) × 属性相性 × クリティカル倍率
    /// </summary>
    public DamageResult CalculatePhysicalDamage(PhysicalDamageParams param)
    {
        // 攻撃力 = 武器攻撃力 + (STR × 2) + バフ補正
        int attackPower = param.WeaponAttack + (param.Strength * 2) + param.AttackBuff;

        // 防御力 = 防具防御力 + (VIT × 1.5) + バフ補正
        int defensePower = param.ArmorDefense + (int)(param.Vitality * 1.5) + param.DefenseBuff;

        // BS-4: 防御貫通を適用（防御力からArmorPenetration分を差し引く）
        int effectiveDefense = Math.Max(0, defensePower - param.ArmorPenetration);

        // 基礎ダメージ (K-4: 防御係数0.5→0.65に引上げ、VIT投資価値向上)
        int baseDamage = (int)(attackPower * param.SkillMultiplier) - (int)(effectiveDefense * 0.65);

        // BS-8: レベルベースのダメージスケーリング（レベル10ごとに5%ボーナス）
        float levelScaling = 1.0f + (param.AttackerLevel - 1) * 0.005f;
        baseDamage = (int)(baseDamage * levelScaling);
        baseDamage = Math.Max(GameConstants.MinimumDamage, baseDamage);

        // 乱数幅 (0.9~1.1)
        double variance = 0.9 + (_random.NextDouble() * 0.2);

        // 属性相性
        float elementMultiplier = ElementSystem.GetAffinityMultiplier(param.AttackElement, param.TargetElement);

        // クリティカル判定
        bool isCritical = CheckCritical(param.CriticalRate);
        float criticalMultiplier = isCritical ? param.CriticalDamageMultiplier : 1.0f;

        // 最終ダメージ計算
        int finalDamage = (int)(baseDamage * variance * elementMultiplier * criticalMultiplier);
        finalDamage = Math.Max(GameConstants.MinimumDamage, finalDamage);

        return new DamageResult
        {
            BaseDamage = baseDamage,
            FinalDamage = finalDamage,
            IsCritical = isCritical,
            ElementAffinity = ElementSystem.GetAffinityType(param.AttackElement, param.TargetElement),
            Variance = variance,
            DamageType = DamageType.Physical
        };
    }

    #endregion

    #region 魔法ダメージ計算

    /// <summary>
    /// 魔法ダメージを計算
    /// 基礎ダメージ = (魔法攻撃力 × スキル倍率 × 魔法言語補正) - (魔法防御 × 0.5)
    /// ※K-1修正: 魔法防御係数を0.3→0.5に引き上げ（物理0.65とは別調整）
    /// 最終ダメージ = 基礎ダメージ × 乱数(0.9~1.1) × 属性相性
    /// ※魔法は必中
    /// </summary>
    public DamageResult CalculateMagicalDamage(MagicalDamageParams param)
    {
        // 魔法攻撃力 = 杖攻撃力 + (INT × 3) + バフ補正
        int magicAttack = param.StaffAttack + (param.Intelligence * 3) + param.MagicAttackBuff;

        // 魔法防御 = 防具魔防 + (MND × 2) + バフ補正
        int magicDefense = param.MagicDefense + (param.Mind * 2) + param.MagicDefenseBuff;

        // 基礎ダメージ（魔法言語補正含む）
        int baseDamage = (int)(magicAttack * param.SkillMultiplier * param.LanguageBonus) - (int)(magicDefense * 0.5);  // K-1: 魔法防御係数0.5（物理0.65とは別調整）
        baseDamage = Math.Max(GameConstants.MinimumDamage, baseDamage);

        // 乱数幅 (0.9~1.1)
        double variance = 0.9 + (_random.NextDouble() * 0.2);

        // 属性相性
        float elementMultiplier = ElementSystem.GetAffinityMultiplier(param.SpellElement, param.TargetElement);

        // 最終ダメージ計算（K-2: クリティカル判定はCombatSystem側で適用）
        int finalDamage = (int)(baseDamage * variance * elementMultiplier);
        finalDamage = Math.Max(GameConstants.MinimumDamage, finalDamage);

        return new DamageResult
        {
            BaseDamage = baseDamage,
            FinalDamage = finalDamage,
            IsCritical = false,
            ElementAffinity = ElementSystem.GetAffinityType(param.SpellElement, param.TargetElement),
            Variance = variance,
            DamageType = DamageType.Magical
        };
    }

    #endregion

    #region 命中・回避・クリティカル

    /// <summary>
    /// 命中判定
    /// 命中率 = 基礎命中率 + (DEX × 1%) - 敵回避率 + 補正
    /// </summary>
    public HitCheckResult CheckHit(HitCheckParams param)
    {
        // 魔法は必中
        if (param.AttackType == AttackType.Magic)
        {
            return new HitCheckResult { IsHit = true, HitRate = 1.0, EvasionRate = 0 };
        }

        // 基礎命中率
        double baseHitRate = param.AttackType switch
        {
            AttackType.Ranged => 0.80,
            AttackType.Magic => 1.00,
            _ => 0.90  // 近接攻撃
        };

        // DEXによる命中補正
        double dexBonus = param.Dexterity * 0.01;

        // 回避率 = 基礎回避率 + (AGI × 0.5%) + (LUK × 0.2%) + 装備補正
        double baseEvasion = param.TargetArmorClass switch
        {
            ArmorClass.Heavy => 0.00,
            ArmorClass.Medium => 0.05,
            ArmorClass.Light => 0.10,
            ArmorClass.Robe => 0.15,
            ArmorClass.None => 0.20,
            _ => 0.0
        };

        double evasionRate = baseEvasion + (param.TargetAgility * 0.005) + (param.TargetLuck * 0.002) + param.TargetEvasionBonus;

        // 回避上限 = 75%
        evasionRate = Math.Min(evasionRate, 0.75);

        // 最終命中率
        double finalHitRate = baseHitRate + dexBonus - evasionRate + param.HitRateBonus;
        finalHitRate = Math.Clamp(finalHitRate, 0.25, 0.99);  // 最低25%は命中保証

        // 判定
        bool isHit = _random.NextDouble() < finalHitRate;

        return new HitCheckResult
        {
            IsHit = isHit,
            HitRate = finalHitRate,
            EvasionRate = evasionRate
        };
    }

    /// <summary>
    /// クリティカル判定
    /// クリティカル率 = 基礎5% + (DEX × 0.5%) + (LUK × 0.5%) + 武器補正 + スキル補正
    /// ※K-3修正: DEXの寄与をLUK（0.5%/pt）と統一
    /// </summary>
    public bool CheckCritical(CriticalCheckParams param)
    {
        double critRate = GameConstants.BaseCriticalRate 
            + (param.Dexterity * 0.005)   // K-3: DEXのクリティカル率寄与をLUKと統一
            + (param.Luck * 0.005) 
            + param.WeaponCritBonus 
            + param.SkillCritBonus;

        return _random.NextDouble() < critRate;
    }

    private bool CheckCritical(double criticalRate)
    {
        return _random.NextDouble() < criticalRate;
    }

    #endregion

    #region 防御計算

    /// <summary>
    /// 防御時のダメージ軽減を計算
    /// 防御時: ダメージ × 0.5
    /// </summary>
    public int CalculateDefendedDamage(int originalDamage)
    {
        return Math.Max(GameConstants.MinimumDamage, (int)(originalDamage * 0.5));
    }

    /// <summary>
    /// BS-11: 盾ブロック判定。盾のBlockChanceに基づきブロック成功/失敗を判定する。
    /// ブロック成功時はBlockReduction分だけダメージを軽減する。
    /// </summary>
    public (bool Blocked, int ReducedDamage) CalculateShieldBlock(int originalDamage, float blockChance, float blockReduction)
    {
        if (blockChance <= 0) return (false, originalDamage);
        bool blocked = _random.NextDouble() < blockChance;
        if (!blocked) return (false, originalDamage);
        int reducedDamage = Math.Max(GameConstants.MinimumDamage, (int)(originalDamage * (1.0f - blockReduction)));
        return (true, reducedDamage);
    }

    /// <summary>
    /// HP状態によるペナルティを取得
    /// </summary>
    public HpStatePenalty GetHpStatePenalty(int currentHp, int maxHp)
    {
        if (maxHp <= 0)
            return new HpStatePenalty(HpState.Dead, 1.0, 1.0);

        double ratio = (double)currentHp / maxHp;

        return ratio switch
        {
            >= 0.76 => new HpStatePenalty(HpState.Healthy, 0, 0),
            >= 0.51 => new HpStatePenalty(HpState.Injured, 0, 0),
            >= 0.26 => new HpStatePenalty(HpState.Wounded, 0.10, 0.20),
            >= 0.01 => new HpStatePenalty(HpState.Critical, 0.25, 0.50),
            _ => new HpStatePenalty(HpState.Dead, 1.0, 1.0)
        };
    }

    #endregion
}

#region パラメータ構造体

/// <summary>
/// 物理ダメージ計算パラメータ
/// </summary>
public record struct PhysicalDamageParams
{
    public int WeaponAttack { get; init; }
    public int Strength { get; init; }
    public int AttackBuff { get; init; }
    public int ArmorDefense { get; init; }
    public int Vitality { get; init; }
    public int DefenseBuff { get; init; }
    public float SkillMultiplier { get; init; }
    public Element AttackElement { get; init; }
    public Element TargetElement { get; init; }
    public double CriticalRate { get; init; }
    public float CriticalDamageMultiplier { get; init; }
    /// <summary>BS-4: 防御貫通値（防御力をこの値分だけ無視する）</summary>
    public int ArmorPenetration { get; init; }
    /// <summary>BS-8: レベルベースのダメージスケーリング（攻撃側レベル）</summary>
    public int AttackerLevel { get; init; }

    public PhysicalDamageParams()
    {
        SkillMultiplier = 1.0f;
        CriticalDamageMultiplier = 1.5f;
        AttackElement = Element.None;
        TargetElement = Element.None;
        AttackerLevel = 1;
    }
}

/// <summary>
/// 魔法ダメージ計算パラメータ
/// </summary>
public record struct MagicalDamageParams
{
    public int StaffAttack { get; init; }
    public int Intelligence { get; init; }
    public int MagicAttackBuff { get; init; }
    public int MagicDefense { get; init; }
    public int Mind { get; init; }
    public int MagicDefenseBuff { get; init; }
    public float SkillMultiplier { get; init; }
    public float LanguageBonus { get; init; }
    public Element SpellElement { get; init; }
    public Element TargetElement { get; init; }

    public MagicalDamageParams()
    {
        SkillMultiplier = 1.0f;
        LanguageBonus = 1.0f;
        SpellElement = Element.None;
        TargetElement = Element.None;
    }
}

/// <summary>
/// 命中判定パラメータ
/// </summary>
public record struct HitCheckParams
{
    public AttackType AttackType { get; init; }
    public int Dexterity { get; init; }
    public double HitRateBonus { get; init; }
    public ArmorClass TargetArmorClass { get; init; }
    public int TargetAgility { get; init; }
    public int TargetLuck { get; init; }
    public double TargetEvasionBonus { get; init; }
}

/// <summary>
/// クリティカル判定パラメータ
/// </summary>
public record struct CriticalCheckParams
{
    public int Dexterity { get; init; }
    public int Luck { get; init; }
    public double WeaponCritBonus { get; init; }
    public double SkillCritBonus { get; init; }
}

#endregion

#region 結果構造体

/// <summary>
/// ダメージ計算結果
/// </summary>
public record struct DamageResult
{
    public int BaseDamage { get; init; }
    public int FinalDamage { get; init; }
    public bool IsCritical { get; init; }
    public ElementAffinity ElementAffinity { get; init; }
    public double Variance { get; init; }
    public DamageType DamageType { get; init; }
}

/// <summary>
/// 命中判定結果
/// </summary>
public record struct HitCheckResult
{
    public bool IsHit { get; init; }
    public double HitRate { get; init; }
    public double EvasionRate { get; init; }
}

/// <summary>
/// HP状態ペナルティ
/// </summary>
public record struct HpStatePenalty(HpState State, double AttackPenalty, double SpeedPenalty);

#endregion

#region 列挙型

/// <summary>
/// 防具クラス（回避率に影響）
/// </summary>
public enum ArmorClass
{
    /// <summary>裸 - 基礎回避20%</summary>
    None,
    /// <summary>ローブ - 基礎回避15%</summary>
    Robe,
    /// <summary>軽装 - 基礎回避10%</summary>
    Light,
    /// <summary>中装 - 基礎回避5%</summary>
    Medium,
    /// <summary>重装 - 基礎回避0%</summary>
    Heavy
}

/// <summary>
/// HP状態
/// </summary>
public enum HpState
{
    /// <summary>健康 (76-100%)</summary>
    Healthy,
    /// <summary>負傷 (51-75%)</summary>
    Injured,
    /// <summary>重傷 (26-50%) - 攻撃力-10%, 移動速度-20%</summary>
    Wounded,
    /// <summary>瀕死 (1-25%) - 攻撃力-25%, 移動速度-50%</summary>
    Critical,
    /// <summary>死亡 (0%)</summary>
    Dead
}

#endregion
