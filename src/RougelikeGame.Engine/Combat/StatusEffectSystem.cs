namespace RougelikeGame.Engine.Combat;

using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;

/// <summary>
/// 状態異常システム
/// 設計書「5. 状態異常」に基づく実装
/// </summary>
public class StatusEffectSystem
{
    private readonly IRandomProvider _random;

    public StatusEffectSystem(IRandomProvider random)
    {
        _random = random;
    }

    #region 状態異常生成

    /// <summary>
    /// 毒状態を生成（ターンごとにHP減少：最大HPの2%）
    /// </summary>
    public StatusEffect CreatePoison(int maxHp, int duration = 20)
    {
        int damage = Math.Max(1, (int)(maxHp * 0.02));
        return new StatusEffect(StatusEffectType.Poison, duration)
        {
            Name = "毒",
            DamagePerTick = damage,
            DamageElement = Element.Poison,
            MaxStack = 3
        };
    }

    /// <summary>
    /// 猛毒状態を生成（ターンごとにHP減少：最大HPの5%）
    /// </summary>
    public StatusEffect CreateDeadlyPoison(int maxHp, int duration = 20)
    {
        int damage = Math.Max(1, (int)(maxHp * 0.05));
        return new StatusEffect(StatusEffectType.Poison, duration)
        {
            Name = "猛毒",
            DamagePerTick = damage,
            DamageElement = Element.Poison,
            MaxStack = 1,
            Priority = 1  // 通常の毒より優先
        };
    }

    /// <summary>
    /// 出血状態を生成（移動ごとにHP減少）
    /// </summary>
    public StatusEffect CreateBleeding(int damage = 3, int duration = 10)
    {
        return new StatusEffect(StatusEffectType.Bleeding, duration)
        {
            Name = "出血",
            DamagePerTick = damage,
            MaxStack = 5
        };
    }

    /// <summary>
    /// 火傷状態を生成（物理攻撃力-15%、水属性に弱体化）
    /// </summary>
    public StatusEffect CreateBurn(int duration = 15)
    {
        return new StatusEffect(StatusEffectType.Burn, duration)
        {
            Name = "火傷",
            AttackMultiplier = 0.85f
        };
    }

    /// <summary>
    /// 凍結状態を生成（行動不能、物理ダメージ×1.2）
    /// </summary>
    public StatusEffect CreateFreeze(int duration = 4)
    {
        return new StatusEffect(StatusEffectType.Freeze, duration)
        {
            Name = "凍結",
            TurnCostModifier = float.MaxValue  // 行動不能
        };
    }

    /// <summary>
    /// 麻痺状態を生成（攻撃力低下-30%、行動ターンコスト+50%）
    /// </summary>
    public StatusEffect CreateParalysis(int duration = 8)
    {
        return new StatusEffect(StatusEffectType.Paralysis, duration)
        {
            Name = "麻痺",
            AttackMultiplier = 0.7f,       // 攻撃力30%低下
            TurnCostModifier = 1.5f        // 行動ターンコスト50%増加
        };
    }

    /// <summary>
    /// スタン状態を生成（一定ターン行動不可）
    /// </summary>
    public StatusEffect CreateStun(int duration = 3)
    {
        return new StatusEffect(StatusEffectType.Stun, duration)
        {
            Name = "スタン",
            TurnCostModifier = float.MaxValue  // 行動不能
        };
    }

    /// <summary>
    /// 盲目状態を生成（命中率-50%、視界が狭まる）
    /// </summary>
    public StatusEffect CreateBlind(int duration = 10)
    {
        return new StatusEffect(StatusEffectType.Blind, duration)
        {
            Name = "盲目",
            HitRateModifier = -0.50f
        };
    }

    /// <summary>
    /// 沈黙状態を生成（魔法使用不可）
    /// </summary>
    public StatusEffect CreateSilence(int duration = 10)
    {
        return new StatusEffect(StatusEffectType.Silence, duration)
        {
            Name = "沈黙"
        };
    }

    /// <summary>
    /// 混乱状態を生成（行動がランダム）
    /// </summary>
    public StatusEffect CreateConfusion(int duration = 5)
    {
        return new StatusEffect(StatusEffectType.Confusion, duration)
        {
            Name = "混乱"
        };
    }

    /// <summary>
    /// 恐怖状態を生成（敵から逃げようとする）
    /// </summary>
    public StatusEffect CreateFear(int duration = 10)
    {
        return new StatusEffect(StatusEffectType.Fear, duration)
        {
            Name = "恐怖"
        };
    }

    /// <summary>
    /// 睡眠状態を生成（行動不能、被ダメージ×1.5）
    /// </summary>
    public StatusEffect CreateSleep(int duration = 100)
    {
        return new StatusEffect(StatusEffectType.Sleep, duration)
        {
            Name = "睡眠",
            TurnCostModifier = float.MaxValue  // 行動不能
        };
    }

    /// <summary>
    /// 呪い状態を生成（ステータス低下、解除困難）
    /// </summary>
    public StatusEffect CreateCurse()
    {
        return new StatusEffect(StatusEffectType.Curse, int.MaxValue)
        {
            Name = "呪い",
            AllStatsMultiplier = 0.80f  // 全ステータス-20%
        };
    }

    /// <summary>
    /// 衰弱状態を生成（全ステータス-20%）
    /// </summary>
    public StatusEffect CreateWeakness(int duration = 100)
    {
        return new StatusEffect(StatusEffectType.Weakness, duration)
        {
            Name = "衰弱",
            AllStatsMultiplier = 0.80f
        };
    }

    /// <summary>
    /// 魅了状態を生成（敵を攻撃できない、被ダメージで解除）
    /// </summary>
    public StatusEffect CreateCharm(int duration = 5)
    {
        return new StatusEffect(StatusEffectType.Charm, duration)
        {
            Name = "魅了"
        };
    }

    /// <summary>
    /// 狂気状態を生成（敵味方無差別攻撃、正気度に影響）
    /// </summary>
    public StatusEffect CreateMadness(int duration = 15)
    {
        return new StatusEffect(StatusEffectType.Madness, duration)
        {
            Name = "狂気"
        };
    }

    /// <summary>
    /// 石化状態を生成（完全行動不能、防御力大幅上昇）
    /// </summary>
    public StatusEffect CreatePetrification()
    {
        return new StatusEffect(StatusEffectType.Petrification, int.MaxValue)
        {
            Name = "石化",
            TurnCostModifier = float.MaxValue,  // 行動不能
            DefenseMultiplier = 3.0f             // 防御力大幅上昇
        };
    }

    /// <summary>
    /// 即死効果（HPを0にする特殊状態）
    /// </summary>
    public StatusEffect CreateInstantDeath()
    {
        return new StatusEffect(StatusEffectType.InstantDeath, 1)
        {
            Name = "即死",
            DamagePerTick = int.MaxValue,
            DamageElement = Element.Dark
        };
    }

    #endregion

    #region バフ生成

    /// <summary>
    /// 加速状態を生成
    /// </summary>
    public StatusEffect CreateHaste(int duration = 20)
    {
        return new StatusEffect(StatusEffectType.Haste, duration)
        {
            Name = "加速",
            TurnCostModifier = 0.75f  // 行動コスト-25%
        };
    }

    /// <summary>
    /// 強化状態を生成（攻撃力上昇）
    /// </summary>
    public StatusEffect CreateStrengthBuff(int duration = 20)
    {
        return new StatusEffect(StatusEffectType.Strength, duration)
        {
            Name = "強化",
            AttackMultiplier = 1.25f
        };
    }

    /// <summary>
    /// 防護状態を生成（防御力上昇）
    /// </summary>
    public StatusEffect CreateProtection(int duration = 20)
    {
        return new StatusEffect(StatusEffectType.Protection, duration)
        {
            Name = "防護",
            DefenseMultiplier = 1.50f
        };
    }

    /// <summary>
    /// 再生状態を生成（HP徐々に回復）
    /// </summary>
    public StatusEffect CreateRegeneration(int healPerTick = 3, int duration = 30)
    {
        return new StatusEffect(StatusEffectType.Regeneration, duration)
        {
            Name = "再生",
            DamagePerTick = -healPerTick  // 負のダメージ = 回復
        };
    }

    #endregion

    #region 耐性判定

    /// <summary>
    /// 状態異常耐性を判定
    /// 耐性判定 = 基礎耐性 + (関連ステータス × 係数) + 装備補正
    /// </summary>
    public bool CheckResistance(StatusResistanceParams param)
    {
        // 基礎耐性
        double baseResistance = param.BaseResistance;

        // ステータスによる耐性
        double statBonus = param.EffectType switch
        {
            StatusEffectType.Poison => param.Vitality * 0.015,
            StatusEffectType.Paralysis => param.Vitality * 0.01,
            StatusEffectType.Stun => param.Vitality * 0.01,
            StatusEffectType.Burn => param.Vitality * 0.005,
            StatusEffectType.Freeze => param.Vitality * 0.005,
            StatusEffectType.Confusion => param.Mind * 0.02,
            StatusEffectType.Fear => param.Mind * 0.02,
            StatusEffectType.Sleep => param.Mind * 0.02,
            StatusEffectType.Silence => param.Mind * 0.02,
            StatusEffectType.Charm => param.Mind * 0.02,
            StatusEffectType.Madness => param.Mind * 0.02,
            StatusEffectType.Petrification => param.Vitality * 0.01,
            _ => param.Mind * 0.01
        };

        // 最終耐性
        double totalResistance = baseResistance + statBonus + param.EquipmentResistance;
        totalResistance = Math.Clamp(totalResistance, 0, 0.95);  // 最大95%

        // 判定
        return _random.NextDouble() < totalResistance;
    }

    /// <summary>
    /// 即死耐性を判定
    /// </summary>
    public bool CheckInstantDeathResistance(int luck, double baseResistance = 0.10)
    {
        double resistance = baseResistance + (luck * 0.01);
        resistance = Math.Clamp(resistance, 0, 0.80);  // 最大80%
        return _random.NextDouble() < resistance;
    }

    #endregion

    #region 状態異常効果判定

    /// <summary>
    /// 麻痺による行動失敗を判定（レガシー: 現在は攻撃力低下+ターンコスト増加で実装）
    /// 後方互換性のため残存。新規コードではStatusEffect.AttackMultiplier/TurnCostModifierを使用。
    /// </summary>
    public bool CheckParalysisActionFail()
    {
        return _random.NextDouble() < 0.50;
    }

    /// <summary>
    /// 混乱による行動決定
    /// </summary>
    public ConfusedAction GetConfusedAction()
    {
        int roll = _random.Next(100);
        return roll switch
        {
            < 30 => ConfusedAction.AttackAlly,
            < 50 => ConfusedAction.AttackSelf,
            < 70 => ConfusedAction.MoveRandom,
            < 85 => ConfusedAction.DoNothing,
            _ => ConfusedAction.ActNormally
        };
    }

    /// <summary>
    /// 睡眠からの覚醒判定（ダメージで覚醒）
    /// </summary>
    public bool CheckWakeUp(int damageTaken)
    {
        // ダメージを受けたら必ず覚醒
        return damageTaken > 0;
    }

    /// <summary>
    /// 凍結の解除判定（火属性ダメージで解除）
    /// </summary>
    public bool CheckFreezeBreak(Element damageElement)
    {
        return damageElement == Element.Fire;
    }

    /// <summary>
    /// 魅了からの解除判定（被ダメージで解除）
    /// </summary>
    public bool CheckCharmBreak(int damageTaken)
    {
        return damageTaken > 0;
    }

    /// <summary>
    /// 狂気時の行動決定
    /// </summary>
    public MadnessAction GetMadnessAction()
    {
        int roll = _random.Next(100);
        return roll switch
        {
            < 40 => MadnessAction.AttackNearest,   // 最も近い対象を攻撃（敵味方問わず）
            < 60 => MadnessAction.AttackSelf,       // 自分を攻撃
            < 80 => MadnessAction.MoveRandom,       // ランダム移動
            _ => MadnessAction.ActNormally           // 正常行動
        };
    }

    #endregion
}

#region パラメータ構造体

/// <summary>
/// 状態異常耐性判定パラメータ
/// </summary>
public record struct StatusResistanceParams
{
    public StatusEffectType EffectType { get; init; }
    public double BaseResistance { get; init; }
    public int Vitality { get; init; }
    public int Mind { get; init; }
    public int Luck { get; init; }
    public double EquipmentResistance { get; init; }
}

#endregion

#region 列挙型

/// <summary>
/// 混乱時の行動
/// </summary>
public enum ConfusedAction
{
    /// <summary>味方を攻撃</summary>
    AttackAlly,
    /// <summary>自分を攻撃</summary>
    AttackSelf,
    /// <summary>ランダム移動</summary>
    MoveRandom,
    /// <summary>何もしない</summary>
    DoNothing,
    /// <summary>正常に行動</summary>
    ActNormally
}

/// <summary>
/// 狂気時の行動
/// </summary>
public enum MadnessAction
{
    /// <summary>最も近い対象を攻撃（敵味方問わず）</summary>
    AttackNearest,
    /// <summary>自分を攻撃</summary>
    AttackSelf,
    /// <summary>ランダム移動</summary>
    MoveRandom,
    /// <summary>正常に行動</summary>
    ActNormally
}

#endregion

/// <summary>
/// 戦闘用ステータス修正（倍率ベース）
/// Core.StatModifierとは異なり、状態異常による倍率修正に使用
/// </summary>
public record CombatStatModifier
{
    public float PhysicalAttackMultiplier { get; init; } = 1.0f;
    public float MagicalAttackMultiplier { get; init; } = 1.0f;
    public float DefenseMultiplier { get; init; } = 1.0f;
    public float MagicDefenseMultiplier { get; init; } = 1.0f;
    public float HitRateModifier { get; init; } = 0f;
    public float EvasionRateModifier { get; init; } = 0f;
    public float AllStatsMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// 修正を適用した値を取得
    /// </summary>
    public int ApplyToAttack(int baseValue) => (int)(baseValue * PhysicalAttackMultiplier * AllStatsMultiplier);
    public int ApplyToMagicAttack(int baseValue) => (int)(baseValue * MagicalAttackMultiplier * AllStatsMultiplier);
    public int ApplyToDefense(int baseValue) => (int)(baseValue * DefenseMultiplier * AllStatsMultiplier);
    public int ApplyToMagicDefense(int baseValue) => (int)(baseValue * MagicDefenseMultiplier * AllStatsMultiplier);
    public double ApplyToHitRate(double baseValue) => baseValue + HitRateModifier;
    public double ApplyToEvasionRate(double baseValue) => baseValue + EvasionRateModifier;
}
