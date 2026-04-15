using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.0.1 テストローンチフェーズのテスト
/// - U.1: チュートリアルシステム（トリガー、ステップ、進行度、セーブ/ロード）
/// - U.2: コンテキストヘルプシステム（トピック登録、カテゴリ別取得、キーバインド検索）
/// - U.3: ゲームオーバー統計情報
/// </summary>
public class TestLaunchVer01Tests
{
    #region U.1: TutorialSystem テスト

    [Fact]
    public void TutorialSystem_Initial_IsEnabled()
    {
        var system = new TutorialSystem();
        Assert.True(system.IsEnabled);
        Assert.False(system.IsComplete);
        Assert.Equal(18, system.TotalSteps);
        Assert.Equal(0, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_GameStart_ReturnsStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
        Assert.Equal("move", step.Id);
        Assert.Equal("移動", step.Title);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_SameTriggerTwice_ReturnsNull()
    {
        var system = new TutorialSystem();
        var step1 = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step1);

        var step2 = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.Null(step2); // 同じトリガーは1回のみ
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstEnemySight_ReturnsAttackStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstEnemySight);
        Assert.NotNull(step);
        Assert.Equal("attack", step.Id);
        Assert.Equal("攻撃", step.Title);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstItemPickup_ReturnsInventoryStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstItemPickup);
        Assert.NotNull(step);
        Assert.Equal("inventory", step.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstStairs_ReturnsStairsStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstStairs);
        Assert.NotNull(step);
        Assert.Equal("stairs", step.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstDeath_ReturnsDeathStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstDeath);
        Assert.NotNull(step);
        Assert.Equal("death", step.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstLevelUp_ReturnsSkillStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstLevelUp);
        Assert.NotNull(step);
        Assert.Equal("skill", step.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstShopVisit_ReturnsShopStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstShopVisit);
        Assert.NotNull(step);
        Assert.Equal("shop", step.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_FirstBossEncounter_ReturnsBossStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstBossEncounter);
        Assert.NotNull(step);
        Assert.Equal("boss", step.Id);
    }

    [Fact]
    public void TutorialSystem_CompletedCount_IncreasesOnTrigger()
    {
        var system = new TutorialSystem();
        Assert.Equal(0, system.CompletedCount);

        system.OnTrigger(TutorialTrigger.GameStart);
        Assert.Equal(1, system.CompletedCount);

        system.OnTrigger(TutorialTrigger.FirstEnemySight);
        Assert.Equal(2, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_GetProgress_CalculatesCorrectly()
    {
        var system = new TutorialSystem();
        Assert.Equal(0.0, system.GetProgress(), 2);

        system.OnTrigger(TutorialTrigger.GameStart);
        double expectedProgress = 1.0 / 18.0;
        Assert.Equal(expectedProgress, system.GetProgress(), 2);
    }

    [Fact]
    public void TutorialSystem_Disabled_OnTriggerReturnsNull()
    {
        var system = new TutorialSystem();
        system.IsEnabled = false;

        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.Null(step);
        Assert.Equal(0, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_Reset_ClearsAllProgress()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        system.OnTrigger(TutorialTrigger.FirstEnemySight);
        Assert.Equal(2, system.CompletedCount);

        system.Reset();
        Assert.Equal(0, system.CompletedCount);

        // リセット後は再びトリガーが発火する
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
    }

    [Fact]
    public void TutorialSystem_SaveRestore_PreservesProgress()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        system.OnTrigger(TutorialTrigger.FirstDeath);
        var completed = system.GetCompletedSteps().ToList();
        Assert.Equal(2, completed.Count);

        // 新しいシステムに復元
        var restored = new TutorialSystem();
        restored.RestoreCompletedSteps(completed);
        Assert.Equal(2, restored.CompletedCount);
        Assert.True(restored.IsStepCompleted("move"));
        Assert.True(restored.IsStepCompleted("death"));
    }

    [Fact]
    public void TutorialSystem_GetAllSteps_Returns18Steps()
    {
        var system = new TutorialSystem();
        var allSteps = system.GetAllSteps();
        Assert.Equal(18, allSteps.Count);
    }

    [Fact]
    public void TutorialSystem_AllTriggerTypes_HaveCorrespondingSteps()
    {
        var system = new TutorialSystem();
        var allTriggers = Enum.GetValues<TutorialTrigger>();

        foreach (var trigger in allTriggers)
        {
            var step = system.GetStepForTrigger(trigger);
            Assert.NotNull(step);
        }
    }

    [Fact]
    public void TutorialSystem_CompleteStep_MarksAsCompleted()
    {
        var system = new TutorialSystem();
        Assert.False(system.IsStepCompleted("move"));

        system.CompleteStep("move");
        Assert.True(system.IsStepCompleted("move"));
        Assert.Equal(1, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_AllTriggersComplete_IsCompleteTrue()
    {
        var system = new TutorialSystem();
        foreach (var trigger in Enum.GetValues<TutorialTrigger>())
        {
            system.OnTrigger(trigger);
        }
        Assert.True(system.IsComplete);
        Assert.Equal(1.0, system.GetProgress(), 2);
    }

    #endregion

    #region U.2: ContextHelpSystem テスト

    [Fact]
    public void ContextHelpSystem_RegisterDefaultTopics_Registers26Topics()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        Assert.Equal(26, system.Topics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Movement()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var movementTopics = system.GetTopicsByCategory(HelpCategory.Movement);
        Assert.Equal(4, movementTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Combat()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var combatTopics = system.GetTopicsByCategory(HelpCategory.Combat);
        Assert.Equal(5, combatTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Inventory()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var inventoryTopics = system.GetTopicsByCategory(HelpCategory.Inventory);
        Assert.Equal(4, inventoryTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Magic()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var magicTopics = system.GetTopicsByCategory(HelpCategory.Magic);
        Assert.Equal(2, magicTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Crafting()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var craftingTopics = system.GetTopicsByCategory(HelpCategory.Crafting);
        Assert.Equal(2, craftingTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Survival()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var survivalTopics = system.GetTopicsByCategory(HelpCategory.Survival);
        Assert.Equal(5, survivalTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_GetTopicsByCategory_Advanced()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var advancedTopics = system.GetTopicsByCategory(HelpCategory.Advanced);
        Assert.Equal(4, advancedTopics.Count);
    }

    [Fact]
    public void ContextHelpSystem_AllCategoriesExist()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        // 30 = 4(Movement) + 5(Combat) + 4(Inventory) + 2(Magic) + 2(Crafting) + 5(Survival) + 4(Advanced) + 4(Other) = 30
        // ただしOtherカテゴリのトピックはないので: 4+5+4+2+2+5+4 = 26 → 残りを確認
        int total = 0;
        foreach (var cat in Enum.GetValues<HelpCategory>())
        {
            total += system.GetTopicsByCategory(cat).Count;
        }
        // 全カテゴリの合計がTopics.Countと一致
        Assert.Equal(system.Topics.Count, total);
    }

    [Fact]
    public void ContextHelpSystem_GetHelpForKey_FindsExistingKey()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var help = system.GetHelpForKey("I");
        Assert.NotNull(help);
        Assert.Contains("インベントリ", help.Title);
    }

    [Fact]
    public void ContextHelpSystem_GetHelpForKey_ReturnsNullForUnknownKey()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var help = system.GetHelpForKey("X");
        Assert.Null(help);
    }

    [Fact]
    public void ContextHelpSystem_GetContextualHelp_ReturnsRelevantTopics()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        var results = system.GetContextualHelp("空腹");
        Assert.True(results.Count > 0);
        Assert.Contains(results, t => t.TopicId == "survival_hunger");
    }

    [Fact]
    public void ContextHelpSystem_GetCategoryName_ReturnsJapanese()
    {
        Assert.Equal("移動", ContextHelpSystem.GetCategoryName(HelpCategory.Movement));
        Assert.Equal("戦闘", ContextHelpSystem.GetCategoryName(HelpCategory.Combat));
        Assert.Equal("インベントリ", ContextHelpSystem.GetCategoryName(HelpCategory.Inventory));
        Assert.Equal("魔法", ContextHelpSystem.GetCategoryName(HelpCategory.Magic));
        Assert.Equal("クラフト", ContextHelpSystem.GetCategoryName(HelpCategory.Crafting));
        Assert.Equal("サバイバル", ContextHelpSystem.GetCategoryName(HelpCategory.Survival));
        Assert.Equal("上級テクニック", ContextHelpSystem.GetCategoryName(HelpCategory.Advanced));
    }

    [Fact]
    public void ContextHelpSystem_Tutorial_Enabled_InitialState()
    {
        var system = new ContextHelpSystem();
        Assert.True(system.TutorialEnabled);
        Assert.Equal(0, system.CurrentStep);
        Assert.Equal(0f, system.TutorialProgress);
    }

    [Fact]
    public void ContextHelpSystem_AddTutorialStep_IncreasesStepCount()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("テスト", "テスト説明", "test_condition");
        Assert.Equal(1, system.TutorialSteps.Count);
    }

    [Fact]
    public void ContextHelpSystem_CompleteTutorialStep_AdvancesStep()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("ステップ1", "説明1", "cond1");
        system.AddTutorialStep("ステップ2", "説明2", "cond2");

        var current = system.GetCurrentTutorial();
        Assert.NotNull(current);
        Assert.Equal("ステップ1", current.Title);

        var completed = system.CompleteTutorialStep();
        Assert.NotNull(completed);
        Assert.True(completed.IsCompleted);
        Assert.Equal(1, system.CurrentStep);

        var next = system.GetCurrentTutorial();
        Assert.NotNull(next);
        Assert.Equal("ステップ2", next.Title);
    }

    [Fact]
    public void ContextHelpSystem_SetTutorialEnabled_DisablesTutorial()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("テスト", "説明", "cond");

        system.SetTutorialEnabled(false);
        Assert.False(system.TutorialEnabled);

        var current = system.GetCurrentTutorial();
        Assert.Null(current);
    }

    [Fact]
    public void ContextHelpSystem_RegisterTopic_OverwritesExisting()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("test", HelpCategory.Movement, "テスト1", "内容1");
        system.RegisterTopic("test", HelpCategory.Movement, "テスト2", "内容2");

        Assert.Equal(1, system.Topics.Count);
        Assert.Equal("テスト2", system.Topics["test"].Title);
    }

    [Fact]
    public void ContextHelpSystem_TopicsSortedByPriority()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("low", HelpCategory.Combat, "低優先", "内容", priority: 1);
        system.RegisterTopic("high", HelpCategory.Combat, "高優先", "内容", priority: 10);
        system.RegisterTopic("mid", HelpCategory.Combat, "中優先", "内容", priority: 5);

        var topics = system.GetTopicsByCategory(HelpCategory.Combat);
        Assert.Equal("high", topics[0].TopicId);
        Assert.Equal("mid", topics[1].TopicId);
        Assert.Equal("low", topics[2].TopicId);
    }

    #endregion

    #region SaveData テスト

    [Fact]
    public void SaveData_TotalEnemiesDefeated_DefaultZero()
    {
        var saveData = new SaveData();
        Assert.Equal(0, saveData.TotalEnemiesDefeated);
    }

    [Fact]
    public void SaveData_DeepestFloorReached_DefaultZero()
    {
        var saveData = new SaveData();
        Assert.Equal(0, saveData.DeepestFloorReached);
    }

    [Fact]
    public void SaveData_TotalEnemiesDefeated_CanBeSet()
    {
        var saveData = new SaveData();
        saveData.TotalEnemiesDefeated = 42;
        Assert.Equal(42, saveData.TotalEnemiesDefeated);
    }

    [Fact]
    public void SaveData_DeepestFloorReached_CanBeSet()
    {
        var saveData = new SaveData();
        saveData.DeepestFloorReached = 15;
        Assert.Equal(15, saveData.DeepestFloorReached);
    }

    #endregion
}
