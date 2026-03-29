using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// シンボルマップシステム 専用テスト
/// テスト数: 16件
/// </summary>
public class SymbolMapSystemCoreTests
{
    // ============================================================
    // 初期状態テスト
    // ============================================================

    [Fact]
    public void InitialState_CurrentMap_IsNull()
    {
        // 初期状態でCurrentMapがnull
        var system = new SymbolMapSystem();
        Assert.Null(system.CurrentMap);
    }

    [Fact]
    public void InitialState_CurrentTerritory_IsNull()
    {
        // 初期状態でCurrentTerritoryがnull
        var system = new SymbolMapSystem();
        Assert.Null(system.CurrentTerritory);
    }

    [Fact]
    public void InitialState_LocationCount_IsZero()
    {
        // 初期状態でLocationCountが0
        var system = new SymbolMapSystem();
        Assert.Equal(0, system.LocationCount);
    }

    // ============================================================
    // GenerateForTerritory テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void GenerateForTerritory_AllTerritories_CreatesMap(TerritoryId territory)
    {
        // 全領地でマップが生成される
        var system = new SymbolMapSystem();
        var map = system.GenerateForTerritory(territory);
        Assert.NotNull(map);
        Assert.Equal(territory, system.CurrentTerritory);
        Assert.True(system.LocationCount > 0);
    }

    [Fact]
    public void GenerateForTerritory_ReplacesExistingMap()
    {
        // 再生成すると既存マップが置き換わる
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var oldCount = system.LocationCount;

        system.GenerateForTerritory(TerritoryId.Forest);
        Assert.Equal(TerritoryId.Forest, system.CurrentTerritory);
        Assert.True(system.LocationCount > 0);
    }

    // ============================================================
    // GetLocationAt / IsLocationSymbol テスト
    // ============================================================

    [Fact]
    public void GetLocationAt_ValidPosition_ReturnsLocation()
    {
        // 有効なロケーション位置で情報を取得できる
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var positions = system.GetAllLocationPositions();
        var firstPos = positions.Keys.First();

        var location = system.GetLocationAt(firstPos);
        Assert.NotNull(location);
        Assert.False(string.IsNullOrEmpty(location!.Name));
    }

    [Fact]
    public void GetLocationAt_InvalidPosition_ReturnsNull()
    {
        // 無効な位置ではnullを返す
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.Null(system.GetLocationAt(new Position(-999, -999)));
    }

    [Fact]
    public void IsLocationSymbol_InvalidPosition_ReturnsFalse()
    {
        // 無効な位置ではfalseを返す
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.False(system.IsLocationSymbol(new Position(-999, -999)));
    }

    // ============================================================
    // ロケーション種別判定テスト
    // ============================================================

    [Fact]
    public void IsDungeonEntrance_InvalidPosition_ReturnsFalse()
    {
        // 無効な位置ではダンジョン入口でない
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.False(system.IsDungeonEntrance(new Position(-999, -999)));
    }

    [Fact]
    public void IsTownEntrance_InvalidPosition_ReturnsFalse()
    {
        // 無効な位置では街の入口でない
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.False(system.IsTownEntrance(new Position(-999, -999)));
    }

    [Fact]
    public void IsFacility_InvalidPosition_ReturnsFalse()
    {
        // 無効な位置では施設でない
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.False(system.IsFacility(new Position(-999, -999)));
    }

    [Fact]
    public void IsShrine_InvalidPosition_ReturnsFalse()
    {
        // 無効な位置では宗教施設でない
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.False(system.IsShrine(new Position(-999, -999)));
    }

    // ============================================================
    // FindLocationPosition テスト
    // ============================================================

    [Fact]
    public void FindLocationPosition_ExistingLocation_ReturnsPosition()
    {
        // 存在するロケーションIDで位置を取得できる
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        var positions = system.GetAllLocationPositions();
        var firstEntry = positions.First();

        var foundPos = system.FindLocationPosition(firstEntry.Value.Id);
        Assert.NotNull(foundPos);
        Assert.Equal(firstEntry.Key, foundPos);
    }

    [Fact]
    public void FindLocationPosition_NonExistent_ReturnsNull()
    {
        // 存在しないロケーションIDではnullを返す
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Capital);
        Assert.Null(system.FindLocationPosition("nonexistent_id_12345"));
    }

    // ============================================================
    // Clear テスト
    // ============================================================

    [Fact]
    public void Clear_ResetsAllState()
    {
        // Clear後に全状態がリセットされる
        var system = new SymbolMapSystem();
        system.GenerateForTerritory(TerritoryId.Forest);
        Assert.NotNull(system.CurrentMap);

        system.Clear();
        Assert.Null(system.CurrentMap);
        Assert.Null(system.CurrentTerritory);
        Assert.Equal(0, system.LocationCount);
        Assert.Empty(system.GetAllLocationPositions());
    }
}
