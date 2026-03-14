using RougelikeGame.Core;

namespace RougelikeGame.Data.MagicLanguage;

/// <summary>
/// 魔法（ガルドル）の計算と解析
/// </summary>
public class SpellParser
{
    /// <summary>
    /// 詠唱文を解析して魔法効果を計算
    /// </summary>
    public SpellResult Parse(string incantation, Dictionary<string, int> masteryLevels)
    {
        var wordIds = incantation.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (wordIds.Length == 0)
        {
            return SpellResult.Failure("詠唱文が空です");
        }

        if (wordIds.Length > GameConstants.MaxSpellWords)
        {
            return SpellResult.Failure($"詠唱文が長すぎます（最大{GameConstants.MaxSpellWords}語）");
        }

        var words = new List<RuneWord>();
        foreach (var wordId in wordIds)
        {
            var word = RuneWordDatabase.GetById(wordId);
            if (word == null)
            {
                return SpellResult.Failure($"未知のルーン語: {wordId}");
            }
            words.Add(word);
        }

        // 効果語が含まれているか確認
        if (!words.Any(w => w.Category == RuneWordCategory.Effect))
        {
            return SpellResult.Failure("効果語が含まれていません");
        }

        // 消費計算
        int baseMp = words.Sum(w => w.BaseMpCost);
        float mpMultiplier = words.Where(w => w.MpMultiplier != 1.0f)
                                   .Select(w => w.MpMultiplier)
                                   .Aggregate(1.0f, (a, b) => a * b);

        int baseTurn = words.Sum(w => w.BaseTurnCost);
        int turnAddition = words.Sum(w => w.TurnCostAddition);

        // 速度修飾の処理
        var speedModifiers = words.Where(w => w.Id is "skjotr" or "hradr" or "thegar").ToList();
        float speedReduction = 1.0f;
        foreach (var mod in speedModifiers)
        {
            speedReduction *= mod.Id switch
            {
                "skjotr" => 0.7f,
                "hradr" => 0.5f,
                "thegar" => 0.0f,  // 最小値に設定
                _ => 1.0f
            };
        }

        // 威力計算
        float powerMultiplier = words.Where(w => w.PowerMultiplier != 1.0f)
                                      .Select(w => w.PowerMultiplier)
                                      .Aggregate(1.0f, (a, b) => a * b);

        // 理解度による成功率計算
        double successRate = CalculateSuccessRate(words, masteryLevels);

        // 最終値計算
        int finalMp = Math.Max(1, (int)Math.Ceiling(baseMp * mpMultiplier));
        int finalTurn = baseTurn + turnAddition;
        if (speedModifiers.Any(m => m.Id == "thegar"))
        {
            finalTurn = TurnCosts.SpellMinimum;
        }
        else
        {
            finalTurn = Math.Max(TurnCosts.SpellMinimum, (int)Math.Ceiling(finalTurn * speedReduction));
        }

        return new SpellResult
        {
            IsSuccess = true,
            Words = words,
            MpCost = finalMp,
            TurnCost = finalTurn,
            PowerMultiplier = powerMultiplier,
            SuccessRate = successRate,
            EffectWord = words.First(w => w.Category == RuneWordCategory.Effect),
            TargetWord = words.FirstOrDefault(w => w.Category == RuneWordCategory.Target),
            ElementWord = words.FirstOrDefault(w => w.Category == RuneWordCategory.Element),
            RangeWord = words.FirstOrDefault(w => w.Category == RuneWordCategory.Range),
            DurationWord = words.FirstOrDefault(w => w.Category == RuneWordCategory.Duration)
        };
    }

    private double CalculateSuccessRate(List<RuneWord> words, Dictionary<string, int> masteryLevels)
    {
        double rate = 1.0;

        // 語数ペナルティ
        rate -= words.Count switch
        {
            >= 7 => 0.25,
            >= 6 => 0.15,
            >= 5 => 0.08,
            >= 4 => 0.03,
            _ => 0.0
        };

        // 理解度ペナルティ
        foreach (var word in words)
        {
            int mastery = masteryLevels.GetValueOrDefault(word.Id, 0);
            rate -= mastery switch
            {
                >= 100 => -0.05,  // ボーナス
                >= 80 => 0.0,
                >= 60 => 0.02,
                >= 40 => 0.05,
                >= 20 => 0.10,
                _ => 0.20
            };
        }

        return Math.Clamp(rate, 0.05, 1.0);
    }

    /// <summary>
    /// 詠唱文をフォーマット（表示用）
    /// </summary>
    public string FormatIncantation(IEnumerable<RuneWord> words)
    {
        var formatted = words.Select(w => char.ToUpper(w.OldNorse[0]) + w.OldNorse[1..]);
        return string.Join(" ", formatted) + "!";
    }
}

/// <summary>
/// 魔法解析結果
/// </summary>
public class SpellResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public List<RuneWord> Words { get; init; } = new();

    public int MpCost { get; init; }
    public int TurnCost { get; init; }
    public float PowerMultiplier { get; init; } = 1.0f;
    public double SuccessRate { get; init; } = 1.0;

    public RuneWord? EffectWord { get; init; }
    public RuneWord? TargetWord { get; init; }
    public RuneWord? ElementWord { get; init; }
    public RuneWord? RangeWord { get; init; }
    public RuneWord? DurationWord { get; init; }

    public static SpellResult Failure(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };

    /// <summary>
    /// 日本語での説明を生成
    /// </summary>
    public string GetDescription()
    {
        if (!IsSuccess) return ErrorMessage ?? "解析失敗";

        var parts = new List<string>();

        if (RangeWord != null) parts.Add(RangeWord.Meaning);
        if (TargetWord != null) parts.Add(TargetWord.Meaning + "を");
        if (ElementWord != null) parts.Add(ElementWord.Meaning + "の力で");

        var modifiers = Words.Where(w => w.Category == RuneWordCategory.Modifier).ToList();
        foreach (var mod in modifiers)
        {
            if (mod.PowerMultiplier > 1.0f)
                parts.Add(mod.Meaning);
        }

        if (EffectWord != null) parts.Add(EffectWord.Meaning);

        if (DurationWord != null && DurationWord.Id != "augnablik")
            parts.Add($"（{DurationWord.Meaning}）");

        return string.Join("", parts);
    }
}
