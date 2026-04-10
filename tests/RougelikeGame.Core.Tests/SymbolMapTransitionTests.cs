using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// シンボルマップ生成・遷移システムのテスト
/// </summary>
public class SymbolMapSystemTests
{
    #region SymbolMapGenerator Tests

    [Fact]
    public void Generate_Capital_ReturnsNonNullResult()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        Assert.NotNull(result);
        Assert.NotNull(result.Map);
        Assert.NotEmpty(result.LocationPositions);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void Generate_AllTerritories_ProducesValidMap(TerritoryId territory)
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(territory);

        var (expectedWidth, expectedHeight) = SymbolMapGenerator.GetTerritoryMapSize(territory);
        Assert.Equal(expectedWidth, result.Map.Width);
        Assert.Equal(expectedHeight, result.Map.Height);
        Assert.Equal(0, result.Map.Depth);
        Assert.True(result.LocationPositions.Count > 0);
        // マップのマス数が2300-5000の範囲内
        int totalTiles = result.Map.Width * result.Map.Height;
        Assert.InRange(totalTiles, 2300, 5000);
    }

    [Fact]
    public void Generate_Capital_PlacesAtLeastDefinedLocations()
    {
        var locations = LocationDefinition.GetSymbolLocations(TerritoryId.Capital);
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        // 定義済みロケーション + 自動生成の村/町/都/ランダムダンジョン
        Assert.True(result.LocationPositions.Count >= locations.Count,
            $"Expected at least {locations.Count} locations, got {result.LocationPositions.Count}");
    }

    [Fact]
    public void Generate_LocationsHaveAccessibleSurroundings()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        foreach (var (pos, _) in result.LocationPositions)
        {
            // ロケーション自体は歩行可能
            Assert.False(result.Map.GetTile(pos).BlocksMovement,
                $"Location at ({pos.X},{pos.Y}) should be walkable");

            // 少なくとも1つの隣接タイルが歩行可能
            bool hasAccess = false;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var neighbor = new Position(pos.X + dx, pos.Y + dy);
                    if (result.Map.IsInBounds(neighbor) && !result.Map.GetTile(neighbor).BlocksMovement)
                    {
                        hasAccess = true;
                    }
                }
            }
            Assert.True(hasAccess, $"Location at ({pos.X},{pos.Y}) has no accessible neighbors");
        }
    }

    [Fact]
    public void Generate_LocationTypeMappingIsCorrect()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        foreach (var (pos, loc) in result.LocationPositions)
        {
            var tile = result.Map.GetTile(pos);
            var expectedType = loc.Type switch
            {
                LocationType.Town => TileType.SymbolTown,
                LocationType.Village => TileType.SymbolVillage,
                LocationType.Capital => TileType.SymbolCapital,
                LocationType.Facility => TileType.SymbolFacility,
                LocationType.ReligiousSite => TileType.SymbolShrine,
                LocationType.Field => TileType.SymbolField,
                LocationType.Dungeon => TileType.SymbolDungeon,
                LocationType.BanditDen => TileType.SymbolBanditDen,
                LocationType.GoblinNest => TileType.SymbolGoblinNest,
                _ => TileType.SymbolField
            };
            Assert.Equal(expectedType, tile.Type);
        }
    }

    [Fact]
    public void Generate_Deterministic_SameSeedSameResult()
    {
        var generator = new SymbolMapGenerator();
        var result1 = generator.Generate(TerritoryId.Forest);
        var result2 = generator.Generate(TerritoryId.Forest);

        Assert.Equal(result1.LocationPositions.Count, result2.LocationPositions.Count);

        foreach (var (pos1, loc1) in result1.LocationPositions)
        {
            Assert.True(result2.LocationPositions.ContainsKey(pos1),
                $"Location {loc1.Name} position should be deterministic");
        }
    }

    [Fact]
    public void Generate_MapHasEntrance()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        Assert.NotNull(result.Map.EntrancePosition);
    }

    [Fact]
    public void Generate_MapContainsRoads()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        bool hasRoad = false;
        for (int x = 0; x < result.Map.Width; x++)
        {
            for (int y = 0; y < result.Map.Height; y++)
            {
                if (result.Map.GetTile(new Position(x, y)).Type == TileType.SymbolRoad)
                {
                    hasRoad = true;
                    break;
                }
            }
            if (hasRoad) break;
        }
        Assert.True(hasRoad, "Symbol map should contain roads");
    }

    [Fact]
    public void IsLocationTile_ReturnsCorrectResults()
    {
        Assert.True(SymbolMapGenerator.IsLocationTile(TileType.SymbolTown));
        Assert.True(SymbolMapGenerator.IsLocationTile(TileType.SymbolDungeon));
        Assert.True(SymbolMapGenerator.IsLocationTile(TileType.SymbolFacility));
        Assert.True(SymbolMapGenerator.IsLocationTile(TileType.SymbolShrine));
        Assert.True(SymbolMapGenerator.IsLocationTile(TileType.SymbolField));
        Assert.False(SymbolMapGenerator.IsLocationTile(TileType.SymbolGrass));
        Assert.False(SymbolMapGenerator.IsLocationTile(TileType.SymbolRoad));
        Assert.False(SymbolMapGenerator.IsLocationTile(TileType.Floor));
    }

    [Fact]
    public void IsSymbolMapTile_ReturnsCorrectResults()
    {
        Assert.True(SymbolMapGenerator.IsSymbolMapTile(TileType.SymbolGrass));
        Assert.True(SymbolMapGenerator.IsSymbolMapTile(TileType.SymbolForest));
        Assert.True(SymbolMapGenerator.IsSymbolMapTile(TileType.SymbolMountain));
        Assert.True(SymbolMapGenerator.IsSymbolMapTile(TileType.SymbolWater));
        Assert.True(SymbolMapGenerator.IsSymbolMapTile(TileType.SymbolRoad));
        Assert.False(SymbolMapGenerator.IsSymbolMapTile(TileType.Floor));
        Assert.False(SymbolMapGenerator.IsSymbolMapTile(TileType.Wall));
    }

    #endregion

    #region SymbolMapSystem Tests

    [Fact]
    public void SymbolMapSystem_GenerateForTerritory_SetsMap()
    {
        var system = new SymbolMapSystem();
        var map = system.GenerateForTerritory(TerritoryId.Capital);

        Assert.NotNull(system.CurrentMap);
        Assert.Equal(TerritoryId.Capital, system.CurrentTerritory);
        Assert.True(system.LocationCount > 0);
    }

    [Fact]
    public void SymbolMapSystem_GetLocationAt_ReturnsCorrectLocation()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var firstPos = allPositions.First();

        var location = system.GetLocationAt(firstPos.Key);
        Assert.NotNull(location);
        Assert.Equal(firstPos.Value.Id, location.Id);
    }

    [Fact]
    public void SymbolMapSystem_GetLocationAt_EmptyPosition_ReturnsNull()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var result = system.GetLocationAt(new Position(0, 0));
        Assert.Null(result);
    }

    [Fact]
    public void SymbolMapSystem_IsDungeonEntrance_IdentifiesDungeons()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var dungeon = allPositions.FirstOrDefault(p => p.Value.Type == LocationType.Dungeon);

        if (dungeon.Value != null)
        {
            Assert.True(system.IsDungeonEntrance(dungeon.Key));
        }
    }

    [Fact]
    public void SymbolMapSystem_IsTownEntrance_IdentifiesTowns()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var town = allPositions.FirstOrDefault(p =>
            p.Value.Type is LocationType.Town or LocationType.Village);

        if (town.Value != null)
        {
            Assert.True(system.IsTownEntrance(town.Key));
        }
    }

    [Fact]
    public void SymbolMapSystem_IsLocationSymbol_Works()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var firstPos = allPositions.First().Key;

        Assert.True(system.IsLocationSymbol(firstPos));
        Assert.False(system.IsLocationSymbol(new Position(0, 0)));
    }

    [Fact]
    public void SymbolMapSystem_GetLocationArrivalMessage_DungeonType()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var dungeon = allPositions.FirstOrDefault(p => p.Value.Type == LocationType.Dungeon);

        if (dungeon.Value != null)
        {
            var message = system.GetLocationArrivalMessage(dungeon.Key);
            Assert.Contains("ダンジョンに入る", message);
            Assert.Contains(dungeon.Value.Name, message);
        }
    }

    [Fact]
    public void SymbolMapSystem_GetLocationArrivalMessage_TownType()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var town = allPositions.FirstOrDefault(p =>
            p.Value.Type is LocationType.Town or LocationType.Village);

        if (town.Value != null)
        {
            var message = system.GetLocationArrivalMessage(town.Key);
            Assert.Contains("街に入る", message);
            Assert.Contains(town.Value.Name, message);
        }
    }

    [Fact]
    public void SymbolMapSystem_FindLocationPosition_ExistingId()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var first = allPositions.First();

        var pos = system.FindLocationPosition(first.Value.Id);
        Assert.NotNull(pos);
        Assert.Equal(first.Key, pos.Value);
    }

    [Fact]
    public void SymbolMapSystem_FindLocationPosition_NonExistingId()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var pos = system.FindLocationPosition("non_existing_id");
        Assert.Null(pos);
    }

    [Fact]
    public void SymbolMapSystem_Clear_ResetsState()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        Assert.NotNull(system.CurrentMap);
        Assert.True(system.LocationCount > 0);

        system.Clear();

        Assert.Null(system.CurrentMap);
        Assert.Null(system.CurrentTerritory);
        Assert.Equal(0, system.LocationCount);
    }

    #endregion

    #region TileType Symbol Map Tests

    [Fact]
    public void SymbolGrass_IsWalkable()
    {
        var tile = Tile.FromType(TileType.SymbolGrass);
        Assert.False(tile.BlocksMovement);
        Assert.False(tile.BlocksSight);
    }

    [Fact]
    public void SymbolForest_HasHigherMovementCost()
    {
        var tile = Tile.FromType(TileType.SymbolForest);
        Assert.False(tile.BlocksMovement);
        Assert.True(tile.MovementCost > 1.0f);
    }

    [Fact]
    public void SymbolMountain_IsPassableWithHighMovementCost()
    {
        var tile = Tile.FromType(TileType.SymbolMountain);
        Assert.True(tile.BlocksMovement); // FD-1: 山岳は通行不可
        Assert.True(tile.MovementCost >= 2.0f);
    }

    [Fact]
    public void SymbolWater_IsPassableWithHighMovementCost()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        Assert.True(tile.BlocksMovement); // FD-2: 水域は通行不可
        Assert.True(tile.MovementCost >= 1.5f);
    }

    [Fact]
    public void SymbolRoad_IsWalkable()
    {
        var tile = Tile.FromType(TileType.SymbolRoad);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void SymbolTown_IsWalkable()
    {
        var tile = Tile.FromType(TileType.SymbolTown);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void SymbolDungeon_IsWalkable()
    {
        var tile = Tile.FromType(TileType.SymbolDungeon);
        Assert.False(tile.BlocksMovement);
    }

    [Theory]
    [InlineData(TileType.SymbolGrass, ',')]
    [InlineData(TileType.SymbolForest, '♣')]
    [InlineData(TileType.SymbolMountain, '▲')]
    [InlineData(TileType.SymbolWater, '~')]
    [InlineData(TileType.SymbolRoad, '=')]
    [InlineData(TileType.SymbolTown, '■')]
    [InlineData(TileType.SymbolDungeon, '▼')]
    [InlineData(TileType.SymbolFacility, '☆')]
    [InlineData(TileType.SymbolShrine, '†')]
    [InlineData(TileType.SymbolField, '◇')]
    public void SymbolTile_DisplayChar_IsCorrect(TileType type, char expectedChar)
    {
        var tile = Tile.FromType(type);
        Assert.Equal(expectedChar, tile.DisplayChar);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_GenerateAndQuery_WorksForAllTerritories()
    {
        var system = new SymbolMapSystem();

        foreach (var territory in Enum.GetValues<TerritoryId>())
        {
            system.GenerateForTerritory(territory);

            Assert.NotNull(system.CurrentMap);
            Assert.Equal(territory, system.CurrentTerritory);
            Assert.True(system.LocationCount > 0,
                $"Territory {territory} should have locations");

            // 自動生成の村/町/都/ランダムダンジョンを含め、定義済み以上のロケーション数
            var locations = LocationDefinition.GetSymbolLocations(territory);
            Assert.True(system.LocationCount >= locations.Count,
                $"Territory {territory}: expected at least {locations.Count}, got {system.LocationCount}");
        }
    }

    [Fact]
    public void Integration_SymbolMapToDungeon_TransitionData()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        // ダンジョン入口を見つける
        var allPositions = system.GetAllLocationPositions();
        var dungeonEntry = allPositions
            .FirstOrDefault(p => p.Value.Type is LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest);

        if (dungeonEntry.Value != null)
        {
            // ダンジョン入口判定
            Assert.True(system.IsDungeonEntrance(dungeonEntry.Key));
            Assert.False(system.IsTownEntrance(dungeonEntry.Key));

            // メッセージ確認
            var msg = system.GetLocationArrivalMessage(dungeonEntry.Key);
            Assert.Contains("ダンジョン", msg);
        }
    }

    [Fact]
    public void Integration_SymbolMapToTown_TransitionData()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var townEntry = allPositions
            .FirstOrDefault(p => p.Value.Type is LocationType.Town or LocationType.Village);

        if (townEntry.Value != null)
        {
            Assert.True(system.IsTownEntrance(townEntry.Key));
            Assert.False(system.IsDungeonEntrance(townEntry.Key));

            var msg = system.GetLocationArrivalMessage(townEntry.Key);
            Assert.Contains("街に入る", msg);
        }
    }

    #endregion

    #region Terrain Field Entry Tests (Elona-style)

    [Theory]
    [InlineData(TileType.SymbolGrass)]
    [InlineData(TileType.SymbolForest)]
    [InlineData(TileType.SymbolMountain)]
    [InlineData(TileType.SymbolWater)]
    [InlineData(TileType.SymbolRoad)]
    public void IsEnterableTerrainTile_ReturnsTrue_ForTerrainTiles(TileType type)
    {
        Assert.True(SymbolMapSystem.IsEnterableTerrainTile(type));
    }

    [Theory]
    [InlineData(TileType.SymbolTown)]
    [InlineData(TileType.SymbolDungeon)]
    [InlineData(TileType.SymbolFacility)]
    [InlineData(TileType.SymbolShrine)]
    [InlineData(TileType.SymbolField)]
    [InlineData(TileType.Floor)]
    [InlineData(TileType.Wall)]
    public void IsEnterableTerrainTile_ReturnsFalse_ForNonTerrainTiles(TileType type)
    {
        Assert.False(SymbolMapSystem.IsEnterableTerrainTile(type));
    }

    [Theory]
    [InlineData(TileType.SymbolGrass, "草原")]
    [InlineData(TileType.SymbolForest, "森林")]
    [InlineData(TileType.SymbolMountain, "山岳地帯")]
    [InlineData(TileType.SymbolWater, "水辺")]
    [InlineData(TileType.SymbolRoad, "街道")]
    public void GetTerrainName_ReturnsCorrectName(TileType type, string expectedName)
    {
        Assert.Equal(expectedName, SymbolMapSystem.GetTerrainName(type));
    }

    [Fact]
    public void CanEnterField_ReturnsTrue_ForTerrainTileWithoutLocation()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        // シンボルマップ上でロケーション未配置の草原タイルを探す
        var map = system.CurrentMap!;
        Position? grassPos = null;
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);
                if (SymbolMapSystem.IsEnterableTerrainTile(tile.Type)
                    && system.GetLocationAt(pos) == null)
                {
                    grassPos = pos;
                    break;
                }
            }
            if (grassPos.HasValue) break;
        }

        Assert.NotNull(grassPos);
        Assert.True(system.CanEnterField(grassPos.Value));
    }

    [Fact]
    public void CanEnterField_ReturnsFalse_ForDungeonLocation()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var dungeon = allPositions.FirstOrDefault(p => p.Value.Type == LocationType.Dungeon);

        if (dungeon.Value != null)
        {
            Assert.False(system.CanEnterField(dungeon.Key));
        }
    }

    [Fact]
    public void CanEnterField_ReturnsTrue_ForTownLocation()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);

        var allPositions = system.GetAllLocationPositions();
        var town = allPositions.FirstOrDefault(p =>
            p.Value.Type is LocationType.Town or LocationType.Village);

        if (town.Value != null)
        {
            Assert.True(system.CanEnterField(town.Key));
        }
    }

    #endregion

    #region LocationMapGenerator Terrain Field Map Tests

    [Theory]
    [InlineData(TileType.SymbolGrass)]
    [InlineData(TileType.SymbolForest)]
    [InlineData(TileType.SymbolMountain)]
    [InlineData(TileType.SymbolWater)]
    [InlineData(TileType.SymbolRoad)]
    public void GenerateTerrainFieldMap_ReturnsValidMap(TileType type)
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTerrainFieldMap(type, new Position(5, 5));

        Assert.NotNull(map);
        Assert.Equal(60, map.Width);
        Assert.Equal(30, map.Height);
        Assert.NotNull(map.EntrancePosition);
    }

    [Fact]
    public void GenerateTerrainFieldMap_MapNameContainsPosition()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTerrainFieldMap(TileType.SymbolForest, new Position(10, 15));

        Assert.Contains("forest", map.Name);
        Assert.Contains("10", map.Name);
        Assert.Contains("15", map.Name);
    }

    [Fact]
    public void GenerateGrasslandMap_HasSparseVegetation()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateGrasslandMap("test_grass");

        int treeCount = 0;
        int grassCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.Tree) treeCount++;
                if (tile.Type == TileType.Grass) grassCount++;
            }
        }

        Assert.True(grassCount > treeCount, "草原マップは草地が木よりも多いはず");
    }

    [Fact]
    public void GenerateForestMap_HasDenseVegetation()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateForestMap("test_forest");

        int treeCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTile(new Position(x, y)).Type == TileType.Tree) treeCount++;
            }
        }

        Assert.True(treeCount > 100, $"森林マップは木が100本以上あるはず（実際: {treeCount}）");
    }

    [Fact]
    public void GenerateMountainMap_HasWallTerrain()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateMountainMap("test_mountain");

        int wallCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTile(new Position(x, y)).Type == TileType.Wall) wallCount++;
            }
        }

        Assert.True(wallCount > 50, $"山岳マップは壁が50以上あるはず（実際: {wallCount}）");
    }

    [Fact]
    public void GenerateWaterfrontMap_HasWater()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateWaterfrontMap("test_water");

        int waterCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTile(new Position(x, y)).Type == TileType.Water) waterCount++;
            }
        }

        Assert.True(waterCount > 200, $"水辺マップは水タイルが200以上あるはず（実際: {waterCount}）");
    }

    [Fact]
    public void GenerateRoadFieldMap_HasFloorRoad()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateRoadFieldMap("test_road");

        int floorCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTile(new Position(x, y)).Type == TileType.Floor) floorCount++;
            }
        }

        Assert.True(floorCount > 50, $"道沿いマップはFloorタイルが50以上あるはず（実際: {floorCount}）");
    }

    #endregion
}
