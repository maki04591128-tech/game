namespace RougelikeGame.Core.Systems;

/// <summary>
/// 種族特性の効果定義
/// </summary>
public record RacialTrait(
    RacialTraitType Type,
    string Name,
    string Description,
    double Value = 0.0)
{
    /// <summary>特性の効果を説明テキストとして取得</summary>
    public string GetEffectDescription() => Type switch
    {
        RacialTraitType.ExpBonus => $"経験値獲得+{Value * 100:0}%",
        RacialTraitType.MagicDamageBonus => $"魔法ダメージ+{Value * 100:0}%",
        RacialTraitType.MagicCostReduction => $"魔法コスト-{Value * 100:0}%",
        RacialTraitType.DarkVision => "暗所での視界ペナルティ軽減",
        RacialTraitType.PoisonResistance => "毒耐性",
        RacialTraitType.MiningKnowledge => "採掘関連アイテム発見率上昇",
        RacialTraitType.BerserkerBlood => $"HP50%以下で攻撃力+{Value * 100:0}%",
        RacialTraitType.Intimidation => "弱い敵が逃げやすくなる",
        RacialTraitType.KeenSenses => $"探知範囲+{Value:0}",
        RacialTraitType.WildIntuition => "罠の事前感知確率上昇",
        RacialTraitType.LuckyBody => $"クリティカル率+{Value * 100:0}%、アイテムドロップ率上昇",
        RacialTraitType.StealthMovement => "隠密移動時の被発見率低下",
        RacialTraitType.PoisonImmunity => "毒状態無効",
        RacialTraitType.NoFoodRequired => "満腹度が減少しない",
        RacialTraitType.DarkAffinity => "闇属性ダメージ+20%、闇属性耐性+30%",
        RacialTraitType.ManaAbsorption => "魔法攻撃を受けた時MP回復",
        RacialTraitType.Levitation => "穴落下罠無効、水上移動可能",
        RacialTraitType.DualElement => "光・闇両属性使用可能",
        RacialTraitType.PhysicalResistance => $"物理ダメージ-{Value * 100:0}%",
        RacialTraitType.Split => "HP50%以下で分裂（1回のみ）",
        RacialTraitType.EquipmentRestriction => "装備可能アイテムが制限される",
        RacialTraitType.Adaptability => "全クラスのスキル習得コスト低下",
        RacialTraitType.SkillCostReduction => $"スキル習得コスト-{Value * 100:0}%",
        _ => Description
    };
}

/// <summary>
/// 種族特性システム - 種族ごとのゲーム効果を管理
/// </summary>
public static class RacialTraitSystem
{
    private static readonly Dictionary<Race, List<RacialTrait>> _traits = new()
    {
        [Race.Human] = new()
        {
            new(RacialTraitType.Adaptability, "適応力", "全クラスのスキル習得コスト-10%", 0.10),
            new(RacialTraitType.ExpBonus, "学習速度上昇", "経験値獲得+10%", 0.10)
        },
        [Race.Elf] = new()
        {
            new(RacialTraitType.MagicDamageBonus, "魔力親和", "魔法ダメージ+15%", 0.15),
            new(RacialTraitType.DarkVision, "暗視", "暗所での視界ペナルティ軽減", 2.0)
        },
        [Race.Dwarf] = new()
        {
            new(RacialTraitType.PoisonResistance, "毒耐性", "毒ダメージ半減", 0.50),
            new(RacialTraitType.MiningKnowledge, "採掘知識", "鉱石系アイテム発見率上昇", 0.30)
        },
        [Race.Orc] = new()
        {
            new(RacialTraitType.BerserkerBlood, "狂戦士の血", "HP50%以下で攻撃力+25%", 0.25),
            new(RacialTraitType.Intimidation, "威圧", "弱い敵が逃げやすくなる", 0.0)
        },
        [Race.Beastfolk] = new()
        {
            new(RacialTraitType.KeenSenses, "鋭敏感覚", "探知範囲+2", 2.0),
            new(RacialTraitType.WildIntuition, "野生の勘", "罠の事前感知確率上昇", 0.20)
        },
        [Race.Halfling] = new()
        {
            new(RacialTraitType.LuckyBody, "幸運体質", "クリティカル率+5%、ドロップ率上昇", 0.05),
            new(RacialTraitType.StealthMovement, "隠密行動", "隠密移動時の被発見率低下", 0.30)
        },
        [Race.Undead] = new()
        {
            new(RacialTraitType.PoisonImmunity, "毒無効", "毒状態完全無効", 1.0),
            new(RacialTraitType.NoFoodRequired, "食事不要", "満腹度が減少しない", 1.0)
        },
        [Race.Demon] = new()
        {
            new(RacialTraitType.DarkAffinity, "闇属性親和", "闇属性ダメージ+20%、耐性+30%", 0.20),
            new(RacialTraitType.ManaAbsorption, "魔力吸収", "魔法攻撃を受けた時MP5%回復", 0.05)
        },
        [Race.FallenAngel] = new()
        {
            new(RacialTraitType.Levitation, "浮遊", "穴落下罠無効、水上移動可能", 1.0),
            new(RacialTraitType.DualElement, "光闇両属性", "光・闇両属性の魔法を使用可能", 1.0)
        },
        [Race.Slime] = new()
        {
            new(RacialTraitType.PhysicalResistance, "物理耐性", "物理ダメージ-25%", 0.25),
            new(RacialTraitType.Split, "分裂", "HP50%以下で分裂（味方スライム召喚）", 1.0),
            new(RacialTraitType.EquipmentRestriction, "装備制限", "武器・防具の大部分が装備不可", 1.0)
        }
    };

    /// <summary>種族の特性一覧を取得</summary>
    public static IReadOnlyList<RacialTrait> GetTraits(Race race) =>
        _traits.TryGetValue(race, out var traits) ? traits : Array.Empty<RacialTrait>();

    /// <summary>種族が特定の特性を持っているか</summary>
    public static bool HasTrait(Race race, RacialTraitType type) =>
        GetTraits(race).Any(t => t.Type == type);

    /// <summary>特性の値を取得（持っていない場合は0）</summary>
    public static double GetTraitValue(Race race, RacialTraitType type) =>
        GetTraits(race).FirstOrDefault(t => t.Type == type)?.Value ?? 0.0;

    /// <summary>経験値倍率を計算（種族特性適用）</summary>
    public static double CalculateExpMultiplier(Race race)
    {
        double bonus = GetTraitValue(race, RacialTraitType.ExpBonus);
        return 1.0 + bonus;
    }

    /// <summary>魔法ダメージ倍率を計算</summary>
    public static double CalculateMagicDamageMultiplier(Race race)
    {
        double bonus = GetTraitValue(race, RacialTraitType.MagicDamageBonus);
        return 1.0 + bonus;
    }

    /// <summary>物理ダメージ軽減率を計算</summary>
    public static double CalculatePhysicalResistance(Race race)
    {
        return GetTraitValue(race, RacialTraitType.PhysicalResistance);
    }

    /// <summary>毒が無効かどうか</summary>
    public static bool IsPoisonImmune(Race race) =>
        HasTrait(race, RacialTraitType.PoisonImmunity);

    /// <summary>食事が不要かどうか</summary>
    public static bool IsNoFoodRequired(Race race) =>
        HasTrait(race, RacialTraitType.NoFoodRequired);

    /// <summary>浮遊しているか</summary>
    public static bool IsLevitating(Race race) =>
        HasTrait(race, RacialTraitType.Levitation);

    /// <summary>装備制限があるか</summary>
    public static bool HasEquipmentRestriction(Race race) =>
        HasTrait(race, RacialTraitType.EquipmentRestriction);

    /// <summary>視界ボーナスを取得</summary>
    public static int GetSightBonus(Race race)
    {
        if (HasTrait(race, RacialTraitType.DarkVision))
            return (int)GetTraitValue(race, RacialTraitType.DarkVision);
        if (HasTrait(race, RacialTraitType.KeenSenses))
            return (int)GetTraitValue(race, RacialTraitType.KeenSenses);
        return 0;
    }

    /// <summary>スキル習得コスト倍率を計算</summary>
    public static double CalculateSkillCostMultiplier(Race race)
    {
        if (HasTrait(race, RacialTraitType.Adaptability))
            return 1.0 - GetTraitValue(race, RacialTraitType.Adaptability);
        if (HasTrait(race, RacialTraitType.SkillCostReduction))
            return 1.0 - GetTraitValue(race, RacialTraitType.SkillCostReduction);
        return 1.0;
    }

    /// <summary>狂戦士の血ボーナスを計算（HP割合に応じた攻撃力ボーナス）</summary>
    public static double CalculateBerserkerBonus(Race race, int currentHp, int maxHp)
    {
        if (!HasTrait(race, RacialTraitType.BerserkerBlood)) return 0.0;
        if (maxHp <= 0) return 0.0;

        double hpRatio = (double)currentHp / maxHp;
        if (hpRatio <= 0.5)
            return GetTraitValue(race, RacialTraitType.BerserkerBlood);
        return 0.0;
    }
}

/// <summary>
/// 職業装備適性システム
/// </summary>
public static class ClassEquipmentSystem
{
    private static readonly Dictionary<CharacterClass, HashSet<EquipmentCategory>> _proficiencies = new()
    {
        [CharacterClass.Fighter] = new() { EquipmentCategory.Sword, EquipmentCategory.Axe, EquipmentCategory.Mace, EquipmentCategory.Shield, EquipmentCategory.HeavyArmor, EquipmentCategory.MediumArmor },
        [CharacterClass.Knight] = new() { EquipmentCategory.Sword, EquipmentCategory.Mace, EquipmentCategory.Shield, EquipmentCategory.HeavyArmor, EquipmentCategory.MediumArmor },
        [CharacterClass.Thief] = new() { EquipmentCategory.Dagger, EquipmentCategory.Sword, EquipmentCategory.Bow, EquipmentCategory.LightArmor },
        [CharacterClass.Ranger] = new() { EquipmentCategory.Bow, EquipmentCategory.Dagger, EquipmentCategory.Sword, EquipmentCategory.MediumArmor, EquipmentCategory.LightArmor },
        [CharacterClass.Mage] = new() { EquipmentCategory.Staff, EquipmentCategory.Wand, EquipmentCategory.Robe },
        [CharacterClass.Cleric] = new() { EquipmentCategory.Mace, EquipmentCategory.Shield, EquipmentCategory.Staff, EquipmentCategory.MediumArmor, EquipmentCategory.Robe },
        [CharacterClass.Monk] = new() { EquipmentCategory.Fist, EquipmentCategory.Staff, EquipmentCategory.LightArmor, EquipmentCategory.Robe },
        [CharacterClass.Bard] = new() { EquipmentCategory.Sword, EquipmentCategory.Dagger, EquipmentCategory.Bow, EquipmentCategory.LightArmor, EquipmentCategory.MediumArmor },
        [CharacterClass.Alchemist] = new() { EquipmentCategory.Dagger, EquipmentCategory.Staff, EquipmentCategory.Wand, EquipmentCategory.LightArmor, EquipmentCategory.Robe },
        [CharacterClass.Necromancer] = new() { EquipmentCategory.Staff, EquipmentCategory.Wand, EquipmentCategory.Dagger, EquipmentCategory.Robe }
    };

    /// <summary>職業が装備カテゴリに習熟しているか</summary>
    public static bool IsProficient(CharacterClass cls, EquipmentCategory category) =>
        _proficiencies.TryGetValue(cls, out var set) && set.Contains(category);

    /// <summary>職業の習熟装備カテゴリ一覧を取得</summary>
    public static IReadOnlySet<EquipmentCategory> GetProficiencies(CharacterClass cls) =>
        _proficiencies.TryGetValue(cls, out var set) ? set : new HashSet<EquipmentCategory>();

    /// <summary>装備適性による攻撃力倍率（習熟: 1.0, 非習熟: 0.7）</summary>
    public static double GetProficiencyMultiplier(CharacterClass cls, EquipmentCategory category) =>
        IsProficient(cls, category) ? 1.0 : 0.7;
}

/// <summary>
/// 素性システム - 素性ごとの初期装備・開始地点・クリア条件
/// </summary>
public record BackgroundBonusData(
    Background Background,
    TerritoryId StartTerritory,
    string[] InitialItemIds,
    string ClearConditionDescription,
    string ClearConditionFlag)
{
    private static readonly Dictionary<Background, BackgroundBonusData> All = new()
    {
        [Background.Adventurer] = new(Background.Adventurer, TerritoryId.Capital,
            new[] { "iron_sword", "leather_armor", "healing_potion", "healing_potion", "bread" },
            "ダンジョン最深部到達", "dungeon_clear"),
        [Background.Soldier] = new(Background.Soldier, TerritoryId.Capital,
            new[] { "iron_sword", "iron_shield", "chain_mail", "healing_potion" },
            "ボスモンスター10体撃破", "boss_kills_10"),
        [Background.Scholar] = new(Background.Scholar, TerritoryId.Capital,
            new[] { "wooden_staff", "robe", "scroll_identify", "scroll_identify", "ancient_book" },
            "全ルーン語の習得", "all_runes_learned"),
        [Background.Merchant] = new(Background.Merchant, TerritoryId.Capital,
            new[] { "dagger", "leather_armor", "healing_potion", "bread", "bread" },
            "100,000ゴールド貯蓄", "gold_100000"),
        [Background.Peasant] = new(Background.Peasant, TerritoryId.Forest,
            new[] { "wooden_club", "cloth_armor", "bread", "bread", "bread" },
            "レベル30到達", "level_30"),
        [Background.Noble] = new(Background.Noble, TerritoryId.Capital,
            new[] { "silver_sword", "noble_garb", "healing_potion", "healing_potion", "healing_potion" },
            "全領地の解放", "all_territories"),
        [Background.Wanderer] = new(Background.Wanderer, TerritoryId.Frontier,
            new[] { "dagger", "traveler_cloak", "healing_potion", "bread", "torch" },
            "全領地訪問", "all_territories_visited"),
        [Background.Criminal] = new(Background.Criminal, TerritoryId.Southern,
            new[] { "dagger", "dagger", "leather_armor", "lockpick", "lockpick" },
            "隠しフロア全発見", "all_secret_floors"),
        [Background.Priest] = new(Background.Priest, TerritoryId.Capital,
            new[] { "wooden_staff", "robe", "holy_water", "healing_potion" },
            "最高信仰段階到達", "faith_saint"),
        [Background.Penitent] = new(Background.Penitent, TerritoryId.Mountain,
            new[] { "rusty_sword", "torn_clothes" },
            "正気度100で全ボス撃破", "sanity_perfect_clear")
    };

    public static BackgroundBonusData Get(Background bg) => All[bg];
    public static IReadOnlyDictionary<Background, BackgroundBonusData> GetAll() => All;
}
