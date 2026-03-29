using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 採取システム＋釣りシステムテスト
/// テスト数: 20件
/// </summary>
public class GatheringFishingSystemTests
{
    // ============================================================
    // GatheringSystem テスト
    // ============================================================

    [Fact]
    public void Gathering_GetNode_Herb_ReturnsNode()
    {
        var node = GatheringSystem.GetNode(GatheringType.Herb);
        Assert.NotNull(node);
        Assert.Equal("薬草採取", node.Name);
        Assert.True(node.PossibleItems.Length > 0);
    }

    [Fact]
    public void Gathering_GetNode_Invalid_ReturnsNull()
    {
        var node = GatheringSystem.GetNode((GatheringType)999);
        Assert.Null(node);
    }

    [Fact]
    public void Gathering_GetAllNodes_ReturnsAllTypes()
    {
        var nodes = GatheringSystem.GetAllNodes();
        Assert.True(nodes.Count >= 5);
        Assert.Contains(GatheringType.Herb, nodes.Keys);
        Assert.Contains(GatheringType.Mining, nodes.Keys);
        Assert.Contains(GatheringType.Fishing, nodes.Keys);
    }

    [Theory]
    [InlineData(GatheringType.Herb, 1, Season.Spring)]
    [InlineData(GatheringType.Mining, 5, Season.Winter)]
    public void Gathering_CalculateSuccessRate_ReturnsValidRange(GatheringType type, int level, Season season)
    {
        float rate = GatheringSystem.CalculateSuccessRate(type, level, season);
        Assert.InRange(rate, 0.1f, 0.95f);
    }

    [Fact]
    public void Gathering_CalculateSuccessRate_BestSeason_HigherRate()
    {
        // 薬草は春がベストシーズン
        float springRate = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 3, Season.Spring);
        float winterRate = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 3, Season.Winter);
        Assert.True(springRate > winterRate);
    }

    [Fact]
    public void Gathering_CalculateRareItemChance_InRange()
    {
        float chance = GatheringSystem.CalculateRareItemChance(5, 0.5f);
        Assert.InRange(chance, 0.01f, 0.3f);
    }

    [Fact]
    public void Gathering_CalculateGatheringDuration_DecreasesWithLevel()
    {
        int lowLevelDuration = GatheringSystem.CalculateGatheringDuration(GatheringType.Herb, 1);
        int highLevelDuration = GatheringSystem.CalculateGatheringDuration(GatheringType.Herb, 10);
        Assert.True(highLevelDuration <= lowLevelDuration);
    }

    [Fact]
    public void Gathering_CanGather_LevelTooLow_ReturnsFalse()
    {
        // 鉱石は必要レベル3
        Assert.False(GatheringSystem.CanGather(GatheringType.Mining, 1));
    }

    [Fact]
    public void Gathering_CanGather_SufficientLevel_ReturnsTrue()
    {
        Assert.True(GatheringSystem.CanGather(GatheringType.Herb, 1));
        Assert.True(GatheringSystem.CanGather(GatheringType.Mining, 5));
    }

    // ============================================================
    // FishingSystem テスト
    // ============================================================

    [Fact]
    public void Fishing_GetAllFish_ReturnsEntries()
    {
        var fish = FishingSystem.GetAllFish();
        Assert.True(fish.Count >= 9);
    }

    [Fact]
    public void Fishing_GetAvailableFish_FiltersCorrectly()
    {
        var available = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 5);
        Assert.NotEmpty(available);
        Assert.All(available, f =>
        {
            Assert.Contains(Season.Spring, f.ActiveSeasons);
            Assert.Contains(TimePeriod.Morning, f.ActiveTimes);
        });
    }

    [Fact]
    public void Fishing_GetAvailableFish_HighLevel_MoreFish()
    {
        var lowLevel = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 1);
        var highLevel = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 10);
        Assert.True(highLevel.Count >= lowLevel.Count);
    }

    [Fact]
    public void Fishing_CalculateCatchRate_InValidRange()
    {
        float rate = FishingSystem.CalculateCatchRate(3, 5, 0.5f);
        Assert.InRange(rate, 0.05f, 0.95f);
    }

    [Fact]
    public void Fishing_CalculateCatchRate_HigherRarity_LowerRate()
    {
        float easyRate = FishingSystem.CalculateCatchRate(1, 5, 0f);
        float hardRate = FishingSystem.CalculateCatchRate(5, 5, 0f);
        Assert.True(easyRate > hardRate);
    }

    [Fact]
    public void Fishing_CalculateJunkRate_DecreasesWithLevel()
    {
        float lowLevel = FishingSystem.CalculateJunkRate(1);
        float highLevel = FishingSystem.CalculateJunkRate(10);
        Assert.True(highLevel < lowLevel);
    }

    [Fact]
    public void Fishing_CalculateTreasureRate_InValidRange()
    {
        float rate = FishingSystem.CalculateTreasureRate(5, 0.5f);
        Assert.InRange(rate, 0.01f, 0.1f);
    }

    [Fact]
    public void Fishing_GetFishById_ReturnsCorrectFish()
    {
        var fish = FishingSystem.GetFishById("fish_common_1");
        Assert.NotNull(fish);
        Assert.Equal("フナ", fish.Name);
    }

    [Fact]
    public void Fishing_GetFishById_InvalidId_ReturnsNull()
    {
        Assert.Null(FishingSystem.GetFishById("nonexistent"));
    }

    [Fact]
    public void Fishing_LegendaryFish_RequiresHighLevel()
    {
        var legendary = FishingSystem.GetFishById("fish_legendary");
        Assert.NotNull(legendary);
        Assert.True(legendary.MinFishingLevel >= 10);
        Assert.Equal(5, legendary.Rarity);
    }

    [Fact]
    public void Fishing_JunkAndTreasure_HaveZeroRarity()
    {
        var junk = FishingSystem.GetFishById("fish_junk");
        var treasure = FishingSystem.GetFishById("fish_treasure");
        Assert.NotNull(junk);
        Assert.NotNull(treasure);
        Assert.Equal(0, junk.Rarity);
        Assert.Equal(0, treasure.Rarity);
    }
}
