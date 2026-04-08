using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.5, α.7, α.8: 主要NPC人物設定・NPC関係性テキスト・商人台詞テキスト
/// </summary>
public static class NpcCharacterData
{
    // =========================================================
    // α.5: 主要NPC人物設定
    // =========================================================

    /// <summary>
    /// NPC人物設定の詳細プロフィールレコード
    /// </summary>
    public record NpcProfile(
        string Id,
        string Name,
        string Title,
        string Race,
        string Age,
        string Personality,
        string Background,
        string Motivation,
        string Secret
    );

    /// <summary>
    /// 主要NPC人物設定一覧
    /// </summary>
    public static IReadOnlyList<NpcProfile> AllNpcProfiles { get; } = new List<NpcProfile>
    {
        new(
            Id: "npc_leon",
            Name: "レオン・ヴァレンシア",
            Title: "死に戻りの案内人",
            Race: "不明（人間？）",
            Age: "外見は30代。実年齢不明",
            Personality: "穏やかで知的。感情を表に出さないが、冒険者への誠実な配慮がある。" +
                         "どんな状況でも微笑みを崩さないが、それが時に不気味に映る。",
            Background: "太古の誓約によって死者と生者の境界に立つ役割を担わされた。" +
                        "人間だったのか、別の存在だったのか、記録は残っていない。" +
                        "記憶を持ち、意識を持つが、肉体は朽ちることがない。",
            Motivation: "冒険者を「正しい結末」へ導くこと。その結末が何かは彼自身も知らない。",
            Secret: "実は「死に戻り」の設計者の遺志を継ぐ番人。装置を終わらせることができるのは彼だけだが、" +
                    "そうすることが「正しい」かどうかを何千年も悩み続けている。"
        ),
        new(
            Id: "npc_marco",
            Name: "マルコ・ベルナーニ",
            Title: "放浪の商人・情報屋",
            Race: "人間",
            Age: "40代後半",
            Personality: "陽気で社交的。冗談が多く場を和ませるが、商売の場では鋭い洞察力を発揮する。" +
                         "一度信頼した相手には誠実。",
            Background: "元傭兵。戦場での経験から情報の価値を知り、商人兼情報屋に転身した。" +
                        "複数の国から情報提供を依頼されているが、特定の国への忠誠はない。",
            Motivation: "商売の成功と自由な生活。特定の主義主張はなく、利益が動機。" +
                        "ただし、子供や弱者を傷つける依頼は断ることがある。",
            Secret: "幼い頃に故郷の村を失った経験があり、それが「どこにも属さない」生き方の原点。" +
                    "実は膨大な資産を各地に隠しているが、それを使おうとしたことは一度もない。"
        ),
        new(
            Id: "npc_albert",
            Name: "アルバート・フォン・ヴァイスベルク",
            Title: "ルーン語の賢者・王立学術院名誉院長",
            Race: "人間",
            Age: "78歳",
            Personality: "研究に一途で少し頑固。若い学者の意見にも耳を傾ける謙虚さを持つ。" +
                         "冗談には不器用だが、弟子への愛情は人一倍深い。",
            Background: "王立学術院に70年以上所属。ルーン語解読に人生を捧げた。" +
                        "若い頃に古代遺跡で「禁断の言葉」の断片を目撃した体験が転機となった。",
            Motivation: "古代文明が滅んだ理由の解明。その中に「禁断の言葉」が関係していると確信している。",
            Secret: "解読した「禁断の言葉」の一部をすでに知っているが、その言葉を発すると何が起きるかを恐れて封印している。"
        ),
        new(
            Id: "npc_marvin",
            Name: "マーヴィン・ライト",
            Title: "冒険者ギルド受付員",
            Race: "人間",
            Age: "25歳",
            Personality: "誠実で真面目。笑顔を絶やさず冒険者への敬意を忘れない。" +
                         "時に過度に礼儀正しすぎると言われる。",
            Background: "辺境の農村出身。魔物に家族を失い、冒険者への敬意から ギルド職員を志した。" +
                        "冒険者になることを夢見たが、家族の借金返済のためギルド員の道を選んだ。",
            Motivation: "冒険者たちの命を守るサポート。いつか自分の力で家族の仇を討ちたい。",
            Secret: "夜、独自に戦闘訓練を続けている。いつか冒険者として一線に立つことを諦めていない。"
        ),
        new(
            Id: "npc_elwen",
            Name: "エルウェン・セレナリエル",
            Title: "王都宮廷魔法使い",
            Race: "エルフ",
            Age: "420歳（外見は30代人間相当）",
            Personality: "物静かで観察眼が鋭い。人間の短命さを理解しており、接する者への優しさがある。" +
                         "しかし感情移入を避けるため、やや距離を置くように見える。",
            Background: "エルフの森の出身。数百年前、一人の人間冒険者との出会いが彼女の人生を変えた。" +
                        "その者の死後、宮廷に留まり人間たちを見守ることを選んだ。",
            Motivation: "失った人間の友の記憶を守ること。人間たちへの責任感。",
            Secret: "宮廷に仕える本当の理由は、死に戻りの装置が王都の地下に眠っていることを知っており、" +
                    "それが悪用されないよう監視しているため。"
        ),
        new(
            Id: "npc_lena",
            Name: "リーナ・ハーヴェスト",
            Title: "放浪の治療師",
            Race: "人間",
            Age: "27歳",
            Personality: "明るく行動的。困っている者を見ると迷わず助ける。" +
                         "ただし、立場の強い者への反発心が強く、貴族や権威者には刺々しくなることがある。",
            Background: "元貴族の娘。医術を「女のすることではない」と禁じた父に反発して家出した。" +
                        "独学で医術と錬金術を習得し、各地を放浪しながら助けを必要とする者を治療する。",
            Motivation: "医術の力で命を救うこと。父への反発と、本当に必要とされる場所を探す旅。",
            Secret: "父が送り込んだ刺客に追われている。また、父の医術研究を盗んできたという後ろめたさがある。"
        ),
        new(
            Id: "npc_carina",
            Name: "カリーナ・ムーンウォッチ",
            Title: "占い師",
            Race: "ハーフリング",
            Age: "55歳（外見は若い）",
            Personality: "神秘的な雰囲気を演出するのが得意。実際は皮肉屋でユーモアがある。" +
                         "人の本質を見抜く洞察力が高い。",
            Background: "星の動きと古代の占術を組み合わせた独自の占いで名を馳せた。" +
                        "一度も外れたことがない予言は、実は論理的分析と情報収集の産物。",
            Motivation: "人々を良い方向に導くこと。占いが「外れた」と思われたくないプライド。",
            Secret: "予言は本物の超能力ではなく、広大な情報網と優秀な分析力によるもの。" +
                    "しかし一度だけ——本当に「見えた」体験があり、それが彼女を占いに向かわせた。"
        ),
        new(
            Id: "npc_wolf",
            Name: "ヴォルフ・アイゼン",
            Title: "辺境自警団長",
            Race: "人間",
            Age: "42歳",
            Personality: "無口で行動力があり、余計な言葉を好まない。ただし、信頼した者には深い忠誠心を示す。",
            Background: "元傭兵。多くの戦場を経験した後、辺境の村を守るために自警団を設立した。" +
                        "かつての仲間の多くが死んだ経験から、命を軽視することを最も嫌う。",
            Motivation: "辺境の民を守ること。戦えない者を戦火に巻き込まないこと。",
            Secret: "若い頃に戦争犯罪に加担した過去を持つ。その罪滅ぼしとして辺境の守護者になった。"
        ),
    };

    /// <summary>
    /// NPCのIDからプロフィールを取得する
    /// </summary>
    public static NpcProfile? GetProfile(string npcId)
        => AllNpcProfiles.FirstOrDefault(p => p.Id == npcId);

    // =========================================================
    // α.7: NPC関係性テキスト
    // =========================================================

    /// <summary>
    /// 素性・宗教・周回数等に応じたNPC反応差分テキストを取得する
    /// </summary>
    public static string GetRelationshipText(string npcId, Background playerBackground, int loopCount = 0)
    {
        string loopPrefix = loopCount > 0
            ? $"（{loopCount}回目）"
            : "";

        return npcId switch
        {
            "npc_leon" => (playerBackground, loopCount) switch
            {
                (_, 0) => "レオンは穏やかな笑顔で迎える。「初めて会う。いや——どちらでもいい」",
                (_, 1) => $"{loopPrefix}レオン「また来たね。今回はどこまで行けるか、楽しみだ」",
                (Background.Scholar, _) => $"{loopPrefix}レオン「学者として、この仕組みを解明しようとしているのかい？実はそれが一番正しい向き合い方かもしれない」",
                (Background.Priest, _) => $"{loopPrefix}レオン「神への祈りがきみを守るかどうかは、きみ自身が証明することだ。でも信仰は——確かに力になる」",
                _ => $"{loopPrefix}レオン「また会えた。さあ——今回の旅を始めよう」"
            },

            "npc_marco" => playerBackground switch
            {
                Background.Merchant => "「おや、同業者！商売の話ならいくらでも。情報料はサービスするよ」",
                Background.Criminal => "「……まあ、過去のことは聞かないよ。私も聞かれたくないことがある。お互い様だ」",
                Background.Noble => "「ご貴族様が珍しい場所に。財布の中身が心配なら、良い投資先を教えましょうか？」",
                _ => "「旅人かい？良い品があるよ。見ていきな」"
            },

            "npc_albert" => playerBackground switch
            {
                Background.Scholar => "「ほう、学者か！一緒に研究しよう。君の知見が聞きたい」",
                Background.Wanderer => "「旅人は様々な場所の知識を持つ。君が見てきた遺跡や碑文を教えてくれないか」",
                _ => "「魔法言語に興味があるなら、教えてあげましょう。ただし真剣に取り組む気があるなら」"
            },

            "npc_elwen" => playerBackground switch
            {
                Background.Scholar => "エルウェンが少し目を細めた。「知識への渇望は——美しいものね。でも、知りすぎることへの恐れも忘れずに」",
                Background.Adventurer => "「冒険者か。レオンに世話になっているのね。彼のことを何か知りたければ——少しなら話せる」",
                _ => "エルウェンは静かに頭を下げた。「何かお力になれることがあれば、遠慮なく」"
            },

            _ => loopCount > 0
                ? $"（{loopCount}回目）{npcId}と再会した。"
                : $"{npcId}と出会った。"
        };
    }

    /// <summary>
    /// 宗教に応じたNPC台詞差分を取得する
    /// </summary>
    public static string GetReligionReactionText(string npcId, ReligionId religion) => (npcId, religion) switch
    {
        ("npc_sara", ReligionId.LightTemple) =>
            "「光の神を信仰しておられるのですね。祝福されますよう、共に祈りましょう」",
        ("npc_sara", ReligionId.DarkCult) =>
            "「……闇の神の信者か。私は歓迎の立場ではないが、神殿に来た者を追い返すことはしない」",
        ("npc_lena", ReligionId.NatureWorship) =>
            "「命の恵みを信仰しているなら、私の治療術にも興味があるでしょう。一緒に学びませんか」",
        ("npc_hassan", ReligionId.ChaosCult) =>
            "「おや、混沌を好む者同士か。これは縁だ。少し価格を下げてあげましょう」",
        _ => ""
    };

    // =========================================================
    // α.8: 商人・ショップNPCの台詞テキスト
    // =========================================================

    /// <summary>
    /// マルコ（放浪商人）の状況別台詞を取得する
    /// </summary>
    public static string GetMarcoDialogue(string situation, int loopCount = 0) => situation switch
    {
        "greeting" => loopCount switch
        {
            0 => "「いらっしゃい！遠路はるばる、良いものを揃えているよ」",
            1 => "「また会えたね！今回も良い品を持ってきたよ」",
            _ => $"「{loopCount}度目だね。もう常連じゃないか。特別に良いものを見せよう」"
        },

        "buy" => "「賢い選択だ。この品の価値はわかる人だけにわかる」",
        "sell" => "「なるほど……これは買い取れるね。悪くない品だ」",
        "no_buy" => "「そうか、縁がなかったね。また来なよ——生きていればの話だが」",
        "dangerous_area" => "「こんな場所で商売してるのかって？危険なところほど希少な品があるんだよ。リスクと利益は比例する」",
        "farewell" => "「気をつけてね。死んだら次の取引ができない——冗談だよ、気をつけて」",
        "secret_stock" => "「……実は特別な品がある。こんなことは滅多に言わないんだが、きみになら見せよう」",

        // 商品別台詞
        "item_potion" => "「良い薬草から丁寧に作られた回復薬だ。飲み頃の良さも保証するよ」",
        "item_weapon" => "「職人が一本一本手作りした武器だ。工場製とはひと味違う。もちろん値段も違うけど」",
        "item_info" => "「情報も商品——そう思わない？これは特別価格でお教えしよう」",
        "item_rare" => "「これは……正規ルートでは手に入らない品だ。出所を聞かないことを前提に」",

        _ => "「何か御用があれば」"
    };

    /// <summary>
    /// 一般ショップNPCの台詞を取得する（領地別）
    /// </summary>
    public static string GetShopDialogue(TerritoryId territory, string situation) =>
        (territory, situation) switch
        {
            (TerritoryId.Capital, "greeting") =>
                "「いらっしゃいませ。王都一の品揃えを誇る我が店に、ようこそ」",
            (TerritoryId.Capital, "buy") =>
                "「王都の品質に間違いはありません。ご満足いただけるはずです」",

            (TerritoryId.Forest, "greeting") =>
                "「森の恵みを扱う店へようこそ。エルフの職人が作った品も揃えています」",
            (TerritoryId.Forest, "rare_item") =>
                "「これはエルフの長老が使った素材から作られた特別な品です。扱いにはご注意を」",

            (TerritoryId.Mountain, "greeting") =>
                "「ドワーフの職人街にようこそ。鍛冶の技は本物だ。試してみな」",
            (TerritoryId.Mountain, "buy") =>
                "「ドワーフ製の武器は重いが、その分頑丈だ。長く使えるぞ」",

            (TerritoryId.Coast, "greeting") =>
                "「港のショップへ。海を越えた珍しい品も扱ってるよ」",
            (TerritoryId.Coast, "rare_item") =>
                "「これは沖の島から来た品だ……どこの島かは聞かないでくれ」",

            (TerritoryId.Southern, "greeting") =>
                "「南部の市場にようこそ。香辛料から魔法素材まで何でも揃う」",
            (TerritoryId.Southern, "bargain") =>
                "「値引き交渉？……面白い。少し気に入った。では特別に」",

            (TerritoryId.Frontier, "greeting") =>
                "「……何の用だ。生死に関わる品しか置いてないぞ」",
            (TerritoryId.Frontier, "buy") =>
                "「これを使えば少しは生き延びられる。少し——だけどな」",
            (TerritoryId.Frontier, "farewell") =>
                "「死ぬなよ。客が減るから」",

            _ => "「いらっしゃいませ」"
        };
}
