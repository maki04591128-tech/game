namespace RougelikeGame.Core.Systems.Platform;

/// <summary>
/// プラットフォームサービスインターフェース（Steam/GOG/ローカル等の抽象化）
/// Ver.0.1: 6.5 Steam対応準備
/// </summary>
public interface IPlatformService
{
    /// <summary>プラットフォーム名</summary>
    string PlatformName { get; }

    /// <summary>プラットフォームが利用可能か</summary>
    bool IsAvailable { get; }

    /// <summary>プラットフォームを初期化</summary>
    bool Initialize(uint appId = 0);

    /// <summary>プラットフォームをシャットダウン</summary>
    void Shutdown();

    /// <summary>定期コールバック処理（Steamの場合RunCallbacks）</summary>
    void Update();

    /// <summary>実績サービス</summary>
    IPlatformAchievementService Achievements { get; }

    /// <summary>クラウドセーブサービス</summary>
    IPlatformCloudSaveService CloudSave { get; }

    /// <summary>統計サービス</summary>
    IPlatformStatsService Stats { get; }
}

/// <summary>
/// プラットフォーム実績サービスインターフェース
/// </summary>
public interface IPlatformAchievementService
{
    /// <summary>実績を解除</summary>
    bool UnlockAchievement(string achievementId);

    /// <summary>実績が解除済みか確認</summary>
    bool IsAchievementUnlocked(string achievementId);

    /// <summary>全実績をリセット（デバッグ用）</summary>
    bool ResetAllAchievements();

    /// <summary>実績データを同期（サーバーに送信）</summary>
    bool StoreStats();
}

/// <summary>
/// プラットフォームクラウドセーブサービスインターフェース
/// </summary>
public interface IPlatformCloudSaveService
{
    /// <summary>クラウドセーブが有効か</summary>
    bool IsCloudSaveEnabled { get; }

    /// <summary>クラウドにファイルを書き込み</summary>
    bool WriteFile(string fileName, byte[] data);

    /// <summary>クラウドからファイルを読み込み</summary>
    byte[]? ReadFile(string fileName);

    /// <summary>クラウドにファイルが存在するか</summary>
    bool FileExists(string fileName);

    /// <summary>クラウドからファイルを削除</summary>
    bool DeleteFile(string fileName);

    /// <summary>クラウドのファイル一覧を取得</summary>
    IReadOnlyList<string> GetFileList();
}

/// <summary>
/// プラットフォーム統計サービスインターフェース
/// </summary>
public interface IPlatformStatsService
{
    /// <summary>整数統計を設定</summary>
    bool SetStat(string statName, int value);

    /// <summary>浮動小数点統計を設定</summary>
    bool SetStat(string statName, float value);

    /// <summary>整数統計を取得</summary>
    int GetStatInt(string statName);

    /// <summary>浮動小数点統計を取得</summary>
    float GetStatFloat(string statName);

    /// <summary>統計をサーバーに送信</summary>
    bool StoreStats();
}
