using Xunit;
using RougelikeGame.Core.Data;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>α.13-16: 宗教テキストコンテンツのテスト</summary>
public class ReligionLoreDataTests
{
    [Theory]
    [InlineData(ReligionId.LightTemple)]
    [InlineData(ReligionId.DarkCult)]
    [InlineData(ReligionId.NatureWorship)]
    [InlineData(ReligionId.DeathFaith)]
    [InlineData(ReligionId.ChaosCult)]
    [InlineData(ReligionId.Atheism)]
    public void GetDoctrine_AllReligions_ReturnsNonEmptyString(ReligionId id)
    {
        var doctrine = ReligionLoreData.GetDoctrine(id);
        Assert.NotEmpty(doctrine);
        Assert.NotEqual("教義情報なし", doctrine);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple)]
    [InlineData(ReligionId.DarkCult)]
    [InlineData(ReligionId.NatureWorship)]
    [InlineData(ReligionId.DeathFaith)]
    [InlineData(ReligionId.ChaosCult)]
    public void GetInitiationText_AllReligions_ReturnsNonEmptyString(ReligionId id)
    {
        var text = ReligionLoreData.GetInitiationText(id);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void GetConversionText_FromAtheism_ReturnsInitiationText()
    {
        var text = ReligionLoreData.GetConversionText(ReligionId.Atheism, ReligionId.LightTemple);
        var initText = ReligionLoreData.GetInitiationText(ReligionId.LightTemple);
        Assert.Equal(initText, text);
    }

    [Fact]
    public void GetConversionText_FromOtherReligion_ContainsConversionMention()
    {
        var text = ReligionLoreData.GetConversionText(ReligionId.LightTemple, ReligionId.DarkCult);
        Assert.Contains("改宗", text);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, 0)]
    [InlineData(ReligionId.LightTemple, 21)]
    [InlineData(ReligionId.LightTemple, 61)]
    [InlineData(ReligionId.DarkCult, 0)]
    [InlineData(ReligionId.DarkCult, 30)]
    public void GetPriestGreeting_VariousFaithPoints_ReturnsNonEmpty(ReligionId id, int faithPoints)
    {
        var greeting = ReligionLoreData.GetPriestGreeting(id, faithPoints);
        Assert.NotEmpty(greeting);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, "テスト恩恵")]
    [InlineData(ReligionId.DarkCult, "闇の力")]
    [InlineData(ReligionId.ChaosCult, "混沌の波動")]
    public void GetBenefitActivationText_AllReligions_ContainsBenefitName(ReligionId id, string benefitName)
    {
        var text = ReligionLoreData.GetBenefitActivationText(id, benefitName);
        Assert.Contains(benefitName, text);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, "殺害禁止")]
    [InlineData(ReligionId.DarkCult, "光魔法禁止")]
    [InlineData(ReligionId.NatureWorship, "自然破壊禁止")]
    public void GetTabooViolationText_AllReligions_ContainsTabooName(ReligionId id, string tabooName)
    {
        var text = ReligionLoreData.GetTabooViolationText(id, tabooName);
        Assert.Contains(tabooName, text);
    }

    [Fact]
    public void GetDoctrine_LightTemple_ContainsKeywords()
    {
        var doctrine = ReligionLoreData.GetDoctrine(ReligionId.LightTemple);
        Assert.Contains("ソラリス", doctrine);
        Assert.Contains("慈悲", doctrine);
    }

    [Fact]
    public void GetDoctrine_DarkCult_ContainsKeywords()
    {
        var doctrine = ReligionLoreData.GetDoctrine(ReligionId.DarkCult);
        Assert.Contains("ニュクス", doctrine);
        Assert.Contains("深淵", doctrine);
    }
}
