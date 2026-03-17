using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.18 天候システムのテスト
/// </summary>
public class WeatherSystemTests
{
    [Theory]
    [InlineData(Weather.Clear, "晴れ")]
    [InlineData(Weather.Rain, "雨")]
    [InlineData(Weather.Fog, "霧")]
    [InlineData(Weather.Snow, "雪")]
    [InlineData(Weather.Storm, "嵐")]
    public void GetWeatherName_ReturnsJapaneseName(Weather weather, string expected)
    {
        Assert.Equal(expected, WeatherSystem.GetWeatherName(weather));
    }

    [Fact]
    public void GetSightModifier_ClearIsFullVisibility()
    {
        Assert.Equal(1.0f, WeatherSystem.GetSightModifier(Weather.Clear));
    }

    [Fact]
    public void GetSightModifier_FogHalvesVisibility()
    {
        Assert.Equal(0.5f, WeatherSystem.GetSightModifier(Weather.Fog));
    }

    [Fact]
    public void GetElementDamageModifier_RainReducesFire()
    {
        Assert.True(WeatherSystem.GetElementDamageModifier(Weather.Rain, Element.Fire) < 1.0f);
    }

    [Fact]
    public void GetElementDamageModifier_RainBoostsLightning()
    {
        Assert.True(WeatherSystem.GetElementDamageModifier(Weather.Rain, Element.Lightning) > 1.0f);
    }

    [Fact]
    public void GetElementDamageModifier_SnowBoostsIce()
    {
        Assert.True(WeatherSystem.GetElementDamageModifier(Weather.Snow, Element.Ice) > 1.0f);
    }

    [Fact]
    public void GetRangedHitModifier_StormPenalty()
    {
        Assert.True(WeatherSystem.GetRangedHitModifier(Weather.Storm) < 0);
    }

    [Fact]
    public void GetRangedHitModifier_ClearBonus()
    {
        Assert.True(WeatherSystem.GetRangedHitModifier(Weather.Clear) > 0);
    }

    [Fact]
    public void GetMovementCostModifier_SnowIncreased()
    {
        Assert.True(WeatherSystem.GetMovementCostModifier(Weather.Snow) > 1.0f);
    }

    [Theory]
    [InlineData(Weather.Rain, true)]
    [InlineData(Weather.Storm, true)]
    [InlineData(Weather.Clear, false)]
    [InlineData(Weather.Fog, false)]
    public void AreTracksErased_ReturnsCorrectResult(Weather weather, bool expected)
    {
        Assert.Equal(expected, WeatherSystem.AreTracksErased(weather));
    }

    [Fact]
    public void DetermineWeather_LowValue_ReturnsClear()
    {
        // With spring probabilities: Clear = 0.4, so 0.1 should return Clear
        Assert.Equal(Weather.Clear, WeatherSystem.DetermineWeather(Season.Spring, 0.1));
    }

    [Fact]
    public void DetermineWeather_WinterHighSnowChance()
    {
        // Winter Snow probability = 0.45, starting at 0.45 cumulative
        // Value 0.5 should land in Snow
        Assert.Equal(Weather.Snow, WeatherSystem.DetermineWeather(Season.Winter, 0.5));
    }

    [Fact]
    public void IsWeatherApplicable_OutdoorTrue()
    {
        Assert.True(WeatherSystem.IsWeatherApplicable(true));
    }

    [Fact]
    public void IsWeatherApplicable_IndoorFalse()
    {
        Assert.False(WeatherSystem.IsWeatherApplicable(false));
    }

    [Fact]
    public void GetEffect_AllWeathers_ReturnValidData()
    {
        foreach (Weather w in Enum.GetValues<Weather>())
        {
            var effect = WeatherSystem.GetEffect(w);
            Assert.NotNull(effect);
            Assert.NotEmpty(effect.Description);
        }
    }
}
