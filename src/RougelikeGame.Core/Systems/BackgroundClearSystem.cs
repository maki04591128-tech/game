namespace RougelikeGame.Core.Systems;

/// <summary>
/// 素性クリア条件の追跡と判定を行うシステム
/// </summary>
public class BackgroundClearSystem
{
    private readonly Dictionary<string, int> _intFlags = new();
    private readonly HashSet<string> _boolFlags = new();

    /// <summary>素性のクリア条件フラグ名を取得</summary>
    public static string GetClearFlag(Background background) =>
        BackgroundBonusData.Get(background).ClearConditionFlag;

    /// <summary>素性のクリア条件説明を取得</summary>
    public static string GetClearDescription(Background background) =>
        BackgroundBonusData.Get(background).ClearConditionDescription;

    /// <summary>ブールフラグを設定</summary>
    public void SetFlag(string flag)
    {
        _boolFlags.Add(flag);
    }

    /// <summary>ブールフラグが立っているか</summary>
    public bool HasFlag(string flag) => _boolFlags.Contains(flag);

    /// <summary>整数フラグを加算</summary>
    public void IncrementFlag(string flag, int amount = 1)
    {
        _intFlags.TryGetValue(flag, out int current);
        _intFlags[flag] = current + amount;
    }

    /// <summary>整数フラグの値を取得</summary>
    public int GetFlagValue(string flag) =>
        _intFlags.TryGetValue(flag, out int val) ? val : 0;

    /// <summary>整数フラグを直接設定</summary>
    public void SetFlagValue(string flag, int value)
    {
        _intFlags[flag] = value;
    }

    /// <summary>指定した素性のクリア条件を満たしているか判定</summary>
    public bool IsClearConditionMet(Background background, int playerLevel, int playerGold)
    {
        var data = BackgroundBonusData.Get(background);
        return data.ClearConditionFlag switch
        {
            "dungeon_clear" => HasFlag("dungeon_clear"),
            "boss_kills_10" => GetFlagValue("boss_kills") >= 10,
            "all_runes_learned" => HasFlag("all_runes_learned"),
            "gold_100000" => playerGold >= 100000,
            "level_30" => playerLevel >= 30,
            "all_territories" => HasFlag("all_territories"),
            "all_territories_visited" => HasFlag("all_territories_visited"),
            "all_secret_floors" => HasFlag("all_secret_floors"),
            "faith_saint" => HasFlag("faith_saint"),
            "sanity_perfect_clear" => HasFlag("sanity_perfect_clear"),
            _ => false
        };
    }

    /// <summary>セーブ用にフラグデータを取得</summary>
    public ClearFlagSaveData CreateSaveData() => new()
    {
        IntFlags = new Dictionary<string, int>(_intFlags),
        BoolFlags = new HashSet<string>(_boolFlags)
    };

    /// <summary>セーブデータからフラグを復元</summary>
    public void RestoreFromSave(ClearFlagSaveData data)
    {
        _intFlags.Clear();
        _boolFlags.Clear();

        // FZ-1: 破損セーブデータのnullチェック
        if (data == null) return;

        foreach (var (key, value) in data.IntFlags)
            _intFlags[key] = value;

        foreach (var flag in data.BoolFlags)
            _boolFlags.Add(flag);
    }
}

/// <summary>
/// クリア条件フラグのセーブデータ
/// </summary>
public class ClearFlagSaveData
{
    public Dictionary<string, int> IntFlags { get; set; } = new();
    public HashSet<string> BoolFlags { get; set; } = new();
}
