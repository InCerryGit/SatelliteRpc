using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.RpcService;

/// <summary>
/// Represents the context for a single RPC service invocation.
/// </summary>
public class ServiceContext
{
    /// <summary>
    /// Gets the raw context of the RPC request. This includes the original request message and metadata.
    /// </summary>
    public RpcRawContext RawContext { get; }
    
    /// <summary>
    /// Gets the endpoint information for the RPC service that will handle this request.
    /// </summary>
    public RpcServiceEndpoint Endpoint { get; }
    
    /// <summary>
    /// Gets the arguments that were passed in the RPC request.
    /// </summary>
    public object?[] Arguments { get; }
    
    /// <summary>
    /// Gets or sets the result of the RPC service invocation. This will be null before the service has been invoked.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceContext"/> class.
    /// </summary>
    /// <param name="rawContext">The raw context of the RPC request.</param>
    /// <param name="endpoint">The endpoint information for the RPC service that will handle this request.</param>
    /// <param name="arguments">The arguments that were passed in the RPC request.</param>
    public ServiceContext(RpcRawContext rawContext, RpcServiceEndpoint endpoint, object?[] arguments)
    {
        RawContext = rawContext;
        Endpoint = endpoint;
        Arguments = arguments;
    }
}
