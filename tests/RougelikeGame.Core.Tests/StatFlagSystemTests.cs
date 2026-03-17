using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.24 能力値フラグシステムのテスト
/// </summary>
public class StatFlagSystemTests
{
    [Fact]
    public void IsActive_HighSTR_ReturnsTrueForHerculean()
    {
        var stats = new Dictionary<string, int> { ["STR"] = 25 };
        Assert.True(StatFlagSystem.IsActive(StatFlag.Herculean, stats));
    }

    [Fact]
    public void IsActive_LowSTR_ReturnsFalseForHerculean()
    {
        var stats = new Dictionary<string, int> { ["STR"] = 20 };
        Assert.False(StatFlagSystem.IsActive(StatFlag.Herculean, stats));
    }

    [Fact]
    public void IsActive_HighCHA_CharismaticAt20()
    {
        var stats = new Dictionary<string, int> { ["CHA"] = 20 };
        Assert.True(StatFlagSystem.IsActive(StatFlag.Charismatic, stats));
    }

    [Fact]
    public void IsActive_MissingStat_ReturnsFalse()
    {
        var stats = new Dictionary<string, int>();
        Assert.False(StatFlagSystem.IsActive(StatFlag.Lucky, stats));
    }

    [Fact]
    public void EvaluateAll_ReturnsAllFlags()
    {
        var stats = new Dictionary<string, int>
        {
            ["STR"] = 30, ["INT"] = 30, ["PER"] = 30, ["AGI"] = 30,
            ["CHA"] = 25, ["LUK"] = 25, ["VIT"] = 30, ["DEX"] = 30, ["MND"] = 30
        };
        var results = StatFlagSystem.EvaluateAll(stats);
        Assert.Equal(9, results.Count);
        Assert.All(results, r => Assert.True(r.IsActive));
    }

    [Fact]
    public void EvaluateAll_NoStats_AllInactive()
    {
        var stats = new Dictionary<string, int>();
        var results = StatFlagSystem.EvaluateAll(stats);
        Assert.Equal(9, results.Count);
        Assert.All(results, r => Assert.False(r.IsActive));
    }

    [Fact]
    public void GetFlagName_ReturnsJapaneseName()
    {
        Assert.Equal("怪力", StatFlagSystem.GetFlagName(StatFlag.Herculean));
        Assert.Equal("博識", StatFlagSystem.GetFlagName(StatFlag.Erudite));
    }

    [Fact]
    public void GetThreshold_ReturnsCorrectValues()
    {
        var threshold = StatFlagSystem.GetThreshold(StatFlag.Herculean);
        Assert.NotNull(threshold);
        Assert.Equal("STR", threshold!.Value.StatName);
        Assert.Equal(25, threshold!.Value.Threshold);
    }

    [Fact]
    public void GetAllDefinitions_Returns9Items()
    {
        Assert.Equal(9, StatFlagSystem.GetAllDefinitions().Count);
    }
}
