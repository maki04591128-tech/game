using Xunit;
using RougelikeGame.Gui;

namespace RougelikeGame.Gui.Tests;

/// <summary>
/// GameTime（ゲーム内暦）のユニットテスト
/// </summary>
public class GameTimeTests
{
    [Fact]
    public void InitialState_ReturnsDefaultStartDate()
    {
        // Arrange & Act
        var time = new GameTime();

        // Assert
        Assert.Equal(1024, time.Year);
        Assert.Equal(6, time.Month);
        Assert.Equal(15, time.Day);
        Assert.Equal(8, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal(0, time.TotalTurns);
    }

    [Fact]
    public void InitialState_MonthName_IsCorrect()
    {
        // Arrange & Act
        var time = new GameTime();

        // Assert
        Assert.Equal("緑風の月", time.MonthName);
    }

    [Fact]
    public void InitialState_TimePeriod_IsCorrect()
    {
        // Arrange
        var time = new GameTime();

        // Assert - 08:00 は「朝」
        Assert.Equal("朝", time.TimePeriod);
    }

    [Fact]
    public void InitialState_ToFullString_FormatsCorrectly()
    {
        // Arrange & Act
        var time = new GameTime();

        // Assert
        Assert.Equal("冒険歴1024年 緑風の月 15日 08:00", time.ToFullString());
    }

    [Fact]
    public void InitialState_ToShortString_FormatsCorrectly()
    {
        // Arrange & Act
        var time = new GameTime();

        // Assert
        Assert.Equal("緑風の月 15日 08:00", time.ToShortString());
    }

    [Fact]
    public void AdvanceTurn_1Turn_DoesNotAdvanceMinute()
    {
        // Arrange
        var time = new GameTime();

        // Act
        time.AdvanceTurn();

        // Assert
        Assert.Equal(1, time.TotalTurns);
        Assert.Equal(0, time.Minute); // 60ターンで1分なので1ターンでは変化なし
        Assert.Equal(8, time.Hour);
    }

    [Fact]
    public void AdvanceTurn_60Turns_Advances1Minute()
    {
        // Arrange
        var time = new GameTime();

        // Act
        time.AdvanceTurn(60);

        // Assert
        Assert.Equal(60, time.TotalTurns);
        Assert.Equal(1, time.Minute);
        Assert.Equal(8, time.Hour);
    }

    [Fact]
    public void AdvanceTurn_3600Turns_Advances1Hour()
    {
        // Arrange
        var time = new GameTime();

        // Act - 3600ターン = 60分 = 1時間
        time.AdvanceTurn(3600);

        // Assert
        Assert.Equal(3600, time.TotalTurns);
        Assert.Equal(0, time.Minute);
        Assert.Equal(9, time.Hour);
        Assert.Equal(15, time.Day);
    }

    [Fact]
    public void AdvanceTurn_MultipleCalls_Accumulates()
    {
        // Arrange
        var time = new GameTime();

        // Act
        time.AdvanceTurn(1800); // 30分
        time.AdvanceTurn(1800); // 30分 → 合計60分=1時間

        // Assert
        Assert.Equal(3600, time.TotalTurns);
        Assert.Equal(9, time.Hour);
        Assert.Equal(0, time.Minute);
    }

    [Fact]
    public void AdvanceTurn_86400Turns_Advances1Day()
    {
        // Arrange
        var time = new GameTime(); // 15日 08:00

        // Act - 86400ターン = 1440分 = 24時間 = 1日
        time.AdvanceTurn(86400);

        // Assert
        Assert.Equal(16, time.Day);
        Assert.Equal(8, time.Hour);
        Assert.Equal(0, time.Minute);
    }

    [Fact]
    public void AdvanceTurn_CrossesDayBoundary()
    {
        // Arrange
        var time = new GameTime(); // 15日 08:00

        // Act - 16時間 = 960分 = 57600ターン → 16日 00:00
        time.AdvanceTurn(57600);

        // Assert
        Assert.Equal(16, time.Day);
        Assert.Equal(0, time.Hour);
        Assert.Equal(0, time.Minute);
    }

    [Fact]
    public void AdvanceTurn_CrossesMonthBoundary()
    {
        // Arrange
        var time = new GameTime(); // 6月15日 08:00

        // Act - 22560分 = 1353600ターン → 7月1日 00:00
        time.AdvanceTurn(1_353_600);

        // Assert
        Assert.Equal(7, time.Month);
        Assert.Equal(1, time.Day);
        Assert.Equal(0, time.Hour);
        Assert.Equal("盛夏の月", time.MonthName);
    }

    [Fact]
    public void AdvanceTurn_CrossesYearBoundary()
    {
        // Arrange
        var time = new GameTime(); // 1024年 6月15日 08:00

        // Act - 281760分 * 60 = 16905600ターン → 1025年1月1日00:00
        time.AdvanceTurn(16_905_600);

        // Assert
        Assert.Equal(1025, time.Year);
        Assert.Equal(1, time.Month);
        Assert.Equal(1, time.Day);
        Assert.Equal(0, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal("霜の月", time.MonthName);
    }

    [Theory]
    [InlineData(0, "朝")]          // 08:00
    [InlineData(7200, "午前")]     // 10:00 (120分*60)
    [InlineData(10800, "午前")]    // 11:00 (180分*60)
    [InlineData(14400, "昼")]      // 12:00 (240分*60)
    [InlineData(21600, "午後")]    // 14:00 (360分*60)
    [InlineData(32400, "夕方")]    // 17:00 (540分*60)
    [InlineData(39600, "夜")]      // 19:00 (660分*60)
    [InlineData(57600, "深夜")]    // 00:00 翌日 (960分*60)
    public void TimePeriod_ChangesWithTime(int turnsToAdvance, string expectedPeriod)
    {
        // Arrange
        var time = new GameTime();

        // Act
        time.AdvanceTurn(turnsToAdvance);

        // Assert
        Assert.Equal(expectedPeriod, time.TimePeriod);
    }

    [Theory]
    [InlineData(1, "霜の月")]
    [InlineData(2, "雪解の月")]
    [InlineData(3, "芽吹の月")]
    [InlineData(4, "花咲の月")]
    [InlineData(5, "陽光の月")]
    [InlineData(6, "緑風の月")]
    [InlineData(7, "盛夏の月")]
    [InlineData(8, "収穫の月")]
    [InlineData(9, "紅葉の月")]
    [InlineData(10, "落葉の月")]
    [InlineData(11, "薄暮の月")]
    [InlineData(12, "星霜の月")]
    public void MonthNames_AreCorrect(int targetMonth, string expectedName)
    {
        // Arrange - 1月1日 00:00開始にする
        var time = new GameTime
        {
            StartYear = 1024,
            StartMonth = 1,
            StartDay = 1,
            StartHour = 0,
            StartMinute = 0
        };

        // Act - (targetMonth - 1) * 30日分進める（60ターン/分）
        long turnsToAdvance = (long)(targetMonth - 1) * 30 * 1440 * 60;
        time.AdvanceTurn((int)turnsToAdvance);

        // Assert
        Assert.Equal(targetMonth, time.Month);
        Assert.Equal(expectedName, time.MonthName);
    }

    [Fact]
    public void CustomStartDate_Works()
    {
        // Arrange
        var time = new GameTime
        {
            EraName = "魔王",
            StartYear = 2000,
            StartMonth = 12,
            StartDay = 30,
            StartHour = 23,
            StartMinute = 59
        };

        // Assert
        Assert.Equal("魔王歴2000年 星霜の月 30日 23:59", time.ToFullString());
    }

    [Fact]
    public void CustomStartDate_AdvanceAcrossYear()
    {
        // Arrange - 12月30日 23:59
        var time = new GameTime
        {
            StartYear = 2000,
            StartMonth = 12,
            StartDay = 30,
            StartHour = 23,
            StartMinute = 59
        };

        // Act - 60ターン進める（=1分） → 2001年 1月1日 00:00
        time.AdvanceTurn(60);

        // Assert
        Assert.Equal(2001, time.Year);
        Assert.Equal(1, time.Month);
        Assert.Equal(1, time.Day);
        Assert.Equal(0, time.Hour);
        Assert.Equal(0, time.Minute);
    }

    [Fact]
    public void ToString_ReturnsFullString()
    {
        // Arrange
        var time = new GameTime();

        // Assert
        Assert.Equal(time.ToFullString(), time.ToString());
    }

    [Fact]
    public void ToFullString_Format_PadsZeros()
    {
        // Arrange
        var time = new GameTime
        {
            StartYear = 5,
            StartMonth = 1,
            StartDay = 1,
            StartHour = 1,
            StartMinute = 5
        };

        // Assert - 年は4桁ゼロ埋め、日は2桁、時:分は2桁ゼロ埋め
        Assert.Equal("冒険歴0005年 霜の月 01日 01:05", time.ToFullString());
    }

    [Fact]
    public void LargeAdvance_DoesNotOverflow()
    {
        // Arrange
        var time = new GameTime();

        // Act - 10年分進める（518400 * 60 * 10 = 311040000ターン）
        time.AdvanceTurn(311_040_000);

        // Assert
        Assert.Equal(1034, time.Year);
        Assert.Equal(6, time.Month);
        Assert.Equal(15, time.Day);
        Assert.Equal(8, time.Hour);
        Assert.Equal(0, time.Minute);
    }
}
