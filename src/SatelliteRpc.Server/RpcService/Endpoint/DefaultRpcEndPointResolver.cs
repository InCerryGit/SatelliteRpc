using SatelliteRpc.Server.Exceptions;

namespace SatelliteRpc.Server.RpcService.Endpoint;

/// <summary>
/// The DefaultRpcEndPointResolver class is an implementation of the IEndpointResolver interface.
/// It resolves RPC service endpoints using a data source.
/// </summary>
public class DefaultRpcEndPointResolver : IEndpointResolver
{
    // The data source used to resolve endpoints.
    private readonly RpcServiceEndpointDataSource _dataSource;

    /// <summary>
    /// Initializes a new instance of the DefaultRpcEndPointResolver class.
    /// </summary>
    /// <param name="dataSource">The data source used to resolve endpoints.</param>
    public DefaultRpcEndPointResolver(RpcServiceEndpointDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <summary>
    /// Gets an RPC service endpoint for a given path.
    /// </summary>
    /// <param name="path">The path for which to get the endpoint.</param>
    /// <returns>The RPC service endpoint for the given path.</returns>
    /// <exception cref="NotFoundException">Thrown when no endpoint is found for the given path.</exception>
    public RpcServiceEndpoint GetEndpoint(string path)
    {
        var endPoint = _dataSource.GetEndpoint(path);
        if (endPoint is null)
        {
            throw new NotFoundException($"No endpoint found for path: {path}");
        }

        return endPoint;
    }
}
