using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// ダンジョン特徴定義
/// </summary>
public record DungeonFeatureDefinition(
    DungeonFeatureType Type,
    string Name,
    string Description,
    MonsterRace[] CommonRaces,
    Element DominantElement,
    float TrapDensity,
    float LootMultiplier,
    int MinDepth,
    TerritoryId[] Territories
);

/// <summary>
/// ダンジョン特徴別ランダム生成パラメータ
/// </summary>
public record DungeonFeatureParams(
    DungeonFeatureType FeatureType,
    int RoomMinSize,
    int RoomMaxSize,
    float CorridorTwistChance,
    float SpecialRoomChance,
    float WaterTileChance,
    float LavaTileChance,
    int EnemyDensity,
    float TrapChance
);

/// <summary>
/// ダンジョン特徴別ランダム生成システム
/// </summary>
public static class DungeonFeatureGenerator
{
    private static readonly Dictionary<DungeonFeatureType, DungeonFeatureDefinition> _definitions = new();
    private static readonly Dictionary<DungeonFeatureType, DungeonFeatureParams> _params = new();

    static DungeonFeatureGenerator()
    {
        RegisterDefaultDefinitions();
        RegisterDefaultParams();
    }

    #region Definitions

    private static void RegisterDefaultDefinitions()
    {
        _definitions[DungeonFeatureType.Standard] = new(
            DungeonFeatureType.Standard, "標準ダンジョン", "一般的なダンジョン",
            new[] { MonsterRace.Humanoid, MonsterRace.Beast },
            Element.None, 0.1f, 1.0f, 1,
            new[] { TerritoryId.Capital });

        _definitions[DungeonFeatureType.Cave] = new(
            DungeonFeatureType.Cave, "洞窟", "自然にできた洞窟。獣や虫が棲む。",
            new[] { MonsterRace.Beast, MonsterRace.Insect, MonsterRace.Amorphous },
            Element.Earth, 0.08f, 0.9f, 1,
            new[] { TerritoryId.Forest, TerritoryId.Mountain });

        _definitions[DungeonFeatureType.Ruins] = new(
            DungeonFeatureType.Ruins, "遺跡", "古代文明の遺跡。構造体や不死者が守る。",
            new[] { MonsterRace.Construct, MonsterRace.Undead, MonsterRace.Spirit },
            Element.None, 0.15f, 1.5f, 5,
            new[] { TerritoryId.Southern, TerritoryId.Frontier });

        _definitions[DungeonFeatureType.Sewer] = new(
            DungeonFeatureType.Sewer, "下水道", "都市の地下に広がる下水道。",
            new[] { MonsterRace.Amorphous, MonsterRace.Insect, MonsterRace.Beast },
            Element.Poison, 0.12f, 0.8f, 1,
            new[] { TerritoryId.Capital });

        _definitions[DungeonFeatureType.Mine] = new(
            DungeonFeatureType.Mine, "鉱山", "廃坑となった鉱山。鉱石が眠る。",
            new[] { MonsterRace.Construct, MonsterRace.Insect, MonsterRace.Humanoid },
            Element.Earth, 0.10f, 1.3f, 3,
            new[] { TerritoryId.Mountain });

        _definitions[DungeonFeatureType.Crypt] = new(
            DungeonFeatureType.Crypt, "墓地", "不死者が跋扈する地下墓地。",
            new[] { MonsterRace.Undead, MonsterRace.Spirit, MonsterRace.Demon },
            Element.Dark, 0.18f, 1.4f, 5,
            new[] { TerritoryId.Southern, TerritoryId.Capital });

        _definitions[DungeonFeatureType.Temple] = new(
            DungeonFeatureType.Temple, "神殿", "古の神を祀る神殿。",
            new[] { MonsterRace.Construct, MonsterRace.Spirit, MonsterRace.Demon },
            Element.Holy, 0.20f, 1.8f, 8,
            new[] { TerritoryId.Southern, TerritoryId.Frontier });

        _definitions[DungeonFeatureType.IceCavern] = new(
            DungeonFeatureType.IceCavern, "氷の洞窟", "凍てついた洞窟。視界が制限される。",
            new[] { MonsterRace.Beast, MonsterRace.Spirit, MonsterRace.Dragon },
            Element.Ice, 0.10f, 1.2f, 5,
            new[] { TerritoryId.Mountain, TerritoryId.Frontier });

        _definitions[DungeonFeatureType.Volcanic] = new(
            DungeonFeatureType.Volcanic, "火山", "灼熱の火山洞窟。溶岩が流れる。",
            new[] { MonsterRace.Dragon, MonsterRace.Demon, MonsterRace.Construct },
            Element.Fire, 0.15f, 1.6f, 10,
            new[] { TerritoryId.Frontier });

        _definitions[DungeonFeatureType.Forest] = new(
            DungeonFeatureType.Forest, "森林迷宮", "鬱蒼とした森の迷宮。",
            new[] { MonsterRace.Plant, MonsterRace.Beast, MonsterRace.Insect },
            Element.Earth, 0.05f, 1.0f, 1,
            new[] { TerritoryId.Forest });
    }

    private static void RegisterDefaultParams()
    {
        _params[DungeonFeatureType.Standard] = new(
            DungeonFeatureType.Standard, 4, 10, 0.1f, 0.15f, 0f, 0f, 5, 0.1f);
        _params[DungeonFeatureType.Cave] = new(
            DungeonFeatureType.Cave, 3, 12, 0.3f, 0.10f, 0.05f, 0f, 6, 0.08f);
        _params[DungeonFeatureType.Ruins] = new(
            DungeonFeatureType.Ruins, 5, 14, 0.05f, 0.25f, 0f, 0f, 4, 0.20f);
        _params[DungeonFeatureType.Sewer] = new(
            DungeonFeatureType.Sewer, 3, 8, 0.2f, 0.10f, 0.15f, 0f, 7, 0.12f);
        _params[DungeonFeatureType.Mine] = new(
            DungeonFeatureType.Mine, 4, 10, 0.15f, 0.20f, 0f, 0f, 5, 0.10f);
        _params[DungeonFeatureType.Crypt] = new(
            DungeonFeatureType.Crypt, 4, 8, 0.05f, 0.30f, 0f, 0f, 6, 0.22f);
        _params[DungeonFeatureType.Temple] = new(
            DungeonFeatureType.Temple, 6, 16, 0.02f, 0.35f, 0f, 0f, 4, 0.25f);
        _params[DungeonFeatureType.IceCavern] = new(
            DungeonFeatureType.IceCavern, 4, 12, 0.25f, 0.15f, 0.10f, 0f, 5, 0.10f);
        _params[DungeonFeatureType.Volcanic] = new(
            DungeonFeatureType.Volcanic, 5, 14, 0.20f, 0.20f, 0f, 0.20f, 6, 0.18f);
        _params[DungeonFeatureType.Forest] = new(
            DungeonFeatureType.Forest, 3, 10, 0.35f, 0.10f, 0.03f, 0f, 7, 0.05f);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ダンジョン特徴定義を取得
    /// </summary>
    public static DungeonFeatureDefinition? GetDefinition(DungeonFeatureType type)
    {
        return _definitions.GetValueOrDefault(type);
    }

    /// <summary>
    /// ダンジョン特徴パラメータを取得
    /// </summary>
    public static DungeonFeatureParams? GetParams(DungeonFeatureType type)
    {
        return _params.GetValueOrDefault(type);
    }

    /// <summary>
    /// 全ダンジョン特徴定義を取得
    /// </summary>
    public static IReadOnlyList<DungeonFeatureDefinition> GetAllDefinitions()
    {
        return _definitions.Values.ToList();
    }

    /// <summary>
    /// 領地に応じたダンジョン特徴をランダムに選択
    /// </summary>
    public static DungeonFeatureType SelectFeatureForTerritory(TerritoryId territory, int depth, IRandomProvider random)
    {
        var candidates = _definitions.Values
            .Where(d => d.Territories.Contains(territory) && depth >= d.MinDepth)
            .ToList();

        if (candidates.Count == 0)
            return DungeonFeatureType.Standard;

        return candidates[random.Next(candidates.Count)].Type;
    }

    /// <summary>
    /// ダンジョン特徴から出現する敵の種族を取得
    /// </summary>
    public static IReadOnlyList<MonsterRace> GetCommonRaces(DungeonFeatureType type)
    {
        if (_definitions.TryGetValue(type, out var def))
            return def.CommonRaces;
        return Array.Empty<MonsterRace>();
    }

    /// <summary>
    /// ダンジョン特徴に応じた敵密度を取得
    /// </summary>
    public static int GetEnemyDensity(DungeonFeatureType type)
    {
        if (_params.TryGetValue(type, out var p))
            return p.EnemyDensity;
        return 5;
    }

    /// <summary>
    /// ダンジョン特徴に応じた罠発生率を取得
    /// </summary>
    public static float GetTrapChance(DungeonFeatureType type)
    {
        if (_params.TryGetValue(type, out var p))
            return p.TrapChance;
        return 0.1f;
    }

    /// <summary>
    /// ダンジョン特徴に応じた戦利品倍率を取得
    /// </summary>
    public static float GetLootMultiplier(DungeonFeatureType type)
    {
        if (_definitions.TryGetValue(type, out var def))
            return def.LootMultiplier;
        return 1.0f;
    }

    /// <summary>
    /// ダンジョン特徴に応じた支配属性を取得
    /// </summary>
    public static Element GetDominantElement(DungeonFeatureType type)
    {
        if (_definitions.TryGetValue(type, out var def))
            return def.DominantElement;
        return Element.None;
    }

    /// <summary>
    /// ダンジョン特徴タイプの日本語名を取得
    /// </summary>
    public static string GetFeatureName(DungeonFeatureType type)
    {
        if (_definitions.TryGetValue(type, out var def))
            return def.Name;
        return "不明";
    }

    #endregion
}
