using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.25 フラグ選択肢変動システムのテスト
/// </summary>
public class FlagConditionSystemTests
{
    #region ParseCondition Tests

    [Fact]
    public void ParseCondition_HasFlag_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("has:quest_dragon_slain");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.HasFlag, cond!.Type);
        Assert.Equal("quest_dragon_slain", cond.Key);
    }

    [Fact]
    public void ParseCondition_StatCompare_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("stat:STR >= 20");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.StatCompare, cond!.Type);
        Assert.Equal("STR", cond.Key);
        Assert.Equal(">=", cond.Operator);
        Assert.Equal("20", cond.Value);
    }

    [Fact]
    public void ParseCondition_Race_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("race:Elf");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.RaceCheck, cond!.Type);
        Assert.Equal("Elf", cond.Value);
    }

    [Fact]
    public void ParseCondition_Religion_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("religion:LightTemple");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.ReligionCheck, cond!.Type);
        Assert.Equal("LightTemple", cond.Value);
    }

    [Fact]
    public void ParseCondition_Mastery_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("mastery:sword >= 10");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.MasteryCheck, cond!.Type);
        Assert.Equal("sword", cond.Key);
        Assert.Equal(">=", cond.Operator);
        Assert.Equal("10", cond.Value);
    }

    [Fact]
    public void ParseCondition_Karma_ParsesCorrectly()
    {
        var cond = FlagConditionSystem.ParseCondition("karma >= 50");
        Assert.NotNull(cond);
        Assert.Equal(FlagConditionType.ValueCompare, cond!.Type);
        Assert.Equal("karma", cond.Key);
    }

    #endregion

    #region CompoundCondition Tests

    [Fact]
    public void ParseCompound_AND_ParsesCorrectly()
    {
        var compound = FlagConditionSystem.ParseCompound("has:guild_registered AND karma >= 30");
        Assert.NotNull(compound);
        Assert.True(compound!.IsAnd);
        Assert.Equal(2, compound.Conditions.Count);
    }

    [Fact]
    public void ParseCompound_OR_ParsesCorrectly()
    {
        var compound = FlagConditionSystem.ParseCompound("race:Elf OR race:Human");
        Assert.NotNull(compound);
        Assert.False(compound!.IsAnd);
        Assert.Equal(2, compound.Conditions.Count);
    }

    #endregion

    #region Evaluation Tests

    [Fact]
    public void EvaluateCondition_HasFlag_TrueWhenPresent()
    {
        var cond = new FlagCondition(FlagConditionType.HasFlag, "quest_done", "==", "true");
        var context = new Dictionary<string, string> { ["quest_done"] = "true" };
        Assert.True(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCondition_HasFlag_FalseWhenMissing()
    {
        var cond = new FlagCondition(FlagConditionType.HasFlag, "quest_done", "==", "true");
        var context = new Dictionary<string, string>();
        Assert.False(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCondition_ValueCompare_GreaterOrEqual()
    {
        var cond = new FlagCondition(FlagConditionType.ValueCompare, "karma", ">=", "50");
        var context = new Dictionary<string, string> { ["karma"] = "80" };
        Assert.True(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCondition_ValueCompare_BelowThreshold()
    {
        var cond = new FlagCondition(FlagConditionType.ValueCompare, "karma", ">=", "50");
        var context = new Dictionary<string, string> { ["karma"] = "30" };
        Assert.False(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCondition_RaceCheck_Matches()
    {
        var cond = new FlagCondition(FlagConditionType.RaceCheck, "Race", "==", "Elf");
        var context = new Dictionary<string, string> { ["Race"] = "Elf" };
        Assert.True(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCondition_RaceCheck_DoesNotMatch()
    {
        var cond = new FlagCondition(FlagConditionType.RaceCheck, "Race", "==", "Elf");
        var context = new Dictionary<string, string> { ["Race"] = "Human" };
        Assert.False(FlagConditionSystem.EvaluateCondition(cond, context));
    }

    [Fact]
    public void EvaluateCompound_AND_AllTrue()
    {
        var compound = FlagConditionSystem.ParseCompound("has:guild_registered AND karma >= 30");
        var context = new Dictionary<string, string>
        {
            ["guild_registered"] = "true",
            ["karma"] = "50"
        };
        Assert.True(FlagConditionSystem.EvaluateCompound(compound!, context));
    }

    [Fact]
    public void EvaluateCompound_AND_OneFalse()
    {
        var compound = FlagConditionSystem.ParseCompound("has:guild_registered AND karma >= 30");
        var context = new Dictionary<string, string>
        {
            ["karma"] = "50"
            // guild_registered missing
        };
        Assert.False(FlagConditionSystem.EvaluateCompound(compound!, context));
    }

    [Fact]
    public void EvaluateCompound_OR_OneTrue()
    {
        var compound = FlagConditionSystem.ParseCompound("race:Elf OR race:Human");
        var context = new Dictionary<string, string> { ["Race"] = "Human" };
        Assert.True(FlagConditionSystem.EvaluateCompound(compound!, context));
    }

    [Fact]
    public void EvaluateCompound_OR_NoneTrue()
    {
        var compound = FlagConditionSystem.ParseCompound("race:Elf OR race:Human");
        var context = new Dictionary<string, string> { ["Race"] = "Orc" };
        Assert.False(FlagConditionSystem.EvaluateCompound(compound!, context));
    }

    #endregion
}
