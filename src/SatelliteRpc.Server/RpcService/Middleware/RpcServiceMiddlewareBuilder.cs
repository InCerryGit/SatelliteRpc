using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.RpcService.Middleware;

/// <summary>
/// Represents a builder for constructing the middleware pipeline for RPC services.
/// Implements the IRpcServiceMiddlewareBuilder interface.
/// </summary>
public class RpcServiceMiddlewareBuilder : IRpcServiceMiddlewareBuilder
{
    /// <summary>
    /// Optional configuration action for the ApplicationBuilder.
    /// </summary>
    private readonly Action<ApplicationBuilder<ServiceContext>>? _configure;

    /// <summary>
    /// The ApplicationBuilder that is used to construct the middleware pipeline.
    /// </summary>
    private readonly ApplicationBuilder<ServiceContext> _builder;

    /// <summary>
    /// Initializes a new instance of the RpcServiceMiddlewareBuilder class.
    /// </summary>
    /// <param name="services">The IServiceProvider that provides access to the application's service container.</param>
    /// <param name="configure">An optional configuration action for the ApplicationBuilder.</param>
    public RpcServiceMiddlewareBuilder(
        IServiceProvider services, 
        Action<ApplicationBuilder<ServiceContext>>? configure = null)
    {
        _configure = configure;
        _builder = new ApplicationBuilder<ServiceContext>(services);
    }

    /// <summary>
    /// Builds the middleware pipeline using the ApplicationBuilder.
    /// </summary>
    /// <returns>The delegate that represents the middleware pipeline.</returns>
    public ApplicationDelegate<ServiceContext> Build()
    {
        // Apply the configuration action to the ApplicationBuilder, if it exists.
        _configure?.Invoke(_builder);

        // Add the EndpointInvokeMiddleware to the pipeline and build the pipeline.
        return _builder.Use<EndpointInvokeMiddleware>().Build();
    }
}
