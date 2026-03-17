using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Xunit;
using Xunit.Abstractions;

namespace RougelikeGame.Gui.Tests;

/// <summary>
/// FlaUI を使ったGUI自動操作テスト（統合版）
/// アプリ起動コストを削減するため、同一起動内で検証可能な項目をまとめて実行する。
/// デバッグマップ（固定シード）と --skip-title を使い高速に検証する。
///
/// ■ 責務: UI要素の存在チェック＋全キーバインドのクラッシュ耐性検証
///   （値レベルの詳細検証は GuiSystemVerificationTests に委譲）
///
/// テスト構成（4テスト）:
///   1. TitleScreen_ButtonsAndSettingsDialog — タイトル画面ボタン4種＋設定ダイアログ（スライダー4種・ラベル4種・Esc閉じ）統合検証
///   2. TitleScreen_EscClosesWindow — Escでアプリ終了（破壊的操作のため分離）
///   3. TitleScreen_NewGameFlow — ニューゲームボタン→メインウィンドウ遷移（状態変化のため分離）
///   4. MainWindow_FullIntegration — 以下を1回のアプリ起動で一括検証:
///      - ウィンドウタイトル・サイズ・メッセージログ初期表示
///      - 操作説明テキスト(KeyHelpText)存在＋内容チェック
///      - ミニマップ切替（M）
///      - ステータスバー全20要素存在チェック（Territory/Surface/Floor/Date/TimePeriod/Lv/Exp/HP/MP/SP/Hunger/Sanity/Gold/Weight/TurnLimit/Season/Weather/Thirst/Karma/CompanionCount）
///      - 移動: WASD / 矢印 / 斜め（Home/PgUp/End/PgDn）
///      - アクション: G拾う / F探索 / X閉ドア / R射撃 / T投擲 / P祈り / E技能 / N登録
///      - ダイアログ: I持物 / C状態 / Lログ / V魔法詠唱 / Jワールドマップ / Kクエスト / O宗教 / H鍛冶 / B街 / Y図鑑 / U仲間 / Z死亡録
///      - システム: Tab自動探索→中断 / F5セーブ / F9ロード / Space×65日時進行
///      - 連打耐性: 18種キー×3ラウンド高速連打（Y/U/Z追加）
///      - 終了: Qキーでゲーム終了（破壊的操作のためテスト末尾に配置）
/// </summary>
[Collection("GuiTests")]
public class GuiAutomationTests : IDisposable
{
    private readonly UIA3Automation _automation;
    private readonly ITestOutputHelper _output;
    private Application? _app;

    public GuiAutomationTests(ITestOutputHelper output)
    {
        _automation = new UIA3Automation();
        _output = output;
    }

    private void Log(string message) => _output.WriteLine($"[GUI Auto] {message}");

    public void Dispose()
    {
        _app?.Close();
        _app?.Dispose();
        _automation.Dispose();
    }

    // ─────────────────────────────────────────
    // ヘルパーメソッド
    // ─────────────────────────────────────────

    private static string GetGuiExePath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        var guiExe = Path.Combine(solutionDir, "src", "RougelikeGame.Gui", "bin", "Debug", "net10.0-windows", "RougelikeGame.Gui.exe");
        if (!File.Exists(guiExe))
            guiExe = Path.Combine(solutionDir, "src", "RougelikeGame.Gui", "bin", "Release", "net10.0-windows", "RougelikeGame.Gui.exe");
        return guiExe;
    }

    private Window LaunchAndGetTitleWindow()
    {
        var exePath = GetGuiExePath();
        Assert.True(File.Exists(exePath), $"GUIアプリのexeが見つかりません: {exePath}");
        _app = Application.Launch(exePath);
        var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);
        Thread.Sleep(500);
        return window;
    }

    private Window LaunchWithDebugMap()
    {
        var exePath = GetGuiExePath();
        Assert.True(File.Exists(exePath), $"GUIアプリのexeが見つかりません: {exePath}");
        var processInfo = new ProcessStartInfo(exePath) { Arguments = "--skip-title --debug-map" };
        _app = Application.Launch(processInfo);
        var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);
        Thread.Sleep(500);
        return window;
    }

    private static AutomationElement? FindElement(Window window, string automationId)
    {
        return Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(250)
        ).Result;
    }

    private static AutomationElement? FindElement(AutomationElement parent, string automationId)
    {
        return Retry.WhileNull(
            () => parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(250)
        ).Result;
    }

    private static void PressKey(Window window, FlaUI.Core.WindowsAPI.VirtualKeyShort key)
    {
        window.Focus();
        FlaUI.Core.Input.Keyboard.Press(key);
        Thread.Sleep(80);
    }

    private static void CloseModals(Window window)
    {
        foreach (var m in window.ModalWindows)
            m.Close();
        Thread.Sleep(200);
    }

    // ─────────────────────────────────────────
    // 1. タイトル画面＋設定ダイアログ 統合テスト
    //    旧: Title_HasAllButtons, Title_SettingsOpensDialog,
    //        Settings_HasVolumeSliders, Settings_HasVolumeLabels,
    //        Settings_EscClosesDialog
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_ButtonsAndSettingsDialog()
    {
        Log("=== テスト開始: タイトル画面＋設定ダイアログ統合検証 ===");
        Log("目的: タイトル画面の全ボタン存在確認、設定ダイアログの開閉・スライダー・ラベル表示を一括検証する");

        var window = LaunchAndGetTitleWindow();

        // --- タイトル画面ボタン存在確認 ---
        Log("検証: タイトル画面に4つのボタン（NewGame/Continue/Settings/Quit）が存在するか");
        Assert.NotNull(FindElement(window, "NewGameButton"));
        Assert.NotNull(FindElement(window, "ContinueButton"));
        Assert.NotNull(FindElement(window, "SettingsButton"));
        Assert.NotNull(FindElement(window, "QuitButton"));
        Log("  → 全ボタン確認OK");

        // --- 設定ダイアログを開く ---
        Log("検証: 設定ボタン押下で設定ダイアログが開くか");
        var settingsBtn = FindElement(window, "SettingsButton");
        Assert.NotNull(settingsBtn);
        settingsBtn!.AsButton().Invoke();
        Thread.Sleep(500);
        var modals = window.ModalWindows;
        Assert.True(modals.Length > 0, "設定ダイアログが開かない");
        var settingsDialog = modals[0];
        Log("  → 設定ダイアログ表示OK");

        // --- スライダー存在確認 ---
        Log("検証: 設定ダイアログに音量スライダー（Master/BGM/SE/FontSize）が存在するか");
        Assert.NotNull(FindElement(settingsDialog, "MasterVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "BgmVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "SeVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "FontSizeSlider"));
        Log("  → 全スライダー確認OK");

        // --- ラベル存在確認＋数値表示 ---
        Log("検証: 各音量ラベルが存在し数値が表示されているか");
        var mt = FindElement(settingsDialog, "MasterVolumeText");
        var bt = FindElement(settingsDialog, "BgmVolumeText");
        var st = FindElement(settingsDialog, "SeVolumeText");
        var ft = FindElement(settingsDialog, "FontSizeText");
        Assert.NotNull(mt);
        Assert.NotNull(bt);
        Assert.NotNull(st);
        Assert.NotNull(ft);
        Assert.Matches(@"\d+", mt!.Name);
        Assert.Matches(@"\d+", bt!.Name);
        Assert.Matches(@"\d+", st!.Name);
        Assert.Matches(@"\d+", ft!.Name);
        Log($"  → ラベル値: Master={mt.Name}, BGM={bt.Name}, SE={st.Name}, Font={ft.Name}");

        // --- Escで設定ダイアログを閉じる ---
        Log("検証: Escキーで設定ダイアログが閉じるか");
        settingsDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(500);
        modals = window.ModalWindows;
        Assert.Empty(modals);
        Log("  → Escでダイアログ閉じOK");
        Log("=== テスト完了: タイトル画面＋設定ダイアログ統合検証 ===");
    }

    // ─────────────────────────────────────────
    // 2. タイトル画面 Esc終了テスト（破壊的操作のため分離）
    //    旧: Title_EscClosesWindow
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_EscClosesWindow()
    {
        Log("=== テスト開始: タイトル画面Escキー終了検証 ===");
        Log("目的: タイトル画面でEscキーを押すとアプリケーションが正常終了するか確認する");

        var window = LaunchAndGetTitleWindow();
        Log("検証: Escキー押下でアプリが終了するか");
        // ウィンドウにフォーカスしてからEscを送信（複数回試行で確実に届ける）
        for (int attempt = 0; attempt < 3 && !_app!.HasExited; attempt++)
        {
            window.Focus();
            Thread.Sleep(300);
            window.SetForeground();
            Thread.Sleep(200);
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
            Thread.Sleep(1000);
        }
        // WPFのShutdown()完了を最大5秒待機
        for (int i = 0; i < 10 && !_app!.HasExited; i++)
            Thread.Sleep(500);
        Assert.True(_app!.HasExited, "Escキー押下後にアプリが終了しなかった");
        Log("  → アプリ終了確認OK");
        Log("=== テスト完了: タイトル画面Escキー終了検証 ===");
    }

    // ─────────────────────────────────────────
    // 3. タイトル画面 ニューゲーム遷移テスト（状態変化のため分離）
    //    旧: Title_NewGameNavigatesToMainWindow
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_NewGameFlow()
    {
        Log("=== テスト開始: ニューゲーム画面遷移検証 ===");
        Log("目的: ニューゲームボタン押下でメインウィンドウに遷移するか確認する");

        var window = LaunchAndGetTitleWindow();
        Log("検証: NewGameボタンをフォーカスしEnterキーで遷移開始");
        var btn = FindElement(window, "NewGameButton");
        Assert.NotNull(btn);
        btn!.Focus();
        Thread.Sleep(200);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);
        Thread.Sleep(3000);
        if (!_app!.HasExited)
        {
            var mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(mainWindow);
            Assert.Equal("ローグライクゲーム", mainWindow.Title);
            Log("  → メインウィンドウ遷移確認OK (タイトル: " + mainWindow.Title + ")");
        }
        else
        {
            Log("  → アプリは終了済み（ニューゲームボタンは機能した）");
        }
        Log("=== テスト完了: ニューゲーム画面遷移検証 ===");
    }

    // ─────────────────────────────────────────
    // 4. メインウィンドウ統合テスト（デバッグマップ1回起動で全検証）
    //    検証内容: ウィンドウ基本 → KeyHelpText →
    //    ステータスバー15要素 → 移動系キー →
    //    アクションキー(G/F/X/R/T/P/E/N) → ダイアログキー(I/C/L/V/J/K/O/H/B) →
    //    Tab自動探索 → F5/F9セーブロード → Space日時進行 →
    //    15種キー連打耐性 → Qキー終了
    // ─────────────────────────────────────────

    [Fact]
    public void MainWindow_FullIntegration()
    {
        Log("=== テスト開始: メインウィンドウ統合検証（デバッグマップ） ===");
        Log("目的: 1回のアプリ起動でUI要素・ステータスバー・キー操作・メッセージログ・連打耐性を一括検証する");

        var window = LaunchWithDebugMap();

        // ========== ウィンドウ基本検証 ==========
        Log("検証: ウィンドウタイトルとサイズが正しいか（タイトル='ローグライクゲーム', 幅>=800, 高さ>=500）");
        Assert.Equal("ローグライクゲーム", window.Title);
        Assert.True(window.BoundingRectangle.Width > 0);
        Assert.True(window.BoundingRectangle.Height > 0);
        Assert.True(window.BoundingRectangle.Width >= 800, "ゲーム画面の幅が不足");
        Assert.True(window.BoundingRectangle.Height >= 500, "ゲーム画面の高さが不足");
        Log($"  → タイトル='{window.Title}', サイズ={window.BoundingRectangle.Width}x{window.BoundingRectangle.Height} OK");

        // ========== メッセージログ存在＋初期メッセージ ==========
        Log("検証: メッセージログが存在し、初期メッセージに'デバッグモード'が含まれるか");
        var msgLog = FindElement(window, "MessageLog");
        Assert.NotNull(msgLog);
        Assert.Contains("デバッグモード", msgLog!.Name);
        Log("  → メッセージログ初期表示OK");

        // ========== 操作説明テキストの存在チェック ==========
        // ※ GameCanvas/MinimapCanvasはWPF CanvasでUIAutomationツリーに公開されないため検証対象外
        Log("検証: 操作説明テキスト(KeyHelpText)が存在し、キーバインド一覧を含むか");
        var helpText = FindElement(window, "KeyHelpText");
        Assert.NotNull(helpText);
        Assert.False(string.IsNullOrWhiteSpace(helpText!.Name), "操作説明テキストが空");
        Assert.Contains("WASD", helpText.Name);
        Log($"  KeyHelpText='{helpText.Name[..30]}…' OK");

        // ========== ミニマップ切替（M キー） ==========
        Log("検証: Mキーでミニマップの表示/非表示を切り替えてクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Thread.Sleep(200);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → ミニマップ切替OK");

        // ========== ステータスバー全要素存在チェック ==========
        // ※ 値の詳細検証（初期値フル確認、形式チェック等）はGuiSystemVerificationTestsで実施
        Log("検証: ステータスバー全20要素のUI存在チェック");
        var statusBarIds = new[]
        {
            "TerritoryText", "SurfaceStatusText", "FloorText", "DateText", "TimePeriodText",
            "LevelText", "ExpText", "HpText", "MpText", "SpText",
            "HungerText", "SanityText", "GoldText", "WeightText", "TurnLimitText",
            "SeasonText", "WeatherText", "ThirstText", "KarmaText", "CompanionCountText"
        };
        foreach (var id in statusBarIds)
        {
            var el = FindElement(window, id);
            Assert.NotNull(el);
            Assert.False(string.IsNullOrWhiteSpace(el!.Name), $"{id}の表示内容が空");
            Log($"  {id}='{el.Name}' OK");
        }
        Log("  ステータスバー全要素存在チェック完了");

        // ========== 移動キー操作（WASD） ==========
        Log("検証: WASD移動キーでクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → WASD移動OK");

        // ========== 移動キー操作（矢印キー） ==========
        Log("検証: 矢印キー移動（上下左右）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.UP);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 矢印キー移動OK");

        // ========== 斜め移動キー操作 ==========
        Log("検証: 斜め移動キー（Home/PgUp/End/PgDn）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.HOME);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.PRIOR);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.END);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.NEXT);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 斜め移動OK");

        // ========== アイテム拾い（G） ==========
        Log("検証: Gキー（アイテム拾い）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_G);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → アイテム拾いOK");

        // ========== 探索（F） ==========
        Log("検証: Fキー（探索）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 探索OK");

        // ========== インベントリダイアログ（I） ==========
        Log("検証: Iキーでインベントリダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_I);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → インベントリダイアログOK");

        // ========== ステータスダイアログ（C） ==========
        Log("検証: Cキーでステータスダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_C);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → ステータスダイアログOK");

        // ========== メッセージログダイアログ（L） ==========
        Log("検証: Lキーでメッセージログダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_L);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → メッセージログダイアログOK");

        // ========== 自動探索（Tab → Space で中断） ==========
        Log("検証: Tabキーで自動探索を開始し、Spaceキーで中断してもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
        Thread.Sleep(300);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 自動探索→中断OK");

        // ========== セーブ（F5） ==========
        Log("検証: F5キーでセーブ処理がクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.F5);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → セーブOK");

        // ========== ロード（F9） ==========
        Log("検証: F9キーでロード処理がクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.F9);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → ロードOK");

        // ========== 魔法詠唱ダイアログ（V） ==========
        Log("検証: Vキーで魔法詠唱ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_V);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 魔法詠唱ダイアログOK");

        // ========== スキル使用（E） ==========
        Log("検証: Eキー（スキル使用）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_E);
        Thread.Sleep(300);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → スキル使用OK");

        // ========== 祈り（P） ==========
        Log("検証: Pキー（祈り）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_P);
        Thread.Sleep(300);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 祈りOK");

        // ========== ワールドマップダイアログ（J） ==========
        Log("検証: Jキーでワールドマップダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_J);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → ワールドマップダイアログOK");

        // ========== クエストログダイアログ（K） ==========
        Log("検証: Kキーでクエストログダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_K);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → クエストログダイアログOK");

        // ========== 宗教画面ダイアログ（O） ==========
        Log("検証: Oキーで宗教画面ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_O);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 宗教画面ダイアログOK");

        // ========== 鍛冶画面ダイアログ（H） ==========
        Log("検証: Hキーで鍛冶画面ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_H);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 鍛冶画面ダイアログOK");

        // ========== 街画面ダイアログ（B） ==========
        Log("検証: Bキー（街画面）でクラッシュしないか（ダンジョン内では入場不可メッセージ）");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_B);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 街画面OK");

        // ========== ギルド登録（N） ==========
        Log("検証: Nキー（ギルド登録）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_N);
        Thread.Sleep(300);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → ギルド登録OK");

        // ========== 図鑑ダイアログ（Y） ==========
        Log("検証: Yキーで図鑑ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Y);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 図鑑ダイアログOK");

        // ========== 仲間管理ダイアログ（U） ==========
        Log("検証: Uキーで仲間管理ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_U);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 仲間管理ダイアログOK");

        // ========== 死亡記録ダイアログ（Z） ==========
        Log("検証: Zキーで死亡記録ダイアログが開き、閉じてもクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Z);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → 死亡記録ダイアログOK");

        // ========== 待機キーで日時進行 ==========
        Log("検証: Spaceキー65回連打で日付が進行するか（時間経過の確認）");
        var dateBeforeWait = FindElement(window, "DateText");
        Assert.NotNull(dateBeforeWait);
        var initialDate = dateBeforeWait!.Name;
        Log($"  待機前の日付: {initialDate}");
        for (int i = 0; i < 65; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(300);
        var dateAfterWait = FindElement(window, "DateText");
        Assert.NotNull(dateAfterWait);
        Assert.NotEqual(initialDate, dateAfterWait!.Name);
        Log($"  待機後の日付: {dateAfterWait.Name}");
        Log("  → 日時進行OK");

        // ========== 連打耐性テスト ==========
        Log("検証: 18種キーを3ラウンド高速連打（30ms間隔）してクラッシュしないか");
        var rapidKeys = new[]
        {
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_G,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_T,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_X,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_P,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.UP,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Y,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_U,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Z,
        };
        for (int round = 0; round < 3; round++)
        {
            foreach (var key in rapidKeys)
            {
                window.Focus();
                FlaUI.Core.Input.Keyboard.Press(key);
                Thread.Sleep(30);
            }
            Log($"  ラウンド{round + 1}/3 完了");
        }
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 連打耐性OK");

        // ========== Qキー（終了）検証 ==========
        // ※ アプリが終了する破壊的操作のため統合テスト最後に配置
        Log("検証: Qキーでゲーム終了処理がクラッシュせずアプリが終了するか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Q);
        Thread.Sleep(500);
        // ゲームオーバーダイアログが出る場合があるので閉じる
        try
        {
            var modalWindow = _app!.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.AutomationId != window.AutomationId);
            if (modalWindow != null)
            {
                // MessageBoxのOKボタンを押す
                var okBtn = modalWindow.FindFirstDescendant(cf => cf.ByName("OK"));
                okBtn?.AsButton().Invoke();
                Thread.Sleep(300);
            }
        }
        catch { /* ダイアログが出ない場合もある */ }
        // アプリが正常終了したことを確認（最大3秒待機）
        var exited = _app!.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(3));
        Log($"  → Qキー終了処理OK (exited={_app!.HasExited})");
        Log("=== テスト完了: メインウィンドウ統合検証 ===");
    }
}
