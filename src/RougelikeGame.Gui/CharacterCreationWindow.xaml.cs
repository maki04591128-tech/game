using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// キャラクター作成ウィンドウ
/// </summary>
public partial class CharacterCreationWindow : Window
{
    /// <summary>選択された種族</summary>
    public Race SelectedRace { get; private set; } = Race.Human;

    /// <summary>選択された職業</summary>
    public RougelikeGame.Core.CharacterClass SelectedClass { get; private set; } = RougelikeGame.Core.CharacterClass.Fighter;

    /// <summary>選択された素性</summary>
    public RougelikeGame.Core.Background SelectedBackground { get; private set; } = RougelikeGame.Core.Background.Adventurer;

    /// <summary>入力された名前</summary>
    public string PlayerName { get; private set; } = "冒険者";

    /// <summary>確定されたか</summary>
    public bool Confirmed { get; private set; }

    public CharacterCreationWindow()
    {
        InitializeComponent();
        LoadLists();
    }

    private void LoadLists()
    {
        // 種族リスト
        var races = RaceDefinition.GetAll().Values
            .Select(r => new SelectionItem(r.Race.ToString(), r.Name, r.Description, r))
            .ToList();
        RaceList.ItemsSource = races;
        RaceList.SelectedIndex = 0;

        // 職業リスト
        var classes = ClassDefinition.GetAll().Values
            .Select(c => new SelectionItem(c.Class.ToString(), c.Name, c.Description, c))
            .ToList();
        ClassList.ItemsSource = classes;
        ClassList.SelectedIndex = 0;

        // 素性リスト
        var backgrounds = BackgroundDefinition.GetAll().Values
            .Select(b => new SelectionItem(b.Background.ToString(), b.Name, b.Description, b))
            .ToList();
        BackgroundList.ItemsSource = backgrounds;
        BackgroundList.SelectedIndex = 0;
    }

    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (RaceList.SelectedItem is not SelectionItem raceItem) return;
        if (ClassList.SelectedItem is not SelectionItem classItem) return;
        if (BackgroundList.SelectedItem is not SelectionItem bgItem) return;

        var raceDef = (RaceDefinition)raceItem.Data;
        var classDef = (ClassDefinition)classItem.Data;
        var bgDef = (BackgroundDefinition)bgItem.Data;

        // ステータス計算
        var stats = Stats.Default
            .Apply(raceDef.StatBonus)
            .Apply(classDef.StatBonus)
            .Apply(bgDef.StatBonus);

        int hp = stats.MaxHp + raceDef.HpBonus + classDef.HpBonus;
        int mp = stats.MaxMp + raceDef.MpBonus + classDef.MpBonus;

        var sb = new StringBuilder();
        sb.AppendLine($"STR:{stats.Strength}  VIT:{stats.Vitality}  AGI:{stats.Agility}  DEX:{stats.Dexterity}  INT:{stats.Intelligence}");
        sb.AppendLine($"MND:{stats.Mind}  PER:{stats.Perception}  LUK:{stats.Luck}  CHA:{stats.Charisma}");
        sb.AppendLine($"HP:{hp}  MP:{mp}  初期金:{bgDef.StartingGold}G");
        sb.Append($"特性: {string.Join(", ", raceDef.Traits)}");
        if (classDef.InitialSkills.Length > 0)
            sb.Append($"  初期スキル: {string.Join(", ", classDef.InitialSkills)}");

        PreviewText.Text = sb.ToString();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (RaceList.SelectedItem is not SelectionItem raceItem) return;
        if (ClassList.SelectedItem is not SelectionItem classItem) return;
        if (BackgroundList.SelectedItem is not SelectionItem bgItem) return;

        var raceDef = (RaceDefinition)raceItem.Data;
        var classDef = (ClassDefinition)classItem.Data;
        var bgDef = (BackgroundDefinition)bgItem.Data;

        SelectedRace = raceDef.Race;
        SelectedClass = classDef.Class;
        SelectedBackground = bgDef.Background;
        PlayerName = string.IsNullOrWhiteSpace(NameBox.Text) ? "冒険者" : NameBox.Text.Trim();
        Confirmed = true;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && !NameBox.IsFocused)
        {
            ConfirmButton_Click(sender, e);
            e.Handled = true;
        }
    }

    public record SelectionItem(string Id, string Name, string Description, object Data);
}
