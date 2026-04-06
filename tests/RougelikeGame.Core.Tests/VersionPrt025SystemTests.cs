using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;
using RougelikeGame.Engine.Combat;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.25 テスト — 満腹度・渇き度9段階システム＆行動ターン消費システム
/// </summary>
public class VersionPrt025SystemTests
{
    private static Player CreateDefaultPlayer()
    {
        return Player.Create("Test", Stats.Default);
    }

    #region タスク25: 満腹度段階テスト

    [Theory]
    [InlineData(150, HungerStage.Nausea)]
    [InlineData(130, HungerStage.Nausea)]
    [InlineData(120, HungerStage.Nausea)]
    [InlineData(119, HungerStage.Overeating)]
    [InlineData(100, HungerStage.Overeating)]
    [InlineData(99, HungerStage.Full)]
    [InlineData(80, HungerStage.Full)]
    [InlineData(79, HungerStage.Normal)]
    [InlineData(50, HungerStage.Normal)]
    [InlineData(49, HungerStage.SlightlyHungry)]
    [InlineData(40, HungerStage.SlightlyHungry)]
    [InlineData(39, HungerStage.VeryHungry)]
    [InlineData(1, HungerStage.VeryHungry)]
    [InlineData(0, HungerStage.VeryHungry)]
    [InlineData(-1, HungerStage.Starving)]
    [InlineData(-5, HungerStage.Starving)]
    [InlineData(-8, HungerStage.Starving)]
    [InlineData(-9, HungerStage.NearStarvation)]
    [InlineData(-10, HungerStage.Starvation)]
    public void HungerStage_BoundaryValues_ReturnsCorrectStage(int hungerValue, HungerStage expected)
    {
        var player = CreateDefaultPlayer();
        player.ModifyHunger(hungerValue - player.Hunger);

        Assert.Equal(expected, player.HungerStage);
    }

    [Fact]
    public void HungerStageChanged_EventFires_OnStageTransition()
    {
        var player = CreateDefaultPlayer();
        var changes = new List<(HungerStage Old, HungerStage New)>();
        player.OnHungerStageChanged += (_, e) => changes.Add((e.OldStage, e.NewStage));

        // 初期 70 (Normal) → 40 (SlightlyHungry)
        player.ModifyHunger(-30);

        Assert.Single(changes);
        Assert.Equal(HungerStage.Normal, changes[0].Old);
        Assert.Equal(HungerStage.SlightlyHungry, changes[0].New);
    }

    [Fact]
    public void Hunger_ClampedToMinMax()
    {
        var player = CreateDefaultPlayer();

        // Maxを超えた場合
        player.ModifyHunger(200);
        Assert.Equal(GameConstants.MaxHunger, player.Hunger);

        // Minを下回った場合
        player.ModifyHunger(-300);
        Assert.Equal(GameConstants.MinHunger, player.Hunger);
    }

    [Fact]
    public void Hunger_InitialValue_Is70()
    {
        var player = CreateDefaultPlayer();
        Assert.Equal(GameConstants.InitialHunger, player.Hunger);
        Assert.Equal(70, player.Hunger);
        Assert.Equal(HungerStage.Normal, player.HungerStage);
    }

    #endregion

    #region タスク26: 渇き度段階テスト

    [Theory]
    [InlineData(150, ThirstStage.Nausea)]
    [InlineData(130, ThirstStage.Nausea)]
    [InlineData(120, ThirstStage.Nausea)]
    [InlineData(119, ThirstStage.Overdrinking)]
    [InlineData(100, ThirstStage.Overdrinking)]
    [InlineData(99, ThirstStage.Full)]
    [InlineData(80, ThirstStage.Full)]
    [InlineData(79, ThirstStage.Normal)]
    [InlineData(50, ThirstStage.Normal)]
    [InlineData(49, ThirstStage.SlightlyThirsty)]
    [InlineData(40, ThirstStage.SlightlyThirsty)]
    [InlineData(39, ThirstStage.VeryThirsty)]
    [InlineData(1, ThirstStage.VeryThirsty)]
    [InlineData(0, ThirstStage.VeryThirsty)]
    [InlineData(-1, ThirstStage.Dehydrated)]
    [InlineData(-5, ThirstStage.Dehydrated)]
    [InlineData(-8, ThirstStage.Dehydrated)]
    [InlineData(-9, ThirstStage.NearDesiccation)]
    [InlineData(-10, ThirstStage.Desiccation)]
    public void ThirstStage_BoundaryValues_ReturnsCorrectStage(int thirstValue, ThirstStage expected)
    {
        var player = CreateDefaultPlayer();
        player.ModifyThirst(thirstValue - player.Thirst);

        Assert.Equal(expected, player.ThirstStage);
    }

    [Fact]
    public void ThirstStageChanged_EventFires_OnStageTransition()
    {
        var player = CreateDefaultPlayer();
        var changes = new List<(ThirstStage Old, ThirstStage New)>();
        player.OnThirstStageChanged += (_, e) => changes.Add((e.OldStage, e.NewStage));

        // 初期 70 (Normal) → 40 (SlightlyThirsty)
        player.ModifyThirst(-30);

        Assert.Single(changes);
        Assert.Equal(ThirstStage.Normal, changes[0].Old);
        Assert.Equal(ThirstStage.SlightlyThirsty, changes[0].New);
    }

    [Fact]
    public void Thirst_ClampedToMinMax()
    {
        var player = CreateDefaultPlayer();

        // Maxを超えた場合
        player.ModifyThirst(200);
        Assert.Equal(GameConstants.MaxThirst, player.Thirst);

        // Minを下回った場合
        player.ModifyThirst(-300);
        Assert.Equal(GameConstants.MinThirst, player.Thirst);
    }

    #endregion

    #region タスク27: 行動コスト統合テスト

    [Fact]
    public void TurnAction_HungerCostBonus_AddedToFinalCost()
    {
        var action = TurnAction.Move(Direction.North);
        // baseCost=1, hungerBonus=2 → cost=3
        int cost = action.CalculateFinalCost(CombatState.Normal, hungerCostBonus: 2);
        Assert.Equal(3, cost);
    }

    [Fact]
    public void TurnAction_ThirstCostBonus_AddedToFinalCost()
    {
        var action = TurnAction.Move(Direction.North);
        // baseCost=1, thirstBonus=2 → cost=3
        int cost = action.CalculateFinalCost(CombatState.Normal, thirstCostBonus: 2);
        Assert.Equal(3, cost);
    }

    [Fact]
    public void TurnAction_CombinedHungerThirstBonus()
    {
        var action = TurnAction.Move(Direction.North);
        // baseCost=1, hunger+2, thirst+2 → cost=5
        int cost = action.CalculateFinalCost(CombatState.Normal, hungerCostBonus: 2, thirstCostBonus: 2);
        Assert.Equal(5, cost);
    }

    [Fact]
    public void TurnAction_FinalCost_MinimumIs1()
    {
        var action = TurnAction.Move(Direction.North);
        int cost = action.CalculateFinalCost(CombatState.Normal, hungerCostBonus: 0, thirstCostBonus: 0);
        Assert.True(cost >= 1);
    }

    #endregion

    #region タスク28: 飢餓・脱水ダメージテスト

    [Fact]
    public void ResourceSystem_HungerEffect_StarvingDamage()
    {
        var system = new ResourceSystem();
        // 飢餓（-1〜-8）: 毎ターン1ダメージ
        var effect = system.GetHungerEffect(-3);
        Assert.Equal(1, effect.DamagePerTurn);
        Assert.Equal(2, effect.ActionCostBonus);
    }

    [Fact]
    public void ResourceSystem_HungerEffect_NearStarvationDamage()
    {
        var system = new ResourceSystem();
        // 餓死寸前（-9）: 毎ターン10ダメージ
        var effect = system.GetHungerEffect(-9);
        Assert.Equal(10, effect.DamagePerTurn);
        Assert.False(effect.AllowHpRecovery);
        Assert.False(effect.AllowSpRecovery);
    }

    [Fact]
    public void ThirstSystem_DehydratedDamage()
    {
        // 脱水（-1〜-8）: 毎ターン1ダメージ
        int damage = ThirstSystem.GetThirstDamage(ThirstStage.Dehydrated);
        Assert.Equal(1, damage);
    }

    [Fact]
    public void ThirstSystem_NearDesiccationDamage()
    {
        // 干死寸前（-9）: 毎ターン10ダメージ
        int damage = ThirstSystem.GetThirstDamage(ThirstStage.NearDesiccation);
        Assert.Equal(10, damage);
    }

    #endregion

    #region タスク29: 餓死・干死判定テスト

    [Fact]
    public void Player_Hunger_CanReachMinus10()
    {
        var player = CreateDefaultPlayer();
        player.ModifyHunger(-200);  // clampedToMinHunger
        Assert.Equal(GameConstants.MinHunger, player.Hunger);
        Assert.Equal(-10, player.Hunger);
        Assert.Equal(HungerStage.Starvation, player.HungerStage);
    }

    [Fact]
    public void Player_Thirst_CanReachMinus10()
    {
        var player = CreateDefaultPlayer();
        player.ModifyThirst(-200);  // clampedToMinThirst
        Assert.Equal(GameConstants.MinThirst, player.Thirst);
        Assert.Equal(-10, player.Thirst);
        Assert.Equal(ThirstStage.Desiccation, player.ThirstStage);
    }

    #endregion

    #region GameConstants 定数テスト

    [Fact]
    public void GameConstants_HungerValues_AreCorrect()
    {
        Assert.Equal(70, GameConstants.InitialHunger);
        Assert.Equal(150, GameConstants.MaxHunger);
        Assert.Equal(-10, GameConstants.MinHunger);
    }

    [Fact]
    public void GameConstants_ThirstValues_AreCorrect()
    {
        Assert.Equal(70, GameConstants.InitialThirst);
        Assert.Equal(150, GameConstants.MaxThirst);
        Assert.Equal(-10, GameConstants.MinThirst);
    }

    [Fact]
    public void TimeConstants_DecayIntervals_AreCorrect()
    {
        Assert.Equal(864, TimeConstants.HungerDecayInterval);
        Assert.Equal(59220, TimeConstants.HungerDecayIntervalStarving);
        Assert.Equal(432, TimeConstants.ThirstDecayInterval);
        Assert.Equal(29610, TimeConstants.ThirstDecayIntervalStarving);
    }

    #endregion

    #region ResourceSystem 9段階テスト

    [Theory]
    [InlineData(130, HungerState.Nausea)]
    [InlineData(120, HungerState.Nausea)]
    [InlineData(119, HungerState.Overeating)]
    [InlineData(100, HungerState.Overeating)]
    [InlineData(99, HungerState.Satiated)]
    [InlineData(80, HungerState.Satiated)]
    [InlineData(79, HungerState.Normal)]
    [InlineData(50, HungerState.Normal)]
    [InlineData(49, HungerState.SlightlyHungry)]
    [InlineData(40, HungerState.SlightlyHungry)]
    [InlineData(39, HungerState.VeryHungry)]
    [InlineData(0, HungerState.VeryHungry)]
    [InlineData(-1, HungerState.Starving)]
    [InlineData(-8, HungerState.Starving)]
    [InlineData(-9, HungerState.NearStarvation)]
    [InlineData(-10, HungerState.Starvation)]
    public void ResourceSystem_GetHungerState_10Stages(int hunger, HungerState expected)
    {
        var system = new ResourceSystem();
        Assert.Equal(expected, system.GetHungerState(hunger));
    }

    [Fact]
    public void ResourceSystem_HungerEffect_NauseaHasActionBlock()
    {
        var system = new ResourceSystem();
        var effect = system.GetHungerEffect(130);
        Assert.Equal(3, effect.ActionCostBonus);
        Assert.Equal(0.3f, effect.ActionBlockChance);
    }

    [Fact]
    public void ResourceSystem_HungerEffect_OvereatingCostBonus()
    {
        var system = new ResourceSystem();
        var effect = system.GetHungerEffect(110);
        Assert.Equal(2, effect.ActionCostBonus);
    }

    [Fact]
    public void ResourceSystem_HungerEffect_NormalNoPenalty()
    {
        var system = new ResourceSystem();
        var effect = system.GetHungerEffect(60);
        Assert.Equal(0, effect.ActionCostBonus);
        Assert.Equal(0f, effect.ActionBlockChance);
        Assert.Equal(0, effect.DamagePerTurn);
    }

    #endregion

    #region ThirstSystem 9段階テスト

    [Theory]
    [InlineData(ThirstStage.Nausea, "吐き気")]
    [InlineData(ThirstStage.Overdrinking, "過飲")]
    [InlineData(ThirstStage.Full, "満腹")]
    [InlineData(ThirstStage.Normal, "通常")]
    [InlineData(ThirstStage.SlightlyThirsty, "渇き（小）")]
    [InlineData(ThirstStage.VeryThirsty, "渇き（大）")]
    [InlineData(ThirstStage.Dehydrated, "脱水")]
    [InlineData(ThirstStage.NearDesiccation, "干死寸前")]
    [InlineData(ThirstStage.Desiccation, "干死")]
    public void ThirstSystem_GetThirstName_ReturnsJapanese(ThirstStage stage, string expectedName)
    {
        Assert.Equal(expectedName, ThirstSystem.GetThirstName(stage));
    }

    [Theory]
    [InlineData(ThirstStage.Nausea, 0.7f, 0.7f, 0.7f)]
    [InlineData(ThirstStage.Overdrinking, 0.9f, 0.9f, 0.9f)]
    [InlineData(ThirstStage.Full, 1.0f, 1.0f, 1.0f)]
    [InlineData(ThirstStage.Normal, 1.0f, 1.0f, 1.0f)]
    [InlineData(ThirstStage.SlightlyThirsty, 0.95f, 0.95f, 0.95f)]
    [InlineData(ThirstStage.VeryThirsty, 0.9f, 0.9f, 0.85f)]
    [InlineData(ThirstStage.Dehydrated, 0.6f, 0.6f, 0.6f)]
    [InlineData(ThirstStage.NearDesiccation, 0.2f, 0.2f, 0.3f)]
    public void ThirstSystem_GetThirstModifiers_NewStages(ThirstStage stage, float strMod, float agiMod, float intMod)
    {
        var (str, agi, inte) = ThirstSystem.GetThirstModifiers(stage);
        Assert.Equal(strMod, str);
        Assert.Equal(agiMod, agi);
        Assert.Equal(intMod, inte);
    }

    [Theory]
    [InlineData(ThirstStage.Nausea, 3)]
    [InlineData(ThirstStage.Overdrinking, 2)]
    [InlineData(ThirstStage.Full, 1)]
    [InlineData(ThirstStage.Normal, 0)]
    [InlineData(ThirstStage.SlightlyThirsty, 0)]
    [InlineData(ThirstStage.VeryThirsty, 1)]
    [InlineData(ThirstStage.Dehydrated, 2)]
    public void ThirstSystem_GetThirstActionCostBonus_ReturnsCorrectValues(ThirstStage stage, int expected)
    {
        Assert.Equal(expected, ThirstSystem.GetThirstActionCostBonus(stage));
    }

    [Fact]
    public void ThirstSystem_NauseaActionBlockChance_Is30Percent()
    {
        Assert.Equal(0.3f, ThirstSystem.GetThirstActionBlockChance(ThirstStage.Nausea));
        Assert.Equal(0f, ThirstSystem.GetThirstActionBlockChance(ThirstStage.Normal));
    }

    #endregion

    #region ペナルティテスト（Player StatModifier）

    [Fact]
    public void Player_HungerPenalty_NauseaReducesStrength()
    {
        var player = CreateDefaultPlayer();
        int baseStr = player.EffectiveStats.Strength;
        player.ModifyHunger(150 - player.Hunger);  // Nausea

        // Nausea: Strength: -5, Agility: -5, Dexterity: -3, Intelligence: -3
        Assert.True(player.EffectiveStats.Strength < baseStr, "Nausea should reduce Strength");
    }

    [Fact]
    public void Player_ThirstPenalty_DehydratedReducesIntelligence()
    {
        var player = CreateDefaultPlayer();
        int baseInt = player.EffectiveStats.Intelligence;
        player.ModifyThirst(-75);  // InitialThirst(70) - 75 = -5 → Dehydrated段階

        Assert.True(player.EffectiveStats.Intelligence < baseInt, "Dehydrated should reduce Intelligence");
    }

    #endregion

    #region タスク42: 行動ターン消費テスト

    [Fact]
    public void TurnCosts_AttackNormal_Is1()
    {
        Assert.Equal(1, TurnCosts.AttackNormal);
    }

    [Fact]
    public void TurnCosts_AttackUnarmed_Is1()
    {
        Assert.Equal(1, TurnCosts.AttackUnarmed);
    }

    [Fact]
    public void TurnCosts_AttackTwoHanded_Is1()
    {
        Assert.Equal(1, TurnCosts.AttackTwoHanded);
    }

    [Fact]
    public void TurnCosts_AttackBow_Is5()
    {
        Assert.Equal(5, TurnCosts.AttackBow);
    }

    [Fact]
    public void TurnCosts_AttackThrow_Is5()
    {
        Assert.Equal(5, TurnCosts.AttackThrow);
    }

    [Fact]
    public void TurnCosts_UsePotion_Is1()
    {
        Assert.Equal(1, TurnCosts.UsePotion);
    }

    [Fact]
    public void TurnCosts_MoveNormal_Is1()
    {
        Assert.Equal(1, TurnCosts.MoveNormal);
    }

    [Fact]
    public void TurnCosts_Search_Is5()
    {
        Assert.Equal(5, TurnCosts.Search);
    }

    [Fact]
    public void TurnCosts_UseStairs_Is10()
    {
        Assert.Equal(10, TurnCosts.UseStairs);
    }

    [Fact]
    public void TurnCosts_Pray_Is10()
    {
        Assert.Equal(10, TurnCosts.Pray);
    }

    [Fact]
    public void TurnCosts_EquipWeapon_Is1()
    {
        Assert.Equal(1, TurnCosts.EquipWeapon);
    }

    [Fact]
    public void TurnCosts_EquipAccessory_Is1()
    {
        Assert.Equal(1, TurnCosts.EquipAccessory);
    }

    [Fact]
    public void TurnCosts_EquipArms_Is10()
    {
        Assert.Equal(10, TurnCosts.EquipArms);
    }

    [Fact]
    public void TurnCosts_EquipHead_Is10()
    {
        Assert.Equal(10, TurnCosts.EquipHead);
    }

    [Fact]
    public void TurnCosts_EquipBody_Is20()
    {
        Assert.Equal(20, TurnCosts.EquipBody);
    }

    [Fact]
    public void TurnCosts_InventorySort_Is20()
    {
        Assert.Equal(20, TurnCosts.InventorySort);
    }

    [Fact]
    public void TurnCosts_SymbolMapMove_Is300()
    {
        Assert.Equal(300, TurnCosts.SymbolMapMove);
    }

    [Fact]
    public void TurnCosts_SymbolMapEntry_Is0()
    {
        Assert.Equal(0, TurnCosts.SymbolMapEntry);
    }

    [Fact]
    public void TurnCosts_OpenDoor_Is1()
    {
        Assert.Equal(1, TurnCosts.OpenDoor);
    }

    [Fact]
    public void TurnCosts_EngagedNonCombatMultiplier_Is2()
    {
        Assert.Equal(2, TurnCosts.EngagedNonCombatMultiplier);
    }

    [Fact]
    public void TurnCosts_StealthMovementMultiplier_Is5()
    {
        Assert.Equal(5, TurnCosts.StealthMovementMultiplier);
    }

    #endregion

    #region タスク47: 状態別ターンコストテスト

    [Fact]
    public void Engaged_CombatAction_NormalCost()
    {
        // 接敵中に通常攻撃はコスト変更なし
        var action = TurnAction.Attack(null!);
        int normalCost = action.CalculateFinalCost(CombatState.Normal);
        int engagedCost = action.CalculateFinalCost(CombatState.Combat);
        Assert.Equal(normalCost, engagedCost);
    }

    [Fact]
    public void Engaged_NonCombatAction_Interact_DoubledCost()
    {
        // 接敵中にインタラクト（ドア開閉等）は×2
        var action = TurnAction.Interact;
        int normalCost = action.CalculateFinalCost(CombatState.Normal);
        int engagedCost = action.CalculateFinalCost(CombatState.Combat);
        Assert.Equal(normalCost * TurnCosts.EngagedNonCombatMultiplier, engagedCost);
    }

    [Fact]
    public void Engaged_NonCombatAction_Search_DoubledCost()
    {
        // 接敵中に周辺調査は×2
        var action = TurnAction.Search;
        int normalCost = action.CalculateFinalCost(CombatState.Normal);
        int engagedCost = action.CalculateFinalCost(CombatState.Combat);
        Assert.Equal(normalCost * TurnCosts.EngagedNonCombatMultiplier, engagedCost);
    }

    [Fact]
    public void Stealth_Move_PenaltyCost()
    {
        // 隠密中に移動はMoveStealth（GetMovementMultiplierで適用）
        var action = TurnAction.Move(Direction.North);
        int stealthCost = action.CalculateFinalCost(CombatState.Stealth);
        Assert.Equal(TurnCosts.MoveStealth, stealthCost);
    }

    [Fact]
    public void Stealth_Interact_PenaltyCost()
    {
        // 隠密中にインタラクト（ドア開閉）は×5
        var action = TurnAction.Interact;
        int normalCost = action.CalculateFinalCost(CombatState.Normal);
        int stealthCost = action.CalculateFinalCost(CombatState.Stealth);
        Assert.Equal(normalCost * TurnCosts.StealthMovementMultiplier, stealthCost);
    }

    [Fact]
    public void Stealth_Attack_NormalCost()
    {
        // 隠密中に通常攻撃はコスト変更なし
        var action = TurnAction.Attack(null!);
        int normalCost = action.CalculateFinalCost(CombatState.Normal);
        int stealthCost = action.CalculateFinalCost(CombatState.Stealth);
        Assert.Equal(normalCost, stealthCost);
    }

    [Fact]
    public void Debuff_HungerBonus_AddsToNormalCost()
    {
        // 通常状態で移動（1T）+飢餓デバフ（+2T）=3T
        var action = TurnAction.Move(Direction.North);
        int cost = action.CalculateFinalCost(CombatState.Normal, hungerCostBonus: 2);
        Assert.Equal(3, cost);
    }

    [Fact]
    public void Debuff_HungerBonus_AddsToEngagedCost()
    {
        // 接敵状態でインタラクト（1×2=2T）+飢餓デバフ（+2T）=4T
        var action = TurnAction.Interact;
        int cost = action.CalculateFinalCost(CombatState.Combat, hungerCostBonus: 2);
        Assert.Equal(4, cost);
    }

    [Fact]
    public void Debuff_HungerBonus_AddsToStealthCost()
    {
        // 隠密状態でインタラクト（1×5=5T）+飢餓デバフ（+2T）=7T
        var action = TurnAction.Interact;
        int cost = action.CalculateFinalCost(CombatState.Stealth, hungerCostBonus: 2);
        Assert.Equal(7, cost);
    }

    [Fact]
    public void MultipleDebuffs_Combined()
    {
        // 複数デバフ: 移動1T + hunger+2 + thirst+2 = 5T
        var action = TurnAction.Move(Direction.North);
        int cost = action.CalculateFinalCost(CombatState.Normal, hungerCostBonus: 2, thirstCostBonus: 2);
        Assert.Equal(5, cost);
    }

    [Fact]
    public void IsCombatAction_AttackIsTrue()
    {
        Assert.True(TurnAction.IsCombatAction(TurnActionType.Attack));
        Assert.True(TurnAction.IsCombatAction(TurnActionType.UseSkill));
        Assert.True(TurnAction.IsCombatAction(TurnActionType.CastSpell));
        Assert.True(TurnAction.IsCombatAction(TurnActionType.UseItem));
    }

    [Fact]
    public void IsCombatAction_MoveIsFalse()
    {
        Assert.False(TurnAction.IsCombatAction(TurnActionType.Move));
        Assert.False(TurnAction.IsCombatAction(TurnActionType.Wait));
        Assert.False(TurnAction.IsCombatAction(TurnActionType.Search));
        Assert.False(TurnAction.IsCombatAction(TurnActionType.Rest));
        Assert.False(TurnAction.IsCombatAction(TurnActionType.Interact));
    }

    [Fact]
    public void IsStealthPenaltyAction_MoveAndInteractAreTrue()
    {
        Assert.True(TurnAction.IsStealthPenaltyAction(TurnActionType.Move));
        Assert.True(TurnAction.IsStealthPenaltyAction(TurnActionType.Interact));
    }

    [Fact]
    public void IsStealthPenaltyAction_AttackIsFalse()
    {
        Assert.False(TurnAction.IsStealthPenaltyAction(TurnActionType.Attack));
        Assert.False(TurnAction.IsStealthPenaltyAction(TurnActionType.UseSkill));
        Assert.False(TurnAction.IsStealthPenaltyAction(TurnActionType.Wait));
    }

    #endregion
}
