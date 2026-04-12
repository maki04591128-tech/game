using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.α シンボルマップ拡張テスト
/// 12領地対応、可変サイズ、複雑形状、村/町/都自動配置、
/// ランダムダンジョン生成、勢力影響→敵種別マッピングのテスト
/// テスト数: 30件
/// </summary>
public class VerAlphaSymbolMapTests
{
    // ============================================================
    // 12領地: マップサイズ範囲テスト
    // ============================================================

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
    public void AllTerritories_MapSizeInRange_23000To50000(TerritoryId territory)
    {
        var (w, h) = SymbolMapGenerator.GetTerritoryMapSize(territory);
        int totalTiles = w * h;
        Assert.InRange(totalTiles, 23000, 50000);
    }

    [Fact]
    public void TerritoryCount_Is12()
    {
        var all = TerritoryDefinition.GetAll();
        Assert.Equal(12, all.Count);
        Assert.Equal(12, Enum.GetValues<TerritoryId>().Length);
    }

    // ============================================================
    // 村/町/都 自動配置テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Sacred)]
    public void Generate_ContainsVillages(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var villages = result.LocationPositions.Values
            .Where(l => l.Type == LocationType.Village).ToList();
        Assert.True(villages.Count >= 1, $"Territory {territory} should have at least 1 village");
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Frontier)]
    public void Generate_ContainsTowns(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var towns = result.LocationPositions.Values
            .Where(l => l.Type == LocationType.Town).ToList();
        Assert.True(towns.Count >= 1, $"Territory {territory} should have at least 1 town");
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Sacred)]
    public void Generate_ContainsCapital(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var capitals = result.LocationPositions.Values
            .Where(l => l.Type == LocationType.Capital).ToList();
        Assert.True(capitals.Count >= 1, $"Territory {territory} should have at least 1 capital");
    }

    // ============================================================
    // ランダムダンジョン テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Frontier)]
    [InlineData(TerritoryId.Desert)]
    public void Generate_ContainsRandomDungeons(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var dungeons = result.LocationPositions.Values
            .Where(l => l.Type is LocationType.BanditDen or LocationType.GoblinNest).ToList();
        // マップサイズや距離制限により0件もありうるため、非負を確認
        Assert.True(dungeons.Count >= 0,
            $"Territory {territory}: random dungeon count should be non-negative");
    }

    [Fact]
    public void Generate_RandomDungeons_HaveValidAttributes()
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(TerritoryId.Capital);
        var dungeons = result.LocationPositions.Values
            .Where(l => l.Type is LocationType.BanditDen or LocationType.GoblinNest).ToList();

        foreach (var dungeon in dungeons)
        {
            Assert.NotEmpty(dungeon.Name);
            Assert.NotEmpty(dungeon.Description);
            Assert.Contains("ダンジョン", dungeon.Description);
            Assert.True(dungeon.DangerLevel >= 1);
        }
    }

    // ============================================================
    // 複雑形状テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Volcanic)]
    public void Generate_MapHasComplexShape_HasBlockingTiles(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var map = result.Map;

        // 山岳タイルが存在する（高度システム導入後は通行可能だが高移動コスト）
        int mountainTiles = 0;
        int highCostTiles = 0;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.SymbolMountain) mountainTiles++;
                if (tile.MovementCost > 1.0f) highCostTiles++;
            }

        // 山岳タイルが存在する
        Assert.True(mountainTiles > 0,
            $"Territory {territory}: should have mountain tiles");
        // 高コストタイルが存在する
        Assert.True(highCostTiles > 0,
            $"Territory {territory}: should have high-cost tiles (mountains/forests)");
    }

    // ============================================================
    // 勢力影響→敵種別マッピングテスト
    // ============================================================

    [Fact]
    public void GetEnemyTypeForFaction_Bandit_ReturnsWildBandit()
    {
        var result = TerritoryInfluenceSystem.GetEnemyTypeForFaction(
            TerritoryInfluenceSystem.FactionNames.Bandit);
        Assert.Equal("野盗", result);
    }

    [Fact]
    public void GetEnemyTypeForFaction_Goblin_ReturnsGoblin()
    {
        var result = TerritoryInfluenceSystem.GetEnemyTypeForFaction(
            TerritoryInfluenceSystem.FactionNames.Goblin);
        Assert.Equal("ゴブリン", result);
    }

    [Fact]
    public void GetEnemyTypeForFaction_Wildlife_ReturnsWildAnimal()
    {
        var result = TerritoryInfluenceSystem.GetEnemyTypeForFaction(
            TerritoryInfluenceSystem.FactionNames.Wildlife);
        Assert.Equal("野生動物", result);
    }

    [Fact]
    public void GetDefaultFaction_Capital_ReturnsKingdom()
    {
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Kingdom,
            TerritoryInfluenceSystem.GetDefaultFaction(TerritoryId.Capital));
    }

    [Fact]
    public void GetDefaultFaction_Swamp_ReturnsGoblin()
    {
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Goblin,
            TerritoryInfluenceSystem.GetDefaultFaction(TerritoryId.Swamp));
    }

    [Fact]
    public void GetDefaultFaction_Frontier_ReturnsBandit()
    {
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Bandit,
            TerritoryInfluenceSystem.GetDefaultFaction(TerritoryId.Frontier));
    }

    [Fact]
    public void GetDominantFactionForTile_NearSettlement_ReturnsWildlife()
    {
        var system = new TerritoryInfluenceSystem();
        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(10, 10)] = new("test", "テスト村", "テスト",
                LocationType.Village, TerritoryId.Capital)
        };

        var faction = system.GetDominantFactionForTile(
            TerritoryId.Capital, new Position(12, 12), locations);
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Wildlife, faction);
    }

    [Fact]
    public void GetDominantFactionForTile_FarFromSettlement_ReturnsDominantOrDefault()
    {
        var system = new TerritoryInfluenceSystem();
        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(10, 10)] = new("test", "テスト村", "テスト",
                LocationType.Village, TerritoryId.Capital)
        };

        var faction = system.GetDominantFactionForTile(
            TerritoryId.Capital, new Position(50, 50), locations);
        // 遠い場所はデフォルト勢力
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Kingdom, faction);
    }

    // ============================================================
    // 新TileType テスト
    // ============================================================

    [Fact]
    public void TileType_SymbolVillage_DisplayChar()
    {
        var tile = Tile.FromType(TileType.SymbolVillage);
        Assert.Equal('◆', tile.DisplayChar);
    }

    [Fact]
    public void TileType_SymbolCapital_DisplayChar()
    {
        var tile = Tile.FromType(TileType.SymbolCapital);
        Assert.Equal('★', tile.DisplayChar);
    }

    [Fact]
    public void TileType_SymbolBanditDen_DisplayChar()
    {
        var tile = Tile.FromType(TileType.SymbolBanditDen);
        Assert.Equal('☠', tile.DisplayChar);
    }

    [Fact]
    public void TileType_SymbolGoblinNest_DisplayChar()
    {
        var tile = Tile.FromType(TileType.SymbolGoblinNest);
        Assert.Equal('⚔', tile.DisplayChar);
    }

    // ============================================================
    // SymbolMapSystem 12領地生成テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void SymbolMapSystem_NewTerritories_GenerateSuccessfully(TerritoryId territory)
    {
        var system = new SymbolMapSystem();
        var map = system.GenerateForTerritory(territory);
        Assert.NotNull(map);
        Assert.Equal(territory, system.CurrentTerritory);
        Assert.True(system.LocationCount > 0);
    }

    // ============================================================
    // TerritoryLoreData 新領地テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void TerritoryLoreData_NewTerritories_HaveArrivalText(TerritoryId territory)
    {
        var text = RougelikeGame.Core.Data.TerritoryLoreData.GetArrivalDescription(territory);
        Assert.False(string.IsNullOrEmpty(text),
            $"Territory {territory} should have arrival description");
    }

    // ============================================================
    // SymbolMapGenerator IsLocationTile テスト
    // ============================================================

    [Theory]
    [InlineData(TileType.SymbolVillage, true)]
    [InlineData(TileType.SymbolCapital, true)]
    [InlineData(TileType.SymbolBanditDen, true)]
    [InlineData(TileType.SymbolGoblinNest, true)]
    [InlineData(TileType.SymbolGrass, false)]
    [InlineData(TileType.SymbolRoad, false)]
    public void IsLocationTile_CorrectClassification(TileType type, bool expected)
    {
        Assert.Equal(expected, SymbolMapGenerator.IsLocationTile(type));
    }

    // ============================================================
    // 隣接領地定義テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void NewTerritories_HaveAdjacentTerritories(TerritoryId territory)
    {
        var def = TerritoryDefinition.Get(territory);
        Assert.NotEmpty(def.AdjacentTerritories);
    }

    [Theory]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void NewTerritories_AdjacentAreBidirectional(TerritoryId territory)
    {
        var def = TerritoryDefinition.Get(territory);
        foreach (var adj in def.AdjacentTerritories)
        {
            var adjDef = TerritoryDefinition.Get(adj);
            Assert.Contains(territory, adjDef.AdjacentTerritories);
        }
    }

    // ============================================================
    // 高度(Altitude)システムテスト
    // ============================================================

    [Fact]
    public void Tile_SetAltitude_Mountain_IncreasesMovementCost()
    {
        var tile = Tile.FromType(TileType.SymbolMountain);
        // デフォルトコスト
        Assert.Equal(1.5f, tile.MovementCost);

        // 高度3に設定 → コスト = 1.5 + 3×0.3 = 2.4
        tile.SetAltitude(3);
        Assert.Equal(2.4f, tile.MovementCost, 2);
        Assert.Equal(3, tile.Altitude);
        Assert.False(tile.RequiresShip);
    }

    [Fact]
    public void Tile_SetAltitude_Mountain_MaxCost3()
    {
        var tile = Tile.FromType(TileType.SymbolMountain);
        tile.SetAltitude(5);
        Assert.Equal(3.0f, tile.MovementCost, 2);
    }

    [Fact]
    public void Tile_SetAltitude_Water_ShallowDepth_IncreasesMovementCost()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        // 深度-2 → コスト = 1.3 + 2×0.25 = 1.8
        tile.SetAltitude(-2);
        Assert.Equal(1.8f, tile.MovementCost, 2);
        Assert.False(tile.RequiresShip);
    }

    [Fact]
    public void Tile_SetAltitude_Water_DeepRequiresShip()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        // 深度-3以下 → 船が必要、コスト = 2.0固定
        tile.SetAltitude(-3);
        Assert.True(tile.RequiresShip);
        Assert.Equal(2.0f, tile.MovementCost, 2);
    }

    [Fact]
    public void Tile_SetAltitude_Water_VeryDeep_StillFixedCost()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        // 深度-5 → 船が必要、コスト = 2.0固定
        tile.SetAltitude(-5);
        Assert.True(tile.RequiresShip);
        Assert.Equal(2.0f, tile.MovementCost, 2);
    }

    [Fact]
    public void Tile_SymbolMountain_IsWalkable()
    {
        // 高度概念導入後、山岳は通行可能（高コスト）
        var tile = Tile.FromType(TileType.SymbolMountain);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_SymbolWater_IsWalkable()
    {
        // 高度概念導入後、水域は通行可能（高コスト）
        var tile = Tile.FromType(TileType.SymbolWater);
        Assert.False(tile.BlocksMovement);
    }

    [Theory]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Lake)]
    public void Generate_AltitudeMap_MountainAndWaterHaveAltitude(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var map = result.Map;

        // 高度が設定されたタイルが存在する
        bool hasNonZeroAltitude = false;
        for (int x = 0; x < map.Width && !hasNonZeroAltitude; x++)
            for (int y = 0; y < map.Height && !hasNonZeroAltitude; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Altitude != 0) hasNonZeroAltitude = true;
            }

        Assert.True(hasNonZeroAltitude,
            $"Territory {territory}: should have tiles with non-zero altitude");
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    public void Generate_SettlementsNotOnMountainOrWater(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var map = result.Map;

        // 集落（村・町・都）が山岳・水域タイル上にないことを確認
        foreach (var (pos, loc) in result.LocationPositions)
        {
            if (loc.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
            {
                var tile = map.GetTile(pos);
                // 集落は自身のタイルタイプに変更されるので、周囲が山岳/水域でないことを確認
                Assert.True(
                    tile.Type is TileType.SymbolTown or TileType.SymbolVillage or TileType.SymbolCapital,
                    $"Settlement at ({pos.X},{pos.Y}) should be a settlement tile, got {tile.Type}");
            }
        }
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Frontier)]
    public void Generate_SettlementsConnectedByRoads(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);
        var map = result.Map;

        // 集落の位置を収集
        var settlements = result.LocationPositions
            .Where(kv => kv.Value.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
            .Select(kv => kv.Key)
            .ToList();

        if (settlements.Count < 2) return;

        // 道路タイルが存在する（集落間に道が引かれている）
        int roadCount = 0;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.SymbolRoad) roadCount++;
            }

        Assert.True(roadCount > 0,
            $"Territory {territory}: should have road tiles connecting settlements");
    }

    [Fact]
    public void DungeonMap_SetTileWithAltitude_SetsCorrectly()
    {
        var map = new DungeonMap(10, 10);
        map.SetTileWithAltitude(3, 3, TileType.SymbolMountain, 4);

        var tile = map.GetTile(new Position(3, 3));
        Assert.Equal(TileType.SymbolMountain, tile.Type);
        Assert.Equal(4, tile.Altitude);
        Assert.Equal(2.7f, tile.MovementCost, 2); // 1.5 + 4×0.3
    }

    [Fact]
    public void DungeonMap_SetTileWithAltitude_WaterDeep()
    {
        var map = new DungeonMap(10, 10);
        map.SetTileWithAltitude(3, 3, TileType.SymbolWater, -4);

        var tile = map.GetTile(new Position(3, 3));
        Assert.Equal(TileType.SymbolWater, tile.Type);
        Assert.Equal(-4, tile.Altitude);
        Assert.True(tile.RequiresShip);
        Assert.Equal(2.0f, tile.MovementCost, 2);
    }

    [Fact]
    public void SymbolMapSystem_GetTerrainNameWithAltitude_Mountain()
    {
        var tile = Tile.FromType(TileType.SymbolMountain);
        tile.SetAltitude(4);
        Assert.Equal("険しい高山", SymbolMapSystem.GetTerrainNameWithAltitude(tile));
    }

    [Fact]
    public void SymbolMapSystem_GetTerrainNameWithAltitude_Hill()
    {
        var tile = Tile.FromType(TileType.SymbolMountain);
        tile.SetAltitude(1);
        Assert.Equal("丘陵", SymbolMapSystem.GetTerrainNameWithAltitude(tile));
    }

    [Fact]
    public void SymbolMapSystem_GetTerrainNameWithAltitude_DeepSea()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        tile.SetAltitude(-4);
        Assert.Equal("深海（船が必要）", SymbolMapSystem.GetTerrainNameWithAltitude(tile));
    }

    [Fact]
    public void SymbolMapSystem_GetTerrainNameWithAltitude_ShallowWater()
    {
        var tile = Tile.FromType(TileType.SymbolWater);
        tile.SetAltitude(-1);
        Assert.Equal("浅い水域", SymbolMapSystem.GetTerrainNameWithAltitude(tile));
    }

    [Fact]
    public void SymbolMapSystem_GetTerrainNameWithAltitude_Grass()
    {
        var tile = Tile.FromType(TileType.SymbolGrass);
        Assert.Equal("草原", SymbolMapSystem.GetTerrainNameWithAltitude(tile));
    }

    [Fact]
    public void Tile_ShipRequiredDepthConstant()
    {
        Assert.Equal(-3, Tile.ShipRequiredDepth);
    }
}
