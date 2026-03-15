using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// クエストログウィンドウ
/// </summary>
public partial class QuestLogWindow : Window
{
    private readonly GameController _controller;
    private bool _showActive = true;
    private List<QuestViewModel> _currentItems = new();

    public QuestLogWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;

        UpdateGuildInfo();
        LoadActiveQuests();
        UpdateTabVisuals();
        CompletedCountText.Text = $"完了: {_controller.CompletedQuestCount}件";
    }

    private void UpdateGuildInfo()
    {
        if (_controller.IsGuildRegistered)
        {
            var rank = _controller.GetGuildRank();
            var points = _controller.GetGuildPoints();
            var nextPoints = _controller.GetPointsForNextRank();
            GuildInfoText.Text = $"ギルドランク: {GetRankName(rank)} ({points}/{nextPoints}pt)";
        }
        else
        {
            GuildInfoText.Text = "ギルド未登録";
        }
    }

    private void LoadActiveQuests()
    {
        _currentItems.Clear();
        var quests = _controller.GetActiveQuests();
        foreach (var (quest, progress) in quests)
        {
            _currentItems.Add(new QuestViewModel(quest, progress, false));
        }
        RefreshList();
        AcceptButton.Visibility = Visibility.Collapsed;
        TurnInButton.Visibility = Visibility.Visible;
        TurnInButton.IsEnabled = false;
    }

    private void LoadAvailableQuests()
    {
        _currentItems.Clear();
        var quests = _controller.GetAvailableQuests();
        foreach (var quest in quests)
        {
            _currentItems.Add(new QuestViewModel(quest, null, true));
        }
        RefreshList();
        TurnInButton.Visibility = Visibility.Collapsed;
        AcceptButton.Visibility = Visibility.Visible;
        AcceptButton.IsEnabled = false;
    }

    private void RefreshList()
    {
        QuestList.ItemsSource = null;
        QuestList.ItemsSource = _currentItems;
        QuestDetailText.Text = "クエストを選択してください";
        QuestRewardText.Text = "";
    }

    private void UpdateTabVisuals()
    {
        if (_showActive)
        {
            ActiveTab.Background = System.Windows.Media.Brushes.SeaGreen;
            AvailableTab.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0f3460"));
        }
        else
        {
            AvailableTab.Background = System.Windows.Media.Brushes.SeaGreen;
            ActiveTab.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0f3460"));
        }
    }

    private void ActiveTab_Click(object sender, RoutedEventArgs e)
    {
        if (_showActive) return;
        _showActive = true;
        LoadActiveQuests();
        UpdateTabVisuals();
    }

    private void AvailableTab_Click(object sender, RoutedEventArgs e)
    {
        if (!_showActive) return;
        _showActive = false;
        LoadAvailableQuests();
        UpdateTabVisuals();
    }

    private void QuestList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuestList.SelectedItem is not QuestViewModel selected)
        {
            QuestDetailText.Text = "クエストを選択してください";
            QuestRewardText.Text = "";
            AcceptButton.IsEnabled = false;
            TurnInButton.IsEnabled = false;
            return;
        }

        QuestDetailText.Text = $"{selected.Quest.Description}\n" +
            $"依頼者: {selected.Quest.GiverNpcId} | 必要Lv.{selected.Quest.RequiredLevel}";

        var reward = selected.Quest.Reward;
        var rewardParts = new List<string>();
        if (reward.Gold > 0) rewardParts.Add($"{reward.Gold}G");
        if (reward.Experience > 0) rewardParts.Add($"EXP {reward.Experience}");
        if (reward.GuildPoints > 0) rewardParts.Add($"GP {reward.GuildPoints}");
        QuestRewardText.Text = rewardParts.Count > 0 ? $"報酬: {string.Join(" / ", rewardParts)}" : "";

        if (selected.IsAvailable)
        {
            AcceptButton.IsEnabled = true;
        }
        else if (selected.Progress?.IsComplete == true)
        {
            TurnInButton.IsEnabled = true;
        }
        else
        {
            TurnInButton.IsEnabled = false;
        }
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (QuestList.SelectedItem is not QuestViewModel selected) return;
        _controller.TryAcceptQuest(selected.Quest.Id);
        LoadAvailableQuests();
    }

    private void TurnInButton_Click(object sender, RoutedEventArgs e)
    {
        if (QuestList.SelectedItem is not QuestViewModel selected) return;
        _controller.TryTurnInQuest(selected.Quest.Id);
        LoadActiveQuests();
        CompletedCountText.Text = $"完了: {_controller.CompletedQuestCount}件";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                DialogResult = false;
                Close();
                break;
            case Key.Enter:
                if (_showActive && TurnInButton.IsEnabled)
                    TurnInButton_Click(sender, e);
                else if (!_showActive && AcceptButton.IsEnabled)
                    AcceptButton_Click(sender, e);
                break;
        }
        e.Handled = true;
    }

    private static string GetRankName(GuildRank rank) => rank switch
    {
        GuildRank.None => "なし",
        GuildRank.Copper => "銅",
        GuildRank.Iron => "鉄",
        GuildRank.Silver => "銀",
        GuildRank.Gold => "金",
        GuildRank.Platinum => "白金",
        GuildRank.Mythril => "ミスリル",
        GuildRank.Adamantine => "アダマンタイト",
        _ => rank.ToString()
    };

    public class QuestViewModel
    {
        public QuestDefinition Quest { get; }
        public QuestSystem.QuestProgress? Progress { get; }
        public bool IsAvailable { get; }

        public string Name => Quest.Name;
        public string StatusIcon => IsAvailable ? "📋" : (Progress?.IsComplete == true ? "✅" : "🔄");
        public string TypeDisplay => $"[{GetTypeName(Quest.Type)}]";

        public string ProgressText
        {
            get
            {
                if (IsAvailable) return $"必要Lv.{Quest.RequiredLevel}";
                if (Progress == null) return "";
                var objectives = Progress.Objectives;
                return string.Join(" | ", objectives.Select(o =>
                    $"{o.Description}: {o.CurrentCount}/{o.TargetCount}" + (o.IsComplete ? " ✓" : "")));
            }
        }

        public QuestViewModel(QuestDefinition quest, QuestSystem.QuestProgress? progress, bool isAvailable)
        {
            Quest = quest;
            Progress = progress;
            IsAvailable = isAvailable;
        }

        private static string GetTypeName(QuestType type) => type switch
        {
            QuestType.Kill => "討伐",
            QuestType.Collect => "収集",
            QuestType.Explore => "探索",
            QuestType.Escort => "護衛",
            QuestType.Deliver => "配達",
            QuestType.Talk => "会話",
            _ => type.ToString()
        };
    }
}
