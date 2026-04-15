namespace RougelikeGame.Core.Systems.Platform;

/// <summary>
/// Steamプラットフォームサービスのスタブ実装
/// Steamworks.NET統合時に実装を差し替える。現段階ではLocalPlatformServiceにフォールバック。
/// Ver.0.1: 6.5 Steam対応準備
///
/// 実装手順（Steamworks.NET統合時）:
/// 1. NuGet: Install-Package Steamworks.NET
/// 2. SteamClient.Init(appId) で初期化
/// 3. SteamUserStats.RequestCurrentStats() で実績/統計を読込
/// 4. SteamUserStats.SetAchievement(id) + StoreStats() で実績解除
/// 5. SteamRemoteStorage でクラウドセーブ
/// 6. 毎フレーム SteamClient.RunCallbacks() を呼出
/// </summary>
public class SteamPlatformService : IPlatformService
{
    private bool _initialized;
    private uint _appId;

    // Steamworks.NET未統合時はローカル実装にフォールバック
    private readonly LocalAchievementService _achievementsFallback = new();
    private readonly LocalCloudSaveService _cloudSaveFallback = new();
    private readonly LocalStatsService _statsFallback = new();

    public string PlatformName => "Steam";
    public bool IsAvailable => _initialized;

    /// <summary>Steam AppID</summary>
    public uint AppId => _appId;

    public bool Initialize(uint appId = 0)
    {
        _appId = appId;
        try
        {
            // TODO: Steamworks.NET統合時に実装
            // _initialized = SteamClient.Init(appId);
            // SteamUserStats.RequestCurrentStats();
            _initialized = false; // Steamworks未統合
            return _initialized;
        }
        catch
        {
            _initialized = false;
            return false;
        }
    }

    public void Shutdown()
    {
        if (_initialized)
        {
            // TODO: SteamClient.Shutdown();
            _initialized = false;
        }
    }

    public void Update()
    {
        if (_initialized)
        {
            // TODO: SteamClient.RunCallbacks();
        }
    }

    public IPlatformAchievementService Achievements => _achievementsFallback;
    public IPlatformCloudSaveService CloudSave => _cloudSaveFallback;
    public IPlatformStatsService Stats => _statsFallback;
}
