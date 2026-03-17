namespace RougelikeGame.Core.Systems;

/// <summary>
/// 戦闘スタンス切替システム - 攻撃/防御/バランスの3スタンス
/// </summary>
public static class CombatStanceSystem
{
    /// <summary>スタンスによる攻撃力倍率</summary>
    public static float GetAttackModifier(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => 1.25f,
        CombatStance.Defensive => 0.75f,
        CombatStance.Balanced => 1.0f,
        _ => 1.0f
    };

    /// <summary>スタンスによる防御力倍率</summary>
    public static float GetDefenseModifier(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => 0.8f,
        CombatStance.Defensive => 1.3f,
        CombatStance.Balanced => 1.0f,
        _ => 1.0f
    };

    /// <summary>スタンスによる回避率補正</summary>
    public static float GetEvasionModifier(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => -0.1f,
        CombatStance.Defensive => 0.15f,
        CombatStance.Balanced => 0f,
        _ => 0f
    };

    /// <summary>スタンスによるクリティカル率補正</summary>
    public static float GetCriticalModifier(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => 0.1f,
        CombatStance.Defensive => -0.05f,
        CombatStance.Balanced => 0f,
        _ => 0f
    };

    /// <summary>スタンス名を取得</summary>
    public static string GetStanceName(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => "攻撃型",
        CombatStance.Defensive => "防御型",
        CombatStance.Balanced => "バランス型",
        _ => "不明"
    };

    /// <summary>スタンスの説明を取得</summary>
    public static string GetStanceDescription(CombatStance stance) => stance switch
    {
        CombatStance.Aggressive => "攻撃力+25%、防御-20%、回避-10%、クリ率+10%",
        CombatStance.Defensive => "防御+30%、回避+15%、攻撃力-25%、クリ率-5%",
        CombatStance.Balanced => "全パラメータ標準",
        _ => ""
    };
}
