using Xunit;
using RougelikeGame.Core;

namespace RougelikeGame.Core.Tests;

public class StartingMapResolverTests
{
    [Theory]
    [InlineData(Background.Soldier, "capital_barracks")]
    [InlineData(Background.Scholar, "capital_academy")]
    [InlineData(Background.Merchant, "capital_market")]
    [InlineData(Background.Noble, "capital_manor")]
    [InlineData(Background.Criminal, "capital_prison")]
    public void Resolve_SpecificBackground_ReturnsExpectedMap(Background bg, string expected)
    {
        Assert.Equal(expected, StartingMapResolver.Resolve(Race.Human, bg));
    }

    [Theory]
    [InlineData(Race.Elf, "forest_village")]
    [InlineData(Race.Dwarf, "mountain_hold")]
    [InlineData(Race.Halfling, "coast_port")]
    [InlineData(Race.Human, "capital_guild")]
    public void Resolve_Adventurer_UsesRaceMap(Race race, string expected)
    {
        Assert.Equal(expected, StartingMapResolver.Resolve(race, Background.Adventurer));
    }

    [Fact]
    public void GetDisplayName_ValidMap_ReturnsJapaneseName()
    {
        Assert.Equal("王都・冒険者ギルド", StartingMapResolver.GetDisplayName("capital_guild"));
    }

    [Fact]
    public void GetStartingTerritory_ForestVillage_ReturnsForest()
    {
        Assert.Equal(TerritoryId.Forest, StartingMapResolver.GetStartingTerritory("forest_village"));
    }

    [Fact]
    public void GetStartingTerritory_CapitalMap_ReturnsCapital()
    {
        Assert.Equal(TerritoryId.Capital, StartingMapResolver.GetStartingTerritory("capital_guild"));
    }
}
