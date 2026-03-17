using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class BodyConditionSystemTests
{
    [Theory]
    [InlineData(BodyWoundType.Cut, "切り傷")]
    [InlineData(BodyWoundType.Fracture, "骨折")]
    [InlineData(BodyWoundType.Burn, "火傷")]
    public void GetWound_ReturnsCorrectName(BodyWoundType type, string expected)
    {
        var wound = BodyConditionSystem.GetWound(type);
        Assert.NotNull(wound);
        Assert.Equal(expected, wound.Name);
    }

    [Theory]
    [InlineData(FatigueLevel.Fresh, 1.0f)]
    [InlineData(FatigueLevel.Collapse, 0.0f)]
    public void GetFatigueModifier_ReturnsExpected(FatigueLevel level, float expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetFatigueModifier(level));
    }

    [Fact]
    public void GetHygieneInfectionRisk_FoulIsHighest()
    {
        Assert.True(BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Foul) > BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Clean));
    }

    [Theory]
    [InlineData(FatigueLevel.Fresh, "元気")]
    [InlineData(FatigueLevel.Exhausted, "重疲労")]
    public void GetFatigueName_ReturnsJapanese(FatigueLevel level, string expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetFatigueName(level));
    }

    [Theory]
    [InlineData(HygieneLevel.Clean, "清潔")]
    [InlineData(HygieneLevel.Foul, "不潔")]
    public void GetHygieneName_ReturnsJapanese(HygieneLevel level, string expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetHygieneName(level));
    }
}
