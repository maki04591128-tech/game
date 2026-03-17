using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class CompanionSystemTests
{
    [Fact]
    public void AddCompanion_Success()
    {
        var system = new CompanionSystem();
        var companion = new CompanionSystem.CompanionData("テスト傭兵", CompanionType.Mercenary, CompanionAIMode.Aggressive, 5, 50, 200);
        Assert.True(system.AddCompanion(companion));
        Assert.Single(system.Party);
    }

    [Fact]
    public void AddCompanion_MaxParty_Fails()
    {
        var system = new CompanionSystem();
        for (int i = 0; i < CompanionSystem.MaxPartySize; i++)
            system.AddCompanion(new CompanionSystem.CompanionData($"仲間{i}", CompanionType.Ally, CompanionAIMode.Support, 1, 50, 0));
        var extra = new CompanionSystem.CompanionData("余分", CompanionType.Ally, CompanionAIMode.Wait, 1, 50, 0);
        Assert.False(system.AddCompanion(extra));
    }

    [Fact]
    public void RemoveCompanion_Success()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData("テスト", CompanionType.Pet, CompanionAIMode.Wait, 1, 50, 50));
        Assert.True(system.RemoveCompanion("テスト"));
        Assert.Empty(system.Party);
    }

    [Fact]
    public void SetAIMode_Success()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData("傭兵A", CompanionType.Mercenary, CompanionAIMode.Aggressive, 5, 50, 200));
        Assert.True(system.SetAIMode("傭兵A", CompanionAIMode.Defensive));
    }

    [Theory]
    [InlineData(CompanionType.Mercenary, 10, 600)]
    [InlineData(CompanionType.Ally, 1, 0)]
    [InlineData(CompanionType.Pet, 5, 150)]
    public void CalculateHireCost_ReturnsExpected(CompanionType type, int level, int expected)
    {
        Assert.Equal(expected, CompanionSystem.CalculateHireCost(type, level));
    }

    [Theory]
    [InlineData(CompanionType.Mercenary, "傭兵")]
    [InlineData(CompanionType.Ally, "仲間")]
    [InlineData(CompanionType.Pet, "ペット")]
    public void GetTypeName_ReturnsJapaneseName(CompanionType type, string expected)
    {
        Assert.Equal(expected, CompanionSystem.GetTypeName(type));
    }
}
