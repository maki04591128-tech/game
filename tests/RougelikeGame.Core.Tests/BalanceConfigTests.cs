using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Tests;

public class BalanceConfigTests
{
    #region GetDepthStatMultiplier

    [Theory]
    [InlineData(1, 1.0)]
    [InlineData(5, 1.0)]
    [InlineData(10, 1.3)]
    [InlineData(15, 1.7)]
    [InlineData(20, 2.2)]
    [InlineData(25, 2.8)]
    [InlineData(30, 3.5)]
    [InlineData(35, 4.5)]
    public void GetDepthStatMultiplier_ForDepth_ReturnsExpected(int depth, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetDepthStatMultiplier(depth));
    }

    #endregion

    #region GetRankStatMultiplier

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 1.5)]
    [InlineData(EnemyRank.Rare, 2.0)]
    [InlineData(EnemyRank.Boss, 2.5)]
    [InlineData(EnemyRank.HiddenBoss, 4.0)]
    public void GetRankStatMultiplier_ForRank_ReturnsExpected(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankStatMultiplier(rank));
    }

    #endregion

    #region GetRecommendedLevel

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    [InlineData(7, 7)]
    [InlineData(10, 10)]
    [InlineData(15, 15)]
    [InlineData(20, 20)]
    [InlineData(25, 25)]
    [InlineData(30, 30)]
    [InlineData(35, 35)]
    public void GetRecommendedLevel_ForDepth_ReturnsExpected(int depth, int expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRecommendedLevel(depth));
    }

    #endregion

    #region GetFloorBossHpMultiplier

    [Theory]
    [InlineData(5, 3.0)]
    [InlineData(10, 3.5)]
    [InlineData(15, 4.0)]
    [InlineData(20, 4.5)]
    [InlineData(25, 5.0)]
    [InlineData(30, 5.5)]
    public void GetFloorBossHpMultiplier_ForFloor_ReturnsExpected(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetFloorBossHpMultiplier(floor));
    }

    #endregion

    #region GetFloorBossAttackMultiplier

    [Theory]
    [InlineData(5, 2.0)]
    [InlineData(10, 2.5)]
    [InlineData(15, 3.0)]
    [InlineData(20, 3.5)]
    [InlineData(25, 4.0)]
    [InlineData(30, 5.0)]
    public void GetFloorBossAttackMultiplier_ForFloor_ReturnsExpected(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetFloorBossAttackMultiplier(floor));
    }

    #endregion

    #region GetBossExpBonus

    [Theory]
    [InlineData(5, 3.0)]
    [InlineData(10, 4.0)]
    [InlineData(15, 5.0)]
    [InlineData(20, 6.0)]
    [InlineData(25, 8.0)]
    [InlineData(30, 10.0)]
    [InlineData(1, 2.0)]   // デフォルト
    [InlineData(12, 2.0)]  // デフォルト
    public void GetBossExpBonus_ForFloor_ReturnsExpected(int floor, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetBossExpBonus(floor));
    }

    #endregion

    #region GetBaseDropRate

    [Theory]
    [InlineData(ItemRarity.Common, 0.50)]
    [InlineData(ItemRarity.Uncommon, 0.25)]
    [InlineData(ItemRarity.Rare, 0.10)]
    [InlineData(ItemRarity.Epic, 0.04)]
    [InlineData(ItemRarity.Legendary, 0.01)]
    public void GetBaseDropRate_ForRarity_ReturnsExpected(ItemRarity rarity, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetBaseDropRate(rarity));
    }

    #endregion

    #region GetRankDropBonus

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 1.5)]
    [InlineData(EnemyRank.Rare, 2.0)]
    [InlineData(EnemyRank.Boss, 2.5)]
    [InlineData(EnemyRank.HiddenBoss, 3.0)]
    public void GetRankDropBonus_ForRank_ReturnsExpected(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankDropBonus(rank));
    }

    #endregion

    #region GetGoldDropMin / GetGoldDropMax

    [Theory]
    [InlineData(1, 8)]
    [InlineData(10, 35)]
    [InlineData(20, 65)]
    [InlineData(30, 95)]
    public void GetGoldDropMin_ForDepth_ReturnsExpected(int depth, int expected)
    {
        Assert.Equal(expected, BalanceConfig.GetGoldDropMin(depth));
    }

    [Theory]
    [InlineData(1, 23)]
    [InlineData(10, 95)]
    [InlineData(20, 175)]
    [InlineData(30, 255)]
    public void GetGoldDropMax_ForDepth_ReturnsExpected(int depth, int expected)
    {
        Assert.Equal(expected, BalanceConfig.GetGoldDropMax(depth));
    }

    #endregion

    #region GetRankGoldMultiplier

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 2.0)]
    [InlineData(EnemyRank.Rare, 3.0)]
    [InlineData(EnemyRank.Boss, 5.0)]
    [InlineData(EnemyRank.HiddenBoss, 10.0)]
    public void GetRankGoldMultiplier_ForRank_ReturnsExpected(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankGoldMultiplier(rank));
    }

    #endregion

    #region GetRankExpMultiplier

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 2.0)]
    [InlineData(EnemyRank.Rare, 3.0)]
    [InlineData(EnemyRank.Boss, 5.0)]
    [InlineData(EnemyRank.HiddenBoss, 10.0)]
    public void GetRankExpMultiplier_ForRank_ReturnsExpected(EnemyRank rank, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetRankExpMultiplier(rank));
    }

    #endregion

    #region GetLevelDiffExpModifier

    [Theory]
    [InlineData(-15, 2.0)]
    [InlineData(-10, 2.0)]
    [InlineData(-7, 1.5)]
    [InlineData(-5, 1.5)]
    [InlineData(-3, 1.0)]
    [InlineData(0, 1.0)]
    [InlineData(3, 0.7)]
    [InlineData(5, 0.7)]
    [InlineData(7, 0.4)]
    [InlineData(10, 0.4)]
    [InlineData(15, 0.1)]
    public void GetLevelDiffExpModifier_ForDiff_ReturnsExpected(int levelDiff, double expected)
    {
        Assert.Equal(expected, BalanceConfig.GetLevelDiffExpModifier(levelDiff));
    }

    #endregion

    #region ショップ定数

    [Fact]
    public void ShopConstants_HaveExpectedValues()
    {
        Assert.Equal(1.2, BalanceConfig.ShopBuyMultiplier);  // L-1: 購入倍率を1.5→1.2に修正
        Assert.Equal(0.5, BalanceConfig.ShopSellMultiplier);
        Assert.Equal(1.2, BalanceConfig.IdentifiedSellBonus);
    }

    #endregion

    #region GameConstants

    [Fact]
    public void GameConstants_SanityValues_AreCorrect()
    {
        Assert.Equal(100, GameConstants.InitialSanity);
        Assert.Equal(100, GameConstants.MaxSanity);
        Assert.Equal(20, GameConstants.SanityRecoveryOnRescue);
        Assert.Equal(3, GameConstants.MaxRescueCount);
    }

    [Fact]
    public void GameConstants_HungerValues_AreCorrect()
    {
        Assert.Equal(100, GameConstants.InitialHunger);
        Assert.Equal(100, GameConstants.MaxHunger);
    }

    [Fact]
    public void GameConstants_CombatValues_AreCorrect()
    {
        Assert.Equal(0.05, GameConstants.BaseCriticalRate);
        Assert.Equal(0.75, GameConstants.MaxEvasionRate);
        Assert.Equal(1, GameConstants.MinimumDamage);
    }

    [Fact]
    public void GameConstants_LevelValues_AreCorrect()
    {
        Assert.Equal(50, GameConstants.MaxLevel);
        Assert.Equal(100, GameConstants.BaseExpRequired);
        Assert.Equal(1.15, GameConstants.ExpGrowthRate);
    }

    [Fact]
    public void GameConstants_MapValues_AreCorrect()
    {
        Assert.Equal(80, GameConstants.DefaultMapWidth);
        Assert.Equal(50, GameConstants.DefaultMapHeight);
        Assert.Equal(8, GameConstants.DefaultViewRadius);
        Assert.Equal(30, GameConstants.MaxDungeonFloor);
        Assert.Equal(5, GameConstants.BossFloorInterval);
    }

    [Fact]
    public void GameConstants_InventoryAndWeight_AreCorrect()
    {
        Assert.Equal(30, GameConstants.DefaultInventorySize);
        Assert.Equal(50f, GameConstants.BaseMaxWeight);
        Assert.Equal(5f, GameConstants.WeightPerStrength);
        Assert.Equal(0.5f, GameConstants.OverweightSpeedPenalty);
    }

    #endregion

    #region TurnCosts

    [Fact]
    public void TurnCosts_MovementValues_AreCorrect()
    {
        Assert.Equal(1, TurnCosts.MoveNormal);
        Assert.Equal(10, TurnCosts.MoveCombat);
        Assert.Equal(10, TurnCosts.MoveStealth);
        Assert.Equal(5, TurnCosts.MovePursuit);
        Assert.Equal(3, TurnCosts.MoveAlert);
        Assert.Equal(1, TurnCosts.MoveDash);
    }

    [Fact]
    public void TurnCosts_AttackValues_AreCorrect()
    {
        Assert.Equal(3, TurnCosts.AttackNormal);
        Assert.Equal(2, TurnCosts.AttackUnarmed);
        Assert.Equal(5, TurnCosts.AttackTwoHanded);
        Assert.Equal(4, TurnCosts.AttackBow);
        Assert.Equal(2, TurnCosts.AttackThrow);
    }

    [Fact]
    public void TurnCosts_OtherValues_AreCorrect()
    {
        Assert.Equal(1, TurnCosts.Wait);
        Assert.Equal(100, TurnCosts.Rest);
        Assert.Equal(5, TurnCosts.Search);
        Assert.Equal(1, TurnCosts.OpenDoor);
    }

    #endregion

    #region TimeConstants

    [Fact]
    public void TimeConstants_BaseValues_AreCorrect()
    {
        Assert.Equal(1, TimeConstants.TurnsPerSecond);
        Assert.Equal(60, TimeConstants.TurnsPerMinute);
        Assert.Equal(3600, TimeConstants.TurnsPerHour);
        Assert.Equal(86400, TimeConstants.TurnsPerDay);
    }

    [Fact]
    public void TimeConstants_EventIntervals_AreCorrect()
    {
        Assert.Equal(600, TimeConstants.HungerDecayInterval);
        Assert.Equal(10, TimeConstants.StatusEffectTickInterval);
        Assert.Equal(60, TimeConstants.NpcScheduleUpdateInterval);
    }

    #endregion

    #region SanityLoss

    [Fact]
    public void SanityLoss_AllValues_AreCorrect()
    {
        Assert.Equal(5, SanityLoss.Combat);
        Assert.Equal(8, SanityLoss.Boss);
        Assert.Equal(10, SanityLoss.Starvation);
        Assert.Equal(10, SanityLoss.Trap);
        Assert.Equal(15, SanityLoss.TimeLimit);
        Assert.Equal(15, SanityLoss.Curse);
        Assert.Equal(20, SanityLoss.Suicide);
        Assert.Equal(25, SanityLoss.SanityDeath);
        Assert.Equal(8, SanityLoss.Fall);
        Assert.Equal(7, SanityLoss.Poison);
        Assert.Equal(10, SanityLoss.Unknown);
    }

    #endregion
}
