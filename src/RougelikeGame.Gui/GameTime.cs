namespace RougelikeGame.Gui;

/// <summary>
/// ゲーム内時間管理
/// 60ターン = 1分
/// </summary>
public class GameTime
{
    // 暦の月名
    private static readonly string[] MonthNames =
    {
        "霜の月",    // 1月
        "雪解の月",  // 2月
        "芽吹の月",  // 3月
        "花咲の月",  // 4月
        "陽光の月",  // 5月
        "緑風の月",  // 6月
        "盛夏の月",  // 7月
        "収穫の月",  // 8月
        "紅葉の月",  // 9月
        "落葉の月",  // 10月
        "薄暮の月",  // 11月
        "星霜の月"   // 12月
    };

    private const int TurnsPerMinute = 60;
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int DaysPerMonth = 30;
    private const int MonthsPerYear = 12;
    private const int MinutesPerDay = MinutesPerHour * HoursPerDay;    // 1440
    private const int MinutesPerMonth = MinutesPerDay * DaysPerMonth;  // 43200
    private const int MinutesPerYear = MinutesPerMonth * MonthsPerYear; // 518400

    /// <summary>
    /// ゲーム開始からの総ターン数（=総分数）
    /// </summary>
    public int TotalTurns { get; private set; }

    /// <summary>
    /// セーブデータからの復元用に総ターン数を設定
    /// </summary>
    public void SetTotalTurns(int turns)
    {
        TotalTurns = turns;
    }

    // 暦名
    public string EraName { get; set; } = "冒険";

    // 開始年
    public int StartYear { get; set; } = 1024;

    // 開始月（1-12）
    public int StartMonth { get; set; } = 6;

    // 開始日（1-30）
    public int StartDay { get; set; } = 15;

    // 開始時刻（時）
    public int StartHour { get; set; } = 8;

    // 開始時刻（分）
    public int StartMinute { get; set; } = 0;

    public GameTime()
    {
    }

    /// <summary>DA-6: 日変更イベント</summary>
    public event Action<int>? OnDayChanged;

    /// <summary>
    /// 1ターン進める
    /// </summary>
    public void AdvanceTurn(int turns = 1)
    {
        int prevDay = Day;
        TotalTurns += turns;
        if (Day != prevDay)
            OnDayChanged?.Invoke(Day);
    }

    /// <summary>
    /// ゲーム開始時刻からの総分数
    /// </summary>
    private long TotalMinutesFromEpoch
    {
        get
        {
            long baseMinutes =
                (long)(StartYear - 1) * MinutesPerYear +
                (long)(StartMonth - 1) * MinutesPerMonth +
                (long)(StartDay - 1) * MinutesPerDay +
                (long)StartHour * MinutesPerHour +
                StartMinute;
            return baseMinutes + TotalTurns / TurnsPerMinute;
        }
    }

    public int Year => (int)(TotalMinutesFromEpoch / MinutesPerYear) + 1;
    public int Month => (int)(TotalMinutesFromEpoch % MinutesPerYear / MinutesPerMonth) + 1;
    public int Day => (int)(TotalMinutesFromEpoch % MinutesPerMonth / MinutesPerDay) + 1;
    public int Hour => (int)(TotalMinutesFromEpoch % MinutesPerDay / MinutesPerHour);
    public int Minute => (int)(TotalMinutesFromEpoch % MinutesPerHour);

    /// <summary>
    /// 秒（0-59）: TotalTurnsから秒を計算（1ターン＝1秒）
    /// </summary>
    public int Second => TotalTurns % TurnsPerMinute;

    public string MonthName => MonthNames[(Month - 1) % MonthNames.Length];

    /// <summary>
    /// 時間帯の名称
    /// </summary>
    public string TimePeriod => Hour switch
    {
        >= 5 and < 7 => "明け方",
        >= 7 and < 10 => "朝",
        >= 10 and < 12 => "午前",
        >= 12 and < 14 => "昼",
        >= 14 and < 17 => "午後",
        >= 17 and < 19 => "夕方",
        >= 19 and < 22 => "夜",
        _ => "深夜"
    };

    /// <summary>
    /// フル表示形式
    /// 例: 冒険歴1024年 緑風の月 15日 08:00:00
    /// </summary>
    public string ToFullString()
    {
        return $"{EraName}歴{Year:D4}年 {MonthName} {Day:D2}日 {Hour:D2}:{Minute:D2}:{Second:D2}";
    }

    /// <summary>
    /// 短縮表示形式
    /// 例: 緑風の月 15日 08:00:00
    /// </summary>
    public string ToShortString()
    {
        return $"{MonthName} {Day:D2}日 {Hour:D2}:{Minute:D2}:{Second:D2}";
    }

    public override string ToString() => ToFullString();
}
