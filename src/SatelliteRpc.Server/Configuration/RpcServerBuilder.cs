using Microsoft.Extensions.DependencyInjection;

namespace SatelliteRpc.Server.Configuration;

/// <summary>
/// Class that provides the functionality to construct an RPC server.
/// This class implements the IRpcServerBuilder interface.
/// </summary>
public class RpcServerBuilder : IRpcServerBuilder
{
    /// <summary>
    /// Gets the service collection that this RPC server will use.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Constructs a new instance of RpcServerBuilder with the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to use for this RPC server.</param>
    public RpcServerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Configures the SatelliteRpcServer with the provided options.
    /// </summary>
    /// <param name="configure">An action that configures the options for the SatelliteRpcServer.</param>
    /// <returns>Returns the IRpcServerBuilder instance after it has been configured.</returns>
    public IRpcServerBuilder ConfigureSatelliteRpcServer(Action<SatelliteRpcServerOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}
