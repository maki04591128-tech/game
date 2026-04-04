using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;
using EquipmentSlot = RougelikeGame.Core.Items.EquipmentSlot;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 敵・アイテム拡張システムのテスト (Phase 5.24-5.28)
/// </summary>
public class EnemyItemExpansionTests
{
    private static Player CreateTestPlayer()
    {
        return Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
    }

    #region EnemyDefinitions - Territory Enemies

    [Fact]
    public void EnemyDefinitions_GetAllEnemies_Returns29Enemies()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        Assert.Equal(31, all.Count);
    }

    [Fact]
    public void EnemyDefinitions_GetAllBosses_Returns6Bosses()
    {
        var bosses = EnemyDefinitions.GetAllBosses();
        Assert.Equal(6, bosses.Count);
        Assert.All(bosses, b => Assert.True(b.Rank == EnemyRank.Boss || b.Rank == EnemyRank.HiddenBoss));
    }

    [Theory]
    [InlineData(TerritoryId.Capital, 4)]
    [InlineData(TerritoryId.Forest, 4)]
    [InlineData(TerritoryId.Mountain, 5)]
    [InlineData(TerritoryId.Coast, 4)]
    [InlineData(TerritoryId.Southern, 4)]
    [InlineData(TerritoryId.Frontier, 5)]
    public void EnemyDefinitions_GetEnemiesForTerritory_ReturnsCorrectCount(TerritoryId territory, int expectedCount)
    {
        var enemies = EnemyDefinitions.GetEnemiesForTerritory(territory);
        Assert.Equal(expectedCount, enemies.Count);
    }

    [Theory]
    [InlineData(TerritoryId.Forest, "boss_forest_guardian")]
    [InlineData(TerritoryId.Mountain, "boss_mountain_king")]
    [InlineData(TerritoryId.Coast, "boss_leviathan")]
    [InlineData(TerritoryId.Southern, "boss_pharaoh")]
    [InlineData(TerritoryId.Frontier, "boss_dragon")]
    public void EnemyDefinitions_GetBossForTerritory_ReturnsCorrectBoss(TerritoryId territory, string expectedTypeId)
    {
        var boss = EnemyDefinitions.GetBossForTerritory(territory);
        Assert.NotNull(boss);
        Assert.Equal(expectedTypeId, boss!.TypeId);
    }

    [Fact]
    public void EnemyDefinitions_GetBossForTerritory_Capital_ReturnsNull()
    {
        var boss = EnemyDefinitions.GetBossForTerritory(TerritoryId.Capital);
        Assert.Null(boss);
    }

    [Fact]
    public void EnemyDefinitions_ForestWolf_HasPackType()
    {
        Assert.Equal(EnemyType.Pack, EnemyDefinitions.ForestWolf.EnemyType);
    }

    [Fact]
    public void EnemyDefinitions_MountainGolem_HasDefensiveType()
    {
        Assert.Equal(EnemyType.Defensive, EnemyDefinitions.MountainGolem.EnemyType);
    }

    [Fact]
    public void EnemyDefinitions_AbyssLord_IsHiddenBoss()
    {
        Assert.Equal(EnemyRank.HiddenBoss, EnemyDefinitions.AbyssLord.Rank);
        Assert.Equal(1000, EnemyDefinitions.AbyssLord.ExperienceReward);
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(5, 4)]
    [InlineData(10, 4)]
    [InlineData(15, 4)]
    [InlineData(25, 3)]
    public void EnemyDefinitions_GetEnemiesForDepth_ReturnsNonEmpty(int depth, int expectedCount)
    {
        var enemies = EnemyDefinitions.GetEnemiesForDepth(depth);
        Assert.Equal(expectedCount, enemies.Count);
    }

    #endregion

    #region EnemyFactory

    [Fact]
    public void EnemyFactory_CreateEnemy_ForestWolf_SetsCorrectProperties()
    {
        var factory = new EnemyFactory();
        var enemy = factory.CreateEnemy(EnemyDefinitions.ForestWolf, new Position(5, 5));

        Assert.Equal("森狼", enemy.Name);
        Assert.Equal("forest_wolf", enemy.EnemyTypeId);
        Assert.Equal(Faction.Enemy, enemy.Faction);
        Assert.NotNull(enemy.Behavior);
    }

    [Fact]
    public void EnemyFactory_CreateEnemy_Boss_HasBehavior()
    {
        var factory = new EnemyFactory();
        var enemy = factory.CreateEnemy(EnemyDefinitions.ForestGuardian, new Position(10, 10));

        Assert.Equal("森の守護者", enemy.Name);
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.ForestGuardian.Rank);
        Assert.NotNull(enemy.Behavior);
    }

    #endregion

    #region AI Behaviors

    [Fact]
    public void RangedBehavior_HasCorrectNameAndPriority()
    {
        var behavior = new RangedBehavior();
        Assert.Equal("Ranged", behavior.Name);
        Assert.Equal(55, behavior.Priority);
    }

    [Fact]
    public void BerserkerBehavior_HasCorrectNameAndPriority()
    {
        var behavior = new BerserkerBehavior();
        Assert.Equal("Berserker", behavior.Name);
        Assert.Equal(80, behavior.Priority);
    }

    [Fact]
    public void SummonerBehavior_HasCorrectNameAndPriority()
    {
        var behavior = new SummonerBehavior();
        Assert.Equal("Summoner", behavior.Name);
        Assert.Equal(65, behavior.Priority);
    }

    [Fact]
    public void RangedBehavior_CustomRange_SetsCorrectly()
    {
        var behavior = new RangedBehavior(preferredRange: 8, minRange: 4);
        Assert.Equal("Ranged", behavior.Name);
    }

    [Fact]
    public void BerserkerBehavior_CustomThreshold_SetsCorrectly()
    {
        var behavior = new BerserkerBehavior(berserkThreshold: 0.5f);
        Assert.Equal("Berserker", behavior.Name);
    }

    [Fact]
    public void SummonerBehavior_CustomCooldown_SetsCorrectly()
    {
        var behavior = new SummonerBehavior(summonCooldown: 10);
        Assert.Equal("Summoner", behavior.Name);
    }

    #endregion

    #region ItemDefinitions - New Weapons

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewWeapons()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("weapon_greatsword", ids);
        Assert.Contains("weapon_spear", ids);
        Assert.Contains("weapon_war_hammer", ids);
        Assert.Contains("weapon_crossbow", ids);
        Assert.Contains("weapon_whip", ids);
    }

    [Fact]
    public void CreateGreatsword_HasCorrectProperties()
    {
        var weapon = ItemFactory.CreateGreatsword();
        Assert.Equal("グレートソード", weapon.Name);
        Assert.Equal(WeaponType.Greatsword, weapon.WeaponType);
        Assert.Equal(22, weapon.BaseDamage);
        Assert.True(weapon.IsTwoHanded);
        Assert.Equal(ItemRarity.Rare, weapon.Rarity);
    }

    [Fact]
    public void CreateSpear_HasCorrectProperties()
    {
        var weapon = ItemFactory.CreateSpear();
        Assert.Equal("槍", weapon.Name);
        Assert.Equal(WeaponType.Spear, weapon.WeaponType);
        Assert.Equal(2, weapon.Range);
        Assert.True(weapon.IsTwoHanded);
    }

    [Fact]
    public void CreateCrossbow_HasRangedAttackType()
    {
        var weapon = ItemFactory.CreateCrossbow();
        Assert.Equal(AttackType.Ranged, weapon.AttackType);
        Assert.Equal(8, weapon.Range);
    }

    #endregion

    #region ItemDefinitions - New Armor & Accessories

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewArmor()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("armor_iron_helm", ids);
        Assert.Contains("armor_leather_gloves", ids);
        Assert.Contains("armor_iron_boots", ids);
    }

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewAccessories()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("accessory_iron_ring", ids);
        Assert.Contains("accessory_protection_amulet", ids);
        Assert.Contains("accessory_speed_cloak", ids);
    }

    [Fact]
    public void CreateIronHelm_HasCorrectSlot()
    {
        var helm = ItemFactory.CreateIronHelm();
        Assert.Equal(EquipmentSlot.Head, helm.Slot);
        Assert.Equal(6, helm.BaseDefense);
    }

    [Fact]
    public void CreateLeatherGloves_HasCorrectSlot()
    {
        var gloves = ItemFactory.CreateLeatherGloves();
        Assert.Equal(EquipmentSlot.Hands, gloves.Slot);
    }

    [Fact]
    public void CreateIronBoots_HasCorrectSlot()
    {
        var boots = ItemFactory.CreateIronBoots();
        Assert.Equal(EquipmentSlot.Feet, boots.Slot);
    }

    [Fact]
    public void CreateIronRing_HasCorrectSlot()
    {
        var ring = ItemFactory.CreateIronRing();
        Assert.Equal(EquipmentSlot.Ring1, ring.Slot);
    }

    [Fact]
    public void CreateProtectionAmulet_HasCorrectSlot()
    {
        var amulet = ItemFactory.CreateProtectionAmulet();
        Assert.Equal(EquipmentSlot.Neck, amulet.Slot);
        Assert.Equal("MagicDefenseUp", amulet.PassiveAbility);
    }

    [Fact]
    public void CreateSpeedCloak_HasCorrectSlot()
    {
        var cloak = ItemFactory.CreateSpeedCloak();
        Assert.Equal(EquipmentSlot.Back, cloak.Slot);
        Assert.Equal("SpeedUp", cloak.PassiveAbility);
    }

    #endregion

    #region ItemDefinitions - New Consumables

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewPotions()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("potion_healing_super", ids);
        Assert.Contains("potion_mana", ids);
        Assert.Contains("potion_strength", ids);
        Assert.Contains("potion_agility", ids);
        Assert.Contains("potion_invisibility", ids);
        Assert.Contains("potion_fire_resist", ids);
        Assert.Contains("potion_cold_resist", ids);
        Assert.Contains("potion_cure_all", ids);
    }

    [Fact]
    public void CreateSuperHealingPotion_HasCorrectEffect()
    {
        var potion = ItemFactory.CreateSuperHealingPotion();
        Assert.Equal(0, potion.EffectValue);  // L-2: EffectPercentageベースに変更
        Assert.Equal(0.80f, potion.EffectPercentage);  // MaxHP80%回復
        Assert.Equal(PotionType.HealingSuper, potion.PotionType);
        Assert.Equal(ItemRarity.Rare, potion.Rarity);
    }

    [Fact]
    public void CreateStrengthPotion_HasDuration()
    {
        var potion = ItemFactory.CreateStrengthPotion();
        Assert.Equal(30, potion.Duration);
        Assert.Equal(PotionType.StrengthBoost, potion.PotionType);
    }

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewFood()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("food_emergency_ration", ids);
        Assert.Contains("food_lembas", ids);
        Assert.Contains("food_fruit", ids);
    }

    [Fact]
    public void CreateLembas_HasHighNutrition()
    {
        var lembas = ItemFactory.CreateLembas();
        Assert.Equal(100, lembas.NutritionValue);
        Assert.Equal(20, lembas.HealValue);
        Assert.Equal(ItemRarity.Rare, lembas.Rarity);
    }

    [Fact]
    public void ItemDefinitions_GetAllItemIds_ContainsNewScrolls()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.Contains("scroll_fireball", ids);
        Assert.Contains("scroll_lightning", ids);
        Assert.Contains("scroll_freeze", ids);
        Assert.Contains("scroll_remove_curse", ids);
        Assert.Contains("scroll_enchant", ids);
        Assert.Contains("scroll_return", ids);
        Assert.Contains("scroll_sanctuary", ids);
    }

    [Fact]
    public void CreateScrollOfFireball_HasCorrectProperties()
    {
        var scroll = ItemFactory.CreateScrollOfFireball();
        Assert.Equal(ScrollType.Fireball, scroll.ScrollType);
        Assert.Equal(TargetType.SingleEnemy, scroll.TargetType);
        Assert.Equal(40, scroll.EffectValue);
    }

    [Fact]
    public void CreateScrollOfFreeze_HasAreaTarget()
    {
        var scroll = ItemFactory.CreateScrollOfFreeze();
        Assert.Equal(TargetType.Area, scroll.TargetType);
        Assert.Equal(3, scroll.EffectRadius);
    }

    #endregion

    #region ItemDefinitions - Total Count

    [Fact]
    public void ItemDefinitions_TotalItemCount()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        // 武器13 + 防具9 + アクセサリ3 + ポーション12 + 食料8 + 巻物12 + 素材27 + 追加素材20 + 追加4 = 108
        Assert.Equal(108, ids.Count);
    }

    [Fact]
    public void ItemDefinitions_Create_ReturnsItemForAllIds()
    {
        foreach (var id in ItemDefinitions.GetAllItemIds())
        {
            var item = ItemDefinitions.Create(id);
            Assert.NotNull(item);
        }
    }

    #endregion

    #region CraftingSystem - Recipes

    [Fact]
    public void CraftingSystem_HasDefaultRecipes()
    {
        var system = new CraftingSystem();
        var recipes = system.GetAllRecipes();
        Assert.True(recipes.Count >= 10);
    }

    [Fact]
    public void CraftingSystem_GetAvailableRecipes_Level1_FiltersCorrectly()
    {
        var system = new CraftingSystem();
        var recipes = system.GetAvailableRecipes(1);
        Assert.All(recipes, r => Assert.True(r.RequiredLevel <= 1));
    }

    [Fact]
    public void CraftingSystem_GetAvailableRecipes_Level10_ReturnsAll()
    {
        var system = new CraftingSystem();
        var allRecipes = system.GetAllRecipes();
        var available = system.GetAvailableRecipes(10);
        Assert.Equal(allRecipes.Count, available.Count);
    }

    [Fact]
    public void CraftingSystem_GetRecipe_ValidId_ReturnsRecipe()
    {
        var system = new CraftingSystem();
        var recipe = system.GetRecipe("recipe_iron_sword");
        Assert.NotNull(recipe);
        Assert.Equal("鉄の剣の鍛造", recipe.Name);
    }

    [Fact]
    public void CraftingSystem_GetRecipe_InvalidId_ReturnsNull()
    {
        var system = new CraftingSystem();
        Assert.Null(system.GetRecipe("nonexistent"));
    }

    #endregion

    #region CraftingSystem - Crafting

    [Fact]
    public void CraftingSystem_CanCraft_WithMaterials_ReturnsTrue()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 3);
        inventory.AddItem("material_wood", 1);

        Assert.True(system.CanCraft("recipe_iron_sword", player, inventory));
    }

    [Fact]
    public void CraftingSystem_CanCraft_WithoutMaterials_ReturnsFalse()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        var inventory = new CraftingInventory();

        Assert.False(system.CanCraft("recipe_iron_sword", player, inventory));
    }

    [Fact]
    public void CraftingSystem_CanCraft_InsufficientGold_ReturnsFalse()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.SpendGold(player.Gold); // ゴールド全消費
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 3);
        inventory.AddItem("material_wood", 1);

        Assert.False(system.CanCraft("recipe_iron_sword", player, inventory));
    }

    [Fact]
    public void CraftingSystem_Craft_Success_ReturnsResultItem()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 3);
        inventory.AddItem("material_wood", 1);

        var result = system.Craft("recipe_iron_sword", player, inventory);

        Assert.True(result.Success);
        Assert.NotNull(result.ResultItem);
        Assert.Equal("weapon_iron_sword", result.ResultItem!.ItemId);
    }

    [Fact]
    public void CraftingSystem_Craft_ConsumesMaterials()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 5);
        inventory.AddItem("material_wood", 2);

        system.Craft("recipe_iron_sword", player, inventory);

        Assert.Equal(2, inventory.CountItem("material_iron_ore")); // 5-3=2
        Assert.Equal(1, inventory.CountItem("material_wood")); // 2-1=1
    }

    [Fact]
    public void CraftingSystem_Craft_ConsumesGold()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        int goldBefore = player.Gold;
        player.AddGold(500);
        int goldAfterAdd = player.Gold;
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 3);
        inventory.AddItem("material_wood", 1);

        system.Craft("recipe_iron_sword", player, inventory);

        Assert.Equal(goldAfterAdd - 50, player.Gold); // recipe cost = 50
    }

    [Fact]
    public void CraftingSystem_Craft_InvalidRecipe_Fails()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        var inventory = new CraftingInventory();

        var result = system.Craft("nonexistent", player, inventory);
        Assert.False(result.Success);
    }

    #endregion

    #region CraftingSystem - Enhancement

    [Fact]
    public void CraftingSystem_EnhanceSuccessRate_Level0_Is100()
    {
        Assert.Equal(100, CraftingSystem.CalculateEnhanceSuccessRate(0));
    }

    [Fact]
    public void CraftingSystem_EnhanceSuccessRate_DecreasesWithLevel()
    {
        int prev = CraftingSystem.CalculateEnhanceSuccessRate(0);
        for (int i = 1; i <= 9; i++)
        {
            int current = CraftingSystem.CalculateEnhanceSuccessRate(i);
            Assert.True(current < prev, $"Level {i} rate ({current}) should be less than level {i - 1} rate ({prev})");
            prev = current;
        }
    }

    [Fact]
    public void CraftingSystem_Enhance_MaxLevel_Fails()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(1000);
        var weapon = ItemFactory.CreateIronSword();
        weapon.EnhancementLevel = 10;

        var result = system.EnhanceEquipment(weapon, player, new TestRandomProvider(0), 100);
        Assert.False(result.Success);
    }

    [Fact]
    public void CraftingSystem_Enhance_InsufficientGold_Fails()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.SpendGold(player.Gold);
        var weapon = ItemFactory.CreateIronSword();

        var result = system.EnhanceEquipment(weapon, player, new TestRandomProvider(0), 100);
        Assert.False(result.Success);
    }

    [Fact]
    public void CraftingSystem_Enhance_Level0_AlwaysSucceeds()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(1000);
        var weapon = ItemFactory.CreateIronSword();

        // Level0は100%成功率なのでどんな乱数でも成功
        var result = system.EnhanceEquipment(weapon, player, new TestRandomProvider(99), 100);
        Assert.True(result.Success);
        Assert.Equal(1, weapon.EnhancementLevel);
    }

    #endregion

    #region CraftingSystem - Enchantment

    [Fact]
    public void CraftingSystem_Enchant_NoElement_Fails()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var weapon = ItemFactory.CreateIronSword();

        var result = system.EnchantWeapon(weapon, Element.None, player, new TestRandomProvider(0), 200);
        Assert.False(result.Success);
    }

    [Fact]
    public void CraftingSystem_Enchant_InsufficientGold_Fails()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.SpendGold(player.Gold);
        var weapon = ItemFactory.CreateIronSword();

        var result = system.EnchantWeapon(weapon, Element.Fire, player, new TestRandomProvider(0), 200);
        Assert.False(result.Success);
    }

    [Fact]
    public void CraftingSystem_Enchant_Success_ReturnsElement()
    {
        var system = new CraftingSystem();
        var player = CreateTestPlayer();
        player.AddGold(500);
        var weapon = ItemFactory.CreateIronSword();

        // 乱数0で70%成功率なので成功
        var result = system.EnchantWeapon(weapon, Element.Fire, player, new TestRandomProvider(0), 200);
        Assert.True(result.Success);
        Assert.Equal(Element.Fire, result.Element);
    }

    #endregion

    #region CraftingSystem - CraftingInventory

    [Fact]
    public void CraftingInventory_AddAndCount()
    {
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 5);
        Assert.Equal(5, inventory.CountItem("material_iron_ore"));
    }

    [Fact]
    public void CraftingInventory_Remove_Success()
    {
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 5);
        Assert.True(inventory.RemoveItem("material_iron_ore", 3));
        Assert.Equal(2, inventory.CountItem("material_iron_ore"));
    }

    [Fact]
    public void CraftingInventory_Remove_InsufficientQuantity_Fails()
    {
        var inventory = new CraftingInventory();
        inventory.AddItem("material_iron_ore", 2);
        Assert.False(inventory.RemoveItem("material_iron_ore", 5));
    }

    [Fact]
    public void CraftingInventory_HasItem_Correct()
    {
        var inventory = new CraftingInventory();
        inventory.AddItem("material_herb", 3);
        Assert.True(inventory.HasItem("material_herb", 2));
        Assert.False(inventory.HasItem("material_herb", 5));
        Assert.False(inventory.HasItem("nonexistent"));
    }

    [Fact]
    public void CraftingInventory_RemoveAll_RemovesEntry()
    {
        var inventory = new CraftingInventory();
        inventory.AddItem("material_wood", 3);
        inventory.RemoveItem("material_wood", 3);
        Assert.Equal(0, inventory.CountItem("material_wood"));
    }

    #endregion

    #region RandomEventSystem - Enhanced

    [Fact]
    public void RandomEventSystem_GetAllEvents_Returns15Events()
    {
        var events = RandomEventSystem.GetAllEvents();
        Assert.Equal(15, events.Count);
    }

    [Fact]
    public void RandomEventSystem_GetAllEvents_ContainsNewTypes()
    {
        var events = RandomEventSystem.GetAllEvents();
        var types = events.Select(e => e.Type).ToHashSet();
        Assert.Contains(RandomEventType.MonsterHouse, types);
        Assert.Contains(RandomEventType.CursedRoom, types);
        Assert.Contains(RandomEventType.BlessedRoom, types);
        Assert.Contains(RandomEventType.HiddenShop, types);
        Assert.Contains(RandomEventType.MaterialDeposit, types);
    }

    [Theory]
    [InlineData(TerritoryId.Forest, 3)]
    [InlineData(TerritoryId.Mountain, 3)]
    [InlineData(TerritoryId.Coast, 3)]
    [InlineData(TerritoryId.Southern, 3)]
    [InlineData(TerritoryId.Frontier, 3)]
    public void RandomEventSystem_GetTerritoryEvents_ReturnsCorrectCount(TerritoryId territory, int expectedCount)
    {
        var events = RandomEventSystem.GetTerritoryEvents(territory);
        Assert.Equal(expectedCount, events.Count);
    }

    [Fact]
    public void RandomEventSystem_GetTerritoryEvents_Capital_ReturnsEmpty()
    {
        var events = RandomEventSystem.GetTerritoryEvents(TerritoryId.Capital);
        Assert.Empty(events);
    }

    [Fact]
    public void RandomEventSystem_RollTerritoryEvent_NullOnHighRoll()
    {
        var system = new RandomEventSystem();
        // 乱数99 >= 12なのでnull
        var result = system.RollTerritoryEvent(10, TerritoryId.Forest, new TestRandomProvider(99));
        Assert.Null(result);
    }

    [Fact]
    public void RandomEventSystem_RollTerritoryEvent_ReturnsEventOnLowRoll()
    {
        var system = new RandomEventSystem();
        // 乱数0 < 12なのでイベント発生、候補からindex 0を選択
        var result = system.RollTerritoryEvent(10, TerritoryId.Forest, new TestRandomProvider(0));
        Assert.NotNull(result);
    }

    [Fact]
    public void RandomEventSystem_ResolveMonsterHouse_ReturnsResult()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveMonsterHouse(5, new TestRandomProvider(2));
        Assert.False(result.IsPositive);
        Assert.True(result.GoldAmount > 0);
    }

    [Fact]
    public void RandomEventSystem_ResolveCursedRoom_ReturnsResult()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveCursedRoom(new TestRandomProvider(0));
        Assert.False(result.IsPositive);
        Assert.Equal(20, result.DamageAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveBlessedRoom_FullHeal()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveBlessedRoom(new TestRandomProvider(0));
        Assert.True(result.IsPositive);
        Assert.Equal(9999, result.HealAmount);
    }

    [Fact]
    public void RandomEventSystem_ResolveHiddenShop_IsPositive()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveHiddenShop();
        Assert.True(result.IsPositive);
    }

    [Fact]
    public void RandomEventSystem_ResolveMaterialDeposit_ReturnsTerritoryMaterial()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveMaterialDeposit(5, TerritoryId.Forest, new TestRandomProvider(1));
        Assert.True(result.IsPositive);
        Assert.Contains("薬草", result.Message);
    }

    [Fact]
    public void RandomEventSystem_ResolveMaterialDeposit_Mountain_ReturnsOre()
    {
        var system = new RandomEventSystem();
        var result = system.ResolveMaterialDeposit(5, TerritoryId.Mountain, new TestRandomProvider(1));
        Assert.Contains("鉱石", result.Message);
    }

    #endregion

    #region Integration

    [Fact]
    public void EnemyFactory_CanCreateAllTerritoryEnemies()
    {
        var factory = new EnemyFactory();
        foreach (TerritoryId territory in Enum.GetValues<TerritoryId>())
        {
            var enemies = EnemyDefinitions.GetEnemiesForTerritory(territory);
            foreach (var def in enemies)
            {
                var enemy = factory.CreateEnemy(def, new Position(1, 1));
                Assert.NotNull(enemy);
                Assert.True(enemy.IsAlive);
            }
        }
    }

    [Fact]
    public void EnemyFactory_CanCreateAllBosses()
    {
        var factory = new EnemyFactory();
        foreach (var boss in EnemyDefinitions.GetAllBosses())
        {
            var enemy = factory.CreateEnemy(boss, new Position(5, 5));
            Assert.NotNull(enemy);
            Assert.True(enemy.CurrentHp > 0);
        }
    }

    [Fact]
    public void AllItemIds_UniqueAndNonEmpty()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        Assert.All(ids, id => Assert.False(string.IsNullOrEmpty(id)));
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    #endregion

    /// <summary>テスト用乱数プロバイダ</summary>
    private class TestRandomProvider : IRandomProvider
    {
        private readonly int _value;
        public TestRandomProvider(int value) => _value = value;
        public int Next(int maxValue) => Math.Min(_value, maxValue > 0 ? maxValue - 1 : 0);
        public int Next(int minValue, int maxValue) => Math.Clamp(_value, minValue, maxValue - 1);
        public double NextDouble() => _value / 100.0;
    }
}
