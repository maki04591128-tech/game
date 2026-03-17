using Xunit;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class AutoExploreSystemTests
{
    [Fact]
    public void CheckStopConditions_EnemyInSight_ReturnsEnemyDetected()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, true, false, false, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.EnemyDetected, result);
    }

    [Fact]
    public void CheckStopConditions_TrapNearby_ReturnsTrapDetected()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, false, false, true, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.TrapDetected, result);
    }

    [Fact]
    public void CheckStopConditions_LowHp_ReturnsLowHp()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.2f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.LowHp, result);
    }

    [Fact]
    public void CheckStopConditions_LowHunger_ReturnsLowHunger()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 1.0f, 0.1f);
        Assert.Equal(AutoExploreSystem.StopReason.LowHunger, result);
    }

    [Fact]
    public void CheckStopConditions_ItemOnTile_ReturnsItemFound()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, true, false, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.ItemFound, result);
    }

    [Fact]
    public void CheckStopConditions_StairsNearby_ReturnsStairsFound()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, false, true, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.StairsFound, result);
    }

    [Fact]
    public void CheckStopConditions_FullyExplored_ReturnsFullyExplored()
    {
        var result = AutoExploreSystem.CheckStopConditions(false, false, false, false, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.FullyExplored, result);
    }

    [Fact]
    public void CheckStopConditions_NoStop_ReturnsNull()
    {
        var result = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 1.0f, 1.0f);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(AutoExploreSystem.StopReason.EnemyDetected)]
    [InlineData(AutoExploreSystem.StopReason.FullyExplored)]
    [InlineData(AutoExploreSystem.StopReason.LowHp)]
    public void GetStopMessage_ReturnsNonEmptyMessage(AutoExploreSystem.StopReason reason)
    {
        var msg = AutoExploreSystem.GetStopMessage(reason);
        Assert.False(string.IsNullOrEmpty(msg));
    }

    [Fact]
    public void CalculateExplorationPriority_MoreUndiscovered_HigherPriority()
    {
        float priorityMore = AutoExploreSystem.CalculateExplorationPriority(5, 10);
        float priorityLess = AutoExploreSystem.CalculateExplorationPriority(5, 2);
        Assert.True(priorityMore > priorityLess);
    }
}
