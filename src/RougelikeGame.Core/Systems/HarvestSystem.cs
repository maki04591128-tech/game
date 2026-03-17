using RougelikeGame.Core.AI;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 剥ぎ取り結果
/// </summary>
public record HarvestResult(List<(string ItemId, int Quantity)> Materials, string Message);

/// <summary>
/// 剥ぎ取りシステム - 敵撃破時に種族に応じた素材を追加ドロップする
/// </summary>
public static class HarvestSystem
{
    /// <summary>種族別素材テーブル（ItemId, 基本ドロップ率）</summary>
    private static readonly Dictionary<MonsterRace, List<(string ItemId, double BaseRate)>> _harvestTable = new()
    {
        [MonsterRace.Beast] = new()
        {
            ("material_beast_hide", 0.50),
            ("material_beast_fang", 0.30)
        },
        [MonsterRace.Undead] = new()
        {
            ("material_bone_fragment", 0.45),
            ("material_cursed_essence", 0.15)
        },
        [MonsterRace.Dragon] = new()
        {
            ("material_dragon_scale", 0.20),
            ("material_dragon_fang", 0.15)
        },
        [MonsterRace.Insect] = new()
        {
            ("material_insect_shell", 0.45),
            ("material_venom_sac", 0.25)
        },
        [MonsterRace.Plant] = new()
        {
            ("material_herb", 0.55),
            ("material_wood", 0.35)
        },
        [MonsterRace.Demon] = new()
        {
            ("material_demon_horn", 0.20),
            ("material_dark_crystal", 0.12)
        },
        [MonsterRace.Spirit] = new()
        {
            ("material_spirit_essence", 0.25),
            ("material_elemental_core", 0.10)
        },
        [MonsterRace.Construct] = new()
        {
            ("material_golem_core", 0.15),
            ("material_iron_fragment", 0.40)
        },
        [MonsterRace.Amorphous] = new()
        {
            ("material_slime_gel", 0.50),
            ("material_magic_crystal", 0.10)
        },
        [MonsterRace.Humanoid] = new()
        {
            ("material_equipment_fragment", 0.30)
        },
    };

    /// <summary>
    /// 剥ぎ取り可能か判定
    /// Spiritは特殊条件（基本は可能だがドロップ率が低い）
    /// </summary>
    public static bool CanHarvest(MonsterRace race)
    {
        return _harvestTable.ContainsKey(race);
    }

    /// <summary>
    /// 剥ぎ取りを実行
    /// 種族×ランクで素材内容・数量を決定
    /// </summary>
    public static HarvestResult Harvest(MonsterRace race, EnemyRank rank, IRandomProvider random)
    {
        var materials = new List<(string ItemId, int Quantity)>();

        if (!_harvestTable.TryGetValue(race, out var entries))
            return new HarvestResult(materials, "剥ぎ取れる素材はなかった。");

        double rankMultiplier = GetRankDropMultiplier(rank);
        int maxQuantityBonus = GetRankQuantityBonus(rank);

        foreach (var (itemId, baseRate) in entries)
        {
            double effectiveRate = Math.Min(1.0, baseRate * rankMultiplier);
            if (random.NextDouble() < effectiveRate)
            {
                // 基本1個 + ランクボーナス分の追加数量
                int quantity = 1 + (maxQuantityBonus > 0 ? random.Next(0, maxQuantityBonus + 1) : 0);
                materials.Add((itemId, quantity));
            }
        }

        if (materials.Count == 0)
            return new HarvestResult(materials, $"{GetRaceName(race)}から剥ぎ取れる素材はなかった。");

        string materialNames = string.Join("、", materials.Select(m => $"{GetItemName(m.ItemId)}×{m.Quantity}"));
        return new HarvestResult(materials, $"{GetRaceName(race)}から{materialNames}を入手した！");
    }

    /// <summary>
    /// 指定種族で取得可能な素材リストを返す
    /// </summary>
    public static IReadOnlyList<string> GetHarvestableItems(MonsterRace race)
    {
        if (!_harvestTable.TryGetValue(race, out var entries))
            return Array.Empty<string>();

        return entries.Select(e => e.ItemId).ToList();
    }

    /// <summary>ランク別ドロップ率倍率</summary>
    private static double GetRankDropMultiplier(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 1.0,
        EnemyRank.Elite => 1.3,
        EnemyRank.Rare => 1.6,
        EnemyRank.Boss => 2.0,
        EnemyRank.HiddenBoss => 2.5,
        _ => 1.0
    };

    /// <summary>ランク別追加数量ボーナス</summary>
    private static int GetRankQuantityBonus(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 0,
        EnemyRank.Elite => 1,
        EnemyRank.Rare => 2,
        EnemyRank.Boss => 3,
        EnemyRank.HiddenBoss => 5,
        _ => 0
    };

    /// <summary>種族の日本語名</summary>
    private static string GetRaceName(MonsterRace race) => race switch
    {
        MonsterRace.Beast => "獣",
        MonsterRace.Humanoid => "人型",
        MonsterRace.Amorphous => "不定形",
        MonsterRace.Undead => "不死",
        MonsterRace.Demon => "悪魔",
        MonsterRace.Dragon => "竜",
        MonsterRace.Plant => "植物",
        MonsterRace.Insect => "昆虫",
        MonsterRace.Spirit => "精霊",
        MonsterRace.Construct => "構造体",
        _ => "不明"
    };

    /// <summary>素材IDの日本語名</summary>
    private static string GetItemName(string itemId) => itemId switch
    {
        "material_beast_hide" => "毛皮",
        "material_beast_fang" => "牙",
        "material_bone_fragment" => "骨片",
        "material_cursed_essence" => "呪いのエッセンス",
        "material_dragon_scale" => "竜鱗",
        "material_dragon_fang" => "竜牙",
        "material_insect_shell" => "甲殻",
        "material_venom_sac" => "毒嚢",
        "material_herb" => "薬草",
        "material_wood" => "樹液",
        "material_demon_horn" => "魔角",
        "material_dark_crystal" => "暗黒結晶",
        "material_spirit_essence" => "精霊のエッセンス",
        "material_elemental_core" => "元素核",
        "material_golem_core" => "核",
        "material_iron_fragment" => "鉄片",
        "material_slime_gel" => "ゼリー",
        "material_magic_crystal" => "魔力結晶",
        "material_equipment_fragment" => "装備品の欠片",
        _ => itemId
    };
}
