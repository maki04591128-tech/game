using Xunit;
using RougelikeGame.Core.Data;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>α.27-α.45: カルマ/能力フラグ/種族関係/仲間NPC/特殊NPC/ダンジョン/季節テキストのテスト</summary>
public class AlphaLoreDataTests2
{
    // =========================================================
    // α.27: KarmaRelatedData - カルマNPC反応
    // =========================================================

    [Theory]
    [InlineData("npc_leon", KarmaRank.Saint)]
    [InlineData("npc_leon", KarmaRank.Villain)]
    [InlineData("npc_marco", KarmaRank.Criminal)]
    [InlineData("npc_marco", KarmaRank.Virtuous)]
    [InlineData("npc_marvin", KarmaRank.Saint)]
    [InlineData("npc_marvin", KarmaRank.Villain)]
    public void KarmaRelated_GetKarmaReaction_KnownNpcs_ReturnsNonEmpty(string npcId, KarmaRank rank)
    {
        var text = KarmaRelatedData.GetKarmaReaction(npcId, rank);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("unknown_npc", KarmaRank.Saint)]
    [InlineData("unknown_npc", KarmaRank.Villain)]
    [InlineData("unknown_npc", KarmaRank.Neutral)]
    public void KarmaRelated_GetKarmaReaction_UnknownNpc_ReturnsFallback(string npcId, KarmaRank rank)
    {
        var text = KarmaRelatedData.GetKarmaReaction(npcId, rank);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(KarmaRank.Neutral, KarmaRank.Normal)]
    [InlineData(KarmaRank.Normal, KarmaRank.Virtuous)]
    [InlineData(KarmaRank.Virtuous, KarmaRank.Saint)]
    [InlineData(KarmaRank.Neutral, KarmaRank.Rogue)]
    [InlineData(KarmaRank.Rogue, KarmaRank.Criminal)]
    [InlineData(KarmaRank.Criminal, KarmaRank.Villain)]
    public void KarmaRelated_GetKarmaTransitionText_MajorTransitions_ReturnsNonEmpty(KarmaRank from, KarmaRank to)
    {
        var text = KarmaRelatedData.GetKarmaTransitionText(from, to);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.28: KarmaRelatedData - カルマ分岐テキスト
    // =========================================================

    [Theory]
    [InlineData("dlg_elder_greeting", KarmaRank.Saint)]
    [InlineData("dlg_elder_greeting", KarmaRank.Villain)]
    [InlineData("dlg_guard_checkpoint", KarmaRank.Virtuous)]
    [InlineData("dlg_guard_checkpoint", KarmaRank.Criminal)]
    [InlineData("dlg_inn_keeper", KarmaRank.Saint)]
    [InlineData("dlg_inn_keeper", KarmaRank.Villain)]
    public void KarmaRelated_GetKarmaConditionalDialogue_ReturnsNonEmpty(string dialogueId, KarmaRank rank)
    {
        var text = KarmaRelatedData.GetKarmaConditionalDialogue(dialogueId, rank);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("help_free", KarmaRank.Virtuous)]
    [InlineData("steal_item", KarmaRank.Rogue)]
    [InlineData("blackmail", KarmaRank.Villain)]
    public void KarmaRelated_GetKarmaChoiceText_ReturnsNonEmpty(string choiceId, KarmaRank rank)
    {
        var text = KarmaRelatedData.GetKarmaChoiceText(choiceId, rank);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.29: KarmaRelatedData - 闇市NPC
    // =========================================================

    [Theory]
    [InlineData("greeting", KarmaRank.Saint)]
    [InlineData("greeting", KarmaRank.Criminal)]
    [InlineData("password_correct")]
    [InlineData("buy_illegal_item")]
    [InlineData("farewell_criminal")]
    public void KarmaRelated_GetBlackMarketDialogue_ReturnsNonEmpty(string situation, KarmaRank rank = KarmaRank.Neutral)
    {
        var text = KarmaRelatedData.GetBlackMarketDialogue(situation, rank);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("poison")]
    [InlineData("stolen_weapon")]
    [InlineData("forbidden_scroll")]
    [InlineData("black_market_info")]
    public void KarmaRelated_GetBlackMarketItemDescription_ReturnsNonEmpty(string category)
    {
        var text = KarmaRelatedData.GetBlackMarketItemDescription(category);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("guild_secret")]
    [InlineData("dungeon_secret")]
    [InlineData("hidden_route")]
    public void KarmaRelated_GetUndergroundInformation_ReturnsNonEmpty(string topic)
    {
        var text = KarmaRelatedData.GetUndergroundInformation(topic);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.30: StatFlagEventData - 能力値フラグNPC反応
    // =========================================================

    [Theory]
    [InlineData("npc_leon", StatFlag.Herculean)]
    [InlineData("npc_leon", StatFlag.Erudite)]
    [InlineData("npc_albert", StatFlag.Erudite)]
    [InlineData("npc_marvin", StatFlag.FleetFooted)]
    public void StatFlagEvent_GetStatFlagNpcReaction_KnownNpcs_ReturnsNonEmpty(string npcId, StatFlag flag)
    {
        var text = StatFlagEventData.GetStatFlagNpcReaction(npcId, flag);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(StatFlag.Herculean)]
    [InlineData(StatFlag.Erudite)]
    [InlineData(StatFlag.EagleEye)]
    [InlineData(StatFlag.FleetFooted)]
    [InlineData(StatFlag.Charismatic)]
    [InlineData(StatFlag.SteadyMind)]
    public void StatFlagEvent_GetStatFlagChoiceLabel_AllFlags_ReturnsNonEmpty(StatFlag flag)
    {
        var text = StatFlagEventData.GetStatFlagChoiceLabel(flag);
        Assert.NotEmpty(text);
        Assert.Contains("］", text);
    }

    // =========================================================
    // α.31: StatFlagEventData - 能力値条件イベント
    // =========================================================

    [Theory]
    [InlineData("boulder_block")]
    [InlineData("iron_gate")]
    [InlineData("trapped_creature")]
    public void StatFlagEvent_GetHerculeanEventText_ReturnsNonEmpty(string eventId)
    {
        var text = StatFlagEventData.GetHerculeanEventText(eventId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("ancient_inscription")]
    [InlineData("mysterious_tome")]
    [InlineData("poison_identification")]
    public void StatFlagEvent_GetEruditeEventText_ReturnsNonEmpty(string eventId)
    {
        var text = StatFlagEventData.GetEruditeEventText(eventId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("hidden_door")]
    [InlineData("trap_detection")]
    [InlineData("hidden_treasure")]
    public void StatFlagEvent_GetEagleEyeEventText_ReturnsNonEmpty(string eventId)
    {
        var text = StatFlagEventData.GetEagleEyeEventText(eventId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(StatFlag.Herculean)]
    [InlineData(StatFlag.Erudite)]
    [InlineData(StatFlag.EagleEye)]
    [InlineData(StatFlag.FleetFooted)]
    public void StatFlagEvent_GetStatFlagFailText_ReturnsNonEmpty(StatFlag flag)
    {
        var text = StatFlagEventData.GetStatFlagFailText(flag);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.32: RaceRelationData - 種族間関係
    // =========================================================

    [Theory]
    [InlineData("npc_elwen", Race.Human)]
    [InlineData("npc_elwen", Race.Elf)]
    [InlineData("npc_elwen", Race.Dwarf)]
    public void RaceRelation_GetRaceReactionText_KnownNpcs_ReturnsNonEmpty(string npcId, Race race)
    {
        var text = RaceRelationData.GetRaceReactionText(npcId, race);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Race.Elf, Race.Dwarf)]
    [InlineData(Race.Human, Race.Orc)]
    [InlineData(Race.Elf, Race.Beastfolk)]
    [InlineData(Race.Dwarf, Race.Human)]
    [InlineData(Race.Human, Race.Halfling)]
    public void RaceRelation_GetRaceRelationDescription_ReturnsNonEmpty(Race raceA, Race raceB)
    {
        var text = RaceRelationData.GetRaceRelationDescription(raceA, raceB);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Race.Human)]
    [InlineData(Race.Elf)]
    [InlineData(Race.Dwarf)]
    [InlineData(Race.Orc)]
    [InlineData(Race.Beastfolk)]
    [InlineData(Race.Halfling)]
    public void RaceRelation_GetRacialGreeting_AllRaces_ReturnsNonEmpty(Race race)
    {
        var text = RaceRelationData.GetRacialGreeting(race);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.33: RaceRelationData - 領地間関係
    // =========================================================

    [Theory]
    [InlineData(TerritoryId.Capital, TerritoryId.Mountain)]
    [InlineData(TerritoryId.Capital, TerritoryId.Forest)]
    [InlineData(TerritoryId.Forest, TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast, TerritoryId.Southern)]
    public void RaceRelation_GetTerritoryRelationText_ReturnsNonEmpty(TerritoryId tA, TerritoryId tB)
    {
        var text = RaceRelationData.GetTerritoryRelationText(tA, tB);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void RaceRelation_GetTerritoryRumorText_AllTerritories_ReturnsNonEmpty(TerritoryId territory)
    {
        var text = RaceRelationData.GetTerritoryRumorText(territory);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.34: CompanionNpcData - 仲間NPC設定
    // =========================================================

    [Fact]
    public void CompanionNpc_AllCompanions_HasFourEntries()
    {
        Assert.Equal(4, CompanionNpcData.AllCompanions.Count);
    }

    [Theory]
    [InlineData("companion_rena")]
    [InlineData("companion_brom")]
    [InlineData("companion_syl")]
    [InlineData("companion_mika")]
    public void CompanionNpc_GetCompanionProfile_ReturnsProfile(string companionId)
    {
        var profile = CompanionNpcData.GetCompanionProfile(companionId);
        Assert.NotNull(profile);
        Assert.Equal(companionId, profile.Id);
        Assert.NotEmpty(profile.Name);
        Assert.NotEmpty(profile.Background);
        Assert.NotEmpty(profile.Secret);
    }

    [Fact]
    public void CompanionNpc_GetCompanionProfile_UnknownId_ReturnsNull()
    {
        var profile = CompanionNpcData.GetCompanionProfile("unknown_companion");
        Assert.Null(profile);
    }

    // =========================================================
    // α.35: CompanionNpcData - 仲間NPC会話
    // =========================================================

    [Theory]
    [InlineData("companion_rena")]
    [InlineData("companion_brom")]
    [InlineData("companion_syl")]
    [InlineData("companion_mika")]
    public void CompanionNpc_GetJoinDialogue_ReturnsNonEmpty(string companionId)
    {
        var text = CompanionNpcData.GetJoinDialogue(companionId);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("companion_rena", 0)]
    [InlineData("companion_rena", 1)]
    [InlineData("companion_brom", 0)]
    [InlineData("companion_syl", 0)]
    [InlineData("companion_mika", 0)]
    public void CompanionNpc_GetCompanionIdleDialogue_ReturnsNonEmpty(string companionId, int loopCount)
    {
        var text = CompanionNpcData.GetCompanionIdleDialogue(companionId, loopCount);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("companion_rena", 1)]
    [InlineData("companion_rena", 2)]
    [InlineData("companion_rena", 3)]
    [InlineData("companion_brom", 1)]
    [InlineData("companion_syl", 3)]
    [InlineData("companion_mika", 3)]
    public void CompanionNpc_GetCompanionAffinityDialogue_ReturnsNonEmpty(string companionId, int affinityLevel)
    {
        var text = CompanionNpcData.GetCompanionAffinityDialogue(companionId, affinityLevel);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("companion_rena", "ally_low_hp")]
    [InlineData("companion_brom", "enemy_killed")]
    [InlineData("companion_syl", "low_hp_self")]
    [InlineData("companion_mika", "ally_low_hp")]
    public void CompanionNpc_GetCompanionBattleDialogue_ReturnsNonEmpty(string companionId, string situation)
    {
        var text = CompanionNpcData.GetCompanionBattleDialogue(companionId, situation);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.36: CompanionNpcData - 傭兵NPC台詞
    // =========================================================

    [Theory]
    [InlineData("swordsman")]
    [InlineData("archer")]
    [InlineData("mage")]
    [InlineData("tank")]
    [InlineData("healer")]
    public void CompanionNpc_GetMercenaryHireDialogue_ReturnsNonEmpty(string mercenaryType)
    {
        var text = CompanionNpcData.GetMercenaryHireDialogue(mercenaryType);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("swordsman", true)]
    [InlineData("swordsman", false)]
    [InlineData("archer", true)]
    [InlineData("tank", true)]
    [InlineData("healer", false)]
    public void CompanionNpc_GetMercenaryFarewellDialogue_ReturnsNonEmpty(string mercType, bool positive)
    {
        var text = CompanionNpcData.GetMercenaryFarewellDialogue(mercType, positive);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.37: SpecialNpcData - 吟遊詩人
    // =========================================================

    [Theory]
    [InlineData("greeting", 0)]
    [InlineData("greeting", 1)]
    [InlineData("information")]
    [InlineData("song_hero")]
    [InlineData("song_tragedy")]
    [InlineData("farewell")]
    public void SpecialNpc_GetBardDialogue_ReturnsNonEmpty(string situation, int loopCount = 0)
    {
        var text = SpecialNpcData.GetBardDialogue(situation, loopCount);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("origin_of_dungeon")]
    [InlineData("ancient_war")]
    [InlineData("forgotten_hero")]
    public void SpecialNpc_GetBardLoreText_ReturnsNonEmpty(string loreId)
    {
        var text = SpecialNpcData.GetBardLoreText(loreId);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.38: SpecialNpcData - 賞金稼ぎ
    // =========================================================

    [Theory]
    [InlineData("greeting")]
    [InlineData("offer_bounty")]
    [InlineData("accept_bounty")]
    [InlineData("reject_bounty")]
    [InlineData("captured_target")]
    [InlineData("farewell")]
    public void SpecialNpc_GetBountyHunterDialogue_ReturnsNonEmpty(string situation)
    {
        var text = SpecialNpcData.GetBountyHunterDialogue(situation);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("criminal_simple", "low")]
    [InlineData("serial_killer", "high")]
    [InlineData("escaped_convict", "medium")]
    [InlineData("monster_elite", "high")]
    public void SpecialNpc_GetWantedPosterText_ReturnsNonEmpty(string targetType, string rewardLevel)
    {
        var text = SpecialNpcData.GetWantedPosterText(targetType, rewardLevel);
        Assert.NotEmpty(text);
        Assert.NotEqual("", text);
    }

    // =========================================================
    // α.39: SpecialNpcData - 占い師
    // =========================================================

    [Theory]
    [InlineData("greeting")]
    [InlineData("first_reading")]
    [InlineData("warning")]
    [InlineData("hope")]
    [InlineData("loop_awareness")]
    [InlineData("payment_refused")]
    public void SpecialNpc_GetFortuneTellerDialogue_ReturnsNonEmpty(string situation)
    {
        var text = SpecialNpcData.GetFortuneTellerDialogue(situation);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void SpecialNpc_GetFortuneTexts_Returns10Entries()
    {
        var texts = SpecialNpcData.GetFortuneTexts();
        Assert.Equal(10, texts.Count);
        foreach (var text in texts)
            Assert.NotEmpty(text);
    }

    // =========================================================
    // α.40: SpecialNpcData - 亡霊NPC
    // =========================================================

    [Theory]
    [InlineData("ghost_soldier", "first_encounter")]
    [InlineData("ghost_soldier", "request")]
    [InlineData("ghost_soldier", "quest_complete")]
    [InlineData("ghost_merchant", "first_encounter")]
    [InlineData("ghost_mage", "knowledge_transfer")]
    [InlineData("ghost_child", "first_encounter")]
    [InlineData("ghost_child", "quest_complete")]
    public void SpecialNpc_GetGhostDialogue_ReturnsNonEmpty(string ghostId, string situation)
    {
        var text = SpecialNpcData.GetGhostDialogue(ghostId, situation);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("ghost_soldier")]
    [InlineData("ghost_merchant")]
    [InlineData("ghost_mage")]
    [InlineData("ghost_child")]
    public void SpecialNpc_GetGhostMemoryText_ReturnsNonEmpty(string ghostId)
    {
        var text = SpecialNpcData.GetGhostMemoryText(ghostId);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.41: DungeonLocationData - ダンジョン描写
    // =========================================================

    [Theory]
    [InlineData(20, "dragon")]
    [InlineData(15, "lich")]
    [InlineData(10, "golem")]
    [InlineData(25, "demon_lord")]
    [InlineData(30, "ancient_guardian")]
    public void DungeonLocation_GetBossFloorDescription_ReturnsNonEmpty(int depth, string bossType)
    {
        var text = DungeonLocationData.GetBossFloorDescription(depth, bossType);
        Assert.NotEmpty(text);
        Assert.Contains(depth.ToString(), text);
    }

    [Theory]
    [InlineData(SpecialFloorType.Shop)]
    [InlineData(SpecialFloorType.TreasureVault)]
    [InlineData(SpecialFloorType.BossRoom)]
    [InlineData(SpecialFloorType.RestPoint)]
    [InlineData(SpecialFloorType.Arena)]
    [InlineData(SpecialFloorType.Library)]
    public void DungeonLocation_GetSpecialFloorDescription_AllTypes_ReturnsNonEmpty(SpecialFloorType floorType)
    {
        var text = DungeonLocationData.GetSpecialFloorDescription(floorType);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void DungeonLocation_GetDungeonDepthDescription_ReturnsNonEmpty(int depth)
    {
        var text = DungeonLocationData.GetDungeonDepthDescription(depth);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.42: DungeonLocationData - 街マップNPC配置
    // =========================================================

    [Theory]
    [InlineData(TerritoryId.Capital, "guild")]
    [InlineData(TerritoryId.Capital, "inn")]
    [InlineData(TerritoryId.Forest, "guild")]
    [InlineData(TerritoryId.Mountain, "blacksmith")]
    [InlineData(TerritoryId.Coast, "port")]
    [InlineData(TerritoryId.Southern, "market")]
    [InlineData(TerritoryId.Frontier, "fortress")]
    public void DungeonLocation_GetTownFacilityDescription_ReturnsNonEmpty(TerritoryId territory, string facility)
    {
        var text = DungeonLocationData.GetTownFacilityDescription(territory, facility);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void DungeonLocation_GetTownPasserbyDialogue_AllTerritories_ReturnsNonEmpty(TerritoryId territory)
    {
        var text = DungeonLocationData.GetTownPasserbyDialogue(territory);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.43: DungeonLocationData - 遺産・遺跡発見
    // =========================================================

    [Theory]
    [InlineData(1, "monster")]
    [InlineData(1, "trap")]
    [InlineData(1, "starvation")]
    [InlineData(2, "monster")]
    [InlineData(3, "trap")]
    public void DungeonLocation_GetPreviousDeathMemoText_ReturnsNonEmpty(int loopCount, string cause)
    {
        var text = DungeonLocationData.GetPreviousDeathMemoText(loopCount, cause);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(99)]
    public void DungeonLocation_GetAdventurerFarewellNoteText_AllIndices_ReturnsNonEmpty(int index)
    {
        var text = DungeonLocationData.GetAdventurerFarewellNoteText(index);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData("temple")]
    [InlineData("laboratory")]
    [InlineData("graveyard")]
    [InlineData("fortress")]
    [InlineData("archive")]
    public void DungeonLocation_GetAncientRuinsDiscoveryText_ReturnsNonEmpty(string ruinsType)
    {
        var text = DungeonLocationData.GetAncientRuinsDiscoveryText(ruinsType);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.44: SeasonalEventData - 季節別NPC台詞
    // =========================================================

    [Theory]
    [InlineData("npc_leon", Season.Spring)]
    [InlineData("npc_leon", Season.Winter)]
    [InlineData("npc_marco", Season.Summer)]
    [InlineData("npc_marco", Season.Autumn)]
    [InlineData("npc_marvin", Season.Spring)]
    [InlineData("npc_hassan", Season.Winter)]
    public void SeasonalEvent_GetSeasonalNpcGreeting_KnownNpcs_ReturnsNonEmpty(string npcId, Season season)
    {
        var text = SeasonalEventData.GetSeasonalNpcGreeting(npcId, season);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Season.Spring, TerritoryId.Forest)]
    [InlineData(Season.Summer, TerritoryId.Coast)]
    [InlineData(Season.Autumn, TerritoryId.Mountain)]
    [InlineData(Season.Winter, TerritoryId.Frontier)]
    [InlineData(Season.Spring, TerritoryId.Capital)]
    [InlineData(Season.Winter, TerritoryId.Capital)]
    public void SeasonalEvent_GetSeasonAtmosphereText_ReturnsNonEmpty(Season season, TerritoryId territory)
    {
        var text = SeasonalEventData.GetSeasonAtmosphereText(season, territory);
        Assert.NotEmpty(text);
    }

    // =========================================================
    // α.45: SeasonalEventData - 季節イベント
    // =========================================================

    [Theory]
    [InlineData("spring_festival", Season.Spring)]
    [InlineData("summer_trial", Season.Summer)]
    [InlineData("harvest_festival", Season.Autumn)]
    [InlineData("winter_light_festival", Season.Winter)]
    [InlineData("spring_planting", Season.Spring)]
    [InlineData("winter_forge_festival", Season.Winter)]
    [InlineData("survival_festival", Season.Winter)]
    public void SeasonalEvent_GetSeasonalEventText_ReturnsNonEmpty(string eventId, Season season)
    {
        var text = SeasonalEventData.GetSeasonalEventText(eventId, season);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Season.Spring, "weather_change")]
    [InlineData(Season.Summer, "heat_fatigue")]
    [InlineData(Season.Autumn, "harvest_bonus")]
    [InlineData(Season.Winter, "cold_damage")]
    public void SeasonalEvent_GetSeasonalEffectText_ReturnsNonEmpty(Season season, string effectType)
    {
        var text = SeasonalEventData.GetSeasonalEffectText(season, effectType);
        Assert.NotEmpty(text);
    }

    [Theory]
    [InlineData(Season.Winter, Season.Spring)]
    [InlineData(Season.Spring, Season.Summer)]
    [InlineData(Season.Summer, Season.Autumn)]
    [InlineData(Season.Autumn, Season.Winter)]
    public void SeasonalEvent_GetSeasonTransitionText_ReturnsNonEmpty(Season from, Season to)
    {
        var text = SeasonalEventData.GetSeasonTransitionText(from, to);
        Assert.NotEmpty(text);
    }
}
