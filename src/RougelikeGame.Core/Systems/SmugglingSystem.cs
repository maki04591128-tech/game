namespace RougelikeGame.Core.Systems;

/// <summary>
/// 密輸・禁制品取引システム - 領地別違法アイテムと検査イベント
/// </summary>
public static class SmugglingSystem
{
    /// <summary>禁制品定義</summary>
    public record ContrabandDefinition(
        ContrabandType Type,
        string Name,
        int BasePrice,
        int SmuggleBonus,
        float DetectionChance
    );

    private static readonly List<ContrabandDefinition> Contrabands = new()
    {
        new(ContrabandType.IllegalWeapons, "禁制武器", 500, 300, 0.3f),
        new(ContrabandType.MonsterMaterials, "魔物素材", 300, 200, 0.2f),
        new(ContrabandType.ForbiddenBooks, "禁書", 800, 500, 0.4f),
        new(ContrabandType.Poisons, "毒物", 400, 250, 0.35f),
    };

    /// <summary>全禁制品定義を取得</summary>
    public static IReadOnlyList<ContrabandDefinition> GetAllContrabands() => Contrabands;

    /// <summary>検査回避判定（DEXベース）</summary>
    public static bool CheckEvasion(float detectionChance, int dexterity, double randomValue)
    {
        float effectiveChance = detectionChance - dexterity * 0.01f;
        return randomValue >= Math.Max(0.05, effectiveChance);
    }

    /// <summary>密輸利益を計算</summary>
    public static int CalculateProfit(ContrabandType type)
    {
        var item = Contrabands.FirstOrDefault(c => c.Type == type);
        return item?.SmuggleBonus ?? 0;
    }

    /// <summary>禁制品種別名を取得</summary>
    public static string GetTypeName(ContrabandType type) => type switch
    {
        ContrabandType.IllegalWeapons => "違法武器",
        ContrabandType.MonsterMaterials => "魔物素材",
        ContrabandType.ForbiddenBooks => "禁書",
        ContrabandType.Poisons => "毒物",
        _ => "不明"
    };

    /// <summary>発覚時のペナルティ（カルマ減少量）</summary>
    public static int GetPenalty(ContrabandType type) => type switch
    {
        ContrabandType.IllegalWeapons => -15,
        ContrabandType.MonsterMaterials => -10,
        ContrabandType.ForbiddenBooks => -20,
        ContrabandType.Poisons => -25,
        _ => -10
    };
}
