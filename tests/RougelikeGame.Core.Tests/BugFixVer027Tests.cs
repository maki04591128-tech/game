using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
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

    #region B.23: HygieneStage ステータスペナルティ

    /// <summary>
    /// B.23: 衛生度Foul（不潔）でCHA-10ペナルティが適用されること。
    /// </summary>
    [Fact]
    public void HygieneStage_Foul_ShouldApply_CharismaMinus10()
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene=0 → Foul
        player.ModifyHygiene(-200);
        Assert.Equal(HygieneStage.Foul, player.HygieneStage);

        int expectedCha = baseCha - 10;
        Assert.Equal(expectedCha, player.EffectiveStats.Charisma);
    }

    /// <summary>
    /// B.23: 衛生度Filthy（不衛生）でCHA-5ペナルティが適用されること。
    /// </summary>
    [Fact]
    public void HygieneStage_Filthy_ShouldApply_CharismaMinus5()
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene初期値100→1にする（Filthy: 1-24）
        player.ModifyHygiene(-99);
        Assert.Equal(HygieneStage.Filthy, player.HygieneStage);

        int expectedCha = baseCha - 5;
        Assert.Equal(expectedCha, player.EffectiveStats.Charisma);
    }

    /// <summary>
    /// B.23: 衛生度Dirty（汚れ）でCHA-2ペナルティが適用されること。
    /// </summary>
    [Fact]
    public void HygieneStage_Dirty_ShouldApply_CharismaMinus2()
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene初期値100→25にする（Dirty: 25-49）
        player.ModifyHygiene(-75);
        Assert.Equal(HygieneStage.Dirty, player.HygieneStage);

        int expectedCha = baseCha - 2;
        Assert.Equal(expectedCha, player.EffectiveStats.Charisma);
    }

    /// <summary>
    /// B.23: 衛生度Normal（普通）でCHAペナルティなしであること。
    /// </summary>
    [Fact]
    public void HygieneStage_Normal_ShouldHave_NoCharismaPenalty()
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene初期値100→50にする（Normal: 50-79）
        player.ModifyHygiene(-50);
        Assert.Equal(HygieneStage.Normal, player.HygieneStage);

        Assert.Equal(baseCha, player.EffectiveStats.Charisma);
    }

    /// <summary>
    /// B.23: 衛生度Clean（清潔）でCHA+2ボーナスが適用されること。
    /// </summary>
    [Fact]
    public void HygieneStage_Clean_ShouldApply_CharismaPlus2()
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene初期値100 → Clean (>=80)
        Assert.Equal(HygieneStage.Clean, player.HygieneStage);

        int expectedCha = baseCha + 2;
        Assert.Equal(expectedCha, player.EffectiveStats.Charisma);
    }

    /// <summary>
    /// B.23: 全5段階のCHAペナルティ/ボーナスが設計書§17.2.3と一致すること。
    /// </summary>
    [Theory]
    [InlineData(100, 2)]   // Clean: CHA+2
    [InlineData(80, 2)]    // Clean境界: CHA+2
    [InlineData(79, 0)]    // Normal: ±0
    [InlineData(50, 0)]    // Normal境界: ±0
    [InlineData(49, -2)]   // Dirty: CHA-2
    [InlineData(25, -2)]   // Dirty境界: CHA-2
    [InlineData(24, -5)]   // Filthy: CHA-5
    [InlineData(1, -5)]    // Filthy境界: CHA-5
    [InlineData(0, -10)]   // Foul: CHA-10
    public void HygieneStage_AllLevels_CorrectCharismaModifier(int hygieneValue, int expectedChaModifier)
    {
        var player = CreateTestPlayer();
        int baseCha = player.BaseStats.Charisma;

        // Hygiene初期値100から目標値に調整
        player.ModifyHygiene(hygieneValue - 100);

        int expectedCha = baseCha + expectedChaModifier;
        Assert.Equal(expectedCha, player.EffectiveStats.Charisma);
    }

    #endregion

    #region B.24: Food.Use() 渇き回復（HydrationValue）未実装修正

    private class FixedRandom : IRandomProvider
    {
        private readonly double _val;
        public FixedRandom(double val) => _val = val;
        public int Next(int maxValue) => 0;
        public int Next(int minValue, int maxValue) => minValue;
        public double NextDouble() => _val;
    }

    [Fact]
    public void FoodUse_Water_RestoresThirst()
    {
        var player = CreateTestPlayer();
        // 渇きを減らしておく
        player.ModifyThirst(-50);
        int thirstBefore = player.Thirst;

        var water = ItemFactory.CreateWater();
        water.Use(player);

        Assert.True(player.Thirst > thirstBefore, "水を飲んだら渇きが回復するべき");
        Assert.Equal(thirstBefore + 1, player.Thirst); // HydrationValue=1
    }

    [Fact]
    public void FoodUse_CleanWater_RestoresThirstAndHp()
    {
        var player = CreateTestPlayer();
        player.ModifyThirst(-50);
        player.TakeDamage(Damage.Physical(30));
        int thirstBefore = player.Thirst;
        int hpBefore = player.CurrentHp;

        var cleanWater = ItemFactory.CreateCleanWater();
        cleanWater.Use(player);

        Assert.True(player.Thirst > thirstBefore, "清水を飲んだら渇きが回復するべき");
        Assert.Equal(thirstBefore + 2, player.Thirst); // HydrationValue=2
        Assert.True(player.CurrentHp > hpBefore, "清水を飲んだらHP回復するべき");
    }

    [Fact]
    public void FoodUse_NormalFood_NoThirstRestore()
    {
        var player = CreateTestPlayer();
        player.ModifyThirst(-50);
        int thirstBefore = player.Thirst;

        var bread = ItemFactory.CreateBread();
        bread.Use(player);

        Assert.Equal(thirstBefore, player.Thirst); // HydrationValue=0 なので渇きは変化しない
    }

    [Fact]
    public void FoodUse_Water_MessageContainsThirst()
    {
        var player = CreateTestPlayer();
        player.ModifyThirst(-50);

        var water = ItemFactory.CreateWater();
        var result = water.Use(player);

        Assert.True(result.Success);
        Assert.Contains("渇き", result.Message);
    }

    #endregion

    #region B.25: Food.Use() 腐食食で毒を受けた際に満腹度回復がスキップされるバグ修正

    [Fact]
    public void FoodUse_RottenFoodPoisoned_StillRestoresHunger()
    {
        var player = CreateTestPlayer();
        player.ModifyHunger(-50);
        int hungerBefore = player.Hunger;

        var bread = ItemFactory.CreateBread();
        bread.IsRotten = true;

        // 0.1 < 0.3 なので毒を受ける
        var result = bread.Use(player, new FixedRandom(0.1));

        Assert.True(result.Success);
        Assert.True(player.Hunger > hungerBefore, "腐った食べ物で毒を受けても満腹度は回復するべき");
    }

    [Fact]
    public void FoodUse_RottenFoodNotPoisoned_RestoresHunger()
    {
        var player = CreateTestPlayer();
        player.ModifyHunger(-50);
        int hungerBefore = player.Hunger;

        var bread = ItemFactory.CreateBread();
        bread.IsRotten = true;

        // 0.5 >= 0.3 なので毒は受けない
        var result = bread.Use(player, new FixedRandom(0.5));

        Assert.True(result.Success);
        Assert.True(player.Hunger > hungerBefore, "腐った食べ物でも満腹度は回復するべき");
    }

    [Fact]
    public void FoodUse_RottenWaterPoisoned_RestoresHungerAndThirst()
    {
        var player = CreateTestPlayer();
        player.ModifyHunger(-50);
        player.ModifyThirst(-50);
        int hungerBefore = player.Hunger;
        int thirstBefore = player.Thirst;

        var water = ItemFactory.CreateWater();
        water.IsRotten = true;

        // 0.1 < 0.3 なので毒を受ける
        var result = water.Use(player, new FixedRandom(0.1));

        Assert.True(result.Success);
        Assert.True(player.Hunger > hungerBefore, "腐った水で毒を受けても満腹度は回復するべき");
        Assert.True(player.Thirst > thirstBefore, "腐った水で毒を受けても渇きは回復するべき");
        Assert.Contains("毒", result.Message);
    }

    [Fact]
    public void FoodUse_RottenFoodPoisoned_HasPoisonStatus()
    {
        var player = CreateTestPlayer();
        var bread = ItemFactory.CreateBread();
        bread.IsRotten = true;

        bread.Use(player, new FixedRandom(0.1));

        Assert.True(player.HasStatusEffect(StatusEffectType.Poison));
    }

    [Fact]
    public void FoodUse_RottenFoodPoisoned_NutritionIsHalved()
    {
        var player = CreateTestPlayer();
        player.ModifyHunger(-80);
        int hungerBefore = player.Hunger;

        var bread = ItemFactory.CreateBread();
        int expectedNutrition = bread.NutritionValue / 2;
        bread.IsRotten = true;

        bread.Use(player, new FixedRandom(0.1));

        Assert.Equal(hungerBefore + expectedNutrition, player.Hunger);
    }

    #endregion

    #region B.26: Food.Use()の渇き回復二重適用修正

    [Fact]
    public void B26_FoodUse_HydrationAppliedOnlyOnce()
    {
        // Food.Use()内でModifyThirst(HydrationValue)が1回だけ適用されることを確認
        var player = CreateTestPlayer();
        player.ModifyThirst(-50);
        int thirstBefore = player.Thirst;

        var water = ItemFactory.CreateWater();
        int expectedHydration = water.HydrationValue;

        var result = water.Use(player);

        Assert.True(result.Success);
        // HydrationValue分だけ渇きが回復するべき（二重に適用されてはいけない）
        Assert.Equal(thirstBefore + expectedHydration, player.Thirst);
    }

    [Fact]
    public void B26_FoodUse_WithHealAndHydration_ThirstRestoredOnce()
    {
        // HP回復付きの食料でも渇き回復は1回だけ
        var player = CreateTestPlayer();
        player.ModifyThirst(-50);
        player.ModifyHunger(-50);
        int thirstBefore = player.Thirst;

        var food = new Food
        {
            ItemId = "test_food_heal_hydration",
            Name = "テスト癒し食",
            NutritionValue = 20,
            HydrationValue = 5,
            HealValue = 10
        };

        food.Use(player);
        Assert.Equal(thirstBefore + 5, player.Thirst);
    }

    [Fact]
    public void B26_FoodUse_NoHydration_ThirstUnchanged()
    {
        // HydrationValue=0の食料は渇きを変えない
        var player = CreateTestPlayer();
        player.ModifyThirst(-30);
        int thirstBefore = player.Thirst;

        var bread = new Food
        {
            ItemId = "test_bread",
            Name = "パン",
            NutritionValue = 30,
            HydrationValue = 0
        };

        bread.Use(player);
        Assert.Equal(thirstBefore, player.Thirst);
    }

    #endregion

    #region B.29: HygieneStage enum コメント修正確認

    [Fact]
    public void B29_HygieneStage_FilthyHasCorrectPenalty()
    {
        // Filthy段階のCHAペナルティが-5であることを確認（設計書の「不潔」に対応）
        var player = CreateTestPlayer();
        // 初期状態はClean(CHA+2)なので、まずNormalにする
        player.ModifyHygiene(-player.Hygiene + 60);  // Hygiene=60 → Normal(50-79)
        int normalCharisma = player.EffectiveStats.Charisma;  // CHA=10（ペナルティなし）

        player.ModifyHygiene(-50);  // Hygiene=10 → Filthy(1-24)
        Assert.Equal(HygieneStage.Filthy, player.HygieneStage);

        int filthyCharisma = player.EffectiveStats.Charisma;
        Assert.Equal(normalCharisma - 5, filthyCharisma);
    }

    [Fact]
    public void B29_HygieneStage_FoulHasCorrectPenalty()
    {
        // Foul段階のCHAペナルティが-10であることを確認（設計書の「悪臭」に対応）
        var player = CreateTestPlayer();
        // 初期状態はClean(CHA+2)なので、まずNormalにする
        player.ModifyHygiene(-player.Hygiene + 60);  // Hygiene=60 → Normal(50-79)
        int normalCharisma = player.EffectiveStats.Charisma;  // CHA=10（ペナルティなし）

        player.ModifyHygiene(-60);  // Hygiene=0 → Foul
        Assert.Equal(HygieneStage.Foul, player.HygieneStage);

        int foulCharisma = player.EffectiveStats.Charisma;
        Assert.Equal(normalCharisma - 10, foulCharisma);
    }

    #endregion
}
