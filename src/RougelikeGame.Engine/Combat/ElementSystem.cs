namespace RougelikeGame.Engine.Combat;

using RougelikeGame.Core;

/// <summary>
/// 属性相性システム
/// 設計書「2.3 属性相性表」に基づく実装
/// </summary>
public static class ElementSystem
{
    /// <summary>
    /// 有利属性の倍率
    /// </summary>
    public const float AdvantageMutiplier = 1.5f;

    /// <summary>
    /// 不利属性の倍率
    /// </summary>
    public const float DisadvantageMutiplier = 0.5f;

    /// <summary>
    /// 無効属性の倍率
    /// </summary>
    public const float NullifyMutiplier = 0.0f;

    /// <summary>
    /// 通常の倍率
    /// </summary>
    public const float NeutralMutiplier = 1.0f;

    /// <summary>
    /// 属性相性を取得
    /// </summary>
    /// <param name="attackElement">攻撃属性</param>
    /// <param name="targetElement">対象属性</param>
    /// <returns>ダメージ倍率</returns>
    public static float GetAffinityMultiplier(Element attackElement, Element targetElement)
    {
        // 無属性は相性なし
        if (attackElement == Element.None || targetElement == Element.None)
            return NeutralMutiplier;

        // 同属性は不利
        if (attackElement == targetElement)
            return DisadvantageMutiplier;

        return attackElement switch
        {
            Element.Fire => GetFireAffinity(targetElement),
            Element.Water => GetWaterAffinity(targetElement),
            Element.Ice => GetIceAffinity(targetElement),
            Element.Lightning => GetLightningAffinity(targetElement),
            Element.Earth => GetEarthAffinity(targetElement),
            Element.Wind => GetWindAffinity(targetElement),
            Element.Light => GetLightAffinity(targetElement),
            Element.Dark => GetDarkAffinity(targetElement),
            Element.Holy => GetHolyAffinity(targetElement),
            Element.Curse => GetCurseAffinity(targetElement),
            Element.Poison => GetPoisonAffinity(targetElement),
            _ => NeutralMutiplier
        };
    }

    /// <summary>
    /// 属性相性の説明を取得
    /// </summary>
    public static ElementAffinity GetAffinityType(Element attackElement, Element targetElement)
    {
        float multiplier = GetAffinityMultiplier(attackElement, targetElement);

        return multiplier switch
        {
            >= AdvantageMutiplier => ElementAffinity.Advantage,
            <= NullifyMutiplier => ElementAffinity.Nullify,
            <= DisadvantageMutiplier => ElementAffinity.Disadvantage,
            _ => ElementAffinity.Neutral
        };
    }

    // 火属性の相性
    // 設計書: 火 | 氷 | 水、火 | - （植物は種族ベース耐性で処理）
    private static float GetFireAffinity(Element target) => target switch
    {
        Element.Ice => AdvantageMutiplier,      // 氷に有利
        Element.Water => DisadvantageMutiplier, // 水に不利
        _ => NeutralMutiplier
    };

    // 水属性の相性
    // 設計書: 水 | 火、雷 | 氷 | - （植物は種族ベース耐性で処理）
    private static float GetWaterAffinity(Element target) => target switch
    {
        Element.Fire => AdvantageMutiplier,         // 火に有利
        Element.Lightning => AdvantageMutiplier,    // 雷に有利（導電）
        Element.Ice => DisadvantageMutiplier,       // 氷に不利
        _ => NeutralMutiplier
    };

    // 氷属性の相性
    private static float GetIceAffinity(Element target) => target switch
    {
        Element.Water => AdvantageMutiplier,    // 水に有利
        Element.Wind => AdvantageMutiplier,     // 風に有利
        Element.Fire => DisadvantageMutiplier,  // 火に不利
        _ => NeutralMutiplier
    };

    // 雷属性の相性
    private static float GetLightningAffinity(Element target) => target switch
    {
        Element.Water => AdvantageMutiplier,    // 水に有利
        Element.Earth => DisadvantageMutiplier, // 地に不利（アース）
        _ => NeutralMutiplier
    };

    // 地属性の相性
    private static float GetEarthAffinity(Element target) => target switch
    {
        Element.Lightning => AdvantageMutiplier,    // 雷に有利
        Element.Fire => AdvantageMutiplier,         // 火に有利
        Element.Wind => DisadvantageMutiplier,      // 風に不利
        _ => NeutralMutiplier
    };

    // 風属性の相性
    private static float GetWindAffinity(Element target) => target switch
    {
        Element.Earth => AdvantageMutiplier,        // 地に有利
        Element.Ice => DisadvantageMutiplier,       // 氷に不利
        Element.Lightning => DisadvantageMutiplier, // 雷に不利
        _ => NeutralMutiplier
    };

    // 光属性の相性
    private static float GetLightAffinity(Element target) => target switch
    {
        Element.Dark => AdvantageMutiplier,     // 闇に有利
        Element.Curse => AdvantageMutiplier,    // 呪いに有利
        _ => NeutralMutiplier
    };

    // 闇属性の相性
    private static float GetDarkAffinity(Element target) => target switch
    {
        Element.Light => AdvantageMutiplier,    // 光に有利
        Element.Curse => NullifyMutiplier,      // 呪い（アンデッド）には無効
        _ => NeutralMutiplier
    };

    // 聖属性の相性
    private static float GetHolyAffinity(Element target) => target switch
    {
        Element.Dark => AdvantageMutiplier,     // 闇に有利
        Element.Curse => AdvantageMutiplier,    // 呪いに有利
        _ => NeutralMutiplier
    };

    // 呪い属性の相性
    // 設計書: 呪い | 聖、光 | 闇、呪い | -
    private static float GetCurseAffinity(Element target) => target switch
    {
        Element.Holy => AdvantageMutiplier,     // 聖に有利
        Element.Light => AdvantageMutiplier,    // 光に有利
        Element.Dark => DisadvantageMutiplier,  // 闇に不利
        _ => NeutralMutiplier
    };

    // 毒属性の相性
    private static float GetPoisonAffinity(Element target) => target switch
    {
        Element.Poison => NullifyMutiplier,     // 毒には無効
        _ => NeutralMutiplier
    };

    /// <summary>
    /// 耐性を持つ属性かチェック
    /// </summary>
    public static bool HasResistance(Element defenderElement, Element attackElement)
    {
        return GetAffinityMultiplier(attackElement, defenderElement) <= DisadvantageMutiplier;
    }

    /// <summary>
    /// 弱点属性かチェック
    /// </summary>
    public static bool HasWeakness(Element defenderElement, Element attackElement)
    {
        return GetAffinityMultiplier(attackElement, defenderElement) >= AdvantageMutiplier;
    }
}

/// <summary>
/// 属性相性の種類
/// </summary>
public enum ElementAffinity
{
    /// <summary>通常</summary>
    Neutral,
    /// <summary>有利（1.5倍）</summary>
    Advantage,
    /// <summary>不利（0.5倍）</summary>
    Disadvantage,
    /// <summary>無効（0倍）</summary>
    Nullify
}
