using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// スキルシステムのテスト（Phase 5.4）
/// SkillDatabase, SkillSystem, ActiveSkill, SkillTreeNode, セーブ/ロード
/// </summary>
public class SkillSystemTests
{
    #region SkillDatabase Tests

    [Fact]
    public void SkillDatabase_HasSkills()
    {
        Assert.True(SkillDatabase.Count > 0);
    }

    [Theory]
    [InlineData("strong_strike", SkillCategory.Combat)]
    [InlineData("fireball", SkillCategory.Magic)]
    [InlineData("heal", SkillCategory.Magic)]
    [InlineData("sneak", SkillCategory.Support)]
    [InlineData("lockpick", SkillCategory.Exploration)]
    [InlineData("brew", SkillCategory.Crafting)]
    [InlineData("hp_boost", SkillCategory.Passive)]
    public void SkillDatabase_GetById_ReturnsCorrectCategory(string skillId, SkillCategory expected)
    {
        var skill = SkillDatabase.GetById(skillId);
        Assert.NotNull(skill);
        Assert.Equal(expected, skill.Category);
    }

    [Fact]
    public void SkillDatabase_GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(SkillDatabase.GetById("nonexistent_skill"));
    }

    [Theory]
    [InlineData(CharacterClass.Fighter)]
    [InlineData(CharacterClass.Knight)]
    [InlineData(CharacterClass.Thief)]
    [InlineData(CharacterClass.Ranger)]
    [InlineData(CharacterClass.Mage)]
    [InlineData(CharacterClass.Cleric)]
    [InlineData(CharacterClass.Monk)]
    [InlineData(CharacterClass.Bard)]
    [InlineData(CharacterClass.Alchemist)]
    [InlineData(CharacterClass.Necromancer)]
    public void SkillDatabase_AllClasses_HaveSkillTrees(CharacterClass cls)
    {
        var tree = SkillDatabase.GetSkillTree(cls);
        Assert.NotNull(tree);
        Assert.True(tree.Count > 0, $"{cls} should have a skill tree");
    }

    [Fact]
    public void SkillDatabase_GetByCategory_ReturnsCombatSkills()
    {
        var combatSkills = SkillDatabase.GetByCategory(SkillCategory.Combat).ToList();
        Assert.True(combatSkills.Count > 0);
        Assert.All(combatSkills, s => Assert.Equal(SkillCategory.Combat, s.Category));
    }

    [Fact]
    public void SkillDatabase_GetByClass_ReturnsFighterSkills()
    {
        var fighterSkills = SkillDatabase.GetByClass(CharacterClass.Fighter).ToList();
        Assert.True(fighterSkills.Count > 0);
        Assert.All(fighterSkills, s => Assert.Equal(CharacterClass.Fighter, s.ClassRequired));
    }

    [Fact]
    public void SkillDatabase_GetAll_ReturnsAllSkills()
    {
        var all = SkillDatabase.GetAll().ToList();
        Assert.Equal(SkillDatabase.Count, all.Count);
    }

    [Fact]
    public void SkillDatabase_StrongStrike_HasCorrectProperties()
    {
        var skill = SkillDatabase.GetById("strong_strike");
        Assert.NotNull(skill);
        Assert.Equal("強打", skill.Name);
        Assert.Equal(SkillTarget.SingleEnemy, skill.Target);
        Assert.Equal(0, skill.ManaCost);
        Assert.Equal(10, skill.SpCost);
        Assert.Equal(3, skill.Cooldown);
        Assert.Equal(1, skill.LevelRequired);
        Assert.Equal(CharacterClass.Fighter, skill.ClassRequired);
        Assert.Null(skill.PrerequisiteSkillId);
        Assert.Equal(1.5, skill.BasePower);
    }

    [Fact]
    public void SkillDatabase_Fireball_HasFireElement()
    {
        var skill = SkillDatabase.GetById("fireball");
        Assert.NotNull(skill);
        Assert.Equal(Element.Fire, skill.Element);
        Assert.Equal(SkillTarget.Area, skill.Target);
        Assert.Equal(15, skill.ManaCost);
    }

    [Fact]
    public void SkillDatabase_PassiveSkills_HaveNoClassRequirement()
    {
        var passiveSkillIds = new[] { "hp_boost", "mp_boost", "poison_resist", "critical_eye", "treasure_sense" };
        foreach (var id in passiveSkillIds)
        {
            var skill = SkillDatabase.GetById(id);
            Assert.NotNull(skill);
            Assert.Null(skill.ClassRequired);
            Assert.Equal(SkillCategory.Passive, skill.Category);
        }
    }

    #endregion

    #region SkillDefinition Tests

    [Fact]
    public void SkillDefinition_IsReady_WhenCooldownIsZero()
    {
        var skill = SkillDatabase.GetById("strong_strike")!;
        Assert.True(skill.IsReady(0));
    }

    [Fact]
    public void SkillDefinition_IsNotReady_WhenCooldownIsPositive()
    {
        var skill = SkillDatabase.GetById("strong_strike")!;
        Assert.False(skill.IsReady(3));
    }

    #endregion

    #region ActiveSkill Tests

    [Fact]
    public void ActiveSkill_InitialCooldown_IsZero()
    {
        var active = new ActiveSkill("strong_strike");
        Assert.Equal(0, active.CurrentCooldown);
        Assert.True(active.IsReady);
    }

    [Fact]
    public void ActiveSkill_Use_SetsCooldown()
    {
        var active = new ActiveSkill("strong_strike");
        active.Use(5);
        Assert.Equal(5, active.CurrentCooldown);
        Assert.False(active.IsReady);
    }

    [Fact]
    public void ActiveSkill_TickCooldown_DecreasesCooldown()
    {
        var active = new ActiveSkill("strong_strike");
        active.Use(3);
        active.TickCooldown();
        Assert.Equal(2, active.CurrentCooldown);
        active.TickCooldown();
        Assert.Equal(1, active.CurrentCooldown);
        active.TickCooldown();
        Assert.Equal(0, active.CurrentCooldown);
        Assert.True(active.IsReady);
    }

    [Fact]
    public void ActiveSkill_TickCooldown_DoesNotGoBelowZero()
    {
        var active = new ActiveSkill("strong_strike");
        active.TickCooldown();
        Assert.Equal(0, active.CurrentCooldown);
    }

    #endregion

    #region SkillSystem Tests

    [Fact]
    public void SkillSystem_RegisterSkill_CanUse()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        // strong_strike: ManaCost=0, SpCost=10
        Assert.True(system.CanUse("strong_strike", 0, 10));
    }

    [Fact]
    public void SkillSystem_CanUse_UnregisteredSkill_ReturnsFalse()
    {
        var system = new SkillSystem();
        Assert.False(system.CanUse("strong_strike", 100, 100));
    }

    [Fact]
    public void SkillSystem_CanUse_UnknownSkill_ReturnsFalse()
    {
        var system = new SkillSystem();
        Assert.False(system.CanUse("nonexistent", 100, 100));
    }

    [Fact]
    public void SkillSystem_CanUse_InsufficientMp_ReturnsFalse()
    {
        var system = new SkillSystem();
        system.RegisterSkill("fireball");
        // fireball: ManaCost=15, SpCost=0
        Assert.False(system.CanUse("fireball", 10, 100));
    }

    [Fact]
    public void SkillSystem_CanUse_InsufficientSp_ReturnsFalse()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        // strong_strike: ManaCost=0, SpCost=10
        Assert.False(system.CanUse("strong_strike", 100, 5));
    }

    [Fact]
    public void SkillSystem_CanUse_OnCooldown_ReturnsFalse()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.Use("strong_strike", 100, 100);
        // strong_strike has Cooldown=3
        Assert.False(system.CanUse("strong_strike", 100, 100));
    }

    [Fact]
    public void SkillSystem_Use_Success_ReturnsCosts()
    {
        var system = new SkillSystem();
        system.RegisterSkill("fireball");
        var result = system.Use("fireball", 100, 100);
        Assert.True(result.Success);
        Assert.Equal(15, result.MpCost);
        Assert.Equal(0, result.SpCost);
        Assert.Equal(3, result.TurnCost); // default TurnCost
    }

    [Fact]
    public void SkillSystem_Use_UnknownSkill_Fails()
    {
        var system = new SkillSystem();
        var result = system.Use("nonexistent", 100, 100);
        Assert.False(result.Success);
    }

    [Fact]
    public void SkillSystem_Use_UnregisteredSkill_Fails()
    {
        var system = new SkillSystem();
        var result = system.Use("strong_strike", 100, 100);
        Assert.False(result.Success);
    }

    [Fact]
    public void SkillSystem_Use_InsufficientMp_Fails()
    {
        var system = new SkillSystem();
        system.RegisterSkill("fireball");
        var result = system.Use("fireball", 5, 100);
        Assert.False(result.Success);
        Assert.Contains("MP", result.Message);
    }

    [Fact]
    public void SkillSystem_Use_InsufficientSp_Fails()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        var result = system.Use("strong_strike", 100, 3);
        Assert.False(result.Success);
        Assert.Contains("SP", result.Message);
    }

    [Fact]
    public void SkillSystem_Use_OnCooldown_Fails()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.Use("strong_strike", 100, 100);
        var result = system.Use("strong_strike", 100, 100);
        Assert.False(result.Success);
        Assert.Contains("クールダウン", result.Message);
    }

    [Fact]
    public void SkillSystem_TickCooldowns_ReducesAllCooldowns()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.RegisterSkill("fireball");
        system.Use("strong_strike", 100, 100); // Cooldown 3
        system.Use("fireball", 100, 100); // Cooldown 4

        system.TickCooldowns();
        Assert.Equal(2, system.GetCooldown("strong_strike"));
        Assert.Equal(3, system.GetCooldown("fireball"));

        system.TickCooldowns();
        system.TickCooldowns();
        Assert.Equal(0, system.GetCooldown("strong_strike"));
        Assert.Equal(1, system.GetCooldown("fireball"));

        // strong_strike should now be usable
        Assert.True(system.CanUse("strong_strike", 100, 100));
        Assert.False(system.CanUse("fireball", 100, 100));
    }

    [Fact]
    public void SkillSystem_GetCooldown_UnregisteredSkill_ReturnsZero()
    {
        var system = new SkillSystem();
        Assert.Equal(0, system.GetCooldown("strong_strike"));
    }

    [Fact]
    public void SkillSystem_RegisterSkill_DuplicateRegistration_DoesNotReset()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.Use("strong_strike", 100, 100);
        Assert.Equal(3, system.GetCooldown("strong_strike"));

        // 二重登録してもクールダウンがリセットされないことを確認
        system.RegisterSkill("strong_strike");
        Assert.Equal(3, system.GetCooldown("strong_strike"));
    }

    #endregion

    #region SkillTreeNode Tests

    [Fact]
    public void SkillTreeNode_CanLearn_Tier1_NoPrerequisites()
    {
        var node = new SkillTreeNode("strong_strike", 1, Array.Empty<string>());
        var learned = new HashSet<string>();
        Assert.True(node.CanLearn(learned, 1, CharacterClass.Fighter));
    }

    [Fact]
    public void SkillTreeNode_CanLearn_Tier2_WithPrerequisite()
    {
        var node = new SkillTreeNode("whirlwind", 2, new[] { "strong_strike" });
        var learned = new HashSet<string> { "strong_strike" };
        // whirlwind requires level 8, class Fighter
        Assert.True(node.CanLearn(learned, 8, CharacterClass.Fighter));
    }

    [Fact]
    public void SkillTreeNode_CanLearn_MissingPrerequisite_ReturnsFalse()
    {
        var node = new SkillTreeNode("whirlwind", 2, new[] { "strong_strike" });
        var learned = new HashSet<string>();
        Assert.False(node.CanLearn(learned, 10, CharacterClass.Fighter));
    }

    [Fact]
    public void SkillTreeNode_CanLearn_InsufficientLevel_ReturnsFalse()
    {
        var node = new SkillTreeNode("strong_strike", 1, Array.Empty<string>());
        var learned = new HashSet<string>();
        // strong_strike requires level 1, but verify higher req
        // fireball requires level 5
        var fireballNode = new SkillTreeNode("fireball", 2, new[] { "basic_magic" });
        var learnedMage = new HashSet<string> { "basic_magic" };
        Assert.False(fireballNode.CanLearn(learnedMage, 3, CharacterClass.Mage));
        Assert.True(fireballNode.CanLearn(learnedMage, 5, CharacterClass.Mage));
    }

    [Fact]
    public void SkillTreeNode_CanLearn_WrongClass_ReturnsFalse()
    {
        var node = new SkillTreeNode("strong_strike", 1, Array.Empty<string>());
        var learned = new HashSet<string>();
        // strong_strike requires Fighter class
        Assert.False(node.CanLearn(learned, 10, CharacterClass.Mage));
    }

    #endregion

    #region GetLearnableSkills Tests

    [Fact]
    public void SkillSystem_GetLearnableSkills_Fighter_InitiallyReturnsFirstTier()
    {
        var system = new SkillSystem();
        var learned = new HashSet<string>();
        var learnable = system.GetLearnableSkills(CharacterClass.Fighter, learned, 1).ToList();

        // Fighter Tier1: strong_strike, weapon_mastery (weapon_mastery is Passive but level 1)
        Assert.Contains(learnable, n => n.SkillId == "strong_strike");
        Assert.Contains(learnable, n => n.SkillId == "weapon_mastery");
    }

    [Fact]
    public void SkillSystem_GetLearnableSkills_Fighter_AfterLearningTier1()
    {
        var system = new SkillSystem();
        var learned = new HashSet<string> { "strong_strike", "weapon_mastery" };
        var learnable = system.GetLearnableSkills(CharacterClass.Fighter, learned, 10).ToList();

        // whirlwind (needs strong_strike, level 8), hp_boost (no prereq, level 5), critical_eye (needs weapon_mastery, level 10)
        Assert.Contains(learnable, n => n.SkillId == "whirlwind");
        Assert.Contains(learnable, n => n.SkillId == "hp_boost");
        Assert.Contains(learnable, n => n.SkillId == "critical_eye");
    }

    [Fact]
    public void SkillSystem_GetLearnableSkills_ExcludesAlreadyLearned()
    {
        var system = new SkillSystem();
        var learned = new HashSet<string> { "strong_strike" };
        var learnable = system.GetLearnableSkills(CharacterClass.Fighter, learned, 1).ToList();
        Assert.DoesNotContain(learnable, n => n.SkillId == "strong_strike");
    }

    #endregion

    #region Save/Load (GetCooldownState / RestoreCooldownState) Tests

    [Fact]
    public void SkillSystem_GetCooldownState_NoActiveCooldowns_ReturnsEmpty()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        var state = system.GetCooldownState();
        Assert.Empty(state);
    }

    [Fact]
    public void SkillSystem_GetCooldownState_WithCooldowns_ReturnsOnlyActive()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.RegisterSkill("fireball");
        system.Use("strong_strike", 100, 100); // Cooldown 3
        // fireball not used, cooldown 0

        var state = system.GetCooldownState();
        Assert.Single(state);
        Assert.Equal(3, state["strong_strike"]);
    }

    [Fact]
    public void SkillSystem_RestoreCooldownState_RestoresCooldowns()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");
        system.RegisterSkill("fireball");

        var state = new Dictionary<string, int>
        {
            { "strong_strike", 2 },
            { "fireball", 4 }
        };
        system.RestoreCooldownState(state);

        Assert.Equal(2, system.GetCooldown("strong_strike"));
        Assert.Equal(4, system.GetCooldown("fireball"));
        Assert.False(system.CanUse("strong_strike", 100, 100));
        Assert.False(system.CanUse("fireball", 100, 100));
    }

    [Fact]
    public void SkillSystem_RestoreCooldownState_IgnoresUnregisteredSkills()
    {
        var system = new SkillSystem();
        system.RegisterSkill("strong_strike");

        var state = new Dictionary<string, int>
        {
            { "strong_strike", 2 },
            { "nonexistent_skill", 5 }
        };
        // 例外が発生しないことを確認
        system.RestoreCooldownState(state);
        Assert.Equal(2, system.GetCooldown("strong_strike"));
    }

    [Fact]
    public void SkillSystem_SaveLoadRoundtrip_PreservesCooldowns()
    {
        // セーブ側
        var saveSystem = new SkillSystem();
        saveSystem.RegisterSkill("strong_strike");
        saveSystem.RegisterSkill("fireball");
        saveSystem.RegisterSkill("heal");
        saveSystem.Use("strong_strike", 100, 100); // CD 3
        saveSystem.Use("fireball", 100, 100); // CD 4
        saveSystem.TickCooldowns(); // CD: strong=2, fireball=3
        var state = saveSystem.GetCooldownState();

        // ロード側
        var loadSystem = new SkillSystem();
        loadSystem.RegisterSkill("strong_strike");
        loadSystem.RegisterSkill("fireball");
        loadSystem.RegisterSkill("heal");
        loadSystem.RestoreCooldownState(state);

        Assert.Equal(2, loadSystem.GetCooldown("strong_strike"));
        Assert.Equal(3, loadSystem.GetCooldown("fireball"));
        Assert.Equal(0, loadSystem.GetCooldown("heal"));
        Assert.True(loadSystem.CanUse("heal", 100, 100));
    }

    #endregion

    #region SkillUseResult Tests

    [Fact]
    public void SkillUseResult_DefaultValues()
    {
        var result = new SkillUseResult(true, "テスト");
        Assert.True(result.Success);
        Assert.Equal("テスト", result.Message);
        Assert.Equal(0, result.MpCost);
        Assert.Equal(0, result.SpCost);
        Assert.Equal(0, result.TurnCost);
    }

    [Fact]
    public void SkillUseResult_WithCosts()
    {
        var result = new SkillUseResult(true, "テスト", MpCost: 10, SpCost: 5, TurnCost: 3);
        Assert.Equal(10, result.MpCost);
        Assert.Equal(5, result.SpCost);
        Assert.Equal(3, result.TurnCost);
    }

    #endregion

    #region SkillDatabase Skill Prerequisite Chain Tests

    [Fact]
    public void SkillDatabase_Thief_SkillTree_BackstabRequiresSneak()
    {
        var tree = SkillDatabase.GetSkillTree(CharacterClass.Thief);
        var backstabNode = tree.FirstOrDefault(n => n.SkillId == "backstab");
        Assert.NotNull(backstabNode);
        Assert.Contains("sneak", backstabNode.Prerequisites);
    }

    [Fact]
    public void SkillDatabase_Mage_SkillTree_FireballRequiresBasicMagic()
    {
        var tree = SkillDatabase.GetSkillTree(CharacterClass.Mage);
        var fireballNode = tree.FirstOrDefault(n => n.SkillId == "fireball");
        Assert.NotNull(fireballNode);
        Assert.Contains("basic_magic", fireballNode.Prerequisites);
    }

    [Fact]
    public void SkillDatabase_Necromancer_SkillTree_CurseRequiresLifeDrain()
    {
        var tree = SkillDatabase.GetSkillTree(CharacterClass.Necromancer);
        var curseNode = tree.FirstOrDefault(n => n.SkillId == "curse");
        Assert.NotNull(curseNode);
        Assert.Contains("life_drain", curseNode.Prerequisites);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SkillSystem_Use_ZeroCooldownSkill_CanBeUsedRepeatedly()
    {
        var system = new SkillSystem();
        system.RegisterSkill("lockpick"); // Cooldown=0
        var result1 = system.Use("lockpick", 100, 100);
        Assert.True(result1.Success);
        var result2 = system.Use("lockpick", 100, 100);
        Assert.True(result2.Success);
    }

    [Fact]
    public void SkillSystem_ComboStrike_HasCustomTurnCost()
    {
        var skill = SkillDatabase.GetById("combo_strike");
        Assert.NotNull(skill);
        Assert.Equal(5, skill.TurnCost);
    }

    [Fact]
    public void SkillSystem_Meditation_HasLongTurnCost()
    {
        var skill = SkillDatabase.GetById("meditation");
        Assert.NotNull(skill);
        Assert.Equal(10, skill.TurnCost);
    }

    [Fact]
    public void SkillDatabase_GetSkillTree_UnknownClass_ReturnsEmpty()
    {
        // CharacterClass enumの範囲外の値でテスト
        var tree = SkillDatabase.GetSkillTree((CharacterClass)999);
        Assert.Empty(tree);
    }

    #endregion
}
