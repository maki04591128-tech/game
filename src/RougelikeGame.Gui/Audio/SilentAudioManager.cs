namespace RougelikeGame.Gui.Audio;

/// <summary>
/// テスト・音声ファイル未配置時用の無音AudioManager実装
/// </summary>
public class SilentAudioManager : IAudioManager
{
    public float MasterVolume { get; set; } = 0.8f;
    public float BgmVolume { get; set; } = 0.8f;
    public float SeVolume { get; set; } = 0.8f;
    public string? CurrentBgmId { get; private set; }
    public bool IsBgmPlaying { get; private set; }

    public void PlayBgm(string bgmId, bool loop = true)
    {
        CurrentBgmId = bgmId;
        IsBgmPlaying = true;
    }

    public void FadeToBgm(string newBgmId, bool loop = true, double fadeOutMs = 800, double fadeInMs = 800)
    {
        CurrentBgmId = newBgmId;
        IsBgmPlaying = true;
    }

    public void StopBgm()
    {
        CurrentBgmId = null;
        IsBgmPlaying = false;
    }

    public void PauseBgm()
    {
        IsBgmPlaying = false;
    }

    public void ResumeBgm()
    {
        if (CurrentBgmId != null)
            IsBgmPlaying = true;
    }

    public void PlaySe(string seId) { }

    public void StopAll()
    {
        StopBgm();
    }

    public void ApplyVolumeSettings(float masterVolume, float bgmVolume, float seVolume)
    {
        MasterVolume = Math.Clamp(masterVolume, 0f, 1f);
        BgmVolume = Math.Clamp(bgmVolume, 0f, 1f);
        SeVolume = Math.Clamp(seVolume, 0f, 1f);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
