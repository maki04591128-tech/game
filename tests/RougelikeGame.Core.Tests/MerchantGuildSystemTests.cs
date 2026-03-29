using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// MerchantGuildSystem（商人ギルド・流通ネットワークシステム）のテスト
/// </summary>
public class MerchantGuildSystemTests
{
    // --- コンストラクタ ---

    [Fact]
    public void Constructor_NoMembership()
    {
        var system = new MerchantGuildSystem();
        Assert.Null(system.Membership);
        Assert.False(system.IsMember);
        Assert.Empty(system.Routes);
    }

    // --- JoinGuild ---

    [Fact]
    public void JoinGuild_CreatesMembership()
    {
        var system = new MerchantGuildSystem();
        var membership = system.JoinGuild("player_1");
        Assert.NotNull(membership);
        Assert.Equal("player_1", membership.PlayerId);
        Assert.Equal(GuildRank.None, membership.Rank);
        Assert.True(system.IsMember);
    }

    // --- EstablishRoute ---

    [Fact]
    public void EstablishRoute_WithMembership_CreatesRoute()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        var route = system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.NotNull(route);
        Assert.Equal("route_1", route.RouteId);
        Assert.Equal(TradeRouteStatus.Open, route.Status);
        Assert.Single(system.Routes);
    }

    [Fact]
    public void EstablishRoute_WithoutMembership_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        var route = system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.Null(route);
    }

    [Fact]
    public void EstablishRoute_DuplicateOriginDestination_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        var dup = system.EstablishRoute("route_2", TerritoryId.Capital, TerritoryId.Forest, 200);
        Assert.Null(dup);
    }

    // --- ExecuteTrade ---

    [Fact]
    public void ExecuteTrade_ValidRoute_ReturnsProfit()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ExecuteTrade("route_1", 1000);
        Assert.NotNull(result);
        Assert.True(result.ActualProfit > 0);
    }

    [Fact]
    public void ExecuteTrade_WithoutMembership_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        var result = system.ExecuteTrade("route_1", 1000);
        Assert.Null(result);
    }

    [Fact]
    public void ExecuteTrade_NonExistentRoute_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        var result = system.ExecuteTrade("missing_route", 1000);
        Assert.Null(result);
    }

    [Fact]
    public void ExecuteTrade_ClosedRoute_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.UpdateRouteStatus("route_1", TradeRouteStatus.Closed);
        var result = system.ExecuteTrade("route_1", 1000);
        Assert.Null(result);
    }

    [Fact]
    public void ExecuteTrade_ProsperousRoute_IncreasesProfit()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_a", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.EstablishRoute("route_b", TerritoryId.Forest, TerritoryId.Capital, 100);
        system.UpdateRouteStatus("route_b", TradeRouteStatus.Prosperous);

        var normalResult = system.ExecuteTrade("route_a", 1000);
        var prosperousResult = system.ExecuteTrade("route_b", 1000);
        Assert.NotNull(normalResult);
        Assert.NotNull(prosperousResult);
        Assert.True(prosperousResult.ActualProfit > normalResult.ActualProfit);
    }

    [Fact]
    public void ExecuteTrade_UpdatesTradeCountAndProfit()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.ExecuteTrade("route_1", 500);
        Assert.Equal(1, system.Membership!.TradeCount);
        Assert.True(system.Membership.TotalProfit > 0);
    }

    // --- UpdateRouteStatus ---

    [Fact]
    public void UpdateRouteStatus_ChangesStatus()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.UpdateRouteStatus("route_1", TradeRouteStatus.Blocked);
        Assert.Equal(TradeRouteStatus.Blocked, system.Routes[0].Status);
    }

    // --- GetRankBonus ---

    [Theory]
    [InlineData(GuildRank.None, 0)]
    [InlineData(GuildRank.Copper, 5)]
    [InlineData(GuildRank.Gold, 50)]
    [InlineData(GuildRank.Adamantine, 200)]
    public void GetRankBonus_ReturnsExpectedBonus(GuildRank rank, int expected)
    {
        Assert.Equal(expected, MerchantGuildSystem.GetRankBonus(rank));
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsAllState()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.Reset();
        Assert.Null(system.Membership);
        Assert.False(system.IsMember);
        Assert.Empty(system.Routes);
    }

    // --- ランクアップ判定 ---

    [Fact]
    public void ExecuteTrade_AccumulatesGuildPoints_RanksUp()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player_1");
        system.EstablishRoute("route_1", TerritoryId.Capital, TerritoryId.Forest, 100);
        // 大量投資で大きな利益 → ポイント蓄積 → ランクアップ
        for (int i = 0; i < 20; i++)
        {
            system.ExecuteTrade("route_1", 10000);
        }
        Assert.NotEqual(GuildRank.None, system.Membership!.Rank);
    }
}
