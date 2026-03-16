using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.4 装備耐久値システムのテスト
/// </summary>
public class DurabilitySystemTests
{
    [Theory]
    [InlineData(100, 100, DurabilityStage.Perfect)]
    [InlineData(76, 100, DurabilityStage.Perfect)]
    [InlineData(75, 100, DurabilityStage.Worn)]
    [InlineData(51, 100, DurabilityStage.Worn)]
    [InlineData(50, 100, DurabilityStage.Damaged)]
    [InlineData(26, 100, DurabilityStage.Damaged)]
    [InlineData(25, 100, DurabilityStage.Critical)]
    [InlineData(1, 100, DurabilityStage.Critical)]
    [InlineData(0, 100, DurabilityStage.Broken)]
    public void GetStage_ReturnsCorrectStage(int current, int max, DurabilityStage expected)
    {
        Assert.Equal(expected, DurabilitySystem.GetStage(current, max));
    }

    [Fact]
    public void GetStage_InfiniteDurability_ReturnsPerfect()
    {
        Assert.Equal(DurabilityStage.Perfect, DurabilitySystem.GetStage(-1, -1));
    }

    [Theory]
    [InlineData(DurabilityStage.Perfect, 1.0f)]
    [InlineData(DurabilityStage.Worn, 0.9f)]
    [InlineData(DurabilityStage.Damaged, 0.7f)]
    [InlineData(DurabilityStage.Critical, 0.5f)]
    [InlineData(DurabilityStage.Broken, 0f)]
    public void GetPerformanceMultiplier_ReturnsCorrectValue(DurabilityStage stage, float expected)
    {
        Assert.Equal(expected, DurabilitySystem.GetPerformanceMultiplier(stage));
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void CalculateWeaponWear_ReturnsCorrectWear(bool isCritical, int expected)
    {
        Assert.Equal(expected, DurabilitySystem.CalculateWeaponWear(isCritical));
    }

    [Fact]
    public void CalculateArmorWear_HighDamage_MoreWear()
    {
        int lowWear = DurabilitySystem.CalculateArmorWear(10, Element.Fire);
        int highWear = DurabilitySystem.CalculateArmorWear(60, Element.Fire);
        Assert.True(highWear > lowWear);
    }

    [Fact]
    public void CalculateArmorWear_AcidDamage_ExtraWear()
    {
        int normalWear = DurabilitySystem.CalculateArmorWear(10, Element.Fire);
        int acidWear = DurabilitySystem.CalculateArmorWear(10, Element.Poison);
        Assert.True(acidWear > normalWear);
    }

    [Fact]
    public void CalculateRepairCost_FullDurability_ReturnsZero()
    {
        Assert.Equal(0, DurabilitySystem.CalculateRepairCost(100, 100, 500));
    }

    [Fact]
    public void CalculateRepairCost_LowerDurability_HigherCost()
    {
        int cost1 = DurabilitySystem.CalculateRepairCost(80, 100, 500);
        int cost2 = DurabilitySystem.CalculateRepairCost(50, 100, 500);
        Assert.True(cost2 > cost1);
    }

    [Theory]
    [InlineData(1, 15)]
    [InlineData(2, 30)]
    [InlineData(3, 50)]
    public void CalculateKitRepairAmount_ReturnsCorrectAmount(int quality, int expected)
    {
        Assert.Equal(expected, DurabilitySystem.CalculateKitRepairAmount(quality));
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void CanSelfRepair_RequiresLevel5(int level, bool expected)
    {
        Assert.Equal(expected, DurabilitySystem.CanSelfRepair(level));
    }

    [Fact]
    public void CalculateSelfRepairAmount_IncreasesWithLevel()
    {
        int low = DurabilitySystem.CalculateSelfRepairAmount(5);
        int high = DurabilitySystem.CalculateSelfRepairAmount(15);
        Assert.True(high > low);
    }

    [Fact]
    public void CalculateSelfRepairAmount_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, DurabilitySystem.CalculateSelfRepairAmount(3));
    }
}
