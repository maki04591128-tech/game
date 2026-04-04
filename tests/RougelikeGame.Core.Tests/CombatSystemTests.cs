using RougelikeGame.Core;
using RougelikeGame.Engine.Combat;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 戦闘システムテスト
/// </summary>
public class CombatSystemTests
{
    private readonly TestRandomProvider _fixedRandom = new(0.5);  // 固定乱数

    #region ElementSystem Tests

    [Theory]
    [InlineData(Element.Fire, Element.Ice, 1.5f)]     // 火→氷は有利
    [InlineData(Element.Fire, Element.Water, 0.5f)]   // 火→水は不利
    [InlineData(Element.Water, Element.Fire, 1.5f)]   // 水→火は有利
    [InlineData(Element.Ice, Element.Fire, 0.5f)]     // 氷→火は不利
    [InlineData(Element.Lightning, Element.Water, 1.5f)] // 雷→水は有利
    [InlineData(Element.Lightning, Element.Earth, 0.5f)] // 雷→地は不利
    [InlineData(Element.Light, Element.Dark, 1.5f)]   // 光→闇は有利
    [InlineData(Element.Dark, Element.Light, 1.5f)]   // 闇→光は有利
    [InlineData(Element.None, Element.Fire, 1.0f)]    // 無→火は通常
    [InlineData(Element.Fire, Element.Fire, 0.5f)]    // 同属性は不利
    public void GetAffinityMultiplier_ReturnsCorrectValue(Element attack, Element target, float expected)
    {
        var result = ElementSystem.GetAffinityMultiplier(attack, target);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAffinityType_Advantage_WhenFireVsIce()
    {
        var result = ElementSystem.GetAffinityType(Element.Fire, Element.Ice);
        Assert.Equal(ElementAffinity.Advantage, result);
    }

    [Fact]
    public void GetAffinityType_Disadvantage_WhenFireVsWater()
    {
        var result = ElementSystem.GetAffinityType(Element.Fire, Element.Water);
        Assert.Equal(ElementAffinity.Disadvantage, result);
    }

    [Fact]
    public void HasWeakness_ReturnsTrue_WhenIceAttackedByFire()
    {
        Assert.True(ElementSystem.HasWeakness(Element.Ice, Element.Fire));
    }

    [Fact]
    public void HasResistance_ReturnsTrue_WhenFireAttackedByFire()
    {
        Assert.True(ElementSystem.HasResistance(Element.Fire, Element.Fire));
    }

    #endregion

    #region DamageCalculator Tests

    [Fact]
    public void CalculatePhysicalDamage_ReturnsPositiveDamage()
    {
        var calculator = new DamageCalculator(_fixedRandom);
        var param = new PhysicalDamageParams
        {
            WeaponAttack = 20,
            Strength = 15,
            ArmorDefense = 10,
            Vitality = 10,
            SkillMultiplier = 1.0f,
            CriticalRate = 0,
            CriticalDamageMultiplier = 1.5f
        };

        var result = calculator.CalculatePhysicalDamage(param);

        Assert.True(result.FinalDamage > 0);
        Assert.Equal(DamageType.Physical, result.DamageType);
    }

    [Fact]
    public void CalculatePhysicalDamage_AppliesElementAffinity()
    {
        var calculator = new DamageCalculator(_fixedRandom);
        var normalParam = new PhysicalDamageParams
        {
            WeaponAttack = 20,
            Strength = 15,
            ArmorDefense = 5,
            Vitality = 5,
            SkillMultiplier = 1.0f,
            AttackElement = Element.None,
            TargetElement = Element.None,
            CriticalRate = 0
        };

        var fireVsIceParam = normalParam with
        {
            AttackElement = Element.Fire,
            TargetElement = Element.Ice
        };

        var normalResult = calculator.CalculatePhysicalDamage(normalParam);
        var advantageResult = calculator.CalculatePhysicalDamage(fireVsIceParam);

        // 有利属性でダメージ増加
        Assert.True(advantageResult.FinalDamage > normalResult.FinalDamage);
        Assert.Equal(ElementAffinity.Advantage, advantageResult.ElementAffinity);
    }

    [Fact]
    public void CalculateMagicalDamage_ReturnsPositiveDamage()
    {
        var calculator = new DamageCalculator(_fixedRandom);
        var param = new MagicalDamageParams
        {
            StaffAttack = 15,
            Intelligence = 20,
            MagicDefense = 5,
            Mind = 10,
            SkillMultiplier = 1.0f,
            LanguageBonus = 1.0f
        };

        var result = calculator.CalculateMagicalDamage(param);

        Assert.True(result.FinalDamage > 0);
        Assert.Equal(DamageType.Magical, result.DamageType);
        Assert.False(result.IsCritical);  // 魔法はクリティカルなし
    }

    [Fact]
    public void CheckHit_MagicAlwaysHits()
    {
        var calculator = new DamageCalculator(new TestRandomProvider(0.99));  // 高い乱数
        var param = new HitCheckParams
        {
            AttackType = AttackType.Magic,
            Dexterity = 5,
            TargetArmorClass = ArmorClass.None,
            TargetAgility = 50  // 高い敏捷
        };

        var result = calculator.CheckHit(param);

        Assert.True(result.IsHit);
        Assert.Equal(1.0, result.HitRate);
    }

    [Fact]
    public void CheckHit_RangedHasLowerBaseRate()
    {
        var calculator = new DamageCalculator(_fixedRandom);
        var meleeParam = new HitCheckParams
        {
            AttackType = AttackType.Slash,  // 近接
            Dexterity = 10,
            TargetArmorClass = ArmorClass.Light,
            TargetAgility = 10
        };
        var rangedParam = meleeParam with { AttackType = AttackType.Ranged };

        var meleeResult = calculator.CheckHit(meleeParam);
        var rangedResult = calculator.CheckHit(rangedParam);

        // 遠距離は基礎命中率が低い
        Assert.True(meleeResult.HitRate > rangedResult.HitRate);
    }

    [Fact]
    public void GetHpStatePenalty_ReturnsCorrectState()
    {
        var calculator = new DamageCalculator(_fixedRandom);

        Assert.Equal(HpState.Healthy, calculator.GetHpStatePenalty(100, 100).State);
        Assert.Equal(HpState.Injured, calculator.GetHpStatePenalty(60, 100).State);
        Assert.Equal(HpState.Wounded, calculator.GetHpStatePenalty(40, 100).State);
        Assert.Equal(HpState.Critical, calculator.GetHpStatePenalty(10, 100).State);
        Assert.Equal(HpState.Dead, calculator.GetHpStatePenalty(0, 100).State);
    }

    #endregion

    #region StatusEffectSystem Tests

    [Fact]
    public void CreatePoison_SetsCorrectDamage()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var poison = system.CreatePoison(maxHp: 100, duration: 20);

        Assert.Equal(StatusEffectType.Poison, poison.Type);
        Assert.Equal(20, poison.Duration);
        Assert.Equal(2, poison.DamagePerTick);  // 100 * 0.02 = 2
        Assert.Equal(Element.Poison, poison.DamageElement);
    }

    [Fact]
    public void CreateDeadlyPoison_HasHigherDamage()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var poison = system.CreatePoison(maxHp: 100);
        var deadlyPoison = system.CreateDeadlyPoison(maxHp: 100);

        Assert.True(deadlyPoison.DamagePerTick > poison.DamagePerTick);
    }

    [Fact]
    public void CreateFreeze_SetsActionDisabled()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var freeze = system.CreateFreeze();

        Assert.Equal(StatusEffectType.Freeze, freeze.Type);
        Assert.Equal(float.MaxValue, freeze.TurnCostModifier);  // 行動不能
    }

    [Fact]
    public void CreateBurn_ReducesAttack()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var burn = system.CreateBurn();

        Assert.Equal(StatusEffectType.Burn, burn.Type);
        Assert.Equal(0.85f, burn.AttackMultiplier);
    }

    [Fact]
    public void CreateStrengthBuff_IncreasesAttack()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var strength = system.CreateStrengthBuff();

        Assert.Equal(StatusEffectType.Strength, strength.Type);
        Assert.Equal(1.25f, strength.AttackMultiplier);
    }

    [Fact]
    public void CreateProtection_IncreasesDefense()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var protection = system.CreateProtection();

        Assert.Equal(StatusEffectType.Protection, protection.Type);
        Assert.Equal(1.50f, protection.DefenseMultiplier);
    }

    [Fact]
    public void CheckResistance_HighVitalityResistsPoison()
    {
        var system = new StatusEffectSystem(new TestRandomProvider(0.1));  // 低い乱数=耐性成功しやすい
        var param = new StatusResistanceParams
        {
            EffectType = StatusEffectType.Poison,
            BaseResistance = 0.10,
            Vitality = 50,  // 高いVIT
            Mind = 10,
            EquipmentResistance = 0
        };

        // VIT 50 × 0.015 = 0.75 + 0.10 = 0.85 (85%耐性)
        var result = system.CheckResistance(param);
        Assert.True(result);  // 高耐性で成功
    }

    [Fact]
    public void GetConfusedAction_ReturnsValidAction()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var action = system.GetConfusedAction();

        Assert.True(Enum.IsDefined(typeof(ConfusedAction), action));
    }

    [Fact]
    public void CheckFreezeBreak_ReturnsTrueForFire()
    {
        var system = new StatusEffectSystem(_fixedRandom);

        Assert.True(system.CheckFreezeBreak(Element.Fire));
        Assert.False(system.CheckFreezeBreak(Element.Water));
        Assert.False(system.CheckFreezeBreak(Element.Ice));
    }

    [Fact]
    public void CreateCharm_SetsCorrectProperties()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var charm = system.CreateCharm();

        Assert.Equal(StatusEffectType.Charm, charm.Type);
        Assert.Equal("魅了", charm.Name);
        Assert.Equal(5, charm.Duration);
    }

    [Fact]
    public void CreateMadness_SetsCorrectProperties()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var madness = system.CreateMadness();

        Assert.Equal(StatusEffectType.Madness, madness.Type);
        Assert.Equal("狂気", madness.Name);
        Assert.Equal(15, madness.Duration);
    }

    [Fact]
    public void CreatePetrification_SetsActionDisabledAndHighDefense()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var petrification = system.CreatePetrification();

        Assert.Equal(StatusEffectType.Petrification, petrification.Type);
        Assert.Equal("石化", petrification.Name);
        Assert.Equal(999f, petrification.TurnCostModifier);  // EO-2: 行動不能（オーバーフロー回避）
        Assert.Equal(3.0f, petrification.DefenseMultiplier);  // 防御力大幅上昇
        Assert.Equal(50, petrification.Duration);  // EO-2: 有限化（50ターン）
    }

    [Fact]
    public void CreateInstantDeath_HasMaxDamage()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var instantDeath = system.CreateInstantDeath();

        Assert.Equal(StatusEffectType.InstantDeath, instantDeath.Type);
        Assert.Equal("即死", instantDeath.Name);
        Assert.Equal(999999, instantDeath.DamagePerTick);  // EO-3: オーバーフロー回避
        Assert.Equal(Element.Dark, instantDeath.DamageElement);
    }

    [Fact]
    public void CheckCharmBreak_ReturnsTrueOnDamage()
    {
        var system = new StatusEffectSystem(_fixedRandom);

        Assert.True(system.CheckCharmBreak(1));
        Assert.True(system.CheckCharmBreak(100));
        Assert.False(system.CheckCharmBreak(0));
    }

    [Fact]
    public void GetMadnessAction_ReturnsValidAction()
    {
        var system = new StatusEffectSystem(_fixedRandom);
        var action = system.GetMadnessAction();

        Assert.True(Enum.IsDefined(typeof(MadnessAction), action));
    }

    [Fact]
    public void CheckResistance_HighMindResistsCharm()
    {
        var system = new StatusEffectSystem(new TestRandomProvider(0.1));
        var param = new StatusResistanceParams
        {
            EffectType = StatusEffectType.Charm,
            BaseResistance = 0.10,
            Vitality = 10,
            Mind = 40,  // 高いMND
            EquipmentResistance = 0
        };

        // MND 40 × 0.02 = 0.80 + 0.10 = 0.90 (90%耐性)
        var result = system.CheckResistance(param);
        Assert.True(result);
    }

    [Fact]
    public void CheckResistance_VitalityResistsPetrification()
    {
        var system = new StatusEffectSystem(new TestRandomProvider(0.1));
        var param = new StatusResistanceParams
        {
            EffectType = StatusEffectType.Petrification,
            BaseResistance = 0.10,
            Vitality = 50,  // 高いVIT
            Mind = 10,
            EquipmentResistance = 0
        };

        // VIT 50 × 0.01 = 0.50 + 0.10 = 0.60 (60%耐性)
        var result = system.CheckResistance(param);
        Assert.True(result);
    }

    [Fact]
    public void CombatSystem_CreateStatusEffect_ReturnsCharm()
    {
        var combatSystem = new CombatSystem(new TestRandomProvider(0.5));

        var effect = combatSystem.CreateStatusEffect(StatusEffectType.Charm);
        Assert.NotNull(effect);
        Assert.Equal(StatusEffectType.Charm, effect!.Type);
    }

    [Fact]
    public void CombatSystem_CreateStatusEffect_ReturnsMadness()
    {
        var combatSystem = new CombatSystem(new TestRandomProvider(0.5));

        var effect = combatSystem.CreateStatusEffect(StatusEffectType.Madness);
        Assert.NotNull(effect);
        Assert.Equal(StatusEffectType.Madness, effect!.Type);
    }

    [Fact]
    public void CombatSystem_CreateStatusEffect_ReturnsPetrification()
    {
        var combatSystem = new CombatSystem(new TestRandomProvider(0.5));

        var effect = combatSystem.CreateStatusEffect(StatusEffectType.Petrification);
        Assert.NotNull(effect);
        Assert.Equal(StatusEffectType.Petrification, effect!.Type);
    }

    [Fact]
    public void CombatSystem_CreateStatusEffect_ReturnsInstantDeath()
    {
        var combatSystem = new CombatSystem(new TestRandomProvider(0.5));

        var effect = combatSystem.CreateStatusEffect(StatusEffectType.InstantDeath);
        Assert.NotNull(effect);
        Assert.Equal(StatusEffectType.InstantDeath, effect!.Type);
    }

    #endregion

    #region ResourceSystem Tests

    [Fact]
    public void CalculateMaxHp_ReturnsCorrectValue()
    {
        var system = new ResourceSystem();
        var param = new HpCalculationParams
        {
            Vitality = 20,
            Level = 5,
            RaceBonus = 10,
            CharacterClass = RougelikeGame.Engine.Combat.CharacterClass.Warrior
        };

        // 50 + (20×10) + (5×5) + 10 + (15×4) = 50 + 200 + 25 + 10 + 60 = 345
        var result = system.CalculateMaxHp(param);
        Assert.Equal(345, result);
    }

    [Fact]
    public void CalculateMaxMp_ReturnsCorrectValue()
    {
        var system = new ResourceSystem();
        var param = new MpCalculationParams
        {
            Mind = 15,
            Intelligence = 20,
            Level = 5,
            RaceBonus = 5,
            CharacterClass = RougelikeGame.Engine.Combat.CharacterClass.Mage
        };

        // 20 + (15×5) + (20×2) + (5×2) + 5 + (8×4) = 20 + 75 + 40 + 10 + 5 + 32 = 182
        var result = system.CalculateMaxMp(param);
        Assert.Equal(182, result);
    }

    [Fact]
    public void GetHpState_ReturnsCorrectState()
    {
        var system = new ResourceSystem();

        Assert.Equal(HpState.Healthy, system.GetHpState(80, 100));
        Assert.Equal(HpState.Injured, system.GetHpState(60, 100));
        Assert.Equal(HpState.Wounded, system.GetHpState(30, 100));
        Assert.Equal(HpState.Critical, system.GetHpState(10, 100));
        Assert.Equal(HpState.Dead, system.GetHpState(0, 100));
    }

    [Fact]
    public void GetStaminaCost_HeavyArmorIncreasesBy20Percent()
    {
        var system = new ResourceSystem();

        var normalCost = system.GetStaminaCost(StaminaAction.NormalAttack, isHeavyArmor: false);
        var heavyCost = system.GetStaminaCost(StaminaAction.NormalAttack, isHeavyArmor: true);

        Assert.Equal(5, normalCost);
        Assert.Equal(6, heavyCost);  // 5 × 1.2 = 6
    }

    [Fact]
    public void GetHungerState_ReturnsCorrectState()
    {
        var system = new ResourceSystem();

        Assert.Equal(HungerState.Satiated, system.GetHungerState(90));
        Assert.Equal(HungerState.Normal, system.GetHungerState(60));
        Assert.Equal(HungerState.Hungry, system.GetHungerState(30));
        Assert.Equal(HungerState.Starving, system.GetHungerState(15));
        Assert.Equal(HungerState.Famished, system.GetHungerState(5));
        Assert.Equal(HungerState.Starvation, system.GetHungerState(0));
    }

    [Fact]
    public void GetHungerEffect_SatiatedIncreasesRecovery()
    {
        var system = new ResourceSystem();
        var effect = system.GetHungerEffect(90);

        Assert.Equal(1.2f, effect.StaminaRecoveryModifier);
        Assert.True(effect.AllowHpRecovery);
        Assert.True(effect.AllowSpRecovery);
    }

    [Fact]
    public void GetHungerEffect_StarvingStopsRecovery()
    {
        var system = new ResourceSystem();
        var effect = system.GetHungerEffect(15);

        Assert.False(effect.AllowHpRecovery);
        Assert.False(effect.AllowSpRecovery);
    }

    [Fact]
    public void CalculateRequiredExp_IncreasesWithLevel()
    {
        var system = new ResourceSystem();

        var level1 = system.CalculateRequiredExp(1);
        var level2 = system.CalculateRequiredExp(2);
        var level10 = system.CalculateRequiredExp(10);

        Assert.True(level2 > level1);
        Assert.True(level10 > level2);
    }

    [Fact]
    public void CalculateExpGain_ScalesWithLevelDifference()
    {
        var system = new ResourceSystem();

        var param = new ExpGainParams
        {
            BaseExp = 100,
            EnemyLevel = 10,
            PlayerLevel = 10,
            DifficultyModifier = 1.0f,
            BackgroundModifier = 1.0f
        };

        var evenExp = system.CalculateExpGain(param);
        var highEnemyExp = system.CalculateExpGain(param with { EnemyLevel = 15 });
        var lowEnemyExp = system.CalculateExpGain(param with { EnemyLevel = 5 });

        Assert.Equal(100, evenExp);  // 同レベル
        Assert.True(highEnemyExp > evenExp);  // 高レベル敵
        Assert.True(lowEnemyExp < evenExp);   // 低レベル敵
    }

    #endregion
}

/// <summary>
/// テスト用固定乱数プロバイダー
/// </summary>
public class TestRandomProvider : Core.Interfaces.IRandomProvider
{
    private readonly double _fixedValue;
    private int _nextIntValue;

    public TestRandomProvider(double fixedValue, int nextIntValue = 50)
    {
        _fixedValue = fixedValue;
        _nextIntValue = nextIntValue;
    }

    public int Next(int maxValue) => Math.Min(_nextIntValue, maxValue - 1);
    public int Next(int minValue, int maxValue) => Math.Min(Math.Max(_nextIntValue, minValue), maxValue - 1);
    public double NextDouble() => _fixedValue;
}
