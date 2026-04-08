using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

#pragma warning disable CS0618 // Obsolete旧enum互換テスト維持

namespace RougelikeGame.Core.Tests;

public class ThirstSystemTests
{
    [Theory]
    [InlineData(ThirstLevel.Hydrated, 1.0f)]
    [InlineData(ThirstLevel.SevereDehydration, 0.4f)]
    public void GetThirstModifiers_Str_ReturnsExpected(ThirstLevel level, float expectedStr)
    {
        var (str, _, _) = ThirstSystem.GetThirstModifiers(level);
        Assert.Equal(expectedStr, str);
    }

    [Theory]
    [InlineData(WaterQuality.Pure, 0.0f)]
    [InlineData(WaterQuality.Polluted, 0.5f)]
    public void GetInfectionRisk_ReturnsExpected(WaterQuality quality, float expected)
    {
        Assert.Equal(expected, ThirstSystem.GetInfectionRisk(quality));
    }

    [Theory]
    [InlineData(ThirstLevel.Hydrated, "潤い")]
    [InlineData(ThirstLevel.Dehydrated, "脱水")]
    public void GetThirstName_ReturnsJapanese(ThirstLevel level, string expected)
    {
        Assert.Equal(expected, ThirstSystem.GetThirstName(level));
    }

    [Fact]
    public void Purify_ImprovesQuality()
    {
        Assert.Equal(WaterQuality.River, ThirstSystem.Purify(WaterQuality.Muddy));
    }

    [Fact]
    public void GetThirstDamage_SevereIsHighest()
    {
        Assert.True(ThirstSystem.GetThirstDamage(ThirstLevel.SevereDehydration) > ThirstSystem.GetThirstDamage(ThirstLevel.Hydrated));
    }
}
