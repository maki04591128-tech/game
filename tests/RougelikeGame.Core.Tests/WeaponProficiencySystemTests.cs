using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// WeaponProficiencySystem テスト
/// - 全武器種のプロファイル取得
/// - スケーリングボーナス計算
/// - 武器ダメージ計算
/// </summary>
public class WeaponProficiencySystemTests
{
    #region GetWeaponProfile Tests

    [Theory]
    [InlineData(WeaponType.Unarmed, AttackType.Unarmed, "STR")]
    [InlineData(WeaponType.Dagger, AttackType.Pierce, "DEX")]
    [InlineData(WeaponType.Sword, AttackType.Slash, "STR/DEX")]
    [InlineData(WeaponType.Greatsword, AttackType.Slash, "STR")]
    [InlineData(WeaponType.Axe, AttackType.Slash, "STR")]
    [InlineData(WeaponType.Greataxe, AttackType.Slash, "STR")]
    [InlineData(WeaponType.Spear, AttackType.Pierce, "DEX")]
    [InlineData(WeaponType.Hammer, AttackType.Blunt, "STR")]
    [InlineData(WeaponType.Staff, AttackType.Blunt, "INT")]
    [InlineData(WeaponType.Bow, AttackType.Ranged, "DEX")]
    [InlineData(WeaponType.Crossbow, AttackType.Ranged, "DEX")]
    [InlineData(WeaponType.Thrown, AttackType.Ranged, "DEX")]
    [InlineData(WeaponType.Whip, AttackType.Slash, "DEX")]
    [InlineData(WeaponType.Fist, AttackType.Unarmed, "STR/DEX")]
    public void GetWeaponProfile_ReturnsCorrectAttackTypeAndScaling(WeaponType type, AttackType expectedAttack, string expectedScaling)
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(type);

        Assert.Equal(type, profile.Type);
        Assert.Equal(expectedAttack, profile.PrimaryAttackType);
        Assert.Equal(expectedScaling, profile.ScalingStat);
    }

    [Theory]
    [InlineData(WeaponType.Dagger, 1.5f)]
    [InlineData(WeaponType.Crossbow, 1.5f)]
    [InlineData(WeaponType.Staff, 0.8f)]
    [InlineData(WeaponType.Unarmed, 1.0f)]
    [InlineData(WeaponType.Sword, 1.2f)]
    [InlineData(WeaponType.Axe, 1.3f)]
    [InlineData(WeaponType.Greataxe, 1.2f)]
    [InlineData(WeaponType.Hammer, 1.0f)]
    public void GetWeaponProfile_ReturnsCorrectCriticalModifier(WeaponType type, float expectedCritMod)
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(type);
        Assert.Equal(expectedCritMod, profile.CriticalModifier);
    }

    [Theory]
    [InlineData(WeaponType.Greataxe, 0.4f)]
    [InlineData(WeaponType.Hammer, 0.35f)]
    [InlineData(WeaponType.Greatsword, 0.3f)]
    [InlineData(WeaponType.Dagger, 0.0f)]
    [InlineData(WeaponType.Bow, 0.0f)]
    public void GetWeaponProfile_ReturnsCorrectStaggerChance(WeaponType type, float expectedStagger)
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(type);
        Assert.Equal(expectedStagger, profile.StaggerChance);
    }

    [Fact]
    public void GetWeaponProfile_DaggerCanParry()
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(WeaponType.Dagger);
        Assert.True(profile.CanParry);
        Assert.False(profile.CanBlock);
    }

    [Fact]
    public void GetWeaponProfile_SwordCanParry()
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(WeaponType.Sword);
        Assert.True(profile.CanParry);
    }

    [Fact]
    public void GetWeaponProfile_SwordHasSecondaryAttackType()
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(WeaponType.Sword);
        Assert.NotNull(profile.SecondaryAttackType);
        Assert.Equal(AttackType.Pierce, profile.SecondaryAttackType);
    }

    [Fact]
    public void GetWeaponProfile_GreatswordHasNoSecondaryAttackType()
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(WeaponType.Greatsword);
        Assert.Null(profile.SecondaryAttackType);
    }

    [Fact]
    public void GetWeaponProfile_AllWeaponTypesHaveProfiles()
    {
        foreach (WeaponType type in Enum.GetValues<WeaponType>())
        {
            var profile = WeaponProficiencySystem.GetWeaponProfile(type);
            Assert.NotNull(profile);
            Assert.True(profile.CriticalModifier > 0);
            Assert.True(profile.StaggerChance >= 0);
        }
    }

    #endregion

    #region GetScalingBonus Tests

    [Fact]
    public void GetScalingBonus_STR_Weapon_ScalesWithStrength()
    {
        var stats = new Stats(Strength: 30, Vitality: 10, Agility: 10, Dexterity: 10,
            Intelligence: 10, Mind: 10, Perception: 10, Charisma: 10, Luck: 10);

        int bonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Hammer, stats);
        Assert.Equal(10, bonus); // 30 / 3 = 10
    }

    [Fact]
    public void GetScalingBonus_DEX_Weapon_ScalesWithDexterity()
    {
        var stats = new Stats(Strength: 10, Vitality: 10, Agility: 10, Dexterity: 24,
            Intelligence: 10, Mind: 10, Perception: 10, Charisma: 10, Luck: 10);

        int bonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Dagger, stats);
        Assert.Equal(8, bonus); // 24 / 3 = 8
    }

    [Fact]
    public void GetScalingBonus_INT_Weapon_ScalesWithIntelligence()
    {
        var stats = new Stats(Strength: 10, Vitality: 10, Agility: 10, Dexterity: 10,
            Intelligence: 21, Mind: 10, Perception: 10, Charisma: 10, Luck: 10);

        int bonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Staff, stats);
        Assert.Equal(7, bonus); // 21 / 3 = 7
    }

    [Fact]
    public void GetScalingBonus_DualScaling_UsesAverage()
    {
        var stats = new Stats(Strength: 20, Vitality: 10, Agility: 10, Dexterity: 15,
            Intelligence: 10, Mind: 10, Perception: 10, Charisma: 10, Luck: 10);

        int bonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Sword, stats);
        Assert.Equal(7, bonus); // (20 + 15) / 5 = 7
    }

    [Fact]
    public void GetScalingBonus_ZeroStats_ReturnsZero()
    {
        int bonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Hammer, Stats.Zero);
        Assert.Equal(0, bonus);
    }

    #endregion

    #region CalculateWeaponDamage Tests

    [Fact]
    public void CalculateWeaponDamage_ReturnsPositive()
    {
        var weapon = new Weapon
        {
            ItemId = "test_sword",
            Name = "テスト剣",
            WeaponType = WeaponType.Sword,
            BaseDamage = 10,
            DamageRange = (8, 12)
        };
        var stats = Stats.Default;
        var random = new Random(42);

        int damage = WeaponProficiencySystem.CalculateWeaponDamage(weapon, stats, random);
        Assert.True(damage >= 1);
    }

    [Fact]
    public void CalculateWeaponDamage_IncludesEnhancementBonus()
    {
        var weapon = new Weapon
        {
            ItemId = "test_sword",
            Name = "テスト剣",
            WeaponType = WeaponType.Sword,
            BaseDamage = 10,
            DamageRange = (10, 10),
            EnhancementLevel = 3
        };
        var stats = Stats.Zero;
        var random = new Random(42);

        int damage = WeaponProficiencySystem.CalculateWeaponDamage(weapon, stats, random);
        // 10 (base) + 6 (enhancement) + 0 (scaling) = 16
        Assert.Equal(16, damage);
    }

    [Fact]
    public void CalculateWeaponDamage_IncludesScalingBonus()
    {
        var weapon = new Weapon
        {
            ItemId = "test_hammer",
            Name = "テストハンマー",
            WeaponType = WeaponType.Hammer,
            BaseDamage = 10,
            DamageRange = (10, 10)
        };
        var stats = new Stats(Strength: 15, Vitality: 10, Agility: 10, Dexterity: 10,
            Intelligence: 10, Mind: 10, Perception: 10, Charisma: 10, Luck: 10);
        var random = new Random(42);

        int damage = WeaponProficiencySystem.CalculateWeaponDamage(weapon, stats, random);
        // 10 (base) + 0 (enhancement) + 5 (15/3 scaling) = 15
        Assert.Equal(15, damage);
    }

    [Fact]
    public void CalculateWeaponDamage_MinimumIsOne()
    {
        var weapon = new Weapon
        {
            ItemId = "test_weapon",
            Name = "テスト武器",
            WeaponType = WeaponType.Unarmed,
            BaseDamage = 1,
            DamageRange = (0, 0)
        };

        int damage = WeaponProficiencySystem.CalculateWeaponDamage(weapon, Stats.Zero, new Random(42));
        Assert.True(damage >= 1);
    }

    #endregion

    #region GetAllProfiles Tests

    [Fact]
    public void GetAllProfiles_ContainsAllWeaponTypes()
    {
        var profiles = WeaponProficiencySystem.GetAllProfiles();
        foreach (WeaponType type in Enum.GetValues<WeaponType>())
        {
            Assert.True(profiles.ContainsKey(type), $"Missing profile for {type}");
        }
    }

    #endregion
}
