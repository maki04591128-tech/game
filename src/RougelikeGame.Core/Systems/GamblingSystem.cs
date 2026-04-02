namespace RougelikeGame.Core.Systems;

/// <summary>
/// 賭博・ギャンブルシステム - ミニゲーム基盤
/// </summary>
public static class GamblingSystem
{
    /// <summary>サイコロの結果を判定（大小方式: 4-6=大、1-3=小）</summary>
    public static bool JudgeDice(int playerGuess, int diceResult)
    {
        // playerGuess >= 4 なら「大」、それ以外なら「小」を選択
        bool guessedBig = playerGuess >= 4;
        bool resultIsBig = diceResult >= 4;
        return guessedBig == resultIsBig;
    }

    /// <summary>丁半の結果を判定</summary>
    public static bool JudgeChoHan(bool playerChoseCho, int dice1, int dice2)
    {
        bool isCho = (dice1 + dice2) % 2 == 0;
        return playerChoseCho == isCho;
    }

    /// <summary>カード（ハイ&amp;ロー）の結果を判定</summary>
    public static bool JudgeHighLow(bool playerChoseHigh, int currentCard, int nextCard)
    {
        if (currentCard == nextCard) return false; // 引き分けは負け
        return playerChoseHigh ? nextCard > currentCard : nextCard < currentCard;
    }

    /// <summary>配当倍率を取得</summary>
    public static float GetPayoutMultiplier(GamblingGameType gameType) => gameType switch
    {
        GamblingGameType.Dice => 1.9f,    // 大小(50%)×1.9倍 = 期待値0.95（胴元有利5%）
        GamblingGameType.ChoHan => 1.9f,  // 丁半(50%)×1.9倍 = 期待値0.95
        GamblingGameType.Card => 1.9f,    // ハイロー(~46%)×1.9倍 = 期待値0.87
        _ => 1.0f
    };

    /// <summary>LUKによるボーナス判定（上限0.05 = 最大5%）</summary>
    public static float GetLuckBonus(int luck)
    {
        return Math.Min(luck * 0.005f, 0.05f); // LUK1あたり0.5%、上限5%
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
