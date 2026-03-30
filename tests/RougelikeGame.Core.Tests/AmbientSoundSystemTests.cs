using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// AmbientSoundSystem（環境音システム）のテスト
/// </summary>
public class AmbientSoundSystemTests
{
    // --- GetAmbientForTerritory ---

    [Theory]
    [InlineData(TerritoryId.Capital, AmbientSoundType.Town)]
    [InlineData(TerritoryId.Forest, AmbientSoundType.Forest)]
    [InlineData(TerritoryId.Mountain, AmbientSoundType.Mountain)]
    [InlineData(TerritoryId.Coast, AmbientSoundType.Coast)]
    [InlineData(TerritoryId.Southern, AmbientSoundType.Desert)]
    [InlineData(TerritoryId.Frontier, AmbientSoundType.Frontier)]
    public void GetAmbientForTerritory_ReturnsCorrectType(TerritoryId territory, AmbientSoundType expected)
    {
        Assert.Equal(expected, AmbientSoundSystem.GetAmbientForTerritory(territory));
    }

    // --- GetAmbientForDungeon ---

    [Fact]
    public void GetAmbientForDungeon_NormalFloor_ReturnsDungeon()
    {
        Assert.Equal(AmbientSoundType.Dungeon, AmbientSoundSystem.GetAmbientForDungeon(3, false));
    }

    [Fact]
    public void GetAmbientForDungeon_BossFloor_ReturnsBossBattle()
    {
        Assert.Equal(AmbientSoundType.BossBattle, AmbientSoundSystem.GetAmbientForDungeon(10, true));
    }

    [Fact]
    public void GetAmbientForDungeon_Floor1Normal_ReturnsDungeon()
    {
        Assert.Equal(AmbientSoundType.Dungeon, AmbientSoundSystem.GetAmbientForDungeon(1, false));
    }

    // --- GetDefaultVolume ---

    [Theory]
    [InlineData(AmbientSoundType.Dungeon, 0.3f)]
    [InlineData(AmbientSoundType.Forest, 0.5f)]
    [InlineData(AmbientSoundType.Mountain, 0.4f)]
    [InlineData(AmbientSoundType.Coast, 0.5f)]
    [InlineData(AmbientSoundType.Desert, 0.3f)]
    [InlineData(AmbientSoundType.Frontier, 0.2f)]
    [InlineData(AmbientSoundType.Town, 0.6f)]
    [InlineData(AmbientSoundType.BossBattle, 0.7f)]
    [InlineData(AmbientSoundType.Silence, 0.0f)]
    public void GetDefaultVolume_ReturnsExpectedValue(AmbientSoundType type, float expected)
    {
        Assert.Equal(expected, AmbientSoundSystem.GetDefaultVolume(type));
    }

    // --- GetSoundName ---

    [Theory]
    [InlineData(AmbientSoundType.Dungeon, "ダンジョン（洞窟の水滴音）")]
    [InlineData(AmbientSoundType.Forest, "森（鳥の声と風音）")]
    [InlineData(AmbientSoundType.Town, "街（人々の声）")]
    [InlineData(AmbientSoundType.BossBattle, "ボス戦（緊迫）")]
    [InlineData(AmbientSoundType.Silence, "静寂")]
    public void GetSoundName_ReturnsJapaneseName(AmbientSoundType type, string expected)
    {
        Assert.Equal(expected, AmbientSoundSystem.GetSoundName(type));
    }

    [Fact]
    public void GetSoundName_AllTypes_ReturnNonEmpty()
    {
        foreach (AmbientSoundType type in Enum.GetValues<AmbientSoundType>())
        {
            var name = AmbientSoundSystem.GetSoundName(type);
            Assert.False(string.IsNullOrEmpty(name));
        }
    }

    // --- CreateEvent ---

    [Fact]
    public void CreateEvent_Forest_ReturnsCorrectEvent()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Forest);
        Assert.Equal(AmbientSoundType.Forest, evt.Type);
        Assert.Equal(0.5f, evt.Volume);
        Assert.True(evt.ShouldLoop);
    }

    [Fact]
    public void CreateEvent_Silence_ShouldNotLoop()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.Silence);
        Assert.Equal(AmbientSoundType.Silence, evt.Type);
        Assert.Equal(0.0f, evt.Volume);
        Assert.False(evt.ShouldLoop);
    }

    [Fact]
    public void CreateEvent_BossBattle_HighVolume()
    {
        var evt = AmbientSoundSystem.CreateEvent(AmbientSoundType.BossBattle);
        Assert.Equal(0.7f, evt.Volume);
        Assert.True(evt.ShouldLoop);
    }

    [Fact]
    public void CreateEvent_VolumeMatchesGetDefaultVolume()
    {
        // 全タイプでCreateEventのVolumeとGetDefaultVolumeが一致することを確認
        foreach (AmbientSoundType type in Enum.GetValues<AmbientSoundType>())
        {
            var evt = AmbientSoundSystem.CreateEvent(type);
            Assert.Equal(AmbientSoundSystem.GetDefaultVolume(type), evt.Volume);
        }
    }
}
