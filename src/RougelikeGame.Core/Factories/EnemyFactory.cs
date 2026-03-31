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

        var enemy = new Enemy
        {
            Name = definition.Name,
            EnemyTypeId = definition.TypeId,
            Description = definition.Description,
            BaseStats = stats,
            ExperienceReward = definition.ExperienceReward,
            DropTableId = definition.DropTableId,
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
        EnemyType: EnemyType.Defensive,
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
        BaseStats: new Stats(4, 4, 14, 12, 15, 10, 14, 3, 8),
        EnemyType: EnemyType.Coward,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
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
        BaseStats: new Stats(10, 16, 4, 6, 1, 18, 3, 1, 3),
        EnemyType: EnemyType.Defensive,
        Rank: EnemyRank.Common,
        ExperienceReward: 25,
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

    // === 辺境領域 ===
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

    #endregion

    /// <summary>
    /// フロアボスを取得（5階ごと）
    /// </summary>
    public static EnemyDefinition? GetFloorBoss(int floor) => floor switch
    {
        5 => FloorBoss5,
        10 => FloorBoss10,
        15 => FloorBoss15,
        20 => FloorBoss20,
        25 => FloorBoss25,
        30 => FloorBoss30,
        _ => null
    };

    /// <summary>
    /// 全フロアボス定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllFloorBosses()
    {
        return new[] { FloorBoss5, FloorBoss10, FloorBoss15, FloorBoss20, FloorBoss25, FloorBoss30 };
    }

    /// <summary>
    /// ダンジョンIDに対応するボスを取得（最深部ボス）
    /// </summary>
    public static EnemyDefinition GetDungeonBoss(string dungeonId)
    {
        return dungeonId switch
        {
            "capital_catacombs" => FloorBoss5,     // 王都地下墓地 - ゴブリンキング
            "capital_rift" => FloorBoss15,         // 始まりの裂け目 - スケルトンロード
            "forest_corruption" => FloorBoss10,    // 腐敗の森 - ゴブリンキング
            "forest_ruins" => FloorBoss20,         // 古代エルフの遺跡 - ダークエルフ将軍
            "mountain_mine" => FloorBoss10,        // 採掘坑 - ゴブリンキング
            "mountain_lava" => FloorBoss25,        // 溶岩洞 - 炎竜ヴァルグレス
            "mountain_dragon" => FloorBoss30,      // 竜の巣 - 深淵の王
            "coast_cave" => FloorBoss5,            // 海岸洞窟 - スライムキング
            "coast_wreck" => FloorBoss15,          // 沈没船 - スケルトンロード
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
            <= 3 => new[] { Slime, Goblin },
            <= 6 => new[] { Slime, Goblin, Skeleton, GiantSpider },
            <= 10 => new[] { Goblin, Skeleton, Orc, GiantSpider },
            <= 15 => new[] { Skeleton, Orc, GiantSpider, DarkElf },
            <= 20 => new[] { Orc, DarkElf, Troll, Draugr },
            _ => new[] { DarkElf, Troll, Draugr }
        };
    }

    /// <summary>
    /// 領域に応じた敵リストを取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetEnemiesForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital => new[] { Slime, Goblin, Skeleton },
            TerritoryId.Forest => new[] { ForestWolf, ForestSprite, GiantSpider, Treant },
            TerritoryId.Mountain => new[] { Harpy, Orc, MountainGolem, Wyvern },
            TerritoryId.Coast => new[] { Crab, SeaSerpent, Siren, Skeleton },
            TerritoryId.Southern => new[] { DesertScorpion, Mummy, Sandworm, DarkElf },
            TerritoryId.Frontier => new[] { Werewolf, Troll, Chimera, DeathKnight, Draugr },
            _ => new[] { Slime, Goblin }
        };
    }

    /// <summary>
    /// 領域のボスを取得
    /// </summary>
    public static EnemyDefinition? GetBossForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Forest => ForestGuardian,
            TerritoryId.Mountain => MountainKing,
            TerritoryId.Coast => DeepSeaLeviathan,
            TerritoryId.Southern => DesertPharaoh,
            TerritoryId.Frontier => FrontierDragon,
            _ => null
        };
    }

    /// <summary>
    /// 全敵定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllEnemies()
    {
        return new[]
        {
            Slime, Goblin, Skeleton, Orc, GiantSpider, DarkElf, Troll, Draugr,
            ForestWolf, Treant, ForestSprite,
            MountainGolem, Harpy, Wyvern,
            SeaSerpent, Siren, Crab,
            DesertScorpion, Mummy, Sandworm,
            Werewolf, Chimera, DeathKnight,
            ForestGuardian, MountainKing, DeepSeaLeviathan, DesertPharaoh, FrontierDragon, AbyssLord
        };
    }

    /// <summary>
    /// 全ボス定義を取得
    /// </summary>
    public static IReadOnlyList<EnemyDefinition> GetAllBosses()
    {
        return new[] { ForestGuardian, MountainKing, DeepSeaLeviathan, DesertPharaoh, FrontierDragon, AbyssLord };
    }
}
