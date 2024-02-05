using Microsoft.Extensions.Logging;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Server.Exceptions;
using SatelliteRpc.Server.RpcService.DataExchange;
using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Server.RpcService.Middleware;
using SatelliteRpc.Server.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.RpcService;

/// <summary>
/// Handles the processing of Rpc services.
/// </summary>
public class RpcServiceHandler : IApplicationMiddleware<RpcRawContext>
{
    private readonly ILogger<RpcServiceHandler> _logger;
    private readonly IEndpointResolver _endpointResolver;
    private readonly IRpcDataExchange _rpcDataExchange;
    private readonly ApplicationDelegate<ServiceContext> _middleware;

    /// <summary>
    /// Initializes a new instance of the <see cref="RpcServiceHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="endpointResolver">The endpoint resolver.</param>
    /// <param name="rpcDataExchange">The RPC data exchange.</param>
    /// <param name="middlewareBuilder">The middleware builder.</param>
    public RpcServiceHandler(
        ILogger<RpcServiceHandler> logger,
        IEndpointResolver endpointResolver,
        IRpcDataExchange rpcDataExchange,
        IRpcServiceMiddlewareBuilder middlewareBuilder)
    {
        _logger = logger;
        _endpointResolver = endpointResolver;
        _rpcDataExchange = rpcDataExchange;
        _middleware = middlewareBuilder.Build();
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    /// <param name="_">The application delegate to be ignored.</param>
    /// <param name="rawContext">The raw RPC context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask InvokeAsync(ApplicationDelegate<RpcRawContext> _, RpcRawContext rawContext)
    {
        try
        {
            // Resolve the endpoint from the request path
            var endpoint = _endpointResolver.GetEndpoint(rawContext.Request.Path);
            
            // Bind the parameters from the endpoint and raw context
            var parameters = _rpcDataExchange.BindParameters(endpoint, rawContext);
            var serviceContext = new ServiceContext(rawContext, endpoint, parameters);
            
            await _middleware(serviceContext);
            
            // Set the response payload writer with the result and raw context
            rawContext.Response.PayloadWriter = _rpcDataExchange.GetPayloadWriter(serviceContext.Result, rawContext);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Not found endpoint");
            rawContext.Response.Status = ResponseStatus.NotFound;
        }
        catch (ParametersBindException ex)
        {
            _logger.LogError(ex, "Parameters bind error");
            rawContext.Response.Status = ResponseStatus.BadRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal server error");
            rawContext.Response.Status = ResponseStatus.InternalError;
        }
    }
}
