using RougelikeGame.Core.AI;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// エンチャント定義
/// </summary>
public record EnchantmentDefinition(
    EnchantmentType Type,
    string Name,
    string Description,
    SoulGemQuality RequiredQuality,
    int ManaCost,
    float EffectValue,
    Element AssociatedElement = Element.None
);

/// <summary>
/// エンチャント適用結果
/// </summary>
public record EnchantApplyResult(bool Success, string Message, EnchantmentType? AppliedType = null);

/// <summary>
/// エンチャントシステム - 装備品に魔法効果を付与するシステム
/// </summary>
public static class EnchantmentSystem
{
    private static readonly Dictionary<EnchantmentType, EnchantmentDefinition> _definitions = new();

    static EnchantmentSystem()
    {
        RegisterDefaultEnchantments();
    }

    #region Definitions

    private static void RegisterDefaultEnchantments()
    {
        Register(new EnchantmentDefinition(EnchantmentType.FireDamage, "火炎付与",
            "武器に炎の力を宿す", SoulGemQuality.Fragment, 30, 1.2f, Element.Fire));
        Register(new EnchantmentDefinition(EnchantmentType.IceDamage, "氷結付与",
            "武器に氷の力を宿す", SoulGemQuality.Fragment, 30, 1.2f, Element.Ice));
        Register(new EnchantmentDefinition(EnchantmentType.LightningDamage, "雷撃付与",
            "武器に雷の力を宿す", SoulGemQuality.Small, 40, 1.3f, Element.Lightning));
        Register(new EnchantmentDefinition(EnchantmentType.PoisonDamage, "毒付与",
            "武器に毒の力を宿す", SoulGemQuality.Fragment, 25, 1.15f, Element.Poison));
        Register(new EnchantmentDefinition(EnchantmentType.HolyDamage, "神聖付与",
            "武器に聖なる力を宿す", SoulGemQuality.Medium, 50, 1.4f, Element.Holy));
        Register(new EnchantmentDefinition(EnchantmentType.DarkDamage, "暗黒付与",
            "武器に闇の力を宿す", SoulGemQuality.Medium, 50, 1.4f, Element.Dark));
        Register(new EnchantmentDefinition(EnchantmentType.Lifesteal, "吸血",
            "攻撃時にHPを吸収する", SoulGemQuality.Medium, 60, 0.1f));
        Register(new EnchantmentDefinition(EnchantmentType.ManaSteal, "マナ吸収",
            "攻撃時にMPを吸収する", SoulGemQuality.Medium, 60, 0.08f));
        Register(new EnchantmentDefinition(EnchantmentType.ParalysisChance, "麻痺付与",
            "攻撃時に確率で麻痺を付与する", SoulGemQuality.Small, 40, 0.15f));
        Register(new EnchantmentDefinition(EnchantmentType.ExpBoost, "経験値増加",
            "獲得経験値が増加する", SoulGemQuality.Large, 80, 1.15f));
        Register(new EnchantmentDefinition(EnchantmentType.DropBoost, "ドロップ率上昇",
            "アイテムドロップ率が上昇する", SoulGemQuality.Large, 80, 1.2f));
        Register(new EnchantmentDefinition(EnchantmentType.CriticalBoost, "クリティカル率上昇",
            "クリティカル率が上昇する", SoulGemQuality.Small, 35, 0.1f));
        Register(new EnchantmentDefinition(EnchantmentType.SpeedBoost, "攻撃速度上昇",
            "攻撃速度が上昇する", SoulGemQuality.Small, 35, 0.15f));
        Register(new EnchantmentDefinition(EnchantmentType.DefenseBoost, "防御力上昇",
            "防御力が上昇する", SoulGemQuality.Fragment, 25, 5.0f));
        Register(new EnchantmentDefinition(EnchantmentType.Thorns, "反射ダメージ",
            "被攻撃時にダメージを反射する", SoulGemQuality.Grand, 100, 0.2f));
    }

    private static void Register(EnchantmentDefinition def)
    {
        _definitions[def.Type] = def;
    }

    #endregion

    #region Soul Gem

    /// <summary>
    /// 敵ランクから魂石品質を変換
    /// </summary>
    public static SoulGemQuality GetSoulGemQualityFromRank(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => SoulGemQuality.Fragment,
        EnemyRank.Elite => SoulGemQuality.Small,
        EnemyRank.Rare => SoulGemQuality.Medium,
        EnemyRank.Boss => SoulGemQuality.Large,
        EnemyRank.HiddenBoss => SoulGemQuality.Grand,
        _ => SoulGemQuality.Fragment
    };

    /// <summary>
    /// 魂石品質ごとの成功率を取得
    /// </summary>
    public static float GetSuccessRate(SoulGemQuality quality) => quality switch
    {
        SoulGemQuality.Fragment => 0.50f,
        SoulGemQuality.Small => 0.60f,
        SoulGemQuality.Medium => 0.70f,
        SoulGemQuality.Large => 0.85f,
        SoulGemQuality.Grand => 0.95f,
        _ => 0.50f
    };

    /// <summary>
    /// 魂石品質の日本語名を取得
    /// </summary>
    public static string GetSoulGemName(SoulGemQuality quality) => quality switch
    {
        SoulGemQuality.Fragment => "魂石の欠片",
        SoulGemQuality.Small => "小さな魂石",
        SoulGemQuality.Medium => "魂石",
        SoulGemQuality.Large => "大きな魂石",
        SoulGemQuality.Grand => "極大魂石",
        _ => "魂石"
    };

    #endregion

    #region Enchantment Operations

    /// <summary>
    /// 付呪可能か判定
    /// </summary>
    public static bool CanEnchant(EquipmentItem item, EnchantmentType type, SoulGemQuality gem)
    {
        if (!_definitions.TryGetValue(type, out var definition))
            return false;

        return gem >= definition.RequiredQuality;
    }

    /// <summary>
    /// 付呪実行
    /// </summary>
    public static EnchantApplyResult Enchant(EquipmentItem item, EnchantmentType type, SoulGemQuality gem, IRandomProvider random)
    {
        if (!_definitions.TryGetValue(type, out var definition))
            return new EnchantApplyResult(false, "不明なエンチャントタイプです");

        if (!CanEnchant(item, type, gem))
            return new EnchantApplyResult(false, "魂石の品質が不足しています");

        float successRate = GetSuccessRate(gem);
        if (random.NextDouble() < successRate)
        {
            // BT-5: エンチャントをアイテムに永続保存
            item.AppliedEnchantments.Add(type.ToString());
            return new EnchantApplyResult(true, $"{item.Name}に{definition.Name}を付与した！", type);
        }

        return new EnchantApplyResult(false, "エンチャントに失敗した…");
    }

    /// <summary>
    /// 使用可能なエンチャント一覧を取得
    /// </summary>
    public static IReadOnlyList<EnchantmentDefinition> GetAvailableEnchantments(SoulGemQuality gem)
    {
        return _definitions.Values
            .Where(d => gem >= d.RequiredQuality)
            .OrderBy(d => d.Type)
            .ToList();
    }

    /// <summary>
    /// エンチャント情報を取得
    /// </summary>
    public static EnchantmentDefinition? GetEnchantmentInfo(EnchantmentType type)
    {
        return _definitions.GetValueOrDefault(type);
    }

    /// <summary>
    /// 全エンチャント定義を取得
    /// </summary>
    public static IReadOnlyList<EnchantmentDefinition> GetAllEnchantments()
    {
        return _definitions.Values.ToList();
    }

    /// <summary>
    /// エンチャントによるダメージボーナスを計算
    /// </summary>
    public static int CalculateEnchantedDamageBonus(EnchantmentType type, int baseDamage)
    {
        if (!_definitions.TryGetValue(type, out var definition))
            return 0;

        return type switch
        {
            EnchantmentType.FireDamage or
            EnchantmentType.IceDamage or
            EnchantmentType.LightningDamage or
            EnchantmentType.PoisonDamage or
            EnchantmentType.HolyDamage or
            EnchantmentType.DarkDamage =>
                (int)(baseDamage * (definition.EffectValue - 1.0f)),

            EnchantmentType.Thorns =>
                (int)(baseDamage * definition.EffectValue),

            _ => 0
        };
    }

    #endregion
}
