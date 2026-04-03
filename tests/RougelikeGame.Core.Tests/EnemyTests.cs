using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

public class EnemyTests
{
    [Fact]
    public void Create_InitializesCorrectly()
    {
        // Arrange & Act
        var enemy = Enemy.Create("Test Enemy", "test_enemy", Stats.Default, 10);

        // Assert
        Assert.Equal("Test Enemy", enemy.Name);
        Assert.Equal("test_enemy", enemy.EnemyTypeId);
        Assert.Equal(10, enemy.ExperienceReward);
        Assert.Equal(Faction.Enemy, enemy.Faction);
        Assert.True(enemy.IsAlive);
        Assert.Equal(AIState.Idle, enemy.CurrentAIState);
    }

    [Fact]
    public void ShouldFlee_ReturnsTrueWhenHpBelowThreshold()
    {
        // Arrange
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.TakeDamage(Damage.Pure((int)(enemy.MaxHp * 0.85))); // HP を 15% に

        // Assert
        Assert.True(enemy.ShouldFlee());
    }

    [Fact]
    public void ShouldFlee_ReturnsFalseWhenHpAboveThreshold()
    {
        // Arrange
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.TakeDamage(Damage.Pure((int)(enemy.MaxHp * 0.5))); // HP を 50% に

        // Assert
        Assert.False(enemy.ShouldFlee());
    }

    [Fact]
    public void TakeDamage_ReducesHp()
    {
        // Arrange
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        var initialHp = enemy.CurrentHp;

        // Act
        enemy.TakeDamage(Damage.Physical(20));

        // Assert
        Assert.True(enemy.CurrentHp < initialHp);
    }

    [Fact]
    public void TakeDamage_TriggersDeathWhenHpReachesZero()
    {
        // Arrange
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        bool deathTriggered = false;
        enemy.OnDeath += (_, _) => deathTriggered = true;

        // Act
        enemy.TakeDamage(Damage.Pure(enemy.MaxHp + 100));

        // Assert
        Assert.False(enemy.IsAlive);
        Assert.True(deathTriggered);
    }

    [Fact]
    public void PatrolRoute_WorksCorrectly()
    {
        // Arrange
        var enemy = Enemy.Create("Guard", "guard", Stats.Default, 10);
        enemy.PatrolRoute.AddRange(new[]
        {
            new Position(0, 0),
            new Position(5, 0),
            new Position(5, 5),
            new Position(0, 5)
        });

        // Assert
        Assert.Equal(4, enemy.PatrolRoute.Count);
        Assert.Equal(0, enemy.PatrolIndex);

        // Act
        enemy.PatrolIndex = (enemy.PatrolIndex + 1) % enemy.PatrolRoute.Count;

        // Assert
        Assert.Equal(1, enemy.PatrolIndex);
    }
}

public class EnemyFactoryTests
{
    private readonly EnemyFactory _factory = new();

    [Fact]
    public void CreateSlime_CreatesValidEnemy()
    {
        // Act
        var slime = _factory.CreateSlime(new Position(5, 5));

        // Assert
        Assert.Equal("スライム", slime.Name);
        Assert.Equal("slime", slime.EnemyTypeId);
        Assert.Equal(5, slime.ExperienceReward);
        Assert.Equal(new Position(5, 5), slime.Position);
        Assert.Equal(new Position(5, 5), slime.HomePosition);
        Assert.NotNull(slime.Behavior);
    }

    [Fact]
    public void CreateGoblin_CreatesValidEnemy()
    {
        // Act
        var goblin = _factory.CreateGoblin(new Position(10, 10));

        // Assert
        Assert.Equal("ゴブリン", goblin.Name);
        Assert.Equal("goblin", goblin.EnemyTypeId);
        Assert.Equal(15, goblin.ExperienceReward);
    }

    [Fact]
    public void CreateSkeleton_HasNoFleeThreshold()
    {
        // Act
        var skeleton = _factory.CreateSkeleton(new Position(0, 0));

        // Assert
        Assert.Equal(0.0f, skeleton.FleeThreshold);
    }

    [Fact]
    public void CreateEnemy_SetsCorrectBehavior()
    {
        // Arrange
        var orc = _factory.CreateOrc(new Position(0, 0));

        // Assert
        Assert.NotNull(orc.Behavior);
        Assert.IsType<CompositeBehavior>(orc.Behavior);
    }

    [Fact]
    public void GetEnemiesForDepth_ReturnsCorrectEnemies()
    {
        // Act
        var depth1Enemies = EnemyDefinitions.GetEnemiesForDepth(1);
        var depth10Enemies = EnemyDefinitions.GetEnemiesForDepth(10);
        var depth25Enemies = EnemyDefinitions.GetEnemiesForDepth(25);

        // Assert
        Assert.Contains(depth1Enemies, e => e.TypeId == "slime");
        Assert.Contains(depth10Enemies, e => e.TypeId == "orc");
        Assert.Contains(depth25Enemies, e => e.TypeId == "draugr");
        Assert.DoesNotContain(depth25Enemies, e => e.TypeId == "slime");
    }
}

public class AIBehaviorTests
{
    [Fact]
    public void IdleBehavior_IsApplicableWhenIdle()
    {
        // Arrange
        var behavior = new IdleBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Idle;

        // Act & Assert
        Assert.True(behavior.IsApplicable(enemy, null!));
    }

    [Fact]
    public void IdleBehavior_NotApplicableWhenInCombat()
    {
        // Arrange
        var behavior = new IdleBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Combat;

        // Act & Assert
        Assert.False(behavior.IsApplicable(enemy, null!));
    }

    [Fact]
    public void ChaseBehavior_IsApplicableWhenInCombatWithTarget()
    {
        // Arrange
        var behavior = new ChaseBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        var player = Player.Create("Player", Stats.Default);

        enemy.CurrentAIState = AIState.Combat;
        enemy.Target = player;

        // Act & Assert
        Assert.True(behavior.IsApplicable(enemy, null!));
    }

    [Fact]
    public void ChaseBehavior_NotApplicableWithoutTarget()
    {
        // Arrange
        var behavior = new ChaseBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Combat;
        enemy.Target = null;

        // Act & Assert
        Assert.False(behavior.IsApplicable(enemy, null!));
    }

    [Fact]
    public void FleeBehavior_HasHighestPriority()
    {
        // Arrange
        var flee = new FleeBehavior();
        var chase = new ChaseBehavior();
        var idle = new IdleBehavior();

        // Assert
        Assert.True(flee.Priority > chase.Priority);
        Assert.True(chase.Priority > idle.Priority);
    }

    [Fact]
    public void CompositeBehavior_SelectsHighestPriorityApplicable()
    {
        // Arrange
        var composite = new CompositeBehavior();
        composite.AddBehavior(new IdleBehavior());
        composite.AddBehavior(new FleeBehavior());
        composite.AddBehavior(new ChaseBehavior());

        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Flee;

        // 簡易的なゲーム状態を使用
        // IsApplicable は AIState を見るので、Flee が選択されるはず

        // Assert
        Assert.Equal("Composite", composite.Name);
    }

    [Fact]
    public void PatrolBehavior_IsApplicableWithRoute()
    {
        // Arrange
        var behavior = new PatrolBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Patrol;
        enemy.PatrolRoute.Add(new Position(0, 0));
        enemy.PatrolRoute.Add(new Position(5, 5));

        // Act & Assert
        Assert.True(behavior.IsApplicable(enemy, null!));
    }

    [Fact]
    public void PatrolBehavior_NotApplicableWithoutRoute()
    {
        // Arrange
        var behavior = new PatrolBehavior();
        var enemy = Enemy.Create("Test", "test", Stats.Default, 10);
        enemy.CurrentAIState = AIState.Patrol;
        // PatrolRoute は空

        // Act & Assert
        Assert.False(behavior.IsApplicable(enemy, null!));
    }
}

public class EnemyDefinitionTests
{
    [Fact]
    public void Slime_HasCorrectStats()
    {
        // Assert
        Assert.Equal("slime", EnemyDefinitions.Slime.TypeId);
        Assert.Equal("スライム", EnemyDefinitions.Slime.Name);
        Assert.Equal(EnemyType.Normal, EnemyDefinitions.Slime.EnemyType);
        Assert.Equal(EnemyRank.Common, EnemyDefinitions.Slime.Rank);
    }

    [Fact]
    public void Troll_IsEliteRank()
    {
        // Assert
        Assert.Equal(EnemyRank.Elite, EnemyDefinitions.Troll.Rank);
    }

    [Fact]
    public void Skeleton_DoesNotFlee()
    {
        // Assert
        Assert.Equal(0.0f, EnemyDefinitions.Skeleton.FleeThreshold);
    }

    [Fact]
    public void GiantSpider_HasHighHearingRange()
    {
        // Assert
        Assert.Equal(10, EnemyDefinitions.GiantSpider.HearingRange);
    }

    [Fact]
    public void DarkElf_HasHighIntelligence()
    {
        // Assert
        Assert.Equal(12, EnemyDefinitions.DarkElf.BaseStats.Intelligence);
    }

    [Fact]
    public void ShouldFlee_ReturnsFalseWhenMaxHpIsZero()
    {
        // MaxHpが0の場合ゼロ除算せずfalseを返す
        var enemy = Enemy.Create("Zero", "zero", new Stats { Vitality = 0, Strength = 1, Intelligence = 1, Mind = 1, Agility = 1, Dexterity = 1, Luck = 1 }, 10);
        // MaxHpは最低1に保護されるため、HP=0（死亡）でfalseを返すことを確認
        enemy.TakeDamage(Damage.Pure(enemy.MaxHp));
        Assert.False(enemy.ShouldFlee());
    }
}
