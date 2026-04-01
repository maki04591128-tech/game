using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// Path of Exile風グリッドインベントリ表示ウィンドウ
/// </summary>
public partial class InventoryWindow : Window
{
    private const int CellSize = 40;
    private const int GridWidth = 10;
    private const int GridHeight = 6;

    private List<Item> _items;
    private readonly Player _player;
    private readonly List<GridCellInfo> _gridCells = new();
    private int _selectedIndex = -1;

    // ドラッグアンドドロップ用フィールド
    private bool _isDragging;
    private GridCellInfo? _draggedCell;
    private Point _dragStartPos;
    private Rectangle? _dragGhost;
    private TextBlock? _dragGhostChar;
    private Rectangle? _dropPreview;
    private const double DragThreshold = 5.0;

    // 装備パネルからのドラッグ用
    private EquipmentSlot? _draggedEquipSlot;
    private bool _isDraggingFromEquip;

    // ドラッグ移動後の位置情報を保持（RenderGrid間で引き継ぐため）
    private readonly Dictionary<int, (int GridX, int GridY)> _savedPositions = new();

    // ソート状態維持フラグ
    private bool _isSorted;

    // コールバック: アイテム使用/装備
    private readonly Action<Item>? _onUseItem;
    // コールバック: アイテムを地面に落とす
    private readonly Action<Item>? _onDropItem;
    // コールバック: 装備外し
    private readonly Action<EquipmentSlot>? _onUnequipItem;
    // コールバック: アイテムリスト再取得
    private readonly Func<List<Item>>? _getItems;

    public int SelectedIndex { get; private set; } = -1;

    /// <summary>ソート状態を取得（ウィンドウ閉鎖時に外部で保存するため）</summary>
    public bool IsSorted => _isSorted;

    public InventoryWindow(List<Item> items, Player player,
        Action<Item>? onUseItem = null, Action<Item>? onDropItem = null, Func<List<Item>>? getItems = null,
        Dictionary<int, (int GridX, int GridY)>? savedPositions = null,
        Action<EquipmentSlot>? onUnequipItem = null,
        bool isSorted = false)
    {
        InitializeComponent();
        _items = items;
        _player = player;
        _onUseItem = onUseItem;
        _onDropItem = onDropItem;
        _onUnequipItem = onUnequipItem;
        _getItems = getItems;
        _isSorted = isSorted;
        if (savedPositions != null && savedPositions.Count > 0)
        {
            foreach (var kvp in savedPositions)
                _savedPositions[kvp.Key] = kvp.Value;
        }

        // ソート状態が復元された場合、初回レンダリング前にソートを適用
        if (_isSorted)
        {
            _items = _items.OrderBy(i => GetItemSortCategory(i))
                           .ThenByDescending(i => (int)i.Rarity)
                           .ThenBy(i => i.GetDisplayName())
                           .ToList();
        }

        RenderGrid();
        RenderEquipmentPanel();
        RenderStats();

        // 装備パネルのD&Dイベント
        EquipmentCanvas.MouseMove += EquipmentCanvas_MouseMove;
        EquipmentCanvas.MouseLeftButtonUp += EquipmentCanvas_MouseUp;
    }

    /// <summary>現在のグリッド位置情報を取得（ウィンドウ閉鎖時に外部で保存するため）</summary>
    public Dictionary<int, (int GridX, int GridY)> GetSavedPositions()
    {
        var positions = new Dictionary<int, (int GridX, int GridY)>(_savedPositions);
        foreach (var cell in _gridCells)
        {
            positions[cell.Index] = (cell.GridX, cell.GridY);
        }
        return positions;
    }

    private void RenderGrid()
    {
        // 既存の位置情報を保存してからクリア
        foreach (var cell in _gridCells)
        {
            _savedPositions[cell.Index] = (cell.GridX, cell.GridY);
        }

        GridCanvas.Children.Clear();
        _gridCells.Clear();

        DrawGridLines();
        PlaceItemsOnGrid();
        UpdateSpaceInfo();
    }

    private void DrawGridLines()
    {
        for (int x = 0; x <= GridWidth; x++)
        {
            var line = new Line
            {
                X1 = x * CellSize, Y1 = 0,
                X2 = x * CellSize, Y2 = GridHeight * CellSize,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);
        }

        for (int y = 0; y <= GridHeight; y++)
        {
            var line = new Line
            {
                X1 = 0, Y1 = y * CellSize,
                X2 = GridWidth * CellSize, Y2 = y * CellSize,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(line);
        }
    }

    private void PlaceItemsOnGrid()
    {
        // 保存済みの位置情報を使用（ドラッグ移動後の位置を維持するため）
        var existingPositions = new Dictionary<int, (int GridX, int GridY)>(_savedPositions);
        var newCells = new List<GridCellInfo>();
        var placed = new bool[GridWidth, GridHeight];
        int itemIndex = 0;

        foreach (var item in _items)
        {
            // 装備中のアイテムはグリッドに表示しない
            if (IsEquipped(item))
            {
                itemIndex++;
                continue;
            }

            var size = GetGridSize(item);
            var (w, h) = GridInventorySystem.GetDimensions(size);

            int gx, gy;
            if (existingPositions.TryGetValue(itemIndex, out var existingPos))
            {
                // 既存の位置を復元（衝突がなければ）
                gx = existingPos.GridX;
                gy = existingPos.GridY;
                bool fits = gx + w <= GridWidth && gy + h <= GridHeight;
                if (fits)
                {
                    for (int dx = 0; dx < w && fits; dx++)
                        for (int dy = 0; dy < h && fits; dy++)
                            if (placed[gx + dx, gy + dy])
                                fits = false;
                }
                if (!fits)
                {
                    var freePos = FindFreePosition(placed, w, h);
                    if (freePos == null) { itemIndex++; continue; }
                    (gx, gy) = freePos.Value;
                }
            }
            else
            {
                var freePos = FindFreePosition(placed, w, h);
                if (freePos == null)
                {
                    itemIndex++;
                    continue;
                }
                (gx, gy) = freePos.Value;
            }

            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    placed[gx + dx, gy + dy] = true;

            var cellInfo = new GridCellInfo
            {
                Index = itemIndex,
                Item = item,
                GridX = gx,
                GridY = gy,
                Width = w,
                Height = h
            };
            newCells.Add(cellInfo);

            AddItemVisual(cellInfo);
            itemIndex++;
        }

        _gridCells.AddRange(newCells);
    }

    private (int X, int Y)? FindFreePosition(bool[,] placed, int w, int h)
    {
        for (int y = 0; y <= GridHeight - h; y++)
        {
            for (int x = 0; x <= GridWidth - w; x++)
            {
                bool fits = true;
                for (int dx = 0; dx < w && fits; dx++)
                    for (int dy = 0; dy < h && fits; dy++)
                        if (placed[x + dx, y + dy])
                            fits = false;

                if (fits)
                    return (x, y);
            }
        }
        return null;
    }

    private void AddItemVisual(GridCellInfo cell)
    {
        var bgColor = GetRarityColor(cell.Item.Rarity);
        bool isEquipped = IsEquipped(cell.Item);

        var rect = new Rectangle
        {
            Width = cell.Width * CellSize - 2,
            Height = cell.Height * CellSize - 2,
            Fill = new SolidColorBrush(bgColor),
            Stroke = isEquipped
                ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                : new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80)),
            StrokeThickness = isEquipped ? 2 : 1,
            RadiusX = 3,
            RadiusY = 3,
            Tag = cell.Index
        };
        rect.MouseLeftButtonDown += ItemRect_Click;
        Canvas.SetLeft(rect, cell.GridX * CellSize + 1);
        Canvas.SetTop(rect, cell.GridY * CellSize + 1);
        GridCanvas.Children.Add(rect);

        var displayChar = cell.Item.DisplayChar.ToString();
        var charText = new TextBlock
        {
            Text = displayChar,
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(charText, cell.GridX * CellSize + 4);
        Canvas.SetTop(charText, cell.GridY * CellSize + 2);
        GridCanvas.Children.Add(charText);

        var nameText = cell.Item.GetDisplayName();
        if (nameText.Length > (cell.Width * 4))
            nameText = nameText[..(cell.Width * 4 - 1)] + "…";

        var label = new TextBlock
        {
            Text = nameText,
            Foreground = Brushes.White,
            FontSize = 10,
            IsHitTestVisible = false,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = cell.Width * CellSize - 6
        };
        Canvas.SetLeft(label, cell.GridX * CellSize + 4);
        Canvas.SetTop(label, cell.GridY * CellSize + cell.Height * CellSize - 18);
        GridCanvas.Children.Add(label);

        if (isEquipped)
        {
            var equippedLabel = new TextBlock
            {
                Text = "E",
                Foreground = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(equippedLabel, (cell.GridX + cell.Width) * CellSize - 16);
            Canvas.SetTop(equippedLabel, cell.GridY * CellSize + 2);
            GridCanvas.Children.Add(equippedLabel);
        }
    }

    private void ItemRect_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Rectangle rect && rect.Tag is int index)
        {
            SelectItemByIndex(index);
            // ドラッグ開始準備
            _draggedCell = _gridCells.FirstOrDefault(c => c.Index == index);
            _dragStartPos = e.GetPosition(GridCanvas);
            _isDragging = false;
            GridCanvas.CaptureMouse();
            e.Handled = true;
        }
    }

    private void GridCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // グリッド空白部分クリック時は選択解除
        _selectedIndex = -1;
        _draggedCell = null;
        UseButton.IsEnabled = false;
        ItemDetailText.Text = "アイテムをクリックして選択";
        HighlightSelected();
    }

    private void GridCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedCell == null || e.LeftButton != MouseButtonState.Pressed)
            return;

        var pos = e.GetPosition(GridCanvas);

        if (!_isDragging)
        {
            var diff = pos - _dragStartPos;
            if (Math.Abs(diff.X) < DragThreshold && Math.Abs(diff.Y) < DragThreshold)
                return;
            _isDragging = true;
            CreateDragGhost(_draggedCell, pos);
        }

        if (_isDragging)
        {
            UpdateDragGhost(pos);
            UpdateDropPreview(pos);
        }
    }

    private void GridCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        GridCanvas.ReleaseMouseCapture();

        if (_isDragging && _draggedCell != null)
        {
            var pos = e.GetPosition(GridCanvas);
            var windowPos = e.GetPosition(this);

            // 装備パネルへのドロップ判定
            if (_draggedCell.Item is EquipmentItem && IsOverEquipmentPanel(windowPos))
            {
                if (_onUseItem != null)
                {
                    _onUseItem(_draggedCell.Item);
                    RefreshItems();
                }
            }
            // グリッド外にドロップした場合はアイテムを地面に落とす
            else if (pos.X < 0 || pos.Y < 0 || pos.X > GridWidth * CellSize || pos.Y > GridHeight * CellSize)
            {
                if (_onDropItem != null)
                {
                    _onDropItem(_draggedCell.Item);
                    RefreshItems();
                }
            }
            else
            {
                TryDropItem(pos);
            }
        }

        CleanupDrag();
    }

    private void CreateDragGhost(GridCellInfo cell, Point pos)
    {
        var bgColor = GetRarityColor(cell.Item.Rarity);
        _dragGhost = new Rectangle
        {
            Width = cell.Width * CellSize - 2,
            Height = cell.Height * CellSize - 2,
            Fill = new SolidColorBrush(bgColor) { Opacity = 0.7 },
            Stroke = new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d)),
            StrokeThickness = 2,
            RadiusX = 3,
            RadiusY = 3,
            IsHitTestVisible = false,
            Opacity = 0.8
        };
        Panel.SetZIndex(_dragGhost, 100);
        DragOverlayCanvas.Children.Add(_dragGhost);

        _dragGhostChar = new TextBlock
        {
            Text = cell.Item.DisplayChar.ToString(),
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false
        };
        Panel.SetZIndex(_dragGhostChar, 101);
        DragOverlayCanvas.Children.Add(_dragGhostChar);

        _dropPreview = new Rectangle
        {
            Width = cell.Width * CellSize,
            Height = cell.Height * CellSize,
            Fill = Brushes.Transparent,
            Stroke = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
            StrokeDashArray = new DoubleCollection { 4, 2 },
            StrokeThickness = 2,
            IsHitTestVisible = false
        };
        Panel.SetZIndex(_dropPreview, 99);
        GridCanvas.Children.Add(_dropPreview);
    }

    private void UpdateDragGhost(Point gridPos)
    {
        if (_dragGhost == null || _dragGhostChar == null)
            return;

        // GridCanvas 座標をオーバーレイ座標に変換
        var overlayPos = GridCanvas.TranslatePoint(gridPos, DragOverlayCanvas);

        double ghostW, ghostH;
        if (_draggedCell != null)
        {
            ghostW = _draggedCell.Width * CellSize;
            ghostH = _draggedCell.Height * CellSize;
        }
        else
        {
            ghostW = _dragGhost.Width;
            ghostH = _dragGhost.Height;
        }

        double left = overlayPos.X - (ghostW / 2.0);
        double top = overlayPos.Y - (ghostH / 2.0);
        Canvas.SetLeft(_dragGhost, left);
        Canvas.SetTop(_dragGhost, top);
        Canvas.SetLeft(_dragGhostChar, left + 4);
        Canvas.SetTop(_dragGhostChar, top + 2);
    }

    private void UpdateDropPreview(Point pos)
    {
        if (_dropPreview == null || _draggedCell == null)
            return;

        var (gx, gy) = GetGridPositionFromMouse(pos, _draggedCell.Width, _draggedCell.Height);
        Canvas.SetLeft(_dropPreview, gx * CellSize);
        Canvas.SetTop(_dropPreview, gy * CellSize);

        bool canPlace = CanPlaceAt(gx, gy, _draggedCell.Width, _draggedCell.Height, _draggedCell.Index);
        _dropPreview.Stroke = canPlace
            ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
            : new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60));
    }

    private void TryDropItem(Point pos)
    {
        if (_draggedCell == null) return;

        var (gx, gy) = GetGridPositionFromMouse(pos, _draggedCell.Width, _draggedCell.Height);

        if (CanPlaceAt(gx, gy, _draggedCell.Width, _draggedCell.Height, _draggedCell.Index))
        {
            _draggedCell.GridX = gx;
            _draggedCell.GridY = gy;
            RenderGrid();
            SelectItemByIndex(_draggedCell.Index);
        }
    }

    private (int X, int Y) GetGridPositionFromMouse(Point pos, int itemW, int itemH)
    {
        int gx = (int)Math.Round((pos.X - itemW * CellSize / 2.0) / CellSize);
        int gy = (int)Math.Round((pos.Y - itemH * CellSize / 2.0) / CellSize);
        gx = Math.Clamp(gx, 0, GridWidth - itemW);
        gy = Math.Clamp(gy, 0, GridHeight - itemH);
        return (gx, gy);
    }

    private bool CanPlaceAt(int gx, int gy, int w, int h, int excludeIndex)
    {
        if (gx < 0 || gy < 0 || gx + w > GridWidth || gy + h > GridHeight)
            return false;

        foreach (var other in _gridCells)
        {
            if (other.Index == excludeIndex) continue;

            // 矩形重複判定
            if (gx < other.GridX + other.Width && gx + w > other.GridX &&
                gy < other.GridY + other.Height && gy + h > other.GridY)
                return false;
        }
        return true;
    }

    private void CleanupDrag()
    {
        if (_dragGhost != null)
        {
            DragOverlayCanvas.Children.Remove(_dragGhost);
            _dragGhost = null;
        }
        if (_dragGhostChar != null)
        {
            DragOverlayCanvas.Children.Remove(_dragGhostChar);
            _dragGhostChar = null;
        }
        if (_dropPreview != null)
        {
            GridCanvas.Children.Remove(_dropPreview);
            _dropPreview = null;
        }
        _isDragging = false;
        _draggedCell = null;
    }

    private void SelectItemByIndex(int index)
    {
        _selectedIndex = index;
        UseButton.IsEnabled = true;
        HighlightSelected();

        if (index >= 0 && index < _items.Count)
        {
            var item = _items[index];
            var equipped = IsEquipped(item) ? " [装備中]" : "";
            var size = GetGridSize(item);
            var (w, h) = GridInventorySystem.GetDimensions(size);
            ItemDetailText.Text = $"{item.GetDisplayName()}{equipped}\n" +
                                  $"種類: {GetItemTypeText(item)} | サイズ: {w}x{h} | " +
                                  $"重さ: {item.Weight:F1} | 価値: {item.CalculatePrice()}G";
        }
    }

    private void HighlightSelected()
    {
        foreach (var child in GridCanvas.Children.OfType<Rectangle>())
        {
            if (child.Tag is int idx)
            {
                var cell = _gridCells.FirstOrDefault(c => c.Index == idx);
                if (cell == null) continue;

                bool isEquipped = IsEquipped(cell.Item);

                if (idx == _selectedIndex)
                {
                    child.Stroke = new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60));
                    child.StrokeThickness = 2;
                }
                else
                {
                    child.Stroke = isEquipped
                        ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                        : new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80));
                    child.StrokeThickness = isEquipped ? 2 : 1;
                }
            }
        }
    }

    private bool IsEquipped(Item item)
    {
        if (item is not EquipmentItem) return false;
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            if (slot != EquipmentSlot.None && _player.Equipment[slot] == item)
                return true;
        }
        return false;
    }

    private static GridItemSize GetGridSize(Item item)
    {
        return item switch
        {
            Weapon { IsTwoHanded: true } => GridItemSize.Size2x3,
            Weapon => GridItemSize.Size1x2,
            Shield => GridItemSize.Size2x2,
            Armor => GridItemSize.Size2x3,
            EquipmentItem => GridItemSize.Size1x2,
            _ => GridItemSize.Size1x1
        };
    }

    private static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.FromRgb(0x2a, 0x2a, 0x4a),
            ItemRarity.Uncommon => Color.FromRgb(0x1a, 0x3a, 0x1a),
            ItemRarity.Rare => Color.FromRgb(0x1a, 0x1a, 0x4a),
            ItemRarity.Epic => Color.FromRgb(0x3a, 0x1a, 0x3a),
            ItemRarity.Legendary => Color.FromRgb(0x4a, 0x3a, 0x1a),
            ItemRarity.Unique => Color.FromRgb(0x4a, 0x2a, 0x1a),
            _ => Color.FromRgb(0x2a, 0x2a, 0x4a)
        };
    }

    private static string GetItemTypeText(Item item)
    {
        return item switch
        {
            Weapon w => $"武器({w.WeaponType})",
            Shield => "盾",
            Armor => "防具",
            EquipmentItem => "装備",
            _ => item.Type.ToString()
        };
    }

    private void UpdateSpaceInfo()
    {
        int usedCells = _gridCells.Sum(c => c.Width * c.Height);
        int totalCells = GridWidth * GridHeight;
        int freeCells = totalCells - usedCells;
        SpaceInfoText.Text = $"空き: {freeCells}/{totalCells} セル ({_items.Count} アイテム)";
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                UseSelectedItem();
                break;
            case Key.I:
            case Key.Escape:
                DialogResult = false;
                Close();
                break;
            case Key.S:
                SortItems();
                break;
            case Key.D1: SelectItemByIndex(0); break;
            case Key.D2: SelectItemByIndex(1); break;
            case Key.D3: SelectItemByIndex(2); break;
            case Key.D4: SelectItemByIndex(3); break;
            case Key.D5: SelectItemByIndex(4); break;
            case Key.D6: SelectItemByIndex(5); break;
            case Key.D7: SelectItemByIndex(6); break;
            case Key.D8: SelectItemByIndex(7); break;
            case Key.D9: SelectItemByIndex(8); break;
        }
    }

    private void UseSelectedItem()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            if (_onUseItem != null)
            {
                var item = _items[_selectedIndex];
                _onUseItem(item);
                RefreshItems();
            }
            else
            {
                SelectedIndex = _selectedIndex;
                DialogResult = true;
                Close();
            }
        }
    }

    private void RefreshItems()
    {
        if (_getItems != null)
        {
            _items = _getItems();
        }
        if (_isSorted)
        {
            _items = _items.OrderBy(i => GetItemSortCategory(i))
                           .ThenByDescending(i => (int)i.Rarity)
                           .ThenBy(i => i.GetDisplayName())
                           .ToList();
        }
        _selectedIndex = -1;
        UseButton.IsEnabled = false;
        ItemDetailText.Text = "アイテムをクリックして選択";
        _savedPositions.Clear();
        foreach (var cell in _gridCells)
        {
            _savedPositions[cell.Index] = (cell.GridX, cell.GridY);
        }
        _gridCells.Clear();
        RenderGrid();
        RenderEquipmentPanel();
        RenderStats();
    }

    private void SortItems()
    {
        _isSorted = true;
        _items = _items.OrderBy(i => GetItemSortCategory(i))
                       .ThenByDescending(i => (int)i.Rarity)
                       .ThenBy(i => i.GetDisplayName())
                       .ToList();
        _savedPositions.Clear();
        _gridCells.Clear();
        _selectedIndex = -1;
        UseButton.IsEnabled = false;
        ItemDetailText.Text = "アイテムをソートしました";
        RenderGrid();
    }

    private static int GetItemSortCategory(Item item) => item switch
    {
        Weapon => 0,
        Shield => 1,
        Armor => 2,
        EquipmentItem => 3,
        ConsumableItem => 4,
        _ => 5
    };

    private void SortButton_Click(object sender, RoutedEventArgs e)
    {
        SortItems();
    }

    private void UseButton_Click(object sender, RoutedEventArgs e)
    {
        UseSelectedItem();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void RenderEquipmentPanel()
    {
        EquipmentCanvas.Children.Clear();
        var equipment = _player.Equipment;

        // PoE風スロット配置: (スロット, ラベル, X, Y, 幅, 高さ)
        // Canvas = 244 x 340。スロットサイズは概ね 54x54（小型）/ 54x72（縦長）/ 72x54（横長）
        const int S = 54;  // 小スロット
        const int L = 72;  // 長スロット
        var slotLayout = new (EquipmentSlot Slot, string Label, int X, int Y, int W, int H)[]
        {
            //                         X      Y     W   H
            (EquipmentSlot.Head,    "頭",    95,    2,  S,  S),   // 上段中央
            (EquipmentSlot.Neck,    "首",    28,   12,  S,  S),   // 左上
            (EquipmentSlot.Back,    "背",   162,   12,  S,  S),   // 右上
            (EquipmentSlot.MainHand,"右手",   6,   72,  S,  L),   // 左（武器 = 縦長）
            (EquipmentSlot.Body,    "胴",    72,   62,  L+28, L+28),// 中央（胴体 = 大）
            (EquipmentSlot.OffHand, "左手", 184,   72,  S,  L),   // 右（盾/サブ = 縦長）
            (EquipmentSlot.Hands,   "手",    14,  152,  S,  S),   // 左中
            (EquipmentSlot.Ring1,   "指1",  176,  152,  S,  S),   // 右中
            (EquipmentSlot.Waist,   "腰",    95,  168,  S,  S),   // 中段下
            (EquipmentSlot.Ring2,   "指2",   28,  230,  S,  S),   // 左下
            (EquipmentSlot.Feet,    "足",    95,  232,  S,  L),   // 下段中央（靴 = 縦長）
        };

        // 人体ガイドライン（装飾）
        DrawBodySilhouette();

        foreach (var (slot, label, x, y, w, h) in slotLayout)
        {
            var item = equipment[slot];

            // スロット背景
            Color bgColor;
            Color borderColor;
            if (item != null)
            {
                bgColor = GetRarityColor(item.Rarity);
                borderColor = Color.FromRgb(0x4e, 0xcc, 0xa3);
            }
            else
            {
                bgColor = Color.FromRgb(0x12, 0x16, 0x28);
                borderColor = Color.FromRgb(0x2a, 0x3a, 0x5a);
            }

            var slotRect = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = new SolidColorBrush(bgColor),
                Stroke = new SolidColorBrush(borderColor),
                StrokeThickness = item != null ? 2 : 1,
                RadiusX = 4,
                RadiusY = 4,
                Tag = slot
            };
            slotRect.MouseLeftButtonDown += EquipSlot_Click;
            slotRect.MouseRightButtonDown += EquipSlot_RightClick;
            Canvas.SetLeft(slotRect, x);
            Canvas.SetTop(slotRect, y);
            EquipmentCanvas.Children.Add(slotRect);

            // ラベル（左上）
            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x90)),
                FontSize = 9,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(labelText, x + 3);
            Canvas.SetTop(labelText, y + 2);
            EquipmentCanvas.Children.Add(labelText);

            if (item != null)
            {
                // アイテム文字
                var charText = new TextBlock
                {
                    Text = item.DisplayChar.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(charText, x + w / 2 - 6);
                Canvas.SetTop(charText, y + h / 2 - 12);
                EquipmentCanvas.Children.Add(charText);

                // アイテム名（下部）
                string displayName = item.GetDisplayName();
                int maxChars = w / 8;
                if (displayName.Length > maxChars)
                    displayName = displayName[..(maxChars - 1)] + "…";
                var nameText = new TextBlock
                {
                    Text = displayName,
                    Foreground = Brushes.White,
                    FontSize = 8,
                    IsHitTestVisible = false,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = w - 4
                };
                Canvas.SetLeft(nameText, x + 2);
                Canvas.SetTop(nameText, y + h - 13);
                EquipmentCanvas.Children.Add(nameText);
            }
            else
            {
                // 空スロット表示
                var emptyText = new TextBlock
                {
                    Text = "─",
                    Foreground = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x5a)),
                    FontSize = 14,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(emptyText, x + w / 2 - 6);
                Canvas.SetTop(emptyText, y + h / 2 - 10);
                EquipmentCanvas.Children.Add(emptyText);
            }
        }
    }

    /// <summary>人体シルエットの装飾ライン描画</summary>
    private void DrawBodySilhouette()
    {
        var guideColor = new SolidColorBrush(Color.FromRgb(0x18, 0x20, 0x38));
        // 頭→胴
        EquipmentCanvas.Children.Add(new Line { X1 = 122, Y1 = 56, X2 = 122, Y2 = 62, Stroke = guideColor, StrokeThickness = 1, IsHitTestVisible = false });
        // 胴→腰
        EquipmentCanvas.Children.Add(new Line { X1 = 122, Y1 = 162, X2 = 122, Y2 = 168, Stroke = guideColor, StrokeThickness = 1, IsHitTestVisible = false });
        // 腰→足
        EquipmentCanvas.Children.Add(new Line { X1 = 122, Y1 = 222, X2 = 122, Y2 = 232, Stroke = guideColor, StrokeThickness = 1, IsHitTestVisible = false });
        // 胴→左手
        EquipmentCanvas.Children.Add(new Line { X1 = 72, Y1 = 108, X2 = 60, Y2 = 108, Stroke = guideColor, StrokeThickness = 1, IsHitTestVisible = false });
        // 胴→右手
        EquipmentCanvas.Children.Add(new Line { X1 = 172, Y1 = 108, X2 = 184, Y2 = 108, Stroke = guideColor, StrokeThickness = 1, IsHitTestVisible = false });
    }

    /// <summary>装備スロットクリック時の処理（ドラッグ開始を兼ねる）</summary>
    private void EquipSlot_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Rectangle rect && rect.Tag is EquipmentSlot slot)
        {
            var item = _player.Equipment[slot];
            if (item != null)
            {
                string curseWarning = item.IsCursed ? "\n⚠ 呪われていて外せない" : "";
                ItemDetailText.Text = $"[装備中] {item.GetDisplayName()}\n" +
                                      $"種類: {GetItemTypeText(item)} | " +
                                      $"重さ: {item.Weight:F1} | 価値: {item.CalculatePrice()}G" +
                                      curseWarning +
                                      (item.IsCursed ? "" : "\n右クリックまたはグリッドへドラッグで装備を外す");
                _selectedEquipSlot = slot;

                // 装備パネルからのドラッグ開始準備
                if (!item.IsCursed)
                {
                    _draggedEquipSlot = slot;
                    _isDraggingFromEquip = false;
                    _dragStartPos = e.GetPosition(this);
                    EquipmentCanvas.CaptureMouse();
                }
            }
            e.Handled = true;
        }
    }

    private EquipmentSlot? _selectedEquipSlot;

    /// <summary>装備パネル上のマウス移動でドラッグを検出</summary>
    private void EquipmentCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedEquipSlot == null || e.LeftButton != MouseButtonState.Pressed)
            return;

        var pos = e.GetPosition(this);
        if (!_isDraggingFromEquip)
        {
            var diff = pos - _dragStartPos;
            if (Math.Abs(diff.X) < DragThreshold && Math.Abs(diff.Y) < DragThreshold)
                return;
            _isDraggingFromEquip = true;

            var item = _player.Equipment[_draggedEquipSlot.Value];
            if (item != null)
            {
                CreateEquipDragGhost(item, e.GetPosition(GridCanvas));
            }
        }

        if (_isDraggingFromEquip)
        {
            UpdateDragGhost(e.GetPosition(GridCanvas));
        }
    }

    /// <summary>装備パネルからのマウスリリースで装備外し実行</summary>
    private void EquipmentCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        EquipmentCanvas.ReleaseMouseCapture();

        if (_isDraggingFromEquip && _draggedEquipSlot != null)
        {
            var windowPos = e.GetPosition(this);
            // グリッド領域にドロップした場合は装備外し
            if (IsOverGridPanel(windowPos))
            {
                if (_onUnequipItem != null)
                {
                    _onUnequipItem(_draggedEquipSlot.Value);
                    RefreshItems();
                }
            }
        }

        _draggedEquipSlot = null;
        _isDraggingFromEquip = false;
        CleanupDrag();
    }

    /// <summary>マウス座標が装備パネルの領域内かどうか判定</summary>
    private bool IsOverEquipmentPanel(Point windowPos)
    {
        var equipTopLeft = EquipmentCanvas.TranslatePoint(new Point(0, 0), this);
        return windowPos.X >= equipTopLeft.X && windowPos.X <= equipTopLeft.X + EquipmentCanvas.Width
            && windowPos.Y >= equipTopLeft.Y && windowPos.Y <= equipTopLeft.Y + EquipmentCanvas.Height;
    }

    /// <summary>マウス座標がグリッドパネルの領域内かどうか判定</summary>
    private bool IsOverGridPanel(Point windowPos)
    {
        var gridTopLeft = GridCanvas.TranslatePoint(new Point(0, 0), this);
        return windowPos.X >= gridTopLeft.X && windowPos.X <= gridTopLeft.X + GridCanvas.Width
            && windowPos.Y >= gridTopLeft.Y && windowPos.Y <= gridTopLeft.Y + GridCanvas.Height;
    }

    /// <summary>装備パネルからのドラッグ用ゴースト作成</summary>
    private void CreateEquipDragGhost(EquipmentItem item, Point gridPos)
    {
        var bgColor = GetRarityColor(item.Rarity);
        var size = GetGridSize(item);
        var (w, h) = GridInventorySystem.GetDimensions(size);

        var overlayPos = GridCanvas.TranslatePoint(gridPos, DragOverlayCanvas);

        _dragGhost = new Rectangle
        {
            Width = w * CellSize,
            Height = h * CellSize,
            Fill = new SolidColorBrush(bgColor) { Opacity = 0.6 },
            Stroke = new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
            StrokeThickness = 2,
            RadiusX = 4,
            RadiusY = 4,
            IsHitTestVisible = false,
            Opacity = 0.8
        };
        Canvas.SetLeft(_dragGhost, overlayPos.X - w * CellSize / 2.0);
        Canvas.SetTop(_dragGhost, overlayPos.Y - h * CellSize / 2.0);
        DragOverlayCanvas.Children.Add(_dragGhost);

        _dragGhostChar = new TextBlock
        {
            Text = item.DisplayChar.ToString(),
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false,
            Opacity = 0.9
        };
        Canvas.SetLeft(_dragGhostChar, overlayPos.X - 6);
        Canvas.SetTop(_dragGhostChar, overlayPos.Y - 10);
        DragOverlayCanvas.Children.Add(_dragGhostChar);
    }

    /// <summary>装備スロット右クリック時の装備外し処理</summary>
    private void EquipSlot_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is Rectangle rect && rect.Tag is EquipmentSlot slot)
        {
            var item = _player.Equipment[slot];
            if (item != null && !item.IsCursed && _onUnequipItem != null)
            {
                _onUnequipItem(slot);
                RefreshItems();
            }
            e.Handled = true;
        }
    }

    private void RenderStats()
    {
        var stats = _player.EffectiveStats;
        var eq = _player.Equipment;
        int physDef = eq.GetTotalPhysicalDefense();
        int magDef = eq.GetTotalMagicDefense();

        StatsText.Text =
            $"── ステータス ──\n" +
            $"HP: {_player.CurrentHp}/{stats.MaxHp}  MP: {_player.CurrentMp}/{stats.MaxMp}\n" +
            $"STR: {stats.Strength}  VIT: {stats.Vitality}  AGI: {stats.Agility}\n" +
            $"DEX: {stats.Dexterity}  INT: {stats.Intelligence}  MND: {stats.Mind}\n" +
            $"PER: {stats.Perception}  CHA: {stats.Charisma}  LUK: {stats.Luck}\n" +
            $"物攻: {stats.PhysicalAttack}  物防: {physDef}\n" +
            $"魔攻: {stats.MagicalAttack}  魔防: {magDef}\n" +
            $"装備重量: {eq.GetTotalWeight():F1}";
    }
}

public class ItemDisplayModel
{
    public int Index { get; set; }
    public string DisplayName { get; set; } = "";
    public string EquippedText { get; set; } = "";
    public Item Item { get; set; } = null!;
}

public class GridCellInfo
{
    public int Index { get; set; }
    public Item Item { get; set; } = null!;
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
