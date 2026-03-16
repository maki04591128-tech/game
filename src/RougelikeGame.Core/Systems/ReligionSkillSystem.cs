namespace RougelikeGame.Core.Systems;

/// <summary>
/// 宗教スキル補正接続情報
/// </summary>
public record ReligionSkillBonus(
    string SkillId,
    string SkillName,
    float DamageMultiplier,
    float CostReduction,
    string Description
);

/// <summary>
/// 宗教スキル補正システム - 信仰段階に応じたスキル補正を管理
/// </summary>
public static class ReligionSkillSystem
{
    /// <summary>
    /// 信仰段階に応じたスキルダメージ倍率を取得
    /// </summary>
    public static float GetSkillDamageMultiplier(ReligionId religion, FaithRank rank)
    {
        if (rank == FaithRank.None) return 1.0f;

        float baseBonus = rank switch
        {
            FaithRank.Believer => 0.05f,
            FaithRank.Devout => 0.10f,
            FaithRank.Blessed => 0.15f,
            FaithRank.Priest => 0.20f,
            FaithRank.Champion => 0.30f,
            FaithRank.Saint => 0.40f,
            _ => 0f
        };

        // 宗教固有ボーナス
        float religionBonus = religion switch
        {
            ReligionId.LightTemple => 0.05f,   // 光の神殿：回復スキル補正
            ReligionId.DarkCult => 0.08f,       // 闇の教団：攻撃スキル補正
            ReligionId.NatureWorship => 0.03f,  // 自然崇拝：バランス補正
            ReligionId.DeathFaith => 0.10f,     // 死神信仰：即死系補正
            ReligionId.ChaosCult => 0.12f,      // 混沌崇拝：ランダム補正
            _ => 0f
        };

        return 1.0f + baseBonus + religionBonus;
    }

    /// <summary>
    /// 信仰段階に応じたスキルコスト軽減率を取得
    /// </summary>
    public static float GetSkillCostReduction(ReligionId religion, FaithRank rank)
    {
        if (rank == FaithRank.None) return 0f;

        return rank switch
        {
            FaithRank.Believer => 0.02f,
            FaithRank.Devout => 0.05f,
            FaithRank.Blessed => 0.08f,
            FaithRank.Priest => 0.12f,
            FaithRank.Champion => 0.18f,
            FaithRank.Saint => 0.25f,
            _ => 0f
        };
    }

    /// <summary>
    /// 宗教ごとのグラントスキルボーナスを取得
    /// </summary>
    public static IReadOnlyList<ReligionSkillBonus> GetGrantedSkillBonuses(ReligionId religion, FaithRank rank)
    {
        if (rank == FaithRank.None) return Array.Empty<ReligionSkillBonus>();

        return religion switch
        {
            ReligionId.LightTemple => GetLightTempleBonuses(rank),
            ReligionId.DarkCult => GetDarkCultBonuses(rank),
            ReligionId.NatureWorship => GetNatureWorshipBonuses(rank),
            ReligionId.DeathFaith => GetDeathFaithBonuses(rank),
            ReligionId.ChaosCult => GetChaosCultBonuses(rank),
            _ => Array.Empty<ReligionSkillBonus>()
        };
    }

    /// <summary>
    /// 宗教属性がスキルに適合するか判定
    /// </summary>
    public static bool IsSkillAlignedWithReligion(ReligionId religion, Element skillElement)
    {
        return religion switch
        {
            ReligionId.LightTemple => skillElement is Element.Light or Element.Holy,
            ReligionId.DarkCult => skillElement is Element.Dark or Element.Curse,
            ReligionId.NatureWorship => skillElement is Element.Earth or Element.Water or Element.Wind,
            ReligionId.DeathFaith => skillElement is Element.Dark or Element.Poison,
            ReligionId.ChaosCult => true,  // 混沌は全属性に適合
            _ => false
        };
    }

    /// <summary>
    /// 宗教属性と一致するスキルの追加ダメージ倍率
    /// </summary>
    public static float GetAlignedSkillBonus(ReligionId religion, Element skillElement, FaithRank rank)
    {
        if (!IsSkillAlignedWithReligion(religion, skillElement))
            return 1.0f;

        return 1.0f + rank switch
        {
            FaithRank.Believer => 0.10f,
            FaithRank.Devout => 0.15f,
            FaithRank.Blessed => 0.20f,
            FaithRank.Priest => 0.25f,
            FaithRank.Champion => 0.35f,
            FaithRank.Saint => 0.50f,
            _ => 0f
        };
    }

    /// <summary>
    /// 背教状態のスキルペナルティ倍率
    /// </summary>
    public static float GetApostasyPenalty(bool isApostate)
    {
        return isApostate ? 0.7f : 1.0f;
    }

    #region Religion-Specific Skill Bonuses

    private static IReadOnlyList<ReligionSkillBonus> GetLightTempleBonuses(FaithRank rank)
    {
        var bonuses = new List<ReligionSkillBonus>
        {
            new("holy_light", "聖光", 1.0f + (int)rank * 0.1f, (int)rank * 0.03f,
                "光属性の浄化スキル"),
            new("purify", "浄化", 1.0f + (int)rank * 0.08f, (int)rank * 0.04f,
                "状態異常を解除する"),
            new("divine_protection", "神聖な守護", 1.0f + (int)rank * 0.05f, (int)rank * 0.02f,
                "防御力を一時的に上昇させる")
        };

        if (rank >= FaithRank.Champion)
        {
            bonuses.Add(new("divine_miracle", "奇跡", 1.5f, 0.2f,
                "HPを大幅に回復する"));
        }

        return bonuses;
    }

    private static IReadOnlyList<ReligionSkillBonus> GetDarkCultBonuses(FaithRank rank)
    {
        var bonuses = new List<ReligionSkillBonus>
        {
            new("dark_embrace", "闇の抱擁", 1.0f + (int)rank * 0.12f, (int)rank * 0.03f,
                "闇属性の攻撃スキル"),
            new("life_drain", "生命吸収", 1.0f + (int)rank * 0.10f, (int)rank * 0.04f,
                "敵のHPを吸収する"),
            new("aura_of_fear", "恐怖のオーラ", 1.0f + (int)rank * 0.06f, (int)rank * 0.02f,
                "周囲の敵に恐怖を付与する")
        };

        if (rank >= FaithRank.Champion)
        {
            bonuses.Add(new("abyssal_gate", "深淵の門", 1.6f, 0.15f,
                "強力な闇属性範囲攻撃"));
        }

        return bonuses;
    }

    private static IReadOnlyList<ReligionSkillBonus> GetNatureWorshipBonuses(FaithRank rank)
    {
        var bonuses = new List<ReligionSkillBonus>
        {
            new("nature_heal", "自然の癒し", 1.0f + (int)rank * 0.08f, (int)rank * 0.05f,
                "HPを回復する"),
            new("beast_summon", "獣召喚", 1.0f + (int)rank * 0.07f, (int)rank * 0.03f,
                "獣を召喚して戦わせる")
        };

        if (rank >= FaithRank.Blessed)
        {
            bonuses.Add(new("shapeshift", "変身", 1.3f, 0.1f,
                "動物に変身して戦う"));
        }

        if (rank >= FaithRank.Champion)
        {
            bonuses.Add(new("world_tree_protection", "世界樹の守護", 1.4f, 0.2f,
                "全ステータスを大幅に上昇させる"));
        }

        return bonuses;
    }

    private static IReadOnlyList<ReligionSkillBonus> GetDeathFaithBonuses(FaithRank rank)
    {
        var bonuses = new List<ReligionSkillBonus>
        {
            new("death_premonition", "死の予感", 1.0f + (int)rank * 0.05f, (int)rank * 0.03f,
                "敵の弱点を見抜く"),
            new("soul_harvest", "魂刈り", 1.0f + (int)rank * 0.12f, (int)rank * 0.04f,
                "敵を倒すと追加経験値を得る"),
            new("guide_of_dead", "死者の導き", 1.0f + (int)rank * 0.06f, (int)rank * 0.02f,
                "アンデッドを一時的に従わせる")
        };

        if (rank >= FaithRank.Champion)
        {
            bonuses.Add(new("death_sentence", "死の宣告", 1.8f, 0.1f,
                "対象に即死判定を付与する"));
        }

        return bonuses;
    }

    private static IReadOnlyList<ReligionSkillBonus> GetChaosCultBonuses(FaithRank rank)
    {
        var bonuses = new List<ReligionSkillBonus>
        {
            new("chaos_wave", "混沌の波動", 1.0f + (int)rank * 0.10f, (int)rank * 0.03f,
                "ランダム属性の範囲攻撃"),
            new("reality_warp", "現実歪曲", 1.0f + (int)rank * 0.08f, (int)rank * 0.05f,
                "周囲の空間を歪める"),
            new("mutation_release", "突然変異解放", 1.0f + (int)rank * 0.09f, (int)rank * 0.02f,
                "一時的にステータスがランダムに変化する")
        };

        if (rank >= FaithRank.Champion)
        {
            bonuses.Add(new("chaos_vortex", "混沌の渦", 1.7f, 0.15f,
                "全属性の強力な攻撃"));
        }

        return bonuses;
    }

    #endregion
}
