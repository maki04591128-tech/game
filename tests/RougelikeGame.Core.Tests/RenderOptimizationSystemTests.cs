using Xunit;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class RenderOptimizationSystemTests
{
    [Fact]
    public void CalculateViewport_CenteredOnPlayer()
    {
        var (minX, minY, maxX, maxY) = RenderOptimizationSystem.CalculateViewport(50, 50, 20, 20);
        Assert.Equal(40, minX);
        Assert.Equal(40, minY);
        Assert.Equal(60, maxX);
        Assert.Equal(60, maxY);
    }

    [Theory]
    [InlineData(50, 50, 40, 40, 60, 60, true)]
    [InlineData(30, 30, 40, 40, 60, 60, false)]
    public void IsInViewport_ReturnsCorrect(int x, int y, int minX, int minY, int maxX, int maxY, bool expected)
    {
        Assert.Equal(expected, RenderOptimizationSystem.IsInViewport(x, y, minX, minY, maxX, maxY));
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(15, 2)]
    [InlineData(30, 4)]
    [InlineData(50, 8)]
    [InlineData(100, 16)]
    public void CalculateUpdateFrequency_DistanceBased(int distance, int expected)
    {
        Assert.Equal(expected, RenderOptimizationSystem.CalculateUpdateFrequency(distance));
    }

    [Fact]
    public void CalculateDistance_ChebyshevDistance()
    {
        Assert.Equal(5, RenderOptimizationSystem.CalculateDistance(0, 0, 3, 5));
        Assert.Equal(0, RenderOptimizationSystem.CalculateDistance(5, 5, 5, 5));
    }

    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(4, 2, true)]
    [InlineData(3, 2, false)]
    public void ShouldUpdate_ReturnsCorrect(int turn, int freq, bool expected)
    {
        Assert.Equal(expected, RenderOptimizationSystem.ShouldUpdate(turn, freq));
    }
}
