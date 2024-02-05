using Microsoft.Extensions.Hosting;
using SatelliteRpc.Server.Configuration;

namespace SatelliteRpc.Server.Extensions;

/// <summary>
///  The IHostBuilder extensions
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    ///  Use satellite rpc server
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IHostBuilder UseSatelliteRpcServer(
        this IHostBuilder builder,
        Action<IRpcServerBuilder>? configure = null)
    {
        // we use kestrel server, so we need to use web host
        builder.ConfigureServices(services =>
        {
            var serverBuilder = new RpcServerBuilder(services);
            configure?.Invoke(serverBuilder);
            serverBuilder.AddRpcService();
            serverBuilder.AddRpcConnectionHandler();
            serverBuilder.AddRpcServiceMiddleware();
            serverBuilder.ConfigureSatelliteRpcServer(_ => { });
        }).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseRpcKestrelServer(); });
        
        
        return builder;
    }
}