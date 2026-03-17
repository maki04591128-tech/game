using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class CookingSystemTests
{
    [Fact]
    public void FindRecipe_Exists()
    {
        var recipe = CookingSystem.FindRecipe("焼き肉");
        Assert.NotNull(recipe);
        Assert.Equal(CookingMethod.Grill, recipe.Method);
    }

    [Fact]
    public void FindRecipe_NotExists_ReturnsNull()
    {
        Assert.Null(CookingSystem.FindRecipe("存在しないレシピ"));
    }

    [Theory]
    [InlineData(CookingMethod.Grill, "焼く")]
    [InlineData(CookingMethod.Boil, "煮る")]
    [InlineData(CookingMethod.Ferment, "発酵")]
    public void GetMethodName_ReturnsJapanese(CookingMethod method, string expected)
    {
        Assert.Equal(expected, CookingSystem.GetMethodName(method));
    }

    [Fact]
    public void CalculateQuality_HighProficiency_Better()
    {
        Assert.True(CookingSystem.CalculateQuality(80) > CookingSystem.CalculateQuality(10));
    }

    [Fact]
    public void GetCookingTime_FermentLongest()
    {
        Assert.True(CookingSystem.GetCookingTime(CookingMethod.Ferment) > CookingSystem.GetCookingTime(CookingMethod.Grill));
    }
}
