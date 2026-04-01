using Xunit;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// LocationMapGenerator テスト
/// 各ロケーションタイプの形だけのマップ生成を検証
/// </summary>
public class LocationMapGeneratorTests
{
    #region Town Map Tests

    [Fact]
    public void GenerateTownMap_ReturnsValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("capital_market", "中央市場");

        Assert.NotNull(map);
        Assert.Equal(50, map.Width);
        Assert.Equal(30, map.Height);
        Assert.Equal("capital_market", map.Name);
    }

    [Fact]
    public void GenerateTownMap_HasStairsUp()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("capital_market", "中央市場");

        // 町マップには入口（Entrance）が配置される
        Assert.NotNull(map.EntrancePosition);
    }

    [Fact]
    public void GenerateTownMap_HasWalkableArea()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("capital_market", "中央市場");

        int walkableCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (!map[x, y].BlocksMovement) walkableCount++;
            }
        }

        Assert.True(walkableCount > 100, $"町マップに十分な歩行可能タイルがない: {walkableCount}");
    }

    [Fact]
    public void GenerateTownMap_HasFountain()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("capital_market", "中央市場");

        bool hasFountain = false;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Fountain) hasFountain = true;
            }
        }

        Assert.True(hasFountain, "町マップに噴水が配置されていない");
    }

    [Fact]
    public void GenerateTownMap_HasDoors()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("capital_market", "中央市場");

        int entranceCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.BuildingEntrance) entranceCount++;
            }
        }

        Assert.True(entranceCount >= 4, $"町マップに建物入口が不足: {entranceCount}");
    }

    #endregion

    #region Village Map Tests

    [Fact]
    public void GenerateVillageMap_ReturnsValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateVillageMap("forest_herbalist", "薬師の村");

        Assert.NotNull(map);
        Assert.Equal(40, map.Width);
        Assert.Equal(25, map.Height);
        Assert.Equal("forest_herbalist", map.Name);
    }

    [Fact]
    public void GenerateVillageMap_HasStairsUp()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateVillageMap("forest_herbalist", "薬師の村");

        Assert.NotNull(map.EntrancePosition);
    }

    [Fact]
    public void GenerateVillageMap_HasFountain()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateVillageMap("forest_herbalist", "薬師の村");

        bool hasFountain = false;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Fountain) hasFountain = true;
            }
        }

        Assert.True(hasFountain, "村マップに井戸（噴水）が配置されていない");
    }

    #endregion

    #region Facility Map Tests

    [Fact]
    public void GenerateFacilityMap_ReturnsValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFacilityMap("capital_guild", "冒険者ギルド本部");

        Assert.NotNull(map);
        Assert.Equal(30, map.Width);
        Assert.Equal(20, map.Height);
        Assert.Equal("capital_guild", map.Name);
    }

    [Fact]
    public void GenerateFacilityMap_HasStairsUp()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFacilityMap("capital_guild", "冒険者ギルド本部");

        Assert.NotNull(map.StairsUpPosition);
    }

    [Fact]
    public void GenerateFacilityMap_HasAltar()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFacilityMap("capital_guild", "冒険者ギルド本部");

        bool hasAltar = false;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Altar) hasAltar = true;
            }
        }

        Assert.True(hasAltar, "施設マップに祭壇が配置されていない");
    }

    [Fact]
    public void GenerateFacilityMap_HasPillars()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFacilityMap("capital_guild", "冒険者ギルド本部");

        int pillarCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Pillar) pillarCount++;
            }
        }

        Assert.True(pillarCount > 0, "施設マップに柱（カウンター）が配置されていない");
    }

    #endregion

    #region Shrine Map Tests

    [Fact]
    public void GenerateShrineMap_ReturnsValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateShrineMap("capital_cathedral", "大聖堂");

        Assert.NotNull(map);
        Assert.Equal(35, map.Width);
        Assert.Equal(25, map.Height);
        Assert.Equal("capital_cathedral", map.Name);
    }

    [Fact]
    public void GenerateShrineMap_HasStairsUp()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateShrineMap("capital_cathedral", "大聖堂");

        Assert.NotNull(map.StairsUpPosition);
    }

    [Fact]
    public void GenerateShrineMap_HasAltar()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateShrineMap("capital_cathedral", "大聖堂");

        bool hasAltar = false;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Altar) hasAltar = true;
            }
        }

        Assert.True(hasAltar, "神殿マップに祭壇が配置されていない");
    }

    [Fact]
    public void GenerateShrineMap_HasPillars()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateShrineMap("capital_cathedral", "大聖堂");

        int pillarCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Pillar) pillarCount++;
            }
        }

        Assert.True(pillarCount >= 4, $"神殿マップに柱が不足: {pillarCount}");
    }

    #endregion

    #region Field Map Tests

    [Fact]
    public void GenerateFieldMap_ReturnsValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFieldMap("capital_slum", "貧民街");

        Assert.NotNull(map);
        Assert.Equal(60, map.Width);
        Assert.Equal(30, map.Height);
        Assert.Equal("capital_slum", map.Name);
    }

    [Fact]
    public void GenerateFieldMap_HasStairsUp()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFieldMap("capital_slum", "貧民街");

        Assert.NotNull(map.EntrancePosition);
    }

    [Fact]
    public void GenerateFieldMap_HasTrees()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFieldMap("forest_deep", "深緑の森");

        int treeCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Tree) treeCount++;
            }
        }

        Assert.True(treeCount > 50, $"フィールドマップに木が不足: {treeCount}");
    }

    [Fact]
    public void GenerateFieldMap_HasWater()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateFieldMap("forest_deep", "深緑の森");

        int waterCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y].Type == TileType.Water) waterCount++;
            }
        }

        Assert.True(waterCount > 10, $"フィールドマップに水域が不足: {waterCount}");
    }

    #endregion

    #region GenerateForLocation Tests

    [Theory]
    [InlineData(LocationType.Town, 50, 30)]
    [InlineData(LocationType.Village, 40, 25)]
    [InlineData(LocationType.Facility, 30, 20)]
    [InlineData(LocationType.ReligiousSite, 35, 25)]
    [InlineData(LocationType.Field, 60, 30)]
    public void GenerateForLocation_CorrectSize(LocationType type, int expectedWidth, int expectedHeight)
    {
        var location = new LocationDefinition("test_id", "テスト", "テスト説明", type, TerritoryId.Capital);
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateForLocation(location);

        Assert.Equal(expectedWidth, map.Width);
        Assert.Equal(expectedHeight, map.Height);
    }

    [Theory]
    [InlineData(LocationType.Town)]
    [InlineData(LocationType.Village)]
    [InlineData(LocationType.Facility)]
    [InlineData(LocationType.ReligiousSite)]
    [InlineData(LocationType.Field)]
    public void GenerateForLocation_HasStairsUp(LocationType type)
    {
        var location = new LocationDefinition("test_id", "テスト", "テスト説明", type, TerritoryId.Capital);
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateForLocation(location);

        // ロケーションマップには入口が配置される
        if (type is LocationType.Facility or LocationType.ReligiousSite)
            Assert.NotNull(map.StairsUpPosition);
        else
            Assert.NotNull(map.EntrancePosition);
    }

    [Fact]
    public void GenerateForLocation_HasWalkableArea()
    {
        foreach (LocationType type in new[] { LocationType.Town, LocationType.Village,
            LocationType.Facility, LocationType.ReligiousSite, LocationType.Field })
        {
            var location = new LocationDefinition("test_id", "テスト", "テスト説明", type, TerritoryId.Capital);
            var generator = new LocationMapGenerator(42);
            var map = generator.GenerateForLocation(location);

            int walkableCount = 0;
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    if (!map[x, y].BlocksMovement) walkableCount++;
            Assert.True(walkableCount > 10, $"{type}のマップに歩行可能タイルが不足: {walkableCount}");
        }
    }

    #endregion

    #region Start Location Map Tests

    [Theory]
    [InlineData("capital_guild")]
    [InlineData("capital_barracks")]
    [InlineData("capital_academy")]
    [InlineData("capital_market")]
    [InlineData("capital_slums")]
    [InlineData("capital_manor")]
    [InlineData("capital_cathedral")]
    [InlineData("capital_prison")]
    [InlineData("capital_monastery")]
    [InlineData("forest_village")]
    [InlineData("mountain_hold")]
    [InlineData("coast_port")]
    [InlineData("underground_ruins")]
    [InlineData("dark_sanctuary")]
    [InlineData("fallen_temple")]
    [InlineData("swamp_den")]
    [InlineData("wanderer_camp")]
    public void GenerateStartLocationMap_AllStartLocations_ReturnValidMap(string mapName)
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateStartLocationMap(mapName);

        Assert.NotNull(map);
        Assert.Equal(mapName, map.Name);
        Assert.True(map.Width > 0);
        Assert.True(map.Height > 0);

        // ロケーションマップには出入り口（階段またはエントランス）が配置される
        Assert.True(map.StairsUpPosition != null || map.EntrancePosition != null,
            $"マップ '{mapName}' に入口（StairsUp or Entrance）が配置されていない");
    }

    #endregion

    #region All WorldMap Locations Tests

    [Fact]
    public void GenerateForLocation_AllWorldMapLocations_NonDungeon_ReturnValidMap()
    {
        var generator = new LocationMapGenerator(42);
        var allLocations = LocationDefinition.GetAll();

        foreach (var kvp in allLocations)
        {
            var location = kvp.Value;
            if (location.Type == LocationType.Dungeon) continue;

            var map = generator.GenerateForLocation(location);

            Assert.NotNull(map);
            Assert.True(map.Width > 0);
            Assert.True(map.Height > 0);
        }
    }

    [Fact]
    public void GenerateForLocation_AllTerritories_TownLocations_Exist()
    {
        var generator = new LocationMapGenerator(42);

        foreach (TerritoryId territory in Enum.GetValues<TerritoryId>())
        {
            var locations = LocationDefinition.GetByTerritory(territory);
            var nonDungeonLocations = locations.Where(l => l.Type != LocationType.Dungeon).ToList();

            Assert.True(nonDungeonLocations.Count > 0,
                $"領地 {territory} にダンジョン以外のロケーションがない");

            foreach (var location in nonDungeonLocations)
            {
                var map = generator.GenerateForLocation(location);
                Assert.NotNull(map);
                Assert.True(map.Width > 0);
            }
        }
    }

    #endregion
}
