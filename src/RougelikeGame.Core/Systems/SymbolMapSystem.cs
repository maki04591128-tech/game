using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// シンボルマップシステム
/// 領地の地上マップ（シンボルマップ）を管理し、ロケーション遷移を制御する。
/// Elona/The Door of Trithius風のシンボルマップ→プレイマップ遷移の中核。
/// </summary>
public class SymbolMapSystem
{
    private readonly SymbolMapGenerator _generator = new();
    private Dictionary<Position, LocationDefinition> _locationPositions = new();

    /// <summary>現在のシンボルマップ</summary>
    public DungeonMap? CurrentMap { get; private set; }

    /// <summary>現在の領地</summary>
    public TerritoryId? CurrentTerritory { get; private set; }

    /// <summary>配置されたロケーション数</summary>
    public int LocationCount => _locationPositions.Count;

    /// <summary>
    /// 指定領地のシンボルマップを生成して設定する
    /// </summary>
    public DungeonMap GenerateForTerritory(TerritoryId territory)
    {
        var result = _generator.Generate(territory);
        CurrentMap = result.Map;
        CurrentTerritory = territory;
        _locationPositions = result.LocationPositions;
        return result.Map;
    }

    /// <summary>
    /// 指定位置にあるロケーションを取得する
    /// </summary>
    public LocationDefinition? GetLocationAt(Position position)
    {
        return _locationPositions.GetValueOrDefault(position);
    }

    /// <summary>
    /// 全ロケーション位置を取得する
    /// </summary>
    public IReadOnlyDictionary<Position, LocationDefinition> GetAllLocationPositions()
    {
        return _locationPositions;
    }

    /// <summary>
    /// 指定位置がダンジョン入口かどうか判定する
    /// </summary>
    public bool IsDungeonEntrance(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type is LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest;
    }

    /// <summary>
    /// 指定位置が街の入口かどうか判定する
    /// </summary>
    public bool IsTownEntrance(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type is LocationType.Town or LocationType.Village or LocationType.Capital;
    }

    /// <summary>
    /// 指定位置が施設かどうか判定する
    /// </summary>
    public bool IsFacility(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.Facility;
    }

    /// <summary>
    /// 指定位置が宗教施設かどうか判定する
    /// </summary>
    public bool IsShrine(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.ReligiousSite;
    }

    /// <summary>
    /// 指定位置のタイルがロケーションシンボルかどうか判定する
    /// </summary>
    public bool IsLocationSymbol(Position position)
    {
        return _locationPositions.ContainsKey(position);
    }

    /// <summary>
    /// ロケーションに到着した際のメッセージを生成する
    /// </summary>
    public string GetLocationArrivalMessage(Position position)
    {
        var location = GetLocationAt(position);
        if (location == null) return string.Empty;

        return location.Type switch
        {
            LocationType.Dungeon => $"【{location.Name}】の入口に到着した。（>キーでダンジョンに入る）",
            LocationType.BanditDen => $"【{location.Name}】を発見した。（>キーでダンジョンに入る）",
            LocationType.GoblinNest => $"【{location.Name}】を発見した。（>キーでダンジョンに入る）",
            LocationType.Town or LocationType.Village => $"【{location.Name}】に到着した。（Tキーで街に入る）",
            LocationType.Capital => $"【{location.Name}】に到着した。（Tキーで都に入る）",
            LocationType.Facility => $"【{location.Name}】に到着した。{location.Description}",
            LocationType.ReligiousSite => $"【{location.Name}】に到着した。{location.Description}",
            LocationType.Field => $"【{location.Name}】に到着した。{location.Description}",
            _ => $"【{location.Name}】に到着した。"
        };
    }

    /// <summary>
    /// 指定位置がダンジョン以外の進入可能なシンボルマップタイルかどうか判定する。
    /// ロケーション配置済みタイル（Dungeon以外）またはフィールド系地形タイルを含む。
    /// </summary>
    public bool CanEnterField(Position position)
    {
        var location = GetLocationAt(position);
        if (location != null)
            return location.Type is not (LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest);

        if (CurrentMap == null) return false;
        var tile = CurrentMap.GetTile(position);
        return IsEnterableTerrainTile(tile.Type);
    }

    /// <summary>
    /// シンボルマップの地形タイルでフィールドとして進入可能かどうか判定する
    /// </summary>
    public static bool IsEnterableTerrainTile(TileType type)
    {
        return type is TileType.SymbolGrass or TileType.SymbolForest
            or TileType.SymbolMountain or TileType.SymbolWater
            or TileType.SymbolRoad;
    }

    /// <summary>
    /// シンボルマップのタイル属性名を取得する（日本語表示用）
    /// </summary>
    public static string GetTerrainName(TileType type)
    {
        return type switch
        {
            TileType.SymbolGrass => "草原",
            TileType.SymbolForest => "森林",
            TileType.SymbolMountain => "山岳地帯",
            TileType.SymbolWater => "水辺",
            TileType.SymbolRoad => "街道",
            _ => "野外"
        };
    }

    /// <summary>
    /// 指定IDのロケーション位置を取得する
    /// </summary>
    public Position? FindLocationPosition(string locationId)
    {
        foreach (var (pos, loc) in _locationPositions)
        {
            if (loc.Id == locationId) return pos;
        }
        return null;
    }

    /// <summary>
    /// シンボルマップをクリアする（ダンジョン進入時等）
    /// </summary>
    public void Clear()
    {
        CurrentMap = null;
        CurrentTerritory = null;
        _locationPositions.Clear();
    }
}
