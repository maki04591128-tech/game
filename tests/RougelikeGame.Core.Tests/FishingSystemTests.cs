using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// FishingSystem（釣りシステム）のテスト
/// </summary>
public class FishingSystemTests
{
    // --- 全魚定義取得 ---

    [Fact]
    public void GetAllFish_ReturnsNonEmpty()
    {
        var fish = FishingSystem.GetAllFish();

        Assert.True(fish.Count > 0);
    }

    [Fact]
    public void GetAllFish_ContainsLegendaryFish()
    {
        var fish = FishingSystem.GetAllFish();

        Assert.Contains(fish, f => f.Id == "fish_legendary");
    }

    [Fact]
    public void GetAllFish_ContainsJunkAndTreasure()
    {
        var fish = FishingSystem.GetAllFish();

        Assert.Contains(fish, f => f.Id == "fish_junk");
        Assert.Contains(fish, f => f.Id == "fish_treasure");
    }

    // --- 条件付き魚リスト ---

    [Fact]
    public void GetAvailableFish_SpringMorning_ReturnsCommonFish()
    {
        var available = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 10);

        Assert.True(available.Count > 0);
        Assert.Contains(available, f => f.Id == "fish_common_1"); // フナは春の午前に釣れる
    }

    [Fact]
    public void GetAvailableFish_ExcludesJunkAndTreasure()
    {
        // ジャンクと宝箱はRarity=0なので除外される
        var available = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Morning, 10);

        Assert.DoesNotContain(available, f => f.Id == "fish_junk");
        Assert.DoesNotContain(available, f => f.Id == "fish_treasure");
    }

    [Fact]
    public void GetAvailableFish_LowLevel_ExcludesHighLevelFish()
    {
        // レベル1では高レベル要求の魚は釣れない
        var available = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Dawn, 1);

        Assert.DoesNotContain(available, f => f.Id == "fish_rare_1"); // 要求レベル6
    }

    [Fact]
    public void GetAvailableFish_WrongSeason_ExcludesFish()
    {
        // 幻の大魚は秋のみ
        var available = FishingSystem.GetAvailableFish(Season.Spring, TimePeriod.Dusk, 10);

        Assert.DoesNotContain(available, f => f.Id == "fish_legendary");
    }

    // --- 釣り成功率計算 ---

    [Fact]
    public void CalculateCatchRate_HighLevel_HigherRate()
    {
        float highLevel = FishingSystem.CalculateCatchRate(1, 10, 0);
        float lowLevel = FishingSystem.CalculateCatchRate(1, 1, 0);

        Assert.True(highLevel > lowLevel);
    }

    [Fact]
    public void CalculateCatchRate_ClampedToMax()
    {
        // 非常に有利な条件でも0.95を超えない
        float rate = FishingSystem.CalculateCatchRate(1, 100, 10);

        Assert.Equal(0.95f, rate, 0.001f);
    }

    [Fact]
    public void CalculateCatchRate_ClampedToMin()
    {
        // 非常に不利な条件でも0.05を下回らない
        float rate = FishingSystem.CalculateCatchRate(10, 0, -10);

        Assert.Equal(0.05f, rate, 0.001f);
    }

    // --- ジャンク率計算 ---

    [Fact]
    public void CalculateJunkRate_HighLevel_LowerRate()
    {
        float highLevel = FishingSystem.CalculateJunkRate(10);
        float lowLevel = FishingSystem.CalculateJunkRate(1);

        Assert.True(highLevel < lowLevel);
    }

    [Fact]
    public void CalculateJunkRate_NeverBelowMinimum()
    {
        float rate = FishingSystem.CalculateJunkRate(100);

        Assert.True(rate >= 0.05f);
    }

    // --- 宝箱率計算 ---

    [Fact]
    public void CalculateTreasureRate_HighLevel_HigherRate()
    {
        float highLevel = FishingSystem.CalculateTreasureRate(10, 0);
        float lowLevel = FishingSystem.CalculateTreasureRate(1, 0);

        Assert.True(highLevel > lowLevel);
    }

    [Fact]
    public void CalculateTreasureRate_ClampedToMax()
    {
        float rate = FishingSystem.CalculateTreasureRate(100, 10);

        Assert.True(rate <= 0.1f);
    }

    [Fact]
    public void CalculateTreasureRate_ClampedToMin()
    {
        float rate = FishingSystem.CalculateTreasureRate(0, -10);

        Assert.True(rate >= 0.01f);
    }

    // --- ID検索 ---

    [Fact]
    public void GetFishById_ExistingId_ReturnsFish()
    {
        var fish = FishingSystem.GetFishById("fish_common_1");

        Assert.NotNull(fish);
        Assert.Equal("フナ", fish!.Name);
    }

    [Fact]
    public void GetFishById_InvalidId_ReturnsNull()
    {
        Assert.Null(FishingSystem.GetFishById("nonexistent_fish"));
    }
}
