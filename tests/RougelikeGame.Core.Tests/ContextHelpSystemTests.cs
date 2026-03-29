using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// ContextHelpSystem（コンテキストヘルプ・チュートリアルシステム）のテスト
/// </summary>
public class ContextHelpSystemTests
{
    // --- ヘルプトピック登録・取得 ---

    [Fact]
    public void RegisterTopic_SingleTopic_AppearsInTopics()
    {
        // Arrange
        var system = new ContextHelpSystem();

        // Act
        system.RegisterTopic("test1", HelpCategory.Combat, "戦闘テスト", "説明文");

        // Assert
        Assert.Single(system.Topics);
        Assert.True(system.Topics.ContainsKey("test1"));
    }

    [Fact]
    public void RegisterTopic_DuplicateId_OverwritesPrevious()
    {
        // Arrange
        var system = new ContextHelpSystem();
        system.RegisterTopic("dup", HelpCategory.Combat, "旧タイトル", "旧内容");

        // Act
        system.RegisterTopic("dup", HelpCategory.Magic, "新タイトル", "新内容");

        // Assert
        Assert.Single(system.Topics);
        Assert.Equal("新タイトル", system.Topics["dup"].Title);
        Assert.Equal(HelpCategory.Magic, system.Topics["dup"].Category);
    }

    [Fact]
    public void GetTopicsByCategory_ReturnsFilteredAndSorted()
    {
        // Arrange
        var system = new ContextHelpSystem();
        system.RegisterTopic("a", HelpCategory.Combat, "A", "内容A", priority: 1);
        system.RegisterTopic("b", HelpCategory.Combat, "B", "内容B", priority: 10);
        system.RegisterTopic("c", HelpCategory.Movement, "C", "内容C", priority: 5);

        // Act
        var combatTopics = system.GetTopicsByCategory(HelpCategory.Combat);

        // Assert
        Assert.Equal(2, combatTopics.Count);
        Assert.Equal("B", combatTopics[0].Title); // 優先度高い順
    }

    [Fact]
    public void GetTopicsByCategory_NoMatch_ReturnsEmpty()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("x", HelpCategory.Combat, "X", "内容");

        var result = system.GetTopicsByCategory(HelpCategory.Crafting);

        Assert.Empty(result);
    }

    [Fact]
    public void GetHelpForKey_ExistingKey_ReturnsTopic()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("inv", HelpCategory.Inventory, "インベントリ", "説明", keyBind: "I");

        var result = system.GetHelpForKey("I");

        Assert.NotNull(result);
        Assert.Equal("inv", result!.TopicId);
    }

    [Fact]
    public void GetHelpForKey_NoMatch_ReturnsNull()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("inv", HelpCategory.Inventory, "インベントリ", "説明", keyBind: "I");

        Assert.Null(system.GetHelpForKey("Z"));
    }

    // --- チュートリアル管理 ---

    [Fact]
    public void AddTutorialStep_AssignsIncrementalStepNumbers()
    {
        var system = new ContextHelpSystem();

        system.AddTutorialStep("S1", "命令1", "条件1");
        system.AddTutorialStep("S2", "命令2", "条件2");

        Assert.Equal(2, system.TutorialSteps.Count);
        Assert.Equal(1, system.TutorialSteps[0].StepNumber);
        Assert.Equal(2, system.TutorialSteps[1].StepNumber);
    }

    [Fact]
    public void GetCurrentTutorial_ReturnsFirstStep()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("最初", "指示", "条件");

        var current = system.GetCurrentTutorial();

        Assert.NotNull(current);
        Assert.Equal("最初", current!.Title);
        Assert.Equal(0, system.CurrentStep);
    }

    [Fact]
    public void GetCurrentTutorial_TutorialDisabled_ReturnsNull()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("S1", "指示", "条件");
        system.SetTutorialEnabled(false);

        Assert.Null(system.GetCurrentTutorial());
        Assert.False(system.TutorialEnabled);
    }

    [Fact]
    public void CompleteTutorialStep_AdvancesCurrentStep()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("S1", "指示1", "条件1");
        system.AddTutorialStep("S2", "指示2", "条件2");

        var completed = system.CompleteTutorialStep();

        Assert.NotNull(completed);
        Assert.True(completed!.IsCompleted);
        Assert.Equal(1, system.CurrentStep);
    }

    [Fact]
    public void CompleteTutorialStep_AllCompleted_ReturnsNull()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("S1", "指示", "条件");
        system.CompleteTutorialStep();

        Assert.Null(system.CompleteTutorialStep());
    }

    [Fact]
    public void TutorialProgress_HalfComplete_ReturnsHalf()
    {
        var system = new ContextHelpSystem();
        system.AddTutorialStep("S1", "指示1", "条件1");
        system.AddTutorialStep("S2", "指示2", "条件2");
        system.CompleteTutorialStep();

        Assert.Equal(0.5f, system.TutorialProgress, 0.01f);
    }

    [Fact]
    public void TutorialProgress_NoSteps_ReturnsZero()
    {
        var system = new ContextHelpSystem();
        Assert.Equal(0f, system.TutorialProgress);
    }

    // --- コンテキスト検索・カテゴリ名 ---

    [Fact]
    public void GetContextualHelp_MatchesTitleAndContent()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("a", HelpCategory.Combat, "攻撃の基本", "内容A");
        system.RegisterTopic("b", HelpCategory.Movement, "移動", "攻撃とは関係ない");

        // 「攻撃」はタイトルAと内容Bの両方にマッチ
        var results = system.GetContextualHelp("攻撃");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void GetContextualHelp_NoMatch_ReturnsEmpty()
    {
        var system = new ContextHelpSystem();
        system.RegisterTopic("a", HelpCategory.Combat, "戦闘", "内容");

        Assert.Empty(system.GetContextualHelp("存在しないキーワード"));
    }

    [Theory]
    [InlineData(HelpCategory.Movement, "移動")]
    [InlineData(HelpCategory.Combat, "戦闘")]
    [InlineData(HelpCategory.Inventory, "インベントリ")]
    [InlineData(HelpCategory.Advanced, "上級テクニック")]
    public void GetCategoryName_ReturnsJapanese(HelpCategory category, string expected)
    {
        Assert.Equal(expected, ContextHelpSystem.GetCategoryName(category));
    }

    [Fact]
    public void RegisterDefaultTopics_PopulatesTopics()
    {
        var system = new ContextHelpSystem();
        system.RegisterDefaultTopics();

        Assert.True(system.Topics.Count >= 7);
        Assert.True(system.Topics.ContainsKey("move_basic"));
        Assert.True(system.Topics.ContainsKey("survival_sanity"));
    }
}
