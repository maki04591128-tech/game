namespace RougelikeGame.Core.Systems;

/// <summary>
/// 季節ごとの環境効果
/// </summary>
public record SeasonEffect(
    Season Season,
    string Name,
    float SightModifier,
    float StaminaCostModifier,
    IReadOnlyList<MonsterRace> IncreasedRaces,
    IReadOnlyList<MonsterRace> DecreasedRaces,
    float GatheringModifier,
    float FoodAvailability,
    string Description
);

/// <summary>
/// 季節システム - 季節による環境効果・敵変動・アイテム入手性の管理
/// </summary>
public static class SeasonSystem
{
    private static readonly Dictionary<Season, SeasonEffect> Effects = new()
    {
        [Season.Spring] = new SeasonEffect(
            Season.Spring, "春",
            SightModifier: 1.05f,
            StaminaCostModifier: 1.0f,
            IncreasedRaces: new[] { MonsterRace.Insect, MonsterRace.Plant },
            DecreasedRaces: new[] { MonsterRace.Undead },
            GatheringModifier: 1.3f,
            FoodAvailability: 1.2f,
            "草花が増え視界やや良好。昆虫・植物系が活発"
        ),
        [Season.Summer] = new SeasonEffect(
            Season.Summer, "夏",
            SightModifier: 1.1f,
            StaminaCostModifier: 1.15f,
            IncreasedRaces: new[] { MonsterRace.Beast, MonsterRace.Demon },
            DecreasedRaces: new[] { MonsterRace.Spirit },
            GatheringModifier: 1.1f,
            FoodAvailability: 1.3f,
            "日照長く視界良好だが暑さでスタミナ消費増。獣系活発"
        ),
        [Season.Autumn] = new SeasonEffect(
            Season.Autumn, "秋",
            SightModifier: 1.0f,
            StaminaCostModifier: 1.0f,
            IncreasedRaces: Array.Empty<MonsterRace>(),
            DecreasedRaces: Array.Empty<MonsterRace>(),
            GatheringModifier: 1.5f,
            FoodAvailability: 1.5f,
            "収穫の季節。食料豊富でバランス良好"
        ),
        [Season.Winter] = new SeasonEffect(
            Season.Winter, "冬",
            SightModifier: 0.8f,
            StaminaCostModifier: 1.3f,
            IncreasedRaces: new[] { MonsterRace.Undead, MonsterRace.Spirit },
            DecreasedRaces: new[] { MonsterRace.Insect, MonsterRace.Plant },
            GatheringModifier: 0.5f,
            FoodAvailability: 0.6f,
            "雪と凍結で視界悪化、スタミナ消費増。不死系増加"
        )
    };

    /// <summary>月から季節を取得</summary>
    public static Season GetSeason(int month) => month switch
    {
        >= 3 and <= 5 => Season.Spring,
        >= 6 and <= 8 => Season.Summer,
        >= 9 and <= 11 => Season.Autumn,
        _ => Season.Winter  // 12, 1, 2
    };

    /// <summary>季節の環境効果を取得</summary>
    public static SeasonEffect GetEffect(Season season)
    {
        return Effects[season];
    }

    /// <summary>季節名を取得</summary>
    public static string GetSeasonName(Season season) => Effects[season].Name;

    /// <summary>季節による視界補正を取得</summary>
    public static float GetSightModifier(Season season) => Effects[season].SightModifier;

    /// <summary>季節によるスタミナコスト倍率を取得</summary>
    public static float GetStaminaCostModifier(Season season) => Effects[season].StaminaCostModifier;

    /// <summary>指定種族が季節で活発かどうか</summary>
    public static bool IsRaceActive(Season season, MonsterRace race)
    {
        return Effects[season].IncreasedRaces.Contains(race);
    }

    /// <summary>指定種族が季節で不活発かどうか</summary>
    public static bool IsRaceInactive(Season season, MonsterRace race)
    {
        return Effects[season].DecreasedRaces.Contains(race);
    }

    /// <summary>季節による採取倍率を取得</summary>
    public static float GetGatheringModifier(Season season) => Effects[season].GatheringModifier;

    /// <summary>季節による食料入手性倍率を取得</summary>
    public static float GetFoodAvailability(Season season) => Effects[season].FoodAvailability;

    /// <summary>天候の発生確率テーブル（季節連動）</summary>
    public static IReadOnlyDictionary<Weather, float> GetWeatherProbabilities(Season season) => season switch
    {
        Season.Spring => new Dictionary<Weather, float>
        {
            [Weather.Clear] = 0.4f, [Weather.Rain] = 0.35f, [Weather.Fog] = 0.15f,
            [Weather.Snow] = 0.0f, [Weather.Storm] = 0.1f
        },
        Season.Summer => new Dictionary<Weather, float>
        {
            [Weather.Clear] = 0.6f, [Weather.Rain] = 0.15f, [Weather.Fog] = 0.05f,
            [Weather.Snow] = 0.0f, [Weather.Storm] = 0.2f
        },
        Season.Autumn => new Dictionary<Weather, float>
        {
            [Weather.Clear] = 0.35f, [Weather.Rain] = 0.3f, [Weather.Fog] = 0.25f,
            [Weather.Snow] = 0.0f, [Weather.Storm] = 0.1f
        },
        Season.Winter => new Dictionary<Weather, float>
        {
            [Weather.Clear] = 0.2f, [Weather.Rain] = 0.1f, [Weather.Fog] = 0.15f,
            [Weather.Snow] = 0.45f, [Weather.Storm] = 0.1f
        },
        _ => new Dictionary<Weather, float> { [Weather.Clear] = 1.0f }
    };
}
