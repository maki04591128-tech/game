using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.21: 6領地のロケーション描写テキスト
/// 到着時の雰囲気描写、主要ランドマーク、特色ある場所の説明
/// </summary>
public static class TerritoryLoreData
{
    /// <summary>α.21: 領地到着時のフレーバーテキストを取得</summary>
    public static string GetArrivalDescription(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital =>
            "【王都近郊】\n" +
            "広大な平原の先に、高い城壁が見える。\n" +
            "王都は常に旅人や商人で賑わい、様々な種族が行き交う。\n" +
            "大通りには商店が立ち並び、街の中心には輝く塔がそびえ立つ。\n" +
            "冒険者ギルドの本部もここにある。",

        TerritoryId.Forest =>
            "【古代の森】\n" +
            "鬱蒼とした森が視界を遮る。木々の間から差し込む光が幻想的だ。\n" +
            "深い緑の香りと、見知らぬ鳥の声が響く。\n" +
            "この森はエルフの聖域として知られ、古代の精霊が宿るとも言われている。\n" +
            "道を外れれば、帰り道を失う者も多い。",

        TerritoryId.Mountain =>
            "【霊峰の山脈】\n" +
            "切り立った岩山が連なり、厳しい寒気が肌を刺す。\n" +
            "山の中腹には鉱山と、それを守るドワーフの集落がある。\n" +
            "古くから「死を管理する神の住処」と呼ばれ、\n" +
            "謎めいた遺跡と洞窟が点在する地だ。",

        TerritoryId.Southern =>
            "【南方の港湾都市】\n" +
            "潮風が強く吹き、遠くに海が広がる。\n" +
            "港には各国の船が並び、異国の文化が混在する賑やかな街だ。\n" +
            "路地裏には怪しげな商人も多く、表と裏の顔を持つ都市として知られる。\n" +
            "夜になると海岸沿いに不思議な光が現れるという噂がある。",

        TerritoryId.Coast =>
            "【海岸の古都】\n" +
            "灼熱の海岸線に沿って古い街並みが続く。潮風が常に吹き、波の音が聞こえる。\n" +
            "かつてアルカナス帝国が栄えた地で、至る所に遺跡が散在している。\n" +
            "昼は耐えがたい暑さ、夜は凍えるような寒さと、過酷な環境が冒険者を試す。\n" +
            "砂の中に眠る財宝を求めて、今日も多くの者が命を落とす。",

        TerritoryId.Frontier =>
            "【辺境の荒野】\n" +
            "文明の届かない広大な荒野が広がる。\n" +
            "険しい地形と獰猛な魔物が住みつき、弱い者には生存を許さない。\n" +
            "この地に来る者は、追われた者か、極限の力を求める者だ。\n" +
            "混沌の崇拝者が多く住むとされ、夜空の下で奇妙な儀式が行われることもある。",

        _ => "見知らぬ土地に足を踏み入れた。"
    };

    /// <summary>α.21: 領地の特色説明（図鑑用）</summary>
    public static string GetTerritoryDetail(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital =>
            "王国の中心地。政治・経済・文化の中枢で、全ての道はここに続くと言われる。\n" +
            "光の神殿の本山があり、王国の守護神ソラリスへの信仰が最も篤い。\n" +
            "人口は最大で、商業・冒険者ギルド・魔術師連合の本部が集中する。\n" +
            "ダンジョン入口は王都の地下にあり、多くの冒険者が挑んでは消えていく。",

        TerritoryId.Forest =>
            "古代から続く神秘の森で、エルフの文明の中心地。\n" +
            "森の奥には世界樹と呼ばれる巨大な木があり、自然崇拝の聖地となっている。\n" +
            "薬草の宝庫であり、珍しい植物系素材が豊富に採取できる。\n" +
            "人間には見えない精霊道が走っており、迷い込んだ者は出口を見つけられないことがある。",

        TerritoryId.Mountain =>
            "鉱山資源が豊富な山岳地帯で、ドワーフ王国の本拠地。\n" +
            "良質な鉄鉱石とミスリル鉱脈が多く、鍛冶師たちにとっての聖地。\n" +
            "死神信仰が根強く、山中の洞窟には死神の祭壇が多く存在する。\n" +
            "高度が上がるほど気温が下がり、頂上付近は一年中雪に覆われている。",

        TerritoryId.Southern =>
            "海洋貿易の中継地点で、多様な種族と文化が混在する港湾都市。\n" +
            "闇の教団の影響が強く、夜の街では表と裏の取引が混在している。\n" +
            "深海の怪物の目撃情報が多く、漁師たちは夜の海を恐れている。\n" +
            "珍しい輸入品や希少な魔法素材が手に入るが、偽物も多い。",

        TerritoryId.Coast =>
            "古代アルカナス帝国の発祥の地で、砂漠全体が遺跡と言っても過言ではない。\n" +
            "文明の痕跡が至る所に残り、考古学者や遺跡探索者が集まる。\n" +
            "太陽の熱が魔法エネルギーを増幅させるため、炎系の魔法が特に強力になる地でもある。\n" +
            "砂嵐が視界を遮ることがあり、方向感覚を失った冒険者が多数行方不明になっている。",

        TerritoryId.Frontier =>
            "王国の支配が及ばない無法地帯。強者が弱者を支配する弱肉強食の世界。\n" +
            "混沌の崇拝者が多く住み、奇妙な変異を遂げた魔物が出現する。\n" +
            "文明の恩恵は少ないが、代わりに他の地では手に入らない希少品が流通している。\n" +
            "生き残ることができれば、強さと適応力が格段に向上するという。",

        _ => "情報なし"
    };

    /// <summary>α.21: 季節・時間帯に応じた領地の雰囲気テキスト</summary>
    public static string GetTimeOfDayDescription(TerritoryId territory, int hour) => hour switch
    {
        >= 5 and < 8 => GetDawnDescription(territory),
        >= 8 and < 18 => GetDaytimeDescription(territory),
        >= 18 and < 21 => GetDuskDescription(territory),
        _ => GetNightDescription(territory)
    };

    private static string GetDawnDescription(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => "夜明けの鐘が鳴り響く。商人たちが店を開く準備を始めている。",
        TerritoryId.Forest => "朝霧が森を包み込む。木漏れ日が霧を照らし、幻想的な景色が広がる。",
        TerritoryId.Mountain => "雪山の頂が朝日に染まり、金色に輝く。空気が澄んで遠くまで見える。",
        TerritoryId.Southern => "港に朝日が差し込み、波が黄金色に輝く。漁師たちが海に出ていく。",
        TerritoryId.Coast => "砂漠の地平線から太陽が昇る。砂の色が刻々と変わり、息をのむ美しさだ。",
        TerritoryId.Frontier => "荒野に夜明けが訪れる。昨夜生き延びたことを、静かに確認する朝だ。",
        _ => "夜明けが来た。新たな一日が始まる。"
    };

    private static string GetDaytimeDescription(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => "賑やかな市場の喧騒が聞こえる。街は活気に満ちている。",
        TerritoryId.Forest => "太陽が高くなり、森の内部にも光が届く。様々な魔物が活動している。",
        TerritoryId.Mountain => "峰から吹き下ろす風が冷たい。採掘の音が岩壁に反響する。",
        TerritoryId.Southern => "港は船の行き来で賑わっている。海風が心地よく吹く。",
        TerritoryId.Coast => "灼熱の太陽が大地を焼く。日陰のない場所は危険だ。",
        TerritoryId.Frontier => "荒野は変わらず危険だが、昼間は比較的安全だ。",
        _ => "日中の活動時間だ。"
    };

    private static string GetDuskDescription(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => "夕暮れの鐘が鳴り、人々が家路につく。街灯に火が灯り始めた。",
        TerritoryId.Forest => "夕暮れが近づくと、森の奥から不思議な音が聞こえ始める。夜の魔物が動き出す時間だ。",
        TerritoryId.Mountain => "山の影が長くなり、気温が急に下がり始める。夜営の準備をする時間だ。",
        TerritoryId.Southern => "夕日が海を赤く染める。港の居酒屋が賑わい始める時間だ。",
        TerritoryId.Coast => "陽が傾くと砂漠の色が変わり、昼間の灼熱が嘘のように涼しくなる。",
        TerritoryId.Frontier => "夕暮れになると、荒野に住む魔物が活発になる。宿を探すべき時間だ。",
        _ => "日没が近い。宿を探した方がいいかもしれない。"
    };

    private static string GetNightDescription(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => "夜の街は昼とは違う顔を持つ。闇に潜む者たちが動き始める。",
        TerritoryId.Forest => "深夜の森は危険だ。精霊の光が木々の間を舞い、迷い込んだ者の心を惑わす。",
        TerritoryId.Mountain => "満天の星が山の上を覆う。しかし暗闇の洞窟では、何かが蠢いている。",
        TerritoryId.Southern => "海岸沿いに不思議な青白い光が見える。漁師たちが怖れているものがそこにある。",
        TerritoryId.Coast => "砂漠の夜は凍えるほど寒い。しかし星空は他のどこよりも美しい。",
        TerritoryId.Frontier => "荒野の夜は最も危険だ。暗闇の中で、何かが近づいてくる気配がする。",
        _ => "夜になった。警戒を怠るな。"
    };
}
