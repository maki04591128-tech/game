using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

public class ItemGradeSystemTests
{
    #region Test Helpers

    private class TestRandomProvider : IRandomProvider
    {
        private readonly double _fixedDouble;

        public TestRandomProvider(double fixedDouble)
        {
            _fixedDouble = fixedDouble;
        }

        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => _fixedDouble;
    }

    #endregion

    #region ItemGrade Enum Tests

    [Fact]
    public void ItemGrade_HasCorrectValues()
    {
        Assert.Equal(1, (int)ItemGrade.Crude);
        Assert.Equal(2, (int)ItemGrade.Cheap);
        Assert.Equal(3, (int)ItemGrade.Standard);
        Assert.Equal(4, (int)ItemGrade.Fine);
        Assert.Equal(5, (int)ItemGrade.Superior);
        Assert.Equal(6, (int)ItemGrade.Masterwork);
    }

    #endregion

    #region GetGradeInfo Tests

    [Theory]
    [InlineData(ItemGrade.Crude, "粗悪品", 0.7f, 0.4f, 0.20f)]
    [InlineData(ItemGrade.Cheap, "廉価品", 0.85f, 0.7f, 0.25f)]
    [InlineData(ItemGrade.Standard, "標準品", 1.0f, 1.0f, 0.30f)]
    [InlineData(ItemGrade.Fine, "良品", 1.15f, 1.5f, 0.15f)]
    [InlineData(ItemGrade.Superior, "上質品", 1.3f, 2.5f, 0.08f)]
    [InlineData(ItemGrade.Masterwork, "傑作品", 1.5f, 5.0f, 0.02f)]
    public void GetGradeInfo_ReturnsCorrectInfo(
        ItemGrade grade, string japaneseName, float statMul, float priceMul, float dropRate)
    {
        var info = ItemGradeSystem.GetGradeInfo(grade);

        Assert.Equal(grade, info.Grade);
        Assert.Equal(japaneseName, info.JapaneseName);
        Assert.Equal(statMul, info.StatMultiplier, 2);
        Assert.Equal(priceMul, info.PriceMultiplier, 2);
        Assert.Equal(dropRate, info.DropRate, 2);
    }

    #endregion

    #region GetStatMultiplier Tests

    [Theory]
    [InlineData(ItemGrade.Crude, 0.7f)]
    [InlineData(ItemGrade.Standard, 1.0f)]
    [InlineData(ItemGrade.Masterwork, 1.5f)]
    public void GetStatMultiplier_ReturnsCorrectValue(ItemGrade grade, float expected)
    {
        var result = ItemGradeSystem.GetStatMultiplier(grade);

        Assert.Equal(expected, result, 2);
    }

    #endregion

    #region GetPriceMultiplier Tests

    [Theory]
    [InlineData(ItemGrade.Crude, 0.4f)]
    [InlineData(ItemGrade.Standard, 1.0f)]
    [InlineData(ItemGrade.Masterwork, 5.0f)]
    public void GetPriceMultiplier_ReturnsCorrectValue(ItemGrade grade, float expected)
    {
        var result = ItemGradeSystem.GetPriceMultiplier(grade);

        Assert.Equal(expected, result, 2);
    }

    #endregion

    #region GetGradeDisplayPrefix Tests

    [Theory]
    [InlineData(ItemGrade.Crude, "粗悪な")]
    [InlineData(ItemGrade.Cheap, "廉価な")]
    [InlineData(ItemGrade.Standard, "")]
    [InlineData(ItemGrade.Fine, "良質な")]
    [InlineData(ItemGrade.Superior, "上質な")]
    [InlineData(ItemGrade.Masterwork, "傑作の")]
    public void GetGradeDisplayPrefix_ReturnsCorrectPrefix(ItemGrade grade, string expected)
    {
        var result = ItemGradeSystem.GetGradeDisplayPrefix(grade);

        Assert.Equal(expected, result);
    }

    #endregion

    #region DetermineGrade Tests

    [Fact]
    public void DetermineGrade_LowRoll_ReturnsCrude()
    {
        // Crude: 0.0 ～ 0.20
        var random = new TestRandomProvider(0.0);
        var grade = ItemGradeSystem.DetermineGrade(random);

        Assert.Equal(ItemGrade.Crude, grade);
    }

    [Fact]
    public void DetermineGrade_MidRoll_ReturnsStandard()
    {
        // Crude(0.20) + Cheap(0.25) = 0.45 → Standard(0.30) = 0.45 ～ 0.75
        var random = new TestRandomProvider(0.50);
        var grade = ItemGradeSystem.DetermineGrade(random);

        Assert.Equal(ItemGrade.Standard, grade);
    }

    [Fact]
    public void DetermineGrade_HighRoll_ReturnsMasterwork()
    {
        // 合計は1.0 → 0.99は最後のMasterwork
        var random = new TestRandomProvider(0.99);
        var grade = ItemGradeSystem.DetermineGrade(random);

        Assert.Equal(ItemGrade.Masterwork, grade);
    }

    [Fact]
    public void DetermineGrade_AllGradesAreReachable()
    {
        // 各等級の境界値をテスト
        var grades = new HashSet<ItemGrade>();
        for (double d = 0.0; d < 1.0; d += 0.01)
        {
            var random = new TestRandomProvider(d);
            grades.Add(ItemGradeSystem.DetermineGrade(random));
        }

        Assert.Contains(ItemGrade.Crude, grades);
        Assert.Contains(ItemGrade.Cheap, grades);
        Assert.Contains(ItemGrade.Standard, grades);
        Assert.Contains(ItemGrade.Fine, grades);
        Assert.Contains(ItemGrade.Superior, grades);
        Assert.Contains(ItemGrade.Masterwork, grades);
    }

    #endregion

    #region GetGradeDropRates Tests

    [Fact]
    public void GetGradeDropRates_DefaultSmithing_SumsToOne()
    {
        var rates = ItemGradeSystem.GetGradeDropRates(0);
        float total = rates.Values.Sum();

        Assert.Equal(1.0f, total, 2);
    }

    [Fact]
    public void GetGradeDropRates_HighSmithing_IncreasesUpperGradeRates()
    {
        var baseRates = ItemGradeSystem.GetGradeDropRates(0);
        var boostedRates = ItemGradeSystem.GetGradeDropRates(10);

        // 高鍛冶レベルでは上位品の割合が増加
        Assert.True(boostedRates[ItemGrade.Fine] > baseRates[ItemGrade.Fine]);
        Assert.True(boostedRates[ItemGrade.Superior] > baseRates[ItemGrade.Superior]);
        Assert.True(boostedRates[ItemGrade.Masterwork] > baseRates[ItemGrade.Masterwork]);

        // 下位品の割合が減少
        Assert.True(boostedRates[ItemGrade.Crude] < baseRates[ItemGrade.Crude]);
    }

    [Fact]
    public void GetGradeDropRates_HighSmithing_StillSumsToOne()
    {
        var rates = ItemGradeSystem.GetGradeDropRates(20);
        float total = rates.Values.Sum();

        Assert.Equal(1.0f, total, 1);
    }

    #endregion

    #region Item Grade Integration Tests

    [Fact]
    public void Item_DefaultGrade_IsStandard()
    {
        var weapon = ItemFactory.CreateRustySword();

        Assert.Equal(ItemGrade.Standard, weapon.Grade);
    }

    [Fact]
    public void Item_CalculatePrice_AppliesGradeMultiplier()
    {
        var standardWeapon = ItemFactory.CreateIronSword();
        standardWeapon.Grade = ItemGrade.Standard;
        int standardPrice = standardWeapon.CalculatePrice();

        var crudeWeapon = ItemFactory.CreateIronSword();
        crudeWeapon.Grade = ItemGrade.Crude;
        int crudePrice = crudeWeapon.CalculatePrice();

        var masterworkWeapon = ItemFactory.CreateIronSword();
        masterworkWeapon.Grade = ItemGrade.Masterwork;
        int masterworkPrice = masterworkWeapon.CalculatePrice();

        // 粗悪品 < 標準品 < 傑作品
        Assert.True(crudePrice < standardPrice);
        Assert.True(standardPrice < masterworkPrice);
    }

    [Fact]
    public void Item_GetDisplayName_IncludesGradePrefix()
    {
        var weapon = ItemFactory.CreateIronSword();

        weapon.Grade = ItemGrade.Crude;
        Assert.Contains("粗悪な", weapon.GetDisplayName());

        weapon.Grade = ItemGrade.Standard;
        Assert.DoesNotContain("粗悪な", weapon.GetDisplayName());
        Assert.DoesNotContain("良質な", weapon.GetDisplayName());

        weapon.Grade = ItemGrade.Masterwork;
        Assert.Contains("傑作の", weapon.GetDisplayName());
    }

    [Fact]
    public void Item_GetDisplayName_CombinesGradeWithOtherPrefixes()
    {
        var weapon = ItemFactory.CreateIronSword();
        weapon.Grade = ItemGrade.Superior;
        weapon.IsBlessed = true;
        weapon.EnhancementLevel = 3;

        var displayName = weapon.GetDisplayName();

        Assert.Contains("祝福された", displayName);
        Assert.Contains("上質な", displayName);
        Assert.Contains("+3", displayName);
    }

    [Fact]
    public void Item_GetDisplayName_UnidentifiedIgnoresGrade()
    {
        var weapon = ItemFactory.CreateIronSword();
        weapon.Grade = ItemGrade.Masterwork;
        weapon.IsIdentified = false;

        var displayName = weapon.GetDisplayName();

        Assert.DoesNotContain("傑作の", displayName);
        Assert.Equal(weapon.UnidentifiedName, displayName);
    }

    #endregion
}
