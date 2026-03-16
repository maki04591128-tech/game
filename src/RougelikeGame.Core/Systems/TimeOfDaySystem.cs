namespace RougelikeGame.Core.Systems;

/// <summary>
/// 時刻行動変化システム - 時刻に応じた敵の活動パターン変化を管理
/// </summary>
public static class TimeOfDaySystem
{
    /// <summary>
    /// 種族別の活動パターンを取得
    /// </summary>
    public static ActivityPattern GetActivityPattern(MonsterRace race) => race switch
    {
        MonsterRace.Beast => ActivityPattern.Nocturnal,
        MonsterRace.Humanoid => ActivityPattern.Diurnal,
        MonsterRace.Amorphous => ActivityPattern.Constant,
        MonsterRace.Undead => ActivityPattern.Nocturnal,
        MonsterRace.Demon => ActivityPattern.Nocturnal,
        MonsterRace.Dragon => ActivityPattern.Diurnal,
        MonsterRace.Plant => ActivityPattern.Diurnal,
        MonsterRace.Insect => ActivityPattern.Crepuscular,
        MonsterRace.Spirit => ActivityPattern.Nocturnal,
        MonsterRace.Construct => ActivityPattern.Constant,
        _ => ActivityPattern.Constant
    };

    /// <summary>
    /// 活動パターンと時刻から活性度倍率を計算
    /// </summary>
    public static float GetActivityMultiplier(ActivityPattern pattern, TimePeriod currentTime) => pattern switch
    {
        ActivityPattern.Diurnal => currentTime switch
        {
            TimePeriod.Dawn => 0.7f,
            TimePeriod.Morning => 1.0f,
            TimePeriod.Afternoon => 1.0f,
            TimePeriod.Dusk => 0.8f,
            TimePeriod.Night => 0.5f,
            TimePeriod.Midnight => 0.5f,
            _ => 1.0f
        },
        ActivityPattern.Nocturnal => currentTime switch
        {
            TimePeriod.Dawn => 1.0f,
            TimePeriod.Morning => 0.6f,
            TimePeriod.Afternoon => 0.6f,
            TimePeriod.Dusk => 0.9f,
            TimePeriod.Night => 1.2f,
            TimePeriod.Midnight => 1.2f,
            _ => 1.0f
        },
        ActivityPattern.Crepuscular => currentTime switch
        {
            TimePeriod.Dawn => 1.3f,
            TimePeriod.Morning => 0.8f,
            TimePeriod.Afternoon => 0.8f,
            TimePeriod.Dusk => 1.3f,
            TimePeriod.Night => 0.9f,
            TimePeriod.Midnight => 0.9f,
            _ => 1.0f
        },
        ActivityPattern.Constant => 1.0f,
        _ => 1.0f
    };

    /// <summary>
    /// 時刻による視界範囲修正を取得
    /// </summary>
    public static float GetSightRangeModifier(TimePeriod currentTime) => currentTime switch
    {
        TimePeriod.Dawn => 0.7f,
        TimePeriod.Morning => 1.0f,
        TimePeriod.Afternoon => 1.0f,
        TimePeriod.Dusk => 0.8f,
        TimePeriod.Night => 0.6f,
        TimePeriod.Midnight => 0.4f,
        _ => 1.0f
    };

    /// <summary>
    /// 種族が現在活動時間かどうか判定（倍率0.6以上で活動中）
    /// </summary>
    public static bool IsActiveTime(MonsterRace race, TimePeriod currentTime)
    {
        var pattern = GetActivityPattern(race);
        var multiplier = GetActivityMultiplier(pattern, currentTime);
        return multiplier >= 0.6f;
    }

    /// <summary>
    /// 時刻によるステータス修正を取得
    /// </summary>
    public static StatModifier GetStatModifier(MonsterRace race, TimePeriod currentTime)
    {
        var pattern = GetActivityPattern(race);
        var multiplier = GetActivityMultiplier(pattern, currentTime);

        if (multiplier >= 1.2f)
        {
            // 最も活発な時間帯：全ステータス+2
            return new StatModifier(2, 2, 2, 2, 2, 2, 2, 2, 2);
        }
        else if (multiplier >= 1.0f)
        {
            // 通常活動時間帯：修正なし
            return StatModifier.Zero;
        }
        else if (multiplier >= 0.7f)
        {
            // やや不活発：全ステータス-1
            return new StatModifier(-1, -1, -1, -1, -1, -1, -1, -1, -1);
        }
        else
        {
            // 非活動時間帯：全ステータス-2
            return new StatModifier(-2, -2, -2, -2, -2, -2, -2, -2, -2);
        }
    }

    /// <summary>
    /// 時刻から TimePeriod を取得
    /// </summary>
    public static TimePeriod GetTimePeriod(int hour) => hour switch
    {
        >= 4 and <= 6 => TimePeriod.Dawn,
        >= 7 and <= 11 => TimePeriod.Morning,
        >= 12 and <= 16 => TimePeriod.Afternoon,
        >= 17 and <= 19 => TimePeriod.Dusk,
        >= 20 and <= 23 => TimePeriod.Night,
        _ => TimePeriod.Midnight  // 0-3
    };

    /// <summary>
    /// TimePeriod の日本語名を取得
    /// </summary>
    public static string GetTimePeriodName(TimePeriod period) => period switch
    {
        TimePeriod.Dawn => "早朝",
        TimePeriod.Morning => "午前",
        TimePeriod.Afternoon => "午後",
        TimePeriod.Dusk => "夕方",
        TimePeriod.Night => "夜",
        TimePeriod.Midnight => "深夜",
        _ => "不明"
    };

    /// <summary>
    /// 活動パターンの日本語名を取得
    /// </summary>
    public static string GetActivityPatternName(ActivityPattern pattern) => pattern switch
    {
        ActivityPattern.Diurnal => "昼行性",
        ActivityPattern.Nocturnal => "夜行性",
        ActivityPattern.Crepuscular => "薄明薄暮性",
        ActivityPattern.Constant => "常時活動",
        _ => "不明"
    };
}
