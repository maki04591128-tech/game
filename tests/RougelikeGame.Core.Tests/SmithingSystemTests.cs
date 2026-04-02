using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;
using RougelikeGame.Core.Entities;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 鍛冶システム 専用テスト
/// テスト数: 18件
/// </summary>
public class SmithingSystemTests
{
    private static Player CreateTestPlayer(int gold = 100000)
    {
        var player = Player.Create("テスト", Stats.Default);
        player.AddGold(gold);
        return player;
    }

    // ============================================================
    // Enhance テスト
    // ============================================================

    [Fact]
    public void Enhance_FromZero_ReturnsLevel1()
    {
        // +0から強化すると+1になる
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "鋼の剣", 0);
        Assert.True(result.Success);
        Assert.Equal(1, result.NewEnhanceLevel);
        Assert.Contains("+1", result.Message);
    }

    [Fact]
    public void Enhance_FromLevel9_ReturnsLevel10()
    {
        // +9から強化すると+10になる
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "鋼の剣", 9);
        Assert.True(result.Success);
        Assert.Equal(10, result.NewEnhanceLevel);
    }

    [Fact]
    public void Enhance_AtMaxLevel_Fails()
    {
        // 最大強化値では強化できない
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "鋼の剣", 10);
        Assert.False(result.Success);
        Assert.Contains("これ以上", result.Message);
    }

    [Fact]
    public void Enhance_CustomMaxEnhance_RespectsLimit()
    {
        // カスタム最大強化値が適用される
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enhance(player, "短剣", 5, maxEnhance: 5);
        Assert.False(result.Success);
    }

    [Fact]
    public void Enhance_InsufficientGold_Fails()
    {
        // ゴールド不足で強化失敗
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 0);
        var result = system.Enhance(player, "鋼の剣", 0);
        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    [Fact]
    public void Enhance_DeductsCorrectGold()
    {
        // 強化時に正しいゴールドが消費される
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 500);
        int expectedCost = SmithingSystem.CalculateEnhanceCost(0); // 100
        system.Enhance(player, "鋼の剣", 0);
        Assert.Equal(500 - expectedCost, player.Gold);
    }

    // ============================================================
    // Repair テスト
    // ============================================================

    [Fact]
    public void Repair_WithDamage_Succeeds()
    {
        // 耐久値減少がある場合に修理成功
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Repair(player, "鋼の盾", 20);
        Assert.True(result.Success);
        Assert.Contains("修理した", result.Message);
    }

    [Fact]
    public void Repair_NoDamage_Fails()
    {
        // 耐久値減少がない場合は修理不要
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Repair(player, "鋼の盾", 0);
        Assert.False(result.Success);
        Assert.Contains("修理の必要がない", result.Message);
    }

    [Fact]
    public void Repair_NegativeDurability_Fails()
    {
        // 負の耐久値減少でも修理不要
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Repair(player, "鋼の盾", -5);
        Assert.False(result.Success);
    }

    [Fact]
    public void Repair_InsufficientGold_Fails()
    {
        // 修理コストが足りない場合に失敗
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 10);
        var result = system.Repair(player, "鋼の盾", 100); // コスト: 100*5=500
        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    // ============================================================
    // Synthesize テスト
    // ============================================================

    [Fact]
    public void Synthesize_WithSufficientGold_Succeeds()
    {
        // ゴールドが十分な場合に合成成功
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Synthesize(player, new[] { "iron", "gem" }, "magic_sword", 1000);
        Assert.True(result.Success);
        Assert.Equal("magic_sword", result.ResultItemId);
        Assert.Contains("合成", result.Message);
    }

    [Fact]
    public void Synthesize_InsufficientGold_Fails()
    {
        // ゴールド不足で合成失敗
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 100);
        var result = system.Synthesize(player, new[] { "iron" }, "sword", 500);
        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    // ============================================================
    // Enchant テスト
    // ============================================================

    [Theory]
    [InlineData(Element.Fire, "炎")]
    [InlineData(Element.Ice, "氷")]
    [InlineData(Element.Lightning, "雷")]
    [InlineData(Element.Holy, "聖")]
    [InlineData(Element.Dark, "闇")]
    public void Enchant_AllElements_ShowsCorrectName(Element element, string expectedName)
    {
        // 各属性のエンチャントが正しい名前で表示される
        var system = new SmithingSystem();
        var player = CreateTestPlayer();
        var result = system.Enchant(player, "鋼の剣", element, 200);
        Assert.True(result.Success);
        Assert.Contains(expectedName, result.Message);
    }

    [Fact]
    public void Enchant_InsufficientGold_Fails()
    {
        // ゴールド不足でエンチャント失敗
        var system = new SmithingSystem();
        var player = CreateTestPlayer(gold: 0);
        var result = system.Enchant(player, "鋼の剣", Element.Fire, 200);
        Assert.False(result.Success);
        Assert.Contains("ゴールドが足りない", result.Message);
    }

    // ============================================================
    // CalculateEnhanceCost テスト
    // ============================================================

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 400)]
    [InlineData(2, 900)]
    [InlineData(3, 1600)]
    [InlineData(9, 10000)]
    public void CalculateEnhanceCost_ReturnsCorrectValue(int currentLevel, int expected)
    {
        // 強化コストが 100*(n+1)^2 の式に従う
        Assert.Equal(expected, SmithingSystem.CalculateEnhanceCost(currentLevel));
    }

    [Fact]
    public void CalculateEnhanceCost_NoOverflowAtHighLevel()
    {
        // 非常に高い強化レベルでもオーバーフローしない
        int cost = SmithingSystem.CalculateEnhanceCost(50000);
        Assert.True(cost > 0, $"Cost {cost} should be positive (no overflow)");
    }

    // ============================================================
    // SmithingResult レコードテスト
    // ============================================================

    [Fact]
    public void SmithingResult_DefaultValues()
    {
        // SmithingResultのデフォルト値が正しい
        var result = new SmithingResult(true, "テスト");
        Assert.Equal(0, result.NewEnhanceLevel);
        Assert.Null(result.ResultItemId);
    }

    [Fact]
    public void SmithingResult_RecordEquality()
    {
        // SmithingResultレコードの等値比較
        var a = new SmithingResult(true, "成功", 5, "sword_1");
        var b = new SmithingResult(true, "成功", 5, "sword_1");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Repair_LargeDurabilityLost_NoIntegerOverflow()
    {
        // int.MaxValue / 5 を超える値でもオーバーフローしない
        var system = new SmithingSystem();
        var player = CreateTestPlayer(0); // ゴールド不足で失敗するがクラッシュしない
        var result = system.Repair(player, "テスト武器", int.MaxValue);
        Assert.False(result.Success); // ゴールド不足で失敗するが例外は発生しない
    }
}
