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

    #region U.5: アクセシビリティ改善テスト

    [Fact]
    public void GameSettings_ColorBlindMode_DefaultIsNone()
    {
        var settings = GameSettings.CreateDefault();
        Assert.Equal(ColorBlindMode.None, settings.ColorBlindMode);
    }

    [Fact]
    public void GameSettings_HighContrastMode_DefaultIsFalse()
    {
        var settings = GameSettings.CreateDefault();
        Assert.False(settings.HighContrastMode);
    }

    [Fact]
    public void GameSettings_GameSpeedMultiplier_DefaultIsOne()
    {
        var settings = GameSettings.CreateDefault();
        Assert.Equal(1.0f, settings.GameSpeedMultiplier);
    }

    [Fact]
    public void GameSettings_ScreenReaderMode_DefaultIsFalse()
    {
        var settings = GameSettings.CreateDefault();
        Assert.False(settings.ScreenReaderMode);
    }

    [Fact]
    public void GameSettings_LargePointer_DefaultIsFalse()
    {
        var settings = GameSettings.CreateDefault();
        Assert.False(settings.LargePointer);
    }

    [Fact]
    public void GameSettings_Validate_ClampsGameSpeed()
    {
        var settings = new GameSettings { GameSpeedMultiplier = 5.0f };
        settings.Validate();
        Assert.Equal(2.0f, settings.GameSpeedMultiplier);
    }

    [Fact]
    public void GameSettings_Validate_ClampsGameSpeed_Low()
    {
        var settings = new GameSettings { GameSpeedMultiplier = 0.1f };
        settings.Validate();
        Assert.Equal(0.25f, settings.GameSpeedMultiplier);
    }

    [Fact]
    public void GameSettings_Validate_InvalidColorBlindMode_ResetsToDefault()
    {
        var settings = new GameSettings { ColorBlindMode = (ColorBlindMode)99 };
        settings.Validate();
        Assert.Equal(ColorBlindMode.None, settings.ColorBlindMode);
    }

    [Fact]
    public void GameSettings_Clone_IncludesAccessibilityProperties()
    {
        var settings = new GameSettings
        {
            ColorBlindMode = ColorBlindMode.Protanopia,
            HighContrastMode = true,
            GameSpeedMultiplier = 1.5f,
            ScreenReaderMode = true,
            LargePointer = true
        };
        var clone = settings.Clone();
        Assert.Equal(ColorBlindMode.Protanopia, clone.ColorBlindMode);
        Assert.True(clone.HighContrastMode);
        Assert.Equal(1.5f, clone.GameSpeedMultiplier);
        Assert.True(clone.ScreenReaderMode);
        Assert.True(clone.LargePointer);
    }

    [Fact]
    public void AccessibilitySystem_ApplyFromGameSettings_SetsColorMode()
    {
        var system = new AccessibilitySystem();
        var settings = new GameSettings { ColorBlindMode = ColorBlindMode.Deuteranopia };
        system.ApplyFromGameSettings(settings);
        Assert.Equal(ColorBlindMode.Deuteranopia, system.Config.ColorMode);
    }

    [Fact]
    public void AccessibilitySystem_ApplyFromGameSettings_SetsHighContrast()
    {
        var system = new AccessibilitySystem();
        var settings = new GameSettings { HighContrastMode = true };
        system.ApplyFromGameSettings(settings);
        Assert.True(system.Config.HighContrastMode);
    }

    [Fact]
    public void AccessibilitySystem_ApplyFromGameSettings_SetsGameSpeed()
    {
        var system = new AccessibilitySystem();
        var settings = new GameSettings { GameSpeedMultiplier = 0.5f };
        system.ApplyFromGameSettings(settings);
        Assert.Equal(0.5f, system.Config.GameSpeedMultiplier);
    }

    [Fact]
    public void AccessibilitySystem_ApplyFromGameSettings_SetsScreenReader()
    {
        var system = new AccessibilitySystem();
        var settings = new GameSettings { ScreenReaderMode = true };
        system.ApplyFromGameSettings(settings);
        Assert.True(system.Config.ScreenReaderMode);
    }

    [Fact]
    public void AccessibilitySystem_ApplyFromGameSettings_SetsLargePointer()
    {
        var system = new AccessibilitySystem();
        var settings = new GameSettings { LargePointer = true };
        system.ApplyFromGameSettings(settings);
        Assert.True(system.Config.LargePointer);
    }

    [Fact]
    public void AccessibilitySystem_TransformColor_Protanopia_RedToDarkYellow()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Protanopia);
        var result = system.TransformColor("Red");
        Assert.Equal("DarkYellow", result.TransformedColor);
    }

    [Fact]
    public void AccessibilitySystem_TransformColor_Deuteranopia_GreenToOrange()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Deuteranopia);
        var result = system.TransformColor("Green");
        Assert.Equal("Orange", result.TransformedColor);
    }

    [Fact]
    public void AccessibilitySystem_TransformColor_Tritanopia_BlueToSyan()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Tritanopia);
        var result = system.TransformColor("Blue");
        Assert.Equal("Cyan", result.TransformedColor);
    }

    [Fact]
    public void AccessibilitySystem_TransformColor_Monochrome_AnyToGray()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Monochrome);
        var result = system.TransformColor("Red");
        Assert.Equal("Gray", result.TransformedColor);
    }

    [Fact]
    public void AccessibilitySystem_CalculateEffectiveFontSize_WithMultiplier()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(2.0f);
        Assert.Equal(28, system.CalculateEffectiveFontSize(14));
    }

    [Fact]
    public void AccessibilitySystem_CalculateEffectiveTurnDelay_WithSpeed()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(2.0f);
        Assert.Equal(100, system.CalculateEffectiveTurnDelay(200));
    }

    [Fact]
    public void AccessibilitySystem_GetModeName_AllModes()
    {
        Assert.Equal("通常", AccessibilitySystem.GetModeName(ColorBlindMode.None));
        Assert.Equal("1型色覚（P型）", AccessibilitySystem.GetModeName(ColorBlindMode.Protanopia));
        Assert.Equal("2型色覚（D型）", AccessibilitySystem.GetModeName(ColorBlindMode.Deuteranopia));
        Assert.Equal("3型色覚（T型）", AccessibilitySystem.GetModeName(ColorBlindMode.Tritanopia));
        Assert.Equal("モノクロ", AccessibilitySystem.GetModeName(ColorBlindMode.Monochrome));
    }

    #endregion

    #region T.2: エッジケース対応テスト

    [Fact]
    public void SaveData_NegativeValues_AreClamped()
    {
        var data = new SaveData
        {
            CurrentFloor = -5,
            TurnCount = -10,
            TotalDeaths = -1,
            TotalEnemiesDefeated = -100,
            DeepestFloorReached = -3,
            BankBalance = -999,
            GuildPoints = -50,
            InfiniteDungeonKills = -1
        };
        data.Validate();
        Assert.Equal(0, data.CurrentFloor);
        Assert.Equal(0, data.TurnCount);
        Assert.Equal(0, data.TotalDeaths);
        Assert.Equal(0, data.TotalEnemiesDefeated);
        Assert.Equal(0, data.DeepestFloorReached);
        Assert.Equal(0, data.BankBalance);
        Assert.Equal(0, data.GuildPoints);
        Assert.Equal(0, data.InfiniteDungeonKills);
    }

    [Fact]
    public void SaveData_PlayerLevel_ClampedToValid()
    {
        var data = new SaveData();
        data.Player.Level = 0;
        data.Validate();
        Assert.Equal(1, data.Player.Level);

        data.Player.Level = 999;
        data.Validate();
        Assert.Equal(99, data.Player.Level);
    }

    [Fact]
    public void SaveData_NullPlayer_CreatesDefault()
    {
        var data = new SaveData { Player = null! };
        data.Validate();
        Assert.NotNull(data.Player);
    }

    [Fact]
    public void SaveData_NullCollections_InitializedToEmpty()
    {
        var data = new SaveData
        {
            MessageHistory = null!,
            ActiveQuests = null!,
            CompletedQuests = null!,
            SkillCooldowns = null!,
            VisitedTerritories = null!,
            NpcStates = null!,
            KarmaHistory = null!,
            ProficiencyLevels = null!,
            ProficiencyExp = null!,
            DialogueFlags = null!
        };
        data.Validate();
        Assert.NotNull(data.MessageHistory);
        Assert.NotNull(data.ActiveQuests);
        Assert.NotNull(data.CompletedQuests);
        Assert.NotNull(data.SkillCooldowns);
        Assert.NotNull(data.VisitedTerritories);
        Assert.NotNull(data.NpcStates);
        Assert.NotNull(data.KarmaHistory);
        Assert.NotNull(data.ProficiencyLevels);
        Assert.NotNull(data.ProficiencyExp);
        Assert.NotNull(data.DialogueFlags);
    }

    [Fact]
    public void SaveData_PlayerStats_NegativeValuesFixed()
    {
        var data = new SaveData();
        data.Player.CurrentHp = -10;
        data.Player.CurrentMp = -5;
        data.Player.CurrentSp = -3;
        data.Player.Gold = -100;
        data.Player.Sanity = -20;
        data.Player.Hunger = -10;
        data.Player.Thirst = -5;
        data.Validate();
        Assert.Equal(0, data.Player.CurrentHp);
        Assert.Equal(0, data.Player.CurrentMp);
        Assert.Equal(0, data.Player.CurrentSp);
        Assert.Equal(0, data.Player.Gold);
        Assert.Equal(0, data.Player.Sanity);
        Assert.Equal(0, data.Player.Hunger);
        Assert.Equal(0, data.Player.Thirst);
    }

    [Fact]
    public void SaveData_PlayerStats_OverflowValuesFixed()
    {
        var data = new SaveData();
        data.Player.Sanity = 200;
        data.Player.Hunger = 150;
        data.Player.Thirst = 300;
        data.Validate();
        Assert.Equal(100, data.Player.Sanity);
        Assert.Equal(100, data.Player.Hunger);
        Assert.Equal(100, data.Player.Thirst);
    }

    [Fact]
    public void AccessibilitySystem_FontSizeMultiplier_ClampsToRange()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(10.0f);
        Assert.Equal(3.0f, system.Config.FontSizeMultiplier);

        system.SetFontSizeMultiplier(-1.0f);
        Assert.Equal(0.5f, system.Config.FontSizeMultiplier);
    }

    [Fact]
    public void AccessibilitySystem_GameSpeedMultiplier_ClampsToRange()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(10.0f);
        Assert.Equal(2.0f, system.Config.GameSpeedMultiplier);

        system.SetGameSpeedMultiplier(0.01f);
        Assert.Equal(0.25f, system.Config.GameSpeedMultiplier);
    }

    [Fact]
    public void AccessibilitySystem_ResetToDefaults_RestoresAll()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Protanopia);
        system.SetHighContrastMode(true);
        system.SetFontSizeMultiplier(2.0f);
        system.SetGameSpeedMultiplier(0.5f);
        system.ResetToDefaults();
        Assert.Equal(ColorBlindMode.None, system.Config.ColorMode);
        Assert.False(system.Config.HighContrastMode);
        Assert.Equal(1.0f, system.Config.FontSizeMultiplier);
        Assert.Equal(1.0f, system.Config.GameSpeedMultiplier);
    }

    [Fact]
    public void AccessibilitySystem_TransformColor_None_ReturnsOriginal()
    {
        var system = new AccessibilitySystem();
        var result = system.TransformColor("Red");
        Assert.Equal("Red", result.TransformedColor);
        Assert.Equal(ColorBlindMode.None, result.Mode);
    }

    #endregion
}
