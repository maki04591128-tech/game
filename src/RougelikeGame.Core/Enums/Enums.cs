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
    Wanderer
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
    Talk
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
