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
        return location?.Type == LocationType.Dungeon;
    }

    /// <summary>
    /// 指定位置が街の入口かどうか判定する
    /// </summary>
    public bool IsTownEntrance(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type is LocationType.Town or LocationType.Village;
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
            LocationType.Town or LocationType.Village => $"【{location.Name}】に到着した。（Tキーで街に入る）",
            LocationType.Facility => $"【{location.Name}】に到着した。{location.Description}",
            LocationType.ReligiousSite => $"【{location.Name}】に到着した。{location.Description}",
            LocationType.Field => $"【{location.Name}】に到着した。{location.Description}",
            _ => $"【{location.Name}】に到着した。"
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
