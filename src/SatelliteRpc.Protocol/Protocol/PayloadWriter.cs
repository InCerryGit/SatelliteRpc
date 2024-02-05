using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace SatelliteRpc.Protocol.Protocol;

public struct PayloadWriter
{
    /// <summary>
    /// Empty payload writer
    /// </summary>
    public static readonly PayloadWriter Empty = new PayloadWriter
    {
        GetPayloadSize = () => 0,
        PayloadWriteTo = _ => { }
    };
        
    /// <summary>
    ///  Has payload
    /// </summary>
    [MemberNotNullWhen(true, nameof(GetPayloadSize), nameof(PayloadWriteTo))]
    public bool HasPayload => PayloadWriteTo is not null && GetPayloadSize is not null;
        
    /// <summary>
    ///  Get or set calculate payload size method
    /// </summary>
    public Func<int>? GetPayloadSize { get; set; }
        
    /// <summary>
    ///  Get or set payload write to method
    /// </summary>
    public Action<IBufferWriter<byte>>? PayloadWriteTo { get; set; } 
}