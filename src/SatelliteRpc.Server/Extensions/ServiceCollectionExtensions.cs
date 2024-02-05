using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteRpc.Protocol.PayloadConverters;
using SatelliteRpc.Server.RpcService;
using SatelliteRpc.Server.RpcService.DataExchange;
using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Server.RpcService.Middleware;
using SatelliteRpc.Server.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Extensions;

/// <summary>
/// Contains extension methods for IServiceCollection to add RPC-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton instance of the <see cref="IRpcConnectionApplicationHandlerBuilder"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configure">An optional configuration action to apply to the <see cref="ApplicationBuilder{RpcRawContext}"/>.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRpcConnectionHandler(
        this IServiceCollection services,
        Action<ApplicationBuilder<RpcRawContext>>? configure = null)
    {
        services.TryAddSingleton<IRpcConnectionApplicationHandlerBuilder>(
            provider => new RpcConnectionApplicationHandlerBuilder(provider, configure));
        return services;
    }
    
    /// <summary>
    /// Adds a singleton instance of the <see cref="IRpcServiceMiddlewareBuilder"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configure">An optional configuration action to apply to the <see cref="ApplicationBuilder{ServiceContext}"/>.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRpcServiceMiddleware(
        this IServiceCollection services,
        Action<ApplicationBuilder<ServiceContext>>? configure = null)
    {
        services.TryAddSingleton<IRpcServiceMiddlewareBuilder>(
            provider => new RpcServiceMiddlewareBuilder(provider, configure));
        return services;
    }

    /// <summary>
    /// Adds various RPC services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRpcService(this IServiceCollection services)
    {
        services.TryAddSingleton<IRpcDataExchange, DefaultRpcDataExchange>();
        services.TryAddSingleton<PayloadConverterSource>();
        services.TryAddSingleton<IPayloadConverter, ProtocolBufferPayloadConverter>();
        services.TryAddSingleton<IEndpointResolver, DefaultRpcEndPointResolver>();
        services.TryAddSingleton<RpcServiceEndpointDataSource>(provider =>
        {
            // Search the entry assembly, find RPC services that implement IRpcService to build RpcServiceEndpointDataSource
            var rpcServiceType = typeof(IRpcService);
            var rpcServiceTypes = Assembly
                .GetEntryAssembly()!
                .GetTypes()
                .Where(type => rpcServiceType.IsAssignableFrom(type) && !type.IsAbstract);

            var dataSource = ActivatorUtilities.CreateInstance<RpcServiceEndpointDataSource>(provider);
            foreach (var serviceType in rpcServiceTypes)
            {
                // Get all public methods of the RPC service
                var methods =
                    serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    // Add endpoint to the data source
                    dataSource.AddEndpoint(RpcServiceEndpoint.FromMethodInfo(serviceType, method));
                }
            }
            
            return dataSource;
        });

        return services;
    }
}
