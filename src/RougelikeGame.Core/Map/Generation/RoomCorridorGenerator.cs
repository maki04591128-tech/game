namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// 部屋生成アルゴリズム
/// </summary>
public static class RoomGenerator
{
    /// <summary>
    /// 矩形の部屋を生成
    /// </summary>
    public static void CarveRoom(DungeonMap map, Room room)
    {
        for (int x = room.X; x < room.X + room.Width; x++)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                if (x == room.X || x == room.X + room.Width - 1 ||
                    y == room.Y || y == room.Y + room.Height - 1)
                {
                    // 境界は壁
                    map.SetTile(x, y, TileType.Wall);
                }
                else
                {
                    // 内部は床
                    map.SetTile(x, y, TileType.Floor);
                    map[x, y].RoomId = room.Id;
                }
            }
        }
    }

    /// <summary>
    /// 円形の部屋を生成
    /// </summary>
    public static void CarveCircularRoom(DungeonMap map, Room room)
    {
        var center = room.Center;
        int radiusX = room.Width / 2 - 1;
        int radiusY = room.Height / 2 - 1;

        // 半径が0以下の場合は矩形として彫る（Width/Heightが2以下のケース）
        if (radiusX <= 0 || radiusY <= 0)
        {
            CarveRoom(map, room);
            return;
        }

        for (int x = room.X; x < room.X + room.Width; x++)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                float dx = (x - center.X) / (float)radiusX;
                float dy = (y - center.Y) / (float)radiusY;

                if (dx * dx + dy * dy <= 1.0f)
                {
                    map.SetTile(x, y, TileType.Floor);
                    map[x, y].RoomId = room.Id;
                }
            }
        }
    }

    /// <summary>
    /// 十字形の部屋を生成
    /// </summary>
    public static void CarveCrossRoom(DungeonMap map, Room room)
    {
        var center = room.Center;
        int armWidth = Math.Max(3, room.Width / 3);
        int armHeight = Math.Max(3, room.Height / 3);

        // 横方向のアーム
        for (int x = room.X + 1; x < room.X + room.Width - 1; x++)
        {
            for (int y = center.Y - armHeight / 2; y <= center.Y + armHeight / 2; y++)
            {
                if (map.IsInBounds(x, y))
                {
                    map.SetTile(x, y, TileType.Floor);
                    map[x, y].RoomId = room.Id;
                }
            }
        }

        // 縦方向のアーム
        for (int y = room.Y + 1; y < room.Y + room.Height - 1; y++)
        {
            for (int x = center.X - armWidth / 2; x <= center.X + armWidth / 2; x++)
            {
                if (map.IsInBounds(x, y))
                {
                    map.SetTile(x, y, TileType.Floor);
                    map[x, y].RoomId = room.Id;
                }
            }
        }
    }

    /// <summary>
    /// 部屋のタイプに応じた装飾を追加
    /// </summary>
    public static void DecorateRoom(DungeonMap map, Room room, Random random)
    {
        switch (room.Type)
        {
            case RoomType.Shrine:
                AddAltar(map, room);
                break;

            case RoomType.Treasure:
                AddChests(map, room, random, 2);
                break;

            case RoomType.Library:
                AddPillars(map, room, random);
                break;

            case RoomType.Boss:
                AddPillarsAtCorners(map, room);
                break;

            case RoomType.Normal:
                // 一定確率で装飾を追加
                if (random.NextDouble() < 0.3)
                {
                    AddRandomDecoration(map, room, random);
                }
                break;
        }
    }

    private static void AddAltar(DungeonMap map, Room room)
    {
        var center = room.Center;
        if (map.IsInBounds(center) && map.IsWalkable(center))
        {
            map.SetTile(center, TileType.Altar);
        }
    }

    private static void AddChests(DungeonMap map, Room room, Random random, int count)
    {
        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var pos = room.GetRandomPosition(random);
                if (map.IsWalkable(pos) && map.GetTileType(pos) == TileType.Floor)
                {
                    map.SetTile(pos, TileType.Chest);
                    break;
                }
            }
        }
    }

    private static void AddPillars(DungeonMap map, Room room, Random random)
    {
        int pillars = Math.Max(2, (room.Width * room.Height) / 20);

        for (int i = 0; i < pillars; i++)
        {
            var pos = room.GetRandomPosition(random);
            if (map.IsWalkable(pos) && map.GetTileType(pos) == TileType.Floor
                && !WouldBlockPassage(map, pos))
            {
                map.SetTile(pos, TileType.Pillar);
            }
        }
    }

    private static void AddPillarsAtCorners(DungeonMap map, Room room)
    {
        int margin = 2;
        var corners = new[]
        {
            new Position(room.X + margin, room.Y + margin),
            new Position(room.X + room.Width - margin - 1, room.Y + margin),
            new Position(room.X + margin, room.Y + room.Height - margin - 1),
            new Position(room.X + room.Width - margin - 1, room.Y + room.Height - margin - 1)
        };

        foreach (var corner in corners)
        {
            if (map.IsInBounds(corner) && map.GetTileType(corner) == TileType.Floor
                && !WouldBlockPassage(map, corner))
            {
                map.SetTile(corner, TileType.Pillar);
            }
        }
    }

    private static void AddRandomDecoration(DungeonMap map, Room room, Random random)
    {
        int roll = random.Next(4);

        switch (roll)
        {
            case 0: // 噴水
                var center = room.Center;
                if (map.IsWalkable(center) && !WouldBlockPassage(map, center))
                {
                    map.SetTile(center, TileType.Fountain);
                }
                break;

            case 1: // 柱
                AddPillars(map, room, random);
                break;

            case 2: // 水たまり
                AddWaterPuddle(map, room, random);
                break;
        }
    }

    /// <summary>
    /// 指定位置に通行不能物を置くと通路やドアをブロックするかを判定。
    /// 隣接する歩行可能タイル同士が分断される場合にtrueを返す。
    /// </summary>
    private static bool WouldBlockPassage(DungeonMap map, Position pos)
    {
        var cardinals = new[]
        {
            new Position(pos.X, pos.Y - 1),
            new Position(pos.X + 1, pos.Y),
            new Position(pos.X, pos.Y + 1),
            new Position(pos.X - 1, pos.Y)
        };

        // 隣接タイルに通路やドアがある場合、その通路を塞ぐ可能性が高い
        foreach (var adj in cardinals)
        {
            if (!map.IsInBounds(adj)) continue;
            var adjType = map.GetTileType(adj);
            if (adjType == TileType.Corridor || adjType == TileType.DoorClosed || adjType == TileType.DoorOpen)
                return true;
        }

        // 4方向の歩行可能タイルを取得
        var walkable = cardinals.Where(p => map.IsInBounds(p) && map.IsWalkable(p)).ToList();

        // 歩行可能な隣接が2つ以下で、それらが互いに隣接していない場合、
        // この位置を塞ぐと分断される
        if (walkable.Count <= 2 && walkable.Count > 0)
        {
            // 2つの歩行可能タイルが対角（向かい合い）にある場合は通路上
            if (walkable.Count == 2)
            {
                var a = walkable[0];
                var b = walkable[1];
                // 向かい合い判定（差が2 = 反対側）
                if (Math.Abs(a.X - b.X) == 2 || Math.Abs(a.Y - b.Y) == 2)
                    return true;
            }
        }

        return false;
    }

    private static void AddWaterPuddle(DungeonMap map, Room room, Random random)
    {
        var center = room.GetRandomPosition(random);
        int size = random.Next(2, 4);

        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                if (dx * dx + dy * dy <= size * size)
                {
                    var pos = new Position(center.X + dx, center.Y + dy);
                    if (map.IsInBounds(pos) && map.GetTileType(pos) == TileType.Floor)
                    {
                        map.SetTile(pos, TileType.Water);
                    }
                }
            }
        }
    }
}

/// <summary>
/// 廊下生成アルゴリズム
/// </summary>
public static class CorridorGenerator
{
    /// <summary>
    /// L字型の廊下で2点を接続
    /// </summary>
    public static void ConnectRoomsL(DungeonMap map, Position from, Position to, Random random)
    {
        // 50%の確率で水平→垂直 or 垂直→水平
        if (random.NextDouble() < 0.5)
        {
            CarveHorizontalCorridor(map, from.X, to.X, from.Y);
            CarveVerticalCorridor(map, from.Y, to.Y, to.X);
        }
        else
        {
            CarveVerticalCorridor(map, from.Y, to.Y, from.X);
            CarveHorizontalCorridor(map, from.X, to.X, to.Y);
        }
    }

    /// <summary>
    /// ジグザグの廊下で2点を接続
    /// </summary>
    public static void ConnectRoomsZigzag(DungeonMap map, Position from, Position to, Random random)
    {
        int midX = (from.X + to.X) / 2 + random.Next(-3, 4);
        int midY = (from.Y + to.Y) / 2 + random.Next(-3, 4);

        // from → mid → to
        CarveHorizontalCorridor(map, from.X, midX, from.Y);
        CarveVerticalCorridor(map, from.Y, midY, midX);
        CarveHorizontalCorridor(map, midX, to.X, midY);
        CarveVerticalCorridor(map, midY, to.Y, to.X);
    }

    /// <summary>
    /// 直線の廊下で2点を接続（A*風のパス）
    /// </summary>
    public static void ConnectRoomsDirect(DungeonMap map, Position from, Position to)
    {
        var current = from;

        while (current != to)
        {
            map.SetTile(current, TileType.Corridor);

            int dx = Math.Sign(to.X - current.X);
            int dy = Math.Sign(to.Y - current.Y);

            // 対角線移動を優先
            if (dx != 0 && dy != 0)
            {
                if (Math.Abs(to.X - current.X) > Math.Abs(to.Y - current.Y))
                {
                    current = new Position(current.X + dx, current.Y);
                }
                else
                {
                    current = new Position(current.X, current.Y + dy);
                }
            }
            else if (dx != 0)
            {
                current = new Position(current.X + dx, current.Y);
            }
            else if (dy != 0)
            {
                current = new Position(current.X, current.Y + dy);
            }
        }

        map.SetTile(to, TileType.Corridor);
    }

    /// <summary>
    /// 水平方向に廊下を掘る
    /// </summary>
    public static void CarveHorizontalCorridor(DungeonMap map, int x1, int x2, int y)
    {
        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);

        for (int x = minX; x <= maxX; x++)
        {
            if (map.IsInBounds(x, y) && map.GetTileType(new Position(x, y)) != TileType.Floor)
            {
                map.SetTile(x, y, TileType.Corridor);
            }
        }
    }

    /// <summary>
    /// 垂直方向に廊下を掘る
    /// </summary>
    public static void CarveVerticalCorridor(DungeonMap map, int y1, int y2, int x)
    {
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);

        for (int y = minY; y <= maxY; y++)
        {
            if (map.IsInBounds(x, y) && map.GetTileType(new Position(x, y)) != TileType.Floor)
            {
                map.SetTile(x, y, TileType.Corridor);
            }
        }
    }

    /// <summary>
    /// 廊下の交差点や角にドアを配置
    /// </summary>
    public static void PlaceDoors(DungeonMap map, Random random, float doorChance = 0.3f)
    {
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                if (!IsDoorCandidate(map, pos)) continue;

                if (random.NextDouble() < doorChance)
                {
                    map.SetTile(pos, TileType.DoorClosed);
                }
            }
        }
    }

    /// <summary>
    /// ドア設置候補か判定（部屋と廊下の境界）
    /// </summary>
    private static bool IsDoorCandidate(DungeonMap map, Position pos)
    {
        var tile = map[pos];
        if (tile.Type != TileType.Corridor && tile.Type != TileType.Floor)
            return false;

        // 南北が壁で東西が通路、または東西が壁で南北が通路
        bool northSouthWalls =
            map[pos.X, pos.Y - 1].BlocksMovement &&
            map[pos.X, pos.Y + 1].BlocksMovement;
        bool eastWestOpen =
            !map[pos.X - 1, pos.Y].BlocksMovement &&
            !map[pos.X + 1, pos.Y].BlocksMovement;

        bool eastWestWalls =
            map[pos.X - 1, pos.Y].BlocksMovement &&
            map[pos.X + 1, pos.Y].BlocksMovement;
        bool northSouthOpen =
            !map[pos.X, pos.Y - 1].BlocksMovement &&
            !map[pos.X, pos.Y + 1].BlocksMovement;

        if (!((northSouthWalls && eastWestOpen) || (eastWestWalls && northSouthOpen)))
            return false;

        // 追加バリデーション: ドアの通路方向の先に実際に部屋/通路が続いているか確認
        // 壁方向のさらに先に通路可能タイルがなければ無意味なドア
        if (northSouthWalls && eastWestOpen)
        {
            // 東西方向に通路が開いている → 壁（南北）の向こう側にも通路があるか確認
            bool northHasPath = HasWalkableBeyondWall(map, pos.X, pos.Y - 1, 0, -1);
            bool southHasPath = HasWalkableBeyondWall(map, pos.X, pos.Y + 1, 0, 1);
            if (!northHasPath && !southHasPath)
                return false; // 壁の向こう側にまったく通路がない = 無意味なドア
        }
        else if (eastWestWalls && northSouthOpen)
        {
            bool westHasPath = HasWalkableBeyondWall(map, pos.X - 1, pos.Y, -1, 0);
            bool eastHasPath = HasWalkableBeyondWall(map, pos.X + 1, pos.Y, 1, 0);
            if (!westHasPath && !eastHasPath)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 壁の向こう側に歩行可能なタイルがあるか確認（最大3タイル先まで走査）
    /// </summary>
    private static bool HasWalkableBeyondWall(DungeonMap map, int startX, int startY, int dx, int dy)
    {
        for (int i = 0; i < 3; i++)
        {
            int x = startX + dx * i;
            int y = startY + dy * i;
            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                return false;
            var t = map[x, y];
            if (!t.BlocksMovement && i > 0) // 壁を越えた先に通路がある
                return true;
        }
        return false;
    }
}
