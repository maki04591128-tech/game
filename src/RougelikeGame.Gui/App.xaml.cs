using System.Windows;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Gui.Audio;

namespace RougelikeGame.Gui;

/// <summary>
/// アプリケーションエントリーポイント
/// タイトル画面を表示し、選択に応じてメインウィンドウを起動する
/// ゲームオーバー/ゲームクリア時にタイトル画面に戻るループをサポート
/// 
/// コマンドライン引数:
///   --skip-title  : タイトル画面をスキップして即座にニューゲーム開始
///   --debug-map   : デバッグ用小マップ（15x10、固定シード、敵1体）で起動
///   --race &lt;Race&gt;        : 種族を指定（--skip-title使用時）
///   --class &lt;Class&gt;      : 職業を指定（--skip-title使用時）
///   --background &lt;Bg&gt;    : 素性を指定（--skip-title使用時）
/// </summary>
public partial class App : Application
{
    private IAudioManager? _audioManager;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var args = e.Args;
        bool skipTitle = Array.Exists(args, a => a == "--skip-title");
        bool debugMap = Array.Exists(args, a => a == "--debug-map");

        // コマンドライン引数からキャラクター設定を取得
        var cliRace = GetArgValue(args, "--race");
        var cliClass = GetArgValue(args, "--class");
        var cliBackground = GetArgValue(args, "--background");

        Race playerRace = cliRace != null && Enum.TryParse<Race>(cliRace, true, out var r) ? r : Race.Human;
        var playerClass = cliClass != null && Enum.TryParse<RougelikeGame.Core.CharacterClass>(cliClass, true, out var c)
            ? c : RougelikeGame.Core.CharacterClass.Fighter;
        var playerBackground = cliBackground != null && Enum.TryParse<RougelikeGame.Core.Background>(cliBackground, true, out var b)
            ? b : RougelikeGame.Core.Background.Adventurer;

        // 設定読み込み
        var settings = GameSettings.Load();

        // AudioManager初期化（音声ファイルが無くても動作する）
        _audioManager = CreateAudioManager(settings);

        if (skipTitle)
        {
            // タイトル画面をスキップして即座にニューゲーム
            RunMainWindow(settings, loadSave: false, debugMap: debugMap,
                playerRace: playerRace, playerClass: playerClass, playerBackground: playerBackground);
            Shutdown();
            return;
        }

        // メインゲームループ（タイトル→ゲーム→タイトルに戻れる）
        RunGameLoop(settings, debugMap);
    }

    /// <summary>
    /// タイトル画面⇔ゲーム画面のメインループ。
    /// ゲームオーバー/クリア時に「タイトルに戻る」を選択した場合、再びタイトル画面を表示する。
    /// </summary>
    private void RunGameLoop(GameSettings settings, bool debugMap)
    {
        while (true)
        {
            // タイトル画面表示
            var titleWindow = new TitleWindow();
            var result = titleWindow.ShowDialog();

            if (result != true)
            {
                Shutdown();
                return;
            }

            GameExitReason exitReason;

            switch (titleWindow.SelectedAction)
            {
                case TitleAction.NewGame:
                    exitReason = RunMainWindow(settings, loadSave: false, debugMap: debugMap,
                        playerName: titleWindow.PlayerName,
                        playerRace: titleWindow.SelectedRace,
                        playerClass: titleWindow.SelectedClass,
                        playerBackground: titleWindow.SelectedBackground,
                        difficulty: titleWindow.SelectedDifficulty);
                    break;
                case TitleAction.Continue:
                    exitReason = RunMainWindow(settings, loadSave: true, debugMap: debugMap,
                        saveSlot: titleWindow.SelectedSaveSlot);
                    break;
                case TitleAction.DebugMap:
                    exitReason = RunMainWindow(settings, loadSave: false, debugMap: true);
                    break;
                default:
                    Shutdown();
                    return;
            }

            // MainWindowの終了理由に応じて処理を分岐
            switch (exitReason)
            {
                case GameExitReason.ReturnToTitle:
                    // ループ継続 → 再びタイトル画面を表示
                    continue;

                case GameExitReason.StartNewGamePlus:
                    // NG+を開始 → 同じキャラ設定で再スタート（タイトル画面をスキップ）
                    // NG+開始後のゲーム終了時もタイトル戻りを許可
                    continue;

                case GameExitReason.Quit:
                default:
                    Shutdown();
                    return;
            }
        }
    }

    /// <summary>
    /// MainWindowを起動して終了を待つ。終了理由を返す。
    /// </summary>
    private GameExitReason RunMainWindow(GameSettings settings, bool loadSave, bool debugMap = false,
        string playerName = "冒険者", Race playerRace = Race.Human,
        RougelikeGame.Core.CharacterClass playerClass = RougelikeGame.Core.CharacterClass.Fighter,
        RougelikeGame.Core.Background playerBackground = RougelikeGame.Core.Background.Adventurer,
        DifficultyLevel difficulty = DifficultyLevel.Normal,
        int saveSlot = 0,
        NewGamePlusTier? ngPlusTier = null)
    {
        var mainWindow = new MainWindow(settings, _audioManager!, loadSave, debugMap,
            playerName, playerRace, playerClass, playerBackground, difficulty, saveSlot);
        mainWindow.ShowDialog();

        var exitReason = mainWindow.ExitReason;

        // NG+開始の場合は即座に新しいMainWindowを起動
        if (exitReason == GameExitReason.StartNewGamePlus && mainWindow.NgPlusTier.HasValue)
        {
            var ngTier = mainWindow.NgPlusTier.Value;
            var ngMainWindow = new MainWindow(settings, _audioManager!, loadSave: false, debugMap: false,
                playerName, playerRace, playerClass, playerBackground, difficulty,
                ngPlusTier: ngTier);
            ngMainWindow.ShowDialog();

            // NG+のMainWindow終了理由をそのまま返す
            return ngMainWindow.ExitReason;
        }

        return exitReason;
    }

    private static IAudioManager CreateAudioManager(GameSettings settings)
    {
        var manager = new SilentAudioManager();
        manager.ApplyVolumeSettings(settings.MasterVolume, settings.BgmVolume, settings.SeVolume);
        return manager;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _audioManager?.Dispose();
        base.OnExit(e);
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }
}

