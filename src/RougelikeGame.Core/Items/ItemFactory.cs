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
        Description = "長い年月で赤錆に侵食された剣。かつては誰かの愛用品だったのかもしれない。切れ味は鈍いが、重量で殴ることはできる。",
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
        Description = "王国の兵士が標準装備として使用する鉄製の直剣。バランスが良く、初心者から熟練者まで幅広く愛用される信頼の一品。",
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
        Description = "高純度の鋼を熟練の鍛冶師が鍛えた剣。鉄より軽く、しかし数倍丈夫。刃の美しさは実用性と審美性を兼ね備えている。",
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
        Description = "懐に隠せる短い両刃の刃。斬撃より刺突に優れ、鎧の隙間を狙うのに適している。盗賊や斥候がよく携帯する武器。",
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
        Description = "分厚い鋼鉄を打ち出した戦闘用の斧。その重量は並の戦士では片手で振ることすら叶わないが、熟練の戦士が全力で叩きつければ鎧諸共に敵を両断する。血溝が刻まれた刃は、幾多の戦場を潜り抜けてきた証でもある。",
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
        Description = "魔力伝導率の高い霊木から削り出された杖。木目には微かな魔力の脈動が走り、術者の意思に応じて先端が淡く光る。初学の魔術師が最初に手にする入門用の杖として広く流通している。",
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
        Description = "狩猟用に設計された小型の短弓。軽量で取り回しがよく、ダンジョンの狭い通路でも素早く射撃できる。射程は長弓に劣るものの、初心者でも直感的に扱えるため、冒険者の間で根強い人気がある。",
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
        Description = "成人の背丈ほどもある巨大な両手剣。その圧倒的な質量から繰り出される一撃は、硬い甲殻を持つ魔物すら粉砕する。扱うには相当の筋力と体力が求められるが、戦場での制圧力は他の追随を許さない。",
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
        Description = "堅木の柄に鋼の穂先を備えた歩兵用の槍。長いリーチを活かした突き攻撃は敵の間合いの外から一方的に攻撃でき、集団戦では無類の強さを発揮する。軽量で扱いやすく、初心者の冒険者にも推奨される武器。",
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
        Description = "鍛鉄の塊を柄に据えた重厚な戦鎚。斬れ味ではなく純粋な打撃力で敵を叩き潰す。板金鎧の上からでもダメージを通す貫通力を持ち、骨を砕く鈍い衝撃音は戦場に恐怖を撒き散らす。",
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
        Description = "精密な歯車機構で弦を引く機械式の弩。腕力に頼らず安定した威力を発揮できるため、非力な者でも扱える。装填に時間を要するのが弱点だが、一撃の貫通力は通常の弓を大きく凌駕する。",
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
        Description = "上質な革を幾重にも編み込んだ長鞭。しなやかな動きで敵の武器を絡め取ったり、離れた位置から正確に打ち据えることができる。使いこなすには高い技術が求められるが、その変幻自在な攻撃は敵の防御を翻弄する。",
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

    public static Weapon CreateMithrilSword() => new()
    {
        ItemId = "weapon_mithril_sword",
        Name = "ミスリルの剣",
        Description = "伝説の金属ミスリルから鍛え上げられた銀白色の剣。羽根のように軽く、しかし鋼の数倍の硬度を誇る。刃は永遠に錆びず、月光の下では青白い燐光を放つ。その切れ味はあらゆる鎧を紙のように切り裂くと言われる。",
        WeaponType = WeaponType.Sword,
        BaseDamage = 25,
        DamageRange = (20, 30),
        AttackSpeed = 1.2f,
        Range = 1,
        Rarity = ItemRarity.Legendary,
        BasePrice = 2000,
        Weight = 2.5f,
        RequiredLevel = 15,
        StatModifier = new StatModifier(Strength: 5, Dexterity: 3)
    };

    public static Weapon CreateAncientStaff() => new()
    {
        ItemId = "weapon_ancient_staff",
        Name = "古代賢者の杖",
        Description = "アルカナス帝国の大賢者が愛用した杖。ルーン語による魔法の威力が大幅に増幅される。その表面には古代ルーン語が刻まれている。",
        WeaponType = WeaponType.Staff,
        BaseDamage = 8,
        DamageRange = (6, 10),
        AttackSpeed = 1.0f,
        Range = 1,
        Rarity = ItemRarity.Epic,
        BasePrice = 1500,
        Weight = 2.5f,
        RequiredLevel = 12,
        StatModifier = new StatModifier(Intelligence: 6, Mind: 4)
    };

    public static Weapon CreateDragonslayerSword() => new()
    {
        ItemId = "weapon_dragonslayer_sword",
        Name = "竜殺しの大剣",
        Description = "太古の英雄が竜王を討伐するために鍛えた伝説の大剣。竜の血を浴びた刃は赤黒く輝き、竜族に対して絶大な威力を発揮する。",
        WeaponType = WeaponType.Greatsword,
        BaseDamage = 35,
        DamageRange = (28, 42),
        AttackSpeed = 0.5f,
        Range = 1,
        IsTwoHanded = true,
        Rarity = ItemRarity.Legendary,
        BasePrice = 5000,
        Weight = 14.0f,
        RequiredLevel = 20,
        RequiredStats = new Stats(Strength: 18, Vitality: 12, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Strength: 7, Vitality: 3)
    };

    public static Weapon CreateShadowDagger() => new()
    {
        ItemId = "weapon_shadow_dagger",
        Name = "影の短剣",
        Description = "闇の教団に伝わる儀式用の短剣。影から影へ渡り歩く暗殺者が愛用したと言われ、使用者の姿を薄暗く揺らがせる効果がある。",
        WeaponType = WeaponType.Dagger,
        BaseDamage = 12,
        DamageRange = (9, 15),
        AttackSpeed = 1.8f,
        Range = 1,
        Rarity = ItemRarity.Epic,
        BasePrice = 1200,
        Weight = 0.7f,
        RequiredLevel = 12,
        StatModifier = new StatModifier(Agility: 5, Dexterity: 4),
        Element = Element.Dark
    };

    public static Weapon CreateHolyMace() => new()
    {
        ItemId = "weapon_holy_mace",
        Name = "聖なる戦槌",
        Description = "光の神殿で祝福を受けた戦槌。アンデッドや悪魔に対して神聖な力を発揮し、振るうたびに淡い光を放つ。",
        WeaponType = WeaponType.Hammer,
        BaseDamage = 20,
        DamageRange = (16, 24),
        AttackSpeed = 0.8f,
        Range = 1,
        Rarity = ItemRarity.Epic,
        BasePrice = 1000,
        Weight = 6.0f,
        RequiredLevel = 10,
        StatModifier = new StatModifier(Strength: 4, Mind: 3),
        Element = Element.Holy
    };

    public static Weapon CreateGalebow() => new()
    {
        ItemId = "weapon_galebow",
        Name = "疾風の弓",
        Description = "風の精霊と契約した弓匠が生涯最後に作り上げた弓。放った矢は風に乗って加速し、通常の弓よりも遥かに速い連射が可能。",
        WeaponType = WeaponType.Bow,
        BaseDamage = 14,
        DamageRange = (10, 18),
        AttackSpeed = 1.5f,
        Range = 8,
        IsTwoHanded = true,
        AttackType = AttackType.Ranged,
        Rarity = ItemRarity.Rare,
        BasePrice = 600,
        Weight = 1.8f,
        RequiredLevel = 8,
        StatModifier = new StatModifier(Dexterity: 4, Agility: 3)
    };

    public static Weapon CreateVolcanicAxe() => new()
    {
        ItemId = "weapon_volcanic_axe",
        Name = "火山の戦斧",
        Description = "火山地帯の溶岩から打ち出された伝説の戦斧。刃は常に赤熱し、斬りつけた対象を炎上させる。",
        WeaponType = WeaponType.Axe,
        BaseDamage = 28,
        DamageRange = (22, 34),
        AttackSpeed = 0.6f,
        Range = 1,
        IsTwoHanded = true,
        Rarity = ItemRarity.Epic,
        BasePrice = 1800,
        Weight = 9.0f,
        RequiredLevel = 15,
        RequiredStats = new Stats(Strength: 16, Vitality: 10, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Strength: 6),
        Element = Element.Fire
    };

    public static Weapon CreateFrostSpear() => new()
    {
        ItemId = "weapon_frost_spear",
        Name = "凍結の槍",
        Description = "凍土の奥深くから発掘された古代の槍。穂先は永遠に凍りつき、突き刺すと対象に深い凍結を与える。",
        WeaponType = WeaponType.Spear,
        BaseDamage = 16,
        DamageRange = (12, 20),
        AttackSpeed = 1.0f,
        Range = 2,
        IsTwoHanded = true,
        AttackType = AttackType.Pierce,
        Rarity = ItemRarity.Rare,
        BasePrice = 700,
        Weight = 4.5f,
        RequiredLevel = 10,
        StatModifier = new StatModifier(Dexterity: 3, Agility: 2),
        Element = Element.Ice
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

    public static Armor CreateMithrilChainmail() => new()
    {
        ItemId = "armor_mithril_chainmail",
        Name = "ミスリル鎖帷子",
        Description = "ミスリル鍛造の鎖帷子。驚くほど軽量でありながら、通常の鋼鉄を遥かに凌ぐ防御力を持つ。エルフの技術で編まれた逸品。",
        ArmorType = ArmorType.Chainmail,
        Slot = EquipmentSlot.Body,
        BaseDefense = 16,
        MagicDefense = 8,
        EvasionModifier = 0.02f,
        SpeedModifier = 0.98f,
        Rarity = ItemRarity.Epic,
        BasePrice = 1200,
        Weight = 8.0f,
        RequiredLevel = 12,
        StatModifier = new StatModifier(Vitality: 4, Agility: 2)
    };

    public static Armor CreateDragonScaleArmor() => new()
    {
        ItemId = "armor_dragon_scale",
        Name = "竜鱗の鎧",
        Description = "竜の鱗を幾重にも重ねて鍛え上げた最高級の鎧。火・氷属性への高い耐性を持ち、その堅牢さは城壁に匹敵するとも言われる。",
        ArmorType = ArmorType.Plate,
        Slot = EquipmentSlot.Body,
        BaseDefense = 25,
        MagicDefense = 12,
        EvasionModifier = -0.05f,
        SpeedModifier = 0.9f,
        Rarity = ItemRarity.Legendary,
        BasePrice = 5000,
        Weight = 20.0f,
        RequiredLevel = 20,
        RequiredStats = new Stats(Strength: 14, Vitality: 12, Agility: 0, Dexterity: 0,
            Intelligence: 0, Mind: 0, Perception: 0, Charisma: 0, Luck: 0),
        StatModifier = new StatModifier(Vitality: 6, Strength: 3)
    };

    public static Shield CreateMithrilShield() => new()
    {
        ItemId = "shield_mithril",
        Name = "ミスリルの盾",
        Description = "ミスリルで鋳造された輝く盾。軽量で扱いやすく、魔法攻撃をも跳ね返す力を持つ。",
        BaseDefense = 10,
        MagicDefense = 5,
        BlockChance = 0.28f,
        BlockReduction = 0.5f,
        Rarity = ItemRarity.Rare,
        BasePrice = 800,
        Weight = 4.0f,
        RequiredLevel = 10,
        StatModifier = new StatModifier(Vitality: 3, Strength: 2)
    };

    public static Armor CreateSageCirclet() => new()
    {
        ItemId = "armor_sage_circlet",
        Name = "賢者の冠",
        Description = "古代の賢者が知識の結晶として鍛えた冠。装着者の魔力を大幅に増幅し、ルーン語の理解を助ける。",
        ArmorType = ArmorType.Plate,
        Slot = EquipmentSlot.Head,
        BaseDefense = 4,
        MagicDefense = 12,
        EvasionModifier = 0.0f,
        SpeedModifier = 1.0f,
        Rarity = ItemRarity.Epic,
        BasePrice = 1500,
        Weight = 1.0f,
        RequiredLevel = 15,
        StatModifier = new StatModifier(Intelligence: 5, Mind: 4)
    };

    public static Armor CreateAssassinGloves() => new()
    {
        ItemId = "armor_assassin_gloves",
        Name = "暗殺者の手袋",
        Description = "闇の教団で使われていた特殊な手袋。指先の感覚を鋭敏にし、鍵開けや罠解除の精度を高める。",
        ArmorType = ArmorType.Leather,
        Slot = EquipmentSlot.Hands,
        BaseDefense = 4,
        MagicDefense = 2,
        EvasionModifier = 0.05f,
        SpeedModifier = 1.02f,
        Rarity = ItemRarity.Rare,
        BasePrice = 500,
        Weight = 0.3f,
        RequiredLevel = 8,
        StatModifier = new StatModifier(Dexterity: 4, Agility: 3)
    };

    public static Armor CreateWingedBoots() => new()
    {
        ItemId = "armor_winged_boots",
        Name = "翼のブーツ",
        Description = "風の魔法が編み込まれた革靴。着用者の歩みを限りなく軽くし、まるで風に乗っているかのような機動性をもたらす。",
        ArmorType = ArmorType.Leather,
        Slot = EquipmentSlot.Feet,
        BaseDefense = 3,
        MagicDefense = 3,
        EvasionModifier = 0.08f,
        SpeedModifier = 1.1f,
        Rarity = ItemRarity.Epic,
        BasePrice = 1200,
        Weight = 1.0f,
        RequiredLevel = 12,
        StatModifier = new StatModifier(Agility: 5, Dexterity: 2)
    };

    public static Accessory CreateDragonboneRing() => new()
    {
        ItemId = "accessory_dragonbone_ring",
        Name = "竜骨の指輪",
        Description = "竜の骨から削り出された指輪。竜の生命力の残滓が宿り、装着者の全身に力がみなぎる。",
        Slot = EquipmentSlot.Ring1,
        Rarity = ItemRarity.Epic,
        BasePrice = 1500,
        Weight = 0.2f,
        RequiredLevel = 15,
        StatModifier = new StatModifier(Strength: 4, Vitality: 4, Intelligence: 2),
        PassiveAbility = "DragonPower"
    };

    public static Accessory CreateHolyAmulet() => new()
    {
        ItemId = "accessory_holy_amulet",
        Name = "聖なるアミュレット",
        Description = "光の神殿の大司祭が祝福した首飾り。聖なる力が宿り、闇属性や呪いへの耐性を高める。持つ者の心を清め、精神攻撃にも強くなる。",
        Slot = EquipmentSlot.Neck,
        Rarity = ItemRarity.Rare,
        BasePrice = 600,
        Weight = 0.2f,
        RequiredLevel = 10,
        StatModifier = new StatModifier(Mind: 4, Vitality: 2, Intelligence: 2),
        PassiveAbility = "HolyProtection"
    };

    #endregion

    #region Predefined Consumables

    public static Potion CreateMinorHealingPotion() => new()
    {
        ItemId = "potion_healing_minor",
        Name = "小回復薬",
        Description = "赤い液体が入った小瓶。薬草を蒸留して作られた基本的な回復薬。傷口を瞬時に塞ぎ、体力を部分的に回復する。",
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
        Description = "鮮やかな赤い液体が入った瓶。高品質な薬草の濃縮エキスで作られた回復薬。体の傷を癒し、消耗した活力を大きく取り戻す。",
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
        Description = "澄んだ青い液体が入った小瓶。魔力水晶を溶かして作られた薬で、失われた魔力をわずかに回復する。",
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
        Description = "苦い黄緑色の液体。毒消し草の汁と鉱物性中和剤の混合物で、体内の毒素を無効化する。冒険者必携の一品。",
        PotionType = PotionType.Antidote,
        EffectValue = 100,  // CN-8: 解毒効果（100%毒解除）
        Rarity = ItemRarity.Common,
        BasePrice = 40,
        Weight = 0.3f
    };

    public static Potion CreateSuperHealingPotion() => new()
    {
        ItemId = "potion_healing_super",
        Name = "超回復薬",
        Description = "深紅色に輝く液体。希少な高地薬草と龍血の結晶から錬成された最高品質の回復薬。瀕死の重傷すら癒す力を持つと言われる。",
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
        Description = "深い青色の液体。高純度の魔力水晶を主成分とした魔力回復薬。魔術師たちが長旅の際に欠かさず持参する一品。",
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
        Description = "どす黒い液体。獣の筋組織から抽出した成分を含み、飲んだ直後は全身に熱が走る。一時的に筋力を著しく高めるが、効果が切れると疲労が残ることがある。",
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
        Description = "透明に近い薄緑の液体。風の精霊の羽根を使って錬成されたとも言われる。飲むと体が軽くなり、思考と動作の速度が向上する。",
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
        Description = "無色透明の液体。光を屈折させる鉱物の粉末を特殊処理した秘薬。飲んだ者の姿を一時的に消し去るが、強い動作をすると効果が乱れることがある。",
        PotionType = PotionType.Invisibility,
        EffectValue = 1,  // X-1: 透明化フラグ（1=有効）
        Duration = 20,
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.5f
    };

    public static Potion CreateFireResistPotion() => new()
    {
        ItemId = "potion_fire_resist",
        Name = "耐火薬",
        Description = "燃えるような橙色の液体。サラマンダーの鱗を粉砕した素材が入っており、飲むと肌が一時的に熱に強くなる。炎系の魔物との戦闘前に推奨される。",
        PotionType = PotionType.FireResistance,
        EffectValue = 50,  // X-3: 耐性50%
        Duration = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateColdResistPotion() => new()
    {
        ItemId = "potion_cold_resist",
        Name = "耐冷薬",
        Description = "涼しげな水色の液体。氷の精霊の結晶を溶かして作られ、飲むと体が冷気に適応する。北方の氷雪地帯を探索する際には必需品。",
        PotionType = PotionType.ColdResistance,
        EffectValue = 50,  // X-4: 耐性50%
        Duration = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateCureAllPotion() => new()
    {
        ItemId = "potion_cure_all",
        Name = "万能薬",
        Description = "虹色に輝く幻想的な液体。七種の珍しい薬草と三種の魔法鉱物から錬成される至高の薬。あらゆる状態異常を解除するとされ、市場では滅多に見かけない。",
        PotionType = PotionType.CureAll,
        EffectValue = 100,  // X-2: 全状態異常解除（100%効果）
        Rarity = ItemRarity.Rare,
        BasePrice = 300,
        Weight = 0.5f
    };

    // B-7: 知力増強薬ファクトリ追加
    public static Potion CreateIntelligenceBoostPotion() => new()
    {
        ItemId = "potion_intelligence_boost",
        Name = "知力増強薬",
        Description = "一時的に知力を上昇させる薬。",
        PotionType = PotionType.IntelligenceBoost,
        EffectValue = 5,
        Duration = 30,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.5f
    };

    // B-11: 毒薬ファクトリ追加
    public static Potion CreatePoisonPotion() => new()
    {
        ItemId = "potion_poison",
        Name = "毒薬",
        Description = "飲むと毒に侵される危険な薬。武器に塗ることも可能。",
        PotionType = PotionType.Poison,
        EffectValue = 10,
        Duration = 10,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 50,
        Weight = 0.5f
    };

    // B-12: 混乱薬ファクトリ追加
    public static Potion CreateConfusionPotion() => new()
    {
        ItemId = "potion_confusion",
        Name = "混乱薬",
        Description = "飲むと方向感覚を失い混乱する薬。",
        PotionType = PotionType.Confusion,
        EffectValue = 1,
        Duration = 10,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 40,
        Weight = 0.5f
    };

    public static Potion CreateStaminaPotionMinor() => new()
    {
        ItemId = "potion_stamina_minor",
        Name = "小スタミナ薬",
        Description = "草色の液体。筋肉の疲労を和らげる成分が含まれ、少量の体力を回復する。冒険者が常備する基本的な回復薬。",
        PotionType = PotionType.StaminaMinor,
        EffectValue = 15,
        Rarity = ItemRarity.Common,
        BasePrice = 20,
        Weight = 0.5f
    };

    public static Potion CreateStaminaPotion() => new()
    {
        ItemId = "potion_stamina",
        Name = "スタミナ薬",
        Description = "深い緑色の液体。高濃度の活力成分を含み、疲弊した筋肉を急速に回復させる。長期戦で重宝される。",
        PotionType = PotionType.StaminaMajor,
        EffectValue = 40,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 60,
        Weight = 0.5f
    };

    public static Potion CreateLightningResistPotion() => new()
    {
        ItemId = "potion_lightning_resist",
        Name = "耐雷薬",
        Description = "黄金色に帯電する液体。雷精霊の毛を煎じて作られ、飲むと全身に静電気への耐性が生まれる。雷系の魔物と対峙する前に推奨。",
        PotionType = PotionType.FireResistance,
        EffectValue = 50,
        Duration = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 80,
        Weight = 0.5f
    };

    public static Potion CreateBerserkerPotion() => new()
    {
        ItemId = "potion_berserker",
        Name = "狂戦士の薬",
        Description = "血のように赤い液体。飲むと理性の一部を失う代わりに攻撃力が爆発的に上昇する。効果切れ後の反動に注意が必要。",
        PotionType = PotionType.StrengthBoost,
        EffectValue = 10,
        Duration = 20,
        Rarity = ItemRarity.Rare,
        BasePrice = 150,
        Weight = 0.5f
    };

    public static Potion CreateVitalityPotion() => new()
    {
        ItemId = "potion_vitality",
        Name = "生命力増強薬",
        Description = "琥珀色の粘性ある液体。大地の精霊から抽出した生命エネルギーが含まれ、一時的に生命力を大幅に高める。",
        PotionType = PotionType.StrengthBoost,
        EffectValue = 5,
        Duration = 30,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
        Weight = 0.5f
    };

    public static Potion CreateLiquidLuck() => new()
    {
        ItemId = "potion_liquid_luck",
        Name = "幸運の霊薬",
        Description = "虹色に揺れる幻想的な液体。七色の花の蜜と星の破片から錬成された至高の霊薬。飲んだ者にありとあらゆる幸運をもたらすと言われる。",
        PotionType = PotionType.AgilityBoost,
        EffectValue = 3,
        Duration = 50,
        Rarity = ItemRarity.Epic,
        BasePrice = 500,
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

    public static Food CreateDesertDateFruit() => new()
    {
        ItemId = "food_desert_date",
        Name = "砂漠のナツメヤシ",
        Description = "砂漠地帯で栽培される甘い果実。渇きと飢えを同時に癒す。砂漠の民の主食であり、ハッサンが仕入れる交易品の一つ。",
        FoodType = FoodType.Fruit,
        NutritionValue = 35,
        HealValue = 5,
        Rarity = ItemRarity.Common,
        BasePrice = 12,
        Weight = 0.2f
    };

    public static Food CreateTundraJerky() => new()
    {
        ItemId = "food_tundra_jerky",
        Name = "凍土の干し肉",
        Description = "凍土で狩った獣の肉を干して保存したもの。噛めば噛むほど味が出る。厳しい寒さの中でも凍らず、栄養価が非常に高い。",
        FoodType = FoodType.CookedMeat,
        NutritionValue = 70,
        HealValue = 15,
        IsCooked = true,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 30,
        Weight = 0.4f
    };

    public static Food CreateSacredBerry() => new()
    {
        ItemId = "food_sacred_berry",
        Name = "聖域の木の実",
        Description = "聖域にのみ自生する輝く木の実。食べると心身ともに清められ、体力と精神力の両方が回復する。自然崇拝の信者にとって神聖な食物。",
        FoodType = FoodType.Lembas,
        NutritionValue = 60,
        HealValue = 30,
        Rarity = ItemRarity.Rare,
        BasePrice = 80,
        Weight = 0.2f
    };

    public static Food CreateVolcanicSpice() => new()
    {
        ItemId = "food_volcanic_spice",
        Name = "火山香辛料",
        Description = "火山地帯に自生する激辛の香辛料。そのまま食べると口が焼けるほど辛いが、体が温まり、一時的に耐火効果を得られるという。",
        FoodType = FoodType.Bread,
        NutritionValue = 15,
        HealValue = 0,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 25,
        Weight = 0.1f
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

    // B-3: 召喚の巻物ファクトリ追加
    public static Scroll CreateScrollOfSummon() => new()
    {
        ItemId = "scroll_summon",
        Name = "召喚の巻物",
        Description = "味方のクリーチャーを一体召喚する。",
        ScrollType = ScrollType.Summon,
        TargetType = TargetType.Self,
        EffectValue = 1,
        Rarity = ItemRarity.Rare,
        BasePrice = 250,
        Weight = 0.1f
    };

    // B-4: 混乱の巻物
    public static Scroll CreateScrollOfConfusion() => new()
    {
        ItemId = "scroll_confusion",
        Name = "混乱の巻物",
        Description = "読むと周囲の敵を混乱させる霧を発生させる。",
        ScrollType = ScrollType.Confusion,
        TargetType = TargetType.Area,
        EffectValue = 10,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 120,
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

    public static Scroll CreateScrollOfHaste() => new()
    {
        ItemId = "scroll_haste",
        Name = "加速の巻物",
        Description = "読むと一時的に時間の流れが遅くなったように感じ、行動速度が大幅に上昇する。",
        ScrollType = ScrollType.Teleport,
        TargetType = TargetType.Self,
        EffectValue = 30,
        Rarity = ItemRarity.Rare,
        BasePrice = 180,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfEarthquake() => new()
    {
        ItemId = "scroll_earthquake",
        Name = "地震の巻物",
        Description = "読むと大地が激しく揺れ、周囲の全ての存在にダメージを与える。",
        ScrollType = ScrollType.Fireball,
        TargetType = TargetType.Area,
        EffectRadius = 5,
        EffectValue = 60,
        Rarity = ItemRarity.Epic,
        BasePrice = 300,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfHolyWard() => new()
    {
        ItemId = "scroll_holy_ward",
        Name = "聖なる結界の巻物",
        Description = "読むと聖なる光の結界が展開され、アンデッドと悪魔を寄せ付けない。",
        ScrollType = ScrollType.Sanctuary,
        TargetType = TargetType.Self,
        EffectRadius = 3,
        Rarity = ItemRarity.Rare,
        BasePrice = 250,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfCurse() => new()
    {
        ItemId = "scroll_curse",
        Name = "呪いの巻物",
        Description = "禁忌の呪文が記された巻物。対象に強力な呪いをかけ、全ステータスを低下させる。",
        ScrollType = ScrollType.Confusion,
        TargetType = TargetType.SingleEnemy,
        EffectValue = 20,
        Rarity = ItemRarity.Rare,
        BasePrice = 200,
        Weight = 0.1f
    };

    public static Scroll CreateScrollOfDivination() => new()
    {
        ItemId = "scroll_divination",
        Name = "千里眼の巻物",
        Description = "読むと意識が拡大し、階層全体の地形と敵の位置が把握できる。",
        ScrollType = ScrollType.MagicMapping,
        TargetType = TargetType.Self,
        EffectValue = 50,
        Rarity = ItemRarity.Uncommon,
        BasePrice = 100,
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
        var deepWeapons = new Func<Item>[] { CreateSteelSword, CreateGreatsword, CreateWarHammer, CreateMithrilDagger, CreatePlateArmor, CreateWhip, CreateIronHelm, CreateMithrilSword, CreateAncientStaff, CreateHolyMace, CreateGalebow, CreateFrostSpear, CreateShadowDagger };

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
            CreateScrollOfRemoveCurse,
            CreateIntelligenceBoostPotion,  // B-7
            CreatePoisonPotion,  // B-11
            CreateConfusionPotion,  // B-12
            CreateStaminaPotionMinor,
            CreateStaminaPotion,
            CreateVitalityPotion
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
            CreateScrollOfEnchant,
            CreateBerserkerPotion,
            CreateLightningResistPotion,
            CreateLiquidLuck,
            CreateScrollOfHaste,
            CreateScrollOfDivination
        };
        var epicPool = new Func<Item>[] {
            CreateScrollOfEarthquake,
            CreateScrollOfHolyWard,
            CreateScrollOfCurse
        };

        Func<Item>[] pool;
        if (rarity >= ItemRarity.Epic)
            pool = commonPool.Concat(uncommonPool).Concat(rarePool).Concat(epicPool).ToArray();
        else if (rarity >= ItemRarity.Rare)
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
        int foodType = _random.Next(12);

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
            8 => CreateDesertDateFruit(),
            9 => CreateTundraJerky(),
            10 => CreateSacredBerry(),
            11 => CreateVolcanicSpice(),
            _ => CreateBread()
        };
    }

    private Item GenerateRandomScroll(ItemRarity rarity)
    {
        var scrollTypes = new[] { ScrollType.Teleport, ScrollType.Identify, ScrollType.MagicMapping,
            ScrollType.Enchant, ScrollType.Freeze, ScrollType.Sanctuary, ScrollType.Return, ScrollType.Summon };
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

    // 追加素材
    public static Item CreateSacredWater() => CreateMaterialItem("material_sacred_water", "聖水", "光の神殿で祝福された水。浄化や聖なる調合に使用される", MaterialCategory.Magical, 80, ItemRarity.Uncommon, 60);
    public static Item CreateChaosFragment() => CreateMaterialItem("material_chaos_fragment", "混沌の欠片", "混沌の力が結晶化した不安定な素材。形状が常に変化し続ける", MaterialCategory.Magical, 150, ItemRarity.Rare, 70);
    public static Item CreateWorldTreeLeaf() => CreateMaterialItem("material_world_tree_leaf", "世界樹の葉", "世界樹から落ちた一枚の葉。強大な自然の力が宿る", MaterialCategory.Herb, 200, ItemRarity.Epic, 85);
    public static Item CreateDeathEssence() => CreateMaterialItem("material_death_essence", "死のエッセンス", "死神タナトスの領域に近い場所で採取される暗黒の精髄", MaterialCategory.Magical, 180, ItemRarity.Rare, 75);
    public static Item CreateVolcanicite() => CreateMaterialItem("material_volcanite", "火山鉱", "火山の溶岩から生成される希少鉱石。鍛冶に使うと炎属性を武器に付与できる", MaterialCategory.Metal, 120, ItemRarity.Rare, 65);
    public static Item CreateFrozenCrystal() => CreateMaterialItem("material_frozen_crystal", "永久凍結晶", "凍土の最深部で採取される決して溶けない結晶。氷属性付与の素材", MaterialCategory.Gem, 130, ItemRarity.Rare, 65);

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
        ["weapon_mithril_sword"] = ItemFactory.CreateMithrilSword,
        ["weapon_ancient_staff"] = ItemFactory.CreateAncientStaff,
        ["weapon_dragonslayer_sword"] = ItemFactory.CreateDragonslayerSword,
        ["weapon_shadow_dagger"] = ItemFactory.CreateShadowDagger,
        ["weapon_holy_mace"] = ItemFactory.CreateHolyMace,
        ["weapon_galebow"] = ItemFactory.CreateGalebow,
        ["weapon_volcanic_axe"] = ItemFactory.CreateVolcanicAxe,
        ["weapon_frost_spear"] = ItemFactory.CreateFrostSpear,

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
        ["armor_mithril_chainmail"] = ItemFactory.CreateMithrilChainmail,
        ["armor_dragon_scale"] = ItemFactory.CreateDragonScaleArmor,
        ["shield_mithril"] = ItemFactory.CreateMithrilShield,
        ["armor_sage_circlet"] = ItemFactory.CreateSageCirclet,
        ["armor_assassin_gloves"] = ItemFactory.CreateAssassinGloves,
        ["armor_winged_boots"] = ItemFactory.CreateWingedBoots,

        // アクセサリ
        ["accessory_iron_ring"] = ItemFactory.CreateIronRing,
        ["accessory_protection_amulet"] = ItemFactory.CreateProtectionAmulet,
        ["accessory_speed_cloak"] = ItemFactory.CreateSpeedCloak,
        ["accessory_dragonbone_ring"] = ItemFactory.CreateDragonboneRing,
        ["accessory_holy_amulet"] = ItemFactory.CreateHolyAmulet,

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
        ["potion_intelligence_boost"] = ItemFactory.CreateIntelligenceBoostPotion,  // B-7
        ["potion_poison"] = ItemFactory.CreatePoisonPotion,  // B-11
        ["potion_confusion"] = ItemFactory.CreateConfusionPotion,  // B-12
        ["potion_stamina_minor"] = ItemFactory.CreateStaminaPotionMinor,
        ["potion_stamina"] = ItemFactory.CreateStaminaPotion,
        ["potion_lightning_resist"] = ItemFactory.CreateLightningResistPotion,
        ["potion_berserker"] = ItemFactory.CreateBerserkerPotion,
        ["potion_vitality"] = ItemFactory.CreateVitalityPotion,
        ["potion_liquid_luck"] = ItemFactory.CreateLiquidLuck,

        // 食料
        ["food_bread"] = ItemFactory.CreateBread,
        ["food_ration"] = ItemFactory.CreateRation,
        ["food_cooked_meat"] = ItemFactory.CreateCookedMeat,
        ["food_emergency_ration"] = ItemFactory.CreateEmergencyRation,
        ["food_lembas"] = ItemFactory.CreateLembas,
        ["food_fruit"] = ItemFactory.CreateFruit,
        ["food_water"] = ItemFactory.CreateWater,
        ["food_clean_water"] = ItemFactory.CreateCleanWater,
        ["food_desert_date"] = ItemFactory.CreateDesertDateFruit,
        ["food_tundra_jerky"] = ItemFactory.CreateTundraJerky,
        ["food_sacred_berry"] = ItemFactory.CreateSacredBerry,
        ["food_volcanic_spice"] = ItemFactory.CreateVolcanicSpice,

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
        ["scroll_summon"] = ItemFactory.CreateScrollOfSummon,  // B-3
        ["scroll_confusion"] = ItemFactory.CreateScrollOfConfusion,  // B-4
        ["ancient_book"] = ItemFactory.CreateAncientBook,
        ["scroll_haste"] = ItemFactory.CreateScrollOfHaste,
        ["scroll_earthquake"] = ItemFactory.CreateScrollOfEarthquake,
        ["scroll_holy_ward"] = ItemFactory.CreateScrollOfHolyWard,
        ["scroll_curse"] = ItemFactory.CreateScrollOfCurse,
        ["scroll_divination"] = ItemFactory.CreateScrollOfDivination,

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
        ["gem_rough"] = ItemFactory.CreateGemRough,

        // 追加素材
        ["material_sacred_water"] = ItemFactory.CreateSacredWater,
        ["material_chaos_fragment"] = ItemFactory.CreateChaosFragment,
        ["material_world_tree_leaf"] = ItemFactory.CreateWorldTreeLeaf,
        ["material_death_essence"] = ItemFactory.CreateDeathEssence,
        ["material_volcanite"] = ItemFactory.CreateVolcanicite,
        ["material_frozen_crystal"] = ItemFactory.CreateFrozenCrystal
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
