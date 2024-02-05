using System.Collections.Concurrent;

namespace SatelliteRpc.Server.RpcService.Endpoint;

/// <summary>
/// Represents a data source for RpcServiceEndpoint instances. 
/// This class is thread-safe and allows for concurrent access to the underlying data.
/// </summary>
public class RpcServiceEndpointDataSource
{
    /// <summary>
    /// A concurrent dictionary used to store RpcServiceEndpoint instances. 
    /// The key is the path of the endpoint.
    /// </summary>
    private readonly ConcurrentDictionary<string, RpcServiceEndpoint> _endpoints = new();

    /// <summary>
    /// Adds a new RpcServiceEndpoint to the data source.
    /// </summary>
    /// <param name="endpoint">The RpcServiceEndpoint to add.</param>
    public void AddEndpoint(RpcServiceEndpoint endpoint)
    {
        _endpoints.TryAdd(endpoint.Path, endpoint);
    }

    /// <summary>
    /// Retrieves an RpcServiceEndpoint from the data source by its path.
    /// </summary>
    /// <param name="path">The path of the RpcServiceEndpoint to retrieve.</param>
    /// <returns>The RpcServiceEndpoint if found, null otherwise.</returns>
    public RpcServiceEndpoint? GetEndpoint(string path)
    {
        _endpoints.TryGetValue(path, out var endpoint);
        return endpoint;
    }

    /// <summary>
    /// Retrieves all RpcServiceEndpoint instances from the data source.
    /// </summary>
    /// <returns>An array of all RpcServiceEndpoint instances.</returns>
    public RpcServiceEndpoint[] GetEndpoints()
    {
        return _endpoints.Values.ToArray();
    }
}