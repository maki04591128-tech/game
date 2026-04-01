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
    int RequiredLevel = 1,
    SkillTreeTab Tab = SkillTreeTab.Class,
    double TreeX = 0,
    double TreeY = 0,
    Background? RequiredBackground = null
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
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 0, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_hp_2", "体力強化II", "最大HP+20", SkillNodeType.StatMajor,
            null, 2, new[] { "shared_hp_1" }, new() { ["MaxHp"] = 20 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 0, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "shared_mp_1", "魔力強化I", "最大MP+10", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["MaxMp"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 80, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_mp_2", "魔力強化II", "最大MP+20", SkillNodeType.StatMajor,
            null, 2, new[] { "shared_mp_1" }, new() { ["MaxMp"] = 20 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 80, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "shared_str_1", "筋力強化I", "STR+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["STR"] = 2 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 160, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_agi_1", "敏捷強化I", "AGI+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["AGI"] = 2 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 240, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_int_1", "知力強化I", "INT+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["INT"] = 2 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 320, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_vit_1", "耐久強化I", "VIT+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["VIT"] = 2 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 400, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "shared_luk_1", "幸運強化I", "LUK+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["LUK"] = 2 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Class, TreeX: 480, TreeY: 0));

        // ── キーストーン（強力だがデメリット付き） ──
        RegisterNode(new SkillNodeDefinition(
            "keystone_glass_cannon", "ガラスの大砲", "全攻撃力+100%、被ダメージ+100%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_str_1" },
            new() { ["AttackMultiplier"] = 100 },
            "被ダメージ+100%", Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 0, TreeY: 360));
        RegisterNode(new SkillNodeDefinition(
            "keystone_iron_wall", "鉄壁", "被ダメージ-50%、攻撃速度-30%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_hp_2" },
            new() { ["DefenseMultiplier"] = 50 },
            "攻撃速度-30%", Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 140, TreeY: 360));
        RegisterNode(new SkillNodeDefinition(
            "keystone_vampiric", "吸血鬼", "攻撃時HP吸収10%、聖属性被ダメージ+50%",
            SkillNodeType.Keystone, null, 3, Array.Empty<string>(),
            new() { ["Lifesteal"] = 10 },
            "聖属性被ダメージ+50%", Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 280, TreeY: 360));
        RegisterNode(new SkillNodeDefinition(
            "keystone_archmage", "大魔導師", "魔法ダメージ+80%、物理防御-40%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_int_1" },
            new() { ["MagicDamage"] = 80 },
            "物理防御-40%", Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 420, TreeY: 360));

        // ── 職業固有ノード（各職業3ノード） ──

        // Fighter（戦士） x=0
        RegisterNode(new SkillNodeDefinition(
            "fighter_heavy_blow", "重撃", "通常攻撃ダメージ+15%",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, Array.Empty<string>(),
            new() { ["PhysicalDamage"] = 15 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 0, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "fighter_armor_mastery", "重装の心得", "重装備ペナルティ軽減",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, new[] { "fighter_heavy_blow" },
            new() { ["ArmorPenaltyReduction"] = 30 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 0, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "fighter_berserker", "狂戦士の血", "HP50%以下で攻撃力+25%",
            SkillNodeType.Passive, CharacterClass.Fighter, 3, new[] { "fighter_heavy_blow" },
            new() { ["BerserkDamage"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 60, TreeY: 240));

        // Knight（騎士） x=160
        RegisterNode(new SkillNodeDefinition(
            "knight_shield_wall", "盾の壁", "盾装備時、防御力+20%",
            SkillNodeType.Passive, CharacterClass.Knight, 2, Array.Empty<string>(),
            new() { ["ShieldDefense"] = 20 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 160, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "knight_holy_guard", "聖なる守護", "闇属性耐性+30%",
            SkillNodeType.Passive, CharacterClass.Knight, 2, new[] { "knight_shield_wall" },
            new() { ["DarkResist"] = 30 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 160, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "knight_fortress", "不落要塞", "最大HP+15%、被物理ダメージ-10%",
            SkillNodeType.Passive, CharacterClass.Knight, 3, new[] { "knight_shield_wall" },
            new() { ["MaxHp"] = 15, ["PhysicalResist"] = 10 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 220, TreeY: 240));

        // Thief（盗賊） x=320
        RegisterNode(new SkillNodeDefinition(
            "thief_dagger_mastery", "短剣の極意", "短剣装備時、クリティカル率+15%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, Array.Empty<string>(),
            new() { ["CritRate"] = 15 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 320, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "thief_stealth", "闇に紛れて", "先制攻撃率+20%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, new[] { "thief_dagger_mastery" },
            new() { ["PreemptiveRate"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 320, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "thief_treasure_hunter", "財宝嗅覚", "アイテムドロップ率+15%",
            SkillNodeType.Passive, CharacterClass.Thief, 2, new[] { "thief_dagger_mastery" },
            new() { ["DropRate"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 380, TreeY: 240));

        // Ranger（狩人） x=480
        RegisterNode(new SkillNodeDefinition(
            "ranger_eagle_eye", "鷹の目", "射撃命中率+15%",
            SkillNodeType.Passive, CharacterClass.Ranger, 2, Array.Empty<string>(),
            new() { ["RangedAccuracy"] = 15 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 480, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "ranger_nature_sense", "自然感知", "罠感知率+25%",
            SkillNodeType.Passive, CharacterClass.Ranger, 2, new[] { "ranger_eagle_eye" },
            new() { ["TrapDetect"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 480, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "ranger_swift_shot", "速射", "遠距離攻撃速度+20%",
            SkillNodeType.Passive, CharacterClass.Ranger, 3, new[] { "ranger_eagle_eye" },
            new() { ["RangedSpeed"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 540, TreeY: 240));

        // Mage（魔術師） x=640
        RegisterNode(new SkillNodeDefinition(
            "mage_arcane_focus", "魔力集中", "魔法ダメージ+20%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, Array.Empty<string>(),
            new() { ["MagicDamage"] = 20 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 640, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "mage_mana_flow", "魔力の奔流", "MP消費-10%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, new[] { "mage_arcane_focus" },
            new() { ["MpCostReduction"] = 10 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 640, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "mage_elemental_mastery", "属性の極致", "全属性魔法ダメージ+15%",
            SkillNodeType.Passive, CharacterClass.Mage, 3, new[] { "mage_arcane_focus" },
            new() { ["ElementalDamage"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 700, TreeY: 240));

        // Cleric（僧侶） x=800
        RegisterNode(new SkillNodeDefinition(
            "cleric_divine_grace", "神の恩寵", "回復魔法効果+20%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, Array.Empty<string>(),
            new() { ["HealingPower"] = 20 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 800, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "cleric_holy_shield", "聖盾", "聖属性耐性+25%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, new[] { "cleric_divine_grace" },
            new() { ["HolyResist"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 800, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "cleric_purify", "浄化の力", "状態異常回復率+20%",
            SkillNodeType.Passive, CharacterClass.Cleric, 2, new[] { "cleric_divine_grace" },
            new() { ["StatusCureRate"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 860, TreeY: 240));

        // Monk（修道士） x=960
        RegisterNode(new SkillNodeDefinition(
            "monk_iron_fist", "鉄拳", "素手攻撃力+25%",
            SkillNodeType.Passive, CharacterClass.Monk, 2, Array.Empty<string>(),
            new() { ["UnarmedDamage"] = 25 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 960, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "monk_inner_peace", "精神統一", "MP自然回復+30%",
            SkillNodeType.Passive, CharacterClass.Monk, 2, new[] { "monk_iron_fist" },
            new() { ["MpRegen"] = 30 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 960, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "monk_evasion_mastery", "見切り", "回避率+15%",
            SkillNodeType.Passive, CharacterClass.Monk, 3, new[] { "monk_iron_fist" },
            new() { ["Evasion"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1020, TreeY: 240));

        // Bard（吟遊詩人） x=1120
        RegisterNode(new SkillNodeDefinition(
            "bard_charisma", "魅惑の声", "CHA+5",
            SkillNodeType.Passive, CharacterClass.Bard, 2, Array.Empty<string>(),
            new() { ["CHA"] = 5 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 1120, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "bard_inspiration", "鼓舞の歌", "パーティ全体の攻撃力+10%",
            SkillNodeType.Passive, CharacterClass.Bard, 2, new[] { "bard_charisma" },
            new() { ["PartyAttack"] = 10 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1120, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "bard_lullaby", "子守唄", "敵睡眠耐性低下+20%",
            SkillNodeType.Passive, CharacterClass.Bard, 2, new[] { "bard_charisma" },
            new() { ["SleepResistReduction"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1180, TreeY: 240));

        // Alchemist（錬金術師） x=1280
        RegisterNode(new SkillNodeDefinition(
            "alchemist_potion_mastery", "薬学の知識", "ポーション回復量+30%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 2, Array.Empty<string>(),
            new() { ["PotionPower"] = 30 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 1280, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "alchemist_transmute", "錬成術", "素材変換効率+25%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 2, new[] { "alchemist_potion_mastery" },
            new() { ["TransmuteEfficiency"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1280, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "alchemist_volatile_mix", "揮発性調合", "投擲アイテムダメージ+20%",
            SkillNodeType.Passive, CharacterClass.Alchemist, 3, new[] { "alchemist_potion_mastery" },
            new() { ["ThrowDamage"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1340, TreeY: 240));

        // Necromancer（死霊術師） x=1440
        RegisterNode(new SkillNodeDefinition(
            "necro_dark_pact", "闇の契約", "闇魔法ダメージ+25%",
            SkillNodeType.Passive, CharacterClass.Necromancer, 2, Array.Empty<string>(),
            new() { ["DarkDamage"] = 25 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Class, TreeX: 1440, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "necro_soul_harvest", "魂の収穫", "敵撃破時MP回復+5",
            SkillNodeType.Passive, CharacterClass.Necromancer, 2, new[] { "necro_dark_pact" },
            new() { ["KillMpRestore"] = 5 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1440, TreeY: 240));
        RegisterNode(new SkillNodeDefinition(
            "necro_undead_mastery", "不死支配", "召喚アンデッドの能力+20%",
            SkillNodeType.Passive, CharacterClass.Necromancer, 3, new[] { "necro_dark_pact" },
            new() { ["SummonPower"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Class, TreeX: 1500, TreeY: 240));

        // ── 種族固有ノード（各種族2ノード） ──

        // Human（人間） x=0
        RegisterNode(new SkillNodeDefinition(
            "race_human_adaptability", "適応力", "全ステータス+1",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 1, ["AGI"] = 1, ["INT"] = 1, ["VIT"] = 1 },
            RequiredRace: Race.Human, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 0, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_human_quick_learner", "学習能力", "経験値取得+10%",
            SkillNodeType.Passive, null, 2, new[] { "race_human_adaptability" },
            new() { ["ExpBonus"] = 10 },
            RequiredRace: Race.Human, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 0, TreeY: 120));

        // Elf（エルフ） x=160
        RegisterNode(new SkillNodeDefinition(
            "race_elf_arcane_heritage", "魔法の血統", "最大MP+15、INT+2",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["MaxMp"] = 15, ["INT"] = 2 },
            RequiredRace: Race.Elf, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 160, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_elf_forest_stride", "森の歩み", "森林地形での移動速度+20%",
            SkillNodeType.Passive, null, 2, new[] { "race_elf_arcane_heritage" },
            new() { ["ForestSpeed"] = 20 },
            RequiredRace: Race.Elf, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 160, TreeY: 120));

        // Dwarf（ドワーフ） x=320
        RegisterNode(new SkillNodeDefinition(
            "race_dwarf_stoneborn", "岩の加護", "物理防御+10、毒耐性+20%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["Defense"] = 10, ["PoisonResist"] = 20 },
            RequiredRace: Race.Dwarf, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 320, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_dwarf_forge_mastery", "鍛冶の心得", "武器強化成功率+15%",
            SkillNodeType.Passive, null, 2, new[] { "race_dwarf_stoneborn" },
            new() { ["ForgeRate"] = 15 },
            RequiredRace: Race.Dwarf, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 320, TreeY: 120));

        // Orc（オーク） x=480
        RegisterNode(new SkillNodeDefinition(
            "race_orc_bloodlust", "血の渇望", "STR+3、攻撃力+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 3, ["PhysicalDamage"] = 10 },
            RequiredRace: Race.Orc, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 480, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_orc_war_cry", "雄叫び", "戦闘開始時、敵の防御力-10%",
            SkillNodeType.Passive, null, 2, new[] { "race_orc_bloodlust" },
            new() { ["EnemyDefReduction"] = 10 },
            RequiredRace: Race.Orc, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 480, TreeY: 120));

        // Beastfolk（獣人） x=640
        RegisterNode(new SkillNodeDefinition(
            "race_beast_wild_sense", "野生の勘", "AGI+3、回避率+5%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["AGI"] = 3, ["Evasion"] = 5 },
            RequiredRace: Race.Beastfolk, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 640, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_beast_feral_instinct", "獣の本能", "先制攻撃率+15%、夜間視界+1",
            SkillNodeType.Passive, null, 2, new[] { "race_beast_wild_sense" },
            new() { ["PreemptiveRate"] = 15, ["NightVision"] = 1 },
            RequiredRace: Race.Beastfolk, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 640, TreeY: 120));

        // Halfling（ハーフリング） x=800
        RegisterNode(new SkillNodeDefinition(
            "race_halfling_lucky", "幸運の星", "LUK+5",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["LUK"] = 5 },
            RequiredRace: Race.Halfling, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 800, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_halfling_nimble", "身軽な体", "罠回避率+20%、AGI+2",
            SkillNodeType.Passive, null, 2, new[] { "race_halfling_lucky" },
            new() { ["TrapEvasion"] = 20, ["AGI"] = 2 },
            RequiredRace: Race.Halfling, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 800, TreeY: 120));

        // Undead（アンデッド） x=960
        RegisterNode(new SkillNodeDefinition(
            "race_undead_deathless", "不死の肉体", "毒・出血無効、聖属性弱点+20%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["PoisonImmunity"] = 1, ["BleedImmunity"] = 1 },
            "聖属性被ダメージ+20%", Race.Undead, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 960, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_undead_drain_touch", "生命吸収", "近接攻撃時HP吸収5%",
            SkillNodeType.Passive, null, 2, new[] { "race_undead_deathless" },
            new() { ["Lifesteal"] = 5 },
            RequiredRace: Race.Undead, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 960, TreeY: 120));

        // Demon（悪魔） x=1120
        RegisterNode(new SkillNodeDefinition(
            "race_demon_hellfire", "地獄の炎", "火属性ダメージ+20%、INT+2",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["FireDamage"] = 20, ["INT"] = 2 },
            RequiredRace: Race.Demon, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 1120, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_demon_dark_aura", "魔王の威圧", "敵命中率-10%",
            SkillNodeType.Passive, null, 2, new[] { "race_demon_hellfire" },
            new() { ["EnemyAccReduction"] = 10 },
            RequiredRace: Race.Demon, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 1120, TreeY: 120));

        // FallenAngel（堕天使） x=1280
        RegisterNode(new SkillNodeDefinition(
            "race_fallen_twilight", "黄昏の翼", "全ステータス+2、聖・闇耐性+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["STR"] = 2, ["AGI"] = 2, ["INT"] = 2, ["HolyResist"] = 10, ["DarkResist"] = 10 },
            RequiredRace: Race.FallenAngel, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 1280, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_fallen_divine_wrath", "堕落の裁き", "クリティカルダメージ+25%",
            SkillNodeType.Passive, null, 2, new[] { "race_fallen_twilight" },
            new() { ["CritDamage"] = 25 },
            RequiredRace: Race.FallenAngel, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 1280, TreeY: 120));

        // Slime（スライム） x=1440
        RegisterNode(new SkillNodeDefinition(
            "race_slime_amorphous", "不定形", "物理ダメージ-15%、装備不可ペナルティ軽減",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["PhysicalResist"] = 15 },
            RequiredRace: Race.Slime, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Race, TreeX: 1440, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "race_slime_absorb", "吸収体質", "被ダメージの5%をMP変換",
            SkillNodeType.Passive, null, 2, new[] { "race_slime_amorphous" },
            new() { ["DamageToMp"] = 5 },
            RequiredRace: Race.Slime, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Race, TreeX: 1440, TreeY: 120));

        // ── 素性固有ノード（各素性2ノード） ──

        // Adventurer（冒険者） x=0
        RegisterNode(new SkillNodeDefinition(
            "bg_adventurer_1", "冒険の心得", "探索報酬+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["ExploreBonus"] = 10 },
            RequiredBackground: Background.Adventurer, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 0, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_adventurer_2", "歴戦の直感", "罠回避+15%",
            SkillNodeType.Passive, null, 2, new[] { "bg_adventurer_1" },
            new() { ["TrapEvasion"] = 15 },
            RequiredBackground: Background.Adventurer, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 0, TreeY: 120));

        // Soldier（兵士） x=160
        RegisterNode(new SkillNodeDefinition(
            "bg_soldier_1", "戦場の経験", "物理攻撃力+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["PhysicalDamage"] = 10 },
            RequiredBackground: Background.Soldier, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 160, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_soldier_2", "鉄の規律", "防御力+15",
            SkillNodeType.Passive, null, 2, new[] { "bg_soldier_1" },
            new() { ["Defense"] = 15 },
            RequiredBackground: Background.Soldier, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 160, TreeY: 120));

        // Scholar（学者） x=320
        RegisterNode(new SkillNodeDefinition(
            "bg_scholar_1", "博識", "INT+3",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["INT"] = 3 },
            RequiredBackground: Background.Scholar, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 320, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_scholar_2", "知恵の泉", "最大MP+25",
            SkillNodeType.Passive, null, 2, new[] { "bg_scholar_1" },
            new() { ["MaxMp"] = 25 },
            RequiredBackground: Background.Scholar, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 320, TreeY: 120));

        // Merchant（商人） x=480
        RegisterNode(new SkillNodeDefinition(
            "bg_merchant_1", "商売上手", "売却価格+15%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["SellBonus"] = 15 },
            RequiredBackground: Background.Merchant, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 480, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_merchant_2", "値切り", "購入価格-10%",
            SkillNodeType.Passive, null, 2, new[] { "bg_merchant_1" },
            new() { ["BuyDiscount"] = 10 },
            RequiredBackground: Background.Merchant, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 480, TreeY: 120));

        // Peasant（農民） x=640
        RegisterNode(new SkillNodeDefinition(
            "bg_peasant_1", "野の知恵", "VIT+3",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["VIT"] = 3 },
            RequiredBackground: Background.Peasant, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 640, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_peasant_2", "大地の恵み", "HP自然回復+20%",
            SkillNodeType.Passive, null, 2, new[] { "bg_peasant_1" },
            new() { ["HpRegen"] = 20 },
            RequiredBackground: Background.Peasant, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 640, TreeY: 120));

        // Noble（貴族） x=800
        RegisterNode(new SkillNodeDefinition(
            "bg_noble_1", "高貴な血筋", "CHA+3",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["CHA"] = 3 },
            RequiredBackground: Background.Noble, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 800, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_noble_2", "権威の力", "NPC好感度+15%",
            SkillNodeType.Passive, null, 2, new[] { "bg_noble_1" },
            new() { ["NpcFavor"] = 15 },
            RequiredBackground: Background.Noble, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 800, TreeY: 120));

        // Wanderer（流浪者） x=960
        RegisterNode(new SkillNodeDefinition(
            "bg_wanderer_1", "放浪の足", "移動速度+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["MoveSpeed"] = 10 },
            RequiredBackground: Background.Wanderer, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 960, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_wanderer_2", "旅の知恵", "マップ発見率+15%",
            SkillNodeType.Passive, null, 2, new[] { "bg_wanderer_1" },
            new() { ["MapReveal"] = 15 },
            RequiredBackground: Background.Wanderer, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 960, TreeY: 120));

        // Criminal（犯罪者） x=1120
        RegisterNode(new SkillNodeDefinition(
            "bg_criminal_1", "裏社会の知識", "罠設置ダメージ+15%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["TrapDamage"] = 15 },
            RequiredBackground: Background.Criminal, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 1120, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_criminal_2", "闇の手", "盗み成功率+20%",
            SkillNodeType.Passive, null, 2, new[] { "bg_criminal_1" },
            new() { ["StealRate"] = 20 },
            RequiredBackground: Background.Criminal, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 1120, TreeY: 120));

        // Priest（聖職者） x=1280
        RegisterNode(new SkillNodeDefinition(
            "bg_priest_1", "祈りの力", "回復魔法効果+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["HealingPower"] = 10 },
            RequiredBackground: Background.Priest, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 1280, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_priest_2", "聖なる加護", "聖属性耐性+20%",
            SkillNodeType.Passive, null, 2, new[] { "bg_priest_1" },
            new() { ["HolyResist"] = 20 },
            RequiredBackground: Background.Priest, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 1280, TreeY: 120));

        // Penitent（贖罪者） x=1440
        RegisterNode(new SkillNodeDefinition(
            "bg_penitent_1", "贖罪の意志", "被ダメージ時反撃率+10%",
            SkillNodeType.Passive, null, 2, Array.Empty<string>(),
            new() { ["CounterRate"] = 10 },
            RequiredBackground: Background.Penitent, Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Background, TreeX: 1440, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "bg_penitent_2", "苦行の果て", "HP50%以下で防御力+20%",
            SkillNodeType.Passive, null, 2, new[] { "bg_penitent_1" },
            new() { ["LowHpDefense"] = 20 },
            RequiredBackground: Background.Penitent, Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Background, TreeX: 1440, TreeY: 120));

        // ── 武器習熟ノード（6武器種×3ノード） ──

        // Sword（剣） x=0
        RegisterNode(new SkillNodeDefinition(
            "weapon_sword_1", "剣術入門", "剣攻撃力+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["SwordDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 0, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_sword_2", "剣術応用", "剣クリティカル+10%",
            SkillNodeType.Passive, null, 2, new[] { "weapon_sword_1" },
            new() { ["SwordCrit"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 0, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_sword_3", "剣聖", "剣ダメージ+25%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_sword_2" },
            new() { ["SwordDamage"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 0, TreeY: 240));

        // Axe（斧） x=200
        RegisterNode(new SkillNodeDefinition(
            "weapon_axe_1", "斧術入門", "斧攻撃力+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["AxeDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 200, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_axe_2", "斧術応用", "斧攻撃速度+10%",
            SkillNodeType.Passive, null, 2, new[] { "weapon_axe_1" },
            new() { ["AxeSpeed"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 200, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_axe_3", "豪斧", "斧ダメージ+25%、出血付与+15%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_axe_2" },
            new() { ["AxeDamage"] = 25, ["BleedChance"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 200, TreeY: 240));

        // Dagger（短剣） x=400
        RegisterNode(new SkillNodeDefinition(
            "weapon_dagger_1", "短剣術入門", "短剣速度+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["DaggerSpeed"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 400, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_dagger_2", "暗器術", "短剣クリティカル+15%",
            SkillNodeType.Passive, null, 2, new[] { "weapon_dagger_1" },
            new() { ["DaggerCrit"] = 15 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 400, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_dagger_3", "暗殺", "背後攻撃ダメージ+30%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_dagger_2" },
            new() { ["BackstabDamage"] = 30 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 400, TreeY: 240));

        // Bow（弓） x=600
        RegisterNode(new SkillNodeDefinition(
            "weapon_bow_1", "弓術入門", "弓命中+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["BowAccuracy"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 600, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_bow_2", "弓術応用", "弓射程+1",
            SkillNodeType.Passive, null, 2, new[] { "weapon_bow_1" },
            new() { ["BowRange"] = 1 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 600, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_bow_3", "狙撃", "弓クリティカルダメージ+25%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_bow_2" },
            new() { ["BowCritDamage"] = 25 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 600, TreeY: 240));

        // Staff（杖） x=800
        RegisterNode(new SkillNodeDefinition(
            "weapon_staff_1", "杖術入門", "杖装備時魔法+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["StaffMagic"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 800, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_staff_2", "杖術応用", "杖装備時MP-5%",
            SkillNodeType.Passive, null, 2, new[] { "weapon_staff_1" },
            new() { ["StaffMpCost"] = 5 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 800, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_staff_3", "魔杖", "杖装備時全魔法+20%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_staff_2" },
            new() { ["StaffMagic"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 800, TreeY: 240));

        // Shield（盾） x=1000
        RegisterNode(new SkillNodeDefinition(
            "weapon_shield_1", "盾術入門", "盾防御+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["ShieldDefense"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Weapon, TreeX: 1000, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "weapon_shield_2", "盾術応用", "盾ブロック率+10%",
            SkillNodeType.Passive, null, 2, new[] { "weapon_shield_1" },
            new() { ["ShieldBlock"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Weapon, TreeX: 1000, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "weapon_shield_3", "不動の盾", "盾装備時被ダメ-15%",
            SkillNodeType.Passive, null, 3, new[] { "weapon_shield_2" },
            new() { ["ShieldDamageReduction"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Weapon, TreeX: 1000, TreeY: 240));

        // ── 魔法習熟ノード（5系統×3ノード） ──

        // Fire（火炎） x=0
        RegisterNode(new SkillNodeDefinition(
            "magic_fire_1", "火炎術入門", "火属性ダメージ+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["FireDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Magic, TreeX: 0, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "magic_fire_2", "火炎術応用", "火属性範囲+10%",
            SkillNodeType.Passive, null, 2, new[] { "magic_fire_1" },
            new() { ["FireArea"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Magic, TreeX: 0, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "magic_fire_3", "火炎の極致", "火属性ダメージ+25%、燃焼付与+15%",
            SkillNodeType.Passive, null, 3, new[] { "magic_fire_2" },
            new() { ["FireDamage"] = 25, ["BurnChance"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 0, TreeY: 240));

        // Ice（氷結） x=240
        RegisterNode(new SkillNodeDefinition(
            "magic_ice_1", "氷結術入門", "氷属性ダメージ+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["IceDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Magic, TreeX: 240, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "magic_ice_2", "氷結術応用", "氷属性の鈍化効果+10%",
            SkillNodeType.Passive, null, 2, new[] { "magic_ice_1" },
            new() { ["IceSlow"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Magic, TreeX: 240, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "magic_ice_3", "氷結の極致", "氷属性ダメージ+25%、凍結付与+15%",
            SkillNodeType.Passive, null, 3, new[] { "magic_ice_2" },
            new() { ["IceDamage"] = 25, ["FreezeChance"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 240, TreeY: 240));

        // Thunder（雷撃） x=480
        RegisterNode(new SkillNodeDefinition(
            "magic_thunder_1", "雷撃術入門", "雷属性ダメージ+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["ThunderDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Magic, TreeX: 480, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "magic_thunder_2", "雷撃術応用", "雷属性クリティカル+10%",
            SkillNodeType.Passive, null, 2, new[] { "magic_thunder_1" },
            new() { ["ThunderCrit"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Magic, TreeX: 480, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "magic_thunder_3", "雷撃の極致", "雷属性ダメージ+25%、麻痺付与+15%",
            SkillNodeType.Passive, null, 3, new[] { "magic_thunder_2" },
            new() { ["ThunderDamage"] = 25, ["ParalyzeChance"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 480, TreeY: 240));

        // Holy（聖術） x=720
        RegisterNode(new SkillNodeDefinition(
            "magic_holy_1", "聖術入門", "回復魔法効果+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["HealingPower"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Magic, TreeX: 720, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "magic_holy_2", "聖術応用", "聖属性ダメージ+10%",
            SkillNodeType.Passive, null, 2, new[] { "magic_holy_1" },
            new() { ["HolyDamage"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Magic, TreeX: 720, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "magic_holy_3", "聖術の極致", "回復+25%、アンデッド特攻+20%",
            SkillNodeType.Passive, null, 3, new[] { "magic_holy_2" },
            new() { ["HealingPower"] = 25, ["UndeadDamage"] = 20 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 720, TreeY: 240));

        // Dark（暗黒） x=960
        RegisterNode(new SkillNodeDefinition(
            "magic_dark_1", "暗黒術入門", "闇属性ダメージ+10%",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["DarkDamage"] = 10 },
            Tier: 1, RequiredLevel: 1, Tab: SkillTreeTab.Magic, TreeX: 960, TreeY: 0));
        RegisterNode(new SkillNodeDefinition(
            "magic_dark_2", "暗黒術応用", "闇属性デバフ+10%",
            SkillNodeType.Passive, null, 2, new[] { "magic_dark_1" },
            new() { ["DarkDebuff"] = 10 },
            Tier: 2, RequiredLevel: 5, Tab: SkillTreeTab.Magic, TreeX: 960, TreeY: 120));
        RegisterNode(new SkillNodeDefinition(
            "magic_dark_3", "暗黒の極致", "闇ダメージ+25%、呪い付与+15%",
            SkillNodeType.Passive, null, 3, new[] { "magic_dark_2" },
            new() { ["DarkDamage"] = 25, ["CurseChance"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 960, TreeY: 240));

        // ── アクティブスキル（武器タブ） ──

        // 剣系アクティブ: 十字斬り
        RegisterNode(new SkillNodeDefinition(
            "active_sword_cross_slash", "十字斬り", "前方の敵に攻撃力150%の二連撃を行う（MP8消費）",
            SkillNodeType.Active, null, 3, new[] { "weapon_sword_3" },
            new() { ["SwordDamage"] = 50 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 0, TreeY: 360));

        // 斧系アクティブ: 大地割り
        RegisterNode(new SkillNodeDefinition(
            "active_axe_ground_slam", "大地割り", "周囲3マスの敵全てに攻撃力120%ダメージ（MP10消費）",
            SkillNodeType.Active, null, 3, new[] { "weapon_axe_3" },
            new() { ["AxeDamage"] = 20 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 192, TreeY: 360));

        // 短剣系アクティブ: 影縫い
        RegisterNode(new SkillNodeDefinition(
            "active_dagger_shadow_stitch", "影縫い", "対象を2ターン行動不能にする暗器（MP6消費）",
            SkillNodeType.Active, null, 3, new[] { "weapon_dagger_3" },
            new() { ["StunChance"] = 100 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 384, TreeY: 360));

        // 弓系アクティブ: 貫通矢
        RegisterNode(new SkillNodeDefinition(
            "active_bow_piercing_shot", "貫通矢", "直線上の敵全てに攻撃力130%ダメージ（MP8消費）",
            SkillNodeType.Active, null, 3, new[] { "weapon_bow_3" },
            new() { ["BowDamage"] = 30 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 576, TreeY: 360));

        // 杖系アクティブ: マナバースト
        RegisterNode(new SkillNodeDefinition(
            "active_staff_mana_burst", "マナバースト", "MPを20消費し周囲に魔法ダメージ200%の爆発",
            SkillNodeType.Active, null, 3, new[] { "weapon_staff_3" },
            new() { ["MagicDamage"] = 100 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 768, TreeY: 360));

        // 盾系アクティブ: シールドバッシュ
        RegisterNode(new SkillNodeDefinition(
            "active_shield_bash", "シールドバッシュ", "盾で殴り攻撃力80%ダメージ＋1ターンスタン（MP5消費）",
            SkillNodeType.Active, null, 3, new[] { "weapon_shield_3" },
            new() { ["ShieldDamage"] = 80, ["StunChance"] = 100 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Weapon, TreeX: 960, TreeY: 360));

        // ── アクティブスキル（魔法タブ） ── 補助系統のみ（魔法は詠唱で発動するため）

        // 瞑想: MP回復アクティブスキル
        RegisterNode(new SkillNodeDefinition(
            "active_magic_meditation", "瞑想", "その場で精神を集中しMPを最大MPの15%回復する（3ターン詠唱）",
            SkillNodeType.Active, null, 2, new[] { "shared_mp_2" },
            new() { ["MpRegen"] = 15 },
            Tier: 3, RequiredLevel: 8, Tab: SkillTreeTab.Magic, TreeX: 0, TreeY: 360));

        // 深い瞑想: 上位MP回復
        RegisterNode(new SkillNodeDefinition(
            "active_magic_deep_meditation", "深い瞑想", "深い集中によりMPを最大MPの30%回復する（5ターン詠唱）",
            SkillNodeType.Active, null, 3, new[] { "active_magic_meditation" },
            new() { ["MpRegen"] = 30 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Magic, TreeX: 0, TreeY: 480));

        // 精神集中: 詠唱速度向上
        RegisterNode(new SkillNodeDefinition(
            "active_magic_focus", "精神集中", "3ターンの間、魔法の詠唱速度を50%短縮する（MP8消費）",
            SkillNodeType.Active, null, 2, new[] { "shared_int_1" },
            new() { ["CastSpeed"] = 50 },
            Tier: 3, RequiredLevel: 8, Tab: SkillTreeTab.Magic, TreeX: 240, TreeY: 360));

        // 魔力障壁: 防御補助
        RegisterNode(new SkillNodeDefinition(
            "active_magic_barrier", "魔力障壁", "MPを10消費し5ターンの間魔法防御+30%のバリアを展開",
            SkillNodeType.Active, null, 3, new[] { "active_magic_focus" },
            new() { ["MagicDefense"] = 30 },
            Tier: 4, RequiredLevel: 12, Tab: SkillTreeTab.Magic, TreeX: 240, TreeY: 480));

        // 魔力譲渡: 味方MP回復
        RegisterNode(new SkillNodeDefinition(
            "active_magic_transfer", "魔力譲渡", "自身のMPを20消費し味方1体のMPを15回復する",
            SkillNodeType.Active, null, 2, new[] { "shared_mp_1" },
            new() { ["MpTransfer"] = 15 },
            Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Magic, TreeX: 480, TreeY: 360));

        // 魔力感知: 探索補助
        RegisterNode(new SkillNodeDefinition(
            "active_magic_sense", "魔力感知", "MP5消費で周囲のマジックアイテムや隠し通路を感知する",
            SkillNodeType.Active, null, 2, new[] { "shared_int_1" },
            new() { ["DetectMagic"] = 1 },
            Tier: 3, RequiredLevel: 8, Tab: SkillTreeTab.Magic, TreeX: 720, TreeY: 360));

        // マナシールド: HP防御をMP肩代わり
        RegisterNode(new SkillNodeDefinition(
            "active_magic_mana_shield", "マナシールド", "10ターンの間、受けるダメージの30%をMPで肩代わりする（MP10消費）",
            SkillNodeType.Active, null, 3, new[] { "active_magic_focus" },
            new() { ["DamageToMp"] = 30 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Magic, TreeX: 480, TreeY: 480));

        // ── アクティブスキル（種族タブ） ──

        // Human: 万能適応
        RegisterNode(new SkillNodeDefinition(
            "active_race_human_versatile", "万能適応", "5ターンの間、全ステータス+3（MP8消費）",
            SkillNodeType.Active, null, 2, new[] { "race_human_quick_learner" },
            new() { ["AllStats"] = 3 },
            RequiredRace: Race.Human, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 0, TreeY: 240));

        // Elf: 精霊の加護
        RegisterNode(new SkillNodeDefinition(
            "active_race_elf_spirit_grace", "精霊の加護", "5ターンの間、魔法ダメージ+20%・MP自然回復2倍（MP10消費）",
            SkillNodeType.Active, null, 2, new[] { "race_elf_forest_stride" },
            new() { ["MagicDamage"] = 20, ["MpRegen"] = 100 },
            RequiredRace: Race.Elf, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 160, TreeY: 240));

        // Dwarf: 岩石防壁
        RegisterNode(new SkillNodeDefinition(
            "active_race_dwarf_stone_wall", "岩石防壁", "3ターンの間、物理防御+50%・移動不可（MP6消費）",
            SkillNodeType.Active, null, 2, new[] { "race_dwarf_forge_mastery" },
            new() { ["Defense"] = 50 },
            RequiredRace: Race.Dwarf, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 320, TreeY: 240));

        // Orc: 戦嵐
        RegisterNode(new SkillNodeDefinition(
            "active_race_orc_war_frenzy", "戦嵐", "3ターンの間、攻撃力+30%・被ダメージ+15%（SP20消費）",
            SkillNodeType.Active, null, 2, new[] { "race_orc_war_cry" },
            new() { ["PhysicalDamage"] = 30 },
            RequiredRace: Race.Orc, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 480, TreeY: 240));

        // Beastfolk: 野生解放
        RegisterNode(new SkillNodeDefinition(
            "active_race_beast_wild_release", "野生解放", "5ターンの間、AGI+5・回避率+15%（SP15消費）",
            SkillNodeType.Active, null, 2, new[] { "race_beast_feral_instinct" },
            new() { ["AGI"] = 5, ["Evasion"] = 15 },
            RequiredRace: Race.Beastfolk, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 640, TreeY: 240));

        // Halfling: 幸運の一撃
        RegisterNode(new SkillNodeDefinition(
            "active_race_halfling_lucky_strike", "幸運の一撃", "次の攻撃のクリティカル率を100%にする（MP5消費）",
            SkillNodeType.Active, null, 2, new[] { "race_halfling_nimble" },
            new() { ["CritRate"] = 100 },
            RequiredRace: Race.Halfling, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 800, TreeY: 240));

        // Undead: 腐食の抱擁
        RegisterNode(new SkillNodeDefinition(
            "active_race_undead_rot_embrace", "腐食の抱擁", "隣接する敵に毒+出血を付与（MP8消費）",
            SkillNodeType.Active, null, 2, new[] { "race_undead_drain_touch" },
            new() { ["PoisonChance"] = 100, ["BleedChance"] = 100 },
            RequiredRace: Race.Undead, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 960, TreeY: 240));

        // Demon: 魔炎放射
        RegisterNode(new SkillNodeDefinition(
            "active_race_demon_hellblaze", "魔炎放射", "前方3マスに火+闇属性の複合ダメージ（MP12消費）",
            SkillNodeType.Active, null, 2, new[] { "race_demon_dark_aura" },
            new() { ["FireDamage"] = 30, ["DarkDamage"] = 30 },
            RequiredRace: Race.Demon, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 1120, TreeY: 240));

        // FallenAngel: 黄昏の裁き
        RegisterNode(new SkillNodeDefinition(
            "active_race_fallen_judgment", "黄昏の裁き", "聖+闇属性の複合攻撃で敵単体に大ダメージ（MP15消費）",
            SkillNodeType.Active, null, 3, new[] { "race_fallen_divine_wrath" },
            new() { ["HolyDamage"] = 40, ["DarkDamage"] = 40 },
            RequiredRace: Race.FallenAngel, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 1280, TreeY: 240));

        // Slime: 分裂
        RegisterNode(new SkillNodeDefinition(
            "active_race_slime_split", "分裂", "HPを30%消費してスライム分身を1体召喚する",
            SkillNodeType.Active, null, 2, new[] { "race_slime_absorb" },
            new() { ["SummonPower"] = 30 },
            RequiredRace: Race.Slime, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Race, TreeX: 1440, TreeY: 240));

        // ── アクティブスキル（職業タブ） ──

        // Fighter: 渾身の一撃
        RegisterNode(new SkillNodeDefinition(
            "active_class_fighter_full_swing", "渾身の一撃", "SP30消費で攻撃力250%の単体攻撃",
            SkillNodeType.Active, CharacterClass.Fighter, 3, new[] { "fighter_berserker" },
            new() { ["PhysicalDamage"] = 150 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 30, TreeY: 360));

        // Knight: 聖なる盾撃
        RegisterNode(new SkillNodeDefinition(
            "active_class_knight_holy_bash", "聖なる盾撃", "盾で敵を打ち据え2ターンスタン+聖ダメージ（MP10消費）",
            SkillNodeType.Active, CharacterClass.Knight, 3, new[] { "knight_fortress" },
            new() { ["HolyDamage"] = 50, ["StunChance"] = 100 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 190, TreeY: 360));

        // Thief: 影分身
        RegisterNode(new SkillNodeDefinition(
            "active_class_thief_shadow_clone", "影分身", "3ターンの間、回避率+40%・次の攻撃が必ず背面攻撃に（SP20消費）",
            SkillNodeType.Active, CharacterClass.Thief, 3, new[] { "thief_treasure_hunter" },
            new() { ["Evasion"] = 40, ["BackstabChance"] = 100 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 350, TreeY: 360));

        // Ranger: 鷹の急降下
        RegisterNode(new SkillNodeDefinition(
            "active_class_ranger_hawk_dive", "鷹の急降下", "遠距離から飛び込み攻撃力200%+出血付与（SP25消費）",
            SkillNodeType.Active, CharacterClass.Ranger, 3, new[] { "ranger_swift_shot" },
            new() { ["PhysicalDamage"] = 100, ["BleedChance"] = 50 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 510, TreeY: 360));

        // Mage: 魔力暴走
        RegisterNode(new SkillNodeDefinition(
            "active_class_mage_arcane_surge", "魔力暴走", "5ターンの間、全魔法ダメージ+50%・MP消費+30%（MP15消費）",
            SkillNodeType.Active, CharacterClass.Mage, 3, new[] { "mage_elemental_mastery" },
            new() { ["MagicDamage"] = 50 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 670, TreeY: 360));

        // Cleric: 聖域展開
        RegisterNode(new SkillNodeDefinition(
            "active_class_cleric_sanctuary", "聖域展開", "周囲3マスに聖域を展開し味方全体をHP15%回復+状態異常解除（MP20消費）",
            SkillNodeType.Active, CharacterClass.Cleric, 3, new[] { "cleric_purify" },
            new() { ["HealPercent"] = 15, ["StatusCureRate"] = 100 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 830, TreeY: 360));

        // Monk: 練気発勁
        RegisterNode(new SkillNodeDefinition(
            "active_class_monk_qi_blast", "練気発勁", "内なる気を解放し前方の敵を吹き飛ばす（SP25消費）",
            SkillNodeType.Active, CharacterClass.Monk, 3, new[] { "monk_evasion_mastery" },
            new() { ["UnarmedDamage"] = 80, ["Knockback"] = 3 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 990, TreeY: 360));

        // Bard: 英雄の讃歌
        RegisterNode(new SkillNodeDefinition(
            "active_class_bard_hero_anthem", "英雄の讃歌", "5ターンの間、味方全体の全ステータス+10%（MP12消費）",
            SkillNodeType.Active, CharacterClass.Bard, 3, new[] { "bard_lullaby" },
            new() { ["PartyAllStats"] = 10 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 1150, TreeY: 360));

        // Alchemist: 秘薬調合
        RegisterNode(new SkillNodeDefinition(
            "active_class_alchemist_elixir", "秘薬調合", "素材を消費せず即座に高効果ポーションを生成・使用（MP10消費）",
            SkillNodeType.Active, CharacterClass.Alchemist, 3, new[] { "alchemist_volatile_mix" },
            new() { ["PotionPower"] = 50 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 1310, TreeY: 360));

        // Necromancer: 死霊大召喚
        RegisterNode(new SkillNodeDefinition(
            "active_class_necro_mass_summon", "死霊大召喚", "アンデッド3体を同時に召喚する（MP25消費）",
            SkillNodeType.Active, CharacterClass.Necromancer, 3, new[] { "necro_undead_mastery" },
            new() { ["SummonPower"] = 60 },
            Tier: 4, RequiredLevel: 15, Tab: SkillTreeTab.Class, TreeX: 1470, TreeY: 360));

        // ── アクティブスキル（素性タブ） ──

        // Adventurer: サバイバル術
        RegisterNode(new SkillNodeDefinition(
            "active_bg_adventurer_survive", "サバイバル術", "HP/MP/SPを各10%回復する（クールダウン50ターン）",
            SkillNodeType.Active, null, 2, new[] { "bg_adventurer_2" },
            new() { ["HealPercent"] = 10 },
            RequiredBackground: Background.Adventurer, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 0, TreeY: 240));

        // Soldier: 鼓舞号令
        RegisterNode(new SkillNodeDefinition(
            "active_bg_soldier_rally", "鼓舞号令", "5ターンの間、自身と味方の攻撃力+15%（SP15消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_soldier_2" },
            new() { ["PartyAttack"] = 15 },
            RequiredBackground: Background.Soldier, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 160, TreeY: 240));

        // Scholar: 弱点看破
        RegisterNode(new SkillNodeDefinition(
            "active_bg_scholar_analyze", "弱点看破", "敵1体の弱点を看破し3ターン被ダメージ+25%にする（MP8消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_scholar_2" },
            new() { ["EnemyWeakness"] = 25 },
            RequiredBackground: Background.Scholar, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 320, TreeY: 240));

        // Merchant: 緊急仕入れ
        RegisterNode(new SkillNodeDefinition(
            "active_bg_merchant_emergency_buy", "緊急仕入れ", "ゴールドを消費してランダムな回復アイテムを即座に入手",
            SkillNodeType.Active, null, 2, new[] { "bg_merchant_2" },
            new() { ["ItemFind"] = 1 },
            RequiredBackground: Background.Merchant, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 480, TreeY: 240));

        // Peasant: 大地の息吹
        RegisterNode(new SkillNodeDefinition(
            "active_bg_peasant_earth_breath", "大地の息吹", "10ターンの間、HP自然回復速度2倍（MP5消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_peasant_2" },
            new() { ["HpRegen"] = 100 },
            RequiredBackground: Background.Peasant, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 640, TreeY: 240));

        // Noble: 権威の宣言
        RegisterNode(new SkillNodeDefinition(
            "active_bg_noble_authority", "権威の宣言", "人間型の敵1体を3ターンの間行動不能にする（MP10消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_noble_2" },
            new() { ["StunChance"] = 100 },
            RequiredBackground: Background.Noble, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 800, TreeY: 240));

        // Wanderer: 霧隠れ
        RegisterNode(new SkillNodeDefinition(
            "active_bg_wanderer_mist_hide", "霧隠れ", "5ターンの間、敵から視認されなくなる（SP15消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_wanderer_2" },
            new() { ["Stealth"] = 100 },
            RequiredBackground: Background.Wanderer, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 960, TreeY: 240));

        // Criminal: 急所突き
        RegisterNode(new SkillNodeDefinition(
            "active_bg_criminal_vital_strike", "急所突き", "敵の急所を突き通常の200%ダメージ+出血（SP20消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_criminal_2" },
            new() { ["PhysicalDamage"] = 100, ["BleedChance"] = 80 },
            RequiredBackground: Background.Criminal, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 1120, TreeY: 240));

        // Priest: 神聖祈祷
        RegisterNode(new SkillNodeDefinition(
            "active_bg_priest_holy_prayer", "神聖祈祷", "味方全体のHP20%回復+状態異常1つ解除（MP15消費）",
            SkillNodeType.Active, null, 2, new[] { "bg_priest_2" },
            new() { ["HealPercent"] = 20, ["StatusCureRate"] = 1 },
            RequiredBackground: Background.Priest, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 1280, TreeY: 240));

        // Penitent: 贖罪の覚悟
        RegisterNode(new SkillNodeDefinition(
            "active_bg_penitent_resolve", "贖罪の覚悟", "HPを20%消費し10ターンの間攻撃力+40%・防御力+20%",
            SkillNodeType.Active, null, 2, new[] { "bg_penitent_2" },
            new() { ["PhysicalDamage"] = 40, ["Defense"] = 20 },
            RequiredBackground: Background.Penitent, Tier: 3, RequiredLevel: 10, Tab: SkillTreeTab.Background, TreeX: 1440, TreeY: 240));
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

    // ── 宗教スキル連携 ──

    /// <summary>宗教スキルをスキルツリーに登録し、自動的にアンロックする</summary>
    public void RegisterReligionSkills(ReligionId religionId)
    {
        var bonuses = ReligionSkillSystem.GetGrantedSkillBonuses(religionId, FaithRank.Saint);
        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null) return;

        foreach (var bonus in bonuses)
        {
            if (!_allNodes.ContainsKey(bonus.SkillId))
            {
                RegisterNode(new SkillNodeDefinition(
                    bonus.SkillId,
                    bonus.SkillName,
                    bonus.Description,
                    SkillNodeType.Active,
                    null,
                    0,
                    Array.Empty<string>(),
                    new Dictionary<string, int>(),
                    Tab: SkillTreeTab.Magic,
                    Tier: 1,
                    RequiredLevel: 1,
                    TreeX: 0,
                    TreeY: 0
                ));
            }

            // 入信時に付与されたスキルのみアンロック
            if (religion.GrantedSkills.Contains(bonus.SkillId))
            {
                _unlockedNodes.Add(bonus.SkillId);
            }
        }
    }

    /// <summary>宗教スキルをスキルツリーからアンロック解除する（脱退時）</summary>
    public void RemoveReligionSkills(ReligionId religionId)
    {
        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null) return;

        foreach (var skillId in religion.GrantedSkills)
        {
            _unlockedNodes.Remove(skillId);
            _equippedSkillSlots.Remove(skillId);
            _allNodes.Remove(skillId);
        }
    }
}
