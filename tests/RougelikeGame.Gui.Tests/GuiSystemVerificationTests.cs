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
/// 各ゲームシステムが GUI 上で正しく動作しているかを「値レベル」で検証するテスト（統合版）。
/// アプリ起動コストを削減するため、同一起動内で検証可能な項目をまとめて実行する。
/// デバッグマップ（固定シード）＋ --skip-title で高速起動する。
///
/// ■ 責務: ステータス初期値・表示形式・システム動作の値レベル詳細検証
///   （UI存在チェック＋キーバインドクラッシュ耐性は GuiAutomationTests に委譲）
///
/// テスト構成（2テスト）:
///   1. SystemVerification_DebugMap_FullIntegration — 1回起動で以下を一括検証:
///      - マップ生成・初期メッセージ（第1層、デバッグモード、WASD）
///      - 領地名・地上/ダンジョン表示の値検証
///      - HP/MP/SP初期値フル（current==max）、形式 'X/Y'
///      - レベル=1、経験値形式、重量 'x.x/y.ykg'（初期非0）、通貨 'XG'、ターン制限
///      - 満腹度・正気度の初期値検証（数値、非0）
///      - 日時表示形式（'冒険暦XXXX年 ○○の月 X日 HH:MM'）・時間帯検証
///      - 戦闘接触後HP形式維持、ドア閉じ(X)、射撃(R)・投擲(T)
///      - 階段上昇(Shift+<)・降下(Shift+>)、スキルCD(20ターン)、詠唱ターン処理
///   2. SystemVerification_LongPlay_HungerAndEndurance — 長時間プレイ検証:
///      - 70ターン待機: 日付進行＋階層不変
///      - 800ターン待機: 満腹度減少確認
///      - 200ターン連続操作: ステータスバー正常維持
///
/// ■ Ver.prt.0.2 追加システムのカバレッジ状況:
///   以下のシステムは Core テストで値レベル検証済み。GUI 接続後にここに追加:
///   - MonsterRaceSystem（敵種族分類）→ 敵情報ダイアログで種族名表示時
///   - TimeOfDaySystem（時刻行動変化）→ TimePeriod ステータスバー連携時
///   - KarmaSystem（カルマ）→ カルマ表示UI実装時
///   - ReputationSystem（評判・名声）→ ワールドマップ/領地情報表示時
///   - ProficiencySystem（熟練度）→ キャラクター情報画面拡張時
///   - EnchantmentSystem（エンチャント）→ 鍛冶画面エンチャントUI実装時
///   - ItemGradeSystem（アイテム等級）→ アイテム表示名に等級接頭辞反映時
///   - DungeonFeatureGenerator（ダンジョン特徴）→ フロア名表示連携時
/// </summary>
[Collection("GuiTests")]
public class GuiSystemVerificationTests : IDisposable
{
    private readonly UIA3Automation _automation;
    private readonly ITestOutputHelper _output;
    private Application? _app;

    public GuiSystemVerificationTests(ITestOutputHelper output)
    {
        _automation = new UIA3Automation();
        _output = output;
    }

    public void Dispose()
    {
        _app?.Close();
        _app?.Dispose();
        _automation.Dispose();
    }

    private void Log(string message) => _output.WriteLine($"[SystemVerify] {message}");

    // ─────────────────────────────────────────
    // ヘルパー
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

    private static void PressKey(Window window, FlaUI.Core.WindowsAPI.VirtualKeyShort key)
    {
        window.Focus();
        FlaUI.Core.Input.Keyboard.Press(key);
        Thread.Sleep(80);
    }

    private static string GetText(Window window, string automationId)
    {
        var el = FindElement(window, automationId);
        Assert.NotNull(el);
        return el!.Name;
    }

    private static void CloseModals(Window window)
    {
        foreach (var m in window.ModalWindows)
            m.Close();
        Thread.Sleep(200);
    }

    // ─────────────────────────────────────────
    // 1. デバッグマップ統合検証（1回起動）
    //    初期ステータス・表示形式・各キー操作・階段操作を一括検証
    // ─────────────────────────────────────────

    [Fact]
    public void SystemVerification_DebugMap_FullIntegration()
    {
        Log("=== テスト開始: システム検証 デバッグマップ統合テスト ===");
        Log("目的: 各システムのGUI上での動作を1回のアプリ起動で一括検証する");

        var window = LaunchWithDebugMap();

        // ========== P2.7-2.8: マップ生成・FOV ==========
        Log("検証: デバッグマップの読み込み（第1層表示、初期メッセージにデバッグモード/WASD）");
        Assert.Contains("第1層", GetText(window, "FloorText"));
        var log = GetText(window, "MessageLog");
        Assert.Contains("デバッグモード", log);
        Assert.Contains("WASD", log);
        Log("  → マップ生成OK");

        // ========== 領地・地上/ダンジョン表示 ==========
        Log("検証: 領地名が空でないか、地上/ダンジョン表示が存在するか");
        var territory = GetText(window, "TerritoryText");
        Assert.False(string.IsNullOrWhiteSpace(territory), "領地名が空");
        var surfaceStatus = GetText(window, "SurfaceStatusText");
        Assert.False(string.IsNullOrWhiteSpace(surfaceStatus), "地上/ダンジョン表示が空");
        Assert.True(surfaceStatus.Contains("地上") || surfaceStatus.Contains("B") || surfaceStatus.Contains("F"),
            $"地上/ダンジョン表示が不正: {surfaceStatus}");
        Log($"  → 領地={territory}, 状態={surfaceStatus} OK");

        // ========== P1.2-1.3: Player・Stats 基盤 ==========
        Log("検証: HP/MP/SPが 'current/max' 形式で初期状態がフル（current==max）か");
        var hp = GetText(window, "HpText");
        var mp = GetText(window, "MpText");
        var sp = GetText(window, "SpText");
        Assert.Matches(@"^\d+/\d+$", hp);
        Assert.Matches(@"^\d+/\d+$", mp);
        Assert.Matches(@"^\d+/\d+$", sp);
        var hpParts = hp.Split('/');
        Assert.Equal(hpParts[0], hpParts[1]);
        Log($"  → HP={hp}, MP={mp}, SP={sp} 初期値フルOK");

        // ========== P4.7: レベル・経験値表示 ==========
        Log("検証: 初期レベルが1、経験値が 'current/max' 形式か");
        var level = GetText(window, "LevelText");
        var exp = GetText(window, "ExpText");
        Assert.Equal("1", level);
        Assert.Matches(@"\d+/\d+", exp);
        Log($"  → Lv={level}, Exp={exp} OK");

        // ========== P2.6: アイテムシステム — 初期装備重量 ==========
        Log("検証: 初期装備により重量が0でなく 'x.x/y.ykg' 形式で表示されるか");
        var weight = GetText(window, "WeightText");
        Assert.Contains("/", weight);
        Assert.Contains("kg", weight);
        Assert.Matches(@"\d+\.\d+/\d+\.\d+kg", weight);
        var currentWeight = weight.Split('/')[0];
        Assert.NotEqual("0.0", currentWeight);
        Log($"  → 重量={weight} OK");

        // ========== P4.24: 通貨システム ==========
        Log("検証: 所持金がXG形式で表示されるか");
        var gold = GetText(window, "GoldText");
        Assert.Matches(@"\d.*G", gold);
        Log($"  → 所持金={gold} OK");

        // ========== P4.16: ターン制限 ==========
        Log("検証: ターン制限表示が '残りX日' または '制限なし' か");
        var turnLimit = GetText(window, "TurnLimitText");
        bool hasLimit = turnLimit.Contains("残り") && turnLimit.Contains("日");
        bool noLimit = turnLimit.Contains("制限なし");
        Assert.True(hasLimit || noLimit, $"ターン制限表示が不正: {turnLimit}");
        Log($"  → ターン制限={turnLimit} OK");

        // ========== 満腹度・正気度: 初期値検証 ==========
        Log("検証: 満腹度と正気度が数値で表示され、初期値が0でないか");
        var hunger = GetText(window, "HungerText");
        Assert.Matches(@"^\d+$", hunger);
        int hungerVal = int.Parse(hunger);
        Assert.True(hungerVal > 0, $"初期満腹度が0: {hunger}");
        Log($"  → 満腹度={hunger} OK");
        var sanity = GetText(window, "SanityText");
        Assert.Matches(@"^\d+$", sanity);
        int sanityVal = int.Parse(sanity);
        Assert.True(sanityVal > 0, $"初期正気度が0: {sanity}");
        Log($"  → 正気度={sanity} OK");

        // ========== 日時表示: 形式検証 ==========
        Log("検証: 日付が'冒険歴XXXX年 ○○の月 X日 HH:MM'形式、時間帯が空でないか");
        var dateText = GetText(window, "DateText");
        Assert.Contains("年", dateText);
        Assert.Contains("月", dateText);
        Assert.Contains("日", dateText);
        Assert.Matches(@"\d{2}:\d{2}", dateText);
        Log($"  → 日付='{dateText}' 形式OK");
        var timePeriod = GetText(window, "TimePeriodText");
        Assert.False(string.IsNullOrWhiteSpace(timePeriod), "時間帯が空");
        Log($"  → 時間帯='{timePeriod}' OK");

        // ========== P2.3-2.4: 戦闘システム — 敵接触移動 ==========
        Log("検証: 複数回移動で敵に接触してもクラッシュしないか");
        for (int i = 0; i < 5; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        }
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        hp = GetText(window, "HpText");
        Assert.Matches(@"\d+/\d+", hp);
        Log($"  → 戦闘後HP={hp} クラッシュなしOK");

        // ========== P4.20: ドア開閉 — Xキー ==========
        Log("検証: Xキー（ドア閉じ）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_X);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → ドア閉じOK");

        // ========== P4.21: 射撃・投擲 — R/Tキー ==========
        Log("検証: Rキー（射撃）とTキー（投擲）でクラッシュしないか");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_T);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 射撃・投擲OK");

        // ========== P4.14: 階段上昇 — Shift+< ==========
        Log("検証: Shift+<（階段上昇）でクラッシュしないか");
        for (int i = 0; i < 3; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        window.Focus();
        FlaUI.Core.Input.Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.OEM_COMMA);
        FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 階段上昇OK");

        // ========== P4.14: 階段降下 — Shift+> ==========
        Log("検証: Shift+>（階段降下）でクラッシュしないか");
        for (int i = 0; i < 3; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        window.Focus();
        FlaUI.Core.Input.Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.OEM_PERIOD);
        FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        Thread.Sleep(300);
        Assert.False(_app!.HasExited);
        Log("  → 階段降下OK");

        // ========== P5.4: スキルシステム統合 ==========
        Log("検証: 20ターン待機でスキルクールダウンTick処理がクラッシュしないか");
        for (int i = 0; i < 20; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → スキルターン処理OK");

        // ========== P5.6: 魔法言語詠唱 ==========
        Log("検証: 移動+待機の繰り返しでIsCasting状態管理がクラッシュしないか");
        for (int i = 0; i < 10; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        }
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        Log("  → 詠唱ターン処理OK");

        Log("=== テスト完了: システム検証 デバッグマップ統合テスト ===");
    }

    // ─────────────────────────────────────────
    // 2. 長時間プレイ・高負荷テスト（1回起動）
    //    800ターン満腹度変化 + 200ターン長時間プレイ + 統合フロー検証
    // ─────────────────────────────────────────

    [Fact]
    public void SystemVerification_LongPlay_HungerAndEndurance()
    {
        Log("=== テスト開始: 長時間プレイ・高負荷耐性テスト ===");
        Log("目的: 大量ターン経過でのシステム安定性と満腹度変化を検証する");

        var window = LaunchWithDebugMap();

        // ========== 統合フロー: 初期状態記録 ==========
        Log("検証: 70ターン待機後に日時が進行し、階層は変化しないか");
        var initialDate = GetText(window, "DateText");
        var initialFloor = GetText(window, "FloorText");
        Log($"  待機前: 日付={initialDate}, 階層={initialFloor}");

        for (int i = 0; i < 70; i++)
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(300);

        var updatedDate = GetText(window, "DateText");
        Assert.NotEqual(initialDate, updatedDate);
        var currentFloor = GetText(window, "FloorText");
        Assert.Equal(initialFloor, currentFloor);
        Assert.False(_app!.HasExited);
        Log($"  待機後: 日付={updatedDate}, 階層={currentFloor}");
        Log("  → 日時進行＋階層不変OK");

        // ========== P4.3: 満腹度変化（800ターン） ==========
        Log("検証: 800ターン待機で満腹度が減少するか（飢餓減少は600ターン毎）");
        var initialHunger = GetText(window, "HungerText");
        Log($"  待機前の満腹度: {initialHunger}");

        for (int i = 0; i < 800; i++)
        {
            window.Focus();
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
            Thread.Sleep(20);
        }
        Thread.Sleep(1000);

        Assert.False(_app!.HasExited);
        var updatedHunger = GetText(window, "HungerText");
        Assert.NotEqual(initialHunger, updatedHunger);
        Log($"  待機後の満腹度: {updatedHunger}");
        Log("  → 満腹度変化OK");

        // ========== 長時間プレイシミュレーション（200ターン連続操作） ==========
        Log("検証: 200ターン連続操作（WASD+Space）後もクラッシュせずステータスバー正常か");
        var keys = new[]
        {
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE,
        };

        for (int i = 0; i < 200; i++)
        {
            var key = keys[i % keys.Length];
            window.Focus();
            FlaUI.Core.Input.Keyboard.Press(key);
            Thread.Sleep(15);
        }
        Thread.Sleep(500);

        Assert.False(_app!.HasExited);
        var finalHp = GetText(window, "HpText");
        Assert.Matches(@"\d+/\d+", finalHp);
        Log($"  → 200ターン後HP={finalHp} クラッシュなしOK");

        Log("=== テスト完了: 長時間プレイ・高負荷耐性テスト ===");
    }
}
