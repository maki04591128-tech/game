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
    public const int AttackNormal = 1;      // 3→1
    public const int AttackUnarmed = 1;     // 2→1（通常攻撃に統合）
    public const int AttackTwoHanded = 1;   // 5→1（通常攻撃に統合）
    public const int AttackBow = 5;         // 4→5（遠距離攻撃統一）
    public const int AttackThrow = 5;       // 2→5（遠距離攻撃統一）

    // アイテム
    public const int UsePotion = 1;         // 2→1（クイックユーズ）
    public const int UseScroll = 3;
    public const int Eat = 10;
    [System.Obsolete("部位別コスト（EquipWeapon/EquipAccessory/EquipArms/EquipHead/EquipBody）を使用してください")]
    public const int EquipChange = 5;
    public const int SetTrap = 15;
    public const int Craft = 30;

    // 装備部位別コスト
    public const int EquipWeapon = 1;       // 武器着脱
    public const int EquipAccessory = 1;    // アクセサリー着脱
    public const int EquipArms = 10;        // 腕装備着脱
    public const int EquipHead = 10;        // 頭装備着脱
    public const int EquipBody = 20;        // 胴装備着脱

    // その他
    public const int Wait = 1;
    public const int Rest = 100;
    public const int Search = 5;
    public const int OpenDoor = 1;
    public const int Unlock = 10;
    public const int OpenChest = 5;
    public const int Talk = 0;  // 戦闘外のみ
    public const int InventorySort = 20;    // インベントリソート
    public const int UseStairs = 10;        // 階層移動（階段使用）

    // シンボルマップ
    public const int SymbolMapMove = 300;   // シンボルマップ上の移動
    public const int SymbolMapEntry = 0;    // Tキーによるシンボルマップ進入

    // 魔法（最小・最大）
    public const int SpellMinimum = 5;
    public const int SpellMaximum = 100;

    // 宗教
    public const int Pray = 10;
    public const int ReligionAction = 5;

    // 状態別ターンコスト倍率
    public const int EngagedNonCombatMultiplier = 2;  // 接敵状態・非戦闘行動倍率
    public const int StealthMovementMultiplier = 5;    // 隠密状態・移動ドア開閉倍率
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
    public const int HungerDecayInterval = 864;             // 満腹度通常消費間隔（14.4分）
    public const int HungerDecayIntervalStarving = 59220;   // 満腹度0以下消費間隔
    public const int ThirstDecayInterval = 432;             // 渇き度通常消費間隔（7.2分）
    public const int ThirstDecayIntervalStarving = 29610;   // 渇き度0以下消費間隔
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
    public const int RebirthSanityCost = 20;
    public const int MaxRescueCount = 3;

    // 満腹度
    public const int InitialHunger = 70;
    public const int MaxHunger = 150;
    public const int MinHunger = -10;

    // 渇き
    public const int InitialThirst = 70;
    public const int MaxThirst = 150;
    public const int MinThirst = -10;

    // 疲労（旧: 100→新: 0が初期値。新仕様では蓄積型で0が最良状態）
    [System.Obsolete("FatigueConstants.InitialFatigue を使用してください")]
    public const int InitialFatigue = 0;
    [System.Obsolete("FatigueConstants.MaxFatigue を使用してください")]
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
    // K-5: ボスHP倍率を計算式ベースに（フロア深度連動）
    public static double GetFloorBossHpMultiplier(int floor) =>
        Math.Min(6.0, 2.5 + (floor * 0.1));

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

/// <summary>
/// 疲労度システムに関する定数
/// 疲労度は0から蓄積し、100が最大値。高い値ほど悪い状態を示す。
/// </summary>
public static class FatigueConstants
{
    // === 基本値 ===
    /// <summary>疲労度の初期値（0.0 = 快調状態）</summary>
    public const double InitialFatigue = 0.0;
    /// <summary>疲労度の最大値</summary>
    public const double MaxFatigue = 100.0;
    /// <summary>疲労度の最小値</summary>
    public const double MinFatigue = 0.0;

    // === 蓄積率 ===
    /// <summary>移動時の疲労度蓄積率（行動コスト × この値）</summary>
    public const double MovementFatigueRate = 0.01;
    /// <summary>スキル使用時の疲労度蓄積率（行動コスト × この値）</summary>
    public const double SkillFatigueRate = 0.1;

    // === 段階閾値 ===
    /// <summary>快調状態の上限（0以上10未満）</summary>
    public const double RefreshedMax = 10.0;
    /// <summary>通常状態の上限（10以上50未満）</summary>
    public const double NormalMax = 50.0;
    /// <summary>倦怠状態の上限（50以上60未満）</summary>
    public const double LethargyMax = 60.0;
    /// <summary>疲労・軽状態の上限（60以上70未満）</summary>
    public const double LightFatigueMax = 70.0;
    /// <summary>疲労状態の上限（70以上80未満）</summary>
    public const double FatigueMax = 80.0;
    /// <summary>疲労・重状態の上限（80以上90未満）</summary>
    public const double HeavyFatigueMax = 90.0;
    /// <summary>疲弊状態の上限（90以上100未満）</summary>
    public const double ExhaustionMax = 100.0;

    // === SP上限修正率 ===
    /// <summary>快調状態のSP上限ボーナス（+1%）</summary>
    public const double RefreshedSpBonus = 0.01;
    /// <summary>倦怠状態のSP上限ペナルティ（−1%）</summary>
    public const double LethargySpPenalty = -0.01;
    /// <summary>疲労・軽状態のSP上限ペナルティ（−5%）</summary>
    public const double LightFatigueSpPenalty = -0.05;
    /// <summary>疲労状態のSP上限ペナルティ（−25%）</summary>
    public const double FatigueSpPenalty = -0.25;
    /// <summary>疲労・重状態のSP上限ペナルティ（−50%）</summary>
    public const double HeavyFatigueSpPenalty = -0.50;
    /// <summary>疲弊状態のSP上限ペナルティ（−80%）</summary>
    public const double ExhaustionSpPenalty = -0.80;
    /// <summary>疲労困憊状態のSP上限ペナルティ（−100%）</summary>
    public const double TotalExhaustionSpPenalty = -1.00;

    // === 行動コスト加算 ===
    /// <summary>疲労・軽状態の行動コスト加算</summary>
    public const int LightFatigueCostBonus = 1;
    /// <summary>疲労状態の行動コスト加算</summary>
    public const int FatigueCostBonus = 2;
    /// <summary>疲労・重状態の行動コスト加算</summary>
    public const int HeavyFatigueCostBonus = 3;
    /// <summary>疲弊状態の行動コスト加算</summary>
    public const int ExhaustionCostBonus = 4;
    /// <summary>疲労困憊状態の行動コスト加算</summary>
    public const int TotalExhaustionCostBonus = 5;

    // === 待機時回復量 ===
    /// <summary>待機1回あたりの基本回復量</summary>
    public const double BaseRestRecovery = 1.0;
    /// <summary>倦怠状態の回復量倍率（基本回復量 × 1/10）</summary>
    public const double LethargyRecoveryMultiplier = 0.1;
    /// <summary>疲労・軽状態の回復量倍率（基本回復量 × 1/100）</summary>
    public const double LightFatigueRecoveryMultiplier = 0.01;
    /// <summary>疲労以上の重度疲労の回復量倍率（基本回復量 × 1/1000）</summary>
    public const double SevereRecoveryMultiplier = 0.001;

    // === 宿屋回復開始値 ===
    /// <summary>疲労状態（70+）の宿屋回復後開始値</summary>
    public const double InnRecoveryFatigueStart = 10.0;
    /// <summary>疲労・重状態（80+）の宿屋回復後開始値</summary>
    public const double InnRecoveryHeavyFatigueStart = 20.0;
    /// <summary>疲弊状態（90+）の宿屋回復後開始値</summary>
    public const double InnRecoveryExhaustionStart = 30.0;
    /// <summary>疲労困憊状態（100）の宿屋回復後開始値</summary>
    public const double InnRecoveryTotalExhaustionStart = 40.0;

    // === 気付け薬（疲労行動制限解除） ===
    /// <summary>気付け薬の効果持続ターン数（3,600ターン＝ゲーム内1時間）</summary>
    public const int RestrictionReliefDuration = 3600;
}
