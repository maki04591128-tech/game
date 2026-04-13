using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// シンボルマップ生成器
/// 領地ごとの地上マップを生成する。Elona/The Door of Trithius風の
/// ロケーションシンボルが配置されたオーバーワールドマップ。
/// Ver.α: 12領地対応、可変サイズ(23000-50000マス)、複雑形状、村/町/都自動配置、
/// ランダムダンジョン(野盗のねぐら/ゴブリンの巣)生成、勢力影響マップ対応。
/// </summary>
public class SymbolMapGenerator
{
    /// <summary>シンボルマップの幅（後方互換性のためデフォルト値を維持）</summary>
    public const int MapWidth = 220;

    /// <summary>シンボルマップの高さ</summary>
    public const int MapHeight = 160;

    /// <summary>シンボルマップ上の視界半径</summary>
    public const int SymbolMapFovRadius = 40;

    /// <summary>集落配置の最大試行回数</summary>
    private const int MaxSettlementPlacementAttempts = 1000;

    /// <summary>ランダムダンジョン配置の最大試行回数</summary>
    private const int MaxRandomDungeonPlacementAttempts = 2000;

    /// <summary>固定ロケーション配置の最大試行回数</summary>
    private const int MaxLocationPlacementAttempts = 500;

    /// <summary>村1つあたりの必要マス数（総マス数÷この値＝村数）</summary>
    private const int TilesPerVillage = 500;

    /// <summary>町1つあたりの必要マス数（総マス数÷この値＝町数）</summary>
    private const int TilesPerTown = 1000;

    /// <summary>領地形状マスクの丸み制御閾値（0.0=最も尖る、1.0=完全な楕円）</summary>
    private const double ShapeMaskRoundness = 0.85;

    /// <summary>領地形状マスクのノイズ強度（境界の凹凸の大きさ）</summary>
    private const double ShapeMaskNoiseAmplitude = 0.25;

    /// <summary>マップ対角線長の最小値（除算ゼロ防止）</summary>
    private const int MinMapDiagonal = 1;

    /// <summary>
    /// マップの対角線長を算出する（除算ゼロ防止付き）
    /// </summary>
    private static int CalculateMapDiagonal(int width, int height)
    {
        int diag = (int)Math.Sqrt(width * width + height * height);
        return Math.Max(diag, MinMapDiagonal);
    }

    /// <summary>領地ごとのマップサイズ定義（幅×高さ、23000-50000マス範囲）</summary>
    private static readonly Dictionary<TerritoryId, (int Width, int Height)> TerritorySizes = new()
    {
        [TerritoryId.Capital]  = (220, 190), // 41800マス（王都領は広い）
        [TerritoryId.Forest]   = (205, 175), // 35875マス
        [TerritoryId.Mountain] = (190, 160), // 30400マス
        [TerritoryId.Coast]    = (205, 160), // 32800マス
        [TerritoryId.Southern] = (190, 175), // 33250マス
        [TerritoryId.Frontier] = (220, 205), // 45100マス（辺境は広大）
        [TerritoryId.Desert]   = (205, 175), // 35875マス
        [TerritoryId.Swamp]    = (175, 160), // 28000マス
        [TerritoryId.Tundra]   = (190, 160), // 30400マス
        [TerritoryId.Lake]     = (175, 160), // 28000マス
        [TerritoryId.Volcanic] = (160, 160), // 25600マス
        [TerritoryId.Sacred]   = (160, 146), // 23360マス（聖域は小さい）
    };

    /// <summary>
    /// 領地のマップサイズを取得する
    /// </summary>
    public static (int Width, int Height) GetTerritoryMapSize(TerritoryId territory)
    {
        return TerritorySizes.TryGetValue(territory, out var size) ? size : (220, 160);
    }

    /// <summary>
    /// 指定領地のシンボルマップを生成する
    /// </summary>
    public SymbolMapResult Generate(TerritoryId territory)
    {
        return Generate(territory, null);
    }

    /// <summary>
    /// 指定領地のシンボルマップを生成する（クリア済みダンジョン除外対応）
    /// </summary>
    public SymbolMapResult Generate(TerritoryId territory, ISet<string>? clearedDungeonIds)
    {
        var (width, height) = GetTerritoryMapSize(territory);
        var allLocations = LocationDefinition.GetSymbolLocations(territory);
        // クリア済みダンジョンを固定ロケーションからも除外
        var locations = clearedDungeonIds != null
            ? allLocations.Where(l => !clearedDungeonIds.Contains(l.Id)).ToList()
            : allLocations.ToList();
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

        // 3. 高度マップを生成
        var altitudeMap = GenerateAltitudeMap(territory, width, height, random);

        // 4. マスクに基づいて基本地形を敷く（高度付き）
        FillBaseTerrainWithMask(map, territory, shapeMask, random, altitudeMap);

        // 5. 既存ロケーションを配置（山岳・水域は回避）
        var locationPositions = PlaceLocations(map, locations, shapeMask, random);

        // 6. 村・町・都を自動配置（山岳・水域は回避）
        var totalTiles = CountWalkableTiles(map, shapeMask);
        PlaceSettlements(map, territory, locationPositions, shapeMask, totalTiles, random);

        // 7. ランダムダンジョン（野盗のねぐら、ゴブリンの巣等）を配置
        PlaceRandomDungeons(map, territory, locationPositions, shapeMask, random, clearedDungeonIds);

        // 8. ロケーション間を道で接続（既存ロケーション）
        ConnectLocationsWithRoads(map, locationPositions.Keys.ToList());

        // 9. 村・町・都を必ず道で接続する（最小全域木方式）
        ConnectSettlementsWithRoads(map, locationPositions);

        // 10. 関所（BorderGate）を隣接領地の方角に配置し、入口を関所に設定
        PlaceBorderGates(map, territory, locationPositions, shapeMask, random);

        // 入口は最初の関所、無ければ最初のロケーション
        var gatePos = locationPositions
            .Where(kv => kv.Value.Type == LocationType.BorderGate)
            .Select(kv => kv.Key)
            .FirstOrDefault();
        if (gatePos != default)
        {
            map.SetEntrance(gatePos);
        }
        else if (locationPositions.Count > 0)
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

                double threshold = ShapeMaskRoundness + noise * ShapeMaskNoiseAmplitude;
                mask[x, y] = dist < threshold;
            }
        }

        AddComplexFeatures(mask, width, height, random);

        return mask;
    }

    /// <summary>
    /// 角度に基づく境界ノイズ値を返す（-1.0～1.0）。
    /// 隣接領地と共有する辺のノイズは、両領地で同一のシード値から生成され、
    /// 境界形状が一致するようにする。
    /// </summary>
    private static double GetBorderNoise(double angle, TerritoryId territory)
    {
        // 方角に応じた隣接領地を特定し、共有シードを使う
        var adjacentTerritories = TerritoryDefinition.Get(territory).AdjacentTerritories;
        int sharedSeed = GetSharedBorderSeed(territory, angle, adjacentTerritories);

        double noise = 0;
        noise += 0.4 * Math.Sin(angle * 3 + sharedSeed * 0.01);
        noise += 0.3 * Math.Sin(angle * 5 + sharedSeed * 0.02);
        noise += 0.2 * Math.Sin(angle * 7 + sharedSeed * 0.03);
        noise += 0.1 * Math.Sin(angle * 11 + sharedSeed * 0.05);

        return noise;
    }

    /// <summary>
    /// 2つの領地間で共有する境界シード値を取得する。
    /// 角度から最も近い隣接領地を判定し、両領地のIDの組み合わせから決定論的シードを生成。
    /// これにより同じ辺を共有する2領地で同一のノイズカーブが使われ境界が一致する。
    /// </summary>
    private static int GetSharedBorderSeed(TerritoryId territory, double angle, TerritoryId[] adjacentTerritories)
    {
        if (adjacentTerritories.Length == 0)
            return (int)territory * 31337 + 42;

        // 角度を0-2πに正規化して隣接領地を均等に割り当て
        double normalizedAngle = (angle + Math.PI) / (2 * Math.PI); // 0.0-1.0
        int adjacentIndex = (int)(normalizedAngle * adjacentTerritories.Length) % adjacentTerritories.Length;
        var adjacentTerritory = adjacentTerritories[adjacentIndex];

        // 両領地IDの組み合わせで決定論的シード（順序非依存）
        int id1 = Math.Min((int)territory, (int)adjacentTerritory);
        int id2 = Math.Max((int)territory, (int)adjacentTerritory);
        return id1 * 10007 + id2 * 31337 + 42;
    }

    /// <summary>形状にフィンガー（突起）と湾（凹み）を追加</summary>
    private static void AddComplexFeatures(bool[,] mask, int width, int height, Random random)
    {
        int featureCount = Math.Max(8, (width + height) / 15);

        for (int i = 0; i < featureCount; i++)
        {
            bool isProtrusion = random.NextDouble() < 0.5;
            int cx = random.Next(width / 4, width * 3 / 4);
            int cy = random.Next(height / 4, height * 3 / 4);
            int radius = random.Next(8, 22);

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
        if (shapeMask.GetLength(0) != map.Width || shapeMask.GetLength(1) != map.Height)
            return 0;

        int count = 0;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                if (shapeMask[x, y])
                    count++;
        return count;
    }

    /// <summary>マスクに基づいて領地タイプに応じた基本地形を生成（高度付き、バイオーム固有タイル対応）</summary>
    private static void FillBaseTerrainWithMask(DungeonMap map, TerritoryId territory, bool[,] shapeMask, Random random, int[,] altitudeMap)
    {
        var (primary, secondary, obstacle) = GetTerrainForTerritory(territory);
        var (tertiary, quaternary) = GetExtraBiomeTiles(territory);

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

                if (noise < 0.50)
                    tileType = primary;
                else if (noise < 0.75)
                    tileType = secondary;
                else if (noise < 0.85 && tertiary.HasValue)
                    tileType = tertiary.Value;
                else if (noise < 0.90 && quaternary.HasValue)
                    tileType = quaternary.Value;
                else if (noise < 0.92)
                    tileType = secondary;  // tertiary/quaternary未定義時のフォールバック
                else
                    tileType = obstacle;

                int altitude = altitudeMap[x, y];

                // 山岳・水域タイルは高度付きで設定
                if (tileType is TileType.SymbolMountain or TileType.SymbolWater)
                {
                    map.SetTileWithAltitude(x, y, tileType, altitude);
                }
                else
                {
                    map.SetTile(x, y, tileType);
                }
            }
        }
    }

    /// <summary>領地タイプごとの地形構成を返す（バイオーム固有タイルを含む拡張版）</summary>
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
            TerritoryId.Desert   => (TileType.SymbolDune, TileType.SymbolGrass, TileType.SymbolMountain),
            TerritoryId.Swamp    => (TileType.SymbolSwamp, TileType.SymbolGrass, TileType.SymbolWater),
            TerritoryId.Tundra   => (TileType.SymbolIce, TileType.SymbolGrass, TileType.SymbolMountain),
            TerritoryId.Lake     => (TileType.SymbolGrass, TileType.SymbolWater, TileType.SymbolWater),
            TerritoryId.Volcanic => (TileType.SymbolLava, TileType.SymbolGrass, TileType.SymbolMountain),
            TerritoryId.Sacred   => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolForest),
            _ => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolMountain)
        };
    }

    /// <summary>
    /// 領地ごとの第3・第4地形（バイオーム固有の追加バリエーション）を返す。
    /// primary/secondaryに加え、低確率で配置される特徴的な地形。
    /// </summary>
    private static (TileType? tertiary, TileType? quaternary) GetExtraBiomeTiles(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital  => (TileType.SymbolForest, null),
            TerritoryId.Forest   => (TileType.SymbolMountain, null),
            TerritoryId.Mountain => (TileType.SymbolWater, null),
            TerritoryId.Coast    => (TileType.SymbolForest, TileType.SymbolMountain),
            TerritoryId.Southern => (TileType.SymbolMountain, TileType.SymbolDune),
            TerritoryId.Frontier => (TileType.SymbolWater, TileType.SymbolSwamp),
            TerritoryId.Desert   => (TileType.SymbolLava, TileType.SymbolMountain),
            TerritoryId.Swamp    => (TileType.SymbolForest, null),
            TerritoryId.Tundra   => (TileType.SymbolWater, null),
            TerritoryId.Lake     => (TileType.SymbolForest, TileType.SymbolSwamp),
            TerritoryId.Volcanic => (TileType.SymbolMountain, TileType.SymbolDune),
            TerritoryId.Sacred   => (TileType.SymbolWater, TileType.SymbolMountain),
            _ => (null, null)
        };
    }

    /// <summary>ロケーションをマップ上に配置する（集落系は山岳・水域回避）</summary>
    private static Dictionary<Position, LocationDefinition> PlaceLocations(
        DungeonMap map,
        IReadOnlyList<LocationDefinition> locations,
        bool[,] shapeMask,
        Random random)
    {
        var positions = new Dictionary<Position, LocationDefinition>();
        var usedPositions = new HashSet<Position>();

        int margin = 12;
        int areaWidth = map.Width - margin * 2;
        int areaHeight = map.Height - margin * 2;

        foreach (var location in locations)
        {
            bool isSettlement = location.Type is LocationType.Town or LocationType.Village or LocationType.Capital;
            Position pos;
            int attempts = 0;

            do
            {
                int x = random.Next(margin, margin + areaWidth);
                int y = random.Next(margin, margin + areaHeight);
                pos = new Position(x, y);
                attempts++;
            }
            while (attempts < MaxLocationPlacementAttempts && (
                !shapeMask[pos.X, pos.Y] ||
                (isSettlement && IsUnbuildableTerrain(map, pos)) ||
                usedPositions.Any(p => p.ChebyshevDistanceTo(pos) < 12)));

            // 配置失敗時はスキップ（不正な位置への強制配置を防止）
            if (attempts >= MaxLocationPlacementAttempts &&
                (!shapeMask[pos.X, pos.Y] ||
                 (isSettlement && IsUnbuildableTerrain(map, pos))))
            {
                continue;
            }

            var tileType = GetTileTypeForLocation(location.Type);
            map.SetTile(pos.X, pos.Y, tileType);
            EnsureAccessible(map, pos, shapeMask);

            positions[pos] = location;
            usedPositions.Add(pos);
        }

        return positions;
    }

    /// <summary>
    /// マップ対角線長から集落配置の最小距離を算出する。
    /// 都: 対角線の1/8、町: 対角線の1/12、村: 対角線の1/20。
    /// これにより全マップサイズで一貫した配置密度が得られる。
    /// </summary>
    public static (int Capital, int Town, int Village) GetSettlementMinDistances(int mapWidth, int mapHeight)
    {
        int diag = (int)Math.Sqrt(mapWidth * mapWidth + mapHeight * mapHeight);
        return (
            Capital: Math.Max(15, diag / 8),   // 対角線272→34, 対角線200→25
            Town: Math.Max(10, diag / 12),      // 対角線272→22, 対角線200→16
            Village: Math.Max(6, diag / 20)     // 対角線272→13, 対角線200→10
        );
    }

    /// <summary>
    /// 村・町・都を自動配置する。
    /// 村: 総マス数/500箇所、町: 総マス数/1000箇所、都: 1箇所
    /// minDistanceはマップ対角線比例で統一的に算出。
    /// </summary>
    private static void PlaceSettlements(
        DungeonMap map, TerritoryId territory,
        Dictionary<Position, LocationDefinition> locationPositions,
        bool[,] shapeMask, int totalTiles, Random random)
    {
        int villageCount = Math.Max(1, totalTiles / TilesPerVillage);
        int townCount = Math.Max(1, totalTiles / TilesPerTown);
        int capitalCount = 1;

        var allPositions = new HashSet<Position>(locationPositions.Keys);
        int margin = 12;

        var (capitalDist, townDist, villageDist) = GetSettlementMinDistances(map.Width, map.Height);

        // 都（首都）を配置（マップ中心付近）
        for (int i = 0; i < capitalCount; i++)
        {
            var pos = FindSettlementPosition(map, shapeMask, allPositions, margin, random,
                preferCenter: true, minDistance: capitalDist);
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
                preferCenter: false, minDistance: townDist);
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
                preferCenter: false, minDistance: villageDist);
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

    /// <summary>集落配置位置を探す（山岳・水域タイルは回避）</summary>
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

            // 山岳・水域・溶岩・沼地・氷原タイルへの配置を禁止
            if (IsUnbuildableTerrain(map, pos)) continue;

            if (usedPositions.Any(p => p.ChebyshevDistanceTo(pos) < minDistance)) continue;

            return pos;
        }
        return null;
    }

    /// <summary>ランダムダンジョン1つあたりの必要マス数（総マス数÷この値＝最大ダンジョン数）</summary>
    private const int TilesPerRandomDungeon = 800;

    /// <summary>ランダムダンジョンの最低配置数</summary>
    private const int MinRandomDungeons = 2;

    /// <summary>
    /// ランダムダンジョン（野盗のねぐら、ゴブリンの巣等）を配置する。
    /// 条件: 村/町/都からSettlementMinDistBaseマス以上離れた場所、他ダンジョンからDungeonMinDistBaseマス以上離れた場所
    /// 階層数・レベルは首都からの距離に基づく成長曲線で決定。
    /// </summary>
    private static void PlaceRandomDungeons(
        DungeonMap map, TerritoryId territory,
        Dictionary<Position, LocationDefinition> locationPositions,
        bool[,] shapeMask, Random random, ISet<string>? clearedDungeonIds)
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

        // 首都位置を特定（成長曲線の基準点）
        var capitalPos = locationPositions
            .Where(kv => kv.Value.Type == LocationType.Capital)
            .Select(kv => kv.Key)
            .FirstOrDefault();

        // 首都が無い場合はマップ中心を基準
        if (capitalPos == default)
            capitalPos = new Position(map.Width / 2, map.Height / 2);

        int mapDiag = CalculateMapDiagonal(map.Width, map.Height);

        var dungeonTypes = new[]
        {
            (name: "野盗のねぐら", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen),
            (name: "ゴブリンの巣", type: LocationType.GoblinNest, tile: TileType.SymbolGoblinNest),
            (name: "オーク族の砦", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen),
            (name: "盗賊団のアジト", type: LocationType.BanditDen, tile: TileType.SymbolBanditDen),
            (name: "コボルドの穴", type: LocationType.GoblinNest, tile: TileType.SymbolGoblinNest),
        };

        int maxDungeons = Math.Max(MinRandomDungeons, (map.Width * map.Height) / TilesPerRandomDungeon);

        for (int i = 0; i < maxDungeons; i++)
        {
            // RNG消費を一貫させるため、クリア済みかどうかに関わらず位置・種別を決定
            string dungeonId = $"{territory}_random_dungeon_{i}";
            var pos = FindRandomDungeonPosition(map, shapeMask, settlementPositions, dungeonPositions, random);
            if (pos == null) break;

            var dungeonDef = dungeonTypes[random.Next(dungeonTypes.Length)];

            // 首都からの距離に基づく成長曲線
            int distFromCapital = pos.Value.ChebyshevDistanceTo(capitalPos);
            float distRatio = Math.Clamp((float)distFromCapital / mapDiag, 0f, 1f);

            // 階層数: 近距離1-2階、中距離2-4階、遠距離3-6階
            int minFloors = Math.Max(1, (int)(1 + distRatio * 3));
            int maxFloors = Math.Max(minFloors + 1, (int)(2 + distRatio * 5));
            int floors = random.Next(minFloors, maxFloors + 1);

            // クリア済みダンジョンはRNG消費後にスキップ（後続ダンジョンの位置決定論性を維持）
            if (clearedDungeonIds != null && clearedDungeonIds.Contains(dungeonId))
            {
                continue;
            }

            // 危険度: 近距離1-2、中距離2-3、遠距離3-5
            int dangerLevel = Math.Clamp((int)(1 + distRatio * 4), 1, 5);

            // 推奨レベル: danger×3 + 距離ボーナス（遠いほど高レベル）
            int minLevel = Math.Max(1, dangerLevel * 3 + (int)(distRatio * 10));

            map.SetTile(pos.Value.X, pos.Value.Y, dungeonDef.tile);
            EnsureAccessible(map, pos.Value, shapeMask);

            var loc = new LocationDefinition(
                dungeonId,
                dungeonDef.name,
                $"全{floors}階層のダンジョン（推奨Lv.{minLevel}）。クリアすると消滅する",
                dungeonDef.type,
                territory,
                MinLevel: minLevel,
                DangerLevel: dangerLevel,
                MaxFloor: floors);

            locationPositions[pos.Value] = loc;
            dungeonPositions.Add(pos.Value);
        }
    }

    /// <summary>
    /// ランダムダンジョンの配置位置を探す。
    /// 集落からの最低距離は SettlementMinDistBase マス、他ダンジョンからは DungeonMinDistBase マス。
    /// ただしマップ対角線に対して自動的にスケールダウンされる。
    /// </summary>
    private const int SettlementMinDistBase = 25;
    private const int DungeonMinDistBase = 50;
    private const int SettlementDiagDivisor = 3;
    private const int DungeonDiagDivisor = 2;

    private static Position? FindRandomDungeonPosition(
        DungeonMap map, bool[,] shapeMask,
        List<Position> settlementPositions, List<Position> dungeonPositions,
        Random random)
    {
        int mapDiag = CalculateMapDiagonal(map.Width, map.Height);
        int settlementMinDist = Math.Min(SettlementMinDistBase, mapDiag / SettlementDiagDivisor);
        int dungeonMinDist = Math.Min(DungeonMinDistBase, mapDiag / DungeonDiagDivisor);

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
            LocationType.BorderGate => TileType.SymbolBorderGate,
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
            or TileType.SymbolGoblinNest or TileType.SymbolBorderGate;
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
            or TileType.SymbolBanditDen or TileType.SymbolGoblinNest
            or TileType.SymbolBorderGate or TileType.SymbolDune
            or TileType.SymbolLava or TileType.SymbolIce
            or TileType.SymbolSwamp;
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

    /// <summary>
    /// 高度マップを生成する。
    /// 領地の地形傾向に基づき、擬似パーリンノイズで自然な高度分布を作る。
    /// 山岳領地は正の高度(0-5)、海岸/湖/沼は負の高度(0～-5)、それ以外は混在。
    /// </summary>
    private static int[,] GenerateAltitudeMap(TerritoryId territory, int width, int height, Random random)
    {
        var altitudeMap = new int[width, height];

        // 領地タイプに応じた高度バイアス
        var (minAlt, maxAlt) = GetAltitudeRange(territory);

        // 複数オクターブのノイズで高度を生成
        double freqX = 0.02 + random.NextDouble() * 0.01;
        double freqY = 0.02 + random.NextDouble() * 0.01;
        double offsetX = random.NextDouble() * 1000;
        double offsetY = random.NextDouble() * 1000;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double noise = 0;
                noise += 0.5 * Math.Sin((x + offsetX) * freqX * 1.0 + (y + offsetY) * freqY * 0.7);
                noise += 0.3 * Math.Sin((x + offsetX) * freqX * 2.3 + (y + offsetY) * freqY * 1.9);
                noise += 0.2 * Math.Sin((x + offsetX) * freqX * 4.1 + (y + offsetY) * freqY * 3.7);

                // -1.0～1.0 を minAlt～maxAlt に線形変換
                double normalized = (noise + 1.0) / 2.0; // 0.0～1.0
                int altitude = minAlt + (int)(normalized * (maxAlt - minAlt));
                altitude = Math.Clamp(altitude, minAlt, maxAlt);

                altitudeMap[x, y] = altitude;
            }
        }

        return altitudeMap;
    }

    /// <summary>領地タイプごとの高度範囲を返す</summary>
    private static (int Min, int Max) GetAltitudeRange(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Mountain => (0, 5),     // 山岳領: 高所中心
            TerritoryId.Volcanic => (0, 4),     // 火山領: 高所
            TerritoryId.Tundra   => (-1, 3),    // 凍土: やや高め
            TerritoryId.Coast    => (-4, 1),    // 海岸: 海が多い
            TerritoryId.Lake     => (-3, 1),    // 湖水: 水域多い
            TerritoryId.Swamp    => (-2, 0),    // 沼沢: 低湿地
            TerritoryId.Frontier => (-1, 3),    // 辺境: 起伏あり
            TerritoryId.Capital  => (-1, 2),    // 王都: 平坦気味
            TerritoryId.Forest   => (-1, 2),    // 森林: 穏やか
            TerritoryId.Southern => (-2, 1),    // 南部: 平地＋沿岸
            TerritoryId.Desert   => (0, 3),     // 砂漠: 砂丘高め
            TerritoryId.Sacred   => (0, 2),     // 聖域: 高原
            _ => (-2, 2)
        };
    }

    /// <summary>
    /// 村・町・都を必ず道路で接続する（プリム法ベースの最小全域木）。
    /// Generate()のステップ9で呼び出される。
    /// </summary>
    private static void ConnectSettlementsWithRoads(
        DungeonMap map, Dictionary<Position, LocationDefinition> locationPositions)
    {
        // 集落（村・町・都）の位置を抽出
        var settlements = locationPositions
            .Where(kv => kv.Value.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
            .Select(kv => kv.Key)
            .ToList();

        if (settlements.Count < 2) return;

        // プリム法で最小全域木を構築（全集落を最短距離で接続）
        var connected = new HashSet<Position> { settlements[0] };
        var remaining = new HashSet<Position>(settlements.Skip(1));

        while (remaining.Count > 0)
        {
            Position bestFrom = default;
            Position bestTo = default;
            int bestDist = int.MaxValue;

            foreach (var from in connected)
            {
                foreach (var to in remaining)
                {
                    int dist = from.ChebyshevDistanceTo(to);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestFrom = from;
                        bestTo = to;
                    }
                }
            }

            CarveRoad(map, bestFrom, bestTo);
            connected.Add(bestTo);
            remaining.Remove(bestTo);
        }
    }

    /// <summary>
    /// 隣接領地方向にBorderGate（関所）を配置する。
    /// 各隣接領地につき1つの関所をマップ辺境付近に配置し、
    /// 道路で最寄りの集落と接続する。
    /// </summary>
    private static void PlaceBorderGates(
        DungeonMap map, TerritoryId territory,
        Dictionary<Position, LocationDefinition> locationPositions,
        bool[,] shapeMask, Random random)
    {
        var territoryDef = TerritoryDefinition.Get(territory);
        var adjacentTerritories = territoryDef.AdjacentTerritories;
        if (adjacentTerritories.Length == 0) return;

        int margin = 5;
        var allPositions = new HashSet<Position>(locationPositions.Keys);

        for (int i = 0; i < adjacentTerritories.Length; i++)
        {
            var adjTerritory = adjacentTerritories[i];

            // 隣接領地の方向を等角分割で決定
            double angle = (2 * Math.PI * i) / adjacentTerritories.Length;
            int targetX = map.Width / 2 + (int)((map.Width / 2 - margin - 5) * Math.Cos(angle));
            int targetY = map.Height / 2 + (int)((map.Height / 2 - margin - 5) * Math.Sin(angle));

            // 目標座標付近で配置可能な位置を探す
            Position? gatePos = null;
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int x = targetX + random.Next(-15, 16);
                int y = targetY + random.Next(-15, 16);
                x = Math.Clamp(x, margin, map.Width - margin - 1);
                y = Math.Clamp(y, margin, map.Height - margin - 1);

                if (!shapeMask[x, y]) continue;
                var pos = new Position(x, y);
                if (IsUnbuildableTerrain(map, pos)) continue;
                if (allPositions.Any(p => p.ChebyshevDistanceTo(pos) < 8)) continue;

                gatePos = pos;
                break;
            }

            if (!gatePos.HasValue)
            {
                // フォールバック: ターゲット座標が有効かつ他ロケーションと十分離れていれば使用
                int fx = Math.Clamp(targetX, margin, map.Width - margin - 1);
                int fy = Math.Clamp(targetY, margin, map.Height - margin - 1);
                var fallbackPos = new Position(fx, fy);
                if (shapeMask[fx, fy] && !IsUnbuildableTerrain(map, fallbackPos)
                    && !allPositions.Any(p => p.ChebyshevDistanceTo(fallbackPos) < 5))
                {
                    gatePos = fallbackPos;
                }
                else
                {
                    continue;
                }
            }

            var adjName = GetTerritoryDisplayName(adjTerritory);
            map.SetTile(gatePos.Value.X, gatePos.Value.Y, TileType.SymbolBorderGate);
            EnsureAccessible(map, gatePos.Value, shapeMask);

            var loc = new LocationDefinition(
                $"{territory}_gate_to_{adjTerritory}",
                $"{adjName}方面の関所",
                $"{adjName}領への国境検問所。ここから{adjName}領へ渡れる",
                LocationType.BorderGate,
                territory,
                DangerLevel: 1);
            locationPositions[gatePos.Value] = loc;
            allPositions.Add(gatePos.Value);

            // 関所と最寄集落を道路で接続
            var nearestSettlement = locationPositions
                .Where(kv => kv.Value.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
                .OrderBy(kv => kv.Key.ChebyshevDistanceTo(gatePos.Value))
                .Select(kv => kv.Key)
                .FirstOrDefault();
            if (nearestSettlement != default)
            {
                CarveRoad(map, gatePos.Value, nearestSettlement);
            }
        }
    }

    /// <summary>
    /// 指定位置のタイルが集落建設不可地形かどうか判定する。
    /// 山岳・水域・溶岩・沼地・氷原が該当する。
    /// </summary>
    private static bool IsUnbuildableTerrain(DungeonMap map, Position pos)
    {
        if (!map.IsInBounds(pos)) return true;
        var tile = map.GetTile(pos);
        return tile.Type is TileType.SymbolMountain or TileType.SymbolWater
            or TileType.SymbolLava or TileType.SymbolSwamp or TileType.SymbolIce;
    }
}

/// <summary>
/// シンボルマップ生成結果
/// </summary>
public record SymbolMapResult(
    DungeonMap Map,
    Dictionary<Position, LocationDefinition> LocationPositions
);
