using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;

namespace RougelikeGame.Gui;

/// <summary>
/// セーブデータ選択ウィンドウ
/// </summary>
public partial class SaveDataSelectWindow : Window
{
    /// <summary>選択されたセーブスロット番号</summary>
    public int SelectedSlot { get; private set; }

    /// <summary>確定されたか</summary>
    public bool Confirmed { get; private set; }

    public SaveDataSelectWindow()
    {
        InitializeComponent();
        LoadSaveSlots();
    }

    private void LoadSaveSlots()
    {
        var slots = SaveManager.GetAllSaveSlots();
        var items = new List<SaveSlotItem>();

        foreach (var slot in slots)
        {
            var data = SaveManager.Load(slot);
            if (data == null) continue;

            var player = data.Player;
            var raceText = GetRaceDisplayName(player.Race);
            var classText = GetClassDisplayName(player.CharacterClass);

            items.Add(new SaveSlotItem(
                Slot: slot,
                SlotLabel: $"[{slot}]",
                PlayerName: player.Name,
                LevelText: $"Lv.{player.Level}",
                RaceClassName: $"{raceText} / {classText}",
                FloorText: data.IsOnSurface ? "地上" : $"B{data.CurrentFloor}F",
                TerritoryText: GetTerritoryDisplayName(data.CurrentTerritory),
                DifficultyText: GetDifficultyDisplayName(data.Difficulty),
                SavedAtText: data.SavedAt.ToString("yyyy/MM/dd HH:mm"),
                TurnCount: data.TurnCount,
                Gold: player.Gold,
                Hp: player.CurrentHp,
                Mp: player.CurrentMp
            ));
        }

        SaveSlotList.ItemsSource = items;
        if (items.Count > 0)
            SaveSlotList.SelectedIndex = 0;
    }

    private void SaveSlotList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SaveSlotList.SelectedItem is SaveSlotItem item)
        {
            DetailText.Text = $"HP: {item.Hp}  MP: {item.Mp}  所持金: {item.Gold}G  ターン数: {item.TurnCount}";
        }
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        ConfirmSelection();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveSlotList.SelectedItem is not SaveSlotItem item) return;

        var result = MessageBox.Show(
            $"スロット {item.Slot} のセーブデータを削除しますか？\nこの操作は取り消せません。",
            "セーブデータ削除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            SaveManager.DeleteSave(item.Slot);
            LoadSaveSlots();

            // セーブデータが全て無くなったらウィンドウを閉じる
            if (SaveSlotList.Items.Count == 0)
            {
                DialogResult = false;
                Close();
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
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
                e.Handled = true;
                break;
            case Key.Enter:
                ConfirmSelection();
                e.Handled = true;
                break;
            case Key.Delete:
                DeleteButton_Click(sender, e);
                e.Handled = true;
                break;
        }
    }

    private void ConfirmSelection()
    {
        if (SaveSlotList.SelectedItem is SaveSlotItem item)
        {
            SelectedSlot = item.Slot;
            Confirmed = true;
            DialogResult = true;
            Close();
        }
    }

    private static string GetRaceDisplayName(Race race) => race switch
    {
        Race.Human => "人間",
        Race.Elf => "エルフ",
        Race.Dwarf => "ドワーフ",
        Race.Halfling => "ハーフリング",
        Race.Orc => "オーク",
        Race.Undead => "アンデッド",
        Race.Beastfolk => "獣人",
        Race.Demon => "悪魔",
        Race.FallenAngel => "堕天使",
        Race.Slime => "スライム",
        _ => race.ToString()
    };

    private static string GetClassDisplayName(RougelikeGame.Core.CharacterClass cls) => cls switch
    {
        RougelikeGame.Core.CharacterClass.Fighter => "戦士",
        RougelikeGame.Core.CharacterClass.Knight => "騎士",
        RougelikeGame.Core.CharacterClass.Mage => "魔術師",
        RougelikeGame.Core.CharacterClass.Thief => "盗賊",
        RougelikeGame.Core.CharacterClass.Cleric => "僧侶",
        RougelikeGame.Core.CharacterClass.Ranger => "狩人",
        RougelikeGame.Core.CharacterClass.Monk => "修道士",
        RougelikeGame.Core.CharacterClass.Necromancer => "死霊術師",
        RougelikeGame.Core.CharacterClass.Bard => "吟遊詩人",
        RougelikeGame.Core.CharacterClass.Alchemist => "錬金術師",
        _ => cls.ToString()
    };

    private static string GetDifficultyDisplayName(DifficultyLevel difficulty) => difficulty switch
    {
        DifficultyLevel.Easy => "Easy",
        DifficultyLevel.Normal => "Normal",
        DifficultyLevel.Hard => "Hard",
        DifficultyLevel.Nightmare => "Nightmare",
        DifficultyLevel.Ironman => "Ironman",
        _ => difficulty.ToString()
    };

    private static string GetTerritoryDisplayName(string territory) => territory switch
    {
        "Capital" => "王都",
        "Forest" => "森林地帯",
        "Mountain" => "山岳地帯",
        "Desert" => "砂漠地帯",
        "Swamp" => "沼沢地帯",
        "Coast" => "海岸地帯",
        "Ruins" => "古代遺跡",
        "Volcano" => "火山地帯",
        "Snowfield" => "雪原地帯",
        "DarkRealm" => "暗黒領域",
        _ => territory
    };

    /// <summary>セーブスロット表示用データ</summary>
    public record SaveSlotItem(
        int Slot,
        string SlotLabel,
        string PlayerName,
        string LevelText,
        string RaceClassName,
        string FloorText,
        string TerritoryText,
        string DifficultyText,
        string SavedAtText,
        int TurnCount,
        int Gold,
        int Hp,
        int Mp);
}
