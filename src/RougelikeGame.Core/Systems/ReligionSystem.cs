namespace RougelikeGame.Core.Systems;

/// <summary>
/// 宗教定義
/// </summary>
public record ReligionDefinition(
    ReligionId Id,
    string Name,
    string GodName,
    string Description,
    Element PrimaryElement,
    TerritoryId HomeTerritory,
    int Difficulty,
    ReligionBenefit[] Benefits,
    ReligionTaboo[] Taboos,
    string[] GrantedSkills)
{
    /// <summary>信仰段階を計算（6段階）</summary>
    public static FaithRank GetFaithRank(int faithPoints) => faithPoints switch
    {
        >= 100 => FaithRank.Saint,
        >= 81 => FaithRank.Champion,
        >= 61 => FaithRank.Priest,
        >= 41 => FaithRank.Blessed,
        >= 21 => FaithRank.Devout,
        >= 1 => FaithRank.Believer,
        _ => FaithRank.None
    };

    /// <summary>信仰段階名を取得</summary>
    public static string GetFaithRankName(FaithRank rank) => rank switch
    {
        FaithRank.Saint => "聖人",
        FaithRank.Champion => "聖騎士",
        FaithRank.Priest => "司祭",
        FaithRank.Blessed => "祝福者",
        FaithRank.Devout => "敬虔",
        FaithRank.Believer => "信者",
        _ => "無信仰"
    };

    /// <summary>宗教別の信仰段階名を取得</summary>
    public string GetRankTitle(FaithRank rank) => Id switch
    {
        ReligionId.LightTemple => rank switch
        {
            FaithRank.Saint => "太陽の聖人",
            FaithRank.Champion => "聖騎士",
            FaithRank.Priest => "司祭",
            FaithRank.Blessed => "祝福者",
            FaithRank.Devout => "敬虔なる信者",
            FaithRank.Believer => "光の信者",
            _ => "無信仰"
        },
        ReligionId.DarkCult => rank switch
        {
            FaithRank.Saint => "深淵の主",
            FaithRank.Champion => "闇の司教",
            FaithRank.Priest => "闇の司祭",
            FaithRank.Blessed => "深淵の従者",
            FaithRank.Devout => "闇の使徒",
            FaithRank.Believer => "入信者",
            _ => "無信仰"
        },
        ReligionId.NatureWorship => rank switch
        {
            FaithRank.Saint => "世界樹の守護者",
            FaithRank.Champion => "大ドルイド",
            FaithRank.Priest => "森の司祭",
            FaithRank.Blessed => "大地の友",
            FaithRank.Devout => "自然の従者",
            FaithRank.Believer => "緑の信者",
            _ => "無信仰"
        },
        ReligionId.DeathFaith => rank switch
        {
            FaithRank.Saint => "タナトスの化身",
            FaithRank.Champion => "死の代弁者",
            FaithRank.Priest => "死の司祭",
            FaithRank.Blessed => "死者の友",
            FaithRank.Devout => "死の理解者",
            FaithRank.Believer => "求道者",
            _ => "無信仰"
        },
        ReligionId.ChaosCult => rank switch
        {
            FaithRank.Saint => "カオスの化身",
            FaithRank.Champion => "混沌の司祭",
            FaithRank.Priest => "変革者",
            FaithRank.Blessed => "混沌の使徒",
            FaithRank.Devout => "変異者",
            FaithRank.Believer => "狂信者",
            _ => "無信仰"
        },
        ReligionId.Atheism => rank switch
        {
            FaithRank.Saint => "超人",
            FaithRank.Champion => "鋼の意志",
            FaithRank.Priest => "不屈の心",
            FaithRank.Blessed => "自立者",
            FaithRank.Devout => "懐疑者",
            FaithRank.Believer => "無神論者",
            _ => "無信仰"
        },
        _ => GetFaithRankName(rank)
    };
}

/// <summary>
/// 宗教恩恵
/// </summary>
public record ReligionBenefit(
    FaithRank RequiredRank,
    string Name,
    string Description,
    ReligionBenefitType Type,
    double Value = 0.0);

/// <summary>
/// 宗教禁忌
/// </summary>
public record ReligionTaboo(
    string Name,
    string Description,
    ReligionTabooType Type,
    int FaithPenalty);

/// <summary>
/// 背教者の呪い定義
/// </summary>
public record ApostasyCurse(
    ReligionId SourceReligion,
    string Name,
    string Description,
    int DurationDays,
    bool IsPermanent = false);

/// <summary>
/// 死に戻り時の宗教効果
/// </summary>
public record RebirthEffect(
    ReligionId Religion,
    string Name,
    string Description);

/// <summary>
/// 恩恵タイプ
/// </summary>
public enum ReligionBenefitType
{
    HealingBonus,
    DamageBonus,
    DefenseBonus,
    ExpBonus,
    StatusResistance,
    SpecialSkill,
    ShopDiscount,
    ItemFind,
    SanityRecovery,
    HungerReduction,
    MpRegeneration,
    CriticalBonus,
    DarkResistance,
    UndeadDamageBonus,
    NatureHealing,
    PoisonResistance,
    DeathResistance,
    ChaosResistance,
    RandomBuff,
    SkillCostReduction,
    AllStatsBonus,
    InstantDeathImmunity,
    SanityLossReduction
}

/// <summary>
/// 禁忌タイプ
/// </summary>
public enum ReligionTabooType
{
    KillInnocent,
    UseDarkMagic,
    UseLightMagic,
    EatMeat,
    DestroyNature,
    HelpUndead,
    KillUndead,
    UseHolyItems,
    Steal,
    Lie,
    ActChaotic,
    ActOrderly,
    Pray,
    UseResurrection,
    FearDeath
}

/// <summary>
/// 宗教データベース
/// </summary>
public static class ReligionDatabase
{
    private static readonly Dictionary<ReligionId, ReligionDefinition> _religions = new();
    private static readonly Dictionary<(ReligionId, ReligionId), ReligionRelation> _relations = new();
    private static readonly Dictionary<ReligionId, ApostasyCurse> _apostasyCurses = new();
    private static readonly Dictionary<ReligionId, RebirthEffect> _rebirthEffects = new();

    static ReligionDatabase()
    {
        InitializeReligions();
        InitializeRelations();
        InitializeApostasyCurses();
        InitializeRebirthEffects();
    }

    private static void InitializeReligions()
    {
        // 光の神殿
        _religions[ReligionId.LightTemple] = new(
            ReligionId.LightTemple, "光の神殿", "太陽神ソラリス",
            "光と正義を司る最大の宗教。癒しと浄化の力をもたらす",
            Element.Light, TerritoryId.Capital, 1,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "光の加護", "回復魔法の効果+5%", ReligionBenefitType.HealingBonus, 0.05),
                new ReligionBenefit(FaithRank.Devout, "聖なる光", "アンデッドへのダメージ+15%", ReligionBenefitType.UndeadDamageBonus, 0.15),
                new ReligionBenefit(FaithRank.Blessed, "浄化の力", "回復魔法の効果+15%、状態異常耐性+10%", ReligionBenefitType.StatusResistance, 0.10),
                new ReligionBenefit(FaithRank.Priest, "聖なる盾", "アンデッドへのダメージ+25%、状態異常耐性+20%", ReligionBenefitType.StatusResistance, 0.20),
                new ReligionBenefit(FaithRank.Champion, "太陽の戦士", "光属性ダメージ+20%、HP自然回復", ReligionBenefitType.HealingBonus, 0.30),
                new ReligionBenefit(FaithRank.Saint, "太陽の聖人", "全ステータス+5%、正気度回復可能", ReligionBenefitType.AllStatsBonus, 0.05)
            },
            Taboos: new[]
            {
                new ReligionTaboo("殺害禁止", "無辜の者を殺してはならない", ReligionTabooType.KillInnocent, 15),
                new ReligionTaboo("闇魔法禁止", "闇属性の魔法を使ってはならない", ReligionTabooType.UseDarkMagic, 10),
                new ReligionTaboo("窃盗禁止", "盗みを行ってはならない", ReligionTabooType.Steal, 10)
            },
            GrantedSkills: new[] { "holy_light", "purify", "divine_protection", "divine_miracle" });

        // 闇の教団
        _religions[ReligionId.DarkCult] = new(
            ReligionId.DarkCult, "闇の教団", "深淵神ニュクス",
            "闇と秘密を司る。強大な魔力と引き換えに精神を蝕む",
            Element.Dark, TerritoryId.Southern, 3,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "闇の力", "闇属性ダメージ+5%", ReligionBenefitType.DamageBonus, 0.05),
                new ReligionBenefit(FaithRank.Devout, "闇の守り", "闇属性耐性+20%、隠密+10%", ReligionBenefitType.DarkResistance, 0.20),
                new ReligionBenefit(FaithRank.Blessed, "深淵の知識", "闇属性ダメージ+15%、毒耐性+30%", ReligionBenefitType.PoisonResistance, 0.30),
                new ReligionBenefit(FaithRank.Priest, "深淵の従者", "魔法スキルコスト-20%", ReligionBenefitType.SkillCostReduction, 0.20),
                new ReligionBenefit(FaithRank.Champion, "闇の司教", "闇属性ダメージ+20%、MP回復速度2倍", ReligionBenefitType.MpRegeneration, 2.0),
                new ReligionBenefit(FaithRank.Saint, "深淵の主", "MP回復速度3倍、闇属性ダメージ+50%", ReligionBenefitType.MpRegeneration, 3.0)
            },
            Taboos: new[]
            {
                new ReligionTaboo("光魔法禁止", "光属性の魔法を使ってはならない", ReligionTabooType.UseLightMagic, 15),
                new ReligionTaboo("聖物禁止", "聖なるアイテムを使ってはならない", ReligionTabooType.UseHolyItems, 10)
            },
            GrantedSkills: new[] { "dark_embrace", "life_drain", "aura_of_fear", "abyssal_gate" });

        // 自然崇拝
        _religions[ReligionId.NatureWorship] = new(
            ReligionId.NatureWorship, "自然崇拝", "大地母神ガイア",
            "自然の力を崇拝する。治癒と成長の力をもたらす",
            Element.Earth, TerritoryId.Forest, 2,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "自然の癒し", "フロア移動時HP3%回復", ReligionBenefitType.NatureHealing, 0.03),
                new ReligionBenefit(FaithRank.Devout, "大地の守り", "毒耐性+25%、自然環境でHP回復+10%", ReligionBenefitType.PoisonResistance, 0.25),
                new ReligionBenefit(FaithRank.Blessed, "動物の友", "動物と中立、自然魔法+15%", ReligionBenefitType.NatureHealing, 0.15),
                new ReligionBenefit(FaithRank.Priest, "満腹の恵み", "満腹度減少速度-50%", ReligionBenefitType.HungerReduction, 0.50),
                new ReligionBenefit(FaithRank.Champion, "変身の力", "変身スキル使用可能", ReligionBenefitType.SpecialSkill, 1.0),
                new ReligionBenefit(FaithRank.Saint, "世界樹の加護", "全属性耐性+20%、経験値+15%", ReligionBenefitType.ExpBonus, 0.15)
            },
            Taboos: new[]
            {
                new ReligionTaboo("自然破壊禁止", "自然を破壊する行為を行ってはならない", ReligionTabooType.DestroyNature, 15),
                new ReligionTaboo("肉食禁止", "肉を食べてはならない", ReligionTabooType.EatMeat, 5)
            },
            GrantedSkills: new[] { "nature_heal", "beast_summon", "shapeshift", "world_tree_protection" });

        // 死神信仰
        _religions[ReligionId.DeathFaith] = new(
            ReligionId.DeathFaith, "死神信仰", "死神タナトス",
            "死を管理する神への信仰。死への理解がもたらす力",
            Element.Dark, TerritoryId.Mountain, 4,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "死の理解", "死に戻り時の正気度減少-10%", ReligionBenefitType.SanityLossReduction, 0.10),
                new ReligionBenefit(FaithRank.Devout, "死の抵抗", "アンデッドと中立、闇属性耐性+15%", ReligionBenefitType.DarkResistance, 0.15),
                new ReligionBenefit(FaithRank.Blessed, "死の予感", "「死の予感」スキル習得、即死耐性+30%", ReligionBenefitType.DeathResistance, 0.30),
                new ReligionBenefit(FaithRank.Priest, "死の支配", "アンデッド召喚可能", ReligionBenefitType.SpecialSkill, 1.0),
                new ReligionBenefit(FaithRank.Champion, "死の代弁者", "死に戻り時の正気度減少-30%", ReligionBenefitType.SanityLossReduction, 0.30),
                new ReligionBenefit(FaithRank.Saint, "タナトスの化身", "即死攻撃無効、「死の宣告」発動可能", ReligionBenefitType.InstantDeathImmunity, 1.0)
            },
            Taboos: new[]
            {
                new ReligionTaboo("蘇生禁止", "蘇生魔法を使ってはならない", ReligionTabooType.UseResurrection, 20),
                new ReligionTaboo("アンデッド撃破禁止", "アンデッドを殺してはならない", ReligionTabooType.KillUndead, 15),
                new ReligionTaboo("死への恐怖", "逃走コマンドの多用", ReligionTabooType.FearDeath, 10),
                new ReligionTaboo("光への協力", "光の神殿に協力してはならない", ReligionTabooType.UseLightMagic, 15)
            },
            GrantedSkills: new[] { "death_premonition", "soul_harvest", "guide_of_dead", "death_sentence" });

        // 混沌の崇拝
        _religions[ReligionId.ChaosCult] = new(
            ReligionId.ChaosCult, "混沌の崇拝", "混沌神カオス",
            "混沌を信仰する。予測不能な力と引き換えに不安定さをもたらす",
            Element.None, TerritoryId.Frontier, 5,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "混沌の波動", "ランダムバフ付与（毎日変化）", ReligionBenefitType.RandomBuff, 0.15),
                new ReligionBenefit(FaithRank.Devout, "変異", "変異を1つ獲得（ランダム、強力）", ReligionBenefitType.RandomBuff, 0.25),
                new ReligionBenefit(FaithRank.Blessed, "混沌の使徒", "混沌耐性+30%、「混沌の波動」スキル", ReligionBenefitType.ChaosResistance, 0.30),
                new ReligionBenefit(FaithRank.Priest, "変革者", "追加変異、現実歪曲スキル", ReligionBenefitType.CriticalBonus, 0.15),
                new ReligionBenefit(FaithRank.Champion, "混沌の司祭", "複数の変異、混沌魔法使用可能", ReligionBenefitType.RandomBuff, 0.40),
                new ReligionBenefit(FaithRank.Saint, "カオスの化身", "「混沌の渦」発動可能、完全ランダム存在", ReligionBenefitType.RandomBuff, 0.50)
            },
            Taboos: new[]
            {
                new ReligionTaboo("秩序的行動禁止", "規則正しい行動を取ってはならない", ReligionTabooType.ActOrderly, 5),
                new ReligionTaboo("祈祷禁止", "他の神に祈ってはならない", ReligionTabooType.Pray, 20),
                new ReligionTaboo("変化の拒否禁止", "変異の拒否をしてはならない", ReligionTabooType.ActOrderly, 10)
            },
            GrantedSkills: new[] { "chaos_wave", "reality_warp", "mutation_release", "chaos_vortex" });

        // 無神論
        _religions[ReligionId.Atheism] = new(
            ReligionId.Atheism, "無神論", "なし",
            "神を信じない道。自分の力のみで立ち向かう",
            Element.None, TerritoryId.Capital, 3,
            Benefits: new[]
            {
                new ReligionBenefit(FaithRank.Believer, "自立の精神", "全ステータス+2%", ReligionBenefitType.AllStatsBonus, 0.02),
                new ReligionBenefit(FaithRank.Devout, "不屈の意志", "状態異常耐性+15%", ReligionBenefitType.StatusResistance, 0.15),
                new ReligionBenefit(FaithRank.Blessed, "経験値強化", "経験値+10%", ReligionBenefitType.ExpBonus, 0.10),
                new ReligionBenefit(FaithRank.Priest, "鋼の心", "正気度減少量-20%", ReligionBenefitType.SanityLossReduction, 0.20),
                new ReligionBenefit(FaithRank.Champion, "鋼の意志", "全ステータス+5%", ReligionBenefitType.AllStatsBonus, 0.05),
                new ReligionBenefit(FaithRank.Saint, "超人", "全ステータス+10%、HP/MP+20%", ReligionBenefitType.AllStatsBonus, 0.10)
            },
            Taboos: new[]
            {
                new ReligionTaboo("神頼み禁止", "神殿での祈祷や宗教施設の利用不可", ReligionTabooType.Pray, 30)
            },
            GrantedSkills: Array.Empty<string>());
    }

    private static void InitializeRelations()
    {
        // 光の神殿の関係
        _relations[(ReligionId.LightTemple, ReligionId.DarkCult)] = ReligionRelation.Hostile;
        _relations[(ReligionId.LightTemple, ReligionId.NatureWorship)] = ReligionRelation.Friendly;
        _relations[(ReligionId.LightTemple, ReligionId.DeathFaith)] = ReligionRelation.Hostile;
        _relations[(ReligionId.LightTemple, ReligionId.ChaosCult)] = ReligionRelation.Hostile;
        _relations[(ReligionId.LightTemple, ReligionId.Atheism)] = ReligionRelation.Neutral;

        // 闇の教団の関係
        _relations[(ReligionId.DarkCult, ReligionId.LightTemple)] = ReligionRelation.Hostile;
        _relations[(ReligionId.DarkCult, ReligionId.NatureWorship)] = ReligionRelation.Neutral;
        _relations[(ReligionId.DarkCult, ReligionId.DeathFaith)] = ReligionRelation.Allied;
        _relations[(ReligionId.DarkCult, ReligionId.ChaosCult)] = ReligionRelation.Friendly;
        _relations[(ReligionId.DarkCult, ReligionId.Atheism)] = ReligionRelation.Neutral;

        // 自然崇拝の関係
        _relations[(ReligionId.NatureWorship, ReligionId.LightTemple)] = ReligionRelation.Friendly;
        _relations[(ReligionId.NatureWorship, ReligionId.DarkCult)] = ReligionRelation.Neutral;
        _relations[(ReligionId.NatureWorship, ReligionId.DeathFaith)] = ReligionRelation.Neutral;
        _relations[(ReligionId.NatureWorship, ReligionId.ChaosCult)] = ReligionRelation.Hostile;
        _relations[(ReligionId.NatureWorship, ReligionId.Atheism)] = ReligionRelation.Neutral;

        // 死神信仰の関係
        _relations[(ReligionId.DeathFaith, ReligionId.LightTemple)] = ReligionRelation.Hostile;
        _relations[(ReligionId.DeathFaith, ReligionId.DarkCult)] = ReligionRelation.Allied;
        _relations[(ReligionId.DeathFaith, ReligionId.NatureWorship)] = ReligionRelation.Neutral;
        _relations[(ReligionId.DeathFaith, ReligionId.ChaosCult)] = ReligionRelation.Neutral;
        _relations[(ReligionId.DeathFaith, ReligionId.Atheism)] = ReligionRelation.Hostile;

        // 混沌の崇拝の関係
        _relations[(ReligionId.ChaosCult, ReligionId.LightTemple)] = ReligionRelation.Hostile;
        _relations[(ReligionId.ChaosCult, ReligionId.DarkCult)] = ReligionRelation.Friendly;
        _relations[(ReligionId.ChaosCult, ReligionId.NatureWorship)] = ReligionRelation.Hostile;
        _relations[(ReligionId.ChaosCult, ReligionId.DeathFaith)] = ReligionRelation.Neutral;
        _relations[(ReligionId.ChaosCult, ReligionId.Atheism)] = ReligionRelation.Neutral;

        // 無神論の関係
        _relations[(ReligionId.Atheism, ReligionId.LightTemple)] = ReligionRelation.Neutral;
        _relations[(ReligionId.Atheism, ReligionId.DarkCult)] = ReligionRelation.Neutral;
        _relations[(ReligionId.Atheism, ReligionId.NatureWorship)] = ReligionRelation.Neutral;
        _relations[(ReligionId.Atheism, ReligionId.DeathFaith)] = ReligionRelation.Hostile;
        _relations[(ReligionId.Atheism, ReligionId.ChaosCult)] = ReligionRelation.Neutral;
    }

    private static void InitializeApostasyCurses()
    {
        _apostasyCurses[ReligionId.LightTemple] = new(
            ReligionId.LightTemple, "光の裁き",
            "光属性に弱体化、聖堂に入れない", 30);

        _apostasyCurses[ReligionId.DarkCult] = new(
            ReligionId.DarkCult, "深淵の追跡",
            "暗殺者が送られる、闇商人利用不可", 0, IsPermanent: true);

        _apostasyCurses[ReligionId.NatureWorship] = new(
            ReligionId.NatureWorship, "自然の怒り",
            "動物に敵対される、自然回復-50%", 30);

        _apostasyCurses[ReligionId.DeathFaith] = new(
            ReligionId.DeathFaith, "死の呪縛",
            "死に戻り時の正気度減少+50%", 0, IsPermanent: true);

        _apostasyCurses[ReligionId.ChaosCult] = new(
            ReligionId.ChaosCult, "混沌の暴走",
            "変異が暴走する可能性、ランダムデバフ", 0, IsPermanent: true);
    }

    private static void InitializeRebirthEffects()
    {
        _rebirthEffects[ReligionId.LightTemple] = new(
            ReligionId.LightTemple, "神の導き",
            "開始地点にヒントが出る");

        _rebirthEffects[ReligionId.DarkCult] = new(
            ReligionId.DarkCult, "深淵の記憶",
            "死因となった敵の情報を得る");

        _rebirthEffects[ReligionId.NatureWorship] = new(
            ReligionId.NatureWorship, "循環の理",
            "植物系アイテムを1つ保持");

        _rebirthEffects[ReligionId.DeathFaith] = new(
            ReligionId.DeathFaith, "死の祝福",
            "正気度減少軽減");

        _rebirthEffects[ReligionId.ChaosCult] = new(
            ReligionId.ChaosCult, "混沌の再誕",
            "ランダムにステータス変化");

        _rebirthEffects[ReligionId.Atheism] = new(
            ReligionId.Atheism, "経験の記録",
            "経験値を10%保持");
    }

    public static ReligionDefinition? GetById(ReligionId id) =>
        _religions.TryGetValue(id, out var religion) ? religion : null;

    public static IEnumerable<ReligionDefinition> GetAll() => _religions.Values;

    public static int Count => _religions.Count;

    /// <summary>宗教間関係を取得</summary>
    public static ReligionRelation GetRelation(ReligionId a, ReligionId b) =>
        a == b ? ReligionRelation.Allied :
        _relations.TryGetValue((a, b), out var relation) ? relation : ReligionRelation.Neutral;

    /// <summary>背教者の呪いを取得</summary>
    public static ApostasyCurse? GetApostasyCurse(ReligionId religionId) =>
        _apostasyCurses.TryGetValue(religionId, out var curse) ? curse : null;

    /// <summary>死に戻り効果を取得</summary>
    public static RebirthEffect? GetRebirthEffect(ReligionId religionId) =>
        _rebirthEffects.TryGetValue(religionId, out var effect) ? effect : null;

    /// <summary>敵対宗教のIDリストを取得</summary>
    public static IReadOnlyList<ReligionId> GetHostileReligions(ReligionId religionId) =>
        _relations
            .Where(kv => kv.Key.Item1 == religionId && kv.Value == ReligionRelation.Hostile)
            .Select(kv => kv.Key.Item2)
            .ToList();

    /// <summary>友好宗教のIDリストを取得</summary>
    public static IReadOnlyList<ReligionId> GetFriendlyReligions(ReligionId religionId) =>
        _relations
            .Where(kv => kv.Key.Item1 == religionId &&
                   (kv.Value == ReligionRelation.Friendly || kv.Value == ReligionRelation.Allied))
            .Select(kv => kv.Key.Item2)
            .ToList();
}

/// <summary>
/// 宗教システム - 信仰度管理・恩恵適用・禁忌チェック
/// </summary>
public class ReligionSystem
{
    /// <summary>入信する</summary>
    public ReligionActionResult JoinReligion(Entities.Player player, ReligionId religionId)
    {
        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null)
            return new ReligionActionResult(false, "不明な宗教");

        // 既に同じ宗教に入信済み
        if (player.CurrentReligion == religionId.ToString())
            return new ReligionActionResult(false, "既にこの宗教に入信している");

        // 改宗の場合
        if (player.CurrentReligion != null)
        {
            return ConvertReligion(player, religionId);
        }

        player.JoinReligion(religionId.ToString());
        // IT-1: Player.JoinReligion()で既にInitialFaithOnJoinが設定されるため追加不要

        // 宗教スキルを付与
        foreach (var skillId in religion.GrantedSkills)
        {
            player.LearnSkill(skillId);
        }

        return new ReligionActionResult(true,
            $"{religion.Name}に入信した。{religion.GodName}の加護を得た");
    }

    /// <summary>改宗する</summary>
    private ReligionActionResult ConvertReligion(Entities.Player player, ReligionId newReligionId)
    {
        var newReligion = ReligionDatabase.GetById(newReligionId);
        if (newReligion == null)
            return new ReligionActionResult(false, "不明な宗教");

        string oldReligionName = player.CurrentReligion ?? "無信仰";

        // 背教者の呪いを適用
        if (Enum.TryParse<ReligionId>(player.CurrentReligion, out var oldReligionId))
        {
            var curse = ReligionDatabase.GetApostasyCurse(oldReligionId);
            if (curse != null)
            {
                player.HasApostasyCurse = true;
                player.ApostasyCurseRemainingDays = curse.IsPermanent ? -1 : curse.DurationDays;
            }
        }

        // 改宗ペナルティ: 正気度減少
        player.ModifySanity(-GameConstants.ConversionSanityPenalty);
        player.LeaveReligion();

        // 新しい宗教に入信
        player.JoinReligion(newReligionId.ToString());
        // IT-1: JoinReligionで初期値20が設定されるため、改宗値10に調整
        player.AddFaithPoints(GameConstants.InitialFaithOnConvert - GameConstants.InitialFaithOnJoin);

        foreach (var skillId in newReligion.GrantedSkills)
        {
            player.LearnSkill(skillId);
        }

        return new ReligionActionResult(true,
            $"{oldReligionName}を捨て、{newReligion.Name}に改宗した（正気度-{GameConstants.ConversionSanityPenalty}）");
    }

    /// <summary>脱退する</summary>
    public ReligionActionResult LeaveReligion(Entities.Player player)
    {
        if (player.CurrentReligion == null)
            return new ReligionActionResult(false, "信仰している宗教がない");

        // 背教者の呪いを適用
        if (Enum.TryParse<ReligionId>(player.CurrentReligion, out var religionId))
        {
            var curse = ReligionDatabase.GetApostasyCurse(religionId);
            if (curse != null)
            {
                player.HasApostasyCurse = true;
                player.ApostasyCurseRemainingDays = curse.IsPermanent ? -1 : curse.DurationDays;
            }
        }

        player.ModifySanity(-GameConstants.LeaveSanityPenalty);
        player.LeaveReligion();
        return new ReligionActionResult(true,
            $"宗教を脱退した（正気度-{GameConstants.LeaveSanityPenalty}）");
    }

    /// <summary>祈りを捧げる（1日1回）</summary>
    public ReligionActionResult Pray(Entities.Player player)
    {
        if (player.CurrentReligion == null)
            return new ReligionActionResult(false, "信仰している宗教がない");

        if (player.HasPrayedToday)
            return new ReligionActionResult(false, "今日は既に祈りを捧げた");

        player.HasPrayedToday = true;
        player.DaysSinceLastPrayer = 0;
        player.AddFaithPoints(GameConstants.PrayFaithGain);

        // F-2: 宗教固有の祈り効果
        string extraEffect = ApplyPrayerEffect(player, player.CurrentReligion);

        var rank = ReligionDefinition.GetFaithRank(player.FaithPoints);
        var rankName = ReligionDefinition.GetFaithRankName(rank);

        string msg = $"祈りを捧げた。信仰度+{GameConstants.PrayFaithGain}（現在: {player.FaithPoints}、{rankName}）";
        if (!string.IsNullOrEmpty(extraEffect)) msg += $"\n{extraEffect}";
        return new ReligionActionResult(true, msg);
    }

    /// <summary>AW-3: 献金/供物メカニクス — ゴールドを捧げて信仰ポイントを獲得</summary>
    public ReligionActionResult Donate(Entities.Player player, int goldAmount)
    {
        if (player.CurrentReligion == null)
            return new ReligionActionResult(false, "信仰している宗教がない");
        if (goldAmount <= 0)
            return new ReligionActionResult(false, "献金額が不正");
        if (player.Gold < goldAmount)
            return new ReligionActionResult(false, "ゴールドが足りない");

        player.AddGold(-goldAmount);
        // 100ゴールドごとに信仰ポイント1（最低1）
        int faithGain = Math.Max(1, goldAmount / 100);
        player.AddFaithPoints(faithGain);

        var rank = ReligionDefinition.GetFaithRank(player.FaithPoints);
        var rankName = ReligionDefinition.GetFaithRankName(rank);
        return new ReligionActionResult(true,
            $"{goldAmount}Gを献金した。信仰度+{faithGain}（現在: {player.FaithPoints}、{rankName}）");
    }

    /// <summary>F-2: 宗教固有の祈り効果を適用</summary>
    private string ApplyPrayerEffect(Entities.Player player, string religionId)
    {
        return religionId switch
        {
            "temple_of_light" => PrayHeal(player),
            "dark_cult" => PrayDarkPower(player),
            "nature_faith" => PrayNature(player),
            "death_faith" => PrayDeath(player),
            "chaos_cult" => PrayChaos(player),
            _ => ""
        };
    }

    private static string PrayHeal(Entities.Player player)
    {
        int heal = Math.Max(5, player.MaxHp / 10);
        player.Heal(heal);
        return $"✨ 光の祝福: HP+{heal}回復";
    }

    private static string PrayDarkPower(Entities.Player player)
    {
        int mpRestore = Math.Max(3, player.MaxMp / 10);
        player.RestoreMp(mpRestore);
        return $"🌑 闇の力: MP+{mpRestore}回復";
    }

    private static string PrayNature(Entities.Player player)
    {
        int hungerRestore = 10;
        player.ModifyHunger(hungerRestore);
        return $"🌿 自然の恵み: 満腹度+{hungerRestore}";
    }

    private static string PrayDeath(Entities.Player player)
    {
        int sanityRestore = 5;
        player.ModifySanity(sanityRestore);
        return $"💀 死の静寂: 正気度+{sanityRestore}";
    }

    private static string PrayChaos(Entities.Player player)
    {
        int spRestore = Math.Max(5, player.MaxSp / 10);
        player.RestoreSp(spRestore);
        return $"🌀 混沌の活力: SP+{spRestore}回復";
    }

    /// <summary>信仰度を増加させる</summary>
    public void AddFaith(Entities.Player player, int amount)
    {
        player.AddFaithPoints(amount);
    }

    /// <summary>禁忌違反による信仰度減少</summary>
    public ReligionActionResult ViolateTaboo(Entities.Player player, ReligionTabooType tabooType)
    {
        if (player.CurrentReligion == null)
            return new ReligionActionResult(false, "信仰なし");

        if (!Enum.TryParse<ReligionId>(player.CurrentReligion, out var religionId))
            return new ReligionActionResult(false, "不明な宗教");

        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null)
            return new ReligionActionResult(false, "不明な宗教");

        var taboo = religion.Taboos.FirstOrDefault(t => t.Type == tabooType);
        if (taboo == null)
            return new ReligionActionResult(false, "この行為は禁忌ではない");

        player.AddFaithPoints(-taboo.FaithPenalty);
        return new ReligionActionResult(true,
            $"禁忌「{taboo.Name}」を犯した！信仰度-{taboo.FaithPenalty}");
    }

    /// <summary>現在の信仰段階で有効な恩恵を取得</summary>
    public IReadOnlyList<ReligionBenefit> GetActiveBenefits(Entities.Player player)
    {
        if (player.CurrentReligion == null) return Array.Empty<ReligionBenefit>();
        if (!Enum.TryParse<ReligionId>(player.CurrentReligion, out var religionId))
            return Array.Empty<ReligionBenefit>();

        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null) return Array.Empty<ReligionBenefit>();

        var currentRank = ReligionDefinition.GetFaithRank(player.FaithPoints);
        return religion.Benefits.Where(b => b.RequiredRank <= currentRank).ToList();
    }

    /// <summary>特定タイプの恩恵値を合算して取得</summary>
    public double GetBenefitValue(Entities.Player player, ReligionBenefitType type)
    {
        var benefits = GetActiveBenefits(player);
        return benefits.Where(b => b.Type == type).Sum(b => b.Value);
    }

    /// <summary>特定タイプの恩恵が有効かチェック</summary>
    public bool HasBenefit(Entities.Player player, ReligionBenefitType type)
    {
        return GetActiveBenefits(player).Any(b => b.Type == type);
    }

    /// <summary>日次処理（祈りリセット、信仰度自然減少、呪い経過）</summary>
    public void ProcessDailyTick(Entities.Player player)
    {
        // 祈りフラグリセット
        player.HasPrayedToday = false;

        // 祈らない日のカウント
        player.DaysSinceLastPrayer++;

        // 長期間祈らない場合の信仰度自然減少
        if (player.CurrentReligion != null &&
            player.DaysSinceLastPrayer >= GameConstants.FaithDecayInterval)
        {
            player.AddFaithPoints(-GameConstants.FaithDecayAmount);
            player.DaysSinceLastPrayer = 0;
        }

        // 背教者の呪いの経過
        if (player.HasApostasyCurse && player.ApostasyCurseRemainingDays > 0)
        {
            player.ApostasyCurseRemainingDays--;
            if (player.ApostasyCurseRemainingDays <= 0)
            {
                player.HasApostasyCurse = false;
            }
        }
    }

    /// <summary>死に戻り時の信仰度引き継ぎを計算（通常80%、死神信仰は段階に応じて上昇）</summary>
    public int CalculateDeathTransferFaith(Entities.Player player)
    {
        int faith = player.FaithPoints;
        double ratio = GameConstants.FaithRetentionRate;

        if (player.CurrentReligion == ReligionId.DeathFaith.ToString())
        {
            var rank = ReligionDefinition.GetFaithRank(faith);
            ratio = rank switch
            {
                FaithRank.Saint => 1.00,
                FaithRank.Champion => 0.95,
                FaithRank.Priest => 0.90,
                _ => GameConstants.FaithRetentionRate
            };
        }

        return (int)(faith * ratio);
    }

    /// <summary>死に戻り時の特殊効果を取得</summary>
    public RebirthEffect? GetRebirthEffect(Entities.Player player)
    {
        if (player.CurrentReligion == null) return null;
        if (!Enum.TryParse<ReligionId>(player.CurrentReligion, out var religionId))
            return null;
        return ReligionDatabase.GetRebirthEffect(religionId);
    }

    /// <summary>現在の宗教情報を取得</summary>
    public ReligionStatusInfo GetStatus(Entities.Player player)
    {
        if (player.CurrentReligion == null)
            return new ReligionStatusInfo("無信仰", "なし", FaithRank.None, 0, "無信仰", Array.Empty<ReligionBenefit>());

        if (!Enum.TryParse<ReligionId>(player.CurrentReligion, out var religionId))
            return new ReligionStatusInfo("不明", "不明", FaithRank.None, 0, "不明", Array.Empty<ReligionBenefit>());

        var religion = ReligionDatabase.GetById(religionId);
        if (religion == null)
            return new ReligionStatusInfo("不明", "不明", FaithRank.None, 0, "不明", Array.Empty<ReligionBenefit>());

        var rank = ReligionDefinition.GetFaithRank(player.FaithPoints);
        var title = religion.GetRankTitle(rank);
        var benefits = GetActiveBenefits(player);

        return new ReligionStatusInfo(
            religion.Name, religion.GodName, rank,
            player.FaithPoints, title, benefits);
    }

    /// <summary>敵対宗教の信者を倒した時の信仰度上昇</summary>
    public ReligionActionResult OnDefeatHostileFollower(Entities.Player player, ReligionId defeatedReligion)
    {
        if (player.CurrentReligion == null)
            return new ReligionActionResult(false, "信仰なし");

        if (!Enum.TryParse<ReligionId>(player.CurrentReligion, out var playerReligion))
            return new ReligionActionResult(false, "不明な宗教");

        var relation = ReligionDatabase.GetRelation(playerReligion, defeatedReligion);
        if (relation != ReligionRelation.Hostile)
            return new ReligionActionResult(false, "敵対宗教ではない");

        int faithGain = 3;
        player.AddFaithPoints(faithGain);
        return new ReligionActionResult(true, $"敵対宗教の信者を倒した！信仰度+{faithGain}");
    }
}

/// <summary>
/// 宗教アクション結果
/// </summary>
public record ReligionActionResult(bool Success, string Message);

/// <summary>
/// 宗教ステータス情報
/// </summary>
public record ReligionStatusInfo(
    string ReligionName,
    string GodName,
    FaithRank Rank,
    int FaithPoints,
    string Title,
    IReadOnlyList<ReligionBenefit> ActiveBenefits);
