namespace RougelikeGame.Core.Systems;

/// <summary>
/// シンボルマップランダムイベント拡張システム
/// </summary>
public static class SymbolMapEventSystem
{
    /// <summary>イベント定義</summary>
    public record MapEvent(
        string Id,
        string Name,
        string Description,
        float BaseChance,
        Season[] ActiveSeasons,
        TerritoryId[] ActiveTerritories
    );

    private static readonly List<MapEvent> Events = new()
    {
        new("event_merchant_caravan", "行商キャラバン", "旅の商人との遭遇。珍しい品を取り扱う",
            0.15f, new[] { Season.Spring, Season.Summer, Season.Autumn }, Array.Empty<TerritoryId>()),
        new("event_bandit_ambush", "山賊の待ち伏せ", "山賊に襲われる戦闘イベント",
            0.1f, Array.Empty<Season>(), new[] { TerritoryId.Frontier, TerritoryId.Mountain }),
        new("event_wandering_healer", "放浪の治療師", "HP/状態異常を無料で回復してくれる",
            0.08f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
        new("event_ancient_shrine", "古代の祠", "祈ると一時的なバフを得られる",
            0.05f, Array.Empty<Season>(), new[] { TerritoryId.Forest, TerritoryId.Mountain }),
        new("event_treasure_map", "宝の地図", "近くのダンジョンに隠し宝箱が追加される",
            0.03f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
        new("event_monster_stampede", "魔物の大移動", "強力な敵集団との遭遇",
            0.05f, new[] { Season.Autumn, Season.Winter }, Array.Empty<TerritoryId>()),
        new("event_fallen_star", "流れ星", "レアアイテムが出現するポイントが生成",
            0.02f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
        new("event_refugee", "避難民", "助けるとカルマ上昇。情報提供",
            0.08f, Array.Empty<Season>(), new[] { TerritoryId.Frontier, TerritoryId.Capital }),
        new("event_storm_shelter", "嵐の避難所", "天候回復まで休憩可能な洞窟",
            0.1f, new[] { Season.Winter }, new[] { TerritoryId.Mountain }),
        new("event_fairy_ring", "妖精の輪", "MPが完全回復する不思議な場所",
            0.03f, new[] { Season.Spring }, new[] { TerritoryId.Forest }),
    };

    /// <summary>全イベント定義を取得</summary>
    public static IReadOnlyList<MapEvent> GetAllEvents() => Events;

    /// <summary>現在の条件で発生可能なイベントを取得</summary>
    public static IReadOnlyList<MapEvent> GetAvailableEvents(Season season, TerritoryId territory)
    {
        return Events.Where(e =>
            (e.ActiveSeasons.Length == 0 || e.ActiveSeasons.Contains(season)) &&
            (e.ActiveTerritories.Length == 0 || e.ActiveTerritories.Contains(territory))
        ).ToList();
    }

    /// <summary>イベント発生判定</summary>
    public static MapEvent? RollEvent(Season season, TerritoryId territory, double randomValue)
    {
        var available = GetAvailableEvents(season, territory);
        double cumulative = 0;
        foreach (var evt in available)
        {
            cumulative += evt.BaseChance;
            if (randomValue < cumulative)
                return evt;
        }
        return null; // イベントなし
    }
}
