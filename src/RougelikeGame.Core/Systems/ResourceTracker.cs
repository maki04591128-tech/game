namespace RougelikeGame.Core.Systems;

/// <summary>
/// T.3: リソース追跡システム - メモリリーク検出のためのリソース割り当て/解放追跡
/// </summary>
public class ResourceTracker
{
    /// <summary>割り当て情報</summary>
    public record AllocationInfo(Guid Id, string ResourceName, long SizeBytes, DateTime AllocatedAt);

    private readonly Dictionary<Guid, AllocationInfo> _allocations = new();
    private long _totalAllocated;
    private long _totalFreed;
    private long _peakMemory;
    private long _currentMemory;

    /// <summary>アクティブな割り当て数</summary>
    public int ActiveAllocations => _allocations.Count;

    /// <summary>累計割り当てバイト数</summary>
    public long TotalAllocated => _totalAllocated;

    /// <summary>累計解放バイト数</summary>
    public long TotalFreed => _totalFreed;

    /// <summary>現在のメモリ使用量</summary>
    public long CurrentMemoryUsage => _currentMemory;

    /// <summary>ピークメモリ使用量</summary>
    public long PeakMemoryUsage => _peakMemory;

    /// <summary>リソースの割り当てを追跡</summary>
    public Guid Track(string resourceName, long sizeBytes)
    {
        var id = Guid.NewGuid();
        var info = new AllocationInfo(id, resourceName, sizeBytes, DateTime.UtcNow);
        _allocations[id] = info;
        _totalAllocated += sizeBytes;
        _currentMemory += sizeBytes;

        if (_currentMemory > _peakMemory)
            _peakMemory = _currentMemory;

        return id;
    }

    /// <summary>リソースの解放を記録</summary>
    public void Release(Guid allocationId)
    {
        if (_allocations.TryGetValue(allocationId, out var info))
        {
            _allocations.Remove(allocationId);
            _totalFreed += info.SizeBytes;
            _currentMemory -= info.SizeBytes;
        }
    }

    /// <summary>全割り当てIDを取得</summary>
    public IReadOnlyList<Guid> GetAllocationIds() =>
        _allocations.Keys.ToList().AsReadOnly();

    /// <summary>未解放リソース（リーク）を検出</summary>
    public IReadOnlyList<AllocationInfo> DetectLeaks() =>
        _allocations.Values.ToList().AsReadOnly();

    /// <summary>全追跡情報をリセット</summary>
    public void Reset()
    {
        _allocations.Clear();
        _totalAllocated = 0;
        _totalFreed = 0;
        _peakMemory = 0;
        _currentMemory = 0;
    }
}
