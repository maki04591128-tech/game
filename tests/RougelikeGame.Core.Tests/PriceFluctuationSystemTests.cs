using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class PriceFluctuationSystemTests
{
    [Fact]
    public void CalculateSupplyDemandModifier_HighSupply_LowPrice()
    {
        float modifier = PriceFluctuationSystem.CalculateSupplyDemandModifier(100, 10);
        Assert.True(modifier < 1.0f);
    }

    [Fact]
    public void CalculateSupplyDemandModifier_LowSupply_HighPrice()
    {
        float modifier = PriceFluctuationSystem.CalculateSupplyDemandModifier(1, 100);
        Assert.True(modifier > 1.0f);
    }

    [Fact]
    public void CalculateSupplyDemandModifier_ZeroSupply_MaxModifier()
    {
        float modifier = PriceFluctuationSystem.CalculateSupplyDemandModifier(0, 100);
        Assert.Equal(2.0f, modifier);
    }

    [Fact]
    public void GetKarmaModifier_Saint_LowerBuyPrice()
    {
        float buyModifier = PriceFluctuationSystem.GetKarmaModifier(KarmaRank.Saint, true);
        Assert.True(buyModifier < 1.0f);
    }

    [Fact]
    public void CalculateFinalPrice_MinimumIs1()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(1, 0.01f, 0.01f, 0.01f, 0.01f, true);
        Assert.True(price >= 1);
    }

    [Fact]
    public void GetReputationModifier_Revered_LowerBuyPrice()
    {
        float modifier = PriceFluctuationSystem.GetReputationModifier(ReputationRank.Revered, true);
        Assert.True(modifier < 1.0f);
    }
}
