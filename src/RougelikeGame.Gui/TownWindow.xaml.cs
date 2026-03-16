using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 街施設メニューウィンドウ
/// </summary>
public partial class TownWindow : Window
{
    private readonly GameController _controller;
    private readonly List<FacilityViewModel> _facilities = new();

    /// <summary>ダンジョンへ入る要求が出されたか</summary>
    public bool EnterDungeonRequested { get; private set; }

    /// <summary>ショップを開く要求が出された場合の施設タイプ</summary>
    public FacilityType? OpenShopRequest { get; private set; }

    public TownWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;

        LoadFacilities();
        UpdateGoldDisplay();
    }

    private void LoadFacilities()
    {
        _facilities.Clear();
        var available = _controller.GetAvailableFacilities();

        foreach (var facility in available)
        {
            string icon = facility.Type switch
            {
                FacilityType.AdventurerGuild => "\u2694",
                FacilityType.GeneralShop => "\uD83D\uDED2",
                FacilityType.WeaponShop => "\u2694\uFE0F",
                FacilityType.ArmorShop => "\uD83D\uDEE1\uFE0F",
                FacilityType.Inn => "\uD83C\uDFE8",
                FacilityType.Smithy => "\uD83D\uDD28",
                FacilityType.Church => "\u26EA",
                FacilityType.Temple => "\u26E9\uFE0F",
                FacilityType.MagicShop => "\u2728",
                FacilityType.Library => "\uD83D\uDCDA",
                FacilityType.Bank => "\uD83C\uDFE6",
                FacilityType.Arena => "\uD83C\uDFDF\uFE0F",
                _ => "\u25A0"
            };

            _facilities.Add(new FacilityViewModel(
                facility.Type, icon, facility.Name, facility.Description));
        }

        FacilityList.ItemsSource = _facilities;

        // 領地情報
        var territory = _controller.GetCurrentTerritoryInfo();
        TownTitle.Text = $"{territory.Name}";
        TownDescription.Text = territory.Description;
    }

    private void UpdateGoldDisplay()
    {
        int gold = _controller.Player.Gold;
        int bank = _controller.GetBankBalance();
        GoldDisplay.Text = $"所持金: {gold:N0}G | 銀行残高: {bank:N0}G";
    }

    private void FacilityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FacilityList.SelectedItem is FacilityViewModel selected)
        {
            BankPanel.Visibility = selected.Type == FacilityType.Bank
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void UseButton_Click(object sender, RoutedEventArgs e)
    {
        if (FacilityList.SelectedItem is not FacilityViewModel selected) return;

        switch (selected.Type)
        {
            case FacilityType.Inn:
                _controller.ProcessInput(GameAction.UseInn);
                UpdateGoldDisplay();
                break;

            case FacilityType.Church:
                _controller.ProcessInput(GameAction.VisitChurch);
                UpdateGoldDisplay();
                break;

            case FacilityType.Bank:
                // 銀行パネルで操作
                BankPanel.Visibility = Visibility.Visible;
                break;

            case FacilityType.GeneralShop:
            case FacilityType.WeaponShop:
            case FacilityType.ArmorShop:
            case FacilityType.MagicShop:
                OpenShopRequest = selected.Type;
                DialogResult = true;
                Close();
                break;

            case FacilityType.Smithy:
                // 鍛冶屋 → 合成ウィンドウ
                var craftingDialog = new CraftingWindow(_controller);
                craftingDialog.Owner = this.Owner;
                craftingDialog.ShowDialog();
                UpdateGoldDisplay();
                break;

            case FacilityType.AdventurerGuild:
                // ギルド → 登録 + クエストログ
                if (!_controller.IsGuildRegistered)
                {
                    _controller.TryRegisterGuild();
                }
                var questDialog = new QuestLogWindow(_controller);
                questDialog.Owner = this.Owner;
                questDialog.ShowDialog();
                UpdateGoldDisplay();
                break;

            case FacilityType.Temple:
                // 神殿 → 宗教ウィンドウ
                var religionDialog = new ReligionWindow(_controller);
                religionDialog.Owner = this.Owner;
                religionDialog.ShowDialog();
                break;

            case FacilityType.Library:
                // 図書館 → ルーン語学習
                _controller.TriggerTutorial(TutorialTrigger.FirstMagicWord);
                MessageBox.Show("古代の書物を調べた。ルーン語の知識が深まった。",
                    "図書館", MessageBoxButton.OK, MessageBoxImage.Information);
                break;

            case FacilityType.Arena:
                MessageBox.Show("闘技場で腕試しをした！\n（将来のアップデートで対戦機能が追加されます）",
                    "闘技場", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
        }
    }

    private void DepositButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(BankAmountBox.Text, out int amount) || amount <= 0)
        {
            MessageBox.Show("有効な金額を入力してください", "入力エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _controller.TryDepositGold(amount);
        UpdateGoldDisplay();
    }

    private void WithdrawButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(BankAmountBox.Text, out int amount) || amount <= 0)
        {
            MessageBox.Show("有効な金額を入力してください", "入力エラー",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _controller.TryWithdrawGold(amount);
        UpdateGoldDisplay();
    }

    private void DungeonButton_Click(object sender, RoutedEventArgs e)
    {
        EnterDungeonRequested = true;
        DialogResult = true;
        Close();
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
                UseButton_Click(sender, e);
                break;
            case Key.D:
                DungeonButton_Click(sender, e);
                break;
        }
        e.Handled = true;
    }

    public record FacilityViewModel(FacilityType Type, string Icon, string Name, string Description);
}

