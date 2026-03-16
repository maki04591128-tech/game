namespace RougelikeGame.Core.Systems;

/// <summary>
/// 描画範囲外抑制・軽量化システム - 視界外のエンティティ更新頻度制御
/// </summary>
public static class RenderOptimizationSystem
{
    /// <summary>描画領域の計算</summary>
    public static (int MinX, int MinY, int MaxX, int MaxY) CalculateViewport(
        int playerX, int playerY, int viewportWidth, int viewportHeight)
    {
        int halfW = viewportWidth / 2;
        int halfH = viewportHeight / 2;
        return (playerX - halfW, playerY - halfH, playerX + halfW, playerY + halfH);
    }

    /// <summary>座標が描画領域内かどうか判定</summary>
    public static bool IsInViewport(int x, int y, int minX, int minY, int maxX, int maxY)
    {
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }

    /// <summary>描画領域外のエンティティの更新頻度を計算（距離に応じて減少）</summary>
    public static int CalculateUpdateFrequency(int distanceFromPlayer)
    {
        return distanceFromPlayer switch
        {
            <= 10 => 1,   // 毎ターン更新
            <= 20 => 2,   // 2ターンに1回
            <= 40 => 4,   // 4ターンに1回
            <= 60 => 8,   // 8ターンに1回
            _ => 16        // 16ターンに1回
        };
    }

    /// <summary>エンティティとプレイヤーの距離を計算（チェビシェフ距離）</summary>
    public static int CalculateDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }

    /// <summary>指定ターンで更新すべきかどうか判定</summary>
    public static bool ShouldUpdate(int currentTurn, int updateFrequency)
    {
        return updateFrequency <= 0 || currentTurn % updateFrequency == 0;
    }
}
