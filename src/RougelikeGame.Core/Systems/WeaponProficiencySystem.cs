using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 武器種別プロファイル
/// </summary>
public record WeaponTypeProfile(
    WeaponType Type,
    AttackType PrimaryAttackType,
    AttackType? SecondaryAttackType,
    string ScalingStat,
    float CriticalModifier,
    float StaggerChance,
    bool CanParry = false,
    bool CanBlock = false
);

/// <summary>
/// 武器種別属性・習熟システム
/// WeaponTypeごとに固有の特性を定義し、スケーリングボーナスやダメージ計算を行う
/// </summary>
public static class WeaponProficiencySystem
{
    private static readonly Dictionary<WeaponType, WeaponTypeProfile> _profiles = new()
    {
        [WeaponType.Unarmed] = new(WeaponType.Unarmed, AttackType.Blunt, null,
            "STR", 1.0f, 0.05f),
        [WeaponType.Dagger] = new(WeaponType.Dagger, AttackType.Pierce, null,
            "DEX", 1.5f, 0.0f, CanParry: true),
        [WeaponType.Sword] = new(WeaponType.Sword, AttackType.Slash, AttackType.Pierce,
            "STR/DEX", 1.2f, 0.1f, CanParry: true),
        [WeaponType.Greatsword] = new(WeaponType.Greatsword, AttackType.Slash, null,
            "STR", 1.1f, 0.3f),
        [WeaponType.Axe] = new(WeaponType.Axe, AttackType.Slash, null,
            "STR", 1.3f, 0.2f),
        [WeaponType.Greataxe] = new(WeaponType.Greataxe, AttackType.Slash, null,
            "STR", 1.2f, 0.4f),
        [WeaponType.Spear] = new(WeaponType.Spear, AttackType.Pierce, null,
            "DEX", 1.1f, 0.15f),
        [WeaponType.Hammer] = new(WeaponType.Hammer, AttackType.Blunt, null,
            "STR", 1.0f, 0.35f),
        [WeaponType.Staff] = new(WeaponType.Staff, AttackType.Blunt, null,
            "INT", 0.8f, 0.1f),
        [WeaponType.Bow] = new(WeaponType.Bow, AttackType.Ranged, null,
            "DEX", 1.3f, 0.0f),
        [WeaponType.Crossbow] = new(WeaponType.Crossbow, AttackType.Ranged, null,
            "DEX", 1.5f, 0.1f),
        [WeaponType.Thrown] = new(WeaponType.Thrown, AttackType.Ranged, null,
            "DEX", 1.2f, 0.05f),
        [WeaponType.Whip] = new(WeaponType.Whip, AttackType.Slash, null,
            "DEX", 1.0f, 0.1f),
        [WeaponType.Fist] = new(WeaponType.Fist, AttackType.Blunt, null,
            "STR/DEX", 1.1f, 0.1f),
    };

    /// <summary>
    /// 武器種のプロファイルを取得
    /// </summary>
    public static WeaponTypeProfile GetWeaponProfile(WeaponType type)
    {
        return _profiles.TryGetValue(type, out var profile)
            ? profile
            : _profiles[WeaponType.Unarmed];
    }

    /// <summary>
    /// スケーリングステータスによるダメージボーナスを計算
    /// </summary>
    public static int GetScalingBonus(WeaponType type, Stats stats)
    {
        var profile = GetWeaponProfile(type);
        return profile.ScalingStat switch
        {
            "STR" => stats.Strength / 3,
            "DEX" => stats.Dexterity / 3,
            "INT" => stats.Intelligence / 3,
            "STR/DEX" => (stats.Strength + stats.Dexterity) / 5,
            _ => 0
        };
    }

    /// <summary>
    /// 武器ダメージをスケーリング込みで計算
    /// </summary>
    public static int CalculateWeaponDamage(Weapon weapon, Stats wielderStats, Random random)
    {
        int baseDmg = random.Next(weapon.DamageRange.Min, weapon.DamageRange.Max + 1);
        int enhancementBonus = weapon.EnhancementLevel * 2;
        int scalingBonus = GetScalingBonus(weapon.WeaponType, wielderStats);

        return Math.Max(1, baseDmg + enhancementBonus + scalingBonus);
    }

    /// <summary>
    /// 全武器種のプロファイル一覧を取得
    /// </summary>
    public static IReadOnlyDictionary<WeaponType, WeaponTypeProfile> GetAllProfiles()
    {
        return _profiles;
    }
}
