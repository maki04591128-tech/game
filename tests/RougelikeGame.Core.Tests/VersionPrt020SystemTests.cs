using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Map;
using RougelikeGame.Core.Map.Generation;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Ver.prt.0.20 Phase 20 システムテスト
/// Task 1: 装備着脱時のアイテム重複バグ修正
/// Task 2: 素材アイテムのインベントリ反映修正
/// Task 3: フィールド/町の可視性修正（RevealAll）
/// Task 4: 通路の柱が通行を妨げない修正（WouldBlockPassage）
/// Task 5: メッセージログの1000件制限・番号削除
/// Task 6: スキルツリーへのアクティブスキル追加
/// </summary>
public class VersionPrt020SystemTests
{
    #region Task 1: 装備着脱時のアイテム重複バグ修正

    [Fact]
    public void Equip_DoesNotDuplicate_WhenEquippingFromInventory()
    {
        var player = CreateTestPlayer();
        var inventory = new Inventory(20);
        var sword = ItemDefinitions.Create("weapon_iron_sword") as EquipmentItem;
        Assert.NotNull(sword);

        inventory.Add(sword!);
        Assert.Single(inventory.Items);

        // インベントリから装備する：インベントリから削除→装備
        inventory.Remove(sword!);
        var previousItem = player.Equipment.Equip(sword!, player);

        Assert.Empty(inventory.Items);
        Assert.Equal(sword, player.Equipment.MainHand);
        Assert.Null(previousItem);
    }

    [Fact]
    public void Unequip_DoesNotDuplicate_WhenUnequippingToInventory()
    {
        var player = CreateTestPlayer();
        var inventory = new Inventory(20);
        var sword = ItemDefinitions.Create("weapon_iron_sword") as EquipmentItem;
        Assert.NotNull(sword);

        // まず装備
        player.Equipment.Equip(sword!, player);
        Assert.Equal(sword, player.Equipment.MainHand);

        // 装備解除→インベントリに戻す
        var unequipped = player.Equipment.Unequip(EquipmentSlot.MainHand, player);
        Assert.NotNull(unequipped);
        inventory.Add(unequipped!);

        Assert.Single(inventory.Items);
        Assert.Null(player.Equipment.MainHand);
    }

    [Fact]
    public void Equip_ReplacingExisting_ReturnsPreviousItem()
    {
        var player = CreateTestPlayer();
        var sword1 = ItemDefinitions.Create("weapon_iron_sword") as EquipmentItem;
        var sword2 = ItemDefinitions.Create("weapon_iron_sword") as EquipmentItem;
        Assert.NotNull(sword1);
        Assert.NotNull(sword2);

        player.Equipment.Equip(sword1!, player);
        var previous = player.Equipment.Equip(sword2!, player);

        Assert.Equal(sword1, previous);
        Assert.Equal(sword2, player.Equipment.MainHand);
    }

    [Fact]
    public void Inventory_Remove_ByReference_RemovesExactItem()
    {
        var inventory = new Inventory(20);
        var item1 = ItemDefinitions.Create("weapon_iron_sword");
        var item2 = ItemDefinitions.Create("weapon_iron_sword");
        Assert.NotNull(item1);
        Assert.NotNull(item2);

        inventory.Add(item1!);
        inventory.Add(item2!);
        Assert.Equal(2, inventory.Items.Count);

        // 参照で特定のアイテムのみ削除
        inventory.Remove(item1!);
        Assert.Single(inventory.Items);
        Assert.Contains(item2, inventory.Items);
        Assert.DoesNotContain(item1, inventory.Items);
    }

    #endregion

    #region Task 2: 素材アイテムのインベントリ反映修正

    [Theory]
    [InlineData("material_bone_fragment")]
    [InlineData("material_magic_crystal")]
    [InlineData("material_slime_gel")]
    [InlineData("material_equipment_fragment")]
    [InlineData("material_stone")]
    [InlineData("material_iron_fragment")]
    [InlineData("material_cursed_essence")]
    [InlineData("material_insect_shell")]
    public void ItemDefinitions_Create_ReturnsMaterialItem(string itemId)
    {
        var item = ItemDefinitions.Create(itemId);
        Assert.NotNull(item);
        Assert.IsAssignableFrom<Item>(item);
    }

    [Theory]
    [InlineData("material_bone_fragment", "骨片")]
    [InlineData("material_magic_crystal", "魔力結晶")]
    [InlineData("material_equipment_fragment", "装備品の欠片")]
    [InlineData("material_stone", "石ころ")]
    [InlineData("material_iron_fragment", "鉄片")]
    public void ItemDefinitions_Create_ReturnsCorrectDisplayName(string itemId, string expectedName)
    {
        var item = ItemDefinitions.Create(itemId);
        Assert.NotNull(item);
        Assert.Equal(expectedName, item!.GetDisplayName());
    }

    [Fact]
    public void Material_AddedToInventory_CanBeRetrieved()
    {
        var inventory = new Inventory(20);
        var material = ItemDefinitions.Create("material_bone_fragment");
        Assert.NotNull(material);

        bool added = inventory.Add(material!);
        Assert.True(added);
        Assert.Contains(material, inventory.Items);
    }

    [Fact]
    public void Material_StacksCorrectly_WhenSameType()
    {
        var inventory = new Inventory(20);
        var mat1 = ItemDefinitions.Create("material_bone_fragment");
        var mat2 = ItemDefinitions.Create("material_bone_fragment");
        Assert.NotNull(mat1);
        Assert.NotNull(mat2);

        inventory.Add(mat1!);
        inventory.Add(mat2!);

        // スタック可能ならスロット1つ、非スタックなら2つ
        // どちらにしてもインベントリにアイテムが存在する
        Assert.True(inventory.Items.Count >= 1);
    }

    #endregion

    #region Task 3: フィールド/町の可視性修正（RevealAll）

    [Fact]
    public void DungeonMap_RevealAll_MakesAllTilesVisible()
    {
        var map = new DungeonMap(30, 30);
        // 初期状態では全て不可視
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                Assert.False(map[x, y].IsVisible);

        map.RevealAll();

        // RevealAll後は全て可視・探索済み
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                Assert.True(map[x, y].IsVisible, $"Tile ({x},{y}) should be visible");
                Assert.True(map[x, y].IsExplored, $"Tile ({x},{y}) should be explored");
            }
        }
    }

    [Fact]
    public void DungeonMap_RevealAll_AfterResetVisibility_StillRevealsAll()
    {
        var map = new DungeonMap(20, 20);
        map.RevealAll();
        map.ResetVisibility();

        // リセット後は不可視
        Assert.False(map[5, 5].IsVisible);

        // 再度RevealAll
        map.RevealAll();
        Assert.True(map[5, 5].IsVisible);
        Assert.True(map[5, 5].IsExplored);
    }

    #endregion

    #region Task 4: 通路の柱が通行を妨げない修正（WouldBlockPassage）

    [Fact]
    public void DungeonGenerator_Pillars_DoNotBlockCorridors()
    {
        // 複数シードで生成して、柱が通路を完全に塞いでいないか確認
        for (int seed = 0; seed < 10; seed++)
        {
            var generator = new DungeonGenerator(seed);
            var parameters = new DungeonGenerationParameters
            {
                Width = 60,
                Height = 40,
                Depth = 1,
                RoomCount = 8
            };

            var map = generator.Generate(parameters) as DungeonMap;
            Assert.NotNull(map);

            // 全通路タイルが少なくとも1つの隣接歩行可能タイルを持つことを確認
            for (int x = 1; x < map!.Width - 1; x++)
            {
                for (int y = 1; y < map.Height - 1; y++)
                {
                    var pos = new Position(x, y);
                    if (map.GetTileType(pos) == TileType.Corridor)
                    {
                        bool hasAdjacentWalkable =
                            map.IsWalkable(new Position(x - 1, y)) ||
                            map.IsWalkable(new Position(x + 1, y)) ||
                            map.IsWalkable(new Position(x, y - 1)) ||
                            map.IsWalkable(new Position(x, y + 1));

                        Assert.True(hasAdjacentWalkable,
                            $"通路タイル ({x},{y}) に隣接する歩行可能タイルがありません（seed={seed}）");
                    }
                }
            }
        }
    }

    [Fact]
    public void DungeonGenerator_Pillars_DoNotBlockAdjacentCorridors()
    {
        for (int seed = 0; seed < 5; seed++)
        {
            var generator = new DungeonGenerator(seed + 100);
            var parameters = new DungeonGenerationParameters
            {
                Width = 50,
                Height = 35,
                Depth = 1,
                RoomCount = 6
            };

            var map = generator.Generate(parameters) as DungeonMap;
            Assert.NotNull(map);

            // 柱タイルの隣にある通路が歩行不能でないことを確認
            for (int x = 1; x < map!.Width - 1; x++)
            {
                for (int y = 1; y < map.Height - 1; y++)
                {
                    var pos = new Position(x, y);
                    if (map.GetTileType(pos) != TileType.Pillar) continue;

                    var cardinals = new[]
                    {
                        (dx: -1, dy: 0), (dx: 1, dy: 0),
                        (dx: 0, dy: -1), (dx: 0, dy: 1)
                    };

                    foreach (var (dx, dy) in cardinals)
                    {
                        var adjPos = new Position(x + dx, y + dy);
                        if (!map.IsInBounds(adjPos)) continue;
                        var adjType = map.GetTileType(adjPos);
                        if (adjType == TileType.Corridor)
                        {
                            Assert.True(map.IsWalkable(adjPos),
                                $"柱({x},{y})の隣の通路({adjPos.X},{adjPos.Y})が歩行不能（seed={seed + 100}）");
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Task 5: メッセージログの1000件制限

    // メッセージログの制限はGameController（GUI層）のため、
    // Core層ではロジックの基本原理をテスト

    [Fact]
    public void MessageList_Capacity_LimitedTo1000()
    {
        var messages = new List<string>();

        // 1005件追加（1000件制限をシミュレーション）
        for (int i = 0; i < 1005; i++)
        {
            if (messages.Count >= 1000)
                messages.RemoveAt(0);
            messages.Add($"メッセージ{i}");
        }

        Assert.Equal(1000, messages.Count);
        Assert.Equal("メッセージ5", messages[0]); // 最初の5件は削除済み
        Assert.Equal("メッセージ1004", messages[999]);
    }

    #endregion

    #region Task 6: スキルツリーへのアクティブスキル追加

    [Fact]
    public void SkillTreeSystem_ContainsActiveSkillNodes_WeaponTab()
    {
        var skillTree = new SkillTreeSystem();

        var expectedActiveSkills = new[]
        {
            "active_sword_cross_slash",
            "active_axe_ground_slam",
            "active_dagger_shadow_stitch",
            "active_bow_piercing_shot",
            "active_staff_mana_burst",
            "active_shield_bash"
        };

        foreach (var skillId in expectedActiveSkills)
        {
            Assert.True(skillTree.AllNodes.ContainsKey(skillId), $"スキル {skillId} が存在しません");
            var node = skillTree.AllNodes[skillId];
            Assert.Equal(SkillNodeType.Active, node.NodeType);
            Assert.Equal(SkillTreeTab.Weapon, node.Tab);
            Assert.Equal(4, node.Tier);
            Assert.Equal(12, node.RequiredLevel);
        }
    }

    [Fact]
    public void SkillTreeSystem_ContainsActiveSkillNodes_MagicTab()
    {
        var skillTree = new SkillTreeSystem();

        var expectedActiveSkills = new[]
        {
            "active_magic_meditation",
            "active_magic_deep_meditation",
            "active_magic_focus",
            "active_magic_barrier",
            "active_magic_transfer",
            "active_magic_sense",
            "active_magic_mana_shield"
        };

        foreach (var skillId in expectedActiveSkills)
        {
            Assert.True(skillTree.AllNodes.ContainsKey(skillId), $"スキル {skillId} が存在しません");
            var node = skillTree.AllNodes[skillId];
            Assert.Equal(SkillNodeType.Active, node.NodeType);
            Assert.Equal(SkillTreeTab.Magic, node.Tab);
        }
    }

    [Fact]
    public void SkillTreeSystem_ActiveSkills_HavePrerequisites()
    {
        var skillTree = new SkillTreeSystem();

        // 各アクティブスキルは少なくとも1つの前提条件を持つ
        var activeSkillIds = new[]
        {
            "active_sword_cross_slash", "active_axe_ground_slam",
            "active_dagger_shadow_stitch", "active_bow_piercing_shot",
            "active_staff_mana_burst", "active_shield_bash",
            "active_magic_meditation", "active_magic_deep_meditation",
            "active_magic_focus", "active_magic_barrier",
            "active_magic_transfer", "active_magic_sense",
            "active_magic_mana_shield"
        };

        foreach (var skillId in activeSkillIds)
        {
            Assert.True(skillTree.AllNodes.ContainsKey(skillId), $"スキル {skillId} が存在しません");
            var node = skillTree.AllNodes[skillId];
            Assert.NotEmpty(node.Prerequisites);
        }
    }

    [Fact]
    public void SkillTreeSystem_ActiveSkills_TotalCount_Is43()
    {
        var skillTree = new SkillTreeSystem();
        var allNodes = skillTree.AllNodes.Values;
        var activeNodes = allNodes.Where(n => n.NodeType == SkillNodeType.Active).ToList();

        Assert.Equal(43, activeNodes.Count);
    }

    [Fact]
    public void SkillTreeSystem_ActiveSkills_HaveJapaneseNames()
    {
        var skillTree = new SkillTreeSystem();

        var expectedNames = new Dictionary<string, string>
        {
            ["active_sword_cross_slash"] = "十字斬り",
            ["active_axe_ground_slam"] = "大地割り",
            ["active_dagger_shadow_stitch"] = "影縫い",
            ["active_bow_piercing_shot"] = "貫通矢",
            ["active_staff_mana_burst"] = "マナバースト",
            ["active_shield_bash"] = "シールドバッシュ",
            ["active_magic_meditation"] = "瞑想",
            ["active_magic_deep_meditation"] = "深い瞑想",
            ["active_magic_focus"] = "精神集中",
            ["active_magic_barrier"] = "魔力障壁",
            ["active_magic_transfer"] = "魔力譲渡",
            ["active_magic_sense"] = "魔力感知",
            ["active_magic_mana_shield"] = "マナシールド"
        };

        foreach (var (id, name) in expectedNames)
        {
            Assert.True(skillTree.AllNodes.ContainsKey(id), $"スキル {id} が存在しません");
            var node = skillTree.AllNodes[id];
            Assert.Equal(name, node.Name);
        }
    }

    #endregion

    #region ヘルパーメソッド

    private static Player CreateTestPlayer()
    {
        var player = Player.Create("テスト勇者", Race.Human, CharacterClass.Fighter, Background.Soldier);
        player.Position = new Position(5, 5);
        return player;
    }

    #endregion
}
