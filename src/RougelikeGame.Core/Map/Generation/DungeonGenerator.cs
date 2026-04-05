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

        // 施錠ドアを設定（閉じたドアの一部を施錠）
        PlaceLockedDoors(map, parameters);

        // 隠し通路を配置
        PlaceSecretDoors(map, parameters);

        // 階段を配置
        PlaceStairs(map);

        // 部屋を装飾
        DecorateRooms(map, parameters);

        // 罠を配置
        PlaceTraps(map, parameters);

        // 宝箱を配置
        PlaceChests(map, parameters);

        // AE-1/BB-4: 特殊タイル（Water/Lava）を配置
        PlaceEnvironmentTiles(map, parameters);

        // CG-2: 採集ノードを配置
        PlaceGatheringNodes(map, parameters);

        // ルーン碑文を配置（遺跡系ダンジョン）
        PlaceRuneInscriptions(map, parameters);

        // 最終チェック: 装飾後にも斜め専用箇所が生まれ得るので再度修正
        FixDiagonalOnlyTiles(map);

        return map;
    }

    /// <summary>
    /// BSPアルゴリズムで部屋を生成
    /// </summary>
    private void GenerateRooms(DungeonMap map, DungeonGenerationParameters parameters)
    {
        const int MinRooms = 5;
        const int MaxRetries = 5;

        for (int retry = 0; retry < MaxRetries; retry++)
        {
            var bspTree = new BSPNode(2, 2, map.Width - 4, map.Height - 4);

            // BSPで空間を分割（リトライ時はより積極的に分割）
            SplitBSP(bspTree, parameters.RoomCount, forceMinRooms: retry > 0);

            // 各リーフノードに部屋を作成
            var leaves = bspTree.GetLeaves();
            var rooms = new List<Room>();
            int roomId = 0;

            foreach (var leaf in leaves)
            {
                var room = CreateRoomInNode(leaf, roomId++);
                if (room != null)
                {
                    rooms.Add(room);
                }
            }

            // 最低部屋数を満たさない場合はリトライ（マップをリセット）
            if (rooms.Count < MinRooms && retry < MaxRetries - 1)
            {
                map.ClearRooms();
                continue;
            }

            foreach (var room in rooms)
            {
                map.AddRoom(room);
                CarveRoom(map, room);
            }

            // リトライ最終回でもMinRooms未満なら強制部屋配置
            if (rooms.Count < MinRooms)
            {
                PlaceFallbackRooms(map, MinRooms - rooms.Count, rooms.Count);
            }
            break;
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
            else
            {
                // DE-3: 単一部屋の場合は入口兼ボス部屋に
                rooms[0].Type = RoomType.Boss;
            }
        }
    }

    /// <summary>
    /// BSPノードを再帰的に分割
    /// </summary>
    private void SplitBSP(BSPNode node, int targetRooms, int depth = 0, bool forceMinRooms = false)
    {
        const int MinSize = 9; // Margin*2 + MinRoomSize = 4+5 = 9を保証
        const int MaxDepth = 6;

        if (depth >= MaxDepth) return;

        // 分割するかどうかの確率（forceMinRooms時は浅い階層で必ず分割）
        float splitChance = 1.0f - (depth * 0.15f);
        if (forceMinRooms && depth < 4)
            splitChance = 1.0f; // 浅い階層では必ず分割
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

        SplitBSP(node.Left, targetRooms, depth + 1, forceMinRooms);
        SplitBSP(node.Right, targetRooms, depth + 1, forceMinRooms);
    }

    /// <summary>
    /// BSPノード内に部屋を作成
    /// </summary>
    private Room? CreateRoomInNode(BSPNode node, int roomId)
    {
        const int MinRoomSize = 5; // 壁含みで5x5 → 内部3x3歩行面を保証
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
    /// BSPで部屋が不足した場合にマップの空き領域にフォールバック部屋を配置
    /// </summary>
    private void PlaceFallbackRooms(DungeonMap map, int count, int startId)
    {
        const int MaxAttempts = 100;
        int placed = 0;

        for (int attempt = 0; attempt < MaxAttempts && placed < count; attempt++)
        {
            int roomWidth = _random.Next(5, 9);
            int roomHeight = _random.Next(5, 7);
            int roomX = _random.Next(2, map.Width - roomWidth - 2);
            int roomY = _random.Next(2, map.Height - roomHeight - 2);

            // 既存の部屋と重ならないか確認
            bool overlaps = false;
            foreach (var existing in map.Rooms)
            {
                if (roomX < existing.X + existing.Width + 1 &&
                    roomX + roomWidth + 1 > existing.X &&
                    roomY < existing.Y + existing.Height + 1 &&
                    roomY + roomHeight + 1 > existing.Y)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                var room = new Room
                {
                    Id = startId + placed,
                    X = roomX,
                    Y = roomY,
                    Width = roomWidth,
                    Height = roomHeight
                };
                map.AddRoom(room);
                CarveRoom(map, room);
                placed++;
            }
        }
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
    /// 階段を配置（到達可能性を保証）
    /// </summary>
    private void PlaceStairs(DungeonMap map)
    {
        var entrance = map.GetEntranceRoom();
        var boss = map.GetBossRoom();

        Position? upPos = null;
        Position? downPos = null;

        if (entrance != null)
        {
            var pos = map.GetRandomWalkablePositionInRoom(entrance, _random);
            if (pos.HasValue)
            {
                map.SetStairsUp(pos.Value);
                map.SetEntrance(pos.Value);
                upPos = pos.Value;
            }
        }

        if (boss != null)
        {
            var pos = map.GetRandomWalkablePositionInRoom(boss, _random);
            if (pos.HasValue)
            {
                map.SetStairsDown(pos.Value);
                downPos = pos.Value;
            }
        }

        // BV-1/CO-1: entranceまたはbossがnull、または階段配置に失敗した場合のフォールバック
        if (!upPos.HasValue)
        {
            var fallbackRoom = map.Rooms.FirstOrDefault();
            if (fallbackRoom != null)
            {
                var pos = map.GetRandomWalkablePositionInRoom(fallbackRoom, _random);
                if (pos.HasValue)
                {
                    map.SetStairsUp(pos.Value);
                    map.SetEntrance(pos.Value);
                    upPos = pos.Value;
                }
            }
            // 最終フォールバック: 歩行可能タイルを線形探索
            if (!upPos.HasValue)
            {
                for (int y = 1; y < map.Height - 1 && !upPos.HasValue; y++)
                    for (int x = 1; x < map.Width - 1 && !upPos.HasValue; x++)
                        if (map.GetTileType(new Position(x, y)) == TileType.Floor)
                        {
                            var fp = new Position(x, y);
                            map.SetStairsUp(fp);
                            map.SetEntrance(fp);
                            upPos = fp;
                        }
            }
        }

        if (!downPos.HasValue)
        {
            var fallbackRoom = map.Rooms.LastOrDefault();
            if (fallbackRoom != null)
            {
                var pos = map.GetRandomWalkablePositionInRoom(fallbackRoom, _random);
                if (pos.HasValue)
                {
                    map.SetStairsDown(pos.Value);
                    downPos = pos.Value;
                }
            }
            // 最終フォールバック: 歩行可能タイルを逆方向に線形探索
            if (!downPos.HasValue)
            {
                for (int y = map.Height - 2; y > 0 && !downPos.HasValue; y--)
                    for (int x = map.Width - 2; x > 0 && !downPos.HasValue; x--)
                        if (map.GetTileType(new Position(x, y)) == TileType.Floor && new Position(x, y) != upPos)
                        {
                            var fp = new Position(x, y);
                            map.SetStairsDown(fp);
                            downPos = fp;
                        }
            }
        }

        // 到達可能性チェック: 上り階段から下り階段へ到達できるか確認
        if (upPos.HasValue && downPos.HasValue && !IsReachable(map, upPos.Value, downPos.Value))
        {
            // 到達不能の場合、入口部屋とボス部屋を直接接続する廊下を生成
            if (entrance != null && boss != null)
            {
                CorridorGenerator.ConnectRoomsDirect(map, entrance.Center, boss.Center);

                // 再度チェック、まだ到達不能なら追加で L字接続
                if (!IsReachable(map, upPos.Value, downPos.Value))
                {
                    CorridorGenerator.ConnectRoomsL(map, entrance.Center, boss.Center, _random);
                }
            }
        }
    }

    /// <summary>
    /// BFSで2点間の到達可能性をチェック（ドアは通過可能として扱う）
    /// </summary>
    private static bool IsReachable(DungeonMap map, Position from, Position to)
    {
        var visited = new HashSet<Position>();
        var queue = new Queue<Position>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == to) return true;

            foreach (var next in current.GetNeighbors(includeDiagonals: false))
            {
                if (!map.IsInBounds(next) || visited.Contains(next)) continue;

                var tileType = map.GetTileType(next);
                // 床、廊下、ドア（閉/開/施錠）、階段等は通過可能
                bool passable = tileType switch
                {
                    TileType.Floor => true,
                    TileType.Corridor => true,
                    TileType.DoorClosed => true,
                    TileType.DoorOpen => true,
                    TileType.StairsUp => true,
                    TileType.StairsDown => true,
                    TileType.Grass => true,
                    TileType.Altar => true,
                    TileType.Fountain => true,
                    TileType.SecretDoor => true,
                    _ => !map.GetTile(next).BlocksMovement
                };

                if (passable)
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return false;
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

        // AE-4/BB-1: 追加の特殊部屋タイプを割り当て
        if (normalRooms.Count > 2 && parameters.Depth >= 3)
        {
            var shopRoom = normalRooms[_random.Next(normalRooms.Count)];
            shopRoom.Type = RoomType.Shop;
            normalRooms.Remove(shopRoom);
        }

        if (normalRooms.Count > 2 && parameters.Depth >= 5)
        {
            var trapRoom = normalRooms[_random.Next(normalRooms.Count)];
            trapRoom.Type = RoomType.TrapRoom;
            normalRooms.Remove(trapRoom);
        }

        if (normalRooms.Count > 2 && parameters.Depth >= 7)
        {
            var libraryRoom = normalRooms[_random.Next(normalRooms.Count)];
            libraryRoom.Type = RoomType.Library;
            normalRooms.Remove(libraryRoom);
        }

        if (normalRooms.Count > 2 && parameters.Depth >= 10)
        {
            var prisonRoom = normalRooms[_random.Next(normalRooms.Count)];
            prisonRoom.Type = RoomType.Prison;
            normalRooms.Remove(prisonRoom);
        }

        if (normalRooms.Count > 2 && parameters.Depth >= 4)
        {
            var storageRoom = normalRooms[_random.Next(normalRooms.Count)];
            storageRoom.Type = RoomType.Storage;
            normalRooms.Remove(storageRoom);
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
    /// <summary>
    /// 閉じたドアの一部を施錠する
    /// </summary>
    private void PlaceLockedDoors(DungeonMap map, DungeonGenerationParameters parameters)
    {
        float lockChance = 0.1f + (parameters.Depth * 0.02f); // 階層が深いほど施錠率上昇

        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                if (map.GetTileType(new Position(x, y)) == TileType.DoorClosed &&
                    _random.NextDouble() < lockChance)
                {
                    var tile = map[x, y];
                    tile.IsLocked = true;
                    tile.LockDifficulty = 5 + parameters.Depth * 2; // 階層依存の解錠難易度
                }
            }
        }
    }

    /// <summary>
    /// 隠し通路（SecretDoor）を配置する
    /// 部屋の壁のうち、反対側が廊下または別の部屋に面しているものを候補にする
    /// </summary>
    private void PlaceSecretDoors(DungeonMap map, DungeonGenerationParameters parameters)
    {
        int secretDoorCount = Math.Max(0, parameters.Depth / 3); // 3階ごとに1つずつ増加
        if (secretDoorCount == 0 && _random.NextDouble() < 0.3)
            secretDoorCount = 1; // 浅い階層でも30%の確率で1つ配置

        var candidates = new List<Position>();

        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                var pos = new Position(x, y);
                if (map.GetTileType(pos) != TileType.Wall) continue;

                // 壁の両側が通行可能（部屋の壁が廊下/他の部屋に面している）
                bool horizontalPassage =
                    map.IsInBounds(x - 1, y) && !map[x - 1, y].BlocksMovement &&
                    map.IsInBounds(x + 1, y) && !map[x + 1, y].BlocksMovement;
                bool verticalPassage =
                    map.IsInBounds(x, y - 1) && !map[x, y - 1].BlocksMovement &&
                    map.IsInBounds(x, y + 1) && !map[x, y + 1].BlocksMovement;

                if (horizontalPassage || verticalPassage)
                {
                    candidates.Add(pos);
                }
            }
        }

        // 候補からランダムに選択して配置
        for (int i = 0; i < secretDoorCount && candidates.Count > 0; i++)
        {
            int idx = _random.Next(candidates.Count);
            var pos = candidates[idx];
            candidates.RemoveAt(idx);
            map.SetTile(pos, TileType.SecretDoor);
        }
    }

    private void PlaceTraps(DungeonMap map, DungeonGenerationParameters parameters)
    {
        int floorTiles = map.CountFloorTiles();
        int trapCount = (int)(floorTiles * parameters.TrapDensity);
        var trapTypes = TrapDefinition.AllTraps;

        for (int i = 0; i < trapCount; i++)
        {
            var pos = map.GetRandomWalkablePosition(_random);
            if (pos.HasValue && map.GetTileType(pos.Value) == TileType.Floor)
            {
                map.SetTile(pos.Value, TileType.TrapHidden);
                var trap = trapTypes[_random.Next(trapTypes.Length)];
                map[pos.Value].TrapId = trap.Type.ToString();
            }
        }
    }

    /// <summary>
    /// 宝箱を配置する。
    /// 各階層30%の確率で生成、1階層の上限は3個。
    /// ボス階は確定で1個追加配置。
    /// </summary>
    private void PlaceChests(DungeonMap map, DungeonGenerationParameters parameters)
    {
        const int MaxChestsPerFloor = 3;
        const double ChestSpawnChance = 0.30;

        int chestsPlaced = 0;

        // ボス階は確定で宝箱を1個配置
        if (parameters.IsBossFloor)
        {
            if (PlaceSingleChest(map, parameters.Depth, parameters.DungeonId))
                chestsPlaced++;
        }

        // 残りの宝箱を30%の確率で配置（上限まで）
        while (chestsPlaced < MaxChestsPerFloor)
        {
            if (_random.NextDouble() >= ChestSpawnChance) break;
            if (PlaceSingleChest(map, parameters.Depth, parameters.DungeonId))
                chestsPlaced++;
            else
                break;
        }
    }

    /// <summary>宝箱を1個配置し、中身のアイテムIDを設定する</summary>
    private bool PlaceSingleChest(DungeonMap map, int depth, string? dungeonId)
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            var pos = map.GetRandomWalkablePosition(_random);
            if (!pos.HasValue) continue;

            var tile = map[pos.Value];
            if (tile.Type != TileType.Floor) continue;

            map.SetTile(pos.Value, TileType.Chest);
            var chestTile = map[pos.Value];
            chestTile.ChestOpened = false;
            chestTile.ChestItems = GenerateChestItems(depth, dungeonId);
            chestTile.ChestLockDifficulty = _random.Next(3) == 0 ? 5 + depth * 2 : 0;
            return true;
        }
        return false;
    }

    /// <summary>階層・ダンジョンテーマに応じた宝箱の中身を生成</summary>
    private List<string> GenerateChestItems(int depth, string? dungeonId)
    {
        var items = new List<string>();
        int itemCount = 1 + _random.Next(3);

        // ダンジョンテーマ別のアイテムプール
        var (commonItems, uncommonItems, rareItems) = GetThemedItemPools(dungeonId);

        for (int i = 0; i < itemCount; i++)
        {
            double roll = _random.NextDouble();
            if (depth >= 5 && roll < 0.1)
                items.Add(rareItems[_random.Next(rareItems.Length)]);
            else if (depth >= 3 && roll < 0.35)
                items.Add(uncommonItems[_random.Next(uncommonItems.Length)]);
            else
                items.Add(commonItems[_random.Next(commonItems.Length)]);
        }

        if (depth >= 2 && _random.NextDouble() < 0.5)
        {
            items.Add($"gold_{50 + depth * 20 + _random.Next(100)}");
        }

        return items;
    }

    /// <summary>ダンジョンテーマに応じたアイテムプールを返す</summary>
    private static (string[] common, string[] uncommon, string[] rare) GetThemedItemPools(string? dungeonId)
    {
        // デフォルトプール
        var defaultCommon = new[] { "potion_healing_minor", "potion_mana_minor", "potion_antidote", "scroll_identify", "food_bread" };
        var defaultUncommon = new[] { "potion_healing", "potion_mana", "scroll_teleport", "potion_strength", "potion_agility" };
        var defaultRare = new[] { "potion_healing_super", "potion_cure_all", "scroll_enchant", "food_lembas", "accessory_protection_amulet" };

        return dungeonId switch
        {
            // 王都地下墓地 - アンデッド対策アイテム
            "capital_catacombs" => (
                new[] { "potion_healing_minor", "potion_antidote", "food_bread", "scroll_identify", "potion_mana_minor" },
                new[] { "potion_healing", "scroll_teleport", "potion_strength", "weapon_iron_sword", "shield_wooden" },
                new[] { "potion_healing_super", "scroll_enchant", "armor_chainmail", "weapon_steel_sword", "accessory_protection_amulet" }),

            // 始まりの裂け目 - バランス型高品質
            "capital_rift" => (
                new[] { "potion_healing_minor", "potion_mana_minor", "scroll_identify", "food_ration", "potion_antidote" },
                new[] { "potion_healing", "potion_mana", "scroll_teleport", "potion_agility", "scroll_fireball" },
                new[] { "potion_healing_super", "scroll_enchant", "weapon_greatsword", "armor_plate", "accessory_speed_cloak" }),

            // 腐敗の森 - 自然系・回復アイテム
            "forest_corruption" => (
                new[] { "potion_healing_minor", "potion_antidote", "food_fruit", "food_bread", "material_herb" },
                new[] { "potion_healing", "potion_mana", "food_lembas", "potion_antidote", "scroll_identify" },
                new[] { "potion_cure_all", "potion_healing_super", "weapon_wooden_staff", "armor_wizard_robe", "scroll_enchant" }),

            // 古代エルフの遺跡 - 魔法系アイテム
            "forest_ruins" => (
                new[] { "potion_mana_minor", "scroll_identify", "potion_healing_minor", "food_bread", "material_magic_crystal" },
                new[] { "potion_mana", "scroll_teleport", "scroll_fireball", "potion_agility", "weapon_wooden_staff" },
                new[] { "scroll_enchant", "armor_wizard_robe", "accessory_protection_amulet", "potion_healing_super", "scroll_magic_mapping" }),

            // 採掘坑 - 鉱石・装備系
            "mountain_mine" => (
                new[] { "potion_healing_minor", "food_bread", "material_iron_ore", "material_stone", "scroll_identify" },
                new[] { "potion_healing", "weapon_iron_sword", "armor_iron_helm", "armor_iron_boots", "shield_iron" },
                new[] { "weapon_steel_sword", "armor_plate", "weapon_battle_axe", "potion_strength", "scroll_enchant" }),

            // 溶岩洞 - 耐火・高品質装備
            "mountain_lava" => (
                new[] { "potion_healing_minor", "potion_fire_resist", "food_ration", "potion_antidote", "scroll_identify" },
                new[] { "potion_healing", "potion_fire_resist", "weapon_battle_axe", "armor_chainmail", "potion_strength" },
                new[] { "weapon_greatsword", "armor_plate", "accessory_protection_amulet", "potion_healing_super", "scroll_enchant" }),

            // 竜の巣 - 最高品質
            "mountain_dragon" => (
                new[] { "potion_healing", "potion_fire_resist", "food_ration", "potion_mana_minor", "scroll_identify" },
                new[] { "potion_healing_super", "potion_strength", "weapon_steel_sword", "armor_plate", "scroll_teleport" },
                new[] { "weapon_greatsword", "accessory_speed_cloak", "scroll_enchant", "potion_cure_all", "food_lembas" }),

            // 海岸洞窟 - 海賊の財宝
            "coast_cave" => (
                new[] { "potion_healing_minor", "food_water", "food_bread", "weapon_dagger", "scroll_identify" },
                new[] { "potion_healing", "weapon_dagger", "potion_agility", "accessory_iron_ring", "scroll_teleport" },
                new[] { "weapon_crossbow", "accessory_protection_amulet", "potion_healing_super", "scroll_enchant", "armor_leather" }),

            // 沈没船 - 水中探索系
            "coast_wreck" => (
                new[] { "potion_healing_minor", "food_water", "potion_cold_resist", "scroll_identify", "food_bread" },
                new[] { "potion_healing", "potion_cold_resist", "weapon_spear", "armor_leather", "scroll_teleport" },
                new[] { "accessory_protection_amulet", "weapon_crossbow", "potion_healing_super", "scroll_enchant", "accessory_speed_cloak" }),

            // 氷の洞窟 - 耐寒・防御系
            "southern_icecave" => (
                new[] { "potion_healing_minor", "potion_cold_resist", "food_ration", "scroll_identify", "food_bread" },
                new[] { "potion_healing", "potion_cold_resist", "armor_chainmail", "shield_iron", "potion_strength" },
                new[] { "armor_plate", "potion_healing_super", "accessory_protection_amulet", "scroll_enchant", "weapon_war_hammer" }),

            // 古戦場跡 - 武具・軍用品
            "southern_battlefield" => (
                new[] { "potion_healing_minor", "food_ration", "weapon_rusty_sword", "scroll_identify", "food_bread" },
                new[] { "potion_healing", "weapon_iron_sword", "armor_chainmail", "shield_iron", "potion_strength" },
                new[] { "weapon_greatsword", "armor_plate", "weapon_war_hammer", "scroll_enchant", "accessory_protection_amulet" }),

            // 大裂け目 - 最高難易度の報酬
            "frontier_great_rift" => (
                new[] { "potion_healing", "potion_mana", "food_ration", "scroll_identify", "potion_antidote" },
                new[] { "potion_healing_super", "potion_strength", "potion_agility", "scroll_teleport", "scroll_fireball" },
                new[] { "accessory_speed_cloak", "weapon_greatsword", "armor_plate", "scroll_enchant", "potion_cure_all" }),

            // 滅びた王国の遺跡 - 古代の遺物
            "frontier_ancient_ruins" => (
                new[] { "potion_mana_minor", "scroll_identify", "material_ancient_relic", "potion_healing_minor", "food_bread" },
                new[] { "potion_mana", "scroll_magic_mapping", "scroll_teleport", "weapon_wooden_staff", "armor_wizard_robe" },
                new[] { "scroll_enchant", "accessory_protection_amulet", "potion_healing_super", "potion_cure_all", "accessory_speed_cloak" }),

            _ => (defaultCommon, defaultUncommon, defaultRare)
        };
    }

    /// <summary>AE-1/BB-4: 環境タイル（Water/Lava/DeepWater/Tree）を配置する</summary>
    private void PlaceEnvironmentTiles(DungeonMap map, DungeonGenerationParameters parameters)
    {
        int floorTiles = map.CountFloorTiles();
        // 階層が深いほど環境タイルが増加
        int envTileCount = Math.Max(0, (int)(floorTiles * 0.02f * (1 + parameters.Depth * 0.1f)));

        for (int i = 0; i < envTileCount; i++)
        {
            var pos = map.GetRandomWalkablePosition(_random);
            if (!pos.HasValue || map.GetTileType(pos.Value) != TileType.Floor) continue;

            // 階層に応じたタイルタイプを選択
            TileType envType = parameters.Depth switch
            {
                < 5 => _random.Next(2) == 0 ? TileType.Water : TileType.Tree,
                < 15 => (_random.Next(3)) switch
                {
                    0 => TileType.Water,
                    1 => TileType.DeepWater,
                    _ => TileType.Pit
                },
                _ => (_random.Next(4)) switch
                {
                    0 => TileType.Lava,
                    1 => TileType.DeepWater,
                    2 => TileType.Pit,
                    _ => TileType.Water
                }
            };

            map.SetTile(pos.Value, envType);
        }
    }

    /// <summary>CG-2: 採集ノード（鉱石・薬草等）を配置する</summary>
    private void PlaceGatheringNodes(DungeonMap map, DungeonGenerationParameters parameters)
    {
        int nodeCount = 1 + parameters.Depth / 3; // 3階ごとに+1ノード

        for (int i = 0; i < nodeCount; i++)
        {
            var pos = map.GetRandomWalkablePosition(_random);
            if (!pos.HasValue || map.GetTileType(pos.Value) != TileType.Floor) continue;

            var tile = map[pos.Value];
            tile.GatheringNodeType = _random.Next(3) switch
            {
                0 => GatheringType.Mining,    // 鉱石
                1 => GatheringType.Herb,      // 薬草
                _ => GatheringType.Foraging   // 採集
            };
        }
    }

    /// <summary>遺跡・魔法系ダンジョンにルーン碑文を配置する</summary>
    private void PlaceRuneInscriptions(DungeonMap map, DungeonGenerationParameters parameters)
    {
        // 碑文が出現するダンジョン（遺跡・魔法関連）
        var inscriptionDungeons = new HashSet<string>
        {
            "capital_catacombs", "capital_rift",
            "forest_ruins", "forest_corruption",
            "mountain_mine",
            "southern_battlefield",
            "frontier_ancient_ruins", "frontier_great_rift"
        };

        string? dungeonId = parameters.DungeonId;
        bool isInscriptionDungeon = dungeonId != null && inscriptionDungeons.Contains(dungeonId);

        // 遺跡系以外でも低確率で出現（20%）
        if (!isInscriptionDungeon && _random.NextDouble() >= 0.20)
            return;

        int maxInscriptions = isInscriptionDungeon ? 2 + parameters.Depth / 5 : 1;
        int placed = 0;

        // ダンジョンテーマに応じた碑文ルーン語プール
        var wordPool = GetInscriptionWordPool(dungeonId, parameters.Depth);

        for (int i = 0; i < maxInscriptions && wordPool.Count > 0; i++)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                var pos = map.GetRandomWalkablePosition(_random);
                if (!pos.HasValue) continue;
                if (map[pos.Value].Type != TileType.Floor) continue;

                map.SetTile(pos.Value, TileType.RuneInscription);
                var tile = map[pos.Value];
                int wordIdx = _random.Next(wordPool.Count);
                tile.InscriptionWordId = wordPool[wordIdx];
                tile.InscriptionRead = false;
                wordPool.RemoveAt(wordIdx);
                placed++;
                break;
            }
        }
    }

    /// <summary>ダンジョンテーマに応じた碑文ルーン語プールを返す</summary>
    private List<string> GetInscriptionWordPool(string? dungeonId, int depth)
    {
        // 深層ほど高難度の語が出る
        int maxDifficulty = Math.Clamp(1 + depth / 3, 1, 5);

        // ルーン語ID → 難易度のローカルマッピング（RuneWordDatabaseへの参照を避ける）
        var wordDifficulty = new Dictionary<string, int>
        {
            ["brenna"] = 1, ["frysta"] = 1, ["thruma"] = 1, ["brjota"] = 1, ["snida"] = 1,
            ["graeda"] = 1, ["verja"] = 1, ["sja"] = 1, ["sjalfr"] = 1, ["fjandi"] = 1,
            ["eldr"] = 1, ["vatn"] = 1, ["jord_elem"] = 1, ["vindr"] = 1, ["litill"] = 1,
            ["einn"] = 1, ["augnablik"] = 1, ["hlutr"] = 1, ["jord"] = 1,
            ["stinga"] = 2, ["springa"] = 2, ["hreinsa"] = 2, ["styrkja"] = 2, ["hrada"] = 2,
            ["binda"] = 2, ["sofa"] = 2, ["hraeda"] = 2, ["vita"] = 2, ["opna"] = 2, ["loka"] = 2,
            ["ovinir"] = 2, ["vinir"] = 2, ["draugr"] = 2, ["thurs"] = 2,
            ["iss"] = 2, ["thruma_elem"] = 2, ["mikill"] = 2, ["sterkr"] = 2,
            ["skjotr"] = 2, ["medal"] = 1, ["rett"] = 2, ["stund"] = 2, ["langr"] = 2,
            ["beinn"] = 2, ["hringr"] = 2, ["gegn"] = 2, ["tha"] = 2,
            ["eyda"] = 3, ["hylja"] = 3, ["blessa"] = 3, ["villa"] = 3, ["kalla"] = 3,
            ["senda"] = 3, ["ljos"] = 3, ["myrkr"] = 3, ["helgr"] = 3, ["bolvadr"] = 3,
            ["ofr"] = 3, ["viss"] = 3, ["hradr"] = 3, ["vidr"] = 3, ["eilifr"] = 3,
            ["ef"] = 3, ["sar"] = 3, ["allir"] = 3,
            ["tortima"] = 4, ["granda"] = 4, ["styra"] = 4, ["afrita"] = 4, ["banna"] = 4,
            ["thegar"] = 4, ["heimr"] = 4, ["daudr"] = 4, ["endalauss"] = 4,
            ["vekja"] = 5, ["snua"] = 5, ["ragnarok"] = 5
        };

        List<string> candidates = dungeonId switch
        {
            "forest_ruins" or "frontier_ancient_ruins" =>
                new() { "vita", "sja", "opna", "loka", "afrita", "banna", "styra", "ljos", "myrkr", "helgr", "eilifr", "heimr" },
            "capital_catacombs" or "southern_battlefield" =>
                new() { "eyda", "draugr", "myrkr", "bolvadr", "hraeda", "sofa", "daudr" },
            "capital_rift" or "frontier_great_rift" =>
                new() { "tortima", "granda", "snua", "banna", "ragnarok", "heimr", "endalauss" },
            "forest_corruption" =>
                new() { "hreinsa", "graeda", "blessa", "vindr", "jord_elem", "styrkja" },
            "mountain_mine" =>
                new() { "brjota", "jord_elem", "sterkr", "hlutr", "mikill" },
            _ => new() { "brenna", "frysta", "graeda", "verja", "sja", "fjandi", "eldr", "vatn" }
        };

        return candidates.Where(id => wordDifficulty.TryGetValue(id, out int d) && d <= maxDifficulty).ToList();
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
