using SatelliteRpc.Server.Configuration;
using SatelliteRpc.Server.RpcService;
using SatelliteRpc.Server.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Extensions;

/// <summary>
/// The IRpcServerBuilder extensions
/// </summary>
public static class RpcServerBuilderExtensions
{
    /// <summary>
    /// Adds a RPC connection handler to the RPC server builder.
    /// </summary>
    /// <param name="builder">The RPC server builder.</param>
    /// <param name="configure">An optional action to configure the application builder.</param>
    /// <returns>The RPC server builder with the added connection handler.</returns>
    public static IRpcServerBuilder AddRpcConnectionHandler(
        this IRpcServerBuilder builder,
        Action<ApplicationBuilder<RpcRawContext>>? configure = null)
    {
        builder.Services.AddRpcConnectionHandler(configure);
        return builder;
    }
    
    /// <summary>
    /// Adds a RPC service to the RPC server builder.
    /// </summary>
    /// <param name="builder">The RPC server builder.</param>
    /// <returns>The RPC server builder with the added RPC service.</returns>
    public static IRpcServerBuilder AddRpcService(this IRpcServerBuilder builder)
    {
        builder.Services.AddRpcService();
        return builder;
    }
    
    /// <summary>
    /// Adds a RPC service middleware to the RPC server builder.
    /// </summary>
    /// <param name="builder">The RPC server builder.</param>
    /// <param name="configure">An optional action to configure the application builder.</param>
    /// <returns>The RPC server builder with the added service middleware.</returns>
    public static IRpcServerBuilder AddRpcServiceMiddleware(
        this IRpcServerBuilder builder,
        Action<ApplicationBuilder<ServiceContext>>? configure = null)
    {
        builder.Services.AddRpcServiceMiddleware(configure);
        return builder;
    }
}
