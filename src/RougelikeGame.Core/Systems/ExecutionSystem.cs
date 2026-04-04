using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 処刑・止めの一撃システム - 瀕死の敵に対する特殊行動
/// </summary>
public static class ExecutionSystem
{
    /// <summary>処刑可能なHP閾値（最大HPの10%以下）</summary>
    public const float ExecutionThreshold = 0.10f;

    /// <summary>敵が処刑可能な状態か判定</summary>
    public static bool CanExecute(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return false;
        return (float)currentHp / maxHp <= ExecutionThreshold;
    }

    /// <summary>処刑時の経験値ボーナス倍率</summary>
    public static float GetExecutionExpBonus() => 1.5f;

    /// <summary>処刑時のドロップ率ボーナス倍率</summary>
    public static float GetExecutionDropBonus() => 1.3f;

    /// <summary>慈悲（見逃し）時のカルマボーナス</summary>
    public static int GetMercyKarmaBonus() => 5;

    /// <summary>処刑時のカルマペナルティ（敵種族による）</summary>
    public static int GetExecutionKarmaPenalty(MonsterRace race) => race switch
    {
        MonsterRace.Humanoid => -5,   // 人型は大きなペナルティ
        MonsterRace.Beast => -1,       // 獣は軽いペナルティ
        MonsterRace.Undead => 0,       // 不死には影響なし
        MonsterRace.Demon => 2,        // 悪魔はカルマ上昇
        MonsterRace.Dragon => -2,
        MonsterRace.Amorphous => -1,  // 不定形は軽いペナルティ
        _ => 0
    };

    /// <summary>処刑のアニメーション名を取得（武器種別）</summary>
    public static string GetExecutionAnimationName(WeaponType weaponType) => weaponType switch
    {
        WeaponType.Sword => "斬首",
        WeaponType.Dagger => "暗殺",
        WeaponType.Axe => "両断",
        WeaponType.Hammer => "粉砕",
        WeaponType.Spear => "貫通",
        WeaponType.Bow => "射殺",
        WeaponType.Greatsword => "大斬撃",     // DP-1: 追加
        WeaponType.Greataxe => "叩き割り",      // DP-1: 追加
        WeaponType.Staff => "魔力爆砕",         // DP-1: 追加
        WeaponType.Crossbow => "狙撃",          // DP-1: 追加
        WeaponType.Thrown => "投擲",             // DP-1: 追加
        WeaponType.Whip => "縛殺",              // DP-1: 追加
        WeaponType.Fist or WeaponType.Unarmed => "撲殺", // DP-1: 追加
        _ => "止めの一撃"
    };
}
