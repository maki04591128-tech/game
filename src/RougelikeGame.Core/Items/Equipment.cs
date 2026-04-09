namespace RougelikeGame.Core.Items;

/// <summary>
/// 武器の種類
/// </summary>
public enum WeaponType
{
    /// <summary>素手</summary>
    Unarmed,
    /// <summary>短剣</summary>
    Dagger,
    /// <summary>剣</summary>
    Sword,
    /// <summary>大剣</summary>
    Greatsword,
    /// <summary>斧</summary>
    Axe,
    /// <summary>大斧</summary>
    Greataxe,
    /// <summary>槍</summary>
    Spear,
    /// <summary>ハンマー</summary>
    Hammer,
    /// <summary>杖</summary>
    Staff,
    /// <summary>弓</summary>
    Bow,
    /// <summary>クロスボウ</summary>
    Crossbow,
    /// <summary>投擲武器</summary>
    Thrown,
    /// <summary>鞭</summary>
    Whip,
    /// <summary>格闘武器</summary>
    Fist
}

/// <summary>
/// 防具の種類
/// </summary>
public enum ArmorType
{
    /// <summary>布装備</summary>
    Cloth,
    /// <summary>革装備</summary>
    Leather,
    /// <summary>鎖帷子</summary>
    Chainmail,
    /// <summary>板金鎧</summary>
    Plate,
    /// <summary>ローブ</summary>
    Robe,
    /// <summary>盾</summary>
    Shield
}

/// <summary>
/// 装備品基底クラス
/// </summary>
public abstract class EquipmentItem : Item, IEquippable
{
    public EquipmentSlot Slot { get; init; }
    public StatModifier StatModifier { get; init; } = new();
    public int RequiredLevel { get; init; } = 1;

    /// <summary>装備カテゴリ（職業適性判定用）</summary>
    public virtual EquipmentCategory Category => EquipmentCategory.Sword;

    /// <summary>必要ステータス</summary>
    public Stats? RequiredStats { get; init; }

    /// <summary>属性耐性</summary>
    public Dictionary<Element, float> ElementalResistances { get; init; } = new();

    /// <summary>特殊効果</summary>
    public List<ItemEffect> Effects { get; init; } = new();

    public EquipmentItem()
    {
        Type = ItemType.Equipment;
    }

    public virtual bool CanEquip(Entities.Player player)
    {
        if (player.Level < RequiredLevel)
            return false;

        // BX-7: 耐久度0以下の装備は装備不可（耐久度システム有効時のみ）
        if (MaxDurability > 0 && Durability <= 0)
            return false;

        // Y-4: スライム種族の装備制限（指輪・首飾り以外装備不可）
        if (Systems.RacialTraitSystem.HasEquipmentRestriction(player.Race)
            && Slot != EquipmentSlot.Ring1
            && Slot != EquipmentSlot.Ring2
            && Slot != EquipmentSlot.Neck)
        {
            return false;
        }

        if (RequiredStats.HasValue)
        {
            var stats = player.EffectiveStats;
            var req = RequiredStats.Value;

            if (stats.Strength < req.Strength ||
                stats.Vitality < req.Vitality ||
                stats.Agility < req.Agility ||
                stats.Dexterity < req.Dexterity ||
                stats.Intelligence < req.Intelligence ||
                stats.Mind < req.Mind)
            {
                return false;
            }
        }

        return true;
    }

    public virtual void OnEquip(Entities.Player player)
    {
        // 装備時の特殊効果を適用
        foreach (var effect in Effects.Where(e => e.Type == ItemEffectType.StatBuff))
        {
            // バフ効果を適用（ステータス修正は別途計算される）
        }
    }

    public virtual void OnUnequip(Entities.Player player)
    {
        // 呪われたアイテムは外せない（Curseステータスの有無で判定）
        if (IsCursed && player.HasStatusEffect(StatusEffectType.Curse))
        {
            // 呪いによる制限
        }
    }

    /// <summary>強化値を含めたステータス修正を取得</summary>
    public virtual StatModifier GetEffectiveStatModifier()
    {
        if (EnhancementLevel == 0)
            return StatModifier;

        // 強化値に応じてボーナスを追加
        return StatModifier with
        {
            Strength = StatModifier.Strength + (Slot == EquipmentSlot.MainHand ? EnhancementLevel : 0),
            Vitality = StatModifier.Vitality + (IsArmorSlot() ? EnhancementLevel : 0)
        };
    }

    private bool IsArmorSlot() => Slot is EquipmentSlot.Head or EquipmentSlot.Body 
        or EquipmentSlot.Hands or EquipmentSlot.Feet or EquipmentSlot.OffHand;

    protected override char GetDefaultDisplayChar() => Slot switch
    {
        EquipmentSlot.MainHand => ')',
        EquipmentSlot.OffHand => '[',
        EquipmentSlot.Head => '^',
        EquipmentSlot.Body => ']',
        EquipmentSlot.Hands => '(',
        EquipmentSlot.Feet => '_',
        EquipmentSlot.Neck or EquipmentSlot.Ring1 or EquipmentSlot.Ring2 => '=',
        EquipmentSlot.Back => '`',
        EquipmentSlot.Waist => '~',
        _ => ')'
    };
}

/// <summary>
/// 武器クラス
/// </summary>
public class Weapon : EquipmentItem
{
    public WeaponType WeaponType { get; init; }

    /// <summary>WeaponTypeからEquipmentCategoryを導出</summary>
    public override EquipmentCategory Category => WeaponType switch
    {
        WeaponType.Sword or WeaponType.Greatsword => EquipmentCategory.Sword,
        WeaponType.Axe or WeaponType.Greataxe => EquipmentCategory.Axe,
        WeaponType.Hammer => EquipmentCategory.Mace,
        WeaponType.Dagger => EquipmentCategory.Dagger,
        WeaponType.Bow or WeaponType.Crossbow or WeaponType.Thrown => EquipmentCategory.Bow,
        WeaponType.Staff => EquipmentCategory.Staff,
        WeaponType.Unarmed or WeaponType.Fist => EquipmentCategory.Fist,
        WeaponType.Spear => EquipmentCategory.Spear,
        WeaponType.Whip => EquipmentCategory.Whip,
        _ => EquipmentCategory.Sword
    };

    /// <summary>基本攻撃力</summary>
    public int BaseDamage { get; init; }

    /// <summary>ダメージ範囲（最小-最大）</summary>
    public (int Min, int Max) DamageRange { get; init; }

    /// <summary>攻撃速度（1.0が基準）</summary>
    public float AttackSpeed { get; init; } = 1.0f;

    /// <summary>射程（1 = 近接）</summary>
    public int Range { get; init; } = 1;

    /// <summary>属性</summary>
    public Element Element { get; init; } = Element.None;

    /// <summary>両手武器か</summary>
    public bool IsTwoHanded { get; init; }

    /// <summary>攻撃タイプ</summary>
    public AttackType AttackType { get; init; } = AttackType.Slash;

    /// <summary>BS-9: 武器固有クリティカルボーナス（0.0～0.15程度）</summary>
    public double CriticalBonus { get; init; }

    public Weapon()
    {
        Slot = EquipmentSlot.MainHand;
    }

    /// <summary>実際のダメージを計算</summary>
    public int CalculateDamage(Random random)
    {
        int min = DamageRange.Min;
        int max = DamageRange.Max;
        if (min > max) (min, max) = (max, min);
        int baseDmg = random.Next(min, max + 1);
        int enhanceBonus = EnhancementLevel * 2;
        int totalDmg = baseDmg + enhanceBonus;
        // 両手武器はダメージ25%ボーナス
        if (IsTwoHanded) totalDmg = (int)(totalDmg * 1.25f);
        return totalDmg;
    }

    /// <summary>攻撃に必要なターン数</summary>
    public int GetAttackTurnCost()
    {
        // 基本1ターン、攻撃速度で調整
        float speed = AttackSpeed > 0f ? AttackSpeed : 1.0f;
        return Math.Max(1, (int)(1.0f / speed));
    }

    protected override char GetDefaultDisplayChar() => WeaponType switch
    {
        WeaponType.Unarmed => ' ',
        WeaponType.Dagger => '†',
        WeaponType.Sword => '/',
        WeaponType.Greatsword => '|',
        WeaponType.Axe => 'P',
        WeaponType.Greataxe => 'P',
        WeaponType.Spear => '/',
        WeaponType.Hammer => 'T',
        WeaponType.Staff => '\\',
        WeaponType.Bow => '}',
        WeaponType.Crossbow => '{',
        WeaponType.Thrown => '*',
        WeaponType.Whip => '~',
        WeaponType.Fist => ')',
        _ => ')'
    };
}

/// <summary>
/// 防具クラス
/// </summary>
public class Armor : EquipmentItem
{
    public ArmorType ArmorType { get; init; }

    /// <summary>ArmorTypeからEquipmentCategoryを導出</summary>
    public override EquipmentCategory Category => ArmorType switch
    {
        ArmorType.Plate => EquipmentCategory.HeavyArmor,
        ArmorType.Chainmail => EquipmentCategory.MediumArmor,
        ArmorType.Leather => EquipmentCategory.LightArmor,
        ArmorType.Robe or ArmorType.Cloth => EquipmentCategory.Robe,
        ArmorType.Shield => EquipmentCategory.Shield,
        _ => EquipmentCategory.LightArmor
    };

    /// <summary>基本防御力</summary>
    public int BaseDefense { get; init; }

    /// <summary>魔法防御力</summary>
    public int MagicDefense { get; init; }

    /// <summary>回避修正</summary>
    public float EvasionModifier { get; init; }

    /// <summary>移動速度修正</summary>
    public float SpeedModifier { get; init; } = 1.0f;

    /// <summary>実際の防御力を計算</summary>
    public int CalculateDefense()
    {
        return BaseDefense + EnhancementLevel;
    }
}

/// <summary>
/// 盾クラス
/// </summary>
public class Shield : Armor
{
    /// <summary>ブロック率</summary>
    public float BlockChance { get; init; }

    /// <summary>ブロック時のダメージ軽減率</summary>
    public float BlockReduction { get; init; } = 0.5f;

    public Shield()
    {
        Slot = EquipmentSlot.OffHand;
        ArmorType = ArmorType.Shield;
    }

    protected override char GetDefaultDisplayChar() => '[';
}

/// <summary>
/// アクセサリクラス
/// </summary>
public class Accessory : EquipmentItem
{
    /// <summary>特殊能力（パッシブ）</summary>
    public string? PassiveAbility { get; init; }

    /// <summary>発動スキル</summary>
    public string? ActivatedSkill { get; init; }

    /// <summary>スキルのクールダウン</summary>
    public int SkillCooldown { get; init; }

    protected override char GetDefaultDisplayChar() => '=';

    /// <summary>アクセサリは専用カテゴリ（職業適性判定対象外）</summary>
    public override EquipmentCategory Category => EquipmentCategory.Accessory;
}

/// <summary>
/// 装備セット管理
/// </summary>
public class Equipment
{
    private readonly Dictionary<EquipmentSlot, EquipmentItem?> _slots = new();

    public Equipment()
    {
        // 全スロットを初期化
        foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
        {
            if (slot != EquipmentSlot.None)
                _slots[slot] = null;
        }
    }

    /// <summary>指定スロットの装備を取得</summary>
    public EquipmentItem? this[EquipmentSlot slot]
    {
        get => _slots.GetValueOrDefault(slot);
        set => _slots[slot] = value;
    }

    /// <summary>メイン武器</summary>
    public Weapon? MainHand => this[EquipmentSlot.MainHand] as Weapon;

    /// <summary>サブ武器/盾</summary>
    public EquipmentItem? OffHand => this[EquipmentSlot.OffHand];

    /// <summary>装備を着ける</summary>
    public EquipmentItem? Equip(EquipmentItem item, Entities.Player player)
    {
        if (!item.CanEquip(player))
            return null;

        // BX-2: 両手武器装備中は盾/オフハンド装備不可
        if (item.Slot == EquipmentSlot.OffHand && MainHand is Weapon mainWeapon && mainWeapon.IsTwoHanded)
            return null;

        var previousItem = _slots[item.Slot];

        // 両手武器の場合、オフハンドも外す
        if (item is Weapon weapon && weapon.IsTwoHanded)
        {
            var offHandItem = _slots[EquipmentSlot.OffHand];
            if (offHandItem != null)
            {
                offHandItem.OnUnequip(player);
                // オフハンドアイテムをインベントリに戻す
                ((Entities.Inventory)player.Inventory).Add(offHandItem);
            }
            _slots[EquipmentSlot.OffHand] = null;
        }

        // 既存装備を外す
        previousItem?.OnUnequip(player);

        // 新しい装備を着ける
        _slots[item.Slot] = item;
        item.OnEquip(player);

        return previousItem;
    }

    /// <summary>装備を外す</summary>
    public EquipmentItem? Unequip(EquipmentSlot slot, Entities.Player player)
    {
        var item = _slots[slot];
        if (item == null)
            return null;

        if (item.IsCursed)
            return null; // 呪われたアイテムは外せない

        item.OnUnequip(player);
        _slots[slot] = null;

        return item;
    }

    /// <summary>全装備からのステータス修正を取得</summary>
    public IEnumerable<StatModifier> GetStatModifiers()
    {
        return _slots.Values
            .Where(item => item != null)
            .Select(item => item!.GetEffectiveStatModifier());
    }

    /// <summary>全装備からの属性耐性を取得</summary>
    public float GetElementalResistance(Element element)
    {
        return _slots.Values
            .Where(item => item != null)
            .Sum(item => item!.ElementalResistances.GetValueOrDefault(element, 0f));
    }

    /// <summary>全装備の物理防御力合計</summary>
    public int GetTotalPhysicalDefense()
    {
        return _slots.Values
            .OfType<Armor>()
            .Sum(armor => armor.CalculateDefense());
    }

    /// <summary>全装備の魔法防御力合計</summary>
    public int GetTotalMagicDefense()
    {
        return _slots.Values
            .OfType<Armor>()
            .Sum(armor => armor.MagicDefense + armor.EnhancementLevel);
    }

    /// <summary>全装備の重量合計</summary>
    public float GetTotalWeight()
    {
        return _slots.Values
            .Where(item => item != null)
            .Sum(item => item!.Weight);
    }

    /// <summary>装備一覧を取得</summary>
    public IReadOnlyDictionary<EquipmentSlot, EquipmentItem?> GetAll()
    {
        return _slots;
    }
}
