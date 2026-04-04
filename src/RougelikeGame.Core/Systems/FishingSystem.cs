namespace RougelikeGame.Core.Systems;

/// <summary>
/// 魚の定義
/// </summary>
public record FishDefinition(
    string Id,
    string Name,
    int Rarity,
    Season[] ActiveSeasons,
    TimePeriod[] ActiveTimes,
    int MinFishingLevel,
    int BasePrice,
    string Description
);

/// <summary>
/// 釣りシステム - 採取システムのサブシステム
/// </summary>
public static class FishingSystem
{
    private static readonly List<FishDefinition> FishList = new()
    {
        new("fish_common_1", "フナ", 1, new[] { Season.Spring, Season.Summer, Season.Autumn },
            new[] { TimePeriod.Morning, TimePeriod.Afternoon }, 1, 10, "どこでも釣れる一般的な淡水魚"),
        new("fish_common_2", "コイ", 1, new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter },
            new[] { TimePeriod.Morning, TimePeriod.Afternoon, TimePeriod.Dusk }, 1, 15, "力強く引く淡水魚"),
        new("fish_medium_1", "マス", 2, new[] { Season.Spring, Season.Summer },
            new[] { TimePeriod.Dawn, TimePeriod.Morning }, 3, 30, "清流に棲む美味な魚"),
        new("fish_medium_2", "ウナギ", 2, new[] { Season.Summer },
            new[] { TimePeriod.Night, TimePeriod.Midnight }, 4, 50, "夜行性の高級魚"),
        new("fish_rare_1", "ニジマス", 3, new[] { Season.Spring },
            new[] { TimePeriod.Dawn }, 6, 80, "朝霧の中でのみ釣れる希少種"),
        new("fish_rare_2", "古代魚", 4, new[] { Season.Winter },
            new[] { TimePeriod.Midnight }, 8, 200, "深層でのみ出現する太古の魚"),
        new("fish_legendary", "幻の大魚", 5, new[] { Season.Autumn },
            new[] { TimePeriod.Dusk }, 10, 500, "伝説に語られる巨大魚"),
        new("fish_treasure", "宝箱", 1, Array.Empty<Season>(),  // EN-1: Rarity0→1
            Array.Empty<TimePeriod>(), 1, 100, "水底に沈んだ宝箱を釣り上げた"),
        new("fish_junk", "ガラクタ", 1, Array.Empty<Season>(),  // EN-1: Rarity0→1
            Array.Empty<TimePeriod>(), 1, 1, "錆びた缶や古い靴"),
    };

    /// <summary>全魚定義を取得</summary>
    public static IReadOnlyList<FishDefinition> GetAllFish() => FishList;

    /// <summary>現在の条件で釣れる魚のリストを取得</summary>
    public static IReadOnlyList<FishDefinition> GetAvailableFish(Season season, TimePeriod time, int fishingLevel, bool isNearWater = true)
    {
        // 水辺でない場合は魚が釣れない
        if (!isNearWater) return Array.Empty<FishDefinition>();

        return FishList.Where(f =>
            f.MinFishingLevel <= fishingLevel &&
            f.Rarity > 0 &&
            f.ActiveSeasons.Contains(season) &&
            f.ActiveTimes.Contains(time)
        ).ToList();
    }

    /// <summary>釣りの成功率を計算</summary>
    public static float CalculateCatchRate(int fishRarity, int fishingLevel, float luckModifier)
    {
        float baseRate = 0.7f - fishRarity * 0.1f + fishingLevel * 0.03f + luckModifier * 0.1f;
        return Math.Clamp(baseRate, 0.05f, 0.95f);
    }

    /// <summary>ジャンクが釣れる確率を計算</summary>
    public static float CalculateJunkRate(int fishingLevel)
    {
        return Math.Max(0.05f, 0.3f - fishingLevel * 0.025f);
    }

    /// <summary>宝箱が釣れる確率を計算</summary>
    public static float CalculateTreasureRate(int fishingLevel, float luckModifier)
    {
        return Math.Clamp(0.02f + fishingLevel * 0.005f + luckModifier * 0.05f, 0.01f, 0.1f);
    }

    /// <summary>魚をIDで取得</summary>
    public static FishDefinition? GetFishById(string id)
    {
        return FishList.FirstOrDefault(f => f.Id == id);
    }
}
