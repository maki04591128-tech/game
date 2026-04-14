using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Map;

/// <summary>
/// ダンジョンマップ
/// </summary>
public class DungeonMap : IMap
{
    private readonly Tile[,] _tiles;
    private readonly List<Room> _rooms = new();

    public int Width { get; }
    public int Height { get; }
    public int Depth { get; init; } = 1;

    /// <summary>マップの管理用名前（種族・素性に応じた開始マップ名等）</summary>
    public string Name { get; set; } = "";

    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    public Position? StairsUpPosition { get; private set; }
    public Position? StairsDownPosition { get; private set; }
    public Position? EntrancePosition { get; private set; }

    public DungeonMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        // 全タイルを壁で初期化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _tiles[x, y] = Tile.FromType(TileType.Wall);
            }
        }
    }

    #region Tile Access

    public Tile this[int x, int y]
    {
        get => IsInBounds(x, y) ? _tiles[x, y] : Tile.FromType(TileType.Wall);
        set
        {
            if (IsInBounds(x, y))
                _tiles[x, y] = value;
        }
    }

    public Tile this[Position pos]
    {
        get => this[pos.X, pos.Y];
        set => this[pos.X, pos.Y] = value;
    }

    public TileType GetTileType(Position pos) => this[pos].Type;

    public void SetTile(Position pos, TileType type)
    {
        if (IsInBounds(pos))
        {
            _tiles[pos.X, pos.Y] = Tile.FromType(type);
        }
    }

    public void SetTile(int x, int y, TileType type)
    {
        SetTile(new Position(x, y), type);
    }

    /// <summary>
    /// タイルを高度付きで設定する（シンボルマップ用）
    /// </summary>
    public void SetTileWithAltitude(int x, int y, TileType type, int altitude)
    {
        var pos = new Position(x, y);
        if (IsInBounds(pos))
        {
            var tile = Tile.FromType(type);
            tile.SetAltitude(altitude);
            _tiles[pos.X, pos.Y] = tile;
        }
    }

    #endregion

    #region Room Management

    public void AddRoom(Room room)
    {
        _rooms.Add(room);
    }

    public Room? GetRoomAt(Position pos)
    {
        return _rooms.FirstOrDefault(r => r.Contains(pos));
    }

    public Room? GetRoomById(int id)
    {
        return _rooms.FirstOrDefault(r => r.Id == id);
    }

    public Room? GetEntranceRoom()
    {
        return _rooms.FirstOrDefault(r => r.Type == RoomType.Entrance);
    }

    public Room? GetBossRoom()
    {
        return _rooms.FirstOrDefault(r => r.Type == RoomType.Boss);
    }

    /// <summary>部屋リストをクリアし、マップタイルを壁にリセット</summary>
    public void ClearRooms()
    {
        _rooms.Clear();
        StairsUpPosition = null;
        StairsDownPosition = null;
        EntrancePosition = null;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _tiles[x, y] = Tile.FromType(TileType.Wall);
            }
        }
    }

    #endregion

    #region Stairs

    public void SetStairsUp(Position pos)
    {
        SetTile(pos, TileType.StairsUp);
        StairsUpPosition = pos;
    }

    public void SetStairsDown(Position pos)
    {
        SetTile(pos, TileType.StairsDown);
        StairsDownPosition = pos;
    }

    public void SetEntrance(Position pos)
    {
        EntrancePosition = pos;
    }

    #endregion

    #region IMap Implementation

    public bool InBounds(Position position) => IsInBounds(position);

    public bool BlocksSight(Position position)
    {
        if (!IsInBounds(position)) return true;
        return _tiles[position.X, position.Y].BlocksSight;
    }

    public Tile GetTile(Position position) => this[position];

    public void SetTile(Position position, Tile tile)
    {
        if (IsInBounds(position))
        {
            _tiles[position.X, position.Y] = tile;
        }
    }

    public bool IsWalkable(Position position)
    {
        if (!IsInBounds(position)) return false;
        return !_tiles[position.X, position.Y].BlocksMovement;
    }

    public bool CanMoveTo(Position position)
    {
        // タイルレベルの移動可否判定のみ。エンティティ衝突チェックはGameControllerで実施
        return IsWalkable(position);
    }

    public bool HasLineOfSight(Position from, Position to)
    {
        // Bresenham's line algorithm
        int x0 = from.X, y0 = from.Y;
        int x1 = to.X, y1 = to.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 == x1 && y0 == y1) return true;

            var tile = this[x0, y0];
            if (tile.BlocksSight && !(x0 == from.X && y0 == from.Y))
            {
                return false;
            }

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public float GetEnvironmentModifier(Position position, TurnActionType actionType)
    {
        if (!IsInBounds(position)) return 1.0f;

        var tile = _tiles[position.X, position.Y];
        return tile.MovementCost;
    }

    #endregion

    #region Visibility (FOV)

    /// <summary>
    /// 視界をリセット
    /// </summary>
    public void ResetVisibility()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _tiles[x, y].IsVisible = false;
            }
        }
    }

    /// <summary>
    /// 全タイルを可視・探索済みにする（町など安全な場所用）
    /// </summary>
    public void RevealAll()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _tiles[x, y].IsVisible = true;
                _tiles[x, y].IsExplored = true;
            }
        }
    }

    /// <summary>
    /// 指定位置からの視界を計算（簡易版：円形視界）
    /// </summary>
    public void ComputeFov(Position center, int radius)
    {
        ResetVisibility();

        for (int x = center.X - radius; x <= center.X + radius; x++)
        {
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                if (!IsInBounds(x, y)) continue;

                var pos = new Position(x, y);
                if (center.ChebyshevDistanceTo(pos) > radius) continue;

                if (HasLineOfSight(center, pos))
                {
                    _tiles[x, y].IsVisible = true;
                    _tiles[x, y].IsExplored = true;
                }
            }
        }
    }

    #endregion

    #region Utility

    public bool IsInBounds(Position pos) => IsInBounds(pos.X, pos.Y);

    public bool IsInBounds(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>
    /// マップを文字列として出力（デバッグ用）
    /// </summary>
    public string ToDebugString(Position? playerPos = null)
    {
        var sb = new System.Text.StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (playerPos.HasValue && playerPos.Value.X == x && playerPos.Value.Y == y)
                {
                    sb.Append('@');
                }
                else
                {
                    sb.Append(_tiles[x, y].DisplayChar);
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// 歩行可能なランダムな位置を取得
    /// </summary>
    public Position? GetRandomWalkablePosition(Random random, int maxAttempts = 100)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            int x = random.Next(1, Width - 1);
            int y = random.Next(1, Height - 1);
            var pos = new Position(x, y);

            if (IsWalkable(pos))
            {
                return pos;
            }
        }

        return null;
    }

    /// <summary>
    /// 歩行可能なランダムな位置を取得（IRandomProvider版）
    /// </summary>
    public Position? GetRandomWalkablePosition(IRandomProvider random, int maxAttempts = 100)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            int x = random.Next(1, Width - 1);
            int y = random.Next(1, Height - 1);
            var pos = new Position(x, y);

            if (IsWalkable(pos))
            {
                return pos;
            }
        }

        return null;
    }

    /// <summary>
    /// 指定した部屋内の歩行可能なランダムな位置を取得
    /// </summary>
    public Position? GetRandomWalkablePositionInRoom(Room room, Random random)
    {
        for (int i = 0; i < 50; i++)
        {
            var pos = room.GetRandomPosition(random);
            if (IsWalkable(pos))
            {
                return pos;
            }
        }

        return null;
    }

    /// <summary>
    /// 床タイルの総数を取得
    /// </summary>
    public int CountFloorTiles()
    {
        int count = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_tiles[x, y].Type == TileType.Floor ||
                    _tiles[x, y].Type == TileType.Corridor)
                {
                    count++;
                }
            }
        }
        return count;
    }

    #endregion
}
