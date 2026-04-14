using RougelikeGame.Core.Interfaces;
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

    /// <summary>現在のシンボルマップ上の徘徊ボスモンスター</summary>
    public WanderingBossInstance? CurrentWanderingBoss { get; private set; }

    /// <summary>配置されたロケーション数</summary>
    public int LocationCount => _locationPositions.Count;

    /// <summary>配置されたロケーション位置辞書を取得（派閥影響判定用）</summary>
    public IReadOnlyDictionary<Position, LocationDefinition> GetLocationPositions()
        => _locationPositions;

    /// <summary>
    /// 指定領地のシンボルマップを生成して設定する
    /// </summary>
    public DungeonMap GenerateForTerritory(TerritoryId territory)
    {
        return GenerateForTerritory(territory, null);
    }

    /// <summary>
    /// 指定領地のシンボルマップを生成して設定する（クリア済みダンジョン除外対応）
    /// </summary>
    public DungeonMap GenerateForTerritory(TerritoryId territory, ISet<string>? clearedDungeonIds)
    {
        var result = _generator.Generate(territory, clearedDungeonIds);
        CurrentMap = result.Map;
        CurrentTerritory = territory;
        _locationPositions = result.LocationPositions;
        CurrentWanderingBoss = result.WanderingBoss;
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
    /// 指定位置がダンジョン入口かどうか判定する。
    /// Dungeon: 固定ダンジョン（ストーリー関連、複数階層）
    /// BanditDen: 野盗のねぐら（盗賊系敵、近距離攻撃中心、罠多め、金品報酬多い）
    /// GoblinNest: ゴブリンの巣（ゴブリン系敵、群体戦、階層浅め、装備品報酬多い）
    /// UndeadCrypt: アンデッドの墓所（骸骨兵等、十字形の墓地・聖堂テーマ）
    /// DemonPortal: 魔族の門（小悪魔等、円形・十字形の儀式的テーマ）
    /// </summary>
    public bool IsDungeonEntrance(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type is LocationType.Dungeon or LocationType.BanditDen
            or LocationType.GoblinNest or LocationType.UndeadCrypt or LocationType.DemonPortal;
    }

    /// <summary>指定位置が野盗のねぐらかどうか判定する</summary>
    public bool IsBanditDen(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.BanditDen;
    }

    /// <summary>指定位置がゴブリンの巣かどうか判定する</summary>
    public bool IsGoblinNest(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.GoblinNest;
    }

    /// <summary>指定位置がアンデッドの墓所かどうか判定する</summary>
    public bool IsUndeadCrypt(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.UndeadCrypt;
    }

    /// <summary>指定位置が魔族の門かどうか判定する</summary>
    public bool IsDemonPortal(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.DemonPortal;
    }

    /// <summary>
    /// ランダムダンジョンの派閥名を取得する（派閥消失判定用）
    /// </summary>
    public static string? GetFactionForDungeonType(LocationType type)
    {
        return type switch
        {
            LocationType.BanditDen => TerritoryInfluenceSystem.FactionNames.Bandit,
            LocationType.GoblinNest => TerritoryInfluenceSystem.FactionNames.Goblin,
            LocationType.UndeadCrypt => TerritoryInfluenceSystem.FactionNames.Undead,
            LocationType.DemonPortal => TerritoryInfluenceSystem.FactionNames.Demon,
            _ => null
        };
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
    /// 指定位置が施設かどうか判定する。
    /// Facility: 冒険者ギルド、鍛冶場、市場等の実用施設（物品売買・依頼受注等が可能）
    /// </summary>
    public bool IsFacility(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.Facility;
    }

    /// <summary>
    /// 指定位置が宗教施設かどうか判定する。
    /// ReligiousSite: 祠・神殿・祭壇等の信仰関連施設（祈り・呪い解除・バフ取得等が可能）
    /// </summary>
    public bool IsShrine(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.ReligiousSite;
    }

    /// <summary>
    /// 指定位置が野外エリアかどうか判定する。
    /// Field: 荒野・平原・森等のオープンフィールド（ランダムエンカウント・採集・探索が可能）
    /// </summary>
    public bool IsField(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.Field;
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
            LocationType.UndeadCrypt => $"【{location.Name}】を発見した。（>キーでダンジョンに入る）",
            LocationType.DemonPortal => $"【{location.Name}】を発見した。（>キーでダンジョンに入る）",
            LocationType.Town or LocationType.Village => $"【{location.Name}】に到着した。（Tキーで街に入る）",
            LocationType.Capital => $"【{location.Name}】に到着した。（Tキーで都に入る）",
            LocationType.BorderGate => $"【{location.Name}】に到着した。{location.Description}",
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
            return location.Type is not (LocationType.Dungeon or LocationType.BanditDen
                or LocationType.GoblinNest or LocationType.UndeadCrypt or LocationType.DemonPortal);

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
            or TileType.SymbolRoad or TileType.SymbolDune
            or TileType.SymbolLava or TileType.SymbolIce
            or TileType.SymbolSwamp;
    }

    /// <summary>
    /// シンボルマップのタイル属性名を取得する（日本語表示用、高度情報付き）
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
            TileType.SymbolDune => "砂丘",
            TileType.SymbolLava => "溶岩地帯",
            TileType.SymbolIce => "氷原",
            TileType.SymbolSwamp => "沼地",
            _ => "野外"
        };
    }

    /// <summary>
    /// シンボルマップのタイル属性名を高度付きで取得する
    /// </summary>
    public static string GetTerrainNameWithAltitude(Tile tile)
    {
        if (tile.Type == TileType.SymbolMountain)
        {
            return tile.Altitude switch
            {
                >= 4 => "険しい高山",
                >= 2 => "山岳地帯",
                _ => "丘陵"
            };
        }
        if (tile.Type == TileType.SymbolWater)
        {
            if (tile.RequiresShip)
                return "深海（船が必要）";
            return tile.Altitude switch
            {
                <= -2 => "深い水域",
                <= -1 => "浅い水域",
                _ => "水辺"
            };
        }
        return GetTerrainName(tile.Type);
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
        CurrentWanderingBoss = null;
    }

    /// <summary>
    /// 指定位置のロケーションを削除する（ランダムダンジョンのクリア消滅用）。
    /// マップ上のタイルを草原に変え、ロケーション辞書からも除去する。
    /// </summary>
    public bool RemoveLocation(Position position)
    {
        if (!_locationPositions.ContainsKey(position)) return false;
        _locationPositions.Remove(position);
        if (CurrentMap != null && CurrentMap.IsInBounds(position))
        {
            CurrentMap.SetTile(position.X, position.Y, TileType.SymbolGrass);
        }
        return true;
    }

    /// <summary>
    /// 指定IDのロケーションを削除する（ランダムダンジョンのクリア消滅用）。
    /// </summary>
    public bool RemoveLocationById(string locationId)
    {
        var match = _locationPositions
            .Where(kv => kv.Value.Id == locationId)
            .Select(kv => (Position: kv.Key, Found: true))
            .FirstOrDefault();
        if (!match.Found) return false;
        return RemoveLocation(match.Position);
    }

    /// <summary>
    /// 指定IDのロケーション定義を取得する。
    /// </summary>
    public LocationDefinition? GetLocationById(string locationId)
    {
        return _locationPositions.Values.FirstOrDefault(l => l.Id == locationId);
    }

    /// <summary>
    /// 関所ロケーションかどうかを判定する
    /// </summary>
    public bool IsBorderGate(Position position)
    {
        var location = GetLocationAt(position);
        return location?.Type == LocationType.BorderGate;
    }

    /// <summary>
    /// 集落間の交易ルート（道路タイル上の経路）を取得する。
    /// 各ルートは始点集落・終点集落のペアと経路上の道路タイル座標リスト。
    /// </summary>
    public IReadOnlyList<TradeRoute> GetTradeRoutes()
    {
        if (CurrentMap == null) return Array.Empty<TradeRoute>();

        var settlements = _locationPositions
            .Where(kv => kv.Value.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
            .ToList();

        var routes = new List<TradeRoute>();

        // 各集落ペア間で道路タイルが接続しているか簡易判定
        for (int i = 0; i < settlements.Count; i++)
        {
            for (int j = i + 1; j < settlements.Count; j++)
            {
                var (posA, locA) = settlements[i];
                var (posB, locB) = settlements[j];

                // 2集落間のチェビシェフ距離が一定以内のもの（直結ルート候補）
                int dist = posA.ChebyshevDistanceTo(posB);
                if (dist <= CurrentMap.Width / 3)
                {
                    routes.Add(new TradeRoute(locA.Id, locB.Id, posA, posB, dist));
                }
            }
        }

        return routes;
    }

    /// <summary>交易ルート定義</summary>
    public record TradeRoute(string FromId, string ToId, Position FromPos, Position ToPos, int Distance);

    /// <summary>
    /// 徘徊ボスを1ステップ移動させる（毎ターン呼び出し）
    /// </summary>
    public void MoveWanderingBoss(IRandomProvider random)
    {
        if (CurrentWanderingBoss == null || CurrentMap == null) return;
        WanderingBossSystem.MoveBoss(CurrentWanderingBoss, CurrentMap, random);
    }

    /// <summary>
    /// プレイヤーが徘徊ボスと接触しているかチェックする
    /// </summary>
    public bool IsPlayerContactingBoss(Position playerPos)
    {
        return WanderingBossSystem.IsPlayerContactingBoss(playerPos, CurrentWanderingBoss);
    }

    /// <summary>
    /// 徘徊ボスを撃破済みにする
    /// </summary>
    public void DefeatWanderingBoss()
    {
        if (CurrentWanderingBoss != null)
        {
            CurrentWanderingBoss.IsDefeated = true;
        }
    }

    /// <summary>
    /// 徘徊ボスの到着メッセージを取得する
    /// </summary>
    public string GetWanderingBossArrivalMessage()
    {
        if (CurrentWanderingBoss == null) return string.Empty;
        return $"⚠ 【{CurrentWanderingBoss.Definition.Name}】が接近している！ 逃げるなら今のうちだ！";
    }
}
