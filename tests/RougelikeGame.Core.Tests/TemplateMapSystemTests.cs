using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class TemplateMapSystemTests
{
    [Fact]
    public void GetTemplate_BossFloor_ReturnsData()
    {
        var template = TemplateMapSystem.GetTemplate(TemplateMapType.BossFloor);
        Assert.NotNull(template);
        Assert.Equal("魔王の間", template.Name);
    }

    [Fact]
    public void MeetsLevelRequirement_HighLevel_True()
    {
        Assert.True(TemplateMapSystem.MeetsLevelRequirement(TemplateMapType.Town, 1));
    }

    [Fact]
    public void MeetsLevelRequirement_LowLevel_False()
    {
        Assert.False(TemplateMapSystem.MeetsLevelRequirement(TemplateMapType.BossFloor, 10));
    }

    [Fact]
    public void GetAllTemplates_ReturnsNonEmpty()
    {
        Assert.True(TemplateMapSystem.GetAllTemplates().Count > 0);
    }

    [Theory]
    [InlineData(TemplateMapType.BossFloor, "ボスフロア")]
    [InlineData(TemplateMapType.Town, "街")]
    public void GetTypeName_ReturnsJapanese(TemplateMapType type, string expected)
    {
        Assert.Equal(expected, TemplateMapSystem.GetTypeName(type));
    }
}
