using System.Windows;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 領地間移動イベントウィンドウ
/// </summary>
public partial class TravelEventWindow : Window
{
    private readonly GameController _controller;
    private readonly TravelEvent _event;
    private bool _resolved;

    public TravelEventWindow(GameController controller, TravelEvent travelEvent)
    {
        InitializeComponent();
        _controller = controller;
        _event = travelEvent;

        EventTitle.Text = _event.Name;
        EventDescription.Text = _event.Description;

        // イベントタイプによってボタンの表示を調整
        switch (_event.Type)
        {
            case TravelEventType.Merchant:
                ForceButton.Visibility = Visibility.Collapsed;
                NegotiateButton.Content = "取引する [2]";
                EvadeButton.Content = "立ち去る [3]";
                break;
            case TravelEventType.Shrine:
                ForceButton.Visibility = Visibility.Collapsed;
                NegotiateButton.Content = "祈る [2]";
                EvadeButton.Content = "通り過ぎる [3]";
                break;
            case TravelEventType.BadWeather:
                ForceButton.Content = "強行する [1]";
                NegotiateButton.Visibility = Visibility.Collapsed;
                EvadeButton.Content = "待機する [3]";
                break;
            case TravelEventType.TreasureChest:
                ForceButton.Content = "開ける [1]";
                NegotiateButton.Content = "調べる [2]";
                EvadeButton.Content = "無視する [3]";
                break;
            case TravelEventType.HelpRequest:
                ForceButton.Visibility = Visibility.Collapsed;
                NegotiateButton.Content = "助ける [2]";
                EvadeButton.Content = "無視する [3]";
                break;
            case TravelEventType.Ambush:
                ForceButton.Content = "応戦する [1]";
                NegotiateButton.Content = "交渉する [2]";
                EvadeButton.Content = "逃げる [3]";
                break;
        }
    }

    private void ShowResult(string message)
    {
        _resolved = true;
        ResultText.Text = message;
        ChoicePanel.Visibility = Visibility.Collapsed;
        CloseBtn.Visibility = Visibility.Visible;
    }

    private void ForceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_resolved) return;

        string result = _event.Type switch
        {
            TravelEventType.Ambush => "敵を撃退した！経験を積んだ。",
            TravelEventType.BadWeather => "悪天候の中を強行した。体力を消耗したが先に進んだ。",
            TravelEventType.TreasureChest => "宝箱を力づくで開けた！",
            _ => "力で解決した。"
        };

        ShowResult(result);
    }

    private void NegotiateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_resolved) return;

        string result = _event.Type switch
        {
            TravelEventType.Merchant => "商人と取引を行った。",
            TravelEventType.Ambush => "交渉で敵を退けた。",
            TravelEventType.Shrine => "祠に祈りを捧げた。心が清められた。",
            TravelEventType.HelpRequest => "困っている人を助けた。感謝された。",
            TravelEventType.TreasureChest => "慎重に調べてから開けた。",
            _ => "交渉で解決した。"
        };

        ShowResult(result);
    }

    private void EvadeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_resolved) return;
        ShowResult("その場を離れた。");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1 or Key.NumPad1:
                if (ForceButton.Visibility == Visibility.Visible)
                    ForceButton_Click(sender, e);
                break;
            case Key.D2 or Key.NumPad2:
                if (NegotiateButton.Visibility == Visibility.Visible)
                    NegotiateButton_Click(sender, e);
                break;
            case Key.D3 or Key.NumPad3:
                EvadeButton_Click(sender, e);
                break;
            case Key.Escape:
                if (_resolved)
                {
                    DialogResult = true;
                    Close();
                }
                break;
        }
        e.Handled = true;
    }
}
