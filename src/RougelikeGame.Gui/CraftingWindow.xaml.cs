using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Gui;

/// <summary>
/// 合成工房ウィンドウ
/// </summary>
public partial class CraftingWindow : Window
{
    private readonly GameController _controller;

    private enum CraftingMode { Craft, Enhance, Enchant }
    private CraftingMode _mode = CraftingMode.Craft;

    public CraftingWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;
        LoadCraftRecipes();
        UpdateTabVisuals();
    }

    private void LoadCraftRecipes()
    {
        var recipes = _controller.GetAvailableRecipes();
        var items = recipes.Select(r => new CraftingItemViewModel(
            r.RecipeId,
            r.Name,
            r.Description,
            $"{r.CraftingCost}G",
            string.Join(", ", r.Materials.Select(m => $"{m.ItemId}×{m.Quantity}"))
        )).ToList();
        ItemList.ItemsSource = items;
        ActionButton.Content = "合成 [Enter]";
    }

    private void LoadEnhanceItems()
    {
        var inventory = (Inventory)_controller.Player.Inventory;
        var items = inventory.Items
            .OfType<EquipmentItem>()
            .Select((eq, i) => new CraftingItemViewModel(
                i.ToString(),
                $"{eq.GetDisplayName()} (+{eq.EnhancementLevel})",
                $"強化レベル: {eq.EnhancementLevel}",
                "100G",
                ""
            )).ToList();
        ItemList.ItemsSource = items;
        ActionButton.Content = "強化 [Enter]";
    }

    private void LoadEnchantItems()
    {
        var inventory = (Inventory)_controller.Player.Inventory;
        var items = inventory.Items
            .OfType<Weapon>()
            .Select((w, i) => new CraftingItemViewModel(
                i.ToString(),
                w.GetDisplayName(),
                $"現在の属性: {GetElementName(w.Element)}",
                "200G",
                ""
            )).ToList();
        ItemList.ItemsSource = items;
        ActionButton.Content = "付与 [Enter]";
    }

    private void UpdateTabVisuals()
    {
        var activeColor = System.Windows.Media.Brushes.SeaGreen;
        var inactiveColor = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0f3460"));

        CraftTab.Background = _mode == CraftingMode.Craft ? activeColor : inactiveColor;
        EnhanceTab.Background = _mode == CraftingMode.Enhance ? activeColor : inactiveColor;
        EnchantTab.Background = _mode == CraftingMode.Enchant ? activeColor : inactiveColor;
    }

    private void CraftTab_Click(object sender, RoutedEventArgs e)
    {
        _mode = CraftingMode.Craft;
        LoadCraftRecipes();
        UpdateTabVisuals();
        ClearDetail();
    }

    private void EnhanceTab_Click(object sender, RoutedEventArgs e)
    {
        _mode = CraftingMode.Enhance;
        LoadEnhanceItems();
        UpdateTabVisuals();
        ClearDetail();
    }

    private void EnchantTab_Click(object sender, RoutedEventArgs e)
    {
        _mode = CraftingMode.Enchant;
        LoadEnchantItems();
        UpdateTabVisuals();
        ClearDetail();
    }

    private void ClearDetail()
    {
        DetailText.Text = "項目を選択してください";
        MaterialsText.Text = "";
        ActionButton.IsEnabled = false;
    }

    private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemList.SelectedItem is not CraftingItemViewModel selected)
        {
            ClearDetail();
            return;
        }

        DetailText.Text = $"{selected.Name}\n{selected.Description}";
        MaterialsText.Text = !string.IsNullOrEmpty(selected.Materials) ? $"必要素材: {selected.Materials}" : "";
        ActionButton.IsEnabled = true;
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
        if (ItemList.SelectedItem is not CraftingItemViewModel selected) return;

        switch (_mode)
        {
            case CraftingMode.Craft:
                _controller.TryCraftItem(selected.Id);
                LoadCraftRecipes();
                break;
            case CraftingMode.Enhance:
                ExecuteEnhance(selected);
                break;
            case CraftingMode.Enchant:
                ExecuteEnchant(selected);
                break;
        }
    }

    private void ExecuteEnhance(CraftingItemViewModel selected)
    {
        if (!int.TryParse(selected.Id, out int index)) return;
        var inventory = (Inventory)_controller.Player.Inventory;
        var equipments = inventory.Items.OfType<EquipmentItem>().ToList();
        if (index < equipments.Count)
        {
            _controller.TryEnhanceEquipment(equipments[index]);
            LoadEnhanceItems();
        }
    }

    private void ExecuteEnchant(CraftingItemViewModel selected)
    {
        if (!int.TryParse(selected.Id, out int index)) return;
        var inventory = (Inventory)_controller.Player.Inventory;
        var weapons = inventory.Items.OfType<Weapon>().ToList();
        if (index < weapons.Count)
        {
            // 属性選択ダイアログは簡略化: 順に炎→水→氷→雷→... でサイクル
            var elements = new[] { Element.Fire, Element.Water, Element.Ice, Element.Lightning, Element.Earth, Element.Wind, Element.Light, Element.Dark };
            int currentIdx = Array.IndexOf(elements, weapons[index].Element);
            var nextElement = elements[(currentIdx + 1) % elements.Length];
            _controller.TryEnchantWeapon(weapons[index], nextElement);
            LoadEnchantItems();
        }
    }

    private static string GetElementName(Element element) => element switch
    {
        Element.None => "なし",
        Element.Fire => "炎",
        Element.Water => "水",
        Element.Earth => "土",
        Element.Wind => "風",
        Element.Light => "光",
        Element.Dark => "闇",
        Element.Lightning => "雷",
        Element.Ice => "氷",
        Element.Poison => "毒",
        Element.Holy => "聖",
        Element.Curse => "呪",
        _ => element.ToString()
    };

    public class CraftingItemViewModel
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string CostDisplay { get; }
        public string Materials { get; }

        public CraftingItemViewModel(string id, string name, string desc, string cost, string materials)
        {
            Id = id;
            Name = name;
            Description = desc;
            CostDisplay = cost;
            Materials = materials;
        }
    }
}
