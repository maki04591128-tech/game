using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.3 方向・向きシステムのテスト
/// </summary>
public class DirectionSystemTests
{
    [Theory]
    [InlineData(AttackDirection.Front, 0f, 0f)]
    [InlineData(AttackDirection.Side, 0.15f, 0.10f)]
    [InlineData(AttackDirection.Back, 0.30f, 0.25f)]
    public void GetDirectionBonus_ReturnsCorrectValues(AttackDirection dir, float expectedHit, float expectedDmg)
    {
        var bonus = DirectionSystem.GetDirectionBonus(dir);
        Assert.Equal(expectedHit, bonus.HitRateModifier);
        Assert.Equal(expectedDmg, bonus.DamageModifier);
    }

    [Fact]
    public void DetermineAttackDirection_SameFacing_IsBack()
    {
        Assert.Equal(AttackDirection.Back,
            DirectionSystem.DetermineAttackDirection(Direction.North, Direction.North));
    }

    [Fact]
    public void DetermineAttackDirection_OppositeFacing_IsFront()
    {
        Assert.Equal(AttackDirection.Front,
            DirectionSystem.DetermineAttackDirection(Direction.North, Direction.South));
    }

    [Fact]
    public void DetermineAttackDirection_PerpendicularFacing_IsSide()
    {
        Assert.Equal(AttackDirection.Side,
            DirectionSystem.DetermineAttackDirection(Direction.North, Direction.East));
    }

    [Theory]
    [InlineData(Direction.North, Direction.South, true)]
    [InlineData(Direction.East, Direction.West, true)]
    [InlineData(Direction.NorthEast, Direction.SouthWest, true)]
    [InlineData(Direction.North, Direction.East, false)]
    [InlineData(Direction.North, Direction.North, false)]
    public void IsOpposite_ReturnsCorrectResult(Direction a, Direction b, bool expected)
    {
        Assert.Equal(expected, DirectionSystem.IsOpposite(a, b));
    }

    [Fact]
    public void GetElevationBonus_HigherAttacker_PositiveBonus()
    {
        var bonus = DirectionSystem.GetElevationBonus(2, 1);
        Assert.True(bonus.DamageModifier > 0);
        Assert.True(bonus.HitRateModifier > 0);
    }

    [Fact]
    public void GetElevationBonus_LowerAttacker_NegativeBonus()
    {
        var bonus = DirectionSystem.GetElevationBonus(1, 2);
        Assert.True(bonus.DamageModifier < 0);
        Assert.True(bonus.HitRateModifier < 0);
    }

    [Fact]
    public void GetElevationBonus_SameElevation_NoBonus()
    {
        var bonus = DirectionSystem.GetElevationBonus(1, 1);
        Assert.Equal(0f, bonus.DamageModifier);
        Assert.Equal(0f, bonus.HitRateModifier);
    }

    [Fact]
    public void GetFacingFromMovement_ReturnsSameDirection()
    {
        Assert.Equal(Direction.NorthEast, DirectionSystem.GetFacingFromMovement(Direction.NorthEast));
    }
}
