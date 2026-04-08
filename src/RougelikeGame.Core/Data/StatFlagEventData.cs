namespace RougelikeGame.Core.Data;

/// <summary>
/// α.30: 能力値フラグ条件の会話テキスト
/// α.31: 能力値条件イベントテキスト
/// </summary>
public static class StatFlagEventData
{
    // =========================================================
    // α.30: 能力値フラグ条件の会話テキスト
    // =========================================================

    /// <summary>
    /// StatFlagに応じたNPC特別反応テキストを取得する
    /// </summary>
    public static string GetStatFlagNpcReaction(string npcId, StatFlag flag) =>
        (npcId, flag) switch
        {
            // レオン
            ("npc_leon", StatFlag.Herculean) =>
                "レオン「その体格は……怪力の持ち主だね。力があるからこそ、どう使うかが重要になる」",
            ("npc_leon", StatFlag.Erudite) =>
                "レオン「博識な人だ。知識は力だが——知りすぎることの代償も知っている？」",
            ("npc_leon", StatFlag.EagleEye) =>
                "レオン「鋭い目をしている。今まで見えていなかったものが見えるようになったなら——大切にしなさい」",
            ("npc_leon", StatFlag.SteadyMind) =>
                "レオン「精神力の強さを感じる。何度死に戻っても正気を保てるのは、そのせいかもしれない」",

            // アルバート（賢者）
            ("npc_albert", StatFlag.Erudite) =>
                "「博識な者が来た！ 一緒に研究しよう。古代文明の文字について、君の意見を聞きたい」",
            ("npc_albert", StatFlag.Herculean) =>
                "「怪力か。遺跡の重い扉を開けるのに力が必要な場所がある。頼んでもいいか？」",
            ("npc_albert", StatFlag.EagleEye) =>
                "「鷹の目……隠し文字が見えたりするかね？ 碑文に何か特別なものを感じないか？」",

            // マーヴィン（ギルド）
            ("npc_marvin", StatFlag.Herculean) =>
                "「怪力の冒険者！ ちょうど依頼が来ています。岩盤を砕く必要がある現場で——」",
            ("npc_marvin", StatFlag.FleetFooted) =>
                "「韋駄天と呼ばれているとか？ 緊急配達の依頼で探していた人材です」",
            ("npc_marvin", StatFlag.Dexterous) =>
                "「神業の持ち主ですね。罠解除の専門依頼が来ています。あなたなら完璧でしょう」",

            // 汎用反応
            (_, StatFlag.Herculean) =>
                "「……その腕、石割りでもできそうだな。力仕事が必要なときは頼もうか」",
            (_, StatFlag.Erudite) =>
                "「博識な方ですね。あなたのような人に話を聞いてほしいことがある」",
            (_, StatFlag.EagleEye) =>
                "「鋭い目をしている。この辺りで奇妙なものを見ませんでしたか？」",
            (_, StatFlag.FleetFooted) =>
                "「足が速いですね。追手が来ても逃げられそうだ」",
            (_, StatFlag.Charismatic) =>
                "「あなたと話していると気持ちが和らぐ。なぜでしょう……」",
            (_, StatFlag.Lucky) =>
                "「強運の方だとか。良い方向で縁があれば、と思います」",
            (_, StatFlag.Robust) =>
                "「頑健な体ですね。並の状態異常ではびくともしなそうだ」",
            (_, StatFlag.Dexterous) =>
                "「器用な手先だ。細かい作業があるときは声をかけてください」",
            (_, StatFlag.SteadyMind) =>
                "「精神力の強さを感じる。何があっても動じないのですね」",
        };

    /// <summary>
    /// 能力値フラグによる特別会話オプションのラベルを取得する
    /// </summary>
    public static string GetStatFlagChoiceLabel(StatFlag flag) => flag switch
    {
        StatFlag.Herculean => "［怪力］ 力でこじ開ける",
        StatFlag.Erudite => "［博識］ 古文書を解読する",
        StatFlag.EagleEye => "［鷹の目］ 隠し扉を見つける",
        StatFlag.FleetFooted => "［韋駄天］ 素早く逃げる",
        StatFlag.Charismatic => "［魅力的］ 交渉で値引きさせる",
        StatFlag.Lucky => "［強運］ 運任せで挑む",
        StatFlag.Robust => "［頑健］ 無理やり耐える",
        StatFlag.Dexterous => "［神業］ 精密な作業をこなす",
        StatFlag.SteadyMind => "［精神力］ 心理的圧迫に耐える",
        _ => "（特殊行動）"
    };

    // =========================================================
    // α.31: 能力値条件イベントテキスト
    // =========================================================

    /// <summary>
    /// 怪力フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetHerculeanEventText(string eventId) => eventId switch
    {
        "boulder_block" =>
            "行く手を塞ぐ巨大な岩がある。普通の人間ならびくともしないが——怪力を持つあなたは違う。" +
            "両腕に力を込めると、岩はゆっくりと動き始めた。道が開けた。",

        "iron_gate" =>
            "鍵のかかった鉄の門。しかし蝶番が錆びている。力を込めると——軋み声を上げながら外れた。",

        "collapsed_passage" =>
            "崩落した瓦礫が通路を塞いでいる。ひとつずつ取り除くと、やがて人が通れる隙間が生まれた。",

        "trapped_creature" =>
            "岩の下に生き物が挟まっている。怪力を振るい、岩を持ち上げると——傷ついた狼が顔を上げた。" +
            "恩義を感じたのか、狼はあなたの周囲をしばらく歩いた。",

        "battle_shortcut" =>
            "敵の群れを迂回する必要があった。しかし、近道となる石壁を——怪力でぶち破った。",

        _ => "（怪力が活かせる場面だった）"
    };

    /// <summary>
    /// 博識フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetEruditeEventText(string eventId) => eventId switch
    {
        "ancient_inscription" =>
            "石壁に刻まれた古代文字。通常は意味不明な模様にしか見えないが、" +
            "あなたの知識がそれを言語として認識した。「——ここより先、名前を呼ぶ者は戻れぬ——」",

        "mysterious_tome" =>
            "錆びた金庫の中に古い書物があった。解読不可能な暗号文字で書かれているが、" +
            "あなたは一行ずつ解読していった。そこには古代の術師が記した禁断の知識が……",

        "strange_mechanism" =>
            "複雑な機械装置が目の前にある。仕組みが理解できれば操作できそうだが——" +
            "あなたの博識が正解の操作手順を示した。装置が動き始めた。",

        "poison_identification" =>
            "怪しい液体が置かれている。色と臭いの組み合わせから——これは神経毒だとわかった。" +
            "回避策も頭に浮かぶ。知識は、時に命を救う。",

        "npc_scholar_event" =>
            "「あなたは……本当に博識だ」老学者が目を丸くした。「この暗号を解けるとは。一つ聞かせてほしい——」",

        _ => "（博識が活かせる場面だった）"
    };

    /// <summary>
    /// 鷹の目フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetEagleEyeEventText(string eventId) => eventId switch
    {
        "hidden_door" =>
            "壁の模様に違和感を覚えた。他の人には見えないが——鷹の目があなたに教える。" +
            "押すと、静かに壁が動いた。隠し通路が現れた。",

        "distant_enemy" =>
            "遠くの闇の中に何かがいる。目を凝らすと——伏兵だ。彼らはまだこちらに気づいていない。" +
            "先手を打つか、迂回するか——いずれにせよ、優位に立てる。",

        "trap_detection" =>
            "床の模様が不自然だ。鷹の目が罠の存在を教えた。針仕掛けの罠を踏まずに通り抜けた。",

        "hidden_treasure" =>
            "岩の隙間に何かが光っている。見逃す人が大半だろうが——あなたは見えた。" +
            "手を伸ばすと、小さな宝石の欠片が出てきた。",

        "spy_spotted" =>
            "人ごみの中に不自然な動きをする人物がいる。スパイだ。" +
            "衛兵に知らせるか、自分で対処するか——情報を持つのはあなただけだ。",

        _ => "（鷹の目が活かせる場面だった）"
    };

    /// <summary>
    /// 韋駄天フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetFleetFootedEventText(string eventId) => eventId switch
    {
        "escape_ambush" =>
            "突然の奇襲。普通の旅人なら逃げ切れなかっただろうが——" +
            "韋駄天の足が地を蹴り、追手を引き離した。",

        "special_route" =>
            "崖の側面に細い獣道がある。普通の足では滑落するが、あなたの脚力なら——" +
            "飛び移り、渡り切った。通常のルートより半日短縮できた。",

        "courier_mission" =>
            "「急いでこれを届けてほしい！」依頼人が叫んだ。" +
            "韋駄天の足でその距離を駆け抜けた。相手は目を疑った。「こんなに早く……？」",

        "chase_criminal" =>
            "逃げる犯人を追跡した。路地を曲がり、屋根を駆け、川を飛び越えた。" +
            "韋駄天でなければ見失っていただろう。",

        _ => "（韋駄天が活かせる場面だった）"
    };

    /// <summary>
    /// 魅力的フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetCharismaticEventText(string eventId) => eventId switch
    {
        "shop_discount" =>
            "「あなたには特別に……10%お引きします」商人が照れたように言った。" +
            "あなたの魅力が、頑固な商人の心を動かした。",

        "hostile_npc_calmed" =>
            "「待て！ 話を聞いてくれ」あなたの声には不思議な説得力があった。" +
            "剣を構えていた衛兵が、ゆっくりと武器を下げた。",

        "information_extracted" =>
            "「普通は教えないんだが……あなたになら」情報屋が声を落とした。" +
            "魅力が口を動かすこともある。",

        "ally_bonus" =>
            "仲間があなたの周囲に自然と集まる。「なんか、安心するんだよな」と誰かが言った。",

        _ => "（魅力が活かせる場面だった）"
    };

    /// <summary>
    /// 精神力フラグ使用時のイベントテキストを取得する
    /// </summary>
    public static string GetSteadyMindEventText(string eventId) => eventId switch
    {
        "fear_zone" =>
            "「……恐怖の場所」に踏み込んだ。他の者は立つくされ、正気を失う場所だが——" +
            "あなたの精神力が波をはね返す。冷静に状況を把握できた。",

        "madness_resist" =>
            "何かが心の中に入り込もうとした感触があった。しかし——精神の壁がそれを弾き飛ばした。",

        "interrogation_resist" =>
            "「吐け！」拷問者が迫った。精神力で苦痛を遠ざけ、情報を守り通した。",

        "psychic_attack" =>
            "強力な精神攻撃が飛んできた。精神力が防壁となり——無効化した。",

        _ => "（精神力が活かせる場面だった）"
    };

    /// <summary>
    /// 能力値フラグ使用失敗テキストを取得する（フラグ不足時）
    /// </summary>
    public static string GetStatFlagFailText(StatFlag requiredFlag) => requiredFlag switch
    {
        StatFlag.Herculean =>
            "（岩は重すぎて動かせなかった。STR25以上の怪力が必要だ）",
        StatFlag.Erudite =>
            "（古代文字を読み解けなかった。INT25以上の博識が必要だ）",
        StatFlag.EagleEye =>
            "（見えなかった。PER25以上の鷹の目が必要だ）",
        StatFlag.FleetFooted =>
            "（速さが足りなかった。AGI25以上の韋駄天が必要だ）",
        StatFlag.Charismatic =>
            "（説得できなかった。CHA20以上の魅力が必要だ）",
        StatFlag.Lucky =>
            "（運が向かなかった。LUK20以上の強運が必要だ）",
        StatFlag.Robust =>
            "（耐えられなかった。VIT25以上の頑健が必要だ）",
        StatFlag.Dexterous =>
            "（精度が足りなかった。DEX25以上の神業が必要だ）",
        StatFlag.SteadyMind =>
            "（精神が揺らいだ。MND25以上の精神力が必要だ）",
        _ => "（その能力が足りなかった）"
    };
}
