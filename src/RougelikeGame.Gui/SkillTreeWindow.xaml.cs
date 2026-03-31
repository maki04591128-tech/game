using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// PoE風スキルツリーウィンドウ - アイコンを線で繋げた5タブビジュアルツリー
/// タブ: 種族 / 職業 / 素性 / 武器 / 魔法
/// </summary>
public partial class SkillTreeWindow : Window
{
    private const double NodeRadius = 22;
    private const double KeystoneRadius = 28;

    private readonly GameController _controller;
    private readonly SkillTreeSystem _tree;
    private string? _selectedNodeId;
    private SkillTreeTab _currentTab = SkillTreeTab.Race;

    public SkillTreeWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;
        _tree = controller.GetSkillTreeSystem();

        LoadData();
        RenderCurrentTab();
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

        BuildSkillSlotPanel();
    }

    #region Tab Management

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl tc)
        {
            _currentTab = tc.SelectedIndex switch
            {
                0 => SkillTreeTab.Race,
                1 => SkillTreeTab.Class,
                2 => SkillTreeTab.Background,
                3 => SkillTreeTab.Weapon,
                4 => SkillTreeTab.Magic,
                _ => SkillTreeTab.Race
            };
            _selectedNodeId = null;
            RenderCurrentTab();
        }
    }

    private Canvas GetCurrentCanvas()
    {
        return _currentTab switch
        {
            SkillTreeTab.Race => RaceTreeCanvas,
            SkillTreeTab.Class => ClassTreeCanvas,
            SkillTreeTab.Background => BackgroundTreeCanvas,
            SkillTreeTab.Weapon => WeaponTreeCanvas,
            SkillTreeTab.Magic => MagicTreeCanvas,
            _ => RaceTreeCanvas
        };
    }

    private void RenderCurrentTab()
    {
        var canvas = GetCurrentCanvas();
        canvas.Children.Clear();

        var nodesForTab = _tree.AllNodes.Values
            .Where(n => n.Tab == _currentTab)
            .ToList();

        // 1) 接続線を描画（前提ノード → 子ノード）
        foreach (var node in nodesForTab)
        {
            foreach (var prereqId in node.Prerequisites)
            {
                if (_tree.AllNodes.TryGetValue(prereqId, out var prereqNode) && prereqNode.Tab == _currentTab)
                {
                    DrawConnection(canvas, prereqNode, node);
                }
            }
        }

        // 2) ノードアイコンを描画
        foreach (var node in nodesForTab)
        {
            DrawNode(canvas, node);
        }

        ClearDetail();
    }

    #endregion

    #region Node Drawing

    private void DrawConnection(Canvas canvas, SkillNodeDefinition from, SkillNodeDefinition to)
    {
        double offsetX = NodeRadius + 20;
        double offsetY = NodeRadius + 20;

        bool fromUnlocked = _tree.UnlockedNodes.Contains(from.Id);
        bool toUnlocked = _tree.UnlockedNodes.Contains(to.Id);
        bool bothUnlocked = fromUnlocked && toUnlocked;

        var line = new Line
        {
            X1 = from.TreeX + offsetX,
            Y1 = from.TreeY + offsetY,
            X2 = to.TreeX + offsetX,
            Y2 = to.TreeY + offsetY,
            Stroke = bothUnlocked
                ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                : fromUnlocked
                    ? new SolidColorBrush(Color.FromRgb(0x3a, 0x6a, 0x3a))
                    : new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x3a)),
            StrokeThickness = bothUnlocked ? 3 : 2,
            IsHitTestVisible = false
        };
        canvas.Children.Add(line);
    }

    private void DrawNode(Canvas canvas, SkillNodeDefinition node)
    {
        double offsetX = NodeRadius + 20;
        double offsetY = NodeRadius + 20;

        int playerLevel = _controller.Player.Level;
        bool isUnlocked = _tree.UnlockedNodes.Contains(node.Id);
        bool canUnlock = _tree.CanUnlock(node.Id, playerLevel);
        bool isSelected = node.Id == _selectedNodeId;
        bool isKeystone = node.NodeType == SkillNodeType.Keystone;
        double radius = isKeystone ? KeystoneRadius : NodeRadius;

        // ノード形状
        Shape shape;
        if (isKeystone)
            shape = CreateDiamond(radius, node, isUnlocked, canUnlock, isSelected);
        else
            shape = CreateCircle(radius, node, isUnlocked, canUnlock, isSelected);

        Canvas.SetLeft(shape, node.TreeX + offsetX - radius);
        Canvas.SetTop(shape, node.TreeY + offsetY - radius);
        shape.Tag = node.Id;
        canvas.Children.Add(shape);

        // アイコン文字
        var icon = GetTypeIcon(node.NodeType);
        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = isKeystone ? 16 : 13,
            Foreground = isUnlocked ? Brushes.White : canUnlock ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d)) : new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x80)),
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false,
            TextAlignment = TextAlignment.Center
        };
        Canvas.SetLeft(iconText, node.TreeX + offsetX - 7);
        Canvas.SetTop(iconText, node.TreeY + offsetY - 9);
        canvas.Children.Add(iconText);

        // ノード名ラベル
        var nameLabel = new TextBlock
        {
            Text = node.Name,
            FontSize = 9,
            Foreground = isUnlocked ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                       : canUnlock ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d))
                       : new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x80)),
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 100,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Canvas.SetLeft(nameLabel, node.TreeX + offsetX - 50);
        Canvas.SetTop(nameLabel, node.TreeY + offsetY + radius + 3);
        canvas.Children.Add(nameLabel);

        // 解放済みチェックマーク
        if (isUnlocked)
        {
            var check = new TextBlock
            {
                Text = "✓",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(check, node.TreeX + offsetX + radius - 6);
            Canvas.SetTop(check, node.TreeY + offsetY - radius - 2);
            canvas.Children.Add(check);
        }
    }

    private static Ellipse CreateCircle(double radius, SkillNodeDefinition node, bool isUnlocked, bool canUnlock, bool isSelected)
    {
        var ellipse = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = GetNodeFillBrush(node, isUnlocked, canUnlock),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                : isUnlocked
                    ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                    : canUnlock
                        ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d))
                        : new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x50)),
            StrokeThickness = isSelected ? 3 : 2,
            Cursor = Cursors.Hand
        };
        return ellipse;
    }

    private static Polygon CreateDiamond(double radius, SkillNodeDefinition node, bool isUnlocked, bool canUnlock, bool isSelected)
    {
        var diamond = new Polygon
        {
            Points = new PointCollection
            {
                new Point(radius, 0),
                new Point(radius * 2, radius),
                new Point(radius, radius * 2),
                new Point(0, radius)
            },
            Fill = GetNodeFillBrush(node, isUnlocked, canUnlock),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                : isUnlocked
                    ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                    : canUnlock
                        ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d))
                        : new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x50)),
            StrokeThickness = isSelected ? 3 : 2,
            Cursor = Cursors.Hand
        };
        return diamond;
    }

    private static Brush GetNodeFillBrush(SkillNodeDefinition node, bool isUnlocked, bool canUnlock)
    {
        if (isUnlocked)
        {
            return node.NodeType switch
            {
                SkillNodeType.Keystone => new SolidColorBrush(Color.FromRgb(0x2a, 0x1a, 0x3a)),
                SkillNodeType.StatMajor => new SolidColorBrush(Color.FromRgb(0x1a, 0x3a, 0x3a)),
                _ => new SolidColorBrush(Color.FromRgb(0x1a, 0x3a, 0x2a))
            };
        }
        if (canUnlock)
        {
            return new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x1a));
        }
        return new SolidColorBrush(Color.FromRgb(0x14, 0x14, 0x20));
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

    #endregion

    #region Interaction

    private void TreeCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        var pos = e.GetPosition(canvas);

        double offsetX = NodeRadius + 20;
        double offsetY = NodeRadius + 20;

        var nodesForTab = _tree.AllNodes.Values.Where(n => n.Tab == _currentTab).ToList();

        SkillNodeDefinition? clickedNode = null;
        foreach (var node in nodesForTab)
        {
            double nx = node.TreeX + offsetX;
            double ny = node.TreeY + offsetY;
            double r = node.NodeType == SkillNodeType.Keystone ? KeystoneRadius : NodeRadius;
            double dist = Math.Sqrt(Math.Pow(pos.X - nx, 2) + Math.Pow(pos.Y - ny, 2));
            if (dist <= r + 5)
            {
                clickedNode = node;
                break;
            }
        }

        if (clickedNode != null)
        {
            _selectedNodeId = clickedNode.Id;
            int playerLevel = _controller.Player.Level;
            bool canUnlock = _tree.CanUnlock(clickedNode.Id, playerLevel);
            bool isUnlocked = _tree.UnlockedNodes.Contains(clickedNode.Id);

            UnlockButton.IsEnabled = canUnlock;
            UnlockButton.Content = isUnlocked ? "解放済み" : "解放 [Enter]";

            ShowNodeDetail(clickedNode);
            RenderCurrentTab();
        }
    }

    private void ShowNodeDetail(SkillNodeDefinition node)
    {
        var (nameText, typeText, descText, prereqText, bonusText, downsideText) = GetDetailTexts(node);

        switch (_currentTab)
        {
            case SkillTreeTab.Race:
                RaceNodeNameText.Text = nameText;
                RaceNodeTypeText.Text = typeText;
                RaceNodeDescText.Text = descText;
                RaceNodePrereqText.Text = prereqText;
                RaceNodeBonusText.Text = bonusText;
                break;
            case SkillTreeTab.Class:
                ClassNodeNameText.Text = nameText;
                ClassNodeTypeText.Text = typeText;
                ClassNodeDescText.Text = descText;
                ClassNodePrereqText.Text = prereqText;
                ClassNodeBonusText.Text = bonusText;
                ClassNodeDownsideText.Text = downsideText;
                break;
            case SkillTreeTab.Background:
                BgNodeNameText.Text = nameText;
                BgNodeTypeText.Text = typeText;
                BgNodeDescText.Text = descText;
                BgNodePrereqText.Text = prereqText;
                BgNodeBonusText.Text = bonusText;
                break;
            case SkillTreeTab.Weapon:
                WeaponNodeNameText.Text = nameText;
                WeaponNodeTypeText.Text = typeText;
                WeaponNodeDescText.Text = descText;
                WeaponNodePrereqText.Text = prereqText;
                WeaponNodeBonusText.Text = bonusText;
                break;
            case SkillTreeTab.Magic:
                MagicNodeNameText.Text = nameText;
                MagicNodeTypeText.Text = typeText;
                MagicNodeDescText.Text = descText;
                MagicNodePrereqText.Text = prereqText;
                MagicNodeBonusText.Text = bonusText;
                break;
        }
    }

    private (string Name, string Type, string Desc, string Prereq, string Bonus, string Downside) GetDetailTexts(SkillNodeDefinition node)
    {
        string typeName = node.NodeType switch
        {
            SkillNodeType.StatMinor => "ステータス（小）",
            SkillNodeType.StatMajor => "ステータス（大）",
            SkillNodeType.Passive => "パッシブ",
            SkillNodeType.Keystone => "キーストーン",
            SkillNodeType.Active => "アクティブ",
            _ => "不明"
        };

        string typeExtra = "";
        if (node.RequiredClass != null) typeExtra += $" | 職業: {node.RequiredClass}";
        if (node.RequiredRace != null) typeExtra += $" | 種族: {node.RequiredRace}";
        if (node.RequiredBackground != null) typeExtra += $" | 素性: {node.RequiredBackground}";
        if (typeExtra == "") typeExtra = " | 共通";

        string typeText = $"{typeName}{typeExtra} | Tier {node.Tier} | Lv{node.RequiredLevel} | {node.PointCost}pt";

        string prereqText = "";
        if (node.Prerequisites.Length > 0)
        {
            var prereqNames = node.Prerequisites
                .Select(id => _tree.AllNodes.TryGetValue(id, out var n) ? n.Name : id)
                .ToList();
            prereqText = $"前提: {string.Join(", ", prereqNames)}";
        }

        string bonusText = node.StatBonuses.Count > 0
            ? "ボーナス: " + string.Join(", ", node.StatBonuses.Select(b => $"{b.Key} +{b.Value}"))
            : "";

        string downsideText = node.KeystoneDownside != null
            ? $"⚠ デメリット: {node.KeystoneDownside}"
            : "";

        return (node.Name, typeText, node.Description, prereqText, bonusText, downsideText);
    }

    private void ClearDetail()
    {
        UnlockButton.IsEnabled = false;
        UnlockButton.Content = "解放 [Enter]";

        RaceNodeNameText.Text = "ノードをクリック";
        RaceNodeTypeText.Text = "";
        RaceNodeDescText.Text = "";
        RaceNodePrereqText.Text = "";
        RaceNodeBonusText.Text = "";

        ClassNodeNameText.Text = "ノードをクリック";
        ClassNodeTypeText.Text = "";
        ClassNodeDescText.Text = "";
        ClassNodePrereqText.Text = "";
        ClassNodeBonusText.Text = "";
        ClassNodeDownsideText.Text = "";

        BgNodeNameText.Text = "ノードをクリック";
        BgNodeTypeText.Text = "";
        BgNodeDescText.Text = "";
        BgNodePrereqText.Text = "";
        BgNodeBonusText.Text = "";

        WeaponNodeNameText.Text = "ノードをクリック";
        WeaponNodeTypeText.Text = "";
        WeaponNodeDescText.Text = "";
        WeaponNodePrereqText.Text = "";
        WeaponNodeBonusText.Text = "";

        MagicNodeNameText.Text = "ノードをクリック";
        MagicNodeTypeText.Text = "";
        MagicNodeDescText.Text = "";
        MagicNodePrereqText.Text = "";
        MagicNodeBonusText.Text = "";
    }

    #endregion

    #region Unlock / Respec

    private void UnlockButton_Click(object sender, RoutedEventArgs e)
    {
        TryUnlockSelected();
    }

    private void TryUnlockSelected()
    {
        if (_selectedNodeId == null) return;
        if (!_tree.AllNodes.TryGetValue(_selectedNodeId, out var node)) return;

        if (!_tree.CanUnlock(_selectedNodeId, _controller.Player.Level)) return;

        if (node.NodeType == SkillNodeType.Keystone)
        {
            var result = MessageBox.Show(
                $"キーストーン「{node.Name}」を解放しますか？\n\nデメリット: {node.KeystoneDownside}",
                "キーストーン確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }

        _tree.UnlockNode(_selectedNodeId, _controller.Player.Level);
        LoadData();
        RenderCurrentTab();

        if (_tree.AllNodes.TryGetValue(_selectedNodeId, out var updatedNode))
            ShowNodeDetail(updatedNode);
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
            _selectedNodeId = null;
            LoadData();
            RenderCurrentTab();
            MessageBox.Show($"{refunded}ポイントが返還されました。", "リスペック完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    #endregion

    #region Skill Slots

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
                    : new SolidColorBrush(Color.FromRgb(0x14, 0x14, 0x20)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 2, 4, 2),
                Margin = new Thickness(2, 0, 2, 0),
                MinWidth = 80,
                Cursor = Cursors.Hand
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock
            {
                Text = $"[{i + 1}] ",
                Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
                FontSize = 10
            });

            if (node != null)
            {
                sp.Children.Add(new TextBlock
                {
                    Text = node.Name,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                });
                var removeBtn = new Button
                {
                    Content = "×",
                    Width = 16, Height = 16,
                    Margin = new Thickness(4, 0, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(0x3a, 0x10, 0x20)),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60)),
                    BorderThickness = new Thickness(0),
                    FontSize = 9,
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
                    Text = "空",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80)),
                    FontSize = 10
                });
            }

            slotBorder.Child = sp;
            SkillSlotPanel.Children.Add(slotBorder);
        }

        if (SlotInfoText != null)
            SlotInfoText.Text = $"装備中: {_tree.EquippedSkillSlots.Count}/{SkillTreeSystem.MaxSkillSlots}";
    }

    private void RemoveSkillSlot_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int slotIdx)
        {
            _tree.UnequipSkillSlot(slotIdx);
            BuildSkillSlotPanel();
        }
    }

    #endregion

    #region Key / Close

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

    #endregion
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
