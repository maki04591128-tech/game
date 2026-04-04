namespace RougelikeGame.Core.Items;

/// <summary>
/// アイテム生成ファクトリ
/// </summary>
public class ItemFactory
{
    private readonly Random _random;

    public ItemFactory(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public ItemFactory(Random random)
    {
        _random = random;
    }

    #region Predefined Weapons

    public static Weapon CreateRustySword() => new()
    {
        ItemId = "weapon_rusty_sword",
        Name = "錆びた剣",
        Description = "長い間放置されていた剣。切れ味は悪いが使えなくはない。",
        WeaponType = WeaponType.Sword,
        BaseDamage = 5,
        DamageRange = (3, 7),
        AttackSpeed = 1.0f,
        Range = 1,
        Rarity = ItemRarity.Common,
        BasePrice = 20,
        Weight = 3.0f,
        StatModifier = new StatModifier(Strength: 1)
    };

    public static Weapon CreateIronSword() => new()
    {
        ItemId = "weapon_iron_sword",
        Name = "鉄の剣",
        Description = "一般的な鉄製の剣。扱いやすく信頼性が高い。",
        WeaponType = WeaponType.Sword,
        BaseDamage = 10,
        DamageRange = (8, 12),
        AttackSpeed = 1.0f,
        Range = 1,
        Rarity = ItemRarity.Common,
        BasePrice = 100,
        Weight = 3.5f,
        StatModifier = new StatModifier(Strength: 2)
    };

    public static Weapon CreateSteelSword() => new()
    {
        ItemId = "weapon_steel_sword",
        Name = "鋼の剣",
        Description = "良質な鋼で作られた剣。鉄よりも軽く丈夫。",
        WeaponType = WeaponType.Sword,
        BaseDamage = 15,
        DamageRange = (12, 18),
        AttackSpeed = 1.1f,
        Range = 1,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 300,
        Weight = 3.0f,
        RequiredLevel = 5,
        StatModifier = new StatModifier(Strength: 3, Dexterity: 1)
    };

    public static Weapon CreateDagger() => new()
    {
        ItemId = "weapon_dagger",
        Name = "短剣",
        Description = "素早い攻撃が可能な短い刃。",
        WeaponType = WeaponType.Dagger,
        BaseDamage = 4,
        DamageRange = (3, 5),
        AttackSpeed = 1.5f,
        Range = 1,
        Rarity = ItemRarity.Common,
        BasePrice = 50,
        Weight = 1.0f,
        StatModifier = new StatModifier(Agility: 2)
    };

    public static Weapon CreateMithrilDagger() => new()
    {
        ItemId = "weapon_mithril_dagger",
        Name = "ミスリルダガー",
        Description = "ミスリル鍛造の短剣。軽量ながら鋭い切れ味を持つ。",
        WeaponType = WeaponType.Dagger,
        BaseDamage = 14,
        DamageRange = (11, 17),
        AttackSpeed = 1.6f,
        Range = 1,
        Rarity = ItemRarity.Rare,
        BasePrice = 800,
        Weight = 0.8f,
        StatModifier = new StatModifier(Agility: 4, Dexterity: 2)
    };

    public static Weapon CreateBattleAxe() => new()
    {
        ItemId = "weapon_battle_axe",
        Name = "戦斧",
        Description = "重量級の戦闘用斧。一撃が重い。",
        WeaponType = WeaponType.Axe,
        BaseDamage = 18,
        DamageRange = (14, 22),
        AttackSpeed = 0.7f,
        Range = 1,
        IsTwoHanded = true,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 250,
        Weight = 6.0f,
        RequiredLevel = 8,
        RequiredStats = new Stats(Strength: 14, Vitality: 10, Agility: 0, Dexterity: 0, 
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Strength: 4)
    };

    public static Weapon CreateWoodenStaff() => new()
    {
        ItemId = "weapon_wooden_staff",
        Name = "木の杖",
        Description = "魔力を込めやすい木製の杖。",
        WeaponType = WeaponType.Staff,
        BaseDamage = 3,
        DamageRange = (2, 4),
        AttackSpeed = 0.9f,
        Range = 1,
        Rarity = ItemRarity.Common,
        BasePrice = 80,
        Weight = 2.0f,
        StatModifier = new StatModifier(Intelligence: 3, Mind: 1)
    };

    public static Weapon CreateShortBow() => new()
    {
        ItemId = "weapon_short_bow",
        Name = "ショートボウ",
        Description = "軽量な短弓。扱いやすいが射程は短め。",
        WeaponType = WeaponType.Bow,
        BaseDamage = 6,
        DamageRange = (4, 8),
        AttackSpeed = 1.2f,
        Range = 6,
        IsTwoHanded = true,
        AttackType = AttackType.Ranged,
        Rarity = ItemRarity.Common,
        BasePrice = 120,
        Weight = 1.5f,
        StatModifier = new StatModifier(Dexterity: 2)
    };

    public static Weapon CreateGreatsword() => new()
    {
        ItemId = "weapon_greatsword",
        Name = "グレートソード",
        Description = "両手で振るう巨大な剣。一撃の威力は絶大。",
        WeaponType = WeaponType.Greatsword,
        BaseDamage = 22,
        DamageRange = (16, 28),
        AttackSpeed = 0.6f,
        Range = 1,
        IsTwoHanded = true,
        Rarity = ItemRarity.Rare,
        BasePrice = 400,
        Weight = 10.0f,
        RequiredLevel = 10,
        RequiredStats = new Stats(Strength: 16, Vitality: 10, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Strength: 5)
    };

    public static Weapon CreateSpear() => new()
    {
        ItemId = "weapon_spear",
        Name = "槍",
        Description = "リーチの長い槍。突きに優れる。",
        WeaponType = WeaponType.Spear,
        BaseDamage = 9,
        DamageRange = (7, 12),
        AttackSpeed = 1.0f,
        Range = 2,
        IsTwoHanded = true,
        AttackType = AttackType.Pierce,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 150,
        Weight = 4.0f,
        RequiredLevel = 3,
        StatModifier = new StatModifier(Dexterity: 2, Agility: 1)
    };

    public static Weapon CreateWarHammer() => new()
    {
        ItemId = "weapon_war_hammer",
        Name = "ウォーハンマー",
        Description = "重厚な戦鎚。鎧ごと叩き潰す。",
        WeaponType = WeaponType.Hammer,
        BaseDamage = 16,
        DamageRange = (12, 20),
        AttackSpeed = 0.7f,
        Range = 1,
        IsTwoHanded = true,
        AttackType = AttackType.Blunt,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 250,
        Weight = 8.0f,
        RequiredLevel = 6,
        RequiredStats = new Stats(Strength: 12, Vitality: 8, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Strength: 3)
    };

    public static Weapon CreateCrossbow() => new()
    {
        ItemId = "weapon_crossbow",
        Name = "クロスボウ",
        Description = "弦を機械仕掛けで引く弩。威力が高いが装填に時間がかかる。",
        WeaponType = WeaponType.Crossbow,
        BaseDamage = 12,
        DamageRange = (9, 15),
        AttackSpeed = 0.5f,
        Range = 8,
        IsTwoHanded = true,
        AttackType = AttackType.Ranged,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 200,
        Weight = 4.0f,
        RequiredLevel = 5,
        StatModifier = new StatModifier(Dexterity: 3)
    };

    public static Weapon CreateWhip() => new()
    {
        ItemId = "weapon_whip",
        Name = "鞭",
        Description = "長い革の鞭。リーチがあり、柔軟に攻撃できる。",
        WeaponType = WeaponType.Whip,
        BaseDamage = 5,
        DamageRange = (3, 7),
        AttackSpeed = 1.3f,
        Range = 2,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 1.5f,
        StatModifier = new StatModifier(Dexterity: 2, Agility: 1)
    };

    #endregion

    #region Predefined Armor

    public static Armor CreateLeatherArmor() => new()
    {
        ItemId = "armor_leather",
        Name = "革鎧",
        Description = "軽量で動きやすい革製の鎧。",
        ArmorType = ArmorType.Leather,
        Slot = EquipmentSlot.Body,
        BaseDefense = 5,
        MagicDefense = 2,
        EvasionModifier = 0.05f,
        SpeedModifier = 1.0f,
        Rarity = ItemRarity.Common,
        BasePrice = 80,
        Weight = 5.0f,
        StatModifier = new StatModifier(Vitality: 1)
    };

    public static Armor CreateChainmail() => new()
    {
        ItemId = "armor_chainmail",
        Name = "鎖帷子",
        Description = "鎖を編み込んだ中装鎧。防御と機動のバランスが良い。",
        ArmorType = ArmorType.Chainmail,
        Slot = EquipmentSlot.Body,
        BaseDefense = 10,
        MagicDefense = 3,
        EvasionModifier = 0.0f,
        SpeedModifier = 0.95f,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 200,
        Weight = 12.0f,
        RequiredLevel = 5,
        StatModifier = new StatModifier(Vitality: 2)
    };

    public static Armor CreatePlateArmor() => new()
    {
        ItemId = "armor_plate",
        Name = "板金鎧",
        Description = "重厚な金属板で作られた鎧。防御力は最高クラス。",
        ArmorType = ArmorType.Plate,
        Slot = EquipmentSlot.Body,
        BaseDefense = 18,
        MagicDefense = 5,
        EvasionModifier = -0.1f,
        SpeedModifier = 0.8f,
        Rarity = ItemRarity.Rare,
        BasePrice = 500,
        Weight = 25.0f,
        RequiredLevel = 10,
        RequiredStats = new Stats(Strength: 12, Vitality: 10, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Vitality: 4, Strength: 1)
    };

    public static Armor CreateWizardRobe() => new()
    {
        ItemId = "armor_wizard_robe",
        Name = "魔術師のローブ",
        Description = "魔力を増幅させる織り方で作られたローブ。",
        ArmorType = ArmorType.Robe,
        Slot = EquipmentSlot.Body,
        BaseDefense = 2,
        MagicDefense = 8,
        EvasionModifier = 0.05f,
        SpeedModifier = 1.0f,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 180,
        Weight = 2.0f,
        StatModifier = new StatModifier(Intelligence: 3, Mind: 2)
    };

    public static Shield CreateWoodenShield() => new()
    {
        ItemId = "shield_wooden",
        Name = "木の盾",
        Description = "木製の簡素な盾。ないよりはマシ。",
        BaseDefense = 3,
        MagicDefense = 0,
        BlockChance = 0.15f,
        BlockReduction = 0.3f,
        Rarity = ItemRarity.Common,
        BasePrice = 30,
        Weight = 3.0f,
        StatModifier = new StatModifier(Vitality: 1)
    };

    public static Shield CreateIronShield() => new()
    {
        ItemId = "shield_iron",
        Name = "鉄の盾",
        Description = "頑丈な鉄製の盾。",
        BaseDefense = 6,
        MagicDefense = 1,
        BlockChance = 0.20f,
        BlockReduction = 0.4f,
        Rarity = ItemRarity.Common,
        BasePrice = 100,
        Weight = 6.0f,
        RequiredLevel = 3,
        StatModifier = new StatModifier(Vitality: 2)
    };

    public static Armor CreateIronHelm() => new()
    {
        ItemId = "armor_iron_helm",
        Name = "鉄兜",
        Description = "頑丈な鉄製の兜。頭部を守る。",
        ArmorType = ArmorType.Plate,
        Slot = EquipmentSlot.Head,
        BaseDefense = 6,
        MagicDefense = 1,
        EvasionModifier = 0.0f,
        SpeedModifier = 0.95f,
        Rarity = ItemRarity.Common,
        BasePrice = 80,
        Weight = 3.0f,
        RequiredLevel = 3,
        StatModifier = new StatModifier(Vitality: 1)
    };

    public static Armor CreateLeatherGloves() => new()
    {
        ItemId = "armor_leather_gloves",
        Name = "革の手袋",
        Description = "柔らかい革製の手袋。手を保護する。",
        ArmorType = ArmorType.Leather,
        Slot = EquipmentSlot.Hands,
        BaseDefense = 2,
        MagicDefense = 0,
        EvasionModifier = 0.0f,
        SpeedModifier = 1.0f,
        Rarity = ItemRarity.Common,
        BasePrice = 30,
        Weight = 0.5f,
        StatModifier = new StatModifier(Dexterity: 1)
    };

    public static Armor CreateIronBoots() => new()
    {
        ItemId = "armor_iron_boots",
        Name = "鉄の靴",
        Description = "足元を守る鉄製の靴。",
        ArmorType = ArmorType.Plate,
        Slot = EquipmentSlot.Feet,
        BaseDefense = 4,
        MagicDefense = 0,
        EvasionModifier = -0.02f,
        SpeedModifier = 0.95f,
        Rarity = ItemRarity.Common,
        BasePrice = 60,
        Weight = 4.0f,
        RequiredLevel = 3,
        StatModifier = new StatModifier(Vitality: 1)
    };

    public static Accessory CreateIronRing() => new()
    {
        ItemId = "accessory_iron_ring",
        Name = "鉄の指輪",
        Description = "シンプルな鉄製の指輪。わずかに力を高める。",
        Slot = EquipmentSlot.Ring1,
        Rarity = ItemRarity.Common,
        BasePrice = 50,
        Weight = 0.1f,
        StatModifier = new StatModifier(Strength: 1, Vitality: 1)
    };

    public static Accessory CreateProtectionAmulet() => new()
    {
        ItemId = "accessory_protection_amulet",
        Name = "護りのアミュレット",
        Description = "身を守る魔力が込められた首飾り。",
        Slot = EquipmentSlot.Neck,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 150,
        Weight = 0.2f,
        RequiredLevel = 5,
        StatModifier = new StatModifier(Vitality: 2, Mind: 2),
        PassiveAbility = "MagicDefenseUp"
    };

    public static Accessory CreateSpeedCloak() => new()
    {
        ItemId = "accessory_speed_cloak",
        Name = "疾風のマント",
        Description = "風の魔法が織り込まれたマント。移動速度が上がる。",
        Slot = EquipmentSlot.Back,
        Rarity = ItemRarity.Rare,
        BasePrice = 300,
        Weight = 1.0f,
        RequiredLevel = 8,
        StatModifier = new StatModifier(Agility: 4, Dexterity: 2),
        PassiveAbility = "SpeedUp"
    };

    #endregion

    #region Predefined Consumables

    public static Potion CreateMinorHealingPotion() => new()
    {
        ItemId = "potion_healing_minor",
        Name = "小回復薬",
        Description = "傷を癒す赤い薬。",
        PotionType = PotionType.HealingMinor,
        EffectValue = 0,
        EffectPercentage = 0.25f,  // L-2: MaxHP25%回復（スケーリング対応）
        Rarity = ItemRarity.Common,
        BasePrice = 25,
        Weight = 0.5f
    };

    public static Potion CreateHealingPotion() => new()
    {
        ItemId = "potion_healing",
        Name = "回復薬",
        Description = "効果の高い回復薬。",
        PotionType = PotionType.HealingMajor,
        EffectValue = 0,
        EffectPercentage = 0.50f,  // L-2: MaxHP50%回復
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateMinorManaPotion() => new()
    {
        ItemId = "potion_mana_minor",
        Name = "小マナポーション",
        Description = "魔力を回復する青い薬。",
        PotionType = PotionType.ManaMinor,
        EffectValue = 20,
        Rarity = ItemRarity.Common,
        BasePrice = 30,
        Weight = 0.5f
    };

    public static Potion CreateAntidote() => new()
    {
        ItemId = "potion_antidote",
        Name = "解毒剤",
        Description = "毒を中和する薬。",
        PotionType = PotionType.Antidote,
        Rarity = ItemRarity.Common,
        BasePrice = 40,
        Weight = 0.3f
    };

    public static Potion CreateSuperHealingPotion() => new()
    {
        ItemId = "potion_healing_super",
        Name = "超回復薬",
        Description = "非常に効果の高い回復薬。",
        PotionType = PotionType.HealingSuper,
        EffectValue = 0,
        EffectPercentage = 0.80f,  // L-2: MaxHP80%回復
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.5f
    };

    public static Potion CreateManaPotion() => new()
    {
        ItemId = "potion_mana",
        Name = "マナポーション",
        Description = "魔力を大きく回復する薬。",
        PotionType = PotionType.ManaMajor,
        EffectValue = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateStrengthPotion() => new()
    {
        ItemId = "potion_strength",
        Name = "筋力増強薬",
        Description = "一時的に筋力を高める薬。",
        PotionType = PotionType.StrengthBoost,
        EffectValue = 5,
        Duration = 30,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.5f
    };

    public static Potion CreateAgilityPotion() => new()
    {
        ItemId = "potion_agility",
        Name = "敏捷増強薬",
        Description = "一時的に素早さを高める薬。",
        PotionType = PotionType.AgilityBoost,
        EffectValue = 5,
        Duration = 30,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.5f
    };

    public static Potion CreateInvisibilityPotion() => new()
    {
        ItemId = "potion_invisibility",
        Name = "透明化薬",
        Description = "一時的に透明になれる薬。",
        PotionType = PotionType.Invisibility,
        Duration = 20,
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.5f
    };

    public static Potion CreateFireResistPotion() => new()
    {
        ItemId = "potion_fire_resist",
        Name = "耐火薬",
        Description = "火のダメージを軽減する薬。",
        PotionType = PotionType.FireResistance,
        Duration = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateColdResistPotion() => new()
    {
        ItemId = "potion_cold_resist",
        Name = "耐冷薬",
        Description = "冷気のダメージを軽減する薬。",
        PotionType = PotionType.ColdResistance,
        Duration = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateCureAllPotion() => new()
    {
        ItemId = "potion_cure_all",
        Name = "万能薬",
        Description = "全ての状態異常を治す奇跡の薬。",
        PotionType = PotionType.CureAll,
        Rarity = ItemRarity.Rare,
        BasePrice = 300,
        Weight = 0.5f
    };

    public static Food CreateBread() => new()
    {
        ItemId = "food_bread",
        Name = "パン",
        Description = "一般的なパン。適度に腹を満たせる。",
        FoodType = FoodType.Bread,
        NutritionValue = 30,
        Rarity = ItemRarity.Common,
        BasePrice = 5,
        Weight = 0.3f
    };

    public static Food CreateRation() => new()
    {
        ItemId = "food_ration",
        Name = "保存食",
        Description = "長期保存が可能な携帯食。",
        FoodType = FoodType.Ration,
        NutritionValue = 50,
        Rarity = ItemRarity.Common,
        BasePrice = 15,
        Weight = 0.5f
    };

    public static Food CreateCookedMeat() => new()
    {
        ItemId = "food_cooked_meat",
        Name = "焼き肉",
        Description = "こんがり焼かれた肉。栄養価が高い。",
        FoodType = FoodType.CookedMeat,
        NutritionValue = 60,
        HealValue = 10,
        IsCooked = true,
        Rarity = ItemRarity.Common,
        BasePrice = 20,
        Weight = 0.4f
    };

    public static Food CreateEmergencyRation() => new()
    {
        ItemId = "food_emergency_ration",
        Name = "非常食",
        Description = "緊急用の高カロリー食。味は期待できない。",
        FoodType = FoodType.EmergencyRation,
        NutritionValue = 80,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 40,
        Weight = 0.3f
    };

    public static Food CreateLembas() => new()
    {
        ItemId = "food_lembas",
        Name = "エルフパン",
        Description = "エルフが焼いた旅人のパン。少量で高い栄養がある。",
        FoodType = FoodType.Lembas,
        NutritionValue = 100,
        HealValue = 20,
        Rarity = ItemRarity.Rare,
        BasePrice = 100,
        Weight = 0.2f
    };

    public static Food CreateFruit() => new()
    {
        ItemId = "food_fruit",
        Name = "果物",
        Description = "新鮮な果物。みずみずしい。",
        FoodType = FoodType.Fruit,
        NutritionValue = 20,
        HealValue = 5,
        Rarity = ItemRarity.Common,
        BasePrice = 8,
        Weight = 0.2f
    };

    public static Food CreateWater() => new()
    {
        ItemId = "food_water",
        Name = "水",
        Description = "ダンジョンで汲んだ水。渇きを癒す。",
        FoodType = FoodType.Water,
        NutritionValue = 5,
        HydrationValue = 1,
        Rarity = ItemRarity.Common,
        BasePrice = 5,
        Weight = 0.5f
    };

    public static Food CreateCleanWater() => new()
    {
        ItemId = "food_clean_water",
        Name = "清水",
        Description = "清らかな水。渇きを完全に癒し、少しHPも回復する。",
        FoodType = FoodType.CleanWater,
        NutritionValue = 10,
        HydrationValue = 2,
        HealValue = 10,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 25,
        Weight = 0.5f
    };

    public static Scroll CreateScrollOfTeleport() => new()
    {
        ItemId = "scroll_teleport",
        Name = "テレポートの巻物",
        Description = "読むとランダムな場所にテレポートする。",
        ScrollType = ScrollType.Teleport,
        TargetType = TargetType.Self,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfIdentify() => new()
    {
        ItemId = "scroll_identify",
        Name = "識別の巻物",
        Description = "アイテムの正体を明らかにする。",
        ScrollType = ScrollType.Identify,
        TargetType = TargetType.Item,
        Rarity = ItemRarity.Common,
        BasePrice = 50,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfMagicMapping() => new()
    {
        ItemId = "scroll_magic_mapping",
        Name = "マップの巻物",
        Description = "周囲の地形を明らかにする。",
        ScrollType = ScrollType.MagicMapping,
        TargetType = TargetType.Self,
        EffectValue = 20,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 75,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfFireball() => new()
    {
        ItemId = "scroll_fireball",
        Name = "火球の巻物",
        Description = "強力な炎の球を放つ。",
        ScrollType = ScrollType.Fireball,
        TargetType = TargetType.SingleEnemy,
        EffectValue = 40,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 120,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfLightning() => new()
    {
        ItemId = "scroll_lightning",
        Name = "落雷の巻物",
        Description = "雷を落として敵を撃つ。",
        ScrollType = ScrollType.Lightning,
        TargetType = TargetType.SingleEnemy,
        EffectValue = 50,
        Rarity = ItemRarity.Rare,
        BasePrice = 150,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfFreeze() => new()
    {
        ItemId = "scroll_freeze",
        Name = "凍結の巻物",
        Description = "周囲の敵を凍りつかせる。",
        ScrollType = ScrollType.Freeze,
        TargetType = TargetType.Area,
        EffectRadius = 3,
        EffectValue = 25,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfRemoveCurse() => new()
    {
        ItemId = "scroll_remove_curse",
        Name = "解呪の巻物",
        Description = "装備にかかった呪いを解く。",
        ScrollType = ScrollType.RemoveCurse,
        TargetType = TargetType.Item,
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfEnchant() => new()
    {
        ItemId = "scroll_enchant",
        Name = "強化の巻物",
        Description = "装備品を1段階強化する。",
        ScrollType = ScrollType.Enchant,
        TargetType = TargetType.Item,
        EffectValue = 1,
        Rarity = ItemRarity.Rare,
        BasePrice = 250,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfReturn() => new()
    {
        ItemId = "scroll_return",
        Name = "帰還の巻物",
        Description = "ダンジョン入口に帰還する。",
        ScrollType = ScrollType.Return,
        TargetType = TargetType.Self,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 150,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfSanctuary() => new()
    {
        ItemId = "scroll_sanctuary",
        Name = "聖域の巻物",
        Description = "周囲に結界を張り、敵を退ける。",
        ScrollType = ScrollType.Sanctuary,
        TargetType = TargetType.Self,
        EffectRadius = 4,
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.1f
    };

    public static Scroll CreateAncientBook() => new()
    {
        ItemId = "ancient_book",
        Name = "古代の書",
        Description = "古代のルーン語が記された書物。読むとルーン語を1つ習得できる。",
        ScrollType = ScrollType.AncientBook,
        TargetType = TargetType.Self,
        EffectValue = 3,
        Rarity = ItemRarity.Rare,
        BasePrice = 300,
        Weight = 0.5f
    };

    #endregion

    #region Random Generation

    /// <summary>
    /// 階層に応じたランダムアイテムを生成
    /// </summary>
    public Item GenerateRandomItem(int depth)
    {
        var rarity = DetermineRarity(depth);
        // HW-1/HZ-5: 全アイテムタイプを生成対象に
        int roll = _random.Next(100);
        return roll switch
        {
            < 35 => GenerateRandomEquipment(depth, rarity),
            < 60 => GenerateRandomConsumable(rarity),
            < 75 => GenerateRandomFood(),
            < 90 => GenerateRandomScroll(rarity),
            _ => CreateBread() // Material等はドロップテーブルから生成
        };
    }

    private ItemRarity DetermineRarity(int depth)
    {
        int roll = _random.Next(100);
        int legendaryChance = Math.Min(1 + depth / 10, 5);
        int epicChance = Math.Min(3 + depth / 5, 15);
        int rareChance = Math.Min(10 + depth / 3, 30);
        int uncommonChance = Math.Min(25 + depth, 50);

        if (roll < legendaryChance) return ItemRarity.Legendary;
        if (roll < legendaryChance + epicChance) return ItemRarity.Epic;
        if (roll < legendaryChance + epicChance + rareChance) return ItemRarity.Rare;
        if (roll < legendaryChance + epicChance + rareChance + uncommonChance) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }

    private Item GenerateRandomEquipment(int depth, ItemRarity rarity)
    {
        // BT-1: 全装備種から深さに応じてランダム生成
        var shallowWeapons = new Func<Item>[] { CreateRustySword, CreateDagger, CreateWoodenStaff, CreateShortBow, CreateWoodenShield };
        var midWeapons = new Func<Item>[] { CreateIronSword, CreateBattleAxe, CreateSpear, CreateLeatherArmor, CreateIronShield, CreateCrossbow };
        var deepWeapons = new Func<Item>[] { CreateSteelSword, CreateGreatsword, CreateWarHammer, CreateMithrilDagger, CreatePlateArmor, CreateWhip, CreateIronHelm };

        Func<Item>[] pool;
        if (depth <= 5)
            pool = shallowWeapons;
        else if (depth <= 15)
            pool = shallowWeapons.Concat(midWeapons).ToArray();
        else
            pool = shallowWeapons.Concat(midWeapons).Concat(deepWeapons).ToArray();

        Item baseItem = pool[_random.Next(pool.Length)]();

        // レアリティに応じて強化
        if (baseItem is EquipmentItem equip)
        {
            equip.EnhancementLevel = rarity switch
            {
                ItemRarity.Uncommon => _random.Next(1, 3),
                ItemRarity.Rare => _random.Next(2, 5),
                ItemRarity.Epic => _random.Next(3, 7),
                ItemRarity.Legendary => _random.Next(5, 10),
                _ => 0
            };

            // 拾った装備品は未鑑定
            equip.IsIdentified = false;

            // 呪い付与（深い階層ほど確率上昇: 5% + 階層*2%、最大30%）
            int curseChance = Math.Min(5 + depth * 2, 30);
            if (_random.Next(100) < curseChance)
            {
                equip.IsCursed = true;
            }

            // 祝福付与（レアリティ依存）
            if (!equip.IsCursed && rarity >= ItemRarity.Rare && _random.Next(100) < 20)
            {
                equip.IsBlessed = true;
            }
        }

        return baseItem;
    }

    private Item GenerateRandomConsumable(ItemRarity rarity)
    {
        // BT-2: レアリティに応じて消耗品プールを拡張
        var commonPool = new Func<Item>[] {
            () => rarity >= ItemRarity.Uncommon ? CreateHealingPotion() : CreateMinorHealingPotion(),
            CreateMinorManaPotion,
            CreateAntidote,
            CreateScrollOfIdentify
        };
        var uncommonPool = new Func<Item>[] {
            CreateManaPotion,
            CreateScrollOfTeleport,
            CreateScrollOfMagicMapping,
            CreateCureAllPotion,
            CreateScrollOfRemoveCurse
        };
        var rarePool = new Func<Item>[] {
            CreateStrengthPotion,
            CreateAgilityPotion,
            CreateInvisibilityPotion,
            CreateFireResistPotion,
            CreateColdResistPotion,
            CreateSuperHealingPotion,
            CreateScrollOfFireball,
            CreateScrollOfLightning,
            CreateScrollOfFreeze,
            CreateScrollOfEnchant
        };

        Func<Item>[] pool;
        if (rarity >= ItemRarity.Rare)
            pool = commonPool.Concat(uncommonPool).Concat(rarePool).ToArray();
        else if (rarity >= ItemRarity.Uncommon)
            pool = commonPool.Concat(uncommonPool).ToArray();
        else
            pool = commonPool;

        var item = pool[_random.Next(pool.Length)]();

        // 巻物は未鑑定で生成
        if (item is Scroll scroll)
        {
            scroll.IsIdentified = false;
        }

        return item;
    }

    private Item GenerateRandomFood()
    {
        // U-1: 全食品タイプから生成
        int foodType = _random.Next(8);

        return foodType switch
        {
            0 => CreateBread(),
            1 => CreateRation(),
            2 => CreateCookedMeat(),
            3 => CreateEmergencyRation(),
            4 => CreateFruit(),
            5 => CreateWater(),
            6 => CreateCleanWater(),
            7 => CreateLembas(),
            _ => CreateBread()
        };
    }

    private Item GenerateRandomScroll(ItemRarity rarity)
    {
        var scrollTypes = new[] { ScrollType.Teleport, ScrollType.Identify, ScrollType.MagicMapping,
            ScrollType.Enchant, ScrollType.Freeze, ScrollType.Sanctuary, ScrollType.Return };
        var type = scrollTypes[_random.Next(scrollTypes.Length)];
        return new Scroll
        {
            ItemId = $"scroll_{type}",
            Name = $"{type}の巻物",
            Description = $"{type}の効果を発揮する巻物",
            ScrollType = type,
            EffectValue = rarity >= ItemRarity.Rare ? 30 : 15
        };
    }

    #endregion

    #region LootTable Generation

    /// <summary>
    /// 敵のドロップアイテムを生成
    /// </summary>
    public List<Item> GenerateLoot(int depth, int enemyLevel, ItemRarity baseRarity, float dropRate)
    {
        var items = new List<Item>();

        // ドロップ判定
        if (_random.NextDouble() > dropRate)
            return items;

        // アイテム数を決定
        int itemCount = 1;
        if (_random.NextDouble() < 0.2) itemCount++;
        if (_random.NextDouble() < 0.05) itemCount++;

        for (int i = 0; i < itemCount; i++)
        {
            items.Add(GenerateRandomItem(depth));
        }

        return items;
    }

    #endregion

    #region Material Item Creation

    private static Material CreateMaterialItem(string itemId, string name, string description, MaterialCategory category, int basePrice, ItemRarity rarity = ItemRarity.Common, int quality = 50, float weight = 0.3f)
    {
        return new Material
        {
            ItemId = itemId,
            Name = name,
            Description = description,
            Weight = weight,
            BasePrice = basePrice,
            Rarity = rarity,
            Category = category,
            Quality = quality
        };
    }

    // 獣系素材
    public static Item CreateBeastHide() => CreateMaterialItem("material_beast_hide", "毛皮", "獣から剥ぎ取った毛皮。防具の素材になる", MaterialCategory.Leather, 30);
    public static Item CreateBeastFang() => CreateMaterialItem("material_beast_fang", "獣牙", "鋭い獣の牙。武器の素材になる", MaterialCategory.Monster, 25);

    // 不死系素材
    public static Item CreateBoneFragment() => CreateMaterialItem("material_bone_fragment", "骨片", "魔力を帯びた骨の欠片", MaterialCategory.Bone, 20);
    public static Item CreateCursedEssence() => CreateMaterialItem("material_cursed_essence", "呪いのエッセンス", "不死の魔物から滲み出る呪いの結晶", MaterialCategory.Magical, 80, ItemRarity.Rare, 60);

    // 竜系素材
    public static Item CreateDragonScale() => CreateMaterialItem("material_dragon_scale", "竜鱗", "竜の硬い鱗。最高級の防具素材", MaterialCategory.Monster, 200, ItemRarity.Epic, 80);
    public static Item CreateDragonFang() => CreateMaterialItem("material_dragon_fang", "竜牙", "竜の巨大な牙。伝説の武器素材", MaterialCategory.Monster, 250, ItemRarity.Epic, 85);

    // 昆虫系素材
    public static Item CreateInsectShell() => CreateMaterialItem("material_insect_shell", "甲殻", "硬い虫の甲殻。軽量な防具素材", MaterialCategory.Monster, 20);
    public static Item CreateVenomSac() => CreateMaterialItem("material_venom_sac", "毒嚢", "毒を溜めた嚢。毒系アイテムの材料", MaterialCategory.Monster, 35, ItemRarity.Uncommon);

    // 植物系素材
    public static Item CreateHerb() => CreateMaterialItem("material_herb", "薬草", "ダンジョンに自生する薬草", MaterialCategory.Herb, 15);
    public static Item CreateWoodMaterial() => CreateMaterialItem("material_wood", "樹液", "魔力を含んだ樹液", MaterialCategory.Wood, 20);

    // 悪魔系素材
    public static Item CreateDemonHorn() => CreateMaterialItem("material_demon_horn", "魔角", "悪魔の角。闘の魔力が宿る", MaterialCategory.Monster, 120, ItemRarity.Rare, 65);
    public static Item CreateDarkCrystal() => CreateMaterialItem("material_dark_crystal", "暗黒結晶", "闇の力が凝縮された結晶", MaterialCategory.Magical, 150, ItemRarity.Rare, 70);

    // 精霊系素材
    public static Item CreateSpiritEssence() => CreateMaterialItem("material_spirit_essence", "精霊のエッセンス", "精霊の残した魔力の結晶", MaterialCategory.Magical, 100, ItemRarity.Rare, 65);
    public static Item CreateElementalCore() => CreateMaterialItem("material_elemental_core", "元素核", "元素の力が凝縮された核。高級素材", MaterialCategory.Magical, 180, ItemRarity.Epic, 75);

    // 構造体系素材
    public static Item CreateGolemCore() => CreateMaterialItem("material_golem_core", "核", "ゴーレムの魔力核。貴重な素材", MaterialCategory.Magical, 160, ItemRarity.Rare, 70);
    public static Item CreateIronFragment() => CreateMaterialItem("material_iron_fragment", "鉄片", "鉄の破片。鍛冶素材になる", MaterialCategory.Metal, 15);

    // 不定形系素材
    public static Item CreateSlimeGel() => CreateMaterialItem("material_slime_gel", "ゼリー", "スライムの粘液。様々な用途がある", MaterialCategory.Monster, 10);
    public static Item CreateMagicCrystal() => CreateMaterialItem("material_magic_crystal", "魔力結晶", "魔力が結晶化した希少な素材", MaterialCategory.Magical, 120, ItemRarity.Rare, 60);

    // 蜘蛛系素材
    public static Item CreateSpiderSilk() => CreateMaterialItem("material_spider_silk", "蜘蛛糸", "強靭な蜘蛛の糸。布の素材になる", MaterialCategory.Cloth, 25);

    // 鉱物・環境素材
    public static Item CreateIronOre() => CreateMaterialItem("material_iron_ore", "鉄鉱石", "鉄を含む鉱石。鍛冶の基本素材", MaterialCategory.Metal, 20);
    public static Item CreatePearl() => CreateMaterialItem("material_pearl", "真珠", "海辺の魔物が持つ真珠", MaterialCategory.Gem, 60, ItemRarity.Uncommon, 55);
    public static Item CreateAncientRelic() => CreateMaterialItem("material_ancient_relic", "古代の遺物", "古代文明の遺物。研究価値が高い", MaterialCategory.Magical, 200, ItemRarity.Epic, 80);
    public static Item CreateEquipmentFragment() => CreateMaterialItem("material_equipment_fragment", "装備品の欠片", "朽ちた装備の残骸。鍛冶素材になる", MaterialCategory.Metal, 10);

    // ダンジョン床用素材
    public static Item CreateStone() => CreateMaterialItem("material_stone", "石ころ", "ダンジョンの壁から崩れ落ちた石片", MaterialCategory.Metal, 5, weight: 0.5f);
    public static Item CreateMoss() => CreateMaterialItem("material_moss", "苔", "ダンジョンの湿った壁に生える苔", MaterialCategory.Herb, 8);
    public static Item CreateDungeonMushroom() => CreateMaterialItem("material_mushroom", "ダンジョンキノコ", "暗所に自生する光るキノコ", MaterialCategory.Herb, 12);
    public static Item CreateCrystalShard() => CreateMaterialItem("material_crystal", "結晶片", "ダンジョンの壁面に露出した鉱物結晶", MaterialCategory.Gem, 40, ItemRarity.Uncommon, 45);

    // クラフト・調理用素材
    public static Item CreateCoal() => CreateMaterialItem("material_coal", "石炭", "鍛冶に使う燃料。高温を生み出す", MaterialCategory.Metal, 10);
    public static Item CreateLeather() => CreateMaterialItem("material_leather", "革", "なめした革。防具の素材になる", MaterialCategory.Leather, 20);
    public static Item CreateRawMeat() => CreateMaterialItem("material_raw_meat", "生肉", "新鮮な生肉。調理して食べられる", MaterialCategory.Monster, 8);
    public static Item CreateFishMaterial() => CreateMaterialItem("material_fish", "魚", "新鮮な魚。調理の素材になる", MaterialCategory.Monster, 12);
    public static Item CreateSalt() => CreateMaterialItem("material_salt", "塩", "保存や調理に使う塩", MaterialCategory.Herb, 5);
    public static Item CreateMagicalEssence() => CreateMaterialItem("material_magical_essence", "魔法のエッセンス", "魔力が凝縮されたエッセンス。調合に使う", MaterialCategory.Magical, 100, ItemRarity.Rare, 65);

    // 釣り用アイテム
    public static Item CreateFishCommon1() => CreateMaterialItem("fish_common_1", "小魚", "どこでも釣れる一般的な小魚", MaterialCategory.Monster, 10);
    public static Item CreateFishCommon2() => CreateMaterialItem("fish_common_2", "川魚", "川に棲む一般的な魚", MaterialCategory.Monster, 15);
    public static Item CreateFishMedium1() => CreateMaterialItem("fish_medium_1", "鯛", "美味な中型の魚", MaterialCategory.Monster, 30, ItemRarity.Uncommon);
    public static Item CreateFishMedium2() => CreateMaterialItem("fish_medium_2", "ウナギ", "夏に旬を迎える滋養のある魚", MaterialCategory.Monster, 35, ItemRarity.Uncommon);
    public static Item CreateFishRare1() => CreateMaterialItem("fish_rare_1", "ニジマス", "美しい虹色の鱗を持つ希少魚", MaterialCategory.Monster, 80, ItemRarity.Rare);
    public static Item CreateFishRare2() => CreateMaterialItem("fish_rare_2", "古代魚", "太古から生き続ける神秘的な魚", MaterialCategory.Monster, 150, ItemRarity.Rare, 70);
    public static Item CreateFishLegendary() => CreateMaterialItem("fish_legendary", "幻の大魚", "伝説に語られる巨大な魚", MaterialCategory.Monster, 500, ItemRarity.Legendary, 90);
    public static Item CreateFishTreasure() => new KeyItem
    {
        ItemId = "fish_treasure", Name = "水底の宝箱", Description = "水底に沈んでいた宝箱",
        BasePrice = 200, Rarity = ItemRarity.Rare, Weight = 2.0f
    };
    public static Item CreateFishJunk() => CreateMaterialItem("fish_junk", "ガラクタ", "水中から引き揚げたガラクタ", MaterialCategory.Monster, 1);

    // 採掘用鉱石・宝石
    public static Item CreateOreIron() => CreateMaterialItem("ore_iron", "鉄鉱石", "鉄を含む鉱石。鍛冶の基本素材", MaterialCategory.Metal, 20);
    public static Item CreateOreSilver() => CreateMaterialItem("ore_silver", "銀鉱石", "銀を含む鉱石。装飾品や武器の素材", MaterialCategory.Metal, 50, ItemRarity.Uncommon, 55);
    public static Item CreateOreGold() => CreateMaterialItem("ore_gold", "金鉱石", "金を含む貴重な鉱石", MaterialCategory.Metal, 100, ItemRarity.Rare, 65);
    public static Item CreateOreMithril() => CreateMaterialItem("ore_mithril", "ミスリル鉱石", "伝説の金属ミスリルを含む希少鉱石", MaterialCategory.Metal, 200, ItemRarity.Epic, 80);
    public static Item CreateGemRough() => CreateMaterialItem("gem_rough", "未加工宝石", "磨けば輝く未加工の宝石", MaterialCategory.Gem, 60, ItemRarity.Uncommon, 50);

    #endregion

    #region Dungeon Floor & Enemy Drop Generation

    /// <summary>
    /// ダンジョン床に自然に落ちているアイテムを生成（素材・資源中心）
    /// </summary>
    public Item GenerateDungeonFloorItem(int depth)
    {
        // 深さに応じてアイテムプールを変える
        var pool = GetDungeonFloorItemPool(depth);
        string itemId = pool[_random.Next(pool.Count)];
        return ItemDefinitions.Create(itemId) ?? CreateStone();
    }

    private List<string> GetDungeonFloorItemPool(int depth)
    {
        var pool = new List<string>();

        // 共通（全階層）
        pool.AddRange(new[] { "material_stone", "material_moss", "material_bone_fragment", "material_iron_fragment", "material_equipment_fragment" });

        if (depth <= 10)
        {
            // 浅層: 基本的な素材
            pool.AddRange(new[] { "material_mushroom", "material_herb", "material_stone", "material_moss" });
        }

        if (depth > 3)
        {
            // やや深い: 鉱物・蜘蛛糸
            pool.AddRange(new[] { "material_iron_ore", "material_spider_silk", "material_insect_shell" });
        }

        if (depth > 8)
        {
            // 中層: 結晶・より希少な素材
            pool.AddRange(new[] { "material_crystal", "material_slime_gel", "material_venom_sac" });
        }

        if (depth > 15)
        {
            // 深層: 希少素材
            pool.AddRange(new[] { "material_magic_crystal", "material_cursed_essence", "material_dark_crystal" });
        }

        if (depth > 25)
        {
            // 最深部: 最希少素材
            pool.AddRange(new[] { "material_spirit_essence", "material_elemental_core", "material_ancient_relic" });
        }

        return pool;
    }

    /// <summary>
    /// 敵の種族に応じたドロップアイテムを生成
    /// </summary>
    public Item GenerateEnemyDropItem(int depth, MonsterRace race)
    {
        var pool = GetEnemyDropItemPool(race, depth);
        string itemId = pool[_random.Next(pool.Count)];
        return ItemDefinitions.Create(itemId) ?? CreateStone();
    }

    private static List<string> GetEnemyDropItemPool(MonsterRace race, int depth)
    {
        var pool = new List<string>();

        switch (race)
        {
            case MonsterRace.Beast:
                pool.AddRange(new[] { "material_beast_hide", "material_beast_fang", "material_beast_hide" });
                break;
            case MonsterRace.Humanoid:
                pool.AddRange(new[] { "material_equipment_fragment", "material_iron_fragment" });
                if (depth > 5) pool.Add("material_iron_ore");
                break;
            case MonsterRace.Amorphous:
                pool.AddRange(new[] { "material_slime_gel", "material_slime_gel", "material_magic_crystal" });
                break;
            case MonsterRace.Undead:
                pool.AddRange(new[] { "material_bone_fragment", "material_bone_fragment", "material_cursed_essence" });
                break;
            case MonsterRace.Demon:
                pool.AddRange(new[] { "material_demon_horn", "material_dark_crystal" });
                break;
            case MonsterRace.Dragon:
                pool.AddRange(new[] { "material_dragon_scale", "material_dragon_fang" });
                break;
            case MonsterRace.Plant:
                pool.AddRange(new[] { "material_herb", "material_wood", "material_herb" });
                break;
            case MonsterRace.Insect:
                pool.AddRange(new[] { "material_insect_shell", "material_venom_sac", "material_insect_shell" });
                break;
            case MonsterRace.Spirit:
                pool.AddRange(new[] { "material_spirit_essence", "material_elemental_core" });
                break;
            case MonsterRace.Construct:
                pool.AddRange(new[] { "material_iron_fragment", "material_golem_core", "material_iron_fragment" });
                break;
            default:
                pool.Add("material_stone");
                break;
        }

        return pool;
    }

    #endregion
}

/// <summary>
/// 定義済みアイテムデータベース
/// </summary>
public static class ItemDefinitions
{
    private static readonly Dictionary<string, Func<Item>> _items = new()
    {
        // 武器
        ["weapon_rusty_sword"] = ItemFactory.CreateRustySword,
        ["weapon_iron_sword"] = ItemFactory.CreateIronSword,
        ["weapon_steel_sword"] = ItemFactory.CreateSteelSword,
        ["weapon_dagger"] = ItemFactory.CreateDagger,
        ["weapon_mithril_dagger"] = ItemFactory.CreateMithrilDagger,
        ["weapon_battle_axe"] = ItemFactory.CreateBattleAxe,
        ["weapon_wooden_staff"] = ItemFactory.CreateWoodenStaff,
        ["weapon_short_bow"] = ItemFactory.CreateShortBow,
        ["weapon_greatsword"] = ItemFactory.CreateGreatsword,
        ["weapon_spear"] = ItemFactory.CreateSpear,
        ["weapon_war_hammer"] = ItemFactory.CreateWarHammer,
        ["weapon_crossbow"] = ItemFactory.CreateCrossbow,
        ["weapon_whip"] = ItemFactory.CreateWhip,

        // 防具
        ["armor_leather"] = ItemFactory.CreateLeatherArmor,
        ["armor_chainmail"] = ItemFactory.CreateChainmail,
        ["armor_plate"] = ItemFactory.CreatePlateArmor,
        ["armor_wizard_robe"] = ItemFactory.CreateWizardRobe,
        ["shield_wooden"] = ItemFactory.CreateWoodenShield,
        ["shield_iron"] = ItemFactory.CreateIronShield,
        ["armor_iron_helm"] = ItemFactory.CreateIronHelm,
        ["armor_leather_gloves"] = ItemFactory.CreateLeatherGloves,
        ["armor_iron_boots"] = ItemFactory.CreateIronBoots,

        // アクセサリ
        ["accessory_iron_ring"] = ItemFactory.CreateIronRing,
        ["accessory_protection_amulet"] = ItemFactory.CreateProtectionAmulet,
        ["accessory_speed_cloak"] = ItemFactory.CreateSpeedCloak,

        // ポーション
        ["potion_healing_minor"] = ItemFactory.CreateMinorHealingPotion,
        ["potion_healing"] = ItemFactory.CreateHealingPotion,
        ["potion_healing_super"] = ItemFactory.CreateSuperHealingPotion,
        ["potion_mana_minor"] = ItemFactory.CreateMinorManaPotion,
        ["potion_mana"] = ItemFactory.CreateManaPotion,
        ["potion_antidote"] = ItemFactory.CreateAntidote,
        ["potion_strength"] = ItemFactory.CreateStrengthPotion,
        ["potion_agility"] = ItemFactory.CreateAgilityPotion,
        ["potion_invisibility"] = ItemFactory.CreateInvisibilityPotion,
        ["potion_fire_resist"] = ItemFactory.CreateFireResistPotion,
        ["potion_cold_resist"] = ItemFactory.CreateColdResistPotion,
        ["potion_cure_all"] = ItemFactory.CreateCureAllPotion,

        // 食料
        ["food_bread"] = ItemFactory.CreateBread,
        ["food_ration"] = ItemFactory.CreateRation,
        ["food_cooked_meat"] = ItemFactory.CreateCookedMeat,
        ["food_emergency_ration"] = ItemFactory.CreateEmergencyRation,
        ["food_lembas"] = ItemFactory.CreateLembas,
        ["food_fruit"] = ItemFactory.CreateFruit,
        ["food_water"] = ItemFactory.CreateWater,
        ["food_clean_water"] = ItemFactory.CreateCleanWater,

        // 巻物
        ["scroll_teleport"] = ItemFactory.CreateScrollOfTeleport,
        ["scroll_identify"] = ItemFactory.CreateScrollOfIdentify,
        ["scroll_magic_mapping"] = ItemFactory.CreateScrollOfMagicMapping,
        ["scroll_fireball"] = ItemFactory.CreateScrollOfFireball,
        ["scroll_lightning"] = ItemFactory.CreateScrollOfLightning,
        ["scroll_freeze"] = ItemFactory.CreateScrollOfFreeze,
        ["scroll_remove_curse"] = ItemFactory.CreateScrollOfRemoveCurse,
        ["scroll_enchant"] = ItemFactory.CreateScrollOfEnchant,
        ["scroll_return"] = ItemFactory.CreateScrollOfReturn,
        ["scroll_sanctuary"] = ItemFactory.CreateScrollOfSanctuary,
        ["ancient_book"] = ItemFactory.CreateAncientBook,

        // 素材 - 魔物素材
        ["material_beast_hide"] = ItemFactory.CreateBeastHide,
        ["material_beast_fang"] = ItemFactory.CreateBeastFang,
        ["material_bone_fragment"] = ItemFactory.CreateBoneFragment,
        ["material_cursed_essence"] = ItemFactory.CreateCursedEssence,
        ["material_dragon_scale"] = ItemFactory.CreateDragonScale,
        ["material_dragon_fang"] = ItemFactory.CreateDragonFang,
        ["material_insect_shell"] = ItemFactory.CreateInsectShell,
        ["material_venom_sac"] = ItemFactory.CreateVenomSac,
        ["material_herb"] = ItemFactory.CreateHerb,
        ["material_wood"] = ItemFactory.CreateWoodMaterial,
        ["material_demon_horn"] = ItemFactory.CreateDemonHorn,
        ["material_dark_crystal"] = ItemFactory.CreateDarkCrystal,
        ["material_spirit_essence"] = ItemFactory.CreateSpiritEssence,
        ["material_elemental_core"] = ItemFactory.CreateElementalCore,
        ["material_golem_core"] = ItemFactory.CreateGolemCore,
        ["material_iron_fragment"] = ItemFactory.CreateIronFragment,
        ["material_slime_gel"] = ItemFactory.CreateSlimeGel,
        ["material_magic_crystal"] = ItemFactory.CreateMagicCrystal,
        ["material_spider_silk"] = ItemFactory.CreateSpiderSilk,
        ["material_equipment_fragment"] = ItemFactory.CreateEquipmentFragment,

        // 素材 - 環境資源
        ["material_iron_ore"] = ItemFactory.CreateIronOre,
        ["material_pearl"] = ItemFactory.CreatePearl,
        ["material_ancient_relic"] = ItemFactory.CreateAncientRelic,

        // 素材 - ダンジョン床用
        ["material_stone"] = ItemFactory.CreateStone,
        ["material_moss"] = ItemFactory.CreateMoss,
        ["material_mushroom"] = ItemFactory.CreateDungeonMushroom,
        ["material_crystal"] = ItemFactory.CreateCrystalShard,

        // 素材 - クラフト・調理用
        ["material_coal"] = ItemFactory.CreateCoal,
        ["material_leather"] = ItemFactory.CreateLeather,
        ["material_raw_meat"] = ItemFactory.CreateRawMeat,
        ["material_fish"] = ItemFactory.CreateFishMaterial,
        ["material_salt"] = ItemFactory.CreateSalt,
        ["material_magical_essence"] = ItemFactory.CreateMagicalEssence,

        // 釣りアイテム
        ["fish_common_1"] = ItemFactory.CreateFishCommon1,
        ["fish_common_2"] = ItemFactory.CreateFishCommon2,
        ["fish_medium_1"] = ItemFactory.CreateFishMedium1,
        ["fish_medium_2"] = ItemFactory.CreateFishMedium2,
        ["fish_rare_1"] = ItemFactory.CreateFishRare1,
        ["fish_rare_2"] = ItemFactory.CreateFishRare2,
        ["fish_legendary"] = ItemFactory.CreateFishLegendary,
        ["fish_treasure"] = ItemFactory.CreateFishTreasure,
        ["fish_junk"] = ItemFactory.CreateFishJunk,

        // 採掘用鉱石・宝石
        ["ore_iron"] = ItemFactory.CreateOreIron,
        ["ore_silver"] = ItemFactory.CreateOreSilver,
        ["ore_gold"] = ItemFactory.CreateOreGold,
        ["ore_mithril"] = ItemFactory.CreateOreMithril,
        ["gem_rough"] = ItemFactory.CreateGemRough
    };

    /// <summary>
    /// IDからアイテムを作成
    /// </summary>
    public static Item? Create(string itemId)
    {
        if (!_items.TryGetValue(itemId, out var factory)) return null;
        var item = factory();
        // ファクトリ側でItemIdが未設定の場合、辞書キーを自動設定
        if (string.IsNullOrEmpty(item.ItemId))
        {
            // ItemIdはinitプロパティのため、リフレクションまたは型別に設定
            SetItemId(item, itemId);
        }
        return item;
    }

    /// <summary>
    /// アイテムにItemIdを設定（initプロパティのためリフレクションで設定）
    /// </summary>
    private static void SetItemId(Item item, string itemId)
    {
        // ItemIdプロパティのバッキングフィールドに書き込む
        var prop = typeof(Item).GetProperty(nameof(Item.ItemId));
        prop?.SetValue(item, itemId);
    }

    /// <summary>
    /// 全アイテムIDを取得
    /// </summary>
    public static IEnumerable<string> GetAllItemIds() => _items.Keys;

    /// <summary>
    /// カテゴリでフィルタしたアイテムIDを取得
    /// </summary>
    public static IEnumerable<string> GetItemIdsByCategory(string category)
    {
        return _items.Keys.Where(id => id.StartsWith(category));
    }
}
