namespace RougelikeGame.Core.Systems;

/// <summary>
/// 飢餓・渇きシステム - 渇き管理と水源品質
/// </summary>
public static class ThirstSystem
{
    /// <summary>渇き段階によるステータス影響（数値ベース）</summary>
    public static (float StrMod, float AgiMod, float IntMod) GetThirstModifiers(ThirstStage stage) => stage switch
    {
        ThirstStage.Nausea => (0.7f, 0.7f, 0.7f),
        ThirstStage.Overdrinking => (0.9f, 0.9f, 0.9f),
        ThirstStage.Full => (1.0f, 1.0f, 1.0f),
        ThirstStage.Normal => (1.0f, 1.0f, 1.0f),
        ThirstStage.SlightlyThirsty => (0.95f, 0.95f, 0.95f),
        ThirstStage.VeryThirsty => (0.9f, 0.9f, 0.85f),
        ThirstStage.Dehydrated => (0.6f, 0.6f, 0.6f),
        ThirstStage.NearDesiccation => (0.2f, 0.2f, 0.3f),
        ThirstStage.Desiccation => (0f, 0f, 0f),
        _ => (1.0f, 1.0f, 1.0f)
    };

    /// <summary>渇き段階によるステータス影響（旧enum互換）</summary>
    [Obsolete("GetThirstModifiers(ThirstStage)を使用してください。")]
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

    /// <summary>渇き段階名を取得（数値ベース）</summary>
    public static string GetThirstName(ThirstStage stage) => stage switch
    {
        ThirstStage.Nausea => "吐き気",
        ThirstStage.Overdrinking => "過飲",
        ThirstStage.Full => "満腹",
        ThirstStage.Normal => "通常",
        ThirstStage.SlightlyThirsty => "渇き（小）",
        ThirstStage.VeryThirsty => "渇き（大）",
        ThirstStage.Dehydrated => "脱水",
        ThirstStage.NearDesiccation => "干死寸前",
        ThirstStage.Desiccation => "干死",
        _ => "不明"
    };

    /// <summary>渇き段階名を取得（旧enum互換）</summary>
    [Obsolete("GetThirstName(ThirstStage)を使用してください。")]
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

    /// <summary>渇きダメージ（ターンあたり、数値ベース）</summary>
    public static int GetThirstDamage(ThirstStage stage) => stage switch
    {
        ThirstStage.Dehydrated => 1,
        ThirstStage.NearDesiccation => 10,
        _ => 0
    };

    /// <summary>渇きダメージ（ターンあたり、旧enum互換）</summary>
    [Obsolete("GetThirstDamage(ThirstStage)を使用してください。")]
    public static int GetThirstDamage(ThirstLevel level) => level switch
    {
        ThirstLevel.Dehydrated => 1,
        ThirstLevel.SevereDehydration => 2,  // S-1: ThirstStageと統一（3→2）
        _ => 0
    };

    /// <summary>渇き段階による行動コスト加算値を取得</summary>
    public static int GetThirstActionCostBonus(ThirstStage stage) => stage switch
    {
        ThirstStage.Nausea => 3,
        ThirstStage.Overdrinking => 2,
        ThirstStage.Full => 1,
        ThirstStage.VeryThirsty => 1,
        ThirstStage.Dehydrated => 2,
        _ => 0
    };

    /// <summary>渇き段階による行動不可確率を取得</summary>
    public static float GetThirstActionBlockChance(ThirstStage stage) => stage switch
    {
        ThirstStage.Nausea => 0.3f,
        _ => 0f
    };

    /// <summary>渇き（小）の30%確率行動コスト+1判定</summary>
    public static int GetThirstSlightPenaltyCostBonus(ThirstStage stage, Random? random = null) => stage switch
    {
        ThirstStage.SlightlyThirsty => (random ?? Random.Shared).Next(100) < 30 ? 1 : 0,
        _ => 0
    };
}
