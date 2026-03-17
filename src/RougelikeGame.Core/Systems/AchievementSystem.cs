namespace RougelikeGame.Core.Systems;

/// <summary>
/// 実績・トロフィーシステム - メタ進行実績、永続記録、次回プレイボーナス
/// 参考: Hades（功績鏡）、Slay the Spire、Risk of Rain 2
/// </summary>
public class AchievementSystem
{
    /// <summary>実績定義</summary>
    public record Achievement(
        string AchievementId,
        string Name,
        string Description,
        AchievementCategory Category,
        bool IsUnlocked,
        int UnlockTurn,
        string? BonusEffect
    );

    /// <summary>実績解除結果</summary>
    public record UnlockResult(
        bool IsNewUnlock,
        string AchievementId,
        string Name,
        string Message
    );

    private readonly Dictionary<string, Achievement> _achievements = new();

    /// <summary>全実績</summary>
    public IReadOnlyDictionary<string, Achievement> Achievements => _achievements;

    /// <summary>解除済み実績数</summary>
    public int UnlockedCount => _achievements.Values.Count(a => a.IsUnlocked);

    /// <summary>実績総数</summary>
    public int TotalCount => _achievements.Count;

    /// <summary>達成率（0.0〜1.0）</summary>
    public float CompletionRate => TotalCount == 0 ? 0f : (float)UnlockedCount / TotalCount;

    /// <summary>実績を登録</summary>
    public void Register(string id, string name, string description, AchievementCategory category, string? bonusEffect = null)
    {
        _achievements[id] = new Achievement(id, name, description, category, false, 0, bonusEffect);
    }

    /// <summary>実績を解除</summary>
    public UnlockResult Unlock(string achievementId, int currentTurn = 0)
    {
        if (!_achievements.TryGetValue(achievementId, out var achievement))
            return new UnlockResult(false, achievementId, "", "実績が見つかりません");

        if (achievement.IsUnlocked)
            return new UnlockResult(false, achievementId, achievement.Name, "既に解除済みです");

        _achievements[achievementId] = achievement with { IsUnlocked = true, UnlockTurn = currentTurn };
        return new UnlockResult(true, achievementId, achievement.Name,
            $"🏆 実績解除: {achievement.Name}");
    }

    /// <summary>実績が解除済みか確認</summary>
    public bool IsUnlocked(string achievementId)
    {
        return _achievements.TryGetValue(achievementId, out var a) && a.IsUnlocked;
    }

    /// <summary>カテゴリ別の実績一覧を取得</summary>
    public IReadOnlyList<Achievement> GetByCategory(AchievementCategory category)
    {
        return _achievements.Values.Where(a => a.Category == category).ToList();
    }

    /// <summary>解除済み実績一覧を取得</summary>
    public IReadOnlyList<Achievement> GetUnlocked()
    {
        return _achievements.Values.Where(a => a.IsUnlocked).ToList();
    }

    /// <summary>次回プレイボーナスを計算（解除実績数に応じた初期ボーナス）</summary>
    public int CalculateNextPlayBonus()
    {
        int bonus = 0;
        foreach (var achievement in _achievements.Values.Where(a => a.IsUnlocked && a.BonusEffect != null))
        {
            bonus += achievement.BonusEffect switch
            {
                "stat_boost_small" => 1,
                "stat_boost_medium" => 3,
                "stat_boost_large" => 5,
                "gold_bonus" => 50,
                "item_bonus" => 1,
                "exp_multiplier" => 10,
                _ => 0
            };
        }
        return bonus;
    }

    /// <summary>カテゴリ別達成率を取得</summary>
    public float GetCategoryCompletionRate(AchievementCategory category)
    {
        var categoryAchievements = _achievements.Values.Where(a => a.Category == category).ToList();
        if (categoryAchievements.Count == 0) return 0f;
        return (float)categoryAchievements.Count(a => a.IsUnlocked) / categoryAchievements.Count;
    }

    /// <summary>カテゴリ名を取得</summary>
    public static string GetCategoryName(AchievementCategory category) => category switch
    {
        AchievementCategory.Combat => "戦闘",
        AchievementCategory.Exploration => "探索",
        AchievementCategory.Collection => "収集",
        AchievementCategory.Story => "ストーリー",
        AchievementCategory.Challenge => "チャレンジ",
        AchievementCategory.Meta => "メタ",
        _ => "不明"
    };
}
