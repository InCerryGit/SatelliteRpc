namespace SatelliteRpc.Protocol.Protocol;

/// <summary>
///  Payload type
/// </summary>
public enum PayloadType
{
    /// <summary>
    /// Protobuf
    /// </summary>
    Protobuf = 0,
    
    /// <summary>
    ///  Json
    /// </summary>
    Json = 1
    
    // add more payload type here, like custom payload type
}