using System.Net;

namespace SatelliteRpc.Server.Configuration;

public class SatelliteRpcServerOptions
{
    /// <summary>
    /// Rpc server listen port
    /// </summary>
    public int Port { get; set; } = 58888;

    /// <summary>
    /// Rpc server listen address
    /// </summary>
    public IPAddress Host { get; set; } = IPAddress.Parse("127.0.0.1");
    
    /// <summary>
    /// Response write channel max count
    /// </summary>
    public int WriteChannelMaxCount { get; set; } = 1024;
}