namespace RougelikeGame.Core;

/// <summary>
/// 2次元座標を表す値オブジェクト
/// </summary>
public readonly record struct Position(int X, int Y)
{
    public static Position Zero => new(0, 0);

    /// <summary>
    /// マンハッタン距離
    /// </summary>
    public int DistanceTo(Position other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    /// <summary>
    /// ユークリッド距離
    /// </summary>
    public double EuclideanDistanceTo(Position other) =>
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

    /// <summary>
    /// チェビシェフ距離（8方向移動での距離）
    /// </summary>
    public int ChebyshevDistanceTo(Position other) =>
        Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

    /// <summary>
    /// 指定方向に移動した座標を返す
    /// </summary>
    public Position Move(Direction direction) => direction switch
    {
        Direction.North => this with { Y = Y - 1 },
        Direction.South => this with { Y = Y + 1 },
        Direction.East => this with { X = X + 1 },
        Direction.West => this with { X = X - 1 },
        Direction.NorthEast => new Position(X + 1, Y - 1),
        Direction.NorthWest => new Position(X - 1, Y - 1),
        Direction.SouthEast => new Position(X + 1, Y + 1),
        Direction.SouthWest => new Position(X - 1, Y + 1),
        _ => this
    };

    /// <summary>
    /// 隣接マスを取得
    /// </summary>
    public IEnumerable<Position> GetNeighbors(bool includeDiagonals = false)
    {
        yield return Move(Direction.North);
        yield return Move(Direction.South);
        yield return Move(Direction.East);
        yield return Move(Direction.West);

        if (includeDiagonals)
        {
            yield return Move(Direction.NorthEast);
            yield return Move(Direction.NorthWest);
            yield return Move(Direction.SouthEast);
            yield return Move(Direction.SouthWest);
        }
    }

    /// <summary>
    /// 対象位置への方向を取得
    /// </summary>
    public Direction GetDirectionTo(Position target)
    {
        int dx = target.X - X;
        int dy = target.Y - Y;

        if (dx == 0 && dy == 0) return Direction.North;

        // 8方向の判定
        if (dx > 0 && dy < 0) return Direction.NorthEast;
        if (dx < 0 && dy < 0) return Direction.NorthWest;
        if (dx > 0 && dy > 0) return Direction.SouthEast;
        if (dx < 0 && dy > 0) return Direction.SouthWest;
        if (dy < 0) return Direction.North;
        if (dy > 0) return Direction.South;
        if (dx > 0) return Direction.East;
        return Direction.West;
    }

    public static Position operator +(Position a, Position b) =>
        new(a.X + b.X, a.Y + b.Y);

    public static Position operator -(Position a, Position b) =>
        new(a.X - b.X, a.Y - b.Y);

    public override string ToString() => $"({X}, {Y})";
}

/// <summary>
/// 方向の拡張メソッド
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// 斜め方向かどうか
    /// </summary>
    public static bool IsDiagonal(this Direction direction) =>
        direction is Direction.NorthEast or Direction.NorthWest
                  or Direction.SouthEast or Direction.SouthWest;

    /// <summary>
    /// 反対方向を取得
    /// </summary>
    public static Direction Opposite(this Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        Direction.NorthEast => Direction.SouthWest,
        Direction.NorthWest => Direction.SouthEast,
        Direction.SouthEast => Direction.NorthWest,
        Direction.SouthWest => Direction.NorthEast,
        _ => direction
    };

    /// <summary>
    /// 時計回りに90度回転
    /// </summary>
    public static Direction RotateClockwise(this Direction direction) => direction switch
    {
        Direction.North => Direction.East,
        Direction.East => Direction.South,
        Direction.South => Direction.West,
        Direction.West => Direction.North,
        Direction.NorthEast => Direction.SouthEast,
        Direction.SouthEast => Direction.SouthWest,
        Direction.SouthWest => Direction.NorthWest,
        Direction.NorthWest => Direction.NorthEast,
        _ => direction
    };
}
