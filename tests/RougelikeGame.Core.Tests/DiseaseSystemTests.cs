using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class DiseaseSystemTests
{
    [Theory]
    [InlineData(DiseaseType.Cold)]
    [InlineData(DiseaseType.Infection)]
    [InlineData(DiseaseType.FoodPoisoning)]
    [InlineData(DiseaseType.Miasma)]
    [InlineData(DiseaseType.CursePlague)]
    public void GetDisease_ReturnsDefinition(DiseaseType type)
    {
        var disease = DiseaseSystem.GetDisease(type);
        Assert.NotNull(disease);
        Assert.Equal(type, disease.Type);
        Assert.False(string.IsNullOrEmpty(disease.Name));
    }

    [Fact]
    public void GetAllDiseases_Returns5Types()
    {
        var diseases = DiseaseSystem.GetAllDiseases();
        Assert.Equal(5, diseases.Count);
    }

    [Fact]
    public void CheckInfection_OpenWound_HigherChance()
    {
        bool infected = DiseaseSystem.CheckInfection(true, 0, 0.01);
        Assert.True(infected);
    }

    [Fact]
    public void CheckInfection_NoWound_LowerChance()
    {
        bool infected = DiseaseSystem.CheckInfection(false, 20, 0.99);
        Assert.False(infected);
    }

    [Theory]
    [InlineData(DiseaseType.Cold, 50)]
    [InlineData(DiseaseType.FoodPoisoning, 80)]
    [InlineData(DiseaseType.CursePlague, 1000)]
    public void CalculateTreatmentCost_ReturnsExpected(DiseaseType type, int expected)
    {
        Assert.Equal(expected, DiseaseSystem.CalculateTreatmentCost(type));
    }

    [Fact]
    public void Cold_IsSelfHealing()
    {
        var cold = DiseaseSystem.GetDisease(DiseaseType.Cold);
        Assert.NotNull(cold);
        Assert.True(cold.SelfHealing);
    }

    [Fact]
    public void Infection_IsNotSelfHealing()
    {
        var infection = DiseaseSystem.GetDisease(DiseaseType.Infection);
        Assert.NotNull(infection);
        Assert.False(infection.SelfHealing);
    }
}
