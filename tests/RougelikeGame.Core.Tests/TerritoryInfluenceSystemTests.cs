using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class TerritoryInfluenceSystemTests
{
    [Fact]
    public void Initialize_SetsFactions()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 0.7f,
            ["Rebels"] = 0.3f
        });
        Assert.NotNull(system.GetInfluenceMap(TerritoryId.Capital));
    }

    [Fact]
    public void GetDominantFaction_ReturnsHighest()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 0.7f,
            ["Rebels"] = 0.3f
        });
        Assert.Equal("Kingdom", system.GetDominantFaction(TerritoryId.Capital));
    }

    [Fact]
    public void ModifyInfluence_ChangesBalance()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 0.5f,
            ["Rebels"] = 0.5f
        });
        system.ModifyInfluence(TerritoryId.Capital, "Rebels", 0.5f);
        Assert.Equal("Rebels", system.GetDominantFaction(TerritoryId.Capital));
    }

    [Fact]
    public void GetDominantFaction_UnknownTerritory_ReturnsNull()
    {
        var system = new TerritoryInfluenceSystem();
        Assert.Null(system.GetDominantFaction(TerritoryId.Forest));
    }

    [Fact]
    public void GetInfluence_ReturnsValue()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 1.0f
        });
        Assert.True(system.GetInfluence(TerritoryId.Capital, "Kingdom") > 0);
    }
}
