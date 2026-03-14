using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// ダンジョン生成器（BSP + ランダム部屋配置）
/// </summary>
public class DungeonGenerator : IMapGenerator
{
    private readonly Random _random;

    public DungeonGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public DungeonGenerator(IRandomProvider random)
    {
        _random = new Random(random.Next(int.MaxValue));
    }

    /// <summary>
    /// ダンジョンを生成
    /// </summary>
    public IMap Generate(DungeonGenerationParameters parameters)
    {
        var map = new DungeonMap(parameters.Width, parameters.Height)
        {
            Depth = parameters.Depth
        };

        // 部屋を生成
        GenerateRooms(map, parameters);

        // 部屋を廊下で接続
        ConnectRooms(map);

        // 斜め移動でしか入れない箇所を修正
        FixDiagonalOnlyTiles(map);

        // ドアを配置
        CorridorGenerator.PlaceDoors(map, _random, 0.25f);

        // 階段を配置
        PlaceStairs(map);

        // 部屋を装飾
        DecorateRooms(map, parameters);

        // 罠を配置
        PlaceTraps(map, parameters);

        // 最終チェック: 装飾後にも斜め専用箇所が生まれ得るので再度修正
        FixDiagonalOnlyTiles(map);

        return map;
    }

    /// <summary>
    /// BSPアルゴリズムで部屋を生成
    /// </summary>
    private void GenerateRooms(DungeonMap map, DungeonGenerationParameters parameters)
    {
        var bspTree = new BSPNode(2, 2, map.Width - 4, map.Height - 4);

        // BSPで空間を分割
        SplitBSP(bspTree, parameters.RoomCount);

        // 各リーフノードに部屋を作成
        var leaves = bspTree.GetLeaves();
        int roomId = 0;

        foreach (var leaf in leaves)
        {
            var room = CreateRoomInNode(leaf, roomId++);
            if (room != null)
            {
                map.AddRoom(room);
                CarveRoom(map, room);
            }
        }

        // 最初の部屋を入口、最後の部屋をボス部屋に
        if (map.Rooms.Count > 0)
        {
            var rooms = map.Rooms.ToList();
            rooms[0].Type = RoomType.Entrance;

            if (rooms.Count > 1)
            {
                // 入口から最も遠い部屋をボス部屋に
                var entrance = rooms[0];
                var farthest = rooms
                    .Skip(1)
                    .OrderByDescending(r => r.Center.ChebyshevDistanceTo(entrance.Center))
                    .First();
                farthest.Type = RoomType.Boss;
            }
        }
    }

    /// <summary>
    /// BSPノードを再帰的に分割
    /// </summary>
    private void SplitBSP(BSPNode node, int targetRooms, int depth = 0)
    {
        const int MinSize = 8;
        const int MaxDepth = 6;

        if (depth >= MaxDepth) return;

        // 分割するかどうかの確率
        float splitChance = 1.0f - (depth * 0.15f);
        if (_random.NextDouble() > splitChance) return;

        // 分割方向を決定
        bool splitHorizontally;
        if (node.Width > node.Height * 1.5)
            splitHorizontally = false;
        else if (node.Height > node.Width * 1.5)
            splitHorizontally = true;
        else
            splitHorizontally = _random.NextDouble() < 0.5;

        int max = (splitHorizontally ? node.Height : node.Width) - MinSize;
        if (max < MinSize) return;

        int split = _random.Next(MinSize, max);

        if (splitHorizontally)
        {
            node.Left = new BSPNode(node.X, node.Y, node.Width, split);
            node.Right = new BSPNode(node.X, node.Y + split, node.Width, node.Height - split);
        }
        else
        {
            node.Left = new BSPNode(node.X, node.Y, split, node.Height);
            node.Right = new BSPNode(node.X + split, node.Y, node.Width - split, node.Height);
        }

        SplitBSP(node.Left, targetRooms, depth + 1);
        SplitBSP(node.Right, targetRooms, depth + 1);
    }

    /// <summary>
    /// BSPノード内に部屋を作成
    /// </summary>
    private Room? CreateRoomInNode(BSPNode node, int roomId)
    {
        const int MinRoomSize = 5;
        const int Margin = 2;

        int maxWidth = node.Width - Margin * 2;
        int maxHeight = node.Height - Margin * 2;

        if (maxWidth < MinRoomSize || maxHeight < MinRoomSize)
            return null;

        int roomWidth = _random.Next(MinRoomSize, Math.Min(maxWidth, 12) + 1);
        int roomHeight = _random.Next(MinRoomSize, Math.Min(maxHeight, 10) + 1);

        int roomX = node.X + Margin + _random.Next(maxWidth - roomWidth + 1);
        int roomY = node.Y + Margin + _random.Next(maxHeight - roomHeight + 1);

        return new Room
        {
            Id = roomId,
            X = roomX,
            Y = roomY,
            Width = roomWidth,
            Height = roomHeight
        };
    }

    /// <summary>
    /// 部屋を掘る
    /// </summary>
    private void CarveRoom(DungeonMap map, Room room)
    {
        // 部屋の形状をランダムに選択
        int shapeRoll = _random.Next(10);

        if (shapeRoll < 7) // 70%: 矩形
        {
            RoomGenerator.CarveRoom(map, room);
        }
        else if (shapeRoll < 9) // 20%: 円形
        {
            RoomGenerator.CarveCircularRoom(map, room);
        }
        else // 10%: 十字形
        {
            RoomGenerator.CarveCrossRoom(map, room);
        }
    }

    /// <summary>
    /// すべての部屋を廊下で接続
    /// </summary>
    private void ConnectRooms(DungeonMap map)
    {
        var rooms = map.Rooms.ToList();
        if (rooms.Count < 2) return;

        // 最小スパニングツリー風に接続
        var connected = new HashSet<int> { 0 };
        var remaining = new HashSet<int>(Enumerable.Range(1, rooms.Count - 1));

        while (remaining.Count > 0)
        {
            int bestFrom = -1;
            int bestTo = -1;
            int bestDistance = int.MaxValue;

            foreach (int from in connected)
            {
                foreach (int to in remaining)
                {
                    int distance = rooms[from].Center.ChebyshevDistanceTo(rooms[to].Center);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestFrom = from;
                        bestTo = to;
                    }
                }
            }

            if (bestFrom >= 0 && bestTo >= 0)
            {
                ConnectTwoRooms(map, rooms[bestFrom], rooms[bestTo]);
                rooms[bestFrom].ConnectedRooms.Add(bestTo);
                rooms[bestTo].ConnectedRooms.Add(bestFrom);
                connected.Add(bestTo);
                remaining.Remove(bestTo);
            }
        }

        // 追加の接続（ループを作る）
        int extraConnections = _random.Next(1, Math.Max(2, rooms.Count / 3));
        for (int i = 0; i < extraConnections; i++)
        {
            int from = _random.Next(rooms.Count);
            int to = _random.Next(rooms.Count);

            if (from != to && !rooms[from].ConnectedRooms.Contains(to))
            {
                ConnectTwoRooms(map, rooms[from], rooms[to]);
                rooms[from].ConnectedRooms.Add(to);
                rooms[to].ConnectedRooms.Add(from);
            }
        }
    }

    /// <summary>
    /// 2つの部屋を廊下で接続
    /// </summary>
    private void ConnectTwoRooms(DungeonMap map, Room room1, Room room2)
    {
        int method = _random.Next(3);

        switch (method)
        {
            case 0:
                CorridorGenerator.ConnectRoomsL(map, room1.Center, room2.Center, _random);
                break;
            case 1:
                CorridorGenerator.ConnectRoomsZigzag(map, room1.Center, room2.Center, _random);
                break;
            default:
                CorridorGenerator.ConnectRoomsDirect(map, room1.Center, room2.Center);
                break;
        }
    }

    /// <summary>
    /// 階段を配置
    /// </summary>
    private void PlaceStairs(DungeonMap map)
    {
        var entrance = map.GetEntranceRoom();
        var boss = map.GetBossRoom();

        if (entrance != null)
        {
            var pos = map.GetRandomWalkablePositionInRoom(entrance, _random);
            if (pos.HasValue)
            {
                map.SetStairsUp(pos.Value);
                map.SetEntrance(pos.Value);
            }
        }

        if (boss != null)
        {
            var pos = map.GetRandomWalkablePositionInRoom(boss, _random);
            if (pos.HasValue)
            {
                map.SetStairsDown(pos.Value);
            }
        }
    }

    /// <summary>
    /// 部屋を装飾
    /// </summary>
    private void DecorateRooms(DungeonMap map, DungeonGenerationParameters parameters)
    {
        var rooms = map.Rooms.ToList();

        // 特殊部屋をランダムに割り当て
        var normalRooms = rooms.Where(r => r.Type == RoomType.Normal).ToList();

        if (normalRooms.Count > 2)
        {
            // 宝物庫
            var treasureRoom = normalRooms[_random.Next(normalRooms.Count)];
            treasureRoom.Type = RoomType.Treasure;
            normalRooms.Remove(treasureRoom);
        }

        if (normalRooms.Count > 2)
        {
            // 祭壇の間
            var shrineRoom = normalRooms[_random.Next(normalRooms.Count)];
            shrineRoom.Type = RoomType.Shrine;
            normalRooms.Remove(shrineRoom);
        }

        // 各部屋を装飾
        foreach (var room in rooms)
        {
            RoomGenerator.DecorateRoom(map, room, _random);
        }
    }

    /// <summary>
    /// 罠を配置
    /// </summary>
    private void PlaceTraps(DungeonMap map, DungeonGenerationParameters parameters)
    {
        int floorTiles = map.CountFloorTiles();
        int trapCount = (int)(floorTiles * parameters.TrapDensity);

        for (int i = 0; i < trapCount; i++)
        {
            var pos = map.GetRandomWalkablePosition(_random);
            if (pos.HasValue && map.GetTileType(pos.Value) == TileType.Floor)
            {
                map.SetTile(pos.Value, TileType.TrapHidden);
            }
        }
    }

    /// <summary>
    /// 斜め移動でしか到達できないタイルを修正する。
    /// 4方向（上下左右）のみで移動可能なマップを保証する。
    /// 歩行可能タイルの4方向隣接がすべて壁の場合は壁に変換し、
    /// 斜め隣接のみで接続されている角を廊下で補完する。
    /// </summary>
    private void FixDiagonalOnlyTiles(DungeonMap map)
    {
        bool changed = true;
        int maxIterations = 10;

        while (changed && maxIterations-- > 0)
        {
            changed = false;

            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int y = 1; y < map.Height - 1; y++)
                {
                    var pos = new Position(x, y);
                    if (!map.IsWalkable(pos)) continue;

                    // 4方向の歩行可能タイル数を数える
                    var cardinals = new[]
                    {
                        new Position(x, y - 1),   // 上
                        new Position(x, y + 1),   // 下
                        new Position(x - 1, y),   // 左
                        new Position(x + 1, y)    // 右
                    };

                    int walkableCardinals = cardinals.Count(p => map.IsInBounds(p) && map.IsWalkable(p));

                    if (walkableCardinals == 0)
                    {
                        // 4方向すべて壁 → 斜めでしか来られないので壁にする
                        map.SetTile(pos, TileType.Wall);
                        changed = true;
                        continue;
                    }

                    // 斜めに歩行可能タイルがあるが、その間を繋ぐ4方向パスがない場合、
                    // 廊下を追加して4方向で通れるようにする
                    var diagonals = new[]
                    {
                        (new Position(x - 1, y - 1), new Position(x - 1, y), new Position(x, y - 1)),  // 左上
                        (new Position(x + 1, y - 1), new Position(x + 1, y), new Position(x, y - 1)),  // 右上
                        (new Position(x - 1, y + 1), new Position(x - 1, y), new Position(x, y + 1)),  // 左下
                        (new Position(x + 1, y + 1), new Position(x + 1, y), new Position(x, y + 1))   // 右下
                    };

                    foreach (var (diagPos, adj1, adj2) in diagonals)
                    {
                        if (!map.IsInBounds(diagPos) || !map.IsWalkable(diagPos)) continue;

                        bool adj1Walkable = map.IsInBounds(adj1) && map.IsWalkable(adj1);
                        bool adj2Walkable = map.IsInBounds(adj2) && map.IsWalkable(adj2);

                        // 斜めに床があるが、間の2タイルが両方壁 → 斜め移動専用
                        if (!adj1Walkable && !adj2Walkable)
                        {
                            // どちらか一方を廊下にして4方向で通れるようにする
                            map.SetTile(adj1, TileType.Corridor);
                            changed = true;
                        }
                    }
                }
            }
        }
    }
}

/// <summary>
/// BSP（Binary Space Partitioning）ノード
/// </summary>
public class BSPNode
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public BSPNode? Left { get; set; }
    public BSPNode? Right { get; set; }

    public bool IsLeaf => Left == null && Right == null;

    public BSPNode(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// すべてのリーフノードを取得
    /// </summary>
    public List<BSPNode> GetLeaves()
    {
        var leaves = new List<BSPNode>();

        if (IsLeaf)
        {
            leaves.Add(this);
        }
        else
        {
            if (Left != null) leaves.AddRange(Left.GetLeaves());
            if (Right != null) leaves.AddRange(Right.GetLeaves());
        }

        return leaves;
    }
}
