using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// バランス調整・チュートリアルシステムのテスト (Phase 5.29-5.30)
/// </summary>
public class BalanceAndTutorialTests
{
    #region BalanceConfig - Depth Stat Multiplier

    [Theory]
    [InlineData(1, 1.0)]
    [InlineData(5, 1.0)]
    [InlineData(6, 1.3)]
    [InlineData(10, 1.3)]
    [InlineData(15, 1.7)]
    [InlineData(20, 2.2)]
    [InlineData(25, 2.8)]
    [InlineData(30, 3.5)]
    public void BalanceConfig_GetDepthStatMultiplier_ReturnsCorrectValue(int depth, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetDepthStatMultiplier(depth));
    }

    [Fact]
    public void BalanceConfig_GetDepthStatMultiplier_BeyondMax_ScalesLinearly()
    {
        double result = BalanceConfig.GetDepthStatMultiplier(35);
        Assert.True(result > 3.5);
    }

    #endregion

    #region BalanceConfig - Rank Multipliers

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 1.5)]
    [InlineData(EnemyRank.Rare, 2.0)]
    [InlineData(EnemyRank.Boss, 2.5)]
    [InlineData(EnemyRank.HiddenBoss, 4.0)]
    public void BalanceConfig_GetRankStatMultiplier_ReturnsCorrectValue(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankStatMultiplier(rank));
    }

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 1.5)]
    [InlineData(EnemyRank.Rare, 2.0)]
    [InlineData(EnemyRank.Boss, 2.5)]
    [InlineData(EnemyRank.HiddenBoss, 3.0)]
    public void BalanceConfig_GetRankDropBonus_ReturnsCorrectValue(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankDropBonus(rank));
    }

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 2.0)]
    [InlineData(EnemyRank.Rare, 3.0)]
    [InlineData(EnemyRank.Boss, 5.0)]
    [InlineData(EnemyRank.HiddenBoss, 10.0)]
    public void BalanceConfig_GetRankGoldMultiplier_ReturnsCorrectValue(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankGoldMultiplier(rank));
    }

    #endregion

    #region BalanceConfig - Recommended Level

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    [InlineData(30, 30)]
    public void BalanceConfig_GetRecommendedLevel_ReturnsExpectedProgression(int depth, int expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRecommendedLevel(depth));
    }

    #endregion

    #region BalanceConfig - Drop Rate

    [Theory]
    [InlineData(ItemRarity.Common, 0.50)]
    [InlineData(ItemRarity.Uncommon, 0.25)]
    [InlineData(ItemRarity.Rare, 0.10)]
    [InlineData(ItemRarity.Epic, 0.04)]
    [InlineData(ItemRarity.Legendary, 0.01)]
    public void BalanceConfig_GetBaseDropRate_ReturnsCorrectValue(ItemRarity rarity, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetBaseDropRate(rarity));
    }

    [Fact]
    public void BalanceConfig_DropRate_HigherRarity_LowerRate()
    {
        Assert.True(BalanceConfig.GetBaseDropRate(ItemRarity.Common) > BalanceConfig.GetBaseDropRate(ItemRarity.Uncommon));
        Assert.True(BalanceConfig.GetBaseDropRate(ItemRarity.Uncommon) > BalanceConfig.GetBaseDropRate(ItemRarity.Rare));
        Assert.True(BalanceConfig.GetBaseDropRate(ItemRarity.Rare) > BalanceConfig.GetBaseDropRate(ItemRarity.Epic));
        Assert.True(BalanceConfig.GetBaseDropRate(ItemRarity.Epic) > BalanceConfig.GetBaseDropRate(ItemRarity.Legendary));
    }

    #endregion

    #region BalanceConfig - Gold

    [Fact]
    public void BalanceConfig_GoldDrop_IncreasesWithDepth()
    {
        Assert.True(BalanceConfig.GetGoldDropMin(10) > BalanceConfig.GetGoldDropMin(1));
        Assert.True(BalanceConfig.GetGoldDropMax(10) > BalanceConfig.GetGoldDropMax(1));
    }

    [Fact]
    public void BalanceConfig_GoldDrop_MaxAlwaysGreaterThanMin()
    {
        for (int depth = 1; depth <= 30; depth++)
        {
            Assert.True(BalanceConfig.GetGoldDropMax(depth) > BalanceConfig.GetGoldDropMin(depth),
                $"Depth {depth}: Max should be > Min");
        }
    }

    #endregion

    #region BalanceConfig - Exp Multiplier

    [Fact]
    public void BalanceConfig_GetLevelDiffExpModifier_HigherEnemyGivesMoreExp()
    {
        Assert.True(BalanceConfig.GetLevelDiffExpModifier(-10) > BalanceConfig.GetLevelDiffExpModifier(0));
    }

    [Fact]
    public void BalanceConfig_GetLevelDiffExpModifier_LowerEnemyGivesLessExp()
    {
        Assert.True(BalanceConfig.GetLevelDiffExpModifier(0) > BalanceConfig.GetLevelDiffExpModifier(10));
    }

    #endregion

    #region BalanceConfig - Shop

    [Fact]
    public void BalanceConfig_ShopBuyMultiplier_GreaterThanSell()
    {
        Assert.True(BalanceConfig.ShopBuyMultiplier > BalanceConfig.ShopSellMultiplier);
    }

    [Fact]
    public void BalanceConfig_IdentifiedSellBonus_IsPositive()
    {
        Assert.True(BalanceConfig.IdentifiedSellBonus > 1.0);
    }

    #endregion

    #region DropTableSystem - Tables

    [Fact]
    public void DropTableSystem_HasDefaultTables()
    {
        var tableIds = DropTableSystem.GetAllTableIds();
        Assert.True(tableIds.Count >= 10);
    }

    [Theory]
    [InlineData("drop_slime")]
    [InlineData("drop_goblin")]
    [InlineData("drop_skeleton")]
    [InlineData("drop_orc")]
    [InlineData("drop_spider")]
    [InlineData("drop_dark_mage")]
    [InlineData("drop_forest")]
    [InlineData("drop_mountain")]
    [InlineData("drop_coast")]
    [InlineData("drop_southern")]
    [InlineData("drop_frontier")]
    [InlineData("drop_boss")]
    [InlineData("drop_hidden_boss")]
    public void DropTableSystem_GetTable_ReturnsTable(string tableId)
    {
        var table = DropTableSystem.GetTable(tableId);
        Assert.NotNull(table);
        Assert.NotEmpty(table!.Entries);
    }

    [Fact]
    public void DropTableSystem_GetTable_NonExistent_ReturnsNull()
    {
        Assert.Null(DropTableSystem.GetTable("nonexistent"));
    }

    #endregion

    #region DropTableSystem - Loot Generation

    [Fact]
    public void DropTableSystem_GenerateLoot_ReturnsGold()
    {
        var random = new TestRandomProvider(0.5);
        var result = DropTableSystem.GenerateLoot("drop_goblin", 5, EnemyRank.Common, random);
        Assert.True(result.Gold > 0);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_BossDropsMoreGold()
    {
        var random = new TestRandomProvider(0.5);
        var commonResult = DropTableSystem.GenerateLoot("drop_goblin", 5, EnemyRank.Common, random);
        var bossResult = DropTableSystem.GenerateLoot("drop_boss", 5, EnemyRank.Boss, random);
        Assert.True(bossResult.Gold > commonResult.Gold);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_InvalidTable_ReturnsEmpty()
    {
        var random = new TestRandomProvider(0.5);
        var result = DropTableSystem.GenerateLoot("nonexistent", 1, EnemyRank.Common, random);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Gold);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_WithLowRoll_DropsItems()
    {
        // ドロップ率テスト：NextDouble()が0.01を返す → ほぼ全てドロップ
        var random = new TestRandomProvider(0.01);
        var result = DropTableSystem.GenerateLoot("drop_boss", 20, EnemyRank.Boss, random);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_WithHighRoll_DropsNothing()
    {
        // ドロップ率テスト：NextDouble()が0.99を返す → ドロップなし
        var random = new TestRandomProvider(0.99);
        var result = DropTableSystem.GenerateLoot("drop_slime", 1, EnemyRank.Common, random);
        Assert.Empty(result.Items);
    }

    #endregion

    #region DropTableSystem - Scaled Stats

    [Fact]
    public void DropTableSystem_GetScaledStats_Depth1Common_UnchangedFromBase()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var scaled = DropTableSystem.GetScaledStats(baseStats, 1, EnemyRank.Common);
        Assert.Equal(10, scaled.Strength);
    }

    [Fact]
    public void DropTableSystem_GetScaledStats_HigherDepth_IncreasesStats()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var scaled1 = DropTableSystem.GetScaledStats(baseStats, 1, EnemyRank.Common);
        var scaled20 = DropTableSystem.GetScaledStats(baseStats, 20, EnemyRank.Common);
        Assert.True(scaled20.Strength > scaled1.Strength);
    }

    [Fact]
    public void DropTableSystem_GetScaledStats_HigherRank_IncreasesStats()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var common = DropTableSystem.GetScaledStats(baseStats, 10, EnemyRank.Common);
        var boss = DropTableSystem.GetScaledStats(baseStats, 10, EnemyRank.Boss);
        Assert.True(boss.Strength > common.Strength);
    }

    [Fact]
    public void DropTableSystem_GetScaledStats_AllStatsScale()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var scaled = DropTableSystem.GetScaledStats(baseStats, 15, EnemyRank.Elite);
        Assert.True(scaled.Strength > 10);
        Assert.True(scaled.Vitality > 10);
        Assert.True(scaled.Agility > 10);
        Assert.True(scaled.Intelligence > 10);
    }

    #endregion

    #region TutorialSystem - Basic

    [Fact]
    public void TutorialSystem_HasDefaultSteps()
    {
        var system = new TutorialSystem();
        Assert.True(system.TotalSteps >= 15);
    }

    [Fact]
    public void TutorialSystem_InitiallyEnabled()
    {
        var system = new TutorialSystem();
        Assert.True(system.IsEnabled);
    }

    [Fact]
    public void TutorialSystem_InitiallyNoCompletedSteps()
    {
        var system = new TutorialSystem();
        Assert.Equal(0, system.CompletedCount);
    }

    #endregion

    #region TutorialSystem - Trigger

    [Fact]
    public void TutorialSystem_OnTrigger_GameStart_ReturnsWelcomeStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
        Assert.Equal("move", step!.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_SameTriggerTwice_ReturnsNullSecondTime()
    {
        var system = new TutorialSystem();
        var step1 = system.OnTrigger(TutorialTrigger.GameStart);
        var step2 = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step1);
        Assert.Null(step2);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_MarksStepCompleted()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        Assert.True(system.IsStepCompleted("move"));
        Assert.Equal(1, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_Disabled_ReturnsNull()
    {
        var system = new TutorialSystem();
        system.IsEnabled = false;
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.Null(step);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_Combat_ReturnsCombatStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstEnemySight);
        Assert.NotNull(step);
        Assert.Equal("attack", step!.Id);
    }

    [Fact]
    public void TutorialSystem_OnTrigger_Death_ReturnsDeathStep()
    {
        var system = new TutorialSystem();
        var step = system.OnTrigger(TutorialTrigger.FirstDeath);
        Assert.NotNull(step);
        Assert.Equal("death", step!.Id);
    }

    #endregion

    #region TutorialSystem - Complete/Reset

    [Fact]
    public void TutorialSystem_CompleteStep_MarksAsCompleted()
    {
        var system = new TutorialSystem();
        system.CompleteStep("move");
        Assert.True(system.IsStepCompleted("move"));
    }

    [Fact]
    public void TutorialSystem_Reset_ClearsProgress()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        system.OnTrigger(TutorialTrigger.FirstEnemySight);
        Assert.Equal(2, system.CompletedCount);

        system.Reset();
        Assert.Equal(0, system.CompletedCount);
    }

    [Fact]
    public void TutorialSystem_Reset_AllowsRetrigger()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        system.Reset();
        var step = system.OnTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
    }

    [Fact]
    public void TutorialSystem_IsAllCompleted_FalseInitially()
    {
        var system = new TutorialSystem();
        Assert.False(system.IsComplete);
    }

    [Fact]
    public void TutorialSystem_IsAllCompleted_TrueWhenAllDone()
    {
        var system = new TutorialSystem();
        foreach (var step in system.GetAllSteps())
        {
            system.CompleteStep(step.Id);
        }
        Assert.True(system.IsComplete);
    }

    #endregion

    #region TutorialSystem - GetStepForTrigger

    [Fact]
    public void TutorialSystem_GetStepForTrigger_ReturnsHighestPriority()
    {
        var system = new TutorialSystem();
        var step = system.GetStepForTrigger(TutorialTrigger.GameStart);
        Assert.NotNull(step);
        Assert.Equal(100, step!.Priority);
    }

    [Fact]
    public void TutorialSystem_GetStepForTrigger_CompletedStep_ReturnsNull()
    {
        var system = new TutorialSystem();
        system.CompleteStep("move");
        var step = system.GetStepForTrigger(TutorialTrigger.GameStart);
        Assert.Null(step);
    }

    [Fact]
    public void TutorialSystem_GetStepForTrigger_Disabled_ReturnsNull()
    {
        var system = new TutorialSystem();
        system.IsEnabled = false;
        var step = system.GetStepForTrigger(TutorialTrigger.GameStart);
        Assert.Null(step);
    }

    #endregion

    #region TutorialSystem - Enable/Disable

    [Fact]
    public void TutorialSystem_SetEnabled_TogglesState()
    {
        var system = new TutorialSystem();
        Assert.True(system.IsEnabled);
        system.IsEnabled = false;
        Assert.False(system.IsEnabled);
        system.IsEnabled = true;
        Assert.True(system.IsEnabled);
    }

    #endregion

    #region TutorialSystem - GetAllSteps/GetCompletedStepIds

    [Fact]
    public void TutorialSystem_GetAllSteps_ReturnsAllRegisteredSteps()
    {
        var system = new TutorialSystem();
        var steps = system.GetAllSteps();
        Assert.True(steps.Count >= 15);
        Assert.Contains(steps, s => s.Id == "move");
        Assert.Contains(steps, s => s.Id == "attack");
        Assert.Contains(steps, s => s.Id == "death");
    }

    [Fact]
    public void TutorialSystem_GetCompletedStepIds_ReturnsOnlyCompleted()
    {
        var system = new TutorialSystem();
        system.OnTrigger(TutorialTrigger.GameStart);
        var completed = system.GetCompletedSteps();
        Assert.Single(completed);
        Assert.Contains("move", completed);
    }

    #endregion

    #region Integration - Balance Consistency

    [Fact]
    public void Balance_DepthProgression_IsMonotonic()
    {
        double prev = 0;
        for (int depth = 1; depth <= 30; depth++)
        {
            double current = BalanceConfig.GetDepthStatMultiplier(depth);
            Assert.True(current >= prev, $"Depth {depth}: multiplier should be >= previous");
            prev = current;
        }
    }

    [Fact]
    public void Balance_RecommendedLevel_IncreasesWithDepth()
    {
        int prevLevel = 0;
        for (int depth = 1; depth <= 30; depth++)
        {
            int level = BalanceConfig.GetRecommendedLevel(depth);
            Assert.True(level >= prevLevel, $"Depth {depth}: recommended level should be >= previous");
            prevLevel = level;
        }
    }

    [Fact]
    public void Balance_GoldReward_ScalesProperlyWithRank()
    {
        int depth = 10;
        double commonGold = BalanceConfig.GetRankGoldMultiplier(EnemyRank.Common);
        double eliteGold = BalanceConfig.GetRankGoldMultiplier(EnemyRank.Elite);
        double bossGold = BalanceConfig.GetRankGoldMultiplier(EnemyRank.Boss);
        Assert.True(eliteGold > commonGold);
        Assert.True(bossGold > eliteGold);
    }

    #endregion

    #region Helper

    private class TestRandomProvider : IRandomProvider
    {
        private readonly double _doubleValue;
        private int _counter;

        public TestRandomProvider(double doubleValue)
        {
            _doubleValue = doubleValue;
        }

        public int Next(int maxValue) => maxValue > 1 ? maxValue / 2 : 0;
        public int Next(int minValue, int maxValue) => minValue + (maxValue - minValue) / 2;
        public double NextDouble() => _doubleValue;
    }

    #endregion
}
