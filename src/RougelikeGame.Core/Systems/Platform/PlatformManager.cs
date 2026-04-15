namespace RougelikeGame.Core.Systems.Platform;

/// <summary>
/// プラットフォーム管理システム
/// Steam/ローカル等のプラットフォームを自動選択し、実績・クラウドセーブ・統計を統合管理
/// Ver.0.1: 6.5 Steam対応準備
/// </summary>
public class PlatformManager
{
    private IPlatformService _platform;
    private AchievementSystem? _achievementSystem;

    /// <summary>現在のプラットフォームサービス</summary>
    public IPlatformService Platform => _platform;

    /// <summary>プラットフォーム名</summary>
    public string PlatformName => _platform.PlatformName;

    /// <summary>プラットフォームが利用可能か</summary>
    public bool IsAvailable => _platform.IsAvailable;

    public PlatformManager()
    {
        _platform = new LocalPlatformService();
    }

    /// <summary>プラットフォームを初期化（Steamを試行し、失敗時はローカルにフォールバック）</summary>
    public bool Initialize(uint steamAppId = 0)
    {
        // Steam初期化を試行
        var steamService = new SteamPlatformService();
        if (steamService.Initialize(steamAppId))
        {
            _platform = steamService;
            return true;
        }

        // フォールバック: ローカルプラットフォーム
        _platform = new LocalPlatformService();
        _platform.Initialize();
        return true;
    }

    /// <summary>AchievementSystemとの連携を設定</summary>
    public void LinkAchievementSystem(AchievementSystem achievementSystem)
    {
        _achievementSystem = achievementSystem;
    }

    /// <summary>実績を解除（AchievementSystemとプラットフォーム実績の両方を更新）</summary>
    public void UnlockAchievement(string achievementId, int currentTurn = 0)
    {
        // ゲーム内実績システムを更新
        _achievementSystem?.Unlock(achievementId, currentTurn);

        // プラットフォーム実績を更新
        _platform.Achievements.UnlockAchievement(achievementId);
        _platform.Achievements.StoreStats();
    }

    /// <summary>統計を更新</summary>
    public void UpdateStat(string statName, int value)
    {
        _platform.Stats.SetStat(statName, value);
    }

    /// <summary>統計を更新（浮動小数点）</summary>
    public void UpdateStat(string statName, float value)
    {
        _platform.Stats.SetStat(statName, value);
    }

    /// <summary>統計をサーバーに送信</summary>
    public void FlushStats()
    {
        _platform.Stats.StoreStats();
    }

    /// <summary>定期更新処理</summary>
    public void Update()
    {
        _platform.Update();
    }

    /// <summary>シャットダウン</summary>
    public void Shutdown()
    {
        _platform.Stats.StoreStats();
        _platform.Shutdown();
    }

    /// <summary>Steam実績定義のマッピング（ゲーム内ID → Steam API名）</summary>
    public static readonly IReadOnlyDictionary<string, string> SteamAchievementMap =
        new Dictionary<string, string>
        {
            ["first_kill"] = "ACH_FIRST_KILL",
            ["boss_slayer"] = "ACH_BOSS_SLAYER",
            ["kill_100"] = "ACH_KILL_100",
            ["kill_1000"] = "ACH_KILL_1000",
            ["no_damage_boss"] = "ACH_NO_DAMAGE_BOSS",
            ["floor_5"] = "ACH_FLOOR_5",
            ["floor_10"] = "ACH_FLOOR_10",
            ["floor_20"] = "ACH_FLOOR_20",
            ["floor_30"] = "ACH_FLOOR_30",
            ["all_territories"] = "ACH_ALL_TERRITORIES",
            ["collect_10"] = "ACH_COLLECT_10",
            ["collect_50"] = "ACH_COLLECT_50",
            ["legendary_item"] = "ACH_LEGENDARY_ITEM",
            ["encyclopedia_50"] = "ACH_ENCYCLOPEDIA_50",
            ["encyclopedia_monster_complete"] = "ACH_ENCYCLOPEDIA_MONSTER",
            ["encyclopedia_region_complete"] = "ACH_ENCYCLOPEDIA_REGION",
            ["encyclopedia_all_complete"] = "ACH_ENCYCLOPEDIA_ALL",
            ["dungeon_clear"] = "ACH_DUNGEON_CLEAR",
            ["true_ending"] = "ACH_TRUE_ENDING",
            ["speedrun"] = "ACH_SPEEDRUN",
            ["no_death"] = "ACH_NO_DEATH",
            ["pacifist"] = "ACH_PACIFIST",
            ["first_clear"] = "ACH_FIRST_CLEAR",
            ["ng_plus"] = "ACH_NG_PLUS",
            ["all_classes"] = "ACH_ALL_CLASSES",
        };

    /// <summary>Steam統計定義</summary>
    public static readonly IReadOnlyList<string> SteamStatNames = new[]
    {
        "STAT_TOTAL_KILLS",
        "STAT_TOTAL_DEATHS",
        "STAT_TOTAL_TURNS",
        "STAT_DEEPEST_FLOOR",
        "STAT_TOTAL_GOLD_EARNED",
        "STAT_TOTAL_ITEMS_FOUND",
        "STAT_TOTAL_SPELLS_CAST",
        "STAT_TOTAL_BOSSES_KILLED",
        "STAT_TOTAL_QUESTS_COMPLETED",
        "STAT_TOTAL_PLAY_TIME_MINUTES",
    };
}
