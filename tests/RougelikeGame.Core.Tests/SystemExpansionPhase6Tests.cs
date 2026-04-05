using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Items;
using static RougelikeGame.Core.Systems.EnvironmentalCombatSystem;
using static RougelikeGame.Core.Systems.NpcRoutineSystem;

namespace RougelikeGame.Core.Tests;

#region 1. DifficultySettings Tests

public class Phase6Expansion_DifficultySettingsTests
{
    [Theory]
    [InlineData(DifficultyLevel.Easy, 0.7, 1.5, 5, false)]
    [InlineData(DifficultyLevel.Normal, 1.0, 1.0, 3, false)]
    [InlineData(DifficultyLevel.Hard, 1.3, 0.8, 2, false)]
    [InlineData(DifficultyLevel.Nightmare, 1.6, 0.6, 1, false)]
    [InlineData(DifficultyLevel.Ironman, 1.5, 0.8, 0, true)]
    public void Get_ReturnsCorrectCoreSettings(DifficultyLevel level, double enemyStat, double exp, int rescue, bool perma)
    {
        var s = DifficultySettings.Get(level);
        Assert.Equal(enemyStat, s.EnemyStatMultiplier);
        Assert.Equal(exp, s.ExpMultiplier);
        Assert.Equal(rescue, s.RescueCount);
        Assert.Equal(perma, s.PermaDeath);
    }

    [Fact]
    public void Get_DefaultReturnsNormal()
    {
        var s = DifficultySettings.Get((DifficultyLevel)999);
        Assert.Equal(DifficultyLevel.Normal, s.Level);
    }

    [Fact]
    public void Easy_AllProperties()
    {
        var s = DifficultySettings.Easy;
        Assert.Equal(DifficultyLevel.Easy, s.Level);
        Assert.Equal("簡単", s.DisplayName);
        Assert.NotEmpty(s.Description);
        Assert.Equal(0.7, s.EnemyStatMultiplier);
        Assert.Equal(1.5, s.ExpMultiplier);
        Assert.Equal(0.5, s.HungerDecayMultiplier);
        Assert.Equal(1.5, s.TurnLimitMultiplier);
        Assert.Equal(5, s.RescueCount);
        Assert.Equal(1.5, s.ItemDropMultiplier);
        Assert.Equal(1.5, s.GoldMultiplier);
        Assert.Equal(0.7, s.DamageTakenMultiplier);
        Assert.Equal(1.3, s.DamageDealtMultiplier);
        Assert.False(s.PermaDeath);
    }

    [Fact]
    public void Normal_AllProperties()
    {
        var s = DifficultySettings.Normal;
        Assert.Equal(DifficultyLevel.Normal, s.Level);
        Assert.Equal("普通", s.DisplayName);
        Assert.NotEmpty(s.Description);
        Assert.Equal(1.0, s.EnemyStatMultiplier);
        Assert.Equal(1.0, s.ExpMultiplier);
        Assert.Equal(1.0, s.HungerDecayMultiplier);
        Assert.Equal(1.0, s.TurnLimitMultiplier);
        Assert.Equal(3, s.RescueCount);
        Assert.Equal(1.0, s.ItemDropMultiplier);
        Assert.Equal(1.0, s.GoldMultiplier);
        Assert.Equal(1.0, s.DamageTakenMultiplier);
        Assert.Equal(1.0, s.DamageDealtMultiplier);
        Assert.False(s.PermaDeath);
    }

    [Fact]
    public void Hard_AllProperties()
    {
        var s = DifficultySettings.Hard;
        Assert.Equal(DifficultyLevel.Hard, s.Level);
        Assert.Equal("難しい", s.DisplayName);
        Assert.NotEmpty(s.Description);
        Assert.Equal(1.3, s.EnemyStatMultiplier);
        Assert.Equal(0.8, s.ExpMultiplier);
        Assert.Equal(1.3, s.HungerDecayMultiplier);
        Assert.Equal(0.8, s.TurnLimitMultiplier);
        Assert.Equal(2, s.RescueCount);
        Assert.Equal(0.7, s.ItemDropMultiplier);
        Assert.Equal(0.7, s.GoldMultiplier);
        Assert.Equal(1.3, s.DamageTakenMultiplier);
        Assert.Equal(0.9, s.DamageDealtMultiplier);
        Assert.False(s.PermaDeath);
    }

    [Fact]
    public void Nightmare_AllProperties()
    {
        var s = DifficultySettings.Nightmare;
        Assert.Equal(DifficultyLevel.Nightmare, s.Level);
        Assert.Equal("悪夢", s.DisplayName);
        Assert.NotEmpty(s.Description);
        Assert.Equal(1.6, s.EnemyStatMultiplier);
        Assert.Equal(0.6, s.ExpMultiplier);
        Assert.Equal(1.5, s.HungerDecayMultiplier);
        Assert.Equal(0.6, s.TurnLimitMultiplier);
        Assert.Equal(1, s.RescueCount);
        Assert.Equal(0.5, s.ItemDropMultiplier);
        Assert.Equal(0.5, s.GoldMultiplier);
        Assert.Equal(1.6, s.DamageTakenMultiplier);
        Assert.Equal(0.8, s.DamageDealtMultiplier);
        Assert.False(s.PermaDeath);
    }

    [Fact]
    public void Ironman_AllProperties()
    {
        var s = DifficultySettings.IronmanSettings;
        Assert.Equal(DifficultyLevel.Ironman, s.Level);
        Assert.Equal("鉄人", s.DisplayName);
        Assert.NotEmpty(s.Description);
        Assert.Equal(1.5, s.EnemyStatMultiplier);
        Assert.Equal(0.8, s.ExpMultiplier);
        Assert.Equal(1.4, s.HungerDecayMultiplier);
        Assert.Equal(0.7, s.TurnLimitMultiplier);
        Assert.Equal(0, s.RescueCount);
        Assert.Equal(0.6, s.ItemDropMultiplier);
        Assert.Equal(0.6, s.GoldMultiplier);
        Assert.Equal(1.8, s.DamageTakenMultiplier);
        Assert.Equal(0.9, s.DamageDealtMultiplier);
        Assert.True(s.PermaDeath);
    }

    [Theory]
    [InlineData(DifficultyLevel.Easy)]
    [InlineData(DifficultyLevel.Normal)]
    [InlineData(DifficultyLevel.Hard)]
    [InlineData(DifficultyLevel.Nightmare)]
    [InlineData(DifficultyLevel.Ironman)]
    public void Get_AllLevelsHaveDisplayNameAndDescription(DifficultyLevel level)
    {
        var s = DifficultySettings.Get(level);
        Assert.NotNull(s.DisplayName);
        Assert.NotEmpty(s.DisplayName);
        Assert.NotNull(s.Description);
        Assert.NotEmpty(s.Description);
    }

    [Theory]
    [InlineData(DifficultyLevel.Easy)]
    [InlineData(DifficultyLevel.Normal)]
    [InlineData(DifficultyLevel.Hard)]
    [InlineData(DifficultyLevel.Nightmare)]
    [InlineData(DifficultyLevel.Ironman)]
    public void Get_AllLevelsHavePositiveMultipliers(DifficultyLevel level)
    {
        var s = DifficultySettings.Get(level);
        Assert.True(s.EnemyStatMultiplier > 0);
        Assert.True(s.ExpMultiplier > 0);
        Assert.True(s.HungerDecayMultiplier > 0);
        Assert.True(s.TurnLimitMultiplier > 0);
        Assert.True(s.DamageTakenMultiplier > 0);
        Assert.True(s.DamageDealtMultiplier > 0);
    }
}

#endregion

#region 2. CompanionSystem Tests

public class Phase6Expansion_CompanionSystemTests
{
    private CompanionSystem CreateSystem() => new();

    private CompanionSystem.CompanionData MakeCompanion(string name = "戦士A",
        CompanionType type = CompanionType.Mercenary,
        CompanionAIMode mode = CompanionAIMode.Aggressive,
        int level = 5, int loyalty = 50, int hp = 100, int maxHp = 100,
        int attack = 10, int defense = 5)
        => new(name, type, mode, level, loyalty, 0, hp, maxHp, attack, defense);

    [Fact]
    public void AddCompanion_Success()
    {
        var sys = CreateSystem();
        Assert.True(sys.AddCompanion(MakeCompanion("A")));
        Assert.Single(sys.Party);
    }

    [Fact]
    public void AddCompanion_DuplicateNameReturnsFalse()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A"));
        Assert.False(sys.AddCompanion(MakeCompanion("A")));
    }

    [Fact]
    public void AddCompanion_OverCapacityReturnsFalse()
    {
        var sys = CreateSystem();
        for (int i = 0; i < CompanionSystem.MaxPartySize; i++)
            sys.AddCompanion(MakeCompanion($"C{i}"));
        Assert.False(sys.AddCompanion(MakeCompanion("Extra")));
    }

    [Fact]
    public void MaxPartySize_Is4()
    {
        Assert.Equal(4, CompanionSystem.MaxPartySize);
    }

    [Fact]
    public void RemoveCompanion_Success()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A"));
        Assert.True(sys.RemoveCompanion("A"));
        Assert.Empty(sys.Party);
    }

    [Fact]
    public void RemoveCompanion_NonExistentReturnsFalse()
    {
        var sys = CreateSystem();
        Assert.False(sys.RemoveCompanion("nobody"));
    }

    [Fact]
    public void SetAIMode_Success()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Aggressive));
        Assert.True(sys.SetAIMode("A", CompanionAIMode.Defensive));
        Assert.Equal(CompanionAIMode.Defensive, sys.Party[0].AIMode);
    }

    [Fact]
    public void SetAIMode_NonExistentReturnsFalse()
    {
        var sys = CreateSystem();
        Assert.False(sys.SetAIMode("nobody", CompanionAIMode.Wait));
    }

    [Theory]
    [InlineData(CompanionType.Mercenary, 1, 150)]
    [InlineData(CompanionType.Mercenary, 5, 350)]
    [InlineData(CompanionType.Ally, 10, 0)]
    [InlineData(CompanionType.Pet, 1, 70)]
    [InlineData(CompanionType.Pet, 5, 150)]
    public void CalculateHireCost_ReturnsCorrect(CompanionType type, int level, int expected)
    {
        Assert.Equal(expected, CompanionSystem.CalculateHireCost(type, level));
    }

    [Theory]
    [InlineData(9, CompanionType.Mercenary, true)]
    [InlineData(10, CompanionType.Mercenary, false)]
    [InlineData(4, CompanionType.Ally, true)]
    [InlineData(5, CompanionType.Ally, false)]
    [InlineData(14, CompanionType.Pet, true)]
    [InlineData(15, CompanionType.Pet, false)]
    public void CheckDesertion_ThresholdCheck(int loyalty, CompanionType type, bool expected)
    {
        Assert.Equal(expected, CompanionSystem.CheckDesertion(loyalty, type));
    }

    [Theory]
    [InlineData(CompanionType.Mercenary, "傭兵")]
    [InlineData(CompanionType.Ally, "仲間")]
    [InlineData(CompanionType.Pet, "ペット")]
    public void GetTypeName_ReturnsJapanese(CompanionType type, string expected)
    {
        Assert.Equal(expected, CompanionSystem.GetTypeName(type));
    }

    [Fact]
    public void ProcessCompanionTurns_AggressiveWithEnemy()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Aggressive));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 1);
        Assert.Single(results);
        Assert.True(results[0].DamageDealt > 0);
        Assert.Equal("ゴブリン", results[0].TargetName);
    }

    [Fact]
    public void ProcessCompanionTurns_AggressiveNoEnemy()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Aggressive));
        var results = sys.ProcessCompanionTurns(false);
        Assert.Single(results);
        Assert.Contains("探している", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_AggressiveEnemyTooFar()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Aggressive));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 5);
        Assert.Contains("探している", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_DefensiveWithCloseEnemy()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Defensive));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 1);
        Assert.Single(results);
        Assert.True(results[0].DamageDealt > 0);
        Assert.Contains("反撃", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_DefensiveEnemyFar()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Defensive));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 3);
        Assert.Contains("防御態勢", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_Support()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Support));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 1);
        Assert.Contains("追従", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_Wait()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", mode: CompanionAIMode.Wait));
        var results = sys.ProcessCompanionTurns(true, "ゴブリン", 1);
        Assert.Contains("待機", results[0].ActionDescription);
    }

    [Fact]
    public void ProcessCompanionTurns_DeadCompanionSkipped()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 0, maxHp: 100) with { IsAlive = false });
        var results = sys.ProcessCompanionTurns(false);
        Assert.Empty(results);
    }

    [Fact]
    public void DamageCompanion_AppliesDamage()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 50, defense: 5));
        bool died = sys.DamageCompanion("A", 10);
        Assert.False(died);
        Assert.True(sys.Party[0].Hp < 50);
    }

    [Fact]
    public void DamageCompanion_KillsCompanion()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 10, defense: 0));
        bool died = sys.DamageCompanion("A", 100);
        Assert.True(died);
        Assert.False(sys.Party[0].IsAlive);
    }

    [Fact]
    public void DamageCompanion_NonExistentReturnsFalse()
    {
        var sys = CreateSystem();
        Assert.False(sys.DamageCompanion("nobody", 10));
    }

    [Fact]
    public void HealCompanion_HealsUp()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 50, maxHp: 100));
        sys.HealCompanion("A", 30);
        Assert.Equal(80, sys.Party[0].Hp);
    }

    [Fact]
    public void HealCompanion_CapsAtMax()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 90, maxHp: 100));
        sys.HealCompanion("A", 50);
        Assert.Equal(100, sys.Party[0].Hp);
    }

    [Fact]
    public void HealCompanion_DoesNotHealDead()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 10, defense: 0));
        sys.DamageCompanion("A", 100);
        sys.HealCompanion("A", 50);
        Assert.Equal(0, sys.Party[0].Hp);
    }

    [Fact]
    public void RemoveDeadCompanions_RemovesDead()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 10, defense: 0));
        sys.AddCompanion(MakeCompanion("B", hp: 100));
        sys.DamageCompanion("A", 100);
        var dead = sys.RemoveDeadCompanions();
        Assert.Single(dead);
        Assert.Equal("A", dead[0]);
        Assert.Single(sys.Party);
    }

    [Fact]
    public void AliveCount_CountsCorrectly()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A", hp: 10, defense: 0));
        sys.AddCompanion(MakeCompanion("B", hp: 100));
        sys.DamageCompanion("A", 100);
        Assert.Equal(1, sys.AliveCount);
    }

    [Fact]
    public void Reset_ClearsParty()
    {
        var sys = CreateSystem();
        sys.AddCompanion(MakeCompanion("A"));
        sys.AddCompanion(MakeCompanion("B"));
        sys.Reset();
        Assert.Empty(sys.Party);
    }
}

#endregion

#region 3. GrowthSystem Tests

public class Phase6Expansion_GrowthSystemTests
{
    [Theory]
    [InlineData(Race.Human)]
    [InlineData(Race.Elf)]
    [InlineData(Race.Dwarf)]
    [InlineData(Race.Orc)]
    [InlineData(Race.Beastfolk)]
    [InlineData(Race.Halfling)]
    [InlineData(Race.Undead)]
    [InlineData(Race.Demon)]
    [InlineData(Race.FallenAngel)]
    [InlineData(Race.Slime)]
    public void GetRaceGrowthRate_ReturnsForAllRaces(Race race)
    {
        var rate = GrowthSystem.GetRaceGrowthRate(race);
        Assert.NotNull(rate);
        Assert.True(rate.Strength >= 0);
        Assert.True(rate.HpPerLevel > 0);
    }

    [Theory]
    [InlineData(CharacterClass.Fighter)]
    [InlineData(CharacterClass.Knight)]
    [InlineData(CharacterClass.Thief)]
    [InlineData(CharacterClass.Ranger)]
    [InlineData(CharacterClass.Mage)]
    [InlineData(CharacterClass.Cleric)]
    [InlineData(CharacterClass.Monk)]
    [InlineData(CharacterClass.Bard)]
    [InlineData(CharacterClass.Alchemist)]
    [InlineData(CharacterClass.Necromancer)]
    public void GetClassGrowthRate_ReturnsForAllClasses(CharacterClass cls)
    {
        var rate = GrowthSystem.GetClassGrowthRate(cls);
        Assert.NotNull(rate);
        Assert.True(rate.HpPerLevel > 0);
    }

    [Fact]
    public void GetRaceGrowthRate_HumanIsBalanced()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        Assert.Equal(0.5, rate.Strength);
        Assert.Equal(0.5, rate.Vitality);
        Assert.Equal(0.5, rate.Agility);
        Assert.Equal(5.0, rate.HpPerLevel);
        Assert.Equal(3.0, rate.MpPerLevel);
    }

    [Fact]
    public void GetRaceGrowthRate_OrcHighStrength()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Orc);
        Assert.Equal(0.9, rate.Strength);
        Assert.Equal(8.0, rate.HpPerLevel);
    }

    [Fact]
    public void GetRaceGrowthRate_ElfHighIntelligence()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Elf);
        Assert.Equal(0.8, rate.Intelligence);
        Assert.Equal(6.0, rate.MpPerLevel);
    }

    [Fact]
    public void GetClassGrowthRate_FighterHighStrength()
    {
        var rate = GrowthSystem.GetClassGrowthRate(CharacterClass.Fighter);
        Assert.Equal(0.8, rate.Strength);
        Assert.Equal(8.0, rate.HpPerLevel);
    }

    [Fact]
    public void GetClassGrowthRate_MageHighIntelligence()
    {
        var rate = GrowthSystem.GetClassGrowthRate(CharacterClass.Mage);
        Assert.Equal(0.9, rate.Intelligence);
        Assert.Equal(10.0, rate.MpPerLevel);
    }

    [Fact]
    public void CalculateLevelUpBonus_ReturnsStatModifier()
    {
        var bonus = GrowthSystem.CalculateLevelUpBonus(Race.Human, CharacterClass.Fighter, 2);
        Assert.IsType<StatModifier>(bonus);
    }

    [Fact]
    public void CalculateHpGrowth_AtLeast1()
    {
        foreach (Race race in Enum.GetValues<Race>())
        foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
        {
            int hp = GrowthSystem.CalculateHpGrowth(race, cls);
            Assert.True(hp >= 1, $"HP growth for {race}/{cls} = {hp}");
        }
    }

    [Fact]
    public void CalculateMpGrowth_AtLeast0()
    {
        foreach (Race race in Enum.GetValues<Race>())
        foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
        {
            int mp = GrowthSystem.CalculateMpGrowth(race, cls);
            Assert.True(mp >= 0, $"MP growth for {race}/{cls} = {mp}");
        }
    }

    [Fact]
    public void RollGrowthWithRandom_DeterministicWithSeed()
    {
        var r1 = new Random(42);
        var r2 = new Random(42);
        int v1 = GrowthSystem.RollGrowthWithRandom(1.3, r1);
        int v2 = GrowthSystem.RollGrowthWithRandom(1.3, r2);
        Assert.Equal(v1, v2);
    }

    [Fact]
    public void RollGrowthWithRandom_GuaranteedPortion()
    {
        var rng = new Random(0);
        int result = GrowthSystem.RollGrowthWithRandom(2.0, rng);
        Assert.Equal(2, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    public void CalculateTotalExpForLevel_PositiveAndIncreasing(int level)
    {
        if (level > 1)
        {
            int exp = GrowthSystem.CalculateTotalExpForLevel(level);
            Assert.True(exp > 0);
            int prevExp = GrowthSystem.CalculateTotalExpForLevel(level - 1);
            Assert.True(exp >= prevExp);
        }
        else
        {
            int exp = GrowthSystem.CalculateTotalExpForLevel(level);
            Assert.Equal(0, exp);
        }
    }

    [Fact]
    public void CalculateMaxHp_ReturnsPositive()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int hp = GrowthSystem.CalculateMaxHp(baseStats, Race.Human, CharacterClass.Fighter, 1);
        Assert.True(hp > 0);
    }

    [Fact]
    public void CalculateMaxMp_ReturnsPositive()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int mp = GrowthSystem.CalculateMaxMp(baseStats, Race.Human, CharacterClass.Mage, 1);
        Assert.True(mp > 0);
    }

    [Fact]
    public void CalculateMaxHp_IncreasesWithLevel()
    {
        var baseStats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int hp1 = GrowthSystem.CalculateMaxHp(baseStats, Race.Human, CharacterClass.Fighter, 1);
        int hp10 = GrowthSystem.CalculateMaxHp(baseStats, Race.Human, CharacterClass.Fighter, 10);
        Assert.True(hp10 > hp1);
    }

    [Fact]
    public void GrowthRate_GetLevelBonus_Level1IsZero()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        var bonus = rate.GetLevelBonus(1);
        Assert.Equal(0, bonus.Strength);
    }

    [Fact]
    public void GrowthRate_GetLevelBonus_Level10()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        var bonus = rate.GetLevelBonus(10);
        Assert.Equal((int)(0.5 * 9), bonus.Strength);
    }

    [Fact]
    public void GrowthRate_GetHpBonus_Level1IsZero()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        Assert.Equal(0, rate.GetHpBonus(1));
    }

    [Fact]
    public void GrowthRate_GetHpBonus_Level10()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        Assert.Equal((int)(5.0 * 9), rate.GetHpBonus(10));
    }

    [Fact]
    public void GrowthRate_GetMpBonus_Level10()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        Assert.Equal((int)(3.0 * 9), rate.GetMpBonus(10));
    }
}

#endregion

#region 4. DeathLogSystem Tests

public class Phase6Expansion_DeathLogSystemTests
{
    private DeathLogSystem CreateSystem() => new();

    private DeathLogSystem.DeathLogEntry MakeEntry(
        int run = 1, string name = "Test", CharacterClass cls = CharacterClass.Fighter,
        Race race = Race.Human, int level = 5, DeathCause cause = DeathCause.Combat,
        string detail = "goblin", string location = "B1F", int floor = 1,
        int turns = 100, DateTime? ts = null)
        => new(run, name, cls, race, level, cause, detail, location, floor, turns, ts ?? DateTime.Now);

    [Fact]
    public void AddLog_IncrementsTotalDeaths()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry());
        Assert.Equal(1, sys.TotalDeaths);
    }

    [Fact]
    public void TotalDeaths_EmptyIsZero()
    {
        var sys = CreateSystem();
        Assert.Equal(0, sys.TotalDeaths);
    }

    [Fact]
    public void GetDeathsByCategory_GroupsCorrectly()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(cause: DeathCause.Combat));
        sys.AddLog(MakeEntry(cause: DeathCause.Combat));
        sys.AddLog(MakeEntry(cause: DeathCause.Starvation));
        var grouped = sys.GetDeathsByCategory();
        Assert.Equal(2, grouped[DeathCause.Combat]);
        Assert.Equal(1, grouped[DeathCause.Starvation]);
    }

    [Fact]
    public void GetMostCommonCause_ReturnsCorrect()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(cause: DeathCause.Trap));
        sys.AddLog(MakeEntry(cause: DeathCause.Trap));
        sys.AddLog(MakeEntry(cause: DeathCause.Combat));
        Assert.Equal(DeathCause.Trap, sys.GetMostCommonCause());
    }

    [Fact]
    public void GetMostCommonCause_EmptyReturnsNull()
    {
        var sys = CreateSystem();
        Assert.Null(sys.GetMostCommonCause());
    }

    [Fact]
    public void GetHighestLevel_ReturnsMax()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(level: 5));
        sys.AddLog(MakeEntry(level: 15));
        sys.AddLog(MakeEntry(level: 10));
        Assert.Equal(15, sys.GetHighestLevel());
    }

    [Fact]
    public void GetHighestLevel_EmptyReturns0()
    {
        Assert.Equal(0, CreateSystem().GetHighestLevel());
    }

    [Fact]
    public void GetDeepestFloor_ReturnsMax()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(floor: 3));
        sys.AddLog(MakeEntry(floor: 10));
        Assert.Equal(10, sys.GetDeepestFloor());
    }

    [Fact]
    public void GetDeepestFloor_EmptyReturns0()
    {
        Assert.Equal(0, CreateSystem().GetDeepestFloor());
    }

    [Fact]
    public void GetAverageSurvivalTurns_ReturnsAverage()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(turns: 100));
        sys.AddLog(MakeEntry(turns: 200));
        Assert.Equal(150.0, sys.GetAverageSurvivalTurns());
    }

    [Fact]
    public void GetAverageSurvivalTurns_EmptyReturns0()
    {
        Assert.Equal(0, CreateSystem().GetAverageSurvivalTurns());
    }

    [Fact]
    public void GetDeathsByClass_GroupsCorrectly()
    {
        var sys = CreateSystem();
        sys.AddLog(MakeEntry(cls: CharacterClass.Mage));
        sys.AddLog(MakeEntry(cls: CharacterClass.Mage));
        sys.AddLog(MakeEntry(cls: CharacterClass.Fighter));
        var grouped = sys.GetDeathsByClass();
        Assert.Equal(2, grouped[CharacterClass.Mage]);
        Assert.Equal(1, grouped[CharacterClass.Fighter]);
    }

    [Fact]
    public void GetRecentLogs_ReturnsLatestN()
    {
        var sys = CreateSystem();
        var ts1 = new DateTime(2024, 1, 1);
        var ts2 = new DateTime(2024, 2, 1);
        var ts3 = new DateTime(2024, 3, 1);
        sys.AddLog(MakeEntry(run: 1, ts: ts1));
        sys.AddLog(MakeEntry(run: 2, ts: ts2));
        sys.AddLog(MakeEntry(run: 3, ts: ts3));
        var recent = sys.GetRecentLogs(2);
        Assert.Equal(2, recent.Count);
        Assert.Equal(3, recent[0].RunNumber);
        Assert.Equal(2, recent[1].RunNumber);
    }
}

#endregion

#region 5. EncyclopediaSystem Tests

public class Phase6Expansion_EncyclopediaSystemTests
{
    private EncyclopediaSystem CreateSystem()
    {
        var sys = new EncyclopediaSystem();
        sys.RegisterEntry(EncyclopediaCategory.Monster, "goblin", "ゴブリン", 3,
            new Dictionary<int, string> { [1] = "小型の魔物", [2] = "群れで行動する", [3] = "弱点は火" });
        return sys;
    }

    [Fact]
    public void RegisterEntry_Registers()
    {
        var sys = CreateSystem();
        Assert.Equal(1, sys.TotalEntries);
    }

    [Fact]
    public void IncrementDiscovery_IncreasesLevel()
    {
        var sys = CreateSystem();
        Assert.True(sys.IncrementDiscovery("goblin"));
        Assert.Equal(1, sys.GetEntry("goblin")!.DiscoveryLevel);
    }

    [Fact]
    public void IncrementDiscovery_ReturnsFalseAtMax()
    {
        var sys = CreateSystem();
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        Assert.False(sys.IncrementDiscovery("goblin"));
    }

    [Fact]
    public void IncrementDiscovery_UnknownReturnsFalse()
    {
        var sys = CreateSystem();
        Assert.False(sys.IncrementDiscovery("unknown"));
    }

    [Fact]
    public void GetEntry_ReturnsEntry()
    {
        var sys = CreateSystem();
        var entry = sys.GetEntry("goblin");
        Assert.NotNull(entry);
        Assert.Equal("ゴブリン", entry.Name);
    }

    [Fact]
    public void GetEntry_UnknownReturnsNull()
    {
        var sys = CreateSystem();
        Assert.Null(sys.GetEntry("unknown"));
    }

    [Fact]
    public void GetByCategory_FiltersCorrectly()
    {
        var sys = CreateSystem();
        sys.RegisterEntry(EncyclopediaCategory.Item, "potion", "ポーション", 1, new Dictionary<int, string> { [1] = "回復薬" });
        var monsters = sys.GetByCategory(EncyclopediaCategory.Monster);
        Assert.Single(monsters);
        Assert.Equal("ゴブリン", monsters[0].Name);
    }

    [Fact]
    public void GetDiscoveryRate_CorrectRatio()
    {
        var sys = CreateSystem();
        sys.RegisterEntry(EncyclopediaCategory.Monster, "orc", "オーク", 2, new Dictionary<int, string> { [1] = "大型の魔物" });
        sys.IncrementDiscovery("goblin");
        float rate = sys.GetDiscoveryRate(EncyclopediaCategory.Monster);
        Assert.Equal(0.5f, rate);
    }

    [Fact]
    public void GetDiscoveryRate_EmptyCategory()
    {
        var sys = CreateSystem();
        Assert.Equal(0f, sys.GetDiscoveryRate(EncyclopediaCategory.Region));
    }

    [Fact]
    public void GetCompletionRate_CorrectRatio()
    {
        var sys = CreateSystem();
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        Assert.Equal(1.0f, sys.GetCompletionRate(EncyclopediaCategory.Monster));
    }

    [Fact]
    public void DiscoveredEntries_CountsCorrectly()
    {
        var sys = CreateSystem();
        Assert.Equal(0, sys.DiscoveredEntries);
        sys.IncrementDiscovery("goblin");
        Assert.Equal(1, sys.DiscoveredEntries);
    }

    [Fact]
    public void GetCurrentDescription_Level0ReturnsQuestionMarks()
    {
        var sys = CreateSystem();
        Assert.Equal("???", sys.GetCurrentDescription("goblin"));
    }

    [Fact]
    public void GetCurrentDescription_AccumulatesDescriptions()
    {
        var sys = CreateSystem();
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        var desc = sys.GetCurrentDescription("goblin");
        Assert.Contains("小型の魔物", desc);
        Assert.Contains("群れで行動する", desc);
    }

    [Fact]
    public void ResetDiscoveryLevels_ResetsAll()
    {
        var sys = CreateSystem();
        sys.IncrementDiscovery("goblin");
        sys.IncrementDiscovery("goblin");
        sys.ResetDiscoveryLevels();
        Assert.Equal(0, sys.GetEntry("goblin")!.DiscoveryLevel);
        Assert.Equal(0, sys.DiscoveredEntries);
    }

    [Fact]
    public void ResetDiscoveryLevels_PreservesEntries()
    {
        var sys = CreateSystem();
        sys.IncrementDiscovery("goblin");
        sys.ResetDiscoveryLevels();
        Assert.Equal(1, sys.TotalEntries);
        Assert.NotNull(sys.GetEntry("goblin"));
    }
}

#endregion

#region 6. DiseaseSystem Tests

public class Phase6Expansion_DiseaseSystemTests
{
    [Theory]
    [InlineData(DiseaseType.Cold)]
    [InlineData(DiseaseType.Infection)]
    [InlineData(DiseaseType.FoodPoisoning)]
    [InlineData(DiseaseType.Miasma)]
    [InlineData(DiseaseType.CursePlague)]
    public void GetDisease_ReturnsForAllTypes(DiseaseType type)
    {
        var disease = DiseaseSystem.GetDisease(type);
        Assert.NotNull(disease);
        Assert.Equal(type, disease.Type);
        Assert.NotEmpty(disease.Name);
        Assert.NotEmpty(disease.Description);
    }

    [Fact]
    public void GetAllDiseases_Returns5()
    {
        Assert.Equal(5, DiseaseSystem.GetAllDiseases().Count);
    }

    [Fact]
    public void Cold_IsSelfHealing()
    {
        Assert.True(DiseaseSystem.GetDisease(DiseaseType.Cold)!.SelfHealing);
    }

    [Fact]
    public void FoodPoisoning_IsSelfHealing()
    {
        Assert.True(DiseaseSystem.GetDisease(DiseaseType.FoodPoisoning)!.SelfHealing);
    }

    [Fact]
    public void Infection_IsNotSelfHealing()
    {
        Assert.False(DiseaseSystem.GetDisease(DiseaseType.Infection)!.SelfHealing);
    }

    [Fact]
    public void Miasma_IsNotSelfHealing()
    {
        Assert.False(DiseaseSystem.GetDisease(DiseaseType.Miasma)!.SelfHealing);
    }

    [Fact]
    public void CursePlague_IsNotSelfHealing()
    {
        Assert.False(DiseaseSystem.GetDisease(DiseaseType.CursePlague)!.SelfHealing);
    }

    [Fact]
    public void CheckInfection_HigherChanceWithWound()
    {
        // With wound: 0.2 - vit*0.005; without: 0.05 - vit*0.005
        // vit=0: wound=0.2, no wound=0.05
        bool withWound = DiseaseSystem.CheckInfection(true, 0, 0.1);
        bool withoutWound = DiseaseSystem.CheckInfection(false, 0, 0.1);
        Assert.True(withWound);
        Assert.False(withoutWound);
    }

    [Fact]
    public void CheckInfection_LowerChanceWithHighVitality()
    {
        // vit=30: chance = 0.2 - 0.15 = 0.05, random 0.04 should pass
        Assert.True(DiseaseSystem.CheckInfection(true, 30, 0.04));
        // vit=30: chance = 0.05, random 0.06 should fail
        Assert.False(DiseaseSystem.CheckInfection(true, 30, 0.06));
    }

    [Theory]
    [InlineData(DiseaseType.Cold, 50)]
    [InlineData(DiseaseType.FoodPoisoning, 80)]
    [InlineData(DiseaseType.Infection, 200)]
    [InlineData(DiseaseType.Miasma, 500)]
    [InlineData(DiseaseType.CursePlague, 1000)]
    public void CalculateTreatmentCost_CorrectValues(DiseaseType type, int expected)
    {
        Assert.Equal(expected, DiseaseSystem.CalculateTreatmentCost(type));
    }
}

#endregion

#region 7. EnvironmentalCombatSystem Tests

public class Phase6Expansion_EnvironmentalCombatSystemTests
{
    [Fact]
    public void GetInteraction_WaterLightning_Electrified()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Water, Element.Lightning);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Electrified, result.ResultSurface);
        Assert.Equal(1.5f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_WaterIce_Ice()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Water, Element.Ice);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Ice, result.ResultSurface);
    }

    [Fact]
    public void GetInteraction_WaterFire_Normal()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Water, Element.Fire);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Normal, result.ResultSurface);
        Assert.Equal(0.5f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_OilFire_HighDamage()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Oil, Element.Fire);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Fire, result.ResultSurface);
        Assert.Equal(2.0f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_IceFire_Water()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Ice, Element.Fire);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Water, result.ResultSurface);
    }

    [Fact]
    public void GetInteraction_PoisonFire_Fire()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Poison, Element.Fire);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Fire, result.ResultSurface);
        Assert.Equal(1.3f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_FireWater_Extinguish()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Fire, Element.Water);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Normal, result.ResultSurface);
        Assert.Equal(0.0f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_FireIce_Water()
    {
        var result = EnvironmentalCombatSystem.GetInteraction(SurfaceType.Fire, Element.Ice);
        Assert.NotNull(result);
        Assert.Equal(SurfaceType.Water, result.ResultSurface);
        Assert.Equal(0.5f, result.DamageMultiplier);
    }

    [Fact]
    public void GetInteraction_UndefinedReturnsNull()
    {
        Assert.Null(EnvironmentalCombatSystem.GetInteraction(SurfaceType.Normal, Element.Fire));
    }

    [Theory]
    [InlineData(SurfaceType.Ice, 1.5f)]
    [InlineData(SurfaceType.Water, 1.2f)]
    [InlineData(SurfaceType.Oil, 1.3f)]
    [InlineData(SurfaceType.Fire, 1.0f)]
    [InlineData(SurfaceType.Poison, 1.1f)]
    [InlineData(SurfaceType.Normal, 1.0f)]
    [InlineData(SurfaceType.Electrified, 1.0f)]
    public void GetMovementModifier_CorrectValues(SurfaceType surface, float expected)
    {
        Assert.Equal(expected, EnvironmentalCombatSystem.GetMovementModifier(surface));
    }

    [Theory]
    [InlineData(SurfaceType.Fire, 5)]
    [InlineData(SurfaceType.Poison, 3)]
    [InlineData(SurfaceType.Electrified, 4)]
    [InlineData(SurfaceType.Normal, 0)]
    [InlineData(SurfaceType.Water, 0)]
    [InlineData(SurfaceType.Oil, 0)]
    [InlineData(SurfaceType.Ice, 0)]
    public void GetSurfaceDamage_CorrectValues(SurfaceType surface, int expected)
    {
        Assert.Equal(expected, EnvironmentalCombatSystem.GetSurfaceDamage(surface));
    }

    [Theory]
    [InlineData(SurfaceType.Water, 999)]
    [InlineData(SurfaceType.Oil, 30)]
    [InlineData(SurfaceType.Ice, 20)]
    [InlineData(SurfaceType.Poison, 15)]
    [InlineData(SurfaceType.Fire, 8)]
    [InlineData(SurfaceType.Electrified, 5)]
    [InlineData(SurfaceType.Normal, 5)]  // 不明な地表面: デフォルト5ターン
    public void GetSurfaceDuration_CorrectValues(SurfaceType surface, int expected)
    {
        Assert.Equal(expected, EnvironmentalCombatSystem.GetSurfaceDuration(surface));
    }
}

#endregion

#region 8. ExecutionSystem Tests

public class Phase6Expansion_ExecutionSystemTests
{
    [Fact]
    public void CanExecute_TrueWhenAtThreshold()
    {
        Assert.True(ExecutionSystem.CanExecute(10, 100));
    }

    [Fact]
    public void CanExecute_TrueWhenBelow()
    {
        Assert.True(ExecutionSystem.CanExecute(5, 100));
    }

    [Fact]
    public void CanExecute_FalseWhenAbove()
    {
        Assert.False(ExecutionSystem.CanExecute(11, 100));
    }

    [Fact]
    public void CanExecute_FalseWhenMaxHpZero()
    {
        Assert.False(ExecutionSystem.CanExecute(0, 0));
    }

    [Fact]
    public void CanExecute_TrueAtZeroHp()
    {
        Assert.True(ExecutionSystem.CanExecute(0, 100));
    }

    [Fact]
    public void GetExecutionExpBonus_Returns1_5()
    {
        Assert.Equal(1.5f, ExecutionSystem.GetExecutionExpBonus());
    }

    [Fact]
    public void GetExecutionDropBonus_Returns1_3()
    {
        Assert.Equal(1.3f, ExecutionSystem.GetExecutionDropBonus());
    }

    [Fact]
    public void GetMercyKarmaBonus_Returns5()
    {
        Assert.Equal(5, ExecutionSystem.GetMercyKarmaBonus());
    }

    [Theory]
    [InlineData(MonsterRace.Humanoid, -5)]
    [InlineData(MonsterRace.Beast, -1)]
    [InlineData(MonsterRace.Undead, 0)]
    [InlineData(MonsterRace.Demon, 2)]
    [InlineData(MonsterRace.Dragon, -2)]
    public void GetExecutionKarmaPenalty_CorrectByRace(MonsterRace race, int expected)
    {
        Assert.Equal(expected, ExecutionSystem.GetExecutionKarmaPenalty(race));
    }

    [Theory]
    [InlineData(WeaponType.Sword, "斬首")]
    [InlineData(WeaponType.Dagger, "暗殺")]
    [InlineData(WeaponType.Axe, "両断")]
    [InlineData(WeaponType.Hammer, "粉砕")]
    [InlineData(WeaponType.Spear, "貫通")]
    [InlineData(WeaponType.Bow, "射殺")]
    [InlineData(WeaponType.Staff, "魔力爆砕")]  // DP-1: 追加
    public void GetExecutionAnimationName_CorrectByWeapon(WeaponType weapon, string expected)
    {
        Assert.Equal(expected, ExecutionSystem.GetExecutionAnimationName(weapon));
    }
}

#endregion

#region 9. NpcRoutineSystem Tests

public class Phase6Expansion_NpcRoutineSystemTests
{
    [Theory]
    [InlineData("Merchant", TimePeriod.Dawn, NpcActivity.Working)]
    [InlineData("Merchant", TimePeriod.Morning, NpcActivity.Working)]
    [InlineData("Merchant", TimePeriod.Afternoon, NpcActivity.Working)]
    [InlineData("Merchant", TimePeriod.Dusk, NpcActivity.Resting)]
    [InlineData("Merchant", TimePeriod.Night, NpcActivity.Sleeping)]
    [InlineData("Merchant", TimePeriod.Midnight, NpcActivity.Sleeping)]
    public void GetRoutine_MerchantSchedule(string npc, TimePeriod time, NpcActivity expected)
    {
        var routine = NpcRoutineSystem.GetRoutine(npc, time);
        Assert.NotNull(routine);
        Assert.Equal(expected, routine.Activity);
    }

    [Theory]
    [InlineData("Guard", TimePeriod.Dawn, NpcActivity.Patrolling)]
    [InlineData("Guard", TimePeriod.Morning, NpcActivity.Patrolling)]
    [InlineData("Guard", TimePeriod.Afternoon, NpcActivity.Patrolling)]
    [InlineData("Guard", TimePeriod.Dusk, NpcActivity.Patrolling)]
    [InlineData("Guard", TimePeriod.Night, NpcActivity.Patrolling)]
    [InlineData("Guard", TimePeriod.Midnight, NpcActivity.Resting)]
    public void GetRoutine_GuardSchedule(string npc, TimePeriod time, NpcActivity expected)
    {
        var routine = NpcRoutineSystem.GetRoutine(npc, time);
        Assert.NotNull(routine);
        Assert.Equal(expected, routine.Activity);
    }

    [Theory]
    [InlineData("Priest", TimePeriod.Dawn, NpcActivity.Praying)]
    [InlineData("Priest", TimePeriod.Morning, NpcActivity.Working)]
    [InlineData("Priest", TimePeriod.Afternoon, NpcActivity.Working)]
    [InlineData("Priest", TimePeriod.Dusk, NpcActivity.Praying)]
    [InlineData("Priest", TimePeriod.Night, NpcActivity.Sleeping)]
    [InlineData("Priest", TimePeriod.Midnight, NpcActivity.Sleeping)]
    public void GetRoutine_PriestSchedule(string npc, TimePeriod time, NpcActivity expected)
    {
        var routine = NpcRoutineSystem.GetRoutine(npc, time);
        Assert.NotNull(routine);
        Assert.Equal(expected, routine.Activity);
    }

    [Theory]
    [InlineData("Adventurer", TimePeriod.Dawn, NpcActivity.Sleeping)]
    [InlineData("Adventurer", TimePeriod.Morning, NpcActivity.Shopping)]
    [InlineData("Adventurer", TimePeriod.Afternoon, NpcActivity.Working)]
    [InlineData("Adventurer", TimePeriod.Dusk, NpcActivity.Drinking)]
    [InlineData("Adventurer", TimePeriod.Night, NpcActivity.Drinking)]
    [InlineData("Adventurer", TimePeriod.Midnight, NpcActivity.Sleeping)]
    public void GetRoutine_AdventurerSchedule(string npc, TimePeriod time, NpcActivity expected)
    {
        var routine = NpcRoutineSystem.GetRoutine(npc, time);
        Assert.NotNull(routine);
        Assert.Equal(expected, routine.Activity);
    }

    [Fact]
    public void GetRoutine_UnknownNpcReturnsNull()
    {
        Assert.Null(NpcRoutineSystem.GetRoutine("Unknown", TimePeriod.Morning));
    }

    [Fact]
    public void GetNpcsAtLocation_Shop_Morning()
    {
        var npcs = NpcRoutineSystem.GetNpcsAtLocation("ショップ", TimePeriod.Morning);
        Assert.Contains("Merchant", npcs);
        Assert.Contains("Adventurer", npcs);
    }

    [Fact]
    public void GetNpcsAtLocation_Tavern_Dusk()
    {
        var npcs = NpcRoutineSystem.GetNpcsAtLocation("酒場", TimePeriod.Dusk);
        Assert.Contains("Merchant", npcs);
        Assert.Contains("Adventurer", npcs);
    }

    [Theory]
    [InlineData("Merchant", TimePeriod.Morning, true)]
    [InlineData("Merchant", TimePeriod.Night, false)]
    [InlineData("Priest", TimePeriod.Morning, true)]
    [InlineData("Priest", TimePeriod.Night, false)]
    [InlineData("Guard", TimePeriod.Morning, false)]
    public void IsNpcAvailable_WorkingState(string npc, TimePeriod time, bool expected)
    {
        Assert.Equal(expected, NpcRoutineSystem.IsNpcAvailable(npc, time));
    }

    [Fact]
    public void GetAllRoutines_Returns24()
    {
        Assert.Equal(24, NpcRoutineSystem.GetAllRoutines().Count);
    }

    [Theory]
    [InlineData(NpcActivity.Working, "仕事中")]
    [InlineData(NpcActivity.Shopping, "買い物中")]
    [InlineData(NpcActivity.Resting, "休憩中")]
    [InlineData(NpcActivity.Patrolling, "巡回中")]
    [InlineData(NpcActivity.Praying, "祈祷中")]
    [InlineData(NpcActivity.Drinking, "飲酒中")]
    [InlineData(NpcActivity.Sleeping, "睡眠中")]
    public void GetActivityName_ReturnsJapanese(NpcActivity activity, string expected)
    {
        Assert.Equal(expected, NpcRoutineSystem.GetActivityName(activity));
    }
}

#endregion

#region 10. MultiClassSystem Tests

public class Phase6Expansion_MultiClassSystemTests
{
    [Fact]
    public void CanClassChange_FighterToKnight_MeetsRequirements()
    {
        var quests = new HashSet<string> { "knight_trial" };
        Assert.True(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 20, quests));
    }

    [Fact]
    public void CanClassChange_FighterToKnight_LevelTooLow()
    {
        var quests = new HashSet<string> { "knight_trial" };
        Assert.False(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 19, quests));
    }

    [Fact]
    public void CanClassChange_FighterToKnight_MissingQuest()
    {
        var quests = new HashSet<string>();
        Assert.False(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Knight, 20, quests));
    }

    [Fact]
    public void CanClassChange_UndefinedPath_ReturnsFalse()
    {
        var quests = new HashSet<string> { "anything" };
        Assert.False(MultiClassSystem.CanClassChange(CharacterClass.Fighter, CharacterClass.Mage, 99, quests));
    }

    [Theory]
    [InlineData(CharacterClass.Fighter, CharacterClass.Knight)]
    [InlineData(CharacterClass.Mage, CharacterClass.Necromancer)]
    [InlineData(CharacterClass.Thief, CharacterClass.Ranger)]
    [InlineData(CharacterClass.Cleric, CharacterClass.Monk)]
    [InlineData(CharacterClass.Bard, CharacterClass.Alchemist)]
    public void GetRequirement_DefinedPaths(CharacterClass from, CharacterClass to)
    {
        var req = MultiClassSystem.GetRequirement(from, to);
        Assert.NotNull(req);
        Assert.Equal(from, req.FromClass);
        Assert.Equal(to, req.ToClass);
        Assert.Equal(20, req.RequiredLevel);
    }

    [Fact]
    public void GetRequirement_UndefinedReturnsNull()
    {
        Assert.Null(MultiClassSystem.GetRequirement(CharacterClass.Fighter, CharacterClass.Mage));
    }

    [Fact]
    public void GetAvailableChanges_FighterHasKnight()
    {
        var changes = MultiClassSystem.GetAvailableChanges(CharacterClass.Fighter);
        Assert.Single(changes);
        Assert.Equal(CharacterClass.Knight, changes[0].ToClass);
    }

    [Fact]
    public void GetAvailableChanges_NoneForKnight()
    {
        var changes = MultiClassSystem.GetAvailableChanges(CharacterClass.Knight);
        Assert.NotEmpty(changes); // DR-1: Knight→Master(Fighter)への転職ルートが存在する
    }

    [Theory]
    [InlineData(ClassTier.Base, "基本職")]
    [InlineData(ClassTier.Advanced, "上位職")]
    [InlineData(ClassTier.Master, "最上位職")]
    public void GetTierName_ReturnsJapanese(ClassTier tier, string expected)
    {
        Assert.Equal(expected, MultiClassSystem.GetTierName(tier));
    }

    [Fact]
    public void GetSubclassExpRate_Returns0_5()
    {
        Assert.Equal(0.5f, MultiClassSystem.GetSubclassExpRate());
    }
}

#endregion

#region 11. SkillFusionSystem Tests

public class Phase6Expansion_SkillFusionSystemTests
{
    [Fact]
    public void FindRecipe_CorrectOrder()
    {
        var recipe = SkillFusionSystem.FindRecipe("strong_strike", "fireball");
        Assert.NotNull(recipe);
        Assert.Equal("flame_slash", recipe.ResultSkill);
    }

    [Fact]
    public void FindRecipe_ReverseOrder()
    {
        var recipe = SkillFusionSystem.FindRecipe("fireball", "strong_strike");
        Assert.NotNull(recipe);
        Assert.Equal("flame_slash", recipe.ResultSkill);
    }

    [Fact]
    public void FindRecipe_UnknownReturnsNull()
    {
        Assert.Null(SkillFusionSystem.FindRecipe("unknown_a", "unknown_b"));
    }

    [Fact]
    public void GetAllRecipes_Returns5()
    {
        Assert.Equal(5, SkillFusionSystem.GetAllRecipes().Count);
    }

    [Theory]
    [InlineData("strong_strike", "fireball", 30, true)]
    [InlineData("strong_strike", "fireball", 29, false)]
    [InlineData("heal", "poison_mist", 40, true)]
    [InlineData("heal", "poison_mist", 39, false)]
    [InlineData("shield_bash", "lightning_bolt", 50, true)]
    [InlineData("sneak", "backstab", 60, true)]
    [InlineData("meditation", "ki_strike", 70, true)]
    [InlineData("meditation", "ki_strike", 69, false)]
    public void CanFuse_ChecksProficiency(string a, string b, int prof, bool expected)
    {
        Assert.Equal(expected, SkillFusionSystem.CanFuse(a, b, prof));
    }

    [Fact]
    public void CanFuse_UnknownRecipe_ReturnsFalse()
    {
        Assert.False(SkillFusionSystem.CanFuse("unknown_a", "unknown_b", 999));
    }

    [Fact]
    public void ExecuteFusion_Success()
    {
        var result = SkillFusionSystem.ExecuteFusion("strong_strike", "fireball", 30);
        Assert.Equal("flame_slash", result);
    }

    [Fact]
    public void ExecuteFusion_InsufficientProficiency_ReturnsNull()
    {
        Assert.Null(SkillFusionSystem.ExecuteFusion("strong_strike", "fireball", 20));
    }

    [Fact]
    public void ExecuteFusion_UnknownRecipe_ReturnsNull()
    {
        Assert.Null(SkillFusionSystem.ExecuteFusion("unknown_a", "unknown_b", 99));
    }

    [Theory]
    [InlineData("strong_strike", "fireball", 30)]
    [InlineData("heal", "poison_mist", 40)]
    [InlineData("shield_bash", "lightning_bolt", 50)]
    [InlineData("sneak", "backstab", 60)]
    [InlineData("meditation", "ki_strike", 70)]
    public void GetRequiredProficiency_CorrectValues(string a, string b, int expected)
    {
        Assert.Equal(expected, SkillFusionSystem.GetRequiredProficiency(a, b));
    }

    [Fact]
    public void GetRequiredProficiency_Unknown_ReturnsMinus1()
    {
        Assert.Equal(-1, SkillFusionSystem.GetRequiredProficiency("unknown_a", "unknown_b"));
    }
}

#endregion

#region 12. SecretRoomSystem Tests

public class Phase6Expansion_SecretRoomSystemTests
{
    [Fact]
    public void CalculateDiscoveryChance_BaseCase()
    {
        float chance = SecretRoomSystem.CalculateDiscoveryChance(10, false);
        Assert.Equal(0.05f + 10 * 0.02f, chance);
    }

    [Fact]
    public void CalculateDiscoveryChance_WithEagleEye()
    {
        float chance = SecretRoomSystem.CalculateDiscoveryChance(10, true);
        Assert.Equal(0.05f + 10 * 0.02f + 0.2f, chance);
    }

    [Fact]
    public void CalculateDiscoveryChance_ClampedMin()
    {
        float chance = SecretRoomSystem.CalculateDiscoveryChance(0, false);
        Assert.Equal(0.05f, chance);
    }

    [Fact]
    public void CalculateDiscoveryChance_ClampedMax()
    {
        float chance = SecretRoomSystem.CalculateDiscoveryChance(100, true);
        Assert.Equal(0.8f, chance);
    }

    [Fact]
    public void CalculateSearchChance_BaseCase()
    {
        float chance = SecretRoomSystem.CalculateSearchChance(10, 5);
        float expected = Math.Clamp(0.3f + 10 * 0.03f + 5 * 0.05f, 0.3f, 0.95f);
        Assert.Equal(expected, chance);
    }

    [Fact]
    public void CalculateSearchChance_ClampedMin()
    {
        Assert.True(SecretRoomSystem.CalculateSearchChance(0, 0) >= 0.3f);
    }

    [Fact]
    public void CalculateSearchChance_ClampedMax()
    {
        Assert.Equal(0.95f, SecretRoomSystem.CalculateSearchChance(100, 100));
    }

    [Theory]
    [InlineData(DungeonFeatureType.Ruins, 1, 3)]
    [InlineData(DungeonFeatureType.Temple, 1, 2)]
    [InlineData(DungeonFeatureType.Crypt, 1, 2)]
    [InlineData(DungeonFeatureType.Cave, 1, 1)]
    public void CalculateSecretRoomCount_BaseValues(DungeonFeatureType type, int floor, int expectedBase)
    {
        int count = SecretRoomSystem.CalculateSecretRoomCount(floor, type);
        Assert.Equal(expectedBase + floor / 5, count);
    }

    [Fact]
    public void CalculateSecretRoomCount_IncreasesWithFloor()
    {
        int countF1 = SecretRoomSystem.CalculateSecretRoomCount(1, DungeonFeatureType.Ruins);
        int countF10 = SecretRoomSystem.CalculateSecretRoomCount(10, DungeonFeatureType.Ruins);
        Assert.True(countF10 > countF1);
    }

    [Theory]
    [InlineData(1, 1.1f)]
    [InlineData(5, 1.5f)]
    [InlineData(10, 2.0f)]
    public void GetRewardQualityMultiplier_Formula(int floor, float expected)
    {
        Assert.Equal(expected, SecretRoomSystem.GetRewardQualityMultiplier(floor));
    }

    [Fact]
    public void ShouldGenerateSecretPassage_Ruins_BelowThreshold()
    {
        Assert.True(SecretRoomSystem.ShouldGenerateSecretPassage(0.3, DungeonFeatureType.Ruins));
    }

    [Fact]
    public void ShouldGenerateSecretPassage_Ruins_AboveThreshold()
    {
        Assert.False(SecretRoomSystem.ShouldGenerateSecretPassage(0.5, DungeonFeatureType.Ruins));
    }

    [Fact]
    public void ShouldGenerateSecretPassage_Temple()
    {
        Assert.True(SecretRoomSystem.ShouldGenerateSecretPassage(0.2, DungeonFeatureType.Temple));
        Assert.False(SecretRoomSystem.ShouldGenerateSecretPassage(0.4, DungeonFeatureType.Temple));
    }

    [Fact]
    public void ShouldGenerateSecretPassage_Cave()
    {
        Assert.True(SecretRoomSystem.ShouldGenerateSecretPassage(0.1, DungeonFeatureType.Cave));
        Assert.False(SecretRoomSystem.ShouldGenerateSecretPassage(0.3, DungeonFeatureType.Cave));
    }
}

#endregion

#region 13. MimicSystem Tests

public class Phase6Expansion_MimicSystemTests
{
    [Fact]
    public void CalculateMimicSpawnRate_Floor1()
    {
        float rate = MimicSystem.CalculateMimicSpawnRate(1);
        Assert.Equal(0.02f + 1 * 0.005f, rate);
    }

    [Fact]
    public void CalculateMimicSpawnRate_ClampedMin()
    {
        Assert.True(MimicSystem.CalculateMimicSpawnRate(0) >= 0.02f);
    }

    [Fact]
    public void CalculateMimicSpawnRate_ClampedMax()
    {
        Assert.Equal(0.15f, MimicSystem.CalculateMimicSpawnRate(100));
    }

    [Fact]
    public void CalculateDetectionRate_BaseCase()
    {
        float rate = MimicSystem.CalculateDetectionRate(10, 50, false);
        Assert.Equal(0.1f + 10 * 0.03f, rate);
    }

    [Fact]
    public void CalculateDetectionRate_LowSanity()
    {
        float rate = MimicSystem.CalculateDetectionRate(10, 30, false);
        float expected = Math.Clamp(0.1f + 10 * 0.03f - 0.15f, 0.05f, 0.9f);
        Assert.Equal(expected, rate);
    }

    [Fact]
    public void CalculateDetectionRate_WithAppraisal()
    {
        float rate = MimicSystem.CalculateDetectionRate(10, 50, true);
        float expected = Math.Clamp(0.1f + 10 * 0.03f + 0.3f, 0.05f, 0.9f);
        Assert.Equal(expected, rate);
    }

    [Fact]
    public void CalculateDetectionRate_ClampedMin()
    {
        Assert.True(MimicSystem.CalculateDetectionRate(0, 10, false) >= 0.05f);
    }

    [Fact]
    public void CalculateDetectionRate_ClampedMax()
    {
        Assert.Equal(0.9f, MimicSystem.CalculateDetectionRate(100, 100, true));
    }

    [Theory]
    [InlineData(ItemGrade.Crude, 0.5f)]
    [InlineData(ItemGrade.Cheap, 0.8f)]
    [InlineData(ItemGrade.Standard, 1.0f)]
    [InlineData(ItemGrade.Fine, 1.3f)]
    [InlineData(ItemGrade.Superior, 1.6f)]
    [InlineData(ItemGrade.Masterwork, 2.0f)]
    public void GetMimicStrengthMultiplier_AllGrades(ItemGrade grade, float expected)
    {
        Assert.Equal(expected, MimicSystem.GetMimicStrengthMultiplier(grade));
    }

    [Fact]
    public void GetMimicRewardMultiplier_Returns1_5()
    {
        Assert.Equal(1.5f, MimicSystem.GetMimicRewardMultiplier());
    }

    [Fact]
    public void GetDisguiseTypes_Returns3()
    {
        var types = MimicSystem.GetDisguiseTypes();
        Assert.Equal(3, types.Count);
        Assert.Contains("宝箱", types);
        Assert.Contains("木箱", types);
        Assert.Contains("収納箱", types);
    }
}

#endregion

#region 14. MonsterRaceSystem Tests

public class Phase6Expansion_MonsterRaceSystemTests
{
    [Theory]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Humanoid)]
    [InlineData(MonsterRace.Amorphous)]
    [InlineData(MonsterRace.Undead)]
    [InlineData(MonsterRace.Demon)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Plant)]
    [InlineData(MonsterRace.Insect)]
    [InlineData(MonsterRace.Spirit)]
    [InlineData(MonsterRace.Construct)]
    public void GetTraits_ReturnsForAllRaces(MonsterRace race)
    {
        var traits = MonsterRaceSystem.GetTraits(race);
        Assert.NotNull(traits);
        Assert.Equal(race, traits.Race);
    }

    [Fact]
    public void GetTraits_UnknownReturnsHumanoid()
    {
        var traits = MonsterRaceSystem.GetTraits((MonsterRace)999);
        Assert.Equal(MonsterRace.Humanoid, traits.Race);
    }

    [Fact]
    public void GetTraits_BeastWeakToFire()
    {
        var traits = MonsterRaceSystem.GetTraits(MonsterRace.Beast);
        Assert.Contains(Element.Fire, traits.Weaknesses);
    }

    [Fact]
    public void GetTraits_UndeadWeakToLight()
    {
        var traits = MonsterRaceSystem.GetTraits(MonsterRace.Undead);
        Assert.Contains(Element.Light, traits.Weaknesses);
        Assert.Contains(Element.Holy, traits.Weaknesses);
    }

    [Fact]
    public void GetTraits_UndeadResistsPoisonDark()
    {
        var traits = MonsterRaceSystem.GetTraits(MonsterRace.Undead);
        Assert.Contains(Element.Poison, traits.Resistances);
        Assert.Contains(Element.Dark, traits.Resistances);
    }

    [Theory]
    [InlineData(MonsterRace.Beast, Element.Fire, 1.5f)]
    [InlineData(MonsterRace.Undead, Element.Light, 1.5f)]
    [InlineData(MonsterRace.Undead, Element.Poison, 0.5f)]
    [InlineData(MonsterRace.Undead, Element.Dark, 0.5f)]
    [InlineData(MonsterRace.Humanoid, Element.Fire, 1.0f)]
    [InlineData(MonsterRace.Demon, Element.Holy, 1.5f)]
    [InlineData(MonsterRace.Demon, Element.Dark, 0.5f)]
    [InlineData(MonsterRace.Dragon, Element.Fire, 0.5f)]
    [InlineData(MonsterRace.Plant, Element.Fire, 1.5f)]
    [InlineData(MonsterRace.Plant, Element.Earth, 0.5f)]
    public void GetElementalMultiplier_CorrectValues(MonsterRace race, Element element, float expected)
    {
        Assert.Equal(expected, MonsterRaceSystem.GetElementalMultiplier(race, element));
    }

    [Fact]
    public void IsStatusEffectImmune_UndeadImmuneToPoison()
    {
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Undead, StatusEffectType.Poison));
    }

    [Fact]
    public void IsStatusEffectImmune_UndeadImmuneToSleep()
    {
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Undead, StatusEffectType.Sleep));
    }

    [Fact]
    public void IsStatusEffectImmune_ConstructImmuneToMany()
    {
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Poison));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Sleep));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Confusion));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Fear));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Charm));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Blind));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Silence));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Madness));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.Paralysis));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Construct, StatusEffectType.InstantDeath));
    }

    [Fact]
    public void IsStatusEffectImmune_InsectImmuneToConfusionFearCharm()
    {
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Insect, StatusEffectType.Confusion));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Insect, StatusEffectType.Fear));
        Assert.True(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Insect, StatusEffectType.Charm));
    }

    [Fact]
    public void IsStatusEffectImmune_BeastNotImmuneToPoison()
    {
        Assert.False(MonsterRaceSystem.IsStatusEffectImmune(MonsterRace.Beast, StatusEffectType.Poison));
    }

    [Theory]
    [InlineData(MonsterRace.Amorphous, AttackType.Slash, 0.5f)]
    [InlineData(MonsterRace.Amorphous, AttackType.Pierce, 0.5f)]
    [InlineData(MonsterRace.Amorphous, AttackType.Blunt, 1.0f)]
    [InlineData(MonsterRace.Spirit, AttackType.Slash, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Pierce, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Blunt, 0.5f)]
    [InlineData(MonsterRace.Spirit, AttackType.Unarmed, 0.5f)]
    [InlineData(MonsterRace.Humanoid, AttackType.Slash, 1.0f)]
    public void GetPhysicalResistance_CorrectValues(MonsterRace race, AttackType attack, float expected)
    {
        Assert.Equal(expected, MonsterRaceSystem.GetPhysicalResistance(race, attack));
    }
}

#endregion

#region 15. EnvironmentalPuzzleSystem Tests

public class Phase6Expansion_EnvironmentalPuzzleSystemTests
{
    [Fact]
    public void GetAllPuzzles_HasEntries()
    {
        var puzzles = EnvironmentalPuzzleSystem.GetAllPuzzles();
        Assert.True(puzzles.Count > 0);
        Assert.Equal(5, puzzles.Count);
    }

    [Theory]
    [InlineData(PuzzleType.RuneLanguage, 2)]
    [InlineData(PuzzleType.Elemental, 1)]
    [InlineData(PuzzleType.Physical, 2)]
    public void GetByType_FiltersCorrectly(PuzzleType type, int expected)
    {
        var puzzles = EnvironmentalPuzzleSystem.GetByType(type);
        Assert.Equal(expected, puzzles.Count);
        Assert.All(puzzles, p => Assert.Equal(type, p.Type));
    }

    [Theory]
    [InlineData(PuzzleType.RuneLanguage, 15, 0, true)]
    [InlineData(PuzzleType.RuneLanguage, 10, 4, false)]
    [InlineData(PuzzleType.RuneLanguage, 10, 5, true)]
    [InlineData(PuzzleType.Elemental, 12, 0, true)]
    [InlineData(PuzzleType.Elemental, 11, 0, false)]
    [InlineData(PuzzleType.Physical, 8, 0, true)]
    [InlineData(PuzzleType.Physical, 7, 0, false)]
    public void CanAttempt_IntelligenceCheck(PuzzleType type, int intel, int knowledge, bool expected)
    {
        Assert.Equal(expected, EnvironmentalPuzzleSystem.CanAttempt(type, intel, knowledge));
    }

    [Fact]
    public void CalculateSuccessRate_BaseFormula()
    {
        float rate = EnvironmentalPuzzleSystem.CalculateSuccessRate(2, 15);
        float expected = Math.Clamp(0.5f + (15 - 2 * 5) * 0.05f, 0.1f, 0.95f);
        Assert.Equal(expected, rate);
    }

    [Fact]
    public void CalculateSuccessRate_ClampedMin()
    {
        float rate = EnvironmentalPuzzleSystem.CalculateSuccessRate(10, 1);
        Assert.Equal(0.1f, rate);
    }

    [Fact]
    public void CalculateSuccessRate_ClampedMax()
    {
        float rate = EnvironmentalPuzzleSystem.CalculateSuccessRate(1, 50);
        Assert.Equal(0.95f, rate);
    }

    [Theory]
    [InlineData(PuzzleType.RuneLanguage, "ルーン語パズル")]
    [InlineData(PuzzleType.Elemental, "属性パズル")]
    [InlineData(PuzzleType.Physical, "物理パズル")]
    public void GetTypeName_ReturnsJapanese(PuzzleType type, string expected)
    {
        Assert.Equal(expected, EnvironmentalPuzzleSystem.GetTypeName(type));
    }
}

#endregion

#region 16. ThirstSystem Tests

public class Phase6Expansion_ThirstSystemTests
{
    [Theory]
    [InlineData(ThirstLevel.Hydrated, 1.0f, 1.0f, 1.0f)]
    [InlineData(ThirstLevel.Thirsty, 0.9f, 0.9f, 0.95f)]
    [InlineData(ThirstLevel.Dehydrated, 0.7f, 0.7f, 0.8f)]
    [InlineData(ThirstLevel.SevereDehydration, 0.4f, 0.4f, 0.5f)]
    public void GetThirstModifiers_CorrectValues(ThirstLevel level, float str, float agi, float intel)
    {
        var (s, a, i) = ThirstSystem.GetThirstModifiers(level);
        Assert.Equal(str, s);
        Assert.Equal(agi, a);
        Assert.Equal(intel, i);
    }

    [Theory]
    [InlineData(WaterQuality.Pure, 0.0f)]
    [InlineData(WaterQuality.River, 0.05f)]
    [InlineData(WaterQuality.Muddy, 0.2f)]
    [InlineData(WaterQuality.Polluted, 0.5f)]
    public void GetInfectionRisk_IncreasingForWorseQuality(WaterQuality quality, float expected)
    {
        Assert.Equal(expected, ThirstSystem.GetInfectionRisk(quality));
    }

    [Theory]
    [InlineData(ThirstLevel.Hydrated, "潤い")]
    [InlineData(ThirstLevel.Thirsty, "渇き")]
    [InlineData(ThirstLevel.Dehydrated, "脱水")]
    [InlineData(ThirstLevel.SevereDehydration, "重度脱水")]
    public void GetThirstName_ReturnsJapanese(ThirstLevel level, string expected)
    {
        Assert.Equal(expected, ThirstSystem.GetThirstName(level));
    }

    [Theory]
    [InlineData(WaterQuality.Pure, "清水")]
    [InlineData(WaterQuality.River, "川水")]
    [InlineData(WaterQuality.Muddy, "泥水")]
    [InlineData(WaterQuality.Polluted, "汚水")]
    public void GetWaterQualityName_ReturnsJapanese(WaterQuality quality, string expected)
    {
        Assert.Equal(expected, ThirstSystem.GetWaterQualityName(quality));
    }

    [Theory]
    [InlineData(WaterQuality.Polluted, WaterQuality.Muddy)]
    [InlineData(WaterQuality.Muddy, WaterQuality.River)]
    [InlineData(WaterQuality.River, WaterQuality.Pure)]
    [InlineData(WaterQuality.Pure, WaterQuality.Pure)]
    public void Purify_ImprovesQuality(WaterQuality input, WaterQuality expected)
    {
        Assert.Equal(expected, ThirstSystem.Purify(input));
    }

    [Theory]
    [InlineData(ThirstLevel.Hydrated, 0)]
    [InlineData(ThirstLevel.Thirsty, 0)]
    [InlineData(ThirstLevel.Dehydrated, 1)]
    [InlineData(ThirstLevel.SevereDehydration, 2)]  // S-1: ThirstStageと統一（3→2）
    public void GetThirstDamage_CorrectValues(ThirstLevel level, int expected)
    {
        Assert.Equal(expected, ThirstSystem.GetThirstDamage(level));
    }
}

#endregion

#region 17. GamblingSystem Tests

public class Phase6Expansion_GamblingSystemTests
{
    [Theory]
    [InlineData(4, 4, true)]   // 大予想、結果大 → 勝ち
    [InlineData(4, 3, false)]  // 大予想、結果小 → 負け
    [InlineData(1, 2, true)]   // 小予想、結果小 → 勝ち
    [InlineData(1, 5, false)]  // 小予想、結果大 → 負け
    [InlineData(6, 6, true)]   // 大予想、結果大 → 勝ち
    public void JudgeDice_CorrectResult(int guess, int result, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeDice(guess, result));
    }

    [Theory]
    [InlineData(true, 2, 4, true)]
    [InlineData(true, 1, 3, true)]
    [InlineData(true, 1, 2, false)]
    [InlineData(false, 1, 2, true)]
    [InlineData(false, 2, 4, false)]
    public void JudgeChoHan_CorrectParity(bool cho, int d1, int d2, bool expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeChoHan(cho, d1, d2));
    }

    [Theory]
    [InlineData(true, 5, 7, true)]
    [InlineData(true, 5, 3, false)]
    [InlineData(false, 5, 3, true)]
    [InlineData(false, 5, 7, false)]
    [InlineData(true, 5, 5, null)]   // AG-2: 引き分けはnull（draw）
    [InlineData(false, 5, 5, null)]  // AG-2: 引き分けはnull（draw）
    public void JudgeHighLow_CorrectResult(bool high, int current, int next, bool? expected)
    {
        Assert.Equal(expected, GamblingSystem.JudgeHighLow(high, current, next));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice, 1.9f)]
    [InlineData(GamblingGameType.ChoHan, 1.9f)]
    [InlineData(GamblingGameType.Card, 1.9f)]
    public void GetPayoutMultiplier_CorrectValues(GamblingGameType type, float expected)
    {
        Assert.Equal(expected, GamblingSystem.GetPayoutMultiplier(type));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(0)]
    public void GetLuckBonus_IncreasesWithLuck(int luck)
    {
        float expected = Math.Min(luck * 0.005f, 0.05f);
        Assert.Equal(expected, GamblingSystem.GetLuckBonus(luck), 4);
    }

    [Theory]
    [InlineData(GamblingGameType.Dice, "サイコロ")]
    [InlineData(GamblingGameType.ChoHan, "丁半")]
    [InlineData(GamblingGameType.Card, "ハイ＆ロー")]
    public void GetGameName_ReturnsJapanese(GamblingGameType type, string expected)
    {
        Assert.Equal(expected, GamblingSystem.GetGameName(type));
    }

    [Theory]
    [InlineData(GamblingGameType.Dice, 50)]
    [InlineData(GamblingGameType.ChoHan, 10)]
    [InlineData(GamblingGameType.Card, 20)]
    public void GetMinimumBet_CorrectValues(GamblingGameType type, int expected)
    {
        Assert.Equal(expected, GamblingSystem.GetMinimumBet(type));
    }

    [Theory]
    [InlineData(10, 50, false)]
    [InlineData(1, 100, false)]
    [InlineData(7, 0, true)]
    public void CheckAddiction_Formula(int gambles, int sanity, bool expected)
    {
        // risk = gambles * 0.05 - sanity * 0.005 > 0.3 (DS-4: 正気度影響を0.001→0.005に修正)
        Assert.Equal(expected, GamblingSystem.CheckAddiction(gambles, sanity));
    }
}

#endregion

#region 18. RestSystem Tests

public class Phase6Expansion_RestSystemTests
{
    [Theory]
    [InlineData(SleepQuality.DeepSleep, 0.5f, 0.5f, 0.8f, 0.2f)]
    [InlineData(SleepQuality.Normal, 0.3f, 0.3f, 0.6f, 0.1f)]
    [InlineData(SleepQuality.Light, 0.15f, 0.15f, 0.3f, 0.05f)]
    [InlineData(SleepQuality.Nap, 0.05f, 0.1f, 0.15f, 0.0f)]
    public void GetRecoveryRates_CorrectValues(SleepQuality quality, float hp, float mp, float fatigue, float sanity)
    {
        var (h, m, f, s) = RestSystem.GetRecoveryRates(quality);
        Assert.Equal(hp, h);
        Assert.Equal(mp, m);
        Assert.Equal(fatigue, f);
        Assert.Equal(sanity, s);
    }

    [Theory]
    [InlineData(SleepQuality.DeepSleep, 50)]
    [InlineData(SleepQuality.Normal, 35)]
    [InlineData(SleepQuality.Light, 20)]
    [InlineData(SleepQuality.Nap, 10)]
    public void GetSleepDuration_CorrectValues(SleepQuality quality, int expected)
    {
        Assert.Equal(expected, RestSystem.GetSleepDuration(quality));
    }

    [Fact]
    public void CanCamp_FalseWithEnemyNearby()
    {
        Assert.False(RestSystem.CanCamp(false, true, 0));
    }

    [Fact]
    public void CanCamp_OutdoorFloor0_True()
    {
        Assert.True(RestSystem.CanCamp(false, false, 0));
    }

    [Fact]
    public void CanCamp_OutdoorFloor1_False()
    {
        Assert.False(RestSystem.CanCamp(false, false, 1));
    }

    [Fact]
    public void CanCamp_IndoorFloor3_True()
    {
        Assert.True(RestSystem.CanCamp(true, false, 3));
    }

    [Fact]
    public void CanCamp_IndoorFloor4_False()
    {
        Assert.False(RestSystem.CanCamp(true, false, 4));
    }

    [Fact]
    public void CalculateAmbushChance_BaseCase()
    {
        float chance = RestSystem.CalculateAmbushChance(5, false, false);
        float expected = Math.Clamp(0.1f + 5 * 0.02f, 0.02f, 0.5f);
        Assert.Equal(expected, chance);
    }

    [Fact]
    public void CalculateAmbushChance_WithCampfire()
    {
        float chance = RestSystem.CalculateAmbushChance(5, true, false);
        float expected = Math.Clamp(0.1f + 5 * 0.02f - 0.05f, 0.02f, 0.5f);
        Assert.Equal(expected, chance);
    }

    [Fact]
    public void CalculateAmbushChance_WithGuard()
    {
        float chance = RestSystem.CalculateAmbushChance(5, false, true);
        float expected = Math.Clamp(0.1f + 5 * 0.02f - 0.1f, 0.02f, 0.5f);
        Assert.Equal(expected, chance);
    }

    [Fact]
    public void CalculateAmbushChance_ClampedMin()
    {
        float chance = RestSystem.CalculateAmbushChance(0, true, true);
        Assert.Equal(0.02f, chance);
    }

    [Fact]
    public void CalculateAmbushChance_ClampedMax()
    {
        float chance = RestSystem.CalculateAmbushChance(100, false, false);
        Assert.Equal(0.5f, chance);
    }

    [Theory]
    [InlineData(SleepQuality.DeepSleep, "熟睡")]
    [InlineData(SleepQuality.Normal, "普通")]
    [InlineData(SleepQuality.Light, "浅い眠り")]
    [InlineData(SleepQuality.Nap, "仮眠")]
    public void GetQualityName_ReturnsJapanese(SleepQuality quality, string expected)
    {
        Assert.Equal(expected, RestSystem.GetQualityName(quality));
    }

    [Theory]
    [InlineData(SleepQuality.DeepSleep, 100)]
    [InlineData(SleepQuality.Normal, 50)]
    [InlineData(SleepQuality.Light, 25)]
    [InlineData(SleepQuality.Nap, 10)]
    public void CalculateInnCost_CorrectValues(SleepQuality quality, int expected)
    {
        Assert.Equal(expected, RestSystem.CalculateInnCost(quality));
    }
}

#endregion

#region 19. DungeonFactionSystem Tests

public class Phase6Expansion_DungeonFactionSystemTests
{
    [Fact]
    public void GetHostility_SameRace_Zero()
    {
        Assert.Equal(0f, DungeonFactionSystem.GetHostility(MonsterRace.Beast, MonsterRace.Beast));
    }

    [Theory]
    [InlineData(MonsterRace.Beast, MonsterRace.Undead, 0.8f)]
    [InlineData(MonsterRace.Humanoid, MonsterRace.Demon, 0.7f)]
    [InlineData(MonsterRace.Humanoid, MonsterRace.Undead, 0.6f)]
    [InlineData(MonsterRace.Spirit, MonsterRace.Demon, 0.9f)]
    [InlineData(MonsterRace.Beast, MonsterRace.Plant, 0.1f)]
    [InlineData(MonsterRace.Undead, MonsterRace.Demon, 0.2f)]
    [InlineData(MonsterRace.Insect, MonsterRace.Plant, 0.15f)]
    public void GetHostility_DefinedPairs(MonsterRace r1, MonsterRace r2, float expected)
    {
        Assert.Equal(expected, DungeonFactionSystem.GetHostility(r1, r2));
    }

    [Fact]
    public void GetHostility_ReverseOrder()
    {
        Assert.Equal(0.8f, DungeonFactionSystem.GetHostility(MonsterRace.Undead, MonsterRace.Beast));
    }

    [Fact]
    public void GetHostility_UndefinedPair_Returns0_4()
    {
        Assert.Equal(0.4f, DungeonFactionSystem.GetHostility(MonsterRace.Dragon, MonsterRace.Insect));
    }

    [Theory]
    [InlineData(MonsterRace.Beast, MonsterRace.Undead, true)]
    [InlineData(MonsterRace.Humanoid, MonsterRace.Demon, true)]
    [InlineData(MonsterRace.Spirit, MonsterRace.Demon, true)]
    public void AreHostile_TrueForHostilePairs(MonsterRace r1, MonsterRace r2, bool expected)
    {
        Assert.Equal(expected, DungeonFactionSystem.AreHostile(r1, r2));
    }

    [Theory]
    [InlineData(MonsterRace.Beast, MonsterRace.Plant, false)]
    [InlineData(MonsterRace.Undead, MonsterRace.Demon, false)]
    public void AreHostile_FalseForFriendlyPairs(MonsterRace r1, MonsterRace r2, bool expected)
    {
        Assert.Equal(expected, DungeonFactionSystem.AreHostile(r1, r2));
    }

    [Theory]
    [InlineData(MonsterRace.Beast, MonsterRace.Plant, true)]
    [InlineData(MonsterRace.Undead, MonsterRace.Demon, true)]
    [InlineData(MonsterRace.Insect, MonsterRace.Plant, true)]
    [InlineData(MonsterRace.Beast, MonsterRace.Beast, true)]
    public void AreAllied_CorrectForPairs(MonsterRace r1, MonsterRace r2, bool expected)
    {
        Assert.Equal(expected, DungeonFactionSystem.AreAllied(r1, r2));
    }

    [Fact]
    public void AreAllied_FalseForHostile()
    {
        Assert.False(DungeonFactionSystem.AreAllied(MonsterRace.Beast, MonsterRace.Undead));
    }

    [Fact]
    public void GetHostileRaces_Beast()
    {
        var hostile = DungeonFactionSystem.GetHostileRaces(MonsterRace.Beast);
        Assert.Contains(MonsterRace.Undead, hostile);
    }

    [Fact]
    public void GetHostileRaces_Humanoid()
    {
        var hostile = DungeonFactionSystem.GetHostileRaces(MonsterRace.Humanoid);
        Assert.Contains(MonsterRace.Demon, hostile);
        Assert.Contains(MonsterRace.Undead, hostile);
    }

    [Fact]
    public void GetAllRelations_Returns11()
    {
        Assert.Equal(11, DungeonFactionSystem.GetAllRelations().Count);
    }
}

#endregion

#region 20. ElementalAffinitySystem Tests

public class Phase6Expansion_ElementalAffinitySystemTests
{
    [Theory]
    [InlineData(MonsterRace.Beast, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Holy, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Light, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Poison, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Undead, Element.Dark, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Demon, Element.Holy, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Demon, Element.Dark, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Demon, Element.Curse, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Dragon, Element.Ice, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Plant, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Plant, Element.Ice, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Plant, Element.Earth, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Plant, Element.Water, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Insect, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Spirit, Element.Light, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Spirit, Element.Dark, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Construct, Element.Lightning, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Construct, Element.Poison, ElementalResistanceLevel.Immune)]
    [InlineData(MonsterRace.Humanoid, Element.Fire, ElementalResistanceLevel.Normal)]
    public void GetResistanceLevel_CorrectValues(MonsterRace race, Element element, ElementalResistanceLevel expected)
    {
        Assert.Equal(expected, ElementalAffinitySystem.GetResistanceLevel(race, element));
    }

    [Fact]
    public void GetResistanceLevel_NoneElement_Normal()
    {
        Assert.Equal(ElementalResistanceLevel.Normal, ElementalAffinitySystem.GetResistanceLevel(MonsterRace.Beast, Element.None));
    }

    [Fact]
    public void GetResistanceLevel_UndefinedPair_Normal()
    {
        Assert.Equal(ElementalResistanceLevel.Normal, ElementalAffinitySystem.GetResistanceLevel(MonsterRace.Beast, Element.Water));
    }

    [Theory]
    [InlineData(ElementalResistanceLevel.Weakness, 1.5f)]
    [InlineData(ElementalResistanceLevel.Normal, 1.0f)]
    [InlineData(ElementalResistanceLevel.Resistant, 0.5f)]
    [InlineData(ElementalResistanceLevel.Immune, 0.0f)]
    [InlineData(ElementalResistanceLevel.Absorb, -1.0f)]
    public void GetDamageMultiplier_CorrectValues(ElementalResistanceLevel level, float expected)
    {
        Assert.Equal(expected, ElementalAffinitySystem.GetDamageMultiplier(level));
    }

    [Theory]
    [InlineData(100, Element.Fire, MonsterRace.Beast, 150)]
    [InlineData(100, Element.Fire, MonsterRace.Humanoid, 100)]
    [InlineData(100, Element.Poison, MonsterRace.Undead, 50)]
    [InlineData(100, Element.Poison, MonsterRace.Construct, 0)]
    [InlineData(100, Element.None, MonsterRace.Beast, 100)]
    public void CalculateElementalDamage_CorrectCalculation(int baseDmg, Element element, MonsterRace race, int expected)
    {
        Assert.Equal(expected, ElementalAffinitySystem.CalculateElementalDamage(baseDmg, element, race));
    }

    [Theory]
    [InlineData(WeaponType.Unarmed, AttackType.Unarmed)]
    [InlineData(WeaponType.Dagger, AttackType.Pierce)]
    [InlineData(WeaponType.Sword, AttackType.Slash)]
    [InlineData(WeaponType.Greatsword, AttackType.Slash)]
    [InlineData(WeaponType.Axe, AttackType.Slash)]
    [InlineData(WeaponType.Greataxe, AttackType.Slash)]
    [InlineData(WeaponType.Spear, AttackType.Pierce)]
    [InlineData(WeaponType.Hammer, AttackType.Blunt)]
    [InlineData(WeaponType.Staff, AttackType.Blunt)]
    [InlineData(WeaponType.Bow, AttackType.Ranged)]
    [InlineData(WeaponType.Crossbow, AttackType.Ranged)]
    [InlineData(WeaponType.Thrown, AttackType.Ranged)]
    [InlineData(WeaponType.Whip, AttackType.Slash)]
    [InlineData(WeaponType.Fist, AttackType.Unarmed)]
    public void GetWeaponTypeAttackType_AllWeapons(WeaponType weapon, AttackType expected)
    {
        Assert.Equal(expected, ElementalAffinitySystem.GetWeaponTypeAttackType(weapon));
    }

    [Theory]
    [InlineData(AttackType.Slash, MonsterRace.Amorphous, 0.5f)]
    [InlineData(AttackType.Pierce, MonsterRace.Amorphous, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Amorphous, 1.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Construct, 1.5f)]
    [InlineData(AttackType.Slash, MonsterRace.Construct, 0.7f)]
    [InlineData(AttackType.Pierce, MonsterRace.Construct, 0.7f)]
    [InlineData(AttackType.Unarmed, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Slash, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Pierce, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Ranged, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Undead, 1.3f)]
    [InlineData(AttackType.Slash, MonsterRace.Undead, 0.8f)]
    [InlineData(AttackType.Slash, MonsterRace.Humanoid, 1.0f)]
    public void GetPhysicalDamageMultiplier_CorrectValues(AttackType attack, MonsterRace race, float expected)
    {
        Assert.Equal(expected, ElementalAffinitySystem.GetPhysicalDamageMultiplier(attack, race));
    }

    [Fact]
    public void Amorphous_AllElementalWeaknesses()
    {
        Assert.Equal(ElementalResistanceLevel.Weakness, ElementalAffinitySystem.GetResistanceLevel(MonsterRace.Amorphous, Element.Fire));
        Assert.Equal(ElementalResistanceLevel.Weakness, ElementalAffinitySystem.GetResistanceLevel(MonsterRace.Amorphous, Element.Lightning));
        Assert.Equal(ElementalResistanceLevel.Weakness, ElementalAffinitySystem.GetResistanceLevel(MonsterRace.Amorphous, Element.Ice));
    }
}

#endregion
