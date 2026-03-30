using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

public class AIBehaviorExtendedTests
{
    #region Mocks

    private class FakeBehavior : IAIBehavior
    {
        public string Name { get; set; } = "Fake";
        public int Priority { get; set; }
        public bool Applicable { get; set; } = true;
        public TurnAction ActionToReturn { get; set; } = TurnAction.Wait;

        public TurnAction DecideAction(Enemy enemy, IGameState state) => ActionToReturn;
        public bool IsApplicable(Enemy enemy, IGameState state) => Applicable;
    }

    private class MockGameState : IGameState
    {
        public IPlayer Player { get; set; } = null!;
        public IMap CurrentMap { get; set; } = null!;
        public ICombatSystem CombatSystem { get; set; } = null!;
        public IRandomProvider Random { get; set; } = null!;
        public CombatState CombatState { get; set; } = CombatState.Normal;
        public long CurrentTurn { get; set; }
        public float GetMovementModifier(IEntity entity) => 1.0f;
    }

    #endregion

    private static Enemy CreateEnemy(int vitality = 5)
    {
        var enemy = Enemy.Create("TestEnemy", "test_enemy", new Stats(0, vitality, 0, 0, 0, 0, 0, 0, 0), 10);
        enemy.Position = new Position(5, 5);
        enemy.HomePosition = new Position(5, 5);
        return enemy;
    }

    private static MockGameState CreateMockState() => new();

    #region CompositeBehavior テスト

    [Fact]
    public void CompositeBehavior_AddBehavior_SortsByPriority()
    {
        var composite = new CompositeBehavior();
        var low = new FakeBehavior { Priority = 10, ActionToReturn = TurnAction.Rest };
        var high = new FakeBehavior { Priority = 50, ActionToReturn = TurnAction.Search };
        var mid = new FakeBehavior { Priority = 30, ActionToReturn = TurnAction.Wait };

        composite.AddBehavior(low);
        composite.AddBehavior(mid);
        composite.AddBehavior(high);

        var action = composite.DecideAction(CreateEnemy(), CreateMockState());

        // 最高優先度(50)のSearchが選ばれる
        Assert.Equal(TurnActionType.Search, action.Type);
    }

    [Fact]
    public void CompositeBehavior_DecideAction_UsesHighestPriorityApplicable()
    {
        var composite = new CompositeBehavior();
        var high = new FakeBehavior { Priority = 50, ActionToReturn = TurnAction.Search, Applicable = true };
        var low = new FakeBehavior { Priority = 10, ActionToReturn = TurnAction.Rest, Applicable = true };

        composite.AddBehavior(low);
        composite.AddBehavior(high);

        var action = composite.DecideAction(CreateEnemy(), CreateMockState());

        Assert.Equal(TurnActionType.Search, action.Type);
    }

    [Fact]
    public void CompositeBehavior_DecideAction_SkipsNonApplicable()
    {
        var composite = new CompositeBehavior();
        var high = new FakeBehavior { Priority = 50, ActionToReturn = TurnAction.Search, Applicable = false };
        var low = new FakeBehavior { Priority = 10, ActionToReturn = TurnAction.Rest, Applicable = true };

        composite.AddBehavior(high);
        composite.AddBehavior(low);

        var action = composite.DecideAction(CreateEnemy(), CreateMockState());

        Assert.Equal(TurnActionType.Rest, action.Type);
    }

    [Fact]
    public void CompositeBehavior_DecideAction_ReturnsWait_WhenNoneApplicable()
    {
        var composite = new CompositeBehavior();
        composite.AddBehavior(new FakeBehavior { Priority = 50, Applicable = false });
        composite.AddBehavior(new FakeBehavior { Priority = 10, Applicable = false });

        var action = composite.DecideAction(CreateEnemy(), CreateMockState());

        Assert.Equal(TurnActionType.Wait, action.Type);
    }

    [Fact]
    public void CompositeBehavior_RemoveBehavior_RemovesCorrectly()
    {
        var composite = new CompositeBehavior();
        var behavior = new FakeBehavior { Priority = 50, ActionToReturn = TurnAction.Search, Applicable = true };

        composite.AddBehavior(behavior);
        composite.RemoveBehavior(behavior);

        var action = composite.DecideAction(CreateEnemy(), CreateMockState());

        // ビヘイビアが無いのでデフォルトのWaitが返る
        Assert.Equal(TurnActionType.Wait, action.Type);
    }

    #endregion

    #region IsApplicable テスト

    [Fact]
    public void IdleBehavior_IsApplicable_WhenIdle()
    {
        var behavior = new IdleBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Idle;

        Assert.True(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void IdleBehavior_NotApplicable_WhenNotIdle()
    {
        var behavior = new IdleBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Combat;

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void ChaseBehavior_IsApplicable_WhenCombatWithTarget()
    {
        var behavior = new ChaseBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Combat;
        enemy.Target = CreateEnemy();

        Assert.True(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void ChaseBehavior_NotApplicable_WhenNoTarget()
    {
        var behavior = new ChaseBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Combat;
        enemy.Target = null;

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void FleeBehavior_IsApplicable_WhenFlee()
    {
        var behavior = new FleeBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Flee;

        Assert.True(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void FleeBehavior_NotApplicable_WhenNotFlee()
    {
        var behavior = new FleeBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Idle;

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void AlertBehavior_IsApplicable_WhenAlert()
    {
        var behavior = new AlertBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Alert;

        Assert.True(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void AlertBehavior_NotApplicable_WhenNotAlert()
    {
        var behavior = new AlertBehavior();
        var enemy = CreateEnemy();
        enemy.CurrentAIState = AIState.Idle;

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void BerserkerBehavior_IsApplicable_WhenHpBelowThreshold()
    {
        var behavior = new BerserkerBehavior(0.4f);
        var enemy = CreateEnemy(vitality: 5); // MaxHp = 50 + 50 = 100
        enemy.TakeDamage(Damage.Pure(61));    // CurrentHp = 39, ratio = 0.39

        Assert.True(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void BerserkerBehavior_NotApplicable_WhenHpAboveThreshold()
    {
        var behavior = new BerserkerBehavior(0.4f);
        var enemy = CreateEnemy(vitality: 5); // MaxHp = 100, CurrentHp = 100

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    [Fact]
    public void BerserkerBehavior_NotApplicable_WhenDead()
    {
        var behavior = new BerserkerBehavior(0.4f);
        var enemy = CreateEnemy(vitality: 5);
        enemy.TakeDamage(Damage.Pure(100)); // CurrentHp = 0, ratio = 0.0

        Assert.False(behavior.IsApplicable(enemy, CreateMockState()));
    }

    #endregion

    #region 名前・優先度テスト

    public static IEnumerable<object[]> BehaviorNamePriorityData => new List<object[]>
    {
        new object[] { new IdleBehavior(), "Idle", 0 },
        new object[] { new PatrolBehavior(), "Patrol", 10 },
        new object[] { new AlertBehavior(), "Alert", 30 },
        new object[] { new ChaseBehavior(), "Chase", 50 },
        new object[] { new FleeBehavior(), "Flee", 100 },
        new object[] { new AggressiveBehavior(), "Aggressive", 60 },
        new object[] { new DefensiveBehavior(), "Defensive", 40 },
        new object[] { new AmbushBehavior(), "Ambush", 70 },
        new object[] { new RangedBehavior(), "Ranged", 55 },
        new object[] { new BerserkerBehavior(), "Berserker", 80 },
        new object[] { new SummonerBehavior(), "Summoner", 65 },
    };

    [Theory]
    [MemberData(nameof(BehaviorNamePriorityData))]
    public void Behavior_HasCorrectNameAndPriority(IAIBehavior behavior, string expectedName, int expectedPriority)
    {
        Assert.Equal(expectedName, behavior.Name);
        Assert.Equal(expectedPriority, behavior.Priority);
    }

    [Fact]
    public void CompositeBehavior_HasCorrectNameAndPriority()
    {
        var composite = new CompositeBehavior();

        Assert.Equal("Composite", composite.Name);
        Assert.Equal(0, composite.Priority);
    }

    #endregion
}
