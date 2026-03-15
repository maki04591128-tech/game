using Xunit;
using RougelikeGame.Core;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 難易度システムのテスト
/// </summary>
public class DifficultySettingsTests
{
    [Theory]
    [InlineData(DifficultyLevel.Easy)]
    [InlineData(DifficultyLevel.Normal)]
    [InlineData(DifficultyLevel.Hard)]
    [InlineData(DifficultyLevel.Nightmare)]
    [InlineData(DifficultyLevel.Ironman)]
    public void Get_AllDifficultyLevels_ReturnValidSettings(DifficultyLevel level)
    {
        // Act
        var settings = DifficultySettings.Get(level);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(level, settings.Level);
        Assert.False(string.IsNullOrEmpty(settings.DisplayName));
        Assert.False(string.IsNullOrEmpty(settings.Description));
    }

    [Fact]
    public void Normal_HasBaselineMultipliers()
    {
        // Act
        var settings = DifficultySettings.Normal;

        // Assert
        Assert.Equal(1.0, settings.EnemyStatMultiplier);
        Assert.Equal(1.0, settings.ExpMultiplier);
        Assert.Equal(1.0, settings.HungerDecayMultiplier);
        Assert.Equal(1.0, settings.TurnLimitMultiplier);
        Assert.Equal(3, settings.RescueCount);
        Assert.Equal(1.0, settings.GoldMultiplier);
        Assert.False(settings.PermaDeath);
    }

    [Fact]
    public void Easy_HasEasierSettings()
    {
        // Act
        var settings = DifficultySettings.Easy;

        // Assert
        Assert.True(settings.EnemyStatMultiplier < 1.0);
        Assert.True(settings.ExpMultiplier > 1.0);
        Assert.True(settings.HungerDecayMultiplier < 1.0);
        Assert.True(settings.TurnLimitMultiplier > 1.0);
        Assert.True(settings.RescueCount > DifficultySettings.Normal.RescueCount);
        Assert.False(settings.PermaDeath);
    }

    [Fact]
    public void Hard_HasHarderSettings()
    {
        // Act
        var settings = DifficultySettings.Hard;

        // Assert
        Assert.True(settings.EnemyStatMultiplier > 1.0);
        Assert.True(settings.ExpMultiplier < 1.0);
        Assert.True(settings.TurnLimitMultiplier < 1.0);
        Assert.True(settings.RescueCount < DifficultySettings.Normal.RescueCount);
        Assert.False(settings.PermaDeath);
    }

    [Fact]
    public void Nightmare_HasVeryHardSettings()
    {
        // Act
        var settings = DifficultySettings.Nightmare;

        // Assert
        Assert.True(settings.EnemyStatMultiplier > DifficultySettings.Hard.EnemyStatMultiplier);
        Assert.True(settings.RescueCount < DifficultySettings.Hard.RescueCount);
        Assert.False(settings.PermaDeath);
    }

    [Fact]
    public void Ironman_HasPermaDeath()
    {
        // Act
        var settings = DifficultySettings.Get(DifficultyLevel.Ironman);

        // Assert
        Assert.True(settings.PermaDeath);
        Assert.Equal(0, settings.RescueCount);
    }
}
