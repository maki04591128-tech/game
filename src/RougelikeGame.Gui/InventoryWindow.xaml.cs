using System.Windows;
using System.Windows.Input;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Gui;

/// <summary>
/// インベントリ表示ウィンドウ
/// </summary>
public partial class InventoryWindow : Window
{
    private readonly List<Item> _items;
    private readonly Player _player;

    public int SelectedIndex { get; private set; } = -1;

    public InventoryWindow(List<Item> items, Player player)
    {
        InitializeComponent();
        _items = items;
        _player = player;

        LoadItems();
    }

    private void LoadItems()
    {
        var displayItems = _items.Select((item, index) => new ItemDisplayModel
        {
            Index = index,
            DisplayName = item.GetDisplayName(),
            EquippedText = GetEquippedText(item),
            Item = item
        }).ToList();

        ItemList.ItemsSource = displayItems;

        if (displayItems.Count > 0)
        {
            ItemList.SelectedIndex = 0;
        }
    }

    private string GetEquippedText(Item item)
    {
        if (item is Weapon w && _player.Equipment.MainHand == w)
            return "[装備中]";
        if (item is Armor a && _player.Equipment[EquipmentSlot.Body] == a)
            return "[装備中]";
        if (item is Shield s && _player.Equipment.OffHand == s)
            return "[装備中]";
        return "";
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                UseSelectedItem();
                break;
            case Key.Escape:
                DialogResult = false;
                Close();
                break;
            case Key.D1: SelectItem(0); break;
            case Key.D2: SelectItem(1); break;
            case Key.D3: SelectItem(2); break;
            case Key.D4: SelectItem(3); break;
            case Key.D5: SelectItem(4); break;
            case Key.D6: SelectItem(5); break;
            case Key.D7: SelectItem(6); break;
            case Key.D8: SelectItem(7); break;
            case Key.D9: SelectItem(8); break;
        }
    }

    private void SelectItem(int index)
    {
        if (index < _items.Count)
        {
            ItemList.SelectedIndex = index;
            UseSelectedItem();
        }
    }

    private void UseSelectedItem()
    {
        if (ItemList.SelectedItem is ItemDisplayModel model)
        {
            SelectedIndex = model.Index;
            DialogResult = true;
            Close();
        }
    }

    private void ItemList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        UseButton.IsEnabled = ItemList.SelectedItem != null;
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
}

public class ItemDisplayModel
{
    public int Index { get; set; }
    public string DisplayName { get; set; } = "";
    public string EquippedText { get; set; } = "";
    public Item Item { get; set; } = null!;
}
