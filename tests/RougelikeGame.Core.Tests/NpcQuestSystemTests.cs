using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// NPC・会話・クエスト・ギルドシステムのテスト (Phase 5.18-5.21)
/// </summary>
public class NpcQuestSystemTests
{
    private static Player CreateTestPlayer()
    {
        return Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
    }

    #region NpcDefinition

    [Fact]
    public void NpcDefinition_GetAll_Returns16Npcs()
    {
        var all = NpcDefinition.GetAll();
        Assert.Equal(16, all.Count);
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Capital_ReturnsCorrectNpcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Capital);
        Assert.Equal(4, npcs.Count);
        Assert.Contains(npcs, n => n.Id == "npc_guild_master");
        Assert.Contains(npcs, n => n.Id == "npc_capital_shopkeeper");
        Assert.Contains(npcs, n => n.Id == "npc_capital_priest");
        Assert.Contains(npcs, n => n.Id == "npc_capital_sage");
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Forest_Returns3Npcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Forest);
        Assert.Equal(3, npcs.Count);
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Mountain_Returns2Npcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Mountain);
        Assert.Equal(2, npcs.Count);
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Coast_Returns3Npcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Coast);
        Assert.Equal(3, npcs.Count);
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Southern_Returns2Npcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Southern);
        Assert.Equal(2, npcs.Count);
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_Frontier_Returns2Npcs()
    {
        var npcs = NpcDefinition.GetByTerritory(TerritoryId.Frontier);
        Assert.Equal(2, npcs.Count);
    }

    [Fact]
    public void NpcDefinition_GetById_ValidId_ReturnsNpc()
    {
        var npc = NpcDefinition.GetById("npc_guild_master");
        Assert.NotNull(npc);
        Assert.Equal("ギルドマスター・レオン", npc.Name);
        Assert.Equal(NpcType.GuildMaster, npc.Type);
    }

    [Fact]
    public void NpcDefinition_GetById_InvalidId_ReturnsNull()
    {
        var npc = NpcDefinition.GetById("invalid_npc");
        Assert.Null(npc);
    }

    [Fact]
    public void NpcDefinition_GetAffinityRank_AllRanks()
    {
        Assert.Equal("敵意", NpcDefinition.GetAffinityRank(10));
        Assert.Equal("警戒", NpcDefinition.GetAffinityRank(30));
        Assert.Equal("普通", NpcDefinition.GetAffinityRank(50));
        Assert.Equal("友好", NpcDefinition.GetAffinityRank(70));
        Assert.Equal("親友", NpcDefinition.GetAffinityRank(90));
    }

    #endregion

    #region NpcSystem

    [Fact]
    public void NpcSystem_GetNpcState_InitializesWithDefaults()
    {
        var system = new NpcSystem();
        var state = system.GetNpcState("npc_test");

        Assert.Equal("npc_test", state.NpcId);
        Assert.Equal(50, state.Affinity);
        Assert.False(state.HasMet);
        Assert.Empty(state.CompletedDialogues);
    }

    [Fact]
    public void NpcSystem_ModifyAffinity_IncreasesAndClamps()
    {
        var system = new NpcSystem();
        system.ModifyAffinity("npc_test", 30);
        Assert.Equal(80, system.GetNpcState("npc_test").Affinity);

        system.ModifyAffinity("npc_test", 50); // 80+50=130 → clamp to 100
        Assert.Equal(100, system.GetNpcState("npc_test").Affinity);
    }

    [Fact]
    public void NpcSystem_ModifyAffinity_DecreasesAndClamps()
    {
        var system = new NpcSystem();
        system.ModifyAffinity("npc_test", -30);
        Assert.Equal(20, system.GetNpcState("npc_test").Affinity);

        system.ModifyAffinity("npc_test", -50); // 20-50=-30 → clamp to 0
        Assert.Equal(0, system.GetNpcState("npc_test").Affinity);
    }

    [Fact]
    public void NpcSystem_MeetNpc_SetsHasMet()
    {
        var system = new NpcSystem();
        system.MeetNpc("npc_test");
        Assert.True(system.GetNpcState("npc_test").HasMet);
    }

    [Fact]
    public void NpcSystem_CreateTransferData_Transfers80Percent()
    {
        var system = new NpcSystem();
        system.ModifyAffinity("npc_a", 30); // 80
        system.ModifyAffinity("npc_b", -20); // 30

        var data = system.CreateTransferData();
        Assert.Equal(64, data["npc_a"]); // 80 * 0.8 = 64
        Assert.Equal(24, data["npc_b"]); // 30 * 0.8 = 24
    }

    [Fact]
    public void NpcSystem_ApplyTransferData_RestoresAffinity()
    {
        var system = new NpcSystem();
        var data = new Dictionary<string, int> { ["npc_a"] = 70, ["npc_b"] = 40 };

        system.ApplyTransferData(data);

        Assert.Equal(70, system.GetNpcState("npc_a").Affinity);
        Assert.Equal(40, system.GetNpcState("npc_b").Affinity);
    }

    [Fact]
    public void NpcSystem_GetAllStates_ReturnsAllTracked()
    {
        var system = new NpcSystem();
        system.MeetNpc("npc_a");
        system.MeetNpc("npc_b");

        var all = system.GetAllStates();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void NpcSystem_RestoreStates_RestoresFromSaveData()
    {
        var system = new NpcSystem();
        var data = new Dictionary<string, NpcStateSaveData>
        {
            ["npc_a"] = new NpcStateSaveData { Affinity = 80, HasMet = true, CompletedDialogues = new() { "dlg_1" } },
            ["npc_b"] = new NpcStateSaveData { Affinity = 30, HasMet = false }
        };

        system.RestoreStates(data);

        Assert.Equal(80, system.GetNpcState("npc_a").Affinity);
        Assert.True(system.GetNpcState("npc_a").HasMet);
        Assert.Contains("dlg_1", system.GetNpcState("npc_a").CompletedDialogues);
        Assert.Equal(30, system.GetNpcState("npc_b").Affinity);
        Assert.False(system.GetNpcState("npc_b").HasMet);
    }

    #endregion

    #region DialogueSystem

    [Fact]
    public void DialogueSystem_RegisterAndStart_ReturnsNode()
    {
        var system = new DialogueSystem();
        var node = new DialogueNode("dlg_1", "テスト", "こんにちは");
        system.RegisterNode(node);

        var result = system.StartDialogue("dlg_1");
        Assert.NotNull(result);
        Assert.Equal("こんにちは", result.Text);
        Assert.True(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_StartDialogue_InvalidId_ReturnsNull()
    {
        var system = new DialogueSystem();
        var result = system.StartDialogue("invalid");
        Assert.Null(result);
        Assert.False(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_StartDialogue_ConditionNotMet_ReturnsNull()
    {
        var system = new DialogueSystem();
        var node = new DialogueNode("dlg_1", "テスト", "秘密の会話", ConditionFlag: "secret_flag");
        system.RegisterNode(node);

        Assert.Null(system.StartDialogue("dlg_1"));
    }

    [Fact]
    public void DialogueSystem_StartDialogue_ConditionMet_Succeeds()
    {
        var system = new DialogueSystem();
        var node = new DialogueNode("dlg_1", "テスト", "秘密の会話", ConditionFlag: "secret_flag");
        system.RegisterNode(node);
        system.SetFlag("secret_flag");

        Assert.NotNull(system.StartDialogue("dlg_1"));
    }

    [Fact]
    public void DialogueSystem_Advance_MovesToNextNode()
    {
        var system = new DialogueSystem();
        system.RegisterNodes(new[]
        {
            new DialogueNode("dlg_1", "A", "最初", NextNodeId: "dlg_2"),
            new DialogueNode("dlg_2", "A", "次")
        });

        system.StartDialogue("dlg_1");
        var next = system.Advance();

        Assert.NotNull(next);
        Assert.Equal("次", next!.Text);
    }

    [Fact]
    public void DialogueSystem_Advance_NoNext_EndsDialogue()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("dlg_1", "A", "終わり"));

        system.StartDialogue("dlg_1");
        var next = system.Advance();

        Assert.Null(next);
        Assert.False(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_SelectChoice_ValidChoice_ReturnsResult()
    {
        var system = new DialogueSystem();
        var choices = new[]
        {
            new DialogueChoice("はい", "dlg_yes", AffinityChange: 10),
            new DialogueChoice("いいえ", "dlg_no", AffinityChange: -5)
        };
        system.RegisterNodes(new[]
        {
            new DialogueNode("dlg_1", "A", "手伝ってくれるか？", Choices: choices),
            new DialogueNode("dlg_yes", "A", "ありがとう！"),
            new DialogueNode("dlg_no", "A", "残念だ...")
        });

        system.StartDialogue("dlg_1");
        var result = system.SelectChoice(0);

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal(10, result.AffinityChange);
        Assert.Equal("ありがとう！", result.NextNode!.Text);
    }

    [Fact]
    public void DialogueSystem_SelectChoice_InvalidIndex_ReturnsNull()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("dlg_1", "A", "テスト",
            Choices: new[] { new DialogueChoice("はい", "dlg_2") }));
        system.StartDialogue("dlg_1");

        Assert.Null(system.SelectChoice(5));
        Assert.Null(system.SelectChoice(-1));
    }

    [Fact]
    public void DialogueSystem_SelectChoice_RequiredFlagNotMet_Fails()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("dlg_1", "A", "テスト",
            Choices: new[] { new DialogueChoice("秘密", "dlg_2", RequiredFlag: "has_key") }));
        system.StartDialogue("dlg_1");

        var result = system.SelectChoice(0);
        Assert.NotNull(result);
        Assert.False(result!.Success);
    }

    [Fact]
    public void DialogueSystem_EndDialogue_ClearsState()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("dlg_1", "A", "テスト"));
        system.StartDialogue("dlg_1");

        system.EndDialogue();
        Assert.False(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_Flags_SetAndCheck()
    {
        var system = new DialogueSystem();
        Assert.False(system.HasFlag("test_flag"));

        system.SetFlag("test_flag");
        Assert.True(system.HasFlag("test_flag"));
    }

    [Fact]
    public void DialogueSystem_RestoreFlags_ClearsAndSetsNew()
    {
        var system = new DialogueSystem();
        system.SetFlag("old_flag");

        system.RestoreFlags(new[] { "new_flag_1", "new_flag_2" });

        Assert.False(system.HasFlag("old_flag"));
        Assert.True(system.HasFlag("new_flag_1"));
        Assert.True(system.HasFlag("new_flag_2"));
    }

    [Fact]
    public void DialogueSystem_GetAllFlags_ReturnsAllSet()
    {
        var system = new DialogueSystem();
        system.SetFlag("a");
        system.SetFlag("b");

        var flags = system.GetAllFlags();
        Assert.Equal(2, flags.Count);
        Assert.Contains("a", flags);
        Assert.Contains("b", flags);
    }

    #endregion

    #region QuestSystem

    [Fact]
    public void QuestSystem_RegisterAndAccept_Success()
    {
        var system = new QuestSystem();
        var quest = QuestDatabase.GetById("quest_rat_hunt")!;
        system.RegisterQuest(quest);

        var result = system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        Assert.True(result.Success);
        Assert.Contains("ネズミ退治", result.Message);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_UnknownId_Fails()
    {
        var system = new QuestSystem();
        var result = system.AcceptQuest("invalid", 1, GuildRank.Copper);
        Assert.False(result.Success);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_AlreadyAccepted_Fails()
    {
        var system = new QuestSystem();
        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);

        var result = system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        Assert.False(result.Success);
        Assert.Contains("受注済み", result.Message);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_LevelTooLow_Fails()
    {
        var system = new QuestSystem();
        system.RegisterQuest(QuestDatabase.GetById("quest_bandit_clear")!); // RequiredLevel: 5
        var result = system.AcceptQuest("quest_bandit_clear", 3, GuildRank.Iron);
        Assert.False(result.Success);
        Assert.Contains("レベルが足りない", result.Message);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_RankTooLow_Fails()
    {
        var system = new QuestSystem();
        system.RegisterQuest(QuestDatabase.GetById("quest_bandit_clear")!); // RequiredGuildRank: Iron
        var result = system.AcceptQuest("quest_bandit_clear", 10, GuildRank.Copper);
        Assert.False(result.Success);
        Assert.Contains("ギルドランクが足りない", result.Message);
    }

    [Fact]
    public void QuestSystem_UpdateObjective_ProgressesCorrectly()
    {
        var system = new QuestSystem();
        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);

        system.UpdateObjective("enemy_rat", 3);

        var quests = system.GetActiveQuests();
        Assert.Single(quests);
        Assert.Equal(3, quests[0].Progress.Objectives[0].CurrentCount);
        Assert.False(quests[0].Progress.IsComplete);
    }

    [Fact]
    public void QuestSystem_UpdateObjective_CompletesQuest()
    {
        var system = new QuestSystem();
        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);

        system.UpdateObjective("enemy_rat", 5);

        var quests = system.GetActiveQuests();
        Assert.Single(quests);
        Assert.True(quests[0].Progress.IsComplete);
        Assert.Equal(QuestState.Completed, quests[0].Progress.State);
    }

    [Fact]
    public void QuestSystem_TurnInQuest_Success_GrantsRewards()
    {
        var system = new QuestSystem();
        var player = CreateTestPlayer();
        var goldBefore = player.Gold;
        var expBefore = player.Experience;

        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        system.UpdateObjective("enemy_rat", 5);

        var result = system.TurnInQuest("quest_rat_hunt", player);
        Assert.True(result.Success);
        Assert.Equal(goldBefore + 100, player.Gold); // Reward: 100G
        Assert.True(player.Experience > expBefore); // 種族倍率適用(Human: 1.1x)
        Assert.Equal(1, system.CompletedQuestCount);
    }

    [Fact]
    public void QuestSystem_TurnInQuest_NotCompleted_Fails()
    {
        var system = new QuestSystem();
        var player = CreateTestPlayer();
        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);

        var result = system.TurnInQuest("quest_rat_hunt", player);
        Assert.False(result.Success);
        Assert.Contains("完了していない", result.Message);
    }

    [Fact]
    public void QuestSystem_TurnInQuest_AlreadyCompleted_Fails()
    {
        var system = new QuestSystem();
        var player = CreateTestPlayer();
        system.RegisterQuest(QuestDatabase.GetById("quest_rat_hunt")!);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        system.UpdateObjective("enemy_rat", 5);
        system.TurnInQuest("quest_rat_hunt", player);

        // 再受注しようとする
        var result = system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        Assert.False(result.Success);
        Assert.Contains("クリア済み", result.Message);
    }

    [Fact]
    public void QuestSystem_GetAvailableQuests_FiltersCorrectly()
    {
        var system = new QuestSystem();
        system.RegisterQuests(QuestDatabase.AllQuests);

        // Lv1, Copper → 銅ランク+Lv1のクエストのみ
        var available = system.GetAvailableQuests(1, GuildRank.Copper);
        Assert.All(available, q => Assert.True(q.RequiredLevel <= 1));
        Assert.All(available, q => Assert.True(q.RequiredGuildRank <= GuildRank.Copper));
    }

    [Fact]
    public void QuestSystem_GetAvailableQuests_ExcludesActiveAndCompleted()
    {
        var system = new QuestSystem();
        system.RegisterQuests(QuestDatabase.AllQuests);

        var beforeCount = system.GetAvailableQuests(100, GuildRank.Adamantine).Count;

        // 受注
        system.AcceptQuest("quest_rat_hunt", 100, GuildRank.Adamantine);
        var afterAccept = system.GetAvailableQuests(100, GuildRank.Adamantine).Count;
        Assert.Equal(beforeCount - 1, afterAccept);
    }

    [Fact]
    public void QuestSystem_SaveAndRestore_PreservesState()
    {
        var system = new QuestSystem();
        system.RegisterQuests(QuestDatabase.AllQuests);
        system.AcceptQuest("quest_rat_hunt", 1, GuildRank.Copper);
        system.UpdateObjective("enemy_rat", 3);

        var player = CreateTestPlayer();
        system.AcceptQuest("quest_herb_collect", 1, GuildRank.Copper);
        system.UpdateObjective("material_herb", 3);
        system.TurnInQuest("quest_herb_collect", player);

        // セーブ
        var saveActive = system.CreateActiveQuestsSaveData();
        var saveCompleted = system.CompletedQuestIds.ToList();

        // 新システムで復元
        var restored = new QuestSystem();
        restored.RegisterQuests(QuestDatabase.AllQuests);
        restored.RestoreState(saveActive, saveCompleted);

        Assert.Single(restored.GetActiveQuests());
        Assert.Equal(1, restored.CompletedQuestCount);
        Assert.Equal(3, restored.GetActiveQuests()[0].Progress.Objectives[0].CurrentCount);
    }

    #endregion

    #region QuestDatabase

    [Fact]
    public void QuestDatabase_AllQuests_HasExpectedCount()
    {
        Assert.Equal(11, QuestDatabase.AllQuests.Count);
    }

    [Fact]
    public void QuestDatabase_GetById_ValidId_ReturnsQuest()
    {
        var quest = QuestDatabase.GetById("quest_rat_hunt");
        Assert.NotNull(quest);
        Assert.Equal("地下倉庫のネズミ退治", quest!.Name);
    }

    [Fact]
    public void QuestDatabase_GetById_InvalidId_ReturnsNull()
    {
        Assert.Null(QuestDatabase.GetById("invalid"));
    }

    [Fact]
    public void QuestDatabase_GetByRank_Copper_ReturnsLowRankQuests()
    {
        var quests = QuestDatabase.GetByRank(GuildRank.Copper);
        Assert.All(quests, q => Assert.True(q.RequiredGuildRank <= GuildRank.Copper));
        Assert.True(quests.Count >= 3);
    }

    [Fact]
    public void QuestDatabase_GetByRank_Adamantine_ReturnsAllQuests()
    {
        var quests = QuestDatabase.GetByRank(GuildRank.Adamantine);
        Assert.Equal(QuestDatabase.AllQuests.Count, quests.Count);
    }

    [Fact]
    public void QuestDatabase_AllQuests_HaveValidRewards()
    {
        foreach (var quest in QuestDatabase.AllQuests)
        {
            Assert.True(quest.Reward.Gold > 0 || quest.Reward.Experience > 0,
                $"クエスト {quest.Id} に報酬がない");
            Assert.NotEmpty(quest.Objectives);
        }
    }

    [Fact]
    public void QuestDatabase_AllQuests_HaveValidNpcGiver()
    {
        foreach (var quest in QuestDatabase.AllQuests)
        {
            var npc = NpcDefinition.GetById(quest.GiverNpcId);
            Assert.NotNull(npc);
        }
    }

    #endregion

    #region GuildSystem

    [Fact]
    public void GuildSystem_Register_Success()
    {
        var system = new GuildSystem();
        Assert.False(system.IsRegistered);

        var result = system.Register();
        Assert.True(result.Success);
        Assert.Equal(GuildRank.Copper, system.CurrentRank);
        Assert.True(system.IsRegistered);
    }

    [Fact]
    public void GuildSystem_Register_AlreadyRegistered_Fails()
    {
        var system = new GuildSystem();
        system.Register();
        var result = system.Register();
        Assert.False(result.Success);
    }

    [Fact]
    public void GuildSystem_AddPoints_NotRegistered_Fails()
    {
        var system = new GuildSystem();
        var result = system.AddPoints(100);
        Assert.False(result.Success);
    }

    [Fact]
    public void GuildSystem_AddPoints_AccumulatesCorrectly()
    {
        var system = new GuildSystem();
        system.Register();

        system.AddPoints(50);
        Assert.Equal(50, system.GuildPoints);
        Assert.Equal(GuildRank.Copper, system.CurrentRank);
    }

    [Fact]
    public void GuildSystem_AddPoints_RankUp_CopperToIron()
    {
        var system = new GuildSystem();
        system.Register();

        var result = system.AddPoints(100);
        Assert.True(result.Success);
        Assert.Equal(GuildRank.Iron, system.CurrentRank);
        Assert.Contains("鉄", result.Message);
    }

    [Fact]
    public void GuildSystem_AddPoints_RankUp_MultipleRanks()
    {
        var system = new GuildSystem();
        system.Register();

        system.AddPoints(5000);
        Assert.Equal(GuildRank.Mythril, system.CurrentRank);
    }

    [Fact]
    public void GuildSystem_GetPointsForNextRank_ReturnsCorrect()
    {
        var system = new GuildSystem();
        system.Register();

        Assert.Equal(100, system.GetPointsForNextRank()); // Copper→Iron需要100

        system.AddPoints(50);
        Assert.Equal(50, system.GetPointsForNextRank()); // 残り50
    }

    [Fact]
    public void GuildSystem_GetRankName_AllRanks()
    {
        Assert.Equal("未登録", GuildSystem.GetRankName(GuildRank.None));
        Assert.Equal("銅", GuildSystem.GetRankName(GuildRank.Copper));
        Assert.Equal("鉄", GuildSystem.GetRankName(GuildRank.Iron));
        Assert.Equal("銀", GuildSystem.GetRankName(GuildRank.Silver));
        Assert.Equal("金", GuildSystem.GetRankName(GuildRank.Gold));
        Assert.Equal("白金", GuildSystem.GetRankName(GuildRank.Platinum));
        Assert.Equal("ミスリル", GuildSystem.GetRankName(GuildRank.Mythril));
        Assert.Equal("アダマンタイト", GuildSystem.GetRankName(GuildRank.Adamantine));
    }

    [Fact]
    public void GuildSystem_RestoreState_SetsRankAndPoints()
    {
        var system = new GuildSystem();
        system.RestoreState(GuildRank.Silver, 450);

        Assert.Equal(GuildRank.Silver, system.CurrentRank);
        Assert.Equal(450, system.GuildPoints);
        Assert.True(system.IsRegistered);
    }

    [Fact]
    public void GuildSystem_RankThresholds_AllCorrect()
    {
        var system = new GuildSystem();
        system.Register();

        system.AddPoints(100); Assert.Equal(GuildRank.Iron, system.CurrentRank);
        system.AddPoints(300); Assert.Equal(GuildRank.Silver, system.CurrentRank);
        system.AddPoints(600); Assert.Equal(GuildRank.Gold, system.CurrentRank);
        system.AddPoints(1500); Assert.Equal(GuildRank.Platinum, system.CurrentRank);
        system.AddPoints(2500); Assert.Equal(GuildRank.Mythril, system.CurrentRank);
        system.AddPoints(5000); Assert.Equal(GuildRank.Adamantine, system.CurrentRank);
    }

    #endregion

    #region QuestObjective

    [Fact]
    public void QuestObjective_IsComplete_WhenCountMet()
    {
        var obj = new QuestObjective("テスト", "target", 5, 5);
        Assert.True(obj.IsComplete);
    }

    [Fact]
    public void QuestObjective_IsComplete_WhenCountExceeded()
    {
        var obj = new QuestObjective("テスト", "target", 5, 7);
        Assert.True(obj.IsComplete);
    }

    [Fact]
    public void QuestObjective_IsNotComplete_WhenCountBelow()
    {
        var obj = new QuestObjective("テスト", "target", 5, 3);
        Assert.False(obj.IsComplete);
    }

    #endregion

    #region Integration

    [Fact]
    public void Integration_NpcAndDialogue_Workflow()
    {
        var npcSystem = new NpcSystem();
        var dialogueSystem = new DialogueSystem();

        // 会話ノード登録
        dialogueSystem.RegisterNodes(new[]
        {
            new DialogueNode("dlg_leon_intro", "レオン", "冒険者ギルドへようこそ！",
                Choices: new[]
                {
                    new DialogueChoice("登録したい", "dlg_leon_register", AffinityChange: 5),
                    new DialogueChoice("情報が欲しい", "dlg_leon_info")
                }),
            new DialogueNode("dlg_leon_register", "レオン", "では手続きを始めよう"),
            new DialogueNode("dlg_leon_info", "レオン", "最近、森に魔物が増えている")
        });

        // NPCに話しかける
        npcSystem.MeetNpc("npc_guild_master");
        Assert.True(npcSystem.GetNpcState("npc_guild_master").HasMet);

        // 会話開始
        var node = dialogueSystem.StartDialogue("dlg_leon_intro");
        Assert.NotNull(node);
        Assert.True(node!.HasChoices);

        // 選択肢を選ぶ
        var result = dialogueSystem.SelectChoice(0);
        Assert.NotNull(result);
        Assert.Equal(5, result!.AffinityChange);

        // 好感度更新
        npcSystem.ModifyAffinity("npc_guild_master", result.AffinityChange);
        Assert.Equal(55, npcSystem.GetNpcState("npc_guild_master").Affinity);
    }

    [Fact]
    public void Integration_QuestAndGuild_FullWorkflow()
    {
        var questSystem = new QuestSystem();
        var guildSystem = new GuildSystem();
        var player = CreateTestPlayer();

        // ギルド登録
        guildSystem.Register();

        // クエスト登録
        questSystem.RegisterQuests(QuestDatabase.AllQuests);

        // 受注可能クエストを確認
        var available = questSystem.GetAvailableQuests(1, guildSystem.CurrentRank);
        Assert.True(available.Count > 0);

        // クエスト受注
        var accept = questSystem.AcceptQuest("quest_rat_hunt", 1, guildSystem.CurrentRank);
        Assert.True(accept.Success);

        // 目標達成
        questSystem.UpdateObjective("enemy_rat", 5);

        // 報酬受取
        var turnIn = questSystem.TurnInQuest("quest_rat_hunt", player);
        Assert.True(turnIn.Success);
        Assert.NotNull(turnIn.Reward);

        // ギルドポイント加算
        guildSystem.AddPoints(turnIn.Reward!.GuildPoints);
        Assert.Equal(10, guildSystem.GuildPoints);
    }

    [Fact]
    public void Integration_SaveAndRestore_AllSystems()
    {
        // 元のシステム
        var npcSystem = new NpcSystem();
        var dialogueSystem = new DialogueSystem();
        var questSystem = new QuestSystem();
        var guildSystem = new GuildSystem();

        npcSystem.MeetNpc("npc_guild_master");
        npcSystem.ModifyAffinity("npc_guild_master", 20);
        dialogueSystem.SetFlag("intro_done");
        guildSystem.Register();
        guildSystem.AddPoints(150); // Iron rank
        questSystem.RegisterQuests(QuestDatabase.AllQuests);
        questSystem.AcceptQuest("quest_rat_hunt", 1, GuildRank.Iron);
        questSystem.UpdateObjective("enemy_rat", 2);

        // セーブデータ作成
        var npcSave = new Dictionary<string, NpcStateSaveData>();
        foreach (var (id, state) in npcSystem.GetAllStates())
        {
            npcSave[id] = new NpcStateSaveData
            {
                Affinity = state.Affinity,
                HasMet = state.HasMet,
                CompletedDialogues = state.CompletedDialogues.ToList()
            };
        }
        var questSaveActive = questSystem.CreateActiveQuestsSaveData();
        var questSaveCompleted = questSystem.CompletedQuestIds.ToList();
        var flagsSave = dialogueSystem.GetAllFlags().ToList();
        var guildRankSave = guildSystem.CurrentRank;
        var guildPointsSave = guildSystem.GuildPoints;

        // 新しいシステムに復元
        var npc2 = new NpcSystem();
        var dlg2 = new DialogueSystem();
        var quest2 = new QuestSystem();
        var guild2 = new GuildSystem();

        npc2.RestoreStates(npcSave);
        dlg2.RestoreFlags(flagsSave);
        guild2.RestoreState(guildRankSave, guildPointsSave);
        quest2.RegisterQuests(QuestDatabase.AllQuests);
        quest2.RestoreState(questSaveActive, questSaveCompleted);

        // 検証
        Assert.Equal(70, npc2.GetNpcState("npc_guild_master").Affinity);
        Assert.True(npc2.GetNpcState("npc_guild_master").HasMet);
        Assert.True(dlg2.HasFlag("intro_done"));
        Assert.Equal(GuildRank.Iron, guild2.CurrentRank);
        Assert.Equal(150, guild2.GuildPoints);
        var active = quest2.GetActiveQuests();
        Assert.Single(active);
        Assert.Equal(2, active[0].Progress.Objectives[0].CurrentCount);
    }

    #endregion
}
