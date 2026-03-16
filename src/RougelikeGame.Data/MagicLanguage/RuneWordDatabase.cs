namespace RougelikeGame.Data.MagicLanguage;

/// <summary>
/// ルーン語（魔法言語の単語）の定義
/// </summary>
public record RuneWord(
    string Id,
    string OldNorse,        // 古ノルド語
    string Pronunciation,   // 発音（カタカナ）
    string Meaning,         // 日本語意味
    RuneWordCategory Category,
    int BaseMpCost,
    int BaseTurnCost,
    float PowerMultiplier = 1.0f,
    float MpMultiplier = 1.0f,
    int TurnCostAddition = 0,
    int Difficulty = 1      // 習得難度 1-5
);

/// <summary>
/// ルーン語カテゴリ
/// </summary>
public enum RuneWordCategory
{
    Effect,     // 効果語（Verkun）
    Target,     // 対象語（Mál）
    Element,    // 属性語（Náttúra）
    Modifier,   // 修飾語（Orðtak）
    Range,      // 範囲語（Svið）
    Duration,   // 時間語（Tíð）
    Condition   // 条件語（Skilyrði）
}

/// <summary>
/// ルーン語データベース
/// </summary>
public static class RuneWordDatabase
{
    private static readonly Dictionary<string, RuneWord> _words = new();

    static RuneWordDatabase()
    {
        InitializeEffectWords();
        InitializeTargetWords();
        InitializeElementWords();
        InitializeModifierWords();
        InitializeRangeWords();
        InitializeDurationWords();
        InitializeConditionWords();
    }

    private static void InitializeEffectWords()
    {
        // 攻撃系
        Add(new("brenna", "brenna", "ブレンナ", "燃やす", RuneWordCategory.Effect, 5, 5, Difficulty: 1));
        Add(new("frysta", "frysta", "フリュスタ", "凍らせる", RuneWordCategory.Effect, 6, 6, Difficulty: 1));
        Add(new("thruma", "þruma", "スルマ", "雷撃つ", RuneWordCategory.Effect, 5, 5, Difficulty: 1));
        Add(new("brjota", "brjóta", "ブリョータ", "砕く", RuneWordCategory.Effect, 5, 5, Difficulty: 1));
        Add(new("snida", "sníða", "スニーザ", "斬る", RuneWordCategory.Effect, 4, 4, Difficulty: 1));
        Add(new("stinga", "stinga", "スティンガ", "貫く", RuneWordCategory.Effect, 6, 6, Difficulty: 2));
        Add(new("springa", "springa", "スプリンガ", "爆ぜる", RuneWordCategory.Effect, 10, 8, Difficulty: 2));
        Add(new("tortima", "tortíma", "トルティーマ", "滅ぼす", RuneWordCategory.Effect, 20, 15, Difficulty: 4));
        Add(new("eyda", "eyða", "エイザ", "蝕む", RuneWordCategory.Effect, 8, 8, Difficulty: 3));
        Add(new("granda", "granda", "グランダ", "害する", RuneWordCategory.Effect, 15, 12, Difficulty: 4));

        // 回復・支援系
        Add(new("graeda", "græða", "グラエザ", "癒す", RuneWordCategory.Effect, 8, 6, Difficulty: 1));
        Add(new("hreinsa", "hreinsa", "フレインサ", "清める", RuneWordCategory.Effect, 6, 5, Difficulty: 2));
        Add(new("verja", "verja", "ヴェリャ", "守る", RuneWordCategory.Effect, 5, 5, Difficulty: 1));
        Add(new("vekja", "vekja", "ヴェキャ", "起こす", RuneWordCategory.Effect, 30, 20, Difficulty: 5));
        Add(new("styrkja", "styrkja", "スティルキャ", "強める", RuneWordCategory.Effect, 6, 5, Difficulty: 2));
        Add(new("hrada", "hraða", "フラザ", "速める", RuneWordCategory.Effect, 5, 4, Difficulty: 2));
        Add(new("hylja", "hylja", "ヒュリャ", "隠す", RuneWordCategory.Effect, 10, 8, Difficulty: 3));
        Add(new("blessa", "blessa", "ブレッサ", "祝福する", RuneWordCategory.Effect, 15, 12, Difficulty: 3));

        // 制御系
        Add(new("binda", "binda", "ビンダ", "縛る", RuneWordCategory.Effect, 7, 6, Difficulty: 2));
        Add(new("sofa", "sofa", "ソヴァ", "眠らせる", RuneWordCategory.Effect, 6, 5, Difficulty: 2));
        Add(new("villa", "villa", "ヴィッラ", "惑わす", RuneWordCategory.Effect, 8, 7, Difficulty: 3));
        Add(new("hraeda", "hræða", "フラエザ", "恐れさせる", RuneWordCategory.Effect, 7, 6, Difficulty: 2));
        Add(new("styra", "stýra", "スティーラ", "操る", RuneWordCategory.Effect, 15, 12, Difficulty: 4));
        Add(new("kalla", "kalla", "カッラ", "召喚する", RuneWordCategory.Effect, 12, 10, Difficulty: 3));
        Add(new("senda", "senda", "センダ", "送る", RuneWordCategory.Effect, 10, 8, Difficulty: 3));

        // 特殊系
        Add(new("sja", "sjá", "シャウ", "見る", RuneWordCategory.Effect, 3, 3, Difficulty: 1));
        Add(new("vita", "vita", "ヴィタ", "知る", RuneWordCategory.Effect, 5, 4, Difficulty: 2));
        Add(new("afrita", "afrita", "アフリタ", "写す", RuneWordCategory.Effect, 20, 15, Difficulty: 4));
        Add(new("snua", "snúa", "スヌーア", "逆さにする", RuneWordCategory.Effect, 25, 18, Difficulty: 5));
        Add(new("banna", "banna", "バンナ", "禁じる", RuneWordCategory.Effect, 15, 12, Difficulty: 4));
        Add(new("opna", "opna", "オプナ", "開く", RuneWordCategory.Effect, 8, 6, Difficulty: 2));
        Add(new("loka", "loka", "ロカ", "閉じる", RuneWordCategory.Effect, 8, 6, Difficulty: 2));
    }

    private static void InitializeTargetWords()
    {
        Add(new("sjalfr", "sjálfr", "シャウルヴル", "自分", RuneWordCategory.Target, 0, 0, Difficulty: 1));
        Add(new("fjandi", "fjandi", "フィヤンディ", "敵", RuneWordCategory.Target, 2, 0, Difficulty: 1));
        Add(new("ovinir", "óvinir", "オーヴィニル", "敵達", RuneWordCategory.Target, 5, 0, Difficulty: 2));
        Add(new("vinir", "vinir", "ヴィニル", "味方達", RuneWordCategory.Target, 5, 0, Difficulty: 2));
        Add(new("hlutr", "hlutr", "フルートル", "物", RuneWordCategory.Target, 1, 0, Difficulty: 1));
        Add(new("jord", "jörð", "ヨルズ", "大地", RuneWordCategory.Target, 2, 0, Difficulty: 1));
        Add(new("himinn", "himinn", "ヒミン", "天", RuneWordCategory.Target, 3, 0, Difficulty: 2));
        Add(new("allir", "allir", "アッリル", "全て", RuneWordCategory.Target, 10, 0, Difficulty: 3));
        Add(new("draugr", "draugr", "ドラウグル", "亡者", RuneWordCategory.Target, 3, 0, Difficulty: 2));
        Add(new("thurs", "þurs", "スルス", "巨人", RuneWordCategory.Target, 4, 0, Difficulty: 2));
    }

    private static void InitializeElementWords()
    {
        Add(new("eldr", "eldr", "エルドル", "炎", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("vatn", "vatn", "ヴァトン", "水", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("iss", "íss", "イース", "氷", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.1f, Difficulty: 2));
        Add(new("thruma_elem", "þruma", "スルマ", "雷", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.1f, Difficulty: 2));
        Add(new("jord_elem", "jörð", "ヨルズ", "土", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("vindr", "vindr", "ヴィンドル", "風", RuneWordCategory.Element, 0, 2, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("ljos", "ljós", "リョース", "光", RuneWordCategory.Element, 0, 3, MpMultiplier: 1.2f, Difficulty: 3));
        Add(new("myrkr", "myrkr", "ミュルクル", "闇", RuneWordCategory.Element, 0, 3, MpMultiplier: 1.2f, Difficulty: 3));
        Add(new("helgr", "helgr", "ヘルグル", "聖なる", RuneWordCategory.Element, 0, 3, MpMultiplier: 1.3f, Difficulty: 3));
        Add(new("bolvadr", "bölvaðr", "ボルヴァズル", "呪われた", RuneWordCategory.Element, 0, 3, MpMultiplier: 1.3f, Difficulty: 3));
    }

    private static void InitializeModifierWords()
    {
        // 強度修飾
        Add(new("litill", "lítill", "リーティル", "小さい", RuneWordCategory.Modifier, 0, 2, PowerMultiplier: 0.5f, MpMultiplier: 0.5f, Difficulty: 1));
        Add(new("medal", "meðal", "メザル", "中程度", RuneWordCategory.Modifier, 0, 3, PowerMultiplier: 0.7f, MpMultiplier: 0.7f, Difficulty: 1));
        Add(new("mikill", "mikill", "ミキル", "大きい", RuneWordCategory.Modifier, 0, 5, PowerMultiplier: 1.3f, MpMultiplier: 1.3f, Difficulty: 2));
        Add(new("sterkr", "sterkr", "ステルクル", "強い", RuneWordCategory.Modifier, 0, 6, PowerMultiplier: 1.5f, MpMultiplier: 1.5f, Difficulty: 2));
        Add(new("ofr", "ofr", "オフル", "超", RuneWordCategory.Modifier, 0, 8, PowerMultiplier: 2.0f, MpMultiplier: 2.0f, Difficulty: 3));
        Add(new("ragnarok", "ragnarök", "ラグナロク", "終末の", RuneWordCategory.Modifier, 0, 12, PowerMultiplier: 3.0f, MpMultiplier: 3.0f, Difficulty: 5));

        // 精度修飾
        Add(new("rett", "rétt", "レット", "正しく", RuneWordCategory.Modifier, 0, 3, MpMultiplier: 1.2f, Difficulty: 2));
        Add(new("viss", "víss", "ヴィース", "確実に", RuneWordCategory.Modifier, 0, 5, MpMultiplier: 1.5f, Difficulty: 3));

        // 速度修飾
        Add(new("skjotr", "skjótr", "スキョートル", "速い", RuneWordCategory.Modifier, 0, -3, MpMultiplier: 1.3f, Difficulty: 2));
        Add(new("hradr", "hraðr", "フラズル", "急速", RuneWordCategory.Modifier, 0, -5, MpMultiplier: 1.5f, Difficulty: 3));
        Add(new("thegar", "þegar", "セガル", "即座に", RuneWordCategory.Modifier, 0, -10, MpMultiplier: 2.0f, Difficulty: 4));
    }

    private static void InitializeRangeWords()
    {
        Add(new("einn", "einn", "エイン", "一点", RuneWordCategory.Range, 0, 0, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("beinn", "beinn", "ベイン", "直線", RuneWordCategory.Range, 0, 4, MpMultiplier: 1.3f, Difficulty: 2));
        Add(new("hringr", "hringr", "フリングル", "円", RuneWordCategory.Range, 0, 5, MpMultiplier: 1.5f, Difficulty: 2));
        Add(new("vidr", "víðr", "ヴィーズル", "広い", RuneWordCategory.Range, 0, 8, MpMultiplier: 2.0f, Difficulty: 3));
        Add(new("heimr", "heimr", "ヘイムル", "世界", RuneWordCategory.Range, 0, 12, MpMultiplier: 3.0f, Difficulty: 4));
    }

    private static void InitializeDurationWords()
    {
        Add(new("augnablik", "augnablik", "アウグナブリク", "瞬き", RuneWordCategory.Duration, 0, 0, MpMultiplier: 1.0f, Difficulty: 1));
        Add(new("stund", "stund", "ストゥンド", "時", RuneWordCategory.Duration, 0, 4, MpMultiplier: 1.3f, Difficulty: 2));
        Add(new("langr", "langr", "ラングル", "長い", RuneWordCategory.Duration, 0, 6, MpMultiplier: 1.5f, Difficulty: 2));
        Add(new("eilifr", "eilífr", "エイリーヴル", "永遠", RuneWordCategory.Duration, 0, 10, MpMultiplier: 2.0f, Difficulty: 3));
        Add(new("sidar", "síðar", "シーザル", "後で", RuneWordCategory.Duration, 0, 3, MpMultiplier: 0.8f, Difficulty: 2));
        Add(new("endalauss", "endalauss", "エンダラウス", "無限", RuneWordCategory.Duration, 0, 12, MpMultiplier: 2.5f, Difficulty: 4));
    }

    private static void InitializeConditionWords()
    {
        Add(new("ef", "ef", "エフ", "もし〜ならば", RuneWordCategory.Condition, 0, 3, MpMultiplier: 1.2f, Difficulty: 3));
        Add(new("tha", "þá", "サウ", "その時", RuneWordCategory.Condition, 0, 2, MpMultiplier: 1.1f, Difficulty: 2));
        Add(new("gegn", "gegn", "ゲグン", "〜に対して", RuneWordCategory.Condition, 0, 2, MpMultiplier: 1.1f, Difficulty: 2));
        Add(new("daudr", "dauðr", "ダウズル", "死に瀕した時", RuneWordCategory.Condition, 0, 5, MpMultiplier: 1.5f, Difficulty: 4));
        Add(new("sar", "sár", "サウル", "傷ついた時", RuneWordCategory.Condition, 0, 3, MpMultiplier: 1.2f, Difficulty: 3));
    }

    private static void Add(RuneWord word)
    {
        _words[word.Id] = word;
    }

    public static RuneWord? GetById(string id) =>
        _words.TryGetValue(id, out var word) ? word : null;

    public static IEnumerable<RuneWord> GetByCategory(RuneWordCategory category) =>
        _words.Values.Where(w => w.Category == category);

    public static IEnumerable<RuneWord> GetAll() => _words.Values;

    public static int Count => _words.Count;
}
