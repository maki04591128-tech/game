using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Phase5 システム拡張テスト - 12システムの網羅的テスト (150+メソッド)
/// </summary>
public class SystemExpansionPhase5Tests
{
    // ========================================================================
    // BaseConstructionSystem Tests
    // ========================================================================

    #region BaseConstructionSystem - GetDefinition

    [Theory]
    [InlineData(FacilityCategory.Camp, "キャンプ", 10)]
    [InlineData(FacilityCategory.Workbench, "作業台", 30)]
    [InlineData(FacilityCategory.Smithy, "鍛冶場", 80)]
    [InlineData(FacilityCategory.Storage, "倉庫", 50)]
    [InlineData(FacilityCategory.Farm, "畑", 40)]
    [InlineData(FacilityCategory.Barricade, "防壁", 60)]
    [InlineData(FacilityCategory.Barracks, "宿舎", 70)]
    public void Base_GetDefinition_ReturnsCorrectNameAndCost(FacilityCategory cat, string name, int cost)
    {
        var def = BaseConstructionSystem.GetDefinition(cat);
        Assert.NotNull(def);
        Assert.Equal(name, def!.Name);
        Assert.Equal(cost, def.MaterialCost);
    }

    [Fact]
    public void Base_GetDefinition_InvalidCategory_ReturnsNull()
    {
        var def = BaseConstructionSystem.GetDefinition((FacilityCategory)999);
        Assert.Null(def);
    }

    #endregion

    #region BaseConstructionSystem - CanBuild

    [Fact]
    public void Base_CanBuild_SufficientMaterials_ReturnsTrue()
    {
        var sys = new BaseConstructionSystem();
        Assert.True(sys.CanBuild(FacilityCategory.Camp, 10));
        Assert.True(sys.CanBuild(FacilityCategory.Camp, 100));
    }

    [Fact]
    public void Base_CanBuild_InsufficientMaterials_ReturnsFalse()
    {
        var sys = new BaseConstructionSystem();
        Assert.False(sys.CanBuild(FacilityCategory.Camp, 9));
        Assert.False(sys.CanBuild(FacilityCategory.Smithy, 79));
    }

    [Fact]
    public void Base_CanBuild_AlreadyBuilt_ReturnsFalse()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        Assert.False(sys.CanBuild(FacilityCategory.Camp, 100));
    }

    [Fact]
    public void Base_CanBuild_ExactMaterialCost_ReturnsTrue()
    {
        var sys = new BaseConstructionSystem();
        Assert.True(sys.CanBuild(FacilityCategory.Barricade, 60));
    }

    #endregion

    #region BaseConstructionSystem - Build and HasFacility

    [Fact]
    public void Base_Build_Success_AddsToBuiltFacilities()
    {
        var sys = new BaseConstructionSystem();
        Assert.True(sys.Build(FacilityCategory.Farm, 100));
        Assert.True(sys.HasFacility(FacilityCategory.Farm));
        Assert.Contains(FacilityCategory.Farm, sys.BuiltFacilities);
    }

    [Fact]
    public void Base_Build_Failure_DoesNotAdd()
    {
        var sys = new BaseConstructionSystem();
        Assert.False(sys.Build(FacilityCategory.Smithy, 5));
        Assert.False(sys.HasFacility(FacilityCategory.Smithy));
    }

    [Fact]
    public void Base_Build_MultipleFacilities()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        sys.Build(FacilityCategory.Storage, 100);
        sys.Build(FacilityCategory.Farm, 100);
        Assert.Equal(3, sys.BuiltFacilities.Count);
    }

    [Fact]
    public void Base_HasFacility_NothingBuilt_ReturnsFalse()
    {
        var sys = new BaseConstructionSystem();
        Assert.False(sys.HasFacility(FacilityCategory.Camp));
    }

    #endregion

    #region BaseConstructionSystem - Defense and Bonuses

    [Fact]
    public void Base_CalculateDefenseRating_NoFacilities_Zero()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(0, sys.CalculateDefenseRating());
    }

    [Fact]
    public void Base_CalculateDefenseRating_Barricade_30()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Barricade, 100);
        Assert.Equal(30, sys.CalculateDefenseRating());
    }

    [Fact]
    public void Base_CalculateDefenseRating_AllDefensive_45()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Barricade, 100);
        sys.Build(FacilityCategory.Barracks, 100);
        sys.Build(FacilityCategory.Camp, 100);
        Assert.Equal(45, sys.CalculateDefenseRating());
    }

    [Fact]
    public void Base_GetTotalBonus_NoFacilities_DefaultValues()
    {
        var sys = new BaseConstructionSystem();
        var bonus = sys.GetTotalBonus();
        Assert.Equal(1.0f, bonus.HpRecoveryMultiplier);
        Assert.Equal(0f, bonus.CraftingSuccessBonus);
        Assert.Equal(0, bonus.ExtraStorageSlots);
        Assert.Equal(0, bonus.FoodProductionPerDay);
        Assert.Equal(0, bonus.DefenseBonus);
        Assert.Equal(0, bonus.ExtraCompanionSlots);
    }

    [Fact]
    public void Base_GetTotalBonus_AllFacilities_CumulativeEffects()
    {
        var sys = new BaseConstructionSystem();
        foreach (var cat in new[] { FacilityCategory.Camp, FacilityCategory.Workbench, FacilityCategory.Smithy,
            FacilityCategory.Storage, FacilityCategory.Farm, FacilityCategory.Barricade, FacilityCategory.Barracks })
        {
            sys.Build(cat, 200);
        }
        var bonus = sys.GetTotalBonus();
        Assert.Equal(1.75f, bonus.HpRecoveryMultiplier, 2);
        Assert.Equal(0.3f, bonus.CraftingSuccessBonus, 2);
        Assert.Equal(50, bonus.ExtraStorageSlots);
        Assert.Equal(3, bonus.FoodProductionPerDay);
        Assert.Equal(30, bonus.DefenseBonus);
        Assert.Equal(2, bonus.ExtraCompanionSlots);
    }

    [Fact]
    public void Base_GetRestHpRecoveryMultiplier_Base_1()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(1.0f, sys.GetRestHpRecoveryMultiplier());
    }

    [Fact]
    public void Base_GetRestHpRecoveryMultiplier_CampOnly_1_25()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        Assert.Equal(1.25f, sys.GetRestHpRecoveryMultiplier(), 2);
    }

    [Fact]
    public void Base_GetRestHpRecoveryMultiplier_CampAndBarracks_1_75()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        sys.Build(FacilityCategory.Barracks, 100);
        Assert.Equal(1.75f, sys.GetRestHpRecoveryMultiplier(), 2);
    }

    [Fact]
    public void Base_GetCraftingSuccessBonus_Base_0()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(0f, sys.GetCraftingSuccessBonus());
    }

    [Fact]
    public void Base_GetCraftingSuccessBonus_WorkbenchOnly_0_1()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Workbench, 100);
        Assert.Equal(0.1f, sys.GetCraftingSuccessBonus(), 2);
    }

    [Fact]
    public void Base_GetCraftingSuccessBonus_WorkbenchAndSmithy_0_3()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Workbench, 100);
        sys.Build(FacilityCategory.Smithy, 100);
        Assert.Equal(0.3f, sys.GetCraftingSuccessBonus(), 2);
    }

    [Fact]
    public void Base_GetExtraStorageSlots_NoStorage_0()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(0, sys.GetExtraStorageSlots());
    }

    [Fact]
    public void Base_GetExtraStorageSlots_WithStorage_50()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Storage, 100);
        Assert.Equal(50, sys.GetExtraStorageSlots());
    }

    [Fact]
    public void Base_GetDailyFoodProduction_NoFarm_0()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(0, sys.GetDailyFoodProduction());
    }

    [Fact]
    public void Base_GetDailyFoodProduction_WithFarm_3()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Farm, 100);
        Assert.Equal(3, sys.GetDailyFoodProduction());
    }

    [Fact]
    public void Base_GetExtraCompanionSlots_NoBarracks_0()
    {
        var sys = new BaseConstructionSystem();
        Assert.Equal(0, sys.GetExtraCompanionSlots());
    }

    [Fact]
    public void Base_GetExtraCompanionSlots_WithBarracks_2()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Barracks, 100);
        Assert.Equal(2, sys.GetExtraCompanionSlots());
    }

    #endregion

    #region BaseConstructionSystem - Reset

    [Fact]
    public void Base_Reset_ClearsAllFacilities()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        sys.Build(FacilityCategory.Farm, 100);
        sys.Reset();
        Assert.Empty(sys.BuiltFacilities);
        Assert.False(sys.HasFacility(FacilityCategory.Camp));
    }

    [Fact]
    public void Base_Reset_CanRebuildAfterReset()
    {
        var sys = new BaseConstructionSystem();
        sys.Build(FacilityCategory.Camp, 100);
        sys.Reset();
        Assert.True(sys.Build(FacilityCategory.Camp, 100));
    }

    #endregion

    // ========================================================================
    // NewGamePlusSystem Tests
    // ========================================================================

    #region NewGamePlusSystem - GetConfig

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, 1.5f, 1.2f)]
    [InlineData(NewGamePlusTier.Plus2, 2.0f, 1.5f)]
    [InlineData(NewGamePlusTier.Plus3, 2.5f, 1.8f)]
    [InlineData(NewGamePlusTier.Plus4, 3.0f, 2.0f)]
    [InlineData(NewGamePlusTier.Plus5, 4.0f, 2.5f)]
    public void NGPlus_GetConfig_ReturnsCorrectMultipliers(NewGamePlusTier tier, float enemy, float exp)
    {
        var cfg = NewGamePlusSystem.GetConfig(tier);
        Assert.NotNull(cfg);
        Assert.Equal(enemy, cfg!.EnemyMultiplier);
        Assert.Equal(exp, cfg.ExpMultiplier);
    }

    [Fact]
    public void NGPlus_GetConfig_InvalidTier_ReturnsNull()
    {
        var cfg = NewGamePlusSystem.GetConfig((NewGamePlusTier)999);
        Assert.Null(cfg);
    }

    [Theory]
    [InlineData(NewGamePlusTier.Plus3, true)]
    [InlineData(NewGamePlusTier.Plus4, true)]
    [InlineData(NewGamePlusTier.Plus5, true)]
    [InlineData(NewGamePlusTier.Plus1, false)]
    [InlineData(NewGamePlusTier.Plus2, false)]
    public void NGPlus_GetConfig_SpecialContentUnlock(NewGamePlusTier tier, bool unlocks)
    {
        var cfg = NewGamePlusSystem.GetConfig(tier);
        Assert.Equal(unlocks, cfg!.UnlocksSpecialContent);
    }

    #endregion

    #region NewGamePlusSystem - CanStartNewGamePlus

    [Theory]
    [InlineData(true, "S", true)]
    [InlineData(true, "A", true)]
    [InlineData(true, "B", true)]
    [InlineData(true, "C", false)]
    [InlineData(true, "D", false)]
    [InlineData(false, "S", false)]
    [InlineData(false, "A", false)]
    public void NGPlus_CanStartNewGamePlus_VariousConditions(bool hasCleared, string rank, bool expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.CanStartNewGamePlus(hasCleared, rank));
    }

    #endregion

    #region NewGamePlusSystem - GetCarryOverItems

    [Fact]
    public void NGPlus_GetCarryOverItems_Plus1_BaseItems()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus1);
        Assert.Equal(3, items.Count);
        Assert.Contains("レベル", items);
        Assert.Contains("スキル", items);
        Assert.Contains("図鑑データ", items);
    }

    [Fact]
    public void NGPlus_GetCarryOverItems_Plus2_IncludesEquipment()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus2);
        Assert.Equal(4, items.Count);
        Assert.Contains("装備品", items);
    }

    [Fact]
    public void NGPlus_GetCarryOverItems_Plus3_IncludesGold()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus3);
        Assert.Equal(5, items.Count);
        Assert.Contains("ゴールド", items);
    }

    [Fact]
    public void NGPlus_GetCarryOverItems_Plus4_IncludesAll()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus4);
        Assert.Equal(6, items.Count);
        Assert.Contains("全アイテム", items);
    }

    [Fact]
    public void NGPlus_GetCarryOverItems_Plus5_SameAsPlus4()
    {
        var items = NewGamePlusSystem.GetCarryOverItems(NewGamePlusTier.Plus5);
        Assert.Equal(6, items.Count);
    }

    #endregion

    #region NewGamePlusSystem - GetTierName / GetNextTier

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, "NG+1")]
    [InlineData(NewGamePlusTier.Plus2, "NG+2")]
    [InlineData(NewGamePlusTier.Plus3, "NG+3")]
    [InlineData(NewGamePlusTier.Plus4, "NG+4")]
    [InlineData(NewGamePlusTier.Plus5, "NG+5")]
    public void NGPlus_GetTierName_ReturnsExpected(NewGamePlusTier tier, string expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.GetTierName(tier));
    }

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, NewGamePlusTier.Plus2)]
    [InlineData(NewGamePlusTier.Plus2, NewGamePlusTier.Plus3)]
    [InlineData(NewGamePlusTier.Plus3, NewGamePlusTier.Plus4)]
    [InlineData(NewGamePlusTier.Plus4, NewGamePlusTier.Plus5)]
    [InlineData(NewGamePlusTier.Plus5, NewGamePlusTier.Plus5)]
    public void NGPlus_GetNextTier_ReturnsExpected(NewGamePlusTier current, NewGamePlusTier expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.GetNextTier(current));
    }

    #endregion

    #region NewGamePlusSystem - Multipliers and Messages

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, 1.5f)]
    [InlineData(NewGamePlusTier.Plus5, 4.0f)]
    public void NGPlus_GetEnemyStatMultiplier_ReturnsConfigValue(NewGamePlusTier tier, float expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.GetEnemyStatMultiplier(tier));
    }

    [Theory]
    [InlineData(NewGamePlusTier.Plus1, 1.2f)]
    [InlineData(NewGamePlusTier.Plus5, 2.5f)]
    public void NGPlus_GetExpMultiplier_ReturnsConfigValue(NewGamePlusTier tier, float expected)
    {
        Assert.Equal(expected, NewGamePlusSystem.GetExpMultiplier(tier));
    }

    [Theory]
    [InlineData("S")]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("D")]
    public void NGPlus_DetermineInitialTier_AlwaysPlus1(string rank)
    {
        Assert.Equal(NewGamePlusTier.Plus1, NewGamePlusSystem.DetermineInitialTier(rank));
    }

    [Fact]
    public void NGPlus_GetStartMessage_ContainsTierNameAndMultipliers()
    {
        var msg = NewGamePlusSystem.GetStartMessage(NewGamePlusTier.Plus1);
        Assert.Contains("NG+1", msg);
        Assert.Contains("1.5", msg);
        Assert.Contains("1.2", msg);
    }

    [Fact]
    public void NGPlus_GetStartMessage_InvalidTier_DefaultMessage()
    {
        var msg = NewGamePlusSystem.GetStartMessage((NewGamePlusTier)999);
        Assert.Equal("NG+を開始します", msg);
    }

    #endregion

    // ========================================================================
    // GameClearSystem Tests
    // ========================================================================

    #region GameClearSystem - GetClearCondition

    [Theory]
    [InlineData(Background.Noble, "王都の復興を成し遂げる")]
    [InlineData(Background.Merchant, "莫大な富を築く")]
    [InlineData(Background.Criminal, "全ての罪を贖う")]
    [InlineData(Background.Scholar, "究極の知識を得る")]
    [InlineData(Background.Soldier, "魔王を討伐する")]
    [InlineData(Background.Peasant, "豊かな大地を取り戻す")]
    [InlineData(Background.Priest, "聖地を巡礼する")]
    [InlineData(Background.Penitent, "全ての罪を贖い救済を得る")]
    [InlineData(Background.Wanderer, "真の故郷を見つける")]
    [InlineData(Background.Adventurer, "伝説の冒険者となる")]
    public void Clear_GetClearCondition_AllBackgrounds(Background bg, string expected)
    {
        Assert.Equal(expected, GameClearSystem.GetClearCondition(bg));
    }

    #endregion

    #region GameClearSystem - GetClearRank

    [Theory]
    [InlineData(4999, 0, "S")]
    [InlineData(5000, 0, "A")]
    [InlineData(9999, 3, "A")]
    [InlineData(10000, 3, "B")]
    [InlineData(19999, 10, "B")]
    [InlineData(20000, 10, "C")]
    [InlineData(49999, 100, "C")]
    [InlineData(50000, 100, "D")]
    public void Clear_GetClearRank_BoundaryValues(int turns, int deaths, string expected)
    {
        Assert.Equal(expected, GameClearSystem.GetClearRank(turns, deaths));
    }

    [Fact]
    public void Clear_GetClearRank_PerfectRun_S()
    {
        Assert.Equal("S", GameClearSystem.GetClearRank(1000, 0));
    }

    [Fact]
    public void Clear_GetClearRank_WorstCase_D()
    {
        Assert.Equal("D", GameClearSystem.GetClearRank(100000, 50));
    }

    #endregion

    #region GameClearSystem - GetClearMessage and UnlocksNewGamePlus

    [Fact]
    public void Clear_GetClearMessage_FormattedCorrectly()
    {
        var msg = GameClearSystem.GetClearMessage("兵士", "S");
        Assert.Contains("兵士", msg);
        Assert.Contains("S", msg);
        Assert.Contains("ランク", msg);
    }

    [Theory]
    [InlineData("S", true)]
    [InlineData("A", true)]
    [InlineData("B", true)]
    [InlineData("C", false)]
    [InlineData("D", false)]
    public void Clear_UnlocksNewGamePlus_RankBasedUnlock(string rank, bool expected)
    {
        Assert.Equal(expected, GameClearSystem.UnlocksNewGamePlus(rank));
    }

    #endregion

    #region GameClearSystem - CalculateScore

    [Fact]
    public void Clear_CalculateScore_FastNoDeathHighLevel()
    {
        var score = GameClearSystem.CalculateScore(1000, 0, 50, 30);
        Assert.True(score.TotalScore > 0);
        Assert.Equal("S", score.Rank);
        Assert.Equal(0, score.DeathPenalty);
    }

    [Fact]
    public void Clear_CalculateScore_DeathPenaltyApplied()
    {
        var score = GameClearSystem.CalculateScore(1000, 5, 10, 10);
        Assert.Equal(2500, score.DeathPenalty);
    }

    [Fact]
    public void Clear_CalculateScore_LevelBonusCalculation()
    {
        var score = GameClearSystem.CalculateScore(1000, 0, 30, 0);
        Assert.Equal(6000, score.LevelBonus);
    }

    [Fact]
    public void Clear_CalculateScore_FloorBonusCalculation()
    {
        var score = GameClearSystem.CalculateScore(1000, 0, 0, 25);
        Assert.Equal(2500, score.FloorBonus);
    }

    [Fact]
    public void Clear_CalculateScore_NeverNegative()
    {
        var score = GameClearSystem.CalculateScore(100000, 100, 1, 1);
        Assert.True(score.TotalScore >= 0);
    }

    #endregion

    #region GameClearSystem - IsFinalBossDefeated and GetClearText

    [Theory]
    [InlineData(30, "floor_boss_30", true)]
    [InlineData(31, "floor_boss_30", true)]
    [InlineData(29, "floor_boss_30", false)]
    [InlineData(30, "floor_boss_20", false)]
    [InlineData(30, "", false)]
    public void Clear_IsFinalBossDefeated_Conditions(int floor, string enemyId, bool expected)
    {
        Assert.Equal(expected, GameClearSystem.IsFinalBossDefeated(floor, enemyId));
    }

    [Theory]
    [InlineData(Background.Noble)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Scholar)]
    [InlineData(Background.Soldier)]
    [InlineData(Background.Peasant)]
    [InlineData(Background.Priest)]
    [InlineData(Background.Penitent)]
    [InlineData(Background.Wanderer)]
    [InlineData(Background.Adventurer)]
    public void Clear_GetClearText_NotEmpty(Background bg)
    {
        var text = GameClearSystem.GetClearText(bg);
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    #endregion

    // ========================================================================
    // GameOverSystem Tests
    // ========================================================================

    #region GameOverSystem - GetChoiceText

    [Theory]
    [InlineData(GameOverSystem.GameOverChoice.Rebirth, "死に戻る")]
    [InlineData(GameOverSystem.GameOverChoice.ReturnToTitle, "タイトル画面")]
    [InlineData(GameOverSystem.GameOverChoice.Quit, "ゲームを終了")]
    public void GameOver_GetChoiceText_AllChoices(GameOverSystem.GameOverChoice choice, string contains)
    {
        var text = GameOverSystem.GetChoiceText(choice);
        Assert.Contains(contains, text);
    }

    #endregion

    #region GameOverSystem - CanRebirth

    [Theory]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void GameOver_CanRebirth_SanityBased(int sanity, bool expected)
    {
        Assert.Equal(expected, GameOverSystem.CanRebirth(sanity));
    }

    #endregion

    #region GameOverSystem - GetAvailableChoices

    [Fact]
    public void GameOver_GetAvailableChoices_WithSanity_AllAvailable()
    {
        var choices = GameOverSystem.GetAvailableChoices(10);
        Assert.Equal(3, choices.Count);
        Assert.True(choices[0].Available); // Rebirth
        Assert.True(choices[1].Available); // ReturnToTitle
        Assert.True(choices[2].Available); // Quit
    }

    [Fact]
    public void GameOver_GetAvailableChoices_ZeroSanity_RebirthUnavailable()
    {
        var choices = GameOverSystem.GetAvailableChoices(0);
        Assert.False(choices[0].Available); // Rebirth
        Assert.True(choices[1].Available);
        Assert.True(choices[2].Available);
    }

    #endregion

    #region GameOverSystem - GetGameOverMessage

    [Fact]
    public void GameOver_GetGameOverMessage_ContainsCauseDetail()
    {
        var msg = GameOverSystem.GetGameOverMessage("戦闘中の致命傷");
        Assert.Contains("戦闘中の致命傷", msg);
        Assert.Contains("命を落とした", msg);
    }

    #endregion

    #region GameOverSystem - ProcessChoice

    [Fact]
    public void GameOver_ProcessChoice_Rebirth_WithSanity()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Rebirth, 10);
        Assert.True(result.ShouldRebirth);
        Assert.False(result.ShouldReturnToTitle);
        Assert.False(result.ShouldQuitGame);
    }

    [Fact]
    public void GameOver_ProcessChoice_Rebirth_NoSanity()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Rebirth, 0);
        Assert.False(result.ShouldRebirth);
        Assert.Contains("正気度が足りない", result.Message);
    }

    [Fact]
    public void GameOver_ProcessChoice_ReturnToTitle()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.ReturnToTitle, 0);
        Assert.True(result.ShouldReturnToTitle);
        Assert.False(result.ShouldRebirth);
        Assert.False(result.ShouldQuitGame);
    }

    [Fact]
    public void GameOver_ProcessChoice_Quit()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Quit, 0);
        Assert.True(result.ShouldQuitGame);
        Assert.False(result.ShouldReturnToTitle);
        Assert.False(result.ShouldRebirth);
    }

    #endregion

    #region GameOverSystem - GetDeathCauseDetail

    [Theory]
    [InlineData(DeathCause.Combat, "戦闘中の致命傷")]
    [InlineData(DeathCause.Boss, "ボスとの激戦")]
    [InlineData(DeathCause.Starvation, "飢餓")]
    [InlineData(DeathCause.Trap, "罠")]
    [InlineData(DeathCause.Poison, "毒")]
    [InlineData(DeathCause.TimeLimit, "時間切れ")]
    [InlineData(DeathCause.Curse, "呪い")]
    public void GameOver_GetDeathCauseDetail_AllCauses(DeathCause cause, string expected)
    {
        Assert.Equal(expected, GameOverSystem.GetDeathCauseDetail(cause));
    }

    #endregion

    // ========================================================================
    // InfiniteDungeonSystem Tests
    // ========================================================================

    #region InfiniteDungeonSystem - GetTier

    [Theory]
    [InlineData(1, InfiniteDungeonTier.Normal)]
    [InlineData(10, InfiniteDungeonTier.Normal)]
    [InlineData(11, InfiniteDungeonTier.Advanced)]
    [InlineData(30, InfiniteDungeonTier.Advanced)]
    [InlineData(31, InfiniteDungeonTier.Deep)]
    [InlineData(50, InfiniteDungeonTier.Deep)]
    [InlineData(51, InfiniteDungeonTier.Abyss)]
    [InlineData(100, InfiniteDungeonTier.Abyss)]
    public void Infinite_GetTier_FloorBoundaries(int floor, InfiniteDungeonTier expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.GetTier(floor));
    }

    #endregion

    #region InfiniteDungeonSystem - CalculateEnemyLevel and IsBossFloor

    [Theory]
    [InlineData(1, 12)]
    [InlineData(10, 30)]
    [InlineData(50, 110)]
    [InlineData(0, 10)]
    public void Infinite_CalculateEnemyLevel_Formula(int floor, int expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.CalculateEnemyLevel(floor));
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(30, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    [InlineData(15, false)]
    public void Infinite_IsBossFloor_Every10Floors(int floor, bool expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.IsBossFloor(floor));
    }

    #endregion

    #region InfiniteDungeonSystem - Multipliers

    [Theory]
    [InlineData(0, 1.0f)]
    [InlineData(10, 1.5f)]
    [InlineData(20, 2.0f)]
    public void Infinite_GetDropRateMultiplier_Formula(int floor, float expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.GetDropRateMultiplier(floor), 2);
    }

    [Theory]
    [InlineData(0, 1.0f)]
    [InlineData(10, 1.3f)]
    [InlineData(100, 4.0f)]
    public void Infinite_GetExpMultiplier_Formula(int floor, float expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.GetExpMultiplier(floor), 2);
    }

    #endregion

    #region InfiniteDungeonSystem - TierName, IsUnlocked, Score, Description

    [Theory]
    [InlineData(InfiniteDungeonTier.Normal, "通常帯")]
    [InlineData(InfiniteDungeonTier.Advanced, "上級帯")]
    [InlineData(InfiniteDungeonTier.Deep, "深層帯")]
    [InlineData(InfiniteDungeonTier.Abyss, "魔界帯")]
    public void Infinite_GetTierName_AllTiers(InfiniteDungeonTier tier, string expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.GetTierName(tier));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Infinite_IsUnlocked_Passthrough(bool completed, bool expected)
    {
        Assert.Equal(expected, InfiniteDungeonSystem.IsUnlocked(completed));
    }

    [Theory]
    [InlineData(100, "SSS")]
    [InlineData(50, "SS")]
    [InlineData(30, "S")]
    [InlineData(20, "A")]
    [InlineData(10, "B")]
    [InlineData(5, "C")]
    [InlineData(3, "D")]
    public void Infinite_CalculateScore_RankBoundaries(int floor, string expectedRank)
    {
        var score = InfiniteDungeonSystem.CalculateScore(floor, 100, 500);
        Assert.Equal(expectedRank, score.Rank);
        Assert.Equal(floor, score.MaxFloorReached);
    }

    [Fact]
    public void Infinite_GetFloorDescription_ContainsFloorAndTier()
    {
        var desc = InfiniteDungeonSystem.GetFloorDescription(25);
        Assert.Contains("25", desc);
        Assert.Contains("上級帯", desc);
    }

    #endregion

    // ========================================================================
    // NpcMemorySystem Tests
    // ========================================================================

    #region NpcMemorySystem - RecordAction and CalculateImpression

    [Fact]
    public void Npc_RecordAction_AddsMemory()
    {
        var sys = new NpcMemorySystem();
        sys.RecordAction("npc1", "help", 5, 10);
        Assert.Single(sys.Memories);
        Assert.Equal("npc1", sys.Memories[0].NpcId);
    }

    [Fact]
    public void Npc_RecordAction_MultipleMemories()
    {
        var sys = new NpcMemorySystem();
        sys.RecordAction("npc1", "help", 5, 1);
        sys.RecordAction("npc1", "trade", 3, 2);
        sys.RecordAction("npc2", "steal", -10, 3);
        Assert.Equal(3, sys.Memories.Count);
    }

    [Fact]
    public void Npc_CalculateImpression_SumsImpacts()
    {
        var sys = new NpcMemorySystem();
        sys.RecordAction("npc1", "help", 5, 1);
        sys.RecordAction("npc1", "gift", 10, 2);
        sys.RecordAction("npc2", "steal", -10, 3);
        Assert.Equal(15, sys.CalculateImpression("npc1"));
    }

    [Fact]
    public void Npc_CalculateImpression_NegativeValues()
    {
        var sys = new NpcMemorySystem();
        sys.RecordAction("npc1", "steal", -5, 1);
        sys.RecordAction("npc1", "attack", -10, 2);
        Assert.Equal(-15, sys.CalculateImpression("npc1"));
    }

    [Fact]
    public void Npc_CalculateImpression_UnknownNpc_Zero()
    {
        var sys = new NpcMemorySystem();
        Assert.Equal(0, sys.CalculateImpression("unknown"));
    }

    #endregion

    #region NpcMemorySystem - Rumors

    [Fact]
    public void Npc_GenerateRumor_AddsRumor()
    {
        var sys = new NpcMemorySystem();
        sys.GenerateRumor(RumorType.Heroic, "英雄的行為", "capital");
        Assert.Single(sys.Rumors);
        Assert.Equal(0, sys.Rumors[0].SpreadCount);
    }

    [Fact]
    public void Npc_SpreadRumors_IncrementsCount()
    {
        var sys = new NpcMemorySystem();
        sys.GenerateRumor(RumorType.Heroic, "test", "capital");
        sys.SpreadRumors();
        Assert.Equal(1, sys.Rumors[0].SpreadCount);
        sys.SpreadRumors();
        Assert.Equal(2, sys.Rumors[0].SpreadCount);
    }

    [Fact]
    public void Npc_SpreadRumors_MultipleRumors()
    {
        var sys = new NpcMemorySystem();
        sys.GenerateRumor(RumorType.Heroic, "a", "capital");
        sys.GenerateRumor(RumorType.Villainous, "b", "forest");
        sys.SpreadRumors();
        Assert.Equal(1, sys.Rumors[0].SpreadCount);
        Assert.Equal(1, sys.Rumors[1].SpreadCount);
    }

    #endregion

    #region NpcMemorySystem - Reset and GetRumorTypeName

    [Fact]
    public void Npc_Reset_ClearsAll()
    {
        var sys = new NpcMemorySystem();
        sys.RecordAction("npc1", "help", 5, 1);
        sys.GenerateRumor(RumorType.Heroic, "test", "capital");
        sys.Reset();
        Assert.Empty(sys.Memories);
        Assert.Empty(sys.Rumors);
    }

    [Theory]
    [InlineData(RumorType.Heroic, "英雄の噂")]
    [InlineData(RumorType.Villainous, "悪漢の噂")]
    [InlineData(RumorType.Eccentric, "奇人の噂")]
    [InlineData(RumorType.Unknown, "無名")]
    public void Npc_GetRumorTypeName_AllTypes(RumorType type, string expected)
    {
        Assert.Equal(expected, NpcMemorySystem.GetRumorTypeName(type));
    }

    #endregion

    // ========================================================================
    // OathSystem Tests
    // ========================================================================

    #region OathSystem - TakeOath and BreakOath

    [Fact]
    public void Oath_TakeOath_Success()
    {
        var sys = new OathSystem();
        Assert.True(sys.TakeOath(OathType.Temperance));
        Assert.Contains(OathType.Temperance, sys.ActiveOaths);
    }

    [Fact]
    public void Oath_TakeOath_Duplicate_ReturnsFalse()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Temperance);
        Assert.False(sys.TakeOath(OathType.Temperance));
    }

    [Fact]
    public void Oath_TakeOath_MultipleOaths()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Temperance);
        sys.TakeOath(OathType.Pacifism);
        sys.TakeOath(OathType.Solitude);
        Assert.Equal(3, sys.ActiveOaths.Count);
    }

    [Fact]
    public void Oath_BreakOath_RemovesOath()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Pacifism);
        Assert.True(sys.BreakOath(OathType.Pacifism));
        Assert.DoesNotContain(OathType.Pacifism, sys.ActiveOaths);
    }

    [Fact]
    public void Oath_BreakOath_NotActive_ReturnsFalse()
    {
        var sys = new OathSystem();
        Assert.False(sys.BreakOath(OathType.Darkness));
    }

    #endregion

    #region OathSystem - Bonuses

    [Fact]
    public void Oath_GetTotalExpBonus_NoOaths_Zero()
    {
        var sys = new OathSystem();
        Assert.Equal(0f, sys.GetTotalExpBonus());
    }

    [Fact]
    public void Oath_GetTotalExpBonus_SingleOath()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Pacifism);
        Assert.Equal(0.5f, sys.GetTotalExpBonus(), 2);
    }

    [Fact]
    public void Oath_GetTotalExpBonus_MultipleOaths()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Temperance); // 0.1
        sys.TakeOath(OathType.Pacifism);   // 0.5
        Assert.Equal(0.6f, sys.GetTotalExpBonus(), 2);
    }

    [Fact]
    public void Oath_GetTotalDropBonus_MultipleOaths()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Temperance); // 0.05
        sys.TakeOath(OathType.Darkness);   // 0.2
        Assert.Equal(0.25f, sys.GetTotalDropBonus(), 2);
    }

    #endregion

    #region OathSystem - IsViolation

    [Theory]
    [InlineData(OathType.Temperance, "use_alcohol", true)]
    [InlineData(OathType.Pacifism, "attack_enemy", true)]
    [InlineData(OathType.Solitude, "recruit_companion", true)]
    [InlineData(OathType.Austerity, "use_rare_food", true)]
    [InlineData(OathType.Darkness, "use_torch", true)]
    [InlineData(OathType.Temperance, "attack_enemy", false)]
    [InlineData(OathType.Pacifism, "use_alcohol", false)]
    [InlineData(OathType.Solitude, "use_torch", false)]
    public void Oath_IsViolation_Checks(OathType type, string action, bool expected)
    {
        var sys = new OathSystem();
        Assert.Equal(expected, sys.IsViolation(type, action));
    }

    #endregion

    #region OathSystem - Reset

    [Fact]
    public void Oath_Reset_ClearsAll()
    {
        var sys = new OathSystem();
        sys.TakeOath(OathType.Temperance);
        sys.TakeOath(OathType.Darkness);
        sys.Reset();
        Assert.Empty(sys.ActiveOaths);
        Assert.Equal(0f, sys.GetTotalExpBonus());
    }

    #endregion

    // ========================================================================
    // InvestmentSystem Tests
    // ========================================================================

    #region InvestmentSystem - Invest

    [Fact]
    public void Invest_Invest_AddsRecord()
    {
        var sys = new InvestmentSystem();
        Assert.True(sys.Invest(InvestmentType.Shop, "武器屋", 100, 1));
        Assert.Single(sys.Investments);
    }

    [Fact]
    public void Invest_Invest_MultipleRecords()
    {
        var sys = new InvestmentSystem();
        sys.Invest(InvestmentType.Shop, "武器屋", 100, 1);
        sys.Invest(InvestmentType.AdventurerParty, "冒険者A", 200, 2);
        Assert.Equal(2, sys.Investments.Count);
    }

    [Fact]
    public void Invest_Invest_CalculatesExpectedReturn()
    {
        var sys = new InvestmentSystem();
        sys.Invest(InvestmentType.Shop, "test", 100, 1);
        Assert.Equal(130f, sys.Investments[0].ExpectedReturn, 1);
    }

    #endregion

    #region InvestmentSystem - GetExpectedReturn

    [Theory]
    [InlineData(InvestmentType.Shop, 100, 130f)]
    [InlineData(InvestmentType.AdventurerParty, 100, 200f)]
    [InlineData(InvestmentType.Business, 100, 160f)]
    public void Invest_GetExpectedReturn_ByType(InvestmentType type, int amount, float expected)
    {
        Assert.Equal(expected, InvestmentSystem.GetExpectedReturn(type, amount), 1);
    }

    #endregion

    #region InvestmentSystem - GetSuccessRate

    [Theory]
    [InlineData(InvestmentType.Shop, 0.6f)]
    [InlineData(InvestmentType.AdventurerParty, 0.3f)]
    [InlineData(InvestmentType.Business, 0.45f)]
    public void Invest_GetSuccessRate_ByType(InvestmentType type, float expected)
    {
        Assert.Equal(expected, InvestmentSystem.GetSuccessRate(type));
    }

    #endregion

    #region InvestmentSystem - GetTypeName

    [Theory]
    [InlineData(InvestmentType.Shop, "ショップ投資")]
    [InlineData(InvestmentType.AdventurerParty, "冒険者パーティ出資")]
    [InlineData(InvestmentType.Business, "事業出資")]
    public void Invest_GetTypeName_ByType(InvestmentType type, string expected)
    {
        Assert.Equal(expected, InvestmentSystem.GetTypeName(type));
    }

    #endregion

    #region InvestmentSystem - GetTotalInvested and GetActiveInvestments

    [Fact]
    public void Invest_GetTotalInvested_SumsAmounts()
    {
        var sys = new InvestmentSystem();
        sys.Invest(InvestmentType.Shop, "a", 100, 1);
        sys.Invest(InvestmentType.Business, "b", 200, 2);
        Assert.Equal(300, sys.GetTotalInvested());
    }

    [Fact]
    public void Invest_GetTotalInvested_Empty_Zero()
    {
        var sys = new InvestmentSystem();
        Assert.Equal(0, sys.GetTotalInvested());
    }

    [Fact]
    public void Invest_GetActiveInvestments_AllActive()
    {
        var sys = new InvestmentSystem();
        sys.Invest(InvestmentType.Shop, "a", 100, 1);
        sys.Invest(InvestmentType.Business, "b", 200, 2);
        Assert.Equal(2, sys.GetActiveInvestments());
    }

    #endregion

    #region InvestmentSystem - Reset

    [Fact]
    public void Invest_Reset_ClearsAll()
    {
        var sys = new InvestmentSystem();
        sys.Invest(InvestmentType.Shop, "a", 100, 1);
        sys.Reset();
        Assert.Empty(sys.Investments);
        Assert.Equal(0, sys.GetTotalInvested());
        Assert.Equal(0, sys.GetActiveInvestments());
    }

    #endregion

    // ========================================================================
    // CookingSystem Tests
    // ========================================================================

    #region CookingSystem - FindRecipe

    [Theory]
    [InlineData("焼き肉", 20, 0, 30)]
    [InlineData("薬草スープ", 40, 10, 20)]
    [InlineData("蒸し魚", 30, 5, 25)]
    [InlineData("干し肉", 15, 0, 40)]
    [InlineData("発酵飲料", 5, 20, 10)]
    public void Cooking_FindRecipe_AllRecipes(string name, int hp, int mp, int hunger)
    {
        var recipe = CookingSystem.FindRecipe(name);
        Assert.NotNull(recipe);
        Assert.Equal(hp, recipe!.HpRestore);
        Assert.Equal(mp, recipe.MpRestore);
        Assert.Equal(hunger, recipe.HungerRestore);
    }

    [Fact]
    public void Cooking_FindRecipe_Unknown_ReturnsNull()
    {
        Assert.Null(CookingSystem.FindRecipe("存在しないレシピ"));
    }

    #endregion

    #region CookingSystem - GetAllRecipes

    [Fact]
    public void Cooking_GetAllRecipes_Returns5()
    {
        var recipes = CookingSystem.GetAllRecipes();
        Assert.Equal(5, recipes.Count);
    }

    #endregion

    #region CookingSystem - GetMethodName

    [Theory]
    [InlineData(CookingMethod.Grill, "焼く")]
    [InlineData(CookingMethod.Boil, "煮る")]
    [InlineData(CookingMethod.Steam, "蒸す")]
    [InlineData(CookingMethod.Dry, "干す")]
    [InlineData(CookingMethod.Ferment, "発酵")]
    public void Cooking_GetMethodName_AllMethods(CookingMethod method, string expected)
    {
        Assert.Equal(expected, CookingSystem.GetMethodName(method));
    }

    #endregion

    #region CookingSystem - CalculateQuality

    [Theory]
    [InlineData(0, 0.3f)]
    [InlineData(50, 0.8f)]
    [InlineData(100, 1.3f)]
    [InlineData(150, 1.3f)]  // capped at 100
    public void Cooking_CalculateQuality_Proficiency(int prof, float expected)
    {
        Assert.Equal(expected, CookingSystem.CalculateQuality(prof), 2);
    }

    #endregion

    #region CookingSystem - GetCookingTime

    [Theory]
    [InlineData(CookingMethod.Grill, 3)]
    [InlineData(CookingMethod.Boil, 5)]
    [InlineData(CookingMethod.Steam, 4)]
    [InlineData(CookingMethod.Dry, 10)]
    [InlineData(CookingMethod.Ferment, 20)]
    public void Cooking_GetCookingTime_AllMethods(CookingMethod method, int expected)
    {
        Assert.Equal(expected, CookingSystem.GetCookingTime(method));
    }

    #endregion

    // ========================================================================
    // SmugglingSystem Tests
    // ========================================================================

    #region SmugglingSystem - GetAllContrabands

    [Fact]
    public void Smuggling_GetAllContrabands_Returns4()
    {
        var items = SmugglingSystem.GetAllContrabands();
        Assert.Equal(4, items.Count);
    }

    #endregion

    #region SmugglingSystem - CheckEvasion

    [Fact]
    public void Smuggling_CheckEvasion_HighDex_Evades()
    {
        // detection=0.3, dex=30 => effectiveChance = 0.3 - 0.3 = 0.0 => clamp to 0.05
        // random=0.1 >= 0.05 => true
        Assert.True(SmugglingSystem.CheckEvasion(0.3f, 30, 0.1));
    }

    [Fact]
    public void Smuggling_CheckEvasion_LowDex_Caught()
    {
        // detection=0.4, dex=0 => effectiveChance=0.4
        // random=0.1 < 0.4 => false
        Assert.False(SmugglingSystem.CheckEvasion(0.4f, 0, 0.1));
    }

    [Fact]
    public void Smuggling_CheckEvasion_ExactThreshold()
    {
        // detection=0.3, dex=10 => effectiveChance = 0.3 - 0.1 = 0.2
        // random=0.21 >= 0.2 => true
        Assert.True(SmugglingSystem.CheckEvasion(0.3f, 10, 0.21));
    }

    [Fact]
    public void Smuggling_CheckEvasion_MinDetectionFloor()
    {
        // detection=0.3, dex=100 => effectiveChance = 0.3 - 1.0 = -0.7 => clamp to 0.05
        // random=0.04 < 0.05 => false (minimum detection always applies)
        Assert.False(SmugglingSystem.CheckEvasion(0.3f, 100, 0.04));
    }

    #endregion

    #region SmugglingSystem - CalculateProfit

    [Theory]
    [InlineData(ContrabandType.IllegalWeapons, 300)]
    [InlineData(ContrabandType.MonsterMaterials, 200)]
    [InlineData(ContrabandType.ForbiddenBooks, 500)]
    [InlineData(ContrabandType.Poisons, 250)]
    public void Smuggling_CalculateProfit_AllTypes(ContrabandType type, int expected)
    {
        Assert.Equal(expected, SmugglingSystem.CalculateProfit(type));
    }

    #endregion

    #region SmugglingSystem - GetTypeName

    [Theory]
    [InlineData(ContrabandType.IllegalWeapons, "違法武器")]
    [InlineData(ContrabandType.MonsterMaterials, "魔物素材")]
    [InlineData(ContrabandType.ForbiddenBooks, "禁書")]
    [InlineData(ContrabandType.Poisons, "毒物")]
    public void Smuggling_GetTypeName_AllTypes(ContrabandType type, string expected)
    {
        Assert.Equal(expected, SmugglingSystem.GetTypeName(type));
    }

    #endregion

    #region SmugglingSystem - GetPenalty

    [Theory]
    [InlineData(ContrabandType.IllegalWeapons, -15)]
    [InlineData(ContrabandType.MonsterMaterials, -10)]
    [InlineData(ContrabandType.ForbiddenBooks, -20)]
    [InlineData(ContrabandType.Poisons, -25)]
    public void Smuggling_GetPenalty_AllTypes(ContrabandType type, int expected)
    {
        Assert.Equal(expected, SmugglingSystem.GetPenalty(type));
    }

    #endregion

    // ========================================================================
    // BodyConditionSystem Tests
    // ========================================================================

    #region BodyConditionSystem - GetWound

    [Theory]
    [InlineData(BodyWoundType.Cut, "切り傷", -0.05f, -0.03f, 20)]
    [InlineData(BodyWoundType.Bruise, "打撲", -0.03f, -0.05f, 15)]
    [InlineData(BodyWoundType.Puncture, "刺し傷", -0.08f, -0.02f, 30)]
    [InlineData(BodyWoundType.Fracture, "骨折", -0.15f, -0.2f, 60)]
    [InlineData(BodyWoundType.Burn, "火傷", -0.1f, -0.1f, 40)]
    public void Body_GetWound_AllTypes(BodyWoundType type, string name, float str, float agi, int heal)
    {
        var wound = BodyConditionSystem.GetWound(type);
        Assert.NotNull(wound);
        Assert.Equal(name, wound!.Name);
        Assert.Equal(str, wound.StrModifier, 2);
        Assert.Equal(agi, wound.AgiModifier, 2);
        Assert.Equal(heal, wound.HealingTurns);
    }

    [Fact]
    public void Body_GetWound_Invalid_ReturnsNull()
    {
        Assert.Null(BodyConditionSystem.GetWound((BodyWoundType)999));
    }

    #endregion

    #region BodyConditionSystem - GetFatigueModifier

    [Theory]
    [InlineData(FatigueLevel.Fresh, 1.0f)]
    [InlineData(FatigueLevel.Mild, 0.9f)]
    [InlineData(FatigueLevel.Tired, 0.75f)]
    [InlineData(FatigueLevel.Exhausted, 0.5f)]
    [InlineData(FatigueLevel.Collapse, 0.0f)]
    public void Body_GetFatigueModifier_AllLevels(FatigueLevel level, float expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetFatigueModifier(level));
    }

    #endregion

    #region BodyConditionSystem - GetHygieneInfectionRisk

    [Theory]
    [InlineData(HygieneLevel.Clean, 0.5f)]
    [InlineData(HygieneLevel.Normal, 1.0f)]
    [InlineData(HygieneLevel.Dirty, 1.5f)]
    [InlineData(HygieneLevel.Filthy, 2.5f)]
    [InlineData(HygieneLevel.Foul, 4.0f)]
    public void Body_GetHygieneInfectionRisk_AllLevels(HygieneLevel level, float expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetHygieneInfectionRisk(level));
    }

    #endregion

    #region BodyConditionSystem - GetFatigueName

    [Theory]
    [InlineData(FatigueLevel.Fresh, "元気")]
    [InlineData(FatigueLevel.Mild, "軽疲労")]
    [InlineData(FatigueLevel.Tired, "疲労")]
    [InlineData(FatigueLevel.Exhausted, "重疲労")]
    [InlineData(FatigueLevel.Collapse, "過労")]
    public void Body_GetFatigueName_AllLevels(FatigueLevel level, string expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetFatigueName(level));
    }

    #endregion

    #region BodyConditionSystem - GetHygieneName

    [Theory]
    [InlineData(HygieneLevel.Clean, "清潔")]
    [InlineData(HygieneLevel.Normal, "普通")]
    [InlineData(HygieneLevel.Dirty, "汚れ")]
    [InlineData(HygieneLevel.Filthy, "不衛生")]
    [InlineData(HygieneLevel.Foul, "不潔")]
    public void Body_GetHygieneName_AllLevels(HygieneLevel level, string expected)
    {
        Assert.Equal(expected, BodyConditionSystem.GetHygieneName(level));
    }

    #endregion

    // ========================================================================
    // TerritoryInfluenceSystem Tests
    // ========================================================================

    #region TerritoryInfluenceSystem - Initialize and GetInfluence

    [Fact]
    public void Territory_Initialize_SetsInfluence()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 0.6f,
            ["Rebels"] = 0.4f
        });
        Assert.Equal(0.6f, sys.GetInfluence(TerritoryId.Capital, "Kingdom"), 2);
    }

    [Fact]
    public void Territory_GetInfluence_UnknownTerritory_Zero()
    {
        var sys = new TerritoryInfluenceSystem();
        Assert.Equal(0f, sys.GetInfluence(TerritoryId.Forest, "Kingdom"));
    }

    [Fact]
    public void Territory_GetInfluence_UnknownFaction_Zero()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 1.0f
        });
        Assert.Equal(0f, sys.GetInfluence(TerritoryId.Capital, "Unknown"));
    }

    #endregion

    #region TerritoryInfluenceSystem - ModifyInfluence

    [Fact]
    public void Territory_ModifyInfluence_IncreasesValue()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Forest, new Dictionary<string, float>
        {
            ["Elves"] = 0.5f,
            ["Humans"] = 0.5f
        });
        sys.ModifyInfluence(TerritoryId.Forest, "Elves", 0.2f);
        Assert.True(sys.GetInfluence(TerritoryId.Forest, "Elves") > 0.5f);
    }

    [Fact]
    public void Territory_ModifyInfluence_ClampToZero()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Mountain, new Dictionary<string, float>
        {
            ["Dwarves"] = 0.3f,
            ["Others"] = 0.7f
        });
        sys.ModifyInfluence(TerritoryId.Mountain, "Dwarves", -1.0f);
        Assert.True(sys.GetInfluence(TerritoryId.Mountain, "Dwarves") >= 0f);
    }

    [Fact]
    public void Territory_ModifyInfluence_NewTerritory_Creates()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.ModifyInfluence(TerritoryId.Coast, "Pirates", 0.5f);
        Assert.True(sys.GetInfluence(TerritoryId.Coast, "Pirates") > 0f);
    }

    #endregion

    #region TerritoryInfluenceSystem - GetDominantFaction

    [Fact]
    public void Territory_GetDominantFaction_ReturnsHighest()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 0.7f,
            ["Rebels"] = 0.3f
        });
        Assert.Equal("Kingdom", sys.GetDominantFaction(TerritoryId.Capital));
    }

    [Fact]
    public void Territory_GetDominantFaction_UnknownTerritory_Null()
    {
        var sys = new TerritoryInfluenceSystem();
        Assert.Null(sys.GetDominantFaction(TerritoryId.Frontier));
    }

    #endregion

    #region TerritoryInfluenceSystem - GetInfluenceMap

    [Fact]
    public void Territory_GetInfluenceMap_ReturnsAll()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Southern, new Dictionary<string, float>
        {
            ["Empire"] = 0.5f,
            ["Nomads"] = 0.5f
        });
        var map = sys.GetInfluenceMap(TerritoryId.Southern);
        Assert.NotNull(map);
        Assert.Equal(2, map!.Count);
    }

    [Fact]
    public void Territory_GetInfluenceMap_UnknownTerritory_Null()
    {
        var sys = new TerritoryInfluenceSystem();
        Assert.Null(sys.GetInfluenceMap(TerritoryId.Frontier));
    }

    #endregion

    #region TerritoryInfluenceSystem - Reset

    [Fact]
    public void Territory_Reset_ClearsAll()
    {
        var sys = new TerritoryInfluenceSystem();
        sys.Initialize(TerritoryId.Capital, new Dictionary<string, float>
        {
            ["Kingdom"] = 1.0f
        });
        sys.Reset();
        Assert.Null(sys.GetInfluenceMap(TerritoryId.Capital));
        Assert.Equal(0f, sys.GetInfluence(TerritoryId.Capital, "Kingdom"));
    }

    #endregion
}
