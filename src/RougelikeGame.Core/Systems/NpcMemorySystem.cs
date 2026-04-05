namespace RougelikeGame.Core.Systems;

/// <summary>
/// NPC記憶システム - NPCが過去のプレイヤー行動を記憶し、噂が伝播する
/// </summary>
public class NpcMemorySystem
{
    /// <summary>行動記憶</summary>
    public record MemoryEntry(
        string NpcId,
        string Action,
        int Impact,
        int TurnRecorded
    );

    /// <summary>噂</summary>
    public record Rumor(
        RumorType Type,
        string Content,
        int SpreadCount,
        string OriginTerritory
    );

    private readonly List<MemoryEntry> _memories = new();
    private readonly List<Rumor> _rumors = new();

    /// <summary>全記憶</summary>
    public IReadOnlyList<MemoryEntry> Memories => _memories;

    /// <summary>全噂</summary>
    public IReadOnlyList<Rumor> Rumors => _rumors;

    /// <summary>行動を記憶</summary>
    public void RecordAction(string npcId, string action, int impact, int currentTurn)
    {
        _memories.Add(new MemoryEntry(npcId, action, impact, currentTurn));
    }

    /// <summary>噂を発生</summary>
    public void GenerateRumor(RumorType type, string content, string originTerritory)
    {
        _rumors.Add(new Rumor(type, content, 0, originTerritory));
    }

    /// <summary>噂を伝播（SpreadCountを増加）</summary>
    public void SpreadRumors()
    {
        for (int i = 0; i < _rumors.Count; i++)
        {
            _rumors[i] = _rumors[i] with { SpreadCount = _rumors[i].SpreadCount + 1 };
        }
    }

    /// <summary>特定NPCのプレイヤーへの印象値を計算</summary>
    public int CalculateImpression(string npcId)
    {
        return _memories.Where(m => m.NpcId == npcId).Sum(m => m.Impact);
    }

    /// <summary>
    /// 全記憶・噂をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、NPCとの関係は全て消失する。
    /// </summary>
    public void Reset()
    {
        _memories.Clear();
        _rumors.Clear();
    }

    /// <summary>噂の種別名を取得</summary>
    public static string GetRumorTypeName(RumorType type) => type switch
    {
        RumorType.Heroic => "英雄の噂",
        RumorType.Villainous => "悪漢の噂",
        RumorType.Eccentric => "奇人の噂",
        RumorType.Unknown => "無名",
        _ => "不明"
    };

    /// <summary>BQ-11: セーブデータから記憶を復元</summary>
    public void RestoreMemories(IEnumerable<MemoryEntry> entries)
    {
        foreach (var entry in entries)
            _memories.Add(entry);
    }
}
