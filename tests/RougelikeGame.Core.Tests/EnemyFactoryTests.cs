using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.AI;
using RougelikeGame.Engine.Combat;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// EnemyFactory/EnemyDefinitionsのユニットテスト
/// </summary>
public class EnemyFactoryExtendedTests
{
    private readonly EnemyFactory _factory = new();

    #region CreateEnemy基本

    [Fact]
    public void CreateEnemy_WithDefinition_SetsNameCorrectly()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(5, 5));
        Assert.Equal("スライム", enemy.Name);
    }

    [Fact]
    public void CreateEnemy_WithDefinition_SetsPositionCorrectly()
    {
        var pos = new Position(10, 20);
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Goblin, pos);
        Assert.Equal(pos, enemy.Position);
        Assert.Equal(pos, enemy.HomePosition);
    }

    [Fact]
    public void CreateEnemy_WithDefinition_SetsFactionToEnemy()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Skeleton, new Position(3, 3));
        Assert.Equal(Faction.Enemy, enemy.Faction);
    }

    [Fact]
    public void CreateEnemy_WithDefinition_SetsBehavior()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(0, 0));
        Assert.NotNull(enemy.Behavior);
    }

    [Fact]
    public void CreateEnemy_WithFloorBonus_AppliesStatModifier()
    {
        var bonus = new StatModifier(Strength: 5, Vitality: 5, Agility: 2);
        var normal = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(0, 0));
        var boosted = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(0, 0), bonus);

        Assert.True(boosted.BaseStats.MaxHp > normal.BaseStats.MaxHp);
    }

    [Fact]
    public void CreateEnemy_WithoutFloorBonus_UsesBaseStats()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Goblin, new Position(0, 0));
        Assert.Equal(EnemyDefinitions.Goblin.BaseStats.MaxHp, enemy.BaseStats.MaxHp);
    }

    #endregion

    #region 個別敵生成メソッド

    [Fact]
    public void CreateSlime_ReturnsSlimeEnemy()
    {
        var enemy = _factory.CreateSlime(new Position(5, 5));
        Assert.Equal("スライム", enemy.Name);
        Assert.Equal(MonsterRace.Amorphous, enemy.Race);
    }

    [Fact]
    public void CreateGoblin_ReturnsGoblinEnemy()
    {
        var enemy = _factory.CreateGoblin(new Position(5, 5));
        Assert.Equal("ゴブリン", enemy.Name);
        Assert.Equal(MonsterRace.Humanoid, enemy.Race);
    }

    [Fact]
    public void CreateSkeleton_ReturnsSkeletonEnemy()
    {
        var enemy = _factory.CreateSkeleton(new Position(5, 5));
        Assert.Equal("スケルトン", enemy.Name);
        Assert.Equal(MonsterRace.Undead, enemy.Race);
    }

    #endregion

    #region EnemyDefinitions

    [Fact]
    public void EnemyDefinitions_GetAllEnemies_ReturnsMultiple()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        Assert.True(all.Count >= 20);
    }

    [Fact]
    public void EnemyDefinitions_GetAllBosses_ReturnsMultiple()
    {
        var bosses = EnemyDefinitions.GetAllBosses();
        Assert.True(bosses.Count >= 5);
    }

    [Fact]
    public void EnemyDefinitions_GetEnemiesForDepth_Shallow_ReturnsWeakEnemies()
    {
        var enemies = EnemyDefinitions.GetEnemiesForDepth(1);
        Assert.True(enemies.Count >= 2);
    }

    [Fact]
    public void EnemyDefinitions_GetEnemiesForDepth_Deep_ReturnsStrongEnemies()
    {
        var enemies = EnemyDefinitions.GetEnemiesForDepth(25);
        Assert.True(enemies.Count >= 2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    public void EnemyDefinitions_GetEnemiesForDepth_AllDepths_ReturnNonEmpty(int depth)
    {
        var enemies = EnemyDefinitions.GetEnemiesForDepth(depth);
        Assert.True(enemies.Count > 0);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void EnemyDefinitions_GetEnemiesForTerritory_ReturnsNonEmpty(TerritoryId territory)
    {
        var enemies = EnemyDefinitions.GetEnemiesForTerritory(territory);
        Assert.True(enemies.Count > 0);
    }

    [Theory]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void EnemyDefinitions_GetBossForTerritory_ReturnsNonNull(TerritoryId territory)
    {
        var boss = EnemyDefinitions.GetBossForTerritory(territory);
        Assert.NotNull(boss);
    }

    [Theory]
    [InlineData("capital_catacombs")]
    [InlineData("forest_corruption")]
    [InlineData("mountain_mine")]
    [InlineData("mountain_dragon")]
    [InlineData("frontier_great_rift")]
    public void EnemyDefinitions_GetDungeonBoss_ReturnsNonNull(string dungeonId)
    {
        var boss = EnemyDefinitions.GetDungeonBoss(dungeonId);
        Assert.NotNull(boss);
    }

    [Fact]
    public void EnemyDefinitions_FloorBosses_HaveBossType()
    {
        // FloorBoss定義の確認
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss5.EnemyType);
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss10.EnemyType);
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss15.EnemyType);
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss20.EnemyType);
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss25.EnemyType);
        Assert.Equal(EnemyType.Boss, EnemyDefinitions.FloorBoss30.EnemyType);
    }

    [Fact]
    public void EnemyDefinitions_AllEnemies_HaveValidNames()
    {
        foreach (var def in EnemyDefinitions.GetAllEnemies())
        {
            Assert.False(string.IsNullOrEmpty(def.Name), $"敵定義にNameがない: {def.TypeId}");
        }
    }

    [Fact]
    public void EnemyDefinitions_AllEnemies_HavePositiveBaseStats()
    {
        foreach (var def in EnemyDefinitions.GetAllEnemies())
        {
            Assert.True(def.BaseStats.MaxHp > 0, $"{def.Name}のMaxHP <= 0");
            Assert.True(def.BaseStats.PhysicalAttack >= 0, $"{def.Name}のPhysicalAttack < 0");
        }
    }

    [Fact]
    public void EnemyDefinitions_AllEnemies_HaveValidExperienceReward()
    {
        foreach (var def in EnemyDefinitions.GetAllEnemies())
        {
            Assert.True(def.ExperienceReward > 0, $"{def.Name}のExpReward <= 0");
        }
    }

    #endregion

    #region AIビヘイビア割当

    [Fact]
    public void CreateEnemy_NormalType_HasFleeBehavior()
    {
        var def = EnemyDefinitions.Slime;
        var enemy = _factory.CreateEnemy(def, new Position(0, 0));
        Assert.NotNull(enemy.Behavior);
    }

    [Fact]
    public void CreateEnemy_BossType_HasBerserkerBehavior()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.FloorBoss5, new Position(0, 0));
        Assert.NotNull(enemy.Behavior);
    }

    [Fact]
    public void CreateEnemy_AllEnemyTypes_CanBeCreated()
    {
        foreach (var def in EnemyDefinitions.GetAllEnemies())
        {
            var enemy = _factory.CreateEnemy(def, new Position(5, 5));
            Assert.NotNull(enemy);
            Assert.Equal(def.Name, enemy.Name);
            Assert.NotNull(enemy.Behavior);
        }
    }

    #endregion

    #region Enemy.Rank設定・DropTableSystem統合

    [Fact]
    public void CreateEnemy_SetsRankFromDefinition()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(5, 5));
        Assert.Equal(EnemyRank.Common, enemy.Rank);
    }

    [Fact]
    public void CreateEnemy_EliteEnemy_SetsEliteRank()
    {
        // DarkElfはEliteランク
        var enemy = _factory.CreateEnemy(EnemyDefinitions.DarkElf, new Position(5, 5));
        Assert.Equal(EnemyRank.Elite, enemy.Rank);
    }

    [Fact]
    public void CreateEnemy_AllEnemies_HaveRankSet()
    {
        foreach (var def in EnemyDefinitions.GetAllEnemies())
        {
            var enemy = _factory.CreateEnemy(def, new Position(5, 5));
            Assert.True(Enum.IsDefined(typeof(EnemyRank), enemy.Rank),
                $"{def.Name} should have a valid EnemyRank");
            Assert.Equal(def.Rank, enemy.Rank);
        }
    }

    [Fact]
    public void CreateEnemy_SetsDropTableId()
    {
        var enemy = _factory.CreateEnemy(EnemyDefinitions.Slime, new Position(5, 5));
        Assert.Equal("drop_slime", enemy.DropTableId);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_WithRank_ReturnsResult()
    {
        var result = DropTableSystem.GenerateLoot("drop_slime", 1, EnemyRank.Common, new RandomProvider());
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_WithRace_ReturnsResult()
    {
        var result = DropTableSystem.GenerateLoot("drop_slime", 1, EnemyRank.Common, new RandomProvider(), MonsterRace.Amorphous);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_BossRank_HasHigherDropBonus()
    {
        // Boss rank should have higher drop rate multiplier
        var bossBonus = BalanceConfig.GetRankDropBonus(EnemyRank.Boss);
        var commonBonus = BalanceConfig.GetRankDropBonus(EnemyRank.Common);
        Assert.True(bossBonus > commonBonus);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_NonHumanoid_NoGold()
    {
        // 非人型の敵（Amorphous=スライム等）はゴールドをドロップしない
        var random = new RandomProvider();
        var result = DropTableSystem.GenerateLoot("drop_goblin", 5, EnemyRank.Common, random, MonsterRace.Amorphous);
        Assert.Equal(0, result.Gold);
    }

    [Fact]
    public void DropTableSystem_GenerateLoot_Humanoid_CanDropGold()
    {
        // 人型の敵（Humanoid=ゴブリン等）はゴールドをドロップ可能
        // drop_goblinテーブルにはゴールド範囲がある (3-12)
        var table = DropTableSystem.GetTable("drop_goblin");
        Assert.NotNull(table);
        Assert.True(table!.GoldMax > 0, "Goblin drop table should have gold");
    }

    [Fact]
    public void Enemy_DefaultRank_IsCommon()
    {
        var enemy = Entities.Enemy.Create("Test", "test", new Stats(5, 5, 5, 5, 5, 5, 5, 5, 5), 10);
        Assert.Equal(EnemyRank.Common, enemy.Rank);
    }

    #endregion

    #region Rankボーナスとソウルジェム品質

    [Theory]
    [InlineData(EnemyRank.Common, 1.0)]
    [InlineData(EnemyRank.Elite, 2.0)]
    [InlineData(EnemyRank.Rare, 3.0)]
    [InlineData(EnemyRank.Boss, 5.0)]
    [InlineData(EnemyRank.HiddenBoss, 10.0)]
    public void GetRankGoldMultiplier_ReturnsExpectedValues(EnemyRank rank, double expected)
    {
        double actual = BalanceConfig.GetRankGoldMultiplier(rank);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(EnemyRank.Common, SoulGemQuality.Fragment)]
    [InlineData(EnemyRank.Elite, SoulGemQuality.Small)]
    [InlineData(EnemyRank.Rare, SoulGemQuality.Medium)]
    [InlineData(EnemyRank.Boss, SoulGemQuality.Large)]
    [InlineData(EnemyRank.HiddenBoss, SoulGemQuality.Grand)]
    public void GetSoulGemQualityFromRank_MapsCorrectly(EnemyRank rank, SoulGemQuality expectedQuality)
    {
        var actual = Systems.EnchantmentSystem.GetSoulGemQualityFromRank(rank);
        Assert.Equal(expectedQuality, actual);
    }

    [Fact]
    public void RankGoldMultiplier_Boss_IsHigherThanCommon()
    {
        double bossMultiplier = BalanceConfig.GetRankGoldMultiplier(EnemyRank.Boss);
        double commonMultiplier = BalanceConfig.GetRankGoldMultiplier(EnemyRank.Common);
        Assert.True(bossMultiplier > commonMultiplier, "Boss gold multiplier should be higher than Common");
    }

    [Fact]
    public void AllEnemyDefinitions_HaveValidRank()
    {
        var factory = new EnemyFactory();
        var definitions = new[]
        {
            EnemyDefinitions.Slime, EnemyDefinitions.Goblin, EnemyDefinitions.Skeleton,
            EnemyDefinitions.DarkElf, EnemyDefinitions.MountainGolem, EnemyDefinitions.FrontierDragon
        };

        foreach (var def in definitions)
        {
            var enemy = factory.CreateEnemy(def, new Position(0, 0));
            Assert.True(Enum.IsDefined(typeof(EnemyRank), enemy.Rank),
                $"{enemy.Name} has undefined Rank: {enemy.Rank}");
        }
    }

    [Theory]
    [InlineData(EnemyRank.Elite)]
    [InlineData(EnemyRank.Rare)]
    [InlineData(EnemyRank.Boss)]
    [InlineData(EnemyRank.HiddenBoss)]
    public void GetRankGoldMultiplier_AboveCommon_ReturnsGreaterThan1(EnemyRank rank)
    {
        double multiplier = BalanceConfig.GetRankGoldMultiplier(rank);
        Assert.True(multiplier > 1.0, $"Rank {rank} should have gold multiplier > 1.0");
    }

    #endregion
}
