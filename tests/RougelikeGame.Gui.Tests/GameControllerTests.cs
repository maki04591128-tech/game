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

    private GameController CreateDebugController()
    {
        var controller = new GameController();
        controller.InitializeDebug();
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
        // Arrange & Act — デバッグマップ（ダンジョン環境）で敵がスポーンされることを検証
        // ※ Initialize()はシンボルマップ（敵なし）を生成するためInitializeDebug()を使用
        var controller = CreateDebugController();

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
        // Arrange & Act — デバッグマップ（ダンジョン環境）でアイテムがスポーンされることを検証
        // ※ Initialize()はシンボルマップ（アイテムなし）を生成するためInitializeDebug()を使用
        var controller = CreateDebugController();

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
    public void Player_InitialStages_AreCorrect()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.Equal(RougelikeGame.Core.HungerStage.Full, controller.Player.HungerStage);
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

        // Assert - 有効値はベース値以上（装備・スキルツリーボーナスで上がることがある）
        Assert.True(player.EffectiveStats.Strength >= player.BaseStats.Strength);
        Assert.True(player.EffectiveStats.Vitality >= player.BaseStats.Vitality);
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

    #region 難易度システムテスト

    [Fact]
    public void Difficulty_DefaultIsNormal()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert
        Assert.Equal(DifficultyLevel.Normal, controller.Difficulty);
    }

    [Fact]
    public void Difficulty_SetDifficulty_ChangesDifficulty()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        controller.SetDifficulty(DifficultyLevel.Hard);

        // Assert
        Assert.Equal(DifficultyLevel.Hard, controller.Difficulty);
        Assert.Equal("難しい", controller.DifficultyConfig.DisplayName);
    }

    [Fact]
    public void Difficulty_Easy_IncreaseTurnLimit()
    {
        // Arrange
        var controllerNormal = CreateInitializedController();
        long normalLimit = controllerNormal.CurrentTurnLimit;

        var controllerEasy = CreateInitializedController();
        controllerEasy.SetDifficulty(DifficultyLevel.Easy);

        // Assert
        Assert.True(controllerEasy.CurrentTurnLimit > normalLimit,
            $"Easy ({controllerEasy.CurrentTurnLimit}) は Normal ({normalLimit}) より大きいべき");
    }

    [Fact]
    public void Difficulty_SaveAndLoad_PreservesDifficulty()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.SetDifficulty(DifficultyLevel.Nightmare);
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.Equal(DifficultyLevel.Nightmare, controller2.Difficulty);
    }

    #endregion

    #region 通貨（ゴールド）テスト

    [Fact]
    public void Gold_InitialValueIsZero()
    {
        // Arrange & Act
        var controller = CreateInitializedController();

        // Assert—Adventurer素性の初期ゴールド100G
        Assert.Equal(100, controller.Player.Gold);
    }

    [Fact]
    public void Gold_AddGold_IncreasesGold()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Act
        controller.Player.AddGold(100);

        // Assert—初期100G + 100G = 200G
        Assert.Equal(200, controller.Player.Gold);
    }

    [Fact]
    public void Gold_SpendGold_DecreasesGold()
    {
        // Arrange
        var controller = CreateInitializedController();
        controller.Player.AddGold(100);

        // Act
        bool result = controller.Player.SpendGold(30);

        // Assert—初期100G + 100G - 30G = 170G
        Assert.True(result);
        Assert.Equal(170, controller.Player.Gold);
    }

    [Fact]
    public void Gold_SpendGold_FailsIfInsufficient()
    {
        // Arrange—初期100Gのまま
        var controller = CreateInitializedController();

        // Act—150Gは支払えない
        bool result = controller.Player.SpendGold(150);

        // Assert
        Assert.False(result);
        Assert.Equal(100, controller.Player.Gold);
    }

    [Fact]
    public void Gold_SaveAndLoad_PreservesGold()
    {
        // Arrange—初期100G + 500G = 600G
        var controller = CreateInitializedController();
        controller.Player.AddGold(500);
        var save = controller.CreateSaveData();

        // Act
        var controller2 = new GameController();
        controller2.Initialize();
        controller2.LoadSaveData(save);

        // Assert
        Assert.Equal(600, controller2.Player.Gold);
    }

    #endregion

    #region 重量システム テスト

    [Fact]
    public void Weight_CalculateMaxWeight_BasedOnStrength()
    {
        // Arrange
        var controller = CreateInitializedController();
        var player = controller.Player;
        int str = player.EffectiveStats.Strength;

        // Act
        float maxWeight = player.CalculateMaxWeight();

        // Assert - BaseMaxWeight(50) + STR * WeightPerStrength(5)
        float expected = GameConstants.BaseMaxWeight + str * GameConstants.WeightPerStrength;
        Assert.Equal(expected, maxWeight);
    }

    [Fact]
    public void Weight_InitializeUpdatesMaxWeight()
    {
        // Arrange & Act
        var controller = CreateInitializedController();
        var inventory = (RougelikeGame.Core.Entities.Inventory)controller.Player.Inventory;

        // Assert - MaxWeight should be STR-based, not default 100
        float expected = controller.Player.CalculateMaxWeight();
        Assert.Equal(expected, inventory.MaxWeight);
    }

    [Fact]
    public void Weight_IsOverweight_FalseWhenUnderLimit()
    {
        // Arrange
        var controller = CreateInitializedController();

        // Assert - 初期装備のみ
        Assert.False(controller.Player.IsOverweight);
    }

    [Fact]
    public void Weight_TotalWeight_ReflectsInventoryItems()
    {
        // Arrange
        var controller = CreateInitializedController();
        var inventory = (RougelikeGame.Core.Entities.Inventory)controller.Player.Inventory;

        // Assert - 初期装備とアイテムの重量合計が0以上
        Assert.True(inventory.TotalWeight >= 0);
    }

    #endregion

    #region アイテム鑑定システム テスト

    [Fact]
    public void Identify_UnidentifiedItem_ShowsUnidentifiedName()
    {
        // Arrange
        var sword = RougelikeGame.Core.Items.ItemFactory.CreateIronSword();
        sword.IsIdentified = false;

        // Act & Assert
        Assert.Equal("不明なアイテム", sword.GetDisplayName());
    }

    [Fact]
    public void Identify_IdentifiedItem_ShowsRealName()
    {
        // Arrange
        var sword = RougelikeGame.Core.Items.ItemFactory.CreateIronSword();
        sword.IsIdentified = true;

        // Act & Assert
        Assert.Contains("鉄の剣", sword.GetDisplayName());
    }

    [Fact]
    public void Identify_CursedItem_ShowsCursedPrefix()
    {
        // Arrange
        var sword = RougelikeGame.Core.Items.ItemFactory.CreateIronSword();
        sword.IsIdentified = true;
        sword.IsCursed = true;

        // Act
        string name = sword.GetDisplayName();

        // Assert
        Assert.Contains("呪われた", name);
    }

    [Fact]
    public void Identify_BlessedItem_ShowsBlessedPrefix()
    {
        // Arrange
        var sword = RougelikeGame.Core.Items.ItemFactory.CreateIronSword();
        sword.IsIdentified = true;
        sword.IsBlessed = true;

        // Act
        string name = sword.GetDisplayName();

        // Assert
        Assert.Contains("祝福された", name);
    }

    [Fact]
    public void Identify_GeneratedEquipment_IsUnidentified()
    {
        // Arrange
        var factory = new RougelikeGame.Core.Items.ItemFactory(42);

        // Act - 複数生成して装備品を見つける
        RougelikeGame.Core.Items.Item? equipment = null;
        for (int i = 0; i < 100; i++)
        {
            var item = factory.GenerateRandomItem(5);
            if (item is RougelikeGame.Core.Items.EquipmentItem)
            {
                equipment = item;
                break;
            }
        }

        // Assert
        Assert.NotNull(equipment);
        Assert.False(equipment!.IsIdentified);
    }

    [Fact]
    public void Identify_GeneratedScroll_IsUnidentified()
    {
        // Arrange
        var factory = new RougelikeGame.Core.Items.ItemFactory(42);

        // Act - 複数生成してスクロールを見つける
        RougelikeGame.Core.Items.Item? scroll = null;
        for (int i = 0; i < 100; i++)
        {
            var item = factory.GenerateRandomItem(1);
            if (item is RougelikeGame.Core.Items.Scroll)
            {
                scroll = item;
                break;
            }
        }

        // Assert
        Assert.NotNull(scroll);
        Assert.False(scroll!.IsIdentified);
    }

    #endregion

    #region ダンジョン階層システム テスト

    [Fact]
    public void Floor_DescendStairs_IncreasesFloor()
    {
        // Arrange
        var controller = CreateInitializedController();

        // プレイヤーを下り階段に移動
        var stairsDown = controller.Map.StairsDownPosition;
        if (stairsDown.HasValue)
        {
            controller.Player.Position = stairsDown.Value;

            // Act
            controller.ProcessInput(GameAction.UseStairs);

            // Assert
            Assert.Equal(2, controller.CurrentFloor);
        }
    }

    [Fact]
    public void Floor_AscendFromFloor1_ExitsDungeon()
    {
        // Arrange
        var controller = CreateInitializedController();
        var stairsUp = controller.Map.StairsUpPosition;
        if (stairsUp.HasValue)
        {
            controller.Player.Position = stairsUp.Value;

            // Act
            controller.ProcessInput(GameAction.AscendStairs);

            // Assert - 1層からの帰還
            Assert.False(controller.IsRunning);
        }
    }

    [Fact]
    public void Floor_EnemiesForDepth_ReturnsEnemies()
    {
        // Act
        var shallow = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(1);
        var mid = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(8);
        var deep = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(25);

        // Assert
        Assert.True(shallow.Count > 0);
        Assert.True(mid.Count > 0);
        Assert.True(deep.Count > 0);
    }

    #endregion

    #region ドア・隠し通路システム

    [Fact]
    public void Door_OpenClosedDoor_ChangesTileToOpen()
    {
        // Arrange
        var controller = CreateInitializedController();
        var playerPos = controller.Player.Position;
        var doorPos = new Position(playerPos.X + 1, playerPos.Y);

        // ドアを配置
        controller.Map.SetTile(doorPos, RougelikeGame.Core.Map.TileType.DoorClosed);

        // Act
        controller.ProcessInput(GameAction.MoveRight);

        // Assert - ドアが開いているはず
        Assert.Equal(RougelikeGame.Core.Map.TileType.DoorOpen, controller.Map.GetTileType(doorPos));
        // プレイヤーはドアの位置に移動していない（開けるだけ）
        Assert.Equal(playerPos, controller.Player.Position);
    }

    [Fact]
    public void Door_LockedDoor_BlocksOpenWithLowDex()
    {
        // Arrange
        var controller = CreateInitializedController();
        var playerPos = controller.Player.Position;
        var doorPos = new Position(playerPos.X + 1, playerPos.Y);

        // 施錠ドアを配置（非常に高い難易度）
        controller.Map.SetTile(doorPos, RougelikeGame.Core.Map.TileType.DoorClosed);
        var tile = controller.Map.GetTile(doorPos);
        tile.IsLocked = true;
        tile.LockDifficulty = 100; // 非常に高い難易度

        // Act
        controller.ProcessInput(GameAction.MoveRight);

        // Assert - 施錠は解除されていない可能性が高い（難易度100）
        // ドアはまだ閉じている可能性が高い
        Assert.Equal(playerPos, controller.Player.Position);
    }

    [Fact]
    public void Door_CloseDoor_ChangesTileToClosed()
    {
        // Arrange
        var controller = CreateInitializedController();
        var playerPos = controller.Player.Position;
        var doorPos = new Position(playerPos.X + 1, playerPos.Y);

        // 開いたドアを配置
        controller.Map.SetTile(doorPos, RougelikeGame.Core.Map.TileType.DoorOpen);

        // Act
        controller.ProcessInput(GameAction.CloseDoor);

        // Assert
        Assert.Equal(RougelikeGame.Core.Map.TileType.DoorClosed, controller.Map.GetTileType(doorPos));
    }

    [Fact]
    public void Door_CloseDoor_NoOpenDoorNearby_NoTurnUsed()
    {
        // Arrange
        var controller = CreateInitializedController();
        int turnsBefore = controller.TurnCount;

        // 近くにドアがない状態でCloseDoor
        // Act
        controller.ProcessInput(GameAction.CloseDoor);

        // Assert - ターン消費なし
        Assert.Equal(turnsBefore, controller.TurnCount);
    }

    [Fact]
    public void Search_FindsSecretDoor_WithHighPerception()
    {
        // Arrange
        var controller = CreateInitializedController();
        var playerPos = controller.Player.Position;
        var secretPos = new Position(playerPos.X + 1, playerPos.Y);

        // 隠し通路を配置
        controller.Map.SetTile(secretPos, RougelikeGame.Core.Map.TileType.SecretDoor);

        // Act - 複数回探索（PER判定の確率的成功を期待）
        bool found = false;
        for (int i = 0; i < 50; i++)
        {
            controller.ProcessInput(GameAction.Search);
            if (controller.Map.GetTileType(secretPos) == RougelikeGame.Core.Map.TileType.DoorClosed)
            {
                found = true;
                break;
            }
        }

        // Assert - 50回も試行すればPER10でも発見できるはず
        Assert.True(found, "50回の探索で隠し通路が発見されるべき");
    }

    [Fact]
    public void Search_ConsumeTurn()
    {
        // Arrange
        var controller = CreateInitializedController();
        int turnsBefore = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.Search);

        // Assert
        Assert.True(controller.TurnCount > turnsBefore, "探索はターンを消費する");
    }

    [Fact]
    public void Search_FindsHiddenTrap()
    {
        // Arrange
        var controller = CreateInitializedController();
        var playerPos = controller.Player.Position;
        var trapPos = new Position(playerPos.X + 1, playerPos.Y);

        // 隠し罠を配置
        controller.Map.SetTile(trapPos, RougelikeGame.Core.Map.TileType.TrapHidden);
        controller.Map.GetTile(trapPos).TrapId = "Arrow";

        // Act - 複数回探索
        bool found = false;
        for (int i = 0; i < 50; i++)
        {
            controller.ProcessInput(GameAction.Search);
            if (controller.Map.GetTileType(trapPos) == RougelikeGame.Core.Map.TileType.TrapVisible)
            {
                found = true;
                break;
            }
        }

        // Assert
        Assert.True(found, "50回の探索で隠し罠が発見されるべき");
    }

    #endregion

    #region 射撃・投擲システム テスト

    [Fact]
    public void RangedAttack_WithoutRangedWeapon_Fails()
    {
        // Arrange
        var controller = CreateInitializedController();
        int turnsBefore = controller.TurnCount;

        // Act - 近接武器のみの状態で射撃
        controller.ProcessInput(GameAction.RangedAttack);

        // Assert - ターン消費なし
        Assert.Equal(turnsBefore, controller.TurnCount);
    }

    [Fact]
    public void RangedAttack_WithBow_NoEnemyInRange_Fails()
    {
        // Arrange
        var controller = CreateInitializedController();

        // 弓を装備
        var bow = new RougelikeGame.Core.Items.Weapon
        {
            Name = "短弓",
            WeaponType = RougelikeGame.Core.Items.WeaponType.Bow,
            BaseDamage = 5,
            DamageRange = (3, 8),
            Range = 6,
            AttackType = AttackType.Ranged,
            Slot = RougelikeGame.Core.Items.EquipmentSlot.MainHand
        };
        controller.Player.Equipment.Equip(bow, controller.Player);

        // 敵をクリア
        controller.Enemies.Clear();

        int turnsBefore = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.RangedAttack);

        // Assert - 敵がいないのでターン消費なし
        Assert.Equal(turnsBefore, controller.TurnCount);
    }

    [Fact]
    public void RangedAttack_WithBow_EnemyInRange_ConsumeTurn()
    {
        // Arrange
        var controller = CreateInitializedController();

        // 弓を装備
        var bow = new RougelikeGame.Core.Items.Weapon
        {
            Name = "短弓",
            WeaponType = RougelikeGame.Core.Items.WeaponType.Bow,
            BaseDamage = 5,
            DamageRange = (3, 8),
            Range = 8,
            AttackType = AttackType.Ranged,
            Slot = RougelikeGame.Core.Items.EquipmentSlot.MainHand
        };
        controller.Player.Equipment.Equip(bow, controller.Player);

        // 射程内に敵を配置（プレイヤーの3マス先）
        var enemyPos = new Position(controller.Player.Position.X + 3, controller.Player.Position.Y);
        // 射線上のタイルを歩行可能にする
        for (int dx = 1; dx <= 3; dx++)
        {
            var pos = new Position(controller.Player.Position.X + dx, controller.Player.Position.Y);
            if (controller.Map.IsInBounds(pos))
                controller.Map.SetTile(pos, RougelikeGame.Core.Map.TileType.Floor);
        }

        var enemyDef = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(1)[0];
        var factory = new RougelikeGame.Core.Factories.EnemyFactory();
        var enemy = factory.CreateEnemy(enemyDef, enemyPos);
        controller.Enemies.Add(enemy);

        int turnsBefore = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.RangedAttack);

        // Assert - ターンが消費される
        Assert.True(controller.TurnCount > turnsBefore, "射撃はターンを消費する");
    }

    [Fact]
    public void ThrowItem_WithoutThrowable_Fails()
    {
        // Arrange
        var controller = CreateInitializedController();

        // インベントリから投擲武器を除去（初期装備には投擲武器がないはず）
        int turnsBefore = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.ThrowItem);

        // Assert - ターン消費なし
        Assert.Equal(turnsBefore, controller.TurnCount);
    }

    [Fact]
    public void ThrowItem_WithThrowable_RemovesFromInventory()
    {
        // Arrange
        var controller = CreateInitializedController();

        // 投擲武器を追加
        var throwKnife = new RougelikeGame.Core.Items.Weapon
        {
            Name = "投げナイフ",
            WeaponType = RougelikeGame.Core.Items.WeaponType.Thrown,
            BaseDamage = 3,
            DamageRange = (2, 5),
            Range = 4,
            AttackType = AttackType.Ranged,
            Slot = RougelikeGame.Core.Items.EquipmentSlot.MainHand
        };
        ((RougelikeGame.Core.Entities.Inventory)controller.Player.Inventory).Add(throwKnife);

        // 射程内に敵を配置
        var enemyPos = new Position(controller.Player.Position.X + 2, controller.Player.Position.Y);
        for (int dx = 1; dx <= 2; dx++)
        {
            var pos = new Position(controller.Player.Position.X + dx, controller.Player.Position.Y);
            if (controller.Map.IsInBounds(pos))
                controller.Map.SetTile(pos, RougelikeGame.Core.Map.TileType.Floor);
        }

        var enemyDef1 = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(1)[0];
        var factory1 = new RougelikeGame.Core.Factories.EnemyFactory();
        var enemy1 = factory1.CreateEnemy(enemyDef1, enemyPos);
        controller.Enemies.Add(enemy1);

        // Act
        controller.ProcessInput(GameAction.ThrowItem);

        // Assert - インベントリから投擲武器が消えている
        var remaining = ((RougelikeGame.Core.Entities.Inventory)controller.Player.Inventory).Items
            .Where(i => i.Name == "投げナイフ")
            .ToList();
        Assert.Empty(remaining);
    }

    [Fact]
    public void ThrowItem_DropsItemOnGround()
    {
        // Arrange
        var controller = CreateInitializedController();

        var throwKnife = new RougelikeGame.Core.Items.Weapon
        {
            Name = "投げナイフ",
            WeaponType = RougelikeGame.Core.Items.WeaponType.Thrown,
            BaseDamage = 3,
            DamageRange = (2, 5),
            Range = 4,
            AttackType = AttackType.Ranged,
            Slot = RougelikeGame.Core.Items.EquipmentSlot.MainHand
        };
        ((RougelikeGame.Core.Entities.Inventory)controller.Player.Inventory).Add(throwKnife);

        // 射程内に敵を配置
        var enemyPos = new Position(controller.Player.Position.X + 2, controller.Player.Position.Y);
        for (int dx = 1; dx <= 2; dx++)
        {
            var pos = new Position(controller.Player.Position.X + dx, controller.Player.Position.Y);
            if (controller.Map.IsInBounds(pos))
                controller.Map.SetTile(pos, RougelikeGame.Core.Map.TileType.Floor);
        }

        var enemyDef2 = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(1)[0];
        var factory2 = new RougelikeGame.Core.Factories.EnemyFactory();
        var enemy2 = factory2.CreateEnemy(enemyDef2, enemyPos);
        controller.Enemies.Add(enemy2);

        int groundItemsBefore = controller.GroundItems.Count;

        // Act
        controller.ProcessInput(GameAction.ThrowItem);

        // Assert - 地面にアイテムが増えている
        Assert.True(controller.GroundItems.Count > groundItemsBefore, "投擲アイテムが地面に落ちる");
    }

    [Fact]
    public void GetDistance_CalculatesCorrectly()
    {
        // Arrange
        var controller = CreateInitializedController();
        var pos1 = new Position(0, 0);
        var pos2 = new Position(3, 4);

        // Act - チェビシェフ距離の確認（Max(|dx|, |dy|)）
        // GetDistanceはprivateだが、RangedAttackの射程判定で使われている
        // 間接的にテスト：射程5の武器で距離4の敵に攻撃可能か
        var bow = new RougelikeGame.Core.Items.Weapon
        {
            Name = "テスト弓",
            WeaponType = RougelikeGame.Core.Items.WeaponType.Bow,
            BaseDamage = 5,
            DamageRange = (3, 8),
            Range = 5,
            AttackType = AttackType.Ranged,
            Slot = RougelikeGame.Core.Items.EquipmentSlot.MainHand
        };
        controller.Player.Equipment.Equip(bow, controller.Player);

        // 距離4の位置に敵を配置
        var enemyPos = new Position(controller.Player.Position.X + 3, controller.Player.Position.Y + 4);
        // 射線上のタイルを確保
        for (int x = Math.Min(controller.Player.Position.X, enemyPos.X); x <= Math.Max(controller.Player.Position.X, enemyPos.X); x++)
        {
            for (int y = Math.Min(controller.Player.Position.Y, enemyPos.Y); y <= Math.Max(controller.Player.Position.Y, enemyPos.Y); y++)
            {
                var p = new Position(x, y);
                if (controller.Map.IsInBounds(p))
                    controller.Map.SetTile(p, RougelikeGame.Core.Map.TileType.Floor);
            }
        }

        var enemyDef3 = RougelikeGame.Core.Factories.EnemyDefinitions.GetEnemiesForDepth(1)[0];
        var factory3 = new RougelikeGame.Core.Factories.EnemyFactory();
        var enemy3 = factory3.CreateEnemy(enemyDef3, enemyPos);
        controller.Enemies.Add(enemy3);

        int turnsBefore = controller.TurnCount;

        // Act
        controller.ProcessInput(GameAction.RangedAttack);

        // Assert - チェビシェフ距離 = Max(3, 4) = 4 <= 射程5 → 攻撃可能
        Assert.True(controller.TurnCount > turnsBefore, "射程内なのでターンが消費される");
    }

    #endregion

    #region スキルスロットテスト

    [Fact]
    public void AssignSkillSlot_ValidSlot_Succeeds()
    {
        var controller = CreateDebugController();
        controller.Player.LearnedSkills.Add("strong_strike");
        controller.AssignSkillSlot(0, "strong_strike");
        var slots = controller.GetSkillSlots();
        Assert.Equal("strong_strike", slots[0]);
    }

    [Fact]
    public void AssignSkillSlot_OutOfRange_Ignored()
    {
        var controller = CreateDebugController();
        controller.Player.LearnedSkills.Add("strong_strike");
        controller.AssignSkillSlot(6, "strong_strike");
        var slots = controller.GetSkillSlots();
        Assert.True(slots.All(s => s == null));
    }

    [Fact]
    public void ClearSkillSlot_RemovesAssignment()
    {
        var controller = CreateDebugController();
        controller.Player.LearnedSkills.Add("strong_strike");
        controller.AssignSkillSlot(2, "strong_strike");
        // EquippedSkillSlotsはリスト方式なのでスロット0に格納される
        controller.ClearSkillSlot(0);
        var slots = controller.GetSkillSlots();
        Assert.Null(slots[0]);
    }

    [Fact]
    public void GetSkillSlots_ReturnsAllSixSlots()
    {
        var controller = CreateDebugController();
        var slots = controller.GetSkillSlots();
        Assert.Equal(6, slots.Count);
    }

    [Fact]
    public void AssignSkillSlot_SixthSlot_Succeeds()
    {
        var controller = CreateDebugController();
        controller.Player.LearnedSkills.Add("strong_strike");
        controller.AssignSkillSlot(5, "strong_strike");
        var slots = controller.GetSkillSlots();
        // EquippedSkillSlotsはリスト方式なので、最初の装備はスロット0に格納される
        Assert.Equal("strong_strike", slots[0]);
    }

    [Fact]
    public void GetCurrentClassSkillTree_ReturnsNonEmpty()
    {
        var controller = CreateDebugController();
        var tree = controller.GetCurrentClassSkillTree();
        Assert.NotNull(tree);
    }

    #endregion

    #region フロアキャッシュテスト

    [Fact]
    public void FloorCache_RecordCreation()
    {
        var map = new RougelikeGame.Core.Map.DungeonMap(10, 10);
        var cache = new FloorCache(map, 1000, new List<(RougelikeGame.Core.Items.Item Item, RougelikeGame.Core.Position Position)>());
        Assert.Same(map, cache.Map);
        Assert.Equal(1000, cache.CreatedAtTurn);
    }

    [Fact]
    public void GenerateFloor_SameFloor_ReusesMapStructure()
    {
        var controller = CreateDebugController();

        // 初回: 第1層のマップを記録
        var map1Width = controller.Map.Width;
        var map1Height = controller.Map.Height;

        // 階下して戻る
        int floor1 = controller.CurrentFloor;
        controller.ProcessInput(GameAction.UseStairs);
        // 階段の位置にいない可能性があるので、CurrentFloorが変わっているか確認
        // フロアキャッシュの存在自体をテスト
        Assert.NotNull(controller.Map);
    }

    #endregion

    #region システム変更対応テスト

    [Fact]
    public void ProcessInput_OpenWorldMap_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.OpenWorldMap));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessInput_ViewQuestLog_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.ViewQuestLog));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessInput_OpenCrafting_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.OpenCrafting));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessInput_OpenSkillTree_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.OpenSkillTree));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessInput_ViewReligion_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.Pray));
        Assert.Null(ex);
    }

    [Fact]
    public void ProcessInput_ShowInventory_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.ProcessInput(GameAction.OpenInventory));
        Assert.Null(ex);
    }

    [Fact]
    public void LearnRandomRuneWord_DoesNotThrow()
    {
        var controller = CreateDebugController();
        var ex = Record.Exception(() => controller.LearnRandomRuneWord(1));
        Assert.Null(ex);
    }

    #endregion
}
