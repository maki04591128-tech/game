using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// P.8 スキルツリーシステムのテスト
/// </summary>
public class SkillTreeSystemTests
{
    [Fact]
    public void AddPoints_IncreasesAvailablePoints()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);
        Assert.Equal(5, tree.AvailablePoints);
    }

    [Fact]
    public void UnlockNode_ReducesPointsAndAddsNode()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(5);
        Assert.True(tree.CanUnlock("shared_hp_1"));
        Assert.True(tree.UnlockNode("shared_hp_1"));
        Assert.Equal(4, tree.AvailablePoints);
        Assert.Equal(1, tree.UnlockedCount);
    }

    [Fact]
    public void CanUnlock_FailsWithoutPoints()
    {
        var tree = new SkillTreeSystem();
        Assert.False(tree.CanUnlock("shared_hp_1"));
    }

    [Fact]
    public void CanUnlock_FailsWithoutPrerequisite()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        Assert.False(tree.CanUnlock("shared_hp_2")); // Requires shared_hp_1
    }

    [Fact]
    public void CanUnlock_SucceedsWithPrerequisite()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        tree.UnlockNode("shared_hp_1");
        Assert.True(tree.CanUnlock("shared_hp_2"));
    }

    [Fact]
    public void UnlockNode_CannotUnlockTwice()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        tree.UnlockNode("shared_hp_1");
        Assert.False(tree.CanUnlock("shared_hp_1"));
    }

    [Fact]
    public void Respec_RefundsAllPoints()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        tree.UnlockNode("shared_hp_1");
        tree.UnlockNode("shared_hp_2");
        int refunded = tree.Respec();
        Assert.Equal(3, refunded); // 1 + 2
        Assert.Equal(10, tree.AvailablePoints);
        Assert.Equal(0, tree.UnlockedCount);
    }

    [Fact]
    public void GetTotalStatBonuses_SumsCorrectly()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        tree.UnlockNode("shared_hp_1");
        tree.UnlockNode("shared_hp_2");
        var bonuses = tree.GetTotalStatBonuses();
        Assert.Equal(30, bonuses["MaxHp"]); // 10 + 20
    }

    [Fact]
    public void GetKeystones_ReturnsKeystoneNodes()
    {
        var tree = new SkillTreeSystem();
        var keystones = tree.GetKeystones();
        Assert.True(keystones.Count >= 3);
        Assert.All(keystones, k => Assert.Equal(SkillNodeType.Keystone, k.NodeType));
    }

    [Fact]
    public void UnlockKeystone_RecordsDownside()
    {
        var tree = new SkillTreeSystem();
        tree.AddPoints(10);
        tree.UnlockNode("shared_str_1");
        tree.UnlockNode("keystone_glass_cannon");
        var downsides = tree.GetActiveKeystoneDownsides();
        Assert.Single(downsides);
        Assert.Contains("被ダメージ+100%", downsides);
    }

    [Fact]
    public void GetNodesForClass_FiltersCorrectly()
    {
        var tree = new SkillTreeSystem();
        var fighterNodes = tree.GetNodesForClass(CharacterClass.Fighter);
        var mageNodes = tree.GetNodesForClass(CharacterClass.Mage);
        Assert.True(fighterNodes.Count > 0);
        Assert.True(mageNodes.Count > 0);
        // Both should include shared nodes
        Assert.Contains(fighterNodes, n => n.Id == "shared_hp_1");
        Assert.Contains(mageNodes, n => n.Id == "shared_hp_1");
        // Class-specific should not overlap
        Assert.Contains(fighterNodes, n => n.Id == "fighter_heavy_blow");
        Assert.DoesNotContain(mageNodes, n => n.Id == "fighter_heavy_blow");
    }

    [Fact]
    public void RegisterNode_AddsCustomNode()
    {
        var tree = new SkillTreeSystem();
        tree.RegisterNode(new SkillNodeDefinition(
            "custom_test", "テスト", "テストノード",
            SkillNodeType.Passive, null, 1, Array.Empty<string>(),
            new() { ["TestStat"] = 5 }));
        Assert.True(tree.AllNodes.ContainsKey("custom_test"));
    }
}
