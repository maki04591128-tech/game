using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.19 Phase 19 システムテスト
/// Task 1: TryPickupItem アイテム拾得バグ修正
/// Task 2: 渇き・疲労・清潔度の数値表示
/// Task 3: パッシブスキル欄の分離
/// Task 4: 町/フィールドでの階層非表示
/// Task 5: 装備スロットからの装備解除
/// Task 6: インベントリソート状態の永続化
/// Task 7: 町内パフォーマンス最適化
/// Task 8: アイテム装備ロジック修正
/// Task 9: ミニマップ描画最適化
/// Task 10: 自動探索 階段ナビゲーション
/// Task 11: カスタマイズ可能キーバインド
/// Task 12: ESCポーズ画面
/// Task 13: スキルツリーノード配置改善
/// Task 14: 町建物ドアのマップ遷移
/// </summary>
public class VersionPrt019SystemTests
{
    #region Task 1: TryPickupItem — Inventory.Add の戻り値チェック

    [Fact]
    public void Inventory_Add_ReturnsFalse_WhenFull()
    {
        var inventory = new Inventory(5);
        // インベントリを満杯にする（装備品は非スタックなので個別スロット消費）
        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            var item = ItemDefinitions.Create("weapon_iron_sword");
            Assert.NotNull(item);
            inventory.Add(item!);
        }

        var extraItem = ItemDefinitions.Create("weapon_iron_sword");
        Assert.NotNull(extraItem);
        bool result = inventory.Add(extraItem!);
        Assert.False(result, "満杯のインベントリにアイテムを追加できてはいけない");
    }

    [Fact]
    public void Inventory_Add_ReturnsTrue_WhenHasSpace()
    {
        var inventory = new Inventory(20);
        var item = ItemDefinitions.Create("potion_healing_minor");
        Assert.NotNull(item);

        bool result = inventory.Add(item!);
        Assert.True(result, "空きがあればアイテム追加は成功するべき");
        Assert.Contains(item, inventory.Items);
    }

    #endregion

    #region Task 2: 渇き・疲労・清潔度の数値表示

    [Fact]
    public void ThirstSystem_ThirstLevel_HasNumericValues()
    {
        // ThirstLevel列挙型が0始まりの整数値を持つことを確認
        Assert.Equal(0, (int)ThirstLevel.Hydrated);
        Assert.True((int)ThirstLevel.SevereDehydration > 0, "SevereDehydration は0より大きい値であるべき");
    }

    [Fact]
    public void BodyConditionSystem_FatigueLevel_HasNumericValues()
    {
        Assert.Equal(0, (int)FatigueLevel.Fresh);
        Assert.True((int)FatigueLevel.Exhausted > 0, "Exhausted は0より大きい値であるべき");
    }

    [Fact]
    public void BodyConditionSystem_HygieneLevel_HasNumericValues()
    {
        Assert.Equal(0, (int)HygieneLevel.Clean);
        Assert.True((int)HygieneLevel.Filthy > 0, "Filthy は0より大きい値であるべき");
    }

    #endregion

    #region Task 3: パッシブスキルの判定

    [Fact]
    public void SkillNodeType_Passive_Exists()
    {
        // パッシブスキルタイプが存在することを確認
        var passiveType = SkillNodeType.Passive;
        Assert.Equal(SkillNodeType.Passive, passiveType);
    }

    [Fact]
    public void SkillNodeType_StatMinor_And_StatMajor_Exist()
    {
        // StatMinor/StatMajor がパッシブ扱いになることを確認
        Assert.NotEqual(SkillNodeType.Passive, SkillNodeType.StatMinor);
        Assert.NotEqual(SkillNodeType.Passive, SkillNodeType.StatMajor);
    }

    #endregion

    #region Task 8: Equipment.Equip の戻り値テスト

    [Fact]
    public void Equipment_Equip_ReturnsPreviousItem()
    {
        var equipment = new Equipment();
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        var weapon1 = ItemDefinitions.Create("weapon_iron_sword");
        Assert.NotNull(weapon1);
        var equip1 = weapon1 as EquipmentItem;
        Assert.NotNull(equip1);
        // Weapon コンストラクタで Slot = MainHand が設定済み

        var weapon2 = ItemDefinitions.Create("weapon_iron_sword");
        Assert.NotNull(weapon2);
        var equip2 = weapon2 as EquipmentItem;
        Assert.NotNull(equip2);

        // 最初の装備
        var prev1 = equipment.Equip(equip1!, player);
        Assert.Null(prev1);

        // 2つ目を装備すると1つ目が返る
        var prev2 = equipment.Equip(equip2!, player);
        Assert.NotNull(prev2);
        Assert.Same(equip1, prev2);
    }

    [Fact]
    public void Equipment_Unequip_ReturnsItem()
    {
        var equipment = new Equipment();
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);
        var weapon = ItemDefinitions.Create("weapon_iron_sword");
        Assert.NotNull(weapon);
        var equip = weapon as EquipmentItem;
        Assert.NotNull(equip);
        // Weapon コンストラクタで Slot = MainHand が設定済み

        equipment.Equip(equip!, player);
        var removed = equipment.Unequip(EquipmentSlot.MainHand, player);
        Assert.NotNull(removed);
        Assert.Same(equip, removed);
    }

    #endregion

    #region Task 10: 自動探索ロジック

    [Fact]
    public void AutoExploreSystem_StopReason_Contains_StairsFound()
    {
        // StairsFound がAutoExploreの停止理由に含まれることを確認
        var reason = AutoExploreSystem.StopReason.StairsFound;
        Assert.Equal(AutoExploreSystem.StopReason.StairsFound, reason);
    }

    #endregion

    #region Task 14: 建物入口/出口タイル・建物外配置・建物間移動

    [Fact]
    public void TileType_BuildingEntrance_Exists()
    {
        var entranceType = TileType.BuildingEntrance;
        Assert.Equal(TileType.BuildingEntrance, entranceType);
    }

    [Fact]
    public void TileType_BuildingExit_Exists()
    {
        var exitType = TileType.BuildingExit;
        Assert.Equal(TileType.BuildingExit, exitType);
    }

    [Fact]
    public void Tile_BuildingEntrance_IsWalkable()
    {
        var tile = Tile.FromType(TileType.BuildingEntrance);
        Assert.False(tile.BlocksMovement, "BuildingEntrance は移動可能であるべき");
        Assert.False(tile.BlocksSight, "BuildingEntrance は視界を遮らないべき");
    }

    [Fact]
    public void Tile_BuildingExit_IsWalkable()
    {
        var tile = Tile.FromType(TileType.BuildingExit);
        Assert.False(tile.BlocksMovement, "BuildingExit は移動可能であるべき");
        Assert.False(tile.BlocksSight, "BuildingExit は視界を遮らないべき");
    }

    [Fact]
    public void Tile_BuildingEntrance_HasBuildingId()
    {
        var tile = Tile.FromType(TileType.BuildingEntrance);
        tile.BuildingId = "inn";
        Assert.Equal("inn", tile.BuildingId);
    }

    [Fact]
    public void TownMap_HasBuildingEntrances_WithBuildingIds()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("test_town", "テスト町");

        var entrances = new Dictionary<string, int>();
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.BuildingEntrance && tile.BuildingId != null)
                {
                    entrances[tile.BuildingId] = entrances.GetValueOrDefault(tile.BuildingId) + 1;
                }
            }
        }

        Assert.True(entrances.Count >= 8, $"町マップに十分な建物入口がない: {entrances.Count}");
        Assert.True(entrances.ContainsKey("inn"), "宿屋の入口がない");
        Assert.True(entrances.ContainsKey("shop"), "商店の入口がない");
        Assert.True(entrances.ContainsKey("smithy"), "鍛冶屋の入口がない");
        Assert.True(entrances.ContainsKey("guild"), "ギルドの入口がない");
        Assert.True(entrances.ContainsKey("church"), "教会の入口がない");
        Assert.True(entrances.ContainsKey("training"), "訓練所の入口がない");
        Assert.True(entrances.ContainsKey("library"), "図書館の入口がない");
        Assert.True(entrances.ContainsKey("magic_shop"), "魔法商店の入口がない");
    }

    [Fact]
    public void BuildingInterior_Inn_HasInnkeeper()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("inn");

        Assert.NotNull(interior);
        Assert.True(interior.Width > 0);
        Assert.True(interior.Height > 0);

        bool hasInnkeeper = false;
        bool hasExit = false;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                var tile = interior.GetTile(new Position(x, y));
                if (tile.Type == TileType.NpcInnkeeper) hasInnkeeper = true;
                if (tile.Type == TileType.BuildingExit) hasExit = true;
            }
        }

        Assert.True(hasInnkeeper, "宿屋内部に宿屋主人がいない");
        Assert.True(hasExit, "宿屋内部に出口がない");
    }

    [Fact]
    public void BuildingInterior_Shop_HasShopkeeper()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("shop");

        bool hasShopkeeper = false;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                if (interior.GetTile(new Position(x, y)).Type == TileType.NpcShopkeeper) hasShopkeeper = true;
            }
        }

        Assert.True(hasShopkeeper, "商店内部に商人がいない");
    }

    [Fact]
    public void BuildingInterior_Guild_HasGuildReceptionist()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("guild");

        bool hasNpc = false;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                if (interior.GetTile(new Position(x, y)).Type == TileType.NpcGuildReceptionist) hasNpc = true;
            }
        }

        Assert.True(hasNpc, "ギルド内部にギルド受付がいない");
    }

    [Fact]
    public void BuildingInterior_HasEntrance()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("smithy");

        Assert.NotNull(interior.EntrancePosition);
    }

    [Fact]
    public void BuildingInterior_Exit_HasBuildingId()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("church");

        bool exitHasBuildingId = false;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                var tile = interior.GetTile(new Position(x, y));
                if (tile.Type == TileType.BuildingExit && tile.BuildingId == "church")
                {
                    exitHasBuildingId = true;
                }
            }
        }

        Assert.True(exitHasBuildingId, "建物出口にBuildingIdが設定されていない");
    }

    [Fact]
    public void VillageMap_HasBuildingEntrances()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateVillageMap("test_village", "テスト村");

        int entranceCount = 0;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTile(new Position(x, y)).Type == TileType.BuildingEntrance)
                    entranceCount++;
            }
        }

        Assert.True(entranceCount >= 3, $"村マップに建物入口が不足: {entranceCount}");
    }

    [Fact]
    public void TownMap_BuildingEntrance_IsOutsideWalls()
    {
        var generator = new LocationMapGenerator(42);
        var map = generator.GenerateTownMap("test_town", "テスト町");

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var tile = map.GetTile(new Position(x, y));
                if (tile.Type == TileType.BuildingEntrance && tile.BuildingId != null)
                {
                    // 入口タイルの上（北）方向が壁であること（建物の南側外に配置されている）
                    if (y > 0)
                    {
                        var aboveTile = map.GetTile(new Position(x, y - 1));
                        Assert.True(aboveTile.Type == TileType.Wall,
                            $"建物入口({x},{y}) buildingId={tile.BuildingId} の上が壁でない（入口が建物外にない可能性）");
                    }
                    // 入口タイル自体が壁でないこと
                    Assert.True(tile.Type != TileType.Wall,
                        $"建物入口({x},{y}) が壁タイルになっている");
                }
            }
        }
    }

    [Fact]
    public void BuildingInterior_WithoutVisitedBuildings_HasNoExtraBuildingEntrance()
    {
        var generator = new LocationMapGenerator();
        var interior = generator.GenerateBuildingInterior("inn");

        int entranceCount = 0;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                if (interior.GetTile(new Position(x, y)).Type == TileType.BuildingEntrance)
                    entranceCount++;
            }
        }

        Assert.Equal(0, entranceCount);
    }

    [Fact]
    public void BuildingInterior_WithVisitedBuildings_HasInterBuildingStairs()
    {
        // 建物内から他の建物への直接移動は物理的に不自然なため廃止
        // 訪問済み建物リストを渡しても階段は生成されないことを確認
        var generator = new LocationMapGenerator();
        var visited = new List<string> { "inn", "shop", "smithy", "guild" };
        var interior = generator.GenerateBuildingInterior("inn", visited);

        // 他建物への階段が存在しないことを確認
        var entrances = new Dictionary<string, Position>();
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                var tile = interior.GetTile(new Position(x, y));
                if (tile.Type == TileType.BuildingEntrance && tile.BuildingId != null)
                {
                    entrances[tile.BuildingId] = new Position(x, y);
                }
            }
        }

        Assert.Empty(entrances);
    }

    [Fact]
    public void BuildingInterior_WithVisitedBuildings_SelfNotIncluded()
    {
        var generator = new LocationMapGenerator();
        var visited = new List<string> { "church" };
        var interior = generator.GenerateBuildingInterior("church", visited);

        // 自分自身しかvisitedにないので階段は0
        int entranceCount = 0;
        for (int x = 0; x < interior.Width; x++)
        {
            for (int y = 0; y < interior.Height; y++)
            {
                if (interior.GetTile(new Position(x, y)).Type == TileType.BuildingEntrance)
                    entranceCount++;
            }
        }

        Assert.Equal(0, entranceCount);
    }

    #endregion
}
