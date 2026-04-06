using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// AutoSaveSystem テスト
/// - 自動セーブ判定テスト（ターン経過・フロア移動・戦闘終了）
/// - 有効/無効切り替えテスト
/// - マーク済み・リセットテスト
/// </summary>
public class AutoSaveSystemTests
{
    [Fact]
    public void ShouldAutoSave_WhenDisabled_ReturnsFalse()
    {
        var system = new AutoSaveSystem { IsEnabled = false };
        Assert.False(system.ShouldAutoSave(1000, AutoSaveTrigger.TurnElapsed));
        Assert.False(system.ShouldAutoSave(1000, AutoSaveTrigger.FloorChange));
    }

    [Fact]
    public void ShouldAutoSave_TurnElapsed_DefaultInterval300()
    {
        var system = new AutoSaveSystem();
        Assert.Equal(300, system.AutoSaveInterval);
        Assert.False(system.ShouldAutoSave(100, AutoSaveTrigger.TurnElapsed)); // 100 < 300
        Assert.True(system.ShouldAutoSave(300, AutoSaveTrigger.TurnElapsed));  // 300 >= 300
        Assert.True(system.ShouldAutoSave(500, AutoSaveTrigger.TurnElapsed));  // 500 >= 300
    }

    [Fact]
    public void ShouldAutoSave_FloorChange_DefaultTrue()
    {
        var system = new AutoSaveSystem();
        Assert.True(system.ShouldAutoSave(0, AutoSaveTrigger.FloorChange));
    }

    [Fact]
    public void ShouldAutoSave_CombatEnd_DefaultFalse()
    {
        var system = new AutoSaveSystem();
        Assert.False(system.ShouldAutoSave(0, AutoSaveTrigger.CombatEnd));
    }

    [Fact]
    public void ShouldAutoSave_CombatEnd_WhenEnabled()
    {
        var system = new AutoSaveSystem { SaveAfterCombat = true };
        Assert.True(system.ShouldAutoSave(0, AutoSaveTrigger.CombatEnd));
    }

    [Fact]
    public void MarkSaved_UpdatesLastSaveTurn()
    {
        var system = new AutoSaveSystem();
        system.MarkSaved(100);
        // 100ターン経過後にマーク → 次の判定は400からtrue
        Assert.False(system.ShouldAutoSave(200, AutoSaveTrigger.TurnElapsed)); // 200 - 100 = 100 < 300
        Assert.True(system.ShouldAutoSave(400, AutoSaveTrigger.TurnElapsed));  // 400 - 100 = 300 >= 300
    }

    [Fact]
    public void Reset_ResetsToSpecifiedTurn()
    {
        var system = new AutoSaveSystem();
        system.MarkSaved(500);
        system.Reset(0);
        Assert.True(system.ShouldAutoSave(300, AutoSaveTrigger.TurnElapsed)); // 300 - 0 >= 300
    }

    [Fact]
    public void FloorChange_CanBeDisabled()
    {
        var system = new AutoSaveSystem { SaveOnFloorChange = false };
        Assert.False(system.ShouldAutoSave(0, AutoSaveTrigger.FloorChange));
    }

    [Fact]
    public void CustomInterval_WorksCorrectly()
    {
        var system = new AutoSaveSystem { AutoSaveInterval = 100 };
        Assert.True(system.ShouldAutoSave(100, AutoSaveTrigger.TurnElapsed));
        Assert.False(system.ShouldAutoSave(50, AutoSaveTrigger.TurnElapsed));
    }
}
