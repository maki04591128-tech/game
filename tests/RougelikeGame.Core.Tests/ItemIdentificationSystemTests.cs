using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// ItemIdentificationSystem（アイテム鑑定・呪いシステム）のテスト
/// </summary>
public class ItemIdentificationSystemTests
{
    // --- コンストラクタ ---

    [Fact]
    public void Constructor_EmptyState()
    {
        var system = new ItemIdentificationSystem();
        Assert.Empty(system.IdentifiedItems);
        Assert.Empty(system.KnownCurses);
    }

    // --- Identify ---

    [Fact]
    public void Identify_NoCurse_ReturnsIdentifiedState()
    {
        var system = new ItemIdentificationSystem();
        var result = system.Identify("sword_1", "炎の剣");
        Assert.Equal(IdentificationState.Identified, result.State);
        Assert.Equal(CurseType.None, result.Curse);
    }

    [Fact]
    public void Identify_WithCurse_ReturnsCursedState()
    {
        var system = new ItemIdentificationSystem();
        var result = system.Identify("ring_1", "呪いの指輪", CurseType.Major);
        Assert.Equal(IdentificationState.Cursed, result.State);
        Assert.Equal(CurseType.Major, result.Curse);
    }

    [Fact]
    public void Identify_WithCurse_AddsToCursesList()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("ring_1", "呪いの指輪", CurseType.Minor);
        Assert.Contains("ring_1", system.KnownCurses);
    }

    [Fact]
    public void Identify_NoCurse_DoesNotAddToCursesList()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("sword_1", "普通の剣");
        Assert.DoesNotContain("sword_1", system.KnownCurses);
    }

    // --- IsIdentified ---

    [Fact]
    public void IsIdentified_AfterIdentify_ReturnsTrue()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "アイテム名");
        Assert.True(system.IsIdentified("item_1"));
    }

    [Fact]
    public void IsIdentified_NotIdentified_ReturnsFalse()
    {
        var system = new ItemIdentificationSystem();
        Assert.False(system.IsIdentified("unknown_item"));
    }

    // --- GetState ---

    [Fact]
    public void GetState_Unknown_ReturnsUnknown()
    {
        var system = new ItemIdentificationSystem();
        Assert.Equal(IdentificationState.Unknown, system.GetState("missing"));
    }

    [Fact]
    public void GetState_Identified_ReturnsIdentified()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "剣");
        Assert.Equal(IdentificationState.Identified, system.GetState("item_1"));
    }

    // --- CanUnequip ---

    [Fact]
    public void CanUnequip_NoCurse_ReturnsTrue()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "普通の剣");
        Assert.True(system.CanUnequip("item_1"));
    }

    [Fact]
    public void CanUnequip_Cursed_ReturnsFalse()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "呪いの剣", CurseType.Major);
        Assert.False(system.CanUnequip("item_1"));
    }

    [Fact]
    public void CanUnequip_UnknownItem_ReturnsTrue()
    {
        var system = new ItemIdentificationSystem();
        Assert.True(system.CanUnequip("not_registered"));
    }

    // --- RemoveCurse ---

    [Fact]
    public void RemoveCurse_NotFound_ReturnsFalse()
    {
        var system = new ItemIdentificationSystem();
        var result = system.RemoveCurse("missing", 99);
        Assert.False(result.Success);
    }

    [Fact]
    public void RemoveCurse_NoCurse_ReturnsFalse()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "普通の剣");
        var result = system.RemoveCurse("item_1", 99);
        Assert.False(result.Success);
    }

    [Fact]
    public void RemoveCurse_SufficientLevel_Succeeds()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "呪いの剣", CurseType.Minor);
        // Minor呪いはレベル5以上で解除
        var result = system.RemoveCurse("item_1", 5);
        Assert.True(result.Success);
        Assert.Equal(CurseType.Minor, result.OriginalCurse);
        Assert.DoesNotContain("item_1", system.KnownCurses);
    }

    [Fact]
    public void RemoveCurse_InsufficientLevel_Fails()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "呪いの剣", CurseType.Deadly);
        // Deadly呪いはレベル30以上が必要
        var result = system.RemoveCurse("item_1", 10);
        Assert.False(result.Success);
        Assert.Contains("レベル", result.Message);
    }

    // --- GetCursePenalty ---

    [Theory]
    [InlineData(CurseType.None, 0)]
    [InlineData(CurseType.Minor, -2)]
    [InlineData(CurseType.Major, -5)]
    [InlineData(CurseType.Deadly, -10)]
    public void GetCursePenalty_ReturnsExpectedPenalty(CurseType curse, int expected)
    {
        Assert.Equal(expected, ItemIdentificationSystem.GetCursePenalty(curse));
    }

    // --- GetCurseDescription ---

    [Fact]
    public void GetCurseDescription_None_ReturnsNoCurse()
    {
        Assert.Equal("呪いなし", ItemIdentificationSystem.GetCurseDescription(CurseType.None));
    }

    [Fact]
    public void GetCurseDescription_Deadly_ContainsLifeDrain()
    {
        var desc = ItemIdentificationSystem.GetCurseDescription(CurseType.Deadly);
        Assert.Contains("生命力", desc);
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsAllState()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("item_1", "剣", CurseType.Minor);
        system.Identify("item_2", "盾");
        system.Reset();
        Assert.Empty(system.IdentifiedItems);
        Assert.Empty(system.KnownCurses);
    }
}
