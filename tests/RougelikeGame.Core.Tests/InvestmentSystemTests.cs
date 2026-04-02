using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class InvestmentSystemTests
{
    [Fact]
    public void Invest_AddsRecord()
    {
        var system = new InvestmentSystem();
        Assert.True(system.Invest(InvestmentType.Shop, "武器屋", 1000, 100));
        Assert.Single(system.Investments);
    }

    [Theory]
    [InlineData(InvestmentType.Shop, 0.6f)]
    [InlineData(InvestmentType.AdventurerParty, 0.3f)]
    public void GetSuccessRate_ReturnsExpected(InvestmentType type, float expected)
    {
        Assert.Equal(expected, InvestmentSystem.GetSuccessRate(type));
    }

    [Fact]
    public void GetExpectedReturn_ShopReturns130Percent()
    {
        Assert.Equal(1300f, InvestmentSystem.GetExpectedReturn(InvestmentType.Shop, 1000));
    }

    [Fact]
    public void GetTotalInvested_SumsCorrectly()
    {
        var system = new InvestmentSystem();
        system.Invest(InvestmentType.Shop, "A", 500, 1);
        system.Invest(InvestmentType.Business, "B", 300, 2);
        Assert.Equal(800, system.GetTotalInvested());
    }

    [Theory]
    [InlineData(InvestmentType.Shop, "ショップ投資")]
    [InlineData(InvestmentType.Business, "事業出資")]
    public void GetTypeName_ReturnsJapanese(InvestmentType type, string expected)
    {
        Assert.Equal(expected, InvestmentSystem.GetTypeName(type));
    }
}
