using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// StealSystem テスト
/// - 盗み成功率計算テスト
/// - 盗み試行テスト（成功・失敗・発覚）
/// </summary>
public class StealSystemTests
{
    private class FixedRandom : IRandomProvider
    {
        private readonly int _intVal;
        public FixedRandom(int intVal) { _intVal = intVal; }
        public int Next(int maxValue) => Math.Clamp(_intVal, 0, maxValue - 1);
        public int Next(int minValue, int maxValue) => Math.Clamp(_intVal, minValue, maxValue - 1);
        public double NextDouble() => 0.5;
    }

    [Theory]
    [InlineData(10, 5, 5, 0, 20f)]   // DEX10 * 2 + 0 - 0 = 20
    [InlineData(30, 10, 10, 0, 60f)]  // DEX30 * 2 + 0 - 0 = 60
    [InlineData(50, 10, 10, 0, 95f)]  // DEX50 * 2 = 100 → clamped to 95
    [InlineData(1, 1, 20, 0, 5f)]     // Very low → clamped to 5
    [InlineData(10, 5, 5, 10, 30f)]   // With skill bonus
    public void CalculateStealChance_ReturnsClampedValue(int dex, int playerLv, int targetLv, int skill, float expected)
    {
        float result = StealSystem.CalculateStealChance(dex, playerLv, targetLv, skill);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateStealChance_HigherTargetLevel_ReducesChance()
    {
        float sameLv = StealSystem.CalculateStealChance(15, 5, 5, 0);
        float higherTarget = StealSystem.CalculateStealChance(15, 5, 10, 0);
        Assert.True(sameLv > higherTarget);
    }

    [Fact]
    public void AttemptSteal_SuccessWithGold_ReturnsGoldResult()
    {
        // roll=0 → 常に成功（chance >= 5）
        var rng = new FixedRandom(0);
        var result = StealSystem.AttemptSteal(20, 5, 5, 100, false, 0, rng);
        Assert.True(result.Success);
        Assert.True(result.StolenGold > 0);
        Assert.False(result.StolenItem);
        Assert.False(result.Detected);
    }

    [Fact]
    public void AttemptSteal_SuccessWithItem_WhenNoGold()
    {
        var rng = new FixedRandom(0);
        var result = StealSystem.AttemptSteal(20, 5, 5, 0, true, 0, rng);
        Assert.True(result.Success);
        Assert.Equal(0, result.StolenGold);
        Assert.True(result.StolenItem);
    }

    [Fact]
    public void AttemptSteal_NothingToSteal()
    {
        var rng = new FixedRandom(0);
        var result = StealSystem.AttemptSteal(20, 5, 5, 0, false, 0, rng);
        Assert.False(result.Success);
        Assert.Contains("盗めるものがなかった", result.Message);
    }

    [Fact]
    public void AttemptSteal_Failure_NotDetected()
    {
        // chance = DEX1*2 + 0 - 0 = 2 → clamped to 5. roll=10 → fail (10 >= 5), not detected (10 < 5+20=25)
        var rng = new FixedRandom(10);
        var result = StealSystem.AttemptSteal(1, 1, 1, 100, false, 0, rng);
        Assert.False(result.Success);
        Assert.False(result.Detected);
    }

    [Fact]
    public void AttemptSteal_Failure_Detected()
    {
        // chance = 5 (clamped min). roll=99 → fail (99 >= 5), detected (99 >= 5+20=25)
        var rng = new FixedRandom(99);
        var result = StealSystem.AttemptSteal(1, 1, 1, 100, false, 0, rng);
        Assert.False(result.Success);
        Assert.True(result.Detected);
        Assert.Contains("見つかった", result.Message);
    }
}
