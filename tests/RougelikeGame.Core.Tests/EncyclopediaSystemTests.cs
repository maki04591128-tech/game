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

    // α.26e: 発見度段階名テスト
    [Fact]
    public void GetDiscoveryStageName_ReturnsCorrectNames()
    {
        Assert.Equal("未発見", EncyclopediaSystem.GetDiscoveryStageName(0));
        Assert.Equal("遭遇", EncyclopediaSystem.GetDiscoveryStageName(1));
        Assert.Equal("調査", EncyclopediaSystem.GetDiscoveryStageName(2));
        Assert.Equal("熟知", EncyclopediaSystem.GetDiscoveryStageName(3));
        Assert.Equal("精通", EncyclopediaSystem.GetDiscoveryStageName(4));
        Assert.Equal("極致", EncyclopediaSystem.GetDiscoveryStageName(5));
    }

    [Fact]
    public void GetDiscoveryStageName_InvalidLevel_ReturnsFallback()
    {
        Assert.Equal("Lv.9", EncyclopediaSystem.GetDiscoveryStageName(9));
    }

    // α.26c: コンプリートボーナステスト
    [Fact]
    public void IsCategoryComplete_WhenAllAtMaxLevel_ReturnsTrue()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("mon_01");

        Assert.True(system.IsCategoryComplete(EncyclopediaCategory.Monster));
    }

    [Fact]
    public void IsCategoryComplete_WhenNotAllAtMaxLevel_ReturnsFalse()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 2,
            new Dictionary<int, string> { [1] = "info1", [2] = "info2" });
        system.IncrementDiscovery("mon_01"); // level 1/2

        Assert.False(system.IsCategoryComplete(EncyclopediaCategory.Monster));
    }

    [Fact]
    public void IsCategoryComplete_EmptyCategory_ReturnsFalse()
    {
        var system = new EncyclopediaSystem();
        Assert.False(system.IsCategoryComplete(EncyclopediaCategory.Monster));
    }

    [Fact]
    public void IsAllComplete_WhenAllCategoriesComplete_ReturnsTrue()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.RegisterEntry(EncyclopediaCategory.Item, "item_01", "B", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("mon_01");
        system.IncrementDiscovery("item_01");

        Assert.True(system.IsAllComplete());
    }

    [Fact]
    public void GetMonsterCompleteDamageBonus_WhenMonsterComplete_Returns5Percent()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("mon_01");

        Assert.Equal(0.05f, system.GetMonsterCompleteDamageBonus());
    }

    [Fact]
    public void GetMonsterCompleteDamageBonus_WhenNotComplete_Returns0()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 2,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("mon_01"); // level 1/2

        Assert.Equal(0.0f, system.GetMonsterCompleteDamageBonus());
    }

    [Fact]
    public void GetRegionCompleteShopDiscount_WhenRegionComplete_Returns10Percent()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Region, "reg_01", "A", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("reg_01");

        Assert.Equal(0.10f, system.GetRegionCompleteShopDiscount());
    }

    [Fact]
    public void GetActiveCompletionBonuses_WhenMonsterComplete_ContainsMonsterBonus()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "mon_01", "A", 1,
            new Dictionary<int, string> { [1] = "info" });
        system.IncrementDiscovery("mon_01");

        var bonuses = system.GetActiveCompletionBonuses();
        Assert.Contains(bonuses, b => b.BonusId == "monster_complete");
    }
}
