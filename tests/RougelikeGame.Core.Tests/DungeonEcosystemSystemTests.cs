using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// DungeonEcosystemSystem（ダンジョン生態系・食物連鎖システム）のテスト
/// </summary>
public class DungeonEcosystemSystemTests
{
    // --- 捕食関係の登録・確認 ---

    [Fact]
    public void RegisterRelation_AddsRelation()
    {
        var system = new DungeonEcosystemSystem();

        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);

        Assert.Single(system.Relations);
        Assert.Equal(MonsterRace.Dragon, system.Relations[0].Predator);
    }

    [Fact]
    public void RegisterRelation_ChanceClamped_Over100()
    {
        var system = new DungeonEcosystemSystem();

        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast, 150);

        Assert.Equal(100, system.Relations[0].PredationChance);
    }

    [Fact]
    public void RegisterRelation_ChanceClamped_BelowZero()
    {
        var system = new DungeonEcosystemSystem();

        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast, -10);

        Assert.Equal(0, system.Relations[0].PredationChance);
    }

    [Fact]
    public void HasPredatorRelation_Registered_ReturnsTrue()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);

        Assert.True(system.HasPredatorRelation(MonsterRace.Dragon, MonsterRace.Beast));
    }

    [Fact]
    public void HasPredatorRelation_Reversed_ReturnsFalse()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);

        // 逆方向は登録されていない
        Assert.False(system.HasPredatorRelation(MonsterRace.Beast, MonsterRace.Dragon));
    }

    // --- インタラクション処理 ---

    [Fact]
    public void ProcessInteraction_WithRelation_ReturnsPredationEvent()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);

        var evt = system.ProcessInteraction("d1", MonsterRace.Dragon, "b1", MonsterRace.Beast, 1, 10);

        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.Predation, evt!.Type);
        Assert.Single(system.Events);
    }

    [Fact]
    public void ProcessInteraction_SameRace_ReturnsTerritoryFight()
    {
        var system = new DungeonEcosystemSystem();

        var evt = system.ProcessInteraction("b1", MonsterRace.Beast, "b2", MonsterRace.Beast, 1, 5);

        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.TerritoryFight, evt!.Type);
    }

    [Fact]
    public void ProcessInteraction_NoRelationDifferentRace_ReturnsNull()
    {
        var system = new DungeonEcosystemSystem();

        var evt = system.ProcessInteraction("a1", MonsterRace.Insect, "b1", MonsterRace.Plant, 1, 1);

        Assert.Null(evt);
    }

    // --- 戦闘痕跡 ---

    [Fact]
    public void AddBattleTrace_AppearsInTraces()
    {
        var system = new DungeonEcosystemSystem();

        system.AddBattleTrace(5, 10, 3, 7, "竜の爪痕", 100);

        Assert.Single(system.Traces);
        Assert.Equal(7, system.Traces[0].DangerLevel);
    }

    [Fact]
    public void GetTracesOnFloor_FiltersCorrectly()
    {
        var system = new DungeonEcosystemSystem();
        system.AddBattleTrace(0, 0, 1, 3, "痕跡A", 1);
        system.AddBattleTrace(0, 0, 2, 5, "痕跡B", 2);
        system.AddBattleTrace(0, 0, 1, 8, "痕跡C", 3);

        var floor1 = system.GetTracesOnFloor(1);

        Assert.Equal(2, floor1.Count);
    }

    [Fact]
    public void EstimateDangerLevel_CalculatesAverage()
    {
        var system = new DungeonEcosystemSystem();
        system.AddBattleTrace(0, 0, 1, 4, "A", 1);
        system.AddBattleTrace(0, 0, 1, 8, "B", 2);

        // 平均 (4+8)/2 = 6
        Assert.Equal(6, system.EstimateDangerLevel(1));
    }

    [Fact]
    public void EstimateDangerLevel_NoTraces_ReturnsZero()
    {
        var system = new DungeonEcosystemSystem();

        Assert.Equal(0, system.EstimateDangerLevel(1));
    }

    // --- リセット ---

    [Fact]
    public void Reset_ClearsEventsAndTraces_KeepsRelations()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);
        system.ProcessInteraction("d1", MonsterRace.Dragon, "b1", MonsterRace.Beast, 1, 10);
        system.AddBattleTrace(0, 0, 1, 5, "痕跡", 1);

        system.Reset();

        Assert.Empty(system.Events);
        Assert.Empty(system.Traces);
        Assert.Single(system.Relations); // 捕食関係は保持
    }

    // --- 静的ヘルパー ---

    [Theory]
    [InlineData(EcosystemEventType.Predation, "捕食")]
    [InlineData(EcosystemEventType.TerritoryFight, "縄張り争い")]
    [InlineData(EcosystemEventType.Symbiosis, "共生")]
    [InlineData(EcosystemEventType.Scavenging, "漁り")]
    public void GetEventTypeName_ReturnsJapanese(EcosystemEventType type, string expected)
    {
        Assert.Equal(expected, DungeonEcosystemSystem.GetEventTypeName(type));
    }
}
