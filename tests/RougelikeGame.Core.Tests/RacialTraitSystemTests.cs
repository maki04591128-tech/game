using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 種族特性・成長・職業装備・素性クリア条件システムのテスト（Phase 5.1-5.3）
/// </summary>
public class RacialTraitSystemTests
{
    #region RacialTraitSystem Tests

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
    public void RacialTraitSystem_AllRaces_HaveTraits(Race race)
    {
        var traits = RacialTraitSystem.GetTraits(race);
        Assert.NotNull(traits);
        Assert.True(traits.Count > 0, $"{race} should have at least one trait");
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Orc_BerserkerBlood()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Orc, RacialTraitType.BerserkerBlood));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Elf_MagicDamageBonus()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Elf, RacialTraitType.MagicDamageBonus));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Undead_NoFoodRequired()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Undead, RacialTraitType.NoFoodRequired));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Undead_PoisonImmunity()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Undead, RacialTraitType.PoisonImmunity));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_FallenAngel_Levitation()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.FallenAngel, RacialTraitType.Levitation));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Slime_PhysicalResistance()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Slime, RacialTraitType.PhysicalResistance));
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Slime_EquipmentRestriction()
    {
        Assert.True(RacialTraitSystem.HasTrait(Race.Slime, RacialTraitType.EquipmentRestriction));
    }

    [Fact]
    public void RacialTraitSystem_GetTraitValue_Orc_BerserkerBlood_Returns025()
    {
        double value = RacialTraitSystem.GetTraitValue(Race.Orc, RacialTraitType.BerserkerBlood);
        Assert.Equal(0.25, value);
    }

    [Fact]
    public void RacialTraitSystem_GetTraitValue_Halfling_LuckyBody_Returns005()
    {
        double value = RacialTraitSystem.GetTraitValue(Race.Halfling, RacialTraitType.LuckyBody);
        Assert.Equal(0.05, value);
    }

    [Fact]
    public void RacialTraitSystem_HasTrait_Human_DoesNotHave_BerserkerBlood()
    {
        Assert.False(RacialTraitSystem.HasTrait(Race.Human, RacialTraitType.BerserkerBlood));
    }

    #endregion

    #region GrowthSystem Tests

    [Fact]
    public void GrowthSystem_GetRaceGrowthRate_AllRaces_ReturnValid()
    {
        foreach (Race race in Enum.GetValues<Race>())
        {
            var rate = GrowthSystem.GetRaceGrowthRate(race);
            Assert.True(rate.HpPerLevel > 0, $"{race} should have positive HP growth");
        }
    }

    [Fact]
    public void GrowthSystem_GetClassGrowthRate_AllClasses_ReturnValid()
    {
        foreach (CharacterClass cls in Enum.GetValues<CharacterClass>())
        {
            var rate = GrowthSystem.GetClassGrowthRate(cls);
            Assert.True(rate.HpPerLevel > 0, $"{cls} should have positive HP growth");
        }
    }

    [Fact]
    public void GrowthSystem_CalculateLevelUpBonus_ReturnsNonNegativeStats()
    {
        var bonus = GrowthSystem.CalculateLevelUpBonus(Race.Human, CharacterClass.Fighter, 5);
        Assert.True(bonus.Strength >= 0);
        Assert.True(bonus.Vitality >= 0);
        Assert.True(bonus.Agility >= 0);
    }

    [Fact]
    public void GrowthSystem_CalculateHpGrowth_Orc_Fighter_HigherThanElf_Mage()
    {
        int orcFighterHp = GrowthSystem.CalculateHpGrowth(Race.Orc, CharacterClass.Fighter);
        int elfMageHp = GrowthSystem.CalculateHpGrowth(Race.Elf, CharacterClass.Mage);
        Assert.True(orcFighterHp > elfMageHp,
            $"Orc Fighter HP growth ({orcFighterHp}) should be higher than Elf Mage ({elfMageHp})");
    }

    [Fact]
    public void GrowthSystem_CalculateMpGrowth_Elf_Mage_HigherThanOrc_Fighter()
    {
        int elfMageMp = GrowthSystem.CalculateMpGrowth(Race.Elf, CharacterClass.Mage);
        int orcFighterMp = GrowthSystem.CalculateMpGrowth(Race.Orc, CharacterClass.Fighter);
        Assert.True(elfMageMp > orcFighterMp,
            $"Elf Mage MP growth ({elfMageMp}) should be higher than Orc Fighter ({orcFighterMp})");
    }

    [Fact]
    public void GrowthSystem_CalculateMaxHp_IncreasesWithLevel()
    {
        var stats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int hpLv1 = GrowthSystem.CalculateMaxHp(stats, Race.Human, CharacterClass.Fighter, 1);
        int hpLv10 = GrowthSystem.CalculateMaxHp(stats, Race.Human, CharacterClass.Fighter, 10);
        Assert.True(hpLv10 > hpLv1, $"HP at level 10 ({hpLv10}) should be > HP at level 1 ({hpLv1})");
    }

    [Fact]
    public void GrowthSystem_CalculateMaxMp_IncreasesWithLevel()
    {
        var stats = new Stats(10, 10, 10, 10, 10, 10, 10, 10, 10);
        int mpLv1 = GrowthSystem.CalculateMaxMp(stats, Race.Elf, CharacterClass.Mage, 1);
        int mpLv10 = GrowthSystem.CalculateMaxMp(stats, Race.Elf, CharacterClass.Mage, 10);
        Assert.True(mpLv10 > mpLv1, $"MP at level 10 ({mpLv10}) should be > MP at level 1 ({mpLv1})");
    }

    [Fact]
    public void GrowthRate_GetLevelBonus_Level1_ReturnsZero()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        var bonus = rate.GetLevelBonus(1);
        Assert.Equal(0, bonus.Strength);
        Assert.Equal(0, bonus.Vitality);
    }

    [Fact]
    public void GrowthRate_GetHpBonus_Level1_ReturnsZero()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Human);
        Assert.Equal(0, rate.GetHpBonus(1));
    }

    [Fact]
    public void GrowthRate_GetHpBonus_Level10_ReturnsPositive()
    {
        var rate = GrowthSystem.GetRaceGrowthRate(Race.Orc);
        Assert.True(rate.GetHpBonus(10) > 0);
    }

    #endregion

    #region ClassEquipmentSystem Tests

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
    public void ClassEquipmentSystem_AllClasses_HaveProficiencies(CharacterClass cls)
    {
        var proficiencies = ClassEquipmentSystem.GetProficiencies(cls);
        Assert.NotNull(proficiencies);
        Assert.True(proficiencies.Count > 0, $"{cls} should have at least one proficiency");
    }

    [Fact]
    public void ClassEquipmentSystem_Fighter_ProficientWithSword()
    {
        Assert.True(ClassEquipmentSystem.IsProficient(CharacterClass.Fighter, EquipmentCategory.Sword));
    }

    [Fact]
    public void ClassEquipmentSystem_Mage_NotProficientWithSword()
    {
        Assert.False(ClassEquipmentSystem.IsProficient(CharacterClass.Mage, EquipmentCategory.Sword));
    }

    [Fact]
    public void ClassEquipmentSystem_Mage_ProficientWithStaff()
    {
        Assert.True(ClassEquipmentSystem.IsProficient(CharacterClass.Mage, EquipmentCategory.Staff));
    }

    [Fact]
    public void ClassEquipmentSystem_ProficiencyMultiplier_Proficient_Returns1()
    {
        double mult = ClassEquipmentSystem.GetProficiencyMultiplier(CharacterClass.Fighter, EquipmentCategory.Sword);
        Assert.Equal(1.0, mult);
    }

    [Fact]
    public void ClassEquipmentSystem_ProficiencyMultiplier_NotProficient_ReturnsLessThan1()
    {
        double mult = ClassEquipmentSystem.GetProficiencyMultiplier(CharacterClass.Mage, EquipmentCategory.Sword);
        Assert.True(mult < 1.0, $"Non-proficient multiplier should be < 1.0, got {mult}");
    }

    #endregion

    #region BackgroundClearSystem Tests

    [Fact]
    public void BackgroundClearSystem_SetFlag_HasFlag_Works()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.HasFlag("dungeon_clear"));
        system.SetFlag("dungeon_clear");
        Assert.True(system.HasFlag("dungeon_clear"));
    }

    [Fact]
    public void BackgroundClearSystem_IncrementFlag_Accumulates()
    {
        var system = new BackgroundClearSystem();
        Assert.Equal(0, system.GetFlagValue("boss_kills"));
        system.IncrementFlag("boss_kills");
        system.IncrementFlag("boss_kills");
        system.IncrementFlag("boss_kills", 3);
        Assert.Equal(5, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void BackgroundClearSystem_SetFlagValue_OverridesValue()
    {
        var system = new BackgroundClearSystem();
        system.IncrementFlag("boss_kills", 5);
        system.SetFlagValue("boss_kills", 99);
        Assert.Equal(99, system.GetFlagValue("boss_kills"));
    }

    [Fact]
    public void BackgroundClearSystem_IsClearConditionMet_DungeonClear()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Adventurer, 1, 0));
        system.SetFlag("dungeon_clear");
        Assert.True(system.IsClearConditionMet(Background.Adventurer, 1, 0));
    }

    [Fact]
    public void BackgroundClearSystem_IsClearConditionMet_BossKills10()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Soldier, 1, 0));
        for (int i = 0; i < 10; i++)
            system.IncrementFlag("boss_kills");
        Assert.True(system.IsClearConditionMet(Background.Soldier, 1, 0));
    }

    [Fact]
    public void BackgroundClearSystem_IsClearConditionMet_Gold100000()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Merchant, 1, 50000));
        Assert.True(system.IsClearConditionMet(Background.Merchant, 1, 100000));
    }

    [Fact]
    public void BackgroundClearSystem_IsClearConditionMet_Level30()
    {
        var system = new BackgroundClearSystem();
        Assert.False(system.IsClearConditionMet(Background.Peasant, 29, 0));
        Assert.True(system.IsClearConditionMet(Background.Peasant, 30, 0));
    }

    [Fact]
    public void BackgroundClearSystem_SaveAndRestore_PreservesState()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("dungeon_clear");
        system.IncrementFlag("boss_kills", 7);
        system.SetFlagValue("test_value", 42);

        var saveData = system.CreateSaveData();

        var restored = new BackgroundClearSystem();
        restored.RestoreFromSave(saveData);

        Assert.True(restored.HasFlag("dungeon_clear"));
        Assert.Equal(7, restored.GetFlagValue("boss_kills"));
        Assert.Equal(42, restored.GetFlagValue("test_value"));
    }

    [Fact]
    public void BackgroundClearSystem_Restore_ClearsPreviousState()
    {
        var system = new BackgroundClearSystem();
        system.SetFlag("old_flag");
        system.IncrementFlag("old_counter", 100);

        var emptyData = new ClearFlagSaveData();
        system.RestoreFromSave(emptyData);

        Assert.False(system.HasFlag("old_flag"));
        Assert.Equal(0, system.GetFlagValue("old_counter"));
    }

    [Theory]
    [InlineData(Background.Adventurer)]
    [InlineData(Background.Soldier)]
    [InlineData(Background.Scholar)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Peasant)]
    [InlineData(Background.Noble)]
    [InlineData(Background.Wanderer)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Priest)]
    [InlineData(Background.Penitent)]
    public void BackgroundClearSystem_GetClearFlag_AllBackgrounds_ReturnNonEmpty(Background bg)
    {
        var flag = BackgroundClearSystem.GetClearFlag(bg);
        Assert.False(string.IsNullOrEmpty(flag), $"{bg} should have a clear condition flag");
    }

    [Theory]
    [InlineData(Background.Adventurer)]
    [InlineData(Background.Soldier)]
    [InlineData(Background.Scholar)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Peasant)]
    [InlineData(Background.Noble)]
    [InlineData(Background.Wanderer)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Priest)]
    [InlineData(Background.Penitent)]
    public void BackgroundClearSystem_GetClearDescription_AllBackgrounds_ReturnNonEmpty(Background bg)
    {
        var desc = BackgroundClearSystem.GetClearDescription(bg);
        Assert.False(string.IsNullOrEmpty(desc), $"{bg} should have a clear condition description");
    }

    #endregion

    #region BackgroundBonusData Tests

    [Theory]
    [InlineData(Background.Adventurer)]
    [InlineData(Background.Soldier)]
    [InlineData(Background.Scholar)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Peasant)]
    [InlineData(Background.Noble)]
    [InlineData(Background.Wanderer)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Priest)]
    [InlineData(Background.Penitent)]
    public void BackgroundBonusData_Get_AllBackgrounds_ReturnValidData(Background bg)
    {
        var data = BackgroundBonusData.Get(bg);
        Assert.Equal(bg, data.Background);
        Assert.False(string.IsNullOrEmpty(data.ClearConditionDescription));
        Assert.False(string.IsNullOrEmpty(data.ClearConditionFlag));
    }

    #endregion

    #region Player Race/Class Integration Tests

    [Fact]
    public void Player_Create_WithRaceAndClass_HasCorrectRaceAndClass()
    {
        var player = Player.Create("TestPlayer", Race.Elf, CharacterClass.Mage, Background.Scholar);
        Assert.Equal(Race.Elf, player.Race);
        Assert.Equal(CharacterClass.Mage, player.CharacterClass);
        Assert.Equal(Background.Scholar, player.Background);
    }

    [Fact]
    public void Player_ExpMultiplier_Human_HigherThanDefault()
    {
        var raceDef = RaceDefinition.Get(Race.Human);
        Assert.True(raceDef.ExpMultiplier > 1.0,
            $"Human ExpMultiplier should be > 1.0, got {raceDef.ExpMultiplier}");
    }

    [Fact]
    public void Player_SanityLossMultiplier_Demon_DifferentFromHuman()
    {
        var human = RaceDefinition.Get(Race.Human);
        var demon = RaceDefinition.Get(Race.Demon);
        Assert.NotEqual(human.SanityLossMultiplier, demon.SanityLossMultiplier);
    }

    #endregion
}
