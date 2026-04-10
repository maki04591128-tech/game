using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.27: カルマ変動時のNPC反応テキスト
/// α.28: カルマ条件の会話分岐テキスト
/// α.29: 闇市NPC会話テキスト
/// </summary>
public static class KarmaRelatedData
{
    // =========================================================
    // α.27: カルマ変動時のNPC反応テキスト
    // =========================================================

    /// <summary>
    /// カルマ段階に応じたNPC全般の反応テキストを取得する
    /// </summary>
    public static string GetKarmaReaction(string npcId, KarmaRank karmaRank) =>
        (npcId, karmaRank) switch
        {
            // レオン（案内人）
            ("npc_leon", KarmaRank.Saint) =>
                "レオン「その輝き……君の行いが世界に反響している。聖人の道は美しいが、重い」",
            ("npc_leon", KarmaRank.Virtuous) =>
                "レオン「良い顔をしている。善意が積み重なると、こういう顔になるんだ」",
            ("npc_leon", KarmaRank.Normal) =>
                "レオン「普通の旅人の顔だ。それが悪いことだとは、私は思わない」",
            ("npc_leon", KarmaRank.Neutral) =>
                "レオン「まだ定まっていないね。それはそれで、可能性がある」",
            ("npc_leon", KarmaRank.Rogue) =>
                "レオン「少し翳りが出てきた。道を選ぶのはきみだが——後悔のない選択を」",
            ("npc_leon", KarmaRank.Criminal) =>
                "レオン「……その顔は好きではないな。でも見捨てるつもりもない」",
            ("npc_leon", KarmaRank.Villain) =>
                "レオン「君が歩いた道に、傷ついた人がいる。それは変えられない事実だ。でも——まだ間に合う」",

            // マルコ（商人）
            ("npc_marco", KarmaRank.Saint) =>
                "「おや、聖人様ですか！ならば特別に良品をご紹介しましょう。評判の高い方とは長いお付き合いができる」",
            ("npc_marco", KarmaRank.Virtuous) =>
                "「善い評判が先に届いていましたよ。こういうお客さんとの商売は気持ちが良い」",
            ("npc_marco", KarmaRank.Normal) =>
                "「普通の旅人ですか。良いですよ、普通が一番安心できる」",
            ("npc_marco", KarmaRank.Neutral) =>
                "「どちらとも取れない、か。商売には関係ないけど、気になる人物ですね」",
            ("npc_marco", KarmaRank.Rogue) =>
                "「……少し評判が悪いですね。でも商売は商売だ。金さえあれば」",
            ("npc_marco", KarmaRank.Criminal) =>
                "「あなたのような客とはあまり……でも、闇の品を買いに来たなら話は別です。特別ルートがある」",
            ("npc_marco", KarmaRank.Villain) =>
                "「……外道か。正直に言うと怖い。でも黙って商売だけします。荒事は困る」",

            // マーヴィン（ギルド受付）
            ("npc_marvin", KarmaRank.Saint) =>
                "「ギルドも君の活躍を誇りに思っています。聖人認定の候補ですよ！」",
            ("npc_marvin", KarmaRank.Virtuous) =>
                "「良い冒険者の顔をしていますね。依頼を紹介するのが楽しみです」",
            ("npc_marvin", KarmaRank.Normal) =>
                "「普通の冒険者ですね。依頼を見ていきますか？」",
            ("npc_marvin", KarmaRank.Neutral) =>
                "「どうも、冒険者さん。今日も依頼が来ていますよ」",
            ("npc_marvin", KarmaRank.Rogue) =>
                "「……少し心配な評判が来ていますが。依頼は出せますが、問題行動があれば除名です」",
            ("npc_marvin", KarmaRank.Criminal) =>
                "「正直に言います。このままでは除名処分になります。ギルドは悪評の冒険者を守れない」",
            ("npc_marvin", KarmaRank.Villain) =>
                "「ギルドとしてはお断りします。あなたの行いは……多くの依頼人が迷惑を被っている」",

            // 汎用反応
            (_, KarmaRank.Saint) => "「噂には聞いていましたよ。聖人と呼ばれる方ですね。名誉です」",
            (_, KarmaRank.Virtuous) => "「善い評判の方ですね。安心してお話しできます」",
            (_, KarmaRank.Normal) => "「普通の旅人ですか。どうぞ」",
            (_, KarmaRank.Neutral) => "「……何か用ですか」",
            (_, KarmaRank.Rogue) => "「……少し距離を置かせてください」",
            (_, KarmaRank.Criminal) => "「悪人とは関わりたくない。用件だけ聞きます」",
            (_, KarmaRank.Villain) => "「来ないでください。怖い」",
            _ => "「……」",
        };

    /// <summary>
    /// カルマ段階変化時の演出メッセージを取得する
    /// </summary>
    public static string GetKarmaTransitionText(KarmaRank oldRank, KarmaRank newRank) =>
        (oldRank, newRank) switch
        {
            (KarmaRank.Neutral, KarmaRank.Normal) =>
                "（あなたの善意が積み重なり、普通の旅人として認められるようになった）",
            (KarmaRank.Normal, KarmaRank.Virtuous) =>
                "（世間があなたの善い行いを認め始めた。「善人」と呼ばれるようになった）",
            (KarmaRank.Virtuous, KarmaRank.Saint) =>
                "（あなたの行いは伝説となった。「聖人」の称号が与えられた。重い責任を伴う）",
            (KarmaRank.Neutral, KarmaRank.Rogue) =>
                "（あなたの行いが影を帯び始めた。「悪漢」と噂されるようになった）",
            (KarmaRank.Rogue, KarmaRank.Criminal) =>
                "（悪評が広まった。「悪党」として警戒される存在になった）",
            (KarmaRank.Criminal, KarmaRank.Villain) =>
                "（あなたは「外道」と呼ばれるようになった。町から締め出されることもあるだろう）",
            // 善方向への回帰
            (KarmaRank.Rogue, KarmaRank.Neutral) =>
                "（悪評が少し和らいだ。まだ信用は薄いが、前よりは受け入れられている）",
            (KarmaRank.Criminal, KarmaRank.Rogue) =>
                "（少しずつ信用を取り戻している。「悪漢」止まりだが、改善の余地はある）",
            (KarmaRank.Villain, KarmaRank.Criminal) =>
                "（外道から悪党へ。まだ道は遠いが、歩み始めたことは確かだ）",
            _ => ""
        };

    // =========================================================
    // α.28: カルマ条件の会話分岐テキスト
    // =========================================================

    /// <summary>
    /// カルマ条件付き選択肢のテキストを取得する
    /// </summary>
    public static string GetKarmaChoiceText(string choiceId, KarmaRank requiredRank) =>
        (choiceId, requiredRank) switch
        {
            // 善良系選択肢
            ("help_free", KarmaRank.Virtuous) =>
                "「無償で手伝ってあげよう（報酬は不要だ）」[善人以上で選択可]",
            ("forgive_enemy", KarmaRank.Normal) =>
                "「今回は見逃してやる」[普通以上で選択可]",
            ("donate_gold", KarmaRank.Normal) =>
                "「寄付しよう」[普通以上で選択可]",
            ("protect_weak", KarmaRank.Virtuous) =>
                "「弱者を守ることが当然だ」[善人以上で選択可]",
            ("sacrifice_self", KarmaRank.Saint) =>
                "「自分の命を犠牲にしてでも守る」[聖人のみ選択可]",

            // 悪意系選択肢
            ("steal_item", KarmaRank.Rogue) =>
                "「こっそり盗んでいこう」[悪漢以下で選択可]",
            ("threaten_npc", KarmaRank.Criminal) =>
                "「脅して情報を吐かせる」[悪党以下で選択可]",
            ("betray_ally", KarmaRank.Criminal) =>
                "「仲間を裏切って報酬を全部いただく」[悪党以下で選択可]",
            ("blackmail", KarmaRank.Villain) =>
                "「弱みを握って永続的に搾取する」[外道のみ選択可]",
            ("mass_intimidation", KarmaRank.Villain) =>
                "「町全体を恐怖で支配する」[外道のみ選択可]",

            _ => $"[カルマ条件: {requiredRank}]"
        };

    /// <summary>
    /// カルマによって変化するNPC台詞を取得する（条件分岐会話）
    /// </summary>
    public static string GetKarmaConditionalDialogue(string dialogueId, KarmaRank playerRank) =>
        dialogueId switch
        {
            "dlg_elder_greeting" => playerRank switch
            {
                KarmaRank.Saint =>
                    "長老「ああ……光の人が来た。あなたがいれば、この村は大丈夫だ。なんでも聞いてください」",
                KarmaRank.Virtuous or KarmaRank.Normal =>
                    "長老「旅人か。我々は困っている。助けてもらえるなら、できる限りのことをしよう」",
                KarmaRank.Neutral =>
                    "長老「……どんな人間かはわからん。でも、今は選んでいられない。聞いてほしいことがある」",
                KarmaRank.Rogue =>
                    "長老「……あなたは信用できるのか？ 迷っているが、他に頼める人がいない」",
                KarmaRank.Criminal =>
                    "長老「なぜ来た。お前のような者に用はない——いや、待て。もう誰も来てくれないなら……」",
                KarmaRank.Villain =>
                    "長老「外道か！ 村に入るな！ ……いや、待て。お前でなければ、この子が死ぬ」",
                _ => "長老「旅人か。用件は何だ」"
            },

            "dlg_guard_checkpoint" => playerRank switch
            {
                KarmaRank.Saint or KarmaRank.Virtuous =>
                    "衛兵「ああ、名高い方ですね。通行証は不要です。どうぞお通りください」",
                KarmaRank.Normal =>
                    "衛兵「通行証を確認します。……よし、通ってください」",
                KarmaRank.Neutral =>
                    "衛兵「身元の確認が必要です。通行証はお持ちですか？」",
                KarmaRank.Rogue =>
                    "衛兵「待て。あなたの名が手配書に似ているが……」",
                KarmaRank.Criminal =>
                    "衛兵「止まれ！ 貴様、お尋ね者か！ 武器を捨てろ！」",
                KarmaRank.Villain =>
                    "衛兵「外道め！ 全員集まれ！ 逃がすな！」",
                _ => "衛兵「止まれ。通行証を見せろ」"
            },

            "dlg_inn_keeper" => playerRank switch
            {
                KarmaRank.Saint =>
                    "宿屋「聖人のお泊まりとは、宿の誉れです。特別室をご用意します。料金はご心配なく」",
                KarmaRank.Virtuous or KarmaRank.Normal =>
                    "宿屋「いらっしゃいませ。清潔な部屋をご用意します」",
                KarmaRank.Neutral =>
                    "宿屋「一泊いくらですか？ ……先払いでお願いします」",
                KarmaRank.Rogue =>
                    "宿屋「……うちは普通のお客さんしか泊めないんですが。まあ、金を先に払うなら」",
                KarmaRank.Criminal =>
                    "宿屋「お断りします。評判の悪い方を泊めると、他のお客に逃げられる」",
                KarmaRank.Villain =>
                    "宿屋「出て行け！ 今すぐ！ 衛兵を呼ぶぞ！」",
                _ => "宿屋「ご宿泊ですか？」"
            },

            _ => ""
        };

    // =========================================================
    // α.29: 闇市NPC会話テキスト
    // =========================================================

    /// <summary>
    /// 闇市商人の台詞を取得する
    /// </summary>
    public static string GetBlackMarketDialogue(string situation, KarmaRank playerRank = KarmaRank.Neutral) =>
        situation switch
        {
            "greeting" => playerRank switch
            {
                KarmaRank.Saint or KarmaRank.Virtuous =>
                    "「……聖人がここに来るとは。忘れてくれ、こんな場所を見たことは。私は商売人、それだけだ」",
                KarmaRank.Normal or KarmaRank.Neutral =>
                    "「ここは知る人ぞ知る場所だ。余計なことを聞くな、余計なことを言うな。商売するだけだ」",
                KarmaRank.Rogue =>
                    "「お、同類の匂いがする。遠慮なく見ていけ。正規ルートにはない品が揃っている」",
                KarmaRank.Criminal or KarmaRank.Villain =>
                    "「来ると思っていたよ。悪党には悪党の市場が必要だ。最高の品を用意してある」",
                _ => "「……誰だ。紹介状はあるか？」"
            },

            "password_correct" =>
                "「なるほど、顔が利く。どうぞ奥へ。見てはいけないものも見えるかもしれないが、忘れろ」",

            "password_wrong" =>
                "「知らない人間だな。ここは見世物ではない。立ち去れ」",

            "haggle" =>
                "「値引き交渉か。面白い度胸だ。……では少し下げよう。でもこれ以上はなし」",

            "buy_illegal_item" =>
                "「これを買うのか。後のことは自分で責任を持ってくれ。私は知らない」",

            "sell_stolen_goods" =>
                "「出所は聞かない。品質さえ良ければ買う。これが闇市のルールだ」",

            "suspicious_item" =>
                "「これは……どこで手に入れた？ いや、聞かない。でも扱いに気をつけろ。曰く付きだ」",

            "rare_poison" =>
                "「毒か。用途は聞かない。ただ、使い方を誤ると自分に返ってくる。注意しろ」",

            "farewell_normal" =>
                "「覚えておいてくれ——ここに来たことを誰にも言うな。お互いのためだ」",

            "farewell_criminal" =>
                "「次回もよろしく。悪党同士の商売は長続きする。生きていればの話だが」",

            "caught_by_guard" =>
                "（商人が素早く品物を隠した。「警備が来た！ 裏口から逃げろ！」）",

            "no_password" =>
                "「紹介がないと話せない。仕組みを理解してから来い」",

            "first_visit" =>
                "「新顔か。ルールを教えてやる。見たことを話すな、聞いたことを広めるな、名前を教えるな。以上だ」",

            _ => "「……何か用か」"
        };

    /// <summary>
    /// 闇市の品揃え説明テキストを取得する
    /// </summary>
    public static string GetBlackMarketItemDescription(string itemCategory) => itemCategory switch
    {
        "poison" =>
            "「毒薬各種。致死性のものから痺れ薬まで。保証はしないが、品質は本物だ」",

        "stolen_weapon" =>
            "「由来を聞くな。良い武器だ。それだけは保証する」",

        "forbidden_scroll" =>
            "「禁書の断片から書き写した呪文書だ。使用は自己責任。副作用があっても私は知らない」",

        "forged_document" =>
            "「精巧な贋造書類だ。本物か偽物か、プロでも見抜けない。ただし使う場所は選べ」",

        "black_market_info" =>
            "「情報も売り物だ。権力者の秘密、犯罪組織の動向——これを知れば交渉の場で優位に立てる」",

        "slave_contract" =>
            "「……これは扱いが難しい品だ。法的にはグレー。でも需要はある」",

        "cursed_item" =>
            "「呪いの品だ。危険だが、その分強力でもある。扱い方を心得た者にしか売らない」",

        "rare_material" =>
            "「正規ルートでは入手不可能な素材だ。出所は深い森か、遥か南の秘境か——詳細は不要だろう」",

        _ => "「これは……特別な品だ。値段は相談だ」"
    };

    /// <summary>
    /// 闇市で聞ける裏情報のテキストを取得する
    /// </summary>
    public static string GetUndergroundInformation(string topic) => topic switch
    {
        "guild_secret" =>
            "「冒険者ギルドの上層部には黒幕がいる。表向きは冒険者保護だが、一部の依頼は別の目的がある」",

        "noble_scandal" =>
            "「ある貴族が領民の税金を横領している。証拠があれば王に直訴できるが——危険も伴う」",

        "dungeon_secret" =>
            "「最下層には人工的な構造物がある。誰かが意図的に作った迷宮だ。それが誰かは知らない」",

        "black_organization" =>
            "「影の組織が各領地で動いている。目的は不明だが、最近活動が活発になっている」",

        "hidden_route" =>
            "「山の北側に衛兵の目に付かない抜け道がある。知る者は少ない。場所は別途料金で教えよう」",

        "weapon_smuggling" =>
            "「大量の武器が北から密輸されている。誰のためかは……教えられない。関わると死ぬ」",

        _ => "「それについては話せない。話せる情報の限界がある」"
    };
}
