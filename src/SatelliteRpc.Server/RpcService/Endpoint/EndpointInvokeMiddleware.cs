using Microsoft.Extensions.DependencyInjection;
using SatelliteRpc.Server.RpcService.Middleware;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.RpcService.Endpoint;

/// <summary>
/// Middleware for invoking an endpoint in a RPC service.
/// This class is responsible for retrieving the service instance associated with a given endpoint and invoking it.
/// </summary>
public class EndpointInvokeMiddleware : IRpcServiceMiddleware
{
    /// <summary>
    /// The service provider used to resolve service instances.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointInvokeMiddleware"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve service instances.</param>
    public EndpointInvokeMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Invokes the middleware in the RPC service pipeline.
    /// </summary>
    /// <param name="_">The next middleware delegate in the pipeline. Currently not used in this middleware.</param>
    /// <param name="context">The context for the current service call.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async ValueTask InvokeAsync(ApplicationDelegate<ServiceContext> _, ServiceContext context)
    {
        var endpoint = context.Endpoint;
        var endpointService = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, endpoint.ServiceType);

        // Check if the endpoint's return type is Task
        if (endpoint.ReturnIsTask)
        {
            // If it is, just invoke the endpoint and await the result
            await endpoint.InvokeAsync(endpointService, context.Arguments);
        }
        else
        {
            // If it's not, invoke the endpoint, await the result, and store it in the context
            var result = await endpoint.InvokeAsync(endpointService, context.Arguments);
            context.Result = result;
        }
    }
}
