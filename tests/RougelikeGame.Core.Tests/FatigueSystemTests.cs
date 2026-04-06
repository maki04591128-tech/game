using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 疲労度システムテスト（タスク59）
/// - 疲労度段階判定テスト（8段階の閾値境界テスト）
/// - 移動時蓄積テスト
/// - スキル使用時蓄積テスト
/// - 待機時回復テスト
/// - SP上限修正テスト
/// - 行動コスト加算テスト
/// - 行動制限テスト
/// - 宿屋回復テスト
/// - 疲労度上限テスト
/// </summary>
public class FatigueSystemTests
{
    #region 段階判定テスト

    [Theory]
    [InlineData(0.0, FatigueStage.Refreshed)]
    [InlineData(5.0, FatigueStage.Refreshed)]
    [InlineData(9.9, FatigueStage.Refreshed)]
    [InlineData(10.0, FatigueStage.Normal)]
    [InlineData(30.0, FatigueStage.Normal)]
    [InlineData(49.9, FatigueStage.Normal)]
    [InlineData(50.0, FatigueStage.Lethargy)]
    [InlineData(59.9, FatigueStage.Lethargy)]
    [InlineData(60.0, FatigueStage.LightFatigue)]
    [InlineData(69.9, FatigueStage.LightFatigue)]
    [InlineData(70.0, FatigueStage.Fatigue)]
    [InlineData(79.9, FatigueStage.Fatigue)]
    [InlineData(80.0, FatigueStage.HeavyFatigue)]
    [InlineData(89.9, FatigueStage.HeavyFatigue)]
    [InlineData(90.0, FatigueStage.Exhaustion)]
    [InlineData(99.9, FatigueStage.Exhaustion)]
    [InlineData(100.0, FatigueStage.TotalExhaustion)]
    public void GetFatigueStage_BoundaryValues_ReturnsCorrectStage(double fatigue, FatigueStage expected)
    {
        var result = FatigueSystem.GetFatigueStage(fatigue);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFatigueStage_AllStages_AreRepresented()
    {
        // 全8段階が適切に割り当てられていることを確認
        Assert.Equal(FatigueStage.Refreshed, FatigueSystem.GetFatigueStage(0));
        Assert.Equal(FatigueStage.Normal, FatigueSystem.GetFatigueStage(10));
        Assert.Equal(FatigueStage.Lethargy, FatigueSystem.GetFatigueStage(50));
        Assert.Equal(FatigueStage.LightFatigue, FatigueSystem.GetFatigueStage(60));
        Assert.Equal(FatigueStage.Fatigue, FatigueSystem.GetFatigueStage(70));
        Assert.Equal(FatigueStage.HeavyFatigue, FatigueSystem.GetFatigueStage(80));
        Assert.Equal(FatigueStage.Exhaustion, FatigueSystem.GetFatigueStage(90));
        Assert.Equal(FatigueStage.TotalExhaustion, FatigueSystem.GetFatigueStage(100));
    }

    #endregion

    #region 蓄積テスト

    [Fact]
    public void AccumulateFatigue_Movement_AddsOneHundredthOfCost()
    {
        var player = Player.Create("Test", Stats.Default);
        Assert.Equal(0.0, player.Fatigue);

        // 移動コスト1 → 疲労度+0.01
        FatigueSystem.AccumulateFatigue(player, 1.0, isSkill: false);
        Assert.Equal(0.01, player.Fatigue, 6);

        // 移動コスト300（シンボルマップ移動） → 疲労度+3.0
        FatigueSystem.AccumulateFatigue(player, 300.0, isSkill: false);
        Assert.Equal(3.01, player.Fatigue, 6);
    }

    [Fact]
    public void AccumulateFatigue_Skill_AddsTenthOfCost()
    {
        var player = Player.Create("Test", Stats.Default);

        // スキルコスト5 → 疲労度+0.5
        FatigueSystem.AccumulateFatigue(player, 5.0, isSkill: true);
        Assert.Equal(0.5, player.Fatigue, 6);

        // スキルコスト100 → 疲労度+10.0
        FatigueSystem.AccumulateFatigue(player, 100.0, isSkill: true);
        Assert.Equal(10.5, player.Fatigue, 6);
    }

    [Fact]
    public void AccumulateFatigue_DoesNotExceedMax()
    {
        var player = Player.Create("Test", Stats.Default);
        // 大量に蓄積しても100を超えない
        FatigueSystem.AccumulateFatigue(player, 10000.0, isSkill: true);
        Assert.Equal(FatigueConstants.MaxFatigue, player.Fatigue);
    }

    #endregion

    #region 回復テスト

    [Fact]
    public void RecoverFatigue_RefreshedStage_RecoversByBaseAmount()
    {
        var player = Player.Create("Test", Stats.Default);
        player.ModifyFatigue(5.0); // 快調状態
        double before = player.Fatigue;

        FatigueSystem.RecoverFatigue(player);
        Assert.True(player.Fatigue < before);
        Assert.Equal(before - FatigueConstants.BaseRestRecovery, player.Fatigue, 6);
    }

    [Fact]
    public void RecoverFatigue_LethargyStage_RecoversByReducedAmount()
    {
        var player = Player.Create("Test", Stats.Default);
        player.ModifyFatigue(55.0); // 倦怠状態
        double before = player.Fatigue;

        FatigueSystem.RecoverFatigue(player);
        double expected = before - (FatigueConstants.BaseRestRecovery * FatigueConstants.LethargyRecoveryMultiplier);
        Assert.Equal(expected, player.Fatigue, 6);
    }

    [Fact]
    public void RecoverFatigue_LightFatigueStage_RecoversByFurtherReducedAmount()
    {
        var player = Player.Create("Test", Stats.Default);
        player.ModifyFatigue(65.0); // 疲労・軽状態
        double before = player.Fatigue;

        FatigueSystem.RecoverFatigue(player);
        double expected = before - (FatigueConstants.BaseRestRecovery * FatigueConstants.LightFatigueRecoveryMultiplier);
        Assert.Equal(expected, player.Fatigue, 6);
    }

    [Fact]
    public void RecoverFatigue_SevereFatigue_RecoversByMinimalAmount()
    {
        var player = Player.Create("Test", Stats.Default);
        player.ModifyFatigue(75.0); // 疲労状態
        double before = player.Fatigue;

        FatigueSystem.RecoverFatigue(player);
        double expected = before - (FatigueConstants.BaseRestRecovery * FatigueConstants.SevereRecoveryMultiplier);
        Assert.Equal(expected, player.Fatigue, 6);
    }

    [Fact]
    public void RecoverFatigue_DoesNotGoBelowZero()
    {
        var player = Player.Create("Test", Stats.Default);
        player.ModifyFatigue(0.5); // 快調状態、少量の疲労
        // 何回回復しても0未満にならない
        for (int i = 0; i < 10; i++)
        {
            FatigueSystem.RecoverFatigue(player);
        }
        Assert.True(player.Fatigue >= 0);
    }

    #endregion

    #region SP上限修正テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, 0.01)]
    [InlineData(FatigueStage.Normal, 0.0)]
    [InlineData(FatigueStage.Lethargy, -0.01)]
    [InlineData(FatigueStage.LightFatigue, -0.05)]
    [InlineData(FatigueStage.Fatigue, -0.25)]
    [InlineData(FatigueStage.HeavyFatigue, -0.50)]
    [InlineData(FatigueStage.Exhaustion, -0.80)]
    [InlineData(FatigueStage.TotalExhaustion, -1.00)]
    public void GetSpModifier_AllStages_ReturnsCorrectValue(FatigueStage stage, double expected)
    {
        Assert.Equal(expected, FatigueSystem.GetSpModifier(stage));
    }

    [Fact]
    public void Player_GetFatigueSpModifier_MatchesFatigueSystem()
    {
        var player = Player.Create("Test", Stats.Default);
        Assert.Equal(FatigueSystem.GetSpModifier(FatigueStage.Refreshed), player.GetFatigueSpModifier());

        player.ModifyFatigue(55.0);
        Assert.Equal(FatigueSystem.GetSpModifier(FatigueStage.Lethargy), player.GetFatigueSpModifier());
    }

    [Fact]
    public void Player_EffectiveMaxSp_AppliesFatigueModifier()
    {
        var player = Player.Create("Test", Stats.Default);
        int baseMaxSp = player.MaxSp;

        // 快調: +1%
        Assert.Equal((int)(baseMaxSp * 1.01), player.EffectiveMaxSp);

        // 疲労困憊: -100% → 0
        player.ModifyFatigue(100.0);
        Assert.Equal(0, player.EffectiveMaxSp);
    }

    #endregion

    #region 行動コスト加算テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, 0)]
    [InlineData(FatigueStage.Normal, 0)]
    [InlineData(FatigueStage.Lethargy, 0)]
    [InlineData(FatigueStage.LightFatigue, 1)]
    [InlineData(FatigueStage.Fatigue, 2)]
    [InlineData(FatigueStage.HeavyFatigue, 3)]
    [InlineData(FatigueStage.Exhaustion, 4)]
    [InlineData(FatigueStage.TotalExhaustion, 5)]
    public void GetActionCostBonus_AllStages_ReturnsCorrectValue(FatigueStage stage, int expected)
    {
        Assert.Equal(expected, FatigueSystem.GetActionCostBonus(stage));
    }

    #endregion

    #region 行動制限テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, TurnActionType.Move, true)]
    [InlineData(FatigueStage.Refreshed, TurnActionType.Attack, true)]
    [InlineData(FatigueStage.Refreshed, TurnActionType.UseSkill, true)]
    [InlineData(FatigueStage.Normal, TurnActionType.Move, true)]
    [InlineData(FatigueStage.Fatigue, TurnActionType.Move, true)]
    [InlineData(FatigueStage.Fatigue, TurnActionType.Attack, true)]
    public void CanPerformAction_NoRestrictionStages_AllActionsAllowed(
        FatigueStage stage, TurnActionType actionType, bool expected)
    {
        Assert.Equal(expected, FatigueSystem.CanPerformAction(stage, actionType));
    }

    [Theory]
    [InlineData(TurnActionType.Attack, false)]
    [InlineData(TurnActionType.UseSkill, false)]
    [InlineData(TurnActionType.CastSpell, false)]
    [InlineData(TurnActionType.Move, true)]
    [InlineData(TurnActionType.Wait, true)]
    [InlineData(TurnActionType.UseItem, true)]
    public void CanPerformAction_HeavyFatigue_AttackAndSkillDisabled(
        TurnActionType actionType, bool expected)
    {
        Assert.Equal(expected, FatigueSystem.CanPerformAction(FatigueStage.HeavyFatigue, actionType));
    }

    [Theory]
    [InlineData(TurnActionType.Move, false)]
    [InlineData(TurnActionType.Attack, false)]
    [InlineData(TurnActionType.UseSkill, false)]
    [InlineData(TurnActionType.CastSpell, false)]
    [InlineData(TurnActionType.Wait, true)]
    [InlineData(TurnActionType.UseItem, true)]
    [InlineData(TurnActionType.Rest, true)]
    public void CanPerformAction_Exhaustion_MovementAttackSkillDisabled(
        TurnActionType actionType, bool expected)
    {
        Assert.Equal(expected, FatigueSystem.CanPerformAction(FatigueStage.Exhaustion, actionType));
    }

    [Theory]
    [InlineData(TurnActionType.Move, false)]
    [InlineData(TurnActionType.Attack, false)]
    [InlineData(TurnActionType.UseSkill, false)]
    [InlineData(TurnActionType.Wait, true)]
    [InlineData(TurnActionType.Rest, true)]
    [InlineData(TurnActionType.UseItem, true)]
    [InlineData(TurnActionType.Interact, true)]
    [InlineData(TurnActionType.Search, false)]
    public void CanPerformAction_TotalExhaustion_OnlyWaitRestItemInteract(
        TurnActionType actionType, bool expected)
    {
        Assert.Equal(expected, FatigueSystem.CanPerformAction(FatigueStage.TotalExhaustion, actionType));
    }

    [Fact]
    public void CanPerformAction_WithRestrictionRelief_AllActionsAllowed()
    {
        // 気付け薬で制限解除中は全ての行動が可能
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.TotalExhaustion, TurnActionType.Move, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.Exhaustion, TurnActionType.Attack, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.HeavyFatigue, TurnActionType.UseSkill, hasFatigueRestrictionRelief: true));
    }

    #endregion

    #region 宿屋回復テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, 0.0)]
    [InlineData(FatigueStage.Normal, 0.0)]
    [InlineData(FatigueStage.Lethargy, 0.0)]
    [InlineData(FatigueStage.LightFatigue, 0.0)]
    [InlineData(FatigueStage.Fatigue, 10.0)]
    [InlineData(FatigueStage.HeavyFatigue, 20.0)]
    [InlineData(FatigueStage.Exhaustion, 30.0)]
    [InlineData(FatigueStage.TotalExhaustion, 40.0)]
    public void GetInnRecoveryStart_AllStages_ReturnsCorrectStartValue(
        FatigueStage stage, double expected)
    {
        Assert.Equal(expected, FatigueSystem.GetInnRecoveryStart(stage));
    }

    #endregion

    #region 段階名テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, "快調")]
    [InlineData(FatigueStage.Normal, "通常")]
    [InlineData(FatigueStage.Lethargy, "倦怠")]
    [InlineData(FatigueStage.LightFatigue, "疲労（軽）")]
    [InlineData(FatigueStage.Fatigue, "疲労")]
    [InlineData(FatigueStage.HeavyFatigue, "疲労（重）")]
    [InlineData(FatigueStage.Exhaustion, "疲弊")]
    [InlineData(FatigueStage.TotalExhaustion, "疲労困憊")]
    public void GetFatigueStageName_AllStages_ReturnsJapanese(
        FatigueStage stage, string expected)
    {
        Assert.Equal(expected, FatigueSystem.GetFatigueStageName(stage));
    }

    #endregion

    #region Player統合テスト

    [Fact]
    public void Player_InitialFatigue_IsZero()
    {
        var player = Player.Create("Test", Stats.Default);
        Assert.Equal(0.0, player.Fatigue);
        Assert.Equal(FatigueStage.Refreshed, player.FatigueStage);
    }

    [Fact]
    public void Player_ModifyFatigue_ClampsBetweenMinAndMax()
    {
        var player = Player.Create("Test", Stats.Default);

        player.ModifyFatigue(200.0);
        Assert.Equal(FatigueConstants.MaxFatigue, player.Fatigue);

        player.ModifyFatigue(-300.0);
        Assert.Equal(FatigueConstants.MinFatigue, player.Fatigue);
    }

    [Fact]
    public void Player_FatigueStageChanged_EventFires()
    {
        var player = Player.Create("Test", Stats.Default);
        FatigueStage? newStage = null;
        FatigueStage? oldStage = null;

        player.OnFatigueStageChanged += (_, e) =>
        {
            oldStage = e.OldStage;
            newStage = e.NewStage;
        };

        player.ModifyFatigue(55.0); // Refreshed → Lethargy
        Assert.Equal(FatigueStage.Refreshed, oldStage);
        Assert.Equal(FatigueStage.Lethargy, newStage);
    }

    [Fact]
    public void Player_RestoreFromSave_RestoresFatigueCorrectly()
    {
        var player = Player.Create("Test", Stats.Default);
        player.RestoreFromSave(1, 0, 100, 70, 50, 50, 50, 3, fatigue: 75.0);
        Assert.Equal(75.0, player.Fatigue);
        Assert.Equal(FatigueStage.Fatigue, player.FatigueStage);
    }

    #endregion

    #region BodyConditionSystem統合テスト

    [Fact]
    public void BodyConditionSystem_GetFatigueModifier_NewStages()
    {
        Assert.Equal(1.0f, BodyConditionSystem.GetFatigueModifier(FatigueStage.Refreshed));
        Assert.Equal(1.0f, BodyConditionSystem.GetFatigueModifier(FatigueStage.Normal));
        Assert.True(BodyConditionSystem.GetFatigueModifier(FatigueStage.Lethargy) < 1.0f);
        Assert.True(BodyConditionSystem.GetFatigueModifier(FatigueStage.TotalExhaustion) == 0.0f);
    }

    [Fact]
    public void BodyConditionSystem_GetFatigueName_NewStages()
    {
        Assert.Equal("快調", BodyConditionSystem.GetFatigueName(FatigueStage.Refreshed));
        Assert.Equal("通常", BodyConditionSystem.GetFatigueName(FatigueStage.Normal));
        Assert.Equal("疲労困憊", BodyConditionSystem.GetFatigueName(FatigueStage.TotalExhaustion));
    }

    #endregion

    #region 回復量テスト

    [Theory]
    [InlineData(FatigueStage.Refreshed, 1.0)]
    [InlineData(FatigueStage.Normal, 1.0)]
    [InlineData(FatigueStage.Lethargy, 0.1)]
    [InlineData(FatigueStage.LightFatigue, 0.01)]
    [InlineData(FatigueStage.Fatigue, 0.001)]
    [InlineData(FatigueStage.HeavyFatigue, 0.001)]
    [InlineData(FatigueStage.Exhaustion, 0.001)]
    [InlineData(FatigueStage.TotalExhaustion, 0.001)]
    public void GetRecoveryAmount_AllStages_ReturnsCorrectAmount(
        FatigueStage stage, double expected)
    {
        Assert.Equal(expected, FatigueSystem.GetRecoveryAmount(stage), 6);
    }

    #endregion

    #region 気付け薬テスト（タスク62）

    [Fact]
    public void UseFatigueReliefDrug_SetsReliefFlagAndDuration()
    {
        var player = Player.Create("Test", Stats.Default);
        Assert.False(player.HasFatigueRestrictionRelief);
        Assert.Equal(0, player.FatigueRestrictionReliefRemainingTurns);

        player.UseFatigueReliefDrug();
        Assert.True(player.HasFatigueRestrictionRelief);
        Assert.Equal(FatigueConstants.RestrictionReliefDuration, player.FatigueRestrictionReliefRemainingTurns);
    }

    [Fact]
    public void UseFatigueReliefDrug_ReUseDuringEffect_ResetsDuration()
    {
        var player = Player.Create("Test", Stats.Default);
        player.UseFatigueReliefDrug();

        // 一部ターン消費
        player.TickFatigueRestrictionRelief(1000);
        Assert.Equal(FatigueConstants.RestrictionReliefDuration - 1000, player.FatigueRestrictionReliefRemainingTurns);

        // 再使用でリセット
        player.UseFatigueReliefDrug();
        Assert.True(player.HasFatigueRestrictionRelief);
        Assert.Equal(FatigueConstants.RestrictionReliefDuration, player.FatigueRestrictionReliefRemainingTurns);
    }

    [Fact]
    public void TickFatigueRestrictionRelief_ExpireAfterDuration()
    {
        var player = Player.Create("Test", Stats.Default);
        player.UseFatigueReliefDrug();

        // 効果持続中
        bool expired = player.TickFatigueRestrictionRelief(FatigueConstants.RestrictionReliefDuration - 1);
        Assert.False(expired);
        Assert.True(player.HasFatigueRestrictionRelief);
        Assert.Equal(1, player.FatigueRestrictionReliefRemainingTurns);

        // 効果切れ
        expired = player.TickFatigueRestrictionRelief(1);
        Assert.True(expired);
        Assert.False(player.HasFatigueRestrictionRelief);
        Assert.Equal(0, player.FatigueRestrictionReliefRemainingTurns);
    }

    [Fact]
    public void TickFatigueRestrictionRelief_WithoutEffect_ReturnsFalse()
    {
        var player = Player.Create("Test", Stats.Default);
        Assert.False(player.TickFatigueRestrictionRelief(100));
    }

    [Fact]
    public void Exhaustion_WithRestrictionRelief_MovementAllowed()
    {
        // 疲弊状態 + 制限解除中に移動が可能
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.Exhaustion, TurnActionType.Move, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.Exhaustion, TurnActionType.Attack, hasFatigueRestrictionRelief: true));
    }

    [Fact]
    public void TotalExhaustion_WithRestrictionRelief_MovementAndAttackAllowed()
    {
        // 疲労困憊状態 + 制限解除中に移動・攻撃が可能
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.TotalExhaustion, TurnActionType.Move, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.TotalExhaustion, TurnActionType.Attack, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.TotalExhaustion, TurnActionType.UseSkill, hasFatigueRestrictionRelief: true));
    }

    [Fact]
    public void HeavyFatigue_WithRestrictionRelief_AttackAndSkillAllowed()
    {
        // 疲労（重）+ 制限解除中に攻撃・スキルが可能
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.HeavyFatigue, TurnActionType.Attack, hasFatigueRestrictionRelief: true));
        Assert.True(FatigueSystem.CanPerformAction(FatigueStage.HeavyFatigue, TurnActionType.UseSkill, hasFatigueRestrictionRelief: true));
    }

    [Fact]
    public void ActionCostBonus_MaintainedDuringRestrictionRelief()
    {
        // 行動コスト加算は制限解除中も維持される
        Assert.Equal(3, FatigueSystem.GetActionCostBonus(FatigueStage.HeavyFatigue));
        Assert.Equal(4, FatigueSystem.GetActionCostBonus(FatigueStage.Exhaustion));
        Assert.Equal(5, FatigueSystem.GetActionCostBonus(FatigueStage.TotalExhaustion));
    }

    [Fact]
    public void SpModifier_MaintainedDuringRestrictionRelief()
    {
        // SP上限修正は制限解除中も維持される
        Assert.Equal(-0.50, FatigueSystem.GetSpModifier(FatigueStage.HeavyFatigue));
        Assert.Equal(-0.80, FatigueSystem.GetSpModifier(FatigueStage.Exhaustion));
        Assert.Equal(-1.00, FatigueSystem.GetSpModifier(FatigueStage.TotalExhaustion));
    }

    #endregion
}
