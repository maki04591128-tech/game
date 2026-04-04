using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 属性耐性段階
/// </summary>
public enum ElementalResistanceLevel
{
    /// <summary>弱点（×1.5ダメージ）</summary>
    Weakness,
    /// <summary>通常（×1.0ダメージ）</summary>
    Normal,
    /// <summary>耐性（×0.5ダメージ）</summary>
    Resistant,
    /// <summary>無効（×0ダメージ）</summary>
    Immune,
    /// <summary>吸収（HP回復）</summary>
    Absorb
}

/// <summary>
/// 属性相性システム - 種族×属性の相性テーブルとダメージ計算を体系化
/// </summary>
public static class ElementalAffinitySystem
{
    /// <summary>種族×属性の相性テーブル</summary>
    private static readonly Dictionary<(MonsterRace Race, Element Element), ElementalResistanceLevel> AffinityTable = new()
    {
        // Beast: Fire弱点
        [(MonsterRace.Beast, Element.Fire)] = ElementalResistanceLevel.Weakness,

        // Humanoid: 全Normal（デフォルトなのでエントリ不要だが明示）

        // Amorphous: Magic（Fire/Lightning/Ice）弱点
        [(MonsterRace.Amorphous, Element.Fire)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Amorphous, Element.Lightning)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Amorphous, Element.Ice)] = ElementalResistanceLevel.Weakness,

        // Undead: Holy/Light弱点、Poison/Dark耐性
        [(MonsterRace.Undead, Element.Holy)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Undead, Element.Light)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Undead, Element.Poison)] = ElementalResistanceLevel.Resistant,
        [(MonsterRace.Undead, Element.Dark)] = ElementalResistanceLevel.Resistant,

        // Demon: Holy弱点、Dark/Curse耐性
        [(MonsterRace.Demon, Element.Holy)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Demon, Element.Dark)] = ElementalResistanceLevel.Resistant,
        [(MonsterRace.Demon, Element.Curse)] = ElementalResistanceLevel.Resistant,

        // Dragon: Ice弱点（全ドラゴン共通の基本設定）
        [(MonsterRace.Dragon, Element.Ice)] = ElementalResistanceLevel.Weakness,

        // Plant: Fire/Ice弱点、Earth/Water耐性
        [(MonsterRace.Plant, Element.Fire)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Plant, Element.Ice)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Plant, Element.Earth)] = ElementalResistanceLevel.Resistant,
        [(MonsterRace.Plant, Element.Water)] = ElementalResistanceLevel.Resistant,

        // Insect: Fire弱点
        [(MonsterRace.Insect, Element.Fire)] = ElementalResistanceLevel.Weakness,

        // Spirit: 対属性（Light→Dark弱点等）
        [(MonsterRace.Spirit, Element.Light)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Spirit, Element.Dark)] = ElementalResistanceLevel.Weakness,

        // Construct: Lightning弱点、Poison無効
        [(MonsterRace.Construct, Element.Lightning)] = ElementalResistanceLevel.Weakness,
        [(MonsterRace.Construct, Element.Poison)] = ElementalResistanceLevel.Immune,
    };

    /// <summary>物理攻撃種別×種族の相性テーブル</summary>
    private static readonly Dictionary<(AttackType Attack, MonsterRace Race), float> PhysicalAffinityTable = new()
    {
        // Amorphous: Slash/Pierce→×0.5, Blunt→×1.5
        [(AttackType.Slash, MonsterRace.Amorphous)] = 0.5f,
        [(AttackType.Pierce, MonsterRace.Amorphous)] = 0.5f,
        [(AttackType.Blunt, MonsterRace.Amorphous)] = 1.5f,

        // Construct: Blunt→×1.5, Slash/Pierce→×0.7
        [(AttackType.Blunt, MonsterRace.Construct)] = 1.5f,
        [(AttackType.Slash, MonsterRace.Construct)] = 0.7f,
        [(AttackType.Pierce, MonsterRace.Construct)] = 0.7f,

        // Spirit: Physical全般→×0.5
        [(AttackType.Unarmed, MonsterRace.Spirit)] = 0.5f,
        [(AttackType.Slash, MonsterRace.Spirit)] = 0.5f,
        [(AttackType.Pierce, MonsterRace.Spirit)] = 0.5f,
        [(AttackType.Blunt, MonsterRace.Spirit)] = 0.5f,
        [(AttackType.Ranged, MonsterRace.Spirit)] = 0.5f,

        // Undead: Blunt→×1.3, Slash→×0.8
        [(AttackType.Blunt, MonsterRace.Undead)] = 1.3f,
        [(AttackType.Slash, MonsterRace.Undead)] = 0.8f,
    };

    /// <summary>WeaponType→AttackType変換テーブル</summary>
    private static readonly Dictionary<WeaponType, AttackType> WeaponAttackTypeMap = new()
    {
        [WeaponType.Unarmed] = AttackType.Unarmed,
        [WeaponType.Dagger] = AttackType.Pierce,
        [WeaponType.Sword] = AttackType.Slash,
        [WeaponType.Greatsword] = AttackType.Slash,
        [WeaponType.Axe] = AttackType.Slash,
        [WeaponType.Greataxe] = AttackType.Slash,
        [WeaponType.Spear] = AttackType.Pierce,
        [WeaponType.Hammer] = AttackType.Blunt,
        [WeaponType.Staff] = AttackType.Blunt,
        [WeaponType.Bow] = AttackType.Ranged,
        [WeaponType.Crossbow] = AttackType.Ranged,
        [WeaponType.Thrown] = AttackType.Ranged,
        [WeaponType.Whip] = AttackType.Slash,
        [WeaponType.Fist] = AttackType.Unarmed,
    };

    /// <summary>
    /// 種族×属性の相性を返す
    /// </summary>
    public static ElementalResistanceLevel GetResistanceLevel(MonsterRace race, Element element)
    {
        if (element == Element.None)
            return ElementalResistanceLevel.Normal;

        return AffinityTable.TryGetValue((race, element), out var level)
            ? level
            : ElementalResistanceLevel.Normal;
    }

    /// <summary>
    /// 耐性段階に対応するダメージ倍率を返す
    /// </summary>
    public static float GetDamageMultiplier(ElementalResistanceLevel level)
    {
        return level switch
        {
            ElementalResistanceLevel.Weakness => 1.5f,
            ElementalResistanceLevel.Normal => 1.0f,
            ElementalResistanceLevel.Resistant => 0.5f,
            ElementalResistanceLevel.Immune => 0.0f,
            ElementalResistanceLevel.Absorb => -1.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// 属性相性を考慮したダメージを計算する
    /// </summary>
    public static int CalculateElementalDamage(int baseDamage, Element attackElement, MonsterRace targetRace)
    {
        var resistanceLevel = GetResistanceLevel(targetRace, attackElement);
        var multiplier = GetDamageMultiplier(resistanceLevel);
        int damage = (int)(baseDamage * multiplier);
        // FK-1: Absorb（-1.0f）で負ダメージ＝回復にならないよう最低0にクランプ
        return Math.Max(0, damage);
    }

    /// <summary>
    /// WeaponTypeからAttackTypeへの変換
    /// </summary>
    public static AttackType GetWeaponTypeAttackType(WeaponType weaponType)
    {
        return WeaponAttackTypeMap.TryGetValue(weaponType, out var attackType)
            ? attackType
            : AttackType.Unarmed;
    }

    /// <summary>
    /// 物理攻撃種別×種族の相性倍率を返す
    /// </summary>
    public static float GetPhysicalDamageMultiplier(AttackType attackType, MonsterRace race)
    {
        return PhysicalAffinityTable.TryGetValue((attackType, race), out var multiplier)
            ? multiplier
            : 1.0f;
    }
}
