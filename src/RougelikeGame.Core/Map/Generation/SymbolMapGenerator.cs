using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// シンボルマップ生成器
/// 領地ごとの地上マップを生成する。Elona/The Door of Trithius風の
/// ロケーションシンボルが配置されたオーバーワールドマップ。
/// </summary>
public class SymbolMapGenerator
{
    /// <summary>シンボルマップの幅</summary>
    public const int MapWidth = 40;

    /// <summary>シンボルマップの高さ</summary>
    public const int MapHeight = 25;

    /// <summary>
    /// 指定領地のシンボルマップを生成する
    /// </summary>
    /// <param name="territory">対象の領地</param>
    /// <returns>生成されたマップとロケーション配置情報</returns>
    public SymbolMapResult Generate(TerritoryId territory)
    {
        var locations = LocationDefinition.GetSymbolLocations(territory);
        var random = new Random(GetTerritorySeed(territory));

        var map = new DungeonMap(MapWidth, MapHeight)
        {
            Depth = 0,
            Name = $"{territory}_surface"
        };

        // 1. 基本地形を敷く
        FillBaseTerrain(map, territory, random);

        // 2. ロケーションを配置
        var locationPositions = PlaceLocations(map, locations, random);

        // 3. ロケーション間を道で接続
        ConnectLocationsWithRoads(map, locationPositions.Keys.ToList());

        // 4. 入口位置を設定（最初のロケーション）
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

    /// <summary>領地タイプに応じた基本地形を生成</summary>
    private static void FillBaseTerrain(DungeonMap map, TerritoryId territory, Random random)
    {
        var (primary, secondary, obstacle) = GetTerrainForTerritory(territory);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                // 外周は通行不可地形
                if (x == 0 || x == map.Width - 1 || y == 0 || y == map.Height - 1)
                {
                    map.SetTile(x, y, obstacle);
                    continue;
                }

                double noise = random.NextDouble();
                TileType tileType;

                if (noise < 0.60)
                    tileType = primary;       // 60%: 主要地形
                else if (noise < 0.85)
                    tileType = secondary;     // 25%: 副次地形
                else
                    tileType = obstacle;      // 15%: 障害物

                map.SetTile(x, y, tileType);
            }
        }
    }

    /// <summary>領地タイプごとの地形構成を返す</summary>
    private static (TileType primary, TileType secondary, TileType obstacle) GetTerrainForTerritory(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital => (TileType.SymbolGrass, TileType.SymbolRoad, TileType.SymbolForest),
            TerritoryId.Forest => (TileType.SymbolForest, TileType.SymbolGrass, TileType.SymbolWater),
            TerritoryId.Mountain => (TileType.SymbolGrass, TileType.SymbolMountain, TileType.SymbolMountain),
            TerritoryId.Coast => (TileType.SymbolGrass, TileType.SymbolWater, TileType.SymbolWater),
            TerritoryId.Southern => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolWater),
            TerritoryId.Frontier => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolMountain),
            _ => (TileType.SymbolGrass, TileType.SymbolForest, TileType.SymbolMountain)
        };
    }

    /// <summary>ロケーションをマップ上に配置する</summary>
    private static Dictionary<Position, LocationDefinition> PlaceLocations(
        DungeonMap map,
        IReadOnlyList<LocationDefinition> locations,
        Random random)
    {
        var positions = new Dictionary<Position, LocationDefinition>();
        var usedPositions = new HashSet<Position>();

        // ロケーション配置領域（外周から3タイル内側）
        int margin = 3;
        int areaWidth = map.Width - margin * 2;
        int areaHeight = map.Height - margin * 2;

        foreach (var location in locations)
        {
            Position pos;
            int attempts = 0;

            // 重ならない位置を探す（最低距離3マス）
            do
            {
                int x = random.Next(margin, margin + areaWidth);
                int y = random.Next(margin, margin + areaHeight);
                pos = new Position(x, y);
                attempts++;
            }
            while (attempts < 100 && usedPositions.Any(p =>
                p.ChebyshevDistanceTo(pos) < 3));

            // タイルタイプをロケーション種別に応じて設定
            var tileType = GetTileTypeForLocation(location.Type);
            map.SetTile(pos.X, pos.Y, tileType);

            // 周囲1マスを歩行可能にする（アクセス保証）
            EnsureAccessible(map, pos);

            positions[pos] = location;
            usedPositions.Add(pos);
        }

        return positions;
    }

    /// <summary>ロケーション種別からタイルタイプを取得</summary>
    private static TileType GetTileTypeForLocation(LocationType type)
    {
        return type switch
        {
            LocationType.Town => TileType.SymbolTown,
            LocationType.Village => TileType.SymbolTown,
            LocationType.Facility => TileType.SymbolFacility,
            LocationType.ReligiousSite => TileType.SymbolShrine,
            LocationType.Field => TileType.SymbolField,
            LocationType.Dungeon => TileType.SymbolDungeon,
            _ => TileType.SymbolField
        };
    }

    /// <summary>指定位置の周囲1マスを歩行可能にする</summary>
    private static void EnsureAccessible(DungeonMap map, Position center)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var pos = new Position(center.X + dx, center.Y + dy);
                if (map.IsInBounds(pos))
                {
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

        // 全ロケーションを順にL字型の道で接続
        for (int i = 0; i < positions.Count - 1; i++)
        {
            CarveRoad(map, positions[i], positions[i + 1]);
        }

        // 最後と最初も接続して循環路を作る（3か所以上の場合）
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

        // 水平方向に移動
        while (x != to.X)
        {
            x += x < to.X ? 1 : -1;
            var pos = new Position(x, y);
            if (map.IsInBounds(pos))
            {
                var tile = map.GetTile(pos);
                // ロケーションマーカーは上書きしない
                if (!IsLocationTile(tile.Type))
                {
                    map.SetTile(x, y, TileType.SymbolRoad);
                }
            }
        }

        // 垂直方向に移動
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
            or TileType.SymbolField;
    }

    /// <summary>シンボルマップ用タイルかどうか判定</summary>
    public static bool IsSymbolMapTile(TileType type)
    {
        return type is TileType.SymbolGrass or TileType.SymbolForest
            or TileType.SymbolMountain or TileType.SymbolWater
            or TileType.SymbolRoad or TileType.SymbolTown
            or TileType.SymbolDungeon or TileType.SymbolFacility
            or TileType.SymbolShrine or TileType.SymbolField;
    }
}

/// <summary>
/// シンボルマップ生成結果
/// </summary>
public record SymbolMapResult(
    DungeonMap Map,
    Dictionary<Position, LocationDefinition> LocationPositions
);
