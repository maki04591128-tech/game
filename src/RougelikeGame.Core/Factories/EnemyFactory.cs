using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Factories;

/// <summary>
/// 敵キャラクターを生成するファクトリ
/// </summary>
public class EnemyFactory
{
    /// <summary>CD-5: 難易度による敵ステータス倍率</summary>
    public double DifficultyStatMultiplier { get; set; } = 1.0;

    /// <summary>
    /// 敵を生成
    /// </summary>
    public Enemy CreateEnemy(EnemyDefinition definition, Position position)
    {
        return CreateEnemy(definition, position, null);
    }

    /// <summary>
    /// 敵を生成（階層補正付き）
    /// </summary>
    public Enemy CreateEnemy(EnemyDefinition definition, Position position, StatModifier? floorBonus)
    {
        var stats = definition.BaseStats;
        if (floorBonus.HasValue)
        {
            stats = stats.Apply(floorBonus.Value);
        }

        // CD-5: 難易度によるステータススケーリング
        if (DifficultyStatMultiplier != 1.0)
        {
            double m = DifficultyStatMultiplier;
            stats = new Stats(
                (int)(stats.Strength * m),
                (int)(stats.Vitality * m),
                (int)(stats.Agility * m),
                (int)(stats.Dexterity * m),
                (int)(stats.Intelligence * m),
                (int)(stats.Mind * m),
                (int)(stats.Perception * m),
                (int)(stats.Charisma * m),
                (int)(stats.Luck * m)
            );
        }

        var enemy = new Enemy
        {
            Name = definition.Name,
            EnemyTypeId = definition.TypeId,
            Description = definition.Description,
            BaseStats = stats,
            ExperienceReward = definition.ExperienceReward,
            DropTableId = definition.DropTableId,
            Rank = definition.Rank,
            SightRange = definition.SightRange,
            HearingRange = definition.HearingRange,
            GiveUpDistance = definition.GiveUpDistance,
            FleeThreshold = definition.FleeThreshold,
            Race = definition.Race,
            WeaponType = definition.WeaponType,
            Position = position,
            HomePosition = position,
            Faction = Faction.Enemy
        };

        enemy.InitializeResources();

        // AIビヘイビアを設定（EnemyType + MonsterRace）
        var behavior = CreateBehaviorFor(definition.EnemyType);
        AddRacialBehavior(behavior, definition.Race);
        enemy.Behavior = behavior;

        return enemy;
    }

    /// <summary>
    /// 敵タイプに応じたビヘイビアを作成
    /// </summary>
    private CompositeBehavior CreateBehaviorFor(EnemyType type)
    {
        var composite = new CompositeBehavior();

        switch (type)
        {
            case EnemyType.Normal:
                composite.AddBehavior(new FleeBehavior());
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.1f));
                break;

            case EnemyType.Aggressive:
                composite.AddBehavior(new AggressiveBehavior(15));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.2f));
                break;

            case EnemyType.Defensive:
                composite.AddBehavior(new FleeBehavior());
                composite.AddBehavior(new DefensiveBehavior(6));
                composite.AddBehavior(new IdleBehavior(0.05f));
                break;

            case EnemyType.Coward:
                composite.AddBehavior(new FleeBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.15f));
                break;

            case EnemyType.Ambusher:
                composite.AddBehavior(new AmbushBehavior(4));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new IdleBehavior(0.0f));  // 動かない
                break;

            case EnemyType.Pack:
                composite.AddBehavior(new AggressiveBehavior(10));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.1f));
                break;

            case EnemyType.Boss:
                composite.AddBehavior(new BerserkerBehavior(0.3f));
                composite.AddBehavior(new AggressiveBehavior(20));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new DefensiveBehavior(10));
                composite.AddBehavior(new IdleBehavior(0.0f));
                break;

            // AZ-2: 召喚者型（SummonerBehaviorを使用）
            case EnemyType.Summoner:
                composite.AddBehavior(new SummonerBehavior());
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.1f));
                break;

            case EnemyType.Berserker:
                composite.AddBehavior(new BerserkerBehavior(0.4f));
                composite.AddBehavior(new AggressiveBehavior(15));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.0f));
                break;

            case EnemyType.Guardian:
                composite.AddBehavior(new DefensiveBehavior(8));
                composite.AddBehavior(new AggressiveBehavior(10));
                composite.AddBehavior(new ChaseBehavior());
                composite.AddBehavior(new IdleBehavior(0.0f));
                break;

            case EnemyType.Ranged:
                composite.AddBehavior(new RangedBehavior(6));
                composite.AddBehavior(new FleeBehavior());
                composite.AddBehavior(new AlertBehavior());
                composite.AddBehavior(new PatrolBehavior());
                composite.AddBehavior(new IdleBehavior(0.1f));
                break;

            default:
                composite.AddBehavior(new IdleBehavior());
                break;
        }

        return composite;
    }

    /// <summary>
    /// 種族に応じた特殊ビヘイビアを追加
    /// </summary>
    private void AddRacialBehavior(CompositeBehavior composite, MonsterRace race)
    {
        switch (race)
        {
            case MonsterRace.Beast:
                composite.AddBehavior(new PackHuntingBehavior());
                break;
            case MonsterRace.Undead:
                composite.AddBehavior(new UndeadBehavior());
                break;
            case MonsterRace.Amorphous:
                composite.AddBehavior(new AmorphousBehavior());
                break;
            case MonsterRace.Construct:
                composite.AddBehavior(new ConstructBehavior());
                break;
            case MonsterRace.Dragon:
                composite.AddBehavior(new DragonBehavior());
                break;
            case MonsterRace.Spirit:
                composite.AddBehavior(new SpiritBehavior());
                break;
        }
    }

    #region Predefined Enemies

    /// <summary>
    /// スライムを生成
    /// </summary>
    public Enemy CreateSlime(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Slime, position);
    }

    /// <summary>
    /// ゴブリンを生成
    /// </summary>
    public Enemy CreateGoblin(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Goblin, position);
    }

    /// <summary>
    /// スケルトンを生成
    /// </summary>
    public Enemy CreateSkeleton(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Skeleton, position);
    }

    /// <summary>
    /// オークを生成
    /// </summary>
    public Enemy CreateOrc(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Orc, position);
    }

    /// <summary>
    /// 大蜘蛛を生成
    /// </summary>
    public Enemy CreateGiantSpider(Position position)
    {
        return CreateEnemy(EnemyDefinitions.GiantSpider, position);
    }

    /// <summary>
    /// ダークエルフを生成
    /// </summary>
    public Enemy CreateDarkElf(Position position)
    {
        return CreateEnemy(EnemyDefinitions.DarkElf, position);
    }

    /// <summary>
    /// トロールを生成
    /// </summary>
    public Enemy CreateTroll(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Troll, position);
    }

    /// <summary>
    /// ドラウグル（亡者）を生成
    /// </summary>
    public Enemy CreateDraugr(Position position)
    {
        return CreateEnemy(EnemyDefinitions.Draugr, position);
    }

    #endregion
}

/// <summary>
/// 敵の定義データ
/// </summary>
public record EnemyDefinition(
    string TypeId,
    string Name,
    string Description,
    Stats BaseStats,
    EnemyType EnemyType,
    EnemyRank Rank,
    int ExperienceReward,
    string? DropTableId = null,
    int SightRange = 8,
    int HearingRange = 5,
    int GiveUpDistance = 15,
    float FleeThreshold = 0.2f,
    MonsterRace Race = MonsterRace.Humanoid,
    WeaponType? WeaponType = null
);

/// <summary>
/// 定義済み敵データ
/// </summary>
public static class EnemyDefinitions
{
    public static readonly EnemyDefinition Rat = new(
        TypeId: "enemy_rat",
        Name: "ドブネズミ",
        Description: "地下に巣食う大型のネズミ。群れで襲いかかる。",
        BaseStats: new Stats(2, 3, 8, 6, 1, 1, 4, 1, 5),
        EnemyType: EnemyType.Pack,
        Rank: EnemyRank.Common,
        ExperienceReward: 3,
        DropTableId: "drop_slime",
        SightRange: 6,
        HearingRange: 8,
        FleeThreshold: 0.4f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Bandit = new(
        TypeId: "enemy_bandit",
        Name: "山賊",
        Description: "山道で旅人を襲う無法者。武器の扱いに長ける。",
        BaseStats: new Stats(9, 8, 7, 8, 3, 5, 6, 2, 4),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
        DropTableId: "drop_goblin",
        SightRange: 9,
        HearingRange: 7,
        FleeThreshold: 0.25f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Sword
    );

    public static readonly EnemyDefinition Slime = new(
        TypeId: "slime",
        Name: "スライム",
        Description: "最も弱い魔物。ゆっくりと這い回る。",
        BaseStats: new Stats(3, 5, 2, 2, 1, 2, 3, 1, 5),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Common,
        ExperienceReward: 5,
        DropTableId: "drop_slime",
        SightRange: 5,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Amorphous
    );

    public static readonly EnemyDefinition Goblin = new(
        TypeId: "goblin",
        Name: "ゴブリン",
        Description: "狡猾な小鬼。群れで行動することが多い。",
        BaseStats: new Stats(6, 5, 8, 7, 4, 3, 6, 2, 6),
        EnemyType: EnemyType.Pack,
        Rank: EnemyRank.Common,
        ExperienceReward: 15,
        DropTableId: "drop_goblin",
        SightRange: 8,
        HearingRange: 6,
        FleeThreshold: 0.3f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Dagger
    );

    public static readonly EnemyDefinition Skeleton = new(
        TypeId: "skeleton",
        Name: "スケルトン",
        Description: "蘇った骸骨の戦士。恐れを知らない。",
        BaseStats: new Stats(8, 6, 6, 6, 2, 8, 4, 1, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 20,
        DropTableId: "drop_skeleton",
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead,
        WeaponType: Items.WeaponType.Sword
    );

    public static readonly EnemyDefinition Orc = new(
        TypeId: "orc",
        Name: "オーク",
        Description: "凶暴な豚鬼。強靭な肉体を持つ。",
        BaseStats: new Stats(12, 10, 5, 5, 3, 5, 5, 2, 4),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,
        DropTableId: "drop_orc",
        SightRange: 7,
        GiveUpDistance: 20,
        FleeThreshold: 0.15f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Axe
    );

    public static readonly EnemyDefinition GiantSpider = new(
        TypeId: "giant_spider",
        Name: "大蜘蛛",
        Description: "暗闘に潜む巨大な蜘蛛。毒を持つ。",
        BaseStats: new Stats(7, 6, 10, 9, 2, 4, 12, 1, 5),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
        DropTableId: "drop_spider",
        SightRange: 6,
        HearingRange: 10,
        Race: MonsterRace.Insect
    );

    public static readonly EnemyDefinition DarkElf = new(
        TypeId: "dark_elf",
        Name: "ダークエルフ",
        Description: "地下に住む邪悪なエルフ。魔法を操る。",
        BaseStats: new Stats(7, 6, 10, 10, 12, 10, 10, 5, 7),
        EnemyType: EnemyType.Summoner,  // AZ-2: 召喚者型を使用
        Rank: EnemyRank.Elite,
        ExperienceReward: 50,
        DropTableId: "drop_dark_elf",
        SightRange: 12,
        FleeThreshold: 0.25f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Sword
    );

    public static readonly EnemyDefinition Troll = new(
        TypeId: "troll",
        Name: "トロール",
        Description: "巨大で凶暴な怪物。再生能力を持つ。",
        BaseStats: new Stats(18, 20, 4, 4, 2, 6, 4, 1, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 80,
        DropTableId: "drop_troll",
        SightRange: 6,
        GiveUpDistance: 25,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Hammer
    );

    public static readonly EnemyDefinition Draugr = new(
        TypeId: "draugr",
        Name: "ドラウグル",
        Description: "北方の墳墓から蘇った亡者。古の武具を纏う。",
        BaseStats: new Stats(14, 12, 6, 8, 5, 12, 6, 1, 2),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 60,
        DropTableId: "drop_draugr",
        SightRange: 8,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead,
        WeaponType: Items.WeaponType.Greatsword
    );

    #region Territory-Specific Enemies

    // === 森林領域 ===
    public static readonly EnemyDefinition ForestWolf = new(
        TypeId: "forest_wolf",
        Name: "森狼",
        Description: "森に棲む凶暴な狼。群れで獲物を追い詰める。",
        BaseStats: new Stats(8, 7, 12, 10, 2, 4, 10, 1, 5),
        EnemyType: EnemyType.Pack,
        Rank: EnemyRank.Common,
        ExperienceReward: 20,
        DropTableId: "drop_wolf",
        SightRange: 10,
        HearingRange: 12,
        FleeThreshold: 0.25f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Treant = new(
        TypeId: "treant",
        Name: "トレント",
        Description: "動く古木。森の番人として侵入者を排除する。",
        BaseStats: new Stats(15, 25, 2, 2, 4, 15, 3, 1, 2),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 55,
        DropTableId: "drop_treant",
        SightRange: 6,
        GiveUpDistance: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Plant
    );

    public static readonly EnemyDefinition ForestSprite = new(
        TypeId: "forest_sprite",
        Name: "森の精霊",
        Description: "森に宿る精霊。魔法で攻撃してくる。",
        BaseStats: new Stats(4, 4, 14, 12, 9, 10, 14, 3, 8),
        EnemyType: EnemyType.Coward,
        Rank: EnemyRank.Common,
        ExperienceReward: 40,  // D-4: ステータス相応に経験値引上げ
        DropTableId: "drop_sprite",
        SightRange: 12,
        FleeThreshold: 0.4f,
        Race: MonsterRace.Spirit
    );

    // === 山岳領域 ===
    public static readonly EnemyDefinition MountainGolem = new(
        TypeId: "mountain_golem",
        Name: "ストーンゴーレム",
        Description: "岩で構成された巨大な人型。非常に頑丈。",
        BaseStats: new Stats(20, 30, 2, 2, 1, 20, 2, 1, 1),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 70,
        DropTableId: "drop_golem",
        SightRange: 5,
        GiveUpDistance: 8,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Construct
    );

    public static readonly EnemyDefinition Harpy = new(
        TypeId: "harpy",
        Name: "ハーピー",
        Description: "山岳に棲む鳥人。素早い動きで翻弄する。",
        BaseStats: new Stats(6, 5, 14, 12, 8, 3, 12, 4, 6),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,
        DropTableId: "drop_harpy",
        SightRange: 14,
        FleeThreshold: 0.3f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Wyvern = new(
        TypeId: "wyvern",
        Name: "ワイバーン",
        Description: "小型の飛竜。毒を持つ尾で攻撃する。",
        BaseStats: new Stats(16, 14, 10, 8, 6, 10, 8, 2, 4),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 75,
        DropTableId: "drop_wyvern",
        SightRange: 15,
        GiveUpDistance: 25,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Dragon
    );

    // === 沿岸領域 ===
    public static readonly EnemyDefinition SeaSerpent = new(
        TypeId: "sea_serpent",
        Name: "海蛇",
        Description: "沿岸の洞窟に潜む巨大な蛇。",
        BaseStats: new Stats(12, 10, 10, 8, 4, 6, 8, 1, 5),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 35,
        DropTableId: "drop_serpent",
        SightRange: 6,
        HearingRange: 12,
        FleeThreshold: 0.15f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Siren = new(
        TypeId: "siren",
        Name: "セイレーン",
        Description: "美しい歌声で冒険者を惑わす海の魔女。",
        BaseStats: new Stats(5, 5, 8, 10, 16, 12, 10, 14, 7),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 60,
        DropTableId: "drop_siren",
        SightRange: 10,
        FleeThreshold: 0.2f,
        Race: MonsterRace.Spirit
    );

    public static readonly EnemyDefinition Crab = new(
        TypeId: "crab",
        Name: "巨大蟹",
        Description: "巨大な甲殻を持つ蟹。挟む力は凄まじい。",
        BaseStats: new Stats(10, 16, 4, 6, 1, 10, 3, 1, 3),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,  // D-5: VIT16のCommonとして経験値引上げ
        DropTableId: "drop_crab",
        SightRange: 5,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Insect
    );

    // === 南方領域 ===
    public static readonly EnemyDefinition DesertScorpion = new(
        TypeId: "desert_scorpion",
        Name: "砂漠蠍",
        Description: "灼熱の砂漠に棲む巨大な蠍。猛毒を持つ。",
        BaseStats: new Stats(9, 8, 10, 8, 2, 12, 8, 1, 4),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,
        DropTableId: "drop_scorpion",
        SightRange: 7,
        HearingRange: 10,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Insect
    );

    public static readonly EnemyDefinition Mummy = new(
        TypeId: "mummy",
        Name: "ミイラ",
        Description: "古代の呪いで蘇った死者。触れると呪われる。",
        BaseStats: new Stats(12, 14, 4, 4, 8, 14, 3, 1, 2),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 55,
        DropTableId: "drop_mummy",
        SightRange: 6,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead
    );

    public static readonly EnemyDefinition Sandworm = new(
        TypeId: "sandworm",
        Name: "サンドワーム",
        Description: "砂の中を泳ぐ巨大な蟲。地響きと共に現れる。",
        BaseStats: new Stats(18, 20, 6, 4, 1, 10, 4, 1, 2),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Elite,
        ExperienceReward: 65,
        DropTableId: "drop_sandworm",
        SightRange: 3,
        HearingRange: 15,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Beast
    );

    // === 沼地領域 ===
    public static readonly EnemyDefinition SwampLizard = new(
        TypeId: "swamp_lizard",
        Name: "沼地蜥蜴",
        Description: "湿地帯に棲む巨大な蜥蜴。鱗は水を弾き、尾の一撃は成人の骨を砕くほどの威力を持つ。沼の泥に紛れて獲物を待ち伏せする狡猾なハンター。",
        BaseStats: new Stats(8, 9, 7, 6, 3, 4, 5, 1, 4),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Common,
        ExperienceReward: 20,
        DropTableId: "drop_slime",
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition SwampToad = new(
        TypeId: "swamp_toad",
        Name: "毒蛙",
        Description: "猛毒を持つ巨大な蛙。その毒は接触するだけで皮膚を焼き、吸い込めば意識を失う。鳴き声で仲間を呼ぶ習性がある。",
        BaseStats: new Stats(5, 8, 4, 3, 2, 5, 8, 1, 5),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 15,
        DropTableId: "drop_spider",
        SightRange: 5,
        HearingRange: 10,
        Race: MonsterRace.Insect
    );

    public static readonly EnemyDefinition SwampWitch = new(
        TypeId: "swamp_witch",
        Name: "沼の魔女",
        Description: "沼地の奥深くに棲む孤独な魔女。呪術と沼の瘴気を操り、アンデッドの従僕を使役する。かつては名のある魔術師だったという噂もある。",
        BaseStats: new Stats(5, 6, 6, 7, 14, 12, 10, 8, 6),
        EnemyType: EnemyType.Summoner,
        Rank: EnemyRank.Elite,
        ExperienceReward: 55,
        DropTableId: "drop_dark_elf",
        FleeThreshold: 0.3f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Staff
    );

    public static readonly EnemyDefinition SwampLord = new(
        TypeId: "boss_swamp_lord",
        Name: "沼地の主",
        Description: "太古より沼地の最深部に君臨する古竜の末裔。全身が黒い鱗に覆われ、毒の霧を纏い、水中から不意に現れて獲物を丸呑みにする。",
        BaseStats: new Stats(22, 25, 10, 8, 12, 18, 14, 5, 8),
        EnemyType: EnemyType.Berserker,
        Rank: EnemyRank.Boss,
        ExperienceReward: 500,
        DropTableId: "drop_dark_elf",
        SightRange: 12,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    // === 凍土領域 ===
    public static readonly EnemyDefinition IceWolf = new(
        TypeId: "ice_wolf",
        Name: "氷原狼",
        Description: "極寒の凍土に適応した大型の白狼。群れで獲物を追い詰め、氷の吹雪の中でも正確に獲物を捕捉する鋭い嗅覚を持つ。",
        BaseStats: new Stats(10, 8, 12, 8, 2, 5, 10, 1, 5),
        EnemyType: EnemyType.Pack,
        Rank: EnemyRank.Common,
        ExperienceReward: 22,
        DropTableId: "drop_slime",
        SightRange: 10,
        HearingRange: 12,
        FleeThreshold: 0.3f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition FrostGiant = new(
        TypeId: "frost_giant",
        Name: "霜の巨人",
        Description: "永久凍土の奥に棲む巨人族の生き残り。身の丈は人の三倍を超え、凍りついた大地そのもののような肌を持つ。投げつける氷塊は岩をも砕く。",
        BaseStats: new Stats(18, 20, 4, 5, 6, 10, 6, 2, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 60,
        DropTableId: "drop_orc",
        SightRange: 10,
        GiveUpDistance: 25,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Humanoid
    );

    public static readonly EnemyDefinition IceWraith = new(
        TypeId: "ice_wraith",
        Name: "氷霊",
        Description: "吹雪の中に現れる半透明の霊体。凍死した旅人の怨念が氷の精霊と融合して生まれたとされる。触れるものすべてを凍りつかせる。",
        BaseStats: new Stats(4, 4, 9, 7, 12, 10, 12, 1, 8),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,
        DropTableId: "drop_skeleton",
        SightRange: 8,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Spirit
    );

    public static readonly EnemyDefinition FrostWyrm = new(
        TypeId: "boss_frost_wyrm",
        Name: "氷竜",
        Description: "凍土の最深部の氷洞に眠る太古の氷竜。目覚めれば吹雪を巻き起こし、その氷のブレスは生あるもの全てを凍てつかせる。古の北方民族が神として崇めた存在。",
        BaseStats: new Stats(28, 30, 8, 7, 16, 20, 12, 5, 6),
        EnemyType: EnemyType.Guardian,
        Rank: EnemyRank.Boss,
        ExperienceReward: 600,
        DropTableId: "drop_dark_elf",
        SightRange: 14,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    // === 湖畔領域 ===
    public static readonly EnemyDefinition WaterNymph = new(
        TypeId: "water_nymph",
        Name: "水妖精",
        Description: "清らかな湖に棲む水の精霊。普段は穏やかだが、湖を汚す者には容赦なく水流の刃を向ける。その歌声は旅人を惑わせる。",
        BaseStats: new Stats(4, 5, 10, 8, 10, 8, 8, 12, 7),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Common,
        ExperienceReward: 20,
        DropTableId: "drop_goblin",
        SightRange: 8,
        Race: MonsterRace.Spirit
    );

    public static readonly EnemyDefinition Kappa = new(
        TypeId: "kappa",
        Name: "河童",
        Description: "湖畔に潜む水棲の小型人型生物。甲羅と嘴を持ち、水中での戦闘に長ける。頭の皿が渇くと力を失うという弱点がある。",
        BaseStats: new Stats(8, 7, 9, 10, 5, 6, 8, 3, 6),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 18,
        DropTableId: "drop_goblin",
        SightRange: 6,
        HearingRange: 10,
        FleeThreshold: 0.35f,
        Race: MonsterRace.Humanoid
    );

    public static readonly EnemyDefinition GiantFish = new(
        TypeId: "giant_fish",
        Name: "巨大魚",
        Description: "湖底に潜む巨大な淡水魚。通常は深い水底でじっとしているが、獲物を見つけると驚異的な速度で突進する。鋭い歯は鎧すら貫く。",
        BaseStats: new Stats(14, 12, 8, 4, 1, 3, 4, 1, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
        DropTableId: "drop_slime",
        SightRange: 5,
        FleeThreshold: 0.15f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition LakeSerpent = new(
        TypeId: "boss_lake_serpent",
        Name: "湖の大蛇",
        Description: "湖の最深部に棲むとされる伝説の大蛇。時折湖面に巨大な影を落とし、漁師たちの畏怖の対象となっている。その体は宝石のような鱗で覆われている。",
        BaseStats: new Stats(24, 22, 12, 10, 10, 15, 10, 4, 6),
        EnemyType: EnemyType.Berserker,
        Rank: EnemyRank.Boss,
        ExperienceReward: 500,
        DropTableId: "drop_dark_elf",
        SightRange: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    // === 火山領域 ===
    public static readonly EnemyDefinition LavaSlime = new(
        TypeId: "lava_slime",
        Name: "溶岩スライム",
        Description: "溶岩の中から生まれた灼熱のスライム。通常のスライムとは異なり、触れるだけで火傷を負う。倒しても溶岩に接触すると再生する厄介な存在。",
        BaseStats: new Stats(8, 12, 3, 2, 4, 6, 4, 1, 3),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
        DropTableId: "drop_slime",
        FleeThreshold: 0.0f,
        Race: MonsterRace.Amorphous
    );

    public static readonly EnemyDefinition Salamander = new(
        TypeId: "salamander",
        Name: "サラマンダー",
        Description: "火山の溶岩流に棲む炎の蜥蜴竜。その鱗は常に赤熱し、口からは炎を吐く。耐火の鱗は最高級の防具素材として珍重される。",
        BaseStats: new Stats(12, 10, 8, 7, 8, 6, 8, 2, 5),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Common,
        ExperienceReward: 35,
        DropTableId: "drop_orc",
        SightRange: 8,
        FleeThreshold: 0.15f,
        Race: MonsterRace.Dragon
    );

    public static readonly EnemyDefinition FireElemental = new(
        TypeId: "fire_elemental",
        Name: "炎の精霊",
        Description: "火山の火口から生まれた純粋な炎の化身。実体を持たず、物理攻撃がほとんど通じない。水や氷の魔法が有効。",
        BaseStats: new Stats(6, 8, 10, 8, 16, 14, 10, 3, 6),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 55,
        DropTableId: "drop_dark_elf",
        SightRange: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Spirit
    );

    public static readonly EnemyDefinition VolcanoTitan = new(
        TypeId: "boss_volcano_titan",
        Name: "火山の巨神",
        Description: "火山の溶岩が意思を持って固まったかのような巨大な存在。古の魔術師が封印したとされる太古の構造体で、胸の核に膨大な魔力を秘める。",
        BaseStats: new Stats(30, 35, 5, 4, 14, 20, 8, 3, 4),
        EnemyType: EnemyType.Guardian,
        Rank: EnemyRank.Boss,
        ExperienceReward: 700,
        DropTableId: "drop_dark_elf",
        SightRange: 8,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Construct
    );

    // === 聖域領域 ===
    public static readonly EnemyDefinition FallenAngel = new(
        TypeId: "fallen_angel",
        Name: "堕天使",
        Description: "かつて神に仕えた天使が堕落した存在。聖なる力と闇の力の両方を操り、人の心の弱さに付け込んで従者とする。その翼は黒く染まっている。",
        BaseStats: new Stats(10, 8, 12, 10, 18, 16, 14, 10, 8),
        EnemyType: EnemyType.Summoner,
        Rank: EnemyRank.Elite,
        ExperienceReward: 70,
        DropTableId: "drop_dark_elf",
        SightRange: 12,
        FleeThreshold: 0.2f,
        Race: MonsterRace.Demon
    );

    public static readonly EnemyDefinition HolyGolem = new(
        TypeId: "holy_golem",
        Name: "聖域の番人",
        Description: "聖域を守護するために古代の聖職者が創り出した石の番人。侵入者を排除するためだけに動き、疲れも痛みも知らない。聖なる紋章が刻まれた体は頑強。",
        BaseStats: new Stats(16, 20, 3, 3, 2, 14, 6, 1, 2),
        EnemyType: EnemyType.Guardian,
        Rank: EnemyRank.Common,
        ExperienceReward: 40,
        DropTableId: "drop_orc",
        SightRange: 6,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Construct
    );

    public static readonly EnemyDefinition Phantom = new(
        TypeId: "phantom",
        Name: "亡霊",
        Description: "聖域の地下に封印された古の邪悪な魂。壁をすり抜け、実体を持たないためほとんどの物理攻撃を無効化する。聖なる武器や光魔法が有効。",
        BaseStats: new Stats(3, 4, 10, 8, 12, 8, 12, 2, 10),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 30,
        DropTableId: "drop_skeleton",
        SightRange: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead
    );

    public static readonly EnemyDefinition SealedDemon = new(
        TypeId: "boss_sealed_demon",
        Name: "封印の魔王",
        Description: "聖域の最深部に千年もの間封印されてきた古の魔王。封印が弱まるにつれてその力を取り戻しつつあり、解放されれば世界を滅ぼしかねない存在。",
        BaseStats: new Stats(32, 28, 14, 12, 22, 24, 16, 8, 10),
        EnemyType: EnemyType.Berserker,
        Rank: EnemyRank.Boss,
        ExperienceReward: 800,
        DropTableId: "drop_dark_elf",
        SightRange: 14,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Demon
    );
    public static readonly EnemyDefinition Werewolf = new(
        TypeId: "werewolf",
        Name: "ウェアウルフ",
        Description: "狼に変身する人狼。夜に力を増す。",
        BaseStats: new Stats(14, 12, 12, 10, 4, 8, 10, 3, 4),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Elite,
        ExperienceReward: 65,
        DropTableId: "drop_werewolf",
        SightRange: 12,
        HearingRange: 14,
        GiveUpDistance: 20,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Chimera = new(
        TypeId: "chimera",
        Name: "キメラ",
        Description: "複数の獣が融合した合成獣。3つの頭を持つ。",
        BaseStats: new Stats(16, 14, 8, 8, 10, 10, 8, 2, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Rare,
        ExperienceReward: 90,
        DropTableId: "drop_chimera",
        SightRange: 10,
        FleeThreshold: 0.05f,
        Race: MonsterRace.Demon
    );

    public static readonly EnemyDefinition DeathKnight = new(
        TypeId: "death_knight",
        Name: "デスナイト",
        Description: "闇の力で蘇った騎士。強力な剣技と暗黒魔法を操る。",
        BaseStats: new Stats(18, 16, 8, 10, 12, 14, 8, 3, 3),
        EnemyType: EnemyType.Aggressive,
        Rank: EnemyRank.Rare,
        ExperienceReward: 100,
        DropTableId: "drop_death_knight",
        SightRange: 10,
        GiveUpDistance: 30,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead,
        WeaponType: Items.WeaponType.Greatsword
    );

    // === 中層バリエーション ===
    public static readonly EnemyDefinition PoisonSnake = new(
        TypeId: "poison_snake",
        Name: "毒蛇",
        Description: "暗闘に潜む大型の毒蛇。素早い動きと致死性の毒を併せ持つ。噛まれたら数ターン以内に解毒しなければ危険。",
        BaseStats: new Stats(5, 4, 14, 12, 1, 3, 12, 1, 8),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Common,
        ExperienceReward: 18,
        DropTableId: "drop_spider",
        SightRange: 5,
        HearingRange: 12,
        Race: MonsterRace.Beast
    );

    public static readonly EnemyDefinition Lich = new(
        TypeId: "lich",
        Name: "リッチ",
        Description: "死を超越した魔術師の成れの果て。膨大な魔力を操り、アンデッドの軍勢を使役する。その核たるフィラクテリを破壊しない限り、何度でも蘇る。",
        BaseStats: new Stats(5, 6, 4, 5, 22, 20, 10, 4, 6),
        EnemyType: EnemyType.Summoner,
        Rank: EnemyRank.Rare,
        ExperienceReward: 80,
        DropTableId: "drop_dark_elf",
        SightRange: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead
    );

    public static readonly EnemyDefinition ShadowAssassin = new(
        TypeId: "shadow_assassin",
        Name: "影の暗殺者",
        Description: "闇に溶けるように動く暗殺者。その正体は闇の教団に魂を売った元人間。姿を消す能力を持ち、背後からの一撃は致命的。",
        BaseStats: new Stats(10, 6, 16, 18, 8, 6, 14, 3, 10),
        EnemyType: EnemyType.Ambusher,
        Rank: EnemyRank.Rare,
        ExperienceReward: 65,
        DropTableId: "drop_dark_elf",
        SightRange: 12,
        HearingRange: 10,
        FleeThreshold: 0.25f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Dagger
    );

    public static readonly EnemyDefinition CaveBeetle = new(
        TypeId: "cave_beetle",
        Name: "地底蟲",
        Description: "地下深くに棲む巨大な甲虫。硬い甲殻は並の武器を跳ね返し、強力な顎で岩をも砕く。群れで巣を形成し、卵を守るために凶暴化する。",
        BaseStats: new Stats(10, 14, 5, 4, 1, 3, 4, 1, 2),
        EnemyType: EnemyType.Pack,
        Rank: EnemyRank.Common,
        ExperienceReward: 20,
        DropTableId: "drop_spider",
        SightRange: 4,
        HearingRange: 12,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Insect
    );

    public static readonly EnemyDefinition Basilisk = new(
        TypeId: "basilisk",
        Name: "バジリスク",
        Description: "蛇の体に竜の翼を持つ伝説の魔獣。その視線は石化の呪いを帯び、目を合わせた者は即座に石と化す。鏡で視線を反射すれば自らの力で石化する。",
        BaseStats: new Stats(14, 12, 6, 5, 10, 8, 14, 2, 6),
        EnemyType: EnemyType.Normal,
        Rank: EnemyRank.Elite,
        ExperienceReward: 55,
        DropTableId: "drop_orc",
        SightRange: 10,
        FleeThreshold: 0.1f,
        Race: MonsterRace.Dragon
    );

    public static readonly EnemyDefinition Minotaur = new(
        TypeId: "minotaur",
        Name: "ミノタウロス",
        Description: "迷宮の奥に棲む牛頭の巨人。巨大な戦斧を振るい、怒りに駆られると手がつけられなくなる。知性は低いが戦闘本能は極めて高い。",
        BaseStats: new Stats(20, 18, 8, 6, 3, 8, 6, 1, 4),
        EnemyType: EnemyType.Berserker,
        Rank: EnemyRank.Elite,
        ExperienceReward: 60,
        DropTableId: "drop_orc",
        SightRange: 6,
        GiveUpDistance: 30,
        FleeThreshold: 0.05f,
        Race: MonsterRace.Beast
    );

    #endregion

    #region Boss Monsters

    public static readonly EnemyDefinition ForestGuardian = new(
        TypeId: "boss_forest_guardian",
        Name: "森の守護者",
        Description: "森の最深部に棲む古代の精霊。森を冒涜する者を許さない。",
        BaseStats: new Stats(25, 35, 8, 8, 20, 20, 8, 5, 5),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 300,
        DropTableId: "drop_boss_forest",
        SightRange: 15,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Spirit
    );

    public static readonly EnemyDefinition MountainKing = new(
        TypeId: "boss_mountain_king",
        Name: "山嶺王",
        Description: "山脈の主たる巨人の王。大地を揺るがす一撃。",
        BaseStats: new Stats(35, 40, 4, 6, 8, 25, 4, 2, 3),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 400,
        DropTableId: "drop_boss_mountain",
        SightRange: 12,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Construct
    );

    public static readonly EnemyDefinition DeepSeaLeviathan = new(
        TypeId: "boss_leviathan",
        Name: "深海の大蛇",
        Description: "海底深くに眠る古代の海竜。津波を呼ぶ。",
        BaseStats: new Stats(30, 30, 10, 8, 18, 18, 6, 3, 4),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 350,
        DropTableId: "drop_boss_sea",
        SightRange: 14,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    public static readonly EnemyDefinition DesertPharaoh = new(
        TypeId: "boss_pharaoh",
        Name: "砂漠の覇王",
        Description: "古代王朝の不死の王。強力な呪術を操る。",
        BaseStats: new Stats(22, 20, 8, 10, 25, 18, 8, 8, 5),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 350,
        DropTableId: "drop_boss_desert",
        SightRange: 12,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead
    );

    public static readonly EnemyDefinition FrontierDragon = new(
        TypeId: "boss_dragon",
        Name: "辺境の古竜",
        Description: "辺境の果てに棲む太古の竜。全てを焼き尽くすブレスを吐く。",
        BaseStats: new Stats(40, 35, 8, 10, 20, 22, 8, 4, 5),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 500,
        DropTableId: "drop_boss_dragon",
        SightRange: 16,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    public static readonly EnemyDefinition AbyssLord = new(
        TypeId: "boss_abyss_lord",
        Name: "深淵の覇王",
        Description: "最深部に封印されていた古の魔王。圧倒的な力を持つ。",
        BaseStats: new Stats(50, 40, 12, 12, 30, 25, 10, 6, 6),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.HiddenBoss,
        ExperienceReward: 1000,
        DropTableId: "drop_boss_abyss",
        SightRange: 20,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Demon
    );

    #endregion

    #region Floor Bosses (5階ごとのフロアボス)

    /// <summary>5階ボス: 大型スライム（初見プレイヤー向け）</summary>
    public static readonly EnemyDefinition FloorBoss5 = new(
        TypeId: "floor_boss_5",
        Name: "キングスライム",
        Description: "数多のスライムが融合した巨大なスライム。弾力ある体で攻撃を吸収する。",
        BaseStats: new Stats(15, 30, 4, 4, 5, 15, 4, 1, 5),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 150,
        DropTableId: "drop_floor_boss_5",
        SightRange: 10,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Amorphous
    );

    /// <summary>10階ボス: ゴブリンキング（集団戦入門）</summary>
    public static readonly EnemyDefinition FloorBoss10 = new(
        TypeId: "floor_boss_10",
        Name: "ゴブリンキング",
        Description: "ゴブリン族の統率者。狡猾な戦術で冒険者を追い詰める。",
        BaseStats: new Stats(20, 25, 12, 10, 8, 12, 10, 4, 8),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 300,
        DropTableId: "drop_floor_boss_10",
        SightRange: 12,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Sword
    );

    /// <summary>15階ボス: スケルトンロード（不死系ボス）</summary>
    public static readonly EnemyDefinition FloorBoss15 = new(
        TypeId: "floor_boss_15",
        Name: "スケルトンロード",
        Description: "不死の軍勢を率いる骸骨の将軍。暗黒の剣技を振るう。",
        BaseStats: new Stats(25, 30, 10, 12, 15, 20, 8, 3, 4),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 500,
        DropTableId: "drop_floor_boss_15",
        SightRange: 12,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Undead,
        WeaponType: Items.WeaponType.Greatsword
    );

    /// <summary>20階ボス: ダークエルフ将軍（高速型ボス）</summary>
    public static readonly EnemyDefinition FloorBoss20 = new(
        TypeId: "floor_boss_20",
        Name: "ダークエルフ将軍",
        Description: "地下帝国の将軍。素早い剣技と暗黒魔法の二刀流。",
        BaseStats: new Stats(28, 25, 18, 16, 20, 15, 16, 6, 8),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 700,
        DropTableId: "drop_floor_boss_20",
        SightRange: 14,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Humanoid,
        WeaponType: Items.WeaponType.Sword
    );

    /// <summary>25階ボス: ドラゴン（高HP・高火力）</summary>
    public static readonly EnemyDefinition FloorBoss25 = new(
        TypeId: "floor_boss_25",
        Name: "炎竜ヴァルグレス",
        Description: "地下深くに棲む古のドラゴン。灼熱のブレスで全てを焼き尽くす。",
        BaseStats: new Stats(45, 50, 10, 10, 20, 28, 8, 4, 5),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.Boss,
        ExperienceReward: 1000,
        DropTableId: "drop_floor_boss_25",
        SightRange: 16,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    /// <summary>30階ボス: 深淵の王（最終ボス）</summary>
    public static readonly EnemyDefinition FloorBoss30 = new(
        TypeId: "floor_boss_30",
        Name: "深淵の王",
        Description: "ダンジョン最深部に封印されていた太古の魔王。万物を支配する力を持つ。",
        BaseStats: new Stats(60, 55, 14, 14, 35, 30, 12, 8, 8),
        EnemyType: EnemyType.Boss,
        Rank: EnemyRank.HiddenBoss,
        ExperienceReward: 2000,
        DropTableId: "drop_floor_boss_30",
        SightRange: 20,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Demon
    );

    /// <summary>35階ボス: 炎竜の将（高火力竜族）</summary>
    public static readonly EnemyDefinition FloorBoss35 = new(
        TypeId: "floor_boss_35",
        Name: "炎竜の将",
        Description: "灼熱の炎を操る竜族の将軍。その咆哮は大地を震わせ、炎のブレスは全てを灰燼に帰す。",
        BaseStats: new Stats(30, 28, 12, 10, 18, 22, 12, 5, 8),
        EnemyType: EnemyType.Berserker,
        Rank: EnemyRank.Boss,
        ExperienceReward: 800,
        DropTableId: "drop_floor_boss_30",
        SightRange: 16,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Dragon
    );

    /// <summary>40階ボス: 闇の大魔術師（召喚型ボス）</summary>
    public static readonly EnemyDefinition FloorBoss40 = new(
        TypeId: "floor_boss_40",
        Name: "闇の大魔術師",
        Description: "闇の深淵から力を汲み上げる大魔術師。無数のアンデッドを召喚し、自らも強力な暗黒魔法で攻撃する。",
        BaseStats: new Stats(20, 22, 14, 12, 28, 26, 14, 8, 10),
        EnemyType: EnemyType.Summoner,
        Rank: EnemyRank.Boss,
        ExperienceReward: 1000,
        DropTableId: "drop_floor_boss_30",
        SightRange: 18,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Demon
    );

    /// <summary>45階ボス: 深淵の支配者（最強格ボス）</summary>
    public static readonly EnemyDefinition FloorBoss45 = new(
        TypeId: "floor_boss_45",
        Name: "深淵の支配者",
        Description: "深淵の果てに君臨する絶対的な支配者。あらゆる魔法と物理攻撃を操り、その存在は世界の均衡を脅かす。",
        BaseStats: new Stats(35, 35, 16, 14, 25, 28, 16, 10, 12),
        EnemyType: EnemyType.Guardian,
        Rank: EnemyRank.Boss,
        ExperienceReward: 1500,
        DropTableId: "drop_floor_boss_30",
        SightRange: 20,
        FleeThreshold: 0.0f,
        Race: MonsterRace.Demon
    );

    #endregion
    public static EnemyDefinition? GetFloorBoss(int floor) => floor switch
    {
        5 => FloorBoss5,
        10 => FloorBoss10,
        15 => FloorBoss15,
        20 => FloorBoss20,
        25 => FloorBoss25,
        30 => FloorBoss30,
        35 => FloorBoss35,
        40 => FloorBoss40,
        45 => FloorBoss45,
        _ => null
    };

    /// <summary>
    /// 全フロアボス定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllFloorBosses()
    {
        return new[] { FloorBoss5, FloorBoss10, FloorBoss15, FloorBoss20, FloorBoss25, FloorBoss30, FloorBoss35, FloorBoss40, FloorBoss45 };
    }

    /// <summary>
    /// ダンジョンIDに対応するボスを取得（最深部ボス）
    /// </summary>
    public static EnemyDefinition GetDungeonBoss(string dungeonId)
    {
        return dungeonId switch
        {
            "capital_catacombs" => FloorBoss15,    // 王都地下墓地 - スケルトンロード（アンデッドテーマ）
            "capital_rift" => FloorBoss10,         // 始まりの裂け目 - ゴブリンキング
            "forest_corruption" => FloorBoss10,    // 腐敗の森 - ゴブリンキング
            "forest_ruins" => FloorBoss20,         // 古代エルフの遺跡 - ダークエルフ将軍
            "mountain_mine" => FloorBoss10,        // 採掘坑 - ゴブリンキング
            "mountain_lava" => FloorBoss25,        // 溶岩洞 - 炎竜ヴァルグレス
            "mountain_dragon" => FloorBoss30,      // 竜の巣 - 深淵の王
            "coast_cave" => FloorBoss5,            // 海岸洞窟 - スライムキング
            "coast_wreck" => DeepSeaLeviathan,     // 沈没船 - 深海リヴァイアサン（水棲テーマ）
            "southern_icecave" => FloorBoss15,     // 氷の洞窟 - スケルトンロード
            "southern_battlefield" => FloorBoss20, // 古戦場跡 - ダークエルフ将軍
            "frontier_great_rift" => FloorBoss30,  // 大裂け目 - 深淵の王
            "frontier_ancient_ruins" => FloorBoss25, // 滅びた王国の遺跡 - 炎竜ヴァルグレス
            _ => FloorBoss5                        // デフォルト
        };
    }

    /// <summary>
    /// 階層に応じた敵リストを取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetEnemiesForDepth(int depth)
    {
        return depth switch
        {
            <= 3 => new[] { Rat, Slime, Goblin },
            <= 6 => new[] { Slime, Goblin, Skeleton, GiantSpider },
            <= 10 => new[] { Goblin, Skeleton, Orc, GiantSpider, CaveBeetle, PoisonSnake },
            <= 15 => new[] { Skeleton, Orc, GiantSpider, DarkElf, ShadowAssassin },
            <= 20 => new[] { Orc, DarkElf, Troll, Draugr, Basilisk, Minotaur },
            <= 30 => new[] { DarkElf, Troll, Draugr, Lich, DeathKnight },
            <= 40 => new[] { Lich, DeathKnight, FallenAngel, ShadowAssassin },
            _ => new[] { Lich, DeathKnight, FallenAngel, SealedDemon }
        };
    }

    /// <summary>
    /// 領域に応じた敵リストを取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetEnemiesForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital => new[] { Rat, Slime, Goblin, Skeleton },
            TerritoryId.Forest => new[] { ForestWolf, ForestSprite, GiantSpider, Treant },
            TerritoryId.Mountain => new[] { Harpy, Orc, MountainGolem, Wyvern, Bandit },
            TerritoryId.Coast => new[] { Crab, SeaSerpent, Siren, Skeleton },
            TerritoryId.Southern => new[] { DesertScorpion, Mummy, Sandworm, DarkElf },
            TerritoryId.Frontier => new[] { Werewolf, Troll, Chimera, DeathKnight, Draugr },
            TerritoryId.Desert => new[] { DesertScorpion, Mummy, Sandworm, Basilisk, Bandit },
            TerritoryId.Swamp => new[] { SwampLizard, SwampToad, SwampWitch, GiantSpider, PoisonSnake },
            TerritoryId.Tundra => new[] { IceWolf, FrostGiant, IceWraith, Draugr },
            TerritoryId.Lake => new[] { WaterNymph, Kappa, GiantFish, Crab, SeaSerpent },
            TerritoryId.Volcanic => new[] { LavaSlime, Salamander, FireElemental, Wyvern, MountainGolem },
            TerritoryId.Sacred => new[] { FallenAngel, HolyGolem, Phantom, Skeleton, DeathKnight },
            _ => new[] { Slime, Goblin }
        };
    }

    /// <summary>
    /// 派閥名に基づく敵リストを取得（フィールドマップ用）。
    /// TerritoryInfluenceSystem.GetDominantFactionForTile()の結果で敵種類を切り替える。
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetEnemiesForFaction(string factionName, TerritoryId territory)
    {
        return factionName switch
        {
            "賊" => territory switch
            {
                TerritoryId.Coast => new[] { Bandit, Rat, GiantSpider },       // 海賊系
                TerritoryId.Desert => new[] { Bandit, DesertScorpion, Sandworm },
                _ => new[] { Bandit, Rat, Goblin }                           // 通常の野盗
            },
            "ゴブリン" => territory switch
            {
                TerritoryId.Swamp => new[] { Goblin, GiantSpider, Slime },    // 沼ゴブリン
                TerritoryId.Mountain => new[] { Goblin, Orc, Harpy },         // 山岳ゴブリン
                _ => new[] { Goblin, Orc, Rat }
            },
            "野生動物" => territory switch
            {
                TerritoryId.Forest => new[] { ForestWolf, Rat, ForestSprite },
                TerritoryId.Mountain => new[] { Harpy, Rat },
                TerritoryId.Coast => new[] { Crab, Rat },
                TerritoryId.Tundra => new[] { IceWolf, Rat },
                TerritoryId.Lake => new[] { GiantFish, Kappa, Crab },
                TerritoryId.Volcanic => new[] { Salamander, LavaSlime },
                _ => new[] { Rat, Slime }
            },
            "アンデッド" => territory switch
            {
                TerritoryId.Southern => new[] { Skeleton, Mummy, Draugr },
                TerritoryId.Volcanic => new[] { Skeleton, Draugr, DarkElf },
                TerritoryId.Sacred => new[] { Phantom, Skeleton, Draugr },
                _ => new[] { Skeleton, Draugr }
            },
            "魔族" => new[] { DarkElf, Troll, Chimera },
            "精霊" => new[] { WaterNymph, IceWraith, FireElemental, ForestSprite },
            "エルフ" => new[] { ForestSprite, Rat, Slime },         // エルフ領の安全圏: 弱い敵
            "ドワーフ" => new[] { Rat, Slime },                      // ドワーフ領の安全圏: 弱い敵
            "王国" => new[] { Rat, Slime },                          // 王国領の安全圏: 弱い敵のみ
            _ => new[] { Rat, Slime }
        };
    }

    /// <summary>
    /// 領域のボスを取得
    /// </summary>
    public static EnemyDefinition? GetBossForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital => null,
            TerritoryId.Forest => ForestGuardian,
            TerritoryId.Mountain => MountainKing,
            TerritoryId.Coast => DeepSeaLeviathan,
            TerritoryId.Southern => DesertPharaoh,
            TerritoryId.Frontier => FrontierDragon,
            TerritoryId.Desert => DesertPharaoh,
            TerritoryId.Swamp => SwampLord,
            TerritoryId.Tundra => FrostWyrm,
            TerritoryId.Lake => LakeSerpent,
            TerritoryId.Volcanic => VolcanoTitan,
            TerritoryId.Sacred => SealedDemon,
            _ => null
        };
    }

    /// <summary>
    /// ダンジョンIDに応じたテーマ敵リストを取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetEnemiesForDungeon(string dungeonId, int depth)
    {
        return dungeonId switch
        {
            // 王都地下墓地 - アンデッド中心
            "capital_catacombs" => depth <= 3
                ? new[] { Skeleton, Slime, Goblin }
                : new[] { Skeleton, Draugr, DarkElf },

            // 始まりの裂け目 - 混成、深層は強敵
            "capital_rift" => depth <= 5
                ? new[] { Goblin, Skeleton, Orc }
                : new[] { DarkElf, Troll, Draugr, DeathKnight },

            // 腐敗の森 - 植物＋精霊＋虫
            "forest_corruption" => depth <= 5
                ? new[] { ForestWolf, GiantSpider, ForestSprite }
                : new[] { Treant, ForestSprite, GiantSpider },

            // 古代エルフの遺跡 - ダークエルフ＋精霊＋構造体
            "forest_ruins" => depth <= 5
                ? new[] { ForestSprite, Skeleton, GiantSpider }
                : new[] { DarkElf, MountainGolem, ForestSprite, Treant },

            // 採掘坑 - ゴーレム＋虫＋ゴブリン
            "mountain_mine" => depth <= 5
                ? new[] { Goblin, GiantSpider, Slime }
                : new[] { MountainGolem, Orc, Harpy },

            // 溶岩洞 - ドラゴン系＋ゴーレム
            "mountain_lava" => depth <= 5
                ? new[] { MountainGolem, Harpy, Orc }
                : new[] { Wyvern, MountainGolem, Troll },

            // 竜の巣 - ドラゴン系メイン
            "mountain_dragon" => depth <= 5
                ? new[] { Wyvern, Harpy, MountainGolem }
                : new[] { Wyvern, Troll, DeathKnight },

            // 海岸洞窟 - 水棲＋海賊（ゴブリン代理）
            "coast_cave" => depth <= 3
                ? new[] { Crab, Slime, Goblin }
                : new[] { SeaSerpent, Crab, Goblin },

            // 沈没船 - 水棲モンスター中心
            "coast_wreck" => depth <= 5
                ? new[] { Crab, SeaSerpent, Skeleton }
                : new[] { Siren, SeaSerpent, Skeleton },

            // 氷の洞窟 - 氷属性（既存敵で代用）
            "southern_icecave" => depth <= 5
                ? new[] { DesertScorpion, Skeleton, Slime }
                : new[] { Mummy, Draugr, DarkElf },

            // 古戦場跡 - アンデッド兵士
            "southern_battlefield" => depth <= 5
                ? new[] { Skeleton, Draugr, Mummy }
                : new[] { Draugr, DeathKnight, Mummy },

            // 大裂け目 - 最強混成
            "frontier_great_rift" => depth <= 5
                ? new[] { Werewolf, Chimera, Troll }
                : new[] { DeathKnight, Chimera, Draugr },

            // 滅びた王国の遺跡 - 古代文明の番人
            "frontier_ancient_ruins" => depth <= 5
                ? new[] { MountainGolem, Skeleton, DarkElf }
                : new[] { DeathKnight, DarkElf, MountainGolem, Chimera },

            // デフォルト: 階層ベース
            _ => GetEnemiesForDepth(depth).ToArray()
        };
    }

    /// <summary>
    /// 全敵定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllEnemies()
    {
        return new[]
        {
            Rat, Bandit,
            Slime, Goblin, Skeleton, Orc, GiantSpider, DarkElf, Troll, Draugr,
            ForestWolf, Treant, ForestSprite,
            MountainGolem, Harpy, Wyvern,
            SeaSerpent, Siren, Crab,
            DesertScorpion, Mummy, Sandworm,
            SwampLizard, SwampToad, SwampWitch,
            IceWolf, FrostGiant, IceWraith,
            WaterNymph, Kappa, GiantFish,
            LavaSlime, Salamander, FireElemental,
            FallenAngel, HolyGolem, Phantom,
            Werewolf, Chimera, DeathKnight,
            PoisonSnake, Lich, ShadowAssassin, CaveBeetle, Basilisk, Minotaur,
            ForestGuardian, MountainKing, DeepSeaLeviathan, DesertPharaoh, FrontierDragon, AbyssLord,
            SwampLord, FrostWyrm, LakeSerpent, VolcanoTitan, SealedDemon
        };
    }

    /// <summary>
    /// 全ボス定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllBosses()
    {
        return new[] { ForestGuardian, MountainKing, DeepSeaLeviathan, DesertPharaoh, FrontierDragon, AbyssLord, SwampLord, FrostWyrm, LakeSerpent, VolcanoTitan, SealedDemon };
    }
}
