using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Engine.TurnSystem;

namespace RougelikeGame.Core.Tests;

public class TurnManagerTests
{
    #region Mocks

    private class MockTurnActor : ITurnActor
    {
        public int ActionPriority { get; set; } = 10;
        public TurnAction ActionToReturn { get; set; } = TurnAction.Wait;
        public bool ExecuteActionCalled { get; private set; }

        public TurnAction DecideAction(IGameState state) => ActionToReturn;
        public void ExecuteAction(TurnAction action, IGameState state) => ExecuteActionCalled = true;
    }

    private class MockDamageableActor : ITurnActor, IDamageable
    {
        public int ActionPriority { get; set; } = 10;
        public TurnAction ActionToReturn { get; set; } = TurnAction.Wait;
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; } = 100;
        public bool IsAlive { get; set; }

        public TurnAction DecideAction(IGameState state) => ActionToReturn;
        public void ExecuteAction(TurnAction action, IGameState state) { }
        public void TakeDamage(Damage damage) { }
        public void Heal(int amount) { }

#pragma warning disable CS0067
        public event EventHandler<DamageEventArgs>? OnDamaged;
        public event EventHandler<DeathEventArgs>? OnDeath;
#pragma warning restore CS0067
    }

    private class MockInventory : IInventory
    {
        public int MaxSlots => 30;
        public int UsedSlots => 0;
        public IReadOnlyList<IItem> Items { get; } = Array.Empty<IItem>();
        public bool Add(IItem item) => true;
        public bool Remove(IItem item) => true;
        public bool Contains(IItem item) => false;
    }

    private class MockPlayer : IPlayer
    {
        public string Name { get; set; } = "TestPlayer";
        public int Level { get; set; } = 1;
        public int Sanity { get; set; } = 100;
        public int Hunger { get; set; } = 100;
        public int ActionPriority { get; set; } = 10;
        public TurnAction ActionToReturn { get; set; } = TurnAction.WaitForInput;
        public int CurrentHp { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public bool IsAlive { get; set; } = true;
        public IInventory Inventory { get; } = new MockInventory();

        public TurnAction DecideAction(IGameState state) => ActionToReturn;
        public void ExecuteAction(TurnAction action, IGameState state) { }
        public void TakeDamage(Damage damage) { }
        public void Heal(int amount) { }
        public bool CanPickUp(IItem item) => true;
        public bool PickUp(IItem item) { return true; }
        public void Drop(IItem item) { }

#pragma warning disable CS0067
        public event EventHandler<DamageEventArgs>? OnDamaged;
        public event EventHandler<DeathEventArgs>? OnDeath;
#pragma warning restore CS0067
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

    private readonly TurnManager _sut = new();
    private readonly MockGameState _state = new();

    /// <summary>
    /// ターンを指定値まで進めるヘルパー
    /// </summary>
    private void AdvanceToTurn(long targetTurn)
    {
        var actor = new MockTurnActor
        {
            ActionToReturn = new TurnAction { Type = TurnActionType.Wait, BaseTurnCost = (int)targetTurn }
        };
        _sut.RegisterActor(actor);
        _sut.ProcessTurn(_state); // _currentTurn = 0, re-enqueue at targetTurn
        _sut.ProcessTurn(_state); // _currentTurn = targetTurn
        _sut.UnregisterActor(actor);
    }

    #region コンストラクタ・登録テスト

    [Fact]
    public void Constructor_Default_CreatesEmptyTurnManager()
    {
        var tm = new TurnManager();

        Assert.Null(tm.CurrentActor);
        Assert.Equal(0, tm.CurrentTurn);
        Assert.Equal(CombatState.Normal, tm.CombatState);
    }

    [Fact]
    public void RegisterActor_Single_AddsActorToQueue()
    {
        var actor = new MockTurnActor();

        _sut.RegisterActor(actor);
        _sut.ProcessTurn(_state);

        Assert.Same(actor, _sut.CurrentActor);
    }

    [Fact]
    public void UnregisterActor_Registered_RemovesFromQueue()
    {
        var actor = new MockTurnActor();
        _sut.RegisterActor(actor);
        _sut.UnregisterActor(actor);

        TurnProcessedEventArgs? eventArgs = null;
        _sut.OnTurnProcessed += (_, e) => eventArgs = e;
        _sut.ProcessTurn(_state);

        Assert.Null(eventArgs);
    }

    #endregion

    #region ProcessTurnテスト

    [Fact]
    public void ProcessTurn_EmptyQueue_DoesNothing()
    {
        var initialTurn = _sut.CurrentTurn;

        _sut.ProcessTurn(_state);

        Assert.Equal(initialTurn, _sut.CurrentTurn);
        Assert.Null(_sut.CurrentActor);
    }

    [Fact]
    public void ProcessTurn_WithActor_ExecutesAction()
    {
        var actor = new MockTurnActor { ActionToReturn = TurnAction.Wait };
        _sut.RegisterActor(actor);

        _sut.ProcessTurn(_state);

        Assert.True(actor.ExecuteActionCalled);
    }

    [Fact]
    public void ProcessTurn_DeadActor_RemovesFromQueue()
    {
        var deadActor = new MockDamageableActor { IsAlive = false };
        _sut.RegisterActor(deadActor);

        _sut.ProcessTurn(_state); // 処理されるが再登録されない

        var processed = false;
        _sut.OnTurnProcessed += (_, _) => processed = true;
        _sut.ProcessTurn(_state);

        Assert.False(processed);
    }

    [Fact]
    public void ProcessTurn_WithActor_FiresOnTurnProcessed()
    {
        var actor = new MockTurnActor { ActionToReturn = TurnAction.Wait };
        _sut.RegisterActor(actor);

        TurnProcessedEventArgs? eventArgs = null;
        _sut.OnTurnProcessed += (_, e) => eventArgs = e;
        _sut.ProcessTurn(_state);

        Assert.NotNull(eventArgs);
        Assert.Same(actor, eventArgs!.Actor);
        Assert.Equal(TurnActionType.Wait, eventArgs.Action.Type);
    }

    [Fact]
    public void ProcessTurn_ZeroCostAction_FiresOnWaitingForInput()
    {
        var actor = new MockTurnActor { ActionToReturn = TurnAction.WaitForInput };
        _sut.RegisterActor(actor);

        var waitingFired = false;
        _sut.OnWaitingForInput += (_, _) => waitingFired = true;
        _sut.ProcessTurn(_state);

        Assert.True(waitingFired);
        Assert.False(actor.ExecuteActionCalled);
    }

    #endregion

    #region ExecutePlayerActionテスト

    [Fact]
    public void ExecutePlayerAction_AfterWaitForInput_ProcessesAndReEnqueues()
    {
        var player = new MockPlayer { ActionToReturn = TurnAction.WaitForInput };
        _sut.RegisterActor(player);
        _sut.ProcessTurn(_state); // WaitForInput → CurrentActor = player

        TurnProcessedEventArgs? eventArgs = null;
        _sut.OnTurnProcessed += (_, e) => eventArgs = e;
        _sut.ExecutePlayerAction(TurnAction.Wait, _state);

        Assert.NotNull(eventArgs);
        Assert.Same(player, eventArgs!.Actor);
    }

    #endregion

    #region 時間関連テスト

    [Fact]
    public void GetGameTime_AtSpecificTurn_ReturnsCorrectTimeSpan()
    {
        AdvanceToTurn(3661);

        Assert.Equal(TimeSpan.FromSeconds(3661), _sut.GetGameTime());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(86399, 1)]
    [InlineData(86400, 2)]
    [InlineData(172800, 3)]
    public void GetGameDay_AtTurn_ReturnsCorrectDay(long turn, int expectedDay)
    {
        if (turn > 0) AdvanceToTurn(turn);

        Assert.Equal(expectedDay, _sut.GetGameDay());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3600, 1)]
    [InlineData(36000, 10)]
    [InlineData(72000, 20)]
    public void GetGameHour_AtTurn_ReturnsCorrectHour(long turn, int expectedHour)
    {
        if (turn > 0) AdvanceToTurn(turn);

        Assert.Equal(expectedHour, _sut.GetGameHour());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(60, 1)]
    [InlineData(120, 2)]
    [InlineData(1800, 30)]
    public void GetGameMinute_AtTurn_ReturnsCorrectMinute(long turn, int expectedMinute)
    {
        if (turn > 0) AdvanceToTurn(turn);

        Assert.Equal(expectedMinute, _sut.GetGameMinute());
    }

    [Fact]
    public void GetGameTimeString_AtSpecificTurn_ReturnsFormattedString()
    {
        AdvanceToTurn(3600 * 14 + 60 * 30); // 14:30

        Assert.Equal("14:30", _sut.GetGameTimeString());
    }

    #endregion

    #region 昼夜判定テスト

    [Theory]
    [InlineData(0)]       // 00:00
    [InlineData(18000)]   // 05:00
    [InlineData(72000)]   // 20:00
    [InlineData(82800)]   // 23:00
    public void IsNightTime_DuringNightHours_ReturnsTrue(long turn)
    {
        if (turn > 0) AdvanceToTurn(turn);

        Assert.True(_sut.IsNightTime());
    }

    [Theory]
    [InlineData(21600)]   // 06:00
    [InlineData(36000)]   // 10:00
    [InlineData(50400)]   // 14:00
    [InlineData(68400)]   // 19:00
    public void IsNightTime_DuringDayHours_ReturnsFalse(long turn)
    {
        AdvanceToTurn(turn);

        Assert.False(_sut.IsNightTime());
    }

    #endregion

    #region 戦闘状態テスト

    [Fact]
    public void SetCombatState_NewState_FiresOnCombatStateChanged()
    {
        CombatStateChangedEventArgs? eventArgs = null;
        _sut.OnCombatStateChanged += (_, e) => eventArgs = e;

        _sut.SetCombatState(CombatState.Combat);

        Assert.NotNull(eventArgs);
        Assert.Equal(CombatState.Normal, eventArgs!.OldState);
        Assert.Equal(CombatState.Combat, eventArgs.NewState);
    }

    [Fact]
    public void SetCombatState_SameState_DoesNotFireEvent()
    {
        var fired = false;
        _sut.OnCombatStateChanged += (_, _) => fired = true;

        _sut.SetCombatState(CombatState.Normal);

        Assert.False(fired);
    }

    #endregion

    #region IsPlayerTurn・ターン進行テスト

    [Fact]
    public void IsPlayerTurn_WhenCurrentActorIsPlayer_ReturnsTrue()
    {
        var player = new MockPlayer { ActionToReturn = TurnAction.WaitForInput };
        _sut.RegisterActor(player);
        _sut.ProcessTurn(_state);

        Assert.True(_sut.IsPlayerTurn);
    }

    [Fact]
    public void IsPlayerTurn_WhenCurrentActorIsNotPlayer_ReturnsFalse()
    {
        var actor = new MockTurnActor { ActionToReturn = TurnAction.Wait };
        _sut.RegisterActor(actor);
        _sut.ProcessTurn(_state);

        Assert.False(_sut.IsPlayerTurn);
    }

    [Fact]
    public void CurrentTurn_AfterMultipleProcessTurns_Advances()
    {
        var actor = new MockTurnActor { ActionToReturn = TurnAction.Wait };
        _sut.RegisterActor(actor);

        Assert.Equal(0, _sut.CurrentTurn);

        _sut.ProcessTurn(_state); // scheduledTurn=0 → _currentTurn=0
        _sut.ProcessTurn(_state); // scheduledTurn=1 → _currentTurn=1

        Assert.True(_sut.CurrentTurn > 0);
    }

    #endregion
}
