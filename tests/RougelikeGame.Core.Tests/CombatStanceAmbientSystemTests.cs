using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 戦闘スタンス＋環境音システムテスト
/// テスト数: 16件
/// </summary>
public class CombatStanceAmbientSystemTests
{
    // ============================================================
    // CombatStanceSystem テスト
    // ============================================================

    [Theory]
    [InlineData(CombatStance.Aggressive, 1.25f)]
    [InlineData(CombatStance.Defensive, 0.75f)]
    [InlineData(CombatStance.Balanced, 1.0f)]
    public void CombatStance_GetAttackModifier_ReturnsCorrectValue(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetAttackModifier(stance));
    }

    [Theory]
    [InlineData(CombatStance.Aggressive, 0.8f)]
    [InlineData(CombatStance.Defensive, 1.3f)]
    [InlineData(CombatStance.Balanced, 1.0f)]
    public void CombatStance_GetDefenseModifier_ReturnsCorrectValue(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetDefenseModifier(stance));
    }

    [Theory]
    [InlineData(CombatStance.Aggressive, -0.1f)]
    [InlineData(CombatStance.Defensive, 0.15f)]
    [InlineData(CombatStance.Balanced, 0f)]
    public void CombatStance_GetEvasionModifier_ReturnsCorrectValue(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetEvasionModifier(stance));
    }

    [Theory]
    [InlineData(CombatStance.Aggressive, 0.1f)]
    [InlineData(CombatStance.Defensive, -0.05f)]
    [InlineData(CombatStance.Balanced, 0f)]
    public void CombatStance_GetCriticalModifier_ReturnsCorrectValue(CombatStance stance, float expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetCriticalModifier(stance));
    }

    [Theory]
    [InlineData(CombatStance.Aggressive, "攻撃型")]
    [InlineData(CombatStance.Defensive, "防御型")]
    [InlineData(CombatStance.Balanced, "バランス型")]
    public void CombatStance_GetStanceName_ReturnsCorrectName(CombatStance stance, string expected)
    {
        Assert.Equal(expected, CombatStanceSystem.GetStanceName(stance));
    }

    [Fact]
    public void CombatStance_GetStanceDescription_IsNotEmpty()
    {
        Assert.NotEmpty(CombatStanceSystem.GetStanceDescription(CombatStance.Aggressive));
        Assert.NotEmpty(CombatStanceSystem.GetStanceDescription(CombatStance.Defensive));
        Assert.NotEmpty(CombatStanceSystem.GetStanceDescription(CombatStance.Balanced));
    }

    // ============================================================
    // AmbientSoundSystem テスト
    // ============================================================

    [Theory]
    [InlineData(TerritoryId.Capital, AmbientSoundType.Town)]
    [InlineData(TerritoryId.Forest, AmbientSoundType.Forest)]
    [InlineData(TerritoryId.Mountain, AmbientSoundType.Mountain)]
    [InlineData(TerritoryId.Coast, AmbientSoundType.Coast)]
    [InlineData(TerritoryId.Southern, AmbientSoundType.Desert)]
    [InlineData(TerritoryId.Frontier, AmbientSoundType.Frontier)]
    public void AmbientSound_GetAmbientForTerritory_ReturnsCorrectType(TerritoryId territory, AmbientSoundType expected)
    {
        Assert.Equal(expected, AmbientSoundSystem.GetAmbientForTerritory(territory));
    }

    [Fact]
    public void AmbientSound_GetAmbientForDungeon_BossFloor_ReturnsBossBattle()
    {
        Assert.Equal(AmbientSoundType.BossBattle, AmbientSoundSystem.GetAmbientForDungeon(5, true));
    }

    [Fact]
    public void AmbientSound_GetAmbientForDungeon_NormalFloor_ReturnsDungeon()
    {
        Assert.Equal(AmbientSoundType.Dungeon, AmbientSoundSystem.GetAmbientForDungeon(3, false));
    }

    [Fact]
    public void AmbientSound_GetDefaultVolume_SilenceIsZero()
    {
        Assert.Equal(0.0f, AmbientSoundSystem.GetDefaultVolume(AmbientSoundType.Silence));
    }

    [Fact]
    public void AmbientSound_GetDefaultVolume_NonSilenceIsPositive()
    {
        Assert.True(AmbientSoundSystem.GetDefaultVolume(AmbientSoundType.Town) > 0);
        Assert.True(AmbientSoundSystem.GetDefaultVolume(AmbientSoundType.Forest) > 0);
    }

    [Fact]
    public void AmbientSound_GetSoundName_ReturnsNonEmpty()
    {
        foreach (AmbientSoundType type in Enum.GetValues<AmbientSoundType>())
        {
            Assert.NotEmpty(AmbientSoundSystem.GetSoundName(type));
        }
    }

    [Fact]
    public void AmbientSound_CreateEvent_SetsCorrectValues()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Forest);
        Assert.Equal(AmbientSoundType.Forest, evt.Type);
        Assert.Equal(0.5f, evt.Volume);
        Assert.True(evt.ShouldLoop);
    }

    [Fact]
    public void AmbientSound_CreateEvent_Silence_DoesNotLoop()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Silence);
        Assert.False(evt.ShouldLoop);
        Assert.Equal(0.0f, evt.Volume);
    }

    // ============================================================
    // MultiSlotSaveSystem テスト
    // ============================================================

    [Fact]
    public void MultiSlotSave_Initial_AllSlotsEmpty()
    {
        var system = new MultiSlotSaveSystem();
        Assert.Equal(5, system.GetAllSlots().Count);
        Assert.Equal(5, system.GetEmptySlotCount());
    }

    [Fact]
    public void MultiSlotSave_SaveToSlot_FillsSlot()
    {
        var system = new MultiSlotSaveSystem();
        Assert.True(system.SaveToSlot(1, "勇者", 10, "王都"));
        var slot = system.GetSlot(1);
        Assert.False(slot.IsEmpty);
        Assert.Equal("勇者", slot.CharacterName);
        Assert.Equal(10, slot.Level);
        Assert.Equal(4, system.GetEmptySlotCount());
    }

    [Fact]
    public void MultiSlotSave_SaveToSlot_InvalidSlot_ReturnsFalse()
    {
        var system = new MultiSlotSaveSystem();
        Assert.False(system.SaveToSlot(0, "テスト", 1, "テスト"));
        Assert.False(system.SaveToSlot(6, "テスト", 1, "テスト"));
    }

    [Fact]
    public void MultiSlotSave_ClearSlot_EmptiesSlot()
    {
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "勇者", 10, "王都");
        system.ClearSlot(1);
        Assert.True(system.GetSlot(1).IsEmpty);
    }

    [Fact]
    public void MultiSlotSave_GetOldestSlot_ReturnsCorrectSlot()
    {
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(3, "キャラ3", 5, "森");
        System.Threading.Thread.Sleep(10);
        system.SaveToSlot(1, "キャラ1", 10, "王都");
        Assert.Equal(3, system.GetOldestSlot());
    }

    [Fact]
    public void MultiSlotSave_GetOldestSlot_NoSaves_Returns1()
    {
        var system = new MultiSlotSaveSystem();
        Assert.Equal(1, system.GetOldestSlot());
    }
}
