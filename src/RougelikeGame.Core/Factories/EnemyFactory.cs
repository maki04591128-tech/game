using RougelikeGame.Core.AI;
using RougelikeGame.Core.AI.Behaviors;
using RougelikeGame.Core.Entities;

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
        var enemy = new Enemy
        {
            Name = definition.Name,
            EnemyTypeId = definition.TypeId,
            Description = definition.Description,
            BaseStats = definition.BaseStats,
            ExperienceReward = definition.ExperienceReward,
            DropTableId = definition.DropTableId,
            SightRange = definition.SightRange,
            HearingRange = definition.HearingRange,
            GiveUpDistance = definition.GiveUpDistance,
            FleeThreshold = definition.FleeThreshold,
            Position = position,
            HomePosition = position,
            Faction = Faction.Enemy
        };

        enemy.InitializeResources();

        // AIビヘイビアを設定
        enemy.Behavior = CreateBehaviorFor(definition.EnemyType);

        return enemy;
    }

    /// <summary>
    /// 敵タイプに応じたビヘイビアを作成
    /// </summary>
    private IAIBehavior CreateBehaviorFor(EnemyType type)
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
    float FleeThreshold = 0.2f
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
        FleeThreshold: 0.1f
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
        FleeThreshold: 0.3f
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
        FleeThreshold: 0.0f  // 逃げない
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
        FleeThreshold: 0.15f
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
        HearingRange: 10  // 振動に敏感
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
        FleeThreshold: 0.25f
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
        FleeThreshold: 0.0f
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
        FleeThreshold: 0.0f
    );

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
}
