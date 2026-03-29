using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 仲間ウィンドウ - パーティ管理・AIモード設定・解雇
/// </summary>
public partial class CompanionWindow : Window
{
    private readonly GameController _controller;
    private readonly CompanionSystem _companions;
    private bool _suppressAIModeChange;

    public CompanionWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;
        _companions = controller.GetCompanionSystem();

        InitAIModeCombo();
        LoadData();
    }

    private void InitAIModeCombo()
    {
        AIModeCombo.Items.Add(new ComboBoxItem { Content = "攻撃優先", Tag = CompanionAIMode.Aggressive });
        AIModeCombo.Items.Add(new ComboBoxItem { Content = "防御優先", Tag = CompanionAIMode.Defensive });
        AIModeCombo.Items.Add(new ComboBoxItem { Content = "支援優先", Tag = CompanionAIMode.Support });
        AIModeCombo.Items.Add(new ComboBoxItem { Content = "待機", Tag = CompanionAIMode.Wait });
    }

    private void LoadData()
    {
        PartyCountText.Text = $"パーティ: {_companions.Party.Count}/{CompanionSystem.MaxPartySize} | 生存: {_companions.AliveCount}";

        if (_companions.Party.Count == 0)
        {
            PartyOverviewText.Text = "仲間がいません。冒険中にNPCを雇用したり仲間にすることができます。";
        }
        else
        {
            int totalAtk = _companions.Party.Where(c => c.IsAlive).Sum(c => c.Attack);
            int totalDef = _companions.Party.Where(c => c.IsAlive).Sum(c => c.Defense);
            PartyOverviewText.Text = $"合計攻撃力: {totalAtk} | 合計防御力: {totalDef}";
        }

        CheckLoyaltyWarning();
        BuildCompanionList();
    }

    private void CheckLoyaltyWarning()
    {
        var lowLoyalty = _companions.Party
            .Where(c => c.IsAlive && CompanionSystem.CheckDesertion(c.Loyalty, c.Type))
            .ToList();

        if (lowLoyalty.Count > 0)
        {
            LoyaltyWarning.Visibility = Visibility.Visible;
            LoyaltyWarningText.Text = "⚠ 忠誠度が低い仲間がいます: " +
                string.Join(", ", lowLoyalty.Select(c => $"{c.Name}(忠誠度:{c.Loyalty})")) +
                " — 離脱する可能性があります！";
        }
        else
        {
            LoyaltyWarning.Visibility = Visibility.Collapsed;
        }
    }

    private void BuildCompanionList()
    {
        var viewModels = _companions.Party.Select(c => new CompanionViewModel
        {
            Name = c.Name,
            StatusIcon = !c.IsAlive ? "💀" : c.Hp < c.MaxHp / 4 ? "⚠" : "●",
            TypeText = $"[{CompanionSystem.GetTypeName(c.Type)}]",
            HpText = $"HP: {c.Hp}/{c.MaxHp}",
            AIModeText = $"AI: {GetAIModeName(c.AIMode)}",
            NameColor = !c.IsAlive
                ? new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60))
                : c.Hp < c.MaxHp / 4
                    ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                    : new SolidColorBrush(Color.FromRgb(0x4e, 0xcc, 0xa3)),
            Data = c
        }).ToList();

        CompanionList.ItemsSource = viewModels;
    }

    private static string GetAIModeName(CompanionAIMode mode) => mode switch
    {
        CompanionAIMode.Aggressive => "攻撃優先",
        CompanionAIMode.Defensive => "防御優先",
        CompanionAIMode.Support => "支援優先",
        CompanionAIMode.Wait => "待機",
        _ => "不明"
    };

    private void CompanionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompanionList.SelectedItem is CompanionViewModel vm)
        {
            var c = vm.Data;
            CompNameText.Text = c.Name;
            CompTypeText.Text = $"種別: {CompanionSystem.GetTypeName(c.Type)}";
            CompLevelText.Text = $"レベル: {c.Level}";
            CompHpText.Text = $"HP: {c.Hp}/{c.MaxHp}";
            CompStatsText.Text = $"攻撃力: {c.Attack} | 防御力: {c.Defense}\nダメージ: {CompanionSystem.CalculateCompanionDamage(c)}";
            CompLoyaltyText.Text = $"忠誠度: {c.Loyalty}/100";

            _suppressAIModeChange = true;
            for (int i = 0; i < AIModeCombo.Items.Count; i++)
            {
                if (AIModeCombo.Items[i] is ComboBoxItem item && item.Tag is CompanionAIMode mode && mode == c.AIMode)
                {
                    AIModeCombo.SelectedIndex = i;
                    break;
                }
            }
            _suppressAIModeChange = false;

            AIModeCombo.IsEnabled = c.IsAlive;
            DismissButton.IsEnabled = true;
        }
        else
        {
            ClearDetail();
        }
    }

    private void ClearDetail()
    {
        CompNameText.Text = "仲間を選択してください";
        CompTypeText.Text = "";
        CompLevelText.Text = "";
        CompHpText.Text = "";
        CompStatsText.Text = "";
        CompLoyaltyText.Text = "";
        AIModeCombo.IsEnabled = false;
        DismissButton.IsEnabled = false;
    }

    private void AIModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressAIModeChange) return;
        if (CompanionList.SelectedItem is CompanionViewModel vm &&
            AIModeCombo.SelectedItem is ComboBoxItem item &&
            item.Tag is CompanionAIMode mode)
        {
            _companions.SetAIMode(vm.Name, mode);
            LoadData();
        }
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        TryDismissSelected();
    }

    private void TryDismissSelected()
    {
        if (CompanionList.SelectedItem is CompanionViewModel vm)
        {
            var result = MessageBox.Show(
                $"「{vm.Name}」を解雇しますか？",
                "解雇確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _companions.RemoveCompanion(vm.Name);
                LoadData();
                ClearDetail();
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D:
                TryDismissSelected();
                break;
            case Key.U:
            case Key.Escape:
                Close();
                break;
        }
    }
}

public class CompanionViewModel
{
    public string Name { get; set; } = "";
    public string StatusIcon { get; set; } = "";
    public string TypeText { get; set; } = "";
    public string HpText { get; set; } = "";
    public string AIModeText { get; set; } = "";
    public Brush NameColor { get; set; } = Brushes.White;
    public CompanionSystem.CompanionData Data { get; set; } = null!;
}
