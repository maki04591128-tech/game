namespace RougelikeGame.Core.Systems;

/// <summary>
/// マルチスロットセーブシステム - 複数セーブスロットの管理
/// </summary>
public class MultiSlotSaveSystem
{
    /// <summary>セーブスロット情報</summary>
    public record SaveSlotInfo(
        int SlotNumber,
        string? CharacterName,
        int? Level,
        string? Location,
        DateTime? SaveTime,
        bool IsEmpty
    );

    /// <summary>最大スロット数</summary>
    public const int MaxSlots = 5;

    private readonly SaveSlotInfo[] _slots;

    public MultiSlotSaveSystem()
    {
        _slots = new SaveSlotInfo[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
            _slots[i] = new SaveSlotInfo(i + 1, null, null, null, null, true);
    }

    /// <summary>全スロット情報を取得</summary>
    public IReadOnlyList<SaveSlotInfo> GetAllSlots() => _slots;

    /// <summary>指定スロットの情報を取得</summary>
    public SaveSlotInfo GetSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MaxSlots)
            return new SaveSlotInfo(slotNumber, null, null, null, null, true);
        return _slots[slotNumber - 1];
    }

    /// <summary>スロットにセーブ情報を記録</summary>
    public bool SaveToSlot(int slotNumber, string characterName, int level, string location)
    {
        if (slotNumber < 1 || slotNumber > MaxSlots) return false;
        _slots[slotNumber - 1] = new SaveSlotInfo(
            slotNumber, characterName, level, location, DateTime.Now, false);
        return true;
    }

    /// <summary>スロットをクリア</summary>
    public bool ClearSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > MaxSlots) return false;
        _slots[slotNumber - 1] = new SaveSlotInfo(slotNumber, null, null, null, null, true);
        return true;
    }

    /// <summary>空きスロット数を取得</summary>
    public int GetEmptySlotCount() => _slots.Count(s => s.IsEmpty);

    /// <summary>最も古いスロット番号を取得（上書き用）</summary>
    public int GetOldestSlot()
    {
        // DF-1/DF-2: SaveTimeがnullの場合はDateTime.MinValueで比較、全スロット空の場合はスロット1を返す
        var oldest = _slots.Where(s => !s.IsEmpty)
            .OrderBy(s => s.SaveTime ?? DateTime.MinValue)
            .FirstOrDefault();
        return oldest?.SlotNumber ?? 1;
    }
}
