using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// PetSystem（ペット・騎乗システム）のテスト
/// </summary>
public class PetSystemTests
{
    // --- ペット定義取得 ---

    [Fact]
    public void GetDefinition_Wolf_ReturnsDefinition()
    {
        var system = new PetSystem();

        var def = system.GetDefinition(PetType.Wolf);

        Assert.NotNull(def);
        Assert.Equal("ウルフ", def!.DefaultName);
    }

    [Fact]
    public void GetDefinition_AllTypes_Exist()
    {
        var system = new PetSystem();

        // 全6種の定義が存在する
        Assert.NotNull(system.GetDefinition(PetType.Wolf));
        Assert.NotNull(system.GetDefinition(PetType.Horse));
        Assert.NotNull(system.GetDefinition(PetType.Hawk));
        Assert.NotNull(system.GetDefinition(PetType.Cat));
        Assert.NotNull(system.GetDefinition(PetType.Bear));
        Assert.NotNull(system.GetDefinition(PetType.Dragon));
    }

    // --- ペット入手 ---

    [Fact]
    public void AddPet_CreatesWithInitialValues()
    {
        var system = new PetSystem();

        var pet = system.AddPet("p1", "タロウ", PetType.Wolf);

        Assert.Equal("タロウ", pet.Name);
        Assert.Equal(PetType.Wolf, pet.Type);
        Assert.Equal(1, pet.Level);
        Assert.Equal(100, pet.Hunger);
        Assert.Equal(50, pet.Loyalty);
        Assert.False(pet.IsRiding);
    }

    [Fact]
    public void AddPet_AppearsInPetsDictionary()
    {
        var system = new PetSystem();

        system.AddPet("p1", "テスト犬", PetType.Wolf);

        Assert.Single(system.Pets);
        Assert.True(system.Pets.ContainsKey("p1"));
    }

    // --- 餌やり ---

    [Fact]
    public void Feed_IncreasesHungerAndLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Cat);
        // 空腹度を下げてからテスト
        system.TickHunger("p1", 50);

        var result = system.Feed("p1", 30, 5);

        Assert.Equal(80, result.Hunger);  // 50 + 30
        Assert.True(result.Loyalty > 50); // 忠誠度も上昇
    }

    [Fact]
    public void Feed_HungerCappedAt100()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Cat);

        // 初期Hunger=100で餌やり → 100を超えない
        var result = system.Feed("p1", 30, 0);

        Assert.Equal(100, result.Hunger);
    }

    // --- しつけ ---

    [Fact]
    public void Train_IncreasesLoyalty()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);

        var result = system.Train("p1", 10);

        Assert.Equal(60, result.Loyalty); // 初期50 + 10
    }

    [Fact]
    public void Train_LoyaltyClamped_Max100()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);

        var result = system.Train("p1", 200);

        Assert.Equal(100, result.Loyalty);
    }

    [Fact]
    public void Train_LoyaltyClamped_Min0()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);

        var result = system.Train("p1", -200);

        Assert.Equal(0, result.Loyalty);
    }

    // --- 騎乗 ---

    [Fact]
    public void ToggleRide_Horse_CanRide()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト馬", PetType.Horse);

        var result = system.ToggleRide("p1");

        Assert.True(result.IsRiding);
    }

    [Fact]
    public void ToggleRide_Cat_CannotRide()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト猫", PetType.Cat);

        var result = system.ToggleRide("p1");

        Assert.False(result.IsRiding);
    }

    [Fact]
    public void ToggleRide_TwiceReturnsToOriginal()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト馬", PetType.Horse);

        system.ToggleRide("p1");
        var result = system.ToggleRide("p1");

        Assert.False(result.IsRiding);
    }

    // --- 移動速度倍率 ---

    [Fact]
    public void GetMoveSpeedMultiplier_RidingHorse_Returns2()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト馬", PetType.Horse);
        system.ToggleRide("p1");

        Assert.Equal(2.0f, system.GetMoveSpeedMultiplier("p1"));
    }

    [Fact]
    public void GetMoveSpeedMultiplier_NotRiding_Returns1()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト馬", PetType.Horse);

        Assert.Equal(1.0f, system.GetMoveSpeedMultiplier("p1"));
    }

    [Fact]
    public void GetMoveSpeedMultiplier_InvalidPet_Returns1()
    {
        var system = new PetSystem();

        Assert.Equal(1.0f, system.GetMoveSpeedMultiplier("nonexistent"));
    }

    // --- 空腹度減少 ---

    [Fact]
    public void TickHunger_DecreasesHunger()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);

        var result = system.TickHunger("p1", 10);

        Assert.Equal(90, result.Hunger);
    }

    [Fact]
    public void TickHunger_ZeroHunger_LoyaltyDecreases()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);

        // 空腹度を0にする
        var result = system.TickHunger("p1", 100);

        Assert.Equal(0, result.Hunger);
        Assert.Equal(48, result.Loyalty); // 初期50 - 2
    }

    // --- リセット ---

    [Fact]
    public void Reset_ClearsPets()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト犬", PetType.Wolf);
        system.AddPet("p2", "テスト猫", PetType.Cat);

        system.Reset();

        Assert.Empty(system.Pets);
    }

    // --- 命令成功率 ---

    [Fact]
    public void GetObedienceRate_ReturnsLoyaltyValue()
    {
        var system = new PetSystem();
        system.AddPet("p1", "テスト", PetType.Wolf);
        system.Train("p1", 30);

        Assert.Equal(80, system.GetObedienceRate("p1")); // 初期50 + 30
    }

    [Fact]
    public void GetObedienceRate_InvalidPet_ReturnsZero()
    {
        var system = new PetSystem();

        Assert.Equal(0, system.GetObedienceRate("nonexistent"));
    }
}
