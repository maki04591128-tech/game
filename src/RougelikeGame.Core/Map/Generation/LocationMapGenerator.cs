using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Map.Generation;

/// <summary>
/// ロケーション別マップ生成器
/// 町・村・施設・宗教施設・フィールドなど、ロケーションタイプに応じた
/// 形だけのマップを生成する。細かなディテールはVer.αで調整予定。
/// </summary>
public class LocationMapGenerator
{
    private readonly Random _random;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="seed">乱数シード。null指定時はランダムなシードを使用（実行ごとに異なるマップが生成される）</param>
    public LocationMapGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// ロケーション定義に応じた適切なマップを生成する
    /// </summary>
    public DungeonMap GenerateForLocation(LocationDefinition location)
    {
        return location.Type switch
        {
            LocationType.Town => GenerateTownMap(location.Id, location.Name),
            LocationType.Village => GenerateVillageMap(location.Id, location.Name),
            LocationType.Facility => GenerateFacilityMap(location.Id, location.Name),
            LocationType.ReligiousSite => GenerateShrineMap(location.Id, location.Name),
            LocationType.Field => GenerateFieldMap(location.Id, location.Name),
            _ => GenerateTownMap(location.Id, location.Name)
        };
    }

    /// <summary>
    /// 町マップを生成（50x30）
    /// 建物が並ぶ通りと中央広場のある町
    /// </summary>
    public DungeonMap GenerateTownMap(string locationId, string locationName)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        // 全体を草地で埋める
        FillAll(map, TileType.Grass);

        // 外周は通行可能（端から外へ出ると町を出る）

        // 中央に十字の道路を敷く
        int midX = width / 2;
        int midY = height / 2;
        DrawHorizontalRoad(map, midY, 2, width - 3);
        DrawVerticalRoad(map, midX, 2, height - 3);

        // 中央広場（5x5）
        FillRect(map, midX - 2, midY - 2, 5, 5, TileType.Floor);
        map.SetTile(new Position(midX, midY), TileType.Fountain);

        // 四隅に建物を配置
        PlaceBuilding(map, 3, 3, 8, 6);        // 左上: 宿屋風
        PlaceBuilding(map, width - 12, 3, 8, 6);  // 右上: 商店風
        PlaceBuilding(map, 3, height - 10, 8, 6);  // 左下: 鍛冶屋風
        PlaceBuilding(map, width - 12, height - 10, 8, 6); // 右下: ギルド風

        // 道路沿いに小さな建物を追加
        PlaceBuilding(map, midX + 4, 3, 6, 5);
        PlaceBuilding(map, midX + 4, height - 9, 6, 5);
        PlaceBuilding(map, 3, midY + 3, 6, 5);
        PlaceBuilding(map, width - 10, midY + 3, 6, 5);

        // NPC配置（建物内部に配置）
        // 左上建物（宿屋）: 宿屋主人
        map.SetTile(new Position(6, 5), TileType.NpcInnkeeper);
        // 右上建物（商店）: 商人
        map.SetTile(new Position(width - 9, 5), TileType.NpcShopkeeper);
        // 左下建物（鍛冶屋）: 鍛冶屋
        map.SetTile(new Position(6, height - 8), TileType.NpcBlacksmith);
        // 右下建物（ギルド）: ギルド受付
        map.SetTile(new Position(width - 9, height - 8), TileType.NpcGuildReceptionist);
        // 道路沿い右上建物: 神父
        map.SetTile(new Position(midX + 6, 5), TileType.NpcPriest);

        // 町の入口（南側道路中央）にスポーンポイントを設定
        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);

        return map;
    }

    /// <summary>
    /// 村マップを生成（40x25）
    /// 小屋と畑のある小さな集落
    /// </summary>
    public DungeonMap GenerateVillageMap(string locationId, string locationName)
    {
        const int width = 40;
        const int height = 25;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 外周は通行可能（端から外へ出ると村を出る）

        // 中央に小さな道
        int midX = width / 2;
        int midY = height / 2;
        DrawHorizontalRoad(map, midY, 2, width - 3);

        // 小屋を3つ配置
        PlaceBuilding(map, 4, 4, 6, 5);
        PlaceBuilding(map, 14, 4, 6, 5);
        PlaceBuilding(map, 4, midY + 3, 6, 5);

        // 畑エリア（右側）
        FillRect(map, width - 14, 4, 10, 8, TileType.Floor);
        // 畑の畝を表現
        for (int x = width - 13; x < width - 5; x += 2)
        {
            for (int y = 5; y < 11; y++)
            {
                map.SetTile(new Position(x, y), TileType.Grass);
            }
        }

        // 井戸（噴水タイル代用）
        map.SetTile(new Position(midX, midY - 2), TileType.Fountain);

        // 村の入口（南側道路中央）にスポーンポイントを設定
        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);

        return map;
    }

    /// <summary>
    /// 施設内部マップを生成（30x20）
    /// ギルド、学院、闘技場などの内部
    /// </summary>
    public DungeonMap GenerateFacilityMap(string locationId, string locationName)
    {
        const int width = 30;
        const int height = 20;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Wall);

        // 外周を通行可能にする（端から外へ出ると施設を出る）
        DrawPassableBorder(map);

        // 大部屋（メインホール）
        FillRect(map, 2, 2, width - 4, height - 4, TileType.Floor);

        // 内部の仕切り壁で2-3部屋に分割
        int midX = width / 2;
        // 縦の仕切り壁（上半分）
        for (int y = 2; y < height / 2 - 1; y++)
        {
            map.SetTile(new Position(midX, y), TileType.Wall);
        }
        // 仕切り壁にドアを配置
        map.SetTile(new Position(midX, height / 2 - 2), TileType.DoorClosed);

        // カウンター（受付）を表現（柱タイル代用）
        for (int x = 4; x < midX - 1; x++)
        {
            map.SetTile(new Position(x, height / 2 + 2), TileType.Pillar);
        }

        // 祭壇（施設の中心にオブジェクト）
        map.SetTile(new Position(midX, height / 2), TileType.Altar);

        // 出口ドア（南側中央）
        PlaceExitDoor(map, midX, height - 2);

        return map;
    }

    /// <summary>
    /// 宗教施設マップを生成（35x25）
    /// 祭壇と回廊のある神殿・大聖堂
    /// </summary>
    public DungeonMap GenerateShrineMap(string locationId, string locationName)
    {
        const int width = 35;
        const int height = 25;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Wall);

        // 外周を通行可能にする（端から外へ出ると施設を出る）
        DrawPassableBorder(map);

        // メイン回廊（中央の大通路）
        int midX = width / 2;
        FillRect(map, midX - 3, 2, 7, height - 4, TileType.Floor);

        // 左右の側廊
        FillRect(map, 3, 2, 5, height - 4, TileType.Floor);
        FillRect(map, width - 8, 2, 5, height - 4, TileType.Floor);

        // 横通路で回廊を接続
        FillRect(map, 3, height / 3, width - 6, 3, TileType.Floor);
        FillRect(map, 3, height * 2 / 3, width - 6, 3, TileType.Floor);

        // 祭壇エリア（奥の聖域）
        FillRect(map, midX - 4, 2, 9, 5, TileType.Floor);
        map.SetTile(new Position(midX, 4), TileType.Altar);

        // 柱を配置（回廊の雰囲気）
        for (int y = 5; y < height - 3; y += 3)
        {
            map.SetTile(new Position(midX - 2, y), TileType.Pillar);
            map.SetTile(new Position(midX + 2, y), TileType.Pillar);
        }

        // 出口ドア（南側中央）
        PlaceExitDoor(map, midX, height - 2);

        return map;
    }

    /// <summary>
    /// フィールドマップを生成（60x30）
    /// 自然地形の野外エリア（草地、木、水辺）
    /// </summary>
    public DungeonMap GenerateFieldMap(string locationId, string locationName)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        // 全面草地
        FillAll(map, TileType.Grass);

        // ランダムに木を配置（20%）
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (_random.NextDouble() < 0.20)
                {
                    map.SetTile(new Position(x, y), TileType.Tree);
                }
            }
        }

        // 水辺（横に流れる小川）
        int riverY = height / 3 + _random.Next(5);
        for (int x = 0; x < width; x++)
        {
            int offsetY = riverY + (int)(Math.Sin(x * 0.3) * 1.5);
            if (offsetY >= 0 && offsetY < height)
            {
                map.SetTile(new Position(x, offsetY), TileType.Water);
            }
            // 川幅を2タイルにする
            if (offsetY + 1 >= 0 && offsetY + 1 < height)
            {
                map.SetTile(new Position(x, offsetY + 1), TileType.Water);
            }
        }

        // 橋（川を渡れる場所）
        int bridgeX = width / 2;
        map.SetTile(new Position(bridgeX, riverY), TileType.Floor);
        map.SetTile(new Position(bridgeX, riverY + 1), TileType.Floor);
        map.SetTile(new Position(bridgeX - 1, riverY), TileType.Floor);
        map.SetTile(new Position(bridgeX - 1, riverY + 1), TileType.Floor);

        // 散策路
        DrawHorizontalRoad(map, height / 2, 2, width - 3);

        // フィールドの入口（南側中央）にスポーンポイントを設定
        int fieldMidX = width / 2;
        var fieldEntrance = new Position(fieldMidX, height - 1);
        map.SetEntrance(fieldEntrance);

        return map;
    }

    /// <summary>
    /// スタート地点固有マップを生成する（StartingMapResolverのマップ名に対応）
    /// </summary>
    public DungeonMap GenerateStartLocationMap(string mapName)
    {
        return mapName switch
        {
            // 王都系
            "capital_guild" => GenerateFacilityMap(mapName, "王都・冒険者ギルド"),
            "capital_barracks" => GenerateFacilityMap(mapName, "王都・兵舎"),
            "capital_academy" => GenerateFacilityMap(mapName, "王都・学院"),
            "capital_market" => GenerateTownMap(mapName, "王都・市場通り"),
            "capital_slums" => GenerateVillageMap(mapName, "王都・貧民街"),
            "capital_manor" => GenerateFacilityMap(mapName, "王都・貴族邸"),
            "capital_cathedral" => GenerateShrineMap(mapName, "王都・大聖堂"),
            "capital_prison" => GenerateFacilityMap(mapName, "王都・牢獄"),
            "capital_monastery" => GenerateShrineMap(mapName, "王都・修道院"),
            // 種族系
            "forest_village" => GenerateVillageMap(mapName, "森の集落"),
            "mountain_hold" => GenerateVillageMap(mapName, "山岳砦"),
            "coast_port" => GenerateTownMap(mapName, "海岸港町"),
            // 特殊種族系
            "underground_ruins" => GenerateShrineMap(mapName, "地下遺跡"),
            "dark_sanctuary" => GenerateShrineMap(mapName, "暗黒聖域"),
            "fallen_temple" => GenerateShrineMap(mapName, "堕天の神殿"),
            "swamp_den" => GenerateFieldMap(mapName, "沼地の洞窟"),
            "wanderer_camp" => GenerateFieldMap(mapName, "流浪者の野営地"),
            // デフォルト
            _ => GenerateTownMap(mapName, mapName)
        };
    }

    #region Helper Methods

    private static void FillAll(DungeonMap map, TileType type)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                map.SetTile(new Position(x, y), type);
            }
        }
    }

    private static void FillRect(DungeonMap map, int x, int y, int w, int h, TileType type)
    {
        for (int dx = 0; dx < w; dx++)
        {
            for (int dy = 0; dy < h; dy++)
            {
                map.SetTile(new Position(x + dx, y + dy), type);
            }
        }
    }

    /// <summary>
    /// 建物系ロケーションの出口ドアを配置（ドア + StairsUp）
    /// プレイヤーがStairsUp上でShift+<を押すとロケーションを出る
    /// </summary>
    private static void PlaceExitDoor(DungeonMap map, int x, int y)
    {
        // ドアの前後を通行可能にする
        map.SetTile(new Position(x, y - 1), TileType.Floor);
        map.SetTile(new Position(x, y), TileType.DoorClosed);
        map.SetTile(new Position(x, y + 1), TileType.Floor);
        // ドアの先（出口側）にStairsUpを配置
        map.SetStairsUp(new Position(x, y + 1));
        map.SetEntrance(new Position(x, y + 1));
    }

    /// <summary>
    /// ロケーションマップの外周を通行可能タイルにする（壁充填マップ用）
    /// プレイヤーが外周端から外方向へ移動するとTryLeaveTownが発動する
    /// </summary>
    private static void DrawPassableBorder(DungeonMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            map.SetTile(new Position(x, 0), TileType.Floor);
            map.SetTile(new Position(x, map.Height - 1), TileType.Floor);
        }
        for (int y = 0; y < map.Height; y++)
        {
            map.SetTile(new Position(0, y), TileType.Floor);
            map.SetTile(new Position(map.Width - 1, y), TileType.Floor);
        }
    }

    private static void DrawHorizontalRoad(DungeonMap map, int y, int xStart, int xEnd)
    {
        for (int x = xStart; x <= xEnd; x++)
        {
            map.SetTile(new Position(x, y), TileType.Floor);
            // 道幅2
            if (y + 1 < map.Height)
                map.SetTile(new Position(x, y + 1), TileType.Floor);
        }
    }

    private static void DrawVerticalRoad(DungeonMap map, int x, int yStart, int yEnd)
    {
        for (int y = yStart; y <= yEnd; y++)
        {
            map.SetTile(new Position(x, y), TileType.Floor);
            if (x + 1 < map.Width)
                map.SetTile(new Position(x + 1, y), TileType.Floor);
        }
    }

    /// <summary>
    /// 矩形の建物を配置（壁で囲み、入口にドアを配置）
    /// </summary>
    private void PlaceBuilding(DungeonMap map, int x, int y, int w, int h)
    {
        // 壁で囲む
        for (int dx = 0; dx < w; dx++)
        {
            for (int dy = 0; dy < h; dy++)
            {
                var pos = new Position(x + dx, y + dy);
                if (dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1)
                {
                    map.SetTile(pos, TileType.Wall);
                }
                else
                {
                    map.SetTile(pos, TileType.Floor);
                }
            }
        }

        // 入口（南側中央にドア）
        int doorX = x + w / 2;
        int doorY = y + h - 1;
        if (map.IsInBounds(new Position(doorX, doorY)))
        {
            map.SetTile(new Position(doorX, doorY), TileType.DoorClosed);
        }
    }

    #endregion
}
