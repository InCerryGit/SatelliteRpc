using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteRpc.Client.Configuration;
using SatelliteRpc.Client.Transport;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Extensions;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Use Rpc Client
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <param name="configureMiddleware"></param>
    /// <returns></returns>
    public static IHostBuilder UseRpcClient(
        this IHostBuilder builder,
        Action<SatelliteRpcClientOptions>? configure = null,
        Action<ApplicationBuilder<CallContext>>? configureMiddleware = null)
    {
        builder.ConfigureServices(services => { services.Configure(configure ?? (_ => { })); });
        return builder.ConfigureServices(services => { services.AddRpcClient(configureMiddleware); });
    }
}