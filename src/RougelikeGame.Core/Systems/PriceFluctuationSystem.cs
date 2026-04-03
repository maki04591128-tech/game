namespace RougelikeGame.Core.Systems;

/// <summary>
/// 物品価値変動システム - 領地/商人種別/需給/カルマ/評判で売買価格が動的変動
/// </summary>
public static class PriceFluctuationSystem
{
    /// <summary>需給係数を計算（0.5〜2.0）</summary>
    public static float CalculateSupplyDemandModifier(int supply, int demand)
    {
        if (supply <= 0) return 2.0f;
        if (demand <= 0) return 0.5f;
        float ratio = (float)demand / supply;
        return Math.Clamp(ratio, 0.5f, 2.0f);
    }

    /// <summary>領地ごとの基本価格係数（特産品は安い/希少品は高い）</summary>
    public static float GetTerritoryModifier(TerritoryId territory, string itemCategory) => (territory, itemCategory) switch
    {
        (TerritoryId.Capital, "weapon") => 0.9f,
        (TerritoryId.Capital, "armor") => 0.9f,
        (TerritoryId.Capital, "potion") => 1.0f,
        (TerritoryId.Forest, "herb") => 0.7f,
        (TerritoryId.Forest, "wood") => 0.7f,
        (TerritoryId.Mountain, "ore") => 0.7f,
        (TerritoryId.Mountain, "gem") => 0.8f,
        (TerritoryId.Coast, "fish") => 0.7f,
        (TerritoryId.Southern, "gem") => 0.8f,
        (TerritoryId.Frontier, _) => 1.2f,
        _ => 1.0f
    };

    /// <summary>カルマによる売買価格補正</summary>
    public static float GetKarmaModifier(KarmaRank karmaRank, bool isBuying) => (karmaRank, isBuying) switch
    {
        (KarmaRank.Saint, true) => 0.85f,
        (KarmaRank.Virtuous, true) => 0.9f,
        (KarmaRank.Villain, true) => 1.2f,
        (KarmaRank.Criminal, true) => 1.15f,
        (KarmaRank.Saint, false) => 1.1f,
        (KarmaRank.Villain, false) => 0.8f,
        _ => 1.0f
    };

    /// <summary>評判による売買価格補正</summary>
    public static float GetReputationModifier(ReputationRank reputationRank, bool isBuying) => (reputationRank, isBuying) switch
    {
        (ReputationRank.Revered, true) => 0.8f,
        (ReputationRank.Trusted, true) => 0.85f,
        (ReputationRank.Friendly, true) => 0.9f,
        (ReputationRank.Hostile, true) => 1.15f,
        (ReputationRank.Hated, true) => 1.3f,
        (ReputationRank.Revered, false) => 1.15f,
        (ReputationRank.Hated, false) => 0.7f,
        _ => 1.0f
    };

    /// <summary>最終価格を計算</summary>
    public static int CalculateFinalPrice(
        int basePrice,
        float supplyDemandModifier,
        float territoryModifier,
        float karmaModifier,
        float reputationModifier,
        bool isBuying)
    {
        float total = basePrice * supplyDemandModifier * territoryModifier * karmaModifier * reputationModifier;
        int result = (int)Math.Round(total);
        // 売却時は購入価格の70%（店のマージン30%）
        return isBuying ? Math.Max(1, result) : Math.Max(1, (int)(result * 0.7f));
    }
}
