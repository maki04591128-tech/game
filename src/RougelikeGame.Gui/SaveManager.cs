using System.IO;
using System.Text.Json;
using RougelikeGame.Core;

namespace RougelikeGame.Gui;

/// <summary>
/// セーブデータのファイル入出力を管理する
/// </summary>
public static class SaveManager
{
    private static readonly string SaveDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RougelikeGame", "Saves");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// セーブデータをJSON形式でファイルに保存する
    /// </summary>
    public static void Save(SaveData data, int slot = 0)
    {
        Directory.CreateDirectory(SaveDirectory);
        var path = GetSavePath(slot);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// 指定スロットからセーブデータを読み込む
    /// </summary>
    public static SaveData? Load(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
    }

    /// <summary>
    /// 指定スロットにセーブデータが存在するか
    /// </summary>
    public static bool SaveExists(int slot = 0) => File.Exists(GetSavePath(slot));

    /// <summary>
    /// 指定スロットのセーブデータを削除する
    /// </summary>
    public static void DeleteSave(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string GetSavePath(int slot) =>
        Path.Combine(SaveDirectory, $"save_{slot}.json");
}
