using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class MultiClassSystemTests
{
    [Fact]
    public void CanClassChange_ValidConditions_True()
    {
        var quests = new HashSet<string> { "knight_trial" };
        Assert.True(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 20, quests));
    }

    [Fact]
    public void CanClassChange_LowLevel_False()
    {
        var quests = new HashSet<string> { "knight_trial" };
        Assert.False(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 10, quests));
    }

    [Fact]
    public void CanClassChange_NoQuest_False()
    {
        var quests = new HashSet<string>();
        Assert.False(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 20, quests));
    }

    [Fact]
    public void GetAvailableChanges_Fighter_ReturnsKnight()
    {
        var changes = MultiClassSystem.GetAvailableChanges(CharacterClass.Fighter);
        Assert.Single(changes);
        Assert.Equal(CharacterClass.Knight, changes[0].ToClass);
    }

    [Theory]
    [InlineData(ClassTier.Base, "基本職")]
    [InlineData(ClassTier.Advanced, "上位職")]
    [InlineData(ClassTier.Master, "最上位職")]
    public void GetTierName_ReturnsJapanese(ClassTier tier, string expected)
    {
        Assert.Equal(expected, MultiClassSystem.GetTierName(tier));
    }

    [Fact]
    public void GetSubclassExpRate_Returns50Percent()
    {
        Assert.Equal(0.5f, MultiClassSystem.GetSubclassExpRate());
    }
}
