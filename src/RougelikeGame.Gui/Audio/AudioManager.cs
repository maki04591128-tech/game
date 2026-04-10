using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace RougelikeGame.Gui.Audio;

/// <summary>
/// WPF MediaPlayerを使用したBGM/SE再生管理
/// </summary>
public class AudioManager : IAudioManager
{
    private readonly MediaPlayer _bgmPlayer = new();
    private readonly List<MediaPlayer> _sePlayers = new();
    private readonly string _bgmDirectory;
    private readonly string _seDirectory;
    private bool _bgmLoop;
    private bool _disposed;

    /// <summary>SE同時再生の上限</summary>
    private const int MaxSimultaneousSe = 8;

    /// <summary>同一SE再生の最小間隔（ms）</summary>
    private const int SeMinIntervalMs = 50;

    /// <summary>最後にSEを再生した時刻（SE ID別）</summary>
    private readonly Dictionary<string, DateTime> _lastSePlayTimes = new();

    public float MasterVolume { get; set; } = 0.8f;
    public float BgmVolume { get; set; } = 0.8f;
    public float SeVolume { get; set; } = 0.8f;
    public string? CurrentBgmId { get; private set; }
    public bool IsBgmPlaying { get; private set; }

    public AudioManager(string? resourceBasePath = null)
    {
        var basePath = resourceBasePath ?? AppDomain.CurrentDomain.BaseDirectory;
        _bgmDirectory = Path.Combine(basePath, "Resources", "BGM");
        _seDirectory = Path.Combine(basePath, "Resources", "SE");

        _bgmPlayer.MediaEnded += BgmPlayer_MediaEnded;
    }

    public void PlayBgm(string bgmId, bool loop = true)
    {
        if (CurrentBgmId == bgmId && IsBgmPlaying) return;

        StopBgm();

        var filePath = FindAudioFile(_bgmDirectory, bgmId);
        if (filePath == null) return;

        try
        {
            _bgmLoop = loop;
            _bgmPlayer.Open(new Uri(filePath, UriKind.Absolute));
            _bgmPlayer.Volume = MasterVolume * BgmVolume;
            _bgmPlayer.Play();
            CurrentBgmId = bgmId;
            IsBgmPlaying = true;
        }
        catch
        {
            // 再生失敗時は無視（音がなくてもゲームに影響なし）
        }
    }

    public void StopBgm()
    {
        _bgmPlayer.Stop();
        _bgmPlayer.Close();
        CurrentBgmId = null;
        IsBgmPlaying = false;
    }

    /// <summary>
    /// β.8: BGMをフェードアウトしてから別のBGMをフェードインする
    /// </summary>
    public void FadeToBgm(string newBgmId, bool loop = true, double fadeOutMs = 800, double fadeInMs = 800)
    {
        if (CurrentBgmId == newBgmId && IsBgmPlaying) return;

        double targetVolume = MasterVolume * BgmVolume;
        double startVolume = IsBgmPlaying ? _bgmPlayer.Volume : 0.0;
        int steps = 20;
        double stepMs = fadeOutMs / steps;
        double volumeStep = startVolume / steps;
        int step = 0;

        var fadeOutTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(stepMs)
        };
        fadeOutTimer.Tick += (_, _) =>
        {
            step++;
            double newVol = Math.Max(0.0, startVolume - volumeStep * step);
            _bgmPlayer.Volume = newVol;

            if (step >= steps)
            {
                fadeOutTimer.Stop();
                StopBgm();

                // フェードイン
                var filePath = FindAudioFile(_bgmDirectory, newBgmId);
                if (filePath == null) return;
                try
                {
                    _bgmLoop = loop;
                    _bgmPlayer.Open(new Uri(filePath, UriKind.Absolute));
                    _bgmPlayer.Volume = 0.0;
                    _bgmPlayer.Play();
                    CurrentBgmId = newBgmId;
                    IsBgmPlaying = true;

                    int inStep = 0;
                    double inStepMs = fadeInMs / steps;
                    double inVolumeStep = targetVolume / steps;
                    var fadeInTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(inStepMs)
                    };
                    fadeInTimer.Tick += (_, _) =>
                    {
                        inStep++;
                        _bgmPlayer.Volume = Math.Min(targetVolume, inVolumeStep * inStep);
                        if (inStep >= steps) fadeInTimer.Stop();
                    };
                    fadeInTimer.Start();
                }
                catch { }
            }
        };
        fadeOutTimer.Start();
    }

    public void PauseBgm()
    {
        if (IsBgmPlaying)
        {
            _bgmPlayer.Pause();
            IsBgmPlaying = false;
        }
    }

    public void ResumeBgm()
    {
        if (CurrentBgmId != null && !IsBgmPlaying)
        {
            _bgmPlayer.Play();
            IsBgmPlaying = true;
        }
    }

    public void PlaySe(string seId)
    {
        // 同一SEの連続再生を抑制
        if (_lastSePlayTimes.TryGetValue(seId, out var lastTime))
        {
            if ((DateTime.Now - lastTime).TotalMilliseconds < SeMinIntervalMs)
                return;
        }

        var filePath = FindAudioFile(_seDirectory, seId);
        if (filePath == null) return;

        try
        {
            // 再利用可能なプレイヤーを探す
            var player = GetAvailableSePlayer();
            player.Open(new Uri(filePath, UriKind.Absolute));
            player.Volume = MasterVolume * SeVolume;
            player.Play();
            _lastSePlayTimes[seId] = DateTime.Now;
        }
        catch
        {
            // 再生失敗時は無視
        }
    }

    public void StopAll()
    {
        StopBgm();
        foreach (var player in _sePlayers)
        {
            player.Stop();
            player.Close();
        }
    }

    public void ApplyVolumeSettings(float masterVolume, float bgmVolume, float seVolume)
    {
        MasterVolume = Math.Clamp(masterVolume, 0f, 1f);
        BgmVolume = Math.Clamp(bgmVolume, 0f, 1f);
        SeVolume = Math.Clamp(seVolume, 0f, 1f);

        // BGM音量を即時反映
        if (IsBgmPlaying)
        {
            _bgmPlayer.Volume = MasterVolume * BgmVolume;
        }
    }

    private void BgmPlayer_MediaEnded(object? sender, EventArgs e)
    {
        if (_bgmLoop)
        {
            _bgmPlayer.Position = TimeSpan.Zero;
            _bgmPlayer.Play();
        }
        else
        {
            IsBgmPlaying = false;
        }
    }

    private MediaPlayer GetAvailableSePlayer()
    {
        // 既存の停止中プレイヤーを再利用
        foreach (var player in _sePlayers)
        {
            if (!player.HasAudio || player.Position >= player.NaturalDuration.TimeSpan)
            {
                return player;
            }
        }

        // 上限に達していなければ新規作成
        if (_sePlayers.Count < MaxSimultaneousSe)
        {
            var player = new MediaPlayer();
            _sePlayers.Add(player);
            return player;
        }

        // 上限に達している場合は最も古いものを再利用
        var oldest = _sePlayers[0];
        oldest.Stop();
        oldest.Close();
        return oldest;
    }

    /// <summary>
    /// ディレクトリ内からIDに一致するオーディオファイルを探す
    /// </summary>
    private static string? FindAudioFile(string directory, string id)
    {
        if (!Directory.Exists(directory)) return null;

        string[] extensions = [".mp3", ".ogg", ".wav"];
        foreach (var ext in extensions)
        {
            var path = Path.Combine(directory, id + ext);
            if (File.Exists(path)) return path;
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopAll();
        _bgmPlayer.MediaEnded -= BgmPlayer_MediaEnded;
        // EF-1: BGMプレイヤーのリソース解放
        _bgmPlayer.Close();
        // EF-2: SEプレイヤーのリソース解放
        foreach (var player in _sePlayers)
        {
            player.Close();
        }
        _sePlayers.Clear();
        GC.SuppressFinalize(this);
    }
}
