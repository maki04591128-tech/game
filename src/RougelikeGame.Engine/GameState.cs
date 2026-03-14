using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Map;
using RougelikeGame.Engine.Combat;

namespace RougelikeGame.Engine;

/// <summary>
/// ゲーム状態の実装
/// </summary>
public class GameState : IGameState
{
    public IPlayer Player { get; }
    public IMap CurrentMap { get; private set; }
    public ICombatSystem CombatSystem { get; }
    public IRandomProvider Random { get; }
    public CombatState CombatState { get; set; } = CombatState.Normal;
    public long CurrentTurn { get; set; }

    private readonly List<Core.Entities.Character> _enemies = new();
    public IReadOnlyList<Core.Entities.Character> Enemies => _enemies.AsReadOnly();

    public GameState(
        IPlayer player,
        IMap initialMap,
        ICombatSystem combatSystem,
        IRandomProvider random)
    {
        Player = player;
        CurrentMap = initialMap;
        CombatSystem = combatSystem;
        Random = random;
    }

    public float GetMovementModifier(IEntity entity)
    {
        float modifier = 1.0f;

        // 戦闘状態による修正
        if (CombatState.HasFlag(CombatState.Combat))
            modifier *= TurnCosts.MoveCombat;
        else if (CombatState.HasFlag(CombatState.Stealth))
            modifier *= TurnCosts.MoveStealth;
        else if (CombatState.HasFlag(CombatState.Pursuit))
            modifier *= TurnCosts.MovePursuit;
        else if (CombatState.HasFlag(CombatState.Alert))
            modifier *= TurnCosts.MoveAlert;

        return modifier;
    }

    public void AddEnemy(Core.Entities.Character enemy)
    {
        _enemies.Add(enemy);
    }

    public void RemoveEnemy(Core.Entities.Character enemy)
    {
        _enemies.Remove(enemy);
    }

    public void ChangeMap(IMap newMap)
    {
        CurrentMap = newMap;
        _enemies.Clear();
    }
}

/// <summary>
/// 基本的なマップ実装
/// </summary>
public class BasicMap : IMap
{
    private readonly bool[,] _walkable;
    private readonly float[,] _environmentModifiers;

    public int Width { get; }
    public int Height { get; }

    public BasicMap(int width, int height)
    {
        Width = width;
        Height = height;
        _walkable = new bool[width, height];
        _environmentModifiers = new float[width, height];

        // 全マスを歩行可能で初期化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _walkable[x, y] = true;
                _environmentModifiers[x, y] = 1.0f;
            }
        }
    }

    public bool IsWalkable(Position position)
    {
        if (!IsInBounds(position)) return false;
        return _walkable[position.X, position.Y];
    }

    public bool CanMoveTo(Position position)
    {
        return IsWalkable(position);  // TODO: エンティティ衝突チェック
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

            if (!IsWalkable(new Position(x0, y0)) && !(x0 == from.X && y0 == from.Y))
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
        return _environmentModifiers[position.X, position.Y];
    }

    public void SetWalkable(Position position, bool walkable)
    {
        if (IsInBounds(position))
        {
            _walkable[position.X, position.Y] = walkable;
        }
    }

    public void SetEnvironmentModifier(Position position, float modifier)
    {
        if (IsInBounds(position))
        {
            _environmentModifiers[position.X, position.Y] = modifier;
        }
    }

    private bool IsInBounds(Position position)
    {
        return position.X >= 0 && position.X < Width &&
               position.Y >= 0 && position.Y < Height;
    }

    // IMap interface implementations
    public bool InBounds(Position position) => IsInBounds(position);

    public bool BlocksSight(Position position)
    {
        if (!IsInBounds(position)) return true;
        return !_walkable[position.X, position.Y]; // 歩行不可=視界遮断とする簡易実装
    }

    public Tile GetTile(Position position)
    {
        if (!IsInBounds(position))
            return Tile.FromType(TileType.Wall);

        return _walkable[position.X, position.Y] 
            ? Tile.FromType(TileType.Floor) 
            : Tile.FromType(TileType.Wall);
    }

    public void SetTile(Position position, Tile tile)
    {
        if (IsInBounds(position))
        {
            _walkable[position.X, position.Y] = !tile.BlocksMovement;
        }
    }
}
