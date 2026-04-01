namespace RougelikeGame.Core.Systems;

/// <summary>
/// 身体状態システム - 傷/疲労/清潔度によるステータス変動
/// </summary>
public static class BodyConditionSystem
{
    /// <summary>傷の定義</summary>
    public record WoundDefinition(
        BodyWoundType Type,
        string Name,
        float StrModifier,
        float AgiModifier,
        int HealingTurns
    );

    private static readonly Dictionary<BodyWoundType, WoundDefinition> Wounds = new()
    {
        [BodyWoundType.Cut] = new(BodyWoundType.Cut, "切り傷", -0.05f, -0.03f, 20),
        [BodyWoundType.Bruise] = new(BodyWoundType.Bruise, "打撲", -0.03f, -0.05f, 15),
        [BodyWoundType.Puncture] = new(BodyWoundType.Puncture, "刺し傷", -0.08f, -0.02f, 30),
        [BodyWoundType.Fracture] = new(BodyWoundType.Fracture, "骨折", -0.15f, -0.2f, 60),
        [BodyWoundType.Burn] = new(BodyWoundType.Burn, "火傷", -0.1f, -0.1f, 40),
    };

    /// <summary>傷の定義を取得</summary>
    public static WoundDefinition? GetWound(BodyWoundType type)
    {
        return Wounds.TryGetValue(type, out var w) ? w : null;
    }

    /// <summary>疲労度によるステータス倍率（数値ベース）</summary>
    public static float GetFatigueModifier(FatigueStage stage) => stage switch
    {
        FatigueStage.Fresh => 1.0f,
        FatigueStage.Mild => 0.9f,
        FatigueStage.Tired => 0.75f,
        FatigueStage.Exhausted => 0.5f,
        FatigueStage.Collapse => 0.0f,
        _ => 1.0f
    };

    /// <summary>疲労度によるステータス倍率（旧enum互換）</summary>
    public static float GetFatigueModifier(FatigueLevel level) => level switch
    {
        FatigueLevel.Fresh => 1.0f,
        FatigueLevel.Mild => 0.9f,
        FatigueLevel.Tired => 0.75f,
        FatigueLevel.Exhausted => 0.5f,
        FatigueLevel.Collapse => 0.0f,
        _ => 1.0f
    };

    /// <summary>清潔度による病気感染リスク倍率（数値ベース）</summary>
    public static float GetHygieneInfectionRisk(HygieneStage stage) => stage switch
    {
        HygieneStage.Clean => 0.5f,
        HygieneStage.Normal => 1.0f,
        HygieneStage.Dirty => 1.5f,
        HygieneStage.Filthy => 2.5f,
        HygieneStage.Foul => 4.0f,
        _ => 1.0f
    };

    /// <summary>清潔度による病気感染リスク倍率（旧enum互換）</summary>
    public static float GetHygieneInfectionRisk(HygieneLevel level) => level switch
    {
        HygieneLevel.Clean => 0.5f,
        HygieneLevel.Normal => 1.0f,
        HygieneLevel.Dirty => 1.5f,
        HygieneLevel.Filthy => 2.5f,
        HygieneLevel.Foul => 4.0f,
        _ => 1.0f
    };

    /// <summary>疲労度名を取得（数値ベース）</summary>
    public static string GetFatigueName(FatigueStage stage) => stage switch
    {
        FatigueStage.Fresh => "元気",
        FatigueStage.Mild => "軽疲労",
        FatigueStage.Tired => "疲労",
        FatigueStage.Exhausted => "重疲労",
        FatigueStage.Collapse => "過労",
        _ => "不明"
    };

    /// <summary>疲労度名を取得（旧enum互換）</summary>
    public static string GetFatigueName(FatigueLevel level) => level switch
    {
        FatigueLevel.Fresh => "元気",
        FatigueLevel.Mild => "軽疲労",
        FatigueLevel.Tired => "疲労",
        FatigueLevel.Exhausted => "重疲労",
        FatigueLevel.Collapse => "過労",
        _ => "不明"
    };

    /// <summary>清潔度名を取得（数値ベース）</summary>
    public static string GetHygieneName(HygieneStage stage) => stage switch
    {
        HygieneStage.Clean => "清潔",
        HygieneStage.Normal => "普通",
        HygieneStage.Dirty => "汚れ",
        HygieneStage.Filthy => "不衛生",
        HygieneStage.Foul => "不潔",
        _ => "不明"
    };

    /// <summary>清潔度名を取得（旧enum互換）</summary>
    public static string GetHygieneName(HygieneLevel level) => level switch
    {
        HygieneLevel.Clean => "清潔",
        HygieneLevel.Normal => "普通",
        HygieneLevel.Dirty => "汚れ",
        HygieneLevel.Filthy => "不衛生",
        HygieneLevel.Foul => "不潔",
        _ => "不明"
    };
}
