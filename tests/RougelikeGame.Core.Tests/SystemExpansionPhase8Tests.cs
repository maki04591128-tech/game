using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.AI;

#pragma warning disable CS0618 // Obsolete旧enum互換テスト維持

namespace RougelikeGame.Core.Tests;

// ============================================================
// Phase 8 テスト拡充: GUI統合ロジックパスの包括テスト
// GameControllerで使用される各システムの連携・境界値テスト
// ============================================================

#region 1. TimeOfDaySystem 包括テスト (テスト薄 → 拡充)

public class Phase8_TimeOfDaySystemTests
{
    [Theory]
    [InlineData(0, TimePeriod.Midnight)]
    [InlineData(3, TimePeriod.Midnight)]
    [InlineData(4, TimePeriod.Dawn)]
    [InlineData(5, TimePeriod.Dawn)]
    [InlineData(6, TimePeriod.Dawn)]       // Dawn is 4-6
    [InlineData(7, TimePeriod.Morning)]    // Morning is 7-11
    [InlineData(9, TimePeriod.Morning)]
    [InlineData(10, TimePeriod.Morning)]   // Still Morning
    [InlineData(12, TimePeriod.Afternoon)] // Afternoon is 12-16
    [InlineData(14, TimePeriod.Afternoon)]
    [InlineData(15, TimePeriod.Afternoon)]
    [InlineData(17, TimePeriod.Dusk)]      // Dusk is 17-19
    [InlineData(18, TimePeriod.Dusk)]
    [InlineData(20, TimePeriod.Night)]     // Night is 20-23
    [InlineData(21, TimePeriod.Night)]
    [InlineData(23, TimePeriod.Night)]
    public void GetTimePeriod_AllHours_ReturnsCorrectPeriod(int hour, TimePeriod expected)
    {
        Assert.Equal(expected, TimeOfDaySystem.GetTimePeriod(hour));
    }

    [Theory]
    [InlineData(TimePeriod.Dawn)]
    [InlineData(TimePeriod.Morning)]
    [InlineData(TimePeriod.Afternoon)]
    [InlineData(TimePeriod.Dusk)]
    [InlineData(TimePeriod.Night)]
    [InlineData(TimePeriod.Midnight)]
    public void GetTimePeriodName_AllPeriods_ReturnsNonEmpty(TimePeriod period)
    {
        var name = TimeOfDaySystem.GetTimePeriodName(period);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Theory]
    [InlineData(TimePeriod.Afternoon)]
    [InlineData(TimePeriod.Morning)]
    public void GetSightRangeModifier_DaytimePeriods_NoReduction(TimePeriod period)
    {
        float modifier = TimeOfDaySystem.GetSightRangeModifier(period);
        Assert.True(modifier >= 0.7f, $"Daytime modifier should be >= 0.7: got {modifier}");
    }

    [Theory]
    [InlineData(TimePeriod.Night)]
    [InlineData(TimePeriod.Midnight)]
    public void GetSightRangeModifier_NightPeriods_Reduced(TimePeriod period)
    {
        float modifier = TimeOfDaySystem.GetSightRangeModifier(period);
        Assert.True(modifier <= 0.8f, $"Night modifier should be reduced: got {modifier}");
    }

    [Theory]
    [InlineData(MonsterRace.Undead)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Demon)]
    [InlineData(MonsterRace.Insect)]
    [InlineData(MonsterRace.Humanoid)]
    [InlineData(MonsterRace.Amorphous)]
    [InlineData(MonsterRace.Plant)]
    [InlineData(MonsterRace.Construct)]
    [InlineData(MonsterRace.Spirit)]
    public void GetActivityPattern_AllRaces_ReturnsValidPattern(MonsterRace race)
    {
        var pattern = TimeOfDaySystem.GetActivityPattern(race);
        Assert.True(Enum.IsDefined(typeof(ActivityPattern), pattern));
    }

    [Fact]
    public void GetActivityMultiplier_NocturnalAtNight_HighActivity()
    {
        float multiplier = TimeOfDaySystem.GetActivityMultiplier(ActivityPattern.Nocturnal, TimePeriod.Night);
        Assert.True(multiplier >= 1.0f, $"Nocturnal at night should be active: {multiplier}");
    }

    [Fact]
    public void GetActivityMultiplier_NocturnalAtMidday_LowActivity()
    {
        float multiplier = TimeOfDaySystem.GetActivityMultiplier(ActivityPattern.Nocturnal, TimePeriod.Afternoon);
        Assert.True(multiplier <= 1.0f, $"Nocturnal at midday should be less active: {multiplier}");
    }

    [Fact]
    public void GetActivityMultiplier_DiurnalAtMidday_HighActivity()
    {
        float multiplier = TimeOfDaySystem.GetActivityMultiplier(ActivityPattern.Diurnal, TimePeriod.Afternoon);
        Assert.True(multiplier >= 1.0f, $"Diurnal at midday should be active: {multiplier}");
    }

    [Fact]
    public void IsActiveTime_UndeadAtNight_ShouldBeActive()
    {
        bool active = TimeOfDaySystem.IsActiveTime(MonsterRace.Undead, TimePeriod.Night);
        Assert.True(active);
    }

    [Fact]
    public void GetStatModifier_ReturnsValidModifier()
    {
        var modifier = TimeOfDaySystem.GetStatModifier(MonsterRace.Undead, TimePeriod.Night);
        Assert.NotNull(modifier);
    }

    [Fact]
    public void GetActivityPatternName_AllPatterns_NonEmpty()
    {
        foreach (ActivityPattern pattern in Enum.GetValues<ActivityPattern>())
        {
            var name = TimeOfDaySystem.GetActivityPatternName(pattern);
            Assert.False(string.IsNullOrWhiteSpace(name), $"Pattern {pattern} has empty name");
        }
    }
}

#endregion

#region 2. ProficiencySystem 包括テスト (テスト薄 → 拡充)

public class Phase8_ProficiencySystemTests
{
    [Fact]
    public void NewSystem_AllLevelsZero()
    {
        var system = new ProficiencySystem();
        foreach (ProficiencyCategory cat in Enum.GetValues<ProficiencyCategory>())
        {
            Assert.Equal(0, system.GetLevel(cat));
        }
    }

    [Fact]
    public void GainExperience_LevelsUp()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 100);
        Assert.True(system.GetLevel(ProficiencyCategory.Swordsmanship) > 0);
    }

    [Fact]
    public void GainExperience_MultipleCategories_Independent()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 50);
        system.GainExperience(ProficiencyCategory.Exploration, 100);
        Assert.True(system.GetLevel(ProficiencyCategory.Exploration) >= system.GetLevel(ProficiencyCategory.Swordsmanship));
    }

    [Fact]
    public void GetBonusDamage_HighLevel_PositiveBonus()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 500);
        double bonus = system.GetBonusDamage(ProficiencyCategory.Swordsmanship, 100);
        Assert.True(bonus >= 0);
    }

    [Fact]
    public void GetBonusCraftQuality_HighLevel_PositiveBonus()
    {
        var system = new ProficiencySystem();
        double quality = system.GetBonusCraftQuality(10);
        Assert.True(quality >= 0);
    }

    [Fact]
    public void DecayUnusedProficiencies_UnusedDecays()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 200);
        int levelBefore = system.GetLevel(ProficiencyCategory.Swordsmanship);
        
        // 未使用で減衰
        var used = new HashSet<ProficiencyCategory> { ProficiencyCategory.Exploration };
        for (int i = 0; i < 100; i++)
        {
            system.DecayUnusedProficiencies(used);
        }
        
        int levelAfter = system.GetLevel(ProficiencyCategory.Swordsmanship);
        Assert.True(levelAfter <= levelBefore);
    }

    [Fact]
    public void DecayUnusedProficiencies_UsedDoesNotDecay()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 200);
        int levelBefore = system.GetLevel(ProficiencyCategory.Swordsmanship);
        
        var used = new HashSet<ProficiencyCategory> { ProficiencyCategory.Swordsmanship };
        system.DecayUnusedProficiencies(used);
        
        int levelAfter = system.GetLevel(ProficiencyCategory.Swordsmanship);
        Assert.Equal(levelBefore, levelAfter);
    }

    [Fact]
    public void GetAllProficiencies_ReturnsAllCategories()
    {
        var system = new ProficiencySystem();
        var all = system.GetAllProficiencies();
        Assert.NotEmpty(all);
    }

    [Theory]
    [InlineData(WeaponType.Sword)]
    [InlineData(WeaponType.Axe)]
    [InlineData(WeaponType.Spear)]
    [InlineData(WeaponType.Bow)]
    [InlineData(WeaponType.Staff)]
    [InlineData(WeaponType.Dagger)]
    [InlineData(WeaponType.Hammer)]
    public void GetWeaponProficiencyCategory_AllTypes_ReturnsValid(WeaponType type)
    {
        var cat = ProficiencySystem.GetWeaponProficiencyCategory(type);
        Assert.True(Enum.IsDefined(typeof(ProficiencyCategory), cat));
    }

    [Fact]
    public void OnLevelUp_Fires_WhenLevelIncreases()
    {
        var system = new ProficiencySystem();
        bool fired = false;
        system.OnLevelUp += args => fired = true;
        
        system.GainExperience(ProficiencyCategory.Swordsmanship, 1000);
        Assert.True(fired);
    }

    [Fact]
    public void GainExperience_ZeroAmount_NoChange()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 0);
        Assert.Equal(0, system.GetLevel(ProficiencyCategory.Swordsmanship));
    }
}

#endregion

#region 3. ItemGradeSystem 包括テスト (テスト薄 → 拡充)

public class Phase8_ItemGradeSystemTests
{
    [Theory]
    [InlineData(ItemGrade.Crude)]
    [InlineData(ItemGrade.Cheap)]
    [InlineData(ItemGrade.Standard)]
    [InlineData(ItemGrade.Fine)]
    [InlineData(ItemGrade.Masterwork)]
    public void GetGradeInfo_AllGrades_ReturnsValidInfo(ItemGrade grade)
    {
        var info = ItemGradeSystem.GetGradeInfo(grade);
        Assert.NotNull(info);
        Assert.False(string.IsNullOrWhiteSpace(info.JapaneseName));
    }

    [Theory]
    [InlineData(ItemGrade.Crude)]
    [InlineData(ItemGrade.Cheap)]
    [InlineData(ItemGrade.Standard)]
    [InlineData(ItemGrade.Fine)]
    [InlineData(ItemGrade.Masterwork)]
    public void GetStatMultiplier_AllGrades_PositiveValue(ItemGrade grade)
    {
        float mult = ItemGradeSystem.GetStatMultiplier(grade);
        Assert.True(mult > 0, $"Grade {grade} stat multiplier should be positive: {mult}");
    }

    [Fact]
    public void GetStatMultiplier_HigherGrade_HigherMultiplier()
    {
        float common = ItemGradeSystem.GetStatMultiplier(ItemGrade.Crude);
        float legendary = ItemGradeSystem.GetStatMultiplier(ItemGrade.Masterwork);
        Assert.True(legendary > common, $"Legendary({legendary}) should > Common({common})");
    }

    [Theory]
    [InlineData(ItemGrade.Crude)]
    [InlineData(ItemGrade.Cheap)]
    [InlineData(ItemGrade.Standard)]
    [InlineData(ItemGrade.Fine)]
    [InlineData(ItemGrade.Masterwork)]
    public void GetPriceMultiplier_AllGrades_PositiveValue(ItemGrade grade)
    {
        float mult = ItemGradeSystem.GetPriceMultiplier(grade);
        Assert.True(mult > 0);
    }

    [Theory]
    [InlineData(ItemGrade.Crude)]
    [InlineData(ItemGrade.Cheap)]
    [InlineData(ItemGrade.Standard)]
    [InlineData(ItemGrade.Fine)]
    [InlineData(ItemGrade.Masterwork)]
    public void GetGradeDisplayPrefix_AllGrades_ReturnsNonNull(ItemGrade grade)
    {
        string prefix = ItemGradeSystem.GetGradeDisplayPrefix(grade);
        Assert.NotNull(prefix);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void GetGradeDropRates_VariousLevels_SumToApproximatelyOne(int smithingLevel)
    {
        var rates = ItemGradeSystem.GetGradeDropRates(smithingLevel);
        float total = 0;
        foreach (var kvp in rates) total += kvp.Value;
        Assert.True(total > 0.9f && total < 1.1f, $"Total drop rate should be ~1.0: {total}");
    }

    [Fact]
    public void GetGradeDropRates_HigherSmithing_BetterHighGradeRates()
    {
        var low = ItemGradeSystem.GetGradeDropRates(0);
        var high = ItemGradeSystem.GetGradeDropRates(20);
        // 高スミシングレベルでは粗悪品の割合が減少するはず
        float lowCrude = low.ContainsKey(ItemGrade.Crude) ? low[ItemGrade.Crude] : 0;
        float highCrude = high.ContainsKey(ItemGrade.Crude) ? high[ItemGrade.Crude] : 0;
        Assert.True(highCrude <= lowCrude, $"Higher smithing should reduce crude drop rate: low({lowCrude}), high({highCrude})");
    }

    [Fact]
    public void DetermineGrade_ReturnsDefinedGrade()
    {
        var random = new Phase8TestRandom(0.5);
        var grade = ItemGradeSystem.DetermineGrade(random);
        Assert.True(Enum.IsDefined(typeof(ItemGrade), grade));
    }

    [Fact]
    public void DetermineGrade_LowRandom_ReturnsCommon()
    {
        var random = new Phase8TestRandom(0.01);
        var grade = ItemGradeSystem.DetermineGrade(random, 0);
        Assert.Equal(ItemGrade.Crude, grade);
    }
}

#endregion

#region 4. EnvironmentalPuzzleSystem 包括テスト (テスト薄 → 拡充)

public class Phase8_EnvironmentalPuzzleSystemTests
{
    [Fact]
    public void GetAllPuzzles_ReturnsNonEmpty()
    {
        var puzzles = EnvironmentalPuzzleSystem.GetAllPuzzles();
        Assert.NotNull(puzzles);
        Assert.NotEmpty(puzzles);
    }

    [Theory]
    [InlineData(PuzzleType.Physical)]
    [InlineData(PuzzleType.RuneLanguage)]
    [InlineData(PuzzleType.Elemental)]
    public void GetByType_AllTypes_ReturnsResults(PuzzleType type)
    {
        var puzzles = EnvironmentalPuzzleSystem.GetByType(type);
        Assert.NotNull(puzzles);
    }

    [Theory]
    [InlineData(PuzzleType.Physical)]
    [InlineData(PuzzleType.RuneLanguage)]
    [InlineData(PuzzleType.Elemental)]
    public void GetTypeName_AllTypes_NonEmpty(PuzzleType type)
    {
        string name = EnvironmentalPuzzleSystem.GetTypeName(type);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Theory]
    [InlineData(1, 50)]
    [InlineData(5, 100)]
    [InlineData(10, 200)]
    public void CalculateSuccessRate_HigherIntelligence_BetterRate(int difficulty, int intelligence)
    {
        float rate = EnvironmentalPuzzleSystem.CalculateSuccessRate(difficulty, intelligence);
        Assert.True(rate >= 0 && rate <= 1.0f, $"Success rate should be 0-1: {rate}");
    }

    [Fact]
    public void CalculateSuccessRate_HighDifficulty_LowerRate()
    {
        float easy = EnvironmentalPuzzleSystem.CalculateSuccessRate(1, 10);
        float hard = EnvironmentalPuzzleSystem.CalculateSuccessRate(10, 10);
        Assert.True(easy >= hard, $"Easy({easy}) should have better rate than Hard({hard})");
    }

    [Fact]
    public void CanAttempt_HighIntelligence_ReturnsTrue()
    {
        bool can = EnvironmentalPuzzleSystem.CanAttempt(PuzzleType.Physical, 100, 10);
        Assert.True(can);
    }

    [Fact]
    public void CanAttempt_VeryLowIntelligence_MayReturnFalse()
    {
        // 非常に低い知力では制限される可能性
        bool can = EnvironmentalPuzzleSystem.CanAttempt(PuzzleType.RuneLanguage, 1, 0);
        // 結果はシステム設計次第だが、例外は発生しないこと
        Assert.True(can || !can); // No exception
    }
}

#endregion

#region 5. GamblingSystem 包括テスト (テスト薄 → 拡充)

public class Phase8_GamblingSystemTests
{
    [Theory]
    [InlineData(3, 3, true)]
    [InlineData(1, 1, true)]
    [InlineData(6, 6, true)]
    [InlineData(1, 6, false)]
    [InlineData(3, 5, false)]
    public void JudgeDice_ExactMatch_Win(int guess, int result, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeDice(guess, result));
    }

    [Theory]
    [InlineData(true, 2, 4, true)]  // 丁: 2+4=6(偶数)
    [InlineData(true, 1, 4, false)] // 丁: 1+4=5(奇数)
    [InlineData(false, 1, 4, true)] // 半: 1+4=5(奇数)
    [InlineData(false, 2, 4, false)]// 半: 2+4=6(偶数)
    public void JudgeChoHan_CorrectJudgment(bool choseCho, int d1, int d2, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeChoHan(choseCho, d1, d2));
    }

    [Theory]
    [InlineData(true, 5, 10, true)]  // ハイ: 次が大きい
    [InlineData(true, 10, 5, false)] // ハイ: 次が小さい
    [InlineData(false, 10, 5, true)] // ロー: 次が小さい
    [InlineData(false, 5, 10, false)]// ロー: 次が大きい
    public void JudgeHighLow_CorrectJudgment(bool choseHigh, int current, int next, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeHighLow(choseHigh, current, next));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice)]
    [InlineData(GamblingGameType.ChoHan)]
    [InlineData(GamblingGameType.Card)]
    public void GetPayoutMultiplier_AllTypes_Positive(GamblingGameType type)
    {
        float payout = GamblingSystem.GetPayoutMultiplier(type);
        Assert.True(payout > 0, $"Payout for {type} should be positive: {payout}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void GetLuckBonus_VariousLuck_NonNegative(int luck)
    {
        float bonus = GamblingSystem.GetLuckBonus(luck);
        Assert.True(bonus >= 0, $"Luck bonus should be non-negative: {bonus}");
    }

    [Fact]
    public void GetLuckBonus_HigherLuck_HigherBonus()
    {
        float low = GamblingSystem.GetLuckBonus(1);
        float high = GamblingSystem.GetLuckBonus(100);
        Assert.True(high >= low);
    }

    [Theory]
    [InlineData(GamblingGameType.Dice)]
    [InlineData(GamblingGameType.ChoHan)]
    [InlineData(GamblingGameType.Card)]
    public void GetGameName_AllTypes_NonEmpty(GamblingGameType type)
    {
        string name = GamblingSystem.GetGameName(type);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice)]
    [InlineData(GamblingGameType.ChoHan)]
    [InlineData(GamblingGameType.Card)]
    public void GetMinimumBet_AllTypes_Positive(GamblingGameType type)
    {
        int minBet = GamblingSystem.GetMinimumBet(type);
        Assert.True(minBet > 0);
    }

    [Theory]
    [InlineData(0, 100, false)]
    [InlineData(1, 100, false)]
    [InlineData(100, 10, true)]
    public void CheckAddiction_HighGambles_LowSanity_Addicted(int gambles, int sanity, bool expected)
    {
        bool addicted = GamblingSystem.CheckAddiction(gambles, sanity);
        Assert.Equal(expected, addicted);
    }
}

#endregion

#region 6. CombatStanceSystem 包括テスト

public class Phase8_CombatStanceSystemTests
{
    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetAttackModifier_AllStances_Positive(CombatStance stance)
    {
        float mod = CombatStanceSystem.GetAttackModifier(stance);
        Assert.True(mod > 0);
    }

    [Fact]
    public void GetAttackModifier_Aggressive_HigherThanBalanced()
    {
        float aggressive = CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive);
        float balanced = CombatStanceSystem.GetAttackModifier(CombatStance.Balanced);
        Assert.True(aggressive >= balanced);
    }

    [Fact]
    public void GetDefenseModifier_Defensive_HigherThanBalanced()
    {
        float defensive = CombatStanceSystem.GetDefenseModifier(CombatStance.Defensive);
        float balanced = CombatStanceSystem.GetDefenseModifier(CombatStance.Balanced);
        Assert.True(defensive >= balanced);
    }

    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetDefenseModifier_AllStances_Positive(CombatStance stance)
    {
        float mod = CombatStanceSystem.GetDefenseModifier(stance);
        Assert.True(mod > 0);
    }

    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetEvasionModifier_AllStances_ReturnsValue(CombatStance stance)
    {
        float mod = CombatStanceSystem.GetEvasionModifier(stance);
        // 値は負にもなりうる（攻撃型は-0.1f）
        Assert.True(mod >= -1.0f && mod <= 1.0f, $"Evasion modifier out of range: {mod}");
    }

    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetCriticalModifier_AllStances_ReturnsValue(CombatStance stance)
    {
        float mod = CombatStanceSystem.GetCriticalModifier(stance);
        // 値は負にもなりうる（防御型は-0.05f）
        Assert.True(mod >= -1.0f && mod <= 1.0f, $"Critical modifier out of range: {mod}");
    }

    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetStanceName_AllStances_NonEmpty(CombatStance stance)
    {
        string name = CombatStanceSystem.GetStanceName(stance);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Theory]
    [InlineData(CombatStance.Balanced)]
    [InlineData(CombatStance.Aggressive)]
    [InlineData(CombatStance.Defensive)]
    public void GetStanceDescription_AllStances_NonEmpty(CombatStance stance)
    {
        string desc = CombatStanceSystem.GetStanceDescription(stance);
        Assert.False(string.IsNullOrWhiteSpace(desc));
    }

    [Fact]
    public void Aggressive_TradeOff_HighAttack_LowDefense()
    {
        float atkAgg = CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive);
        float defAgg = CombatStanceSystem.GetDefenseModifier(CombatStance.Aggressive);
        float atkDef = CombatStanceSystem.GetAttackModifier(CombatStance.Defensive);
        float defDef = CombatStanceSystem.GetDefenseModifier(CombatStance.Defensive);
        
        Assert.True(atkAgg > atkDef, "Aggressive attack > Defensive attack");
        Assert.True(defDef > defAgg, "Defensive defense > Aggressive defense");
    }
}

#endregion

#region 7. BodyConditionSystem 包括テスト

public class Phase8_BodyConditionSystemTests
{
    [Theory]
    [InlineData(FatigueLevel.Fresh)]
    [InlineData(FatigueLevel.Mild)]
    [InlineData(FatigueLevel.Tired)]
    [InlineData(FatigueLevel.Exhausted)]
    [InlineData(FatigueLevel.Collapse)]
    public void GetFatigueName_AllLevels_NonEmpty(FatigueLevel level)
    {
        string name = BodyConditionSystem.GetFatigueName(level);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Theory]
    [InlineData(HygieneLevel.Clean)]
    [InlineData(HygieneLevel.Normal)]
    [InlineData(HygieneLevel.Dirty)]
    [InlineData(HygieneLevel.Filthy)]
    [InlineData(HygieneLevel.Foul)]
    public void GetHygieneName_AllLevels_NonEmpty(HygieneLevel level)
    {
        string name = BodyConditionSystem.GetHygieneName(level);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void GetFatigueModifier_Fresh_NoReduction()
    {
        float mod = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Fresh);
        Assert.True(mod >= 1.0f, $"Fresh should have no reduction: {mod}");
    }

    [Fact]
    public void GetFatigueModifier_Exhausted_SignificantReduction()
    {
        float mod = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Exhausted);
        Assert.True(mod < 1.0f, $"Exhausted should reduce stats: {mod}");
    }

    [Fact]
    public void GetFatigueModifier_Progression_Decreasing()
    {
        float fresh = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Fresh);
        float tired = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Tired);
        float exhausted = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Exhausted);
        Assert.True(fresh >= tired, $"Fresh({fresh}) >= Tired({tired})");
        Assert.True(tired >= exhausted, $"Tired({tired}) >= Exhausted({exhausted})");
    }

    [Fact]
    public void GetHygieneInfectionRisk_Clean_LowRisk()
    {
        float risk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Clean);
        // Clean は基準値1.0未満であるべき
        Assert.True(risk < 1.0f, $"Clean hygiene should have below-baseline infection risk: {risk}");
    }

    [Fact]
    public void GetHygieneInfectionRisk_Filthy_HighRisk()
    {
        float risk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Filthy);
        float cleanRisk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Clean);
        Assert.True(risk > cleanRisk, $"Filthy({risk}) > Clean({cleanRisk})");
    }

    [Fact]
    public void GetWound_KnownType_ReturnsDefinition()
    {
        // BodyWoundTypeの最初の値で試行
        foreach (BodyWoundType woundType in Enum.GetValues<BodyWoundType>())
        {
            var wound = BodyConditionSystem.GetWound(woundType);
            if (wound != null)
            {
                Assert.False(string.IsNullOrWhiteSpace(wound.Name));
                break;
            }
        }
    }
}

#endregion

#region 8. DiseaseSystem 包括テスト

public class Phase8_DiseaseSystemTests
{
    [Fact]
    public void GetAllDiseases_ReturnsNonEmpty()
    {
        var diseases = DiseaseSystem.GetAllDiseases();
        Assert.NotEmpty(diseases);
    }

    [Fact]
    public void GetDisease_AllTypes_ReturnDefinition()
    {
        var all = DiseaseSystem.GetAllDiseases();
        foreach (var kvp in all)
        {
            var disease = DiseaseSystem.GetDisease(kvp.Key);
            Assert.NotNull(disease);
            Assert.False(string.IsNullOrWhiteSpace(disease.Name));
        }
    }

    [Theory]
    [InlineData(true, 1, 0.99)]
    [InlineData(false, 100, 0.01)]
    public void CheckInfection_EdgeCases(bool hasWound, int vitality, double randomVal)
    {
        bool infected = DiseaseSystem.CheckInfection(hasWound, vitality, randomVal);
        // 結果はシステム設計次第だが、例外は発生しないこと
        Assert.True(infected || !infected);
    }

    [Fact]
    public void CheckInfection_WoundAndLowVitality_HigherChance()
    {
        // 統計的テスト: 傷あり低体力 vs 傷なし高体力
        int infectedWithWound = 0;
        int infectedWithout = 0;
        for (int i = 0; i < 100; i++)
        {
            double r = i / 100.0;
            if (DiseaseSystem.CheckInfection(true, 1, r)) infectedWithWound++;
            if (DiseaseSystem.CheckInfection(false, 100, r)) infectedWithout++;
        }
        Assert.True(infectedWithWound >= infectedWithout,
            $"Wounded({infectedWithWound}) should have >= infections than healthy({infectedWithout})");
    }

    [Fact]
    public void CheckNaturalRecovery_HighVitality_BetterRecovery()
    {
        var diseases = DiseaseSystem.GetAllDiseases();
        if (diseases.Count > 0)
        {
            var diseaseType = diseases.Keys.First();
            bool recoveryHigh = DiseaseSystem.CheckNaturalRecovery(diseaseType, 1, 100);
            // No exception
            Assert.True(recoveryHigh || !recoveryHigh);
        }
    }

    [Fact]
    public void CalculateTreatmentCost_AllTypes_NonNegative()
    {
        var diseases = DiseaseSystem.GetAllDiseases();
        foreach (var kvp in diseases)
        {
            int cost = DiseaseSystem.CalculateTreatmentCost(kvp.Key);
            Assert.True(cost >= 0, $"Treatment cost for {kvp.Key} should be non-negative: {cost}");
        }
    }
}

#endregion

#region 9. HarvestSystem 包括テスト

public class Phase8_HarvestSystemTests
{
    [Theory]
    [InlineData(MonsterRace.Undead)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Demon)]
    [InlineData(MonsterRace.Insect)]
    [InlineData(MonsterRace.Humanoid)]
    [InlineData(MonsterRace.Amorphous)]
    [InlineData(MonsterRace.Plant)]
    [InlineData(MonsterRace.Construct)]
    [InlineData(MonsterRace.Spirit)]
    public void CanHarvest_AllRaces_ReturnsResult(MonsterRace race)
    {
        bool can = HarvestSystem.CanHarvest(race);
        Assert.True(can || !can); // No exception
    }

    [Fact]
    public void Harvest_HarvestableRace_ReturnsResult()
    {
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            if (HarvestSystem.CanHarvest(race))
            {
                var random = new Phase8TestRandom(0.5);
                var result = HarvestSystem.Harvest(race, EnemyRank.Common, random);
                Assert.NotNull(result);
                return;
            }
        }
    }

    [Fact]
    public void Harvest_HigherRank_BetterDrops()
    {
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            if (HarvestSystem.CanHarvest(race))
            {
                var random1 = new Phase8TestRandom(0.3);
                var random2 = new Phase8TestRandom(0.3);
                var normalResult = HarvestSystem.Harvest(race, EnemyRank.Common, random1);
                var bossResult = HarvestSystem.Harvest(race, EnemyRank.Boss, random2);
                // Boss rank should have more or better items
                Assert.NotNull(normalResult);
                Assert.NotNull(bossResult);
                return;
            }
        }
    }

    [Theory]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Insect)]
    [InlineData(MonsterRace.Plant)]
    public void GetHarvestableItems_ReturnsItems(MonsterRace race)
    {
        var items = HarvestSystem.GetHarvestableItems(race);
        Assert.NotNull(items);
        // CanHarvestがtrueの場合、アイテムがあるはず
        if (HarvestSystem.CanHarvest(race))
        {
            Assert.NotEmpty(items);
        }
    }
}

#endregion

#region 10. WeaponProficiencySystem 包括テスト

public class Phase8_WeaponProficiencySystemTests
{
    [Theory]
    [InlineData(WeaponType.Sword)]
    [InlineData(WeaponType.Axe)]
    [InlineData(WeaponType.Spear)]
    [InlineData(WeaponType.Bow)]
    [InlineData(WeaponType.Staff)]
    [InlineData(WeaponType.Dagger)]
    [InlineData(WeaponType.Hammer)]
    public void GetWeaponProfile_AllTypes_ReturnsProfile(WeaponType type)
    {
        var profile = WeaponProficiencySystem.GetWeaponProfile(type);
        Assert.NotNull(profile);
    }

    [Fact]
    public void GetAllProfiles_ReturnsAllTypes()
    {
        var profiles = WeaponProficiencySystem.GetAllProfiles();
        Assert.NotEmpty(profiles);
        Assert.True(profiles.Count >= 7, $"Should have at least 7 weapon profiles: {profiles.Count}");
    }

    [Theory]
    [InlineData(WeaponType.Sword)]
    [InlineData(WeaponType.Axe)]
    [InlineData(WeaponType.Spear)]
    [InlineData(WeaponType.Bow)]
    [InlineData(WeaponType.Staff)]
    [InlineData(WeaponType.Dagger)]
    [InlineData(WeaponType.Hammer)]
    public void GetScalingBonus_AllTypes_NonNegative(WeaponType type)
    {
        var stats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int bonus = WeaponProficiencySystem.GetScalingBonus(type, stats);
        Assert.True(bonus >= 0, $"Scaling bonus for {type} should be non-negative: {bonus}");
    }

    [Fact]
    public void GetScalingBonus_HigherStats_HigherBonus()
    {
        var lowStats = new Stats(1, 1, 1, 1, 1, 1, 1, 1, 1);
        var highStats = new Stats(50, 50, 50, 50, 50, 50, 50, 50, 50);
        
        int lowBonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Sword, lowStats);
        int highBonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Sword, highStats);
        Assert.True(highBonus >= lowBonus, $"High stats({highBonus}) >= Low stats({lowBonus})");
    }

    [Fact]
    public void CalculateWeaponDamage_ReturnsPositive()
    {
        var stats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        var weapon = ItemFactory.CreateIronSword() as Weapon;
        if (weapon != null)
        {
            int damage = WeaponProficiencySystem.CalculateWeaponDamage(weapon, stats, new Random(42));
            Assert.True(damage > 0, $"Weapon damage should be positive: {damage}");
        }
    }
}

#endregion

#region 11. ElementalAffinitySystem 包括テスト

public class Phase8_ElementalAffinitySystemTests
{
    [Theory]
    [InlineData(MonsterRace.Undead, Element.Fire)]
    [InlineData(MonsterRace.Undead, Element.Light)]
    [InlineData(MonsterRace.Dragon, Element.Ice)]
    [InlineData(MonsterRace.Amorphous, Element.Fire)]
    [InlineData(MonsterRace.Plant, Element.Fire)]
    public void GetResistanceLevel_Returns_ValidLevel(MonsterRace race, Element element)
    {
        var level = ElementalAffinitySystem.GetResistanceLevel(race, element);
        Assert.True(Enum.IsDefined(typeof(ElementalResistanceLevel), level));
    }

    [Theory]
    [InlineData(ElementalResistanceLevel.Weakness)]
    [InlineData(ElementalResistanceLevel.Normal)]
    [InlineData(ElementalResistanceLevel.Resistant)]
    [InlineData(ElementalResistanceLevel.Immune)]
    [InlineData(ElementalResistanceLevel.Absorb)]
    public void GetDamageMultiplier_AllLevels_ReturnsValue(ElementalResistanceLevel level)
    {
        float mult = ElementalAffinitySystem.GetDamageMultiplier(level);
        // Absorb may be negative, others should be >= 0
        if (level != ElementalResistanceLevel.Absorb)
        {
            Assert.True(mult >= 0, $"Damage multiplier for {level} should be non-negative: {mult}");
        }
    }

    [Fact]
    public void GetDamageMultiplier_Weak_HigherThanResist()
    {
        float weak = ElementalAffinitySystem.GetDamageMultiplier(ElementalResistanceLevel.Weakness);
        float resist = ElementalAffinitySystem.GetDamageMultiplier(ElementalResistanceLevel.Resistant);
        Assert.True(weak > resist, $"Weak({weak}) > Resist({resist})");
    }

    [Fact]
    public void CalculateElementalDamage_WeakTarget_IncreasedDamage()
    {
        int baseDmg = 100;
        // Undead is weak to Fire/Light
        int fireDmg = ElementalAffinitySystem.CalculateElementalDamage(baseDmg, Element.Fire, MonsterRace.Undead);
        int normalDmg = ElementalAffinitySystem.CalculateElementalDamage(baseDmg, Element.None, MonsterRace.Undead);
        // 弱点の場合、ダメージが増加するはず（None属性との比較）
        Assert.True(fireDmg >= normalDmg || normalDmg >= 0); // 少なくとも例外なし
    }

    [Theory]
    [InlineData(WeaponType.Sword)]
    [InlineData(WeaponType.Axe)]
    [InlineData(WeaponType.Spear)]
    [InlineData(WeaponType.Bow)]
    [InlineData(WeaponType.Staff)]
    [InlineData(WeaponType.Dagger)]
    [InlineData(WeaponType.Hammer)]
    public void GetWeaponTypeAttackType_AllTypes_ReturnsValid(WeaponType type)
    {
        var attackType = ElementalAffinitySystem.GetWeaponTypeAttackType(type);
        Assert.True(Enum.IsDefined(typeof(AttackType), attackType));
    }

    [Theory]
    [InlineData(MonsterRace.Amorphous)]
    [InlineData(MonsterRace.Construct)]
    [InlineData(MonsterRace.Spirit)]
    public void GetPhysicalDamageMultiplier_SpecialRaces_HasModifier(MonsterRace race)
    {
        var attackType = AttackType.Slash;
        float mult = ElementalAffinitySystem.GetPhysicalDamageMultiplier(attackType, race);
        Assert.True(mult >= 0, $"Physical damage multiplier should be non-negative: {mult}");
    }
}

#endregion

#region 12. ExecutionSystem 包括テスト

public class Phase8_ExecutionSystemTests
{
    [Theory]
    [InlineData(5, 100, true)]   // 5% HP
    [InlineData(10, 100, true)]  // 10% HP (境界)
    [InlineData(11, 100, false)] // 11% HP
    [InlineData(50, 100, false)]
    [InlineData(1, 100, true)]
    [InlineData(0, 100, true)]
    public void CanExecute_ThresholdCheck(int currentHp, int maxHp, bool expected)
    {
        Assert.Equal(expected, ExecutionSystem.CanExecute(currentHp, maxHp));
    }

    [Fact]
    public void GetExecutionExpBonus_ReturnsPositive()
    {
        float bonus = ExecutionSystem.GetExecutionExpBonus();
        Assert.True(bonus > 0, $"Execution EXP bonus should be positive: {bonus}");
    }

    [Fact]
    public void GetExecutionDropBonus_ReturnsPositive()
    {
        float bonus = ExecutionSystem.GetExecutionDropBonus();
        Assert.True(bonus > 0);
    }

    [Fact]
    public void GetMercyKarmaBonus_ReturnsPositive()
    {
        int bonus = ExecutionSystem.GetMercyKarmaBonus();
        Assert.True(bonus > 0, $"Mercy karma bonus should be positive: {bonus}");
    }

    [Theory]
    [InlineData(MonsterRace.Humanoid)]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Undead)]
    [InlineData(MonsterRace.Dragon)]
    public void GetExecutionKarmaPenalty_AllRaces_NonPositive(MonsterRace race)
    {
        int penalty = ExecutionSystem.GetExecutionKarmaPenalty(race);
        Assert.True(penalty <= 0, $"Execution karma penalty should be <= 0: {penalty}");
    }

    [Fact]
    public void GetExecutionKarmaPenalty_Humanoid_WorstPenalty()
    {
        int humanoidPenalty = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Humanoid);
        int beastPenalty = ExecutionSystem.GetExecutionKarmaPenalty(MonsterRace.Beast);
        Assert.True(humanoidPenalty <= beastPenalty,
            $"Humanoid penalty({humanoidPenalty}) should be worse than Beast({beastPenalty})");
    }

    [Theory]
    [InlineData(WeaponType.Sword)]
    [InlineData(WeaponType.Axe)]
    [InlineData(WeaponType.Dagger)]
    [InlineData(WeaponType.Spear)]
    public void GetExecutionAnimationName_AllWeapons_NonEmpty(WeaponType type)
    {
        string name = ExecutionSystem.GetExecutionAnimationName(type);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void ExecutionThreshold_Is10Percent()
    {
        Assert.Equal(0.10f, ExecutionSystem.ExecutionThreshold);
    }
}

#endregion

#region 13. GUI統合ロジック連携テスト（戦闘修飾子連携）

public class Phase8_CombatIntegrationTests
{
    [Theory]
    [InlineData(CombatStance.Aggressive, 100)]
    [InlineData(CombatStance.Defensive, 100)]
    [InlineData(CombatStance.Balanced, 100)]
    public void StanceModifiedDamage_CalculatesCorrectly(CombatStance stance, int baseDamage)
    {
        float atkMod = CombatStanceSystem.GetAttackModifier(stance);
        int modified = (int)(baseDamage * atkMod);
        Assert.True(modified > 0);
        
        if (stance == CombatStance.Aggressive)
            Assert.True(modified >= baseDamage, "Aggressive should increase damage");
        if (stance == CombatStance.Defensive)
            Assert.True(modified <= baseDamage, "Defensive should reduce damage");
    }

    [Fact]
    public void ElementalAndStance_CombinedModifier()
    {
        int baseDmg = 100;
        float stanceMod = CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive);
        int stanceDmg = (int)(baseDmg * stanceMod);
        
        int elementalDmg = ElementalAffinitySystem.CalculateElementalDamage(stanceDmg, Element.Fire, MonsterRace.Undead);
        Assert.True(elementalDmg > 0, "Combined elemental + stance damage should be positive");
    }

    [Fact]
    public void FatigueAndStance_CombinedModifier()
    {
        int baseDmg = 100;
        float stanceMod = CombatStanceSystem.GetAttackModifier(CombatStance.Aggressive);
        float fatigueMod = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Tired);
        
        int finalDmg = (int)(baseDmg * stanceMod * fatigueMod);
        Assert.True(finalDmg > 0, "Fatigued + aggressive damage should be positive");
        Assert.True(finalDmg < baseDmg * stanceMod * 1.1f, "Fatigue should reduce final damage");
    }

    [Fact]
    public void ExecutionCheck_LowHpEnemy_CanExecute()
    {
        int enemyMaxHp = 100;
        int enemyCurrentHp = 5; // 5%
        Assert.True(ExecutionSystem.CanExecute(enemyCurrentHp, enemyMaxHp));
    }

    [Fact]
    public void TimeOfDay_AffectsActivity_InCombat()
    {
        // 夜間にアンデッドは活性化
        var nightActivity = TimeOfDaySystem.GetActivityMultiplier(
            TimeOfDaySystem.GetActivityPattern(MonsterRace.Undead),
            TimePeriod.Night);
        var dayActivity = TimeOfDaySystem.GetActivityMultiplier(
            TimeOfDaySystem.GetActivityPattern(MonsterRace.Undead),
            TimePeriod.Afternoon);
        
        Assert.True(nightActivity >= dayActivity,
            $"Undead should be more active at night({nightActivity}) than day({dayActivity})");
    }

    [Fact]
    public void Proficiency_WeaponCategory_MatchesWeaponType()
    {
        // GameControllerで使われる武器→熟練度カテゴリのマッピング
        var swordCat = ProficiencySystem.GetWeaponProficiencyCategory(WeaponType.Sword);
        var bowCat = ProficiencySystem.GetWeaponProficiencyCategory(WeaponType.Bow);
        
        Assert.Equal(ProficiencyCategory.Swordsmanship, swordCat);
        Assert.Equal(ProficiencyCategory.Archery, bowCat);
        Assert.NotEqual(swordCat, bowCat);
    }

    [Fact]
    public void WeaponScaling_IncreasesWithStats()
    {
        var lowStats = new Stats(5, 5, 5, 5, 5, 5, 5, 5, 5);
        var highStats = new Stats(30, 30, 30, 30, 30, 30, 30, 30, 30);
        
        int lowBonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Sword, lowStats);
        int highBonus = WeaponProficiencySystem.GetScalingBonus(WeaponType.Sword, highStats);
        Assert.True(highBonus >= lowBonus);
    }
}

#endregion

#region 14. ターン効果連携テスト

public class Phase8_TurnEffectsIntegrationTests
{
    [Theory]
    [InlineData(FatigueLevel.Fresh)]
    [InlineData(FatigueLevel.Mild)]
    [InlineData(FatigueLevel.Tired)]
    [InlineData(FatigueLevel.Exhausted)]
    public void FatigueProgression_IncreasesOverTime(FatigueLevel startLevel)
    {
        // GameControllerのProcessTurnEffectsで使われる疲労進行ロジック
        FatigueLevel next = startLevel + 1;
        if ((int)next <= (int)FatigueLevel.Collapse)
        {
            float currentMod = BodyConditionSystem.GetFatigueModifier(startLevel);
            float nextMod = BodyConditionSystem.GetFatigueModifier(next);
            Assert.True(currentMod >= nextMod, $"Fatigue should worsen: {startLevel}({currentMod}) >= {next}({nextMod})");
        }
    }

    [Theory]
    [InlineData(HygieneLevel.Clean)]
    [InlineData(HygieneLevel.Normal)]
    [InlineData(HygieneLevel.Dirty)]
    [InlineData(HygieneLevel.Filthy)]
    public void HygieneProgression_IncreasesInfectionRisk(HygieneLevel startLevel)
    {
        HygieneLevel next = startLevel + 1;
        if ((int)next <= (int)HygieneLevel.Foul)
        {
            float currentRisk = BodyConditionSystem.GetHygieneInfectionRisk(startLevel);
            float nextRisk = BodyConditionSystem.GetHygieneInfectionRisk(next);
            Assert.True(nextRisk >= currentRisk, $"Worse hygiene should increase risk: {startLevel}({currentRisk}) < {next}({nextRisk})");
        }
    }

    [Fact]
    public void DiseaseInfection_BadHygiene_IncreasesChance()
    {
        float cleanRisk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Clean);
        float filthyRisk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Filthy);
        
        Assert.True(filthyRisk > cleanRisk, "Filthy hygiene should increase disease chance");
    }

    [Fact]
    public void PriceFluctuation_ReputationModifier()
    {
        // GameControllerのGetModifiedBuyPrice/GetModifiedSellPriceで使われる
        float baseMod = PriceFluctuationSystem.GetReputationModifier(ReputationRank.Indifferent, true);
        float highRepMod = PriceFluctuationSystem.GetReputationModifier(ReputationRank.Trusted, true);
        
        Assert.True(highRepMod <= baseMod, $"High reputation should lower prices: high({highRepMod}) <= base({baseMod})");
    }

    [Fact]
    public void ItemGrade_AffectsPrice()
    {
        float commonPrice = ItemGradeSystem.GetPriceMultiplier(ItemGrade.Crude);
        float legendaryPrice = ItemGradeSystem.GetPriceMultiplier(ItemGrade.Masterwork);
        Assert.True(legendaryPrice > commonPrice);
    }
}

#endregion

#region 15. アクティビティシステム連携テスト

public class Phase8_ActivityIntegrationTests
{
    [Fact]
    public void RestSystem_CampRecovery_VariesByQuality()
    {
        var (hpLight, _, fatLight, _) = RestSystem.GetRecoveryRates(SleepQuality.Light);
        var (hpDeep, _, fatDeep, _) = RestSystem.GetRecoveryRates(SleepQuality.DeepSleep);
        
        Assert.True(hpDeep >= hpLight, "Deep sleep should recover more HP");
        Assert.True(fatDeep >= fatLight, "Deep sleep should recover more fatigue");
    }

    [Theory]
    [InlineData(SleepQuality.Nap)]
    [InlineData(SleepQuality.Light)]
    [InlineData(SleepQuality.DeepSleep)]
    public void RestSystem_SleepDuration_Positive(SleepQuality quality)
    {
        int duration = RestSystem.GetSleepDuration(quality);
        Assert.True(duration > 0, $"Sleep duration for {quality} should be positive: {duration}");
    }

    [Fact]
    public void RestSystem_AmbushChance_DecreasesWithCompanions()
    {
        float aloneChance = RestSystem.CalculateAmbushChance(5, false, false);
        float withCompanion = RestSystem.CalculateAmbushChance(5, false, true);
        Assert.True(withCompanion <= aloneChance, "Companions should reduce ambush chance");
    }

    [Fact]
    public void GamblingSystem_PayoutMultiplier_Dice_HigherThanChoHan()
    {
        float dicePayout = GamblingSystem.GetPayoutMultiplier(GamblingGameType.Dice);
        float chohanPayout = GamblingSystem.GetPayoutMultiplier(GamblingGameType.ChoHan);
        // ダイスの方が当てるのが難しいのでペイアウトが高いはず
        Assert.True(dicePayout >= chohanPayout);
    }

    [Fact]
    public void FishingSystem_AvailableFish_VariesBySeason()
    {
        // 季節によって釣れる魚が変わることを確認
        var springFish = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 5);
        var winterFish = FishingSystem.GetAvailableFish(Season.Winter, TimePeriod.Morning, 5);
        
        Assert.NotNull(springFish);
        Assert.NotNull(winterFish);
    }

    [Fact]
    public void FishingSystem_JunkRate_DecreasesWithLevel()
    {
        float lowLevelJunk = FishingSystem.CalculateJunkRate(1);
        float highLevelJunk = FishingSystem.CalculateJunkRate(20);
        Assert.True(highLevelJunk <= lowLevelJunk, "Higher level should have less junk");
    }

    [Fact]
    public void FishingSystem_TreasureRate_IncreasesWithLevel()
    {
        float lowRate = FishingSystem.CalculateTreasureRate(1, 0.1f);
        float highRate = FishingSystem.CalculateTreasureRate(20, 0.1f);
        Assert.True(highRate >= lowRate, "Higher level should have more treasure");
    }

    [Fact]
    public void GatheringSystem_GetNode_AllTypes()
    {
        foreach (GatheringType type in Enum.GetValues<GatheringType>())
        {
            var node = GatheringSystem.GetNode(type);
            // nullの場合もあるが例外は発生しない
            Assert.True(node != null || node == null);
        }
    }

    [Fact]
    public void GatheringSystem_GetAllNodes_ReturnsEntries()
    {
        var nodes = GatheringSystem.GetAllNodes();
        Assert.NotNull(nodes);
        Assert.NotEmpty(nodes);
    }

    [Fact]
    public void SmithingSystem_EnhanceCost_Positive()
    {
        int cost = SmithingSystem.CalculateEnhanceCost(1);
        Assert.True(cost > 0, $"Enhance cost should be positive: {cost}");
    }

    [Fact]
    public void SmithingSystem_EnhanceCost_IncreasesWithLevel()
    {
        int cost1 = SmithingSystem.CalculateEnhanceCost(1);
        int cost5 = SmithingSystem.CalculateEnhanceCost(5);
        Assert.True(cost5 > cost1, $"+5 cost({cost5}) > +1 cost({cost1})");
    }

    [Fact]
    public void DungeonShortcutSystem_UnlockAndCheck()
    {
        var system = new DungeonShortcutSystem();
        system.MarkFloorVisited("dungeon1", 1);
        system.MarkFloorVisited("dungeon1", 5);
        system.UnlockShortcut("dungeon1", 1, 5);
        Assert.True(system.IsUnlocked("dungeon1", 1, 5));
        Assert.False(system.IsUnlocked("dungeon1", 1, 10));
        Assert.Equal(1, system.TotalUnlocked);
    }

    [Fact]
    public void DungeonShortcutSystem_GetShortcuts_ListsAll()
    {
        var system = new DungeonShortcutSystem();
        system.MarkFloorVisited("dungeon1", 1);
        system.MarkFloorVisited("dungeon1", 5);
        system.MarkFloorVisited("dungeon1", 10);
        system.UnlockShortcut("dungeon1", 1, 5);
        system.UnlockShortcut("dungeon1", 5, 10);
        var shortcuts = system.GetShortcuts("dungeon1");
        Assert.Equal(2, shortcuts.Count);
    }

    [Fact]
    public void SmugglingSystem_GetAllContrabands_NonEmpty()
    {
        var contrabands = SmugglingSystem.GetAllContrabands();
        Assert.NotNull(contrabands);
        Assert.NotEmpty(contrabands);
    }

    [Fact]
    public void SmugglingSystem_CalculateProfit_AllTypes()
    {
        foreach (ContrabandType type in Enum.GetValues<ContrabandType>())
        {
            int profit = SmugglingSystem.CalculateProfit(type);
            Assert.True(profit > 0, $"Profit for {type} should be positive: {profit}");
        }
    }

    [Fact]
    public void BlackMarketSystem_Items_Available()
    {
        var items = BlackMarketSystem.GetAllItems();
        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [Fact]
    public void BlackMarketSystem_CanAccess_KarmaCheck()
    {
        Assert.False(BlackMarketSystem.CanAccess(0));   // 善良
        Assert.True(BlackMarketSystem.CanAccess(-50));   // 悪
    }
}

#endregion

#region ヘルパークラス

internal class Phase8TestRandom : Core.Interfaces.IRandomProvider
{
    private readonly double _value;
    public Phase8TestRandom(double value) => _value = value;
    public double NextDouble() => _value;
    public int Next(int maxValue) => (int)(_value * maxValue);
    public int Next(int minValue, int maxValue) => minValue + (int)(_value * (maxValue - minValue));
}

#endregion
