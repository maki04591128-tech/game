using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class EnvironmentalPuzzleSystemTests
{
    [Fact]
    public void GetByType_RuneLanguage_ReturnsResults()
    {
        var puzzles = EnvironmentalPuzzleSystem.GetByType(PuzzleType.RuneLanguage);
        Assert.True(puzzles.Count > 0);
    }

    [Fact]
    public void CanAttempt_HighStats_True()
    {
        Assert.True(EnvironmentalPuzzleSystem.CanAttempt(PuzzleType.Physical, 10, 5));
    }

    [Fact]
    public void CalculateSuccessRate_HighInt_HigherRate()
    {
        var high = EnvironmentalPuzzleSystem.CalculateSuccessRate(1, 20);
        var low = EnvironmentalPuzzleSystem.CalculateSuccessRate(1, 5);
        Assert.True(high > low);
    }

    [Theory]
    [InlineData(PuzzleType.RuneLanguage, "ルーン語パズル")]
    [InlineData(PuzzleType.Elemental, "属性パズル")]
    [InlineData(PuzzleType.Physical, "物理パズル")]
    public void GetTypeName_ReturnsJapanese(PuzzleType type, string expected)
    {
        Assert.Equal(expected, EnvironmentalPuzzleSystem.GetTypeName(type));
    }
}
