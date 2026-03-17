using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.17 季節システムのテスト
/// </summary>
public class SeasonSystemTests
{
    [Theory]
    [InlineData(3, Season.Spring)]
    [InlineData(4, Season.Spring)]
    [InlineData(5, Season.Spring)]
    [InlineData(6, Season.Summer)]
    [InlineData(7, Season.Summer)]
    [InlineData(8, Season.Summer)]
    [InlineData(9, Season.Autumn)]
    [InlineData(10, Season.Autumn)]
    [InlineData(11, Season.Autumn)]
    [InlineData(12, Season.Winter)]
    [InlineData(1, Season.Winter)]
    [InlineData(2, Season.Winter)]
    public void GetSeason_ReturnsCorrectSeason(int month, Season expected)
    {
        Assert.Equal(expected, SeasonSystem.GetSeason(month));
    }

    [Theory]
    [InlineData(Season.Spring, "春")]
    [InlineData(Season.Summer, "夏")]
    [InlineData(Season.Autumn, "秋")]
    [InlineData(Season.Winter, "冬")]
    public void GetSeasonName_ReturnsJapaneseName(Season season, string expected)
    {
        Assert.Equal(expected, SeasonSystem.GetSeasonName(season));
    }

    [Fact]
    public void GetSightModifier_WinterReduced()
    {
        Assert.True(SeasonSystem.GetSightModifier(Season.Winter) < 1.0f);
    }

    [Fact]
    public void GetSightModifier_SummerEnhanced()
    {
        Assert.True(SeasonSystem.GetSightModifier(Season.Summer) > 1.0f);
    }

    [Fact]
    public void GetStaminaCostModifier_WinterIncreased()
    {
        Assert.True(SeasonSystem.GetStaminaCostModifier(Season.Winter) > 1.0f);
    }

    [Fact]
    public void GetStaminaCostModifier_AutumnNormal()
    {
        Assert.Equal(1.0f, SeasonSystem.GetStaminaCostModifier(Season.Autumn));
    }

    [Fact]
    public void IsRaceActive_InsectInSpring_True()
    {
        Assert.True(SeasonSystem.IsRaceActive(Season.Spring, MonsterRace.Insect));
    }

    [Fact]
    public void IsRaceActive_UndeadInWinter_True()
    {
        Assert.True(SeasonSystem.IsRaceActive(Season.Winter, MonsterRace.Undead));
    }

    [Fact]
    public void IsRaceInactive_InsectInWinter_True()
    {
        Assert.True(SeasonSystem.IsRaceInactive(Season.Winter, MonsterRace.Insect));
    }

    [Fact]
    public void GetGatheringModifier_AutumnHighest()
    {
        var autumn = SeasonSystem.GetGatheringModifier(Season.Autumn);
        var spring = SeasonSystem.GetGatheringModifier(Season.Spring);
        var winter = SeasonSystem.GetGatheringModifier(Season.Winter);
        Assert.True(autumn > spring);
        Assert.True(autumn > winter);
    }

    [Fact]
    public void GetFoodAvailability_WinterLowest()
    {
        var winter = SeasonSystem.GetFoodAvailability(Season.Winter);
        Assert.True(winter < 1.0f);
    }

    [Fact]
    public void GetWeatherProbabilities_WinterHasSnow()
    {
        var probs = SeasonSystem.GetWeatherProbabilities(Season.Winter);
        Assert.True(probs[Weather.Snow] > 0);
    }

    [Fact]
    public void GetWeatherProbabilities_SummerNoSnow()
    {
        var probs = SeasonSystem.GetWeatherProbabilities(Season.Summer);
        Assert.Equal(0f, probs[Weather.Snow]);
    }

    [Fact]
    public void GetEffect_ReturnsValidEffect()
    {
        foreach (Season season in Enum.GetValues<Season>())
        {
            var effect = SeasonSystem.GetEffect(season);
            Assert.NotNull(effect);
            Assert.NotEmpty(effect.Description);
        }
    }
}
