using Xunit;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Tests;

public class ItemSystemTests
{
    #region Item Basic Tests

    [Fact]
    public void Item_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var weapon = ItemFactory.CreateRustySword();

        // Assert
        Assert.Equal("weapon_rusty_sword", weapon.ItemId);
        Assert.Equal("錆びた剣", weapon.Name);
        Assert.Equal(Items.ItemType.Equipment, weapon.Type);
        Assert.Equal(ItemRarity.Common, weapon.Rarity);
        Assert.True(weapon.IsIdentified); // デフォルトは識別済み
    }

    [Fact]
    public void Item_IdentifyRevealsProperties()
    {
        // Arrange
        var weapon = ItemFactory.CreateRustySword();
        weapon.IsIdentified = false;

        // Act
        weapon.IsIdentified = true;

        // Assert
        Assert.True(weapon.IsIdentified);
    }

    #endregion

    #region Weapon Tests

    [Fact]
    public void Weapon_CreateRustySword_HasCorrectStats()
    {
        // Arrange & Act
        var sword = ItemFactory.CreateRustySword();

        // Assert
        Assert.Equal(WeaponType.Sword, sword.WeaponType);
        Assert.Equal(5, sword.BaseDamage);
        Assert.Equal((3, 7), sword.DamageRange);
        Assert.Equal(1, sword.Range);
        Assert.False(sword.IsTwoHanded);
    }

    [Fact]
    public void Weapon_CreateBattleAxe_IsTwoHanded()
    {
        // Arrange & Act
        var axe = ItemFactory.CreateBattleAxe();

        // Assert
        Assert.Equal(WeaponType.Axe, axe.WeaponType);
        Assert.True(axe.IsTwoHanded);
    }

    [Fact]
    public void Weapon_CalculateDamage_ReturnsValueInRange()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword();
        var random = new Random(42);

        // Act
        var damage = sword.CalculateDamage(random);

        // Assert
        Assert.InRange(damage, sword.DamageRange.Min, sword.DamageRange.Max + 10); // 強化ボーナス考慮
    }

    [Fact]
    public void Weapon_Enhancement_IncreasesDamage()
    {
        // Arrange
        var sword = ItemFactory.CreateIronSword();
        var random = new Random(42);
        var baseDamage = sword.CalculateDamage(random);

        // Act
        sword.EnhancementLevel = 3;
        random = new Random(42); // リセット
        var enhancedDamage = sword.CalculateDamage(random);

        // Assert
        Assert.True(enhancedDamage > baseDamage);
    }

    #endregion

    #region Armor Tests

    [Fact]
    public void Armor_CreateLeatherArmor_HasCorrectStats()
    {
        // Arrange & Act
        var armor = ItemFactory.CreateLeatherArmor();

        // Assert
        Assert.Equal(ArmorType.Leather, armor.ArmorType);
        Assert.Equal(Items.EquipmentSlot.Body, armor.Slot);
        Assert.Equal(5, armor.BaseDefense);
    }

    [Fact]
    public void Armor_CreatePlateArmor_HasHighDefense()
    {
        // Arrange & Act
        var armor = ItemFactory.CreatePlateArmor();

        // Assert
        Assert.Equal(ArmorType.Plate, armor.ArmorType);
        Assert.True(armor.BaseDefense > 10);
    }

    [Fact]
    public void Armor_CalculateDefense_IncludesEnhancement()
    {
        // Arrange
        var armor = ItemFactory.CreateLeatherArmor();
        var baseDefense = armor.CalculateDefense();

        // Act
        armor.EnhancementLevel = 3;
        var enhancedDefense = armor.CalculateDefense();

        // Assert
        Assert.Equal(baseDefense + 3, enhancedDefense);
    }

    #endregion

    #region Potion Tests

    [Fact]
    public void Potion_CreateMinorHealingPotion_HasCorrectEffect()
    {
        // Arrange & Act
        var potion = ItemFactory.CreateMinorHealingPotion();

        // Assert
        Assert.Equal(PotionType.HealingMinor, potion.PotionType);
        Assert.Equal(30, potion.EffectValue);
        Assert.True(potion.ConsumeOnUse);
    }

    [Fact]
    public void Potion_Use_HealsCharacter()
    {
        // Arrange
        var potion = ItemFactory.CreateMinorHealingPotion();
        var player = CreateTestPlayer();
        player.TakeDamage(Damage.Pure(50));
        var hpBefore = player.CurrentHp;

        // Act
        var result = potion.Use(player);

        // Assert
        Assert.True(result.Success);
        Assert.True(player.CurrentHp > hpBefore);
    }

    [Fact]
    public void Potion_IsStackable()
    {
        // Arrange
        var potion1 = ItemFactory.CreateMinorHealingPotion();
        var potion2 = ItemFactory.CreateMinorHealingPotion();

        // Assert
        Assert.IsAssignableFrom<IStackable>(potion1);
        Assert.Equal(potion1.ItemId, potion2.ItemId);
    }

    #endregion

    #region Food Tests

    [Fact]
    public void Food_CreateBread_HasNutritionValue()
    {
        // Arrange & Act
        var bread = ItemFactory.CreateBread();

        // Assert
        Assert.Equal(FoodType.Bread, bread.FoodType);
        Assert.True(bread.NutritionValue > 0);
    }

    [Fact]
    public void Food_Use_RestoresHunger()
    {
        // Arrange
        var bread = ItemFactory.CreateBread();
        var player = CreateTestPlayer();
        player.ModifyHunger(-50); // 満腹度を減少させる
        var hungerBefore = player.Hunger;

        // Act
        var result = bread.Use(player);

        // Assert
        Assert.True(result.Success);
        Assert.True(player.Hunger > hungerBefore, "食事後は満腹度が回復するべき");
    }

    #endregion

    #region Scroll Tests

    [Fact]
    public void Scroll_CreateScrollOfIdentify_HasCorrectType()
    {
        // Arrange & Act
        var scroll = ItemFactory.CreateScrollOfIdentify();

        // Assert
        Assert.Equal(ScrollType.Identify, scroll.ScrollType);
        Assert.True(scroll.ConsumeOnUse);
    }

    [Fact]
    public void Scroll_Use_ReturnsEffect()
    {
        // Arrange
        var scroll = ItemFactory.CreateScrollOfIdentify();
        var player = CreateTestPlayer();

        // Act
        var result = scroll.Use(player);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Effect);
    }

    #endregion

    #region Inventory Tests

    [Fact]
    public void Inventory_Add_AddsItemSuccessfully()
    {
        // Arrange
        var inventory = new Inventory(10);
        var sword = ItemFactory.CreateRustySword();

        // Act
        var result = inventory.Add(sword);

        // Assert
        Assert.True(result);
        Assert.Equal(1, inventory.UsedSlots);
        Assert.Contains(sword, inventory.Items);
    }

    [Fact]
    public void Inventory_Add_RespectsMaxSlots()
    {
        // Arrange
        var inventory = new Inventory(2);
        var sword1 = ItemFactory.CreateRustySword();
        var sword2 = ItemFactory.CreateIronSword();
        var sword3 = ItemFactory.CreateSteelSword();

        // Act
        inventory.Add(sword1);
        inventory.Add(sword2);
        var result = inventory.Add(sword3);

        // Assert
        Assert.False(result);
        Assert.Equal(2, inventory.UsedSlots);
    }

    [Fact]
    public void Inventory_Remove_RemovesItemSuccessfully()
    {
        // Arrange
        var inventory = new Inventory(10);
        var sword = ItemFactory.CreateRustySword();
        inventory.Add(sword);

        // Act
        var result = inventory.Remove(sword);

        // Assert
        Assert.True(result);
        Assert.Equal(0, inventory.UsedSlots);
    }

    [Fact]
    public void Inventory_GetEquippableItems_ReturnsOnlyEquippables()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.Add(ItemFactory.CreateRustySword());
        inventory.Add(ItemFactory.CreateMinorHealingPotion());
        inventory.Add(ItemFactory.CreateLeatherArmor());

        // Act
        var equippables = inventory.GetEquippableItems().ToList();

        // Assert
        Assert.Equal(2, equippables.Count);
    }

    [Fact]
    public void Inventory_GetConsumableItems_ReturnsOnlyConsumables()
    {
        // Arrange
        var inventory = new Inventory(10);
        inventory.Add(ItemFactory.CreateRustySword());
        inventory.Add(ItemFactory.CreateMinorHealingPotion());
        inventory.Add(ItemFactory.CreateBread());

        // Act
        var consumables = inventory.GetConsumableItems().ToList();

        // Assert
        Assert.Equal(2, consumables.Count);
    }

    [Fact]
    public void Inventory_UseItem_ConsumesItem()
    {
        // Arrange
        var inventory = new Inventory(10);
        var potion = ItemFactory.CreateMinorHealingPotion();
        inventory.Add(potion);
        var player = CreateTestPlayer();
        player.TakeDamage(Damage.Pure(30));

        // Act
        var result = inventory.UseItem(potion, player);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(0, inventory.UsedSlots); // ポーションは消費される
    }

    #endregion

    #region ItemFactory Tests

    [Fact]
    public void ItemFactory_GenerateRandomItem_ReturnsValidItem()
    {
        // Arrange
        var factory = new ItemFactory(42);

        // Act
        var item = factory.GenerateRandomItem(5);

        // Assert
        Assert.NotNull(item);
    }

    [Fact]
    public void ItemFactory_GenerateRandomItem_HigherFloorHasBetterItems()
    {
        // Arrange
        var factory = new ItemFactory(42);
        var lowFloorItems = Enumerable.Range(0, 100).Select(_ => factory.GenerateRandomItem(1)).ToList();

        factory = new ItemFactory(42);
        var highFloorItems = Enumerable.Range(0, 100).Select(_ => factory.GenerateRandomItem(20)).ToList();

        // Act
        var lowFloorRareCount = lowFloorItems.Count(i => i.Rarity >= ItemRarity.Rare);
        var highFloorRareCount = highFloorItems.Count(i => i.Rarity >= ItemRarity.Rare);

        // Assert
        Assert.True(highFloorRareCount >= lowFloorRareCount);
    }

    #endregion

    #region Equipment Management Tests

    [Fact]
    public void Equipment_Equip_SetsItemInSlot()
    {
        // Arrange
        var equipment = new Equipment();
        var sword = ItemFactory.CreateIronSword();
        var player = CreateTestPlayer();

        // Act
        var result = equipment.Equip(sword, player);

        // Assert
        Assert.Null(result); // 以前の装備なし
        Assert.Equal(sword, equipment.MainHand);
    }

    [Fact]
    public void Equipment_Equip_ReturnsOldItem()
    {
        // Arrange
        var equipment = new Equipment();
        var sword1 = ItemFactory.CreateRustySword();
        var sword2 = ItemFactory.CreateIronSword();
        var player = CreateTestPlayer();
        equipment.Equip(sword1, player);

        // Act
        var oldItem = equipment.Equip(sword2, player);

        // Assert
        Assert.Equal(sword1, oldItem);
        Assert.Equal(sword2, equipment.MainHand);
    }

    [Fact]
    public void Equipment_Unequip_RemovesItem()
    {
        // Arrange
        var equipment = new Equipment();
        var sword = ItemFactory.CreateIronSword();
        var player = CreateTestPlayer();
        equipment.Equip(sword, player);

        // Act
        var unequipped = equipment.Unequip(Items.EquipmentSlot.MainHand, player);

        // Assert
        Assert.Equal(sword, unequipped);
        Assert.Null(equipment.MainHand);
    }

    [Fact]
    public void Equipment_TwoHandedWeapon_OccupiesBothHands()
    {
        // Arrange
        var equipment = new Equipment();
        // BattleAxeはRequiredLevel=3なので、IsTwoHandedなWeaponを直接作成
        var axe = new Weapon
        {
            ItemId = "test_two_handed",
            Name = "テスト両手武器",
            WeaponType = WeaponType.Axe,
            IsTwoHanded = true,
            RequiredLevel = 1
        };
        var player = CreateTestPlayer();

        // Act
        equipment.Equip(axe, player);

        // Assert
        Assert.Equal(axe, equipment.MainHand);
        Assert.Null(equipment.OffHand); // 副手は空（両手武器）
    }

    [Fact]
    public void Equipment_GetStatModifiers_CombinesAllEquipment()
    {
        // Arrange
        var equipment = new Equipment();
        var player = CreateTestPlayer();
        equipment.Equip(ItemFactory.CreateIronSword(), player);
        equipment.Equip(ItemFactory.CreateLeatherArmor(), player);

        // Act
        var modifiers = equipment.GetStatModifiers().ToList();

        // Assert
        Assert.True(modifiers.Count >= 2);
    }

    [Fact]
    public void Equipment_GetTotalPhysicalDefense_SumsArmorDefense()
    {
        // Arrange
        var equipment = new Equipment();
        var player = CreateTestPlayer();
        equipment.Equip(ItemFactory.CreateLeatherArmor(), player);

        // Act
        var defense = equipment.GetTotalPhysicalDefense();

        // Assert
        Assert.True(defense > 0);
    }

    #endregion

    #region Helper Methods

    private static Player CreateTestPlayer()
    {
        var player = new Player { Name = "Test Player" };
        player.InitializeResources();
        return player;
    }

    #endregion

    #region Weapon - Guard tests

    [Fact]
    public void Weapon_GetAttackTurnCost_ZeroSpeed_ReturnsOne()
    {
        // AttackSpeed=0でもゼロ除算せずに1を返す
        var weapon = new Weapon { AttackSpeed = 0f };
        Assert.Equal(1, weapon.GetAttackTurnCost());
    }

    [Fact]
    public void Weapon_CalculateDamage_InvertedRange_DoesNotThrow()
    {
        // Min > Max でもArgumentOutOfRangeExceptionにならない
        var weapon = new Weapon { DamageRange = (10, 5) };
        var random = new Random(42);
        int damage = weapon.CalculateDamage(random);
        Assert.True(damage >= 5); // min=5, max=10として扱われる
    }

    #endregion
}
