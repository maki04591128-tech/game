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

            // T.2: 既存セーブがあればバックアップを作成
            if (File.Exists(path))
            {
                var backupPath = path + ".bak";
                try { File.Copy(path, backupPath, overwrite: true); }
                catch { /* バックアップ失敗は無視 */ }
            }

            data.Version = CurrentSaveVersion;
            data.SavedAt = DateTime.UtcNow;
            ValidateSaveData(data);

            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception ex)
        {
            // CA-5: ディスク容量不足/権限エラー等を安全に処理
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Save failed (slot {slot}): {ex.GetType().Name}: {ex.Message}");
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
            if (!File.Exists(path))
            {
                // T.2: バックアップからの自動復旧を試みる
                var backupPath = path + ".bak";
                if (File.Exists(backupPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[SaveManager] Main save missing, trying backup for slot {slot}");
                    path = backupPath;
                }
                else
                {
                    return null;
                }
            }

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return null;

            var data = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

            // CA-3: セーブデータバージョン検証
            if (data != null && data.Version > CurrentSaveVersion)
            {
                // 未来のバージョンのセーブデータは読み込み不可
                return null;
            }

            if (data != null)
                ValidateSaveData(data);

            return data;
        }
        catch (JsonException ex)
        {
            // T.2: JSON破損時にバックアップからの復旧を試みる
            System.Diagnostics.Debug.WriteLine($"[SaveManager] JSON corrupt (slot {slot}): {ex.Message}");
            return TryLoadBackup(slot);
        }
        catch (Exception ex)
        {
            // CA-6: I/Oエラーを安全に処理
            System.Diagnostics.Debug.WriteLine($"[SaveManager] Load failed (slot {slot}): {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>T.2: バックアップからのセーブデータ復旧</summary>
    private static SaveData? TryLoadBackup(int slot)
    {
        try
        {
            var backupPath = GetSavePath(slot) + ".bak";
            if (!File.Exists(backupPath)) return null;

            var json = File.ReadAllText(backupPath);
            if (string.IsNullOrWhiteSpace(json)) return null;

            var data = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
            if (data != null)
            {
                ValidateSaveData(data);
                System.Diagnostics.Debug.WriteLine($"[SaveManager] Restored from backup (slot {slot})");
            }
            return data;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>T.2: セーブデータの値の範囲を検証・修正</summary>
    public static void ValidateSaveData(SaveData data)
    {
        data.Validate();
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
