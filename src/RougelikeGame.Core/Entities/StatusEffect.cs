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

    // IJ-2: 持続時間上限（スタック時のリフレッシュ制限）
    public int MaxDuration { get; init; } = 100;

    public StatusEffect(StatusEffectType type, int duration)
    {
        Type = type;
        Duration = duration;
        MaxDuration = Math.Max(duration * 2, 100);  // 初期値の2倍、最低100
        Name = type.ToString();

        // BG-1: 状態異常ごとのデフォルト倍率を設定
        (AttackMultiplier, DefenseMultiplier, DamagePerTick) = type switch
        {
            // 毒系（ダメージ効果）
            StatusEffectType.Poison => (1.0f, 1.0f, 3),
            StatusEffectType.Bleeding => (1.0f, 1.0f, 2),
            StatusEffectType.Burn => (0.85f, 1.0f, 4),
            StatusEffectType.Curse => (0.9f, 0.9f, 1),             // 呪い: 攻防低下+毎ターンダメージ
            StatusEffectType.InstantDeath => (1.0f, 1.0f, 999999),  // 即死: 超大ダメージ
            // デバフ（ステータス低下）
            StatusEffectType.Paralysis => (0.7f, 0.8f, 0),
            StatusEffectType.Weakness => (0.7f, 0.7f, 0),
            StatusEffectType.Vulnerability => (1.0f, 0.6f, 0),
            StatusEffectType.Slow => (0.8f, 1.0f, 0),
            StatusEffectType.Freeze => (0.5f, 1.0f, 0),            // 凍結: 攻撃力大幅低下
            StatusEffectType.Fear => (0.7f, 0.8f, 0),              // 恐怖: 攻防低下
            StatusEffectType.Blind => (0.7f, 1.0f, 0),             // 盲目: 攻撃力低下
            StatusEffectType.Confusion => (0.85f, 0.85f, 0),       // 混乱: 攻防やや低下
            StatusEffectType.Charm => (0.5f, 0.8f, 0),             // 魅了: 攻撃意欲喪失+防御低下
            StatusEffectType.Madness => (1.3f, 0.5f, 0),           // 狂気: 攻撃力上昇+防御大幅低下
            StatusEffectType.Stun => (0.5f, 0.8f, 0),              // スタン: 攻防低下
            StatusEffectType.Petrification => (0.0f, 3.0f, 0),     // 石化: 攻撃不能+防御大幅上昇
            StatusEffectType.Apostasy => (0.9f, 0.9f, 0),          // 背教: 攻防やや低下
            // バフ（ステータス上昇）
            StatusEffectType.Strength => (1.3f, 1.0f, 0),
            StatusEffectType.Blessing => (1.15f, 1.15f, 0),
            StatusEffectType.Haste => (1.2f, 1.0f, 0),             // 加速: 攻撃効率上昇
            StatusEffectType.Protection => (1.0f, 1.5f, 0),        // 防護: 防御力上昇
            StatusEffectType.Regeneration => (1.0f, 1.0f, -3),     // 再生: 回復効果（負値）
            // 効果なし（ステータス修正は他の仕組みで処理）
            // Sleep: 行動不能はTurnCostModifierで処理、ダメージ0はユーザー確認済み
            // Silence: 魔法封印は上位判定で処理
            // Invisibility/FireResistance/ColdResistance: 別システムで処理
            _ => (1.0f, 1.0f, 0)
        };

        // 行動速度修正のデフォルト設定
        TurnCostModifier = type switch
        {
            StatusEffectType.Haste => 0.75f,        // 加速: 行動コスト-25%
            StatusEffectType.Slow => 1.5f,           // 鈍足: 行動コスト+50%
            StatusEffectType.Paralysis => 1.5f,      // 麻痺: 行動コスト+50%
            StatusEffectType.Confusion => 1.3f,      // 混乱: 行動コスト+30%
            StatusEffectType.Freeze => 999f,         // 凍結: 行動不能
            StatusEffectType.Sleep => 999f,          // 睡眠: 行動不能
            StatusEffectType.Stun => 999f,           // スタン: 行動不能
            StatusEffectType.Petrification => 999f,  // 石化: 行動不能
            _ => 1.0f
        };

        // 命中率修正のデフォルト設定
        HitRateModifier = type switch
        {
            StatusEffectType.Blind => -0.5f,         // 盲目: 命中率-50%
            StatusEffectType.Invisibility => -0.1f,  // 透明: 命中率微減（視界不良）
            _ => 0f
        };

        // 回避率修正のデフォルト設定
        EvasionRateModifier = type switch
        {
            StatusEffectType.Invisibility => 0.5f,   // 透明: 回避率+50%
            StatusEffectType.Haste => 0.1f,          // 加速: 回避率+10%
            _ => 0f
        };

        // 全ステータス修正のデフォルト設定
        AllStatsMultiplier = type switch
        {
            StatusEffectType.Curse => 0.8f,          // 呪い: 全ステータス-20%
            StatusEffectType.Apostasy => 0.9f,       // 背教: 全ステータス-10%
            _ => 1.0f
        };
    }

    /// <summary>
    /// 効果を重ねる
    /// </summary>
    public void Stack(StatusEffect other)
    {
        if (other.Type != Type || other.StackCount <= 0) return;  // IJ-1: 負値/ゼロスタック防止

        StackCount = Math.Min(StackCount + other.StackCount, MaxStack);
        // IJ-2: 持続時間の上限を設ける（初期値の2倍まで）
        Duration = Math.Min(Math.Max(Duration, other.Duration), MaxDuration);
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
