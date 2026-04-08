using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.24: ダンジョンイベントテキスト
/// 宝箱/泉/遺跡/NPC遭遇等のダンジョン内イベント描写テキスト
/// </summary>
public static class DungeonEventLoreData
{
    /// <summary>
    /// ランダムイベントの発見テキストを取得する（プレイヤーが場所を発見した時）
    /// </summary>
    public static string GetDiscoveryText(RandomEventType eventType, int seed = 0) => eventType switch
    {
        RandomEventType.TreasureChest => GetTreasureDiscovery(seed % 4),
        RandomEventType.Fountain => GetFountainDiscovery(seed % 3),
        RandomEventType.Shrine => GetShrineDiscovery(seed % 3),
        RandomEventType.Ruins => GetRuinsDiscovery(seed % 4),
        RandomEventType.NpcEncounter => GetNpcEncounterDiscovery(seed % 3),
        RandomEventType.MerchantEncounter => "旅の商人に出会った。この深部で商売をしているとは……",
        RandomEventType.RestPoint => GetRestPointDiscovery(seed % 3),
        RandomEventType.MysteriousItem => "怪しい光を放つものが落ちている。これは……いったい何だろう？",
        RandomEventType.MonsterHouse => "何かが違う。この部屋、息が詰まるほど何かの気配が充満している……",
        RandomEventType.CursedRoom => "全身に悪寒が走った。ここは穢れている……立ち去った方がいいかもしれない。",
        RandomEventType.BlessedRoom => "清らかな空気が漂う。聖なる加護があるかのような静謐さだ。",
        RandomEventType.HiddenShop => "壁の奥に隠し扉を発見した。中からかすかに光が漏れている。",
        RandomEventType.AmbushEvent => "——来た。殺気を感じた瞬間、四方から包囲される！",
        RandomEventType.Trap => GetTrapDiscovery(seed % 3),
        _ => "何かを発見した。"
    };

    /// <summary>
    /// ランダムイベントの調査テキストを取得する（プレイヤーがアクションを起こした時）
    /// </summary>
    public static string GetInteractText(RandomEventType eventType, int seed = 0) => eventType switch
    {
        RandomEventType.TreasureChest => GetTreasureInteract(seed % 3),
        RandomEventType.Fountain => GetFountainInteract(seed % 3),
        RandomEventType.Shrine => GetShrineInteract(seed % 3),
        RandomEventType.Ruins => GetRuinsInteract(seed % 4),
        RandomEventType.NpcEncounter => GetNpcEncounterInteract(seed % 3),
        RandomEventType.RestPoint => GetRestPointInteract(seed % 3),
        RandomEventType.MysteriousItem => "手に取ってみると、不思議な温もりを感じる。何か力が宿っているようだ。",
        RandomEventType.MerchantEncounter => "「やあ、旅人。ここまで来るとは大した者だ。良い品を見せよう」",
        RandomEventType.BlessedRoom => "部屋の中心で祈りを捧げると、温かな光が包んでくれた。体が軽くなる。",
        _ => "何も起きなかった。"
    };

    // --------- 宝箱 ---------
    private static string GetTreasureDiscovery(int variant) => variant switch
    {
        0 => "薄暗がりの中、何かが光を反射した。……宝箱だ！埃を被っているが、まだ開いていない様子。",
        1 => "壁の隙間に押し込まれていた古い木箱を発見した。錠前がかかっているが、中に価値ある物が入っているかもしれない。",
        2 => "骸骨の傍らに、半ば土に埋まった鉄製の箱があった。この冒険者は道半ばで力尽きたのだろうか……",
        _ => "宝箱を発見した。"
    };

    private static string GetTreasureInteract(int variant) => variant switch
    {
        0 => "錠前を外すと、蝶番が軋んで箱が開いた。中には……！",
        1 => "長年の錆で固まった蓋を力づくでこじ開けた。積もった埃が舞い上がり、光が中身を照らす。",
        _ => "宝箱を開けた。"
    };

    // --------- 泉・噴水 ---------
    private static string GetFountainDiscovery(int variant) => variant switch
    {
        0 => "清らかな水が石造りの噴水から湧き出している。こんな深部にあるとは不思議だ。飲んでも大丈夫だろうか……",
        1 => "苔むした泉の底から、淡い光が揺らめいている。魔力を帯びた水のように見える。",
        _ => "不思議な泉を発見した。"
    };

    private static string GetFountainInteract(int variant) => variant switch
    {
        0 => "水に口をつけると、冷たく澄んだ液体が喉を通った。不思議と疲れが和らいでいく……",
        1 => "泉に触れた瞬間、温かな光が体を包んだ。傷や毒が中和されていくのを感じる。",
        2 => "水面に己の顔が映っている。……飲むと、なぜか体の深いところから何かが回復した気がした。",
        _ => "泉の水を飲んだ。"
    };

    // --------- 祠・神殿 ---------
    private static string GetShrineDiscovery(int variant) => variant switch
    {
        0 => "小さな祠が立っている。長年誰も訪れていないのか、供え物の跡だけが残っている。どの神への祠だろうか。",
        1 => "壁に彫られた神の像の前に、石製の台座があった。かつては何かが祀られていたのかもしれない。",
        _ => "古い祠を発見した。"
    };

    private static string GetShrineInteract(int variant) => variant switch
    {
        0 => "祈りを捧げると、微かに光が灯った。この神はまだ、ここに存在しているようだ。",
        1 => "台座に手を置くと、ぞくりと鳥肌が立った。何かが——繋がった、ような気がする。",
        2 => "無言で頭を下げた。しばらくすると、どこからともなく温かな風が吹いてきた。",
        _ => "祠に祈りを捧げた。"
    };

    // --------- 遺跡・廃墟 ---------
    private static string GetRuinsDiscovery(int variant) => variant switch
    {
        0 => "崩れかけた石造りの建物の跡がある。かつてここに文明があったのだろうか。壁には古い文字が刻まれている。",
        1 => "巨大な門の残骸が道を塞いでいる。かつての栄光を偲ばせる精巧な彫刻が施されているが、時の流れが無情に削り取っている。",
        2 => "地下に続く階段を見つけた。目の前の遺跡から続いているようだ。深みから冷たい空気が流れてくる。",
        3 => "砕け散った石碑が散らばっている。かつての碑文を繋ぎ合わせると、何かが読めそうだ……",
        _ => "遺跡の跡を発見した。"
    };

    private static string GetRuinsInteract(int variant) => variant switch
    {
        0 => "壁の文字を解読しようとした。古代語で何かが書かれている。「……この先は——」。残りは読み取れなかった。",
        1 => "遺跡を調べると、隠し扉や宝物の痕跡が見つかった。ここには何か価値があるものが残っているかもしれない。",
        2 => "崩れた床の下に何かが埋まっていた。丁寧に掘り起こすと……！",
        3 => "石碑のピースを組み合わせると、断片的に言葉が浮かび上がった。「偉大なる王の名のもとに……繁栄は永劫に……」",
        _ => "遺跡を調べた。"
    };

    // --------- NPC遭遇 ---------
    private static string GetNpcEncounterDiscovery(int variant) => variant switch
    {
        0 => "人の気配がする。こんな深部に自分以外の者がいるとは……敵か、それとも——",
        1 => "壁際に座り込んだ人影を発見した。怪我をしているのか、動けない様子だ。",
        _ => "誰かに出会った。"
    };

    private static string GetNpcEncounterInteract(int variant) => variant switch
    {
        0 => "声をかけると、相手はゆっくりと振り返った。目が合う。どちらも武器に手を掛けず、ひとまず敵意がないことを確認し合った。",
        1 => "「……助かった。ここで死ぬかと思っていたよ」彼（彼女）は弱々しく笑った。",
        2 => "「あんたも冒険者か？ここは危ない。一緒に行動しようじゃないか」",
        _ => "NPCと会話した。"
    };

    // --------- 休憩ポイント ---------
    private static string GetRestPointDiscovery(int variant) => variant switch
    {
        0 => "岩の窪みが天然のシェルターを作っている。中は外よりも温かく、しばらく休める環境だ。",
        1 => "先人の冒険者が残したテントの跡があった。薪の残骸と簡素な寝具。誰かがここで一晩を過ごしたのだろう。",
        2 => "ランタンの明かりが灯った小部屋があった。魔法で維持されているようで、不思議と安心感がある。",
        _ => "休憩できそうな場所を見つけた。"
    };

    private static string GetRestPointInteract(int variant) => variant switch
    {
        0 => "腰を下ろし、しばらく目を閉じた。水と乾燥した食料を少し口に入れ、体力を回復する。",
        1 => "体を横たえると、あっという間に意識が遠のいた……どのくらい眠っただろう。起き上がると体が軽くなっていた。",
        _ => "休憩した。"
    };

    // --------- 罠 ---------
    private static string GetTrapDiscovery(int variant) => variant switch
    {
        0 => "床に不自然な模様がある。踏み石の配置が変だ……いや、これは罠だ！",
        1 => "細い糸が膝の高さに張られているのを、ギリギリのところで気づいた。何かに繋がっている。",
        2 => "壁の小さな穴を発見した。矢罠か何かだろうか？慎重に進む必要がある。",
        _ => "罠を発見した。"
    };

    /// <summary>
    /// 宝箱のランク別開封テキストを取得する
    /// </summary>
    public static string GetTreasureOpenText(int rarity) => rarity switch
    {
        1 => "古びた木箱を開けると、使い古した道具と少量の金貨が入っていた。まあ、ないよりはマシか。",
        2 => "錠前を外すと、中から品質の良い装備品と金貨が現れた。冒険者の血が沸き立つ！",
        3 => "蓋を開けた瞬間、眩い光が溢れ出した。……これは！めったにお目にかかれない代物だ。",
        4 => "開封した途端、魔力の奔流が溢れ出た。伝説の——いや、これほどのものが本当に存在していたとは！",
        _ => "宝箱を開けた。"
    };

    /// <summary>
    /// 罠発動テキストを取得する（種類別）
    /// </summary>
    public static string GetTrapActivateText(string trapType) => trapType switch
    {
        "arrow" => "ガチン！壁の穴から矢が飛び出した！",
        "pit" => "足元の床が崩れた！咄嗟に体を捻ったが——落下してしまった！",
        "poison_gas" => "見えない毒ガスが噴き出した！息苦しくなり、視界が霞む……",
        "alarm" => "大きな音が響き渡った！近くの魔物が集まってくるかもしれない！",
        "flame" => "炎が噴き出した！熱波が全身を焼く！",
        "electric" => "床から電撃が走った！体が痺れて動きが鈍くなる！",
        _ => "罠が発動した！"
    };

    /// <summary>
    /// 泉・噴水の種類別効果テキストを取得する
    /// </summary>
    public static string GetFountainEffectText(string fountainType) => fountainType switch
    {
        "healing" => "水を口に含むと、傷口がじわじわと癒えていく感覚があった。HPが回復した。",
        "mana" => "泉の水に触れると、体内の魔力が満ちていく。MPが回復した。",
        "stamina" => "水を飲み干すと体が軽くなった。長旅の疲れが嘘のように消えた。SPが回復した。",
        "curse" => "水を飲んだ途端、口の中に苦みが広がった……何か変なものを飲んでしまったかもしれない。",
        "strength" => "泉の水は不思議な味がした。しばらくすると、体中の力が増した気がする。",
        "wisdom" => "水面に映る自分の姿を眺めていると、不意に何かを悟った気がした。",
        _ => "泉の水を飲んだ。何かが変わった気がする。"
    };
}
