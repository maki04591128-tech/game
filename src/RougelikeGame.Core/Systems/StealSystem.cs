using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// T-1: 盗み/スリ（ピックポケット）システム
/// 敵やNPCからアイテム・ゴールドを盗む
/// </summary>
public static class StealSystem
{
    /// <summary>盗み成功率を計算</summary>
    public static float CalculateStealChance(int playerDex, int playerLevel, int targetLevel, int stealSkillRate = 0)
    {
        // 基本成功率 = DEX * 2 + スキルボーナス - レベル差 * 5
        float baseChance = playerDex * 2f + stealSkillRate - (targetLevel - playerLevel) * 5f;
        return Math.Clamp(baseChance, 5f, 95f); // 最低5%, 最大95%
    }

    /// <summary>盗みを試行する</summary>
    public static StealResult AttemptSteal(int playerDex, int playerLevel, int targetLevel,
        int targetGold, bool targetHasItems, int stealSkillRate, IRandomProvider random)
    {
        float chance = CalculateStealChance(playerDex, playerLevel, targetLevel, stealSkillRate);
        int roll = random.Next(100);

        if (roll >= chance)
        {
            // 失敗 - 発覚リスク
            bool detected = roll >= chance + 20; // 大幅な失敗は発覚
            return new StealResult(false, 0, false, detected, "盗みに失敗した！" + (detected ? "見つかった！" : ""));
        }

        // 成功 - ゴールドかアイテムを盗む
        if (targetGold > 0)
        {
            int stolenGold = Math.Max(1, random.Next(1, Math.Min(targetGold, playerLevel * 10) + 1));
            return new StealResult(true, stolenGold, false, false, $"{stolenGold}Gを盗んだ！");
        }

        if (targetHasItems)
        {
            return new StealResult(true, 0, true, false, "アイテムを盗んだ！");
        }

        return new StealResult(false, 0, false, false, "盗めるものがなかった。");
    }
}

/// <summary>盗み結果</summary>
public record StealResult(
    bool Success,
    int StolenGold,
    bool StolenItem,
    bool Detected,
    string Message);
