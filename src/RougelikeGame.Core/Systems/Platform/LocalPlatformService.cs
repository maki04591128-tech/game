namespace RougelikeGame.Core.Systems.Platform;

/// <summary>
/// ローカルプラットフォームサービス（Steam非接続時のフォールバック実装）
/// クラウドセーブはローカルファイルシステム、実績はメモリ内で管理
/// Ver.0.1: 6.5 Steam対応準備
/// </summary>
public class LocalPlatformService : IPlatformService
{
    private readonly LocalAchievementService _achievements = new();
    private readonly LocalCloudSaveService _cloudSave = new();
    private readonly LocalStatsService _stats = new();

    public string PlatformName => "Local";
    public bool IsAvailable => true;

    public bool Initialize(uint appId = 0) => true;
    public void Shutdown() { }
    public void Update() { }

    public IPlatformAchievementService Achievements => _achievements;
    public IPlatformCloudSaveService CloudSave => _cloudSave;
    public IPlatformStatsService Stats => _stats;
}

/// <summary>ローカル実績サービス（メモリ内管理）</summary>
public class LocalAchievementService : IPlatformAchievementService
{
    private readonly HashSet<string> _unlocked = new();

    public bool UnlockAchievement(string achievementId)
    {
        return _unlocked.Add(achievementId);
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return _unlocked.Contains(achievementId);
    }

    public bool ResetAllAchievements()
    {
        _unlocked.Clear();
        return true;
    }

    public bool StoreStats() => true;
}

/// <summary>ローカルクラウドセーブサービス（ローカルファイルシステム）</summary>
public class LocalCloudSaveService : IPlatformCloudSaveService
{
    private readonly string _saveDirectory;

    public LocalCloudSaveService()
    {
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RougelikeGame", "CloudSave");
        Directory.CreateDirectory(_saveDirectory);
    }

    public bool IsCloudSaveEnabled => true;

    public bool WriteFile(string fileName, byte[] data)
    {
        try
        {
            var path = Path.Combine(_saveDirectory, fileName);
            File.WriteAllBytes(path, data);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudSave] WriteFile failed: {fileName} - {ex.Message}");
            return false;
        }
    }

    public byte[]? ReadFile(string fileName)
    {
        try
        {
            var path = Path.Combine(_saveDirectory, fileName);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudSave] ReadFile failed: {fileName} - {ex.Message}");
            return null;
        }
    }

    public bool FileExists(string fileName)
    {
        return File.Exists(Path.Combine(_saveDirectory, fileName));
    }

    public bool DeleteFile(string fileName)
    {
        try
        {
            var path = Path.Combine(_saveDirectory, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudSave] DeleteFile failed: {fileName} - {ex.Message}");
            return false;
        }
    }

    public IReadOnlyList<string> GetFileList()
    {
        try
        {
            return Directory.GetFiles(_saveDirectory)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CloudSave] GetFileList failed: {ex.Message}");
            return Array.Empty<string>();
        }
    }
}

/// <summary>ローカル統計サービス（メモリ内管理）</summary>
public class LocalStatsService : IPlatformStatsService
{
    private readonly Dictionary<string, int> _intStats = new();
    private readonly Dictionary<string, float> _floatStats = new();

    public bool SetStat(string statName, int value)
    {
        _intStats[statName] = value;
        return true;
    }

    public bool SetStat(string statName, float value)
    {
        _floatStats[statName] = value;
        return true;
    }

    public int GetStatInt(string statName)
    {
        return _intStats.GetValueOrDefault(statName, 0);
    }

    public float GetStatFloat(string statName)
    {
        return _floatStats.GetValueOrDefault(statName, 0f);
    }

    public bool StoreStats() => true;
}
