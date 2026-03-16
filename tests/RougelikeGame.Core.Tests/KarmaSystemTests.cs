using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class KarmaSystemTests
{
    #region 初期化テスト

    [Fact]
    public void Constructor_InitializesKarmaToZero()
    {
        var system = new KarmaSystem();
        Assert.Equal(0, system.KarmaValue);
        Assert.Equal(KarmaRank.Neutral, system.CurrentRank);
    }

    [Fact]
    public void Constructor_EmptyHistory()
    {
        var system = new KarmaSystem();
        Assert.Empty(system.KarmaHistory);
    }

    #endregion

    #region カルマ変動テスト

    [Fact]
    public void ModifyKarma_PositiveAmount_IncreasesKarma()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(30, "善行");

        Assert.Equal(30, system.KarmaValue);
    }

    [Fact]
    public void ModifyKarma_NegativeAmount_DecreasesKarma()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(-40, "悪行");

        Assert.Equal(-40, system.KarmaValue);
    }

    [Fact]
    public void ModifyKarma_ClampsToMax100()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(200, "大善行");

        Assert.Equal(100, system.KarmaValue);
    }

    [Fact]
    public void ModifyKarma_ClampsToMinMinus100()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(-200, "大悪行");

        Assert.Equal(-100, system.KarmaValue);
    }

    [Fact]
    public void ModifyKarma_RecordsHistory()
    {
        var system = new KarmaSystem();
        system.SetCurrentTurn(5);
        system.ModifyKarma(10, "善い行い");

        Assert.Single(system.KarmaHistory);
        var evt = system.KarmaHistory[0];
        Assert.Equal(0, evt.OldValue);
        Assert.Equal(10, evt.NewValue);
        Assert.Equal("善い行い", evt.Reason);
        Assert.Equal(5, evt.TurnNumber);
    }

    [Fact]
    public void ModifyKarma_MultipleChanges_RecordsAll()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(10, "理由1");
        system.ModifyKarma(-5, "理由2");
        system.ModifyKarma(20, "理由3");

        Assert.Equal(3, system.KarmaHistory.Count);
        Assert.Equal(25, system.KarmaValue);
    }

    #endregion

    #region カルマ段階判定テスト

    [Theory]
    [InlineData(100, KarmaRank.Saint)]
    [InlineData(80, KarmaRank.Saint)]
    [InlineData(79, KarmaRank.Virtuous)]
    [InlineData(50, KarmaRank.Virtuous)]
    [InlineData(49, KarmaRank.Normal)]
    [InlineData(20, KarmaRank.Normal)]
    [InlineData(19, KarmaRank.Neutral)]
    [InlineData(0, KarmaRank.Neutral)]
    [InlineData(-19, KarmaRank.Neutral)]
    [InlineData(-20, KarmaRank.Rogue)]
    [InlineData(-49, KarmaRank.Rogue)]
    [InlineData(-50, KarmaRank.Criminal)]
    [InlineData(-79, KarmaRank.Criminal)]
    [InlineData(-80, KarmaRank.Villain)]
    [InlineData(-100, KarmaRank.Villain)]
    public void CurrentRank_ReturnsCorrectRank(int karma, KarmaRank expected)
    {
        var system = new KarmaSystem();
        system.ModifyKarma(karma, "テスト");
        Assert.Equal(expected, system.CurrentRank);
    }

    #endregion

    #region ショップ価格修正テスト

    [Theory]
    [InlineData(100, 0.8)]
    [InlineData(50, 0.9)]
    [InlineData(20, 1.0)]
    [InlineData(0, 1.0)]
    [InlineData(-20, 1.1)]
    [InlineData(-50, 1.3)]
    [InlineData(-80, 1.5)]
    public void GetShopPriceModifier_ReturnsCorrectModifier(int karma, double expected)
    {
        var system = new KarmaSystem();
        system.ModifyKarma(karma, "テスト");
        Assert.Equal(expected, system.GetShopPriceModifier(), 2);
    }

    #endregion

    #region NPC態度修正テスト

    [Fact]
    public void GetNpcDispositionModifier_Saint_Returns1_5()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(100, "聖人");
        Assert.Equal(1.5, system.GetNpcDispositionModifier(), 2);
    }

    [Fact]
    public void GetNpcDispositionModifier_Villain_Returns0_1()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(-100, "外道");
        Assert.Equal(0.1, system.GetNpcDispositionModifier(), 2);
    }

    #endregion

    #region 闇市・聖域アクセステスト

    [Theory]
    [InlineData(-20, true)]
    [InlineData(-50, true)]
    [InlineData(-100, true)]
    [InlineData(-19, false)]
    [InlineData(0, false)]
    [InlineData(50, false)]
    public void CanAccessDarkMarket_ReturnsCorrectly(int karma, bool expected)
    {
        var system = new KarmaSystem();
        system.ModifyKarma(karma, "テスト");
        Assert.Equal(expected, system.CanAccessDarkMarket());
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-49, true)]
    [InlineData(-50, false)]
    [InlineData(-80, false)]
    [InlineData(50, true)]
    public void CanEnterHolyGround_ReturnsCorrectly(int karma, bool expected)
    {
        var system = new KarmaSystem();
        system.ModifyKarma(karma, "テスト");
        Assert.Equal(expected, system.CanEnterHolyGround());
    }

    #endregion

    #region 日本語名テスト

    [Theory]
    [InlineData(KarmaRank.Saint, "聖人")]
    [InlineData(KarmaRank.Virtuous, "善人")]
    [InlineData(KarmaRank.Normal, "普通")]
    [InlineData(KarmaRank.Neutral, "中立")]
    [InlineData(KarmaRank.Rogue, "悪漢")]
    [InlineData(KarmaRank.Criminal, "悪党")]
    [InlineData(KarmaRank.Villain, "外道")]
    public void GetKarmaRankName_ReturnsJapaneseName(KarmaRank rank, string expected)
    {
        Assert.Equal(expected, KarmaSystem.GetKarmaRankName(rank));
    }

    #endregion
}
