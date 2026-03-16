using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class BaseConstructionSystemTests
{
    [Fact]
    public void Build_WithSufficientMaterials_Succeeds()
    {
        var system = new BaseConstructionSystem();
        Assert.True(system.Build(FacilityCategory.Camp, 100));
        Assert.True(system.HasFacility(FacilityCategory.Camp));
    }

    [Fact]
    public void Build_InsufficientMaterials_Fails()
    {
        var system = new BaseConstructionSystem();
        Assert.False(system.Build(FacilityCategory.Smithy, 10));
    }

    [Fact]
    public void Build_AlreadyBuilt_Fails()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Camp, 100);
        Assert.False(system.Build(FacilityCategory.Camp, 100));
    }

    [Fact]
    public void CalculateDefenseRating_WithBarricade()
    {
        var system = new BaseConstructionSystem();
        system.Build(FacilityCategory.Barricade, 100);
        Assert.Equal(30, system.CalculateDefenseRating());
    }

    [Fact]
    public void GetDefinition_ReturnsValidData()
    {
        var def = BaseConstructionSystem.GetDefinition(FacilityCategory.Storage);
        Assert.NotNull(def);
        Assert.Equal("倉庫", def.Name);
    }
}
