using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

public partial class EncyclopediaWindow : Window
{
    private readonly EncyclopediaSystem _encyclopedia;
    private EncyclopediaCategory _currentCategory = EncyclopediaCategory.Monster;
    private readonly Button[] _tabButtons;

    public EncyclopediaWindow(GameController controller)
    {
        InitializeComponent();
        _encyclopedia = controller.GetEncyclopediaSystem();
        _tabButtons = new[] { TabMonster, TabItem, TabNpc, TabRegion };
        UpdateSummary();
        LoadCategory(_currentCategory);
    }

    private void UpdateSummary()
    {
        SummaryText.Text = "総エントリ: " + _encyclopedia.TotalEntries + " | 発見済み: " + _encyclopedia.DiscoveredEntries;
    }

    private void LoadCategory(EncyclopediaCategory category)
    {
        _currentCategory = category;
        UpdateTabHighlight();
        var entries = _encyclopedia.GetByCategory(category);
        var viewModels = entries.Select(e => new EncyclopediaEntryViewModel
        {
            Id = e.Id,
            DisplayName = e.DiscoveryLevel > 0 ? e.Name : "???",
            DiscoveryIcon = e.DiscoveryLevel >= e.MaxLevel ? "\u2605" : e.DiscoveryLevel > 0 ? "\u25C6" : "\u25CB",
            LevelText = e.Category == EncyclopediaCategory.Monster && e.KillCount > 0
                ? "[" + e.KillCount + "体撃破]"
                : "[" + e.DiscoveryLevel + "/" + e.MaxLevel + "]",
            NameColor = e.DiscoveryLevel >= e.MaxLevel
                ? new SolidColorBrush(Color.FromRgb(0xff, 0xd9, 0x3d))
                : e.DiscoveryLevel > 0
                    ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                    : new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60)),
            Entry = e
        }).ToList();
        EntryList.ItemsSource = viewModels;
        float discoveryRate = _encyclopedia.GetDiscoveryRate(category);
        float completionRate = _encyclopedia.GetCompletionRate(category);
        CategoryRateText.Text = GetCategoryName(category) + ": 発見率 " + discoveryRate.ToString("P0") + " | 完全開示率 " + completionRate.ToString("P0");
        ClearDetail();
    }

    private void UpdateTabHighlight()
    {
        var categories = new[] { EncyclopediaCategory.Monster, EncyclopediaCategory.Item, EncyclopediaCategory.Npc, EncyclopediaCategory.Region };
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            _tabButtons[i].BorderBrush = categories[i] == _currentCategory
                ? new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3))
                : new SolidColorBrush(Color.FromRgb(0x0f, 0x34, 0x60));
        }
    }

    private static string GetCategoryName(EncyclopediaCategory cat)
    {
        return cat switch
        {
            EncyclopediaCategory.Monster => "モンスター",
            EncyclopediaCategory.Item => "アイテム",
            EncyclopediaCategory.Npc => "NPC",
            EncyclopediaCategory.Region => "地域",
            _ => cat.ToString()
        };
    }

    private void EntryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EntryList.SelectedItem is EncyclopediaEntryViewModel vm)
        {
            var entry = vm.Entry;
            EntryNameText.Text = entry.DiscoveryLevel > 0 ? entry.Name : "???";
            EntryCategoryText.Text = "カテゴリ: " + GetCategoryName(entry.Category);
            if (entry.Category == EncyclopediaCategory.Monster && entry.KillCount > 0)
            {
                DiscoveryLevelText.Text = "撃破数: " + entry.KillCount + " | 開示Lv." + entry.DiscoveryLevel + "/" + entry.MaxLevel;
            }
            else
            {
                DiscoveryLevelText.Text = entry.DiscoveryLevel + "/" + entry.MaxLevel;
            }
            EntryDescText.Text = _encyclopedia.GetCurrentDescription(entry.Id);
        }
        else
        {
            ClearDetail();
        }
    }

    private void ClearDetail()
    {
        EntryNameText.Text = "エントリを選択してください";
        EntryCategoryText.Text = "";
        DiscoveryLevelText.Text = "";
        EntryDescText.Text = "";
    }

    private void TabMonster_Click(object sender, RoutedEventArgs e) => LoadCategory(EncyclopediaCategory.Monster);
    private void TabItem_Click(object sender, RoutedEventArgs e) => LoadCategory(EncyclopediaCategory.Item);
    private void TabNpc_Click(object sender, RoutedEventArgs e) => LoadCategory(EncyclopediaCategory.Npc);
    private void TabRegion_Click(object sender, RoutedEventArgs e) => LoadCategory(EncyclopediaCategory.Region);

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1: LoadCategory(EncyclopediaCategory.Monster); break;
            case Key.D2: LoadCategory(EncyclopediaCategory.Item); break;
            case Key.D3: LoadCategory(EncyclopediaCategory.Npc); break;
            case Key.D4: LoadCategory(EncyclopediaCategory.Region); break;
            case Key.Y:
            case Key.Escape: Close(); break;
        }
    }
}

public class EncyclopediaEntryViewModel
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string DiscoveryIcon { get; set; } = "";
    public string LevelText { get; set; } = "";
    public Brush NameColor { get; set; } = Brushes.White;
    public EncyclopediaSystem.EncyclopediaEntry Entry { get; set; } = null!;
}
