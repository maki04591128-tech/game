namespace RougelikeGame.Core.Systems;

/// <summary>
/// 死因統計・死亡ログシステム - 全死亡記録の管理
/// </summary>
public class DeathLogSystem
{
    /// <summary>死亡ログエントリ</summary>
    public record DeathLogEntry(
        int RunNumber,
        string CharacterName,
        CharacterClass Class,
        Race Race,
        int Level,
        DeathCause Cause,
        string CauseDetail,
        string Location,
        int Floor,
        int TotalTurns,
        DateTime Timestamp
    );

    private readonly List<DeathLogEntry> _logs = new();

    /// <summary>全死亡ログ</summary>
    public IReadOnlyList<DeathLogEntry> AllLogs => _logs;

    /// <summary>死亡記録を追加</summary>
    public void AddLog(DeathLogEntry entry) => _logs.Add(entry);

    /// <summary>総死亡回数</summary>
    public int TotalDeaths => _logs.Count;

    /// <summary>死因別の統計を取得</summary>
    public IReadOnlyDictionary<DeathCause, int> GetDeathsByCategory()
    {
        return _logs.GroupBy(l => l.Cause)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>最も多い死因を取得</summary>
    public DeathCause? GetMostCommonCause()
    {
        if (_logs.Count == 0) return null;
        return _logs.GroupBy(l => l.Cause)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    /// <summary>最高到達レベルを取得</summary>
    public int GetHighestLevel()
    {
        return _logs.Count > 0 ? _logs.Max(l => l.Level) : 0;
    }

    /// <summary>最深到達階層を取得</summary>
    public int GetDeepestFloor()
    {
        return _logs.Count > 0 ? _logs.Max(l => l.Floor) : 0;
    }

    /// <summary>平均生存ターン数を取得</summary>
    public double GetAverageSurvivalTurns()
    {
        return _logs.Count > 0 ? _logs.Average(l => l.TotalTurns) : 0;
    }

    /// <summary>職業別死亡統計を取得</summary>
    public IReadOnlyDictionary<CharacterClass, int> GetDeathsByClass()
    {
        return _logs.GroupBy(l => l.Class)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>直近N件の死亡ログを取得</summary>
    public IReadOnlyList<DeathLogEntry> GetRecentLogs(int count)
    {
        return _logs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
    }
}
