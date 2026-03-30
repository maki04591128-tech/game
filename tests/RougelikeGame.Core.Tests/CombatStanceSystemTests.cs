using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// CombatStanceSystem（戦闘スタンスシステム）のテスト
/// </summary>
public class CombatStanceSystemTests
{
    // --- GetAttackModifier ---

    [Theory]
    [InlineData(CombatStance.Aggressive, 1.25f)]
    [InlineData(CombatStance.Balanced, 1.0f)]
    [InlineData(CombatStance.Defensive, 0.75f)]
    public void GetAttackModifier_ReturnsExpected(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetAttackModifier(stance));
    }

    // --- GetDefenseModifier ---

    [Theory]
    [InlineData(CombatStance.Aggressive, 0.8f)]
    [InlineData(CombatStance.Balanced, 1.0f)]
    [InlineData(CombatStance.Defensive, 1.3f)]
    public void GetDefenseModifier_ReturnsExpected(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetDefenseModifier(stance));
    }

    // --- GetEvasionModifier ---

    [Theory]
    [InlineData(CombatStance.Aggressive, -0.1f)]
    [InlineData(CombatStance.Balanced, 0f)]
    [InlineData(CombatStance.Defensive, 0.15f)]
    public void GetEvasionModifier_ReturnsExpected(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetEvasionModifier(stance));
    }

    // --- GetCriticalModifier ---

    [Theory]
    [InlineData(CombatStance.Aggressive, 0.1f)]
    [InlineData(CombatStance.Balanced, 0f)]
    [InlineData(CombatStance.Defensive, -0.05f)]
    public void GetCriticalModifier_ReturnsExpected(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetCriticalModifier(stance));
    }

    // --- GetStanceName ---

    [Theory]
    [InlineData(CombatStance.Aggressive, "攻撃型")]
    [InlineData(CombatStance.Defensive, "防御型")]
    [InlineData(CombatStance.Balanced, "バランス型")]
    public void GetStanceName_ReturnsJapaneseName(CombatStance stance, string expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetStanceName(stance));
    }

    // --- GetStanceDescription ---

    [Fact]
    public void GetStanceDescription_Aggressive_ContainsAttackBoost()
    {
        var desc = CombatStanceSystem.GetStanceDescription(CombatStance.Aggressive);
        Assert.Contains("攻撃力+25%", desc);
    }

    [Fact]
    public void GetStanceDescription_Defensive_ContainsDefenseBoost()
    {
        var desc = CombatStanceSystem.GetStanceDescription(CombatStance.Defensive);
        Assert.Contains("防御+30%", desc);
    }

    [Fact]
    public void GetStanceDescription_Balanced_MentionsStandard()
    {
        var desc = CombatStanceSystem.GetStanceDescription(CombatStance.Balanced);
        Assert.Contains("標準", desc);
    }

    // --- スタンス間のバランス検証 ---

    [Fact]
    public void Aggressive_HigherAttackThanDefensive()
    {
        Assert.True(
            CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive) >
            CombatStanceSystem.GetAttackModifier(CombatStance.Defensive));
    }

    [Fact]
    public void Defensive_HigherDefenseThanAggressive()
    {
        Assert.True(
            CombatStanceSystem.GetDefenseModifier(CombatStance.Defensive) >
            CombatStanceSystem.GetDefenseModifier(CombatStance.Aggressive));
    }

    [Fact]
    public void Balanced_AttackAndDefenseAreOne()
    {
        Assert.Equal(1.0f, CombatStanceSystem.GetAttackModifier(CombatStance.Balanced));
        Assert.Equal(1.0f, CombatStanceSystem.GetDefenseModifier(CombatStance.Balanced));
    }

    [Fact]
    public void Balanced_EvasionAndCriticalAreZero()
    {
        Assert.Equal(0f, CombatStanceSystem.GetEvasionModifier(CombatStance.Balanced));
        Assert.Equal(0f, CombatStanceSystem.GetCriticalModifier(CombatStance.Balanced));
    }
}
