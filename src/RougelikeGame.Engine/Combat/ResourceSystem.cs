namespace RougelikeGame.Engine.Combat;

using RougelikeGame.Core;

/// <summary>
/// リソース管理システム（HP/MP/SP/満腹度）
/// 設計書「4. HP・MP・リソース管理」に基づく実装
/// </summary>
public class ResourceSystem
{
    #region HP計算

    /// <summary>
    /// 最大HPを計算
    /// 最大HP = 基礎値(50) + (VIT × 10) + (レベル × 5) + 種族補正 + クラス補正
    /// </summary>
    public int CalculateMaxHp(HpCalculationParams param)
    {
        int baseHp = 50;
        int vitBonus = param.Vitality * 10;
        int levelBonus = param.Level * 5;
        int classBonus = GetClassHpBonus(param.CharacterClass, param.Level);

        return baseHp + vitBonus + levelBonus + param.RaceBonus + classBonus;
    }

    /// <summary>
    /// クラスごとのHP成長を取得
    /// </summary>
    public int GetClassHpBonus(CharacterClass characterClass, int level)
    {
        int hpPerLevel = characterClass switch
        {
            CharacterClass.Fighter => 15,
            CharacterClass.Knight => 12,
            CharacterClass.Monk => 10,
            CharacterClass.Cleric => 8,
            CharacterClass.Mage => 5,
            CharacterClass.Necromancer => 5,
            CharacterClass.Thief => 8,
            CharacterClass.Ranger => 10,
            CharacterClass.Bard => 7,
            CharacterClass.Alchemist => 6,
            _ => 8
        };

        return hpPerLevel * (level - 1);
    }

    /// <summary>
    /// HP状態を取得
    /// </summary>
    public HpState GetHpState(int currentHp, int maxHp)
    {
        if (currentHp <= 0) return HpState.Dead;
        if (maxHp <= 0) return HpState.Dead;

        double ratio = (double)currentHp / maxHp;

        return ratio switch
        {
            >= 0.76 => HpState.Healthy,
            >= 0.51 => HpState.Injured,
            >= 0.26 => HpState.Wounded,
            _ => HpState.Critical
        };
    }

    #endregion

    #region MP計算

    /// <summary>
    /// 最大MPを計算
    /// 最大MP = 基礎値(20) + (MND × 5) + (INT × 2) + (レベル × 2) + 種族補正
    /// </summary>
    public int CalculateMaxMp(MpCalculationParams param)
    {
        int baseMp = 20;
        int mndBonus = param.Mind * 5;
        int intBonus = param.Intelligence * 2;
        int levelBonus = param.Level * 2;
        int classBonus = GetClassMpBonus(param.CharacterClass, param.Level);

        return baseMp + mndBonus + intBonus + levelBonus + param.RaceBonus + classBonus;
    }

    /// <summary>
    /// クラスごとのMP成長を取得
    /// </summary>
    public int GetClassMpBonus(CharacterClass characterClass, int level)
    {
        int mpPerLevel = characterClass switch
        {
            CharacterClass.Fighter => 2,
            CharacterClass.Knight => 3,
            CharacterClass.Monk => 4,
            CharacterClass.Cleric => 6,
            CharacterClass.Mage => 8,
            CharacterClass.Necromancer => 8,
            CharacterClass.Thief => 3,
            CharacterClass.Ranger => 4,
            CharacterClass.Bard => 5,
            CharacterClass.Alchemist => 7,
            _ => 4
        };

        return mpPerLevel * (level - 1);
    }

    /// <summary>
    /// MP自然回復量を計算（10ターンごと）
    /// </summary>
    public int CalculateMpNaturalRecovery(int maxMp)
    {
        return Math.Max(1, (int)(maxMp * 0.01));
    }

    /// <summary>
    /// 休息によるMP回復量を計算（100ターン消費）
    /// </summary>
    public int CalculateMpRestRecovery(int maxMp)
    {
        return Math.Max(1, (int)(maxMp * 0.10));
    }

    /// <summary>
    /// 瞑想によるMP回復量を計算
    /// </summary>
    public int CalculateMpMeditationRecovery(int maxMp)
    {
        return Math.Max(1, (int)(maxMp * 0.20));
    }

    #endregion

    #region スタミナ(SP)計算

    /// <summary>
    /// 最大SP（固定値100）
    /// </summary>
    public const int MaxStamina = 100;

    /// <summary>
    /// 行動に必要なSP消費を取得
    /// </summary>
    public int GetStaminaCost(StaminaAction action, bool isHeavyArmor = false)
    {
        int baseCost = action switch
        {
            StaminaAction.NormalMove => 0,
            StaminaAction.Dash => 2,  // マスごと
            StaminaAction.NormalAttack => 5,
            StaminaAction.PhysicalSkillLight => 10,
            StaminaAction.PhysicalSkillMedium => 20,
            StaminaAction.PhysicalSkillHeavy => 30,
            StaminaAction.Defend => 3,
            StaminaAction.Dodge => 8,
            _ => 0
        };

        // 重装備着用時は全消費+20%
        if (isHeavyArmor && baseCost > 0)
        {
            baseCost = (int)(baseCost * 1.2);
        }

        return baseCost;
    }

    /// <summary>
    /// 待機によるSP回復量
    /// </summary>
    public const int StaminaWaitRecovery = 10;

    /// <summary>
    /// 自然回復量（10ターンごと）
    /// </summary>
    public const int StaminaNaturalRecovery = 5;

    /// <summary>
    /// SP状態を取得
    /// </summary>
    public StaminaState GetStaminaState(int currentSp)
    {
        return currentSp switch
        {
            >= 80 => StaminaState.Full,
            >= 50 => StaminaState.Normal,
            >= 20 => StaminaState.Low,
            > 0 => StaminaState.Critical,
            _ => StaminaState.Exhausted
        };
    }

    #endregion

    #region 満腹度計算

    /// <summary>
    /// 最大満腹度
    /// </summary>
    public const int MaxHunger = 150;

    /// <summary>
    /// 満腹度状態を取得
    /// </summary>
    public HungerState GetHungerState(int currentHunger)
    {
        return currentHunger switch
        {
            >= 120 => HungerState.Nausea,
            >= 100 => HungerState.Overeating,
            >= 80 => HungerState.Satiated,
            >= 50 => HungerState.Normal,
            >= 40 => HungerState.SlightlyHungry,
            >= 0 => HungerState.VeryHungry,
            >= -8 => HungerState.Starving,
            >= -9 => HungerState.NearStarvation,
            _ => HungerState.Starvation
        };
    }

    /// <summary>
    /// 満腹度による効果を取得
    /// </summary>
    public HungerEffect GetHungerEffect(int currentHunger)
    {
        var state = GetHungerState(currentHunger);

        return state switch
        {
            HungerState.Nausea => new HungerEffect(0.7f, true, true, 0, ActionCostBonus: 3, ActionBlockChance: 0.3f),
            HungerState.Overeating => new HungerEffect(0.9f, true, true, 0, ActionCostBonus: 2),
            HungerState.Satiated => new HungerEffect(1.0f, true, true, 0, ActionCostBonus: 1),
            HungerState.Normal => new HungerEffect(1.0f, true, true, 0),
            HungerState.SlightlyHungry => new HungerEffect(0.95f, true, true, 0),
            HungerState.VeryHungry => new HungerEffect(0.9f, true, true, 0, ActionCostBonus: 1),
            HungerState.Starving => new HungerEffect(0.7f, true, true, 1, ActionCostBonus: 2),
            HungerState.NearStarvation => new HungerEffect(0f, false, false, 10),
            HungerState.Starvation => new HungerEffect(0f, false, false, 999),
            _ => new HungerEffect(1.0f, true, true, 0)
        };
    }

    /// <summary>
    /// 満腹度消費を計算
    /// </summary>
    public int CalculateHungerConsumption(HungerConsumptionParams param)
    {
        int baseConsumption = 0;

        // 自然減少（100ターンごとに1）
        if (param.TurnCount % 100 == 0)
        {
            baseConsumption += 1;
        }

        // 激しい戦闘
        if (param.WasInCombat)
        {
            baseConsumption += param.CombatIntensity switch
            {
                CombatIntensity.Light => 1,
                CombatIntensity.Normal => 2,
                CombatIntensity.Heavy => 3,
                _ => 0
            };
        }

        // 休息
        if (param.WasResting)
        {
            baseConsumption += 2;
        }

        // 種族による差異（±20~50%）
        float raceModifier = param.RaceHungerModifier;
        baseConsumption = (int)(baseConsumption * raceModifier);

        return Math.Max(0, baseConsumption);
    }

    #endregion

    #region 経験値計算

    /// <summary>
    /// 必要経験値を計算
    /// </summary>
    public int CalculateRequiredExp(int level)
    {
        if (level >= 99) return int.MaxValue;  // レベル上限

        // 基礎経験値 × 1.5の累乗
        double baseExp = 100;
        double multiplier = 1.5;

        double required = baseExp * Math.Pow(multiplier, level - 1);
        // AI-2: 高レベルでint.MaxValueを超えないようにクランプ
        return (int)Math.Min(required, int.MaxValue - 1);
    }

    /// <summary>
    /// 累計必要経験値を計算
    /// </summary>
    public int CalculateTotalRequiredExp(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++)
        {
            total += CalculateRequiredExp(i);
        }
        return total;
    }

    /// <summary>
    /// 獲得経験値を計算
    /// 獲得経験値 = 敵基礎経験値 × (敵レベル / 自レベル) × 難易度補正 × 素性補正
    /// </summary>
    public int CalculateExpGain(ExpGainParams param)
    {
        int playerLevel = Math.Max(1, param.PlayerLevel);
        double levelRatio = (double)param.EnemyLevel / playerLevel;
        levelRatio = Math.Clamp(levelRatio, 0.1, 3.0);  // 最低10%、最大300%

        double exp = param.BaseExp * levelRatio * param.DifficultyModifier * param.BackgroundModifier;
        return Math.Max(1, (int)exp);
    }

    #endregion
}

#region パラメータ構造体

/// <summary>
/// HP計算パラメータ
/// </summary>
public record struct HpCalculationParams
{
    public int Vitality { get; init; }
    public int Level { get; init; }
    public int RaceBonus { get; init; }
    public CharacterClass CharacterClass { get; init; }
}

/// <summary>
/// MP計算パラメータ
/// </summary>
public record struct MpCalculationParams
{
    public int Mind { get; init; }
    public int Intelligence { get; init; }
    public int Level { get; init; }
    public int RaceBonus { get; init; }
    public CharacterClass CharacterClass { get; init; }
}

/// <summary>
/// 満腹度消費パラメータ
/// </summary>
public record struct HungerConsumptionParams
{
    public int TurnCount { get; init; }
    public bool WasInCombat { get; init; }
    public CombatIntensity CombatIntensity { get; init; }
    public bool WasResting { get; init; }
    public float RaceHungerModifier { get; init; }

    public HungerConsumptionParams()
    {
        RaceHungerModifier = 1.0f;
    }
}

/// <summary>
/// 経験値獲得パラメータ
/// </summary>
public record struct ExpGainParams
{
    public int BaseExp { get; init; }
    public int EnemyLevel { get; init; }
    public int PlayerLevel { get; init; }
    public float DifficultyModifier { get; init; }
    public float BackgroundModifier { get; init; }

    public ExpGainParams()
    {
        DifficultyModifier = 1.0f;
        BackgroundModifier = 1.0f;
    }
}

#endregion

#region 結果構造体

/// <summary>
/// 満腹度効果
/// </summary>
public record struct HungerEffect(
    float StaminaRecoveryModifier,
    bool AllowHpRecovery,
    bool AllowSpRecovery,
    int DamagePerTurn,
    int ActionCostBonus = 0,
    float ActionBlockChance = 0f
);

#endregion

#region 列挙型

// B.40: CharacterClass enumはRougelikeGame.Core.CharacterClassに統一（重複定義を削除）

/// <summary>
/// スタミナ消費行動
/// </summary>
public enum StaminaAction
{
    NormalMove,
    Dash,
    NormalAttack,
    PhysicalSkillLight,
    PhysicalSkillMedium,
    PhysicalSkillHeavy,
    Defend,
    Dodge
}

/// <summary>
/// スタミナ状態
/// </summary>
public enum StaminaState
{
    /// <summary>十分 (80-100)</summary>
    Full,
    /// <summary>普通 (50-79)</summary>
    Normal,
    /// <summary>少ない (20-49)</summary>
    Low,
    /// <summary>危険 (1-19)</summary>
    Critical,
    /// <summary>消耗 (0)</summary>
    Exhausted
}

/// <summary>
/// 満腹度状態（Core層のHungerStageに対応）
/// </summary>
public enum HungerState
{
    /// <summary>吐き気 (120+) - 行動コスト+3、30%行動不可</summary>
    Nausea,
    /// <summary>過食 (100-119) - 行動コスト+2</summary>
    Overeating,
    /// <summary>満腹 (80-99) - 行動コスト+1</summary>
    Satiated,
    /// <summary>通常 (50-79) - 通常状態</summary>
    Normal,
    /// <summary>空腹・小 (40-49) - 30%確率で行動コスト+1</summary>
    SlightlyHungry,
    /// <summary>空腹・大 (0-39) - 行動コスト+1</summary>
    VeryHungry,
    /// <summary>飢餓 (-1〜-8) - 行動コスト+2、毎ターン1ダメージ</summary>
    Starving,
    /// <summary>餓死寸前 (-9) - 移動攻撃不能、毎ターン10ダメージ</summary>
    NearStarvation,
    /// <summary>餓死 (-10) - 即死</summary>
    Starvation
}

/// <summary>
/// 戦闘の激しさ
/// </summary>
public enum CombatIntensity
{
    None,
    Light,
    Normal,
    Heavy
}

#endregion
