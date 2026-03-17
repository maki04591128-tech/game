namespace RougelikeGame.Core.Systems;

/// <summary>
/// 知識図鑑・百科事典システム - 討伐/発見で情報が段階的に開示
/// </summary>
public class EncyclopediaSystem
{
    /// <summary>図鑑エントリ</summary>
    public record EncyclopediaEntry(
        EncyclopediaCategory Category,
        string Id,
        string Name,
        int DiscoveryLevel,
        int MaxLevel,
        Dictionary<int, string> LevelDescriptions
    );

    private readonly Dictionary<string, EncyclopediaEntry> _entries = new();

    /// <summary>エントリを登録</summary>
    public void RegisterEntry(EncyclopediaCategory category, string id, string name, int maxLevel, Dictionary<int, string> descriptions)
    {
        _entries[id] = new EncyclopediaEntry(category, id, name, 0, maxLevel, descriptions);
    }

    /// <summary>発見レベルを上昇</summary>
    public bool IncrementDiscovery(string id)
    {
        if (!_entries.TryGetValue(id, out var entry)) return false;
        if (entry.DiscoveryLevel >= entry.MaxLevel) return false;

        _entries[id] = entry with { DiscoveryLevel = entry.DiscoveryLevel + 1 };
        return true;
    }

    /// <summary>エントリを取得</summary>
    public EncyclopediaEntry? GetEntry(string id)
    {
        return _entries.TryGetValue(id, out var entry) ? entry : null;
    }

    /// <summary>カテゴリ別のエントリ一覧を取得</summary>
    public IReadOnlyList<EncyclopediaEntry> GetByCategory(EncyclopediaCategory category)
    {
        return _entries.Values.Where(e => e.Category == category).ToList();
    }

    /// <summary>発見率を計算（発見済み/全体）</summary>
    public float GetDiscoveryRate(EncyclopediaCategory category)
    {
        var entries = _entries.Values.Where(e => e.Category == category).ToList();
        if (entries.Count == 0) return 0;
        return (float)entries.Count(e => e.DiscoveryLevel > 0) / entries.Count;
    }

    /// <summary>完全開示率を計算（MaxLevel到達/全体）</summary>
    public float GetCompletionRate(EncyclopediaCategory category)
    {
        var entries = _entries.Values.Where(e => e.Category == category).ToList();
        if (entries.Count == 0) return 0;
        return (float)entries.Count(e => e.DiscoveryLevel >= e.MaxLevel) / entries.Count;
    }

    /// <summary>全エントリ数</summary>
    public int TotalEntries => _entries.Count;

    /// <summary>発見済みエントリ数</summary>
    public int DiscoveredEntries => _entries.Values.Count(e => e.DiscoveryLevel > 0);

    /// <summary>現在の発見レベルで閲覧可能な説明テキストを取得</summary>
    public string GetCurrentDescription(string id)
    {
        if (!_entries.TryGetValue(id, out var entry)) return "未発見";
        if (entry.DiscoveryLevel == 0) return "???";

        string desc = "";
        for (int i = 1; i <= entry.DiscoveryLevel; i++)
        {
            if (entry.LevelDescriptions.TryGetValue(i, out var text))
                desc += (desc.Length > 0 ? "\n" : "") + text;
        }
        return desc;
    }

    /// <summary>
    /// 全発見レベルをリセットする（死に戻り＋正気度0時に呼び出し）。
    /// 正気度0での死に戻りでは知識が消失するため、図鑑の発見レベルが0に戻る。
    /// エントリ定義（マスターデータ）は保持される。
    /// </summary>
    public void ResetDiscoveryLevels()
    {
        foreach (var id in _entries.Keys.ToList())
        {
            _entries[id] = _entries[id] with { DiscoveryLevel = 0 };
        }
    }
}
