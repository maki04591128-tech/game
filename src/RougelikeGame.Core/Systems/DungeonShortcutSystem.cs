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

    /// <summary>ショートカットを開通</summary>
    public bool UnlockShortcut(string dungeonId, int fromFloor, int toFloor)
    {
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
}
