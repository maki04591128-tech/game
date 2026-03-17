namespace RougelikeGame.Core.Systems;

/// <summary>
/// モンスター種族ごとの特性データ
/// </summary>
public record MonsterRaceTraits(
    MonsterRace Race,
    IReadOnlyList<Element> Weaknesses,
    IReadOnlyList<Element> Resistances,
    IReadOnlyList<AttackType> PhysicalResistances,
    IReadOnlyList<StatusEffectType> StatusImmunities,
    float PhysicalDamageMultiplier = 1.0f
);

/// <summary>
/// モンスター種族システム - 種族ごとの弱点・耐性・状態異常耐性を管理
/// </summary>
public static class MonsterRaceSystem
{
    private static readonly Dictionary<MonsterRace, MonsterRaceTraits> TraitsMap = new()
    {
        [MonsterRace.Beast] = new MonsterRaceTraits(
            Race: MonsterRace.Beast,
            Weaknesses: new[] { Element.Fire },
            Resistances: Array.Empty<Element>(),
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: Array.Empty<StatusEffectType>()
        ),
        [MonsterRace.Humanoid] = new MonsterRaceTraits(
            Race: MonsterRace.Humanoid,
            Weaknesses: Array.Empty<Element>(),
            Resistances: Array.Empty<Element>(),
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: Array.Empty<StatusEffectType>()
        ),
        [MonsterRace.Amorphous] = new MonsterRaceTraits(
            Race: MonsterRace.Amorphous,
            Weaknesses: Array.Empty<Element>(),
            Resistances: Array.Empty<Element>(),
            PhysicalResistances: new[] { AttackType.Slash, AttackType.Pierce },
            StatusImmunities: Array.Empty<StatusEffectType>(),
            PhysicalDamageMultiplier: 0.5f
        ),
        [MonsterRace.Undead] = new MonsterRaceTraits(
            Race: MonsterRace.Undead,
            Weaknesses: new[] { Element.Light, Element.Holy },
            Resistances: new[] { Element.Poison, Element.Dark },
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: new[] { StatusEffectType.Poison, StatusEffectType.Sleep }
        ),
        [MonsterRace.Demon] = new MonsterRaceTraits(
            Race: MonsterRace.Demon,
            Weaknesses: new[] { Element.Holy },
            Resistances: new[] { Element.Dark, Element.Curse },
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: Array.Empty<StatusEffectType>()
        ),
        [MonsterRace.Dragon] = new MonsterRaceTraits(
            Race: MonsterRace.Dragon,
            Weaknesses: Array.Empty<Element>(),
            Resistances: new[] { Element.Fire, Element.Ice, Element.Lightning },
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: Array.Empty<StatusEffectType>()
        ),
        [MonsterRace.Plant] = new MonsterRaceTraits(
            Race: MonsterRace.Plant,
            Weaknesses: new[] { Element.Fire, Element.Ice },
            Resistances: new[] { Element.Earth, Element.Water },
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: Array.Empty<StatusEffectType>()
        ),
        [MonsterRace.Insect] = new MonsterRaceTraits(
            Race: MonsterRace.Insect,
            Weaknesses: new[] { Element.Fire },
            Resistances: Array.Empty<Element>(),
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: new[] { StatusEffectType.Confusion, StatusEffectType.Fear, StatusEffectType.Charm }
        ),
        [MonsterRace.Spirit] = new MonsterRaceTraits(
            Race: MonsterRace.Spirit,
            Weaknesses: Array.Empty<Element>(),
            Resistances: Array.Empty<Element>(),
            PhysicalResistances: new[] { AttackType.Slash, AttackType.Pierce, AttackType.Blunt, AttackType.Unarmed },
            StatusImmunities: Array.Empty<StatusEffectType>(),
            PhysicalDamageMultiplier: 0.5f
        ),
        [MonsterRace.Construct] = new MonsterRaceTraits(
            Race: MonsterRace.Construct,
            Weaknesses: new[] { Element.Lightning },
            Resistances: new[] { Element.Poison },
            PhysicalResistances: Array.Empty<AttackType>(),
            StatusImmunities: new[]
            {
                StatusEffectType.Poison, StatusEffectType.Sleep, StatusEffectType.Confusion,
                StatusEffectType.Fear, StatusEffectType.Charm, StatusEffectType.Blind,
                StatusEffectType.Silence, StatusEffectType.Madness, StatusEffectType.Paralysis,
                StatusEffectType.Burn, StatusEffectType.Freeze, StatusEffectType.Petrification,
                StatusEffectType.Curse, StatusEffectType.InstantDeath
            }
        )
    };

    /// <summary>
    /// 指定した種族の特性データを取得
    /// </summary>
    public static MonsterRaceTraits GetTraits(MonsterRace race)
    {
        return TraitsMap.TryGetValue(race, out var traits)
            ? traits
            : TraitsMap[MonsterRace.Humanoid];
    }

    /// <summary>
    /// 属性攻撃の倍率を取得（弱点: 1.5, 耐性: 0.5, 通常: 1.0）
    /// </summary>
    public static float GetElementalMultiplier(MonsterRace race, Element element)
    {
        var traits = GetTraits(race);

        if (traits.Weaknesses.Contains(element))
            return 1.5f;

        if (traits.Resistances.Contains(element))
            return 0.5f;

        return 1.0f;
    }

    /// <summary>
    /// 状態異常が無効かどうかを判定
    /// </summary>
    public static bool IsStatusEffectImmune(MonsterRace race, StatusEffectType statusEffect)
    {
        var traits = GetTraits(race);
        return traits.StatusImmunities.Contains(statusEffect);
    }

    /// <summary>
    /// 物理攻撃タイプに対するダメージ倍率を取得
    /// </summary>
    public static float GetPhysicalResistance(MonsterRace race, AttackType attackType)
    {
        var traits = GetTraits(race);

        if (traits.PhysicalResistances.Contains(attackType))
            return traits.PhysicalDamageMultiplier;

        return 1.0f;
    }
}
