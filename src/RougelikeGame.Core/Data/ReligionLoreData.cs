using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Data;

/// <summary>
/// α.13-16: 宗教テキストコンテンツ - 6宗教の教義・経典・フレーバーテキスト
/// </summary>
public static class ReligionLoreData
{
    /// <summary>α.13: 宗教ごとの教義テキストを取得</summary>
    public static string GetDoctrine(ReligionId id) => id switch
    {
        ReligionId.LightTemple =>
            "【光の神殿 教義】\n" +
            "「闇を照らすは光の使命、光を守るは信者の務め」\n\n" +
            "太陽神ソラリスの教えは3つの柱からなる。\n" +
            "・慈悲：苦しむ者を助け、傷ついた者を癒せ。\n" +
            "・正義：悪を前にして沈黙してはならない。\n" +
            "・清廉：心身ともに清らかであれ。\n\n" +
            "経典『太陽の書』第一章より:\n" +
            "「暁とともに目覚め、祈りを捧げよ。\n" +
            "　夕暮れに感謝を述べ、眠れ。\n" +
            "　太陽の恵みは、信じる者すべてに注がれる」",

        ReligionId.DarkCult =>
            "【闇の教団 教義】\n" +
            "「闇は弱さではない。それは力の本質である」\n\n" +
            "深淵神ニュクスの教えは知識と力を至上とする。\n" +
            "・秘密：知識は力。他者に与えるな。\n" +
            "・力への意志：弱者は消える。強者のみが残る。\n" +
            "・深淵への傾倒：恐れずに闇の中を歩け。\n\n" +
            "秘典『深淵の書』第七章より:\n" +
            "「光は見えるものを照らすが、\n" +
            "　闇は見えないものを明かす。\n" +
            "　真の知者は闇の中にこそ真実を見る」",

        ReligionId.NatureWorship =>
            "【自然崇拝 教義】\n" +
            "「すべては循環する。大地に還り、再び芽吹く」\n\n" +
            "大地母神ガイアの教えは共生と循環を基とする。\n" +
            "・共生：自然と共に生き、奪いすぎてはならない。\n" +
            "・循環：死は終わりではなく、始まりである。\n" +
            "・感謝：大地の恵みに常に感謝せよ。\n\n" +
            "聖歌『緑の讃歌』より:\n" +
            "「川は流れ、木は育ち、\n" +
            "　風は歌い、大地は謳う。\n" +
            "　我らはその一部、永遠の命の輪の中にある」",

        ReligionId.DeathFaith =>
            "【死神信仰 教義】\n" +
            "「死を恐れるな。それはただの扉に過ぎない」\n\n" +
            "死神タナトスの教えは死の必然性と受容を説く。\n" +
            "・不可避：全ての命は死に向かっている。抗うな。\n" +
            "・管理：死者の魂は導かれるべきだ。邪魔するな。\n" +
            "・死の尊重：死体を冒涜してはならない。\n\n" +
            "死典『彼岸の書』第十三章より:\n" +
            "「生とは死への旅路。\n" +
            "　美しく生きることは、美しく死ぬことである。\n" +
            "　タナトスは審判者ではなく、案内者である」",

        ReligionId.ChaosCult =>
            "【混沌の崇拝 教義】\n" +
            "「規則など幻だ。真実は混沌の中にある」\n\n" +
            "混沌神カオスの教えは変化と予測不能性を讃える。\n" +
            "・変化：同じことを繰り返すな。変われ。\n" +
            "・破壊と創造：壊れたものからこそ新しいものが生まれる。\n" +
            "・自由：いかなる秩序にも縛られるな。\n\n" +
            "混沌の祈り:\n" +
            "「昨日の自分は死んだ。\n" +
            "　明日の自分は知らない。\n" +
            "　今この混沌の中にこそ、神がいる」",

        ReligionId.Atheism =>
            "【無神論 理念】\n" +
            "「神は人が作った物語。力は自分の中にある」\n\n" +
            "無神論者の信念は自立と理性を基とする。\n" +
            "・自己責任：結果は全て自分の選択の産物だ。\n" +
            "・理性：感情ではなく論理で判断せよ。\n" +
            "・実力主義：神の加護など頼らず、己の力を磨け。\n\n" +
            "無神論者の格言:\n" +
            "「神に祈る時間があるなら、剣を磨け。\n" +
            "　天に頼る前に、自分の足で立て。\n" +
            "　運命は自分で切り開くものだ」",

        _ => "教義情報なし"
    };

    /// <summary>α.14: 入信テキストを取得</summary>
    public static string GetInitiationText(ReligionId id) => id switch
    {
        ReligionId.LightTemple =>
            "光の神殿の司祭が聖水を額に注ぐ。\n" +
            "「太陽神ソラリスの名において、あなたを光の信者として迎えます。\n" +
            "　慈悲と正義の心を持ち、常に光ある道を歩んでください」\n" +
            "温かい光が体を包む感覚がした。",

        ReligionId.DarkCult =>
            "暗闇の中、仮面の祭司が囁く。\n" +
            "「深淵神ニュクスがあなたを認めた。\n" +
            "　光の届かない場所でこそ、真の力は目覚める。\n" +
            "　闇を恐れるな。あなたはもう闇の一部だ」\n" +
            "影が自分に宿ったような感覚がした。",

        ReligionId.NatureWorship =>
            "古木の根元で、エルフの長老が詠唱する。\n" +
            "「大地母神ガイアよ、この魂を受け入れたまえ。\n" +
            "　自然の一部として生き、自然の循環に従え」\n" +
            "足元の土から温かいエネルギーが流れてきた。",

        ReligionId.DeathFaith =>
            "骸骨の仮面をつけた司祭が低く唱える。\n" +
            "「死神タナトスの名において、あなたの死を受け入れよ。\n" +
            "　死を恐れる必要はない。それはただの道の続きだ」\n" +
            "死の概念を受け入れた瞬間、不思議な平静が訪れた。",

        ReligionId.ChaosCult =>
            "混沌の祭壇の前で、何かが囁く。\n" +
            "「変われ、壊れろ、混沌に溶けろ。\n" +
            "　カオスはあなたを待っていた。\n" +
            "　固定された自己などない。ただ混沌があるのみ」\n" +
            "世界が一瞬歪んで見えた。",

        _ => "神の名のもとに、新たな道が始まる。"
    };

    /// <summary>α.14: 改宗テキストを取得</summary>
    public static string GetConversionText(ReligionId fromId, ReligionId toId)
    {
        if (fromId == ReligionId.Atheism) return GetInitiationText(toId);
        return $"「{GetReligionShortName(fromId)}」から「{GetReligionShortName(toId)}」へ改宗する。\n" +
               "かつての信仰を捨て、新たな道を歩む。\n" +
               "前の神への背信は、時に痛みを伴うことがある。\n\n" +
               GetInitiationText(toId);
    }

    /// <summary>α.15: 司祭NPCの挨拶テキストを取得（信仰度に応じた変化）</summary>
    public static string GetPriestGreeting(ReligionId id, int faithPoints) => id switch
    {
        ReligionId.LightTemple when faithPoints >= 61 =>
            "ようこそ、司祭よ。太陽神の加護があなたに宿っています。\n信仰の証として、特別な祝福を与えましょう。",
        ReligionId.LightTemple when faithPoints >= 21 =>
            "光の信者よ、よく来ました。\n今日も神の御心に従い、善き行いを積んでください。",
        ReligionId.LightTemple =>
            "神の祝福がありますように。迷える者にも、光は必ず届きます。",

        ReligionId.DarkCult when faithPoints >= 61 =>
            "…久しいな、深淵の従者よ。\n闇の主があなたのさらなる力を認めている。",
        ReligionId.DarkCult when faithPoints >= 21 =>
            "…闇の中へようこそ。あなたの信仰は本物だ。力を授けよう。",
        ReligionId.DarkCult =>
            "…何者だ。ここは好奇心本位の場所ではない。",

        ReligionId.NatureWorship when faithPoints >= 61 =>
            "大地の声が聞こえるか？自然はあなたを仲間として認めている。",
        ReligionId.NatureWorship =>
            "大地の恵みに感謝を。自然と共に生きることで、人は本来の力を取り戻す。",

        ReligionId.DeathFaith when faithPoints >= 61 =>
            "死を受け入れた者よ。タナトスはあなたに特別な役割を与えたようだ。",
        ReligionId.DeathFaith =>
            "死を恐れるか？恐れることはない。それはただの通過点に過ぎない。",

        ReligionId.ChaosCult =>
            "くくく…また来たか。混沌は変わり続ける。あなたも変わったか？",

        _ => "何を求めてここへ来た？"
    };

    /// <summary>α.16: 恩恵発動時のフレーバーテキスト</summary>
    public static string GetBenefitActivationText(ReligionId id, string benefitName) => id switch
    {
        ReligionId.LightTemple => $"太陽神ソラリスの加護が輝く！【{benefitName}】",
        ReligionId.DarkCult => $"深淵の力が呼応する…【{benefitName}】",
        ReligionId.NatureWorship => $"大地の息吹が満ちる。【{benefitName}】",
        ReligionId.DeathFaith => $"タナトスの目が向けられた。【{benefitName}】",
        ReligionId.ChaosCult => $"混沌が弾ける！【{benefitName}】",
        _ => $"【{benefitName}】が発動した。"
    };

    /// <summary>α.16: 禁忌違反時のフレーバーテキスト</summary>
    public static string GetTabooViolationText(ReligionId id, string tabooName) => id switch
    {
        ReligionId.LightTemple => $"太陽神ソラリスの怒りが降り注ぐ！禁忌「{tabooName}」を破った！",
        ReligionId.DarkCult => $"深淵の眼が冷たく見つめる…禁忌「{tabooName}」を犯した。",
        ReligionId.NatureWorship => $"大地が震え、自然が嘆く！禁忌「{tabooName}」を破った！",
        ReligionId.DeathFaith => $"タナトスの鎌が揺れた。禁忌「{tabooName}」を犯した。",
        ReligionId.ChaosCult => $"混沌が乱れる！禁忌「{tabooName}」違反。",
        _ => $"禁忌「{tabooName}」を破った！"
    };

    private static string GetReligionShortName(ReligionId id) => id switch
    {
        ReligionId.LightTemple => "光の神殿",
        ReligionId.DarkCult => "闇の教団",
        ReligionId.NatureWorship => "自然崇拝",
        ReligionId.DeathFaith => "死神信仰",
        ReligionId.ChaosCult => "混沌の崇拝",
        ReligionId.Atheism => "無神論",
        _ => "不明な宗教"
    };
}
