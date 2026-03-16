using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class SecretRoomSystemTests
{
    [Fact]
    public void CalculateDiscoveryChance_HighPerception_HigherChance()
    {
        float high = SecretRoomSystem.CalculateDiscoveryChance(20, false);
        float low = SecretRoomSystem.CalculateDiscoveryChance(5, false);
        Assert.True(high > low);
    }

    [Fact]
    public void CalculateDiscoveryChance_WithEagleEye_Bonus()
    {
        float without = SecretRoomSystem.CalculateDiscoveryChance(10, false);
        float with_ = SecretRoomSystem.CalculateDiscoveryChance(10, true);
        Assert.True(with_ > without);
    }

    [Fact]
    public void CalculateSearchChance_MinimumClamp()
    {
        float chance = SecretRoomSystem.CalculateSearchChance(0, 0);
        Assert.True(chance >= 0.3f);
    }

    [Fact]
    public void CalculateSecretRoomCount_Ruins_HigherBase()
    {
        int ruinsCount = SecretRoomSystem.CalculateSecretRoomCount(1, DungeonFeatureType.Ruins);
        int caveCount = SecretRoomSystem.CalculateSecretRoomCount(1, DungeonFeatureType.Cave);
        Assert.True(ruinsCount > caveCount);
    }

    [Fact]
    public void GetRewardQualityMultiplier_DeepFloor_Higher()
    {
        float deep = SecretRoomSystem.GetRewardQualityMultiplier(10);
        float shallow = SecretRoomSystem.GetRewardQualityMultiplier(1);
        Assert.True(deep > shallow);
    }

    [Fact]
    public void ShouldGenerateSecretPassage_Ruins_HigherChance()
    {
        Assert.True(SecretRoomSystem.ShouldGenerateSecretPassage(0.35, DungeonFeatureType.Ruins));
        Assert.False(SecretRoomSystem.ShouldGenerateSecretPassage(0.35, DungeonFeatureType.Cave));
    }
}
