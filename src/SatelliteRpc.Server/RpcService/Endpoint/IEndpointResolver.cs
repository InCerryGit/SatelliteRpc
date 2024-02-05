namespace SatelliteRpc.Server.RpcService.Endpoint;

/// <summary>
/// Defines a mechanism for resolving RPC service endpoints.
/// </summary>
public interface IEndpointResolver
{
    /// <summary>
    /// Resolves the <see cref="RpcServiceEndpoint"/> for a given path.
    /// </summary>
    /// <param name="path">The path for which to resolve the endpoint.</param>
    /// <returns>The resolved <see cref="RpcServiceEndpoint"/>.</returns>
    RpcServiceEndpoint GetEndpoint(string path);
}
