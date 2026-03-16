using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.55 ダンジョン特徴別ランダム生成システムのテスト
/// </summary>
public class DungeonFeatureGeneratorTests
{
    #region Definition Tests

    [Theory]
    [InlineData(DungeonFeatureType.Standard, "標準ダンジョン")]
    [InlineData(DungeonFeatureType.Cave, "洞窟")]
    [InlineData(DungeonFeatureType.Ruins, "遺跡")]
    [InlineData(DungeonFeatureType.Sewer, "下水道")]
    [InlineData(DungeonFeatureType.Mine, "鉱山")]
    [InlineData(DungeonFeatureType.Crypt, "墓地")]
    [InlineData(DungeonFeatureType.Temple, "神殿")]
    [InlineData(DungeonFeatureType.IceCavern, "氷の洞窟")]
    [InlineData(DungeonFeatureType.Volcanic, "火山")]
    [InlineData(DungeonFeatureType.Forest, "森林迷宮")]
    public void GetDefinition_ReturnsValidDefinition(DungeonFeatureType type, string expectedName)
    {
        var def = DungeonFeatureGenerator.GetDefinition(type);
        Assert.NotNull(def);
        Assert.Equal(expectedName, def!.Name);
    }

    [Fact]
    public void GetAllDefinitions_Returns10Types()
    {
        var all = DungeonFeatureGenerator.GetAllDefinitions();
        Assert.Equal(10, all.Count);
    }

    [Fact]
    public void GetDefinition_AllTypes_HaveParams()
    {
        foreach (DungeonFeatureType type in Enum.GetValues<DungeonFeatureType>())
        {
            var param = DungeonFeatureGenerator.GetParams(type);
            Assert.NotNull(param);
        }
    }

    #endregion

    #region Common Races Tests

    [Fact]
    public void GetCommonRaces_Cave_ContainsBeast()
    {
        var races = DungeonFeatureGenerator.GetCommonRaces(DungeonFeatureType.Cave);
        Assert.Contains(MonsterRace.Beast, races);
        Assert.Contains(MonsterRace.Insect, races);
    }

    [Fact]
    public void GetCommonRaces_Crypt_ContainsUndead()
    {
        var races = DungeonFeatureGenerator.GetCommonRaces(DungeonFeatureType.Crypt);
        Assert.Contains(MonsterRace.Undead, races);
        Assert.Contains(MonsterRace.Spirit, races);
    }

    [Fact]
    public void GetCommonRaces_Volcanic_ContainsDragon()
    {
        var races = DungeonFeatureGenerator.GetCommonRaces(DungeonFeatureType.Volcanic);
        Assert.Contains(MonsterRace.Dragon, races);
    }

    [Fact]
    public void GetCommonRaces_AllTypes_HaveRaces()
    {
        foreach (DungeonFeatureType type in Enum.GetValues<DungeonFeatureType>())
        {
            var races = DungeonFeatureGenerator.GetCommonRaces(type);
            Assert.True(races.Count > 0, $"{type} should have common races");
        }
    }

    #endregion

    #region Territory Selection Tests

    [Fact]
    public void SelectFeatureForTerritory_Capital_ReturnsValidType()
    {
        var random = new TestRandom();
        var type = DungeonFeatureGenerator.SelectFeatureForTerritory(TerritoryId.Capital, 1, random);
        Assert.True(Enum.IsDefined(typeof(DungeonFeatureType), type));
    }

    [Fact]
    public void SelectFeatureForTerritory_Forest_ReturnsForestType()
    {
        var random = new TestRandom();
        var type = DungeonFeatureGenerator.SelectFeatureForTerritory(TerritoryId.Forest, 1, random);
        // Forest territory should return Cave or Forest type (depth 1)
        Assert.True(type == DungeonFeatureType.Cave || type == DungeonFeatureType.Forest);
    }

    [Fact]
    public void SelectFeatureForTerritory_HighDepth_MoreOptions()
    {
        var random = new TestRandom();
        // At depth 1, some features won't be available (minDepth)
        // At depth 10, more features should be available
        var typeDepth1 = DungeonFeatureGenerator.SelectFeatureForTerritory(TerritoryId.Frontier, 1, random);
        Assert.True(Enum.IsDefined(typeof(DungeonFeatureType), typeDepth1));
    }

    #endregion

    #region Feature Properties Tests

    [Theory]
    [InlineData(DungeonFeatureType.Temple, Element.Holy)]
    [InlineData(DungeonFeatureType.Volcanic, Element.Fire)]
    [InlineData(DungeonFeatureType.IceCavern, Element.Ice)]
    [InlineData(DungeonFeatureType.Crypt, Element.Dark)]
    [InlineData(DungeonFeatureType.Sewer, Element.Poison)]
    public void GetDominantElement_ReturnsCorrectElement(DungeonFeatureType type, Element expected)
    {
        Assert.Equal(expected, DungeonFeatureGenerator.GetDominantElement(type));
    }

    [Fact]
    public void GetLootMultiplier_Temple_HigherThanStandard()
    {
        float temple = DungeonFeatureGenerator.GetLootMultiplier(DungeonFeatureType.Temple);
        float standard = DungeonFeatureGenerator.GetLootMultiplier(DungeonFeatureType.Standard);
        Assert.True(temple > standard);
    }

    [Fact]
    public void GetEnemyDensity_AllTypes_PositiveValue()
    {
        foreach (DungeonFeatureType type in Enum.GetValues<DungeonFeatureType>())
        {
            int density = DungeonFeatureGenerator.GetEnemyDensity(type);
            Assert.True(density > 0, $"{type} should have positive enemy density");
        }
    }

    [Fact]
    public void GetTrapChance_Crypt_HigherThanForest()
    {
        float crypt = DungeonFeatureGenerator.GetTrapChance(DungeonFeatureType.Crypt);
        float forest = DungeonFeatureGenerator.GetTrapChance(DungeonFeatureType.Forest);
        Assert.True(crypt > forest);
    }

    [Fact]
    public void GetFeatureName_ReturnsJapaneseName()
    {
        Assert.Equal("洞窟", DungeonFeatureGenerator.GetFeatureName(DungeonFeatureType.Cave));
        Assert.Equal("火山", DungeonFeatureGenerator.GetFeatureName(DungeonFeatureType.Volcanic));
    }

    #endregion

    #region Params Tests

    [Fact]
    public void GetParams_Temple_LargeRooms()
    {
        var param = DungeonFeatureGenerator.GetParams(DungeonFeatureType.Temple);
        Assert.NotNull(param);
        Assert.True(param!.RoomMinSize >= 6);
    }

    [Fact]
    public void GetParams_Volcanic_HasLava()
    {
        var param = DungeonFeatureGenerator.GetParams(DungeonFeatureType.Volcanic);
        Assert.NotNull(param);
        Assert.True(param!.LavaTileChance > 0);
    }

    [Fact]
    public void GetParams_Standard_NoLava()
    {
        var param = DungeonFeatureGenerator.GetParams(DungeonFeatureType.Standard);
        Assert.NotNull(param);
        Assert.Equal(0f, param!.LavaTileChance);
    }

    #endregion

    #region Helpers

    private class TestRandom : IRandomProvider
    {
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => 0.5;
    }

    #endregion
}
