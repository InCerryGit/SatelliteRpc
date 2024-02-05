using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Transport;

/// <summary>
///  Defines a mechanism for adding middleware to the RPC connection pipeline.
/// </summary>
public interface IRpcConnectionApplicationHandlerBuilder
{
    ApplicationDelegate<RpcRawContext> Build();
}