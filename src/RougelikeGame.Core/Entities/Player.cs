using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Entities;

/// <summary>
/// プレイヤーキャラクター
/// </summary>
public class Player : Character, IPlayer, IInventoryHolder
{
    #region Player Properties
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }

    /// <summary>種族</summary>
    public Race Race { get; private set; } = Race.Human;
    /// <summary>職業</summary>
    public CharacterClass CharacterClass { get; internal set; } = CharacterClass.Fighter;  // BH-1: 転職用にinternal set
    /// <summary>素性</summary>
    public Background Background { get; private set; } = Background.Adventurer;
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

    private int _thirst;
    public int Thirst
    {
        get => _thirst;
        private set
        {
            var oldStage = GetThirstStage(_thirst);
            _thirst = Math.Clamp(value, 0, GameConstants.MaxThirst);
            var newStage = GetThirstStage(_thirst);

            if (oldStage != newStage)
            {
                OnThirstStageChanged?.Invoke(this, new ThirstStageEventArgs(oldStage, newStage));
            }
        }
    }
    public ThirstStage ThirstStage => GetThirstStage(Thirst);

    private int _fatigue;
    public int Fatigue
    {
        get => _fatigue;
        private set
        {
            var oldStage = GetFatigueStage(_fatigue);
            _fatigue = Math.Clamp(value, 0, GameConstants.MaxFatigue);
            var newStage = GetFatigueStage(_fatigue);

            if (oldStage != newStage)
            {
                OnFatigueStageChanged?.Invoke(this, new FatigueStageEventArgs(oldStage, newStage));
            }
        }
    }
    public FatigueStage FatigueStage => GetFatigueStage(Fatigue);

    private int _hygiene;
    public int Hygiene
    {
        get => _hygiene;
        private set
        {
            var oldStage = GetHygieneStage(_hygiene);
            _hygiene = Math.Clamp(value, 0, GameConstants.MaxHygiene);
            var newStage = GetHygieneStage(_hygiene);

            if (oldStage != newStage)
            {
                OnHygieneStageChanged?.Invoke(this, new HygieneStageEventArgs(oldStage, newStage));
            }
        }
    }
    public HygieneStage HygieneStage => GetHygieneStage(Hygiene);

    /// <summary>所持金</summary>
    public int Gold { get; private set; }

    /// <summary>ゴールドを獲得する</summary>
    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(this, new GoldChangedEventArgs(amount, Gold));
        }
    }

    /// <summary>ゴールドを消費する（成功時true）</summary>
    public bool SpendGold(int amount)
    {
        if (amount > 0 && Gold >= amount)
        {
            Gold -= amount;
            OnGoldChanged?.Invoke(this, new GoldChangedEventArgs(-amount, Gold));
            return true;
        }
        return false;
    }

    /// <summary>ゴールドを直接設定する（セーブ復元用）</summary>
    public void SetGold(int amount)
    {
        Gold = Math.Max(0, amount);
    }
    #endregion

    #region Rescue System
    public int RescueCountRemaining { get; private set; } = GameConstants.MaxRescueCount;
    public bool CanBeRescued => RescueCountRemaining > 0 && Sanity > 0;
    #endregion

    #region Inventory & Equipment
    public IInventory Inventory { get; } = new Inventory(GameConstants.DefaultInventorySize);
    public Items.Equipment Equipment { get; } = new();

    /// <summary>
    /// STRベースの最大所持重量を計算
    /// </summary>
    public float CalculateMaxWeight() =>
        GameConstants.BaseMaxWeight + EffectiveStats.Strength * GameConstants.WeightPerStrength;

    /// <summary>
    /// インベントリのMaxWeightをSTRに基づいて更新
    /// </summary>
    public void UpdateMaxWeight()
    {
        ((Inventory)Inventory).MaxWeight = CalculateMaxWeight();
    }

    /// <summary>
    /// 重量超過中かどうか
    /// </summary>
    public bool IsOverweight => ((Inventory)Inventory).TotalWeight > CalculateMaxWeight();

    // IInventoryHolder implementation
    public bool CanPickUp(Interfaces.IItem item)
    {
        if (Inventory.UsedSlots >= Inventory.MaxSlots)
            return false;

        // 重量チェック
        if (item is Items.Item concreteItem)
        {
            float itemWeight = concreteItem.Weight;
            if (concreteItem is IStackable stackable)
                itemWeight *= stackable.StackCount;
            if (((Inventory)Inventory).TotalWeight + itemWeight > CalculateMaxWeight())
                return false;
        }

        return true;
    }
    public void PickUp(Interfaces.IItem item)
    {
        if (item is Items.Item concreteItem)
            ((Inventory)Inventory).Add(concreteItem);
        // IF-1: 非Items.Item型の場合もログを残す（呼び出し側でワールドからは除去されないようにする）
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

    #region Stat Overrides
    /// <summary>種族・職業由来のHP/MPボーナス</summary>
    public int BonusMaxHp { get; set; }
    public int BonusMaxMp { get; set; }

    public override int MaxHp => base.MaxHp + BonusMaxHp + GetSkillTreeResourceBonus("MaxHp");
    public override int MaxMp => base.MaxMp + BonusMaxMp + GetSkillTreeResourceBonus("MaxMp");

    /// <summary>スキルツリーのパッシブボーナスを提供するコールバック（GameControllerから設定）</summary>
    public Func<Dictionary<string, int>>? SkillTreeBonusProvider { get; set; }

    /// <summary>スキルツリーから特定リソース（MaxHp/MaxMp等）のボーナスを取得</summary>
    private int GetSkillTreeResourceBonus(string key)
    {
        if (SkillTreeBonusProvider == null) return 0;
        var bonuses = SkillTreeBonusProvider();
        return bonuses.GetValueOrDefault(key);
    }

    protected override IEnumerable<StatModifier> GetAllStatModifiers()
    {
        // 状態異常ボーナス（基底クラスの処理）
        foreach (var mod in base.GetAllStatModifiers())
            yield return mod;

        // 装備ボーナス
        foreach (var mod in Equipment.GetStatModifiers())
            yield return mod;

        // スキルツリーパッシブボーナス（ステータス系のみ。MaxHp/MaxMpはMaxHp/MaxMpオーバーライドで処理）
        if (SkillTreeBonusProvider != null)
        {
            var bonuses = SkillTreeBonusProvider();
            yield return ConvertSkillBonusesToStatModifier(bonuses);
        }
    }

    /// <summary>スキルツリーボーナス辞書をStatModifierに変換（ステータス系のみ）</summary>
    private static StatModifier ConvertSkillBonusesToStatModifier(Dictionary<string, int> bonuses)
    {
        return new StatModifier(
            Strength: bonuses.GetValueOrDefault("STR") + bonuses.GetValueOrDefault("Strength"),
            Vitality: bonuses.GetValueOrDefault("VIT") + bonuses.GetValueOrDefault("Vitality"),
            Agility: bonuses.GetValueOrDefault("AGI") + bonuses.GetValueOrDefault("Agility"),
            Dexterity: bonuses.GetValueOrDefault("DEX") + bonuses.GetValueOrDefault("Dexterity"),
            Intelligence: bonuses.GetValueOrDefault("INT") + bonuses.GetValueOrDefault("Intelligence"),
            Mind: bonuses.GetValueOrDefault("MND") + bonuses.GetValueOrDefault("Mind"),
            Perception: bonuses.GetValueOrDefault("PER") + bonuses.GetValueOrDefault("Perception"),
            Charisma: bonuses.GetValueOrDefault("CHA") + bonuses.GetValueOrDefault("Charisma"),
            Luck: bonuses.GetValueOrDefault("LUK") + bonuses.GetValueOrDefault("Luck")
        );
    }
    #endregion

    #region Religion
    public string? CurrentReligion { get; private set; }
    public int FaithPoints { get; private set; }
    public string? PreviousReligion { get; private set; }
    public bool HasApostasyCurse { get; set; }
    public int ApostasyCurseRemainingDays { get; set; }
    public int DaysSinceLastPrayer { get; set; }
    public bool HasPrayedToday { get; set; }
    public HashSet<string> PreviousReligions { get; } = new();  // 過去に信仰した宗教のID
    public int FaithCap { get; set; } = GameConstants.MaxFaithPoints;  // 再入信時の信仰度上限

    public void JoinReligion(string religionId)
    {
        if (CurrentReligion != null)
        {
            LeaveReligion();
        }

        CurrentReligion = religionId;
        FaithPoints = 0;
        DaysSinceLastPrayer = 0;
        HasPrayedToday = false;

        // 再入信の場合、信仰度上限を下げる
        if (PreviousReligions.Contains(religionId))
        {
            FaithCap = Math.Max(0, GameConstants.MaxFaithPoints - GameConstants.MaxFaithCapReductionOnRejoin);
        }
        else
        {
            FaithCap = GameConstants.MaxFaithPoints;
        }

        OnReligionJoined?.Invoke(this, new ReligionEventArgs(religionId));
    }

    public void LeaveReligion()
    {
        if (CurrentReligion != null)
        {
            var oldReligion = CurrentReligion;
            PreviousReligion = oldReligion;
            PreviousReligions.Add(oldReligion);
            CurrentReligion = null;
            FaithPoints = 0;
            OnReligionLeft?.Invoke(this, new ReligionEventArgs(oldReligion));
        }
    }

    public void AddFaithPoints(int amount)
    {
        FaithPoints = Math.Clamp(FaithPoints + amount, 0, FaithCap);
    }
    #endregion

    #region Level System
    public void GainExperience(int amount)
    {
        // 種族の経験値倍率を適用
        var raceDef = RaceDefinition.Get(Race);
        int adjustedAmount = (int)(amount * raceDef.ExpMultiplier);
        Experience += adjustedAmount;

        while (Experience >= ExperienceToNextLevel && Level < GameConstants.MaxLevel)
        {
            Experience -= ExperienceToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;

        // 種族/職業に基づくステータス成長
        int oldMaxHp = MaxHp;
        int oldMaxMp = MaxMp;

        var growth = GrowthSystem.CalculateLevelUpBonus(Race, CharacterClass, Level);
        BaseStats = BaseStats.Apply(growth);

        // HP/MP成長分を現在値にも加算
        int hpGain = MaxHp - oldMaxHp;
        int mpGain = MaxMp - oldMaxMp;
        if (hpGain > 0) Heal(hpGain);
        if (mpGain > 0) RestoreMp(mpGain);

        // 最大重量を更新
        UpdateMaxWeight();

        OnLevelUp?.Invoke(this, new LevelUpEventArgs(Level));
    }

    private static int CalculateExpRequired(int level) =>
        (int)(GameConstants.BaseExpRequired * Math.Pow(GameConstants.ExpGrowthRate, level - 1));
    #endregion

    #region Sanity & Hunger & Thirst & Fatigue & Hygiene
    public void ModifySanity(int amount) => Sanity += amount;
    public void ModifyHunger(int amount) => Hunger += amount;
    public void ModifyThirst(int amount) => Thirst += amount;
    public void ModifyFatigue(int amount) => Fatigue += amount;
    public void ModifyHygiene(int amount) => Hygiene += amount;

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

    private static ThirstStage GetThirstStage(int thirst) => thirst switch
    {
        >= 80 => ThirstStage.Hydrated,
        >= 50 => ThirstStage.Thirsty,
        >= 25 => ThirstStage.Dehydrated,
        >= 1 => ThirstStage.SevereDehydration,
        _ => ThirstStage.CriticalDehydration
    };

    private static FatigueStage GetFatigueStage(int fatigue) => fatigue switch
    {
        >= 80 => FatigueStage.Fresh,
        >= 50 => FatigueStage.Mild,
        >= 25 => FatigueStage.Tired,
        >= 1 => FatigueStage.Exhausted,
        _ => FatigueStage.Collapse
    };

    private static HygieneStage GetHygieneStage(int hygiene) => hygiene switch
    {
        >= 80 => HygieneStage.Clean,
        >= 50 => HygieneStage.Normal,
        >= 25 => HygieneStage.Dirty,
        >= 1 => HygieneStage.Filthy,
        _ => HygieneStage.Foul
    };
    #endregion

    #region Death & Rebirth
    public void HandleDeath(DeathCause cause)
    {
        int baseSanityLoss = GetSanityLoss(cause);
        // 種族の正気度減少倍率を適用
        var raceDef = RaceDefinition.Get(Race);
        int sanityLoss = (int)(baseSanityLoss * raceDef.SanityLossMultiplier);
        sanityLoss = Math.Max(1, sanityLoss);
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
            PreviousReligion = PreviousReligion,
            PreviousReligions = new HashSet<string>(PreviousReligions),
            TotalDeaths = 0,  // 外部で管理
            RescueCountRemaining = RescueCountRemaining
        };
    }

    /// <summary>
    /// 引き継ぎデータを適用
    /// </summary>
    public void ApplyTransferData(TransferData data)
    {
        // 正気度を引き継ぐ（死に戻り後の減少値を反映）
        _sanity = Math.Clamp(data.Sanity, 0, GameConstants.MaxSanity);

        if (data.Sanity > 0)  // 正気度0でない場合のみ知識を引き継ぎ
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

            // 宗教履歴の引き継ぎ
            PreviousReligion = data.PreviousReligion;
            foreach (var prevReligion in data.PreviousReligions)
            {
                PreviousReligions.Add(prevReligion);
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
    /// <summary>
    /// 基本ステータスを指定してプレイヤーを作成するファクトリメソッド。
    /// これはプログラム的にPlayerオブジェクトを生成する処理であり、
    /// GUI上のキャラクター作成画面への遷移は発生しない。
    /// テスト用やカスタムステータス指定時に使用する。
    /// </summary>
    /// <param name="name">キャラクター名</param>
    /// <param name="baseStats">基礎ステータス</param>
    /// <returns>初期状態のPlayerインスタンス</returns>
    public static Player Create(string name, Stats baseStats)
    {
        var player = new Player
        {
            Name = name,
            BaseStats = baseStats,
            _sanity = GameConstants.InitialSanity,
            _hunger = GameConstants.InitialHunger,
            _thirst = GameConstants.InitialThirst,       // EX-1: 渇き初期化
            _fatigue = GameConstants.InitialFatigue,      // EX-1: 疲労初期化
            _hygiene = GameConstants.InitialHygiene,      // EX-1: 衛生初期化
            Faction = Faction.Player
        };

        player.InitializeResources();

        return player;
    }

    /// <summary>
    /// 種族・職業・素性を指定してプレイヤーを作成するファクトリメソッド。
    /// これはプログラム的にPlayerオブジェクトを生成する処理であり、
    /// GUI上のキャラクター作成画面への遷移は発生しない。
    /// 初回作成時はMainWindow経由でユーザーが選択した種族/職業/素性を渡し、
    /// 死に戻り時はExecuteRebirth()が前回と同じ種族/職業/素性を再利用して呼び出す。
    /// </summary>
    /// <param name="name">キャラクター名</param>
    /// <param name="race">種族</param>
    /// <param name="characterClass">職業</param>
    /// <param name="background">素性</param>
    /// <returns>種族・職業・素性ボーナス適用済みのPlayerインスタンス</returns>
    public static Player Create(string name, Race race, CharacterClass characterClass, Background background)
    {
        var raceDef = RaceDefinition.Get(race);
        var classDef = ClassDefinition.Get(characterClass);
        var bgDef = BackgroundDefinition.Get(background);

        // 基礎ステータスに種族・職業・素性のボーナスを適用
        var baseStats = Stats.Default
            .Apply(raceDef.StatBonus)
            .Apply(classDef.StatBonus)
            .Apply(bgDef.StatBonus);

        var player = new Player
        {
            Name = name,
            BaseStats = baseStats,
            Race = race,
            CharacterClass = characterClass,
            Background = background,
            _sanity = GameConstants.InitialSanity,
            _hunger = GameConstants.InitialHunger,
            _thirst = GameConstants.InitialThirst,
            _fatigue = GameConstants.InitialFatigue,
            _hygiene = GameConstants.InitialHygiene,
            Faction = Faction.Player
        };

        // 種族・職業によるHP/MPボーナス
        player.BonusMaxHp = raceDef.HpBonus + classDef.HpBonus;
        player.BonusMaxMp = raceDef.MpBonus + classDef.MpBonus;

        player.InitializeResources();

        // 素性による初期ゴールド
        player.AddGold(bgDef.StartingGold);

        // 職業による初期スキル
        foreach (var skill in classDef.InitialSkills)
        {
            player.LearnedSkills.Add(skill);
        }

        // 職業による初期ルーン語習得
        foreach (var wordId in GetInitialRuneWords(characterClass))
        {
            player.LearnWord(wordId);
        }

        return player;
    }

    /// <summary>職業に応じた初期ルーン語IDリストを返す</summary>
    private static string[] GetInitialRuneWords(CharacterClass cls) => cls switch
    {
        // 魔術師: 攻撃効果語+属性語+対象語（計5語）
        CharacterClass.Mage => ["brenna", "frysta", "fjandi", "eldr", "sjalfr"],
        // 僧侶: 回復・浄化+対象語（計4語）
        CharacterClass.Cleric => ["graeda", "hreinsa", "sjalfr", "vinir"],
        // 死霊術師: 闇系+召喚+対象語（計5語）
        CharacterClass.Necromancer => ["eyda", "kalla", "myrkr", "draugr", "fjandi"],
        // 吟遊詩人: 支援系+修飾語（計3語）
        CharacterClass.Bard => ["styrkja", "sjalfr", "litill"],
        // 錬金術師: 知識系+属性語（計3語）
        CharacterClass.Alchemist => ["vita", "sja", "eldr"],
        // 修道士: 回復+聖属性（計2語）
        CharacterClass.Monk => ["graeda", "sjalfr"],
        // 戦闘系（戦士・騎士・盗賊・狩人）: 基本1語のみ
        _ => ["sjalfr"]
    };

    /// <summary>
    /// セーブデータからプレイヤーを復元
    /// </summary>
    public void RestoreFromSave(int level, int experience, int sanity, int hunger,
        int currentHp, int currentMp, int currentSp, int rescueCountRemaining,
        Race race = Race.Human, CharacterClass characterClass = CharacterClass.Fighter,
        Background background = Background.Adventurer,
        int thirst = 100, int fatigue = 100, int hygiene = 100)
    {
        Level = level;
        Experience = experience;
        _sanity = Math.Clamp(sanity, 0, GameConstants.MaxSanity);
        _hunger = Math.Clamp(hunger, 0, GameConstants.MaxHunger);
        _thirst = Math.Clamp(thirst, 0, GameConstants.MaxThirst);
        _fatigue = Math.Clamp(fatigue, 0, GameConstants.MaxFatigue);
        _hygiene = Math.Clamp(hygiene, 0, GameConstants.MaxHygiene);
        CurrentHp = currentHp;
        CurrentMp = currentMp;
        CurrentSp = currentSp;
        RescueCountRemaining = rescueCountRemaining;
        Race = race;
        CharacterClass = characterClass;
        Background = background;

        // 種族・職業によるHP/MPボーナスを再計算
        var raceDef = RaceDefinition.Get(race);
        var classDef = ClassDefinition.Get(characterClass);
        BonusMaxHp = raceDef.HpBonus + classDef.HpBonus;
        BonusMaxMp = raceDef.MpBonus + classDef.MpBonus;
    }
    #endregion

    #region Events
    public event EventHandler<LevelUpEventArgs>? OnLevelUp;
    public event EventHandler<SanityStageEventArgs>? OnSanityStageChanged;
    public event EventHandler<HungerStageEventArgs>? OnHungerStageChanged;
    public event EventHandler<ThirstStageEventArgs>? OnThirstStageChanged;
    public event EventHandler<FatigueStageEventArgs>? OnFatigueStageChanged;
    public event EventHandler<HygieneStageEventArgs>? OnHygieneStageChanged;
    public event EventHandler<SkillEventArgs>? OnSkillLearned;
    public event EventHandler<MagicWordEventArgs>? OnMagicWordLearned;
    public event EventHandler<ReligionEventArgs>? OnReligionJoined;
    public event EventHandler<ReligionEventArgs>? OnReligionLeft;
    public event EventHandler<PlayerDeathEventArgs>? OnPlayerDeath;
    public event EventHandler<GoldChangedEventArgs>? OnGoldChanged;
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

public class ThirstStageEventArgs : EventArgs
{
    public ThirstStage OldStage { get; }
    public ThirstStage NewStage { get; }
    public ThirstStageEventArgs(ThirstStage oldStage, ThirstStage newStage)
    {
        OldStage = oldStage;
        NewStage = newStage;
    }
}

public class FatigueStageEventArgs : EventArgs
{
    public FatigueStage OldStage { get; }
    public FatigueStage NewStage { get; }
    public FatigueStageEventArgs(FatigueStage oldStage, FatigueStage newStage)
    {
        OldStage = oldStage;
        NewStage = newStage;
    }
}

public class HygieneStageEventArgs : EventArgs
{
    public HygieneStage OldStage { get; }
    public HygieneStage NewStage { get; }
    public HygieneStageEventArgs(HygieneStage oldStage, HygieneStage newStage)
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

public class GoldChangedEventArgs : EventArgs
{
    public int Amount { get; }
    public int NewTotal { get; }
    public GoldChangedEventArgs(int amount, int newTotal)
    {
        Amount = amount;
        NewTotal = newTotal;
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
    public string? PreviousReligion { get; set; }
    public HashSet<string> PreviousReligions { get; set; } = new();
    public int TotalDeaths { get; set; }
    public int RescueCountRemaining { get; set; } = GameConstants.MaxRescueCount;
    public int Sanity { get; set; } = GameConstants.InitialSanity;
}
#endregion
