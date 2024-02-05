using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.RpcService.Middleware;

/// <summary>
///  Defines a mechanism for adding middleware to the RPC service pipeline.
/// </summary>
public interface IRpcServiceMiddlewareBuilder
{
    ApplicationDelegate<ServiceContext> Build();
}