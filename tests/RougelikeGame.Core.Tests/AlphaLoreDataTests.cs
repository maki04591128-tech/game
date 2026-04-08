using Xunit;
using RougelikeGame.Core.Data;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>α.23-26, α.1-12: テキストコンテンツのテスト</summary>
public class AlphaLoreDataTests
{
    // =========================================================
    // α.23: MonsterLoreData テスト
    // =========================================================

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
    public void MonsterLore_GetDescription_Level1To5_ReturnsNonEmpty(MonsterRace race)
    {
        for (int level = 1; level <= 5; level++)
        {
            var text = MonsterLoreData.GetDescription(race, level);
            Assert.NotEmpty(text);
            Assert.NotEqual("情報なし", text);
        }
    }

    [Theory]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Dragon)]
    [InlineData(MonsterRace.Undead)]
    public void MonsterLore_GetRaceFlavorText_ReturnsNonEmpty(MonsterRace race)
    {
        var text = MonsterLoreData.GetRaceFlavorText(race);
        Assert.NotEmpty(text);
        Assert.NotEqual("詳細不明", text);
    }

    [Theory]
    [InlineData(MonsterRace.Beast)]
    [InlineData(MonsterRace.Dragon)]
    public void MonsterLore_GetLevelDescriptions_Returns5Entries(MonsterRace race)
    {
        var dict = MonsterLoreData.GetLevelDescriptions(race);
        Assert.Equal(5, dict.Count);
        for (int i = 1; i <= 5; i++)
            Assert.True(dict.ContainsKey(i));
    }

    [Fact]
    public void MonsterLore_AllRaces_Have5LevelDescriptions()
    {
        var races = Enum.GetValues<MonsterRace>();
        foreach (var race in races)
        {
            var dict = MonsterLoreData.GetLevelDescriptions(race);
            Assert.Equal(5, dict.Count);
        }
    }

    // =========================================================
    // α.24: DungeonEventLoreData テスト
    // =========================================================

    [Theory]
    [InlineData(RandomEventType.TreasureChest)]
    [InlineData(RandomEventType.Fountain)]
    [InlineData(RandomEventType.Shrine)]
    [InlineData(RandomEventType.Ruins)]
    [InlineData(RandomEventType.NpcEncounter)]
    [InlineData(RandomEventType.MerchantEncounter)]
    [InlineData(RandomEventType.RestPoint)]
    [InlineData(RandomEventType.MysteriousItem)]
    [InlineData(RandomEventType.MonsterHouse)]
    [InlineData(RandomEventType.CursedRoom)]
    [InlineData(RandomEventType.BlessedRoom)]
    [InlineData(RandomEventType.Trap)]
    public void DungeonEvent_GetDiscoveryText_ReturnsNonEmpty(RandomEventType eventType)
    {
        var text = DungeonEventLoreData.GetDiscoveryText(eventType);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(RandomEventType.TreasureChest)]
    [InlineData(RandomEventType.Fountain)]
    [InlineData(RandomEventType.Shrine)]
    [InlineData(RandomEventType.Ruins)]
    [InlineData(RandomEventType.BlessedRoom)]
    public void DungeonEvent_GetInteractText_ReturnsNonEmpty(RandomEventType eventType)
    {
        var text = DungeonEventLoreData.GetInteractText(eventType);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void DungeonEvent_GetTreasureOpenText_ReturnsNonEmpty(int rarity)
    {
        var text = DungeonEventLoreData.GetTreasureOpenText(rarity);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("arrow")]
    [InlineData("pit")]
    [InlineData("poison_gas")]
    [InlineData("alarm")]
    [InlineData("flame")]
    [InlineData("electric")]
    public void DungeonEvent_GetTrapActivateText_ReturnsNonEmpty(string trapType)
    {
        var text = DungeonEventLoreData.GetTrapActivateText(trapType);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.25: GameOverSystem 死亡演出テキスト テスト
    // =========================================================

    [Theory]
    [InlineData(DeathCause.Combat)]
    [InlineData(DeathCause.Boss)]
    [InlineData(DeathCause.Starvation)]
    [InlineData(DeathCause.Dehydration)]
    [InlineData(DeathCause.Trap)]
    [InlineData(DeathCause.Poison)]
    [InlineData(DeathCause.TimeLimit)]
    [InlineData(DeathCause.Curse)]
    [InlineData(DeathCause.Suicide)]
    [InlineData(DeathCause.SanityDeath)]
    [InlineData(DeathCause.Fall)]
    [InlineData(DeathCause.Unknown)]
    public void GameOver_GetDeathFlavorText_AllCauses_ReturnsNonEmpty(DeathCause cause)
    {
        var text = GameOverSystem.GetDeathFlavorText(cause);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void GameOver_GetRebornText_VariousLoopCounts_ReturnsNonEmpty(int loopCount)
    {
        var text = GameOverSystem.GetRebornText(loopCount, 50);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void GameOver_GetRebornText_LowSanity_ContainsWarning()
    {
        var text = GameOverSystem.GetRebornText(8, 5);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void GameOver_GetSanityGameOverText_ReturnsNonEmpty()
    {
        var text = GameOverSystem.GetSanityGameOverText();
        Assert.NotEmpty(text);
        Assert.Contains("GAME OVER", text);
    }

    [Fact]
    public void GameOver_GetTrueGameOverText_ContainsTotalDeaths()
    {
        var text = GameOverSystem.GetTrueGameOverText(15);
        Assert.NotEmpty(text);
        Assert.Contains("15", text);
        Assert.Contains("THE END", text);
    }

    // =========================================================
    // α.26: TimeSeasonLoreData テスト
    // =========================================================

    [Theory]
    [InlineData(TimePeriod.Dawn)]
    [InlineData(TimePeriod.Morning)]
    [InlineData(TimePeriod.Afternoon)]
    [InlineData(TimePeriod.Dusk)]
    [InlineData(TimePeriod.Night)]
    [InlineData(TimePeriod.Midnight)]
    public void TimeSeasonLore_GetTimePeriodText_AllPeriods_ReturnsNonEmpty(TimePeriod time)
    {
        for (int variant = 0; variant < 3; variant++)
        {
            var text = TimeSeasonLoreData.GetTimePeriodText(time, variant);
            Assert.NotEmpty(text);
            Assert.NotEqual("時間帯不明", text);
        }
    }

    [Theory]
    [InlineData(Season.Spring)]
    [InlineData(Season.Summer)]
    [InlineData(Season.Autumn)]
    [InlineData(Season.Winter)]
    public void TimeSeasonLore_GetSeasonText_AllSeasons_ReturnsNonEmpty(Season season)
    {
        for (int variant = 0; variant < 3; variant++)
        {
            var text = TimeSeasonLoreData.GetSeasonText(season, variant);
            Assert.NotEmpty(text);
            Assert.NotEqual("季節不明", text);
        }
    }

    [Theory]
    [InlineData(Weather.Clear)]
    [InlineData(Weather.Rain)]
    [InlineData(Weather.Fog)]
    [InlineData(Weather.Snow)]
    [InlineData(Weather.Storm)]
    public void TimeSeasonLore_GetWeatherText_AllWeathers_ReturnsNonEmpty(Weather weather)
    {
        for (int variant = 0; variant < 3; variant++)
        {
            var text = TimeSeasonLoreData.GetWeatherText(weather, variant);
            Assert.NotEmpty(text);
        }
    }

    [Theory]
    [InlineData(Season.Spring, TimePeriod.Dawn)]
    [InlineData(Season.Summer, TimePeriod.Night)]
    [InlineData(Season.Autumn, TimePeriod.Dusk)]
    [InlineData(Season.Winter, TimePeriod.Midnight)]
    public void TimeSeasonLore_GetSpecialComboText_SpecialCombos_ReturnsNonNull(Season season, TimePeriod time)
    {
        var text = TimeSeasonLoreData.GetSpecialComboText(season, time);
        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void TimeSeasonLore_GetSpecialComboText_NormalCombo_ReturnsNull()
    {
        var text = TimeSeasonLoreData.GetSpecialComboText(Season.Spring, TimePeriod.Afternoon);
        Assert.Null(text);
    }

    // =========================================================
    // α.26d: NpcLocationLoreData テスト
    // =========================================================

    [Theory]
    [InlineData("npc_leon")]
    [InlineData("npc_marco")]
    [InlineData("npc_albert")]
    [InlineData("npc_marvin")]
    [InlineData("npc_elwen")]
    [InlineData("npc_lena")]
    public void NpcLocation_GetNpcProfile_AllLevels_ReturnsNonEmpty(string npcId)
    {
        for (int level = 1; level <= 5; level++)
        {
            var text = NpcLocationLoreData.GetNpcProfile(npcId, level);
            Assert.NotEmpty(text);
        }
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void NpcLocation_GetTerritoryDescription_AllLevels_ReturnsNonEmpty(TerritoryId territory)
    {
        for (int level = 1; level <= 5; level++)
        {
            var text = NpcLocationLoreData.GetTerritoryDescription(territory, level);
            Assert.NotEmpty(text);
            Assert.DoesNotContain("詳細な記録は存在しない", text);
        }
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void NpcLocation_GetTerritoryShortDescription_ReturnsNonEmpty(TerritoryId territory)
    {
        var text = NpcLocationLoreData.GetTerritoryShortDescription(territory);
        Assert.NotEmpty(text);
        Assert.NotEqual("記録なし", text);
    }

    // =========================================================
    // α.1-4: MainStoryData テスト
    // =========================================================

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
    public void MainStory_GetOpeningText_AllBackgrounds_ReturnsNonEmpty(Background bg)
    {
        var text = MainStoryData.GetOpeningText(bg);
        Assert.NotEmpty(text);
        Assert.NotEqual("気づいたら、知らない場所にいた。", text);
    }

    [Theory]
    [InlineData(Background.Adventurer)]
    [InlineData(Background.Scholar)]
    [InlineData(Background.Penitent)]
    public void MainStory_GetStorylineSummary_ReturnsNonEmpty(Background bg)
    {
        var text = MainStoryData.GetStorylineSummary(bg);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("ending_true")]
    [InlineData("ending_good")]
    [InlineData("ending_normal")]
    [InlineData("ending_bad")]
    [InlineData("ending_sacrifice")]
    public void MainStory_GetEndingText_AllEndings_ReturnsNonEmpty(string endingId)
    {
        var text = MainStoryData.GetEndingText(endingId, Background.Adventurer);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("leon_greeting", 0)]
    [InlineData("leon_greeting", 1)]
    [InlineData("leon_greeting", 2)]
    [InlineData("leon_greeting", 5)]
    [InlineData("world_change", 1)]
    [InlineData("world_change", 5)]
    public void MainStory_GetLoopVariationText_ReturnsNonEmpty(string contextId, int loopCount)
    {
        var text = MainStoryData.GetLoopVariationText(contextId, loopCount);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.5, α.7, α.8: NpcCharacterData テスト
    // =========================================================

    [Fact]
    public void NpcCharacter_AllNpcProfiles_HaveRequiredFields()
    {
        foreach (var profile in NpcCharacterData.AllNpcProfiles)
        {
            Assert.NotEmpty(profile.Id);
            Assert.NotEmpty(profile.Name);
            Assert.NotEmpty(profile.Title);
            Assert.NotEmpty(profile.Personality);
            Assert.NotEmpty(profile.Background);
            Assert.NotEmpty(profile.Motivation);
            Assert.NotEmpty(profile.Secret);
        }
    }

    [Theory]
    [InlineData("npc_leon")]
    [InlineData("npc_marco")]
    [InlineData("npc_albert")]
    public void NpcCharacter_GetProfile_KnownNpc_ReturnsProfile(string npcId)
    {
        var profile = NpcCharacterData.GetProfile(npcId);
        Assert.NotNull(profile);
        Assert.Equal(npcId, profile!.Id);
    }

    [Fact]
    public void NpcCharacter_GetProfile_UnknownNpc_ReturnsNull()
    {
        var profile = NpcCharacterData.GetProfile("npc_unknown_xyz");
        Assert.Null(profile);
    }

    [Theory]
    [InlineData(Background.Adventurer)]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Scholar)]
    public void NpcCharacter_GetRelationshipText_LeonVariants_ReturnsNonEmpty(Background bg)
    {
        var text = NpcCharacterData.GetRelationshipText("npc_leon", bg);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Background.Merchant)]
    [InlineData(Background.Criminal)]
    [InlineData(Background.Noble)]
    [InlineData(Background.Adventurer)]
    public void NpcCharacter_GetRelationshipText_MarcoVariants_ReturnsNonEmpty(Background bg)
    {
        var text = NpcCharacterData.GetRelationshipText("npc_marco", bg);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("greeting")]
    [InlineData("buy")]
    [InlineData("sell")]
    [InlineData("farewell")]
    [InlineData("secret_stock")]
    public void NpcCharacter_GetMarcoDialogue_ReturnsNonEmpty(string situation)
    {
        var text = NpcCharacterData.GetMarcoDialogue(situation);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void NpcCharacter_GetShopDialogue_Greeting_ReturnsNonEmpty(TerritoryId territory)
    {
        var text = NpcCharacterData.GetShopDialogue(territory, "greeting");
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.9-12: QuestLoreData テスト
    // =========================================================

    [Theory]
    [InlineData("main_prologue")]
    [InlineData("main_chapter1")]
    [InlineData("main_chapter2")]
    [InlineData("main_chapter3")]
    [InlineData("main_final")]
    public void QuestLore_GetMainQuestDescription_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetMainQuestDescription(questId);
        Assert.NotEmpty(text);
        Assert.NotEqual("クエストの詳細は不明。", text);
    }

    [Theory]
    [InlineData("main_prologue")]
    [InlineData("main_chapter1")]
    [InlineData("main_chapter2")]
    [InlineData("main_final")]
    public void QuestLore_GetMainQuestClearText_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetMainQuestClearText(questId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("quest_rat_hunt")]
    [InlineData("quest_herb_collect")]
    [InlineData("quest_bandit_clear")]
    [InlineData("quest_mine_boss")]
    [InlineData("quest_frontier_escort")]
    [InlineData("quest_ancient_ruins")]
    public void QuestLore_GetSubQuestDescription_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetSubQuestDescription(questId);
        Assert.NotEmpty(text);
        Assert.NotEqual("依頼の詳細は掲示板を参照。", text);
    }

    [Theory]
    [InlineData("quest_rat_hunt")]
    [InlineData("quest_herb_collect")]
    [InlineData("quest_bandit_clear")]
    [InlineData("quest_mine_boss")]
    [InlineData("quest_ancient_ruins")]
    public void QuestLore_GetGuildBoardText_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetGuildBoardText(questId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("quest_rat_hunt")]
    [InlineData("quest_herb_collect")]
    [InlineData("quest_bandit_clear")]
    [InlineData("quest_mine_boss")]
    [InlineData("quest_frontier_escort")]
    [InlineData("quest_ancient_ruins")]
    public void QuestLore_GetQuestCompleteThankText_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetQuestCompleteThankText(questId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("quest_rat_hunt")]
    [InlineData("quest_bandit_clear")]
    [InlineData("quest_mine_boss")]
    public void QuestLore_GetQuestEpilogueText_ReturnsNonEmpty(string questId)
    {
        var text = QuestLoreData.GetQuestEpilogueText(questId);
        Assert.NotEmpty(text);
    }
}
