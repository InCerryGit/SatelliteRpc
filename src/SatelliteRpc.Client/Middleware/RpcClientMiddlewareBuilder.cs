using SatelliteRpc.Client.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Middleware;

public class RpcClientMiddlewareBuilder : IRpcClientMiddlewareBuilder
{
    /// <summary>
    /// The application middleware builder
    /// </summary>
    public ApplicationBuilder<CallContext> Builder { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="services">service provider</param>
    /// <param name="configure">configure middleware action</param>
    public RpcClientMiddlewareBuilder(
        IServiceProvider services, 
        Action<ApplicationBuilder<CallContext>>? configure = null)
    {
        Builder = new ApplicationBuilder<CallContext>(services);
        configure?.Invoke(Builder);
    }

    /// <summary>
    /// Build the middleware
    /// </summary>
    /// <returns></returns>
    public ApplicationDelegate<CallContext> Build()
    {
        return Builder.Build();
    }
}