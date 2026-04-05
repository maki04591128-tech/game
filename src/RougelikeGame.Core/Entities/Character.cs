using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Entities;

/// <summary>
/// 全てのキャラクター（プレイヤー、敵、NPC）の基底クラス
/// </summary>
public abstract class Character : IEntity, ITurnActor, IDamageable
{
    #region Identity
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    #endregion

    #region Position
    public Position Position { get; set; }
    public Direction FacingDirection { get; set; } = Direction.South;
    #endregion

    #region Stats
    public Stats BaseStats { get; protected internal set; } = Stats.Default;

    public Stats EffectiveStats => CalculateEffectiveStats();

    protected virtual Stats CalculateEffectiveStats()
    {
        var modifiers = GetAllStatModifiers();
        return BaseStats.ApplyAll(modifiers);
    }

    protected virtual IEnumerable<StatModifier> GetAllStatModifiers()
    {
        return StatusEffects
            .Where(e => e.StatModifier.HasValue)
            .Select(e => e.StatModifier!.Value);
    }
    #endregion

    #region Resources
    private int _currentHp;
    public int CurrentHp
    {
        get => _currentHp;
        protected set => _currentHp = Math.Clamp(value, 0, MaxHp);
    }
    public virtual int MaxHp => EffectiveStats.MaxHp;

    private int _currentMp;
    public int CurrentMp
    {
        get => _currentMp;
        protected set => _currentMp = Math.Clamp(value, 0, MaxMp);
    }
    public virtual int MaxMp => EffectiveStats.MaxMp;

    private int _currentSp;
    public int CurrentSp
    {
        get => _currentSp;
        protected set => _currentSp = Math.Clamp(value, 0, MaxSp);
    }
    public int MaxSp => EffectiveStats.MaxSp;

    /// <summary>
    /// リソースを初期化（最大値に設定）
    /// </summary>
    public virtual void InitializeResources()
    {
        _currentHp = MaxHp;
        _currentMp = MaxMp;
        _currentSp = MaxSp;
    }
    #endregion

    #region Status
    public bool IsAlive => CurrentHp > 0;
    public Faction Faction { get; set; } = Faction.Neutral;
    public List<StatusEffect> StatusEffects { get; } = new();
    #endregion

    #region Combat
    public virtual void TakeDamage(Damage damage)
    {
        int defense = damage.Type switch
        {
            DamageType.Physical => EffectiveStats.PhysicalDefense,
            DamageType.Magical => EffectiveStats.MagicalDefense,
            DamageType.Pure => 0,      // AI-1: 貫通ダメージ（防御無視）
            DamageType.Healing => 0,   // AI-1: 回復は防御不要
            _ => 0
        };

        float resistance = GetResistanceAgainst(damage.Element);
        int finalDamage = damage.CalculateFinal(defense, resistance);

        CurrentHp -= finalDamage;

        OnDamaged?.Invoke(this, new DamageEventArgs(damage, finalDamage, CurrentHp));

        if (!IsAlive)
        {
            OnDeath?.Invoke(this, new DeathEventArgs(damage));
        }
    }

    protected virtual float GetResistanceAgainst(Element element) => 0f;

    public virtual void Heal(int amount)
    {
        amount = Math.Max(0, amount);  // AH-5b: 負値ガード
        int oldHp = CurrentHp;
        CurrentHp += amount;
        int actualHeal = CurrentHp - oldHp;

        OnHealed?.Invoke(this, new HealEventArgs(actualHeal, CurrentHp));
    }

    public virtual void RestoreMp(int amount) => CurrentMp += Math.Max(0, amount);
    public virtual void RestoreSp(int amount) => CurrentSp += Math.Max(0, amount);

    public virtual void ConsumeMp(int amount) => CurrentMp -= Math.Max(0, amount);  // AH-5: 負値ガード
    public virtual void ConsumeSp(int amount) => CurrentSp -= Math.Max(0, amount);
    #endregion

    #region Status Effects
    public void ApplyStatusEffect(StatusEffect effect)
    {
        // AR-6: 相反するバフ/デバフの共存チェック（新しい効果で古い効果を上書き）
        var conflicting = GetConflictingEffectType(effect.Type);
        if (conflicting.HasValue)
        {
            RemoveStatusEffect(conflicting.Value);
        }

        var existing = StatusEffects.FirstOrDefault(e => e.Type == effect.Type);

        if (existing != null)
        {
            if (effect.IsStackable)
            {
                existing.Stack(effect);
            }
            else if (effect.Priority >= existing.Priority)
            {
                StatusEffects.Remove(existing);
                StatusEffects.Add(effect);
            }
        }
        else
        {
            StatusEffects.Add(effect);
        }

        OnStatusEffectApplied?.Invoke(this, new StatusEffectEventArgs(effect));
    }

    public void RemoveStatusEffect(StatusEffectType type)
    {
        var effect = StatusEffects.FirstOrDefault(e => e.Type == type);
        if (effect != null)
        {
            StatusEffects.Remove(effect);
            OnStatusEffectRemoved?.Invoke(this, new StatusEffectEventArgs(effect));
        }
    }

    public bool HasStatusEffect(StatusEffectType type) =>
        StatusEffects.Any(e => e.Type == type);

    /// <summary>AR-6: 相反する状態異常の対応を取得</summary>
    private static StatusEffectType? GetConflictingEffectType(StatusEffectType type) => type switch
    {
        StatusEffectType.Strength => StatusEffectType.Weakness,
        StatusEffectType.Weakness => StatusEffectType.Strength,
        StatusEffectType.Haste => StatusEffectType.Slow,
        StatusEffectType.Slow => StatusEffectType.Haste,
        StatusEffectType.Protection => StatusEffectType.Vulnerability,
        StatusEffectType.Vulnerability => StatusEffectType.Protection,
        StatusEffectType.Regeneration => StatusEffectType.Poison,
        StatusEffectType.Poison => StatusEffectType.Regeneration,
        StatusEffectType.FireResistance => StatusEffectType.Burn,
        StatusEffectType.Burn => StatusEffectType.FireResistance,
        StatusEffectType.ColdResistance => StatusEffectType.Freeze,
        StatusEffectType.Freeze => StatusEffectType.ColdResistance,
        _ => null
    };

    public void TickStatusEffects()
    {
        var expiredEffects = new List<StatusEffect>();

        foreach (var effect in StatusEffects)
        {
            // ダメージ効果を適用
            var tickDamage = effect.GetTickDamage();
            if (tickDamage.HasValue)
            {
                TakeDamage(tickDamage.Value);
            }

            effect.Tick();

            if (effect.IsExpired)
            {
                expiredEffects.Add(effect);
            }
        }

        foreach (var effect in expiredEffects)
        {
            StatusEffects.Remove(effect);
            OnStatusEffectRemoved?.Invoke(this, new StatusEffectEventArgs(effect));
        }
    }

    /// <summary>
    /// 状態異常によるターン消費修正を取得
    /// </summary>
    public float GetStatusEffectTurnModifier()
    {
        float modifier = 1.0f;
        foreach (var effect in StatusEffects)
        {
            modifier *= effect.TurnCostModifier;
        }
        return Math.Max(0.1f, modifier);
    }
    #endregion

    #region Turn System
    public virtual int ActionPriority => EffectiveStats.ActionSpeed;

    public abstract TurnAction DecideAction(IGameState state);
    public abstract void ExecuteAction(TurnAction action, IGameState state);
    #endregion

    #region Events
    public event EventHandler<DamageEventArgs>? OnDamaged;
    public event EventHandler<DeathEventArgs>? OnDeath;
    public event EventHandler<HealEventArgs>? OnHealed;
    public event EventHandler<StatusEffectEventArgs>? OnStatusEffectApplied;
    public event EventHandler<StatusEffectEventArgs>? OnStatusEffectRemoved;
    #endregion
}
