namespace RougelikeGame.Core.Systems;

/// <summary>
/// 闇市場・裏社会ネットワーク - カルマ依存の裏取引
/// </summary>
public static class BlackMarketSystem
{
    /// <summary>闇市場商品</summary>
    public record BlackMarketItem(
        string Name,
        BlackMarketCategory Category,
        int Price,
        int RequiredKarma,
        float DetectionRisk
    );

    private static readonly List<BlackMarketItem> Items = new()
    {
        new("盗まれた聖杯", BlackMarketCategory.StolenGoods, 500, -20, 0.3f),
        new("禁忌の魔導書", BlackMarketCategory.ForbiddenItems, 1000, -40, 0.5f),
        new("暗殺用毒針", BlackMarketCategory.AssassinTools, 300, -30, 0.4f),
        new("密偵の報告書", BlackMarketCategory.Information, 200, -10, 0.1f),
        new("呪いの護符", BlackMarketCategory.ForbiddenItems, 800, -50, 0.6f),
    };

    /// <summary>アクセス可能か判定（カルマ閾値）</summary>
    public static bool CanAccess(int karma) => karma <= -20;

    /// <summary>購入可能な商品を取得</summary>
    public static IReadOnlyList<BlackMarketItem> GetAvailableItems(int karma)
    {
        if (!CanAccess(karma)) return Array.Empty<BlackMarketItem>();
        return Items.Where(i => karma <= i.RequiredKarma).ToList();
    }

    /// <summary>全商品を取得</summary>
    public static IReadOnlyList<BlackMarketItem> GetAllItems() => Items;

    /// <summary>発覚リスクを計算（DEX/LUKで軽減）</summary>
    public static float CalculateDetectionRisk(float baseRisk, int dexterity, int luck)
    {
        float reduction = (dexterity + luck) * 0.005f;
        return Math.Clamp(baseRisk - reduction, 0.05f, 1.0f);
    }

    /// <summary>カテゴリ名を取得</summary>
    public static string GetCategoryName(BlackMarketCategory category) => category switch
    {
        BlackMarketCategory.StolenGoods => "盗品",
        BlackMarketCategory.ForbiddenItems => "禁忌アイテム",
        BlackMarketCategory.AssassinTools => "暗殺道具",
        BlackMarketCategory.Information => "情報",
        _ => "不明"
    };
}
