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
    string? KeystoneDownside = null
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
    public bool CanUnlock(string nodeId)
    {
        if (!_allNodes.TryGetValue(nodeId, out var node))
            return false;

        if (_unlockedNodes.Contains(nodeId))
            return false;

        if (_availablePoints < node.PointCost)
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
    public bool UnlockNode(string nodeId)
    {
        if (!CanUnlock(nodeId))
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

    /// <summary>クラスでアクセス可能なノード一覧を取得</summary>
    public IReadOnlyList<SkillNodeDefinition> GetNodesForClass(CharacterClass characterClass)
    {
        return _allNodes.Values
            .Where(n => n.RequiredClass == null || n.RequiredClass == characterClass)
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
        // 共有パッシブツリー（全クラス共通）
        RegisterNode(new SkillNodeDefinition(
            "shared_hp_1", "体力強化I", "最大HP+10", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["MaxHp"] = 10 }));
        RegisterNode(new SkillNodeDefinition(
            "shared_hp_2", "体力強化II", "最大HP+20", SkillNodeType.StatMajor,
            null, 2, new[] { "shared_hp_1" }, new() { ["MaxHp"] = 20 }));
        RegisterNode(new SkillNodeDefinition(
            "shared_mp_1", "魔力強化I", "最大MP+10", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["MaxMp"] = 10 }));
        RegisterNode(new SkillNodeDefinition(
            "shared_str_1", "筋力強化I", "STR+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["STR"] = 2 }));
        RegisterNode(new SkillNodeDefinition(
            "shared_agi_1", "敏捷強化I", "AGI+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["AGI"] = 2 }));
        RegisterNode(new SkillNodeDefinition(
            "shared_int_1", "知力強化I", "INT+2", SkillNodeType.StatMinor,
            null, 1, Array.Empty<string>(), new() { ["INT"] = 2 }));

        // キーストーン（強力だがデメリット付き）
        RegisterNode(new SkillNodeDefinition(
            "keystone_glass_cannon", "ガラスの大砲", "全攻撃力+100%、被ダメージ+100%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_str_1" },
            new() { ["AttackMultiplier"] = 100 },
            "被ダメージ+100%"));
        RegisterNode(new SkillNodeDefinition(
            "keystone_iron_wall", "鉄壁", "被ダメージ-50%、攻撃速度-30%",
            SkillNodeType.Keystone, null, 3, new[] { "shared_hp_2" },
            new() { ["DefenseMultiplier"] = 50 },
            "攻撃速度-30%"));
        RegisterNode(new SkillNodeDefinition(
            "keystone_vampiric", "吸血鬼", "攻撃時HP吸収10%、聖属性被ダメージ+50%",
            SkillNodeType.Keystone, null, 3, Array.Empty<string>(),
            new() { ["Lifesteal"] = 10 },
            "聖属性被ダメージ+50%"));

        // クラス固有ノード例（Fighter）
        RegisterNode(new SkillNodeDefinition(
            "fighter_heavy_blow", "重撃", "通常攻撃ダメージ+15%",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, Array.Empty<string>(),
            new() { ["PhysicalDamage"] = 15 }));
        RegisterNode(new SkillNodeDefinition(
            "fighter_armor_mastery", "重装の心得", "重装備ペナルティ軽減",
            SkillNodeType.Passive, CharacterClass.Fighter, 2, new[] { "fighter_heavy_blow" },
            new() { ["ArmorPenaltyReduction"] = 30 }));

        // クラス固有ノード例（Mage）
        RegisterNode(new SkillNodeDefinition(
            "mage_arcane_focus", "魔力集中", "魔法ダメージ+20%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, Array.Empty<string>(),
            new() { ["MagicDamage"] = 20 }));
        RegisterNode(new SkillNodeDefinition(
            "mage_mana_flow", "魔力の奔流", "MP消費-10%",
            SkillNodeType.Passive, CharacterClass.Mage, 2, new[] { "mage_arcane_focus" },
            new() { ["MpCostReduction"] = 10 }));
    }
}
