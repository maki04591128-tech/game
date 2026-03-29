using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// クエストボードウィンドウ - 受注可能/進行中/完了報告のクエストを管理
/// </summary>
public partial class QuestBoardWindow : Window
{
    private readonly GameController _gameController;
    private enum QuestTab { Available, Active, Completed }
    private QuestTab _currentTab = QuestTab.Available;

    public QuestBoardWindow(GameController gameController)
    {
        InitializeComponent();
        _gameController = gameController;
        ShowAvailableQuests();
    }

    private void AvailableTab_Click(object sender, RoutedEventArgs e)
    {
        _currentTab = QuestTab.Available;
        UpdateTabStyles();
        ShowAvailableQuests();
    }

    private void ActiveTab_Click(object sender, RoutedEventArgs e)
    {
        _currentTab = QuestTab.Active;
        UpdateTabStyles();
        ShowActiveQuests();
    }

    private void CompletedTab_Click(object sender, RoutedEventArgs e)
    {
        _currentTab = QuestTab.Completed;
        UpdateTabStyles();
        ShowCompletedQuests();
    }

    private void UpdateTabStyles()
    {
        var activeBg = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60));
        var inactiveBg = new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3e));
        var activeBorder = new SolidColorBrush(Color.FromRgb(0x53, 0x34, 0x83));
        var inactiveBorder = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));

        AvailableTabButton.Background = _currentTab == QuestTab.Available ? activeBg : inactiveBg;
        AvailableTabButton.Foreground = _currentTab == QuestTab.Available ? Brushes.White : Brushes.Gray;
        AvailableTabButton.BorderBrush = _currentTab == QuestTab.Available ? activeBorder : inactiveBorder;

        ActiveTabButton.Background = _currentTab == QuestTab.Active ? activeBg : inactiveBg;
        ActiveTabButton.Foreground = _currentTab == QuestTab.Active ? Brushes.White : Brushes.Gray;
        ActiveTabButton.BorderBrush = _currentTab == QuestTab.Active ? activeBorder : inactiveBorder;

        CompletedTabButton.Background = _currentTab == QuestTab.Completed ? activeBg : inactiveBg;
        CompletedTabButton.Foreground = _currentTab == QuestTab.Completed ? Brushes.White : Brushes.Gray;
        CompletedTabButton.BorderBrush = _currentTab == QuestTab.Completed ? activeBorder : inactiveBorder;
    }

    private void ShowAvailableQuests()
    {
        QuestPanel.Children.Clear();
        var quests = _gameController.GetAvailableQuests();

        if (quests.Count == 0)
        {
            AddEmptyMessage("受注可能なクエストはありません");
            return;
        }

        foreach (var quest in quests)
        {
            var border = CreateQuestBorder();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoPanel = CreateQuestInfoPanel(quest.Name, quest.Description,
                $"種類: {GetQuestTypeName(quest.Type)}  必要Lv: {quest.RequiredLevel}  ランク: {GuildSystem.GetRankName(quest.RequiredGuildRank)}",
                $"報酬: {quest.Reward.Gold}G / {quest.Reward.Experience}EXP / ギルドPt: {quest.Reward.GuildPoints}");

            Grid.SetColumn(infoPanel, 0);
            grid.Children.Add(infoPanel);

            var acceptButton = new Button
            {
                Content = "受注",
                Width = 70,
                Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x53, 0x34, 0x83)),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = quest.Id
            };
            acceptButton.Click += AcceptButton_Click;

            Grid.SetColumn(acceptButton, 1);
            grid.Children.Add(acceptButton);

            border.Child = grid;
            QuestPanel.Children.Add(border);
        }
    }

    private void ShowActiveQuests()
    {
        QuestPanel.Children.Clear();
        var quests = _gameController.GetActiveQuests();

        if (quests.Count == 0)
        {
            AddEmptyMessage("進行中のクエストはありません");
            return;
        }

        foreach (var (quest, progress) in quests)
        {
            var border = CreateQuestBorder();
            var infoPanel = CreateQuestInfoPanel(quest.Name, quest.Description, "", "");

            // 目標進捗
            foreach (var obj in progress.Objectives)
            {
                var objText = new TextBlock
                {
                    Text = $"  {(obj.IsComplete ? "✅" : "⬜")} {obj.Description} ({obj.CurrentCount}/{obj.TargetCount})",
                    FontSize = 12,
                    Foreground = obj.IsComplete
                        ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                        : new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xcc)),
                    Margin = new Thickness(0, 2, 0, 0)
                };
                infoPanel.Children.Add(objText);
            }

            border.Child = infoPanel;
            QuestPanel.Children.Add(border);
        }
    }

    private void ShowCompletedQuests()
    {
        QuestPanel.Children.Clear();
        var quests = _gameController.GetActiveQuests();
        var completable = quests.Where(q => q.Progress.IsComplete).ToList();

        if (completable.Count == 0)
        {
            AddEmptyMessage("報告可能なクエストはありません");
            return;
        }

        foreach (var (quest, progress) in completable)
        {
            var border = CreateQuestBorder();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoPanel = CreateQuestInfoPanel(quest.Name, "全目標達成！報告して報酬を受け取ろう",
                "", $"報酬: {quest.Reward.Gold}G / {quest.Reward.Experience}EXP / ギルドPt: {quest.Reward.GuildPoints}");

            Grid.SetColumn(infoPanel, 0);
            grid.Children.Add(infoPanel);

            var turnInButton = new Button
            {
                Content = "報告",
                Width = 70,
                Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(0x2a, 0x6a, 0x2a)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = quest.Id
            };
            turnInButton.Click += TurnInButton_Click;

            Grid.SetColumn(turnInButton, 1);
            grid.Children.Add(turnInButton);

            border.Child = grid;
            QuestPanel.Children.Add(border);
        }
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string questId)
        {
            _gameController.TryAcceptQuest(questId);
            ShowAvailableQuests();
        }
    }

    private void TurnInButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string questId)
        {
            _gameController.TryTurnInQuest(questId);
            ShowCompletedQuests();
        }
    }

    private Border CreateQuestBorder()
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3e)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 0, 0, 6)
        };
    }

    private static StackPanel CreateQuestInfoPanel(string name, string description, string details, string reward)
    {
        var panel = new StackPanel();

        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        panel.Children.Add(nameText);

        if (!string.IsNullOrEmpty(description))
        {
            var descText = new TextBlock
            {
                Text = description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xcc)),
                Margin = new Thickness(0, 3, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            panel.Children.Add(descText);
        }

        if (!string.IsNullOrEmpty(details))
        {
            var detailText = new TextBlock
            {
                Text = details,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xaa)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(detailText);
        }

        if (!string.IsNullOrEmpty(reward))
        {
            var rewardText = new TextBlock
            {
                Text = reward,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xd7, 0x00)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(rewardText);
        }

        return panel;
    }

    private void AddEmptyMessage(string message)
    {
        var text = new TextBlock
        {
            Text = message,
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 0)
        };
        QuestPanel.Children.Add(text);
    }

    private static string GetQuestTypeName(QuestType type) => type switch
    {
        QuestType.Main => "メイン",
        QuestType.Kill => "討伐",
        QuestType.Collect => "収集",
        QuestType.Explore => "探索",
        QuestType.Deliver => "配達",
        QuestType.Escort => "護衛",
        QuestType.Talk => "会話",
        _ => "その他"
    };

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
