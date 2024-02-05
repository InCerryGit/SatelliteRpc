using SatelliteRpc.Server.RpcService;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Transport;

/// <summary>
/// Builder for constructing the application handler for an RPC connection.
/// </summary>
public class RpcConnectionApplicationHandlerBuilder : IRpcConnectionApplicationHandlerBuilder
{
    /// <summary>
    /// An optional function to configure the application builder.
    /// </summary>
    private readonly Action<ApplicationBuilder<RpcRawContext>>? _configure;

    /// <summary>
    /// The application builder which will be used to build the application handler.
    /// </summary>
    private readonly ApplicationBuilder<RpcRawContext> _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RpcConnectionApplicationHandlerBuilder"/> class.
    /// </summary>
    /// <param name="services">The service provider to be used by the application builder.</param>
    /// <param name="configure">An optional function to configure the application builder.</param>
    public RpcConnectionApplicationHandlerBuilder(
        IServiceProvider services, 
        Action<ApplicationBuilder<RpcRawContext>>? configure = null)
    {
        _configure = configure;
        _builder = new ApplicationBuilder<RpcRawContext>(services);
    }

    /// <summary>
    /// Builds the application handler for the RPC connection.
    /// </summary>
    /// <returns>The built application handler.</returns>
    public ApplicationDelegate<RpcRawContext> Build()
    {
        // Invoke the configuration function if it is provided.
        _configure?.Invoke(_builder);

        // Use the RpcServiceHandler middleware and build the application delegate.
        return _builder.Use<RpcServiceHandler>().Build();
    }
}
