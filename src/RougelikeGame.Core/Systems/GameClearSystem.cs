namespace RougelikeGame.Core.Systems;

/// <summary>
/// ゲームクリアシステム - クリア条件判定と結果管理
/// </summary>
public static class GameClearSystem
{
    /// <summary>クリア結果</summary>
    public record ClearResult(
        string BackgroundName,
        int TotalTurns,
        int TotalDeaths,
        int HighestLevel,
        string ClearCondition,
        bool IsFirstClear
    );

    /// <summary>クリアスコア</summary>
    public record ClearScore(
        int TotalScore,
        string Rank,
        int TurnBonus,
        int DeathPenalty,
        int LevelBonus,
        int FloorBonus
    );

    /// <summary>素性別クリア条件を取得</summary>
    public static string GetClearCondition(Background background) => background switch
    {
        Background.Noble => "王都の復興を成し遂げる",
        Background.Merchant => "莫大な富を築く",
        Background.Criminal => "全ての罪を贖う",
        Background.Scholar => "究極の知識を得る",
        Background.Soldier => "魔王を討伐する",
        Background.Peasant => "豊かな大地を取り戻す",
        Background.Priest => "聖地を巡礼する",
        Background.Penitent => "全ての罪を贖い救済を得る",
        Background.Wanderer => "真の故郷を見つける",
        Background.Adventurer => "伝説の冒険者となる",
        _ => "冒険を完遂する"
    };

    /// <summary>クリアランクを判定</summary>
    public static string GetClearRank(int totalTurns, int totalDeaths) => (totalTurns, totalDeaths) switch
    {
        ( < 5000, 0) => "S",
        ( < 10000, <= 3) => "A",
        ( < 20000, <= 10) => "B",
        ( < 50000, _) => "C",
        _ => "D"
    };

    /// <summary>クリアメッセージを取得</summary>
    public static string GetClearMessage(string backgroundName, string rank)
    {
        return $"【{backgroundName}】としての冒険を完遂した！ ランク: {rank}";
    }

    /// <summary>NG+解放条件を満たしたか判定</summary>
    public static bool UnlocksNewGamePlus(string rank)
    {
        return rank is "S" or "A" or "B";
    }

    /// <summary>クリアスコアを計算</summary>
    public static ClearScore CalculateScore(int totalTurns, int totalDeaths, int highestLevel, int maxFloor)
    {
        // ターンボーナス（少ないほど高得点）
        int turnBonus = Math.Max(0, 10000 - totalTurns / 5);
        // 死亡ペナルティ
        int deathPenalty = totalDeaths * 500;
        // レベルボーナス
        int levelBonus = highestLevel * 200;
        // フロアボーナス
        int floorBonus = maxFloor * 100;

        int totalScore = Math.Max(0, turnBonus - deathPenalty + levelBonus + floorBonus);
        string rank = GetClearRank(totalTurns, totalDeaths);

        return new ClearScore(totalScore, rank, turnBonus, deathPenalty, levelBonus, floorBonus);
    }

    /// <summary>30階ボス撃破がゲームクリア条件か判定</summary>
    public static bool IsFinalBossDefeated(int currentFloor, string defeatedEnemyTypeId)
    {
        return currentFloor >= 30 && defeatedEnemyTypeId == "floor_boss_30";
    }

    /// <summary>素性別クリアテキストを取得</summary>
    public static string GetClearText(Background background) => background switch
    {
        Background.Noble => "あなたは深淵の王を打ち倒し、王都に真の平和をもたらした。\n高貴なる血筋は、新たな時代の礎となった。",
        Background.Merchant => "深淵の王の財宝を手に入れ、あなたは伝説の大商人となった。\nその富は七つの海を越えて語り継がれた。",
        Background.Criminal => "深淵の王を倒したことで、あなたの罪は赦された。\nかつての罪人は、今や英雄として民に慕われている。",
        Background.Scholar => "深淵の王の知識を解き明かし、あなたは究極の叡智を得た。\n賢者の書には、新たな一章が加えられた。",
        Background.Soldier => "見事に魔王を討伐した。あなたの武勇は永久に語り継がれる。\n最強の戦士の名は、歴史に刻まれた。",
        Background.Peasant => "深淵の王を倒し、大地は再び肥沃さを取り戻した。\nかつての農民は、今や大地の守護者と呼ばれている。",
        Background.Priest => "深淵の王を浄化し、聖なる巡礼を完遂した。\nあなたの信仰は、永遠の光となって輝いている。",
        Background.Penitent => "深淵の王を倒し、全ての罪は贖われた。\n苦行の果てに、あなたは真の救済を手にした。",
        Background.Wanderer => "深淵の王を打ち倒したその場所こそが、あなたの真の故郷だった。\n放浪の旅は、ここに終わりを告げた。",
        Background.Adventurer => "深淵の王を撃破し、あなたは伝説の冒険者となった。\nその冒険譚は、世代を超えて語り継がれることだろう。",
        _ => "深淵の王を倒し、冒険を完遂した。\nあなたの勇姿は、永遠に記憶される。"
    };
}
