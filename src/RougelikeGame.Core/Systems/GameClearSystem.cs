namespace RougelikeGame.Core.Systems;

/// <summary>
/// ゲームクリアシステム - クリア条件判定と結果管理
/// </summary>
public static class GameClearSystem
{
    /// <summary>クリア結果</summary>
    public record ClearResult(
        string BackgroundName,
        int TotalTurns,
        int TotalDeaths,
        int HighestLevel,
        string ClearCondition,
        bool IsFirstClear
    );

    /// <summary>素性別クリア条件を取得</summary>
    public static string GetClearCondition(Background background) => background switch
    {
        Background.Noble => "王都の復興を成し遂げる",
        Background.Merchant => "莫大な富を築く",
        Background.Criminal => "全ての罪を贖う",
        Background.Scholar => "究極の知識を得る",
        Background.Soldier => "魔王を討伐する",
        Background.Peasant => "豊かな大地を取り戻す",
        Background.Priest => "聖地を巡礼する",
        Background.Penitent => "全ての罪を贖い救済を得る",
        Background.Wanderer => "真の故郷を見つける",
        Background.Adventurer => "伝説の冒険者となる",
        _ => "冒険を完遂する"
    };

    /// <summary>クリアランクを判定</summary>
    public static string GetClearRank(int totalTurns, int totalDeaths) => (totalTurns, totalDeaths) switch
    {
        ( < 5000, 0) => "S",
        ( < 10000, <= 3) => "A",
        ( < 20000, <= 10) => "B",
        ( < 50000, _) => "C",
        _ => "D"
    };

    /// <summary>クリアメッセージを取得</summary>
    public static string GetClearMessage(string backgroundName, string rank)
    {
        return $"【{backgroundName}】としての冒険を完遂した！ ランク: {rank}";
    }

    /// <summary>NG+解放条件を満たしたか判定</summary>
    public static bool UnlocksNewGamePlus(string rank)
    {
        return rank is "S" or "A" or "B";
    }
}
