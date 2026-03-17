using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class DeathLogSystemTests
{
    private static DeathLogSystem.DeathLogEntry CreateEntry(int run, DeathCause cause, CharacterClass cls, int level = 5, int floor = 3, int turns = 100)
    {
        return new DeathLogSystem.DeathLogEntry(run, "TestChar", cls, Race.Human, level, cause,
            "テスト死因", "テスト場所", floor, turns, DateTime.Now);
    }

    [Fact]
    public void AddLog_IncrementsTotalDeaths()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter));
        Assert.Equal(1, system.TotalDeaths);
    }

    [Fact]
    public void GetDeathsByCategory_ReturnsCorrectCounts()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter));
        system.AddLog(CreateEntry(2, DeathCause.Combat, CharacterClass.Mage));
        system.AddLog(CreateEntry(3, DeathCause.Starvation, CharacterClass.Thief));
        var stats = system.GetDeathsByCategory();
        Assert.Equal(2, stats[DeathCause.Combat]);
        Assert.Equal(1, stats[DeathCause.Starvation]);
    }

    [Fact]
    public void GetMostCommonCause_ReturnsCorrect()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter));
        system.AddLog(CreateEntry(2, DeathCause.Combat, CharacterClass.Fighter));
        system.AddLog(CreateEntry(3, DeathCause.Trap, CharacterClass.Thief));
        Assert.Equal(DeathCause.Combat, system.GetMostCommonCause());
    }

    [Fact]
    public void GetMostCommonCause_EmptyLog_ReturnsNull()
    {
        var system = new DeathLogSystem();
        Assert.Null(system.GetMostCommonCause());
    }

    [Fact]
    public void GetHighestLevel_ReturnsMax()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter, level: 5));
        system.AddLog(CreateEntry(2, DeathCause.Combat, CharacterClass.Fighter, level: 15));
        Assert.Equal(15, system.GetHighestLevel());
    }

    [Fact]
    public void GetDeepestFloor_ReturnsMax()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter, floor: 3));
        system.AddLog(CreateEntry(2, DeathCause.Combat, CharacterClass.Fighter, floor: 10));
        Assert.Equal(10, system.GetDeepestFloor());
    }

    [Fact]
    public void GetAverageSurvivalTurns_CalculatesCorrectly()
    {
        var system = new DeathLogSystem();
        system.AddLog(CreateEntry(1, DeathCause.Combat, CharacterClass.Fighter, turns: 100));
        system.AddLog(CreateEntry(2, DeathCause.Combat, CharacterClass.Fighter, turns: 200));
        Assert.Equal(150.0, system.GetAverageSurvivalTurns());
    }
}
