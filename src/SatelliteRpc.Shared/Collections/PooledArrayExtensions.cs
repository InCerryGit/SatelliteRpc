namespace SatelliteRpc.Shared.Collections;

/// <summary>
/// Contains extension methods for PooledArray and Array classes.
/// </summary>
public static class PooledArrayExtensions
{
    /// <summary>
    /// Converts a regular array to a PooledArray.
    /// </summary>
    /// <param name="array">The array to be converted.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new instance of PooledArray containing the same elements as the input array.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T[] array)
    {
        var pooledArray = new PooledArray<T>(array.Length);
        array.CopyTo(pooledArray.Span);
        return pooledArray;
    }

    /// <summary>
    /// Converts a PooledArray to a regular array.
    /// </summary>
    /// <param name="pooledArray">The PooledArray to be converted.</param>
    /// <typeparam name="T">The type of the elements in the PooledArray.</typeparam>
    /// <returns>A new array containing the same elements as the input PooledArray.</returns>
    public static T[] ToArray<T>(this PooledArray<T> pooledArray)
    {
        return pooledArray.Span.ToArray();
    }
}
