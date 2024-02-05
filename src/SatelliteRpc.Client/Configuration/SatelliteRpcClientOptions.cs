using System.Net;
using SatelliteRpc.Protocol.Protocol;

namespace SatelliteRpc.Client.Configuration;

public class SatelliteRpcClientOptions
{
    /// <summary>
    /// Rpc remote server address
    /// </summary>
    public IPAddress ServerAddress { get; set; } = IPAddress.Parse("127.0.0.1");
    
    /// <summary>
    /// Rpc remote server port
    /// </summary>
    public int ServerPort { get; set; } = 58888;
    
    /// <summary>
    /// Request channel max count
    /// </summary>
    public int RequestChannelMaxCount { get; set; } = 1024;
    
    /// <summary>
    /// The request default serializer type
    /// </summary>
    public PayloadType PayloadType { get; set; } = PayloadType.Protobuf;
}