namespace RougelikeGame.Core.Systems;

/// <summary>
/// 罠作成システム - プレイヤーによる罠の設置
/// </summary>
public static class TrapCraftingSystem
{
    /// <summary>罠レシピ</summary>
    public record TrapRecipe(
        PlayerTrapType Type,
        string Name,
        int MaterialCost,
        int RequiredSmithing,
        int Damage,
        string Effect
    );

    private static readonly Dictionary<PlayerTrapType, TrapRecipe> Recipes = new()
    {
        [PlayerTrapType.SpikeTrap] = new(PlayerTrapType.SpikeTrap, "棘罠", 5, 10, 20, "物理ダメージ"),
        [PlayerTrapType.PitfallTrap] = new(PlayerTrapType.PitfallTrap, "落とし穴", 8, 15, 15, "落下+移動不可1ターン"),
        [PlayerTrapType.ExplosiveTrap] = new(PlayerTrapType.ExplosiveTrap, "爆発罠", 15, 30, 40, "範囲3×3ダメージ"),
        [PlayerTrapType.SleepTrap] = new(PlayerTrapType.SleepTrap, "睡眠罠", 10, 20, 0, "睡眠3ターン"),
        [PlayerTrapType.AlarmTrap] = new(PlayerTrapType.AlarmTrap, "警報罠", 3, 5, 0, "周囲の敵を誘引"),
    };

    /// <summary>罠レシピを取得</summary>
    public static TrapRecipe? GetRecipe(PlayerTrapType type)
    {
        return Recipes.TryGetValue(type, out var r) ? r : null;
    }

    /// <summary>全レシピを取得</summary>
    public static IReadOnlyDictionary<PlayerTrapType, TrapRecipe> GetAllRecipes() => Recipes;

    /// <summary>作成可能か判定</summary>
    public static bool CanCraft(PlayerTrapType type, int materials, int smithingLevel)
    {
        var recipe = GetRecipe(type);
        return recipe != null && materials >= recipe.MaterialCost && smithingLevel >= recipe.RequiredSmithing;
    }

    /// <summary>罠効果の期待値を計算</summary>
    public static float CalculateEfficiency(PlayerTrapType type, int smithingLevel)
    {
        var recipe = GetRecipe(type);
        if (recipe == null) return 0;
        float bonus = (smithingLevel - recipe.RequiredSmithing) * 0.02f;
        return 1.0f + Math.Max(0, bonus);
    }
}
