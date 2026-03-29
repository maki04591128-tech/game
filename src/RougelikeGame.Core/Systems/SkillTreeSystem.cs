namespace RougelikeGame.Core.Systems;

/// <summary>
/// スキルノード定義
/// </summary>
public record SkillNodeDefinition(
    string Id,
    string Name,
    string Description,
    SkillNodeType NodeType,
    CharacterClass? RequiredClass,
    int PointCost,
    string[] Prerequisites,
    Dictionary<string, int> StatBonuses,
    string? KeystoneDownside = null,
    Race? RequiredRace = null,
    int Tier = 1,
    int RequiredLevel = 1
);

/// <summary>
/// スキルツリーシステム - パッシブノード/キーストーン/クラス横断パスを管理
/// </summary>
public class SkillTreeSystem
{
    private readonly Dictionary<string, SkillNodeDefinition> _allNodes = new();
    private readonly HashSet<string> _unlockedNodes = new();
    private int _availablePoints;

    /// <summary>利用可能なスキルポイント</summary>
    public int AvailablePoints => _availablePoints;

    /// <summary>解放済みノード数</summary>
    public int UnlockedCount => _unlockedNodes.Count;

    /// <summary>全ノード定義</summary>
    public IReadOnlyDictionary<string, SkillNodeDefinition> AllNodes => _allNodes;

    /// <summary>解放済みノードID一覧</summary>
    public IReadOnlySet<string> UnlockedNodes => _unlockedNodes;

    public SkillTreeSystem()
    {
        RegisterDefaultNodes();
    }

    /// <summary>スキルポイントを追加（レベルアップ時）</summary>
    public void AddPoints(int points)
    {
        _availablePoints += points;
    }

    /// <summary>ノードを解放可能か判定</summary>
    public bool CanUnlock(string nodeId, int playerLevel = int.MaxValue)
    {
        if (!_allNodes.TryGetValue(nodeId, out var node))
            return false;

        if (_unlockedNodes.Contains(nodeId))
            return false;

        if (_availablePoints < node.PointCost)
            return false;

        // レベル制限チェック
        if (playerLevel < node.RequiredLevel)
            return false;

        // 前提条件チェック
        foreach (var prereq in node.Prerequisites)
        {
            if (!_unlockedNodes.Contains(prereq))
                return false;
        }

        return true;
    }

    /// <summary>ノードを解放する</summary>
    public bool UnlockNode(string nodeId, int playerLevel = int.MaxValue)
    {
        if (!CanUnlock(nodeId, playerLevel))
            return false;

        var node = _allNodes[nodeId];
        _availablePoints -= node.PointCost;
        _unlockedNodes.Add(nodeId);
        return true;
    }

    /// <summary>リスペック（全ノード解除、ポイント返却）</summary>
    public int Respec()
    {
        int refunded = 0;
        foreach (var nodeId in _unlockedNodes)
        {
            if (_allNodes.TryGetValue(nodeId, out var node))
                refunded += node.PointCost;
        }
        _unlockedNodes.Clear();
        _availablePoints += refunded;
        return refunded;
    }

    /// <summary>解放済みノードのステータスボーナス合計を取得</summary>
    public Dictionary<string, int> GetTotalStatBonuses()
    {
        var totals = new Dictionary<string, int>();
        foreach (var nodeId in _unlockedNodes)
        {
            if (_allNodes.TryGetValue(nodeId, out var node))
            {
                foreach (var (stat, value) in node.StatBonuses)
                {
                    totals[stat] = totals.GetValueOrDefault(stat) + value;
                }
            }
        }
        return totals;
    }

    /// <summary>クラスと種族でアクセス可能なノード一覧を取得</summary>
    public IReadOnlyList<SkillNodeDefinition> GetNodesForClass(CharacterClass characterClass, Race? race = null)
    {
        return _allNodes.Values
            .Where(n => (n.RequiredClass == null || n.RequiredClass == characterClass)
                     && (n.RequiredRace == null || n.RequiredRace == race))
            .ToList();
    }

    /// <summary>キーストーンノード一覧を取得</summary>
    public IReadOnlyList<SkillNodeDefinition> GetKeystones()
    {
        return _allNodes.Values.Where(n => n.NodeType == SkillNodeType.Keystone).ToList();
    }

    /// <summary>解放済みキーストーンのデメリット一覧</summary>
    public IReadOnlyList<string> GetActiveKeystoneDownsides()
    {
        return _unlockedNodes
            .Where(id => _allNodes.TryGetValue(id, out var n) && n.KeystoneDownside != null)
            .Select(id => _allNodes[id].KeystoneDownside!)
            .ToList();
    }

    /// <summary>ノード定義を登録</summary>
    public void RegisterNode(SkillNodeDefinition node)
    {
        _allNodes[node.Id] = node;
    }

    private void RegisterDefaultNodes()
    {
        // ── 共有パッシブツリー（全クラス・全種族共通） ──
        RegisterNode(new SkillNodeDefinition(
            "shared_hp_1", "体力強化I", "最大HP+10", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["MaxHp"] = 10 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_hp_2", "体力強化II", "最大HP+20", SkillNodeType.StatMajor,
            null, 2, new[] { "shared_hp_1" }, new() { ["MaxHp"] = 20 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "shared_mp_1", "魔力強化I", "最大MP+10", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["MaxMp"] = 10 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_mp_2", "魔力強化II", "最大MP+20", SkillNodeType.StatMajor,
            null, 2, new[] { "shared_mp_1" }, new() { ["MaxMp"] = 20 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "shared_str_1", "筋力強化I", "STR+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["STR"] = 2 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_agi_1", "敏捷強化I", "AGI+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["AGI"] = 2 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_int_1", "知力強化I", "INT+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["INT"] = 2 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_vit_1", "耐久強化I", "VIT+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["VIT"] = 2 },
            Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "shared_luk_1", "幸運強化I", "LUK+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["LUK"] = 2 },
            Tier: 1, RequiredLevel: 1));

        // ── キーストーン（強力だがデメリット付き） ──
        RegisterNode(new SkillNodeDefinition(
            "keystone_glass_cannon", "ガラスの大砲", "全攻撃力+100%、被ダメージ+100%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_str_1" },
            new() { ["AttackMultiplier"] = 100 },
            "被ダメージ+100%", Tier: 4, RequiredLevel: 15));
        RegisterNode(new SkillNodeDefinition(
            "keystone_iron_wall", "鉄壁", "被ダメージ-50%、攻撃速度-30%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_hp_2" },
            new() { ["DefenseMultiplier"] = 50 },
            "攻撃速度-30%", Tier: 4, RequiredLevel: 15));
        RegisterNode(new SkillNodeDefinition(
            "keystone_vampiric", "吸血鬼", "攻撃時HP吸収10%、聖属性被ダメージ+50%",
            SkillNodeType.Keystone, null, 3, Array.Empty<string>(),
            new() { ["Lifesteal"] = 10 },
            "聖属性被ダメージ+50%", Tier: 4, RequiredLevel: 15));
        RegisterNode(new SkillNodeDefinition(
            "keystone_archmage", "大魔導師", "魔法ダメージ+80%、物理防御-40%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_int_1" },
            new() { ["MagicDamage"] = 80 },
            "物理防御-40%", Tier: 4, RequiredLevel: 15));

        // ── 職業固有ノード（各職業3ノード） ──

        // Fighter（戦士）
        RegisterNode(new SkillNodeDefinition(
            "fighter_heavy_blow", "重撃", "通常攻撃ダメージ+15%",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, Array.Empty<string>(),
            new() { ["PhysicalDamage"] = 15 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "fighter_armor_mastery", "重装の心得", "重装備ペナルティ軽減",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, new[] { "fighter_heavy_blow" },
            new() { ["ArmorPenaltyReduction"] = 30 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "fighter_berserker", "狂戦士の血", "HP50%以下で攻撃力+25%",
            SkillNodeType.Passive, CharacterClass.Fighter, 3, new[] { "fighter_heavy_blow" },
            new() { ["BerserkDamage"] = 25 },
            Tier: 3, RequiredLevel: 10));

        // Knight（騎士）
        RegisterNode(new SkillNodeDefinition(
            "knight_shield_wall", "盾の壁", "盾装備時、防御力+20%",
            SkillNodeType.Passive, CharacterClass.Knight, 2, Array.Empty<string>(),
            new() { ["ShieldDefense"] = 20 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "knight_holy_guard", "聖なる守護", "闇属性耐性+30%",
            SkillNodeType.Passive, CharacterClass.Knight, 2, new[] { "knight_shield_wall" },
            new() { ["DarkResist"] = 30 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "knight_fortress", "不落要塞", "最大HP+15%、被物理ダメージ-10%",
            SkillNodeType.Passive, CharacterClass.Knight, 3, new[] { "knight_shield_wall" },
            new() { ["MaxHp"] = 15, ["PhysicalResist"] = 10 },
            Tier: 3, RequiredLevel: 10));

        // Thief（盗賊）
        RegisterNode(new SkillNodeDefinition(
            "thief_dagger_mastery", "短剣の極意", "短剣装備時、クリティカル率+15%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, Array.Empty<string>(),
            new() { ["CritRate"] = 15 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "thief_stealth", "闇に紛れて", "先制攻撃率+20%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, new[] { "thief_dagger_mastery" },
            new() { ["PreemptiveRate"] = 20 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "thief_treasure_hunter", "財宝嗅覚", "アイテムドロップ率+15%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, new[] { "thief_dagger_mastery" },
            new() { ["DropRate"] = 15 },
            Tier: 3, RequiredLevel: 10));

        // Ranger（狩人）
        RegisterNode(new SkillNodeDefinition(
            "ranger_eagle_eye", "鷹の目", "射撃命中率+15%",
            SkillNodeType.Passive, CharacterClass.Ranger, 2, Array.Empty<string>(),
            new() { ["RangedAccuracy"] = 15 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "ranger_nature_sense", "自然感知", "罠感知率+25%",
            SkillNodeType.Passive, CharacterClass.Ranger, 2, new[] { "ranger_eagle_eye" },
            new() { ["TrapDetect"] = 25 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "ranger_swift_shot", "速射", "遠距離攻撃速度+20%",
            SkillNodeType.Passive, CharacterClass.Ranger, 3, new[] { "ranger_eagle_eye" },
            new() { ["RangedSpeed"] = 20 },
            Tier: 3, RequiredLevel: 10));

        // Mage（魔術師）
        RegisterNode(new SkillNodeDefinition(
            "mage_arcane_focus", "魔力集中", "魔法ダメージ+20%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, Array.Empty<string>(),
            new() { ["MagicDamage"] = 20 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "mage_mana_flow", "魔力の奔流", "MP消費-10%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, new[] { "mage_arcane_focus" },
            new() { ["MpCostReduction"] = 10 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "mage_elemental_mastery", "属性の極致", "全属性魔法ダメージ+15%",
            SkillNodeType.Passive, CharacterClass.Mage, 3, new[] { "mage_arcane_focus" },
            new() { ["ElementalDamage"] = 15 },
            Tier: 3, RequiredLevel: 10));

        // Cleric（僧侶）
        RegisterNode(new SkillNodeDefinition(
            "cleric_divine_grace", "神の恩寵", "回復魔法効果+20%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, Array.Empty<string>(),
            new() { ["HealingPower"] = 20 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "cleric_holy_shield", "聖盾", "聖属性耐性+25%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, new[] { "cleric_divine_grace" },
            new() { ["HolyResist"] = 25 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "cleric_purify", "浄化の力", "状態異常回復率+20%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, new[] { "cleric_divine_grace" },
            new() { ["StatusCureRate"] = 20 },
            Tier: 3, RequiredLevel: 10));

        // Monk（修道士）
        RegisterNode(new SkillNodeDefinition(
            "monk_iron_fist", "鉄拳", "素手攻撃力+25%",
            SkillNodeType.Passive, CharacterClass.Monk, 2, Array.Empty<string>(),
            new() { ["UnarmedDamage"] = 25 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "monk_inner_peace", "精神統一", "MP自然回復+30%",
            SkillNodeType.Passive, CharacterClass.Monk, 2, new[] { "monk_iron_fist" },
            new() { ["MpRegen"] = 30 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "monk_evasion_mastery", "見切り", "回避率+15%",
            SkillNodeType.Passive, CharacterClass.Monk, 3, new[] { "monk_iron_fist" },
            new() { ["Evasion"] = 15 },
            Tier: 3, RequiredLevel: 10));

        // Bard（吟遊詩人）
        RegisterNode(new SkillNodeDefinition(
            "bard_charisma", "魅惑の声", "CHA+5",
            SkillNodeType.Passive, CharacterClass.Bard, 2, Array.Empty<string>(),
            new() { ["CHA"] = 5 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "bard_inspiration", "鼓舞の歌", "パーティ全体の攻撃力+10%",
            SkillNodeType.Passive, CharacterClass.Bard, 2, new[] { "bard_charisma" },
            new() { ["PartyAttack"] = 10 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "bard_lullaby", "子守唄", "敵睡眠耐性低下+20%",
            SkillNodeType.Passive, CharacterClass.Bard, 2, new[] { "bard_charisma" },
            new() { ["SleepResistReduction"] = 20 },
            Tier: 3, RequiredLevel: 10));

        // Alchemist（錬金術師）
        RegisterNode(new SkillNodeDefinition(
            "alchemist_potion_mastery", "薬学の知識", "ポーション回復量+30%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 2, Array.Empty<string>(),
            new() { ["PotionPower"] = 30 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "alchemist_transmute", "錬成術", "素材変換効率+25%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 2, new[] { "alchemist_potion_mastery" },
            new() { ["TransmuteEfficiency"] = 25 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "alchemist_volatile_mix", "揮発性調合", "投擲アイテムダメージ+20%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 3, new[] { "alchemist_potion_mastery" },
            new() { ["ThrowDamage"] = 20 },
            Tier: 3, RequiredLevel: 10));

        // Necromancer（死霊術師）
        RegisterNode(new SkillNodeDefinition(
            "necro_dark_pact", "闇の契約", "闇魔法ダメージ+25%",
            SkillNodeType.Passive, CharacterClass.Necromancer, 2, Array.Empty<string>(),
            new() { ["DarkDamage"] = 25 },
            Tier: 2, RequiredLevel: 5));
        RegisterNode(new SkillNodeDefinition(
            "necro_soul_harvest", "魂の収穫", "敵撃破時MP回復+5",
            SkillNodeType.Passive, CharacterClass.Necromancer, 2, new[] { "necro_dark_pact" },
            new() { ["KillMpRestore"] = 5 },
            Tier: 3, RequiredLevel: 10));
        RegisterNode(new SkillNodeDefinition(
            "necro_undead_mastery", "不死支配", "召喚アンデッドの能力+20%",
            SkillNodeType.Passive, CharacterClass.Necromancer, 3, new[] { "necro_dark_pact" },
            new() { ["SummonPower"] = 20 },
            Tier: 3, RequiredLevel: 10));

        // ── 種族固有ノード（各種族2ノード） ──

        // Human（人間）
        RegisterNode(new SkillNodeDefinition(
            "race_human_adaptability", "適応力", "全ステータス+1",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 1, ["AGI"] = 1, ["INT"] = 1, ["VIT"] = 1 },
            RequiredRace: Race.Human, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_human_quick_learner", "学習能力", "経験値取得+10%",
            SkillNodeType.Passive, null, 2, new[] { "race_human_adaptability" },
            new() { ["ExpBonus"] = 10 },
            RequiredRace: Race.Human, Tier: 2, RequiredLevel: 5));

        // Elf（エルフ）
        RegisterNode(new SkillNodeDefinition(
            "race_elf_arcane_heritage", "魔法の血統", "最大MP+15、INT+2",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["MaxMp"] = 15, ["INT"] = 2 },
            RequiredRace: Race.Elf, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_elf_forest_stride", "森の歩み", "森林地形での移動速度+20%",
            SkillNodeType.Passive, null, 2, new[] { "race_elf_arcane_heritage" },
            new() { ["ForestSpeed"] = 20 },
            RequiredRace: Race.Elf, Tier: 2, RequiredLevel: 5));

        // Dwarf（ドワーフ）
        RegisterNode(new SkillNodeDefinition(
            "race_dwarf_stoneborn", "岩の加護", "物理防御+10、毒耐性+20%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["Defense"] = 10, ["PoisonResist"] = 20 },
            RequiredRace: Race.Dwarf, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_dwarf_forge_mastery", "鍛冶の心得", "武器強化成功率+15%",
            SkillNodeType.Passive, null, 2, new[] { "race_dwarf_stoneborn" },
            new() { ["ForgeRate"] = 15 },
            RequiredRace: Race.Dwarf, Tier: 2, RequiredLevel: 5));

        // Orc（オーク）
        RegisterNode(new SkillNodeDefinition(
            "race_orc_bloodlust", "血の渇望", "STR+3、攻撃力+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 3, ["PhysicalDamage"] = 10 },
            RequiredRace: Race.Orc, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_orc_war_cry", "雄叫び", "戦闘開始時、敵の防御力-10%",
            SkillNodeType.Passive, null, 2, new[] { "race_orc_bloodlust" },
            new() { ["EnemyDefReduction"] = 10 },
            RequiredRace: Race.Orc, Tier: 2, RequiredLevel: 5));

        // Beastfolk（獣人）
        RegisterNode(new SkillNodeDefinition(
            "race_beast_wild_sense", "野生の勘", "AGI+3、回避率+5%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["AGI"] = 3, ["Evasion"] = 5 },
            RequiredRace: Race.Beastfolk, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_beast_feral_instinct", "獣の本能", "先制攻撃率+15%、夜間視界+1",
            SkillNodeType.Passive, null, 2, new[] { "race_beast_wild_sense" },
            new() { ["PreemptiveRate"] = 15, ["NightVision"] = 1 },
            RequiredRace: Race.Beastfolk, Tier: 2, RequiredLevel: 5));

        // Halfling（ハーフリング）
        RegisterNode(new SkillNodeDefinition(
            "race_halfling_lucky", "幸運の星", "LUK+5",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["LUK"] = 5 },
            RequiredRace: Race.Halfling, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_halfling_nimble", "身軽な体", "罠回避率+20%、AGI+2",
            SkillNodeType.Passive, null, 2, new[] { "race_halfling_lucky" },
            new() { ["TrapEvasion"] = 20, ["AGI"] = 2 },
            RequiredRace: Race.Halfling, Tier: 2, RequiredLevel: 5));

        // Undead（アンデッド）
        RegisterNode(new SkillNodeDefinition(
            "race_undead_deathless", "不死の肉体", "毒・出血無効、聖属性弱点+20%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["PoisonImmunity"] = 1, ["BleedImmunity"] = 1 },
            "聖属性被ダメージ+20%", Race.Undead, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_undead_drain_touch", "生命吸収", "近接攻撃時HP吸収5%",
            SkillNodeType.Passive, null, 2, new[] { "race_undead_deathless" },
            new() { ["Lifesteal"] = 5 },
            RequiredRace: Race.Undead, Tier: 2, RequiredLevel: 5));

        // Demon（悪魔）
        RegisterNode(new SkillNodeDefinition(
            "race_demon_hellfire", "地獄の炎", "火属性ダメージ+20%、INT+2",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["FireDamage"] = 20, ["INT"] = 2 },
            RequiredRace: Race.Demon, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_demon_dark_aura", "魔王の威圧", "敵命中率-10%",
            SkillNodeType.Passive, null, 2, new[] { "race_demon_hellfire" },
            new() { ["EnemyAccReduction"] = 10 },
            RequiredRace: Race.Demon, Tier: 2, RequiredLevel: 5));

        // FallenAngel（堕天使）
        RegisterNode(new SkillNodeDefinition(
            "race_fallen_twilight", "黄昏の翼", "全ステータス+2、聖・闇耐性+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 2, ["AGI"] = 2, ["INT"] = 2, ["HolyResist"] = 10, ["DarkResist"] = 10 },
            RequiredRace: Race.FallenAngel, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_fallen_divine_wrath", "堕落の裁き", "クリティカルダメージ+25%",
            SkillNodeType.Passive, null, 2, new[] { "race_fallen_twilight" },
            new() { ["CritDamage"] = 25 },
            RequiredRace: Race.FallenAngel, Tier: 2, RequiredLevel: 5));

        // Slime（スライム）
        RegisterNode(new SkillNodeDefinition(
            "race_slime_amorphous", "不定形", "物理ダメージ-15%、装備不可ペナルティ軽減",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["PhysicalResist"] = 15 },
            RequiredRace: Race.Slime, Tier: 1, RequiredLevel: 1));
        RegisterNode(new SkillNodeDefinition(
            "race_slime_absorb", "吸収体質", "被ダメージの5%をMP変換",
            SkillNodeType.Passive, null, 2, new[] { "race_slime_amorphous" },
            new() { ["DamageToMp"] = 5 },
            RequiredRace: Race.Slime, Tier: 2, RequiredLevel: 5));
    }

    /// <summary>
    /// 全ノードを解除しポイントをゼロにする（死に戻り＋正気度0時に呼び出し）。
    /// 正気度0での死に戻りではスキル知識が消失するため、ツリーが完全リセットされる。
    /// ノード定義（マスターデータ）は保持される。
    /// </summary>
    public void Reset()
    {
        _unlockedNodes.Clear();
        _availablePoints = 0;
        _equippedSkillSlots.Clear();
    }

    // ── スキルスロット（習得済みスキルを装備するスロット） ──

    /// <summary>スキルスロット数上限</summary>
    public const int MaxSkillSlots = 6;

    private readonly List<string> _equippedSkillSlots = new();

    /// <summary>装備中のスキルスロット一覧</summary>
    public IReadOnlyList<string> EquippedSkillSlots => _equippedSkillSlots.AsReadOnly();

    /// <summary>スキルをスロットに装備する</summary>
    public bool EquipSkillToSlot(string nodeId)
    {
        if (!_unlockedNodes.Contains(nodeId))
            return false;
        if (_equippedSkillSlots.Contains(nodeId))
            return false;
        if (_equippedSkillSlots.Count >= MaxSkillSlots)
            return false;
        _equippedSkillSlots.Add(nodeId);
        return true;
    }

    /// <summary>スロットからスキルを外す</summary>
    public bool UnequipSkillFromSlot(string nodeId)
    {
        return _equippedSkillSlots.Remove(nodeId);
    }

    /// <summary>スロットのインデックスで直接外す</summary>
    public bool UnequipSkillSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedSkillSlots.Count)
            return false;
        _equippedSkillSlots.RemoveAt(slotIndex);
        return true;
    }
}
