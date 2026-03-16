using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class EnvironmentalCombatSystemTests
{
    [Fact]
    public void GetInteraction_WaterAndFire_ReturnsResult()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(EnvironmentalCombatSystem.SurfaceType.Water, Element.Fire);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetInteraction_OilAndFire_ReturnsHighDamage()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(EnvironmentalCombatSystem.SurfaceType.Oil, Element.Fire);
        Assert.NotNull(result);
        Assert.True(result.DamageMultiplier > 1.0f);
    }

    [Fact]
    public void GetMovementModifier_Normal_Returns1()
    {
        Assert.Equal(1.0f, EnvironmentalCombatSystem.GetMovementModifier(EnvironmentalCombatSystem.SurfaceType.Normal));
    }

    [Fact]
    public void GetMovementModifier_Ice_ReturnsHigherThan1()
    {
        Assert.True(EnvironmentalCombatSystem.GetMovementModifier(EnvironmentalCombatSystem.SurfaceType.Ice) > 1.0f);
    }

    [Fact]
    public void GetSurfaceDamage_Fire_ReturnsPositive()
    {
        Assert.True(EnvironmentalCombatSystem.GetSurfaceDamage(EnvironmentalCombatSystem.SurfaceType.Fire) > 0);
    }

    [Fact]
    public void GetSurfaceDamage_Normal_ReturnsZero()
    {
        Assert.Equal(0, EnvironmentalCombatSystem.GetSurfaceDamage(EnvironmentalCombatSystem.SurfaceType.Normal));
    }

    [Fact]
    public void GetSurfaceDuration_Water_LongestDuration()
    {
        int waterDuration = EnvironmentalCombatSystem.GetSurfaceDuration(EnvironmentalCombatSystem.SurfaceType.Water);
        int fireDuration = EnvironmentalCombatSystem.GetSurfaceDuration(EnvironmentalCombatSystem.SurfaceType.Fire);
        Assert.True(waterDuration > fireDuration);
    }
}
