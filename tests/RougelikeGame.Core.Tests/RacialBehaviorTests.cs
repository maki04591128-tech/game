using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Map;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.15 敵AI拡張（種族ビヘイビア）のテスト
/// </summary>
public class RacialBehaviorTests
{
    #region Behavior Existence Tests

    [Fact]
    public void PackHuntingBehavior_HasCorrectProperties()
    {
        var behavior = new PackHuntingBehavior();
        Assert.Equal("PackHunting", behavior.Name);
        Assert.Equal(55, behavior.Priority);
    }

    [Fact]
    public void UndeadBehavior_HasCorrectProperties()
    {
        var behavior = new UndeadBehavior();
        Assert.Equal("Undead", behavior.Name);
        Assert.Equal(105, behavior.Priority);
    }

    [Fact]
    public void AmorphousBehavior_HasCorrectProperties()
    {
        var behavior = new AmorphousBehavior();
        Assert.Equal("Amorphous", behavior.Name);
        Assert.Equal(15, behavior.Priority);
    }

    [Fact]
    public void ConstructBehavior_HasCorrectProperties()
    {
        var behavior = new ConstructBehavior();
        Assert.Equal("Construct", behavior.Name);
        Assert.Equal(45, behavior.Priority);
    }

    [Fact]
    public void DragonBehavior_HasCorrectProperties()
    {
        var behavior = new DragonBehavior();
        Assert.Equal("Dragon", behavior.Name);
        Assert.Equal(75, behavior.Priority);
    }

    [Fact]
    public void SpiritBehavior_HasCorrectProperties()
    {
        var behavior = new SpiritBehavior();
        Assert.Equal("Spirit", behavior.Name);
        Assert.Equal(35, behavior.Priority);
    }

    #endregion

    #region Applicability Tests

    [Fact]
    public void PackHuntingBehavior_OnlyApplicableToBeast()
    {
        var beast = CreateEnemy(MonsterRace.Beast, AIState.Combat);
        var human = CreateEnemy(MonsterRace.Humanoid, AIState.Combat);
        var behavior = new PackHuntingBehavior();

        // 直接テスト用にTarget設定
        beast.Target = CreateDummyTarget();
        human.Target = CreateDummyTarget();

        Assert.True(behavior.IsApplicable(beast, CreateMockState()));
        Assert.False(behavior.IsApplicable(human, CreateMockState()));
    }

    [Fact]
    public void UndeadBehavior_OnlyApplicableToUndead()
    {
        var undead = CreateEnemy(MonsterRace.Undead, AIState.Combat, lowHp: true);
        var beast = CreateEnemy(MonsterRace.Beast, AIState.Combat, lowHp: true);
        var behavior = new UndeadBehavior();

        Assert.True(behavior.IsApplicable(undead, CreateMockState()));
        Assert.False(behavior.IsApplicable(beast, CreateMockState()));
    }

    [Fact]
    public void AmorphousBehavior_OnlyApplicableToAmorphousWhenIdle()
    {
        var amorphous = CreateEnemy(MonsterRace.Amorphous, AIState.Idle);
        var amorphousCombat = CreateEnemy(MonsterRace.Amorphous, AIState.Combat);
        var behavior = new AmorphousBehavior();

        Assert.True(behavior.IsApplicable(amorphous, CreateMockState()));
        Assert.False(behavior.IsApplicable(amorphousCombat, CreateMockState()));
    }

    [Fact]
    public void ConstructBehavior_RequiresTarget()
    {
        var construct = CreateEnemy(MonsterRace.Construct, AIState.Combat);
        var constructNoTarget = CreateEnemy(MonsterRace.Construct, AIState.Combat);
        var behavior = new ConstructBehavior();

        construct.Target = CreateDummyTarget();

        Assert.True(behavior.IsApplicable(construct, CreateMockState()));
        Assert.False(behavior.IsApplicable(constructNoTarget, CreateMockState()));
    }

    [Fact]
    public void DragonBehavior_OnlyApplicableToDragonInCombat()
    {
        var dragon = CreateEnemy(MonsterRace.Dragon, AIState.Combat);
        var dragonIdle = CreateEnemy(MonsterRace.Dragon, AIState.Idle);
        var behavior = new DragonBehavior();

        dragon.Target = CreateDummyTarget();
        dragonIdle.Target = CreateDummyTarget();

        Assert.True(behavior.IsApplicable(dragon, CreateMockState()));
        Assert.False(behavior.IsApplicable(dragonIdle, CreateMockState()));
    }

    [Fact]
    public void SpiritBehavior_ApplicableInCombatAndAlert()
    {
        var spiritCombat = CreateEnemy(MonsterRace.Spirit, AIState.Combat);
        var spiritAlert = CreateEnemy(MonsterRace.Spirit, AIState.Alert);
        var spiritIdle = CreateEnemy(MonsterRace.Spirit, AIState.Idle);
        var behavior = new SpiritBehavior();

        Assert.True(behavior.IsApplicable(spiritCombat, CreateMockState()));
        Assert.True(behavior.IsApplicable(spiritAlert, CreateMockState()));
        Assert.False(behavior.IsApplicable(spiritIdle, CreateMockState()));
    }

    #endregion

    #region EnemyFactory Integration Tests

    [Fact]
    public void EnemyFactory_CreateEnemy_SetsRacialBehavior()
    {
        var factory = new EnemyFactory();
        var def = EnemyDefinitions.ForestWolf;  // Beast
        var enemy = factory.CreateEnemy(def, new Position(0, 0));

        Assert.Equal(MonsterRace.Beast, enemy.Race);
        Assert.NotNull(enemy.Behavior);
    }

    [Fact]
    public void EnemyFactory_AllDefinitions_HaveValidRace()
    {
        var allEnemies = EnemyDefinitions.GetAllEnemies();
        foreach (var def in allEnemies)
        {
            Assert.True(Enum.IsDefined(typeof(MonsterRace), def.Race),
                $"{def.Name} has invalid race {def.Race}");
        }
    }

    #endregion

    #region Helper Methods

    private static Enemy CreateEnemy(MonsterRace race, AIState state, bool lowHp = false)
    {
        var factory = new EnemyFactory();
        var def = new EnemyDefinition(
            TypeId: "test",
            Name: "Test",
            Description: "Test enemy",
            BaseStats: new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10),
            EnemyType: EnemyType.Normal,
            Rank: EnemyRank.Common,
            ExperienceReward: 10,
            Race: race
        );
        var enemy = factory.CreateEnemy(def, new Position(10, 10));
        enemy.CurrentAIState = state;

        if (lowHp)
        {
            // HP を FleeThreshold (20%) 以下にする
            int targetHp = (int)(enemy.MaxHp * 0.1f);
            enemy.TakeDamage(new Damage(enemy.MaxHp - targetHp, DamageType.Pure, Element.None, false));
        }

        return enemy;
    }

    private static Character CreateDummyTarget()
    {
        return Enemy.Create("Target", "target", new Stats(5, 5, 5, 5, 5, 5, 5, 5, 5), 5);
    }

    private static TestGameState CreateMockState()
    {
        return new TestGameState();
    }

    /// <summary>
    /// テスト用のダミーGameState
    /// </summary>
    private class TestGameState : IGameState
    {
        public IPlayer Player { get; } = null!;
        public IMap CurrentMap { get; } = new TestMap();
        public ICombatSystem CombatSystem { get; } = null!;
        public IRandomProvider Random { get; } = new TestRandom();
        public CombatState CombatState => CombatState.None;
        public long CurrentTurn => 0;
        public float GetMovementModifier(IEntity entity) => 1.0f;
    }

    private class TestMap : IMap
    {
        public int Width => 50;
        public int Height => 50;
        public bool InBounds(Position pos) => pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        public bool IsWalkable(Position pos) => true;
        public bool BlocksSight(Position pos) => false;
        public bool CanMoveTo(Position pos) => pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        public bool HasLineOfSight(Position from, Position to) => true;
        public float GetEnvironmentModifier(Position pos, TurnActionType actionType) => 1.0f;
        public Tile GetTile(Position pos) => new Tile { Type = TileType.Floor };
        public void SetTile(Position pos, Tile tile) { }
    }

    private class TestRandom : IRandomProvider
    {
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => 0.5;
    }

    #endregion
}
