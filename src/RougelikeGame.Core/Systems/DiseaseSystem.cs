namespace RougelikeGame.Core.Systems;

/// <summary>
/// 病気・疫病システム - 病気の発症/治療/重症化を管理
/// </summary>
public static class DiseaseSystem
{
    /// <summary>病気の詳細定義</summary>
    public record DiseaseDefinition(
        DiseaseType Type,
        string Name,
        string Description,
        int DefaultDuration,
        bool SelfHealing,
        Dictionary<string, float> StatModifiers
    );

    private static readonly Dictionary<DiseaseType, DiseaseDefinition> Diseases = new()
    {
        [DiseaseType.Cold] = new(DiseaseType.Cold, "風邪",
            "軽い体調不良。自然回復あり", 30, true,
            new() { ["STR"] = -2, ["AGI"] = -1, ["ActionSpeed"] = -0.05f }),

        [DiseaseType.Infection] = new(DiseaseType.Infection, "感染症",
            "傷口からの感染。治療しないと重症化", 50, false,
            new() { ["VIT"] = -3, ["HpRegen"] = -0.5f, ["MaxHp"] = -0.1f }),

        [DiseaseType.FoodPoisoning] = new(DiseaseType.FoodPoisoning, "食中毒",
            "汚染された食料を食べた。一時的", 15, true,
            new() { ["STR"] = -3, ["AGI"] = -3, ["Hunger"] = -0.2f }),

        [DiseaseType.Miasma] = new(DiseaseType.Miasma, "瘴気病",
            "瘴気に汚染されたダンジョンで感染。重度", 80, false,
            new() { ["MaxHp"] = -0.15f, ["MaxMp"] = -0.1f, ["Sanity"] = -0.1f }),

        [DiseaseType.CursePlague] = new(DiseaseType.CursePlague, "呪い病",
            "呪いの攻撃で感染。魔法治療が必要", 100, false,
            new() { ["LUK"] = -5, ["HolyResist"] = -0.3f, ["Sanity"] = -0.15f }),
    };

    /// <summary>病気定義を取得</summary>
    public static DiseaseDefinition? GetDisease(DiseaseType type)
    {
        return Diseases.TryGetValue(type, out var d) ? d : null;
    }

    /// <summary>全病気定義を取得</summary>
    public static IReadOnlyDictionary<DiseaseType, DiseaseDefinition> GetAllDiseases() => Diseases;

    /// <summary>感染判定（傷状態+環境）</summary>
    public static bool CheckInfection(bool hasOpenWound, int vitality, double randomValue)
    {
        float chance = hasOpenWound ? 0.2f : 0.05f;
        chance -= vitality * 0.005f;
        return randomValue < Math.Max(0.01, chance);
    }

    /// <summary>自然回復判定（ターン経過）</summary>
    public static bool CheckNaturalRecovery(DiseaseType type, int remainingDuration, int vitality)
    {
        var disease = GetDisease(type);
        if (disease == null || !disease.SelfHealing) return false;
        float recoveryChance = 0.02f + vitality * 0.003f;
        if (remainingDuration < disease.DefaultDuration / 2) recoveryChance += 0.05f;
        return new Random().NextDouble() < recoveryChance;
    }

    /// <summary>治療コストを計算</summary>
    public static int CalculateTreatmentCost(DiseaseType type) => type switch
    {
        DiseaseType.Cold => 50,
        DiseaseType.FoodPoisoning => 80,
        DiseaseType.Infection => 200,
        DiseaseType.Miasma => 500,
        DiseaseType.CursePlague => 1000,
        _ => 100
    };
}
