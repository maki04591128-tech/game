using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// 宗教システムのテスト（Phase 5.9-5.11）
/// </summary>
public class ReligionSystemTests
{
    private static Player CreateTestPlayer() =>
        Player.Create("テスト", Race.Human, CharacterClass.Fighter, Background.Adventurer);

    #region ReligionDatabase Tests

    [Fact]
    public void ReligionDatabase_GetAll_Returns6Religions()
    {
        var all = ReligionDatabase.GetAll().ToList();
        Assert.Equal(6, all.Count);
    }

    [Fact]
    public void ReligionDatabase_Count_Is6()
    {
        Assert.Equal(6, ReligionDatabase.Count);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, "光の神殿")]
    [InlineData(ReligionId.DarkCult, "闇の教団")]
    [InlineData(ReligionId.NatureWorship, "自然崇拝")]
    [InlineData(ReligionId.DeathFaith, "死神信仰")]
    [InlineData(ReligionId.ChaosCult, "混沌の崇拝")]
    [InlineData(ReligionId.Atheism, "無神論")]
    public void ReligionDatabase_GetById_ReturnsCorrectReligion(ReligionId id, string expectedName)
    {
        var religion = ReligionDatabase.GetById(id);
        Assert.NotNull(religion);
        Assert.Equal(expectedName, religion.Name);
    }

    [Fact]
    public void ReligionDatabase_GetById_None_ReturnsNull()
    {
        var religion = ReligionDatabase.GetById(ReligionId.None);
        Assert.Null(religion);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, "太陽神ソラリス")]
    [InlineData(ReligionId.DarkCult, "深淵神ニュクス")]
    [InlineData(ReligionId.NatureWorship, "大地母神ガイア")]
    [InlineData(ReligionId.DeathFaith, "死神タナトス")]
    [InlineData(ReligionId.ChaosCult, "混沌神カオス")]
    [InlineData(ReligionId.Atheism, "なし")]
    public void ReligionDatabase_EachReligion_HasCorrectGodName(ReligionId id, string expectedGod)
    {
        var religion = ReligionDatabase.GetById(id);
        Assert.NotNull(religion);
        Assert.Equal(expectedGod, religion.GodName);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, 6)]
    [InlineData(ReligionId.DarkCult, 6)]
    [InlineData(ReligionId.NatureWorship, 6)]
    [InlineData(ReligionId.DeathFaith, 6)]
    [InlineData(ReligionId.ChaosCult, 6)]
    [InlineData(ReligionId.Atheism, 6)]
    public void ReligionDatabase_EachReligion_Has6Benefits(ReligionId id, int expectedCount)
    {
        var religion = ReligionDatabase.GetById(id);
        Assert.NotNull(religion);
        Assert.Equal(expectedCount, religion.Benefits.Length);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, 3)]
    [InlineData(ReligionId.DarkCult, 2)]
    [InlineData(ReligionId.NatureWorship, 2)]
    [InlineData(ReligionId.DeathFaith, 4)]
    [InlineData(ReligionId.ChaosCult, 3)]
    [InlineData(ReligionId.Atheism, 1)]
    public void ReligionDatabase_EachReligion_HasCorrectTabooCount(ReligionId id, int expectedCount)
    {
        var religion = ReligionDatabase.GetById(id);
        Assert.NotNull(religion);
        Assert.Equal(expectedCount, religion.Taboos.Length);
    }

    [Theory]
    [InlineData(ReligionId.LightTemple, 4)]
    [InlineData(ReligionId.DarkCult, 4)]
    [InlineData(ReligionId.NatureWorship, 4)]
    [InlineData(ReligionId.DeathFaith, 4)]
    [InlineData(ReligionId.ChaosCult, 4)]
    [InlineData(ReligionId.Atheism, 0)]
    public void ReligionDatabase_EachReligion_HasCorrectSkillCount(ReligionId id, int expectedCount)
    {
        var religion = ReligionDatabase.GetById(id);
        Assert.NotNull(religion);
        Assert.Equal(expectedCount, religion.GrantedSkills.Length);
    }

    #endregion

    #region FaithRank Tests

    [Theory]
    [InlineData(0, FaithRank.None)]
    [InlineData(1, FaithRank.Believer)]
    [InlineData(20, FaithRank.Believer)]
    [InlineData(21, FaithRank.Devout)]
    [InlineData(40, FaithRank.Devout)]
    [InlineData(41, FaithRank.Blessed)]
    [InlineData(60, FaithRank.Blessed)]
    [InlineData(61, FaithRank.Priest)]
    [InlineData(80, FaithRank.Priest)]
    [InlineData(81, FaithRank.Champion)]
    [InlineData(99, FaithRank.Champion)]
    [InlineData(100, FaithRank.Saint)]
    public void GetFaithRank_ReturnsCorrectRank(int faithPoints, FaithRank expectedRank)
    {
        Assert.Equal(expectedRank, ReligionDefinition.GetFaithRank(faithPoints));
    }

    [Theory]
    [InlineData(FaithRank.None, "無信仰")]
    [InlineData(FaithRank.Believer, "信者")]
    [InlineData(FaithRank.Devout, "敬虔")]
    [InlineData(FaithRank.Blessed, "祝福者")]
    [InlineData(FaithRank.Priest, "司祭")]
    [InlineData(FaithRank.Champion, "聖騎士")]
    [InlineData(FaithRank.Saint, "聖人")]
    public void GetFaithRankName_ReturnsCorrectName(FaithRank rank, string expectedName)
    {
        Assert.Equal(expectedName, ReligionDefinition.GetFaithRankName(rank));
    }

    [Fact]
    public void GetRankTitle_LightTemple_ReturnsCustomTitles()
    {
        var religion = ReligionDatabase.GetById(ReligionId.LightTemple)!;
        Assert.Equal("太陽の聖人", religion.GetRankTitle(FaithRank.Saint));
        Assert.Equal("聖騎士", religion.GetRankTitle(FaithRank.Champion));
        Assert.Equal("光の信者", religion.GetRankTitle(FaithRank.Believer));
    }

    [Fact]
    public void GetRankTitle_DeathFaith_ReturnsCustomTitles()
    {
        var religion = ReligionDatabase.GetById(ReligionId.DeathFaith)!;
        Assert.Equal("タナトスの化身", religion.GetRankTitle(FaithRank.Saint));
        Assert.Equal("求道者", religion.GetRankTitle(FaithRank.Believer));
    }

    #endregion

    #region Religion Relations Tests

    [Fact]
    public void GetRelation_LightVsDark_IsHostile()
    {
        Assert.Equal(ReligionRelation.Hostile, ReligionDatabase.GetRelation(ReligionId.LightTemple, ReligionId.DarkCult));
    }

    [Fact]
    public void GetRelation_DarkVsDeath_IsAllied()
    {
        Assert.Equal(ReligionRelation.Allied, ReligionDatabase.GetRelation(ReligionId.DarkCult, ReligionId.DeathFaith));
    }

    [Fact]
    public void GetRelation_LightVsNature_IsFriendly()
    {
        Assert.Equal(ReligionRelation.Friendly, ReligionDatabase.GetRelation(ReligionId.LightTemple, ReligionId.NatureWorship));
    }

    [Fact]
    public void GetRelation_SameReligion_IsAllied()
    {
        Assert.Equal(ReligionRelation.Allied, ReligionDatabase.GetRelation(ReligionId.LightTemple, ReligionId.LightTemple));
    }

    [Fact]
    public void GetHostileReligions_LightTemple_ReturnsCorrect()
    {
        var hostile = ReligionDatabase.GetHostileReligions(ReligionId.LightTemple);
        Assert.Contains(ReligionId.DarkCult, hostile);
        Assert.Contains(ReligionId.DeathFaith, hostile);
        Assert.Contains(ReligionId.ChaosCult, hostile);
        Assert.DoesNotContain(ReligionId.NatureWorship, hostile);
    }

    [Fact]
    public void GetFriendlyReligions_DarkCult_ReturnsCorrect()
    {
        var friendly = ReligionDatabase.GetFriendlyReligions(ReligionId.DarkCult);
        Assert.Contains(ReligionId.DeathFaith, friendly);
        Assert.Contains(ReligionId.ChaosCult, friendly);
        Assert.DoesNotContain(ReligionId.LightTemple, friendly);
    }

    #endregion

    #region Apostasy Curse Tests

    [Fact]
    public void GetApostasyCurse_LightTemple_Returns30Days()
    {
        var curse = ReligionDatabase.GetApostasyCurse(ReligionId.LightTemple);
        Assert.NotNull(curse);
        Assert.Equal(30, curse.DurationDays);
        Assert.False(curse.IsPermanent);
    }

    [Fact]
    public void GetApostasyCurse_DarkCult_IsPermanent()
    {
        var curse = ReligionDatabase.GetApostasyCurse(ReligionId.DarkCult);
        Assert.NotNull(curse);
        Assert.True(curse.IsPermanent);
    }

    [Fact]
    public void GetApostasyCurse_None_ReturnsNull()
    {
        var curse = ReligionDatabase.GetApostasyCurse(ReligionId.None);
        Assert.Null(curse);
    }

    #endregion

    #region Rebirth Effect Tests

    [Theory]
    [InlineData(ReligionId.LightTemple, "神の導き")]
    [InlineData(ReligionId.DarkCult, "深淵の記憶")]
    [InlineData(ReligionId.NatureWorship, "循環の理")]
    [InlineData(ReligionId.DeathFaith, "死の祝福")]
    [InlineData(ReligionId.ChaosCult, "混沌の再誕")]
    [InlineData(ReligionId.Atheism, "経験の記録")]
    public void GetRebirthEffect_ReturnsCorrectEffect(ReligionId id, string expectedName)
    {
        var effect = ReligionDatabase.GetRebirthEffect(id);
        Assert.NotNull(effect);
        Assert.Equal(expectedName, effect.Name);
    }

    #endregion

    #region ReligionSystem - Join Tests

    [Fact]
    public void JoinReligion_Success_SetsFaith()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.JoinReligion(player, ReligionId.LightTemple);

        Assert.True(result.Success);
        Assert.Equal(ReligionId.LightTemple.ToString(), player.CurrentReligion);
        Assert.Equal(GameConstants.InitialFaithOnJoin, player.FaithPoints);
    }

    [Fact]
    public void JoinReligion_GrantsSkills()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        Assert.True(player.HasSkill("holy_light"));
        Assert.True(player.HasSkill("purify"));
        Assert.True(player.HasSkill("divine_protection"));
        Assert.True(player.HasSkill("divine_miracle"));
    }

    [Fact]
    public void JoinReligion_AlreadyJoined_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        var result = system.JoinReligion(player, ReligionId.LightTemple);

        Assert.False(result.Success);
    }

    [Fact]
    public void JoinReligion_InvalidId_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.JoinReligion(player, ReligionId.None);

        Assert.False(result.Success);
    }

    #endregion

    #region ReligionSystem - Convert Tests

    [Fact]
    public void JoinReligion_WhileInAnother_ConvertsReligion()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        var result = system.JoinReligion(player, ReligionId.DarkCult);

        Assert.True(result.Success);
        Assert.Equal(ReligionId.DarkCult.ToString(), player.CurrentReligion);
        Assert.Equal(GameConstants.InitialFaithOnConvert, player.FaithPoints);
    }

    [Fact]
    public void ConvertReligion_AppliesApostasyCurse()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        system.JoinReligion(player, ReligionId.DarkCult);

        Assert.True(player.HasApostasyCurse);
        Assert.Equal(30, player.ApostasyCurseRemainingDays); // 光の神殿は30日
    }

    [Fact]
    public void ConvertReligion_PermanentCurse_FromDarkCult()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.DarkCult);
        system.JoinReligion(player, ReligionId.LightTemple);

        Assert.True(player.HasApostasyCurse);
        Assert.Equal(-1, player.ApostasyCurseRemainingDays); // 闇の教団は永続
    }

    [Fact]
    public void ConvertReligion_ReducesSanity()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();
        int initialSanity = player.Sanity;

        system.JoinReligion(player, ReligionId.LightTemple);
        system.JoinReligion(player, ReligionId.DarkCult);

        Assert.Equal(initialSanity - GameConstants.ConversionSanityPenalty, player.Sanity);
    }

    [Fact]
    public void ConvertReligion_TracksPreviousReligion()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        system.JoinReligion(player, ReligionId.DarkCult);

        Assert.Equal(ReligionId.LightTemple.ToString(), player.PreviousReligion);
        Assert.Contains(ReligionId.LightTemple.ToString(), player.PreviousReligions);
    }

    #endregion

    #region ReligionSystem - Leave Tests

    [Fact]
    public void LeaveReligion_Success()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        var result = system.LeaveReligion(player);

        Assert.True(result.Success);
        Assert.Null(player.CurrentReligion);
        Assert.Equal(0, player.FaithPoints);
    }

    [Fact]
    public void LeaveReligion_NoReligion_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.LeaveReligion(player);

        Assert.False(result.Success);
    }

    [Fact]
    public void LeaveReligion_AppliesCurse()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.NatureWorship);
        system.LeaveReligion(player);

        Assert.True(player.HasApostasyCurse);
    }

    [Fact]
    public void LeaveReligion_ReducesSanity()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();
        int initialSanity = player.Sanity;

        system.JoinReligion(player, ReligionId.LightTemple);
        system.LeaveReligion(player);

        Assert.Equal(initialSanity - GameConstants.LeaveSanityPenalty, player.Sanity);
    }

    #endregion

    #region ReligionSystem - Pray Tests

    [Fact]
    public void Pray_IncreasesFaith()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        int faithBefore = player.FaithPoints;
        var result = system.Pray(player);

        Assert.True(result.Success);
        Assert.Equal(faithBefore + GameConstants.PrayFaithGain, player.FaithPoints);
    }

    [Fact]
    public void Pray_OncePerDay()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        system.Pray(player);
        var result = system.Pray(player);

        Assert.False(result.Success);
    }

    [Fact]
    public void Pray_AfterDailyTick_CanPrayAgain()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        system.Pray(player);
        system.ProcessDailyTick(player);
        var result = system.Pray(player);

        Assert.True(result.Success);
    }

    [Fact]
    public void Pray_NoReligion_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.Pray(player);

        Assert.False(result.Success);
    }

    #endregion

    #region ReligionSystem - Taboo Tests

    [Fact]
    public void ViolateTaboo_ReducesFaith()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        player.AddFaithPoints(50); // 信仰度を上げておく
        int faithBefore = player.FaithPoints;

        var result = system.ViolateTaboo(player, ReligionTabooType.UseDarkMagic);

        Assert.True(result.Success);
        Assert.True(player.FaithPoints < faithBefore);
    }

    [Fact]
    public void ViolateTaboo_NonTaboo_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        var result = system.ViolateTaboo(player, ReligionTabooType.EatMeat);

        Assert.False(result.Success); // 光の神殿に肉食禁止はない
    }

    [Fact]
    public void ViolateTaboo_NoReligion_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.ViolateTaboo(player, ReligionTabooType.UseDarkMagic);

        Assert.False(result.Success);
    }

    [Fact]
    public void ViolateTaboo_DarkCult_LightMagic_Penalty15()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.DarkCult);
        player.AddFaithPoints(50);
        int faithBefore = player.FaithPoints;

        system.ViolateTaboo(player, ReligionTabooType.UseLightMagic);

        Assert.Equal(faithBefore - 15, player.FaithPoints);
    }

    #endregion

    #region ReligionSystem - Benefits Tests

    [Fact]
    public void GetActiveBenefits_NoReligion_ReturnsEmpty()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var benefits = system.GetActiveBenefits(player);

        Assert.Empty(benefits);
    }

    [Fact]
    public void GetActiveBenefits_Believer_Returns1()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        // InitialFaithOnJoin = 20 → Believer rank

        var benefits = system.GetActiveBenefits(player);

        Assert.Single(benefits);
        Assert.Equal("光の加護", benefits[0].Name);
    }

    [Fact]
    public void GetActiveBenefits_HighFaith_ReturnsMultiple()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        player.AddFaithPoints(80); // total = 100 → Saint

        var benefits = system.GetActiveBenefits(player);

        Assert.Equal(6, benefits.Count); // All 6 benefits active
    }

    [Fact]
    public void GetBenefitValue_ReturnsSumOfMatchingBenefits()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.Atheism);
        player.AddFaithPoints(80); // total = 100 → Saint

        double value = system.GetBenefitValue(player, ReligionBenefitType.AllStatsBonus);

        Assert.True(value > 0);
    }

    [Fact]
    public void HasBenefit_ReturnsTrueWhenActive()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        Assert.True(system.HasBenefit(player, ReligionBenefitType.HealingBonus));
    }

    [Fact]
    public void HasBenefit_ReturnsFalseWhenNotActive()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        // Faith = 20 → Believer, no CriticalBonus benefit

        Assert.False(system.HasBenefit(player, ReligionBenefitType.CriticalBonus));
    }

    #endregion

    #region ReligionSystem - Daily Tick Tests

    [Fact]
    public void ProcessDailyTick_ResetsHasPrayedToday()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        system.Pray(player);
        Assert.True(player.HasPrayedToday);

        system.ProcessDailyTick(player);
        Assert.False(player.HasPrayedToday);
    }

    [Fact]
    public void ProcessDailyTick_IncrementsDaysSinceLastPrayer()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        system.ProcessDailyTick(player);
        Assert.Equal(1, player.DaysSinceLastPrayer);

        system.ProcessDailyTick(player);
        Assert.Equal(2, player.DaysSinceLastPrayer);
    }

    [Fact]
    public void ProcessDailyTick_FaithDecays_AfterInterval()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        player.AddFaithPoints(50);
        int faithBefore = player.FaithPoints;

        // DaysSinceLastPrayer をインターバルまで進める
        for (int i = 0; i < GameConstants.FaithDecayInterval; i++)
        {
            system.ProcessDailyTick(player);
        }

        Assert.Equal(faithBefore - GameConstants.FaithDecayAmount, player.FaithPoints);
    }

    [Fact]
    public void ProcessDailyTick_ApostasyCurse_Expires()
    {
        var player = CreateTestPlayer();
        player.HasApostasyCurse = true;
        player.ApostasyCurseRemainingDays = 2;

        var system = new ReligionSystem();

        system.ProcessDailyTick(player);
        Assert.True(player.HasApostasyCurse);
        Assert.Equal(1, player.ApostasyCurseRemainingDays);

        system.ProcessDailyTick(player);
        Assert.False(player.HasApostasyCurse);
    }

    [Fact]
    public void ProcessDailyTick_PermanentCurse_DoesNotExpire()
    {
        var player = CreateTestPlayer();
        player.HasApostasyCurse = true;
        player.ApostasyCurseRemainingDays = -1; // 永続

        var system = new ReligionSystem();

        for (int i = 0; i < 100; i++)
        {
            system.ProcessDailyTick(player);
        }

        Assert.True(player.HasApostasyCurse);
    }

    #endregion

    #region ReligionSystem - Death Transfer Tests

    [Fact]
    public void CalculateDeathTransferFaith_Normal_80Percent()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        player.AddFaithPoints(80); // total = 100

        int transfer = system.CalculateDeathTransferFaith(player);

        Assert.Equal(80, transfer); // 100 * 0.80
    }

    [Fact]
    public void CalculateDeathTransferFaith_DeathFaith_Priest_90Percent()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.DeathFaith);
        player.AddFaithPoints(50); // total = 70 → Priest

        int transfer = system.CalculateDeathTransferFaith(player);

        Assert.Equal((int)(70 * 0.90), transfer);
    }

    [Fact]
    public void CalculateDeathTransferFaith_DeathFaith_Saint_100Percent()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.DeathFaith);
        player.AddFaithPoints(80); // total = 100 → Saint

        int transfer = system.CalculateDeathTransferFaith(player);

        Assert.Equal(100, transfer); // 100 * 1.00
    }

    [Fact]
    public void GetRebirthEffect_WithReligion_ReturnsEffect()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        var effect = system.GetRebirthEffect(player);

        Assert.NotNull(effect);
        Assert.Equal("神の導き", effect.Name);
    }

    [Fact]
    public void GetRebirthEffect_NoReligion_ReturnsNull()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var effect = system.GetRebirthEffect(player);

        Assert.Null(effect);
    }

    #endregion

    #region ReligionSystem - Status Tests

    [Fact]
    public void GetStatus_NoReligion_ReturnsDefault()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var status = system.GetStatus(player);

        Assert.Equal("無信仰", status.ReligionName);
        Assert.Equal(FaithRank.None, status.Rank);
    }

    [Fact]
    public void GetStatus_WithReligion_ReturnsCorrectInfo()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        var status = system.GetStatus(player);

        Assert.Equal("光の神殿", status.ReligionName);
        Assert.Equal("太陽神ソラリス", status.GodName);
        Assert.Equal(FaithRank.Believer, status.Rank);
        Assert.Equal(GameConstants.InitialFaithOnJoin, status.FaithPoints);
        Assert.Equal("光の信者", status.Title);
    }

    #endregion

    #region ReligionSystem - Defeat Hostile Follower Tests

    [Fact]
    public void OnDefeatHostileFollower_IncreasesFaith()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);
        int faithBefore = player.FaithPoints;

        var result = system.OnDefeatHostileFollower(player, ReligionId.DarkCult);

        Assert.True(result.Success);
        Assert.True(player.FaithPoints > faithBefore);
    }

    [Fact]
    public void OnDefeatHostileFollower_NonHostile_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        system.JoinReligion(player, ReligionId.LightTemple);

        var result = system.OnDefeatHostileFollower(player, ReligionId.NatureWorship);

        Assert.False(result.Success);
    }

    [Fact]
    public void OnDefeatHostileFollower_NoReligion_Fails()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        var result = system.OnDefeatHostileFollower(player, ReligionId.DarkCult);

        Assert.False(result.Success);
    }

    #endregion

    #region Player Religion Properties Tests

    [Fact]
    public void Player_FaithCap_LimitsFaithPoints()
    {
        var player = CreateTestPlayer();
        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.AddFaithPoints(200);

        Assert.Equal(GameConstants.MaxFaithPoints, player.FaithPoints);
    }

    [Fact]
    public void Player_FaithPoints_CannotGoBelowZero()
    {
        var player = CreateTestPlayer();
        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.AddFaithPoints(10);
        player.AddFaithPoints(-100);

        Assert.Equal(0, player.FaithPoints);
    }

    [Fact]
    public void Player_Rejoin_ReducesFaithCap()
    {
        var player = CreateTestPlayer();

        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.LeaveReligion();

        // 再入信
        player.JoinReligion(ReligionId.LightTemple.ToString());

        Assert.Equal(GameConstants.MaxFaithPoints - GameConstants.MaxFaithCapReductionOnRejoin, player.FaithCap);
    }

    [Fact]
    public void Player_PreviousReligions_TracksHistory()
    {
        var player = CreateTestPlayer();

        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.LeaveReligion();

        player.JoinReligion(ReligionId.DarkCult.ToString());
        player.LeaveReligion();

        Assert.Contains(ReligionId.LightTemple.ToString(), player.PreviousReligions);
        Assert.Contains(ReligionId.DarkCult.ToString(), player.PreviousReligions);
    }

    [Fact]
    public void Player_TransferData_IncludesReligionHistory()
    {
        var player = CreateTestPlayer();

        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.LeaveReligion();
        player.JoinReligion(ReligionId.DarkCult.ToString());

        var data = player.CreateTransferData();

        Assert.Equal(ReligionId.DarkCult.ToString(), data.Religion);
        Assert.Equal(ReligionId.LightTemple.ToString(), data.PreviousReligion);
        Assert.Contains(ReligionId.LightTemple.ToString(), data.PreviousReligions);
    }

    [Fact]
    public void Player_ApplyTransferData_RestoresReligionHistory()
    {
        var player = CreateTestPlayer();
        var newPlayer = CreateTestPlayer();

        player.JoinReligion(ReligionId.LightTemple.ToString());
        player.LeaveReligion();
        player.JoinReligion(ReligionId.DarkCult.ToString());
        player.AddFaithPoints(50);

        var data = player.CreateTransferData();
        newPlayer.ApplyTransferData(data);

        Assert.Equal(ReligionId.DarkCult.ToString(), newPlayer.CurrentReligion);
        Assert.Equal(ReligionId.LightTemple.ToString(), newPlayer.PreviousReligion);
        Assert.Contains(ReligionId.LightTemple.ToString(), newPlayer.PreviousReligions);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullLifecycle_JoinPrayViolateLeave()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        // 入信
        var joinResult = system.JoinReligion(player, ReligionId.NatureWorship);
        Assert.True(joinResult.Success);
        Assert.Equal(GameConstants.InitialFaithOnJoin, player.FaithPoints);

        // 祈り
        system.Pray(player);
        Assert.Equal(GameConstants.InitialFaithOnJoin + GameConstants.PrayFaithGain, player.FaithPoints);

        // 禁忌違反
        int faithBefore = player.FaithPoints;
        system.ViolateTaboo(player, ReligionTabooType.EatMeat);
        Assert.True(player.FaithPoints < faithBefore);

        // 脱退
        system.LeaveReligion(player);
        Assert.Null(player.CurrentReligion);
        Assert.True(player.HasApostasyCurse);
    }

    [Fact]
    public void FullLifecycle_ConvertionAndRebirthEffect()
    {
        var player = CreateTestPlayer();
        var system = new ReligionSystem();

        // 光の神殿に入信
        system.JoinReligion(player, ReligionId.LightTemple);
        player.AddFaithPoints(80);

        // 死に戻り信仰度
        int transfer = system.CalculateDeathTransferFaith(player);
        Assert.Equal((int)(100 * GameConstants.FaithRetentionRate), transfer);

        // 闇の教団に改宗
        system.JoinReligion(player, ReligionId.DarkCult);
        Assert.True(player.HasApostasyCurse);

        // 死に戻り効果
        var rebirthEffect = system.GetRebirthEffect(player);
        Assert.NotNull(rebirthEffect);
        Assert.Equal("深淵の記憶", rebirthEffect.Name);
    }

    #endregion
}
