using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Map;

namespace RougelikeGame.Core.Tests;

public class MapUnitTests
{
    #region Tile

    [Fact]
    public void Tile_Wall_BlocksSightAndMovement()
    {
        var tile = Tile.FromType(TileType.Wall);
        Assert.True(tile.BlocksSight);
        Assert.True(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_Floor_DoesNotBlockAnything()
    {
        var tile = Tile.FromType(TileType.Floor);
        Assert.False(tile.BlocksSight);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_ClosedDoor_BlocksSightAndMovement()
    {
        var tile = Tile.FromType(TileType.DoorClosed);
        Assert.True(tile.BlocksSight);
        Assert.True(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_OpenDoor_DoesNotBlockAnything()
    {
        var tile = Tile.FromType(TileType.DoorOpen);
        Assert.False(tile.BlocksSight);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_Water_HasHigherMovementCost()
    {
        var tile = Tile.FromType(TileType.Water);
        Assert.Equal(2.0f, tile.MovementCost);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_DeepWater_BlocksMovement()
    {
        var tile = Tile.FromType(TileType.DeepWater);
        Assert.True(tile.BlocksMovement);
    }

    [Fact]
    public void Tile_Lava_BlocksMovement()
    {
        var tile = Tile.FromType(TileType.Lava);
        Assert.True(tile.BlocksMovement);
        Assert.False(tile.BlocksSight);
    }

    [Fact]
    public void Tile_Visibility_DefaultValues()
    {
        var tile = Tile.FromType(TileType.Floor);
        Assert.False(tile.IsVisible);
        Assert.False(tile.IsExplored);
    }

    [Fact]
    public void Tile_DisplayChar_WallIsHash()
    {
        var tile = Tile.FromType(TileType.Wall);
        Assert.Equal('#', tile.DisplayChar);
    }

    [Fact]
    public void Tile_DisplayChar_FloorIsDot()
    {
        var tile = Tile.FromType(TileType.Floor);
        Assert.Equal('.', tile.DisplayChar);
    }

    [Fact]
    public void Tile_Feature_CanStoreItemId()
    {
        var tile = Tile.FromType(TileType.Floor);
        tile.ItemId = "test_item";
        Assert.Equal("test_item", tile.ItemId);
    }

    [Fact]
    public void Tile_Pillar_BlocksBoth()
    {
        var tile = Tile.FromType(TileType.Pillar);
        Assert.True(tile.BlocksSight);
        Assert.True(tile.BlocksMovement);
    }

    #endregion

    #region DungeonMap creation and bounds

    [Fact]
    public void DungeonMap_Creation_HasCorrectDimensions()
    {
        var map = new DungeonMap(40, 30);
        Assert.Equal(40, map.Width);
        Assert.Equal(30, map.Height);
    }

    [Fact]
    public void DungeonMap_InitializedWithWalls()
    {
        var map = new DungeonMap(10, 10);
        var tile = map[5, 5];
        Assert.Equal(TileType.Wall, tile.Type);
    }

    [Fact]
    public void DungeonMap_IsInBounds_ValidPosition()
    {
        var map = new DungeonMap(20, 20);
        Assert.True(map.IsInBounds(0, 0));
        Assert.True(map.IsInBounds(19, 19));
    }

    [Fact]
    public void DungeonMap_IsInBounds_InvalidPositionReturnsFalse()
    {
        var map = new DungeonMap(20, 20);
        Assert.False(map.IsInBounds(-1, 0));
        Assert.False(map.IsInBounds(20, 0));
        Assert.False(map.IsInBounds(0, 20));
    }

    [Fact]
    public void DungeonMap_OutOfBoundsAccess_ReturnsWall()
    {
        var map = new DungeonMap(10, 10);
        var tile = map[-1, -1];
        Assert.Equal(TileType.Wall, tile.Type);
    }

    #endregion

    #region DungeonMap tile operations

    [Fact]
    public void DungeonMap_SetTile_ChangesTileType()
    {
        var map = new DungeonMap(10, 10);
        map.SetTile(5, 5, TileType.Floor);
        Assert.Equal(TileType.Floor, map[5, 5].Type);
    }

    [Fact]
    public void DungeonMap_IsWalkable_FloorIsWalkable()
    {
        var map = new DungeonMap(10, 10);
        map.SetTile(5, 5, TileType.Floor);
        Assert.True(map.IsWalkable(new Position(5, 5)));
    }

    [Fact]
    public void DungeonMap_IsWalkable_WallIsNotWalkable()
    {
        var map = new DungeonMap(10, 10);
        Assert.False(map.IsWalkable(new Position(5, 5)));
    }

    #endregion

    #region Room management

    [Fact]
    public void DungeonMap_AddRoom_IncreasesRoomCount()
    {
        var map = new DungeonMap(40, 30);
        Assert.Empty(map.Rooms);
        map.AddRoom(new Room { Id = 0, X = 5, Y = 5, Width = 10, Height = 8 });
        Assert.Single(map.Rooms);
    }

    [Fact]
    public void DungeonMap_GetRoomAt_FindsCorrectRoom()
    {
        var map = new DungeonMap(40, 30);
        var room = new Room { Id = 1, X = 5, Y = 5, Width = 10, Height = 8 };
        map.AddRoom(room);
        var found = map.GetRoomAt(new Position(7, 7));
        Assert.NotNull(found);
        Assert.Equal(1, found!.Id);
    }

    [Fact]
    public void DungeonMap_GetRoomAt_ReturnsNullOutsideRooms()
    {
        var map = new DungeonMap(40, 30);
        map.AddRoom(new Room { Id = 0, X = 5, Y = 5, Width = 10, Height = 8 });
        Assert.Null(map.GetRoomAt(new Position(0, 0)));
    }

    [Fact]
    public void Room_Center_ReturnsMiddlePosition()
    {
        var room = new Room { Id = 0, X = 10, Y = 20, Width = 8, Height = 6 };
        var center = room.Center;
        Assert.Equal(14, center.X);
        Assert.Equal(23, center.Y);
    }

    [Fact]
    public void Room_Area_ReturnsWidthTimesHeight()
    {
        var room = new Room { Id = 0, X = 0, Y = 0, Width = 10, Height = 5 };
        Assert.Equal(50, room.Area);
    }

    #endregion

    #region Floor counting and stairs

    [Fact]
    public void DungeonMap_CountFloorTiles_ReturnsCorrectCount()
    {
        var map = new DungeonMap(10, 10);
        map.SetTile(1, 1, TileType.Floor);
        map.SetTile(2, 2, TileType.Floor);
        map.SetTile(3, 3, TileType.Corridor);
        Assert.Equal(3, map.CountFloorTiles());
    }

    [Fact]
    public void DungeonMap_SetStairs_UpdatesPositionsAndTiles()
    {
        var map = new DungeonMap(10, 10);
        map.SetStairsUp(new Position(2, 2));
        map.SetStairsDown(new Position(8, 8));
        Assert.Equal(new Position(2, 2), map.StairsUpPosition);
        Assert.Equal(new Position(8, 8), map.StairsDownPosition);
        Assert.Equal(TileType.StairsUp, map[2, 2].Type);
        Assert.Equal(TileType.StairsDown, map[8, 8].Type);
    }

    #endregion
}
