using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class OathSystemTests
{
    [Fact]
    public void TakeOath_Success()
    {
        var system = new OathSystem();
        Assert.True(system.TakeOath(OathType.Temperance));
        Assert.Contains(OathType.Temperance, system.ActiveOaths);
    }

    [Fact]
    public void TakeOath_Duplicate_ReturnsFalse()
    {
        var system = new OathSystem();
        system.TakeOath(OathType.Pacifism);
        Assert.False(system.TakeOath(OathType.Pacifism));
    }

    [Fact]
    public void BreakOath_Success()
    {
        var system = new OathSystem();
        system.TakeOath(OathType.Darkness);
        Assert.True(system.BreakOath(OathType.Darkness));
    }

    [Fact]
    public void GetTotalExpBonus_AccumulatesCorrectly()
    {
        var system = new OathSystem();
        system.TakeOath(OathType.Temperance);
        system.TakeOath(OathType.Pacifism);
        Assert.True(system.GetTotalExpBonus() > 0);
    }

    [Theory]
    [InlineData(OathType.Temperance, "use_alcohol", true)]
    [InlineData(OathType.Pacifism, "attack_enemy", true)]
    [InlineData(OathType.Temperance, "attack_enemy", false)]
    public void IsViolation_ReturnsCorrect(OathType type, string action, bool expected)
    {
        var system = new OathSystem();
        Assert.Equal(expected, system.IsViolation(type, action));
    }
}
