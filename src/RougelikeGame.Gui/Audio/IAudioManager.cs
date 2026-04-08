namespace RougelikeGame.Gui.Audio;

/// <summary>
/// BGM/SE再生を管理するインターフェース
/// </summary>
public interface IAudioManager : IDisposable
{
    /// <summary>マスター音量 (0.0 - 1.0)</summary>
    float MasterVolume { get; set; }

    /// <summary>BGM音量 (0.0 - 1.0)</summary>
    float BgmVolume { get; set; }

    /// <summary>SE音量 (0.0 - 1.0)</summary>
    float SeVolume { get; set; }

    /// <summary>現在再生中のBGM ID</summary>
    string? CurrentBgmId { get; }

    /// <summary>BGMが再生中か</summary>
    bool IsBgmPlaying { get; }

    /// <summary>
    /// BGMを再生する（既に同じIDが再生中なら何もしない）
    /// </summary>
    void PlayBgm(string bgmId, bool loop = true);

    /// <summary>
    /// BGMをフェードアウトしてから別のBGMをフェードインする（β.8）
    /// </summary>
    void FadeToBgm(string newBgmId, bool loop = true, double fadeOutMs = 800, double fadeInMs = 800);

    /// <summary>
    /// BGMを停止する
    /// </summary>
    void StopBgm();

    /// <summary>
    /// BGMを一時停止する
    /// </summary>
    void PauseBgm();

    /// <summary>
    /// BGMを再開する
    /// </summary>
    void ResumeBgm();

    /// <summary>
    /// SEを再生する
    /// </summary>
    void PlaySe(string seId);

    /// <summary>
    /// 全ての音声を停止する
    /// </summary>
    void StopAll();

    /// <summary>
    /// 音量設定を適用する
    /// </summary>
    void ApplyVolumeSettings(float masterVolume, float bgmVolume, float seVolume);
}
