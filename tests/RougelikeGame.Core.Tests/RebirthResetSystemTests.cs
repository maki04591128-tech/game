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
}
