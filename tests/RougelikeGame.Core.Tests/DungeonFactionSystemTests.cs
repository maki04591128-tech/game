using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class DungeonFactionSystemTests
{
    [Fact]
    public void GetHostility_SameRace_ReturnsZero()
    {
        float hostility = DungeonFactionSystem.GetHostility(MonsterRace.Beast, MonsterRace.Beast);
        Assert.Equal(0.0f, hostility);
    }

    [Fact]
    public void AreHostile_UndeadAndHumanoid_ReturnsTrue()
    {
        Assert.True(DungeonFactionSystem.AreHostile(MonsterRace.Undead, MonsterRace.Humanoid));
    }

    [Fact]
    public void AreAllied_SameRace_ReturnsTrue()
    {
        Assert.True(DungeonFactionSystem.AreAllied(MonsterRace.Beast, MonsterRace.Beast));
    }

    [Fact]
    public void AreHostile_SameRace_ReturnsFalse()
    {
        Assert.False(DungeonFactionSystem.AreHostile(MonsterRace.Beast, MonsterRace.Beast));
    }

    [Fact]
    public void GetHostileRaces_ReturnsNonEmpty()
    {
        var hostileRaces = DungeonFactionSystem.GetHostileRaces(MonsterRace.Humanoid);
        Assert.True(hostileRaces.Count > 0);
    }

    [Fact]
    public void GetAllRelations_ReturnsNonEmpty()
    {
        var relations = DungeonFactionSystem.GetAllRelations();
        Assert.True(relations.Count > 0);
    }

    [Fact]
    public void GetHostility_IsSymmetric()
    {
        float ab = DungeonFactionSystem.GetHostility(MonsterRace.Undead, MonsterRace.Humanoid);
        float ba = DungeonFactionSystem.GetHostility(MonsterRace.Humanoid, MonsterRace.Undead);
        Assert.Equal(ab, ba);
    }
}
