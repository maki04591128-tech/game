using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// InscriptionSystem（碑文・壁画解読システム）のテスト
/// </summary>
public class InscriptionSystemTests
{
    // --- コンストラクタ ---

    [Fact]
    public void Constructor_EmptyInscriptions()
    {
        var system = new InscriptionSystem();
        Assert.Empty(system.Inscriptions);
        Assert.Equal(0, system.DecodedCount);
    }

    // --- Register ---

    [Fact]
    public void Register_AddsInscription()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Lore, "???", "古の伝承", 5);
        Assert.Single(system.Inscriptions);
        Assert.True(system.Inscriptions.ContainsKey("ins_1"));
    }

    [Fact]
    public void Register_WithReward_StoresRewardInfo()
    {
        var system = new InscriptionSystem();
        system.Register("ins_r", InscriptionType.Recipe, "???", "錬金術レシピ", 3, "recipe_potion");
        var ins = system.Inscriptions["ins_r"];
        Assert.Equal("recipe_potion", ins.RewardInfo);
        Assert.False(ins.IsDecoded);
    }

    [Fact]
    public void Register_SameId_OverwritesExisting()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Lore, "???", "旧テキスト", 1);
        system.Register("ins_1", InscriptionType.Warning, "!!!", "新テキスト", 10);
        Assert.Single(system.Inscriptions);
        Assert.Equal("新テキスト", system.Inscriptions["ins_1"].DecodedText);
    }

    // --- TryDecode ---

    [Fact]
    public void TryDecode_NonExistentId_ReturnsFalse()
    {
        var system = new InscriptionSystem();
        var result = system.TryDecode("missing", 99);
        Assert.False(result.Success);
        Assert.Contains("見つかりません", result.Message);
    }

    [Fact]
    public void TryDecode_SufficientLevel_Succeeds()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Hint, "???", "秘密の通路", 5, "hint_data");
        var result = system.TryDecode("ins_1", 5);
        Assert.True(result.Success);
        Assert.Equal(100, result.PartialProgress);
        Assert.Equal("hint_data", result.RewardInfo);
    }

    [Fact]
    public void TryDecode_InsufficientLevel_ReturnsPartialProgress()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Lore, "???", "古の伝承テキスト", 10);
        var result = system.TryDecode("ins_1", 3);
        Assert.False(result.Success);
        Assert.True(result.PartialProgress > 0 && result.PartialProgress < 100);
        Assert.Contains("解読中", result.Message);
    }

    [Fact]
    public void TryDecode_AlreadyDecoded_ReturnsSuccess()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Spell, "???", "ファイアボール", 1, "spell_fire");
        system.TryDecode("ins_1", 10);
        var result = system.TryDecode("ins_1", 1);
        Assert.True(result.Success);
        Assert.Equal(100, result.PartialProgress);
    }

    // --- DecodedCount ---

    [Fact]
    public void DecodedCount_AfterDecode_Increments()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Lore, "???", "テキスト1", 1);
        system.Register("ins_2", InscriptionType.Warning, "???", "テキスト2", 1);
        Assert.Equal(0, system.DecodedCount);
        system.TryDecode("ins_1", 10);
        Assert.Equal(1, system.DecodedCount);
    }

    // --- GetTypeName ---

    [Theory]
    [InlineData(InscriptionType.Lore, "伝承の碑文")]
    [InlineData(InscriptionType.Warning, "警告の碑文")]
    [InlineData(InscriptionType.Hint, "手がかりの碑文")]
    [InlineData(InscriptionType.Recipe, "秘伝の碑文")]
    [InlineData(InscriptionType.Spell, "呪文の碑文")]
    [InlineData(InscriptionType.Map, "地図の壁画")]
    public void GetTypeName_ReturnsJapaneseName(InscriptionType type, string expected)
    {
        Assert.Equal(expected, InscriptionSystem.GetTypeName(type));
    }

    // --- GetRewardDescription ---

    [Theory]
    [InlineData(InscriptionType.Recipe, "新しいレシピを習得した")]
    [InlineData(InscriptionType.Map, "隠された部屋の位置を把握した")]
    public void GetRewardDescription_ReturnsExpectedText(InscriptionType type, string expected)
    {
        Assert.Equal(expected, InscriptionSystem.GetRewardDescription(type));
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsDecodedState()
    {
        var system = new InscriptionSystem();
        system.Register("ins_1", InscriptionType.Lore, "???", "テキスト", 1);
        system.TryDecode("ins_1", 10);
        Assert.Equal(1, system.DecodedCount);
        system.Reset();
        Assert.Equal(0, system.DecodedCount);
        // 碑文の登録自体は保持される
        Assert.Single(system.Inscriptions);
    }

    // --- GetByType ---

    [Fact]
    public void GetByType_ReturnsMatchingOnly()
    {
        var system = new InscriptionSystem();
        system.Register("lore_1", InscriptionType.Lore, "???", "伝承1", 1);
        system.Register("warn_1", InscriptionType.Warning, "!!!", "警告1", 2);
        system.Register("lore_2", InscriptionType.Lore, "???", "伝承2", 3);
        var loreList = system.GetByType(InscriptionType.Lore);
        Assert.Equal(2, loreList.Count);
    }

    [Fact]
    public void GetByType_NoMatch_ReturnsEmpty()
    {
        var system = new InscriptionSystem();
        system.Register("lore_1", InscriptionType.Lore, "???", "伝承", 1);
        var result = system.GetByType(InscriptionType.Spell);
        Assert.Empty(result);
    }
}
