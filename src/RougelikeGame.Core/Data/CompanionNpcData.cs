namespace RougelikeGame.Core.Data;

/// <summary>
/// α.34: 仲間NPC人物設定
/// α.35: 仲間NPC会話テキスト
/// α.36: 傭兵NPC台詞テキスト
/// </summary>
public static class CompanionNpcData
{
    // =========================================================
    // α.34: 仲間NPC人物設定
    // =========================================================

    /// <summary>
    /// 加入可能仲間NPCのプロフィールレコード
    /// </summary>
    public record CompanionProfile(
        string Id,
        string Name,
        string Race,
        string ClassType,
        string Age,
        string Personality,
        string Background,
        string Motivation,
        string JoinCondition,
        string Secret
    );

    /// <summary>
    /// 仲間NPC一覧
    /// </summary>
    public static IReadOnlyList<CompanionProfile> AllCompanions { get; } = new List<CompanionProfile>
    {
        new(
            Id: "companion_rena",
            Name: "レナ・フォスター",
            Race: "人間",
            ClassType: "治癒術師",
            Age: "22歳",
            Personality: "明るく世話好き。弱者には優しいが、不正義には強く反発する。" +
                         "時々突っ走って失敗するが、反省から学ぶ誠実さがある。",
            Background: "大きな村の施療院で育った。医師の父の背中を見て育ち、戦場医療を志す。" +
                        "武装した冒険者についていけるよう剣術も独学で習得。",
            Motivation: "「どんな状況でも死ぬべきではない命を守りたい」——戦場で父を失った経験が原点。",
            JoinCondition: "海岸領地の施療院で負傷した民衆を治療中に遭遇。共に救助を行うことで仲間になる。",
            Secret: "父は実は事故ではなく、ある組織に消された疑いがある。その真相を探るために冒険に出た側面もある。"
        ),
        new(
            Id: "companion_brom",
            Name: "ブロム・アイアンフィスト",
            Race: "ドワーフ",
            ClassType: "重戦士",
            Age: "156歳（ドワーフとしては中年）",
            Personality: "頑固で口数が少ない。信頼した相手には不器用に誠実。酒を飲むと少し柔らかくなる。" +
                         "一度決めたことは必ずやり遂げる不屈の意志の持ち主。",
            Background: "ドワーフ山岳部の名門鍛冶師の家系。後継ぎとして期待されていたが、" +
                        "師匠への裏切りが原因で一族を追われた（詳細は語らない）。",
            Motivation: "「借りを返す旅だ」と言うが、具体的には言わない。",
            JoinCondition: "山岳部のダンジョンで、罠に嵌まった状態で発見。助けることで仲間になる。",
            Secret: "一族を追われた理由は、師匠の犯罪を告発したからだった。正しいことをして追放された——" +
                    "そのことを誰にも言えない不条理を抱えている。"
        ),
        new(
            Id: "companion_syl",
            Name: "シル・サンダーウィング",
            Race: "エルフ",
            ClassType: "魔法使い",
            Age: "340歳（外見は20代）",
            Personality: "知的でやや皮肉屋。感情表現が乏しいように見えるが、実際は深く感じている。" +
                         "若者（人間基準）には辛抱強く接する。",
            Background: "エルフの学術院で三百年研究を続けてきたが、ある禁断実験を目撃して学院を離れた。" +
                        "今は「現場で学ぶ」ことを選んだ。",
            Motivation: "「長く生きすぎた。変化の中でしか本当のことは学べない」",
            JoinCondition: "森林地帯のエルフ遺跡で、危険な古代魔法を研究中のところを発見。" +
                           "研究の協力を申し出ることで仲間になる。",
            Secret: "学院で見た「禁断実験」の内容を知っている。それは「死に戻り」の仕組みに関わるものだった。"
        ),
        new(
            Id: "companion_mika",
            Name: "ミカ・サンドストーン",
            Race: "人間（南部出身）",
            ClassType: "斥候・盗賊",
            Age: "19歳",
            Personality: "快活で楽観的。危険を「面白い」と表現する困った習性がある。" +
                         "口が軽く見えるが、本当の秘密は決して漏らさない。",
            Background: "南部の市場で育ったスリ。腕は立つが義賊的な行動原則を持つ。" +
                        "弱者から盗まず、権力者の懐だけを狙う。",
            Motivation: "「面白いことを探している。世界は広い。一人では見切れない」",
            JoinCondition: "南部の市場で、プレイヤーのアイテムを盗もうとして失敗。" +
                           "捕まえた後に話を聞くと、飢えた子供たちのためだったとわかる。",
            Secret: "両親を奴隷商人に売られた過去がある。探しているが、見つかるたびに「まだ動けない」と感じてしまう。"
        )
    };

    /// <summary>
    /// 仲間NPCのプロフィールを取得する
    /// </summary>
    public static CompanionProfile? GetCompanionProfile(string companionId) =>
        AllCompanions.FirstOrDefault(c => c.Id == companionId);

    // =========================================================
    // α.35: 仲間NPC会話テキスト
    // =========================================================

    /// <summary>
    /// 仲間の加入時台詞を取得する
    /// </summary>
    public static string GetJoinDialogue(string companionId) => companionId switch
    {
        "companion_rena" =>
            "「……わかった。あなたなら信頼できる。一緒に行きましょう。" +
            "でも約束して——できる限り、誰も死なせない戦い方をすること。私が保証する代わりに、あなたも約束して」",

        "companion_brom" =>
            "「……助けてもらった恩は返す。それがドワーフの流儀だ。しばらく付いていく——" +
            "借りを返すまではな。それ以上でも以下でもない。勘違いするな」",

        "companion_syl" =>
            "「三百年の研究より、十日の冒険のほうが多くを教えてくれた。" +
            "あなたと旅を続けることにする。断る理由が見当たらない——" +
            "もし見つかれば、その時に話し合おう」",

        "companion_mika" =>
            "「しょうがないな！ バレたんだから開き直る。一緒に行くよ、どうせひとりで食ってけないし。" +
            "あ、でも仕事は選ばせて。弱い人から盗む仕事は断る。そこだけ条件」",

        _ => "「……わかった。一緒に行こう」"
    };

    /// <summary>
    /// 仲間の通常会話テキストを取得する
    /// </summary>
    public static string GetCompanionIdleDialogue(string companionId, int loopCount = 0) => companionId switch
    {
        "companion_rena" => loopCount switch
        {
            0 => "「この傷、手当てさせて。放っておくと悪化するから」",
            1 => "「前の旅のこと……少し教えてもらえる？」",
            _ => "「また新しい旅が始まった。一緒に生き延びましょう」"
        },

        "companion_brom" => loopCount switch
        {
            0 => "「……今夜は野営か。悪くない。薪を集めてくる」",
            1 => "「ドワーフ料理を作ってやる。不味いとは言わせない——旨いから」",
            _ => "「また一緒に来たな。縁というのは不思議なものだ」"
        },

        "companion_syl" => loopCount switch
        {
            0 => "「この地形には魔力の流れがある。何かが起きた場所だ——数百年前に」",
            1 => "「人間の旅に付き合うのは新鮮だ。何もかも急ぎすぎるが、その分濃密でもある」",
            _ => "「また同じ経路を。変わったことと変わらないこと、両方を見るのが旅の楽しさだ」"
        },

        "companion_mika" => loopCount switch
        {
            0 => "「腹減った。何か食べよう。私が調達してくる——合法的な方法で、たぶん」",
            1 => "「あの町、良い市場があった。今度寄ろうよ。もちろん買い物するだけだよ？」",
            _ => "「また一緒か。生き残れるもんだね、私たち」"
        },

        _ => "「……」"
    };

    /// <summary>
    /// 仲間の好感度別会話テキストを取得する
    /// </summary>
    public static string GetCompanionAffinityDialogue(string companionId, int affinityLevel) =>
        (companionId, affinityLevel) switch
        {
            // レナ
            ("companion_rena", 1) =>
                "レナ「……あなたのことがわかってきた気がする。もう少し話を聞かせて」",
            ("companion_rena", 2) =>
                "レナ「一緒にいると安心する。戦場で信頼できる人がいることの大切さ——今は身に染みている」",
            ("companion_rena", 3) =>
                "レナ「父のことを話したことなかったね。……実は、ここだから話せることがある」",

            // ブロム
            ("companion_brom", 1) =>
                "ブロム「……お前は筋がある。ドワーフでも認める人間は少ない。悪くない」",
            ("companion_brom", 2) =>
                "ブロム「酒を飲むか。一族を追われた後、こうして誰かと飲むのは久しぶりだ」",
            ("companion_brom", 3) =>
                "ブロム「……本当のことを話す。師匠への裏切りといわれているが、実際は——」",

            // シル
            ("companion_syl", 1) =>
                "シル「あなたとの旅は予測不可能だ。三百年の経験でも読めない——面白い」",
            ("companion_syl", 2) =>
                "シル「長く生きていると、新しいものに驚かなくなる。でも——あなたは例外だ」",
            ("companion_syl", 3) =>
                "シル「言うべきことがある。死に戻りの仕組みについて、私が知っていることを——」",

            // ミカ
            ("companion_mika", 1) =>
                "ミカ「あなたって面白い人だよね。普通の冒険者と違う。なんか、信用できる感じがする」",
            ("companion_mika", 2) =>
                "ミカ「……親のこと、探してるんだ。いつか——話せる気がしてきた」",
            ("companion_mika", 3) =>
                "ミカ「ねえ、お願いがある。私の親を探すのを、一緒に手伝ってほしい。力を借りたい」",

            _ => "「……」"
        };

    /// <summary>
    /// 仲間の戦闘中台詞を取得する
    /// </summary>
    public static string GetCompanionBattleDialogue(string companionId, string situation) =>
        (companionId, situation) switch
        {
            ("companion_rena", "ally_low_hp") =>
                "「危ない！ 下がって、今すぐ治療する！」",
            ("companion_rena", "enemy_killed") =>
                "「……行かせたくなかった。でも仕方なかった」",
            ("companion_rena", "low_hp_self") =>
                "「まだ、やれる……仲間がいる限り、倒れない！」",

            ("companion_brom", "ally_low_hp") =>
                "「俺が引きつける！ その隙に下がれ！」",
            ("companion_brom", "enemy_killed") =>
                "「鉄拳の前に敵なし。ドワーフの意地を見せた」",
            ("companion_brom", "low_hp_self") =>
                "「ぐ……ドワーフは倒れない。倒れたことに気づかないだけだ」",

            ("companion_syl", "ally_low_hp") =>
                "「治癒魔法の詠唱を始める。あと10秒、持ちこたえろ」",
            ("companion_syl", "enemy_killed") =>
                "「計算通り。この程度の敵なら予測の誤差は1%以内だった」",
            ("companion_syl", "low_hp_self") =>
                "「……計算が狂った。これほどの強さとは予測外だ。修正する」",

            ("companion_mika", "ally_low_hp") =>
                "「やばい！ 私が囮になる。こっちを見て！」",
            ("companion_mika", "enemy_killed") =>
                "「よしっ！ 動きが止まった瞬間が勝負だと思ってた。当たり！」",
            ("companion_mika", "low_hp_self") =>
                "「いたたた……でも、まだ動ける。諦めるのは最後の選択肢だ」",

            _ => "「……！」"
        };

    // =========================================================
    // α.36: 傭兵NPC台詞テキスト
    // =========================================================

    /// <summary>
    /// 傭兵の雇用時台詞を取得する
    /// </summary>
    public static string GetMercenaryHireDialogue(string mercenaryType) => mercenaryType switch
    {
        "swordsman" =>
            "「剣を雇いたいのか。金と仕事内容次第だ。" +
            "どんな敵でも一対一なら負けない自信がある——その代わり、給料は高い」",

        "archer" =>
            "「遠距離支援が欲しいのか。良い弓を持っている。" +
            "前線には出ない。後方から援護する——それが私の戦い方だ。文句があるなら雇うな」",

        "mage" =>
            "「魔法使いを探しているか。一回の雇用で呪文20発まで保証する。" +
            "それ以上は追加料金。魔力は商品だ——タダで使い潰すなよ」",

        "tank" =>
            "「盾が欲しいのか。俺は盾役の専門だ。" +
            "守ることに命を賭ける——でも死に急がせないでくれ。守る価値のある仲間がいる時だけ、本気になれる」",

        "assassin" =>
            "「……依頼の内容を聞く。ターゲットは誰だ。" +
            "子供や老人は断る。それ以外の条件は交渉次第だ。名前は聞くな」",

        "healer" =>
            "「治癒師を求めているか。腕前は確か、でも前線には出ない。" +
            "後方で待機する代わりに、全力で治療する——守ってくれるなら最高の仕事をする」",

        _ => "「仕事内容を聞かせてくれ。金と条件次第だ」"
    };

    /// <summary>
    /// 傭兵の戦闘中台詞を取得する
    /// </summary>
    public static string GetMercenaryBattleDialogue(string mercenaryType, string situation) =>
        (mercenaryType, situation) switch
        {
            ("swordsman", "engage") => "「やってやる。剣に恥じない戦いを見せてやる！」",
            ("swordsman", "low_hp") => "「傷が……深い。でも退けない。依頼が終わるまでは」",
            ("swordsman", "victory") => "「仕事は終わった。金払ってくれれば文句なし」",

            ("archer", "engage") => "「距離を保て。私の射程内に入れる——始末する」",
            ("archer", "low_hp") => "「近距離は苦手だ。引いてくれ！」",
            ("archer", "victory") => "「全弾命中。満足のいく仕事だった」",

            ("mage", "engage") => "「詠唱開始。邪魔するな——集中が切れる」",
            ("mage", "low_hp") => "「魔力が……もたない。早く決めてくれ」",
            ("mage", "victory") => "「20発使い切った。これ以上は追加料金だ」",

            ("tank", "engage") => "「俺が前に出る。後ろは任せた。絶対に守る」",
            ("tank", "low_hp") => "「……ここで退いたら仲間が死ぬ。まだ動ける」",
            ("tank", "victory") => "「誰も死ななかった。それで十分だ」",

            ("assassin", "engage") => "（無言で動き出した）",
            ("assassin", "target_killed") => "「仕事は終わった。残りは任せる」",
            ("assassin", "unexpected_target") => "「これは話が違う。引く」",

            ("healer", "ally_damaged") => "「傷を見せろ、今すぐ治す！」",
            ("healer", "low_hp") => "「……治癒師が倒れたら誰が治す。護衛してくれ！」",
            ("healer", "victory") => "「全員生還。これが一番の結果だ」",

            _ => "「……！」"
        };

    /// <summary>
    /// 傭兵の契約終了時台詞を取得する
    /// </summary>
    public static string GetMercenaryFarewellDialogue(string mercenaryType, bool positiveEnd) =>
        (mercenaryType, positiveEnd) switch
        {
            ("swordsman", true) => "「良い仕事だった。また依頼があれば声をかけてくれ。今度は少し安くしてやる——少しだけ」",
            ("swordsman", false) => "「金だけ受け取る。また雇う気があるなら、条件を改めてくれ」",

            ("archer", true) => "「弓の腕が認められるのは嬉しい。また機会があれば」",
            ("archer", false) => "「今回は仕事をこなした。それだけだ」",

            ("mage", true) => "「……面白い依頼だった。次回は呪文の数を増やしておく」",
            ("mage", false) => "「条件通りこなした。ただし次は魔力の補充費用も請求する」",

            ("tank", true) => "「誰も死なかった。最高の結末だ。また呼んでくれ」",
            ("tank", false) => "「依頼は終わった。次は護衛に適した作戦を立ててくれ」",

            ("assassin", true) => "（受け取りに来る——それだけが挨拶だった）",
            ("assassin", false) => "（書置きが一枚残っていた。「金は受け取った」とだけ書いてある）",

            ("healer", true) => "「治癒師として誇りを持てる仕事だった。また依頼があれば」",
            ("healer", false) => "「今後は護衛をもう少し真剣に考えてほしい。命がいくつあっても足りない」",

            _ => positiveEnd
                ? "「良い仕事だった。また機会があれば」"
                : "「仕事は終わった」"
        };
}
