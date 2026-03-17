using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class ExecutionSystemTests
{
    [Fact]
    public void CanExecute_LowHp_ReturnsTrue()
    {
        Assert.True(ExecutionSystem.CanExecute(5, 100));
    }

    [Fact]
    public void CanExecute_HighHp_ReturnsFalse()
    {
        Assert.False(ExecutionSystem.CanExecute(50, 100));
    }

    [Fact]
    public void CanExecute_ExactThreshold_ReturnsTrue()
    {
        Assert.True(ExecutionSystem.CanExecute(10, 100));
    }

    [Fact]
    public void GetExecutionExpBonus_ReturnsPositive()
    {
        Assert.True(ExecutionSystem.GetExecutionExpBonus() > 1.0f);
    }

    [Fact]
    public void GetExecutionDropBonus_ReturnsPositive()
    {
        Assert.True(ExecutionSystem.GetExecutionDropBonus() > 1.0f);
    }

    [Fact]
    public void GetMercyKarmaBonus_ReturnsPositive()
    {
        Assert.True(ExecutionSystem.GetMercyKarmaBonus() > 0);
    }

    [Fact]
    public void GetExecutionKarmaPenalty_Humanoid_HighPenalty()
    {
        int humanoidPenalty = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Humanoid);
        int beastPenalty = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Beast);
        Assert.True(Math.Abs(humanoidPenalty) > Math.Abs(beastPenalty));
    }
}
