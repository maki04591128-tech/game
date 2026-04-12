using RougelikeGame.Core.Map;

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
        new("event_sandstorm", "砂嵐", "視界が極端に悪化。迷いやすくなる",
            0.1f, new[] { Season.Summer }, new[] { TerritoryId.Desert }),
        new("event_swamp_miasma", "瘴気の濃霧", "毒の霧に包まれる。毒耐性がないと危険",
            0.08f, Array.Empty<Season>(), new[] { TerritoryId.Swamp }),
        new("event_blizzard", "猛吹雪", "凍傷の危険。移動コストが大幅増加",
            0.1f, new[] { Season.Winter }, new[] { TerritoryId.Tundra }),
        new("event_lake_mist", "湖上の幻霧", "幻惑効果。方向感覚を失う",
            0.06f, new[] { Season.Autumn }, new[] { TerritoryId.Lake }),
        new("event_eruption", "火山噴火", "溶岩弾が降り注ぐ。素早く退避が必要",
            0.05f, Array.Empty<Season>(), new[] { TerritoryId.Volcanic }),
        new("event_divine_light", "神聖な光", "聖なる力が降り注ぎ、HP全回復",
            0.03f, Array.Empty<Season>(), new[] { TerritoryId.Sacred }),
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

    /// <summary>
    /// バイオーム固有タイルに踏み入った際の環境イベントを返す。
    /// 確率的にダメージやバフ/デバフを適用するイベントを生成する。
    /// </summary>
    public static MapEvent? GetTerrainEvent(TileType terrainType, double randomValue)
    {
        return terrainType switch
        {
            TileType.SymbolDune when randomValue < 0.08 =>
                new MapEvent("terrain_dune_heat", "灼熱の砂丘", "焼けるような砂に体力を奪われる", 0.08f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
            TileType.SymbolLava when randomValue < 0.12 =>
                new MapEvent("terrain_lava_eruption", "溶岩噴出", "足元から溶岩が噴き出す！", 0.12f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
            TileType.SymbolIce when randomValue < 0.10 =>
                new MapEvent("terrain_ice_slip", "氷上滑走", "滑りやすい氷の上で足を取られる", 0.10f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
            TileType.SymbolSwamp when randomValue < 0.10 =>
                new MapEvent("terrain_swamp_poison", "毒沼", "有毒な瘴気に包まれる", 0.10f, Array.Empty<Season>(), Array.Empty<TerritoryId>()),
            _ => null
        };
    }
}
