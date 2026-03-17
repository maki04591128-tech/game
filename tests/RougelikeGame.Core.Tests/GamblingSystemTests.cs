using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class GamblingSystemTests
{
    [Theory]
    [InlineData(3, 3, true)]
    [InlineData(3, 5, false)]
    public void JudgeDice_ReturnsCorrect(int guess, int result, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeDice(guess, result));
    }

    [Theory]
    [InlineData(true, 2, 4, true)]   // Cho(even): 2+4=6, even
    [InlineData(true, 1, 2, false)]  // Cho(even): 1+2=3, odd
    public void JudgeChoHan_ReturnsCorrect(bool choseCho, int d1, int d2, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeChoHan(choseCho, d1, d2));
    }

    [Fact]
    public void JudgeHighLow_Equal_Loses()
    {
        Assert.False(GamblingSystem.JudgeHighLow(true, 5, 5));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice, 6.0f)]
    [InlineData(GamblingGameType.ChoHan, 2.0f)]
    public void GetPayoutMultiplier_ReturnsExpected(GamblingGameType type, float expected)
    {
        Assert.Equal(expected, GamblingSystem.GetPayoutMultiplier(type));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice, "サイコロ")]
    [InlineData(GamblingGameType.ChoHan, "丁半")]
    [InlineData(GamblingGameType.Card, "ハイ＆ロー")]
    public void GetGameName_ReturnsJapanese(GamblingGameType type, string expected)
    {
        Assert.Equal(expected, GamblingSystem.GetGameName(type));
    }

    [Fact]
    public void GetMinimumBet_DiceIsHighest()
    {
        Assert.True(GamblingSystem.GetMinimumBet(GamblingGameType.Dice) > GamblingSystem.GetMinimumBet(GamblingGameType.ChoHan));
    }
}
