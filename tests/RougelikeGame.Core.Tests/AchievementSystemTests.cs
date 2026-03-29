using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// AchievementSystem（実績システム）のテスト
/// </summary>
public class AchievementSystemTests
{
    // --- コンストラクタ ---

    [Fact]
    public void Constructor_EmptyAchievements()
    {
        var system = new AchievementSystem();
        Assert.Equal(0, system.TotalCount);
        Assert.Equal(0, system.UnlockedCount);
    }

    // --- Register ---

    [Fact]
    public void Register_AddsAchievement()
    {
        var system = new AchievementSystem();
        system.Register("test_1", "テスト実績", "説明", AchievementCategory.Combat);
        Assert.Equal(1, system.TotalCount);
    }

    [Fact]
    public void Register_WithBonusEffect_StoresEffect()
    {
        var system = new AchievementSystem();
        system.Register("test_1", "テスト", "説明", AchievementCategory.Combat, "stat_boost_small");
        Assert.False(system.IsUnlocked("test_1"));
    }

    [Fact]
    public void Register_SameId_OverwritesExisting()
    {
        var system = new AchievementSystem();
        system.Register("dup", "名前1", "説明1", AchievementCategory.Combat);
        system.Register("dup", "名前2", "説明2", AchievementCategory.Exploration);
        Assert.Equal(1, system.TotalCount);
    }

    // --- Unlock ---

    [Fact]
    public void Unlock_ValidId_ReturnsNewUnlock()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        var result = system.Unlock("a1", 10);
        Assert.True(result.IsNewUnlock);
        Assert.Equal("a1", result.AchievementId);
    }

    [Fact]
    public void Unlock_AlreadyUnlocked_ReturnsNotNew()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        system.Unlock("a1", 5);
        var result = system.Unlock("a1", 10);
        Assert.False(result.IsNewUnlock);
    }

    [Fact]
    public void Unlock_NonExistentId_ReturnsNotNew()
    {
        var system = new AchievementSystem();
        var result = system.Unlock("nonexistent", 1);
        Assert.False(result.IsNewUnlock);
    }

    // --- IsUnlocked ---

    [Fact]
    public void IsUnlocked_NotUnlocked_ReturnsFalse()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        Assert.False(system.IsUnlocked("a1"));
    }

    [Fact]
    public void IsUnlocked_AfterUnlock_ReturnsTrue()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        system.Unlock("a1");
        Assert.True(system.IsUnlocked("a1"));
    }

    [Fact]
    public void IsUnlocked_NonExistentId_ReturnsFalse()
    {
        var system = new AchievementSystem();
        Assert.False(system.IsUnlocked("missing"));
    }

    // --- GetByCategory ---

    [Fact]
    public void GetByCategory_ReturnsMatchingOnly()
    {
        var system = new AchievementSystem();
        system.Register("c1", "戦闘1", "説明", AchievementCategory.Combat);
        system.Register("e1", "探索1", "説明", AchievementCategory.Exploration);
        system.Register("c2", "戦闘2", "説明", AchievementCategory.Combat);
        var combatList = system.GetByCategory(AchievementCategory.Combat);
        Assert.Equal(2, combatList.Count);
    }

    [Fact]
    public void GetByCategory_NoMatch_ReturnsEmpty()
    {
        var system = new AchievementSystem();
        system.Register("c1", "戦闘1", "説明", AchievementCategory.Combat);
        var result = system.GetByCategory(AchievementCategory.Story);
        Assert.Empty(result);
    }

    // --- GetUnlocked ---

    [Fact]
    public void GetUnlocked_NoneUnlocked_ReturnsEmpty()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        Assert.Empty(system.GetUnlocked());
    }

    [Fact]
    public void GetUnlocked_SomeUnlocked_ReturnsOnlyUnlocked()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        system.Register("a2", "実績B", "説明", AchievementCategory.Combat);
        system.Unlock("a1");
        Assert.Single(system.GetUnlocked());
    }

    // --- CompletionRate ---

    [Fact]
    public void CompletionRate_NoAchievements_ReturnsZero()
    {
        var system = new AchievementSystem();
        Assert.Equal(0f, system.CompletionRate);
    }

    [Fact]
    public void CompletionRate_HalfUnlocked_Returns05()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        system.Register("a2", "実績B", "説明", AchievementCategory.Combat);
        system.Unlock("a1");
        Assert.Equal(0.5f, system.CompletionRate);
    }

    // --- CalculateNextPlayBonus ---

    [Fact]
    public void CalculateNextPlayBonus_NoBonusEffect_ReturnsZero()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        system.Unlock("a1");
        Assert.Equal(0, system.CalculateNextPlayBonus());
    }

    [Fact]
    public void CalculateNextPlayBonus_WithBonusEffects_CalculatesCorrectly()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat, "stat_boost_small");
        system.Register("a2", "実績B", "説明", AchievementCategory.Combat, "gold_bonus");
        system.Unlock("a1");
        system.Unlock("a2");
        // stat_boost_small=1, gold_bonus=50 → 合計51
        Assert.Equal(51, system.CalculateNextPlayBonus());
    }

    [Fact]
    public void CalculateNextPlayBonus_UnlockedOnly_IgnoresLocked()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat, "stat_boost_large");
        system.Register("a2", "実績B", "説明", AchievementCategory.Combat, "gold_bonus");
        system.Unlock("a1");
        // stat_boost_large=5のみ（a2は未解除）
        Assert.Equal(5, system.CalculateNextPlayBonus());
    }

    // --- GetCategoryCompletionRate ---

    [Fact]
    public void GetCategoryCompletionRate_EmptyCategory_ReturnsZero()
    {
        var system = new AchievementSystem();
        Assert.Equal(0f, system.GetCategoryCompletionRate(AchievementCategory.Meta));
    }

    [Fact]
    public void GetCategoryCompletionRate_AllUnlocked_ReturnsOne()
    {
        var system = new AchievementSystem();
        system.Register("c1", "戦闘1", "説明", AchievementCategory.Combat);
        system.Register("c2", "戦闘2", "説明", AchievementCategory.Combat);
        system.Unlock("c1");
        system.Unlock("c2");
        Assert.Equal(1.0f, system.GetCategoryCompletionRate(AchievementCategory.Combat));
    }

    // --- UnlockedCount / TotalCount ---

    [Fact]
    public void UnlockedCount_IncreasesAfterUnlock()
    {
        var system = new AchievementSystem();
        system.Register("a1", "実績A", "説明", AchievementCategory.Combat);
        Assert.Equal(0, system.UnlockedCount);
        system.Unlock("a1");
        Assert.Equal(1, system.UnlockedCount);
    }

    // --- GetCategoryName ---

    [Theory]
    [InlineData(AchievementCategory.Combat, "戦闘")]
    [InlineData(AchievementCategory.Exploration, "探索")]
    [InlineData(AchievementCategory.Collection, "収集")]
    [InlineData(AchievementCategory.Story, "ストーリー")]
    [InlineData(AchievementCategory.Challenge, "チャレンジ")]
    [InlineData(AchievementCategory.Meta, "メタ")]
    public void GetCategoryName_ReturnsJapaneseName(AchievementCategory category, string expected)
    {
        Assert.Equal(expected, AchievementSystem.GetCategoryName(category));
    }
}
