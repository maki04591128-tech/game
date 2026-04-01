using System.Windows;
using System.Windows.Input;

namespace RougelikeGame.Gui;

/// <summary>
/// メッセージログ履歴閲覧ウィンドウ
/// </summary>
public partial class MessageLogWindow : Window
{
    private readonly List<string> _allMessages;
    private string _currentFilter = "all";

    public MessageLogWindow(List<string> messages)
    {
        InitializeComponent();
        _allMessages = new List<string>(messages);
        ApplyFilter("all");
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
            case Key.L:
                Close();
                break;
            case Key.Up:
                LogScroller.ScrollToVerticalOffset(LogScroller.VerticalOffset - 20);
                e.Handled = true;
                break;
            case Key.Down:
                LogScroller.ScrollToVerticalOffset(LogScroller.VerticalOffset + 20);
                e.Handled = true;
                break;
            case Key.Home:
                LogScroller.ScrollToTop();
                e.Handled = true;
                break;
            case Key.End:
                LogScroller.ScrollToEnd();
                e.Handled = true;
                break;
        }
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string tag)
        {
            ApplyFilter(tag);
        }
    }

    private void ApplyFilter(string filter)
    {
        _currentFilter = filter;

        var filtered = filter switch
        {
            "combat" => _allMessages.Where(m => IsCombatMessage(m)),
            "item" => _allMessages.Where(m => IsItemMessage(m)),
            "system" => _allMessages.Where(m => IsSystemMessage(m)),
            "explore" => _allMessages.Where(m => IsExploreMessage(m)),
            _ => _allMessages.AsEnumerable()
        };

        LogText.Text = string.Join("\n", filtered);
        LogScroller.ScrollToEnd();

        // フィルタボタンのハイライト
        UpdateFilterButtonStyles(filter);
    }

    private void UpdateFilterButtonStyles(string activeFilter)
    {
        var activeBg = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x4E, 0xCC, 0xA3));
        var inactiveBg = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x0F, 0x34, 0x60));
        var blackFg = System.Windows.Media.Brushes.Black;
        var whiteFg = System.Windows.Media.Brushes.White;

        BtnAll.Background = activeFilter == "all" ? activeBg : inactiveBg;
        BtnAll.Foreground = activeFilter == "all" ? blackFg : whiteFg;
        BtnCombat.Background = activeFilter == "combat" ? activeBg : inactiveBg;
        BtnCombat.Foreground = activeFilter == "combat" ? blackFg : whiteFg;
        BtnItem.Background = activeFilter == "item" ? activeBg : inactiveBg;
        BtnItem.Foreground = activeFilter == "item" ? blackFg : whiteFg;
        BtnSystem.Background = activeFilter == "system" ? activeBg : inactiveBg;
        BtnSystem.Foreground = activeFilter == "system" ? blackFg : whiteFg;
        BtnExplore.Background = activeFilter == "explore" ? activeBg : inactiveBg;
        BtnExplore.Foreground = activeFilter == "explore" ? blackFg : whiteFg;
    }

    /// <summary>戦闘関連メッセージ判定</summary>
    internal static bool IsCombatMessage(string msg) =>
        msg.Contains("ダメージ") || msg.Contains("攻撃") || msg.Contains("倒した") ||
        msg.Contains("クリティカル") || msg.Contains("外れた") || msg.Contains("罠");

    /// <summary>アイテム関連メッセージ判定</summary>
    internal static bool IsItemMessage(string msg) =>
        msg.Contains("拾った") || msg.Contains("装備") || msg.Contains("使った") ||
        msg.Contains("落とした") || msg.Contains("アイテム") || msg.Contains("飲んだ") ||
        msg.Contains("食べた") || msg.Contains("回復");

    /// <summary>システム関連メッセージ判定</summary>
    internal static bool IsSystemMessage(string msg) =>
        msg.Contains("レベル") || msg.Contains("正気") || msg.Contains("空腹") ||
        msg.Contains("餓死") || msg.Contains("狂気") || msg.Contains("不安") ||
        msg.Contains("ゲームオーバー") || msg.Contains("意識") || msg.Contains("救出") ||
        msg.Contains("お腹");

    /// <summary>探索関連メッセージ判定</summary>
    internal static bool IsExploreMessage(string msg) =>
        msg.Contains("階段") || msg.Contains("ドア") || msg.Contains("降りた") ||
        msg.Contains("上がった") || msg.Contains("入った") || msg.Contains("探索") ||
        msg.Contains("帰還") || msg.Contains("脱出");
}
