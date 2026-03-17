namespace RougelikeGame.Core.Systems;

/// <summary>
/// 無限ダンジョンシステム - エンドコンテンツの深層チャレンジ
/// </summary>
public static class InfiniteDungeonSystem
{
    /// <summary>無限ダンジョンのスコア記録</summary>
    public record InfiniteDungeonScore(
        int MaxFloorReached,
        int TotalKills,
        int TotalTurns,
        string Rank
    );

    /// <summary>階層帯を判定</summary>
    public static InfiniteDungeonTier GetTier(int floor) => floor switch
    {
        <= 10 => InfiniteDungeonTier.Normal,
        <= 30 => InfiniteDungeonTier.Advanced,
        <= 50 => InfiniteDungeonTier.Deep,
        _ => InfiniteDungeonTier.Abyss
    };

    /// <summary>敵レベルスケーリングを計算</summary>
    public static int CalculateEnemyLevel(int floor)
    {
        return 10 + floor * 2;
    }

    /// <summary>ボス出現階か判定（10階ごと）</summary>
    public static bool IsBossFloor(int floor) => floor > 0 && floor % 10 == 0;

    /// <summary>ドロップ率倍率を計算</summary>
    public static float GetDropRateMultiplier(int floor)
    {
        return 1.0f + floor * 0.05f;
    }

    /// <summary>階層帯名を取得</summary>
    public static string GetTierName(InfiniteDungeonTier tier) => tier switch
    {
        InfiniteDungeonTier.Normal => "通常帯",
        InfiniteDungeonTier.Advanced => "上級帯",
        InfiniteDungeonTier.Deep => "深層帯",
        InfiniteDungeonTier.Abyss => "魔界帯",
        _ => "不明"
    };

    /// <summary>解放条件（メインストーリークリア必須）</summary>
    public static bool IsUnlocked(bool hasCompletedMainStory) => hasCompletedMainStory;

    /// <summary>経験値倍率を計算</summary>
    public static float GetExpMultiplier(int floor)
    {
        return 1.0f + floor * 0.03f;
    }

    /// <summary>スコアを計算</summary>
    public static InfiniteDungeonScore CalculateScore(int maxFloor, int totalKills, int totalTurns)
    {
        string rank = maxFloor switch
        {
            >= 100 => "SSS",
            >= 50 => "SS",
            >= 30 => "S",
            >= 20 => "A",
            >= 10 => "B",
            >= 5 => "C",
            _ => "D"
        };
        return new InfiniteDungeonScore(maxFloor, totalKills, totalTurns, rank);
    }

    /// <summary>解放メッセージを取得</summary>
    public static string GetUnlockMessage()
    {
        return "🏆 無限ダンジョンが解放された！ タイトル画面から挑戦できる";
    }

    /// <summary>階層別の難易度説明を取得</summary>
    public static string GetFloorDescription(int floor)
    {
        var tier = GetTier(floor);
        return $"無限ダンジョン 第{floor}層 [{GetTierName(tier)}]";
    }
}
