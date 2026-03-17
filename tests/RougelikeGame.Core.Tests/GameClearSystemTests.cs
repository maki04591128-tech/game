using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class GameClearSystemTests
{
    [Theory]
    [InlineData(Background.Noble, "王都")]
    [InlineData(Background.Soldier, "魔王")]
    public void GetClearCondition_ReturnsCondition(Background bg, string contains)
    {
        Assert.Contains(contains, GameClearSystem.GetClearCondition(bg));
    }

    [Theory]
    [InlineData(3000, 0, "S")]
    [InlineData(8000, 2, "A")]
    [InlineData(15000, 5, "B")]
    [InlineData(40000, 20, "C")]
    [InlineData(60000, 50, "D")]
    public void GetClearRank_ReturnsCorrectRank(int turns, int deaths, string expected)
    {
        Assert.Equal(expected, GameClearSystem.GetClearRank(turns, deaths));
    }

    [Fact]
    public void GetClearMessage_ContainsRank()
    {
        var msg = GameClearSystem.GetClearMessage("貴族", "S");
        Assert.Contains("S", msg);
        Assert.Contains("貴族", msg);
    }

    [Theory]
    [InlineData("S", true)]
    [InlineData("A", true)]
    [InlineData("D", false)]
    public void UnlocksNewGamePlus_ReturnsExpected(string rank, bool expected)
    {
        Assert.Equal(expected, GameClearSystem.UnlocksNewGamePlus(rank));
    }
}
