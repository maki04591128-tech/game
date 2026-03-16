using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.48 採取システム / P.49 釣りシステムのテスト
/// </summary>
public class GatheringFishingTests
{
    #region Gathering Tests

    [Theory]
    [InlineData(GatheringType.Herb)]
    [InlineData(GatheringType.Mining)]
    [InlineData(GatheringType.Logging)]
    [InlineData(GatheringType.Fishing)]
    [InlineData(GatheringType.Foraging)]
    public void GetNode_AllTypes_ReturnValid(GatheringType type)
    {
        var node = GatheringSystem.GetNode(type);
        Assert.NotNull(node);
        Assert.NotEmpty(node!.Name);
    }

    [Fact]
    public void CanGather_LevelTooLow_ReturnsFalse()
    {
        Assert.False(GatheringSystem.CanGather(GatheringType.Mining, 1)); // Requires level 3
    }

    [Fact]
    public void CanGather_SufficientLevel_ReturnsTrue()
    {
        Assert.True(GatheringSystem.CanGather(GatheringType.Herb, 1));
    }

    [Fact]
    public void CalculateSuccessRate_IncreasesWithLevel()
    {
        float low = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 1, Season.Winter);
        float high = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 10, Season.Winter);
        Assert.True(high > low);
    }

    [Fact]
    public void CalculateSuccessRate_BestSeason_Bonus()
    {
        float offSeason = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 5, Season.Winter);
        float bestSeason = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 5, Season.Spring);
        Assert.True(bestSeason > offSeason);
    }

    [Fact]
    public void CalculateGatheringDuration_DecreasesWithLevel()
    {
        int slow = GatheringSystem.CalculateGatheringDuration(GatheringType.Mining, 1);
        int fast = GatheringSystem.CalculateGatheringDuration(GatheringType.Mining, 15);
        Assert.True(fast < slow);
    }

    #endregion

    #region Fishing Tests

    [Fact]
    public void GetAllFish_ReturnsMultiple()
    {
        Assert.True(FishingSystem.GetAllFish().Count >= 9);
    }

    [Fact]
    public void GetAvailableFish_SpringMorning_ReturnsResults()
    {
        var fish = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 5);
        Assert.NotEmpty(fish);
    }

    [Fact]
    public void GetAvailableFish_FiltersByLevel()
    {
        var low = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Dawn, 1);
        var high = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Dawn, 10);
        Assert.True(high.Count >= low.Count);
    }

    [Fact]
    public void CalculateCatchRate_HighRarity_LowerRate()
    {
        float common = FishingSystem.CalculateCatchRate(1, 5, 0);
        float rare = FishingSystem.CalculateCatchRate(4, 5, 0);
        Assert.True(common > rare);
    }

    [Fact]
    public void CalculateJunkRate_DecreasesWithLevel()
    {
        float low = FishingSystem.CalculateJunkRate(1);
        float high = FishingSystem.CalculateJunkRate(10);
        Assert.True(high < low);
    }

    [Fact]
    public void GetFishById_ValidId_ReturnsFish()
    {
        var fish = FishingSystem.GetFishById("fish_common_1");
        Assert.NotNull(fish);
        Assert.Equal("フナ", fish!.Name);
    }

    #endregion
}
