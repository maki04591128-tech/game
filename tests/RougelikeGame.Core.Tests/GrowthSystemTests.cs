using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class GrowthSystemTests
{
    [Theory]
    [InlineData(Race.Human, CharacterClass.Fighter)]
    [InlineData(Race.Elf, CharacterClass.Mage)]
    [InlineData(Race.Dwarf, CharacterClass.Knight)]
    public void CalculateLevelUpBonus_ReturnsValidModifier(Race race, CharacterClass cls)
    {
        var bonus = GrowthSystem.CalculateLevelUpBonus(race, cls, 2);
        Assert.True(bonus.Strength >= 0);
        Assert.True(bonus.Vitality >= 0);
        Assert.True(bonus.Intelligence >= 0);
    }

    [Fact]
    public void CalculateHpGrowth_ReturnsPositive()
    {
        int hp = GrowthSystem.CalculateHpGrowth(Race.Human, CharacterClass.Fighter);
        Assert.True(hp > 0);
    }

    [Fact]
    public void CalculateMpGrowth_MageHigherThanFighter()
    {
        int mageGrowth = GrowthSystem.CalculateMpGrowth(Race.Human, CharacterClass.Mage);
        int fighterGrowth = GrowthSystem.CalculateMpGrowth(Race.Human, CharacterClass.Fighter);
        Assert.True(mageGrowth > fighterGrowth);
    }

    [Fact]
    public void RollGrowthWithRandom_HighRate_ReturnsAtLeastOne()
    {
        var random = new Random(42);
        int result = GrowthSystem.RollGrowthWithRandom(1.8, random);
        Assert.True(result >= 1);
    }

    [Fact]
    public void CalculateMaxHp_Level1_ReturnsPositive()
    {
        int hp = GrowthSystem.CalculateMaxHp(Stats.Default, Race.Human, CharacterClass.Fighter, 1);
        Assert.True(hp > 0);
    }

    [Fact]
    public void CalculateMaxMp_Level1_ReturnsPositive()
    {
        int mp = GrowthSystem.CalculateMaxMp(Stats.Default, Race.Human, CharacterClass.Mage, 1);
        Assert.True(mp > 0);
    }

    [Fact]
    public void CalculateMaxHp_HigherLevel_HigherHp()
    {
        int hpLv1 = GrowthSystem.CalculateMaxHp(Stats.Default, Race.Human, CharacterClass.Fighter, 1);
        int hpLv10 = GrowthSystem.CalculateMaxHp(Stats.Default, Race.Human, CharacterClass.Fighter, 10);
        Assert.True(hpLv10 > hpLv1);
    }

    [Fact]
    public void CalculateTotalExpForLevel_IncreasesWithLevel()
    {
        int expLv2 = GrowthSystem.CalculateTotalExpForLevel(2);
        int expLv10 = GrowthSystem.CalculateTotalExpForLevel(10);
        Assert.True(expLv10 > expLv2);
    }
}
