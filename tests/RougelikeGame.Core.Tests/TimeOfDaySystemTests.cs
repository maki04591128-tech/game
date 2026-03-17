using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.53 時刻行動変化システムのテスト
/// </summary>
public class TimeOfDaySystemTests
{
    #region ActivityPattern Tests

    [Theory]
    [InlineData(MonsterRace.Beast, ActivityPattern.Nocturnal)]
    [InlineData(MonsterRace.Humanoid, ActivityPattern.Diurnal)]
    [InlineData(MonsterRace.Amorphous, ActivityPattern.Constant)]
    [InlineData(MonsterRace.Undead, ActivityPattern.Nocturnal)]
    [InlineData(MonsterRace.Demon, ActivityPattern.Nocturnal)]
    [InlineData(MonsterRace.Dragon, ActivityPattern.Diurnal)]
    [InlineData(MonsterRace.Plant, ActivityPattern.Diurnal)]
    [InlineData(MonsterRace.Insect, ActivityPattern.Crepuscular)]
    [InlineData(MonsterRace.Spirit, ActivityPattern.Nocturnal)]
    [InlineData(MonsterRace.Construct, ActivityPattern.Constant)]
    public void GetActivityPattern_ReturnsCorrectPattern(MonsterRace race, ActivityPattern expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetActivityPattern(race));
    }

    #endregion

    #region ActivityMultiplier Tests

    [Theory]
    [InlineData(ActivityPattern.Diurnal, TimePeriod.Morning, 1.0f)]
    [InlineData(ActivityPattern.Diurnal, TimePeriod.Night, 0.5f)]
    [InlineData(ActivityPattern.Nocturnal, TimePeriod.Night, 1.2f)]
    [InlineData(ActivityPattern.Nocturnal, TimePeriod.Morning, 0.6f)]
    [InlineData(ActivityPattern.Crepuscular, TimePeriod.Dawn, 1.3f)]
    [InlineData(ActivityPattern.Crepuscular, TimePeriod.Dusk, 1.3f)]
    [InlineData(ActivityPattern.Constant, TimePeriod.Morning, 1.0f)]
    [InlineData(ActivityPattern.Constant, TimePeriod.Midnight, 1.0f)]
    public void GetActivityMultiplier_ReturnsCorrectValue(ActivityPattern pattern, TimePeriod time, float expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetActivityMultiplier(pattern, time));
    }

    #endregion

    #region SightRange Tests

    [Theory]
    [InlineData(TimePeriod.Morning, 1.0f)]
    [InlineData(TimePeriod.Night, 0.6f)]
    [InlineData(TimePeriod.Midnight, 0.4f)]
    [InlineData(TimePeriod.Dawn, 0.7f)]
    public void GetSightRangeModifier_ReturnsCorrectValue(TimePeriod time, float expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetSightRangeModifier(time));
    }

    #endregion

    #region IsActiveTime Tests

    [Fact]
    public void IsActiveTime_DiurnalAtMorning_ReturnsTrue()
    {
        Assert.True(TimeOfDaySystem.IsActiveTime(MonsterRace.Humanoid, TimePeriod.Morning));
    }

    [Fact]
    public void IsActiveTime_DiurnalAtMidnight_ReturnsFalse()
    {
        Assert.False(TimeOfDaySystem.IsActiveTime(MonsterRace.Humanoid, TimePeriod.Midnight));
    }

    [Fact]
    public void IsActiveTime_NocturnalAtNight_ReturnsTrue()
    {
        Assert.True(TimeOfDaySystem.IsActiveTime(MonsterRace.Undead, TimePeriod.Night));
    }

    [Fact]
    public void IsActiveTime_ConstantAlwaysActive()
    {
        foreach (TimePeriod period in Enum.GetValues<TimePeriod>())
        {
            Assert.True(TimeOfDaySystem.IsActiveTime(MonsterRace.Construct, period));
        }
    }

    #endregion

    #region StatModifier Tests

    [Fact]
    public void GetStatModifier_ActiveTime_PositiveBonus()
    {
        var mod = TimeOfDaySystem.GetStatModifier(MonsterRace.Undead, TimePeriod.Night);
        Assert.True(mod.Strength > 0);
    }

    [Fact]
    public void GetStatModifier_InactiveTime_NegativeBonus()
    {
        var mod = TimeOfDaySystem.GetStatModifier(MonsterRace.Humanoid, TimePeriod.Midnight);
        Assert.True(mod.Strength < 0);
    }

    [Fact]
    public void GetStatModifier_Constant_ZeroModifier()
    {
        var mod = TimeOfDaySystem.GetStatModifier(MonsterRace.Construct, TimePeriod.Morning);
        Assert.Equal(StatModifier.Zero, mod);
    }

    #endregion

    #region TimePeriod Utility Tests

    [Theory]
    [InlineData(0, TimePeriod.Midnight)]
    [InlineData(5, TimePeriod.Dawn)]
    [InlineData(10, TimePeriod.Morning)]
    [InlineData(14, TimePeriod.Afternoon)]
    [InlineData(18, TimePeriod.Dusk)]
    [InlineData(22, TimePeriod.Night)]
    public void GetTimePeriod_ReturnsCorrectPeriod(int hour, TimePeriod expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetTimePeriod(hour));
    }

    [Theory]
    [InlineData(TimePeriod.Dawn, "早朝")]
    [InlineData(TimePeriod.Morning, "午前")]
    [InlineData(TimePeriod.Night, "夜")]
    public void GetTimePeriodName_ReturnsJapaneseName(TimePeriod period, string expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetTimePeriodName(period));
    }

    [Theory]
    [InlineData(ActivityPattern.Diurnal, "昼行性")]
    [InlineData(ActivityPattern.Nocturnal, "夜行性")]
    public void GetActivityPatternName_ReturnsJapaneseName(ActivityPattern pattern, string expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetActivityPatternName(pattern));
    }

    #endregion
}
