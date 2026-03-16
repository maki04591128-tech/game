namespace RougelikeGame.Core.Systems;

/// <summary>
/// 賭博・ギャンブルシステム - ミニゲーム基盤
/// </summary>
public static class GamblingSystem
{
    /// <summary>サイコロの結果を判定</summary>
    public static bool JudgeDice(int playerGuess, int diceResult)
    {
        return playerGuess == diceResult;
    }

    /// <summary>丁半の結果を判定</summary>
    public static bool JudgeChoHan(bool playerChoseCho, int dice1, int dice2)
    {
        bool isCho = (dice1 + dice2) % 2 == 0;
        return playerChoseCho == isCho;
    }

    /// <summary>カード（ハイ&ロー）の結果を判定</summary>
    public static bool JudgeHighLow(bool playerChoseHigh, int currentCard, int nextCard)
    {
        if (currentCard == nextCard) return false; // 引き分けは負け
        return playerChoseHigh ? nextCard > currentCard : nextCard < currentCard;
    }

    /// <summary>配当倍率を取得</summary>
    public static float GetPayoutMultiplier(GamblingGameType gameType) => gameType switch
    {
        GamblingGameType.Dice => 6.0f,
        GamblingGameType.ChoHan => 2.0f,
        GamblingGameType.Card => 2.0f,
        _ => 1.0f
    };

    /// <summary>LUKによるボーナス判定</summary>
    public static float GetLuckBonus(int luck)
    {
        return luck * 0.01f; // LUK1あたり1%の追加勝率
    }

    /// <summary>ゲーム種別名を取得</summary>
    public static string GetGameName(GamblingGameType type) => type switch
    {
        GamblingGameType.Dice => "サイコロ",
        GamblingGameType.ChoHan => "丁半",
        GamblingGameType.Card => "ハイ＆ロー",
        _ => "不明"
    };

    /// <summary>最低賭金を取得</summary>
    public static int GetMinimumBet(GamblingGameType type) => type switch
    {
        GamblingGameType.Dice => 50,
        GamblingGameType.ChoHan => 10,
        GamblingGameType.Card => 20,
        _ => 10
    };

    /// <summary>賭博中毒チェック（正気度連動）</summary>
    public static bool CheckAddiction(int consecutiveGambles, int sanity)
    {
        float risk = consecutiveGambles * 0.05f - sanity * 0.001f;
        return risk > 0.3f;
    }
}
