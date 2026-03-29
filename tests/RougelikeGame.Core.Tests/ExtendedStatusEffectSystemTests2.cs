using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 拡張状態異常システム 追加テスト
/// テスト数: 16件
/// </summary>
public class ExtendedStatusEffectSystemTests2
{
    // ============================================================
    // GetAll テスト
    // ============================================================

    [Fact]
    public void GetAll_AllEffectsHaveNonEmptyName()
    {
        // 全エフェクトの名前が空でない
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.All(effects.Values, e => Assert.False(string.IsNullOrEmpty(e.Name)));
    }

    [Fact]
    public void GetAll_AllEffectsHaveNonEmptyDescription()
    {
        // 全エフェクトの説明が空でない
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.All(effects.Values, e => Assert.False(string.IsNullOrEmpty(e.Description)));
    }

    [Fact]
    public void GetAll_AllEffectsHavePositiveDuration()
    {
        // 全エフェクトのデフォルト持続ターンが正の値
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.All(effects.Values, e => Assert.True(e.DefaultDuration > 0));
    }

    [Fact]
    public void GetAll_AllEffectsHaveStatModifiers()
    {
        // 全エフェクトにステータス修正値が存在する
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.All(effects.Values, e => Assert.NotEmpty(e.StatModifiers));
    }

    // ============================================================
    // 個別エフェクト検証テスト
    // ============================================================

    [Theory]
    [InlineData("intoxication", "酩酊", false)]
    [InlineData("frostbite", "凍傷", false)]
    [InlineData("infection", "感染症", false)]
    [InlineData("bind", "呪縛", false)]
    [InlineData("hallucination", "幻惑", false)]
    [InlineData("vampiric_drain", "吸血", false)]
    [InlineData("berserk", "狂戦士", false)]
    [InlineData("corrosion", "腐食", false)]
    [InlineData("marked", "標的", false)]
    [InlineData("blessed_weapon", "神聖武装", true)]
    [InlineData("iron_skin", "鉄の肌", true)]
    [InlineData("haste_burst", "疾風", true)]
    public void GetById_AllEffects_HaveCorrectProperties(string id, string expectedName, bool expectedIsBuff)
    {
        // 各エフェクトのID・名前・バフ/デバフ分類が正しい
        var effect = ExtendedStatusEffectSystem.GetById(id);
        Assert.NotNull(effect);
        Assert.Equal(expectedName, effect!.Name);
        Assert.Equal(expectedIsBuff, effect.IsBuff);
        Assert.Equal(id, effect.Id);
    }

    // ============================================================
    // GetBuffs / GetDebuffs テスト
    // ============================================================

    [Fact]
    public void GetBuffs_Returns3Buffs()
    {
        // バフは3種類
        var buffs = ExtendedStatusEffectSystem.GetBuffs();
        Assert.Equal(3, buffs.Count);
    }

    [Fact]
    public void GetDebuffs_Returns9Debuffs()
    {
        // デバフは9種類
        var debuffs = ExtendedStatusEffectSystem.GetDebuffs();
        Assert.Equal(9, debuffs.Count);
    }

    [Fact]
    public void GetBuffs_AndDebuffs_SumEquals12()
    {
        // バフ+デバフの合計が全エフェクト数と一致
        var buffs = ExtendedStatusEffectSystem.GetBuffs();
        var debuffs = ExtendedStatusEffectSystem.GetDebuffs();
        Assert.Equal(12, buffs.Count + debuffs.Count);
    }

    // ============================================================
    // GetStatModifier テスト
    // ============================================================

    [Fact]
    public void GetStatModifier_Frostbite_AGI_ReturnsMinus5()
    {
        // 凍傷のAGI修正値が-5
        float agi = ExtendedStatusEffectSystem.GetStatModifier("frostbite", "AGI");
        Assert.Equal(-5f, agi);
    }

    [Fact]
    public void GetStatModifier_Berserk_AttackMultiplier()
    {
        // 狂戦士の攻撃力倍率が+50%
        float atk = ExtendedStatusEffectSystem.GetStatModifier("berserk", "AttackMultiplier");
        Assert.Equal(0.5f, atk);
    }

    [Fact]
    public void GetStatModifier_IronSkin_PhysicalDefense()
    {
        // 鉄の肌の物理防御修正値が+25%
        float def = ExtendedStatusEffectSystem.GetStatModifier("iron_skin", "PhysicalDefense");
        Assert.Equal(0.25f, def);
    }

    [Fact]
    public void GetStatModifier_InvalidEffect_ReturnsZero()
    {
        // 存在しないエフェクトIDで0を返す
        Assert.Equal(0f, ExtendedStatusEffectSystem.GetStatModifier("nonexistent", "AGI"));
    }

    // ============================================================
    // GetEffectName テスト
    // ============================================================

    [Fact]
    public void GetEffectName_InvalidId_ReturnsUnknown()
    {
        // 存在しないIDで「不明」を返す
        Assert.Equal("不明", ExtendedStatusEffectSystem.GetEffectName("nonexistent"));
    }

    [Theory]
    [InlineData("intoxication", "酩酊")]
    [InlineData("blessed_weapon", "神聖武装")]
    [InlineData("haste_burst", "疾風")]
    public void GetEffectName_ValidIds_ReturnsCorrectName(string id, string expectedName)
    {
        // 有効なIDで正しい名前を返す
        Assert.Equal(expectedName, ExtendedStatusEffectSystem.GetEffectName(id));
    }

    // ============================================================
    // ExtendedStatusEffect レコードテスト
    // ============================================================

    [Fact]
    public void ExtendedStatusEffect_BaseType_IsNull()
    {
        // 全拡張状態異常のBaseTypeがnull（基本状態異常との紐付けなし）
        var effects = ExtendedStatusEffectSystem.GetAll();
        Assert.All(effects.Values, e => Assert.Null(e.BaseType));
    }

    [Fact]
    public void ExtendedStatusEffect_DefaultDuration_Varies()
    {
        // エフェクトごとにデフォルト持続ターンが異なる
        var effects = ExtendedStatusEffectSystem.GetAll();
        var durations = effects.Values.Select(e => e.DefaultDuration).Distinct().ToList();
        Assert.True(durations.Count > 1, "複数の異なる持続ターン値が存在すべき");
    }
}
