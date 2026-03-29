using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// スキルツリーウィンドウ - パッシブノード閲覧・解放・リスペック
/// </summary>
public partial class SkillTreeWindow : Window
{
    private readonly GameController _controller;
    private readonly SkillTreeSystem _tree;
    private List<SkillNodeViewModel> _nodeViewModels = new();
    private Point _dragStartPoint;

    public SkillTreeWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;
        _tree = controller.GetSkillTreeSystem();

        LoadData();
    }

    private void LoadData()
    {
        PointsText.Text = $"スキルポイント: {_tree.AvailablePoints} | 解放済み: {_tree.UnlockedCount}/{_tree.AllNodes.Count}";

        var bonuses = _tree.GetTotalStatBonuses();
        TotalBonusText.Text = bonuses.Count > 0
            ? string.Join("  ", bonuses.Select(b => $"{b.Key}: +{b.Value}"))
            : "なし";

        var downsides = _tree.GetActiveKeystoneDownsides();
        if (downsides.Count > 0)
        {
            KeystoneWarning.Visibility = Visibility.Visible;
            KeystoneWarningText.Text = "⚠ キーストーンデメリット: " + string.Join(", ", downsides);
        }
        else
        {
            KeystoneWarning.Visibility = Visibility.Collapsed;
        }

        BuildNodeList();
    }

    private void BuildNodeList()
    {
        int playerLevel = _controller.Player.Level;
        _nodeViewModels = _tree.AllNodes.Values
            .OrderBy(n => n.Tier)
            .ThenBy(n => n.NodeType)
            .ThenBy(n => n.RequiredClass?.ToString() ?? "")
            .ThenBy(n => n.RequiredRace?.ToString() ?? "")
            .ThenBy(n => n.PointCost)
            .Select(n =>
            {
                bool unlocked = _tree.UnlockedNodes.Contains(n.Id);
                bool canUnlock = _tree.CanUnlock(n.Id, playerLevel);
                bool levelLocked = playerLevel < n.RequiredLevel;
                return new SkillNodeViewModel
                {
                    Id = n.Id,
                    Name = n.Name,
                    StatusIcon = unlocked ? "✓" : canUnlock ? "◆" : levelLocked ? "🔒" : "○",
                    TypeIcon = GetTypeIcon(n.NodeType),
                    CostText = $"[{n.PointCost}pt]",
                    TierLabel = $"T{n.Tier}",
                    LevelText = n.RequiredLevel > 1 ? $"Lv{n.RequiredLevel}" : "",
                    NameColor = unlocked
                        ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                        : canUnlock
                            ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d))
                            : levelLocked
                                ? new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x60))
                                : new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
                    Node = n,
                    IsUnlocked = unlocked,
                    CanUnlock = canUnlock
                };
            })
            .ToList();

        NodeList.ItemsSource = _nodeViewModels;
    }

    private static string GetTypeIcon(SkillNodeType type)
    {
        return type switch
        {
            SkillNodeType.StatMinor => "▪",
            SkillNodeType.StatMajor => "▫",
            SkillNodeType.Passive => "◈",
            SkillNodeType.Keystone => "★",
            SkillNodeType.Active => "⚡",
            _ => "?"
        };
    }

    private static string GetTypeName(SkillNodeType type)
    {
        return type switch
        {
            SkillNodeType.StatMinor => "ステータス（小）",
            SkillNodeType.StatMajor => "ステータス（大）",
            SkillNodeType.Passive => "パッシブ",
            SkillNodeType.Keystone => "キーストーン",
            SkillNodeType.Active => "アクティブ",
            _ => "不明"
        };
    }

    private void NodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NodeList.SelectedItem is SkillNodeViewModel vm)
        {
            var node = vm.Node;
            NodeNameText.Text = node.Name;
            NodeTypeText.Text = $"{GetTypeName(node.NodeType)}" +
                                (node.RequiredClass != null ? $" | クラス: {node.RequiredClass}" : "") +
                                (node.RequiredRace != null ? $" | 種族: {node.RequiredRace}" : "") +
                                (node.RequiredClass == null && node.RequiredRace == null ? " | 共通" : "");
            NodeTierText.Text = $"Tier {node.Tier} | 必要レベル: {node.RequiredLevel}";
            NodeDescText.Text = node.Description;

            if (node.Prerequisites.Length > 0)
            {
                var prereqNames = node.Prerequisites
                    .Select(id => _tree.AllNodes.TryGetValue(id, out var n) ? n.Name : id)
                    .ToList();
                NodePrereqText.Text = $"前提: {string.Join(", ", prereqNames)}";
            }
            else
            {
                NodePrereqText.Text = "前提: なし";
            }

            NodeBonusText.Text = node.StatBonuses.Count > 0
                ? "ボーナス: " + string.Join(", ", node.StatBonuses.Select(b => $"{b.Key} +{b.Value}"))
                : "";

            NodeDownsideText.Text = node.KeystoneDownside != null
                ? $"⚠ デメリット: {node.KeystoneDownside}"
                : "";

            UnlockButton.IsEnabled = vm.CanUnlock;
            UnlockButton.Content = vm.IsUnlocked ? "解放済み" : "解放 [Enter]";
        }
        else
        {
            ClearDetail();
        }
    }

    private void ClearDetail()
    {
        NodeNameText.Text = "ノードを選択してください";
        NodeTypeText.Text = "";
        NodeTierText.Text = "";
        NodeDescText.Text = "";
        NodePrereqText.Text = "";
        NodeBonusText.Text = "";
        NodeDownsideText.Text = "";
        UnlockButton.IsEnabled = false;
        UnlockButton.Content = "解放 [Enter]";
    }

    private void UnlockButton_Click(object sender, RoutedEventArgs e)
    {
        TryUnlockSelected();
    }

    private void TryUnlockSelected()
    {
        if (NodeList.SelectedItem is SkillNodeViewModel vm && vm.CanUnlock)
        {
            if (vm.Node.NodeType == SkillNodeType.Keystone)
            {
                var result = MessageBox.Show(
                    $"キーストーン「{vm.Node.Name}」を解放しますか？\n\nデメリット: {vm.Node.KeystoneDownside}",
                    "キーストーン確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            _tree.UnlockNode(vm.Id, _controller.Player.Level);
            LoadData();
        }
    }

    private void RespecButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tree.UnlockedCount == 0)
        {
            MessageBox.Show("解放済みのノードがありません。", "リスペック", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"全てのスキルノード({_tree.UnlockedCount}個)を解除してポイントを返還しますか？",
            "リスペック確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            int refunded = _tree.Respec();
            LoadData();
            MessageBox.Show($"{refunded}ポイントが返還されました。", "リスペック完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                TryUnlockSelected();
                break;
            case Key.R:
                RespecButton_Click(sender, e);
                break;
            case Key.E:
            case Key.Escape:
                Close();
                break;
        }
    }

    // ── タブ切り替え ──

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl)
        {
            BuildUnlockedSkillList();
            BuildSkillSlotPanel();
        }
    }

    // ── 習得済みスキルタブ ──

    private void BuildUnlockedSkillList()
    {
        if (UnlockedSkillList == null) return;

        var unlockedVms = _tree.UnlockedNodes
            .Where(id => _tree.AllNodes.ContainsKey(id))
            .Select(id => _tree.AllNodes[id])
            .OrderBy(n => n.NodeType)
            .ThenBy(n => n.Name)
            .Select(n => new UnlockedSkillViewModel
            {
                Id = n.Id,
                Name = n.Name,
                Description = n.Description,
                TypeIcon = GetTypeIcon(n.NodeType)
            })
            .ToList();

        UnlockedSkillList.ItemsSource = unlockedVms;
    }

    private void BuildSkillSlotPanel()
    {
        if (SkillSlotPanel == null) return;

        SkillSlotPanel.Children.Clear();

        for (int i = 0; i < SkillTreeSystem.MaxSkillSlots; i++)
        {
            var equipped = i < _tree.EquippedSkillSlots.Count ? _tree.EquippedSkillSlots[i] : null;
            var node = equipped != null && _tree.AllNodes.TryGetValue(equipped, out var n) ? n : null;

            var slotBorder = new Border
            {
                Background = node != null
                    ? new SolidColorBrush(Color.FromRgb(0x1a, 0x3a, 0x2a))
                    : new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x2e)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 2, 0, 2),
                Tag = i,
                AllowDrop = true,
            };
            slotBorder.Drop += SkillSlot_Drop;
            slotBorder.DragOver += SkillSlot_DragOver;

            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock
            {
                Text = $"[{i + 1}] ",
                Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
                FontSize = 12,
                Width = 28
            });

            if (node != null)
            {
                sp.Children.Add(new TextBlock
                {
                    Text = node.Name,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                });

                var removeBtn = new Button
                {
                    Content = "×",
                    Width = 20, Height = 20,
                    Margin = new Thickness(6, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(0x3a, 0x10, 0x20)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60)),
                    BorderThickness = new Thickness(0),
                    FontSize = 10,
                    Padding = new Thickness(0),
                    Tag = i
                };
                removeBtn.Click += RemoveSkillSlot_Click;
                sp.Children.Add(removeBtn);
            }
            else
            {
                sp.Children.Add(new TextBlock
                {
                    Text = "── 空 ──",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80)),
                    FontSize = 12
                });
            }

            slotBorder.Child = sp;
            SkillSlotPanel.Children.Add(slotBorder);
        }

        if (SlotInfoText != null)
            SlotInfoText.Text = $"装備中: {_tree.EquippedSkillSlots.Count}/{SkillTreeSystem.MaxSkillSlots}\nドラッグ＆ドロップでスロットに装備";
    }

    // ── D&D: 習得済みスキルリスト → スキルスロット ──

    private void UnlockedSkillList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void UnlockedSkillList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(null);
        var diff = pos - _dragStartPoint;
        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (UnlockedSkillList.SelectedItem is UnlockedSkillViewModel vm)
        {
            var data = new DataObject("SkillNodeId", vm.Id);
            DragDrop.DoDragDrop(UnlockedSkillList, data, DragDropEffects.Copy);
        }
    }

    private void SkillSlotPanel_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent("SkillNodeId") ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void SkillSlotPanel_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData("SkillNodeId") is string nodeId)
        {
            if (_tree.EquipSkillToSlot(nodeId))
            {
                BuildSkillSlotPanel();
            }
        }
    }

    private void SkillSlot_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent("SkillNodeId") ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void SkillSlot_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData("SkillNodeId") is string nodeId)
        {
            if (_tree.EquipSkillToSlot(nodeId))
            {
                BuildSkillSlotPanel();
            }
        }
        e.Handled = true;
    }

    private void RemoveSkillSlot_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int slotIdx)
        {
            _tree.UnequipSkillSlot(slotIdx);
            BuildSkillSlotPanel();
        }
    }
}

public class SkillNodeViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string StatusIcon { get; set; } = "";
    public string TypeIcon { get; set; } = "";
    public string CostText { get; set; } = "";
    public string TierLabel { get; set; } = "";
    public string LevelText { get; set; } = "";
    public Brush NameColor { get; set; } = Brushes.White;
    public SkillNodeDefinition Node { get; set; } = null!;
    public bool IsUnlocked { get; set; }
    public bool CanUnlock { get; set; }
}

public class UnlockedSkillViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string TypeIcon { get; set; } = "";
}
