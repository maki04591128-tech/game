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
            LocationType.Town => GenerateTownMap(location.Id, location.Name, location.Territory),
            LocationType.Village => GenerateVillageMap(location.Id, location.Name),
            LocationType.Facility => GenerateFacilityMap(location.Id, location.Name),
            LocationType.ReligiousSite => GenerateShrineMap(location.Id, location.Name),
            LocationType.Field => GenerateFieldMap(location.Id, location.Name),
            _ => GenerateTownMap(location.Id, location.Name, location.Territory)
        };
    }

    /// <summary>
    /// 町マップを生成（50x30）
    /// 領地ごとに異なるデザインのマップを返す
    /// </summary>
    public DungeonMap GenerateTownMap(string locationId, string locationName, TerritoryId territory = TerritoryId.Capital)
    {
        return territory switch
        {
            TerritoryId.Capital => GenerateCapitalTown(locationId),
            TerritoryId.Forest => GenerateForestTown(locationId),
            TerritoryId.Mountain => GenerateMountainTown(locationId),
            TerritoryId.Coast => GenerateCoastTown(locationId),
            TerritoryId.Southern => GenerateSouthernTown(locationId),
            TerritoryId.Frontier => GenerateFrontierTown(locationId),
            _ => GenerateCapitalTown(locationId)
        };
    }

    /// <summary>王都領の街 - 石畳の大通りと大きな建物が並ぶ壮麗な首都</summary>
    private DungeonMap GenerateCapitalTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 十字の大通り
        int midX = width / 2;
        int midY = height / 2;
        DrawHorizontalRoad(map, midY, 2, width - 3);
        DrawVerticalRoad(map, midX, 2, height - 3);

        // 中央広場（7x7の石畳）と噴水
        FillRect(map, midX - 3, midY - 3, 7, 7, TileType.Floor);
        map.SetTile(new Position(midX, midY), TileType.Fountain);
        // 広場の四隅に柱
        map.SetTile(new Position(midX - 2, midY - 2), TileType.Pillar);
        map.SetTile(new Position(midX + 2, midY - 2), TileType.Pillar);
        map.SetTile(new Position(midX - 2, midY + 2), TileType.Pillar);
        map.SetTile(new Position(midX + 2, midY + 2), TileType.Pillar);

        // 建物配置: 王都は大きくて立派な建物（重ならないよう4象限に配置）
        PlaceBuilding(map, 2, 2, 10, 6, "inn");                // 左上: 大きな宿屋
        PlaceBuilding(map, 14, 2, 7, 5, "library");            // 左上寄り: 王立図書館
        PlaceBuilding(map, width - 13, 2, 10, 6, "shop");      // 右上: 大商店
        PlaceBuilding(map, midX + 3, 2, 7, 5, "church");       // 右上寄り: 大聖堂
        PlaceBuilding(map, 2, height - 8, 10, 6, "smithy");    // 左下: 王立鍛冶場
        PlaceBuilding(map, 14, height - 8, 7, 5, "training");  // 左下寄り: 闘技場
        PlaceBuilding(map, width - 13, height - 8, 10, 6, "guild"); // 右下: ギルド本部
        PlaceBuilding(map, midX + 3, height - 8, 7, 5, "magic_shop"); // 右下寄り: 魔法学院

        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);
        return map;
    }

    /// <summary>森林領の街 - 樹木が点在する自然と調和した都市</summary>
    private DungeonMap GenerateForestTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 緑の中に蛇行する小道
        int midX = width / 2;
        int midY = height / 2;
        for (int x = 2; x < width - 2; x++)
        {
            int roadY = midY + (int)(Math.Sin(x * 0.2) * 2);
            roadY = Math.Clamp(roadY, 2, height - 3);
            map.SetTile(new Position(x, roadY), TileType.Floor);
            if (roadY + 1 < height) map.SetTile(new Position(x, roadY + 1), TileType.Floor);
        }
        DrawVerticalRoad(map, midX, 2, height - 3);

        // 散らばる木々（街の中にも自然が残る）
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_random.NextDouble() < 0.12 && map.GetTile(new Position(x, y)).Type == TileType.Grass)
                    map.SetTile(new Position(x, y), TileType.Tree);

        // 中央に世界樹風の大木（噴水で代用）
        FillRect(map, midX - 2, midY - 2, 5, 5, TileType.Floor);
        map.SetTile(new Position(midX, midY), TileType.Fountain);

        // 建物は小さめで自然に馴染む配置（重複なし）
        PlaceBuilding(map, 4, 3, 7, 5, "inn");
        PlaceBuilding(map, width - 12, 3, 7, 5, "shop");
        PlaceBuilding(map, 4, height - 9, 7, 5, "smithy");
        PlaceBuilding(map, width - 12, height - 9, 7, 5, "guild");
        PlaceBuilding(map, midX + 5, 3, 6, 5, "church");
        PlaceBuilding(map, 4, midY - 1, 6, 5, "library");

        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);
        return map;
    }

    /// <summary>山岳領の街 - 岩壁に囲まれた堅固な要塞都市</summary>
    private DungeonMap GenerateMountainTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Floor);

        // 外周を岩壁（Wall）で囲む
        for (int x = 0; x < width; x++)
        {
            map.SetTile(new Position(x, 0), TileType.Wall);
            map.SetTile(new Position(x, height - 1), TileType.Wall);
        }
        for (int y = 0; y < height; y++)
        {
            map.SetTile(new Position(0, y), TileType.Wall);
            map.SetTile(new Position(width - 1, y), TileType.Wall);
        }

        // 内壁でセクション分け（要塞感を演出）
        int midX = width / 2;
        int midY = height / 2;

        // 横通路
        DrawHorizontalRoad(map, midY, 1, width - 2);
        // 縦通路
        DrawVerticalRoad(map, midX, 1, height - 2);

        // 岩壁をランダムに点在（山岳感）
        for (int x = 2; x < width - 2; x++)
            for (int y = 2; y < height - 2; y++)
                if (_random.NextDouble() < 0.06 && map.GetTile(new Position(x, y)).Type == TileType.Floor
                    && Math.Abs(x - midX) > 2 && Math.Abs(y - midY) > 2)
                    map.SetTile(new Position(x, y), TileType.Pillar);

        // 中央に鍛冶の炉（祭壇で代用）
        FillRect(map, midX - 2, midY - 2, 5, 5, TileType.Floor);
        map.SetTile(new Position(midX, midY), TileType.Altar);

        // 建物: 頑丈な石造りの大きな建物
        PlaceBuilding(map, 3, 3, 9, 6, "smithy");            // 左上: 大鍛冶場
        PlaceBuilding(map, width - 13, 3, 9, 6, "shop");      // 右上: 鉱物商店
        PlaceBuilding(map, 3, height - 10, 9, 6, "inn");      // 左下: 山岳宿屋
        PlaceBuilding(map, width - 13, height - 10, 9, 6, "guild"); // 右下: ギルド支部
        PlaceBuilding(map, midX + 4, 3, 7, 5, "training");    // 訓練場
        PlaceBuilding(map, midX + 4, height - 9, 7, 5, "church"); // 山頂の祠

        // 門（南側中央に出入口）
        map.SetTile(new Position(midX, height - 1), TileType.Floor);
        map.SetTile(new Position(midX + 1, height - 1), TileType.Floor);
        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);
        return map;
    }

    /// <summary>海岸領の街 - 港と水辺が特徴的な海運都市</summary>
    private DungeonMap GenerateCoastTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 右側を海（水タイル）で埋める
        for (int x = width - 10; x < width; x++)
            for (int y = 0; y < height; y++)
                map.SetTile(new Position(x, y), TileType.Water);

        // 海岸線をランダム化
        for (int y = 0; y < height; y++)
        {
            int shore = width - 10 + _random.Next(-1, 2);
            shore = Math.Clamp(shore, width - 12, width - 8);
            for (int x = shore; x < width; x++)
                map.SetTile(new Position(x, y), TileType.Water);
        }

        // 桟橋（Floor）を海に突き出す
        int pierY = height / 2;
        for (int x = width - 12; x < width - 4; x++)
        {
            map.SetTile(new Position(x, pierY), TileType.Floor);
            map.SetTile(new Position(x, pierY + 1), TileType.Floor);
        }

        // 町の通り
        int midX = width / 3;
        int midY = height / 2;
        DrawHorizontalRoad(map, midY, 2, width - 13);
        DrawVerticalRoad(map, midX, 2, height - 3);

        // 中央に灯台風（噴水代用）
        FillRect(map, midX - 1, midY - 1, 3, 3, TileType.Floor);
        map.SetTile(new Position(midX, midY), TileType.Fountain);

        // 建物
        PlaceBuilding(map, 3, 3, 8, 6, "inn");               // 港の宿屋
        PlaceBuilding(map, midX + 4, 3, 8, 6, "shop");        // 海産物商店
        PlaceBuilding(map, 3, height - 10, 8, 6, "guild");    // 海運ギルド
        PlaceBuilding(map, midX + 4, height - 10, 8, 6, "smithy"); // 船大工

        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);
        return map;
    }

    /// <summary>南部領の街 - 城壁に守られた貴族の城下町</summary>
    private DungeonMap GenerateSouthernTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 城壁（外周をWallで囲む）
        for (int x = 2; x < width - 2; x++)
        {
            map.SetTile(new Position(x, 2), TileType.Wall);
            map.SetTile(new Position(x, height - 3), TileType.Wall);
        }
        for (int y = 2; y < height - 2; y++)
        {
            map.SetTile(new Position(2, y), TileType.Wall);
            map.SetTile(new Position(width - 3, y), TileType.Wall);
        }

        // 城壁内部を石畳に
        FillRect(map, 3, 3, width - 6, height - 6, TileType.Floor);

        // 十字大通り
        int midX = width / 2;
        int midY = height / 2;

        // 中央に城（大きなWall建物）
        FillRect(map, midX - 5, 4, 11, 8, TileType.Wall);
        FillRect(map, midX - 3, 6, 7, 4, TileType.Floor); // 城内部
        // 城入口を壁の外側（南1マス下）に配置
        map.SetTile(new Position(midX, 12), TileType.BuildingEntrance);
        var castleDoor = map.GetTile(new Position(midX, 12));
        castleDoor.BuildingId = "castle";

        // 建物（重複なし配置: inn/shopを南側、guild/church/trainingを中央帯に分離）
        PlaceBuilding(map, 4, height - 10, 8, 6, "inn");
        PlaceBuilding(map, width - 13, height - 10, 8, 6, "shop");
        PlaceBuilding(map, 4, midY + 2, 7, 5, "guild");
        PlaceBuilding(map, width - 12, midY + 2, 7, 5, "church");
        PlaceBuilding(map, midX + 6, midY + 2, 7, 5, "training");

        // 城門（南側中央）
        map.SetTile(new Position(midX, height - 3), TileType.Floor);
        map.SetTile(new Position(midX + 1, height - 3), TileType.Floor);

        var entrancePos = new Position(midX, height - 2);
        map.SetEntrance(entrancePos);
        return map;
    }

    /// <summary>辺境領の街 - 荒廃した砦風の粗末な拠点</summary>
    private DungeonMap GenerateFrontierTown(string locationId)
    {
        const int width = 50;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = locationId };

        FillAll(map, TileType.Grass);

        // 壊れかけた柵（Wall+隙間）で囲む
        for (int x = 3; x < width - 3; x++)
        {
            if (_random.NextDouble() < 0.7) map.SetTile(new Position(x, 3), TileType.Wall);
            if (_random.NextDouble() < 0.7) map.SetTile(new Position(x, height - 4), TileType.Wall);
        }
        for (int y = 3; y < height - 3; y++)
        {
            if (_random.NextDouble() < 0.7) map.SetTile(new Position(3, y), TileType.Wall);
            if (_random.NextDouble() < 0.7) map.SetTile(new Position(width - 4, y), TileType.Wall);
        }

        // 内部は荒れた地面（Floor+Grass混在）
        for (int x = 4; x < width - 4; x++)
            for (int y = 4; y < height - 4; y++)
                map.SetTile(new Position(x, y), _random.NextDouble() < 0.6 ? TileType.Floor : TileType.Grass);

        // 不規則な道
        int midX = width / 2;
        int midY = height / 2;
        DrawHorizontalRoad(map, midY, 4, width - 5);

        // 焚き火（噴水代用）
        map.SetTile(new Position(midX, midY), TileType.Fountain);

        // 建物: 小さくて粗末
        PlaceBuilding(map, 6, 5, 7, 5, "inn");                // ボロ宿
        PlaceBuilding(map, width - 14, 5, 7, 5, "shop");       // 雑貨屋
        PlaceBuilding(map, 6, height - 10, 7, 5, "guild");     // ギルド小屋
        PlaceBuilding(map, width - 14, height - 10, 7, 5, "smithy"); // 修繕屋

        // 門（南側）
        map.SetTile(new Position(midX, height - 4), TileType.Floor);
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
        PlaceBuilding(map, 4, 4, 6, 5, "village_house_1");
        PlaceBuilding(map, 14, 4, 6, 5, "village_house_2");
        PlaceBuilding(map, 4, midY + 3, 6, 5, "village_house_3");

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
    /// シンボルマップのタイル属性に応じたフィールドマップを自動生成する
    /// Elona風の「任意のタイルに入れる」機能用
    /// </summary>
    public DungeonMap GenerateTerrainFieldMap(TileType symbolTileType, Position symbolPosition)
    {
        string terrainKey = symbolTileType switch
        {
            TileType.SymbolGrass => "grassland",
            TileType.SymbolForest => "forest",
            TileType.SymbolMountain => "mountain",
            TileType.SymbolWater => "waterfront",
            TileType.SymbolRoad => "road",
            _ => "grassland"
        };
        string mapId = $"field_{terrainKey}_{symbolPosition.X}_{symbolPosition.Y}";
        return symbolTileType switch
        {
            TileType.SymbolGrass => GenerateGrasslandMap(mapId),
            TileType.SymbolForest => GenerateForestMap(mapId),
            TileType.SymbolMountain => GenerateMountainMap(mapId),
            TileType.SymbolWater => GenerateWaterfrontMap(mapId),
            TileType.SymbolRoad => GenerateRoadFieldMap(mapId),
            _ => GenerateGrasslandMap(mapId)
        };
    }

    /// <summary>草原フィールドマップ（60x30）</summary>
    public DungeonMap GenerateGrasslandMap(string mapId)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = mapId };

        FillAll(map, TileType.Grass);

        // まばらに木を配置（8%）
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_random.NextDouble() < 0.08)
                    map.SetTile(new Position(x, y), TileType.Tree);

        // 小さな池
        int pondX = width / 3 + _random.Next(10);
        int pondY = height / 3 + _random.Next(5);
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                var p = new Position(pondX + dx, pondY + dy);
                if (map.IsInBounds(p)) map.SetTile(p, TileType.Water);
            }

        map.SetEntrance(new Position(width / 2, height - 1));
        return map;
    }

    /// <summary>森林フィールドマップ（60x30）</summary>
    public DungeonMap GenerateForestMap(string mapId)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = mapId };

        FillAll(map, TileType.Grass);

        // 密な木（45%）
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_random.NextDouble() < 0.45)
                    map.SetTile(new Position(x, y), TileType.Tree);

        // 獣道（横通路）を確保
        DrawHorizontalRoad(map, height / 2, 1, width - 2);
        // 獣道（縦通路）を確保
        DrawVerticalRoad(map, width / 3, 1, height - 2);
        DrawVerticalRoad(map, width * 2 / 3, 1, height - 2);

        map.SetEntrance(new Position(width / 2, height - 1));
        return map;
    }

    /// <summary>山岳フィールドマップ（60x30）</summary>
    public DungeonMap GenerateMountainMap(string mapId)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = mapId };

        FillAll(map, TileType.Grass);

        // 岩壁（壁タイル）を密に配置（35%）
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_random.NextDouble() < 0.35)
                    map.SetTile(new Position(x, y), TileType.Wall);

        // 山道（通行可能な通路）を確保
        DrawHorizontalRoad(map, height / 2, 0, width - 1);
        DrawVerticalRoad(map, width / 2, 0, height - 1);

        // 鉱脈っぽい地面（Pillarタイル）を少し配置
        for (int i = 0; i < 5; i++)
        {
            int rx = 2 + _random.Next(width - 4);
            int ry = 2 + _random.Next(height - 4);
            var p = new Position(rx, ry);
            if (map.IsInBounds(p) && !map.GetTile(p).BlocksMovement)
                map.SetTile(p, TileType.Pillar);
        }

        map.SetEntrance(new Position(width / 2, height - 1));
        return map;
    }

    /// <summary>水辺フィールドマップ（60x30）</summary>
    public DungeonMap GenerateWaterfrontMap(string mapId)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = mapId };

        // 上半分は草地、下半分は水
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y < height / 2)
                    map.SetTile(new Position(x, y), TileType.Grass);
                else
                    map.SetTile(new Position(x, y), TileType.Water);
            }
        }

        // 汀線をランダム化
        for (int x = 0; x < width; x++)
        {
            int shoreline = height / 2 + (int)(Math.Sin(x * 0.4) * 2) + _random.Next(-1, 2);
            shoreline = Math.Clamp(shoreline, 2, height - 2);
            for (int y = shoreline; y < height; y++)
                map.SetTile(new Position(x, y), TileType.Water);
            for (int y = 0; y < shoreline; y++)
                if (map.GetTile(new Position(x, y)).Type == TileType.Water)
                    map.SetTile(new Position(x, y), TileType.Grass);
        }

        // 砂浜エリア（水際に沿った通行可能地帯）
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.Grass)
                {
                    // 隣が水のタイルは砂浜（Floor）にする
                    bool nearWater = false;
                    foreach (var d in new[] { new Position(x, y - 1), new Position(x, y + 1), new Position(x - 1, y), new Position(x + 1, y) })
                    {
                        if (map.IsInBounds(d) && map.GetTile(d).Type == TileType.Water)
                        { nearWater = true; break; }
                    }
                    if (nearWater) map.SetTile(new Position(x, y), TileType.Floor);
                }
            }
        }

        // 木を少し配置
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height / 2 - 2; y++)
                if (_random.NextDouble() < 0.12)
                    map.SetTile(new Position(x, y), TileType.Tree);

        map.SetEntrance(new Position(width / 2, 0));
        return map;
    }

    /// <summary>道沿いフィールドマップ（60x30）</summary>
    public DungeonMap GenerateRoadFieldMap(string mapId)
    {
        const int width = 60;
        const int height = 30;
        var map = new DungeonMap(width, height) { Depth = 0, Name = mapId };

        FillAll(map, TileType.Grass);

        // 木をまばらに配置（12%）
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_random.NextDouble() < 0.12)
                    map.SetTile(new Position(x, y), TileType.Tree);

        // 中央に道路を敷く
        DrawHorizontalRoad(map, height / 2, 0, width - 1);
        DrawVerticalRoad(map, width / 2, 0, height - 1);

        map.SetEntrance(new Position(width / 2, height - 1));
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
            "capital_market" => GenerateTownMap(mapName, "王都・市場通り", TerritoryId.Capital),
            "capital_slums" => GenerateVillageMap(mapName, "王都・貧民街"),
            "capital_manor" => GenerateFacilityMap(mapName, "王都・貴族邸"),
            "capital_cathedral" => GenerateShrineMap(mapName, "王都・大聖堂"),
            "capital_prison" => GenerateFacilityMap(mapName, "王都・牢獄"),
            "capital_monastery" => GenerateShrineMap(mapName, "王都・修道院"),
            // 種族系
            "forest_village" => GenerateVillageMap(mapName, "森の集落"),
            "mountain_hold" => GenerateVillageMap(mapName, "山岳砦"),
            "coast_port" => GenerateTownMap(mapName, "海岸港町", TerritoryId.Coast),
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
    /// 矩形の建物を配置（壁で囲み、入口タイルを建物の外側に配置）
    /// </summary>
    private void PlaceBuilding(DungeonMap map, int x, int y, int w, int h, string? buildingId = null)
    {
        // 壁で囲む（内部は壁で埋める — 建物内部は別マップ）
        for (int dx = 0; dx < w; dx++)
        {
            for (int dy = 0; dy < h; dy++)
            {
                var pos = new Position(x + dx, y + dy);
                map.SetTile(pos, TileType.Wall);
            }
        }

        // 入口を建物の外側（南1マス下）に配置
        int doorX = x + w / 2;
        int doorY = y + h; // 壁の1マス外側
        var doorPos = new Position(doorX, doorY);
        if (map.IsInBounds(doorPos))
        {
            map.SetTile(doorPos, TileType.BuildingEntrance);
            if (buildingId != null)
            {
                var tile = map.GetTile(doorPos);
                tile.BuildingId = buildingId;
            }
        }

        // 入口の外側（さらに南1マス下）も通行可能にして確実にアクセスできるようにする
        var accessPos = new Position(doorX, doorY + 1);
        if (map.IsInBounds(accessPos) && map.GetTile(accessPos).BlocksMovement)
        {
            map.SetTile(accessPos, TileType.Floor);
        }
    }

    /// <summary>
    /// 建物内部マップを生成（建物IDに応じた内装とNPCを配置）
    /// </summary>
    /// <param name="buildingId">生成する建物のID</param>
    /// <param name="visitedBuildings">訪問済み建物IDのリスト（階段で移動可能な他建物）</param>
    public DungeonMap GenerateBuildingInterior(string buildingId, IReadOnlyList<string>? visitedBuildings = null)
    {
        const int width = 12;
        const int height = 10;
        var map = new DungeonMap(width, height) { Depth = 0, Name = $"building_{buildingId}" };

        // 壁で囲んだ部屋
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    map.SetTile(new Position(x, y), TileType.Wall);
                else
                    map.SetTile(new Position(x, y), TileType.Floor);
            }
        }

        // 出口（南側中央）
        int exitX = width / 2;
        int exitY = height - 1;
        var exitPos = new Position(exitX, exitY);
        map.SetTile(exitPos, TileType.BuildingExit);
        var exitTile = map.GetTile(exitPos);
        exitTile.BuildingId = buildingId;

        // スポーンポイント（出口の1マス上）
        map.SetEntrance(new Position(exitX, exitY - 1));

        // 建物IDに応じたNPC配置
        var npcPos = new Position(width / 2, 2);
        switch (buildingId)
        {
            case "inn":
                map.SetTile(npcPos, TileType.NpcInnkeeper);
                break;
            case "shop":
            case "magic_shop":
                map.SetTile(npcPos, TileType.NpcShopkeeper);
                break;
            case "smithy":
                map.SetTile(npcPos, TileType.NpcBlacksmith);
                break;
            case "guild":
                map.SetTile(npcPos, TileType.NpcGuildReceptionist);
                break;
            case "church":
                map.SetTile(npcPos, TileType.NpcPriest);
                break;
            case "training":
                map.SetTile(npcPos, TileType.NpcTrainer);
                break;
            case "library":
                map.SetTile(npcPos, TileType.NpcLibrarian);
                break;
        }

        // 建物内から他の建物への直接移動は物理的に不自然なため廃止
        // プレイヤーは一度建物を出てから別の建物に入る必要がある

        return map;
    }

    #endregion
}
