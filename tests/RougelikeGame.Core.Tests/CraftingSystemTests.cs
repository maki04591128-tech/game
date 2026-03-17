using Xunit;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class CraftingSystemTests
{
    [Fact]
    public void GetAllRecipes_ReturnsDefaultRecipes()
    {
        var system = new CraftingSystem();
        var recipes = system.GetAllRecipes();
        Assert.True(recipes.Count > 0);
    }

    [Fact]
    public void GetRecipe_ExistingRecipe_ReturnsRecipe()
    {
        var system = new CraftingSystem();
        var recipe = system.GetRecipe("recipe_iron_sword");
        Assert.NotNull(recipe);
        Assert.Equal("鉄の剣の鍛造", recipe.Name);
    }

    [Fact]
    public void GetRecipe_NonExistingRecipe_ReturnsNull()
    {
        var system = new CraftingSystem();
        Assert.Null(system.GetRecipe("nonexistent"));
    }

    [Fact]
    public void GetAvailableRecipes_Level1_ReturnsLevel1Recipes()
    {
        var system = new CraftingSystem();
        var recipes = system.GetAvailableRecipes(1);
        Assert.True(recipes.Count > 0);
        Assert.All(recipes, r => Assert.True(r.RequiredLevel <= 1));
    }

    [Fact]
    public void GetAvailableRecipes_Level10_ReturnsMoreRecipes()
    {
        var system = new CraftingSystem();
        var recipesLv1 = system.GetAvailableRecipes(1);
        var recipesLv10 = system.GetAvailableRecipes(10);
        Assert.True(recipesLv10.Count >= recipesLv1.Count);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 95)]
    [InlineData(5, 55)]
    [InlineData(9, 10)]
    [InlineData(10, 5)]
    public void CalculateEnhanceSuccessRate_ReturnsExpected(int level, int expected)
    {
        Assert.Equal(expected, CraftingSystem.CalculateEnhanceSuccessRate(level));
    }

    [Fact]
    public void CraftingInventory_AddAndRemove()
    {
        var inv = new CraftingInventory();
        inv.AddItem("item1", 5);
        Assert.Equal(5, inv.CountItem("item1"));
        Assert.True(inv.HasItem("item1", 3));
        Assert.True(inv.RemoveItem("item1", 3));
        Assert.Equal(2, inv.CountItem("item1"));
    }

    [Fact]
    public void CraftingInventory_RemoveInsufficient_ReturnsFalse()
    {
        var inv = new CraftingInventory();
        inv.AddItem("item1", 2);
        Assert.False(inv.RemoveItem("item1", 5));
    }

    [Fact]
    public void RegisterRecipe_CustomRecipe()
    {
        var system = new CraftingSystem();
        var recipe = new CraftingRecipe("test_recipe", "テストレシピ", "テスト",
            new List<CraftingMaterial> { new("test_item", 1) }, "result_item");
        system.RegisterRecipe(recipe);
        Assert.NotNull(system.GetRecipe("test_recipe"));
    }
}
