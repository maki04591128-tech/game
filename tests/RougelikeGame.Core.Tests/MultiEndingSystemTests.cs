using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// マルチエンディングシステム 専用テスト
/// テスト数: 16件
/// </summary>
public class MultiEndingSystemTests
{
    // ============================================================
    // DetermineEnding - 境界値テスト
    // ============================================================

    [Fact]
    public void DetermineEnding_DarkEnding_ExactlyMinus50Karma()
    {
        // カルマちょうど-50で闇エンディングが発動する
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 3, karmaValue: -50,
            allTerritoriesVisited: false, clearRank: "B");
        Assert.Equal(EndingType.Dark, result.Type);
        Assert.Equal("闇の支配者", result.Title);
        Assert.Contains("深淵の王", result.Description);
    }

    [Fact]
    public void DetermineEnding_NotDarkEnding_KarmaMinus49()
    {
        // カルマ-49では闇エンディングにならない
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: -49,
            allTerritoriesVisited: false, clearRank: "B");
        Assert.NotEqual(EndingType.Dark, result.Type);
    }

    [Fact]
    public void DetermineEnding_SalvationEnding_ExactlyKarma50()
    {
        // カルマちょうど50・死亡0回で救済エンディング
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: 50,
            allTerritoriesVisited: false, clearRank: "C");
        Assert.Equal(EndingType.Salvation, result.Type);
        Assert.Equal("聖者の凱旋", result.Title);
    }

    [Fact]
    public void DetermineEnding_NotSalvation_Karma49()
    {
        // カルマ49では救済エンディングにならない
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: 49,
            allTerritoriesVisited: false, clearRank: "C");
        Assert.NotEqual(EndingType.Salvation, result.Type);
    }

    [Fact]
    public void DetermineEnding_NotSalvation_OneDeathHighKarma()
    {
        // 死亡1回では救済エンディングにならない
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 1, karmaValue: 100,
            allTerritoriesVisited: false, clearRank: "C");
        Assert.NotEqual(EndingType.Salvation, result.Type);
    }

    [Fact]
    public void DetermineEnding_TrueEnding_RankA()
    {
        // ランクAでも真エンディングになる
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 5, karmaValue: 10,
            allTerritoriesVisited: false, clearRank: "A");
        Assert.Equal(EndingType.True, result.Type);
        Assert.Equal("真の英雄", result.Title);
    }

    [Fact]
    public void DetermineEnding_NotTrueEnding_RankB()
    {
        // ランクBでは真エンディングにならない
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 5, karmaValue: 10,
            allTerritoriesVisited: false, clearRank: "B");
        Assert.NotEqual(EndingType.True, result.Type);
    }

    // ============================================================
    // DetermineEnding - 優先度テスト
    // ============================================================

    [Fact]
    public void DetermineEnding_DarkTakesPriority_OverSalvation()
    {
        // DG-1: 真エンディング（S/Aランク）が最優先
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: -100,
            allTerritoriesVisited: true, clearRank: "S");
        Assert.Equal(EndingType.True, result.Type);
    }

    [Fact]
    public void DetermineEnding_SalvationTakesPriority_OverTrue()
    {
        // DG-1: 真エンディング（S/Aランク）が最優先
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 0, karmaValue: 80,
            allTerritoriesVisited: true, clearRank: "S");
        Assert.Equal(EndingType.True, result.Type);
    }

    [Fact]
    public void DetermineEnding_WandererEnding_DescriptionContainsTravel()
    {
        // 放浪エンディングの説明文に旅に関する内容が含まれる
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: false, totalDeaths: 5, karmaValue: 20,
            allTerritoriesVisited: true, clearRank: "");
        Assert.Equal(EndingType.Wanderer, result.Type);
        Assert.Contains("旅", result.Description);
    }

    [Fact]
    public void DetermineEnding_NormalEnding_Description()
    {
        // 正規エンディングの説明文検証
        var result = MultiEndingSystem.DetermineEnding(
            hasClearedFinalBoss: true, totalDeaths: 10, karmaValue: 0,
            allTerritoriesVisited: false, clearRank: "D");
        Assert.Equal(EndingType.Normal, result.Type);
        Assert.Equal("冒険の終わり", result.Title);
        Assert.Contains("平穏", result.Description);
    }

    // ============================================================
    // GetEndingTypeName テスト
    // ============================================================

    [Fact]
    public void GetEndingTypeName_InvalidEnumValue_ReturnsUnknown()
    {
        // 未定義のEnum値は「不明」を返す
        var name = MultiEndingSystem.GetEndingTypeName((EndingType)999);
        Assert.Equal("不明", name);
    }

    // ============================================================
    // GetEndingConditions テスト
    // ============================================================

    [Fact]
    public void GetEndingConditions_ContainsAllEndingTypes()
    {
        // 全エンディング種別の条件が含まれる
        var conditions = MultiEndingSystem.GetEndingConditions();
        var types = conditions.Select(c => c.Type).ToList();
        Assert.Contains(EndingType.Normal, types);
        Assert.Contains(EndingType.True, types);
        Assert.Contains(EndingType.Dark, types);
        Assert.Contains(EndingType.Salvation, types);
        Assert.Contains(EndingType.Wanderer, types);
    }

    [Fact]
    public void GetEndingConditions_AllHaveNonEmptyCondition()
    {
        // 全条件の説明文が空でない
        var conditions = MultiEndingSystem.GetEndingConditions();
        Assert.All(conditions, c => Assert.False(string.IsNullOrEmpty(c.Condition)));
    }

    // ============================================================
    // EndingResult レコードテスト
    // ============================================================

    [Fact]
    public void EndingResult_RecordEquality()
    {
        // EndingResultレコードの等値比較
        var a = new MultiEndingSystem.EndingResult(EndingType.Normal, "テスト", "説明");
        var b = new MultiEndingSystem.EndingResult(EndingType.Normal, "テスト", "説明");
        Assert.Equal(a, b);
    }

    [Fact]
    public void EndingResult_Properties()
    {
        // EndingResultレコードのプロパティアクセス
        var result = new MultiEndingSystem.EndingResult(EndingType.Dark, "闇", "闇の説明");
        Assert.Equal(EndingType.Dark, result.Type);
        Assert.Equal("闇", result.Title);
        Assert.Equal("闇の説明", result.Description);
    }
}
