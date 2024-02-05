using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.RpcService.DataExchange;

/// <summary>
/// The IRpcDataExchange interface defines the methods needed to bind parameters for RPC calls and to
/// get a payload writer for the results of those calls.
/// </summary>
public interface IRpcDataExchange
{
    /// <summary>
    /// Binds the parameters for an RPC service call.
    /// </summary>
    /// <param name="endpoint">The endpoint of the RPC service.</param>
    /// <param name="rawContext">The raw context of the RPC call, which contains the raw payload.</param>
    /// <returns>An array of objects representing the parameters to be used for the RPC call.</returns>
    object?[] BindParameters(RpcServiceEndpoint endpoint, RpcRawContext rawContext);

    /// <summary>
    /// Gets a PayloadWriter to write the result of an RPC call to a payload.
    /// </summary>
    /// <param name="result">The result of the RPC call.</param>
    /// <param name="rawContext">The raw context of the RPC call, which contains the raw payload.</param>
    /// <returns>A PayloadWriter that can be used to write the result to a payload.</returns>
    PayloadWriter GetPayloadWriter(object? result, RpcRawContext rawContext);
}
