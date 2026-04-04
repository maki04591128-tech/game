namespace RougelikeGame.Core.Systems;

/// <summary>
/// ゲームオーバーシステム - 死亡後の選択肢管理
/// </summary>
public static class GameOverSystem
{
    /// <summary>ゲームオーバー時の選択肢</summary>
    public enum GameOverChoice
    {
        /// <summary>死に戻り（コンティニュー）</summary>
        Rebirth,
        /// <summary>タイトル画面に戻る</summary>
        ReturnToTitle,
        /// <summary>ゲーム終了</summary>
        Quit
    }

    /// <summary>選択肢テキストを取得</summary>
    public static string GetChoiceText(GameOverChoice choice) => choice switch
    {
        GameOverChoice.Rebirth => "死に戻る（正気度消費）",
        GameOverChoice.ReturnToTitle => "タイトル画面に戻る",
        GameOverChoice.Quit => "ゲームを終了する",
        _ => ""
    };

    /// <summary>死に戻りが可能か判定</summary>
    public static bool CanRebirth(int sanity)
    {
        return sanity > 0;
    }

    /// <summary>全選択肢を取得（状態に応じて利用可否付き）</summary>
    public static IReadOnlyList<(GameOverChoice Choice, bool Available)> GetAvailableChoices(int sanity)
    {
        return new List<(GameOverChoice, bool)>
        {
            (GameOverChoice.Rebirth, CanRebirth(sanity)),
            (GameOverChoice.ReturnToTitle, true),
            (GameOverChoice.Quit, true)
        };
    }

    /// <summary>ゲームオーバーメッセージを取得</summary>
    public static string GetGameOverMessage(string causeDetail)
    {
        return $"あなたは{causeDetail}により命を落とした…";
    }

    /// <summary>選択肢を実行した結果</summary>
    public record GameOverActionResult(
        bool ShouldReturnToTitle,
        bool ShouldQuitGame,
        bool ShouldRebirth,
        string Message
    );

    /// <summary>ゲームオーバー選択肢を処理</summary>
    public static GameOverActionResult ProcessChoice(GameOverChoice choice, int sanity)
    {
        return choice switch
        {
            GameOverChoice.Rebirth when CanRebirth(sanity) =>
                new GameOverActionResult(false, false, true, "死に戻りを実行..."),
            GameOverChoice.Rebirth =>
                new GameOverActionResult(false, false, false, "正気度が足りない..."),
            GameOverChoice.ReturnToTitle =>
                new GameOverActionResult(true, false, false, "タイトル画面に戻ります..."),
            GameOverChoice.Quit =>
                new GameOverActionResult(false, true, false, "ゲームを終了します..."),
            _ => new GameOverActionResult(false, false, false, "")
        };
    }

    /// <summary>死因の詳細説明を取得</summary>
    public static string GetDeathCauseDetail(DeathCause cause) => cause switch
    {
        DeathCause.Combat => "戦闘中の致命傷",
        DeathCause.Boss => "ボスとの激戦",
        DeathCause.Starvation => "飢餓",
        DeathCause.Dehydration => "脱水",  // DC-1: 渇き死
        DeathCause.Trap => "罠",
        DeathCause.Poison => "毒",
        DeathCause.TimeLimit => "時間切れ",
        DeathCause.Curse => "呪い",
        _ => "不明な原因"
    };
}
