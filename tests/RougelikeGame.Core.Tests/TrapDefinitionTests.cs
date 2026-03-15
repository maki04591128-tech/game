using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 罠定義システムのテスト
/// </summary>
public class TrapDefinitionTests
{
    /// <summary>テスト用固定乱数プロバイダー</summary>
    private class FixedRandomProvider : IRandomProvider
    {
        private readonly int _value;
        public FixedRandomProvider(int value) => _value = value;
        public int Next(int maxValue) => Math.Min(_value, maxValue - 1);
        public int Next(int minValue, int maxValue) => Math.Min(Math.Max(_value, minValue), maxValue - 1);
        public double NextDouble() => 0.5;
    }

    [Theory]
    [InlineData(TrapType.Poison)]
    [InlineData(TrapType.PitFall)]
    [InlineData(TrapType.Teleport)]
    [InlineData(TrapType.Arrow)]
    [InlineData(TrapType.Alarm)]
    [InlineData(TrapType.Fire)]
    [InlineData(TrapType.Sleep)]
    [InlineData(TrapType.Confusion)]
    public void Get_AllTrapTypes_ReturnValidDefinition(TrapType type)
    {
        var def = TrapDefinition.Get(type);

        Assert.NotNull(def);
        Assert.Equal(type, def.Type);
        Assert.False(string.IsNullOrEmpty(def.Name));
        Assert.True(def.DetectionDifficulty > 0);
        Assert.True(def.DisarmDifficulty > 0);
    }

    [Fact]
    public void AllTraps_ContainsAllTrapTypes()
    {
        Assert.Equal(8, TrapDefinition.AllTraps.Length);

        var types = TrapDefinition.AllTraps.Select(t => t.Type).ToHashSet();
        foreach (TrapType type in Enum.GetValues<TrapType>())
        {
            Assert.Contains(type, types);
        }
    }

    [Fact]
    public void CalculateDamage_Floor1_ReturnsBaseDamage()
    {
        var arrow = TrapDefinition.Get(TrapType.Arrow);
        Assert.Equal(arrow.BaseDamage, arrow.CalculateDamage(1));
    }

    [Fact]
    public void CalculateDamage_HigherFloor_IncreasesDamage()
    {
        var arrow = TrapDefinition.Get(TrapType.Arrow);
        int floor5Damage = arrow.CalculateDamage(5);
        int expected = arrow.BaseDamage + arrow.DamagePerFloor * 4;
        Assert.Equal(expected, floor5Damage);
        Assert.True(floor5Damage > arrow.BaseDamage);
    }

    [Fact]
    public void CanDetect_HighPerception_Succeeds()
    {
        var arrow = TrapDefinition.Get(TrapType.Arrow); // DetectionDifficulty = 10
        // PER 15 + ランダム(0) = 15 >= 10 → 成功
        var random = new FixedRandomProvider(0);
        Assert.True(arrow.CanDetect(15, random));
    }

    [Fact]
    public void CanDetect_LowPerception_Fails()
    {
        var teleport = TrapDefinition.Get(TrapType.Teleport); // DetectionDifficulty = 14
        // PER 3 + ランダム(0) = 3 < 14 → 失敗
        var random = new FixedRandomProvider(0);
        Assert.False(teleport.CanDetect(3, random));
    }

    [Fact]
    public void CanDisarm_HighDexterity_Succeeds()
    {
        var alarm = TrapDefinition.Get(TrapType.Alarm); // DisarmDifficulty = 6
        // DEX 10 + ランダム(0) = 10 >= 6 → 成功
        var random = new FixedRandomProvider(0);
        Assert.True(alarm.CanDisarm(10, random));
    }

    [Fact]
    public void CanDisarm_LowDexterity_Fails()
    {
        var pitfall = TrapDefinition.Get(TrapType.PitFall); // DisarmDifficulty = 15
        // DEX 3 + ランダム(0) = 3 < 15 → 失敗
        var random = new FixedRandomProvider(0);
        Assert.False(pitfall.CanDisarm(3, random));
    }

    [Fact]
    public void PoisonTrap_HasStatusEffect()
    {
        var poison = TrapDefinition.Get(TrapType.Poison);
        Assert.NotNull(poison.StatusEffect);
        Assert.Equal(StatusEffectType.Poison, poison.StatusEffect);
        Assert.True(poison.StatusDuration > 0);
    }

    [Fact]
    public void TeleportTrap_HasNoDamage()
    {
        var teleport = TrapDefinition.Get(TrapType.Teleport);
        Assert.Equal(0, teleport.BaseDamage);
        Assert.Equal(0, teleport.DamagePerFloor);
    }

    [Fact]
    public void FireTrap_HasDamageAndStatusEffect()
    {
        var fire = TrapDefinition.Get(TrapType.Fire);
        Assert.True(fire.BaseDamage > 0);
        Assert.Equal(StatusEffectType.Burn, fire.StatusEffect);
        Assert.True(fire.DamagePerFloor > 0);
    }
}
