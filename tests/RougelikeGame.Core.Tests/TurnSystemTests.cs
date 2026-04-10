using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;

namespace RougelikeGame.Core.Tests;

public class TurnActionTests
{
    [Fact]
    public void Move_NormalState_Returns1Turn()
    {
        // Arrange
        var action = TurnAction.Move(Direction.North);

        // Act
        var cost = action.CalculateFinalCost(CombatState.Normal);

        // Assert
        Assert.Equal(1, cost);
    }

    [Fact]
    public void Move_CombatState_Returns10Turns()
    {
        // Arrange
        var action = TurnAction.Move(Direction.North);

        // Act
        var cost = action.CalculateFinalCost(CombatState.Combat);

        // Assert
        Assert.Equal(10, cost);
    }

    [Fact]
    public void Move_StealthState_Returns10Turns()
    {
        // Arrange
        var action = TurnAction.Move(Direction.North);

        // Act
        var cost = action.CalculateFinalCost(CombatState.Stealth);

        // Assert
        Assert.Equal(10, cost);
    }

    [Fact]
    public void Move_Diagonal_ReturnsMultipliedCost()
    {
        // Arrange
        var action = TurnAction.Move(Direction.NorthEast);

        // Act
        var normalCost = action.CalculateFinalCost(CombatState.Normal);
        var combatCost = action.CalculateFinalCost(CombatState.Combat);

        // Assert
        Assert.Equal(2, normalCost);  // 1 * 1.4 = 1.4 -> ceiling to 2
        Assert.Equal(14, combatCost); // 10 * 1.4 = 14
    }

    [Fact]
    public void Attack_Returns1Turn()
    {
        // Arrange
        var action = TurnAction.Attack(null!);

        // Act
        var cost = action.CalculateFinalCost(CombatState.Normal);

        // Assert
        Assert.Equal(TurnCosts.AttackNormal, cost);
        Assert.Equal(1, cost);
    }

    [Fact]
    public void Wait_Returns1Turn()
    {
        // Arrange
        var action = TurnAction.Wait;

        // Act
        var cost = action.CalculateFinalCost(CombatState.Normal);

        // Assert
        Assert.Equal(1, cost);
    }

    [Fact]
    public void Rest_Returns100Turns()
    {
        // Arrange
        var action = TurnAction.Rest;

        // Act
        var cost = action.CalculateFinalCost(CombatState.Normal);

        // Assert
        Assert.Equal(100, cost);
    }

    [Fact]
    public void CastSpell_ClampsBetweenMinAndMax()
    {
        // Arrange
        var shortSpell = TurnAction.CastSpell("test", 2);  // Below minimum
        var longSpell = TurnAction.CastSpell("test", 200); // Above maximum

        // Assert
        Assert.Equal(TurnCosts.SpellMinimum, shortSpell.BaseTurnCost);
        Assert.Equal(TurnCosts.SpellMaximum, longSpell.BaseTurnCost);
    }
}

public class PlayerTests
{
    [Fact]
    public void Create_InitializesWithCorrectValues()
    {
        // Arrange & Act
        var player = Player.Create("Test", Stats.Default);

        // Assert
        Assert.Equal("Test", player.Name);
        Assert.Equal(1, player.Level);
        Assert.Equal(GameConstants.InitialSanity, player.Sanity);
        Assert.Equal(GameConstants.InitialHunger, player.Hunger);
        Assert.Equal(SanityStage.Normal, player.SanityStage);
        Assert.Equal(HungerStage.Normal, player.HungerStage);
        Assert.Equal(GameConstants.MaxRescueCount, player.RescueCountRemaining);
        Assert.True(player.IsAlive);
    }

    [Fact]
    public void TakeDamage_ReducesHp()
    {
        // Arrange
        var player = Player.Create("Test", Stats.Default);
        var initialHp = player.CurrentHp;
        var damage = Damage.Physical(10);

        // Act
        player.TakeDamage(damage);

        // Assert
        Assert.True(player.CurrentHp < initialHp);
    }

    [Fact]
    public void Heal_RestoresHp()
    {
        // Arrange
        var player = Player.Create("Test", Stats.Default);
        player.TakeDamage(Damage.Physical(50));
        var hpAfterDamage = player.CurrentHp;

        // Act
        player.Heal(20);

        // Assert
        Assert.True(player.CurrentHp > hpAfterDamage);
    }

    [Fact]
    public void LearnWord_AddsWordWithInitialMastery()
    {
        // Arrange
        var player = Player.Create("Test", Stats.Default);

        // Act
        player.LearnWord("brenna");

        // Assert
        Assert.True(player.LearnedWords.ContainsKey("brenna"));
        Assert.Equal(GameConstants.InitialWordMastery, player.GetWordMastery("brenna"));
    }

    [Fact]
    public void LearnSkill_AddsSkill()
    {
        // Arrange
        var player = Player.Create("Test", Stats.Default);

        // Act
        player.LearnSkill("power_attack");

        // Assert
        Assert.True(player.HasSkill("power_attack"));
    }

    [Fact]
    public void SanityStage_ChangesCorrectly()
    {
        // Arrange
        var player = Player.Create("Test", Stats.Default);

        // Act & Assert
        player.ModifySanity(-30); // 70
        Assert.Equal(SanityStage.Uneasy, player.SanityStage);

        player.ModifySanity(-20); // 50
        Assert.Equal(SanityStage.Anxious, player.SanityStage);

        player.ModifySanity(-20); // 30
        Assert.Equal(SanityStage.Unstable, player.SanityStage);

        player.ModifySanity(-20); // 10
        Assert.Equal(SanityStage.Madness, player.SanityStage);

        player.ModifySanity(-10); // 0
        Assert.Equal(SanityStage.Broken, player.SanityStage);
    }
}

public class PositionTests
{
    [Fact]
    public void Move_North_DecreasesY()
    {
        // Arrange
        var pos = new Position(5, 5);

        // Act
        var newPos = pos.Move(Direction.North);

        // Assert
        Assert.Equal(5, newPos.X);
        Assert.Equal(4, newPos.Y);
    }

    [Fact]
    public void Move_South_IncreasesY()
    {
        // Arrange
        var pos = new Position(5, 5);

        // Act
        var newPos = pos.Move(Direction.South);

        // Assert
        Assert.Equal(5, newPos.X);
        Assert.Equal(6, newPos.Y);
    }

    [Fact]
    public void DistanceTo_ReturnsManhattenDistance()
    {
        // Arrange
        var pos1 = new Position(0, 0);
        var pos2 = new Position(3, 4);

        // Act
        var distance = pos1.DistanceTo(pos2);

        // Assert
        Assert.Equal(7, distance);
    }

    [Fact]
    public void IsDiagonal_ReturnsCorrectly()
    {
        // Assert
        Assert.False(Direction.North.IsDiagonal());
        Assert.False(Direction.South.IsDiagonal());
        Assert.False(Direction.East.IsDiagonal());
        Assert.False(Direction.West.IsDiagonal());
        Assert.True(Direction.NorthEast.IsDiagonal());
        Assert.True(Direction.NorthWest.IsDiagonal());
        Assert.True(Direction.SouthEast.IsDiagonal());
        Assert.True(Direction.SouthWest.IsDiagonal());
    }
}

public class StatsTests
{
    [Fact]
    public void Apply_AddsModifier()
    {
        // Arrange
        var stats = Stats.Default;
        var modifier = new StatModifier(Strength: 5, Vitality: 3);

        // Act
        var newStats = stats.Apply(modifier);

        // Assert
        Assert.Equal(15, newStats.Strength);
        Assert.Equal(13, newStats.Vitality);
    }

    [Fact]
    public void MaxHp_CalculatesCorrectly()
    {
        // Arrange
        var stats = new Stats(10, 15, 10, 10, 10, 10, 10, 10, 10);

        // Act
        var maxHp = stats.MaxHp;

        // Assert
        // 50 + (VIT * 10) + (STR * 2) = 50 + 150 + 20 = 220
        Assert.Equal(220, maxHp);
    }
}

public class TimeConstantsTests
{
    [Fact]
    public void TurnsPerMinute_Is60()
    {
        Assert.Equal(60, TimeConstants.TurnsPerMinute);
    }

    [Fact]
    public void TurnsPerHour_Is3600()
    {
        Assert.Equal(3600, TimeConstants.TurnsPerHour);
    }

    [Fact]
    public void TurnsPerDay_Is86400()
    {
        Assert.Equal(86400, TimeConstants.TurnsPerDay);
    }
}
