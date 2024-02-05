using Microsoft.Extensions.DependencyInjection;

namespace SatelliteRpc.Shared.Application;

/// <summary>
/// Represents a builder for creating an application with a specific context.
/// </summary>
public class ApplicationBuilder<TContext>
{
    private readonly ApplicationDelegate<TContext> _fallbackHandler;
    private readonly List<Func<ApplicationDelegate<TContext>, ApplicationDelegate<TContext>>> _middlewares = new();

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    public IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationBuilder{TContext}"/> class with the specified service provider.
    /// </summary>
    /// <param name="appServices">The service provider for the application.</param>
    public ApplicationBuilder(IServiceProvider appServices)
        : this(appServices, _ => ValueTask.CompletedTask)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationBuilder{TContext}"/> class with the specified service provider and fallback handler.
    /// </summary>
    /// <param name="appServices">The service provider for the application.</param>
    /// <param name="fallbackHandler">The fallback handler for the application.</param>
    public ApplicationBuilder(IServiceProvider appServices, ApplicationDelegate<TContext> fallbackHandler)
    {
        ApplicationServices = appServices;
        _fallbackHandler = fallbackHandler;
    }

    /// <summary>
    /// Builds the delegate that will handle the application's requests.
    /// </summary>
    /// <returns>The delegate to handle the application's requests.</returns>
    public ApplicationDelegate<TContext> Build()
    {
        var handler = _fallbackHandler;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            handler = _middlewares[i](handler);
        }
        return handler;
    }

    /// <summary>
    /// Creates a new <see cref="ApplicationBuilder{TContext}"/> with the default configuration.
    /// </summary>
    /// <returns>A new <see cref="ApplicationBuilder{TContext}"/> with the default configuration.</returns>
    public ApplicationBuilder<TContext> New()
    {
        return new ApplicationBuilder<TContext>(this.ApplicationServices, this._fallbackHandler);
    }         

    /// <summary>
    /// Adds a conditional middleware to the application.
    /// </summary>
    /// <param name="predicate">The condition under which the middleware will be used.</param>
    /// <param name="handler">The delegate to handle the middleware.</param>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> When(Func<TContext, bool> predicate, ApplicationDelegate<TContext> handler)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                await handler(context);
            }
            else
            {
                await next(context);
            }
        });
    }

    /// <summary>
    /// Adds a conditional middleware to the application.
    /// </summary>
    /// <param name="predicate">The condition under which the middleware will be used.</param>
    /// <param name="configureAction">The action to configure the middleware.</param>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> When(Func<TContext, bool> predicate, Action<ApplicationBuilder<TContext>> configureAction)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                var branchBuilder = this.New();
                configureAction(branchBuilder);
                await branchBuilder.Build().Invoke(context);
            }
            else
            {
                await next(context);
            }
        });
    }

    /// <summary>
    /// Adds a middleware of the specified type to the application.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to add.</typeparam>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> Use<TMiddleware>()
        where TMiddleware : IApplicationMiddleware<TContext>
    {
        var middleware = ActivatorUtilities.GetServiceOrCreateInstance<TMiddleware>(this.ApplicationServices);
        return Use(middleware);
    }

    /// <summary>
    /// Adds the specified middleware to the application.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to add.</typeparam>
    /// <param name="middleware">The middleware to add.</param>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> Use<TMiddleware>(TMiddleware middleware)
        where TMiddleware : IApplicationMiddleware<TContext>
    {
        return Use(middleware.InvokeAsync);
    }

    /// <summary>
    /// Adds the specified middleware to the application.
    /// </summary>
    /// <param name="middleware">The middleware to add.</param>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> Use(Func<ApplicationDelegate<TContext>, TContext, ValueTask> middleware)
    {
        return Use(next => context => middleware(next, context));
    }

    /// <summary>
    /// Adds the specified middleware to the application.
    /// </summary>
    /// <param name="middleware">The middleware to add.</param>
    /// <returns>The <see cref="ApplicationBuilder{TContext}"/> so that additional calls can be chained.</returns>
    public ApplicationBuilder<TContext> Use(Func<ApplicationDelegate<TContext>, ApplicationDelegate<TContext>> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }
}
