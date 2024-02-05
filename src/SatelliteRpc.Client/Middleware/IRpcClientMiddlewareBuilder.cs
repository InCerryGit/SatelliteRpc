using SatelliteRpc.Client.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Middleware;

/// <summary>
/// The middleware builder interface of Rpc Client
/// </summary>
public interface IRpcClientMiddlewareBuilder
{
    /// <summary>
    /// Build the middleware
    /// </summary>
    /// <returns></returns>
    ApplicationDelegate<CallContext> Build();
    
    /// <summary>
    /// The application middleware builder
    /// </summary>
    ApplicationBuilder<CallContext> Builder { get; }
}