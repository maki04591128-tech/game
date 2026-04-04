using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;
using RougelikeGame.Data.MagicLanguage;

namespace RougelikeGame.Engine.Magic;

/// <summary>
/// 記録済み呪文レシピ
/// </summary>
public record SavedSpellRecipe(
    string Name,
    List<string> WordIds,
    string Description,
    int MpCost,
    string FormattedIncantation);

/// <summary>
/// 詠唱システム - 魔法言語の詠唱UIロジックと発動制御
/// </summary>
public class SpellCastingSystem
{
    private readonly SpellParser _parser = new();
    private readonly List<SavedSpellRecipe> _savedSpells = new();

    /// <summary>記録済み呪文一覧</summary>
    public IReadOnlyList<SavedSpellRecipe> SavedSpells => _savedSpells.AsReadOnly();

    /// <summary>記録済み呪文の最大数</summary>
    public const int MaxSavedSpells = 20;

    /// <summary>現在の詠唱文を呪文として記録する</summary>
    public SavedSpellRecipe? SaveCurrentSpell(string name, Player player)
    {
        if (CurrentIncantation.Count == 0) return null;
        if (_savedSpells.Count >= MaxSavedSpells) return null;
        if (string.IsNullOrWhiteSpace(name)) return null;

        var preview = GetPreview(player);
        if (!preview.IsValid) return null;

        var recipe = new SavedSpellRecipe(
            name,
            new List<string>(CurrentIncantation),
            preview.Description,
            preview.MpCost,
            preview.FormattedIncantation);

        _savedSpells.Add(recipe);
        return recipe;
    }

    /// <summary>記録済み呪文を削除する</summary>
    public bool RemoveSavedSpell(int index)
    {
        if (index < 0 || index >= _savedSpells.Count) return false;
        _savedSpells.RemoveAt(index);
        return true;
    }

    /// <summary>記録済み呪文を詠唱文にロードする</summary>
    public bool LoadSavedSpell(int index, Player player)
    {
        if (index < 0 || index >= _savedSpells.Count) return false;

        var recipe = _savedSpells[index];
        CancelCasting();

        foreach (var wordId in recipe.WordIds)
        {
            if (!AddWord(wordId, player))
            {
                CancelCasting();
                return false;
            }
        }
        return true;
    }

    /// <summary>現在構築中の詠唱文（語ID一覧）</summary>
    public List<string> CurrentIncantation { get; } = new();

    /// <summary>詠唱中かどうか</summary>
    public bool IsCasting => CurrentIncantation.Count > 0;

    /// <summary>語を追加</summary>
    public bool AddWord(string wordId, Player player)
    {
        if (CurrentIncantation.Count >= GameConstants.MaxSpellWords)
            return false;

        var word = RuneWordDatabase.GetById(wordId);
        if (word == null) return false;

        // プレイヤーが習得済みか
        if (!player.LearnedWords.ContainsKey(wordId))
            return false;

        CurrentIncantation.Add(wordId);
        return true;
    }

    /// <summary>最後の語を削除</summary>
    public bool RemoveLastWord()
    {
        if (CurrentIncantation.Count == 0) return false;
        CurrentIncantation.RemoveAt(CurrentIncantation.Count - 1);
        return true;
    }

    /// <summary>詠唱をキャンセル</summary>
    public void CancelCasting()
    {
        CurrentIncantation.Clear();
    }

    /// <summary>現在の詠唱文のプレビューを取得</summary>
    public SpellPreview GetPreview(Player player)
    {
        if (CurrentIncantation.Count == 0)
            return new SpellPreview(false, "詠唱文が空です", 0, 0, 0.0, "");

        var incantation = string.Join(" ", CurrentIncantation);
        var result = _parser.Parse(incantation, player.LearnedWords);

        if (!result.IsSuccess)
            return new SpellPreview(false, result.ErrorMessage ?? "解析失敗", 0, 0, 0.0, "");

        return new SpellPreview(
            true,
            result.GetDescription(),
            result.MpCost,
            result.TurnCost,
            result.SuccessRate,
            _parser.FormatIncantation(result.Words));
    }

    /// <summary>詠唱を実行</summary>
    public SpellCastResult Cast(Player player, IRandomProvider random)
    {
        if (CurrentIncantation.Count == 0)
            return SpellCastResult.Failure("詠唱文が空です");

        var incantation = string.Join(" ", CurrentIncantation);
        var result = _parser.Parse(incantation, player.LearnedWords);

        if (!result.IsSuccess)
        {
            CancelCasting();
            return SpellCastResult.Failure(result.ErrorMessage ?? "解析失敗");
        }

        // MP消費チェック
        if (player.CurrentMp < result.MpCost)
        {
            CancelCasting();
            return SpellCastResult.Failure("MPが足りない");
        }

        // 成功判定
        bool success = random.NextDouble() < result.SuccessRate;

        // MP消費（失敗しても消費）
        player.ConsumeMp(result.MpCost);

        // 使用した語の理解度上昇
        foreach (var wordId in CurrentIncantation)
        {
            player.ImproveWordMastery(wordId, success ? 2 : 1);
        }

        var formattedIncantation = _parser.FormatIncantation(result.Words);
        CancelCasting();

        if (!success)
        {
            // 暴発判定（成功率が50%以下の場合は暴発の可能性）
            bool backfire = result.SuccessRate < 0.5 && random.NextDouble() < 0.3;
            if (backfire)
            {
                return SpellCastResult.Backfire(
                    $"「{formattedIncantation}」の詠唱に失敗し、魔力が暴走した！",
                    result.MpCost,
                    result.TurnCost,
                    (int)(result.MpCost * 0.5));
            }
            return SpellCastResult.Failed(
                $"「{formattedIncantation}」の詠唱に失敗した",
                result.MpCost,
                result.TurnCost);
        }

        return SpellCastResult.Success(
            $"「{formattedIncantation}」{result.GetDescription()}！",
            result.MpCost,
            result.TurnCost,
            result.PowerMultiplier,
            result.EffectWord,
            result.TargetWord,
            result.ElementWord,
            result.RangeWord,
            result.DurationWord);
    }
}

/// <summary>
/// 詠唱プレビュー
/// </summary>
public record SpellPreview(
    bool IsValid,
    string Description,
    int MpCost,
    int TurnCost,
    double SuccessRate,
    string FormattedIncantation);

/// <summary>
/// 詠唱結果
/// </summary>
public class SpellCastResult
{
    public bool IsSuccess { get; init; }
    public bool IsBackfire { get; init; }
    public string Message { get; init; } = "";
    public int MpCost { get; init; }
    public int TurnCost { get; init; }
    public float PowerMultiplier { get; init; } = 1.0f;
    public int BackfireDamage { get; init; }

    public RuneWord? EffectWord { get; init; }
    public RuneWord? TargetWord { get; init; }
    public RuneWord? ElementWord { get; init; }
    public RuneWord? RangeWord { get; init; }
    public RuneWord? DurationWord { get; init; }

    public static SpellCastResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };

    public static SpellCastResult Failed(string message, int mpCost, int turnCost) => new()
    {
        IsSuccess = false,
        Message = message,
        MpCost = mpCost,
        TurnCost = turnCost
    };

    public static SpellCastResult Backfire(string message, int mpCost, int turnCost, int backfireDamage) => new()
    {
        IsSuccess = false,
        IsBackfire = true,
        Message = message,
        MpCost = mpCost,
        TurnCost = turnCost,
        BackfireDamage = backfireDamage
    };

    public static SpellCastResult Success(string message, int mpCost, int turnCost,
        float powerMultiplier, RuneWord? effect, RuneWord? target, RuneWord? element,
        RuneWord? range, RuneWord? duration) => new()
    {
        IsSuccess = true,
        Message = message,
        MpCost = mpCost,
        TurnCost = turnCost,
        PowerMultiplier = powerMultiplier,
        EffectWord = effect,
        TargetWord = target,
        ElementWord = element,
        RangeWord = range,
        DurationWord = duration
    };
}

/// <summary>
/// 語彙入手システム - ルーン碑文・古代の書からの語彙習得
/// </summary>
public static class VocabularyAcquisitionSystem
{
    /// <summary>ルーン碑文から語彙を学ぶ</summary>
    public static VocabularyLearnResult LearnFromRuneStone(Player player, string wordId)
    {
        var word = RuneWordDatabase.GetById(wordId);
        if (word == null)
            return new VocabularyLearnResult(false, "不明なルーン語");

        if (player.LearnedWords.ContainsKey(wordId))
        {
            // 既知の語 → 理解度上昇
            int currentMastery = player.GetWordMastery(wordId);
            int gain = Math.Max(1, 10 - word.Difficulty);
            player.ImproveWordMastery(wordId, gain);
            return new VocabularyLearnResult(true,
                $"ルーン碑文から「{word.Meaning}（{word.OldNorse}）」の理解を深めた（理解度: {Math.Min(100, currentMastery + gain)}）",
                wordId, gain, false);
        }

        // 新規習得
        player.LearnWord(wordId);
        return new VocabularyLearnResult(true,
            $"ルーン碑文から「{word.Meaning}（{word.OldNorse}）」を習得した！",
            wordId, GameConstants.InitialWordMastery, true);
    }

    /// <summary>古代の書から複数の語彙を学ぶ</summary>
    public static List<VocabularyLearnResult> LearnFromAncientBook(Player player, string[] wordIds)
    {
        var results = new List<VocabularyLearnResult>();
        foreach (var wordId in wordIds)
        {
            results.Add(LearnFromRuneStone(player, wordId));
        }
        return results;
    }

    /// <summary>ランダムな未習得語を学ぶ（難易度範囲指定）</summary>
    public static VocabularyLearnResult LearnRandomWord(Player player, int maxDifficulty, IRandomProvider random)
    {
        var candidates = RuneWordDatabase.GetAll()
            .Where(w => w.Difficulty <= maxDifficulty && !player.LearnedWords.ContainsKey(w.Id))
            .ToList();

        if (candidates.Count == 0)
            return new VocabularyLearnResult(false, "学べる新しい語彙がない");

        var word = candidates[random.Next(candidates.Count)];
        player.LearnWord(word.Id);
        return new VocabularyLearnResult(true,
            $"「{word.Meaning}（{word.OldNorse}）」を習得した！",
            word.Id, GameConstants.InitialWordMastery, true);
    }
}

/// <summary>
/// 語彙習得結果
/// </summary>
public record VocabularyLearnResult(
    bool Success,
    string Message,
    string? WordId = null,
    int MasteryGain = 0,
    bool IsNewWord = false);

/// <summary>
/// 魔法効果解決システム - 詠唱結果からゲーム効果を生成
/// </summary>
public static class SpellEffectResolver
{
    /// <summary>魔法効果を解決</summary>
    public static SpellEffect Resolve(SpellCastResult castResult)
    {
        if (!castResult.IsSuccess || castResult.EffectWord == null)
            return SpellEffect.None;

        var effectWord = castResult.EffectWord;
        var targetWord = castResult.TargetWord;
        var elementWord = castResult.ElementWord;

        // 基本ダメージ/回復量
        int basePower = effectWord.BaseMpCost * 3;
        int finalPower = (int)(basePower * castResult.PowerMultiplier);

        // 属性決定
        Element element = DetermineElement(effectWord, elementWord);

        // ダメージタイプ決定
        DamageType damageType = element == Element.None ? DamageType.Magical : DamageType.Elemental;

        // 効果タイプ決定
        SpellEffectType effectType = CategorizeEffect(effectWord.Id);

        // ターゲットタイプ決定
        SpellTargetType targetType = DetermineTarget(targetWord, castResult.RangeWord);

        // 範囲決定
        int range = DetermineRange(castResult.RangeWord);

        // 持続時間決定
        int duration = DetermineDuration(castResult.DurationWord);

        return new SpellEffect(
            effectType, targetType, element, damageType,
            finalPower, range, duration, castResult.PowerMultiplier);
    }

    private static Element DetermineElement(RuneWord effectWord, RuneWord? elementWord)
    {
        if (elementWord != null)
        {
            return elementWord.Id switch
            {
                "eldr" => Element.Fire,
                "vatn" => Element.Water,
                "iss" => Element.Ice,
                "thruma_elem" => Element.Lightning,
                "jord_elem" => Element.Earth,
                "vindr" => Element.Wind,
                "ljos" => Element.Light,
                "myrkr" => Element.Dark,
                "helgr" => Element.Holy,
                "bolvadr" => Element.Curse,
                _ => Element.None
            };
        }

        // 効果語による暗黙の属性
        return effectWord.Id switch
        {
            "brenna" => Element.Fire,
            "frysta" => Element.Ice,
            "thruma" => Element.Lightning,
            "graeda" => Element.Light,
            "blessa" => Element.Holy,
            "eyda" => Element.Curse,
            "granda" => Element.Poison,
            _ => Element.None
        };
    }

    private static SpellEffectType CategorizeEffect(string effectWordId) => effectWordId switch
    {
        "brenna" or "frysta" or "thruma" or "brjota" or "snida" or "stinga" or "springa" or "tortima" or "eyda" or "granda"
            => SpellEffectType.Damage,
        "graeda" => SpellEffectType.Heal,
        "hreinsa" => SpellEffectType.Purify,
        "verja" or "styrkja" => SpellEffectType.Buff,
        "hrada" => SpellEffectType.Speed,
        "hylja" => SpellEffectType.Stealth,
        "blessa" => SpellEffectType.Blessing,
        "binda" or "sofa" or "villa" or "hraeda" or "styra" or "loka" => SpellEffectType.Control,
        "kalla" => SpellEffectType.Summon,
        "senda" => SpellEffectType.Teleport,
        "sja" or "vita" => SpellEffectType.Detect,
        "afrita" => SpellEffectType.Copy,
        "snua" => SpellEffectType.Reverse,
        "banna" => SpellEffectType.Seal,
        "opna" => SpellEffectType.Unlock,
        "vekja" => SpellEffectType.Resurrect,
        _ => SpellEffectType.Damage
    };

    private static SpellTargetType DetermineTarget(RuneWord? targetWord, RuneWord? rangeWord)
    {
        if (targetWord != null)
        {
            return targetWord.Id switch
            {
                "sjalfr" => SpellTargetType.Self,
                "fjandi" => SpellTargetType.SingleEnemy,
                "ovinir" => SpellTargetType.AllEnemies,
                "vinir" => SpellTargetType.AllAllies,
                "allir" => SpellTargetType.All,
                "hlutr" => SpellTargetType.Object,
                "jord" => SpellTargetType.Ground,
                _ => SpellTargetType.SingleEnemy
            };
        }

        // 対象語なし → 前方1マス
        return SpellTargetType.Forward;
    }

    private static int DetermineRange(RuneWord? rangeWord)
    {
        if (rangeWord == null) return 1;
        return rangeWord.Id switch
        {
            "einn" => 1,
            "beinn" => 5,
            "hringr" => 3,
            "vidr" => 5,
            "heimr" => 99,
            _ => 1
        };
    }

    private static int DetermineDuration(RuneWord? durationWord)
    {
        if (durationWord == null) return 0;
        return durationWord.Id switch
        {
            "augnablik" => 0,
            "stund" => 10,
            "langr" => 30,
            "eilifr" => 100,
            "sidar" => 5,
            "endalauss" => 999,
            _ => 0
        };
    }
}

/// <summary>
/// 魔法効果タイプ
/// </summary>
public enum SpellEffectType
{
    Damage,
    Heal,
    Purify,
    Buff,
    Speed,
    Stealth,
    Blessing,
    Control,
    Summon,
    Teleport,
    Detect,
    Copy,
    Reverse,
    Seal,
    Unlock,
    Resurrect
}

/// <summary>
/// 魔法ターゲットタイプ
/// </summary>
public enum SpellTargetType
{
    Self,
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    AllAllies,
    All,
    Object,
    Ground,
    Forward
}

/// <summary>
/// 魔法効果データ
/// </summary>
public record SpellEffect(
    SpellEffectType Type,
    SpellTargetType TargetType,
    Element Element,
    DamageType DamageType,
    int Power,
    int Range,
    int Duration,
    float PowerMultiplier)
{
    public static SpellEffect None => new(SpellEffectType.Damage, SpellTargetType.Forward, Element.None, DamageType.Magical, 0, 0, 0, 0);
    public bool IsNone => Power == 0 && Range == 0;
}
