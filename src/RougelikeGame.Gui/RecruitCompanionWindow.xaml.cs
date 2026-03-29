using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 仲間募集ウィンドウ - ギルドで仲間候補を表示し雇用する
/// </summary>
public partial class RecruitCompanionWindow : Window
{
    private readonly GameController _gameController;
    private readonly List<CompanionSystem.CompanionData> _candidates;

    public RecruitCompanionWindow(GameController gameController, List<CompanionSystem.CompanionData> candidates)
    {
        InitializeComponent();
        _gameController = gameController;
        _candidates = candidates;
        BuildCandidateList();
    }

    private void BuildCandidateList()
    {
        CandidatePanel.Children.Clear();
        foreach (var candidate in _candidates)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3e)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 情報パネル
            var infoPanel = new StackPanel();

            var typeName = CompanionSystem.GetTypeName(candidate.Type);
            var nameText = new TextBlock
            {
                Text = $"{candidate.Name}（{typeName}）Lv.{candidate.Level}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            infoPanel.Children.Add(nameText);

            var statsText = new TextBlock
            {
                Text = $"HP: {candidate.MaxHp}  攻撃: {candidate.Attack}  防御: {candidate.Defense}  忠誠: {candidate.Loyalty}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xcc)),
                Margin = new Thickness(0, 3, 0, 0)
            };
            infoPanel.Children.Add(statsText);

            var costText = new TextBlock
            {
                Text = $"雇用費: {candidate.HireCost}G",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xd7, 0x00)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            infoPanel.Children.Add(costText);

            Grid.SetColumn(infoPanel, 0);
            grid.Children.Add(infoPanel);

            // 雇用ボタン
            var hireButton = new Button
            {
                Content = "雇用する",
                Width = 80,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x53, 0x34, 0x83)),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = candidate
            };
            hireButton.Click += HireButton_Click;

            Grid.SetColumn(hireButton, 1);
            grid.Children.Add(hireButton);

            border.Child = grid;
            CandidatePanel.Children.Add(border);
        }
    }

    private void HireButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CompanionSystem.CompanionData candidate)
        {
            if (_gameController.TryHireCompanion(candidate))
            {
                _candidates.Remove(candidate);
                BuildCandidateList();

                if (_candidates.Count == 0)
                {
                    DialogResult = true;
                    Close();
                }
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
