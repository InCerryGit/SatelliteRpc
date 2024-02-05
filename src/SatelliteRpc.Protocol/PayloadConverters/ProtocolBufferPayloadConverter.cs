using Google.Protobuf;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.PayloadConverters;

/// <summary>
///  Payload converter for protobuf
/// </summary>
public class ProtocolBufferPayloadConverter : IPayloadConverter
{
    public PayloadType PayloadType => PayloadType.Protobuf;

    /// <summary>
    ///  Create payload writer
    ///  for performance reasons, we use the PayloadWriter to write the payload to the network buffer
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public PayloadWriter CreatePayloadWriter(object? payload)
    {
        return payload switch
        {
            null => PayloadWriter.Empty,
            IMessage message => new PayloadWriter
            {
                GetPayloadSize = () => message.CalculateSize(),
                PayloadWriteTo = (buffer) => message.WriteTo(buffer)
            },
            _ => throw new Exception("Invalid response type")
        };
    }

    /// <summary>
    ///  Convert payload to bytes
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object? Convert(PooledArray<byte> payload, Type type)
    {
        if (!typeof(IMessage).IsAssignableFrom(type))
        {
            throw new ArgumentException("Type must be protobuf message", nameof(type));
        }

        if (payload.Length == 0)
        {
            return null;
        }
        
        var message = Activator.CreateInstance(type) as IMessage;
        message!.MergeFrom(payload.Span);
        return message!;
    }
}