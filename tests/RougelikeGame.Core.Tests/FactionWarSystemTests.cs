using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// FactionWarSystem（陣営・派閥間戦争イベントシステム）のテスト
/// </summary>
public class FactionWarSystemTests
{
    // --- 戦争開始 ---

    [Fact]
    public void StartWar_AddsToActiveWars()
    {
        var system = new FactionWarSystem();

        var war = system.StartWar("war1", "森林戦争", TerritoryId.Forest, TerritoryId.Mountain, 1);

        Assert.Single(system.ActiveWars);
        Assert.Equal("war1", war.WarId);
        Assert.Equal(WarPhase.Tension, war.Phase);
    }

    [Fact]
    public void StartWar_InitialAlignment_IsNeutral()
    {
        var system = new FactionWarSystem();

        var war = system.StartWar("w1", "テスト", TerritoryId.Capital, TerritoryId.Coast, 1);

        Assert.Equal(FactionAlignment.Neutral, war.PlayerAlignment);
    }

    // --- フェーズ進行 ---

    [Fact]
    public void AdvancePhase_TensionToSkirmish()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        var advanced = system.AdvancePhase("w1", 10);

        Assert.NotNull(advanced);
        Assert.Equal(WarPhase.Skirmish, advanced!.Phase);
        Assert.Equal(9, advanced.Duration); // 10 - 1 = 9
    }

    [Fact]
    public void AdvancePhase_FullProgression()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 0);

        system.AdvancePhase("w1", 5);  // Tension → Skirmish
        system.AdvancePhase("w1", 10); // Skirmish → Battle
        system.AdvancePhase("w1", 15); // Battle → Aftermath
        var final = system.AdvancePhase("w1", 20); // Aftermath → Peace

        Assert.NotNull(final);
        Assert.Equal(WarPhase.Peace, final!.Phase);
    }

    [Fact]
    public void AdvancePhase_InvalidWarId_ReturnsNull()
    {
        var system = new FactionWarSystem();

        Assert.Null(system.AdvancePhase("nonexistent", 10));
    }

    // --- 陣営選択 ---

    [Fact]
    public void ChooseAlignment_Faction1_GivesReputation()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        var result = system.ChooseAlignment("w1", FactionAlignment.Faction1);

        Assert.NotNull(result);
        Assert.Equal(20, result!.ReputationChange);
        Assert.Equal(0, result.GoldReward);
    }

    [Fact]
    public void ChooseAlignment_Mercenary_GivesGoldAndPenalty()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        var result = system.ChooseAlignment("w1", FactionAlignment.Mercenary);

        Assert.NotNull(result);
        Assert.Equal(-5, result!.ReputationChange);
        Assert.Equal(500, result.GoldReward);
    }

    [Fact]
    public void ChooseAlignment_Neutral_NoRewardOrPenalty()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        var result = system.ChooseAlignment("w1", FactionAlignment.Neutral);

        Assert.NotNull(result);
        Assert.Equal(0, result!.ReputationChange);
        Assert.Equal(0, result.GoldReward);
    }

    [Fact]
    public void ChooseAlignment_InvalidWarId_ReturnsNull()
    {
        var system = new FactionWarSystem();

        Assert.Null(system.ChooseAlignment("nonexistent", FactionAlignment.Faction1));
    }

    // --- 戦争終結 ---

    [Fact]
    public void ResolveWar_RemovesFromActive_AddsToHistory()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        var outcome = system.ResolveWar("w1", TerritoryId.Forest, 30);

        Assert.NotNull(outcome);
        Assert.Empty(system.ActiveWars);
        Assert.Single(system.WarHistory);
        Assert.Equal(TerritoryId.Forest, outcome!.Winner);
        Assert.Equal(TerritoryId.Mountain, outcome.Loser);
    }

    [Fact]
    public void ResolveWar_InvalidWarId_ReturnsNull()
    {
        var system = new FactionWarSystem();

        Assert.Null(system.ResolveWar("nonexistent", TerritoryId.Capital, 10));
    }

    // --- 領地検索 ---

    [Fact]
    public void GetWarsInvolving_FindsAttackerAndDefender()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "戦争A", TerritoryId.Forest, TerritoryId.Mountain, 1);
        system.StartWar("w2", "戦争B", TerritoryId.Coast, TerritoryId.Forest, 1);

        var forestWars = system.GetWarsInvolving(TerritoryId.Forest);

        Assert.Equal(2, forestWars.Count);
    }

    [Fact]
    public void GetWarsInvolving_NoMatch_ReturnsEmpty()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);

        Assert.Empty(system.GetWarsInvolving(TerritoryId.Capital));
    }

    // --- リセット ---

    [Fact]
    public void Reset_ClearsActiveWarsAndHistory()
    {
        var system = new FactionWarSystem();
        system.StartWar("w1", "テスト", TerritoryId.Forest, TerritoryId.Mountain, 1);
        system.ResolveWar("w1", TerritoryId.Forest, 10);
        system.StartWar("w2", "テスト2", TerritoryId.Coast, TerritoryId.Capital, 5);

        system.Reset();

        Assert.Empty(system.ActiveWars);
        Assert.Empty(system.WarHistory);
    }

    // --- 静的ヘルパー ---

    [Theory]
    [InlineData(WarPhase.Tension, "両国の間に緊張が走っている。戦争の予感...")]
    [InlineData(WarPhase.Battle, "全面戦争が勃発！各地で激しい戦闘が行われている")]
    [InlineData(WarPhase.Peace, "和平が成立した。新しい秩序が生まれつつある")]
    public void GetPhaseDescription_ReturnsJapanese(WarPhase phase, string expected)
    {
        Assert.Equal(expected, FactionWarSystem.GetPhaseDescription(phase));
    }

    [Theory]
    [InlineData(FactionAlignment.Faction1, "攻撃側陣営")]
    [InlineData(FactionAlignment.Faction2, "防衛側陣営")]
    [InlineData(FactionAlignment.Neutral, "中立")]
    [InlineData(FactionAlignment.Mercenary, "傭兵")]
    public void GetAlignmentName_ReturnsJapanese(FactionAlignment alignment, string expected)
    {
        Assert.Equal(expected, FactionWarSystem.GetAlignmentName(alignment));
    }
}
