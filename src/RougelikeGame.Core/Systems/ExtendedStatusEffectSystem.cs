namespace RougelikeGame.Core.Systems;

/// <summary>
/// 拡張状態異常の定義
/// </summary>
public record ExtendedStatusEffect(
    string Id,
    string Name,
    string Description,
    int DefaultDuration,
    bool IsBuff,
    StatusEffectType? BaseType,
    Dictionary<string, float> StatModifiers
);

/// <summary>
/// 状態異常拡充システム - 追加の状態異常を管理
/// </summary>
public static class ExtendedStatusEffectSystem
{
    private static readonly Dictionary<string, ExtendedStatusEffect> Effects = new()
    {
        ["intoxication"] = new("intoxication", "酩酊", "命中率-15%、STR+2、回避率+5%（酔拳効果）", 10, false, null,
            new() { ["HitRate"] = -0.15f, ["STR"] = 2, ["EvasionRate"] = 0.05f }),

        ["frostbite"] = new("frostbite", "凍傷", "AGI-5、移動コスト+25%、継続ダメージ(小)", 15, false, null,
            new() { ["AGI"] = -5, ["MoveCost"] = 0.25f, ["DotDamage"] = 2 }),

        ["infection"] = new("infection", "感染症", "VIT-3、HP自然回復停止、放置で重症化", 20, false, null,
            new() { ["VIT"] = -3, ["HpRegen"] = -1.0f }),

        ["bind"] = new("bind", "呪縛", "移動不可、回避率-50%、防御行動のみ可能", 5, false, null,
            new() { ["MoveSpeed"] = -1.0f, ["EvasionRate"] = -0.5f }),

        ["hallucination"] = new("hallucination", "幻惑", "マップにダミー敵表示、アイテム誤認率30%", 8, false, null,
            new() { ["Perception"] = -0.3f }),

        ["vampiric_drain"] = new("vampiric_drain", "吸血", "毎ターンHP-3、攻撃者HP+3", 10, false, null,
            new() { ["DotDamage"] = 3, ["LeechAmount"] = 3 }),

        ["berserk"] = new("berserk", "狂戦士", "攻撃力+50%、被ダメージ+30%、対象選択不可", 6, false, null,
            new() { ["AttackMultiplier"] = 1.5f, ["DamageTaken"] = 0.3f }),  // V-6: 攻撃力+50%

        ["corrosion"] = new("corrosion", "腐食", "防御力-20%、装備耐久値減少2倍", 12, false, null,
            new() { ["DefenseMultiplier"] = -0.2f, ["DurabilityWearMultiplier"] = 2.0f }),

        ["marked"] = new("marked", "標的", "受けるクリティカル率+20%、回避不可", 5, false, null,
            new() { ["CritReceiveRate"] = 0.2f, ["EvasionRate"] = -1.0f }),

        ["blessed_weapon"] = new("blessed_weapon", "神聖武装", "武器に聖属性追加、不死系に+30%ダメージ", 15, true, null,
            new() { ["HolyDamageBonus"] = 0.3f }),

        ["iron_skin"] = new("iron_skin", "鉄の肌", "物理被ダメージ-25%、AGI-3", 10, true, null,
            new() { ["PhysicalDefense"] = 0.25f, ["AGI"] = -3 }),

        ["haste_burst"] = new("haste_burst", "疾風", "行動速度+50%、SP消費+30%", 5, true, null,
            new() { ["ActionSpeed"] = 0.5f, ["SpCost"] = 0.3f }),
    };

    /// <summary>全拡張状態異常を取得</summary>
    public static IReadOnlyDictionary<string, ExtendedStatusEffect> GetAll() => Effects;

    /// <summary>IDから拡張状態異常を取得</summary>
    public static ExtendedStatusEffect? GetById(string id)
    {
        return Effects.TryGetValue(id, out var effect) ? effect : null;
    }

    /// <summary>バフのみ取得</summary>
    public static IReadOnlyList<ExtendedStatusEffect> GetBuffs()
    {
        return Effects.Values.Where(e => e.IsBuff).ToList();
    }

    /// <summary>デバフのみ取得</summary>
    public static IReadOnlyList<ExtendedStatusEffect> GetDebuffs()
    {
        return Effects.Values.Where(e => !e.IsBuff).ToList();
    }

    /// <summary>特定の状態異常のステータス修正値を取得</summary>
    public static float GetStatModifier(string effectId, string statName)
    {
        if (!Effects.TryGetValue(effectId, out var effect)) return 0;
        return effect.StatModifiers.GetValueOrDefault(statName);
    }

    /// <summary>状態異常名を取得</summary>
    public static string GetEffectName(string effectId)
    {
        return Effects.TryGetValue(effectId, out var effect) ? effect.Name : "不明";
    }
}
