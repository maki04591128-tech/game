using RougelikeGame.Core.Utilities;
using Xunit;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// ObjectPool のユニットテスト
/// </summary>
public class ObjectPoolTests
{
    private class TestObject
    {
        public int Value { get; set; }
    }

    [Fact]
    public void Rent_新規オブジェクトを生成して返す()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var obj = pool.Rent();

        Assert.NotNull(obj);
        Assert.Equal(1, pool.TotalCount);
    }

    [Fact]
    public void Return_オブジェクトをプールに返却する()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        var obj = pool.Rent();

        pool.Return(obj);

        Assert.Equal(1, pool.AvailableCount);
    }

    [Fact]
    public void Rent_プールからオブジェクトを再利用する()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        var obj1 = pool.Rent();
        obj1.Value = 42;
        pool.Return(obj1);

        var obj2 = pool.Rent();

        Assert.Same(obj1, obj2);
        Assert.Equal(0, pool.AvailableCount);
    }

    [Fact]
    public void ResetAction_返却時にリセットが実行される()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            obj => obj.Value = 0);

        var obj = pool.Rent();
        obj.Value = 42;
        pool.Return(obj);

        var reused = pool.Rent();
        Assert.Equal(0, reused.Value);
    }

    [Fact]
    public void InitialSize_初期プール確保()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            initialSize: 5);

        Assert.Equal(5, pool.AvailableCount);
        Assert.Equal(5, pool.TotalCount);
    }

    [Fact]
    public void MaxSize_上限を超えた場合はプールに返却されない()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            maxSize: 2);

        var obj1 = pool.Rent();
        var obj2 = pool.Rent();
        var obj3 = pool.Rent();

        pool.Return(obj1);
        pool.Return(obj2);
        pool.Return(obj3); // maxSize=2 なので3つ目は捨てられる

        Assert.Equal(2, pool.AvailableCount);
    }

    [Fact]
    public void Clear_プールを空にする()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            initialSize: 5);

        pool.Clear();

        Assert.Equal(0, pool.AvailableCount);
        Assert.Equal(0, pool.TotalCount);
    }

    [Fact]
    public void Return_nullを渡しても例外にならない()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        pool.Return(null!);

        Assert.Equal(0, pool.AvailableCount);
    }

    [Fact]
    public void MaxSize_プロパティが正しい()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            maxSize: 100);

        Assert.Equal(100, pool.MaxSize);
    }

    [Fact]
    public void 複数回のRentReturn_正しくカウントされる()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var objects = new List<TestObject>();
        for (int i = 0; i < 10; i++)
        {
            objects.Add(pool.Rent());
        }

        Assert.Equal(10, pool.TotalCount);
        Assert.Equal(0, pool.AvailableCount);

        foreach (var obj in objects)
        {
            pool.Return(obj);
        }

        Assert.Equal(10, pool.AvailableCount);
    }

    [Fact]
    public void InitialSize_maxSizeを超えない()
    {
        var pool = new ObjectPool<TestObject>(
            () => new TestObject(),
            maxSize: 3,
            initialSize: 10);

        Assert.True(pool.AvailableCount <= 3);
    }
}
