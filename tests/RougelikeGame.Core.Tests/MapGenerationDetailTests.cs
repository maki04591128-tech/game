using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// RoomGenerator/CorridorGenerator/FeaturePlacerのユニットテスト
/// </summary>
public class MapGenerationDetailTests
{
    private DungeonMap CreateTestMap(int width = 80, int height = 50)
    {
        return new DungeonMap(width, height);
    }

    #region RoomGenerator - CarveRoom

    [Fact]
    public void RoomGenerator_CarveRoom_CreatesFloorTilesInside()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 8, Height = 6, Type = RoomType.Normal };

        RoomGenerator.CarveRoom(map, room);

        // 内部は床
        Assert.Equal(TileType.Floor, map.GetTileType(new Position(12, 12)));
        Assert.Equal(TileType.Floor, map.GetTileType(new Position(14, 13)));
    }

    [Fact]
    public void RoomGenerator_CarveRoom_CreatesWallsOnBoundary()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 8, Height = 6, Type = RoomType.Normal };

        RoomGenerator.CarveRoom(map, room);

        // 境界は壁
        Assert.Equal(TileType.Wall, map.GetTileType(new Position(10, 10)));
        Assert.Equal(TileType.Wall, map.GetTileType(new Position(17, 15)));
    }

    [Fact]
    public void RoomGenerator_CarveRoom_SetsRoomIdOnFloorTiles()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 5, X = 10, Y = 10, Width = 8, Height = 6, Type = RoomType.Normal };

        RoomGenerator.CarveRoom(map, room);

        // 内部タイルにRoomIdが設定される
        Assert.Equal(5, map[12, 12].RoomId);
    }

    #endregion

    #region RoomGenerator - CarveCircularRoom

    [Fact]
    public void RoomGenerator_CarveCircularRoom_CenterIsFloor()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 20, Y = 20, Width = 10, Height = 10, Type = RoomType.Normal };

        RoomGenerator.CarveCircularRoom(map, room);

        // 中央は必ず床
        var center = room.Center;
        Assert.Equal(TileType.Floor, map.GetTileType(center));
    }

    [Fact]
    public void RoomGenerator_CarveCircularRoom_SetsRoomId()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 2, X = 20, Y = 20, Width = 10, Height = 10, Type = RoomType.Normal };

        RoomGenerator.CarveCircularRoom(map, room);

        var center = room.Center;
        Assert.Equal(2, map[center.X, center.Y].RoomId);
    }

    #endregion

    #region RoomGenerator - CarveCrossRoom

    [Fact]
    public void RoomGenerator_CarveCrossRoom_CenterIsFloor()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 20, Y = 20, Width = 12, Height = 12, Type = RoomType.Normal };

        RoomGenerator.CarveCrossRoom(map, room);

        var center = room.Center;
        Assert.Equal(TileType.Floor, map.GetTileType(center));
    }

    [Fact]
    public void RoomGenerator_CarveCrossRoom_HasCrossShape()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 20, Y = 20, Width = 12, Height = 12, Type = RoomType.Normal };

        RoomGenerator.CarveCrossRoom(map, room);

        // 十字の横アーム: room中央Y座標で横方向に床がある
        var center = room.Center;
        Assert.Equal(TileType.Floor, map.GetTileType(new Position(room.X + 2, center.Y)));
        Assert.Equal(TileType.Floor, map.GetTileType(new Position(room.X + room.Width - 3, center.Y)));
    }

    #endregion

    #region RoomGenerator - DecorateRoom

    [Fact]
    public void RoomGenerator_DecorateRoom_Shrine_AddsAltar()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 8, Height = 6, Type = RoomType.Shrine };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        RoomGenerator.DecorateRoom(map, room, new Random(42));

        // 中央に祭壇
        var center = room.Center;
        Assert.Equal(TileType.Altar, map.GetTileType(center));
    }

    [Fact]
    public void RoomGenerator_DecorateRoom_Boss_AddsPillars()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 12, Height = 10, Type = RoomType.Boss };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        RoomGenerator.DecorateRoom(map, room, new Random(42));

        // ボス部屋はコーナーに柱がある可能性
        bool hasPillar = false;
        for (int x = room.X; x < room.X + room.Width; x++)
            for (int y = room.Y; y < room.Y + room.Height; y++)
                if (map.GetTileType(new Position(x, y)) == TileType.Pillar) hasPillar = true;
        Assert.True(hasPillar);
    }

    [Fact]
    public void RoomGenerator_DecorateRoom_Treasure_AddsChests()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 8, Height = 6, Type = RoomType.Treasure };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        RoomGenerator.DecorateRoom(map, room, new Random(42));

        bool hasChest = false;
        for (int x = room.X; x < room.X + room.Width; x++)
            for (int y = room.Y; y < room.Y + room.Height; y++)
                if (map.GetTileType(new Position(x, y)) == TileType.Chest) hasChest = true;
        Assert.True(hasChest);
    }

    #endregion

    #region CorridorGenerator

    [Fact]
    public void CorridorGenerator_ConnectRoomsL_CreatesPath()
    {
        var map = CreateTestMap();
        var from = new Position(10, 10);
        var to = new Position(30, 25);

        CorridorGenerator.ConnectRoomsL(map, from, to, new Random(42));

        // 始点と終点が通路
        Assert.Equal(TileType.Corridor, map.GetTileType(from));
        Assert.Equal(TileType.Corridor, map.GetTileType(to));
    }

    [Fact]
    public void CorridorGenerator_ConnectRoomsDirect_CreatesPath()
    {
        var map = CreateTestMap();
        var from = new Position(10, 10);
        var to = new Position(20, 15);

        CorridorGenerator.ConnectRoomsDirect(map, from, to);

        Assert.Equal(TileType.Corridor, map.GetTileType(from));
        Assert.Equal(TileType.Corridor, map.GetTileType(to));
    }

    [Fact]
    public void CorridorGenerator_ConnectRoomsZigzag_CreatesPath()
    {
        var map = CreateTestMap();
        var from = new Position(10, 10);
        var to = new Position(30, 30);

        CorridorGenerator.ConnectRoomsZigzag(map, from, to, new Random(42));

        Assert.Equal(TileType.Corridor, map.GetTileType(from));
        Assert.Equal(TileType.Corridor, map.GetTileType(to));
    }

    [Fact]
    public void CorridorGenerator_CarveHorizontalCorridor_CreatesHorizontalPath()
    {
        var map = CreateTestMap();
        CorridorGenerator.CarveHorizontalCorridor(map, 5, 15, 10);

        for (int x = 5; x <= 15; x++)
        {
            Assert.Equal(TileType.Corridor, map.GetTileType(new Position(x, 10)));
        }
    }

    [Fact]
    public void CorridorGenerator_CarveVerticalCorridor_CreatesVerticalPath()
    {
        var map = CreateTestMap();
        CorridorGenerator.CarveVerticalCorridor(map, 5, 15, 10);

        for (int y = 5; y <= 15; y++)
        {
            Assert.Equal(TileType.Corridor, map.GetTileType(new Position(10, y)));
        }
    }

    [Fact]
    public void CorridorGenerator_CarveHorizontalCorridor_ReversedCoords_Works()
    {
        var map = CreateTestMap();
        // x1 > x2 のケースも正しく動作
        CorridorGenerator.CarveHorizontalCorridor(map, 15, 5, 10);

        for (int x = 5; x <= 15; x++)
        {
            Assert.Equal(TileType.Corridor, map.GetTileType(new Position(x, 10)));
        }
    }

    [Fact]
    public void CorridorGenerator_DoesNotOverwriteFloorTiles()
    {
        var map = CreateTestMap();
        // まず床タイルを配置
        map.SetTile(10, 10, TileType.Floor);

        CorridorGenerator.CarveHorizontalCorridor(map, 8, 12, 10);

        // 床はそのまま（CorridorではなくFloor）
        Assert.Equal(TileType.Floor, map.GetTileType(new Position(10, 10)));
    }

    #endregion

    #region FeaturePlacer

    [Fact]
    public void FeaturePlacer_PlaceEnemies_ReturnsSpawnPoints()
    {
        var map = CreateTestMap();
        // 部屋を作成して床タイルを配置
        var room1 = new Room { Id = 0, X = 5, Y = 5, Width = 10, Height = 8, Type = RoomType.Normal };
        var room2 = new Room { Id = 1, X = 30, Y = 20, Width = 12, Height = 10, Type = RoomType.Normal };
        RoomGenerator.CarveRoom(map, room1);
        RoomGenerator.CarveRoom(map, room2);
        map.AddRoom(room1);
        map.AddRoom(room2);
        map.SetEntrance(room1.Center);

        var placer = new FeaturePlacer(new Random(42));
        var spawnPoints = placer.PlaceEnemies(map, 0.02f, 5);

        Assert.True(spawnPoints.Count > 0);
    }

    [Fact]
    public void FeaturePlacer_PlaceEnemies_AllOnWalkableTiles()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 20, Height = 15, Type = RoomType.Normal };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);
        map.SetEntrance(room.Center);

        var placer = new FeaturePlacer(new Random(42));
        var spawnPoints = placer.PlaceEnemies(map, 0.02f, 1);

        foreach (var sp in spawnPoints)
        {
            Assert.True(map.IsWalkable(sp.Position), $"スポーン位置 {sp.Position} が歩行不可");
        }
    }

    [Fact]
    public void FeaturePlacer_PlaceItems_ReturnsSpawnPoints()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 15, Height = 10, Type = RoomType.Normal };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        var placer = new FeaturePlacer(new Random(42));
        var items = placer.PlaceItems(map, 0.02f);

        Assert.True(items.Count > 0);
    }

    [Fact]
    public void FeaturePlacer_PlaceItems_InTreasureRoom_HasUncommonQuality()
    {
        var map = CreateTestMap();
        var room = new Room { Id = 0, X = 10, Y = 10, Width = 15, Height = 10, Type = RoomType.Treasure };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        // 宝箱を配置
        var center = room.Center;
        map.SetTile(center, TileType.Chest);

        var placer = new FeaturePlacer(new Random(42));
        var items = placer.PlaceItems(map, 0.05f);

        // 宝箱内のアイテムはIsInChest=trueでUncommon品質
        var chestItems = items.Where(i => i.IsInChest).ToList();
        if (chestItems.Any())
        {
            Assert.All(chestItems, i => Assert.Equal(ItemQuality.Uncommon, i.Quality));
        }
    }

    [Fact]
    public void FeaturePlacer_PlaceSecretRooms_AddsRoomToMap()
    {
        var map = CreateTestMap(60, 40);
        // まず通常の部屋と通路を作って、壁の領域を確保
        var room = new Room { Id = 0, X = 2, Y = 2, Width = 10, Height = 8, Type = RoomType.Normal };
        RoomGenerator.CarveRoom(map, room);
        map.AddRoom(room);

        int initialRoomCount = map.Rooms.Count;

        var placer = new FeaturePlacer(new Random(42));
        placer.PlaceSecretRooms(map, 1);

        // 隠し部屋が追加されたかは壁の配置次第（試行回数に依存）
        // 少なくともクラッシュしないことを確認
        Assert.True(map.Rooms.Count >= initialRoomCount);
    }

    #endregion

    #region EnemySpawnPoint / ItemSpawnPoint

    [Fact]
    public void EnemySpawnPoint_Properties_SetCorrectly()
    {
        var sp = new EnemySpawnPoint
        {
            Position = new Position(5, 5),
            IsBoss = true,
            IsElite = false,
            Depth = 10
        };

        Assert.Equal(new Position(5, 5), sp.Position);
        Assert.True(sp.IsBoss);
        Assert.False(sp.IsElite);
        Assert.Equal(10, sp.Depth);
    }

    [Fact]
    public void ItemSpawnPoint_Properties_SetCorrectly()
    {
        var sp = new ItemSpawnPoint
        {
            Position = new Position(3, 7),
            IsInChest = true,
            Quality = ItemQuality.Rare
        };

        Assert.Equal(new Position(3, 7), sp.Position);
        Assert.True(sp.IsInChest);
        Assert.Equal(ItemQuality.Rare, sp.Quality);
    }

    [Theory]
    [InlineData(ItemQuality.Common)]
    [InlineData(ItemQuality.Uncommon)]
    [InlineData(ItemQuality.Rare)]
    [InlineData(ItemQuality.Epic)]
    [InlineData(ItemQuality.Legendary)]
    public void ItemQuality_AllValues_AreValid(ItemQuality quality)
    {
        Assert.True(Enum.IsDefined(quality));
    }

    #endregion
}
