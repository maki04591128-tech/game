namespace RougelikeGame.Core.Systems;

/// <summary>
/// 商人ギルド・流通ネットワークシステム - 交易路開拓、市場操作、ギルドランク経済活動
/// 参考: Recettear、Kenshi
/// </summary>
public class MerchantGuildSystem
{
    /// <summary>ギルド加入状態</summary>
    public record GuildMembership(
        string PlayerId,
        GuildRank Rank,
        int GuildPoints,
        int TradeCount,
        int TotalProfit
    );

    /// <summary>交易路定義</summary>
    public record TradeRoute(
        string RouteId,
        TerritoryId Origin,
        TerritoryId Destination,
        TradeRouteStatus Status,
        int ProfitMultiplier,
        int EstablishmentCost
    );

    /// <summary>交易結果</summary>
    public record TradeResult(
        string RouteId,
        int BaseProfit,
        int ActualProfit,
        string Description
    );

    private GuildMembership? _membership;
    private readonly List<TradeRoute> _routes = new();

    /// <summary>現在のギルド加入状態</summary>
    public GuildMembership? Membership => _membership;

    /// <summary>全交易路</summary>
    public IReadOnlyList<TradeRoute> Routes => _routes;

    /// <summary>ギルドに加入</summary>
    public GuildMembership JoinGuild(string playerId)
    {
        _membership = new GuildMembership(playerId, GuildRank.None, 0, 0, 0);
        return _membership;
    }

    /// <summary>ギルドに加入済みか確認</summary>
    public bool IsMember => _membership != null;

    /// <summary>交易路を開拓</summary>
    public TradeRoute? EstablishRoute(string routeId, TerritoryId origin, TerritoryId destination, int cost)
    {
        if (_membership == null) return null;
        if (_routes.Any(r => r.Origin == origin && r.Destination == destination)) return null;

        var route = new TradeRoute(routeId, origin, destination, TradeRouteStatus.Open, 100, cost);
        _routes.Add(route);
        return route;
    }

    /// <summary>交易を実行して利益を計算</summary>
    public TradeResult? ExecuteTrade(string routeId, int investmentAmount)
    {
        if (_membership == null) return null;
        var route = _routes.FirstOrDefault(r => r.RouteId == routeId);
        if (route == null || route.Status != TradeRouteStatus.Open && route.Status != TradeRouteStatus.Prosperous)
            return null;

        int rankBonus = GetRankBonus(_membership.Rank);
        int baseProfit = investmentAmount * route.ProfitMultiplier / 100;
        int statusBonus = route.Status == TradeRouteStatus.Prosperous ? baseProfit / 2 : 0;
        int actualProfit = baseProfit + rankBonus + statusBonus;

        _membership = _membership with
        {
            TradeCount = _membership.TradeCount + 1,
            TotalProfit = _membership.TotalProfit + actualProfit,
            GuildPoints = _membership.GuildPoints + Math.Max(1, actualProfit / 10)
        };

        CheckRankUp();

        return new TradeResult(routeId, baseProfit, actualProfit,
            $"交易成功: 基本利益{baseProfit}G + ランクボーナス{rankBonus}G + 状態ボーナス{statusBonus}G = {actualProfit}G");
    }

    /// <summary>交易路の状態を変更</summary>
    public void UpdateRouteStatus(string routeId, TradeRouteStatus newStatus)
    {
        for (int i = 0; i < _routes.Count; i++)
        {
            if (_routes[i].RouteId == routeId)
            {
                _routes[i] = _routes[i] with { Status = newStatus };
                return;
            }
        }
    }

    /// <summary>セーブデータからギルド状態を復元する</summary>
    public void RestoreFromSave(string playerId, GuildRank rank, int guildPoints, int tradeCount, int totalProfit,
        IEnumerable<TradeRoute>? routes = null)
    {
        _membership = new GuildMembership(playerId, rank, guildPoints, tradeCount, totalProfit);
        _routes.Clear();
        if (routes != null)
        {
            _routes.AddRange(routes);
        }
    }

    /// <summary>
    /// 商人ギルド状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、ギルド加入・交易路は全て消失する。
    /// </summary>
    public void Reset()
    {
        _membership = null;
        _routes.Clear();
    }

    /// <summary>ギルドランクに応じたボーナスを取得</summary>
    public static int GetRankBonus(GuildRank rank) => rank switch
    {
        GuildRank.None => 0,
        GuildRank.Copper => 5,
        GuildRank.Iron => 15,
        GuildRank.Silver => 30,
        GuildRank.Gold => 50,
        GuildRank.Platinum => 80,
        GuildRank.Mythril => 120,
        GuildRank.Adamantine => 200,
        _ => 0
    };

    /// <summary>ランクアップ判定</summary>
    /// <returns>ランクアップ報酬ゴールド（0ならランクアップなし）</returns>
    private int CheckRankUp()
    {
        if (_membership == null) return 0;
        var oldRank = _membership.Rank;
        var newRank = _membership.GuildPoints switch
        {
            >= 10000 => GuildRank.Adamantine,
            >= 5000 => GuildRank.Mythril,
            >= 2000 => GuildRank.Platinum,
            >= 1000 => GuildRank.Gold,
            >= 500 => GuildRank.Silver,
            >= 200 => GuildRank.Iron,
            >= 50 => GuildRank.Copper,
            _ => GuildRank.None
        };
        if (newRank != oldRank)
        {
            _membership = _membership with { Rank = newRank };
            // CX-1: ランクアップ報酬（ランクに応じたゴールドボーナス）
            LastRankUpReward = GetRankUpReward(newRank);
            return LastRankUpReward;
        }
        return 0;
    }

    /// <summary>CX-1: 最後のランクアップ報酬額</summary>
    public int LastRankUpReward { get; private set; }

    /// <summary>CX-1: ランクアップ報酬額を取得</summary>
    private static int GetRankUpReward(GuildRank rank) => rank switch
    {
        GuildRank.Copper => 50,
        GuildRank.Iron => 100,
        GuildRank.Silver => 200,
        GuildRank.Gold => 500,
        GuildRank.Platinum => 1000,
        GuildRank.Mythril => 2000,
        GuildRank.Adamantine => 5000,
        _ => 0
    };

    /// <summary>交易路の状態に基づく価格修正率を取得</summary>
    public float GetPriceModifier(string routeId)
    {
        var route = _routes.FirstOrDefault(r => r.RouteId == routeId);
        if (route == null) return 1.0f;

        return route.Status switch
        {
            TradeRouteStatus.Open => 1.0f,
            TradeRouteStatus.Prosperous => 1.2f,   // 好景気: 売値20%UP
            TradeRouteStatus.Closed => 0.7f,        // 閉鎖: 売値30%DOWN
            TradeRouteStatus.Blocked => 0.5f,       // 封鎖: 売値50%DOWN
            _ => 1.0f
        };
    }

    /// <summary>ギルドランクに基づくショップ割引率を取得</summary>
    public float GetGuildDiscount()
    {
        if (_membership == null) return 0f;
        return _membership.Rank switch
        {
            GuildRank.Copper => 0.02f,
            GuildRank.Iron => 0.05f,
            GuildRank.Silver => 0.10f,
            GuildRank.Gold => 0.15f,
            GuildRank.Platinum => 0.20f,
            GuildRank.Mythril => 0.25f,
            GuildRank.Adamantine => 0.30f,
            _ => 0f
        };
    }
}
