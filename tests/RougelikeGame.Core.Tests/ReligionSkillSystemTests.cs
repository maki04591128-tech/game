using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.38 宗教スキル補正システムのテスト
/// </summary>
public class ReligionSkillSystemTests
{
    #region Damage Multiplier Tests

    [Fact]
    public void GetSkillDamageMultiplier_None_Returns1()
    {
        float result = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.LightTemple, FaithRank.None);
        Assert.Equal(1.0f, result);
    }

    [Theory]
    [InlineData(FaithRank.Believer)]
    [InlineData(FaithRank.Devout)]
    [InlineData(FaithRank.Saint)]
    public void GetSkillDamageMultiplier_HigherRank_HigherMultiplier(FaithRank rank)
    {
        float result = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.LightTemple, rank);
        Assert.True(result > 1.0f);
    }

    [Fact]
    public void GetSkillDamageMultiplier_SaintRank_HighestMultiplier()
    {
        float saint = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.LightTemple, FaithRank.Saint);
        float believer = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.LightTemple, FaithRank.Believer);
        Assert.True(saint > believer);
    }

    [Fact]
    public void GetSkillDamageMultiplier_DifferentReligions_DifferentBonuses()
    {
        float light = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.LightTemple, FaithRank.Saint);
        float dark = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.DarkCult, FaithRank.Saint);
        float chaos = ReligionSkillSystem.GetSkillDamageMultiplier(ReligionId.ChaosCult, FaithRank.Saint);

        // 闘系宗教は補正が高い
        Assert.True(chaos > light);
        Assert.True(dark > light);
    }

    #endregion

    #region Cost Reduction Tests

    [Fact]
    public void GetSkillCostReduction_None_ReturnsZero()
    {
        float result = ReligionSkillSystem.GetSkillCostReduction(ReligionId.LightTemple, FaithRank.None);
        Assert.Equal(0f, result);
    }

    [Theory]
    [InlineData(FaithRank.Believer)]
    [InlineData(FaithRank.Champion)]
    [InlineData(FaithRank.Saint)]
    public void GetSkillCostReduction_HigherRank_HigherReduction(FaithRank rank)
    {
        float result = ReligionSkillSystem.GetSkillCostReduction(ReligionId.LightTemple, rank);
        Assert.True(result > 0f);
        Assert.True(result < 1.0f);
    }

    [Fact]
    public void GetSkillCostReduction_Saint_MaxReduction()
    {
        float saint = ReligionSkillSystem.GetSkillCostReduction(ReligionId.LightTemple, FaithRank.Saint);
        Assert.Equal(0.25f, saint);
    }

    #endregion

    #region Granted Skill Bonuses Tests

    [Fact]
    public void GetGrantedSkillBonuses_None_ReturnsEmpty()
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(ReligionId.LightTemple, FaithRank.None);
        Assert.Empty(bonuses);
    }

    [Fact]
    public void GetGrantedSkillBonuses_LightTemple_ReturnsSkills()
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(ReligionId.LightTemple, FaithRank.Believer);
        Assert.True(bonuses.Count >= 3);
        Assert.Contains(bonuses, b => b.SkillId == "holy_light");
        Assert.Contains(bonuses, b => b.SkillId == "purify");
    }

    [Fact]
    public void GetGrantedSkillBonuses_Champion_UnlocksUltimate()
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(ReligionId.LightTemple, FaithRank.Champion);
        Assert.Contains(bonuses, b => b.SkillId == "divine_miracle");
    }

    [Fact]
    public void GetGrantedSkillBonuses_BelowChampion_NoUltimate()
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(ReligionId.LightTemple, FaithRank.Blessed);
        Assert.DoesNotContain(bonuses, b => b.SkillId == "divine_miracle");
    }

    [Fact]
    public void GetGrantedSkillBonuses_AllReligions_HaveSkills()
    {
        var religions = new[] { ReligionId.LightTemple, ReligionId.DarkCult,
            ReligionId.NatureWorship, ReligionId.DeathFaith, ReligionId.ChaosCult };

        foreach (var religion in religions)
        {
            var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(religion, FaithRank.Believer);
            Assert.True(bonuses.Count > 0, $"{religion} should have skills");
        }
    }

    [Fact]
    public void GetGrantedSkillBonuses_Atheism_NoSkills()
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(ReligionId.Atheism, FaithRank.Saint);
        Assert.Empty(bonuses);
    }

    #endregion

    #region Skill Alignment Tests

    [Theory]
    [InlineData(ReligionId.LightTemple, Element.Light, true)]
    [InlineData(ReligionId.LightTemple, Element.Holy, true)]
    [InlineData(ReligionId.LightTemple, Element.Dark, false)]
    [InlineData(ReligionId.DarkCult, Element.Dark, true)]
    [InlineData(ReligionId.DarkCult, Element.Curse, true)]
    [InlineData(ReligionId.DarkCult, Element.Light, false)]
    [InlineData(ReligionId.ChaosCult, Element.Fire, true)]  // Chaos matches all
    [InlineData(ReligionId.ChaosCult, Element.Dark, true)]
    public void IsSkillAlignedWithReligion_ReturnsCorrectResult(ReligionId religion, Element element, bool expected)
    {
        Assert.Equal(expected, ReligionSkillSystem.IsSkillAlignedWithReligion(religion, element));
    }

    [Fact]
    public void GetAlignedSkillBonus_AlignedElement_GivesBonus()
    {
        float bonus = ReligionSkillSystem.GetAlignedSkillBonus(ReligionId.LightTemple, Element.Holy, FaithRank.Saint);
        Assert.True(bonus > 1.0f);
    }

    [Fact]
    public void GetAlignedSkillBonus_UnalignedElement_NoBonus()
    {
        float bonus = ReligionSkillSystem.GetAlignedSkillBonus(ReligionId.LightTemple, Element.Dark, FaithRank.Saint);
        Assert.Equal(1.0f, bonus);
    }

    #endregion

    #region Apostasy Tests

    [Fact]
    public void GetApostasyPenalty_NotApostate_NoPenalty()
    {
        Assert.Equal(1.0f, ReligionSkillSystem.GetApostasyPenalty(false));
    }

    [Fact]
    public void GetApostasyPenalty_Apostate_HasPenalty()
    {
        float penalty = ReligionSkillSystem.GetApostasyPenalty(true);
        Assert.True(penalty < 1.0f);
        Assert.Equal(0.7f, penalty);
    }

    #endregion
}
