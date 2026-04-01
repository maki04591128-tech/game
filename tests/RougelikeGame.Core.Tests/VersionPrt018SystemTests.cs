using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.18 Phase 18 システムテスト
/// Task 1: ダンジョン生成バグ修正（60x30マップで部屋が1つしか生成されない問題）
///   - BSP MinSize/MinRoomSizeの不整合修正
///   - forceMinRooms強化
///   - フォールバック部屋配置追加
/// </summary>
public class VersionPrt018SystemTests
{
    #region Task 1: ダンジョン生成 - 60x30マップでの部屋数保証

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(999)]
    [InlineData(12345)]
    [InlineData(54321)]
    [InlineData(77777)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    public void Generate_60x30Map_AlwaysCreatesAtLeast5Rooms(int seed)
    {
        // Arrange - 実際のゲームと同じ60x30パラメータ
        var generator = new DungeonGenerator(seed);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 1,
            RoomCount = 7,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.True(map.Rooms.Count >= 5,
            $"Seed {seed}: 部屋数が{map.Rooms.Count}で最低5部屋に満たない");
    }

    [Fact]
    public void Generate_60x30Map_HasEntranceAndBossRoom()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 1,
            RoomCount = 7,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.NotNull(map.GetEntranceRoom());
        Assert.NotNull(map.GetBossRoom());
        Assert.Equal(RoomType.Entrance, map.GetEntranceRoom()!.Type);
        Assert.Equal(RoomType.Boss, map.GetBossRoom()!.Type);
    }

    [Fact]
    public void Generate_60x30Map_PlacesStairs()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 1,
            RoomCount = 7,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.NotNull(map.StairsUpPosition);
        Assert.NotNull(map.StairsDownPosition);
    }

    [Fact]
    public void Generate_60x30Map_RoomsDoNotOverlap()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 1,
            RoomCount = 7,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        var rooms = map.Rooms.ToList();
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                var a = rooms[i];
                var b = rooms[j];
                bool overlaps = a.X < b.X + b.Width &&
                                a.X + a.Width > b.X &&
                                a.Y < b.Y + b.Height &&
                                a.Y + a.Height > b.Y;
                Assert.False(overlaps,
                    $"部屋{a.Id}({a.X},{a.Y},{a.Width}x{a.Height})と部屋{b.Id}({b.X},{b.Y},{b.Width}x{b.Height})が重なっている");
            }
        }
    }

    [Fact]
    public void Generate_60x30Map_RoomsWithinMapBounds()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 1,
            RoomCount = 7,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        foreach (var room in map.Rooms)
        {
            Assert.True(room.X >= 0, $"部屋{room.Id}のX座標が負");
            Assert.True(room.Y >= 0, $"部屋{room.Id}のY座標が負");
            Assert.True(room.X + room.Width <= parameters.Width,
                $"部屋{room.Id}がマップ右端を超過");
            Assert.True(room.Y + room.Height <= parameters.Height,
                $"部屋{room.Id}がマップ下端を超過");
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(54321)]
    public void Generate_60x30Map_DeeperFloors_AlsoCreatesSufficientRooms(int seed)
    {
        // Arrange - 深い階層のパラメータ（RoomCountが増加）
        var generator = new DungeonGenerator(seed);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 30,
            Depth = 5,
            RoomCount = 11, // 6 + CurrentFloor(5)
            EnemyDensity = 0.02f,
            ItemDensity = 0.015f,
            TrapDensity = 0.01f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.True(map.Rooms.Count >= 5,
            $"Seed {seed}, Depth 5: 部屋数が{map.Rooms.Count}で最低5部屋に満たない");
    }

    [Fact]
    public void Generate_50x40Map_StillCreatesAtLeast5Rooms()
    {
        // Arrange - 従来テストのサイズでもリグレッションがないことを確認
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 50,
            Height = 40,
            Depth = 1,
            RoomCount = 8,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.True(map.Rooms.Count >= 5,
            $"部屋数が{map.Rooms.Count}で最低5部屋に満たない");
    }

    #endregion
}
