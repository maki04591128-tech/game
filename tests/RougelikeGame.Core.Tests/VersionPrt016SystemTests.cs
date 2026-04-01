using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.16 Phase 16 システムテスト
/// Task 1: ショップアイテムID修正
/// Task 2: フロアキャッシュ保存
/// Task 3: レベルアップ時スキルポイント付与
/// Task 4: スキルツリーY反転・行レベル制限
/// Task 5: 水アイテム追加・渇き回復
/// Task 6: 訓練師・図書館司書NPC追加
/// </summary>
public class VersionPrt016SystemTests
{
    #region Task 1: ショップアイテムIDがItemDefinitionsに存在するか

    [Fact]
    public void ShopSystem_GeneralShop_AllItemIds_ExistInItemDefinitions()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 15);
        var items = shop.GetShopItems(FacilityType.GeneralShop);

        Assert.NotEmpty(items);
        foreach (var shopItem in items)
        {
            var created = ItemDefinitions.Create(shopItem.ItemId);
            Assert.True(created != null, $"GeneralShop item '{shopItem.ItemId}' ({shopItem.Name}) not found in ItemDefinitions");
        }
    }

    [Fact]
    public void ShopSystem_WeaponShop_AllItemIds_ExistInItemDefinitions()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.WeaponShop, TerritoryId.Mountain, 15);
        var items = shop.GetShopItems(FacilityType.WeaponShop);

        Assert.NotEmpty(items);
        foreach (var shopItem in items)
        {
            var created = ItemDefinitions.Create(shopItem.ItemId);
            Assert.True(created != null, $"WeaponShop item '{shopItem.ItemId}' ({shopItem.Name}) not found in ItemDefinitions");
        }
    }

    [Fact]
    public void ShopSystem_ArmorShop_AllItemIds_ExistInItemDefinitions()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.ArmorShop, TerritoryId.Capital, 15);
        var items = shop.GetShopItems(FacilityType.ArmorShop);

        Assert.NotEmpty(items);
        foreach (var shopItem in items)
        {
            var created = ItemDefinitions.Create(shopItem.ItemId);
            Assert.True(created != null, $"ArmorShop item '{shopItem.ItemId}' ({shopItem.Name}) not found in ItemDefinitions");
        }
    }

    [Fact]
    public void ShopSystem_MagicShop_AllItemIds_ExistInItemDefinitions()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.MagicShop, TerritoryId.Capital, 20);
        var items = shop.GetShopItems(FacilityType.MagicShop);

        Assert.NotEmpty(items);
        foreach (var shopItem in items)
        {
            var created = ItemDefinitions.Create(shopItem.ItemId);
            Assert.True(created != null, $"MagicShop item '{shopItem.ItemId}' ({shopItem.Name}) not found in ItemDefinitions");
        }
    }

    #endregion

    #region Task 3: スキルポイント付与

    [Fact]
    public void SkillTreeSystem_AddPoints_IncrementsAvailablePoints()
    {
        var tree = new SkillTreeSystem();
        Assert.Equal(0, tree.AvailablePoints);

        tree.AddPoints(1);
        Assert.Equal(1, tree.AvailablePoints);

        tree.AddPoints(2);
        Assert.Equal(3, tree.AvailablePoints);
    }

    #endregion

    #region Task 4: スキルツリーTier2レベル制限

    [Fact]
    public void SkillTreeSystem_Tier2Nodes_RequireLevel5()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(100);

        // Tier1ノードはレベル1で解放可能
        Assert.True(tree.CanUnlock("weapon_sword_1", playerLevel: 1));
        tree.UnlockNode("weapon_sword_1", playerLevel: 1);

        // Tier2ノードはレベル1では解放不可
        Assert.False(tree.CanUnlock("weapon_sword_2", playerLevel: 1));

        // Tier2ノードはレベル5で解放可能
        Assert.True(tree.CanUnlock("weapon_sword_2", playerLevel: 5));
    }

    [Fact]
    public void SkillTreeSystem_Tier3Nodes_RequireLevel10()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(100);

        tree.UnlockNode("weapon_sword_1", playerLevel: 1);
        tree.UnlockNode("weapon_sword_2", playerLevel: 5);

        // Tier3ノードはレベル9では解放不可
        Assert.False(tree.CanUnlock("weapon_sword_3", playerLevel: 9));

        // Tier3ノードはレベル10で解放可能
        Assert.True(tree.CanUnlock("weapon_sword_3", playerLevel: 10));
    }

    [Fact]
    public void SkillTreeSystem_MagicTier2_RequiresLevel5()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(100);

        tree.UnlockNode("magic_fire_1", playerLevel: 1);

        Assert.False(tree.CanUnlock("magic_fire_2", playerLevel: 4));
        Assert.True(tree.CanUnlock("magic_fire_2", playerLevel: 5));
    }

    #endregion

    #region Task 5: 水アイテム

    [Fact]
    public void ItemDefinitions_WaterItems_CanBeCreated()
    {
        var water = ItemDefinitions.Create("food_water");
        Assert.NotNull(water);
        Assert.Equal("水", water!.Name);

        var cleanWater = ItemDefinitions.Create("food_clean_water");
        Assert.NotNull(cleanWater);
        Assert.Equal("清水", cleanWater!.Name);
    }

    [Fact]
    public void WaterItem_HasHydrationValue()
    {
        var water = ItemFactory.CreateWater();
        Assert.Equal(1, water.HydrationValue);
        Assert.Equal(FoodType.Water, water.FoodType);

        var cleanWater = ItemFactory.CreateCleanWater();
        Assert.Equal(2, cleanWater.HydrationValue);
        Assert.Equal(FoodType.CleanWater, cleanWater.FoodType);
    }

    [Fact]
    public void WaterItem_CanBeUsedByPlayer()
    {
        var water = ItemFactory.CreateWater();
        var player = TestHelper.CreateTestPlayer();

        Assert.True(water.CanUse(player));
        var result = water.Use(player);
        Assert.True(result.Success);
    }

    #endregion

    #region Task 6: NPC TileType

    [Fact]
    public void TileType_HasTrainerAndLibrarian()
    {
        Assert.True(Enum.IsDefined(typeof(TileType), TileType.NpcTrainer));
        Assert.True(Enum.IsDefined(typeof(TileType), TileType.NpcLibrarian));
    }

    [Fact]
    public void LocationMapGenerator_TownMap_ContainsTrainerAndLibrarian()
    {
        var generator = new RougelikeGame.Core.Map.Generation.LocationMapGenerator();
        var townMap = generator.GenerateTownMap("test_town", "テスト町");

        // 町マップに建物入口（BuildingEntrance）があることを確認
        bool hasTrainingEntrance = false;
        bool hasLibraryEntrance = false;

        for (int x = 0; x < townMap.Width; x++)
        {
            for (int y = 0; y < townMap.Height; y++)
            {
                var tile = townMap.GetTile(new Position(x, y));
                if (tile.Type == TileType.BuildingEntrance)
                {
                    if (tile.BuildingId == "training") hasTrainingEntrance = true;
                    if (tile.BuildingId == "library") hasLibraryEntrance = true;
                }
            }
        }

        Assert.True(hasTrainingEntrance, "町マップに訓練所の建物入口が配置されていない");
        Assert.True(hasLibraryEntrance, "町マップに図書館の建物入口が配置されていない");

        // 建物内部マップにNPCが配置されていることを確認
        var trainingInterior = generator.GenerateBuildingInterior("training");
        var libraryInterior = generator.GenerateBuildingInterior("library");

        bool hasTrainer = false;
        bool hasLibrarian = false;

        for (int x = 0; x < trainingInterior.Width; x++)
        {
            for (int y = 0; y < trainingInterior.Height; y++)
            {
                if (trainingInterior.GetTile(new Position(x, y)).Type == TileType.NpcTrainer) hasTrainer = true;
            }
        }
        for (int x = 0; x < libraryInterior.Width; x++)
        {
            for (int y = 0; y < libraryInterior.Height; y++)
            {
                if (libraryInterior.GetTile(new Position(x, y)).Type == TileType.NpcLibrarian) hasLibrarian = true;
            }
        }

        Assert.True(hasTrainer, "訓練所の建物内部マップに訓練師NPCが配置されていない");
        Assert.True(hasLibrarian, "図書館の建物内部マップに図書館司書NPCが配置されていない");
    }

    #endregion

    #region ヘルパー

    private static class TestHelper
    {
        public static RougelikeGame.Core.Entities.Player CreateTestPlayer()
        {
            return RougelikeGame.Core.Entities.Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        }
    }

    #endregion
}
