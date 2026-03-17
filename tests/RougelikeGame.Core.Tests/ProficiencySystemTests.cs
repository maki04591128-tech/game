using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

public class ProficiencySystemTests
{
    #region 初期化テスト

    [Fact]
    public void Constructor_InitializesAll12Categories()
    {
        var system = new ProficiencySystem();
        var all = system.GetAllProficiencies();

        Assert.Equal(12, all.Count);
        foreach (ProficiencyCategory cat in Enum.GetValues<ProficiencyCategory>())
        {
            Assert.True(all.ContainsKey(cat));
            Assert.Equal(0, all[cat].Level);
            Assert.Equal(0, all[cat].CurrentExp);
        }
    }

    [Fact]
    public void GetLevel_InitialState_ReturnsZero()
    {
        var system = new ProficiencySystem();
        Assert.Equal(0, system.GetLevel(ProficiencyCategory.Swordsmanship));
    }

    #endregion

    #region 経験値・レベルアップテスト

    [Fact]
    public void GainExperience_AddsExp()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 50);

        var data = system.GetAllProficiencies()[ProficiencyCategory.Swordsmanship];
        Assert.Equal(50, data.CurrentExp);
        Assert.Equal(0, data.Level);
    }

    [Fact]
    public void GainExperience_ExactlyRequiredExp_LevelsUp()
    {
        var system = new ProficiencySystem();
        // Level 0 requires 100 exp (100 * 1.15^0 = 100)
        system.GainExperience(ProficiencyCategory.Archery, 100);

        Assert.Equal(1, system.GetLevel(ProficiencyCategory.Archery));
    }

    [Fact]
    public void GainExperience_OverflowExp_CarriesOver()
    {
        var system = new ProficiencySystem();
        // Level 0 requires 100, give 150 → level 1 with 50 remaining
        system.GainExperience(ProficiencyCategory.Mining, 150);

        Assert.Equal(1, system.GetLevel(ProficiencyCategory.Mining));
        Assert.Equal(50, system.GetAllProficiencies()[ProficiencyCategory.Mining].CurrentExp);
    }

    [Fact]
    public void GainExperience_MultipleLevelUps()
    {
        var system = new ProficiencySystem();
        // Level 0 → 100, Level 1 → 115, total = 215 for 2 levels
        system.GainExperience(ProficiencyCategory.Sorcery, 215);

        Assert.Equal(2, system.GetLevel(ProficiencyCategory.Sorcery));
    }

    [Fact]
    public void GainExperience_ZeroAmount_NoEffect()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Stealth, 0);

        Assert.Equal(0, system.GetLevel(ProficiencyCategory.Stealth));
        Assert.Equal(0, system.GetAllProficiencies()[ProficiencyCategory.Stealth].CurrentExp);
    }

    [Fact]
    public void GainExperience_NegativeAmount_NoEffect()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Stealth, -10);

        Assert.Equal(0, system.GetLevel(ProficiencyCategory.Stealth));
    }

    [Fact]
    public void GainExperience_FiresOnLevelUp()
    {
        var system = new ProficiencySystem();
        ProficiencyLevelUpEventArgs? firedArgs = null;
        system.OnLevelUp += args => firedArgs = args;

        system.GainExperience(ProficiencyCategory.Faith, 100);

        Assert.NotNull(firedArgs);
        Assert.Equal(ProficiencyCategory.Faith, firedArgs.Category);
        Assert.Equal(0, firedArgs.OldLevel);
        Assert.Equal(1, firedArgs.NewLevel);
    }

    [Fact]
    public void GainExperience_AtMaxLevel_NoFurtherGain()
    {
        var system = new ProficiencySystem();
        // 直接レベルを100に設定
        var data = system.GetAllProficiencies()[ProficiencyCategory.Alchemy];
        data.Level = 100;
        data.CurrentExp = 0;

        system.GainExperience(ProficiencyCategory.Alchemy, 1000);

        Assert.Equal(100, system.GetLevel(ProficiencyCategory.Alchemy));
        Assert.Equal(0, data.CurrentExp);
    }

    #endregion

    #region RequiredExpテスト

    [Fact]
    public void GetRequiredExp_Level0_Returns100()
    {
        var data = new ProficiencyData(ProficiencyCategory.Swordsmanship);
        Assert.Equal(100, data.GetRequiredExp());
    }

    [Fact]
    public void GetRequiredExp_Level1_ReturnsExpectedValue()
    {
        var data = new ProficiencyData(ProficiencyCategory.Swordsmanship) { Level = 1 };
        // 100 * 1.15^1 = 115.0, (int)cast
        int expected = (int)(100 * Math.Pow(1.15, 1));
        Assert.Equal(expected, data.GetRequiredExp());
    }

    [Fact]
    public void GetRequiredExp_IncreasesExponentially()
    {
        var data = new ProficiencyData(ProficiencyCategory.Mining);
        int prevRequired = data.GetRequiredExp();
        data.Level = 10;
        int laterRequired = data.GetRequiredExp();

        Assert.True(laterRequired > prevRequired);
    }

    #endregion

    #region ボーナス計算テスト

    [Theory]
    [InlineData(0, 100, 0.0)]
    [InlineData(10, 100, 5.0)]
    [InlineData(50, 200, 50.0)]
    [InlineData(100, 100, 50.0)]
    public void GetBonusDamage_CalculatesCorrectly(int level, int baseDmg, double expected)
    {
        var system = new ProficiencySystem();
        var data = system.GetAllProficiencies()[ProficiencyCategory.Swordsmanship];
        data.Level = level;

        double bonus = system.GetBonusDamage(ProficiencyCategory.Swordsmanship, baseDmg);
        Assert.Equal(expected, bonus, 2);
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(50, 0.5)]
    [InlineData(100, 1.0)]
    public void GetBonusCraftQuality_CalculatesCorrectly(int smithingLevel, double expected)
    {
        var system = new ProficiencySystem();
        Assert.Equal(expected, system.GetBonusCraftQuality(smithingLevel), 2);
    }

    #endregion

    #region 武器種→熟練度カテゴリ変換テスト

    [Theory]
    [InlineData(WeaponType.Sword, ProficiencyCategory.Swordsmanship)]
    [InlineData(WeaponType.Greatsword, ProficiencyCategory.Swordsmanship)]
    [InlineData(WeaponType.Dagger, ProficiencyCategory.Swordsmanship)]
    [InlineData(WeaponType.Axe, ProficiencyCategory.Swordsmanship)]
    [InlineData(WeaponType.Greataxe, ProficiencyCategory.Swordsmanship)]
    [InlineData(WeaponType.Spear, ProficiencyCategory.Spearmanship)]
    [InlineData(WeaponType.Bow, ProficiencyCategory.Archery)]
    [InlineData(WeaponType.Crossbow, ProficiencyCategory.Archery)]
    [InlineData(WeaponType.Thrown, ProficiencyCategory.Archery)]
    [InlineData(WeaponType.Unarmed, ProficiencyCategory.MartialArts)]
    [InlineData(WeaponType.Fist, ProficiencyCategory.MartialArts)]
    [InlineData(WeaponType.Hammer, ProficiencyCategory.MartialArts)]
    [InlineData(WeaponType.Whip, ProficiencyCategory.MartialArts)]
    [InlineData(WeaponType.Staff, ProficiencyCategory.Sorcery)]
    public void GetWeaponProficiencyCategory_MapsCorrectly(WeaponType weapon, ProficiencyCategory expected)
    {
        Assert.Equal(expected, ProficiencySystem.GetWeaponProficiencyCategory(weapon));
    }

    #endregion

    #region Decayテスト

    [Fact]
    public void DecayUnusedProficiencies_ReducesUnusedExp()
    {
        var system = new ProficiencySystem();
        system.GainExperience(ProficiencyCategory.Swordsmanship, 50);
        system.GainExperience(ProficiencyCategory.Archery, 30);

        var used = new HashSet<ProficiencyCategory> { ProficiencyCategory.Swordsmanship };
        system.DecayUnusedProficiencies(used);

        // Swordsmanship は使用済みなので減少しない
        Assert.Equal(50, system.GetAllProficiencies()[ProficiencyCategory.Swordsmanship].CurrentExp);
        // Archery は未使用なので1減少
        Assert.Equal(29, system.GetAllProficiencies()[ProficiencyCategory.Archery].CurrentExp);
    }

    [Fact]
    public void DecayUnusedProficiencies_DoesNotGoBelowZero()
    {
        var system = new ProficiencySystem();
        // 経験値0の状態でDecay
        system.DecayUnusedProficiencies(new HashSet<ProficiencyCategory>());

        Assert.Equal(0, system.GetAllProficiencies()[ProficiencyCategory.Swordsmanship].CurrentExp);
    }

    #endregion
}
