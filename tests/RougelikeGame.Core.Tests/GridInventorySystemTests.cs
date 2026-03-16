using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class GridInventorySystemTests
{
    [Fact]
    public void PlaceItem_ValidPosition_Succeeds()
    {
        var system = new GridInventorySystem(10, 6);
        Assert.True(system.PlaceItem("item1", "剣", GridItemSize.Size1x2, 0, 0));
    }

    [Fact]
    public void PlaceItem_OutOfBounds_Fails()
    {
        var system = new GridInventorySystem(10, 6);
        Assert.False(system.PlaceItem("item1", "剣", GridItemSize.Size2x3, 9, 5));
    }

    [Fact]
    public void PlaceItem_Overlap_Fails()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "剣", GridItemSize.Size2x2, 0, 0);
        Assert.False(system.PlaceItem("item2", "盾", GridItemSize.Size2x2, 1, 1));
    }

    [Fact]
    public void RemoveItem_Succeeds()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "剣", GridItemSize.Size1x1, 0, 0);
        Assert.True(system.RemoveItem("item1"));
        Assert.Empty(system.Items);
    }

    [Fact]
    public void GetFreeSpaceRatio_InitiallyFull()
    {
        var system = new GridInventorySystem(10, 6);
        Assert.Equal(1.0f, system.GetFreeSpaceRatio());
    }

    [Theory]
    [InlineData(GridItemSize.Size1x1, 1, 1)]
    [InlineData(GridItemSize.Size2x3, 2, 3)]
    public void GetDimensions_ReturnsCorrectSize(GridItemSize size, int expectedW, int expectedH)
    {
        var (w, h) = GridInventorySystem.GetDimensions(size);
        Assert.Equal(expectedW, w);
        Assert.Equal(expectedH, h);
    }
}
