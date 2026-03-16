namespace RougelikeGame.Core.Systems;

/// <summary>
/// 睡眠・野営システム - 休息と回復の管理
/// </summary>
public static class RestSystem
{
    /// <summary>睡眠の質による回復量テーブル</summary>
    public static (float HpRecovery, float MpRecovery, float FatigueRecovery, float SanityRecovery) GetRecoveryRates(SleepQuality quality) => quality switch
    {
        SleepQuality.DeepSleep => (0.5f, 0.5f, 0.8f, 0.2f),
        SleepQuality.Normal => (0.3f, 0.3f, 0.6f, 0.1f),
        SleepQuality.Light => (0.15f, 0.15f, 0.3f, 0.05f),
        SleepQuality.Nap => (0.05f, 0.1f, 0.15f, 0.0f),
        _ => (0.1f, 0.1f, 0.2f, 0.05f)
    };

    /// <summary>睡眠に必要なターン数</summary>
    public static int GetSleepDuration(SleepQuality quality) => quality switch
    {
        SleepQuality.DeepSleep => 50,
        SleepQuality.Normal => 35,
        SleepQuality.Light => 20,
        SleepQuality.Nap => 10,
        _ => 20
    };

    /// <summary>野営可能かどうか判定</summary>
    public static bool CanCamp(bool isIndoor, bool hasEnemyNearby, int floorDepth)
    {
        if (hasEnemyNearby) return false;
        if (!isIndoor && floorDepth == 0) return true; // 地上屋外は可能
        if (isIndoor && floorDepth <= 3) return true;  // 浅い階層は可能
        return false;
    }

    /// <summary>野営中の襲撃確率を計算</summary>
    public static float CalculateAmbushChance(int floorDepth, bool hasCampfire, bool hasGuard)
    {
        float chance = 0.1f + floorDepth * 0.02f;
        if (hasCampfire) chance -= 0.05f;
        if (hasGuard) chance -= 0.1f;
        return Math.Clamp(chance, 0.02f, 0.5f);
    }

    /// <summary>睡眠の質名を取得</summary>
    public static string GetQualityName(SleepQuality quality) => quality switch
    {
        SleepQuality.DeepSleep => "熟睡",
        SleepQuality.Normal => "普通",
        SleepQuality.Light => "浅い眠り",
        SleepQuality.Nap => "仮眠",
        _ => "不明"
    };

    /// <summary>宿屋での睡眠コストを計算</summary>
    public static int CalculateInnCost(SleepQuality quality) => quality switch
    {
        SleepQuality.DeepSleep => 100,
        SleepQuality.Normal => 50,
        SleepQuality.Light => 25,
        SleepQuality.Nap => 10,
        _ => 50
    };
}
