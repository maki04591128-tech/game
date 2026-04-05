namespace RougelikeGame.Core.Systems;

/// <summary>
/// 投資・出資システム - NPCショップ/冒険者パーティへの出資
/// </summary>
public class InvestmentSystem
{
    /// <summary>投資記録</summary>
    public record InvestmentRecord(
        InvestmentType Type,
        string TargetName,
        int Amount,
        float ExpectedReturn,
        int InvestedTurn,
        bool IsCompleted
    );

    private readonly List<InvestmentRecord> _investments = new();

    /// <summary>全投資記録</summary>
    public IReadOnlyList<InvestmentRecord> Investments => _investments;

    /// <summary>投資を行う</summary>
    public bool Invest(InvestmentType type, string target, int amount, int currentTurn)
    {
        float expectedReturn = GetExpectedReturn(type, amount);
        _investments.Add(new InvestmentRecord(type, target, amount, expectedReturn, currentTurn, false));
        return true;
    }

    /// <summary>期待リターンを計算（成功時の返却額）</summary>
    public static float GetExpectedReturn(InvestmentType type, int amount) => type switch
    {
        InvestmentType.Shop => amount * 1.3f,           // 成功時30%利益
        InvestmentType.AdventurerParty => amount * 2.0f, // 成功時100%利益（高リスク）
        InvestmentType.Business => amount * 1.6f,       // 成功時60%利益
        _ => amount * 1.0f
    };

    /// <summary>成功確率を取得（失敗時は投資額を失う）</summary>
    public static float GetSuccessRate(InvestmentType type) => type switch
    {
        InvestmentType.Shop => 0.6f,            // 60%（期待値: 0.78 — 胴元有利）
        InvestmentType.AdventurerParty => 0.3f,  // 30%（期待値: 0.6 — 高リスク高リターン）
        InvestmentType.Business => 0.45f,        // 45%（期待値: 0.72 — 中リスク）
        _ => 0.5f
    };

    /// <summary>投資種別名を取得</summary>
    public static string GetTypeName(InvestmentType type) => type switch
    {
        InvestmentType.Shop => "ショップ投資",
        InvestmentType.AdventurerParty => "冒険者パーティ出資",
        InvestmentType.Business => "事業出資",
        _ => "不明"
    };

    /// <summary>総投資額を取得</summary>
    public int GetTotalInvested() => _investments.Sum(i => i.Amount);

    /// <summary>アクティブな投資数を取得</summary>
    public int GetActiveInvestments() => _investments.Count(i => !i.IsCompleted);

    /// <summary>AT-1: 成熟した投資の回収を試みる</summary>
    /// <param name="currentTurn">現在のターン数</param>
    /// <param name="maturityTurns">投資が成熟するまでのターン数（デフォルト1000）</param>
    /// <returns>回収結果のリスト（投資名、成否、金額）</returns>
    public List<(string TargetName, bool Success, int ReturnAmount)> TryCollectReturns(int currentTurn, Random random, int maturityTurns = 1000)
    {
        var results = new List<(string, bool, int)>();

        for (int i = 0; i < _investments.Count; i++)
        {
            var inv = _investments[i];
            if (inv.IsCompleted) continue;
            if (currentTurn - inv.InvestedTurn < maturityTurns) continue;

            float successRate = GetSuccessRate(inv.Type);
            bool success = random.NextDouble() < successRate;
            int returnAmount = success ? (int)inv.ExpectedReturn : 0;

            _investments[i] = inv with { IsCompleted = true };
            results.Add((inv.TargetName, success, returnAmount));
        }

        return results;
    }

    /// <summary>
    /// 全投資を消去する（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、投資記録は全て消失する。
    /// </summary>
    public void Reset()
    {
        _investments.Clear();
    }

    /// <summary>BQ-9: セーブデータから投資記録を復元</summary>
    public void RestoreInvestments(IEnumerable<InvestmentRecord> records)
    {
        foreach (var r in records)
            _investments.Add(r);
    }
}