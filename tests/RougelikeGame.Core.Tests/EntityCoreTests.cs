using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Entity系クラス（Player, Enemy, Character, Inventory, StatusEffect）の包括テスト
/// </summary>
public class EntityCoreTests
{
    #region Helper

    /// <summary>テスト用の具象Itemクラス</summary>
    private class TestItem : Item
    {
        protected override char GetDefaultDisplayChar() => '?';
    }

    /// <summary>テスト用のスタック可能アイテム</summary>
    private class TestStackableItem : StackableItem
    {
        protected override char GetDefaultDisplayChar() => '#';
    }

    private static Player CreateDefaultPlayer(string name = "テスト勇者")
        => Player.Create(name, Stats.Default);

    private static Enemy CreateDefaultEnemy(string name = "スライム", int expReward = 10)
        => Enemy.Create(name, "slime_01", Stats.Default, expReward);

    #endregion

    // ================================================================
    // Player テスト
    // ================================================================

    #region Player - 生成と基本プロパティ

    [Fact]
    public void Player_CreateWithStats_SetsNameAndLevel()
    {
        var player = CreateDefaultPlayer("英雄");

        Assert.Equal("英雄", player.Name);
        Assert.Equal(1, player.Level);
        Assert.Equal(0, player.Experience);
        Assert.Equal(Faction.Player, player.Faction);
    }

    [Fact]
    public void Player_CreateWithStats_InitializesResourcesToMax()
    {
        var player = CreateDefaultPlayer();

        Assert.Equal(player.MaxHp, player.CurrentHp);
        Assert.Equal(player.MaxMp, player.CurrentMp);
        Assert.Equal(player.MaxSp, player.CurrentSp);
        Assert.True(player.IsAlive);
    }

    [Fact]
    public void Player_CreateWithRaceClassBackground_SetsProperties()
    {
        var player = Player.Create("エルフ魔術師", Race.Elf, CharacterClass.Mage, Background.Scholar);

        Assert.Equal(Race.Elf, player.Race);
        Assert.Equal(CharacterClass.Mage, player.CharacterClass);
        Assert.Equal(Background.Scholar, player.Background);
        Assert.Equal("エルフ魔術師", player.Name);
        Assert.Equal(1, player.Level);
    }

    [Fact]
    public void Player_Create_InitializesSanityAndHunger()
    {
        var player = CreateDefaultPlayer();

        Assert.Equal(GameConstants.InitialSanity, player.Sanity);
        Assert.Equal(GameConstants.InitialHunger, player.Hunger);
    }

    #endregion

    #region Player - 経験値とレベルアップ

    [Fact]
    public void Player_GainExperience_IncreasesExperience()
    {
        var player = CreateDefaultPlayer();
        int expBefore = player.Experience;

        player.GainExperience(50);

        Assert.True(player.Experience > expBefore);
    }

    [Fact]
    public void Player_GainExperience_TriggersLevelUp()
    {
        var player = CreateDefaultPlayer();
        bool levelUpFired = false;
        player.OnLevelUp += (_, _) => levelUpFired = true;

        // 大量経験値でレベルアップを確実に発火
        player.GainExperience(10000);

        Assert.True(player.Level > 1);
        Assert.True(levelUpFired);
    }

    [Fact]
    public void Player_ExperienceToNextLevel_IsPositive()
    {
        var player = CreateDefaultPlayer();

        Assert.True(player.ExperienceToNextLevel > 0);
    }

    #endregion

    #region Player - 正気度と満腹度

    [Theory]
    [InlineData(100, SanityStage.Normal)]
    [InlineData(80, SanityStage.Normal)]
    [InlineData(79, SanityStage.Uneasy)]
    [InlineData(60, SanityStage.Uneasy)]
    [InlineData(59, SanityStage.Anxious)]
    [InlineData(40, SanityStage.Anxious)]
    [InlineData(39, SanityStage.Unstable)]
    [InlineData(20, SanityStage.Unstable)]
    [InlineData(19, SanityStage.Madness)]
    [InlineData(1, SanityStage.Madness)]
    [InlineData(0, SanityStage.Broken)]
    public void Player_SanityStage_ReturnsCorrectStage(int sanityValue, SanityStage expected)
    {
        var player = CreateDefaultPlayer();
        // ModifySanity は相対値なので、初期100から目標値まで引く
        player.ModifySanity(sanityValue - GameConstants.InitialSanity);

        Assert.Equal(expected, player.SanityStage);
    }

    [Theory]
    [InlineData(150, HungerStage.Nausea)]
    [InlineData(120, HungerStage.Nausea)]
    [InlineData(119, HungerStage.Overeating)]
    [InlineData(100, HungerStage.Overeating)]
    [InlineData(99, HungerStage.Full)]
    [InlineData(80, HungerStage.Full)]
    [InlineData(79, HungerStage.Normal)]
    [InlineData(50, HungerStage.Normal)]
    [InlineData(49, HungerStage.SlightlyHungry)]
    [InlineData(40, HungerStage.SlightlyHungry)]
    [InlineData(39, HungerStage.VeryHungry)]
    [InlineData(0, HungerStage.VeryHungry)]
    [InlineData(-1, HungerStage.Starving)]
    [InlineData(-8, HungerStage.Starving)]
    [InlineData(-9, HungerStage.NearStarvation)]
    [InlineData(-10, HungerStage.Starvation)]
    public void Player_HungerStage_ReturnsCorrectStage(int hungerValue, HungerStage expected)
    {
        var player = CreateDefaultPlayer();
        player.ModifyHunger(hungerValue - GameConstants.InitialHunger);

        Assert.Equal(expected, player.HungerStage);
    }

    [Fact]
    public void Player_ModifySanity_ClampsBetweenZeroAndMax()
    {
        var player = CreateDefaultPlayer();

        player.ModifySanity(-9999);
        Assert.Equal(0, player.Sanity);

        player.ModifySanity(9999);
        Assert.Equal(GameConstants.MaxSanity, player.Sanity);
    }

    #endregion

    #region Player - ゴールド

    [Fact]
    public void Player_AddGold_IncreasesGold()
    {
        var player = CreateDefaultPlayer();
        player.AddGold(100);

        Assert.Equal(100, player.Gold);
    }

    [Fact]
    public void Player_SpendGold_DecreasesGold()
    {
        var player = CreateDefaultPlayer();
        player.AddGold(200);

        bool result = player.SpendGold(50);

        Assert.True(result);
        Assert.Equal(150, player.Gold);
    }

    [Fact]
    public void Player_SpendGold_FailsWhenInsufficient()
    {
        var player = CreateDefaultPlayer();
        player.AddGold(10);

        bool result = player.SpendGold(100);

        Assert.False(result);
        Assert.Equal(10, player.Gold);
    }

    #endregion

    #region Player - スキルと魔法語

    [Fact]
    public void Player_LearnSkill_AddsAndDetects()
    {
        var player = CreateDefaultPlayer();

        player.LearnSkill("fireball");

        Assert.True(player.HasSkill("fireball"));
        Assert.False(player.HasSkill("icebolt"));
    }

    [Fact]
    public void Player_LearnWord_AddsWordWithInitialMastery()
    {
        var player = CreateDefaultPlayer();

        player.LearnWord("rune_fire");

        Assert.True(player.LearnedWords.ContainsKey("rune_fire"));
        Assert.Equal(GameConstants.InitialWordMastery, player.GetWordMastery("rune_fire"));
    }

    [Fact]
    public void Player_ImproveWordMastery_IncreasesMastery()
    {
        var player = CreateDefaultPlayer();
        player.LearnWord("rune_ice");
        int initial = player.GetWordMastery("rune_ice");

        player.ImproveWordMastery("rune_ice", 5);

        Assert.Equal(initial + 5, player.GetWordMastery("rune_ice"));
    }

    #endregion

    // ================================================================
    // Enemy テスト
    // ================================================================

    #region Enemy - 生成と基本プロパティ

    [Fact]
    public void Enemy_Create_SetsPropertiesCorrectly()
    {
        var enemy = CreateDefaultEnemy("ゴブリン", 25);

        Assert.Equal("ゴブリン", enemy.Name);
        Assert.Equal("slime_01", enemy.EnemyTypeId);
        Assert.Equal(25, enemy.ExperienceReward);
        Assert.Equal(Faction.Enemy, enemy.Faction);
        Assert.True(enemy.IsAlive);
    }

    [Fact]
    public void Enemy_Create_InitializesResourcesToMax()
    {
        var enemy = CreateDefaultEnemy();

        Assert.Equal(enemy.MaxHp, enemy.CurrentHp);
        Assert.Equal(enemy.MaxMp, enemy.CurrentMp);
    }

    [Fact]
    public void Enemy_DefaultAIState_IsIdle()
    {
        var enemy = CreateDefaultEnemy();

        Assert.Equal(AIState.Idle, enemy.CurrentAIState);
    }

    #endregion

    #region Enemy - 逃走と追跡

    [Fact]
    public void Enemy_ShouldFlee_TrueWhenHpBelowThreshold()
    {
        var enemy = CreateDefaultEnemy();
        // HP をFleeThreshold以下に設定（デフォルト0.2 → MaxHpの20%以下）
        int damageAmount = enemy.MaxHp - (int)(enemy.MaxHp * enemy.FleeThreshold) + 1;
        enemy.TakeDamage(Damage.Pure(damageAmount));

        Assert.True(enemy.ShouldFlee());
    }

    [Fact]
    public void Enemy_ShouldFlee_FalseWhenHpAboveThreshold()
    {
        var enemy = CreateDefaultEnemy();
        // HP 満タンでは逃げない
        Assert.False(enemy.ShouldFlee());
    }

    [Fact]
    public void Enemy_ShouldGiveUpChase_TrueWhenNoTarget()
    {
        var enemy = CreateDefaultEnemy();
        enemy.Target = null;

        Assert.True(enemy.ShouldGiveUpChase());
    }

    #endregion

    // ================================================================
    // Character テスト（Player/Enemy を通じてテスト）
    // ================================================================

    #region Character - ダメージとHP

    [Fact]
    public void Character_TakeDamage_ReducesHp()
    {
        var player = CreateDefaultPlayer();
        int hpBefore = player.CurrentHp;

        player.TakeDamage(Damage.Pure(10));

        Assert.True(player.CurrentHp < hpBefore);
    }

    [Fact]
    public void Character_TakeDamage_PureDamage_IgnoresDefense()
    {
        var player = CreateDefaultPlayer();
        int hpBefore = player.CurrentHp;

        player.TakeDamage(Damage.Pure(50));

        Assert.Equal(hpBefore - 50, player.CurrentHp);
    }

    [Fact]
    public void Character_TakeDamage_FiresOnDamagedEvent()
    {
        var player = CreateDefaultPlayer();
        bool eventFired = false;
        player.OnDamaged += (_, _) => eventFired = true;

        player.TakeDamage(Damage.Pure(5));

        Assert.True(eventFired);
    }

    [Fact]
    public void Character_TakeDamage_LethalDamage_FiresOnDeath()
    {
        var player = CreateDefaultPlayer();
        bool deathFired = false;
        player.OnDeath += (_, _) => deathFired = true;

        player.TakeDamage(Damage.Pure(player.MaxHp + 100));

        Assert.False(player.IsAlive);
        Assert.True(deathFired);
    }

    [Fact]
    public void Character_Heal_RestoresHp()
    {
        var player = CreateDefaultPlayer();
        player.TakeDamage(Damage.Pure(30));
        int hpAfterDamage = player.CurrentHp;

        player.Heal(20);

        Assert.Equal(hpAfterDamage + 20, player.CurrentHp);
    }

    [Fact]
    public void Character_Heal_DoesNotExceedMaxHp()
    {
        var player = CreateDefaultPlayer();
        player.TakeDamage(Damage.Pure(10));

        player.Heal(9999);

        Assert.Equal(player.MaxHp, player.CurrentHp);
    }

    [Fact]
    public void Character_HpClampedAtZero()
    {
        var player = CreateDefaultPlayer();

        player.TakeDamage(Damage.Pure(player.MaxHp + 999));

        Assert.Equal(0, player.CurrentHp);
        Assert.False(player.IsAlive);
    }

    #endregion

    #region Character - 状態異常

    [Fact]
    public void Character_ApplyStatusEffect_AddsEffect()
    {
        var player = CreateDefaultPlayer();
        var poison = new StatusEffect(StatusEffectType.Poison, 5);

        player.ApplyStatusEffect(poison);

        Assert.True(player.HasStatusEffect(StatusEffectType.Poison));
        Assert.Single(player.StatusEffects);
    }

    [Fact]
    public void Character_RemoveStatusEffect_RemovesEffect()
    {
        var player = CreateDefaultPlayer();
        player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, 5));

        player.RemoveStatusEffect(StatusEffectType.Poison);

        Assert.False(player.HasStatusEffect(StatusEffectType.Poison));
        Assert.Empty(player.StatusEffects);
    }

    [Fact]
    public void Character_ApplyStatusEffect_HigherPriority_ReplacesExisting()
    {
        var player = CreateDefaultPlayer();
        var weak = new StatusEffect(StatusEffectType.Slow, 3) { Priority = 1 };
        var strong = new StatusEffect(StatusEffectType.Slow, 5) { Priority = 2 };

        player.ApplyStatusEffect(weak);
        player.ApplyStatusEffect(strong);

        Assert.Single(player.StatusEffects);
        Assert.Equal(2, player.StatusEffects[0].Priority);
    }

    [Fact]
    public void Character_TickStatusEffects_ExpiredEffectsRemoved()
    {
        var enemy = CreateDefaultEnemy();
        enemy.ApplyStatusEffect(new StatusEffect(StatusEffectType.Haste, 1));

        Assert.True(enemy.HasStatusEffect(StatusEffectType.Haste));

        enemy.TickStatusEffects();

        Assert.False(enemy.HasStatusEffect(StatusEffectType.Haste));
    }

    [Fact]
    public void Character_CalculateEffectiveStats_IncludesStatModifiers()
    {
        var player = CreateDefaultPlayer();
        var buff = new StatusEffect(StatusEffectType.Strength, 5)
        {
            StatModifier = new StatModifier(Strength: 5)
        };

        int baseStat = player.BaseStats.Strength;
        player.ApplyStatusEffect(buff);

        Assert.Equal(baseStat + 5, player.EffectiveStats.Strength);
    }

    [Fact]
    public void Character_GetStatusEffectTurnModifier_DefaultIsOne()
    {
        var player = CreateDefaultPlayer();

        Assert.Equal(1.0f, player.GetStatusEffectTurnModifier());
    }

    [Fact]
    public void Character_GetStatusEffectTurnModifier_NeverBelowMinimum()
    {
        var player = CreateDefaultPlayer();
        // ターンコスト修正0のステータス効果を追加
        var effect = new StatusEffect(StatusEffectType.Poison, 5) { TurnCostModifier = 0f };
        player.ApplyStatusEffect(effect);
        float modifier = player.GetStatusEffectTurnModifier();
        Assert.True(modifier >= 0.1f, $"Modifier {modifier} should be >= 0.1");
    }

    #endregion

    // ================================================================
    // Inventory テスト
    // ================================================================

    #region Inventory - 追加と削除

    [Fact]
    public void Inventory_Add_IncreasesUsedSlots()
    {
        var inv = new Inventory(10);
        var item = new TestItem { ItemId = "sword_01", Name = "剣", Weight = 2.0f };

        bool added = inv.Add(item);

        Assert.True(added);
        Assert.Equal(1, inv.UsedSlots);
        Assert.True(inv.Contains(item));
    }

    [Fact]
    public void Inventory_Remove_DecreasesUsedSlots()
    {
        var inv = new Inventory(10);
        var item = new TestItem { ItemId = "shield_01", Name = "盾", Weight = 3.0f };
        inv.Add(item);

        bool removed = inv.Remove(item);

        Assert.True(removed);
        Assert.Equal(0, inv.UsedSlots);
    }

    [Fact]
    public void Inventory_Add_FailsWhenFull()
    {
        var inv = new Inventory(2);
        inv.Add(new TestItem { ItemId = "a", Name = "A", Weight = 1f });
        inv.Add(new TestItem { ItemId = "b", Name = "B", Weight = 1f });

        bool result = inv.Add(new TestItem { ItemId = "c", Name = "C", Weight = 1f });

        Assert.False(result);
        Assert.Equal(2, inv.UsedSlots);
    }

    [Fact]
    public void Inventory_FreeSlots_ReturnsCorrectValue()
    {
        var inv = new Inventory(5);
        inv.Add(new TestItem { ItemId = "x", Name = "X", Weight = 1f });
        inv.Add(new TestItem { ItemId = "y", Name = "Y", Weight = 1f });

        Assert.Equal(3, inv.FreeSlots);
    }

    [Fact]
    public void Inventory_TotalWeight_SumsItemWeights()
    {
        var inv = new Inventory(10);
        inv.Add(new TestItem { ItemId = "a", Name = "A", Weight = 2.5f });
        inv.Add(new TestItem { ItemId = "b", Name = "B", Weight = 3.5f });

        Assert.Equal(6.0f, inv.TotalWeight);
    }

    [Fact]
    public void Inventory_StackableItems_MergeOnAdd()
    {
        var inv = new Inventory(10);
        var item1 = new TestStackableItem { ItemId = "potion", Name = "薬", Weight = 0.5f, StackCount = 3 };
        var item2 = new TestStackableItem { ItemId = "potion", Name = "薬", Weight = 0.5f, StackCount = 2 };

        inv.Add(item1);
        inv.Add(item2);

        // 同一IDのスタック可能アイテムは合流するのでスロットは1つ
        Assert.Equal(1, inv.UsedSlots);
        Assert.Equal(5, item1.StackCount);
    }

    [Fact]
    public void Inventory_FindById_ReturnsCorrectItem()
    {
        var inv = new Inventory(10);
        var item = new TestItem { ItemId = "rare_gem", Name = "宝石", Weight = 0.1f };
        inv.Add(item);

        var found = inv.FindById("rare_gem");

        Assert.NotNull(found);
        Assert.Equal("宝石", found.Name);
    }

    [Fact]
    public void Inventory_FindById_ReturnsNullWhenNotFound()
    {
        var inv = new Inventory(10);

        Assert.Null(inv.FindById("nonexistent"));
    }

    [Fact]
    public void Inventory_Clear_RemovesAllItems()
    {
        var inv = new Inventory(10);
        inv.Add(new TestItem { ItemId = "a", Name = "A", Weight = 1f });
        inv.Add(new TestItem { ItemId = "b", Name = "B", Weight = 1f });

        inv.Clear();

        Assert.Equal(0, inv.UsedSlots);
    }

    #endregion

    // ================================================================
    // StatusEffect テスト
    // ================================================================

    #region StatusEffect - 基本動作

    [Fact]
    public void StatusEffect_Constructor_SetsTypeAndDuration()
    {
        var effect = new StatusEffect(StatusEffectType.Poison, 10);

        Assert.Equal(StatusEffectType.Poison, effect.Type);
        Assert.Equal(10, effect.Duration);
        Assert.Equal("Poison", effect.Name);
        Assert.False(effect.IsExpired);
    }

    [Fact]
    public void StatusEffect_Tick_DecrementsDuration()
    {
        var effect = new StatusEffect(StatusEffectType.Burn, 3);

        effect.Tick();

        Assert.Equal(2, effect.Duration);
    }

    [Fact]
    public void StatusEffect_IsExpired_TrueWhenDurationZero()
    {
        var effect = new StatusEffect(StatusEffectType.Freeze, 1);

        effect.Tick();

        Assert.True(effect.IsExpired);
    }

    [Fact]
    public void StatusEffect_Stack_IncreasesStackCount()
    {
        var effect = new StatusEffect(StatusEffectType.Poison, 5) { MaxStack = 5 };
        var additional = new StatusEffect(StatusEffectType.Poison, 3) { MaxStack = 5 };

        effect.Stack(additional);

        Assert.Equal(2, effect.StackCount);
        // Duration は大きい方を維持
        Assert.Equal(5, effect.Duration);
    }

    [Fact]
    public void StatusEffect_Stack_CapsAtMaxStack()
    {
        var effect = new StatusEffect(StatusEffectType.Bleeding, 5) { MaxStack = 3 };

        for (int i = 0; i < 5; i++)
        {
            effect.Stack(new StatusEffect(StatusEffectType.Bleeding, 5) { MaxStack = 3 });
        }

        Assert.Equal(3, effect.StackCount);
    }

    [Fact]
    public void StatusEffect_Stack_DifferentType_DoesNotStack()
    {
        var poison = new StatusEffect(StatusEffectType.Poison, 5) { MaxStack = 5 };
        var burn = new StatusEffect(StatusEffectType.Burn, 3) { MaxStack = 5 };

        poison.Stack(burn);

        Assert.Equal(1, poison.StackCount);
    }

    [Fact]
    public void StatusEffect_IsStackable_FalseWhenMaxStackIsOne()
    {
        var effect = new StatusEffect(StatusEffectType.Sleep, 3);

        Assert.Equal(1, effect.MaxStack);
        Assert.False(effect.IsStackable);
    }

    #endregion

    #region StatusEffect - ダメージ効果

    [Fact]
    public void StatusEffect_HasDamageEffect_TrueWhenDamagePerTickPositive()
    {
        var effect = new StatusEffect(StatusEffectType.Poison, 5) { DamagePerTick = 3 };

        Assert.True(effect.HasDamageEffect);
    }

    [Fact]
    public void StatusEffect_GetTickDamage_ReturnsDamageScaledByStacks()
    {
        var effect = new StatusEffect(StatusEffectType.Poison, 5)
        {
            DamagePerTick = 4,
            DamageElement = Element.Poison,
            MaxStack = 3
        };
        effect.Stack(new StatusEffect(StatusEffectType.Poison, 5) { MaxStack = 3 });

        var damage = effect.GetTickDamage();

        Assert.NotNull(damage);
        // DamagePerTick * StackCount = 4 * 2 = 8
        Assert.Equal(8, damage.Value.Amount);
        Assert.Equal(DamageType.Pure, damage.Value.Type);
        Assert.Equal(Element.Poison, damage.Value.Element);
    }

    [Fact]
    public void StatusEffect_GetTickDamage_ReturnsNullWhenNoDamage()
    {
        var effect = new StatusEffect(StatusEffectType.Haste, 5);

        Assert.Null(effect.GetTickDamage());
    }

    #endregion

    #region StatusEffect - コンストラクタデフォルト値

    [Theory]
    [InlineData(StatusEffectType.Poison, 1.0f, 1.0f, 3)]
    [InlineData(StatusEffectType.Bleeding, 1.0f, 1.0f, 2)]
    [InlineData(StatusEffectType.Burn, 0.85f, 1.0f, 4)]
    [InlineData(StatusEffectType.Paralysis, 0.7f, 0.8f, 0)]
    [InlineData(StatusEffectType.Weakness, 0.7f, 0.7f, 0)]
    [InlineData(StatusEffectType.Vulnerability, 1.0f, 0.5f, 0)]
    [InlineData(StatusEffectType.Strength, 1.25f, 1.0f, 0)]
    [InlineData(StatusEffectType.Blessing, 1.1f, 1.1f, 0)]
    [InlineData(StatusEffectType.Slow, 0.8f, 1.0f, 0)]
    [InlineData(StatusEffectType.Protection, 1.0f, 1.5f, 0)]
    [InlineData(StatusEffectType.Haste, 1.2f, 1.0f, 0)]
    [InlineData(StatusEffectType.Regeneration, 1.0f, 1.0f, -3)]
    [InlineData(StatusEffectType.Freeze, 0.5f, 1.0f, 0)]
    [InlineData(StatusEffectType.Fear, 0.7f, 0.8f, 0)]
    [InlineData(StatusEffectType.Blind, 0.7f, 1.0f, 0)]
    [InlineData(StatusEffectType.Confusion, 0.85f, 0.85f, 0)]
    [InlineData(StatusEffectType.Charm, 0.5f, 0.8f, 0)]
    [InlineData(StatusEffectType.Madness, 1.3f, 0.5f, 0)]
    [InlineData(StatusEffectType.Stun, 0.5f, 0.8f, 0)]
    [InlineData(StatusEffectType.Petrification, 0.0f, 3.0f, 0)]
    [InlineData(StatusEffectType.InstantDeath, 1.0f, 1.0f, 999999)]
    [InlineData(StatusEffectType.Curse, 0.9f, 0.9f, 1)]
    [InlineData(StatusEffectType.Apostasy, 0.9f, 0.9f, 0)]
    public void StatusEffect_Constructor_SetsCorrectDefaultMultipliers(
        StatusEffectType type, float expectedAtk, float expectedDef, int expectedDpt)
    {
        var effect = new StatusEffect(type, 10);

        Assert.Equal(expectedAtk, effect.AttackMultiplier);
        Assert.Equal(expectedDef, effect.DefenseMultiplier);
        Assert.Equal(expectedDpt, effect.DamagePerTick);
    }

    [Theory]
    [InlineData(StatusEffectType.Sleep, 1.0f, 1.0f, 0)]
    [InlineData(StatusEffectType.Silence, 1.0f, 1.0f, 0)]
    [InlineData(StatusEffectType.Invisibility, 1.0f, 1.0f, 0)]
    [InlineData(StatusEffectType.FireResistance, 1.0f, 1.0f, 0)]
    [InlineData(StatusEffectType.ColdResistance, 1.0f, 1.0f, 0)]
    public void StatusEffect_Constructor_DefaultCasesHaveNoStatChange(
        StatusEffectType type, float expectedAtk, float expectedDef, int expectedDpt)
    {
        var effect = new StatusEffect(type, 10);

        Assert.Equal(expectedAtk, effect.AttackMultiplier);
        Assert.Equal(expectedDef, effect.DefenseMultiplier);
        Assert.Equal(expectedDpt, effect.DamagePerTick);
    }

    [Theory]
    [InlineData(StatusEffectType.Haste, 0.75f)]
    [InlineData(StatusEffectType.Slow, 1.5f)]
    [InlineData(StatusEffectType.Paralysis, 1.5f)]
    [InlineData(StatusEffectType.Confusion, 1.3f)]
    [InlineData(StatusEffectType.Freeze, 999f)]
    [InlineData(StatusEffectType.Sleep, 999f)]
    [InlineData(StatusEffectType.Stun, 999f)]
    [InlineData(StatusEffectType.Petrification, 999f)]
    public void StatusEffect_Constructor_SetsCorrectTurnCostModifier(
        StatusEffectType type, float expectedTurnCost)
    {
        var effect = new StatusEffect(type, 10);

        Assert.Equal(expectedTurnCost, effect.TurnCostModifier);
    }

    [Fact]
    public void StatusEffect_Constructor_DefaultTurnCostModifierIsOne()
    {
        // 特別な行動速度修正がないタイプ
        var effect = new StatusEffect(StatusEffectType.Poison, 10);
        Assert.Equal(1.0f, effect.TurnCostModifier);
    }

    [Fact]
    public void StatusEffect_Constructor_BlindHasHitRatePenalty()
    {
        var effect = new StatusEffect(StatusEffectType.Blind, 10);

        Assert.Equal(-0.5f, effect.HitRateModifier);
    }

    [Fact]
    public void StatusEffect_Constructor_InvisibilityHasEvasionBonus()
    {
        var effect = new StatusEffect(StatusEffectType.Invisibility, 10);

        Assert.Equal(0.5f, effect.EvasionRateModifier);
        Assert.Equal(-0.1f, effect.HitRateModifier);
    }

    [Fact]
    public void StatusEffect_Constructor_CurseHasAllStatsReduction()
    {
        var effect = new StatusEffect(StatusEffectType.Curse, 10);

        Assert.Equal(0.8f, effect.AllStatsMultiplier);
    }

    [Fact]
    public void StatusEffect_Constructor_ApostasyHasAllStatsReduction()
    {
        var effect = new StatusEffect(StatusEffectType.Apostasy, 10);

        Assert.Equal(0.9f, effect.AllStatsMultiplier);
    }

    [Fact]
    public void StatusEffect_Constructor_HasteHasEvasionBonus()
    {
        var effect = new StatusEffect(StatusEffectType.Haste, 10);

        Assert.Equal(0.1f, effect.EvasionRateModifier);
    }

    #endregion
}
