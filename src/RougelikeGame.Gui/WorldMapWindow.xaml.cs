using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

public partial class WorldMapWindow : Window
{
    private readonly GameController _controller;
    private readonly List<TerritoryViewModel> _territories = new();
    private TerritoryId? _selectedTerritory;

    /// <summary>移動先の領地ID（確定時に設定）</summary>
    public TerritoryId? TravelDestination { get; private set; }

    public WorldMapWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;
        LoadCurrentTerritory();
        LoadAdjacentTerritories();
        LoadFacilities();
        LoadLocationsAndDungeons();
    }

    private void LoadCurrentTerritory()
    {
        var current = TerritoryDefinition.Get(_controller.CurrentTerritory);
        CurrentTerritoryName.Text = current.Name;
        CurrentTerritoryDesc.Text = current.Description;

        bool isOnSurface = _controller.IsOnSurface;
        SurfaceDungeonStatus.Text = isOnSurface ? "【地上】" : $"【ダンジョン B{_controller.CurrentFloor}F】";
        SurfaceDungeonStatus.Foreground = isOnSurface
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4e, 0xcc, 0xa3))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xff, 0x6b, 0x6b));

        CurrentLocationStatus.Text = $"推奨Lv.{current.MinLevel} | 最深部: B{current.MaxDungeonDepth}F | 施設数: {current.AvailableFacilities.Length}";
    }

    private void LoadAdjacentTerritories()
    {
        _territories.Clear();
        var adjacents = TerritoryDefinition.Get(_controller.CurrentTerritory).AdjacentTerritories;
        foreach (var tid in adjacents)
        {
            var def = TerritoryDefinition.Get(tid);
            bool visited = _controller.VisitedTerritories.Contains(tid);
            _territories.Add(new TerritoryViewModel(tid, def.Name, def.Description, def.MinLevel,
                def.MaxDungeonDepth, def.TravelTurnCost, def.AvailableFacilities, visited));
        }
        TerritoryList.ItemsSource = null;
        TerritoryList.ItemsSource = _territories;
    }

    private void LoadFacilities()
    {
        var current = TerritoryDefinition.Get(_controller.CurrentTerritory);
        var facilities = current.AvailableFacilities.Select(f => new FacilityViewModel(f)).ToList();
        FacilityList.ItemsSource = facilities;
    }

    private void LoadLocationsAndDungeons()
    {
        var currentId = _controller.CurrentTerritory;

        // ロケーション（ダンジョン以外）
        var locations = LocationDefinition.GetByTerritory(currentId)
            .Where(l => l.Type != LocationType.Dungeon)
            .Select(l => new LocationViewModel(l.Name, l.Description))
            .ToList();
        LocationList.ItemsSource = locations;

        // ダンジョン
        var dungeons = LocationDefinition.GetDungeonsByTerritory(currentId)
            .Select(d => new DungeonViewModel(d.Name, d.DangerLevel))
            .ToList();
        DungeonList.ItemsSource = dungeons;
    }

    private void TerritoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TerritoryList.SelectedItem is not TerritoryViewModel selected)
        {
            _selectedTerritory = null;
            TravelButton.IsEnabled = false;
            SelectedTerritoryInfo.Text = "領地を選択してください";
            return;
        }
        _selectedTerritory = selected.Id;
        int travelDays = selected.TravelTurnCost / TimeConstants.TurnsPerDay;
        bool canTravel = _controller.Player.Level >= selected.MinLevel;

        var facilityNames = string.Join("、", selected.Facilities.Select(GetFacilityName));
        SelectedTerritoryInfo.Text = $"{selected.Name}: {selected.Description}\n"
            + $"推奨Lv.{selected.MinLevel} | 最深部: B{selected.MaxDungeonDepth}F | 移動: 約{travelDays}日\n"
            + $"施設: {(facilityNames.Length > 0 ? facilityNames : "なし")}";

        TravelButton.IsEnabled = canTravel && _controller.IsOnSurface;
        if (!canTravel)
            SelectedTerritoryInfo.Text += $"\n※レベルが足りません（Lv.{selected.MinLevel}以上必要）";
        if (!_controller.IsOnSurface)
            SelectedTerritoryInfo.Text += "\n※ダンジョン内から移動はできません";
    }

    private void TravelButton_Click(object sender, RoutedEventArgs e) => ExecuteTravel();

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
            case Key.J:
                DialogResult = false;
                Close();
                e.Handled = true;
                break;
            case Key.Enter:
                ExecuteTravel();
                e.Handled = true;
                break;
        }
    }

    private void ExecuteTravel()
    {
        if (!_selectedTerritory.HasValue) return;
        if (!_controller.IsOnSurface) return;
        TravelDestination = _selectedTerritory.Value;
        DialogResult = true;
        Close();
    }

    private static string GetFacilityName(FacilityType facility) => facility switch
    {
        FacilityType.AdventurerGuild => "冒険者ギルド",
        FacilityType.GeneralShop => "雑貨店",
        FacilityType.WeaponShop => "武器店",
        FacilityType.ArmorShop => "防具店",
        FacilityType.Inn => "宿屋",
        FacilityType.Smithy => "鍛冶屋",
        FacilityType.Church => "教会",
        FacilityType.Temple => "神殿",
        FacilityType.MagicShop => "魔法店",
        FacilityType.Library => "図書館",
        FacilityType.Bank => "銀行",
        FacilityType.Arena => "闘技場",
        _ => facility.ToString()
    };

    public class TerritoryViewModel
    {
        public TerritoryId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MinLevel { get; }
        public int MaxDungeonDepth { get; }
        public int TravelTurnCost { get; }
        public FacilityType[] Facilities { get; }
        public bool Visited { get; }
        public string LevelRange => $"推奨Lv.{MinLevel}";
        public string VisitedMark => Visited ? "✓" : "";

        public TerritoryViewModel(TerritoryId id, string name, string desc, int minLevel,
            int maxDepth, int travelCost, FacilityType[] facilities, bool visited)
        {
            Id = id; Name = name; Description = desc; MinLevel = minLevel;
            MaxDungeonDepth = maxDepth; TravelTurnCost = travelCost;
            Facilities = facilities; Visited = visited;
        }
    }

    public class FacilityViewModel
    {
        public FacilityType Type { get; }
        public string DisplayName => GetFacilityName(Type);
        public FacilityViewModel(FacilityType type) => Type = type;
    }

    public class LocationViewModel
    {
        public string Name { get; }
        public string Description { get; }
        public string DisplayName => $"{Name} - {Description}";
        public LocationViewModel(string name, string desc) { Name = name; Description = desc; }
    }

    public class DungeonViewModel
    {
        public string Name { get; }
        public int DangerLevel { get; }
        public string DisplayName => $"{Name} (危険度{DangerLevel})";
        public DungeonViewModel(string name, int dangerLevel) { Name = name; DangerLevel = dangerLevel; }
    }
}
