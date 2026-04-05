namespace RougelikeGame.Core.Systems;

/// <summary>
/// 種族定義
/// </summary>
public record RaceDefinition(
    Race Race,
    string Name,
    string Description,
    StatModifier StatBonus,
    int HpBonus,
    int MpBonus,
    string[] Traits,
    double ExpMultiplier,
    double SanityLossMultiplier)
{
    private static readonly Dictionary<Race, RaceDefinition> All = new()
    {
        [Race.Human] = new(Race.Human, "人間", "バランスの取れた万能種族",
            new StatModifier(Luck: 2, Charisma: 1),
            HpBonus: 0, MpBonus: 0,
            Traits: ["適応力", "学習速度上昇"],
            ExpMultiplier: 1.1, SanityLossMultiplier: 1.0),

        [Race.Elf] = new(Race.Elf, "エルフ", "魔法に秀でた長命種族",
            new StatModifier(Intelligence: 3, Mind: 2, Agility: 1, Vitality: -2),
            HpBonus: -10, MpBonus: 20,
            Traits: ["魔力親和", "暗視"],
            ExpMultiplier: 1.0, SanityLossMultiplier: 0.9),

        [Race.Dwarf] = new(Race.Dwarf, "ドワーフ", "頑強な山岳民族",
            new StatModifier(Vitality: 3, Strength: 2, Agility: -2),
            HpBonus: 20, MpBonus: -10,
            Traits: ["毒耐性", "採掘知識"],
            ExpMultiplier: 1.0, SanityLossMultiplier: 0.85),

        [Race.Orc] = new(Race.Orc, "オーク", "圧倒的な力を持つ戦闘種族",
            new StatModifier(Strength: 4, Vitality: 2, Intelligence: -3, Charisma: -2),
            HpBonus: 15, MpBonus: -15,
            Traits: ["狂戦士の血", "威圧"],
            ExpMultiplier: 0.95, SanityLossMultiplier: 1.2),

        [Race.Beastfolk] = new(Race.Beastfolk, "獣人", "獣の力と人の知恵を併せ持つ",
            new StatModifier(Agility: 3, Perception: 2, Dexterity: 1, Mind: -2),
            HpBonus: 5, MpBonus: -5,
            Traits: ["鋭敏感覚", "野生の勘"],
            ExpMultiplier: 1.0, SanityLossMultiplier: 1.0),

        [Race.Halfling] = new(Race.Halfling, "ハーフリング", "小柄だが幸運に恵まれた種族",
            new StatModifier(Luck: 4, Dexterity: 2, Agility: 1, Strength: -3),
            HpBonus: -10, MpBonus: 0,
            Traits: ["幸運体質", "隠密行動"],
            ExpMultiplier: 1.05, SanityLossMultiplier: 0.95),

        [Race.Undead] = new(Race.Undead, "アンデッド", "死を超越した不死の存在",
            new StatModifier(Vitality: 2, Mind: 2, Charisma: -4, Luck: -2),
            HpBonus: 10, MpBonus: 10,
            Traits: ["毒無効", "食事不要"],
            ExpMultiplier: 0.9, SanityLossMultiplier: 1.3),

        [Race.Demon] = new(Race.Demon, "悪魔", "強大な力を持つ魔界の住人",
            new StatModifier(Strength: 2, Intelligence: 2, Mind: 2, Charisma: -3, Luck: -3),
            HpBonus: 10, MpBonus: 15,
            Traits: ["闇属性親和", "魔力吸収"],
            ExpMultiplier: 0.85, SanityLossMultiplier: 1.4),

        [Race.FallenAngel] = new(Race.FallenAngel, "堕天使", "天界を追放された高位存在",
            new StatModifier(Intelligence: 2, Mind: 3, Agility: 2, Perception: 1, Luck: -4),
            HpBonus: 0, MpBonus: 25,
            Traits: ["浮遊", "光闇両属性"],
            ExpMultiplier: 0.8, SanityLossMultiplier: 1.5),

        [Race.Slime] = new(Race.Slime, "スライム", "不定形の特殊種族",
            new StatModifier(Vitality: 4, Agility: -2, Dexterity: -3, Charisma: -4),
            HpBonus: 30, MpBonus: 0,
            Traits: ["物理耐性", "分裂", "装備制限"],
            ExpMultiplier: 0.75, SanityLossMultiplier: 1.6)
    };

    public static RaceDefinition Get(Race race) =>
        All.TryGetValue(race, out var def) ? def : All[Race.Human];  // ID-3: 未登録種族はHumanにフォールバック
    public static IReadOnlyDictionary<Race, RaceDefinition> GetAll() => All;

    /// <summary>Y-6: ステータス修正を含む詳細説明を取得</summary>
    public string GetDetailedDescription()
    {
        var parts = new List<string> { Description };

        // ボーナス表示
        var bonuses = new List<string>();
        var penalties = new List<string>();
        AddStatInfo(bonuses, penalties, "STR", StatBonus.Strength);
        AddStatInfo(bonuses, penalties, "VIT", StatBonus.Vitality);
        AddStatInfo(bonuses, penalties, "AGI", StatBonus.Agility);
        AddStatInfo(bonuses, penalties, "DEX", StatBonus.Dexterity);
        AddStatInfo(bonuses, penalties, "INT", StatBonus.Intelligence);
        AddStatInfo(bonuses, penalties, "MND", StatBonus.Mind);
        AddStatInfo(bonuses, penalties, "PER", StatBonus.Perception);
        AddStatInfo(bonuses, penalties, "CHA", StatBonus.Charisma);
        AddStatInfo(bonuses, penalties, "LUK", StatBonus.Luck);

        if (bonuses.Count > 0)
            parts.Add($"[ボーナス: {string.Join(", ", bonuses)}]");
        if (penalties.Count > 0)
            parts.Add($"[ペナルティ: {string.Join(", ", penalties)}]");
        if (SanityLossMultiplier != 1.0)
            parts.Add($"[正気度消耗: {SanityLossMultiplier:F1}倍]");

        return string.Join(" ", parts);
    }

    private static void AddStatInfo(List<string> bonuses, List<string> penalties, string name, int value)
    {
        if (value > 0) bonuses.Add($"{name}+{value}");
        else if (value < 0) penalties.Add($"{name}{value}");
    }
}

/// <summary>
/// 職業（クラス）定義
/// </summary>
public record ClassDefinition(
    CharacterClass Class,
    string Name,
    string Description,
    StatModifier StatBonus,
    int HpBonus,
    int MpBonus,
    string[] InitialSkills)
{
    private static readonly Dictionary<CharacterClass, ClassDefinition> All = new()
    {
        [CharacterClass.Fighter] = new(CharacterClass.Fighter, "戦士", "近接戦闘のエキスパート",
            new StatModifier(Strength: 3, Vitality: 2),
            HpBonus: 15, MpBonus: 0,
            InitialSkills: ["strong_strike", "weapon_mastery"]),

        [CharacterClass.Knight] = new(CharacterClass.Knight, "騎士", "防御と護衛に秀でた戦士",
            new StatModifier(Vitality: 3, Strength: 1, Charisma: 1),
            HpBonus: 20, MpBonus: 0,
            InitialSkills: ["shield_block", "provoke"]),

        [CharacterClass.Thief] = new(CharacterClass.Thief, "盗賊", "隠密と罠に精通した技巧派",
            new StatModifier(Dexterity: 3, Agility: 2, Perception: 1),
            HpBonus: 0, MpBonus: 0,
            InitialSkills: ["lockpick", "sneak"]),

        [CharacterClass.Ranger] = new(CharacterClass.Ranger, "狩人", "弓術と追跡の達人",
            new StatModifier(Dexterity: 2, Perception: 2, Agility: 1),
            HpBonus: 5, MpBonus: 0,
            InitialSkills: ["precise_shot", "tracking"]),

        [CharacterClass.Mage] = new(CharacterClass.Mage, "魔術師", "攻撃魔法を極めし者",
            new StatModifier(Intelligence: 4, Mind: 1, Vitality: -2),
            HpBonus: -10, MpBonus: 25,
            InitialSkills: ["mana_focus", "basic_magic"]),

        [CharacterClass.Cleric] = new(CharacterClass.Cleric, "僧侶", "信仰の力で癒しをもたらす",
            new StatModifier(Mind: 3, Intelligence: 1, Charisma: 1),
            HpBonus: 5, MpBonus: 15,
            InitialSkills: ["heal", "purify"]),

        [CharacterClass.Monk] = new(CharacterClass.Monk, "修道士", "肉体と精神を鍛え上げた格闘家",
            new StatModifier(Agility: 2, Mind: 2, Strength: 1),
            HpBonus: 10, MpBonus: 5,
            InitialSkills: ["ki_strike", "combo_strike"]),

        [CharacterClass.Bard] = new(CharacterClass.Bard, "吟遊詩人", "歌と物語で仲間を鼓舞する",
            new StatModifier(Charisma: 3, Luck: 1, Dexterity: 1),
            HpBonus: 0, MpBonus: 10,
            InitialSkills: ["inspire_song", "knowledge_collect"]),

        [CharacterClass.Alchemist] = new(CharacterClass.Alchemist, "錬金術師", "調合と付与で戦う学者",
            new StatModifier(Intelligence: 2, Perception: 2, Dexterity: 1),
            HpBonus: 0, MpBonus: 10,
            InitialSkills: ["brew", "identify"]),

        [CharacterClass.Necromancer] = new(CharacterClass.Necromancer, "死霊術師", "死者を操る闇の魔術師",
            new StatModifier(Intelligence: 3, Mind: 2, Charisma: -2),
            HpBonus: -5, MpBonus: 20,
            InitialSkills: ["summon_undead", "life_drain"])
    };

    public static ClassDefinition Get(CharacterClass cls) => All[cls];
    public static IReadOnlyDictionary<CharacterClass, ClassDefinition> GetAll() => All;
}

/// <summary>
/// 素性（バックグラウンド）定義
/// </summary>
public record BackgroundDefinition(
    Background Background,
    string Name,
    string Description,
    int StartingGold,
    StatModifier StatBonus)
{
    private static readonly Dictionary<Background, BackgroundDefinition> All = new()
    {
        [Background.Adventurer] = new(Background.Adventurer, "冒険者", "各地を巡る旅の冒険者",
            StartingGold: 100,
            StatBonus: new StatModifier(Luck: 1, Agility: 1)),

        [Background.Soldier] = new(Background.Soldier, "兵士", "軍に所属していた元兵士",
            StartingGold: 80,
            StatBonus: new StatModifier(Strength: 2, Vitality: 1)),

        [Background.Scholar] = new(Background.Scholar, "学者", "書物と知識に精通した学者",
            StartingGold: 120,
            StatBonus: new StatModifier(Intelligence: 2, Perception: 1)),

        [Background.Merchant] = new(Background.Merchant, "商人", "商売で財を成した商人",
            StartingGold: 250,
            StatBonus: new StatModifier(Charisma: 2, Luck: 1)),

        [Background.Peasant] = new(Background.Peasant, "農民", "素朴な暮らしから来た農民",
            StartingGold: 30,
            StatBonus: new StatModifier(Vitality: 2, Strength: 1)),

        [Background.Noble] = new(Background.Noble, "貴族", "没落した貴族の末裔",
            StartingGold: 300,
            StatBonus: new StatModifier(Charisma: 3)),

        [Background.Wanderer] = new(Background.Wanderer, "流浪者", "当てのない旅を続ける放浪者",
            StartingGold: 50,
            StatBonus: new StatModifier(Agility: 1, Perception: 1, Luck: 1)),

        [Background.Criminal] = new(Background.Criminal, "犯罪者", "裏社会に身を置いていた者",
            StartingGold: 150,
            StatBonus: new StatModifier(Dexterity: 2, Agility: 1)),

        [Background.Priest] = new(Background.Priest, "聖職者", "信仰に生きる聖職者",
            StartingGold: 60,
            StatBonus: new StatModifier(Mind: 2, Charisma: 1)),

        [Background.Penitent] = new(Background.Penitent, "贖罪者", "過去の罪を償うために戦う者",
            StartingGold: 10,
            StatBonus: new StatModifier(Mind: 1, Vitality: 1, Strength: 1))
    };

    public static BackgroundDefinition Get(Background bg) => All[bg];
    public static IReadOnlyDictionary<Background, BackgroundDefinition> GetAll() => All;
}
