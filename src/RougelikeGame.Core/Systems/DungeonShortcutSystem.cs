namespace RougelikeGame.Core.Systems;

/// <summary>
/// ダンジョンショートカットシステム - 永続的なショートカット管理
/// </summary>
public class DungeonShortcutSystem
{
    /// <summary>ショートカット情報</summary>
    public record ShortcutInfo(
        string DungeonId,
        int FromFloor,
        int ToFloor,
        string Description
    );

    private readonly HashSet<(string DungeonId, int FromFloor, int ToFloor)> _unlockedShortcuts = new();
    private readonly HashSet<(string DungeonId, int Floor)> _visitedFloors = new();

    /// <summary>階の訪問を記録</summary>
    public void MarkFloorVisited(string dungeonId, int floor)
    {
        _visitedFloors.Add((dungeonId, floor));
    }

    /// <summary>階が訪問済みか確認</summary>
    public bool IsFloorVisited(string dungeonId, int floor)
    {
        return _visitedFloors.Contains((dungeonId, floor));
    }

    /// <summary>ショートカットを開通（出発階と到着階の両方が訪問済みの場合のみ）</summary>
    public bool UnlockShortcut(string dungeonId, int fromFloor, int toFloor)
    {
        // 訪問済みチェック: 両方の階を訪問済みでないとショートカット開通不可
        if (!IsFloorVisited(dungeonId, fromFloor) || !IsFloorVisited(dungeonId, toFloor))
            return false;

        return _unlockedShortcuts.Add((dungeonId, fromFloor, toFloor));
    }

    /// <summary>ショートカットが開通済みか確認</summary>
    public bool IsUnlocked(string dungeonId, int fromFloor, int toFloor)
    {
        return _unlockedShortcuts.Contains((dungeonId, fromFloor, toFloor));
    }

    /// <summary>特定ダンジョンの全ショートカットを取得</summary>
    public IReadOnlyList<(int FromFloor, int ToFloor)> GetShortcuts(string dungeonId)
    {
        return _unlockedShortcuts
            .Where(s => s.DungeonId == dungeonId)
            .Select(s => (s.FromFloor, s.ToFloor))
            .ToList();
    }

    /// <summary>総開通数</summary>
    public int TotalUnlocked => _unlockedShortcuts.Count;

    /// <summary>ショートカットで移動可能な最深階を取得</summary>
    public int GetDeepestAccessibleFloor(string dungeonId)
    {
        var shortcuts = _unlockedShortcuts.Where(s => s.DungeonId == dungeonId).ToList();
        return shortcuts.Count > 0 ? shortcuts.Max(s => s.ToFloor) : 1;
    }


    /// <summary>BQ-21: 訪問済みフロア一覧を取得</summary>
    public IReadOnlyCollection<(string DungeonId, int Floor)> GetVisitedFloors() => _visitedFloors;

    /// <summary>BQ-21: 解放済みショートカット一覧を取得</summary>
    public IReadOnlyCollection<(string DungeonId, int FromFloor, int ToFloor)> GetUnlockedShortcuts() => _unlockedShortcuts;

    /// <summary>BQ-21: セーブデータからショートカット状態を復元</summary>
    public void RestoreState(
        IEnumerable<(string DungeonId, int Floor)> visitedFloors,
        IEnumerable<(string DungeonId, int FromFloor, int ToFloor)> shortcuts)
    {
        foreach (var vf in visitedFloors)
            _visitedFloors.Add(vf);
        foreach (var sc in shortcuts)
            _unlockedShortcuts.Add(sc);
    }
}