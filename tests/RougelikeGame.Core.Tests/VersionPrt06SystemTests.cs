using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.6 新規システムテスト - 10システム + 死に戻りメカニクス修正
/// </summary>
public class VersionPrt06SystemTests
{
    // ============================================================
    // T.0: 死に戻りメカニクス修正（NpcMemorySystem.Reset）
    // ============================================================

    [Fact]
    public void NpcMemorySystem_Reset_ClearsAllMemories()
    {
        var system = new NpcMemorySystem();
        system.RecordAction("npc1", "saved", 10, 100);
        system.RecordAction("npc2", "stole", -5, 200);
        Assert.Equal(2, system.Memories.Count);

        system.Reset();
        Assert.Empty(system.Memories);
    }

    [Fact]
    public void NpcMemorySystem_Reset_ClearsAllRumors()
    {
        var system = new NpcMemorySystem();
        system.GenerateRumor(RumorType.Heroic, "勇者の噂", "grass_plains");
        system.GenerateRumor(RumorType.Villainous, "悪漢の噂", "dark_forest");
        Assert.Equal(2, system.Rumors.Count);

        system.Reset();
        Assert.Empty(system.Rumors);
    }

    [Fact]
    public void NpcMemorySystem_Reset_ImpressionBecomesZero()
    {
        var system = new NpcMemorySystem();
        system.RecordAction("npc1", "helped", 20, 100);
        Assert.Equal(20, system.CalculateImpression("npc1"));

        system.Reset();
        Assert.Equal(0, system.CalculateImpression("npc1"));
    }

    // ============================================================
    // T.1: アイテム鑑定・呪いシステム
    // ============================================================

    [Fact]
    public void ItemIdentification_NewItem_IsUnknown()
    {
        var system = new ItemIdentificationSystem();
        Assert.Equal(IdentificationState.Unknown, system.GetState("item_001"));
        Assert.False(system.IsIdentified("item_001"));
    }

    [Fact]
    public void ItemIdentification_Identify_SetsIdentified()
    {
        var system = new ItemIdentificationSystem();
        var result = system.Identify("item_001", "鋼の剣");
        Assert.Equal(IdentificationState.Identified, result.State);
        Assert.Equal(CurseType.None, result.Curse);
        Assert.True(system.IsIdentified("item_001"));
    }

    [Fact]
    public void ItemIdentification_IdentifyCursed_SetsCursedState()
    {
        var system = new ItemIdentificationSystem();
        var result = system.Identify("item_002", "呪われた指輪", CurseType.Major);
        Assert.Equal(IdentificationState.Cursed, result.State);
        Assert.Equal(CurseType.Major, result.Curse);
        Assert.Contains("item_002", system.KnownCurses);
    }

    [Fact]
    public void ItemIdentification_CursedItem_CannotUnequip()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("cursed_ring", "呪いの指輪", CurseType.Major);
        Assert.False(system.CanUnequip("cursed_ring"));
    }

    [Fact]
    public void ItemIdentification_RemoveCurse_RequiresLevel()
    {
        var system = new ItemIdentificationSystem();
        system.Identify("cursed_sword", "呪いの剣", CurseType.Major);

        var lowLevel = system.RemoveCurse("cursed_sword", 5);
        Assert.False(lowLevel.Success);

        var highLevel = system.RemoveCurse("cursed_sword", 15);
        Assert.True(highLevel.Success);
        Assert.True(system.CanUnequip("cursed_sword"));
    }

    [Fact]
    public void ItemIdentification_CursePenalty_ReturnsCorrectValues()
    {
        Assert.Equal(0, ItemIdentificationSystem.GetCursePenalty(CurseType.None));
        Assert.Equal(-2, ItemIdentificationSystem.GetCursePenalty(CurseType.Minor));
        Assert.Equal(-5, ItemIdentificationSystem.GetCursePenalty(CurseType.Major));
        Assert.Equal(-10, ItemIdentificationSystem.GetCursePenalty(CurseType.Deadly));
    }

    // ============================================================
    // T.2: ダンジョン生態系・食物連鎖システム
    // ============================================================

    [Fact]
    public void DungeonEcosystem_RegisterRelation_CreatesRelation()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast, 80);
        Assert.True(system.HasPredatorRelation(MonsterRace.Dragon, MonsterRace.Beast));
        Assert.False(system.HasPredatorRelation(MonsterRace.Beast, MonsterRace.Dragon));
    }

    [Fact]
    public void DungeonEcosystem_ProcessPredation_CreatesEvent()
    {
        var system = new DungeonEcosystemSystem();
        system.RegisterRelation(MonsterRace.Dragon, MonsterRace.Beast);
        var evt = system.ProcessInteraction("dragon1", MonsterRace.Dragon, "wolf1", MonsterRace.Beast, 5, 100);
        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.Predation, evt.Type);
    }

    [Fact]
    public void DungeonEcosystem_SameRace_CreatesTerritoryFight()
    {
        var system = new DungeonEcosystemSystem();
        var evt = system.ProcessInteraction("wolf1", MonsterRace.Beast, "wolf2", MonsterRace.Beast, 3, 50);
        Assert.NotNull(evt);
        Assert.Equal(EcosystemEventType.TerritoryFight, evt.Type);
    }

    [Fact]
    public void DungeonEcosystem_BattleTrace_TracksFloor()
    {
        var system = new DungeonEcosystemSystem();
        system.AddBattleTrace(10, 20, 5, 3, "血痕", 100);
        system.AddBattleTrace(15, 25, 5, 7, "残骸", 110);
        system.AddBattleTrace(5, 5, 3, 2, "爪痕", 90);

        var floor5 = system.GetTracesOnFloor(5);
        Assert.Equal(2, floor5.Count);
        Assert.Equal(5, system.EstimateDangerLevel(5));
    }

    [Fact]
    public void DungeonEcosystem_NoRelation_ReturnsNull()
    {
        var system = new DungeonEcosystemSystem();
        var evt = system.ProcessInteraction("a", MonsterRace.Plant, "b", MonsterRace.Undead, 1, 10);
        Assert.Null(evt);
    }

    // ============================================================
    // T.3: ペット・騎乗システム
    // ============================================================

    [Fact]
    public void PetSystem_AddPet_CreatesNewPet()
    {
        var system = new PetSystem();
        var pet = system.AddPet("pet1", "シロ", PetType.Wolf);
        Assert.Equal("シロ", pet.Name);
        Assert.Equal(PetType.Wolf, pet.Type);
        Assert.Equal(1, pet.Level);
        Assert.Equal(50, pet.Loyalty);
    }

    [Fact]
    public void PetSystem_Feed_IncreasesHungerAndLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "シロ", PetType.Wolf);
        var fed = system.Feed("pet1", 30, 10);
        Assert.Equal(100, fed.Hunger); // Already at 100
        Assert.Equal(60, fed.Loyalty); // 50 + 10
    }

    [Fact]
    public void PetSystem_Train_IncreasesLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "シロ", PetType.Wolf);
        var trained = system.Train("pet1", 15);
        Assert.Equal(65, trained.Loyalty); // 50 + 15
    }

    [Fact]
    public void PetSystem_Ride_OnlyRideable()
    {
        var system = new PetSystem();
        system.AddPet("horse1", "ハヤテ", PetType.Horse);
        system.AddPet("hawk1", "タカ", PetType.Hawk);

        var horse = system.ToggleRide("horse1");
        Assert.True(horse.IsRiding);

        var hawk = system.ToggleRide("hawk1");
        Assert.False(hawk.IsRiding); // Hawks can't be ridden
    }

    [Fact]
    public void PetSystem_MoveSpeed_ChangesWhenRiding()
    {
        var system = new PetSystem();
        system.AddPet("horse1", "ハヤテ", PetType.Horse);
        Assert.Equal(1.0f, system.GetMoveSpeedMultiplier("horse1"));

        system.ToggleRide("horse1");
        Assert.Equal(2.0f, system.GetMoveSpeedMultiplier("horse1"));
    }

    [Fact]
    public void PetSystem_HungerTick_DecreasesHunger()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "シロ", PetType.Wolf);
        var pet = system.TickHunger("pet1", 5);
        Assert.Equal(95, pet.Hunger);
    }

    // ============================================================
    // T.4: 商人ギルド・流通ネットワークシステム
    // ============================================================

    [Fact]
    public void MerchantGuild_JoinGuild_CreatesMembership()
    {
        var system = new MerchantGuildSystem();
        Assert.False(system.IsMember);
        var membership = system.JoinGuild("player1");
        Assert.True(system.IsMember);
        Assert.Equal(GuildRank.None, membership.Rank);
    }

    [Fact]
    public void MerchantGuild_EstablishRoute_CreatesRoute()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        var route = system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.NotNull(route);
        Assert.Equal(TradeRouteStatus.Open, route.Status);
    }

    [Fact]
    public void MerchantGuild_Trade_CalculatesProfit()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ExecuteTrade("route1", 1000);
        Assert.NotNull(result);
        Assert.True(result.ActualProfit > 0);
    }

    [Fact]
    public void MerchantGuild_NotMember_CannotTrade()
    {
        var system = new MerchantGuildSystem();
        var result = system.ExecuteTrade("route1", 1000);
        Assert.Null(result);
    }

    [Fact]
    public void MerchantGuild_RouteStatusUpdate_Works()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.UpdateRouteStatus("route1", TradeRouteStatus.Blocked);
        Assert.Equal(TradeRouteStatus.Blocked, system.Routes[0].Status);
    }

    // ============================================================
    // T.5: 実績・トロフィーシステム
    // ============================================================

    [Fact]
    public void Achievement_Register_AddsAchievement()
    {
        var system = new AchievementSystem();
        system.Register("first_kill", "初めての撃破", "敵を初めて倒す", AchievementCategory.Combat);
        Assert.Equal(1, system.TotalCount);
        Assert.Equal(0, system.UnlockedCount);
    }

    [Fact]
    public void Achievement_Unlock_SetsUnlocked()
    {
        var system = new AchievementSystem();
        system.Register("first_kill", "初めての撃破", "敵を初めて倒す", AchievementCategory.Combat);
        var result = system.Unlock("first_kill", 100);
        Assert.True(result.IsNewUnlock);
        Assert.True(system.IsUnlocked("first_kill"));
    }

    [Fact]
    public void Achievement_DoubleUnlock_ReportsFalse()
    {
        var system = new AchievementSystem();
        system.Register("first_kill", "初めての撃破", "敵を初めて倒す", AchievementCategory.Combat);
        system.Unlock("first_kill");
        var second = system.Unlock("first_kill");
        Assert.False(second.IsNewUnlock);
    }

    [Fact]
    public void Achievement_CompletionRate_CalculatesCorrectly()
    {
        var system = new AchievementSystem();
        system.Register("a1", "A", "A", AchievementCategory.Combat);
        system.Register("a2", "B", "B", AchievementCategory.Combat);
        system.Register("a3", "C", "C", AchievementCategory.Exploration);
        system.Unlock("a1");
        Assert.True(Math.Abs(1f / 3f - system.CompletionRate) < 0.01f);
    }

    [Fact]
    public void Achievement_CategoryFilter_Works()
    {
        var system = new AchievementSystem();
        system.Register("c1", "戦闘1", "", AchievementCategory.Combat);
        system.Register("e1", "探索1", "", AchievementCategory.Exploration);
        system.Register("c2", "戦闘2", "", AchievementCategory.Combat);
        Assert.Equal(2, system.GetByCategory(AchievementCategory.Combat).Count);
    }

    [Fact]
    public void Achievement_NextPlayBonus_CalculatesFromBonusEffect()
    {
        var system = new AchievementSystem();
        system.Register("a1", "A", "", AchievementCategory.Combat, "stat_boost_small");
        system.Register("a2", "B", "", AchievementCategory.Combat, "gold_bonus");
        system.Unlock("a1");
        system.Unlock("a2");
        int bonus = system.CalculateNextPlayBonus();
        Assert.Equal(51, bonus); // 1 + 50
    }

    // ============================================================
    // T.6: 碑文・壁画解読システム
    // ============================================================

    [Fact]
    public void Inscription_Register_AddsInscription()
    {
        var system = new InscriptionSystem();
        system.Register("ins1", InscriptionType.Lore, "古代文字", "この地は昔、竜の巣だった", 5);
        Assert.Equal(1, system.Inscriptions.Count);
    }

    [Fact]
    public void Inscription_TryDecode_SucceedsWithSufficientLevel()
    {
        var system = new InscriptionSystem();
        system.Register("ins1", InscriptionType.Hint, "文字", "3階に隠し部屋がある", 5, "secret_room_hint");
        var result = system.TryDecode("ins1", 10);
        Assert.True(result.Success);
        Assert.Equal("secret_room_hint", result.RewardInfo);
        Assert.Equal(100, result.PartialProgress);
    }

    [Fact]
    public void Inscription_TryDecode_FailsWithLowLevel()
    {
        var system = new InscriptionSystem();
        system.Register("ins1", InscriptionType.Spell, "呪文碑文", "イグニス・フレア", 10);
        var result = system.TryDecode("ins1", 3);
        Assert.False(result.Success);
        Assert.True(result.PartialProgress > 0);
        Assert.True(result.PartialProgress < 100);
    }

    [Fact]
    public void Inscription_AlreadyDecoded_ReturnsDecoded()
    {
        var system = new InscriptionSystem();
        system.Register("ins1", InscriptionType.Warning, "警告", "この先危険", 1);
        system.TryDecode("ins1", 5);
        var again = system.TryDecode("ins1", 5);
        Assert.True(again.Success);
        Assert.Equal(1, system.DecodedCount);
    }

    [Fact]
    public void Inscription_GetByType_FiltersCorrectly()
    {
        var system = new InscriptionSystem();
        system.Register("i1", InscriptionType.Lore, "a", "b", 1);
        system.Register("i2", InscriptionType.Warning, "c", "d", 1);
        system.Register("i3", InscriptionType.Lore, "e", "f", 1);
        Assert.Equal(2, system.GetByType(InscriptionType.Lore).Count);
    }

    // ============================================================
    // T.7: コンテキストヘルプ・チュートリアルシステム
    // ============================================================

    [Fact]
    public void ContextHelp_RegisterTopic_AddsTopic()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("move", HelpCategory.Movement, "移動", "WASDで移動", "WASD");
        Assert.Equal(1, system.Topics.Count);
    }

    [Fact]
    public void ContextHelp_GetByCategory_FiltersCorrectly()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("move", HelpCategory.Movement, "移動", "テンキーで移動");
        system.RegisterTopic("attack", HelpCategory.Combat, "攻撃", "敵に向かって移動");
        system.RegisterTopic("wait", HelpCategory.Movement, "待機", ".キーで待機");
        Assert.Equal(2, system.GetTopicsByCategory(HelpCategory.Movement).Count);
    }

    [Fact]
    public void ContextHelp_Tutorial_ProgressesSteps()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("移動", "テンキーで移動してみよう", "first_move");
        system.AddTutorialStep("攻撃", "敵を攻撃してみよう", "first_attack");

        Assert.Equal(0, system.CurrentStep);
        var current = system.GetCurrentTutorial();
        Assert.NotNull(current);
        Assert.Equal("移動", current.Title);

        system.CompleteTutorialStep();
        Assert.Equal(1, system.CurrentStep);
        Assert.Equal(0.5f, system.TutorialProgress);
    }

    [Fact]
    public void ContextHelp_TutorialDisabled_ReturnsNull()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("テスト", "テスト", "test");
        system.SetTutorialEnabled(false);
        Assert.Null(system.GetCurrentTutorial());
    }

    [Fact]
    public void ContextHelp_DefaultTopics_RegistersAll()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();
        Assert.True(system.Topics.Count >= 7);
    }

    // ============================================================
    // T.8: アクセシビリティオプション
    // ============================================================

    [Fact]
    public void Accessibility_DefaultConfig_IsNormal()
    {
        var system = new AccessibilitySystem();
        Assert.Equal(ColorBlindMode.None, system.Config.ColorMode);
        Assert.Equal(1.0f, system.Config.FontSizeMultiplier);
        Assert.Equal(1.0f, system.Config.GameSpeedMultiplier);
        Assert.False(system.Config.HighContrastMode);
    }

    [Fact]
    public void Accessibility_SetColorBlindMode_Changes()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Protanopia);
        Assert.Equal(ColorBlindMode.Protanopia, system.Config.ColorMode);
    }

    [Fact]
    public void Accessibility_FontSize_ClampsToRange()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(5.0f);
        Assert.Equal(3.0f, system.Config.FontSizeMultiplier);
        system.SetFontSizeMultiplier(0.1f);
        Assert.Equal(0.5f, system.Config.FontSizeMultiplier);
    }

    [Fact]
    public void Accessibility_ColorTransform_Protanopia()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Protanopia);
        var transform = system.TransformColor("Red");
        Assert.Equal("DarkYellow", transform.TransformedColor);
    }

    [Fact]
    public void Accessibility_EffectiveFontSize_Calculated()
    {
        var system = new AccessibilitySystem();
        system.SetFontSizeMultiplier(2.0f);
        Assert.Equal(24, system.CalculateEffectiveFontSize(12));
    }

    [Fact]
    public void Accessibility_ResetDefaults_RestoresAll()
    {
        var system = new AccessibilitySystem();
        system.SetColorBlindMode(ColorBlindMode.Monochrome);
        system.SetFontSizeMultiplier(2.0f);
        system.SetHighContrastMode(true);
        system.ResetToDefaults();
        Assert.Equal(ColorBlindMode.None, system.Config.ColorMode);
        Assert.Equal(1.0f, system.Config.FontSizeMultiplier);
        Assert.False(system.Config.HighContrastMode);
    }

    // ============================================================
    // T.9: MOD・カスタムコンテンツ対応基盤
    // ============================================================

    [Fact]
    public void ModLoader_ParseManifest_CreatesManifest()
    {
        var system = new ModLoaderSystem();
        var manifest = system.ParseManifest("test_mod", "テストMOD", "Author", "1.0.0", "テスト用");
        Assert.NotNull(manifest);
        Assert.Equal("test_mod", manifest.ModId);
    }

    [Fact]
    public void ModLoader_Validate_DetectsErrors()
    {
        var system = new ModLoaderSystem();
        // DI-3/DI-4: ParseManifestは空modIdでnullを返す仕様に変更
        var manifest = system.ParseManifest("", "テスト", "Author", "1.0.0", "テスト");
        Assert.Null(manifest);
    }

    [Fact]
    public void ModLoader_LoadMod_Succeeds()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Enemy, "enemies.json", "カスタム敵")
        };
        var manifest = new ModLoaderSystem.ModManifest("mod1", "MOD1", "Author", "1.0", "テスト", contents, true);
        var result = system.LoadMod(manifest);
        Assert.True(result.Success);
        Assert.Equal(1, system.LoadedCount);
    }

    [Fact]
    public void ModLoader_DuplicateLoad_Fails()
    {
        var system = new ModLoaderSystem();
        var manifest = new ModLoaderSystem.ModManifest("mod1", "MOD1", "A", "1.0", "T",
            new List<ModLoaderSystem.ModContentEntry>(), true);
        system.LoadMod(manifest);
        var second = system.LoadMod(manifest);
        Assert.False(second.Success);
    }

    [Fact]
    public void ModLoader_UnloadMod_RemovesMod()
    {
        var system = new ModLoaderSystem();
        var manifest = new ModLoaderSystem.ModManifest("mod1", "MOD1", "A", "1.0", "T",
            new List<ModLoaderSystem.ModContentEntry>(), true);
        system.LoadMod(manifest);
        Assert.True(system.UnloadMod("mod1"));
        Assert.Equal(0, system.LoadedCount);
    }

    [Fact]
    public void ModLoader_GetContentsByType_FiltersCorrectly()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Enemy, "enemies.json", "敵"),
            new(ModContentType.Item, "items.json", "アイテム"),
            new(ModContentType.Enemy, "bosses.json", "ボス")
        };
        var manifest = new ModLoaderSystem.ModManifest("mod1", "MOD1", "A", "1.0", "T", contents, true);
        system.LoadMod(manifest);
        Assert.Equal(2, system.GetContentsByType(ModContentType.Enemy).Count);
    }

    // ============================================================
    // T.10: 陣営・派閥間戦争イベントシステム
    // ============================================================

    [Fact]
    public void FactionWar_StartWar_CreatesEvent()
    {
        var system = new FactionWarSystem();
        var war = system.StartWar("war1", "平原vs森林", TerritoryId.Capital, TerritoryId.Forest, 1000);
        Assert.Equal(WarPhase.Tension, war.Phase);
        Assert.Equal(1, system.ActiveWars.Count);
    }

    [Fact]
    public void FactionWar_AdvancePhase_ProgressesCorrectly()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト戦争", TerritoryId.Capital, TerritoryId.Forest, 100);

        var skirmish = system.AdvancePhase("war1", 200);
        Assert.Equal(WarPhase.Skirmish, skirmish!.Phase);

        var battle = system.AdvancePhase("war1", 300);
        Assert.Equal(WarPhase.Battle, battle!.Phase);
    }

    [Fact]
    public void FactionWar_ChooseAlignment_SetsPlayerSide()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ChooseAlignment("war1", FactionAlignment.Faction1);
        Assert.NotNull(result);
        Assert.Equal(FactionAlignment.Faction1, result.ChosenSide);
        Assert.Equal(20, result.ReputationChange);
    }

    [Fact]
    public void FactionWar_Mercenary_GetsGoldReward()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ChooseAlignment("war1", FactionAlignment.Mercenary);
        Assert.Equal(500, result!.GoldReward);
        Assert.Equal(-5, result.ReputationChange);
    }

    [Fact]
    public void FactionWar_ResolveWar_MovesToHistory()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var outcome = system.ResolveWar("war1", TerritoryId.Capital, 30);
        Assert.NotNull(outcome);
        Assert.Equal(TerritoryId.Capital, outcome.Winner);
        Assert.Empty(system.ActiveWars);
        Assert.Equal(1, system.WarHistory.Count);
    }

    [Fact]
    public void FactionWar_GetWarsInvolving_FiltersTerritory()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "戦争1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.StartWar("war2", "戦争2", TerritoryId.Mountain, TerritoryId.Coast, 200);
        var wars = system.GetWarsInvolving(TerritoryId.Capital);
        Assert.Equal(1, wars.Count);
    }
}
