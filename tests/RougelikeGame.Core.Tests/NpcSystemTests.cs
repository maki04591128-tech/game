using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Entities;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// NpcSystem総合テスト - NpcDefinition/NpcSystem/DialogueSystem/QuestSystem/GuildSystem
/// テスト数: 35件
/// </summary>
public class NpcSystemTests
{
    // ============================================================
    // NpcDefinition テスト
    // ============================================================

    [Fact]
    public void NpcDefinition_GetAll_ReturnsNpcs()
    {
        var all = NpcDefinition.GetAll();
        Assert.NotEmpty(all);
        Assert.True(all.Count >= 14); // 6領地×2-3 NPC
    }

    [Fact]
    public void NpcDefinition_GetByTerritory_ReturnsCorrectTerritory()
    {
        var capitalNpcs = NpcDefinition.GetByTerritory(TerritoryId.Capital);
        Assert.NotEmpty(capitalNpcs);
        Assert.All(capitalNpcs, npc => Assert.Equal(TerritoryId.Capital, npc.Location));
    }

    [Fact]
    public void NpcDefinition_GetById_ReturnsCorrectNpc()
    {
        var npc = NpcDefinition.GetById("npc_guild_master");
        Assert.NotNull(npc);
        Assert.Equal("ギルドマスター・レオン", npc.Name);
        Assert.Equal(NpcType.GuildMaster, npc.Type);
    }

    [Fact]
    public void NpcDefinition_GetById_InvalidId_ReturnsNull()
    {
        var npc = NpcDefinition.GetById("nonexistent");
        Assert.Null(npc);
    }

    [Theory]
    [InlineData(100, "親友")]
    [InlineData(90, "親友")]
    [InlineData(70, "友好")]
    [InlineData(50, "普通")]
    [InlineData(30, "警戒")]
    [InlineData(10, "敵意")]
    public void NpcDefinition_GetAffinityRank_ReturnsCorrectRank(int affinity, string expected)
    {
        Assert.Equal(expected, NpcDefinition.GetAffinityRank(affinity));
    }

    // ============================================================
    // NpcSystem テスト
    // ============================================================

    [Fact]
    public void NpcSystem_GetNpcState_InitializesDefaultState()
    {
        var system = new NpcSystem();
        var state = system.GetNpcState("npc_test");
        Assert.Equal("npc_test", state.NpcId);
        Assert.Equal(50, state.Affinity);
        Assert.False(state.HasMet);
    }

    [Fact]
    public void NpcSystem_ModifyAffinity_ClampsTo0And100()
    {
        var system = new NpcSystem();
        system.ModifyAffinity("npc_test", 200);
        Assert.Equal(100, system.GetNpcState("npc_test").Affinity);

        system.ModifyAffinity("npc_test", -500);
        Assert.Equal(0, system.GetNpcState("npc_test").Affinity);
    }

    [Fact]
    public void NpcSystem_MeetNpc_SetsHasMet()
    {
        var system = new NpcSystem();
        Assert.False(system.GetNpcState("npc_test").HasMet);
        system.MeetNpc("npc_test");
        Assert.True(system.GetNpcState("npc_test").HasMet);
    }

    [Fact]
    public void NpcSystem_CreateTransferData_Contains80Percent()
    {
        var system = new NpcSystem();
        system.ModifyAffinity("npc1", 50); // 50 + 50 = 100
        var data = system.CreateTransferData();
        Assert.Equal(80, data["npc1"]); // 100 * 0.8
    }

    [Fact]
    public void NpcSystem_ApplyTransferData_RestoresAffinity()
    {
        var system = new NpcSystem();
        var data = new Dictionary<string, int> { { "npc1", 75 } };
        system.ApplyTransferData(data);
        Assert.Equal(75, system.GetNpcState("npc1").Affinity);
    }

    [Fact]
    public void NpcSystem_Reset_ClearsAllStates()
    {
        var system = new NpcSystem();
        system.MeetNpc("npc1");
        system.ModifyAffinity("npc2", 10);
        system.Reset();
        Assert.Empty(system.GetAllStates());
    }

    [Fact]
    public void NpcSystem_RestoreStates_RebuildsCorrectly()
    {
        var system = new NpcSystem();
        var saveData = new Dictionary<string, NpcStateSaveData>
        {
            ["npc1"] = new NpcStateSaveData { Affinity = 80, HasMet = true, CompletedDialogues = new List<string> { "dlg1" } }
        };
        system.RestoreStates(saveData);
        var state = system.GetNpcState("npc1");
        Assert.Equal(80, state.Affinity);
        Assert.True(state.HasMet);
        Assert.Contains("dlg1", state.CompletedDialogues);
    }

    // ============================================================
    // DialogueSystem テスト
    // ============================================================

    [Fact]
    public void DialogueSystem_StartDialogue_ReturnsNode()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "テスト", "こんにちは"));
        var node = system.StartDialogue("node1");
        Assert.NotNull(node);
        Assert.Equal("こんにちは", node.Text);
        Assert.True(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_StartDialogue_InvalidId_ReturnsNull()
    {
        var system = new DialogueSystem();
        Assert.Null(system.StartDialogue("nonexistent"));
    }

    [Fact]
    public void DialogueSystem_StartDialogue_ConditionNotMet_ReturnsNull()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "テスト", "秘密", ConditionFlag: "secret_flag"));
        Assert.Null(system.StartDialogue("node1"));
    }

    [Fact]
    public void DialogueSystem_StartDialogue_ConditionMet_ReturnsNode()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "テスト", "秘密", ConditionFlag: "secret_flag"));
        system.SetFlag("secret_flag");
        Assert.NotNull(system.StartDialogue("node1"));
    }

    [Fact]
    public void DialogueSystem_SelectChoice_AdvancesToNextNode()
    {
        var system = new DialogueSystem();
        var choices = new[] { new DialogueChoice("はい", "node2", AffinityChange: 5) };
        system.RegisterNode(new DialogueNode("node1", "NPC", "質問", choices));
        system.RegisterNode(new DialogueNode("node2", "NPC", "ありがとう"));
        system.StartDialogue("node1");

        var result = system.SelectChoice(0);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(5, result.AffinityChange);
        Assert.Equal("ありがとう", result.NextNode?.Text);
    }

    [Fact]
    public void DialogueSystem_SelectChoice_InvalidIndex_ReturnsNull()
    {
        var system = new DialogueSystem();
        var choices = new[] { new DialogueChoice("はい", "node2") };
        system.RegisterNode(new DialogueNode("node1", "NPC", "質問", choices));
        system.StartDialogue("node1");
        Assert.Null(system.SelectChoice(5));
    }

    [Fact]
    public void DialogueSystem_Advance_FollowsNextNodeId()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "NPC", "最初", NextNodeId: "node2"));
        system.RegisterNode(new DialogueNode("node2", "NPC", "次"));
        system.StartDialogue("node1");

        var next = system.Advance();
        Assert.NotNull(next);
        Assert.Equal("次", next.Text);
    }

    [Fact]
    public void DialogueSystem_Advance_NoNext_EndsDialogue()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "NPC", "最後"));
        system.StartDialogue("node1");
        var next = system.Advance();
        Assert.Null(next);
        Assert.False(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_EndDialogue_ClearsCurrentNode()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "NPC", "テスト"));
        system.StartDialogue("node1");
        system.EndDialogue();
        Assert.False(system.IsInDialogue);
    }

    [Fact]
    public void DialogueSystem_Reset_ClearsFlagsAndCurrentNode()
    {
        var system = new DialogueSystem();
        system.RegisterNode(new DialogueNode("node1", "NPC", "テスト"));
        system.SetFlag("flag1");
        system.StartDialogue("node1");
        system.Reset();
        Assert.False(system.IsInDialogue);
        Assert.False(system.HasFlag("flag1"));
    }

    // ============================================================
    // QuestSystem テスト
    // ============================================================

    private static QuestDefinition CreateTestQuest(string id = "quest_test", int requiredLevel = 1)
    {
        return new QuestDefinition(id, "テストクエスト", "テスト用",
            QuestType.Kill, "npc_test", requiredLevel, GuildRank.None,
            new[] { new QuestObjective("敵を倒す", "enemy_test", 3) },
            new QuestReward(Gold: 100, Experience: 50));
    }

    [Fact]
    public void QuestSystem_AcceptQuest_Succeeds()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest());
        var result = system.AcceptQuest("quest_test", 1, GuildRank.None);
        Assert.True(result.Success);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_LevelTooLow_Fails()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest(requiredLevel: 10));
        var result = system.AcceptQuest("quest_test", 1, GuildRank.None);
        Assert.False(result.Success);
    }

    [Fact]
    public void QuestSystem_AcceptQuest_AlreadyAccepted_Fails()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest());
        system.AcceptQuest("quest_test", 1, GuildRank.None);
        var result = system.AcceptQuest("quest_test", 1, GuildRank.None);
        Assert.False(result.Success);
    }

    [Fact]
    public void QuestSystem_UpdateObjective_ProgressesQuest()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest());
        system.AcceptQuest("quest_test", 1, GuildRank.None);
        system.UpdateObjective("enemy_test", 3);
        var active = system.GetActiveQuests();
        Assert.Single(active);
        Assert.True(active[0].Progress.IsComplete);
    }

    [Fact]
    public void QuestSystem_GetAvailableQuests_ExcludesActiveAndCompleted()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest("q1"));
        system.RegisterQuest(CreateTestQuest("q2"));
        system.AcceptQuest("q1", 1, GuildRank.None);
        var available = system.GetAvailableQuests(1, GuildRank.None);
        Assert.Single(available);
        Assert.Equal("q2", available[0].Id);
    }

    [Fact]
    public void QuestSystem_Reset_ClearsAllProgress()
    {
        var system = new QuestSystem();
        system.RegisterQuest(CreateTestQuest());
        system.AcceptQuest("quest_test", 1, GuildRank.None);
        system.Reset();
        Assert.Empty(system.GetActiveQuests());
        Assert.Equal(0, system.CompletedQuestCount);
    }

    [Fact]
    public void QuestSystem_RegisterMainQuest_RegistersCorrectly()
    {
        var system = new QuestSystem();
        system.RegisterMainQuest();
        var quest = system.GetQuestDefinition("main_quest_abyss");
        Assert.NotNull(quest);
        Assert.Equal(QuestType.Main, quest.Type);
        Assert.Equal(6, quest.Objectives.Length);
    }

    // ============================================================
    // GuildSystem テスト
    // ============================================================

    [Fact]
    public void GuildSystem_Register_SetsCopperRank()
    {
        var guild = new GuildSystem();
        var result = guild.Register();
        Assert.True(result.Success);
        Assert.Equal(GuildRank.Copper, guild.CurrentRank);
    }

    [Fact]
    public void GuildSystem_Register_Twice_Fails()
    {
        var guild = new GuildSystem();
        guild.Register();
        var result = guild.Register();
        Assert.False(result.Success);
    }

    [Fact]
    public void GuildSystem_AddPoints_RanksUp()
    {
        var guild = new GuildSystem();
        guild.Register();
        guild.AddPoints(100);
        Assert.Equal(GuildRank.Iron, guild.CurrentRank);
    }

    [Fact]
    public void GuildSystem_AddPoints_NotRegistered_Fails()
    {
        var guild = new GuildSystem();
        var result = guild.AddPoints(100);
        Assert.False(result.Success);
    }

    [Fact]
    public void GuildSystem_Reset_ClearsRankAndPoints()
    {
        var guild = new GuildSystem();
        guild.Register();
        guild.AddPoints(500);
        guild.Reset();
        Assert.Equal(GuildRank.None, guild.CurrentRank);
        Assert.Equal(0, guild.GuildPoints);
    }

    [Theory]
    [InlineData(GuildRank.Copper, "銅")]
    [InlineData(GuildRank.Iron, "鉄")]
    [InlineData(GuildRank.Silver, "銀")]
    [InlineData(GuildRank.Gold, "金")]
    [InlineData(GuildRank.Adamantine, "アダマンタイト")]
    public void GuildSystem_GetRankName_ReturnsCorrectName(GuildRank rank, string expected)
    {
        Assert.Equal(expected, GuildSystem.GetRankName(rank));
    }

    // ============================================================
    // QuestDatabase テスト
    // ============================================================

    [Fact]
    public void QuestDatabase_AllQuests_HasEntries()
    {
        Assert.NotEmpty(QuestDatabase.AllQuests);
        Assert.True(QuestDatabase.AllQuests.Count >= 10);
    }

    [Fact]
    public void QuestDatabase_GetByRank_FiltersCorrectly()
    {
        var copperQuests = QuestDatabase.GetByRank(GuildRank.Copper);
        Assert.All(copperQuests, q => Assert.True(q.RequiredGuildRank <= GuildRank.Copper));
    }

    [Fact]
    public void QuestDatabase_GetById_ReturnsCorrectQuest()
    {
        var quest = QuestDatabase.GetById("quest_rat_hunt");
        Assert.NotNull(quest);
        Assert.Equal("地下倉庫のネズミ退治", quest.Name);
    }
}
