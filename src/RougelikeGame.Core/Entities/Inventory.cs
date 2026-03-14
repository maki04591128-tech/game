using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Items;

namespace RougelikeGame.Core.Entities;

/// <summary>
/// インベントリ実装
/// </summary>
public class Inventory : IInventory
{
    private readonly List<Items.Item> _items = new();

    public int MaxSlots { get; }
    public int UsedSlots => _items.Count;
    public IReadOnlyList<Items.Item> Items => _items.AsReadOnly();

    /// <summary>
    /// 総重量
    /// </summary>
    public float TotalWeight => _items.Sum(i => i.Weight * (i is IStackable s ? s.StackCount : 1));

    /// <summary>
    /// 最大重量
    /// </summary>
    public float MaxWeight { get; set; } = 100f;

    public Inventory(int maxSlots)
    {
        MaxSlots = maxSlots;
    }

    /// <summary>
    /// アイテムを追加
    /// </summary>
    public bool Add(Items.Item item)
    {
        // スタック可能なアイテムの場合、既存スタックに追加を試みる
        if (item is IStackable stackable)
        {
            var existing = _items
                .OfType<IStackable>()
                .FirstOrDefault(s => s.CanStackWith(stackable));

            if (existing != null)
            {
                existing.StackCount += stackable.StackCount;
                return true;
            }
        }

        if (UsedSlots >= MaxSlots)
            return false;

        _items.Add(item);
        return true;
    }

    /// <summary>
    /// アイテムを削除
    /// </summary>
    public bool Remove(Items.Item item)
    {
        return _items.Remove(item);
    }

    /// <summary>
    /// アイテムを数量指定で削除（スタックアイテム用）
    /// </summary>
    public bool Remove(Items.Item item, int count)
    {
        if (item is not IStackable stackable)
            return Remove(item);

        if (stackable.StackCount < count)
            return false;

        stackable.StackCount -= count;

        if (stackable.StackCount <= 0)
            return _items.Remove(item);

        return true;
    }

    /// <summary>
    /// アイテムを含むか確認
    /// </summary>
    public bool Contains(Items.Item item)
    {
        return _items.Contains(item);
    }

    /// <summary>
    /// アイテムIDで検索
    /// </summary>
    public Items.Item? FindById(string itemId)
    {
        return _items.FirstOrDefault(i => i.ItemId == itemId);
    }

    /// <summary>
    /// 複数のアイテムIDで検索
    /// </summary>
    public IEnumerable<Items.Item> FindAllById(string itemId)
    {
        return _items.Where(i => i.ItemId == itemId);
    }

    /// <summary>
    /// インデックスでアイテムを取得
    /// </summary>
    public Items.Item? GetByIndex(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        return _items[index];
    }

    /// <summary>
    /// 種類でフィルタ
    /// </summary>
    public IEnumerable<Items.Item> GetByType(Items.ItemType type)
    {
        return _items.Where(i => i.Type == type);
    }

    /// <summary>
    /// 装備可能アイテムを取得
    /// </summary>
    public IEnumerable<IEquippable> GetEquippableItems()
    {
        return _items.OfType<IEquippable>();
    }

    /// <summary>
    /// 消費可能アイテムを取得
    /// </summary>
    public IEnumerable<IConsumable> GetConsumableItems()
    {
        return _items.OfType<IConsumable>();
    }

    /// <summary>
    /// アイテムを使用
    /// </summary>
    public UseResult? UseItem(Items.Item item, Character user, IRandomProvider? random = null)
    {
        if (item is not IConsumable consumable)
            return null;

        if (!consumable.CanUse(user))
            return UseResult.Fail("このアイテムは使用できない");

        var result = consumable.Use(user, random);

        if (result.Success && consumable.ConsumeOnUse)
        {
            Remove(item, 1);
        }

        return result;
    }

    /// <summary>
    /// アイテムを整理（同種スタック）
    /// </summary>
    public void Organize()
    {
        // スタック可能なアイテムをまとめる
        var stackables = _items.OfType<IStackable>().ToList();
        var toRemove = new List<Items.Item>();

        foreach (var group in stackables.GroupBy(s => ((Items.Item)s).ItemId))
        {
            var items = group.ToList();
            if (items.Count <= 1) continue;

            var first = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                var other = items[i];
                int transferAmount = Math.Min(other.StackCount, first.MaxStack - first.StackCount);

                if (transferAmount > 0)
                {
                    first.StackCount += transferAmount;
                    other.StackCount -= transferAmount;
                }

                if (other.StackCount <= 0)
                    toRemove.Add((Items.Item)other);
            }
        }

        foreach (var item in toRemove)
        {
            _items.Remove(item);
        }

        // 種類順にソート
        _items.Sort((a, b) => {
            int typeCompare = a.Type.CompareTo(b.Type);
            if (typeCompare != 0) return typeCompare;
            return string.Compare(a.ItemId, b.ItemId, StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// インベントリをクリア
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// 空きスロット数
    /// </summary>
    public int FreeSlots => MaxSlots - UsedSlots;

    // IInventory互換メソッド
    bool IInventory.Add(Interfaces.IItem item)
    {
        if (item is Items.Item newItem)
            return Add(newItem);
        return false;
    }

    bool IInventory.Remove(Interfaces.IItem item)
    {
        if (item is Items.Item newItem)
            return Remove(newItem);
        return false;
    }

    bool IInventory.Contains(Interfaces.IItem item)
    {
        if (item is Items.Item newItem)
            return Contains(newItem);
        return false;
    }

    IReadOnlyList<Interfaces.IItem> IInventory.Items => 
        _items.Cast<Interfaces.IItem>().ToList().AsReadOnly();
}
