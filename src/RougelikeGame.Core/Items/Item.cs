using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Items;

/// <summary>
/// アイテムの種類
/// </summary>
public enum ItemType
{
    /// <summary>装備品（武器、防具、アクセサリ）</summary>
    Equipment,
    /// <summary>消費アイテム（ポーション、巻物）</summary>
    Consumable,
    /// <summary>食料</summary>
    Food,
    /// <summary>素材</summary>
    Material,
    /// <summary>鍵・特殊アイテム</summary>
    Key,
    /// <summary>クエストアイテム</summary>
    Quest,
    /// <summary>その他</summary>
    Miscellaneous
}

/// <summary>
/// アイテムのレアリティ
/// </summary>
public enum ItemRarity
{
    /// <summary>一般（白）</summary>
    Common = 0,
    /// <summary>非凡（緑）</summary>
    Uncommon = 1,
    /// <summary>希少（青）</summary>
    Rare = 2,
    /// <summary>壮大（紫）</summary>
    Epic = 3,
    /// <summary>伝説（橙）</summary>
    Legendary = 4,
    /// <summary>唯一（赤）- 固有アイテム</summary>
    Unique = 5
}

/// <summary>
/// アイテムインターフェース
/// </summary>
public interface IItem
{
    /// <summary>アイテム固有ID</summary>
    string ItemId { get; }

    /// <summary>表示名</summary>
    string Name { get; }

    /// <summary>説明文</summary>
    string Description { get; }

    /// <summary>アイテム種類</summary>
    ItemType Type { get; }

    /// <summary>レアリティ</summary>
    ItemRarity Rarity { get; }

    /// <summary>基本価格</summary>
    int BasePrice { get; }

    /// <summary>重量</summary>
    float Weight { get; }

    /// <summary>表示アイコン（ASCII文字）</summary>
    char DisplayChar { get; }
}

/// <summary>
/// 装備可能なアイテム
/// </summary>
public interface IEquippable : IItem
{
    /// <summary>装備スロット</summary>
    EquipmentSlot Slot { get; }

    /// <summary>装備時のステータス修正</summary>
    StatModifier StatModifier { get; }

    /// <summary>必要レベル</summary>
    int RequiredLevel { get; }

    /// <summary>装備可能か判定</summary>
    bool CanEquip(Entities.Player player);

    /// <summary>装備時の効果</summary>
    void OnEquip(Entities.Player player);

    /// <summary>装備解除時の効果</summary>
    void OnUnequip(Entities.Player player);
}

/// <summary>
/// 消費可能なアイテム
/// </summary>
public interface IConsumable : IItem
{
    /// <summary>使用可能か判定</summary>
    bool CanUse(Entities.Character user);

    /// <summary>アイテムを使用</summary>
    UseResult Use(Entities.Character user, IRandomProvider? random = null);

    /// <summary>消費後に消滅するか</summary>
    bool ConsumeOnUse { get; }
}

/// <summary>
/// スタック可能なアイテム
/// </summary>
public interface IStackable : IItem
{
    /// <summary>現在のスタック数</summary>
    int StackCount { get; set; }

    /// <summary>最大スタック数</summary>
    int MaxStack { get; }

    /// <summary>スタック可能か</summary>
    bool CanStackWith(IStackable other);
}

/// <summary>
/// アイテム使用結果
/// </summary>
public record UseResult(bool Success, string Message, ItemEffect? Effect = null)
{
    public static UseResult Ok(string message, ItemEffect? effect = null) 
        => new(true, message, effect);
    public static UseResult Fail(string message) 
        => new(false, message, null);
}

/// <summary>
/// アイテムの効果
/// </summary>
public record ItemEffect(
    ItemEffectType Type,
    int Value,
    Element Element = Element.None,
    int Duration = 0,
    StatusEffectType? StatusEffect = null
);

/// <summary>
/// アイテム効果の種類
/// </summary>
public enum ItemEffectType
{
    /// <summary>HP回復</summary>
    HealHp,
    /// <summary>MP回復</summary>
    RestoreMp,
    /// <summary>SP回復</summary>
    RestoreSp,
    /// <summary>満腹度回復</summary>
    RestoreHunger,
    /// <summary>正気度回復</summary>
    RestoreSanity,
    /// <summary>ダメージ</summary>
    Damage,
    /// <summary>状態異常付与</summary>
    ApplyStatus,
    /// <summary>状態異常解除</summary>
    RemoveStatus,
    /// <summary>ステータスバフ</summary>
    StatBuff,
    /// <summary>テレポート</summary>
    Teleport,
    /// <summary>マップ表示</summary>
    RevealMap,
    /// <summary>識別</summary>
    Identify,
    /// <summary>ルーン語習得</summary>
    LearnRuneWord,
    /// <summary>聖域展開</summary>
    Sanctuary,
    /// <summary>ダンジョン入口への帰還</summary>
    ReturnToEntrance
}

/// <summary>
/// 装備スロット
/// </summary>
public enum EquipmentSlot
{
    /// <summary>なし</summary>
    None = 0,
    /// <summary>メイン武器</summary>
    MainHand = 1,
    /// <summary>サブ武器/盾</summary>
    OffHand = 2,
    /// <summary>頭部</summary>
    Head = 3,
    /// <summary>胴体</summary>
    Body = 4,
    /// <summary>手</summary>
    Hands = 5,
    /// <summary>足</summary>
    Feet = 6,
    /// <summary>首飾り</summary>
    Neck = 7,
    /// <summary>指輪1</summary>
    Ring1 = 8,
    /// <summary>指輪2</summary>
    Ring2 = 9,
    /// <summary>背中（マント等）</summary>
    Back = 10,
    /// <summary>腰（ベルト等）</summary>
    Waist = 11
}

/// <summary>
/// アイテム基底クラス
/// </summary>
public abstract class Item : IItem
{
    public string ItemId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ItemType Type { get; init; }
    public ItemRarity Rarity { get; init; } = ItemRarity.Common;
    public int BasePrice { get; init; }
    public float Weight { get; init; }
    public virtual char DisplayChar => GetDefaultDisplayChar();

    /// <summary>未識別状態の名前</summary>
    public string UnidentifiedName { get; init; } = "不明なアイテム";

    /// <summary>識別済みか</summary>
    public bool IsIdentified { get; set; } = true;

    /// <summary>呪われているか</summary>
    public bool IsCursed { get; set; }

    /// <summary>祝福されているか</summary>
    public bool IsBlessed { get; set; }

    /// <summary>強化値（+1, +2など）</summary>
    public int EnhancementLevel { get; set; }

    /// <summary>耐久度（-1 = 無限）</summary>
    public int Durability { get; set; } = -1;

    /// <summary>最大耐久度</summary>
    public int MaxDurability { get; init; } = -1;

    /// <summary>アイテム等級（品質）</summary>
    public ItemGrade Grade { get; set; } = ItemGrade.Standard;

    /// <summary>実際の価格を計算</summary>
    public virtual int CalculatePrice()
    {
        float multiplier = Rarity switch
        {
            ItemRarity.Common => 1.0f,
            ItemRarity.Uncommon => 2.0f,
            ItemRarity.Rare => 5.0f,
            ItemRarity.Epic => 15.0f,
            ItemRarity.Legendary => 50.0f,
            ItemRarity.Unique => 100.0f,
            _ => 1.0f
        };

        if (IsBlessed) multiplier *= 1.5f;
        if (IsCursed) multiplier *= 0.5f;

        multiplier *= 1.0f + (EnhancementLevel * 0.2f);

        // 等級係数を適用
        multiplier *= ItemGradeSystem.GetPriceMultiplier(Grade);

        return (int)(BasePrice * multiplier);
    }

    /// <summary>表示名を取得（識別・強化値・等級を考慮）</summary>
    public virtual string GetDisplayName()
    {
        if (!IsIdentified)
            return UnidentifiedName;

        var prefix = "";
        if (IsBlessed) prefix = "祝福された";
        else if (IsCursed) prefix = "呪われた";

        var gradePrefix = ItemGradeSystem.GetGradeDisplayPrefix(Grade);

        var enhancement = EnhancementLevel != 0 
            ? $"+{EnhancementLevel} " 
            : "";

        return $"{prefix}{gradePrefix}{enhancement}{Name}";
    }

    /// <summary>アイテムタイプに応じた表示文字</summary>
    protected virtual char GetDefaultDisplayChar() => Type switch
    {
        ItemType.Equipment => ')',
        ItemType.Consumable => '!',
        ItemType.Food => '%',
        ItemType.Material => '*',
        ItemType.Key => '-',
        ItemType.Quest => '?',
        _ => '?'
    };

    public override string ToString() => GetDisplayName();
}

/// <summary>
/// スタック可能なアイテム基底クラス
/// </summary>
public abstract class StackableItem : Item, IStackable
{
    public int StackCount { get; set; } = 1;
    public virtual int MaxStack => 99;

    public bool CanStackWith(IStackable other)
    {
        return other is StackableItem otherItem 
            && otherItem.ItemId == ItemId 
            && otherItem.IsIdentified == IsIdentified
            && otherItem.IsCursed == IsCursed
            && otherItem.IsBlessed == IsBlessed
            && StackCount + other.StackCount <= MaxStack;
    }

    public override int CalculatePrice()
    {
        return base.CalculatePrice() * StackCount;
    }

    public override string GetDisplayName()
    {
        var baseName = base.GetDisplayName();
        return StackCount > 1 ? $"{baseName} x{StackCount}" : baseName;
    }
}
