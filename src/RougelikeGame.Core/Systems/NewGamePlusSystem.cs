namespace RougelikeGame.Core.Systems;

/// <summary>
/// New Game+システム - 周回プレイの引き継ぎと強化
/// </summary>
public static class NewGamePlusSystem
{
    /// <summary>NG+設定</summary>
    public record NgPlusConfig(
        NewGamePlusTier Tier,
        float EnemyMultiplier,
        float ExpMultiplier,
        float DropMultiplier,
        bool UnlocksSpecialContent
    );

    private static readonly Dictionary<NewGamePlusTier, NgPlusConfig> Configs = new()
    {
        [NewGamePlusTier.Plus1] = new(NewGamePlusTier.Plus1, 1.5f, 1.2f, 1.1f, false),
        [NewGamePlusTier.Plus2] = new(NewGamePlusTier.Plus2, 2.0f, 1.5f, 1.2f, false),
        [NewGamePlusTier.Plus3] = new(NewGamePlusTier.Plus3, 2.5f, 1.8f, 1.3f, true),
        [NewGamePlusTier.Plus4] = new(NewGamePlusTier.Plus4, 3.0f, 2.0f, 1.5f, true),
        [NewGamePlusTier.Plus5] = new(NewGamePlusTier.Plus5, 4.0f, 2.5f, 2.0f, true),
    };

    /// <summary>NG+設定を取得</summary>
    public static NgPlusConfig? GetConfig(NewGamePlusTier tier)
    {
        return Configs.TryGetValue(tier, out var c) ? c : null;
    }

    /// <summary>引き継ぎ可能かチェック</summary>
    public static bool CanStartNewGamePlus(bool hasCleared, string clearRank)
    {
        return hasCleared && clearRank is "S" or "A" or "B";
    }

    /// <summary>引き継ぎ項目を取得</summary>
    public static IReadOnlyList<string> GetCarryOverItems(NewGamePlusTier tier)
    {
        var items = new List<string> { "レベル", "スキル", "図鑑データ" };
        if (tier >= NewGamePlusTier.Plus2) items.Add("装備品");
        if (tier >= NewGamePlusTier.Plus3) items.Add("ゴールド");
        if (tier >= NewGamePlusTier.Plus4) items.Add("全アイテム");
        return items;
    }

    /// <summary>NG+段階名を取得</summary>
    public static string GetTierName(NewGamePlusTier tier) => tier switch
    {
        NewGamePlusTier.Plus1 => "NG+1",
        NewGamePlusTier.Plus2 => "NG+2",
        NewGamePlusTier.Plus3 => "NG+3",
        NewGamePlusTier.Plus4 => "NG+4",
        NewGamePlusTier.Plus5 => "NG+5",
        _ => "不明"
    };
}
