using System.Buffers;
using System.Buffers.Binary;
using SatelliteRpc.Shared;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.Protocol;

/// <summary>
/// AppResponse
/// </summary>
public class AppResponse : DisposeManager
{
    /// <summary>
    ///  Total length of the request
    /// </summary>
    public int TotalLength { get; set; }
    
    /// <summary>
    ///  Request Id, used to match the request
    ///  for multiplexing 
    /// </summary>
    public ulong Id { get; set; }
    
    /// <summary>
    ///  Response status
    ///  <see cref="ResponseStatus"/>
    /// </summary>
    public ResponseStatus Status { get; set; }
    
    /// <summary>
    ///  Payload type
    /// </summary>
    public PayloadType PayloadType { get; set; }
    
    /// <summary>
    ///  Payload
    ///  if the payload is empty, then the PayloadWriter is used
    ///  note: because performance reasons, we use the PayloadWriter to write the payload to the network buffer.
    /// </summary>
    public PooledArray<byte> Payload { get; set; }
    
    /// <summary>
    ///  Payload writer
    /// </summary>
    public PayloadWriter PayloadWriter { get; set; } = default;

    /// <summary>
    ///  Get this request size
    ///  except for the first 4 bytes
    /// </summary>
    /// <returns></returns>
    public (int HeadLength, int PayloadLength) GetSize()
    {
        // sum every field length
        return PayloadWriter.HasPayload
            ? (sizeof(ulong) + sizeof(int) + sizeof(int) + sizeof(int), PayloadWriter.GetPayloadSize())
            : (sizeof(ulong) + sizeof(int) + sizeof(int) + sizeof(int), Payload.Length);
    }
        
    public void Serialize(IBufferWriter<byte> writer)
    {
        // The data format is as follows:
        // 4 bytes total length
        // N bytes header
        // N bytes payload
            
        var length = GetSize();
        var totalLength = length.HeadLength + length.PayloadLength + 4;
            
        scoped var span = writer.GetSpan(totalLength);
            
        // write total length
        BinaryPrimitives.WriteInt32LittleEndian(span, length.HeadLength + length.PayloadLength);
        span = span[sizeof(int)..];

        // write id
        BinaryPrimitives.WriteUInt64LittleEndian(span, Id);
        span = span[sizeof(ulong)..];

        // write status
        BinaryPrimitives.WriteInt32LittleEndian(span, (int)Status);
        span = span[sizeof(int)..];

        // write payload type
        BinaryPrimitives.WriteInt32LittleEndian(span, (int)PayloadType);
        span = span[sizeof(int)..];
            
        // write payload length
        BinaryPrimitives.WriteInt32LittleEndian(span, length.PayloadLength);
        span = span[sizeof(int)..];

        // for performance reasons, we use the PayloadWriter to write the payload to the network buffer
        // reduced memory copying
        if (!PayloadWriter.HasPayload)
        {
            Payload.Span.CopyTo(span);
            writer.Advance(totalLength);
        }
        else
        {
            writer.Advance(length.HeadLength + 4);
            PayloadWriter.PayloadWriteTo(writer);
        }
    }

    /// <summary>
    ///  Try deserialize
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="consumed"></param>
    /// <param name="reuse"></param>
    /// <returns></returns>
    public static (bool Success, AppResponse? Request) TryDeserialize(
        ReadOnlySequence<byte> sequence,
        out SequencePosition consumed,
        AppResponse? reuse = null)
    {
        // The data format is as follows:
        // 4 bytes total length
        // N bytes header
        // N bytes payload
        
        consumed = default;
        
        // if the sequence length is less than 4, it means that the request is not complete
        if (sequence.Length < 4)
        {
            return (false, null);
        }
            
        // read the total length of the request,
        // if the sequence length is less than the total length,
        // it means that the request is not complete
        var reader = new SequenceReader<byte>(sequence);
        reader.TryReadLittleEndian(out int totalLength);
        if (sequence.Length < totalLength + 4)
        {
            return (false, null);
        }
            
        // Deserialize the response
        var response = reuse ?? new AppResponse();
        response.TotalLength = totalLength;
            
        // read id
        reader.TryReadLittleEndian(out long id);
        response.Id = (ulong)id;

        // read status
        reader.TryReadLittleEndian(out int status);
        response.Status = (ResponseStatus)status;

        // read payload type
        reader.TryReadLittleEndian(out int payloadType);
        response.PayloadType = (PayloadType)payloadType;

        // read payload length
        reader.TryReadLittleEndian(out int payloadLength);

        // for performance reasons, we use the PooledArray to store the payload
        // PooledArray use memory pool
        var payload = new PooledArray<byte>(payloadLength);
        reader.Sequence.Slice(reader.Position, payloadLength).CopyTo(payload.Span);
        
        // The payload is stored in the PooledArray,
        // so we need to register the PooledArray to the DisposeManager,
        // when the AppRequest is disposed, the PooledArray will be automatically returned to the memory pool       
        response.RegisterForDispose(payload);
        response.Payload = payload;
            
        reader.Advance(payloadLength);
            
        consumed = reader.Position;

        return (true, response);
    }
}