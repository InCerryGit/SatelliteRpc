using Microsoft.Extensions.DependencyInjection;

namespace SatelliteRpc.Server.Configuration;

/// <summary>
/// Builder for rpc server
/// </summary>
public interface IRpcServerBuilder
{
    /// <summary>
    ///  Service collection
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    ///  Configure satellite rpc server
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    IRpcServerBuilder ConfigureSatelliteRpcServer(Action<SatelliteRpcServerOptions> configure);
}