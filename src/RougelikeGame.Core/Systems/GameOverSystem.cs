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
}
