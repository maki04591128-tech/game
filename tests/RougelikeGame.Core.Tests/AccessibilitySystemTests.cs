using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// AccessibilitySystem（アクセシビリティシステム）のテスト
/// </summary>
public class AccessibilitySystemTests
{
    // --- コンストラクタのテスト ---

    [Fact]
    public void Constructor_DefaultConfig_ColorModeIsNone()
    {
        var system = new AccessibilitySystem();
        Assert.Equal(ColorBlindMode.None, system.Config.ColorMode);
    }

    [Fact]
    public void Constructor_DefaultConfig_FontSizeMultiplierIs1()
    {
        var system = new AccessibilitySystem();
        Assert.Equal(1.0f, system.Config.FontSizeMultiplier);
    }

    [Fact]
    public void Constructor_DefaultConfig_AllModesDisabled()
    {
        var system = new AccessibilitySystem();
        Assert.False(system.Config.HighContrastMode);
        Assert.False(system.Config.ScreenReaderMode);
        Assert.False(system.Config.AutoCombatEnabled);
        Assert.False(system.Config.SimplifiedUI);
        Assert.False(system.Config.LargePointer);
    }

    // --- SetColorBlindMode ---

    [Theory]
    [InlineData(ColorBlindMode.Protanopia)]
    [InlineData(ColorBlindMode.Deuteranopia)]
    [InlineData(ColorBlindMode.Tritanopia)]
    [InlineData(ColorBlindMode.Monochrome)]
    public void SetColorBlindMode_SetsMode(ColorBlindMode mode)
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(mode);
        Assert.Equal(mode, system.Config.ColorMode);
    }

    // --- SetFontSizeMultiplier ---

    [Fact]
    public void SetFontSizeMultiplier_WithinRange_SetsValue()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(2.0f);
        Assert.Equal(2.0f, system.Config.FontSizeMultiplier);
    }

    [Fact]
    public void SetFontSizeMultiplier_BelowMin_ClampsTo05()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(0.1f);
        Assert.Equal(0.5f, system.Config.FontSizeMultiplier);
    }

    [Fact]
    public void SetFontSizeMultiplier_AboveMax_ClampsTo30()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(5.0f);
        Assert.Equal(3.0f, system.Config.FontSizeMultiplier);
    }

    // --- SetGameSpeedMultiplier ---

    [Fact]
    public void SetGameSpeedMultiplier_WithinRange_SetsValue()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(1.5f);
        Assert.Equal(1.5f, system.Config.GameSpeedMultiplier);
    }

    [Fact]
    public void SetGameSpeedMultiplier_BelowMin_ClampsTo025()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(0.1f);
        Assert.Equal(0.25f, system.Config.GameSpeedMultiplier);
    }

    [Fact]
    public void SetGameSpeedMultiplier_AboveMax_ClampsTo20()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(5.0f);
        Assert.Equal(2.0f, system.Config.GameSpeedMultiplier);
    }

    // --- トグル系メソッドのテスト ---

    [Fact]
    public void SetHighContrastMode_True_Enables()
    {
        var system = new AccessibilitySystem();
        system.SetHighContrastMode(true);
        Assert.True(system.Config.HighContrastMode);
    }

    [Fact]
    public void SetScreenReaderMode_True_Enables()
    {
        var system = new AccessibilitySystem();
        system.SetScreenReaderMode(true);
        Assert.True(system.Config.ScreenReaderMode);
    }

    [Fact]
    public void SetAutoCombat_True_Enables()
    {
        var system = new AccessibilitySystem();
        system.SetAutoCombat(true);
        Assert.True(system.Config.AutoCombatEnabled);
    }

    [Fact]
    public void SetSimplifiedUI_True_Enables()
    {
        var system = new AccessibilitySystem();
        system.SetSimplifiedUI(true);
        Assert.True(system.Config.SimplifiedUI);
    }

    [Fact]
    public void SetLargePointer_True_Enables()
    {
        var system = new AccessibilitySystem();
        system.SetLargePointer(true);
        Assert.True(system.Config.LargePointer);
    }

    // --- TransformColor ---

    [Fact]
    public void TransformColor_NoneMode_ReturnsOriginal()
    {
        var system = new AccessibilitySystem();
        var result = system.TransformColor("Red");
        Assert.Equal("Red", result.TransformedColor);
        Assert.Equal(ColorBlindMode.None, result.Mode);
    }

    [Fact]
    public void TransformColor_Protanopia_RedToDarkYellow()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Protanopia);
        var result = system.TransformColor("Red");
        Assert.Equal("DarkYellow", result.TransformedColor);
    }

    [Fact]
    public void TransformColor_Monochrome_AlwaysGray()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Monochrome);
        var result = system.TransformColor("Blue");
        Assert.Equal("Gray", result.TransformedColor);
    }

    // --- CalculateEffectiveFontSize ---

    [Fact]
    public void CalculateEffectiveFontSize_DefaultMultiplier_ReturnsSameSize()
    {
        var system = new AccessibilitySystem();
        Assert.Equal(16, system.CalculateEffectiveFontSize(16));
    }

    [Fact]
    public void CalculateEffectiveFontSize_DoubleMultiplier_ReturnsDouble()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(2.0f);
        Assert.Equal(32, system.CalculateEffectiveFontSize(16));
    }

    // --- CalculateEffectiveTurnDelay ---

    [Fact]
    public void CalculateEffectiveTurnDelay_DefaultSpeed_ReturnsSameDelay()
    {
        var system = new AccessibilitySystem();
        Assert.Equal(1000, system.CalculateEffectiveTurnDelay(1000));
    }

    [Fact]
    public void CalculateEffectiveTurnDelay_DoubleSpeed_ReturnsHalfDelay()
    {
        var system = new AccessibilitySystem();
        system.SetGameSpeedMultiplier(2.0f);
        Assert.Equal(500, system.CalculateEffectiveTurnDelay(1000));
    }

    // --- GetModeName ---

    [Theory]
    [InlineData(ColorBlindMode.None, "通常")]
    [InlineData(ColorBlindMode.Protanopia, "1型色覚（P型）")]
    [InlineData(ColorBlindMode.Deuteranopia, "2型色覚（D型）")]
    [InlineData(ColorBlindMode.Tritanopia, "3型色覚（T型）")]
    [InlineData(ColorBlindMode.Monochrome, "モノクロ")]
    public void GetModeName_ReturnsJapaneseName(ColorBlindMode mode, string expected)
    {
        Assert.Equal(expected, AccessibilitySystem.GetModeName(mode));
    }

    // --- ResetToDefaults ---

    [Fact]
    public void ResetToDefaults_AfterChanges_RestoresDefaults()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Monochrome);
        system.SetFontSizeMultiplier(2.5f);
        system.SetHighContrastMode(true);

        system.ResetToDefaults();

        Assert.Equal(ColorBlindMode.None, system.Config.ColorMode);
        Assert.Equal(1.0f, system.Config.FontSizeMultiplier);
        Assert.Equal(1.0f, system.Config.GameSpeedMultiplier);
        Assert.False(system.Config.HighContrastMode);
    }
}
