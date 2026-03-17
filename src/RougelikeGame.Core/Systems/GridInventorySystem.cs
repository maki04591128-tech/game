namespace RougelikeGame.Core.Systems;

/// <summary>
/// グリッドインベントリシステム - EFT参考のアイテムサイズ管理
/// </summary>
public class GridInventorySystem
{
    /// <summary>グリッドアイテム</summary>
    public record GridItem(
        string ItemId,
        string Name,
        GridItemSize Size,
        int GridX,
        int GridY,
        bool IsRotated
    );

    private readonly int _width;
    private readonly int _height;
    private readonly List<GridItem> _items = new();

    public GridInventorySystem(int width = 10, int height = 6)
    {
        _width = width;
        _height = height;
    }

    /// <summary>グリッド幅</summary>
    public int Width => _width;

    /// <summary>グリッド高さ</summary>
    public int Height => _height;

    /// <summary>配置済みアイテム</summary>
    public IReadOnlyList<GridItem> Items => _items;

    /// <summary>アイテムサイズを取得</summary>
    public static (int W, int H) GetDimensions(GridItemSize size) => size switch
    {
        GridItemSize.Size1x1 => (1, 1),
        GridItemSize.Size1x2 => (1, 2),
        GridItemSize.Size2x1 => (2, 1),
        GridItemSize.Size2x2 => (2, 2),
        GridItemSize.Size2x3 => (2, 3),
        _ => (1, 1)
    };

    /// <summary>配置可能か判定</summary>
    public bool CanPlace(GridItemSize size, int x, int y)
    {
        var (w, h) = GetDimensions(size);
        if (x < 0 || y < 0 || x + w > _width || y + h > _height) return false;
        return !_items.Any(item => Overlaps(item, x, y, w, h));
    }

    /// <summary>アイテムを配置</summary>
    public bool PlaceItem(string itemId, string name, GridItemSize size, int x, int y)
    {
        if (!CanPlace(size, x, y)) return false;
        _items.Add(new GridItem(itemId, name, size, x, y, false));
        return true;
    }

    /// <summary>アイテムを除去</summary>
    public bool RemoveItem(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.ItemId == itemId);
        if (item == null) return false;
        _items.Remove(item);
        return true;
    }

    /// <summary>空きスペースの割合を取得</summary>
    public float GetFreeSpaceRatio()
    {
        int total = _width * _height;
        int used = _items.Sum(i =>
        {
            var (w, h) = GetDimensions(i.Size);
            return w * h;
        });
        return (float)(total - used) / total;
    }

    private static bool Overlaps(GridItem item, int x, int y, int w, int h)
    {
        var (iw, ih) = GetDimensions(item.Size);
        return !(item.GridX + iw <= x || x + w <= item.GridX || item.GridY + ih <= y || y + h <= item.GridY);
    }

    /// <summary>
    /// 全アイテムを消去する（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、グリッドインベントリは空になる。
    /// </summary>
    public void Reset()
    {
        _items.Clear();
    }
}
