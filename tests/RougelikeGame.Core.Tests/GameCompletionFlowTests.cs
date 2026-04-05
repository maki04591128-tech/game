using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.AI;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.5 ゲーム完走フローテスト
/// Phase A: ゲームクリアフロー確立（T.1〜T.6）
/// Phase B: システム統合（T.7〜T.11）
/// Phase C: 品質向上（T.12〜T.15）
/// </summary>
public class GameCompletionFlowTests
{
    #region T.1 フロアボス定義・配置テスト

    [Theory]
    [InlineData(5, "floor_boss_5", "キングスライム")]
    [InlineData(10, "floor_boss_10", "ゴブリンキング")]
    [InlineData(15, "floor_boss_15", "スケルトンロード")]
    [InlineData(20, "floor_boss_20", "ダークエルフ将軍")]
    [InlineData(25, "floor_boss_25", "炎竜ヴァルグレス")]
    [InlineData(30, "floor_boss_30", "深淵の王")]
    public void GetFloorBoss_ReturnsCorrectBoss(int floor, string expectedTypeId, string expectedName)
    {
        var boss = EnemyDefinitions.GetFloorBoss(floor);
        Assert.NotNull(boss);
        Assert.Equal(expectedTypeId, boss.TypeId);
        Assert.Equal(expectedName, boss.Name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(12)]
    public void GetFloorBoss_NonBossFloor_ReturnsNull(int floor)
    {
        Assert.Null(EnemyDefinitions.GetFloorBoss(floor));
    }

    [Fact]
    public void GetAllFloorBosses_Returns6Bosses()
    {
        var bosses = EnemyDefinitions.GetAllFloorBosses();
        Assert.Equal(6, bosses.Count);
    }

    [Fact]
    public void FloorBosses_AreAllBossType()
    {
        foreach (var boss in EnemyDefinitions.GetAllFloorBosses())
        {
            Assert.Equal(EnemyType.Boss, boss.EnemyType);
        }
    }

    [Fact]
    public void FloorBosses_HaveHighStats()
    {
        foreach (var boss in EnemyDefinitions.GetAllFloorBosses())
        {
            Assert.True(boss.BaseStats.Strength >= 15, $"{boss.Name} STR should be >= 15");
            Assert.True(boss.BaseStats.Vitality >= 25, $"{boss.Name} VIT should be >= 25");
            Assert.True(boss.ExperienceReward >= 150, $"{boss.Name} EXP should be >= 150");
        }
    }

    [Fact]
    public void FloorBoss30_IsHiddenBossRank()
    {
        var boss = EnemyDefinitions.GetFloorBoss(30);
        Assert.NotNull(boss);
        Assert.Equal(EnemyRank.HiddenBoss, boss.Rank);
    }

    [Fact]
    public void FloorBosses_ExperienceScalesWithFloor()
    {
        var boss5 = EnemyDefinitions.GetFloorBoss(5)!;
        var boss10 = EnemyDefinitions.GetFloorBoss(10)!;
        var boss15 = EnemyDefinitions.GetFloorBoss(15)!;
        var boss20 = EnemyDefinitions.GetFloorBoss(20)!;
        var boss25 = EnemyDefinitions.GetFloorBoss(25)!;
        var boss30 = EnemyDefinitions.GetFloorBoss(30)!;

        Assert.True(boss5.ExperienceReward < boss10.ExperienceReward);
        Assert.True(boss10.ExperienceReward < boss15.ExperienceReward);
        Assert.True(boss15.ExperienceReward < boss20.ExperienceReward);
        Assert.True(boss20.ExperienceReward < boss25.ExperienceReward);
        Assert.True(boss25.ExperienceReward < boss30.ExperienceReward);
    }

    [Fact]
    public void FloorBosses_HaveDropTableIds()
    {
        foreach (var boss in EnemyDefinitions.GetAllFloorBosses())
        {
            Assert.False(string.IsNullOrEmpty(boss.DropTableId), $"{boss.Name} should have drop table");
        }
    }

    [Fact]
    public void EnemyFactory_CanCreateFloorBoss()
    {
        var factory = new EnemyFactory();
        var boss = EnemyDefinitions.GetFloorBoss(5)!;
        var enemy = factory.CreateEnemy(boss, new Position(10, 10));

        Assert.Equal("キングスライム", enemy.Name);
        Assert.True(enemy.IsAlive);
        Assert.Equal(new Position(10, 10), enemy.Position);
    }

    #endregion

    #region T.2 ゲームクリア判定テスト

    [Fact]
    public void IsFinalBossDefeated_Floor30_BossId_ReturnsTrue()
    {
        Assert.True(GameClearSystem.IsFinalBossDefeated(30, "floor_boss_30"));
    }

    [Fact]
    public void IsFinalBossDefeated_Floor30_WrongId_ReturnsFalse()
    {
        Assert.False(GameClearSystem.IsFinalBossDefeated(30, "goblin"));
    }

    [Fact]
    public void IsFinalBossDefeated_WrongFloor_ReturnsFalse()
    {
        Assert.False(GameClearSystem.IsFinalBossDefeated(25, "floor_boss_30"));
    }

    [Fact]
    public void CalculateScore_ReturnsValidResult()
    {
        var score = GameClearSystem.CalculateScore(4000, 0, 40, 30);
        Assert.Equal("S", score.Rank);
        Assert.True(score.TotalScore > 0);
        Assert.True(score.TurnBonus > 0);
        Assert.Equal(0, score.DeathPenalty);
        Assert.Equal(40 * 200, score.LevelBonus);
        Assert.Equal(30 * 100, score.FloorBonus);
    }

    [Fact]
    public void CalculateScore_HighDeaths_HasPenalty()
    {
        var score = GameClearSystem.CalculateScore(10000, 10, 30, 30);
        Assert.Equal(10 * 500, score.DeathPenalty);
    }

    [Fact]
    public void CalculateScore_ScoreNeverNegative()
    {
        var score = GameClearSystem.CalculateScore(100000, 100, 1, 1);
        Assert.True(score.TotalScore >= 0);
    }

    #endregion

    #region T.3 ゲームクリア画面テスト

    [Theory]
    [InlineData(Background.Noble)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Soldier)]
    [InlineData(Background.Adventurer)]
    public void GetClearText_ReturnsNonEmpty(Background bg)
    {
        var text = GameClearSystem.GetClearText(bg);
        Assert.False(string.IsNullOrWhiteSpace(text));
        // 全テキストにクリア関連キーワードを含む
        Assert.True(text.Contains("深淵の王") || text.Contains("魔王"),
            $"Clear text for {bg} should contain '深淵の王' or '魔王'");
    }

    [Fact]
    public void GetClearText_AllBackgroundsHaveText()
    {
        foreach (Background bg in Enum.GetValues<Background>())
        {
            var text = GameClearSystem.GetClearText(bg);
            Assert.False(string.IsNullOrWhiteSpace(text), $"Background {bg} should have clear text");
        }
    }

    #endregion

    #region T.4 ゲームオーバーメニューテスト

    [Fact]
    public void ProcessChoice_Rebirth_WithSanity_Succeeds()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Rebirth, 50);
        Assert.True(result.ShouldRebirth);
        Assert.False(result.ShouldReturnToTitle);
        Assert.False(result.ShouldQuitGame);
    }

    [Fact]
    public void ProcessChoice_Rebirth_NoSanity_Fails()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Rebirth, 0);
        Assert.False(result.ShouldRebirth);
        Assert.Contains("正気度", result.Message);
    }

    [Fact]
    public void ProcessChoice_ReturnToTitle_AlwaysSucceeds()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.ReturnToTitle, 0);
        Assert.True(result.ShouldReturnToTitle);
        Assert.False(result.ShouldQuitGame);
    }

    [Fact]
    public void ProcessChoice_Quit_AlwaysSucceeds()
    {
        var result = GameOverSystem.ProcessChoice(GameOverSystem.GameOverChoice.Quit, 0);
        Assert.True(result.ShouldQuitGame);
        Assert.False(result.ShouldReturnToTitle);
    }

    [Theory]
    [InlineData(DeathCause.Combat)]
    [InlineData(DeathCause.Boss)]
    [InlineData(DeathCause.Starvation)]
    [InlineData(DeathCause.Trap)]
    public void GetDeathCauseDetail_ReturnsNonEmpty(DeathCause cause)
    {
        var detail = GameOverSystem.GetDeathCauseDetail(cause);
        Assert.False(string.IsNullOrWhiteSpace(detail));
    }

    #endregion

    #region T.5 NG+開始処理テスト

    [Fact]
    public void GetNextTier_ProgressesCorrectly()
    {
        Assert.Equal(NewGamePlusTier.Plus2, NewGamePlusSystem.GetNextTier(NewGamePlusTier.Plus1));
        Assert.Equal(NewGamePlusTier.Plus3, NewGamePlusSystem.GetNextTier(NewGamePlusTier.Plus2));
        Assert.Equal(NewGamePlusTier.Plus5, NewGamePlusSystem.GetNextTier(NewGamePlusTier.Plus5));
    }

    [Fact]
    public void GetEnemyStatMultiplier_ScalesWithTier()
    {
        float tier1 = NewGamePlusSystem.GetEnemyStatMultiplier(NewGamePlusTier.Plus1);
        float tier3 = NewGamePlusSystem.GetEnemyStatMultiplier(NewGamePlusTier.Plus3);
        float tier5 = NewGamePlusSystem.GetEnemyStatMultiplier(NewGamePlusTier.Plus5);

        Assert.True(tier1 < tier3);
        Assert.True(tier3 < tier5);
    }

    [Fact]
    public void GetExpMultiplier_ScalesWithTier()
    {
        float tier1 = NewGamePlusSystem.GetExpMultiplier(NewGamePlusTier.Plus1);
        float tier5 = NewGamePlusSystem.GetExpMultiplier(NewGamePlusTier.Plus5);

        Assert.True(tier1 < tier5);
        Assert.True(tier1 > 1.0f);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("A")]
    [InlineData("B")]
    public void DetermineInitialTier_ValidRanks_ReturnsPlus1(string rank)
    {
        Assert.Equal(NewGamePlusTier.Plus1, NewGamePlusSystem.DetermineInitialTier(rank));
    }

    [Fact]
    public void GetStartMessage_ContainsTierName()
    {
        var msg = NewGamePlusSystem.GetStartMessage(NewGamePlusTier.Plus1);
        Assert.Contains("NG+1", msg);
    }

    [Fact]
    public void NgPlusCarryOver_RecordCreatable()
    {
        var data = new NewGamePlusSystem.NgPlusCarryOver(
            NewGamePlusTier.Plus1, 40, 5000,
            new[] { "炎斬り" }, new[] { "スライム" }, "A");
        Assert.Equal(NewGamePlusTier.Plus1, data.Tier);
        Assert.Equal(40, data.Level);
        Assert.Equal(5000, data.Gold);
    }

    #endregion

    #region T.6 メインクエストラインテスト

    [Fact]
    public void RegisterMainQuest_CreatesQuest()
    {
        var questSystem = new QuestSystem();
        questSystem.RegisterMainQuest();

        var quest = questSystem.GetQuestDefinition("main_quest_abyss");
        Assert.NotNull(quest);
        Assert.Equal("深淵の探索", quest.Name);
        Assert.Equal(QuestType.Main, quest.Type);
    }

    [Fact]
    public void MainQuest_Has6Objectives()
    {
        var questSystem = new QuestSystem();
        questSystem.RegisterMainQuest();

        var quest = questSystem.GetQuestDefinition("main_quest_abyss")!;
        Assert.Equal(6, quest.Objectives.Length);
    }

    [Fact]
    public void MainQuest_ObjectivesTargetFloorBosses()
    {
        var questSystem = new QuestSystem();
        questSystem.RegisterMainQuest();

        var quest = questSystem.GetQuestDefinition("main_quest_abyss")!;
        Assert.Contains(quest.Objectives, o => o.TargetId == "floor_boss_5");
        Assert.Contains(quest.Objectives, o => o.TargetId == "floor_boss_30");
    }

    [Fact]
    public void MainQuest_CanBeAccepted()
    {
        var questSystem = new QuestSystem();
        questSystem.RegisterMainQuest();

        var result = questSystem.AcceptQuest("main_quest_abyss", 1, GuildRank.None);
        Assert.True(result.Success);
    }

    [Fact]
    public void MainQuest_IsMainQuestComplete_FalseByDefault()
    {
        var questSystem = new QuestSystem();
        questSystem.RegisterMainQuest();
        Assert.False(questSystem.IsMainQuestComplete);
    }

    #endregion

    #region T.7 仲間戦闘AIテスト

    [Fact]
    public void ProcessCompanionTurns_AggressiveMode_AttacksNearbyEnemy()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト傭兵", CompanionType.Mercenary, CompanionAIMode.Aggressive, 10, 50, 100));

        var results = system.ProcessCompanionTurns(true, "ゴブリン", 2);
        Assert.Single(results);
        Assert.True(results[0].DamageDealt > 0);
        Assert.Equal("ゴブリン", results[0].TargetName);
    }

    [Fact]
    public void ProcessCompanionTurns_DefensiveMode_CounterAttacksAdjacent()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト仲間", CompanionType.Ally, CompanionAIMode.Defensive, 5, 50, 0));

        var results = system.ProcessCompanionTurns(true, "スライム", 1);
        Assert.Single(results);
        Assert.True(results[0].DamageDealt > 0);
    }

    [Fact]
    public void ProcessCompanionTurns_SupportMode_FollowsPlayer()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テストペット", CompanionType.Pet, CompanionAIMode.Support, 3, 50, 50));

        var results = system.ProcessCompanionTurns(false);
        Assert.Single(results);
        Assert.Equal(0, results[0].DamageDealt);
        Assert.Contains("追従", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_WaitMode_DoesNothing()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト", CompanionType.Ally, CompanionAIMode.Wait, 5, 50, 0));

        var results = system.ProcessCompanionTurns(true, "ゴブリン", 1);
        Assert.Single(results);
        Assert.Equal(0, results[0].DamageDealt);
        Assert.Contains("待機", results[0].ActionDescription);
    }

    [Fact]
    public void CalculateCompanionDamage_PositiveValue()
    {
        var companion = new CompanionSystem.CompanionData(
            "Test", CompanionType.Mercenary, CompanionAIMode.Aggressive, 10, 50, 100);
        int damage = CompanionSystem.CalculateCompanionDamage(companion);
        Assert.True(damage > 0);
    }

    [Fact]
    public void DamageCompanion_ReducesHp()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 50, 50, 10, 5));

        bool died = system.DamageCompanion("テスト", 20);
        Assert.False(died);
        Assert.True(system.Party[0].Hp < 50);
    }

    [Fact]
    public void DamageCompanion_FatalDamage_KillsCompanion()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 10, 50, 10, 0));

        bool died = system.DamageCompanion("テスト", 100);
        Assert.True(died);
        Assert.False(system.Party[0].IsAlive);
    }

    [Fact]
    public void HealCompanion_RestoresHp()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 50, 100, 10, 5));

        system.HealCompanion("テスト", 30);
        Assert.Equal(80, system.Party[0].Hp);
    }

    [Fact]
    public void HealCompanion_DoesNotExceedMaxHp()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "テスト", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 90, 100, 10, 5));

        system.HealCompanion("テスト", 50);
        Assert.Equal(100, system.Party[0].Hp);
    }

    [Fact]
    public void RemoveDeadCompanions_RemovesOnlyDead()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "生存", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 50, 50, 10, 5, true));
        system.AddCompanion(new CompanionSystem.CompanionData(
            "死亡", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 0, 50, 10, 5, false));

        var dead = system.RemoveDeadCompanions();
        Assert.Single(dead);
        Assert.Equal("死亡", dead[0]);
        Assert.Single(system.Party);
    }

    [Fact]
    public void AliveCount_CountsOnlyAlive()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData(
            "生存1", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 50, 50, 10, 5, true));
        system.AddCompanion(new CompanionSystem.CompanionData(
            "死亡", CompanionType.Ally, CompanionAIMode.Aggressive, 5, 50, 0, 0, 50, 10, 5, false));

        Assert.Equal(1, system.AliveCount);
    }

    #endregion

    #region T.11 拠点施設効果テスト

    [Fact]
    public void GetTotalBonus_NoFacilities_DefaultValues()
    {
        var system = new BaseConstructionSystem();
        var bonus = system.GetTotalBonus();
        Assert.Equal(1.0f, bonus.HpRecoveryMultiplier);
        Assert.Equal(0f, bonus.CraftingSuccessBonus);
        Assert.Equal(0, bonus.ExtraStorageSlots);
        Assert.Equal(0, bonus.FoodProductionPerDay);
    }

    [Fact]
    public void GetTotalBonus_WithCamp_IncreasesHpRecovery()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Camp, 100);
        var bonus = system.GetTotalBonus();
        Assert.Equal(1.25f, bonus.HpRecoveryMultiplier);
    }

    [Fact]
    public void GetTotalBonus_WithSmithy_IncrasesCraftingBonus()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Smithy, 100);
        var bonus = system.GetTotalBonus();
        Assert.Equal(0.2f, bonus.CraftingSuccessBonus);
    }

    [Fact]
    public void GetTotalBonus_WithBarracks_IncreasesCompanionSlots()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Barracks, 100);
        var bonus = system.GetTotalBonus();
        Assert.Equal(2, bonus.ExtraCompanionSlots);
    }

    [Fact]
    public void GetTotalBonus_WithFarm_ProducesFood()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Farm, 100);
        Assert.Equal(3, system.GetDailyFoodProduction());
    }

    [Fact]
    public void GetTotalBonus_WithStorage_IncreasesSlots()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Storage, 100);
        Assert.Equal(50, system.GetExtraStorageSlots());
    }

    [Fact]
    public void GetRestHpRecoveryMultiplier_CombinesMultipleFacilities()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Camp, 100);
        system.Build(FacilityCategory.Barracks, 100);
        // Camp: +0.25, Barracks: +0.5 → 1.0 + 0.25 + 0.5 = 1.75
        Assert.Equal(1.75f, system.GetRestHpRecoveryMultiplier());
    }

    #endregion

    #region T.12 クエスト自動進行テスト

    [Fact]
    public void UpdateKillObjective_ProgressesMainQuest()
    {
        var system = new QuestSystem();
        system.RegisterMainQuest();
        system.AcceptQuest("main_quest_abyss", 1, GuildRank.None);

        var messages = system.UpdateKillObjective("floor_boss_5");
        Assert.True(messages.Count > 0);
        Assert.Contains(messages, m => m.Contains("クエスト目標達成"));
    }

    [Fact]
    public void UpdateKillObjective_IrrelevantEnemy_NoMessages()
    {
        var system = new QuestSystem();
        system.RegisterMainQuest();
        system.AcceptQuest("main_quest_abyss", 1, GuildRank.None);

        var messages = system.UpdateKillObjective("slime");
        Assert.Empty(messages);
    }

    [Fact]
    public void UpdateExploreObjective_ReturnsMessages()
    {
        var system = new QuestSystem();
        var msgs = system.UpdateExploreObjective(5);
        Assert.NotNull(msgs);
    }

    [Fact]
    public void UpdateCollectObjective_ReturnsMessages()
    {
        var system = new QuestSystem();
        var msgs = system.UpdateCollectObjective("healing_potion");
        Assert.NotNull(msgs);
    }

    #endregion

    #region T.13 無限ダンジョン解放テスト

    [Fact]
    public void CalculateScore_ReturnsValidScore()
    {
        var score = InfiniteDungeonSystem.CalculateScore(50, 200, 10000);
        Assert.Equal(50, score.MaxFloorReached);
        Assert.Equal(200, score.TotalKills);
        Assert.Equal("SS", score.Rank);
    }

    [Theory]
    [InlineData(100, "SSS")]
    [InlineData(50, "SS")]
    [InlineData(30, "S")]
    [InlineData(20, "A")]
    [InlineData(10, "B")]
    [InlineData(5, "C")]
    [InlineData(3, "D")]
    public void CalculateScore_RanksByFloor(int floor, string expectedRank)
    {
        var score = InfiniteDungeonSystem.CalculateScore(floor, 0, 0);
        Assert.Equal(expectedRank, score.Rank);
    }

    [Fact]
    public void GetUnlockMessage_NotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(InfiniteDungeonSystem.GetUnlockMessage()));
    }

    [Fact]
    public void GetFloorDescription_ContainsFloorNumber()
    {
        var desc = InfiniteDungeonSystem.GetFloorDescription(25);
        Assert.Contains("25", desc);
    }

    #endregion

    #region T.14 ゲームバランス調整テスト

    [Theory]
    [InlineData(5, 3.0)]
    [InlineData(30, 5.5)]
    public void FloorBossHpMultiplier_ScalesWithFloor(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetFloorBossHpMultiplier(floor));
    }

    [Theory]
    [InlineData(5, 2.0)]
    [InlineData(30, 5.0)]
    public void FloorBossAttackMultiplier_ScalesWithFloor(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetFloorBossAttackMultiplier(floor));
    }

    [Theory]
    [InlineData(5, 3.0)]
    [InlineData(30, 10.0)]
    public void BossExpBonus_ScalesWithFloor(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetBossExpBonus(floor));
    }

    [Fact]
    public void FloorBossMultipliers_IncreaseWithFloor()
    {
        for (int floor = 10; floor <= 30; floor += 5)
        {
            int prevFloor = floor - 5;
            Assert.True(
                BalanceConfig.GetFloorBossHpMultiplier(floor) >= BalanceConfig.GetFloorBossHpMultiplier(prevFloor),
                $"HP multiplier at floor {floor} should be >= floor {prevFloor}");
        }
    }

    #endregion

    #region T.15 残存システム統合テスト

    [Fact]
    public void ExtendedItemCategory_Has8Values()
    {
        Assert.Equal(8, Enum.GetValues<ExtendedItemCategory>().Length);
    }

    [Fact]
    public void EndingType_Has5Values()
    {
        Assert.Equal(5, Enum.GetValues<EndingType>().Length);
    }

    [Fact]
    public void AmbientSoundType_Has9Values()
    {
        Assert.Equal(9, Enum.GetValues<AmbientSoundType>().Length);
    }

    [Fact]
    public void NpcType_HasExpandedValues()
    {
        Assert.True(Enum.GetValues<NpcType>().Length >= 15);
    }

    [Fact]
    public void QuestType_HasMain()
    {
        Assert.True(Enum.IsDefined(typeof(QuestType), QuestType.Main));
    }

    // マルチエンディング
    [Fact]
    public void DetermineEnding_NormalClear_ReturnsNormal()
    {
        var ending = MultiEndingSystem.DetermineEnding(true, 5, 0, false, "C");
        Assert.Equal(EndingType.Normal, ending.Type);
    }

    [Fact]
    public void DetermineEnding_DarkKarma_ReturnsDark()
    {
        var ending = MultiEndingSystem.DetermineEnding(true, 5, -60, false, "C");
        Assert.Equal(EndingType.Dark, ending.Type);
    }

    [Fact]
    public void DetermineEnding_NoDeaths_HighKarma_ReturnsSalvation()
    {
        var ending = MultiEndingSystem.DetermineEnding(true, 0, 80, false, "C");
        Assert.Equal(EndingType.Salvation, ending.Type);
    }

    [Fact]
    public void DetermineEnding_HighRank_ReturnsTrue()
    {
        var ending = MultiEndingSystem.DetermineEnding(true, 3, 0, false, "S");
        Assert.Equal(EndingType.True, ending.Type);
    }

    [Fact]
    public void DetermineEnding_AllTerritories_NoClear_ReturnsWanderer()
    {
        var ending = MultiEndingSystem.DetermineEnding(false, 5, 0, true, "");
        Assert.Equal(EndingType.Wanderer, ending.Type);
    }

    [Fact]
    public void GetEndingTypeName_AllTypes_NotEmpty()
    {
        foreach (EndingType type in Enum.GetValues<EndingType>())
        {
            Assert.False(string.IsNullOrWhiteSpace(MultiEndingSystem.GetEndingTypeName(type)));
        }
    }

    [Fact]
    public void GetEndingConditions_Returns5Conditions()
    {
        var conditions = MultiEndingSystem.GetEndingConditions();
        Assert.Equal(5, conditions.Count);
    }

    // 環境音システム
    [Theory]
    [InlineData(TerritoryId.Forest, AmbientSoundType.Forest)]
    [InlineData(TerritoryId.Mountain, AmbientSoundType.Mountain)]
    [InlineData(TerritoryId.Coast, AmbientSoundType.Coast)]
    [InlineData(TerritoryId.Capital, AmbientSoundType.Town)]
    public void GetAmbientForTerritory_ReturnsExpected(TerritoryId territory, AmbientSoundType expected)
    {
        Assert.Equal(expected, AmbientSoundSystem.GetAmbientForTerritory(territory));
    }

    [Fact]
    public void GetAmbientForDungeon_BossFloor_ReturnsBossBattle()
    {
        Assert.Equal(AmbientSoundType.BossBattle, AmbientSoundSystem.GetAmbientForDungeon(10, true));
    }

    [Fact]
    public void GetAmbientForDungeon_NormalFloor_ReturnsDungeon()
    {
        Assert.Equal(AmbientSoundType.Dungeon, AmbientSoundSystem.GetAmbientForDungeon(7, false));
    }

    [Fact]
    public void GetDefaultVolume_AllTypes_NonNegative()
    {
        foreach (AmbientSoundType type in Enum.GetValues<AmbientSoundType>())
        {
            Assert.True(AmbientSoundSystem.GetDefaultVolume(type) >= 0f);
        }
    }

    [Fact]
    public void GetSoundName_AllTypes_NotEmpty()
    {
        foreach (AmbientSoundType type in Enum.GetValues<AmbientSoundType>())
        {
            Assert.False(string.IsNullOrWhiteSpace(AmbientSoundSystem.GetSoundName(type)));
        }
    }

    [Fact]
    public void CreateEvent_ReturnsValidEvent()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Forest);
        Assert.Equal(AmbientSoundType.Forest, evt.Type);
        Assert.True(evt.Volume > 0);
        Assert.True(evt.ShouldLoop);
    }

    [Fact]
    public void CreateEvent_Silence_DoesNotLoop()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Silence);
        Assert.False(evt.ShouldLoop);
    }

    #endregion
}
