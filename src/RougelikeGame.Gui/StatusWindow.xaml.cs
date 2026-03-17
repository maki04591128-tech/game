using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// プレイヤーステータス表示ウィンドウ
/// </summary>
public partial class StatusWindow : Window
{
    public StatusWindow(GameController controller)
    {
        InitializeComponent();
        LoadStatus(controller);
    }

    private void LoadStatus(GameController controller)
    {
        var player = controller.Player;
        var baseStats = player.BaseStats;
        var stats = player.EffectiveStats;

        // レベル・経験値
        LevelText.Text = $"Lv: {player.Level}";
        ExpText.Text = $"EXP: {player.Experience}/{player.ExperienceToNextLevel}";

        // HP/MP/SP
        HpText.Text = $"HP: {player.CurrentHp}/{player.MaxHp}";
        MpText.Text = $"MP: {player.CurrentMp}/{player.MaxMp}";
        SpText.Text = $"SP: {player.CurrentSp}/{player.MaxSp}";
        HungerText.Text = $"満腹度: {player.Hunger} ({player.HungerStage})";
        SanityText.Text = $"正気度: {player.Sanity} ({player.SanityStage})";

        // 基本ステータス（装備補正がある場合は差分表示）
        StrText.Text = FormatStat("筋力", baseStats.Strength, stats.Strength);
        VitText.Text = FormatStat("体力", baseStats.Vitality, stats.Vitality);
        AgiText.Text = FormatStat("敏捷", baseStats.Agility, stats.Agility);
        DexText.Text = FormatStat("器用", baseStats.Dexterity, stats.Dexterity);
        IntText.Text = FormatStat("知力", baseStats.Intelligence, stats.Intelligence);
        MndText.Text = FormatStat("精神", baseStats.Mind, stats.Mind);
        PerText.Text = FormatStat("知覚", baseStats.Perception, stats.Perception);
        ChaText.Text = FormatStat("魅力", baseStats.Charisma, stats.Charisma);
        LukText.Text = FormatStat("幸運", baseStats.Luck, stats.Luck);

        // 戦闘パラメータ
        PhysAtkText.Text = $"物理攻撃: {stats.PhysicalAttack}";
        PhysDefText.Text = $"物理防御: {stats.PhysicalDefense}";
        MagAtkText.Text = $"魔法攻撃: {stats.MagicalAttack}";
        MagDefText.Text = $"魔法防御: {stats.MagicalDefense}";
        HitRateText.Text = $"命中率: {stats.HitRate:P0}";
        EvasionText.Text = $"回避率: {stats.EvasionRate:P0}";
        CritRateText.Text = $"会心率: {stats.CriticalRate:P1}";
        ActionSpeedText.Text = $"行動速度: {stats.ActionSpeed}";
        EquipDefText.Text = $"装備防御: {player.Equipment.GetTotalPhysicalDefense()}";
        EquipMDefText.Text = $"装備魔防: {player.Equipment.GetTotalMagicDefense()}";

        // 装備
        LoadEquipment(player.Equipment);

        // 状態異常
        var effects = player.StatusEffects;
        if (effects.Count > 0)
        {
            StatusEffectsText.Text = string.Join("、", effects.Select(e => $"{e.Name}（残{e.Duration}ターン）"));
        }
        else
        {
            StatusEffectsText.Text = "なし";
        }

        // 身体状態・環境
        ThirstStatusText.Text = $"渇き: {controller.PlayerThirstName}";
        KarmaStatusText.Text = $"善悪: {KarmaSystem.GetKarmaRankName(controller.PlayerKarmaRank)} ({controller.PlayerKarma})";
        ReputationStatusText.Text = $"評判: {controller.PlayerReputationRank}";
        SeasonStatusText.Text = $"季節: {controller.CurrentSeasonName}";
        WeatherStatusText.Text = $"天候: {controller.CurrentWeatherName}";
        CompanionStatusText.Text = $"仲間: {controller.CompanionCount}/{CompanionSystem.MaxPartySize}";
    }

    private static string FormatStat(string name, int baseValue, int effectiveValue)
    {
        if (baseValue == effectiveValue)
            return $"{name}: {baseValue}";

        var diff = effectiveValue - baseValue;
        var sign = diff > 0 ? "+" : "";
        return $"{name}: {baseValue}({sign}{diff})";
    }

    private void LoadEquipment(Core.Items.Equipment equipment)
    {
        var slots = equipment.GetAll();
        foreach (var (slot, item) in slots)
        {
            var slotName = GetSlotName(slot);
            var itemName = item?.Name ?? "（なし）";
            var text = new TextBlock
            {
                Text = $"{slotName}: {itemName}",
                Foreground = item != null
                    ? System.Windows.Media.Brushes.White
                    : System.Windows.Media.Brushes.Gray,
                FontSize = 13,
                Margin = new Thickness(0, 1, 0, 1)
            };
            EquipmentPanel.Children.Add(text);
        }
    }

    private static string GetSlotName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.MainHand => "メイン武器",
        EquipmentSlot.OffHand => "サブ/盾",
        EquipmentSlot.Head => "頭部",
        EquipmentSlot.Body => "胴体",
        EquipmentSlot.Hands => "手",
        EquipmentSlot.Feet => "足",
        EquipmentSlot.Neck => "首飾り",
        EquipmentSlot.Ring1 => "指輪1",
        EquipmentSlot.Ring2 => "指輪2",
        EquipmentSlot.Back => "背中",
        EquipmentSlot.Waist => "腰",
        _ => slot.ToString()
    };

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.C)
        {
            DialogResult = false;
            Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
