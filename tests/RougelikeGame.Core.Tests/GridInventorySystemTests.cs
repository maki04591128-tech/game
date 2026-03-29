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

    [Fact]
    public void Reset_ClearsAllItems()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "剣", GridItemSize.Size1x2, 0, 0);
        system.PlaceItem("item2", "盾", GridItemSize.Size2x2, 3, 0);
        system.Reset();
        Assert.Empty(system.Items);
        Assert.Equal(1.0f, system.GetFreeSpaceRatio());
    }

    [Fact]
    public void PlaceItem_MultipleItems_FreeSpaceDecreases()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "ポーション", GridItemSize.Size1x1, 0, 0);
        system.PlaceItem("item2", "大剣", GridItemSize.Size2x3, 2, 0);
        float ratio = system.GetFreeSpaceRatio();
        Assert.True(ratio < 1.0f);
        Assert.True(ratio > 0.0f);
    }

    [Fact]
    public void CanPlace_AfterRemove_ReturnsTrue()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "剣", GridItemSize.Size2x2, 0, 0);
        system.RemoveItem("item1");
        Assert.True(system.CanPlace(GridItemSize.Size2x2, 0, 0));
    }

    [Fact]
    public void PlaceItem_AdjacentNoOverlap_Succeeds()
    {
        var system = new GridInventorySystem(10, 6);
        system.PlaceItem("item1", "剣", GridItemSize.Size2x2, 0, 0);
        Assert.True(system.PlaceItem("item2", "盾", GridItemSize.Size2x2, 2, 0));
    }

    [Fact]
    public void RemoveItem_NonExistent_ReturnsFalse()
    {
        var system = new GridInventorySystem(10, 6);
        Assert.False(system.RemoveItem("nonexistent"));
    }

    [Fact]
    public void Width_And_Height_ReturnConstructorValues()
    {
        var system = new GridInventorySystem(8, 4);
        Assert.Equal(8, system.Width);
        Assert.Equal(4, system.Height);
    }

    [Theory]
    [InlineData(GridItemSize.Size1x2, 1, 2)]
    [InlineData(GridItemSize.Size2x1, 2, 1)]
    [InlineData(GridItemSize.Size2x2, 2, 2)]
    public void GetDimensions_AllSizes_ReturnCorrectValues(GridItemSize size, int expectedW, int expectedH)
    {
        var (w, h) = GridInventorySystem.GetDimensions(size);
        Assert.Equal(expectedW, w);
        Assert.Equal(expectedH, h);
    }

    [Fact]
    public void CanPlace_NegativeCoords_ReturnsFalse()
    {
        var system = new GridInventorySystem(10, 6);
        Assert.False(system.CanPlace(GridItemSize.Size1x1, -1, 0));
        Assert.False(system.CanPlace(GridItemSize.Size1x1, 0, -1));
    }
}
