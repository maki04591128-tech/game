namespace RougelikeGame.Core;

/// <summary>
/// 種族・素性の組み合わせから開始マップ名を決定する
/// マップ名は管理用の識別子であり、現時点では同じ構造のマップを生成するが
/// 後ほどマップ名ごとに異なる構造・演出を実装可能にする
/// </summary>
public static class StartingMapResolver
{
    /// <summary>
    /// 種族と素性から開始マップ名を決定する
    /// </summary>
    public static string Resolve(Race race, Background background)
    {
        // 素性を優先的に判定（特定の素性は固有の開始地がある）
        var bgMap = GetBackgroundMap(background);
        if (bgMap != null) return bgMap;

        // 素性が汎用の場合は種族で判定
        return GetRaceMap(race);
    }

    /// <summary>
    /// マップ名の日本語表示名を返す
    /// </summary>
    public static string GetDisplayName(string mapName) => mapName switch
    {
        "capital_guild" => "王都・冒険者ギルド",
        "capital_barracks" => "王都・兵舎",
        "capital_academy" => "王都・学院",
        "capital_market" => "王都・市場通り",
        "capital_slums" => "王都・貧民街",
        "capital_manor" => "王都・貴族邸",
        "capital_cathedral" => "王都・大聖堂",
        "capital_prison" => "王都・牢獄",
        "capital_monastery" => "王都・修道院",
        "forest_village" => "森の集落",
        "mountain_hold" => "山岳砦",
        "coast_port" => "海岸港町",
        "underground_ruins" => "地下遺跡",
        "dark_sanctuary" => "暗黒聖域",
        "fallen_temple" => "堕天の神殿",
        "swamp_den" => "沼地の洞窟",
        "wanderer_camp" => "流浪者の野営地",
        "debug_arena" => "デバッグアリーナ",
        _ => mapName
    };

    /// <summary>
    /// マップ名から推奨される開始領地を返す
    /// </summary>
    public static TerritoryId GetStartingTerritory(string mapName) => mapName switch
    {
        "forest_village" => TerritoryId.Forest,
        "mountain_hold" => TerritoryId.Mountain,
        "coast_port" => TerritoryId.Coast,
        _ => TerritoryId.Capital
    };

    private static string? GetBackgroundMap(Background background) => background switch
    {
        Background.Soldier => "capital_barracks",
        Background.Scholar => "capital_academy",
        Background.Merchant => "capital_market",
        Background.Peasant => "capital_slums",
        Background.Noble => "capital_manor",
        Background.Criminal => "capital_prison",
        Background.Priest => "capital_cathedral",
        Background.Penitent => "capital_monastery",
        Background.Wanderer => "wanderer_camp",
        // Adventurerは汎用 → 種族で判定
        Background.Adventurer => null,
        _ => null
    };

    private static string GetRaceMap(Race race) => race switch
    {
        Race.Elf => "forest_village",
        Race.Dwarf => "mountain_hold",
        Race.Halfling => "coast_port",
        Race.Orc => "mountain_hold",
        Race.Beastfolk => "forest_village",
        Race.Undead => "underground_ruins",
        Race.Demon => "dark_sanctuary",
        Race.FallenAngel => "fallen_temple",
        Race.Slime => "swamp_den",
        // Human及びデフォルト → 王都ギルド
        Race.Human => "capital_guild",
        _ => "capital_guild"
    };
}
