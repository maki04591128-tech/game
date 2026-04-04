using RougelikeGame.Core.AI;
using RougelikeGame.Core.Items;

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

    // 宗教
    public const int Pray = 10;
    public const int ReligionAction = 5;
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

    // 渇き
    public const int InitialThirst = 100;
    public const int MaxThirst = 100;

    // 疲労
    public const int InitialFatigue = 100;
    public const int MaxFatigue = 100;

    // 衛生
    public const int InitialHygiene = 100;
    public const int MaxHygiene = 100;

    // 戦闘
    public const double BaseCriticalRate = 0.05;
    public const double MaxEvasionRate = 0.75;
    public const int MinimumDamage = 1;

    // レベル
    public const int MaxLevel = 50;
    public const int BaseExpRequired = 100;
    public const double ExpGrowthRate = 1.15;

    // マップ
    public const int DefaultMapWidth = 80;
    public const int DefaultMapHeight = 50;
    public const int DefaultViewRadius = 8;

    // ダンジョン階層
    public const int MaxDungeonFloor = 30;
    public const int BossFloorInterval = 5;

    // インベントリ
    public const int DefaultInventorySize = 30;

    // 重量
    public const float BaseMaxWeight = 50f;
    public const float WeightPerStrength = 5f;
    public const float OverweightSpeedPenalty = 0.5f;

    // 魔法言語
    public const int MaxSpellWords = 7;
    public const int InitialWordMastery = 20;
    public const int MaxWordMastery = 100;

    // 宗教
    public const int MaxFaithPoints = 100;
    public const double FaithRetentionRate = 0.80;          // 死に戻り時の信仰度維持率
    public const int InitialFaithOnJoin = 20;               // 入信時初期信仰度
    public const int InitialFaithOnConvert = 10;            // 改宗時初期信仰度
    public const int PrayFaithGain = 2;                     // 祈り1回あたりの信仰度上昇
    public const int ConversionSanityPenalty = 10;          // 改宗時正気度ペナルティ
    public const int LeaveSanityPenalty = 5;                // 脱退時正気度ペナルティ
    public const int ApostasyCurseDurationDays = 30;        // 背教者の呪い持続日数
    public const int FaithDecayInterval = 10;               // 信仰度自然減少の間隔（日）
    public const int FaithDecayAmount = 1;                  // 信仰度自然減少量
    public const int MaxFaithCapReductionOnRejoin = 20;     // 再入信時の信仰度上限減少
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

/// <summary>
/// バランス調整パラメータ
/// </summary>
public static class BalanceConfig
{
    #region 敵ステータス倍率

    /// <summary>階層別ステータス倍率（階層 → 倍率）</summary>
    public static double GetDepthStatMultiplier(int depth) => depth switch
    {
        <= 5 => 1.0,
        <= 10 => 1.3,
        <= 15 => 1.7,
        <= 20 => 2.2,
        <= 25 => 2.8,
        <= 30 => 3.5,
        _ => 3.5 + (depth - 30) * 0.2
    };

    /// <summary>ランク別ステータス倍率</summary>
    public static double GetRankStatMultiplier(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 1.0,
        EnemyRank.Elite => 1.5,
        EnemyRank.Rare => 2.0,
        EnemyRank.Boss => 2.5,
        EnemyRank.HiddenBoss => 4.0,
        _ => 1.0
    };

    /// <summary>階層別推奨レベル</summary>
    // J-2: ExpGrowthRate=1.15に合わせた現実的な推奨レベル
    public static int GetRecommendedLevel(int depth) => depth switch
    {
        <= 5 => 1 + (depth - 1),        // F1=1, F5=5
        <= 10 => 5 + (depth - 5),       // F6=6, F10=10
        <= 20 => 10 + (depth - 10),     // F11=11, F20=20
        <= 30 => 20 + (depth - 20),     // F21=21, F30=30
        _ => 30 + (depth - 30)          // F31+=31+
    };

    /// <summary>フロアボスのHP倍率（通常敵比）</summary>
    public static double GetFloorBossHpMultiplier(int floor) => floor switch
    {
        5 => 3.0,    // 初ボスは控えめ
        10 => 3.5,
        15 => 4.0,
        20 => 4.5,
        25 => 5.0,
        30 => 6.0,   // 最終ボスは特に硬い
        _ => 3.0
    };

    /// <summary>フロアボスの攻撃力倍率（通常敵比）</summary>
    public static double GetFloorBossAttackMultiplier(int floor) => floor switch
    {
        5 => 2.0,
        10 => 2.5,
        15 => 3.0,
        20 => 3.5,
        25 => 4.0,
        30 => 5.0,
        _ => 2.0
    };

    /// <summary>ボス撃破時の経験値ボーナス倍率</summary>
    public static double GetBossExpBonus(int floor) => floor switch
    {
        5 => 3.0,
        10 => 4.0,
        15 => 5.0,
        20 => 6.0,
        25 => 8.0,
        30 => 10.0,
        _ => 2.0
    };

    #endregion

    #region ドロップ率

    /// <summary>レアリティ別基本ドロップ率</summary>
    public static double GetBaseDropRate(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => 0.50,
        ItemRarity.Uncommon => 0.25,
        ItemRarity.Rare => 0.10,
        ItemRarity.Epic => 0.04,
        ItemRarity.Legendary => 0.01,
        _ => 0.50
    };

    /// <summary>ランク別ドロップ率ボーナス倍率</summary>
    public static double GetRankDropBonus(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 1.0,
        EnemyRank.Elite => 1.5,
        EnemyRank.Rare => 2.0,
        EnemyRank.Boss => 2.5,
        EnemyRank.HiddenBoss => 3.0,
        _ => 1.0
    };

    #endregion

    #region ゴールド報酬

    /// <summary>階層別ゴールドドロップ範囲（最小値）</summary>
    public static int GetGoldDropMin(int depth) => 5 + depth * 3;

    /// <summary>階層別ゴールドドロップ範囲（最大値）</summary>
    public static int GetGoldDropMax(int depth) => 15 + depth * 8;

    /// <summary>ランク別ゴールド倍率</summary>
    public static double GetRankGoldMultiplier(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 1.0,
        EnemyRank.Elite => 2.0,
        EnemyRank.Rare => 3.0,
        EnemyRank.Boss => 5.0,
        EnemyRank.HiddenBoss => 10.0,
        _ => 1.0
    };

    #endregion

    #region 経験値バランス

    /// <summary>ランク別経験値倍率</summary>
    public static double GetRankExpMultiplier(EnemyRank rank) => rank switch
    {
        EnemyRank.Common => 1.0,
        EnemyRank.Elite => 2.0,
        EnemyRank.Rare => 3.0,
        EnemyRank.Boss => 5.0,
        EnemyRank.HiddenBoss => 10.0,
        _ => 1.0
    };

    /// <summary>レベル差による経験値補正（自レベル - 敵レベル）</summary>
    public static double GetLevelDiffExpModifier(int levelDiff) => levelDiff switch
    {
        <= -10 => 2.0,
        <= -5 => 1.5,
        <= 0 => 1.0,
        <= 5 => 0.7,
        <= 10 => 0.4,
        _ => 0.1
    };

    #endregion

    #region ショップバランス

    /// <summary>ショップ買値倍率（基本価格 × この倍率）</summary>
    public const double ShopBuyMultiplier = 1.2;  // L-1: 往復80%損失を56%損失に緩和

    /// <summary>ショップ売値倍率（基本価格 × この倍率）</summary>
    public const double ShopSellMultiplier = 0.5;  // L-1: 往復損失を40%に緩和（買1.2×売0.5=0.6）

    /// <summary>識別済みアイテムの売値ボーナス倍率</summary>
    public const double IdentifiedSellBonus = 1.2;

    #endregion
}
