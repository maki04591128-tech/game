namespace RougelikeGame.Core.Systems;

/// <summary>
/// ミミック・偽装オブジェクトシステム
/// </summary>
public static class MimicSystem
{
    /// <summary>ミミック出現率を計算（階層依存）</summary>
    public static float CalculateMimicSpawnRate(int floorDepth)
    {
        return Math.Clamp(0.02f + floorDepth * 0.005f, 0.02f, 0.15f);
    }

    /// <summary>ミミック看破率を計算（正気度/鑑定依存）</summary>
    public static float CalculateDetectionRate(int perception, int sanity, bool hasAppraisalSkill)
    {
        float baseRate = 0.1f + perception * 0.03f;
        if (sanity < 40) baseRate -= 0.15f;  // 低正気度で看破困難
        if (hasAppraisalSkill) baseRate += 0.3f;
        return Math.Clamp(baseRate, 0.05f, 0.9f);
    }

    /// <summary>ミミックの強さ倍率（偽装対象アイテムの等級依存）</summary>
    public static float GetMimicStrengthMultiplier(ItemGrade disguisedItemGrade) => disguisedItemGrade switch
    {
        ItemGrade.Crude => 0.5f,
        ItemGrade.Cheap => 0.8f,
        ItemGrade.Standard => 1.0f,
        ItemGrade.Fine => 1.3f,
        ItemGrade.Superior => 1.6f,
        ItemGrade.Masterwork => 2.0f,
        _ => 1.0f
    };

    /// <summary>ミミック撃破報酬の追加倍率</summary>
    public static float GetMimicRewardMultiplier() => 1.5f;

    /// <summary>偽装タイプの一覧</summary>
    public static IReadOnlyList<string> GetDisguiseTypes() => new[]
    {
        "宝箱", "扉", "階段", "石像", "祭壇", "商人"
    };
}
