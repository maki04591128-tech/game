using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Entities;

namespace RougelikeGame.Core.Tests;

#region 1. BlackMarketSystem Tests

public class Phase7Expansion_BlackMarketSystemTests
{
    [Theory]
    [InlineData(-20, true)]
    [InlineData(-21, true)]
    [InlineData(-100, true)]
    [InlineData(-19, false)]
    [InlineData(0, false)]
    [InlineData(50, false)]
    public void CanAccess_KarmaThreshold(int karma, bool expected)
    {
        Assert.Equal(expected, BlackMarketSystem.CanAccess(karma));
    }

    [Fact]
    public void GetAllItems_Returns5Items()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public void GetAllItems_ContainsStolenChalice()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.Contains(items, i => i.Name == "盗まれた聖杯");
    }

    [Fact]
    public void GetAllItems_ContainsForbiddenBook()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.Contains(items, i => i.Name == "禁忌の魔導書");
    }

    [Fact]
    public void GetAllItems_AllHavePositivePrice()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.All(items, i => Assert.True(i.Price > 0));
    }

    [Fact]
    public void GetAllItems_AllHaveNegativeRequiredKarma()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.All(items, i => Assert.True(i.RequiredKarma < 0));
    }

    [Fact]
    public void GetAllItems_DetectionRiskBetween0And1()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.All(items, i =>
        {
            Assert.InRange(i.DetectionRisk, 0f, 1f);
        });
    }

    [Fact]
    public void GetAvailableItems_KarmaAboveThreshold_Empty()
    {
        var items = BlackMarketSystem.GetAvailableItems(0);
        Assert.Empty(items);
    }

    [Fact]
    public void GetAvailableItems_KarmaMinus20_ReturnsMatchingItems()
    {
        var items = BlackMarketSystem.GetAvailableItems(-20);
        Assert.NotEmpty(items);
        Assert.All(items, i => Assert.True(-20 <= i.RequiredKarma));
    }

    [Fact]
    public void GetAvailableItems_KarmaMinus100_ReturnsAll()
    {
        var items = BlackMarketSystem.GetAvailableItems(-100);
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public void GetAvailableItems_KarmaMinus30_IncludesAssassinTools()
    {
        var items = BlackMarketSystem.GetAvailableItems(-30);
        Assert.Contains(items, i => i.Category == BlackMarketCategory.AssassinTools);
    }

    [Fact]
    public void GetAvailableItems_KarmaMinus10_ExcludesInformation()
    {
        // カルマ-10ではCanAccessがfalseなので空
        var items = BlackMarketSystem.GetAvailableItems(-10);
        Assert.Empty(items);
    }

    [Theory]
    [InlineData(0.3f, 0, 0, 0.3f)]
    [InlineData(0.3f, 10, 10, 0.2f)]
    [InlineData(0.3f, 50, 50, 0.05f)]
    [InlineData(0.5f, 0, 0, 0.5f)]
    public void CalculateDetectionRisk_DexLuckReduction(float baseRisk, int dex, int luck, float expected)
    {
        float result = BlackMarketSystem.CalculateDetectionRisk(baseRisk, dex, luck);
        Assert.Equal(expected, result, 2);
    }

    [Fact]
    public void CalculateDetectionRisk_ClampsToMinimum005()
    {
        float result = BlackMarketSystem.CalculateDetectionRisk(0.1f, 100, 100);
        Assert.Equal(0.05f, result);
    }

    [Fact]
    public void CalculateDetectionRisk_ClampsToMax1()
    {
        float result = BlackMarketSystem.CalculateDetectionRisk(1.5f, 0, 0);
        Assert.Equal(1.0f, result);
    }

    [Theory]
    [InlineData(BlackMarketCategory.StolenGoods, "盗品")]
    [InlineData(BlackMarketCategory.ForbiddenItems, "禁忌アイテム")]
    [InlineData(BlackMarketCategory.AssassinTools, "暗殺道具")]
    [InlineData(BlackMarketCategory.Information, "情報")]
    public void GetCategoryName_ReturnsCorrectJapanese(BlackMarketCategory cat, string expected)
    {
        Assert.Equal(expected, BlackMarketSystem.GetCategoryName(cat));
    }

    [Fact]
    public void GetCategoryName_UnknownReturnsDefault()
    {
        Assert.Equal("不明", BlackMarketSystem.GetCategoryName((BlackMarketCategory)999));
    }

    [Fact]
    public void GetAvailableItems_KarmaMinus50_IncludesCursedCharm()
    {
        var items = BlackMarketSystem.GetAvailableItems(-50);
        Assert.Contains(items, i => i.Name == "呪いの護符");
    }

    [Fact]
    public void Items_CategoryDistribution()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.Equal(1, items.Count(i => i.Category == BlackMarketCategory.StolenGoods));
        Assert.Equal(2, items.Count(i => i.Category == BlackMarketCategory.ForbiddenItems));
        Assert.Equal(1, items.Count(i => i.Category == BlackMarketCategory.AssassinTools));
        Assert.Equal(1, items.Count(i => i.Category == BlackMarketCategory.Information));
    }
}

#endregion

#region 2. DungeonShortcutSystem Tests

public class Phase7Expansion_DungeonShortcutSystemTests
{
    private static DungeonShortcutSystem CreateWithVisited(params (string dungeon, int floor)[] floors)
    {
        var sys = new DungeonShortcutSystem();
        foreach (var (dungeon, floor) in floors)
            sys.MarkFloorVisited(dungeon, floor);
        return sys;
    }

    [Fact]
    public void UnlockShortcut_FirstTime_ReturnsTrue()
    {
        var sys = CreateWithVisited(("dungeon_a", 1), ("dungeon_a", 5));
        Assert.True(sys.UnlockShortcut("dungeon_a", 1, 5));
    }

    [Fact]
    public void UnlockShortcut_Duplicate_ReturnsFalse()
    {
        var sys = CreateWithVisited(("dungeon_a", 1), ("dungeon_a", 5));
        sys.UnlockShortcut("dungeon_a", 1, 5);
        Assert.False(sys.UnlockShortcut("dungeon_a", 1, 5));
    }

    [Fact]
    public void IsUnlocked_BeforeUnlock_ReturnsFalse()
    {
        var sys = new DungeonShortcutSystem();
        Assert.False(sys.IsUnlocked("dungeon_a", 1, 5));
    }

    [Fact]
    public void IsUnlocked_AfterUnlock_ReturnsTrue()
    {
        var sys = CreateWithVisited(("dungeon_a", 1), ("dungeon_a", 5));
        sys.UnlockShortcut("dungeon_a", 1, 5);
        Assert.True(sys.IsUnlocked("dungeon_a", 1, 5));
    }

    [Fact]
    public void GetShortcuts_EmptyDungeon_ReturnsEmpty()
    {
        var sys = new DungeonShortcutSystem();
        Assert.Empty(sys.GetShortcuts("unknown"));
    }

    [Fact]
    public void GetShortcuts_ReturnsDungeonSpecific()
    {
        var sys = CreateWithVisited(
            ("dungeon_a", 1), ("dungeon_a", 5), ("dungeon_a", 10),
            ("dungeon_b", 1), ("dungeon_b", 3));
        sys.UnlockShortcut("dungeon_a", 1, 5);
        sys.UnlockShortcut("dungeon_a", 5, 10);
        sys.UnlockShortcut("dungeon_b", 1, 3);
        Assert.Equal(2, sys.GetShortcuts("dungeon_a").Count);
        Assert.Equal(1, sys.GetShortcuts("dungeon_b").Count);
    }

    [Fact]
    public void TotalUnlocked_CountsAll()
    {
        var sys = CreateWithVisited(
            ("a", 1), ("a", 5), ("a", 10),
            ("b", 1), ("b", 3));
        Assert.Equal(0, sys.TotalUnlocked);
        sys.UnlockShortcut("a", 1, 5);
        sys.UnlockShortcut("a", 5, 10);
        sys.UnlockShortcut("b", 1, 3);
        Assert.Equal(3, sys.TotalUnlocked);
    }

    [Fact]
    public void GetDeepestAccessibleFloor_NoShortcuts_Returns1()
    {
        var sys = new DungeonShortcutSystem();
        Assert.Equal(1, sys.GetDeepestAccessibleFloor("dungeon_a"));
    }

    [Fact]
    public void GetDeepestAccessibleFloor_MultipleShortcuts_ReturnsMax()
    {
        var sys = CreateWithVisited(
            ("dungeon_a", 1), ("dungeon_a", 5), ("dungeon_a", 10), ("dungeon_a", 15));
        sys.UnlockShortcut("dungeon_a", 1, 5);
        sys.UnlockShortcut("dungeon_a", 5, 10);
        sys.UnlockShortcut("dungeon_a", 10, 15);
        Assert.Equal(15, sys.GetDeepestAccessibleFloor("dungeon_a"));
    }

    [Fact]
    public void GetDeepestAccessibleFloor_DifferentDungeons_Independent()
    {
        var sys = CreateWithVisited(
            ("dungeon_a", 1), ("dungeon_a", 20),
            ("dungeon_b", 1), ("dungeon_b", 5));
        sys.UnlockShortcut("dungeon_a", 1, 20);
        sys.UnlockShortcut("dungeon_b", 1, 5);
        Assert.Equal(20, sys.GetDeepestAccessibleFloor("dungeon_a"));
        Assert.Equal(5, sys.GetDeepestAccessibleFloor("dungeon_b"));
    }

    [Fact]
    public void UnlockShortcut_DifferentDungeonSameFloors_BothUnlocked()
    {
        var sys = CreateWithVisited(
            ("dungeon_a", 1), ("dungeon_a", 5),
            ("dungeon_b", 1), ("dungeon_b", 5));
        Assert.True(sys.UnlockShortcut("dungeon_a", 1, 5));
        Assert.True(sys.UnlockShortcut("dungeon_b", 1, 5));
        Assert.Equal(2, sys.TotalUnlocked);
    }

    [Fact]
    public void GetShortcuts_ContainsCorrectFloorPairs()
    {
        var sys = CreateWithVisited(("d", 1), ("d", 5), ("d", 10));
        sys.UnlockShortcut("d", 1, 5);
        sys.UnlockShortcut("d", 5, 10);
        var shortcuts = sys.GetShortcuts("d");
        Assert.Contains((1, 5), shortcuts);
        Assert.Contains((5, 10), shortcuts);
    }

    [Fact]
    public void UnlockShortcut_WithoutVisiting_ReturnsFalse()
    {
        var sys = new DungeonShortcutSystem();
        Assert.False(sys.UnlockShortcut("dungeon_a", 1, 5));
    }
}

#endregion

#region 3. ModularHudSystem Tests

public class Phase7Expansion_ModularHudSystemTests
{
    [Fact]
    public void Constructor_AllElementsVisible()
    {
        var sys = new ModularHudSystem();
        foreach (HudElement elem in Enum.GetValues<HudElement>())
        {
            var config = sys.GetConfig(elem);
            Assert.NotNull(config);
            Assert.True(config!.IsVisible);
        }
    }

    [Fact]
    public void Constructor_AllElementsScale1()
    {
        var sys = new ModularHudSystem();
        foreach (HudElement elem in Enum.GetValues<HudElement>())
        {
            var config = sys.GetConfig(elem);
            Assert.Equal(1.0f, config!.Scale);
        }
    }

    [Fact]
    public void Constructor_AllElementsPosition00()
    {
        var sys = new ModularHudSystem();
        foreach (HudElement elem in Enum.GetValues<HudElement>())
        {
            var config = sys.GetConfig(elem);
            Assert.Equal(0, config!.PositionX);
            Assert.Equal(0, config!.PositionY);
        }
    }

    [Fact]
    public void SetVisibility_HideElement()
    {
        var sys = new ModularHudSystem();
        sys.SetVisibility(HudElement.HpBar, false);
        Assert.False(sys.GetConfig(HudElement.HpBar)!.IsVisible);
    }

    [Fact]
    public void SetVisibility_ShowHiddenElement()
    {
        var sys = new ModularHudSystem();
        sys.SetVisibility(HudElement.HpBar, false);
        sys.SetVisibility(HudElement.HpBar, true);
        Assert.True(sys.GetConfig(HudElement.HpBar)!.IsVisible);
    }

    [Fact]
    public void SetPosition_UpdatesCoordinates()
    {
        var sys = new ModularHudSystem();
        sys.SetPosition(HudElement.MiniMap, 100, 200);
        var config = sys.GetConfig(HudElement.MiniMap);
        Assert.Equal(100, config!.PositionX);
        Assert.Equal(200, config!.PositionY);
    }

    [Fact]
    public void SetScale_NormalValue()
    {
        var sys = new ModularHudSystem();
        sys.SetScale(HudElement.MessageLog, 1.5f);
        Assert.Equal(1.5f, sys.GetConfig(HudElement.MessageLog)!.Scale);
    }

    [Fact]
    public void SetScale_ClampsToMin05()
    {
        var sys = new ModularHudSystem();
        sys.SetScale(HudElement.HpBar, 0.1f);
        Assert.Equal(0.5f, sys.GetConfig(HudElement.HpBar)!.Scale);
    }

    [Fact]
    public void SetScale_ClampsToMax20()
    {
        var sys = new ModularHudSystem();
        sys.SetScale(HudElement.HpBar, 5.0f);
        Assert.Equal(2.0f, sys.GetConfig(HudElement.HpBar)!.Scale);
    }

    [Fact]
    public void GetVisibleCount_AllVisible()
    {
        var sys = new ModularHudSystem();
        int total = Enum.GetValues<HudElement>().Length;
        Assert.Equal(total, sys.GetVisibleCount());
    }

    [Fact]
    public void GetVisibleCount_OneHidden()
    {
        var sys = new ModularHudSystem();
        sys.SetVisibility(HudElement.HpBar, false);
        int total = Enum.GetValues<HudElement>().Length;
        Assert.Equal(total - 1, sys.GetVisibleCount());
    }

    [Fact]
    public void GetVisibleCount_AllHidden()
    {
        var sys = new ModularHudSystem();
        foreach (HudElement elem in Enum.GetValues<HudElement>())
            sys.SetVisibility(elem, false);
        Assert.Equal(0, sys.GetVisibleCount());
    }

    [Theory]
    [InlineData(HudElement.HpBar, "HPバー")]
    [InlineData(HudElement.MpBar, "MPバー")]
    [InlineData(HudElement.MiniMap, "ミニマップ")]
    [InlineData(HudElement.MessageLog, "メッセージログ")]
    [InlineData(HudElement.StatusInfo, "ステータス情報")]
    public void GetElementName_ReturnsCorrectJapanese(HudElement elem, string expected)
    {
        Assert.Equal(expected, ModularHudSystem.GetElementName(elem));
    }

    [Fact]
    public void MaxPresets_Is3()
    {
        Assert.Equal(3, ModularHudSystem.MaxPresets);
    }

    [Fact]
    public void SetPosition_NegativeValues_Allowed()
    {
        var sys = new ModularHudSystem();
        sys.SetPosition(HudElement.HpBar, -10, -20);
        Assert.Equal(-10, sys.GetConfig(HudElement.HpBar)!.PositionX);
        Assert.Equal(-20, sys.GetConfig(HudElement.HpBar)!.PositionY);
    }
}

#endregion

#region 4. RenderOptimizationSystem Tests

public class Phase7Expansion_RenderOptimizationSystemTests
{
    [Fact]
    public void CalculateViewport_CenteredOnPlayer()
    {
        var (minX, minY, maxX, maxY) = RenderOptimizationSystem.CalculateViewport(50, 50, 20, 10);
        Assert.Equal(40, minX);
        Assert.Equal(45, minY);
        Assert.Equal(60, maxX);
        Assert.Equal(55, maxY);
    }

    [Fact]
    public void CalculateViewport_Origin()
    {
        var (minX, minY, maxX, maxY) = RenderOptimizationSystem.CalculateViewport(0, 0, 10, 10);
        Assert.Equal(-5, minX);
        Assert.Equal(-5, minY);
        Assert.Equal(5, maxX);
        Assert.Equal(5, maxY);
    }

    [Fact]
    public void IsInViewport_Inside_ReturnsTrue()
    {
        Assert.True(RenderOptimizationSystem.IsInViewport(5, 5, 0, 0, 10, 10));
    }

    [Fact]
    public void IsInViewport_OnMinBoundary_ReturnsTrue()
    {
        Assert.True(RenderOptimizationSystem.IsInViewport(0, 0, 0, 0, 10, 10));
    }

    [Fact]
    public void IsInViewport_OnMaxBoundary_ReturnsTrue()
    {
        Assert.True(RenderOptimizationSystem.IsInViewport(10, 10, 0, 0, 10, 10));
    }

    [Fact]
    public void IsInViewport_Outside_ReturnsFalse()
    {
        Assert.False(RenderOptimizationSystem.IsInViewport(11, 5, 0, 0, 10, 10));
    }

    [Fact]
    public void IsInViewport_BelowMinX_ReturnsFalse()
    {
        Assert.False(RenderOptimizationSystem.IsInViewport(-1, 5, 0, 0, 10, 10));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(5, 1)]
    [InlineData(10, 1)]
    [InlineData(11, 2)]
    [InlineData(20, 2)]
    [InlineData(21, 4)]
    [InlineData(40, 4)]
    [InlineData(41, 8)]
    [InlineData(60, 8)]
    [InlineData(61, 16)]
    [InlineData(100, 16)]
    public void CalculateUpdateFrequency_DistanceThresholds(int distance, int expected)
    {
        Assert.Equal(expected, RenderOptimizationSystem.CalculateUpdateFrequency(distance));
    }

    [Fact]
    public void CalculateDistance_ChebyshevHorizontal()
    {
        Assert.Equal(10, RenderOptimizationSystem.CalculateDistance(0, 0, 10, 0));
    }

    [Fact]
    public void CalculateDistance_ChebyshevVertical()
    {
        Assert.Equal(10, RenderOptimizationSystem.CalculateDistance(0, 0, 0, 10));
    }

    [Fact]
    public void CalculateDistance_ChebyshevDiagonal()
    {
        Assert.Equal(10, RenderOptimizationSystem.CalculateDistance(0, 0, 10, 10));
    }

    [Fact]
    public void CalculateDistance_SamePosition_Zero()
    {
        Assert.Equal(0, RenderOptimizationSystem.CalculateDistance(5, 5, 5, 5));
    }

    [Fact]
    public void CalculateDistance_NegativeCoords()
    {
        Assert.Equal(20, RenderOptimizationSystem.CalculateDistance(-10, -10, 10, 10));
    }

    [Fact]
    public void ShouldUpdate_FreqZero_AlwaysTrue()
    {
        Assert.True(RenderOptimizationSystem.ShouldUpdate(5, 0));
    }

    [Fact]
    public void ShouldUpdate_Freq1_AlwaysTrue()
    {
        Assert.True(RenderOptimizationSystem.ShouldUpdate(1, 1));
        Assert.True(RenderOptimizationSystem.ShouldUpdate(99, 1));
    }

    [Fact]
    public void ShouldUpdate_Freq2_EveryOtherTurn()
    {
        Assert.True(RenderOptimizationSystem.ShouldUpdate(0, 2));
        Assert.False(RenderOptimizationSystem.ShouldUpdate(1, 2));
        Assert.True(RenderOptimizationSystem.ShouldUpdate(2, 2));
        Assert.False(RenderOptimizationSystem.ShouldUpdate(3, 2));
    }

    [Fact]
    public void ShouldUpdate_Freq4_EveryFourthTurn()
    {
        Assert.True(RenderOptimizationSystem.ShouldUpdate(0, 4));
        Assert.False(RenderOptimizationSystem.ShouldUpdate(1, 4));
        Assert.False(RenderOptimizationSystem.ShouldUpdate(2, 4));
        Assert.False(RenderOptimizationSystem.ShouldUpdate(3, 4));
        Assert.True(RenderOptimizationSystem.ShouldUpdate(4, 4));
    }
}

#endregion

#region 5. StartingMapResolver Tests

public class Phase7Expansion_StartingMapResolverTests
{
    [Theory]
    [InlineData(Background.Soldier, "capital_barracks")]
    [InlineData(Background.Scholar, "capital_academy")]
    [InlineData(Background.Merchant, "capital_market")]
    [InlineData(Background.Peasant, "capital_slums")]
    [InlineData(Background.Noble, "capital_manor")]
    [InlineData(Background.Criminal, "capital_prison")]
    [InlineData(Background.Priest, "capital_cathedral")]
    [InlineData(Background.Penitent, "capital_monastery")]
    [InlineData(Background.Wanderer, "wanderer_camp")]
    public void Resolve_BackgroundPriority(Background bg, string expected)
    {
        Assert.Equal(expected, StartingMapResolver.Resolve(Race.Human, bg));
    }

    [Theory]
    [InlineData(Race.Human, "capital_guild")]
    [InlineData(Race.Elf, "forest_village")]
    [InlineData(Race.Dwarf, "mountain_hold")]
    [InlineData(Race.Halfling, "coast_port")]
    [InlineData(Race.Orc, "mountain_hold")]
    [InlineData(Race.Beastfolk, "forest_village")]
    [InlineData(Race.Undead, "underground_ruins")]
    [InlineData(Race.Demon, "dark_sanctuary")]
    [InlineData(Race.FallenAngel, "fallen_temple")]
    [InlineData(Race.Slime, "swamp_den")]
    public void Resolve_AdventurerBackground_UsesRace(Race race, string expected)
    {
        Assert.Equal(expected, StartingMapResolver.Resolve(race, Background.Adventurer));
    }

    [Theory]
    [InlineData("capital_guild", "王都・冒険者ギルド")]
    [InlineData("capital_barracks", "王都・兵舎")]
    [InlineData("capital_academy", "王都・学院")]
    [InlineData("capital_market", "王都・市場通り")]
    [InlineData("capital_slums", "王都・貧民街")]
    [InlineData("capital_manor", "王都・貴族邸")]
    [InlineData("capital_cathedral", "王都・大聖堂")]
    [InlineData("capital_prison", "王都・牢獄")]
    [InlineData("capital_monastery", "王都・修道院")]
    [InlineData("forest_village", "森の集落")]
    [InlineData("mountain_hold", "山岳砦")]
    [InlineData("coast_port", "海岸港町")]
    [InlineData("underground_ruins", "地下遺跡")]
    [InlineData("dark_sanctuary", "暗黒聖域")]
    [InlineData("fallen_temple", "堕天の神殿")]
    [InlineData("swamp_den", "沼地の洞窟")]
    [InlineData("wanderer_camp", "流浪者の野営地")]
    [InlineData("debug_arena", "デバッグアリーナ")]
    public void GetDisplayName_AllMaps(string mapName, string expected)
    {
        Assert.Equal(expected, StartingMapResolver.GetDisplayName(mapName));
    }

    [Fact]
    public void GetDisplayName_Unknown_ReturnsRaw()
    {
        Assert.Equal("unknown_map", StartingMapResolver.GetDisplayName("unknown_map"));
    }

    [Theory]
    [InlineData("forest_village", TerritoryId.Forest)]
    [InlineData("mountain_hold", TerritoryId.Mountain)]
    [InlineData("coast_port", TerritoryId.Coast)]
    [InlineData("capital_guild", TerritoryId.Capital)]
    [InlineData("unknown", TerritoryId.Capital)]
    public void GetStartingTerritory_CorrectMapping(string mapName, TerritoryId expected)
    {
        Assert.Equal(expected, StartingMapResolver.GetStartingTerritory(mapName));
    }

    [Fact]
    public void Resolve_SpecificBgOverridesRace()
    {
        // Elf + Soldier → capital_barracks (background priority)
        Assert.Equal("capital_barracks", StartingMapResolver.Resolve(Race.Elf, Background.Soldier));
    }
}

#endregion

#region 6. SymbolMapEventSystem Tests

public class Phase7Expansion_SymbolMapEventSystemTests
{
    [Fact]
    public void GetAllEvents_Returns10Events()
    {
        Assert.Equal(10, SymbolMapEventSystem.GetAllEvents().Count);
    }

    [Fact]
    public void GetAllEvents_AllHaveId()
    {
        var events = SymbolMapEventSystem.GetAllEvents();
        Assert.All(events, e => Assert.NotEmpty(e.Id));
    }

    [Fact]
    public void GetAllEvents_AllHavePositiveChance()
    {
        var events = SymbolMapEventSystem.GetAllEvents();
        Assert.All(events, e => Assert.True(e.BaseChance > 0));
    }

    [Fact]
    public void GetAvailableEvents_SpringForest_IncludesFairyRing()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Spring, TerritoryId.Forest);
        Assert.Contains(events, e => e.Id == "event_fairy_ring");
    }

    [Fact]
    public void GetAvailableEvents_WinterMountain_IncludesStormShelter()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Winter, TerritoryId.Mountain);
        Assert.Contains(events, e => e.Id == "event_storm_shelter");
    }

    [Fact]
    public void GetAvailableEvents_NoSeasonNoTerritory_IncludesUnrestricted()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Spring, TerritoryId.Capital);
        // Wandering healer has no season/territory restrictions
        Assert.Contains(events, e => e.Id == "event_wandering_healer");
    }

    [Fact]
    public void GetAvailableEvents_SummerCapital_ExcludesMountainOnly()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Summer, TerritoryId.Capital);
        Assert.DoesNotContain(events, e => e.Id == "event_storm_shelter");
    }

    [Fact]
    public void RollEvent_VeryLowRandom_ReturnsEvent()
    {
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Capital, 0.0);
        Assert.NotNull(evt);
    }

    [Fact]
    public void RollEvent_VeryHighRandom_ReturnsNull()
    {
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Capital, 10.0);
        Assert.Null(evt);
    }

    [Fact]
    public void RollEvent_MidRandom_MayReturnEvent()
    {
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Forest, 0.1);
        // With 0.1 random, merchant caravan (0.15 chance) should trigger
        Assert.NotNull(evt);
    }

    [Fact]
    public void GetAvailableEvents_FrontierTerritory_IncludesBanditAmbush()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Spring, TerritoryId.Frontier);
        Assert.Contains(events, e => e.Id == "event_bandit_ambush");
    }

    [Fact]
    public void GetAvailableEvents_WinterSeason_IncludesMonsterStampede()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Winter, TerritoryId.Capital);
        Assert.Contains(events, e => e.Id == "event_monster_stampede");
    }

    [Fact]
    public void GetAvailableEvents_AutumnSeason_ExcludesFairyRing()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Autumn, TerritoryId.Forest);
        Assert.DoesNotContain(events, e => e.Id == "event_fairy_ring");
    }

    [Fact]
    public void AllEvents_UniqueIds()
    {
        var events = SymbolMapEventSystem.GetAllEvents();
        var ids = events.Select(e => e.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}

#endregion

#region 7. TemplateMapSystem Tests

public class Phase7Expansion_TemplateMapSystemTests
{
    [Theory]
    [InlineData(TemplateMapType.BossFloor, "魔王の間", 40, 30, 40)]
    [InlineData(TemplateMapType.Town, "王都メインストリート", 60, 40, 1)]
    [InlineData(TemplateMapType.Ruins, "古代遺跡", 50, 50, 20)]
    [InlineData(TemplateMapType.Tower, "魔導塔", 30, 30, 30)]
    [InlineData(TemplateMapType.SpecialDungeon, "隠しダンジョン", 45, 45, 35)]
    public void GetTemplate_ReturnsCorrectDefinition(TemplateMapType type, string name, int w, int h, int minLevel)
    {
        var t = TemplateMapSystem.GetTemplate(type);
        Assert.NotNull(t);
        Assert.Equal(name, t!.Name);
        Assert.Equal(w, t.Width);
        Assert.Equal(h, t.Height);
        Assert.Equal(minLevel, t.MinLevel);
    }

    [Fact]
    public void GetTemplate_InvalidType_ReturnsNull()
    {
        Assert.Null(TemplateMapSystem.GetTemplate((TemplateMapType)999));
    }

    [Fact]
    public void GetAllTemplates_Returns5()
    {
        Assert.Equal(5, TemplateMapSystem.GetAllTemplates().Count);
    }

    [Fact]
    public void GetAllTemplates_AllHaveDescription()
    {
        var templates = TemplateMapSystem.GetAllTemplates();
        Assert.All(templates, t => Assert.NotEmpty(t.Description));
    }

    [Theory]
    [InlineData(TemplateMapType.Town, 1, true)]
    [InlineData(TemplateMapType.Town, 0, false)]
    [InlineData(TemplateMapType.BossFloor, 40, true)]
    [InlineData(TemplateMapType.BossFloor, 39, false)]
    [InlineData(TemplateMapType.Ruins, 20, true)]
    [InlineData(TemplateMapType.Ruins, 19, false)]
    public void MeetsLevelRequirement_ThresholdBehavior(TemplateMapType type, int level, bool expected)
    {
        Assert.Equal(expected, TemplateMapSystem.MeetsLevelRequirement(type, level));
    }

    [Fact]
    public void MeetsLevelRequirement_InvalidType_ReturnsFalse()
    {
        Assert.False(TemplateMapSystem.MeetsLevelRequirement((TemplateMapType)999, 100));
    }

    [Theory]
    [InlineData(TemplateMapType.BossFloor, "ボスフロア")]
    [InlineData(TemplateMapType.Town, "街")]
    [InlineData(TemplateMapType.Ruins, "遺跡")]
    [InlineData(TemplateMapType.Tower, "塔")]
    [InlineData(TemplateMapType.SpecialDungeon, "特殊ダンジョン")]
    public void GetTypeName_ReturnsJapanese(TemplateMapType type, string expected)
    {
        Assert.Equal(expected, TemplateMapSystem.GetTypeName(type));
    }

    [Fact]
    public void GetTypeName_Unknown_ReturnsDefault()
    {
        Assert.Equal("不明", TemplateMapSystem.GetTypeName((TemplateMapType)999));
    }

    [Fact]
    public void AllTemplates_PositiveDimensions()
    {
        var templates = TemplateMapSystem.GetAllTemplates();
        Assert.All(templates, t =>
        {
            Assert.True(t.Width > 0);
            Assert.True(t.Height > 0);
        });
    }

    [Fact]
    public void AllTemplates_NonNegativeMinLevel()
    {
        var templates = TemplateMapSystem.GetAllTemplates();
        Assert.All(templates, t => Assert.True(t.MinLevel >= 0));
    }
}

#endregion

#region 8. TrapCraftingSystem Tests

public class Phase7Expansion_TrapCraftingSystemTests
{
    [Theory]
    [InlineData(PlayerTrapType.SpikeTrap, "棘罠", 5, 10, 20)]
    [InlineData(PlayerTrapType.PitfallTrap, "落とし穴", 8, 15, 15)]
    [InlineData(PlayerTrapType.ExplosiveTrap, "爆発罠", 15, 30, 40)]
    [InlineData(PlayerTrapType.SleepTrap, "睡眠罠", 10, 20, 0)]
    [InlineData(PlayerTrapType.AlarmTrap, "警報罠", 3, 5, 0)]
    public void GetRecipe_ReturnsCorrectValues(PlayerTrapType type, string name, int cost, int req, int dmg)
    {
        var r = TrapCraftingSystem.GetRecipe(type);
        Assert.NotNull(r);
        Assert.Equal(name, r!.Name);
        Assert.Equal(cost, r.MaterialCost);
        Assert.Equal(req, r.RequiredSmithing);
        Assert.Equal(dmg, r.Damage);
    }

    [Fact]
    public void GetRecipe_AllHaveEffect()
    {
        foreach (PlayerTrapType type in Enum.GetValues<PlayerTrapType>())
        {
            var r = TrapCraftingSystem.GetRecipe(type);
            if (r != null) Assert.NotEmpty(r.Effect);
        }
    }

    [Fact]
    public void GetAllRecipes_Returns5()
    {
        Assert.Equal(5, TrapCraftingSystem.GetAllRecipes().Count);
    }

    [Theory]
    [InlineData(PlayerTrapType.SpikeTrap, 5, 10, true)]
    [InlineData(PlayerTrapType.SpikeTrap, 4, 10, false)]
    [InlineData(PlayerTrapType.SpikeTrap, 5, 9, false)]
    [InlineData(PlayerTrapType.SpikeTrap, 100, 100, true)]
    public void CanCraft_MaterialAndSmithingCheck(PlayerTrapType type, int materials, int smithing, bool expected)
    {
        Assert.Equal(expected, TrapCraftingSystem.CanCraft(type, materials, smithing));
    }

    [Fact]
    public void CanCraft_ExplosiveTrap_HighRequirements()
    {
        Assert.False(TrapCraftingSystem.CanCraft(PlayerTrapType.ExplosiveTrap, 10, 25));
        Assert.True(TrapCraftingSystem.CanCraft(PlayerTrapType.ExplosiveTrap, 15, 30));
    }

    [Fact]
    public void CalculateEfficiency_AtRequiredLevel_Returns1()
    {
        float eff = TrapCraftingSystem.CalculateEfficiency(PlayerTrapType.SpikeTrap, 10);
        Assert.Equal(1.0f, eff);
    }

    [Fact]
    public void CalculateEfficiency_AboveRequired_GreaterThan1()
    {
        float eff = TrapCraftingSystem.CalculateEfficiency(PlayerTrapType.SpikeTrap, 20);
        Assert.True(eff > 1.0f);
    }

    [Fact]
    public void CalculateEfficiency_BelowRequired_Returns1()
    {
        float eff = TrapCraftingSystem.CalculateEfficiency(PlayerTrapType.SpikeTrap, 5);
        Assert.Equal(1.0f, eff);
    }

    [Fact]
    public void CalculateEfficiency_InvalidType_Returns0()
    {
        Assert.Equal(0f, TrapCraftingSystem.CalculateEfficiency((PlayerTrapType)999, 50));
    }

    [Fact]
    public void CanCraft_InvalidType_ReturnsFalse()
    {
        Assert.False(TrapCraftingSystem.CanCraft((PlayerTrapType)999, 100, 100));
    }

    [Fact]
    public void AlarmTrap_LowestRequirements()
    {
        var r = TrapCraftingSystem.GetRecipe(PlayerTrapType.AlarmTrap);
        Assert.Equal(3, r!.MaterialCost);
        Assert.Equal(5, r.RequiredSmithing);
    }

    [Fact]
    public void ExplosiveTrap_HighestDamage()
    {
        var recipes = TrapCraftingSystem.GetAllRecipes().Values;
        var maxDmg = recipes.Max(r => r.Damage);
        Assert.Equal(40, maxDmg);
    }
}

#endregion

#region 9. PriceFluctuationSystem Tests

public class Phase7Expansion_PriceFluctuationSystemTests
{
    [Theory]
    [InlineData(10, 10, 1.0f)]
    [InlineData(10, 20, 2.0f)]
    [InlineData(20, 10, 0.5f)]
    [InlineData(0, 10, 2.0f)]
    [InlineData(10, 0, 0.5f)]
    public void CalculateSupplyDemandModifier_CoreCases(int supply, int demand, float expected)
    {
        Assert.Equal(expected, PriceFluctuationSystem.CalculateSupplyDemandModifier(supply, demand));
    }

    [Fact]
    public void CalculateSupplyDemandModifier_ClampsToRange()
    {
        float high = PriceFluctuationSystem.CalculateSupplyDemandModifier(1, 100);
        Assert.Equal(2.0f, high);
        float low = PriceFluctuationSystem.CalculateSupplyDemandModifier(100, 1);
        Assert.Equal(0.5f, low);
    }

    [Theory]
    [InlineData(TerritoryId.Capital, "weapon", 0.9f)]
    [InlineData(TerritoryId.Capital, "armor", 0.9f)]
    [InlineData(TerritoryId.Forest, "herb", 0.7f)]
    [InlineData(TerritoryId.Forest, "wood", 0.7f)]
    [InlineData(TerritoryId.Mountain, "ore", 0.7f)]
    [InlineData(TerritoryId.Mountain, "gem", 0.8f)]
    [InlineData(TerritoryId.Coast, "fish", 0.7f)]
    [InlineData(TerritoryId.Frontier, "anything", 1.2f)]
    public void GetTerritoryModifier_SpecificCombinations(TerritoryId territory, string category, float expected)
    {
        Assert.Equal(expected, PriceFluctuationSystem.GetTerritoryModifier(territory, category));
    }

    [Fact]
    public void GetTerritoryModifier_DefaultIs1()
    {
        Assert.Equal(1.0f, PriceFluctuationSystem.GetTerritoryModifier(TerritoryId.Capital, "food"));
    }

    [Theory]
    [InlineData(KarmaRank.Saint, true, 0.85f)]
    [InlineData(KarmaRank.Virtuous, true, 0.9f)]
    [InlineData(KarmaRank.Villain, true, 1.2f)]
    [InlineData(KarmaRank.Criminal, true, 1.15f)]
    [InlineData(KarmaRank.Saint, false, 1.1f)]
    [InlineData(KarmaRank.Villain, false, 0.8f)]
    [InlineData(KarmaRank.Neutral, true, 1.0f)]
    [InlineData(KarmaRank.Neutral, false, 1.0f)]
    public void GetKarmaModifier_Values(KarmaRank rank, bool buying, float expected)
    {
        Assert.Equal(expected, PriceFluctuationSystem.GetKarmaModifier(rank, buying));
    }

    [Theory]
    [InlineData(ReputationRank.Revered, true, 0.8f)]
    [InlineData(ReputationRank.Trusted, true, 0.85f)]
    [InlineData(ReputationRank.Friendly, true, 0.9f)]
    [InlineData(ReputationRank.Hostile, true, 1.15f)]
    [InlineData(ReputationRank.Hated, true, 1.3f)]
    [InlineData(ReputationRank.Revered, false, 1.15f)]
    [InlineData(ReputationRank.Hated, false, 0.7f)]
    [InlineData(ReputationRank.Indifferent, true, 1.0f)]
    public void GetReputationModifier_Values(ReputationRank rank, bool buying, float expected)
    {
        Assert.Equal(expected, PriceFluctuationSystem.GetReputationModifier(rank, buying));
    }

    [Fact]
    public void CalculateFinalPrice_BaseCase()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(100, 1.0f, 1.0f, 1.0f, 1.0f, true);
        Assert.Equal(100, price);
    }

    [Fact]
    public void CalculateFinalPrice_Minimum1()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(1, 0.5f, 0.5f, 0.5f, 0.5f, true);
        Assert.True(price >= 1);
    }

    [Fact]
    public void CalculateFinalPrice_AllModifiers()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(100, 1.5f, 0.9f, 0.85f, 0.8f, true);
        Assert.True(price > 0);
    }

    [Fact]
    public void CalculateFinalPrice_SellingAlsoMin1()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(1, 0.5f, 0.5f, 0.5f, 0.5f, false);
        Assert.True(price >= 1);
    }
}

#endregion

#region 10. RelationshipSystem Tests

public class Phase7Expansion_RelationshipSystemTests
{
    [Fact]
    public void SetGetRelation_BasicRoundtrip()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "Human", "Elf", 50);
        Assert.Equal(50, sys.GetRelation(RelationshipType.Racial, "Human", "Elf"));
    }

    [Fact]
    public void GetRelation_Unset_ReturnsZero()
    {
        var sys = new RelationshipSystem();
        Assert.Equal(0, sys.GetRelation(RelationshipType.Personal, "A", "B"));
    }

    [Fact]
    public void SetRelation_ClampsToMinus100()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "A", "B", -200);
        Assert.Equal(-100, sys.GetRelation(RelationshipType.Racial, "A", "B"));
    }

    [Fact]
    public void SetRelation_ClampsTo100()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "A", "B", 200);
        Assert.Equal(100, sys.GetRelation(RelationshipType.Racial, "A", "B"));
    }

    [Fact]
    public void ModifyRelation_AddsToExisting()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Personal, "A", "B", 30);
        sys.ModifyRelation(RelationshipType.Personal, "A", "B", 20);
        Assert.Equal(50, sys.GetRelation(RelationshipType.Personal, "A", "B"));
    }

    [Fact]
    public void ModifyRelation_NegativeDelta()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Personal, "A", "B", 30);
        sys.ModifyRelation(RelationshipType.Personal, "A", "B", -50);
        Assert.Equal(-20, sys.GetRelation(RelationshipType.Personal, "A", "B"));
    }

    [Fact]
    public void ModifyRelation_ClampsAtMax()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Personal, "A", "B", 90);
        sys.ModifyRelation(RelationshipType.Personal, "A", "B", 50);
        Assert.Equal(100, sys.GetRelation(RelationshipType.Personal, "A", "B"));
    }

    [Theory]
    [InlineData(100, "盟友")]
    [InlineData(80, "盟友")]
    [InlineData(79, "友好")]
    [InlineData(50, "友好")]
    [InlineData(49, "好意的")]
    [InlineData(20, "好意的")]
    [InlineData(19, "中立")]
    [InlineData(0, "中立")]
    [InlineData(-19, "中立")]
    [InlineData(-20, "警戒")]
    [InlineData(-49, "警戒")]
    [InlineData(-50, "敵対")]
    [InlineData(-79, "敵対")]
    [InlineData(-80, "宿敵")]
    [InlineData(-100, "宿敵")]
    public void GetRelationName_BoundaryValues(int value, string expected)
    {
        Assert.Equal(expected, RelationshipSystem.GetRelationName(value));
    }

    [Theory]
    [InlineData(80, 0.2f)]
    [InlineData(100, 0.2f)]
    [InlineData(50, 0.1f)]
    [InlineData(79, 0.1f)]
    [InlineData(20, 0.05f)]
    [InlineData(49, 0.05f)]
    [InlineData(0, 0f)]
    [InlineData(19, 0f)]
    [InlineData(-50, -0.1f)]
    [InlineData(-100, -0.1f)]
    public void GetShopDiscount_BoundaryValues(int value, float expected)
    {
        Assert.Equal(expected, RelationshipSystem.GetShopDiscount(value));
    }

    [Fact]
    public void Reset_ClearsAll()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "A", "B", 50);
        sys.SetRelation(RelationshipType.Personal, "C", "D", -30);
        sys.Reset();
        Assert.Equal(0, sys.TotalRelations);
        Assert.Equal(0, sys.GetRelation(RelationshipType.Racial, "A", "B"));
    }

    [Fact]
    public void TotalRelations_CountsCorrectly()
    {
        var sys = new RelationshipSystem();
        Assert.Equal(0, sys.TotalRelations);
        sys.SetRelation(RelationshipType.Racial, "A", "B", 10);
        Assert.Equal(1, sys.TotalRelations);
        sys.SetRelation(RelationshipType.Personal, "C", "D", 20);
        Assert.Equal(2, sys.TotalRelations);
    }

    [Fact]
    public void NormalizeKey_OrderIndependent()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "Elf", "Human", 50);
        Assert.Equal(50, sys.GetRelation(RelationshipType.Racial, "Human", "Elf"));
    }

    [Fact]
    public void DifferentTypes_Independent()
    {
        var sys = new RelationshipSystem();
        sys.SetRelation(RelationshipType.Racial, "A", "B", 50);
        sys.SetRelation(RelationshipType.Personal, "A", "B", -30);
        Assert.Equal(50, sys.GetRelation(RelationshipType.Racial, "A", "B"));
        Assert.Equal(-30, sys.GetRelation(RelationshipType.Personal, "A", "B"));
    }
}

#endregion

#region 11. GameSettings Tests

public class Phase7Expansion_GameSettingsTests
{
    [Fact]
    public void CreateDefault_AllDefaultValues()
    {
        var s = GameSettings.CreateDefault();
        Assert.Equal(GameSettings.DefaultMasterVolume, s.MasterVolume);
        Assert.Equal(GameSettings.DefaultBgmVolume, s.BgmVolume);
        Assert.Equal(GameSettings.DefaultSeVolume, s.SeVolume);
        Assert.Equal(GameSettings.DefaultFontSize, s.FontSize);
        Assert.Equal(GameSettings.DefaultWindowWidth, s.WindowWidth);
        Assert.Equal(GameSettings.DefaultWindowHeight, s.WindowHeight);
    }

    [Fact]
    public void EffectiveBgmVolume_Calculation()
    {
        var s = new GameSettings { MasterVolume = 0.5f, BgmVolume = 0.6f };
        Assert.Equal(0.3f, s.EffectiveBgmVolume, 3);
    }

    [Fact]
    public void EffectiveSeVolume_Calculation()
    {
        var s = new GameSettings { MasterVolume = 0.5f, SeVolume = 0.8f };
        Assert.Equal(0.4f, s.EffectiveSeVolume, 3);
    }

    [Fact]
    public void Validate_ClampsVolumeAbove1()
    {
        var s = new GameSettings { MasterVolume = 2f, BgmVolume = 1.5f, SeVolume = 3f };
        s.Validate();
        Assert.Equal(1f, s.MasterVolume);
        Assert.Equal(1f, s.BgmVolume);
        Assert.Equal(1f, s.SeVolume);
    }

    [Fact]
    public void Validate_ClampsVolumeBelow0()
    {
        var s = new GameSettings { MasterVolume = -1f, BgmVolume = -0.5f, SeVolume = -2f };
        s.Validate();
        Assert.Equal(0f, s.MasterVolume);
        Assert.Equal(0f, s.BgmVolume);
        Assert.Equal(0f, s.SeVolume);
    }

    [Fact]
    public void Validate_ClampsFontSize()
    {
        var s = new GameSettings { FontSize = 5 };
        s.Validate();
        Assert.Equal(10, s.FontSize);
        s.FontSize = 30;
        s.Validate();
        Assert.Equal(24, s.FontSize);
    }

    [Fact]
    public void Validate_ClampsWindowWidth()
    {
        var s = new GameSettings { WindowWidth = 400 };
        s.Validate();
        Assert.Equal(800, s.WindowWidth);
        s.WindowWidth = 3000;
        s.Validate();
        Assert.Equal(1920, s.WindowWidth);
    }

    [Fact]
    public void Validate_ClampsWindowHeight()
    {
        var s = new GameSettings { WindowHeight = 300 };
        s.Validate();
        Assert.Equal(600, s.WindowHeight);
        s.WindowHeight = 2000;
        s.Validate();
        Assert.Equal(1080, s.WindowHeight);
    }

    [Fact]
    public void Clone_IndependentCopy()
    {
        var s = new GameSettings { MasterVolume = 0.5f, FontSize = 16 };
        var clone = s.Clone();
        clone.MasterVolume = 1.0f;
        clone.FontSize = 20;
        Assert.Equal(0.5f, s.MasterVolume);
        Assert.Equal(16, s.FontSize);
    }

    [Fact]
    public void Clone_CopiesAllValues()
    {
        var s = new GameSettings
        {
            MasterVolume = 0.3f, BgmVolume = 0.4f, SeVolume = 0.5f,
            FontSize = 18, WindowWidth = 1280, WindowHeight = 960
        };
        var clone = s.Clone();
        Assert.Equal(0.3f, clone.MasterVolume);
        Assert.Equal(0.4f, clone.BgmVolume);
        Assert.Equal(0.5f, clone.SeVolume);
        Assert.Equal(18, clone.FontSize);
        Assert.Equal(1280, clone.WindowWidth);
        Assert.Equal(960, clone.WindowHeight);
    }

    [Fact]
    public void DefaultConstants()
    {
        Assert.Equal(0.8f, GameSettings.DefaultMasterVolume);
        Assert.Equal(0.8f, GameSettings.DefaultBgmVolume);
        Assert.Equal(0.8f, GameSettings.DefaultSeVolume);
        Assert.Equal(14, GameSettings.DefaultFontSize);
        Assert.Equal(1024, GameSettings.DefaultWindowWidth);
        Assert.Equal(720, GameSettings.DefaultWindowHeight);
    }

    [Fact]
    public void EffectiveVolume_ZeroMaster_AllZero()
    {
        var s = new GameSettings { MasterVolume = 0f, BgmVolume = 1f, SeVolume = 1f };
        Assert.Equal(0f, s.EffectiveBgmVolume);
        Assert.Equal(0f, s.EffectiveSeVolume);
    }
}

#endregion

#region 12. AutoExploreSystem Tests

public class Phase7Expansion_AutoExploreSystemTests
{
    [Fact]
    public void CheckStopConditions_EnemyHighestPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, true, true, true, true, 0.1f, 0.1f);
        Assert.Equal(AutoExploreSystem.StopReason.EnemyDetected, reason);
    }

    [Fact]
    public void CheckStopConditions_TrapSecondPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, true, true, true, 0.1f, 0.1f);
        Assert.Equal(AutoExploreSystem.StopReason.TrapDetected, reason);
    }

    [Fact]
    public void CheckStopConditions_LowHpThirdPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.2f, 0.5f);
        Assert.Equal(AutoExploreSystem.StopReason.LowHp, reason);
    }

    [Fact]
    public void CheckStopConditions_LowHungerFourthPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.5f, 0.1f);
        Assert.Equal(AutoExploreSystem.StopReason.LowHunger, reason);
    }

    [Fact]
    public void CheckStopConditions_ItemFifthPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, true, false, false, 0.5f, 0.5f);
        Assert.Equal(AutoExploreSystem.StopReason.ItemFound, reason);
    }

    [Fact]
    public void CheckStopConditions_StairsSixthPriority()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, true, false, 0.5f, 0.5f);
        Assert.Equal(AutoExploreSystem.StopReason.StairsFound, reason);
    }

    [Fact]
    public void CheckStopConditions_FullyExploredLast()
    {
        var reason = AutoExploreSystem.CheckStopConditions(false, false, false, false, false, 0.5f, 0.5f);
        Assert.Equal(AutoExploreSystem.StopReason.FullyExplored, reason);
    }

    [Fact]
    public void CheckStopConditions_NullWhenContinuing()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.5f, 0.5f);
        Assert.Null(reason);
    }

    [Fact]
    public void CheckStopConditions_HpThreshold03()
    {
        Assert.Null(AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.3f, 0.5f));
        Assert.Equal(AutoExploreSystem.StopReason.LowHp,
            AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.29f, 0.5f));
    }

    [Fact]
    public void CheckStopConditions_HungerThreshold015()
    {
        Assert.Null(AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.5f, 0.15f));
        Assert.Equal(AutoExploreSystem.StopReason.LowHunger,
            AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.5f, 0.14f));
    }

    [Theory]
    [InlineData(AutoExploreSystem.StopReason.FullyExplored)]
    [InlineData(AutoExploreSystem.StopReason.EnemyDetected)]
    [InlineData(AutoExploreSystem.StopReason.ItemFound)]
    [InlineData(AutoExploreSystem.StopReason.StairsFound)]
    [InlineData(AutoExploreSystem.StopReason.TrapDetected)]
    [InlineData(AutoExploreSystem.StopReason.LowHp)]
    [InlineData(AutoExploreSystem.StopReason.LowHunger)]
    [InlineData(AutoExploreSystem.StopReason.UserInterrupt)]
    [InlineData(AutoExploreSystem.StopReason.NoPath)]
    public void GetStopMessage_ReturnsNonEmptyForAllReasons(AutoExploreSystem.StopReason reason)
    {
        Assert.NotEmpty(AutoExploreSystem.GetStopMessage(reason));
    }

    [Fact]
    public void CalculateExplorationPriority_Formula()
    {
        Assert.Equal(10f, AutoExploreSystem.CalculateExplorationPriority(0, 5));
        Assert.Equal(8f, AutoExploreSystem.CalculateExplorationPriority(2, 5));
        Assert.Equal(-5f, AutoExploreSystem.CalculateExplorationPriority(5, 0));
    }

    [Fact]
    public void CalculateExplorationPriority_HighUndiscovered_HighPriority()
    {
        float high = AutoExploreSystem.CalculateExplorationPriority(3, 10);
        float low = AutoExploreSystem.CalculateExplorationPriority(3, 2);
        Assert.True(high > low);
    }
}

#endregion

#region 13. DurabilitySystem Tests

public class Phase7Expansion_DurabilitySystemTests
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
    public void GetStage_RatioThresholds(int current, int max, DurabilityStage expected)
    {
        Assert.Equal(expected, DurabilitySystem.GetStage(current, max));
    }

    [Fact]
    public void GetStage_MaxZero_Perfect()
    {
        Assert.Equal(DurabilityStage.Perfect, DurabilitySystem.GetStage(0, 0));
    }

    [Theory]
    [InlineData(DurabilityStage.Perfect, 1.0f)]
    [InlineData(DurabilityStage.Worn, 0.9f)]
    [InlineData(DurabilityStage.Damaged, 0.7f)]
    [InlineData(DurabilityStage.Critical, 0.5f)]
    [InlineData(DurabilityStage.Broken, 0f)]
    public void GetPerformanceMultiplier_Values(DurabilityStage stage, float expected)
    {
        Assert.Equal(expected, DurabilitySystem.GetPerformanceMultiplier(stage));
    }

    [Fact]
    public void CalculateWeaponWear_Normal_1()
    {
        Assert.Equal(1, DurabilitySystem.CalculateWeaponWear(false));
    }

    [Fact]
    public void CalculateWeaponWear_Critical_2()
    {
        Assert.Equal(2, DurabilitySystem.CalculateWeaponWear(true));
    }

    [Theory]
    [InlineData(10, Element.None, 1)]
    [InlineData(25, Element.None, 2)]
    [InlineData(50, Element.None, 3)]
    [InlineData(10, Element.Poison, 4)]
    [InlineData(50, Element.Poison, 6)]
    public void CalculateArmorWear_DamageAndElement(int dmg, Element elem, int expected)
    {
        Assert.Equal(expected, DurabilitySystem.CalculateArmorWear(dmg, elem));
    }

    [Fact]
    public void CalculateRepairCost_FullDurability_Zero()
    {
        Assert.Equal(0, DurabilitySystem.CalculateRepairCost(100, 100, 1000));
    }

    [Fact]
    public void CalculateRepairCost_Proportional()
    {
        int cost = DurabilitySystem.CalculateRepairCost(50, 100, 1000);
        Assert.Equal(250, cost);
    }

    [Fact]
    public void CalculateRepairCost_MaxZero_Zero()
    {
        Assert.Equal(0, DurabilitySystem.CalculateRepairCost(0, 0, 1000));
    }

    [Theory]
    [InlineData(1, 15)]
    [InlineData(2, 30)]
    [InlineData(3, 50)]
    [InlineData(0, 10)]
    public void CalculateKitRepairAmount_Quality(int quality, int expected)
    {
        Assert.Equal(expected, DurabilitySystem.CalculateKitRepairAmount(quality));
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void CanSelfRepair_SmithingThreshold(int level, bool expected)
    {
        Assert.Equal(expected, DurabilitySystem.CanSelfRepair(level));
    }

    [Fact]
    public void CalculateSelfRepairAmount_BelowThreshold_Zero()
    {
        Assert.Equal(0, DurabilitySystem.CalculateSelfRepairAmount(4));
    }

    [Fact]
    public void CalculateSelfRepairAmount_AtThreshold_10()
    {
        Assert.Equal(10, DurabilitySystem.CalculateSelfRepairAmount(5));
    }

    [Fact]
    public void CalculateSelfRepairAmount_HighLevel_CappedAt40()
    {
        Assert.Equal(40, DurabilitySystem.CalculateSelfRepairAmount(100));
    }

    [Fact]
    public void CalculateSelfRepairAmount_Progressive()
    {
        Assert.Equal(10, DurabilitySystem.CalculateSelfRepairAmount(5));
        Assert.Equal(13, DurabilitySystem.CalculateSelfRepairAmount(6));
        Assert.Equal(16, DurabilitySystem.CalculateSelfRepairAmount(7));
    }
}

#endregion

#region 14. GridInventorySystem Tests

public class Phase7Expansion_GridInventorySystemTests
{
    [Fact]
    public void Constructor_DefaultSize()
    {
        var sys = new GridInventorySystem();
        Assert.Equal(10, sys.Width);
        Assert.Equal(6, sys.Height);
    }

    [Fact]
    public void Constructor_CustomSize()
    {
        var sys = new GridInventorySystem(5, 3);
        Assert.Equal(5, sys.Width);
        Assert.Equal(3, sys.Height);
    }

    [Theory]
    [InlineData(GridItemSize.Size1x1, 1, 1)]
    [InlineData(GridItemSize.Size1x2, 1, 2)]
    [InlineData(GridItemSize.Size2x1, 2, 1)]
    [InlineData(GridItemSize.Size2x2, 2, 2)]
    [InlineData(GridItemSize.Size2x3, 2, 3)]
    public void GetDimensions_AllSizes(GridItemSize size, int w, int h)
    {
        var (rw, rh) = GridInventorySystem.GetDimensions(size);
        Assert.Equal(w, rw);
        Assert.Equal(h, rh);
    }

    [Fact]
    public void CanPlace_EmptyGrid_True()
    {
        var sys = new GridInventorySystem();
        Assert.True(sys.CanPlace(GridItemSize.Size1x1, 0, 0));
    }

    [Fact]
    public void CanPlace_OutOfBounds_False()
    {
        var sys = new GridInventorySystem(5, 5);
        Assert.False(sys.CanPlace(GridItemSize.Size2x2, 4, 4));
    }

    [Fact]
    public void CanPlace_NegativeCoords_False()
    {
        var sys = new GridInventorySystem();
        Assert.False(sys.CanPlace(GridItemSize.Size1x1, -1, 0));
    }

    [Fact]
    public void PlaceItem_Success()
    {
        var sys = new GridInventorySystem();
        Assert.True(sys.PlaceItem("item1", "Sword", GridItemSize.Size1x2, 0, 0));
        Assert.Single(sys.Items);
    }

    [Fact]
    public void PlaceItem_Overlap_Fails()
    {
        var sys = new GridInventorySystem();
        sys.PlaceItem("item1", "Sword", GridItemSize.Size2x2, 0, 0);
        Assert.False(sys.PlaceItem("item2", "Shield", GridItemSize.Size2x2, 1, 1));
    }

    [Fact]
    public void PlaceItem_Adjacent_Succeeds()
    {
        var sys = new GridInventorySystem();
        sys.PlaceItem("item1", "Sword", GridItemSize.Size2x2, 0, 0);
        Assert.True(sys.PlaceItem("item2", "Shield", GridItemSize.Size2x2, 2, 0));
    }

    [Fact]
    public void RemoveItem_Existing_True()
    {
        var sys = new GridInventorySystem();
        sys.PlaceItem("item1", "Sword", GridItemSize.Size1x1, 0, 0);
        Assert.True(sys.RemoveItem("item1"));
        Assert.Empty(sys.Items);
    }

    [Fact]
    public void RemoveItem_NonExisting_False()
    {
        var sys = new GridInventorySystem();
        Assert.False(sys.RemoveItem("nonexistent"));
    }

    [Fact]
    public void GetFreeSpaceRatio_EmptyGrid_1()
    {
        var sys = new GridInventorySystem(10, 6);
        Assert.Equal(1.0f, sys.GetFreeSpaceRatio());
    }

    [Fact]
    public void GetFreeSpaceRatio_AfterPlacement()
    {
        var sys = new GridInventorySystem(10, 10); // 100 cells
        sys.PlaceItem("item1", "Big", GridItemSize.Size2x2, 0, 0); // 4 cells
        Assert.Equal(0.96f, sys.GetFreeSpaceRatio(), 2);
    }

    [Fact]
    public void Reset_ClearsAllItems()
    {
        var sys = new GridInventorySystem();
        sys.PlaceItem("a", "A", GridItemSize.Size1x1, 0, 0);
        sys.PlaceItem("b", "B", GridItemSize.Size1x1, 1, 0);
        sys.Reset();
        Assert.Empty(sys.Items);
        Assert.Equal(1.0f, sys.GetFreeSpaceRatio());
    }

    [Fact]
    public void PlaceItem_AfterRemove_Succeeds()
    {
        var sys = new GridInventorySystem(2, 2);
        sys.PlaceItem("item1", "A", GridItemSize.Size2x2, 0, 0);
        sys.RemoveItem("item1");
        Assert.True(sys.PlaceItem("item2", "B", GridItemSize.Size2x2, 0, 0));
    }

    [Fact]
    public void MultipleItems_CorrectCount()
    {
        var sys = new GridInventorySystem();
        sys.PlaceItem("a", "A", GridItemSize.Size1x1, 0, 0);
        sys.PlaceItem("b", "B", GridItemSize.Size1x1, 1, 0);
        sys.PlaceItem("c", "C", GridItemSize.Size1x1, 2, 0);
        Assert.Equal(3, sys.Items.Count);
    }
}

#endregion

#region 15. InscriptionSystem Tests

public class Phase7Expansion_InscriptionSystemTests
{
    [Fact]
    public void Register_AddsInscription()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Lore, "???", "Ancient text", 5);
        Assert.Single(sys.Inscriptions);
    }

    [Fact]
    public void TryDecode_NotFound_Fails()
    {
        var sys = new InscriptionSystem();
        var result = sys.TryDecode("nonexistent", 10);
        Assert.False(result.Success);
    }

    [Fact]
    public void TryDecode_SufficientLevel_Success()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Lore, "???", "Secret lore", 5, "exp_bonus");
        var result = sys.TryDecode("ins1", 5);
        Assert.True(result.Success);
        Assert.Contains("Secret lore", result.Message);
        Assert.Equal("exp_bonus", result.RewardInfo);
        Assert.Equal(100, result.PartialProgress);
    }

    [Fact]
    public void TryDecode_InsufficientLevel_PartialProgress()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Warning, "???", "Danger ahead", 10);
        var result = sys.TryDecode("ins1", 5);
        Assert.False(result.Success);
        Assert.Equal(50, result.PartialProgress);
    }

    [Fact]
    public void TryDecode_AlreadyDecoded_ReturnsDecoded()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Hint, "???", "Use fire", 3, "hint");
        sys.TryDecode("ins1", 3);
        var result = sys.TryDecode("ins1", 1);
        Assert.True(result.Success);
        Assert.Equal("hint", result.RewardInfo);
    }

    [Fact]
    public void DecodedCount_TracksCorrectly()
    {
        var sys = new InscriptionSystem();
        sys.Register("a", InscriptionType.Lore, "?", "A", 1);
        sys.Register("b", InscriptionType.Lore, "?", "B", 1);
        Assert.Equal(0, sys.DecodedCount);
        sys.TryDecode("a", 1);
        Assert.Equal(1, sys.DecodedCount);
        sys.TryDecode("b", 1);
        Assert.Equal(2, sys.DecodedCount);
    }

    [Theory]
    [InlineData(InscriptionType.Lore, "伝承の碑文")]
    [InlineData(InscriptionType.Warning, "警告の碑文")]
    [InlineData(InscriptionType.Hint, "手がかりの碑文")]
    [InlineData(InscriptionType.Recipe, "秘伝の碑文")]
    [InlineData(InscriptionType.Spell, "呪文の碑文")]
    [InlineData(InscriptionType.Map, "地図の壁画")]
    public void GetTypeName_AllTypes(InscriptionType type, string expected)
    {
        Assert.Equal(expected, InscriptionSystem.GetTypeName(type));
    }

    [Theory]
    [InlineData(InscriptionType.Lore, "世界観の知識を得た")]
    [InlineData(InscriptionType.Warning, "危険な情報を得た")]
    [InlineData(InscriptionType.Recipe, "新しいレシピを習得した")]
    [InlineData(InscriptionType.Spell, "新しい呪文を習得した")]
    [InlineData(InscriptionType.Map, "隠された部屋の位置を把握した")]
    public void GetRewardDescription_AllTypes(InscriptionType type, string expected)
    {
        Assert.Equal(expected, InscriptionSystem.GetRewardDescription(type));
    }

    [Fact]
    public void Reset_MarksAllUndecoded()
    {
        var sys = new InscriptionSystem();
        sys.Register("a", InscriptionType.Lore, "?", "A", 1);
        sys.Register("b", InscriptionType.Hint, "?", "B", 1);
        sys.TryDecode("a", 1);
        sys.TryDecode("b", 1);
        Assert.Equal(2, sys.DecodedCount);
        sys.Reset();
        Assert.Equal(0, sys.DecodedCount);
        Assert.Equal(2, sys.Inscriptions.Count); // Registrations preserved
    }

    [Fact]
    public void GetByType_FiltersCorrectly()
    {
        var sys = new InscriptionSystem();
        sys.Register("a", InscriptionType.Lore, "?", "A", 1);
        sys.Register("b", InscriptionType.Warning, "?", "B", 1);
        sys.Register("c", InscriptionType.Lore, "?", "C", 1);
        Assert.Equal(2, sys.GetByType(InscriptionType.Lore).Count);
        Assert.Single(sys.GetByType(InscriptionType.Warning));
    }

    [Fact]
    public void TryDecode_PartialProgress_CalculatesCorrectly()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Lore, "???", "Hello World", 10);
        var result = sys.TryDecode("ins1", 3);
        Assert.Equal(30, result.PartialProgress);
    }

    [Fact]
    public void TryDecode_ZeroLevel_MinimalProgress()
    {
        var sys = new InscriptionSystem();
        sys.Register("ins1", InscriptionType.Lore, "???", "Hello", 10);
        var result = sys.TryDecode("ins1", 0);
        Assert.Equal(0, result.PartialProgress);
    }
}

#endregion

#region 16. DungeonEcosystemSystem Tests

public class Phase7Expansion_DungeonEcosystemSystemTests
{
    [Fact]
    public void RegisterRelation_AddsToList()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid, 70);
        Assert.Single(sys.Relations);
    }

    [Fact]
    public void RegisterRelation_ClampsChance()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid, 150);
        Assert.Equal(100, sys.Relations[0].PredationChance);
    }

    [Fact]
    public void HasPredatorRelation_Exists_ReturnsTrue()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid);
        Assert.True(sys.HasPredatorRelation(MonsterRace.Beast, MonsterRace.Humanoid));
    }

    [Fact]
    public void HasPredatorRelation_NotExists_ReturnsFalse()
    {
        var sys = new DungeonEcosystemSystem();
        Assert.False(sys.HasPredatorRelation(MonsterRace.Beast, MonsterRace.Humanoid));
    }

    [Fact]
    public void ProcessInteraction_WithRelation_ReturnsPredation()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid);
        var evt = sys.ProcessInteraction("wolf1", MonsterRace.Beast, "goblin1", MonsterRace.Humanoid, 1, 10);
        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.Predation, evt!.Type);
    }

    [Fact]
    public void ProcessInteraction_SameRace_ReturnsTerritoryFight()
    {
        var sys = new DungeonEcosystemSystem();
        var evt = sys.ProcessInteraction("wolf1", MonsterRace.Beast, "wolf2", MonsterRace.Beast, 1, 10);
        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.TerritoryFight, evt!.Type);
    }

    [Fact]
    public void ProcessInteraction_NoRelationDifferentRace_ReturnsNull()
    {
        var sys = new DungeonEcosystemSystem();
        var evt = sys.ProcessInteraction("a", MonsterRace.Beast, "b", MonsterRace.Undead, 1, 10);
        Assert.Null(evt);
    }

    [Fact]
    public void ProcessInteraction_AddsToEvents()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid);
        sys.ProcessInteraction("a", MonsterRace.Beast, "b", MonsterRace.Humanoid, 1, 10);
        Assert.Single(sys.Events);
    }

    [Fact]
    public void AddBattleTrace_AddsToList()
    {
        var sys = new DungeonEcosystemSystem();
        sys.AddBattleTrace(5, 5, 1, 3, "Blood stain", 10);
        Assert.Single(sys.Traces);
    }

    [Fact]
    public void GetTracesOnFloor_FiltersCorrectly()
    {
        var sys = new DungeonEcosystemSystem();
        sys.AddBattleTrace(5, 5, 1, 3, "A", 10);
        sys.AddBattleTrace(10, 10, 2, 5, "B", 20);
        sys.AddBattleTrace(15, 15, 1, 4, "C", 30);
        Assert.Equal(2, sys.GetTracesOnFloor(1).Count);
        Assert.Single(sys.GetTracesOnFloor(2));
    }

    [Fact]
    public void EstimateDangerLevel_Average()
    {
        var sys = new DungeonEcosystemSystem();
        sys.AddBattleTrace(0, 0, 1, 2, "A", 1);
        sys.AddBattleTrace(0, 0, 1, 4, "B", 2);
        sys.AddBattleTrace(0, 0, 1, 6, "C", 3);
        Assert.Equal(4, sys.EstimateDangerLevel(1));
    }

    [Fact]
    public void EstimateDangerLevel_Empty_Zero()
    {
        var sys = new DungeonEcosystemSystem();
        Assert.Equal(0, sys.EstimateDangerLevel(1));
    }

    [Fact]
    public void Reset_ClearsEventsAndTraces_PreservesRelations()
    {
        var sys = new DungeonEcosystemSystem();
        sys.RegisterRelation(MonsterRace.Beast, MonsterRace.Humanoid);
        sys.ProcessInteraction("a", MonsterRace.Beast, "b", MonsterRace.Humanoid, 1, 1);
        sys.AddBattleTrace(0, 0, 1, 1, "X", 1);
        sys.Reset();
        Assert.Empty(sys.Events);
        Assert.Empty(sys.Traces);
        Assert.Single(sys.Relations);
    }

    [Theory]
    [InlineData(EcosystemEventType.Predation, "捕食")]
    [InlineData(EcosystemEventType.TerritoryFight, "縄張り争い")]
    [InlineData(EcosystemEventType.Symbiosis, "共生")]
    [InlineData(EcosystemEventType.Scavenging, "漁り")]
    public void GetEventTypeName_AllTypes(EcosystemEventType type, string expected)
    {
        Assert.Equal(expected, DungeonEcosystemSystem.GetEventTypeName(type));
    }
}

#endregion

#region 17. CraftingSystem Tests

public class Phase7Expansion_CraftingSystemTests
{
    [Fact]
    public void DefaultRecipes_RegisteredOnConstruction()
    {
        var sys = new CraftingSystem();
        Assert.True(sys.GetAllRecipes().Count >= 10);
    }

    [Fact]
    public void GetRecipe_ExistingRecipe()
    {
        var sys = new CraftingSystem();
        var recipe = sys.GetRecipe("recipe_iron_sword");
        Assert.NotNull(recipe);
        Assert.Equal("鉄の剣の鍛造", recipe!.Name);
    }

    [Fact]
    public void GetRecipe_NonExisting_Null()
    {
        var sys = new CraftingSystem();
        Assert.Null(sys.GetRecipe("nonexistent"));
    }

    [Fact]
    public void RegisterRecipe_CustomRecipe()
    {
        var sys = new CraftingSystem();
        int before = sys.GetAllRecipes().Count;
        sys.RegisterRecipe(new CraftingRecipe("custom_recipe", "Custom", "Test",
            new List<CraftingMaterial> { new("item_a", 1) }, "result_a"));
        Assert.Equal(before + 1, sys.GetAllRecipes().Count);
    }

    [Fact]
    public void GetAvailableRecipes_LevelFiltering()
    {
        var sys = new CraftingSystem();
        var level1 = sys.GetAvailableRecipes(1);
        var level10 = sys.GetAvailableRecipes(10);
        Assert.True(level10.Count >= level1.Count);
    }

    [Fact]
    public void GetAvailableRecipes_Level0_OnlyLevel0Recipes()
    {
        var sys = new CraftingSystem();
        var recipes = sys.GetAvailableRecipes(0);
        Assert.All(recipes, r => Assert.True(r.RequiredLevel <= 0));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 95)]
    [InlineData(5, 55)]
    [InlineData(9, 10)]
    [InlineData(10, 5)]
    public void CalculateEnhanceSuccessRate_Values(int level, int expected)
    {
        Assert.Equal(expected, CraftingSystem.CalculateEnhanceSuccessRate(level));
    }

    [Fact]
    public void CalculateEnhanceSuccessRate_DecreasesWithLevel()
    {
        for (int i = 0; i < 10; i++)
        {
            Assert.True(CraftingSystem.CalculateEnhanceSuccessRate(i) >= CraftingSystem.CalculateEnhanceSuccessRate(i + 1));
        }
    }

    [Fact]
    public void DefaultRecipes_IncludeIronSword()
    {
        var sys = new CraftingSystem();
        var recipe = sys.GetRecipe("recipe_iron_sword");
        Assert.NotNull(recipe);
        Assert.Equal(2, recipe!.Materials.Count);
    }

    [Fact]
    public void DefaultRecipes_IncludeHealingPotion()
    {
        var sys = new CraftingSystem();
        var recipe = sys.GetRecipe("recipe_healing_potion");
        Assert.NotNull(recipe);
        Assert.Equal("回復薬の調合", recipe!.Name);
    }

    [Fact]
    public void CraftingInventory_AddAndCount()
    {
        var inv = new CraftingInventory();
        inv.AddItem("iron_ore", 5);
        Assert.Equal(5, inv.CountItem("iron_ore"));
    }

    [Fact]
    public void CraftingInventory_RemoveItem()
    {
        var inv = new CraftingInventory();
        inv.AddItem("iron_ore", 5);
        Assert.True(inv.RemoveItem("iron_ore", 3));
        Assert.Equal(2, inv.CountItem("iron_ore"));
    }

    [Fact]
    public void CraftingInventory_RemoveInsufficient_False()
    {
        var inv = new CraftingInventory();
        inv.AddItem("iron_ore", 2);
        Assert.False(inv.RemoveItem("iron_ore", 5));
    }

    [Fact]
    public void CraftingInventory_HasItem()
    {
        var inv = new CraftingInventory();
        inv.AddItem("herb", 3);
        Assert.True(inv.HasItem("herb", 2));
        Assert.False(inv.HasItem("herb", 5));
    }

    [Fact]
    public void CraftingInventory_GetAllItems()
    {
        var inv = new CraftingInventory();
        inv.AddItem("a", 1);
        inv.AddItem("b", 2);
        Assert.Equal(2, inv.GetAllItems().Count);
    }

    [Fact]
    public void CraftingInventory_CountNonExistent_Zero()
    {
        var inv = new CraftingInventory();
        Assert.Equal(0, inv.CountItem("nonexistent"));
    }

    [Fact]
    public void CraftingInventory_RemoveAll_RemovesKey()
    {
        var inv = new CraftingInventory();
        inv.AddItem("a", 3);
        inv.RemoveItem("a", 3);
        Assert.Equal(0, inv.CountItem("a"));
    }

    [Fact]
    public void DefaultRecipes_AllHaveResultItemId()
    {
        var sys = new CraftingSystem();
        Assert.All(sys.GetAllRecipes(), r => Assert.NotEmpty(r.ResultItemId));
    }

    [Fact]
    public void DefaultRecipes_AllHaveDescription()
    {
        var sys = new CraftingSystem();
        Assert.All(sys.GetAllRecipes(), r => Assert.NotEmpty(r.Description));
    }
}

#endregion

#region 18. SaveData Tests

public class Phase7Expansion_SaveDataTests
{
    [Fact]
    public void SaveData_DefaultValues()
    {
        var sd = new SaveData();
        Assert.Equal(1, sd.Version);
        Assert.Equal(0, sd.CurrentFloor);
        Assert.Equal(0, sd.TurnCount);
        Assert.False(sd.TurnLimitExtended);
        Assert.False(sd.TurnLimitRemoved);
        Assert.Equal(DifficultyLevel.Normal, sd.Difficulty);
        Assert.Equal("capital_guild", sd.CurrentMapName);
    }

    [Fact]
    public void PlayerSaveData_DefaultValues()
    {
        var pd = new PlayerSaveData();
        Assert.Equal(string.Empty, pd.Name);
        Assert.Equal(0, pd.Level);
        Assert.Equal(Race.Human, pd.Race);
        Assert.Equal(CharacterClass.Fighter, pd.CharacterClass);
        Assert.Equal(Background.Adventurer, pd.Background);
        Assert.Equal(0, pd.TotalDeaths);
    }

    [Fact]
    public void StatsSaveData_FromStats()
    {
        var stats = new Stats(10, 20, 30, 40, 50, 60, 70, 80, 90);
        var saved = StatsSaveData.FromStats(stats);
        Assert.Equal(10, saved.Strength);
        Assert.Equal(20, saved.Vitality);
        Assert.Equal(90, saved.Luck);
    }

    [Fact]
    public void StatsSaveData_ToStats()
    {
        var saved = new StatsSaveData { Strength = 15, Vitality = 25, Agility = 35, Dexterity = 45, Intelligence = 55, Mind = 65, Perception = 75, Charisma = 85, Luck = 95 };
        var stats = saved.ToStats();
        Assert.Equal(15, stats.Strength);
        Assert.Equal(95, stats.Luck);
    }

    [Fact]
    public void StatsSaveData_RoundTrip()
    {
        var original = new Stats(1, 2, 3, 4, 5, 6, 7, 8, 9);
        var restored = StatsSaveData.FromStats(original).ToStats();
        Assert.Equal(original.Strength, restored.Strength);
        Assert.Equal(original.Vitality, restored.Vitality);
        Assert.Equal(original.Agility, restored.Agility);
        Assert.Equal(original.Dexterity, restored.Dexterity);
        Assert.Equal(original.Intelligence, restored.Intelligence);
        Assert.Equal(original.Mind, restored.Mind);
        Assert.Equal(original.Perception, restored.Perception);
        Assert.Equal(original.Charisma, restored.Charisma);
        Assert.Equal(original.Luck, restored.Luck);
    }

    [Fact]
    public void PositionSaveData_FromPosition()
    {
        var pos = new Position(10, 20);
        var saved = PositionSaveData.FromPosition(pos);
        Assert.Equal(10, saved.X);
        Assert.Equal(20, saved.Y);
    }

    [Fact]
    public void PositionSaveData_ToPosition()
    {
        var saved = new PositionSaveData { X = 5, Y = 15 };
        var pos = saved.ToPosition();
        Assert.Equal(5, pos.X);
        Assert.Equal(15, pos.Y);
    }

    [Fact]
    public void PositionSaveData_RoundTrip()
    {
        var original = new Position(42, 99);
        var restored = PositionSaveData.FromPosition(original).ToPosition();
        Assert.Equal(original.X, restored.X);
        Assert.Equal(original.Y, restored.Y);
    }

    [Fact]
    public void ItemSaveData_Defaults()
    {
        var item = new ItemSaveData();
        Assert.Equal(string.Empty, item.ItemId);
        Assert.Equal(0, item.EnhancementLevel);
        Assert.True(item.IsIdentified);
        Assert.False(item.IsCursed);
        Assert.False(item.IsBlessed);
        Assert.Equal(-1, item.Durability);
        Assert.Equal(1, item.StackCount);
    }

    [Fact]
    public void SaveData_Collections_InitializedEmpty()
    {
        var sd = new SaveData();
        Assert.NotNull(sd.MessageHistory);
        Assert.Empty(sd.MessageHistory);
        Assert.NotNull(sd.SkillCooldowns);
        Assert.Empty(sd.SkillCooldowns);
        Assert.NotNull(sd.NpcStates);
        Assert.Empty(sd.NpcStates);
        Assert.NotNull(sd.ActiveQuests);
        Assert.Empty(sd.ActiveQuests);
        Assert.NotNull(sd.CompletedQuests);
        Assert.Empty(sd.CompletedQuests);
        Assert.NotNull(sd.DialogueFlags);
        Assert.Empty(sd.DialogueFlags);
    }

    [Fact]
    public void SaveData_VisitedTerritories_DefaultContainsCapital()
    {
        var sd = new SaveData();
        Assert.Contains(nameof(TerritoryId.Capital), sd.VisitedTerritories);
    }

    [Fact]
    public void SaveData_IsOnSurface_DefaultTrue()
    {
        var sd = new SaveData();
        Assert.True(sd.IsOnSurface);
    }

    [Fact]
    public void PlayerSaveData_Collections_InitializedEmpty()
    {
        var pd = new PlayerSaveData();
        Assert.NotNull(pd.InventoryItems);
        Assert.Empty(pd.InventoryItems);
        Assert.NotNull(pd.EquippedItems);
        Assert.Empty(pd.EquippedItems);
        Assert.NotNull(pd.LearnedWords);
        Assert.Empty(pd.LearnedWords);
        Assert.NotNull(pd.LearnedSkills);
        Assert.Empty(pd.LearnedSkills);
        Assert.NotNull(pd.PreviousReligions);
        Assert.Empty(pd.PreviousReligions);
    }

    [Fact]
    public void TransferDataSaveData_Defaults()
    {
        var td = new TransferDataSaveData();
        Assert.NotNull(td.LearnedWords);
        Assert.NotNull(td.LearnedSkills);
        Assert.Equal(GameConstants.MaxRescueCount, td.RescueCountRemaining);
        Assert.Equal(GameConstants.InitialSanity, td.Sanity);
    }

    [Fact]
    public void GameTimeSaveData_Default()
    {
        var gt = new GameTimeSaveData();
        Assert.Equal(0, gt.TotalTurns);
    }
}

#endregion
