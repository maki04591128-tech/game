using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.51 物品価値変動 / P.59 戦闘スタンス / P.60 処刑 / P.58 環境利用戦闘のテスト
/// </summary>
public class CombatExtensionTests
{
    #region PriceFluctuation Tests

    [Fact]
    public void CalculateSupplyDemandModifier_HighDemand_HighPrice()
    {
        float mod = PriceFluctuationSystem.CalculateSupplyDemandModifier(10, 30);
        Assert.True(mod > 1.0f);
    }

    [Fact]
    public void CalculateSupplyDemandModifier_LowDemand_LowPrice()
    {
        float mod = PriceFluctuationSystem.CalculateSupplyDemandModifier(30, 10);
        Assert.True(mod < 1.0f);
    }

    [Fact]
    public void GetTerritoryModifier_ForestHerb_Discount()
    {
        Assert.True(PriceFluctuationSystem.GetTerritoryModifier(TerritoryId.Forest, "herb") < 1.0f);
    }

    [Fact]
    public void GetKarmaModifier_SaintBuying_Discount()
    {
        Assert.True(PriceFluctuationSystem.GetKarmaModifier(KarmaRank.Saint, true) < 1.0f);
    }

    [Fact]
    public void CalculateFinalPrice_ReturnsPositive()
    {
        int price = PriceFluctuationSystem.CalculateFinalPrice(100, 1.0f, 1.0f, 1.0f, 1.0f, true);
        Assert.True(price > 0);
    }

    #endregion

    #region CombatStance Tests

    [Fact]
    public void GetAttackModifier_Aggressive_Higher()
    {
        Assert.True(CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive) > 1.0f);
    }

    [Fact]
    public void GetDefenseModifier_Defensive_Higher()
    {
        Assert.True(CombatStanceSystem.GetDefenseModifier(CombatStance.Defensive) > 1.0f);
    }

    [Fact]
    public void GetStanceName_ReturnsJapanese()
    {
        Assert.Equal("攻撃型", CombatStanceSystem.GetStanceName(CombatStance.Aggressive));
        Assert.Equal("防御型", CombatStanceSystem.GetStanceName(CombatStance.Defensive));
        Assert.Equal("バランス型", CombatStanceSystem.GetStanceName(CombatStance.Balanced));
    }

    [Fact]
    public void Balanced_AllModifiersAreDefault()
    {
        Assert.Equal(1.0f, CombatStanceSystem.GetAttackModifier(CombatStance.Balanced));
        Assert.Equal(1.0f, CombatStanceSystem.GetDefenseModifier(CombatStance.Balanced));
        Assert.Equal(0f, CombatStanceSystem.GetEvasionModifier(CombatStance.Balanced));
    }

    #endregion

    #region Execution Tests

    [Fact]
    public void CanExecute_BelowThreshold_True()
    {
        Assert.True(ExecutionSystem.CanExecute(5, 100));
    }

    [Fact]
    public void CanExecute_AboveThreshold_False()
    {
        Assert.False(ExecutionSystem.CanExecute(15, 100));
    }

    [Fact]
    public void GetExecutionKarmaPenalty_HumanoidMost()
    {
        int humanoid = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Humanoid);
        int beast = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Beast);
        Assert.True(humanoid < beast);
    }

    [Fact]
    public void GetMercyKarmaBonus_Positive()
    {
        Assert.True(ExecutionSystem.GetMercyKarmaBonus() > 0);
    }

    #endregion

    #region Environmental Combat Tests

    [Fact]
    public void GetInteraction_WaterLightning_Electrified()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(
            EnvironmentalCombatSystem.SurfaceType.Water, Element.Lightning);
        Assert.NotNull(result);
        Assert.Equal(EnvironmentalCombatSystem.SurfaceType.Electrified, result!.ResultSurface);
        Assert.True(result.DamageMultiplier > 1.0f);
    }

    [Fact]
    public void GetInteraction_OilFire_HighDamage()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(
            EnvironmentalCombatSystem.SurfaceType.Oil, Element.Fire);
        Assert.NotNull(result);
        Assert.Equal(2.0f, result!.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_NoReaction_ReturnsNull()
    {
        Assert.Null(EnvironmentalCombatSystem.GetInteraction(
            EnvironmentalCombatSystem.SurfaceType.Normal, Element.Fire));
    }

    [Fact]
    public void GetSurfaceDamage_FireDealsDamage()
    {
        Assert.True(EnvironmentalCombatSystem.GetSurfaceDamage(
            EnvironmentalCombatSystem.SurfaceType.Fire) > 0);
    }

    [Fact]
    public void GetMovementModifier_IceSlows()
    {
        Assert.True(EnvironmentalCombatSystem.GetMovementModifier(
            EnvironmentalCombatSystem.SurfaceType.Ice) > 1.0f);
    }

    #endregion
}
