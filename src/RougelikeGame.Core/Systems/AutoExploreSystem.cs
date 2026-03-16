namespace RougelikeGame.Core.Systems;

/// <summary>
/// 自動探索・マクロ操作システム - 未発見タイルの自動移動
/// </summary>
public static class AutoExploreSystem
{
    /// <summary>自動探索の停止条件</summary>
    public enum StopReason
    {
        /// <summary>全タイル発見済み</summary>
        FullyExplored,
        /// <summary>敵発見</summary>
        EnemyDetected,
        /// <summary>アイテム発見</summary>
        ItemFound,
        /// <summary>階段発見</summary>
        StairsFound,
        /// <summary>罠発見</summary>
        TrapDetected,
        /// <summary>HP低下</summary>
        LowHp,
        /// <summary>満腹度低下</summary>
        LowHunger,
        /// <summary>ユーザー中断</summary>
        UserInterrupt,
        /// <summary>経路なし</summary>
        NoPath
    }

    /// <summary>自動探索中の停止判定を行う</summary>
    public static StopReason? CheckStopConditions(
        bool hasUndiscoveredTiles,
        bool enemyInSight,
        bool itemOnTile,
        bool stairsNearby,
        bool trapNearby,
        float hpRatio,
        float hungerRatio)
    {
        if (enemyInSight) return StopReason.EnemyDetected;
        if (trapNearby) return StopReason.TrapDetected;
        if (hpRatio < 0.3f) return StopReason.LowHp;
        if (hungerRatio < 0.15f) return StopReason.LowHunger;
        if (itemOnTile) return StopReason.ItemFound;
        if (stairsNearby) return StopReason.StairsFound;
        if (!hasUndiscoveredTiles) return StopReason.FullyExplored;
        return null; // 続行
    }

    /// <summary>停止理由のメッセージを取得</summary>
    public static string GetStopMessage(StopReason reason) => reason switch
    {
        StopReason.FullyExplored => "このフロアの全タイルを発見した。",
        StopReason.EnemyDetected => "敵を発見！自動探索を中断。",
        StopReason.ItemFound => "アイテムを発見。自動探索を中断。",
        StopReason.StairsFound => "階段を発見。自動探索を中断。",
        StopReason.TrapDetected => "罠を発見！自動探索を中断。",
        StopReason.LowHp => "HPが低下。自動探索を中断。",
        StopReason.LowHunger => "空腹度が低下。自動探索を中断。",
        StopReason.UserInterrupt => "自動探索を中断した。",
        StopReason.NoPath => "移動可能な経路がない。",
        _ => "自動探索を中断した。"
    };

    /// <summary>探索優先度を計算（距離+未発見タイル数の重み）</summary>
    public static float CalculateExplorationPriority(int distanceToTile, int nearbyUndiscoveredCount)
    {
        return nearbyUndiscoveredCount * 2.0f - distanceToTile;
    }
}
