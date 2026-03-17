using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.7 拡張状態異常システムのテスト
/// </summary>
public class ExtendedStatusEffectSystemTests
{
    [Fact]
    public void GetAll_Returns12Effects()
    {
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.Equal(12, effects.Count);
    }

    [Fact]
    public void GetById_ValidId_ReturnsEffect()
    {
        var effect = ExtendedStatusEffectSystem.GetById("intoxication");
        Assert.NotNull(effect);
        Assert.Equal("酩酊", effect!.Name);
    }

    [Fact]
    public void GetById_InvalidId_ReturnsNull()
    {
        Assert.Null(ExtendedStatusEffectSystem.GetById("nonexistent"));
    }

    [Fact]
    public void GetBuffs_ReturnsOnlyBuffs()
    {
        var buffs = ExtendedStatusEffectSystem.GetBuffs();
        Assert.All(buffs, b => Assert.True(b.IsBuff));
        Assert.True(buffs.Count >= 3);
    }

    [Fact]
    public void GetDebuffs_ReturnsOnlyDebuffs()
    {
        var debuffs = ExtendedStatusEffectSystem.GetDebuffs();
        Assert.All(debuffs, d => Assert.False(d.IsBuff));
        Assert.True(debuffs.Count >= 9);
    }

    [Fact]
    public void GetStatModifier_ReturnsCorrectValue()
    {
        float hitRate = ExtendedStatusEffectSystem.GetStatModifier("intoxication", "HitRate");
        Assert.Equal(-0.15f, hitRate);
    }

    [Fact]
    public void GetStatModifier_UnknownStat_ReturnsZero()
    {
        Assert.Equal(0, ExtendedStatusEffectSystem.GetStatModifier("intoxication", "NonExistent"));
    }

    [Fact]
    public void GetEffectName_ValidId_ReturnsName()
    {
        Assert.Equal("凍傷", ExtendedStatusEffectSystem.GetEffectName("frostbite"));
    }
}
