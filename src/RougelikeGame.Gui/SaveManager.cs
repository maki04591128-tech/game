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
    public static bool Save(SaveData data, int slot = 0)
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var path = GetSavePath(slot);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception)
        {
            // CA-5: ディスク容量不足/権限エラー等を安全に処理
            return false;
        }
    }

    /// <summary>
    /// 指定スロットからセーブデータを読み込む
    /// </summary>
    public static SaveData? Load(int slot = 0)
    {
        try
        {
            var path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

            // CA-3: セーブデータバージョン検証
            if (data != null && data.Version > CurrentSaveVersion)
            {
                // 未来のバージョンのセーブデータは読み込み不可
                return null;
            }

            return data;
        }
        catch (Exception)
        {
            // CA-6: 破損JSONやI/Oエラーを安全に処理
            return null;
        }
    }

    /// <summary>CA-3: 現在のセーブデータバージョン</summary>
    public const int CurrentSaveVersion = 1;

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

    /// <summary>
    /// 存在する全セーブスロットの番号一覧を返す
    /// </summary>
    public static List<int> GetAllSaveSlots(int maxSlots = 10)
    {
        var slots = new List<int>();
        for (int i = 0; i < maxSlots; i++)
        {
            if (SaveExists(i))
                slots.Add(i);
        }
        return slots;
    }

    /// <summary>
    /// いずれかのスロットにセーブデータが存在するか
    /// </summary>
    public static bool AnySaveExists(int maxSlots = 10)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (SaveExists(i)) return true;
        }
        return false;
    }

    private static string GetSavePath(int slot) =>
        Path.Combine(SaveDirectory, $"save_{slot}.json");
}
