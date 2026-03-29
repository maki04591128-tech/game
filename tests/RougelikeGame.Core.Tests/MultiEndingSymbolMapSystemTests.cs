using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// マルチエンディングシステム＋シンボルマップシステムテスト
/// テスト数: 18件
/// </summary>
public class MultiEndingSymbolMapSystemTests
{
    // ============================================================
    // MultiEndingSystem テスト
    // ============================================================

    [Fact]
    public void MultiEnding_DarkEnding_LowKarma()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 5, karmaValue: -60,
            allTerritoriesVisited: false, clearRank: "C");
        Assert.Equal(EndingType.Dark, result.Type);
        Assert.Equal("闇の支配者", result.Title);
    }

    [Fact]
    public void MultiEnding_SalvationEnding_NoDeathHighKarma()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: 60,
            allTerritoriesVisited: true, clearRank: "B");
        Assert.Equal(EndingType.Salvation, result.Type);
        Assert.Equal("聖者の凱旋", result.Title);
    }

    [Fact]
    public void MultiEnding_TrueEnding_HighRank()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 3, karmaValue: 30,
            allTerritoriesVisited: true, clearRank: "S");
        Assert.Equal(EndingType.True, result.Type);
    }

    [Fact]
    public void MultiEnding_WandererEnding_NoClearAllVisited()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: false, totalDeaths: 10, karmaValue: 0,
            allTerritoriesVisited: true, clearRank: "");
        Assert.Equal(EndingType.Wanderer, result.Type);
    }

    [Fact]
    public void MultiEnding_NormalEnding_ClearedWithLowRank()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 5, karmaValue: 10,
            allTerritoriesVisited: false, clearRank: "C");
        Assert.Equal(EndingType.Normal, result.Type);
    }

    [Fact]
    public void MultiEnding_NoClear_NoVisit_ReturnsNormal()
    {
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: false, totalDeaths: 0, karmaValue: 0,
            allTerritoriesVisited: false, clearRank: "");
        Assert.Equal(EndingType.Normal, result.Type);
        Assert.Equal("未完の旅", result.Title);
    }

    [Theory]
    [InlineData(EndingType.Normal, "正規エンディング")]
    [InlineData(EndingType.True, "真エンディング")]
    [InlineData(EndingType.Dark, "闇エンディング")]
    [InlineData(EndingType.Salvation, "救済エンディング")]
    [InlineData(EndingType.Wanderer, "放浪エンディング")]
    public void MultiEnding_GetEndingTypeName_ReturnsCorrectName(EndingType type, string expected)
    {
        Assert.Equal(expected, MultiEndingSystem.GetEndingTypeName(type));
    }

    [Fact]
    public void MultiEnding_GetEndingConditions_Returns5Entries()
    {
        var conditions = MultiEndingSystem.GetEndingConditions();
        Assert.Equal(5, conditions.Count);
    }

    // ============================================================
    // SymbolMapSystem テスト
    // ============================================================

    [Fact]
    public void SymbolMap_InitialState_IsEmpty()
    {
        var system = new SymbolMapSystem();
        Assert.Null(system.CurrentMap);
        Assert.Null(system.CurrentTerritory);
        Assert.Equal(0, system.LocationCount);
    }

    [Fact]
    public void SymbolMap_GenerateForTerritory_CreatesMap()
    {
        var system = new SymbolMapSystem();
        var map = system.GenerateForTerritory(TerritoryId.Capital);
        Assert.NotNull(map);
        Assert.NotNull(system.CurrentMap);
        Assert.Equal(TerritoryId.Capital, system.CurrentTerritory);
        Assert.True(system.LocationCount > 0);
    }

    [Fact]
    public void SymbolMap_GetAllLocationPositions_ReturnsPositions()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Forest);
        var positions = system.GetAllLocationPositions();
        Assert.NotEmpty(positions);
    }

    [Fact]
    public void SymbolMap_IsLocationSymbol_ReturnsTrueForLocations()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var positions = system.GetAllLocationPositions();
        foreach (var pos in positions.Keys.Take(1))
        {
            Assert.True(system.IsLocationSymbol(pos));
        }
    }

    [Fact]
    public void SymbolMap_GetLocationAt_InvalidPosition_ReturnsNull()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var result = system.GetLocationAt(new Position(-999, -999));
        Assert.Null(result);
    }

    [Fact]
    public void SymbolMap_GetLocationArrivalMessage_EmptyForInvalidPosition()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var msg = system.GetLocationArrivalMessage(new Position(-999, -999));
        Assert.Equal(string.Empty, msg);
    }

    [Fact]
    public void SymbolMap_GetLocationArrivalMessage_HasContent()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var positions = system.GetAllLocationPositions();
        var firstPos = positions.Keys.First();
        var msg = system.GetLocationArrivalMessage(firstPos);
        Assert.NotEmpty(msg);
        Assert.Contains("【", msg);
    }

    [Fact]
    public void SymbolMap_Clear_ResetsAll()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        system.Clear();
        Assert.Null(system.CurrentMap);
        Assert.Null(system.CurrentTerritory);
        Assert.Equal(0, system.LocationCount);
    }

    [Fact]
    public void SymbolMap_FindLocationPosition_InvalidId_ReturnsNull()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.Null(system.FindLocationPosition("nonexistent_location"));
    }
}
