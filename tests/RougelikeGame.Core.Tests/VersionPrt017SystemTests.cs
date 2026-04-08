using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Map;

#pragma warning disable CS0618 // Obsolete旧enum互換テスト維持

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.17 Phase 17 システムテスト
/// Task 1: ショップ購入時インベントリ空きチェック
/// Task 2: 自動探索の階段移動（統合テストのため単体テスト困難、BFS関連テストで代替）
/// Task 3: スキルツリーノード選択Y座標反転（GUIテストのためCoreではCanUnlockテスト）
/// Task 4: スキルツリー解放ボタン（CanUnlock + UnlockNodeテスト）
/// Task 5: 町の移動パフォーマンス（構造変更のため単体テスト不要）
/// Task 6: 渇き悪化速度（定数テスト）
/// Task 7: 疲労悪化速度（定数テスト）
/// Task 8: 宿屋で疲労・衛生回復（RestAtInnテスト）
/// </summary>
public class VersionPrt017SystemTests
{
    #region Task 1: ショップ購入 - インベントリ容量チェック

    [Fact]
    public void Inventory_Add_ReturnsFalse_WhenFull()
    {
        var inventory = new Inventory(2);
        var item1 = ItemDefinitions.Create("food_bread");
        var item2 = ItemDefinitions.Create("food_water");
        var item3 = ItemDefinitions.Create("potion_healing");

        Assert.NotNull(item1);
        Assert.NotNull(item2);
        Assert.NotNull(item3);

        Assert.True(inventory.Add(item1!));
        Assert.True(inventory.Add(item2!));
        Assert.False(inventory.Add(item3!)); // 満杯で追加失敗
    }

    [Fact]
    public void Inventory_Add_StackableItem_SucceedsEvenWhenFull()
    {
        var inventory = new Inventory(1);
        var item1 = ItemDefinitions.Create("potion_healing");
        var item2 = ItemDefinitions.Create("potion_healing");

        Assert.NotNull(item1);
        Assert.NotNull(item2);

        Assert.True(inventory.Add(item1!));
        // スタック可能なら満杯でもスタック追加できる
        if (item1 is IStackable && item2 is IStackable)
        {
            Assert.True(inventory.Add(item2!));
        }
    }

    [Fact]
    public void ShopSystem_Buy_ReturnsItemId_OnSuccess()
    {
        var shop = new ShopSystem();
        shop.InitializeShop(FacilityType.GeneralShop, TerritoryId.Capital, 1);
        var items = shop.GetShopItems(FacilityType.GeneralShop);
        Assert.NotEmpty(items);

        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Soldier);
        player.AddGold(10000);

        var result = shop.Buy(player, FacilityType.GeneralShop, 0, 0.0);
        Assert.True(result.Success);
        Assert.NotNull(result.ItemId);

        // ItemDefinitionsでアイテム作成可能
        var created = ItemDefinitions.Create(result.ItemId!);
        Assert.NotNull(created);
    }

    #endregion

    #region Task 3-4: スキルツリー CanUnlock / UnlockNode

    [Fact]
    public void SkillTreeSystem_CanUnlock_WithPoints_ReturnsTrue()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);

        // shared_hp_1: Tier1, RequiredLevel=1, PointCost=1, Prerequisites=[]
        bool canUnlock = tree.CanUnlock("shared_hp_1", 1);
        Assert.True(canUnlock);
    }

    [Fact]
    public void SkillTreeSystem_CanUnlock_WithoutPoints_ReturnsFalse()
    {
        var tree = new SkillTreeSystem();
        // ポイントなし
        bool canUnlock = tree.CanUnlock("shared_hp_1", 1);
        Assert.False(canUnlock);
    }

    [Fact]
    public void SkillTreeSystem_CanUnlock_LevelTooLow_ReturnsFalse()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);

        // shared_hp_2: RequiredLevel=5
        bool canUnlock = tree.CanUnlock("shared_hp_2", 3);
        Assert.False(canUnlock);
    }

    [Fact]
    public void SkillTreeSystem_UnlockNode_DeductsPoints()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(3);

        bool unlocked = tree.UnlockNode("shared_hp_1", 1); // PointCost=1
        Assert.True(unlocked);
        Assert.Equal(2, tree.AvailablePoints);
        Assert.Contains("shared_hp_1", tree.UnlockedNodes);
    }

    [Fact]
    public void SkillTreeSystem_UnlockNode_WithPrerequisites_RequiresParent()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);

        // shared_hp_2は shared_hp_1が前提
        Assert.False(tree.CanUnlock("shared_hp_2", 5));

        tree.UnlockNode("shared_hp_1", 1);
        Assert.True(tree.CanUnlock("shared_hp_2", 5));
    }

    [Fact]
    public void SkillTreeSystem_AlreadyUnlocked_CannotUnlockAgain()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);

        tree.UnlockNode("shared_hp_1", 1);
        Assert.False(tree.CanUnlock("shared_hp_1", 1)); // 既に解放済み
    }

    #endregion

    #region Task 6: 渇き悪化速度 - 満腹度と同じ間隔

    [Fact]
    public void HungerDecayInterval_Equals864()
    {
        Assert.Equal(864, TimeConstants.HungerDecayInterval);
    }

    [Fact]
    public void ThirstSystem_ThirstLevels_AreOrdered()
    {
        // 渇きレベルは Hydrated < Thirsty < Dehydrated < SevereDehydration
        Assert.True(ThirstLevel.Hydrated < ThirstLevel.Thirsty);
        Assert.True(ThirstLevel.Thirsty < ThirstLevel.Dehydrated);
        Assert.True(ThirstLevel.Dehydrated < ThirstLevel.SevereDehydration);
    }

    #endregion

    #region Task 7: 疲労悪化速度

    [Fact]
    public void FatigueLevels_AreOrdered()
    {
        Assert.True(FatigueLevel.Fresh < FatigueLevel.Mild);
        Assert.True(FatigueLevel.Mild < FatigueLevel.Tired);
        Assert.True(FatigueLevel.Tired < FatigueLevel.Exhausted);
        Assert.True(FatigueLevel.Exhausted < FatigueLevel.Collapse);
    }

    [Fact]
    public void BodyConditionSystem_FatigueModifier_DecreasesWithLevel()
    {
        float fresh = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Fresh);
        float mild = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Mild);
        float tired = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Tired);
        float exhausted = BodyConditionSystem.GetFatigueModifier(FatigueLevel.Exhausted);

        Assert.True(fresh > mild);
        Assert.True(mild > tired);
        Assert.True(tired > exhausted);
    }

    #endregion

    #region Task 8: 宿屋で疲労・衛生回復

    [Fact]
    public void RestAtInn_RestoresHP_MP_SP()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Soldier);
        player.AddGold(1000);
        player.TakeDamage(Damage.Pure(10));

        var townSystem = new TownSystem();

        var result = townSystem.RestAtInn(player);
        Assert.True(result.Success);
        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void RestAtInn_FailsWithoutGold()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Soldier);
        // 初期ゴールドを使い切る
        player.SpendGold(player.Gold);

        var townSystem = new TownSystem();

        var result = townSystem.RestAtInn(player);
        Assert.False(result.Success);
    }

    [Fact]
    public void RestAtInn_ReturnsTurnCost()
    {
        var player = Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Soldier);
        player.AddGold(1000);

        var townSystem = new TownSystem();

        var result = townSystem.RestAtInn(player);
        Assert.True(result.Success);
        Assert.True(result.TurnCost > 0); // 8時間分のターンコスト
    }

    [Fact]
    public void HygieneLevel_Clean_HasLowInfectionRisk()
    {
        float risk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Clean);
        Assert.True(risk < 1.0f);
    }

    [Fact]
    public void HygieneLevel_Filthy_HasHighInfectionRisk()
    {
        float risk = BodyConditionSystem.GetHygieneInfectionRisk(HygieneLevel.Filthy);
        Assert.True(risk > 1.0f);
    }

    #endregion

    #region Task 2: 自動探索 - 下り階段へのBFS経路（マップ関連テスト）

    [Fact]
    public void DungeonMap_StairsDown_IsAccessible()
    {
        var map = new DungeonMap(30, 30);
        // フロアタイルで埋める
        for (int x = 1; x < 29; x++)
            for (int y = 1; y < 29; y++)
                map.SetTile(x, y, TileType.Floor);

        map.SetStairsDown(new Position(25, 25));
        var tile = map.GetTile(new Position(25, 25));
        Assert.Equal(TileType.StairsDown, tile.Type);
        Assert.False(tile.BlocksMovement);
    }

    [Fact]
    public void DungeonMap_ComputeFov_ExploresTiles()
    {
        var map = new DungeonMap(30, 30);
        for (int x = 1; x < 29; x++)
            for (int y = 1; y < 29; y++)
                map.SetTile(x, y, TileType.Floor);

        map.ComputeFov(new Position(15, 15), 8);

        // 中心付近のタイルはExploredになっている
        var tile = map.GetTile(new Position(15, 15));
        Assert.True(tile.IsExplored);
        Assert.True(tile.IsVisible);
    }

    #endregion
}
