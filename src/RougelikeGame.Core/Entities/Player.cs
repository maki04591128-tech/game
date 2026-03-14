using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Entities;

/// <summary>
/// プレイヤーキャラクター
/// </summary>
public class Player : Character, IPlayer, IInventoryHolder
{
    #region Player Properties
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    public int ExperienceToNextLevel => CalculateExpRequired(Level);

    private int _sanity;
    public int Sanity
    {
        get => _sanity;
        private set
        {
            var oldStage = GetSanityStage(_sanity);
            _sanity = Math.Clamp(value, 0, GameConstants.MaxSanity);
            var newStage = GetSanityStage(_sanity);

            if (oldStage != newStage)
            {
                OnSanityStageChanged?.Invoke(this, new SanityStageEventArgs(oldStage, newStage));
            }
        }
    }
    public SanityStage SanityStage => GetSanityStage(Sanity);

    private int _hunger;
    public int Hunger
    {
        get => _hunger;
        private set
        {
            var oldStage = GetHungerStage(_hunger);
            _hunger = Math.Clamp(value, 0, GameConstants.MaxHunger);
            var newStage = GetHungerStage(_hunger);

            if (oldStage != newStage)
            {
                OnHungerStageChanged?.Invoke(this, new HungerStageEventArgs(oldStage, newStage));
            }
        }
    }
    public HungerStage HungerStage => GetHungerStage(Hunger);
    #endregion

    #region Rescue System
    public int RescueCountRemaining { get; private set; } = GameConstants.MaxRescueCount;
    public bool CanBeRescued => RescueCountRemaining > 0 && Sanity > 0;
    #endregion

    #region Inventory & Equipment
    public IInventory Inventory { get; } = new Inventory(GameConstants.DefaultInventorySize);
    public Items.Equipment Equipment { get; } = new();

    // IInventoryHolder implementation
    public bool CanPickUp(Interfaces.IItem item) => Inventory.UsedSlots < Inventory.MaxSlots;
    public void PickUp(Interfaces.IItem item)
    {
        if (item is Items.Item concreteItem)
            ((Inventory)Inventory).Add(concreteItem);
    }
    public void Drop(Interfaces.IItem item)
    {
        if (item is Items.Item concreteItem)
            ((Inventory)Inventory).Remove(concreteItem);
    }
    #endregion

    #region Magic Language
    public Dictionary<string, int> LearnedWords { get; } = new();  // ルーン語ID -> 理解度

    public void LearnWord(string wordId)
    {
        if (!LearnedWords.ContainsKey(wordId))
        {
            LearnedWords[wordId] = GameConstants.InitialWordMastery;
            OnMagicWordLearned?.Invoke(this, new MagicWordEventArgs(wordId));
        }
    }

    public void ImproveWordMastery(string wordId, int amount)
    {
        if (LearnedWords.TryGetValue(wordId, out int mastery))
        {
            LearnedWords[wordId] = Math.Min(GameConstants.MaxWordMastery, mastery + amount);
        }
    }

    public int GetWordMastery(string wordId) =>
        LearnedWords.TryGetValue(wordId, out int mastery) ? mastery : 0;
    #endregion

    #region Skills
    public HashSet<string> LearnedSkills { get; } = new();

    public void LearnSkill(string skillId)
    {
        if (LearnedSkills.Add(skillId))
        {
            OnSkillLearned?.Invoke(this, new SkillEventArgs(skillId));
        }
    }

    public bool HasSkill(string skillId) => LearnedSkills.Contains(skillId);
    #endregion

    #region Religion
    public string? CurrentReligion { get; private set; }
    public int FaithPoints { get; private set; }

    public void JoinReligion(string religionId)
    {
        if (CurrentReligion != null)
        {
            LeaveReligion();
        }

        CurrentReligion = religionId;
        FaithPoints = 0;
        OnReligionJoined?.Invoke(this, new ReligionEventArgs(religionId));
    }

    public void LeaveReligion()
    {
        if (CurrentReligion != null)
        {
            var oldReligion = CurrentReligion;
            CurrentReligion = null;
            FaithPoints = 0;
            OnReligionLeft?.Invoke(this, new ReligionEventArgs(oldReligion));
        }
    }

    public void AddFaithPoints(int amount) =>
        FaithPoints = Math.Max(0, FaithPoints + amount);
    #endregion

    #region Level System
    public void GainExperience(int amount)
    {
        Experience += amount;

        while (Experience >= ExperienceToNextLevel && Level < GameConstants.MaxLevel)
        {
            Experience -= ExperienceToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        // ステータスポイント付与などはここで行う
        OnLevelUp?.Invoke(this, new LevelUpEventArgs(Level));
    }

    private static int CalculateExpRequired(int level) =>
        (int)(GameConstants.BaseExpRequired * Math.Pow(GameConstants.ExpGrowthRate, level - 1));
    #endregion

    #region Sanity & Hunger
    public void ModifySanity(int amount) => Sanity += amount;
    public void ModifyHunger(int amount) => Hunger += amount;

    private static SanityStage GetSanityStage(int sanity) => sanity switch
    {
        >= 80 => SanityStage.Normal,
        >= 60 => SanityStage.Uneasy,
        >= 40 => SanityStage.Anxious,
        >= 20 => SanityStage.Unstable,
        >= 1 => SanityStage.Madness,
        _ => SanityStage.Broken
    };

    private static HungerStage GetHungerStage(int hunger) => hunger switch
    {
        >= 80 => HungerStage.Full,
        >= 50 => HungerStage.Normal,
        >= 25 => HungerStage.Hungry,
        >= 1 => HungerStage.Starving,
        _ => HungerStage.Famished
    };
    #endregion

    #region Death & Rebirth
    public void HandleDeath(DeathCause cause)
    {
        int sanityLoss = GetSanityLoss(cause);
        ModifySanity(-sanityLoss);

        if (CanBeRescued)
        {
            RescueCountRemaining--;
            // リスポーン処理は外部で行う
        }

        OnPlayerDeath?.Invoke(this, new PlayerDeathEventArgs(cause, sanityLoss, CanBeRescued));
    }

    private static int GetSanityLoss(DeathCause cause) => cause switch
    {
        DeathCause.Combat => SanityLoss.Combat,
        DeathCause.Boss => SanityLoss.Boss,
        DeathCause.Starvation => SanityLoss.Starvation,
        DeathCause.Trap => SanityLoss.Trap,
        DeathCause.TimeLimit => SanityLoss.TimeLimit,
        DeathCause.Curse => SanityLoss.Curse,
        DeathCause.Suicide => SanityLoss.Suicide,
        DeathCause.SanityDeath => SanityLoss.SanityDeath,
        DeathCause.Fall => SanityLoss.Fall,
        DeathCause.Poison => SanityLoss.Poison,
        _ => SanityLoss.Unknown
    };

    /// <summary>
    /// 引き継ぎデータを生成
    /// </summary>
    public TransferData CreateTransferData()
    {
        return new TransferData
        {
            LearnedWords = new Dictionary<string, int>(LearnedWords),
            LearnedSkills = new HashSet<string>(LearnedSkills),
            Religion = CurrentReligion,
            FaithPoints = FaithPoints,
            TotalDeaths = 0,  // 外部で管理
            RescueCountRemaining = RescueCountRemaining
        };
    }

    /// <summary>
    /// 引き継ぎデータを適用
    /// </summary>
    public void ApplyTransferData(TransferData data)
    {
        if (data.Sanity > 0)  // 正気度0でない場合のみ引き継ぎ
        {
            foreach (var (wordId, mastery) in data.LearnedWords)
            {
                LearnedWords[wordId] = mastery;
            }

            foreach (var skillId in data.LearnedSkills)
            {
                LearnedSkills.Add(skillId);
            }

            if (!string.IsNullOrEmpty(data.Religion))
            {
                CurrentReligion = data.Religion;
                FaithPoints = data.FaithPoints;
            }
        }

        RescueCountRemaining = data.RescueCountRemaining;
    }
    #endregion

    #region Turn System
    public override TurnAction DecideAction(IGameState state)
    {
        // プレイヤーは入力待ち
        return TurnAction.WaitForInput;
    }

    public override void ExecuteAction(TurnAction action, IGameState state)
    {
        switch (action.Type)
        {
            case TurnActionType.Move:
                Position = Position.Move(action.Direction);
                FacingDirection = action.Direction;
                break;

            case TurnActionType.Attack:
                if (action.Target != null)
                {
                    state.CombatSystem.ExecuteAttack(this, action.Target, AttackType.Slash);
                }
                break;

            case TurnActionType.Wait:
                RestoreSp(5);
                break;

            case TurnActionType.Rest:
                Heal(MaxHp / 10);
                RestoreMp(MaxMp / 10);
                RestoreSp(MaxSp / 2);
                break;

            case TurnActionType.UseSkill:
                // スキル実行は外部システムで処理
                break;

            case TurnActionType.CastSpell:
                // 魔法詠唱は外部システムで処理
                break;

            case TurnActionType.UseItem:
                // アイテム使用は外部システムで処理
                break;
        }

        TickStatusEffects();
    }
    #endregion

    #region Initialization
    public static Player Create(string name, Stats baseStats)
    {
        var player = new Player
        {
            Name = name,
            BaseStats = baseStats,
            _sanity = GameConstants.InitialSanity,
            _hunger = GameConstants.InitialHunger,
            Faction = Faction.Player
        };

        player.InitializeResources();

        return player;
    }

    /// <summary>
    /// セーブデータからプレイヤーを復元
    /// </summary>
    public void RestoreFromSave(int level, int experience, int sanity, int hunger,
        int currentHp, int currentMp, int currentSp, int rescueCountRemaining)
    {
        Level = level;
        Experience = experience;
        _sanity = Math.Clamp(sanity, 0, GameConstants.MaxSanity);
        _hunger = Math.Clamp(hunger, 0, GameConstants.MaxHunger);
        CurrentHp = currentHp;
        CurrentMp = currentMp;
        CurrentSp = currentSp;
        RescueCountRemaining = rescueCountRemaining;
    }
    #endregion

    #region Events
    public event EventHandler<LevelUpEventArgs>? OnLevelUp;
    public event EventHandler<SanityStageEventArgs>? OnSanityStageChanged;
    public event EventHandler<HungerStageEventArgs>? OnHungerStageChanged;
    public event EventHandler<SkillEventArgs>? OnSkillLearned;
    public event EventHandler<MagicWordEventArgs>? OnMagicWordLearned;
    public event EventHandler<ReligionEventArgs>? OnReligionJoined;
    public event EventHandler<ReligionEventArgs>? OnReligionLeft;
    public event EventHandler<PlayerDeathEventArgs>? OnPlayerDeath;
    #endregion
}

#region Event Args
public class LevelUpEventArgs : EventArgs
{
    public int NewLevel { get; }
    public LevelUpEventArgs(int newLevel) => NewLevel = newLevel;
}

public class SanityStageEventArgs : EventArgs
{
    public SanityStage OldStage { get; }
    public SanityStage NewStage { get; }
    public SanityStageEventArgs(SanityStage oldStage, SanityStage newStage)
    {
        OldStage = oldStage;
        NewStage = newStage;
    }
}

public class HungerStageEventArgs : EventArgs
{
    public HungerStage OldStage { get; }
    public HungerStage NewStage { get; }
    public HungerStageEventArgs(HungerStage oldStage, HungerStage newStage)
    {
        OldStage = oldStage;
        NewStage = newStage;
    }
}

public class SkillEventArgs : EventArgs
{
    public string SkillId { get; }
    public SkillEventArgs(string skillId) => SkillId = skillId;
}

public class MagicWordEventArgs : EventArgs
{
    public string WordId { get; }
    public MagicWordEventArgs(string wordId) => WordId = wordId;
}

public class ReligionEventArgs : EventArgs
{
    public string ReligionId { get; }
    public ReligionEventArgs(string religionId) => ReligionId = religionId;
}

public class PlayerDeathEventArgs : EventArgs
{
    public DeathCause Cause { get; }
    public int SanityLoss { get; }
    public bool WillBeRescued { get; }
    public PlayerDeathEventArgs(DeathCause cause, int sanityLoss, bool willBeRescued)
    {
        Cause = cause;
        SanityLoss = sanityLoss;
        WillBeRescued = willBeRescued;
    }
}
#endregion

#region Transfer Data
public class TransferData
{
    public Dictionary<string, int> LearnedWords { get; set; } = new();
    public HashSet<string> LearnedSkills { get; set; } = new();
    public string? Religion { get; set; }
    public int FaithPoints { get; set; }
    public int TotalDeaths { get; set; }
    public int RescueCountRemaining { get; set; } = GameConstants.MaxRescueCount;
    public int Sanity { get; set; } = GameConstants.InitialSanity;
}
#endregion
