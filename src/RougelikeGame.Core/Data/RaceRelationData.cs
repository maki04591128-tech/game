namespace RougelikeGame.Core.Data;

/// <summary>
/// α.32: 種族間関係テキスト
/// α.33: 領地間関係テキスト
/// </summary>
public static class RaceRelationData
{
    // =========================================================
    // α.32: 種族間関係テキスト
    // =========================================================

    /// <summary>
    /// 種族に応じたNPCの反応テキストを取得する
    /// </summary>
    public static string GetRaceReactionText(string npcId, Race playerRace) =>
        (npcId, playerRace) switch
        {
            // エルフNPC → 他種族への反応
            ("npc_elwen", Race.Human) =>
                "エルウェン「人間か。短命だけど、その分濃密に生きる種族ね。嫌いじゃないわ」",
            ("npc_elwen", Race.Elf) =>
                "エルウェン「同族か。こんな場所で会えるとは……故郷はまだ変わらない？」",
            ("npc_elwen", Race.Dwarf) =>
                "エルウェン「ドワーフ……仕方ない。過去のことは忘れましょう。今は協力できる」",
            ("npc_elwen", Race.Orc) =>
                "エルウェン「オークが来るとは珍しい。……見かけで判断するのは嫌だけど、少し警戒している」",
            ("npc_elwen", Race.Beastfolk) =>
                "エルウェン「獣人ね。森の近くで生きてきた同士、わかりあえることがあるかもしれない」",
            ("npc_elwen", Race.Undead) =>
                "エルウェン「アンデッド……生死の境を超えた存在。研究対象として興味がある。もちろん友人としても」",

            // ドワーフNPC → 他種族への反応
            ("npc_gor", Race.Dwarf) =>
                "「同族か！ 故郷の訛りがある。あそこの鉄鉱石は最高だったな」",
            ("npc_gor", Race.Elf) =>
                "「……エルフか。まあ、仕事の話をするだけなら構わんが。昔のことは持ち出すな」",
            ("npc_gor", Race.Human) =>
                "「人間か。鍛冶の技を教えてやろうか？ まあ、お前らに扱えるかは保証しないが」",
            ("npc_gor", Race.Orc) =>
                "「オークか。力仕事なら任せるが、細工は任せるな——冗談だ。仕事をしよう」",

            // 汎用エルフ反応
            (_, Race.Elf) when npcId.StartsWith("npc_") =>
                "「エルフか。森の民だな。長命の種族は見方が違うと聞くが——どうだ？」",

            // 汎用ドワーフ反応
            (_, Race.Dwarf) when npcId.StartsWith("npc_") =>
                "「ドワーフか。鍛冶の腕は本物だろう。頼もしい旅人だ」",

            // 汎用オーク反応
            (_, Race.Orc) when npcId.StartsWith("npc_") =>
                "「オークか……見た目に驚いた。でも話せば違うかもしれない。用件を聞こう」",

            // 汎用アンデッド反応
            (_, Race.Undead) when npcId.StartsWith("npc_") =>
                "「アンデッド……生きているのか、死んでいるのか。どちらにせよ、話が通じるならそれでいい」",

            // 汎用ハーフリング反応
            (_, Race.Halfling) when npcId.StartsWith("npc_") =>
                "「小さな体で大きな冒険か。ハーフリングは運が強いと聞く——本当か？」",

            // 汎用悪魔反応
            (_, Race.Demon) when npcId.StartsWith("npc_") =>
                "「悪魔か！ ……落ち着け。動かない。で——何の用だ？」",

            _ => ""
        };

    /// <summary>
    /// 種族間対立・友好関係の説明テキストを取得する
    /// </summary>
    public static string GetRaceRelationDescription(Race raceA, Race raceB) =>
        (raceA, raceB) switch
        {
            (Race.Elf, Race.Dwarf) or (Race.Dwarf, Race.Elf) =>
                "エルフとドワーフは古くから対立してきた。森の民と地の民——価値観の根本が違う。" +
                "しかし、共通の敵を前に、かつて共闘した歴史もある。",

            (Race.Human, Race.Orc) or (Race.Orc, Race.Human) =>
                "人間とオークの関係は複雑だ。かつては戦争を繰り返し、今も偏見は残る。" +
                "しかし、オーク個人は必ずしも好戦的ではなく、都市で平和に暮らす者も多い。",

            (Race.Elf, Race.Beastfolk) or (Race.Beastfolk, Race.Elf) =>
                "エルフと獣人は森を共有することが多く、比較的友好的な関係を築いている。" +
                "自然を敬うという共通の価値観が両者をつなぐ。",

            (Race.Dwarf, Race.Human) or (Race.Human, Race.Dwarf) =>
                "ドワーフと人間は商取引で繋がることが多い。ドワーフの鍛冶技術を人間が求め、" +
                "人間の農産物をドワーフが必要とする。利害が一致する実用的な関係。",

            (Race.Human, Race.Halfling) or (Race.Halfling, Race.Human) =>
                "人間社会の中でハーフリングは比較的受け入れられている。" +
                "小さな体と陽気な性格が警戒心を和らげ、街角の商人として親しまれることが多い。",

            (Race.Human, Race.Undead) or (Race.Undead, Race.Human) =>
                "アンデッドは人間社会では異端だ。死の気配が本能的な恐怖を呼び起こす。" +
                "しかし理性を持つアンデッドが少数ながら存在し、差別と共存の間で揺れている。",

            (Race.Elf, Race.Undead) or (Race.Undead, Race.Elf) =>
                "長命のエルフはアンデッドに対して独特の見方をする。" +
                "生と死の境界を哲学的に捉えるエルフは、アンデッドを単純に排除しない。",

            _ => $"{raceA}と{raceB}の関係は複雑で、地域や個人によって大きく異なる。"
        };

    /// <summary>
    /// 種族別の挨拶・慣用表現を取得する
    /// </summary>
    public static string GetRacialGreeting(Race race) => race switch
    {
        Race.Human => "「こんにちは。旅のご無事をお祈りします」",
        Race.Elf => "「アシェア・ミルア（風よ、導きたまえ）——人の言葉に翻訳するとそういう意味だ」",
        Race.Dwarf => "「グルンド・ハル（鉄の意志と共に）——挨拶だ」",
        Race.Orc => "「ガル・トゥーク（強き者よ）——これが我らの挨拶だ」",
        Race.Beastfolk => "「同じ風の下で会えた——それが縁だ」",
        Race.Halfling => "「今日も良い日になりますよ！ ハーフリングの挨拶はそれだけだ」",
        Race.Undead => "（言葉の代わりに静かな頷き）",
        Race.Demon => "（低い唸り声と鋭い視線——これが敵意ではない表現だとわかるまで時間がかかった）",
        Race.FallenAngel => "「かつて天に仕えた。今は——違う場所にいる。あなたとは違う理由で」",
        Race.Slime => "（表面が波打つ——これが友好的な表現らしい）",
        _ => "「……」"
    };

    // =========================================================
    // α.33: 領地間関係テキスト
    // =========================================================

    /// <summary>
    /// 領地の政治的関係・状況テキストを取得する
    /// </summary>
    public static string GetTerritoryRelationText(TerritoryId territoryA, TerritoryId territoryB) =>
        (territoryA, territoryB) switch
        {
            (TerritoryId.Capital, TerritoryId.Mountain) or (TerritoryId.Mountain, TerritoryId.Capital) =>
                "王都とドワーフ山岳部は鉱山資源をめぐる交易で強く結びついている。" +
                "王都は武器・鎧の原材料を山岳部に依存しており、ドワーフは王都の市場なしに経済が成立しない。" +
                "表向きは対等な取引だが、実際は王都側が優位に立っていると言われている。",

            (TerritoryId.Capital, TerritoryId.Forest) or (TerritoryId.Forest, TerritoryId.Capital) =>
                "王都と森林地帯の関係は微妙だ。エルフは王国の統治に名目上従っているが、" +
                "森の奥深くは実質的にエルフの自治区となっている。" +
                "薬草・木材の供給で繋がるが、王都の伐採政策を巡り摩擦が絶えない。",

            (TerritoryId.Capital, TerritoryId.Coast) or (TerritoryId.Coast, TerritoryId.Capital) =>
                "王都と海岸部は海上交易の中継地として密接に繋がっている。" +
                "海岸部の港は王国最大の貿易拠点であり、ここを押さえる商人ギルドが実質的な経済権力を握る。" +
                "王都はこの富を課税で得ており、対立よりも共存の関係。",

            (TerritoryId.Capital, TerritoryId.Southern) or (TerritoryId.Southern, TerritoryId.Capital) =>
                "南部領地は王都から遠く、統治の目が届きにくい。" +
                "多様な種族が混在し、独自の文化圏を形成している。" +
                "表向きは王国の版図だが、実際は「独立した部族連合」に近い政治状況。" +
                "王都との摩擦は定期的に発生する。",

            (TerritoryId.Capital, TerritoryId.Frontier) or (TerritoryId.Frontier, TerritoryId.Capital) =>
                "辺境地帯は王国の防衛線として機能しているが、王都の支援は薄い。" +
                "辺境の住民は「王都に守られていると思ったことはない」と言う者も多い。" +
                "モンスターの脅威が常態化しており、中央の外交とは別の論理で動いている。",

            (TerritoryId.Forest, TerritoryId.Mountain) or (TerritoryId.Mountain, TerritoryId.Forest) =>
                "エルフとドワーフの古くからの対立は、今も続く。" +
                "森と山の境界線を巡る領土争いは何百年も未解決のまま。" +
                "しかし近年、共通の脅威（モンスターの大規模移動）を前に、局所的な協力関係が生まれている。",

            (TerritoryId.Coast, TerritoryId.Southern) or (TerritoryId.Southern, TerritoryId.Coast) =>
                "海岸部と南部は海上ルートで繋がっており、交易関係は活発だ。" +
                "南部の香辛料・魔法素材が海岸部で外国商品と交換される。" +
                "この交易路を管理する密輸組織の存在も噂されている。",

            (TerritoryId.Mountain, TerritoryId.Frontier) or (TerritoryId.Frontier, TerritoryId.Mountain) =>
                "ドワーフ山岳部と辺境は地続きであり、ドワーフ戦士が傭兵として辺境防衛に参加することも多い。" +
                "金銭的な結びつきが強く、ドワーフにとって辺境は「商売相手兼戦場」の認識。",

            _ => "それぞれの領地は独自の利害を持ち、関係は複雑に絡み合っている。"
        };

    /// <summary>
    /// プレイヤーの出身領地に応じたNPC反応テキストを取得する
    /// </summary>
    public static string GetTerritoryNpcReaction(string npcId, TerritoryId playerOrigin) =>
        (npcId, playerOrigin) switch
        {
            ("npc_marvin", TerritoryId.Capital) =>
                "「王都の出身か。なら規則はわかっているだろう。ギルドのルールも同じだ」",
            ("npc_marvin", TerritoryId.Frontier) =>
                "「辺境から来たのか！ それは大変だったな。そういう経歴の冒険者は頼もしい」",
            ("npc_marvin", TerritoryId.Forest) =>
                "「森の出身か。エルフ文化の影響を受けているなら、魔法系の依頼が向いているかも」",

            ("npc_marco", TerritoryId.Coast) =>
                "「海岸育ちか！ 商売の嗅覚があるはずだ。仕入れの話をしよう」",
            ("npc_marco", TerritoryId.Southern) =>
                "「南部の出身か。あそこの市場は面白い。顔が利く人と繋がっているか？」",

            (_, TerritoryId.Frontier) =>
                "「辺境から来たのか。……あそこを生き延びてきたなら、相当の使い手だ」",
            (_, TerritoryId.Capital) =>
                "「王都の人間か。上品な話し方をするね」",

            _ => ""
        };

    /// <summary>
    /// 領地の噂・政治情報テキストを取得する
    /// </summary>
    public static string GetTerritoryRumorText(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital =>
            "「王都で最近、謎の失踪事件が続いているらしい。夜に出歩いた市民が朝になると消えている——" +
            "衛兵は調査しているが進展がないとか」",

        TerritoryId.Forest =>
            "「森の奥でエルフの長老会議が開かれたらしい。何か重大な決定が下されたと聞いたが、" +
            "内容はエルフ以外には知らされていない」",

        TerritoryId.Mountain =>
            "「ドワーフの主要鉱山で落盤事故があったとか。表向きは事故だが、" +
            "何者かによる意図的な破壊という説もある」",

        TerritoryId.Coast =>
            "「港に正体不明の船が現れ、夜に荷物を降ろして消えた。密輸か、それとも別の目的か——" +
            "調べようとした港湾局員が行方不明になったらしい」",

        TerritoryId.Southern =>
            "「南部の族長が急死した。後継者問題で各部族が揉めているとか。" +
            "外部からの干渉を受けやすい状況で、王都がこれを利用しようとしているという噂もある」",

        TerritoryId.Frontier =>
            "「辺境でモンスターの大規模な群れが確認された。単独行動ではなく、統率された動きだという。" +
            "誰か——あるいは何かが——モンスターを指揮しているのかもしれない」",

        _ => "「特に目立った情報はない」"
    };
}
