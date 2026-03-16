namespace RougelikeGame.Core.Systems;

/// <summary>
/// 能力値フラグ判定結果
/// </summary>
public record StatFlagResult(StatFlag Flag, string Name, string Description, bool IsActive);

/// <summary>
/// 能力値フラグシステム - 能力値閾値に応じたフラグ解放を管理
/// </summary>
public static class StatFlagSystem
{
    /// <summary>
    /// 能力値フラグの閾値定義
    /// </summary>
    private static readonly (StatFlag Flag, string StatName, int Threshold, string Name, string Description)[] FlagDefinitions =
    {
        (StatFlag.Herculean,   "STR", 25, "怪力",   "岩の障害物を破壊可能、力自慢NPC挑戦イベント"),
        (StatFlag.Erudite,     "INT", 25, "博識",   "古文書解読、賢者NPC専用会話"),
        (StatFlag.EagleEye,    "PER", 25, "鷹の目", "隠しドア自動発見、遠距離の敵察知"),
        (StatFlag.FleetFooted, "AGI", 25, "韋駄天", "逃走確率100%、特殊ルート解放"),
        (StatFlag.Charismatic, "CHA", 20, "魅力的", "ショップ追加値引、NPC好感度初期値↑"),
        (StatFlag.Lucky,       "LUK", 20, "強運",   "レアドロップ率↑、カジノ特殊イベント"),
        (StatFlag.Robust,      "VIT", 25, "頑健",   "状態異常耐性↑、スタミナ上限↑"),
        (StatFlag.Dexterous,   "DEX", 25, "神業",   "クリティカル率↑、罠解除成功率↑"),
        (StatFlag.SteadyMind,  "MND", 25, "精神力", "MP回復速度↑、恐怖・混乱耐性"),
    };

    /// <summary>
    /// 能力値辞書から有効な全フラグを判定
    /// </summary>
    public static IReadOnlyList<StatFlagResult> EvaluateAll(IReadOnlyDictionary<string, int> stats)
    {
        var results = new List<StatFlagResult>();
        foreach (var (flag, statName, threshold, name, desc) in FlagDefinitions)
        {
            bool active = stats.TryGetValue(statName, out int value) && value >= threshold;
            results.Add(new StatFlagResult(flag, name, desc, active));
        }
        return results;
    }

    /// <summary>
    /// 特定の能力値フラグが有効か判定
    /// </summary>
    public static bool IsActive(StatFlag flag, IReadOnlyDictionary<string, int> stats)
    {
        foreach (var (f, statName, threshold, _, _) in FlagDefinitions)
        {
            if (f == flag)
                return stats.TryGetValue(statName, out int value) && value >= threshold;
        }
        return false;
    }

    /// <summary>
    /// フラグ名を取得
    /// </summary>
    public static string GetFlagName(StatFlag flag)
    {
        foreach (var (f, _, _, name, _) in FlagDefinitions)
        {
            if (f == flag) return name;
        }
        return "不明";
    }

    /// <summary>
    /// フラグの閾値情報を取得
    /// </summary>
    public static (string StatName, int Threshold)? GetThreshold(StatFlag flag)
    {
        foreach (var (f, statName, threshold, _, _) in FlagDefinitions)
        {
            if (f == flag) return (statName, threshold);
        }
        return null;
    }

    /// <summary>
    /// 全フラグ定義を取得
    /// </summary>
    public static IReadOnlyList<(StatFlag Flag, string StatName, int Threshold, string Name, string Description)> GetAllDefinitions()
    {
        return FlagDefinitions;
    }
}
