using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class NpcRoutineSystemTests
{
    [Fact]
    public void GetRoutine_MerchantMorning_ReturnsWorking()
    {
        var routine = NpcRoutineSystem.GetRoutine("Merchant", TimePeriod.Morning);
        Assert.NotNull(routine);
        Assert.Equal(NpcRoutineSystem.NpcActivity.Working, routine.Activity);
    }

    [Fact]
    public void GetRoutine_GuardNight_ReturnsRoutine()
    {
        var routine = NpcRoutineSystem.GetRoutine("Guard", TimePeriod.Night);
        Assert.NotNull(routine);
    }

    [Fact]
    public void GetRoutine_InvalidNpc_ReturnsNull()
    {
        var routine = NpcRoutineSystem.GetRoutine("NonExistentNpc", TimePeriod.Morning);
        Assert.Null(routine);
    }

    [Fact]
    public void IsNpcAvailable_MerchantMorning_ReturnsTrue()
    {
        Assert.True(NpcRoutineSystem.IsNpcAvailable("Merchant", TimePeriod.Morning));
    }

    [Fact]
    public void GetNpcsAtLocation_ReturnsResults()
    {
        var npcs = NpcRoutineSystem.GetNpcsAtLocation("Shop", TimePeriod.Morning);
        Assert.NotNull(npcs);
    }

    [Fact]
    public void GetAllRoutines_ReturnsNonEmpty()
    {
        var routines = NpcRoutineSystem.GetAllRoutines();
        Assert.True(routines.Count > 0);
    }
}
