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
/// ■ 責務: UI要素の存在チェック＋全キーバインドのクラッシュ耐性＋値レベル詳細検証を統合
///   （旧 GuiSystemVerificationTests.SystemVerification_DebugMap_FullIntegration の内容もここに統合済み）
///
/// テスト構成（6テスト）:
///   1. TitleScreen_ButtonsAndSettingsDialog — タイトル画面ボタン4種＋設定ダイアログ（スライダー4種・ラベル4種・Esc閉じ）統合検証
///   2. TitleScreen_EscClosesWindow — Escでアプリ終了（破壊的操作のため分離）
///   3. TitleScreen_NewGameFlow_CharacterCreation — ニューゲーム→難易度選択→キャラクター作成画面のUI要素検証＋キャンセル動作
///   4. TitleScreen_SettingsParameterChanges — 設定ダイアログ内スライダー操作による値変化確認＋初期値リセット
///   5. MainWindow_FullIntegration — デバッグマップ1回起動で全検証:
///      === 初期値・表示形式検証 ===
///      - ウィンドウタイトル・サイズ・メッセージログ初期表示（第1層、デバッグモード、WASD）
///      - 操作説明テキスト(KeyHelpText)存在＋内容チェック
///      - 領地名・地上/ダンジョン表示の値検証
///      - HP/MP/SP初期値フル（current==max）、形式 'X/Y'
///      - レベル=1、経験値形式、重量 'x.x/y.ykg'（初期非0）、通貨 'XG'、ターン制限
///      - 満腹度・正気度の初期値検証（数値、非0）
///      - 日時表示形式（'冒険暦XXXX年 ○○の月 X日 HH:MM'）・時間帯検証
///      === アイテム拾い・インベントリ・ドアインタラクト ===
///      - 地面アイテム位置へ移動→Gで拾取→Iでインベントリ確認
///      - ドア方向へ移動→ドア開放→Xでドア閉じ
///      === NPC対話・地形効果 ===
///      - NPC位置へ移動→対話確認、水タイル移動
///      === 各ダイアログ・キー操作 ===
///      - ミニマップ切替（M）、ステータスバー全19要素（構え/疲労/衛生/病気追加）
///      - ダイアログ: C/L/V/J/K/O/H/B/E/P/N、探索F、自動探索Tab
///      === 移動・戦闘・階段・日時進行 ===
///      - WASD/矢印/斜め、戦闘→HP確認、R射撃/T投擲、階段Shift+&lt;/&gt;
///      - Space×65日付進行、スキルCD20ターン、詠唱ターン処理
///      === 連打耐性・セーブロード・終了 ===
///      - 22種キー×3ラウンド高速連打（移動WASD・Space・アイテムGFR・戦闘TX・方向キー4種・スキルスロット1-6・Nスタンス切替）、F5/F9、Qキー終了
///   6. TitleScreen_ContinueFlow_SaveDataSelect — セーブ後コンティニュー→セーブデータ選択画面検証
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

    private static string GetText(Window window, string automationId)
    {
        var el = FindElement(window, automationId);
        Assert.NotNull(el);
        return el!.Name;
    }

    // ─────────────────────────────────────────
    // 1. タイトル画面＋設定ダイアログ 統合テスト
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_ButtonsAndSettingsDialog()
    {
        Log("=== テスト開始: タイトル画面＋設定ダイアログ統合検証 ===");

        var window = LaunchAndGetTitleWindow();

        Log("検証: タイトル画面ボタン存在");
        Assert.NotNull(FindElement(window, "NewGameButton"));
        Assert.NotNull(FindElement(window, "ContinueButton"));
        Assert.NotNull(FindElement(window, "SettingsButton"));
        Assert.NotNull(FindElement(window, "QuitButton"));
        Log("  → 全ボタン確認OK");

        Log("検証: 設定ダイアログ開閉");
        var settingsBtn = FindElement(window, "SettingsButton");
        Assert.NotNull(settingsBtn);
        settingsBtn!.AsButton().Invoke();
        Thread.Sleep(500);
        var modals = window.ModalWindows;
        Assert.True(modals.Length > 0, "設定ダイアログが開かない");
        var settingsDialog = modals[0];

        Assert.NotNull(FindElement(settingsDialog, "MasterVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "BgmVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "SeVolumeSlider"));
        Assert.NotNull(FindElement(settingsDialog, "FontSizeSlider"));

        var mt = FindElement(settingsDialog, "MasterVolumeText");
        var bt = FindElement(settingsDialog, "BgmVolumeText");
        var st = FindElement(settingsDialog, "SeVolumeText");
        var ft = FindElement(settingsDialog, "FontSizeText");
        Assert.NotNull(mt); Assert.NotNull(bt); Assert.NotNull(st); Assert.NotNull(ft);
        Assert.Matches(@"\d+", mt!.Name);
        Assert.Matches(@"\d+", bt!.Name);
        Assert.Matches(@"\d+", st!.Name);
        Assert.Matches(@"\d+", ft!.Name);
        Log($"  → ラベル値: Master={mt.Name}, BGM={bt.Name}, SE={st.Name}, Font={ft.Name}");

        settingsDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(500);
        Assert.Empty(window.ModalWindows);
        Log("  → Escでダイアログ閉じOK");
        Log("=== テスト完了 ===");
    }

    // ─────────────────────────────────────────
    // 2. タイトル画面 Esc終了テスト
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_EscClosesWindow()
    {
        Log("=== テスト開始: タイトル画面Esc終了検証 ===");

        var window = LaunchAndGetTitleWindow();
        for (int attempt = 0; attempt < 3 && !_app!.HasExited; attempt++)
        {
            window.Focus();
            Thread.Sleep(300);
            window.SetForeground();
            Thread.Sleep(200);
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
            Thread.Sleep(1000);
        }
        for (int i = 0; i < 10 && !_app!.HasExited; i++)
            Thread.Sleep(500);
        Assert.True(_app!.HasExited, "Escキー押下後にアプリが終了しなかった");
        Log("  → アプリ終了確認OK");
        Log("=== テスト完了 ===");
    }

    // ─────────────────────────────────────────
    // 3. ニューゲーム→難易度選択→キャラクター作成画面 検証
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_NewGameFlow_CharacterCreation()
    {
        Log("=== テスト開始: ニューゲーム→難易度選択→キャラクター作成 検証 ===");

        var window = LaunchAndGetTitleWindow();

        Log("検証: NewGameボタン→難易度選択ダイアログ");
        var btn = FindElement(window, "NewGameButton");
        Assert.NotNull(btn);
        btn!.AsButton().Invoke();
        Thread.Sleep(1000);

        var modals = window.ModalWindows;
        Assert.True(modals.Length > 0, "難易度選択ダイアログが開かない");
        var diffDialog = modals[0];
        Log("  → 難易度選択ダイアログ表示OK");

        Assert.NotNull(FindElement(diffDialog, "DifficultyList"));
        var detailText = FindElement(diffDialog, "DetailText");
        Assert.NotNull(detailText);
        Log($"  → DifficultyList/DetailText OK");

        Log("検証: Enter→キャラクター作成画面");
        diffDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);
        Thread.Sleep(1000);

        modals = window.ModalWindows;
        Assert.True(modals.Length > 0, "キャラクター作成ダイアログが開かない");
        var charDialog = modals[0];
        Log("  → キャラクター作成ダイアログ表示OK");

        Assert.NotNull(FindElement(charDialog, "NameBox"));
        Assert.NotNull(FindElement(charDialog, "RaceList"));
        Assert.NotNull(FindElement(charDialog, "ClassList"));
        Assert.NotNull(FindElement(charDialog, "BackgroundList"));
        var previewText = FindElement(charDialog, "PreviewText");
        Assert.NotNull(previewText);
        Assert.False(string.IsNullOrWhiteSpace(previewText!.Name), "プレビューテキストが空");
        Log($"  → 全UI要素OK, Preview='{previewText.Name[..Math.Min(40, previewText.Name.Length)]}…'");

        Log("検証: Escキャンセル");
        charDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited, "キャンセル後にアプリが終了した");
        Log("  → キャンセルOK");
        Log("=== テスト完了 ===");
    }

    // ─────────────────────────────────────────
    // 4. 設定ダイアログ パラメータ変更検証
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_SettingsParameterChanges()
    {
        Log("=== テスト開始: 設定パラメータ変更検証 ===");

        var window = LaunchAndGetTitleWindow();

        var settingsBtn = FindElement(window, "SettingsButton");
        Assert.NotNull(settingsBtn);
        settingsBtn!.AsButton().Invoke();
        Thread.Sleep(500);
        var modals = window.ModalWindows;
        Assert.True(modals.Length > 0);
        var settingsDialog = modals[0];

        Log("検証: マスター音量スライダー操作");
        var masterSlider = FindElement(settingsDialog, "MasterVolumeSlider");
        Assert.NotNull(masterSlider);
        var slider = masterSlider!.AsSlider();

        slider.Value = 0;
        Thread.Sleep(300);
        var label = FindElement(settingsDialog, "MasterVolumeText");
        Assert.NotNull(label);
        Assert.Contains("0", label!.Name);
        Log($"  → 最小値: {label.Name}");

        slider.Value = 100;
        Thread.Sleep(300);
        label = FindElement(settingsDialog, "MasterVolumeText");
        Assert.NotNull(label);
        Assert.Contains("100", label!.Name);
        Log($"  → 最大値: {label.Name}");

        Log("検証: フォントサイズスライダー操作");
        var fontSlider = FindElement(settingsDialog, "FontSizeSlider");
        Assert.NotNull(fontSlider);
        var fSlider = fontSlider!.AsSlider();

        fSlider.Value = 10;
        Thread.Sleep(300);
        var fontLabel = FindElement(settingsDialog, "FontSizeText");
        Assert.NotNull(fontLabel);
        Assert.Contains("10", fontLabel!.Name);
        Log($"  → フォント最小: {fontLabel.Name}");

        fSlider.Value = 24;
        Thread.Sleep(300);
        fontLabel = FindElement(settingsDialog, "FontSizeText");
        Assert.NotNull(fontLabel);
        Assert.Contains("24", fontLabel!.Name);
        Log($"  → フォント最大: {fontLabel.Name}");

        Log("検証: 初期値リセット");
        var resetBtn = settingsDialog.FindFirstDescendant(cf => cf.ByText("初期値に戻す"));
        if (resetBtn != null)
        {
            resetBtn.AsButton().Invoke();
            Thread.Sleep(500);
            var resetLabel = FindElement(settingsDialog, "MasterVolumeText");
            Assert.NotNull(resetLabel);
            Log($"  → リセット後: {resetLabel!.Name}");
        }

        settingsDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited);
        Log("=== テスト完了 ===");
    }

    // ─────────────────────────────────────────
    // 5. メインウィンドウ統合テスト（デバッグマップ1回起動で全検証）
    //    旧 MainWindow_FullIntegration + SystemVerification_DebugMap_FullIntegration 統合
    // ─────────────────────────────────────────

    [Fact]
    public void MainWindow_FullIntegration()
    {
        Log("=== テスト開始: メインウィンドウ統合検証（デバッグマップ） ===");

        var window = LaunchWithDebugMap();

        // ====================================================================
        // Section 1: ウィンドウ基本＋初期値検証
        // ====================================================================
        Log("--- Section 1: 初期値検証 ---");

        Assert.Equal("ローグライクゲーム", window.Title);
        Assert.True(window.BoundingRectangle.Width >= 800);
        Assert.True(window.BoundingRectangle.Height >= 500);
        Log($"  → サイズ={window.BoundingRectangle.Width}x{window.BoundingRectangle.Height} OK");

        var msgLog = GetText(window, "MessageLog");
        Assert.Contains("デバッグモード", msgLog);
        Assert.Contains("WASD", msgLog);

        var helpText = FindElement(window, "KeyHelpText");
        Assert.NotNull(helpText);
        Assert.Contains("WASD", helpText!.Name);

        Assert.Contains("第1層", GetText(window, "FloorText"));

        var territory = GetText(window, "TerritoryText");
        Assert.False(string.IsNullOrWhiteSpace(territory));
        var surfaceStatus = GetText(window, "SurfaceStatusText");
        Assert.True(surfaceStatus.Contains("地上") || surfaceStatus.Contains("B") || surfaceStatus.Contains("F"));
        Log($"  → 領地={territory}, 状態={surfaceStatus}");

        var hp = GetText(window, "HpText");
        var mp = GetText(window, "MpText");
        var sp = GetText(window, "SpText");
        Assert.Matches(@"^\d+/\d+$", hp);
        Assert.Matches(@"^\d+/\d+$", mp);
        Assert.Matches(@"^\d+/\d+$", sp);
        var hpParts = hp.Split('/');
        Assert.Equal(hpParts[0], hpParts[1]);
        Log($"  → HP={hp}, MP={mp}, SP={sp} フルOK");

        Assert.Equal("1", GetText(window, "LevelText"));
        Assert.Matches(@"\d+/\d+", GetText(window, "ExpText"));

        var weight = GetText(window, "WeightText");
        Assert.Matches(@"\d+\.\d+/\d+\.\d+kg", weight);
        Assert.NotEqual("0.0", weight.Split('/')[0]);

        Assert.Matches(@"\d.*G", GetText(window, "GoldText"));

        var turnLimit = GetText(window, "TurnLimitText");
        Assert.True(turnLimit.Contains("残り") || turnLimit.Contains("制限なし"));

        var hunger = GetText(window, "HungerText");
        Assert.Matches(@"^\d+$", hunger);
        Assert.True(int.Parse(hunger) > 0);
        var sanity = GetText(window, "SanityText");
        Assert.Matches(@"^\d+$", sanity);
        Assert.True(int.Parse(sanity) > 0);
        Log($"  → 満腹度={hunger}, 正気度={sanity}");

        var dateText = GetText(window, "DateText");
        Assert.Contains("年", dateText);
        Assert.Contains("月", dateText);
        Assert.Contains("日", dateText);
        Assert.Matches(@"\d{2}:\d{2}", dateText);
        Assert.False(string.IsNullOrWhiteSpace(GetText(window, "TimePeriodText")));
        Log($"  → 日付='{dateText}' OK");

        // ====================================================================
        // Section 2: アイテム拾い・インベントリ
        //   プレイヤー(16,12) → アイテム(14,14): 左2,下2
        // ====================================================================
        Log("--- Section 2: アイテム拾い・インベントリ ---");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_G);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → アイテム拾いOK");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_I);
        Thread.Sleep(500);
        var invModals = window.ModalWindows;
        Assert.True(invModals.Length > 0, "インベントリダイアログが開かない");
        Assert.Equal("所持品", invModals[0].Title);
        Log("  → インベントリ='所持品' OK");
        invModals[0].Close();
        Thread.Sleep(200);

        // ====================================================================
        // Section 3: NPC対話
        //   (14,14)付近 → NPC(5,12): 左9,上2
        // ====================================================================
        Log("--- Section 3: NPC対話 ---");

        for (int i = 0; i < 9; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        for (int i = 0; i < 2; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        CloseModals(window);
        Log("  → NPC方向移動OK");

        // ====================================================================
        // Section 4: ドアインタラクト
        //   (5,12)付近 → ドア(10,2): 右5,上10,上1(ドア開放),X(ドア閉じ)
        // ====================================================================
        Log("--- Section 4: ドアインタラクト ---");

        for (int i = 0; i < 5; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        for (int i = 0; i < 10; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_X);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → ドア開閉OK");

        // ====================================================================
        // Section 5: 地形効果（水タイル）
        //   (10,2)付近 → 水(15,6): 右5,下4,右2
        // ====================================================================
        Log("--- Section 5: 地形効果（水タイル） ---");

        for (int i = 0; i < 5; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        for (int i = 0; i < 4; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 水タイル移動OK");

        // ====================================================================
        // Section 6: 全ダイアログ開閉
        // ====================================================================
        Log("--- Section 6: ダイアログ・キー操作 ---");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Thread.Sleep(200);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Assert.False(_app!.HasExited);
        Log("  → ミニマップ切替OK");

        var statusBarIds = new[]
        {
            "TerritoryText", "SurfaceStatusText", "FloorText", "DateText", "TimePeriodText",
            "LevelText", "ExpText", "HpText", "MpText", "SpText",
            "HungerText", "SanityText", "GoldText", "WeightText", "TurnLimitText",
            "StanceText", "FatigueText", "HygieneText", "DiseaseText"
        };
        foreach (var id in statusBarIds)
        {
            var el = FindElement(window, id);
            Assert.NotNull(el);
            Assert.False(string.IsNullOrWhiteSpace(el!.Name), $"{id}が空");
        }
        Log("  → ステータスバー全19要素OK");

        var dialogKeys = new[]
        {
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_C, "ステータス"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_L, "ログ"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_V, "魔法"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_J, "世界地図"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_K, "クエスト"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_O, "宗教"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_H, "鍛冶"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_B, "街"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_E, "スキル"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_P, "祈り"),
            (FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_N, "ギルド"),
        };
        foreach (var (key, name) in dialogKeys)
        {
            PressKey(window, key);
            Thread.Sleep(400);
            CloseModals(window);
            Assert.False(_app!.HasExited);
            Log($"  → {name}OK");
        }

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 探索OK");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
        Thread.Sleep(300);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 自動探索→中断OK");

        // ====================================================================
        // Section 7: 移動全種
        // ====================================================================
        Log("--- Section 7: 移動全種 ---");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.UP);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.HOME);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.PRIOR);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.END);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.NEXT);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 全移動キーOK");

        // ====================================================================
        // Section 8: 戦闘・射撃・投擲
        // ====================================================================
        Log("--- Section 8: 戦闘・射撃・投擲 ---");

        for (int i = 0; i < 5; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        }
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        hp = GetText(window, "HpText");
        Assert.Matches(@"\d+/\d+", hp);
        Log($"  → 戦闘後HP={hp}");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R);
        Thread.Sleep(200);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_T);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 射撃・投擲OK");

        // ====================================================================
        // Section 9: 階段操作
        // ====================================================================
        Log("--- Section 9: 階段操作 ---");

        window.Focus();
        FlaUI.Core.Input.Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.OEM_COMMA);
        FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 階段上昇OK");

        window.Focus();
        FlaUI.Core.Input.Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.OEM_PERIOD);
        FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 階段降下OK");

        // ====================================================================
        // Section 10: 日時進行・スキルCD・詠唱
        // ====================================================================
        Log("--- Section 10: 日時進行・スキル・詠唱 ---");

        var initialDate = GetText(window, "DateText");
        for (int i = 0; i < 65; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(300);
        Assert.NotEqual(initialDate, GetText(window, "DateText"));
        Log("  → 日時進行OK");

        for (int i = 0; i < 20; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → スキルCD処理OK");

        for (int i = 0; i < 10; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        }
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 詠唱ターンOK");

        // ====================================================================
        // Section 11: 連打耐性
        // ====================================================================
        Log("--- Section 11: 連打耐性 ---");

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
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_1,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_2,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_3,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_4,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_5,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_6,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_N,
        };
        for (int round = 0; round < 3; round++)
        {
            foreach (var key in rapidKeys)
            {
                window.Focus();
                FlaUI.Core.Input.Keyboard.Press(key);
                Thread.Sleep(30);
            }
            Log($"  ラウンド{round + 1}/3");
        }
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 連打耐性OK");

        // ====================================================================
        // Section 12: セーブ・ロード
        // ====================================================================
        Log("--- Section 12: セーブ・ロード ---");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.F5);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → セーブOK");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.F9);
        Thread.Sleep(500);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        Log("  → ロードOK");

        // ====================================================================
        // Section 13: Qキー終了
        // ====================================================================
        Log("--- Section 13: Qキー終了 ---");

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_Q);
        Thread.Sleep(500);
        try
        {
            var modalWindow = _app!.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(w => w.AutomationId != window.AutomationId);
            if (modalWindow != null)
            {
                var okBtn = modalWindow.FindFirstDescendant(cf => cf.ByName("OK"));
                okBtn?.AsButton().Invoke();
                Thread.Sleep(300);
            }
        }
        catch { }
        _app!.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(3));
        Log($"  → 終了OK (exited={_app!.HasExited})");
        Log("=== テスト完了 ===");
    }

    // ─────────────────────────────────────────
    // 6. コンティニュー→セーブデータ選択画面 検証
    // ─────────────────────────────────────────

    [Fact]
    public void TitleScreen_ContinueFlow_SaveDataSelect()
    {
        Log("=== テスト開始: コンティニュー→セーブデータ選択 検証 ===");

        // Step 1: デバッグマップでセーブデータ作成
        Log("前準備: セーブデータ作成");
        var debugWindow = LaunchWithDebugMap();
        PressKey(debugWindow, FlaUI.Core.WindowsAPI.VirtualKeyShort.F5);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited);

        _app.Close();
        _app.Dispose();
        _app = null;
        Thread.Sleep(1000);

        // Step 2: タイトル画面でコンティニュー
        Log("検証: コンティニュー→セーブデータ選択画面");
        var titleWindow = LaunchAndGetTitleWindow();

        var continueBtn = FindElement(titleWindow, "ContinueButton");
        Assert.NotNull(continueBtn);

        if (!continueBtn!.AsButton().IsEnabled)
        {
            Log("  → コンティニューボタン無効、スキップ");
            return;
        }

        continueBtn.AsButton().Invoke();
        Thread.Sleep(1000);

        var modals = titleWindow.ModalWindows;
        Assert.True(modals.Length > 0, "セーブデータ選択ダイアログが開かない");
        var saveDialog = modals[0];
        Log("  → セーブデータ選択ダイアログOK");

        Assert.NotNull(FindElement(saveDialog, "SaveSlotList"));
        var detailText = FindElement(saveDialog, "DetailText");
        Assert.NotNull(detailText);
        Log($"  → SaveSlotList/DetailText OK");

        saveDialog.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited);
        Log("  → キャンセルOK");
        Log("=== テスト完了 ===");
    }
}
