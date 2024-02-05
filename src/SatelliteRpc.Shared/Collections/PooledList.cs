#nullable disable
using System.Buffers;
using System.Collections;

namespace SatelliteRpc.Shared.Collections;

/// <summary>
/// A list that uses array pooling for improved performance and reduced GC pressure.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class PooledList<T> : IList<T>, IDisposable
{
    internal T[] Buffer;
    private int _count;

    /// <summary>
    /// Initializes a new instance of the PooledList class with a specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The number of elements that the new list can initially store.</param>
    public PooledList(int initialCapacity = 8)
    {
        if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        Buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
            return Buffer[index];
        }
        set
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
            Buffer[index] = value;
        }
    }

    /// <summary>
    /// Gets a span that includes all elements of the list.
    /// </summary>
    public Span<T> Span => Buffer.AsSpan(0, _count);

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    /// <param name="item">The object to add to the list.</param>
    public void Add(T item)
    {
        EnsureCapacity(_count + 1);
        Buffer[_count++] = item;
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        Array.Clear(Buffer, 0, _count); // Clear to allow GC to collect
        _count = 0;
    }

    /// <summary>
    /// Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    public bool Contains(T item)
    {
        return Array.IndexOf(Buffer, item, 0, _count) >= 0;
    }

    /// <summary>
    /// Copies the elements of the list to an Array, starting at a particular Array index.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from list.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(Buffer, 0, array, arrayIndex, _count);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < _count; i++)
        {
            yield return Buffer[i];
        }
    }

    /// <summary>
    /// Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    public int IndexOf(T item)
    {
        return Array.IndexOf(Buffer, item, 0, _count);
    }

    /// <summary>
    /// Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert into the list.</param>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > _count) throw new ArgumentOutOfRangeException(nameof(index));
        EnsureCapacity(_count + 1);
        Array.Copy(Buffer, index, Buffer, index + 1, _count - index);
        Buffer[index] = item;
        _count++;
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The object to remove from the list.</param>
    public bool Remove(T item)
    {
        var index = Array.IndexOf(Buffer, item, 0, _count);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes the list item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
        _count--;
        Array.Copy(Buffer, index + 1, Buffer, index, _count - index);
        Buffer[_count] = default; // Clear to allow GC to collect
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Releases all resources used by the PooledList.
    /// </summary>
    public void Dispose()
    {
        if (Buffer != null)
        {
            try
            {
                ArrayPool<T>.Shared.Return(Buffer);
            }
            catch (Exception)
            {
                // Catch exceptions because ArrayPool doesn't always accept returned arrays
            }
            Buffer = null;
        }
    }

    /// <summary>
    /// Ensures that the capacity of this list is at least the specified minimum value.
    /// </summary>
    /// <param name="min">The new minimum capacity for this list.</param>
    private void EnsureCapacity(int min)
    {
        if (Buffer.Length < min)
        {
            var newCapacity = Buffer.Length == 0 ? 4 : Buffer.Length * 2;
            if (newCapacity < min) newCapacity = min;
            var newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);
            if (_count > 0)
            {
                Array.Copy(Buffer, 0, newBuffer, 0, _count);
                ArrayPool<T>.Shared.Return(Buffer);
            }
            Buffer = newBuffer;
        }
    }
}
