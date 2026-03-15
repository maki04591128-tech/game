using System.Windows;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Gui.Audio;

namespace RougelikeGame.Gui;

/// <summary>
/// アプリケーションエントリーポイント
/// タイトル画面を表示し、選択に応じてメインウィンドウを起動する
/// 
/// コマンドライン引数:
///   --skip-title  : タイトル画面をスキップして即座にニューゲーム開始
///   --debug-map   : デバッグ用小マップ（15x10、固定シード、敵1体）で起動
/// </summary>
public partial class App : Application
{
    private IAudioManager? _audioManager;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var args = e.Args;
        bool skipTitle = Array.Exists(args, a => a == "--skip-title");
        bool debugMap = Array.Exists(args, a => a == "--debug-map");

        // 設定読み込み
        var settings = GameSettings.Load();

        // AudioManager初期化（音声ファイルが無くても動作する）
        _audioManager = CreateAudioManager(settings);

        if (skipTitle)
        {
            // タイトル画面をスキップして即座にニューゲーム
            StartMainWindow(settings, loadSave: false, debugMap: debugMap);
            return;
        }

        // タイトル画面表示
        var titleWindow = new TitleWindow();
        var result = titleWindow.ShowDialog();

        if (result != true)
        {
            Shutdown();
            return;
        }

        switch (titleWindow.SelectedAction)
        {
            case TitleAction.NewGame:
                StartMainWindow(settings, loadSave: false, debugMap: debugMap,
                    playerName: titleWindow.PlayerName,
                    playerRace: titleWindow.SelectedRace,
                    playerClass: titleWindow.SelectedClass,
                    playerBackground: titleWindow.SelectedBackground,
                    difficulty: titleWindow.SelectedDifficulty);
                break;
            case TitleAction.Continue:
                StartMainWindow(settings, loadSave: true, debugMap: debugMap,
                    saveSlot: titleWindow.SelectedSaveSlot);
                break;
            case TitleAction.DebugMap:
                StartMainWindow(settings, loadSave: false, debugMap: true);
                break;
            default:
                Shutdown();
                break;
        }
    }

    private void StartMainWindow(GameSettings settings, bool loadSave, bool debugMap = false,
        string playerName = "冒険者", Race playerRace = Race.Human,
        RougelikeGame.Core.CharacterClass playerClass = RougelikeGame.Core.CharacterClass.Fighter,
        RougelikeGame.Core.Background playerBackground = RougelikeGame.Core.Background.Adventurer,
        DifficultyLevel difficulty = DifficultyLevel.Normal,
        int saveSlot = 0)
    {
        var mainWindow = new MainWindow(settings, _audioManager!, loadSave, debugMap,
            playerName, playerRace, playerClass, playerBackground, difficulty, saveSlot);
        mainWindow.Show();
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
}

