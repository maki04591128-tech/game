using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class SkillFusionSystemTests
{
    [Fact]
    public void FindRecipe_ValidCombination_ReturnsRecipe()
    {
        var recipe = SkillFusionSystem.FindRecipe("パワースラッシュ", "ファイアボルト");
        Assert.NotNull(recipe);
        Assert.Equal("炎斬り", recipe.ResultSkill);
    }

    [Fact]
    public void FindRecipe_ReversedOrder_StillFinds()
    {
        var recipe = SkillFusionSystem.FindRecipe("ファイアボルト", "パワースラッシュ");
        Assert.NotNull(recipe);
    }

    [Fact]
    public void FindRecipe_Invalid_ReturnsNull()
    {
        Assert.Null(SkillFusionSystem.FindRecipe("不明A", "不明B"));
    }

    [Fact]
    public void CanFuse_SufficientProficiency_True()
    {
        Assert.True(SkillFusionSystem.CanFuse("パワースラッシュ", "ファイアボルト", 30));
    }

    [Fact]
    public void CanFuse_InsufficientProficiency_False()
    {
        Assert.False(SkillFusionSystem.CanFuse("パワースラッシュ", "ファイアボルト", 10));
    }

    [Fact]
    public void GetAllRecipes_ReturnsNonEmpty()
    {
        Assert.True(SkillFusionSystem.GetAllRecipes().Count > 0);
    }
}
