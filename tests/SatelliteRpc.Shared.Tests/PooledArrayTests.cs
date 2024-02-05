using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Shared.Tests;

public class PooledArrayTests
{
    [Fact]
    public void Can_Rent_And_Return_Array()
    {
        using var pooledArray = new PooledArray<int>(10);
        Assert.NotNull(pooledArray.RawArray.Array);
        Assert.Equal(10, pooledArray.RawArray.Count);
    }

    [Fact]
    public void Can_Access_Array_As_Span()
    {
        using var pooledArray = new PooledArray<int>(10);
        Span<int> span = pooledArray.Span;
        Assert.Equal(10, span.Length);
    }

    [Fact]
    public void Can_Access_Array_As_Memory()
    {
        using var pooledArray = new PooledArray<int>(10);
        Memory<int> memory = pooledArray.Memory;
        Assert.Equal(10, memory.Length);
    }

    [Fact]
    public void Throws_When_Disposed()
    {
        var pooledArray = new PooledArray<int>(10);
        pooledArray.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pooledArray.RawArray);
        Assert.Throws<ObjectDisposedException>(() =>
        {
            _ = pooledArray.Span;
        });
        Assert.Throws<ObjectDisposedException>(() => pooledArray.Memory);
    }
}