using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.11 エンチャントシステムのテスト
/// </summary>
public class EnchantmentSystemTests
{
    #region SoulGem Tests

    [Theory]
    [InlineData(EnemyRank.Common, SoulGemQuality.Fragment)]
    [InlineData(EnemyRank.Elite, SoulGemQuality.Small)]
    [InlineData(EnemyRank.Rare, SoulGemQuality.Medium)]
    [InlineData(EnemyRank.Boss, SoulGemQuality.Large)]
    [InlineData(EnemyRank.HiddenBoss, SoulGemQuality.Grand)]
    public void GetSoulGemQualityFromRank_ReturnsCorrectQuality(EnemyRank rank, SoulGemQuality expected)
    {
        Assert.Equal(expected, EnchantmentSystem.GetSoulGemQualityFromRank(rank));
    }

    [Theory]
    [InlineData(SoulGemQuality.Fragment, 0.50f)]
    [InlineData(SoulGemQuality.Small, 0.60f)]
    [InlineData(SoulGemQuality.Medium, 0.70f)]
    [InlineData(SoulGemQuality.Large, 0.85f)]
    [InlineData(SoulGemQuality.Grand, 0.95f)]
    public void GetSuccessRate_ReturnsCorrectRate(SoulGemQuality quality, float expected)
    {
        Assert.Equal(expected, EnchantmentSystem.GetSuccessRate(quality));
    }

    [Fact]
    public void GetSoulGemName_ReturnsJapaneseName()
    {
        Assert.Equal("魂石の欠片", EnchantmentSystem.GetSoulGemName(SoulGemQuality.Fragment));
        Assert.Equal("極大魂石", EnchantmentSystem.GetSoulGemName(SoulGemQuality.Grand));
    }

    #endregion

    #region Enchantment Definition Tests

    [Fact]
    public void GetAllEnchantments_Returns15Types()
    {
        var all = EnchantmentSystem.GetAllEnchantments();
        Assert.Equal(15, all.Count);
    }

    [Fact]
    public void GetEnchantmentInfo_ReturnsValidDefinition()
    {
        var fire = EnchantmentSystem.GetEnchantmentInfo(EnchantmentType.FireDamage);
        Assert.NotNull(fire);
        Assert.Equal("火炎付与", fire!.Name);
        Assert.Equal(Element.Fire, fire.AssociatedElement);
    }

    [Fact]
    public void GetEnchantmentInfo_UnknownType_ReturnsNull()
    {
        var result = EnchantmentSystem.GetEnchantmentInfo((EnchantmentType)999);
        Assert.Null(result);
    }

    #endregion

    #region CanEnchant Tests

    [Fact]
    public void CanEnchant_SufficientQuality_ReturnsTrue()
    {
        var weapon = CreateTestWeapon();
        Assert.True(EnchantmentSystem.CanEnchant(weapon, EnchantmentType.FireDamage, SoulGemQuality.Fragment));
        Assert.True(EnchantmentSystem.CanEnchant(weapon, EnchantmentType.FireDamage, SoulGemQuality.Grand));
    }

    [Fact]
    public void CanEnchant_InsufficientQuality_ReturnsFalse()
    {
        var weapon = CreateTestWeapon();
        // HolyDamage requires Medium quality
        Assert.False(EnchantmentSystem.CanEnchant(weapon, EnchantmentType.HolyDamage, SoulGemQuality.Fragment));
    }

    #endregion

    #region Enchant Tests

    [Fact]
    public void Enchant_Success_ReturnsSuccessResult()
    {
        var weapon = CreateTestWeapon();
        var random = new AlwaysSuccessRandom();

        var result = EnchantmentSystem.Enchant(weapon, EnchantmentType.FireDamage, SoulGemQuality.Grand, random);

        Assert.True(result.Success);
        Assert.Equal(EnchantmentType.FireDamage, result.AppliedType);
        Assert.Contains("火炎付与", result.Message);
    }

    [Fact]
    public void Enchant_Failure_ReturnsFailResult()
    {
        var weapon = CreateTestWeapon();
        var random = new AlwaysFailRandom();

        var result = EnchantmentSystem.Enchant(weapon, EnchantmentType.FireDamage, SoulGemQuality.Fragment, random);

        Assert.False(result.Success);
        Assert.Null(result.AppliedType);
    }

    [Fact]
    public void Enchant_InsufficientQuality_ReturnsError()
    {
        var weapon = CreateTestWeapon();
        var random = new AlwaysSuccessRandom();

        var result = EnchantmentSystem.Enchant(weapon, EnchantmentType.HolyDamage, SoulGemQuality.Fragment, random);

        Assert.False(result.Success);
        Assert.Contains("品質が不足", result.Message);
    }

    #endregion

    #region Available Enchantments Tests

    [Fact]
    public void GetAvailableEnchantments_Fragment_ReturnsBasicTypes()
    {
        var available = EnchantmentSystem.GetAvailableEnchantments(SoulGemQuality.Fragment);
        Assert.True(available.Count > 0);
        Assert.True(available.Count < 15);  // Fragment can't access all
    }

    [Fact]
    public void GetAvailableEnchantments_Grand_ReturnsAllTypes()
    {
        var available = EnchantmentSystem.GetAvailableEnchantments(SoulGemQuality.Grand);
        Assert.Equal(15, available.Count);
    }

    [Fact]
    public void GetAvailableEnchantments_HigherQuality_IncludesLower()
    {
        var fragment = EnchantmentSystem.GetAvailableEnchantments(SoulGemQuality.Fragment).Count;
        var small = EnchantmentSystem.GetAvailableEnchantments(SoulGemQuality.Small).Count;
        var medium = EnchantmentSystem.GetAvailableEnchantments(SoulGemQuality.Medium).Count;

        Assert.True(small >= fragment);
        Assert.True(medium >= small);
    }

    #endregion

    #region Damage Bonus Tests

    [Fact]
    public void CalculateEnchantedDamageBonus_FireDamage_ReturnsPositive()
    {
        int bonus = EnchantmentSystem.CalculateEnchantedDamageBonus(EnchantmentType.FireDamage, 100);
        Assert.True(bonus > 0);
    }

    [Fact]
    public void CalculateEnchantedDamageBonus_NonDamageType_ReturnsZero()
    {
        int bonus = EnchantmentSystem.CalculateEnchantedDamageBonus(EnchantmentType.ExpBoost, 100);
        Assert.Equal(0, bonus);
    }

    #endregion

    #region Helpers

    private static Weapon CreateTestWeapon()
    {
        return new Weapon
        {
            ItemId = "test_sword",
            Name = "テスト剣",
            WeaponType = WeaponType.Sword,
            BaseDamage = 10,
            DamageRange = (8, 12)
        };
    }

    private class AlwaysSuccessRandom : IRandomProvider
    {
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => 0.01;  // Always below success rate
    }

    private class AlwaysFailRandom : IRandomProvider
    {
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => 0.99;  // Always above success rate
    }

    #endregion
}
