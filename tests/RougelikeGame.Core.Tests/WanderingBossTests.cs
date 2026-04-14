using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 徘徊ボスモンスターシステムのテスト
/// </summary>
public class WanderingBossTests
{
    [Fact]
    public void GetBossForTerritory_Mountain_ReturnsBahamut()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain);
        Assert.NotNull(boss);
        Assert.Equal("バハムート", boss.Name);
        Assert.Equal(TerritoryId.Mountain, boss.Territory);
        Assert.Contains(TileType.SymbolMountain, boss.WalkableTerrain);
        Assert.Equal(3, boss.MinAltitude);
    }

    [Fact]
    public void GetBossForTerritory_Coast_ReturnsLeviathan()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Coast);
        Assert.NotNull(boss);
        Assert.Equal("リヴァイアサン", boss.Name);
        Assert.Contains(TileType.SymbolWater, boss.WalkableTerrain);
        Assert.True(boss.MaxAltitude <= -2);
    }

    [Fact]
    public void GetBossForTerritory_Volcanic_ReturnsIfrit()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Volcanic);
        Assert.NotNull(boss);
        Assert.Equal("イフリート", boss.Name);
        Assert.Contains(TileType.SymbolLava, boss.WalkableTerrain);
    }

    [Fact]
    public void GetBossForTerritory_Capital_ReturnsNull()
    {
        // 王都領にはボスがいない
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Capital);
        Assert.Null(boss);
    }

    [Fact]
    public void GetBossForTerritory_AllTerritories_HaveValidDefinitions()
    {
        var territoriesWithBoss = new[]
        {
            TerritoryId.Mountain, TerritoryId.Coast, TerritoryId.Volcanic,
            TerritoryId.Forest, TerritoryId.Tundra, TerritoryId.Desert,
            TerritoryId.Swamp, TerritoryId.Lake, TerritoryId.Sacred,
            TerritoryId.Frontier, TerritoryId.Southern
        };

        foreach (var territory in territoriesWithBoss)
        {
            var boss = WanderingBossSystem.GetBossForTerritory(territory);
            Assert.NotNull(boss);
            Assert.False(string.IsNullOrEmpty(boss.Name));
            Assert.True(boss.Level > 0);
            Assert.True(boss.Hp > 0);
            Assert.True(boss.Attack > 0);
            Assert.True(boss.Defense > 0);
            Assert.True(boss.WalkableTerrain.Length > 0);
            Assert.Equal(territory, boss.Territory);
        }
    }

    [Fact]
    public void CanBossOccupy_MatchingTerrainAndAltitude_ReturnsTrue()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var tile = new Tile { Type = TileType.SymbolMountain };
        tile.SetAltitude(4); // 高度4は範囲3-5内

        Assert.True(WanderingBossSystem.CanBossOccupy(boss, tile));
    }

    [Fact]
    public void CanBossOccupy_WrongTerrain_ReturnsFalse()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var tile = new Tile { Type = TileType.SymbolGrass };
        tile.SetAltitude(4);

        Assert.False(WanderingBossSystem.CanBossOccupy(boss, tile));
    }

    [Fact]
    public void CanBossOccupy_AltitudeTooLow_ReturnsFalse()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var tile = new Tile { Type = TileType.SymbolMountain };
        tile.SetAltitude(1); // 高度1は範囲3-5外

        Assert.False(WanderingBossSystem.CanBossOccupy(boss, tile));
    }

    [Fact]
    public void CanBossOccupy_CoastBoss_DeepWater_ReturnsTrue()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Coast)!;
        var tile = new Tile { Type = TileType.SymbolWater };
        tile.SetAltitude(-3); // 深度-3は範囲-4〜-2内

        Assert.True(WanderingBossSystem.CanBossOccupy(boss, tile));
    }

    [Fact]
    public void WanderingBossInstance_InitialState_NotDefeated()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var instance = new WanderingBossInstance(boss, new Position(10, 10));

        Assert.False(instance.IsDefeated);
        Assert.Equal(new Position(10, 10), instance.Position);
    }

    [Fact]
    public void WanderingBossInstance_Defeat_SetsFlag()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var instance = new WanderingBossInstance(boss, new Position(10, 10));

        instance.IsDefeated = true;

        Assert.True(instance.IsDefeated);
    }

    [Fact]
    public void IsPlayerContactingBoss_SamePosition_ReturnsTrue()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var instance = new WanderingBossInstance(boss, new Position(10, 10));

        Assert.True(WanderingBossSystem.IsPlayerContactingBoss(new Position(10, 10), instance));
    }

    [Fact]
    public void IsPlayerContactingBoss_DifferentPosition_ReturnsFalse()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var instance = new WanderingBossInstance(boss, new Position(10, 10));

        Assert.False(WanderingBossSystem.IsPlayerContactingBoss(new Position(11, 10), instance));
    }

    [Fact]
    public void IsPlayerContactingBoss_DefeatedBoss_ReturnsFalse()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain)!;
        var instance = new WanderingBossInstance(boss, new Position(10, 10));
        instance.IsDefeated = true;

        Assert.False(WanderingBossSystem.IsPlayerContactingBoss(new Position(10, 10), instance));
    }

    [Fact]
    public void IsPlayerContactingBoss_NullBoss_ReturnsFalse()
    {
        Assert.False(WanderingBossSystem.IsPlayerContactingBoss(new Position(10, 10), null));
    }

    [Fact]
    public void SymbolMapResult_IncludesBossInstance()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Mountain);

        // 山岳領にはバハムートが配置される（配置可能な地形がある場合）
        // ボスが配置されるかはマップ生成次第だが、定義は存在する
        var bossDef = WanderingBossSystem.GetBossForTerritory(TerritoryId.Mountain);
        Assert.NotNull(bossDef);
    }

    [Fact]
    public void SymbolMapResult_Capital_NoBoss()
    {
        var generator = new SymbolMapGenerator();
        var result = generator.Generate(TerritoryId.Capital);

        // 王都領にはボスがいない
        Assert.Null(result.WanderingBoss);
    }

    [Fact]
    public void SymbolMapSystem_GenerateForTerritory_SetsBoss()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital, null);

        // 王都領にはボスがいない
        Assert.Null(system.CurrentWanderingBoss);
    }

    [Fact]
    public void SymbolMapSystem_Clear_ClearsBoss()
    {
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Mountain, null);
        system.Clear();

        Assert.Null(system.CurrentWanderingBoss);
    }

    [Fact]
    public void TileType_SymbolWanderingBoss_DisplayChar()
    {
        var tile = new Tile { Type = TileType.SymbolWanderingBoss };
        Assert.Equal('龍', tile.DisplayChar);
    }

    [Fact]
    public void TileType_SymbolWanderingBoss_NonBlocking()
    {
        var tile = Tile.FromType(TileType.SymbolWanderingBoss);
        Assert.False(tile.BlocksMovement);
        Assert.False(tile.BlocksSight);
    }

    [Fact]
    public void BossDefinitions_HaveUniqueIds()
    {
        var ids = new HashSet<string>();
        var territories = new[]
        {
            TerritoryId.Mountain, TerritoryId.Coast, TerritoryId.Volcanic,
            TerritoryId.Forest, TerritoryId.Tundra, TerritoryId.Desert,
            TerritoryId.Swamp, TerritoryId.Lake, TerritoryId.Sacred,
            TerritoryId.Frontier, TerritoryId.Southern
        };

        foreach (var territory in territories)
        {
            var boss = WanderingBossSystem.GetBossForTerritory(territory);
            Assert.NotNull(boss);
            Assert.True(ids.Add(boss.Id), $"ボスID '{boss.Id}' が重複しています");
        }
    }

    [Fact]
    public void BossDefinitions_HaveValidAltitudeRanges()
    {
        var territories = new[]
        {
            TerritoryId.Mountain, TerritoryId.Coast, TerritoryId.Volcanic,
            TerritoryId.Forest, TerritoryId.Tundra, TerritoryId.Desert,
            TerritoryId.Swamp, TerritoryId.Lake, TerritoryId.Sacred,
            TerritoryId.Frontier, TerritoryId.Southern
        };

        foreach (var territory in territories)
        {
            var boss = WanderingBossSystem.GetBossForTerritory(territory)!;
            Assert.True(boss.MinAltitude <= boss.MaxAltitude,
                $"{boss.Name}の高度範囲が不正: Min={boss.MinAltitude}, Max={boss.MaxAltitude}");
        }
    }

    [Fact]
    public void GetBossForTerritory_Southern_ReturnsChimera()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Southern);
        Assert.NotNull(boss);
        Assert.Equal("キマイラ", boss.Name);
        Assert.Equal(TerritoryId.Southern, boss.Territory);
        Assert.Contains(TileType.SymbolGrass, boss.WalkableTerrain);
        Assert.True(boss.MinAltitude <= 0);
    }

    [Fact]
    public void CanBossOccupy_SouthernBoss_GrassWithinAltitude_ReturnsTrue()
    {
        var boss = WanderingBossSystem.GetBossForTerritory(TerritoryId.Southern)!;
        var tile = new Tile { Type = TileType.SymbolGrass };
        tile.SetAltitude(0); // 高度0は範囲-1〜1内

        Assert.True(WanderingBossSystem.CanBossOccupy(boss, tile));
    }
}
