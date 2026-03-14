using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Gui.Tests;

/// <summary>
/// ダンジョン生成で斜め移動限定タイルが存在しないことを検証するテスト
/// </summary>
public class DiagonalOnlyTileTests
{
    private DungeonGenerationParameters CreateDefaultParameters(int seed = 42)
    {
        return new DungeonGenerationParameters
        {
            Width = 60,
            Height = 40,
            Depth = 1,
            RoomCount = 8,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };
    }

    /// <summary>
    /// マップ上に斜めでしか到達できないタイルがないことを検証するヘルパー
    /// </summary>
    private void AssertNoDiagonalOnlyTiles(DungeonMap map)
    {
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                if (!map.IsWalkable(pos)) continue;

                // 4方向の歩行可能タイルを確認
                var cardinals = new[]
                {
                    new Position(x, y - 1),
                    new Position(x, y + 1),
                    new Position(x - 1, y),
                    new Position(x + 1, y)
                };

                int walkableCardinals = cardinals.Count(p => map.IsInBounds(p) && map.IsWalkable(p));

                // 歩行可能タイルの4方向隣接に最低1つは歩行可能タイルがあるべき
                Assert.True(walkableCardinals > 0,
                    $"({x},{y})は4方向に歩行可能なタイルがなく、斜め移動でしか到達できません");
            }
        }
    }

    /// <summary>
    /// 斜め接続のみ（隣接2タイルが両方壁）の箇所がないことを検証するヘルパー
    /// </summary>
    private void AssertNoDiagonalOnlyConnections(DungeonMap map)
    {
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                if (!map.IsWalkable(pos)) continue;

                // 各斜め方向をチェック
                var diagonals = new[]
                {
                    (new Position(x - 1, y - 1), new Position(x - 1, y), new Position(x, y - 1)),
                    (new Position(x + 1, y - 1), new Position(x + 1, y), new Position(x, y - 1)),
                    (new Position(x - 1, y + 1), new Position(x - 1, y), new Position(x, y + 1)),
                    (new Position(x + 1, y + 1), new Position(x + 1, y), new Position(x, y + 1))
                };

                foreach (var (diagPos, adj1, adj2) in diagonals)
                {
                    if (!map.IsInBounds(diagPos) || !map.IsWalkable(diagPos)) continue;

                    bool adj1Walkable = map.IsInBounds(adj1) && map.IsWalkable(adj1);
                    bool adj2Walkable = map.IsInBounds(adj2) && map.IsWalkable(adj2);

                    // 斜めに床があるなら、間の2タイルのうち少なくとも1つは歩行可能であるべき
                    Assert.True(adj1Walkable || adj2Walkable,
                        $"({x},{y})と({diagPos.X},{diagPos.Y})の間に斜め移動限定の接続があります");
                }
            }
        }
    }

    [Fact]
    public void Generate_NoDiagonalOnlyIsolatedTiles()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = CreateDefaultParameters();

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyTiles(map);
    }

    [Fact]
    public void Generate_NoDiagonalOnlyConnections()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = CreateDefaultParameters();

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyConnections(map);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(123)]
    [InlineData(999)]
    [InlineData(12345)]
    public void Generate_MultipleSeeds_NoDiagonalOnlyTiles(int seed)
    {
        // Arrange
        var generator = new DungeonGenerator(seed);
        var parameters = CreateDefaultParameters();

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyTiles(map);
        AssertNoDiagonalOnlyConnections(map);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Generate_DifferentFloors_NoDiagonalOnlyTiles(int floor)
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 40,
            Depth = floor,
            RoomCount = 6 + floor,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f * floor
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyTiles(map);
        AssertNoDiagonalOnlyConnections(map);
    }

    [Fact]
    public void Generate_LargeMap_NoDiagonalOnlyTiles()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 100,
            Height = 80,
            Depth = 1,
            RoomCount = 15,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.01f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyTiles(map);
        AssertNoDiagonalOnlyConnections(map);
    }

    [Fact]
    public void Generate_SmallMap_NoDiagonalOnlyTiles()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 30,
            Height = 20,
            Depth = 1,
            RoomCount = 4,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        AssertNoDiagonalOnlyTiles(map);
        AssertNoDiagonalOnlyConnections(map);
    }

    [Fact]
    public void Generate_StillHasSufficientWalkableTiles()
    {
        // 斜め修正後もマップが十分なサイズを持っていること
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = CreateDefaultParameters();

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.True(map.CountFloorTiles() > 100, "修正後も十分な歩行可能タイルがあること");
    }
}
