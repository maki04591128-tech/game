using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// HarvestSystem テスト
/// - 剥ぎ取り可否判定
/// - 種族別素材ドロップ
/// - ランク別ドロップ率補正
/// - 素材リスト取得
/// </summary>
public class HarvestSystemTests
{
    #region Test Helpers

    private class TestRandomProvider : IRandomProvider
    {
        private readonly double _fixedDouble;
        private readonly int _fixedInt;

        public TestRandomProvider(double fixedDouble, int fixedInt = 0)
        {
            _fixedDouble = fixedDouble;
            _fixedInt = fixedInt;
        }

        public int Next(int maxValue) => Math.Min(_fixedInt, maxValue > 0 ? maxValue - 1 : 0);
        public int Next(int minValue, int maxValue) => Math.Clamp(_fixedInt, minValue, maxValue > minValue ? maxValue - 1 : minValue);
        public double NextDouble() => _fixedDouble;
    }

    #endregion

    #region CanHarvest Tests

    [Theory]
    [InlineData(MonsterRace.Beast, true)]
    [InlineData(MonsterRace.Undead, true)]
    [InlineData(MonsterRace.Dragon, true)]
    [InlineData(MonsterRace.Insect, true)]
    [InlineData(MonsterRace.Plant, true)]
    [InlineData(MonsterRace.Demon, true)]
    [InlineData(MonsterRace.Spirit, true)]
    [InlineData(MonsterRace.Construct, true)]
    [InlineData(MonsterRace.Amorphous, true)]
    [InlineData(MonsterRace.Humanoid, true)]
    public void CanHarvest_AllRaces_ReturnsExpected(MonsterRace race, bool expected)
    {
        Assert.Equal(expected, HarvestSystem.CanHarvest(race));
    }

    #endregion

    #region Harvest Tests

    [Fact]
    public void Harvest_Beast_CanDropHideAndFang()
    {
        // NextDouble = 0.0 → 全ドロップ成功
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Beast, EnemyRank.Common, random);

        Assert.NotEmpty(result.Materials);
        Assert.Contains(result.Materials, m => m.ItemId == "material_beast_hide");
        Assert.Contains(result.Materials, m => m.ItemId == "material_beast_fang");
        Assert.Contains("入手した", result.Message);
    }

    [Fact]
    public void Harvest_Undead_CanDropBoneAndEssence()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Undead, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_bone_fragment");
        Assert.Contains(result.Materials, m => m.ItemId == "material_cursed_essence");
    }

    [Fact]
    public void Harvest_Dragon_CanDropScaleAndFang()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Dragon, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_dragon_scale");
        Assert.Contains(result.Materials, m => m.ItemId == "material_dragon_fang");
    }

    [Fact]
    public void Harvest_Insect_CanDropShellAndVenom()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Insect, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_insect_shell");
        Assert.Contains(result.Materials, m => m.ItemId == "material_venom_sac");
    }

    [Fact]
    public void Harvest_Plant_CanDropHerbAndWood()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Plant, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_herb");
        Assert.Contains(result.Materials, m => m.ItemId == "material_wood");
    }

    [Fact]
    public void Harvest_Demon_CanDropHornAndCrystal()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Demon, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_demon_horn");
        Assert.Contains(result.Materials, m => m.ItemId == "material_dark_crystal");
    }

    [Fact]
    public void Harvest_Spirit_CanDropEssenceAndCore()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Spirit, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_spirit_essence");
        Assert.Contains(result.Materials, m => m.ItemId == "material_elemental_core");
    }

    [Fact]
    public void Harvest_Construct_CanDropCoreAndIron()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Construct, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_golem_core");
        Assert.Contains(result.Materials, m => m.ItemId == "material_iron_fragment");
    }

    [Fact]
    public void Harvest_Amorphous_CanDropGelAndCrystal()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Amorphous, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_slime_gel");
        Assert.Contains(result.Materials, m => m.ItemId == "material_magic_crystal");
    }

    [Fact]
    public void Harvest_Humanoid_CanDropEquipmentFragment()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Humanoid, EnemyRank.Common, random);

        Assert.Contains(result.Materials, m => m.ItemId == "material_equipment_fragment");
    }

    [Fact]
    public void Harvest_HighRollFails_ReturnsEmptyWithMessage()
    {
        // NextDouble = 0.99 → ほとんどのドロップが失敗
        var random = new TestRandomProvider(0.99, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Dragon, EnemyRank.Common, random);

        Assert.Empty(result.Materials);
        Assert.Contains("剥ぎ取れる素材はなかった", result.Message);
    }

    #endregion

    #region Rank Scaling Tests

    [Fact]
    public void Harvest_HigherRank_IncreasesDropRate()
    {
        // 中間的なドロップ率でテスト（0.4 = BossのrankMultiplier 2.0で成功する可能性が高い）
        int commonSuccesses = 0;
        int bossSuccesses = 0;

        for (int i = 0; i < 100; i++)
        {
            double roll = i / 100.0;
            var random = new TestRandomProvider(roll, 0);

            var commonResult = HarvestSystem.Harvest(MonsterRace.Dragon, EnemyRank.Common, random);
            var bossResult = HarvestSystem.Harvest(MonsterRace.Dragon, EnemyRank.Boss, random);

            commonSuccesses += commonResult.Materials.Count;
            bossSuccesses += bossResult.Materials.Count;
        }

        Assert.True(bossSuccesses >= commonSuccesses,
            $"Boss ({bossSuccesses}) should drop >= Common ({commonSuccesses})");
    }

    [Fact]
    public void Harvest_BossRank_CanDropMultipleQuantity()
    {
        // NextDouble = 0.0（全ドロップ成功）、fixedInt = 3（数量ボーナス3）
        var random = new TestRandomProvider(0.0, 3);
        var result = HarvestSystem.Harvest(MonsterRace.Beast, EnemyRank.Boss, random);

        Assert.NotEmpty(result.Materials);
        // Boss rank has quantity bonus of 3, so max quantity = 1 + 3 = 4
        Assert.True(result.Materials.Any(m => m.Quantity > 1),
            "Boss should be able to drop multiple quantity");
    }

    [Fact]
    public void Harvest_CommonRank_DropsOneQuantity()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Beast, EnemyRank.Common, random);

        Assert.NotEmpty(result.Materials);
        Assert.All(result.Materials, m => Assert.Equal(1, m.Quantity));
    }

    #endregion

    #region GetHarvestableItems Tests

    [Theory]
    [InlineData(MonsterRace.Beast, new[] { "material_beast_hide", "material_beast_fang" })]
    [InlineData(MonsterRace.Dragon, new[] { "material_dragon_scale", "material_dragon_fang" })]
    [InlineData(MonsterRace.Humanoid, new[] { "material_equipment_fragment" })]
    public void GetHarvestableItems_ReturnsCorrectItems(MonsterRace race, string[] expectedItems)
    {
        var items = HarvestSystem.GetHarvestableItems(race);

        Assert.Equal(expectedItems.Length, items.Count);
        foreach (var expected in expectedItems)
        {
            Assert.Contains(expected, items);
        }
    }

    [Fact]
    public void GetHarvestableItems_AllRaces_ReturnNonEmpty()
    {
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            var items = HarvestSystem.GetHarvestableItems(race);
            Assert.True(items.Count > 0, $"{race} should have harvestable items");
        }
    }

    #endregion

    #region Message Tests

    [Fact]
    public void Harvest_Success_MessageContainsMaterials()
    {
        var random = new TestRandomProvider(0.0, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Beast, EnemyRank.Common, random);

        Assert.Contains("獣", result.Message);
        Assert.Contains("入手した", result.Message);
    }

    [Fact]
    public void Harvest_Failure_MessageIndicatesNothing()
    {
        var random = new TestRandomProvider(0.99, 0);
        var result = HarvestSystem.Harvest(MonsterRace.Dragon, EnemyRank.Common, random);

        Assert.Contains("剥ぎ取れる素材はなかった", result.Message);
    }

    #endregion
}
