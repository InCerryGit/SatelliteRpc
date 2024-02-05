using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using SatelliteRpc.Shared;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.Protocol;

/// <summary>
/// AppRequest
/// </summary>
public class AppRequest : DisposeManager
{
    /// <summary>
    ///  Total length of the request
    /// </summary>
    public int TotalLength { get; set; }
    
    /// <summary>
    ///  Request Id, used to match the response
    ///  for multiplexing 
    /// </summary>
    public ulong Id { get; set; }
    
    /// <summary>
    ///  Request path
    /// </summary>
    public string Path { get; set; }
    
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
    /// </summary>
    /// <returns></returns>
    public (int HeadLength, int PayloadLength) GetSize()
    {
        // sum every field length
        return PayloadWriter.HasPayload 
            ? (sizeof(ulong) + Encoding.UTF8.GetByteCount(Path) + 1 + sizeof(int) + sizeof(int), PayloadWriter.GetPayloadSize()) 
            : (sizeof(ulong) + Encoding.UTF8.GetByteCount(Path) + 1 + sizeof(int) + sizeof(int), Payload.Length);
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

        // write path, and add null terminator
        var pathBytes = Encoding.UTF8.GetBytes(Path);
        pathBytes.CopyTo(span);
        span[pathBytes.Length] = 0; // Null terminator
        span = span[(pathBytes.Length + 1)..];

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
    public static (bool Success, AppRequest? Request) TryDeserialize(
        ReadOnlySequence<byte> sequence,
        out SequencePosition consumed,
        AppRequest? reuse = null)
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
            
        // Deserialize the request
        var request = reuse ?? new AppRequest();
        request.TotalLength = totalLength;

        // read id
        reader.TryReadLittleEndian(out long id);
        request.Id = (ulong)id;

        // read path
        if (reader.TryReadTo(out ReadOnlySequence<byte> pathSequence, (byte)'\0'))
        {
            request.Path = Encoding.UTF8.GetString(pathSequence.ToArray());
        }

        // read payload type
        reader.TryReadLittleEndian(out int payloadType);
        request.PayloadType = (PayloadType)payloadType;

        // read payload length
        reader.TryReadLittleEndian(out int payloadLength);

        // for performance reasons, we use the PooledArray to store the payload
        // PooledArray use memory pool
        var payload = new PooledArray<byte>(payloadLength);
        reader.Sequence.Slice(reader.Position, payloadLength).CopyTo(payload.Span);
        
        // The payload is stored in the PooledArray,
        // so we need to register the PooledArray to the DisposeManager,
        // when the AppRequest is disposed, the PooledArray will be automatically returned to the memory pool
        request.RegisterForDispose(payload);
        request.Payload = payload;
        reader.Advance(payloadLength);
            
        // consumed
        consumed = reader.Position;

        return (true, request);
    }
}