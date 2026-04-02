using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// GatheringSystem（採取システム）のテスト
/// </summary>
public class GatheringSystemTests
{
    // --- GetNode ---

    [Fact]
    public void GetNode_Herb_ReturnsHerbNode()
    {
        var node = GatheringSystem.GetNode(GatheringType.Herb);
        Assert.NotNull(node);
        Assert.Equal(GatheringType.Herb, node.Type);
        Assert.Equal("薬草採取", node.Name);
    }

    [Fact]
    public void GetNode_Mining_ReturnsCorrectRequiredLevel()
    {
        var node = GatheringSystem.GetNode(GatheringType.Mining);
        Assert.NotNull(node);
        Assert.Equal(3, node.RequiredLevel);
        Assert.Equal(ProficiencyCategory.Smithing, node.RequiredProficiency);
    }

    [Fact]
    public void GetNode_Fishing_ReturnsBestSeasons()
    {
        var node = GatheringSystem.GetNode(GatheringType.Fishing);
        Assert.NotNull(node);
        Assert.Contains(Season.Spring, node.BestSeasons);
        Assert.Contains(Season.Summer, node.BestSeasons);
    }

    [Fact]
    public void GetNode_InvalidType_ReturnsNull()
    {
        var node = GatheringSystem.GetNode((GatheringType)999);
        Assert.Null(node);
    }

    // --- GetAllNodes ---

    [Fact]
    public void GetAllNodes_ReturnsFiveTypes()
    {
        var nodes = GatheringSystem.GetAllNodes();
        Assert.Equal(5, nodes.Count);
        Assert.True(nodes.ContainsKey(GatheringType.Herb));
        Assert.True(nodes.ContainsKey(GatheringType.Foraging));
    }

    // --- CalculateSuccessRate ---

    [Fact]
    public void CalculateSuccessRate_BaseProficiency_ReturnsBaseRate()
    {
        // 熟練度0、季節ボーナスなし → 0.3f（修正後: baseRate=0.3, Miningは季節ボーナスなし）
        float rate = GatheringSystem.CalculateSuccessRate(GatheringType.Mining, 0, Season.Spring);
        Assert.Equal(0.3f, rate);
    }

    [Fact]
    public void CalculateSuccessRate_WithSeasonBonus_IncreasesRate()
    {
        // 薬草は春がベストシーズン → +0.15f
        float rateSpring = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 0, Season.Spring);
        float rateWinter = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 0, Season.Winter);
        Assert.True(rateSpring > rateWinter);
    }

    [Fact]
    public void CalculateSuccessRate_HighProficiency_ClampedToMax()
    {
        // 非常に高い熟練度 → 0.95fに制限
        float rate = GatheringSystem.CalculateSuccessRate(GatheringType.Herb, 100, Season.Spring);
        Assert.Equal(0.95f, rate);
    }

    [Fact]
    public void CalculateSuccessRate_InvalidType_ReturnsZero()
    {
        float rate = GatheringSystem.CalculateSuccessRate((GatheringType)999, 10, Season.Spring);
        Assert.Equal(0f, rate);
    }

    // --- CalculateRareItemChance ---

    [Fact]
    public void CalculateRareItemChance_ZeroValues_ReturnsBaseChance()
    {
        float chance = GatheringSystem.CalculateRareItemChance(0, 0f);
        Assert.Equal(0.05f, chance);
    }

    [Fact]
    public void CalculateRareItemChance_HighValues_ClampedToMax()
    {
        float chance = GatheringSystem.CalculateRareItemChance(100, 10f);
        Assert.Equal(0.3f, chance);
    }

    [Fact]
    public void CalculateRareItemChance_NegativeLuck_ClampedToMin()
    {
        float chance = GatheringSystem.CalculateRareItemChance(0, -10f);
        Assert.Equal(0.01f, chance);
    }

    // --- CalculateGatheringDuration ---

    [Fact]
    public void CalculateGatheringDuration_ZeroProficiency_ReturnsBaseDuration()
    {
        // 薬草のBaseDuration=3、熟練度0 → 3
        int duration = GatheringSystem.CalculateGatheringDuration(GatheringType.Herb, 0);
        Assert.Equal(3, duration);
    }

    [Fact]
    public void CalculateGatheringDuration_HighProficiency_MinimumOne()
    {
        // 非常に高い熟練度でも最低1
        int duration = GatheringSystem.CalculateGatheringDuration(GatheringType.Herb, 100);
        Assert.Equal(1, duration);
    }

    [Fact]
    public void CalculateGatheringDuration_InvalidType_ReturnsTen()
    {
        int duration = GatheringSystem.CalculateGatheringDuration((GatheringType)999, 5);
        Assert.Equal(10, duration);
    }

    // --- CanGather ---

    [Fact]
    public void CanGather_SufficientLevel_ReturnsTrue()
    {
        // 鉱石採掘はRequiredLevel=3
        Assert.True(GatheringSystem.CanGather(GatheringType.Mining, 3));
    }

    [Fact]
    public void CanGather_InsufficientLevel_ReturnsFalse()
    {
        Assert.False(GatheringSystem.CanGather(GatheringType.Mining, 2));
    }

    [Fact]
    public void CanGather_InvalidType_ReturnsFalse()
    {
        Assert.False(GatheringSystem.CanGather((GatheringType)999, 100));
    }
}
