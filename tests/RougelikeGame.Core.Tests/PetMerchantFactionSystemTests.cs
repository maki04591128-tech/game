using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// ペット・商人ギルド・陣営戦争システム追加テスト
/// テスト数: 30件
/// </summary>
public class PetMerchantFactionSystemTests
{
    // ============================================================
    // PetSystem テスト
    // ============================================================

    [Fact]
    public void Pet_GetDefinition_AllTypes()
    {
        var system = new PetSystem();
        foreach (PetType type in Enum.GetValues<PetType>())
        {
            var def = system.GetDefinition(type);
            Assert.NotNull(def);
            Assert.NotEmpty(def.DefaultName);
        }
    }

    [Fact]
    public void Pet_AddPet_CreatesWithDefaults()
    {
        var system = new PetSystem();
        var pet = system.AddPet("pet1", "タロウ", PetType.Wolf);
        Assert.Equal("タロウ", pet.Name);
        Assert.Equal(PetType.Wolf, pet.Type);
        Assert.Equal(1, pet.Level);
        Assert.Equal(100, pet.Hunger);
        Assert.Equal(50, pet.Loyalty);
        Assert.False(pet.IsRiding);
    }

    [Fact]
    public void Pet_Feed_IncreasesHungerAndLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Cat);
        var updated = system.Feed("pet1", 30, 10);
        Assert.Equal(100, updated.Hunger); // 100 + 30 → capped at 100
        Assert.Equal(60, updated.Loyalty); // 50 + 10
    }

    [Fact]
    public void Pet_Train_IncreasesLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Wolf);
        var updated = system.Train("pet1", 15);
        Assert.Equal(65, updated.Loyalty); // 50 + 15
    }

    [Fact]
    public void Pet_Train_ClampsLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Wolf);
        var updated = system.Train("pet1", 200);
        Assert.Equal(100, updated.Loyalty);
    }

    [Fact]
    public void Pet_ToggleRide_Horse_Works()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Horse);
        var riding = system.ToggleRide("pet1");
        Assert.True(riding.IsRiding);
        var dismounted = system.ToggleRide("pet1");
        Assert.False(dismounted.IsRiding);
    }

    [Fact]
    public void Pet_ToggleRide_Cat_CannotRide()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Cat);
        var result = system.ToggleRide("pet1");
        Assert.False(result.IsRiding);
    }

    [Fact]
    public void Pet_GetMoveSpeedMultiplier_Riding_Horse()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Horse);
        system.ToggleRide("pet1");
        Assert.Equal(2.0f, system.GetMoveSpeedMultiplier("pet1"));
    }

    [Fact]
    public void Pet_GetMoveSpeedMultiplier_NotRiding_Returns1()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Horse);
        Assert.Equal(1.0f, system.GetMoveSpeedMultiplier("pet1"));
    }

    [Fact]
    public void Pet_TickHunger_DecreasesHunger()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Wolf);
        var updated = system.TickHunger("pet1", 10);
        Assert.Equal(90, updated.Hunger);
    }

    [Fact]
    public void Pet_TickHunger_ZeroHunger_DecreasesLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Wolf);
        // 空腹を0にする
        system.TickHunger("pet1", 100);
        var starving = system.Pets["pet1"];
        Assert.Equal(0, starving.Hunger);
        Assert.True(starving.Loyalty < 50); // 忠誠度が減っている
    }

    [Fact]
    public void Pet_Reset_ClearsAllPets()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト1", PetType.Wolf);
        system.AddPet("pet2", "テスト2", PetType.Cat);
        system.Reset();
        Assert.Empty(system.Pets);
    }

    [Fact]
    public void Pet_GetObedienceRate_BasedOnLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("pet1", "テスト", PetType.Wolf);
        Assert.Equal(50, system.GetObedienceRate("pet1")); // 初期忠誠度50
    }

    // ============================================================
    // MerchantGuildSystem テスト
    // ============================================================

    [Fact]
    public void Merchant_JoinGuild_CreatesMembership()
    {
        var system = new MerchantGuildSystem();
        var membership = system.JoinGuild("player1");
        Assert.NotNull(membership);
        Assert.True(system.IsMember);
        Assert.Equal(GuildRank.None, membership.Rank);
    }

    [Fact]
    public void Merchant_EstablishRoute_NotMember_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        var route = system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.Null(route);
    }

    [Fact]
    public void Merchant_EstablishRoute_CreatesRoute()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        var route = system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.NotNull(route);
        Assert.Equal(TradeRouteStatus.Open, route.Status);
        Assert.Single(system.Routes);
    }

    [Fact]
    public void Merchant_EstablishRoute_DuplicateRoute_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        var duplicate = system.EstablishRoute("route2", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.Null(duplicate);
    }

    [Fact]
    public void Merchant_ExecuteTrade_ReturnsProfit()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ExecuteTrade("route1", 1000);
        Assert.NotNull(result);
        Assert.True(result.ActualProfit > 0);
    }

    [Fact]
    public void Merchant_ExecuteTrade_NotMember_ReturnsNull()
    {
        var system = new MerchantGuildSystem();
        Assert.Null(system.ExecuteTrade("route1", 1000));
    }

    [Fact]
    public void Merchant_UpdateRouteStatus_ChangesStatus()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.UpdateRouteStatus("route1", TradeRouteStatus.Blocked);
        Assert.Equal(TradeRouteStatus.Blocked, system.Routes[0].Status);
    }

    [Fact]
    public void Merchant_Reset_ClearsMembershipAndRoutes()
    {
        var system = new MerchantGuildSystem();
        system.JoinGuild("player1");
        system.EstablishRoute("route1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.Reset();
        Assert.False(system.IsMember);
        Assert.Empty(system.Routes);
    }

    [Fact]
    public void Merchant_GetRankBonus_IncreasesWithRank()
    {
        Assert.True(MerchantGuildSystem.GetRankBonus(GuildRank.Gold) >
                    MerchantGuildSystem.GetRankBonus(GuildRank.Copper));
    }

    // ============================================================
    // FactionWarSystem テスト
    // ============================================================

    [Fact]
    public void FactionWar_StartWar_AddsActiveWar()
    {
        var system = new FactionWarSystem();
        var war = system.StartWar("war1", "王都vs森林", TerritoryId.Capital, TerritoryId.Forest, 100);
        Assert.Equal(WarPhase.Tension, war.Phase);
        Assert.Single(system.ActiveWars);
    }

    [Fact]
    public void FactionWar_AdvancePhase_ProgressesCorrectly()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var advanced = system.AdvancePhase("war1", 110);
        Assert.NotNull(advanced);
        Assert.Equal(WarPhase.Skirmish, advanced.Phase);
    }

    [Fact]
    public void FactionWar_AdvancePhase_FullCycle()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.AdvancePhase("war1", 110); // Tension → Skirmish
        system.AdvancePhase("war1", 120); // Skirmish → Battle
        system.AdvancePhase("war1", 130); // Battle → Aftermath
        var peace = system.AdvancePhase("war1", 140); // Aftermath → Peace
        Assert.Equal(WarPhase.Peace, peace!.Phase);
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
    public void FactionWar_ChooseAlignment_Mercenary_GetsGold()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var result = system.ChooseAlignment("war1", FactionAlignment.Mercenary);
        Assert.Equal(500, result!.GoldReward);
    }

    [Fact]
    public void FactionWar_ResolveWar_MovesToHistory()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        var outcome = system.ResolveWar("war1", TerritoryId.Capital, 30);
        Assert.NotNull(outcome);
        Assert.Equal(TerritoryId.Capital, outcome.Winner);
        Assert.Equal(TerritoryId.Forest, outcome.Loser);
        Assert.Empty(system.ActiveWars);
        Assert.Single(system.WarHistory);
    }

    [Fact]
    public void FactionWar_GetWarsInvolving_FiltersCorrectly()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト1", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.StartWar("war2", "テスト2", TerritoryId.Mountain, TerritoryId.Coast, 100);
        var capitalWars = system.GetWarsInvolving(TerritoryId.Capital);
        Assert.Single(capitalWars);
    }

    [Fact]
    public void FactionWar_Reset_ClearsAll()
    {
        var system = new FactionWarSystem();
        system.StartWar("war1", "テスト", TerritoryId.Capital, TerritoryId.Forest, 100);
        system.ResolveWar("war1", TerritoryId.Capital, 30);
        system.StartWar("war2", "テスト2", TerritoryId.Mountain, TerritoryId.Coast, 200);
        system.Reset();
        Assert.Empty(system.ActiveWars);
        Assert.Empty(system.WarHistory);
    }

    [Theory]
    [InlineData(WarPhase.Tension)]
    [InlineData(WarPhase.Skirmish)]
    [InlineData(WarPhase.Battle)]
    [InlineData(WarPhase.Aftermath)]
    [InlineData(WarPhase.Peace)]
    public void FactionWar_GetPhaseDescription_ReturnsNonEmpty(WarPhase phase)
    {
        Assert.NotEmpty(FactionWarSystem.GetPhaseDescription(phase));
    }
}
