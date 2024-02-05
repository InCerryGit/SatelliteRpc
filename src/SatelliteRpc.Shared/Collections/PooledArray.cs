using System.Buffers;

namespace SatelliteRpc.Shared.Collections;

/// <summary>
/// Represents a disposable array object that uses an array pool for better performance and less memory usage.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
public class PooledArray<T> : IDisposable
{
    private readonly T[] _array;
    private readonly int _length;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the PooledArray class with a specified length.
    /// </summary>
    /// <param name="length">The length of the array.</param>
    public PooledArray(int length)
    {
        _length = length;
        _array = ArrayPool<T>.Shared.Rent(length);
    }

    /// <summary>
    /// Gets a span that represents the array.
    /// </summary>
    public Span<T> Span
    {
        get
        {
            ThrowIfDisposed();
            return _array.AsSpan(0, _length);
        }
    }

    /// <summary>
    /// Gets a memory that represents the array.
    /// </summary>
    public Memory<T> Memory
    {
        get
        {
            ThrowIfDisposed();
            return _array.AsMemory(0, _length);
        }
    }

    /// <summary>
    /// Gets the raw array as an ArraySegment.
    /// </summary>
    public ArraySegment<T> RawArray
    {
        get
        {
            ThrowIfDisposed();
            return new ArraySegment<T>(_array, 0, _length);
        }
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Releases the array back to the pool and sets the PooledArray instance as disposed.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                ArrayPool<T>.Shared.Return(_array);
            }
            catch (Exception)
            {
                // Catch exceptions because ArrayPool doesn't always accept returned arrays
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the PooledArray instance has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("PooledArray is disposed");
        }
    }
}
