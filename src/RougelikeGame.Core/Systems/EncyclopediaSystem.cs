namespace RougelikeGame.Core.Systems;

/// <summary>
/// モンスター図鑑用の詳細データ
/// </summary>
public record MonsterEncyclopediaData(
    string RaceName,
    int MaxHp,
    int MaxMp,
    int MaxSp,
    Stats BaseStats,
    string? DropTableId,
    string Description
);

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
        Dictionary<int, string> LevelDescriptions,
        int KillCount = 0,
        MonsterEncyclopediaData? MonsterData = null
    );

    /// <summary>モンスター撃破数による開示閾値</summary>
    public static class MonsterRevealThresholds
    {
        public const int NameAndRace = 1;      // 初回遭遇: 名前と種族
        public const int HpMpSp = 10;          // 10体撃破: HP/MP/SP
        public const int Stats = 20;           // 20体撃破: ステータス
        public const int DropItems = 30;       // 30体撃破: ドロップアイテム
        public const int ElementalAffinity = 40; // 40体撃破: 属性相性
        public const int MaxDiscoveryLevel = 5;
    }

    private readonly Dictionary<string, EncyclopediaEntry> _entries = new();

    /// <summary>エントリを登録</summary>
    public void RegisterEntry(EncyclopediaCategory category, string id, string name, int maxLevel, Dictionary<int, string> descriptions)
    {
        _entries[id] = new EncyclopediaEntry(category, id, name, 0, maxLevel, descriptions);
    }

    /// <summary>モンスターエントリを登録（詳細データ付き）</summary>
    public void RegisterMonsterEntry(string id, string name, MonsterEncyclopediaData monsterData)
    {
        if (_entries.ContainsKey(id)) return;
        _entries[id] = new EncyclopediaEntry(
            EncyclopediaCategory.Monster, id, name,
            DiscoveryLevel: 0, // DI-1: 初回は未発見（0）。遭遇で1に上がる
            MaxLevel: MonsterRevealThresholds.MaxDiscoveryLevel,
            LevelDescriptions: new Dictionary<int, string>(),
            KillCount: 0,
            MonsterData: monsterData
        );
    }

    /// <summary>モンスター撃破数をインクリメントし、開示レベルを自動更新</summary>
    public bool IncrementMonsterKill(string id)
    {
        if (!_entries.TryGetValue(id, out var entry)) return false;
        if (entry.Category != EncyclopediaCategory.Monster) return false;

        int newKillCount = entry.KillCount + 1;
        int newLevel = CalculateMonsterDiscoveryLevel(newKillCount);

        _entries[id] = entry with { KillCount = newKillCount, DiscoveryLevel = newLevel };
        return newLevel > entry.DiscoveryLevel;
    }

    /// <summary>撃破数から開示レベルを算出</summary>
    private static int CalculateMonsterDiscoveryLevel(int killCount)
    {
        if (killCount >= MonsterRevealThresholds.ElementalAffinity) return 5;
        if (killCount >= MonsterRevealThresholds.DropItems) return 4;
        if (killCount >= MonsterRevealThresholds.Stats) return 3;
        if (killCount >= MonsterRevealThresholds.HpMpSp) return 2;
        if (killCount >= MonsterRevealThresholds.NameAndRace) return 1;
        return 0;
    }

    /// <summary>モンスターの開示情報テキストを生成</summary>
    public string GetMonsterDescription(string id)
    {
        if (!_entries.TryGetValue(id, out var entry)) return "未発見";
        if (entry.DiscoveryLevel == 0) return "???";
        if (entry.MonsterData == null) return GetCurrentDescription(id);

        var data = entry.MonsterData;
        var lines = new List<string>();

        // レベル1: 名前と種族
        lines.Add($"種族: {data.RaceName}");
        lines.Add($"{data.Description}");

        // レベル2: HP/MP/SP (10体撃破)
        if (entry.DiscoveryLevel >= 2)
        {
            lines.Add($"\n【生命力】");
            lines.Add($"HP: {data.MaxHp}  MP: {data.MaxMp}  SP: {data.MaxSp}");
        }

        // レベル3: ステータス (20体撃破)
        if (entry.DiscoveryLevel >= 3)
        {
            lines.Add($"\n【ステータス】");
            lines.Add($"STR:{data.BaseStats.Strength} VIT:{data.BaseStats.Vitality} AGI:{data.BaseStats.Agility}");
            lines.Add($"DEX:{data.BaseStats.Dexterity} INT:{data.BaseStats.Intelligence} MND:{data.BaseStats.Mind}");
            lines.Add($"PER:{data.BaseStats.Perception} CHA:{data.BaseStats.Charisma} LUK:{data.BaseStats.Luck}");
        }

        // レベル4: ドロップアイテム (30体撃破)
        if (entry.DiscoveryLevel >= 4)
        {
            lines.Add($"\n【ドロップ】");
            lines.Add(data.DropTableId != null ? $"ドロップテーブル: {data.DropTableId}" : "ドロップなし");
        }

        // レベル5: 属性相性 (40体撃破)
        if (entry.DiscoveryLevel >= 5)
        {
            lines.Add($"\n【属性相性】");
            lines.Add($"物理防御: {data.BaseStats.PhysicalDefense}  魔法防御: {data.BaseStats.MagicalDefense}");
        }

        lines.Add($"\n撃破数: {entry.KillCount}");

        return string.Join("\n", lines);
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

        // モンスターカテゴリは専用メソッドを使用
        if (entry.Category == EncyclopediaCategory.Monster && entry.MonsterData != null)
            return GetMonsterDescription(id);

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
            _entries[id] = _entries[id] with { DiscoveryLevel = 0, KillCount = 0 };
        }
    }


    /// <summary>全エントリを取得</summary>
    public IReadOnlyCollection<EncyclopediaEntry> GetAllEntries() => _entries.Values;

    /// <summary>BQ-4: セーブデータから発見状態を復元</summary>
    public void RestoreDiscoveryState(Dictionary<string, (int DiscoveryLevel, int KillCount)> savedEntries)
    {
        foreach (var (id, state) in savedEntries)
        {
            if (_entries.TryGetValue(id, out var entry))
            {
                _entries[id] = entry with { DiscoveryLevel = state.DiscoveryLevel, KillCount = state.KillCount };
            }
        }
    }
}