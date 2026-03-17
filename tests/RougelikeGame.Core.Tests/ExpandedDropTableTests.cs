using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.AI;
using RougelikeGame.Core.Factories;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 拡張ドロップテーブル テスト
/// - DropTableEntryのMinGradeパラメータ
/// - 種族別ボーナスドロップ
/// - GenerateLoot種族オーバーロード
/// </summary>
public class ExpandedDropTableTests
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

    #region DropTableEntry Tests

    [Fact]
    public void DropTableEntry_DefaultMinGrade_IsStandard()
    {
        var entry = new DropTableEntry("test_item", 0.5);
        Assert.Equal(ItemGrade.Standard, entry.MinGrade);
    }

    [Fact]
    public void DropTableEntry_CustomMinGrade_IsPreserved()
    {
        var entry = new DropTableEntry("test_item", 0.5, MinGrade: ItemGrade.Fine);
        Assert.Equal(ItemGrade.Fine, entry.MinGrade);
    }

    [Fact]
    public void DropTableEntry_BackwardCompatibility_DefaultValues()
    {
        var entry = new DropTableEntry("test_item", 0.5);
        Assert.Equal(1, entry.MinQuantity);
        Assert.Equal(1, entry.MaxQuantity);
        Assert.Equal(ItemGrade.Standard, entry.MinGrade);
    }

    [Fact]
    public void DropTableEntry_AllParameters()
    {
        var entry = new DropTableEntry("test_item", 0.5, 2, 5, ItemGrade.Superior);
        Assert.Equal("test_item", entry.ItemId);
        Assert.Equal(0.5, entry.DropRate);
        Assert.Equal(2, entry.MinQuantity);
        Assert.Equal(5, entry.MaxQuantity);
        Assert.Equal(ItemGrade.Superior, entry.MinGrade);
    }

    #endregion

    #region GetRaceBonusDrops Tests

    [Theory]
    [InlineData(MonsterRace.Beast, "material_beast_hide", "material_beast_fang")]
    [InlineData(MonsterRace.Undead, "material_bone_fragment", "material_cursed_essence")]
    [InlineData(MonsterRace.Dragon, "material_dragon_scale", "material_dragon_fang")]
    [InlineData(MonsterRace.Insect, "material_insect_shell", "material_venom_sac")]
    [InlineData(MonsterRace.Plant, "material_herb", "material_wood")]
    [InlineData(MonsterRace.Demon, "material_demon_horn", "material_dark_crystal")]
    [InlineData(MonsterRace.Spirit, "material_spirit_essence", "material_elemental_core")]
    [InlineData(MonsterRace.Construct, "material_golem_core", "material_iron_fragment")]
    [InlineData(MonsterRace.Amorphous, "material_slime_gel", "material_magic_crystal")]
    public void GetRaceBonusDrops_ReturnsCorrectItems(MonsterRace race, string expectedItem1, string expectedItem2)
    {
        var drops = DropTableSystem.GetRaceBonusDrops(race);

        Assert.True(drops.Count >= 2);
        Assert.Contains(drops, d => d.ItemId == expectedItem1);
        Assert.Contains(drops, d => d.ItemId == expectedItem2);
    }

    [Fact]
    public void GetRaceBonusDrops_Humanoid_ReturnsEmpty()
    {
        var drops = DropTableSystem.GetRaceBonusDrops(MonsterRace.Humanoid);
        Assert.Empty(drops);
    }

    [Fact]
    public void GetRaceBonusDrops_AllDropRatesAreValid()
    {
        foreach (MonsterRace race in Enum.GetValues<MonsterRace>())
        {
            var drops = DropTableSystem.GetRaceBonusDrops(race);
            foreach (var entry in drops)
            {
                Assert.True(entry.DropRate > 0 && entry.DropRate <= 1.0,
                    $"Invalid drop rate {entry.DropRate} for {race}/{entry.ItemId}");
            }
        }
    }

    #endregion

    #region GenerateLoot with Race Tests

    [Fact]
    public void GenerateLoot_WithoutRace_WorksAsOriginal()
    {
        var random = new TestRandomProvider(0.01, 5);
        var result = DropTableSystem.GenerateLoot("drop_slime", 1, AI.EnemyRank.Common, random, race: null);

        Assert.NotNull(result);
        Assert.True(result.Gold >= 0);
    }

    [Fact]
    public void GenerateLoot_WithRace_IncludesBaseAndBonusDrops()
    {
        // NextDouble()が0.0を返すので全ドロップ成功する
        var random = new TestRandomProvider(0.0, 5);
        var resultWithRace = DropTableSystem.GenerateLoot("drop_slime", 1, AI.EnemyRank.Common, random, MonsterRace.Beast);
        var resultWithoutRace = DropTableSystem.GenerateLoot("drop_slime", 1, AI.EnemyRank.Common, random);

        // 種族ボーナスがある分、アイテムが増える可能性がある
        Assert.True(resultWithRace.Items.Count >= resultWithoutRace.Items.Count);
    }

    [Fact]
    public void GenerateLoot_InvalidTableId_ReturnsEmptyResult()
    {
        var random = new TestRandomProvider(0.0);
        var result = DropTableSystem.GenerateLoot("nonexistent_table", 1, AI.EnemyRank.Common, random, MonsterRace.Beast);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Gold);
    }

    [Fact]
    public void GenerateLoot_OriginalSignature_StillWorks()
    {
        var random = new TestRandomProvider(0.5, 5);
        var result = DropTableSystem.GenerateLoot("drop_goblin", 1, AI.EnemyRank.Common, random);

        Assert.NotNull(result);
    }

    #endregion

    #region Drop Rate Validation Tests

    [Fact]
    public void BeastBonusDrops_HaveCorrectRates()
    {
        var drops = DropTableSystem.GetRaceBonusDrops(MonsterRace.Beast);
        var hide = drops.First(d => d.ItemId == "material_beast_hide");
        var fang = drops.First(d => d.ItemId == "material_beast_fang");

        Assert.Equal(0.3, hide.DropRate);
        Assert.Equal(0.2, fang.DropRate);
    }

    [Fact]
    public void DragonBonusDrops_HaveCorrectRates()
    {
        var drops = DropTableSystem.GetRaceBonusDrops(MonsterRace.Dragon);
        var scale = drops.First(d => d.ItemId == "material_dragon_scale");
        var fang = drops.First(d => d.ItemId == "material_dragon_fang");

        Assert.Equal(0.15, scale.DropRate);
        Assert.Equal(0.1, fang.DropRate);
    }

    [Fact]
    public void SpiritBonusDrops_HaveCorrectRates()
    {
        var drops = DropTableSystem.GetRaceBonusDrops(MonsterRace.Spirit);
        var essence = drops.First(d => d.ItemId == "material_spirit_essence");
        var core = drops.First(d => d.ItemId == "material_elemental_core");

        Assert.Equal(0.2, essence.DropRate);
        Assert.Equal(0.08, core.DropRate);
    }

    #endregion
}
