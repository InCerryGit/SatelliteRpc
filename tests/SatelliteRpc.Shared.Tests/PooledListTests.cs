using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Shared.Tests;

public class PooledListTests
{
    [Fact]
    public void Add_Adds_Items_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Insert_Inserts_Items_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Insert(1, 3);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
        Assert.Equal(2, list[2]);
    }

    [Fact]
    public void Remove_Removes_Items_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Remove(2);

        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
    }

    [Fact]
    public void Clear_Clears_Items_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void Indexer_Gets_And_Sets_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list[1] = 3;

        Assert.Equal(3, list[1]);
    }

    [Fact]
    public void Dispose_Returns_Buffer_To_Pool()
    {
        var list = new PooledList<int>();
        list.Add(1);
        list.Dispose();
        
        Assert.Null(list.Buffer);
    }

    [Fact]
    public void Enumerator_Enumerates_Correctly()
    {
        using var list = new PooledList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        int expected = 1;
        foreach (var item in list)
        {
            Assert.Equal(expected++, item);
        }
    }
}