using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class MimicSystemTests
{
    [Fact]
    public void CalculateMimicSpawnRate_Floor1_LowRate()
    {
        float rate = MimicSystem.CalculateMimicSpawnRate(1);
        Assert.True(rate > 0);
        Assert.True(rate < 0.1f);
    }

    [Fact]
    public void CalculateMimicSpawnRate_DeepFloor_HigherRate()
    {
        float shallow = MimicSystem.CalculateMimicSpawnRate(1);
        float deep = MimicSystem.CalculateMimicSpawnRate(20);
        Assert.True(deep > shallow);
    }

    [Fact]
    public void CalculateDetectionRate_HighPerception_HigherRate()
    {
        float highPerception = MimicSystem.CalculateDetectionRate(20, 50, false);
        float lowPerception = MimicSystem.CalculateDetectionRate(5, 50, false);
        Assert.True(highPerception > lowPerception);
    }

    [Fact]
    public void GetMimicRewardMultiplier_ReturnsPositive()
    {
        Assert.True(MimicSystem.GetMimicRewardMultiplier() > 1.0f);
    }

    [Fact]
    public void GetDisguiseTypes_ReturnsNonEmpty()
    {
        var types = MimicSystem.GetDisguiseTypes();
        Assert.True(types.Count > 0);
    }

    [Fact]
    public void GetMimicStrengthMultiplier_HigherGrade_StrongerMimic()
    {
        float standard = MimicSystem.GetMimicStrengthMultiplier(ItemGrade.Standard);
        float masterwork = MimicSystem.GetMimicStrengthMultiplier(ItemGrade.Masterwork);
        Assert.True(masterwork > standard);
    }
}
