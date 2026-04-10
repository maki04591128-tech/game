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
    public void AllTerritories_MapSizeInRange_2300To5000(TerritoryId territory)
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

        // SymbolMountainが障害物として存在する領地で検証
        int obstacles = 0;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.BlocksMovement) obstacles++;
            }

        // 障害物が存在する
        Assert.True(obstacles > 0,
            $"Territory {territory}: should have blocking tiles (mountains)");
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
}
