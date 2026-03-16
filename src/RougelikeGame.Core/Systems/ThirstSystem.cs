namespace RougelikeGame.Core.Systems;

/// <summary>
/// 飢餓・渇きシステム - 渇き管理と水源品質
/// </summary>
public static class ThirstSystem
{
    /// <summary>渇き段階によるステータス影響</summary>
    public static (float StrMod, float AgiMod, float IntMod) GetThirstModifiers(ThirstLevel level) => level switch
    {
        ThirstLevel.Hydrated => (1.0f, 1.0f, 1.0f),
        ThirstLevel.Thirsty => (0.9f, 0.9f, 0.95f),
        ThirstLevel.Dehydrated => (0.7f, 0.7f, 0.8f),
        ThirstLevel.SevereDehydration => (0.4f, 0.4f, 0.5f),
        _ => (1.0f, 1.0f, 1.0f)
    };

    /// <summary>水源品質による感染リスク</summary>
    public static float GetInfectionRisk(WaterQuality quality) => quality switch
    {
        WaterQuality.Pure => 0.0f,
        WaterQuality.River => 0.05f,
        WaterQuality.Muddy => 0.2f,
        WaterQuality.Polluted => 0.5f,
        _ => 0.1f
    };

    /// <summary>渇き段階名を取得</summary>
    public static string GetThirstName(ThirstLevel level) => level switch
    {
        ThirstLevel.Hydrated => "潤い",
        ThirstLevel.Thirsty => "渇き",
        ThirstLevel.Dehydrated => "脱水",
        ThirstLevel.SevereDehydration => "重度脱水",
        _ => "不明"
    };

    /// <summary>水源品質名を取得</summary>
    public static string GetWaterQualityName(WaterQuality quality) => quality switch
    {
        WaterQuality.Pure => "清水",
        WaterQuality.River => "川水",
        WaterQuality.Muddy => "泥水",
        WaterQuality.Polluted => "汚水",
        _ => "不明"
    };

    /// <summary>浄化後の水質を取得</summary>
    public static WaterQuality Purify(WaterQuality quality) => quality switch
    {
        WaterQuality.Polluted => WaterQuality.Muddy,
        WaterQuality.Muddy => WaterQuality.River,
        WaterQuality.River => WaterQuality.Pure,
        _ => quality
    };

    /// <summary>渇きダメージ（ターンあたり）</summary>
    public static int GetThirstDamage(ThirstLevel level) => level switch
    {
        ThirstLevel.Dehydrated => 1,
        ThirstLevel.SevereDehydration => 3,
        _ => 0
    };
}
