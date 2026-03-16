using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class ReputationSystemTests
{
    #region 初期化テスト

    [Fact]
    public void Constructor_InitializesAllTerritoriesToZero()
    {
        var system = new ReputationSystem();
        var all = system.GetAllReputations();

        foreach (TerritoryId territory in Enum.GetValues<TerritoryId>())
        {
            Assert.Equal(0, all[territory]);
        }
    }

    [Fact]
    public void GetReputationRank_InitialState_ReturnsIndifferent()
    {
        var system = new ReputationSystem();
        Assert.Equal(ReputationRank.Indifferent, system.GetReputationRank(TerritoryId.Capital));
    }

    #endregion

    #region 評判変動テスト

    [Fact]
    public void ModifyReputation_PositiveAmount_IncreasesReputation()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, 30, "クエスト完了");
        Assert.Equal(30, system.GetReputation(TerritoryId.Capital));
    }

    [Fact]
    public void ModifyReputation_NegativeAmount_DecreasesReputation()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Forest, -25, "住民攻撃");
        Assert.Equal(-25, system.GetReputation(TerritoryId.Forest));
    }

    [Fact]
    public void ModifyReputation_ClampsToMax100()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Mountain, 200, "大功績");
        Assert.Equal(100, system.GetReputation(TerritoryId.Mountain));
    }

    [Fact]
    public void ModifyReputation_ClampsToMinMinus100()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Coast, -200, "大罪");
        Assert.Equal(-100, system.GetReputation(TerritoryId.Coast));
    }

    [Fact]
    public void ModifyReputation_IndependentPerTerritory()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, 50, "王都貢献");
        system.ModifyReputation(TerritoryId.Forest, -30, "森林破壊");

        Assert.Equal(50, system.GetReputation(TerritoryId.Capital));
        Assert.Equal(-30, system.GetReputation(TerritoryId.Forest));
    }

    [Fact]
    public void ModifyReputation_FiresEvent()
    {
        var system = new ReputationSystem();
        ReputationChangedEventArgs? firedArgs = null;
        system.OnReputationChanged += args => firedArgs = args;

        system.ModifyReputation(TerritoryId.Southern, 25, "助力");

        Assert.NotNull(firedArgs);
        Assert.Equal(TerritoryId.Southern, firedArgs.Territory);
        Assert.Equal(0, firedArgs.OldValue);
        Assert.Equal(25, firedArgs.NewValue);
        Assert.Equal("助力", firedArgs.Reason);
    }

    [Fact]
    public void ModifyReputation_NoChange_DoesNotFireEvent()
    {
        var system = new ReputationSystem();
        // まず100にする
        system.ModifyReputation(TerritoryId.Capital, 100, "初期設定");

        bool eventFired = false;
        system.OnReputationChanged += _ => eventFired = true;

        // さらに+10してもクランプで変化なし
        system.ModifyReputation(TerritoryId.Capital, 10, "追加");
        Assert.False(eventFired);
    }

    #endregion

    #region 評判段階判定テスト

    [Theory]
    [InlineData(100, ReputationRank.Revered)]
    [InlineData(80, ReputationRank.Revered)]
    [InlineData(79, ReputationRank.Trusted)]
    [InlineData(50, ReputationRank.Trusted)]
    [InlineData(49, ReputationRank.Friendly)]
    [InlineData(20, ReputationRank.Friendly)]
    [InlineData(19, ReputationRank.Indifferent)]
    [InlineData(0, ReputationRank.Indifferent)]
    [InlineData(-19, ReputationRank.Indifferent)]
    [InlineData(-20, ReputationRank.Unfriendly)]
    [InlineData(-49, ReputationRank.Unfriendly)]
    [InlineData(-50, ReputationRank.Hostile)]
    [InlineData(-79, ReputationRank.Hostile)]
    [InlineData(-80, ReputationRank.Hated)]
    [InlineData(-100, ReputationRank.Hated)]
    public void GetReputationRank_ReturnsCorrectRank(int value, ReputationRank expected)
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, value, "テスト");
        Assert.Equal(expected, system.GetReputationRank(TerritoryId.Capital));
    }

    #endregion

    #region ショップ割引テスト

    [Theory]
    [InlineData(80, 0.8)]
    [InlineData(50, 0.9)]
    [InlineData(20, 0.95)]
    [InlineData(0, 1.0)]
    [InlineData(-20, 1.1)]
    [InlineData(-50, 1.3)]
    [InlineData(-80, 1.5)]
    public void GetShopDiscount_ReturnsCorrectRate(int reputation, double expected)
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, reputation, "テスト");
        Assert.Equal(expected, system.GetShopDiscount(TerritoryId.Capital), 2);
    }

    #endregion

    #region クエスト解放率テスト

    [Theory]
    [InlineData(80, 1.0)]
    [InlineData(50, 0.9)]
    [InlineData(20, 0.7)]
    [InlineData(0, 0.5)]
    [InlineData(-20, 0.3)]
    [InlineData(-50, 0.1)]
    [InlineData(-80, 0.0)]
    public void GetQuestAvailability_ReturnsCorrectRate(int reputation, double expected)
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, reputation, "テスト");
        Assert.Equal(expected, system.GetQuestAvailability(TerritoryId.Capital), 2);
    }

    #endregion

    #region 入場可否テスト

    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(-50, true)]
    [InlineData(-79, true)]
    [InlineData(-80, false)]
    [InlineData(-100, false)]
    public void IsWelcome_ReturnsCorrectly(int reputation, bool expected)
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, reputation, "テスト");
        Assert.Equal(expected, system.IsWelcome(TerritoryId.Capital));
    }

    #endregion

    #region 日本語名テスト

    [Theory]
    [InlineData(ReputationRank.Revered, "崇拝")]
    [InlineData(ReputationRank.Trusted, "信頼")]
    [InlineData(ReputationRank.Friendly, "友好")]
    [InlineData(ReputationRank.Indifferent, "無関心")]
    [InlineData(ReputationRank.Unfriendly, "不信")]
    [InlineData(ReputationRank.Hostile, "敵意")]
    [InlineData(ReputationRank.Hated, "憎悪")]
    public void GetReputationRankName_ReturnsJapaneseName(ReputationRank rank, string expected)
    {
        Assert.Equal(expected, ReputationSystem.GetReputationRankName(rank));
    }

    #endregion
}
