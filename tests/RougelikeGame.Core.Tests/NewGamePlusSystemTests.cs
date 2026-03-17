using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class NewGamePlusSystemTests
{
    [Fact]
    public void GetConfig_Plus1_ReturnsData()
    {
        var config = NewGamePlusSystem.GetConfig(NewGamePlusTier.Plus1);
        Assert.NotNull(config);
        Assert.Equal(1.5f, config.EnemyMultiplier);
    }

    [Theory]
    [InlineData(true, "S", true)]
    [InlineData(true, "D", false)]
    [InlineData(false, "S", false)]
    public void CanStartNewGamePlus_ReturnsExpected(bool cleared, string rank, bool expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.CanStartNewGamePlus(cleared, rank));
    }

    [Fact]
    public void GetCarryOverItems_Plus1_HasBasics()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus1);
        Assert.Contains("レベル", items);
        Assert.Contains("スキル", items);
    }

    [Fact]
    public void GetCarryOverItems_Plus4_HasAll()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus4);
        Assert.Contains("全アイテム", items);
    }

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, "NG+1")]
    [InlineData(NewGamePlusTier.Plus5, "NG+5")]
    public void GetTierName_ReturnsCorrect(NewGamePlusTier tier, string expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.GetTierName(tier));
    }
}
