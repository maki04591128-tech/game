using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class InfiniteDungeonSystemTests
{
    [Theory]
    [InlineData(5, InfiniteDungeonTier.Normal)]
    [InlineData(20, InfiniteDungeonTier.Advanced)]
    [InlineData(40, InfiniteDungeonTier.Deep)]
    [InlineData(60, InfiniteDungeonTier.Abyss)]
    public void GetTier_ReturnsCorrect(int floor, InfiniteDungeonTier expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.GetTier(floor));
    }

    [Fact]
    public void CalculateEnemyLevel_ScalesWithFloor()
    {
        Assert.True(InfiniteDungeonSystem.CalculateEnemyLevel(50) > InfiniteDungeonSystem.CalculateEnemyLevel(1));
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(15, false)]
    [InlineData(0, false)]
    public void IsBossFloor_ReturnsCorrect(int floor, bool expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.IsBossFloor(floor));
    }

    [Fact]
    public void GetDropRateMultiplier_IncreasesWithFloor()
    {
        Assert.True(InfiniteDungeonSystem.GetDropRateMultiplier(50) > InfiniteDungeonSystem.GetDropRateMultiplier(1));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsUnlocked_ReturnsCorrect(bool cleared, bool expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.IsUnlocked(cleared));
    }
}
