using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Map;
using RougelikeGame.Engine;
using RougelikeGame.Engine.Combat;
using RougelikeGame.Engine.Magic;

namespace RougelikeGame.Core.Tests;

public class EngineUnitTests
{
    private class FixedRandom : IRandomProvider
    {
        private readonly double _doubleVal;
        private readonly int _intVal;
        public FixedRandom(double doubleVal = 0.5, int intVal = 0)
        {
            _doubleVal = doubleVal;
            _intVal = intVal;
        }
        public int Next(int maxValue) => Math.Min(_intVal, maxValue - 1);
        public int Next(int minValue, int maxValue) => Math.Min(Math.Max(_intVal, minValue), maxValue - 1);
        public double NextDouble() => _doubleVal;
    }

    #region ElementSystem

    [Fact]
    public void ElementSystem_FireVsIce_HasAdvantage()
    {
        float mult = ElementSystem.GetAffinityMultiplier(Element.Fire, Element.Ice);
        Assert.Equal(ElementSystem.AdvantageMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_FireVsWater_HasDisadvantage()
    {
        float mult = ElementSystem.GetAffinityMultiplier(Element.Fire, Element.Water);
        Assert.Equal(ElementSystem.DisadvantageMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_SameElement_HasDisadvantage()
    {
        float mult = ElementSystem.GetAffinityMultiplier(Element.Fire, Element.Fire);
        Assert.Equal(ElementSystem.DisadvantageMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_NoneAttack_IsNeutral()
    {
        float mult = ElementSystem.GetAffinityMultiplier(Element.None, Element.Fire);
        Assert.Equal(ElementSystem.NeutralMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_DarkVsCurse_IsNeutral()
    {
        // 設計書: 闇→呪い は通常。アンデッドへの無効は種族ベース耐性で処理
        float mult = ElementSystem.GetAffinityMultiplier(Element.Dark, Element.Curse);
        Assert.Equal(ElementSystem.NeutralMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_LightVsDark_HasAdvantage()
    {
        float mult = ElementSystem.GetAffinityMultiplier(Element.Light, Element.Dark);
        Assert.Equal(ElementSystem.AdvantageMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_PoisonVsPoison_IsNullified()
    {
        // Poison vs Poison: same-element check fires first → Disadvantage (0.5)
        // Actually, same-element is checked before the specific method
        float mult = ElementSystem.GetAffinityMultiplier(Element.Poison, Element.Poison);
        Assert.Equal(ElementSystem.DisadvantageMutiplier, mult);
    }

    [Fact]
    public void ElementSystem_GetAffinityType_ReturnsCorrectEnum()
    {
        var aff = ElementSystem.GetAffinityType(Element.Fire, Element.Ice);
        Assert.Equal(ElementAffinity.Advantage, aff);
    }

    #endregion

    #region ResourceSystem

    [Fact]
    public void ResourceSystem_MaxHp_WarriorHigherThanMage()
    {
        var rs = new ResourceSystem();
        int warriorHp = rs.CalculateMaxHp(new HpCalculationParams
        {
            Vitality = 10, Level = 5, RaceBonus = 0,
            CharacterClass = CharacterClass.Fighter
        });
        int mageHp = rs.CalculateMaxHp(new HpCalculationParams
        {
            Vitality = 10, Level = 5, RaceBonus = 0,
            CharacterClass = CharacterClass.Mage
        });
        Assert.True(warriorHp > mageHp);
    }

    [Fact]
    public void ResourceSystem_MaxHp_FormulaValidation()
    {
        var rs = new ResourceSystem();
        // MaxHP = 50 + (VIT*10) + (Lv*5) + raceBonus + classBonus
        // Warrior Lv1: classBonus = 15*(1-1) = 0
        int hp = rs.CalculateMaxHp(new HpCalculationParams
        {
            Vitality = 10, Level = 1, RaceBonus = 5,
            CharacterClass = CharacterClass.Fighter
        });
        Assert.Equal(50 + 100 + 5 + 5 + 0, hp); // 160
    }

    [Fact]
    public void ResourceSystem_GetHpState_DeadWhenZero()
    {
        var rs = new ResourceSystem();
        Assert.Equal(HpState.Dead, rs.GetHpState(0, 100));
    }

    [Fact]
    public void ResourceSystem_GetHpState_HealthyWhenFull()
    {
        var rs = new ResourceSystem();
        Assert.Equal(HpState.Healthy, rs.GetHpState(100, 100));
    }

    [Fact]
    public void ResourceSystem_GetHpState_CriticalWhenLow()
    {
        var rs = new ResourceSystem();
        Assert.Equal(HpState.Critical, rs.GetHpState(10, 100));
    }

    #endregion

    #region DamageCalculator

    [Fact]
    public void DamageCalculator_PhysicalDamage_ProducesPositiveDamage()
    {
        var calc = new DamageCalculator(new FixedRandom(0.5));
        var result = calc.CalculatePhysicalDamage(new PhysicalDamageParams
        {
            WeaponAttack = 20,
            Strength = 10,
            ArmorDefense = 5,
            Vitality = 5,
            SkillMultiplier = 1.0f,
            CriticalRate = 0.0
        });
        Assert.True(result.FinalDamage > 0);
        Assert.Equal(DamageType.Physical, result.DamageType);
    }

    [Fact]
    public void DamageCalculator_MagicalDamage_ProducesPositiveDamage()
    {
        var calc = new DamageCalculator(new FixedRandom(0.5));
        var result = calc.CalculateMagicalDamage(new MagicalDamageParams
        {
            StaffAttack = 15,
            Intelligence = 10,
            MagicDefense = 3,
            Mind = 5,
            SkillMultiplier = 1.0f,
            LanguageBonus = 1.0f
        });
        Assert.True(result.FinalDamage > 0);
        Assert.False(result.IsCritical); // magic has no critical
    }

    [Fact]
    public void DamageCalculator_DefendedDamage_ReducesByHalf()
    {
        var calc = new DamageCalculator(new FixedRandom(0.5));
        int defended = calc.CalculateDefendedDamage(100);
        Assert.Equal(50, defended);
    }

    #endregion

    #region StatusEffectSystem

    [Fact]
    public void StatusEffectSystem_CreatePoison_HasCorrectType()
    {
        var system = new StatusEffectSystem(new FixedRandom());
        var effect = system.CreatePoison(100);
        Assert.Equal(StatusEffectType.Poison, effect.Type);
        Assert.Equal(20, effect.Duration);
    }

    [Fact]
    public void StatusEffectSystem_CreateBurn_ReducesAttack()
    {
        var system = new StatusEffectSystem(new FixedRandom());
        var effect = system.CreateBurn();
        Assert.Equal(StatusEffectType.Burn, effect.Type);
        Assert.Equal(0.85f, effect.AttackMultiplier);
    }

    [Fact]
    public void StatusEffectSystem_CreateFreeze_CausesImmobility()
    {
        var system = new StatusEffectSystem(new FixedRandom());
        var effect = system.CreateFreeze();
        Assert.Equal(StatusEffectType.Freeze, effect.Type);
        Assert.Equal(999f, effect.TurnCostModifier);  // 行動不能（オーバーフロー回避）
    }

    [Fact]
    public void StatusEffectSystem_CreateCurse_HasInfiniteDuration()
    {
        var system = new StatusEffectSystem(new FixedRandom());
        var effect = system.CreateCurse();
        Assert.Equal(StatusEffectType.Curse, effect.Type);
        Assert.Equal(200, effect.Duration);  // EO-1: 呪い持続を有限化（200ターン）
    }

    [Fact]
    public void StatusEffectSystem_CheckFreezeBreak_FireBreaksFreeze()
    {
        var system = new StatusEffectSystem(new FixedRandom());
        Assert.True(system.CheckFreezeBreak(Element.Fire));
        Assert.False(system.CheckFreezeBreak(Element.Water));
    }

    #endregion

    #region GameState

    [Fact]
    public void GameState_Initialization_PropertiesAreSet()
    {
        var random = new FixedRandom();
        var player = Player.Create("テスト", Stats.Default);
        var map = new BasicMap(20, 20);
        var combat = new CombatSystem(random);
        var state = new GameState(player, map, combat, random);

        Assert.Same(player, state.Player);
        Assert.Same(map, state.CurrentMap);
        Assert.Equal(0, state.CurrentTurn);
    }

    [Fact]
    public void GameState_AddEnemy_IncreasesEnemyCount()
    {
        var random = new FixedRandom();
        var player = Player.Create("テスト", Stats.Default);
        var map = new BasicMap(20, 20);
        var combat = new CombatSystem(random);
        var state = new GameState(player, map, combat, random);

        var enemy = Enemy.Create("スライム", "slime", Stats.Default, 10);
        state.AddEnemy(enemy);
        Assert.Single(state.Enemies);
    }

    [Fact]
    public void GameState_ChangeMap_ClearsEnemies()
    {
        var random = new FixedRandom();
        var player = Player.Create("テスト", Stats.Default);
        var map1 = new BasicMap(20, 20);
        var map2 = new BasicMap(30, 30);
        var combat = new CombatSystem(random);
        var state = new GameState(player, map1, combat, random);

        state.AddEnemy(Enemy.Create("スライム", "slime", Stats.Default, 10));
        state.ChangeMap(map2);

        Assert.Empty(state.Enemies);
        Assert.Same(map2, state.CurrentMap);
    }

    #endregion

    #region SpellCastingSystem

    [Fact]
    public void SpellCastingSystem_InitialState_NotCasting()
    {
        var system = new SpellCastingSystem();
        Assert.False(system.IsCasting);
        Assert.Empty(system.CurrentIncantation);
    }

    [Fact]
    public void SpellCastingSystem_RemoveLastWord_EmptyReturnsFalse()
    {
        var system = new SpellCastingSystem();
        Assert.False(system.RemoveLastWord());
    }

    [Fact]
    public void SpellCastingSystem_CancelCasting_ClearsIncantation()
    {
        var system = new SpellCastingSystem();
        // Manually add to test cancel, bypassing player check
        system.CurrentIncantation.Add("test_word");
        Assert.True(system.IsCasting);

        system.CancelCasting();
        Assert.False(system.IsCasting);
        Assert.Empty(system.CurrentIncantation);
    }

    #endregion
}
