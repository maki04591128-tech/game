using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Interfaces;

namespace RougelikeGame.Core.Items;

/// <summary>
/// 消費アイテム基底クラス
/// </summary>
public abstract class ConsumableItem : StackableItem, IConsumable
{
    public bool ConsumeOnUse { get; init; } = true;

    /// <summary>使用時の効果リスト</summary>
    public List<ItemEffect> Effects { get; init; } = new();

    public ConsumableItem()
    {
        Type = ItemType.Consumable;
    }

    public virtual bool CanUse(Character user)
    {
        return user.IsAlive;
    }

    public abstract UseResult Use(Character user, IRandomProvider? random = null);

    protected override char GetDefaultDisplayChar() => '!';
}

/// <summary>
/// ポーション（回復アイテム）
/// </summary>
public class Potion : ConsumableItem
{
    public PotionType PotionType { get; init; }

    /// <summary>回復量または効果値</summary>
    public int EffectValue { get; init; }

    /// <summary>効果の割合（%回復の場合）</summary>
    public float EffectPercentage { get; init; }

    /// <summary>効果時間（ターン）</summary>
    public int Duration { get; init; }

    public override UseResult Use(Character user, IRandomProvider? random = null)
    {
        switch (PotionType)
        {
            case PotionType.HealingMinor:
            case PotionType.HealingMajor:
            case PotionType.HealingSuper:
                int healAmount = EffectValue > 0 
                    ? EffectValue 
                    : (int)(user.MaxHp * EffectPercentage);
                user.Heal(healAmount);
                return UseResult.Ok($"HPが{healAmount}回復した！", 
                    new ItemEffect(ItemEffectType.HealHp, healAmount));

            case PotionType.ManaMinor:
            case PotionType.ManaMajor:
            case PotionType.ManaSuper:
                int mpAmount = EffectValue > 0 
                    ? EffectValue 
                    : (int)(user.MaxMp * EffectPercentage);
                user.RestoreMp(mpAmount);
                return UseResult.Ok($"MPが{mpAmount}回復した！",
                    new ItemEffect(ItemEffectType.RestoreMp, mpAmount));

            case PotionType.StaminaMinor:
            case PotionType.StaminaMajor:
                int spAmount = EffectValue > 0 
                    ? EffectValue 
                    : (int)(user.MaxSp * EffectPercentage);
                user.RestoreSp(spAmount);
                return UseResult.Ok($"SPが{spAmount}回復した！",
                    new ItemEffect(ItemEffectType.RestoreSp, spAmount));

            case PotionType.Antidote:
                user.RemoveStatusEffect(StatusEffectType.Poison);
                return UseResult.Ok("毒が解消された！",
                    new ItemEffect(ItemEffectType.RemoveStatus, 0, StatusEffect: StatusEffectType.Poison));

            case PotionType.CureAll:
                // 全てのデバフを解除
                user.RemoveStatusEffect(StatusEffectType.Poison);
                user.RemoveStatusEffect(StatusEffectType.Bleeding);
                user.RemoveStatusEffect(StatusEffectType.Burn);
                user.RemoveStatusEffect(StatusEffectType.Freeze);
                user.RemoveStatusEffect(StatusEffectType.Paralysis);
                user.RemoveStatusEffect(StatusEffectType.Stun);
                user.RemoveStatusEffect(StatusEffectType.Confusion);
                user.RemoveStatusEffect(StatusEffectType.Blind);
                user.RemoveStatusEffect(StatusEffectType.Silence);
                user.RemoveStatusEffect(StatusEffectType.Charm);
                user.RemoveStatusEffect(StatusEffectType.Madness);
                user.RemoveStatusEffect(StatusEffectType.Petrification);
                user.RemoveStatusEffect(StatusEffectType.Fear);
                user.RemoveStatusEffect(StatusEffectType.Sleep);
                // CB-3: 不足していたデバフの除去
                user.RemoveStatusEffect(StatusEffectType.Weakness);
                user.RemoveStatusEffect(StatusEffectType.Curse);
                user.RemoveStatusEffect(StatusEffectType.Slow);
                user.RemoveStatusEffect(StatusEffectType.Vulnerability);
                user.RemoveStatusEffect(StatusEffectType.Apostasy);
                user.RemoveStatusEffect(StatusEffectType.InstantDeath);
                return UseResult.Ok("全ての状態異常が解消された！");

            case PotionType.StrengthBoost:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength, Duration));
                return UseResult.Ok("攻撃力が上がった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration, StatusEffect: StatusEffectType.Strength));

            case PotionType.AgilityBoost:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Haste, Duration));
                return UseResult.Ok("素早さが上がった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration, StatusEffect: StatusEffectType.Haste));

            case PotionType.Invisibility:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Invisibility, Duration));
                return UseResult.Ok("透明になった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration, StatusEffect: StatusEffectType.Invisibility));

            case PotionType.FireResistance:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.FireResistance, Duration));
                return UseResult.Ok("火耐性が上がった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration, StatusEffect: StatusEffectType.FireResistance));

            case PotionType.ColdResistance:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.ColdResistance, Duration));
                return UseResult.Ok("冷気耐性が上がった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration, StatusEffect: StatusEffectType.ColdResistance));

            case PotionType.IntelligenceBoost:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Blessing, Duration > 0 ? Duration : 20));
                return UseResult.Ok("能力が強化された！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration > 0 ? Duration : 20, StatusEffect: StatusEffectType.Blessing));

            case PotionType.Poison:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, Duration > 0 ? Duration : 10));
                return UseResult.Ok("毒を受けた！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration > 0 ? Duration : 10, StatusEffect: StatusEffectType.Poison));

            case PotionType.Confusion:
                user.ApplyStatusEffect(new StatusEffect(StatusEffectType.Confusion, Duration > 0 ? Duration : 5));
                return UseResult.Ok("混乱した！",
                    new ItemEffect(ItemEffectType.ApplyStatus, Duration > 0 ? Duration : 5, StatusEffect: StatusEffectType.Confusion));

            default:
                return UseResult.Fail("効果がなかった");
        }
    }

    protected override char GetDefaultDisplayChar() => '!';
}

/// <summary>
/// ポーションの種類
/// </summary>
public enum PotionType
{
    /// <summary>小回復薬</summary>
    HealingMinor,
    /// <summary>回復薬</summary>
    HealingMajor,
    /// <summary>超回復薬</summary>
    HealingSuper,
    /// <summary>小マナポーション</summary>
    ManaMinor,
    /// <summary>マナポーション</summary>
    ManaMajor,
    /// <summary>超マナポーション</summary>
    ManaSuper,
    /// <summary>小スタミナポーション</summary>
    StaminaMinor,
    /// <summary>スタミナポーション</summary>
    StaminaMajor,
    /// <summary>解毒剤</summary>
    Antidote,
    /// <summary>万能薬</summary>
    CureAll,
    /// <summary>筋力増強</summary>
    StrengthBoost,
    /// <summary>敏捷増強</summary>
    AgilityBoost,
    /// <summary>知力増強</summary>
    IntelligenceBoost,
    /// <summary>透明化</summary>
    Invisibility,
    /// <summary>火耐性</summary>
    FireResistance,
    /// <summary>冷気耐性</summary>
    ColdResistance,
    /// <summary>毒</summary>
    Poison,
    /// <summary>混乱</summary>
    Confusion
}

/// <summary>
/// 食料アイテム
/// </summary>
public class Food : ConsumableItem
{
    public FoodType FoodType { get; init; }

    /// <summary>栄養価（満腹度回復量）</summary>
    public int NutritionValue { get; init; }

    /// <summary>HP回復量（ある場合）</summary>
    public int HealValue { get; init; }

    /// <summary>渇き回復量（0=回復なし、1=Thirsty→Hydrated、2=Dehydrated→Hydrated）</summary>
    public int HydrationValue { get; init; }

    /// <summary>腐っているか</summary>
    public bool IsRotten { get; set; }

    /// <summary>調理済みか</summary>
    public bool IsCooked { get; init; }

    public Food()
    {
        Type = ItemType.Food;
    }

    public override bool CanUse(Character user)
    {
        return base.CanUse(user) && user is Player;
    }

    public override UseResult Use(Character user, IRandomProvider? random = null)
    {
        if (user is not Player player)
            return UseResult.Fail("プレイヤーのみ使用可能");

        int nutrition = NutritionValue;

        // 腐っている場合
        if (IsRotten)
        {
            nutrition = NutritionValue / 2;
            double chance = random?.NextDouble() ?? new Random().NextDouble();

            if (chance < 0.3)
            {
                player.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, 10));
                return UseResult.Ok($"腐った{Name}を食べて毒を受けた！ 満腹度+{nutrition}",
                    new ItemEffect(ItemEffectType.ApplyStatus, 0, StatusEffect: StatusEffectType.Poison));
            }
        }

        // 満腹度回復
        player.ModifyHunger(nutrition);

        // HP回復
        if (HealValue > 0)
        {
            player.Heal(HealValue);
            return UseResult.Ok($"{Name}を食べた。満腹度+{nutrition}、HP+{HealValue}",
                new ItemEffect(ItemEffectType.RestoreHunger, nutrition));
        }

        return UseResult.Ok($"{Name}を食べた。満腹度+{nutrition}",
            new ItemEffect(ItemEffectType.RestoreHunger, nutrition));
    }

    protected override char GetDefaultDisplayChar() => '%';
}

/// <summary>
/// 食料の種類
/// </summary>
public enum FoodType
{
    /// <summary>パン</summary>
    Bread,
    /// <summary>肉（生）</summary>
    RawMeat,
    /// <summary>肉（調理済み）</summary>
    CookedMeat,
    /// <summary>魚（生）</summary>
    RawFish,
    /// <summary>魚（調理済み）</summary>
    CookedFish,
    /// <summary>果物</summary>
    Fruit,
    /// <summary>野菜</summary>
    Vegetable,
    /// <summary>保存食</summary>
    Ration,
    /// <summary>非常食</summary>
    EmergencyRation,
    /// <summary>エルフパン</summary>
    Lembas,
    /// <summary>腐った食べ物</summary>
    Rotten,
    /// <summary>水</summary>
    Water,
    /// <summary>清水</summary>
    CleanWater
}

/// <summary>
/// 巻物（魔法効果）
/// </summary>
public class Scroll : ConsumableItem
{
    public ScrollType ScrollType { get; init; }

    /// <summary>効果の対象（自分/敵/範囲）</summary>
    public TargetType TargetType { get; init; }

    /// <summary>効果範囲（0 = 単体）</summary>
    public int EffectRadius { get; init; }

    /// <summary>効果値</summary>
    public int EffectValue { get; init; }

    public Scroll()
    {
        UnidentifiedName = "不明な巻物";
    }

    public override UseResult Use(Character user, IRandomProvider? random = null)
    {
        switch (ScrollType)
        {
            case ScrollType.Teleport:
                // テレポート処理（state必要）
                return UseResult.Ok("テレポートした！",
                    new ItemEffect(ItemEffectType.Teleport, 0));

            case ScrollType.Identify:
                // 識別処理
                return UseResult.Ok("アイテムを識別した！",
                    new ItemEffect(ItemEffectType.Identify, 0));

            case ScrollType.MagicMapping:
                // マップ表示処理
                return UseResult.Ok("周囲のマップが明らかになった！",
                    new ItemEffect(ItemEffectType.RevealMap, EffectValue));

            case ScrollType.Fireball:
                // 火球発射
                return UseResult.Ok($"炎の球が放たれた！",
                    new ItemEffect(ItemEffectType.Damage, EffectValue, Element.Fire));

            case ScrollType.Lightning:
                return UseResult.Ok("雷が落ちた！",
                    new ItemEffect(ItemEffectType.Damage, EffectValue, Element.Lightning));

            case ScrollType.RemoveCurse:
                // 呪い解除
                return UseResult.Ok("呪いが解けた！",
                    new ItemEffect(ItemEffectType.RemoveStatus, 0, StatusEffect: StatusEffectType.Curse));

            case ScrollType.Enchant:
                // 装備強化
                return UseResult.Ok("装備が強化された！",
                    new ItemEffect(ItemEffectType.StatBuff, 1));

            case ScrollType.AncientBook:
                return UseResult.Ok("古代の書を読み解いた…",
                    new ItemEffect(ItemEffectType.LearnRuneWord, EffectValue));

            case ScrollType.Freeze:
                return UseResult.Ok("冷気が周囲を包んだ！",
                    new ItemEffect(ItemEffectType.Damage, EffectValue, Element.Ice));

            case ScrollType.Sanctuary:
                return UseResult.Ok("聖域が展開された！",
                    new ItemEffect(ItemEffectType.Sanctuary, 0));

            case ScrollType.Return:
                return UseResult.Ok("ダンジョンの入口に戻った！",
                    new ItemEffect(ItemEffectType.ReturnToEntrance, 0));

            case ScrollType.Summon:
                return UseResult.Ok("味方モンスターを召喚した！",
                    new ItemEffect(ItemEffectType.Summon, EffectValue));

            case ScrollType.Confusion:  // B-4: 混乱の巻物
                return UseResult.Ok("混乱の霧が広がった！",
                    new ItemEffect(ItemEffectType.ApplyStatus, EffectValue, StatusEffect: StatusEffectType.Confusion));

            default:
                return UseResult.Fail("何も起こらなかった");
        }
    }

    protected override char GetDefaultDisplayChar() => '?';
}

/// <summary>
/// 巻物の種類
/// </summary>
public enum ScrollType
{
    /// <summary>テレポート</summary>
    Teleport,
    /// <summary>識別</summary>
    Identify,
    /// <summary>マップ表示</summary>
    MagicMapping,
    /// <summary>聖域</summary>
    Sanctuary,
    /// <summary>火球</summary>
    Fireball,
    /// <summary>落雷</summary>
    Lightning,
    /// <summary>凍結</summary>
    Freeze,
    /// <summary>呪い解除</summary>
    RemoveCurse,
    /// <summary>装備強化</summary>
    Enchant,
    /// <summary>帰還</summary>
    Return,
    /// <summary>召喚</summary>
    Summon,
    /// <summary>古代の書（ルーン語習得）</summary>
    AncientBook,
    /// <summary>B-4: 混乱</summary>
    Confusion
}

/// <summary>
/// 効果の対象
/// </summary>
public enum TargetType
{
    /// <summary>自分</summary>
    Self,
    /// <summary>単体敵</summary>
    SingleEnemy,
    /// <summary>範囲</summary>
    Area,
    /// <summary>全体</summary>
    All,
    /// <summary>アイテム</summary>
    Item,
    /// <summary>地面</summary>
    Ground
}

/// <summary>
/// 素材アイテム
/// </summary>
public class Material : StackableItem
{
    public MaterialCategory Category { get; init; }

    /// <summary>品質（1-100）</summary>
    public int Quality { get; init; } = 50;

    public Material()
    {
        Type = ItemType.Material;
    }

    protected override char GetDefaultDisplayChar() => '*';
}

/// <summary>
/// 素材カテゴリ
/// </summary>
public enum MaterialCategory
{
    /// <summary>金属</summary>
    Metal,
    /// <summary>宝石</summary>
    Gem,
    /// <summary>布</summary>
    Cloth,
    /// <summary>皮革</summary>
    Leather,
    /// <summary>木材</summary>
    Wood,
    /// <summary>骨</summary>
    Bone,
    /// <summary>薬草</summary>
    Herb,
    /// <summary>モンスター素材</summary>
    Monster,
    /// <summary>魔法素材</summary>
    Magical
}

/// <summary>
/// 鍵アイテム
/// </summary>
public class KeyItem : Item
{
    /// <summary>対応するドア/宝箱のID</summary>
    public string? TargetId { get; init; }

    /// <summary>マスターキーか（複数に使用可能）</summary>
    public bool IsMasterKey { get; init; }

    /// <summary>使用後に消滅するか</summary>
    public bool ConsumeOnUse { get; init; } = true;

    public KeyItem()
    {
        Type = ItemType.Key;
    }

    public bool CanUnlock(string lockId)
    {
        return IsMasterKey || TargetId == lockId;
    }

    protected override char GetDefaultDisplayChar() => '-';
}
