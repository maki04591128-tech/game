namespace RougelikeGame.Core;

/// <summary>
/// ターン消費に関する定数
/// </summary>
public static class TurnCosts
{
    // 移動
    public const int MoveNormal = 1;
    public const int MoveCombat = 10;
    public const int MoveStealth = 10;
    public const int MovePursuit = 5;
    public const int MoveAlert = 3;
    public const int MoveDash = 1;  // SP消費あり、通常時のみ
    public const int MoveCrawl = 3;
    public const int MoveSwim = 2;
    public const int MoveClimb = 5;

    // 斜め移動係数（1.4倍、整数計算用）
    public const int DiagonalNumerator = 14;
    public const int DiagonalDenominator = 10;

    // 攻撃
    public const int AttackNormal = 3;
    public const int AttackUnarmed = 2;
    public const int AttackTwoHanded = 5;
    public const int AttackBow = 4;
    public const int AttackThrow = 2;

    // アイテム
    public const int UsePotion = 2;
    public const int UseScroll = 3;
    public const int Eat = 10;
    public const int EquipChange = 5;
    public const int SetTrap = 15;
    public const int Craft = 30;

    // その他
    public const int Wait = 1;
    public const int Rest = 100;
    public const int Search = 5;
    public const int OpenDoor = 1;
    public const int Unlock = 10;
    public const int OpenChest = 5;
    public const int Talk = 0;  // 戦闘外のみ

    // 魔法（最小・最大）
    public const int SpellMinimum = 5;
    public const int SpellMaximum = 100;
}

/// <summary>
/// ゲーム時間に関する定数
/// </summary>
public static class TimeConstants
{
    public const int TurnsPerSecond = 1;
    public const int TurnsPerMinute = 60;
    public const int TurnsPerHour = 3600;
    public const int TurnsPerDay = 86400;

    // イベント周期
    public const int HungerDecayInterval = 600;         // 10分
    public const int StatusEffectTickInterval = 10;     // 10秒
    public const int NpcScheduleUpdateInterval = 60;    // 1分
    public const int DayNightCycleHalf = 43200;         // 12時間
    public const int DungeonResetInterval = 604800;     // 7日

    // ターン制限
    public const long TurnLimitYear = 31_536_000;               // 1年（365日 × 86400ターン/日）
    public const long TurnLimitExtension = 15_768_000;          // 延長分: 半年
    public const long TurnLimitWithExtension = TurnLimitYear + TurnLimitExtension;  // 1.5年

    // ターン制限の警告閾値
    public const long TurnLimitWarning90Days = TurnLimitYear - 86400L * 90;   // 残り90日で警告
    public const long TurnLimitWarning30Days = TurnLimitYear - 86400L * 30;   // 残り30日で警告
    public const long TurnLimitWarning7Days = TurnLimitYear - 86400L * 7;     // 残り7日で警告
}

/// <summary>
/// ゲームバランスに関する定数
/// </summary>
public static class GameConstants
{
    // 正気度
    public const int InitialSanity = 100;
    public const int MaxSanity = 100;
    public const int SanityRecoveryOnRescue = 20;
    public const int MaxRescueCount = 3;

    // 満腹度
    public const int InitialHunger = 100;
    public const int MaxHunger = 100;

    // 戦闘
    public const double BaseCriticalRate = 0.05;
    public const double MaxEvasionRate = 0.75;
    public const int MinimumDamage = 1;

    // レベル
    public const int MaxLevel = 50;
    public const int BaseExpRequired = 100;
    public const double ExpGrowthRate = 1.5;

    // マップ
    public const int DefaultMapWidth = 80;
    public const int DefaultMapHeight = 50;
    public const int DefaultViewRadius = 8;

    // インベントリ
    public const int DefaultInventorySize = 30;

    // 魔法言語
    public const int MaxSpellWords = 7;
    public const int InitialWordMastery = 20;
    public const int MaxWordMastery = 100;
}

/// <summary>
/// 死因別正気度減少量
/// </summary>
public static class SanityLoss
{
    public const int Combat = 5;
    public const int Boss = 8;
    public const int Starvation = 10;
    public const int Trap = 10;
    public const int TimeLimit = 15;
    public const int Curse = 15;
    public const int Suicide = 20;
    public const int SanityDeath = 25;
    public const int Fall = 8;
    public const int Poison = 7;
    public const int Unknown = 10;
}
