using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Gui;

/// <summary>
/// ショップウィンドウ
/// </summary>
public partial class ShopWindow : Window
{
    private readonly GameController _controller;
    private readonly FacilityType _shopType;
    private bool _isBuyMode = true;
    private List<ShopItemViewModel> _buyItems = new();
    private List<ShopItemViewModel> _sellItems = new();

    public ShopWindow(GameController controller, FacilityType shopType)
    {
        InitializeComponent();
        _controller = controller;
        _shopType = shopType;

        ShopTitle.Text = $"【{GetShopName(shopType)}】";
        UpdateGold();
        LoadBuyItems();
        UpdateTabVisuals();
    }

    private void LoadBuyItems()
    {
        _buyItems.Clear();
        var items = _controller.InitializeAndGetShopItems(_shopType);
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            _buyItems.Add(new ShopItemViewModel(i, item.Name, item.BasePrice, item.Stock, false));
        }
        RefreshList();
    }

    private void LoadSellItems()
    {
        _sellItems.Clear();
        var inventory = (Inventory)_controller.Player.Inventory;
        int index = 0;
        foreach (var item in inventory.Items)
        {
            int sellPrice = Math.Max(1, item.BasePrice / 2);
            _sellItems.Add(new ShopItemViewModel(index, item.GetDisplayName(), sellPrice, 1, true));
            index++;
        }
        RefreshList();
    }

    private void RefreshList()
    {
        ItemList.ItemsSource = null;
        ItemList.ItemsSource = _isBuyMode ? _buyItems : _sellItems;
        ActionButton.Content = _isBuyMode ? "購入 [Enter]" : "売却 [Enter]";
        ActionButton.IsEnabled = false;
        ItemInfoText.Text = "アイテムを選択してください";
    }

    private void UpdateGold()
    {
        GoldText.Text = $"{_controller.Player.Gold}G";
    }

    private void UpdateTabVisuals()
    {
        if (_isBuyMode)
        {
            BuyTab.Background = System.Windows.Media.Brushes.SeaGreen;
            SellTab.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0f3460"));
        }
        else
        {
            SellTab.Background = System.Windows.Media.Brushes.SeaGreen;
            BuyTab.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0f3460"));
        }
    }

    private void BuyTab_Click(object sender, RoutedEventArgs e)
    {
        if (_isBuyMode) return;
        _isBuyMode = true;
        LoadBuyItems();
        UpdateTabVisuals();
    }

    private void SellTab_Click(object sender, RoutedEventArgs e)
    {
        if (!_isBuyMode) return;
        _isBuyMode = false;
        LoadSellItems();
        UpdateTabVisuals();
    }

    private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemList.SelectedItem is not ShopItemViewModel selected)
        {
            ActionButton.IsEnabled = false;
            ItemInfoText.Text = "アイテムを選択してください";
            return;
        }

        ActionButton.IsEnabled = true;

        if (_isBuyMode)
        {
            bool canAfford = _controller.Player.Gold >= selected.Price;
            ItemInfoText.Text = $"{selected.Name} — {selected.Price}G" +
                (canAfford ? "" : " ⚠ 所持金が足りません");
            ActionButton.IsEnabled = canAfford && selected.Stock > 0;
        }
        else
        {
            ItemInfoText.Text = $"{selected.Name} — 売値: {selected.Price}G";
        }
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        ExecuteAction();
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
                ExecuteAction();
                break;
        }
        e.Handled = true;
    }

    private void ExecuteAction()
    {
        if (ItemList.SelectedItem is not ShopItemViewModel selected) return;

        if (_isBuyMode)
        {
            bool success = _controller.TryBuyItem(_shopType, selected.Index);
            if (success)
            {
                UpdateGold();
                LoadBuyItems();
            }
        }
        else
        {
            var inventory = (Inventory)_controller.Player.Inventory;
            if (selected.Index < inventory.Items.Count)
            {
                var item = inventory.Items[selected.Index];
                bool success = _controller.TrySellItem(item.GetDisplayName(), item.BasePrice);
                if (success)
                {
                    inventory.Remove(item);
                    UpdateGold();
                    LoadSellItems();
                }
            }
        }
    }

    private static string GetShopName(FacilityType shopType) => shopType switch
    {
        FacilityType.GeneralShop => "雑貨店",
        FacilityType.WeaponShop => "武器店",
        FacilityType.ArmorShop => "防具店",
        FacilityType.MagicShop => "魔法店",
        _ => "ショップ"
    };

    public class ShopItemViewModel
    {
        public int Index { get; }
        public string Name { get; }
        public int Price { get; }
        public int Stock { get; }
        public bool IsSellItem { get; }
        public string PriceDisplay => $"{Price}G";
        public string StockDisplay => IsSellItem ? "" : $"在庫: {Stock}";

        public ShopItemViewModel(int index, string name, int price, int stock, bool isSellItem)
        {
            Index = index;
            Name = name;
            Price = price;
            Stock = stock;
            IsSellItem = isSellItem;
        }
    }
}
