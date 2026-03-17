using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.71 病気 / P.72 睡眠・野営 / P.68 NPC日常行動 / P.74 図鑑 / P.80 死亡ログ / P.56 勢力地変動 / P.35 マルチセーブ / P.54 マップイベントのテスト
/// </summary>
public class SystemIntegrationBatch2Tests
{
    #region Disease Tests

    [Fact]
    public void GetDisease_ValidType_ReturnsDefinition()
    {
        var disease = DiseaseSystem.GetDisease(DiseaseType.Cold);
        Assert.NotNull(disease);
        Assert.Equal("風邪", disease!.Name);
        Assert.True(disease.SelfHealing);
    }

    [Fact]
    public void GetAllDiseases_Returns5Types()
    {
        Assert.Equal(5, DiseaseSystem.GetAllDiseases().Count);
    }

    [Fact]
    public void CalculateTreatmentCost_CurseMostExpensive()
    {
        int cold = DiseaseSystem.CalculateTreatmentCost(DiseaseType.Cold);
        int curse = DiseaseSystem.CalculateTreatmentCost(DiseaseType.CursePlague);
        Assert.True(curse > cold);
    }

    #endregion

    #region Rest Tests

    [Fact]
    public void GetRecoveryRates_DeepSleep_HighestRecovery()
    {
        var deep = RestSystem.GetRecoveryRates(SleepQuality.DeepSleep);
        var nap = RestSystem.GetRecoveryRates(SleepQuality.Nap);
        Assert.True(deep.HpRecovery > nap.HpRecovery);
        Assert.True(deep.FatigueRecovery > nap.FatigueRecovery);
    }

    [Fact]
    public void GetSleepDuration_DeepSleepLongest()
    {
        int deep = RestSystem.GetSleepDuration(SleepQuality.DeepSleep);
        int nap = RestSystem.GetSleepDuration(SleepQuality.Nap);
        Assert.True(deep > nap);
    }

    [Fact]
    public void CanCamp_EnemyNearby_False()
    {
        Assert.False(RestSystem.CanCamp(false, true, 0));
    }

    [Fact]
    public void CanCamp_SurfaceOutdoor_True()
    {
        Assert.True(RestSystem.CanCamp(false, false, 0));
    }

    [Fact]
    public void CalculateAmbushChance_DeepFloor_Higher()
    {
        float shallow = RestSystem.CalculateAmbushChance(1, false, false);
        float deep = RestSystem.CalculateAmbushChance(10, false, false);
        Assert.True(deep > shallow);
    }

    [Fact]
    public void GetQualityName_ReturnsJapanese()
    {
        Assert.Equal("熟睡", RestSystem.GetQualityName(SleepQuality.DeepSleep));
    }

    #endregion

    #region NpcRoutine Tests

    [Fact]
    public void GetRoutine_MerchantMorning_Working()
    {
        var routine = NpcRoutineSystem.GetRoutine("Merchant", TimePeriod.Morning);
        Assert.NotNull(routine);
        Assert.Equal(NpcRoutineSystem.NpcActivity.Working, routine!.Activity);
    }

    [Fact]
    public void IsNpcAvailable_MerchantNight_False()
    {
        Assert.False(NpcRoutineSystem.IsNpcAvailable("Merchant", TimePeriod.Night));
    }

    [Fact]
    public void GetNpcsAtLocation_ShopMorning_HasMerchant()
    {
        var npcs = NpcRoutineSystem.GetNpcsAtLocation("ショップ", TimePeriod.Morning);
        Assert.Contains("Merchant", npcs);
    }

    [Fact]
    public void GetActivityName_ReturnsJapanese()
    {
        Assert.Equal("仕事中", NpcRoutineSystem.GetActivityName(NpcRoutineSystem.NpcActivity.Working));
    }

    #endregion

    #region Encyclopedia Tests

    [Fact]
    public void RegisterAndDiscover_Works()
    {
        var enc = new EncyclopediaSystem();
        enc.RegisterEntry(EncyclopediaCategory.Monster, "goblin", "ゴブリン", 3,
            new() { [1] = "小さな緑色の生物", [2] = "集団で行動する", [3] = "弱点は火属性" });

        Assert.Equal(1, enc.TotalEntries);
        Assert.Equal(0, enc.DiscoveredEntries);
        Assert.Equal("???", enc.GetCurrentDescription("goblin"));

        enc.IncrementDiscovery("goblin");
        Assert.Equal(1, enc.DiscoveredEntries);
        Assert.Contains("小さな緑色の生物", enc.GetCurrentDescription("goblin"));
    }

    [Fact]
    public void GetDiscoveryRate_Calculated()
    {
        var enc = new EncyclopediaSystem();
        enc.RegisterEntry(EncyclopediaCategory.Monster, "goblin", "ゴブリン", 3, new() { [1] = "a" });
        enc.RegisterEntry(EncyclopediaCategory.Monster, "slime", "スライム", 3, new() { [1] = "b" });
        enc.IncrementDiscovery("goblin");
        Assert.Equal(0.5f, enc.GetDiscoveryRate(EncyclopediaCategory.Monster));
    }

    #endregion

    #region DeathLog Tests

    [Fact]
    public void AddLog_IncreasesTotalDeaths()
    {
        var log = new DeathLogSystem();
        Assert.Equal(0, log.TotalDeaths);

        log.AddLog(new DeathLogSystem.DeathLogEntry(
            1, "テスト戦士", CharacterClass.Fighter, Race.Human,
            5, DeathCause.Combat, "ゴブリンに倒された", "王都ダンジョン", 3, 100, DateTime.Now));

        Assert.Equal(1, log.TotalDeaths);
    }

    [Fact]
    public void GetHighestLevel_ReturnsMax()
    {
        var log = new DeathLogSystem();
        log.AddLog(new(1, "A", CharacterClass.Fighter, Race.Human, 5, DeathCause.Combat, "", "", 3, 100, DateTime.Now));
        log.AddLog(new(2, "B", CharacterClass.Mage, Race.Elf, 12, DeathCause.Starvation, "", "", 8, 500, DateTime.Now));
        Assert.Equal(12, log.GetHighestLevel());
    }

    [Fact]
    public void GetDeathsByCategory_GroupsCorrectly()
    {
        var log = new DeathLogSystem();
        log.AddLog(new(1, "A", CharacterClass.Fighter, Race.Human, 5, DeathCause.Combat, "", "", 3, 100, DateTime.Now));
        log.AddLog(new(2, "B", CharacterClass.Mage, Race.Elf, 8, DeathCause.Combat, "", "", 5, 200, DateTime.Now));
        log.AddLog(new(3, "C", CharacterClass.Thief, Race.Orc, 3, DeathCause.Trap, "", "", 2, 50, DateTime.Now));
        var stats = log.GetDeathsByCategory();
        Assert.Equal(2, stats[DeathCause.Combat]);
        Assert.Equal(1, stats[DeathCause.Trap]);
    }

    #endregion

    #region TerritoryInfluence Tests

    [Fact]
    public void Initialize_And_GetDominant()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new() { ["Empire"] = 0.6f, ["Rebels"] = 0.4f });
        Assert.Equal("Empire", system.GetDominantFaction(TerritoryId.Capital));
    }

    [Fact]
    public void ModifyInfluence_ChangesDominant()
    {
        var system = new TerritoryInfluenceSystem();
        system.Initialize(TerritoryId.Capital, new() { ["Empire"] = 0.6f, ["Rebels"] = 0.4f });
        system.ModifyInfluence(TerritoryId.Capital, "Rebels", 0.5f);
        Assert.Equal("Rebels", system.GetDominantFaction(TerritoryId.Capital));
    }

    #endregion

    #region MultiSlotSave Tests

    [Fact]
    public void SaveAndLoad_Works()
    {
        var system = new MultiSlotSaveSystem();
        Assert.True(system.GetSlot(1).IsEmpty);

        system.SaveToSlot(1, "勇者", 10, "王都");
        var slot = system.GetSlot(1);
        Assert.False(slot.IsEmpty);
        Assert.Equal("勇者", slot.CharacterName);
    }

    [Fact]
    public void ClearSlot_RemovesSave()
    {
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "勇者", 10, "王都");
        system.ClearSlot(1);
        Assert.True(system.GetSlot(1).IsEmpty);
    }

    [Fact]
    public void GetEmptySlotCount_Initial5()
    {
        var system = new MultiSlotSaveSystem();
        Assert.Equal(5, system.GetEmptySlotCount());
    }

    #endregion

    #region SymbolMapEvent Tests

    [Fact]
    public void GetAllEvents_ReturnsMultiple()
    {
        Assert.True(SymbolMapEventSystem.GetAllEvents().Count >= 10);
    }

    [Fact]
    public void GetAvailableEvents_FiltersBySeason()
    {
        var spring = SymbolMapEventSystem.GetAvailableEvents(Season.Spring, TerritoryId.Forest);
        Assert.NotEmpty(spring);
    }

    [Fact]
    public void RollEvent_LowRandom_ReturnsEvent()
    {
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Forest, 0.01);
        Assert.NotNull(evt);
    }

    [Fact]
    public void RollEvent_HighRandom_ReturnsNull()
    {
        var evt = SymbolMapEventSystem.RollEvent(Season.Spring, TerritoryId.Forest, 0.99);
        Assert.Null(evt);
    }

    #endregion
}
