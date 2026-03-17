namespace RougelikeGame.Core.Systems;

/// <summary>
/// 隠し通路・シークレットルームシステム
/// </summary>
public static class SecretRoomSystem
{
    /// <summary>基本発見確率（PER依存）</summary>
    public static float CalculateDiscoveryChance(int perception, bool hasEagleEye)
    {
        float baseChance = 0.05f + perception * 0.02f;
        if (hasEagleEye) baseChance += 0.2f; // 鷹の目フラグ
        return Math.Clamp(baseChance, 0.05f, 0.8f);
    }

    /// <summary>探索アクションによる発見確率（Search行動）</summary>
    public static float CalculateSearchChance(int perception, int searchSkillLevel)
    {
        return Math.Clamp(0.3f + perception * 0.03f + searchSkillLevel * 0.05f, 0.3f, 0.95f);
    }

    /// <summary>階層あたりのシークレットルーム数を計算</summary>
    public static int CalculateSecretRoomCount(int floorDepth, DungeonFeatureType featureType)
    {
        int baseCount = featureType switch
        {
            DungeonFeatureType.Ruins => 3,
            DungeonFeatureType.Temple => 2,
            DungeonFeatureType.Crypt => 2,
            _ => 1
        };
        return baseCount + floorDepth / 5;
    }

    /// <summary>シークレットルームの報酬品質倍率</summary>
    public static float GetRewardQualityMultiplier(int floorDepth)
    {
        return 1.0f + floorDepth * 0.1f;
    }

    /// <summary>隠し通路の有無をランダム生成時に判定</summary>
    public static bool ShouldGenerateSecretPassage(double randomValue, DungeonFeatureType featureType)
    {
        float chance = featureType switch
        {
            DungeonFeatureType.Ruins => 0.4f,
            DungeonFeatureType.Temple => 0.3f,
            DungeonFeatureType.Cave => 0.2f,
            _ => 0.15f
        };
        return randomValue < chance;
    }
}
