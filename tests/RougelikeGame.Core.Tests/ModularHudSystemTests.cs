using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class ModularHudSystemTests
{
    [Fact]
    public void Constructor_AllElementsVisible()
    {
        var system = new ModularHudSystem();
        Assert.Equal(5, system.GetVisibleCount());
    }

    [Fact]
    public void SetVisibility_HidesElement()
    {
        var system = new ModularHudSystem();
        system.SetVisibility(HudElement.MiniMap, false);
        Assert.Equal(4, system.GetVisibleCount());
    }

    [Fact]
    public void SetPosition_UpdatesConfig()
    {
        var system = new ModularHudSystem();
        system.SetPosition(HudElement.HpBar, 100, 50);
        var config = system.GetConfig(HudElement.HpBar);
        Assert.NotNull(config);
        Assert.Equal(100, config.PositionX);
        Assert.Equal(50, config.PositionY);
    }

    [Fact]
    public void SetScale_ClampsToRange()
    {
        var system = new ModularHudSystem();
        system.SetScale(HudElement.MpBar, 5.0f);
        var config = system.GetConfig(HudElement.MpBar);
        Assert.NotNull(config);
        Assert.Equal(2.0f, config.Scale);
    }

    [Theory]
    [InlineData(HudElement.HpBar, "HPバー")]
    [InlineData(HudElement.MiniMap, "ミニマップ")]
    public void GetElementName_ReturnsJapanese(HudElement elem, string expected)
    {
        Assert.Equal(expected, ModularHudSystem.GetElementName(elem));
    }
}
