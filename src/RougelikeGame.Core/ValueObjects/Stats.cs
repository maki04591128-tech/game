namespace RougelikeGame.Core;

/// <summary>
/// キャラクターのステータスを表す値オブジェクト
/// </summary>
public readonly record struct Stats(
    int Strength,       // 筋力: 物理攻撃力、運搬能力
    int Vitality,       // 体力: HP、物理防御
    int Agility,        // 敏捷: 行動速度、回避
    int Dexterity,      // 器用: 命中、クリティカル
    int Intelligence,   // 知力: 魔法攻撃力、MP
    int Mind,           // 精神: 魔法防御、状態異常耐性
    int Perception,     // 感覚: 探知、罠発見
    int Charisma,       // 魅力: NPC交渉、価格
    int Luck            // 幸運: ドロップ率、クリティカル
)
{
    // 計算プロパティ
    public int MaxHp => Math.Max(1, 50 + (Vitality * 10) + (Strength * 2));
    public int MaxMp => Math.Max(0, 20 + (Mind * 5) + (Intelligence * 3));
    public int MaxSp => Math.Max(0, 100 + (Vitality * 2));

    public int PhysicalAttack => Strength * 3 + Dexterity;
    public int PhysicalDefense => Vitality * 2 + Strength;
    public int MagicalAttack => Intelligence * 3 + Mind;
    public int MagicalDefense => Mind * 2 + Intelligence;

    public double HitRate => Math.Max(0.05, 0.7 + (Dexterity * 0.02));
    public double EvasionRate => Math.Max(0.0, Math.Min(GameConstants.MaxEvasionRate, Agility * 0.02));
    public double CriticalRate => Math.Max(0.0, Math.Min(1.0, GameConstants.BaseCriticalRate + (Luck * 0.01) + (Dexterity * 0.005)));

    public int ActionSpeed => Agility * 2 + Dexterity;

    /// <summary>
    /// ステータス修正を適用
    /// </summary>
    public Stats Apply(StatModifier modifier) => new(
        Strength + modifier.Strength,
        Vitality + modifier.Vitality,
        Agility + modifier.Agility,
        Dexterity + modifier.Dexterity,
        Intelligence + modifier.Intelligence,
        Mind + modifier.Mind,
        Perception + modifier.Perception,
        Charisma + modifier.Charisma,
        Luck + modifier.Luck
    );

    /// <summary>
    /// 複数の修正を適用
    /// </summary>
    public Stats ApplyAll(IEnumerable<StatModifier> modifiers)
    {
        var result = this;
        foreach (var modifier in modifiers)
        {
            result = result.Apply(modifier);
        }
        return result;
    }

    /// <summary>
    /// デフォルトステータス（全て10）
    /// </summary>
    public static Stats Default => new(10, 10, 10, 10, 10, 10, 10, 10, 10);

    /// <summary>
    /// ゼロステータス
    /// </summary>
    public static Stats Zero => new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    public override string ToString() =>
        $"STR:{Strength} VIT:{Vitality} AGI:{Agility} DEX:{Dexterity} " +
        $"INT:{Intelligence} MND:{Mind} PER:{Perception} CHA:{Charisma} LUK:{Luck}";
}

/// <summary>
/// ステータス修正値
/// </summary>
public readonly record struct StatModifier(
    int Strength = 0,
    int Vitality = 0,
    int Agility = 0,
    int Dexterity = 0,
    int Intelligence = 0,
    int Mind = 0,
    int Perception = 0,
    int Charisma = 0,
    int Luck = 0
)
{
    public static StatModifier operator +(StatModifier a, StatModifier b) => new(
        a.Strength + b.Strength,
        a.Vitality + b.Vitality,
        a.Agility + b.Agility,
        a.Dexterity + b.Dexterity,
        a.Intelligence + b.Intelligence,
        a.Mind + b.Mind,
        a.Perception + b.Perception,
        a.Charisma + b.Charisma,
        a.Luck + b.Luck
    );

    public static StatModifier operator -(StatModifier a, StatModifier b) => new(
        a.Strength - b.Strength,
        a.Vitality - b.Vitality,
        a.Agility - b.Agility,
        a.Dexterity - b.Dexterity,
        a.Intelligence - b.Intelligence,
        a.Mind - b.Mind,
        a.Perception - b.Perception,
        a.Charisma - b.Charisma,
        a.Luck - b.Luck
    );

    public static StatModifier operator *(StatModifier a, int multiplier) => new(
        a.Strength * multiplier,
        a.Vitality * multiplier,
        a.Agility * multiplier,
        a.Dexterity * multiplier,
        a.Intelligence * multiplier,
        a.Mind * multiplier,
        a.Perception * multiplier,
        a.Charisma * multiplier,
        a.Luck * multiplier
    );

    public static StatModifier Zero => new();

    /// <summary>
    /// 全ステータスに同じ値を適用
    /// </summary>
    public static StatModifier All(int value) => new(
        value, value, value, value, value, value, value, value, value
    );
}
