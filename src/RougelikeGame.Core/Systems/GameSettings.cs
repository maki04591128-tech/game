using System.IO;
using System.Text.Json;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// ゲーム設定を管理するクラス（音量・表示設定等）
/// </summary>
public class GameSettings
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RougelikeGame");

    private static readonly string SettingsFilePath = Path.Combine(
        SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>マスター音量 (0.0 - 1.0)</summary>
    public float MasterVolume { get; set; } = DefaultMasterVolume;

    /// <summary>BGM音量 (0.0 - 1.0)</summary>
    public float BgmVolume { get; set; } = DefaultBgmVolume;

    /// <summary>SE音量 (0.0 - 1.0)</summary>
    public float SeVolume { get; set; } = DefaultSeVolume;

    /// <summary>フォントサイズ</summary>
    public int FontSize { get; set; } = DefaultFontSize;

    /// <summary>ウィンドウ幅</summary>
    public int WindowWidth { get; set; } = DefaultWindowWidth;

    /// <summary>ウィンドウ高さ</summary>
    public int WindowHeight { get; set; } = DefaultWindowHeight;

    // デフォルト値
    public const float DefaultMasterVolume = 0.8f;
    public const float DefaultBgmVolume = 0.8f;
    public const float DefaultSeVolume = 0.8f;
    public const int DefaultFontSize = 14;
    public const int DefaultWindowWidth = 1024;
    public const int DefaultWindowHeight = 720;

    /// <summary>
    /// 実効BGM音量（マスター × BGM）
    /// </summary>
    public float EffectiveBgmVolume => MasterVolume * BgmVolume;

    /// <summary>
    /// 実効SE音量（マスター × SE）
    /// </summary>
    public float EffectiveSeVolume => MasterVolume * SeVolume;

    /// <summary>
    /// デフォルト設定を返す
    /// </summary>
    public static GameSettings CreateDefault() => new();

    /// <summary>
    /// 設定値を範囲内にクランプする
    /// </summary>
    public void Validate()
    {
        MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
        BgmVolume = Math.Clamp(BgmVolume, 0f, 1f);
        SeVolume = Math.Clamp(SeVolume, 0f, 1f);
        FontSize = Math.Clamp(FontSize, 10, 24);
        WindowWidth = Math.Clamp(WindowWidth, 800, 1920);
        WindowHeight = Math.Clamp(WindowHeight, 600, 1080);
    }

    /// <summary>
    /// 設定をJSONファイルに保存する
    /// </summary>
    public void Save()
    {
        Validate();
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// 設定をJSONファイルから読み込む（ファイルがなければデフォルト値）
    /// </summary>
    public static GameSettings Load()
    {
        if (!File.Exists(SettingsFilePath))
            return CreateDefault();

        try
        {
            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions);
            if (settings == null) return CreateDefault();
            settings.Validate();
            return settings;
        }
        catch
        {
            return CreateDefault();
        }
    }

    /// <summary>
    /// 設定ファイルが存在するか
    /// </summary>
    public static bool SettingsFileExists() => File.Exists(SettingsFilePath);

    /// <summary>
    /// 現在の設定のコピーを作成する
    /// </summary>
    public GameSettings Clone() => new()
    {
        MasterVolume = MasterVolume,
        BgmVolume = BgmVolume,
        SeVolume = SeVolume,
        FontSize = FontSize,
        WindowWidth = WindowWidth,
        WindowHeight = WindowHeight
    };
}
