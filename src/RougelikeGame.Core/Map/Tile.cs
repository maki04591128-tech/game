namespace RougelikeGame.Core.Map;

/// <summary>
/// タイルの種類
/// </summary>
public enum TileType
{
    /// <summary>
    /// 未定義（生成前）
    /// </summary>
    Undefined,

    /// <summary>
    /// 壁
    /// </summary>
    Wall,

    /// <summary>
    /// 床
    /// </summary>
    Floor,

    /// <summary>
    /// 廊下
    /// </summary>
    Corridor,

    /// <summary>
    /// 閉じたドア
    /// </summary>
    DoorClosed,

    /// <summary>
    /// 開いたドア
    /// </summary>
    DoorOpen,

    /// <summary>
    /// 上り階段
    /// </summary>
    StairsUp,

    /// <summary>
    /// 下り階段
    /// </summary>
    StairsDown,

    /// <summary>
    /// 水
    /// </summary>
    Water,

    /// <summary>
    /// 深い水
    /// </summary>
    DeepWater,

    /// <summary>
    /// 溶岩
    /// </summary>
    Lava,

    /// <summary>
    /// 穴
    /// </summary>
    Pit,

    /// <summary>
    /// 柱
    /// </summary>
    Pillar,

    /// <summary>
    /// 祭壇
    /// </summary>
    Altar,

    /// <summary>
    /// 噴水
    /// </summary>
    Fountain,

    /// <summary>
    /// 宝箱
    /// </summary>
    Chest,

    /// <summary>
    /// ルーン碑文（魔法言語習得用）
    /// </summary>
    RuneInscription,

    /// <summary>
    /// 罠（隠れている）
    /// </summary>
    TrapHidden,

    /// <summary>
    /// 罠（発見済み）
    /// </summary>
    TrapVisible,

    /// <summary>
    /// 秘密のドア
    /// </summary>
    SecretDoor,

    /// <summary>
    /// 草地
    /// </summary>
    Grass,

    /// <summary>
    /// 木
    /// </summary>
    Tree,

    // === シンボルマップ用タイル ===

    /// <summary>
    /// シンボルマップ: 草原（移動可能な開けた地形）
    /// </summary>
    SymbolGrass,

    /// <summary>
    /// シンボルマップ: 森林（移動コスト増）
    /// </summary>
    SymbolForest,

    /// <summary>
    /// シンボルマップ: 山岳（通行可、移動コスト2.0）
    /// </summary>
    SymbolMountain,

    /// <summary>
    /// シンボルマップ: 水域（通行可、移動コスト1.8）
    /// </summary>
    SymbolWater,

    /// <summary>
    /// シンボルマップ: 道（移動コスト低）
    /// </summary>
    SymbolRoad,

    /// <summary>
    /// シンボルマップ: 街の入口
    /// </summary>
    SymbolTown,

    /// <summary>
    /// シンボルマップ: ダンジョン入口
    /// </summary>
    SymbolDungeon,

    /// <summary>
    /// シンボルマップ: 施設
    /// </summary>
    SymbolFacility,

    /// <summary>
    /// シンボルマップ: 祠・宗教施設
    /// </summary>
    SymbolShrine,

    /// <summary>
    /// シンボルマップ: 野外エリア
    /// </summary>
    SymbolField,

    /// <summary>
    /// シンボルマップ: 村
    /// </summary>
    SymbolVillage,

    /// <summary>
    /// シンボルマップ: 都（首都）
    /// </summary>
    SymbolCapital,

    /// <summary>
    /// シンボルマップ: 野盗のねぐら（ランダム生成ダンジョン）
    /// </summary>
    SymbolBanditDen,

    /// <summary>
    /// シンボルマップ: ゴブリンの巣（ランダム生成ダンジョン）
    /// </summary>
    SymbolGoblinNest,

    // === デバッグ専用タイル ===

    /// <summary>
    /// デバッグ: 敵種類変更マス（踏むと敵の種類を切り替え）
    /// </summary>
    DebugEnemySpawn,

    /// <summary>
    /// デバッグ: AI切替マス（踏むと敵AIの活性/非活性を切り替え）
    /// </summary>
    DebugAIToggle,

    /// <summary>
    /// デバッグ: 日数進行マス（踏むと1日進む）
    /// </summary>
    DebugDayAdvance,

    /// <summary>
    /// デバッグ: NPC対話マス（踏むとNPCと対話）
    /// </summary>
    DebugNpc,

    // === 町内NPC用タイル ===

    /// <summary>
    /// ギルド受付（冒険者ギルド登録・クエスト受注）
    /// </summary>
    NpcGuildReceptionist,

    /// <summary>
    /// 神父（宗教入信・改宗）
    /// </summary>
    NpcPriest,

    /// <summary>
    /// 商人（ショップ）
    /// </summary>
    NpcShopkeeper,

    /// <summary>
    /// 鍛冶屋（装備強化・合成）
    /// </summary>
    NpcBlacksmith,

    /// <summary>
    /// 宿屋主人（休息）
    /// </summary>
    NpcInnkeeper,

    /// <summary>
    /// 訓練師（スキル習得・戦闘訓練）
    /// </summary>
    NpcTrainer,

    /// <summary>
    /// 図書館司書（魔法習得・知識取得）
    /// </summary>
    NpcLibrarian,

    // === 町内建物入口 ===

    /// <summary>
    /// 建物入口（町マップ上のドアの代わり。踏むと建物内部マップへ遷移）
    /// </summary>
    BuildingEntrance,

    /// <summary>
    /// 建物出口（建物内部マップ上。踏むと町マップへ戻る）
    /// </summary>
    BuildingExit
}

/// <summary>
/// タイルの情報
/// </summary>
public class Tile
{
    public TileType Type { get; set; } = TileType.Undefined;

    /// <summary>
    /// 視界を遮るか
    /// </summary>
    public bool BlocksSight { get; set; }

    /// <summary>
    /// 移動を遮るか
    /// </summary>
    public bool BlocksMovement { get; set; }

    /// <summary>
    /// 探索済みか
    /// </summary>
    public bool IsExplored { get; set; }

    /// <summary>
    /// 現在視界内か
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// 移動コスト係数（1.0 = 通常）
    /// </summary>
    public float MovementCost { get; set; } = 1.0f;

    /// <summary>
    /// 所属する部屋ID（-1 = 廊下/壁）
    /// </summary>
    public int RoomId { get; set; } = -1;

    /// <summary>
    /// タイル上のアイテムID
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// タイル上の罠ID
    /// </summary>
    public string? TrapId { get; set; }

    /// <summary>
    /// ドアが施錠されているか
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// 施錠の難易度（0=未施錠、高いほど解除が難しい）
    /// </summary>
    public int LockDifficulty { get; set; }

    /// <summary>
    /// 建物ID（BuildingEntrance/BuildingExit タイルで使用）
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// 宝箱の中身アイテムIDリスト（Chestタイルで使用）
    /// </summary>
    public List<string>? ChestItems { get; set; }

    /// <summary>
    /// 宝箱が開封済みかどうか
    /// </summary>
    public bool ChestOpened { get; set; }

    /// <summary>
    /// 宝箱の施錠難易度（0=未施錠）
    /// </summary>
    public int ChestLockDifficulty { get; set; }

    /// <summary>
    /// ルーン碑文のルーン語ID（RuneInscriptionタイルで使用）
    /// </summary>
    public string? InscriptionWordId { get; set; }

    /// <summary>
    /// ルーン碑文が既に解読済みかどうか
    /// </summary>
    public bool InscriptionRead { get; set; }

    /// <summary>
    /// CG-1: 採取ポイントの種類（null=採取不可）
    /// </summary>
    public GatheringType? GatheringNodeType { get; set; }

    /// <summary>
    /// シンボルマップ用の高度（Altitude）。
    /// 正値=高所（山岳等）、0=平地、負値=低地/深度（水域等）。
    /// 山岳: 高度が高いほど行動コスト増加。
    /// 水域: 深度が深いほど行動コスト増加、一定深度以下は船が必要（コスト×2固定）。
    /// </summary>
    public int Altitude { get; set; } = 0;

    /// <summary>
    /// 船が必要かどうか（水域で深度が一定以下の場合）
    /// </summary>
    public bool RequiresShip { get; set; } = false;

    /// <summary>
    /// 船が必要になる水域の深度閾値（この値以下で船が必要）
    /// </summary>
    public const int ShipRequiredDepth = -3;

    /// <summary>
    /// 高度/深度を設定し、行動コストを自動計算する。
    /// 山岳（高度1-5）: コスト = 1.5 + 高度 × 0.3（最大3.0）
    /// 水域（深度-1～-2）: コスト = 1.3 + |深度| × 0.25
    /// 水域（深度-3以下）: 船が必要、コスト = 2.0固定
    /// </summary>
    public void SetAltitude(int altitude)
    {
        Altitude = altitude;

        if (Type == TileType.SymbolMountain)
        {
            // 山岳: 高度が高いほどコスト増加
            // 高度0=1.5, 高度1=1.8, 高度2=2.1, 高度3=2.4, 高度4=2.7, 高度5=3.0
            MovementCost = Math.Min(3.0f, 1.5f + Math.Max(0, altitude) * 0.3f);
        }
        else if (Type == TileType.SymbolWater)
        {
            if (altitude <= ShipRequiredDepth)
            {
                // 深い水域: 船が必要、コスト×2固定
                RequiresShip = true;
                MovementCost = 2.0f;
            }
            else
            {
                // 浅い水域: 深度に応じてコスト増加
                // 深度0=1.3, 深度-1=1.55, 深度-2=1.8
                RequiresShip = false;
                MovementCost = 1.3f + Math.Abs(Math.Min(0, altitude)) * 0.25f;
            }
        }
    }

    /// <summary>
    /// 表示文字
    /// </summary>
    public char DisplayChar => GetDisplayChar();

    /// <summary>
    /// タイルタイプから基本プロパティを設定
    /// </summary>
    public static Tile FromType(TileType type)
    {
        var tile = new Tile { Type = type };
        ApplyTypeProperties(tile);
        return tile;
    }

    private static void ApplyTypeProperties(Tile tile)
    {
        switch (tile.Type)
        {
            case TileType.Wall:
                tile.BlocksSight = true;
                tile.BlocksMovement = true;
                break;

            case TileType.Floor:
            case TileType.Corridor:
            case TileType.Grass:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.DoorClosed:
            case TileType.SecretDoor:
                tile.BlocksSight = true;
                tile.BlocksMovement = true;
                break;

            case TileType.DoorOpen:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.StairsUp:
            case TileType.StairsDown:
            case TileType.Altar:
            case TileType.Fountain:
            case TileType.Chest:
            case TileType.RuneInscription:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.Water:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                tile.MovementCost = 2.0f;
                break;

            case TileType.DeepWater:
                tile.BlocksSight = false;
                tile.BlocksMovement = true; // 泳げるスキルがないと通れない
                break;

            case TileType.Lava:
                tile.BlocksSight = false;
                tile.BlocksMovement = true;
                break;

            case TileType.Pit:
                tile.BlocksSight = false;
                tile.BlocksMovement = true;
                break;

            case TileType.Pillar:
            case TileType.Tree:
                tile.BlocksSight = true;
                tile.BlocksMovement = true;
                break;

            case TileType.TrapHidden:
            case TileType.TrapVisible:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            // シンボルマップ用タイル
            case TileType.SymbolGrass:
            case TileType.SymbolRoad:
            case TileType.SymbolTown:
            case TileType.SymbolDungeon:
            case TileType.SymbolFacility:
            case TileType.SymbolShrine:
            case TileType.SymbolField:
            case TileType.SymbolVillage:
            case TileType.SymbolCapital:
            case TileType.SymbolBanditDen:
            case TileType.SymbolGoblinNest:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.SymbolForest:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                tile.MovementCost = 1.5f;
                break;

            case TileType.SymbolMountain:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                // 高度に応じた行動コストはSetAltitudeで設定される
                // デフォルトは高度0の山岳（基本コスト1.5）
                tile.MovementCost = 1.5f;
                break;

            case TileType.SymbolWater:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                // 深度に応じた行動コストはSetAltitudeで設定される
                // デフォルトは深度0の浅い水域（基本コスト1.3）
                tile.MovementCost = 1.3f;
                break;

            case TileType.DebugEnemySpawn:
            case TileType.DebugAIToggle:
            case TileType.DebugDayAdvance:
            case TileType.DebugNpc:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.NpcGuildReceptionist:
            case TileType.NpcPriest:
            case TileType.NpcShopkeeper:
            case TileType.NpcBlacksmith:
            case TileType.NpcInnkeeper:
            case TileType.NpcTrainer:
            case TileType.NpcLibrarian:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            case TileType.BuildingEntrance:
            case TileType.BuildingExit:
                tile.BlocksSight = false;
                tile.BlocksMovement = false;
                break;

            default:
                tile.BlocksSight = true;
                tile.BlocksMovement = true;
                break;
        }
    }

    private char GetDisplayChar()
    {
        return Type switch
        {
            TileType.Undefined => ' ',
            TileType.Wall => '#',
            TileType.Floor => '.',
            TileType.Corridor => '.',
            TileType.DoorClosed => '+',
            TileType.DoorOpen => '/',
            TileType.StairsUp => '<',
            TileType.StairsDown => '>',
            TileType.Water => '~',
            TileType.DeepWater => '≈',
            TileType.Lava => '≋',
            TileType.Pit => '^',
            TileType.Pillar => 'O',
            TileType.Altar => '_',
            TileType.Fountain => '{',
            TileType.Chest => '□',
            TileType.RuneInscription => 'ᚱ',
            TileType.TrapHidden => '.',  // 隠れている罠は床に見える
            TileType.TrapVisible => '^',
            TileType.SecretDoor => '#',  // 秘密のドアは壁に見える
            TileType.Grass => '"',
            TileType.Tree => '♣',
            TileType.DebugEnemySpawn => 'E',
            TileType.DebugAIToggle => 'A',
            TileType.DebugDayAdvance => 'D',
            TileType.DebugNpc => 'N',
            TileType.SymbolGrass => ',',
            TileType.SymbolForest => '♣',
            TileType.SymbolMountain => '▲',
            TileType.SymbolWater => '~',
            TileType.SymbolRoad => '=',
            TileType.SymbolTown => '■',
            TileType.SymbolDungeon => '▼',
            TileType.SymbolFacility => '☆',
            TileType.SymbolShrine => '†',
            TileType.SymbolField => '◇',
            TileType.SymbolVillage => '◆',
            TileType.SymbolCapital => '★',
            TileType.SymbolBanditDen => '☠',
            TileType.SymbolGoblinNest => '⚔',
            TileType.NpcGuildReceptionist => 'G',
            TileType.NpcPriest => 'P',
            TileType.NpcShopkeeper => 'S',
            TileType.NpcBlacksmith => 'B',
            TileType.NpcInnkeeper => 'I',
            TileType.NpcTrainer => 'T',      // P-1
            TileType.NpcLibrarian => 'L',     // P-2
            TileType.BuildingEntrance => '⌂',
            TileType.BuildingExit => '<',
            _ => '?'
        };
    }
}

/// <summary>
/// 部屋の種類
/// </summary>
public enum RoomType
{
    /// <summary>
    /// 通常の部屋
    /// </summary>
    Normal,

    /// <summary>
    /// 開始地点
    /// </summary>
    Entrance,

    /// <summary>
    /// ボス部屋
    /// </summary>
    Boss,

    /// <summary>
    /// 宝物庫
    /// </summary>
    Treasure,

    /// <summary>
    /// 図書室
    /// </summary>
    Library,

    /// <summary>
    /// 祭壇の間
    /// </summary>
    Shrine,

    /// <summary>
    /// 牢獄
    /// </summary>
    Prison,

    /// <summary>
    /// 倉庫
    /// </summary>
    Storage,

    /// <summary>
    /// 隠し部屋
    /// </summary>
    Secret,

    /// <summary>
    /// BB-1: 店
    /// </summary>
    Shop,

    /// <summary>
    /// BB-1: 罠部屋
    /// </summary>
    TrapRoom
}

/// <summary>
/// 部屋の情報
/// </summary>
public class Room
{
    public int Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public RoomType Type { get; set; } = RoomType.Normal;

    /// <summary>
    /// 接続している部屋のID
    /// </summary>
    public List<int> ConnectedRooms { get; } = new();

    /// <summary>
    /// 部屋の中心座標
    /// </summary>
    public Position Center => new(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// 部屋の面積
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// 指定座標が部屋内か判定
    /// </summary>
    public bool Contains(Position pos) =>
        pos.X >= X && pos.X < X + Width &&
        pos.Y >= Y && pos.Y < Y + Height;

    /// <summary>
    /// 部屋内のランダムな座標を取得
    /// </summary>
    public Position GetRandomPosition(Random random)
    {
        return new Position(
            random.Next(X + 1, X + Width - 1),
            random.Next(Y + 1, Y + Height - 1)
        );
    }

    /// <summary>
    /// 他の部屋と重なっているか判定
    /// </summary>
    public bool Intersects(Room other, int margin = 1)
    {
        return X - margin < other.X + other.Width &&
               X + Width + margin > other.X &&
               Y - margin < other.Y + other.Height &&
               Y + Height + margin > other.Y;
    }
}

/// <summary>
/// ダンジョン生成パラメータ
/// </summary>
public record struct DungeonGenerationParameters
{
    /// <summary>
    /// マップの幅
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// マップの高さ
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// 階層の深さ
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// 目標部屋数
    /// </summary>
    public int RoomCount { get; init; }

    /// <summary>
    /// 敵密度（床タイルに対する敵の割合）
    /// </summary>
    public float EnemyDensity { get; init; }

    /// <summary>
    /// アイテム密度
    /// </summary>
    public float ItemDensity { get; init; }

    /// <summary>
    /// 罠密度
    /// </summary>
    public float TrapDensity { get; init; }

    /// <summary>
    /// 乱数シード（省略可）
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>
    /// ボス階かどうか（宝箱確定配置用）
    /// </summary>
    public bool IsBossFloor { get; init; }

    /// <summary>
    /// ダンジョンID（テーマ別アイテム・構造生成用）
    /// </summary>
    public string? DungeonId { get; init; }

    /// <summary>
    /// デフォルトパラメータ
    /// </summary>
    public static DungeonGenerationParameters Default => new()
    {
        Width = 80,
        Height = 50,
        Depth = 1,
        RoomCount = 8,
        EnemyDensity = 0.02f,
        ItemDensity = 0.01f,
        TrapDensity = 0.005f,
        Seed = null
    };

    /// <summary>
    /// 指定階層用のパラメータを作成
    /// </summary>
    public static DungeonGenerationParameters ForDepth(int depth)
    {
        return Default with
        {
            Depth = depth,
            EnemyDensity = 0.02f + (depth * 0.002f),
            TrapDensity = 0.005f + (depth * 0.001f)
        };
    }
}
