namespace RougelikeGame.Core.Systems;

/// <summary>
/// 料理・調合システム - 素材組み合わせで効果変化
/// </summary>
public static class CookingSystem
{
    /// <summary>レシピ定義</summary>
    public record CookingRecipe(
        string Name,
        CookingMethod Method,
        string[] Ingredients,
        int HpRestore,
        int MpRestore,
        int HungerRestore,
        string SpecialEffect
    );

    private static readonly List<CookingRecipe> Recipes = new()
    {
        new("焼き肉", CookingMethod.Grill, new[] { "生肉" }, 20, 0, 30, ""),
        new("薬草スープ", CookingMethod.Boil, new[] { "薬草", "水" }, 40, 10, 20, "毒回復"),
        new("蒸し魚", CookingMethod.Steam, new[] { "魚", "水" }, 30, 5, 25, ""),
        new("干し肉", CookingMethod.Dry, new[] { "生肉", "塩" }, 15, 0, 40, "保存食（腐敗しない）"),
        new("発酵飲料", CookingMethod.Ferment, new[] { "果実", "水" }, 5, 20, 10, "一時的にLUK+3"),
    };

    /// <summary>レシピを検索</summary>
    public static CookingRecipe? FindRecipe(string name)
    {
        return Recipes.FirstOrDefault(r => r.Name == name);
    }

    /// <summary>全レシピを取得</summary>
    public static IReadOnlyList<CookingRecipe> GetAllRecipes() => Recipes;

    /// <summary>調理方法名を取得</summary>
    public static string GetMethodName(CookingMethod method) => method switch
    {
        CookingMethod.Grill => "焼く",
        CookingMethod.Boil => "煮る",
        CookingMethod.Steam => "蒸す",
        CookingMethod.Dry => "干す",
        CookingMethod.Ferment => "発酵",
        _ => "不明"
    };

    /// <summary>料理の品質倍率を計算（熟練度による）</summary>
    public static float CalculateQuality(int cookingProficiency)
    {
        return 0.5f + Math.Min(cookingProficiency, 100) * 0.01f;
    }

    /// <summary>調理に必要なターン数を取得</summary>
    public static int GetCookingTime(CookingMethod method) => method switch
    {
        CookingMethod.Grill => 3,
        CookingMethod.Boil => 5,
        CookingMethod.Steam => 4,
        CookingMethod.Dry => 10,
        CookingMethod.Ferment => 20,
        _ => 5
    };
}
