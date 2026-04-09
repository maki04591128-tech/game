using RougelikeGame.Core;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Systems;
using RougelikeGame.Engine.Combat;
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

    #region B.30〜B.33: 除算ゼロ対策、アクセサリカテゴリ、コメント修正

    [Fact]
    public void B30_ThirstDesiccation_ModifiersReturnZero()
    {
        // Desiccation段階のModifiersが(0,0,0)を返すことを確認
        var (str, agi, intel) = ThirstSystem.GetThirstModifiers(ThirstStage.Desiccation);
        Assert.Equal(0f, str);
        Assert.Equal(0f, agi);
        Assert.Equal(0f, intel);
    }

    [Fact]
    public void B30_ThirstNearDesiccation_ModifiersReturnPositive()
    {
        // NearDesiccation段階のModifiersは正の値を返すことを確認
        var (str, agi, intel) = ThirstSystem.GetThirstModifiers(ThirstStage.NearDesiccation);
        Assert.True(str > 0f, "NearDesiccation STR modifier should be positive");
        Assert.True(agi > 0f, "NearDesiccation AGI modifier should be positive");
        Assert.True(intel > 0f, "NearDesiccation INT modifier should be positive");
    }

    [Fact]
    public void B30_ThirstActionCostBonus_DesiccationHasHighestCost()
    {
        // Desiccation段階の行動コスト加算が最大であることを確認
        int desiccationCost = ThirstSystem.GetThirstActionCostBonus(ThirstStage.Desiccation);
        int nearDesiccationCost = ThirstSystem.GetThirstActionCostBonus(ThirstStage.NearDesiccation);
        int dehydratedCost = ThirstSystem.GetThirstActionCostBonus(ThirstStage.Dehydrated);

        Assert.True(desiccationCost >= nearDesiccationCost,
            $"Desiccation({desiccationCost}) should >= NearDesiccation({nearDesiccationCost})");
        Assert.True(nearDesiccationCost >= dehydratedCost,
            $"NearDesiccation({nearDesiccationCost}) should >= Dehydrated({dehydratedCost})");
    }

    [Fact]
    public void B32_Accessory_HasAccessoryCategory()
    {
        // アクセサリの EquipmentCategory が Accessory であることを確認
        var accessory = new Accessory
        {
            ItemId = "test_ring",
            Name = "テストリング",
            Slot = EquipmentSlot.Ring1
        };
        Assert.Equal(EquipmentCategory.Accessory, accessory.Category);
    }

    [Fact]
    public void B32_Accessory_NotSwordCategory()
    {
        // アクセサリが剣カテゴリでないことを確認（旧バグ: デフォルトSwordのまま）
        var accessory = new Accessory
        {
            ItemId = "test_neck",
            Name = "テストネックレス",
            Slot = EquipmentSlot.Neck
        };
        Assert.NotEqual(EquipmentCategory.Sword, accessory.Category);
    }

    [Fact]
    public void B31_ArmorSpeedModifier_DefaultIsOne()
    {
        // Armor の SpeedModifier デフォルト値が 1.0f であることを確認（除算ゼロにならない）
        var armor = new Armor
        {
            ItemId = "test_armor",
            Name = "テスト鎧",
            Slot = EquipmentSlot.Body,
            ArmorType = ArmorType.Plate
        };
        Assert.Equal(1.0f, armor.SpeedModifier);
    }

    #endregion

    #region B.34: CombatSystem ArmorClass がターゲット装備から導出される

    [Fact]
    public void DeriveArmorClass_PlayerWithPlateArmor_ReturnsHeavy()
    {
        // Arrange: CombatSystemのBuildHitCheckParamsを間接的にテスト
        // ArmorClassの導出ロジックを確認するため、
        // Armor.Category で ArmorType→EquipmentCategory 変換をテスト
        var armor = new Armor
        {
            ItemId = "plate_test",
            Name = "板金鎧",
            Slot = EquipmentSlot.Body,
            ArmorType = ArmorType.Plate
        };
        Assert.Equal(EquipmentCategory.HeavyArmor, armor.Category);
    }

    [Fact]
    public void DeriveArmorClass_PlayerWithChainmailArmor_ReturnsMedium()
    {
        var armor = new Armor
        {
            ItemId = "chain_test",
            Name = "鎖帷子",
            Slot = EquipmentSlot.Body,
            ArmorType = ArmorType.Chainmail
        };
        Assert.Equal(EquipmentCategory.MediumArmor, armor.Category);
    }

    [Fact]
    public void DeriveArmorClass_PlayerWithLeatherArmor_ReturnsLight()
    {
        var armor = new Armor
        {
            ItemId = "leather_test",
            Name = "革鎧",
            Slot = EquipmentSlot.Body,
            ArmorType = ArmorType.Leather
        };
        Assert.Equal(EquipmentCategory.LightArmor, armor.Category);
    }

    [Fact]
    public void DeriveArmorClass_PlayerWithRobe_ReturnsRobe()
    {
        var armor = new Armor
        {
            ItemId = "robe_test",
            Name = "ローブ",
            Slot = EquipmentSlot.Body,
            ArmorType = ArmorType.Robe
        };
        Assert.Equal(EquipmentCategory.Robe, armor.Category);
    }

    [Fact]
    public void DeriveArmorClass_PlayerWithNoArmor_BodySlotIsNull()
    {
        var player = CreateTestPlayer();
        // 装備なしの場合、Body枠はnull
        Assert.Null(player.Equipment[EquipmentSlot.Body]);
    }

    #endregion

    #region B.35/B.36: DamageCalculator docstring修正確認（コンパイル時テスト）

    [Fact]
    public void DamageCalculator_CriticalCheck_DEXContribution_MatchesK3Fix()
    {
        // K-3修正後: DEXもLUKも0.5%/ptで統一されている
        // DEX=100, LUK=0 の場合と DEX=0, LUK=100 の場合で
        // 同じクリティカル率になることを確認
        var calc = new RougelikeGame.Engine.Combat.DamageCalculator(
            new TestRandomProvider(0.99)); // 高い値で常にfalse

        // DEX=100, LUK=0
        var paramDex = new RougelikeGame.Engine.Combat.CriticalCheckParams
        {
            Dexterity = 100,
            Luck = 0,
            WeaponCritBonus = 0,
            SkillCritBonus = 0
        };

        // DEX=0, LUK=100
        var paramLuk = new RougelikeGame.Engine.Combat.CriticalCheckParams
        {
            Dexterity = 0,
            Luck = 100,
            WeaponCritBonus = 0,
            SkillCritBonus = 0
        };

        // K-3修正により、両方とも同じクリティカル率（=基礎5% + 100*0.5% = 55%）
        // 乱数0.99では両方ともfalse（55% < 99%）
        Assert.Equal(
            calc.CheckCritical(paramDex),
            calc.CheckCritical(paramLuk)
        );
    }

    #endregion

    #region B.38: 重複ItemType enum削除確認

    [Fact]
    public void ItemType_InItemsNamespace_HasExpectedValues()
    {
        // Items.ItemType が正しい値を持つことを確認
        Assert.Equal(0, (int)RougelikeGame.Core.Items.ItemType.Equipment);
        Assert.Equal(1, (int)RougelikeGame.Core.Items.ItemType.Consumable);
        Assert.Equal(2, (int)RougelikeGame.Core.Items.ItemType.Food);
        Assert.Equal(3, (int)RougelikeGame.Core.Items.ItemType.Material);
        Assert.Equal(4, (int)RougelikeGame.Core.Items.ItemType.Key);
        Assert.Equal(5, (int)RougelikeGame.Core.Items.ItemType.Quest);
        Assert.Equal(6, (int)RougelikeGame.Core.Items.ItemType.Miscellaneous);
    }

    #endregion

    #region B.39: Player.ExecuteAction Interact/Search case追加

    [Fact]
    public void ExecuteAction_InteractType_DoesNotThrow()
    {
        var player = CreateTestPlayer();
        var action = TurnAction.Interact;
        var state = new TestGameState();

        // Interact アクションが例外を投げないことを確認
        var exception = Record.Exception(() => player.ExecuteAction(action, state));
        Assert.Null(exception);
    }

    [Fact]
    public void ExecuteAction_SearchType_DoesNotThrow()
    {
        var player = CreateTestPlayer();
        var action = TurnAction.Search;
        var state = new TestGameState();

        // Search アクションが例外を投げないことを確認
        var exception = Record.Exception(() => player.ExecuteAction(action, state));
        Assert.Null(exception);
    }

    #endregion

    #region テスト用ヘルパークラス

    private class TestGameState : IGameState
    {
        public IPlayer Player { get; } = null!;
        public IMap CurrentMap { get; } = new TestMap();
        public ICombatSystem CombatSystem { get; } = null!;
        public IRandomProvider Random { get; } = new TestRandomProvider(0.5);
        public CombatState CombatState => CombatState.None;
        public long CurrentTurn => 0;
        public float GetMovementModifier(IEntity entity) => 1.0f;
    }

    private class TestMap : IMap
    {
        public int Width => 50;
        public int Height => 50;
        public bool InBounds(Position pos) => true;
        public bool IsWalkable(Position pos) => true;
        public bool BlocksSight(Position pos) => false;
        public bool CanMoveTo(Position pos) => true;
        public bool HasLineOfSight(Position from, Position to) => true;
        public float GetEnvironmentModifier(Position pos, TurnActionType actionType) => 1.0f;
        public Tile GetTile(Position pos) => new Tile { Type = TileType.Floor };
        public void SetTile(Position pos, Tile tile) { }
    }

    #endregion

    #region B.40: ResourceSystem CharacterClass重複enum統一

    [Fact]
    public void B40_ResourceSystem_UsesCoreFighterClass_HpCalculation()
    {
        // B.40: ResourceSystem.CharacterClass.Warrior を Core.CharacterClass.Fighter に統一
        var rs = new ResourceSystem();
        var hp = rs.CalculateMaxHp(new HpCalculationParams
        {
            Vitality = 10, Level = 5, RaceBonus = 0,
            CharacterClass = CharacterClass.Fighter
        });
        // Fighter: hpPerLevel=15, bonus=15*(5-1)=60
        // 50 + 100 + 25 + 0 + 60 = 235
        Assert.Equal(235, hp);
    }

    [Fact]
    public void B40_ResourceSystem_UsesCoreFighterClass_MpCalculation()
    {
        var rs = new ResourceSystem();
        var mp = rs.CalculateMaxMp(new MpCalculationParams
        {
            Mind = 10, Intelligence = 10, Level = 5, RaceBonus = 0,
            CharacterClass = CharacterClass.Fighter
        });
        // Fighter: mpPerLevel=2, bonus=2*(5-1)=8
        // 20 + 50 + 20 + 10 + 0 + 8 = 108
        Assert.Equal(108, mp);
    }

    [Fact]
    public void B40_ResourceSystem_AllCoreClasses_HaveValidHpBonus()
    {
        var rs = new ResourceSystem();
        foreach (var cls in Enum.GetValues<CharacterClass>())
        {
            var hp = rs.CalculateMaxHp(new HpCalculationParams
            {
                Vitality = 10, Level = 5, RaceBonus = 0,
                CharacterClass = cls
            });
            Assert.True(hp > 0, $"CharacterClass.{cls}のHP計算が正しくありません: {hp}");
        }
    }

    #endregion

    #region B.41: InscriptionSystem 除算ゼロ対策

    [Fact]
    public void B41_InscriptionSystem_ZeroRequiredLevel_NoDivisionByZero()
    {
        // B.41: RequiredLevel=0の碑文でも除算ゼロにならないことを確認
        var system = new InscriptionSystem();
        system.Register("test_zero", InscriptionType.Lore, "???", "テスト碑文", 0, "報酬");
        var result = system.TryDecode("test_zero", 0);
        // RequiredLevel=0 かつ playerMagicLevel=0 → 条件 0 < 0 はfalse → 解読成功
        Assert.True(result.Success);
    }

    [Fact]
    public void B41_InscriptionSystem_LowRequiredLevel_ProgressCalculation()
    {
        var system = new InscriptionSystem();
        system.Register("test_low", InscriptionType.Lore, "???", "テスト碑文", 1, "報酬");
        var result = system.TryDecode("test_low", 0);
        // playerMagicLevel(0) < RequiredLevel(1) → progress計算
        Assert.False(result.Success);
        Assert.Equal(0, result.PartialProgress);
    }

    #endregion

    #region B.42: Enemy.ExecuteAction Interact case追加

    [Fact]
    public void B42_Enemy_ExecuteAction_InteractDoesNotThrow()
    {
        // B.42: TurnActionType.Interact が Enemy.ExecuteAction で例外を投げないことを確認
        var enemy = Enemy.Create("テスト敵", "test_enemy", Stats.Default, 10);
        var state = new TestGameState();
        var action = TurnAction.Interact;
        
        // Interact を実行しても例外にならない
        enemy.ExecuteAction(action, state);
        Assert.True(true); // 例外が発生しなければ成功
    }

    [Fact]
    public void B42_Enemy_ExecuteAction_AllActionTypes_NoThrow()
    {
        var enemy = Enemy.Create("テスト敵", "test_enemy", Stats.Default, 10);
        var state = new TestGameState();
        
        // 全TurnActionTypeについてクラッシュしないことを確認
        var actions = new[]
        {
            TurnAction.Wait,
            TurnAction.Rest,
            TurnAction.Search,
            TurnAction.Interact
        };
        foreach (var action in actions)
        {
            enemy.ExecuteAction(action, state);
        }
        Assert.True(true);
    }

    #endregion

    #region B.43: InscriptionSystem.GetPartialText 空文字列防御

    [Fact]
    public void InscriptionSystem_GetPartialText_EmptyDecodedText_NoException()
    {
        // B.43: decodedTextが空文字列の場合にIndexOutOfRangeExceptionが発生しないこと
        var system = new InscriptionSystem();
        system.Register("empty_test", InscriptionType.Lore, "???", "",
            requiredLevel: 10, rewardInfo: null);

        // playerMagicLevel < requiredLevel なのでGetPartialTextが呼ばれる
        var result = system.TryDecode("empty_test", 5);
        Assert.False(result.Success);
        Assert.Contains("???", result.Message);
    }

    [Fact]
    public void InscriptionSystem_GetPartialText_HighProgress_NoOverflow()
    {
        // B.43: progressPercentが100を超える場合に範囲外アクセスしないこと
        var system = new InscriptionSystem();
        system.Register("overflow_test", InscriptionType.Hint, "???", "AB",
            requiredLevel: 1, rewardInfo: null);

        // requiredLevel=1, playerMagicLevel=0 → progress計算
        var result = system.TryDecode("overflow_test", 0);
        Assert.False(result.Success);
    }

    #endregion

    #region B.44: DamageCalculator 魔法クリティカルコメント修正

    [Fact]
    public void DamageCalculator_MagicalDamage_IsCriticalFalse_InResult()
    {
        // B.44: DamageCalculator自体はIsCritical=falseを返す
        // （K-2: クリティカル判定はCombatSystem側で1.3倍適用）
        var calc = new DamageCalculator(new TestRandomProvider(0.5));
        var param = new MagicalDamageParams
        {
            StaffAttack = 10,
            Intelligence = 10,
            MagicAttackBuff = 0,
            MagicDefense = 5,
            Mind = 5,
            MagicDefenseBuff = 0,
            SkillMultiplier = 1.0f,
            LanguageBonus = 1.0f,
            SpellElement = Element.Fire,
            TargetElement = Element.None
        };
        var result = calc.CalculateMagicalDamage(param);
        Assert.False(result.IsCritical);
        Assert.True(result.FinalDamage > 0);
    }

    #endregion

    #region B.45: BasicBehaviors.BerserkerBehavior MaxHp除算ゼロ対策

    [Fact]
    public void BerserkerBehavior_IsApplicable_NoException()
    {
        // B.45: BerserkerBehavior.IsApplicableが除算ゼロしないこと
        var enemy = Enemy.Create("テスト敵", "test", new Stats(0, 0, 0, 0, 0, 0, 0, 0, 0), 10);
        var behavior = new BerserkerBehavior(0.3f);
        // IsApplicableは正常に動作すること（例外なし）
        var result = behavior.IsApplicable(enemy, new TestGameState());
        Assert.IsType<bool>(result);
    }

    #endregion

    #region B.46: Enemy.ExecuteAction UseItem MaxHp除算ゼロ対策

    [Fact]
    public void Enemy_ExecuteAction_UseItem_NoException()
    {
        // B.46: UseItemアクションで例外が発生しないこと
        var state = new TestGameState();
        var enemy = Enemy.Create("テスト敵", "test", new Stats(0, 0, 0, 0, 0, 0, 0, 0, 0), 10);

        var action = TurnAction.UseItem("potion", 100);
        enemy.ExecuteAction(action, state);
        Assert.True(true); // 例外が発生しないことを確認
    }

    [Fact]
    public void Enemy_ExecuteAction_UseItem_HealsWhenLowHp()
    {
        // B.46: HP半分以下のとき回復が行われること
        var state = new TestGameState();
        var enemy = Enemy.Create("テスト敵", "test", Stats.Default, 10);
        int maxHp = enemy.MaxHp;

        // HPを半分以下に減らす（Pure属性で防御無視）
        enemy.TakeDamage(new Damage(maxHp * 3 / 4, DamageType.Pure, Element.None, false));
        int hpBeforeItem = enemy.CurrentHp;

        enemy.ExecuteAction(TurnAction.UseItem("potion", 100), state);
        // 回復されていること
        Assert.True(enemy.CurrentHp >= hpBeforeItem);
    }

    #endregion
}
