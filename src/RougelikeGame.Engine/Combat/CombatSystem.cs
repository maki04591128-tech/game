using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

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
        float attackerHitBonus = 0f;
        float targetEvasionBonus = 0f;

        if (target is Core.Entities.Character targetChar)
        {
            targetAgility = targetChar.EffectiveStats.Agility;
            targetLuck = targetChar.EffectiveStats.Luck;
            // CB-10: 状態異常による回避率修正
            foreach (var eff in targetChar.StatusEffects)
            {
                targetEvasionBonus += eff.EvasionRateModifier;
            }
        }

        // CB-10: 攻撃者の状態異常による命中率修正
        if (attacker is Core.Entities.Character attackerChar2)
        {
            foreach (var eff in attackerChar2.StatusEffects)
            {
                attackerHitBonus += eff.HitRateModifier;
            }
        }

        return new HitCheckParams
        {
            AttackType = attackType,
            Dexterity = dexterity,
            HitRateBonus = attackerHitBonus,
            TargetArmorClass = armorClass,
            TargetAgility = targetAgility,
            TargetLuck = targetLuck,
            TargetEvasionBonus = targetEvasionBonus
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
            // K-2: 魔法攻撃もクリティカル可能に
            return CalculateMagicalDamage(attacker, target, attackElement, targetElement, isCritical);
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
        double attackerRacialBonus = 0.0;
        double targetRacialResistance = 0.0;
        double proficiencyMultiplier = 1.0;
        int attackerLevel = 1;          // BS-8
        bool hasShield = false;          // BS-11
        float shieldBlockChance = 0f;    // BS-11
        float shieldBlockReduction = 0.5f; // BS-11

        if (attacker is Character attackerChar)
        {
            weaponAttack = attackerChar.EffectiveStats.PhysicalAttack;
            strength = attackerChar.EffectiveStats.Strength;
            critRate = attackerChar.EffectiveStats.CriticalRate;

            // 種族特性: 狂戦士の血（HP50%以下で攻撃力ボーナス）
            if (attacker is Player attackerPlayer)
            {
                attackerRacialBonus = RacialTraitSystem.CalculateBerserkerBonus(
                    attackerPlayer.Race, attackerPlayer.CurrentHp, attackerPlayer.MaxHp);
                // 種族特性: 幸運体質（クリティカル率ボーナス）
                critRate += RacialTraitSystem.GetTraitValue(attackerPlayer.Race, RacialTraitType.LuckyBody);
                // BH-2: パッシブスキル由来のクリティカル率ボーナス
                critRate += attackerPlayer.BonusCriticalRate;
                // 装備適性倍率
                var weapon = attackerPlayer.Equipment.MainHand;
                if (weapon != null)
                {
                    proficiencyMultiplier = ClassEquipmentSystem.GetProficiencyMultiplier(
                        attackerPlayer.CharacterClass, weapon.Category);
                    // BS-1: 武器ダメージレンジを物理ダメージに反映
                    if (weapon.DamageRange.Max > 0)
                    {
                        int weaponRangeDmg = _random.Next(weapon.DamageRange.Min, weapon.DamageRange.Max + 1);
                        weaponAttack += weaponRangeDmg;
                    }
                    // BS-9: 武器固有クリティカルボーナスを適用
                    critRate += weapon.CriticalBonus;
                }

                // BS-19: オフハンド武器ダメージ（二刀流時）
                var offHand = attackerPlayer.Equipment.OffHand;
                if (offHand is Weapon offHandWeapon)
                {
                    int offHandDmg = offHandWeapon.BaseDamage / 2; // オフハンドは50%の攻撃力
                    if (offHandWeapon.DamageRange.Max > 0)
                    {
                        offHandDmg += _random.Next(offHandWeapon.DamageRange.Min, offHandWeapon.DamageRange.Max + 1) / 2;
                    }
                    weaponAttack += offHandDmg;
                }

                // BS-8: レベルスケーリング用
                attackerLevel = attackerPlayer.Level;
            }
        }

        if (target is Character targetChar)
        {
            armorDefense = targetChar.EffectiveStats.PhysicalDefense;
            vitality = targetChar.EffectiveStats.Vitality;

            // 種族特性: 物理耐性（スライム等）
            if (target is Player targetPlayer)
            {
                targetRacialResistance = RacialTraitSystem.CalculatePhysicalResistance(targetPlayer.Race);
            }

            // BS-11: 盾ブロック判定
            if (target is Player shieldTarget)
            {
                var shield = shieldTarget.Equipment.OffHand as Shield;
                if (shield != null && shield.BlockChance > 0)
                {
                    hasShield = true;
                    shieldBlockChance = shield.BlockChance;
                    shieldBlockReduction = shield.BlockReduction;
                }
            }
        }

        // BS-5: クリティカル倍率は武器・スキル特化で最大2.5倍に設定可能
        float critDamageMultiplier = 1.5f;
        if (attacker is Player critPlayer)
        {
            // 基本1.5倍 + パッシブスキルや装備による追加（最大2.5倍）
            critDamageMultiplier = Math.Min(2.5f, 1.5f + (float)critPlayer.BonusCriticalRate * 2.0f);
        }

        var param = new PhysicalDamageParams
        {
            WeaponAttack = weaponAttack,
            Strength = strength,
            AttackBuff = (int)(weaponAttack * attackerRacialBonus),
            ArmorDefense = armorDefense,
            Vitality = vitality,
            DefenseBuff = 0,
            SkillMultiplier = (float)proficiencyMultiplier,
            AttackElement = attackElement,
            TargetElement = targetElement,
            CriticalRate = critRate,
            CriticalDamageMultiplier = critDamageMultiplier,
            AttackerLevel = attackerLevel
        };

        var result = _damageCalculator.CalculatePhysicalDamage(param);

        // 種族特性: 物理耐性を最終ダメージに適用
        int finalDamage = result.FinalDamage;
        if (targetRacialResistance > 0)
        {
            finalDamage = (int)(finalDamage * (1.0 - targetRacialResistance));
            finalDamage = Math.Max(GameConstants.MinimumDamage, finalDamage);
        }

        // BS-11: 盾ブロック適用
        if (hasShield)
        {
            var (blocked, reducedDmg) = _damageCalculator.CalculateShieldBlock(finalDamage, shieldBlockChance, shieldBlockReduction);
            if (blocked) finalDamage = reducedDmg;
        }

        return new Damage(finalDamage, DamageType.Physical, attackElement, result.IsCritical);
    }

    private Damage CalculateMagicalDamage(IDamageable attacker, IDamageable target,
        Element spellElement, Element targetElement, bool isCritical = false)
    {
        int staffAttack = 10;
        int intelligence = 10;
        int magicDefense = 5;
        int mind = 10;
        double magicDamageMultiplier = 1.0;

        if (attacker is Character attackerChar)
        {
            staffAttack = attackerChar.EffectiveStats.MagicalAttack;
            intelligence = attackerChar.EffectiveStats.Intelligence;

            // 種族特性: 魔法ダメージボーナス（エルフ等）
            if (attacker is Player attackerPlayer)
            {
                magicDamageMultiplier = RacialTraitSystem.CalculateMagicDamageMultiplier(attackerPlayer.Race);
            }
        }

        if (target is Character targetChar)
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
            LanguageBonus = (float)magicDamageMultiplier,
            SpellElement = spellElement,
            TargetElement = targetElement
        };

        var result = _damageCalculator.CalculateMagicalDamage(param);

        // 種族特性: 魔力吸収（悪魔等 - 魔法被弾時MP回復）
        if (target is Player targetPlayer)
        {
            double absorbRate = RacialTraitSystem.GetTraitValue(targetPlayer.Race, RacialTraitType.ManaAbsorption);
            if (absorbRate > 0)
            {
                int mpRecover = (int)(result.FinalDamage * absorbRate);
                if (mpRecover > 0) targetPlayer.RestoreMp(mpRecover);
            }
        }

        // K-2: 魔法クリティカル対応（1.3倍）
        int finalDmg = isCritical ? (int)(result.FinalDamage * 1.3) : result.FinalDamage;

        return new Damage(finalDmg, DamageType.Magical, spellElement, isCritical);
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

        // BS-9: 武器固有クリティカルボーナスを取得
        double weaponCritBonus = 0;
        if (attacker is Core.Entities.Player attackerPlayer && attackerPlayer.Equipment.MainHand != null)
        {
            weaponCritBonus = attackerPlayer.Equipment.MainHand.CriticalBonus;
        }

        var param = new CriticalCheckParams
        {
            Dexterity = dexterity,
            Luck = luck,
            WeaponCritBonus = weaponCritBonus,
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
        // CB-5: ボス/種族免疫チェック
        if (target is Core.Entities.Enemy enemy &&
            Core.Systems.MonsterRaceSystem.IsStatusEffectImmune(enemy.Race, effectType))
        {
            return false;
        }

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

        // 状態異常を適用（CP-7: 実際にターゲットに適用する）
        var effect = CreateStatusEffect(effectType, target is Core.Entities.Character c ? c.MaxHp : 100);
        if (effect != null && target is Core.Entities.Character targetCharApply)
        {
            targetCharApply.ApplyStatusEffect(effect);
        }
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
            StatusEffectType.Stun => _statusEffectSystem.CreateStun(),
            StatusEffectType.Blind => _statusEffectSystem.CreateBlind(),
            StatusEffectType.Silence => _statusEffectSystem.CreateSilence(),
            StatusEffectType.Confusion => _statusEffectSystem.CreateConfusion(),
            StatusEffectType.Fear => _statusEffectSystem.CreateFear(),
            StatusEffectType.Sleep => _statusEffectSystem.CreateSleep(),
            StatusEffectType.Curse => _statusEffectSystem.CreateCurse(),
            StatusEffectType.Weakness => _statusEffectSystem.CreateWeakness(),
            StatusEffectType.Charm => _statusEffectSystem.CreateCharm(),
            StatusEffectType.Madness => _statusEffectSystem.CreateMadness(),
            StatusEffectType.Petrification => _statusEffectSystem.CreatePetrification(),
            StatusEffectType.InstantDeath => _statusEffectSystem.CreateInstantDeath(),
            StatusEffectType.Haste => _statusEffectSystem.CreateHaste(),
            StatusEffectType.Strength => _statusEffectSystem.CreateStrengthBuff(),
            StatusEffectType.Protection => _statusEffectSystem.CreateProtection(),
            StatusEffectType.Regeneration => _statusEffectSystem.CreateRegeneration(),
            StatusEffectType.Slow => _statusEffectSystem.CreateSlow(),
            StatusEffectType.Vulnerability => _statusEffectSystem.CreateVulnerability(),
            StatusEffectType.Invisibility => _statusEffectSystem.CreateInvisibility(),
            StatusEffectType.Blessing => _statusEffectSystem.CreateBlessing(),
            StatusEffectType.Apostasy => _statusEffectSystem.CreateApostasy(),
            StatusEffectType.FireResistance => _statusEffectSystem.CreateFireResistance(),
            StatusEffectType.ColdResistance => _statusEffectSystem.CreateColdResistance(),
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
