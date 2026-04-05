namespace RougelikeGame.Core.Systems;

/// <summary>
/// マルチエンディングシステム - カルマ/死に戻り回数によるエンディング分岐
/// </summary>
public static class MultiEndingSystem
{
    /// <summary>エンディング判定結果</summary>
    public record EndingResult(
        EndingType Type,
        string Title,
        string Description
    );

    /// <summary>エンディングを判定</summary>
    public static EndingResult DetermineEnding(
        bool hasClearedFinalBoss,
        int totalDeaths,
        int karmaValue,
        bool allTerritoriesVisited,
        string clearRank)
    {
        // DG-1: 真エンディング（高ランク）を闇エンディングより優先
        // 真エンディング: 高ランク（S/A）でクリア
        if (hasClearedFinalBoss && clearRank is "S" or "A")
        {
            return new EndingResult(
                EndingType.True,
                "真の英雄",
                "深淵の王を完全に打ち倒し、世界に真の平和をもたらした。あなたの伝説は永遠に語り継がれる。"
            );
        }

        // 救済エンディング: 死に戻り0回+高カルマ
        if (hasClearedFinalBoss && totalDeaths == 0 && karmaValue >= 50)
        {
            return new EndingResult(
                EndingType.Salvation,
                "聖者の凱旋",
                "一度も倒れることなく、清き心で深淵の王を打ち倒した。あなたは真の救世主として讃えられた。"
            );
        }

        // 闇エンディング: カルマが極端に低い
        if (hasClearedFinalBoss && karmaValue <= -50)
        {
            return new EndingResult(
                EndingType.Dark,
                "闇の支配者",
                "深淵の王を倒したが、その力に呑まれた。あなた自身が新たな脅威となった。"
            );
        }

        // 放浪エンディング: クリアせず全領地踏破
        if (!hasClearedFinalBoss && allTerritoriesVisited)
        {
            return new EndingResult(
                EndingType.Wanderer,
                "果てなき旅路",
                "深淵の王には挑まず、全ての土地を巡る旅を選んだ。旅そのものがあなたの冒険だった。"
            );
        }

        // 正規エンディング
        if (hasClearedFinalBoss)
        {
            return new EndingResult(
                EndingType.Normal,
                "冒険の終わり",
                "深淵の王を打ち倒し、冒険を完遂した。平穏な日々が訪れた。"
            );
        }

        // 未クリア（通常到達しない）
        return new EndingResult(
            EndingType.Normal,
            "未完の旅",
            "冒険はまだ終わっていない。"
        );
    }

    /// <summary>エンディング種別名を取得</summary>
    public static string GetEndingTypeName(EndingType type) => type switch
    {
        EndingType.Normal => "正規エンディング",
        EndingType.True => "真エンディング",
        EndingType.Dark => "闇エンディング",
        EndingType.Salvation => "救済エンディング",
        EndingType.Wanderer => "放浪エンディング",
        _ => "不明"
    };

    /// <summary>エンディング解放条件の説明を取得</summary>
    public static IReadOnlyList<(EndingType Type, string Condition)> GetEndingConditions()
    {
        return new List<(EndingType, string)>
        {
            (EndingType.Normal, "30階ボスを撃破する"),
            (EndingType.True, "ランクS/Aでクリアする"),
            (EndingType.Dark, "カルマ-50以下でクリアする"),
            (EndingType.Salvation, "死亡0回+カルマ50以上でクリアする"),
            (EndingType.Wanderer, "全6領地を踏破する（ボス未撃破）"),
        };
    }
}
