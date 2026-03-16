using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class NpcMemorySystemTests
{
    [Fact]
    public void RecordAction_AddsMemory()
    {
        var system = new NpcMemorySystem();
        system.RecordAction("npc1", "helped", 10, 100);
        Assert.Single(system.Memories);
    }

    [Fact]
    public void CalculateImpression_SumsImpact()
    {
        var system = new NpcMemorySystem();
        system.RecordAction("npc1", "helped", 10, 100);
        system.RecordAction("npc1", "traded", 5, 200);
        Assert.Equal(15, system.CalculateImpression("npc1"));
    }

    [Fact]
    public void GenerateRumor_AddsRumor()
    {
        var system = new NpcMemorySystem();
        system.GenerateRumor(RumorType.Heroic, "英雄が現れた", "North");
        Assert.Single(system.Rumors);
    }

    [Fact]
    public void SpreadRumors_IncrementsCount()
    {
        var system = new NpcMemorySystem();
        system.GenerateRumor(RumorType.Villainous, "悪漢が出た", "South");
        system.SpreadRumors();
        Assert.Equal(1, system.Rumors[0].SpreadCount);
    }

    [Theory]
    [InlineData(RumorType.Heroic, "英雄の噂")]
    [InlineData(RumorType.Villainous, "悪漢の噂")]
    public void GetRumorTypeName_ReturnsJapanese(RumorType type, string expected)
    {
        Assert.Equal(expected, NpcMemorySystem.GetRumorTypeName(type));
    }
}
