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

    /// <summary>期待リターンを計算</summary>
    public static float GetExpectedReturn(InvestmentType type, int amount) => type switch
    {
        InvestmentType.Shop => amount * 1.2f,
        InvestmentType.AdventurerParty => amount * 1.8f,
        InvestmentType.Business => amount * 1.5f,
        _ => amount * 1.0f
    };

    /// <summary>成功確率を取得</summary>
    public static float GetSuccessRate(InvestmentType type) => type switch
    {
        InvestmentType.Shop => 0.8f,
        InvestmentType.AdventurerParty => 0.5f,
        InvestmentType.Business => 0.65f,
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
}
