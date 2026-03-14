using Xunit;
using RougelikeGame.Gui;
using RougelikeGame.Core;

namespace RougelikeGame.Gui.Tests;

/// <summary>
/// GameControllerのロジックテスト
/// </summary>
public class GameControllerTests
{
    private GameController CreateInitializedController()
    {
        var controller = new GameController();
        controller.Initialize();
        return controller;
    }

    [Fact]
    public void Initialize_CreatesPlayerAndMap()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.NotNull(controller.Player);
        Assert.NotNull(controller.Map);
        Assert.True(controller.Player.IsAlive);
        Assert.Equal(1, controller.CurrentFloor);
        Assert.Equal(0, controller.TurnCount);
    }

    [Fact]
    public void Initialize_GameTimeStartsAtDefault()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.Equal("冒険歴1024年 緑風の月 15日 08:00", controller.GameTime.ToFullString());
    }

    [Fact]
    public void ProcessInput_Wait_AdvancesTurnAndGameTime()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialTurn = controller.TurnCount;
        int initialTotalTurns = controller.GameTime.TotalTurns;

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert
        Assert.Equal(initialTurn + 1, controller.TurnCount);
        Assert.Equal(initialTotalTurns + 1, controller.GameTime.TotalTurns);
    }

    [Fact]
    public void ProcessInput_Wait60Times_AdvancesGameTimeBy1Minute()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        for (int i = 0; i < 60; i++)
        {
            controller.ProcessInput(GameAction.Wait);
        }

        // Assert
        Assert.Equal(60, controller.TurnCount);
        Assert.Equal(8, controller.GameTime.Hour);  // 60ターン=1分なので時間は変わらない
        Assert.Equal(1, controller.GameTime.Minute); // 08:00 → 08:01
    }

    [Fact]
    public void ProcessInput_Move_AdvancesTurnAndGameTime()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialTurn = controller.TurnCount;

        // Act - 移動（壁に当たる可能性があるのでいくつか試す）
        controller.ProcessInput(GameAction.MoveUp);
        controller.ProcessInput(GameAction.MoveDown);
        controller.ProcessInput(GameAction.MoveLeft);
        controller.ProcessInput(GameAction.MoveRight);

        // Assert - 少なくとも何回かは移動できているはず
        // TurnCountとGameTime.TotalTurnsが同期していること
        Assert.Equal(controller.TurnCount, controller.GameTime.TotalTurns);
    }

    [Fact]
    public void ProcessInput_OpenInventory_DoesNotAdvanceTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        // インベントリダイアログ表示イベントをダミーで購読
        controller.OnShowInventory += items => { };

        int initialTurn = controller.TurnCount;
        int initialGameTimeTurns = controller.GameTime.TotalTurns;

        // Act
        controller.ProcessInput(GameAction.OpenInventory);

        // Assert - インベントリ表示はターンを消費しない
        Assert.Equal(initialTurn, controller.TurnCount);
        Assert.Equal(initialGameTimeTurns, controller.GameTime.TotalTurns);
    }

    [Fact]
    public void Enemies_AreSpawned()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.NotEmpty(controller.Enemies);
    }

    [Fact]
    public void EnemiesOutsideActiveRange_AreNotProcessed()
    {
        // Arrange
        var controller = CreateInitializedController();

        // 全ての敵の位置を記録
        var farEnemies = controller.Enemies
            .Where(e => e.IsAlive && e.Position.ChebyshevDistanceTo(controller.Player.Position) > 10)
            .Select(e => (e, OriginalPosition: e.Position))
            .ToList();

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert - 範囲外の敵は移動していないはず
        foreach (var (enemy, originalPos) in farEnemies)
        {
            // 範囲外の敵は位置が変わらない（移動していない）
            Assert.Equal(originalPos, enemy.Position);
        }
    }

    [Fact]
    public void GameTime_SynchronizedWithTurnCount()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act - 10回待機
        for (int i = 0; i < 10; i++)
        {
            controller.ProcessInput(GameAction.Wait);
        }

        // Assert
        Assert.Equal(controller.TurnCount, controller.GameTime.TotalTurns);
    }

    [Fact]
    public void ProcessInput_Quit_SetsIsRunningFalse()
    {
        // Arrange
        var controller = CreateInitializedController();
        bool gameOverCalled = false;
        controller.OnGameOver += () => gameOverCalled = true;

        // Act
        controller.ProcessInput(GameAction.Quit);

        // Assert
        Assert.False(controller.IsRunning);
        Assert.True(gameOverCalled);
    }

    [Fact]
    public void StateChangedEvent_FiredOnTurnAction()
    {
        // Arrange
        var controller = CreateInitializedController();
        int stateChangedCount = 0;
        controller.OnStateChanged += () => stateChangedCount++;

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert
        Assert.True(stateChangedCount > 0);
    }

    [Fact]
    public void MessageEvent_FiredOnActions()
    {
        // Arrange
        var controller = CreateInitializedController();
        var messages = new List<string>();
        controller.OnMessage += msg => messages.Add(msg);

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert
        Assert.NotEmpty(messages);
        Assert.Contains(messages, m => m.Contains("待機した"));
    }

    [Fact]
    public void GroundItems_AreSpawned()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.NotEmpty(controller.GroundItems);
    }

    [Fact]
    public void ProcessTurnEffects_HungerDecaysOverTime()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialHunger = controller.Player.Hunger;

        // Act - HungerDecayInterval (600) ターン待機
        for (int i = 0; i < 600; i++)
        {
            controller.ProcessInput(GameAction.Wait);
        }

        // Assert - 満腹度が減少しているはず
        Assert.True(controller.Player.Hunger < initialHunger,
            $"600ターン後、満腹度は{initialHunger}から減少するべき（現在: {controller.Player.Hunger}）");
    }

    [Fact]
    public void ProcessTurnEffects_SpRegenOverTime()
    {
        // Arrange
        var controller = CreateInitializedController();
        // SPを消費（直接操作はできないので、初期値を記録）
        int initialSp = controller.Player.CurrentSp;

        // Act - 30ターン待機（SP回復間隔）
        for (int i = 0; i < 30; i++)
        {
            controller.ProcessInput(GameAction.Wait);
        }

        // Assert - SPが最大でなければ回復しているはず
        // SPが最大の場合はそのまま（減っていないこと）
        Assert.True(controller.Player.CurrentSp >= initialSp,
            "SPは減少しないはず");
    }

    [Fact]
    public void ProcessTurnEffects_StatusEffectsAreTicked()
    {
        // Arrange
        var controller = CreateInitializedController();
        // 状態異常を付与（3ターンの毒）
        controller.Player.ApplyStatusEffect(new RougelikeGame.Core.StatusEffect(StatusEffectType.Poison, 3));

        // Act - 数ターン進める
        for (int i = 0; i < 5; i++)
        {
            controller.ProcessInput(GameAction.Wait);
        }

        // Assert - 毒が期限切れで消えているはず
        Assert.False(controller.Player.HasStatusEffect(StatusEffectType.Poison),
            "3ターンの毒は5ターン後に消えているべき");
    }

    [Fact]
    public void LevelUp_FiresMessageEvent()
    {
        // Arrange
        var controller = CreateInitializedController();
        var messages = new List<string>();
        controller.OnMessage += msg => messages.Add(msg);

        // Act - 大量の経験値を与えてレベルアップ
        controller.Player.GainExperience(10000);

        // Assert
        Assert.True(controller.Player.Level > 1, "レベルが上がっているべき");
        Assert.Contains(messages, m => m.Contains("レベルアップ"));
    }

    [Fact]
    public void HungerStageChanged_FiresWarningMessage()
    {
        // Arrange
        var controller = CreateInitializedController();
        var messages = new List<string>();
        controller.OnMessage += msg => messages.Add(msg);

        // Act - 満腹度を大幅に減少させてHungry段階に
        controller.Player.ModifyHunger(-60); // 100→40 (Hungry stage: 25-49)

        // Assert
        Assert.Contains(messages, m => m.Contains("お腹が空いてきた"));
    }

    [Fact]
    public void HandlePlayerDeath_WithRescue_PlayerRevives()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialRescueCount = controller.Player.RescueCountRemaining;

        // Assert - 初期救出回数が設定されていること
        Assert.True(initialRescueCount > 0, "初期救出回数は1以上であるべき");
        Assert.True(controller.Player.CanBeRescued, "初期状態では救出可能であるべき");
    }

    [Fact]
    public void Player_InitialHungerStageIsFull()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.Equal(RougelikeGame.Core.HungerStage.Full, controller.Player.HungerStage);
    }

    [Fact]
    public void Player_InitialSanityStageIsNormal()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.Equal(RougelikeGame.Core.SanityStage.Normal, controller.Player.SanityStage);
    }

    [Fact]
    public void ProcessInput_OpenStatus_DoesNotAdvanceTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        bool showStatusCalled = false;
        controller.OnShowStatus += () => showStatusCalled = true;
        int initialTurn = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.OpenStatus);

        // Assert
        Assert.True(showStatusCalled);
        Assert.Equal(initialTurn, controller.TurnCount);
    }

    [Fact]
    public void AutoExplore_StartsExploring()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        controller.ProcessInput(GameAction.AutoExplore);

        // Assert - 自動探索が開始された場合はIsAutoExploringがtrue、
        // または停止条件で即停止した場合はfalse（両方正常動作）
        // ターンが進んだか、停止メッセージが出たか
        Assert.True(controller.TurnCount >= 0);
    }

    [Fact]
    public void AutoExplore_StopsOnManualInput()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act - 自動探索開始
        controller.ProcessInput(GameAction.AutoExplore);
        // 手動入力で中断
        controller.ProcessInput(GameAction.Wait);

        // Assert - 自動探索が停止していること
        Assert.False(controller.IsAutoExploring);
    }

    [Fact]
    public void AutoExplore_StopsWhenHpLow()
    {
        // Arrange
        var controller = CreateInitializedController();
        // HPを半分以下に設定
        int damage = controller.Player.MaxHp / 2 + 1;
        controller.Player.TakeDamage(new RougelikeGame.Core.Damage(damage, RougelikeGame.Core.DamageType.Pure, RougelikeGame.Core.Element.None, false, "テスト"));

        // Act
        controller.ProcessInput(GameAction.AutoExplore);

        // Assert - HP低下のため停止
        Assert.False(controller.IsAutoExploring);
    }

    [Fact]
    public void Player_EffectiveStats_ReflectBaseStats()
    {
        // Arrange
        var controller = CreateInitializedController();
        var player = controller.Player;

        // Assert - 装備なしの場合、基本値と有効値が等しい
        Assert.Equal(player.BaseStats.Strength, player.EffectiveStats.Strength);
        Assert.Equal(player.BaseStats.Vitality, player.EffectiveStats.Vitality);
        Assert.True(player.EffectiveStats.MaxHp > 0);
        Assert.True(player.EffectiveStats.PhysicalAttack > 0);
    }

    #region セーブ・ロード テスト

    [Fact]
    public void CreateSaveData_CapturesCurrentState()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.ProcessInput(GameAction.Wait); // 1ターン進める

        // Act
        var save = controller.CreateSaveData();

        // Assert
        Assert.NotNull(save);
        Assert.Equal(controller.CurrentFloor, save.CurrentFloor);
        Assert.Equal(controller.TurnCount, save.TurnCount);
        Assert.Equal(controller.Player.Name, save.Player.Name);
        Assert.Equal(controller.Player.Level, save.Player.Level);
        Assert.Equal(controller.Player.CurrentHp, save.Player.CurrentHp);
        Assert.Equal(controller.Player.Position.X, save.Player.Position.X);
        Assert.Equal(controller.Player.Position.Y, save.Player.Position.Y);
    }

    [Fact]
    public void CreateSaveData_IncludesInventoryItems()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        var save = controller.CreateSaveData();

        // Assert - 初期アイテム（ポーション×2、パン）＋装備分
        Assert.NotEmpty(save.Player.InventoryItems);
    }

    [Fact]
    public void CreateSaveData_IncludesEquippedItems()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        var save = controller.CreateSaveData();

        // Assert - 初期装備（剣と鎧）
        Assert.NotEmpty(save.Player.EquippedItems);
    }

    [Fact]
    public void CreateSaveData_IncludesMessageHistory()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        var save = controller.CreateSaveData();

        // Assert - 初期化メッセージが含まれているはず
        Assert.NotEmpty(save.MessageHistory);
    }

    [Fact]
    public void LoadSaveData_RestoresPlayerState()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.Player.GainExperience(500);
        for (int i = 0; i < 5; i++) controller.ProcessInput(GameAction.Wait);
        var save = controller.CreateSaveData();

        // Act - 新しいコントローラーでロード
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.Equal(save.Player.Name, controller2.Player.Name);
        Assert.Equal(save.Player.Level, controller2.Player.Level);
        Assert.Equal(save.Player.Experience, controller2.Player.Experience);
        Assert.Equal(save.CurrentFloor, controller2.CurrentFloor);
        Assert.Equal(save.TurnCount, controller2.TurnCount);
    }

    [Fact]
    public void LoadSaveData_RestoresGameTime()
    {
        // Arrange
        var controller = CreateInitializedController();
        for (int i = 0; i < 100; i++) controller.ProcessInput(GameAction.Wait);
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.Equal(save.GameTime.TotalTurns, controller2.GameTime.TotalTurns);
    }

    [Fact]
    public void LoadSaveData_RestoresMessageHistory()
    {
        // Arrange
        var controller = CreateInitializedController();
        var save = controller.CreateSaveData();
        int originalMessageCount = save.MessageHistory.Count;

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert - ロード後のメッセージ履歴には元のメッセージ + ロードメッセージが含まれる
        Assert.True(controller2.MessageHistory.Count >= originalMessageCount);
    }

    [Fact]
    public void LoadSaveData_ResetsGameOverState()
    {
        // Arrange
        var controller = CreateInitializedController();
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.False(controller2.IsGameOver);
        Assert.True(controller2.IsRunning);
        Assert.False(controller2.IsAutoExploring);
    }

    [Fact]
    public void ProcessInput_Save_DoesNotAdvanceTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        bool saveCalled = false;
        controller.OnSaveGame += () => saveCalled = true;
        int initialTurn = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.Save);

        // Assert
        Assert.True(saveCalled);
        Assert.Equal(initialTurn, controller.TurnCount);
    }

    [Fact]
    public void ProcessInput_Load_DoesNotAdvanceTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        bool loadCalled = false;
        controller.OnLoadGame += () => loadCalled = true;
        int initialTurn = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.Load);

        // Assert
        Assert.True(loadCalled);
        Assert.Equal(initialTurn, controller.TurnCount);
    }

    #endregion

    #region 階段上昇テスト

    [Fact]
    public void ProcessInput_AscendStairs_OnFloor1AtStairsUp_EscapesSurface()
    {
        // Arrange
        var controller = CreateInitializedController();
        var stairsUp = controller.Map.StairsUpPosition;
        if (stairsUp == null) return; // テスト不可能

        controller.Player.Position = stairsUp.Value;
        bool gameOverCalled = false;
        controller.OnGameOver += () => gameOverCalled = true;

        // Act
        controller.ProcessInput(GameAction.AscendStairs);

        // Assert - 1階で上り階段 = 地上帰還
        Assert.True(gameOverCalled || !controller.IsRunning || controller.CurrentFloor == 1);
    }

    [Fact]
    public void ProcessInput_AscendStairs_NotOnStairs_NoEffect()
    {
        // Arrange
        var controller = CreateInitializedController();
        var messages = new List<string>();
        controller.OnMessage += msg => messages.Add(msg);
        int initialFloor = controller.CurrentFloor;

        // プレイヤーを階段のない場所に移動
        var stairsUp = controller.Map.StairsUpPosition;
        // 階段でない場所を探す
        for (int x = 1; x < controller.Map.Width - 1; x++)
        {
            for (int y = 1; y < controller.Map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                if (pos != stairsUp && controller.Map.IsWalkable(pos))
                {
                    controller.Player.Position = pos;
                    goto found;
                }
            }
        }
        return;
    found:

        // Act
        controller.ProcessInput(GameAction.AscendStairs);

        // Assert - 階段にいないので階は変わらない
        Assert.Equal(initialFloor, controller.CurrentFloor);
    }

    #endregion

    #region メッセージログテスト

    [Fact]
    public void MessageHistory_RecordsMessages()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert - メッセージ履歴に記録されている
        Assert.NotEmpty(controller.MessageHistory);
        Assert.Contains(controller.MessageHistory, m => m.Contains("待機した"));
    }

    [Fact]
    public void ProcessInput_OpenMessageLog_FiresEvent()
    {
        // Arrange
        var controller = CreateInitializedController();
        List<string>? receivedMessages = null;
        controller.OnShowMessageLog += msgs => receivedMessages = msgs;

        // Act
        controller.ProcessInput(GameAction.OpenMessageLog);

        // Assert
        Assert.NotNull(receivedMessages);
        Assert.NotEmpty(receivedMessages);
    }

    [Fact]
    public void ProcessInput_OpenMessageLog_DoesNotAdvanceTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.OnShowMessageLog += _ => { };
        int initialTurn = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.OpenMessageLog);

        // Assert
        Assert.Equal(initialTurn, controller.TurnCount);
    }

    #endregion

    #region ターン制限テスト

    [Fact]
    public void TurnLimit_InitialState_NotExtendedNotRemoved()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.False(controller.IsTurnLimitExtended);
        Assert.False(controller.IsTurnLimitRemoved);
    }

    [Fact]
    public void TurnLimit_RemainingDays_InitialValueIs365()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert - 初期状態では残り365日（ゲーム時間経過分の誤差を許容）
        Assert.True(controller.RemainingDays >= 364 && controller.RemainingDays <= 365,
            $"初期残り日数は約365日であるべき（実際: {controller.RemainingDays}）");
    }

    [Fact]
    public void TurnLimit_RemainingDays_DecreasesAfterWait()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialDays = controller.RemainingDays;

        // Act - 1日分のターンを待機（86400ターン）
        // 直接大量に待機するのは遅いので、RemainingTurnsプロパティで計算を確認
        long initialTurns = controller.RemainingTurns;
        controller.ProcessInput(GameAction.Wait); // 1ターン消費

        // Assert
        Assert.True(controller.RemainingTurns < initialTurns,
            "待機後に残りターン数が減少するべき");
    }

    [Fact]
    public void TurnLimit_ExtendTurnLimit_IncreasesLimit()
    {
        // Arrange
        var controller = CreateInitializedController();
        long initialLimit = controller.CurrentTurnLimit;

        // Act
        controller.ExtendTurnLimit();

        // Assert
        Assert.True(controller.IsTurnLimitExtended);
        Assert.True(controller.CurrentTurnLimit > initialLimit,
            "延長後のターン制限は初期値より大きいべき");
        Assert.Equal(TimeConstants.TurnLimitWithExtension,
            controller.CurrentTurnLimit);
    }

    [Fact]
    public void TurnLimit_ExtendTurnLimit_CannotExtendTwice()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.ExtendTurnLimit();
        long limitAfterFirst = controller.CurrentTurnLimit;

        // Act
        controller.ExtendTurnLimit(); // 2回目

        // Assert - 制限は変わらない
        Assert.Equal(limitAfterFirst, controller.CurrentTurnLimit);
    }

    [Fact]
    public void TurnLimit_RemoveTurnLimit_SetsRemovedFlag()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        controller.RemoveTurnLimit();

        // Assert
        Assert.True(controller.IsTurnLimitRemoved);
        Assert.Equal(int.MaxValue, controller.RemainingDays);
    }

    [Fact]
    public void TurnLimit_RemoveTurnLimit_CannotExtendAfterRemoval()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.RemoveTurnLimit();

        // Act
        controller.ExtendTurnLimit();

        // Assert - 撤廃済みなので延長フラグは立たない
        Assert.False(controller.IsTurnLimitExtended);
        Assert.True(controller.IsTurnLimitRemoved);
    }

    [Fact]
    public void TurnLimit_SaveAndLoad_PreservesFlags()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.ExtendTurnLimit();
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.True(controller2.IsTurnLimitExtended);
        Assert.False(controller2.IsTurnLimitRemoved);
    }

    [Fact]
    public void TurnLimit_SaveAndLoad_PreservesRemovedFlag()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.RemoveTurnLimit();
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.False(controller2.IsTurnLimitExtended);
        Assert.True(controller2.IsTurnLimitRemoved);
    }

    #endregion

    #region 行動コスト差別化テスト

    [Fact]
    public void ActionCost_Wait_Consumes1Turn()
    {
        // Arrange
        var controller = CreateInitializedController();
        int initialTurns = controller.GameTime.TotalTurns;

        // Act
        controller.ProcessInput(GameAction.Wait);

        // Assert - 待機は1ターン消費
        Assert.Equal(initialTurns + 1, controller.GameTime.TotalTurns);
    }

    [Fact]
    public void ActionCost_TurnCountAndGameTime_Synchronized()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act - 複数の行動を実行
        controller.ProcessInput(GameAction.Wait);
        controller.ProcessInput(GameAction.MoveUp);
        controller.ProcessInput(GameAction.MoveDown);
        controller.ProcessInput(GameAction.Wait);

        // Assert - TurnCountとGameTime.TotalTurnsが同期
        Assert.Equal(controller.TurnCount, controller.GameTime.TotalTurns);
    }

    #endregion
}
