using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Engine.Combat;

/// <summary>
/// 戦闘システム実装
/// 設計書「1. 戦闘システム概要」に基づく実装
/// </summary>
public class CombatSystem : ICombatSystem
{
    private readonly IRandomProvider _random;
    private readonly DamageCalculator _damageCalculator;
    private readonly StatusEffectSystem _statusEffectSystem;

    public CombatSystem(IRandomProvider random)
    {
        _random = random;
        _damageCalculator = new DamageCalculator(random);
        _statusEffectSystem = new StatusEffectSystem(random);
    }

    /// <summary>
    /// DamageCalculatorへのアクセス
    /// </summary>
    public DamageCalculator DamageCalculator => _damageCalculator;

    /// <summary>
    /// StatusEffectSystemへのアクセス
    /// </summary>
    public StatusEffectSystem StatusEffectSystem => _statusEffectSystem;

    #region 基本攻撃

    public CombatResult ExecuteAttack(IDamageable attacker, IDamageable target, AttackType attackType)
    {
        return ExecuteAttack(attacker, target, attackType, Element.None, Element.None);
    }

    /// <summary>
    /// 属性付き攻撃を実行
    /// </summary>
    public CombatResult ExecuteAttack(IDamageable attacker, IDamageable target, AttackType attackType, 
        Element attackElement, Element targetElement)
    {
        // 命中判定
        var hitResult = CheckHitAdvanced(attacker, target, attackType);
        if (!hitResult.IsHit)
        {
            return CombatResult.Miss(attacker, target);
        }

        // クリティカル判定
        bool isCritical = CheckCritical(attacker);

        // ダメージ計算（属性相性考慮）
        var damage = CalculateDamageAdvanced(attacker, target, attackType, attackElement, targetElement, isCritical);

        // ダメージ適用
        target.TakeDamage(damage);

        return CombatResult.Hit(attacker, target, damage);
    }

    #endregion

    #region 命中判定

    public bool CheckHit(IDamageable attacker, IDamageable target, AttackType attackType)
    {
        return CheckHitAdvanced(attacker, target, attackType).IsHit;
    }

    /// <summary>
    /// 詳細な命中判定（設計書準拠）
    /// </summary>
    public HitCheckResult CheckHitAdvanced(IDamageable attacker, IDamageable target, AttackType attackType)
    {
        var param = BuildHitCheckParams(attacker, target, attackType);
        return _damageCalculator.CheckHit(param);
    }

    private HitCheckParams BuildHitCheckParams(IDamageable attacker, IDamageable target, AttackType attackType)
    {
        int dexterity = 10;
        if (attacker is Core.Entities.Character attackerChar)
        {
            dexterity = attackerChar.EffectiveStats.Dexterity;
        }

        int targetAgility = 10;
        int targetLuck = 10;
        ArmorClass armorClass = ArmorClass.Light;

        if (target is Core.Entities.Character targetChar)
        {
            targetAgility = targetChar.EffectiveStats.Agility;
            targetLuck = targetChar.EffectiveStats.Luck;
        }

        return new HitCheckParams
        {
            AttackType = attackType,
            Dexterity = dexterity,
            HitRateBonus = 0,
            TargetArmorClass = armorClass,
            TargetAgility = targetAgility,
            TargetLuck = targetLuck,
            TargetEvasionBonus = 0
        };
    }

    #endregion

    #region ダメージ計算

    public Damage CalculateDamage(IDamageable attacker, IDamageable target, AttackType attackType, bool isCritical)
    {
        return CalculateDamageAdvanced(attacker, target, attackType, Element.None, Element.None, isCritical);
    }

    /// <summary>
    /// 詳細なダメージ計算（設計書準拠、属性相性考慮）
    /// </summary>
    public Damage CalculateDamageAdvanced(IDamageable attacker, IDamageable target, AttackType attackType,
        Element attackElement, Element targetElement, bool isCritical)
    {
        if (attackType == AttackType.Magic)
        {
            return CalculateMagicalDamage(attacker, target, attackElement, targetElement);
        }
        else
        {
            return CalculatePhysicalDamage(attacker, target, attackElement, targetElement, isCritical);
        }
    }

    private Damage CalculatePhysicalDamage(IDamageable attacker, IDamageable target, 
        Element attackElement, Element targetElement, bool isCritical)
    {
        int weaponAttack = 10;
        int strength = 10;
        int armorDefense = 5;
        int vitality = 10;
        double critRate = GameConstants.BaseCriticalRate;

        if (attacker is Core.Entities.Character attackerChar)
        {
            weaponAttack = attackerChar.EffectiveStats.PhysicalAttack;
            strength = attackerChar.EffectiveStats.Strength;
            critRate = attackerChar.EffectiveStats.CriticalRate;
        }

        if (target is Core.Entities.Character targetChar)
        {
            armorDefense = targetChar.EffectiveStats.PhysicalDefense;
            vitality = targetChar.EffectiveStats.Vitality;
        }

        var param = new PhysicalDamageParams
        {
            WeaponAttack = weaponAttack,
            Strength = strength,
            AttackBuff = 0,
            ArmorDefense = armorDefense,
            Vitality = vitality,
            DefenseBuff = 0,
            SkillMultiplier = 1.0f,
            AttackElement = attackElement,
            TargetElement = targetElement,
            CriticalRate = critRate,
            CriticalDamageMultiplier = 1.5f
        };

        var result = _damageCalculator.CalculatePhysicalDamage(param);

        return new Damage(result.FinalDamage, DamageType.Physical, attackElement, result.IsCritical);
    }

    private Damage CalculateMagicalDamage(IDamageable attacker, IDamageable target,
        Element spellElement, Element targetElement)
    {
        int staffAttack = 10;
        int intelligence = 10;
        int magicDefense = 5;
        int mind = 10;

        if (attacker is Core.Entities.Character attackerChar)
        {
            staffAttack = attackerChar.EffectiveStats.MagicalAttack;
            intelligence = attackerChar.EffectiveStats.Intelligence;
        }

        if (target is Core.Entities.Character targetChar)
        {
            magicDefense = targetChar.EffectiveStats.MagicalDefense;
            mind = targetChar.EffectiveStats.Mind;
        }

        var param = new MagicalDamageParams
        {
            StaffAttack = staffAttack,
            Intelligence = intelligence,
            MagicAttackBuff = 0,
            MagicDefense = magicDefense,
            Mind = mind,
            MagicDefenseBuff = 0,
            SkillMultiplier = 1.0f,
            LanguageBonus = 1.0f,
            SpellElement = spellElement,
            TargetElement = targetElement
        };

        var result = _damageCalculator.CalculateMagicalDamage(param);

        return new Damage(result.FinalDamage, DamageType.Magical, spellElement, false);
    }

    #endregion

    #region クリティカル判定

    private bool CheckCritical(IDamageable attacker)
    {
        double critRate = GameConstants.BaseCriticalRate;
        int dexterity = 10;
        int luck = 10;

        if (attacker is Core.Entities.Character attackerChar)
        {
            critRate = attackerChar.EffectiveStats.CriticalRate;
            dexterity = attackerChar.EffectiveStats.Dexterity;
            luck = attackerChar.EffectiveStats.Luck;
        }

        var param = new CriticalCheckParams
        {
            Dexterity = dexterity,
            Luck = luck,
            WeaponCritBonus = 0,
            SkillCritBonus = 0
        };

        return _damageCalculator.CheckCritical(param);
    }

    #endregion

    #region 状態異常

    /// <summary>
    /// 状態異常を付与（耐性判定付き）
    /// </summary>
    public bool TryApplyStatusEffect(IDamageable target, StatusEffectType effectType)
    {
        int vitality = 10;
        int mind = 10;
        int luck = 10;

        if (target is Core.Entities.Character targetChar)
        {
            vitality = targetChar.EffectiveStats.Vitality;
            mind = targetChar.EffectiveStats.Mind;
            luck = targetChar.EffectiveStats.Luck;
        }

        var resistanceParam = new StatusResistanceParams
        {
            EffectType = effectType,
            BaseResistance = 0.10,
            Vitality = vitality,
            Mind = mind,
            Luck = luck,
            EquipmentResistance = 0
        };

        // 耐性判定（成功したら状態異常を防ぐ）
        if (_statusEffectSystem.CheckResistance(resistanceParam))
        {
            return false;
        }

        // 状態異常を適用（実際の適用はCharacter側で行う）
        return true;
    }

    /// <summary>
    /// 状態異常エフェクトを生成
    /// </summary>
    public StatusEffect? CreateStatusEffect(StatusEffectType effectType, int maxHp = 100)
    {
        return effectType switch
        {
            StatusEffectType.Poison => _statusEffectSystem.CreatePoison(maxHp),
            StatusEffectType.Bleeding => _statusEffectSystem.CreateBleeding(),
            StatusEffectType.Burn => _statusEffectSystem.CreateBurn(),
            StatusEffectType.Freeze => _statusEffectSystem.CreateFreeze(),
            StatusEffectType.Paralysis => _statusEffectSystem.CreateParalysis(),
            StatusEffectType.Blind => _statusEffectSystem.CreateBlind(),
            StatusEffectType.Silence => _statusEffectSystem.CreateSilence(),
            StatusEffectType.Confusion => _statusEffectSystem.CreateConfusion(),
            StatusEffectType.Fear => _statusEffectSystem.CreateFear(),
            StatusEffectType.Sleep => _statusEffectSystem.CreateSleep(),
            StatusEffectType.Curse => _statusEffectSystem.CreateCurse(),
            StatusEffectType.Weakness => _statusEffectSystem.CreateWeakness(),
            StatusEffectType.Haste => _statusEffectSystem.CreateHaste(),
            StatusEffectType.Strength => _statusEffectSystem.CreateStrengthBuff(),
            StatusEffectType.Protection => _statusEffectSystem.CreateProtection(),
            StatusEffectType.Regeneration => _statusEffectSystem.CreateRegeneration(),
            _ => null
        };
    }

    #endregion

    #region HP状態判定

    /// <summary>
    /// HP状態によるペナルティを取得
    /// </summary>
    public HpStatePenalty GetHpStatePenalty(IDamageable entity)
    {
        return _damageCalculator.GetHpStatePenalty(entity.CurrentHp, entity.MaxHp);
    }

    #endregion
}

/// <summary>
/// 乱数プロバイダー実装
/// </summary>
public class RandomProvider : IRandomProvider
{
    private readonly Random _random;

    public RandomProvider(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public int Next(int maxValue) => _random.Next(maxValue);
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    public double NextDouble() => _random.NextDouble();
}
