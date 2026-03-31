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
/// ショップウィンドウ — PoE風グリッド表示ベースの購入/売却UI
/// ショップ側・プレイヤー側の双方をグリッドで表示する。
/// </summary>
public partial class ShopWindow : Window
{
    private const int CellSize = 40;
    private const int ShopGridWidth = 10;
    private const int ShopGridHeight = 9;
    private const int PlayerGridWidth = 10;
    private const int PlayerGridHeight = 9;

    private readonly GameController _controller;
    private readonly FacilityType _shopType;

    // ショップ側グリッド管理
    private readonly List<ShopGridCellInfo> _shopCells = new();
    private int _selectedShopIndex = -1;

    // プレイヤー側グリッド管理
    private readonly List<PlayerGridCellInfo> _playerCells = new();
    private int _selectedPlayerIndex = -1;

    public ShopWindow(GameController controller, FacilityType shopType)
    {
        InitializeComponent();
        _controller = controller;
        _shopType = shopType;

        ShopTitle.Text = string.Format("【{0}】", GetShopName(shopType));
        ShopGridCanvas.Width = ShopGridWidth * CellSize;
        ShopGridCanvas.Height = ShopGridHeight * CellSize;
        PlayerGridCanvas.Width = PlayerGridWidth * CellSize;
        PlayerGridCanvas.Height = PlayerGridHeight * CellSize;

        UpdateGold();
        RenderBothGrids();
    }

    private void RenderBothGrids()
    {
        RenderShopGrid();
        RenderPlayerGrid();
    }

    #region Shop Grid Rendering

    private void RenderShopGrid()
    {
        ShopGridCanvas.Children.Clear();
        _shopCells.Clear();
        _selectedShopIndex = -1;

        DrawGridLines(ShopGridCanvas, ShopGridWidth, ShopGridHeight);
        PlaceShopItemsOnGrid();

        int count = _shopCells.Count;
        ShopGridLabel.Text = string.Format("ショップ商品 ({0})", count);
    }

    private void PlaceShopItemsOnGrid()
    {
        var items = _controller.InitializeAndGetShopItems(_shopType);
        var placed = new bool[ShopGridWidth, ShopGridHeight];
        int shopItemIndex = 0;

        foreach (var shopItem in items)
        {
            if (shopItem.Stock <= 0) { shopItemIndex++; continue; }

            var (w, h) = GridInventorySystem.GetDimensions(shopItem.GridSize);
            var freePos = FindFreePosition(placed, w, h, ShopGridWidth, ShopGridHeight);
            if (freePos == null) { shopItemIndex++; continue; }

            var (gx, gy) = freePos.Value;
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    placed[gx + dx, gy + dy] = true;

            var cellInfo = new ShopGridCellInfo
            {
                Index = shopItemIndex,
                ShopItem = shopItem,
                GridX = gx,
                GridY = gy,
                Width = w,
                Height = h
            };
            _shopCells.Add(cellInfo);

            AddShopItemVisual(cellInfo);
            shopItemIndex++;
        }
    }

    private void AddShopItemVisual(ShopGridCellInfo cell)
    {
        bool isSelected = cell.Index == _selectedShopIndex;

        var bgColor = GetShopItemColor(cell.ShopItem);
        var rect = new Rectangle
        {
            Width = cell.Width * CellSize - 2,
            Height = cell.Height * CellSize - 2,
            Fill = new SolidColorBrush(bgColor),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                : new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80)),
            StrokeThickness = isSelected ? 2 : 1,
            RadiusX = 3,
            RadiusY = 3,
            Tag = cell.Index
        };
        Canvas.SetLeft(rect, cell.GridX * CellSize + 1);
        Canvas.SetTop(rect, cell.GridY * CellSize + 1);
        ShopGridCanvas.Children.Add(rect);

        // アイテム名
        var nameText = cell.ShopItem.Name;
        if (nameText.Length > cell.Width * 4)
            nameText = nameText[..(cell.Width * 4 - 1)] + "…";

        var label = new TextBlock
        {
            Text = nameText,
            Foreground = Brushes.White,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = cell.Width * CellSize - 6
        };
        Canvas.SetLeft(label, cell.GridX * CellSize + 4);
        Canvas.SetTop(label, cell.GridY * CellSize + 3);
        ShopGridCanvas.Children.Add(label);

        // 価格表示
        var priceLabel = new TextBlock
        {
            Text = string.Format("{0}G", cell.ShopItem.BasePrice),
            Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xd7, 0x00)),
            FontSize = 10,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(priceLabel, cell.GridX * CellSize + 4);
        Canvas.SetTop(priceLabel, cell.GridY * CellSize + cell.Height * CellSize - 18);
        ShopGridCanvas.Children.Add(priceLabel);

        // 在庫表示
        var stockLabel = new TextBlock
        {
            Text = string.Format("x{0}", cell.ShopItem.Stock),
            Foreground = new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0)),
            FontSize = 9,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(stockLabel, cell.GridX * CellSize + cell.Width * CellSize - 28);
        Canvas.SetTop(stockLabel, cell.GridY * CellSize + cell.Height * CellSize - 16);
        ShopGridCanvas.Children.Add(stockLabel);
    }

    #endregion

    #region Player Grid Rendering

    private void RenderPlayerGrid()
    {
        PlayerGridCanvas.Children.Clear();
        _playerCells.Clear();
        _selectedPlayerIndex = -1;

        DrawGridLines(PlayerGridCanvas, PlayerGridWidth, PlayerGridHeight);
        PlacePlayerItemsOnGrid();

        int count = _playerCells.Count;
        PlayerGridLabel.Text = string.Format("所持品 ({0} アイテム)", count);
    }

    private void PlacePlayerItemsOnGrid()
    {
        var inventory = (Inventory)_controller.Player.Inventory;
        var placed = new bool[PlayerGridWidth, PlayerGridHeight];
        int itemIndex = 0;

        foreach (var item in inventory.Items)
        {
            var size = GetGridSize(item);
            var (w, h) = GridInventorySystem.GetDimensions(size);
            var freePos = FindFreePosition(placed, w, h, PlayerGridWidth, PlayerGridHeight);
            if (freePos == null) { itemIndex++; continue; }

            var (gx, gy) = freePos.Value;
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    placed[gx + dx, gy + dy] = true;

            var cellInfo = new PlayerGridCellInfo
            {
                Index = itemIndex,
                Item = item,
                GridX = gx,
                GridY = gy,
                Width = w,
                Height = h
            };
            _playerCells.Add(cellInfo);

            AddPlayerItemVisual(cellInfo);
            itemIndex++;
        }
    }

    private void AddPlayerItemVisual(PlayerGridCellInfo cell)
    {
        bool isSelected = cell.Index == _selectedPlayerIndex;
        var bgColor = GetRarityColor(cell.Item.Rarity);

        var rect = new Rectangle
        {
            Width = cell.Width * CellSize - 2,
            Height = cell.Height * CellSize - 2,
            Fill = new SolidColorBrush(bgColor),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                : new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x80)),
            StrokeThickness = isSelected ? 2 : 1,
            RadiusX = 3,
            RadiusY = 3,
            Tag = cell.Index
        };
        Canvas.SetLeft(rect, cell.GridX * CellSize + 1);
        Canvas.SetTop(rect, cell.GridY * CellSize + 1);
        PlayerGridCanvas.Children.Add(rect);

        // アイテム表示文字
        var displayChar = cell.Item.DisplayChar.ToString();
        var charText = new TextBlock
        {
            Text = displayChar,
            Foreground = Brushes.White,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(charText, cell.GridX * CellSize + 4);
        Canvas.SetTop(charText, cell.GridY * CellSize + 2);
        PlayerGridCanvas.Children.Add(charText);

        // アイテム名
        var nameText = cell.Item.GetDisplayName();
        if (nameText.Length > cell.Width * 4)
            nameText = nameText[..(cell.Width * 4 - 1)] + "…";

        var label = new TextBlock
        {
            Text = nameText,
            Foreground = new SolidColorBrush(GetRarityTextColor(cell.Item.Rarity)),
            FontSize = 10,
            IsHitTestVisible = false,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = cell.Width * CellSize - 6
        };
        Canvas.SetLeft(label, cell.GridX * CellSize + 4);
        Canvas.SetTop(label, cell.GridY * CellSize + cell.Height * CellSize - 18);
        PlayerGridCanvas.Children.Add(label);

        // 売値表示
        int sellPrice = Math.Max(1, cell.Item.BasePrice / 2);
        var priceLabel = new TextBlock
        {
            Text = string.Format("売:{0}G", sellPrice),
            Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xd7, 0x00)),
            FontSize = 9,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(priceLabel, cell.GridX * CellSize + cell.Width * CellSize - 48);
        Canvas.SetTop(priceLabel, cell.GridY * CellSize + 2);
        PlayerGridCanvas.Children.Add(priceLabel);
    }

    #endregion

    #region Grid Interaction

    private void ShopGridCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(ShopGridCanvas);
        int gx = (int)(pos.X / CellSize);
        int gy = (int)(pos.Y / CellSize);

        var clickedCell = _shopCells.FirstOrDefault(c =>
            gx >= c.GridX && gx < c.GridX + c.Width &&
            gy >= c.GridY && gy < c.GridY + c.Height);

        if (clickedCell != null)
        {
            _selectedShopIndex = clickedCell.Index;
            _selectedPlayerIndex = -1;
            BuyButton.IsEnabled = true;
            SellButton.IsEnabled = false;

            bool canAfford = _controller.Player.Gold >= clickedCell.ShopItem.BasePrice;
            var sizeText = FormatGridSize(clickedCell.ShopItem.GridSize);
            ItemInfoText.Text = string.Format("{0} — {1}G  在庫:{2}  サイズ:{3}{4}",
                clickedCell.ShopItem.Name, clickedCell.ShopItem.BasePrice,
                clickedCell.ShopItem.Stock, sizeText,
                canAfford ? "" : " ⚠ 所持金が足りません");

            RenderBothGrids();
        }
    }

    private void PlayerGridCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(PlayerGridCanvas);
        int gx = (int)(pos.X / CellSize);
        int gy = (int)(pos.Y / CellSize);

        var clickedCell = _playerCells.FirstOrDefault(c =>
            gx >= c.GridX && gx < c.GridX + c.Width &&
            gy >= c.GridY && gy < c.GridY + c.Height);

        if (clickedCell != null)
        {
            _selectedPlayerIndex = clickedCell.Index;
            _selectedShopIndex = -1;
            SellButton.IsEnabled = true;
            BuyButton.IsEnabled = false;

            int sellPrice = Math.Max(1, clickedCell.Item.BasePrice / 2);
            ItemInfoText.Text = string.Format("{0} — 売値: {1}G", clickedCell.Item.GetDisplayName(), sellPrice);

            RenderBothGrids();
        }
    }

    #endregion

    #region Buy/Sell Actions

    private void BuyButton_Click(object sender, RoutedEventArgs e)
    {
        var cell = _shopCells.FirstOrDefault(c => c.Index == _selectedShopIndex);
        if (cell == null) return;

        bool success = _controller.TryBuyItem(_shopType, cell.Index);
        if (success)
        {
            _selectedShopIndex = -1;
            BuyButton.IsEnabled = false;
            UpdateGold();
            RenderBothGrids();
            ItemInfoText.Text = "購入しました";
        }
    }

    private void SellButton_Click(object sender, RoutedEventArgs e)
    {
        var cell = _playerCells.FirstOrDefault(c => c.Index == _selectedPlayerIndex);
        if (cell == null) return;

        var inventory = (Inventory)_controller.Player.Inventory;
        if (cell.Index < inventory.Items.Count)
        {
            var item = inventory.Items[cell.Index];
            bool success = _controller.TrySellItem(item.GetDisplayName(), item.BasePrice);
            if (success)
            {
                inventory.Remove(item);
                _selectedPlayerIndex = -1;
                SellButton.IsEnabled = false;
                UpdateGold();
                RenderBothGrids();
                ItemInfoText.Text = "売却しました";
            }
        }
    }

    #endregion

    #region Common Methods

    private void CloseButton_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { DialogResult = false; Close(); }
        e.Handled = true;
    }

    private void UpdateGold() { GoldText.Text = string.Format("{0}G", _controller.Player.Gold); }

    private static void DrawGridLines(Canvas canvas, int gridWidth, int gridHeight)
    {
        for (int x = 0; x <= gridWidth; x++)
        {
            var line = new Line
            {
                X1 = x * CellSize, Y1 = 0,
                X2 = x * CellSize, Y2 = gridHeight * CellSize,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }
        for (int y = 0; y <= gridHeight; y++)
        {
            var line = new Line
            {
                X1 = 0, Y1 = y * CellSize,
                X2 = gridWidth * CellSize, Y2 = y * CellSize,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60)),
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }
    }

    private static (int X, int Y)? FindFreePosition(bool[,] placed, int w, int h, int gridWidth, int gridHeight)
    {
        for (int y = 0; y <= gridHeight - h; y++)
        {
            for (int x = 0; x <= gridWidth - w; x++)
            {
                bool fits = true;
                for (int dx = 0; dx < w && fits; dx++)
                    for (int dy = 0; dy < h && fits; dy++)
                        if (placed[x + dx, y + dy])
                            fits = false;
                if (fits) return (x, y);
            }
        }
        return null;
    }

    private static string FormatGridSize(GridItemSize size)
    {
        var (w, h) = GridInventorySystem.GetDimensions(size);
        return string.Format("{0}x{1}", w, h);
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

    private static Color GetShopItemColor(ShopSystem.ShopItem item)
    {
        // ショップ商品はカテゴリで色分け
        return item.ShopType switch
        {
            FacilityType.WeaponShop => Color.FromRgb(0x3a, 0x1a, 0x1a),
            FacilityType.ArmorShop => Color.FromRgb(0x1a, 0x1a, 0x3a),
            FacilityType.MagicShop => Color.FromRgb(0x2a, 0x1a, 0x3a),
            _ => Color.FromRgb(0x1a, 0x2a, 0x1a)
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

    private static Color GetRarityTextColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Colors.White,
            ItemRarity.Uncommon => Color.FromRgb(0x4e, 0xcc, 0xa3),
            ItemRarity.Rare => Color.FromRgb(0x53, 0x7b, 0xff),
            ItemRarity.Epic => Color.FromRgb(0xc0, 0x5e, 0xff),
            ItemRarity.Legendary => Color.FromRgb(0xff, 0xd9, 0x3d),
            ItemRarity.Unique => Color.FromRgb(0xff, 0x8c, 0x42),
            _ => Colors.White
        };
    }

    private static string GetShopName(FacilityType shopType) => shopType switch
    {
        FacilityType.GeneralShop => "雑貨店",
        FacilityType.WeaponShop => "武器店",
        FacilityType.ArmorShop => "防具店",
        FacilityType.MagicShop => "魔法店",
        _ => "ショップ"
    };

    #endregion
}

/// <summary>ショップ側グリッドセル情報</summary>
public class ShopGridCellInfo
{
    public int Index { get; set; }
    public ShopSystem.ShopItem ShopItem { get; set; } = null!;
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>プレイヤー側グリッドセル情報</summary>
public class PlayerGridCellInfo
{
    public int Index { get; set; }
    public Item Item { get; set; } = null!;
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
