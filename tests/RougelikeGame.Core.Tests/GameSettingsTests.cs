using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// GameSettings のユニットテスト
/// </summary>
public class GameSettingsTests
{
    [Fact]
    public void CreateDefault_デフォルト値が正しい()
    {
        var settings = GameSettings.CreateDefault();

        Assert.Equal(GameSettings.DefaultMasterVolume, settings.MasterVolume);
        Assert.Equal(GameSettings.DefaultBgmVolume, settings.BgmVolume);
        Assert.Equal(GameSettings.DefaultSeVolume, settings.SeVolume);
        Assert.Equal(GameSettings.DefaultFontSize, settings.FontSize);
        Assert.Equal(GameSettings.DefaultWindowWidth, settings.WindowWidth);
        Assert.Equal(GameSettings.DefaultWindowHeight, settings.WindowHeight);
    }

    [Fact]
    public void EffectiveBgmVolume_マスターとBGMの乗算()
    {
        var settings = new GameSettings
        {
            MasterVolume = 0.5f,
            BgmVolume = 0.8f
        };

        Assert.Equal(0.4f, settings.EffectiveBgmVolume, 3);
    }

    [Fact]
    public void EffectiveSeVolume_マスターとSEの乗算()
    {
        var settings = new GameSettings
        {
            MasterVolume = 0.6f,
            SeVolume = 0.5f
        };

        Assert.Equal(0.3f, settings.EffectiveSeVolume, 3);
    }

    [Fact]
    public void Validate_範囲外の値がクランプされる()
    {
        var settings = new GameSettings
        {
            MasterVolume = 1.5f,
            BgmVolume = -0.1f,
            SeVolume = 2.0f,
            FontSize = 5,
            WindowWidth = 100,
            WindowHeight = 2000
        };

        settings.Validate();

        Assert.Equal(1.0f, settings.MasterVolume);
        Assert.Equal(0.0f, settings.BgmVolume);
        Assert.Equal(1.0f, settings.SeVolume);
        Assert.Equal(10, settings.FontSize);
        Assert.Equal(800, settings.WindowWidth);
        Assert.Equal(1080, settings.WindowHeight);
    }

    [Fact]
    public void Validate_範囲内の値は変わらない()
    {
        var settings = new GameSettings
        {
            MasterVolume = 0.5f,
            BgmVolume = 0.7f,
            SeVolume = 0.3f,
            FontSize = 16,
            WindowWidth = 1024,
            WindowHeight = 720
        };

        settings.Validate();

        Assert.Equal(0.5f, settings.MasterVolume);
        Assert.Equal(0.7f, settings.BgmVolume);
        Assert.Equal(0.3f, settings.SeVolume);
        Assert.Equal(16, settings.FontSize);
        Assert.Equal(1024, settings.WindowWidth);
        Assert.Equal(720, settings.WindowHeight);
    }

    [Fact]
    public void Clone_独立したコピーが作成される()
    {
        var original = new GameSettings
        {
            MasterVolume = 0.5f,
            BgmVolume = 0.6f,
            SeVolume = 0.7f,
            FontSize = 18,
            WindowWidth = 1280,
            WindowHeight = 960
        };

        var clone = original.Clone();

        Assert.Equal(original.MasterVolume, clone.MasterVolume);
        Assert.Equal(original.BgmVolume, clone.BgmVolume);
        Assert.Equal(original.SeVolume, clone.SeVolume);
        Assert.Equal(original.FontSize, clone.FontSize);
        Assert.Equal(original.WindowWidth, clone.WindowWidth);
        Assert.Equal(original.WindowHeight, clone.WindowHeight);

        // 変更が独立している
        clone.MasterVolume = 1.0f;
        Assert.Equal(0.5f, original.MasterVolume);
    }

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f)]
    [InlineData(1.0f, 1.0f, 1.0f)]
    [InlineData(0.5f, 0.5f, 0.25f)]
    public void EffectiveBgmVolume_様々な組み合わせ(float master, float bgm, float expected)
    {
        var settings = new GameSettings
        {
            MasterVolume = master,
            BgmVolume = bgm
        };

        Assert.Equal(expected, settings.EffectiveBgmVolume, 3);
    }

    [Fact]
    public void デフォルト定数_期待値()
    {
        Assert.Equal(0.8f, GameSettings.DefaultMasterVolume);
        Assert.Equal(0.8f, GameSettings.DefaultBgmVolume);
        Assert.Equal(0.8f, GameSettings.DefaultSeVolume);
        Assert.Equal(14, GameSettings.DefaultFontSize);
        Assert.Equal(1024, GameSettings.DefaultWindowWidth);
        Assert.Equal(720, GameSettings.DefaultWindowHeight);
    }
}
