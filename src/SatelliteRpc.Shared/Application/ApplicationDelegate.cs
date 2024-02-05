namespace SatelliteRpc.Shared.Application;

/// <summary>
/// Represents a delegate that can handle application requests.
/// </summary>
/// <typeparam name="TContext">The type of the middleware context.</typeparam>
/// <param name="context">The middleware context.</param>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate ValueTask ApplicationDelegate<in TContext>(TContext context);
