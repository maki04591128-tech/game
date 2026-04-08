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
            _ => new GameOverActionResult(false, false, false, "無効な選択です")
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
        DeathCause.Suicide => "自らの意志で命を絶った",
        DeathCause.SanityDeath => "正気を完全に失い倒れた",
        DeathCause.Fall => "落下",
        _ => "不明な原因"
    };

    /// <summary>
    /// α.25: 死因別のゲームオーバーフレーバーテキストを取得する（没入感のある死亡描写）
    /// </summary>
    public static string GetDeathFlavorText(DeathCause cause, int deathCount = 0) => cause switch
    {
        DeathCause.Combat =>
            "血が滲み出るのを感じながら、意識が遠のいていく。" +
            "最後に見えたのは、倒れかけた敵の影だったか、仲間の顔だったか……" +
            "膝から崩れ落ち、あなたは静かに目を閉じた。",

        DeathCause.Boss =>
            "その一撃は圧倒的だった。" +
            "全力で挑んでなお、届かなかった……。" +
            "石畳に倒れ伏したあなたの耳に、ボスの笑い声か、それとも風の音が聞こえた気がした。" +
            "完全な敗北だ。だが、次があるなら——",

        DeathCause.Starvation =>
            "体がいよいよ動かなくなった。" +
            "最後の力を振り絞って一歩踏み出そうとしたが、足が言うことを聞かない。" +
            "飢えの痛みさえ、もう感じなかった……静かな終幕だった。",

        DeathCause.Dehydration =>
            "喉の渇きは限界を超えていた。" +
            "視界が霞み、幻が見え始めた頃には、もう立っていることすらできなかった。" +
            "砂漠の蜃気楼を夢見ながら、あなたは力尽きた。",

        DeathCause.Trap =>
            "油断だった。一瞬の注意散漫が命取りになるとは——。" +
            "罠が発動した音が反響する中、あなたの意識は急速に失われていった。" +
            "この経験は……次に活かされるはずだ。",

        DeathCause.Poison =>
            "手足の感覚が消え、視界がぐにゃりと歪んだ。" +
            "毒が全身に回り切った瞬間、抵抗する意志さえ溶けていった。" +
            "最期は苦しみもなく——静かに、意識は消えていった。",

        DeathCause.TimeLimit =>
            "時間が来た。逃げ切れなかった。" +
            "背後から迫りくるものを感じながら、あなたは全力で走り続けたが……" +
            "力尽きたその瞬間、全てが暗闇に覆われた。",

        DeathCause.Curse =>
            "呪いは最初から全身に絡みついていた。" +
            "最後の瞬間、それが何の呪いだったか、誰が何のために——そんな問いが頭をよぎったが、" +
            "答えを得る前に意識は消えた。呪いの囁きだけが、残響として消えていった。",

        DeathCause.Suicide =>
            "これが自分の選んだ結末だ。" +
            "誰かのために、何かを守るために、あるいは全てに疲れ果てて——。" +
            "その理由はあなただけが知っている。炎は静かに、燃え尽きていった。",

        DeathCause.SanityDeath =>
            "最後は、自分が誰なのかも分からなくなっていた。" +
            "声が聞こえる。幻が見える。現実と夢の境界が溶けていく。" +
            "正気の灯火が完全に消えた時、あなたという人間も、静かに終わりを迎えた。",

        DeathCause.Fall =>
            "足が滑った——その一瞬の感覚が、最後の記憶となった。" +
            "暗闇の中を落ちながら、遠ざかっていく光の点を見ていた。" +
            "地に叩きつけられる衝撃はほとんど感じなかった。速すぎたから。",

        _ =>
            "何が起きたのか、自分でもよく分からなかった。" +
            "気づいた時には、全てが終わっていた。" +
            "次の世界では、もう少し注意深くあろう……。"
    };

    /// <summary>
    /// α.25: 死に戻り演出テキストを取得する（周回数に応じて変化）
    /// </summary>
    public static string GetRebornText(int loopCount, int sanity) => loopCount switch
    {
        0 =>
            "——暗闇の中に、声が聞こえた。\n" +
            "「まだ終わりじゃない。戻っておいで」\n" +
            "意識が、引き戻される——",

        1 =>
            "また、この感覚だ。\n" +
            "死の淵から引き戻される、あの奇妙な温もり。\n" +
            "前に死んだのは……そうだ、あの場所で。次は気をつけよう。",

        2 =>
            "二度目。今度は何が悪かったのか、すでに分かっている。\n" +
            "死に戻りの感覚が、少しずつ「慣れた」ものになっていく。それが……怖い。",

        3 =>
            "三度目の目覚め。体は痛い。正気度も削れていく。\n" +
            "「案内人」の声は今回は聞こえなかった。気のせいか、それとも——",

        4 =>
            "何度も死んで、何度も戻ってきた。\n" +
            "もはや死への恐怖は薄れている。それが果たして良いことなのかどうか……。\n" +
            "精神に靄がかかっている。だが、先に進むしかない。",

        _ when sanity <= 10 =>
            "……戻れた。ギリギリだった。\n" +
            "正気度が底をつきかけている。次に死んだら、本当に戻れないかもしれない。\n" +
            "手が震えている。それでも——進む。",

        _ =>
            $"これで{loopCount}回目の目覚めだ。\n" +
            "もはや数えることすら無意味に思えてくるが、その数字が自分の失敗の証明でもある。\n" +
            "今度こそ——今度こそ、帰り着く。",
    };

    /// <summary>
    /// α.25: 正気度が0でゲームオーバーになった場合のテキスト
    /// </summary>
    public static string GetSanityGameOverText() =>
        "正気の糸が、完全に切れた。\n\n" +
        "何度も死に、何度も戻るうちに——人としての感覚が消えていった。\n" +
        "名前も、目的も、誰かを守りたいという想いも、\n" +
        "全てが霧の中に溶けていく。\n\n" +
        "あなたはもう、戻れない。\n\n" +
        "— GAME OVER —";

    /// <summary>
    /// α.25: 真のゲームオーバー（正気度0・死亡）テキスト
    /// </summary>
    public static string GetTrueGameOverText(int totalDeaths) =>
        $"冒険の記録 — 総死亡回数：{totalDeaths}回\n\n" +
        "あなたは最後まで戦い続けた。\n" +
        "結末は敗北だったかもしれないが、その旅路は確かにあなたのものだった。\n\n" +
        "次の冒険者も、同じ苦難に立ち向かうだろう。\n" +
        "あなたが残した軌跡は、世界のどこかに刻まれている。\n\n" +
        "— THE END —";
}
