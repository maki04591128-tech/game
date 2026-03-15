using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// ロケーション種別
/// </summary>
public enum LocationType
{
    Town,
    Village,
    Facility,
    ReligiousSite,
    Field,
    Dungeon
}

/// <summary>
/// ロケーション定義
/// </summary>
public record LocationDefinition(
    string Id,
    string Name,
    string Description,
    LocationType Type,
    TerritoryId Territory,
    int MinLevel = 1,
    int DangerLevel = 1)
{
    private static readonly Dictionary<string, LocationDefinition> All = new()
    {
        // 王都領
        ["capital_castle"] = new("capital_castle", "王城", "国王への謁見、メインクエスト関連", LocationType.Facility, TerritoryId.Capital),
        ["capital_guild"] = new("capital_guild", "冒険者ギルド本部", "クエスト受注、ランク昇格試験", LocationType.Facility, TerritoryId.Capital),
        ["capital_academy"] = new("capital_academy", "賢者院", "魔法言語の学習、魔法書購入", LocationType.Facility, TerritoryId.Capital),
        ["capital_cathedral"] = new("capital_cathedral", "大聖堂", "光の神殿の総本山", LocationType.ReligiousSite, TerritoryId.Capital),
        ["capital_market"] = new("capital_market", "中央市場", "最も品揃えが良い市場", LocationType.Town, TerritoryId.Capital),
        ["capital_arena"] = new("capital_arena", "闘技場", "戦闘訓練、賞金稼ぎ", LocationType.Facility, TerritoryId.Capital),
        ["capital_slum"] = new("capital_slum", "貧民街", "情報収集、闇市場、盗賊ギルド", LocationType.Field, TerritoryId.Capital, DangerLevel: 2),
        ["capital_catacombs"] = new("capital_catacombs", "王都地下墓地", "中難易度、アンデッド多数", LocationType.Dungeon, TerritoryId.Capital, MinLevel: 3, DangerLevel: 3),
        ["capital_rift"] = new("capital_rift", "始まりの裂け目", "高難易度、最初に出現したダンジョン", LocationType.Dungeon, TerritoryId.Capital, MinLevel: 8, DangerLevel: 4),

        // 森林領
        ["forest_city"] = new("forest_city", "緑樹の都", "エルフの首都、樹上都市", LocationType.Town, TerritoryId.Forest),
        ["forest_herbalist"] = new("forest_herbalist", "薬師の村", "薬草・ポーション販売", LocationType.Village, TerritoryId.Forest),
        ["forest_worldtree"] = new("forest_worldtree", "世界樹の根元", "自然崇拝の聖地", LocationType.ReligiousSite, TerritoryId.Forest),
        ["forest_deep"] = new("forest_deep", "深緑の森", "迷いやすい、幻惑効果", LocationType.Field, TerritoryId.Forest, DangerLevel: 3),
        ["forest_spring"] = new("forest_spring", "精霊の泉", "回復スポット、精霊との交信", LocationType.Field, TerritoryId.Forest),
        ["forest_corruption"] = new("forest_corruption", "腐敗の森", "高難易度、汚染された区域", LocationType.Dungeon, TerritoryId.Forest, MinLevel: 10, DangerLevel: 4),
        ["forest_ruins"] = new("forest_ruins", "古代エルフの遺跡", "高難易度、古代魔法の罠", LocationType.Dungeon, TerritoryId.Forest, MinLevel: 12, DangerLevel: 5),

        // 山岳領
        ["mountain_fortress"] = new("mountain_fortress", "鉄床城", "ドワーフの首都、最高の鍛冶屋", LocationType.Town, TerritoryId.Mountain),
        ["mountain_miner"] = new("mountain_miner", "坑夫の村", "鉱山入口、採掘依頼", LocationType.Village, TerritoryId.Mountain),
        ["mountain_shrine"] = new("mountain_shrine", "山頂の祠", "死神信仰の隠れ聖地", LocationType.ReligiousSite, TerritoryId.Mountain),
        ["mountain_road"] = new("mountain_road", "山岳街道", "険しい道、落石注意", LocationType.Field, TerritoryId.Mountain, DangerLevel: 3),
        ["mountain_mine"] = new("mountain_mine", "採掘坑", "中難易度、資源収集", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 10, DangerLevel: 3),
        ["mountain_lava"] = new("mountain_lava", "溶岩洞", "高難易度、火属性の敵", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 15, DangerLevel: 4),
        ["mountain_dragon"] = new("mountain_dragon", "竜の巣", "最高難易度、ドラゴン", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 25, DangerLevel: 5),

        // 海岸領
        ["coast_port"] = new("coast_port", "港町マリーナ", "海運、船の購入、海外商品", LocationType.Town, TerritoryId.Coast),
        ["coast_vineyard"] = new("coast_vineyard", "ぶどう畑の村", "ワイン生産、のどかな農村", LocationType.Village, TerritoryId.Coast),
        ["coast_field"] = new("coast_field", "麦畑地帯", "平和だが時折盗賊出没", LocationType.Field, TerritoryId.Coast, DangerLevel: 1),
        ["coast_cave"] = new("coast_cave", "海岸洞窟", "低難易度、海賊の隠れ家", LocationType.Dungeon, TerritoryId.Coast, MinLevel: 5, DangerLevel: 2),
        ["coast_wreck"] = new("coast_wreck", "沈没船", "中難易度、水棲モンスター", LocationType.Dungeon, TerritoryId.Coast, MinLevel: 8, DangerLevel: 3),
        ["coast_darkchapel"] = new("coast_darkchapel", "闇の礼拝堂", "闇の教団の拠点", LocationType.ReligiousSite, TerritoryId.Coast, MinLevel: 10, DangerLevel: 3),

        // 南部領
        ["southern_castle"] = new("southern_castle", "サンライト城", "領主の城、華やかな貴族社会", LocationType.Town, TerritoryId.Southern),
        ["southern_village"] = new("southern_village", "雪原の村", "休息、補給、情報収集", LocationType.Village, TerritoryId.Southern),
        ["southern_hunter"] = new("southern_hunter", "狩人の集落", "獣人が住む、特殊装備入手", LocationType.Village, TerritoryId.Southern),
        ["southern_plain"] = new("southern_plain", "凍てつく平原", "広大な雪原、野生動物", LocationType.Field, TerritoryId.Southern, DangerLevel: 3),
        ["southern_icecave"] = new("southern_icecave", "氷の洞窟", "中難易度、氷属性の敵", LocationType.Dungeon, TerritoryId.Southern, MinLevel: 15, DangerLevel: 3),
        ["southern_battlefield"] = new("southern_battlefield", "古戦場跡", "中難易度、アンデッド兵士", LocationType.Dungeon, TerritoryId.Southern, MinLevel: 15, DangerLevel: 3),

        // 辺境領
        ["frontier_fort"] = new("frontier_fort", "辺境砦", "かろうじて秩序が保たれた拠点", LocationType.Town, TerritoryId.Frontier),
        ["frontier_outlaw"] = new("frontier_outlaw", "ならず者の集落", "無法者の溜まり場、闇市場", LocationType.Village, TerritoryId.Frontier),
        ["frontier_ruins_city"] = new("frontier_ruins_city", "廃墟の街", "かつての都市の残骸", LocationType.Field, TerritoryId.Frontier, DangerLevel: 4),
        ["frontier_wasteland"] = new("frontier_wasteland", "荒野", "魔物が徘徊する危険地帯", LocationType.Field, TerritoryId.Frontier, DangerLevel: 4),
        ["frontier_chaos_altar"] = new("frontier_chaos_altar", "混沌の祭壇", "混沌の崇拝の中心地", LocationType.ReligiousSite, TerritoryId.Frontier, MinLevel: 20, DangerLevel: 4),
        ["frontier_great_rift"] = new("frontier_great_rift", "大裂け目", "最高難易度、最終ダンジョン候補", LocationType.Dungeon, TerritoryId.Frontier, MinLevel: 25, DangerLevel: 5),
        ["frontier_ancient_ruins"] = new("frontier_ancient_ruins", "滅びた王国の遺跡", "高難易度、古代文明の秘密", LocationType.Dungeon, TerritoryId.Frontier, MinLevel: 20, DangerLevel: 5)
    };

    public static LocationDefinition Get(string id) => All[id];
    public static IReadOnlyDictionary<string, LocationDefinition> GetAll() => All;

    public static IReadOnlyList<LocationDefinition> GetByTerritory(TerritoryId territory) =>
        All.Values.Where(l => l.Territory == territory).ToList();

    public static IReadOnlyList<LocationDefinition> GetDungeonsByTerritory(TerritoryId territory) =>
        All.Values.Where(l => l.Territory == territory && l.Type == LocationType.Dungeon).ToList();
}

/// <summary>
/// 領地定義
/// </summary>
public record TerritoryDefinition(
    TerritoryId Id,
    string Name,
    string Description,
    int MinLevel,
    int MaxDungeonDepth,
    TerritoryId[] AdjacentTerritories,
    int TravelTurnCost,
    FacilityType[] AvailableFacilities)
{
    private static readonly Dictionary<TerritoryId, TerritoryDefinition> All = new()
    {
        [TerritoryId.Capital] = new(TerritoryId.Capital, "王都領", "王国の中心。大都市と大聖堂がある", 1, 10,
            new[] { TerritoryId.Forest, TerritoryId.Mountain, TerritoryId.Coast },
            0,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.WeaponShop, FacilityType.ArmorShop, FacilityType.Inn, FacilityType.Smithy, FacilityType.Church, FacilityType.Temple, FacilityType.MagicShop, FacilityType.Library, FacilityType.Bank, FacilityType.Arena }),

        [TerritoryId.Forest] = new(TerritoryId.Forest, "森林領", "深い森に覆われた自然豊かな領地", 5, 15,
            new[] { TerritoryId.Capital, TerritoryId.Southern },
            TimeConstants.TurnsPerDay * 3,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Mountain] = new(TerritoryId.Mountain, "山岳領", "険しい山々と鉱脈が広がる領地", 10, 20,
            new[] { TerritoryId.Capital, TerritoryId.Frontier },
            TimeConstants.TurnsPerDay * 5,
            new[] { FacilityType.AdventurerGuild, FacilityType.WeaponShop, FacilityType.Smithy, FacilityType.Inn }),

        [TerritoryId.Coast] = new(TerritoryId.Coast, "海岸領", "港湾都市と漁村が点在する沿岸地帯", 5, 15,
            new[] { TerritoryId.Capital, TerritoryId.Southern },
            TimeConstants.TurnsPerDay * 2,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.ArmorShop, FacilityType.Inn, FacilityType.Bank }),

        [TerritoryId.Southern] = new(TerritoryId.Southern, "南部領", "荒野と古代遺跡が広がる辺境", 15, 25,
            new[] { TerritoryId.Forest, TerritoryId.Coast, TerritoryId.Frontier },
            TimeConstants.TurnsPerDay * 4,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Frontier] = new(TerritoryId.Frontier, "辺境領", "未開の地。危険だが宝物も眠る", 20, 30,
            new[] { TerritoryId.Mountain, TerritoryId.Southern },
            TimeConstants.TurnsPerDay * 7,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop })
    };

    public static TerritoryDefinition Get(TerritoryId id) => All[id];
    public static IReadOnlyDictionary<TerritoryId, TerritoryDefinition> GetAll() => All;
}

/// <summary>
/// 世界マップシステム - 領地間移動と全体マップ管理
/// </summary>
public class WorldMapSystem
{
    /// <summary>現在の領地</summary>
    public TerritoryId CurrentTerritory { get; private set; } = TerritoryId.Capital;

    /// <summary>訪問済み領地</summary>
    public HashSet<TerritoryId> VisitedTerritories { get; } = new() { TerritoryId.Capital };

    /// <summary>現在地上にいるか（街/フィールド）</summary>
    public bool IsOnSurface { get; set; } = true;

    /// <summary>現在の領地情報を取得</summary>
    public TerritoryDefinition GetCurrentTerritoryInfo() => TerritoryDefinition.Get(CurrentTerritory);

    /// <summary>現在の領地のロケーション一覧を取得</summary>
    public IReadOnlyList<LocationDefinition> GetCurrentLocations() =>
        LocationDefinition.GetByTerritory(CurrentTerritory);

    /// <summary>現在の領地のダンジョン一覧を取得</summary>
    public IReadOnlyList<LocationDefinition> GetCurrentDungeons() =>
        LocationDefinition.GetDungeonsByTerritory(CurrentTerritory);

    /// <summary>隣接領地の一覧を取得</summary>
    public IReadOnlyList<TerritoryDefinition> GetAdjacentTerritories()
    {
        var current = TerritoryDefinition.Get(CurrentTerritory);
        return current.AdjacentTerritories.Select(TerritoryDefinition.Get).ToList();
    }

    /// <summary>隣接領地への移動が可能か</summary>
    public bool CanTravelTo(TerritoryId destination, int playerLevel)
    {
        var current = TerritoryDefinition.Get(CurrentTerritory);
        if (!current.AdjacentTerritories.Contains(destination))
            return false;

        var dest = TerritoryDefinition.Get(destination);
        return playerLevel >= dest.MinLevel;
    }

    /// <summary>領地間移動を実行</summary>
    public TravelResult TravelTo(TerritoryId destination, int playerLevel)
    {
        if (!CanTravelTo(destination, playerLevel))
        {
            var dest = TerritoryDefinition.Get(destination);
            if (playerLevel < dest.MinLevel)
                return new TravelResult(false, $"レベルが足りない（必要Lv.{dest.MinLevel}）", 0);
            return new TravelResult(false, "この領地には直接移動できない", 0);
        }

        var destination_def = TerritoryDefinition.Get(destination);
        CurrentTerritory = destination;
        VisitedTerritories.Add(destination);
        IsOnSurface = true;

        return new TravelResult(true,
            $"{destination_def.Name}に到着した（{destination_def.TravelTurnCost / TimeConstants.TurnsPerDay}日経過）",
            destination_def.TravelTurnCost);
    }

    /// <summary>領地を直接設定する（セーブデータ復元用）</summary>
    public void SetTerritory(TerritoryId territory, HashSet<TerritoryId>? visited = null)
    {
        CurrentTerritory = territory;
        if (visited != null)
        {
            VisitedTerritories.Clear();
            foreach (var t in visited)
                VisitedTerritories.Add(t);
        }
        if (!VisitedTerritories.Contains(territory))
            VisitedTerritories.Add(territory);
    }

    /// <summary>領地間移動時のランダムイベント判定</summary>
    public TravelEvent? RollTravelEvent(TerritoryId from, TerritoryId to, IRandomProvider random)
    {
        // 30%の確率でイベント発生
        if (random.Next(100) >= 30) return null;

        var events = new[]
        {
            new TravelEvent("商人との遭遇", "旅の商人と出会った。アイテムを取引できる", TravelEventType.Merchant),
            new TravelEvent("野盗の襲撃", "野盗に襲われた！戦闘になる", TravelEventType.Ambush),
            new TravelEvent("旅人の助け", "困っている旅人を見つけた。助けると報酬がもらえる", TravelEventType.HelpRequest),
            new TravelEvent("隠れた祠", "道の脇に小さな祠を見つけた。祈るとHPが回復する", TravelEventType.Shrine),
            new TravelEvent("天候悪化", "天候が悪化した。移動に追加ターンがかかる", TravelEventType.BadWeather),
            new TravelEvent("宝箱発見", "道の脇に宝箱が置かれている。罠かもしれない", TravelEventType.TreasureChest)
        };

        return events[random.Next(events.Length)];
    }

    /// <summary>領地間移動時のランダムイベント判定（System.Random版）</summary>
    public TravelEvent? RollTravelEvent(TerritoryId from, TerritoryId to, Random random)
    {
        if (random.Next(100) >= 30) return null;

        var events = new[]
        {
            new TravelEvent("商人との遭遇", "旅の商人と出会った。アイテムを取引できる", TravelEventType.Merchant),
            new TravelEvent("野盗の襲撃", "野盗に襲われた！戦闘になる", TravelEventType.Ambush),
            new TravelEvent("旅人の助け", "困っている旅人を見つけた。助けると報酬がもらえる", TravelEventType.HelpRequest),
            new TravelEvent("隠れた祠", "道の脇に小さな祠を見つけた。祈るとHPが回復する", TravelEventType.Shrine),
            new TravelEvent("天候悪化", "天候が悪化した。移動に追加ターンがかかる", TravelEventType.BadWeather),
            new TravelEvent("宝箱発見", "道の脇に宝箱が置かれている。罠かもしれない", TravelEventType.TreasureChest)
        };

        return events[random.Next(events.Length)];
    }
}

/// <summary>
/// 移動結果
/// </summary>
public record TravelResult(bool Success, string Message, int TurnCost);

/// <summary>
/// 移動イベント
/// </summary>
public record TravelEvent(string Name, string Description, TravelEventType Type);

/// <summary>
/// 移動イベントタイプ
/// </summary>
public enum TravelEventType
{
    Merchant,
    Ambush,
    HelpRequest,
    Shrine,
    BadWeather,
    TreasureChest
}

/// <summary>
/// 施設定義
/// </summary>
public record FacilityDefinition(
    FacilityType Type,
    string Name,
    string Description,
    bool RequiresGold)
{
    private static readonly Dictionary<FacilityType, FacilityDefinition> All = new()
    {
        [FacilityType.AdventurerGuild] = new(FacilityType.AdventurerGuild, "冒険者ギルド", "依頼を受けたり報酬を受け取る", false),
        [FacilityType.GeneralShop] = new(FacilityType.GeneralShop, "雑貨店", "消耗品や素材を販売", true),
        [FacilityType.WeaponShop] = new(FacilityType.WeaponShop, "武器店", "武器を販売", true),
        [FacilityType.ArmorShop] = new(FacilityType.ArmorShop, "防具店", "防具を販売", true),
        [FacilityType.Inn] = new(FacilityType.Inn, "宿屋", "休息してHPとMPを全回復", true),
        [FacilityType.Smithy] = new(FacilityType.Smithy, "鍛冶屋", "装備の強化・修理・合成", true),
        [FacilityType.Church] = new(FacilityType.Church, "教会", "呪い解除・状態異常回復", true),
        [FacilityType.Temple] = new(FacilityType.Temple, "神殿", "入信・改宗・祈祷", false),
        [FacilityType.MagicShop] = new(FacilityType.MagicShop, "魔法店", "魔法関連アイテムを販売", true),
        [FacilityType.Library] = new(FacilityType.Library, "図書館", "ルーン語の学習", false),
        [FacilityType.Bank] = new(FacilityType.Bank, "銀行", "ゴールドの預け入れ・引き出し", false),
        [FacilityType.Arena] = new(FacilityType.Arena, "闘技場", "連続戦闘で報酬を得る", false)
    };

    public static FacilityDefinition Get(FacilityType type) => All[type];
    public static IReadOnlyDictionary<FacilityType, FacilityDefinition> GetAll() => All;
}

/// <summary>
/// 街システム - 地上拠点の施設管理
/// </summary>
public class TownSystem
{
    /// <summary>銀行預金残高</summary>
    public int BankBalance { get; private set; } = 0;

    /// <summary>現在の領地で利用可能な施設を取得</summary>
    public IReadOnlyList<FacilityDefinition> GetAvailableFacilities(TerritoryId territory)
    {
        var terrDef = TerritoryDefinition.Get(territory);
        return terrDef.AvailableFacilities.Select(FacilityDefinition.Get).ToList();
    }

    /// <summary>指定施設が利用可能か</summary>
    public bool IsFacilityAvailable(TerritoryId territory, FacilityType facility)
    {
        var terrDef = TerritoryDefinition.Get(territory);
        return terrDef.AvailableFacilities.Contains(facility);
    }

    /// <summary>宿屋で休息（HP/MP全回復、ターン消費）</summary>
    public TownActionResult RestAtInn(Entities.Player player, int cost = 50)
    {
        if (!player.SpendGold(cost))
            return new TownActionResult(false, $"ゴールドが足りない（必要: {cost}G）");

        player.Heal(player.MaxHp);
        player.RestoreMp(player.MaxMp);
        player.RestoreSp(player.MaxSp);
        return new TownActionResult(true, $"宿屋で休息した。HP/MP/SP全回復（{cost}G）",
            TurnCost: TimeConstants.TurnsPerHour * 8);
    }

    /// <summary>教会で呪い解除</summary>
    public TownActionResult RemoveCurseAtChurch(Entities.Player player, int cost = 100)
    {
        if (!player.SpendGold(cost))
            return new TownActionResult(false, $"ゴールドが足りない（必要: {cost}G）");

        var cursedEffects = player.StatusEffects
            .Where(e => e.Type == StatusEffectType.Curse)
            .ToList();

        foreach (var effect in cursedEffects)
        {
            player.RemoveStatusEffect(effect.Type);
        }

        return new TownActionResult(true, $"教会で呪いを解除してもらった（{cost}G）");
    }

    /// <summary>銀行に預け入れ</summary>
    public TownActionResult DepositGold(Entities.Player player, int amount)
    {
        if (amount <= 0)
            return new TownActionResult(false, "預け入れ額が不正");

        if (!player.SpendGold(amount))
            return new TownActionResult(false, "ゴールドが足りない");

        BankBalance += amount;
        return new TownActionResult(true, $"{amount}Gを預け入れた（残高: {BankBalance}G）");
    }

    /// <summary>銀行から引き出し</summary>
    public TownActionResult WithdrawGold(Entities.Player player, int amount)
    {
        if (amount <= 0)
            return new TownActionResult(false, "引き出し額が不正");

        if (amount > BankBalance)
            return new TownActionResult(false, $"残高が足りない（残高: {BankBalance}G）");

        BankBalance -= amount;
        player.AddGold(amount);
        return new TownActionResult(true, $"{amount}Gを引き出した（残高: {BankBalance}G）");
    }

    /// <summary>銀行残高を設定（セーブ復元用）</summary>
    public void SetBankBalance(int balance)
    {
        BankBalance = Math.Max(0, balance);
    }
}

/// <summary>
/// 街アクション結果
/// </summary>
public record TownActionResult(bool Success, string Message, int TurnCost = 0);

/// <summary>
/// ショップシステム - 売買処理
/// </summary>
public class ShopSystem
{
    /// <summary>ショップ商品定義</summary>
    public record ShopItem(
        string ItemId,
        string Name,
        int BasePrice,
        int Stock,
        FacilityType ShopType);

    private readonly Dictionary<FacilityType, List<ShopItem>> _shopInventories = new();

    /// <summary>ショップの在庫を初期化</summary>
    public void InitializeShop(FacilityType shopType, TerritoryId territory, int playerLevel)
    {
        var items = GenerateShopItems(shopType, territory, playerLevel);
        _shopInventories[shopType] = items;
    }

    /// <summary>ショップの在庫を取得</summary>
    public IReadOnlyList<ShopItem> GetShopItems(FacilityType shopType) =>
        _shopInventories.TryGetValue(shopType, out var items) ? items : Array.Empty<ShopItem>();

    /// <summary>ショップの在庫をクリア</summary>
    public void ClearShopInventory()
    {
        _shopInventories.Clear();
    }

    /// <summary>アイテムを購入</summary>
    public ShopActionResult Buy(Entities.Player player, FacilityType shopType, int index, double charismaDiscount = 0.0)
    {
        if (!_shopInventories.TryGetValue(shopType, out var items) || index < 0 || index >= items.Count)
            return new ShopActionResult(false, "商品が見つからない");

        var item = items[index];
        if (item.Stock <= 0)
            return new ShopActionResult(false, "在庫切れ");

        int finalPrice = Math.Max(1, (int)(item.BasePrice * (1.0 - charismaDiscount)));

        if (!player.SpendGold(finalPrice))
            return new ShopActionResult(false, $"ゴールドが足りない（必要: {finalPrice}G）");

        items[index] = item with { Stock = item.Stock - 1 };

        return new ShopActionResult(true, $"{item.Name}を{finalPrice}Gで購入した", item.ItemId);
    }

    /// <summary>アイテムを売却</summary>
    public ShopActionResult Sell(Entities.Player player, string itemName, int baseValue, double charismaBonus = 0.0)
    {
        int sellPrice = Math.Max(1, (int)(baseValue * (0.4 + charismaBonus * 0.2)));
        player.AddGold(sellPrice);
        return new ShopActionResult(true, $"{itemName}を{sellPrice}Gで売却した");
    }

    /// <summary>カリスマに基づく値引き率を計算</summary>
    public static double CalculateCharismaDiscount(int charisma)
    {
        return Math.Min(0.20, Math.Max(0, (charisma - 10) * 0.01));
    }

    /// <summary>領地レベルに基づく価格倍率を計算</summary>
    public static double GetTerritoryPriceMultiplier(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => 1.0,
        TerritoryId.Forest => 1.1,
        TerritoryId.Mountain => 1.2,
        TerritoryId.Coast => 0.9,
        TerritoryId.Southern => 1.3,
        TerritoryId.Frontier => 1.5,
        _ => 1.0
    };

    private List<ShopItem> GenerateShopItems(FacilityType shopType, TerritoryId territory, int playerLevel)
    {
        double priceMultiplier = GetTerritoryPriceMultiplier(territory);

        int Scale(int basePrice) => Math.Max(1, (int)(basePrice * priceMultiplier));

        return shopType switch
        {
            FacilityType.GeneralShop => GenerateGeneralShopItems(territory, playerLevel, Scale),
            FacilityType.WeaponShop => GenerateWeaponShopItems(territory, playerLevel, Scale),
            FacilityType.ArmorShop => GenerateArmorShopItems(territory, playerLevel, Scale),
            FacilityType.MagicShop => GenerateMagicShopItems(territory, playerLevel, Scale),
            _ => new()
        };
    }

    private static List<ShopItem> GenerateGeneralShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("healing_potion", "回復ポーション", scale(50), 10, FacilityType.GeneralShop),
            new("mana_potion", "マナポーション", scale(80), 5, FacilityType.GeneralShop),
            new("bread", "パン", scale(20), 15, FacilityType.GeneralShop),
            new("torch", "松明", scale(10), 20, FacilityType.GeneralShop),
            new("antidote", "解毒薬", scale(40), 5, FacilityType.GeneralShop),
            new("scroll_identify", "識別の巻物", scale(100), 3, FacilityType.GeneralShop)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("greater_healing_potion", "上質回復ポーション", scale(150), 5, FacilityType.GeneralShop));
        }

        if (territory == TerritoryId.Forest)
        {
            items.Add(new("herbal_remedy", "薬草", scale(30), 10, FacilityType.GeneralShop));
        }

        return items;
    }

    private static List<ShopItem> GenerateWeaponShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("iron_sword", "鉄の剣", scale(200), 3, FacilityType.WeaponShop),
            new("iron_axe", "鉄の斧", scale(250), 2, FacilityType.WeaponShop),
            new("iron_mace", "鉄の鎚", scale(220), 2, FacilityType.WeaponShop),
            new("dagger", "ダガー", scale(100), 5, FacilityType.WeaponShop),
            new("short_bow", "短弓", scale(180), 2, FacilityType.WeaponShop),
            new("wooden_staff", "木の杖", scale(150), 2, FacilityType.WeaponShop)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("steel_sword", "鋼の剣", scale(500), 2, FacilityType.WeaponShop));
        }

        if (territory == TerritoryId.Mountain)
        {
            items.Add(new("mithril_dagger", "ミスリルダガー", scale(800), 1, FacilityType.WeaponShop));
        }

        return items;
    }

    private static List<ShopItem> GenerateArmorShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("leather_armor", "革鎧", scale(150), 3, FacilityType.ArmorShop),
            new("chain_mail", "鎖帷子", scale(400), 2, FacilityType.ArmorShop),
            new("iron_shield", "鉄の盾", scale(200), 2, FacilityType.ArmorShop),
            new("robe", "ローブ", scale(100), 3, FacilityType.ArmorShop),
            new("iron_helmet", "鉄の兜", scale(150), 2, FacilityType.ArmorShop)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("steel_armor", "鋼の鎧", scale(800), 1, FacilityType.ArmorShop));
        }

        return items;
    }

    private static List<ShopItem> GenerateMagicShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("mana_potion", "マナポーション", scale(80), 10, FacilityType.MagicShop),
            new("scroll_teleport", "転移の巻物", scale(200), 2, FacilityType.MagicShop),
            new("scroll_identify", "識別の巻物", scale(100), 5, FacilityType.MagicShop),
            new("wand_fire", "火の杖", scale(500), 1, FacilityType.MagicShop),
            new("rune_stone", "ルーン石", scale(300), 3, FacilityType.MagicShop)
        };

        if (playerLevel >= 15)
        {
            items.Add(new("greater_mana_potion", "上質マナポーション", scale(200), 5, FacilityType.MagicShop));
        }

        return items;
    }
}

/// <summary>
/// ショップアクション結果
/// </summary>
public record ShopActionResult(bool Success, string Message, string? ItemId = null);

/// <summary>
/// ダンジョン特殊フロアシステム
/// </summary>
public static class SpecialFloorSystem
{
    /// <summary>フロアの特殊タイプを決定（IRandomProvider版）</summary>
    public static SpecialFloorType DetermineFloorType(int depth, IRandomProvider random)
    {
        if (depth % GameConstants.BossFloorInterval == 0)
            return SpecialFloorType.BossRoom;

        if (random.Next(100) < 15)
        {
            var types = new[]
            {
                SpecialFloorType.Shop,
                SpecialFloorType.TreasureVault,
                SpecialFloorType.RestPoint,
                SpecialFloorType.Library
            };
            return types[random.Next(types.Length)];
        }

        return SpecialFloorType.Normal;
    }

    /// <summary>フロアの特殊タイプを決定（System.Random版）</summary>
    public static SpecialFloorType DetermineFloorType(int depth, Random random)
    {
        if (depth % GameConstants.BossFloorInterval == 0)
            return SpecialFloorType.BossRoom;

        if (random.Next(100) < 15)
        {
            var types = new[]
            {
                SpecialFloorType.Shop,
                SpecialFloorType.TreasureVault,
                SpecialFloorType.RestPoint,
                SpecialFloorType.Library
            };
            return types[random.Next(types.Length)];
        }

        return SpecialFloorType.Normal;
    }

    /// <summary>特殊フロアの説明を取得</summary>
    public static string GetFloorDescription(SpecialFloorType type) => type switch
    {
        SpecialFloorType.Shop => "ショップフロア - 旅の商人がアイテムを販売している",
        SpecialFloorType.TreasureVault => "宝物庫 - 貴重なアイテムが眠るが罠も多い",
        SpecialFloorType.BossRoom => "ボス部屋 - 強力なボスモンスターが待ち受ける",
        SpecialFloorType.RestPoint => "休憩ポイント - 安全にHP/MPを回復できる",
        SpecialFloorType.Arena => "闘技場 - 連続戦闘で特別な報酬を得られる",
        SpecialFloorType.Library => "古代図書館 - ルーン語を学べる書物がある",
        _ => "通常フロア"
    };

    /// <summary>特殊フロアの敵出現率倍率を取得</summary>
    public static double GetEnemySpawnMultiplier(SpecialFloorType type) => type switch
    {
        SpecialFloorType.Shop => 0.0,
        SpecialFloorType.RestPoint => 0.0,
        SpecialFloorType.Library => 0.3,
        SpecialFloorType.TreasureVault => 1.5,
        SpecialFloorType.BossRoom => 0.5,
        SpecialFloorType.Arena => 2.0,
        _ => 1.0
    };
}

/// <summary>
/// ランダムイベントシステム - ダンジョン内イベント管理
/// </summary>
public class RandomEventSystem
{
    /// <summary>ランダムイベント定義</summary>
    public record RandomEvent(
        RandomEventType Type,
        string Name,
        string Description,
        int MinDepth = 1);

    private static readonly List<RandomEvent> _events = new()
    {
        new(RandomEventType.TreasureChest, "宝箱", "宝箱を発見した。開ける？", 1),
        new(RandomEventType.Fountain, "泉", "澄んだ泉を発見した。飲む？", 1),
        new(RandomEventType.Shrine, "祠", "小さな祠がある。祈る？", 3),
        new(RandomEventType.Ruins, "古代遺跡", "古代の遺跡を発見した。調べる？", 5),
        new(RandomEventType.NpcEncounter, "NPC遭遇", "誰かがいるようだ", 1),
        new(RandomEventType.MerchantEncounter, "商人遭遇", "旅の商人と出会った", 3),
        new(RandomEventType.RestPoint, "休憩場所", "安全そうな場所を見つけた", 1),
        new(RandomEventType.MysteriousItem, "謎のアイテム", "光るアイテムが落ちている", 5),
        new(RandomEventType.AmbushEvent, "待ち伏せ", "何かの気配を感じる...", 5),
        new(RandomEventType.Trap, "罠", "足元に何かある...", 1),
        new(RandomEventType.MonsterHouse, "モンスターハウス", "大量の敵が潜んでいる！", 5),
        new(RandomEventType.CursedRoom, "呪われた部屋", "部屋に入った瞬間、不吉な気配が…", 8),
        new(RandomEventType.BlessedRoom, "祝福の部屋", "聖なる光が満ちている", 10),
        new(RandomEventType.HiddenShop, "隠しショップ", "壁の奥に隠された店がある", 7),
        new(RandomEventType.MaterialDeposit, "素材採取場", "貴重な素材が採取できそうだ", 3)
    };

    /// <summary>領域別追加イベントプール</summary>
    private static readonly Dictionary<TerritoryId, List<RandomEvent>> _territoryEvents = new()
    {
        [TerritoryId.Forest] = new()
        {
            new(RandomEventType.Fountain, "精霊の泉", "森の精霊が守る泉を発見した", 1),
            new(RandomEventType.MaterialDeposit, "薬草群生地", "貴重な薬草が群生している", 2),
            new(RandomEventType.NpcEncounter, "森の隠者", "森に住む隠者と出会った", 3)
        },
        [TerritoryId.Mountain] = new()
        {
            new(RandomEventType.MaterialDeposit, "鉱石の露頭", "良質な鉱石が露出している", 3),
            new(RandomEventType.Trap, "落石", "上から岩が落ちてきた！", 2),
            new(RandomEventType.TreasureChest, "鍛冶場跡", "古い鍛冶場の跡地を発見した", 5)
        },
        [TerritoryId.Coast] = new()
        {
            new(RandomEventType.TreasureChest, "漂着物", "海岸に何かが打ち上げられている", 1),
            new(RandomEventType.Fountain, "潮溜まり", "不思議な力を持つ潮溜まりを発見した", 3),
            new(RandomEventType.MerchantEncounter, "海賊商人", "密輸品を売る海賊と出会った", 5)
        },
        [TerritoryId.Southern] = new()
        {
            new(RandomEventType.Trap, "流砂", "砂の中に引きずり込まれそうになった！", 3),
            new(RandomEventType.Ruins, "砂に埋もれた遺跡", "砂漠の下に古代遺跡がある", 5),
            new(RandomEventType.MysteriousItem, "蜃気楼の宝", "蜃気楼の中に光るものが見える", 7)
        },
        [TerritoryId.Frontier] = new()
        {
            new(RandomEventType.AmbushEvent, "魔物の巣", "強力な魔物の巣に迷い込んだ", 5),
            new(RandomEventType.CursedRoom, "闇の祭壇", "邪悪な祭壇が佇んでいる", 8),
            new(RandomEventType.Shrine, "忘れられた社", "古い社がひっそりと立っている", 5)
        }
    };

    /// <summary>領域を考慮したランダムイベントを発生させる</summary>
    public RandomEvent? RollTerritoryEvent(int depth, TerritoryId territory, IRandomProvider random)
    {
        if (random.Next(100) >= 12) return null;

        var candidates = _events.Where(e => depth >= e.MinDepth).ToList();

        if (_territoryEvents.TryGetValue(territory, out var extras))
        {
            candidates.AddRange(extras.Where(e => depth >= e.MinDepth));
        }

        if (candidates.Count == 0) return null;
        return candidates[random.Next(candidates.Count)];
    }

    /// <summary>ダンジョン内ランダムイベントを発生させる（IRandomProvider版）</summary>
    public RandomEvent? RollEvent(int depth, IRandomProvider random)
    {
        if (random.Next(100) >= 10) return null;

        var candidates = _events.Where(e => depth >= e.MinDepth).ToList();
        if (candidates.Count == 0) return null;
        return candidates[random.Next(candidates.Count)];
    }

    /// <summary>ダンジョン内ランダムイベントを発生させる（System.Random版）</summary>
    public RandomEvent? RollEvent(int depth, Random random)
    {
        if (random.Next(100) >= 10) return null;

        var candidates = _events.Where(e => depth >= e.MinDepth).ToList();
        if (candidates.Count == 0) return null;
        return candidates[random.Next(candidates.Count)];
    }

    /// <summary>宝箱イベントの結果</summary>
    public EventResult ResolveTreasureChest(int depth, IRandomProvider random)
    {
        bool isTrap = random.Next(100) < 20;
        if (isTrap)
        {
            int damage = 5 + depth * 2;
            return new EventResult(false, $"宝箱は罠だった！{damage}ダメージ！", damage);
        }

        int gold = 50 + depth * 20 + random.Next(depth * 30 + 1);
        return new EventResult(true, $"宝箱から{gold}ゴールドを見つけた！", 0, gold);
    }

    /// <summary>宝箱イベントの結果（System.Random版）</summary>
    public EventResult ResolveTreasureChest(int depth, Random random)
    {
        bool isTrap = random.Next(100) < 20;
        if (isTrap)
        {
            int damage = 5 + depth * 2;
            return new EventResult(false, $"宝箱は罠だった！{damage}ダメージ！", damage);
        }

        int gold = 50 + depth * 20 + random.Next(depth * 30);
        return new EventResult(true, $"宝箱から{gold}ゴールドを見つけた！", 0, gold);
    }

    /// <summary>泉イベントの結果</summary>
    public EventResult ResolveFountain(IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 50)
            return new EventResult(true, "泉の水を飲んだ。HPが30回復した", HealAmount: 30);
        if (roll < 80)
            return new EventResult(true, "泉の水を飲んだ。MPが20回復した", MpHealAmount: 20);
        return new EventResult(false, "泉の水は毒だった！", DamageAmount: 10);
    }

    /// <summary>泉イベントの結果（System.Random版）</summary>
    public EventResult ResolveFountain(Random random)
    {
        int roll = random.Next(100);
        if (roll < 50)
            return new EventResult(true, "泉の水を飲んだ。HPが30回復した", HealAmount: 30);
        if (roll < 80)
            return new EventResult(true, "泉の水を飲んだ。MPが20回復した", MpHealAmount: 20);
        return new EventResult(false, "泉の水は毒だった！", DamageAmount: 10);
    }

    /// <summary>祠イベントの結果</summary>
    public EventResult ResolveShrine(IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 40)
            return new EventResult(true, "祠に祈った。全ステータスが一時的に上昇した");
        if (roll < 70)
            return new EventResult(true, "祠に祈った。HPとMPが全回復した", HealAmount: 9999, MpHealAmount: 9999);
        return new EventResult(true, "祠は何の反応も示さなかった");
    }

    /// <summary>祠イベントの結果（System.Random版）</summary>
    public EventResult ResolveShrine(Random random)
    {
        int roll = random.Next(100);
        if (roll < 40)
            return new EventResult(true, "祠に祈った。全ステータスが一時的に上昇した");
        if (roll < 70)
            return new EventResult(true, "祠に祈った。HPとMPが全回復した", HealAmount: 9999, MpHealAmount: 9999);
        return new EventResult(true, "祠は何の反応も示さなかった");
    }

    /// <summary>休憩イベントの結果</summary>
    public EventResult ResolveRestPoint()
    {
        return new EventResult(true, "安全な場所で休憩した。HPが少し回復した", HealAmount: 20, MpHealAmount: 10);
    }

    /// <summary>罠イベントの結果</summary>
    public EventResult ResolveTrap(int depth, IRandomProvider random)
    {
        int damage = 3 + depth + random.Next(depth + 1);
        return new EventResult(false, $"罠にかかった！{damage}ダメージ！", DamageAmount: damage);
    }

    /// <summary>古代遺跡イベントの結果</summary>
    public EventResult ResolveRuins(int depth, IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 40)
        {
            int gold = 100 + depth * 30 + random.Next(depth * 20 + 1);
            return new EventResult(true, $"遺跡から{gold}ゴールド相当の財宝を発見した！", GoldAmount: gold);
        }
        if (roll < 70)
            return new EventResult(true, "古代の知識を得た。経験値を獲得した");
        return new EventResult(false, "遺跡の罠が作動した！", DamageAmount: depth * 3);
    }

    /// <summary>謎のアイテムイベントの結果</summary>
    public EventResult ResolveMysteriousItem(IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 60)
            return new EventResult(true, "光るアイテムを拾った。有用なアイテムだった！");
        if (roll < 85)
            return new EventResult(true, "光るアイテムを拾った。呪われていた...");
        return new EventResult(false, "光るアイテムは罠だった！爆発した！", DamageAmount: 15);
    }

    /// <summary>モンスターハウスイベントの結果</summary>
    public EventResult ResolveMonsterHouse(int depth, IRandomProvider random)
    {
        int monsterCount = 3 + random.Next(depth / 2 + 1);
        int bonusGold = monsterCount * 20 + depth * 10;
        return new EventResult(false,
            $"モンスターハウスだ！{monsterCount}体の敵が襲いかかってくる！撃破報酬:{bonusGold}G",
            GoldAmount: bonusGold);
    }

    /// <summary>呪われた部屋イベントの結果</summary>
    public EventResult ResolveCursedRoom(IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 40)
            return new EventResult(false, "呪いの力で体が蝕まれた！", DamageAmount: 20);
        if (roll < 70)
            return new EventResult(false, "呪いで持ち物の一つが呪われた…");
        return new EventResult(true, "呪いを跳ね返した！呪力を吸収してMPが回復した", MpHealAmount: 30);
    }

    /// <summary>祝福の部屋イベントの結果</summary>
    public EventResult ResolveBlessedRoom(IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 50)
            return new EventResult(true, "聖なる光に包まれた。HPとMPが全回復した！", HealAmount: 9999, MpHealAmount: 9999);
        if (roll < 80)
            return new EventResult(true, "聖なる祝福を受けた。ステータスが一時的に上昇した！");
        return new EventResult(true, "聖なる光で装備が浄化された。呪いが解除された！");
    }

    /// <summary>隠しショップイベントの結果</summary>
    public EventResult ResolveHiddenShop()
    {
        return new EventResult(true, "隠しショップを発見した！珍しいアイテムが並んでいる");
    }

    /// <summary>素材採取イベントの結果</summary>
    public EventResult ResolveMaterialDeposit(int depth, TerritoryId territory, IRandomProvider random)
    {
        string material = territory switch
        {
            TerritoryId.Forest => "薬草",
            TerritoryId.Mountain => "鉱石",
            TerritoryId.Coast => "珊瑚",
            TerritoryId.Southern => "砂金",
            TerritoryId.Frontier => "魔石",
            _ => "素材"
        };
        int quantity = 1 + random.Next(3) + depth / 5;
        return new EventResult(true, $"{material}を{quantity}個採取した！");
    }

    /// <summary>全イベント定義を取得</summary>
    public static IReadOnlyList<RandomEvent> GetAllEvents() => _events;

    /// <summary>領域別イベント定義を取得</summary>
    public static IReadOnlyList<RandomEvent> GetTerritoryEvents(TerritoryId territory)
    {
        return _territoryEvents.TryGetValue(territory, out var events)
            ? events
            : new List<RandomEvent>();
    }
}

/// <summary>
/// イベント結果
/// </summary>
public record EventResult(
    bool IsPositive,
    string Message,
    int DamageAmount = 0,
    int GoldAmount = 0,
    int HealAmount = 0,
    int MpHealAmount = 0);
