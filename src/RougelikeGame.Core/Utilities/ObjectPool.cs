using System.Collections.Concurrent;

namespace RougelikeGame.Core.Utilities;

/// <summary>
/// 頻繁に生成・破棄されるオブジェクトの再利用プール
/// </summary>
public class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly Func<T> _factory;
    private readonly Action<T>? _resetAction;
    private readonly int _maxSize;
    private int _currentCount;

    /// <summary>
    /// プール内の利用可能なオブジェクト数
    /// </summary>
    public int AvailableCount => _pool.Count;

    /// <summary>
    /// プールが管理している総オブジェクト数
    /// </summary>
    public int TotalCount => _currentCount;

    /// <summary>
    /// プールの最大サイズ
    /// </summary>
    public int MaxSize => _maxSize;

    /// <summary>
    /// ObjectPool を作成する
    /// </summary>
    /// <param name="factory">新しいオブジェクトを生成するファクトリ</param>
    /// <param name="resetAction">オブジェクトをプールに戻す際のリセット処理</param>
    /// <param name="maxSize">プールの最大サイズ</param>
    /// <param name="initialSize">初期に確保するオブジェクト数</param>
    public ObjectPool(Func<T> factory, Action<T>? resetAction = null, int maxSize = 256, int initialSize = 0)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _resetAction = resetAction;
        _maxSize = maxSize;

        // 初期プール確保
        initialSize = Math.Min(initialSize, maxSize);
        for (int i = 0; i < initialSize; i++)
        {
            _pool.Add(_factory());
            _currentCount++;
        }
    }

    /// <summary>
    /// プールからオブジェクトを取得する（プールが空なら新規作成）
    /// </summary>
    public T Rent()
    {
        if (_pool.TryTake(out var item))
        {
            return item;
        }

        Interlocked.Increment(ref _currentCount);
        return _factory();
    }

    /// <summary>
    /// オブジェクトをプールに返却する
    /// </summary>
    public void Return(T item)
    {
        if (item == null) return;

        _resetAction?.Invoke(item);

        if (_pool.Count < _maxSize)
        {
            _pool.Add(item);
        }
        else
        {
            Interlocked.Decrement(ref _currentCount);
        }
    }

    /// <summary>
    /// プール内の全オブジェクトをクリアする
    /// </summary>
    public void Clear()
    {
        while (_pool.TryTake(out _)) { }
        _currentCount = 0;
    }
}
