using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class SmugglingSystemTests
{
    [Fact]
    public void GetAllContrabands_ReturnsFourTypes()
    {
        Assert.Equal(4, SmugglingSystem.GetAllContrabands().Count);
    }

    [Fact]
    public void CheckEvasion_HighDex_MoreLikelyToEvade()
    {
        // With high DEX, effective detection should be lower
        Assert.True(SmugglingSystem.CheckEvasion(0.3f, 50, 0.1));
    }

    [Fact]
    public void CalculateProfit_IllegalWeapons_ReturnsBonus()
    {
        Assert.True(SmugglingSystem.CalculateProfit(ContrabandType.IllegalWeapons) > 0);
    }

    [Theory]
    [InlineData(ContrabandType.IllegalWeapons, "違法武器")]
    [InlineData(ContrabandType.Poisons, "毒物")]
    public void GetTypeName_ReturnsJapanese(ContrabandType type, string expected)
    {
        Assert.Equal(expected, SmugglingSystem.GetTypeName(type));
    }

    [Fact]
    public void GetPenalty_ReturnsNegative()
    {
        Assert.True(SmugglingSystem.GetPenalty(ContrabandType.ForbiddenBooks) < 0);
    }
}
