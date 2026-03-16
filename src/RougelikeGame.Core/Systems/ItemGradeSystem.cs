using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// アイテム等級情報
/// </summary>
public record ItemGradeInfo(
    ItemGrade Grade,
    string JapaneseName,
    float StatMultiplier,
    float PriceMultiplier,
    float DropRate
);

/// <summary>
/// アイテム等級システム - 品質による性能・価格・出現率を管理
/// </summary>
public static class ItemGradeSystem
{
    private static readonly Dictionary<ItemGrade, ItemGradeInfo> GradeInfoMap = new()
    {
        [ItemGrade.Crude] = new ItemGradeInfo(ItemGrade.Crude, "粗悪品", 0.7f, 0.4f, 0.20f),
        [ItemGrade.Cheap] = new ItemGradeInfo(ItemGrade.Cheap, "廉価品", 0.85f, 0.7f, 0.25f),
        [ItemGrade.Standard] = new ItemGradeInfo(ItemGrade.Standard, "標準品", 1.0f, 1.0f, 0.30f),
        [ItemGrade.Fine] = new ItemGradeInfo(ItemGrade.Fine, "良品", 1.15f, 1.5f, 0.15f),
        [ItemGrade.Superior] = new ItemGradeInfo(ItemGrade.Superior, "上質品", 1.3f, 2.5f, 0.08f),
        [ItemGrade.Masterwork] = new ItemGradeInfo(ItemGrade.Masterwork, "傑作品", 1.5f, 5.0f, 0.02f),
    };

    /// <summary>等級接頭辞マップ</summary>
    private static readonly Dictionary<ItemGrade, string> GradePrefixMap = new()
    {
        [ItemGrade.Crude] = "粗悪な",
        [ItemGrade.Cheap] = "廉価な",
        [ItemGrade.Standard] = "",
        [ItemGrade.Fine] = "良質な",
        [ItemGrade.Superior] = "上質な",
        [ItemGrade.Masterwork] = "傑作の",
    };

    /// <summary>
    /// 等級情報を取得
    /// </summary>
    public static ItemGradeInfo GetGradeInfo(ItemGrade grade)
    {
        return GradeInfoMap.TryGetValue(grade, out var info)
            ? info
            : GradeInfoMap[ItemGrade.Standard];
    }

    /// <summary>
    /// ステータス係数を取得
    /// </summary>
    public static float GetStatMultiplier(ItemGrade grade)
    {
        return GetGradeInfo(grade).StatMultiplier;
    }

    /// <summary>
    /// 価格係数を取得
    /// </summary>
    public static float GetPriceMultiplier(ItemGrade grade)
    {
        return GetGradeInfo(grade).PriceMultiplier;
    }

    /// <summary>
    /// 日本語接頭辞を取得（表示名用）
    /// </summary>
    public static string GetGradeDisplayPrefix(ItemGrade grade)
    {
        return GradePrefixMap.TryGetValue(grade, out var prefix)
            ? prefix
            : "";
    }

    /// <summary>
    /// 各等級の出現率配列を取得（鍛冶レベルで上位確率上昇）
    /// </summary>
    public static Dictionary<ItemGrade, float> GetGradeDropRates(int smithingLevel = 0)
    {
        var rates = new Dictionary<ItemGrade, float>();

        foreach (var kvp in GradeInfoMap)
        {
            rates[kvp.Key] = kvp.Value.DropRate;
        }

        if (smithingLevel > 0)
        {
            // 鍛冶レベルに応じて上位品の確率を上昇、下位品の確率を低下
            float bonus = smithingLevel * 0.02f;
            float penalty = smithingLevel * 0.01f;

            // 下位品の確率を減少
            rates[ItemGrade.Crude] = Math.Max(0.01f, rates[ItemGrade.Crude] - penalty * 2);
            rates[ItemGrade.Cheap] = Math.Max(0.01f, rates[ItemGrade.Cheap] - penalty);

            // 上位品の確率を増加
            rates[ItemGrade.Fine] += bonus;
            rates[ItemGrade.Superior] += bonus * 0.5f;
            rates[ItemGrade.Masterwork] += bonus * 0.25f;

            // 正規化（合計を1.0にする）
            float total = rates.Values.Sum();
            foreach (var grade in rates.Keys.ToList())
            {
                rates[grade] /= total;
            }
        }

        return rates;
    }

    /// <summary>
    /// ランダムに等級を決定（鍛冶レベルで上位確率上昇）
    /// </summary>
    public static ItemGrade DetermineGrade(IRandomProvider random, int smithingLevel = 0)
    {
        var rates = GetGradeDropRates(smithingLevel);
        double roll = random.NextDouble();
        double cumulative = 0;

        foreach (var kvp in rates.OrderBy(r => (int)r.Key))
        {
            cumulative += kvp.Value;
            if (roll < cumulative)
                return kvp.Key;
        }

        return ItemGrade.Standard;
    }
}
