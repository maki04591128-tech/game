using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Core.Tests;

public class TileTests
{
    [Fact]
    public void FromType_Wall_BlocksMovementAndSight()
    {
        // Act
        var tile = Tile.FromType(TileType.Wall);

        // Assert
        Assert.True(tile.BlocksMovement);
        Assert.True(tile.BlocksSight);
        Assert.Equal('#', tile.DisplayChar);
    }

    [Fact]
    public void FromType_Floor_DoesNotBlock()
    {
        // Act
        var tile = Tile.FromType(TileType.Floor);

        // Assert
        Assert.False(tile.BlocksMovement);
        Assert.False(tile.BlocksSight);
        Assert.Equal('.', tile.DisplayChar);
    }

    [Fact]
    public void FromType_Water_HasMovementCost()
    {
        // Act
        var tile = Tile.FromType(TileType.Water);

        // Assert
        Assert.False(tile.BlocksMovement);
        Assert.Equal(2.0f, tile.MovementCost);
    }

    [Fact]
    public void FromType_DoorClosed_BlocksBoth()
    {
        // Act
        var tile = Tile.FromType(TileType.DoorClosed);

        // Assert
        Assert.True(tile.BlocksMovement);
        Assert.True(tile.BlocksSight);
        Assert.Equal('+', tile.DisplayChar);
    }

    [Fact]
    public void FromType_DoorOpen_BlocksNeither()
    {
        // Act
        var tile = Tile.FromType(TileType.DoorOpen);

        // Assert
        Assert.False(tile.BlocksMovement);
        Assert.False(tile.BlocksSight);
        Assert.Equal('/', tile.DisplayChar);
    }
}

public class RoomTests
{
    [Fact]
    public void Center_ReturnsCorrectPosition()
    {
        // Arrange
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 10, Height = 8 };

        // Act
        var center = room.Center;

        // Assert
        Assert.Equal(15, center.X);
        Assert.Equal(14, center.Y);
    }

    [Fact]
    public void Contains_PositionInside_ReturnsTrue()
    {
        // Arrange
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 10, Height = 10 };

        // Assert
        Assert.True(room.Contains(new Position(15, 15)));
        Assert.True(room.Contains(new Position(10, 10)));
        Assert.True(room.Contains(new Position(19, 19)));
    }

    [Fact]
    public void Contains_PositionOutside_ReturnsFalse()
    {
        // Arrange
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 10, Height = 10 };

        // Assert
        Assert.False(room.Contains(new Position(5, 5)));
        Assert.False(room.Contains(new Position(20, 20)));
    }

    [Fact]
    public void Intersects_OverlappingRooms_ReturnsTrue()
    {
        // Arrange
        var room1 = new Room { Id = 0, X = 10, Y = 10, Width = 10, Height = 10 };
        var room2 = new Room { Id = 1, X = 15, Y = 15, Width = 10, Height = 10 };

        // Assert
        Assert.True(room1.Intersects(room2));
    }

    [Fact]
    public void Intersects_SeparateRooms_ReturnsFalse()
    {
        // Arrange
        var room1 = new Room { Id = 0, X = 10, Y = 10, Width = 10, Height = 10 };
        var room2 = new Room { Id = 1, X = 30, Y = 30, Width = 10, Height = 10 };

        // Assert
        Assert.False(room1.Intersects(room2));
    }

    [Fact]
    public void Area_ReturnsCorrectValue()
    {
        // Arrange
        var room = new Room { Id = 0, X = 0, Y = 0, Width = 10, Height = 5 };

        // Assert
        Assert.Equal(50, room.Area);
    }
}

public class DungeonMapTests
{
    [Fact]
    public void Constructor_InitializesAllTilesToWall()
    {
        // Arrange & Act
        var map = new DungeonMap(20, 20);

        // Assert
        Assert.Equal(TileType.Wall, map[5, 5].Type);
        Assert.Equal(TileType.Wall, map[0, 0].Type);
        Assert.Equal(TileType.Wall, map[19, 19].Type);
    }

    [Fact]
    public void SetTile_ChangesType()
    {
        // Arrange
        var map = new DungeonMap(20, 20);

        // Act
        map.SetTile(5, 5, TileType.Floor);

        // Assert
        Assert.Equal(TileType.Floor, map[5, 5].Type);
    }

    [Fact]
    public void IsWalkable_Floor_ReturnsTrue()
    {
        // Arrange
        var map = new DungeonMap(20, 20);
        map.SetTile(5, 5, TileType.Floor);

        // Assert
        Assert.True(map.IsWalkable(new Position(5, 5)));
    }

    [Fact]
    public void IsWalkable_Wall_ReturnsFalse()
    {
        // Arrange
        var map = new DungeonMap(20, 20);

        // Assert
        Assert.False(map.IsWalkable(new Position(5, 5)));
    }

    [Fact]
    public void IsWalkable_OutOfBounds_ReturnsFalse()
    {
        // Arrange
        var map = new DungeonMap(20, 20);

        // Assert
        Assert.False(map.IsWalkable(new Position(-1, 0)));
        Assert.False(map.IsWalkable(new Position(20, 0)));
    }

    [Fact]
    public void HasLineOfSight_ClearPath_ReturnsTrue()
    {
        // Arrange
        var map = new DungeonMap(20, 20);
        for (int x = 5; x <= 15; x++)
        {
            map.SetTile(x, 10, TileType.Floor);
        }

        // Assert
        Assert.True(map.HasLineOfSight(new Position(5, 10), new Position(15, 10)));
    }

    [Fact]
    public void HasLineOfSight_BlockedPath_ReturnsFalse()
    {
        // Arrange
        var map = new DungeonMap(20, 20);
        for (int x = 5; x <= 15; x++)
        {
            map.SetTile(x, 10, TileType.Floor);
        }
        map.SetTile(10, 10, TileType.Wall);  // 中央に壁

        // Assert
        Assert.False(map.HasLineOfSight(new Position(5, 10), new Position(15, 10)));
    }

    [Fact]
    public void SetStairs_SetsCorrectPositions()
    {
        // Arrange
        var map = new DungeonMap(20, 20);

        // Act
        map.SetStairsUp(new Position(5, 5));
        map.SetStairsDown(new Position(15, 15));

        // Assert
        Assert.Equal(new Position(5, 5), map.StairsUpPosition);
        Assert.Equal(new Position(15, 15), map.StairsDownPosition);
        Assert.Equal(TileType.StairsUp, map[5, 5].Type);
        Assert.Equal(TileType.StairsDown, map[15, 15].Type);
    }

    [Fact]
    public void CountFloorTiles_ReturnsCorrectCount()
    {
        // Arrange
        var map = new DungeonMap(20, 20);
        map.SetTile(5, 5, TileType.Floor);
        map.SetTile(6, 5, TileType.Floor);
        map.SetTile(7, 5, TileType.Corridor);

        // Assert
        Assert.Equal(3, map.CountFloorTiles());
    }
}

public class DungeonGeneratorTests
{
    [Fact]
    public void Generate_CreatesMapWithCorrectDimensions()
    {
        // Arrange
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
        Assert.Equal(50, map.Width);
        Assert.Equal(40, map.Height);
    }

    [Fact]
    public void Generate_CreatesRooms()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
            Depth = 1,
            RoomCount = 10,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map = generator.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map);
        Assert.True(map.Rooms.Count >= 3);  // 最低3部屋は生成される
    }

    [Fact]
    public void Generate_PlacesStairs()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
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
        Assert.NotNull(map.StairsUpPosition);
        Assert.NotNull(map.StairsDownPosition);
    }

    [Fact]
    public void Generate_HasEntranceAndBossRoom()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
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
        Assert.NotNull(map.GetEntranceRoom());
        Assert.NotNull(map.GetBossRoom());
        Assert.Equal(RoomType.Entrance, map.GetEntranceRoom()!.Type);
        Assert.Equal(RoomType.Boss, map.GetBossRoom()!.Type);
    }

    [Fact]
    public void Generate_ProducesWalkableFloor()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
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
        Assert.True(map.CountFloorTiles() > 100);  // 十分な床タイルがある
    }

    [Fact]
    public void Generate_WithSeed_ProducesSameResult()
    {
        // Arrange
        var generator1 = new DungeonGenerator(12345);
        var generator2 = new DungeonGenerator(12345);
        var parameters = new DungeonGenerationParameters
        {
            Width = 50,
            Height = 40,
            Depth = 1,
            RoomCount = 6,
            EnemyDensity = 0.01f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };

        // Act
        var map1 = generator1.Generate(parameters) as DungeonMap;
        var map2 = generator2.Generate(parameters) as DungeonMap;

        // Assert
        Assert.NotNull(map1);
        Assert.NotNull(map2);
        Assert.Equal(map1.Rooms.Count, map2.Rooms.Count);
        Assert.Equal(map1.StairsUpPosition, map2.StairsUpPosition);
        Assert.Equal(map1.StairsDownPosition, map2.StairsDownPosition);
    }
}

public class FeaturePlacerTests
{
    [Fact]
    public void PlaceEnemies_ReturnsSpawnPoints()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
            Depth = 1,
            RoomCount = 8,
            EnemyDensity = 0.02f,
            ItemDensity = 0.01f,
            TrapDensity = 0.005f
        };
        var map = generator.Generate(parameters) as DungeonMap;
        var placer = new FeaturePlacer(new Random(42));

        // Act
        var spawnPoints = placer.PlaceEnemies(map!, 0.02f, 1);

        // Assert
        Assert.NotEmpty(spawnPoints);
        Assert.All(spawnPoints, sp => Assert.True(map!.IsWalkable(sp.Position)));
    }

    [Fact]
    public void PlaceItems_ReturnsSpawnPoints()
    {
        // Arrange
        var generator = new DungeonGenerator(42);
        var parameters = new DungeonGenerationParameters
        {
            Width = 60,
            Height = 50,
            Depth = 1,
            RoomCount = 8,
            EnemyDensity = 0.02f,
            ItemDensity = 0.02f,
            TrapDensity = 0.005f
        };
        var map = generator.Generate(parameters) as DungeonMap;
        var placer = new FeaturePlacer(new Random(42));

        // Act
        var spawnPoints = placer.PlaceItems(map!, 0.02f);

        // Assert
        Assert.NotEmpty(spawnPoints);
    }
}

public class BSPNodeTests
{
    [Fact]
    public void IsLeaf_NoChildren_ReturnsTrue()
    {
        // Arrange
        var node = new BSPNode(0, 0, 50, 50);

        // Assert
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void IsLeaf_WithChildren_ReturnsFalse()
    {
        // Arrange
        var node = new BSPNode(0, 0, 50, 50);
        node.Left = new BSPNode(0, 0, 25, 50);
        node.Right = new BSPNode(25, 0, 25, 50);

        // Assert
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void GetLeaves_ReturnsAllLeafNodes()
    {
        // Arrange
        var root = new BSPNode(0, 0, 100, 100);
        root.Left = new BSPNode(0, 0, 50, 100);
        root.Right = new BSPNode(50, 0, 50, 100);
        root.Left.Left = new BSPNode(0, 0, 50, 50);
        root.Left.Right = new BSPNode(0, 50, 50, 50);

        // Act
        var leaves = root.GetLeaves();

        // Assert
        Assert.Equal(3, leaves.Count);  // 2 from Left subtree + 1 from Right
    }
}
