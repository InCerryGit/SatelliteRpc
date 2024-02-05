using System.Diagnostics;
using SatelliteRpc.Server.RpcService;
using SatelliteRpc.Server.RpcService.Middleware;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Observability;

/// <summary>
/// Middleware for providing observability support to service requests.
/// </summary>
public class ObservabilityMiddleware : IRpcServiceMiddleware
{
    /// <summary>
    /// Invokes the middleware with the specified context.
    /// </summary>
    /// <param name="next">The delegate representing the remaining middleware in the request pipeline.</param>
    /// <param name="context">The context of the current service request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask InvokeAsync(ApplicationDelegate<ServiceContext> next, ServiceContext context)
    {
        // Create a new Activity for the RPC request, adding relevant information as tags.
        var activity = new Activity("RPC request")
            .AddTag("requestId", context.RawContext.Request.Id) 
            .AddTag("method", context.RawContext.Request.Path) 
            .AddTag("service", context.Endpoint.ServiceName);

        try
        {
            activity.Start();
            await next(context); 
        }
        catch (Exception ex)
        {
            // If an exception occurs, add it as an event to the activity
            activity.AddEvent(new ActivityEvent("Exception occurred",
                tags: new ActivityTagsCollection(new[]
                {
                    new KeyValuePair<string, object>("exception", ex)
                }!)));
            throw;
        }
        finally
        {
            activity.Stop();
        }
    }
}
