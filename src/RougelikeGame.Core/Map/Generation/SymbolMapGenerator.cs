using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// シンボルマップ生成器
/// 領地ごとの地上マップを生成する。Elona/The Door of Trithius風の
/// ロケーションシンボルが配置されたオーバーワールドマップ。
/// Ver.α: 12領地対応、可変サイズ(2300-5000マス)、複雑形状、村/町/都自動配置、
/// ランダムダンジョン(野盗のねぐら/ゴブリンの巣)生成、勢力影響マップ対応。
/// </summary>
public class SymbolMapGenerator
{
    /// <summary>シンボルマップの幅（後方互換性のためデフォルト値を維持）</summary>
    public const int MapWidth = 70;

    /// <summary>集落配置の最大試行回数</summary>
    private const int MaxSettlementPlacementAttempts = 300;

    /// <summary>ランダムダンジョン配置の最大試行回数</summary>
    private const int MaxRandomDungeonPlacementAttempts = 500;

    /// <summary>シンボルマップの高さ</summary>
    public const int MapHeight = 50;

    /// <summary>領地ごとのマップサイズ定義（幅×高さ、2300-5000マス範囲）</summary>
    private static readonly Dictionary<TerritoryId, (int Width, int Height)> TerritorySizes = new()
    {
        [TerritoryId.Capital]  = (70, 60),  // 4200マス（王都領は広い）
        [TerritoryId.Forest]   = (65, 55),  // 3575マス
        [TerritoryId.Mountain] = (60, 50),  // 3000マス
        [TerritoryId.Coast]    = (65, 50),  // 3250マス
        [TerritoryId.Southern] = (60, 55),  // 3300マス
        [TerritoryId.Frontier] = (70, 65),  // 4550マス（辺境は広大）
        [TerritoryId.Desert]   = (65, 55),  // 3575マス
        [TerritoryId.Swamp]    = (55, 50),  // 2750マス
        [TerritoryId.Tundra]   = (60, 50),  // 3000マス
        [TerritoryId.Lake]     = (55, 50),  // 2750マス
        [TerritoryId.Volcanic] = (50, 50),  // 2500マス
        [TerritoryId.Sacred]   = (50, 46),  // 2300マス（聖域は小さい）
    };

    /// <summary>
    /// 領地のマップサイズを取得する
    /// </summary>
    public static (int Width, int Height) GetTerritoryMapSize(TerritoryId territory)
    {
        return TerritorySizes.TryGetValue(territory, out var size) ? size : (70, 50);
    }

    /// <summary>
    /// 指定領地のシンボルマップを生成する
    /// </summary>
    public SymbolMapResult Generate(TerritoryId territory)
    {
        var (width, height) = GetTerritoryMapSize(territory);
        var locations = LocationDefinition.GetSymbolLocations(territory);
        var random = new Random(GetTerritorySeed(territory));

        var map = new DungeonMap(width, height)
        {
            Depth = 0,
            Name = $"{territory}_surface"
        };

        // 1. 外周を壁で埋める
        FillBorderWalls(map, territory);

        // 2. 複雑な形状マスクを生成（隣接領地と境界一致）
        var shapeMask = GenerateComplexShapeMask(territory, width, height, random);

        // 3. マスクに基づいて基本地形を敷く
        FillBaseTerrainWithMask(map, territory, shapeMask, random);

        // 4. 既存ロケーションを配置
        var locationPositions = PlaceLocations(map, locations, shapeMask, random);

        // 5. 村・町・都を自動配置
        var totalTiles = CountWalkableTiles(map, shapeMask);
        PlaceSettlements(map, territory, locationPositions, shapeMask, totalTiles, random);

        // 6. ランダムダンジョン（野盗のねぐら、ゴブリンの巣等）を配置
        PlaceRandomDungeons(map, territory, locationPositions, shapeMask, random);

        // 7. ロケーション間を道で接続
        ConnectLocationsWithRoads(map, locationPositions.Keys.ToList());

        // 8. 入口位置を設定（最初のロケーション）
        if (locationPositions.Count > 0)
        {
            var entrance = locationPositions.First().Key;
            map.SetEntrance(entrance);
        }

        return new SymbolMapResult(map, locationPositions);
    }

    /// <summary>領地IDから決定論的シードを生成</summary>
    private static int GetTerritorySeed(TerritoryId territory)
    {
        return (int)territory * 31337 + 42;
    }

    /// <summary>外周を通行不可タイルで埋める</summary>
    private static void FillBorderWalls(DungeonMap map, TerritoryId territory)
    {
        var obstacle = GetTerrainForTerritory(territory).obstacle;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                map.SetTile(x, y, obstacle);
            }
        }
    }

    /// <summary>
    /// 複雑な形状マスクを生成する。
    /// 隣接領地の境目はシード値で決定論的に生成し、同じ辺を共有する2領地で
    /// 同一のノイズカーブを使用して境界が一致するようにする。
    /// </summary>
    private static bool[,] GenerateComplexShapeMask(TerritoryId territory, int width, int height, Random random)
    {
        var mask = new bool[width, height];
        int margin = 2;

        double centerX = width / 2.0;
        double centerY = height / 2.0;
        double radiusX = (width - margin * 2) / 2.0;
        double radiusY = (height - margin * 2) / 2.0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < margin || x >= width - margin || y < margin || y >= height - margin)
                {
                    mask[x, y] = false;
                    continue;
                }

                double dx = (x - centerX) / radiusX;
                double dy = (y - centerY) / radiusY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                double angle = Math.Atan2(dy, dx);
                double noise = GetBorderNoise(angle, territory);

                double threshold = 0.85 + noise * 0.25;
                mask[x, y] = dist < threshold;
            }
        }

        AddComplexFeatures(mask, width, height, random);

        return mask;
    }

    /// <summary>角度に基づく境界ノイズ値を返す（-1.0～1.0）</summary>
    private static double GetBorderNoise(double angle, TerritoryId territory)
    {
        int baseSeed = (int)territory * 31337 + 42;

        double noise = 0;
        noise += 0.4 * Math.Sin(angle * 3 + baseSeed * 0.01);
        noise += 0.3 * Math.Sin(angle * 5 + baseSeed * 0.02);
        noise += 0.2 * Math.Sin(angle * 7 + baseSeed * 0.03);
        noise += 0.1 * Math.Sin(angle * 11 + baseSeed * 0.05);

        return noise;
    }

    /// <summary>形状にフィンガー（突起）と湾（凹み）を追加</summary>
    private static void AddComplexFeatures(bool[,] mask, int width, int height, Random random)
    {
        int featureCount = Math.Max(3, (width + height) / 10);

        for (int i = 0; i < featureCount; i++)
        {
            bool isProtrusion = random.NextDouble() < 0.5;
            int cx = random.Next(width / 4, width * 3 / 4);
            int cy = random.Next(height / 4, height * 3 / 4);
            int radius = random.Next(3, 7);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (nx < 2 || nx >= width - 2 || ny < 2 || ny >= height - 2) continue;
                    if (dx * dx + dy * dy > radius * radius) continue;

                    if (isProtrusion)
                    {
                        mask[nx, ny] = true;
                    }
                    else
                    {
                        double distFromCenter = Math.Sqrt(
                            Math.Pow((nx - width / 2.0) / (width / 2.0), 2) +
                            Math.Pow((ny - height / 2.0) / (height / 2.0), 2));
                        if (distFromCenter > 0.5)
                            mask[nx, ny] = false;
                    }
                }
            }
        }
    }

    /// <summary>歩行可能タイル数をカウント</summary>
    private static int CountWalkableTiles(DungeonMap map, bool[,] shapeMask)
    {
        int count = 0;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                if (shapeMask[x, y])
                    count++;
        return count;
    }

    /// <summary>マスクに基づいて領地タイプに応じた基本地形を生成</summary>
    private static void FillBaseTerrainWithMask(DungeonMap map, TerritoryId territory, bool[,] shapeMask, Random random)
    {
        var (primary, secondary, obstacle) = GetTerrainForTerritory(territory);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (!shapeMask[x, y])
                {
                    map.SetTile(x, y, obstacle);
                    continue;
                }

                double noise = random.NextDouble();
                TileType tileType;

                if (noise < 0.60)
                    tileType = primary;
                else if (noise < 0.85)
                    tileType = secondary;
                else
                    tileType = obstacle;

                map.SetTile(x, y, tileType);
            }
        }
    }

    /// <summary>領地タイプごとの地形構成を返す</summary>
    private static (TileType primary, TileType secondary, TileType obstacle) GetTerrainForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital  => (TileType.SymbolGrass, TileType.SymbolRoad, TileType.SymbolForest),
            TerritoryId.Forest   => (TileType.SymbolForest, TileType.SymbolGrass, TileType.SymbolWater),
            TerritoryId.Mountain => (TileType.SymbolGrass, TileType.SymbolMountain, TileType.SymbolMountain),
            TerritoryId.Coast    => (TileType.SymbolGrass, TileType.SymbolWater, TileType.SymbolWater),
            TerritoryId.Southern => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolWater),
            TerritoryId.Frontier => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolMountain),
            TerritoryId.Desert   => (TileType.SymbolGrass, TileType.SymbolMountain, TileType.SymbolMountain),
            TerritoryId.Swamp    => (TileType.SymbolGrass, TileType.SymbolWater, TileType.SymbolWater),
            TerritoryId.Tundra   => (TileType.SymbolGrass, TileType.SymbolMountain, TileType.SymbolMountain),
            TerritoryId.Lake     => (TileType.SymbolGrass, TileType.SymbolWater, TileType.SymbolWater),
            TerritoryId.Volcanic => (TileType.SymbolMountain, TileType.SymbolGrass, TileType.SymbolMountain),
            TerritoryId.Sacred   => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolForest),
            _ => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolMountain)
        };
    }

    /// <summary>ロケーションをマップ上に配置する</summary>
    private static Dictionary<Position, LocationDefinition> PlaceLocations(
        DungeonMap map,
        IReadOnlyList<LocationDefinition> locations,
        bool[,] shapeMask,
        Random random)
    {
        var positions = new Dictionary<Position, LocationDefinition>();
        var usedPositions = new HashSet<Position>();

        int margin = 4;
        int areaWidth = map.Width - margin * 2;
        int areaHeight = map.Height - margin * 2;

        foreach (var location in locations)
        {
            Position pos;
            int attempts = 0;

            do
            {
                int x = random.Next(margin, margin + areaWidth);
                int y = random.Next(margin, margin + areaHeight);
                pos = new Position(x, y);
                attempts++;
            }
            while (attempts < 200 && (
                !shapeMask[pos.X, pos.Y] ||
                usedPositions.Any(p => p.ChebyshevDistanceTo(pos) < 4)));

            var tileType = GetTileTypeForLocation(location.Type);
            map.SetTile(pos.X, pos.Y, tileType);
            EnsureAccessible(map, pos, shapeMask);

            positions[pos] = location;
            usedPositions.Add(pos);
        }

        return positions;
    }

    /// <summary>
    /// 村・町・都を自動配置する。
    /// 村: 総マス数/500箇所、町: 総マス数/1000箇所、都: 1箇所
    /// </summary>
    private static void PlaceSettlements(
        DungeonMap map, TerritoryId territory,
        Dictionary<Position, LocationDefinition> locationPositions,
        bool[,] shapeMask, int totalTiles, Random random)
    {
        int villageCount = Math.Max(1, totalTiles / 500);
        int townCount = Math.Max(1, totalTiles / 1000);
        int capitalCount = 1;

        var allPositions = new HashSet<Position>(locationPositions.Keys);
        int margin = 4;

        // 都（首都）を配置（マップ中心付近）
        for (int i = 0; i < capitalCount; i++)
        {
            var pos = FindSettlementPosition(map, shapeMask, allPositions, margin, random,
                preferCenter: true, minDistance: 8);
            if (pos.HasValue)
            {
                map.SetTile(pos.Value.X, pos.Value.Y, TileType.SymbolCapital);
                EnsureAccessible(map, pos.Value, shapeMask);

                var loc = new LocationDefinition(
                    $"{territory}_auto_capital_{i}",
                    GetCapitalName(territory),
                    $"{GetTerritoryDisplayName(territory)}の首都",
                    LocationType.Capital,
                    territory,
                    DangerLevel: 1);
                locationPositions[pos.Value] = loc;
                allPositions.Add(pos.Value);
            }
        }

        // 町を配置
        for (int i = 0; i < townCount; i++)
        {
            var pos = FindSettlementPosition(map, shapeMask, allPositions, margin, random,
                preferCenter: false, minDistance: 6);
            if (pos.HasValue)
            {
                map.SetTile(pos.Value.X, pos.Value.Y, TileType.SymbolTown);
                EnsureAccessible(map, pos.Value, shapeMask);

                var loc = new LocationDefinition(
                    $"{territory}_auto_town_{i}",
                    $"{GetTerritoryDisplayName(territory)}の町{i + 1}",
                    "地域の交易拠点",
                    LocationType.Town,
                    territory,
                    DangerLevel: 1);
                locationPositions[pos.Value] = loc;
                allPositions.Add(pos.Value);
            }
        }

        // 村を配置
        for (int i = 0; i < villageCount; i++)
        {
            var pos = FindSettlementPosition(map, shapeMask, allPositions, margin, random,
                preferCenter: false, minDistance: 4);
            if (pos.HasValue)
            {
                map.SetTile(pos.Value.X, pos.Value.Y, TileType.SymbolVillage);
                EnsureAccessible(map, pos.Value, shapeMask);

                var loc = new LocationDefinition(
                    $"{territory}_auto_village_{i}",
                    $"{GetTerritoryDisplayName(territory)}の村{i + 1}",
                    "小さな農村",
                    LocationType.Village,
                    territory,
                    DangerLevel: 1);
                locationPositions[pos.Value] = loc;
                allPositions.Add(pos.Value);
            }
        }
    }

    /// <summary>集落配置位置を探す</summary>
    private static Position? FindSettlementPosition(
        DungeonMap map, bool[,] shapeMask, HashSet<Position> usedPositions,
        int margin, Random random, bool preferCenter, int minDistance)
    {
        int width = map.Width;
        int height = map.Height;

        for (int attempt = 0; attempt < MaxSettlementPlacementAttempts; attempt++)
        {
            int x, y;
            if (preferCenter && attempt < 100)
            {
                x = width / 2 + random.Next(-width / 6, width / 6);
                y = height / 2 + random.Next(-height / 6, height / 6);
            }
            else
            {
                x = random.Next(margin, width - margin);
                y = random.Next(margin, height - margin);
            }

            if (x < 0 || x >= width || y < 0 || y >= height) continue;
            if (!shapeMask[x, y]) continue;

            var pos = new Position(x, y);
            if (usedPositions.Any(p => p.ChebyshevDistanceTo(pos) < minDistance)) continue;

            return pos;
        }
        return null;
    }

    /// <summary>
    /// ランダムダンジョン（野盗のねぐら、ゴブリンの巣等）を配置する。
    /// 条件: 村/町/都から50マス以上離れた場所、他ダンジョンから100マス以上離れた場所
    /// </summary>
    private static void PlaceRandomDungeons(
        DungeonMap map, TerritoryId territory,
        Dictionary<Position, LocationDefinition> locationPositions,
        bool[,] shapeMask, Random random)
    {
        var settlementPositions = locationPositions
            .Where(kv => kv.Value.Type is LocationType.Town or LocationType.Village
                or LocationType.Capital or LocationType.Facility or LocationType.ReligiousSite)
            .Select(kv => kv.Key)
            .ToList();

        var dungeonPositions = locationPositions
            .Where(kv => kv.Value.Type is LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest)
            .Select(kv => kv.Key)
            .ToList();

        var dungeonTypes = new[]
        {
            (name: "野盗のねぐら", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen, danger: 2),
            (name: "ゴブリンの巣", type: LocationType.GoblinNest, tile: TileType.SymbolGoblinNest, danger: 2),
            (name: "オーク族の砦", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen, danger: 3),
            (name: "盗賊団のアジト", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen, danger: 3),
            (name: "コボルドの穴", type: LocationType.GoblinNest, tile: TileType.SymbolGoblinNest, danger: 1),
        };

        int maxDungeons = Math.Max(2, (map.Width * map.Height) / 800);

        for (int i = 0; i < maxDungeons; i++)
        {
            var pos = FindRandomDungeonPosition(map, shapeMask, settlementPositions, dungeonPositions, random);
            if (pos == null) break;

            var dungeonDef = dungeonTypes[random.Next(dungeonTypes.Length)];
            int floors = random.Next(1, 4);

            map.SetTile(pos.Value.X, pos.Value.Y, dungeonDef.tile);
            EnsureAccessible(map, pos.Value, shapeMask);

            var loc = new LocationDefinition(
                $"{territory}_random_dungeon_{i}",
                dungeonDef.name,
                $"1～{floors}階層のダンジョン。クリアすると消滅する",
                dungeonDef.type,
                territory,
                MinLevel: Math.Max(1, dungeonDef.danger * 3),
                DangerLevel: dungeonDef.danger);

            locationPositions[pos.Value] = loc;
            dungeonPositions.Add(pos.Value);
        }
    }

    /// <summary>
    /// ランダムダンジョンの配置位置を探す。
    /// 集落からの最低距離は50マス（マップ対角線の1/3が上限）、
    /// 他ダンジョンからの最低距離は100マス（マップ対角線の1/2が上限）。
    /// 小さいマップではこれらの距離が自動的にスケールダウンされる。
    /// </summary>
    private static Position? FindRandomDungeonPosition(
        DungeonMap map, bool[,] shapeMask,
        List<Position> settlementPositions, List<Position> dungeonPositions,
        Random random)
    {
        int mapDiag = (int)Math.Sqrt(map.Width * map.Width + map.Height * map.Height);
        int settlementMinDist = Math.Min(50, mapDiag / 3);
        int dungeonMinDist = Math.Min(100, mapDiag / 2);

        for (int attempt = 0; attempt < MaxRandomDungeonPlacementAttempts; attempt++)
        {
            int x = random.Next(3, map.Width - 3);
            int y = random.Next(3, map.Height - 3);

            if (!shapeMask[x, y]) continue;

            var pos = new Position(x, y);

            // 集落からsettlementMinDist以上離れているか（Chebyshev距離）
            bool tooCloseToSettlement = settlementPositions.Any(s =>
                pos.ChebyshevDistanceTo(s) < settlementMinDist);
            if (tooCloseToSettlement) continue;

            // 他ダンジョンからdungeonMinDist以上離れているか（Chebyshev距離）
            bool tooCloseToDungeon = dungeonPositions.Any(d =>
                pos.ChebyshevDistanceTo(d) < dungeonMinDist);
            if (tooCloseToDungeon) continue;

            return pos;
        }
        return null;
    }

    /// <summary>ロケーション種別からタイルタイプを取得</summary>
    private static TileType GetTileTypeForLocation(LocationType type)
    {
        return type switch
        {
            LocationType.Town => TileType.SymbolTown,
            LocationType.Village => TileType.SymbolVillage,
            LocationType.Capital => TileType.SymbolCapital,
            LocationType.Facility => TileType.SymbolFacility,
            LocationType.ReligiousSite => TileType.SymbolShrine,
            LocationType.Field => TileType.SymbolField,
            LocationType.Dungeon => TileType.SymbolDungeon,
            LocationType.BanditDen => TileType.SymbolBanditDen,
            LocationType.GoblinNest => TileType.SymbolGoblinNest,
            _ => TileType.SymbolField
        };
    }

    /// <summary>指定位置の周囲1マスを歩行可能にする</summary>
    private static void EnsureAccessible(DungeonMap map, Position center, bool[,]? shapeMask = null)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(center.X + dx, center.Y + dy);
                if (map.IsInBounds(pos))
                {
                    if (shapeMask != null && pos.X >= 0 && pos.X < shapeMask.GetLength(0)
                        && pos.Y >= 0 && pos.Y < shapeMask.GetLength(1))
                        shapeMask[pos.X, pos.Y] = true;

                    var tile = map.GetTile(pos);
                    if (tile.BlocksMovement)
                    {
                        map.SetTile(pos.X, pos.Y, TileType.SymbolGrass);
                    }
                }
            }
        }
    }

    /// <summary>ロケーション間を道で接続する（最短経路法）</summary>
    private static void ConnectLocationsWithRoads(DungeonMap map, List<Position> positions)
    {
        if (positions.Count < 2) return;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            CarveRoad(map, positions[i], positions[i + 1]);
        }

        if (positions.Count >= 3)
        {
            CarveRoad(map, positions[^1], positions[0]);
        }
    }

    /// <summary>2点間をL字型の道で接続する</summary>
    private static void CarveRoad(DungeonMap map, Position from, Position to)
    {
        int x = from.X;
        int y = from.Y;

        while (x != to.X)
        {
            x += x < to.X ? 1 : -1;
            var pos = new Position(x, y);
            if (map.IsInBounds(pos))
            {
                var tile = map.GetTile(pos);
                if (!IsLocationTile(tile.Type))
                {
                    map.SetTile(x, y, TileType.SymbolRoad);
                }
            }
        }

        while (y != to.Y)
        {
            y += y < to.Y ? 1 : -1;
            var pos = new Position(x, y);
            if (map.IsInBounds(pos))
            {
                var tile = map.GetTile(pos);
                if (!IsLocationTile(tile.Type))
                {
                    map.SetTile(x, y, TileType.SymbolRoad);
                }
            }
        }
    }

    /// <summary>ロケーションシンボルかどうか判定</summary>
    public static bool IsLocationTile(TileType type)
    {
        return type is TileType.SymbolTown or TileType.SymbolDungeon
            or TileType.SymbolFacility or TileType.SymbolShrine
            or TileType.SymbolField or TileType.SymbolVillage
            or TileType.SymbolCapital or TileType.SymbolBanditDen
            or TileType.SymbolGoblinNest;
    }

    /// <summary>シンボルマップ用タイルかどうか判定</summary>
    public static bool IsSymbolMapTile(TileType type)
    {
        return type is TileType.SymbolGrass or TileType.SymbolForest
            or TileType.SymbolMountain or TileType.SymbolWater
            or TileType.SymbolRoad or TileType.SymbolTown
            or TileType.SymbolDungeon or TileType.SymbolFacility
            or TileType.SymbolShrine or TileType.SymbolField
            or TileType.SymbolVillage or TileType.SymbolCapital
            or TileType.SymbolBanditDen or TileType.SymbolGoblinNest;
    }

    /// <summary>領地名を日本語で取得</summary>
    private static string GetTerritoryDisplayName(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital  => "王都",
            TerritoryId.Forest   => "森林",
            TerritoryId.Mountain => "山岳",
            TerritoryId.Coast    => "海岸",
            TerritoryId.Southern => "南部",
            TerritoryId.Frontier => "辺境",
            TerritoryId.Desert   => "砂漠",
            TerritoryId.Swamp    => "沼沢",
            TerritoryId.Tundra   => "凍土",
            TerritoryId.Lake     => "湖水",
            TerritoryId.Volcanic => "火山",
            TerritoryId.Sacred   => "聖域",
            _ => "不明"
        };
    }

    /// <summary>領地の首都名を取得</summary>
    private static string GetCapitalName(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital  => "王都セントラル",
            TerritoryId.Forest   => "緑樹の都",
            TerritoryId.Mountain => "鉄床城",
            TerritoryId.Coast    => "港都マリーナ",
            TerritoryId.Southern => "サンライト城",
            TerritoryId.Frontier => "辺境砦",
            TerritoryId.Desert   => "砂都オアシス",
            TerritoryId.Swamp    => "水郷の里",
            TerritoryId.Tundra   => "氷壁砦",
            TerritoryId.Lake     => "湖都ミラージュ",
            TerritoryId.Volcanic => "溶鉄の城塞",
            TerritoryId.Sacred   => "光輝の聖都",
            _ => "首都"
        };
    }
}

/// <summary>
/// シンボルマップ生成結果
/// </summary>
public record SymbolMapResult(
    DungeonMap Map,
    Dictionary<Position, LocationDefinition> LocationPositions
);
