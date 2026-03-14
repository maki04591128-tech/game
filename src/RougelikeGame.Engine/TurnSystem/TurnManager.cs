using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Engine.TurnSystem;

/// <summary>
/// ターン制御を管理するクラス
/// </summary>
public class TurnManager : ITurnManager
{
    private readonly PriorityQueue<(ITurnActor Actor, long NextTurn), long> _turnQueue;
    private long _currentTurn;

    public bool IsPlayerTurn => CurrentActor is IPlayer;
    public ITurnActor? CurrentActor { get; private set; }
    public long CurrentTurn => _currentTurn;
    public CombatState CombatState { get; set; } = CombatState.Normal;

    public TurnManager()
    {
        _turnQueue = new PriorityQueue<(ITurnActor, long), long>();
    }

    public void RegisterActor(ITurnActor actor)
    {
        _turnQueue.Enqueue((actor, _currentTurn), _currentTurn);
    }

    public void UnregisterActor(ITurnActor actor)
    {
        // 優先度キューからの削除（再構築が必要）
        var items = new List<(ITurnActor Actor, long NextTurn)>();

        while (_turnQueue.Count > 0)
        {
            var item = _turnQueue.Dequeue();
            if (!ReferenceEquals(item.Actor, actor))
            {
                items.Add(item);
            }
        }

        foreach (var item in items)
        {
            _turnQueue.Enqueue(item, item.NextTurn);
        }
    }

    public void ProcessTurn(IGameState state)
    {
        if (_turnQueue.Count == 0) return;

        // 次に行動するアクターを取得
        var (actor, scheduledTurn) = _turnQueue.Dequeue();
        CurrentActor = actor;
        _currentTurn = scheduledTurn;

        // アクションを決定
        var action = actor.DecideAction(state);

        // 入力待ちの場合（プレイヤー）
        if (action.BaseTurnCost == 0)
        {
            _turnQueue.Enqueue((actor, _currentTurn), _currentTurn);
            OnWaitingForInput?.Invoke(this, EventArgs.Empty);
            return;
        }

        // アクションを実行
        actor.ExecuteAction(action, state);

        // 次の行動ターンを計算
        int turnCost = action.CalculateFinalCost(
            CombatState,
            1.0f,  // equipmentModifier
            1.0f,  // statusModifier
            1.0f   // environmentModifier
        );

        long nextTurn = _currentTurn + turnCost;

        // 生存チェック（IDamageable を実装している場合）
        bool isAlive = actor is not IDamageable damageable || damageable.IsAlive;

        if (isAlive)
        {
            _turnQueue.Enqueue((actor, nextTurn), nextTurn);
        }

        // イベント発火
        OnTurnProcessed?.Invoke(this, new TurnProcessedEventArgs(actor, action, turnCost));
    }

    /// <summary>
    /// プレイヤーの行動を実行（入力受付後）
    /// </summary>
    public void ExecutePlayerAction(TurnAction action, IGameState state)
    {
        if (CurrentActor == null) return;

        CurrentActor.ExecuteAction(action, state);

        int turnCost = action.CalculateFinalCost(
            CombatState,
            1.0f,
            1.0f,
            1.0f
        );

        long nextTurn = _currentTurn + turnCost;

        _turnQueue.Enqueue((CurrentActor, nextTurn), nextTurn);

        OnTurnProcessed?.Invoke(this, new TurnProcessedEventArgs(CurrentActor, action, turnCost));
    }

    public TimeSpan GetGameTime() => TimeSpan.FromSeconds(_currentTurn);
    public int GetGameDay() => (int)(_currentTurn / TimeConstants.TurnsPerDay) + 1;
    public int GetGameHour() => (int)((_currentTurn % TimeConstants.TurnsPerDay) / TimeConstants.TurnsPerHour);
    public int GetGameMinute() => (int)((_currentTurn % TimeConstants.TurnsPerHour) / TimeConstants.TurnsPerMinute);

    /// <summary>
    /// ゲーム内時刻を文字列で取得
    /// </summary>
    public string GetGameTimeString()
    {
        int hour = GetGameHour();
        int minute = GetGameMinute();
        return $"{hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// 昼か夜かを取得
    /// </summary>
    public bool IsNightTime()
    {
        int hour = GetGameHour();
        return hour < 6 || hour >= 20;  // 20:00 - 06:00 が夜
    }

    public event EventHandler<TurnProcessedEventArgs>? OnTurnProcessed;
    public event EventHandler? OnWaitingForInput;
    public event EventHandler<CombatStateChangedEventArgs>? OnCombatStateChanged;

    /// <summary>
    /// 戦闘状態を変更
    /// </summary>
    public void SetCombatState(CombatState newState)
    {
        if (CombatState != newState)
        {
            var old = CombatState;
            CombatState = newState;
            OnCombatStateChanged?.Invoke(this, new CombatStateChangedEventArgs(old, newState));
        }
    }
}

/// <summary>
/// ターン処理完了イベント引数
/// </summary>
public class TurnProcessedEventArgs : EventArgs
{
    public ITurnActor Actor { get; }
    public TurnAction Action { get; }
    public int TurnCost { get; }

    public TurnProcessedEventArgs(ITurnActor actor, TurnAction action, int turnCost)
    {
        Actor = actor;
        Action = action;
        TurnCost = turnCost;
    }
}

/// <summary>
/// 戦闘状態変更イベント引数
/// </summary>
public class CombatStateChangedEventArgs : EventArgs
{
    public CombatState OldState { get; }
    public CombatState NewState { get; }

    public CombatStateChangedEventArgs(CombatState oldState, CombatState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
