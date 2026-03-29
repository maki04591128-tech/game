using System.Windows;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 死亡録ウィンドウ - 死亡統計・記録閲覧
/// </summary>
public partial class DeathLogWindow : Window
{
    private readonly DeathLogSystem _deathLog;

    public DeathLogWindow(GameController controller)
    {
        InitializeComponent();
        _deathLog = controller.GetDeathLogSystem();

        LoadData();
    }

    private void LoadData()
    {
        TotalDeathsText.Text = $"総死亡回数: {_deathLog.TotalDeaths}";

        if (_deathLog.TotalDeaths == 0)
        {
            HighestLevelText.Text = "最高レベル: -";
            DeepestFloorText.Text = "最深階: -";
            MostCommonCauseText.Text = "最多死因: -";
            CauseBreakdownText.Text = "データなし";
            LogList.ItemsSource = null;
            return;
        }

        HighestLevelText.Text = $"最高レベル: Lv.{_deathLog.GetHighestLevel()}";
        DeepestFloorText.Text = $"最深階: {_deathLog.GetDeepestFloor()}F";

        var mostCommon = _deathLog.GetMostCommonCause();
        MostCommonCauseText.Text = $"最多死因: {(mostCommon != null ? GetCauseName(mostCommon.Value) : "-")}";

        var breakdown = _deathLog.GetDeathsByCategory();
        CauseBreakdownText.Text = string.Join("\n",
            breakdown.OrderByDescending(b => b.Value)
                     .Select(b => $"{GetCauseName(b.Key)}: {b.Value}回"));

        var logs = _deathLog.AllLogs
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new DeathLogViewModel
            {
                RunText = $"#{l.RunNumber}",
                CharacterText = $"{l.CharacterName} ({l.Race}/{l.Class})",
                LevelText = $"Lv.{l.Level}",
                CauseText = $"死因: {GetCauseName(l.Cause)} — {l.CauseDetail}",
                LocationText = $"{l.Location} {l.Floor}F",
                TurnText = $"{l.TotalTurns}ターン"
            })
            .ToList();

        LogList.ItemsSource = logs;
    }

    private static string GetCauseName(DeathCause cause) => cause switch
    {
        DeathCause.Combat => "戦闘",
        DeathCause.Boss => "ボス戦",
        DeathCause.Starvation => "餓死",
        DeathCause.Trap => "罠",
        DeathCause.TimeLimit => "時間切れ",
        DeathCause.Curse => "呪い",
        DeathCause.Suicide => "自殺",
        DeathCause.SanityDeath => "正気度喪失",
        DeathCause.Fall => "落下",
        DeathCause.Poison => "毒",
        DeathCause.Unknown => "不明",
        _ => cause.ToString()
    };

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Z)
            Close();
    }
}

public class DeathLogViewModel
{
    public string RunText { get; set; } = "";
    public string CharacterText { get; set; } = "";
    public string LevelText { get; set; } = "";
    public string CauseText { get; set; } = "";
    public string LocationText { get; set; } = "";
    public string TurnText { get; set; } = "";
}
