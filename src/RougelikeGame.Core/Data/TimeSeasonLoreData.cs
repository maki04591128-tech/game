using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.26: ゲーム内時間・季節描写テキスト
/// 時間帯/季節/天候に応じた雰囲気テキスト（朝霧、夜の静寂等）
/// </summary>
public static class TimeSeasonLoreData
{
    /// <summary>
    /// 時間帯ごとの雰囲気テキストを取得する（ランダムバリエーション付き）
    /// </summary>
    public static string GetTimePeriodText(TimePeriod time, int variant = 0) => (time, variant % 3) switch
    {
        // 早朝（Dawn）
        (TimePeriod.Dawn, 0) =>
            "東の空が白み始めた。鳥の声が最初の一音を奏で、世界が静かに目を覚ます時間だ。",
        (TimePeriod.Dawn, 1) =>
            "夜明け前の冷気が肌を刺す。地平線に橙色の帯が現れ、闇が少しずつ押しやられていく。",
        (TimePeriod.Dawn, _) =>
            "薄明の光が山の稜線を縁取る。この時間帯は何かが動き始める——感じる者だけが知る、一日の始まりの予感。",

        // 午前（Morning）
        (TimePeriod.Morning, 0) =>
            "朝の光が大地に降り注ぎ、露が草の先で光を弾いている。街では人々の活動が始まった。",
        (TimePeriod.Morning, 1) =>
            "空は青く澄み渡っている。朝市の喧騒や鍛冶屋の音が遠くから聞こえてくる。",
        (TimePeriod.Morning, _) =>
            "清々しい朝だ。体が軽く、頭が冴えている。長い旅の始まりには最適な時間帯。",

        // 午後（Afternoon）
        (TimePeriod.Afternoon, 0) =>
            "太陽は中天を過ぎ、暑さが最高潮に達している。木陰では人も獣も昼寝をしているころだ。",
        (TimePeriod.Afternoon, 1) =>
            "日差しが強烈で、砂埃が舞う。体力の消耗が激しいが、視界は最も開けている時間。",
        (TimePeriod.Afternoon, _) =>
            "穏やかな午後。風が枝を揺らし、木漏れ日が地面に模様を作る。旅には快適だが、眠気に注意。",

        // 夕方（Dusk）
        (TimePeriod.Dusk, 0) =>
            "西の空が燃えるように赤く染まった。影が長く伸び、夜への移行が始まっている。",
        (TimePeriod.Dusk, 1) =>
            "夕暮れ時。街では灯りが次々と灯り、人々が帰路につく。旅人にとっては宿を探す時間だ。",
        (TimePeriod.Dusk, _) =>
            "夕風が涼しい。赤から紫へと変わる空の色が美しい。だが、この美しさが長続きしないことは誰もが知っている。",

        // 夜（Night）
        (TimePeriod.Night, 0) =>
            "夜の帳が降りた。星々が瞬き、月が銀色の光を地に落としている。夜行性の魔物が動き始める時間だ。",
        (TimePeriod.Night, 1) =>
            "暗闇の中、遠くで狼の遠吠えが聞こえる。夜は危険だが、夜にしか動かない存在もいる。",
        (TimePeriod.Night, _) =>
            "静寂の夜。篝火の光が揺れ、影が踊る。昼間は見えなかったものが、闇の中で姿を現す。",

        // 深夜（Midnight）
        (TimePeriod.Midnight, 0) =>
            "深夜の静けさが世界を覆っている。時折風が吹くだけで、生き物の気配すらない。",
        (TimePeriod.Midnight, 1) =>
            "月が中天に達した。この時間、魔力の流れが最も活発になると言われる。賢者たちが深夜に儀式を行うのも頷ける。",
        (TimePeriod.Midnight, _) =>
            "丑三つ時。幽霊話が最も多い時間帯だ。正気を保った状態でここを歩いていることが、すでに一つの達成だ。",

        _ => "時間帯不明"
    };

    /// <summary>
    /// 季節ごとの雰囲気テキストを取得する
    /// </summary>
    public static string GetSeasonText(Season season, int variant = 0) => (season, variant % 3) switch
    {
        // 春
        (Season.Spring, 0) =>
            "春の訪れと共に、世界が色づき始めた。新芽が地を割り、鳥たちの歌声が戻ってきた。",
        (Season.Spring, 1) =>
            "桜に似た花々が空に舞い、甘い香りが辺りを包んでいる。春の嵐が来る前の、つかの間の穏やかさ。",
        (Season.Spring, _) =>
            "雪解け水が小川を満たし、大地の生命が目を覚ます季節。魔物たちも活性化し始めている。",

        // 夏
        (Season.Summer, 0) =>
            "燦々と降り注ぐ陽光の下、全てが輝いて見える。しかし、この熱さは体力を容赦なく奪っていく。",
        (Season.Summer, 1) =>
            "蝉の声が耳を劈く。木の葉は鮮やかな緑を湛え、遠くには入道雲が頭をもたげている。",
        (Season.Summer, _) =>
            "夏の最中、熱波が大地を焦がす。水の補給が特に重要な季節だ。",

        // 秋
        (Season.Autumn, 0) =>
            "木の葉が赤や黄に色づき、風に乗って舞い散る。実りの季節であると同時に、死への準備の季節でもある。",
        (Season.Autumn, 1) =>
            "秋晴れの空は高く澄んでいる。収穫祭の賑わいが遠くから聞こえてくる。",
        (Season.Autumn, _) =>
            "枯れ葉が積もる道を歩く。踏み締めるたびに乾いた音がして、どこか寂しさを感じる。",

        // 冬
        (Season.Winter, 0) =>
            "雪が大地を白く覆い、世界は静寂の中にある。冷え込みは厳しいが、雪景色の美しさに息をのむ。",
        (Season.Winter, 1) =>
            "凍てつく寒さが全身を刺す。吐く息が白く、指先が痛い。暖かな宿が懐かしくなる季節だ。",
        (Season.Winter, _) =>
            "吹雪の夜、視界は数歩先も見えない。自然の猛威の前に、人の力など微力だと思い知る。",

        _ => "季節不明"
    };

    /// <summary>
    /// 天候ごとのテキストを取得する
    /// </summary>
    public static string GetWeatherText(Weather weather, int variant = 0) => (weather, variant % 3) switch
    {
        // 晴れ
        (Weather.Clear, 0) => "澄んだ青空が広がっている。旅には絶好の日和だ。",
        (Weather.Clear, 1) => "太陽が燦々と輝き、視界は申し分ない。",
        (Weather.Clear, _) => "風もなく穏やかな晴天。長距離の移動に適した天気だ。",

        // 雨
        (Weather.Rain, 0) => "雨が降り始めた。足元が滑りやすくなり、視界も悪化する。",
        (Weather.Rain, 1) => "雨粒が地面を叩く音が絶え間ない。雨に濡れると体力が奪われる。",
        (Weather.Rain, _) => "霧雨が肌を濡らす。遠くの景色は霞み、気配が分かりにくくなっている。",

        // 霧
        (Weather.Fog, 0) => "濃い霧が辺りを包んでいる。数歩先しか見えない。奇襲を受ける危険がある。",
        (Weather.Fog, 1) => "霧の中では方向感覚が狂う。慎重に進まないと迷子になりかねない。",
        (Weather.Fog, _) => "白い霧が静かに漂っている。この霧の中に何が潜んでいるか、想像するだけで背筋が寒くなる。",

        // 雪
        (Weather.Snow, 0) => "雪が降り始めた。足跡が雪に残り、追跡が容易になる——される側にとっては危険だ。",
        (Weather.Snow, 1) => "厚く積もった雪が足を取られる。移動速度が落ちるが、足音は消える。",
        (Weather.Snow, _) => "しんしんと降る雪が世界を静寂に包む。この静けさが、時に恐ろしい。",

        // 嵐
        (Weather.Storm, 0) => "嵐が迫っている。強風が木々を揺らし、雨が横殴りに叩きつける。",
        (Weather.Storm, 1) => "稲妻が空を裂き、雷鳴が大地を揺るがす。嵐の中での戦闘は命取りだ。",
        (Weather.Storm, _) => "これほどの嵐は久しぶりだ。自然の怒りを前に、人は無力だと改めて感じる。",

        _ => "天候不明"
    };

    /// <summary>
    /// 季節×時間帯の組み合わせテキスト（特定の組み合わせで特別なテキスト）
    /// </summary>
    public static string? GetSpecialComboText(Season season, TimePeriod time) => (season, time) switch
    {
        (Season.Spring, TimePeriod.Dawn) =>
            "春の夜明け。朝霧の中に桜の花びらが舞い、幻想的な景色が広がっている。" +
            "この美しさは長くは続かない——だからこそ、今この瞬間が尊い。",

        (Season.Summer, TimePeriod.Night) =>
            "夏の夜、虫の声が賑やかに響く。熱帯夜の蒸し暑さの中でも、星は美しく輝いている。" +
            "冒険者たちは夜でも野営をして旅を続けることが多い。",

        (Season.Autumn, TimePeriod.Dusk) =>
            "秋の夕暮れ。赤く染まった空と紅葉が重なり、まるで世界が燃えているようだ。" +
            "この季節、死の気配が濃くなると古い伝承は語る。",

        (Season.Winter, TimePeriod.Midnight) =>
            "冬の深夜。凍てつく星空の下、雪は静かに降り続ける。" +
            "古くから、冬の深夜は幽霊が最も活発になる時間とされている。",

        _ => null // 特別なテキストなし
    };

    /// <summary>
    /// フィールド移動時の季節・時間帯コメントを取得する（短い版）
    /// </summary>
    public static string GetTravelComment(Season season, TimePeriod time, Weather weather) =>
        weather switch
        {
            Weather.Storm => "嵐の中、行く手が見えない。",
            Weather.Fog => "霧が深く、道を見失いそうだ。",
            Weather.Snow when season == Season.Winter => "吹雪の中を歩き続ける。寒さが骨に染みる。",
            Weather.Rain => "雨の中、黙々と足を進める。",
            _ => (season, time) switch
            {
                (Season.Spring, _) => "春の風が心地よく吹き抜けていく。",
                (Season.Summer, TimePeriod.Afternoon) => "太陽が容赦なく照りつける。水分補給を忘れずに。",
                (Season.Summer, _) => "夏の大気が体を包む。",
                (Season.Autumn, _) => "秋風と共に枯れ葉が舞う。",
                (Season.Winter, TimePeriod.Morning) => "凍てつく朝の空気が肺に刺さる。",
                (Season.Winter, _) => "冬の冷気が服の間から忍び込む。",
                (_, TimePeriod.Dawn) => "夜明けの光の中を歩く。",
                (_, TimePeriod.Night) => "夜道は危険だ。足音を立てずに進む。",
                (_, TimePeriod.Midnight) => "深夜の静寂が、不安を煽る。",
                _ => "旅を続ける。"
            }
        };
}
