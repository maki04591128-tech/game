using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class ElementalAffinitySystemTests
{
    #region GetResistanceLevel Tests

    [Theory]
    [InlineData(MonsterRace.Beast, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Beast, Element.Water, ElementalResistanceLevel.Normal)]
    [InlineData(MonsterRace.Humanoid, Element.Fire, ElementalResistanceLevel.Normal)]
    [InlineData(MonsterRace.Humanoid, Element.Dark, ElementalResistanceLevel.Normal)]
    [InlineData(MonsterRace.Amorphous, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Amorphous, Element.Lightning, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Amorphous, Element.Ice, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Holy, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Light, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Undead, Element.Poison, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Undead, Element.Dark, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Demon, Element.Holy, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Demon, Element.Dark, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Demon, Element.Curse, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Dragon, Element.Ice, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Dragon, Element.Fire, ElementalResistanceLevel.Normal)]
    [InlineData(MonsterRace.Plant, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Plant, Element.Ice, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Plant, Element.Earth, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Plant, Element.Water, ElementalResistanceLevel.Resistant)]
    [InlineData(MonsterRace.Insect, Element.Fire, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Spirit, Element.Light, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Spirit, Element.Dark, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Construct, Element.Lightning, ElementalResistanceLevel.Weakness)]
    [InlineData(MonsterRace.Construct, Element.Poison, ElementalResistanceLevel.Immune)]
    public void GetResistanceLevel_ReturnsCorrectLevel(MonsterRace race, Element element, ElementalResistanceLevel expected)
    {
        var result = ElementalAffinitySystem.GetResistanceLevel(race, element);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetResistanceLevel_NoneElement_ReturnsNormal()
    {
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            var result = ElementalAffinitySystem.GetResistanceLevel(race, Element.None);
            Assert.Equal(ElementalResistanceLevel.Normal, result);
        }
    }

    #endregion

    #region GetDamageMultiplier Tests

    [Theory]
    [InlineData(ElementalResistanceLevel.Weakness, 1.5f)]
    [InlineData(ElementalResistanceLevel.Normal, 1.0f)]
    [InlineData(ElementalResistanceLevel.Resistant, 0.5f)]
    [InlineData(ElementalResistanceLevel.Immune, 0.0f)]
    [InlineData(ElementalResistanceLevel.Absorb, -1.0f)]
    public void GetDamageMultiplier_ReturnsCorrectValue(ElementalResistanceLevel level, float expected)
    {
        var result = ElementalAffinitySystem.GetDamageMultiplier(level);

        Assert.Equal(expected, result);
    }

    #endregion

    #region CalculateElementalDamage Tests

    [Theory]
    [InlineData(100, Element.Fire, MonsterRace.Beast, 150)]    // 弱点: ×1.5
    [InlineData(100, Element.Water, MonsterRace.Beast, 100)]   // 通常: ×1.0
    [InlineData(100, Element.Poison, MonsterRace.Undead, 50)]  // 耐性: ×0.5
    [InlineData(100, Element.Poison, MonsterRace.Construct, 0)] // 無効: ×0.0
    [InlineData(100, Element.None, MonsterRace.Beast, 100)]    // 無属性: ×1.0
    [InlineData(0, Element.Fire, MonsterRace.Beast, 0)]        // ダメージ0
    public void CalculateElementalDamage_ReturnsCorrectDamage(
        int baseDamage, Element element, MonsterRace race, int expected)
    {
        var result = ElementalAffinitySystem.CalculateElementalDamage(baseDamage, element, race);

        Assert.Equal(expected, result);
    }

    #endregion

    #region GetWeaponTypeAttackType Tests

    [Theory]
    [InlineData(WeaponType.Dagger, AttackType.Pierce)]
    [InlineData(WeaponType.Sword, AttackType.Slash)]
    [InlineData(WeaponType.Greatsword, AttackType.Slash)]
    [InlineData(WeaponType.Axe, AttackType.Slash)]
    [InlineData(WeaponType.Greataxe, AttackType.Slash)]
    [InlineData(WeaponType.Spear, AttackType.Pierce)]
    [InlineData(WeaponType.Hammer, AttackType.Blunt)]
    [InlineData(WeaponType.Staff, AttackType.Blunt)]
    [InlineData(WeaponType.Bow, AttackType.Ranged)]
    [InlineData(WeaponType.Crossbow, AttackType.Ranged)]
    [InlineData(WeaponType.Thrown, AttackType.Ranged)]
    [InlineData(WeaponType.Whip, AttackType.Slash)]
    [InlineData(WeaponType.Fist, AttackType.Unarmed)]
    [InlineData(WeaponType.Unarmed, AttackType.Unarmed)]
    public void GetWeaponTypeAttackType_ReturnsCorrectAttackType(WeaponType weaponType, AttackType expected)
    {
        var result = ElementalAffinitySystem.GetWeaponTypeAttackType(weaponType);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWeaponTypeAttackType_AllWeaponTypes_AreMapped()
    {
        foreach (WeaponType wt in Enum.GetValues<WeaponType>())
        {
            var result = ElementalAffinitySystem.GetWeaponTypeAttackType(wt);
            Assert.True(Enum.IsDefined(typeof(AttackType), result),
                $"WeaponType.{wt} returned invalid AttackType");
        }
    }

    #endregion

    #region GetPhysicalDamageMultiplier Tests

    [Theory]
    [InlineData(AttackType.Slash, MonsterRace.Amorphous, 0.5f)]
    [InlineData(AttackType.Pierce, MonsterRace.Amorphous, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Amorphous, 1.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Construct, 1.5f)]
    [InlineData(AttackType.Slash, MonsterRace.Construct, 0.7f)]
    [InlineData(AttackType.Pierce, MonsterRace.Construct, 0.7f)]
    [InlineData(AttackType.Slash, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Pierce, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Unarmed, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Ranged, MonsterRace.Spirit, 0.5f)]
    [InlineData(AttackType.Blunt, MonsterRace.Undead, 1.3f)]
    [InlineData(AttackType.Slash, MonsterRace.Undead, 0.8f)]
    [InlineData(AttackType.Slash, MonsterRace.Humanoid, 1.0f)]
    [InlineData(AttackType.Pierce, MonsterRace.Beast, 1.0f)]
    [InlineData(AttackType.Magic, MonsterRace.Spirit, 1.0f)]
    public void GetPhysicalDamageMultiplier_ReturnsCorrectValues(
        AttackType attackType, MonsterRace race, float expected)
    {
        var result = ElementalAffinitySystem.GetPhysicalDamageMultiplier(attackType, race);

        Assert.Equal(expected, result);
    }

    #endregion
}
