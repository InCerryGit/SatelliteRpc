using SatelliteRpc.Client.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Middleware;

/// <summary>
/// The middleware interface of Rpc Client
/// </summary>
public interface IRpcClientMiddleware : IApplicationMiddleware<CallContext>
{
    
}