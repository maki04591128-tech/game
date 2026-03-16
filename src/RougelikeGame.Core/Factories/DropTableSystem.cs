using RougelikeGame.Core.AI;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Factories;

/// <summary>
/// ドロップテーブルエントリ
/// </summary>
public record DropTableEntry(string ItemId, double DropRate, int MinQuantity = 1, int MaxQuantity = 1, ItemGrade MinGrade = ItemGrade.Standard);

/// <summary>
/// ドロップテーブル定義
/// </summary>
public record DropTable(string TableId, List<DropTableEntry> Entries, int GoldMin = 0, int GoldMax = 0);

/// <summary>
/// ドロップ結果
/// </summary>
public record DropResult(List<Item> Items, int Gold);

/// <summary>
/// ドロップテーブル管理・戦利品生成システム
/// </summary>
public static class DropTableSystem
{
    private static readonly Dictionary<string, DropTable> _tables = new();

    static DropTableSystem()
    {
        RegisterDefaultTables();
    }

    /// <summary>テーブルを登録</summary>
    public static void RegisterTable(DropTable table)
    {
        _tables[table.TableId] = table;
    }

    /// <summary>テーブルを取得</summary>
    public static DropTable? GetTable(string tableId)
    {
        return _tables.GetValueOrDefault(tableId);
    }

    /// <summary>全テーブルIDを取得</summary>
    public static IReadOnlyList<string> GetAllTableIds()
    {
        return _tables.Keys.ToList();
    }

    /// <summary>
    /// ドロップテーブルからアイテムとゴールドを生成
    /// </summary>
    public static DropResult GenerateLoot(string tableId, int depth, EnemyRank rank, IRandomProvider random)
    {
        var items = new List<Item>();
        int gold = 0;

        if (!_tables.TryGetValue(tableId, out var table))
            return new DropResult(items, 0);

        // ゴールド生成
        int goldMin = (int)(BalanceConfig.GetGoldDropMin(depth) * BalanceConfig.GetRankGoldMultiplier(rank));
        int goldMax = (int)(BalanceConfig.GetGoldDropMax(depth) * BalanceConfig.GetRankGoldMultiplier(rank));
        if (table.GoldMin > 0 || table.GoldMax > 0)
        {
            goldMin = Math.Max(goldMin, table.GoldMin);
            goldMax = Math.Max(goldMax, table.GoldMax);
        }
        gold = random.Next(goldMin, goldMax + 1);

        // アイテムドロップ
        double rankBonus = BalanceConfig.GetRankDropBonus(rank);
        foreach (var entry in table.Entries)
        {
            double effectiveRate = entry.DropRate * rankBonus;
            if (random.NextDouble() < effectiveRate)
            {
                int quantity = entry.MinQuantity == entry.MaxQuantity
                    ? entry.MinQuantity
                    : random.Next(entry.MinQuantity, entry.MaxQuantity + 1);

                for (int i = 0; i < quantity; i++)
                {
                    var item = ItemDefinitions.Create(entry.ItemId);
                    if (item != null)
                        items.Add(item);
                }
            }
        }

        return new DropResult(items, gold);
    }

    /// <summary>
    /// スケーリングされた敵ステータスを計算
    /// </summary>
    public static Stats GetScaledStats(Stats baseStats, int depth, EnemyRank rank)
    {
        double depthMult = BalanceConfig.GetDepthStatMultiplier(depth);
        double rankMult = BalanceConfig.GetRankStatMultiplier(rank);
        double totalMult = depthMult * rankMult;

        return new Stats(
            (int)(baseStats.Strength * totalMult),
            (int)(baseStats.Vitality * totalMult),
            (int)(baseStats.Agility * totalMult),
            (int)(baseStats.Dexterity * totalMult),
            (int)(baseStats.Intelligence * totalMult),
            (int)(baseStats.Mind * totalMult),
            (int)(baseStats.Luck * totalMult),
            (int)(baseStats.Charisma * totalMult),
            (int)(baseStats.Perception * totalMult)
        );
    }

    #region Default Drop Tables

    /// <summary>種族別ボーナスドロップテーブル</summary>
    private static readonly Dictionary<MonsterRace, List<DropTableEntry>> _raceBonusDrops = new()
    {
        [MonsterRace.Beast] = new()
        {
            new("material_beast_hide", 0.3),
            new("material_beast_fang", 0.2)
        },
        [MonsterRace.Undead] = new()
        {
            new("material_bone_fragment", 0.35),
            new("material_cursed_essence", 0.1)
        },
        [MonsterRace.Dragon] = new()
        {
            new("material_dragon_scale", 0.15),
            new("material_dragon_fang", 0.1)
        },
        [MonsterRace.Insect] = new()
        {
            new("material_insect_shell", 0.3),
            new("material_venom_sac", 0.2)
        },
        [MonsterRace.Plant] = new()
        {
            new("material_herb", 0.4),
            new("material_wood", 0.25)
        },
        [MonsterRace.Demon] = new()
        {
            new("material_demon_horn", 0.15),
            new("material_dark_crystal", 0.1)
        },
        [MonsterRace.Spirit] = new()
        {
            new("material_spirit_essence", 0.2),
            new("material_elemental_core", 0.08)
        },
        [MonsterRace.Construct] = new()
        {
            new("material_golem_core", 0.1),
            new("material_iron_fragment", 0.3)
        },
        [MonsterRace.Amorphous] = new()
        {
            new("material_slime_gel", 0.4),
            new("material_magic_crystal", 0.08)
        },
    };

    /// <summary>
    /// 種族別ボーナスドロップエントリリストを取得
    /// </summary>
    public static IReadOnlyList<DropTableEntry> GetRaceBonusDrops(MonsterRace race)
    {
        return _raceBonusDrops.TryGetValue(race, out var entries)
            ? entries
            : Array.Empty<DropTableEntry>();
    }

    /// <summary>
    /// ドロップテーブルからアイテムとゴールドを生成（種族別ボーナス付き）
    /// </summary>
    public static DropResult GenerateLoot(string tableId, int depth, EnemyRank rank, IRandomProvider random, MonsterRace? race = null)
    {
        var baseResult = GenerateLoot(tableId, depth, rank, random);

        if (race == null || !_raceBonusDrops.TryGetValue(race.Value, out var bonusEntries))
            return baseResult;

        var items = new List<Item>(baseResult.Items);
        double rankBonus = BalanceConfig.GetRankDropBonus(rank);

        foreach (var entry in bonusEntries)
        {
            double effectiveRate = entry.DropRate * rankBonus;
            if (random.NextDouble() < effectiveRate)
            {
                int quantity = entry.MinQuantity == entry.MaxQuantity
                    ? entry.MinQuantity
                    : random.Next(entry.MinQuantity, entry.MaxQuantity + 1);

                for (int i = 0; i < quantity; i++)
                {
                    var item = ItemDefinitions.Create(entry.ItemId);
                    if (item != null)
                        items.Add(item);
                }
            }
        }

        return new DropResult(items, baseResult.Gold);
    }

    #region Default Drop Tables Definitions

    private static void RegisterDefaultTables()
    {
        // スライム
        RegisterTable(new DropTable("drop_slime",
            new List<DropTableEntry>
            {
                new("potion_healing", 0.30),
                new("food_bread", 0.15)
            },
            GoldMin: 1, GoldMax: 5));

        // ゴブリン
        RegisterTable(new DropTable("drop_goblin",
            new List<DropTableEntry>
            {
                new("weapon_short_sword", 0.10),
                new("armor_leather", 0.08),
                new("potion_healing", 0.20),
                new("food_bread", 0.15)
            },
            GoldMin: 3, GoldMax: 12));

        // スケルトン
        RegisterTable(new DropTable("drop_skeleton",
            new List<DropTableEntry>
            {
                new("weapon_iron_sword", 0.08),
                new("armor_chainmail", 0.05),
                new("scroll_identify", 0.10)
            },
            GoldMin: 5, GoldMax: 15));

        // オーク
        RegisterTable(new DropTable("drop_orc",
            new List<DropTableEntry>
            {
                new("weapon_battle_axe", 0.10),
                new("armor_chainmail", 0.08),
                new("potion_healing", 0.25),
                new("food_cooked_meat", 0.20)
            },
            GoldMin: 8, GoldMax: 25));

        // 大蜘蛛
        RegisterTable(new DropTable("drop_spider",
            new List<DropTableEntry>
            {
                new("potion_antidote", 0.30),
                new("material_spider_silk", 0.40)
            },
            GoldMin: 2, GoldMax: 8));

        // ダークメイジ
        RegisterTable(new DropTable("drop_dark_mage",
            new List<DropTableEntry>
            {
                new("scroll_fireball", 0.15),
                new("scroll_lightning", 0.12),
                new("potion_mana", 0.25),
                new("scroll_identify", 0.20)
            },
            GoldMin: 10, GoldMax: 30));

        // 森の敵
        RegisterTable(new DropTable("drop_forest",
            new List<DropTableEntry>
            {
                new("food_fruit", 0.30),
                new("potion_healing", 0.20),
                new("material_wood", 0.25)
            },
            GoldMin: 3, GoldMax: 10));

        // 山の敵
        RegisterTable(new DropTable("drop_mountain",
            new List<DropTableEntry>
            {
                new("material_iron_ore", 0.30),
                new("weapon_war_hammer", 0.08),
                new("armor_iron_helm", 0.10)
            },
            GoldMin: 8, GoldMax: 20));

        // 海岸の敵
        RegisterTable(new DropTable("drop_coast",
            new List<DropTableEntry>
            {
                new("food_cooked_meat", 0.20),
                new("potion_healing", 0.25),
                new("material_pearl", 0.15)
            },
            GoldMin: 5, GoldMax: 18));

        // 南方の敵
        RegisterTable(new DropTable("drop_southern",
            new List<DropTableEntry>
            {
                new("scroll_remove_curse", 0.12),
                new("potion_cure_all", 0.10),
                new("material_ancient_relic", 0.08)
            },
            GoldMin: 12, GoldMax: 35));

        // 辺境の敵
        RegisterTable(new DropTable("drop_frontier",
            new List<DropTableEntry>
            {
                new("weapon_greatsword", 0.06),
                new("potion_healing_super", 0.12),
                new("scroll_enchant", 0.08),
                new("accessory_protection_amulet", 0.04)
            },
            GoldMin: 15, GoldMax: 50));

        // ボス共通
        RegisterTable(new DropTable("drop_boss",
            new List<DropTableEntry>
            {
                new("potion_healing_super", 0.80),
                new("scroll_enchant", 0.30),
                new("accessory_protection_amulet", 0.20),
                new("accessory_speed_cloak", 0.15)
            },
            GoldMin: 100, GoldMax: 500));

        // 隠しボス
        RegisterTable(new DropTable("drop_hidden_boss",
            new List<DropTableEntry>
            {
                new("potion_healing_super", 1.00),
                new("scroll_enchant", 0.50),
                new("accessory_protection_amulet", 0.40),
                new("accessory_speed_cloak", 0.30),
                new("accessory_iron_ring", 0.25)
            },
            GoldMin: 500, GoldMax: 2000));
    }

    #endregion

    #endregion
}
