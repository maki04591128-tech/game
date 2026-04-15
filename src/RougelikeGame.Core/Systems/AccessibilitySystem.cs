namespace RougelikeGame.Core.Systems;

/// <summary>
/// アクセシビリティオプション - 色覚対応、フォントサイズ、速度調整等のアクセシビリティ設定
/// 参考: Hades（ゴッドモード）、Celeste（アシストモード）
/// </summary>
public class AccessibilitySystem
{
    /// <summary>アクセシビリティ設定</summary>
    public record AccessibilityConfig(
        ColorBlindMode ColorMode,
        float FontSizeMultiplier,
        float GameSpeedMultiplier,
        bool HighContrastMode,
        bool ScreenReaderMode,
        bool AutoCombatEnabled,
        bool SimplifiedUI,
        bool LargePointer
    );

    /// <summary>色変換情報</summary>
    public record ColorTransform(
        string OriginalColor,
        string TransformedColor,
        ColorBlindMode Mode
    );

    private AccessibilityConfig _config;

    /// <summary>現在の設定</summary>
    public AccessibilityConfig Config => _config;

    /// <summary>初期化</summary>
    public AccessibilitySystem()
    {
        _config = new AccessibilityConfig(
            ColorMode: ColorBlindMode.None,
            FontSizeMultiplier: 1.0f,
            GameSpeedMultiplier: 1.0f,
            HighContrastMode: false,
            ScreenReaderMode: false,
            AutoCombatEnabled: false,
            SimplifiedUI: false,
            LargePointer: false
        );
    }

    /// <summary>色覚モードを設定</summary>
    public void SetColorBlindMode(ColorBlindMode mode)
    {
        _config = _config with { ColorMode = mode };
    }

    /// <summary>フォントサイズ倍率を設定（0.5〜3.0）</summary>
    public void SetFontSizeMultiplier(float multiplier)
    {
        _config = _config with { FontSizeMultiplier = Math.Clamp(multiplier, 0.5f, 3.0f) };
    }

    /// <summary>ゲーム速度倍率を設定（0.25〜2.0）</summary>
    public void SetGameSpeedMultiplier(float multiplier)
    {
        _config = _config with { GameSpeedMultiplier = Math.Clamp(multiplier, 0.25f, 2.0f) };
    }

    /// <summary>ハイコントラストモードを切り替え</summary>
    public void SetHighContrastMode(bool enabled)
    {
        _config = _config with { HighContrastMode = enabled };
    }

    /// <summary>スクリーンリーダーモードを切り替え</summary>
    public void SetScreenReaderMode(bool enabled)
    {
        _config = _config with { ScreenReaderMode = enabled };
    }

    /// <summary>オート戦闘を切り替え</summary>
    public void SetAutoCombat(bool enabled)
    {
        _config = _config with { AutoCombatEnabled = enabled };
    }

    /// <summary>簡易UIモードを切り替え</summary>
    public void SetSimplifiedUI(bool enabled)
    {
        _config = _config with { SimplifiedUI = enabled };
    }

    /// <summary>大きなポインタを切り替え</summary>
    public void SetLargePointer(bool enabled)
    {
        _config = _config with { LargePointer = enabled };
    }

    /// <summary>色覚モードに応じた色変換を取得</summary>
    public ColorTransform TransformColor(string originalColor)
    {
        string transformed = _config.ColorMode switch
        {
            ColorBlindMode.Protanopia => TransformForProtanopia(originalColor),
            ColorBlindMode.Deuteranopia => TransformForDeuteranopia(originalColor),
            ColorBlindMode.Tritanopia => TransformForTritanopia(originalColor),
            ColorBlindMode.Monochrome => TransformToMonochrome(originalColor),
            _ => originalColor
        };
        return new ColorTransform(originalColor, transformed, _config.ColorMode);
    }

    /// <summary>実効フォントサイズを計算</summary>
    public int CalculateEffectiveFontSize(int baseFontSize)
    {
        return (int)(baseFontSize * _config.FontSizeMultiplier);
    }

    /// <summary>実効ターン遅延を計算（ミリ秒）</summary>
    public int CalculateEffectiveTurnDelay(int baseDelay)
    {
        float speed = Math.Max(0.1f, _config.GameSpeedMultiplier);
        return (int)(baseDelay / speed);
    }

    /// <summary>色覚モード名を取得</summary>
    public static string GetModeName(ColorBlindMode mode) => mode switch
    {
        ColorBlindMode.None => "通常",
        ColorBlindMode.Protanopia => "1型色覚（P型）",
        ColorBlindMode.Deuteranopia => "2型色覚（D型）",
        ColorBlindMode.Tritanopia => "3型色覚（T型）",
        ColorBlindMode.Monochrome => "モノクロ",
        _ => "不明"
    };

    /// <summary>設定をリセット</summary>
    public void ResetToDefaults()
    {
        _config = new AccessibilityConfig(
            ColorBlindMode.None, 1.0f, 1.0f, false, false, false, false, false
        );
    }

    /// <summary>GameSettingsからアクセシビリティ設定を適用</summary>
    public void ApplyFromGameSettings(GameSettings settings)
    {
        SetColorBlindMode(settings.ColorBlindMode);
        SetFontSizeMultiplier(settings.FontSize / 14.0f);
        SetGameSpeedMultiplier(settings.GameSpeedMultiplier);
        SetHighContrastMode(settings.HighContrastMode);
        SetScreenReaderMode(settings.ScreenReaderMode);
        SetLargePointer(settings.LargePointer);
    }

    private static string TransformForProtanopia(string color) => color switch
    {
        "Red" => "DarkYellow",
        "Green" => "Blue",
        _ => color
    };

    private static string TransformForDeuteranopia(string color) => color switch
    {
        "Green" => "Orange",
        "Red" => "Brown",
        _ => color
    };

    private static string TransformForTritanopia(string color) => color switch
    {
        "Blue" => "Cyan",
        "Yellow" => "Pink",
        _ => color
    };

    private static string TransformToMonochrome(string color) => "Gray";
}
