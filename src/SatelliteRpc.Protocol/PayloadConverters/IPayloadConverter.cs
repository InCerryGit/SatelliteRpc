using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.PayloadConverters;

public interface IPayloadConverter
{
    /// <summary>
    ///  Payload type
    ///  <see cref="PayloadType"/>
    /// </summary>
    PayloadType PayloadType { get; }
    
    /// <summary>
    ///  Convert payload to bytes
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    object? Convert(PooledArray<byte> payload, Type type);

    /// <summary>
    ///  Create payload writer
    ///  for performance reasons, we use the PayloadWriter to write the payload to the network buffer
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    PayloadWriter CreatePayloadWriter(object? payload);
}