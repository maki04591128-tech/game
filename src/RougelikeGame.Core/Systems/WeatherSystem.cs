namespace RougelikeGame.Core.Systems;

/// <summary>
/// 天候効果の定義
/// </summary>
public record WeatherEffect(
    Weather Weather,
    string Name,
    float SightModifier,
    float FireDamageModifier,
    float IceDamageModifier,
    float LightningDamageModifier,
    float RangedHitModifier,
    float MovementCostModifier,
    bool TracksErased,
    string Description
);

/// <summary>
/// 天候システム - 天候による戦闘/移動への影響を管理
/// </summary>
public static class WeatherSystem
{
    private static readonly Dictionary<Weather, WeatherEffect> Effects = new()
    {
        [Weather.Clear] = new WeatherEffect(
            Weather.Clear, "晴れ",
            SightModifier: 1.0f,
            FireDamageModifier: 1.0f,
            IceDamageModifier: 1.0f,
            LightningDamageModifier: 1.0f,
            RangedHitModifier: 0.05f,
            MovementCostModifier: 1.0f,
            TracksErased: false,
            "視界良好。弓命中率+5%"
        ),
        [Weather.Rain] = new WeatherEffect(
            Weather.Rain, "雨",
            SightModifier: 0.8f,
            FireDamageModifier: 0.8f,
            IceDamageModifier: 1.0f,
            LightningDamageModifier: 1.2f,
            RangedHitModifier: -0.1f,
            MovementCostModifier: 1.1f,
            TracksErased: true,
            "火属性ダメージ-20%、雷属性ダメージ+20%、足跡消去"
        ),
        [Weather.Fog] = new WeatherEffect(
            Weather.Fog, "霧",
            SightModifier: 0.5f,
            FireDamageModifier: 1.0f,
            IceDamageModifier: 1.0f,
            LightningDamageModifier: 1.0f,
            RangedHitModifier: -0.2f,
            MovementCostModifier: 1.0f,
            TracksErased: false,
            "視界半減、奇襲率↑"
        ),
        [Weather.Snow] = new WeatherEffect(
            Weather.Snow, "雪",
            SightModifier: 0.7f,
            FireDamageModifier: 0.9f,
            IceDamageModifier: 1.2f,
            LightningDamageModifier: 1.0f,
            RangedHitModifier: -0.1f,
            MovementCostModifier: 1.5f,
            TracksErased: false,
            "移動コスト+50%、氷属性ダメージ+20%"
        ),
        [Weather.Storm] = new WeatherEffect(
            Weather.Storm, "嵐",
            SightModifier: 0.4f,
            FireDamageModifier: 0.7f,
            IceDamageModifier: 1.0f,
            LightningDamageModifier: 1.5f,
            RangedHitModifier: -0.3f,
            MovementCostModifier: 1.3f,
            TracksErased: true,
            "遠距離攻撃命中率-30%、雷攻撃ランダム発生"
        )
    };

    /// <summary>天候の効果を取得</summary>
    public static WeatherEffect GetEffect(Weather weather) => Effects[weather];

    /// <summary>天候名を取得</summary>
    public static string GetWeatherName(Weather weather) => Effects[weather].Name;

    /// <summary>天候による視界補正を取得</summary>
    public static float GetSightModifier(Weather weather) => Effects[weather].SightModifier;

    /// <summary>天候による属性ダメージ補正を取得</summary>
    public static float GetElementDamageModifier(Weather weather, Element element) => element switch
    {
        Element.Fire => Effects[weather].FireDamageModifier,
        Element.Ice => Effects[weather].IceDamageModifier,
        Element.Lightning => Effects[weather].LightningDamageModifier,
        _ => 1.0f
    };

    /// <summary>天候による遠距離命中補正を取得</summary>
    public static float GetRangedHitModifier(Weather weather) => Effects[weather].RangedHitModifier;

    /// <summary>天候による移動コスト倍率を取得</summary>
    public static float GetMovementCostModifier(Weather weather) => Effects[weather].MovementCostModifier;

    /// <summary>天候で足跡が消えるか</summary>
    public static bool AreTracksErased(Weather weather) => Effects[weather].TracksErased;

    /// <summary>季節と乱数値から天候を決定</summary>
    public static Weather DetermineWeather(Season season, double randomValue)
    {
        var probs = SeasonSystem.GetWeatherProbabilities(season);
        double cumulative = 0;
        foreach (var (weather, prob) in probs)
        {
            cumulative += prob;
            if (randomValue < cumulative)
                return weather;
        }
        return Weather.Clear;
    }

    /// <summary>ダンジョン内では天候影響なし（屋外のみ）</summary>
    public static bool IsWeatherApplicable(bool isOutdoor) => isOutdoor;
}
