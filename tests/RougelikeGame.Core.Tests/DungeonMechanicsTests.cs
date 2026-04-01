using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.62 派閥 / P.63 隠し通路 / P.65 ミミック / P.52 描画最適化 / P.79 自動探索のテスト
/// </summary>
public class DungeonMechanicsTests
{
    #region Faction Tests

    [Fact]
    public void GetHostility_SameRace_Zero()
    {
        Assert.Equal(0f, DungeonFactionSystem.GetHostility(MonsterRace.Beast, MonsterRace.Beast));
    }

    [Fact]
    public void AreHostile_BeastUndead_True()
    {
        Assert.True(DungeonFactionSystem.AreHostile(MonsterRace.Beast, MonsterRace.Undead));
    }

    [Fact]
    public void AreAllied_BeastPlant_True()
    {
        Assert.True(DungeonFactionSystem.AreAllied(MonsterRace.Beast, MonsterRace.Plant));
    }

    [Fact]
    public void GetHostileRaces_ReturnsNonEmpty()
    {
        var hostile = DungeonFactionSystem.GetHostileRaces(MonsterRace.Humanoid);
        Assert.NotEmpty(hostile);
    }

    #endregion

    #region SecretRoom Tests

    [Fact]
    public void CalculateDiscoveryChance_IncreasesWithPerception()
    {
        float low = SecretRoomSystem.CalculateDiscoveryChance(5, false);
        float high = SecretRoomSystem.CalculateDiscoveryChance(20, false);
        Assert.True(high > low);
    }

    [Fact]
    public void CalculateDiscoveryChance_EagleEyeBonus()
    {
        float normal = SecretRoomSystem.CalculateDiscoveryChance(10, false);
        float eagle = SecretRoomSystem.CalculateDiscoveryChance(10, true);
        Assert.True(eagle > normal);
    }

    [Fact]
    public void CalculateSecretRoomCount_RuinsHasMore()
    {
        int ruins = SecretRoomSystem.CalculateSecretRoomCount(1, DungeonFeatureType.Ruins);
        int cave = SecretRoomSystem.CalculateSecretRoomCount(1, DungeonFeatureType.Cave);
        Assert.True(ruins >= cave);
    }

    #endregion

    #region Mimic Tests

    [Fact]
    public void CalculateMimicSpawnRate_IncreasesWithDepth()
    {
        float shallow = MimicSystem.CalculateMimicSpawnRate(1);
        float deep = MimicSystem.CalculateMimicSpawnRate(20);
        Assert.True(deep > shallow);
    }

    [Fact]
    public void CalculateDetectionRate_AppraisalBonus()
    {
        float normal = MimicSystem.CalculateDetectionRate(10, 80, false);
        float appraisal = MimicSystem.CalculateDetectionRate(10, 80, true);
        Assert.True(appraisal > normal);
    }

    [Fact]
    public void GetMimicStrengthMultiplier_HigherGradeStronger()
    {
        float crude = MimicSystem.GetMimicStrengthMultiplier(ItemGrade.Crude);
        float masterwork = MimicSystem.GetMimicStrengthMultiplier(ItemGrade.Masterwork);
        Assert.True(masterwork > crude);
    }

    [Fact]
    public void GetDisguiseTypes_ReturnsMultiple()
    {
        Assert.True(MimicSystem.GetDisguiseTypes().Count >= 2);
    }

    #endregion

    #region RenderOptimization Tests

    [Fact]
    public void CalculateViewport_CenteredOnPlayer()
    {
        var (minX, minY, maxX, maxY) = RenderOptimizationSystem.CalculateViewport(50, 50, 20, 20);
        Assert.Equal(40, minX);
        Assert.Equal(40, minY);
        Assert.Equal(60, maxX);
        Assert.Equal(60, maxY);
    }

    [Fact]
    public void IsInViewport_InsidePoint_True()
    {
        Assert.True(RenderOptimizationSystem.IsInViewport(50, 50, 40, 40, 60, 60));
    }

    [Fact]
    public void IsInViewport_OutsidePoint_False()
    {
        Assert.False(RenderOptimizationSystem.IsInViewport(70, 70, 40, 40, 60, 60));
    }

    [Fact]
    public void CalculateUpdateFrequency_CloseEntities_EveryTurn()
    {
        Assert.Equal(1, RenderOptimizationSystem.CalculateUpdateFrequency(5));
    }

    [Fact]
    public void CalculateUpdateFrequency_FarEntities_LessFrequent()
    {
        int freq = RenderOptimizationSystem.CalculateUpdateFrequency(50);
        Assert.True(freq > 1);
    }

    #endregion

    #region AutoExplore Tests

    [Fact]
    public void CheckStopConditions_EnemyInSight_StopsImmediately()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, true, false, false, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.EnemyDetected, reason);
    }

    [Fact]
    public void CheckStopConditions_LowHp_Stops()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 0.2f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.LowHp, reason);
    }

    [Fact]
    public void CheckStopConditions_AllClear_ReturnsNull()
    {
        var reason = AutoExploreSystem.CheckStopConditions(true, false, false, false, false, 1.0f, 1.0f);
        Assert.Null(reason);
    }

    [Fact]
    public void CheckStopConditions_FullyExplored_Stops()
    {
        var reason = AutoExploreSystem.CheckStopConditions(false, false, false, false, false, 1.0f, 1.0f);
        Assert.Equal(AutoExploreSystem.StopReason.FullyExplored, reason);
    }

    [Fact]
    public void GetStopMessage_ReturnsJapanese()
    {
        Assert.NotEmpty(AutoExploreSystem.GetStopMessage(AutoExploreSystem.StopReason.EnemyDetected));
    }

    #endregion
}
