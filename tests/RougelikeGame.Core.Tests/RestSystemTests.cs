using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class RestSystemTests
{
    [Theory]
    [InlineData(SleepQuality.DeepSleep)]
    [InlineData(SleepQuality.Normal)]
    [InlineData(SleepQuality.Light)]
    [InlineData(SleepQuality.Nap)]
    public void GetRecoveryRates_ReturnsPositiveValues(SleepQuality quality)
    {
        var (hp, mp, fatigue, sanity) = RestSystem.GetRecoveryRates(quality);
        Assert.True(hp >= 0);
        Assert.True(mp >= 0);
        Assert.True(fatigue >= 0);
        Assert.True(sanity >= 0);
    }

    [Fact]
    public void GetRecoveryRates_DeepSleep_HigherThanNap()
    {
        var deep = RestSystem.GetRecoveryRates(SleepQuality.DeepSleep);
        var nap = RestSystem.GetRecoveryRates(SleepQuality.Nap);
        Assert.True(deep.HpRecovery > nap.HpRecovery);
    }

    [Fact]
    public void GetSleepDuration_DeepSleep_LongestDuration()
    {
        int deep = RestSystem.GetSleepDuration(SleepQuality.DeepSleep);
        int nap = RestSystem.GetSleepDuration(SleepQuality.Nap);
        Assert.True(deep > nap);
    }

    [Fact]
    public void CanCamp_EnemyNearby_ReturnsFalse()
    {
        Assert.False(RestSystem.CanCamp(false, true, 0));
    }

    [Fact]
    public void CanCamp_SafeArea_ReturnsTrue()
    {
        Assert.True(RestSystem.CanCamp(false, false, 0));
    }

    [Fact]
    public void CanCamp_DeepDungeon_ReturnsFalse()
    {
        Assert.False(RestSystem.CanCamp(true, false, 10));
    }

    [Fact]
    public void CalculateAmbushChance_WithCampfire_LowerChance()
    {
        float withFire = RestSystem.CalculateAmbushChance(5, true, false);
        float withoutFire = RestSystem.CalculateAmbushChance(5, false, false);
        Assert.True(withFire < withoutFire);
    }

    [Theory]
    [InlineData(SleepQuality.DeepSleep, "熟睡")]
    [InlineData(SleepQuality.Normal, "普通")]
    [InlineData(SleepQuality.Light, "浅い眠り")]
    [InlineData(SleepQuality.Nap, "仮眠")]
    public void GetQualityName_ReturnsJapaneseName(SleepQuality quality, string expected)
    {
        Assert.Equal(expected, RestSystem.GetQualityName(quality));
    }
}
