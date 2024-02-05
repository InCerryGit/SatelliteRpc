using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SatelliteRpc.Client.Middleware;
using SatelliteRpc.Client.Transport;
using SatelliteRpc.Protocol.PayloadConverters;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Rpc Client
    /// Register <see cref="ISatelliteRpcClient"/> and <see cref="IRpcConnection"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureMiddleware"></param>
    /// <returns></returns>
    public static IServiceCollection AddRpcClient(this IServiceCollection services,
        Action<ApplicationBuilder<CallContext>>? configureMiddleware)
    {
        services.TryAddTransient<ISatelliteRpcClient, DefaultSatelliteRpcClient>();
        services.TryAddTransient<IRpcConnection, RpcConnection>();
        
        services.TryAddSingleton<PayloadConverterSource>();
        services.TryAddSingleton<IPayloadConverter, ProtocolBufferPayloadConverter>();
        services.TryAddSingleton<IRpcClientMiddlewareBuilder>(provider => 
            new RpcClientMiddlewareBuilder(provider, configureMiddleware));
        return services;
    }
}