namespace RougelikeGame.Core.Systems;

/// <summary>
/// NPC日常行動ルーティン（時間帯×曜日で行動パターンが変化）
/// </summary>
public static class NpcRoutineSystem
{
    /// <summary>NPC行動種別</summary>
    public enum NpcActivity
    {
        Working,
        Shopping,
        Resting,
        Patrolling,
        Praying,
        Drinking,
        Sleeping
    }

    /// <summary>NPC行動ルーティン定義</summary>
    public record NpcRoutine(string NpcType, TimePeriod Time, NpcActivity Activity, string Location);

    private static readonly List<NpcRoutine> Routines = new()
    {
        // 商人
        new("Merchant", TimePeriod.Dawn, NpcActivity.Working, "ショップ"),
        new("Merchant", TimePeriod.Morning, NpcActivity.Working, "ショップ"),
        new("Merchant", TimePeriod.Afternoon, NpcActivity.Working, "ショップ"),
        new("Merchant", TimePeriod.Dusk, NpcActivity.Resting, "酒場"),
        new("Merchant", TimePeriod.Night, NpcActivity.Sleeping, "自宅"),
        new("Merchant", TimePeriod.Midnight, NpcActivity.Sleeping, "自宅"),

        // 衛兵
        new("Guard", TimePeriod.Dawn, NpcActivity.Patrolling, "城門"),
        new("Guard", TimePeriod.Morning, NpcActivity.Patrolling, "街路"),
        new("Guard", TimePeriod.Afternoon, NpcActivity.Patrolling, "街路"),
        new("Guard", TimePeriod.Dusk, NpcActivity.Patrolling, "城門"),
        new("Guard", TimePeriod.Night, NpcActivity.Patrolling, "城壁"),
        new("Guard", TimePeriod.Midnight, NpcActivity.Resting, "詰所"),

        // 司祭
        new("Priest", TimePeriod.Dawn, NpcActivity.Praying, "神殿"),
        new("Priest", TimePeriod.Morning, NpcActivity.Working, "神殿"),
        new("Priest", TimePeriod.Afternoon, NpcActivity.Working, "神殿"),
        new("Priest", TimePeriod.Dusk, NpcActivity.Praying, "神殿"),
        new("Priest", TimePeriod.Night, NpcActivity.Sleeping, "神殿"),
        new("Priest", TimePeriod.Midnight, NpcActivity.Sleeping, "神殿"),

        // 冒険者
        new("Adventurer", TimePeriod.Dawn, NpcActivity.Sleeping, "宿屋"),
        new("Adventurer", TimePeriod.Morning, NpcActivity.Shopping, "ショップ"),
        new("Adventurer", TimePeriod.Afternoon, NpcActivity.Working, "ギルド"),
        new("Adventurer", TimePeriod.Dusk, NpcActivity.Drinking, "酒場"),
        new("Adventurer", TimePeriod.Night, NpcActivity.Drinking, "酒場"),
        new("Adventurer", TimePeriod.Midnight, NpcActivity.Sleeping, "宿屋"),
    };

    /// <summary>NPC種別と時間帯からルーティンを取得</summary>
    public static NpcRoutine? GetRoutine(string npcType, TimePeriod time)
    {
        return Routines.FirstOrDefault(r => r.NpcType == npcType && r.Time == time);
    }

    /// <summary>指定場所にいるNPC種別一覧を取得</summary>
    public static IReadOnlyList<string> GetNpcsAtLocation(string location, TimePeriod time)
    {
        return Routines.Where(r => r.Location == location && r.Time == time)
            .Select(r => r.NpcType)
            .Distinct()
            .ToList();
    }

    /// <summary>NPCが利用可能（Working状態）かどうか判定</summary>
    public static bool IsNpcAvailable(string npcType, TimePeriod time)
    {
        var routine = GetRoutine(npcType, time);
        return routine?.Activity == NpcActivity.Working;
    }

    /// <summary>全ルーティン定義を取得</summary>
    public static IReadOnlyList<NpcRoutine> GetAllRoutines() => Routines;

    /// <summary>NPC活動名を取得</summary>
    public static string GetActivityName(NpcActivity activity) => activity switch
    {
        NpcActivity.Working => "仕事中",
        NpcActivity.Shopping => "買い物中",
        NpcActivity.Resting => "休憩中",
        NpcActivity.Patrolling => "巡回中",
        NpcActivity.Praying => "祈祷中",
        NpcActivity.Drinking => "飲酒中",
        NpcActivity.Sleeping => "睡眠中",
        _ => "不明"
    };
}
