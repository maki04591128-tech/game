using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class TrapCraftingSystemTests
{
    [Fact]
    public void GetRecipe_SpikeTrap_ReturnsData()
    {
        var recipe = TrapCraftingSystem.GetRecipe(PlayerTrapType.SpikeTrap);
        Assert.NotNull(recipe);
        Assert.Equal("棘罠", recipe.Name);
    }

    [Fact]
    public void CanCraft_Sufficient_True()
    {
        Assert.True(TrapCraftingSystem.CanCraft(PlayerTrapType.SpikeTrap, 10, 15));
    }

    [Fact]
    public void CanCraft_Insufficient_False()
    {
        Assert.False(TrapCraftingSystem.CanCraft(PlayerTrapType.ExplosiveTrap, 5, 5));
    }

    [Fact]
    public void CalculateEfficiency_HighSkill_AboveOne()
    {
        Assert.True(TrapCraftingSystem.CalculateEfficiency(PlayerTrapType.SpikeTrap, 30) > 1.0f);
    }

    [Fact]
    public void GetAllRecipes_ReturnsFiveTypes()
    {
        Assert.Equal(5, TrapCraftingSystem.GetAllRecipes().Count);
    }
}
