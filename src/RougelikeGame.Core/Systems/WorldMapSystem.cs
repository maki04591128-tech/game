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
    Dungeon,
    /// <summary>都（領地の首都）</summary>
    Capital,
    /// <summary>野盗のねぐら（ランダム生成ダンジョン）</summary>
    BanditDen,
    /// <summary>ゴブリンの巣（ランダム生成ダンジョン）</summary>
    GoblinNest,
    /// <summary>アンデッドの墓所（ランダム生成ダンジョン）</summary>
    UndeadCrypt,
    /// <summary>魔族の門（ランダム生成ダンジョン）</summary>
    DemonPortal,
    /// <summary>関所（領地間の境界ゲート）</summary>
    BorderGate,
    /// <summary>徘徊ボスモンスター（シンボルエンカウント）</summary>
    WanderingBoss
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
    int DangerLevel = 1,
    string[]? SubLocationIds = null,
    int? MaxFloor = null)
{
    /// <summary>関所IDの区切り文字列</summary>
    private const string GateToDelimiter = "_gate_to_";

    /// <summary>
    /// 関所(BorderGate)の遷移先領地を取得する。
    /// 関所でない場合やIDが解析できない場合はnullを返す。
    /// </summary>
    public TerritoryId? GetBorderGateTarget()
    {
        if (Type != LocationType.BorderGate) return null;
        int idx = Id.IndexOf(GateToDelimiter, StringComparison.Ordinal);
        if (idx < 0) return null;
        string targetStr = Id[(idx + GateToDelimiter.Length)..];
        return Enum.TryParse<TerritoryId>(targetStr, out var target) ? target : null;
    }

    private static readonly Dictionary<string, LocationDefinition> All = new()
    {
        // 王都領 ── 統合: 非ダンジョンを「王都」に集約
        ["capital_town"] = new("capital_town", "王都", "王国の中心都市。王城、冒険者ギルド本部、賢者院、大聖堂、中央市場、闘技場、貧民街が存在する",
            LocationType.Town, TerritoryId.Capital,
            SubLocationIds: new[] { "capital_castle", "capital_guild", "capital_academy", "capital_cathedral", "capital_market", "capital_arena", "capital_slum" }),
        ["capital_castle"] = new("capital_castle", "王城", "国王への謁見、メインクエスト関連", LocationType.Facility, TerritoryId.Capital),
        ["capital_guild"] = new("capital_guild", "冒険者ギルド本部", "クエスト受注、ランク昇格試験", LocationType.Facility, TerritoryId.Capital),
        ["capital_academy"] = new("capital_academy", "賢者院", "魔法言語の学習、魔法書購入", LocationType.Facility, TerritoryId.Capital),
        ["capital_cathedral"] = new("capital_cathedral", "大聖堂", "光の神殿の総本山", LocationType.ReligiousSite, TerritoryId.Capital),
        ["capital_market"] = new("capital_market", "中央市場", "最も品揃えが良い市場", LocationType.Town, TerritoryId.Capital),
        ["capital_arena"] = new("capital_arena", "闘技場", "戦闘訓練、賞金稼ぎ", LocationType.Facility, TerritoryId.Capital),
        ["capital_slum"] = new("capital_slum", "貧民街", "情報収集、闇市場、盗賊ギルド", LocationType.Field, TerritoryId.Capital, DangerLevel: 2),
        ["capital_catacombs"] = new("capital_catacombs", "王都地下墓地", "中難易度、アンデッド多数", LocationType.Dungeon, TerritoryId.Capital, MinLevel: 3, DangerLevel: 3),
        ["capital_rift"] = new("capital_rift", "始まりの裂け目", "高難易度、最初に出現したダンジョン", LocationType.Dungeon, TerritoryId.Capital, MinLevel: 8, DangerLevel: 4),

        // 森林領 ── 統合: 非ダンジョンを「緑樹の都」に集約
        ["forest_town"] = new("forest_town", "緑樹の都", "エルフの首都。薬師の村、世界樹の根元、深緑の森、精霊の泉が周辺に広がる",
            LocationType.Town, TerritoryId.Forest,
            SubLocationIds: new[] { "forest_city", "forest_herbalist", "forest_worldtree", "forest_deep", "forest_spring" }),
        ["forest_city"] = new("forest_city", "緑樹の都中心部", "エルフの首都、樹上都市", LocationType.Town, TerritoryId.Forest),
        ["forest_herbalist"] = new("forest_herbalist", "薬師の村", "薬草・ポーション販売", LocationType.Village, TerritoryId.Forest),
        ["forest_worldtree"] = new("forest_worldtree", "世界樹の根元", "自然崇拝の聖地", LocationType.ReligiousSite, TerritoryId.Forest),
        ["forest_deep"] = new("forest_deep", "深緑の森", "迷いやすい、幻惑効果", LocationType.Field, TerritoryId.Forest, DangerLevel: 3),
        ["forest_spring"] = new("forest_spring", "精霊の泉", "回復スポット、精霊との交信", LocationType.Field, TerritoryId.Forest),
        ["forest_corruption"] = new("forest_corruption", "腐敗の森", "高難易度、汚染された区域", LocationType.Dungeon, TerritoryId.Forest, MinLevel: 10, DangerLevel: 4),
        ["forest_ruins"] = new("forest_ruins", "古代エルフの遺跡", "高難易度、古代魔法の罠", LocationType.Dungeon, TerritoryId.Forest, MinLevel: 12, DangerLevel: 5),

        // 山岳領 ── 統合: 非ダンジョンを「鉄床城」に集約
        ["mountain_town"] = new("mountain_town", "鉄床城", "ドワーフの首都。坑夫の村、山頂の祠、山岳街道が周辺に広がる",
            LocationType.Town, TerritoryId.Mountain,
            SubLocationIds: new[] { "mountain_fortress", "mountain_miner", "mountain_shrine", "mountain_road" }),
        ["mountain_fortress"] = new("mountain_fortress", "鉄床城中心部", "ドワーフの首都、最高の鍛冶屋", LocationType.Town, TerritoryId.Mountain),
        ["mountain_miner"] = new("mountain_miner", "坑夫の村", "鉱山入口、採掘依頼", LocationType.Village, TerritoryId.Mountain),
        ["mountain_shrine"] = new("mountain_shrine", "山頂の祠", "死神信仰の隠れ聖地", LocationType.ReligiousSite, TerritoryId.Mountain),
        ["mountain_road"] = new("mountain_road", "山岳街道", "険しい道、落石注意", LocationType.Field, TerritoryId.Mountain, DangerLevel: 3),
        ["mountain_mine"] = new("mountain_mine", "採掘坑", "中難易度、資源収集", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 10, DangerLevel: 3),
        ["mountain_lava"] = new("mountain_lava", "溶岩洞", "高難易度、火属性の敵", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 15, DangerLevel: 4),
        ["mountain_dragon"] = new("mountain_dragon", "竜の巣", "最高難易度、ドラゴン", LocationType.Dungeon, TerritoryId.Mountain, MinLevel: 25, DangerLevel: 5),

        // 海岸領 ── 統合: 非ダンジョンを「港町マリーナ」に集約
        ["coast_town"] = new("coast_town", "港町マリーナ", "海運の要所。ぶどう畑の村、麦畑地帯、闇の礼拝堂が周辺に広がる",
            LocationType.Town, TerritoryId.Coast,
            SubLocationIds: new[] { "coast_port", "coast_vineyard", "coast_field", "coast_darkchapel" }),
        ["coast_port"] = new("coast_port", "港町マリーナ中心部", "海運、船の購入、海外商品", LocationType.Town, TerritoryId.Coast),
        ["coast_vineyard"] = new("coast_vineyard", "ぶどう畑の村", "ワイン生産、のどかな農村", LocationType.Village, TerritoryId.Coast),
        ["coast_field"] = new("coast_field", "麦畑地帯", "平和だが時折盗賊出没", LocationType.Field, TerritoryId.Coast, DangerLevel: 1),
        ["coast_darkchapel"] = new("coast_darkchapel", "闇の礼拝堂", "闇の教団の拠点", LocationType.ReligiousSite, TerritoryId.Coast, MinLevel: 10, DangerLevel: 3),
        ["coast_cave"] = new("coast_cave", "海岸洞窟", "低難易度、海賊の隠れ家", LocationType.Dungeon, TerritoryId.Coast, MinLevel: 5, DangerLevel: 2),
        ["coast_wreck"] = new("coast_wreck", "沈没船", "中難易度、水棲モンスター", LocationType.Dungeon, TerritoryId.Coast, MinLevel: 8, DangerLevel: 3),

        // 南部領 ── 統合: 非ダンジョンを「サンライト城」に集約
        ["southern_town"] = new("southern_town", "サンライト城", "領主の城下町。雪原の村、狩人の集落、凍てつく平原が周辺に広がる",
            LocationType.Town, TerritoryId.Southern,
            SubLocationIds: new[] { "southern_castle", "southern_village", "southern_hunter", "southern_plain" }),
        ["southern_castle"] = new("southern_castle", "サンライト城中心部", "領主の城、華やかな貴族社会", LocationType.Town, TerritoryId.Southern),
        ["southern_village"] = new("southern_village", "雪原の村", "休息、補給、情報収集", LocationType.Village, TerritoryId.Southern),
        ["southern_hunter"] = new("southern_hunter", "狩人の集落", "獣人が住む、特殊装備入手", LocationType.Village, TerritoryId.Southern),
        ["southern_plain"] = new("southern_plain", "凍てつく平原", "広大な雪原、野生動物", LocationType.Field, TerritoryId.Southern, DangerLevel: 3),
        ["southern_icecave"] = new("southern_icecave", "氷の洞窟", "中難易度、氷属性の敵", LocationType.Dungeon, TerritoryId.Southern, MinLevel: 15, DangerLevel: 3),
        ["southern_battlefield"] = new("southern_battlefield", "古戦場跡", "中難易度、アンデッド兵士", LocationType.Dungeon, TerritoryId.Southern, MinLevel: 15, DangerLevel: 3),

        // 辺境領 ── 統合: 非ダンジョンを「辺境砦」に集約
        ["frontier_town"] = new("frontier_town", "辺境砦", "かろうじて秩序が保たれた拠点。ならず者の集落、廃墟の街、荒野、混沌の祭壇が周辺に広がる",
            LocationType.Town, TerritoryId.Frontier,
            SubLocationIds: new[] { "frontier_fort", "frontier_outlaw", "frontier_ruins_city", "frontier_wasteland", "frontier_chaos_altar" }),
        ["frontier_fort"] = new("frontier_fort", "辺境砦中心部", "かろうじて秩序が保たれた拠点", LocationType.Town, TerritoryId.Frontier),
        ["frontier_outlaw"] = new("frontier_outlaw", "ならず者の集落", "無法者の溜まり場、闇市場", LocationType.Village, TerritoryId.Frontier),
        ["frontier_ruins_city"] = new("frontier_ruins_city", "廃墟の街", "かつての都市の残骸", LocationType.Field, TerritoryId.Frontier, DangerLevel: 4),
        ["frontier_wasteland"] = new("frontier_wasteland", "荒野", "魔物が徘徊する危険地帯", LocationType.Field, TerritoryId.Frontier, DangerLevel: 4),
        ["frontier_chaos_altar"] = new("frontier_chaos_altar", "混沌の祭壇", "混沌の崇拝の中心地", LocationType.ReligiousSite, TerritoryId.Frontier, MinLevel: 20, DangerLevel: 4),
        ["frontier_great_rift"] = new("frontier_great_rift", "大裂け目", "最高難易度、最終ダンジョン候補", LocationType.Dungeon, TerritoryId.Frontier, MinLevel: 25, DangerLevel: 5),
        ["frontier_ancient_ruins"] = new("frontier_ancient_ruins", "滅びた王国の遺跡", "高難易度、古代文明の秘密", LocationType.Dungeon, TerritoryId.Frontier, MinLevel: 20, DangerLevel: 5),

        // 砂漠領
        ["desert_town"] = new("desert_town", "砂都オアシス", "砂漠のオアシスに築かれた交易都市。商人ギルド、砂漠の祠、隊商宿が存在する",
            LocationType.Town, TerritoryId.Desert,
            SubLocationIds: new[] { "desert_market", "desert_shrine", "desert_caravan" }),
        ["desert_market"] = new("desert_market", "砂漠の大市場", "異国の珍品が集まる市場", LocationType.Town, TerritoryId.Desert),
        ["desert_shrine"] = new("desert_shrine", "砂漠の祠", "太陽神への祈りの場", LocationType.ReligiousSite, TerritoryId.Desert),
        ["desert_caravan"] = new("desert_caravan", "隊商宿", "旅人の休息所、情報交換の場", LocationType.Facility, TerritoryId.Desert),
        ["desert_village"] = new("desert_village", "砂の民の集落", "砂漠に住む遊牧民の集落", LocationType.Village, TerritoryId.Desert),
        ["desert_wasteland"] = new("desert_wasteland", "灼熱の砂原", "猛暑と砂嵐に苛まれる危険地帯", LocationType.Field, TerritoryId.Desert, DangerLevel: 3),
        ["desert_pyramid"] = new("desert_pyramid", "古代のピラミッド", "古代王朝の墓所、罠とミイラ", LocationType.Dungeon, TerritoryId.Desert, MinLevel: 12, DangerLevel: 4),
        ["desert_antlion"] = new("desert_antlion", "蟻地獄の巣穴", "巨大蟲の巣、地下空洞", LocationType.Dungeon, TerritoryId.Desert, MinLevel: 8, DangerLevel: 3),

        // 沼沢領
        ["swamp_town"] = new("swamp_town", "水郷の里", "沼沢地の中に浮かぶ水上集落。薬草園、毒の沼、蛙人の村がある",
            LocationType.Town, TerritoryId.Swamp,
            SubLocationIds: new[] { "swamp_village", "swamp_herb", "swamp_frog" }),
        ["swamp_village"] = new("swamp_village", "水上の村", "高床式の家が並ぶ漁村", LocationType.Village, TerritoryId.Swamp),
        ["swamp_herb"] = new("swamp_herb", "薬草園", "毒草と薬草が混在する", LocationType.Facility, TerritoryId.Swamp),
        ["swamp_frog"] = new("swamp_frog", "蛙人の村", "蛙人族の小さな集落", LocationType.Village, TerritoryId.Swamp),
        ["swamp_bog"] = new("swamp_bog", "腐敗の沼地", "瘴気が立ち込める危険な沼", LocationType.Field, TerritoryId.Swamp, DangerLevel: 3),
        ["swamp_sunken"] = new("swamp_sunken", "沈んだ遺跡", "水没した古代遺跡", LocationType.Dungeon, TerritoryId.Swamp, MinLevel: 10, DangerLevel: 4),
        ["swamp_hag"] = new("swamp_hag", "魔女の住処", "沼の奥に潜む魔女の洞窟", LocationType.Dungeon, TerritoryId.Swamp, MinLevel: 14, DangerLevel: 4),

        // 凍土領
        ["tundra_town"] = new("tundra_town", "氷壁砦", "永久凍土の中に築かれた要塞都市。狼牙の集落、氷の祭壇がある",
            LocationType.Town, TerritoryId.Tundra,
            SubLocationIds: new[] { "tundra_fort", "tundra_wolf", "tundra_altar" }),
        ["tundra_fort"] = new("tundra_fort", "氷壁砦中心部", "凍土の守り手の拠点", LocationType.Town, TerritoryId.Tundra),
        ["tundra_wolf"] = new("tundra_wolf", "狼牙の集落", "獣人狩猟民の集落", LocationType.Village, TerritoryId.Tundra),
        ["tundra_altar"] = new("tundra_altar", "氷の祭壇", "冬の精霊を祀る聖地", LocationType.ReligiousSite, TerritoryId.Tundra),
        ["tundra_plain"] = new("tundra_plain", "白銀の平原", "猛吹雪が吹き荒れる凍土", LocationType.Field, TerritoryId.Tundra, DangerLevel: 4),
        ["tundra_glacier"] = new("tundra_glacier", "氷河の裂け目", "氷に閉ざされた古代生物の墓", LocationType.Dungeon, TerritoryId.Tundra, MinLevel: 18, DangerLevel: 4),
        ["tundra_mammoth"] = new("tundra_mammoth", "マンモスの墓場", "巨獣の骨が累々と横たわる洞窟", LocationType.Dungeon, TerritoryId.Tundra, MinLevel: 20, DangerLevel: 5),

        // 湖水領
        ["lake_town"] = new("lake_town", "湖畔の都ミラージュ", "巨大湖のほとりに広がる美しい都市。漁師の浜、水の神殿、霧の島がある",
            LocationType.Town, TerritoryId.Lake,
            SubLocationIds: new[] { "lake_city", "lake_fisher", "lake_temple" }),
        ["lake_city"] = new("lake_city", "ミラージュ中心部", "湖に映る幻のような美しい街", LocationType.Town, TerritoryId.Lake),
        ["lake_fisher"] = new("lake_fisher", "漁師の浜", "新鮮な魚介類が手に入る", LocationType.Village, TerritoryId.Lake),
        ["lake_temple"] = new("lake_temple", "水の神殿", "水の精霊を祀る聖堂", LocationType.ReligiousSite, TerritoryId.Lake),
        ["lake_island"] = new("lake_island", "霧の島", "湖上に浮かぶ謎の島、幻惑の霧", LocationType.Field, TerritoryId.Lake, DangerLevel: 3),
        ["lake_underwater"] = new("lake_underwater", "湖底神殿", "水中に沈んだ古代神殿", LocationType.Dungeon, TerritoryId.Lake, MinLevel: 12, DangerLevel: 4),
        ["lake_serpent"] = new("lake_serpent", "大蛇の棲家", "湖に住む大蛇の巣", LocationType.Dungeon, TerritoryId.Lake, MinLevel: 16, DangerLevel: 5),

        // 火山領
        ["volcanic_town"] = new("volcanic_town", "溶鉄の城塞", "火山の麓に築かれたドワーフの鍛冶都市。炎の鍛冶場、火山の祠がある",
            LocationType.Town, TerritoryId.Volcanic,
            SubLocationIds: new[] { "volcanic_forge", "volcanic_mine", "volcanic_shrine" }),
        ["volcanic_forge"] = new("volcanic_forge", "炎の鍛冶場", "溶岩の熱を利用した究極の鍛冶", LocationType.Facility, TerritoryId.Volcanic),
        ["volcanic_mine"] = new("volcanic_mine", "火山鉱脈", "貴重な鉱石が眠る採掘場", LocationType.Village, TerritoryId.Volcanic),
        ["volcanic_shrine"] = new("volcanic_shrine", "火山の祠", "炎の精霊を祀る祭壇", LocationType.ReligiousSite, TerritoryId.Volcanic),
        ["volcanic_waste"] = new("volcanic_waste", "灰の荒野", "火山灰に覆われた死の大地", LocationType.Field, TerritoryId.Volcanic, DangerLevel: 4),
        ["volcanic_crater"] = new("volcanic_crater", "火口洞窟", "活火山の内部、溶岩の川が流れる", LocationType.Dungeon, TerritoryId.Volcanic, MinLevel: 20, DangerLevel: 5),
        ["volcanic_dragon"] = new("volcanic_dragon", "炎竜の巣", "火山に住む炎竜の住処", LocationType.Dungeon, TerritoryId.Volcanic, MinLevel: 25, DangerLevel: 5),

        // 聖域
        ["sacred_town"] = new("sacred_town", "光輝の聖都", "天使と聖者が守護する神聖な都市。聖堂、巡礼の道、聖なる泉がある",
            LocationType.Town, TerritoryId.Sacred,
            SubLocationIds: new[] { "sacred_cathedral", "sacred_pilgrim", "sacred_spring" }),
        ["sacred_cathedral"] = new("sacred_cathedral", "大聖堂ルミナス", "光の神の総本山", LocationType.ReligiousSite, TerritoryId.Sacred),
        ["sacred_pilgrim"] = new("sacred_pilgrim", "巡礼者の村", "聖地を目指す巡礼者の拠点", LocationType.Village, TerritoryId.Sacred),
        ["sacred_spring"] = new("sacred_spring", "聖なる泉", "万病を癒す奇跡の泉", LocationType.Facility, TerritoryId.Sacred),
        ["sacred_garden"] = new("sacred_garden", "天上の庭園", "精霊が舞う聖域の庭", LocationType.Field, TerritoryId.Sacred, DangerLevel: 2),
        ["sacred_abyss"] = new("sacred_abyss", "封印の深淵", "かつて封印された邪悪が眠る", LocationType.Dungeon, TerritoryId.Sacred, MinLevel: 22, DangerLevel: 5),
        ["sacred_trial"] = new("sacred_trial", "試練の塔", "英雄を選ぶための試練の場", LocationType.Dungeon, TerritoryId.Sacred, MinLevel: 18, DangerLevel: 4)
    };

    public static LocationDefinition Get(string id) => All[id];
    public static IReadOnlyDictionary<string, LocationDefinition> GetAll() => All;

    public static IReadOnlyList<LocationDefinition> GetByTerritory(TerritoryId territory) =>
        All.Values.Where(l => l.Territory == territory).ToList();

    /// <summary>シンボルマップ上に表示するロケーション（統合街 + ダンジョン）のみ取得</summary>
    public static IReadOnlyList<LocationDefinition> GetSymbolLocations(TerritoryId territory) =>
        All.Values.Where(l => l.Territory == territory && (l.SubLocationIds != null || l.Type == LocationType.Dungeon)).ToList();

    public static IReadOnlyList<LocationDefinition> GetDungeonsByTerritory(TerritoryId territory) =>
        All.Values.Where(l => l.Territory == territory && l.Type == LocationType.Dungeon).ToList();

    /// <summary>サブロケーション定義を取得</summary>
    public IReadOnlyList<LocationDefinition> GetSubLocations() =>
        SubLocationIds?.Select(id => All.TryGetValue(id, out var loc) ? loc : null!)
            .Where(l => l != null).Cast<LocationDefinition>().ToList()
        ?? new List<LocationDefinition>();
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
            new[] { TerritoryId.Forest, TerritoryId.Mountain, TerritoryId.Coast, TerritoryId.Lake },
            0,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.WeaponShop, FacilityType.ArmorShop, FacilityType.Inn, FacilityType.Smithy, FacilityType.Church, FacilityType.Temple, FacilityType.MagicShop, FacilityType.Library, FacilityType.Bank, FacilityType.Arena }),

        [TerritoryId.Forest] = new(TerritoryId.Forest, "森林領", "深い森に覆われた自然豊かな領地", 5, 15,
            new[] { TerritoryId.Capital, TerritoryId.Southern, TerritoryId.Swamp },
            TimeConstants.TurnsPerDay * 3,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Mountain] = new(TerritoryId.Mountain, "山岳領", "険しい山々と鉱脈が広がる領地", 10, 20,
            new[] { TerritoryId.Capital, TerritoryId.Frontier, TerritoryId.Volcanic },
            TimeConstants.TurnsPerDay * 5,
            new[] { FacilityType.AdventurerGuild, FacilityType.WeaponShop, FacilityType.Smithy, FacilityType.Inn }),

        [TerritoryId.Coast] = new(TerritoryId.Coast, "海岸領", "港湾都市と漁村が点在する沿岸地帯", 5, 15,
            new[] { TerritoryId.Capital, TerritoryId.Southern, TerritoryId.Desert },
            TimeConstants.TurnsPerDay * 2,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.ArmorShop, FacilityType.Inn, FacilityType.Bank }),

        [TerritoryId.Southern] = new(TerritoryId.Southern, "南部領", "荒野と古代遺跡が広がる辺境", 15, 25,
            new[] { TerritoryId.Forest, TerritoryId.Coast, TerritoryId.Frontier, TerritoryId.Tundra },
            TimeConstants.TurnsPerDay * 4,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Frontier] = new(TerritoryId.Frontier, "辺境領", "未開の地。危険だが宝物も眠る", 20, 30,
            new[] { TerritoryId.Mountain, TerritoryId.Southern, TerritoryId.Sacred },
            TimeConstants.TurnsPerDay * 7,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop }),

        [TerritoryId.Desert] = new(TerritoryId.Desert, "砂漠領", "灼熱の砂漠とオアシスの交易都市", 8, 15,
            new[] { TerritoryId.Coast, TerritoryId.Volcanic },
            TimeConstants.TurnsPerDay * 4,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Swamp] = new(TerritoryId.Swamp, "沼沢領", "瘴気漂う湿地帯と水上集落", 8, 15,
            new[] { TerritoryId.Forest, TerritoryId.Lake },
            TimeConstants.TurnsPerDay * 3,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Tundra] = new(TerritoryId.Tundra, "凍土領", "永久凍土に覆われた極寒の大地", 15, 20,
            new[] { TerritoryId.Southern, TerritoryId.Sacred },
            TimeConstants.TurnsPerDay * 5,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn }),

        [TerritoryId.Lake] = new(TerritoryId.Lake, "湖水領", "巨大湖とその周辺の豊かな水郷", 5, 15,
            new[] { TerritoryId.Capital, TerritoryId.Swamp },
            TimeConstants.TurnsPerDay * 2,
            new[] { FacilityType.AdventurerGuild, FacilityType.GeneralShop, FacilityType.Inn, FacilityType.Church }),

        [TerritoryId.Volcanic] = new(TerritoryId.Volcanic, "火山領", "活火山群と溶岩の大地", 18, 25,
            new[] { TerritoryId.Mountain, TerritoryId.Desert },
            TimeConstants.TurnsPerDay * 6,
            new[] { FacilityType.AdventurerGuild, FacilityType.WeaponShop, FacilityType.Smithy }),

        [TerritoryId.Sacred] = new(TerritoryId.Sacred, "聖域", "神聖な力に守られた浄化の地", 15, 25,
            new[] { TerritoryId.Frontier, TerritoryId.Tundra },
            TimeConstants.TurnsPerDay * 5,
            new[] { FacilityType.AdventurerGuild, FacilityType.Church, FacilityType.Temple, FacilityType.Inn })
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

    /// <summary>プレイヤーが手配中かどうか</summary>
    public bool IsPlayerWanted { get; set; } = false;

    /// <summary>所持金（通行料判定用、外部から設定）</summary>
    public int PlayerGold { get; set; } = 0;

    /// <summary>関所通行料（1回あたりの固定額）</summary>
    public const int BorderGateToll = 100;

    /// <summary>戦争システム参照（外部から設定）</summary>
    public FactionWarSystem? FactionWarSystem { get; set; }

    /// <summary>
    /// 隣接領地への移動が可能か。
    /// 条件: (1)隣接であること (2)手配されていないこと (3)通行料を支払えること (4)戦争状態でないこと
    /// </summary>
    public bool CanTravelTo(TerritoryId destination, int playerLevel)
    {
        var current = TerritoryDefinition.Get(CurrentTerritory);
        if (!current.AdjacentTerritories.Contains(destination))
            return false;

        // 手配中は通行不可
        if (IsPlayerWanted)
            return false;

        // 通行料を支払えるか
        if (PlayerGold < BorderGateToll)
            return false;

        // 戦争状態にある領地間は通行不可
        if (FactionWarSystem != null)
        {
            var wars = FactionWarSystem.ActiveWars;
            foreach (var war in wars)
            {
                if (war.Phase is WarPhase.Skirmish or WarPhase.Battle)
                {
                    bool involvesFrom = war.Attacker == CurrentTerritory || war.Defender == CurrentTerritory;
                    bool involvesTo = war.Attacker == destination || war.Defender == destination;
                    if (involvesFrom && involvesTo)
                        return false;
                }
            }
        }

        return true;
    }

    /// <summary>通行拒否理由を取得する（UI表示用）</summary>
    public string GetTravelDeniedReason(TerritoryId destination, int playerLevel)
    {
        var current = TerritoryDefinition.Get(CurrentTerritory);
        if (!current.AdjacentTerritories.Contains(destination))
            return "この領地には直接移動できない";

        if (IsPlayerWanted)
            return "手配中のため関所を通過できない";

        if (PlayerGold < BorderGateToll)
            return $"通行料が足りない（必要: {BorderGateToll}G、所持: {PlayerGold}G）";

        if (FactionWarSystem != null)
        {
            var wars = FactionWarSystem.ActiveWars;
            foreach (var war in wars)
            {
                if (war.Phase is WarPhase.Skirmish or WarPhase.Battle)
                {
                    bool involvesFrom = war.Attacker == CurrentTerritory || war.Defender == CurrentTerritory;
                    bool involvesTo = war.Attacker == destination || war.Defender == destination;
                    if (involvesFrom && involvesTo)
                        return $"戦争中のため通行不可（{war.Name}）";
                }
            }
        }

        return string.Empty;
    }

    /// <summary>領地間移動を実行</summary>
    public TravelResult TravelTo(TerritoryId destination, int playerLevel)
    {
        if (!CanTravelTo(destination, playerLevel))
        {
            string reason = GetTravelDeniedReason(destination, playerLevel);
            if (string.IsNullOrEmpty(reason))
                reason = "この領地には移動できない";
            return new TravelResult(false, reason, 0);
        }

        // 通行料を差し引き
        PlayerGold -= BorderGateToll;

        var destination_def = TerritoryDefinition.Get(destination);
        CurrentTerritory = destination;
        VisitedTerritories.Add(destination);
        IsOnSurface = true;

        return new TravelResult(true,
            $"{destination_def.Name}に到着した（通行料{BorderGateToll}G支払い、{destination_def.TravelTurnCost / TimeConstants.TurnsPerDay}日経過）",
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

    /// <summary>
    /// 全状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、訪問履歴は消失し、開始領地に戻る。
    /// </summary>
    public void Reset(TerritoryId startTerritory)
    {
        CurrentTerritory = startTerritory;
        VisitedTerritories.Clear();
        VisitedTerritories.Add(startTerritory);
        IsOnSurface = true;
    }

    /// <summary>旅イベントの基本発生確率（%）</summary>
    private const int BaseTravelEventChance = 30;

    /// <summary>領地間移動時のランダムイベント判定</summary>
    public TravelEvent? RollTravelEvent(TerritoryId from, TerritoryId to, IRandomProvider random)
    {
        // 基本確率を領地危険度で調整: 危険な領地ほどイベント発生率上昇
        var destDef = TerritoryDefinition.Get(to);
        int eventChance = BaseTravelEventChance + (destDef.MaxDungeonDepth / 5);
        eventChance = Math.Clamp(eventChance, 10, 60);

        if (random.Next(100) >= eventChance) return null;

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
        var destDef = TerritoryDefinition.Get(to);
        int eventChance = BaseTravelEventChance + (destDef.MaxDungeonDepth / 5);
        eventChance = Math.Clamp(eventChance, 10, 60);

        if (random.Next(100) >= eventChance) return null;

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
    /// <summary>ショップ商品定義（PoE風グリッドショップ対応）</summary>
    public record ShopItem(
        string ItemId,
        string Name,
        int BasePrice,
        int Stock,
        FacilityType ShopType,
        GridItemSize GridSize = GridItemSize.Size1x1);

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
        TerritoryId.Desert => 1.4,
        TerritoryId.Swamp => 1.2,
        TerritoryId.Tundra => 1.3,
        TerritoryId.Lake => 1.0,
        TerritoryId.Volcanic => 1.5,
        TerritoryId.Sacred => 1.1,
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
            new("potion_healing", "回復ポーション", scale(50), 10, FacilityType.GeneralShop, GridItemSize.Size1x1),
            new("food_bread", "パン", scale(20), 15, FacilityType.GeneralShop, GridItemSize.Size1x1),
            new("food_water", "水", scale(8), 20, FacilityType.GeneralShop, GridItemSize.Size1x1),
            new("material_wood", "松明", scale(10), 20, FacilityType.GeneralShop, GridItemSize.Size1x2),
            new("potion_antidote", "解毒薬", scale(40), 5, FacilityType.GeneralShop, GridItemSize.Size1x1)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("potion_healing_super", "上質回復ポーション", scale(150), 5, FacilityType.GeneralShop, GridItemSize.Size1x1));
            items.Add(new("food_clean_water", "清水", scale(30), 10, FacilityType.GeneralShop, GridItemSize.Size1x1));
        }

        if (territory == TerritoryId.Forest)
        {
            items.Add(new("material_herb", "薬草", scale(30), 10, FacilityType.GeneralShop, GridItemSize.Size1x1));
        }

        return items;
    }

    private static List<ShopItem> GenerateWeaponShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("weapon_iron_sword", "鉄の剣", scale(200), 3, FacilityType.WeaponShop, GridItemSize.Size1x2),
            new("weapon_battle_axe", "鉄の斧", scale(250), 2, FacilityType.WeaponShop, GridItemSize.Size1x2),
            new("weapon_war_hammer", "鉄の鎚", scale(220), 2, FacilityType.WeaponShop, GridItemSize.Size1x2),
            new("weapon_dagger", "ダガー", scale(100), 5, FacilityType.WeaponShop, GridItemSize.Size1x1),
            new("weapon_short_bow", "短弓", scale(180), 2, FacilityType.WeaponShop, GridItemSize.Size2x3),
            new("weapon_wooden_staff", "木の杖", scale(150), 2, FacilityType.WeaponShop, GridItemSize.Size1x2)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("weapon_steel_sword", "鋼の剣", scale(500), 2, FacilityType.WeaponShop, GridItemSize.Size1x2));
        }

        if (territory == TerritoryId.Mountain)
        {
            items.Add(new("weapon_mithril_dagger", "ミスリルダガー", scale(800), 1, FacilityType.WeaponShop, GridItemSize.Size1x1));
        }

        return items;
    }

    private static List<ShopItem> GenerateArmorShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("armor_leather", "革鎧", scale(150), 3, FacilityType.ArmorShop, GridItemSize.Size2x3),
            new("armor_chainmail", "鎖帷子", scale(400), 2, FacilityType.ArmorShop, GridItemSize.Size2x3),
            new("shield_iron", "鉄の盾", scale(200), 2, FacilityType.ArmorShop, GridItemSize.Size2x2),
            new("armor_wizard_robe", "ローブ", scale(100), 3, FacilityType.ArmorShop, GridItemSize.Size2x3),
            new("armor_iron_helm", "鉄の兜", scale(150), 2, FacilityType.ArmorShop, GridItemSize.Size2x2)
        };

        if (playerLevel >= 10)
        {
            items.Add(new("armor_plate", "鋼の鎧", scale(800), 1, FacilityType.ArmorShop, GridItemSize.Size2x3));
        }

        return items;
    }

    private static List<ShopItem> GenerateMagicShopItems(TerritoryId territory, int playerLevel, Func<int, int> scale)
    {
        var items = new List<ShopItem>
        {
            new("potion_mana", "マナポーション", scale(80), 10, FacilityType.MagicShop, GridItemSize.Size1x1),
            new("scroll_teleport", "転移の巻物", scale(200), 2, FacilityType.MagicShop, GridItemSize.Size1x1),
            new("scroll_identify", "識別の巻物", scale(100), 5, FacilityType.MagicShop, GridItemSize.Size1x1),
            new("scroll_fireball", "火炎の巻物", scale(500), 1, FacilityType.MagicShop, GridItemSize.Size1x1),
            new("material_magic_crystal", "魔力の結晶", scale(300), 3, FacilityType.MagicShop, GridItemSize.Size1x1)
        };

        if (playerLevel >= 15)
        {
            items.Add(new("potion_mana_minor", "上質マナポーション", scale(200), 5, FacilityType.MagicShop, GridItemSize.Size1x1));
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
    public RandomEvent? RollTerritoryEvent(int depth, TerritoryId territory, IRandomProvider random, int karmaValue = 0, float reputationModifier = 1.0f)
    {
        // BP-2: カルマ/評判に基づくイベント発生率修正
        int baseChance = 12;
        // 善カルマ(正): 良いイベント増加、悪カルマ(負): 悪いイベント増加
        float karmaModifier = 1.0f + Math.Abs(karmaValue) * 0.001f;
        int adjustedChance = Math.Clamp((int)(baseChance * karmaModifier * reputationModifier), 5, 30);
        if (random.Next(100) >= adjustedChance) return null;

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
            TerritoryId.Capital => "薬草",
            TerritoryId.Forest => "薬草",
            TerritoryId.Mountain => "鉱石",
            TerritoryId.Coast => "珊瑚",
            TerritoryId.Southern => "砂金",
            TerritoryId.Frontier => "魔石",
            TerritoryId.Desert => "砂鉄",
            TerritoryId.Swamp => "毒草",
            TerritoryId.Tundra => "氷晶",
            TerritoryId.Lake => "真珠",
            TerritoryId.Volcanic => "溶岩石",
            TerritoryId.Sacred => "聖水晶",
            _ => "素材"
        };
        int quantity = 1 + random.Next(3) + depth / 5;
        return new EventResult(true, $"{material}を{quantity}個採取した！");
    }

    /// <summary>AF-1: NPC遭遇イベントの結果</summary>
    public EventResult ResolveNpcEncounter(int depth, IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 40)
            return new EventResult(true, "旅の冒険者と情報を交換した。周囲の地形が明らかになった！");
        if (roll < 70)
        {
            int gold = 20 + depth * 10 + random.Next(depth * 5 + 1);
            return new EventResult(true, $"親切なNPCから{gold}ゴールドの報酬を得た", GoldAmount: gold);
        }
        if (roll < 90)
            return new EventResult(true, "賢者から助言を受けた。経験値を獲得した");
        return new EventResult(false, "不審な人物だった…何も起こらなかった");
    }

    /// <summary>AF-2: 商人遭遇イベントの結果</summary>
    public EventResult ResolveMerchantEncounter(int depth, IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 50)
            return new EventResult(true, "旅の商人から珍しいアイテムを購入できた！");
        if (roll < 80)
        {
            int gold = 30 + depth * 15 + random.Next(depth * 10 + 1);
            return new EventResult(true, $"商人と取引して{gold}ゴールドの利益を得た", GoldAmount: gold);
        }
        return new EventResult(true, "商人は特に良い品物を持っていなかった");
    }

    /// <summary>AF-3: 待ち伏せイベントの結果</summary>
    public EventResult ResolveAmbushEvent(int depth, IRandomProvider random)
    {
        int roll = random.Next(100);
        if (roll < 50)
        {
            int damage = 10 + depth * 3 + random.Next(depth * 2 + 1);
            return new EventResult(false, $"待ち伏せされた！{damage}ダメージを受けた！", DamageAmount: damage);
        }
        if (roll < 75)
        {
            int damage = 5 + depth;
            int gold = 40 + depth * 20 + random.Next(depth * 15 + 1);
            return new EventResult(true, $"待ち伏せを返り討ちにした！{gold}ゴールドを奪った（{damage}ダメージ）",
                DamageAmount: damage, GoldAmount: gold);
        }
        return new EventResult(true, "気配を察知して待ち伏せを回避した！");
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
