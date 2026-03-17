namespace RougelikeGame.Core.Systems;

/// <summary>
/// 採取ポイント定義
/// </summary>
public record GatheringNode(
    GatheringType Type,
    string Name,
    string[] PossibleItems,
    int RequiredLevel,
    ProficiencyCategory RequiredProficiency,
    int BaseDuration,
    Season[] BestSeasons
);

/// <summary>
/// 採取システム - 素材収集の基盤
/// </summary>
public static class GatheringSystem
{
    private static readonly Dictionary<GatheringType, GatheringNode> Nodes = new()
    {
        [GatheringType.Herb] = new(GatheringType.Herb, "薬草採取",
            new[] { "herb_heal", "herb_cure", "herb_mana", "herb_rare", "herb_poison" },
            1, ProficiencyCategory.Alchemy, 3,
            new[] { Season.Spring, Season.Summer }),

        [GatheringType.Mining] = new(GatheringType.Mining, "鉱石採掘",
            new[] { "ore_iron", "ore_silver", "ore_gold", "ore_mithril", "gem_rough" },
            3, ProficiencyCategory.Smithing, 5,
            Array.Empty<Season>()),

        [GatheringType.Logging] = new(GatheringType.Logging, "木材伐採",
            new[] { "wood_common", "wood_hard", "wood_magic", "sap_tree" },
            2, ProficiencyCategory.Smithing, 4,
            new[] { Season.Autumn }),

        [GatheringType.Fishing] = new(GatheringType.Fishing, "釣り",
            new[] { "fish_common", "fish_rare", "fish_treasure", "fish_junk" },
            1, ProficiencyCategory.Exploration, 6,
            new[] { Season.Spring, Season.Summer }),

        [GatheringType.Foraging] = new(GatheringType.Foraging, "採集",
            new[] { "mushroom_edible", "mushroom_poison", "berry", "nut", "insect_material" },
            1, ProficiencyCategory.Exploration, 2,
            new[] { Season.Autumn })
    };

    /// <summary>採取ポイント定義を取得</summary>
    public static GatheringNode? GetNode(GatheringType type)
    {
        return Nodes.TryGetValue(type, out var node) ? node : null;
    }

    /// <summary>全採取タイプの定義を取得</summary>
    public static IReadOnlyDictionary<GatheringType, GatheringNode> GetAllNodes() => Nodes;

    /// <summary>採取成功率を計算</summary>
    public static float CalculateSuccessRate(GatheringType type, int proficiencyLevel, Season currentSeason)
    {
        var node = GetNode(type);
        if (node == null) return 0;

        float baseRate = 0.5f + proficiencyLevel * 0.05f;
        if (node.BestSeasons.Contains(currentSeason)) baseRate += 0.15f;
        return Math.Clamp(baseRate, 0.1f, 0.95f);
    }

    /// <summary>レア素材の取得確率を計算</summary>
    public static float CalculateRareItemChance(int proficiencyLevel, float luckModifier)
    {
        return Math.Clamp(0.05f + proficiencyLevel * 0.02f + luckModifier * 0.1f, 0.01f, 0.3f);
    }

    /// <summary>採取に必要なターン数を計算</summary>
    public static int CalculateGatheringDuration(GatheringType type, int proficiencyLevel)
    {
        var node = GetNode(type);
        if (node == null) return 10;
        return Math.Max(1, node.BaseDuration - proficiencyLevel / 3);
    }

    /// <summary>採取可能かどうか判定</summary>
    public static bool CanGather(GatheringType type, int proficiencyLevel)
    {
        var node = GetNode(type);
        return node != null && proficiencyLevel >= node.RequiredLevel;
    }
}
