using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class SymbolMapEventSystemTests
{
    [Fact]
    public void GetAllEvents_ReturnsNonEmpty()
    {
        var events = SymbolMapEventSystem.GetAllEvents();
        Assert.True(events.Count > 0);
    }

    [Fact]
    public void GetAvailableEvents_SpringForest_ReturnsFiltered()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Spring, TerritoryId.Forest);
        Assert.True(events.Count > 0);
    }

    [Fact]
    public void RollEvent_LowRandom_ReturnsEvent()
    {
        var result = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Capital, 0.01);
        Assert.NotNull(result);
    }

    [Fact]
    public void RollEvent_HighRandom_ReturnsNull()
    {
        var result = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Capital, 0.99);
        Assert.Null(result);
    }

    [Fact]
    public void GetAvailableEvents_ContainsUniversalEvents()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Winter, TerritoryId.Capital);
        Assert.True(events.Any(e => e.Id == "event_wandering_healer"));
    }
}
