using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class GameOverSystemTests
{
    [Fact]
    public void CanRebirth_PositiveSanity_True()
    {
        Assert.True(GameOverSystem.CanRebirth(50));
    }

    [Fact]
    public void CanRebirth_ZeroSanity_False()
    {
        Assert.False(GameOverSystem.CanRebirth(0));
    }

    [Fact]
    public void GetAvailableChoices_ZeroSanity_RebirthUnavailable()
    {
        var choices = GameOverSystem.GetAvailableChoices(0);
        Assert.False(choices[0].Available); // Rebirth
        Assert.True(choices[1].Available);  // ReturnToTitle
    }

    [Fact]
    public void GetGameOverMessage_ContainsCause()
    {
        var msg = GameOverSystem.GetGameOverMessage("敵の攻撃");
        Assert.Contains("敵の攻撃", msg);
    }

    [Theory]
    [InlineData(GameOverSystem.GameOverChoice.Rebirth, "死に戻る")]
    [InlineData(GameOverSystem.GameOverChoice.ReturnToTitle, "タイトル画面")]
    public void GetChoiceText_ReturnsText(GameOverSystem.GameOverChoice choice, string contains)
    {
        Assert.Contains(contains, GameOverSystem.GetChoiceText(choice));
    }
}
