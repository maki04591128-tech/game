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
/// 装備スロット
/// </summary>
public enum EquipmentSlot
{
    MainHand,
    OffHand,
    Head,
    Body,
    Hands,
    Feet,
    Accessory1,
    Accessory2
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
