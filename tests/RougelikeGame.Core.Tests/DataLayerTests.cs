using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Data.MagicLanguage;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// Data層（RuneWordDatabase/SpellParser）のユニットテスト
/// </summary>
public class DataLayerTests
{
    #region RuneWordDatabase

    [Fact]
    public void RuneWordDatabase_Count_IsPositive()
    {
        Assert.True(RuneWordDatabase.Count > 0);
    }

    [Fact]
    public void RuneWordDatabase_GetAll_ReturnsAllWords()
    {
        var all = RuneWordDatabase.GetAll().ToList();
        Assert.Equal(RuneWordDatabase.Count, all.Count);
    }

    [Theory]
    [InlineData("brenna", "燃やす", RuneWordCategory.Effect)]
    [InlineData("frysta", "凍らせる", RuneWordCategory.Effect)]
    [InlineData("thruma", "雷撃つ", RuneWordCategory.Effect)]
    [InlineData("graeda", "癒す", RuneWordCategory.Effect)]
    [InlineData("verja", "守る", RuneWordCategory.Effect)]
    [InlineData("binda", "縛る", RuneWordCategory.Effect)]
    [InlineData("kalla", "召喚する", RuneWordCategory.Effect)]
    public void RuneWordDatabase_GetById_EffectWords_ReturnsCorrectWord(string id, string expectedMeaning, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedMeaning, word.Meaning);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Theory]
    [InlineData("sjalfr", "自分", RuneWordCategory.Target)]
    [InlineData("fjandi", "敵", RuneWordCategory.Target)]
    [InlineData("allir", "全て", RuneWordCategory.Target)]
    public void RuneWordDatabase_GetById_TargetWords_ReturnsCorrectWord(string id, string expectedMeaning, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedMeaning, word.Meaning);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Theory]
    [InlineData("eldr", "炎", RuneWordCategory.Element)]
    [InlineData("vatn", "水", RuneWordCategory.Element)]
    [InlineData("iss", "氷", RuneWordCategory.Element)]
    [InlineData("ljos", "光", RuneWordCategory.Element)]
    [InlineData("myrkr", "闇", RuneWordCategory.Element)]
    public void RuneWordDatabase_GetById_ElementWords_ReturnsCorrectWord(string id, string expectedMeaning, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedMeaning, word.Meaning);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Theory]
    [InlineData("litill", RuneWordCategory.Modifier, 0.5f)]
    [InlineData("mikill", RuneWordCategory.Modifier, 1.3f)]
    [InlineData("sterkr", RuneWordCategory.Modifier, 1.5f)]
    [InlineData("ofr", RuneWordCategory.Modifier, 2.0f)]
    [InlineData("ragnarok", RuneWordCategory.Modifier, 3.0f)]
    public void RuneWordDatabase_ModifierWords_HaveCorrectPowerMultiplier(string id, RuneWordCategory expectedCategory, float expectedPower)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedCategory, word.Category);
        Assert.Equal(expectedPower, word.PowerMultiplier);
    }

    [Theory]
    [InlineData("einn", RuneWordCategory.Range)]
    [InlineData("beinn", RuneWordCategory.Range)]
    [InlineData("hringr", RuneWordCategory.Range)]
    [InlineData("vidr", RuneWordCategory.Range)]
    [InlineData("heimr", RuneWordCategory.Range)]
    public void RuneWordDatabase_RangeWords_ExistWithCorrectCategory(string id, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Theory]
    [InlineData("augnablik", RuneWordCategory.Duration)]
    [InlineData("stund", RuneWordCategory.Duration)]
    [InlineData("langr", RuneWordCategory.Duration)]
    [InlineData("eilifr", RuneWordCategory.Duration)]
    [InlineData("endalauss", RuneWordCategory.Duration)]
    public void RuneWordDatabase_DurationWords_ExistWithCorrectCategory(string id, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Theory]
    [InlineData("ef", RuneWordCategory.Condition)]
    [InlineData("tha", RuneWordCategory.Condition)]
    [InlineData("gegn", RuneWordCategory.Condition)]
    [InlineData("daudr", RuneWordCategory.Condition)]
    [InlineData("sar", RuneWordCategory.Condition)]
    public void RuneWordDatabase_ConditionWords_ExistWithCorrectCategory(string id, RuneWordCategory expectedCategory)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expectedCategory, word.Category);
    }

    [Fact]
    public void RuneWordDatabase_GetById_NonExistent_ReturnsNull()
    {
        var word = RuneWordDatabase.GetById("nonexistent_word_xyz");
        Assert.Null(word);
    }

    [Fact]
    public void RuneWordDatabase_GetByCategory_Effect_ReturnsMultiple()
    {
        var effects = RuneWordDatabase.GetByCategory(RuneWordCategory.Effect).ToList();
        Assert.True(effects.Count >= 20); // 攻撃系+回復系+制御系+特殊系
        Assert.All(effects, w => Assert.Equal(RuneWordCategory.Effect, w.Category));
    }

    [Fact]
    public void RuneWordDatabase_GetByCategory_Target_ReturnsMultiple()
    {
        var targets = RuneWordDatabase.GetByCategory(RuneWordCategory.Target).ToList();
        Assert.True(targets.Count >= 5);
        Assert.All(targets, w => Assert.Equal(RuneWordCategory.Target, w.Category));
    }

    [Fact]
    public void RuneWordDatabase_GetByCategory_Element_ReturnsMultiple()
    {
        var elements = RuneWordDatabase.GetByCategory(RuneWordCategory.Element).ToList();
        Assert.True(elements.Count >= 8);
        Assert.All(elements, w => Assert.Equal(RuneWordCategory.Element, w.Category));
    }

    [Fact]
    public void RuneWordDatabase_AllCategories_HaveWords()
    {
        foreach (RuneWordCategory cat in Enum.GetValues<RuneWordCategory>())
        {
            var words = RuneWordDatabase.GetByCategory(cat).ToList();
            Assert.True(words.Count > 0, $"カテゴリ {cat} に単語がない");
        }
    }

    [Fact]
    public void RuneWordDatabase_AllWords_HaveValidProperties()
    {
        foreach (var word in RuneWordDatabase.GetAll())
        {
            Assert.False(string.IsNullOrEmpty(word.Id), $"IDが空: {word}");
            Assert.False(string.IsNullOrEmpty(word.OldNorse), $"古ノルド語が空: {word.Id}");
            Assert.False(string.IsNullOrEmpty(word.Pronunciation), $"発音が空: {word.Id}");
            Assert.False(string.IsNullOrEmpty(word.Meaning), $"意味が空: {word.Id}");
            Assert.InRange(word.Difficulty, 1, 5);
        }
    }

    [Fact]
    public void RuneWordDatabase_AllWordIds_AreUnique()
    {
        var all = RuneWordDatabase.GetAll().ToList();
        var distinctIds = all.Select(w => w.Id).Distinct().Count();
        Assert.Equal(all.Count, distinctIds);
    }

    [Fact]
    public void RuneWordDatabase_SpeedModifiers_HaveNegativeTurnCostAddition()
    {
        var skjotr = RuneWordDatabase.GetById("skjotr");
        var hradr = RuneWordDatabase.GetById("hradr");
        var thegar = RuneWordDatabase.GetById("thegar");

        Assert.NotNull(skjotr);
        Assert.NotNull(hradr);
        Assert.NotNull(thegar);

        // 速度修飾語はBaseTurnCostが負
        Assert.True(skjotr.BaseTurnCost < 0);
        Assert.True(hradr.BaseTurnCost < 0);
        Assert.True(thegar.BaseTurnCost < 0);
    }

    #endregion

    #region SpellParser

    [Fact]
    public void SpellParser_Parse_EmptyString_ReturnsFailure()
    {
        var parser = new SpellParser();
        var result = parser.Parse("", new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
        Assert.Contains("空", result.ErrorMessage!);
    }

    [Fact]
    public void SpellParser_Parse_TooManyWords_ReturnsFailure()
    {
        var parser = new SpellParser();
        // MaxSpellWords = 7、8語以上で失敗
        var incantation = "brenna fjandi eldr mikill hringr langr ef daudr";
        var result = parser.Parse(incantation, new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
        Assert.Contains("長すぎ", result.ErrorMessage!);
    }

    [Fact]
    public void SpellParser_Parse_UnknownWord_ReturnsFailure()
    {
        var parser = new SpellParser();
        var result = parser.Parse("unknown_xyz", new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
        Assert.Contains("未知", result.ErrorMessage!);
    }

    [Fact]
    public void SpellParser_Parse_NoEffectWord_ReturnsFailure()
    {
        var parser = new SpellParser();
        // TargetWordのみ、EffectWordなし
        var result = parser.Parse("fjandi", new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
        Assert.Contains("効果語", result.ErrorMessage!);
    }

    [Fact]
    public void SpellParser_Parse_SingleEffectWord_Succeeds()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.EffectWord);
        Assert.Equal("brenna", result.EffectWord!.Id);
        Assert.True(result.MpCost > 0);
    }

    [Fact]
    public void SpellParser_Parse_EffectPlusTarget_Succeeds()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna fjandi", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.EffectWord);
        Assert.NotNull(result.TargetWord);
        Assert.Equal("fjandi", result.TargetWord!.Id);
    }

    [Fact]
    public void SpellParser_Parse_FullSpell_CalculatesMpCost()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna fjandi eldr mikill", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.True(result.MpCost > 5); // brenna(5) + fjandi(2) + eldr(0) の組み合わせ×修飾
    }

    [Fact]
    public void SpellParser_Parse_WithModifier_IncreasePower()
    {
        var parser = new SpellParser();
        var baseResult = parser.Parse("brenna fjandi", new Dictionary<string, int>());
        var modifiedResult = parser.Parse("brenna fjandi mikill", new Dictionary<string, int>());

        Assert.True(baseResult.IsSuccess);
        Assert.True(modifiedResult.IsSuccess);
        Assert.True(modifiedResult.PowerMultiplier > baseResult.PowerMultiplier);
    }

    [Fact]
    public void SpellParser_Parse_HighMastery_BetterSuccessRate()
    {
        var parser = new SpellParser();
        var lowMastery = new Dictionary<string, int> { ["brenna"] = 10, ["fjandi"] = 10 };
        var highMastery = new Dictionary<string, int> { ["brenna"] = 100, ["fjandi"] = 100 };

        var lowResult = parser.Parse("brenna fjandi", lowMastery);
        var highResult = parser.Parse("brenna fjandi", highMastery);

        Assert.True(lowResult.IsSuccess);
        Assert.True(highResult.IsSuccess);
        Assert.True(highResult.SuccessRate >= lowResult.SuccessRate);
    }

    [Fact]
    public void SpellParser_Parse_ElementWord_IsDetected()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna eldr fjandi", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ElementWord);
        Assert.Equal("eldr", result.ElementWord!.Id);
    }

    [Fact]
    public void SpellParser_Parse_RangeWord_IsDetected()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna hringr", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.RangeWord);
        Assert.Equal("hringr", result.RangeWord!.Id);
    }

    [Fact]
    public void SpellParser_Parse_DurationWord_IsDetected()
    {
        var parser = new SpellParser();
        var result = parser.Parse("verja langr", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.DurationWord);
        Assert.Equal("langr", result.DurationWord!.Id);
    }

    [Fact]
    public void SpellParser_Parse_ConditionWord_IsDetected()
    {
        var parser = new SpellParser();
        var result = parser.Parse("verja ef", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ConditionWord);
        Assert.Equal("ef", result.ConditionWord!.Id);
    }

    [Fact]
    public void SpellParser_Parse_TurnCost_IsWithinBounds()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna fjandi", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.InRange(result.TurnCost, TurnCosts.SpellMinimum, TurnCosts.SpellMaximum);
    }

    [Fact]
    public void SpellParser_Parse_CaseInsensitive()
    {
        var parser = new SpellParser();
        var lower = parser.Parse("brenna fjandi", new Dictionary<string, int>());
        var upper = parser.Parse("BRENNA FJANDI", new Dictionary<string, int>());

        Assert.True(lower.IsSuccess);
        Assert.True(upper.IsSuccess);
        Assert.Equal(lower.MpCost, upper.MpCost);
    }

    [Fact]
    public void SpellParser_FormatIncantation_ReturnsFormattedString()
    {
        var parser = new SpellParser();
        var words = new[]
        {
            RuneWordDatabase.GetById("brenna")!,
            RuneWordDatabase.GetById("fjandi")!
        };

        var formatted = parser.FormatIncantation(words);
        Assert.EndsWith("!", formatted);
        Assert.Contains("Brenna", formatted);
    }

    [Fact]
    public void SpellResult_Failure_HasErrorMessage()
    {
        var result = SpellResult.Failure("テストエラー");
        Assert.False(result.IsSuccess);
        Assert.Equal("テストエラー", result.ErrorMessage);
    }

    [Fact]
    public void SpellResult_GetDescription_ForSuccessfulSpell()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna fjandi eldr", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);

        var description = result.GetDescription();
        Assert.False(string.IsNullOrEmpty(description));
    }

    [Fact]
    public void SpellResult_GetDescription_ForFailure_ReturnsErrorMessage()
    {
        var result = SpellResult.Failure("エラーです");
        var description = result.GetDescription();
        Assert.Equal("エラーです", description);
    }

    [Fact]
    public void SpellParser_Parse_SpeedModifier_ReducesTurnCost()
    {
        var parser = new SpellParser();
        var normal = parser.Parse("brenna fjandi", new Dictionary<string, int>());
        var fast = parser.Parse("brenna fjandi skjotr", new Dictionary<string, int>());

        Assert.True(normal.IsSuccess);
        Assert.True(fast.IsSuccess);
        Assert.True(fast.TurnCost <= normal.TurnCost);
    }

    [Fact]
    public void SpellParser_Parse_InstantModifier_MinimumTurnCost()
    {
        var parser = new SpellParser();
        var result = parser.Parse("brenna thegar", new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
        Assert.Equal(TurnCosts.SpellMinimum, result.TurnCost);
    }

    #endregion
}
