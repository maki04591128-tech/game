using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class SkillFusionSystemTests
{
    [Fact]
    public void FindRecipe_ValidCombination_ReturnsRecipe()
    {
        var recipe = SkillFusionSystem.FindRecipe("strong_strike", "fireball");
        Assert.NotNull(recipe);
        Assert.Equal("flame_slash", recipe.ResultSkill);
    }

    [Fact]
    public void FindRecipe_ReversedOrder_StillFinds()
    {
        var recipe = SkillFusionSystem.FindRecipe("fireball", "strong_strike");
        Assert.NotNull(recipe);
    }

    [Fact]
    public void FindRecipe_Invalid_ReturnsNull()
    {
        Assert.Null(SkillFusionSystem.FindRecipe("unknown_a", "unknown_b"));
    }

    [Fact]
    public void CanFuse_SufficientProficiency_True()
    {
        Assert.True(SkillFusionSystem.CanFuse("strong_strike", "fireball", 30));
    }

    [Fact]
    public void CanFuse_InsufficientProficiency_False()
    {
        Assert.False(SkillFusionSystem.CanFuse("strong_strike", "fireball", 10));
    }

    [Fact]
    public void GetAllRecipes_ReturnsNonEmpty()
    {
        Assert.True(SkillFusionSystem.GetAllRecipes().Count > 0);
    }
}
