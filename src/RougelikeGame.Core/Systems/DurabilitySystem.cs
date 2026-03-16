namespace RougelikeGame.Core.Systems;

/// <summary>
/// 装備耐久値システム - 装備品の使用に伴う耐久値管理
/// </summary>
public static class DurabilitySystem
{
    /// <summary>
    /// 耐久値の割合から段階を判定
    /// </summary>
    public static DurabilityStage GetStage(int current, int max)
    {
        if (max <= 0) return DurabilityStage.Perfect; // 耐久値無限
        if (current <= 0) return DurabilityStage.Broken;
        float ratio = (float)current / max;
        return ratio switch
        {
            >= 0.76f => DurabilityStage.Perfect,
            >= 0.51f => DurabilityStage.Worn,
            >= 0.26f => DurabilityStage.Damaged,
            _ => DurabilityStage.Critical
        };
    }

    /// <summary>
    /// 耐久値段階に応じた性能係数を取得
    /// </summary>
    public static float GetPerformanceMultiplier(DurabilityStage stage) => stage switch
    {
        DurabilityStage.Perfect => 1.0f,
        DurabilityStage.Worn => 0.9f,
        DurabilityStage.Damaged => 0.7f,
        DurabilityStage.Critical => 0.5f,
        DurabilityStage.Broken => 0f,
        _ => 1.0f
    };

    /// <summary>
    /// 攻撃による武器耐久値の減少量を計算
    /// </summary>
    public static int CalculateWeaponWear(bool isCritical) => isCritical ? 2 : 1;

    /// <summary>
    /// 被ダメージによる防具耐久値の減少量を計算
    /// </summary>
    public static int CalculateArmorWear(int damageReceived, Element damageElement)
    {
        int wear = 1;
        if (damageReceived >= 50) wear += 2;
        else if (damageReceived >= 25) wear += 1;

        // 酸属性は追加ダメージ
        if (damageElement == Element.Poison) wear += 3;

        return wear;
    }

    /// <summary>
    /// 修理コストを計算
    /// </summary>
    public static int CalculateRepairCost(int currentDurability, int maxDurability, int itemBasePrice)
    {
        if (maxDurability <= 0) return 0;
        int durabilityLost = maxDurability - currentDurability;
        if (durabilityLost <= 0) return 0;
        return (int)(itemBasePrice * 0.5 * durabilityLost / maxDurability);
    }

    /// <summary>
    /// 修理キットでの回復量を計算（部分修理）
    /// </summary>
    public static int CalculateKitRepairAmount(int kitQuality) => kitQuality switch
    {
        1 => 15,  // 簡易修理キット
        2 => 30,  // 標準修理キット
        3 => 50,  // 高級修理キット
        _ => 10
    };

    /// <summary>
    /// 熟練度レベルによる自己修理の可否判定
    /// </summary>
    public static bool CanSelfRepair(int smithingLevel) => smithingLevel >= 5;

    /// <summary>
    /// 自己修理時の回復量（熟練度依存）
    /// </summary>
    public static int CalculateSelfRepairAmount(int smithingLevel)
    {
        if (!CanSelfRepair(smithingLevel)) return 0;
        return Math.Min(10 + (smithingLevel - 5) * 3, 40);
    }
}
