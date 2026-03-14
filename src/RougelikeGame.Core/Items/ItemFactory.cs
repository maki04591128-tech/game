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

    #endregion

    #region Predefined Consumables

    public static Potion CreateMinorHealingPotion() => new()
    {
        ItemId = "potion_healing_minor",
        Name = "小回復薬",
        Description = "傷を癒す赤い薬。",
        PotionType = PotionType.HealingMinor,
        EffectValue = 30,
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
        EffectValue = 75,
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

    #endregion

    #region Random Generation

    /// <summary>
    /// 階層に応じたランダムアイテムを生成
    /// </summary>
    public Item GenerateRandomItem(int depth)
    {
        var rarity = DetermineRarity(depth);
        var itemType = (ItemType)_random.Next(3); // Equipment, Consumable, Food

        return itemType switch
        {
            ItemType.Equipment => GenerateRandomEquipment(depth, rarity),
            ItemType.Consumable => GenerateRandomConsumable(rarity),
            ItemType.Food => GenerateRandomFood(),
            _ => CreateBread()
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
        int equipType = _random.Next(4);

        Item baseItem = equipType switch
        {
            0 => CreateIronSword(),
            1 => CreateDagger(),
            2 => CreateLeatherArmor(),
            3 => CreateWoodenShield(),
            _ => CreateIronSword()
        };

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
        }

        return baseItem;
    }

    private Item GenerateRandomConsumable(ItemRarity rarity)
    {
        int consumableType = _random.Next(4);

        return consumableType switch
        {
            0 => rarity >= ItemRarity.Uncommon ? CreateHealingPotion() : CreateMinorHealingPotion(),
            1 => CreateMinorManaPotion(),
            2 => CreateAntidote(),
            3 => CreateScrollOfIdentify(),
            _ => CreateMinorHealingPotion()
        };
    }

    private Item GenerateRandomFood()
    {
        int foodType = _random.Next(3);

        return foodType switch
        {
            0 => CreateBread(),
            1 => CreateRation(),
            2 => CreateCookedMeat(),
            _ => CreateBread()
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
        ["weapon_battle_axe"] = ItemFactory.CreateBattleAxe,
        ["weapon_wooden_staff"] = ItemFactory.CreateWoodenStaff,
        ["weapon_short_bow"] = ItemFactory.CreateShortBow,

        // 防具
        ["armor_leather"] = ItemFactory.CreateLeatherArmor,
        ["armor_chainmail"] = ItemFactory.CreateChainmail,
        ["armor_plate"] = ItemFactory.CreatePlateArmor,
        ["armor_wizard_robe"] = ItemFactory.CreateWizardRobe,
        ["shield_wooden"] = ItemFactory.CreateWoodenShield,
        ["shield_iron"] = ItemFactory.CreateIronShield,

        // ポーション
        ["potion_healing_minor"] = ItemFactory.CreateMinorHealingPotion,
        ["potion_healing"] = ItemFactory.CreateHealingPotion,
        ["potion_mana_minor"] = ItemFactory.CreateMinorManaPotion,
        ["potion_antidote"] = ItemFactory.CreateAntidote,

        // 食料
        ["food_bread"] = ItemFactory.CreateBread,
        ["food_ration"] = ItemFactory.CreateRation,
        ["food_cooked_meat"] = ItemFactory.CreateCookedMeat,

        // 巻物
        ["scroll_teleport"] = ItemFactory.CreateScrollOfTeleport,
        ["scroll_identify"] = ItemFactory.CreateScrollOfIdentify,
        ["scroll_magic_mapping"] = ItemFactory.CreateScrollOfMagicMapping
    };

    /// <summary>
    /// IDからアイテムを作成
    /// </summary>
    public static Item? Create(string itemId)
    {
        return _items.TryGetValue(itemId, out var factory) ? factory() : null;
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
