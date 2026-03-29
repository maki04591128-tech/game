using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Entities;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 鍛冶システム＋チュートリアルシステムテスト
/// テスト数: 22件
/// </summary>
public class SmithingTutorialSystemTests
{
    // ============================================================
    // SmithingSystem テスト
    // ============================================================

    private static Player CreateTestPlayer(int gold = 10000)
    {
        var player = Player.Create("テスト", Stats.Default);
        player.AddGold(gold);
        return player;
    }

    [Fact]
    public void Smithing_Enhance_Succeeds()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "鋼の剣", 0);
        Assert.True(result.Success);
        Assert.Equal(1, result.NewEnhanceLevel);
    }

    [Fact]
    public void Smithing_Enhance_MaxLevel_Fails()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "鋼の剣", 10, maxEnhance: 10);
        Assert.False(result.Success);
        Assert.Contains("これ以上", result.Message);
    }

    [Fact]
    public void Smithing_Enhance_InsufficientGold_Fails()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 0);
        var result = system.Enhance(player, "鋼の剣", 0);
        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    [Fact]
    public void Smithing_Repair_Succeeds()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Repair(player, "鋼の剣", 10);
        Assert.True(result.Success);
        Assert.Contains("修理した", result.Message);
    }

    [Fact]
    public void Smithing_Repair_NoDamage_Fails()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Repair(player, "鋼の剣", 0);
        Assert.False(result.Success);
    }

    [Fact]
    public void Smithing_Synthesize_Succeeds()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Synthesize(player, new[] { "ore1", "ore2" }, "sword_new", 500);
        Assert.True(result.Success);
        Assert.Equal("sword_new", result.ResultItemId);
    }

    [Fact]
    public void Smithing_Enchant_Succeeds()
    {
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enchant(player, "鋼の剣", Element.Fire, 300);
        Assert.True(result.Success);
        Assert.Contains("炎", result.Message);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 400)]
    [InlineData(2, 900)]
    [InlineData(5, 3600)]
    public void Smithing_CalculateEnhanceCost_ReturnsCorrectCost(int currentEnhance, int expectedCost)
    {
        Assert.Equal(expectedCost, SmithingSystem.CalculateEnhanceCost(currentEnhance));
    }

    // ============================================================
    // TutorialSystem テスト
    // ============================================================

    [Fact]
    public void Tutorial_Initial_IsEnabled()
    {
        var system = new TutorialSystem();
        Assert.True(system.IsEnabled);
        Assert.False(system.IsComplete);
        Assert.Equal(0, system.CompletedCount);
        Assert.True(system.TotalSteps > 0);
    }

    [Fact]
    public void Tutorial_OnTrigger_ReturnsStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
        Assert.Equal("move", step.Id);
        Assert.Equal("移動", step.Title);
    }

    [Fact]
    public void Tutorial_OnTrigger_SameTriggerTwice_ReturnsNull()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        var second = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.Null(second);
    }

    [Fact]
    public void Tutorial_OnTrigger_Disabled_ReturnsNull()
    {
        var system = new TutorialSystem();
        system.IsEnabled = false;
        Assert.Null(system.OnTrigger(TutorialTrigger.GameStart));
    }

    [Fact]
    public void Tutorial_CompleteStep_MarksCompleted()
    {
        var system = new TutorialSystem();
        system.CompleteStep("move");
        Assert.True(system.IsStepCompleted("move"));
        Assert.Equal(1, system.CompletedCount);
    }

    [Fact]
    public void Tutorial_GetProgress_ReturnsCorrectValue()
    {
        var system = new TutorialSystem();
        Assert.Equal(0.0, system.GetProgress());
        system.OnTrigger(TutorialTrigger.GameStart);
        Assert.True(system.GetProgress() > 0);
    }

    [Fact]
    public void Tutorial_Reset_ClearsProgress()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        system.OnTrigger(TutorialTrigger.FirstEnemySight);
        system.Reset();
        Assert.Equal(0, system.CompletedCount);
        Assert.Equal(0.0, system.GetProgress());
    }

    [Fact]
    public void Tutorial_RestoreCompletedSteps_Restores()
    {
        var system = new TutorialSystem();
        system.RestoreCompletedSteps(new[] { "move", "attack", "inventory" });
        Assert.Equal(3, system.CompletedCount);
        Assert.True(system.IsStepCompleted("move"));
        Assert.True(system.IsStepCompleted("attack"));
    }

    [Fact]
    public void Tutorial_GetAllSteps_ReturnsAllSteps()
    {
        var system = new TutorialSystem();
        var steps = system.GetAllSteps();
        Assert.True(steps.Count >= 18);
    }

    [Fact]
    public void Tutorial_GetStepForTrigger_ReturnsHighestPriority()
    {
        var system = new TutorialSystem();
        var step = system.GetStepForTrigger(TutorialTrigger.FirstDeath);
        Assert.NotNull(step);
        Assert.Equal("death", step.Id);
    }

    [Fact]
    public void Tutorial_GetCompletedSteps_ReturnsSaveData()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        var completed = system.GetCompletedSteps();
        Assert.Contains("move", completed);
    }

    // ============================================================
    // BackgroundClearSystem テスト
    // ============================================================

    [Fact]
    public void BackgroundClear_SetFlag_And_HasFlag()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("dungeon_clear");
        Assert.True(system.HasFlag("dungeon_clear"));
        Assert.False(system.HasFlag("nonexistent"));
    }

    [Fact]
    public void BackgroundClear_IncrementFlag_Accumulates()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills", 5);
        system.IncrementFlag("boss_kills", 3);
        Assert.Equal(8, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void BackgroundClear_IsClearConditionMet_DungeonClear()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("dungeon_clear");
        Assert.True(system.IsClearConditionMet(Background.Adventurer, 1, 0));
    }

    [Fact]
    public void BackgroundClear_CreateAndRestoreSaveData()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("test_flag");
        system.IncrementFlag("counter", 5);
        var save = system.CreateSaveData();

        var restored = new BackgroundClearSystem();
        restored.RestoreFromSave(save);
        Assert.True(restored.HasFlag("test_flag"));
        Assert.Equal(5, restored.GetFlagValue("counter"));
    }
}
