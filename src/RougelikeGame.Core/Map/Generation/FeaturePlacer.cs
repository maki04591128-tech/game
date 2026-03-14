namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// マップ上の特徴物（アイテム、敵、インタラクティブオブジェクト）の配置
/// </summary>
public class FeaturePlacer
{
    private readonly Random _random;

    public FeaturePlacer(Random random)
    {
        _random = random;
    }

    /// <summary>
    /// 敵の配置位置を決定
    /// </summary>
    public List<EnemySpawnPoint> PlaceEnemies(DungeonMap map, float density, int depth)
    {
        var spawnPoints = new List<EnemySpawnPoint>();
        int floorTiles = map.CountFloorTiles();
        int enemyCount = Math.Max(3, (int)(floorTiles * density));

        var entrance = map.GetEntranceRoom();

        for (int i = 0; i < enemyCount; i++)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var pos = map.GetRandomWalkablePosition(_random);
                if (!pos.HasValue) continue;

                // 入口付近には配置しない
                if (entrance != null && entrance.Center.ChebyshevDistanceTo(pos.Value) < 5)
                    continue;

                // 階段上には配置しない
                var tileType = map.GetTileType(pos.Value);
                if (tileType == TileType.StairsUp || tileType == TileType.StairsDown)
                    continue;

                // 既存のスポーンポイントから離れている
                bool tooClose = spawnPoints.Any(sp => sp.Position.ChebyshevDistanceTo(pos.Value) < 3);
                if (tooClose) continue;

                var room = map.GetRoomAt(pos.Value);
                var spawnPoint = new EnemySpawnPoint
                {
                    Position = pos.Value,
                    IsBoss = room?.Type == RoomType.Boss,
                    IsElite = room?.Type == RoomType.Treasure && _random.NextDouble() < 0.5,
                    Depth = depth
                };

                spawnPoints.Add(spawnPoint);
                break;
            }
        }

        return spawnPoints;
    }

    /// <summary>
    /// アイテムの配置位置を決定
    /// </summary>
    public List<ItemSpawnPoint> PlaceItems(DungeonMap map, float density)
    {
        var spawnPoints = new List<ItemSpawnPoint>();
        int floorTiles = map.CountFloorTiles();
        int itemCount = Math.Max(5, (int)(floorTiles * density));

        for (int i = 0; i < itemCount; i++)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var pos = map.GetRandomWalkablePosition(_random);
                if (!pos.HasValue) continue;

                var tileType = map.GetTileType(pos.Value);

                // 宝箱があれば中にアイテム
                if (tileType == TileType.Chest)
                {
                    spawnPoints.Add(new ItemSpawnPoint
                    {
                        Position = pos.Value,
                        IsInChest = true,
                        Quality = ItemQuality.Uncommon
                    });
                    break;
                }

                // 床にアイテム
                if (tileType == TileType.Floor || tileType == TileType.Corridor)
                {
                    var room = map.GetRoomAt(pos.Value);
                    var quality = DetermineItemQuality(room?.Type);

                    spawnPoints.Add(new ItemSpawnPoint
                    {
                        Position = pos.Value,
                        IsInChest = false,
                        Quality = quality
                    });
                    break;
                }
            }
        }

        return spawnPoints;
    }

    /// <summary>
    /// 部屋タイプに応じたアイテム品質を決定
    /// </summary>
    private ItemQuality DetermineItemQuality(RoomType? roomType)
    {
        if (roomType == RoomType.Treasure)
        {
            return _random.NextDouble() switch
            {
                < 0.3 => ItemQuality.Rare,
                < 0.7 => ItemQuality.Uncommon,
                _ => ItemQuality.Common
            };
        }

        if (roomType == RoomType.Boss)
        {
            return _random.NextDouble() switch
            {
                < 0.5 => ItemQuality.Rare,
                < 0.9 => ItemQuality.Uncommon,
                _ => ItemQuality.Common
            };
        }

        return _random.NextDouble() switch
        {
            < 0.05 => ItemQuality.Rare,
            < 0.2 => ItemQuality.Uncommon,
            _ => ItemQuality.Common
        };
    }

    /// <summary>
    /// 隠し部屋を配置
    /// </summary>
    public void PlaceSecretRooms(DungeonMap map, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            // 既存の壁に隣接した場所を探す
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = _random.Next(5, map.Width - 10);
                int y = _random.Next(5, map.Height - 10);

                if (CanPlaceSecretRoom(map, x, y, 5, 5))
                {
                    CreateSecretRoom(map, x, y, 5, 5);
                    break;
                }
            }
        }
    }

    private bool CanPlaceSecretRoom(DungeonMap map, int x, int y, int width, int height)
    {
        // 全て壁であることを確認
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (map.GetTileType(new Position(x + dx, y + dy)) != TileType.Wall)
                    return false;
            }
        }

        // 隣接する廊下または部屋があることを確認
        bool hasAdjacent = false;
        for (int dx = 0; dx < width; dx++)
        {
            if (!map[x + dx, y - 1].BlocksMovement) hasAdjacent = true;
            if (!map[x + dx, y + height].BlocksMovement) hasAdjacent = true;
        }
        for (int dy = 0; dy < height; dy++)
        {
            if (!map[x - 1, y + dy].BlocksMovement) hasAdjacent = true;
            if (!map[x + width, y + dy].BlocksMovement) hasAdjacent = true;
        }

        return hasAdjacent;
    }

    private void CreateSecretRoom(DungeonMap map, int x, int y, int width, int height)
    {
        var room = new Room
        {
            Id = map.Rooms.Count,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Type = RoomType.Secret
        };

        // 部屋を掘る
        for (int dx = 1; dx < width - 1; dx++)
        {
            for (int dy = 1; dy < height - 1; dy++)
            {
                map.SetTile(x + dx, y + dy, TileType.Floor);
                map[x + dx, y + dy].RoomId = room.Id;
            }
        }

        // 秘密のドアを配置
        PlaceSecretDoor(map, x, y, width, height);

        map.AddRoom(room);
    }

    private void PlaceSecretDoor(DungeonMap map, int x, int y, int width, int height)
    {
        var candidates = new List<Position>();

        // 上下の壁をチェック
        for (int dx = 1; dx < width - 1; dx++)
        {
            if (!map[x + dx, y - 1].BlocksMovement)
                candidates.Add(new Position(x + dx, y));
            if (!map[x + dx, y + height - 1].BlocksMovement)
                candidates.Add(new Position(x + dx, y + height - 1));
        }

        // 左右の壁をチェック
        for (int dy = 1; dy < height - 1; dy++)
        {
            if (!map[x - 1, y + dy].BlocksMovement)
                candidates.Add(new Position(x, y + dy));
            if (!map[x + width - 1, y + dy].BlocksMovement)
                candidates.Add(new Position(x + width - 1, y + dy));
        }

        if (candidates.Count > 0)
        {
            var doorPos = candidates[_random.Next(candidates.Count)];
            map.SetTile(doorPos, TileType.SecretDoor);
        }
    }
}

/// <summary>
/// 敵のスポーンポイント
/// </summary>
public class EnemySpawnPoint
{
    public Position Position { get; init; }
    public bool IsBoss { get; init; }
    public bool IsElite { get; init; }
    public int Depth { get; init; }
    public string? EnemyTypeId { get; set; }
}

/// <summary>
/// アイテムのスポーンポイント
/// </summary>
public class ItemSpawnPoint
{
    public Position Position { get; init; }
    public bool IsInChest { get; init; }
    public ItemQuality Quality { get; init; }
    public string? ItemId { get; set; }
}

/// <summary>
/// アイテム品質
/// </summary>
public enum ItemQuality
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
