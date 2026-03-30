using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// BackgroundClearSystem（素性クリア条件システム）のテスト
/// </summary>
public class BackgroundClearSystemTests
{
    // --- GetClearFlag ---

    [Theory]
    [InlineData(Background.Adventurer, "dungeon_clear")]
    [InlineData(Background.Soldier, "boss_kills_10")]
    [InlineData(Background.Scholar, "all_runes_learned")]
    [InlineData(Background.Merchant, "gold_100000")]
    [InlineData(Background.Peasant, "level_30")]
    public void GetClearFlag_ReturnsCorrectFlag(Background bg, string expected)
    {
        Assert.Equal(expected, BackgroundClearSystem.GetClearFlag(bg));
    }

    // --- GetClearDescription ---

    [Theory]
    [InlineData(Background.Adventurer, "ダンジョン最深部到達")]
    [InlineData(Background.Merchant, "100,000ゴールド貯蓄")]
    [InlineData(Background.Peasant, "レベル30到達")]
    public void GetClearDescription_ReturnsJapaneseDescription(Background bg, string expected)
    {
        Assert.Equal(expected, BackgroundClearSystem.GetClearDescription(bg));
    }

    // --- SetFlag / HasFlag ---

    [Fact]
    public void HasFlag_NotSet_ReturnsFalse()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.HasFlag("dungeon_clear"));
    }

    [Fact]
    public void SetFlag_ThenHasFlag_ReturnsTrue()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("dungeon_clear");
        Assert.True(system.HasFlag("dungeon_clear"));
    }

    [Fact]
    public void SetFlag_Duplicate_NoError()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("test");
        system.SetFlag("test");
        Assert.True(system.HasFlag("test"));
    }

    // --- IncrementFlag / GetFlagValue ---

    [Fact]
    public void GetFlagValue_NotSet_ReturnsZero()
    {
        var system = new BackgroundClearSystem();
        Assert.Equal(0, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void IncrementFlag_DefaultAmount_IncrementsBy1()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills");
        Assert.Equal(1, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void IncrementFlag_CustomAmount_IncrementsByAmount()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills", 5);
        Assert.Equal(5, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void IncrementFlag_MultipleTimes_Accumulates()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills", 3);
        system.IncrementFlag("boss_kills", 7);
        Assert.Equal(10, system.GetFlagValue("boss_kills"));
    }

    // --- SetFlagValue ---

    [Fact]
    public void SetFlagValue_SetsExactValue()
    {
        var system = new BackgroundClearSystem();
        system.SetFlagValue("count", 42);
        Assert.Equal(42, system.GetFlagValue("count"));
    }

    [Fact]
    public void SetFlagValue_OverwritesPreviousValue()
    {
        var system = new BackgroundClearSystem();
        system.SetFlagValue("count", 10);
        system.SetFlagValue("count", 20);
        Assert.Equal(20, system.GetFlagValue("count"));
    }

    // --- IsClearConditionMet ---

    [Fact]
    public void IsClearConditionMet_Adventurer_NeedsFlag()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Adventurer, 1, 0));
        system.SetFlag("dungeon_clear");
        Assert.True(system.IsClearConditionMet(Background.Adventurer, 1, 0));
    }

    [Fact]
    public void IsClearConditionMet_Soldier_NeedsBossKills10()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills", 9);
        Assert.False(system.IsClearConditionMet(Background.Soldier, 1, 0));
        system.IncrementFlag("boss_kills", 1);
        Assert.True(system.IsClearConditionMet(Background.Soldier, 1, 0));
    }

    [Fact]
    public void IsClearConditionMet_Merchant_NeedsGold100000()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Merchant, 1, 99999));
        Assert.True(system.IsClearConditionMet(Background.Merchant, 1, 100000));
    }

    [Fact]
    public void IsClearConditionMet_Peasant_NeedsLevel30()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Peasant, 29, 0));
        Assert.True(system.IsClearConditionMet(Background.Peasant, 30, 0));
    }

    // --- CreateSaveData / RestoreFromSave ---

    [Fact]
    public void CreateSaveData_PreservesFlags()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("dungeon_clear");
        system.SetFlagValue("boss_kills", 5);

        var save = system.CreateSaveData();

        Assert.Contains("dungeon_clear", save.BoolFlags);
        Assert.Equal(5, save.IntFlags["boss_kills"]);
    }

    [Fact]
    public void RestoreFromSave_RestoresState()
    {
        var original = new BackgroundClearSystem();
        original.SetFlag("dungeon_clear");
        original.SetFlagValue("boss_kills", 10);
        var save = original.CreateSaveData();

        var restored = new BackgroundClearSystem();
        restored.RestoreFromSave(save);

        Assert.True(restored.HasFlag("dungeon_clear"));
        Assert.Equal(10, restored.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void RestoreFromSave_ClearsExistingState()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("old_flag");
        system.SetFlagValue("old_count", 99);

        var emptyData = new ClearFlagSaveData();
        system.RestoreFromSave(emptyData);

        Assert.False(system.HasFlag("old_flag"));
        Assert.Equal(0, system.GetFlagValue("old_count"));
    }
}
