using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class EncyclopediaSystemTests
{
    [Fact]
    public void RegisterEntry_AddsEntry()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "スライム", 3,
            new Dictionary<int, string> { [1] = "柔らかい生物", [2] = "物理に弱い", [3] = "酸で攻撃する" });
        Assert.Equal(1, system.TotalEntries);
    }

    [Fact]
    public void IncrementDiscovery_IncreasesLevel()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "スライム", 3,
            new Dictionary<int, string> { [1] = "情報1" });
        Assert.True(system.IncrementDiscovery("mon_01"));
    }

    [Fact]
    public void IncrementDiscovery_AtMaxLevel_ReturnsFalse()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "スライム", 1,
            new Dictionary<int, string> { [1] = "情報1" });
        system.IncrementDiscovery("mon_01");
        Assert.False(system.IncrementDiscovery("mon_01"));
    }

    [Fact]
    public void GetCurrentDescription_Undiscovered_ReturnsQuestionMarks()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "スライム", 3,
            new Dictionary<int, string> { [1] = "情報1" });
        Assert.Equal("???", system.GetCurrentDescription("mon_01"));
    }

    [Fact]
    public void GetDiscoveryRate_CalculatesCorrectly()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1, new Dictionary<int, string> { [1] = "1" });
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_02", "B", 1, new Dictionary<int, string> { [1] = "1" });
        system.IncrementDiscovery("mon_01");
        Assert.Equal(0.5f, system.GetDiscoveryRate(EncyclopediaCategory.Monster));
    }

    [Fact]
    public void GetByCategory_FiltersCorrectly()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1, new Dictionary<int, string> { [1] = "1" });
        system.RegisterEntry(EncyclopediaCategory.Item, "item_01", "B", 1, new Dictionary<int, string> { [1] = "1" });
        Assert.Single(system.GetByCategory(EncyclopediaCategory.Monster));
        Assert.Single(system.GetByCategory(EncyclopediaCategory.Item));
    }
}
