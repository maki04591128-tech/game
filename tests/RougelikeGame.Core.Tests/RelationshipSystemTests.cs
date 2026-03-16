using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class RelationshipSystemTests
{
    [Fact]
    public void SetAndGetRelation_ReturnsCorrectValue()
    {
        var system = new RelationshipSystem();
        system.SetRelation(RelationshipType.Personal, "NPC1", "Player", 50);
        Assert.Equal(50, system.GetRelation(RelationshipType.Personal, "NPC1", "Player"));
    }

    [Fact]
    public void GetRelation_Default_Zero()
    {
        var system = new RelationshipSystem();
        Assert.Equal(0, system.GetRelation(RelationshipType.Racial, "Human", "Elf"));
    }

    [Fact]
    public void ModifyRelation_ChangesValue()
    {
        var system = new RelationshipSystem();
        system.SetRelation(RelationshipType.Territorial, "North", "South", 30);
        system.ModifyRelation(RelationshipType.Territorial, "North", "South", 20);
        Assert.Equal(50, system.GetRelation(RelationshipType.Territorial, "North", "South"));
    }

    [Fact]
    public void SetRelation_ClampsToRange()
    {
        var system = new RelationshipSystem();
        system.SetRelation(RelationshipType.Personal, "A", "B", 200);
        Assert.Equal(100, system.GetRelation(RelationshipType.Personal, "A", "B"));
    }

    [Theory]
    [InlineData(80, "盟友")]
    [InlineData(50, "友好")]
    [InlineData(0, "中立")]
    [InlineData(-50, "敵対")]
    [InlineData(-80, "宿敵")]
    public void GetRelationName_ReturnsCorrectName(int value, string expected)
    {
        Assert.Equal(expected, RelationshipSystem.GetRelationName(value));
    }

    [Fact]
    public void GetShopDiscount_HighRelation_ReturnsPositive()
    {
        Assert.True(RelationshipSystem.GetShopDiscount(80) > 0);
    }
}
