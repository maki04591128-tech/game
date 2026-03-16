using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class BlackMarketSystemTests
{
    [Theory]
    [InlineData(-20, true)]
    [InlineData(-10, false)]
    [InlineData(0, false)]
    public void CanAccess_KarmaThreshold(int karma, bool expected)
    {
        Assert.Equal(expected, BlackMarketSystem.CanAccess(karma));
    }

    [Fact]
    public void GetAvailableItems_HighKarma_Empty()
    {
        Assert.Empty(BlackMarketSystem.GetAvailableItems(0));
    }

    [Fact]
    public void GetAvailableItems_LowKarma_HasItems()
    {
        Assert.True(BlackMarketSystem.GetAvailableItems(-50).Count > 0);
    }

    [Fact]
    public void CalculateDetectionRisk_HighDex_LowerRisk()
    {
        var highDex = BlackMarketSystem.CalculateDetectionRisk(0.5f, 30, 20);
        var lowDex = BlackMarketSystem.CalculateDetectionRisk(0.5f, 5, 5);
        Assert.True(highDex < lowDex);
    }

    [Theory]
    [InlineData(BlackMarketCategory.StolenGoods, "盗品")]
    [InlineData(BlackMarketCategory.Information, "情報")]
    public void GetCategoryName_ReturnsJapanese(BlackMarketCategory cat, string expected)
    {
        Assert.Equal(expected, BlackMarketSystem.GetCategoryName(cat));
    }
}
