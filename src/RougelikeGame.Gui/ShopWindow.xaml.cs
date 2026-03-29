using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// ショップウィンドウ — リスト表示ベースの購入/売却UI
/// </summary>
public partial class ShopWindow : Window
{
    private readonly GameController _controller;
    private readonly FacilityType _shopType;
    private int _tradeQuantity = 1;

    public ShopWindow(GameController controller, FacilityType shopType)
    {
        InitializeComponent();
        _controller = controller;
        _shopType = shopType;

        ShopTitle.Text = string.Format("【{0}】", GetShopName(shopType));
        UpdateGold();
        RenderBothLists();
    }

    private void RenderBothLists()
    {
        RenderShopList();
        RenderPlayerList();
    }

    private void RenderShopList()
    {
        var items = _controller.InitializeAndGetShopItems(_shopType);
        var viewModels = new List<ShopItemViewModel>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.Stock <= 0) continue;
            viewModels.Add(new ShopItemViewModel
            {
                Index = i,
                Name = item.Name,
                Price = item.BasePrice,
                PriceText = string.Format("{0}G", item.BasePrice),
                StockText = string.Format("x{0}", item.Stock),
                Stock = item.Stock,
                IsShopItem = true
            });
        }
        ShopListBox.ItemsSource = viewModels;
        ShopGridLabel.Text = string.Format("ショップ商品 ({0})", viewModels.Count);
    }

    private void RenderPlayerList()
    {
        var inventory = (Inventory)_controller.Player.Inventory;
        var viewModels = new List<ShopItemViewModel>();
        int idx = 0;
        foreach (var item in inventory.Items)
        {
            int sellPrice = Math.Max(1, item.BasePrice / 2);
            viewModels.Add(new ShopItemViewModel
            {
                Index = idx,
                Name = item.GetDisplayName(),
                Price = sellPrice,
                PriceText = string.Format("売:{0}G", sellPrice),
                StockText = "",
                Stock = 1,
                IsShopItem = false,
                NameColor = GetRarityBrush(item.Rarity),
                PlayerItem = item
            });
            idx++;
        }
        PlayerListBox.ItemsSource = viewModels;
        PlayerGridLabel.Text = string.Format("所持品 ({0} アイテム)", viewModels.Count);
    }

    private void ShopListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ShopListBox.SelectedItem is ShopItemViewModel vm)
        {
            PlayerListBox.SelectedItem = null;
            BuyButton.IsEnabled = true;
            SellButton.IsEnabled = false;
            _tradeQuantity = 1;
            QuantityText.Text = "1";
            int totalPrice = vm.Price * _tradeQuantity;
            bool canAfford = _controller.Player.Gold >= totalPrice;
            ItemInfoText.Text = string.Format("{0} — {1}G x{2} = {3}G{4}", vm.Name, vm.Price, _tradeQuantity, totalPrice,
                canAfford ? "" : " ⚠ 所持金が足りません");
        }
    }

    private void PlayerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlayerListBox.SelectedItem is ShopItemViewModel vm)
        {
            ShopListBox.SelectedItem = null;
            SellButton.IsEnabled = true;
            BuyButton.IsEnabled = false;
            _tradeQuantity = 1;
            QuantityText.Text = "1";
            ItemInfoText.Text = string.Format("{0} — 売値: {1}G", vm.Name, vm.Price);
        }
    }

    private void BuyButton_Click(object sender, RoutedEventArgs e)
    {
        if (ShopListBox.SelectedItem is not ShopItemViewModel vm) return;
        for (int i = 0; i < _tradeQuantity; i++)
        {
            bool success = _controller.TryBuyItem(_shopType, vm.Index);
            if (!success) break;
        }
        _tradeQuantity = 1;
        QuantityText.Text = "1";
        BuyButton.IsEnabled = false;
        UpdateGold();
        RenderBothLists();
        ItemInfoText.Text = "購入しました";
    }

    private void SellButton_Click(object sender, RoutedEventArgs e)
    {
        if (PlayerListBox.SelectedItem is not ShopItemViewModel vm) return;
        var inventory = (Inventory)_controller.Player.Inventory;
        if (vm.Index < inventory.Items.Count)
        {
            var item = inventory.Items[vm.Index];
            bool success = _controller.TrySellItem(item.GetDisplayName(), item.BasePrice);
            if (success)
            {
                inventory.Remove(item);
                _tradeQuantity = 1;
                QuantityText.Text = "1";
                SellButton.IsEnabled = false;
                UpdateGold();
                RenderBothLists();
                ItemInfoText.Text = "売却しました";
            }
        }
    }

    private void QuantityUp_Click(object sender, RoutedEventArgs e)
    {
        if (ShopListBox.SelectedItem is ShopItemViewModel vm)
        {
            _tradeQuantity = Math.Min(_tradeQuantity + 1, vm.Stock);
            QuantityText.Text = _tradeQuantity.ToString();
            int totalPrice = vm.Price * _tradeQuantity;
            bool canAfford = _controller.Player.Gold >= totalPrice;
            ItemInfoText.Text = string.Format("{0} — {1}G x{2} = {3}G{4}", vm.Name, vm.Price, _tradeQuantity, totalPrice,
                canAfford ? "" : " ⚠ 所持金が足りません");
        }
    }

    private void QuantityDown_Click(object sender, RoutedEventArgs e)
    {
        _tradeQuantity = Math.Max(1, _tradeQuantity - 1);
        QuantityText.Text = _tradeQuantity.ToString();
        if (ShopListBox.SelectedItem is ShopItemViewModel vm)
        {
            int totalPrice = vm.Price * _tradeQuantity;
            bool canAfford = _controller.Player.Gold >= totalPrice;
            ItemInfoText.Text = string.Format("{0} — {1}G x{2} = {3}G{4}", vm.Name, vm.Price, _tradeQuantity, totalPrice,
                canAfford ? "" : " ⚠ 所持金が足りません");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { DialogResult = false; Close(); }
        e.Handled = true;
    }

    private void UpdateGold() { GoldText.Text = string.Format("{0}G", _controller.Player.Gold); }

    private static Brush GetRarityBrush(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => Brushes.White,
        ItemRarity.Uncommon => new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
        ItemRarity.Rare => new SolidColorBrush(Color.FromRgb(0x53, 0x7b, 0xff)),
        ItemRarity.Epic => new SolidColorBrush(Color.FromRgb(0xc0, 0x5e, 0xff)),
        ItemRarity.Legendary => new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d)),
        ItemRarity.Unique => new SolidColorBrush(Color.FromRgb(0xff, 0x8c, 0x42)),
        _ => Brushes.White
    };

    private static string GetShopName(FacilityType shopType) => shopType switch
    {
        FacilityType.GeneralShop => "雑貨店",
        FacilityType.WeaponShop => "武器店",
        FacilityType.ArmorShop => "防具店",
        FacilityType.MagicShop => "魔法店",
        _ => "ショップ"
    };
}

public class ShopItemViewModel
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public string PriceText { get; set; } = "";
    public string StockText { get; set; } = "";
    public int Stock { get; set; }
    public bool IsShopItem { get; set; }
    public Brush NameColor { get; set; } = Brushes.White;
    public Item? PlayerItem { get; set; }
}
