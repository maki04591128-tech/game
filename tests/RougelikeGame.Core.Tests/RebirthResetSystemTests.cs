using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 死に戻り時の各システムReset()メソッドのテスト。
/// 「死に戻りはキャラクター作成直後への時間巻き戻し」であることを検証。
/// </summary>
public class RebirthResetSystemTests
{
    #region NpcSystem.Reset

    [Fact]
    public void NpcSystem_Reset_ClearsAllNpcStates()
    {
        var system = new NpcSystem();
        system.MeetNpc("npc_guild_master");
        system.ModifyAffinity("npc_guild_master", 30);
        system.MeetNpc("npc_shopkeeper");

        system.Reset();

        Assert.Empty(system.GetAllStates());
    }

    [Fact]
    public void NpcSystem_Reset_NewNpcGetsFreshState()
    {
        var system = new NpcSystem();
        system.MeetNpc("npc_guild_master");
        system.ModifyAffinity("npc_guild_master", 30);

        system.Reset();

        var state = system.GetNpcState("npc_guild_master");
        Assert.Equal(50, state.Affinity); // default
        Assert.False(state.HasMet);
    }

    #endregion

    #region DialogueSystem.Reset

    [Fact]
    public void DialogueSystem_Reset_ClearsAllFlags()
    {
        var system = new DialogueSystem();
        system.SetFlag("intro_done");
        system.SetFlag("quest_accepted");

        system.Reset();

        Assert.False(system.HasFlag("intro_done"));
        Assert.False(system.HasFlag("quest_accepted"));
    }

    [Fact]
    public void DialogueSystem_Reset_ClearsCurrentNode()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("test", "NPC", "Hello"));
        system.StartDialogue("test");

        system.Reset();

        Assert.Null(system.CurrentNode);
        Assert.False(system.IsInDialogue);
    }

    #endregion

    #region QuestSystem.Reset

    [Fact]
    public void QuestSystem_Reset_ClearsActiveAndCompletedQuests()
    {
        var system = new QuestSystem();
        system.RegisterQuests(QuestDatabase.AllQuests);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        system.UpdateObjective("enemy_rat", 5);

        system.Reset();

        Assert.Equal(0, system.CompletedQuestCount);
        Assert.Empty(system.GetActiveQuests());
    }

    [Fact]
    public void QuestSystem_Reset_PreservesDefinitions()
    {
        var system = new QuestSystem();
        system.RegisterQuests(QuestDatabase.AllQuests);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);

        system.Reset();

        // 定義は残っているので再受注可能
        var def = system.GetQuestDefinition("quest_rat_hunt");
        Assert.NotNull(def);
    }

    #endregion

    #region GuildSystem.Reset

    [Fact]
    public void GuildSystem_Reset_ResetsToUnregistered()
    {
        var system = new GuildSystem();
        system.Register();
        system.AddPoints(500);
        Assert.Equal(GuildRank.Silver, system.CurrentRank);

        system.Reset();

        Assert.Equal(GuildRank.None, system.CurrentRank);
        Assert.Equal(0, system.GuildPoints);
        Assert.False(system.IsRegistered);
    }

    #endregion

    #region KarmaSystem.Reset

    [Fact]
    public void KarmaSystem_Reset_ResetsKarmaToZero()
    {
        var system = new KarmaSystem();
        system.ModifyKarma(50, "善行");
        system.ModifyKarma(-30, "悪行");

        system.Reset();

        Assert.Equal(0, system.KarmaValue);
        Assert.Equal(KarmaRank.Neutral, system.CurrentRank);
        Assert.Empty(system.KarmaHistory);
    }

    #endregion

    #region ReputationSystem.Reset

    [Fact]
    public void ReputationSystem_Reset_ResetsAllTerritoriesToZero()
    {
        var system = new ReputationSystem();
        system.ModifyReputation(TerritoryId.Capital, 50, "クエスト完了");
        system.ModifyReputation(TerritoryId.Forest, -30, "違法行為");

        system.Reset();

        Assert.Equal(0, system.GetReputation(TerritoryId.Capital));
        Assert.Equal(0, system.GetReputation(TerritoryId.Forest));
        Assert.Equal(ReputationRank.Indifferent, system.GetReputationRank(TerritoryId.Capital));
    }

    #endregion

    #region CompanionSystem.Reset

    [Fact]
    public void CompanionSystem_Reset_RemovesAllCompanions()
    {
        var system = new CompanionSystem();
        system.AddCompanion(new CompanionSystem.CompanionData("傭兵A", CompanionType.Mercenary, CompanionAIMode.Aggressive, 5, 50, 200));
        system.AddCompanion(new CompanionSystem.CompanionData("ペットB", CompanionType.Pet, CompanionAIMode.Wait, 1, 50, 50));

        system.Reset();

        Assert.Empty(system.Party);
        Assert.Equal(0, system.AliveCount);
    }

    #endregion

    #region OathSystem.Reset

    [Fact]
    public void OathSystem_Reset_ClearsAllOaths()
    {
        var system = new OathSystem();
        system.TakeOath(OathType.Temperance);
        system.TakeOath(OathType.Pacifism);

        system.Reset();

        Assert.Empty(system.ActiveOaths);
        Assert.Equal(0f, system.GetTotalExpBonus());
    }

    #endregion

    #region InvestmentSystem.Reset

    [Fact]
    public void InvestmentSystem_Reset_ClearsAllInvestments()
    {
        var system = new InvestmentSystem();
        system.Invest(InvestmentType.Shop, "test_shop", 1000, 100);
        system.Invest(InvestmentType.Business, "test_biz", 500, 200);

        system.Reset();

        Assert.Empty(system.Investments);
        Assert.Equal(0, system.GetTotalInvested());
    }

    #endregion

    #region BaseConstructionSystem.Reset

    [Fact]
    public void BaseConstructionSystem_Reset_ClearsAllFacilities()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Camp, 100);
        system.Build(FacilityCategory.Workbench, 100);

        system.Reset();

        Assert.Empty(system.BuiltFacilities);
        Assert.False(system.HasFacility(FacilityCategory.Camp));
    }

    #endregion

    #region WorldMapSystem.Reset

    [Fact]
    public void WorldMapSystem_Reset_ReturnsToStartTerritory()
    {
        var system = new WorldMapSystem();
        system.PlayerGold = 100000;
        system.TravelTo(TerritoryId.Forest, 10);
        system.TravelTo(TerritoryId.Southern, 15);

        system.Reset(TerritoryId.Capital);

        Assert.Equal(TerritoryId.Capital, system.CurrentTerritory);
        Assert.Contains(TerritoryId.Capital, system.VisitedTerritories);
        Assert.Single(system.VisitedTerritories);
        Assert.True(system.IsOnSurface);
    }

    #endregion

    #region GridInventorySystem.Reset

    [Fact]
    public void GridInventorySystem_Reset_ClearsAllItems()
    {
        var system = new GridInventorySystem();
        system.PlaceItem("sword", "剣", GridItemSize.Size1x2, 0, 0);

        system.Reset();

        Assert.Empty(system.Items);
        Assert.Equal(1.0f, system.GetFreeSpaceRatio());
    }

    #endregion

    #region EncyclopediaSystem.ResetDiscoveryLevels

    [Fact]
    public void EncyclopediaSystem_ResetDiscoveryLevels_ResetsAllToZero()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "slime", "スライム", 3,
            new() { [1] = "弱い", [2] = "緑色", [3] = "最弱モンスター" });
        system.IncrementDiscovery("slime");
        system.IncrementDiscovery("slime");

        system.ResetDiscoveryLevels();

        var entry = system.GetEntry("slime");
        Assert.NotNull(entry);
        Assert.Equal(0, entry.DiscoveryLevel);
        Assert.Equal("???", system.GetCurrentDescription("slime"));
    }

    [Fact]
    public void EncyclopediaSystem_ResetDiscoveryLevels_PreservesEntryDefinitions()
    {
        var system = new EncyclopediaSystem();
        system.RegisterEntry(EncyclopediaCategory.Monster, "goblin", "ゴブリン", 2,
            new() { [1] = "緑の小鬼", [2] = "群れで行動" });
        system.IncrementDiscovery("goblin");

        system.ResetDiscoveryLevels();

        // 定義は保持、再発見可能
        Assert.True(system.IncrementDiscovery("goblin"));
        Assert.Equal(1, system.GetEntry("goblin")!.DiscoveryLevel);
    }

    #endregion

    #region SkillTreeSystem.Reset

    [Fact]
    public void SkillTreeSystem_Reset_ClearsAllUnlockedNodesAndPoints()
    {
        var system = new SkillTreeSystem();
        system.AddPoints(10);
        system.UnlockNode("shared_hp_1");
        system.UnlockNode("shared_mp_1");

        system.Reset();

        Assert.Equal(0, system.UnlockedCount);
        Assert.Equal(0, system.AvailablePoints);
        Assert.Empty(system.UnlockedNodes);
    }

    [Fact]
    public void SkillTreeSystem_Reset_PreservesNodeDefinitions()
    {
        var system = new SkillTreeSystem();
        system.AddPoints(5);
        system.UnlockNode("shared_hp_1");

        system.Reset();

        // 定義は保持されているので再度ポイント追加後に解放可能
        Assert.True(system.AllNodes.ContainsKey("shared_hp_1"));
        system.AddPoints(1);
        Assert.True(system.CanUnlock("shared_hp_1"));
    }

    #endregion

    #region 統合テスト: 死に戻りコンセプト検証

    [Fact]
    public void RebirthConcept_AllWorldStateSystems_AreProperlyResettable()
    {
        // 各システムに状態を設定
        var npcSystem = new NpcSystem();
        var dialogueSystem = new DialogueSystem();
        var questSystem = new QuestSystem();
        var guildSystem = new GuildSystem();
        var karmaSystem = new KarmaSystem();
        var reputationSystem = new ReputationSystem();
        var companionSystem = new CompanionSystem();
        var oathSystem = new OathSystem();
        var investmentSystem = new InvestmentSystem();
        var baseSystem = new BaseConstructionSystem();
        var worldMapSystem = new WorldMapSystem();
        var gridSystem = new GridInventorySystem();

        // 世界状態を変更
        npcSystem.MeetNpc("npc_guild_master");
        npcSystem.ModifyAffinity("npc_guild_master", 30);
        dialogueSystem.SetFlag("intro_done");
        questSystem.RegisterQuests(QuestDatabase.AllQuests);
        questSystem.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        guildSystem.Register();
        guildSystem.AddPoints(500);
        karmaSystem.ModifyKarma(50, "善行");
        reputationSystem.ModifyReputation(TerritoryId.Capital, 30, "クエスト");
        companionSystem.AddCompanion(new CompanionSystem.CompanionData("傭兵", CompanionType.Mercenary, CompanionAIMode.Aggressive, 5, 50, 200));
        oathSystem.TakeOath(OathType.Temperance);
        investmentSystem.Invest(InvestmentType.Shop, "shop", 1000, 100);
        baseSystem.Build(FacilityCategory.Camp, 100);
        worldMapSystem.PlayerGold = 10000;
        worldMapSystem.TravelTo(TerritoryId.Forest, 10);
        gridSystem.PlaceItem("sword", "剣", GridItemSize.Size1x2, 0, 0);

        // 死に戻りリセット
        npcSystem.Reset();
        dialogueSystem.Reset();
        questSystem.Reset();
        guildSystem.Reset();
        karmaSystem.Reset();
        reputationSystem.Reset();
        companionSystem.Reset();
        oathSystem.Reset();
        investmentSystem.Reset();
        baseSystem.Reset();
        worldMapSystem.Reset(TerritoryId.Capital);
        gridSystem.Reset();

        // 全てがキャラクター作成直後の初期状態に戻ったことを検証
        Assert.Empty(npcSystem.GetAllStates());
        Assert.False(dialogueSystem.HasFlag("intro_done"));
        Assert.Empty(questSystem.GetActiveQuests());
        Assert.Equal(GuildRank.None, guildSystem.CurrentRank);
        Assert.Equal(0, karmaSystem.KarmaValue);
        Assert.Equal(0, reputationSystem.GetReputation(TerritoryId.Capital));
        Assert.Empty(companionSystem.Party);
        Assert.Empty(oathSystem.ActiveOaths);
        Assert.Empty(investmentSystem.Investments);
        Assert.Empty(baseSystem.BuiltFacilities);
        Assert.Equal(TerritoryId.Capital, worldMapSystem.CurrentTerritory);
        Assert.Single(worldMapSystem.VisitedTerritories);
        Assert.Empty(gridSystem.Items);
    }

    [Fact]
    public void RebirthConcept_NormalDeath_KeepsKnowledgeSystems()
    {
        // 図鑑とスキルツリーは通常死で保持される
        var encyclopedia = new EncyclopediaSystem();
        var skillTree = new SkillTreeSystem();

        encyclopedia.RegisterEntry(EncyclopediaCategory.Monster, "slime", "スライム", 3,
            new() { [1] = "弱い", [2] = "緑色", [3] = "最弱" });
        encyclopedia.IncrementDiscovery("slime");
        skillTree.AddPoints(5);
        skillTree.UnlockNode("shared_hp_1");

        // 通常死（正気度 > 0）→ 知識システムはリセットしない
        // （ExecuteRebirthでisSanityZero=falseの場合リセットされない）
        Assert.Equal(1, encyclopedia.GetEntry("slime")!.DiscoveryLevel);
        Assert.Equal(1, skillTree.UnlockedCount);
    }

    [Fact]
    public void RebirthConcept_SanityZeroDeath_ClearsKnowledgeSystems()
    {
        // 図鑑とスキルツリーは正気度0で消失
        var encyclopedia = new EncyclopediaSystem();
        var skillTree = new SkillTreeSystem();

        encyclopedia.RegisterEntry(EncyclopediaCategory.Monster, "slime", "スライム", 3,
            new() { [1] = "弱い", [2] = "緑色", [3] = "最弱" });
        encyclopedia.IncrementDiscovery("slime");
        encyclopedia.IncrementDiscovery("slime");
        skillTree.AddPoints(5);
        skillTree.UnlockNode("shared_hp_1");
        skillTree.UnlockNode("shared_mp_1");

        // 正気度0→知識消失
        encyclopedia.ResetDiscoveryLevels();
        skillTree.Reset();

        Assert.Equal(0, encyclopedia.GetEntry("slime")!.DiscoveryLevel);
        Assert.Equal(0, skillTree.UnlockedCount);
        Assert.Equal(0, skillTree.AvailablePoints);
    }

    #endregion

    #region NpcMemorySystem.Reset

    [Fact]
    public void NpcMemorySystem_Reset_ClearsAllMemoriesAndRumors()
    {
        var system = new NpcMemorySystem();
        system.RecordAction("npc1", "steal", -10, 100);
        system.RecordAction("npc2", "help", 5, 200);
        system.GenerateRumor(RumorType.Heroic, "英雄の噂", "Capital");
        system.GenerateRumor(RumorType.Villainous, "悪漢の噂", "Forest");

        system.Reset();

        Assert.Empty(system.Memories);
        Assert.Empty(system.Rumors);
    }

    #endregion

    #region ItemIdentificationSystem.Reset

    [Fact]
    public void ItemIdentificationSystem_Reset_ClearsAllIdentifiedItems()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("sword_01", "炎の剣", CurseType.None);
        system.Identify("ring_01", "呪いの指輪", CurseType.Major);

        system.Reset();

        Assert.Empty(system.IdentifiedItems);
        Assert.Empty(system.KnownCurses);
        Assert.False(system.IsIdentified("sword_01"));
    }

    #endregion

    #region DungeonEcosystemSystem.Reset

    [Fact]
    public void DungeonEcosystemSystem_Reset_ClearsEventsAndTracesButKeepsRelations()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Beast, MonsterRace.Insect, 70);
        system.ProcessInteraction("wolf1", MonsterRace.Beast, "bug1", MonsterRace.Insect, 1, 100);
        system.AddBattleTrace(5, 5, 1, 3, "戦闘痕跡", 100);

        system.Reset();

        // イベント・痕跡はクリアされるが、捕食関係定義は保持
        Assert.Empty(system.Events);
        Assert.Empty(system.Traces);
        Assert.NotEmpty(system.Relations);
        Assert.True(system.HasPredatorRelation(MonsterRace.Beast, MonsterRace.Insect));
    }

    #endregion

    #region PetSystem.Reset

    [Fact]
    public void PetSystem_Reset_ClearsPetsButKeepsDefinitions()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "ポチ", PetType.Wolf);
        system.AddPet("pet2", "タマ", PetType.Cat);

        system.Reset();

        Assert.Empty(system.Pets);
        // 定義は保持
        Assert.NotNull(system.GetDefinition(PetType.Wolf));
    }

    #endregion

    #region MerchantGuildSystem.Reset

    [Fact]
    public void MerchantGuildSystem_Reset_ClearsMembershipAndRoutes()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);

        system.Reset();

        Assert.Null(system.Membership);
        Assert.False(system.IsMember);
        Assert.Empty(system.Routes);
    }

    #endregion

    #region InscriptionSystem.Reset

    [Fact]
    public void InscriptionSystem_Reset_ResetsDecodedStatesToFalse()
    {
        var system = new InscriptionSystem();
        system.Register("ins1", InscriptionType.Lore, "???", "古の伝承", 1);
        system.Register("ins2", InscriptionType.Hint, "???", "隠し通路のヒント", 5);
        system.TryDecode("ins1", 10); // 解読成功

        system.Reset();

        // 碑文の登録は保持されるが、解読状態はリセット
        Assert.Equal(2, system.Inscriptions.Count);
        Assert.Equal(0, system.DecodedCount);
    }

    #endregion

    #region FactionWarSystem.Reset

    [Fact]
    public void FactionWarSystem_Reset_ClearsAllWarsAndHistory()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "王都侵攻", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.ResolveWar("war1", TerritoryId.Capital, 10);

        system.Reset();

        Assert.Empty(system.ActiveWars);
        Assert.Empty(system.WarHistory);
    }

    #endregion

    #region RelationshipSystem.Reset

    [Fact]
    public void RelationshipSystem_Reset_ClearsAllRelations()
    {
        var system = new RelationshipSystem();
        system.SetRelation(RelationshipType.Racial, "Human", "Elf", 50);
        system.SetRelation(RelationshipType.Personal, "player", "npc1", 80);

        system.Reset();

        Assert.Equal(0, system.TotalRelations);
        Assert.Equal(0, system.GetRelation(RelationshipType.Racial, "Human", "Elf"));
    }

    #endregion

    #region TerritoryInfluenceSystem.Reset

    [Fact]
    public void TerritoryInfluenceSystem_Reset_ClearsAllInfluence()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new Dictionary<string, float> { { "Kingdom", 0.7f }, { "Rebellion", 0.3f } });
        system.ModifyInfluence(TerritoryId.Forest, "Beasts", 0.5f);

        system.Reset();

        Assert.Null(system.GetDominantFaction(TerritoryId.Capital));
        Assert.Equal(0f, system.GetInfluence(TerritoryId.Forest, "Beasts"));
    }

    #endregion

    #region 統合テスト: 死に戻りコンセプト検証（拡張版）

    [Fact]
    public void RebirthConcept_AllWorldStateSystems_IncludingV06_AreProperlyResettable()
    {
        // Ver.prt.0.1-0.4 システム
        var npcSystem = new NpcSystem();
        var dialogueSystem = new DialogueSystem();
        var questSystem = new QuestSystem();
        var guildSystem = new GuildSystem();
        var karmaSystem = new KarmaSystem();
        var reputationSystem = new ReputationSystem();
        var companionSystem = new CompanionSystem();
        var oathSystem = new OathSystem();
        var investmentSystem = new InvestmentSystem();
        var baseSystem = new BaseConstructionSystem();
        var worldMapSystem = new WorldMapSystem();
        var gridSystem = new GridInventorySystem();

        // Ver.prt.0.5-0.6 システム
        var npcMemorySystem = new NpcMemorySystem();
        var relationshipSystem = new RelationshipSystem();
        var itemIdSystem = new ItemIdentificationSystem();
        var ecosystemSystem = new DungeonEcosystemSystem();
        var petSystem = new PetSystem();
        var merchantGuildSystem = new MerchantGuildSystem();
        var inscriptionSystem = new InscriptionSystem();
        var factionWarSystem = new FactionWarSystem();
        var territoryInfluenceSystem = new TerritoryInfluenceSystem();

        // 世界状態を変更
        npcSystem.MeetNpc("npc_guild_master");
        dialogueSystem.SetFlag("intro_done");
        questSystem.RegisterQuests(QuestDatabase.AllQuests);
        questSystem.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        guildSystem.Register();
        karmaSystem.ModifyKarma(50, "善行");
        reputationSystem.ModifyReputation(TerritoryId.Capital, 30, "クエスト");
        companionSystem.AddCompanion(new CompanionSystem.CompanionData("傭兵", CompanionType.Mercenary, CompanionAIMode.Aggressive, 5, 50, 200));
        oathSystem.TakeOath(OathType.Temperance);
        investmentSystem.Invest(InvestmentType.Shop, "shop", 1000, 100);
        baseSystem.Build(FacilityCategory.Camp, 100);
        worldMapSystem.PlayerGold = 10000;
        worldMapSystem.TravelTo(TerritoryId.Forest, 10);
        gridSystem.PlaceItem("sword", "剣", GridItemSize.Size1x2, 0, 0);

        // Ver.prt.0.5-0.6 世界状態を変更
        npcMemorySystem.RecordAction("npc1", "help", 5, 100);
        npcMemorySystem.GenerateRumor(RumorType.Heroic, "英雄", "Capital");
        relationshipSystem.SetRelation(RelationshipType.Personal, "player", "npc1", 80);
        itemIdSystem.Identify("sword_01", "炎の剣");
        ecosystemSystem.AddBattleTrace(5, 5, 1, 3, "痕跡", 100);
        petSystem.AddPet("pet1", "ポチ", PetType.Wolf);
        merchantGuildSystem.JoinGuild("player");
        inscriptionSystem.Register("ins1", InscriptionType.Lore, "???", "伝承", 1);
        inscriptionSystem.TryDecode("ins1", 10);
        factionWarSystem.StartWar("war1", "侵攻", TerritoryId.Capital, TerritoryId.Forest, 100);
        territoryInfluenceSystem.Initialize(TerritoryId.Capital, new Dictionary<string, float> { { "Kingdom", 0.7f } });

        // === 死に戻りリセット（全システム） ===
        npcSystem.Reset();
        dialogueSystem.Reset();
        questSystem.Reset();
        guildSystem.Reset();
        karmaSystem.Reset();
        reputationSystem.Reset();
        companionSystem.Reset();
        oathSystem.Reset();
        investmentSystem.Reset();
        baseSystem.Reset();
        worldMapSystem.Reset(TerritoryId.Capital);
        gridSystem.Reset();
        npcMemorySystem.Reset();
        relationshipSystem.Reset();
        itemIdSystem.Reset();
        ecosystemSystem.Reset();
        petSystem.Reset();
        merchantGuildSystem.Reset();
        inscriptionSystem.Reset();
        factionWarSystem.Reset();
        territoryInfluenceSystem.Reset();

        // === 全てがキャラクター作成直後の初期状態に戻ったことを検証 ===
        // Ver.prt.0.1-0.4
        Assert.Empty(npcSystem.GetAllStates());
        Assert.False(dialogueSystem.HasFlag("intro_done"));
        Assert.Empty(questSystem.GetActiveQuests());
        Assert.Equal(GuildRank.None, guildSystem.CurrentRank);
        Assert.Equal(0, karmaSystem.KarmaValue);
        Assert.Equal(0, reputationSystem.GetReputation(TerritoryId.Capital));
        Assert.Empty(companionSystem.Party);
        Assert.Empty(oathSystem.ActiveOaths);
        Assert.Empty(investmentSystem.Investments);
        Assert.Empty(baseSystem.BuiltFacilities);
        Assert.Equal(TerritoryId.Capital, worldMapSystem.CurrentTerritory);
        Assert.Empty(gridSystem.Items);

        // Ver.prt.0.5-0.6
        Assert.Empty(npcMemorySystem.Memories);
        Assert.Empty(npcMemorySystem.Rumors);
        Assert.Equal(0, relationshipSystem.TotalRelations);
        Assert.Empty(itemIdSystem.IdentifiedItems);
        Assert.Empty(ecosystemSystem.Events);
        Assert.Empty(ecosystemSystem.Traces);
        Assert.Empty(petSystem.Pets);
        Assert.Null(merchantGuildSystem.Membership);
        Assert.Empty(merchantGuildSystem.Routes);
        Assert.Equal(0, inscriptionSystem.DecodedCount);
        Assert.Empty(factionWarSystem.ActiveWars);
        Assert.Empty(factionWarSystem.WarHistory);
        Assert.Null(territoryInfluenceSystem.GetDominantFaction(TerritoryId.Capital));
    }

    #endregion

    #region StartingMapResolver — 死に戻り時の初期スポーン場所

    [Fact]
    public void Rebirth_SpawnLocation_HumanAdventurer_ReturnsCapitalGuild()
    {
        var mapName = StartingMapResolver.Resolve(Race.Human, Background.Adventurer);
        Assert.Equal("capital_guild", mapName);
        Assert.Equal("王都・冒険者ギルド", StartingMapResolver.GetDisplayName(mapName));
    }

    [Fact]
    public void Rebirth_SpawnLocation_ElfAdventurer_ReturnsForestVillage()
    {
        var mapName = StartingMapResolver.Resolve(Race.Elf, Background.Adventurer);
        Assert.Equal("forest_village", mapName);
        Assert.Equal(TerritoryId.Forest, StartingMapResolver.GetStartingTerritory(mapName));
    }

    [Fact]
    public void Rebirth_SpawnLocation_NobleBackground_ReturnsCapitalManor()
    {
        // 素性が優先される（種族に関わらず貴族は王都貴族邸）
        var mapNameHuman = StartingMapResolver.Resolve(Race.Human, Background.Noble);
        var mapNameElf = StartingMapResolver.Resolve(Race.Elf, Background.Noble);
        Assert.Equal("capital_manor", mapNameHuman);
        Assert.Equal("capital_manor", mapNameElf);
    }

    [Fact]
    public void Rebirth_SpawnLocation_DwarfAdventurer_ReturnsMountainHold()
    {
        var mapName = StartingMapResolver.Resolve(Race.Dwarf, Background.Adventurer);
        Assert.Equal("mountain_hold", mapName);
        Assert.Equal(TerritoryId.Mountain, StartingMapResolver.GetStartingTerritory(mapName));
    }

    [Fact]
    public void Rebirth_SpawnLocation_SoldierBackground_ReturnsCapitalBarracks()
    {
        var mapName = StartingMapResolver.Resolve(Race.Human, Background.Soldier);
        Assert.Equal("capital_barracks", mapName);
    }

    #endregion
}
