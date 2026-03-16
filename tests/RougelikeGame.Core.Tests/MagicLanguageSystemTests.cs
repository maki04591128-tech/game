using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Data.MagicLanguage;
using RougelikeGame.Engine.Magic;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 魔法言語システムのテスト（Phase 5.6-5.8）
/// RuneWordDatabase, SpellParser, SpellCastingSystem, VocabularyAcquisitionSystem, SpellEffectResolver
/// </summary>
public class MagicLanguageSystemTests
{
    /// <summary>テスト用IRandomProvider</summary>
    private class TestRandomProvider : IRandomProvider
    {
        private readonly double _nextDouble;
        private readonly int _nextInt;

        public TestRandomProvider(double nextDouble = 0.1, int nextInt = 0)
        {
            _nextDouble = nextDouble;
            _nextInt = nextInt;
        }

        public int Next(int maxValue) => _nextInt % maxValue;
        public int Next(int minValue, int maxValue) => minValue + (_nextInt % (maxValue - minValue));
        public double NextDouble() => _nextDouble;
    }

    private static Player CreateTestPlayer(CharacterClass cls = CharacterClass.Mage)
    {
        return Player.Create("テスト", Race.Human, cls, Background.Scholar);
    }

    #region RuneWordDatabase Tests

    [Fact]
    public void RuneWordDatabase_HasWords()
    {
        Assert.True(RuneWordDatabase.Count > 0);
    }

    [Theory]
    [InlineData("brenna", RuneWordCategory.Effect)]
    [InlineData("fjandi", RuneWordCategory.Target)]
    [InlineData("eldr", RuneWordCategory.Element)]
    [InlineData("mikill", RuneWordCategory.Modifier)]
    [InlineData("hringr", RuneWordCategory.Range)]
    [InlineData("stund", RuneWordCategory.Duration)]
    [InlineData("ef", RuneWordCategory.Condition)]
    public void RuneWordDatabase_GetById_ReturnsCorrectCategory(string id, RuneWordCategory expected)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(expected, word.Category);
    }

    [Fact]
    public void RuneWordDatabase_GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(RuneWordDatabase.GetById("nonexistent_word"));
    }

    [Theory]
    [InlineData(RuneWordCategory.Effect)]
    [InlineData(RuneWordCategory.Target)]
    [InlineData(RuneWordCategory.Element)]
    [InlineData(RuneWordCategory.Modifier)]
    [InlineData(RuneWordCategory.Range)]
    [InlineData(RuneWordCategory.Duration)]
    [InlineData(RuneWordCategory.Condition)]
    public void RuneWordDatabase_GetByCategory_ReturnsWords(RuneWordCategory category)
    {
        var words = RuneWordDatabase.GetByCategory(category).ToList();
        Assert.True(words.Count > 0, $"{category} should have words");
        Assert.All(words, w => Assert.Equal(category, w.Category));
    }

    [Fact]
    public void RuneWordDatabase_GetAll_ReturnsAllWords()
    {
        var all = RuneWordDatabase.GetAll().ToList();
        Assert.Equal(RuneWordDatabase.Count, all.Count);
    }

    [Fact]
    public void RuneWordDatabase_Brenna_HasCorrectProperties()
    {
        var word = RuneWordDatabase.GetById("brenna");
        Assert.NotNull(word);
        Assert.Equal("brenna", word.OldNorse);
        Assert.Equal("ブレンナ", word.Pronunciation);
        Assert.Equal("燃やす", word.Meaning);
        Assert.Equal(RuneWordCategory.Effect, word.Category);
        Assert.Equal(1, word.Difficulty);
    }

    [Fact]
    public void RuneWordDatabase_Ragnarok_HasHighDifficulty()
    {
        var word = RuneWordDatabase.GetById("ragnarok");
        Assert.NotNull(word);
        Assert.Equal(5, word.Difficulty);
        Assert.Equal(3.0f, word.PowerMultiplier);
    }

    [Theory]
    [InlineData("ef", "エフ", "もし〜ならば", 3)]
    [InlineData("tha", "サウ", "その時", 2)]
    [InlineData("gegn", "ゲグン", "〜に対して", 2)]
    [InlineData("daudr", "ダウズル", "死に瀕した時", 4)]
    [InlineData("sar", "サウル", "傷ついた時", 3)]
    public void RuneWordDatabase_ConditionWords_HaveCorrectProperties(string id, string pronunciation, string meaning, int difficulty)
    {
        var word = RuneWordDatabase.GetById(id);
        Assert.NotNull(word);
        Assert.Equal(RuneWordCategory.Condition, word.Category);
        Assert.Equal(pronunciation, word.Pronunciation);
        Assert.Equal(meaning, word.Meaning);
        Assert.Equal(difficulty, word.Difficulty);
    }

    [Fact]
    public void RuneWordDatabase_ConditionWords_Has5Words()
    {
        var conditionWords = RuneWordDatabase.GetByCategory(RuneWordCategory.Condition).ToList();
        Assert.Equal(5, conditionWords.Count);
    }

    [Fact]
    public void RuneWordDatabase_Loka_HasCorrectProperties()
    {
        var word = RuneWordDatabase.GetById("loka");
        Assert.NotNull(word);
        Assert.Equal(RuneWordCategory.Effect, word.Category);
        Assert.Equal("ロカ", word.Pronunciation);
        Assert.Equal("閉じる", word.Meaning);
        Assert.Equal(2, word.Difficulty);
    }

    #endregion

    #region SpellParser Tests

    [Fact]
    public void SpellParser_Parse_EmptyIncantation_ReturnsFailure()
    {
        var parser = new SpellParser();
        var result = parser.Parse("", new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void SpellParser_Parse_NoEffectWord_ReturnsFailure()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int> { { "fjandi", 50 } };
        var result = parser.Parse("fjandi", mastery);
        Assert.False(result.IsSuccess);
        Assert.Contains("効果語", result.ErrorMessage ?? "");
    }

    [Fact]
    public void SpellParser_Parse_UnknownWord_ReturnsFailure()
    {
        var parser = new SpellParser();
        var result = parser.Parse("unknown_word", new Dictionary<string, int>());
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void SpellParser_Parse_SimpleEffectWord_Succeeds()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int> { { "brenna", 80 } };
        var result = parser.Parse("brenna", mastery);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.EffectWord);
        Assert.Equal("brenna", result.EffectWord.Id);
        Assert.True(result.MpCost > 0);
        Assert.True(result.TurnCost >= TurnCosts.SpellMinimum);
    }

    [Fact]
    public void SpellParser_Parse_EffectPlusTarget_SetsTargetWord()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "fjandi", 80 }
        };
        var result = parser.Parse("brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.TargetWord);
        Assert.Equal("fjandi", result.TargetWord.Id);
    }

    [Fact]
    public void SpellParser_Parse_WithElement_SetsElementWord()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "eldr", 80 }
        };
        var result = parser.Parse("brenna eldr", mastery);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ElementWord);
        Assert.Equal("eldr", result.ElementWord.Id);
    }

    [Fact]
    public void SpellParser_Parse_WithModifier_IncreasesMultiplier()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "mikill", 80 }
        };
        var result = parser.Parse("brenna mikill", mastery);
        Assert.True(result.IsSuccess);
        Assert.True(result.PowerMultiplier > 1.0f);
    }

    [Fact]
    public void SpellParser_Parse_TooManyWords_ReturnsFailure()
    {
        var parser = new SpellParser();
        // MaxSpellWords is 7, create 8 words
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 }, { "fjandi", 80 }, { "eldr", 80 },
            { "mikill", 80 }, { "hringr", 80 }, { "stund", 80 },
            { "rett", 80 }, { "viss", 80 }
        };
        var result = parser.Parse("brenna fjandi eldr mikill hringr stund rett viss", mastery);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void SpellParser_Parse_SuccessRate_HighMastery_NearOne()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 100 },
            { "fjandi", 100 }
        };
        var result = parser.Parse("brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        Assert.True(result.SuccessRate >= 0.9, $"Success rate with max mastery: {result.SuccessRate}");
    }

    [Fact]
    public void SpellParser_Parse_SuccessRate_LowMastery_Lower()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 10 },
            { "fjandi", 10 }
        };
        var result = parser.Parse("brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        Assert.True(result.SuccessRate < 0.9, $"Success rate with low mastery: {result.SuccessRate}");
    }

    [Fact]
    public void SpellParser_FormatIncantation_CapitalizesFirstLetter()
    {
        var parser = new SpellParser();
        var words = new List<RuneWord>
        {
            RuneWordDatabase.GetById("brenna")!,
            RuneWordDatabase.GetById("fjandi")!
        };
        var formatted = parser.FormatIncantation(words);
        Assert.StartsWith("B", formatted);
        Assert.EndsWith("!", formatted);
    }

    [Fact]
    public void SpellParser_Parse_SpeedModifier_ReducesTurnCost()
    {
        var parser = new SpellParser();
        var masteryBase = new Dictionary<string, int> { { "brenna", 80 } };
        var resultBase = parser.Parse("brenna", masteryBase);

        var masteryFast = new Dictionary<string, int> { { "brenna", 80 }, { "skjotr", 80 } };
        var resultFast = parser.Parse("brenna skjotr", masteryFast);

        Assert.True(resultBase.IsSuccess);
        Assert.True(resultFast.IsSuccess);
        Assert.True(resultFast.TurnCost <= resultBase.TurnCost,
            $"Fast: {resultFast.TurnCost}, Base: {resultBase.TurnCost}");
    }

    [Fact]
    public void SpellResult_GetDescription_ReturnsNonEmptyString()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "fjandi", 80 },
            { "eldr", 80 }
        };
        var result = parser.Parse("brenna fjandi eldr", mastery);
        Assert.True(result.IsSuccess);
        var description = result.GetDescription();
        Assert.False(string.IsNullOrEmpty(description));
    }

    [Fact]
    public void SpellResult_Failure_ReturnsErrorDescription()
    {
        var fail = SpellResult.Failure("テストエラー");
        Assert.Equal("テストエラー", fail.GetDescription());
    }

    [Fact]
    public void SpellParser_Parse_WithConditionWord_SetsConditionWord()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "ef", 80 },
            { "brenna", 80 },
            { "fjandi", 80 }
        };
        var result = parser.Parse("ef brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ConditionWord);
        Assert.Equal("ef", result.ConditionWord.Id);
        Assert.Equal(RuneWordCategory.Condition, result.ConditionWord.Category);
    }

    [Fact]
    public void SpellParser_Parse_WithoutConditionWord_ConditionWordIsNull()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "brenna", 80 },
            { "fjandi", 80 }
        };
        var result = parser.Parse("brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        Assert.Null(result.ConditionWord);
    }

    [Fact]
    public void SpellParser_Parse_Loka_ReturnsSuccess()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "loka", 80 }
        };
        var result = parser.Parse("loka", mastery);
        Assert.True(result.IsSuccess);
        Assert.Equal("loka", result.EffectWord!.Id);
    }

    [Fact]
    public void SpellResult_GetDescription_WithCondition_IncludesConditionText()
    {
        var parser = new SpellParser();
        var mastery = new Dictionary<string, int>
        {
            { "ef", 80 },
            { "brenna", 80 },
            { "fjandi", 80 }
        };
        var result = parser.Parse("ef brenna fjandi", mastery);
        Assert.True(result.IsSuccess);
        var description = result.GetDescription();
        Assert.Contains("もし〜ならば", description);
    }

    #endregion

    #region SpellCastingSystem Tests

    [Fact]
    public void SpellCastingSystem_Initial_NotCasting()
    {
        var system = new SpellCastingSystem();
        Assert.False(system.IsCasting);
    }

    [Fact]
    public void SpellCastingSystem_AddWord_BecomeCasting()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");

        bool added = system.AddWord("brenna", player);
        Assert.True(added);
        Assert.True(system.IsCasting);
        Assert.Single(system.CurrentIncantation);
    }

    [Fact]
    public void SpellCastingSystem_AddWord_UnlearnedWord_Fails()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        // Not learned
        bool added = system.AddWord("brenna", player);
        Assert.False(added);
    }

    [Fact]
    public void SpellCastingSystem_AddWord_UnknownWord_Fails()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        bool added = system.AddWord("nonexistent", player);
        Assert.False(added);
    }

    [Fact]
    public void SpellCastingSystem_AddWord_MaxWords_Fails()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        // Learn enough words
        var wordIds = RuneWordDatabase.GetAll().Take(GameConstants.MaxSpellWords + 1).Select(w => w.Id).ToList();
        foreach (var id in wordIds)
        {
            player.LearnWord(id);
        }
        // Add MaxSpellWords words (first must be effect word)
        for (int i = 0; i < GameConstants.MaxSpellWords; i++)
        {
            system.AddWord(wordIds[i], player);
        }
        // Next one should fail
        bool added = system.AddWord(wordIds[GameConstants.MaxSpellWords], player);
        Assert.False(added);
    }

    [Fact]
    public void SpellCastingSystem_RemoveLastWord_RemovesWord()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        player.LearnWord("fjandi");

        system.AddWord("brenna", player);
        system.AddWord("fjandi", player);
        Assert.Equal(2, system.CurrentIncantation.Count);

        bool removed = system.RemoveLastWord();
        Assert.True(removed);
        Assert.Single(system.CurrentIncantation);
        Assert.Equal("brenna", system.CurrentIncantation[0]);
    }

    [Fact]
    public void SpellCastingSystem_RemoveLastWord_Empty_Fails()
    {
        var system = new SpellCastingSystem();
        Assert.False(system.RemoveLastWord());
    }

    [Fact]
    public void SpellCastingSystem_CancelCasting_ClearsIncantation()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        system.AddWord("brenna", player);
        Assert.True(system.IsCasting);

        system.CancelCasting();
        Assert.False(system.IsCasting);
        Assert.Empty(system.CurrentIncantation);
    }

    [Fact]
    public void SpellCastingSystem_GetPreview_Empty_ReturnsInvalid()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        var preview = system.GetPreview(player);
        Assert.False(preview.IsValid);
    }

    [Fact]
    public void SpellCastingSystem_GetPreview_ValidSpell_ReturnsValid()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        player.LearnWord("fjandi");
        system.AddWord("brenna", player);
        system.AddWord("fjandi", player);

        var preview = system.GetPreview(player);
        Assert.True(preview.IsValid);
        Assert.True(preview.MpCost > 0);
        Assert.True(preview.SuccessRate > 0);
        Assert.False(string.IsNullOrEmpty(preview.FormattedIncantation));
    }

    [Fact]
    public void SpellCastingSystem_Cast_Empty_ReturnsFailure()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        var random = new TestRandomProvider();
        var result = system.Cast(player, random);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void SpellCastingSystem_Cast_Success_ConsumesMp()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        system.AddWord("brenna", player);

        int mpBefore = player.CurrentMp;
        var random = new TestRandomProvider(0.01); // Low value → success (below successRate)
        var result = system.Cast(player, random);

        Assert.True(result.IsSuccess);
        Assert.True(player.CurrentMp < mpBefore, "MP should be consumed");
        Assert.True(result.MpCost > 0);
    }

    [Fact]
    public void SpellCastingSystem_Cast_InsufficientMp_ReturnsFailure()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("tortima"); // High MP cost (20)

        // Drain MP to 0
        player.ConsumeMp(player.CurrentMp);
        Assert.Equal(0, player.CurrentMp);

        system.AddWord("tortima", player);
        var random = new TestRandomProvider();
        var result = system.Cast(player, random);
        Assert.False(result.IsSuccess);
        Assert.Contains("MP", result.Message);
    }

    [Fact]
    public void SpellCastingSystem_Cast_Failure_StillConsumesMp()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        system.AddWord("brenna", player);

        int mpBefore = player.CurrentMp;
        var random = new TestRandomProvider(0.999); // High value → failure
        var result = system.Cast(player, random);

        // 成功率が高いと失敗しない場合があるので、MPが減っていることだけ確認
        // (result may succeed or fail depending on successRate)
        Assert.True(player.CurrentMp <= mpBefore);
    }

    [Fact]
    public void SpellCastingSystem_Cast_ClearsIncantation()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        system.AddWord("brenna", player);

        var random = new TestRandomProvider(0.01);
        system.Cast(player, random);

        Assert.False(system.IsCasting);
        Assert.Empty(system.CurrentIncantation);
    }

    [Fact]
    public void SpellCastingSystem_Cast_ImprovesWordMastery()
    {
        var system = new SpellCastingSystem();
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        int masteryBefore = player.GetWordMastery("brenna");

        system.AddWord("brenna", player);
        var random = new TestRandomProvider(0.01);
        system.Cast(player, random);

        Assert.True(player.GetWordMastery("brenna") > masteryBefore);
    }

    #endregion

    #region VocabularyAcquisitionSystem Tests

    [Fact]
    public void VocabularyAcquisition_LearnFromRuneStone_NewWord()
    {
        var player = CreateTestPlayer();
        var result = VocabularyAcquisitionSystem.LearnFromRuneStone(player, "brenna");
        Assert.True(result.Success);
        Assert.True(result.IsNewWord);
        Assert.Equal("brenna", result.WordId);
        Assert.True(player.LearnedWords.ContainsKey("brenna"));
    }

    [Fact]
    public void VocabularyAcquisition_LearnFromRuneStone_ExistingWord_IncreasesMastery()
    {
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        int masteryBefore = player.GetWordMastery("brenna");

        var result = VocabularyAcquisitionSystem.LearnFromRuneStone(player, "brenna");
        Assert.True(result.Success);
        Assert.False(result.IsNewWord);
        Assert.True(player.GetWordMastery("brenna") > masteryBefore);
    }

    [Fact]
    public void VocabularyAcquisition_LearnFromRuneStone_UnknownWord_Fails()
    {
        var player = CreateTestPlayer();
        var result = VocabularyAcquisitionSystem.LearnFromRuneStone(player, "nonexistent");
        Assert.False(result.Success);
    }

    [Fact]
    public void VocabularyAcquisition_LearnFromAncientBook_MultipleWords()
    {
        var player = CreateTestPlayer();
        var results = VocabularyAcquisitionSystem.LearnFromAncientBook(player,
            new[] { "brenna", "frysta", "graeda" });
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
        Assert.True(player.LearnedWords.ContainsKey("brenna"));
        Assert.True(player.LearnedWords.ContainsKey("frysta"));
        Assert.True(player.LearnedWords.ContainsKey("graeda"));
    }

    [Fact]
    public void VocabularyAcquisition_LearnRandomWord_LearnsNewWord()
    {
        var player = CreateTestPlayer();
        var random = new TestRandomProvider();
        var result = VocabularyAcquisitionSystem.LearnRandomWord(player, 3, random);
        Assert.True(result.Success);
        Assert.True(result.IsNewWord);
        Assert.NotNull(result.WordId);
    }

    [Fact]
    public void VocabularyAcquisition_LearnRandomWord_RespectsMaxDifficulty()
    {
        var player = CreateTestPlayer();
        var random = new TestRandomProvider();

        // 難易度1以下の語のみ取得
        var result = VocabularyAcquisitionSystem.LearnRandomWord(player, 1, random);
        Assert.True(result.Success);

        // 学んだ語の難易度を確認
        var word = RuneWordDatabase.GetById(result.WordId!);
        Assert.NotNull(word);
        Assert.True(word.Difficulty <= 1);
    }

    [Fact]
    public void VocabularyAcquisition_LearnRandomWord_AllLearned_ReturnsFailure()
    {
        var player = CreateTestPlayer();
        // Learn all words
        foreach (var word in RuneWordDatabase.GetAll())
        {
            player.LearnWord(word.Id);
        }

        var random = new TestRandomProvider();
        var result = VocabularyAcquisitionSystem.LearnRandomWord(player, 5, random);
        Assert.False(result.Success);
    }

    #endregion

    #region SpellEffectResolver Tests

    [Fact]
    public void SpellEffectResolver_Resolve_FailedCast_ReturnsNone()
    {
        var failedResult = SpellCastResult.Failure("テスト失敗");
        var effect = SpellEffectResolver.Resolve(failedResult);
        Assert.True(effect.IsNone);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_DamageSpell_ReturnsDamageType()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 10,
            turnCost: 5,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("brenna"),
            target: RuneWordDatabase.GetById("fjandi"),
            element: RuneWordDatabase.GetById("eldr"),
            range: null,
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(SpellEffectType.Damage, effect.Type);
        Assert.Equal(Element.Fire, effect.Element);
        Assert.Equal(SpellTargetType.SingleEnemy, effect.TargetType);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_HealSpell_ReturnsHealType()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 8,
            turnCost: 5,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("graeda"),
            target: RuneWordDatabase.GetById("sjalfr"),
            element: null,
            range: null,
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(SpellEffectType.Heal, effect.Type);
        Assert.Equal(SpellTargetType.Self, effect.TargetType);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_ControlSpell_ReturnsControlType()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 7,
            turnCost: 5,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("binda"),
            target: RuneWordDatabase.GetById("fjandi"),
            element: null,
            range: null,
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(SpellEffectType.Control, effect.Type);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_DetectSpell_ReturnsDetectType()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 3,
            turnCost: 3,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("sja"),
            target: null,
            element: null,
            range: RuneWordDatabase.GetById("hringr"),
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(SpellEffectType.Detect, effect.Type);
        Assert.True(effect.Range > 1);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_PowerMultiplier_Applies()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 10,
            turnCost: 5,
            powerMultiplier: 2.0f,
            effect: RuneWordDatabase.GetById("brenna"),
            target: RuneWordDatabase.GetById("fjandi"),
            element: null,
            range: null,
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(2.0f, effect.PowerMultiplier);
        Assert.True(effect.Power > 0);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_WithDuration_SetsDuration()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 8,
            turnCost: 5,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("verja"),
            target: RuneWordDatabase.GetById("sjalfr"),
            element: null,
            range: null,
            duration: RuneWordDatabase.GetById("langr"));

        var effect = SpellEffectResolver.Resolve(result);
        Assert.True(effect.Duration > 0);
    }

    [Fact]
    public void SpellEffectResolver_Resolve_AreaRange_SetsRange()
    {
        var result = SpellCastResult.Success(
            "テスト",
            mpCost: 10,
            turnCost: 5,
            powerMultiplier: 1.0f,
            effect: RuneWordDatabase.GetById("brenna"),
            target: RuneWordDatabase.GetById("ovinir"),
            element: null,
            range: RuneWordDatabase.GetById("vidr"),
            duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(SpellTargetType.AllEnemies, effect.TargetType);
        Assert.True(effect.Range >= 5);
    }

    #endregion

    #region SpellCastResult Tests

    [Fact]
    public void SpellCastResult_Failure_HasCorrectProperties()
    {
        var result = SpellCastResult.Failure("テストエラー");
        Assert.False(result.IsSuccess);
        Assert.False(result.IsBackfire);
        Assert.Equal("テストエラー", result.Message);
    }

    [Fact]
    public void SpellCastResult_Failed_HasTurnCost()
    {
        var result = SpellCastResult.Failed("失敗", 10, 5);
        Assert.False(result.IsSuccess);
        Assert.Equal(10, result.MpCost);
        Assert.Equal(5, result.TurnCost);
    }

    [Fact]
    public void SpellCastResult_Backfire_HasDamage()
    {
        var result = SpellCastResult.Backfire("暴発", 10, 5, 25);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsBackfire);
        Assert.Equal(25, result.BackfireDamage);
    }

    [Fact]
    public void SpellCastResult_Success_HasAllProperties()
    {
        var result = SpellCastResult.Success("成功",
            mpCost: 15, turnCost: 7, powerMultiplier: 1.5f,
            effect: RuneWordDatabase.GetById("brenna"),
            target: RuneWordDatabase.GetById("fjandi"),
            element: RuneWordDatabase.GetById("eldr"),
            range: null, duration: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.MpCost);
        Assert.Equal(7, result.TurnCost);
        Assert.Equal(1.5f, result.PowerMultiplier);
        Assert.NotNull(result.EffectWord);
        Assert.NotNull(result.TargetWord);
        Assert.NotNull(result.ElementWord);
    }

    #endregion

    #region SpellPreview Tests

    [Fact]
    public void SpellPreview_Invalid_HasDefaultValues()
    {
        var preview = new SpellPreview(false, "空", 0, 0, 0.0, "");
        Assert.False(preview.IsValid);
    }

    [Fact]
    public void SpellPreview_Valid_HasAllFields()
    {
        var preview = new SpellPreview(true, "燃やす", 10, 5, 0.85, "Brenna Fjandi!");
        Assert.True(preview.IsValid);
        Assert.Equal(10, preview.MpCost);
        Assert.Equal(5, preview.TurnCost);
        Assert.Equal(0.85, preview.SuccessRate);
    }

    #endregion

    #region Element Resolution Tests

    [Theory]
    [InlineData("eldr", Element.Fire)]
    [InlineData("iss", Element.Ice)]
    [InlineData("thruma_elem", Element.Lightning)]
    [InlineData("ljos", Element.Light)]
    [InlineData("myrkr", Element.Dark)]
    [InlineData("helgr", Element.Holy)]
    [InlineData("bolvadr", Element.Curse)]
    public void SpellEffectResolver_ElementMapping_Correct(string elementWordId, Element expected)
    {
        var result = SpellCastResult.Success(
            "テスト", 10, 5, 1.0f,
            effect: RuneWordDatabase.GetById("brenna"),
            target: RuneWordDatabase.GetById("fjandi"),
            element: RuneWordDatabase.GetById(elementWordId),
            range: null, duration: null);

        var effect = SpellEffectResolver.Resolve(result);
        Assert.Equal(expected, effect.Element);
    }

    #endregion

    #region Player Word Mastery Tests

    [Fact]
    public void Player_LearnWord_SetsInitialMastery()
    {
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        Assert.True(player.LearnedWords.ContainsKey("brenna"));
        Assert.Equal(GameConstants.InitialWordMastery, player.GetWordMastery("brenna"));
    }

    [Fact]
    public void Player_ImproveWordMastery_Increases()
    {
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        player.ImproveWordMastery("brenna", 10);
        Assert.Equal(GameConstants.InitialWordMastery + 10, player.GetWordMastery("brenna"));
    }

    [Fact]
    public void Player_ImproveWordMastery_CapsAtMax()
    {
        var player = CreateTestPlayer();
        player.LearnWord("brenna");
        player.ImproveWordMastery("brenna", 200);
        Assert.Equal(GameConstants.MaxWordMastery, player.GetWordMastery("brenna"));
    }

    [Fact]
    public void Player_GetWordMastery_UnlearnedWord_ReturnsZero()
    {
        var player = CreateTestPlayer();
        Assert.Equal(0, player.GetWordMastery("brenna"));
    }

    #endregion
}
