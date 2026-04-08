using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.27 バグ修正テスト
/// B.11〜B.20: セーブ/ロード値クランプ、飢餓/渇き最重度ペナルティ、ペットボーナス、装備表示、ポーション効果
/// </summary>
public class BugFixVer027Tests
{
    private static Player CreateTestPlayer() => Player.Create("テスト", Stats.Default);

    #region B.11: RestoreFromSave Hunger/Thirst 最小値クランプ修正

    [Fact]
    public void RestoreFromSave_NegativeHunger_PreservesNegativeValue()
    {
        var player = CreateTestPlayer();
        player.RestoreFromSave(1, 0, 100, -5, 50, 20, 50, 3);
        // MinHunger=-10なので、-5は範囲内として保持される
        Assert.Equal(-5, player.Hunger);
    }

    [Fact]
    public void RestoreFromSave_NegativeThirst_PreservesNegativeValue()
    {
        var player = CreateTestPlayer();
        player.RestoreFromSave(1, 0, 100, 70, 50, 20, 50, 3, thirst: -8);
        // MinThirst=-10なので、-8は範囲内として保持される
        Assert.Equal(-8, player.Thirst);
    }

    [Fact]
    public void RestoreFromSave_HungerBelowMin_ClampedToMin()
    {
        var player = CreateTestPlayer();
        player.RestoreFromSave(1, 0, 100, -20, 50, 20, 50, 3);
        // MinHunger=-10なので、-20は-10にクランプ
        Assert.Equal(GameConstants.MinHunger, player.Hunger);
    }

    [Fact]
    public void RestoreFromSave_ThirstBelowMin_ClampedToMin()
    {
        var player = CreateTestPlayer();
        player.RestoreFromSave(1, 0, 100, 70, 50, 20, 50, 3, thirst: -20);
        // MinThirst=-10なので、-20は-10にクランプ
        Assert.Equal(GameConstants.MinThirst, player.Thirst);
    }

    #endregion

    #region B.12: HungerStage.Starvation ステータスペナルティ

    [Fact]
    public void Player_StarvationStage_HasStatPenalty()
    {
        var player = CreateTestPlayer();
        var baseStr = player.EffectiveStats.Strength;
        // Hunger=-10で餓死状態
        player.RestoreFromSave(1, 0, 100, GameConstants.MinHunger, 50, 20, 50, 3);
        Assert.Equal(HungerStage.Starvation, player.HungerStage);
        // EffectiveStatsのStrengthがベースより低下していること
        Assert.True(player.EffectiveStats.Strength < baseStr,
            "餓死状態ではStrengthにペナルティが必要");
    }

    #endregion

    #region B.13: ThirstStage.Desiccation ステータスペナルティ

    [Fact]
    public void Player_DesiccationStage_HasStatPenalty()
    {
        var player = CreateTestPlayer();
        var baseInt = player.EffectiveStats.Intelligence;
        // Thirst=-10で干死状態
        player.RestoreFromSave(1, 0, 100, 70, 50, 20, 50, 3, thirst: GameConstants.MinThirst);
        Assert.Equal(ThirstStage.Desiccation, player.ThirstStage);
        // EffectiveStatsのIntelligenceがベースより低下していること
        Assert.True(player.EffectiveStats.Intelligence < baseInt,
            "干死状態ではIntelligenceにペナルティが必要");
    }

    #endregion

    #region B.14: ThirstSystem.GetThirstDamage Desiccation

    [Fact]
    public void GetThirstDamage_Desiccation_Returns20()
    {
        Assert.Equal(20, ThirstSystem.GetThirstDamage(ThirstStage.Desiccation));
    }

    [Fact]
    public void GetThirstDamage_NearDesiccation_Returns10()
    {
        Assert.Equal(10, ThirstSystem.GetThirstDamage(ThirstStage.NearDesiccation));
    }

    [Fact]
    public void GetThirstDamage_Dehydrated_Returns1()
    {
        Assert.Equal(1, ThirstSystem.GetThirstDamage(ThirstStage.Dehydrated));
    }

    [Fact]
    public void GetThirstDamage_Normal_Returns0()
    {
        Assert.Equal(0, ThirstSystem.GetThirstDamage(ThirstStage.Normal));
    }

    #endregion

    #region B.15: ThirstSystem.GetThirstActionCostBonus NearDesiccation/Desiccation

    [Fact]
    public void GetThirstActionCostBonus_Desiccation_Returns5()
    {
        Assert.Equal(5, ThirstSystem.GetThirstActionCostBonus(ThirstStage.Desiccation));
    }

    [Fact]
    public void GetThirstActionCostBonus_NearDesiccation_Returns3()
    {
        Assert.Equal(3, ThirstSystem.GetThirstActionCostBonus(ThirstStage.NearDesiccation));
    }

    [Fact]
    public void GetThirstActionCostBonus_Normal_Returns0()
    {
        Assert.Equal(0, ThirstSystem.GetThirstActionCostBonus(ThirstStage.Normal));
    }

    #endregion

    #region B.16: PetSystem.GetPetAbilityBonuses Horse/Dragon

    [Fact]
    public void PetAbilityBonuses_Horse_HasDropBonus()
    {
        var petSystem = new PetSystem();
        petSystem.AddPet("horse1", "テスト馬", PetType.Horse);
        var (drop, view, dmg, atk) = petSystem.GetPetAbilityBonuses();
        Assert.True(drop > 0, "Horseはドロップボーナスを持つべき");
    }

    [Fact]
    public void PetAbilityBonuses_Dragon_HasAllBonuses()
    {
        var petSystem = new PetSystem();
        petSystem.AddPet("dragon1", "テスト竜", PetType.Dragon);
        var (drop, view, dmg, atk) = petSystem.GetPetAbilityBonuses();
        Assert.True(drop > 0, "Dragonはドロップボーナスを持つべき");
        Assert.True(view > 0, "Dragonは視野ボーナスを持つべき");
        Assert.True(dmg > 0, "Dragonはダメージ軽減ボーナスを持つべき");
        Assert.True(atk > 0, "Dragonは攻撃弱体ボーナスを持つべき");
    }

    #endregion

    #region B.17: Equipment.GetDefaultDisplayChar Waist

    [Fact]
    public void EquipmentItem_WaistSlot_DisplaysTilde()
    {
        var item = new Armor
        {
            Name = "テストベルト",
            Slot = EquipmentSlot.Waist,
            Weight = 1.0f
        };
        Assert.Equal('~', item.DisplayChar);
    }

    #endregion

    #region B.18: IntelligenceBoost ポーション効果修正

    [Fact]
    public void IntelligenceBoostPotion_AppliesBlessingNotProtection()
    {
        var player = CreateTestPlayer();
        var potion = new Potion
        {
            Name = "知力ブースト薬",
            PotionType = PotionType.IntelligenceBoost,
            Duration = 20
        };
        var result = potion.Use(player);
        Assert.True(result.Success);
        // Protection(防御バフ)ではなくBlessing(祝福バフ)が適用されること
        Assert.True(player.StatusEffects.Any(e => e.Type == StatusEffectType.Blessing),
            "IntelligenceBoostポーションはBlessingを適用すべき");
        Assert.False(player.StatusEffects.Any(e => e.Type == StatusEffectType.Protection),
            "IntelligenceBoostポーションはProtectionを適用してはならない");
    }

    #endregion

    #region B.20: RestoreFromSave デフォルト引数

    [Fact]
    public void RestoreFromSave_DefaultThirst_UsesInitialThirst()
    {
        var player = CreateTestPlayer();
        // thirstをデフォルト値で呼び出し
        player.RestoreFromSave(1, 0, 100, 70, 50, 20, 50, 3);
        Assert.Equal(GameConstants.InitialThirst, player.Thirst);
    }

    [Fact]
    public void RestoreFromSave_DefaultHygiene_UsesInitialHygiene()
    {
        var player = CreateTestPlayer();
        // hygieneをデフォルト値で呼び出し
        player.RestoreFromSave(1, 0, 100, 70, 50, 20, 50, 3);
        Assert.Equal(GameConstants.InitialHygiene, player.Hygiene);
    }

    #endregion

    #region B.21: Weapon.GetDefaultDisplayChar 全WeaponType対応

    [Theory]
    [InlineData(WeaponType.Unarmed, ' ')]
    [InlineData(WeaponType.Dagger, '†')]
    [InlineData(WeaponType.Sword, '/')]
    [InlineData(WeaponType.Greatsword, '|')]
    [InlineData(WeaponType.Axe, 'P')]
    [InlineData(WeaponType.Greataxe, 'P')]
    [InlineData(WeaponType.Spear, '/')]
    [InlineData(WeaponType.Hammer, 'T')]
    [InlineData(WeaponType.Staff, '\\')]
    [InlineData(WeaponType.Bow, '}')]
    [InlineData(WeaponType.Crossbow, '{')]
    [InlineData(WeaponType.Thrown, '*')]
    [InlineData(WeaponType.Whip, '~')]
    [InlineData(WeaponType.Fist, ')')]
    public void Weapon_GetDefaultDisplayChar_AllWeaponTypesHaveExplicitChar(WeaponType weaponType, char expectedChar)
    {
        var weapon = new Weapon
        {
            Name = $"テスト{weaponType}",
            WeaponType = weaponType,
            Weight = 1.0f
        };
        Assert.Equal(expectedChar, weapon.DisplayChar);
    }

    #endregion

    #region B.22: Weapon.Category - WeaponType.UnarmedのEquipmentCategory修正

    /// <summary>
    /// B.22: WeaponType.Unarmed は EquipmentCategory.Fist にマッピングされるべき。
    /// 修正前は default ケースで EquipmentCategory.Sword にフォールしていた。
    /// これにより、Monk（Fist熟練）が素手で戦う際に熟練ボーナスが適用されなかった。
    /// </summary>
    [Fact]
    public void Weapon_Category_Unarmed_ShouldBeFist()
    {
        var weapon = new Weapon
        {
            Name = "素手",
            WeaponType = WeaponType.Unarmed,
            Weight = 0f
        };
        Assert.Equal(EquipmentCategory.Fist, weapon.Category);
    }

    /// <summary>
    /// B.22: WeaponType.Fist（格闘武器）も引き続き EquipmentCategory.Fist であることを確認。
    /// </summary>
    [Fact]
    public void Weapon_Category_Fist_ShouldBeFist()
    {
        var weapon = new Weapon
        {
            Name = "ナックル",
            WeaponType = WeaponType.Fist,
            Weight = 1.0f
        };
        Assert.Equal(EquipmentCategory.Fist, weapon.Category);
    }

    /// <summary>
    /// B.22: 全WeaponTypeのCategory導出が正しいことを包括的に確認。
    /// </summary>
    [Theory]
    [InlineData(WeaponType.Unarmed, EquipmentCategory.Fist)]
    [InlineData(WeaponType.Dagger, EquipmentCategory.Dagger)]
    [InlineData(WeaponType.Sword, EquipmentCategory.Sword)]
    [InlineData(WeaponType.Greatsword, EquipmentCategory.Sword)]
    [InlineData(WeaponType.Axe, EquipmentCategory.Axe)]
    [InlineData(WeaponType.Greataxe, EquipmentCategory.Axe)]
    [InlineData(WeaponType.Spear, EquipmentCategory.Spear)]
    [InlineData(WeaponType.Hammer, EquipmentCategory.Mace)]
    [InlineData(WeaponType.Staff, EquipmentCategory.Staff)]
    [InlineData(WeaponType.Bow, EquipmentCategory.Bow)]
    [InlineData(WeaponType.Crossbow, EquipmentCategory.Bow)]
    [InlineData(WeaponType.Thrown, EquipmentCategory.Bow)]
    [InlineData(WeaponType.Whip, EquipmentCategory.Whip)]
    [InlineData(WeaponType.Fist, EquipmentCategory.Fist)]
    public void Weapon_Category_AllWeaponTypes_CorrectCategory(WeaponType weaponType, EquipmentCategory expectedCategory)
    {
        var weapon = new Weapon
        {
            Name = $"テスト{weaponType}",
            WeaponType = weaponType,
            Weight = 1.0f
        };
        Assert.Equal(expectedCategory, weapon.Category);
    }

    /// <summary>
    /// B.22: Monk（Fist熟練）がUnarmedで熟練判定を通ることを確認。
    /// </summary>
    [Fact]
    public void Monk_ShouldBeProficient_WithUnarmed()
    {
        Assert.True(ClassEquipmentSystem.IsProficient(CharacterClass.Monk, EquipmentCategory.Fist));

        // UnarmedのCategoryがFistなので、Monkは素手で熟練ボーナスを得る
        var unarmed = new Weapon { Name = "素手", WeaponType = WeaponType.Unarmed, Weight = 0f };
        Assert.True(ClassEquipmentSystem.IsProficient(CharacterClass.Monk, unarmed.Category));
    }

    /// <summary>
    /// B.22: Fighter（Sword熟練だがFist非熟練）はUnarmedで熟練判定を通らないことを確認。
    /// </summary>
    [Fact]
    public void Fighter_ShouldNotBeProficient_WithUnarmed()
    {
        Assert.False(ClassEquipmentSystem.IsProficient(CharacterClass.Fighter, EquipmentCategory.Fist));

        var unarmed = new Weapon { Name = "素手", WeaponType = WeaponType.Unarmed, Weight = 0f };
        Assert.False(ClassEquipmentSystem.IsProficient(CharacterClass.Fighter, unarmed.Category));
    }

    #endregion
}
