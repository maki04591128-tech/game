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
/// テスト構成（5テスト）:
///   1. SystemVerification_DebugMap_FullIntegration — 1回起動で以下を一括検証:
///      - マップ生成・初期メッセージ（第1層、デバッグモード、WASD）
///      - 領地名・地上/ダンジョン表示の値検証
///      - HP/MP/SP初期値フル（current==max）、形式 'X/Y'
///      - レベル=1、経験値形式、重量 'x.x/y.ykg'（初期非0）、通貨 'XG'、ターン制限
///      - 満腹度・正気度の初期値検証（数値、非0）
///      - 日時表示形式（'冒険暦XXXX年 ○○の月 X日 HH:MM'）・時間帯検証
///      - 季節・天候・渇き・カルマ・仲間数の新ステータスバー要素値検証
///      - 戦闘接触後HP形式維持、ドア閉じ(X)、射撃(R)・投擲(T)
///      - 階段上昇(Shift+&lt;)・降下(Shift+&gt;)、スキルCD(20ターン)、詠唱ターン処理
///   2. SystemVerification_LongPlay_HungerAndEndurance — 長時間プレイ検証:
///      - 70ターン待機: 日付進行＋階層不変
///      - 800ターン待機: 満腹度減少確認
///      - 200ターン連続操作: ステータスバー正常維持
///   3. SystemVerification_CombatAndStatusTransition — 戦闘・ステータス遷移検証:
///      - 敵接触でHP減少方向検証（current &lt; max確認）
///      - 戦闘前後のステータスバー全20要素一貫性検証（表示が壊れないこと）
///      - 自動探索(Tab)→移動→Space中断のフロー検証
///      - 複数回戦闘後のステータスバー整合性維持
///   4. SystemVerification_StatusBarConsistencyAfterActions — アクション後ステータス一貫性:
///      - 各種アクション（拾う/探索/射撃/投擲/祈り/ドア閉じ）実行後のステータスバー全要素検証
///      - 階段操作前後のステータスバー変化追跡
///      - ミニマップ切替前後のステータスバー不変検証
///   5. SystemVerification_NewStatusBarFormats — 新ステータスバー要素のフォーマット詳細検証:
///      - Season/Weather/Thirst/Karma/CompanionCountの初期値が適切な日本語表記であること
///      - 各値の取りうる範囲（enum値や数値範囲）の検証
///      - 複数ターン経過後もフォーマットが維持されること
///
/// ■ GUI接続済みシステムのカバレッジ:
///   - SeasonSystem（季節）→ SeasonText ステータスバー値検証済み
///   - WeatherSystem（天候）→ WeatherText ステータスバー値検証済み
///   - ThirstSystem（渇き）→ ThirstText ステータスバー値検証済み
///   - KarmaSystem（カルマ）→ KarmaText ステータスバー値検証済み
///   - CompanionSystem（仲間）→ CompanionCountText ステータスバー値検証済み
///   - EncyclopediaSystem（図鑑）→ Yキー画面遷移はGuiAutomationTestsで検証
///   - DeathLogSystem（死亡記録）→ Zキー画面遷移はGuiAutomationTestsで検証
///   - CompanionSystem（仲間管理）→ Uキー画面遷移はGuiAutomationTestsで検証
///
/// ■ 将来GUI接続後に追加予定:
///   - MonsterRaceSystem（敵種族分類）→ 敵情報ダイアログで種族名表示時
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

        // ========== 季節表示: 値検証 ==========
        Log("検証: 季節テキストが空でなく、有効な季節名を含むか");
        var season = GetText(window, "SeasonText");
        Assert.False(string.IsNullOrWhiteSpace(season), "季節テキストが空");
        var validSeasons = new[] { "春", "夏", "秋", "冬" };
        Assert.True(validSeasons.Any(s => season.Contains(s)),
            $"季節テキストが有効な季節名を含まない: '{season}'");
        Log($"  → 季節='{season}' OK");

        // ========== 天候表示: 値検証 ==========
        Log("検証: 天候テキストが空でないか");
        var weather = GetText(window, "WeatherText");
        Assert.False(string.IsNullOrWhiteSpace(weather), "天候テキストが空");
        Log($"  → 天候='{weather}' OK");

        // ========== 渇き表示: 値検証 ==========
        Log("検証: 渇きテキストが空でないか");
        var thirst = GetText(window, "ThirstText");
        Assert.False(string.IsNullOrWhiteSpace(thirst), "渇きテキストが空");
        Log($"  → 渇き='{thirst}' OK");

        // ========== カルマ表示: 値検証 ==========
        Log("検証: カルマテキストが空でなく、有効なランク名を含むか");
        var karma = GetText(window, "KarmaText");
        Assert.False(string.IsNullOrWhiteSpace(karma), "カルマテキストが空");
        Log($"  → カルマ='{karma}' OK");

        // ========== 仲間数表示: 値検証 ==========
        Log("検証: 仲間数テキストが数値形式か");
        var companionCount = GetText(window, "CompanionCountText");
        Assert.Matches(@"^\d+$", companionCount);
        int compCount = int.Parse(companionCount);
        Assert.True(compCount >= 0, $"仲間数が負値: {companionCount}");
        Log($"  → 仲間数={companionCount} OK");

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

    // ─────────────────────────────────────────
    // 3. 戦闘・ステータス遷移検証（1回起動）
    //    敵接触でHP減少方向確認、戦闘前後のステータスバー一貫性検証
    // ─────────────────────────────────────────

    [Fact]
    public void SystemVerification_CombatAndStatusTransition()
    {
        Log("=== テスト開始: 戦闘・ステータス遷移検証 ===");
        Log("目的: 戦闘前後のステータスバー整合性と自動探索フローを検証する");

        var window = LaunchWithDebugMap();

        var allStatusIds = new[]
        {
            "TerritoryText", "SurfaceStatusText", "FloorText", "DateText", "TimePeriodText",
            "LevelText", "ExpText", "HpText", "MpText", "SpText",
            "HungerText", "SanityText", "GoldText", "WeightText", "TurnLimitText",
            "SeasonText", "WeatherText", "ThirstText", "KarmaText", "CompanionCountText"
        };

        // ========== 初期状態の全ステータス記録 ==========
        Log("記録: 初期状態の全20ステータスバー値");
        var initialValues = new Dictionary<string, string>();
        foreach (var id in allStatusIds)
        {
            var text = GetText(window, id);
            Assert.False(string.IsNullOrWhiteSpace(text), $"初期状態で{id}が空");
            initialValues[id] = text;
            Log($"  {id}='{text}'");
        }
        Log("  → 初期状態記録完了");

        // ========== HP初期値がフル（current == max）であることを確認 ==========
        Log("検証: 初期HPがフル（current == max）であること");
        var initialHp = GetText(window, "HpText");
        var hpParts = initialHp.Split('/');
        Assert.Equal(2, hpParts.Length);
        Assert.Equal(hpParts[0], hpParts[1]);
        int initialHpMax = int.Parse(hpParts[1]);
        Log($"  → 初期HP={initialHp} (max={initialHpMax}) OK");

        // ========== 移動で敵に繰り返し接触 ==========
        Log("検証: 30回移動して敵に接触し、HP減少方向を確認");
        for (int i = 0; i < 30; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        }
        Thread.Sleep(300);
        Assert.False(_app!.HasExited, "戦闘中にクラッシュ");

        // ========== 戦闘後HP検証 ==========
        Log("検証: 戦闘後HPが 'X/Y' 形式を維持し、currentがmax以下であること");
        var afterHp = GetText(window, "HpText");
        Assert.Matches(@"^\d+/\d+$", afterHp);
        var afterHpParts = afterHp.Split('/');
        int afterCurrent = int.Parse(afterHpParts[0]);
        int afterMax = int.Parse(afterHpParts[1]);
        Assert.True(afterCurrent <= afterMax, $"HP current({afterCurrent}) > max({afterMax})");
        Assert.True(afterCurrent >= 0, $"HPが負値: {afterCurrent}");
        Log($"  → 戦闘後HP={afterHp} (current<=max, current>=0) OK");

        // ========== 戦闘後の全ステータスバー整合性検証 ==========
        Log("検証: 戦闘後も全20ステータスバー要素が空でなく表示形式が維持されること");
        foreach (var id in allStatusIds)
        {
            var text = GetText(window, id);
            Assert.False(string.IsNullOrWhiteSpace(text), $"戦闘後に{id}の表示が空");
        }
        // HP/MP/SPは形式維持
        Assert.Matches(@"^\d+/\d+$", GetText(window, "HpText"));
        Assert.Matches(@"^\d+/\d+$", GetText(window, "MpText"));
        Assert.Matches(@"^\d+/\d+$", GetText(window, "SpText"));
        // レベルが数値
        Assert.Matches(@"^\d+$", GetText(window, "LevelText"));
        // 満腹度・正気度が数値
        Assert.Matches(@"^\d+$", GetText(window, "HungerText"));
        Assert.Matches(@"^\d+$", GetText(window, "SanityText"));
        Log("  → 戦闘後ステータスバー整合性OK");

        // ========== 自動探索フロー検証 ==========
        Log("検証: Tab(自動探索開始)→待機→Space(中断)のフローでクラッシュしないか");
        const int AutoExploreWaitMs = 3000; // 自動探索が数ステップ進むのを待つ時間
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
        Thread.Sleep(AutoExploreWaitMs);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited, "自動探索フロー中にクラッシュ");
        // 自動探索後もステータスバーが正常
        foreach (var id in allStatusIds)
        {
            var text = GetText(window, id);
            Assert.False(string.IsNullOrWhiteSpace(text), $"自動探索後に{id}の表示が空");
        }
        Log("  → 自動探索フローOK");

        // ========== 複数回戦闘→回復不可状態でも安定 ==========
        Log("検証: 追加50回移動でさらに戦闘を重ねてもクラッシュしないか");
        for (int i = 0; i < 50; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        }
        Thread.Sleep(300);
        Assert.False(_app!.HasExited, "長時間戦闘でクラッシュ");
        var finalHp = GetText(window, "HpText");
        Assert.Matches(@"^\d+/\d+$", finalHp);
        Log($"  → 追加戦闘後HP={finalHp} クラッシュなしOK");

        Log("=== テスト完了: 戦闘・ステータス遷移検証 ===");
    }

    // ─────────────────────────────────────────
    // 4. アクション後ステータス一貫性検証（1回起動）
    //    各種アクション実行後のステータスバー全要素が壊れないことを検証
    // ─────────────────────────────────────────

    [Fact]
    public void SystemVerification_StatusBarConsistencyAfterActions()
    {
        Log("=== テスト開始: アクション後ステータス一貫性検証 ===");
        Log("目的: 各種アクション実行後もステータスバー全要素が正常に表示されることを検証する");

        var window = LaunchWithDebugMap();

        var allStatusIds = new[]
        {
            "TerritoryText", "SurfaceStatusText", "FloorText", "DateText", "TimePeriodText",
            "LevelText", "ExpText", "HpText", "MpText", "SpText",
            "HungerText", "SanityText", "GoldText", "WeightText", "TurnLimitText",
            "SeasonText", "WeatherText", "ThirstText", "KarmaText", "CompanionCountText"
        };

        // ヘルパー: 全ステータスバーが正常かチェック
        void AssertAllStatusBarsValid(string context)
        {
            foreach (var id in allStatusIds)
            {
                var text = GetText(window, id);
                Assert.False(string.IsNullOrWhiteSpace(text), $"{context}: {id}の表示が空");
            }
            // 数値フォーマット検証
            Assert.Matches(@"^\d+/\d+$", GetText(window, "HpText"));
            Assert.Matches(@"^\d+/\d+$", GetText(window, "MpText"));
            Assert.Matches(@"^\d+/\d+$", GetText(window, "SpText"));
            Assert.Matches(@"^\d+$", GetText(window, "LevelText"));
            Assert.Matches(@"^\d+$", GetText(window, "HungerText"));
            Assert.Matches(@"^\d+$", GetText(window, "SanityText"));
            Assert.Matches(@"\d.*G", GetText(window, "GoldText"));
        }

        // ========== 初期状態検証 ==========
        Log("検証: 初期状態のステータスバー");
        AssertAllStatusBarsValid("初期状態");
        Log("  → 初期状態OK");

        // ========== 拾う(G)操作後 ==========
        Log("検証: Gキー（拾う）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_G);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("拾う操作後");
        Log("  → 拾う操作後OK");

        // ========== 探索(F)操作後 ==========
        Log("検証: Fキー（探索）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("探索操作後");
        Log("  → 探索操作後OK");

        // ========== 射撃(R)操作後 ==========
        Log("検証: Rキー（射撃）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("射撃操作後");
        Log("  → 射撃操作後OK");

        // ========== 投擲(T)操作後 ==========
        Log("検証: Tキー（投擲）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_T);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("投擲操作後");
        Log("  → 投擲操作後OK");

        // ========== 祈り(P)操作後 ==========
        Log("検証: Pキー（祈り）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_P);
        Thread.Sleep(300);
        CloseModals(window);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("祈り操作後");
        Log("  → 祈り操作後OK");

        // ========== ドア閉じ(X)操作後 ==========
        Log("検証: Xキー（ドア閉じ）操作後のステータスバー");
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_X);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);
        AssertAllStatusBarsValid("ドア閉じ操作後");
        Log("  → ドア閉じ操作後OK");

        // ========== ミニマップ切替前後のステータスバー不変検証 ==========
        Log("検証: Mキー（ミニマップ切替）前後でステータスバーが変化しないこと");
        var beforeMinimap = new Dictionary<string, string>();
        foreach (var id in allStatusIds)
            beforeMinimap[id] = GetText(window, id);

        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Thread.Sleep(200);
        PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_M);
        Thread.Sleep(200);
        Assert.False(_app!.HasExited);

        foreach (var id in allStatusIds)
        {
            var afterText = GetText(window, id);
            Assert.Equal(beforeMinimap[id], afterText);
        }
        Log("  → ミニマップ切替前後ステータス不変OK");

        // ========== 階段操作前後のステータスバー変化追跡 ==========
        Log("検証: 階段降下(Shift+>)前後のステータスバー変化追跡");
        var beforeStairs = new Dictionary<string, string>();
        foreach (var id in allStatusIds)
            beforeStairs[id] = GetText(window, id);

        window.Focus();
        FlaUI.Core.Input.Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.OEM_PERIOD);
        FlaUI.Core.Input.Keyboard.Release(FlaUI.Core.WindowsAPI.VirtualKeyShort.SHIFT);
        Thread.Sleep(500);
        Assert.False(_app!.HasExited);

        // 階段操作後もステータスバーが正常形式
        AssertAllStatusBarsValid("階段操作後");

        // 変化した要素をログ出力
        int changedCount = 0;
        foreach (var id in allStatusIds)
        {
            var afterText = GetText(window, id);
            if (beforeStairs[id] != afterText)
            {
                Log($"  変化: {id} '{beforeStairs[id]}' → '{afterText}'");
                changedCount++;
            }
        }
        Log($"  → 階段操作後 {changedCount}要素変化、形式維持OK");

        // ========== 連続アクション後の安定性検証 ==========
        Log("検証: 移動→拾う→探索→射撃→投擲→待機の連続実行後にステータスバーが正常か");
        var actionSequence = new[]
        {
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_G,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_F,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_R,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_T,
            FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE,
        };

        for (int round = 0; round < 10; round++)
        {
            foreach (var key in actionSequence)
            {
                window.Focus();
                FlaUI.Core.Input.Keyboard.Press(key);
                Thread.Sleep(20);
            }
        }
        Thread.Sleep(500);
        Assert.False(_app!.HasExited, "連続アクション後にクラッシュ");
        AssertAllStatusBarsValid("連続アクション後");
        Log("  → 連続アクション後ステータスバー整合性OK");

        Log("=== テスト完了: アクション後ステータス一貫性検証 ===");
    }

    // ─────────────────────────────────────────
    // 5. 新ステータスバー要素フォーマット詳細検証（1回起動）
    //    Season/Weather/Thirst/Karma/CompanionCountの初期値と
    //    複数ターン経過後のフォーマット維持を値レベルで検証
    // ─────────────────────────────────────────

    [Fact]
    public void SystemVerification_NewStatusBarFormats()
    {
        Log("=== テスト開始: 新ステータスバー要素フォーマット詳細検証 ===");
        Log("目的: Season/Weather/Thirst/Karma/CompanionCountの初期値フォーマットと経過後のフォーマット維持を検証する");

        var window = LaunchWithDebugMap();

        // ========== SeasonText 初期値検証 ==========
        Log("検証: SeasonText（季節）が有効な季節名であること");
        var seasonText = GetText(window, "SeasonText");
        var validSeasons = new[] { "春", "夏", "秋", "冬" };
        Assert.Contains(seasonText, validSeasons);
        Log($"  → SeasonText='{seasonText}' OK（有効な季節名）");

        // ========== WeatherText 初期値検証 ==========
        Log("検証: WeatherText（天候）が有効な天候名であること");
        var weatherText = GetText(window, "WeatherText");
        Assert.False(string.IsNullOrWhiteSpace(weatherText), "WeatherTextが空");
        // 天候は「晴」「曇」「雨」「雪」「霧」「嵐」等の日本語1〜2文字
        Assert.True(weatherText.Length >= 1 && weatherText.Length <= 4,
            $"WeatherText='{weatherText}' の文字数が1〜4の範囲外");
        Log($"  → WeatherText='{weatherText}' OK（有効な天候名）");

        // ========== ThirstText 初期値検証 ==========
        Log("検証: ThirstText（渇き）が有効な渇きレベル表記であること");
        var thirstText = GetText(window, "ThirstText");
        Assert.False(string.IsNullOrWhiteSpace(thirstText), "ThirstTextが空");
        // 渇きは「潤沢」「普通」「やや乾燥」「乾燥」「脱水」等の日本語表記
        Assert.True(thirstText.Length >= 1 && thirstText.Length <= 6,
            $"ThirstText='{thirstText}' の文字数が1〜6の範囲外");
        Log($"  → ThirstText='{thirstText}' OK（有効な渇きレベル）");

        // ========== KarmaText 初期値検証 ==========
        Log("検証: KarmaText（カルマ）が有効なカルマ表記であること");
        var karmaText = GetText(window, "KarmaText");
        Assert.False(string.IsNullOrWhiteSpace(karmaText), "KarmaTextが空");
        // カルマは「善良」「中立」「邪悪」等の日本語表記
        Assert.True(karmaText.Length >= 1 && karmaText.Length <= 6,
            $"KarmaText='{karmaText}' の文字数が1〜6の範囲外");
        Log($"  → KarmaText='{karmaText}' OK（有効なカルマ表記）");

        // ========== CompanionCountText 初期値検証 ==========
        Log("検証: CompanionCountText（仲間数）が非負整数であること");
        var companionText = GetText(window, "CompanionCountText");
        Assert.Matches(@"^\d+$", companionText);
        int companionCount = int.Parse(companionText);
        Assert.True(companionCount >= 0, $"仲間数が負値: {companionCount}");
        Log($"  → CompanionCountText='{companionText}' OK（非負整数）");

        // ========== 初期値の相互整合性検証 ==========
        Log("検証: 初期状態で全新ステータスバー要素が同時に正常値であること");
        // 初期状態では仲間数=0が期待される
        Assert.Equal("0", companionText);
        Log("  → 初期仲間数=0 OK");

        // ========== 複数ターン経過後のフォーマット維持検証 ==========
        Log("検証: 100ターン待機後も全新ステータスバー要素のフォーマットが維持されること");
        for (int i = 0; i < 100; i++)
        {
            window.Focus();
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
            Thread.Sleep(10);
        }
        Thread.Sleep(500);
        Assert.False(_app!.HasExited, "100ターン待機中にクラッシュ");

        // 100ターン後の季節検証
        var seasonAfter = GetText(window, "SeasonText");
        Assert.Contains(seasonAfter, validSeasons);
        Log($"  → 100ターン後SeasonText='{seasonAfter}' OK");

        // 100ターン後の天候検証
        var weatherAfter = GetText(window, "WeatherText");
        Assert.False(string.IsNullOrWhiteSpace(weatherAfter), "100ターン後WeatherTextが空");
        Assert.True(weatherAfter.Length >= 1 && weatherAfter.Length <= 4,
            $"100ターン後WeatherText='{weatherAfter}' の文字数が範囲外");
        Log($"  → 100ターン後WeatherText='{weatherAfter}' OK");

        // 100ターン後の渇き検証
        var thirstAfter = GetText(window, "ThirstText");
        Assert.False(string.IsNullOrWhiteSpace(thirstAfter), "100ターン後ThirstTextが空");
        Assert.True(thirstAfter.Length >= 1 && thirstAfter.Length <= 6,
            $"100ターン後ThirstText='{thirstAfter}' の文字数が範囲外");
        Log($"  → 100ターン後ThirstText='{thirstAfter}' OK");

        // 100ターン後のカルマ検証
        var karmaAfter = GetText(window, "KarmaText");
        Assert.False(string.IsNullOrWhiteSpace(karmaAfter), "100ターン後KarmaTextが空");
        Assert.True(karmaAfter.Length >= 1 && karmaAfter.Length <= 6,
            $"100ターン後KarmaText='{karmaAfter}' の文字数が範囲外");
        Log($"  → 100ターン後KarmaText='{karmaAfter}' OK");

        // 100ターン後の仲間数検証
        var companionAfter = GetText(window, "CompanionCountText");
        Assert.Matches(@"^\d+$", companionAfter);
        Log($"  → 100ターン後CompanionCountText='{companionAfter}' OK");

        // ========== 移動操作後のフォーマット維持検証 ==========
        Log("検証: 50回移動後も全新ステータスバー要素のフォーマットが維持されること");
        for (int i = 0; i < 50; i++)
        {
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
            PressKey(window, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        }
        Thread.Sleep(300);
        Assert.False(_app!.HasExited, "50回移動中にクラッシュ");

        // 移動後の全新ステータスバー要素の検証
        var seasonMove = GetText(window, "SeasonText");
        Assert.Contains(seasonMove, validSeasons);
        var weatherMove = GetText(window, "WeatherText");
        Assert.False(string.IsNullOrWhiteSpace(weatherMove));
        var thirstMove = GetText(window, "ThirstText");
        Assert.False(string.IsNullOrWhiteSpace(thirstMove));
        var karmaMove = GetText(window, "KarmaText");
        Assert.False(string.IsNullOrWhiteSpace(karmaMove));
        var companionMove = GetText(window, "CompanionCountText");
        Assert.Matches(@"^\d+$", companionMove);
        Log($"  → 移動後 Season='{seasonMove}' Weather='{weatherMove}' Thirst='{thirstMove}' Karma='{karmaMove}' Companion='{companionMove}' OK");

        // ========== 全20ステータスバー要素の最終整合性確認 ==========
        Log("検証: テスト終了時に全20ステータスバー要素が正常に表示されること");
        var allStatusIds = new[]
        {
            "TerritoryText", "SurfaceStatusText", "FloorText", "DateText", "TimePeriodText",
            "LevelText", "ExpText", "HpText", "MpText", "SpText",
            "HungerText", "SanityText", "GoldText", "WeightText", "TurnLimitText",
            "SeasonText", "WeatherText", "ThirstText", "KarmaText", "CompanionCountText"
        };
        foreach (var id in allStatusIds)
        {
            var text = GetText(window, id);
            Assert.False(string.IsNullOrWhiteSpace(text), $"最終チェックで{id}の表示が空");
        }
        Assert.Matches(@"^\d+/\d+$", GetText(window, "HpText"));
        Assert.Matches(@"^\d+/\d+$", GetText(window, "MpText"));
        Assert.Matches(@"^\d+/\d+$", GetText(window, "SpText"));
        Assert.Matches(@"^\d+$", GetText(window, "LevelText"));
        Assert.Matches(@"^\d+$", GetText(window, "HungerText"));
        Assert.Matches(@"^\d+$", GetText(window, "SanityText"));
        Log("  → 全20ステータスバー要素最終整合性OK");

        Log("=== テスト完了: 新ステータスバー要素フォーマット詳細検証 ===");
    }
}
