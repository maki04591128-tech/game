namespace RougelikeGame.Core.Data;

/// <summary>
/// α.44: 季節別NPC台詞差分
/// α.45: 季節イベントテキスト
/// </summary>
public static class SeasonalEventData
{
    // =========================================================
    // α.44: 季節別NPC台詞差分
    // =========================================================

    /// <summary>
    /// 季節に応じたNPCの挨拶テキストを取得する
    /// </summary>
    public static string GetSeasonalNpcGreeting(string npcId, Season season) =>
        (npcId, season) switch
        {
            // レオン
            ("npc_leon", Season.Spring) =>
                "レオン「春だね。始まりの季節。君の旅も新しい章が始まる」",
            ("npc_leon", Season.Summer) =>
                "レオン「夏の暑さは、生きていることを実感させる。良いことだ」",
            ("npc_leon", Season.Autumn) =>
                "レオン「秋——収穫と終わりの季節。積み上げてきたものを確かめる時だ」",
            ("npc_leon", Season.Winter) =>
                "レオン「冬は静かだ。静かだからこそ、本当に大切なものが見える」",

            // マルコ
            ("npc_marco", Season.Spring) =>
                "「春の市場は活気があるよ！ 新鮮な情報も入荷中。見ていきなよ」",
            ("npc_marco", Season.Summer) =>
                "「夏の商売は体力勝負だ。でも旅人も多くてチャンスも多い！」",
            ("npc_marco", Season.Autumn) =>
                "「秋は仕入れの季節。良い品を揃えたよ。冬に備えておかないと」",
            ("npc_marco", Season.Winter) =>
                "「冬は商売が落ち着く……でも酒の需要は増える。ちゃんと仕入れてある」",

            // マーヴィン
            ("npc_marvin", Season.Spring) =>
                "「春は冒険依頼が増える季節です。皆が動き始める——良い時期ですよ」",
            ("npc_marvin", Season.Summer) =>
                "「夏は遠征依頼が多い。体力を使いますが、報酬も良い案件が多いです」",
            ("npc_marvin", Season.Autumn) =>
                "「秋は収穫期の護衛依頼が増えます。地道ですが安定した仕事です」",
            ("npc_marvin", Season.Winter) =>
                "「冬は依頼が減ります。でも逆に、リスクの高い依頼が来やすい時期でもある」",

            // ハッサン（闇市商人）
            ("npc_hassan", Season.Spring) =>
                "「春は人の動きが増える。嬉しいことだよ、客も増える」",
            ("npc_hassan", Season.Summer) =>
                "「夏の夜は長い。商売は夜に動く——この時期が一番忙しい」",
            ("npc_hassan", Season.Autumn) =>
                "「秋は品物が豊富になる。珍しいものが多く入ってくる時期だ」",
            ("npc_hassan", Season.Winter) =>
                "「冬は闇市の本番だ。寒さが人を必死にさせる。良い商売ができる」",

            // 汎用
            (_, Season.Spring) => $"「春ですね。良い季節だ。旅にも良い」",
            (_, Season.Summer) => $"「暑い夏ですね。水分をしっかり取ってくださいよ」",
            (_, Season.Autumn) => $"「秋になった。旅には良い季節——紅葉が美しい」",
            (_, Season.Winter) => $"「冬は厳しい。体に気をつけて」",
        };

    /// <summary>
    /// 季節ごとの一般的な雰囲気テキストを取得する
    /// </summary>
    public static string GetSeasonAtmosphereText(Season season, TerritoryId territory) =>
        (season, territory) switch
        {
            (Season.Spring, TerritoryId.Forest) =>
                "森が芽吹いている。新緑の光が木漏れ日となって降り注ぎ、どこからか鳥の歌が聞こえる。\n" +
                "エルフたちが何かの儀式を行っている様子——春の感謝祭だろうか。",

            (Season.Spring, TerritoryId.Capital) =>
                "王都の広場では春の祭りの準備が始まっている。\n" +
                "花飾りが街路に並び始め、子供たちが駆け回っている。\n" +
                "一年で最も活気ある季節の始まりだ。",

            (Season.Summer, TerritoryId.Coast) =>
                "強い日差しが海面を照らし、海が眩しく光っている。\n" +
                "港は活気があり、遠洋から戻った船が次々と荷を降ろしている。\n" +
                "夏の海は商売の季節だ。",

            (Season.Summer, TerritoryId.Southern) =>
                "南部の夏は厳しい。日中は外を歩くことができないほどの暑さになる。\n" +
                "人々は日陰に集まり、昼間は活動を最低限に抑えている。\n" +
                "夕方から夜にかけてが、この地での活動時間だ。",

            (Season.Autumn, TerritoryId.Mountain) =>
                "山の木々が赤と金に染まっている。\n" +
                "ドワーフたちは冬の備えで忙しそうだ——食料の貯蔵、設備の補強。\n" +
                "採掘の作業が加速している。",

            (Season.Autumn, TerritoryId.Forest) =>
                "森が色づいた。赤、橙、黄——エルフはこの季節を「森の夕暮れ」と呼ぶ。\n" +
                "木の実が豊富で、獣たちも活発だ。\n" +
                "狩りをするなら今が適期だが——深い森には近づくな。",

            (Season.Winter, TerritoryId.Frontier) =>
                "辺境の冬は命の問題だ。雪で道が塞がれ、補給が困難になる。\n" +
                "守備隊は最小限の人員で対応している。\n" +
                "モンスターは減るが——逆に飢えた獣が村に近づいてくる季節でもある。",

            (Season.Winter, TerritoryId.Capital) =>
                "王都が雪に覆われた。白い街並みが美しいが、寒さは厳しい。\n" +
                "暖炉のある宿が人気で、価格が上がっている。\n" +
                "冬の王都では慈善事業も活発になる——聖なる季節の影響か。",

            (Season.Spring, _) =>
                "春が来た。大地が目覚め、新しい命が生まれる季節。\n" +
                "旅に出る人が増え、街に活気が戻ってきた。",

            (Season.Summer, _) =>
                "夏の太陽が全てを照らしている。エネルギーに満ちた季節。\n" +
                "長い日照時間が冒険者にとっては有利でもあり、体力の消耗でもある。",

            (Season.Autumn, _) =>
                "秋の静けさの中に、冬への準備が始まっている。\n" +
                "実りの季節——戦利品も増えるが、気候も変わりやすくなる。",

            (Season.Winter, _) =>
                "冬——万物が静まる季節。しかし止まってはいられない。\n" +
                "寒さは敵でもあり、同時に弱い者を炙り出すフィルターでもある。",
        };

    // =========================================================
    // α.45: 季節イベントテキスト
    // =========================================================

    /// <summary>
    /// 季節限定イベントのテキストを取得する
    /// </summary>
    public static string GetSeasonalEventText(string eventId, Season season) =>
        (eventId, season) switch
        {
            // 春祭り
            ("spring_festival", Season.Spring) =>
                "【春の目覚め祭】\n" +
                "王都全体が賑わっている。広場では演奏が続き、街路には花が溢れている。\n" +
                "期間中は全商品が10%割引。ギルドの特別依頼も出現する。\n" +
                "NPC全員が機嫌良く、会話が弾む——年一度の特別な日だ。",

            // 夏の試練
            ("summer_trial", Season.Summer) =>
                "【剣士の夏試練】\n" +
                "夏の間だけ開催される武闘大会。各地の強者が集まってくる。\n" +
                "優勝すれば称号「夏の覇者」と特別な武器が授与される。\n" +
                "ただし怪我の責任は参加者自身が負う——覚悟して挑め。",

            // 秋の収穫祭
            ("harvest_festival", Season.Autumn) =>
                "【収穫の感謝祭】\n" +
                "農村地帯で開催される。食料の物々交換が盛んになる期間。\n" +
                "特産品が安価で手に入り、珍しい食材が出回る。\n" +
                "祭りの最終日には、「豊穣の守護者」への感謝の儀式が執り行われる。",

            // 冬の光祭
            ("winter_light_festival", Season.Winter) =>
                "【冬至の灯り祭】\n" +
                "最も長い夜に、全ての明かりを灯す祭り。\n" +
                "町中にランタンが飾られ、暗闇の中でも安全に歩ける。\n" +
                "この夜は亡くなった冒険者たちを悼む習慣があり、ギルドでは追悼式が行われる。",

            // 春の植樹祭（森林地帯）
            ("spring_planting", Season.Spring) =>
                "【緑の誓い祭——エルフの春祭り】\n" +
                "新しい木を植え、命の継続を誓う儀式。\n" +
                "参加した旅人にはエルフの感謝と、「森の加護」が一時的に付与される。\n" +
                "森の動物たちも平和的になり、この期間は攻撃してこない。",

            // 夏の嵐祭り（海岸地帯）
            ("summer_storm_rite", Season.Summer) =>
                "【嵐乗り越えの祭——海岸の夏祭り】\n" +
                "海の嵐を鎮めるための祈りの祭り。\n" +
                "航海する船乗りたちが神に祈りを捧げ、安全を願う。\n" +
                "祭りの夜には、海神への供物として財宝を海に投げる伝統がある。",

            // 秋の先祖祭（南部）
            ("ancestor_festival", Season.Autumn) =>
                "【先祖の声——南部の秋祭り】\n" +
                "先祖の霊が戻ってくると信じる南部の伝統祭り。\n" +
                "各家に先祖の写真や遺品が飾られ、夜通し語り合う。\n" +
                "この夜は亡霊との遭遇率が上がるが、悪意を持つものは少ない——先祖が守るからだ。",

            // 冬の炉端祭（山岳）
            ("winter_forge_festival", Season.Winter) =>
                "【炎の絆祭——ドワーフの冬祭り】\n" +
                "家族で炉端を囲み、一年の仕事を称え合う日。\n" +
                "ドワーフの鍛冶師たちがこの日だけ特別な品を作る——「炉端の剣」と呼ばれる。\n" +
                "外部者が参加するのは珍しいが、信頼関係があれば招かれることがある。",

            // 辺境の生存祭
            ("survival_festival", Season.Winter) =>
                "【生き残り祝い——辺境の冬祭り】\n" +
                "冬を生き延びたことを祝う、質素だが心温まる祭り。\n" +
                "「また春が来た」という安堵が全員の顔にある。\n" +
                "この祭りに参加した者は、辺境の住民として認められる——それが最大の名誉だ。",

            _ => $"（{season}に関連するイベント情報はない）"
        };

    /// <summary>
    /// 季節による特殊効果のフレーバーテキストを取得する
    /// </summary>
    public static string GetSeasonalEffectText(Season season, string effectType) =>
        (season, effectType) switch
        {
            (Season.Spring, "weather_change") =>
                "春雨が降り始めた。視界が少し悪くなるが、空気が澄んで気持ちが良い。",
            (Season.Spring, "encounter_bonus") =>
                "春の気候に動物たちも活発だ。遭遇が増えるが、友好的な種類も多い。",

            (Season.Summer, "heat_fatigue") =>
                "夏の強烈な日差しが体力を奪う。屋外での行動は通常より疲労が蓄積しやすい。",
            (Season.Summer, "night_bonus") =>
                "夏の夜は短いが涼しい。夜間行動の疲労が軽減される。",

            (Season.Autumn, "harvest_bonus") =>
                "秋の収穫期。食料の入手が容易になり、市場での価格が下がる。",
            (Season.Autumn, "leaf_cover") =>
                "落ち葉が積もり、足音が立ちやすくなった。隠密行動が少し難しくなる。",

            (Season.Winter, "cold_damage") =>
                "冬の冷気が体に染みる。防寒装備がなければ行動するたびに体力が消耗する。",
            (Season.Winter, "visibility") =>
                "雪が視界を遮る。遠距離の敵が見えにくく、接近される前に察知しにくい。",

            _ => $"（{season}の{effectType}効果）"
        };

    /// <summary>
    /// 季節の変わり目テキストを取得する
    /// </summary>
    public static string GetSeasonTransitionText(Season fromSeason, Season toSeason) =>
        (fromSeason, toSeason) switch
        {
            (Season.Winter, Season.Spring) =>
                "長い冬が終わり、春の息吹が感じられるようになった。\n" +
                "雪が溶け始め、新芽が顔を出している。旅に良い季節が来た。",
            (Season.Spring, Season.Summer) =>
                "春の穏やかさが夏の活力に変わっていく。\n" +
                "日差しが強くなり、空気が乾いてきた。体力の管理に注意しよう。",
            (Season.Summer, Season.Autumn) =>
                "夏の暑さが和らぎ、秋の涼しさが漂い始めた。\n" +
                "木の葉が少しずつ色づいている。実りの季節が来る。",
            (Season.Autumn, Season.Winter) =>
                "秋の彩りが散り、灰色の冬が迫ってきた。\n" +
                "防寒の備えを整えろ。冬の旅は命取りになりうる。",
            _ => "季節が移り変わった。"
        };
}
