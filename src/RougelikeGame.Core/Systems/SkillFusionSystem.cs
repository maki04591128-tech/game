namespace RougelikeGame.Core.Systems;

/// <summary>
/// スキル合成システム - 別職業スキル×2で合成スキルを生成
/// </summary>
public static class SkillFusionSystem
{
    /// <summary>合成レシピ</summary>
    public record FusionRecipe(
        string SkillA,
        string SkillB,
        string ResultSkill,
        string Description,
        int RequiredProficiency
    );

    private static readonly List<FusionRecipe> Recipes = new()
    {
        new("strong_strike", "fireball", "flame_slash", "炎属性の近接攻撃", 30),
        new("heal", "poison_mist", "purify", "毒を回復に変換", 40),
        new("shield_bash", "lightning_bolt", "thunder_shield", "雷属性の盾攻撃＋麻痺", 50),
        new("sneak", "backstab", "shadow_stitch", "確定クリティカル＋移動封じ", 60),
        new("meditation", "ki_strike", "spirit_blast", "MP消費で大ダメージ", 70),
    };

    /// <summary>合成可能なレシピを検索</summary>
    public static FusionRecipe? FindRecipe(string skillA, string skillB)
    {
        return Recipes.FirstOrDefault(r =>
            (r.SkillA == skillA && r.SkillB == skillB) ||
            (r.SkillA == skillB && r.SkillB == skillA));
    }

    /// <summary>全レシピを取得</summary>
    public static IReadOnlyList<FusionRecipe> GetAllRecipes() => Recipes;

    /// <summary>合成可能か判定</summary>
    public static bool CanFuse(string skillA, string skillB, int proficiency)
    {
        var recipe = FindRecipe(skillA, skillB);
        return recipe != null && proficiency >= recipe.RequiredProficiency;
    }

    /// <summary>合成実行（レシピ結果を返す）</summary>
    public static string? ExecuteFusion(string skillA, string skillB, int proficiency)
    {
        if (!CanFuse(skillA, skillB, proficiency)) return null;
        return FindRecipe(skillA, skillB)?.ResultSkill;
    }

    /// <summary>合成に必要な熟練度を取得</summary>
    public static int GetRequiredProficiency(string skillA, string skillB)
    {
        return FindRecipe(skillA, skillB)?.RequiredProficiency ?? -1;
    }
}
