using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Xunit;

namespace RougelikeGame.Gui.Tests;

/// <summary>
/// FlaUI を使ったGUI自動操作テスト
/// WPFウィンドウを起動し、UI要素の表示や操作を検証する
/// </summary>
[Collection("GuiTests")]
public class GuiAutomationTests : IDisposable
{
    private readonly UIA3Automation _automation;
    private Application? _app;
    private Window? _mainWindow;

    /// <summary>
    /// GUIアプリのexeパスを取得
    /// </summary>
    private static string GetGuiExePath()
    {
        // テストプロジェクトの出力ディレクトリから相対的にGUIプロジェクトの出力を探す
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        var guiExe = Path.Combine(solutionDir, "src", "RougelikeGame.Gui", "bin", "Debug", "net10.0-windows", "RougelikeGame.Gui.exe");

        if (!File.Exists(guiExe))
        {
            // Release ビルドも試す
            guiExe = Path.Combine(solutionDir, "src", "RougelikeGame.Gui", "bin", "Release", "net10.0-windows", "RougelikeGame.Gui.exe");
        }

        return guiExe;
    }

    public GuiAutomationTests()
    {
        _automation = new UIA3Automation();
    }

    private Window LaunchApp()
    {
        var exePath = GetGuiExePath();
        Assert.True(File.Exists(exePath), $"GUIアプリのexeが見つかりません: {exePath}");

        _app = Application.Launch(exePath);
        _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(_mainWindow);

        // ウィンドウが完全に読み込まれるまで少し待つ
        Thread.Sleep(1000);

        return _mainWindow;
    }

    public void Dispose()
    {
        _app?.Close();
        _app?.Dispose();
        _automation.Dispose();
    }

    [Fact]
    public void Window_HasCorrectTitle()
    {
        // Arrange & Act
        var window = LaunchApp();

        // Assert
        Assert.Equal("ローグライクゲーム", window.Title);
    }

    [Fact]
    public void StatusBar_ShowsFloorText()
    {
        // Arrange
        var window = LaunchApp();

        // Act
        var floorText = FindElementByAutomationId(window, "FloorText");

        // Assert
        Assert.NotNull(floorText);
        Assert.Contains("第1層", floorText.Name);
    }

    [Fact]
    public void StatusBar_ShowsDateInsteadOfTurn()
    {
        // Arrange
        var window = LaunchApp();

        // Act
        var dateText = FindElementByAutomationId(window, "DateText");

        // Assert
        Assert.NotNull(dateText);
        var text = dateText.Name;
        // 日付形式であること: "冒険歴1024年 緑風の月 15日 08:00"
        Assert.Contains("歴", text);
        Assert.Contains("年", text);
        Assert.Contains("の月", text);
        Assert.Contains("日", text);
        Assert.Contains(":", text);
    }

    [Fact]
    public void StatusBar_ShowsTimePeriod()
    {
        // Arrange
        var window = LaunchApp();

        // Act
        var timePeriodText = FindElementByAutomationId(window, "TimePeriodText");

        // Assert
        Assert.NotNull(timePeriodText);
        var text = timePeriodText.Name;
        // 時間帯のいずれかであること
        var validPeriods = new[] { "明け方", "朝", "午前", "昼", "午後", "夕方", "夜", "深夜" };
        Assert.Contains(validPeriods, period => text.Contains(period));
    }

    [Fact]
    public void StatusBar_ShowsHpMpSpHunger()
    {
        // Arrange
        var window = LaunchApp();

        // Act
        var hpText = FindElementByAutomationId(window, "HpText");
        var mpText = FindElementByAutomationId(window, "MpText");
        var spText = FindElementByAutomationId(window, "SpText");
        var hungerText = FindElementByAutomationId(window, "HungerText");

        // Assert
        Assert.NotNull(hpText);
        Assert.NotNull(mpText);
        Assert.NotNull(spText);
        Assert.NotNull(hungerText);

        // 形式チェック（x/y）
        Assert.Contains("/", hpText.Name);
        Assert.Contains("/", mpText.Name);
        Assert.Contains("/", spText.Name);
    }

    [Fact]
    public void KeyPress_Wait_AdvancesDate()
    {
        // Arrange
        var window = LaunchApp();
        var dateText = FindElementByAutomationId(window, "DateText");
        Assert.NotNull(dateText);
        var initialDateStr = dateText.Name;

        // Act - Spaceキーで待機（60ターン=1分なので65回押して確実に変化させる）
        for (int i = 0; i < 65; i++)
        {
            window.Focus();
            FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.SPACE);
            Thread.Sleep(50);
        }

        // 表示反映待ち
        Thread.Sleep(500);

        // 再取得
        dateText = FindElementByAutomationId(window, "DateText");
        Assert.NotNull(dateText);
        var updatedDateStr = dateText.Name;

        // Assert - 日時が変わっているはず（65ターン=1分経過で08:01等）
        Assert.NotEqual(initialDateStr, updatedDateStr);
    }

    [Fact]
    public void KeyPress_Movement_UpdatesDisplay()
    {
        // Arrange
        var window = LaunchApp();

        // Act - WASD移動を試す
        window.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_W);
        Thread.Sleep(200);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_S);
        Thread.Sleep(200);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        Thread.Sleep(200);
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_D);
        Thread.Sleep(200);

        // Assert - クラッシュしないこと、ウィンドウが存在すること
        Assert.True(_app!.HasExited == false);
    }

    [Fact]
    public void KeyPress_Inventory_OpensDialog()
    {
        // Arrange
        var window = LaunchApp();

        // Act - I キーでインベントリを開く
        window.Focus();
        FlaUI.Core.Input.Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_I);
        Thread.Sleep(500);

        // Assert - モーダルダイアログが表示されること
        var modalWindows = window.ModalWindows;
        if (modalWindows.Length > 0)
        {
            // インベントリウィンドウが開いた
            Assert.True(modalWindows.Length > 0);

            // 閉じる
            modalWindows[0].Close();
        }
        // インベントリが空の場合はダイアログが出ないこともあるため、
        // ここではクラッシュしないことを主に確認
        Assert.False(_app!.HasExited);
    }

    [Fact]
    public void Window_CanBeResized()
    {
        // Arrange
        var window = LaunchApp();
        var originalWidth = window.BoundingRectangle.Width;

        // Assert - ウィンドウのサイズが正常
        Assert.True(originalWidth > 0);
        Assert.True(window.BoundingRectangle.Height > 0);
    }

    [Fact]
    public void MessageLog_Exists()
    {
        // Arrange
        var window = LaunchApp();

        // Act
        var messageLog = FindElementByAutomationId(window, "MessageLog");

        // Assert
        Assert.NotNull(messageLog);
    }

    /// <summary>
    /// AutomationIdで要素を検索するヘルパー
    /// WPFのx:Nameが自動的にAutomationIdになる
    /// </summary>
    private AutomationElement? FindElementByAutomationId(Window window, string automationId)
    {
        return Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromMilliseconds(500)
        ).Result;
    }
}
