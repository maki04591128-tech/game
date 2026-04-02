using Xunit;
using RougelikeGame.Core;

namespace RougelikeGame.Core.Tests;

public class ValueObjectsTests
{
    #region Position - 距離計算テスト

    [Fact]
    public void Position_DistanceTo_ManhattanDistance_ReturnsCorrectValue()
    {
        var a = new Position(0, 0);
        var b = new Position(3, 4);

        var distance = a.DistanceTo(b);

        Assert.Equal(7, distance);
    }

    [Fact]
    public void Position_DistanceTo_SamePosition_ReturnsZero()
    {
        var pos = new Position(5, 5);

        Assert.Equal(0, pos.DistanceTo(pos));
    }

    [Fact]
    public void Position_EuclideanDistanceTo_ReturnsCorrectValue()
    {
        var a = new Position(0, 0);
        var b = new Position(3, 4);

        var distance = a.EuclideanDistanceTo(b);

        Assert.Equal(5.0, distance, precision: 5);
    }

    [Fact]
    public void Position_ChebyshevDistanceTo_ReturnsMaxComponent()
    {
        var a = new Position(0, 0);
        var b = new Position(3, 7);

        var distance = a.ChebyshevDistanceTo(b);

        Assert.Equal(7, distance);
    }

    [Fact]
    public void Position_ChebyshevDistanceTo_DiagonalEqualsMax()
    {
        var a = new Position(1, 1);
        var b = new Position(4, 5);

        // max(|4-1|, |5-1|) = max(3, 4) = 4
        Assert.Equal(4, a.ChebyshevDistanceTo(b));
    }

    #endregion

    #region Position - Move テスト

    [Fact]
    public void Position_Move_North_DecreasesY()
    {
        var pos = new Position(5, 5);

        var result = pos.Move(Direction.North);

        Assert.Equal(new Position(5, 4), result);
    }

    [Fact]
    public void Position_Move_South_IncreasesY()
    {
        var pos = new Position(5, 5);

        var result = pos.Move(Direction.South);

        Assert.Equal(new Position(5, 6), result);
    }

    [Fact]
    public void Position_Move_East_IncreasesX()
    {
        var pos = new Position(5, 5);

        var result = pos.Move(Direction.East);

        Assert.Equal(new Position(6, 5), result);
    }

    [Fact]
    public void Position_Move_SouthWest_DiagonalMove()
    {
        var pos = new Position(5, 5);

        var result = pos.Move(Direction.SouthWest);

        Assert.Equal(new Position(4, 6), result);
    }

    #endregion

    #region Position - GetNeighbors テスト

    [Fact]
    public void Position_GetNeighbors_CardinalOnly_ReturnsFourPositions()
    {
        var pos = new Position(5, 5);

        var neighbors = pos.GetNeighbors(includeDiagonals: false).ToList();

        Assert.Equal(4, neighbors.Count);
        Assert.Contains(new Position(5, 4), neighbors); // North
        Assert.Contains(new Position(5, 6), neighbors); // South
        Assert.Contains(new Position(6, 5), neighbors); // East
        Assert.Contains(new Position(4, 5), neighbors); // West
    }

    [Fact]
    public void Position_GetNeighbors_IncludeDiagonals_ReturnsEightPositions()
    {
        var pos = new Position(5, 5);

        var neighbors = pos.GetNeighbors(includeDiagonals: true).ToList();

        Assert.Equal(8, neighbors.Count);
        Assert.Contains(new Position(6, 4), neighbors); // NorthEast
        Assert.Contains(new Position(4, 6), neighbors); // SouthWest
    }

    #endregion

    #region Position - GetDirectionTo テスト

    [Fact]
    public void Position_GetDirectionTo_East_ReturnsEast()
    {
        var from = new Position(0, 0);
        var to = new Position(3, 0);

        Assert.Equal(Direction.East, from.GetDirectionTo(to));
    }

    [Fact]
    public void Position_GetDirectionTo_SamePosition_ReturnsNorth()
    {
        var pos = new Position(5, 5);

        // 同じ位置の場合はNorthを返す
        Assert.Equal(Direction.North, pos.GetDirectionTo(pos));
    }

    [Fact]
    public void Position_GetDirectionTo_NorthEast_ReturnsDiagonal()
    {
        var from = new Position(0, 0);
        var to = new Position(3, -2);

        Assert.Equal(Direction.NorthEast, from.GetDirectionTo(to));
    }

    #endregion

    #region Position - 演算子テスト

    [Fact]
    public void Position_AddOperator_ReturnsSumOfComponents()
    {
        var a = new Position(1, 2);
        var b = new Position(3, 4);

        var result = a + b;

        Assert.Equal(new Position(4, 6), result);
    }

    [Fact]
    public void Position_SubtractOperator_ReturnsDifferenceOfComponents()
    {
        var a = new Position(5, 7);
        var b = new Position(2, 3);

        var result = a - b;

        Assert.Equal(new Position(3, 4), result);
    }

    [Fact]
    public void Position_Zero_ReturnsOrigin()
    {
        Assert.Equal(new Position(0, 0), Position.Zero);
    }

    [Fact]
    public void Position_ToString_ReturnsFormattedString()
    {
        var pos = new Position(3, 7);

        Assert.Equal("(3, 7)", pos.ToString());
    }

    #endregion

    #region Stats - 計算プロパティテスト

    [Fact]
    public void Stats_Default_AllValuesAreTen()
    {
        var stats = Stats.Default;

        Assert.Equal(10, stats.Strength);
        Assert.Equal(10, stats.Vitality);
        Assert.Equal(10, stats.Agility);
        Assert.Equal(10, stats.Intelligence);
        Assert.Equal(10, stats.Luck);
    }

    [Fact]
    public void Stats_Zero_AllValuesAreZero()
    {
        var stats = Stats.Zero;

        Assert.Equal(0, stats.Strength);
        Assert.Equal(0, stats.Vitality);
    }

    [Fact]
    public void Stats_MaxHp_CalculatedFromVitalityAndStrength()
    {
        // MaxHp = 50 + (Vitality * 10) + (Strength * 2)
        var stats = Stats.Default; // STR=10, VIT=10

        Assert.Equal(50 + 100 + 20, stats.MaxHp); // 170
    }

    [Fact]
    public void Stats_MaxMp_CalculatedFromMindAndIntelligence()
    {
        // MaxMp = 20 + (Mind * 5) + (Intelligence * 3)
        var stats = Stats.Default; // MND=10, INT=10

        Assert.Equal(20 + 50 + 30, stats.MaxMp); // 100
    }

    [Fact]
    public void Stats_PhysicalAttack_CalculatedFromStrengthAndDexterity()
    {
        // PhysicalAttack = Strength * 3 + Dexterity
        var stats = Stats.Default; // STR=10, DEX=10

        Assert.Equal(30 + 10, stats.PhysicalAttack); // 40
    }

    [Fact]
    public void Stats_ActionSpeed_CalculatedFromAgilityAndDexterity()
    {
        // ActionSpeed = Agility * 2 + Dexterity
        var stats = Stats.Default; // AGI=10, DEX=10

        Assert.Equal(30, stats.ActionSpeed);
    }

    [Fact]
    public void Stats_MaxHp_NeverNegative()
    {
        // 極端な負のステータスでもMaxHpは1以上
        var stats = new Stats(-20, -20, 0, 0, 0, 0, 0, 0, 0);
        Assert.True(stats.MaxHp >= 1, $"MaxHp={stats.MaxHp} should be >= 1");
    }

    [Fact]
    public void Stats_MaxMp_NeverNegative()
    {
        var stats = new Stats(0, 0, 0, 0, -20, -20, 0, 0, 0);
        Assert.True(stats.MaxMp >= 0, $"MaxMp={stats.MaxMp} should be >= 0");
    }

    [Fact]
    public void Stats_MaxSp_NeverNegative()
    {
        var stats = new Stats(0, -60, 0, 0, 0, 0, 0, 0, 0);
        Assert.True(stats.MaxSp >= 0, $"MaxSp={stats.MaxSp} should be >= 0");
    }

    [Fact]
    public void Stats_HitRate_NeverNegative()
    {
        var stats = new Stats(0, 0, 0, -50, 0, 0, 0, 0, 0);
        Assert.True(stats.HitRate >= 0.05, $"HitRate={stats.HitRate} should be >= 0.05");
    }

    [Fact]
    public void Stats_EvasionRate_NeverNegative()
    {
        var stats = new Stats(0, 0, -50, 0, 0, 0, 0, 0, 0);
        Assert.True(stats.EvasionRate >= 0.0, $"EvasionRate={stats.EvasionRate} should be >= 0");
    }

    [Fact]
    public void Stats_CriticalRate_NeverNegative()
    {
        var stats = new Stats(0, 0, 0, -100, 0, 0, 0, 0, -100);
        Assert.True(stats.CriticalRate >= 0.0, $"CriticalRate={stats.CriticalRate} should be >= 0");
    }

    #endregion

    #region Stats - 修正適用テスト

    [Fact]
    public void Stats_Apply_SingleModifier_AddsValues()
    {
        var stats = Stats.Default;
        var modifier = new StatModifier(Strength: 5, Vitality: -3);

        var result = stats.Apply(modifier);

        Assert.Equal(15, result.Strength);
        Assert.Equal(7, result.Vitality);
        Assert.Equal(10, result.Agility); // 変更なし
    }

    [Fact]
    public void Stats_ApplyAll_MultipleModifiers_CumulativeEffect()
    {
        var stats = Stats.Zero;
        var modifiers = new[]
        {
            new StatModifier(Strength: 5),
            new StatModifier(Strength: 3),
            new StatModifier(Vitality: 10)
        };

        var result = stats.ApplyAll(modifiers);

        Assert.Equal(8, result.Strength);
        Assert.Equal(10, result.Vitality);
    }

    #endregion

    #region StatModifier - 演算子テスト

    [Fact]
    public void StatModifier_AddOperator_CombinesModifiers()
    {
        var a = new StatModifier(Strength: 5, Agility: 3);
        var b = new StatModifier(Strength: 2, Intelligence: 7);

        var result = a + b;

        Assert.Equal(7, result.Strength);
        Assert.Equal(3, result.Agility);
        Assert.Equal(7, result.Intelligence);
    }

    [Fact]
    public void StatModifier_MultiplyOperator_ScalesAllValues()
    {
        var modifier = new StatModifier(Strength: 2, Vitality: 3);

        var result = modifier * 4;

        Assert.Equal(8, result.Strength);
        Assert.Equal(12, result.Vitality);
    }

    [Fact]
    public void StatModifier_All_SetsAllFieldsToSameValue()
    {
        var modifier = StatModifier.All(5);

        Assert.Equal(5, modifier.Strength);
        Assert.Equal(5, modifier.Luck);
        Assert.Equal(5, modifier.Charisma);
    }

    #endregion

    #region Damage - ファクトリメソッドテスト

    [Fact]
    public void Damage_Physical_CreatesPhysicalDamage()
    {
        var dmg = Damage.Physical(50);

        Assert.Equal(50, dmg.Amount);
        Assert.Equal(DamageType.Physical, dmg.Type);
        Assert.Equal(Element.None, dmg.Element);
        Assert.False(dmg.IsCritical);
    }

    [Fact]
    public void Damage_Physical_WithCritical_SetsCriticalFlag()
    {
        var dmg = Damage.Physical(50, isCritical: true);

        Assert.True(dmg.IsCritical);
    }

    [Fact]
    public void Damage_Magical_CreatesElementalDamage()
    {
        var dmg = Damage.Magical(30, Element.Fire);

        Assert.Equal(30, dmg.Amount);
        Assert.Equal(DamageType.Magical, dmg.Type);
        Assert.Equal(Element.Fire, dmg.Element);
    }

    [Fact]
    public void Damage_Pure_CreatesDefenseIgnoringDamage()
    {
        var dmg = Damage.Pure(25);

        Assert.Equal(DamageType.Pure, dmg.Type);
        Assert.Equal(Element.None, dmg.Element);
        Assert.False(dmg.IsCritical);
    }

    [Fact]
    public void Damage_Healing_CreatesHealingType()
    {
        var dmg = Damage.Healing(40);

        Assert.Equal(40, dmg.Amount);
        Assert.Equal(DamageType.Healing, dmg.Type);
    }

    #endregion

    #region Damage - 変換メソッドテスト

    [Fact]
    public void Damage_WithMultiplier_ScalesAmount()
    {
        var dmg = Damage.Physical(100);

        var result = dmg.WithMultiplier(1.5f);

        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void Damage_WithMultiplier_MinimumIsOne()
    {
        var dmg = Damage.Physical(1);

        // 非常に小さい倍率でも最低1を保証
        var result = dmg.WithMultiplier(0.01f);

        Assert.Equal(1, result.Amount);
    }

    [Fact]
    public void Damage_WithElement_ChangesElement()
    {
        var dmg = Damage.Physical(50);

        var result = dmg.WithElement(Element.Ice);

        Assert.Equal(Element.Ice, result.Element);
        Assert.Equal(50, result.Amount); // ダメージ量は変わらない
    }

    [Fact]
    public void Damage_WithSource_SetsSourceName()
    {
        var dmg = Damage.Physical(50);

        var result = dmg.WithSource("スケルトン");

        Assert.Equal("スケルトン", result.SourceName);
    }

    #endregion

    #region Damage - CalculateFinal テスト

    [Fact]
    public void Damage_CalculateFinal_Physical_ReducedByDefense()
    {
        var dmg = Damage.Physical(50);

        // 防御20: 50 - 20 = 30, 耐性0: 30 * 1.0 = 30
        var final = dmg.CalculateFinal(defense: 20);

        Assert.Equal(30, final);
    }

    [Fact]
    public void Damage_CalculateFinal_Pure_IgnoresDefense()
    {
        var dmg = Damage.Pure(25);

        // 純粋ダメージは防御を無視する
        var final = dmg.CalculateFinal(defense: 100);

        Assert.Equal(25, final);
    }

    [Fact]
    public void Damage_CalculateFinal_Healing_ReturnsFullAmount()
    {
        var dmg = Damage.Healing(40);

        // 回復はそのまま返す
        var final = dmg.CalculateFinal(defense: 999, resistance: 0.5f);

        Assert.Equal(40, final);
    }

    [Fact]
    public void Damage_CalculateFinal_MinimumDamageIsOne()
    {
        var dmg = Damage.Physical(5);

        // 防御が非常に高い場合でも最低ダメージは1
        var final = dmg.CalculateFinal(defense: 1000);

        Assert.Equal(1, final);
    }

    #endregion

    #region TurnAction - ファクトリメソッドテスト

    [Fact]
    public void TurnAction_Move_SetsCorrectTypeAndCost()
    {
        var action = TurnAction.Move(Direction.North);

        Assert.Equal(TurnActionType.Move, action.Type);
        Assert.Equal(Direction.North, action.Direction);
        Assert.Equal(TurnCosts.MoveNormal, action.BaseTurnCost);
    }

    [Fact]
    public void TurnAction_Wait_HasMinimalCost()
    {
        var action = TurnAction.Wait;

        Assert.Equal(TurnActionType.Wait, action.Type);
        Assert.Equal(TurnCosts.Wait, action.BaseTurnCost);
    }

    [Fact]
    public void TurnAction_Rest_HasHighCost()
    {
        var action = TurnAction.Rest;

        Assert.Equal(TurnActionType.Rest, action.Type);
        Assert.Equal(TurnCosts.Rest, action.BaseTurnCost);
    }

    #endregion

    #region TurnAction - CalculateFinalCost テスト

    [Fact]
    public void TurnAction_CalculateFinalCost_NormalMove_ReturnsBaseCost()
    {
        var action = TurnAction.Move(Direction.North);

        // 通常移動: 1 * 1.0 = 1
        var cost = action.CalculateFinalCost(CombatState.Normal);

        Assert.Equal(1, cost);
    }

    [Fact]
    public void TurnAction_CalculateFinalCost_CombatMove_HigherCost()
    {
        var action = TurnAction.Move(Direction.North);

        // 戦闘中移動: 1 * 10.0 = 10
        var cost = action.CalculateFinalCost(CombatState.Combat);

        Assert.Equal(10, cost);
    }

    [Fact]
    public void TurnAction_CalculateFinalCost_DiagonalMove_AppliesDiagonalMultiplier()
    {
        var action = TurnAction.Move(Direction.NorthEast);

        // 斜め移動: 1 * 1.0 * (14/10) = 1.4 → ceiling = 2
        var cost = action.CalculateFinalCost(CombatState.Normal);

        Assert.Equal(2, cost);
    }

    [Fact]
    public void TurnAction_CalculateFinalCost_NonMoveAction_IgnoresCombatState()
    {
        var action = TurnAction.Wait;

        // 移動以外はCombatState修正を受けない
        var costNormal = action.CalculateFinalCost(CombatState.Normal);
        var costCombat = action.CalculateFinalCost(CombatState.Combat);

        Assert.Equal(costNormal, costCombat);
    }

    [Fact]
    public void TurnAction_CalculateFinalCost_WithEquipmentModifier_AppliesMultiplier()
    {
        var action = TurnAction.Move(Direction.North);

        // 装備修正2.0: 1 * 1.0 * 2.0 = 2
        var cost = action.CalculateFinalCost(CombatState.Normal, equipmentModifier: 2.0f);

        Assert.Equal(2, cost);
    }

    [Fact]
    public void TurnAction_CalculateFinalCost_MinimumIsOne()
    {
        var action = TurnAction.Move(Direction.North);

        // 全修正を極端に小さくしても最低1
        var cost = action.CalculateFinalCost(CombatState.Normal,
            equipmentModifier: 0.01f, statusModifier: 0.01f, environmentModifier: 0.01f);

        Assert.Equal(1, cost);
    }

    [Fact]
    public void TurnAction_CastSpell_ClampsTurnCost()
    {
        // SpellMinimum=5, SpellMaximum=100
        var tooLow = TurnAction.CastSpell("fireball", turnCost: 1);
        var tooHigh = TurnAction.CastSpell("meteor", turnCost: 999);

        Assert.Equal(TurnCosts.SpellMinimum, tooLow.BaseTurnCost);
        Assert.Equal(TurnCosts.SpellMaximum, tooHigh.BaseTurnCost);
    }

    #endregion

    #region DirectionExtensions テスト

    [Fact]
    public void Direction_IsDiagonal_CardinalDirection_ReturnsFalse()
    {
        Assert.False(Direction.North.IsDiagonal());
        Assert.False(Direction.East.IsDiagonal());
    }

    [Fact]
    public void Direction_IsDiagonal_DiagonalDirection_ReturnsTrue()
    {
        Assert.True(Direction.NorthEast.IsDiagonal());
        Assert.True(Direction.SouthWest.IsDiagonal());
    }

    [Fact]
    public void Direction_Opposite_ReturnsOppositeDirection()
    {
        Assert.Equal(Direction.South, Direction.North.Opposite());
        Assert.Equal(Direction.SouthWest, Direction.NorthEast.Opposite());
    }

    [Fact]
    public void Direction_RotateClockwise_ReturnsCorrectRotation()
    {
        Assert.Equal(Direction.East, Direction.North.RotateClockwise());
        Assert.Equal(Direction.South, Direction.East.RotateClockwise());
        Assert.Equal(Direction.SouthEast, Direction.NorthEast.RotateClockwise());
    }

    #endregion
}
