using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class MonsterRaceSystemTests
{
    [Theory]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Humanoid)]
    [InlineData(MonsterRace.Amorphous)]
    [InlineData(MonsterRace.Undead)]
    [InlineData(MonsterRace.Demon)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Plant)]
    [InlineData(MonsterRace.Insect)]
    [InlineData(MonsterRace.Spirit)]
    [InlineData(MonsterRace.Construct)]
    public void GetTraits_ReturnsTraitsForAllRaces(MonsterRace race)
    {
        var traits = MonsterRaceSystem.GetTraits(race);

        Assert.NotNull(traits);
        Assert.Equal(race, traits.Race);
    }

    [Theory]
    [InlineData(MonsterRace.Beast, Element.Fire, 1.5f)]
    [InlineData(MonsterRace.Beast, Element.Water, 1.0f)]
    [InlineData(MonsterRace.Undead, Element.Light, 1.5f)]
    [InlineData(MonsterRace.Undead, Element.Holy, 1.5f)]
    [InlineData(MonsterRace.Undead, Element.Poison, 0.5f)]
    [InlineData(MonsterRace.Undead, Element.Dark, 0.5f)]
    [InlineData(MonsterRace.Demon, Element.Holy, 1.5f)]
    [InlineData(MonsterRace.Demon, Element.Dark, 0.5f)]
    [InlineData(MonsterRace.Demon, Element.Curse, 0.5f)]
    [InlineData(MonsterRace.Dragon, Element.Fire, 0.5f)]
    [InlineData(MonsterRace.Dragon, Element.Ice, 0.5f)]
    [InlineData(MonsterRace.Dragon, Element.Lightning, 0.5f)]
    [InlineData(MonsterRace.Plant, Element.Fire, 1.5f)]
    [InlineData(MonsterRace.Plant, Element.Ice, 1.5f)]
    [InlineData(MonsterRace.Plant, Element.Earth, 0.5f)]
    [InlineData(MonsterRace.Plant, Element.Water, 0.5f)]
    [InlineData(MonsterRace.Insect, Element.Fire, 1.5f)]
    [InlineData(MonsterRace.Construct, Element.Lightning, 1.5f)]
    [InlineData(MonsterRace.Construct, Element.Poison, 0.5f)]
    [InlineData(MonsterRace.Humanoid, Element.Fire, 1.0f)]
    public void GetElementalMultiplier_ReturnsCorrectValues(MonsterRace race, Element element, float expected)
    {
        var result = MonsterRaceSystem.GetElementalMultiplier(race, element);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(MonsterRace.Undead, StatusEffectType.Poison, true)]
    [InlineData(MonsterRace.Undead, StatusEffectType.Sleep, true)]
    [InlineData(MonsterRace.Undead, StatusEffectType.Burn, false)]
    [InlineData(MonsterRace.Insect, StatusEffectType.Confusion, true)]
    [InlineData(MonsterRace.Insect, StatusEffectType.Fear, true)]
    [InlineData(MonsterRace.Insect, StatusEffectType.Charm, true)]
    [InlineData(MonsterRace.Insect, StatusEffectType.Poison, false)]
    [InlineData(MonsterRace.Construct, StatusEffectType.Poison, true)]
    [InlineData(MonsterRace.Construct, StatusEffectType.Sleep, true)]
    [InlineData(MonsterRace.Construct, StatusEffectType.InstantDeath, true)]
    [InlineData(MonsterRace.Construct, StatusEffectType.Petrification, true)]
    [InlineData(MonsterRace.Humanoid, StatusEffectType.Poison, false)]
    [InlineData(MonsterRace.Beast, StatusEffectType.Sleep, false)]
    public void IsStatusEffectImmune_ReturnsCorrectValues(MonsterRace race, StatusEffectType effect, bool expected)
    {
        var result = MonsterRaceSystem.IsStatusEffectImmune(race, effect);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(MonsterRace.Amorphous, AttackType.Slash, 0.5f)]
    [InlineData(MonsterRace.Amorphous, AttackType.Pierce, 0.5f)]
    [InlineData(MonsterRace.Amorphous, AttackType.Blunt, 1.0f)]
    [InlineData(MonsterRace.Spirit, AttackType.Slash, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Pierce, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Blunt, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Unarmed, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Magic, 1.0f)]
    [InlineData(MonsterRace.Humanoid, AttackType.Slash, 1.0f)]
    [InlineData(MonsterRace.Beast, AttackType.Pierce, 1.0f)]
    public void GetPhysicalResistance_ReturnsCorrectValues(MonsterRace race, AttackType attackType, float expected)
    {
        var result = MonsterRaceSystem.GetPhysicalResistance(race, attackType);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnemyFactory_SetsRaceFromDefinition()
    {
        var factory = new EnemyFactory();

        var slime = factory.CreateEnemy(EnemyDefinitions.Slime, new Position(0, 0));
        Assert.Equal(MonsterRace.Amorphous, slime.Race);

        var skeleton = factory.CreateEnemy(EnemyDefinitions.Skeleton, new Position(0, 0));
        Assert.Equal(MonsterRace.Undead, skeleton.Race);

        var dragon = factory.CreateEnemy(EnemyDefinitions.FrontierDragon, new Position(0, 0));
        Assert.Equal(MonsterRace.Dragon, dragon.Race);
    }

    [Fact]
    public void AllEnemyDefinitions_HaveValidRace()
    {
        var allEnemies = EnemyDefinitions.GetAllEnemies();

        foreach (var enemy in allEnemies)
        {
            Assert.True(Enum.IsDefined(typeof(MonsterRace), enemy.Race),
                $"{enemy.Name} has invalid MonsterRace: {enemy.Race}");
        }
    }
}
