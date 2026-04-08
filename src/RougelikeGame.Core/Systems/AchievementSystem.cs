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

    /// <summary>デフォルト実績を一括登録 (AD-1)</summary>
    public void RegisterDefaults()
    {
        // 戦闘系
        Register("first_kill", "初めての勝利", "敵を初めて倒した", AchievementCategory.Combat);
        Register("boss_slayer", "ボスキラー", "ボスを初めて倒した", AchievementCategory.Combat);
        Register("kill_100", "百人斬り", "敵を100体倒した", AchievementCategory.Combat);
        Register("kill_1000", "千人斬り", "敵を1000体倒した", AchievementCategory.Combat, "attack_bonus_1");
        Register("no_damage_boss", "無傷のボス戦", "ボスをノーダメージで倒した", AchievementCategory.Challenge);
        // 探索系
        Register("floor_5", "地下5階到達", "ダンジョン5階に到達した", AchievementCategory.Exploration);
        Register("floor_10", "地下10階到達", "ダンジョン10階に到達した", AchievementCategory.Exploration);
        Register("floor_20", "深淵探索者", "ダンジョン20階に到達した", AchievementCategory.Exploration);
        Register("floor_30", "最深部到達", "ダンジョン最深部に到達した", AchievementCategory.Exploration, "unlock_infinite_dungeon");
        Register("all_territories", "世界踏破", "全ての領地を訪問した", AchievementCategory.Exploration, "wanderer_ending");
        // 収集系
        Register("collect_10", "駆け出し収集家", "10種類のアイテムを入手した", AchievementCategory.Collection);
        Register("collect_50", "収集家", "50種類のアイテムを入手した", AchievementCategory.Collection);
        Register("legendary_item", "伝説の発見", "伝説級アイテムを入手した", AchievementCategory.Collection);
        Register("encyclopedia_50", "博識", "図鑑を50%埋めた", AchievementCategory.Collection);
        Register("encyclopedia_monster_complete", "モンスター研究家", "モンスター図鑑を全て完全開示した（ダメージ+5%）", AchievementCategory.Collection);
        Register("encyclopedia_region_complete", "地理学者", "地域図鑑を全て完全開示した（ショップ割引10%）", AchievementCategory.Collection);
        Register("encyclopedia_all_complete", "博物学者", "全図鑑を完全開示した（称号「博物学者」獲得）", AchievementCategory.Collection);
        // ストーリー系
        Register("dungeon_clear", "ダンジョンクリア", "ダンジョンを攻略した", AchievementCategory.Story);
        Register("true_ending", "真のエンディング", "真エンディングに到達した", AchievementCategory.Story);
        // チャレンジ系
        Register("speedrun", "スピードランナー", "1000ターン以内にクリアした", AchievementCategory.Challenge);
        Register("no_death", "不死身", "一度も死なずにクリアした", AchievementCategory.Challenge, "bonus_hp_10");
        Register("pacifist", "平和主義者", "最小限の敵だけ倒してクリアした", AchievementCategory.Challenge);
        // メタ系
        Register("first_clear", "初回クリア", "ゲームを初めてクリアした", AchievementCategory.Meta);
        Register("ng_plus", "周回プレイ", "ニューゲーム+を開始した", AchievementCategory.Meta, "ng_plus_bonus");
        Register("all_classes", "全職制覇", "全職業でクリアした", AchievementCategory.Meta, "master_bonus");
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

    /// <summary>BU-11: 解除済み実績IDのリストを取得（セーブ用）</summary>
    public List<string> GetUnlockedIds()
    {
        return _achievements.Values.Where(a => a.IsUnlocked).Select(a => a.AchievementId).ToList();
    }

    /// <summary>BU-11: セーブデータから解除済み実績を復元</summary>
    public void RestoreUnlocked(IEnumerable<string> unlockedIds)
    {
        foreach (var id in unlockedIds)
        {
            if (_achievements.TryGetValue(id, out var achievement) && !achievement.IsUnlocked)
            {
                _achievements[id] = achievement with { IsUnlocked = true };
            }
        }
    }
}
