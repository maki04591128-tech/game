using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 世界マップシステムのテスト（Phase 5.12-5.17）
/// </summary>
public class WorldMapSystemTests
{
    private static Player CreateTestPlayer(int level = 1) =>
        Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);

    private static Player CreateHighLevelPlayer()
    {
        var player = CreateTestPlayer();
        for (int i = 0; i < 30; i++)
            player.GainExperience(10000);
        return player;
    }

    private class TestRandom : IRandomProvider
    {
        private readonly int _fixedValue;
        public TestRandom(int fixedValue = 0) => _fixedValue = fixedValue;
        public int Next(int maxValue) => Math.Min(_fixedValue, maxValue > 0 ? maxValue - 1 : 0);
        public int Next(int minValue, int maxValue) => Math.Min(_fixedValue, maxValue - 1);
        public double NextDouble() => 0.5;
    }

    #region TerritoryDefinition Tests

    [Fact]
    public void TerritoryDefinition_GetAll_Returns6Territories()
    {
        var all = TerritoryDefinition.GetAll();
        Assert.Equal(6, all.Count);
    }

    [Theory]
    [InlineData(TerritoryId.Capital, "王都領")]
    [InlineData(TerritoryId.Forest, "森林領")]
    [InlineData(TerritoryId.Mountain, "山岳領")]
    [InlineData(TerritoryId.Coast, "海岸領")]
    [InlineData(TerritoryId.Southern, "南部領")]
    [InlineData(TerritoryId.Frontier, "辺境領")]
    public void TerritoryDefinition_Get_ReturnsCorrectName(TerritoryId id, string expectedName)
    {
        var territory = TerritoryDefinition.Get(id);
        Assert.Equal(expectedName, territory.Name);
    }

    [Fact]
    public void TerritoryDefinition_Capital_HasAllFacilities()
    {
        var capital = TerritoryDefinition.Get(TerritoryId.Capital);
        Assert.Equal(12, capital.AvailableFacilities.Length);
        Assert.Contains(FacilityType.AdventurerGuild, capital.AvailableFacilities);
        Assert.Contains(FacilityType.Arena, capital.AvailableFacilities);
    }

    [Fact]
    public void TerritoryDefinition_Frontier_HasMinimalFacilities()
    {
        var frontier = TerritoryDefinition.Get(TerritoryId.Frontier);
        Assert.Equal(2, frontier.AvailableFacilities.Length);
    }

    [Fact]
    public void TerritoryDefinition_Capital_HasZeroTravelCost()
    {
        var capital = TerritoryDefinition.Get(TerritoryId.Capital);
        Assert.Equal(0, capital.TravelTurnCost);
    }

    [Fact]
    public void TerritoryDefinition_AdjacentTerritories_AreCorrect()
    {
        var capital = TerritoryDefinition.Get(TerritoryId.Capital);
        Assert.Contains(TerritoryId.Forest, capital.AdjacentTerritories);
        Assert.Contains(TerritoryId.Mountain, capital.AdjacentTerritories);
        Assert.Contains(TerritoryId.Coast, capital.AdjacentTerritories);
        Assert.DoesNotContain(TerritoryId.Southern, capital.AdjacentTerritories);
    }

    #endregion

    #region LocationDefinition Tests

    [Fact]
    public void LocationDefinition_GetAll_ReturnsMultipleLocations()
    {
        var all = LocationDefinition.GetAll();
        Assert.True(all.Count > 30);
    }

    [Theory]
    [InlineData(TerritoryId.Capital)]
    [InlineData(TerritoryId.Forest)]
    [InlineData(TerritoryId.Mountain)]
    [InlineData(TerritoryId.Coast)]
    [InlineData(TerritoryId.Southern)]
    [InlineData(TerritoryId.Frontier)]
    public void LocationDefinition_GetByTerritory_ReturnsLocations(TerritoryId territory)
    {
        var locations = LocationDefinition.GetByTerritory(territory);
        Assert.NotEmpty(locations);
    }

    [Fact]
    public void LocationDefinition_GetDungeonsByTerritory_ReturnsOnlyDungeons()
    {
        var dungeons = LocationDefinition.GetDungeonsByTerritory(TerritoryId.Capital);
        Assert.NotEmpty(dungeons);
        Assert.All(dungeons, d => Assert.Equal(LocationType.Dungeon, d.Type));
    }

    [Fact]
    public void LocationDefinition_Get_ReturnsCorrectLocation()
    {
        var loc = LocationDefinition.Get("capital_guild");
        Assert.Equal("冒険者ギルド本部", loc.Name);
        Assert.Equal(TerritoryId.Capital, loc.Territory);
    }

    #endregion

    #region WorldMapSystem Tests

    [Fact]
    public void WorldMapSystem_InitialTerritory_IsCapital()
    {
        var system = new WorldMapSystem();
        Assert.Equal(TerritoryId.Capital, system.CurrentTerritory);
    }

    [Fact]
    public void WorldMapSystem_InitialVisited_ContainsCapital()
    {
        var system = new WorldMapSystem();
        Assert.Contains(TerritoryId.Capital, system.VisitedTerritories);
    }

    [Fact]
    public void WorldMapSystem_IsOnSurface_DefaultTrue()
    {
        var system = new WorldMapSystem();
        Assert.True(system.IsOnSurface);
    }

    [Fact]
    public void WorldMapSystem_CanTravelTo_AdjacentTerritory_WithSufficientLevel()
    {
        var system = new WorldMapSystem();
        Assert.True(system.CanTravelTo(TerritoryId.Coast, 5));
    }

    [Fact]
    public void WorldMapSystem_CanTravelTo_AdjacentTerritory_InsufficientLevel()
    {
        var system = new WorldMapSystem();
        Assert.False(system.CanTravelTo(TerritoryId.Mountain, 1));
    }

    [Fact]
    public void WorldMapSystem_CanTravelTo_NonAdjacentTerritory_ReturnsFalse()
    {
        var system = new WorldMapSystem();
        Assert.False(system.CanTravelTo(TerritoryId.Frontier, 30));
    }

    [Fact]
    public void WorldMapSystem_TravelTo_Success_ChangesTerritory()
    {
        var system = new WorldMapSystem();
        var result = system.TravelTo(TerritoryId.Forest, 10);

        Assert.True(result.Success);
        Assert.Equal(TerritoryId.Forest, system.CurrentTerritory);
        Assert.Contains(TerritoryId.Forest, system.VisitedTerritories);
        Assert.True(result.TurnCost > 0);
    }

    [Fact]
    public void WorldMapSystem_TravelTo_InsufficientLevel_Fails()
    {
        var system = new WorldMapSystem();
        var result = system.TravelTo(TerritoryId.Mountain, 1);

        Assert.False(result.Success);
        Assert.Equal(TerritoryId.Capital, system.CurrentTerritory);
        Assert.Contains("レベルが足りない", result.Message);
    }

    [Fact]
    public void WorldMapSystem_TravelTo_NonAdjacent_Fails()
    {
        var system = new WorldMapSystem();
        var result = system.TravelTo(TerritoryId.Frontier, 30);

        Assert.False(result.Success);
        Assert.Contains("直接移動できない", result.Message);
    }

    [Fact]
    public void WorldMapSystem_TravelTo_SetsIsOnSurface()
    {
        var system = new WorldMapSystem();
        system.IsOnSurface = false;
        system.TravelTo(TerritoryId.Forest, 10);
        Assert.True(system.IsOnSurface);
    }

    [Fact]
    public void WorldMapSystem_SetTerritory_RestoresState()
    {
        var system = new WorldMapSystem();
        var visited = new HashSet<TerritoryId> { TerritoryId.Capital, TerritoryId.Forest, TerritoryId.Mountain };
        system.SetTerritory(TerritoryId.Forest, visited);

        Assert.Equal(TerritoryId.Forest, system.CurrentTerritory);
        Assert.Equal(3, system.VisitedTerritories.Count);
    }

    [Fact]
    public void WorldMapSystem_SetTerritory_AutoAddsCurrentToVisited()
    {
        var system = new WorldMapSystem();
        system.SetTerritory(TerritoryId.Mountain, new HashSet<TerritoryId> { TerritoryId.Capital });

        Assert.Contains(TerritoryId.Mountain, system.VisitedTerritories);
    }

    [Fact]
    public void WorldMapSystem_GetCurrentTerritoryInfo_ReturnsCorrect()
    {
        var system = new WorldMapSystem();
        var info = system.GetCurrentTerritoryInfo();
        Assert.Equal("王都領", info.Name);
    }

    [Fact]
    public void WorldMapSystem_GetAdjacentTerritories_ReturnsCorrect()
    {
        var system = new WorldMapSystem();
        var adj = system.GetAdjacentTerritories();
        Assert.Equal(3, adj.Count);
    }

    [Fact]
    public void WorldMapSystem_GetCurrentLocations_ReturnsLocations()
    {
        var system = new WorldMapSystem();
        var locs = system.GetCurrentLocations();
        Assert.NotEmpty(locs);
        Assert.All(locs, l => Assert.Equal(TerritoryId.Capital, l.Territory));
    }

    [Fact]
    public void WorldMapSystem_GetCurrentDungeons_ReturnsDungeons()
    {
        var system = new WorldMapSystem();
        var dungeons = system.GetCurrentDungeons();
        Assert.NotEmpty(dungeons);
        Assert.All(dungeons, d => Assert.Equal(LocationType.Dungeon, d.Type));
    }

    [Fact]
    public void WorldMapSystem_RollTravelEvent_WithIRandomProvider_ReturnsEvent()
    {
        var system = new WorldMapSystem();
        var random = new TestRandom(0); // 0 < 30 → イベント発生

        var evt = system.RollTravelEvent(TerritoryId.Capital, TerritoryId.Forest, random);
        Assert.NotNull(evt);
    }

    [Fact]
    public void WorldMapSystem_RollTravelEvent_HighRoll_ReturnsNull()
    {
        var system = new WorldMapSystem();
        // 固定値50 → Next(100)で50を返そうとする → min(50, 99) = 50 >= 30 → null
        var random = new TestRandom(50);

        var evt = system.RollTravelEvent(TerritoryId.Capital, TerritoryId.Forest, random);
        Assert.Null(evt);
    }

    #endregion

    #region FacilityDefinition Tests

    [Fact]
    public void FacilityDefinition_GetAll_Returns12Facilities()
    {
        var all = FacilityDefinition.GetAll();
        Assert.Equal(12, all.Count);
    }

    [Fact]
    public void FacilityDefinition_Inn_RequiresGold()
    {
        var inn = FacilityDefinition.Get(FacilityType.Inn);
        Assert.True(inn.RequiresGold);
    }

    [Fact]
    public void FacilityDefinition_Temple_DoesNotRequireGold()
    {
        var temple = FacilityDefinition.Get(FacilityType.Temple);
        Assert.False(temple.RequiresGold);
    }

    #endregion

    #region TownSystem Tests

    [Fact]
    public void TownSystem_GetAvailableFacilities_ReturnsCorrect()
    {
        var town = new TownSystem();
        var facilities = town.GetAvailableFacilities(TerritoryId.Capital);
        Assert.Equal(12, facilities.Count);
    }

    [Fact]
    public void TownSystem_IsFacilityAvailable_Capital_HasInn()
    {
        var town = new TownSystem();
        Assert.True(town.IsFacilityAvailable(TerritoryId.Capital, FacilityType.Inn));
    }

    [Fact]
    public void TownSystem_IsFacilityAvailable_Frontier_NoBank()
    {
        var town = new TownSystem();
        Assert.False(town.IsFacilityAvailable(TerritoryId.Frontier, FacilityType.Bank));
    }

    [Fact]
    public void TownSystem_RestAtInn_Success_HealsPlayer()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        player.AddGold(100);
        player.TakeDamage(Damage.Physical(20));

        var hpBefore = player.CurrentHp;
        var result = town.RestAtInn(player);

        Assert.True(result.Success);
        Assert.Equal(player.MaxHp, player.CurrentHp);
        Assert.True(result.TurnCost > 0);
    }

    [Fact]
    public void TownSystem_RestAtInn_InsufficientGold_Fails()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        // 初期ゴールド(100G)を使い切る
        player.SpendGold(player.Gold);

        var result = town.RestAtInn(player);

        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    [Fact]
    public void TownSystem_RemoveCurseAtChurch_Success()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        player.AddGold(200);
        player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Curse, 100));

        var result = town.RemoveCurseAtChurch(player);

        Assert.True(result.Success);
    }

    [Fact]
    public void TownSystem_DepositGold_Success()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var totalGold = player.Gold; // 初期ゴールド + 500

        var result = town.DepositGold(player, 200);

        Assert.True(result.Success);
        Assert.Equal(200, town.BankBalance);
        Assert.Equal(totalGold - 200, player.Gold);
    }

    [Fact]
    public void TownSystem_DepositGold_InsufficientGold_Fails()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        // 初期ゴールドを使い切る
        player.SpendGold(player.Gold);

        var result = town.DepositGold(player, 100);

        Assert.False(result.Success);
        Assert.Equal(0, town.BankBalance);
    }

    [Fact]
    public void TownSystem_DepositGold_InvalidAmount_Fails()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        player.AddGold(100);

        var result = town.DepositGold(player, 0);

        Assert.False(result.Success);
    }

    [Fact]
    public void TownSystem_WithdrawGold_Success()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var goldBeforeDeposit = player.Gold;
        town.DepositGold(player, 300);
        var goldAfterDeposit = player.Gold; // goldBeforeDeposit - 300

        var result = town.WithdrawGold(player, 100);

        Assert.True(result.Success);
        Assert.Equal(200, town.BankBalance);
        Assert.Equal(goldAfterDeposit + 100, player.Gold);
    }

    [Fact]
    public void TownSystem_WithdrawGold_InsufficientBalance_Fails()
    {
        var town = new TownSystem();
        var player = CreateTestPlayer();

        var result = town.WithdrawGold(player, 100);

        Assert.False(result.Success);
    }

    [Fact]
    public void TownSystem_SetBankBalance_RestoresState()
    {
        var town = new TownSystem();
        town.SetBankBalance(1000);
        Assert.Equal(1000, town.BankBalance);
    }

    [Fact]
    public void TownSystem_SetBankBalance_NegativeClampedToZero()
    {
        var town = new TownSystem();
        town.SetBankBalance(-100);
        Assert.Equal(0, town.BankBalance);
    }

    #endregion

    #region ShopSystem Tests

    [Fact]
    public void ShopSystem_InitializeShop_GeneralShop_HasItems()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.GeneralShop);
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Name == "回復ポーション");
    }

    [Fact]
    public void ShopSystem_InitializeShop_WeaponShop_HasItems()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Name == "鉄の剣");
    }

    [Fact]
    public void ShopSystem_InitializeShop_HighLevel_HasBetterItems()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 15);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        Assert.Contains(items, i => i.Name == "鋼の剣");
    }

    [Fact]
    public void ShopSystem_InitializeShop_ForestTerritory_HasHerbs()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Forest, 1);

        var items = shop.GetShopItems(FacilityType.GeneralShop);
        Assert.Contains(items, i => i.Name == "薬草");
    }

    [Fact]
    public void ShopSystem_InitializeShop_MountainTerritory_HasMithril()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Mountain, 1);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        Assert.Contains(items, i => i.Name == "ミスリルダガー");
    }

    [Fact]
    public void ShopSystem_Buy_Success()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var player = CreateTestPlayer();
        player.AddGold(1000);

        var result = shop.Buy(player, FacilityType.GeneralShop, 0);

        Assert.True(result.Success);
        Assert.NotNull(result.ItemId);
    }

    [Fact]
    public void ShopSystem_Buy_InsufficientGold_Fails()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 1);
        var player = CreateTestPlayer();

        var result = shop.Buy(player, FacilityType.WeaponShop, 0);

        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    [Fact]
    public void ShopSystem_Buy_InvalidIndex_Fails()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var player = CreateTestPlayer();
        player.AddGold(1000);

        var result = shop.Buy(player, FacilityType.GeneralShop, 999);

        Assert.False(result.Success);
    }

    [Fact]
    public void ShopSystem_Buy_ReducesStock()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var player = CreateTestPlayer();
        player.AddGold(10000);

        var stockBefore = shop.GetShopItems(FacilityType.GeneralShop)[0].Stock;
        shop.Buy(player, FacilityType.GeneralShop, 0);
        var stockAfter = shop.GetShopItems(FacilityType.GeneralShop)[0].Stock;

        Assert.Equal(stockBefore - 1, stockAfter);
    }

    [Fact]
    public void ShopSystem_Buy_WithCharismaDiscount_ReducesPrice()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var player = CreateTestPlayer();
        player.AddGold(10000);

        var goldBefore = player.Gold;
        shop.Buy(player, FacilityType.GeneralShop, 0, 0.10); // 10%割引
        var goldSpentWithDiscount = goldBefore - player.Gold;

        // リセットして割引なしで購入
        var shop2 = new ShopSystem();
        shop2.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var player2 = CreateTestPlayer();
        player2.AddGold(10000);
        var goldBefore2 = player2.Gold;
        shop2.Buy(player2, FacilityType.GeneralShop, 0);
        var goldSpentFull = goldBefore2 - player2.Gold;

        Assert.True(goldSpentWithDiscount < goldSpentFull);
    }

    [Fact]
    public void ShopSystem_Sell_AddsGold()
    {
        var shop = new ShopSystem();
        var player = CreateTestPlayer();
        var goldBefore = player.Gold;

        var result = shop.Sell(player, "鉄の剣", 200);

        Assert.True(result.Success);
        Assert.True(player.Gold > goldBefore);
    }

    [Fact]
    public void ShopSystem_Sell_PriceIs40PercentOfBase()
    {
        var shop = new ShopSystem();
        var player = CreateTestPlayer();
        var goldBefore = player.Gold;

        shop.Sell(player, "テスト", 100, 0.0);

        Assert.Equal(goldBefore + 40, player.Gold); // 100 * 0.4 = 40
    }

    [Fact]
    public void ShopSystem_CalculateCharismaDiscount_Base10_Returns0()
    {
        Assert.Equal(0.0, ShopSystem.CalculateCharismaDiscount(10));
    }

    [Fact]
    public void ShopSystem_CalculateCharismaDiscount_High_CapsAt20Percent()
    {
        Assert.Equal(0.20, ShopSystem.CalculateCharismaDiscount(50));
    }

    [Fact]
    public void ShopSystem_GetTerritoryPriceMultiplier_Capital_Is1()
    {
        Assert.Equal(1.0, ShopSystem.GetTerritoryPriceMultiplier(TerritoryId.Capital));
    }

    [Fact]
    public void ShopSystem_GetTerritoryPriceMultiplier_Frontier_IsHigher()
    {
        Assert.True(ShopSystem.GetTerritoryPriceMultiplier(TerritoryId.Frontier) > 1.0);
    }

    [Fact]
    public void ShopSystem_ClearShopInventory_ClearsAll()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        Assert.NotEmpty(shop.GetShopItems(FacilityType.GeneralShop));

        shop.ClearShopInventory();
        Assert.Empty(shop.GetShopItems(FacilityType.GeneralShop));
    }

    [Fact]
    public void ShopSystem_GetShopItems_Uninitialized_ReturnsEmpty()
    {
        var shop = new ShopSystem();
        Assert.Empty(shop.GetShopItems(FacilityType.GeneralShop));
    }

    #endregion

    #region SpecialFloorSystem Tests

    [Fact]
    public void SpecialFloorSystem_BossFloor_OnInterval()
    {
        var random = new TestRandom(0);
        var type = SpecialFloorSystem.DetermineFloorType(5, random);
        Assert.Equal(SpecialFloorType.BossRoom, type);
    }

    [Fact]
    public void SpecialFloorSystem_BossFloor_OnInterval10()
    {
        var random = new TestRandom(0);
        var type = SpecialFloorSystem.DetermineFloorType(10, random);
        Assert.Equal(SpecialFloorType.BossRoom, type);
    }

    [Fact]
    public void SpecialFloorSystem_NormalFloor_HighRoll()
    {
        var random = new TestRandom(50); // 50 >= 15 → Normal
        var type = SpecialFloorSystem.DetermineFloorType(3, random);
        Assert.Equal(SpecialFloorType.Normal, type);
    }

    [Fact]
    public void SpecialFloorSystem_SpecialFloor_LowRoll()
    {
        var random = new TestRandom(0); // 0 < 15 → special
        var type = SpecialFloorSystem.DetermineFloorType(3, random);
        Assert.NotEqual(SpecialFloorType.Normal, type);
    }

    [Fact]
    public void SpecialFloorSystem_GetFloorDescription_ReturnsDescription()
    {
        var desc = SpecialFloorSystem.GetFloorDescription(SpecialFloorType.Shop);
        Assert.Contains("ショップフロア", desc);
    }

    [Fact]
    public void SpecialFloorSystem_GetEnemySpawnMultiplier_Shop_Is0()
    {
        Assert.Equal(0.0, SpecialFloorSystem.GetEnemySpawnMultiplier(SpecialFloorType.Shop));
    }

    [Fact]
    public void SpecialFloorSystem_GetEnemySpawnMultiplier_TreasureVault_IsHigh()
    {
        Assert.True(SpecialFloorSystem.GetEnemySpawnMultiplier(SpecialFloorType.TreasureVault) > 1.0);
    }

    [Fact]
    public void SpecialFloorSystem_GetEnemySpawnMultiplier_Normal_Is1()
    {
        Assert.Equal(1.0, SpecialFloorSystem.GetEnemySpawnMultiplier(SpecialFloorType.Normal));
    }

    #endregion

    #region RandomEventSystem Tests

    [Fact]
    public void RandomEventSystem_RollEvent_LowRoll_ReturnsEvent()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // 0 < 10 → event
        var evt = system.RollEvent(5, random);
        Assert.NotNull(evt);
    }

    [Fact]
    public void RandomEventSystem_RollEvent_HighRoll_ReturnsNull()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(50); // 50 >= 10 → null
        var evt = system.RollEvent(5, random);
        Assert.Null(evt);
    }

    [Fact]
    public void RandomEventSystem_RollEvent_FiltersByDepth()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0);
        // Depth 1 should not include events with MinDepth > 1
        var evt = system.RollEvent(1, random);
        if (evt != null)
        {
            Assert.True(1 >= evt.MinDepth);
        }
    }

    [Fact]
    public void RandomEventSystem_ResolveTreasureChest_LowRoll_IsTrap()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // 0 < 20 → trap
        var result = system.ResolveTreasureChest(5, random);
        Assert.False(result.IsPositive);
        Assert.True(result.DamageAmount > 0);
    }

    [Fact]
    public void RandomEventSystem_ResolveTreasureChest_HighRoll_GivesGold()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(50); // 50 >= 20 → gold
        var result = system.ResolveTreasureChest(5, random);
        Assert.True(result.IsPositive);
        Assert.True(result.GoldAmount > 0);
    }

    [Fact]
    public void RandomEventSystem_ResolveFountain_LowRoll_HealsHp()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // 0 < 50 → HP heal
        var result = system.ResolveFountain(random);
        Assert.True(result.IsPositive);
        Assert.Equal(30, result.HealAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveFountain_MidRoll_HealsMp()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(60); // 50 <= 60 < 80 → MP heal
        var result = system.ResolveFountain(random);
        Assert.True(result.IsPositive);
        Assert.Equal(20, result.MpHealAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveFountain_HighRoll_IsPoison()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(90); // >= 80 → poison
        var result = system.ResolveFountain(random);
        Assert.False(result.IsPositive);
        Assert.Equal(10, result.DamageAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveShrine_LowRoll_BuffEffect()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // < 40 → buff
        var result = system.ResolveShrine(random);
        Assert.True(result.IsPositive);
        Assert.Contains("上昇", result.Message);
    }

    [Fact]
    public void RandomEventSystem_ResolveShrine_MidRoll_FullHeal()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(50); // 40 <= 50 < 70 → full heal
        var result = system.ResolveShrine(random);
        Assert.True(result.IsPositive);
        Assert.Equal(9999, result.HealAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveRestPoint_HealsSmallAmount()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveRestPoint();
        Assert.True(result.IsPositive);
        Assert.Equal(20, result.HealAmount);
        Assert.Equal(10, result.MpHealAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveTrap_DealsDamage()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0);
        var result = system.ResolveTrap(5, random);
        Assert.False(result.IsPositive);
        Assert.True(result.DamageAmount > 0);
    }

    [Fact]
    public void RandomEventSystem_ResolveRuins_LowRoll_GivesGold()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // < 40 → gold
        var result = system.ResolveRuins(5, random);
        Assert.True(result.IsPositive);
        Assert.True(result.GoldAmount > 0);
    }

    [Fact]
    public void RandomEventSystem_ResolveMysteriousItem_LowRoll_Positive()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(0); // < 60 → positive
        var result = system.ResolveMysteriousItem(random);
        Assert.True(result.IsPositive);
    }

    [Fact]
    public void RandomEventSystem_ResolveMysteriousItem_HighRoll_Trap()
    {
        var system = new RandomEventSystem();
        var random = new TestRandom(90); // >= 85 → trap
        var result = system.ResolveMysteriousItem(random);
        Assert.False(result.IsPositive);
        Assert.True(result.DamageAmount > 0);
    }

    #endregion

    #region TravelEventType Tests

    [Fact]
    public void TravelEvent_AllTypesExist()
    {
        Assert.Equal(6, Enum.GetValues<TravelEventType>().Length);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_TravelChain_CapitalToFrontier()
    {
        var system = new WorldMapSystem();

        // Capital → Mountain (要Lv10)
        var r1 = system.TravelTo(TerritoryId.Mountain, 10);
        Assert.True(r1.Success);

        // Mountain → Frontier (要Lv20)
        var r2 = system.TravelTo(TerritoryId.Frontier, 20);
        Assert.True(r2.Success);

        Assert.Equal(TerritoryId.Frontier, system.CurrentTerritory);
        Assert.Equal(3, system.VisitedTerritories.Count);
    }

    [Fact]
    public void Integration_ShopAndBank_Workflow()
    {
        var town = new TownSystem();
        var shop = new ShopSystem();
        var player = CreateTestPlayer();
        var initialGold = player.Gold; // 初期ゴールド（素性による）
        player.AddGold(1000);
        var totalGold = player.Gold; // initialGold + 1000

        // 銀行に500預ける
        town.DepositGold(player, 500);
        Assert.Equal(500, town.BankBalance);
        Assert.Equal(totalGold - 500, player.Gold);

        // ショップで購入
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var goldBeforeBuy = player.Gold;
        shop.Buy(player, FacilityType.GeneralShop, 0); // 回復ポーション50G

        Assert.Equal(goldBeforeBuy - 50, player.Gold);

        // 銀行から引き出し
        var goldBeforeWithdraw = player.Gold;
        town.WithdrawGold(player, 200);
        Assert.Equal(300, town.BankBalance);
        Assert.Equal(goldBeforeWithdraw + 200, player.Gold);
    }

    [Fact]
    public void Integration_TerritoryShopPrices_Differ()
    {
        var shop1 = new ShopSystem();
        shop1.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var capitalPrice = shop1.GetShopItems(FacilityType.GeneralShop)[0].BasePrice;

        var shop2 = new ShopSystem();
        shop2.InitializeShop(FacilityType.GeneralShop, TerritoryId.Frontier, 1);
        var frontierPrice = shop2.GetShopItems(FacilityType.GeneralShop)[0].BasePrice;

        Assert.True(frontierPrice > capitalPrice);
    }

    #endregion

    #region Grid Shop Tests (PoE-style)

    [Fact]
    public void ShopSystem_ShopItem_HasGridSize()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        var sword = items.FirstOrDefault(i => i.Name == "鉄の剣");
        Assert.NotNull(sword);
        Assert.Equal(GridItemSize.Size1x2, sword.GridSize);
    }

    [Fact]
    public void ShopSystem_GeneralShop_ItemsHaveGridSize1x1()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.GeneralShop);
        var potion = items.FirstOrDefault(i => i.Name == "回復ポーション");
        Assert.NotNull(potion);
        Assert.Equal(GridItemSize.Size1x1, potion.GridSize);
    }

    [Fact]
    public void ShopSystem_ArmorShop_ArmorHasLargeGridSize()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.ArmorShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.ArmorShop);
        var armor = items.FirstOrDefault(i => i.Name == "革鎧");
        Assert.NotNull(armor);
        Assert.Equal(GridItemSize.Size2x3, armor.GridSize);
    }

    [Fact]
    public void ShopSystem_ArmorShop_ShieldHasSize2x2()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.ArmorShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.ArmorShop);
        var shield = items.FirstOrDefault(i => i.Name == "鉄の盾");
        Assert.NotNull(shield);
        Assert.Equal(GridItemSize.Size2x2, shield.GridSize);
    }

    [Fact]
    public void ShopSystem_WeaponShop_BowHasSize2x3()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        var bow = items.FirstOrDefault(i => i.Name == "短弓");
        Assert.NotNull(bow);
        Assert.Equal(GridItemSize.Size2x3, bow.GridSize);
    }

    [Fact]
    public void ShopSystem_MagicShop_WandHasSize1x2()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.MagicShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.MagicShop);
        var wand = items.FirstOrDefault(i => i.Name == "火の杖");
        Assert.NotNull(wand);
        Assert.Equal(GridItemSize.Size1x2, wand.GridSize);
    }

    [Fact]
    public void ShopSystem_AllItems_HaveValidGridSize()
    {
        var shop = new ShopSystem();
        foreach (var shopType in new[] { FacilityType.GeneralShop, FacilityType.WeaponShop, FacilityType.ArmorShop, FacilityType.MagicShop })
        {
            shop.InitializeShop(shopType, TerritoryId.Capital, 15);
            var items = shop.GetShopItems(shopType);
            foreach (var item in items)
            {
                var (w, h) = GridInventorySystem.GetDimensions(item.GridSize);
                Assert.True(w >= 1 && w <= 3, $"グリッド幅が不正: {item.Name} (w={w})");
                Assert.True(h >= 1 && h <= 3, $"グリッド高さが不正: {item.Name} (h={h})");
            }
        }
    }

    [Fact]
    public void ShopSystem_GridShopItems_CanFitInGrid()
    {
        // 10x9のグリッドに全ショップアイテムが配置可能か検証
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Capital, 1);

        var items = shop.GetShopItems(FacilityType.WeaponShop);
        var grid = new GridInventorySystem(10, 9);
        int placedCount = 0;

        foreach (var item in items)
        {
            if (item.Stock <= 0) continue;
            // 自動配置: 空き位置を探す
            bool placed = false;
            for (int y = 0; y <= 9 - GridInventorySystem.GetDimensions(item.GridSize).H; y++)
            {
                for (int x = 0; x <= 10 - GridInventorySystem.GetDimensions(item.GridSize).W; x++)
                {
                    if (grid.CanPlace(item.GridSize, x, y))
                    {
                        grid.PlaceItem(item.ItemId + placedCount, item.Name, item.GridSize, x, y);
                        placed = true;
                        placedCount++;
                        break;
                    }
                }
                if (placed) break;
            }
        }

        Assert.True(placedCount > 0, "少なくとも1つのアイテムが配置できるはず");
    }

    [Fact]
    public void ShopSystem_DefaultGridSize_IsSize1x1()
    {
        // ShopItemのGridSizeデフォルト値はSize1x1
        var item = new ShopSystem.ShopItem("test", "テスト", 100, 1, FacilityType.GeneralShop);
        Assert.Equal(GridItemSize.Size1x1, item.GridSize);
    }

    #endregion
}
