namespace RougelikeGame.Core;

/// <summary>
/// 難易度別パラメータ設定
/// </summary>
public class DifficultySettings
{
    /// <summary>難易度レベル</summary>
    public DifficultyLevel Level { get; }

    /// <summary>表示名</summary>
    public string DisplayName { get; }

    /// <summary>説明</summary>
    public string Description { get; }

    /// <summary>敵ステータス倍率</summary>
    public double EnemyStatMultiplier { get; }

    /// <summary>獲得経験値倍率</summary>
    public double ExpMultiplier { get; }

    /// <summary>満腹度減少速度倍率</summary>
    public double HungerDecayMultiplier { get; }

    /// <summary>ターン制限倍率（1.0 = 1年）</summary>
    public double TurnLimitMultiplier { get; }

    /// <summary>救出（死に戻り）回数</summary>
    public int RescueCount { get; }

    /// <summary>アイテムドロップ倍率</summary>
    public double ItemDropMultiplier { get; }

    /// <summary>ゴールド獲得倍率</summary>
    public double GoldMultiplier { get; }

    /// <summary>被ダメージ倍率</summary>
    public double DamageTakenMultiplier { get; }

    /// <summary>与ダメージ倍率</summary>
    public double DamageDealtMultiplier { get; }

    /// <summary>死亡時にセーブデータを削除するか（Ironman用）</summary>
    public bool PermaDeath { get; }

    private DifficultySettings(
        DifficultyLevel level,
        string displayName,
        string description,
        double enemyStatMultiplier,
        double expMultiplier,
        double hungerDecayMultiplier,
        double turnLimitMultiplier,
        int rescueCount,
        double itemDropMultiplier,
        double goldMultiplier,
        double damageTakenMultiplier,
        double damageDealtMultiplier,
        bool permaDeath)
    {
        Level = level;
        DisplayName = displayName;
        Description = description;
        EnemyStatMultiplier = enemyStatMultiplier;
        ExpMultiplier = expMultiplier;
        HungerDecayMultiplier = hungerDecayMultiplier;
        TurnLimitMultiplier = turnLimitMultiplier;
        RescueCount = rescueCount;
        ItemDropMultiplier = itemDropMultiplier;
        GoldMultiplier = goldMultiplier;
        DamageTakenMultiplier = damageTakenMultiplier;
        DamageDealtMultiplier = damageDealtMultiplier;
        PermaDeath = permaDeath;
    }

    /// <summary>
    /// 難易度レベルから設定を取得
    /// </summary>
    public static DifficultySettings Get(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Easy => Easy,
        DifficultyLevel.Normal => Normal,
        DifficultyLevel.Hard => Hard,
        DifficultyLevel.Nightmare => Nightmare,
        DifficultyLevel.Ironman => IronmanSettings,
        _ => Normal
    };

    /// <summary>Easy: 初心者向け、緩い制限</summary>
    public static readonly DifficultySettings Easy = new(
        level: DifficultyLevel.Easy,
        displayName: "簡単",
        description: "初心者向け。敵が弱く、制限が緩い。",
        enemyStatMultiplier: 0.7,
        expMultiplier: 1.5,
        hungerDecayMultiplier: 0.5,
        turnLimitMultiplier: 1.5,
        rescueCount: 5,
        itemDropMultiplier: 1.5,
        goldMultiplier: 1.5,
        damageTakenMultiplier: 0.7,
        damageDealtMultiplier: 1.3,
        permaDeath: false
    );

    /// <summary>Normal: 標準難易度</summary>
    public static readonly DifficultySettings Normal = new(
        level: DifficultyLevel.Normal,
        displayName: "普通",
        description: "標準的な難易度。バランスの取れた冒険。",
        enemyStatMultiplier: 1.0,
        expMultiplier: 1.0,
        hungerDecayMultiplier: 1.0,
        turnLimitMultiplier: 1.0,
        rescueCount: 3,
        itemDropMultiplier: 1.0,
        goldMultiplier: 1.0,
        damageTakenMultiplier: 1.0,
        damageDealtMultiplier: 1.0,
        permaDeath: false
    );

    /// <summary>Hard: 上級者向け</summary>
    public static readonly DifficultySettings Hard = new(
        level: DifficultyLevel.Hard,
        displayName: "難しい",
        description: "上級者向け。敵が強く、資源が乏しい。",
        enemyStatMultiplier: 1.3,
        expMultiplier: 0.8,
        hungerDecayMultiplier: 1.3,
        turnLimitMultiplier: 0.8,
        rescueCount: 2,
        itemDropMultiplier: 0.7,
        goldMultiplier: 0.7,
        damageTakenMultiplier: 1.3,
        damageDealtMultiplier: 0.9,
        permaDeath: false
    );

    /// <summary>Nightmare: 極限の挑戦</summary>
    public static readonly DifficultySettings Nightmare = new(
        level: DifficultyLevel.Nightmare,
        displayName: "悪夢",
        description: "極限の挑戦。一瞬の油断が命取り。",
        enemyStatMultiplier: 1.6,
        expMultiplier: 0.6,
        hungerDecayMultiplier: 1.5,
        turnLimitMultiplier: 0.6,
        rescueCount: 1,
        itemDropMultiplier: 0.5,
        goldMultiplier: 0.5,
        damageTakenMultiplier: 1.6,
        damageDealtMultiplier: 0.8,
        permaDeath: false
    );

    /// <summary>Ironman: 永久死亡、セーブ削除</summary>
    public static readonly DifficultySettings IronmanSettings = new(
        level: DifficultyLevel.Ironman,
        displayName: "鉄人",
        description: "永久死亡。死はすべての終わり。セーブデータは削除される。",
        enemyStatMultiplier: 1.3,
        expMultiplier: 1.0,
        hungerDecayMultiplier: 1.2,
        turnLimitMultiplier: 0.8,
        rescueCount: 0,
        itemDropMultiplier: 0.8,
        goldMultiplier: 0.8,
        damageTakenMultiplier: 1.2,
        damageDealtMultiplier: 1.0,
        permaDeath: true
    );
}
