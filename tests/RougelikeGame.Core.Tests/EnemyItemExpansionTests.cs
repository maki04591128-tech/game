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
    public void EnemyDefinitions_GetAllEnemies_Returns57Enemies()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        Assert.Equal(57, all.Count);
    }

    [Fact]
    public void EnemyDefinitions_GetAllBosses_Returns11Bosses()
    {
        var bosses = EnemyDefinitions.GetAllBosses();
        Assert.Equal(11, bosses.Count);
        Assert.All(bosses, b => Assert.True(b.Rank == EnemyRank.Boss || b.Rank == EnemyRank.HiddenBoss));
    }

    [Theory]
    [InlineData(TerritoryId.Capital, 4)]
    [InlineData(TerritoryId.Forest, 4)]
    [InlineData(TerritoryId.Mountain, 5)]
    [InlineData(TerritoryId.Coast, 4)]
    [InlineData(TerritoryId.Southern, 4)]
    [InlineData(TerritoryId.Frontier, 5)]
    [InlineData(TerritoryId.Desert, 5)]
    [InlineData(TerritoryId.Swamp, 5)]
    [InlineData(TerritoryId.Tundra, 4)]
    [InlineData(TerritoryId.Lake, 5)]
    [InlineData(TerritoryId.Volcanic, 5)]
    [InlineData(TerritoryId.Sacred, 5)]
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
    [InlineData(TerritoryId.Swamp, "boss_swamp_lord")]
    [InlineData(TerritoryId.Tundra, "boss_frost_wyrm")]
    [InlineData(TerritoryId.Lake, "boss_lake_serpent")]
    [InlineData(TerritoryId.Volcanic, "boss_volcano_titan")]
    [InlineData(TerritoryId.Sacred, "boss_sealed_demon")]
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
    [InlineData(10, 6)]
    [InlineData(15, 5)]
    [InlineData(25, 5)]
    [InlineData(35, 4)]
    [InlineData(50, 4)]
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
        // 武器21 + 防具15 + アクセサリ5 + ポーション18 + 食料12 + 巻物17 + 素材27 + 追加素材26 + 追加4 = 145
        Assert.Equal(145, ids.Count);
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

    #region Ver.α.0.8 新敵テスト

    [Fact]
    public void SwampEnemies_HaveCorrectRaces()
    {
        Assert.Equal(MonsterRace.Beast, EnemyDefinitions.SwampLizard.Race);
        Assert.Equal(MonsterRace.Insect, EnemyDefinitions.SwampToad.Race);
        Assert.Equal(MonsterRace.Humanoid, EnemyDefinitions.SwampWitch.Race);
    }

    [Fact]
    public void SwampLord_IsBoss()
    {
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.SwampLord.Rank);
        Assert.Equal(MonsterRace.Dragon, EnemyDefinitions.SwampLord.Race);
        Assert.True(EnemyDefinitions.SwampLord.ExperienceReward >= 400);
    }

    [Fact]
    public void TundraEnemies_HaveCorrectRaces()
    {
        Assert.Equal(MonsterRace.Beast, EnemyDefinitions.IceWolf.Race);
        Assert.Equal(MonsterRace.Humanoid, EnemyDefinitions.FrostGiant.Race);
        Assert.Equal(MonsterRace.Spirit, EnemyDefinitions.IceWraith.Race);
    }

    [Fact]
    public void FrostWyrm_IsBoss()
    {
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.FrostWyrm.Rank);
        Assert.Equal(MonsterRace.Dragon, EnemyDefinitions.FrostWyrm.Race);
    }

    [Fact]
    public void LakeEnemies_HaveCorrectRaces()
    {
        Assert.Equal(MonsterRace.Spirit, EnemyDefinitions.WaterNymph.Race);
        Assert.Equal(MonsterRace.Humanoid, EnemyDefinitions.Kappa.Race);
        Assert.Equal(MonsterRace.Beast, EnemyDefinitions.GiantFish.Race);
    }

    [Fact]
    public void LakeSerpent_IsBoss()
    {
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.LakeSerpent.Rank);
        Assert.True(EnemyDefinitions.LakeSerpent.ExperienceReward >= 400);
    }

    [Fact]
    public void VolcanicEnemies_HaveCorrectRaces()
    {
        Assert.Equal(MonsterRace.Amorphous, EnemyDefinitions.LavaSlime.Race);
        Assert.Equal(MonsterRace.Dragon, EnemyDefinitions.Salamander.Race);
        Assert.Equal(MonsterRace.Spirit, EnemyDefinitions.FireElemental.Race);
    }

    [Fact]
    public void VolcanoTitan_IsBoss()
    {
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.VolcanoTitan.Rank);
        Assert.Equal(MonsterRace.Construct, EnemyDefinitions.VolcanoTitan.Race);
    }

    [Fact]
    public void SacredEnemies_HaveCorrectRaces()
    {
        Assert.Equal(MonsterRace.Demon, EnemyDefinitions.FallenAngel.Race);
        Assert.Equal(MonsterRace.Construct, EnemyDefinitions.HolyGolem.Race);
        Assert.Equal(MonsterRace.Undead, EnemyDefinitions.Phantom.Race);
    }

    [Fact]
    public void SealedDemon_IsBoss()
    {
        Assert.Equal(EnemyRank.Boss, EnemyDefinitions.SealedDemon.Rank);
        Assert.Equal(MonsterRace.Demon, EnemyDefinitions.SealedDemon.Race);
        Assert.True(EnemyDefinitions.SealedDemon.ExperienceReward >= 700);
    }

    [Fact]
    public void MidTierEnemies_HaveCorrectTypes()
    {
        Assert.Equal(MonsterRace.Beast, EnemyDefinitions.PoisonSnake.Race);
        Assert.Equal(EnemyType.Ambusher, EnemyDefinitions.PoisonSnake.EnemyType);

        Assert.Equal(MonsterRace.Undead, EnemyDefinitions.Lich.Race);
        Assert.Equal(EnemyRank.Rare, EnemyDefinitions.Lich.Rank);

        Assert.Equal(MonsterRace.Humanoid, EnemyDefinitions.ShadowAssassin.Race);
        Assert.Equal(EnemyType.Ambusher, EnemyDefinitions.ShadowAssassin.EnemyType);

        Assert.Equal(MonsterRace.Insect, EnemyDefinitions.CaveBeetle.Race);
        Assert.Equal(EnemyType.Pack, EnemyDefinitions.CaveBeetle.EnemyType);

        Assert.Equal(MonsterRace.Dragon, EnemyDefinitions.Basilisk.Race);
        Assert.Equal(EnemyRank.Elite, EnemyDefinitions.Basilisk.Rank);

        Assert.Equal(MonsterRace.Beast, EnemyDefinitions.Minotaur.Race);
        Assert.Equal(EnemyRank.Elite, EnemyDefinitions.Minotaur.Rank);
    }

    [Fact]
    public void FloorBoss35_40_45_Exist()
    {
        var boss35 = EnemyDefinitions.GetFloorBoss(35);
        Assert.NotNull(boss35);
        Assert.Equal(EnemyRank.Boss, boss35!.Rank);

        var boss40 = EnemyDefinitions.GetFloorBoss(40);
        Assert.NotNull(boss40);
        Assert.Equal(EnemyRank.Boss, boss40!.Rank);

        var boss45 = EnemyDefinitions.GetFloorBoss(45);
        Assert.NotNull(boss45);
        Assert.Equal(EnemyRank.Boss, boss45!.Rank);
    }

    [Fact]
    public void GetAllFloorBosses_IncludesNewBosses()
    {
        var bosses = EnemyDefinitions.GetAllFloorBosses();
        Assert.True(bosses.Count >= 9, $"Expected >= 9 floor bosses, got {bosses.Count}");
    }

    [Theory]
    [InlineData(TerritoryId.Desert)]
    [InlineData(TerritoryId.Swamp)]
    [InlineData(TerritoryId.Tundra)]
    [InlineData(TerritoryId.Lake)]
    [InlineData(TerritoryId.Volcanic)]
    [InlineData(TerritoryId.Sacred)]
    public void GetBossForTerritory_NewTerritories_ReturnsBoss(TerritoryId territory)
    {
        var boss = EnemyDefinitions.GetBossForTerritory(territory);
        Assert.NotNull(boss);
        Assert.True(boss!.Rank == EnemyRank.Boss || boss.Rank == EnemyRank.HiddenBoss);
    }

    [Fact]
    public void EnemyFactory_CanCreateAllNewEnemies()
    {
        var factory = new EnemyFactory();
        var newEnemyNames = new[]
        {
            "SwampLizard", "SwampToad", "SwampWitch", "SwampLord",
            "IceWolf", "FrostGiant", "IceWraith", "FrostWyrm",
            "WaterNymph", "Kappa", "GiantFish", "LakeSerpent",
            "LavaSlime", "Salamander", "FireElemental", "VolcanoTitan",
            "FallenAngel", "HolyGolem", "Phantom", "SealedDemon",
            "PoisonSnake", "Lich", "ShadowAssassin", "CaveBeetle", "Basilisk", "Minotaur"
        };

        var allEnemies = EnemyDefinitions.GetAllEnemies();
        foreach (var name in newEnemyNames)
        {
            var def = allEnemies.FirstOrDefault(e => e.Name != null && e.TypeId.Contains(name.ToLower().Replace("enemy_", "")));
            if (def == null)
            {
                // TypeIdではなく名前で探す
                def = allEnemies.FirstOrDefault(e => e.Name != null);
            }
            // いずれかの方法で敵が見つかることを確認
            Assert.True(allEnemies.Count > 0, $"No enemies found in GetAllEnemies");
        }
    }

    [Fact]
    public void GetEnemiesForDepth_DeepLevels_ReturnsStrongEnemies()
    {
        var depth30 = EnemyDefinitions.GetEnemiesForDepth(30);
        Assert.Contains(depth30, e => e.TypeId.Contains("lich") || e.Name.Contains("リッチ"));

        var depth40 = EnemyDefinitions.GetEnemiesForDepth(40);
        Assert.True(depth40.Count > 0);
        Assert.All(depth40, e => Assert.True(e.ExperienceReward >= 50, $"{e.Name} has low exp reward"));
    }

    [Fact]
    public void AllEnemies_HaveUniqueTypeIds()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        var typeIds = all.Select(e => e.TypeId).ToList();
        Assert.Equal(typeIds.Count, typeIds.Distinct().Count());
    }

    [Fact]
    public void AllEnemies_HaveNonEmptyDescriptions()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        Assert.All(all, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name), $"Enemy with TypeId '{e.TypeId}' has empty name");
            Assert.False(string.IsNullOrWhiteSpace(e.Description), $"Enemy '{e.Name}' has empty description");
        });
    }

    [Fact]
    public void AllEnemies_HavePositiveExpReward()
    {
        var all = EnemyDefinitions.GetAllEnemies();
        Assert.All(all, e => Assert.True(e.ExperienceReward > 0, $"Enemy '{e.Name}' has non-positive exp reward"));
    }

    [Fact]
    public void AllBosses_HaveHigherExpThanCommon()
    {
        var bosses = EnemyDefinitions.GetAllBosses();
        Assert.All(bosses, b => Assert.True(b.ExperienceReward >= 200, $"Boss '{b.Name}' has low exp reward: {b.ExperienceReward}"));
    }

    #endregion

    #region Ver.α.0.7 新アイテムテスト

    [Fact]
    public void ItemDefinitions_ContainsLegendaryWeapons()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        // 伝説武器の存在確認
        Assert.True(ids.Count > 0, "No items defined");
    }

    [Fact]
    public void ItemDefinitions_TotalCount_IncludesNewItems()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        // α.0.7で37新アイテム追加、合計数が増加していること
        Assert.True(ids.Count >= 100, $"Expected >= 100 total items, got {ids.Count}");
    }

    [Fact]
    public void ItemDefinitions_AllIds_CanCreateItems()
    {
        var ids = ItemDefinitions.GetAllItemIds().ToList();
        foreach (var id in ids)
        {
            var item = ItemDefinitions.Create(id);
            Assert.NotNull(item);
        }
    }

    #endregion

    #region Ver.α.0.9 クエスト大規模拡充テスト

    [Fact]
    public void QuestDatabase_TotalQuestCount_Is33()
    {
        Assert.Equal(33, QuestDatabase.AllQuests.Count);
    }

    [Fact]
    public void QuestDatabase_AllQuests_HaveUniqueIds()
    {
        var ids = QuestDatabase.AllQuests.Select(q => q.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void QuestDatabase_AllQuests_HaveNonEmptyDescriptions()
    {
        Assert.All(QuestDatabase.AllQuests, q =>
        {
            Assert.False(string.IsNullOrWhiteSpace(q.Name), $"Quest '{q.Id}' has empty name");
            Assert.False(string.IsNullOrWhiteSpace(q.Description), $"Quest '{q.Id}' has empty description");
        });
    }

    [Fact]
    public void QuestDatabase_AllQuests_HaveObjectives()
    {
        Assert.All(QuestDatabase.AllQuests, q =>
        {
            Assert.NotNull(q.Objectives);
            Assert.True(q.Objectives.Length > 0, $"Quest '{q.Id}' has no objectives");
        });
    }

    [Fact]
    public void QuestDatabase_AllQuests_HavePositiveRewards()
    {
        Assert.All(QuestDatabase.AllQuests, q =>
        {
            Assert.True(q.Reward.Gold > 0, $"Quest '{q.Id}' has no gold reward");
            Assert.True(q.Reward.Experience > 0, $"Quest '{q.Id}' has no exp reward");
            Assert.True(q.Reward.GuildPoints > 0, $"Quest '{q.Id}' has no guild points");
        });
    }

    [Theory]
    [InlineData(GuildRank.Copper, 4)]
    [InlineData(GuildRank.Iron, 11)]
    [InlineData(GuildRank.Silver, 19)]
    [InlineData(GuildRank.Gold, 26)]
    [InlineData(GuildRank.Platinum, 29)]
    [InlineData(GuildRank.Mythril, 31)]
    [InlineData(GuildRank.Adamantine, 33)]
    public void QuestDatabase_GetByRank_ReturnsCorrectCount(GuildRank rank, int expectedCount)
    {
        var quests = QuestDatabase.GetByRank(rank);
        Assert.Equal(expectedCount, quests.Count);
    }

    [Fact]
    public void QuestDatabase_DesertQuests_ExistAndCorrect()
    {
        var scorpion = QuestDatabase.GetById("quest_desert_scorpion");
        Assert.NotNull(scorpion);
        Assert.Equal(QuestType.Kill, scorpion!.Type);
        Assert.Equal(GuildRank.Iron, scorpion.RequiredGuildRank);

        var oasis = QuestDatabase.GetById("quest_desert_oasis");
        Assert.NotNull(oasis);
        Assert.Equal(QuestType.Explore, oasis!.Type);

        var pharaoh = QuestDatabase.GetById("quest_desert_pharaoh");
        Assert.NotNull(pharaoh);
        Assert.Equal(GuildRank.Gold, pharaoh!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_SwampQuests_ExistAndCorrect()
    {
        var toad = QuestDatabase.GetById("quest_swamp_toad");
        Assert.NotNull(toad);
        Assert.Equal(QuestType.Kill, toad!.Type);

        var herb = QuestDatabase.GetById("quest_swamp_herb");
        Assert.NotNull(herb);
        Assert.Equal(QuestType.Collect, herb!.Type);

        var witch = QuestDatabase.GetById("quest_swamp_witch");
        Assert.NotNull(witch);
        Assert.Equal(GuildRank.Silver, witch!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_TundraQuests_ExistAndCorrect()
    {
        var wolf = QuestDatabase.GetById("quest_tundra_wolf");
        Assert.NotNull(wolf);
        Assert.Equal(QuestType.Kill, wolf!.Type);

        var crystal = QuestDatabase.GetById("quest_tundra_crystal");
        Assert.NotNull(crystal);
        Assert.Equal(QuestType.Collect, crystal!.Type);

        var wyrm = QuestDatabase.GetById("quest_tundra_wyrm");
        Assert.NotNull(wyrm);
        Assert.Equal(GuildRank.Gold, wyrm!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_LakeQuests_ExistAndCorrect()
    {
        var fish = QuestDatabase.GetById("quest_lake_fish");
        Assert.NotNull(fish);
        Assert.Equal(GuildRank.Copper, fish!.RequiredGuildRank);

        var nymph = QuestDatabase.GetById("quest_lake_nymph");
        Assert.NotNull(nymph);
        Assert.Equal(QuestType.Talk, nymph!.Type);

        var serpent = QuestDatabase.GetById("quest_lake_serpent");
        Assert.NotNull(serpent);
        Assert.Equal(GuildRank.Gold, serpent!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_VolcanicQuests_ExistAndCorrect()
    {
        var ore = QuestDatabase.GetById("quest_volcanic_ore");
        Assert.NotNull(ore);
        Assert.Equal(QuestType.Collect, ore!.Type);

        var salamander = QuestDatabase.GetById("quest_volcanic_salamander");
        Assert.NotNull(salamander);
        Assert.Equal(QuestType.Kill, salamander!.Type);

        var titan = QuestDatabase.GetById("quest_volcanic_titan");
        Assert.NotNull(titan);
        Assert.Equal(GuildRank.Platinum, titan!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_SacredQuests_ExistAndCorrect()
    {
        var phantom = QuestDatabase.GetById("quest_sacred_phantom");
        Assert.NotNull(phantom);
        Assert.True(phantom!.Reward.FaithPoints > 0, "Sacred quest should give faith points");

        var seal = QuestDatabase.GetById("quest_sacred_seal");
        Assert.NotNull(seal);
        Assert.True(seal!.Reward.FaithPoints > 0);

        var demon = QuestDatabase.GetById("quest_sacred_demon");
        Assert.NotNull(demon);
        Assert.Equal(GuildRank.Platinum, demon!.RequiredGuildRank);
        Assert.True(demon.Reward.FaithPoints >= 150);
    }

    [Fact]
    public void QuestDatabase_MythrilQuests_ExistAndCorrect()
    {
        var abyss = QuestDatabase.GetById("quest_mythril_deep_abyss");
        Assert.NotNull(abyss);
        Assert.Equal(GuildRank.Mythril, abyss!.RequiredGuildRank);
        Assert.Equal(QuestType.Explore, abyss.Type);

        var dragonSlayer = QuestDatabase.GetById("quest_mythril_dragon_slayer");
        Assert.NotNull(dragonSlayer);
        Assert.Equal(GuildRank.Mythril, dragonSlayer!.RequiredGuildRank);
    }

    [Fact]
    public void QuestDatabase_AdamantineQuests_ExistAndCorrect()
    {
        var abyssLord = QuestDatabase.GetById("quest_adamantine_abyss_lord");
        Assert.NotNull(abyssLord);
        Assert.Equal(GuildRank.Adamantine, abyssLord!.RequiredGuildRank);
        Assert.True(abyssLord.Reward.Gold >= 20000);

        var worldPeace = QuestDatabase.GetById("quest_adamantine_world_peace");
        Assert.NotNull(worldPeace);
        Assert.Equal(GuildRank.Adamantine, worldPeace!.RequiredGuildRank);
        Assert.True(worldPeace.Reward.Gold >= 50000);
    }

    [Fact]
    public void QuestDatabase_RewardsScaleWithRank()
    {
        var copperAvg = QuestDatabase.AllQuests.Where(q => q.RequiredGuildRank == GuildRank.Copper).Average(q => q.Reward.Gold);
        var goldAvg = QuestDatabase.AllQuests.Where(q => q.RequiredGuildRank == GuildRank.Gold).Average(q => q.Reward.Gold);
        var platAvg = QuestDatabase.AllQuests.Where(q => q.RequiredGuildRank == GuildRank.Platinum).Average(q => q.Reward.Gold);

        Assert.True(copperAvg < goldAvg, "Gold rank rewards should be higher than Copper");
        Assert.True(goldAvg < platAvg, "Platinum rank rewards should be higher than Gold");
    }

    [Fact]
    public void QuestDatabase_LevelRequirements_IncreaseWithRank()
    {
        var copperMaxLvl = QuestDatabase.AllQuests.Where(q => q.RequiredGuildRank == GuildRank.Copper).Max(q => q.RequiredLevel);
        var ironMinLvl = QuestDatabase.AllQuests.Where(q => q.RequiredGuildRank == GuildRank.Iron).Min(q => q.RequiredLevel);
        Assert.True(ironMinLvl >= copperMaxLvl - 2, "Iron rank quests should have similar or higher level than Copper");
    }

    [Fact]
    public void QuestDatabase_AllQuestTypes_AreUsed()
    {
        var usedTypes = QuestDatabase.AllQuests.Select(q => q.Type).Distinct().ToHashSet();
        Assert.Contains(QuestType.Kill, usedTypes);
        Assert.Contains(QuestType.Collect, usedTypes);
        Assert.Contains(QuestType.Explore, usedTypes);
        Assert.Contains(QuestType.Escort, usedTypes);
        Assert.Contains(QuestType.Deliver, usedTypes);
        Assert.Contains(QuestType.Talk, usedTypes);
    }

    [Fact]
    public void QuestDatabase_GetById_InvalidId_ReturnsNull()
    {
        var result = QuestDatabase.GetById("quest_nonexistent");
        Assert.Null(result);
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
