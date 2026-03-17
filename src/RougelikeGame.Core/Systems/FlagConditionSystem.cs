namespace RougelikeGame.Core.Systems;

/// <summary>
/// フラグ条件の1つの節
/// </summary>
public record FlagCondition(
    FlagConditionType Type,
    string Key,
    string Operator,   // ">=", "<=", "==", "!="
    string Value
);

/// <summary>
/// 複合条件（AND/OR）
/// </summary>
public record CompoundCondition(
    IReadOnlyList<FlagCondition> Conditions,
    bool IsAnd  // true=全てAND, false=いずれかOR
);

/// <summary>
/// フラグ選択肢変動システム - 複数条件のAND/OR判定でダイアログ選択肢を制御
/// </summary>
public static class FlagConditionSystem
{
    /// <summary>
    /// 条件文字列をパースして FlagCondition に変換
    /// </summary>
    public static FlagCondition? ParseCondition(string conditionText)
    {
        conditionText = conditionText.Trim();

        // has:flag_name
        if (conditionText.StartsWith("has:"))
        {
            string flagName = conditionText[4..].Trim();
            return new FlagCondition(FlagConditionType.HasFlag, flagName, "==", "true");
        }

        // stat:STR >= 20
        if (conditionText.StartsWith("stat:"))
        {
            var parts = conditionText[5..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
                return new FlagCondition(FlagConditionType.StatCompare, parts[0], parts[1], parts[2]);
        }

        // race:Elf
        if (conditionText.StartsWith("race:"))
        {
            string race = conditionText[5..].Trim();
            return new FlagCondition(FlagConditionType.RaceCheck, "Race", "==", race);
        }

        // religion:LightTemple
        if (conditionText.StartsWith("religion:"))
        {
            string religion = conditionText[9..].Trim();
            return new FlagCondition(FlagConditionType.ReligionCheck, "Religion", "==", religion);
        }

        // mastery:sword >= 10
        if (conditionText.StartsWith("mastery:"))
        {
            var parts = conditionText[8..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
                return new FlagCondition(FlagConditionType.MasteryCheck, parts[0], parts[1], parts[2]);
        }

        // karma >= 50
        if (conditionText.StartsWith("karma"))
        {
            var parts = conditionText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
                return new FlagCondition(FlagConditionType.ValueCompare, "karma", parts[1], parts[2]);
        }

        return null;
    }

    /// <summary>
    /// 複合条件文字列をパース (AND / OR で連結)
    /// </summary>
    public static CompoundCondition? ParseCompound(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        bool isAnd = text.Contains(" AND ");
        string separator = isAnd ? " AND " : " OR ";
        var parts = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        var conditions = new List<FlagCondition>();
        foreach (var part in parts)
        {
            var cond = ParseCondition(part.Trim());
            if (cond != null) conditions.Add(cond);
        }

        if (conditions.Count == 0) return null;
        return new CompoundCondition(conditions, isAnd);
    }

    /// <summary>
    /// 単一条件を評価
    /// </summary>
    public static bool EvaluateCondition(FlagCondition condition, IReadOnlyDictionary<string, string> context)
    {
        switch (condition.Type)
        {
            case FlagConditionType.HasFlag:
                return context.ContainsKey(condition.Key);

            case FlagConditionType.ValueCompare:
            case FlagConditionType.StatCompare:
            case FlagConditionType.MasteryCheck:
                if (!context.TryGetValue(condition.Key, out string? actualStr))
                    return false;
                if (!int.TryParse(actualStr, out int actual))
                    return false;
                if (!int.TryParse(condition.Value, out int target))
                    return false;
                return CompareValues(actual, condition.Operator, target);

            case FlagConditionType.RaceCheck:
            case FlagConditionType.ReligionCheck:
                if (!context.TryGetValue(condition.Key, out string? value))
                    return false;
                return condition.Operator == "==" 
                    ? value == condition.Value 
                    : value != condition.Value;

            case FlagConditionType.KarmaRankCheck:
                if (!context.TryGetValue("karma", out string? karmaStr))
                    return false;
                if (!int.TryParse(karmaStr, out int karma))
                    return false;
                if (!int.TryParse(condition.Value, out int threshold))
                    return false;
                return CompareValues(karma, condition.Operator, threshold);

            default:
                return false;
        }
    }

    /// <summary>
    /// 複合条件を評価
    /// </summary>
    public static bool EvaluateCompound(CompoundCondition compound, IReadOnlyDictionary<string, string> context)
    {
        if (compound.IsAnd)
            return compound.Conditions.All(c => EvaluateCondition(c, context));
        else
            return compound.Conditions.Any(c => EvaluateCondition(c, context));
    }

    private static bool CompareValues(int actual, string op, int target) => op switch
    {
        ">=" => actual >= target,
        "<=" => actual <= target,
        ">" => actual > target,
        "<" => actual < target,
        "==" => actual == target,
        "!=" => actual != target,
        _ => false
    };
}
