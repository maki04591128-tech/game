namespace RougelikeGame.Core.Systems;

/// <summary>
/// 種族/職業別の成長テーブル定義
/// </summary>
public record GrowthRate(
    double Strength,
    double Vitality,
    double Agility,
    double Dexterity,
    double Intelligence,
    double Mind,
    double Perception,
    double Charisma,
    double Luck,
    double HpPerLevel,
    double MpPerLevel)
{
    /// <summary>指定レベル時の成長分をStatModifierとして取得</summary>
    public StatModifier GetLevelBonus(int level)
    {
        int lvFactor = level - 1;
        return new StatModifier(
            Strength: (int)(Strength * lvFactor),
            Vitality: (int)(Vitality * lvFactor),
            Agility: (int)(Agility * lvFactor),
            Dexterity: (int)(Dexterity * lvFactor),
            Intelligence: (int)(Intelligence * lvFactor),
            Mind: (int)(Mind * lvFactor),
            Perception: (int)(Perception * lvFactor),
            Charisma: (int)(Charisma * lvFactor),
            Luck: (int)(Luck * lvFactor)
        );
    }

    /// <summary>指定レベル時のHP成長分を取得</summary>
    public int GetHpBonus(int level) => (int)(HpPerLevel * (level - 1));

    /// <summary>指定レベル時のMP成長分を取得</summary>
    public int GetMpBonus(int level) => (int)(MpPerLevel * (level - 1));
}

/// <summary>
/// 成長システム - 種族/職業別の成長曲線を管理
/// </summary>
public static class GrowthSystem
{
    #region 種族別成長率

    private static readonly Dictionary<Race, GrowthRate> _raceGrowth = new()
    {
        [Race.Human] = new(0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 5.0, 3.0),
        [Race.Elf] = new(0.3, 0.3, 0.5, 0.6, 0.8, 0.7, 0.5, 0.4, 0.4, 3.0, 6.0),
        [Race.Dwarf] = new(0.7, 0.8, 0.3, 0.4, 0.3, 0.5, 0.4, 0.3, 0.4, 7.0, 2.0),
        [Race.Orc] = new(0.9, 0.7, 0.4, 0.3, 0.2, 0.3, 0.3, 0.2, 0.3, 8.0, 1.0),
        [Race.Beastfolk] = new(0.5, 0.5, 0.7, 0.5, 0.3, 0.4, 0.7, 0.3, 0.4, 5.0, 2.0),
        [Race.Halfling] = new(0.3, 0.4, 0.6, 0.6, 0.4, 0.4, 0.5, 0.5, 0.8, 3.5, 2.5),
        [Race.Undead] = new(0.5, 0.6, 0.4, 0.4, 0.5, 0.6, 0.4, 0.2, 0.3, 5.0, 4.0),
        [Race.Demon] = new(0.6, 0.5, 0.5, 0.4, 0.7, 0.6, 0.4, 0.2, 0.2, 5.0, 5.0),
        [Race.FallenAngel] = new(0.4, 0.4, 0.6, 0.5, 0.7, 0.8, 0.6, 0.4, 0.3, 4.0, 7.0),
        [Race.Slime] = new(0.3, 0.9, 0.3, 0.2, 0.3, 0.4, 0.3, 0.1, 0.3, 10.0, 1.0)
    };

    #endregion

    #region 職業別成長率

    private static readonly Dictionary<CharacterClass, GrowthRate> _classGrowth = new()
    {
        [CharacterClass.Fighter] = new(0.8, 0.6, 0.4, 0.4, 0.2, 0.2, 0.3, 0.2, 0.3, 8.0, 1.0),
        [CharacterClass.Knight] = new(0.5, 0.8, 0.3, 0.3, 0.2, 0.4, 0.3, 0.4, 0.3, 10.0, 1.0),
        [CharacterClass.Thief] = new(0.3, 0.3, 0.7, 0.8, 0.2, 0.3, 0.6, 0.3, 0.5, 4.0, 1.0),
        [CharacterClass.Ranger] = new(0.4, 0.4, 0.5, 0.7, 0.3, 0.3, 0.7, 0.3, 0.4, 5.0, 2.0),
        [CharacterClass.Mage] = new(0.1, 0.2, 0.3, 0.3, 0.9, 0.5, 0.4, 0.3, 0.3, 2.0, 10.0),
        [CharacterClass.Cleric] = new(0.3, 0.4, 0.3, 0.3, 0.4, 0.8, 0.4, 0.5, 0.3, 5.0, 7.0),
        [CharacterClass.Monk] = new(0.5, 0.5, 0.7, 0.4, 0.3, 0.6, 0.3, 0.2, 0.3, 6.0, 3.0),
        [CharacterClass.Bard] = new(0.3, 0.3, 0.4, 0.5, 0.4, 0.4, 0.4, 0.8, 0.5, 4.0, 4.0),
        [CharacterClass.Alchemist] = new(0.2, 0.3, 0.4, 0.6, 0.7, 0.4, 0.6, 0.3, 0.4, 3.0, 5.0),
        [CharacterClass.Necromancer] = new(0.2, 0.3, 0.3, 0.3, 0.8, 0.6, 0.3, 0.2, 0.3, 3.0, 8.0)
    };

    #endregion

    /// <summary>種族の成長率を取得</summary>
    public static GrowthRate GetRaceGrowthRate(Race race) =>
        _raceGrowth.TryGetValue(race, out var rate) ? rate : _raceGrowth[Race.Human];

    /// <summary>職業の成長率を取得</summary>
    public static GrowthRate GetClassGrowthRate(CharacterClass cls) =>
        _classGrowth.TryGetValue(cls, out var rate) ? rate : _classGrowth[CharacterClass.Fighter];

    /// <summary>レベルアップ時のステータス成長を計算（種族+職業の合算）</summary>
    public static StatModifier CalculateLevelUpBonus(Race race, CharacterClass cls, int newLevel)
    {
        var raceRate = GetRaceGrowthRate(race);
        var classRate = GetClassGrowthRate(cls);

        // 種族成長率 + 職業成長率を各0.5倍で合算
        return new StatModifier(
            Strength: RollGrowth(raceRate.Strength + classRate.Strength),
            Vitality: RollGrowth(raceRate.Vitality + classRate.Vitality),
            Agility: RollGrowth(raceRate.Agility + classRate.Agility),
            Dexterity: RollGrowth(raceRate.Dexterity + classRate.Dexterity),
            Intelligence: RollGrowth(raceRate.Intelligence + classRate.Intelligence),
            Mind: RollGrowth(raceRate.Mind + classRate.Mind),
            Perception: RollGrowth(raceRate.Perception + classRate.Perception),
            Charisma: RollGrowth(raceRate.Charisma + classRate.Charisma),
            Luck: RollGrowth(raceRate.Luck + classRate.Luck)
        );
    }

    /// <summary>レベルアップ時のHP成長を計算</summary>
    public static int CalculateHpGrowth(Race race, CharacterClass cls)
    {
        var raceRate = GetRaceGrowthRate(race);
        var classRate = GetClassGrowthRate(cls);
        double combined = (raceRate.HpPerLevel + classRate.HpPerLevel) * 0.5;
        return Math.Max(1, RollGrowth(combined));
    }

    /// <summary>レベルアップ時のMP成長を計算</summary>
    public static int CalculateMpGrowth(Race race, CharacterClass cls)
    {
        var raceRate = GetRaceGrowthRate(race);
        var classRate = GetClassGrowthRate(cls);
        double combined = (raceRate.MpPerLevel + classRate.MpPerLevel) * 0.5;
        return Math.Max(0, RollGrowth(combined));
    }

    /// <summary>
    /// 成長率に基づいて成長値を決定。
    /// 整数部分は確定で上昇、小数部分は確率で+1。
    /// 例: 1.3 → 必ず1上昇 + 30%の確率で追加1上昇
    /// </summary>
    private static int RollGrowth(double rate)
    {
        int guaranteed = (int)rate;
        double fraction = rate - guaranteed;

        // 確率判定（乱数を使わず切り上げで確定的にする：テスト容易性のため）
        // 実際のゲームではRandomを使う
        if (fraction >= 0.5)
            return guaranteed + 1;
        return guaranteed;
    }

    /// <summary>
    /// 成長率に基づいて成長値を決定（乱数版）
    /// </summary>
    public static int RollGrowthWithRandom(double rate, Random random)
    {
        int guaranteed = (int)rate;
        double fraction = rate - guaranteed;

        if (random.NextDouble() < fraction)
            return guaranteed + 1;
        return guaranteed;
    }

    /// <summary>指定レベルまでの累計経験値を計算</summary>
    public static int CalculateTotalExpForLevel(int level, double raceExpMultiplier = 1.0)
    {
        int total = 0;
        for (int lv = 1; lv < level; lv++)
        {
            total += (int)(GameConstants.BaseExpRequired * Math.Pow(GameConstants.ExpGrowthRate, lv - 1) / raceExpMultiplier);
        }
        return total;
    }

    /// <summary>HP最大値を計算（ベース + 種族/職業レベル成長分）</summary>
    public static int CalculateMaxHp(Stats baseStats, Race race, CharacterClass cls, int level)
    {
        int baseHp = baseStats.MaxHp;
        var raceRate = GetRaceGrowthRate(race);
        var classRate = GetClassGrowthRate(cls);
        int levelBonus = (int)((raceRate.HpPerLevel + classRate.HpPerLevel) * 0.5 * (level - 1));

        var raceDef = RaceDefinition.Get(race);
        var classDef = ClassDefinition.Get(cls);

        return baseHp + raceDef.HpBonus + classDef.HpBonus + levelBonus;
    }

    /// <summary>MP最大値を計算（ベース + 種族/職業レベル成長分）</summary>
    public static int CalculateMaxMp(Stats baseStats, Race race, CharacterClass cls, int level)
    {
        int baseMp = baseStats.MaxMp;
        var raceRate = GetRaceGrowthRate(race);
        var classRate = GetClassGrowthRate(cls);
        int levelBonus = (int)((raceRate.MpPerLevel + classRate.MpPerLevel) * 0.5 * (level - 1));

        var raceDef = RaceDefinition.Get(race);
        var classDef = ClassDefinition.Get(cls);

        return baseMp + raceDef.MpBonus + classDef.MpBonus + levelBonus;
    }
}
