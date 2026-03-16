using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class DungeonShortcutSystemTests
{
    [Fact]
    public void UnlockShortcut_Success()
    {
        var system = new DungeonShortcutSystem();
        Assert.True(system.UnlockShortcut("dungeon1", 1, 5));
        Assert.True(system.IsUnlocked("dungeon1", 1, 5));
    }

    [Fact]
    public void UnlockShortcut_Duplicate_ReturnsFalse()
    {
        var system = new DungeonShortcutSystem();
        system.UnlockShortcut("dungeon1", 1, 5);
        Assert.False(system.UnlockShortcut("dungeon1", 1, 5));
    }

    [Fact]
    public void GetShortcuts_ReturnsAll()
    {
        var system = new DungeonShortcutSystem();
        system.UnlockShortcut("dungeon1", 1, 5);
        system.UnlockShortcut("dungeon1", 5, 10);
        Assert.Equal(2, system.GetShortcuts("dungeon1").Count);
    }

    [Fact]
    public void GetDeepestAccessibleFloor_ReturnsMax()
    {
        var system = new DungeonShortcutSystem();
        system.UnlockShortcut("d1", 1, 5);
        system.UnlockShortcut("d1", 5, 10);
        Assert.Equal(10, system.GetDeepestAccessibleFloor("d1"));
    }

    [Fact]
    public void TotalUnlocked_TracksCorrectly()
    {
        var system = new DungeonShortcutSystem();
        system.UnlockShortcut("d1", 1, 3);
        system.UnlockShortcut("d2", 1, 5);
        Assert.Equal(2, system.TotalUnlocked);
    }
}
