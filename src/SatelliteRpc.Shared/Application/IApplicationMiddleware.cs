namespace SatelliteRpc.Shared.Application;

/// <summary>
/// Interface for application middleware.
/// </summary>
/// <typeparam name="TContext">The type of the middleware context.</typeparam>
public interface IApplicationMiddleware<TContext>
{
    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="context">The context for the middleware.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask InvokeAsync(ApplicationDelegate<TContext> next, TContext context);
}
