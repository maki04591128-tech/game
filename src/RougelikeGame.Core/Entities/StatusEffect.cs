namespace RougelikeGame.Core;

/// <summary>
/// 状態異常
/// </summary>
public class StatusEffect
{
    public StatusEffectType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Duration { get; private set; }
    public int StackCount { get; private set; } = 1;
    public int MaxStack { get; init; } = 1;
    public bool IsStackable => MaxStack > 1;
    public bool IsExpired => Duration <= 0;
    public int Priority { get; init; } = 0;
    public StatModifier? StatModifier { get; init; }

    // 倍率ベースの修正（戦闘中のバフ/デバフ用）
    public float AttackMultiplier { get; init; } = 1.0f;
    public float DefenseMultiplier { get; init; } = 1.0f;
    public float HitRateModifier { get; init; } = 0f;
    public float EvasionRateModifier { get; init; } = 0f;
    public float AllStatsMultiplier { get; init; } = 1.0f;

    // ダメージ系効果
    public int DamagePerTick { get; init; }
    public Element DamageElement { get; init; } = Element.None;

    // ターン修正
    public float TurnCostModifier { get; init; } = 1.0f;

    public StatusEffect(StatusEffectType type, int duration)
    {
        Type = type;
        Duration = duration;
        Name = type.ToString();

        // BG-1: 状態異常ごとのデフォルト倍率を設定
        (AttackMultiplier, DefenseMultiplier, DamagePerTick) = type switch
        {
            StatusEffectType.Poison => (1.0f, 1.0f, 3),
            StatusEffectType.Bleeding => (1.0f, 1.0f, 2),
            StatusEffectType.Burn => (0.85f, 1.0f, 4),
            StatusEffectType.Paralysis => (0.7f, 0.8f, 0),
            StatusEffectType.Weakness => (0.7f, 0.7f, 0),
            StatusEffectType.Vulnerability => (1.0f, 0.6f, 0),
            StatusEffectType.Strength => (1.3f, 1.0f, 0),
            StatusEffectType.Blessing => (1.15f, 1.15f, 0),
            StatusEffectType.Slow => (0.8f, 1.0f, 0),
            _ => (1.0f, 1.0f, 0)
        };
    }

    /// <summary>
    /// 効果を重ねる
    /// </summary>
    public void Stack(StatusEffect other)
    {
        if (other.Type != Type) return;

        StackCount = Math.Min(StackCount + other.StackCount, MaxStack);
        Duration = Math.Max(Duration, other.Duration);
    }

    /// <summary>
    /// ターン経過処理
    /// </summary>
    public void Tick()
    {
        Duration--;
    }

    /// <summary>
    /// ダメージ効果を持つか
    /// </summary>
    public bool HasDamageEffect => DamagePerTick > 0;

    /// <summary>
    /// ティック時のダメージを取得
    /// </summary>
    public Damage? GetTickDamage()
    {
        if (!HasDamageEffect) return null;

        return new Damage(
            DamagePerTick * StackCount,
            DamageType.Pure,
            DamageElement,
            false,
            Name
        );
    }
}

/// <summary>
/// 状態異常イベント引数
/// </summary>
public class StatusEffectEventArgs : EventArgs
{
    public StatusEffect Effect { get; }

    public StatusEffectEventArgs(StatusEffect effect)
    {
        Effect = effect;
    }
}
