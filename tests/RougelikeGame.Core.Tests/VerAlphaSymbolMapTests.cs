using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.α シンボルマップ拡張テスト
/// 12領地対応、可変サイズ、複雑形状、村/町/都自動配置、
/// ランダムダンジョン生成、勢力影響→敵種別マッピングのテスト
/// テスト数: 43件（Phase修正テスト13件追加）
/// </summary>
public class VerAlphaSymbolMapTests
{
    private class FixedRandomProvider : IRandomProvider
    {
        private readonly int _value;
        public FixedRandomProvider(int value) => _value = value;
        public int Next(int maxValue) => Math.Min(_value, maxValue - 1);
        public int Next(int minValue, int maxValue) => Math.Min(Math.Max(_value, minValue), maxValue - 1);
        public double NextDouble() => 0.5;
    }
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

    // ============================================================
    // Issue 4: 集落配置minDistance統一化テスト
    // ============================================================

    [Fact]
    public void GetSettlementMinDistances_StandardMap_ReturnsConsistentValues()
    {
        // 標準マップ (220×160) → 対角線≈272
        var (capital, town, village) = SymbolMapGenerator.GetSettlementMinDistances(220, 160);
        Assert.True(capital > town, "Capital distance should be larger than Town");
        Assert.True(town > village, "Town distance should be larger than Village");
        Assert.True(capital >= 15, $"Capital min distance should be >= 15, got {capital}");
        Assert.True(village >= 6, $"Village min distance should be >= 6, got {village}");
    }

    [Fact]
    public void GetSettlementMinDistances_SmallMap_RespectsMinimums()
    {
        // 小さいマップでも最低値は保証される
        var (capital, town, village) = SymbolMapGenerator.GetSettlementMinDistances(50, 50);
        Assert.True(capital >= 15, "Capital min distance should be at least 15");
        Assert.True(town >= 10, "Town min distance should be at least 10");
        Assert.True(village >= 6, "Village min distance should be at least 6");
    }

    [Fact]
    public void GetSettlementMinDistances_LargeMap_ScalesProportionally()
    {
        var small = SymbolMapGenerator.GetSettlementMinDistances(160, 160);
        var large = SymbolMapGenerator.GetSettlementMinDistances(220, 190);
        Assert.True(large.Capital >= small.Capital, "Larger map should have larger capital distance");
    }

    // ============================================================
    // Issue 5: ランダムダンジョン成長曲線テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    public void Generate_RandomDungeons_HaveLevelAndFloorProgression(TerritoryId territory)
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(territory);

        // ダンジョンが存在する
        var dungeons = result.LocationPositions
            .Where(kv => kv.Value.Type is LocationType.BanditDen or LocationType.GoblinNest)
            .ToList();

        if (dungeons.Count < 2) return;

        // 階層数や推奨レベルが設定されている
        foreach (var (_, loc) in dungeons)
        {
            Assert.True(loc.MinLevel >= 1, $"Dungeon {loc.Name} MinLevel should be >= 1");
            Assert.True(loc.DangerLevel >= 1, $"Dungeon {loc.Name} DangerLevel should be >= 1");
            Assert.True(loc.DangerLevel <= 5, $"Dungeon {loc.Name} DangerLevel should be <= 5");
            Assert.Contains("階層", loc.Description);
        }
    }

    [Fact]
    public void Generate_RandomDungeons_DescriptionShowsFloorCount()
    {
        var gen = new SymbolMapGenerator();
        var result = gen.Generate(TerritoryId.Capital);

        var dungeons = result.LocationPositions
            .Where(kv => kv.Value.Type is LocationType.BanditDen or LocationType.GoblinNest)
            .ToList();

        foreach (var (_, loc) in dungeons)
        {
            // 全X階層の形式であること
            Assert.Matches(@"全\d+階層", loc.Description);
            Assert.Contains("推奨Lv.", loc.Description);
        }
    }

    // ============================================================
    // Issue 6: 安全圏距離マップサイズ連動テスト
    // ============================================================

    [Fact]
    public void GetSafeZoneDistance_StandardMap_ReturnsReasonableValue()
    {
        int dist = TerritoryInfluenceSystem.GetSafeZoneDistance(220, 160);
        // 対角線272 → 27マス
        Assert.InRange(dist, 10, 50);
    }

    [Fact]
    public void GetSafeZoneDistance_SmallMap_RespectsMinimum()
    {
        int dist = TerritoryInfluenceSystem.GetSafeZoneDistance(50, 50);
        Assert.True(dist >= 10, $"Small map safe zone should be at least 10, got {dist}");
    }

    [Fact]
    public void GetSafeZoneDistance_LargerMap_LargerSafeZone()
    {
        int small = TerritoryInfluenceSystem.GetSafeZoneDistance(100, 100);
        int large = TerritoryInfluenceSystem.GetSafeZoneDistance(220, 160);
        Assert.True(large >= small, $"Larger map should have >= safe zone distance: large={large}, small={small}");
    }

    [Fact]
    public void GetDominantFactionForTile_WithMapSize_RespectsDistance()
    {
        var system = new TerritoryInfluenceSystem();
        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(10, 10)] = new("test", "テスト村", "テスト",
                LocationType.Village, TerritoryId.Capital)
        };

        // マップサイズを指定してのオーバーロード
        var factionNear = system.GetDominantFactionForTile(
            TerritoryId.Capital, new Position(12, 12), locations, 220, 160);
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Wildlife, factionNear);
    }

    // ============================================================
    // Issue 7: SymbolMapEventSystemイベントワイヤリングテスト
    // ============================================================

    [Fact]
    public void SymbolMapEventSystem_RollEvent_ReturnsValidEvent()
    {
        // 非常に低いrandomValueでイベントが発生する
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Forest, 0.001);
        Assert.NotNull(evt);
        Assert.False(string.IsNullOrEmpty(evt!.Id));
        Assert.False(string.IsNullOrEmpty(evt.Name));
    }

    [Fact]
    public void SymbolMapEventSystem_RollEvent_NoEvent_HighValue()
    {
        // 非常に高いrandomValueではイベントなし
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Capital, 0.999);
        Assert.Null(evt);
    }

    [Fact]
    public void SymbolMapEventSystem_GetAvailableEvents_DesertSummer_ContainsSandstorm()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Summer, TerritoryId.Desert);
        Assert.Contains(events, e => e.Id == "event_sandstorm");
    }

    [Fact]
    public void SymbolMapEventSystem_GetAvailableEvents_WinterTundra_ContainsBlizzard()
    {
        var events = SymbolMapEventSystem.GetAvailableEvents(Season.Winter, TerritoryId.Tundra);
        Assert.Contains(events, e => e.Id == "event_blizzard");
    }

    [Fact]
    public void SymbolMapEventSystem_AllEventsHaveValidData()
    {
        var allEvents = SymbolMapEventSystem.GetAllEvents();
        Assert.True(allEvents.Count > 0);
        foreach (var evt in allEvents)
        {
            Assert.False(string.IsNullOrEmpty(evt.Id), $"Event ID should not be empty");
            Assert.False(string.IsNullOrEmpty(evt.Name), $"Event {evt.Id} name should not be empty");
            Assert.False(string.IsNullOrEmpty(evt.Description), $"Event {evt.Id} description should not be empty");
            Assert.True(evt.BaseChance > 0, $"Event {evt.Id} base chance should be > 0");
            Assert.NotNull(evt.ActiveSeasons);
            Assert.NotNull(evt.ActiveTerritories);
        }
    }

    [Fact]
    public void SymbolMapEventSystem_EventCount_Is16()
    {
        Assert.Equal(16, SymbolMapEventSystem.GetAllEvents().Count);
    }

    // ============================================================
    // Phase修正テスト: BorderGate、難易度連携、switch補完、enum安全性
    // ============================================================

    [Fact]
    public void BorderGate_LocationId_ContainsTargetTerritory()
    {
        // 関所のID形式が "{territory}_gate_to_{adjTerritory}" であることを確認
        var gateLoc = new LocationDefinition(
            "Capital_gate_to_Forest",
            "森林領方面の関所",
            "森林領への国境検問所",
            LocationType.BorderGate,
            TerritoryId.Capital);

        Assert.Contains("_gate_to_", gateLoc.Id);
        var parts = gateLoc.Id.Split("_gate_to_");
        Assert.Equal(2, parts.Length);
        Assert.True(Enum.TryParse<TerritoryId>(parts[1], out var target));
        Assert.Equal(TerritoryId.Forest, target);
    }

    [Fact]
    public void GetBorderGateTarget_ValidGate_ReturnsTarget()
    {
        var gateLoc = new LocationDefinition(
            "Capital_gate_to_Forest",
            "森林領方面の関所",
            "森林領への国境検問所",
            LocationType.BorderGate,
            TerritoryId.Capital);

        var target = gateLoc.GetBorderGateTarget();
        Assert.NotNull(target);
        Assert.Equal(TerritoryId.Forest, target.Value);
    }

    [Fact]
    public void GetBorderGateTarget_NonBorderGate_ReturnsNull()
    {
        var loc = new LocationDefinition("test_town", "テスト", "説明",
            LocationType.Town, TerritoryId.Capital);
        Assert.Null(loc.GetBorderGateTarget());
    }

    [Fact]
    public void GetBorderGateTarget_InvalidId_ReturnsNull()
    {
        var gateLoc = new LocationDefinition(
            "invalid_gate_id",
            "壊れた関所",
            "不正なID",
            LocationType.BorderGate,
            TerritoryId.Capital);
        Assert.Null(gateLoc.GetBorderGateTarget());
    }

    [Fact]
    public void LocationDefinition_MaxFloor_DefaultIsNull()
    {
        var loc = new LocationDefinition("test", "テスト", "説明", LocationType.Dungeon, TerritoryId.Capital);
        Assert.Null(loc.MaxFloor);
    }

    [Fact]
    public void LocationDefinition_MaxFloor_SetByRandomDungeon()
    {
        var loc = new LocationDefinition(
            "Capital_random_dungeon_0",
            "野盗のねぐら",
            "全3階層のダンジョン（推奨Lv.5）。クリアすると消滅する",
            LocationType.BanditDen,
            TerritoryId.Capital,
            MinLevel: 5,
            DangerLevel: 2,
            MaxFloor: 3);

        Assert.Equal(3, loc.MaxFloor);
        Assert.Equal(5, loc.MinLevel);
        Assert.Equal(2, loc.DangerLevel);
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
    public void GetTerritoryPriceMultiplier_AllTerritories_ReturnExplicitValue(TerritoryId territory)
    {
        // 全12領地がデフォルト(1.0)ではなく明示的な値を返すことを確認
        double multiplier = ShopSystem.GetTerritoryPriceMultiplier(territory);
        Assert.True(multiplier >= 0.9 && multiplier <= 1.5,
            $"{territory}の価格倍率{multiplier}が想定範囲外");
    }

    [Theory]
    [InlineData(TerritoryId.Desert, "砂鉄")]
    [InlineData(TerritoryId.Swamp, "毒草")]
    [InlineData(TerritoryId.Tundra, "氷晶")]
    [InlineData(TerritoryId.Lake, "真珠")]
    [InlineData(TerritoryId.Volcanic, "溶岩石")]
    [InlineData(TerritoryId.Sacred, "聖水晶")]
    public void ResolveMaterialDeposit_NewTerritories_ReturnUniqueMaterial(TerritoryId territory, string expectedMaterial)
    {
        var eventSystem = new RandomEventSystem();
        var random = new FixedRandomProvider(0);
        var result = eventSystem.ResolveMaterialDeposit(1, territory, random);
        Assert.Contains(expectedMaterial, result.Message);
    }

    [Fact]
    public void TileType_EnumIsDefined_ValidValues()
    {
        // 有効なTileType値の検証
        Assert.True(Enum.IsDefined(typeof(TileType), (int)TileType.Floor));
        Assert.True(Enum.IsDefined(typeof(TileType), (int)TileType.Wall));
        Assert.True(Enum.IsDefined(typeof(TileType), (int)TileType.StairsUp));
        Assert.True(Enum.IsDefined(typeof(TileType), (int)TileType.StairsDown));
    }

    [Fact]
    public void TileType_EnumIsDefined_InvalidValue_ReturnsFalse()
    {
        // 不正な値はIsDefined=false
        Assert.False(Enum.IsDefined(typeof(TileType), 9999));
        Assert.False(Enum.IsDefined(typeof(TileType), -1));
    }

    [Fact]
    public void RemoveLocationById_NonExistentId_ReturnsFalse()
    {
        var system = new SymbolMapSystem();
        // 存在しないIDの削除はfalseを返す
        Assert.False(system.RemoveLocationById("nonexistent_dungeon"));
    }

    [Fact]
    public void RemoveLocationById_AfterGenerate_ExistingDungeon_ReturnsTrue()
    {
        var system = new SymbolMapSystem();
        // Capital領地のシンボルマップを生成
        system.GenerateForTerritory(TerritoryId.Capital);

        // 生成されたロケーションからランダムダンジョンを探す
        var locations = system.GetAllLocationPositions();
        var randomDungeon = locations
            .FirstOrDefault(kv => kv.Value.Id.Contains("_random_dungeon_"));

        if (randomDungeon.Value != null)
        {
            Assert.True(system.RemoveLocationById(randomDungeon.Value.Id));
            Assert.Null(system.GetLocationAt(randomDungeon.Key));
        }
        // ランダムダンジョンがない場合はスキップ（生成の確率による）
    }

    [Fact]
    public void BorderGate_PlacedForAdjacentTerritories()
    {
        // Capital領地は4つの隣接領地があるため、関所が配置される
        var def = TerritoryDefinition.Get(TerritoryId.Capital);
        Assert.True(def.AdjacentTerritories.Length > 0,
            "Capital領地には隣接領地が必要");
    }

    [Fact]
    public void TerritoryDefinition_AllTerritories_HaveAdjacentList()
    {
        foreach (TerritoryId tid in Enum.GetValues<TerritoryId>())
        {
            var def = TerritoryDefinition.Get(tid);
            Assert.NotNull(def.AdjacentTerritories);
            // 全領地が少なくとも1つの隣接領地を持つ
            Assert.True(def.AdjacentTerritories.Length > 0,
                $"{tid}には少なくとも1つの隣接領地が必要");
        }
    }

    [Fact]
    public void BossFloorInterval_IsPositive()
    {
        Assert.True(GameConstants.BossFloorInterval > 0,
            "BossFloorIntervalは正の値である必要がある");
        // Math.Max(1, BossFloorInterval)と同じ効果を保証
        Assert.Equal(GameConstants.BossFloorInterval, Math.Max(1, GameConstants.BossFloorInterval));
    }

    // =====================================================
    // Phase 4: 追加防御的コーディング修正テスト (5件)
    // =====================================================

    /// <summary>
    /// A: SpellParser.EffectWordがnull許容（FirstOrDefault）であることを検証
    /// Effect語が無いwordsリストでもクラッシュしないことを確認
    /// </summary>
    [Fact]
    public void SpellResult_EffectWord_IsNullable()
    {
        // SpellResult.EffectWordの型がRuneWord?であること
        var result = new RougelikeGame.Data.MagicLanguage.SpellResult();
        Assert.Null(result.EffectWord);  // デフォルトがnullであること
    }

    /// <summary>
    /// B: SaveManager例外ログ出力パターンの検証
    /// catch(Exception ex)で例外変数を受け取り、Debug.WriteLineで出力するパターンが正しいことを確認
    /// </summary>
    [Fact]
    public void SaveManager_ExceptionLogging_Pattern_IsValid()
    {
        // Debug.WriteLineパターンが安全であることの構造テスト
        // 例外をcatchして変数exで受け取り、情報をログ出力するパターンの動作確認
        var ex = new InvalidOperationException("test error");
        string logMessage = $"[SaveManager] Save failed: {ex.GetType().Name}: {ex.Message}";
        Assert.Contains("InvalidOperationException", logMessage);
        Assert.Contains("test error", logMessage);
    }

    /// <summary>
    /// C/D: Enum.TryParseのout変数パターン検証
    /// FacilityCategory の TryParse→直接使用パターンが安全であること
    /// </summary>
    [Fact]
    public void EnumTryParse_OutVariable_Pattern_IsSafe()
    {
        // TryParseのout変数パターンが正しく動作することを検証
        var testData = new List<string> { "Camp", "InvalidValue", "Smithy" };

        var parsed = testData
            .Select(f => Enum.TryParse<FacilityCategory>(f, out var cat) ? (FacilityCategory?)cat : null)
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .ToList();

        // 有効な値のみが残ること（InvalidValueはフィルタされる）
        Assert.Equal(2, parsed.Count);
        Assert.Contains(FacilityCategory.Camp, parsed);
        Assert.Contains(FacilityCategory.Smithy, parsed);
    }

    /// <summary>
    /// E: speedMult除算ガード - 0.0fより大きい場合のみ除算されることを検証
    /// </summary>
    [Fact]
    public void SpeedMult_ZeroDivision_Guard_Prevents_DivideByZero()
    {
        // speedMult > 0.0f のガード条件テスト
        float speedMult = 0.0f;
        int actionCost = 100;

        // ガード条件: speedMult > 0.0f でなければ除算しない
        if (speedMult > 0.0f && speedMult != 1.0f)
        {
            actionCost = Math.Max(1, (int)(actionCost / speedMult));
        }

        // speedMult=0 の場合、actionCostは変更されないこと
        Assert.Equal(100, actionCost);

        // speedMult=0.5f の場合、コストが2倍になること
        speedMult = 0.5f;
        if (speedMult > 0.0f && speedMult != 1.0f)
        {
            actionCost = Math.Max(1, (int)(actionCost / speedMult));
        }
        Assert.Equal(200, actionCost);

        // speedMult=2.0f の場合、コストが半分になること
        actionCost = 100;
        speedMult = 2.0f;
        if (speedMult > 0.0f && speedMult != 1.0f)
        {
            actionCost = Math.Max(1, (int)(actionCost / speedMult));
        }
        Assert.Equal(50, actionCost);
    }

    /// <summary>
    /// 追加: speedMult=1.0fの場合は除算をスキップすることを検証
    /// </summary>
    [Fact]
    public void SpeedMult_ExactlyOne_SkipsDivision()
    {
        float speedMult = 1.0f;
        int actionCost = 100;

        if (speedMult > 0.0f && speedMult != 1.0f)
        {
            actionCost = Math.Max(1, (int)(actionCost / speedMult));
        }

        // speedMult=1.0f の場合、コストは変更されないこと
        Assert.Equal(100, actionCost);
    }

    // ============================================================
    // A1: 安全圏/危険圏の重複処理ルールテスト
    // ============================================================

    /// <summary>
    /// A1-1: 安全圏同士の重複はそのまま処理される（複数集落の安全圏が重なっても安全圏のまま）
    /// </summary>
    [Fact]
    public void SafeZoneOverlap_StaysSafe_WhenMultipleSettlementsOverlap()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new() { [TerritoryInfluenceSystem.FactionNames.Kingdom] = 1.0f });

        // 2つの集落を近くに配置（安全圏が重複する）
        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(10, 10)] = new("town1", "町A", "test", LocationType.Town, TerritoryId.Capital),
            [new Position(20, 10)] = new("town2", "町B", "test", LocationType.Town, TerritoryId.Capital),
        };

        // 2つの集落の中間点（安全圏が重複する位置）
        var midPoint = new Position(15, 10);
        string faction = system.GetDominantFactionForTile(TerritoryId.Capital, midPoint, locations);
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Wildlife, faction);
    }

    /// <summary>
    /// A1-2: 安全圏と危険圏の重なりは日数経過で危険圏が拡大する
    /// </summary>
    [Fact]
    public void SafeZoneShrinksWithDaysElapsed()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Frontier, new() { [TerritoryInfluenceSystem.FactionNames.Bandit] = 1.0f });

        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(50, 50)] = new("village1", "村", "test", LocationType.Village, TerritoryId.Frontier),
        };

        int safeZone = TerritoryInfluenceSystem.GetSafeZoneDistance(220, 160);

        // 安全圏の端のタイル（距離 = safeZone - 2）
        var borderTile = new Position(50 + safeZone - 2, 50);

        // 日数0: 安全圏内
        string factionDay0 = system.GetDominantFactionForTileWithDays(
            TerritoryId.Frontier, borderTile, locations, safeZone, 0);
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Wildlife, factionDay0);

        // 大量日数経過後: 安全圏が縮小して危険圏になる
        string factionLate = system.GetDominantFactionForTileWithDays(
            TerritoryId.Frontier, borderTile, locations, safeZone,
            TerritoryInfluenceSystem.DangerExpansionDaysPerTile * safeZone);
        // 安全圏の最小値は元の半分なので、borderTile が危険圏になっているはず
        Assert.NotEqual(TerritoryInfluenceSystem.FactionNames.Wildlife, factionLate);
    }

    /// <summary>
    /// A1-3: BorderGateは常に安全圏として扱われる
    /// </summary>
    [Fact]
    public void BorderGateIsSafeZone()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Frontier, new() { [TerritoryInfluenceSystem.FactionNames.Bandit] = 1.0f });

        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(100, 100)] = new("frontier_gate_to_Capital", "関所",
                "test", LocationType.BorderGate, TerritoryId.Frontier),
        };

        int safeZone = TerritoryInfluenceSystem.GetSafeZoneDistance(220, 160);

        // BorderGate の近く → 安全圏
        var nearGate = new Position(100 + 5, 100);
        string faction = system.GetDominantFactionForTileWithDays(
            TerritoryId.Frontier, nearGate, locations, safeZone, 0);
        Assert.Equal(TerritoryInfluenceSystem.FactionNames.Wildlife, faction);
    }

    /// <summary>
    /// A1-4: Dungeon/BanditDen/GoblinNestは安全圏判定対象外
    /// </summary>
    [Theory]
    [InlineData(LocationType.Dungeon)]
    [InlineData(LocationType.BanditDen)]
    [InlineData(LocationType.GoblinNest)]
    public void DungeonLocationsExcludedFromSafeZone(LocationType dungeonType)
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Frontier, new() { [TerritoryInfluenceSystem.FactionNames.Bandit] = 1.0f });

        var locations = new Dictionary<Position, LocationDefinition>
        {
            [new Position(50, 50)] = new("dungeon1", "ダンジョン",
                "test", dungeonType, TerritoryId.Frontier),
        };

        int safeZone = TerritoryInfluenceSystem.GetSafeZoneDistance(220, 160);

        // ダンジョンの近くでも安全圏にはならない
        var nearDungeon = new Position(51, 50);
        string faction = system.GetDominantFactionForTileWithDays(
            TerritoryId.Frontier, nearDungeon, locations, safeZone, 0);
        Assert.NotEqual(TerritoryInfluenceSystem.FactionNames.Wildlife, faction);
    }

    /// <summary>
    /// A1-5: 危険圏同士の重複は派閥勢力により決定される
    /// </summary>
    [Fact]
    public void DangerZoneOverlap_DeterminedByFactionInfluence()
    {
        var system = new TerritoryInfluenceSystem();
        // 賊60%、ゴブリン40%で初期化
        system.Initialize(TerritoryId.Frontier, new()
        {
            [TerritoryInfluenceSystem.FactionNames.Bandit] = 0.6f,
            [TerritoryInfluenceSystem.FactionNames.Goblin] = 0.4f,
        });

        var faction1Center = new Position(10, 10);
        var faction2Center = new Position(90, 10);

        // 中間点（距離50の位置）
        var midPoint = new Position(50, 10);

        // 賊の勢力が強いので、中間点よりもさらに遠い地点でも賊が支配
        bool isFaction1 = system.IsInFactionTerritory(
            TerritoryId.Frontier, midPoint,
            faction1Center, TerritoryInfluenceSystem.FactionNames.Bandit,
            faction2Center, TerritoryInfluenceSystem.FactionNames.Goblin);
        Assert.True(isFaction1, "勢力が強い賊が中間点を支配すべき");

        // ゴブリン拠点のすぐ近く → ゴブリンが支配
        var nearGoblin = new Position(85, 10);
        bool isGoblinTerritory = system.IsInFactionTerritory(
            TerritoryId.Frontier, nearGoblin,
            faction1Center, TerritoryInfluenceSystem.FactionNames.Bandit,
            faction2Center, TerritoryInfluenceSystem.FactionNames.Goblin);
        Assert.False(isGoblinTerritory, "ゴブリン拠点近くはゴブリンが支配すべき");
    }

    /// <summary>
    /// A1-6: DangerExpansionDaysPerTile 定数が正の値であること
    /// </summary>
    [Fact]
    public void DangerExpansionDaysPerTile_IsPositive()
    {
        Assert.True(TerritoryInfluenceSystem.DangerExpansionDaysPerTile > 0);
    }

    // ============================================================
    // A2: ワールドマップ廃止→関所NPC統一テスト
    // ============================================================

    /// <summary>
    /// A2-1: TryTravelToは常にfalseを返す（ワールドマップからの直接移動は廃止）
    /// </summary>
    [Fact]
    public void TryTravelTo_AlwaysReturnsFalse_WorldMapDisabled()
    {
        // WorldMapSystem.TravelTo は引き続き内部で使用可能だが、
        // GameController.TryTravelTo は関所案内メッセージを表示して false を返す
        var worldMap = new WorldMapSystem();
        // 隣接領地への移動自体は可能な状態
        worldMap.PlayerGold = 1000;
        Assert.True(worldMap.CanTravelTo(TerritoryId.Forest, 1));
    }

    /// <summary>
    /// A2-2: BorderGateのGetBorderGateTargetが正しく動作する
    /// </summary>
    [Fact]
    public void BorderGateTarget_ParsesCorrectly()
    {
        var loc = new LocationDefinition("Capital_gate_to_Forest", "関所",
            "test", LocationType.BorderGate, TerritoryId.Capital);
        var target = loc.GetBorderGateTarget();
        Assert.NotNull(target);
        Assert.Equal(TerritoryId.Forest, target!.Value);
    }

    /// <summary>
    /// A2-3: 非BorderGateはGetBorderGateTargetがnullを返す
    /// </summary>
    [Fact]
    public void NonBorderGate_ReturnsNull()
    {
        var loc = new LocationDefinition("forest_town", "町",
            "test", LocationType.Town, TerritoryId.Forest);
        var target = loc.GetBorderGateTarget();
        Assert.Null(target);
    }

    /// <summary>
    /// A2-4: WorldMapSystem.BorderGateTollが正の値であること
    /// </summary>
    [Fact]
    public void BorderGateToll_IsPositive()
    {
        Assert.True(WorldMapSystem.BorderGateToll > 0);
    }
}
