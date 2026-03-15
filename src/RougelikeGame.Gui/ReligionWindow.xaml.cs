using System.Windows;
using System.Windows.Input;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Gui;

/// <summary>
/// 宗教・信仰ウィンドウ
/// </summary>
public partial class ReligionWindow : Window
{
    private readonly GameController _controller;

    public ReligionWindow(GameController controller)
    {
        InitializeComponent();
        _controller = controller;

        LoadReligionStatus();
    }

    private void LoadReligionStatus()
    {
        var player = _controller.Player;

        if (player.CurrentReligion != null)
        {
            var status = _controller.GetReligionStatus();
            ReligionNameText.Text = $"{status.ReligionName} — {status.GodName}";
            FaithRankText.Text = $"信仰段階: {ReligionDefinition.GetFaithRankName(status.Rank)}";
            FaithPointsText.Text = $"信仰度: {status.FaithPoints}/100";
            ReligionTitleText.Text = $"称号: {status.Title}";

            // 恩恵一覧
            var benefits = status.ActiveBenefits.Select(b => new BenefitViewModel(
                b.Name,
                b.Description,
                $"必要: {ReligionDefinition.GetFaithRankName(b.RequiredRank)}",
                (int)b.RequiredRank <= (int)status.Rank ? "✅" : "🔒"
            )).ToList();
            BenefitList.ItemsSource = benefits;
            ListTitle.Text = "恩恵一覧";

            PrayButton.Visibility = Visibility.Visible;
            LeaveButton.Visibility = Visibility.Visible;
            JoinButton.Visibility = Visibility.Collapsed;

            // 背教呪い警告
            if (player.HasApostasyCurse)
            {
                ApostasyWarning.Visibility = Visibility.Visible;
                ApostasyText.Text = $"⚠ 背教の呪い発動中（残り{player.ApostasyCurseRemainingDays}日）";
            }
        }
        else
        {
            ReligionNameText.Text = "無信仰";
            FaithRankText.Text = "";
            FaithPointsText.Text = "";
            ReligionTitleText.Text = "";

            // 利用可能な宗教一覧
            var religions = new[]
            {
                ReligionId.LightTemple, ReligionId.DarkCult, ReligionId.NatureWorship,
                ReligionId.DeathFaith, ReligionId.ChaosCult, ReligionId.Atheism
            };

            var items = religions.Select(id =>
            {
                var def = ReligionDatabase.GetAll().FirstOrDefault(r => r.Id == id);
                return def != null
                    ? new BenefitViewModel(def.Name, def.Description, $"神: {def.GodName}", "⛪")
                    : null;
            }).Where(x => x != null).ToList();

            BenefitList.ItemsSource = items;
            ListTitle.Text = "入信可能な宗教";

            PrayButton.Visibility = Visibility.Collapsed;
            LeaveButton.Visibility = Visibility.Collapsed;
            JoinButton.Visibility = Visibility.Visible;

            if (player.HasApostasyCurse)
            {
                ApostasyWarning.Visibility = Visibility.Visible;
                ApostasyText.Text = $"⚠ 背教の呪い発動中（残り{player.ApostasyCurseRemainingDays}日）— 入信に制限あり";
            }
        }
    }

    private void PrayButton_Click(object sender, RoutedEventArgs e)
    {
        _controller.ProcessInput(GameAction.Pray);
        LoadReligionStatus();
    }

    private void JoinButton_Click(object sender, RoutedEventArgs e)
    {
        if (BenefitList.SelectedItem is not BenefitViewModel selected) return;

        // 宗教名からIDを逆引き
        var allReligions = ReligionDatabase.GetAll();
        var match = allReligions.FirstOrDefault(r => r.Name == selected.Name);
        if (match == null) return;

        var result = _controller.TryJoinReligion(match.Id);
        if (result.Success)
        {
            MessageBox.Show(result.Message, "入信", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(result.Message, "入信失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        LoadReligionStatus();
    }

    private void LeaveButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "棄教すると背教の呪いを受け、信仰度が失われます。本当に棄教しますか？",
            "棄教確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _controller.ProcessInput(GameAction.LeaveReligion);
            LoadReligionStatus();
        }
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
            case Key.P:
                if (PrayButton.Visibility == Visibility.Visible)
                    PrayButton_Click(sender, e);
                break;
        }
        e.Handled = true;
    }

    public class BenefitViewModel
    {
        public string Name { get; }
        public string Description { get; }
        public string RequiredRank { get; }
        public string StatusIcon { get; }

        public BenefitViewModel(string name, string desc, string requiredRank, string statusIcon)
        {
            Name = name;
            Description = desc;
            RequiredRank = requiredRank;
            StatusIcon = statusIcon;
        }
    }
}
