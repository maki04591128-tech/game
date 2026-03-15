using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// キャラクター作成・死に戻りシステムのテスト
/// </summary>
public class CharacterCreationTests
{
    #region RaceDefinition Tests

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
    public void RaceDefinition_Get_AllRaces_ReturnValidDefinition(Race race)
    {
        var def = RaceDefinition.Get(race);
        Assert.Equal(race, def.Race);
        Assert.False(string.IsNullOrEmpty(def.Name));
        Assert.False(string.IsNullOrEmpty(def.Description));
        Assert.NotNull(def.Traits);
        Assert.True(def.Traits.Length > 0);
        Assert.True(def.ExpMultiplier > 0);
        Assert.True(def.SanityLossMultiplier > 0);
    }

    [Fact]
    public void RaceDefinition_GetAll_Returns10Races()
    {
        var all = RaceDefinition.GetAll();
        Assert.Equal(10, all.Count);
    }

    [Fact]
    public void RaceDefinition_Human_HasExpMultiplierBonus()
    {
        var human = RaceDefinition.Get(Race.Human);
        Assert.True(human.ExpMultiplier > 1.0);
    }

    [Fact]
    public void RaceDefinition_Elf_HasMagicBonus()
    {
        var elf = RaceDefinition.Get(Race.Elf);
        Assert.True(elf.StatBonus.Intelligence > 0);
        Assert.True(elf.MpBonus > 0);
    }

    #endregion

    #region ClassDefinition Tests

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
    public void ClassDefinition_Get_AllClasses_ReturnValidDefinition(CharacterClass cls)
    {
        var def = ClassDefinition.Get(cls);
        Assert.Equal(cls, def.Class);
        Assert.False(string.IsNullOrEmpty(def.Name));
        Assert.NotNull(def.InitialSkills);
        Assert.True(def.InitialSkills.Length > 0);
    }

    [Fact]
    public void ClassDefinition_GetAll_Returns10Classes()
    {
        var all = ClassDefinition.GetAll();
        Assert.Equal(10, all.Count);
    }

    [Fact]
    public void ClassDefinition_Mage_HasHighMpBonus()
    {
        var mage = ClassDefinition.Get(CharacterClass.Mage);
        Assert.True(mage.MpBonus >= 20);
        Assert.True(mage.StatBonus.Intelligence > 0);
    }

    #endregion

    #region BackgroundDefinition Tests

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
    public void BackgroundDefinition_Get_AllBackgrounds_ReturnValidDefinition(Background bg)
    {
        var def = BackgroundDefinition.Get(bg);
        Assert.Equal(bg, def.Background);
        Assert.False(string.IsNullOrEmpty(def.Name));
        Assert.True(def.StartingGold >= 0);
    }

    [Fact]
    public void BackgroundDefinition_GetAll_Returns10Backgrounds()
    {
        var all = BackgroundDefinition.GetAll();
        Assert.Equal(10, all.Count);
    }

    [Fact]
    public void BackgroundDefinition_Noble_HasHighStartingGold()
    {
        var noble = BackgroundDefinition.Get(Background.Noble);
        Assert.True(noble.StartingGold >= 200);
    }

    [Fact]
    public void BackgroundDefinition_Merchant_HasHighStartingGold()
    {
        var merchant = BackgroundDefinition.Get(Background.Merchant);
        Assert.True(merchant.StartingGold >= 200);
    }

    #endregion

    #region Player Creation Tests

    [Fact]
    public void Player_CreateWithRaceClassBackground_SetsProperties()
    {
        var player = Player.Create("テスト", Race.Elf, CharacterClass.Mage, Background.Scholar);
        Assert.Equal(Race.Elf, player.Race);
        Assert.Equal(CharacterClass.Mage, player.CharacterClass);
        Assert.Equal(Background.Scholar, player.Background);
        Assert.Equal("テスト", player.Name);
    }

    [Fact]
    public void Player_CreateWithRaceClassBackground_AppliesStatBonuses()
    {
        // Default stats = 10 all
        // Elf: INT+3, MND+2, AGI+1, VIT-2
        // Mage: INT+4, MND+1, VIT-2
        // Scholar: INT+2, PER+1
        var player = Player.Create("テスト", Race.Elf, CharacterClass.Mage, Background.Scholar);
        Assert.Equal(10 + 3 + 4 + 2, player.BaseStats.Intelligence); // 19
        Assert.Equal(10 + 2 + 1, player.BaseStats.Mind); // 13
        Assert.Equal(10 - 2 - 2, player.BaseStats.Vitality); // 6
    }

    [Fact]
    public void Player_CreateWithRaceClassBackground_AppliesStartingGold()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Noble);
        Assert.Equal(300, player.Gold);
    }

    [Fact]
    public void Player_CreateWithRaceClassBackground_AppliesInitialSkills()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Thief, Background.Adventurer);
        Assert.Contains("解錠", player.LearnedSkills);
        Assert.Contains("忍び足", player.LearnedSkills);
    }

    [Fact]
    public void Player_CreateDefault_HasDefaultRaceClassBackground()
    {
        var player = Player.Create("テスト", Stats.Default);
        Assert.Equal(Race.Human, player.Race);
        Assert.Equal(CharacterClass.Fighter, player.CharacterClass);
        Assert.Equal(Background.Adventurer, player.Background);
    }

    #endregion

    #region Death and TransferData Tests

    [Fact]
    public void Player_HandleDeath_ReducesSanity()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        int initialSanity = player.Sanity;
        player.HandleDeath(DeathCause.Combat);
        Assert.True(player.Sanity < initialSanity);
    }

    [Fact]
    public void Player_HandleDeath_ReducesRescueCount()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        int initialRescue = player.RescueCountRemaining;
        player.HandleDeath(DeathCause.Combat);
        Assert.Equal(initialRescue - 1, player.RescueCountRemaining);
    }

    [Fact]
    public void Player_CreateTransferData_PreservesKnowledge()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        player.LearnWord("fire");
        player.LearnSkill("強打");
        var transfer = player.CreateTransferData();
        Assert.True(transfer.LearnedWords.ContainsKey("fire"));
        Assert.Contains("強打", transfer.LearnedSkills);
    }

    [Fact]
    public void Player_ApplyTransferData_RestoresKnowledge()
    {
        var player1 = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        player1.LearnWord("fire");
        player1.LearnSkill("強打");
        var transfer = player1.CreateTransferData();

        var player2 = Player.Create("テスト2", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        player2.ApplyTransferData(transfer);
        Assert.True(player2.LearnedWords.ContainsKey("fire"));
        Assert.Contains("強打", player2.LearnedSkills);
    }

    [Fact]
    public void Player_ApplyTransferData_WithZeroSanity_DoesNotRestoreKnowledge()
    {
        var player1 = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        player1.LearnWord("fire");
        var transfer = player1.CreateTransferData();
        transfer.Sanity = 0; // 正気度0

        var player2 = Player.Create("テスト2", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        player2.ApplyTransferData(transfer);
        Assert.False(player2.LearnedWords.ContainsKey("fire"));
    }

    [Fact]
    public void Player_RestoreFromSave_WithRaceClassBackground()
    {
        var player = Player.Create("テスト", Stats.Default);
        player.RestoreFromSave(5, 100, 80, 90, 50, 30, 100, 2,
            Race.Dwarf, CharacterClass.Knight, Background.Soldier);
        Assert.Equal(Race.Dwarf, player.Race);
        Assert.Equal(CharacterClass.Knight, player.CharacterClass);
        Assert.Equal(Background.Soldier, player.Background);
        Assert.Equal(5, player.Level);
    }

    #endregion
}
