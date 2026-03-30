using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Tests;

public class ItemUnitTests
{
    #region Item base properties

    [Fact]
    public void Item_DisplayChar_ReturnsCorrectCharForEquipment()
    {
        var weapon = new Weapon { WeaponType = WeaponType.Sword };
        Assert.Equal('/', weapon.DisplayChar);
    }

    [Fact]
    public void Item_DisplayChar_ReturnsCorrectCharForFood()
    {
        var food = new Food { FoodType = FoodType.Bread };
        Assert.Equal('%', food.DisplayChar);
    }

    [Fact]
    public void Item_Rarity_DefaultsToCommon()
    {
        var weapon = new Weapon();
        Assert.Equal(ItemRarity.Common, weapon.Rarity);
    }

    [Fact]
    public void Item_GetDisplayName_ShowsBlessedPrefix()
    {
        var sword = ItemFactory.CreateIronSword();
        sword.IsBlessed = true;
        Assert.Contains("祝福された", sword.GetDisplayName());
    }

    [Fact]
    public void Item_GetDisplayName_ShowsCursedPrefix()
    {
        var sword = ItemFactory.CreateIronSword();
        sword.IsCursed = true;
        Assert.Contains("呪われた", sword.GetDisplayName());
    }

    [Fact]
    public void Item_GetDisplayName_UnidentifiedReturnsUnidentifiedName()
    {
        var sword = ItemFactory.CreateIronSword();
        sword.IsIdentified = false;
        Assert.Equal("不明なアイテム", sword.GetDisplayName());
    }

    [Fact]
    public void Item_CalculatePrice_RarityMultiplierApplied()
    {
        var common = ItemFactory.CreateIronSword();
        var uncommon = ItemFactory.CreateSteelSword();
        // uncommon has 2x rarity multiplier on BasePrice 300 vs common 1x on 100
        Assert.True(uncommon.CalculatePrice() > common.CalculatePrice());
    }

    #endregion

    #region StackableItem

    [Fact]
    public void StackableItem_CanStackWith_SameItemReturnsTrue()
    {
        var potion1 = ItemFactory.CreateMinorHealingPotion();
        var potion2 = ItemFactory.CreateMinorHealingPotion();
        Assert.True(potion1.CanStackWith(potion2));
    }

    [Fact]
    public void StackableItem_CanStackWith_DifferentItemReturnsFalse()
    {
        var potion1 = ItemFactory.CreateMinorHealingPotion();
        var potion2 = ItemFactory.CreateMinorManaPotion();
        Assert.False(potion1.CanStackWith(potion2));
    }

    [Fact]
    public void StackableItem_CalculatePrice_MultipliedByStackCount()
    {
        var potion = ItemFactory.CreateMinorHealingPotion();
        int singlePrice = potion.CalculatePrice();
        potion.StackCount = 5;
        Assert.Equal(singlePrice * 5, potion.CalculatePrice());
    }

    #endregion

    #region Equipment

    [Fact]
    public void Weapon_CreatedViaFactory_HasCorrectSlot()
    {
        var sword = ItemFactory.CreateIronSword();
        Assert.Equal(EquipmentSlot.MainHand, sword.Slot);
    }

    [Fact]
    public void Weapon_BattleAxe_IsTwoHanded()
    {
        var axe = ItemFactory.CreateBattleAxe();
        Assert.True(axe.IsTwoHanded);
    }

    [Fact]
    public void Armor_LeatherArmor_HasCorrectSlotAndType()
    {
        var armor = ItemFactory.CreateLeatherArmor();
        Assert.Equal(EquipmentSlot.Body, armor.Slot);
        Assert.Equal(ArmorType.Leather, armor.ArmorType);
    }

    [Fact]
    public void Armor_CalculateDefense_IncludesEnhancementLevel()
    {
        var armor = ItemFactory.CreateLeatherArmor();
        int baseDefense = armor.CalculateDefense();
        armor.EnhancementLevel = 3;
        Assert.Equal(baseDefense + 3, armor.CalculateDefense());
    }

    [Fact]
    public void Shield_HasOffHandSlot()
    {
        var shield = ItemFactory.CreateWoodenShield();
        Assert.Equal(EquipmentSlot.OffHand, shield.Slot);
        Assert.Equal(ArmorType.Shield, shield.ArmorType);
    }

    [Fact]
    public void Equipment_GetEffectiveStatModifier_EnhancedWeaponGetsStrengthBonus()
    {
        var sword = ItemFactory.CreateIronSword();
        sword.EnhancementLevel = 2;
        var modifier = sword.GetEffectiveStatModifier();
        Assert.Equal(sword.StatModifier.Strength + 2, modifier.Strength);
    }

    #endregion

    #region Consumables

    [Fact]
    public void Scroll_UseFireball_ReturnsDamageEffect()
    {
        var scroll = ItemFactory.CreateScrollOfFireball();
        var player = Entities.Player.Create("テスト", Stats.Default);
        var result = scroll.Use(player);
        Assert.True(result.Success);
        Assert.NotNull(result.Effect);
        Assert.Equal(ItemEffectType.Damage, result.Effect!.Type);
        Assert.Equal(Element.Fire, result.Effect.Element);
    }

    [Fact]
    public void Scroll_UseTeleport_ReturnsSuccess()
    {
        var scroll = ItemFactory.CreateScrollOfTeleport();
        var player = Entities.Player.Create("テスト", Stats.Default);
        var result = scroll.Use(player);
        Assert.True(result.Success);
        Assert.Equal(ItemEffectType.Teleport, result.Effect!.Type);
    }

    [Fact]
    public void Food_Bread_HasCorrectNutrition()
    {
        var bread = ItemFactory.CreateBread();
        Assert.Equal(30, bread.NutritionValue);
        Assert.Equal(FoodType.Bread, bread.FoodType);
    }

    #endregion

    #region ItemFactory static methods

    [Fact]
    public void ItemFactory_CreateRustySword_ReturnsCorrectWeapon()
    {
        var sword = ItemFactory.CreateRustySword();
        Assert.Equal("weapon_rusty_sword", sword.ItemId);
        Assert.Equal(WeaponType.Sword, sword.WeaponType);
        Assert.Equal(ItemRarity.Common, sword.Rarity);
        Assert.Equal(5, sword.BaseDamage);
    }

    [Fact]
    public void ItemFactory_CreateWoodenStaff_HasIntelligenceBonus()
    {
        var staff = ItemFactory.CreateWoodenStaff();
        Assert.Equal(WeaponType.Staff, staff.WeaponType);
        Assert.True(staff.StatModifier.Intelligence > 0);
    }

    [Fact]
    public void ItemFactory_CreatePlateArmor_IsRareWithHighDefense()
    {
        var plate = ItemFactory.CreatePlateArmor();
        Assert.Equal(ItemRarity.Rare, plate.Rarity);
        Assert.Equal(18, plate.BaseDefense);
        Assert.Equal(ArmorType.Plate, plate.ArmorType);
    }

    #endregion
}
