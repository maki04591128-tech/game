namespace RougelikeGame.Core;

/// <summary>
/// 方向
/// </summary>
public enum Direction
{
    North,
    South,
    East,
    West,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest
}

/// <summary>
/// 陣営
/// </summary>
public enum Faction
{
    Player,
    Enemy,
    Neutral,
    Friendly
}

/// <summary>
/// 正気度段階
/// </summary>
public enum SanityStage
{
    Normal,     // 80-100
    Uneasy,     // 60-79
    Anxious,    // 40-59
    Unstable,   // 20-39
    Madness,    // 1-19
    Broken      // 0
}

/// <summary>
/// 満腹度段階
/// </summary>
public enum HungerStage
{
    Full,       // 80-100
    Normal,     // 50-79
    Hungry,     // 25-49
    Starving,   // 1-24
    Famished    // 0
}

/// <summary>
/// 戦闘状況フラグ
/// </summary>
[Flags]
public enum CombatState
{
    None = 0,
    Normal = 1,
    Combat = 2,
    Stealth = 4,
    Pursuit = 8,
    Alert = 16
}

/// <summary>
/// AIの状態
/// </summary>
public enum AIState
{
    Idle,
    Patrol,
    Alert,
    Combat,
    Flee
}

/// <summary>
/// 攻撃タイプ
/// </summary>
public enum AttackType
{
    Unarmed,
    Slash,
    Pierce,
    Blunt,
    Ranged,
    Magic
}

/// <summary>
/// ダメージタイプ
/// </summary>
public enum DamageType
{
    Physical,
    Magical,
    Pure,
    Healing
}

/// <summary>
/// 属性
/// </summary>
public enum Element
{
    None,
    Fire,
    Water,
    Earth,
    Wind,
    Light,
    Dark,
    Lightning,
    Ice,
    Poison,
    Holy,
    Curse
}

/// <summary>
/// 死因
/// </summary>
public enum DeathCause
{
    Combat,
    Boss,
    Starvation,
    Trap,
    TimeLimit,
    Curse,
    Suicide,
    SanityDeath,
    Fall,
    Poison,
    Unknown
}

/// <summary>
/// ターン行動タイプ
/// </summary>
public enum TurnActionType
{
    Wait,
    Move,
    Attack,
    UseSkill,
    CastSpell,
    UseItem,
    Interact,
    Search,
    Rest
}

/// <summary>
/// ゲームコマンド
/// </summary>
public enum GameCommand
{
    None,
    MoveNorth,
    MoveSouth,
    MoveEast,
    MoveWest,
    MoveNorthEast,
    MoveNorthWest,
    MoveSouthEast,
    MoveSouthWest,
    Attack,
    Skill,
    Magic,
    Item,
    Wait,
    Rest,
    OpenInventory,
    OpenStatus,
    OpenMap,
    OpenSkills,
    OpenMagic,
    Interact,
    Search,
    Quit,
    Confirm,
    Cancel
}

/// <summary>
/// アイテムタイプ
/// </summary>
public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Material,
    KeyItem,
    Scroll,
    Book
}

/// <summary>
/// 状態異常タイプ
/// </summary>
public enum StatusEffectType
{
    // バフ
    Haste,
    Strength,
    Protection,
    Regeneration,
    Invisibility,
    FireResistance,     // 火耐性
    ColdResistance,     // 冷気耐性

    // デバフ
    Slow,
    Weakness,
    Vulnerability,
    Poison,
    Bleeding,
    Burn,
    Freeze,
    Paralysis,
    Sleep,
    Confusion,
    Fear,
    Blind,
    Silence,

    // 追加デバフ
    Charm,          // 魅了（敵を攻撃できない）
    Madness,        // 狂気（敵味方無差別攻撃）
    Petrification,  // 石化（完全行動不能、防御力大幅上昇）
    InstantDeath,   // 即死（HPを0にする）
    Stun,           // スタン（一定ターン行動不可）

    // 特殊
    Apostasy,   // 背教
    Curse,
    Blessing
}

/// <summary>
/// 難易度レベル
/// </summary>
public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Nightmare,
    Ironman
}

/// <summary>
/// 種族
/// </summary>
public enum Race
{
    /// <summary>人間 - バランス型</summary>
    Human,
    /// <summary>エルフ - 魔法特化</summary>
    Elf,
    /// <summary>ドワーフ - 戦士向け</summary>
    Dwarf,
    /// <summary>オーク - 高火力</summary>
    Orc,
    /// <summary>獣人 - 機動力</summary>
    Beastfolk,
    /// <summary>ハーフリング - 幸運・隠密</summary>
    Halfling,
    /// <summary>アンデッド - 不死</summary>
    Undead,
    /// <summary>悪魔 - 強力</summary>
    Demon,
    /// <summary>堕天使 - 高性能</summary>
    FallenAngel,
    /// <summary>スライム - 特殊</summary>
    Slime
}

/// <summary>
/// 職業（クラス）
/// </summary>
public enum CharacterClass
{
    /// <summary>戦士 - 近接戦闘</summary>
    Fighter,
    /// <summary>騎士 - 防御特化</summary>
    Knight,
    /// <summary>盗賊 - 隠密・罠</summary>
    Thief,
    /// <summary>狩人 - 射撃・追跡</summary>
    Ranger,
    /// <summary>魔術師 - 攻撃魔法</summary>
    Mage,
    /// <summary>僧侶 - 回復・信仰</summary>
    Cleric,
    /// <summary>修道士 - 格闘・精神</summary>
    Monk,
    /// <summary>吟遊詩人 - 支援・交渉</summary>
    Bard,
    /// <summary>錬金術師 - 調合・付与</summary>
    Alchemist,
    /// <summary>死霊術師 - 闇魔法・召喚</summary>
    Necromancer
}

/// <summary>
/// 素性（バックグラウンド）
/// </summary>
public enum Background
{
    /// <summary>冒険者 - 標準</summary>
    Adventurer,
    /// <summary>兵士 - 戦闘経験</summary>
    Soldier,
    /// <summary>学者 - 知識豊富</summary>
    Scholar,
    /// <summary>商人 - 資金豊富</summary>
    Merchant,
    /// <summary>農民 - 素朴な出自</summary>
    Peasant,
    /// <summary>貴族 - 特権階級</summary>
    Noble,
    /// <summary>流浪者 - 放浪の旅人</summary>
    Wanderer,
    /// <summary>犯罪者 - 裏社会</summary>
    Criminal,
    /// <summary>聖職者 - 信仰篤い</summary>
    Priest,
    /// <summary>贖罪者 - 過去の罪</summary>
    Penitent
}

/// <summary>
/// 種族特性タイプ
/// </summary>
public enum RacialTraitType
{
    ExpBonus,
    MagicDamageBonus,
    MagicCostReduction,
    DarkVision,
    PoisonResistance,
    MiningKnowledge,
    BerserkerBlood,
    Intimidation,
    KeenSenses,
    WildIntuition,
    LuckyBody,
    StealthMovement,
    PoisonImmunity,
    NoFoodRequired,
    DarkAffinity,
    ManaAbsorption,
    Levitation,
    DualElement,
    PhysicalResistance,
    Split,
    EquipmentRestriction,
    Adaptability,
    SkillCostReduction
}

/// <summary>
/// 装備カテゴリ（職業適性用）
/// </summary>
public enum EquipmentCategory
{
    Sword,
    Axe,
    Mace,
    Dagger,
    Bow,
    Staff,
    Wand,
    Shield,
    HeavyArmor,
    MediumArmor,
    LightArmor,
    Robe,
    Fist
}

/// <summary>
/// 宗教ID
/// </summary>
public enum ReligionId
{
    None,
    LightTemple,
    DarkCult,
    NatureWorship,
    DeathFaith,
    ChaosCult,
    Atheism
}

/// <summary>
/// 信仰段階（信仰度に応じた6段階）
/// </summary>
public enum FaithRank
{
    None,       // 無信仰 (0)
    Believer,   // 信者 (1-20)
    Devout,     // 敬虔 (21-40)
    Blessed,    // 祝福者 (41-60)
    Priest,     // 司祭 (61-80)
    Champion,   // 聖騎士/大司教 (81-99)
    Saint       // 聖人/化身 (100)
}

/// <summary>
/// 宗教間関係
/// </summary>
public enum ReligionRelation
{
    Neutral,
    Friendly,
    Allied,
    Hostile
}

/// <summary>
/// スキルカテゴリ
/// </summary>
public enum SkillCategory
{
    Combat,
    Magic,
    Support,
    Passive,
    Crafting,
    Exploration
}

/// <summary>
/// スキルターゲット
/// </summary>
public enum SkillTarget
{
    Self,
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies,
    Area
}

/// <summary>
/// NPC種別
/// </summary>
public enum NpcType
{
    Shopkeeper,
    Blacksmith,
    Innkeeper,
    GuildMaster,
    Priest,
    Sage,
    Villager,
    QuestGiver,
    Wanderer,
    // Ver.prt.0.5 追加（P.26 NPC拡充基盤）
    /// <summary>バード（情報屋）</summary>
    Bard,
    /// <summary>魔法商人</summary>
    MagicShopkeeper,
    /// <summary>闇商人</summary>
    BlackMarketDealer,
    /// <summary>訓練師</summary>
    Trainer,
    /// <summary>薬師</summary>
    Alchemist,
    /// <summary>番人</summary>
    Guardian
}

/// <summary>
/// クエスト状態
/// </summary>
public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed,
    TurnedIn
}

/// <summary>
/// クエストタイプ
/// </summary>
public enum QuestType
{
    Kill,
    Collect,
    Explore,
    Escort,
    Deliver,
    Talk,
    /// <summary>メインクエスト</summary>
    Main
}

/// <summary>
/// ギルドランク
/// </summary>
public enum GuildRank
{
    None,
    Copper,
    Iron,
    Silver,
    Gold,
    Platinum,
    Mythril,
    Adamantine
}

/// <summary>
/// ランダムイベントタイプ
/// </summary>
public enum RandomEventType
{
    TreasureChest,
    Trap,
    Fountain,
    Shrine,
    Ruins,
    NpcEncounter,
    MerchantEncounter,
    AmbushEvent,
    RestPoint,
    MysteriousItem,
    MonsterHouse,
    CursedRoom,
    BlessedRoom,
    HiddenShop,
    MaterialDeposit
}

/// <summary>
/// 施設タイプ
/// </summary>
public enum FacilityType
{
    AdventurerGuild,
    GeneralShop,
    WeaponShop,
    ArmorShop,
    Inn,
    Smithy,
    Church,
    Temple,
    MagicShop,
    Library,
    Bank,
    Arena
}

/// <summary>
/// 領地ID
/// </summary>
public enum TerritoryId
{
    Capital,
    Forest,
    Mountain,
    Coast,
    Southern,
    Frontier
}

/// <summary>
/// 特殊フロアタイプ
/// </summary>
public enum SpecialFloorType
{
    Normal,
    Shop,
    TreasureVault,
    BossRoom,
    RestPoint,
    Arena,
    Library
}

/// <summary>
/// アイテム等級（品質）
/// </summary>
public enum ItemGrade
{
    /// <summary>粗悪品 - ステ×0.7, 価格×0.4</summary>
    Crude = 1,
    /// <summary>廉価品 - ステ×0.85, 価格×0.7</summary>
    Cheap = 2,
    /// <summary>標準品 - ステ×1.0, 価格×1.0</summary>
    Standard = 3,
    /// <summary>良品 - ステ×1.15, 価格×1.5</summary>
    Fine = 4,
    /// <summary>上質品 - ステ×1.3, 価格×2.5</summary>
    Superior = 5,
    /// <summary>傑作品 - ステ×1.5, 価格×5.0</summary>
    Masterwork = 6
}

/// <summary>
/// 敵種族（モンスター種族分類）
/// </summary>
public enum MonsterRace
{
    /// <summary>獣 - 高AGI、群れ行動、夜行性</summary>
    Beast,
    /// <summary>人型 - 装備品使用、戦術的AI</summary>
    Humanoid,
    /// <summary>不定形 - 物理耐性高、分裂能力</summary>
    Amorphous,
    /// <summary>不死 - 毒/睡眠無効、暗闘</summary>
    Undead,
    /// <summary>悪魔 - 高MND、魔法使用</summary>
    Demon,
    /// <summary>竜 - 全ステ高、ブレス攻撃</summary>
    Dragon,
    /// <summary>植物 - 再生能力、状態異常付与</summary>
    Plant,
    /// <summary>昆虫 - 高速、毒攻撃、群れ</summary>
    Insect,
    /// <summary>精霊 - 物理攻撃半減、属性攻撃</summary>
    Spirit,
    /// <summary>構造体 - 状態異常完全無効</summary>
    Construct
}

/// <summary>
/// 熟練度カテゴリ
/// </summary>
public enum ProficiencyCategory
{
    /// <summary>剣術 - 剣系武器の扱い</summary>
    Swordsmanship,
    /// <summary>槍術 - 槍系武器の扱い</summary>
    Spearmanship,
    /// <summary>弓術 - 弓・射撃武器の扱い</summary>
    Archery,
    /// <summary>格闘 - 素手・格闘武器の扱い</summary>
    MartialArts,
    /// <summary>魔術 - 魔法の行使</summary>
    Sorcery,
    /// <summary>鍛冶 - 装備の作成・強化</summary>
    Smithing,
    /// <summary>採掘 - 鉱石の採取</summary>
    Mining,
    /// <summary>錬金 - 薬品・素材の調合</summary>
    Alchemy,
    /// <summary>交渉 - 商取引・会話</summary>
    Negotiation,
    /// <summary>隠密 - 隠れ・忍び</summary>
    Stealth,
    /// <summary>探索 - 罠発見・宝箱開錠</summary>
    Exploration,
    /// <summary>信仰 - 祈り・神聖魔法</summary>
    Faith
}

/// <summary>
/// カルマ段階
/// </summary>
public enum KarmaRank
{
    /// <summary>外道 (-100 ~ -80)</summary>
    Villain,
    /// <summary>悪党 (-79 ~ -50)</summary>
    Criminal,
    /// <summary>悪漢 (-49 ~ -20)</summary>
    Rogue,
    /// <summary>中立 (-19 ~ 19)</summary>
    Neutral,
    /// <summary>普通 (20 ~ 49)</summary>
    Normal,
    /// <summary>善人 (50 ~ 79)</summary>
    Virtuous,
    /// <summary>聖人 (80 ~ 100)</summary>
    Saint
}

/// <summary>
/// 評判段階
/// </summary>
public enum ReputationRank
{
    /// <summary>憎悪 (-100 ~ -80)</summary>
    Hated,
    /// <summary>敵意 (-79 ~ -50)</summary>
    Hostile,
    /// <summary>不信 (-49 ~ -20)</summary>
    Unfriendly,
    /// <summary>無関心 (-19 ~ 19)</summary>
    Indifferent,
    /// <summary>友好 (20 ~ 49)</summary>
    Friendly,
    /// <summary>信頼 (50 ~ 79)</summary>
    Trusted,
    /// <summary>崇拝 (80 ~ 100)</summary>
    Revered
}

/// <summary>
/// 時間帯
/// </summary>
public enum TimePeriod
{
    /// <summary>早朝 (4:00-6:59)</summary>
    Dawn,
    /// <summary>午前 (7:00-11:59)</summary>
    Morning,
    /// <summary>午後 (12:00-16:59)</summary>
    Afternoon,
    /// <summary>夕方 (17:00-19:59)</summary>
    Dusk,
    /// <summary>夜 (20:00-23:59)</summary>
    Night,
    /// <summary>深夜 (0:00-3:59)</summary>
    Midnight
}

/// <summary>
/// 活動パターン
/// </summary>
public enum ActivityPattern
{
    /// <summary>昼行性</summary>
    Diurnal,
    /// <summary>夜行性</summary>
    Nocturnal,
    /// <summary>薄明薄暮性（夕方・早朝活性）</summary>
    Crepuscular,
    /// <summary>常時活動</summary>
    Constant
}

/// <summary>
/// 魂石の品質
/// </summary>
public enum SoulGemQuality
{
    /// <summary>欠片 (Common敵)</summary>
    Fragment,
    /// <summary>小 (Elite敵)</summary>
    Small,
    /// <summary>中 (Rare敵)</summary>
    Medium,
    /// <summary>大 (Boss敵)</summary>
    Large,
    /// <summary>極大 (HiddenBoss敵)</summary>
    Grand
}

/// <summary>
/// エンチャント効果の種類
/// </summary>
public enum EnchantmentType
{
    /// <summary>火炎付与</summary>
    FireDamage,
    /// <summary>氷結付与</summary>
    IceDamage,
    /// <summary>雷撃付与</summary>
    LightningDamage,
    /// <summary>毒付与</summary>
    PoisonDamage,
    /// <summary>神聖付与</summary>
    HolyDamage,
    /// <summary>暗黒付与</summary>
    DarkDamage,
    /// <summary>吸血</summary>
    Lifesteal,
    /// <summary>マナ吸収</summary>
    ManaSteal,
    /// <summary>麻痺付与</summary>
    ParalysisChance,
    /// <summary>経験値増加</summary>
    ExpBoost,
    /// <summary>ドロップ率上昇</summary>
    DropBoost,
    /// <summary>クリティカル率上昇</summary>
    CriticalBoost,
    /// <summary>攻撃速度上昇</summary>
    SpeedBoost,
    /// <summary>防御力上昇</summary>
    DefenseBoost,
    /// <summary>反射ダメージ</summary>
    Thorns
}

/// <summary>
/// ダンジョン特徴タイプ
/// </summary>
public enum DungeonFeatureType
{
    /// <summary>通常</summary>
    Standard,
    /// <summary>洞窟</summary>
    Cave,
    /// <summary>遺跡</summary>
    Ruins,
    /// <summary>下水道</summary>
    Sewer,
    /// <summary>鉱山</summary>
    Mine,
    /// <summary>墓地</summary>
    Crypt,
    /// <summary>神殿</summary>
    Temple,
    /// <summary>氷の洞窟</summary>
    IceCavern,
    /// <summary>火山</summary>
    Volcanic,
    /// <summary>森林</summary>
    Forest
}

/// <summary>
/// スキルノードの種類
/// </summary>
public enum SkillNodeType
{
    /// <summary>アクティブスキル</summary>
    Active,
    /// <summary>パッシブ強化</summary>
    Passive,
    /// <summary>ステータスノード（小）</summary>
    StatMinor,
    /// <summary>ステータスノード（大）</summary>
    StatMajor,
    /// <summary>キーストーン（強力だがデメリット付き）</summary>
    Keystone
}

/// <summary>
/// スキルツリータブ分類（PoE風5タブ）
/// </summary>
public enum SkillTreeTab
{
    /// <summary>種族タブ</summary>
    Race,
    /// <summary>職業タブ</summary>
    Class,
    /// <summary>素性タブ</summary>
    Background,
    /// <summary>武器タブ</summary>
    Weapon,
    /// <summary>魔法タブ</summary>
    Magic
}

/// <summary>
/// 攻撃方向（正面/側面/背面）
/// </summary>
public enum AttackDirection
{
    /// <summary>正面攻撃</summary>
    Front,
    /// <summary>側面攻撃</summary>
    Side,
    /// <summary>背面攻撃</summary>
    Back
}

/// <summary>
/// 耐久値段階
/// </summary>
public enum DurabilityStage
{
    /// <summary>完全 (100%〜76%)</summary>
    Perfect,
    /// <summary>消耗 (75%〜51%)</summary>
    Worn,
    /// <summary>損傷 (50%〜26%)</summary>
    Damaged,
    /// <summary>危険 (25%〜1%)</summary>
    Critical,
    /// <summary>破壊 (0%)</summary>
    Broken
}

/// <summary>
/// 能力値フラグ
/// </summary>
public enum StatFlag
{
    /// <summary>STR≥25: 怪力（岩破壊/力自慢NPC）</summary>
    Herculean,
    /// <summary>INT≥25: 博識（古文書解読/賢者会話）</summary>
    Erudite,
    /// <summary>PER≥25: 鷹の目（隠しドア自動発見/遠距離察知）</summary>
    EagleEye,
    /// <summary>AGI≥25: 韋駄天（逃走確率100%/特殊ルート）</summary>
    FleetFooted,
    /// <summary>CHA≥20: 魅力的（ショップ値引/好感度↑）</summary>
    Charismatic,
    /// <summary>LUK≥20: 強運（レアドロップ↑/カジノイベント）</summary>
    Lucky,
    /// <summary>VIT≥25: 頑健（状態異常耐性↑/スタミナ上限↑）</summary>
    Robust,
    /// <summary>DEX≥25: 神業（クリティカル率↑/罠解除↑）</summary>
    Dexterous,
    /// <summary>MND≥25: 精神力（MP回復↑/恐怖・混乱耐性）</summary>
    SteadyMind
}

/// <summary>
/// フラグ条件の種別
/// </summary>
public enum FlagConditionType
{
    /// <summary>フラグ存在チェック (has:flag_name)</summary>
    HasFlag,
    /// <summary>数値比較 (karma >= 50)</summary>
    ValueCompare,
    /// <summary>能力値条件 (stat:STR >= 20)</summary>
    StatCompare,
    /// <summary>種族条件 (race:Elf)</summary>
    RaceCheck,
    /// <summary>宗教条件 (religion:LightTemple)</summary>
    ReligionCheck,
    /// <summary>熟練度条件 (mastery:sword >= 10)</summary>
    MasteryCheck,
    /// <summary>カルマ段階条件</summary>
    KarmaRankCheck
}

/// <summary>
/// 季節
/// </summary>
public enum Season
{
    /// <summary>春 (3-5月)</summary>
    Spring,
    /// <summary>夏 (6-8月)</summary>
    Summer,
    /// <summary>秋 (9-11月)</summary>
    Autumn,
    /// <summary>冬 (12-2月)</summary>
    Winter
}

/// <summary>
/// 天候
/// </summary>
public enum Weather
{
    /// <summary>晴れ</summary>
    Clear,
    /// <summary>雨</summary>
    Rain,
    /// <summary>霧</summary>
    Fog,
    /// <summary>雪</summary>
    Snow,
    /// <summary>嵐</summary>
    Storm
}

/// <summary>
/// 戦闘スタンス
/// </summary>
public enum CombatStance
{
    /// <summary>バランス型（補正なし）</summary>
    Balanced,
    /// <summary>攻撃型（攻撃↑/防御↓）</summary>
    Aggressive,
    /// <summary>防御型（防御↑/攻撃↓）</summary>
    Defensive
}

/// <summary>
/// 採取ポイントの種類
/// </summary>
public enum GatheringType
{
    /// <summary>薬草採取</summary>
    Herb,
    /// <summary>鉱石採掘</summary>
    Mining,
    /// <summary>木材伐採</summary>
    Logging,
    /// <summary>釣り</summary>
    Fishing,
    /// <summary>採集（キノコ/果実等）</summary>
    Foraging
}

/// <summary>
/// 病気の種類
/// </summary>
public enum DiseaseType
{
    /// <summary>風邪（軽度、自然回復あり）</summary>
    Cold,
    /// <summary>感染症（傷口から、治療必要）</summary>
    Infection,
    /// <summary>食中毒（汚染食料、一時的）</summary>
    FoodPoisoning,
    /// <summary>瘴気病（特定ダンジョン、重度）</summary>
    Miasma,
    /// <summary>呪い病（呪い攻撃、魔法治療必要）</summary>
    CursePlague
}

/// <summary>
/// 睡眠の質
/// </summary>
public enum SleepQuality
{
    /// <summary>熟睡（宿屋/安全な野営）</summary>
    DeepSleep,
    /// <summary>普通（テント野営）</summary>
    Normal,
    /// <summary>浅い（屋外直寝）</summary>
    Light,
    /// <summary>仮眠（ダンジョン内）</summary>
    Nap
}

/// <summary>
/// 図鑑カテゴリ
/// </summary>
public enum EncyclopediaCategory
{
    /// <summary>モンスター図鑑</summary>
    Monster,
    /// <summary>アイテム図鑑</summary>
    Item,
    /// <summary>NPC図鑑</summary>
    Npc,
    /// <summary>地域図鑑</summary>
    Region
}

/// <summary>
/// 仲間の種別
/// </summary>
public enum CompanionType
{
    /// <summary>傭兵（金で雇用）</summary>
    Mercenary,
    /// <summary>仲間NPC（好感度で加入）</summary>
    Ally,
    /// <summary>ペット（捕獲・テイム）</summary>
    Pet
}

/// <summary>
/// 仲間のAI制御モード
/// </summary>
public enum CompanionAIMode
{
    /// <summary>攻撃優先</summary>
    Aggressive,
    /// <summary>防御優先</summary>
    Defensive,
    /// <summary>支援優先</summary>
    Support,
    /// <summary>待機（行動しない）</summary>
    Wait
}

/// <summary>
/// 関係値の種別
/// </summary>
public enum RelationshipType
{
    /// <summary>種族間関係</summary>
    Racial,
    /// <summary>領地間関係</summary>
    Territorial,
    /// <summary>宗教間関係</summary>
    Religious,
    /// <summary>個人間関係</summary>
    Personal
}

/// <summary>
/// 傷の種類
/// </summary>
public enum BodyWoundType
{
    /// <summary>切り傷</summary>
    Cut,
    /// <summary>打撲</summary>
    Bruise,
    /// <summary>刺し傷</summary>
    Puncture,
    /// <summary>骨折</summary>
    Fracture,
    /// <summary>火傷</summary>
    Burn
}

/// <summary>
/// 疲労度段階
/// </summary>
public enum FatigueLevel
{
    /// <summary>元気</summary>
    Fresh,
    /// <summary>軽疲労</summary>
    Mild,
    /// <summary>疲労</summary>
    Tired,
    /// <summary>重疲労</summary>
    Exhausted,
    /// <summary>過労（行動不能）</summary>
    Collapse
}

/// <summary>
/// 清潔度段階
/// </summary>
public enum HygieneLevel
{
    /// <summary>清潔</summary>
    Clean,
    /// <summary>普通</summary>
    Normal,
    /// <summary>汚れ</summary>
    Dirty,
    /// <summary>不衛生</summary>
    Filthy,
    /// <summary>不潔（病気リスク高）</summary>
    Foul
}

/// <summary>
/// 渇きの段階（数値ベース）
/// </summary>
public enum ThirstStage
{
    /// <summary>潤い（80-100）</summary>
    Hydrated,
    /// <summary>軽い渇き（50-79）</summary>
    Thirsty,
    /// <summary>脱水（25-49）</summary>
    Dehydrated,
    /// <summary>重度脱水（1-24）</summary>
    SevereDehydration,
    /// <summary>致命的脱水（0）</summary>
    CriticalDehydration
}

/// <summary>
/// 疲労の段階（数値ベース）
/// </summary>
public enum FatigueStage
{
    /// <summary>元気（80-100）</summary>
    Fresh,
    /// <summary>軽疲労（50-79）</summary>
    Mild,
    /// <summary>疲労（25-49）</summary>
    Tired,
    /// <summary>重疲労（1-24）</summary>
    Exhausted,
    /// <summary>過労（0）</summary>
    Collapse
}

/// <summary>
/// 衛生の段階（数値ベース）
/// </summary>
public enum HygieneStage
{
    /// <summary>清潔（80-100）</summary>
    Clean,
    /// <summary>普通（50-79）</summary>
    Normal,
    /// <summary>汚れ（25-49）</summary>
    Dirty,
    /// <summary>不衛生（1-24）</summary>
    Filthy,
    /// <summary>不潔（0）</summary>
    Foul
}

/// <summary>
/// 施設カテゴリ（拠点作成）
/// </summary>
public enum FacilityCategory
{
    /// <summary>キャンプ（基本野営地）</summary>
    Camp,
    /// <summary>作業台（クラフト用）</summary>
    Workbench,
    /// <summary>鍛冶場（武器防具作成）</summary>
    Smithy,
    /// <summary>倉庫（アイテム保管）</summary>
    Storage,
    /// <summary>畑（食料生産）</summary>
    Farm,
    /// <summary>防壁（防衛用）</summary>
    Barricade,
    /// <summary>宿舎（仲間収容）</summary>
    Barracks
}

/// <summary>
/// テンプレートマップの種類
/// </summary>
public enum TemplateMapType
{
    /// <summary>ボスフロア</summary>
    BossFloor,
    /// <summary>街マップ</summary>
    Town,
    /// <summary>遺跡マップ</summary>
    Ruins,
    /// <summary>塔マップ</summary>
    Tower,
    /// <summary>特殊ダンジョン</summary>
    SpecialDungeon
}

/// <summary>
/// 罠の種類（プレイヤー設置用）
/// </summary>
public enum PlayerTrapType
{
    /// <summary>棘罠（物理ダメージ）</summary>
    SpikeTrap,
    /// <summary>落とし穴（移動阻害+ダメージ）</summary>
    PitfallTrap,
    /// <summary>爆発罠（範囲ダメージ）</summary>
    ExplosiveTrap,
    /// <summary>睡眠罠（状態異常付与）</summary>
    SleepTrap,
    /// <summary>警報罠（敵を誘引）</summary>
    AlarmTrap
}

/// <summary>
/// 料理方法
/// </summary>
public enum CookingMethod
{
    /// <summary>焼く</summary>
    Grill,
    /// <summary>煮る</summary>
    Boil,
    /// <summary>蒸す</summary>
    Steam,
    /// <summary>干す</summary>
    Dry,
    /// <summary>発酵</summary>
    Ferment
}

/// <summary>
/// グリッドアイテムサイズ
/// </summary>
public enum GridItemSize
{
    /// <summary>1×1（小型アイテム）</summary>
    Size1x1,
    /// <summary>1×2（縦長）</summary>
    Size1x2,
    /// <summary>2×1（横長）</summary>
    Size2x1,
    /// <summary>2×2（中型）</summary>
    Size2x2,
    /// <summary>2×3（大型装備）</summary>
    Size2x3
}

/// <summary>
/// パズルの種類
/// </summary>
public enum PuzzleType
{
    /// <summary>ルーン語パズル</summary>
    RuneLanguage,
    /// <summary>属性パズル</summary>
    Elemental,
    /// <summary>物理パズル（レバー/圧力板等）</summary>
    Physical
}

/// <summary>
/// 誓約の種類
/// </summary>
public enum OathType
{
    /// <summary>禁酒の誓約</summary>
    Temperance,
    /// <summary>不殺の誓約</summary>
    Pacifism,
    /// <summary>孤高の誓約（仲間禁止）</summary>
    Solitude,
    /// <summary>粗食の誓約（上質食料禁止）</summary>
    Austerity,
    /// <summary>暗闘の誓約（松明禁止）</summary>
    Darkness
}

/// <summary>
/// クラス段階（転職）
/// </summary>
public enum ClassTier
{
    /// <summary>基本職</summary>
    Base,
    /// <summary>上位職</summary>
    Advanced,
    /// <summary>最上位職</summary>
    Master
}

/// <summary>
/// 噂の種別（NPC記憶）
/// </summary>
public enum RumorType
{
    /// <summary>英雄的行為の噂</summary>
    Heroic,
    /// <summary>悪行の噂</summary>
    Villainous,
    /// <summary>奇行の噂</summary>
    Eccentric,
    /// <summary>無名（噂なし）</summary>
    Unknown
}

/// <summary>
/// 闇市場商品カテゴリ
/// </summary>
public enum BlackMarketCategory
{
    /// <summary>盗品</summary>
    StolenGoods,
    /// <summary>禁忌アイテム</summary>
    ForbiddenItems,
    /// <summary>暗殺道具</summary>
    AssassinTools,
    /// <summary>情報</summary>
    Information
}

/// <summary>
/// 渇きの段階
/// </summary>
public enum ThirstLevel
{
    /// <summary>潤い（十分）</summary>
    Hydrated,
    /// <summary>軽い渇き</summary>
    Thirsty,
    /// <summary>脱水</summary>
    Dehydrated,
    /// <summary>重度脱水（危険）</summary>
    SevereDehydration
}

/// <summary>
/// 水源の品質
/// </summary>
public enum WaterQuality
{
    /// <summary>清水（安全）</summary>
    Pure,
    /// <summary>川水（軽い感染リスク）</summary>
    River,
    /// <summary>泥水（感染リスク中）</summary>
    Muddy,
    /// <summary>汚水（高感染リスク）</summary>
    Polluted
}

/// <summary>
/// 投資種別
/// </summary>
public enum InvestmentType
{
    /// <summary>ショップ投資</summary>
    Shop,
    /// <summary>冒険者パーティ出資</summary>
    AdventurerParty,
    /// <summary>事業出資</summary>
    Business
}

/// <summary>
/// 禁制品カテゴリ
/// </summary>
public enum ContrabandType
{
    /// <summary>違法武器</summary>
    IllegalWeapons,
    /// <summary>魔物素材</summary>
    MonsterMaterials,
    /// <summary>禁書</summary>
    ForbiddenBooks,
    /// <summary>毒物</summary>
    Poisons
}

/// <summary>
/// HUD要素種別
/// </summary>
public enum HudElement
{
    /// <summary>HPバー</summary>
    HpBar,
    /// <summary>MPバー</summary>
    MpBar,
    /// <summary>ミニマップ</summary>
    MiniMap,
    /// <summary>メッセージログ</summary>
    MessageLog,
    /// <summary>ステータス情報</summary>
    StatusInfo
}

/// <summary>
/// 無限ダンジョン難易度帯
/// </summary>
public enum InfiniteDungeonTier
{
    /// <summary>通常（1-10F）</summary>
    Normal,
    /// <summary>上級（11-30F）</summary>
    Advanced,
    /// <summary>深層（31-50F）</summary>
    Deep,
    /// <summary>魔界（51F-）</summary>
    Abyss
}

/// <summary>
/// NG+段階
/// </summary>
public enum NewGamePlusTier
{
    /// <summary>NG+1</summary>
    Plus1,
    /// <summary>NG+2</summary>
    Plus2,
    /// <summary>NG+3</summary>
    Plus3,
    /// <summary>NG+4</summary>
    Plus4,
    /// <summary>NG+5（最大）</summary>
    Plus5
}

/// <summary>
/// ギャンブルゲーム種別
/// </summary>
public enum GamblingGameType
{
    /// <summary>サイコロ（出目予想）</summary>
    Dice,
    /// <summary>丁半（偶数奇数）</summary>
    ChoHan,
    /// <summary>カード（ハイ&amp;ロー）</summary>
    Card
}

// ═══════════════════════════════════════════════════
// Ver.prt.0.5 追加
// ═══════════════════════════════════════════════════

/// <summary>
/// 拡張アイテムカテゴリ（P.13）
/// </summary>
public enum ExtendedItemCategory
{
    /// <summary>素材</summary>
    Material,
    /// <summary>魂石</summary>
    SoulGem,
    /// <summary>罠キット</summary>
    TrapKit,
    /// <summary>修理道具</summary>
    RepairTool,
    /// <summary>料理</summary>
    CookedFood,
    /// <summary>書物</summary>
    Book,
    /// <summary>鍵</summary>
    Key,
    /// <summary>楽器</summary>
    Instrument
}

/// <summary>
/// マルチエンディング種別（P.75）
/// </summary>
public enum EndingType
{
    /// <summary>正規エンディング（30階ボス撃破）</summary>
    Normal,
    /// <summary>真エンディング（全ボス撃破+高ランク）</summary>
    True,
    /// <summary>闇エンディング（カルマ極悪時）</summary>
    Dark,
    /// <summary>救済エンディング（死に戻り0回+高カルマ）</summary>
    Salvation,
    /// <summary>放浪エンディング（クリアせず全領地踏破）</summary>
    Wanderer
}

/// <summary>
/// 環境音種別（P.82）
/// </summary>
public enum AmbientSoundType
{
    /// <summary>ダンジョン（洞窟の滴る水音）</summary>
    Dungeon,
    /// <summary>森（鳥の鳴き声、風）</summary>
    Forest,
    /// <summary>山岳（強い風）</summary>
    Mountain,
    /// <summary>沿岸（波の音）</summary>
    Coast,
    /// <summary>砂漠（乾いた風）</summary>
    Desert,
    /// <summary>辺境（不気味な雰囲気）</summary>
    Frontier,
    /// <summary>街（人々の声）</summary>
    Town,
    /// <summary>ボス戦（緊迫した空気）</summary>
    BossBattle,
    /// <summary>静寂</summary>
    Silence
}

/// <summary>
/// アイテム鑑定状態
/// </summary>
public enum IdentificationState
{
    /// <summary>未鑑定</summary>
    Unknown,
    /// <summary>鑑定済み</summary>
    Identified,
    /// <summary>呪い判明</summary>
    Cursed
}

/// <summary>
/// 呪い種別
/// </summary>
public enum CurseType
{
    /// <summary>呪いなし</summary>
    None,
    /// <summary>軽度の呪い（ステータス微減）</summary>
    Minor,
    /// <summary>重度の呪い（外せない＋ステータス減）</summary>
    Major,
    /// <summary>致死呪い（徐々にHP減少）</summary>
    Deadly
}

/// <summary>
/// 生態系イベント種別
/// </summary>
public enum EcosystemEventType
{
    /// <summary>捕食（強い敵が弱い敵を倒す）</summary>
    Predation,
    /// <summary>縄張り争い（同格の敵同士の戦闘）</summary>
    TerritoryFight,
    /// <summary>共生（異種間の協力関係）</summary>
    Symbiosis,
    /// <summary>漁り（戦闘残骸からの採取）</summary>
    Scavenging
}

/// <summary>
/// ペット種別
/// </summary>
public enum PetType
{
    /// <summary>狼（戦闘補助型）</summary>
    Wolf,
    /// <summary>馬（騎乗移動型）</summary>
    Horse,
    /// <summary>鷹（偵察型）</summary>
    Hawk,
    /// <summary>猫（幸運型）</summary>
    Cat,
    /// <summary>熊（タンク型）</summary>
    Bear,
    /// <summary>竜（万能型・希少）</summary>
    Dragon
}

/// <summary>
/// 交易路状態
/// </summary>
public enum TradeRouteStatus
{
    /// <summary>閉鎖中</summary>
    Closed,
    /// <summary>開通済み</summary>
    Open,
    /// <summary>封鎖中（戦争・災害等）</summary>
    Blocked,
    /// <summary>繁栄中（利益増大）</summary>
    Prosperous
}

/// <summary>
/// 実績カテゴリ
/// </summary>
public enum AchievementCategory
{
    /// <summary>戦闘系</summary>
    Combat,
    /// <summary>探索系</summary>
    Exploration,
    /// <summary>収集系</summary>
    Collection,
    /// <summary>ストーリー系</summary>
    Story,
    /// <summary>チャレンジ系</summary>
    Challenge,
    /// <summary>メタ系（周回・実績数等）</summary>
    Meta
}

/// <summary>
/// 碑文種別
/// </summary>
public enum InscriptionType
{
    /// <summary>伝承（世界観・歴史）</summary>
    Lore,
    /// <summary>警告（危険情報）</summary>
    Warning,
    /// <summary>ヒント（攻略情報）</summary>
    Hint,
    /// <summary>レシピ（調合・鍛冶）</summary>
    Recipe,
    /// <summary>呪文（新魔法習得）</summary>
    Spell,
    /// <summary>地図（隠し部屋情報）</summary>
    Map
}

/// <summary>
/// ヘルプカテゴリ
/// </summary>
public enum HelpCategory
{
    /// <summary>移動</summary>
    Movement,
    /// <summary>戦闘</summary>
    Combat,
    /// <summary>インベントリ</summary>
    Inventory,
    /// <summary>魔法</summary>
    Magic,
    /// <summary>クラフト</summary>
    Crafting,
    /// <summary>サバイバル</summary>
    Survival,
    /// <summary>上級</summary>
    Advanced
}

/// <summary>
/// 色覚モード
/// </summary>
public enum ColorBlindMode
{
    /// <summary>通常</summary>
    None,
    /// <summary>1型色覚（赤色覚異常）</summary>
    Protanopia,
    /// <summary>2型色覚（緑色覚異常）</summary>
    Deuteranopia,
    /// <summary>3型色覚（青色覚異常）</summary>
    Tritanopia,
    /// <summary>モノクロ</summary>
    Monochrome
}

/// <summary>
/// MODコンテンツ種別
/// </summary>
public enum ModContentType
{
    /// <summary>敵データ</summary>
    Enemy,
    /// <summary>アイテムデータ</summary>
    Item,
    /// <summary>マップデータ</summary>
    Map,
    /// <summary>呪文データ</summary>
    Spell,
    /// <summary>クエストデータ</summary>
    Quest,
    /// <summary>システム拡張</summary>
    System
}

/// <summary>
/// 戦争フェーズ
/// </summary>
public enum WarPhase
{
    /// <summary>緊張（開戦前）</summary>
    Tension,
    /// <summary>小競り合い</summary>
    Skirmish,
    /// <summary>本格戦闘</summary>
    Battle,
    /// <summary>戦後処理</summary>
    Aftermath,
    /// <summary>和平</summary>
    Peace
}

/// <summary>
/// 陣営所属
/// </summary>
public enum FactionAlignment
{
    /// <summary>第1陣営（攻撃側）</summary>
    Faction1,
    /// <summary>第2陣営（防衛側）</summary>
    Faction2,
    /// <summary>中立</summary>
    Neutral,
    /// <summary>傭兵（状況で変動）</summary>
    Mercenary
}
